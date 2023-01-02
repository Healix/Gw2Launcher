using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.IO;
using System.Diagnostics;
using System.Management;
using Gw2Launcher.Windows.Native;

namespace Gw2Launcher.Client
{
    static partial class Launcher
    {
        private const ushort TIMEOUT_TRYENTER = 1000;

        //public delegate void AccountStateChangedEventHandler(ushort uid, AccountState state, AccountState previousState, object data);
        public delegate void AccountEventHandler(Settings.IAccount account);
        public delegate void AccountEventHandler<T>(Settings.IAccount account, T e);
        public delegate void AccountTypeEventHandler<T>(AccountType type, T e);
        private delegate void InternalAccountEventHandler<T>(Account account, T e);
        public delegate void LaunchExceptionEventHandler(Settings.IAccount account, LaunchExceptionEventArgs e);
        public delegate void BuildUpdatedEventHandler(BuildUpdatedEventArgs e);

        public class AccountTopMostWindowEventEventArgs : AccountWindowEventEventArgs
        {
            private Util.SortedQueue<byte, IntPtr> windows;

            public AccountTopMostWindowEventEventArgs(Process process, IntPtr handle)
                : base(EventType.TopMost, process, handle)
            {

            }

            /// <summary>
            /// Adds other windows to show on top of the window
            /// </summary>
            /// <param name="priority">Highest is on top</param>
            public void Add(byte priority, IntPtr window)
            {
                if (windows == null)
                    windows = new Util.SortedQueue<byte, IntPtr>();
                windows.Add(priority, window);
            }

            public Util.SortedQueue<byte, IntPtr> Windows
            {
                get
                {
                    return windows;
                }
            }

            public int Count
            {
                get
                {
                    return windows != null ? windows.Count : 0;
                }
            }
        }

        public class AccountWindowEventEventArgs : EventArgs
        {
            public enum EventType
            {
                WindowReady,
                WindowLoaded,
                Focused,
                Minimized,
                TopMost,
                BoundsChanged,
                TemplateChanged,
                WindowOptionsChanged,
            }

            public AccountWindowEventEventArgs(EventType type, Process process, IntPtr handle)
            {
                this.Type = type;
                this.Process = process;
                this.Handle = handle;
            }

            public AccountWindowEventEventArgs (EventType type, Process process)
            {
                this.Type = type;
                this.Process = process;
                this.Handle = Windows.FindWindow.FindMainWindow(process);
            }

            public Process Process
            {
                get;
                private set;
            }

            public IntPtr Handle
            {
                get;
                private set;
            }

            public EventType Type
            {
                get;
                private set;
            }

            public bool Handled
            {
                get;
                set;
            }
        }

        public class AccountStateEventArgs : EventArgs
        {
            public AccountStateEventArgs(Settings.IAccount account, AccountState state, AccountState previousState, object data)
            {
                this.Account = account;
                this.State = state;
                this.PreviousState = previousState;
                this.Data = data;
            }

            public Settings.IAccount Account
            {
                get;
                private set;
            }

            public ushort UID
            {
                get
                {
                    return Account.UID;
                }
            }

            public AccountState State
            {
                get;
                private set;
            }

            public AccountState PreviousState
            {
                get;
                private set;
            }

            public Object Data
            {
                get;
                set;
            }
        }

        private class NodeQueue : IEnumerable<QueuedLaunch>
        {
            private class Node
            {
                public QueuedLaunch value;
                public Node next;
            }

            private Node[] first;
            private Node last;
            private ushort[] count;

            public NodeQueue()
            {
                first = new Node[3];
                count = new ushort[3];
            }

            private byte GetIndex(AccountType type)
            {
                switch (type)
                {
                    case AccountType.GuildWars1:
                        return 1;
                    case AccountType.GuildWars2:
                        return 2;
                    case AccountType.Any:
                    default:
                        return 0;
                }
            }

            /// <summary>
            /// Adds the item to the top of the queue
            /// </summary>
            public void Push(QueuedLaunch q)
            {
                var n = new Node()
                {
                    value = q,
                };

                var i = GetIndex(q.Type);

                if (i > 0)
                {
                    first[i] = n;
                    ++count[i];
                }

                if (count[0] == 0)
                {
                    first[0] = last = n;
                }
                else
                {
                    n.next = first[0];
                    first[0] = n;
                }

                ++count[0];
            }

            /// <summary>
            /// Adds the item to the end of the queue
            /// </summary>
            public void Enqueue(QueuedLaunch q)
            {
                var n = new Node()
                {
                    value = q,
                };

                var i = GetIndex(q.Type);

                if (i > 0)
                {
                    if (count[i] == 0)
                    {
                        first[i] = n;
                    }

                    ++count[i];
                }

                if (count[0] == 0)
                {
                    first[0] = last = n;
                }
                else
                {
                    last.next = n;
                    last = n;
                }

                ++count[0];
            }

            public QueuedLaunch Peek()
            {
                if (count[0] == 0)
                    return null;
                return first[0].value;
            }

            public QueuedLaunch Peek(AccountType t)
            {
                var i = GetIndex(t);
                var n = first[i];

                if (n != null)
                {
                    return n.value;
                }

                return null;
            }

            private Node FindNext(Node from)
            {
                var i = GetIndex(from.value.Type);
                var n = from.next;

                while (n != null)
                {
                    if (GetIndex(n.value.Type) == i)
                    {
                        return n;
                    }

                    n = n.next;
                }

                return null;
            }

            private Node FindParent(Node from)
            {
                if (object.ReferenceEquals(first[0], from))
                    return null;

                Node p = null;
                var n = first[0];

                while (n != null)
                {
                    if (object.ReferenceEquals(n.next, from))
                        return n;

                    p = n;
                    n = n.next;
                }

                return null;
            }

            public QueuedLaunch Dequeue()
            {
                if (count[0] != 0)
                {
                    var n = first[0];
                    var i = GetIndex(n.value.Type);

                    if (i > 0)
                    {
                        if (--count[i] == 0)
                        {
                            first[i] = null;
                        }
                        else
                        {
                            first[i] = FindNext(n);
                        }
                    }

                    if (--count[0] == 0)
                    {
                        first[0] = last = null;
                    }
                    else
                    {
                        first[0] = n.next;
                    }

                    return n.value;
                }

                return null;
            }

            public bool Remove(QueuedLaunch q)
            {
                foreach (var l in Dequeue(
                    delegate(QueuedLaunch _q)
                    {
                        return object.ReferenceEquals(q, _q);
                    }))
                {
                    return true;
                }

                return false;
            }

            public QueuedLaunch Dequeue(AccountType t)
            {
                foreach (var l in Dequeue(
                    delegate(QueuedLaunch q)
                    {
                        return t.HasFlag(q.Type);
                    }))
                {
                    return l;
                }

                return null;
            }

            public QueuedLaunch Dequeue(AccountType t, LaunchMode m)
            {
                foreach (var l in Dequeue(
                    delegate(QueuedLaunch q)
                    {
                        return q.mode == m && t.HasFlag(q.Type);
                    }))
                {
                    return l;
                }

                return null;
            }

            /// <summary>
            /// Enumerates through the queue, only removing items that match
            /// </summary>
            /// <param name="search">Matching items will be removed from the queue</param>
            public IEnumerable<QueuedLaunch> Dequeue(Func<QueuedLaunch, bool> search)
            {
                Node p = null;
                var n = first[0];

                while (n != null)
                {
                    if (search(n.value))
                    {
                        var i = GetIndex(n.value.Type);

                        if (i > 0)
                        {
                            if (--count[i] == 0)
                            {
                                first[i] = null;
                            }
                            else
                            {
                                first[i] = FindNext(n);
                            }
                        }

                        if (--count[0] == 0)
                        {
                            first[0] = last = null;
                        }
                        else if (p == null)
                        {
                            first[0] = n.next;
                        }
                        else
                        {
                            p.next = n.next;
                            if (object.ReferenceEquals(n, last))
                                last = p;
                        }

                        yield return n.value;
                    }
                    else
                    {
                        p = n;
                    }

                    n = n.next;
                }
            }

            /// <summary>
            /// Enumerates through the queue, limited to the type of account and only removing items that match
            /// </summary>
            /// <param name="search">Matching items will be removed from the queue</param>
            public IEnumerable<QueuedLaunch> Dequeue(AccountType t, Func<QueuedLaunch, bool> search)
            {
                var i = GetIndex(t);
                var _count = count[i];

                var n = first[i];
                Node p = FindParent(n);

                while (_count-- > 0)
                {
                    if (GetIndex(n.value.Type) == i && search(n.value))
                    {
                        if (i > 0)
                        {
                            if (--count[i] == 0)
                            {
                                first[i] = null;
                            }
                            else
                            {
                                first[i] = FindNext(n);
                            }
                        }

                        if (--count[0] == 0)
                        {
                            first[0] = last = null;
                        }
                        else if (p == null)
                        {
                            first[0] = n.next;
                        }
                        else
                        {
                            p.next = n.next;
                            if (object.ReferenceEquals(n, last))
                                last = p;
                        }

                        yield return n.value;
                    }
                    else
                    {
                        p = n;
                    }

                    n = n.next;
                }
            }

            public ushort Count
            {
                get
                {
                    return count[0];
                }
            }

            public ushort GetCount(AccountType type)
            {
                return count[GetIndex(type)];
            }

            public void Clear()
            {
                for (var i = count.Length - 1; i >= 0;--i )
                {
                    count[i] = 0;
                    first[i] = null;
                }
                last = null;
            }

            public IEnumerator<QueuedLaunch> GetEnumerator()
            {
                var n = first[0];

                while (n != null)
                {
                    yield return n.value;

                    n = n.next;
                }
            }

            System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
            {
                return GetEnumerator();
            }
        }

        private class ScanPaths
        {
            public class Paths
            {
                public struct Path
                {
                    public Path(string directory, AccountType type)
                        : this()
                    {
                        this.Directory = directory;
                        this.Type = type;
                    }

                    public string Directory
                    {
                        get;
                        private set;
                    }

                    public AccountType Type
                    {
                        get;
                        private set;
                    }
                }

                private Dictionary<string, AccountType> paths;

                public Paths()
                {
                    paths = new Dictionary<string, AccountType>(StringComparer.OrdinalIgnoreCase);
                }

                public void Add(string path, AccountType type)
                {
                    paths[path] = type;
                }

                public AccountType Type
                {
                    get;
                    set;
                }

                public bool TryGetMatch(string path, out string match)
                {
                    foreach (var p in paths.Keys)
                    {
                        if (path.StartsWith(p, StringComparison.OrdinalIgnoreCase))
                        {
                            match = p;
                            return true;
                        }
                    }
                    match = null;
                    return false;
                }

                public bool ContainsMatch(string path)
                {
                    return TryGetMatch(path, out path);
                }

                public IEnumerable<Path> GetPaths()
                {
                    foreach (var v in paths)
                    {
                        yield return new Path(v.Key, v.Value);
                    }
                }

                public AccountType GetType(string path)
                {
                    return paths[path];
                }

                public bool Contains(string path)
                {
                    return paths.ContainsKey(path);
                }

                public int Count
                {
                    get
                    {
                        return paths.Count;
                    }
                }
            }

            private Dictionary<string, Paths> paths;

            public ScanPaths()
            {
                paths = new Dictionary<string, Paths>(StringComparer.OrdinalIgnoreCase);
            }

            public void Add(AccountType type)
            {
                if (type.HasFlag(AccountType.GuildWars2))
                {
                    if (Settings.GuildWars2.Path.HasValue && !string.IsNullOrEmpty(Settings.GuildWars2.Path.Value))
                        this.Add(AccountType.GuildWars2, Settings.GuildWars2.Path.Value);
                    if (Settings.GuildWars2.PathSteam.HasValue && !string.IsNullOrEmpty(Settings.GuildWars2.PathSteam.Value))
                        this.Add(AccountType.GuildWars2, Settings.GuildWars2.PathSteam.Value);
                }

                if (type.HasFlag(AccountType.GuildWars1))
                {
                    if (Settings.GuildWars1.Path.HasValue && !string.IsNullOrEmpty(Settings.GuildWars1.Path.Value))
                    {
                        this.Add(AccountType.GuildWars1, Settings.GuildWars1.Path.Value);

                        var exe = Path.GetFileName(Settings.GuildWars1.Path.Value);
                        foreach (var a in Util.Accounts.GetGw1Accounts())
                        {
                            if (a.DatFile != null)
                            {
                                this.Add(AccountType.GuildWars1, Path.Combine(Path.GetDirectoryName(a.DatFile.Path), exe));
                            }
                        }
                    }
                }
            }

            public void Add(AccountType type, string path)
            {
                var fi = new FileInfo(path);
                
                var processName = fi.Name.Substring(0, fi.Name.Length - fi.Extension.Length);
                var directory = fi.DirectoryName;

                Paths p;
                if (!paths.TryGetValue(processName, out p))
                    p = paths[processName] = new Paths();
                p.Type |= type;

                p.Add(directory, type);
            }

            public bool TryGetValue(string processName, out Paths path)
            {
                //process name can also end with .tmp or .000 when updating

                var l = processName.Length;
                if (l > 4 && processName[l - 4] == '.')
                {
                    bool b;
                    if (processName.EndsWith(".tmp"))
                        b = true;
                    else
                    {
                        b = true;
                        for (var i = l - 3; i < l && b; i++)
                        {
                            b = char.IsDigit(processName[i]);
                        }
                    }
                    if (b)
                        processName = processName.Substring(0, l - 4);
                }
                return paths.TryGetValue(processName, out path);
            }

            public int Count
            {
                get
                {
                    var count = 0;

                    foreach (var p in paths.Values)
                    {
                        count += p.Count;
                    }

                    return count;
                }
            }
        }

        private class Limiter
        {
            public interface ISession : IDisposable
            {
                /// <summary>
                /// Release using the specified time
                /// </summary>
                /// <param name="duration">Time (ms) it took to finish the login</param>
                void Release(int duration);

                /// <summary>
                /// Release using the time this session was alive
                /// </summary>
                /// <param name="success">If the login was successful</param>
                void Release(bool success = false);

                /// <summary>
                /// Resets the timestamp and causes the session to release on dispose instead of cancel
                /// </summary>
                void SetTime();

                /// <summary>
                /// Release without affecting the limiter
                /// </summary>
                void Cancel();
            }

            private class Session : ISession
            {
                private Limiter limiter;
                private int started;
                private bool confirmed;

                public Session(Limiter limiter)
                {
                    this.limiter = limiter;
                    this.started = Environment.TickCount;
                }

                ~Session()
                {
                    Dispose();
                }

                public void SetTime()
                {
                    confirmed = true;
                    started = Environment.TickCount;
                }

                private void OnRelease(int duration, bool success)
                {
                    lock (this)
                    {
                        if (limiter == null)
                            return;
                        if (success && duration > 1000)
                            duration -= 1000;
                        limiter.OnRelease(started, duration, success);
                        limiter = null;
                    }
                }

                public void Release(int duration)
                {
                    OnRelease(duration, true);
                }

                public void Release(bool success = false)
                {
                    OnRelease(Environment.TickCount - started, success);
                }

                public void Cancel()
                {
                    lock (this)
                    {
                        if (limiter == null)
                            return;
                        limiter.OnRelease(0, -1, false);
                        limiter = null;
                    }
                }

                public void Dispose()
                {
                    GC.SuppressFinalize(this);

                    if (confirmed)
                    {
                        Release(false);
                    }
                    else
                    {
                        Cancel();
                    }
                }
            }

            private const int MAXIMUM = 3;

            private byte current, maximum, recharge, rechargeTime;
            private int timestamp, blocked;
            private bool automatic;

            public Limiter(Settings.LaunchLimiterOptions o)
            {
                if (automatic = o.IsAutomatic)
                {
                    current = 0;
                    maximum = MAXIMUM;
                    timestamp = blocked = Environment.TickCount;
                }
                else
                {
                    current = maximum = o.Count;
                    recharge = o.RechargeCount;
                    rechargeTime = o.RechargeTime;
                }
            }

            public bool IsBlocked()
            {
                lock (this)
                {
                    if (automatic)
                    {
                        var t = blocked - Environment.TickCount;

                        if (maximum < MAXIMUM)
                        {
                            if (current == 0 && t < -60000)
                            {
                                //has been waiting 60s, reset
                                maximum = MAXIMUM;
                            }
                        }

                        return current >= maximum || t > 0;
                    }
                    else
                    {
                        return current == 0 && !Refresh();
                    }
                }
            }

            private void OnRelease(int time, int duration, bool success)
            {
                lock (this)
                {
                    if (!automatic)
                        return;

                    --current;

                    if (duration < 0 || (time - this.timestamp) < 0)
                        return;

                    this.timestamp = time;

                    OnSample(duration, success);
                }
            }

            public void OnSample(int duration, bool success)
            {
                //each login within a certain time frame will increase the time it takes to login
                //after the 3rd login, the time will roughly double as long as requests keep being made, which drops down after stopping
                //the launcher will time out if the login takes too long (60 seconds), or may receive an error instead if too many requests have been made
                //logins through the website are initially faster, but will see the same result

                if (duration < 5000)
                {
                    var blocked = Environment.TickCount;
                    var t = blocked - this.blocked;

                    if (success || t > 0)
                    {
                        this.blocked = blocked;
                    }

                    if (current == 0 && (success || t > 30000))
                    {
                        maximum = MAXIMUM;

                        if (t < 30000)
                        {
                            --maximum;
                        }
                    }
                }
                else if (duration < 10000)
                {
                    blocked = Environment.TickCount + 2000;
                }
                else
                {
                    blocked = Environment.TickCount + 10000;
                }
            }

            public void Sample(int duration)
            {
                if (!automatic)
                    return;

                lock(this)
                {
                    OnSample(duration, false);
                }
            }

            public ISession BeginSession()
            {
                if (!automatic)
                    return null;

                lock (this)
                {
                    if (current == 0 && maximum != MAXIMUM && Environment.TickCount - blocked > 30000)
                    {
                        maximum = MAXIMUM;
                    }

                    if (++current == maximum)
                    {
                        maximum = 1;
                    }

                    return new Session(this);
                }
            }

            public bool Reduce()
            {
                if (automatic)
                    return false;

                lock (this)
                {
                    if (current == maximum)
                    {
                        timestamp = Environment.TickCount;
                    }
                    else
                    {
                        if (!Refresh() && current == 0)
                        {
                            return false;
                        }
                    }

                    --current;

                    return true;
                }
            }

            public int GetDelay()
            {
                lock (this)
                {
                    if (automatic)
                    {
                        var d = blocked - Environment.TickCount;

                        if (d < 0)
                        {
                            if (current >= maximum)
                            {
                                return 1000;
                            }
                            else
                            {
                                return 0;
                            }
                        }
                        else
                        {
                            return d;
                        }
                    }
                    else
                    {
                        if (current == 0)
                        {
                            var ms = 1000 * rechargeTime - (Environment.TickCount - timestamp);
                            if (ms > 0)
                                return ms;
                        }

                        return 0;
                    }
                }
            }

            /// <summary>
            /// Waits until ready
            /// </summary>
            /// <returns>False if cancelled</returns>
            public bool Wait(CancellationToken cancel)
            {
                while (IsBlocked())
                {
                    var d = GetDelay();

                    if (d < 100)
                        d = 100;
                    else if (automatic && d > 1000)
                        d = 1000;

                    try
                    {
                        if (cancel.WaitHandle.WaitOne(d))
                            return false;
                    }
                    catch
                    {
                        return false;
                    }
                }

                return true;
            }

            private bool Refresh()
            {
                lock (this)
                {
                    var s = (Environment.TickCount - timestamp) / 1000f;
                    var e = (int)(s / rechargeTime);

                    if (e >= 1)
                    {
                        var i = current + e * recharge;

                        if (i > maximum)
                        {
                            current = maximum;
                            timestamp = Environment.TickCount;
                        }
                        else
                        {
                            current = (byte)i;
                            timestamp = Environment.TickCount + (int)((e * rechargeTime - s) * 1000);
                        }

                        return true;
                    }

                    return false;
                }
            }

            /// <summary>
            /// Applies settings; changing automatic is not supported
            /// </summary>
            /// <param name="o"></param>
            public void Apply(Settings.LaunchLimiterOptions o)
            {
                if (automatic)
                {
                    return;
                }

                lock (this)
                {
                    Refresh();

                    var maxed = current == maximum;

                    maximum = o.Count;
                    if (current > maximum)
                        current = maximum;
                    else if (maxed && current != maximum)
                        timestamp = Environment.TickCount;

                    recharge = o.RechargeCount;
                    rechargeTime = o.RechargeTime;
                }
            }

            public bool IsAutomatic
            {
                get
                {
                    return automatic;
                }
            }
        }

        /// <summary>
        /// Occurs when the account's state has changed
        /// </summary>
        public static event AccountEventHandler<AccountStateEventArgs> AccountStateChanged;
        /// <summary>
        /// Occurs when there's an error while launching
        /// </summary>
        public static event LaunchExceptionEventHandler LaunchException;
        /// <summary>
        /// Occurs when the account is launched
        /// </summary>
        public static event AccountEventHandler AccountLaunched;
        /// <summary>
        /// Occurs when the account has fully exited
        /// </summary>
        public static event AccountEventHandler AccountExited;
        /// <summary>
        /// Occurs when the process linked to the account changes
        /// </summary>
        public static event AccountEventHandler<Process> AccountProcessChanged;
        /// <summary>
        /// Occurs when a started process is linked to the account
        /// </summary>
        public static event AccountEventHandler<Process> AccountProcessActivated;
        /// <summary>
        /// Occurs when the account's process is exited
        /// </summary>
        public static event AccountEventHandler<Process> AccountProcessExited;
        /// <summary>
        /// Occurs when the queue is empty
        /// </summary>
        public static event EventHandler AllQueuedLaunchesComplete;
        /// <summary>
        /// Occurs when the number of active processes has changed
        /// </summary>
        public static event AccountTypeEventHandler<ushort> AnyActiveProcessCountChanged;
        /// <summary>
        /// Occurs when the number of active processes for the given type has changed
        /// </summary>
        public static event AccountTypeEventHandler<ushort> ActiveProcessCountChanged;
        /// <summary>
        /// Occurs when an update is requested
        /// </summary>
        public static event BuildUpdatedEventHandler BuildUpdated;
        /// <summary>
        /// Occurs when an account is queued
        /// </summary>
        public static event AccountEventHandler<LaunchMode> AccountQueued;
        /// <summary>
        /// Occurs after adding an account to the queue
        /// </summary>
        private static event InternalAccountEventHandler<LaunchMode> QueueAdded;
        /// <summary>
        /// Occurs when the account's window changes
        /// </summary>
        public static event AccountEventHandler<AccountWindowEventEventArgs> AccountWindowEvent;
        /// <summary>
        /// Occurs when the account's window is going to be set top most
        /// </summary>
        public static event AccountEventHandler<AccountTopMostWindowEventEventArgs> AccountTopMostWindowEvent;
        /// <summary>
        /// Occurs when the queue is emptied and all accounts are exited
        /// </summary>
        public static event EventHandler AllQueuedLaunchesCompleteAllAccountsExited;
        /// <summary>
        /// Occurs when the account's mumble link changes
        /// </summary>
        public static event AccountEventHandler<Tools.Mumble.MumbleMonitor.IMumbleProcess> MumbleLinkChanged;
        /// <summary>
        /// Occurs when the account's mumble link is verified (account is in-game on a character)
        /// </summary>
        public static event AccountEventHandler<Tools.Mumble.MumbleMonitor.IMumbleProcess> MumbleLinkVerified;
        /// <summary>
        /// Occurs when the account's run after is initialized
        /// </summary>
        public static event AccountEventHandler RunAfterChanged;

