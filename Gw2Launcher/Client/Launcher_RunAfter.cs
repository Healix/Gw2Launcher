using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.Diagnostics;
using System.IO;

namespace Gw2Launcher.Client
{
    static partial class Launcher
    {
        public static IRunAfterManager GetRunAfter(Settings.IAccount account)
        {
            return RunAfterManager.Create(GetAccount(account), false);
        }

        public interface IRunAfterProcess
        {
            event EventHandler Started;
            event EventHandler Exited;

            Settings.RunAfter RunAfterSettings
            {
                get;
            }

            Process Process
            {
                get;
            }

            bool IsActive
            {
                get;
            }

            bool WasStarted
            {
                get;
            }
        }

        public interface IRunAfterManager : IDisposable
        {
            event EventHandler ProcessesChanged;

            /// <summary>
            /// Starts the process from settings and returns the process
            /// </summary>
            IRunAfterProcess Start(Settings.RunAfter r, bool silent);

            /// <summary>
            /// Starts the process
            /// </summary>
            /// <returns>False if the process was already started or failed to start</returns>
            bool Start(IRunAfterProcess p, bool silent);

            /// <summary>
            /// All run after processes linked to the account
            /// </summary>
            /// <returns></returns>
            IRunAfterProcess[] GetProcesses();

            Settings.IAccount Account
            {
                get;
            }
        }

        private class RunAfterManagerSubscriber : IRunAfterManager
        {
            public event EventHandler ProcessesChanged;

            private RunAfterManager m;

            public RunAfterManagerSubscriber(RunAfterManager m)
            {
                this.m = m;

                m.ProcessesChanged += m_ProcessesChanged;

                lock (m)
                {
                    ++m.Subscribers;
                }
            }

            void m_ProcessesChanged(object sender, EventArgs e)
            {
                if (ProcessesChanged != null)
                    ProcessesChanged(this, e);
            }

            ~RunAfterManagerSubscriber()
            {
                Dispose();
            }

            public IRunAfterProcess Start(Settings.RunAfter r, bool silent)
            {
                return m.Start(r, silent);
            }

            public bool Start(IRunAfterProcess p, bool silent)
            {
                return m.Start(p, silent);
            }

            public IRunAfterProcess[] GetProcesses()
            {
                return m.GetProcesses();
            }

            public Settings.IAccount Account
            {
                get
                {
                    return m.Account;
                }
            }

            public void Dispose()
            {
                lock (this)
                {
                    if (m != null)
                    {
                        GC.SuppressFinalize(this);

                        ProcessesChanged = null;
                        m.ProcessesChanged -= m_ProcessesChanged;

                        lock (m)
                        {
                            --m.Subscribers;
                        }

                        m = null;
                    }
                }
            }
        }

        private class RunAfterManager : IRunAfterManager, IDisposable
        {
            public class RunAfterProcess : IRunAfterProcess
            {
                public event EventHandler Started;
                public event EventHandler Exited;

                public Process process;
                public Settings.RunAfter settings;
                public bool started;
                public bool removed;

                public RunAfterProcess(Settings.RunAfter r)
                {
                    settings = r;
                }

