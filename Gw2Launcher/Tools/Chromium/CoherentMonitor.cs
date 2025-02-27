using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Diagnostics;

namespace Gw2Launcher.Tools.Chromium
{
    class CoherentMonitor : IDisposable
    {
        private class AccountProcessInfo : ProcessInfo
        {
            [Flags]
            public enum MonitoredState : byte
            {
                None = 0,
                /// <summary>
                /// Allows monitoring
                /// </summary>
                Enabled = 1,
                /// <summary>
                /// Processes will be monitored if enabled
                /// </summary>
                Monitored = 2,
                /// <summary>
                /// Enabled and Monitored
                /// </summary>
                Monitoring = 3,
                /// <summary>
                /// Flagged for removal
                /// </summary>
                Removing = 4,
                /// <summary>
                /// No longer valid for the account
                /// </summary>
                Replaced = 8,
            }

            public Settings.IGw2Account account;
            public ProcessInfo[] processes;
            public MonitoredState state;

            public AccountProcessInfo()
            {
                processes = new ProcessInfo[5];
            }

            //public bool Enabled
            //{
            //    get
            //    {
            //        return (state & MonitoredState.Enabled) != 0;
            //    }
            //    set
            //    {
            //        if (value)
            //            state |= MonitoredState.Enabled;
            //        else
            //            state &= ~MonitoredState.Enabled;
            //    }
            //}

            //public bool Monitored
            //{
            //    get
            //    {
            //        return (state & MonitoredState.Monitored) != 0;
            //    }
            //    set
            //    {
            //        if (value)
            //            state |= MonitoredState.Monitored;
            //        else
            //            state &= ~MonitoredState.Monitored;
            //    }
            //}

            //public bool Removing
            //{
            //    get
            //    {
            //        return (state & MonitoredState.Removing) != 0;
            //    }
            //    set
            //    {
            //        if (value)
            //            state |= MonitoredState.Removing;
            //        else
            //            state &= ~MonitoredState.Removing;
            //    }
            //}

            public bool GetState(MonitoredState state)
            {
                return (this.state & state) == state;
            }

            public void SetState(MonitoredState state, bool value)
            {
                if (value)
                    this.state |= state;
                else
                    this.state &= ~state;
            }

            public void Add(ProcessInfo p)
            {
                for (var i = 0; i < processes.Length; i++)
                {
                    if (processes[i] == null)
                    {
                        processes[i] = p;
                        return;
                    }
                }

                Array.Resize(ref processes, processes.Length + 1);

                processes[processes.Length - 1] = p;
            }

            public void Remove(ProcessInfo p)
            {
                for (var i = 0; i < processes.Length; i++)
                {
                    if (processes[i] == null)
                        return;

                    if (object.ReferenceEquals(p, processes[i]))
                    {
                        for (var j = processes.Length - 1; j > i; --j)
                        {
                            if (processes[j] != null)
                            {
                                processes[i] = processes[j];
                                processes[j] = null;

                                return;
                            }
                        }

                        processes[i] = null;

                        return;
                    }
                }
            }
        }

        private class ProcessInfo : IDisposable
        {
            public int pid;
            public int parent;
            public Process process;

            public void Dispose()
            {
                if (parent != -1)
                    process.Dispose();
            }
        }

        private class QueueItem
        {
            public enum QueueType
            {
                Add,
                Remove,
                Removed,
                Exited,
                SettingsChanged,
            }

            public QueueItem()
            {

            }

            public QueueItem(Process p)
            {
                this.process = p;

                try
                {
                    pid = p.Id;
                }
                catch { }
            }

            public QueueType type;
            public Process process;
            public int pid;
            public Settings.IGw2Account account;
            public bool monitored;
        }

        private Task task;
        private Queue<QueueItem> queue;
        private Dictionary<ushort, AccountProcessInfo> accounts;
        private Dictionary<int, ProcessInfo> processes;
        private HashSet<int> unwatchable;
        private Windows.ProcessInfo processInfo;
        private bool refresh;
        private ushort monitored;

