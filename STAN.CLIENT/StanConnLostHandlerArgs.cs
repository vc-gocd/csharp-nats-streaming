﻿// Copyright 2015-2018 The NATS Authors
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
using System;

namespace STAN.Client
{
    /// <summary>
    /// This class is used with an acknowledgement event handler indicating 
    /// a message has been acknowledged by the NATS Streaming server.
    /// </summary>
    public class StanConnLostHandlerArgs : EventArgs
    {
        private IStanConnection sc;
        private Exception connEx;

        private StanConnLostHandlerArgs() { }

        internal StanConnLostHandlerArgs(IStanConnection connection, Exception exception)
        {
            sc = connection;
            connEx = exception;
        }

        /// <summary>
        /// The connection to the NATS Streaming server.
        /// </summary>
        public IStanConnection Connection => sc;

        /// <summary>
        /// The connection error if applicable, null otherwise.
        /// </summary>
        public Exception ConnectionException => connEx;
    }
}
