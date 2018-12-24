using System;
using System.Collections.Generic;
using System.Windows.Forms;
using System.Threading;

namespace Gw2Launcher.Util
{
    /// <summary>
    /// Schedules events to be called at a later time. Must be initialized from the main UI thread, which will be used to synchronize all events.
    /// </summary>
    static class ScheduledEvents
    {
        /// <summary>
        /// Called when the scheduled event is ready
        /// </summary>
        /// <returns>The number of milliseconds until the next call or -1 to cancel</returns>
        public delegate int ScheduledEventCallbackEventHandler();

        private class Node
        {
            public ScheduledEventCallbackEventHandler callback;
            public long ticks;
            public Node previous, next;

            public Node(ScheduledEventCallbackEventHandler callback)
            {
                this.callback = callback;
            }

            public void Detach()
            {
                if (next != null)
                    next.previous = previous;
                if (previous != null)
                    previous.next = next;

                next = previous = null;
            }

            public void Attach(Node previous, Node next)
            {
                this.previous = previous;
                if (previous != null)
                    previous.next = this;
                this.next = next;
                if (next != null)
                    next.previous = this;
            }
        }

        //private class Instance
        //{
        //    public Instance()
        //    {
        //        Microsoft.Win32.SystemEvents.PowerModeChanged += SystemEvents_PowerModeChanged;
        //    }

        //    void SystemEvents_PowerModeChanged(object sender, Microsoft.Win32.PowerModeChangedEventArgs e)
        //    {
        //        switch (e.Mode)
        //        {
        //            case Microsoft.Win32.PowerModes.Resume:

        //                OnSystemWakeup();

        //                break;
        //        }
        //    }

        //    ~Instance()
        //    {
        //        Microsoft.Win32.SystemEvents.PowerModeChanged -= SystemEvents_PowerModeChanged;
        //    }
        //}

        private static Node first, last;
        private static int count;
        private static System.Windows.Forms.Timer timer;
        private static Dictionary<ScheduledEventCallbackEventHandler, Node> events;
        //private static Instance instance;
        private static SynchronizationContext context;
        private static int contextId;

        static ScheduledEvents()
        {
            //note: Forms.Timer is unaffected by system sleep
            //instance = new Instance();
            events = new Dictionary<ScheduledEventCallbackEventHandler, Node>();
            timer = new System.Windows.Forms.Timer();
            timer.Tick += timer_Tick;
            context = SynchronizationContext.Current;
            contextId = Thread.CurrentThread.ManagedThreadId;
        }

        /// <summary>
        /// The event will be called at the specified time in milliseconds (ticks / 10000)
        /// </summary>
        public static void Register(ScheduledEventCallbackEventHandler e, long millis)
        {
            if (contextId != Thread.CurrentThread.ManagedThreadId)
            {
                context.Send(
                    delegate
                    {
                        Register(e, millis);
                    }, null);
                return;
            }

            Node n;
            if (!events.TryGetValue(e, out n))
            {
                n = new Node(e);
                events[e] = n;
                count++;
            }
            else
            {
                if (first == n)
                {
                    if (first == last)
                        first = last = null;
                    else
                        first = n.next;
                }
                else if (last == n)
                {
                    last = n.previous;
                }

                n.Detach();
            }

            n.ticks = millis + 1;

            if (Insert(n))
            {
                var t = (int)(millis - DateTime.UtcNow.Ticks / 10000);
                if (t < 0)
                    t = 0;

                context.Send(
                    delegate
                    {
                        if (t >= int.MaxValue)
                            timer.Interval = int.MaxValue;
                        else
                            timer.Interval = t + 1;
                        timer.Enabled = true;
                    }, null);
            }
        }