        public CoherentMonitor()
        {
            queue = new Queue<QueueItem>();
            accounts = new Dictionary<ushort, AccountProcessInfo>();
            processes = new Dictionary<int, ProcessInfo>();
            processInfo = new Windows.ProcessInfo();

            Settings.GuildWars2.ChromiumPriority.ValueChanged += OnSettingsPriorityChanged;
            Settings.GuildWars2.ChromiumAffinity.ValueChanged += OnSettingsChanged;
        }

        void OnSettingsChanged(object sender, EventArgs e)
        {
            refresh = true;
        }

        void OnSettingsPriorityChanged(object sender, EventArgs e)
        {
            lock (queue)
            {
                refresh = true;

                if (accounts != null && accounts.Count > 0)
                {
                    StartQueue();
                }
            }
        }

        private void StartQueue()
        {
            if (task == null || task.IsCompleted)
                task = DoProcesses();
        }

        void OnAccountPriorityChanged(object sender, EventArgs e)
        {
            lock (queue)
            {
                if (IsDisposed)
                    return;

                queue.Enqueue(new QueueItem()
                {
                    type = QueueItem.QueueType.SettingsChanged,
                    account = (Settings.IGw2Account)sender,
                });

                StartQueue();
            }
        }

        private bool IsDisposed
        {
            get
            {
                return accounts == null;
            }
        }

        public void Add(Settings.IGw2Account account, Process p, bool monitored)
        {
            lock (queue)
            {
                if (IsDisposed)
                    return;

                queue.Enqueue(new QueueItem(p)
                {
                    type = QueueItem.QueueType.Add,
                    account = account,
                    monitored = monitored,
                });

                StartQueue();
            }
        }

        public void Remove(Settings.IGw2Account account)
        {
            lock (queue)
            {
                if (IsDisposed)
                    return;

                queue.Enqueue(new QueueItem()
                {
                    type = QueueItem.QueueType.Remove,
                    account = account,
                });

                StartQueue();
            }
        }

        private async Task DoProcesses()
        {
            var scan = new Func<int>(DoScan);
            var delay = 1000;

            while (true)
            {
                //WMI could alternatively be used to poll processes, but it may not be available and has a higher impact
                //ManagementEventWatcher("SELECT * FROM __InstanceCreationEvent WITHIN 1 WHERE TargetInstance ISA 'Win32_Process' AND TargetInstance.Name = 'CoherentUI_Host.exe'")
                //Win32_ProcessStartTrace could be used, but requires admin privileges

                await Task.Delay(delay);

                if (await Task.Run(scan) == 0)
                {
                    lock (queue)
                    {
                        if (queue.Count > 0)
                        {
                            continue;
                        }
                        else if (this.processes.Count == 0)
                        {
                            task = null;
                            break;
                        }
                    }

                    foreach (var pi in this.processes.Values)
                    {
                        pi.Dispose();
                    }

                    this.processes.Clear();
                    this.unwatchable = null;
                }

                if (monitored > 0)
                {
                    delay = 5000;
                }
                else
                {
                    lock (queue)
                    {
                        if (monitored == 0 && queue.Count == 0)
                        {
                            task = null;
                            break;
                        }
                    }
                }
            }
        }