                public bool Start(RunAfterManager ra, bool silent)
                {
                    if (IsActive)
                        return false;

                    started = true;

                    try
                    {
                        ProcessStartInfo si;

                        if (settings.Type == Settings.RunAfter.RunAfterType.ShellCommands)
                        {
                            si = new ProcessStartInfo()
                            {
                                FileName = "cmd.exe",
                                RedirectStandardInput = true,
                                UseShellExecute = false,
                                CreateNoWindow = true
                            };
                        }
                        else
                        {
                            si = new ProcessStartInfo()
                            {
                                WorkingDirectory = Path.GetDirectoryName(settings.Path),
                                FileName = settings.Path,
                                Arguments = Variables.Replace(settings.Arguments, ra.GetVariables()),
                                UseShellExecute = false,
                            };
                        }

                        if ((settings.Options & Settings.RunAfter.RunAfterOptions.UseCurrentUser) == 0)
                        {
                            var username = ra.account.Settings.WindowsAccount;

                            if (!Util.Users.IsCurrentUser(username))
                            {
                                var password = Security.Credentials.GetPassword(username);
                                if (password != null)
                                {
                                    si.UserName = username;
                                    si.Password = password;
                                    si.LoadUserProfile = true;
                                }
                            }
                        }

                        process = new Process()
                        {
                            StartInfo = si,
                        };

                        if (process.Start())
                        {
                            if (settings.Type == Settings.RunAfter.RunAfterType.ShellCommands)
                            {
                                using (StreamWriter sw = process.StandardInput)
                                {
                                    if (sw.BaseStream.CanWrite)
                                    {
                                        sw.WriteLine(Variables.Replace(settings.Arguments, ra.GetVariables()));
                                    }
                                }
                            }

                            if (Started != null)
                                Started(this, EventArgs.Empty);

                            process.Exited += process_Exited;
                            process.EnableRaisingEvents = true; //will trigger if the process is already exited

                            return true;
                        }
                        else
                        {
                            process.Dispose();
                            process = null;
                        }
                    }
                    catch (Exception e)
                    {
                        Util.Logging.Log(e);

                        if (process != null)
                        {
                            process.Dispose();
                            process = null;
                        }

                        if (Util.Logging.Enabled)
                        {
                            Util.Logging.LogEvent(ra.Account, "Error when running [" + settings.GetName() + "]: " + e.Message);
                        }

                        if (!silent)
                            throw;
                    }

                    return false;
                }

                public void Close(bool kill)
                {
                    var p = process;

                    if (p != null)
                    {
                        try
                        {
                            if (!kill && p.CloseMainWindow())
                            {
                                return;
                            }

                            p.Kill();
                        }
                        catch { }
                    }
                }

                public void Close()
                {
                    if ((settings.Options & Settings.RunAfter.RunAfterOptions.KillOnExit) != 0)
                    {
                        Close(true);
                    }
                    else if ((settings.Options & Settings.RunAfter.RunAfterOptions.CloseOnExit) != 0)
                    {
                        Close(false);
                    }
                }

                void process_Exited(object sender, EventArgs e)
                {
                    lock (this)
                    {
                        if (process != null)
                        {
                            process.Dispose();
                            process = null;
                        }
                    }

                    if (Exited != null)
                        Exited(this, EventArgs.Empty);
                }

                public Settings.RunAfter RunAfterSettings
                {
                    get
                    {
                        return settings;
                    }
                }

                public Process Process
                {
                    get
                    {
                        return process;
                    }
                }

                public bool IsActive
                {
                    get
                    {
                        return process != null;
                    }
                }

                public bool WasStarted
                {
                    get
                    {
                        return started;
                    }
                }
            }

            public event EventHandler Disposing;
            public event EventHandler ProcessesChanged;

            [Flags]
            private enum ManagerState : byte
            {
                None = 0,
                /// <summary>
                /// Flag to refresh list of processes used by the account
                /// </summary>
                Refresh = 1,
                /// <summary>
                /// Flag when account is closing; automatic starts are cancelled
                /// </summary>
                Closing = 2,
                Disposing = 4,
                Disposed = 4 | 8,
            }

            private Account account;
            private RunAfterProcess[] processes;
            private Variables.DataSource variables;
            private ManagerState state;
            private ushort active;
            private byte subscribers;

            public RunAfterManager(Account account, RunAfterProcess[] processes)
            {
                this.account = account;
                this.processes = processes;

                account.Settings.RunAfterChanged += OnSettingsChanged;
                Settings.GetSettings(account.Settings.Type).RunAfter.ValueChanged += OnSettingsChanged;

                account.Process.Exited += OnAccountExited;
            }

