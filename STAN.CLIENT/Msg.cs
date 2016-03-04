using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace STAN.Client
{
    public class Msg
    {
        internal MsgProto proto;
        private  AsyncSubscription sub;

        internal Msg(MsgProto p, AsyncSubscription s)
        {
            this.proto = p;
            this.sub = s;
        }

        internal long Time
        {
            get
            {
                return proto.Timestamp;
            }
        }

        public void Ack()
        {

            if (sub == null)
            {
                throw new STANBadSubscriptionException();
            }

            sub.manualAck(this);
        }

        internal ulong Sequence
        {
            get
            {
                return proto.Sequence;
            }
        }

        internal string Subject
        {
            get
            {
                return proto.Subject;
            }
        }
    }
}