        private int DoQueue()
        {
            int removing = 0;

            while (true)
            {
                QueueItem item;

                lock (queue)
                {
                    if (queue.Count == 0)
                        return removing;
                    item = queue.Dequeue();
                }

                AccountProcessInfo ap;
                ProcessInfo pi;

                switch (item.type)
                {
                    case QueueItem.QueueType.Add:

                        int pid;

                        try
                        {
                            pid = item.process.Id;
                            if (item.process.HasExited)
                                break;
                        }
                        catch
                        {
                            break;
                        }

                        if (accounts.TryGetValue(item.account.UID, out ap) && ap.pid == pid)
                        {
                            ap.SetState(AccountProcessInfo.MonitoredState.Removing, false);
                        }
                        else
                        {
                            if (ap != null)
                            {
                                ap.SetState(AccountProcessInfo.MonitoredState.Removing | AccountProcessInfo.MonitoredState.Replaced, true);
                                ++removing;
                            }

                            ap = new AccountProcessInfo()
                            {
                                pid = pid,
                                account = item.account,
                                process = item.process,
                                parent = -1,
                            };

                            accounts[item.account.UID] = ap;
                            processes[pid] = ap;

                            item.account.ChromiumPriorityChanged += OnAccountPriorityChanged;
                            item.account.ChromiumAffinityChanged += OnSettingsChanged;

                            if (IsMonitored(item.account))
                            {
                                ap.SetState(AccountProcessInfo.MonitoredState.Monitored, true);

                                lock (queue)
                                {
                                    ++monitored;
                                }
                            }
                        }

                        if (ap.GetState(AccountProcessInfo.MonitoredState.Enabled) != item.monitored)
                        {
                            ap.SetState(AccountProcessInfo.MonitoredState.Enabled, item.monitored);

                            if (item.monitored && ap.GetState(AccountProcessInfo.MonitoredState.Monitored))
                                SetProcessOptions(ap);
                        }

                        break;
                    case QueueItem.QueueType.Remove:

                        if (accounts.TryGetValue(item.account.UID, out ap))
                        {
                            ap.SetState(AccountProcessInfo.MonitoredState.Removing, true);
                            ++removing;
                        }

                        break;
                    case QueueItem.QueueType.Removed:

                        if (processes.TryGetValue(item.pid, out pi))
                        {
                            if (pi is AccountProcessInfo)
                            {
                                if (accounts.TryGetValue(((AccountProcessInfo)pi).account.UID, out ap) && ap.pid == pi.pid)
                                {
                                    accounts.Remove(ap.account.UID);
                                }
                                else
                                {
                                    ap = (AccountProcessInfo)pi;
                                }

                                ap.account.ChromiumPriorityChanged -= OnAccountPriorityChanged;
                                ap.account.ChromiumAffinityChanged -= OnSettingsChanged;

                                if (ap.GetState(AccountProcessInfo.MonitoredState.Monitored))
                                {
                                    lock (queue)
                                    {
                                        --monitored;
                                    }
                                }

                                var exited = true;

                                try
                                {
                                    exited = ap.process.HasExited;
                                }
                                catch { }

                                //check for any remaining processes that were under this account (CefHost can fail to close on exit)

                                for (var i = 0; i < ap.processes.Length; i++)
                                {
                                    if (ap.processes[i] == null)
                                        break;

                                    if (exited)
                                    {
                                        try
                                        {
                                            if (!ap.processes[i].process.HasExited)
                                                ap.processes[i].process.Kill();
                                        }
                                        catch { }
                                    }

                                    processes.Remove(ap.processes[i].pid);
                                    ap.processes[i].Dispose();
                                }
                            }

                            processes.Remove(item.pid);
                        }

                        break;
                    case QueueItem.QueueType.Exited:

                        if (processes.TryGetValue(item.pid, out pi))
                        {
                            var owner = FindOwner(pi);

                            if (owner != null)
                            {
                                owner.Remove(pi);
                            }

                            processes.Remove(item.pid);
                            pi.Dispose();
                        }

                        if (unwatchable != null && unwatchable.Remove(item.pid) && unwatchable.Count == 0)
                            unwatchable = null;

                        break;
                    case QueueItem.QueueType.SettingsChanged:

                        if (accounts.TryGetValue(item.account.UID, out ap))
                        {
                            var b = IsMonitored(ap.account);

                            if (ap.GetState(AccountProcessInfo.MonitoredState.Monitored) != b)
                            {
                                ap.SetState(AccountProcessInfo.MonitoredState.Monitored, b);

                                lock (queue)
                                {
                                    if (b)
                                        ++monitored;
                                    else
                                        --monitored;
                                }

                                if (ap.GetState(AccountProcessInfo.MonitoredState.Enabled))
                                    SetProcessOptions(ap);
                            }
                            else if (b && ap.GetState(AccountProcessInfo.MonitoredState.Enabled))
                            {
                                SetProcessOptions(ap);
                            }
                        }

                        break;
                }
            }
        }