            public static RunAfterProcess[] GetRunAfterProcesses(Account account)
            {
                var ra1 = account.Settings.RunAfter;
                var ra2 = Settings.GetSettings(account.Settings.Type).RunAfter.Value;

                var count = 0;

                if (ra1 != null)
                    count += ra1.Length;
                if (ra2 != null)
                    count += ra2.Length;

                if (count > 0)
                {
                    var ra = new RunAfterProcess[count];
                    var j = 0;

                    if (ra1 != null)
                    {
                        foreach (var r in ra1)
                        {
                            ra[j++] = new RunAfterProcess(r);
                        }
                    }

                    if (ra2 != null)
                    {
                        foreach (var r in ra2)
                        {
                            ra[j++] = new RunAfterProcess(r);
                        }
                    }

                    return ra;
                }

                return null;
            }

            /// <summary>
            /// Returns a subscriber to the manager
            /// </summary>
            /// <param name="reset">Allows processes that were already started to be started again</param>
            public static IRunAfterManager Create(Account account, bool reset)
            {
                RunAfterManagerSubscriber s;
                RunAfterManager m;

                lock (account)
                {
                    m = account.RunAfter;

                    if (m != null && !m.IsDisposed)
                    {
                        if (reset)
                            m.Reset();
                        return new RunAfterManagerSubscriber(m);
                    }
                    else
                    {
                        var p = GetRunAfterProcesses(account);

                        m = new RunAfterManager(account, p);
                        s = new RunAfterManagerSubscriber(m);

                        account.RunAfter = m;
                    }
                }

                if (RunAfterChanged != null)
                    RunAfterChanged(account.Settings);

                return s;
            }

            public static void Reset(RunAfterManager m)
            {
                if (m != null)
                    m.Reset();
            }

            public bool HasActive
            {
                get
                {
                    return active > 0;
                }
            }

            public bool IsDisposed
            {
                get
                {
                    return (state & ManagerState.Disposed) != 0; //disposed or disposing
                }
            }

            /// <summary>
            /// Subscribers using this manager
            /// </summary>
            public byte Subscribers
            {
                get
                {
                    return subscribers;
                }
                set
                {
                    subscribers = value;

                    if (value == 0 && active == 0)
                    {
                        DelayedDispose();
                    }
                }
            }

            public Settings.IAccount Account
            {
                get
                {
                    return account.Settings;
                }
            }

            public bool IsClosing
            {
                get
                {
                    return HasState(ManagerState.Closing);
                }
            }

            private bool HasState(ManagerState state)
            {
                return (this.state & state) == state;
            }

            /// <summary>
            /// Resets if process states (if not active)
            /// </summary>
            public void Reset()
            {
                var changed = false;

                lock (this)
                {
                    state &= ~ManagerState.Closing;

                    if (processes == null)
                        return;

                    foreach (var p in processes)
                    {
                        var b = p.IsActive;

                        if (p.started != b)
                        {
                            p.started = b;
                            changed = true;
                        }
                    }
                }

                if (changed && ProcessesChanged != null)
                    ProcessesChanged(this, EventArgs.Empty);
            }

            private void RefreshRunAfterProcesses()
            {
                Dictionary<Settings.RunAfter, RunAfterProcess> existing = null;

                if (this.processes != null)
                {
                    existing = new Dictionary<Settings.RunAfter, RunAfterProcess>(this.processes.Length);
                    foreach (var p in this.processes)
                    {
                        existing.Add(p.settings, p);
                    }
                }

                var processes = GetRunAfterProcesses(this.account);

                if (existing != null)
                {
                    for(var i = 0; i < processes.Length;i++)
                    {
                        RunAfterProcess p;
                        if (existing.TryGetValue(processes[i].settings, out p))
                        {
                            processes[i] = p;
                            existing.Remove(p.settings);
                        }
                    }

                    //keep any existing processes that are still active

                    var remaining = new RunAfterProcess[existing.Count];
                    var r = 0;

                    foreach (var p in existing.Values)
                    {
                        if (p.IsActive)
                        {
                            p.removed = true;
                            remaining[r++] = p;
                        }
                    }

                    if (r > 0)
                    {
                        Array.Resize<RunAfterProcess>(ref processes, processes.Length + r);
                        Array.Copy(remaining, 0, processes, processes.Length - r, r);
                    }
                }

                this.processes = processes;
            }

