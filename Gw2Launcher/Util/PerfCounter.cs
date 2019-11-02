using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;

namespace Gw2Launcher.Util
{
    static class PerfCounter
    {
        public enum CategoryName
        {
            Process
        }

        private static string ToString(CategoryName n)
        {
            switch (n)
            {
                case CategoryName.Process:
                    return "Process";
            }

            return "";
        }

        public enum CounterName
        {
            ProcessID,
            IOWriteByesPerSecond,
            IOReadBytesPerSecond,
        }

        private static string ToString(CounterName n)
        {
            switch (n)
            {
                case CounterName.ProcessID:
                    return "ID Process";
                case CounterName.IOWriteByesPerSecond:
                    return "IO Write Bytes/sec";
                case CounterName.IOReadBytesPerSecond:
                    return "IO Read Bytes/sec";
            }

            return "";
        }

        public static string GetInstanceName(int pid)
        {
            using (var p = Process.GetProcessById(pid))
            {
                return GetInstanceName(p);
            }
        }

        public static string GetInstanceName(Process process)
        {
            var counterName = ToString(CounterName.ProcessID);
            var categoryName = ToString(CategoryName.Process);

            if (PerformanceCounterCategory.CounterExists(counterName, categoryName))
            {
                Process[] processes = Process.GetProcessesByName(process.ProcessName);
                if (processes.Length > 0)
                {
                    for (var i = processes.Length - 1; i >= 0; i--)
                    {
                        var instanceName = i == 0 ? process.ProcessName : process.ProcessName + "#" + i;

                        try
                        {
                            using (var counter = new PerformanceCounter(categoryName, counterName, instanceName))
                            {
                                if (process.Id == counter.RawValue)
                                {
                                    return instanceName;
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            Util.Logging.Log(ex);
                        }
                    }
                }
            }

            return null;
        }

        public static PerformanceCounter GetCounter(CategoryName category, CounterName counter, string name)
        {
            return GetCounter(ToString(category), ToString(counter), name);
        }

        public static PerformanceCounter GetCounter(string category, string counter, string name)
        {
            if (PerformanceCounterCategory.Exists(category) && PerformanceCounterCategory.CounterExists(counter, category))
            {
                return new PerformanceCounter(category, counter, name, true);
            }

            return null;
        }
    }
}