        private bool IsMonitored(Settings.IGw2Account a)
        {
            switch (a.ChromiumPriority)
            {
                case Settings.ProcessPriorityClass.None:
                    break;
                case Settings.ProcessPriorityClass.Normal:
                    return false;
                default:
                    return true;
            }

            switch (Settings.GuildWars2.ChromiumPriority.Value)
            {
                case Settings.ProcessPriorityClass.Normal:
                case Settings.ProcessPriorityClass.None:
                    break;
                default:
                    return true;
            }

            return false;
        }

        private void SetProcessOptions(AccountProcessInfo ap)
        {
            for (var i = 0; i < ap.processes.Length; i++)
            {
                if (ap.processes[i] == null)
                    break;
                SetProcessOptions(ap.processes[i].process, ap.account);
            }
        }

        private void DoExitWatch()
        {
            if (unwatchable == null)
                return;

            List<int> removed = null;

            foreach (var pid in unwatchable)
            {
                try
                {
                    var pi = processes[pid];
                    if (pi.process.HasExited)
                    {
                        if (removed == null)
                            removed = new List<int>();
                        removed.Add(pid);
                        processes.Remove(pid);
                        pi.process.Dispose();
                    }
                }
                catch (Exception e)
                {
                    Util.Logging.Log(e);
                }
            }

            if (removed != null)
            {
                foreach (var pid in removed)
                {
                    unwatchable.Remove(pid);
                }
                if (unwatchable.Count == 0)
                    unwatchable = null;
            }
        }

        private AccountProcessInfo FindOwner(ProcessInfo p)
        {
            while (p.parent != -1 && processes.TryGetValue(p.parent, out p)) 
            { 
            
            }

            if (p is AccountProcessInfo)
            {
                return (AccountProcessInfo)p;
            }

            return null;
        }

