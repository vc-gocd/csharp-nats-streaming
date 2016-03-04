// Copyright 2015 Apcera Inc. All rights reserved.

using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace STAN.Client
{

    // This dictionary class is a capacity bound, threadsafe, dictionary.
    internal sealed class BlockingDictionary<TKey, TValue>
    {
        IDictionary<TKey, TValue> d = new Dictionary<TKey, TValue>();
        Object dLock   = new Object();
        Object addLock = new Object();
        bool finished = false;
        long maxSize = 1024;
        bool atCapacity = false;

        internal bool isAtCapacity()
        {
            lock (dLock)
            {
                return d.Count < maxSize;
            }
        }

        internal void waitForSpace()
        {
            lock (addLock)
            {
                lock (dLock)
                {
                    atCapacity = d.Count >= maxSize;
                }

                if (atCapacity)
                {
                    Monitor.Wait(addLock);
                }
            }
        }

        internal void notifySpaceAvailable()
        {
            lock (addLock)
            {
                if (atCapacity)
                {
                    atCapacity = false;
                    Monitor.Pulse(addLock);
                }
            }
        }

        private BlockingDictionary() { }

        internal BlockingDictionary(long maxSize)
        {
            if (maxSize <= 0)
                throw new ArgumentException("max size must be greater than 0");

            this.maxSize = maxSize;
        }

        internal bool TryGetValue(TKey key, out TValue value)
        {
            return TryGetValue(key, out value, -1);
        }

        internal bool TryGetValue(TKey key, out TValue value, int timeout)
        {
            bool rv = false;

            value = default(TValue);

            lock (dLock)
            {
                if (!finished)
                {
                    if (d.Count > 0)
                    {
                        rv = d.TryGetValue(key, out value);
                    }
                    else
                    {
                        if (timeout < 0)
                        {
                            Monitor.Wait(dLock);
                        }
                        else
                        {
                            if (Monitor.Wait(dLock, timeout) == false)
                            {
                                throw new Exception("timeout");
                            }
                        }
                    }
                }

                // we waited..
                if (!finished)
                {
                    rv = d.TryGetValue(key, out value);
                }
            }

            notifySpaceAvailable();

            return rv;

        } // get

        internal void Add(TKey key, TValue value)
        {
            waitForSpace();

            lock (dLock)
            {
                
                d[key] =  value;

                // if the queue count was previously zero, we were
                // waiting, so signal.
                if (d.Count <= 1)
                {
                    Monitor.Pulse(dLock);
                }
            }
        }

        internal void close()
        {
            lock (dLock)
            {
                finished = true;
                Monitor.Pulse(dLock);
            }
        }

        internal int Count
        {
            get
            {
                lock (dLock)
                {
                    return d.Count;
                }
            }
        }
    } // class Channel

#if sdlfkj
    // This channel class really a capacity bound blocking queue, is named the
    // way it is so the code more closely reads with GO.  We implement our own channels 
    // to be lightweight and performant - other concurrent classes do the
    // task but are more heavyweight than what we want.
    internal sealed class BlockingChannel<T>
    {
        Queue<T> q;
        Object   qLock = new Object();
        Object   writeLock = new Object(); 
        bool     finished = false;
        int      maxSize = 1024;
        bool     atCapacity = false;
        
        internal bool isAtCapacity()
        {
            lock (qLock)
            {
                return q.Count < maxSize;
            }
        }

        internal void waitForSpace()
        {
            lock (writeLock)
            {
                lock (qLock)
                {
                    atCapacity = q.Count >= maxSize;
                }

                if (atCapacity)
                {
                    Monitor.Wait(writeLock);
                }
            }
        }

        internal void notifySpaceAvailable()
        {
            lock (writeLock)
            {
                if (atCapacity)
                {
                    atCapacity = false;
                    Monitor.Pulse(writeLock);
                }
            }
        }
        
        internal BlockingChannel()
        {
            q = new Queue<T>(512);
        }

        internal BlockingChannel(int maxSize)
        {
            if (maxSize <= 0)
                throw new ArgumentException("max size must be greater than 0");

            this.maxSize = maxSize;

            q = new Queue<T>(512);
        }

        internal T get(int timeout)
        {
            T rv = default(T);

            lock (qLock)
            {
                if (!finished)
                {
                    if (q.Count > 0)
                    {
                        rv = q.Dequeue();
                    }
                    else
                    {
                        if (timeout < 0)
                        {
                            Monitor.Wait(qLock);
                        }
                        else
                        {
                            if (Monitor.Wait(qLock, timeout) == false)
                            {
                                throw new Exception("timeout");
                            }
                        }
                    }
                }
                   
                // we waited..
                if (!finished)
                {
                    return q.Dequeue();
                }
            }

            notifySpaceAvailable();

            return rv;

        } // get
        
        internal void add(T item)
        {
            waitForSpace();

            lock (qLock)
            {
                q.Enqueue(item);

                // if the queue count was previously zero, we were
                // waiting, so signal.
                if (q.Count <= 1)
                {
                    Monitor.Pulse(qLock);
                }
            }
        }

        internal void close()
        {
            lock (qLock)
            {
                finished = true;
                Monitor.Pulse(qLock);
            }
        }

        internal int Count
        {
            get
            {
                lock (qLock)
                {
                    return q.Count;
                }
            }
        }

    } // class Channel

#endif

}