            private void OnSettingsChanged(object sender, EventArgs e)
            {
                lock (this)
                {
                    //processes won't be loaded until something wants it
                    if (HasState(ManagerState.Refresh))
                        return;
                    state |= ManagerState.Refresh;
                }

                if (ProcessesChanged != null)
                    ProcessesChanged(this, EventArgs.Empty);
            }

            private Variables.DataSource GetVariables()
            {
                var p = account.Process.Process;
                var v = variables;

                if (v == null)
                {
                    variables = v = new Variables.DataSource(account.Settings, p);
                }
                else
                {
                    v.Process = p;
                }

                return v;
            }

            void OnAccountExited(object sender, Account e)
            {
                Close();
            }

            //private int GetWaitIndex(Settings.RunAfter.RunAfterWhen o)
            //{
            //    switch (o)
            //    {
            //        case Settings.RunAfter.RunAfterWhen.None:
            //        case Settings.RunAfter.RunAfterWhen.Manual:
            //            return int.MaxValue;
            //        default:
            //            return (int)o;
            //    }
            //}

            /// <summary>
            /// Starts all attached processes at or below the given state, if they haven't already been started
            /// </summary>
            /// <param name="state">Starts processes configured to run at the specified state</param>
            /// <returns>Number of started processes</returns>
            public int Start(Settings.RunAfter.RunAfterWhen state)
            {
                RunAfterProcess[] processes;

                lock (this)
                {
                    processes = GetProcessesInternal();
                    if (processes == null || processes.Length == 0)
                        return 0;
                }

                //var s1 = GetWaitIndex(state);
                var started = 0;

                for (var i = 0; i < processes.Length; i++)
                {
                    var p = processes[i];

                    if (p.settings.Enabled)
                    {
                        //var s2 = GetWaitIndex(p.settings.When);

                        if (state == p.settings.When && !p.WasStarted && !p.removed)
                        {
                            lock (this)
                            {
                                if (HasState(ManagerState.Closing))
                                    break;

                                Start(p, true);

                                ++started;
                            }
                        }
                    }
                }

                return started;
            }

            /// <summary>
            /// Starts the process, adding it if it doesn't already exist
            /// </summary>
            public IRunAfterProcess Start(Settings.RunAfter r, bool silent)
            {
                RunAfterProcess p;

                lock (this)
                {
                    var processes = GetProcessesInternal();

                    if (processes == null)
                    {
                        this.processes = processes = new RunAfterProcess[] { p = new RunAfterProcess(r) };
                    }
                    else
                    {
                        p = null;

                        for (var i = processes.Length - 1; i >= 0; --i)
                        {
                            if (object.ReferenceEquals(processes[i].settings, r))
                            {
                                p = processes[i];
                                break;
                            }
                        }

                        if (p == null)
                        {
                            Array.Resize<RunAfterProcess>(ref processes, processes.Length + 1);
                            processes[processes.Length - 1] = p = new RunAfterProcess(r);
                            this.processes = processes;
                        }
                    }
                }

                Start(p, silent);

                return p;
            }

            /// <summary>
            /// Starts the process
            /// </summary>
            public bool Start(IRunAfterProcess p, bool silent)
            {
                if (p != null && p is RunAfterProcess)
                    return Start((RunAfterProcess)p, silent);
                return false;
            }

            /// <summary>
            /// Starts the process
            /// </summary>
            /// <returns>False if the process was already started or failed to start</returns>
            private bool Start(RunAfterProcess p, bool silent)
            {
                lock (p)
                {
                    if (p.IsActive)
                        return false;

                    p.Started += OnProcessStarted;

                    if (!p.Start(this, silent))
                    {
                        p.Started -= OnProcessStarted;

                        return false;
                    }

                    return true;
                }
            }

            void OnProcessStarted(object sender, EventArgs e)
            {
                var p = (RunAfterProcess)sender;

                lock (this)
                {
                    active++;
                }

                p.Exited += OnProcessExited;
                p.Started -= OnProcessStarted;
            }

