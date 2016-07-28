/*******************************************************************************
 * Copyright (c) 2015-2016 Apcera Inc. All rights reserved. This program and the accompanying
 * materials are made available under the terms of the MIT License (MIT) which accompanies this
 * distribution, and is available at http://opensource.org/licenses/MIT
 *******************************************************************************/
using System;

namespace STAN.Client
{
    /// <summary>
    /// A general exception thrown by the NATS streaming server client.
    /// </summary>
    public class StanException : Exception
    {
        internal StanException() : base() { }
        internal StanException(string err) : base (err) {}
        internal StanException(string err, Exception innerEx) : base(err, innerEx) { }
    }

    /// <summary>
    /// This exception is thrown when 
    /// </summary>
    public class StanConnectRequestException : StanException
    {
        internal StanConnectRequestException() : base("Connection request timeout.") { }
        internal StanConnectRequestException(string msg) : base(msg) { }
        internal StanConnectRequestException(Exception e) : base("Connection request timeout.", e) { }
    }

    public class StanCloseRequestException : StanException
    {
        internal StanCloseRequestException() : base("Close request timeout.") { }
        internal StanCloseRequestException(string msg) : base(msg) { }
        internal StanCloseRequestException(Exception e) : base("Close request timeout.", e) { }
    }

    public class StanConnectionClosedException : StanException
    {
        internal StanConnectionClosedException() : base("Connection closed.") { }
        internal StanConnectionClosedException(Exception e) : base("Connection closed.", e) { }
    }

    public class StanPublishAckTimeoutException : StanException
    {
        internal StanPublishAckTimeoutException() : base("Publish acknowledgement timeout.") { }
        internal StanPublishAckTimeoutException(Exception e) : base("Publish acknowledgement timeout.", e) { }
    }

    public class StanBadSubscriptionException : StanException
    {
        internal StanBadSubscriptionException() : base("Invalid subscription.") { }
        internal StanBadSubscriptionException(Exception e) : base("Invalid subscription.", e) { }
    }

    public class StanTimeoutException : StanException
    {
        internal StanTimeoutException() : base("Operation timed out.") { }
        internal StanTimeoutException(Exception e) : base("Operation timed out.", e) { }
        internal StanTimeoutException(string msg) : base(msg) { }
    }

    public class StanConnectRequestTimeoutException : StanTimeoutException
    {
        internal StanConnectRequestTimeoutException() : base("Connection Request Timed out.") { }
    }

    public class StanConnectionException : StanException
    {
        internal StanConnectionException() : base("Invalid connection.") { }
        internal StanConnectionException(Exception e) : base("Invalid connection.", e) { }
    }

    public class StanManualAckException : StanException
    {
        internal StanManualAckException() : base("Cannot manually ack in auto-ack mode.") { }
        internal StanManualAckException(Exception e) : base("Cannot manually ack in auto-ack mode.", e) { }
    }

    public class STANNullMessageException : StanException
    {
        internal STANNullMessageException() : base("Null Message.") { }
        internal STANNullMessageException(Exception e) : base("Null Message.", e) { }
    }


}
