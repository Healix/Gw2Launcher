using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;

namespace Gw2Launcher.Client
{
    static partial class Launcher
    {
        private class LinkedProcess : IDisposable
        {
            public static event EventHandler<Account> ProcessExited;
            public static event EventHandler<Account> ProcessActive;

            public event EventHandler<Account> Exited;
            public event EventHandler<Process> Changed;

            private static Dictionary<int, LinkedProcess> activeProcesses = new Dictionary<int, LinkedProcess>();

            private class ProcessWatcher : IDisposable
            {
                public event EventHandler Exited;

                private Process process;
                private Task waiter;

                public ProcessWatcher(Process p)
                {
                    this.process = p;
                    this.waiter = new Task(WaitForExit, TaskCreationOptions.LongRunning);
                    this.waiter.Start();
                }

                private void WaitForExit()
                {
                    this.process.WaitForExit();

                    if (Exited != null)
                        Exited(this.process, null);
                }

                public void Dispose()
                {
                    if (waiter != null)
                    {
                        if (waiter.IsCompleted)
                            waiter.Dispose();
                        waiter = null;
                    }
                }
            }

            private Account account;
            private ProcessWatcher watcher;

            public static int[] GetActivePIDs()
            {
                lock (activeProcesses)
                {
                    return activeProcesses.Keys.ToArray();
                }
            }

            public static LinkedProcess[] GetActive()
            {
                lock (activeProcesses)
                {
                    LinkedProcess[] active = new LinkedProcess[activeProcesses.Count];
                    int i = 0;

                    foreach (LinkedProcess p in activeProcesses.Values)
                    {
                        active[i++] = p;
                    }

                    return active;
                }
            }

            public static IEnumerable<LinkedProcess> GetActiveEnumerable()
            {
                lock (activeProcesses)
                {
                    var values = activeProcesses.Values;

                    foreach (var p in activeProcesses.Values)
                    {
                        yield return p;
                    }
                }
            }

            public static int GetActiveCount()
            {
                return GetActiveCount(AccountType.Any);
            }

            public static int GetActiveCount(AccountType type, AccountState state = AccountState.None)
            {
                lock (activeProcesses)
                {
                    Settings.AccountType t;
                    int count = 0;

                    switch (type)
                    {
                        case AccountType.GuildWars2:

                            t = Settings.AccountType.GuildWars2;

                            break;
                        case AccountType.GuildWars1:

                            t = Settings.AccountType.GuildWars1;

                            break;
                        case AccountType.Any:

                            if (state == AccountState.None)
                                return activeProcesses.Count;

                            foreach (var p in activeProcesses.Values)
                            {
                                if (p.account != null && p.account.State == state)
                                    count++;
                            }

                            return count;

                        default:

                            throw new NotSupportedException();
                    }

                    foreach (var p in activeProcesses.Values)
                    {
                        if (p.account != null && p.account.Settings.Type == t && (state == AccountState.None || p.account.State == state))
                            count++;
                    }

                    return count;
                }
            }

            public static Account GetAccount(Process p)
            {
                return GetAccount(p.Id);
            }

            public static Account GetAccount(int pid)
            {
                lock (activeProcesses)
                {
                    LinkedProcess l;
                    if (activeProcesses.TryGetValue(pid, out l))
                    {
                        return l.account;
                    }
                    return null;
                }
            }

            public static bool Contains(Process p)
            {
                return Contains(p.Id);
            }

            public static bool Contains(int pid)
            {
                lock (activeProcesses)
                {
                    return activeProcesses.ContainsKey(pid);
                }
            }

            public LinkedProcess(Account account)
            {
                this.account = account;
            }

            public Process Launch(ProcessStartInfo startInfo)
            {
                Process p = new Process();
                p.StartInfo = startInfo;

                Attach(p);

                if (p.Start())
                {
                    lock (activeProcesses)
                    {
                        if (!p.HasExited)
                        {
                            activeProcesses.Add(p.Id, this);

                            if (ProcessActive != null)
                                ProcessActive(p, this.account);
                        }
                    }
                }

                return p;
            }

            public void Attach(Process p)
            {
                lock (this)
                {
                    _Attach(p);
                }
            }

