using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace STAN.Client
{
    internal class PublishAck
    {
        string guidValue;

        internal System.Threading.Timer t;
        Object cond = new Object();
        bool isComplete = false;
        private Connection connection;
        private EventHandler<StanAckHandlerArgs> ah;

        internal PublishAck(Connection connection, string guidValue, EventHandler<StanAckHandlerArgs> handler)
        {
            this.connection = connection;
            this.guidValue = guidValue;
            this.ah = handler;
            this.guidValue = guidValue;
            this.t = new Timer(timerCb);
        }

        internal string GUID
        {
            get { return guidValue;  }
        }

        private void timerCb(object state)
        {
            System.Console.WriteLine("Ack timed out: " + this.guidValue);

            connection.removeAck(this.guidValue);
            invokeHandler(guidValue, "Timeout occurred.");
        }

        internal void wait(int timeout)
        {
            lock (cond)
            {
                if (!isComplete)
                {
                    Monitor.Wait(cond, timeout);
                }
            }
        }

        internal void wait()
        {
            wait(int.MaxValue);
        }

        internal void complete()
        {
            lock (cond)
            {
                if (!isComplete)
                {
                    isComplete = true;
                    Monitor.Pulse(cond);
                }
            }
        }

        internal void invokeHandler(String guidValue, string error)
        {
            if (ah != null)
            {
                ah(this, new StanAckHandlerArgs(guidValue, error));
            }

            System.Console.WriteLine("Ack completed {0} {1}", 
                guidValue, error == null ? "null" : error);

            complete();
        }
    }

    public class Connection : IConnection, IDisposable
    {
	    Object mu = new Object();
	    string clientID;
	    string serverID;
	    string pubPrefix; // Publish prefix set by stan, append our subject.
	    string subRequests; // Subject to send subscription requests.
	    string unsubRequests; // Subject to send unsubscribe requests.
	    string closeRequests; // Subject to send close requests.
	    string ackSubject; // publish acks
        string hbInbox; // heartbeat inbox
	    NATS.Client.ISubscription ackSubscription;
        NATS.Client.ISubscription hbSubscription;

        IDictionary<string, AsyncSubscription> subMap = new Dictionary<string, AsyncSubscription>();
        BlockingDictionary<string, PublishAck> pubAckMap;

        ProtocolSerializer ps = new ProtocolSerializer();
        
        Options opts = null;

	    NATS.Client.IConnection  nc;

        private Connection() { }
        
        internal Connection(string stanClusterID, string clientID, Options options)
        {
            this.clientID = clientID;
            if (options != null)
            {
                this.opts = new Options(options);
            }
            else
            {
                this.opts = new Options();
            }

            if (opts.NatsConn == null)
            {
                nc = new NATS.Client.ConnectionFactory().CreateConnection(opts.NatsURL);
            }
            else
            {
                nc = opts.NatsConn;
            }

            pubAckMap = new BlockingDictionary<string, PublishAck>(opts.maxPubAcksInflight);

            // create a heartbeat inbox
            this.hbInbox = newInbox();
            this.hbSubscription = nc.SubscribeAsync(hbInbox, processHeartBeat);

            string discoverSubject = opts.discoverPrefix + "." + stanClusterID;

            ConnectRequest req = new ConnectRequest();
            req.ClientID = this.clientID;
            req.HeartbeatInbox = this.hbInbox;

            NATS.Client.Msg cr = nc.Request(discoverSubject, ps.marshal(req), opts.ConnectWait);

            ConnectResponse response = new ConnectResponse();
            try
            {
                ps.unmarshal(cr.Data, response);
            }
            catch (Exception e)
            {
                throw new STANConnectRequestException(e);
            }
            
            if (!string.IsNullOrEmpty(response.Error))
            {
                throw new STANConnectRequestException(response.Error);
            }

            // capture clister configuration endpoints to publish and subscribe/unsubscribe
            this.pubPrefix = response.PubPrefix;
            this.subRequests = response.SubRequests;
            this.unsubRequests = response.UnsubRequests;
            this.closeRequests = response.CloseRequests;

            // setup the Ack subscription
            this.ackSubject = Consts.DefaultACKPrefix + "." + newGUID();
            this.ackSubscription = nc.SubscribeAsync(ackSubject, processAck);

            // TODO:  hardcode or options?
            this.ackSubscription.SetPendingLimits(1024 * 1024, 32 * 1024 * 1024);

            // GO sets a finalizer, we have a destructor.
            System.Console.WriteLine("DEBUG:  " + response);
        }

        private void processHeartBeat(object sender, NATS.Client.MsgHandlerEventArgs args)
        {
            // no payload assumed, just reply.
            nc.Publish(args.Message.Reply, null);
        }

        internal PublishAck removeAck(string guid)
        {
            PublishAck a;

            lock (mu)
            {
                pubAckMap.TryGetValue(guid, out a);
                if (a == null)
                    return null;
            }

            //a.complete();

            return a;
        }

        internal NATS.Client.IConnection NatsConn
        {
            get
            {
                lock (mu)
                {
                    return this.nc;
                }
            }
        }

        private void processAck(object sender, NATS.Client.MsgHandlerEventArgs args)
        {
            PubAck pa = new PubAck();
            try
            {
                ps.unmarshal(args.Message.Data, pa);
            }
            catch (Exception e)
            {
                // TOOD:  YIKES - handle before alpha.
                System.Console.WriteLine(e);
            }

            PublishAck a = removeAck(pa.Guid);

            a.invokeHandler(pa.Guid, pa.Error);
        }

        internal void processMsg(object sender, NATS.Client.MsgHandlerEventArgs args)
        {
            bool isClosed = false;
            AsyncSubscription sub = null;
            NATS.Client.Msg raw = null;

            MsgProto mp = new MsgProto();
            ps.unmarshal(args.Message.Data, mp);

            raw = args.Message;

            lock (mu)
            {
                isClosed = (nc == null);
                subMap.TryGetValue(raw.Subject, out sub);
            }

            if (isClosed || sub == null)
                return;

            Msg msg = new Msg(mp, sub);

            sub.processMsg(mp);
        }

        ~Connection()
        {
            try
            {
                Close();
            }
            catch (Exception) { }
        }

        static public string newGUID()
        {
            return NATS.Client.NUID.NextGlobal;
        }

        public void Publish(string subject, byte[] data)
        {
            Publish(subject, null, data);
        }

        public string PublishAsync(string subject, byte[] data, EventHandler<StanAckHandlerArgs> handler)
        {
            return publishAsync(subject, null, data, handler).GUID;
        }

        public void Publish(string subject, string reply, byte[] data)
        {
            PublishAck a = publishAsync(subject, reply, data, null);
            a.wait();
        }

        internal PublishAck publishAsync(string subject, string reply, byte[] data, EventHandler<StanAckHandlerArgs> handler)
        {
            string localAckSubject = null;
            long localAckTimeout = 0;

            string subj = this.pubPrefix + "." + subject;
            string guidValue = newGUID();
            byte[] b = ps.createPubMsg(clientID, guidValue, subject,
                reply == null ? "" : reply, data);

            PublishAck a = new PublishAck(this, guidValue, handler);

            lock (mu)
            {
                pubAckMap.Add(guidValue, a);
                localAckSubject = this.ackSubject;
                localAckTimeout = this.opts.ackTimeout;
            }

            try
            {
                nc.Publish(subj, localAckSubject, b);
            }
            catch (Exception e)
            {
                removeAck(guidValue);
                throw e;
            }

            return a;
        }

        private ISubscription subscribe(string subject, string qgroup, EventHandler<StanMsgHandlerArgs> handler, SubscriptionOptions options)
        {
            AsyncSubscription sub = new AsyncSubscription(this, options);

            lock (mu)
            {
                if (nc == null)
                {
                    throw new STANConnectionClosedException();
                }

                // Register the subscription
                subMap[sub.Inbox] = sub;
                NATS.Client.IConnection localNc = nc;
            }

            try
            {
                sub.subscribe(subRequests, subject, qgroup, handler);
            }
            catch (Exception ex)
            {
                subMap.Remove(sub.Inbox);
                throw ex;
                // TODO = reorganize the go client.
            }

            return sub;
        }


        /// <summary>
        /// PublishAsyncWithReply will publish to the cluster and asynchronously
        //  process the ACK or error state. It will return the GUID for the message being sent.
        /// </summary>
        /// <param name="subject"></param>
        /// <param name="reply"></param>
        /// <param name="data"></param>
        /// <param name="handler"></param>
        /// <returns></returns>
        public string PublishAsync(string subject, string reply, byte[] data, EventHandler<StanAckHandlerArgs> handler)
        {
            return publishAsync(subject, reply, data, handler).GUID;
        }


        internal static string newInbox() 
        {
            return "_INBOX." + newGUID();
        }

        public ISubscription Subscribe(string subject, SubscriptionOptions options, EventHandler<StanMsgHandlerArgs> handler)
        {
            if (subject == null)
                throw new ArgumentException("cannot be null", "subject");
            if (options == null)
                throw new ArgumentException("cannot be null", "options");
            if (handler == null)
                throw new ArgumentException("cannot be null", "handler");

            return subscribe(subject, null, handler, options);
        }

        public ISubscription QueueSubscribe(string subject, string qgroup, SubscriptionOptions options, EventHandler<StanMsgHandlerArgs> handler)
        {
            if (subject == null)
                throw new ArgumentException("cannot be null", "subject");
            if (qgroup == null)
                throw new ArgumentException("cannot be null", "qgroup");
            if (options == null)
                throw new ArgumentException("cannot be null", "options");
            if (handler == null)
                throw new ArgumentException("cannot be null", "handler");

            return subscribe(subject, qgroup, handler, options);
        }

        public void Close()
        {
            NATS.Client.Msg reply = null;

            lock (mu)
            {
                if (nc == null)
                    throw new STANBadConnectionException();


                if (ackSubscription != null)
                {
                    ackSubscription.Unsubscribe();
                    ackSubscription = null;
                }

                if (hbSubscription != null)
                {
                    hbSubscription.Unsubscribe();
                    hbSubscription = null;
                }

                NATS.Client.IConnection c = nc;
                nc = null;

                CloseRequest req = new CloseRequest();
                req.ClientID = this.clientID;

                try
                {
                    if (this.closeRequests != null)
                    {
                        reply = c.Request(this.closeRequests, ps.marshal(req));
                    }
                }
                catch (STANBadSubscriptionException)
                {
                    // it's possible we never actually connected.
                    return;
                }

                if (reply != null)
                {
                    CloseResponse resp = new CloseResponse();
                    try
                    {
                        ps.unmarshal(reply.Data, resp);
                    }
                    catch (Exception e)
                    {
                        throw new STANCloseRequestException(e);
                    }

                    if (!string.IsNullOrEmpty(resp.Error))
                    {
                        throw new STANCloseRequestException(resp.Error);
                    }
                }
            }
        }

        public void Dispose()
        {
            try { Close(); } catch (Exception) { }
        }

        public string ClientID 
        { 
            get { return this.clientID; }
        }

        internal ProtocolSerializer ProtoSer
        {
            get { return this.ps; }
        }
    }
}
