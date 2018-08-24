using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.IO;
using System.Diagnostics;
using System.Management;

namespace Gw2Launcher.Client
{
    static partial class Launcher
    {
        public delegate void AccountStateChangedEventHandler(ushort uid, AccountState state, AccountState previousState, object data);
        public delegate void AccountEventHandler(Settings.IAccount account);
        public delegate void AccountEventHandler<T>(Settings.IAccount account, T e);
        public delegate void LaunchExceptionEventHandler(Settings.IAccount account, LaunchExceptionEventArgs e);
        public delegate void BuildUpdatedEventHandler(BuildUpdatedEventArgs e);

        public static event AccountStateChangedEventHandler AccountStateChanged;
        public static event LaunchExceptionEventHandler LaunchException;
        public static event AccountEventHandler AccountLaunched;
        public static event AccountEventHandler AccountExited;
        public static event AccountEventHandler<Process> AccountProcessChanged;
        public static event AccountEventHandler<Process> AccountProcessActivated;
        public static event EventHandler AllQueuedLaunchesComplete;
        public static event EventHandler<int> ActiveProcessCountChanged;
        public static event BuildUpdatedEventHandler BuildUpdated;
        public static event AccountEventHandler<LaunchMode> AccountQueued;

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
            Exited
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

        public class InvalidGW2PathException : Exception
        {
            public InvalidGW2PathException()
                : base("The location of Gw2.exe is invalid")
            {

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

            public void OnDequeued(DequeuedState state)
            {
                if (Dequeued != null)
                    Dequeued(this, state);
            }
        }

        private class QueuedAnnounce
        {
            public QueuedAnnounce(ushort uid, AccountState state, AccountState previousState, object data)
            {
                this.uid = uid;
                this.state = state;
                this.previousState = previousState;
                this.data = data;
            }

            public ushort uid;
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

        private const string ARGS_UID = "-l:id:";
        private const ushort PROCESS_EXIT_DELAY = 1000;
        private const ushort PROCESS_UPDATING_EXIT_DELAY = 5000;

        private static Dictionary<ushort, Account> accounts;
        private static Queue<QueuedLaunch> queueMulti;
        private static Queue<QueuedLaunch> queueSingle;
        private static Queue<QueuedLaunch> queue;
        private static Queue<QueuedAnnounce> queueAnnounce;
        private static Queue<QueuedExit> queueExit;
        private static Dictionary<int, Process> unknownProcesses;
        private static HashSet<string> activeUsers;
        private static DateTime lastExit;
        private static Task taskQueue;
        private static Task taskAnnounce;
        private static Task taskScan;
        private static Task taskWatchUnknowns;
        private static ushort activeProcesses;
        private static CancellationTokenSource cancelQueue;
        private static ManualResetEvent activeProcessWait;
        private static QueuedLaunch lastLaunch;
        private static bool aborting;

        static Launcher()
        {
            accounts = new Dictionary<ushort, Account>();
            queue = new Queue<QueuedLaunch>();
            queueMulti = new Queue<QueuedLaunch>();
            queueSingle = new Queue<QueuedLaunch>();
            queueAnnounce = new Queue<QueuedAnnounce>();
            queueExit = new Queue<QueuedExit>();
            unknownProcesses = new Dictionary<int, Process>();
            activeUsers = new HashSet<string>();

            LinkedProcess.ProcessExited += LinkedProcess_ProcessExited;
            LinkedProcess.ProcessActive += LinkedProcess_ProcessActive;
        }

        public static Process FindProcess(Settings.IAccount account)
        {
            Account _account;
            lock (accounts)
            {
                if (accounts.TryGetValue(account.UID, out _account))
                {
                    return _account.Process.Process;
                }
            }
            return null;
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

        public static void ApplyWindowedBounds(Settings.IAccount account)
        {
            if (account.Windowed && !account.WindowBounds.IsEmpty)
            {
                try
                {
                    var p = GetAccount(account).Process.Process;
                    if (p != null && !p.HasExited)
                    {
                        WindowWatcher.SetBounds(p, account.WindowBounds);
                    }
                }
                catch (Exception e)
                {
                    Util.Logging.Log(e);
                }
            }
        }

        public static void Kill(Settings.IAccount account)
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
                }
                catch { }
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

                if (Monitor.TryEnter(queueMulti, 100))
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
                        Monitor.Exit(queueMulti);
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

                if (taskQueue == null || taskQueue.IsCompleted)
                {
                    cancelQueue = new CancellationTokenSource();
                    var cancel = cancelQueue.Token;

                    taskQueue = new Task(
                        delegate
                        {
                            DoQueue(cancel);
                        }, cancel);
                    taskQueue.Start();
                }
            }
        }