        private int DoScan()
        {
            var removed = DoQueue();
            DoExitWatch();

            int count;

            if (accounts != null)
            {
                count = accounts.Count;

                if (count == 0)
                    return count;
            }
            else
            {
                return 0;
            }

            if (refresh)
            {
                refresh = false;

                foreach (var ap in accounts.Values)
                {
                    var b = IsMonitored(ap.account);

                    if (ap.GetState(AccountProcessInfo.MonitoredState.Monitored) != b)
                    {
                        ap.SetState(AccountProcessInfo.MonitoredState.Monitored, b);

                        lock (queue)
                        {
                            if (b)
                                ++monitored;
                            else
                                --monitored;
                        }

                        if (ap.GetState(AccountProcessInfo.MonitoredState.Enabled))
                            SetProcessOptions(ap);
                    }
                    else if (b && ap.GetState(AccountProcessInfo.MonitoredState.Enabled))
                    {
                        SetProcessOptions(ap);
                    }
                }

            }

            if (monitored == 0 && removed == 0)
                return count;

            List<ProcessInfo> added = null;

            foreach (var p in Process.GetProcesses())
            {
                bool used = false;
                try
                {
                    if (processes.ContainsKey(p.Id))
                        continue;

                    var n = p.ProcessName;
                    int parent;

                    //CefHost
                    //CoherentUI_Host
                    if (n.Length > 3 && n[0] == 'C' && (n[2] == 'h' || n[2] == 'f'))
                    {
                        if (processInfo.Open(p.Id))
                        {
                            parent = (int)processInfo.GetParent();

                            if (!processes.ContainsKey(parent))
                                continue;
                        }
                        else
                            continue;
                    }
                    else
                        continue;

                    if (added == null)
                        added = new List<ProcessInfo>();

                    used = true;

                    ProcessInfo pi;

                    processes[p.Id] = pi = new ProcessInfo()
                    {
                        pid = p.Id,
                        process = p,
                        parent = parent,
                    };

                    //if (processInfo.Open(p.Id))
                    //    pi.parent = (int)processInfo.GetParent();

                    added.Add(pi);
                }
                finally
                {
                    if (!used)
                        p.Dispose();
                }
            }

            if (added != null)
            {
                var handled = new HashSet<int>();

                foreach (var pi in added)
                {
                    Settings.IGw2Account account;
                    var owner = FindOwner(pi);

                    if (owner != null)
                    {
                        account = owner.account;

                        if (handled.Add(owner.pid))
                        {
                            //priority will be reset on the main CEF processes when starting up another renderer

                            if (owner.GetState(AccountProcessInfo.MonitoredState.Monitoring))
                            {
                                for (var i = 0; i < owner.processes.Length; i++)
                                {
                                    if (owner.processes[i] == null)
                                        break;
                                    SetProcessOptions(owner.processes[i].process, owner.account);
                                }
                            }
                        }

                        owner.Add(pi);
                    }
                    else
                    {
                        account = null;
                    }

                    try
                    {
                        pi.process.EnableRaisingEvents = true;
                        pi.process.Exited += p_Exited;
                    }
                    catch
                    {
                        bool supportsEvents = false;
                        string username;

                        if (account != null)
                        {
                            username = account.WindowsAccount;
                        }
                        else
                        {
                            if (processInfo.Open(pi.pid))
                                username = processInfo.GetUsername();
                            else
                                username = null;
                        }

                        if (!Util.Users.IsCurrentUser(username))
                        {
                            try
                            {
                                using (var impersonation = Security.Impersonation.Impersonate(username, Security.Credentials.GetPassword(username)))
                                {
                                    pi.process.EnableRaisingEvents = true;
                                    pi.process.Exited += p_Exited;

                                    supportsEvents = true;
                                }
                            }
                            catch { }
                        }

                        if (!supportsEvents)
                        {
                            if (unwatchable == null)
                                unwatchable = new HashSet<int>();
                            unwatchable.Add(pi.pid);
                        }
                    }

                    if (account != null)
                    {
                        if (owner.GetState(AccountProcessInfo.MonitoredState.Monitoring))
                        {
                            SetProcessOptions(pi.process, account);
                        }
                    }

                    try
                    {
                        if (pi.process.HasExited)
                        {
                            lock (queue)
                            {
                                queue.Enqueue(new QueueItem(pi.process)
                                {
                                    type = QueueItem.QueueType.Exited,
                                });
                            }
                        }
                    }
                    catch { }
                }

                processInfo.Close();
            }

            if (removed > 0)
            {
                var removing = new List<AccountProcessInfo>(removed);

                foreach (var pi in processes.Values)
                {
                    if (pi is AccountProcessInfo)
                    {
                        var ap = (AccountProcessInfo)pi;
                        if (ap.GetState(AccountProcessInfo.MonitoredState.Removing))
                        {
                            removing.Add(ap);
                            if (--removed == 0)
                                break;
                        }
                    }
                }

                foreach (var ap in removing)
                {
                    if (!ap.GetState(AccountProcessInfo.MonitoredState.Replaced))
                        accounts.Remove(ap.account.UID);

                    processes.Remove(ap.pid);

                    ap.account.ChromiumPriorityChanged -= OnAccountPriorityChanged;
                    ap.account.ChromiumAffinityChanged -= OnSettingsChanged;

                    if (ap.GetState(AccountProcessInfo.MonitoredState.Monitored))
                    {
                        lock (queue)
                        {
                            --monitored;
                        }
                    }

                    var exited = true;

                    try
                    {
                        exited = ap.process.HasExited;
                    }
                    catch { }

                    //check for any remaining processes that were under this account (CefHost can fail to close on exit)

                    for (var i = 0; i < ap.processes.Length; i++)
                    {
                        if (ap.processes[i] == null)
                            break;

                        if (exited)
                        {
                            try
                            {
                                if (!ap.processes[i].process.HasExited)
                                {
                                    if (Util.Logging.Enabled)
                                    {
                                        var args = "";
                                        var name = "";

                                        try
                                        {
                                            name = ap.processes[i].process.ProcessName;

                                            if (processInfo.Open(ap.processes[i].pid))
                                            {
                                                args = processInfo.GetCommandLineArgs();
                                            }
                                        }
                                        catch { }

                                        processInfo.Close();

                                        Util.Logging.LogEvent(ap.account, "Process " + ap.processes[i].pid + " is still active after exit [" + name + "] [" + args + "]");
                                    }

                                    ap.processes[i].process.Kill();
                                }
                            }
                            catch { }
                        }

                        processes.Remove(ap.processes[i].pid);
                        ap.processes[i].Dispose();
                    }

                    ap.Dispose();
                }


                //lock (queue)
                //{
                //    foreach (var pi in processes.Values)
                //    {
                //        if (pi is AccountProcessInfo)
                //        {
                //            if (((AccountProcessInfo)pi).removing)
                //            {
                //                queue.Enqueue(new QueueItem()
                //                {
                //                    type = QueueItem.QueueType.Removed,
                //                    pid = pi.pid,
                //                });
                //            }
                //        }
                //    }
                //}
            }

            return count;
        }