            void OnProcessExited(object sender, EventArgs e)
            {
                var p = (RunAfterProcess)sender;

                lock (this)
                {
                    --active;

                    if (active == 0 && subscribers == 0)
                    {
                        DelayedDispose();
                    }
                }

                if (p.removed)
                {
                    OnSettingsChanged(null, null);
                }

                p.Exited -= OnProcessExited;
            }

            private RunAfterProcess[] GetProcessesInternal()
            {
                lock (this)
                {
                    if (HasState(ManagerState.Refresh))
                    {
                        state &= ~ManagerState.Refresh;
                        RefreshRunAfterProcesses();
                    }

                    return processes;
                }
            }

            public IRunAfterProcess[] GetProcesses()
            {
                lock (this)
                {
                    var processes = GetProcessesInternal();
                    if (processes == null)
                        return new IRunAfterProcess[0];

                    var _processes = new IRunAfterProcess[processes.Length];

                    Array.Copy(processes, _processes, processes.Length);

                    return _processes;
                }
            }

            /// <summary>
            /// Waits until processes exit
            /// </summary>
            public void Wait(CancellationToken cancel)
            {
                RunAfterProcess[] processes;

                lock (this)
                {
                    processes = this.processes;
                    if (processes == null || processes.Length == 0)
                        return;
                }

                for (var i = 0; i < processes.Length; i++)
                {
                    var p = processes[i];

                    if (!p.IsActive || (p.settings.Options & Settings.RunAfter.RunAfterOptions.WaitUntilComplete) == 0)
                        continue;

                    Wait(cancel, p);
                }
            }

            public bool Wait(CancellationToken cancel, IRunAfterProcess r)
            {
                if (!r.IsActive || cancel.IsCancellationRequested)
                    return false;

                var waiter = new ManualResetEvent(false);
                var exited = false;

                EventHandler onExit = delegate
                {
                    lock (waiter)
                    {
                        if (!exited)
                            waiter.Set();
                    }
                };

                r.Exited += onExit;
                this.Disposing += onExit;

                try
                {
                    if (!r.IsActive)
                        return true;

                    using (cancel.Register(new Action(
                        delegate
                        {
                            waiter.Set();
                        })))
                    {
                        if (!exited)
                        {
                            waiter.WaitOne();
                        }
                    }

                    return true;
                }
                catch 
                {
                    return false;
                }
                finally
                {
                    lock (waiter)
                    {
                        exited = true;

                        r.Exited -= onExit;
                        this.Disposing -= onExit;
                        waiter.Dispose();
                    }
                }
            }

            public void Close()
            {
                RunAfterProcess[] processes;

                lock (this)
                {
                    state |= ManagerState.Closing;
                    processes = this.processes;
                    if (processes == null)
                        return;
                }

                foreach (var p in processes)
                {
                    p.Close();
                }
            }

            private async void DelayedDispose()
            {
                await Task.Delay(500);

                lock (this)
                {
                    if (active != 0 || subscribers != 0 || IsDisposed)
                    {
                        return;
                    }

                    state |= ManagerState.Disposing;
                }

                Dispose();
            }

            public void Dispose()
            {
                lock (this)
                {
                    if (HasState(ManagerState.Disposed))
                        return;
                    state |= ManagerState.Disposed;
                }

                try
                {
                    if (Disposing != null)
                        Disposing(this, EventArgs.Empty);
                }
                catch { }

                lock (account)
                {
                    account.Process.Exited -= OnAccountExited;
                    account.Settings.RunAfterChanged -= OnSettingsChanged;
                    Settings.GetSettings(account.Settings.Type).RunAfter.ValueChanged -= OnSettingsChanged;

                    Disposing = null;
                    ProcessesChanged = null;

                    if (object.ReferenceEquals(account.RunAfter, this))
                    {
                        account.RunAfter = null;
                    }
                }

                Close();

                processes = null;
                variables = null;
            }
        }
    }
}