            private void _Attach(Process p)
            {
                if (this.Process != null)
                {
                    try
                    {
                        int pid = this.Process.Id;
                        lock (activeProcesses)
                        {
                            activeProcesses.Remove(pid);
                        }
                    }
                    catch (Exception e)
                    {
                        Util.Logging.Log(e);
                    }

                    if (watcher != null)
                    {
                        watcher.Dispose();
                        watcher = null;
                    }
                    else
                    {
                        this.Process.Exited -= p_Exited;
                    }
                }

                bool hasExited = false;
                bool hasStarted = false;

                try
                {
                    hasStarted = p.Id > 0;
                    hasExited = p.HasExited;
                }
                catch (Exception ex)
                {
                    Util.Logging.Log(ex);
                }

                this.Process = p;

                if (!hasExited)
                {
                    bool supportsEvents = false;

                    if (!p.EnableRaisingEvents)
                    {
                        //try setting the value as the current user, then the user running the process
                        //the current user will be able to set another user's process if this process is still the owner (ownership is lost when this process is closed)
                        try
                        {
                            p.EnableRaisingEvents = true;
                            supportsEvents = true;
                        }
                        catch  (Exception e)
                        {
                            Util.Logging.Log(e);

                            if (!Util.Users.IsCurrentUser(account.Settings.WindowsAccount))
                            {
                                string username = Util.Users.GetUserName(account.Settings.WindowsAccount);

                                try
                                {
                                    using (Security.Impersonation.Impersonate(username, Security.Credentials.GetPassword(username)))
                                    {
                                        p.EnableRaisingEvents = true;
                                        supportsEvents = true;
                                    }
                                }
                                catch (Exception ex)
                                {
                                    Util.Logging.Log(ex);
                                }
                            }
                        }
                    }
                    else
                        supportsEvents = true;

                    if (supportsEvents)
                    {
                        p.Exited += p_Exited;
                    }
                    else
                    {
                        watcher = new ProcessWatcher(p);
                        watcher.Exited += p_Exited;
                    }

                    if (hasStarted && p.HasExited)
                    {
                        this.HasMutex = false;
                    }
                    else
                    {
                        this.HasMutex = true;
                    }
                }
                else
                {
                    this.HasMutex = false;

                    p_Exited(p, null);
                }

                if (hasStarted)
                {
                    lock (activeProcesses)
                    {
                        if (!p.HasExited)
                        {
                            activeProcesses.Add(p.Id, this);

                            if (ProcessActive != null)
                                ProcessActive(p, this.account);
                        }
                    }
                }
            }

            void p_Exited(object sender, EventArgs e)
            {
                Process p = sender as Process;

                var pid = p.Id;

                lock (activeProcesses)
                {
                    activeProcesses.Remove(pid);
                }

                bool b;

                lock (this)
                {
                    if (b = p == this.Process)
                        this.Process = null;
                }

                if (b)
                {
                    //using (var pi = new Windows.ProcessInfo())
                    //{
                    //    foreach (var process in Process.GetProcesses())
                    //    {
                    //        var dispose = true;
                    //        if (b)
                    //        {
                    //            try
                    //            {
                    //                if (pi.Open(process.Id))
                    //                {
                    //                    if (pi.GetParent() == pid)
                    //                    {
                    //                        b = false;
                    //                        dispose = false;

                    //                        Attach(process);
                    //                    }
                    //                }
                    //            }
                    //            catch (Exception ex)
                    //            {
                    //                Util.Logging.Log(ex);
                    //            }
                    //        }
                    //        if (dispose)
                    //            process.Dispose();
                    //    }
                    //}

                    IDisposable token = null;
                    if (!Util.Users.IsCurrentEnvironmentUser())
                    {
                        try
                        {
                            token = Security.Impersonation.Impersonate();
                        }
                        catch (Exception ex)
                        {
                            Util.Logging.Log(ex);
                        }
                    }

                    using (token)
                    {
                        if (Exited != null)
                            Exited(this, this.account);

                        if (ProcessExited != null)
                            ProcessExited(p, this.account);
                    }
                }
            }

            private Process _process;
            public Process Process
            {
                get
                {
                    return _process;
                }
                private set
                {
                    if (_process != value)
                    {
                        _process = value;
                        if (Changed != null)
                            Changed(this, value);
                    }
                }
            }

            public bool HasMutex
            {
                get;
                private set;
            }

            public Account Account
            {
                get
                {
                    return account;
                }
            }

            public bool KillMutex(bool test = false)
            {
                if (!Util.Users.IsCurrentUser(account.Settings.WindowsAccount))
                {
                    string username = Util.Users.GetUserName(account.Settings.WindowsAccount);
                    System.Security.SecureString password = Security.Credentials.GetPassword(username);
                    try
                    {
                        using (Security.Impersonation.Impersonate(username, password))
                        {
                            if (!_KillMutex(test))
                                return false;
                        }
                    }
                    catch (Exception e)
                    {
                        Util.Logging.Log(e);
                        Util.ProcessUtil.KillMutexWindow(account.Settings.Type, this.Process.Id, username, password);
                    }
                }
                else if (!_KillMutex(test))
                {
                    return false;
                }

                this.HasMutex = false;

                return true;
            }

            private bool _KillMutex(bool test)
            {
                if (test && !IsMutexOpen(account.Type))
                {
                    return true;
                }

                var p = this.Process;
                var hasWindow = false;
                var name = Util.ProcessUtil.GetMutexName(this.account.Settings.Type);
                var limit = DateTime.UtcNow.AddSeconds(30);

                //wait for the window to be created before checking the mutex

                do
                {
                    var h = Windows.FindWindow.FindMainWindow(p);
                    if (h != IntPtr.Zero)
                    {
                        hasWindow = true;
                        break;
                    }
                    else if (DateTime.UtcNow > limit || p.WaitForExit(100))
                    {
                        return false;
                    }
                }
                while (true);

                var handle = Windows.Win32Handles.GetHandle(p.Id, Windows.Win32Handles.HandleType.Mutex, name, Windows.Win32Handles.MatchMode.EndsWith);

                if (handle != null)
                {
                    handle.Kill();
                    return true;
                }
                else
                {
                    return hasWindow;
                }
            }

            public void Dispose()
            {
                if (watcher != null)
                {
                    watcher.Dispose();
                    watcher = null;
                }
            }
        }
    }
}
