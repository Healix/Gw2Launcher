using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Management;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Gw2Launcher.Client
{
    class SteamProxy
    {
        public static Process WaitForProcess(string processName, ushort uid)
        {
            using (var pi = new Windows.ProcessInfo())
            {
                while (true)
                {
                    //GetUIDFromCommandLine

                    foreach (var p in Process.GetProcessesByName(processName))
                    {
                        var used = false;

                        try
                        {
                            string commandLine = null;

                            try
                            {
                                if (pi.Open(p.Id))
                                {
                                    commandLine = pi.GetCommandLine();
                                }
                            }
                            catch { }

                            if (commandLine != null)
                            {
                                var i = commandLine.IndexOf("-l:uid:");
                                if (i != -1)
                                {
                                    i += 7;
                                    var limit = commandLine.IndexOf(' ', i);
                                    if (limit == -1)
                                        limit = commandLine.Length;
                                    var suid = commandLine.Substring(i, limit - i);
                                    ushort uid2;
                                    if (ushort.TryParse(suid, out uid2) && uid2 == uid)
                                    {
                                        used = true;
                                        return p;
                                    }
                                }
                            }
                        }
                        finally
                        {
                            if (!used)
                                p.Dispose();
                        }
                    }

                    Thread.Sleep(500);
                }
            }
        }
    }
}
