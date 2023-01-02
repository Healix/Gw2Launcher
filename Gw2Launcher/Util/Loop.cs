using System;
using System.Threading;

namespace Gw2Launcher.Util
{
    static class Loop
    {
        public interface ILoop
        {
            event EventHandler Complete;

            /// <summary>
            /// Waits for all threads to finish
            /// </summary>
            void Wait();

            /// <summary>
            /// Immediately terminates all threads
            /// </summary>
            void Abort();

            /// <summary>
            /// Terminates all threads after completing their current task
            /// </summary>
            void Break();

            bool IsComplete
            {
                get;
            }

            object Result
            {
                get;
            }
        }

        public interface IState
        {
            /// <summary>
            /// Breaks out of the loop, terminating all threads after completing their current task
            /// </summary>
            void Break();

            /// <summary>
            /// Immediately terminates all threads
            /// </summary>
            void Abort();

            /// <summary>
            /// Breaks out of the loop, termination all threads after completing their current task and returning the value
            /// </summary>
            void Return(object o);
        }

        public delegate void WorkAction(byte thread, long index, IState state);

        private class ParallelFor : IState, ILoop
        {
            private class Worker
            {
                private ParallelFor source;
                private long index, last;
                private byte threadIndex;
                private bool completed;
                private Thread thread;

                public Worker(ParallelFor p, byte index)
                {
                    this.source = p;
                    this.threadIndex = index;
                }

                public void DoWork()
                {
                    while (true)
                    {
                        lock (source)
                        {
                            if (source.from < source.to && !source.abort)
                            {
                                index = source.from++;
                            }
                            else
                            {
                                lock (this)
                                {
                                    thread = null;
                                    completed = true;
                                    if (--source.active == 0)
                                        source.OnComplete();
                                    return;
                                }
                            }
                        }

                        try
                        {
                            source.work(threadIndex, index, source);
                        }
                        catch (Exception e)
                        {
                            Util.Logging.Log(e);
                        }
                    }
                }

                public void Start()
                {
                    lock (this)
                    {
                        if (thread == null || !thread.IsAlive)
                        {
                            completed = false;
                            thread = new Thread(DoWork);
                            thread.IsBackground = true;
                            thread.Start();
                        }
                    }
                }

                public void Abort()
                {
                    lock (this)
                    {
                        var t = thread;

                        if (t != null && t.IsAlive)
                        {
                            thread = null;
                            completed = true;

                            t.Abort();
                        }
                    }
                }

                public void Restart()
                {
                    lock (this)
                    {
                        Abort();
                        Start();
                    }
                }

                public bool Wait(int millis)
                {
                    if (millis > 0)
                    {
                        return thread.Join(millis);
                    }
                    else
                    {
                        thread.Join();
                        return true;
                    }
                }

                public bool IsComplete
                {
                    get
                    {
                        return completed;
                    }
                }

                public bool IsBlocked()
                {
                    if (completed)
                        return false;

                    var i = index;

                    if (i == last)
                    {
                        return true;
                    }

                    last = i;

                    return false;
                }
            }

            public event EventHandler Complete;

            private long from, to;
            private int timeout;
            private WorkAction work;
            private Worker[] threads;
            private byte active;
            private bool abort, terminate;
            private object result;

            public ParallelFor(long from, long to, byte threads, int timeout, WorkAction work)
            {
                this.from = from;
                this.to = to;
                this.threads = new Worker[threads];
                this.timeout = timeout;
                this.work = work;
                this.active = threads;

                lock (this)
                {
                    for (byte i = 0; i < threads; i++)
                    {
                        var t = this.threads[i] = new Worker(this, i);
                        t.Start();
                    }
                }
            }

            private void OnComplete()
            {
                if (Complete != null)
                    Complete(this, EventArgs.Empty);
            }

            private Worker FindActive()
            {
                if (active == 0)
                    return null;

                foreach (var t in threads)
                {
                    if (t.IsComplete)
                        continue;

                    return t;
                }

                return null;
            }

            public void Wait()
            {
                while (true)
                {
                    if (terminate)
                    {
                        foreach (var t in threads)
                        {
                            if (!t.IsComplete)
                                t.Abort();
                        }

                        break;
                    }

                    var active = FindActive();
                    if (active == null)
                        break;

                    if (!active.Wait(timeout))
                    {
                        foreach (var t in threads)
                        {
                            if (t.IsBlocked())
                            {
                                lock (this)
                                {
                                    if (!abort)
                                    {
                                        t.Restart();
                                    }
                                }
                            }
                        }
                    }
                }
            }

            public void Abort()
            {
                lock (this)
                {
                    abort = true;
                    terminate = true;
                }
            }

            public void Break()
            {
                lock (this)
                {
                    if (abort)
                        return;
                    abort = true;
                }
            }

            public void Return(object o)
            {
                lock (this)
                {
                    if (abort)
                        return;
                    abort = true;
                    result = o;
                }
            }

            public bool IsComplete
            {
                get
                {
                    return active == 0;
                }
            }

            public object Result
            {
                get
                {
                    return result;
                }
            }
        }

        /// <summary>
        /// Loops through the items using multiple threads
        /// </summary>
        /// <param name="from">Start index, inclusive</param>
        /// <param name="to">End index, exclusive</param>
        /// <param name="threads">Number of threads to use</param>
        /// <param name="timeout">The duration (ms) a task can run for before being aborted, if > 0</param>
        /// <param name="work">Work to run</param>
        public static ILoop For(long from, long to, byte threads, int timeout, WorkAction work)
        {
            return new ParallelFor(from, to, threads, timeout, work);
        }
    }
}
