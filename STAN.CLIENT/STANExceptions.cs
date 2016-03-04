using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace STAN.Client
{
    public class STANException : Exception
    {
        internal STANException() : base() { }
        internal STANException(string err) : base (err) {}
        internal STANException(string err, Exception innerEx) : base(err, innerEx) { }
    }

    public class STANConnectRequestException : STANException
    {
        internal STANConnectRequestException() : base("Connection request timeout.") { }
        internal STANConnectRequestException(string msg) : base(msg) { }
        internal STANConnectRequestException(Exception e) : base("Connection request timeout.", e) { }
    }

    public class STANCloseRequestException : STANException
    {
        internal STANCloseRequestException() : base("Close request timeout.") { }
        internal STANCloseRequestException(string msg) : base(msg) { }
        internal STANCloseRequestException(Exception e) : base("Close request timeout.", e) { }
    }

    public class STANConnectionClosedException : STANException
    {
        internal STANConnectionClosedException() : base("Connection closed.") { }
        internal STANConnectionClosedException(Exception e) : base("Connection closed.", e) { }
    }

    public class STANPublishAckTimeoutException : STANException
    {
        internal STANPublishAckTimeoutException() : base("Publish acknowledgement timeout.") { }
        internal STANPublishAckTimeoutException(Exception e) : base("Publish acknowledgement timeout.", e) { }
    }

    public class STANBadSubscriptionException : STANException
    {
        internal STANBadSubscriptionException() : base("Invalid subscription.") { }
        internal STANBadSubscriptionException(Exception e) : base("Invalid subscription.", e) { }
    }

    public class STANTimeoutException : STANException
    {
        internal STANTimeoutException() : base("A timeout occurred.") { }
        internal STANTimeoutException(Exception e) : base("A timeout occurred.", e) { }
    }

    public class STANBadConnectionException : STANException
    {
        internal STANBadConnectionException() : base("Invalid connection.") { }
        internal STANBadConnectionException(Exception e) : base("Invalid connection.", e) { }
    }

    public class STANManualAckException : STANException
    {
        internal STANManualAckException() : base("Cannot manually ack in auto-ack mode.") { }
        internal STANManualAckException(Exception e) : base("Cannot manually ack in auto-ack mode.", e) { }
    }

    public class STANNullMessageException : STANException
    {
        internal STANNullMessageException() : base("Null Message.") { }
        internal STANNullMessageException(Exception e) : base("Null Message.", e) { }
    }


}