        public enum AccountState
        {
            None,
            Waiting,
            Active,
            ActiveGame,
            Launching,
            Updating,
            UpdatingVisible,
            WaitingForOtherProcessToExit,
            Error,
            Exited,
            WaitingForAuthentication
        }

        [Flags]
        public enum AccountType
        {
            None = 0,

            GuildWars2 = 1,
            GuildWars1 = 2,
            Unknown = 4,

            Any = 3,
        }

        public enum LaunchMode
        {
            /// <summary>
            /// The process will be launched with all arguments
            /// </summary>
            Launch,
            /// <summary>
            /// The process will be launched without -sharedArchive
            /// </summary>
            LaunchSingle,
            /// <summary>
            /// The process will launch with only -image and -nopatchui
            /// </summary>
            Update,
            /// <summary>
            /// The process will launch with only -image
            /// </summary>
            UpdateVisible
        }

        private enum ScanOptions
        {
            None,
            KillLinked,
            KillAll
        }

        public class NetworkChangedException : Exception
        {
            public NetworkChangedException()
                : base("Network was not allowed; launching was aborted")
            { }
        }

        public class Gw2LockException : Exception
        {
            public Gw2LockException()
                : base("Lock.dat is in use; exclusive access is required")
            { }
        }
        
        public class InvalidSteamPathException : Exception
        {
            public InvalidSteamPathException(AccountType type, string message)
                : base(message)
            {
                this.Type = type;
            }

            public AccountType Type
            {
                get;
                private set;
            }
        }

        public class InvalidPathException : Exception
        {
            public InvalidPathException(AccountType type, string message)
                : base(message)
            {
                this.Type = type;
            }

            public AccountType Type
            {
                get;
                private set;
            }

            public static InvalidPathException From(AccountType type)
            {
                string message;

                switch (type)
                {
                    case AccountType.GuildWars2:
                        message = "The location of Gw2.exe is invalid";
                        break;
                    case AccountType.GuildWars1:
                        message = "The location of Gw.exe is invalid";
                        break;
                    default:
                        message = "Invalid path location";
                        break;
                }

                return new InvalidPathException(type, message);
            }
        }

        public class BadUsernameOrPasswordException : Exception
        {
            public BadUsernameOrPasswordException()
                : base("The username or password is invalid")
            {

            }
        }

        public class UserAlreadyActiveException : Exception
        {
            public UserAlreadyActiveException(string username)
                : base("The user \"" + username + "\" is already in use. Only accounts sharing the same Local.dat file can be active on a single user at the same time")
            {

            }
        }

        public class DatFileNotInitialized : Exception
        {
            public DatFileNotInitialized()
                : base("Local.dat has not been initialized")
            {

            }
        }

        public class DatFileUpdateRequiredException : Exception
        {
            public DatFileUpdateRequiredException()
                : base("An update is required; Local.dat is out of date")
            {

            }
        }

        public class BuildUpdatedEventArgs : EventArgs
        {
            private Queue<Settings.IAccount> queue;

            public BuildUpdatedEventArgs()
            {
            }

            /// <summary>
            /// The accounts that will be updated next, or nothing to ignore
            /// </summary>
            public Queue<Settings.IAccount> Queue
            {
                get
                {
                    return this.queue;
                }
            }

            /// <summary>
            /// The specified accounts will be updated next
            /// </summary>
            /// <param name="accounts"></param>
            public void Update(IEnumerable<Settings.IAccount> accounts)
            {
                if (this.queue == null)
                {
                    this.queue = new Queue<Settings.IAccount>(accounts);
                }
                else
                {
                    foreach (var account in accounts)
                        this.queue.Enqueue(account);
                }
            }
        }

        public class LaunchExceptionEventArgs : EventArgs
        {
            public LaunchExceptionEventArgs(Exception e)
            {
                this.Exception = e;
            }

            public Exception Exception
            {
                get;
                private set;
            }

            public bool Retry
            {
                get;
                set;
            }
        }

        private class QueuedLaunch
        {
            public event EventHandler<DequeuedState> Dequeued;

            public enum DequeuedState
            {
                Skipped,
                OK
            }

            public QueuedLaunch(Account account, LaunchMode mode, string args)
            {
                this.account = account;
                this.mode = mode;
                this.args = args;
            }

            public QueuedLaunch(Account account, LaunchMode mode)
                : this(account, mode, null)
            {
            }

            public Account account;
            public LaunchMode mode;
            public string args;
            public bool disableProxy;

            public void OnDequeued(DequeuedState state)
            {
                if (Dequeued != null)
                {
                    Dequeued(this, state);
                    Dequeued = null;
                }
            }

            public AccountType Type
            {
                get
                {
                    if (account == null) //this should apply to all account types, but only gw2 is using this to update all accounts
                        return AccountType.GuildWars2;
                    return account.Type;
                }
            }
        }

        private class QueuedAnnounce
        {
            public QueuedAnnounce(Settings.IAccount account, AccountState state, AccountState previousState, object data)
            {
                this.account = account;
                this.state = state;
                this.previousState = previousState;
                this.data = data;
            }

            public Settings.IAccount account;
            public AccountState state;
            public AccountState previousState;
            public object data;
        }

        private class QueuedExit
        {
            public QueuedExit(Account account, DateTime exitTime, Process process)
            {
                this.account = account;
                this.exitTime = exitTime;
                this.process = process;
            }

            public Account account;
            public DateTime exitTime;
            public Process process;
        }

        public class ProcessOptions
        {
            public const string VAR_USERPOFILE = "USERPROFILE",
                                VAR_APPDATA = "APPDATA",
                                VAR_TEMP = "TMP",
                                VAR_STEAM_ID = "SteamAppId",
                                VAR_STEAM_ID_VALUE_GW2 = "1284210";

            public ProcessOptions()
            {
                Variables = new Dictionary<string, string>(5, StringComparer.OrdinalIgnoreCase);
            }

            public string FileName
            {
                get;
                set;
            }

            public string WorkingDirectory
            {
                get;
                set;
            }

            public string Arguments
            {
                get;
                set;
            }

            public string UserName
            {
                get;
                set;
            }

            public System.Security.SecureString Password
            {
                get;
                set;
            }

            public Dictionary<string, string> Variables
            {
                get;
                private set;
            }

            public ProcessStartInfo ToProcessStartInfo()
            {
                var startInfo = new ProcessStartInfo(this.FileName, this.Arguments);
                startInfo.UseShellExecute = false;

                if (this.WorkingDirectory != null)
                    startInfo.WorkingDirectory = this.WorkingDirectory;

                if (!string.IsNullOrEmpty(this.UserName))
                {
                    startInfo.UserName = this.UserName;
                    startInfo.Password = this.Password;
                    startInfo.LoadUserProfile = true;
                }

                foreach (var key in this.Variables.Keys)
                {
                    var value = this.Variables[key];
                    if (!string.IsNullOrEmpty(value))
                        startInfo.EnvironmentVariables[key] = value;
                }

                return startInfo;
            }
        }

        private class UnknownProcess : IDisposable
        {
            public UnknownProcess(AccountType type, Process p)
            {
                this.Type = type;
                this.Process = p;
            }

            public Process Process
            {
                get;
                private set;
            }

            public AccountType Type
            {
                get;
                private set;
            }

            public void Dispose()
            {
                if (Process != null)
                    Process.Dispose();
            }
        }

        private const string ARGS_UID = "-l:id:";

        private static Dictionary<ushort, Account> accounts;
        private static NodeQueue queueLaunch;
        private static Queue<QueuedLaunch> queue;
        private static Queue<QueuedAnnounce> queueAnnounce;
        private static Queue<QueuedExit> queueExit;
        private static Dictionary<int, UnknownProcess> unknownProcesses;
        private static HashSet<string> activeUsers;
        private static DateTime lastExit;
        private static Task taskQueue;
        private static Task taskAnnounce;
        private static Task taskScan;
        private static Task taskWatchUnknowns;
        private static ushort[] activeProcesses;
        private static CancellationTokenSource cancelQueue;
        private static bool aborting;
        private static Tools.ProcessPriority processPriority;
        private static Tools.CoherentMonitor coherentMonitor;
        private static WindowEvents windowEvents;
        private static Autologin autologin;
        private static Stream coherentLock;
        private static WindowLock wlocker;
        private static Limiter limiter;
        private static Tools.Mumble.MumbleMonitor mumble;

        static Launcher()
        {
            accounts = new Dictionary<ushort, Account>();
            queue = new Queue<QueuedLaunch>();
            queueLaunch = new NodeQueue();
            queueAnnounce = new Queue<QueuedAnnounce>();
            queueExit = new Queue<QueuedExit>();
            activeProcesses = new ushort[2];
            unknownProcesses = new Dictionary<int, UnknownProcess>();
            activeUsers = new HashSet<string>();
            if (Settings.LaunchLimiter.HasValue)
                limiter = new Limiter(Settings.LaunchLimiter.Value);
            if (Settings.LaunchBehindOtherAccounts.Value)
                wlocker = new WindowLock();

            windowEvents = new WindowEvents();
            autologin = new Autologin();
            mumble = new Tools.Mumble.MumbleMonitor();

            autologin.LoginEntered += Autologin_LoginEntered;
            LinkedProcess.ProcessExited += LinkedProcess_ProcessExited;
            LinkedProcess.ProcessActive += LinkedProcess_ProcessActive;

            Settings.GuildWars2.PreventDefaultCoherentUI.ValueChanged += PreventDefaultCoherentUI_ValueChanged;
            Settings.LaunchLimiter.ValueChanged += LaunchLimiter_ValueChanged;
            Settings.LaunchBehindOtherAccounts.ValueChanged += LaunchBehindOtherAccounts_ValueChanged;
        }

        static void Autologin_LoginEntered(object sender, Launcher.Account e)
        {
            var l = e.Session.Limiter;
            if (l != null)
            {
                l.SetTime();
            }
        }

        static void LaunchBehindOtherAccounts_ValueChanged(object sender, EventArgs e)
        {
            var v = ((Settings.ISettingValue<bool>)sender).Value;

            if (v)
            {
                if (wlocker == null)
                {
                    wlocker = new WindowLock();
                }
            }
            else
            {
                wlocker = null;
            }
        }

        static void LaunchLimiter_ValueChanged(object sender, EventArgs e)
        {
            var v = ((Settings.ISettingValue<Settings.LaunchLimiterOptions>)sender).Value;

            if (v == null)
            {
                limiter = null;
            }
            else
            {
                if (limiter == null || limiter.IsAutomatic != v.IsAutomatic)
                    limiter = new Limiter(v);
                else
                    limiter.Apply(v);
            }
        }

        static void PreventDefaultCoherentUI_ValueChanged(object sender, EventArgs e)
        {
            var v = ((Settings.ISettingValue<bool>)sender).Value;

            if (!v)
            {
                lock (queueLaunch)
                {
                    if (coherentLock != null)
                    {
                        coherentLock.Dispose();
                        coherentLock = null;
                    }
                }
            }
        }

        public static AccountState GetState(ushort uid)
        {
            lock (accounts)
            {
                Account a;
                if (accounts.TryGetValue(uid, out a))
                    return a.State;
            }

            return AccountState.None;
        }

        public static bool IsActive(Settings.IAccount account)
        {
            return GetAccount(account).IsActive;
        }

        public static AccountState GetState(Settings.IAccount account)
        {
            return GetAccount(account).State;
        }
        
        private static Account GetAccount(Settings.IAccount account)
        {
            Account _account;
            lock (accounts)
            {
                if (!accounts.TryGetValue(account.UID, out _account))
                {
                    _account = new Account(account);
                    _account.Process.Changed += LinkedProcess_Changed;
                    accounts.Add(account.UID, _account);
                }
            }
            return _account;
        }

        private static int GetIndex(AccountType type)
        {
            switch (type)
            {
                case AccountType.GuildWars1:
                    return 0;
                case AccountType.GuildWars2:
                    return 1;
                //case AccountType.Unknown:
                //    return 2;
            }

            return -1;
        }
        
        public static bool ApplyWindowedBounds(Settings.IAccount account)
        {
            if (account.Windowed && !account.WindowBounds.IsEmpty)
            {
                return ApplyWindowedBounds(account, account.WindowBounds);
            }

            return false;
        }

        public static bool ApplyWindowedBounds(Settings.IAccount account, System.Drawing.Rectangle bounds)
        {
            try
            {
                var a = GetAccount(account);
                var p = a.Process.Process;
                if (p != null && !p.HasExited)
                {
                    var h = WindowWatcher.FindDxWindow(p);
                    if (h != IntPtr.Zero && WindowWatcher.SetBounds(account, p, h, bounds))
                    {
                        OnWindowStateChanged(h, a);
                        if ((a.WindowOptions & Settings.WindowOptions.TopMost) != 0)
                            DelayedWindowStateChangedTopMost(100, h, a);
                        return true;
                    }
                }
            }
            catch (Exception e)
            {
                Util.Logging.Log(e);
            }

            return false;
        }

        /// <summary>
        /// Applies window templates to all active accounts
        /// </summary>
        /// <param name="refresh">True to include accounts that already have a valid template</param>
        public static void ApplyWindowedTemplate(bool refresh)
        {
            foreach (var l in LinkedProcess.GetActive())
            {
                if (IsWindowed(l.Account.Settings))
                {
                    if (!refresh)
                    {
                        var s = l.Account.Session;
                        if (s != null)
                        {
                            var t = s.WindowTemplate;
                            if (t != null && t.IsValid)
                                continue;
                        }
                    }
                    ApplyWindowedTemplate(l.Account.Settings);
                }
            }
        }