        /// <summary>
        /// The event will be called after the specified amount of milliseconds
        /// </summary>
        public static void Register(ScheduledEventCallbackEventHandler e, int millisToNext)
        {
            if (contextId != Thread.CurrentThread.ManagedThreadId)
            {
                context.Send(
                    delegate
                    {
                        Register(e, millisToNext);
                    }, null);
                return;
            }

            Node n;
            if (!events.TryGetValue(e, out n))
            {
                n = new Node(e);
                events[e] = n;
                count++;
            }
            else
            {
                if (first == n)
                {
                    if (first == last)
                        first = last = null;
                    else
                        first = n.next;
                }
                else if (last == n)
                {
                    last = n.previous;
                }

                n.Detach();
            }

            n.ticks = DateTime.UtcNow.Ticks / 10000 + millisToNext + 1;

            if (Insert(n))
            {
                if (millisToNext >= int.MaxValue)
                    timer.Interval = int.MaxValue;
                else 
                    timer.Interval = millisToNext + 1;
                timer.Enabled = true;
            }
        }

        private static bool Insert(Node n)
        {
            if (first == null)
            {
                first = n;
                last = n;

                return true;
            }
            else if (n.ticks <= first.ticks)
            {
                first.previous = n;
                n.next = first;
                first = n;

                return true;
            }
            else if (n.ticks >= last.ticks)
            {
                last.next = n;
                n.previous = last;
                last = n;

                return false;
            }
            else
            {
                var next = first.next;

                do
                {
                    if (n.ticks <= next.ticks)
                    {
                        var p = next.previous;
                        p.next = n;
                        n.previous = p;
                        n.next = next;
                        next.previous = n;

                        return false;
                    }

                    next = next.next;
                }
                while (true);
            }
        }

        public static void Unregister(ScheduledEventCallbackEventHandler e)
        {
            if (contextId != Thread.CurrentThread.ManagedThreadId)
            {
                context.Send(
                    delegate
                    {
                        Unregister(e);
                    }, null);
                return;
            }

            Node n;
            if (!events.TryGetValue(e, out n))
                return;

            events.Remove(e);

            if (count == 1)
            {
                first = last = null;
            }
            else
            {
                if (first == n)
                    first = n.next;
                else if (last == n)
                    last = n.previous;

                n.Detach();
            }

            count--;
        }

        private static Node RemoveFirst()
        {
            Node _first = first;

            if (count > 0)
            {
                events.Remove(first.callback);

                if (count == 1)
                    first = last = null;
                else
                {
                    var n = first;
                    first = first.next;
                    n.Detach();
                }

                count--;
            }

            return _first;
        }

        public static DateTime GetDate(ScheduledEventCallbackEventHandler e)
        {
            if (contextId != Thread.CurrentThread.ManagedThreadId)
            {
                var d = DateTime.MaxValue;
                context.Send(
                    delegate
                    {
                        d = GetDate(e);
                    }, null);
                return d;
            }

            Node n;
            if (!events.TryGetValue(e, out n))
                return DateTime.MaxValue;

            return new DateTime((n.ticks - 1) * 10000, DateTimeKind.Utc);
        }

        static void timer_Tick(object sender, EventArgs e)
        {
            var ticks = DateTime.UtcNow.Ticks / 10000;

            while (first != null && ticks >= first.ticks)
            {
                var n = RemoveFirst();
                var ms = n.callback();

                if (ms != -1)
                {
                    if (!events.ContainsKey(n.callback))
                    {
                        n.previous = n.next = null;
                        n.ticks = ticks + ms;

                        events[n.callback] = n;
                        count++;

                        Insert(n);
                    }
                }
            }

            if (first == null)
            {
                timer.Enabled = false;
            }
            else
            {
                ticks = first.ticks - ticks;
                if (ticks < 1)
                    ticks = 1;
                else if (ticks > int.MaxValue)
                    ticks = int.MaxValue;
                timer.Interval = (int)ticks;
            }
        }

        static void OnSystemWakeup()
        {
            if (contextId != Thread.CurrentThread.ManagedThreadId)
            {
                context.Send(
                    delegate
                    {
                        OnSystemWakeup();
                    }, null);
                return;
            }

            if (first != null)
            {
                var ticks = DateTime.UtcNow.Ticks / 10000;
                ticks = first.ticks - ticks;
                if (ticks < 1)
                    ticks = 1;
                else if (ticks > int.MaxValue)
                    ticks = int.MaxValue;
                timer.Interval = (int)ticks;
            }
        }
    }
}