        void p_Exited(object sender, EventArgs e)
        {
            var p = (Process)sender;
            int pid;

            try
            {
                pid = p.Id;
            }
            catch 
            {
                pid = 0;
            }

            lock (queue)
            {
                queue.Enqueue(new QueueItem(p)
                {
                    type = QueueItem.QueueType.Exited,
                });

                StartQueue();
            }
        }

        private void SetProcessOptions(Process p, Settings.IGw2Account account)
        {
            var gw2 = (Settings.IGw2Account)account;
            var priority = gw2.ChromiumPriority;
            var affinity = gw2.ChromiumAffinity;

            if (priority == Settings.ProcessPriorityClass.None)
            {
                if (Settings.GuildWars2.ChromiumPriority.HasValue)
                    priority = Settings.GuildWars2.ChromiumPriority.Value;
                else
                    priority = Settings.ProcessPriorityClass.Normal;
            }

            if (affinity == 0)
            {
                affinity = Settings.GuildWars2.ChromiumAffinity.Value;
            }

            try
            {
                p.PriorityClass = priority.ToProcessPriorityClass();
            }
            catch (Exception e)
            {
                Util.Logging.Log(e);
            }

            if (affinity > 0)
            {
                try
                {
                    var processors = Environment.ProcessorCount;
                    p.ProcessorAffinity = (IntPtr)(affinity & (long)(processors >= 64 ? ulong.MaxValue : ((ulong)1 << processors) - 1));
                }
                catch (Exception ex)
                {
                    Util.Logging.Log(ex);
                }
            }
        }

        public void Dispose()
        {
            Settings.GuildWars2.ChromiumPriority.ValueChanged -= OnSettingsChanged;
            Settings.GuildWars2.ChromiumAffinity.ValueChanged -= OnSettingsChanged;

            lock (queue)
            {
                if (this.accounts != null)
                {
                    foreach (var ap in this.accounts.Values)
                    {
                        ap.account.ChromiumPriorityChanged -= OnSettingsChanged;
                        ap.account.ChromiumAffinityChanged -= OnSettingsChanged;
                    }

                    this.accounts = null;
                }

                this.queue.Clear();
            }

            var processes = this.processes;
            this.processes = null;

            if (task != null)
            {
                try
                {
                    task.Wait(1000);
                }
                catch { }
            }

            foreach (var pi in processes.Values)
            {
                pi.Dispose();
            }

            unwatchable = null;

            if (processInfo != null)
                processInfo.Dispose();

            if (task != null && task.IsCompleted)
                task.Dispose();
        }
    }
}
