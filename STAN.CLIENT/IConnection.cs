/*******************************************************************************
 * Copyright (c) 2015-2016 Apcera Inc. All rights reserved. This program and the accompanying
 * materials are made available under the terms of the MIT License (MIT) which accompanies this
 * distribution, and is available at http://opensource.org/licenses/MIT
 *******************************************************************************/
using System;

namespace STAN.Client
{
    public interface IStanConnection : IDisposable
    {
	    void Publish(string subject, byte[] data);

	    void Publish(string subject, string reply, byte[] data);

        string Publish(string subject, byte[] data, EventHandler<StanAckHandlerArgs> handler);

	    string Publish(string subject, string reply, byte[] data, EventHandler<StanAckHandlerArgs> handler);

	    IStanSubscription Subscribe(string subject, EventHandler<StanMsgHandlerArgs> handler);

        IStanSubscription Subscribe(string subject, SubscriptionOptions options, EventHandler<StanMsgHandlerArgs> handler);

        IStanSubscription Subscribe(string subject, string qgroup, EventHandler<StanMsgHandlerArgs> handler);

        IStanSubscription Subscribe(string subject, string qgroup, SubscriptionOptions options, EventHandler<StanMsgHandlerArgs> handler);

        void Close();

        // NatsConn returns the underlying NATS conn. Use this with care. For
        // example, closing the wrapped NATS conn will put the NATS Streaming Conn
        // in an invalid state.
        NATS.Client.IConnection NATSConnection { get; }
    }
}
