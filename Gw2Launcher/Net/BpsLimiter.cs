using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;

namespace Gw2Launcher.Net
{
    public class BpsLimiter : IDisposable
    {
        private DateTime nextReset;
        private int bps;
        private int bpsLimit;
        private byte cycle;
        private byte threads, wid, waited, waiting;
        private bool abortWaiting;
        private System.Threading.ManualResetEvent reset;

        public class BpsShare : IDisposable
        {
            private BpsLimiter limiter;
            private int limit;
            private byte cycle;

            public BpsShare(BpsLimiter limiter)
            {
                this.limiter = limiter;
                lock (limiter)
                {
                    limiter.threads++;
                }
            }

            public void Used(int read)
            {
                var remaining = limit - read;
                if (remaining > 0)
                {
                    lock (limiter)
                    {
                        if (limiter.cycle == this.cycle)
                        {
                            limiter.bps -= remaining;
                            limiter.reset.Set();
                        }
                    }
                }
            }

            private int _GetLimit(int maximum)
            {
                var now = DateTime.UtcNow;
                if (now > limiter.nextReset)
                {
                    limiter.bps = 0;
                    limiter.nextReset = now.AddSeconds(1);
                    limiter.cycle++;
                    limiter.reset.Set();
                }

                var remaining = limiter.bpsLimit - limiter.bps;
                if (remaining > 0)
                {
                    this.limit = limiter.bpsLimit / limiter.threads;
                    if (this.limit > remaining || remaining < 1024)
                    {
                        if (remaining > maximum)
                            remaining = maximum;
                        else
                            limiter.reset.Reset();
                        this.limit = remaining;
                    }
                    else if (this.limit > maximum)
                        this.limit = maximum;

                    this.cycle = limiter.cycle;

                    limiter.bps += this.limit;

                    return this.limit;
                }

                return 0;
            }

            public int GetLimit(int maximum)
            {
                //if the limit hasn't been reached, it's a free for all, otherwise threads are dealt with in order

                bool isWaiting = false;

                try
                {
                    ushort wid;
                    int delay;

                    lock (limiter)
                    {
                        if (limiter.wid == limiter.waited)
                        {
                            var limit = _GetLimit(maximum);
                            if (limit > 0)
                                return limit;
                        }

                        wid = limiter.wid++;
                        limiter.waiting++;
                        isWaiting = true;

                        delay = (int)(limiter.nextReset.Subtract(DateTime.UtcNow).TotalMilliseconds + 0.5) + 1;
                    }

                    if (delay > 0)
                        limiter.reset.WaitOne(delay);

                    Monitor.Enter(limiter);
                    try
                    {
                        do
                        {
                            if (wid == limiter.waited)
                            {
                                var limit = _GetLimit(maximum);
                                if (limit > 0)
                                {
                                    if (++limiter.waited == limiter.wid)
                                        limiter.waited = limiter.wid = 0;

                                    limiter.waiting--;
                                    isWaiting = false;

                                    return limit;
                                }

                                delay = (int)(limiter.nextReset.Subtract(DateTime.UtcNow).TotalMilliseconds + 0.5) + 1;
                            }
                            else if (limiter.abortWaiting)
                            {
                                break;
                            }
                            else if (limiter.bps < limiter.bpsLimit)
                            {
                                delay = 0;
                            }
                            else
                            {
                                delay = (int)(limiter.nextReset.Subtract(DateTime.UtcNow).TotalMilliseconds + 0.5) + 1;
                            }

                            if (delay > 0)
                            {
                                Monitor.PulseAll(limiter);
                                Monitor.Exit(limiter);

                                limiter.reset.WaitOne(delay);

                                Monitor.Enter(limiter);
                            }
                            else
                                Monitor.Wait(limiter);
                        }
                        while (true);
                    }
                    finally
                    {
                        if (Monitor.IsEntered(limiter))
                        {
                            Monitor.PulseAll(limiter);
                            Monitor.Exit(limiter);
                        }
                    }

                    //one of the waiting threads was aborted -- the wait queue is invalid

                    lock (limiter)
                    {
                        if (--limiter.waiting == 0)
                        {
                            limiter.wid = limiter.waited = 0;
                            limiter.abortWaiting = false;
                        }
                        isWaiting = false;
                    }

                    while (limiter.abortWaiting)
                    {
                        Thread.Sleep(10);
                    }
                }
                finally
                {
                    if (isWaiting)
                    {
                        lock (limiter)
                        {
                            if (--limiter.waiting == 0)
                            {
                                limiter.wid = limiter.waited = 0;
                                limiter.abortWaiting = false;
                            }
                            else
                                limiter.abortWaiting = true;
                        }
                    }
                }

                return GetLimit(maximum);
            }

            public void Dispose()
            {
                if (limiter != null)
                {
                    lock (limiter)
                    {
                        limiter.threads--;
                    }
                    limiter = null;
                }
            }
        }

        public BpsLimiter()
        {
            reset = new ManualResetEvent(false);
            bpsLimit = Int32.MaxValue;
        }

        public bool Enabled
        {
            get
            {
                return this.bpsLimit != Int32.MaxValue;
            }
            set
            {
                if (value)
                {
                    if (this.bpsLimit == Int32.MaxValue)
                        this.bpsLimit = Int32.MaxValue - 1;
                }
                else
                {
                    this.bpsLimit = Int32.MaxValue;
                }
            }
        }

        public int BpsLimit
        {
            get
            {
                return bpsLimit;
            }
            set
            {
                this.bpsLimit = value;
            }
        }

        public BpsShare GetShare()
        {
            return new BpsShare(this);
        }

        public void Dispose()
        {
            if (reset != null)
            {
                abortWaiting = true;
                bpsLimit = Int32.MaxValue;
                reset.Dispose();
                reset = null;
            }
        }
    }
}
