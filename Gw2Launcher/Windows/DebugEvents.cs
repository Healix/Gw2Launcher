using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.Runtime.InteropServices;
using System.Security;
using Gw2Launcher.Windows.Native;

namespace Gw2Launcher.Windows
{
    class DebugEvents
    {
        private class QueueItem
        {
            public enum QueueType
            {
                Add,
                Remove,
                Exited
            }

            public QueueItem(int pid, QueueType type, string username = null, SecureString password = null)
            {
                this.pid = pid;
                this.type = type;
                this.username = username;
                this.password = password;
            }

            public QueueType type;
            public int pid;
            public string username;
            public SecureString password;
        }

        private class EventItem
        {
            public EventItem(string username = null, SecureString password = null)
            {
                this.username = username;
                this.password = password;
            }

            public byte references;
            public string username;
            public SecureString password;

            public bool HasCredentials
            {
                get
                {
                    return password != null && username != null;
                }
            }

            public void Stop()
            {

            }
        }

        private class DebugEventsToken : IDebugEventsToken
        {
            private DebugEvents d;
            private int pid;

            public DebugEventsToken(DebugEvents d, int pid)
            {
                this.d = d;
                this.pid = pid;
            }

            public void Dispose()
            {
                lock (this)
                {
                    if (d != null)
                    {
                        d.Remove(pid);
                        d = null;
                    }
                }
            }
        }

        public interface IDebugEventsToken : IDisposable
        {

        }

        private Queue<QueueItem> queue;
        private Task task;
        private bool hasMore;

        public DebugEvents()
        {
            queue = new Queue<QueueItem>();
        }

        static DebugEvents()
        {
            Instance = new DebugEvents();
        }

        public static readonly DebugEvents Instance;

        public IDebugEventsToken Add(int pid, string username = null, SecureString password = null)
        {
            Queue(new QueueItem(pid, QueueItem.QueueType.Add, username, password));

            return new DebugEventsToken(this, pid);
        }

        private void Remove(int pid)
        {
            Queue(new QueueItem(pid, QueueItem.QueueType.Remove));
        }

        private void Queue(QueueItem q)
        {
            lock (queue)
            {
                hasMore = true;
                queue.Enqueue(q);

                if (queue.Count == 1 && (task == null || task.IsCompleted))
                {
                    if (q.type != QueueItem.QueueType.Add)
                    {
                        //nothing is active, skipping removals
                        queue.Clear();
                        return;
                    }
                    task = new Task(DoEvents, TaskCreationOptions.LongRunning);
                    task.Start();
                }
            }
        }

        private void DoEvents()
        {
            var pids = new Dictionary<int, EventItem>();
            var d = new DEBUG_EVENT();
            var c = ContinueStatus.DBG_CONTINUE;

            while (true)
            {
                #region Queue

                if (hasMore)
                {
                    while (true)
                    {
                        QueueItem q;

                        lock (queue)
                        {
                            if (queue.Count == 0)
                            {
                                hasMore = false;

                                if (pids.Count == 0)
                                {
                                    task = null;
                                    return;
                                }

                                break;
                            }

                            q = queue.Dequeue();
                        }

                        EventItem e;

                        if (pids.TryGetValue(q.pid, out e))
                        {
                            if (q.type == QueueItem.QueueType.Remove)
                            {
                                if (--e.references == 0)
                                {
                                    SetDebugging(false, q.pid, e);
                                    pids.Remove(q.pid);
                                }

                                continue;
                            }
                            else if (q.type == QueueItem.QueueType.Exited)
                            {
                                SetDebugging(false, q.pid, e);
                                pids.Remove(q.pid);
                            }
                        }
                        else if (q.type == QueueItem.QueueType.Add)
                        {
                            pids[q.pid] = e = new EventItem(q.username, q.password);
                        }

                        if (q.type == QueueItem.QueueType.Add)
                        {
                            if (++e.references == 1)
                            {
                                if (!SetDebugging(true, q.pid, e))
                                {
                                    pids.Remove(q.pid);
                                }
                            }
                        }
                    }
                }

                #endregion

                if (NativeMethods.WaitForDebugEvent(ref d, 100))
                {
                    if (d.dwDebugEventCode == DebugEventType.EXCEPTION_DEBUG_EVENT)
                        c = ContinueStatus.DBG_EXCEPTION_NOT_HANDLED;
                    else
                        c = ContinueStatus.DBG_CONTINUE;

                    if (!NativeMethods.ContinueDebugEvent(d.dwProcessId, d.dwThreadId, c) || d.dwDebugEventCode == DebugEventType.EXIT_PROCESS_DEBUG_EVENT)
                    {
                        lock (queue)
                        {
                            queue.Enqueue(new QueueItem(d.dwProcessId, QueueItem.QueueType.Exited));
                            hasMore = true;
                        }
                    }
                }
            }
        }

        private bool SetDebugging(bool enabled, int pid, EventItem e)
        {
            if (e.HasCredentials)
            {
                try
                {
                    using (Security.Impersonation.Impersonate(e.username, e.password))
                    {
                        return SetDebugging(enabled, pid);
                    }
                }
                catch
                {
                    return false;
                }
            }
            return SetDebugging(enabled, pid);
        }

        private bool SetDebugging(bool enabled, int pid)
        {
            if (enabled)
                return NativeMethods.DebugActiveProcess(pid);
            else
                return NativeMethods.DebugActiveProcessStop(pid);
        }
    }
}
