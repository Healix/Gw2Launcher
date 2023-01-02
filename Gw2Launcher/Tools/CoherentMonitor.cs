using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Diagnostics;

namespace Gw2Launcher.Tools
{
    class CoherentMonitor : IDisposable
    {
        private class AccountProcessInfo : ProcessInfo
        {
            public Settings.IAccount account;
        }

        private class ProcessInfo
        {
            public int pid;
            public int parent;
            public Process process;
        }

        private class QueueItem
        {
            public enum QueueType
            {
                Add,
                Remove,
                Exited
            }

            public QueueType type;
            public Process process;
            public Settings.IAccount account;
        }

        private Task task;

        private Queue<QueueItem> queue;
        private Dictionary<ushort, AccountProcessInfo> accounts;
        private Dictionary<int, ProcessInfo> processes;
        private HashSet<int> unwatchable;
        private Windows.ProcessInfo processInfo;

        public CoherentMonitor()
        {
            queue = new Queue<QueueItem>();
            accounts = new Dictionary<ushort, AccountProcessInfo>();
            processes = new Dictionary<int, ProcessInfo>();
            processInfo = new Windows.ProcessInfo();
        }

        public void Add(Settings.IAccount account, Process p)
        {
            lock (queue)
            {
                queue.Enqueue(new QueueItem()
                    {
                        type = QueueItem.QueueType.Add,
                        process = p,
                        account = account,
                    });

                if (task == null || task.IsCompleted)
                    task = DoProcesses();
            }
        }

        public void Remove(Settings.IAccount account)
        {
            lock (queue)
            {
                queue.Enqueue(new QueueItem()
                    {
                        type = QueueItem.QueueType.Remove,
                        account = account,
                    });

                if (task == null || task.IsCompleted)
                    task = DoProcesses();
            }
        }

        private async Task DoProcesses()
        {
            var scan = new Func<int>(DoScan);

            do
            {
                //WMI could alternatively be used to poll processes, but it may not be available and has a higher impact
                //ManagementEventWatcher("SELECT * FROM __InstanceCreationEvent WITHIN 1 WHERE TargetInstance ISA 'Win32_Process' AND TargetInstance.Name = 'CoherentUI_Host.exe'")
                //Win32_ProcessStartTrace could be used, but requires admin privileges

                await Task.Delay(5000);

                if (await Task.Run(scan) == 0)
                {
                    lock (queue)
                    {
                        if (queue.Count == 0)
                        {
                            if (this.processes.Count > 0)
                            {
                                foreach (var pi in this.processes.Values)
                                {
                                    if (pi.parent != -1)
                                        pi.process.Dispose();
                                }

                                this.processes.Clear();
                                this.unwatchable = null;
                            }

                            task = null;
                            return;
                        }
                    }
                }
            }
            while (true);
        }

        private void DoQueue()
        {
            lock (queue)
            {
                while (queue.Count > 0)
                {
                    var item = queue.Dequeue();

                    AccountProcessInfo ap;
                    int pid;

                    switch (item.type)
                    {
                        case QueueItem.QueueType.Add:

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

                            if (accounts.TryGetValue(item.account.UID, out ap))
                            {
                                if (ap.pid != pid)
                                {
                                    processes.Remove(ap.pid);

                                    ap.process = item.process;
                                    ap.pid = pid;
                                    processes[pid] = ap;
                                }
                            }
                            else
                            {
                                ap = new AccountProcessInfo()
                                {
                                    pid = pid,
                                    account = item.account,
                                    process = item.process,
                                    parent = -1,
                                };
                                accounts[item.account.UID] = ap;
                                processes[pid] = ap;
                            }

                            break;
                        case QueueItem.QueueType.Remove:

                            if (accounts.TryGetValue(item.account.UID, out ap))
                            {
                                accounts.Remove(item.account.UID);
                                processes.Remove(ap.pid);
                            }

                            break;
                        case QueueItem.QueueType.Exited:

                            try
                            {
                                pid = item.process.Id;
                            }
                            catch
                            {
                                pid = -1;
                                foreach (var pi in processes.Values)
                                {
                                    if (pi.process == item.process)
                                    {
                                        pid = pi.pid;
                                        break;
                                    }
                                }
                                if (pid == -1)
                                    break;
                            }

                            processes.Remove(pid);
                            if (unwatchable != null && unwatchable.Remove(pid) && unwatchable.Count == 0)
                                unwatchable = null;

                            break;
                    }
                }
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

        private int DoScan()
        {
            DoQueue();
            DoExitWatch();

            var count = accounts.Count;
            if (count == 0)
                return count;

            List<ProcessInfo> added = null;

            foreach (var p in Process.GetProcesses())
            {
                bool used = false;
                try
                {
                    if (!p.ProcessName.Equals("CoherentUI_Host", StringComparison.OrdinalIgnoreCase) || processes.ContainsKey(p.Id))
                        continue;

                    if (added == null)
                        added = new List<ProcessInfo>();

                    used = true;

                    ProcessInfo pi;

                    processes[p.Id] = pi = new ProcessInfo()
                    {
                        pid = p.Id,
                        process = p,
                    };

                    if (processInfo.Open(p.Id))
                        pi.parent = (int)processInfo.GetParent();

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
                foreach (var pi in added)
                {
                    var pid = pi.parent;
                    ProcessInfo owner = null;

                    while (processes.TryGetValue(pid, out owner))
                    {
                        if (owner.parent == -1)
                            break;
                        pid = owner.parent;
                    }

                    Settings.IAccount account;

                    if (owner != null && owner is AccountProcessInfo)
                    {
                        account = ((AccountProcessInfo)owner).account;
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
                                var impersonation = Security.Impersonation.Impersonate(username, Security.Credentials.GetPassword(username));
                                try
                                {
                                    pi.process.EnableRaisingEvents = true;
                                    pi.process.Exited += p_Exited;

                                    supportsEvents = true;
                                }
                                finally
                                {
                                    impersonation.Dispose();
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
                        OnLinkedProcessAdded(account, pi);

                    try
                    {
                        if (pi.process.HasExited)
                        {
                            lock (queue)
                            {
                                queue.Enqueue(new QueueItem()
                                    {
                                        type = QueueItem.QueueType.Exited,
                                        process = pi.process,
                                    });
                            }
                        }
                    }
                    catch { }
                }

                processInfo.Close();
            }

            return count;
        }

        void p_Exited(object sender, EventArgs e)
        {
            lock(queue)
            {
                queue.Enqueue(new QueueItem()
                    {
                        type = QueueItem.QueueType.Exited,
                        process = (Process)sender,
                    });
            }
        }

        private void OnLinkedProcessAdded(Settings.IAccount account, ProcessInfo pi)
        {
            try
            {
                pi.process.PriorityClass = ProcessPriorityClass.High;
            }
            catch (Exception e)
            {
                Util.Logging.Log(e);
            }
        }

        public void Dispose()
        {
            lock (queue)
            {
                this.accounts = null;
                this.queue = null;
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
                if (pi.pid != -1)
                    pi.process.Dispose();
            }

            unwatchable = null;

            if (processInfo != null)
                processInfo.Dispose();

            if (task != null && task.IsCompleted)
                task.Dispose();
        }
    }
}
