using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace STAN.Client
{
    public interface IConnection
    {
	    void Publish(string subject, byte[] data);

        string PublishAsync(string subject, byte[] data, EventHandler<StanAckHandlerArgs> handler);

	    void Publish(string subject, string reply, byte[] data);

	    string PublishAsync(string subject, string reply, byte[] data, EventHandler<StanAckHandlerArgs> handler);

	    ISubscription Subscribe(string subject, SubscriptionOptions options, EventHandler<StanMsgHandlerArgs> handler);

        ISubscription QueueSubscribe(string subject, string qgroup, SubscriptionOptions options, EventHandler<StanMsgHandlerArgs> handler);

        void Close();
    }
}
