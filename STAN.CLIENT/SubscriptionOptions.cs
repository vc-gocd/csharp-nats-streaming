using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
        internal DateTime startTime = DateTime.Now;
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

            this.manualAcks = opts.manualAcks;
            this.maxInFlight = opts.maxInFlight;
            this.startAt = opts.startAt;
            this.startSequence = opts.startSequence;

            if (opts.startTime != null)
            {
                this.startTime = new DateTime(opts.startTime.Ticks);
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
        /// Controls the time the cluster will wait for an ACK for a given message.
        /// </summary>
	    public int AckWait
        {
            get { return ackWait; }
            set { ackWait = value; }
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
        /// Optional Time span to start from.
        /// </summary>
        public void StartFrom(TimeSpan duration)
        {
            startTime = DateTime.Now.Subtract(duration);
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
