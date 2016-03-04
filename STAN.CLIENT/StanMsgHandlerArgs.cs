using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace STAN.Client
{
    public class StanMsgHandlerArgs : EventArgs
    {
        Msg msg = null;

        internal StanMsgHandlerArgs(Msg m)
        {
            msg = m;
        }

        public Msg Message
        {
            get
            {
                return msg;
            }
        }
    }
}
