using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gw2Launcher.Tools
{
    class QueuedAccountApi : IDisposable
    {
        public event EventHandler<DataEventArgs> DataReceived;

        public class DataEventArgs : EventArgs
        {
            private QueuedAccountApi source;

            public DataEventArgs(QueuedAccountApi source, Settings.IAccount account, Api.Account response, Api.Account responsePrevious, byte attempt, DateTime dateScheduled, DateTime dateRequested, object data)
            {
                this.source = source;
                this.Account = account;
                this.Response = response;
                this.ResponsePreviousAttempt = responsePrevious;
                this.Attempt = attempt;
                this.Date = dateRequested;
                this.DateScheduled = dateScheduled;
                this.Data = data;
            }

            public Settings.IAccount Account
            {
                get;
                private set;
            }

            /// <summary>
            /// The date the response was received
            /// </summary>
            public DateTime Date
            {
                get;
                private set;
            }

            /// <summary>
            /// The date the request was originally scheduled
            /// </summary>
            public DateTime DateScheduled
            {
                get;
                private set;
            }

            /// <summary>
            /// Data that was passed to the request
            /// </summary>
            public object Data
            {
                get;
                private set;
            }

            public Api.Account Response
            {
                get;
                private set;
            }

            /// <summary>
            /// The previous response when rescheduling
            /// </summary>
            public Api.Account ResponsePreviousAttempt
            {
                get;
                private set;
            }

            public byte Attempt
            {
                get;
                private set;
            }

            public void Reschedule(int millisDelay)
            {
                var q = new QueuedAccount(this.Account)
                {
                    ticks = DateTime.UtcNow.Ticks / 10000 + millisDelay,
                    attempt = (byte)(Attempt + 1),
                    response = this.Response,
                    date = this.DateScheduled,
                    data = this.Data,
                };
                source.Schedule(q, false);
            }
        }

        private class QueuedAccount : IEquatable<QueuedAccount>
        {
            public Settings.IAccount account;
            public byte retry;
            public long ticks;
            public byte attempt;
            public DateTime date;
            public object data;
            public Api.Account response;

            public QueuedAccount(Settings.IAccount account)
            {
                this.account = account;
            }

            public bool Equals(QueuedAccount other)
            {
                return this.account.Equals(other.account) && this.data == other.data;
            }

            public override int GetHashCode()
            {
                var h = account.GetHashCode();
                if (data != null)
                    h += data.GetHashCode();
                return h;
            }
        }

        private Util.SortedQueue<long, QueuedAccount> queue;
        private Task<int> task;
        private long first;

        public QueuedAccountApi()
        {
            queue = new Util.SortedQueue<long, QueuedAccount>();
        }

        public void Dispose()
        {
            if (task != null && task.IsCompleted)
                task.Dispose();
        }

        /// <summary>
        /// The account will be queued to use the API after the specified amount of time.
        /// Adding an account that is already queued will overwrite it, unless it has a unique data object attached.
        /// </summary>
        /// <param name="data">Optional data that will be returned with the response</param>
        public void Schedule(Settings.IAccount account, object data, int millisDelay)
        {
            var now = DateTime.UtcNow;
            var q = new QueuedAccount(account)
            {
                ticks = now.Ticks / 10000 + millisDelay,
                date = now,
                data = data,
            };
            Schedule(q, millisDelay == 0);
        }

        private void Schedule(QueuedAccount q, bool nodelay)
        {
            queue.Add(q.ticks, q);

            if (task != null && !task.IsCompleted)
                return;

            if (nodelay)
            {
                if (first != 0)
                    Util.ScheduledEvents.Unregister(OnScheduledCallback);
                first = q.ticks;

                DoQueue();
            }
            else if (first == 0 || q.ticks < first)
            {
                first = q.ticks;
                Util.ScheduledEvents.Register(OnScheduledCallback, q.ticks);
            }
        }

        private int OnScheduledCallback()
        {
            DoQueue();

            if (task.IsCompleted)
            {
                var ms = task.Result;

                using (task)
                {
                    task = null;
                }

                return ms;
            }

            return -1;
        }

        private void DoQueue()
        {
            if (task != null)
            {
                using (task)
                {
                    task = null;
                }
            }
            task = DoQueueAsync();
        }

        private async Task<int> DoQueueAsync()
        {
            while (true)
            {
                if (queue.Count == 0)
                {
                    first = 0;
                    return -1;
                }

                var q = queue.First;
                var next = (int)(q.ticks - DateTime.UtcNow.Ticks / 10000);

                if (next > 0)
                {
                    first = q.ticks;

                    if (task != null)
                        Util.ScheduledEvents.Register(OnScheduledCallback, q.ticks);

                    return next;
                }
                else
                {
                    q = queue.Dequeue();

                    if (!string.IsNullOrEmpty(q.account.ApiKey))
                    {
                        bool retry = true;

                        var date = DateTime.UtcNow;
                        Api.Account data;
                        try
                        {
                            data = await Api.Account.GetAccountAsync(q.account.ApiKey);
                        }
                        catch (Exception e)
                        {
                            Util.Logging.Log(e);
                            if (e is Api.Exceptions.PermissionRequiredException || e is Api.Exceptions.InvalidKeyException)
                            {
                                retry = false;
                            }
                            data = null;
                        }

                        if (data == null)
                        {
                            if (retry && q.retry++ < 5)
                            {
                                q.ticks = DateTime.UtcNow.Ticks / 10000 + 60000;
                                queue.Add(q.ticks, q);
                            }
                        }
                        else
                        {
                            if (DataReceived != null)
                                DataReceived(this, new DataEventArgs(this, q.account, data, q.response, q.attempt, q.date, date, q.data));
                        }
                    }
                }
            }
        }
    }
}
