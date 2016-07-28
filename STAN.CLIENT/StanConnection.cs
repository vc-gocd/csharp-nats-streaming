/*******************************************************************************
 * Copyright (c) 2015-2016 Apcera Inc. All rights reserved. This program and the accompanying
 * materials are made available under the terms of the MIT License (MIT) which accompanies this
 * distribution, and is available at http://opensource.org/licenses/MIT
 *******************************************************************************/
using System;
using System.Collections.Generic;
using System.Threading;

namespace STAN.Client
{
    internal class PublishAck
    {
        string guidValue;

        internal Timer ackTimer;
        object cond = new Object();
        bool isComplete = false;
        private Connection connection;
        private EventHandler<StanAckHandlerArgs> ah;
        private Exception ex = null;

        internal PublishAck(Connection conn, string guid, EventHandler<StanAckHandlerArgs> handler, long timeout)
        {
            connection = conn;
            guidValue = guid;

            ah = handler;
            guidValue = guid;
            ackTimer = new Timer(ackTimerCb);
            ackTimer.Change(timeout, Timeout.Infinite);
        }

        internal string GUID
        {
            get { return guidValue;  }
        }

        private void ackTimerCb(object state)
        {
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
            if (ex != null)
                throw ex;
        }

        internal void complete()
        {
            lock (cond)
            {
                if (!isComplete)
                {
                    ackTimer.Dispose();
                    isComplete = true;
                    Monitor.Pulse(cond);
                }
            }
        }

        internal void invokeHandler(string guidValue, string error)
        {
            try
            {
                if (ah != null)
                {
                    ah(this, new StanAckHandlerArgs(guidValue, error));
                }
                else
                {
                    if (string.IsNullOrEmpty(error) == false)
                        ex = new StanException(error);
                }
                complete();
            }
            catch { /* ignore user exceptions */ }
        }
    }

    public class Connection : IStanConnection, IDisposable
    {
        private Object mu = new Object();

        private readonly string clientID;
        private readonly string pubPrefix; // Publish prefix set by stan, append our subject.
        private readonly string subRequests; // Subject to send subscription requests.
        private readonly string unsubRequests; // Subject to send unsubscribe requests.
        private readonly string closeRequests; // Subject to send close requests.
        private readonly string ackSubject; // publish acks

        private NATS.Client.ISubscription ackSubscription;
        private NATS.Client.ISubscription hbSubscription;

        private IDictionary<string, AsyncSubscription> subMap = new Dictionary<string, AsyncSubscription>();
        private BlockingDictionary<string, PublishAck> pubAckMap;

        internal ProtocolSerializer ps = new ProtocolSerializer();
        
        private StanOptions opts = null;

	    private NATS.Client.IConnection  nc;
        private bool ncOwned = false;

        private Connection() { }
        
        internal Connection(string stanClusterID, string clientID, StanOptions options)
        {
            this.clientID = clientID;

            if (options != null)
                this.opts = new StanOptions(options);
            else
                this.opts = new StanOptions();

            if (opts.NatsConn == null)
            {
                ncOwned = true;
                try
                {
                    nc = new NATS.Client.ConnectionFactory().CreateConnection(opts.NatsURL);
                }
                catch (Exception ex)
                {
                    throw new StanConnectionException(ex);
                }
            }
            else
            {
                nc = opts.NatsConn;
                this.ncOwned = false;
            }

            // create a heartbeat inbox
            string hbInbox = newInbox();
            hbSubscription = nc.SubscribeAsync(hbInbox, processHeartBeat);

            string discoverSubject = opts.discoverPrefix + "." + stanClusterID;

            ConnectRequest req = new ConnectRequest();
            req.ClientID = this.clientID;
            req.HeartbeatInbox = hbInbox;

            NATS.Client.Msg cr;
            try
            {
                cr = nc.Request(discoverSubject, 
                    ProtocolSerializer.marshal(req),
                    opts.ConnectTimeout);
            }
            catch (NATS.Client.NATSTimeoutException)
            {
                throw new StanConnectRequestTimeoutException();
            }

            ConnectResponse response = new ConnectResponse();
            try
            {
                ProtocolSerializer.unmarshal(cr.Data, response);
            }
            catch (Exception e)
            {
                throw new StanConnectRequestException(e);
            }
            
            if (!string.IsNullOrEmpty(response.Error))
            {
                throw new StanConnectRequestException(response.Error);
            }

            // capture clister configuration endpoints to publish and subscribe/unsubscribe
            pubPrefix = response.PubPrefix;
            subRequests = response.SubRequests;
            unsubRequests = response.UnsubRequests;
            closeRequests = response.CloseRequests;

            // setup the Ack subscription
            ackSubject = StanConsts.DefaultACKPrefix + "." + newGUID();
            ackSubscription = nc.SubscribeAsync(ackSubject, processAck);

            // TODO:  hardcode or options?
            ackSubscription.SetPendingLimits(1024 * 1024, 32 * 1024 * 1024);

            pubAckMap = new BlockingDictionary<string, PublishAck>(opts.maxPubAcksInflight);
        }

        private void processHeartBeat(object sender, NATS.Client.MsgHandlerEventArgs args)
        {
            NATS.Client.IConnection lnc;

            lock (mu)
            {
                lnc = this.nc;
            }
            lnc.Publish(args.Message.Reply, null);
        }

        internal PublishAck removeAck(string guid)
        {
            PublishAck a;

            lock (mu)
            {
                pubAckMap.TryGetValue(guid, out a);
            }

            return a;
        }

