/*******************************************************************************
 * Copyright (c) 2015-2016 Apcera Inc. All rights reserved. This program and the accompanying
 * materials are made available under the terms of the MIT License (MIT) which accompanies this
 * distribution, and is available at http://opensource.org/licenses/MIT
 *******************************************************************************/
using System;

namespace STAN.Client
{
    /// <summary>
    /// Subscription represents a subscription within the NATS Streaming cluster. Subscriptions will be rate matched and follow at-least delivery semantics.
    /// </summary>
    public interface IStanSubscription : IDisposable
    {
        void Unsubscribe();
    }
}