        private static bool WaitOnActiveProcesses(CancellationToken cancel)
        {
            bool waiting;

            lock (queueMulti)
            {
                if (waiting = activeProcesses > 0)
                {
                    if (activeProcessWait == null)
                        activeProcessWait = new ManualResetEvent(false);
                    else
                        activeProcessWait.Reset();
                }
            }

            if (waiting)
            {
                try
                {
                    while (!activeProcessWait.WaitOne(500) && !cancel.IsCancellationRequested)
                    {
                    }
                }
                catch (Exception e)
                {
                    Util.Logging.Log(e);
                }
            }

            return waiting;
        }
        
        private static Task WaitForScannerTask()
        {
            Task t;

            lock (queueExit)
            {
                t = taskScan;
                if (t == null || t.IsCompleted)
                {
                    t = taskScan = new Task(DoScan);
                    t.Start();
                }
            }

            return t;
        }

        private static void WaitForScanner()
        {
            try
            {
                WaitForScannerTask().Wait();
            }
            catch (Exception e)
            {
                Util.Logging.Log(e);
            }
        }

        private static void WaitForExit(Account account, CancellationToken cancel)
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
                    AccountStateChanged(q.uid, q.state, q.previousState, q.data);
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

            do
            {
                Queue<QueuedLaunch> queue;
                QueuedLaunch q;

                #region wait for last launch

                if (lastLaunch != null && lastLaunch.mode != LaunchMode.Launch && lastLaunch.account != null)
                {
                    var account = lastLaunch.account;
                    if (account.IsActive)
                    {
                        WaitForExit(account, cancel);
                    }
                }

                #endregion

                Monitor.Enter(queueMulti);
                try
                {
                    #region Add queued items

                    if (Monitor.TryEnter(Launcher.queue, 100))
                    {
                        try
                        {
                            while (Launcher.queue.Count > 0)
                            {
                                var _q = Launcher.queue.Dequeue();
                                if (_q.mode == LaunchMode.Launch)
                                    queueMulti.Enqueue(_q);
                                else
                                    queueSingle.Enqueue(_q);
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
                        foreach (var _queue in new Queue<QueuedLaunch>[] { queueSingle, queueMulti })
                        {
                            while (_queue.Count > 0)
                            {
                                var _q = _queue.Dequeue();

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
                        }

                        if (Monitor.TryEnter(Launcher.queue, 100))
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
                                Monitor.Exit(queueMulti);

                                try
                                {
                                    AllQueuedLaunchesComplete(null, EventArgs.Empty);
                                }
                                catch (Exception e)
                                {
                                    Util.Logging.Log(e);
                                }
                            }

                            return;
                        }
                        else
                        {
                            Thread.Sleep(100);
                            continue;
                        }
                    }

                    #endregion

                    if (queueSingle.Count > 0)
                        queue = queueSingle;
                    else
                        queue = queueMulti;

                    #region Queues empty

                    if (queue.Count == 0)
                    {
                        if (Monitor.TryEnter(Launcher.queue, 100))
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

                            Monitor.Exit(queueMulti);

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

                            return;
                        }
                        else
                        {
                            Thread.Sleep(100);
                            continue;
                        }
                    }

                    #endregion

                    q = queue.Peek();
                }
                finally
                {
                    if (Monitor.IsEntered(queueMulti))
                        Monitor.Exit(queueMulti);
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
                        queue.Dequeue();
                        q.OnDequeued(QueuedLaunch.DequeuedState.Skipped);
                        continue;
                    }
                }
                else if (q.mode == LaunchMode.Launch)
                {
                    if (Settings.CheckForNewBuilds.Value && activeProcesses == 0)
                    {
                        int b = Tools.Gw2Build.Build;
                        if (b > 0 && b != build && Settings.LastKnownBuild.Value != b)
                        {
                            build = b;
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

                            lock (queueMulti)
                            {
                                int i = queueSingle.Count;
                                bool first = true;

                                foreach (var _q in e.Queue)
                                {
                                    Account account = GetAccount(_q);
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

                                    var ql = new QueuedLaunch(account, mode);
                                    queueSingle.Enqueue(ql);
                                    announce.Add(ql);
                                }

                                //shifting any existing single queued items to the back
                                //and removing any that were to be updated, since everything is
                                while (i-- > 0)
                                {
                                    var _q = queueSingle.Dequeue();
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
                                        queueSingle.Enqueue(_q);
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
                        queue.Dequeue();
                        q.OnDequeued(QueuedLaunch.DequeuedState.Skipped);
                        continue;
                    }
                }

                #endregion

                if (q.mode != LaunchMode.Launch)
                {
                    //launches running in normal mode can only run one client at a time
                    //these launches will be delayed until all clients are closed

                    if (WaitOnActiveProcesses(cancel) || cancel.IsCancellationRequested)
                    {
                        continue;
                    }
                }

                try
                {
                    try
                    {
                        Launch(q.account, q.mode, q.args);
                    }
                    catch (Security.Impersonation.BadUsernameOrPasswordException)
                    {
                        throw new BadUsernameOrPasswordException();
                    }
                    catch (System.ComponentModel.Win32Exception e)
                    {
                        if (((System.ComponentModel.Win32Exception)e).NativeErrorCode == 1326)
                            throw new BadUsernameOrPasswordException();
                        else
                            throw;
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
                                if (!q.account.Settings.DatFile.IsInitialized)
                                {
                                    lock (queueMulti)
                                    {
                                        queue.Dequeue();
                                        q.mode = LaunchMode.LaunchSingle;
                                        queueSingle.Enqueue(q);
                                    }
                                }
                            }

                            #endregion

                            continue;
                        }
                        else if (e is InvalidGW2PathException)
                        {
                            #region InvalidGW2PathException

                            //dump the remaining queue
                            lock (queueMulti)
                            {
                                var handled = new HashSet<Account>();
                                foreach (var _queue in new Queue<QueuedLaunch>[] { queueSingle, queueMulti })
                                {
                                    while (_queue.Count > 0)
                                    {
                                        var _q = _queue.Dequeue();
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
                            }

                            #endregion

                            continue;
                        }
                        else if (e is BadUsernameOrPasswordException)
                        {
                            #region BadUsernameOrPasswordException

                            //dump queued items that are using the same account
                            lock (queueMulti)
                            {
                                string username = Util.Users.GetUserName(q.account.Settings.WindowsAccount);
                                var handled = new HashSet<Account>();
                                //bool dumpAll = queueSingle.Count > 0;
                                int i;

                                i = queueSingle.Count;
                                while (i-- > 0)
                                {
                                    var _q = queueSingle.Dequeue();
                                    if (Util.Users.GetUserName(_q.account.Settings.WindowsAccount).Equals(username, StringComparison.OrdinalIgnoreCase))
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
                                    else
                                        queueSingle.Enqueue(_q);
                                }

                                i = queueMulti.Count;
                                while (i-- > 0)
                                {
                                    var _q = queueMulti.Dequeue();
                                    if (Util.Users.GetUserName(_q.account.Settings.WindowsAccount).Equals(username, StringComparison.OrdinalIgnoreCase))
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
                                    //else if (dumpAll)
                                    //{
                                    //    if (handled.Add(_q.account))
                                    //        _q.account.SetState(AccountState.None, true);
                                    //    _q.account.InUse = false;
                                    //}
                                    else
                                        queueMulti.Enqueue(_q);
                                }
                            }

                            #endregion

                            continue;
                        }
                    }

                    lock (queueMulti)
                    {
                        queue.Dequeue();
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

                lock (queueMulti)
                {
                    queue.Dequeue();
                    q.account.inQueueCount--;
                    q.OnDequeued(QueuedLaunch.DequeuedState.OK);

                    lock (q.account)
                    {
                        if (q.account.inQueueCount == 0 && q.account.State == AccountState.Waiting)
                            q.account.SetState(AccountState.None, true);
                    }
                }
            }
            while (true);
        }

        private static bool Launch(Account account, LaunchMode mode, string args)
        {
            FileInfo fi;
            try
            {
                if (Settings.GW2Path.HasValue && !string.IsNullOrEmpty(Settings.GW2Path.Value))
                    fi = new FileInfo(Settings.GW2Path.Value);
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
                throw new InvalidGW2PathException();
            }

            return Launch(account, mode, fi, args);
        }

        /// <summary>
        /// Returns if the DatFile is queued to be updated
        /// </summary>
        private static bool IsUpdateQueued(Account account)
        {
            lock (queueMulti)
            {
                ushort uid;
                if (account.Settings.DatFile != null)
                    uid=account.Settings.DatFile.UID;
                else
                    uid=0;

                foreach (var q in queueSingle)
                {
                    if (q.account == null)
                    {
                        return true;
                    }
                    else if (uid == 0)
                    {
                        if (q.account.Settings.UID == account.Settings.UID)
                            return true;
                    }
                    else
                    {
                        var dat = q.account.Settings.DatFile;
                        if (dat != null && dat.UID == uid)
                            return true;
                    }
                }

                return false;
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

        private static bool CheckDat(string path)
        {
            try
            {
                FileInfo fi = new FileInfo(path);
                if (fi.Exists)
                    return true;
            }
            catch (Exception e)
            {
                Util.Logging.Log(e);
            }

            return false;
        }

        private static bool Launch(Account account, LaunchMode mode, FileInfo fi, string args)
        {
            string username = Util.Users.GetUserName(account.Settings.WindowsAccount);

            #region UserAlreadyActiveException (no longer used)
            //Account _active = GetActiveAccount(username);
            //if (_active != null)
            //{
            //    //the user's account is already in use, however, if the same dat file is being used, it doesn't matter
            //    if (_active.Settings.DatFile != account.Settings.DatFile)
            //        throw new UserAlreadyActiveException(username);
            //}
            #endregion
            
            bool customProfile;
            byte retries = 0;

            do
            {
                try
                {
                    customProfile = DatManager.Activate(account.Settings);
                    break;
                }
                catch (DatManager.UserAccountNotInitializedException e)
                {
                    Util.Logging.Log(e);

                    if (retries++ > 0)
                    {
                        throw;
                    }

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

            if (mode == LaunchMode.Launch && (!account.Settings.DatFile.IsInitialized || !CheckDat(account.Settings.DatFile.Path)))
            {
                throw new DatFileNotInitialized();
            }

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

            KillMutex();

            var startInfo = GetProcessStartInfo(account.Settings, customProfile, mode, fi, args);
            var isWindowed = IsWindowed(account.Settings);

            retries = 0;

            do
            {
                Process gw2 = account.Process.Launch(startInfo);
                lastLaunch = new QueuedLaunch(account, mode);

                if (!gw2.WaitForExit(2000))
                {
                    if (mode == LaunchMode.Launch || mode == LaunchMode.LaunchSingle)
                    {
                        lock (account)
                        {
                            account.SetState(AccountState.Active, true, gw2);
                        }
                    }

                    if (!gw2.HasExited)
                    {
                        lock (unknownProcesses)
                        {
                            activeProcesses++;
                            OnActiveProcessCountChanged();
                        }

                        if (mode == LaunchMode.LaunchSingle)
                        {
                            account.Settings.DatFile.IsInitialized = true;
                        }

                        if (!IsUpdate(mode))
                        {
                            WindowWatcher watcher = account.watcher = new WindowWatcher(account, gw2, isWindowed, args);
                            if (isWindowed)
                                watcher.WindowChanged += OnWatchedWindowChanged;
                            watcher.WindowCrashed += OnWatchedWindowCrashed;
                            watcher.WindowCreated += OnWatchedWindowCreated;
                            watcher.Start();

                            if (AccountLaunched != null)
                            {
                                try
                                {
                                    AccountLaunched(account.Settings);
                                }
                                catch(Exception e) 
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

                            if (Settings.RunAfterLaunching.HasValue)
                                RunAfter(Settings.RunAfterLaunching.Value, account, gw2);

                            if (!string.IsNullOrEmpty(account.Settings.RunAfterLaunching))
                                RunAfter(account.Settings.RunAfterLaunching, account, gw2);
                        }
                    }

                    return true;
                }
                else
                {
                    double duration = gw2.ExitTime.Subtract(gw2.StartTime).TotalSeconds;
                    bool isUpdate = IsUpdate(mode);

                    if (isUpdate || duration < 1) //(IsUpdate(mode) && duration < 0.5 || !IsUpdate(mode) && duration < 1)
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
                            if (!KillMutex())
                            {
                                retries++;

                                try
                                {
                                    //try killing the mutex using admin rights
                                    if (retries == 1)
                                    {
                                        Util.ProcessUtil.KillMutexWindow(fi.FullName);
                                    }
                                    else if (retries == 2)
                                    {
                                        Util.ProcessUtil.KillMutexWindowByProcessName(fi.Name.Substring(0, fi.Name.Length - fi.Extension.Length));
                                    }
                                }
                                catch(Exception e)
                                {
                                    //failed or cancelled
                                    Util.Logging.Log(e);
                                    break;
                                }
                            }
                        }
                    }
                    else
                    {
                        //AccountStateChanged handled by process exit
                        return true;
                    }
                }
            }
            while (retries < 3);

            lock (account)
            {
                account.SetState(AccountState.None, true);
            }

            return false;
        }

        private static void RunAfter(string run, Account account, Process gw2)
        {
            try
            {
                //run = run.Replace("%processid%", gw2.Id.ToString()).Replace("%accountid%", account.Settings.UID.ToString());

                using (Process p = new Process())
                {
                    p.StartInfo = new ProcessStartInfo()
                    {
                        FileName = "cmd.exe",
                        RedirectStandardInput = true,
                        UseShellExecute = false,
                        CreateNoWindow = true
                    };

                    if (p.Start())
                    {
                        using (StreamWriter sw = p.StandardInput)
                        {
                            if (sw.BaseStream.CanWrite)
                            {
                                sw.WriteLine("set processid=" + gw2.Id.ToString());
                                sw.WriteLine("set accountid=" + gw2.Id.ToString());
                                sw.WriteLine(run);
                            }
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Util.Logging.Log(e);
            }
        }

        /// <summary>
        /// Returns the count of all processes, including unknowns
        /// </summary>
        public static int GetActiveProcessCount()
        {
            return activeProcesses;
        }

        public static int GetPendingLaunchCount()
        {
            return queue.Count + queueMulti.Count + queueSingle.Count;
        }

        public static void CancelPendingLaunches()
        {
            lock (queueMulti)
            {
                aborting = true;
                if (cancelQueue != null)
                    cancelQueue.Cancel();
            }
        }

        public static async void CancelAndKillActiveLaunches()
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
                                DoScan(ScanOptions.KillLinked);
                            });
                        t.Start();

                        break;
                    }
                }

                await t;
            }
            while (true);

            await t;

            //KillActiveLaunches();

            //do
            //{
            //    //ensure there's nothing waiting to be rehooked and kill it again
            //    await WaitForScannerTask();
            //} 
            //while (KillActiveLaunches() > 0);
        }

        public static int KillActiveLaunches()
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

        public static int KillAllActiveProcesses()
        {
            int count = KillActiveLaunches();

            lock (unknownProcesses)
            {
                foreach (var p in unknownProcesses.Values)
                {
                    bool hasExited;
                    try
                    {
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

        /// <summary>
        /// Returns any accounts with an active status (active, updating, launching)
        /// </summary>
        public static List<Settings.IAccount> GetActiveStates()
        {
            lock (accounts)
            {
                List<Settings.IAccount> _accounts = new List<Settings.IAccount>(activeProcesses + 1);
                foreach (var account in accounts.Values)
                {
                    if (account.IsActive || account.State == AccountState.Launching)
                        _accounts.Add(account.Settings);
                }
                return _accounts;
            }
        }

        private static bool IsUserActive(string username)
        {
            foreach (var l in LinkedProcess.GetActive())
            {
                if (Util.Users.GetUserName(l.Account.Settings.WindowsAccount).Equals(username, StringComparison.OrdinalIgnoreCase))
                    return true;
            }
            return false;
        }

        private static bool IsWindowed(Settings.IAccount account)
        {
            return account.Windowed && !account.WindowBounds.IsEmpty;
        }

        private static string GetArguments(Settings.IAccount account, string arguments, string arguments2, LaunchMode mode)
        {
            StringBuilder args = new StringBuilder(256);

            args.Append(ARGS_UID);
            args.Append(account.UID);

            if (mode == LaunchMode.Update)
            {
                args.Append(" -nopatchui -image");
            }
            else if (mode == LaunchMode.UpdateVisible)
            {
                args.Append(" -image");
            }
            else
            {
                if (!string.IsNullOrEmpty(arguments))
                {
                    args.Append(' ');
                    if (!Settings.DisableAutomaticLogins)
                        args.Append(arguments);
                    else
                        args.Append(Util.Args.AddOrReplace(arguments, "autologin", ""));
                }

                if (mode == LaunchMode.Launch)
                    args.Append(" -shareArchive");

                if (IsWindowed(account))
                    args.Append(" -windowed");

                if (!string.IsNullOrEmpty(account.Arguments))
                {
                    args.Append(' ');
                    if (!Settings.DisableAutomaticLogins)
                        args.Append(account.Arguments);
                    else
                        args.Append(Util.Args.AddOrReplace(account.Arguments, "autologin", ""));
                }

                if (!string.IsNullOrEmpty(account.AutomaticLoginEmail) && !string.IsNullOrEmpty(account.AutomaticLoginPassword))
                {
                    if (!Settings.DisableAutomaticLogins)
                        args.Append(" -nopatchui");
                    args.Append(" -email \"");
                    args.Append(account.AutomaticLoginEmail);
                    args.Append("\" -password \"");
                    args.Append(account.AutomaticLoginPassword);
                    args.Append('"');
                }
            }

            if (!string.IsNullOrEmpty(arguments2))
            {
                args.Append(' ');
                args.Append(arguments2);
            }

            if (mode == LaunchMode.Update || mode == LaunchMode.UpdateVisible)
            {
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
            }

            var a = args.ToString();

            if (Net.AssetProxy.ServerController.Enabled)
            {
                var proxy = Net.AssetProxy.ServerController.Active;
                if (proxy != null)
                {
                    try
                    {
                        int port = proxy.Port;
                        if (port != 0)
                            a = Util.Args.AddOrReplace(a, "assetsrv", "-assetsrv 127.0.0.1:" + port);
                    }
                    catch (Exception ex)
                    {
                        Util.Logging.Log(ex);
                    }
                }
            }

            return a;
        }

        private static ProcessStartInfo GetProcessStartInfo(Settings.IAccount account, bool customProfile, LaunchMode mode, FileInfo fi, string args)
        {
            ProcessStartInfo startInfo = new ProcessStartInfo(fi.FullName, GetArguments(account, Settings.GW2Arguments.Value, args, mode));
            startInfo.UseShellExecute = false;
            startInfo.WorkingDirectory = fi.DirectoryName;

            if (!Util.Users.IsCurrentUser(account.WindowsAccount))
            {
                startInfo.UserName = account.WindowsAccount;
                var password = Security.Credentials.GetPassword(account.WindowsAccount);
                if (password == null)
                    throw new BadUsernameOrPasswordException();
                startInfo.Password = password;
                startInfo.LoadUserProfile = true;
            }

            if (customProfile)
            {
                Security.Impersonation.IImpersonationToken impersonation;
                string username = Util.Users.GetUserName(account.WindowsAccount);

                if (Util.Users.IsCurrentUser(username))
                    impersonation = null;
                else
                    impersonation = Security.Impersonation.Impersonate(username, Security.Credentials.GetPassword(username));

                try
                {
                    string appdata = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
                    string userprofile = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);

                    if (appdata.StartsWith(userprofile, StringComparison.OrdinalIgnoreCase))
                    {
                        string path = Path.Combine(appdata, "Guild Wars 2", account.DatFile.UID.ToString());

                        //GW2 only uses the userprofile variable
                        startInfo.EnvironmentVariables["USERPROFILE"] = path;
                        startInfo.EnvironmentVariables["APPDATA"] = Path.Combine(path, appdata.Substring(userprofile.Length + 1));                        
                    }
                    else
                        throw new Exception("Unknown user profile directory structure");
                }
                finally
                {
                    if (impersonation != null)
                        impersonation.Dispose();
                }
            }

            return startInfo;
        }

        private static bool KillMutex()
        {
            bool killed = false;

            foreach (LinkedProcess p in LinkedProcess.GetActive())
            {
                try
                {
                    if (p.HasMutex)
                    {
                        p.KillMutex();
                        killed = true;
                    }
                }
                catch(Exception e)
                {
                    Util.Logging.Log(e);
                }
            }

            return killed;
        }

        /// <summary>
        /// Looks for any running GW2 processes and if valid (started by this program), link them
        /// </summary>
        public static void Scan()
        {
            if (!Settings.GW2Path.HasValue || string.IsNullOrEmpty(Settings.GW2Path.Value))
                return;

            FileInfo fi;

            try
            {
                fi = new FileInfo(Settings.GW2Path.Value);
            }
            catch (Exception e)
            {
                Util.Logging.Log(e);

                return;
            }

            Scan(fi, ScanOptions.None);
        }

        private static int Scan(FileInfo fi, ScanOptions options)
        {
            var startTime = DateTime.UtcNow;
            int counter = 0;

            Process[] ps;
            try
            {
                if (fi == null)
                    throw new NullReferenceException();
                ps = Process.GetProcessesByName(fi.Name.Substring(0, fi.Name.Length - fi.Extension.Length));
            }
            catch (Exception e)
            {
                Util.Logging.Log(e);
                ps = new Process[0];
            }

            foreach (Process p in ps)
            {
                try
                {
                    string path = null,
                           commandLine = null,
                           query = string.Format("SELECT ExecutablePath, CommandLine FROM Win32_Process WHERE ProcessId={0}", p.Id);

                    using (ManagementObjectSearcher searcher = new ManagementObjectSearcher(query))
                    {
                        using (ManagementObjectCollection results = searcher.Get())
                        {
                            foreach (ManagementObject o in results)
                            {
                                path = o["ExecutablePath"] as string;
                                commandLine = o["CommandLine"] as string;
                                break;
                            }
                        }
                    }

                    if (string.Equals(path, fi.FullName, StringComparison.OrdinalIgnoreCase))
                    {
                        bool isUnknown = false;

                        lock (accounts)
                        {
                            Account account = LinkedProcess.GetAccount(p);

                            //handle processes that haven't been linked yet
                            if (account == null)
                            {
                                int uqid = -1;

                                try
                                {
                                    //string commandLine = GetProcessCommandLine(p);
                                    if (commandLine != null)
                                        uqid = GetUIDFromCommandLine(commandLine);
                                }
                                catch (Exception e)
                                {
                                    Util.Logging.Log(e);
                                }

                                if (options == ScanOptions.KillAll || uqid != -1 && options == ScanOptions.KillLinked)
                                {
                                    counter++;
                                    try
                                    {
                                        p.Kill();
                                        p.Dispose();
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

                                    if (!accounts.TryGetValue(uid, out account))
                                    {
                                        if (Settings.Accounts.Contains(uid))
                                        {
                                            account = new Account(Settings.Accounts[uid].Value);
                                            account.Process.Changed += LinkedProcess_Changed;
                                            accounts.Add(account.Settings.UID, account);

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
                                        lock (account)
                                        {
                                            if (!account.IsActive)
                                                account.SetState(AccountState.Active, true, p);
                                            account.isRelaunch++;
                                        }

                                        account.Process.Attach(p);

                                        switch (account.State)
                                        {
                                            case AccountState.Updating:
                                            case AccountState.UpdatingVisible:
                                                break;
                                            default:

                                                bool isWindowed = IsWindowed(account.Settings);
                                                WindowWatcher watcher = account.watcher = new WindowWatcher(account, p, isWindowed, null);
                                                if (isWindowed)
                                                    watcher.WindowChanged += OnWatchedWindowChanged;
                                                watcher.WindowCrashed += OnWatchedWindowCrashed;
                                                watcher.WindowCreated += OnWatchedWindowCreated;
                                                watcher.Start();

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
                                if (options == ScanOptions.KillAll || options == ScanOptions.KillLinked)
                                {
                                    counter++;
                                    try
                                    {
                                        p.Kill();
                                        p.Dispose();
                                        continue;
                                    }
                                    catch { }
                                }
                            }
                        }

                        if (isUnknown)
                        {
                            lock (unknownProcesses)
                            {
                                if (!unknownProcesses.ContainsKey(p.Id))
                                {
                                    unknownProcesses.Add(p.Id, p);
                                    if (taskWatchUnknowns == null || taskWatchUnknowns.IsCompleted)
                                    {
                                        taskWatchUnknowns = new Task(DoWatch, TaskCreationOptions.LongRunning);
                                        taskWatchUnknowns.Start();
                                    }
                                }
                                else
                                    p.Dispose();
                            }
                        }
                    }
                    else
                    {
                        if (path == null) //process could have been closed while searching
                            counter++;
                        p.Dispose();
                    }
                }
                catch (Exception e)
                {
                    Util.Logging.Log(e);

                    p.Dispose();
                }
            }

            OnScanComplete(startTime, options);

            lock (unknownProcesses)
            {
                ushort activeProcesses = (ushort)(LinkedProcess.GetActiveCount() + unknownProcesses.Count);
                if (Launcher.activeProcesses != activeProcesses)
                {
                    Launcher.activeProcesses = activeProcesses;
                    OnActiveProcessCountChanged();
                }
            }

            return counter;
        }

        private static void DoScan()
        {
            DoScan(ScanOptions.None);
        }

        private static void DoScan(ScanOptions options)
        {
            FileInfo fi = null;
            try
            {
                if (Settings.GW2Path.HasValue && !string.IsNullOrEmpty(Settings.GW2Path.Value))
                    fi = new FileInfo(Settings.GW2Path.Value);
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
                    DateTime waitUntil;

                    //when updating, the launcher closes and swaps the executable, which may take longer to restart
                    if (account != null && account.State == AccountState.UpdatingVisible && account.isRelaunch == 0)
                        waitUntil = lastExit.AddMilliseconds(PROCESS_UPDATING_EXIT_DELAY);
                    else
                        waitUntil = lastExit.AddMilliseconds(PROCESS_EXIT_DELAY);

                    while (DateTime.UtcNow < waitUntil)
                    {
                        Thread.Sleep(500);
                    }

                    Scan(fi, options);
                }
                else
                {
                    DateTime limit = DateTime.UtcNow.AddSeconds(5);
                    do
                    {
                        var count = Scan(fi, options);
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

                    foreach (Process _p in unknownProcesses.Values)
                    {
                        bool hasExited = false;
                        try
                        {
                            hasExited = _p.HasExited;
                        }
                        catch(Exception e) 
                        {
                            Util.Logging.Log(e);
                        }

                        if (hasExited)
                        {
                            unknownProcesses.Remove(_p.Id);
                            break;
                        }
                        else
                        {
                            p = _p;
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

        private static void OnActiveProcessCountChanged()
        {
            if (ActiveProcessCountChanged != null)
                ActiveProcessCountChanged(null, activeProcesses);

            lock (unknownProcesses)
            {
                if (activeProcesses == 0)
                {
                    lock (queueMulti)
                    {
                        if (activeProcessWait != null)
                            activeProcessWait.Set();
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
                        if (q.exitTime.AddMilliseconds(PROCESS_UPDATING_EXIT_DELAY) < startTime)
                            queueExit.Dequeue();
                        else
                            break;
                    }
                    else if (q.exitTime.AddMilliseconds(PROCESS_EXIT_DELAY) < startTime)
                        queueExit.Dequeue();
                    else
                        break;

                    if (q.account.Process.Process == null)
                    {
                        if (Monitor.TryEnter(queueMulti, 100))
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
                                Monitor.Exit(queueMulti);
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
        }

        private static void OnWatchedWindowChanged(object sender, IntPtr e)
        {
            WindowWatcher watcher = (WindowWatcher)sender;
            if (watcher.Account.Settings.Windowed)
            {
                var r = watcher.Account.Settings.WindowBounds;

                if (!r.IsEmpty)
                {
                    watcher.SetBounds(e, r, 30000);
                }
            }
        }

        private static void OnWatchedWindowCreated(object sender, EventArgs e)
        {
            WindowWatcher watcher = (WindowWatcher)sender;
            lock (watcher.Account)
            {
                if (watcher.Account.State == AccountState.Active)
                    watcher.Account.SetState(AccountState.ActiveGame, true);
            }

            if (watcher.Account.Settings.VolumeEnabled)
                watcher.SetVolume(watcher.Account.Settings.Volume);
            else if (Settings.Volume.HasValue)
                watcher.SetVolume(Settings.Volume.Value);
        }

        private static void OnWatchedWindowCrashed(object sender, WindowWatcher.CrashReason e)
        {
            if (e == WindowWatcher.CrashReason.PatchRequired)
            {
                WindowWatcher watcher = (WindowWatcher)sender;
                var account = watcher.Account;
                var p = account.Process.Process;

                try
                {
                    if (p != null && !p.HasExited)
                    {
                        p.Kill();
                        p.WaitForExit();

                        WaitForScanner();

                        Action<Account> error = delegate(Account _account)
                        {
                            if (_account.inQueueCount == 0 && _account.State == AccountState.None)
                            {
                                if (Monitor.TryEnter(_account, 100))
                                {
                                    try
                                    {
                                        _account.SetState(AccountState.Error, true, new Exception("An update is required; client is out of date"));
                                        _account.SetState(AccountState.None, false);
                                    }
                                    finally
                                    {
                                        Monitor.Exit(_account);
                                    }
                                }
                            }
                        };

                        Action<Account, string> queue = delegate(Account _account, string args)
                        {
                            _account.inQueueCount++;
                            queueMulti.Enqueue(new QueuedLaunch(_account, LaunchMode.Launch, args));
                            if (AccountQueued != null)
                                AccountQueued(_account.Settings, LaunchMode.Launch);
                        };

                        bool retry = false;

                        lock (queueMulti)
                        {
                            if (cancelQueue == null || taskQueue == null || !cancelQueue.IsCancellationRequested)
                            {
                                if (Monitor.TryEnter(accounts, 100))
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

                                if (account.inQueueCount == 0 && account.errors == 1)
                                {
                                    var ql = new QueuedLaunch(null, LaunchMode.Update);
                                    var args = watcher.Args;

                                    ql.Dequeued += delegate(object o, QueuedLaunch.DequeuedState state)
                                    {
                                        lock (queueMulti)
                                        {
                                            if (state == QueuedLaunch.DequeuedState.OK)
                                                queue(account, args);
                                            else
                                                error(account);
                                        }
                                    };

                                    queueSingle.Enqueue(ql);

                                    retry = true;
                                }
                                else
                                {
                                    if (IsUpdateQueued(account))
                                    {
                                        queue(account, watcher.Args);
                                        retry = true;
                                    }
                                }
                            }

                            if (!retry)
                                error(account);
                        }

                        lock (queue)
                        {
                            if (taskQueue == null || taskQueue.IsCompleted)
                            {
                                cancelQueue = new CancellationTokenSource();
                                var cancel = cancelQueue.Token;

                                taskQueue = new Task(
                                    delegate
                                    {
                                        DoQueue(cancel);
                                    }, cancel);
                                taskQueue.Start();
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    Util.Logging.Log(ex);
                }
            }
        }

        private static int GetUIDFromCommandLine(string commandLine)
        {
            int i = commandLine.IndexOf(ARGS_UID);
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
    }
}
