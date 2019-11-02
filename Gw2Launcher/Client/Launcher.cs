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
        public delegate void LaunchExceptionEventHandler(Settings.IAccount account, LaunchExceptionEventArgs e);
        public delegate void BuildUpdatedEventHandler(BuildUpdatedEventArgs e);

        public class AccountWindowEventEventArgs : EventArgs
        {
            public enum EventType
            {
                WindowReady,
                Focused,
                Minimized,
                TopMost,
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
                this.Handle = process.MainWindowHandle;
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

        public static event AccountEventHandler<AccountStateEventArgs> AccountStateChanged;
        public static event LaunchExceptionEventHandler LaunchException;
        public static event AccountEventHandler AccountLaunched;
        public static event AccountEventHandler AccountExited;
        public static event AccountEventHandler<Process> AccountProcessChanged;
        public static event AccountEventHandler<Process> AccountProcessActivated;
        public static event AccountEventHandler<Process> AccountProcessExited;
        public static event EventHandler AllQueuedLaunchesComplete;
        public static event EventHandler<int> ActiveProcessCountChanged;
        public static event BuildUpdatedEventHandler BuildUpdated;
        public static event AccountEventHandler<LaunchMode> AccountQueued;
        public static event AccountEventHandler<AccountWindowEventEventArgs> AccountWindowEvent;

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
            public Tools.ArenaAccount session;

            public void OnDequeued(DequeuedState state)
            {
                if (Dequeued != null)
                {
                    Dequeued(this, state);
                    Dequeued = null;
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
                                VAR_TEMP = "TMP";

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

        private const string ARGS_UID = "-l:id:";

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
        private static QueuedLaunch lastLaunch;
        private static bool aborting;
        private static Tools.ProcessPriority processPriority;
        private static Tools.CoherentMonitor coherentMonitor;
        private static WindowEvents windowEvents;
        private static Autologin autologin;
        private static Stream coherentLock;

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

            windowEvents = new WindowEvents();
            autologin = new Autologin();

            LinkedProcess.ProcessExited += LinkedProcess_ProcessExited;
            LinkedProcess.ProcessActive += LinkedProcess_ProcessActive;

            Settings.PreventDefaultCoherentUI.ValueChanged += PreventDefaultCoherentUI_ValueChanged;
        }

        static void PreventDefaultCoherentUI_ValueChanged(object sender, EventArgs e)
        {
            var v = ((Settings.ISettingValue<bool>)sender).Value;

            if (!v)
            {
                lock (queueMulti)
                {
                    if (coherentLock != null)
                    {
                        coherentLock.Dispose();
                        coherentLock = null;
                    }
                }
            }
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

        public static Process GetProcess(Settings.IAccount account)
        {
            Process p;
            lock (accounts)
            {
                var _account = GetAccount(account);
                p = _account.Process.Process;
            }

            return p;
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
                    var account = new Account(Settings.CreateVoidAccount());
                    account.Settings.Name = Path.GetFileName(f);
                    account.Settings.DatFile = Settings.CreateVoidDatFile();
                    account.Settings.DatFile.Path = f;
                    
                    var q = new QueuedLaunch(account, mode, f);
                    queue.Enqueue(q);

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

                if (Monitor.TryEnter(queueMulti, TIMEOUT_TRYENTER))
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
                        }, cancel);
                    taskQueue.Start();
                }
            }
        }

        private static bool WaitOnActiveProcesses(CancellationToken cancel)
        {
            return WaitOnActiveProcesses(cancel, 0);
        }

        private static bool WaitOnActiveProcesses(CancellationToken cancel, int maxCount)
        {
            bool waiting;

            lock (unknownProcesses)
            {
                waiting = activeProcesses > maxCount;
            }

            if (waiting)
            {
                EventHandler<int> onCountChanged = null;
                using (var waiter = new ManualResetEvent(false))
                {
                    onCountChanged += delegate(object o, int count)
                    {
                        if (count <= maxCount)
                        {
                            waiter.Set();
                        }
                    };

                    ActiveProcessCountChanged += onCountChanged;

                    try
                    {
                        using (cancel.Register(
                            delegate
                            {
                                waiter.Set();
                            }))
                        {

                            if (activeProcesses > maxCount)
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

                    ActiveProcessCountChanged -= onCountChanged;
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
            Tools.DatUpdater datUpdater = null;

            do
            {
                Queue<QueuedLaunch> queue;
                QueuedLaunch q;

                #region Wait for last launch

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

                    if (Monitor.TryEnter(Launcher.queue, TIMEOUT_TRYENTER))
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

                                    if (account.Settings != null)
                                    {
                                        var dat = account.Settings.DatFile;
                                        if (dat != null)
                                            dat.IsPending = true;
                                    }

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

                    #region DatUpdater

                    if (q.mode == LaunchMode.Update && datUpdater != null && datUpdater.CanUpdate)
                    {
                        var dat = q.account.Settings.DatFile;
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

                                lock (queueMulti)
                                {
                                    queue.Dequeue();
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

                    if (Settings.DelayLaunchSeconds.HasValue && activeProcesses > 0 && lastMode == q.mode)
                    {
                        if (cancel.WaitHandle.WaitOne(Settings.DelayLaunchSeconds.Value * 1000))
                        {
                            continue;
                        }
                    }

                    var limit = Settings.LimitActiveAccounts.Value;
                    if (limit > 0)
                    {
                        if (WaitOnActiveProcesses(cancel, limit - 1) || cancel.IsCancellationRequested)
                        {
                            continue;
                        }
                    }
                }

                try
                {
                    try
                    {
                        Launch(q, cancel);

                        lastMode = q.mode;
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
                        lock (queueMulti)
                        {
                            queue.Dequeue();
                            q.account.inQueueCount--;
                            q.OnDequeued(QueuedLaunch.DequeuedState.OK);
                        }

                        OnUpdateRequired(q.account, q.mode, q.args, true, "An update may be required; client exited unexpectedly");

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

                #region DatUpdater

                if (IsUpdate(q.mode) && WaitForExit(q.account, cancel))
                {
                    var dat = q.account.Settings.DatFile;
                    dat.IsPending = false;

                    FileManager.VerifyLinks(FileManager.FileType.Dat, dat);

                    if (Settings.DatUpdaterEnabled.Value || Settings.UseCustomGw2Cache.Value)
                    {
                        if (q.mode == LaunchMode.UpdateVisible)
                        {
                            try
                            {
                                if (Settings.DatUpdaterEnabled.Value)
                                {
                                    datUpdater = Tools.DatUpdater.Create(q.account.Settings.DatFile);
                                }
                                else if (Settings.UseCustomGw2Cache.Value)
                                {
                                    datUpdater = Tools.DatUpdater.Create();
                                    datUpdater.UpdateCache(q.account.Settings.DatFile);
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
                                datUpdater.UpdateCache(q.account.Settings.DatFile);
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

                #endregion
            }
            while (true);
        }

        private static bool Launch(QueuedLaunch q, CancellationToken cancel)
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

            return Launch(q, fi, cancel);
        }

        /// <summary>
        /// Returns a queued launch that will update the DatFile linked to the account, if available
        /// </summary>
        private static QueuedLaunch GetQueuedUpdate(Account account)
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
                        return q;
                    }
                    else if (uid == 0)
                    {
                        if (q.account.Settings.UID == account.Settings.UID)
                            return q;
                    }
                    else
                    {
                        var dat = q.account.Settings.DatFile;
                        if (dat != null && dat.UID == uid)
                            return q;
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

        private static bool Launch(QueuedLaunch q, FileInfo fi, CancellationToken cancel)
        {
            var account = q.account;
            var mode = q.mode;
            var isUpdate = IsUpdate(mode);

            var username = Util.Users.GetUserName(account.Settings.WindowsAccount);

            #region UserAlreadyActiveException (no longer used)
            //Account _active = GetActiveAccount(username);
            //if (_active != null)
            //{
            //    //the user's account is already in use, however, if the same dat file is being used, it doesn't matter
            //    if (_active.Settings.DatFile != account.Settings.DatFile)
            //        throw new UserAlreadyActiveException(username);
            //}
            #endregion

            if (Settings.LocalizeAccountExecution.Value.HasFlag(Settings.LocalizeAccountExecutionOptions.Enabled) && !isUpdate)
            {
                try
                {
                    fi = new FileInfo(FileManager.ActivateExecutable(account.Settings, fi));
                }
                catch (NotSupportedException)
                {
                    Settings.LocalizeAccountExecution.Value = Settings.LocalizeAccountExecution.Value & ~Settings.LocalizeAccountExecutionOptions.Enabled;
                    throw new Exception("Linking " + fi.Name + " is not supported; localized execution has been disabled");
                }
                catch (Exception e)
                {
                    throw new Exception("Failed to create executable path:\n" + e.Message);
                }
            }

            FileManager.IProfileInformation customProfile;
            byte retries = 0;

            do
            {
                try
                {
                    if (account.Settings.UID > 0)
                    {
                        customProfile = FileManager.Activate(account.Settings);
                    }
                    else
                    {
                        customProfile = FileManager.Activate(q.args);

                        accounts[0] = q.account;

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

            if (!isUpdate)
            {
                if (mode == LaunchMode.Launch && (!account.Settings.DatFile.IsInitialized || !CheckDat(account.Settings.DatFile.Path)))
                    throw new DatFileNotInitialized();

                if (Settings.NetworkAuthorization.HasValue)
                    NetworkAuthorization.Verify(account.Settings, q.session);

                try
                {
                    Tools.Gw2Cache.DeleteLoginCounter(account.Settings.UID);
                }
                catch { }
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

            if (Settings.PreventDefaultCoherentUI.Value)
            {
                lock (queueMulti)
                {
                    if (mode == LaunchMode.Launch)
                    {
                        if (coherentLock == null)
                        {
                            try
                            {
                                //CoherentUI_Host.exe is the first file loaded
                                //icudt.dll is the first file that requires write access

                                var exebits = Util.FileUtil.GetExecutableBits(Settings.GW2Path.Value);
                                coherentLock = File.Open(Path.Combine(Path.GetDirectoryName(Settings.GW2Path.Value), exebits == 32 ? "bin" : "bin64", "CoherentUI_Host.exe"), FileMode.Open, FileAccess.Read, FileShare.None);
                            }
                            catch (Exception e)
                            {
                                Util.Logging.Log(e);
                            }
                        }
                    }
                    else if (coherentLock != null)
                    {
                        coherentLock.Dispose();
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

            KillMutex();

            var processOptions = GetProcessStartInfo(account.Settings, customProfile, mode, fi, q.args);
            var isWindowed = IsWindowed(account.Settings);

            retries = 0;

            do
            {
                bool useProxy;

                if (mode == LaunchMode.Launch)
                {
                    useProxy = Settings.PreventTaskbarGrouping.Value;

                    //GW2 will load with default GFX settings if the xml file can't be read / is being written to
                    account.GfxLock = FileLocker.Lock(account, account.Settings.GfxFile, 5000);
                }
                else
                    useProxy = false;

                Process gw2;

                if (useProxy)
                {
                    gw2 = ProxyLauncher.Launch(account.Settings, processOptions);
                    if (gw2 != null)
                        account.Process.Attach(gw2);
                    else
                    {
                        //process was started, but could not found (exited or restarted)
                    }
                }
                else
                {
                    gw2 = account.Process.Launch(processOptions.ToProcessStartInfo());
                }

                lastLaunch = q;

                bool okay;
                byte windowState = 0;
                WindowWatcher watcher = null;
                ManualResetEvent waiter = null;
                EventHandler<WindowWatcher.WindowChangedEventArgs> onChanged = null;

                try
                {
                    if (gw2 != null)
                    {
                        lock (unknownProcesses)
                        {
                            if (!gw2.HasExited)
                            {
                                activeProcesses++;
                                OnActiveProcessCountChanged();
                            }
                        }

                        if (!isUpdate)
                        {
                            watcher = account.Watcher = new WindowWatcher(account, gw2, true, isWindowed, mode, q.args);
                            watcher.WindowChanged += OnWatchedWindowChanged;
                            watcher.WindowCrashed += OnWatchedWindowCrashed;
                            watcher.AuthenticationRequired += OnWatchedWindowAuthenticationRequired;
                            watcher.ProcessOptions = processOptions;

                            waiter = new ManualResetEvent(false);

                            onChanged = delegate(object o, WindowWatcher.WindowChangedEventArgs e)
                            {
                                switch (e.Type)
                                {
                                    case WindowWatcher.WindowChangedEventArgs.EventType.LauncherCoherentUIReady:
                                        if (windowState < 2)
                                        {
                                            windowState = 2;
                                            waiter.Set();
                                        }
                                        return;
                                    case WindowWatcher.WindowChangedEventArgs.EventType.InGameCoherentUIReady:
                                    case WindowWatcher.WindowChangedEventArgs.EventType.WatcherExited:
                                        if (windowState < 3)
                                        {
                                            windowState = 3;
                                            waiter.Set();
                                        }
                                        return;
                                }

                                if (windowState == 0)
                                {
                                    waiter.Set();
                                    windowState = 1;
                                }
                            };

                            watcher.WindowChanged += onChanged;
                            watcher.Start();

                            if (waiter.WaitOne())
                            {
                                okay = !gw2.HasExited;
                                waiter.Reset();
                            }
                            else
                                okay = !gw2.WaitForExit(2000);
                        }
                        else
                        {
                            okay = !gw2.WaitForExit(2000);
                        }
                    }
                    else
                    {
                        waiter = null;
                        okay = false;
                    }

                    if (okay)
                    {
                        if (!isUpdate && !gw2.HasExited)
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

                                    account.Settings.DatFile.IsInitialized = true;

                                    break;
                            }

                            lock (account)
                            {
                                if (!gw2.HasExited)
                                    account.SetState(AccountState.Active, true, gw2);
                            }

                            try
                            {
                                var events = windowEvents.Add(gw2.Id, account);
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

                            if (!gw2.HasExited)
                            {
                                if (AccountWindowEvent != null)
                                {
                                    try
                                    {
                                        var h = gw2.MainWindowHandle;
                                        if (NativeMethods.GetForegroundWindow() == h)
                                            AccountWindowEvent(account.Settings, new AccountWindowEventEventArgs(AccountWindowEventEventArgs.EventType.Focused, gw2, h));
                                    }
                                    catch { }
                                }

                                if (Settings.RunAfterLaunching.HasValue)
                                    RunAfter(Settings.RunAfterLaunching.Value, account, gw2);

                                if (!string.IsNullOrEmpty(account.Settings.RunAfterLaunching))
                                    RunAfter(account.Settings.RunAfterLaunching, account, gw2);

                                if (mode == LaunchMode.Launch)
                                {
                                    if (Settings.DelayLaunchUntilLoaded.Value)
                                    {
                                        using (cancel.Register(delegate
                                        {
                                            windowState = 4;
                                            waiter.Set();
                                        }))
                                        {
                                            while (windowState < 3 && waiter != null)
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
                        if (gw2 != null)
                            duration = gw2.ExitTime.Subtract(gw2.StartTime).TotalSeconds;
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
                                if (!KillMutex())
                                {
                                    retries++;

                                    try
                                    {
                                        //this should only happen if there are multiple GW2 installs, in which case trying to find it may be impossible (renamed)
                                        //rather than trying to first find a GW2 process, the entire system will be searched once

                                        if (!Util.ProcessUtil.KillMutexWindow(false))
                                            Util.ProcessUtil.KillMutexWindow(true);
                                    }
                                    catch (Exception e)
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
                            if (mode == LaunchMode.Launch && Util.Args.Contains(processOptions.Arguments, "nopatchui"))
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
                    if (watcher != null)
                    {
                        watcher.WindowChanged -= onChanged;
                        waiter.Dispose();
                    }
                }
            }
            while (retries < 2);

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
            int count = 0;

            try
            {
                if (Settings.GW2Path.HasValue && !string.IsNullOrEmpty(Settings.GW2Path.Value))
                {
                    var fi = new FileInfo(Settings.GW2Path.Value);
                    count = Scan(fi, ScanOptions.KillAll);
                }
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

        private static bool IsAutomaticLogin(Settings.IAccount account)
        {
            return !Settings.DisableAutomaticLogins && account.AutomaticLogin && account.HasCredentials;
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
            StringBuilder args = new StringBuilder(256);

            args.Append(ARGS_UID);
            args.Append(account.UID);

            if (mode == LaunchMode.Update || mode == LaunchMode.UpdateVisible)
            {
                if (mode != LaunchMode.UpdateVisible)
                {
                    args.Append(" -nopatchui");
                }

                args.Append(" -image");

                if (Settings.MaxPatchConnections.HasValue)
                {
                    args.Append(" -patchconnections ");
                    args.Append(Settings.MaxPatchConnections.Value);
                }
            }
            else
            {
                var disableAutologin = Settings.DisableAutomaticLogins || account.AutomaticLogin && account.HasCredentials;

                if (!string.IsNullOrEmpty(arguments))
                {
                    args.Append(' ');
                    if (disableAutologin)
                        args.Append(Util.Args.AddOrReplace(arguments, "autologin", ""));
                    else
                        args.Append(arguments);
                }

                if (mode == LaunchMode.Launch)
                    args.Append(" -shareArchive");

                if (IsWindowed(account))
                    args.Append(" -windowed");

                if (!string.IsNullOrEmpty(account.Arguments))
                {
                    args.Append(' ');
                    if (disableAutologin)
                        args.Append(Util.Args.AddOrReplace(account.Arguments, "autologin", ""));
                    else
                        args.Append(account.Arguments);
                }

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

                if (!disableAutologin && (account.AutomaticRememberedLogin || Settings.AutomaticRememberedLogin.Value))
                    args.Append(" -autologin");

                if (account.ClientPort != 0 || Settings.ClientPort.HasValue)
                {
                    args.Append(" -clientport ");
                    if (account.ClientPort != 0)
                        args.Append(account.ClientPort);
                    else
                        args.Append(Settings.ClientPort.Value);
                }

                var mute = account.Mute;
                if (Settings.Mute.HasValue)
                    mute |= Settings.Mute.Value;

                if (mute != Settings.MuteOptions.None)
                {
                    if (mute.HasFlag(Settings.MuteOptions.All))
                    {
                        args.Append(" -nosound");
                    }
                    else
                    {
                        if (mute.HasFlag(Settings.MuteOptions.Music))
                            args.Append(" -nomusic");
                        if (mute.HasFlag(Settings.MuteOptions.Voices))
                            args.Append(" -novoice");
                    }
                }


                if (account.ScreenshotsFormat == Settings.ScreenshotFormat.Bitmap || Settings.ScreenshotsFormat.Value == Settings.ScreenshotFormat.Bitmap)
                    args.Append(" -bmp");
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

            if (mode == LaunchMode.LaunchSingle)
                a = Util.Args.AddOrReplace(a, "sharearchive", "");

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

        private static ProcessOptions GetProcessStartInfo(Settings.IAccount account, FileManager.IProfileInformation customProfile, LaunchMode mode, FileInfo fi, string args)
        {
            var options = new ProcessOptions();

            options.FileName = fi.FullName;
            options.Arguments = GetArguments(account, Settings.GW2Arguments.Value, args, mode);
            options.WorkingDirectory = fi.DirectoryName;

            if (!Util.Users.IsCurrentUser(account.WindowsAccount))
            {
                options.UserName = account.WindowsAccount;
                var password = Security.Credentials.GetPassword(account.WindowsAccount);
                if (password == null)
                    throw new BadUsernameOrPasswordException();
                options.Password = password;
            }

            var temp = Path.Combine(DataPath.AppDataAccountDataTemp, account.UID.ToString());
            if (!Directory.Exists(temp))
                Directory.CreateDirectory(temp);

            options.Variables[ProcessOptions.VAR_TEMP] = temp;

            if (customProfile != null)
            {
                options.Variables[ProcessOptions.VAR_USERPOFILE] = customProfile.UserProfile;
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

            return options;
        }

        private static bool KillMutex()
        {
            bool killed = false;

            foreach (LinkedProcess p in LinkedProcess.GetActive())
            {
                try
                {
                    if (p.HasMutex && p.KillMutex())
                    {
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
            if (fi == null)
                ps = new Process[0];
            else
                ps = Process.GetProcesses();

            if (ps.Length > 0)
            {
                var processName = fi.Name.Substring(0, fi.Name.Length - fi.Extension.Length);
                var directory = fi.DirectoryName;

                using (var pi = new Windows.ProcessInfo())
                {
                    foreach (Process p in ps)
                    {
                        var used = false;

                        try
                        {
                            //searching for process matching either the name of the process (Gw2), or the name with an extension (Gw2.tmp)
                            var l = p.ProcessName.Length - processName.Length;
                            if (!(l == 0 || (l == 4 && p.ProcessName[processName.Length] == '.')) || !p.ProcessName.StartsWith(processName, StringComparison.OrdinalIgnoreCase))
                                continue;

                            if (LinkedProcess.Contains(p))
                            {
                                switch (options)
                                {
                                    case ScanOptions.KillLinked:
                                    case ScanOptions.KillAll:

                                        try
                                        {
                                            counter++;
                                            p.Kill();
                                        }
                                        catch { }

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
                            catch 
                            {
                                try
                                {
                                    if (p.HasExited)
                                    {
                                        counter++;
                                        continue;
                                    }
                                }
                                catch { }
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
                            
                            if (!Path.GetDirectoryName(path).StartsWith(directory, StringComparison.OrdinalIgnoreCase))
                                continue;

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
                                            used = true;

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

                                                    LaunchMode mode;
                                                    if (Util.Args.Contains(commandLine, "shareArchive"))
                                                        mode = LaunchMode.Launch;
                                                    else
                                                        mode = LaunchMode.LaunchSingle;

                                                    if (Util.Args.Contains(commandLine, "isRelaunch"))
                                                    {
                                                        if (Util.Args.Contains(commandLine, "nopatchui"))
                                                        {
                                                            //logging out with -nopatchui will cause the client to restart and log back in, except it can't and will sit on a black/white screen
                                                            #warning killing -nopatchui relaunches

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
                                                            //account was relaunched (logout)
                                                            if (mode == LaunchMode.Launch)
                                                            {
                                                                try
                                                                {
                                                                    account.Process.KillMutex();
                                                                }
                                                                catch { }
                                                            }
                                                        }
                                                    }

                                                    bool isWindowed = IsWindowed(account.Settings);

                                                    var watcher = account.Watcher = new WindowWatcher(account, p, false, isWindowed, mode, null);
                                                    watcher.WindowChanged += OnWatchedWindowChanged;
                                                    watcher.WindowCrashed += OnWatchedWindowCrashed;
                                                    watcher.AuthenticationRequired += OnWatchedWindowAuthenticationRequired;
                                                    watcher.Start();

                                                    try
                                                    {
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
                                    if (options == ScanOptions.KillAll || options == ScanOptions.KillLinked)
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
                                lock (unknownProcesses)
                                {
                                    if (!unknownProcesses.ContainsKey(p.Id))
                                    {
                                        used = true;
                                        unknownProcesses.Add(p.Id, p);
                                        if (taskWatchUnknowns == null || taskWatchUnknowns.IsCompleted)
                                        {
                                            taskWatchUnknowns = new Task(DoWatch, TaskCreationOptions.LongRunning);
                                            taskWatchUnknowns.Start();
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

            if (activeProcesses == 0)
            {
                lock (queueMulti)
                {
                    if (coherentLock != null)
                    {
                        coherentLock.Dispose();
                        coherentLock = null;
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
                        if (Monitor.TryEnter(queueMulti, TIMEOUT_TRYENTER))
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

            if (coherentMonitor != null)
                coherentMonitor.Remove(account.Settings);

            if (account.Icons != null)
            {
                account.Icons.Dispose();
                account.Icons = null;
            }

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
            WindowWatcher watcher = (WindowWatcher)sender;
            var settings = watcher.Account.Settings;

            switch (e.Type)
            {
                case WindowWatcher.WindowChangedEventArgs.EventType.HandleChanged:

                    if (Settings.WindowIcon.Value)
                    {
                        try
                        {
                            var icons = watcher.Account.Icons;

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
                                watcher.Account.Icons = icons;

                                NativeMethods.SendMessage(e.Handle, 0x0080, (IntPtr)0, icons.Small.Handle);
                                NativeMethods.SendMessage(e.Handle, 0x0080, (IntPtr)1, icons.Big.Handle);
                            }
                        }
                        catch (Exception ex)
                        {
                            Util.Logging.Log(ex);
                        }
                    }

                    break;
                case WindowWatcher.WindowChangedEventArgs.EventType.TitleChanged:

                    if (Settings.WindowCaption.HasValue)
                    {
                        var format = new StringBuilder(Settings.WindowCaption.Value)
                            .Replace("%accountname%", "{0}")
                            .Replace("%accountid%", "{1}")
                            .Replace("%processid%", "{2}")
                            .Replace("%handle%", "{3}");
                        WindowWatcher.SetText(e.Handle, string.Format(format.ToString(), settings.Name, settings.UID, watcher.Process.Id, e.Handle));
                    }

                    break;
                case WindowWatcher.WindowChangedEventArgs.EventType.DxWindowStyleChanged:

                    if (Settings.RepaintInitialWindow.Value)
                    {
                        RepaintWindow(e.Handle);
                    }

                    break;
                case WindowWatcher.WindowChangedEventArgs.EventType.DxWindowCreated:
                    
                    lock (watcher.Account)
                    {
                        if (watcher.Account.State == AccountState.Active)
                            watcher.Account.SetState(AccountState.ActiveGame, true);
                    }

                    var affinity = settings.ProcessAffinity;
                    if (affinity == 0 && Settings.ProcessAffinity.HasValue)
                        affinity = Settings.ProcessAffinity.Value;
                    if (affinity > 0)
                    {
                        try
                        {
                            watcher.Process.ProcessorAffinity = (IntPtr)affinity;
                        }
                        catch (Exception ex)
                        {
                            Util.Logging.Log(ex);
                        }
                    }

                    if (Settings.RepaintInitialWindow.Value)
                    {
                        RepaintWindow(e.Handle);
                    }

                    break;
                case WindowWatcher.WindowChangedEventArgs.EventType.DxWindowReady:

                    if (settings.VolumeEnabled)
                        watcher.SetVolume(settings.Volume);
                    else if (Settings.Volume.HasValue)
                        watcher.SetVolume(Settings.Volume.Value);

                    if (IsWindowed(settings))
                    {
                        var r = settings.WindowBounds;

                        if (!r.IsEmpty)
                            watcher.SetBounds(e.Handle, r, 30000);
                    }

                    try
                    {
                        var events = windowEvents.Add(watcher.Process.Id, watcher.Account);
                        events.MoveSizeEnd += events_MoveSizeEnd;
                        events.MoveSizeBegin += events_MoveSizeBegin;
                        events.MinimizeEnd += events_MinimizeEnd;
                        events.MinimizeStart += events_MinimizeStart;
                    }
                    catch (Exception ex)
                    {
                        Util.Logging.Log(ex);
                    }

                    var priority = settings.ProcessPriority;
                    if (priority == Settings.ProcessPriorityClass.None && Settings.ProcessPriority.HasValue)
                        priority = Settings.ProcessPriority.Value;
                    if (priority != Settings.ProcessPriorityClass.None && priority != Settings.ProcessPriorityClass.Normal)
                    {
                        if (processPriority == null)
                        {
                            lock(queue)
                            {
                                if (processPriority == null)
                                    processPriority = new Tools.ProcessPriority();
                            }
                        }

                        try
                        {
                            processPriority.SetPriority(watcher.Process, GetPriority(priority));
                        }
                        catch (Exception ex)
                        {
                            Util.Logging.Log(ex);
                        }
                    }

                    if (Settings.PrioritizeCoherentUI.Value)
                    {
                        if (coherentMonitor == null)
                        {
                            lock(queue)
                            {
                                if (coherentMonitor == null)
                                    coherentMonitor = new Tools.CoherentMonitor();
                            }
                        }

                        coherentMonitor.Add(settings, watcher.Process);
                    }

                    if (AccountWindowEvent != null)
                        AccountWindowEvent(settings, new AccountWindowEventEventArgs(AccountWindowEventEventArgs.EventType.WindowReady, watcher.Process, e.Handle));

                    break;
                case WindowWatcher.WindowChangedEventArgs.EventType.LauncherCoherentUIReady:
                    
                    if (settings.AutomaticLogin && settings.HasCredentials || !Settings.DisableAutomaticLogins && settings.AutomaticPlay && watcher.ProcessOptions != null && Util.Args.Contains(watcher.ProcessOptions.Arguments, "autologin"))
                    {
                        autologin.Queue(watcher.Account, watcher.Process);
                    }

                    if (watcher.Account.watcher == watcher)
                        watcher.Account.GfxLock = null;

                    break;
                case WindowWatcher.WindowChangedEventArgs.EventType.InGameCoherentUIReady:

                    OnWindowStateChanged(e.Handle, watcher.Account);

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

                    if (IsWindowed(settings))
                    {
                        var r = settings.WindowBounds;

                        if (!r.IsEmpty)
                            watcher.SetBounds(e.Handle, r, 30000);
                    }

                    break;
                case WindowWatcher.WindowChangedEventArgs.EventType.WatcherExited:

                    if (watcher.Account.watcher == watcher)
                    {
                        watcher.Account.watcher = null;
                        if (watcher.Account.watcher == watcher)
                            watcher.Account.GfxLock = null;
                    }

                    watcher.Dispose();

                    break;
            }
        }

        static void RepaintWindow(IntPtr handle)
        {
            try
            {
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
            var options = account.Settings.WindowOptions;

            if ((options & Settings.WindowOptions.Windowed) == Settings.WindowOptions.Windowed)
            {
                try
                {
                    if ((options & Settings.WindowOptions.PreventChanges) == Settings.WindowOptions.PreventChanges)
                        WindowWatcher.RemoveWindowStyle(handle, WindowStyle.WS_MAXIMIZEBOX);

                    if ((options & Settings.WindowOptions.TopMost) == Settings.WindowOptions.TopMost)
                    {
                        bool handled;

                        if (AccountWindowEvent != null)
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
            var options = e.Account.Settings.WindowOptions;

            if ((options & Settings.WindowOptions.PreventChanges) == Settings.WindowOptions.PreventChanges)
            {
                try
                {
                    NativeMethods.PostMessage(e.Handle, (int)WindowMessages.WM_CANCELMODE, IntPtr.Zero, IntPtr.Zero);
                }
                catch (Exception ex)
                {
                    Util.Logging.Log(ex);
                }
            }
        }

        static void events_MoveSizeEnd(object sender, WindowEvents.WindowEventsEventArgs e)
        {
            var options = e.Account.Settings.WindowOptions;

            if ((options & Settings.WindowOptions.Windowed) == Settings.WindowOptions.Windowed)
            {
                var handle = e.Handle;
                var changes = options & (Settings.WindowOptions.PreventChanges | Settings.WindowOptions.RememberChanges);

                if (changes != Settings.WindowOptions.None)
                {
                    try
                    {
                        var placement = new WINDOWPLACEMENT();

                        if (NativeMethods.GetWindowPlacement(handle, ref placement))
                        {
                            var r = placement.rcNormalPosition.ToRectangle();
                            var bounds = e.Account.Settings.WindowBounds;

                            if (bounds != r)
                            {
                                switch (changes)
                                {
                                    case Settings.WindowOptions.RememberChanges:

                                        e.Account.Settings.WindowBounds = r;

                                        break;
                                    case Settings.WindowOptions.PreventChanges | Settings.WindowOptions.RememberChanges:
                                    case Settings.WindowOptions.PreventChanges:

                                        //this is only a backup and will only happen if the cancel message failed
                                        //note that the window bounds cannot be set until the actual operation finishes

                                        Task.Run(new Action(
                                            delegate
                                            {
                                                try
                                                {
                                                    Thread.Sleep(100);

                                                    placement.rcNormalPosition = RECT.FromRectangle(bounds);
                                                    NativeMethods.SetWindowPlacement(handle, ref placement);
                                                }
                                                catch { }
                                            }));

                                        break;
                                }
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Util.Logging.Log(ex);
                    }
                }

                OnWindowStateChanged(handle, e.Account);
            }
        }

        static void events_ForegroundChanged(object sender, WindowEvents.WindowEventsEventArgs e)
        {
            if (AccountWindowEvent != null)
                AccountWindowEvent(e.Account.Settings, new AccountWindowEventEventArgs(AccountWindowEventEventArgs.EventType.Focused, e.Account.Process.Process, e.Handle));
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
                var _queue = _mode == LaunchMode.LaunchSingle ? queueSingle : queueMulti;
                var _ql = new QueuedLaunch(_account, _mode, _args);
                _ql.Dequeued += delegate(object o, QueuedLaunch.DequeuedState state)
                {
                    if (state == QueuedLaunch.DequeuedState.Skipped)
                        error(_account, true);
                };
                _queue.Enqueue(_ql);
                if (AccountQueued != null)
                    AccountQueued(_account.Settings, _mode);
            };

            bool retry = false;

            lock (queueMulti)
            {
                if (onlyShowIfNoActiveProcesses && activeProcesses == 0 || !onlyShowIfNoActiveProcesses)
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
                            lock (queueMulti)
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
                        else if (account.inQueueCount == 0 && account.errors == 1)
                        {
                            ql = new QueuedLaunch(null, LaunchMode.Update);
                            ql.Dequeued += onDequeue;
                            queueSingle.Enqueue(ql);
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

        private static void OnWatchedWindowCrashed(object sender, WindowWatcher.CrashReason e)
        {
            if (e == WindowWatcher.CrashReason.PatchRequired || e == WindowWatcher.CrashReason.NoPatchUI)
            {
                WindowWatcher watcher = (WindowWatcher)sender;
                var account = watcher.Account;
                var p = account.Process.Process;

                try
                {
                    if (e == WindowWatcher.CrashReason.NoPatchUI)
                    {
                        OnUpdateRequired(watcher.Account, watcher.Mode, watcher.Args, true, "An update may be required; client exited unexpectedly");
                    }
                    else if (p != null && !p.HasExited)
                    {
                        p.Kill();
                        p.WaitForExit();

                        OnUpdateRequired(watcher.Account, watcher.Mode, watcher.Args, false, "An update is required; client is out of date");
                    }
                }
                catch (Exception ex)
                {
                    Util.Logging.Log(ex);
                }
            }
        }

        static void OnWatchedWindowAuthenticationRequired(object sender, Tools.ArenaAccount e)
        {
            var watcher = (WindowWatcher)sender;
            var account = watcher.Account;
            var p = account.Process.Process;

            bool retry = false;

            try
            {
                if (p != null && !p.HasExited)
                {
                    p.Kill();
                    p.WaitForExit();

                    WaitForScanner();

                    lock (queueMulti)
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

                            if (account.Settings.HasCredentials && account.Settings.NetworkAuthorizationState == Settings.NetworkAuthorizationState.Unknown && Settings.NetworkAuthorization.HasValue)
                            {
                                if (account.inQueueCount == 0 && account.errors == 1)
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
                                    var queue = watcher.Mode == LaunchMode.LaunchSingle ? queueSingle : queueMulti;
                                    queue.Enqueue(new QueuedLaunch(account, watcher.Mode, watcher.Args)
                                        {
                                            session = e
                                        });
                                    if (AccountQueued != null)
                                        AccountQueued(account.Settings, watcher.Mode);

                                    retry = true;
                                }
                            }
                        }

                        if (!retry && account.inQueueCount == 0 && account.State == AccountState.None)
                        {
                            if (Monitor.TryEnter(account, TIMEOUT_TRYENTER))
                            {
                                try
                                {
                                    account.SetState(AccountState.Error, true, new Exception("The window timed out while loading; verify the login credentials and network authorization"));
                                    account.SetState(AccountState.None, false);
                                }
                                finally
                                {
                                    Monitor.Exit(account);
                                }
                            }
                        }
                    }

                    if (retry)
                        StartQueue();
                }
            }
            catch (Exception ex)
            {
                Util.Logging.Log(ex);
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