        public static bool ApplyWindowedTemplate(Settings.IAccount account)
        {
            try
            {
                var a = GetAccount(account);
                var p = a.Process.Process;

                if (p != null && !p.HasExited && a.Session != null)
                {
                    Tools.WindowManager.IWindowBounds wb;
                    if (Tools.WindowManager.Instance.TryGetBounds(account, out wb) == Tools.WindowManager.BoundsResult.Success)
                    {
                        var s = a.Session;
                        var wo = a.WindowOptions;

                        if (s != null)
                        {
                            lock (s)
                            {
                                if (s.IsDisposed)
                                {
                                    wb.Dispose();
                                    wb = null;
                                }
                                else
                                {
                                    s.WindowTemplate = wb;
                                }
                            }
                        }
                        else
                        {
                            wb.Dispose();
                            wb = null;
                        }

                        if (wb != null)
                        {
                            var handle = WindowWatcher.FindDxWindow(p);

                            if (handle != IntPtr.Zero)
                            {
                                if ((wo & Settings.WindowOptions.TopMost) != 0 && (wb.Options & Settings.WindowOptions.TopMost) == 0)
                                {
                                    if (Windows.WindowLong.HasValue(handle, GWL.GWL_EXSTYLE, WindowStyle.WS_EX_TOPMOST))
                                    {
                                        NativeMethods.SetWindowPos(handle, (IntPtr)WindowZOrder.HWND_NOTOPMOST, 0, 0, 0, 0, SetWindowPosFlags.SWP_NOMOVE | SetWindowPosFlags.SWP_NOACTIVATE | SetWindowPosFlags.SWP_NOSIZE);
                                    }
                                }

                                if ((wo & Settings.WindowOptions.PreventChanges) != 0 && (wb.Options & Settings.WindowOptions.PreventChanges) == 0)
                                {
                                    if (Windows.WindowLong.HasValue(handle, GWL.GWL_STYLE, WindowStyle.WS_MINIMIZEBOX))
                                    {
                                        Windows.WindowLong.Add(handle, GWL.GWL_STYLE, WindowStyle.WS_MAXIMIZEBOX);
                                    }
                                }

                                if (WindowWatcher.SetBounds(account, p, handle, wb.Bounds) || (wb.Options & ~Settings.WindowOptions.Windowed) != 0)
                                {
                                    OnWindowStateChanged(handle, a);

                                    if ((wb.Options & Settings.WindowOptions.TopMost) != 0)
                                        DelayedWindowStateChangedTopMost(100, handle, a);
                                }

                                if (AccountWindowEvent != null)
                                {
                                    if (a.WindowOptions != wo)
                                        AccountWindowEvent(account, new AccountWindowEventEventArgs(AccountWindowEventEventArgs.EventType.WindowOptionsChanged, p, handle));
                                    AccountWindowEvent(account, new AccountWindowEventEventArgs(AccountWindowEventEventArgs.EventType.TemplateChanged, p, handle));
                                }

                                return true;
                            }
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Util.Logging.Log(e);
            }

            return false;
        }

        public static Settings.WindowOptions GetWindowOptions(Settings.IAccount account)
        {
            return GetAccount(account).WindowOptions;
        }

        public static bool SetWindowOptions(Settings.IAccount account, Settings.WindowOptions options)
        {
            try
            {
                var a = GetAccount(account);
                var p = a.Process.Process;
                var s = a.Session;

                if (p != null && !p.HasExited && s != null)
                {
                    var wo = a.WindowOptions;
                    var handle = WindowWatcher.FindDxWindow(p);
                    s.WindowOptions = options | Settings.WindowOptions.Windowed;

                    if (handle != IntPtr.Zero)
                    {
                        if ((wo & Settings.WindowOptions.TopMost) != 0 && (options & Settings.WindowOptions.TopMost) == 0)
                        {
                            if (Windows.WindowLong.HasValue(handle, GWL.GWL_EXSTYLE, WindowStyle.WS_EX_TOPMOST))
                            {
                                NativeMethods.SetWindowPos(handle, (IntPtr)WindowZOrder.HWND_NOTOPMOST, 0, 0, 0, 0, SetWindowPosFlags.SWP_NOMOVE | SetWindowPosFlags.SWP_NOACTIVATE | SetWindowPosFlags.SWP_NOSIZE);
                            }
                        }

                        if ((wo & Settings.WindowOptions.PreventChanges) != 0 && (options & Settings.WindowOptions.PreventChanges) == 0)
                        {
                            if (Windows.WindowLong.HasValue(handle, GWL.GWL_STYLE, WindowStyle.WS_MINIMIZEBOX))
                            {
                                Windows.WindowLong.Add(handle, GWL.GWL_STYLE, WindowStyle.WS_MAXIMIZEBOX);
                            }
                        }

                        if ((options & ~Settings.WindowOptions.Windowed) != 0)
                        {
                            OnWindowStateChanged(handle, a);
                        }

                        if (AccountWindowEvent != null && a.WindowOptions != wo)
                            AccountWindowEvent(account, new AccountWindowEventEventArgs(AccountWindowEventEventArgs.EventType.WindowOptionsChanged, p, handle));

                        return true;
                    }
                }
            }
            catch (Exception e)
            {
                Util.Logging.Log(e);
            }

            return false;
        }

        public static Tools.WindowManager.IWindowBounds GetWindowTemplate(Settings.IAccount account)
        {
            return GetAccount(account).WindowTemplate;
        }

        public static Tools.Mumble.MumbleMonitor.IMumbleProcess GetMumbleLink(Settings.IAccount account)
        {
            return GetAccount(account).MumbleLink;
        }

        public static bool IsMumbleLinkInvalid(Settings.IAccount account)
        {
            var m = GetAccount(account).MumbleLink;

            return m != null && !m.IsValid;
        }

        public static Process GetProcess(Settings.IAccount account)
        {
            lock (accounts)
            {
                Account _account;
                if (accounts.TryGetValue(account.UID, out _account))
                {
                    return _account.Process.Process;
                }
            }
            return null;
        }

        public static bool Kill(Settings.IAccount account)
        {
            Process p;
            lock (accounts)
            {
                var _account = GetAccount(account);
                p = _account.Process.Process;
            }

            if (p != null)
            {
                try
                {
                    p.Kill();
                    return true;
                }
                catch { }
            }

            return false;
        }

        public static void Update(IList<string> files)
        {
            lock (queue)
            {
                aborting = false;

                var first = true;
                var mode = LaunchMode.UpdateVisible;

                foreach (var f in files)
                {
                    var account = new Account(Settings.CreateVoidAccount(Settings.AccountType.GuildWars2));
                    var gw2 = (Settings.IGw2Account)account.Settings;
                    gw2.Name = Path.GetFileName(f);
                    gw2.DatFile = Settings.CreateVoidDatFile();
                    gw2.DatFile.Path = f;
                    
                    var q = new QueuedLaunch(account, mode, f);
                    queue.Enqueue(q);

                    if (QueueAdded != null)
                    {
                        QueueAdded(account, mode);
                    }

                    if (first)
                    {
                        first = false;
                        mode = LaunchMode.Update;
                    }
                }

                StartQueue();
            }
        }

        public static void Launch(Settings.IAccount account, LaunchMode mode)
        {
            Launch(account, mode, null);
        }

        public static void Launch(Settings.IAccount account, LaunchMode mode, string args)
        {
            aborting = false;

            Account _account;
            lock(accounts)
            {
                _account = GetAccount(account);

                if (_account.State != AccountState.None)
                    return;

                if (Monitor.TryEnter(queueLaunch, TIMEOUT_TRYENTER))
                {
                    try
                    {
                        if (_account.inQueueCount > 0)
                            return;
                        else
                            _account.inQueueCount++;
                    }
                    finally
                    {
                        Monitor.Exit(queueLaunch);
                    }
                }
                else
                {
                    return;
                }

                _account.errors = 0;
            }

            AddQueuedLaunch(_account, mode, args);
        }

        private static void AddQueuedLaunch(Account account, LaunchMode mode, string args)
        {
            lock (account)
            {
                account.SetState(AccountState.Waiting, true);
            }

            lock (queue)
            {
                queue.Enqueue(new QueuedLaunch(account, mode, args));

                if (QueueAdded != null)
                {
                    QueueAdded(account, mode);
                }

                StartQueue();
            }
        }

        private static void StartQueue()
        {
            lock (queue)
            {
                if (taskQueue == null || taskQueue.IsCompleted)
                {
                    if (cancelQueue == null || cancelQueue.IsCancellationRequested)
                    {
                        if (cancelQueue != null)
                            cancelQueue.Dispose();
                        cancelQueue = new CancellationTokenSource();
                    }
                    var cancel = cancelQueue.Token;

                    taskQueue = new Task(
                        delegate
                        {
                            DoQueue(cancel);
                        }, cancel, TaskCreationOptions.LongRunning);
                    taskQueue.Start();
                }
            }
        }

        private static bool WaitOnActiveProcesses(AccountType type, CancellationToken cancel)
        {
            return WaitOnActiveProcesses(type, cancel, 0, false);
        }

        /// <summary>
        /// Waits until the specified type of process exit
        /// </summary>
        /// <param name="maxCount">Only wait if > this</param>
        /// <returns>True to continue waiting, False if no active processes were found</returns>
        private static bool WaitOnActiveProcesses(AccountType type, CancellationToken cancel, int maxCount, bool interruptOnQueueChange)
        {
            bool waiting;

            lock (unknownProcesses)
            {
                waiting = GetActiveProcessCount(type) > maxCount;
            }

            if (waiting)
            {
                using (var waiter = new ManualResetEvent(false))
                {
                    AccountTypeEventHandler<ushort> onCountChanged = delegate(AccountType t, ushort count)
                    {
                        if (count <= maxCount)
                        {
                            waiter.Set();
                        }
                    };

                    InternalAccountEventHandler<LaunchMode> onAdded = delegate(Account a, LaunchMode m)
                    {
                        if (interruptOnQueueChange && !type.HasFlag(a.Type) && !IsQueueWaiting(a.Type, m, out a))
                        {
                            waiter.Set();
                        }
                    };

                    if (type == AccountType.Any)
                        AnyActiveProcessCountChanged += onCountChanged;
                    else
                        ActiveProcessCountChanged += onCountChanged;
                    QueueAdded += onAdded;

                    try
                    {
                        using (cancel.Register(
                            delegate
                            {
                                waiter.Set();
                            }))
                        {

                            if (GetActiveProcessCount(type) > maxCount)
                            {
                                while (!waiter.WaitOne())
                                {

                                }
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Util.Logging.Log(ex);
                    }
                    finally
                    {
                        if (type == AccountType.Any)
                            AnyActiveProcessCountChanged -= onCountChanged;
                        else
                            ActiveProcessCountChanged -= onCountChanged;
                        QueueAdded -= onAdded;
                    }
                }
            }

            return waiting;
        }

        private static bool WaitOnSteam(AccountType type, CancellationToken cancel, bool interruptOnQueueChange)
        {
            Account account;
            var waiting = IsSteamActive(type, out account);

            if (waiting)
            {
                var changed = false;

                using (var waiter = new ManualResetEvent(false))
                {
                    using (cancel.Register(
                        delegate
                        {
                            waiter.Set();
                        }))
                    {
                        InternalAccountEventHandler<LaunchMode> onAdded = delegate(Account a, LaunchMode m)
                        {
                            if (!type.HasFlag(a.Type) || a.Settings.Proxy != Settings.LaunchProxy.Steam)
                            {
                                changed = true;
                                waiter.Set();
                            }
                        };

                        if (interruptOnQueueChange)
                            QueueAdded += onAdded;

                        try
                        {
                            while (waiting)
                            {
                                if (account != null)
                                {
                                    //the account is known, wait for it to exit

                                    EventHandler<Account> onExit = null;
                                    var exited = false;

                                    lock (queueExit)
                                    {
                                        if (account.IsActive)
                                        {
                                            onExit = delegate(object o, Account a)
                                            {
                                                exited = true;
                                                waiter.Set();
                                            };
                                            account.Exited += onExit;
                                        }
                                    }

                                    if (onExit != null)
                                    {
                                        try
                                        {
                                            waiter.WaitOne();

                                            if (changed || cancel.IsCancellationRequested)
                                            {
                                                return true;
                                            }
                                        }
                                        finally
                                        {
                                            account.Exited -= onExit;
                                        }

                                        waiter.Reset();
                                    }
                                    
                                    waiting = IsSteamActive(type, out account);
                                }
                                else
                                {
                                    //the account is unknown, waiting for steam to change/exit

                                    using (var steam = Steam.GetSteamProcess())
                                    {
                                        if (steam == null)
                                        {
                                            waiting = false;
                                        }
                                        else
                                        {
                                            while (!waiter.WaitOne(1000))
                                            {
                                                try
                                                {
                                                    if (type == AccountType.GuildWars2)
                                                    {
                                                        if (!Steam.IsRunning(Steam.APPID_GW2))
                                                        {
                                                            break;
                                                        }
                                                    }
                                                    else
                                                    {
                                                        throw new NotSupportedException();
                                                    }

                                                    if (steam.HasExited)
                                                    {
                                                        break;
                                                    }
                                                }
                                                catch { }
                                            }

                                            if (changed || cancel.IsCancellationRequested)
                                            {
                                                return true;
                                            }

                                            waiting = IsSteamActive(type, out account);
                                        }
                                    }
                                }
                            }
                        }
                        finally
                        {
                            if (interruptOnQueueChange)
                                QueueAdded -= onAdded;
                        }
                    }
                }
            }

            return waiting;
        }

        /// <summary>
        /// Returns if the queue is having to wait due to a normal mode launch
        /// </summary>
        /// <param name="type">Type of account</param>
        /// <param name="mode">The mode wanting to be launched</param>
        /// <param name="active">The account being waited on</param>
        private static bool IsQueueWaiting(AccountType type, LaunchMode mode, out Account active)
        {
            var count = GetActiveProcessCount(type);
            
            if (count > 0)
            {
                lock (accounts)
                {
                    foreach (var a in accounts.Values)
                    {
                        if (type.HasFlag(a.Type) && a.IsActive)
                        {
                            var s = a.Session;
                            if (s == null || s.Mode != LaunchMode.Launch || mode != LaunchMode.Launch)
                            {
                                active = a;
                                return true;
                            }
                            else
                            {
                                break;
                            }
                        }
                    }
                }
            }

            active = null;
            return false;
        }

        /// <summary>
        /// Returns if the specified type of launch would need to wait due to an active account
        /// </summary>
        private static bool IsQueueWaiting(AccountType type, LaunchMode mode, QueuedLaunch last = null)
        {
            if (last != null && last.account != null && last.account.IsActive)
            {
                return last.mode != LaunchMode.Launch || mode != LaunchMode.Launch;
            }
            
            var count = GetActiveProcessCount(type);

            if (count == 1)
            {
                foreach (var p in LinkedProcess.GetActiveEnumerable())
                {
                    var a = p.Account;

                    if (type.HasFlag(a.Type) && a.IsActive)
                    {
                        var s = a.Session;
                        if (s == null || s.Mode != LaunchMode.Launch || mode != LaunchMode.Launch)
                        {
                            return true;
                        }
                        else
                        {
                            break;
                        }
                    }
                }
                //lock (accounts)
                //{
                //    foreach (var a in accounts.Values)
                //    {
                //        if (type.HasFlag(a.Type) && a.IsActive)
                //        {
                //            if (a.Mode != LaunchMode.Launch || mode != LaunchMode.Launch)
                //            {
                //                return true;
                //            }
                //            else
                //            {
                //                break;
                //            }
                //        }
                //    }
                //}
            }
            else if (count > 1)
            {
                return mode != LaunchMode.Launch;
            }

            return false;
        }

        private static bool IsQueueActive(AccountType type, out LaunchMode mode)
        {
            lock (accounts)
            {
                foreach (var a in accounts.Values)
                {
                    if (type.HasFlag(a.Type) && a.IsActive)
                    {
                        var s = a.Session;
                        if (s != null)
                        {
                            mode = s.Mode;
                        }
                        else
                        {
                            mode = LaunchMode.Launch;
                        }
                        return true;
                    }
                }
            }

            mode = (LaunchMode)(-1);
            return false;
        }

        /// <summary>
        /// Returns if an account is currently using Steam
        /// </summary>
        /// <param name="account">The account using Steam or null if Steam is being used, but the account is unknown</param>
        /// <returns>True if Steam is being used</returns>
        private static bool IsSteamActive(AccountType type, out Account account)
        {
            lock (accounts)
            {
                foreach (var a in accounts.Values)
                {
                    if (type.HasFlag(a.Type) && a.IsActive)
                    {
                        var s = a.Session;
                        if (s != null)
                        {
                            if (s.Proxy == Settings.LaunchProxy.Steam)
                            {
                                account = a;
                                return true;
                            }
                        }
                    }
                }
            }

            account = null;

            if (type == AccountType.GuildWars2)
            {
                if (Steam.IsRunning(Steam.APPID_GW2))
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Waits if the queue is paused (due to conflicting launch modes)
        /// </summary>
        /// <param name="type"></param>
        /// <param name="cancel"></param>
        /// <returns>True to continue waiting, False to resume</returns>
        private static bool WaitOnQueue(AccountType type, LaunchMode mode, CancellationToken cancel)
        {
            if (!IsQueueWaiting(type, mode))
                return false;

            using (var waiter = new ManualResetEvent(false))
            {
                AccountTypeEventHandler<ushort> onCountChanged = delegate(AccountType t, ushort count)
                {
                    waiter.Set();
                };

                InternalAccountEventHandler<LaunchMode> onAdded = delegate(Account a, LaunchMode m)
                {
                    if (!type.HasFlag(a.Type) && !IsQueueWaiting(a.Type, m, out a))
                    {
                        waiter.Set();
                    }
                };

                AnyActiveProcessCountChanged += onCountChanged;
                QueueAdded += onAdded;

                try
                {
                    using (cancel.Register(
                        delegate
                        {
                            waiter.Set();
                        }))
                    {
                        while (!waiter.WaitOne())
                        {

                        }
                    }
                }
                catch (Exception ex)
                {
                    Util.Logging.Log(ex);
                }
                finally
                {
                    AnyActiveProcessCountChanged -= onCountChanged;
                    QueueAdded -= onAdded;
                }
            }

            return true;
        }

        private static Task WaitForScannerTask(bool runOnce = true)
        {
            Task t;

            lock (queueExit)
            {
                t = taskScan;
                if (runOnce && (t == null || t.IsCompleted))
                {
                    t = taskScan = new Task(DoScan);
                    t.Start();
                }
            }

            return t;
        }

        private static void WaitForScanner(bool runOnce = true)
        {
            try
            {
                var t = WaitForScannerTask(runOnce);
                if (t != null)
                    t.Wait();
            }
            catch (Exception e)
            {
                Util.Logging.Log(e);
            }
        }

        /// <summary>
        /// Returns true if or when the account exits or false if cancelled
        /// </summary>
        private static bool WaitForExit(Account account, CancellationToken cancel)
        {
            ManualResetEvent waitOnExit;
            EventHandler<Account> onExit = null;

            lock (queueExit)
            {
                if (account.IsActive)
                {
                    waitOnExit = new ManualResetEvent(false);
                    onExit = delegate(object o, Account a)
                    {
                        account.Exited -= onExit;
                        waitOnExit.Set();
                    };
                    account.Exited += onExit;
                }
                else
                    waitOnExit = null;
            }

            if (waitOnExit != null)
            {
                using (waitOnExit)
                {
                    using (cancel.Register(
                        delegate
                        {
                            account.Exited -= onExit;
                            waitOnExit.Set();
                        }))
                    {
                        waitOnExit.WaitOne();
                    }
                }
            }

            return !cancel.IsCancellationRequested;
        }

        private static void DoAnnounce()
        {
            Thread.Sleep(100);

            do
            {
                QueuedAnnounce q;

                lock (queueAnnounce)
                {
                    if (queueAnnounce.Count > 0)
                    {
                        q = queueAnnounce.Dequeue();
                    }
                    else
                    {
                        taskAnnounce = null;
                        return;
                    }
                }

                try
                {
                    AccountStateChanged(q.account, new AccountStateEventArgs(q.account, q.state, q.previousState, q.data));
                }
                catch (Exception e)
                {
                    Util.Logging.Log(e);
                }
            }
            while (true);
        }

        private static void DoQueue(CancellationToken cancel)
        {
            Thread.Sleep(100);

            int build = 0;
            var lastMode = (LaunchMode)(-1);
            var lastLaunchTime = 0;
            var _lastLaunch = new QueuedLaunch[2];
            Tools.DatUpdater datUpdater = null;

            do
            {
                QueuedLaunch q;

                Monitor.Enter(queueLaunch);
                try
                {
                    #region Add queued items

                    if (Monitor.TryEnter(Launcher.queue, TIMEOUT_TRYENTER))
                    {
                        try
                        {
                            while (Launcher.queue.Count > 0)
                            {
                                var _q = Launcher.queue.Dequeue();
                                queueLaunch.Enqueue(_q);
                                lock (_q.account)
                                {
                                    if (_q.account.State == AccountState.None)
                                        _q.account.SetState(AccountState.Waiting, true);
                                }
                                if (AccountQueued != null)
                                {
                                    Monitor.Exit(Launcher.queue);
                                    try
                                    {
                                        AccountQueued(_q.account.Settings, _q.mode);
                                    }
                                    catch (Exception e)
                                    {
                                        Util.Logging.Log(e);
                                    }
                                    Monitor.Enter(Launcher.queue);
                                }
                            }
                        }
                        finally
                        {
                            Monitor.Exit(Launcher.queue);
                        }
                    }

                    #endregion

                    #region Abort, dump queues

                    if (aborting || cancel.IsCancellationRequested)
                    {
                        while (queueLaunch.Count > 0)
                        {
                            var _q = queueLaunch.Dequeue();

                            _q.OnDequeued(QueuedLaunch.DequeuedState.Skipped);

                            if (_q.account == null)
                                continue;

                            _q.account.inQueueCount--;

                            lock (_q.account)
                            {
                                if (_q.account.State == AccountState.Waiting || _q.account.State == AccountState.WaitingForOtherProcessToExit)
                                    _q.account.SetState(AccountState.None, true);
                            }
                        }

                        if (Monitor.TryEnter(Launcher.queue, TIMEOUT_TRYENTER))
                        {
                            try
                            {
                                if (Launcher.queue.Count > 0)
                                    continue;
                                taskQueue = null;
                            }
                            finally
                            {
                                Monitor.Exit(Launcher.queue);
                            }

                            if (AllQueuedLaunchesComplete != null)
                            {
                                Monitor.Exit(queueLaunch);

                                try
                                {
                                    AllQueuedLaunchesComplete(null, EventArgs.Empty);
                                }
                                catch (Exception e)
                                {
                                    Util.Logging.Log(e);
                                }
                            }

                            if (AllQueuedLaunchesCompleteAllAccountsExited != null && GetActiveProcessCount() == 0)
                                AllQueuedLaunchesCompleteAllAccountsExited(null, EventArgs.Empty);

                            return;
                        }
                        else
                        {
                            Thread.Sleep(100);
                            continue;
                        }
                    }

                    #endregion

                    #region Queues empty

                    if (queueLaunch.Count == 0)
                    {
                        if (Monitor.TryEnter(Launcher.queue, TIMEOUT_TRYENTER))
                        {
                            try
                            {
                                if (Launcher.queue.Count > 0)
                                    continue;
                                taskQueue = null;
                            }
                            finally
                            {
                                Monitor.Exit(Launcher.queue);
                            }

                            Monitor.Exit(queueLaunch);

                            if (build > 0 && Settings.CheckForNewBuilds.Value)
                                Settings.LastKnownBuild.Value = build;

                            if (AllQueuedLaunchesComplete != null)
                            {
                                try
                                {
                                    AllQueuedLaunchesComplete(null, EventArgs.Empty);
                                }
                                catch (Exception e)
                                {
                                    Util.Logging.Log(e);
                                }
                            }

                            if (AllQueuedLaunchesCompleteAllAccountsExited != null && GetActiveProcessCount() == 0)
                                AllQueuedLaunchesCompleteAllAccountsExited(null, EventArgs.Empty);

                            return;
                        }
                        else
                        {
                            Thread.Sleep(100);
                            continue;
                        }
                    }

                    #endregion

                    q = queueLaunch.Peek();

                    #region Check other account types if this one needs to wait

                    if (q.account != null && !IsUpdate(q.mode) && queueLaunch.Count > 1 && IsQueueWaiting(q.Type, q.mode, _lastLaunch[(byte)q.account.Settings.Type]))
                    {
                        var qt = q.Type;

                        foreach (var t in new AccountType[] { AccountType.GuildWars1, AccountType.GuildWars2 })
                        {
                            if (qt != t)
                            {
                                var _q = queueLaunch.Peek(t);

                                if (_q != null)
                                {
                                    if (!IsQueueWaiting(_q.Type, _q.mode, _lastLaunch[(byte)_q.account.Settings.Type]))
                                    {
                                        q = _q;
                                    }

                                    break; //only need to check 1 other account type, since there's only 2
                                }
                            }
                        }
                    }

                    #endregion
                }
                finally
                {
                    if (Monitor.IsEntered(queueLaunch))
                        Monitor.Exit(queueLaunch);
                }

                if (WaitOnQueue(q.Type, q.mode, cancel) || cancel.IsCancellationRequested)
                {
                    continue;
                }

                #region Update accounts

                bool doUpdate = false;

                if (q.account == null)
                {
                    //applies to all accounts

                    if (q.mode == LaunchMode.Update)
                    {
                        doUpdate = true;
                    }
                    else
                    {
                        //not supported
                        lock (queueLaunch)
                        {
                            queueLaunch.Remove(q);
                            q.OnDequeued(QueuedLaunch.DequeuedState.Skipped);
                        }
                        continue;
                    }
                }
                else if (q.mode == LaunchMode.Launch)
                {
                    if (q.Type == AccountType.GuildWars2 && GetActiveProcessCount(AccountType.GuildWars2) == 0)
                    {
                        if (Settings.CheckForNewBuilds.Value)
                        {
                            int b = Tools.Gw2Build.Build;
                            if (b > 0 && b != build && Settings.LastKnownBuild.Value != b)
                            {
                                build = b;
                                doUpdate = true;
                            }
                        }
                        else if (!doUpdate && !FileManager.VerifyLocalDatBuild())
                        {
                            doUpdate = true;
                        }
                    }
                }

                if (doUpdate)
                {
                    if (BuildUpdated != null)
                    {
                        var e = new BuildUpdatedEventArgs();
                        try
                        {
                            BuildUpdated(e);
                        }
                        catch (Exception ex)
                        {
                            Util.Logging.Log(ex);
                        }
                        if (e.Queue != null && e.Queue.Count > 0)
                        {
                            List<QueuedLaunch> announce = new List<QueuedLaunch>(e.Queue.Count);

                            lock (queueLaunch)
                            {
                                var i = queueLaunch.Count;
                                bool first = true;

                                foreach (var _q in e.Queue)
                                {
                                    var account = GetAccount(_q);
                                    lock (account)
                                    {
                                        if (account.State == AccountState.None)
                                            account.SetState(AccountState.Waiting, true);
                                    }
                                    account.inQueueCount++;
                                    LaunchMode mode;

                                    if (first)
                                    {
                                        first = false;
                                        mode = LaunchMode.UpdateVisible;
                                    }
                                    else
                                        mode = LaunchMode.Update;

                                    if (account.Settings != null)
                                    {
                                        if (account.Type == AccountType.GuildWars2)
                                        {
                                            var gw2 = (Settings.IGw2Account)account.Settings;
                                            var dat = gw2.DatFile;
                                            if (dat != null)
                                                dat.IsPending = true;
                                        }
                                    }

                                    var ql = new QueuedLaunch(account, mode);
                                    queueLaunch.Enqueue(ql);
                                    announce.Add(ql);
                                }

                                //shifting any existing queued items to the back
                                //and removing any that were to be updated, since everything is
                                while (i-- > 0)
                                {
                                    var _q = queueLaunch.Dequeue();
                                    if (_q.account == null)
                                    {
                                        _q.OnDequeued(QueuedLaunch.DequeuedState.OK);
                                    }
                                    else if (IsUpdate(_q.mode))
                                    {
                                        _q.account.inQueueCount--;
                                        _q.OnDequeued(QueuedLaunch.DequeuedState.Skipped);
                                    }
                                    else
                                    {
                                        queueLaunch.Enqueue(_q);
                                    }
                                }
                            }

                            if (AccountQueued != null)
                            {
                                foreach (var _q in announce)
                                {
                                    try
                                    {
                                        AccountQueued(_q.account.Settings, _q.mode);
                                    }
                                    catch (Exception ex)
                                    {
                                        Util.Logging.Log(ex);
                                    }
                                }
                            }

                            continue;
                        }
                    }

                    if (q.account == null)
                    {
                        lock (queueLaunch)
                        {
                            queueLaunch.Remove(q);
                            q.OnDequeued(QueuedLaunch.DequeuedState.Skipped);
                        }
                        continue;
                    }
                }

                #endregion

                if (!IsUpdate(q.mode))
                {
                    #region Steam limiter

                    if (q.account.Settings.Proxy == Settings.LaunchProxy.Steam && !q.disableProxy)
                    {
                        if (Settings.Steam.Limitation.Value != Settings.SteamLimitation.BlockAll)
                        {
                            Account asteam;

                            if (IsSteamActive(q.Type, out asteam))
                            {
                                switch (Settings.Steam.Limitation.Value)
                                {
                                    case Settings.SteamLimitation.LaunchWithoutSteam:

                                        q.disableProxy = true;
                                        continue;

                                    case Settings.SteamLimitation.OnlyBlockSteam:
                                        
                                        var reordered = false;

                                        lock (queueLaunch)
                                        {
                                            //moving one non-steam launch to the top of the queue
                                            foreach (var qnext in queueLaunch.Dequeue(new Func<QueuedLaunch, bool>(
                                                delegate(QueuedLaunch qsearch)
                                                {
                                                    if (qsearch.account != null && qsearch.account.Settings.Proxy != Settings.LaunchProxy.Steam)
                                                    {
                                                        return true;
                                                    }

                                                    return false;
                                                })))
                                            {
                                                queueLaunch.Push(qnext);
                                                reordered = true;
                                                break;
                                            }
                                        }

                                        if (reordered)
                                            continue;

                                        break;
                                }
                            }
                        }

                        if (WaitOnSteam(q.Type, cancel, Settings.Steam.Limitation.Value == Settings.SteamLimitation.OnlyBlockSteam) || cancel.IsCancellationRequested)
                        {
                            continue;
                        }
                    }

                    #endregion
                }

                if (q.mode != LaunchMode.Launch)
                {
                    //launches running in normal mode can only run one client at a time
                    //these launches will be delayed until all clients are closed
                    //note: although this is not required for gw1, it's still done to mimic a normal launch

                    if (WaitOnActiveProcesses(q.Type, cancel, 0, true) || cancel.IsCancellationRequested)
                    {
                        continue;
                    }

                    #region DatUpdater

                    if (q.mode == LaunchMode.Update && datUpdater != null && datUpdater.CanUpdate && q.Type == AccountType.GuildWars2)
                    {
                        var gw2 = (Settings.IGw2Account)q.account.Settings;
                        var dat = gw2.DatFile;
                        if (dat != null)
                        {
                            lock (q.account)
                            {
                                q.account.SetState(AccountState.Updating, true);
                            }

                            FileManager.VerifyLinks(FileManager.FileType.Dat, dat);

                            if (datUpdater.Update(dat))
                            {

                                try
                                {
                                    //note that gw2 normally creates a new cache folder after a build change
                                    Tools.Gw2Cache.Delete(q.account.Settings.UID, true);
                                }
                                catch { }

                                lock (queueLaunch)
                                {
                                    queueLaunch.Remove(q);
                                    q.account.inQueueCount--;
                                    q.OnDequeued(QueuedLaunch.DequeuedState.OK);

                                    lock (q.account)
                                    {
                                        if (q.account.inQueueCount > 0)
                                        {
                                            q.account.SetState(AccountState.None, true);
                                            q.account.SetState(AccountState.Waiting, true);
                                        }
                                        else
                                        {
                                            q.account.SetState(AccountState.None, true);
                                        }
                                    }
                                }

                                continue;
                            }
                        }
                    }

                    #endregion
                }
                else
                {
                    //launches running in shared mode

                    #region Settings.DelayLaunchSeconds

                    if (Settings.DelayLaunchSeconds.HasValue && GetActiveProcessCount(AccountType.Any) > 0 && lastMode == q.mode)
                    {
                        var delay = Settings.DelayLaunchSeconds.Value * 1000;

                        if (lastLaunchTime != 0)
                        {
                            delay -= Environment.TickCount - lastLaunchTime;
                        }

                        if (delay >= 0 && cancel.WaitHandle.WaitOne(delay))
                        {
                            continue;
                        }
                    }

                    #endregion

                    #region Settings.LimitActiveAccounts

                    var limit = Settings.LimitActiveAccounts.Value;
                    if (limit > 0)
                    {
                        if (WaitOnActiveProcesses(AccountType.Any, cancel, limit - 1, true) || cancel.IsCancellationRequested)
                        {
                            continue;
                        }
                    }

                    #endregion

                    #region Login limiter

                    var l = limiter;
                    if (l != null && l.IsBlocked())
                    {
                        l.Wait(cancel);
                        continue;
                    }

                    #endregion
                }

                try
                {
                    try
                    {
                        var okay = false;

                        _lastLaunch[(byte)q.account.Settings.Type] = q;

                        try
                        {
                            if (okay = Launch(q, cancel))
                            {
                                if (q.mode == LaunchMode.Launch)
                                {
                                    var l = limiter;
                                    if (l != null)
                                    {
                                        l.Reduce();
                                    }
                                }
                            }
                        }
                        finally
                        {
                            if (!okay)
                            {
                                var ra = q.account.RunAfter;
                                if (ra != null)
                                    ra.Close();
                                q.account.Session = null;
                            }
                        }

                        lastMode = q.mode;
                        lastLaunchTime = Environment.TickCount;
                    }
                    catch (TaskCanceledException)
                    {
                        if (q.account.State == AccountState.WaitingForAuthentication)
                        {
                            lock (q.account)
                            {
                                q.account.SetState(AccountState.Waiting, false);
                            }
                        }

                        continue;
                    }
                    catch (Security.Impersonation.BadUsernameOrPasswordException)
                    {
                        throw new BadUsernameOrPasswordException();
                    }
                    catch (System.ComponentModel.Win32Exception e)
                    {
                        switch (e.NativeErrorCode)
                        {
                            case 1326:
                                throw new BadUsernameOrPasswordException();
                            case 267:
                                if (!Util.Users.IsCurrentUser(q.account.Settings.WindowsAccount))
                                    throw new Exception("The user \"" + q.account.Settings.WindowsAccount + "\" does not have permission to access the folder");
                                break;
                            case 5:
                                if (!Util.Users.IsCurrentUser(q.account.Settings.WindowsAccount))
                                    throw new Exception("The user \"" + q.account.Settings.WindowsAccount + "\" does not have permission to access the file");
                                break;
                        }

                        throw;
                    }
                    catch (DatFileUpdateRequiredException)
                    {
                        lock (queueLaunch)
                        {
                            queueLaunch.Remove(q);
                            q.account.inQueueCount--;
                            q.OnDequeued(QueuedLaunch.DequeuedState.OK);
                        }

                        OnUpdateRequired(q.account, q.mode, q.args, true, "An update may be required; client exited unexpectedly");

                        continue;
                    }
                    catch (Steam.AlreadyRunningSteamException)
                    {
                        if (q.account.errors++ > 5)
                            throw;

                        continue;
                    }
                }
                catch (Exception e)
                {
                    Util.Logging.Log(e);

                    lock (q.account)
                    {
                        q.account.SetState(AccountState.Waiting, false);
                    }

                    if (LaunchException != null)
                    {
                        var ex = new LaunchExceptionEventArgs(e);

                        try
                        {
                            LaunchException(q.account.Settings, ex);
                        }
                        catch (Exception exc)
                        {
                            Util.Logging.Log(exc);
                        }
                        
                        if (ex.Retry)
                        {
                            #region DatFileNotInitialized

                            if (e is DatFileNotInitialized)
                            {
                                if (q.Type == AccountType.GuildWars2)
                                {
                                    if (!((Settings.IGw2Account)q.account.Settings).DatFile.IsInitialized)
                                    {
                                        lock (queueLaunch)
                                        {
                                            queueLaunch.Remove(q);
                                            q.mode = LaunchMode.LaunchSingle;
                                            queueLaunch.Enqueue(q);
                                        }
                                    }
                                }
                                else
                                {

                                }
                            }

                            #endregion

                            continue;
                        }
                        else if (e is InvalidPathException)
                        {
                            #region InvalidPathException

                            var type = ((InvalidPathException)e).Type;

                            DumpQueue(
                                delegate(QueuedLaunch _q)
                                {
                                    return _q.account != null && _q.Type == type;
                                }, e);

                            continue;

                            #endregion
                        }
                        else if (e is InvalidSteamPathException)
                        {
                            #region InvalidSteamPathException

                            var type = ((InvalidSteamPathException)e).Type;

                            DumpQueue(
                                delegate(QueuedLaunch _q)
                                {
                                    return _q.account != null && _q.Type == type && _q.account.Settings.Proxy == Settings.LaunchProxy.Steam;
                                }, e);

                            continue;

                            #endregion
                        }
                        else if (e is BadUsernameOrPasswordException)
                        {
                            #region BadUsernameOrPasswordException

                            var username = Util.Users.GetUserName(q.account.Settings.WindowsAccount);

                            DumpQueue(
                                delegate(QueuedLaunch _q)
                                {
                                    return _q.account != null && Util.Users.GetUserName(_q.account.Settings.WindowsAccount).Equals(username, StringComparison.OrdinalIgnoreCase);
                                }, e);

                            continue;

                            #endregion
                        }
                        else if (e is NetworkChangedException)
                        {
                            #region NetworkChangedException

                            DumpQueue(
                                delegate(QueuedLaunch _q)
                                {
                                    return true;
                                }, e);

                            continue;

                            #endregion
                        }
                        else if (e is Gw2LockException)
                        {
                            #region Gw2LockException

                            DumpQueue(
                                delegate(QueuedLaunch _q)
                                {
                                    return _q.account != null && _q.Type == AccountType.GuildWars2;
                                }, e);

                            continue;

                            #endregion
                        }
                    }

                    lock (queueLaunch)
                    {
                        queueLaunch.Remove(q);
                        q.account.inQueueCount--;
                        q.OnDequeued(QueuedLaunch.DequeuedState.Skipped);

                        if (q.account.inQueueCount == 0)
                        {
                            lock (q.account)
                            {
                                q.account.SetState(AccountState.Error, true, e);
                                q.account.SetState(AccountState.None, false);
                            }
                        }
                    }

                    continue;
                }

                lock (queueLaunch)
                {
                    queueLaunch.Remove(q);
                    q.account.inQueueCount--;
                    q.OnDequeued(QueuedLaunch.DequeuedState.OK);

                    lock (q.account)
                    {
                        if (q.account.inQueueCount == 0 && q.account.State == AccountState.Waiting)
                            q.account.SetState(AccountState.None, true);
                    }
                }

                #region DatUpdater

                if (IsUpdate(q.mode) && WaitForExit(q.account, cancel))
                {
                    if (q.Type == AccountType.GuildWars2)
                    {
                        var gw2 = (Settings.IGw2Account)q.account.Settings;
                        var dat = gw2.DatFile;

                        FileManager.VerifyLinks(FileManager.FileType.Dat, dat);
                        FileManager.VerifyLocalDatBuild(false);

                        if (Settings.GuildWars2.DatUpdaterEnabled.Value || Settings.GuildWars2.UseCustomGw2Cache.Value)
                        {
                            if (q.mode == LaunchMode.UpdateVisible)
                            {
                                try
                                {
                                    if (Settings.GuildWars2.DatUpdaterEnabled.Value)
                                    {
                                        datUpdater = Tools.DatUpdater.Create(gw2.DatFile);
                                        dat.IsPending = false;
                                    }
                                    else if (Settings.GuildWars2.UseCustomGw2Cache.Value)
                                    {
                                        datUpdater = Tools.DatUpdater.Create();
                                        if (datUpdater.UpdateCache(gw2.DatFile))
                                            dat.IsPending = false;
                                    }
                                }
                                catch
                                {
                                    datUpdater = null;
                                }
                            }
                            else if (datUpdater != null && !datUpdater.CanUpdate)
                            {
                                try
                                {
                                    if (datUpdater.UpdateCache(gw2.DatFile))
                                        dat.IsPending = false;
                                }
                                catch { }
                            }

                            try
                            {
                                //note that gw2 normally creates a new cache folder after a build change
                                Tools.Gw2Cache.Delete(q.account.Settings.UID, true);
                            }
                            catch { }
                        }
                    }
                }

                #endregion
            }
            while (true);
        }

        /// <summary>
        /// Removes items from the queue, raising an error for the account
        /// </summary>
        /// <param name="onItem">Return true to remove the item from the queue</param>
        private static void DumpQueue(Func<QueuedLaunch, bool> onItem, Exception e)
        {
            //dump the remaining queue
            lock (queueLaunch)
            {
                var handled = new HashSet<Account>();
                var i = queueLaunch.Count;

                while (i-- > 0)
                {
                    var _q = queueLaunch.Dequeue();

                    if (onItem(_q))
                    {
                        if (_q.account == null)
                        {
                            _q.OnDequeued(QueuedLaunch.DequeuedState.Skipped);
                        }
                        else
                        {
                            _q.account.inQueueCount--;
                            _q.OnDequeued(QueuedLaunch.DequeuedState.Skipped);

                            if (handled.Add(_q.account))
                            {
                                lock (_q.account)
                                {
                                    _q.account.SetState(AccountState.Error, true, e);
                                    _q.account.SetState(AccountState.None, false);
                                }
                            }
                        }
                    }
                    else
                        queueLaunch.Enqueue(_q);
                }
            }
        }

        private static bool Launch(QueuedLaunch q, CancellationToken cancel)
        {
            FileInfo fi;
            try
            {
                string path = null;

                switch (q.Type)
                {
                    case AccountType.GuildWars2:

                        if (Settings.GuildWars2.Path.HasValue)
                            path = Settings.GuildWars2.Path.Value;

                        break;
                    case AccountType.GuildWars1:

                        if (Settings.GuildWars1.Path.HasValue)
                            path = Settings.GuildWars1.Path.Value;

                        break;
                }

                if (!string.IsNullOrEmpty(path))
                    fi = new FileInfo(path);
                else
                    fi = null;
            }
            catch (Exception e)
            {
                Util.Logging.Log(e);

                fi = null;
            }

            if (fi == null || !fi.Exists)
            {
                throw InvalidPathException.From(q.Type);
            }

            return Launch(q, fi, cancel);
        }

        /// <summary>
        /// Returns a queued launch that will update the DatFile linked to the account, if available (not including LaunchSingle)
        /// </summary>
        private static QueuedLaunch GetQueuedUpdate(Account account)
        {
            lock (queueLaunch)
            {
                if (queueLaunch.Count == 0)
                    return null;

                ushort uid = 0;

                switch (account.Type)
                {
                    case AccountType.GuildWars2:

                        var gw2 = (Settings.IGw2Account)account.Settings;
                        if (gw2.DatFile != null)
                            uid = gw2.DatFile.UID;

                        break;
                    case AccountType.GuildWars1:
                        
                        var gw1 = (Settings.IGw1Account)account.Settings;
                        if (gw1.DatFile != null)
                            uid = gw1.DatFile.UID;

                        break;
                    default:

                        throw new NotSupportedException();
                }

                foreach (var q in queueLaunch)
                {
                    if (!IsUpdate(q.mode))
                        continue;

                    if (q.account == null)
                    {
                        return q;
                    }
                    else if (q.Type != account.Type)
                    {
                        continue;
                    }
                    else if (uid == 0)
                    {
                        if (q.account.Settings.UID == account.Settings.UID)
                            return q;
                    }
                    else
                    {
                        switch (q.Type)
                        {
                            case AccountType.GuildWars2:

                                var gw2 = (Settings.IGw2Account)q.account.Settings;
                                if (gw2.DatFile != null && gw2.DatFile.UID == uid)
                                    return q;

                                break;
                            case AccountType.GuildWars1:

                                var gw1 = (Settings.IGw1Account)q.account.Settings;
                                if (gw1.DatFile != null && gw1.DatFile.UID == uid)
                                    return q;

                                break;
                        }
                    }
                }

                return null;
            }
        }

        private static bool IsUpdate(LaunchMode mode)
        {
            switch (mode)
            {
                case LaunchMode.Update:
                case LaunchMode.UpdateVisible:
                    return true;
            }
            return false;
        }

        private static bool IsLaunch(LaunchMode mode)
        {
            switch (mode)
            {
                case LaunchMode.Launch:
                case LaunchMode.LaunchSingle:
                    return true;
            }
            return false;
        }

        private static bool CheckDat(string path)
        {
            try
            {
                return File.Exists(path);
            }
            catch (Exception e)
            {
                Util.Logging.Log(e);
            }

            return false;
        }

        private static ProcessPriorityClass GetPriority(Settings.ProcessPriorityClass priority)
        {
            switch (priority)
            {
                case Settings.ProcessPriorityClass.High:
                    return ProcessPriorityClass.High;
                case Settings.ProcessPriorityClass.AboveNormal:
                    return ProcessPriorityClass.AboveNormal;
                case Settings.ProcessPriorityClass.BelowNormal:
                    return ProcessPriorityClass.BelowNormal;
                case Settings.ProcessPriorityClass.Low:
                    return ProcessPriorityClass.Idle;
            }
            return ProcessPriorityClass.Normal;
        }

        private static bool IsLocalized(Settings.IGw2Account a)
        {
            if (a.Proxy == Settings.LaunchProxy.Steam)
                return false;

            switch (Settings.GuildWars2.LocalizeAccountExecutionSelection.Value)
            {
                case Settings.LocalizeAccountExecutionSelectionOptions.Include:
                    return a.LocalizedExecution;
                case Settings.LocalizeAccountExecutionSelectionOptions.Exclude:
                    return !a.LocalizedExecution;
            }
            return true;
        }

        private static bool Launch(QueuedLaunch q, FileInfo exe, CancellationToken cancel)
        {
            var account = q.account;
            var mode = q.mode;
            var type = q.Type;
            var isUpdate = IsUpdate(mode);
            var fi = exe;
            var localized = false;

            #region UserAlreadyActiveException (no longer used)
            //var username = Util.Users.GetUserName(account.Settings.WindowsAccount);

            //Account _active = GetActiveAccount(username);
            //if (_active != null)
            //{
            //    //the user's account is already in use, however, if the same dat file is being used, it doesn't matter
            //    if (_active.Settings.DatFile != account.Settings.DatFile)
            //        throw new UserAlreadyActiveException(username);
            //}
            #endregion

            FileManager.IProfileInformation customProfile = null;
            Security.Impersonation.IIdentity identity = null;
            Account.LaunchSession s = null;
            Tools.WindowManager.IWindowBounds wtemplate = null;

            byte retries = 0;

            try
            {
                if (!Util.Users.IsCurrentEnvironmentUser(account.Settings.WindowsAccount))
                {
                    var password = Security.Credentials.GetPassword(account.Settings.WindowsAccount);
                    if (password == null)
                        throw new BadUsernameOrPasswordException();
                    identity = Security.Impersonation.GetIdentity(account.Settings.WindowsAccount, password);
                }

                if (type == AccountType.GuildWars2)
                {
                    var gw2 = (Settings.IGw2Account)account.Settings;

                    #region Activate profile

                    do
                    {
                        try
                        {
                            if (gw2.UID > 0)
                            {
                                customProfile = FileManager.Activate(gw2, identity);
                            }
                            else
                            {
                                customProfile = FileManager.Activate(q.args);

                                accounts[0] = account;

                                var userprofile = customProfile.UserProfile;
                                EventHandler<Account> onExit = null;
                                onExit = delegate(object o, Account a)
                                {
                                    a.Exited -= onExit;
                                    try
                                    {
                                        FileManager.DeleteProfile(userprofile);
                                    }
                                    catch { }
                                };
                                account.Exited += onExit;
                            }
                            break;
                        }
                        catch (FileManager.UserAccountNotInitializedException e)
                        {
                            Util.Logging.Log(e);

                            if (retries++ > 0)
                            {
                                throw;
                            }

                            var username = Util.Users.GetUserName(account.Settings.WindowsAccount);
                            var password = Security.Credentials.GetPassword(username);
                            if (password == null)
                                throw new BadUsernameOrPasswordException();
                            try
                            {
                                Util.ProcessUtil.InitializeAccount(username, password);
                            }
                            catch (Exception ex)
                            {
                                Util.Logging.Log(ex);
                            }
                        }
                    }
                    while (true);

                    if (mode == LaunchMode.Launch)
                    {
                        if (!gw2.DatFile.IsInitialized || !CheckDat(gw2.DatFile.Path))
                        {
                            throw new DatFileNotInitialized();
                        }
                    }

                    #endregion

                    #region Activate localized path

                    if (!isUpdate && Settings.GuildWars2.LocalizeAccountExecution.Value.HasFlag(Settings.LocalizeAccountExecutionOptions.Enabled) && IsLocalized(gw2))
                    {
                        try
                        {
                            var path = FileManager.ActivateLocalizedPath(gw2, fi);
                            if (path != null)
                                fi = new FileInfo(path);
                            localized = true;
                        }
                        catch (NotSupportedException)
                        {
                            Settings.GuildWars2.LocalizeAccountExecution.Value = Settings.GuildWars2.LocalizeAccountExecution.Value & ~Settings.LocalizeAccountExecutionOptions.Enabled;
                            throw new Exception("Linking " + fi.Name + " is not supported; localized execution has been disabled");
                        }
                        catch (Exception e)
                        {
                            throw new Exception("Failed to create localized path:\n" + e.Message);
                        }
                    }

                    if (!localized && mode == LaunchMode.Launch && FileManager.IsGw2China)
                    {
                        try
                        {
                            using (var f = File.Open(Path.Combine(fi.DirectoryName, "Gw2.dat", "Lock.dat"), FileMode.Open, FileAccess.Read, FileShare.None))
                            {

                            }
                        }
                        catch (IOException e)
                        {
                            switch (e.HResult & 0xFFFF)
                            {
                                case 32: //ERROR_SHARING_VIOLATION

                                    throw new Gw2LockException();
                            }
                        }
                    }

                    #endregion

                    #region Activate basic

                    //needs to update localized path regardless of mode
                    if (customProfile != null && customProfile.IsBasic)
                    {
                        try
                        {
                            FileManager.ActivateBasic(gw2, identity, customProfile);
                        }
                        catch (Exception e)
                        {
                            throw new Exception("Unable to link basic profile", e);
                        }
                    }

                    #endregion

                    if (!isUpdate)
                    {
                        try
                        {
                            if (gw2.Proxy == Settings.LaunchProxy.Steam)
                                Tools.Gw2Cache.DeleteLoginCounter();
                            else
                                Tools.Gw2Cache.DeleteLoginCounter(gw2.UID);
                        }
                        catch { }
                    }
                }
                else if (type == AccountType.GuildWars1)
                {
                    var gw1 = (Settings.IGw1Account)account.Settings;

                    customProfile = FileManager.Activate(gw1);

                    fi = new FileInfo(Path.Combine(Path.GetDirectoryName(gw1.DatFile.Path), fi.Name));
                }
                else
                {
                    throw new NotSupportedException();
                }

                #region NetworkAuthorization

                //if (!isUpdate && Settings.NetworkAuthorization.HasValue)
                //{
                //    //warning: authorization can take ~7s after 3+ logins, which will increase to 30~60+s if more requests are made too quickly; this will eventually cause the launcher to fail to login
                //    session = NetworkAuthorization.Verify(account.Settings,
                //        delegate
                //        {
                //            lock (account)
                //            {
                //                account.SetState(AccountState.WaitingForAuthentication, true);
                //            }
                //        });

                //    if (cancel.IsCancellationRequested)
                //    {
                //        throw new TaskCanceledException();
                //    }

                //    var l = limiter;
                //    if (l != null)
                //    {
                //        if (l.IsBlocked())
                //        {
                //            if (account.State == AccountState.WaitingForAuthentication)
                //            {
                //                lock (account)
                //                {
                //                    account.SetState(AccountState.Waiting, true);
                //                }
                //            }

                //            if (!l.Wait(cancel))
                //            {
                //                throw new TaskCanceledException();
                //            }
                //        }
                //    }
                //}

                #endregion

                #region Window Manager

                if (!isUpdate && IsWindowed(account.Settings) && Tools.WindowManager.IsActive)
                {
                    Tools.WindowManager.IWindowBounds wb;
                    var wm = Tools.WindowManager.Instance;
                    var wbr = wm.TryGetBounds(account.Settings, out wb);

                    if (wbr == Tools.WindowManager.BoundsResult.Success)
                    {
                        wtemplate = wb;
                    }
                    else if (wbr == Tools.WindowManager.BoundsResult.Busy && wm.DelayLaunchUntilAvailable)
                    {
                        lock (account)
                        {
                            account.SetState(AccountState.Waiting, true);
                        }

                        using (var waiter = new ManualResetEvent(false))
                        {
                            EventHandler onTemplatesChanged = delegate
                            {
                                try
                                {
                                    waiter.Set();
                                }
                                catch { }
                            };
                            wm.TemplatesChanged += onTemplatesChanged;
                            Settings.WindowManagerOptions.ValueChanged += onTemplatesChanged;
                            try
                            {
                                using (cancel.Register(delegate
                                {
                                    waiter.Set();
                                }))
                                {
                                    while (true)
                                    {
                                        var b = waiter.WaitOne();

                                        if (cancel.IsCancellationRequested)
                                        {
                                            throw new TaskCanceledException();
                                        }

                                        if (b)
                                        {
                                            waiter.Reset();

                                            wbr = wm.TryGetBounds(account.Settings, out wb);

                                            if (wbr == Tools.WindowManager.BoundsResult.Success)
                                            {
                                                wtemplate = wb;
                                                break;
                                            }
                                            else if (wbr == Tools.WindowManager.BoundsResult.None || !wm.DelayLaunchUntilAvailable)
                                            {
                                                break;
                                            }
                                        }
                                    }
                                }
                            }
                            finally
                            {
                                wm.TemplatesChanged -= onTemplatesChanged;
                                Settings.WindowManagerOptions.ValueChanged -= onTemplatesChanged;
                            }
                        }
                    }
                }

                #endregion

                lock (account)
                {
                    switch (mode)
                    {
                        case LaunchMode.Update:
                            account.SetState(AccountState.Updating, true);
                            break;
                        case LaunchMode.UpdateVisible:
                            account.SetState(AccountState.UpdatingVisible, true);
                            break;
                        default:
                            account.SetState(AccountState.Launching, true);
                            break;
                    }

                    account.isRelaunch = 0;
                }

                if (type == AccountType.GuildWars2)
                {
                    if (mode == LaunchMode.Launch && (Settings.GuildWars2.PreventDefaultCoherentUI.Value && !localized) || localized && Settings.GuildWars2.LocalizeAccountExecution.Value.HasFlag(Settings.LocalizeAccountExecutionOptions.OnlyIncludeBinFolders))
                    {
                        lock (queueLaunch)
                        {
                            if (coherentLock == null)
                            {
                                try
                                {
                                    //CoherentUI_Host.exe is the first file loaded
                                    //icudt.dll is the first file that requires write access
                                    //icudt.dll changed to icudtl.dat
                                    //icudtl.dat can no longer be write-locked (causes CoherentUI to crash)
                                    //changed to CoherentUI64.dll

                                    coherentLock = File.Open(Path.Combine(Path.GetDirectoryName(Settings.GuildWars2.Path.Value), FileManager.IsGw264Bit ? "bin64" : "bin", FileManager.IsGw264Bit ? "CoherentUI64.dll" : "CoherentUI.dll"), FileMode.Open, FileAccess.Read, FileShare.None);
                                }
                                catch (Exception e)
                                {
                                    Util.Logging.Log(e);
                                }
                            }
                        }
                    }
                    else if (coherentLock != null)
                    {
                        lock (queueLaunch)
                        {
                            using (coherentLock) { }
                            coherentLock = null;
                        }
                    }
                }

                //flush announcements
                Task t = taskAnnounce;
                if (t != null)
                {
                    try
                    {
                        t.Wait();
                    }
                    catch (Exception e)
                    {
                        Util.Logging.Log(e);
                    }
                }

                KillMutex(type);

                var processOptions = GetProcessStartInfo(q, customProfile, fi);
                var isWindowed = IsWindowed(account.Settings);

                if (processOptions.UserName != null)
                    Security.Impersonation.EnsureDefault();

                retries = 0;

                account.Session = s = new Account.LaunchSession(account, mode, q.args);

                if (!isUpdate)
                {
                    s.RunAfter = RunAfterManager.Create(account, true);

                    if (RunAfter(Settings.RunAfter.RunAfterWhen.BeforeLaunching, account) > 0)
                    {
                        account.RunAfter.Wait(cancel);
                        if (cancel.IsCancellationRequested)
                        {
                            lock (account)
                            {
                                account.SetState(AccountState.None, true);
                            }
                            return false;
                        }
                    }
                }

                #region Network

                if ((Settings.Network.Value & Settings.NetworkOptions.Enabled) != 0 && mode != LaunchMode.Update)
                {
                    var e = Network.Elapsed;
                    var check = false;
                    
                    if (e < 30000)
                    {
                        check = GetActiveProcessCount() == 0;
                    }
                    else
                    {
                        check = GetActiveGameProcessCount(AccountType.Any) == 0;
                    }

                    if (check && !Network.Verify())
                    {
                        throw new NetworkChangedException();
                    }
                }

                #endregion

                do
                {
                    const byte WINDOW_STATE_INITIALIZED = 1;
                    const byte WINDOW_STATE_LOADED = 2;
                    const byte WINDOW_STATE_DX_CREATED = 3;
                    const byte WINDOW_STATE_READY = 4;
                    const byte WINDOW_STATE_CANCEL = 255;

                    WindowWatcher watcher = null;
                    ManualResetEvent waiter = null;
                    EventHandler<WindowWatcher.WindowChangedEventArgs> onChanged = null;
                    WindowLock.ILock wlock = null;

                    try
                    {
                        var useProxy = !isUpdate && !q.disableProxy && (account.Settings.Proxy == Settings.LaunchProxy.Steam || account.Type == AccountType.GuildWars2 && ((Settings.IGw2Account)account.Settings).Provider == Settings.AccountProvider.Steam)
                            //note the proxy must be used when launching on another user's account due to changing the environmental variables - alternatively, all variables must be set
                            || processOptions.UserName != null && (customProfile == null || customProfile != null && customProfile.UserProfile == null);

                        if (mode == LaunchMode.Launch)
                        {
                            useProxy = useProxy || Settings.PreventTaskbarGrouping.Value || Settings.ForceTaskbarGrouping.Value;

                            if (type == AccountType.GuildWars2)
                            {
                                //GW2 will load with default GFX settings if the xml file can't be read / is being written to
                                s.GfxLock = FileManager.FileLocker.Lock(((Settings.IGw2Account)account.Settings).GfxFile, 5000, customProfile != null && customProfile.IsBasic);
                            }

                            var _locker = wlocker;
                            if (_locker != null)
                            {
                                wlock = _locker.Lock();
                            }
                        }

                        Process p;

                        if (useProxy)
                        {
                            if (account.Settings.Proxy == Settings.LaunchProxy.Steam && !q.disableProxy)
                            {
                                if (type == AccountType.GuildWars2)
                                {
                                    string steam;
                                    if (Settings.Steam.Path.HasValue)
                                        steam = Settings.Steam.Path.Value;
                                    else
                                        steam = Steam.Path;
                                    if (!File.Exists(steam))
                                        throw new InvalidSteamPathException(AccountType.GuildWars2, "Path to Steam not found");
                                    try
                                    {
                                        if (Steam.Launch(steam, Steam.APPID_GW2, processOptions.Arguments, cancel))
                                        {
                                            string n;
                                            if (Settings.GuildWars2.PathSteam.HasValue)
                                                n = Path.GetFileNameWithoutExtension(Settings.GuildWars2.PathSteam.Value);
                                            else
                                                n = Path.GetFileNameWithoutExtension(fi.Name);
                                            p = WaitForProxyLaunch(account.Settings.UID, n, cancel, Settings.Steam.Timeout.HasValue ? Settings.Steam.Timeout.Value * 1000 : 5000);
                                            if (p != null)
                                                account.Process.Attach(p);
                                            s.Proxy = Settings.LaunchProxy.Steam;
                                        }
                                        else
                                        {
                                            throw new Exception("Failed to launch Steam");
                                        }
                                    }
                                    catch (Steam.AlreadyRunningSteamException)
                                    {
                                        switch (Settings.Steam.Limitation.Value)
                                        {
                                            case Settings.SteamLimitation.LaunchWithoutSteam:

                                                q.disableProxy = true;

                                                continue;
                                        }

                                        throw;
                                    }
                                }
                                else
                                {
                                    throw new NotSupportedException();
                                }
                            }
                            else
                            {
                                p = ProxyLauncher.Launch(account.Settings, processOptions, mode == LaunchMode.Launch && (Settings.PreventTaskbarGrouping.Value || Settings.ForceTaskbarGrouping.Value), Settings.ForceTaskbarGrouping.Value);
                                if (p != null)
                                    account.Process.Attach(p);
                                else
                                {
                                    //process was started, but could not found (exited or restarted)
                                }
                            }
                        }
                        else
                        {
                            p = account.Process.Launch(processOptions.ToProcessStartInfo());
                        }

                        bool okay;
                        byte windowState = 0;

                        if (p != null)
                        {
                            lock (unknownProcesses)
                            {
                                if (!p.HasExited)
                                {
                                    activeProcesses[GetIndex(type)]++;
                                    OnActiveProcessCountChanged(type);
                                    OnActiveProcessCountChanged();
                                }
                            }

                            if (!isUpdate)
                            {
                                s.WindowOptions = account.Settings.WindowOptions;

                                s.ProcessSettings = new ProcessSettings()
                                {
                                    ProcessAffinity = account.Settings.ProcessAffinity,
                                    ProcessPriority = account.Settings.ProcessPriority != Settings.ProcessPriorityClass.None ? account.Settings.ProcessPriority : Settings.ProcessPriority.Value,
                                    WindowOptions = account.Settings.WindowOptions,
                                };

                                if (wtemplate != null)
                                {
                                    s.WindowTemplate = wtemplate;
                                    wtemplate = null;
                                }
                                else if (isWindowed)
                                {
                                    Tools.WindowManager.IWindowBounds wb;
                                    if (Tools.WindowManager.Instance.TryGetBounds(account.Settings, out wb) == Tools.WindowManager.BoundsResult.Success)
                                        s.WindowTemplate = wb;
                                }

                                if (account.Type == AccountType.GuildWars2)
                                {
                                    s.MumbleLink = mumble.Add(Util.Args.GetValue(processOptions.Arguments, "mumble"), p.Id, account.Settings);
                                    s.MumbleLink.Verified += OnMumbleLinkVerified;
                                }

                                watcher = s.Watcher = new WindowWatcher(account, p, true, isWindowed, s);
                                watcher.WindowChanged += OnWatchedWindowChanged;
                                watcher.WindowCrashed += OnWatchedWindowCrashed;
                                if (account.Type == AccountType.GuildWars2)
                                    watcher.Timeout += OnWatchedWindowTimeout;
                                watcher.ProcessOptions = processOptions;

                                var l = limiter;
                                if (l != null && l.IsAutomatic && account.Type == AccountType.GuildWars2 && IsAutomaticLogin(account.Settings))
                                {
                                    s.Limiter = l.BeginSession();
                                    if (s.Limiter != null)
                                    {
                                        watcher.LoginComplete += OnWatchedWindowLoginComplete;
                                    }
                                }

                                waiter = new ManualResetEvent(false);

                                onChanged = delegate(object o, WindowWatcher.WindowChangedEventArgs e)
                                {
                                    lock (watcher)
                                    {
                                        if (waiter == null)
                                            return;

                                        switch (e.Type)
                                        {
                                            case WindowWatcher.WindowChangedEventArgs.EventType.LauncherWindowHandleCreated:

                                                if (wlock != null)
                                                {
                                                    WindowLock.ToBottom(e.Handle, true);
                                                }

                                                return;
                                            case WindowWatcher.WindowChangedEventArgs.EventType.HandleChanged:

                                                if (wlock != null)
                                                {
                                                    wlock.Release(e.Handle);
                                                    wlock = null;
                                                }

                                                break;
                                            case WindowWatcher.WindowChangedEventArgs.EventType.LauncherWindowLoaded:
                                                if (windowState < WINDOW_STATE_LOADED)
                                                {
                                                    windowState = WINDOW_STATE_LOADED;
                                                    waiter.Set();
                                                }
                                                return;
                                            case WindowWatcher.WindowChangedEventArgs.EventType.DxWindowCreated:
                                                if (windowState < WINDOW_STATE_DX_CREATED)
                                                {
                                                    windowState = WINDOW_STATE_DX_CREATED;
                                                }
                                                break;
                                            case WindowWatcher.WindowChangedEventArgs.EventType.DxWindowLoaded:
                                            case WindowWatcher.WindowChangedEventArgs.EventType.WatcherExited:
                                                if (windowState < WINDOW_STATE_READY)
                                                {
                                                    windowState = WINDOW_STATE_READY;
                                                    waiter.Set();
                                                }
                                                return;
                                        }

                                        if (windowState == 0)
                                        {
                                            windowState = WINDOW_STATE_INITIALIZED;
                                            waiter.Set();
                                        }
                                    }
                                };

                                watcher.WindowChanged += onChanged;
                                watcher.Start();

                                if (waiter.WaitOne())
                                {
                                    okay = !p.HasExited;
                                    waiter.Reset();
                                }
                                else
                                    okay = !p.WaitForExit(2000);
                            }
                            else
                            {
                                okay = !p.WaitForExit(2000);
                            }
                        }
                        else
                        {
                            waiter = null;
                            okay = false;
                        }

                        if (okay)
                        {
                            if (!isUpdate && !p.HasExited)
                            {
                                switch (mode)
                                {
                                    case LaunchMode.Launch:

                                        try
                                        {
                                            account.Process.KillMutex();
                                        }
                                        catch { }

                                        break;
                                    case LaunchMode.LaunchSingle:

                                        if (type == AccountType.GuildWars2)
                                        {
                                            ((Settings.IGw2Account)account.Settings).DatFile.IsInitialized = true;
                                        }

                                        break;
                                }

                                lock (account)
                                {
                                    if (!p.HasExited)
                                    {
                                        account.SetState(AccountState.Active, true, p);
                                        if (windowState >= WINDOW_STATE_DX_CREATED)
                                            account.SetState(AccountState.ActiveGame, true);
                                    }
                                }

                                try
                                {
                                    var events = windowEvents.Add(p.Id, account);
                                    events.ForegroundChanged += events_ForegroundChanged;
                                }
                                catch (Exception ex)
                                {
                                    Util.Logging.Log(ex);
                                }

                                if (AccountLaunched != null)
                                {
                                    try
                                    {
                                        AccountLaunched(account.Settings);
                                    }
                                    catch (Exception e)
                                    {
                                        Util.Logging.Log(e);
                                    }
                                }

                                EventHandler<Account> onExit = null;
                                onExit = delegate(object o, Account a)
                                {
                                    a.Exited -= onExit;
                                    if (AccountExited != null)
                                        AccountExited(a.Settings);
                                };

                                lock (queueExit)
                                {
                                    if (account.State == AccountState.Active || account.State == AccountState.ActiveGame)
                                        account.Exited += onExit;
                                }

                                if (!p.HasExited)
                                {
                                    if (AccountWindowEvent != null)
                                    {
                                        try
                                        {
                                            var h = Windows.FindWindow.FindMainWindow(p);
                                            if (NativeMethods.GetForegroundWindow() == h)
                                                AccountWindowEvent(account.Settings, new AccountWindowEventEventArgs(AccountWindowEventEventArgs.EventType.Focused, p, h));
                                        }
                                        catch { }
                                    }

                                    if (RunAfter(Settings.RunAfter.RunAfterWhen.AfterLaunching, account) > 0)
                                        account.RunAfter.Wait(cancel);

                                    if (mode == LaunchMode.Launch)
                                    {
                                        byte delayUntil;

                                        if (Settings.DelayLaunchUntilLoaded.Value)
                                        {
                                            delayUntil = WINDOW_STATE_READY;
                                        }
                                        else if (customProfile != null && customProfile.IsBasic)
                                        {
                                            //in basic mode, the launcher must be fully loaded before allowing another account to launch
                                            delayUntil = WINDOW_STATE_LOADED;
                                        }
                                        else
                                        {
                                            delayUntil = 0;
                                        }

                                        if (windowState < delayUntil)
                                        {
                                            using (cancel.Register(delegate
                                            {
                                                windowState = WINDOW_STATE_CANCEL;
                                                waiter.Set();
                                            }))
                                            {
                                                while (windowState < delayUntil && waiter != null)
                                                {
                                                    while (!waiter.WaitOne())
                                                    {

                                                    }
                                                    waiter.Reset();
                                                }
                                            }
                                        }
                                    }
                                }
                            }

                            return true;
                        }
                        else
                        {
                            double duration;
                            if (p != null)
                                duration = p.ExitTime.Subtract(p.StartTime).TotalSeconds;
                            else
                                duration = 0;

                            if (isUpdate || duration < 1)
                            {
                                //GW2 was likely closed due to another copy running, or the client is being updated

                                WaitForScanner();

                                if (isUpdate)
                                {
                                    return true;
                                }
                                else if (account.Process.Process != null)
                                {
                                    //was handled by taskScan
                                    //assuming GW2 closed itself, restarted and that process was attached
                                    return true;
                                }
                                else
                                {
                                    //don't try to restart the process if the queue has been killed
                                    if (cancelQueue != null && cancelQueue.IsCancellationRequested)
                                        break;

                                    //assuming GW2 failed to open due to another instance running
                                    //try to kill any other accounts prior to trying administrative access
                                    if (!KillMutex(type))
                                    {
                                        retries++;

                                        if (IsMutexOpen(type))
                                        {
                                            try
                                            {
                                                //this should only happen if there are multiple GW2 installs, in which case trying to find it may be impossible (renamed)
                                                //rather than trying to first find a GW2 process, the entire system will be searched once

                                                if (!Util.ProcessUtil.KillMutexWindow(account.Settings.Type, false))
                                                    Util.ProcessUtil.KillMutexWindow(account.Settings.Type, true);
                                            }
                                            catch (Exception e)
                                            {
                                                //failed or cancelled
                                                Util.Logging.Log(e);
                                                break;
                                            }
                                        }
                                        else
                                        {
                                            throw new Exception("Exited while launching");
                                        }
                                    }
                                }
                            }
                            else
                            {
                                if (type == AccountType.GuildWars2 && mode == LaunchMode.Launch && Util.Args.Contains(processOptions.Arguments, "nopatchui"))
                                {
                                    //if Local.dat is outdated when launched with -nopatchui, it will simply exit instead of crashing
                                    throw new DatFileUpdateRequiredException();
                                }

                                //AccountStateChanged handled by process exit
                                return true;
                            }
                        }
                    }
                    finally
                    {
                        if (wlock != null)
                        {
                            wlock.Dispose();
                        }

                        if (watcher != null)
                        {
                            lock (watcher)
                            {
                                watcher.WindowChanged -= onChanged;
                                waiter.Dispose();
                                waiter = null;
                            }
                        }
                    }
                }
                while (retries < 2);

                lock (account)
                {
                    account.SetState(AccountState.None, true);
                }
            }
            finally
            {
                if (wtemplate != null)
                {
                    wtemplate.Dispose();
                }

                if (s != null && account.Session != s)
                {
                    s.Dispose();
                }

                if (customProfile != null)
                {
                    using (customProfile)
                    {
                        if (mode == LaunchMode.Launch && customProfile.IsBasic)
                        {
                            try
                            {
                                FileManager.DeactivateBasic(identity, customProfile);
                            }
                            catch { }
                        }
                    }
                }

                if (identity != null)
                    identity.Dispose();
            }
            return false;
        }

        private static int RunAfter(Settings.RunAfter.RunAfterWhen state, Account a)
        {
            if (Settings.DisableRunAfter.Value)
                return 0;

            try
            {
                var ra = a.RunAfter;
                if (ra != null)
                {
                    return ra.Start(state);
                }
            }
            catch { }

            return 0;
        }

        /// <summary>
        /// Returns the count of all processes, including unknowns
        /// </summary>
        public static int GetActiveProcessCount()
        {
            return GetActiveProcessCount(AccountType.Any);
        }

        public static ushort GetActiveProcessCount(AccountType type)
        {
            ushort count;

            switch (type)
            {
                case AccountType.GuildWars1:
                case AccountType.GuildWars2:
                    
                    count = activeProcesses[GetIndex(type)];

                    break;
                case AccountType.Unknown:

                    count = (ushort)unknownProcesses.Count;

                    break;
                case AccountType.None:

                    count = 0;

                    break;
                case AccountType.Any:

                    count = 0;
                    foreach (var c in activeProcesses)
                        count += c;

                    break;
                default:

                    throw new NotSupportedException();
            }

            return count;
        }

        public static int GetActiveGameProcessCount(AccountType type)
        {
            return LinkedProcess.GetActiveCount(type, AccountState.ActiveGame);
        }

        public static int GetPendingLaunchCount()
        {
            return queue.Count + queueLaunch.Count;
        }

        public static void CancelPendingLaunches()
        {
            lock (queueLaunch)
            {
                aborting = true;
                if (cancelQueue != null)
                    cancelQueue.Cancel();
            }
        }
        
        /// <summary>
        /// Cancels pending launching and closes any active processes matching the specified launch mode
        /// </summary>
        /// <param name="mode">Only launch modes of this type will be killed, or all if none are specified</param>
        public static async void CancelAndKillActiveLaunches(AccountType type, params LaunchMode[] mode)
        {
            CancelPendingLaunches();

            Task t;

            do
            {
                lock (queueExit)
                {
                    t = taskScan;
                    if (t == null || t.IsCompleted)
                    {
                        t = taskScan = new Task(
                            delegate
                            {
                                DoScan(type, ScanOptions.KillLinked, mode);
                            });
                        t.Start();

                        break;
                    }
                }

                try
                {
                    await t;
                }
                catch (Exception e)
                {
                    Util.Logging.Log(e);
                }
            }
            while (true);

            try
            {
                await t;
            }
            catch (Exception e)
            {
                Util.Logging.Log(e);
            }
        }

        public static int KillActiveLaunches(AccountType type)
        {
            int count = 0;
            foreach (var l in LinkedProcess.GetActive())
            {
                var p = l.Process;
                bool hasExited;
                try
                {
                    if (p == null)
                        hasExited = true;
                    else
                        hasExited = p.HasExited;
                }
                catch (Exception e)
                {
                    Util.Logging.Log(e);
                    hasExited = false;
                }
                if (!hasExited)
                {
                    try
                    {
                        p.Kill();
                        count++;
                    }
                    catch(Exception e) 
                    {
                        Util.Logging.Log(e);
                    }
                }
            }
            return count;
        }

        public static int KillAllActiveProcesses(AccountType type)
        {
            int count = 0;

            try
            {
                var p = new ScanPaths();
                p.Add(type);
                count = Scan(p, ScanOptions.KillAll);
            }
            catch (Exception e)
            {
                Util.Logging.Log(e);
            }

            return count;
        }

        private static Account GetActiveAccount(string username)
        {
            foreach (var l in LinkedProcess.GetActive())
            {
                if (Util.Users.GetUserName(l.Account.Settings.WindowsAccount).Equals(username, StringComparison.OrdinalIgnoreCase))
                    return l.Account;
            }
            return null;
        }

        /// <summary>
        /// Returns any accounts with an active process
        /// </summary>
        public static List<Settings.IAccount> GetActiveProcesses()
        {
            var active = LinkedProcess.GetActive();
            List<Settings.IAccount> accounts = new List<Settings.IAccount>(active.Length);

            foreach (var l in active)
            {
                accounts.Add(l.Account.Settings);
            }

            return accounts;
        }

        public static Settings.IAccount GetAccountFromProcessId(int pid)
        {
            var a = LinkedProcess.GetAccount(pid);
            if (a != null)
                return a.Settings;
            return null;
        }

        /// <summary>
        /// Returns any accounts with an active status (active, updating, launching)
        /// </summary>
        public static List<Settings.IAccount> GetActiveStates()
        {
            lock (accounts)
            {
                var _accounts = new List<Settings.IAccount>(GetActiveProcessCount(AccountType.Any) + 1);
                foreach (var account in accounts.Values)
                {
                    if (account.IsActive || account.State == AccountState.Launching)
                        _accounts.Add(account.Settings);
                }
                return _accounts;
            }
        }

        public static bool IsUserActive(AccountType type, string username)
        {
            foreach (var l in LinkedProcess.GetActive())
            {
                if ((type == AccountType.Any || type.HasFlag(l.Account.Type)) && Util.Users.GetUserName(l.Account.Settings.WindowsAccount).Equals(username, StringComparison.OrdinalIgnoreCase))
                    return true;
            }
            return false;
        }

        private static bool IsWindowLockEnabled(Account account)
        {
            if (wlocker != null)
            {
                if (IsWindowed(account.Settings))
                    return (account.WindowOptions & Settings.WindowOptions.TopMost) == 0;
                else
                    return true;
            }
            return false;
        }

        private static bool IsWindowed(Settings.IAccount account)
        {
            return account.Windowed;// && !account.WindowBounds.IsEmpty;
        }

        /// <summary>
        /// Returns true if the account's login is automatically filled, remembered or otherwise
        /// </summary>
        private static bool IsAutomaticLogin(Settings.IAccount account)
        {
            if (!Settings.DisableAutomaticLogins.Value)
            {
                switch (account.Type)
                {
                    case Settings.AccountType.GuildWars2:

                        if (account.AutomaticLogin)
                        {
                            return account.HasCredentials;
                        }
                        else
                        {
                            return ((Settings.IGw2Account)account).AutomaticRememberedLogin || Settings.GuildWars2.AutomaticRememberedLogin.Value;
                        }

                    case Settings.AccountType.GuildWars1:

                        return account.AutomaticLogin && account.HasCredentials && !string.IsNullOrEmpty(((Settings.IGw1Account)account).CharacterName);

                }
            }

            return false;
        }

        private static void AppendAndEscapeCharArray(System.Text.StringBuilder output, char[] input, char[] needle)
        {
            int count = needle.Length;
            int last = 1;
            int[] i = new int[count];

            while (true)
            {
                int min = int.MaxValue, mini = -1;

                for (var j = 0; j < count; )
                {
                    var v = i[j];
                    if (v <= last)
                    {
                        v = Array.IndexOf<char>(input, needle[j], v) + 1;
                        if (v == 0)
                        {
                            if (--count > 0)
                            {
                                needle[j] = needle[count];
                                i[j] = i[count];

                                continue;
                            }
                            else
                            {
                                output.Append(input, last - 1, input.Length - last + 1);

                                return;
                            }
                        }
                        else
                        {
                            i[j] = v;
                            if (v < min)
                            {
                                min = v;
                                mini = j;
                            }
                        }
                    }
                    else if (v < min)
                    {
                        min = v;
                        mini = j;
                    }

                    ++j;
                }

                output.Append(input, last - 1, min - last);
                output.Append('\\');

                last = min;
            }
        }

        private static string GetArguments(Settings.IAccount account, string arguments, string arguments2, LaunchMode mode)
        {
            //warning: gw1 will stop parsing arguments on the first argument it doesn't know of

            StringBuilder args = new StringBuilder(256);

            if (mode == LaunchMode.Update || mode == LaunchMode.UpdateVisible)
            {
                args.Append(" -image");

                if (account.Type == Settings.AccountType.GuildWars2)
                {
                    if (Settings.MaxPatchConnections.HasValue)
                    {
                        args.Append(" -patchconnections ");
                        args.Append(Settings.MaxPatchConnections.Value);
                    }
                }

                if (!string.IsNullOrEmpty(arguments))
                {
                    string assetsrv = Util.Args.GetValue(arguments, "assetsrv");
                    string authsrv = Util.Args.GetValue(arguments, "authsrv");

                    if (!string.IsNullOrEmpty(assetsrv))
                    {
                        args.Append(" -assetsrv ");
                        args.Append(assetsrv);
                    }

                    if (!string.IsNullOrEmpty(authsrv))
                    {
                        args.Append(" -authsrv ");
                        args.Append(authsrv);
                    }
                }

                if (!string.IsNullOrEmpty(arguments2))
                {
                    args.Append(' ');
                    args.Append(arguments2);
                }

                if (mode != LaunchMode.UpdateVisible)
                {
                    args.Append(" -nopatchui");
                }
            }
            else
            {
                var disableAutologin = Settings.DisableAutomaticLogins.Value || account.Type == Settings.AccountType.GuildWars2 && account.AutomaticLogin && account.HasCredentials;

                #region -email -password -nopatchui (obsolete)

                //if (account.AutomaticLogin && account.HasCredentials)
                //{
                //    if (!Settings.DisableAutomaticLogins)
                //        args.Append(" -nopatchui");
                //    args.Append(" -email \"");
                //    args.Append(account.Email);
                //    args.Append("\" -password \"");
                //    var chars = Security.Credentials.ToCharArray(account.Password);
                //    try
                //    {
                //        AppendAndEscapeCharArray(args, chars, new char[] { '\\', '"' });
                //    }
                //    finally
                //    {
                //        Array.Clear(chars, 0, chars.Length);
                //    }
                //    args.Append('"');
                //}

                #endregion

                Settings.IAccountTypeSettings settings;

                if (account.Type == Settings.AccountType.GuildWars2)
                {
                    var gw2 = (Settings.IGw2Account)account;
                    settings = Settings.GuildWars2;
                    
                    if (!string.IsNullOrEmpty(account.Arguments))
                    {
                        if (Util.Args.Contains(account.Arguments, "dx11"))
                        {
                            arguments = Util.Args.AddOrReplace(arguments, "dx9", "");
                        }
                        else if (Util.Args.Contains(account.Arguments, "dx9"))
                        {
                            arguments = Util.Args.AddOrReplace(arguments, "dx11", "");
                        }

                        args.Append(' ');
                        if (disableAutologin)
                            args.Append(Util.Args.AddOrReplace(account.Arguments, "autologin", ""));
                        else
                            args.Append(account.Arguments);
                    }

                    if (gw2.Provider == Settings.AccountProvider.Steam)
                        args.Append(" -provider Steam");
                    else if (gw2.Proxy == Settings.LaunchProxy.Steam)
                        args.Append(" -provider Portal");

                    if (!disableAutologin && (gw2.AutomaticRememberedLogin || Settings.GuildWars2.AutomaticRememberedLogin.Value))
                        args.Append(" -autologin");

                    if (gw2.ClientPort != 0 || Settings.GuildWars2.ClientPort.HasValue)
                    {
                        args.Append(" -clientport ");
                        if (gw2.ClientPort != 0)
                            args.Append(gw2.ClientPort);
                        else
                            args.Append(Settings.GuildWars2.ClientPort.Value);
                    }

                    if (!string.IsNullOrEmpty(gw2.MumbleLinkName) || Settings.GuildWars2.MumbleLinkName.HasValue)
                    {
                        args.Append(" -mumble \"");
                        if (!string.IsNullOrEmpty(gw2.MumbleLinkName))
                            args.Append(gw2.MumbleLinkName);
                        else
                            args.Append(Settings.GuildWars2.MumbleLinkName.Value);
                        args.Append('"');
                    }
                }
                else if (account.Type == Settings.AccountType.GuildWars1)
                {
                    var gw1 = (Settings.IGw1Account)account;
                    settings = Settings.GuildWars1;

                    if (!string.IsNullOrEmpty(gw1.CharacterName))
                    {
                        if (!disableAutologin && account.AutomaticLogin && account.HasCredentials)
                        {
                            args.Append(" -email \"");
                            args.Append(account.Email);
                            args.Append("\" -password \"");
                            var chars = Security.Credentials.ToCharArray(account.Password.ToSecureString());
                            try
                            {
                                AppendAndEscapeCharArray(args, chars, new char[] { '\\', '"' });
                            }
                            finally
                            {
                                Array.Clear(chars, 0, chars.Length);
                            }
                            args.Append('"');
                        }

                        args.Append(" -character \"");
                        args.Append(gw1.CharacterName);
                        args.Append('"');
                    }
                }
                else
                {
                    throw new NotSupportedException();
                }

                if (IsWindowed(account))
                    args.Append(" -windowed");

                var mute = account.Mute;
                if (settings.Mute.HasValue)
                    mute |= settings.Mute.Value;

                if (mute != Settings.MuteOptions.None)
                {
                    if (mute.HasFlag(Settings.MuteOptions.All))
                    {
                        args.Append(" -nosound");
                    }
                    else if (account.Type == Settings.AccountType.GuildWars2)
                    {
                        if (mute.HasFlag(Settings.MuteOptions.Music))
                            args.Append(" -nomusic");
                        if (mute.HasFlag(Settings.MuteOptions.Voices))
                            args.Append(" -novoice");
                    }
                }

                if (account.ScreenshotsFormat == Settings.ScreenshotFormat.Bitmap || settings.ScreenshotsFormat.Value == Settings.ScreenshotFormat.Bitmap)
                    args.Append(" -bmp");

                if (!string.IsNullOrEmpty(arguments))
                {
                    args.Append(' ');
                    if (disableAutologin)
                        args.Append(Util.Args.AddOrReplace(arguments, "autologin", ""));
                    else
                        args.Append(arguments);
                }

                if (!string.IsNullOrEmpty(arguments2))
                {
                    args.Append(' ');
                    if (disableAutologin)
                        args.Append(Util.Args.AddOrReplace(arguments2, "autologin", ""));
                    else
                        args.Append(arguments2);
                }

                if (mode == LaunchMode.Launch)
                    args.Append(" -shareArchive"); //included in Gw1 accounts despite doing nothing - used to track shared/normal launches
            }

            args.Append(' ');
            args.Append(ARGS_UID);
            args.Append(account.UID);



            var a = args.ToString(1, args.Length - 1);

            if (mode == LaunchMode.LaunchSingle)
                a = Util.Args.AddOrReplace(a, "sharearchive", "");

            a = Variables.Replace(a, new Variables.DataSource(account, null));

            if (Net.AssetProxy.ServerController.Enabled)
            {
                if (account.Type == Settings.AccountType.GuildWars2)
                {
                    var proxy = Net.AssetProxy.ServerController.Active;
                    if (proxy != null)
                    {
                        try
                        {
                            int port = proxy.CurrentPort;
                            if (port != 0)
                                a = Util.Args.AddOrReplace(a, "assetsrv", "-assetsrv 127.0.0.1:" + port);
                        }
                        catch (Exception ex)
                        {
                            Util.Logging.Log(ex);
                        }
                    }
                }
            }

            return a;
        }

        private static ProcessOptions GetProcessStartInfo(QueuedLaunch q, FileManager.IProfileInformation customProfile, FileInfo fi)
        {
            var account = q.account.Settings;
            var options = new ProcessOptions();
            var appId = 0;

            options.FileName = fi.FullName;
            if (account.Type == Settings.AccountType.GuildWars2)
            {
                options.Arguments = GetArguments(account, Settings.GuildWars2.Arguments.Value, q.args, q.mode);
                if (((Settings.IGw2Account)account).Provider == Settings.AccountProvider.Steam)
                    appId = Steam.APPID_GW2;
            }
            else
            {
                options.Arguments = GetArguments(account, Settings.GuildWars1.Arguments.Value, q.args, q.mode);
            }
            options.WorkingDirectory = fi.DirectoryName;

            if (!Util.Users.IsCurrentUser(account.WindowsAccount))
            {
                options.UserName = account.WindowsAccount;
                var password = Security.Credentials.GetPassword(account.WindowsAccount);
                if (password == null)
                    throw new BadUsernameOrPasswordException();
                options.Password = password;
            }

            if (appId != 0)
            {
                options.Variables[ProcessOptions.VAR_STEAM_ID] = appId.ToString();
            }

            if (account.Proxy == Settings.LaunchProxy.None || q.disableProxy)
            {
                var temp = Path.Combine(DataPath.AppDataAccountDataTemp, account.UID.ToString());
                if (!Directory.Exists(temp))
                    Directory.CreateDirectory(temp);

                options.Variables[ProcessOptions.VAR_TEMP] = temp;

                if (customProfile != null)
                {
                    if (customProfile.UserProfile != null)
                        options.Variables[ProcessOptions.VAR_USERPOFILE] = customProfile.UserProfile;
                    if (customProfile.AppData != null)
                        options.Variables[ProcessOptions.VAR_APPDATA] = customProfile.AppData;

                    //Security.Impersonation.IImpersonationToken impersonation;
                    //string username = Util.Users.GetUserName(account.WindowsAccount);

                    //if (Util.Users.IsCurrentUser(username))
                    //    impersonation = null;
                    //else
                    //    impersonation = Security.Impersonation.Impersonate(username, Security.Credentials.GetPassword(username));

                    //try
                    //{
                    //    string appdata = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
                    //    string userprofile = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);

                    //    if (appdata.StartsWith(userprofile, StringComparison.OrdinalIgnoreCase))
                    //    {
                    //        string path = Path.Combine(appdata, "Guild Wars 2", account.DatFile.UID.ToString());

                    //        //GW2 only uses the userprofile variable
                    //        options.UserProfile = path;
                    //        options.AppData = Path.Combine(path, appdata.Substring(userprofile.Length + 1));
                    //    }
                    //    else
                    //        throw new Exception("Unknown user profile directory structure");
                    //}
                    //finally
                    //{
                    //    if (impersonation != null)
                    //        impersonation.Dispose();
                    //}
                }
                else
                {
                    //string appdata = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
                }
            }

            return options;
        }

        private static bool KillMutex(AccountType type)
        {
            bool killed = false;

            foreach (LinkedProcess p in LinkedProcess.GetActive())
            {
                try
                {
                    if (type.HasFlag(p.Account.Type))
                    {
                        if (p.HasMutex && p.KillMutex())
                        {
                            killed = true;
                        }
                    }
                }
                catch(Exception e)
                {
                    Util.Logging.Log(e);
                }
            }

            return killed;
        }

        private static bool IsMutexOpen(AccountType type)
        {
            var alive = false;

            Mutex mutex;

            if (type.HasFlag(AccountType.GuildWars1))
            {
                if (Mutex.TryOpenExisting("AN-Mutex-Window-Guild Wars", out mutex))
                {
                    alive = true;
                    mutex.Dispose();
                }
            }
            if (type.HasFlag(AccountType.GuildWars2))
            {
                if (Mutex.TryOpenExisting("AN-Mutex-Window-Guild Wars 2", out mutex))
                {
                    alive = true;
                    mutex.Dispose();
                }
            }

            return alive;
        }

        /// <summary>
        /// Scans for and recovers related processes, such as a process that restarts itself
        /// </summary>
        public static void Scan(AccountType type)
        {
            var paths = new ScanPaths();
            try
            {
                paths.Add(type);
            }
            catch (Exception e)
            {
                Util.Logging.Log(e);
            }

            if (paths.Count == 0)
                return;

            Scan(paths, ScanOptions.None);
        }

        /// <summary>
        /// Scans for processes matching the path
        /// </summary>
        /// <param name="paths">Paths to match to</param>
        /// <param name="options">Applicable scan options</param>
        /// <param name="modes">Launch modes to target when using the KillLinked option</param>
        /// <returns></returns>
        private static int Scan(ScanPaths paths, ScanOptions options, HashSet<LaunchMode> modes = null)
        {
            var startTime = DateTime.UtcNow;
            int counter = 0;

            Process[] ps;
            if (paths == null || paths.Count == 0)
                ps = new Process[0];
            else
                ps = Process.GetProcesses();

            if (ps.Length > 0)
            {
                using (var pi = new Windows.ProcessInfo())
                {
                    foreach (Process p in ps)
                    {
                        var used = false;

                        try
                        {
                            ScanPaths.Paths _paths;
                            if (!paths.TryGetValue(p.ProcessName, out _paths))
                                continue;

                            if (LinkedProcess.Contains(p))
                            {
                                switch (options)
                                {
                                    case ScanOptions.KillLinked:
                                    case ScanOptions.KillAll:

                                        var b = modes == null;

                                        if (!b && options == ScanOptions.KillLinked)
                                        {
                                            var a = LinkedProcess.GetAccount(p);
                                            if (a != null)
                                            {
                                                var s = a.Session;
                                                b = modes.Contains(s != null ? s.Mode : LaunchMode.Launch);
                                            }
                                        }

                                        if (b)
                                        {
                                            try
                                            {
                                                counter++;
                                                p.Kill();
                                            }
                                            catch (Exception e)
                                            {
                                                Util.Logging.Log(e);
                                            }
                                        }

                                        break;
                                }

                                continue;
                            }

                            string path,
                                   commandLine = null;

                            try
                            {
                                path = p.MainModule.FileName;
                            }
                            catch (Exception e)
                            {
                                Util.Logging.Log(e);

                                try
                                {
                                    if (p.HasExited)
                                    {
                                        counter++;
                                        continue;
                                    }
                                }
                                catch (Exception ex)
                                {
                                    Util.Logging.Log(ex);
                                }
                                path = null;
                            }

                            if (path == null)
                            {
                                using (var searcher = new ManagementObjectSearcher("SELECT ExecutablePath, CommandLine FROM Win32_Process WHERE ProcessId=" + p.Id))
                                {
                                    using (var results = searcher.Get())
                                    {
                                        foreach (ManagementObject o in results)
                                        {
                                            path = o["ExecutablePath"] as string;
                                            commandLine = o["CommandLine"] as string;
                                            break;
                                        }
                                    }
                                }

                                if (path == null)
                                {
                                    try
                                    {
                                        if (p.HasExited)
                                            counter++;
                                    }
                                    catch { }
                                    continue;
                                }
                            }
                            else if (pi.Open(p.Id))
                            {
                                commandLine = pi.GetCommandLine();
                            }

                            if (!_paths.ContainsMatch(Path.GetDirectoryName(path)))
                                continue;

                            bool isUnknown = false;

                            lock (accounts)
                            {
                                Account account = LinkedProcess.GetAccount(p);

                                //handle processes that haven't been linked yet
                                if (account == null)
                                {
                                    if (p.HasExited)
                                        continue;

                                    int uqid = -1;

                                    try
                                    {
                                        if (commandLine != null)
                                            uqid = GetUIDFromCommandLine(commandLine);
                                    }
                                    catch (Exception e)
                                    {
                                        Util.Logging.Log(e);
                                    }

                                    if (options == ScanOptions.KillAll || uqid != -1 && options == ScanOptions.KillLinked && modes == null)
                                    {
                                        counter++;
                                        try
                                        {
                                            p.Kill();
                                            continue;
                                        }
                                        catch { }
                                    }

                                    if (uqid == -1)
                                    {
                                        isUnknown = true;
                                    }
                                    else
                                    {
                                        ushort uid = (ushort)uqid;
                                        bool existing;

                                        if (!(existing = accounts.TryGetValue(uid, out account)))
                                        {
                                            if (Settings.Accounts.Contains(uid))
                                            {
                                                account = new Account(Settings.Accounts[uid].Value);
                                                account.Process.Changed += LinkedProcess_Changed;
                                                accounts.Add(account.Settings.UID, account);

                                                LaunchMode m;
                                                Account.LaunchSession s;

                                                if (Util.Args.Contains(commandLine, "shareArchive"))
                                                {
                                                    m = LaunchMode.Launch;
                                                }
                                                else if (Util.Args.Contains(commandLine, "image"))
                                                {
                                                    if (Util.Args.Contains(commandLine, "nopatchui"))
                                                        m = LaunchMode.Update;
                                                    else
                                                        m = LaunchMode.UpdateVisible;
                                                }
                                                else
                                                {
                                                    m = LaunchMode.LaunchSingle;
                                                }

                                                account.Session = s = new Account.LaunchSession(account, m);

                                                s.WindowOptions = account.Settings.WindowOptions;
                                                if (account.Type == AccountType.GuildWars2)
                                                {
                                                    s.MumbleLink = mumble.Add(Util.Args.GetValue(commandLine, "mumble"), p.Id, account.Settings);
                                                    s.MumbleLink.Verified += OnMumbleLinkVerified;
                                                }
                                                s.RunAfter = RunAfterManager.Create(account, false);
                                                
                                                //this program was exited before the account was
                                                //- assuming this process should trigger the exit event
                                                EventHandler<Account> onExit = null;
                                                onExit = delegate(object o, Account a)
                                                {
                                                    a.Exited -= onExit;
                                                    if (AccountExited != null)
                                                        AccountExited(a.Settings);
                                                };
                                                account.Exited += onExit;
                                            }
                                        }

                                        if (account != null)
                                        {
                                            used = true;

                                            lock (account)
                                            {
                                                if (!account.IsActive)
                                                    account.SetState(AccountState.Active, true, p);
                                                account.isRelaunch++;
                                            }

                                            if (!Util.Users.IsCurrentUser(account.Settings.WindowsAccount))
                                                Security.Impersonation.EnsureDefault();

                                            account.Process.Attach(p);

                                            if (options == ScanOptions.KillLinked && modes != null)
                                            {
                                                var s = account.Session;
                                                if (modes.Contains(s != null ? s.Mode : LaunchMode.Launch))
                                                {
                                                    counter++;
                                                    try
                                                    {
                                                        p.Kill();
                                                        continue;
                                                    }
                                                    catch { }
                                                }
                                            }

                                            switch (account.State)
                                            {
                                                case AccountState.Updating:
                                                case AccountState.UpdatingVisible:
                                                    break;
                                                default:

                                                    if (existing && Util.Args.Contains(commandLine, "isRelaunch")) //account was relaunched (logout)
                                                    {
                                                        if (account.Settings.Type == Settings.AccountType.GuildWars2 && Settings.GuildWars2.PreventRelaunching.Value != 0)
                                                        {
                                                            try
                                                            {
                                                                p.Kill();

                                                                if ((Settings.GuildWars2.PreventRelaunching.Value & Settings.RelaunchOptions.Relaunch) == Settings.RelaunchOptions.Relaunch)
                                                                {
                                                                    if (Monitor.TryEnter(queueLaunch, TIMEOUT_TRYENTER))
                                                                    {
                                                                        try
                                                                        {
                                                                            if (account.inQueueCount == 0)
                                                                            {
                                                                                ++account.inQueueCount;

                                                                                EventHandler<Account> onExit = null;
                                                                                var s = account.Session;

                                                                                onExit = delegate(object o, Account a)
                                                                                {
                                                                                    a.Exited -= onExit;

                                                                                    if (Monitor.TryEnter(queueLaunch, TIMEOUT_TRYENTER))
                                                                                    {
                                                                                        try
                                                                                        {
                                                                                            queueLaunch.Enqueue(new QueuedLaunch(a, s.Mode, s.Args));
                                                                                            if (AccountQueued != null)
                                                                                                AccountQueued(a.Settings, s.Mode);
                                                                                            StartQueue();
                                                                                        }
                                                                                        finally
                                                                                        {
                                                                                            Monitor.Exit(queueLaunch);
                                                                                        }
                                                                                    }
                                                                                    else
                                                                                    {
                                                                                        --a.inQueueCount;
                                                                                    }
                                                                                };

                                                                                account.Exited += onExit;
                                                                            }
                                                                        }
                                                                        finally
                                                                        {
                                                                            Monitor.Exit(queueLaunch);
                                                                        }
                                                                    }
                                                                }

                                                                break;
                                                            }
                                                            catch (Exception e)
                                                            {
                                                                Util.Logging.Log(e);
                                                            }
                                                        }
                                                        else if (account.Settings.PendingFiles //accounts with pending files should not be relaunched
                                                            || account.Settings.Type == Settings.AccountType.GuildWars2 && Settings.GuildWars2.ProfileMode.Value == Settings.ProfileMode.Basic //basic accounts may use files that no longer exist on exit
                                                            //|| account.Settings.NetworkAuthorizationState != Settings.NetworkAuthorizationState.Disabled && Settings.NetworkAuthorization.Value.HasFlag(Settings.NetworkAuthorizationFlags.RemoveAll)
                                                            || Util.Args.Contains(commandLine, "nopatchui")) //(obsolete) logging out with -nopatchui -email -password will cause the client to restart and log back in, except it can't and will sit on a black/white screen
                                                        {
                                                            try
                                                            {
                                                                p.Kill();
                                                                break;
                                                            }
                                                            catch (Exception e)
                                                            {
                                                                Util.Logging.Log(e);
                                                            }
                                                        }
                                                        else
                                                        {
                                                            var s = account.Session;
                                                            if (s == null || s.Mode == LaunchMode.Launch)
                                                            {
                                                                try
                                                                {
                                                                    account.Process.KillMutex();
                                                                }
                                                                catch { }
                                                            }
                                                        }
                                                    }

                                                    try
                                                    {
                                                        bool isWindowed = IsWindowed(account.Settings);
                                                        var watcher = account.Session.Watcher = new WindowWatcher(account, p, false, isWindowed, account.Session);
                                                        watcher.WindowChanged += OnWatchedWindowChanged;
                                                        watcher.WindowCrashed += OnWatchedWindowCrashed;
                                                        watcher.Start();

                                                        var events = windowEvents.Add(p.Id, account);
                                                        events.ForegroundChanged += events_ForegroundChanged;
                                                    }
                                                    catch (Exception ex)
                                                    {
                                                        Util.Logging.Log(ex);
                                                    }

                                                    break;
                                            }
                                        }
                                        else
                                        {
                                            isUnknown = true;
                                        }
                                    }
                                }
                                else
                                {
                                    if (options == ScanOptions.KillAll || options == ScanOptions.KillLinked && modes == null)
                                    {
                                        counter++;
                                        try
                                        {
                                            p.Kill();
                                            continue;
                                        }
                                        catch { }
                                    }
                                }
                            }

                            if (isUnknown)
                            {
                                string match;
                                if (_paths.TryGetMatch(path, out match))
                                {
                                    var type = _paths.GetType(match);

                                    lock (unknownProcesses)
                                    {
                                        if (!unknownProcesses.ContainsKey(p.Id))
                                        {
                                            used = true;
                                            unknownProcesses.Add(p.Id, new UnknownProcess(type, p));

                                            activeProcesses[GetIndex(type)]++;
                                            OnActiveProcessCountChanged(type);
                                            OnActiveProcessCountChanged();

                                            if (taskWatchUnknowns == null || taskWatchUnknowns.IsCompleted)
                                            {
                                                taskWatchUnknowns = new Task(DoWatch, TaskCreationOptions.LongRunning);
                                                taskWatchUnknowns.Start();
                                            }
                                        }
                                    }
                                }
                            }
                        }
                        catch (Exception e)
                        {
                            Util.Logging.Log(e);
                        }
                        finally
                        {
                            if (!used)
                                p.Dispose();
                        }
                    }
                }
            }

            OnScanComplete(startTime, options);

            lock (unknownProcesses)
            {
                var count = (ushort)(LinkedProcess.GetActiveCount() + unknownProcesses.Count);
                if (GetActiveProcessCount(AccountType.Any) != count)
                {
                    var counts = new ushort[activeProcesses.Length];

                    foreach (var u in unknownProcesses.Values)
                    {
                        counts[GetIndex(u.Type)]++;
                    }

                    foreach (var t in new AccountType[] { AccountType.GuildWars2, AccountType.GuildWars1 })
                    {
                        var i = GetIndex(t);

                        counts[i] = (ushort)LinkedProcess.GetActiveCount(t);

                        if (counts[i] != activeProcesses[i])
                        {
                            activeProcesses[i] = counts[i];
                            OnActiveProcessCountChanged(t);
                        }
                    }

                    OnActiveProcessCountChanged();
                }
            }

            return counter;
        }

        private static void DoScan()
        {
            DoScan(AccountType.Any, ScanOptions.None);
        }

        private static void DoScan(AccountType type, ScanOptions options, LaunchMode[] mode = null)
        {
            var paths = new ScanPaths();
            try
            {
                paths.Add(type);
            }
            catch (Exception e)
            {
                Util.Logging.Log(e);
            }

            bool ranOnce = false;

            while (true)
            {
                Account account = null;

                lock (queueExit)
                {
                    if (queueExit.Count > 0)
                    {
                        account = queueExit.Peek().account;
                    }
                    else if (ranOnce)
                    {
                        taskScan = null;
                        return;
                    }
                    ranOnce = true;
                }

                if (options == ScanOptions.None)
                {
                    var waitUntil = lastExit.AddMilliseconds(100);

                    ////when updating, the launcher closes and swaps the executable, which may take longer to restart
                    //if (account != null && account.State == AccountState.UpdatingVisible && account.isRelaunch == 0)
                    //    waitUntil = lastExit.AddMilliseconds(PROCESS_UPDATING_EXIT_DELAY);
                    //else
                    //    waitUntil = lastExit.AddMilliseconds(PROCESS_EXIT_DELAY);

                    while (DateTime.UtcNow < waitUntil)
                    {
                        Thread.Sleep(500);
                    }

                    Scan(paths, options);
                }
                else
                {
                    DateTime limit = DateTime.UtcNow.AddSeconds(5);
                    
                    HashSet<LaunchMode> modes;
                    if (mode != null && mode.Length > 0)
                        modes = new HashSet<LaunchMode>(mode);
                    else
                        modes = null;

                    do
                    {
                        var count = Scan(paths, options, modes);
                        if (count == 0)
                            break;
                    }
                    while (DateTime.UtcNow < limit);
                }
            }
        }

        private static void DoWatch()
        {
            while (true)
            {
                Process p = null;

                lock (unknownProcesses)
                {
                    if (unknownProcesses.Count == 0)
                    {
                        taskWatchUnknowns = null;

                        lock (queueExit)
                        {
                            lastExit = DateTime.UtcNow;

                            if (taskScan == null || taskScan.IsCompleted)
                            {
                                taskScan = new Task(DoScan);
                                taskScan.Start();
                            }
                        }

                        return;
                    }

                    foreach (var u in unknownProcesses.Values)
                    {
                        var hasExited = false;

                        try
                        {
                            hasExited = u.Process.HasExited;
                        }
                        catch(Exception e) 
                        {
                            Util.Logging.Log(e);
                        }

                        if (hasExited)
                        {
                            unknownProcesses.Remove(u.Process.Id);
                            break;
                        }
                        else
                        {
                            p = u.Process;
                            break;
                        }
                    }
                }

                if (p != null)
                {
                    try
                    {
                        p.WaitForExit();
                    }
                    catch (Exception e)
                    {
                        Util.Logging.Log(e);
                        Thread.Sleep(500);
                    }
                }
            }
        }

        private static void OnActiveProcessCountChanged(AccountType type)
        {
            if (ActiveProcessCountChanged != null)
                ActiveProcessCountChanged(type, GetActiveProcessCount(type));

            if (GetActiveProcessCount(type) == 0)
            {
                switch (type)
                {
                    case AccountType.GuildWars2:

                        FileManager.Deactivate(Settings.AccountType.GuildWars2);

                        break;
                    case AccountType.GuildWars1:

                        FileManager.Deactivate(Settings.AccountType.GuildWars1);

                        break;
                }

                if (type == AccountType.GuildWars2)
                {
                    lock (queueLaunch)
                    {
                        if (coherentLock != null)
                        {
                            coherentLock.Dispose();
                            coherentLock = null;
                        }
                    }
                }
            }
        }

        private static void OnActiveProcessCountChanged()
        {
            if (AnyActiveProcessCountChanged != null)
                AnyActiveProcessCountChanged(AccountType.Any, GetActiveProcessCount(AccountType.Any));

            if (AllQueuedLaunchesCompleteAllAccountsExited != null && GetActiveProcessCount(AccountType.Any) == 0)
            {
                lock (queue)
                {
                    if (GetPendingLaunchCount() == 0)
                    {
                        if (taskQueue == null || taskQueue.IsCompleted)
                            AllQueuedLaunchesCompleteAllAccountsExited(null, EventArgs.Empty);
                    }
                }
            }
        }

        private static void OnScanComplete(DateTime startTime, ScanOptions options)
        {
            lock (queueExit)
            {
                while (queueExit.Count > 0)
                {
                    var q = queueExit.Peek();

                    if (options == ScanOptions.None && q.account.State == AccountState.UpdatingVisible && q.account.isRelaunch == 0)
                    {
                        //when initially updating, the launcher closes to update its executable, then relaunches
                        if (q.exitTime < startTime) //(q.exitTime.AddMilliseconds(PROCESS_UPDATING_EXIT_DELAY) < startTime)
                            queueExit.Dequeue();
                        else
                            break;
                    }
                    else if (q.exitTime < startTime) //(q.exitTime.AddMilliseconds(PROCESS_EXIT_DELAY) < startTime)
                        queueExit.Dequeue();
                    else
                        break;

                    if (q.account.Process.Process == null)
                    {
                        OnAccountExiting(q.account);

                        if (Monitor.TryEnter(queueLaunch, TIMEOUT_TRYENTER))
                        {
                            try
                            {
                                lock (q.account)
                                {
                                    TimeSpan runtime;
                                    try
                                    {
                                        runtime = q.process.ExitTime.Subtract(q.process.StartTime);
                                    }
                                    catch (Exception e)
                                    {
                                        Util.Logging.Log(e);
                                        runtime = TimeSpan.MinValue;
                                    }

                                    q.account.SetState(AccountState.Exited, true, runtime);
                                    if (q.account.inQueueCount > 0)
                                    {
                                        q.account.SetState(AccountState.None, true);
                                        q.account.SetState(AccountState.Waiting, true);
                                    }
                                    else
                                    {
                                        q.account.SetState(AccountState.None, true);
                                    }
                                }
                            }
                            finally
                            {
                                Monitor.Exit(queueLaunch);
                            }
                        }
                        else
                        {
                            lock (q.account)
                            {
                                TimeSpan runtime;
                                try
                                {
                                    runtime = q.process.ExitTime.Subtract(q.process.StartTime);
                                }
                                catch (Exception e)
                                {
                                    Util.Logging.Log(e);
                                    runtime = TimeSpan.MinValue;
                                }
                                q.account.SetState(AccountState.Exited, true, runtime);
                                q.account.SetState(AccountState.None, true);
                            }
                        }

                        OnAccountExited(q.account);
                    }
                }
            }
        }

        private static void OnAccountExiting(Account account)
        {
            account.Session = null;
            FileManager.Deactivate(account.Settings);
        }

        private static void OnAccountExited(Account account)
        {
            account.OnExited();
        }

        private static void OnProcessExited(Process process, Account account)
        {
            lock (queueExit)
            {

                queueExit.Enqueue(new QueuedExit(account, lastExit = DateTime.UtcNow, process));

                if (taskScan == null || taskScan.IsCompleted)
                {
                    taskScan = new Task(DoScan);
                    taskScan.Start();
                }
            }

            if (coherentMonitor != null)
                coherentMonitor.Remove(account.Settings);

            try
            {
                windowEvents.Remove(process.Id);
            }
            catch (Exception e)
            {
                Util.Logging.Log(e);
            }

            if (AccountProcessExited != null)
                AccountProcessExited(account.Settings, process);
        }

        private static void OnWatchedWindowChanged(object sender, WindowWatcher.WindowChangedEventArgs e)
        {
            var watcher = (WindowWatcher)sender;
            var account = watcher.Account;
            var settings = account.Settings;
            var s = watcher.Session;

            switch (e.Type)
            {
                case WindowWatcher.WindowChangedEventArgs.EventType.HandleChanged:

                    if (Settings.WindowIcon.Value)
                    {
                        SetIcons(e.Handle, account, s);
                    }

                    break;
                case WindowWatcher.WindowChangedEventArgs.EventType.TitleChanged:

                    if (Settings.WindowCaption.HasValue)
                    {
                        WindowWatcher.SetText(e.Handle, Variables.Replace(Settings.WindowCaption.Value, new Variables.DataSource(settings, watcher.Process)));
                    }

                    break;
                case WindowWatcher.WindowChangedEventArgs.EventType.LauncherWindowLoaded:

                    if (settings.Type == Settings.AccountType.GuildWars2)
                    {
                        var l = s.Limiter;
                        if (l != null)
                        {
                            l.SetTime();
                        }

                        if (Settings.HideInitialWindow.Value)
                        {
                            s.Hidden = watcher.CreateHidden();
                        }

                        if (!e.WasAlreadyStarted)
                        {
                            var gw2 = (Settings.IGw2Account)settings;
                            var automatic = watcher.ProcessOptions != null && Util.Args.Contains(watcher.ProcessOptions.Arguments, "autologin");
                            Autologin.EventAction? action = null;
                            
                            if (watcher.SupportsLoginEvents)
                            {
                                if (settings.AutomaticLogin && settings.HasCredentials)
                                {
                                    action = Autologin.EventAction.Login;
                                }
                                else if (automatic)
                                {
                                    action = Autologin.EventAction.Autologin;
                                }
                            }
                            else if (settings.AutomaticLogin && settings.HasCredentials || !Settings.DisableAutomaticLogins.Value && (gw2.AutomaticPlay || automatic))
                            {
                                action = Autologin.EventAction.Auto;
                            }

                            if (action.HasValue)
                            {
                                autologin.Queue(account, watcher.Process, action.Value, automatic);
                            }
                        }
                    }

                    RunAfter(Settings.RunAfter.RunAfterWhen.LoadedLauncher, account);

                    break;
                case WindowWatcher.WindowChangedEventArgs.EventType.LauncherLoginCodeRequired:

                    if ((settings.NetworkAuthorization & Settings.NetworkAuthorizationOptions.Enabled) != 0 && settings.TotpKey != null)
                    {
                        autologin.Queue(account, watcher.Process, Autologin.EventAction.Totp);
                    }

                    break;
                case WindowWatcher.WindowChangedEventArgs.EventType.LauncherLoginComplete:

                    if (settings.Type == Settings.AccountType.GuildWars2)
                    {
                        var gw2 = (Settings.IGw2Account)settings;
                        if (!Settings.DisableAutomaticLogins.Value && gw2.AutomaticPlay)
                        {
                            autologin.Queue(account, watcher.Process, Autologin.EventAction.Play);
                        }
                    }

                    break;
                case WindowWatcher.WindowChangedEventArgs.EventType.LauncherLoginError:

                    if (Settings.LaunchTimeout.Value > 0)
                        OnWatchedWindowTimeout(watcher, null);

                    break;
                case WindowWatcher.WindowChangedEventArgs.EventType.DxWindowHandleCreated:

                    if (s.Hidden != null)
                    {
                        s.Hidden.Hide(e.Handle);
                    }

                    if (!e.WasAlreadyStarted)
                    {
                        if (settings.Type == Settings.AccountType.GuildWars2)
                        {
                            if (Settings.GuildWars2.DxLoadingPriority.HasValue)
                                SetPriority(watcher.Process, Settings.GuildWars2.DxLoadingPriority.Value, false);
                        }

                        if (IsWindowLockEnabled(account))
                        {
                            WindowLock.ToBottom(e.Handle, false);
                        }
                    }

                    break;
                case WindowWatcher.WindowChangedEventArgs.EventType.DxWindowStyleChanged:

                    if (!e.WasAlreadyStarted && Settings.RepaintInitialWindow.Value)
                    {
                        RepaintWindow(e.Handle);
                    }

                    break;
                case WindowWatcher.WindowChangedEventArgs.EventType.DxWindowCreated:

                    {
                        var l = s.Limiter;
                        if (l != null)
                        {
                            l.Release(true);
                            s.Limiter = null;
                        }
                    }

                    if (!e.WasAlreadyStarted && IsWindowLockEnabled(account))
                    {
                        WindowLock.ToBottom(e.Handle, false);
                    }

                    if (!e.WasAlreadyStarted && Settings.RepaintInitialWindow.Value)
                    {
                        RepaintWindow(e.Handle);
                    }

                    break;
                case WindowWatcher.WindowChangedEventArgs.EventType.DxWindowInitialized:

                    if (settings.VolumeEnabled)
                        watcher.SetVolume(settings.Volume);
                    else if (Settings.GetSettings(settings.Type).Volume.HasValue)
                        watcher.SetVolume(Settings.GetSettings(settings.Type).Volume.Value);

                    WindowEvents.Events events;

                    try
                    {
                        events = windowEvents.Add(watcher.Process.Id, account);
                        events.MoveSizeEnd += events_MoveSizeEnd;
                        events.MoveSizeBegin += events_MoveSizeBegin;
                        events.MinimizeEnd += events_MinimizeEnd;
                        events.MinimizeStart += events_MinimizeStart;
                        events.HandleChanged += events_HandleChanged;
                    }
                    catch (Exception ex)
                    {
                        events = null;
                        Util.Logging.Log(ex);
                    }

                    watcher.Events = events;

                    if (settings.Type == Settings.AccountType.GuildWars2)
                    {
                        if (Settings.GuildWars2.PrioritizeCoherentUI.Value)
                        {
                            if (coherentMonitor == null)
                            {
                                lock (queue)
                                {
                                    if (coherentMonitor == null)
                                        coherentMonitor = new Tools.CoherentMonitor();
                                }
                            }

                            coherentMonitor.Add(settings, watcher.Process);
                        }
                    }

                    if (AccountWindowEvent != null)
                        AccountWindowEvent(settings, new AccountWindowEventEventArgs(AccountWindowEventEventArgs.EventType.WindowReady, watcher.Process, e.Handle));

                    break;
                case WindowWatcher.WindowChangedEventArgs.EventType.DxWindowLoaded:

                    if (settings.ProcessPriority != Settings.ProcessPriorityClass.None || Settings.GetSettings(settings.Type).ProcessPriority.HasValue)
                        SetPriority(watcher.Process, settings.ProcessPriority == Settings.ProcessPriorityClass.None ? Settings.GetSettings(settings.Type).ProcessPriority.Value : settings.ProcessPriority);
                    else
                        SetPriority(watcher.Process, Settings.ProcessPriorityClass.None, false);

                    var affinity = settings.ProcessAffinity;
                    if (affinity == 0 && Settings.GetSettings(settings.Type).ProcessAffinity.HasValue)
                        affinity = Settings.GetSettings(settings.Type).ProcessAffinity.Value;
                    if (affinity > 0)
                    {
                        try
                        {
                            var processors = Environment.ProcessorCount;
                            watcher.Process.ProcessorAffinity = (IntPtr)(affinity & (long)(processors >= 64 ? ulong.MaxValue : ((ulong)1 << processors) - 1));
                        }
                        catch (Exception ex)
                        {
                            Util.Logging.Log(ex);
                        }
                    }

                    OnWindowStateChanged(e.Handle, account);
                    
                    var m = s.MumbleLink;
                    if (m != null)
                    {
                        if (!e.WasAlreadyStarted)
                        {
                            m.Verified += OnRunAfterCharacterLoaded;
                        }
                        m.Verify();
                    }

                    if (Settings.WindowIcon.Value)
                    {
                        SetIcons(e.Handle, account, s);
                    }

                    var hw = s.Hidden;

                    if (!e.WasAlreadyStarted)
                    {
                        if (IsWindowed(settings))
                        {
                            var wt = account.WindowTemplate;
                            var r = wt != null ? wt.Bounds : settings.WindowBounds;

                            if (!r.IsEmpty)
                            {
                                if (hw != null)
                                    hw.DisposeAfter(5000); //in case loading stalls

                                watcher.SetBounds(account.Settings, watcher.Process, e.Handle, r, 1000, watcher.Events,
                                    delegate(IntPtr w)
                                    {
                                        OnWindowStateChanged(w, account);
                                    },
                                    delegate(IntPtr w)
                                    {
                                        s.Hidden = null;
                                    });
                            }
                        }

                        if (IsWindowLockEnabled(account))
                        {
                            var b = IsWindowed(settings);
                            RECT r;

                            //full screen windows will try to focus on load - only moving full screen windows if another window is overlapping it
                            if (!b && NativeMethods.GetWindowRect(e.Handle, out r))
                            {
                                var bounds1 = r.ToRectangle();
                                var active = LinkedProcess.GetActive();
                                foreach (var a in active)
                                {
                                    if (a.Account.State == AccountState.ActiveGame && a.Account != account)
                                    {
                                        try
                                        {
                                            var h = Windows.FindWindow.FindMainWindow(a.Account.Process.Process);
                                            if (h != IntPtr.Zero && NativeMethods.GetWindowRect(h, out r))
                                            {
                                                var bounds2 = r.ToRectangle();
                                                if (bounds1.Contains(bounds2) || bounds1.IntersectsWith(bounds2))
                                                {
                                                    b = true;

                                                    break;
                                                }
                                            }
                                        }
                                        catch { }
                                    }
                                }

                                if (!b && e.Handle == NativeMethods.GetForegroundWindow())
                                {
                                    //the window is focused, but was in the background and may appear behind the taskbar
                                    WindowLock.ResetForegroundWindowFocus(e.Handle);
                                }
                            }

                            if (b)
                            {
                                WindowLock.ToBackground(e.Handle, true, false);
                            }
                        }
                    }

                    lock (account)
                    {
                        if (account.State == AccountState.Active)
                            account.SetState(AccountState.ActiveGame, true);
                    }

                    s.GfxLock = null;
                    if (hw != null && !hw.Disposing)
                        s.Hidden = null;

                    RunAfter(Settings.RunAfter.RunAfterWhen.LoadedCharacterSelect, account);

                    if (AccountWindowEvent != null)
                        AccountWindowEvent(settings, new AccountWindowEventEventArgs(AccountWindowEventEventArgs.EventType.WindowLoaded, watcher.Process, e.Handle));

                    break;
                case WindowWatcher.WindowChangedEventArgs.EventType.WatcherExited:

                    {
                        var l = s.Limiter;
                        if (l != null)
                        {
                            l.Release();
                            s.Limiter = null;
                        }
                    }

                    if (settings.Type == Settings.AccountType.GuildWars2)
                    {
                        if (Settings.DeleteCacheOnLaunch.Value)
                        {
                            Task.Run(new Action(
                                delegate
                                {
                                    try
                                    {
                                        Tools.Gw2Cache.Delete(settings.UID);
                                    }
                                    catch (Exception ex)
                                    {
                                        Util.Logging.Log(ex);
                                    }
                                }));
                        }
                    }

                    lock (s)
                    {
                        var h = s.Hidden;
                        if (h != null && !h.Disposing)
                            s.Hidden = null;
                        s.Watcher = null;
                        s.GfxLock = null;
                        s.Limiter = null;

                        if (account.Session != s)
                        {
                            s.Dispose();
                        }
                    }

                    break;
            }
        }

        static void OnRunAfterCharacterLoaded(object sender, EventArgs e)
        {
            var l = (Tools.Mumble.MumbleMonitor.IMumbleProcess)sender;
            l.Verified -= OnRunAfterCharacterLoaded;

            RunAfter(Settings.RunAfter.RunAfterWhen.LoadedCharacter, GetAccount(l.Account));
        }

        static void OnMumbleLinkVerified(object sender, EventArgs e)
        {
            if (MumbleLinkVerified != null)
            {
                var l = (Tools.Mumble.MumbleMonitor.IMumbleProcess)sender;

                MumbleLinkVerified(l.Account, l);
            }
        }

        /// <summary>
        /// Sets the priority of the process
        /// </summary>
        /// <param name="monitor">True to ensure the priority is kept (GW2 will reset its priority to normal), False to only apply once</param>
        public static void SetPriority(Process p, Settings.ProcessPriorityClass priority, bool monitor = true)
        {
            if (monitor && processPriority == null)
            {
                if (priority == Settings.ProcessPriorityClass.None || priority == Settings.ProcessPriorityClass.Normal)
                    return;

                lock (queue)
                {
                    if (processPriority == null)
                        processPriority = new Tools.ProcessPriority();
                }
            }

            try
            {
                if (monitor)
                {
                    processPriority.SetPriority(p, GetPriority(priority));
                }
                else
                {
                    p.PriorityClass = GetPriority(priority);
                }
            }
            catch (Exception ex)
            {
                Util.Logging.Log(ex);
            }
        }

        /// <summary>
        /// Returns the monitored priority of the process, or the current priority if it's not monitored
        /// </summary>
        public static ProcessPriorityClass GetPriority(Process p)
        {
            if (processPriority == null)
            {
                try
                {
                    return p.PriorityClass;
                }
                catch
                {
                    return ProcessPriorityClass.Normal;
                }
            }
            else
            {
                return processPriority.GetPriority(p);
            }
        }

        private static void SetIcons(IntPtr h, Account a, Account.LaunchSession s)
        {
            try
            {
                Tools.Icons icons;
                lock (s)
                {
                    if (s.IsDisposed)
                        return;
                    icons = s.Icons;
                }
                var settings = a.Settings;

                if (icons == null)
                {
                    switch (settings.IconType)
                    {
                        case Settings.IconType.File:
                            icons = Tools.Icons.From(settings.Icon);
                            break;
                        case Settings.IconType.ColorKey:
                        case Settings.IconType.Gw2LauncherColorKey:
                            var colorKey = settings.ColorKey;
                            if (colorKey.IsEmpty)
                                colorKey = Util.Color.FromUID(settings.UID);
                            icons = Tools.Icons.From(colorKey, settings.IconType == Settings.IconType.Gw2LauncherColorKey);
                            break;
                        case Settings.IconType.None:
                        default:
                            icons = null;
                            break;
                    }
                }

                if (icons != null)
                {
                    lock (s)
                    {
                        if (s.IsDisposed)
                        {
                            icons.Dispose();
                            return;
                        }
                        else
                        {
                            s.Icons = icons;
                        }
                    }

                    WindowWatcher.SetIcons(h, icons);

                    //NativeMethods.SendMessage(h, 0x0080, (IntPtr)0, icons.Small.Handle);
                    //NativeMethods.SendMessage(h, 0x0080, (IntPtr)1, icons.Big.Handle);
                }
            }
            catch (Exception ex)
            {
                Util.Logging.Log(ex);
            }
        }

        static void RepaintWindow(IntPtr handle)
        {
            try
            {
                if (!NativeMethods.IsWindow(handle))
                    return;

                using (var g = System.Drawing.Graphics.FromHwnd(handle))
                {
                    g.Clear(System.Drawing.Color.Black);
                }
            }
            catch (Exception ex)
            {
                Util.Logging.Log(ex);
            }
        }

        static void events_MinimizeStart(object sender, Launcher.WindowEvents.WindowEventsEventArgs e)
        {
            if (AccountWindowEvent != null)
                AccountWindowEvent(e.Account.Settings, new AccountWindowEventEventArgs(AccountWindowEventEventArgs.EventType.Minimized, e.Account.Process.Process, e.Handle));
        }

        static void events_MinimizeEnd(object sender, Launcher.WindowEvents.WindowEventsEventArgs e)
        {
            OnWindowStateChanged(e.Handle, e.Account);
        }

        static void OnWindowStateChanged(IntPtr handle, Account account)
        {
            var options = account.WindowOptions;

            if ((options & Settings.WindowOptions.Windowed) == Settings.WindowOptions.Windowed)
            {
                try
                {
                    if ((options & Settings.WindowOptions.PreventChanges) == Settings.WindowOptions.PreventChanges)
                        Windows.WindowLong.Remove(handle, GWL.GWL_STYLE, WindowStyle.WS_MAXIMIZEBOX);

                    if ((options & Settings.WindowOptions.TopMost) == Settings.WindowOptions.TopMost && account.State == AccountState.ActiveGame && !WindowWatcher.HasTopMost(handle))
                    {
                        bool handled;

                        if (AccountTopMostWindowEvent != null)
                        {
                            var we = new AccountTopMostWindowEventEventArgs(account.Process.Process, handle);
                            AccountTopMostWindowEvent(account.Settings, we);

                            if (handled = we.Count > 0)
                            {
                                handled = WindowWatcher.SetTopMost(handle, we.Windows);
                            }

                            if (AccountWindowEvent != null)
                            {
                                AccountWindowEvent(account.Settings, we);
                                handled = handled || we.Handled;
                            }
                        }
                        else if (AccountWindowEvent != null)
                        {
                            var we = new AccountWindowEventEventArgs(AccountWindowEventEventArgs.EventType.TopMost, account.Process.Process, handle);
                            AccountWindowEvent(account.Settings, we);
                            handled = we.Handled;
                        }
                        else
                            handled = false;

                        if (!handled)
                            WindowWatcher.SetTopMost(handle);
                    }
                }
                catch (Exception ex)
                {
                    Util.Logging.Log(ex);
                }
            }
        }

        static void events_MoveSizeBegin(object sender, WindowEvents.WindowEventsEventArgs e)
        {
            var options = e.Account.WindowOptions;

            if ((options & Settings.WindowOptions.PreventChanges) == Settings.WindowOptions.PreventChanges)
            {
                try
                {
                    NativeMethods.PostMessage(e.Handle, (uint)WindowMessages.WM_CANCELMODE, IntPtr.Zero, IntPtr.Zero);
                    //NativeMethods.PostMessage(e.Handle, 0x0100, 0x1B, 0); //WM_KEYDOWN (VK_ESCAPE)
                }
                catch (Exception ex)
                {
                    Util.Logging.Log(ex);
                }
            }
        }

        static void events_MoveSizeEnd(object sender, WindowEvents.WindowEventsEventArgs e)
        {
            var options = e.Account.WindowOptions;

            if ((options & Settings.WindowOptions.Windowed) == Settings.WindowOptions.Windowed)
            {
                var t = e.Account.WindowTemplate;
                var handle = e.Handle;
                var changes = options & (Settings.WindowOptions.PreventChanges | Settings.WindowOptions.RememberChanges);

                if (changes != Settings.WindowOptions.None)
                {
                    try
                    {
                        var bounds = t != null ? t.Bounds : e.Account.Settings.WindowBounds;

                        switch (changes)
                        {
                            case Settings.WindowOptions.RememberChanges:

                                if (t == null)
                                {
                                    var placement = new WINDOWPLACEMENT();

                                    if (NativeMethods.GetWindowPlacement(handle, ref placement))
                                    {
                                        var r = placement.rcNormalPosition.ToRectangle();

                                        if (bounds != r)
                                        {
                                            e.Account.Settings.WindowBounds = r;
                                        }
                                    }
                                }

                                break;
                            case Settings.WindowOptions.PreventChanges | Settings.WindowOptions.RememberChanges:
                            case Settings.WindowOptions.PreventChanges:

                                //this is only a backup and will only happen if the cancel message failed
                                //note that the window bounds cannot be set until the actual operation finishes

                                if (!bounds.IsEmpty)
                                {
                                    WindowWatcher.SetBounds(e.Account.Settings, e.Account.Process.Process, handle, bounds, 5000, (WindowEvents.Events)sender,
                                        delegate(IntPtr w)
                                        {
                                            OnWindowStateChanged(w, e.Account);
                                        },
                                        null, true, (options & Settings.WindowOptions.TopMost) == Settings.WindowOptions.TopMost);
                                }

                                break;
                        }
                    }
                    catch (Exception ex)
                    {
                        Util.Logging.Log(ex);
                    }
                }

                //needs to be called after MoveSizeEnd due to how GW2 resets its window (notably top most) when it changes (moved, resized, etc)
                DelayedWindowStateChanged(1, handle, e.Account);
            }
        }

        private static async void DelayedWindowStateChanged(int delay, IntPtr handle, Account a)
        {
            await Task.Delay(delay);
            OnWindowStateChanged(handle, a);
        }

        /// <summary>
        /// Triggers a window state change if the window isn't top most (GW2 will reset top most after the window is moved)
        /// </summary>
        private static async void DelayedWindowStateChangedTopMost(int delay, IntPtr handle, Account a)
        {
            await Task.Delay(delay);
            if (!WindowWatcher.HasTopMost(handle))
                OnWindowStateChanged(handle, a);
        }

        static void events_ForegroundChanged(object sender, WindowEvents.WindowEventsEventArgs e)
        {
            if (IsWindowed(e.Account.Settings))
            {
                OnWindowStateChanged(e.Handle, e.Account);
            }

            if (AccountWindowEvent != null)
                AccountWindowEvent(e.Account.Settings, new AccountWindowEventEventArgs(AccountWindowEventEventArgs.EventType.Focused, e.Account.Process.Process, e.Handle));
        }

        static void events_HandleChanged(object sender, Launcher.WindowEvents.WindowEventsEventArgs e)
        {
            var buffer = new char[2];
            if (NativeMethods.GetClassName(e.Handle, buffer, 2) > 0 && buffer[0] == '#')
            {
                try
                {
                    var we = (WindowEvents.Events)sender;
                    windowEvents.Remove(we.Account.Process.Process.Id);
                }
                catch { }
            }
        }

        private static void OnUpdateRequired(Account account, LaunchMode mode, string args, bool onlyShowIfNoActiveProcesses, string messsageUpdateRequired)
        {
            WaitForScanner();

            Action<Account, bool> error = delegate(Account _account, bool queued)
            {
                if (_account.inQueueCount == 0 && (queued && _account.State == AccountState.Waiting || !queued && _account.State == AccountState.None) || queued && _account.inQueueCount == 1 && _account.State == AccountState.Waiting)
                {
                    if (Monitor.TryEnter(_account, TIMEOUT_TRYENTER))
                    {
                        try
                        {
                            _account.SetState(AccountState.Error, true, new Exception(messsageUpdateRequired));
                            _account.SetState(AccountState.None, false);
                        }
                        finally
                        {
                            Monitor.Exit(_account);
                        }
                    }
                }
            };

            Action<Account, LaunchMode, string> queue = delegate(Account _account, LaunchMode _mode, string _args)
            {
                account.inQueueCount++;
                var _ql = new QueuedLaunch(_account, _mode, _args);
                _ql.Dequeued += delegate(object o, QueuedLaunch.DequeuedState state)
                {
                    if (state == QueuedLaunch.DequeuedState.Skipped)
                        error(_account, true);
                };
                queueLaunch.Enqueue(_ql);
                if (AccountQueued != null)
                    AccountQueued(_account.Settings, _mode);
            };

            bool retry = false;

            lock (queueLaunch)
            {
                if (onlyShowIfNoActiveProcesses && GetActiveProcessCount(account.Type) == 0 || !onlyShowIfNoActiveProcesses)
                {
                    if (cancelQueue == null || taskQueue == null || !cancelQueue.IsCancellationRequested)
                    {
                        if (Monitor.TryEnter(accounts, TIMEOUT_TRYENTER))
                        {
                            try
                            {
                                account.errors++;
                            }
                            finally
                            {
                                Monitor.Exit(accounts);
                            }
                        }

                        var ql = GetQueuedUpdate(account);

                        EventHandler<QueuedLaunch.DequeuedState> onDequeue = delegate(object o, QueuedLaunch.DequeuedState state)
                        {
                            lock (queueLaunch)
                            {
                                if (state == QueuedLaunch.DequeuedState.OK)
                                    queue(account, mode, args);
                                else
                                    error(account, true);
                            }
                        };

                        if (ql != null)
                        {
                            ql.Dequeued += onDequeue;
                            retry = true;
                        }
                        else if (account.errors == 1) //account.inQueueCount == 0 && 
                        {
                            ql = new QueuedLaunch(null, LaunchMode.Update);
                            ql.Dequeued += onDequeue;
                            queueLaunch.Push(ql);
                            retry = true;
                        }

                        if (retry && Monitor.TryEnter(account, TIMEOUT_TRYENTER))
                        {
                            try
                            {
                                if (account.State == AccountState.None)
                                    account.SetState(AccountState.Waiting, true);
                            }
                            finally
                            {
                                Monitor.Exit(account);
                            }
                        }
                    }
                }

                if (!retry)
                    error(account, false);
            }

            StartQueue();
        }

        private static void OnWatchedWindowLoginComplete(object sender, int duration)
        {
            try
            {
                var watcher = (WindowWatcher)sender;
                var account = watcher.Account;

                var l = account.Session.Limiter;
                if (l != null)
                {
                    l.Release(duration);
                }
            }
            catch { }
        }

        private static void OnWatchedWindowTimeout(object sender, WindowWatcher.TimeoutEventArgs e)
        {
            try
            {
                var watcher = (WindowWatcher)sender;
                var account = watcher.Account;
                var p = account.Process.Process;

                if (e != null)
                {
                    if (e.Reason == WindowWatcher.TimeoutEventArgs.TimeoutReason.Launcher && !IsAutomaticLogin(account.Settings))
                        return;
                    e.Handled = true;
                }

                if (p != null && !p.HasExited)
                {
                    p.Kill();
                    p.WaitForExit();
                }

                WaitForScanner();

                lock (queueLaunch)
                {
                    if (account.inQueueCount == 0)
                    {
                        if (Monitor.TryEnter(account, TIMEOUT_TRYENTER))
                        {
                            try
                            {
                                account.SetState(AccountState.Waiting, true);
                            }
                            finally
                            {
                                Monitor.Exit(account);
                            }
                        }

                        account.inQueueCount++;
                        queueLaunch.Enqueue(new QueuedLaunch(account, watcher.Session.Mode, watcher.Session.Args));
                        if (AccountQueued != null)
                            AccountQueued(account.Settings, watcher.Session.Mode);

                        StartQueue();
                    }
                }
            }
            catch (Exception ex)
            {
                Util.Logging.Log(ex);
            }
        }

        private static void OnWatchedWindowCrashed(object sender, WindowWatcher.CrashReason e)
        {
            try
            {
                var watcher = (WindowWatcher)sender;
                var account = watcher.Account;
                var p = account.Process.Process;

                switch (e)
                {
                    case WindowWatcher.CrashReason.NoPatchUI:

                        OnUpdateRequired(watcher.Account, watcher.Session.Mode, watcher.Session.Args, true, "An update may be required; client exited unexpectedly");

                        break;
                    case WindowWatcher.CrashReason.PatchRequired:

                        if (p != null && !p.HasExited)
                        {
                            p.Kill();
                            p.WaitForExit();

                            OnUpdateRequired(watcher.Account, watcher.Session.Mode, watcher.Session.Args, false, "An update is required; client is out of date");
                        }

                        break;
                    case WindowWatcher.CrashReason.ErrorDialog:

                        //unknown error - GW2 needs to be patched or Local.dat/Gw2.dat can't be accessed
                        //assuming a patch is required

                        if (p != null && !p.HasExited && account.errors == 0 && watcher.Session.Mode == LaunchMode.Launch)
                        {
                            p.Kill();
                            p.WaitForExit();

                            OnUpdateRequired(watcher.Account, watcher.Session.Mode, watcher.Session.Args, false, "An update may be required");
                        }

                        break;
                }
            }
            catch (Exception ex)
            {
                Util.Logging.Log(ex);
            }
        }

        private static int GetUIDFromCommandLine(string commandLine)
        {
            int i = commandLine.LastIndexOf(ARGS_UID);
            if (i == -1)
                return -1;

            i += ARGS_UID.Length;
            int j = commandLine.IndexOf(' ', i);

            try
            {
                if (j == -1)
                    return Int32.Parse(commandLine.Substring(i));
                else
                    return Int32.Parse(commandLine.Substring(i, j - i));
            }
            catch (Exception e)
            {
                Util.Logging.Log(e);

                return -1;
            }
        }

        static void LinkedProcess_ProcessActive(object sender, Account e)
        {
            if (AccountProcessActivated != null)
            {
                Process p = sender as Process;

                try
                {
                    AccountProcessActivated(e.Settings, p);
                }
                catch (Exception ex)
                {
                    Util.Logging.Log(ex);
                }
            }
        }

        private static void LinkedProcess_ProcessExited(object sender, Account e)
        {
            Process p = sender as Process;

            OnProcessExited(p, e);
        }

        private static void LinkedProcess_Changed(object sender, Process e)
        {
            if (AccountProcessChanged != null)
            {
                LinkedProcess p = (LinkedProcess)sender;

                try
                {
                    AccountProcessChanged(p.Account.Settings, e);
                }
                catch (Exception ex)
                {
                    Util.Logging.Log(ex);
                }
            }
        }

        private static Process WaitForProxyLaunch(ushort uid, string processName, CancellationToken cancel, int timeout)
        {
            var t = Environment.TickCount;

            using (var pi = new Windows.ProcessInfo())
            {
                var ids = new HashSet<int>(LinkedProcess.GetActivePIDs());

                while (true)
                {
                    foreach (var p in Process.GetProcessesByName(processName))
                    {
                        try
                        {
                            var pid = p.Id;

                            if (ids.Contains(pid))
                                continue;

                            if (pi.Open(pid))
                            {
                                var commandLine = pi.GetCommandLine();

                                if (GetUIDFromCommandLine(commandLine) == uid)
                                {
                                    return p;
                                }

                                pi.Close();
                            }
                        }
                        catch { }

                        p.Dispose();
                    }

                    if (timeout >= 0 && Environment.TickCount - t > timeout)
                        throw new TimeoutException("Timed out while waiting for Steam");

                    if (cancel.IsCancellationRequested || cancel.WaitHandle.WaitOne(500))
                        throw new TaskCanceledException();
                }
            }
        }
    }
}
