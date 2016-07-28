/*******************************************************************************
 * Copyright (c) 2015-2016 Apcera Inc. All rights reserved. This program and the accompanying
 * materials are made available under the terms of the MIT License (MIT) which accompanies this
 * distribution, and is available at http://opensource.org/licenses/MIT
 *******************************************************************************/
using System;

namespace STAN.Client
{
    /// <summary>
    /// Subscription options 
    /// </summary>
    public class SubscriptionOptions
    {
        internal string durableName = null;
        internal int maxInFlight = Consts.DefaultMaxInflight;
        internal int ackWait = 30000;
        internal StartPosition startAt = StartPosition.NewOnly;
        internal ulong startSequence = 0;
        internal DateTime startTime;
        internal bool manualAcks = false;

        internal SubscriptionOptions() { }

        internal SubscriptionOptions(SubscriptionOptions opts)
        {
            if (opts == null)
                return;

            this.ackWait = opts.ackWait;

            if (opts.durableName != null)
            {
                this.durableName = new String(opts.durableName.ToCharArray());
            }

            manualAcks = opts.manualAcks;
            maxInFlight = opts.maxInFlight;
            startAt = opts.startAt;
            startSequence = opts.startSequence;

            if (opts.startTime != null)
            {
                this.startTime = opts.startTime;
            }
        }

        /// <summary>
        /// DurableName, if set will survive client restarts.
        /// </summary>
        public string DurableName
        {
            get { return durableName; }
            set { durableName = value; }
        }

        /// <summary>
        /// Controls the number of messages the cluster will have inflight without an ACK.
        /// </summary>
	    public int MaxInflight
        {
            get { return maxInFlight; }
            set
            {
                if (maxInFlight < 0)
                {
                    throw new ArgumentException("value must be > 0");
                }
                maxInFlight = value;
            }
        }

        /// <summary>
        /// Controls the time the cluster will wait for an ACK for a given message in milliseconds.
        /// </summary>
        /// <remarks>
        /// The value must be at least one second.
        /// </remarks>
	    public int AckWait
        {
            get { return ackWait; }
            set
            {
                if (value < 1000)
                    throw new ArgumentException("value cannot be less than 1000");
                        
                ackWait = value;
            }
        }

        /// <summary>
        /// Controls the time the cluster will wait for an ACK for a given message.
        /// </summary>
	    public bool ManualAcks
        {
            get { return manualAcks; }
            set { manualAcks = value; }
        }

        /// <summary>
        /// Optional start sequence number.
        /// </summary>
        /// <param name="sequence"></param>
	    public void StartAtSequence(ulong sequence)
        {
            startAt = StartPosition.SequenceStart;
            startSequence = sequence;    
        }

        /// <summary>
        /// Optional start time.
        /// </summary>
        /// <param name="time"></param>
	    public void StartAtTime(DateTime time)
        {
            startTime = time;
            startAt = StartPosition.TimeDeltaStart;
        }

        /// <summary>
        /// Start with the last received message.
        /// </summary>
        public void StartWithLastReceived()
        {
		    startAt = StartPosition.LastReceived;
        }

        /// <summary>
        /// Deliver all messages available.
        /// </summary>
        public void DeliverAllAvailable()
        {
            startAt = StartPosition.First;
        }
    }
}
