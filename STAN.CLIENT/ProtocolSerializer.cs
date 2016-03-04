using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Google.Protobuf;
using System.IO;
using System.Diagnostics;

namespace STAN.Client
{
    /// <summary>
    /// Keep all of the protocol serialization encapulated here.
    /// </summary>
    internal class ProtocolSerializer
    {
        /// <summary>
        /// Makes a copy of bytes representing the serialized protocol buffer.
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        internal byte[] marshal(Object req)
        {
            return ((IMessage)req).ToByteArray();
        }

        internal byte[] createPubMsg(string clientID, string guidValue, string subject, string reply, byte[] data)
        {
            PubMsg pm = new PubMsg();

            pm.ClientID = clientID;
            pm.Guid = guidValue;
            pm.Subject = subject;
            pm.Reply = reply;
            pm.Data = Google.Protobuf.ByteString.CopyFrom(data);

            return pm.ToByteArray();
        }

        private static long ticksToNanos(long ticks)
        {
            return (long)(1000000000.0 * (double)ticks / Stopwatch.Frequency);
        }

        internal byte[] createSubRequest(string clientID, string subject,
            string qgroup, string inbox, SubscriptionOptions options)
        {

            SubscriptionRequest sr = new SubscriptionRequest();
            sr.ClientID = clientID;
            sr.Subject = subject;
            sr.QGroup = (qgroup == null ? "" : qgroup);
            sr.Inbox = inbox;
            sr.MaxInFlight = options.MaxInflight;
            sr.AckWaitInSecs =  options.AckWait/1000;
            sr.StartPosition = options.startAt;
            sr.DurableName = options.DurableName;

	        // Conditionals
            switch (sr.StartPosition)
            {
                case StartPosition.TimeDeltaStart:
                    sr.StartTimeDelta = ticksToNanos(options.startTime.Ticks);
                    break;
                case StartPosition.SequenceStart:
                    sr.StartSequence = options.startSequence;
                    break;
            }

            return sr.ToByteArray();
        }

        internal void unmarshal(byte[] bytes, Object obj)
        {
            IMessage protoMsg = (IMessage)obj;
            protoMsg.MergeFrom(bytes);
        }

        internal byte[] createAck(MsgProto mp)
        {
            Ack a = new Ack();
            a.Subject = mp.Subject;
            a.Sequence = mp.Sequence;
            return a.ToByteArray();
        }
    }
}