        public NATS.Client.IConnection NATSConnection
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
                ProtocolSerializer.unmarshal(args.Message.Data, pa);
            }
            catch (Exception)
            {
                // TODO:  (cls) handle this...
                return;
            }

            PublishAck a = removeAck(pa.Guid);

            if (a != null)
                a.invokeHandler(pa.Guid, pa.Error);
        }

        internal void processMsg(object sender, NATS.Client.MsgHandlerEventArgs args)
        {
            bool isClosed = false;
            AsyncSubscription sub = null;
            NATS.Client.Msg raw = null;

            MsgProto mp = new MsgProto();
            ProtocolSerializer.unmarshal(args.Message.Data, mp);

            raw = args.Message;

            lock (mu)
            {
                isClosed = (nc == null);
                subMap.TryGetValue(raw.Subject, out sub);
            }

            if (isClosed || sub == null)
                return;

            StanMsg msg = new StanMsg(mp, sub);

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

        public string Publish(string subject, byte[] data, EventHandler<StanAckHandlerArgs> handler)
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
            byte[] b = ProtocolSerializer.createPubMsg(clientID, guidValue, subject,
                reply == null ? "" : reply, data);

            PublishAck a = new PublishAck(this, guidValue, handler, opts.AckTimeout);

            lock (mu)
            {
                if (nc == null)
                    throw new StanConnectionClosedException();

                if (pubAckMap.isAtCapacity())
                {
                    var bd = pubAckMap;

                    Monitor.Exit(mu);
                    // Wait for space outside of the lock so 
                    // acks can be removed.
                    bd.waitForSpace();
                    Monitor.Enter(mu);

                    if (nc == null)
                    {
                        throw new StanConnectionClosedException();
                    }
                }

                pubAckMap.Add(guidValue, a);
                localAckSubject = ackSubject;
                localAckTimeout = opts.ackTimeout;
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

        private IStanSubscription subscribe(string subject, string qgroup, EventHandler<StanMsgHandlerArgs> handler, StanSubscriptionOptions options)
        {
            AsyncSubscription sub = new AsyncSubscription(this, options);

            lock (mu)
            {
                if (nc == null)
                {
                    throw new StanConnectionClosedException();
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
            }

            return sub;
        }

        internal void unsubscribe(string subject, string ackInbox)
        {
            NATS.Client.IConnection lnc;

            lock (mu)
            {
                lnc = this.nc;
                subMap.Remove(ackInbox);
            }

            UnsubscribeRequest usr = new UnsubscribeRequest();
            usr.ClientID = clientID;
            usr.Subject = subject;
            usr.Inbox = ackInbox;
            byte[] b = ProtocolSerializer.marshal(usr);

            var r = lnc.Request(unsubRequests, b, 2000);

            SubscriptionResponse sr = new SubscriptionResponse();
            ProtocolSerializer.unmarshal(r.Data, sr);
            if (!string.IsNullOrEmpty(sr.Error))
                throw new StanException(sr.Error);
        }

        /// <summary>
        /// Publish will publish to the cluster and asynchronously
        //  process the ACK or error state. It will return the GUID for the message being sent.
        /// </summary>
        /// <param name="subject"></param>
        /// <param name="reply"></param>
        /// <param name="data"></param>
        /// <param name="handler"></param>
        /// <returns></returns>
        public string Publish(string subject, string reply, byte[] data, EventHandler<StanAckHandlerArgs> handler)
        {
            return publishAsync(subject, reply, data, handler).GUID;
        }


        internal static string newInbox() 
        {
            return "_INBOX." + newGUID();
        }


        public IStanSubscription Subscribe(string subject, EventHandler<StanMsgHandlerArgs> handler)
        {
            return Subscribe(subject, AsyncSubscription.DefaultOptions, handler);
        }

        public IStanSubscription Subscribe(string subject, StanSubscriptionOptions options, EventHandler<StanMsgHandlerArgs> handler)
        {
            if (subject == null)
                throw new ArgumentException("cannot be null", "subject");
            if (options == null)
                throw new ArgumentException("cannot be null", "options");
            if (handler == null)
                throw new ArgumentException("cannot be null", "handler");

            return subscribe(subject, null, handler, options);
        }

        public IStanSubscription Subscribe(string subject, string qgroup,EventHandler<StanMsgHandlerArgs> handler)
        {
            return Subscribe(subject, qgroup, AsyncSubscription.DefaultOptions, handler);
        }

        public IStanSubscription Subscribe(string subject, string qgroup, StanSubscriptionOptions options, EventHandler<StanMsgHandlerArgs> handler)
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

                NATS.Client.IConnection lnc = nc;
                nc = null;

                if (lnc == null)
                    return;

                if (lnc.IsClosed())
                    return;

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

                CloseRequest req = new CloseRequest();
                req.ClientID = this.clientID;

                try
                {
                    if (this.closeRequests != null)
                    {
                        reply = lnc.Request(closeRequests, ProtocolSerializer.marshal(req));
                    }
                }
                catch (StanBadSubscriptionException)
                {
                    // it's possible we never actually connected.
                    return;
                }

                if (reply != null)
                {
                    CloseResponse resp = new CloseResponse();
                    try
                    {
                        ProtocolSerializer.unmarshal(reply.Data, resp);
                    }
                    catch (Exception e)
                    {
                        throw new StanCloseRequestException(e);
                    }

                    if (!string.IsNullOrEmpty(resp.Error))
                    {
                        throw new StanCloseRequestException(resp.Error);
                    }
                }

                if (ncOwned && lnc != null)
                {
                    lnc.Close();
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
