/*******************************************************************************
 * Copyright (c) 2015-2016 Apcera Inc. All rights reserved. This program and the accompanying
 * materials are made available under the terms of the MIT License (MIT) which accompanies this
 * distribution, and is available at http://opensource.org/licenses/MIT
 *******************************************************************************/
 using System;

namespace STAN.Client
{
    public class Options
    {
        internal string natsURL = Consts.DefaultNatsURL;
        internal NATS.Client.IConnection natsConn = null;
        internal int connectTimeout = Consts.DefaultConnectWait;
        internal long ackTimeout = Consts.DefaultConnectWait;
        internal string discoverPrefix = Consts.DefaultDiscoverPrefix;
        internal long maxPubAcksInflight = Consts.DefaultMaxPubAcksInflight;

        internal Options() { }

        private string deepCopy(string value)
        {
            if (value == null)
                return null;

            return string.Copy(value);
        }

        internal Options(Options options)
        {
            this.ackTimeout = options.ackTimeout;
            this.NatsURL = deepCopy(options.NatsURL);
            this.ConnectTimeout = options.ConnectTimeout;
            this.AckTimeout = options.AckTimeout;
            this.DiscoverPrefix = deepCopy(options.DiscoverPrefix);
            this.MaxPubAcksInFlight = options.MaxPubAcksInFlight;
            this.NatsConn = options.NatsConn;
        }

	    public string NatsURL
        {
            get 
            {
                return natsURL;
            }
            set
            {
                if (natsURL == null)
                    natsURL = Consts.DefaultNatsURL;

                natsURL = value;
            }
        }

        public NATS.Client.IConnection NatsConn
        {
            get
            {
                return natsConn;
            }
            set
            {
                // allow null values for testing?
                natsConn = value;
            }
        }

        public int ConnectTimeout
        {
            get
            {
                return connectTimeout;
            }
            set
            {
                if (value <= 0)
                    throw new ArgumentOutOfRangeException("value", "Value must be greater than zero.");

                connectTimeout = value;
            }
        }

        public long AckTimeout
        {
            get
            {
                return ackTimeout;
            }
            set
            {
                if (value <= 0)
                    throw new ArgumentOutOfRangeException("value", "Value must be greater than zero.");

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
                if (value == null)
                    throw new ArgumentNullException("value", "Value cannot be null.");

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
                if (value <= 0)
                    throw new ArgumentOutOfRangeException("value", "Value must be greater than zero.");

                maxPubAcksInflight = value;
            }
        }
    }
}
