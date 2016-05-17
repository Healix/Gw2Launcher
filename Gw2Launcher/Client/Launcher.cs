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
        public static event EventHandler AllQueuedLaunchesComplete;
        public static event EventHandler<int> ActiveProcessCountChanged;
        public static event BuildUpdatedEventHandler BuildUpdated;

        public enum AccountState
        {
            None,
            Waiting,
            Active,
            Launching,
            Updating,
            UpdatingVisible,
            WaitingForOtherProcessToExit,
            Error
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

        public class InvalidGW2PathException : Exception
        {
            public InvalidGW2PathException()
                : base("The path to Gw2.exe has not been set.")
            {

            }
        }

        public class BadUsernameOrPasswordException : Exception
        {
            public BadUsernameOrPasswordException()
                : base("The username or password is invalid.")
            {

            }
        }

        public class UserAlreadyActiveException : Exception
        {
            public UserAlreadyActiveException(string username)
                : base("The user \"" + username + "\" is already in use. Only accounts sharing the same Local.dat file can be active on a single user at the same time.")
            {

            }
        }

        public class DatFileNotInitialized : Exception
        {
            public DatFileNotInitialized()
                : base("The Local.dat file has not been initialized.")
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

        private struct QueuedLaunch
        {
            public QueuedLaunch(Account account, LaunchMode mode)
            {
                this.account = account;
                this.mode = mode;
            }

            public Account account;
            public LaunchMode mode;
        }

        private struct QueuedExit
        {
            public QueuedExit(Account account, int exitTime)
            {
                this.account = account;
                this.exitTime = exitTime;
            }

            public Account account;
            public int exitTime;
        }

        private const string ARGS_UID = "-uqid";
        private const ushort PROCESS_EXIT_DELAY = 1000;

        private static Dictionary<ushort, Account> accounts;
        private static Queue<QueuedLaunch> queue;
        private static Queue<QueuedExit> queueExit;
        private static Dictionary<int, Process> unknownProcesses;
        private static HashSet<string> activeUsers;
        private static int lastExit;
        private static Task taskQueue;
        private static Task taskScan;
        private static Task taskWatchUnknowns;
        private static ushort activeProcesses;
        private static CancellationTokenSource cancelQueue;
        private static ManualResetEvent activeProcessWait;
        private static QueuedLaunch lastLaunch;

        static Launcher()
        {
            accounts = new Dictionary<ushort, Account>();
            queue = new Queue<QueuedLaunch>();
            queueExit = new Queue<QueuedExit>();
            unknownProcesses = new Dictionary<int, Process>();
            activeUsers = new HashSet<string>();

            LinkedProcess.ProcessExited += LinkedProcess_ProcessExited;
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

        public static void Launch(Settings.IAccount account, LaunchMode mode)
        {
            Account _account;
            lock(accounts)
            {
                _account = GetAccount(account);

                if (_account.State != AccountState.None)
                    return;

                if (_account.InUse)
                    return;
                else
                    _account.InUse = true;
            }

            _account.SetState(AccountState.Waiting, true);

            lock (queue)
            {
                queue.Enqueue(new QueuedLaunch(_account, mode));

                if (taskQueue == null || taskQueue.IsCompleted)
                {
                    cancelQueue = new CancellationTokenSource();
                    var cancel = cancelQueue.Token;

                    taskQueue = Task.Factory.StartNew(
                        delegate
                        {
                            DoQueue(cancel);
                        }, cancel);
                }
            }
        }

        private static bool WaitOnActiveProcesses()
        {
            bool waiting;

            lock (queue)
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
                    while (!activeProcessWait.WaitOne())
                    {
                    }
                }
                catch { }
            }

            return waiting;
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

        private static void DoQueue(CancellationToken cancel)
        {
            bool next = false;
            int build = 0;
            Queue<QueuedLaunch> delayed = null;

            do
            {
                QueuedLaunch q;
                
                lock(queue)
                {
                    if (next)
                        queue.Dequeue();
                    else
                        next = true;

                    #region dumpQueue

                    if (cancel.IsCancellationRequested)
                    {
                        if (delayed != null)
                        {
                            while (delayed.Count > 0)
                            {
                                var _q = delayed.Dequeue();
                                _q.account.SetState(AccountState.None, true);
                                _q.account.InUse = false;
                            }
                        }

                        lock (queue)
                        {
                            while (queue.Count > 0)
                            {
                                var _q = queue.Dequeue();
                                _q.account.SetState(AccountState.None, true);
                                _q.account.InUse = false;
                            }

                            taskQueue = null;
                            return;
                        }
                    }

                    #endregion

                    #region complete

                    if (queue.Count == 0)
                    {
                        if (build > 0 && Settings.CheckForNewBuilds)
                            Settings.LastKnownBuild.Value = build;

                        if (delayed != null && delayed.Count > 0)
                        {
                            while (delayed.Count > 0)
                            {
                                var _q = delayed.Dequeue();
                                _q.account.SetState(AccountState.Waiting, true);
                                queue.Enqueue(_q);
                            }
                            delayed = null;
                        }
                        else
                        {
                            if (AllQueuedLaunchesComplete != null)
                                AllQueuedLaunchesComplete(null, EventArgs.Empty);

                            taskQueue = null;
                            return;
                        }
                    }

                    #endregion

                    q = queue.Peek();
                }

                if (lastLaunch.mode != LaunchMode.Launch && lastLaunch.account != null)
                {
                    var account = lastLaunch.account;
                    if (account.IsActive)
                    {
                        WaitForExit(account, cancel);
                        if (cancel.IsCancellationRequested)
                        {
                            next = false;
                            continue;
                        }
                    }
                }

                if (q.mode == LaunchMode.Launch)
                {
                    #region Check build

                    if (Settings.CheckForNewBuilds)
                    {
                        int b = Tools.Gw2Build.Build;
                        if (b > 0 && b != build && Settings.LastKnownBuild.Value != b)
                        {
                            build = b;

                            if (BuildUpdated != null)
                            {
                                var e = new BuildUpdatedEventArgs();
                                BuildUpdated(e);
                                if (e.Queue != null)
                                {
                                    lock(queue)
                                    {
                                        if (delayed == null)
                                            delayed = new Queue<QueuedLaunch>();

                                        while (queue.Count > 0)
                                            delayed.Enqueue(queue.Dequeue());

                                        int count = queue.Count;
                                        bool first = true;
                                        foreach (var _q in e.Queue)
                                        {
                                            Account account = GetAccount(_q);
                                            account.InUse = true;
                                            account.SetState(AccountState.Waiting, true);

                                            if (first)
                                            {
                                                first = false;
                                                queue.Enqueue(new QueuedLaunch(account, LaunchMode.UpdateVisible));
                                            }
                                            else
                                                queue.Enqueue(new QueuedLaunch(account, LaunchMode.Update));
                                        }
                                    }

                                    next = false;
                                    continue;
                                }
                            }
                        }
                    }

                    #endregion
                }
                else
                {
                    //launches running in normal mode can only run one client at a time
                    //these launches will be delayed until all clients are closed

                    if (WaitOnActiveProcesses())
                    {
                        next = false;
                        continue;
                    }
                }

                try
                {
                    try
                    {
                        Launch(q.account, q.mode);
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
                            throw e;
                    }
                }
                catch (Exception e)
                {
                    if (LaunchException != null)
                    {
                        try
                        {
                            var ex = new LaunchExceptionEventArgs(e);
                            
                            LaunchException(q.account.Settings, ex);

                            if (ex.Retry)
                            {
                                next = false;

                                #region DatFileNotInitialized

                                if (e is DatFileNotInitialized)
                                {
                                    if (!q.account.Settings.DatFile.IsInitialized)
                                    {
                                        queue.Dequeue();
                                        queue.Enqueue(new QueuedLaunch(q.account, LaunchMode.LaunchSingle));

                                        int i = queue.Count - 1;
                                        while (i-- > 0)
                                        {
                                            queue.Enqueue(queue.Dequeue());
                                        }
                                    }
                                }

                                #endregion
                            }
                            else if (e is InvalidGW2PathException)
                            {
                                #region InvalidGW2PathException

                                //dump the remaining queue
                                lock (queue)
                                {
                                    while (queue.Count > 0)
                                    {
                                        var _q = queue.Dequeue();
                                        _q.account.SetState(AccountState.Error, true, e);
                                        _q.account.SetState(AccountState.None, false);
                                        _q.account.InUse = false;
                                    }
                                    if (delayed != null)
                                    {
                                        while (delayed.Count > 0)
                                        {
                                            var _q = delayed.Dequeue();
                                            _q.account.SetState(AccountState.Error, true, e);
                                            _q.account.SetState(AccountState.None, false);
                                            _q.account.InUse = false;
                                        }
                                    }
                                }
                                next = false;

                                #endregion
                            }
                            else if (e is BadUsernameOrPasswordException)
                            {
                                #region BadUsernameOrPasswordException

                                //dump queued items that are using the same account
                                lock (queue)
                                {
                                    if (delayed != null && delayed.Count > 0)
                                    {
                                        //while running a build update, all accounts will be dropped on an error

                                        var aborted = queue.Dequeue();
                                        aborted.account.SetState(AccountState.Error, true, e);
                                        aborted.account.SetState(AccountState.None, false);
                                        aborted.account.InUse = false;

                                        while (queue.Count > 0)
                                        {
                                            var _q = queue.Dequeue();
                                            _q.account.SetState(AccountState.None, true);
                                            _q.account.InUse = false;
                                        }

                                        while (delayed.Count > 0)
                                        {
                                            var _q = delayed.Dequeue();
                                            if (_q.account != aborted.account)
                                                _q.account.SetState(AccountState.None, true);
                                            _q.account.InUse = false;
                                        }
                                    }
                                    else
                                    {
                                        string username = Util.Users.GetUserName(q.account.Settings.WindowsAccount);
                                        int i = queue.Count;
                                        while (i-- > 0)
                                        {
                                            var _q = queue.Dequeue();
                                            var _username = Util.Users.GetUserName(_q.account.Settings.WindowsAccount);
                                            if (_username.Equals(username, StringComparison.OrdinalIgnoreCase))
                                            {
                                                _q.account.SetState(AccountState.Error, true, e);
                                                _q.account.SetState(AccountState.None, false);
                                                _q.account.InUse = false;
                                            }
                                            else
                                            {
                                                queue.Enqueue(_q);
                                            }
                                        }
                                    }
                                }
                                next = false;

                                #endregion
                            }
                        }
                        catch { }
                    }

                    if (next)
                    {
                        q.account.SetState(AccountState.Error, true, e);
                        q.account.SetState(AccountState.None, false);
                    }
                }
                finally
                {
                    if (next)
                    {
                        q.account.InUse = false;
                    }
                }
            }
            while (true);
        }

        private static bool Launch(Account account, LaunchMode mode)
        {
            FileInfo fi;
            try
            {
                if (Settings.GW2Path.HasValue)
                    fi = new FileInfo(Settings.GW2Path.Value);
                else
                    fi = null;
            }
            catch
            {
                fi = null;
            }

            if (fi == null || !fi.Exists)
            {
                throw new InvalidGW2PathException();
            }

            return Launch(account, mode, fi);
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

        private static bool Launch(Account account, LaunchMode mode, FileInfo fi)
        {
            string username = Util.Users.GetUserName(account.Settings.WindowsAccount);
            Account _active = GetActiveAccount(username);
            if (_active != null)
            {
                //the user's account is already in use, however, if the same dat file is being used, it doesn't matter
                if (_active.Settings.DatFile != account.Settings.DatFile)
                    throw new UserAlreadyActiveException(username);
            }

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

            byte retries = 0;

            do
            {
                try
                {
                    DatManager.Activate(account.Settings);
                    break;
                }
                catch (DatManager.UserAccountNotInitializedException e)
                {
                    if (retries++ > 0)
                    {
                        throw e;
                    }

                    var password = Security.Credentials.GetPassword(username);
                    if (password == null)
                        throw new BadUsernameOrPasswordException();
                    try
                    {
                        Util.ProcessUtil.InitializeAccount(username, password);
                    }
                    catch { }
                }
            }
            while (true);

            if (mode == LaunchMode.Launch && !account.Settings.DatFile.IsInitialized)
            {
                throw new DatFileNotInitialized();
            }

            KillMutex();

            var startInfo = GetProcessStartInfo(account.Settings, mode, fi);
            var isWindowed = IsWindowed(account.Settings);

            retries = 0;

            do
            {
                Process gw2 = account.Process.Launch(startInfo);
                lastLaunch = new QueuedLaunch(account, mode);

                if (AccountProcessChanged != null)
                    AccountProcessChanged(account.Settings, gw2);

                if (!gw2.WaitForExit(2000))
                {
                    if (mode == LaunchMode.Launch || mode == LaunchMode.LaunchSingle)
                        account.SetState(AccountState.Active, true, gw2);

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
                            if (isWindowed)
                            {
                                WindowWatcher watcher = new WindowWatcher(account, gw2);
                                watcher.WindowChanged += OnWatchedWindowChanged;
                            }

                            if (AccountLaunched != null)
                                AccountLaunched(account.Settings);

                            EventHandler<Account> onExit = null;
                            onExit = delegate(object o, Account a)
                            {
                                a.Exited -= onExit;
                                if (AccountExited != null)
                                    AccountExited(a.Settings);
                            };
                            lock (queueExit)
                            {
                                if (account.State == AccountState.Active)
                                    account.Exited += onExit;
                            }
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

                        Thread.Sleep(500);

                        Task t = taskScan;
                        if (t != null)
                        {
                            try
                            {
                                t.Wait();
                            }
                            catch { }
                        }

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
                                catch
                                {
                                    //failed or cancelled
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

            account.SetState(AccountState.None, true);

            return false;
        }

        public static int GetActiveProcessCount()
        {
            return activeProcesses;
        }

        public static int GetPendingLaunchCount()
        {
            return queue.Count;
        }

        public static void CancelPendingLaunches()
        {
            lock (queue)
            {
                if (cancelQueue != null)
                    cancelQueue.Cancel();
            }
        }

        public static int KillActiveLaunches()
        {
            int count = 0;
            foreach (var l in LinkedProcess.GetActive())
            {
                var p = l.Process;
                if (p != null && !p.HasExited)
                {
                    try
                    {
                        p.Kill();
                        count++;
                    }
                    catch { }
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
                    if (!p.HasExited)
                    {
                        try
                        {
                            p.Kill();
                            count++;
                        }
                        catch { }
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

        public static List<Settings.IAccount> GetActive()
        {
            var active = LinkedProcess.GetActive();
            List<Settings.IAccount> accounts = new List<Settings.IAccount>(active.Length);

            foreach (var l in active)
            {
                accounts.Add(l.Account.Settings);
            }

            return accounts;
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

        private static string GetArguments(Settings.IAccount account, string arguments, LaunchMode mode)
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
                    args.Append(arguments);
                }

                if (mode == LaunchMode.Launch)
                    args.Append(" -shareArchive");

                if (IsWindowed(account))
                    args.Append(" -windowed");

                if (!string.IsNullOrEmpty(account.Arguments))
                {
                    args.Append(' ');
                    args.Append(account.Arguments);
                }

                if (!string.IsNullOrEmpty(account.AutomaticLoginEmail) && !string.IsNullOrEmpty(account.AutomaticLoginPassword))
                {
                    args.Append(" -nopatchui -email \"");
                    args.Append(account.AutomaticLoginEmail);
                    args.Append("\" -password \"");
                    args.Append(account.AutomaticLoginPassword);
                    args.Append('"');
                }
            }

            return args.ToString();
        }

        private static ProcessStartInfo GetProcessStartInfo(Settings.IAccount account, LaunchMode mode, FileInfo fi)
        {
            ProcessStartInfo startInfo = new ProcessStartInfo(fi.FullName, GetArguments(account, Settings.GW2Arguments.Value, mode));
            startInfo.UseShellExecute = false;

            if (!Util.Users.IsCurrentUser(account.WindowsAccount))
            {
                startInfo.UserName = account.WindowsAccount;
                var password = Security.Credentials.GetPassword(account.WindowsAccount);
                if (password == null)
                    throw new BadUsernameOrPasswordException();
                startInfo.Password = password;
                startInfo.LoadUserProfile = true;
                startInfo.WorkingDirectory = fi.DirectoryName;
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
                catch { }
            }

            return killed;
        }

        /// <summary>
        /// Looks for any running GW2 processes and if valid (started by this program), link them
        /// </summary>
        public static void Scan()
        {
            if (!Settings.GW2Path.HasValue)
                return;

            FileInfo fi;

            try
            {
                fi = new FileInfo(Settings.GW2Path.Value);
            }
            catch (Exception e)
            {
                return;
            }

            Scan(fi);
        }

        private static void Scan(FileInfo fi)
        {
            int startTime = Environment.TickCount;
            Process[] ps = Process.GetProcessesByName(fi.Name.Substring(0, fi.Name.Length - fi.Extension.Length));

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

                            if (account == null)
                            {
                                int uqid = -1;

                                try
                                {
                                    //string commandLine = GetProcessCommandLine(p);
                                    if (commandLine != null)
                                        uqid = GetUIDFromCommandLine(commandLine);
                                }
                                catch { }

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
                                        if (!account.IsActive)
                                            account.SetState(AccountState.Active, true, p);

                                        account.Process.Attach(p);
                                        
                                        if (IsWindowed(account.Settings))
                                        {
                                            WindowWatcher watcher = new WindowWatcher(account, p);
                                            watcher.WindowChanged += OnWatchedWindowChanged;
                                        }
                                    }
                                    else
                                    {
                                        isUnknown = true;
                                    }
                                }
                            }
                        }

                        if (isUnknown)
                        {
                            lock (unknownProcesses)
                            {
                                if (!unknownProcesses.ContainsKey(p.Id))
                                {
                                    unknownProcesses.Add(p.Id,p);
                                    if (taskWatchUnknowns == null || taskWatchUnknowns.IsCompleted)
                                        taskWatchUnknowns = Task.Factory.StartNew(DoWatch);
                                }
                                else
                                {
                                    p.Dispose();
                                }
                            }
                        }
                    }
                    else
                        p.Dispose();
                }
                catch (Exception e)
                {
                    p.Dispose();
                }
            }

            OnScanComplete(startTime);

            lock (unknownProcesses)
            {
                ushort activeProcesses = (ushort)(LinkedProcess.GetActiveCount() + unknownProcesses.Count);
                if (Launcher.activeProcesses != activeProcesses)
                {
                    Launcher.activeProcesses = activeProcesses;
                    OnActiveProcessCountChanged();
                }
            }
        }

        private static void DoScan()
        {
            if (!Settings.GW2Path.HasValue)
                return;

            FileInfo fi;

            try
            {
                fi = new FileInfo(Settings.GW2Path.Value);
            }
            catch (Exception e)
            {
                taskScan = null;
                return;
            }

            bool ranOnce = false;

            while (true)
            {
                lock (queueExit)
                {
                    if (queueExit.Count == 0 && ranOnce)
                    {
                        taskScan = null;
                        return;
                    }
                    ranOnce = true;
                }

                do
                {
                    int w = lastExit + PROCESS_EXIT_DELAY - Environment.TickCount;
                    if (w > 500)
                        w = 500;
                    else if (w < 50)
                        w = 50;
                    Thread.Sleep(w);
                }
                while (Environment.TickCount < lastExit + PROCESS_EXIT_DELAY);

                Scan(fi);
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
                            lastExit = Environment.TickCount;

                            if (taskScan == null || taskScan.IsCompleted)
                                taskScan = Task.Factory.StartNew(DoScan);
                        }

                        return;
                    }

                    foreach (Process _p in unknownProcesses.Values)
                    {
                        if (_p.HasExited)
                            unknownProcesses.Remove(_p.Id);
                        else
                            p = _p;
                        break;
                    }
                }

                if (p != null)
                    p.WaitForExit();
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
                    lock (queue)
                    {
                        if (activeProcessWait != null)
                            activeProcessWait.Set();
                    }
                }
            }
        }

        private static void OnScanComplete(int startTime)
        {
            lock (queueExit)
            {
                while (queueExit.Count > 0)
                {
                    var q = queueExit.Peek();
                    if (q.exitTime + PROCESS_EXIT_DELAY < startTime)
                        queueExit.Dequeue();
                    else
                        break;

                    if (q.account.Process.Process == null)
                    {
                        q.account.SetState(AccountState.None, true);
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
                queueExit.Enqueue(new QueuedExit(account, lastExit = Environment.TickCount));

                if (taskScan == null || taskScan.IsCompleted)
                    taskScan = Task.Factory.StartNew(DoScan);
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

        private static string[] GetProcessDetails(Process p)
        {
            string query = string.Format("SELECT ExecutablePath, CommandLine FROM Win32_Process WHERE ProcessId={0}", p.Id);

            using (ManagementObjectSearcher searcher = new ManagementObjectSearcher(query))
            {
                using (ManagementObjectCollection results = searcher.Get())
                {
                    foreach (ManagementObject o in results)
                    {
                        return new string[]
                        {
                            o["ExecutablePath"] as string,
                            o["CommandLine"] as string
                        };
                    }
                }
            }

            return null;
        }

        private static string GetProcessCommandLine(Process p)
        {
            string query = string.Format("SELECT CommandLine FROM Win32_Process WHERE ProcessId={0}", p.Id);

            using (ManagementObjectSearcher searcher = new ManagementObjectSearcher(query))
            {
                using (ManagementObjectCollection results = searcher.Get())
                {
                    foreach (ManagementObject o in results)
                    {
                        return o["CommandLine"] as string;
                    }
                }
            }

            return null;
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
            catch
            {
                return -1;
            }
        }

        private static void LinkedProcess_ProcessExited(object sender, Account e)
        {
            Process p = sender as Process;

            OnProcessExited(p, e);
        }

        private static void LinkedProcess_Changed(object sender, Process e)
        {
            LinkedProcess p = (LinkedProcess)sender;

            if (AccountProcessChanged != null)
                AccountProcessChanged(p.Account.Settings, e);
        }
    }
}
