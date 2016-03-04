using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace STAN.Client
{
    public class Options
    {
        // TODO:  Error checking

        internal string natsURL = Consts.DefaultNatsURL;
        internal NATS.Client.Connection natsConn = null;
        internal int connectTimeout = Consts.DefaultConnectWait;
        internal long ackTimeout = Consts.DefaultConnectWait;
        internal string discoverPrefix = Consts.DefaultDiscoverPrefix;
        internal long maxPubAcksInflight = Consts.DefaultMaxPubAcksInflight;

        internal Options() { }

        internal Options(Options options)
        {
            // TODO: Complete member initialization
            this.ackTimeout = options.ackTimeout;

            if (options.natsURL != null)
            {
                this.natsURL = new String(options.NatsURL.ToCharArray());
            }

            this.connectTimeout = options.connectTimeout;
            this.ConnectWait = options.ConnectWait;

            if (options.discoverPrefix != null)
            {
                this.discoverPrefix = new String(options.discoverPrefix.ToCharArray());
            }

            this.maxPubAcksInflight = options.maxPubAcksInflight;
        }

	    public string NatsURL
        {
            get 
            {
                return natsURL;
            }
            set
            {
                natsURL = value;
            }
        }

        public NATS.Client.Connection NatsConn
        {
            get
            {
                return natsConn;
            }
        }

        public int ConnectWait
        {
            get
            {
                return connectTimeout;
            }
            set
            {
                connectTimeout = value;
            }
        }

        public long PubAckWait
        {
            get
            {
                return ackTimeout;
            }
            set
            {
                ackTimeout = value;
            }
        }

        public string DiscoverPrefix
        {
            get
            {
                return discoverPrefix;
            }
            set
            {
                discoverPrefix = value;
            }
        }

        public long MaxPubAcksInFlight
        {
            get
            {
                return maxPubAcksInflight;
            }
            set
            {
                maxPubAcksInflight = value;
            }
        }
    }
}
