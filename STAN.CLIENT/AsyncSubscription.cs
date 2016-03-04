using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace STAN.Client
{
    class AsyncSubscription : ISubscription
    {
        private object mu = new Object();
        internal SubscriptionOptions options;
        private string inbox = null;
        private Connection sc = null;
        internal string ackInbox = null;
        private NATS.Client.IAsyncSubscription inboxSub = null;
        private EventHandler<StanMsgHandlerArgs> handler;

        ReaderWriterLockSlim rwLock = new ReaderWriterLockSlim();

        internal AsyncSubscription(Connection sc, SubscriptionOptions opts)
        {
            // TODO: Complete member initialization
            this.options = new SubscriptionOptions(opts);
            this.inbox = Connection.newInbox();
            this.sc = sc;
        }

        internal string Inbox
        {
            get { return inbox; }
        }

        // in STAN, much of this code is in the connection module.
        internal void subscribe(string subRequestSubject, string subject, string qgroup, EventHandler<StanMsgHandlerArgs> handler)
        {
            Exception exToThrow = null;

            rwLock.EnterWriteLock();


            this.handler = handler;

            try
                {

                    if (sc == null)
                    {
                        throw new STANConnectionClosedException();
                    }

                    // Listen for actual messages.
                    this.inboxSub = sc.NatsConn.SubscribeAsync(inbox, sc.processMsg);

                    byte[] payload = sc.ProtoSer.createSubRequest(sc.ClientID,
                        subject, qgroup, inbox, options);

                    // TODO:  Configure request timeout?
                    NATS.Client.Msg m = sc.NatsConn.Request(subRequestSubject, payload, 2000);

                    SubscriptionResponse r = new SubscriptionResponse();
                    sc.ProtoSer.unmarshal(m.Data, r);

                    if (string.IsNullOrWhiteSpace(r.Error) == false)
                    {
                        throw new STANException(r.Error);
                    }

                    this.ackInbox = r.AckInbox;
                }
                catch (Exception ex)
                {
                    if (inboxSub != null)
                    {
                        inboxSub.Unsubscribe();
                    }
                    exToThrow = ex;
                }

            rwLock.ExitWriteLock();

            if (exToThrow != null)
                throw exToThrow;
        }

        public void Unsubscribe()
        {
            throw new NotImplementedException();
        }

        internal void manualAck(Msg m)
        {
            if (m == null)
                return;

            rwLock.EnterReadLock();
            
            string localAckSubject = this.ackInbox;
            bool   localManualAck = options.manualAcks;
            Connection sc = this.sc;

            rwLock.ExitReadLock();

            if (localManualAck == false)
            {
                throw new STANManualAckException();
            }

            if (sc == null)
            {
                throw new STANBadSubscriptionException();
            }

            byte[] b = sc.ProtoSer.createAck(m.proto);
            sc.NatsConn.Publish(localAckSubject, b);
        }

        internal void processMsg(MsgProto mp)
        {
            rwLock.EnterReadLock();

            EventHandler<StanMsgHandlerArgs> cb = this.handler;
            bool isManualAck  = this.options.manualAcks;
            string localAckSubject = this.ackInbox;
            STAN.Client.IConnection subsSc = this.sc;
            NATS.Client.IConnection localNc = null;

            if (subsSc != null)
            {
                localNc = sc.NatsConn;
            }

            rwLock.ExitReadLock();

            if (cb != null && subsSc != null)
            {
                StanMsgHandlerArgs args = new StanMsgHandlerArgs(new Msg(mp, this));
                cb(this, args);
            }

            if (!isManualAck && localNc != null)
            {
                byte[] b = sc.ProtoSer.createAck(mp);
                localNc.Publish(localAckSubject, b);
            }
        }
    }
}
