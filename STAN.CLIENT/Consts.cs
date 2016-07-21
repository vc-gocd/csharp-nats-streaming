/*******************************************************************************
 * Copyright (c) 2015-2016 Apcera Inc. All rights reserved. This program and the accompanying
 * materials are made available under the terms of the MIT License (MIT) which accompanies this
 * distribution, and is available at http://opensource.org/licenses/MIT
 *******************************************************************************/

namespace STAN.Client
{
    class Consts
    {
	    static public readonly string Version = "0.0.1";
        static public readonly string DefaultNatsURL = "nats://localhost:4222";
        static public readonly int    DefaultConnectWait = 2000;
        static public readonly string DefaultDiscoverPrefix = "_STAN.discover";
        static public readonly string DefaultACKPrefix = "_STAN.acks";
        static public readonly long   DefaultMaxPubAcksInflight = 16384;

        static public readonly long DefaultAckWait = 3000;

        // TODO:  Differentiate the max in flight constants.
        static public readonly int  DefaultMaxInflight = 1024;
    }

    public class ConnectionOptions
    {
        string natsURL = Consts.DefaultNatsURL;
        NATS.Client.IConnection natsConn = null;
        long connectTimeout = Consts.DefaultConnectWait;
        long ackTimeout = 5000;  // TODO:  ADD default
        string discoverPrefix = Consts.DefaultDiscoverPrefix;
        long maxPubsInFlight = Consts.DefaultMaxPubAcksInflight;

	    string NatsURL
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
            
	    NATS.Client.IConnection NatsConn
        {
            get
            {
                return natsConn;
            }
        }

        long ConnectTimeout
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

        long AckTimeout
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

        string DiscoverPrefix
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

        long MaxPubAcksInFlight
        {
            get
            {
                return maxPubsInFlight;
            }
            set
            {
                maxPubsInFlight = value;
            }
        }
    }
}
