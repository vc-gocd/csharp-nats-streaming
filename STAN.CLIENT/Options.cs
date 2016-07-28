/*******************************************************************************
 * Copyright (c) 2015-2016 Apcera Inc. All rights reserved. This program and the accompanying
 * materials are made available under the terms of the MIT License (MIT) which accompanies this
 * distribution, and is available at http://opensource.org/licenses/MIT
 *******************************************************************************/
using System;
using NATS.Client;

namespace STAN.Client
{
    /// <summary>
    /// Options available to configure a connection to the NATS streaming server.
    /// </summary>
    public class StanOptions
    {
        internal string natsURL = StanConsts.DefaultNatsURL;
        internal IConnection natsConn = null;
        internal int connectTimeout = StanConsts.DefaultConnectWait;
        internal long ackTimeout = StanConsts.DefaultConnectWait;
        internal string discoverPrefix = StanConsts.DefaultDiscoverPrefix;
        internal long maxPubAcksInflight = StanConsts.DefaultMaxPubAcksInflight;

        internal StanOptions() { }

        private string deepCopy(string value)
        {
            if (value == null)
                return null;

            return string.Copy(value);
        }

        internal StanOptions(StanOptions options)
        {
            ackTimeout = options.ackTimeout;
            NatsURL = deepCopy(options.NatsURL);
            ConnectTimeout = options.ConnectTimeout;
            AckTimeout = options.AckTimeout;
            DiscoverPrefix = deepCopy(options.DiscoverPrefix);
            MaxPubAcksInFlight = options.MaxPubAcksInFlight;
            NatsConn = options.NatsConn;
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
                    natsURL = StanConsts.DefaultNatsURL;

                natsURL = value;
            }
        }

        public IConnection NatsConn
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

        /// <summary>
        /// Returns the default connection options.
        /// </summary>
        /// <returns></returns>
        public static StanOptions GetDefaultOptions()
        {
            return new StanOptions();
        }
    }
}
