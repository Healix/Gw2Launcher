using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Diagnostics;

namespace Gw2Launcher.Tools
{
    class ProcessPriority : IDisposable
    {
        private class ProcessInfo
        {
            public int pid;
            public Process process;
            public ProcessPriorityClass priority;
        }

        private Task task;
        private Dictionary<int, ProcessInfo> processes;
        private ProcessInfo[] infos;

        public ProcessPriority()
        {
            processes = new Dictionary<int, ProcessInfo>();
        }

        public void SetPriority(Process p, ProcessPriorityClass priority)
        {
            lock (this)
            {
                var pid = p.Id;

                ProcessInfo pi;
                if (processes.TryGetValue(pid, out pi))
                {
                    pi.priority = priority;
                    if (priority == ProcessPriorityClass.Normal)
                    {
                        processes.Remove(pid);
                        infos = null;
                    }
                }
                else if (priority != ProcessPriorityClass.Normal)
                {
                    pi = new ProcessInfo()
                    {
                        pid = pid,
                        priority = priority,
                        process = p,
                    };
                    processes[pid] = pi;
                    infos = null;
                }
                else
                    return;

                if (task == null || task.IsCompleted)
                    task = DoProcesses();
            }
        }

        private async Task DoProcesses()
        {
            var scan = new Func<int>(delegate
                {
                    int count = 0;
                    ProcessInfo[] infos;
                    lock (this)
                    {
                        if (this.infos == null)
                        {
                            this.infos = new ProcessInfo[processes.Count];
                            processes.Values.CopyTo(this.infos, 0);
                        }
                        infos = this.infos;
                    }

                    foreach (var info in infos)
                    {
                        try
                        {
                            info.process.Refresh();
                            if (info.process.PriorityClass != info.priority)
                                info.process.PriorityClass = info.priority;
                            count++;
                        }
                        catch
                        {
                            lock (this)
                            {
                                processes.Remove(info.pid);
                                this.infos = null;
                            }
                        }
                    }

                    return count;
                });

            do
            {
                await Task.Delay(5000);

                if (await Task.Run(scan) == 0)
                {
                    lock (this)
                    {
                        if (this.processes.Count == 0)
                        {
                            task = null;
                            return;
                        }
                    }
                }
            }
            while (true);
        }

        public void Dispose()
        {
            lock (this)
            {
                processes.Clear();
                infos = null;
            }

            if (task != null && task.IsCompleted)
                task.Dispose();
        }
    }
}
