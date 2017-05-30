using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Threading;
using System.Windows.Forms;
using System.Drawing;
using System.Diagnostics;
using System.IO;
using System.IO.MemoryMappedFiles;
using Gw2Launcher.Windows;
using System.Runtime.InteropServices;

namespace Gw2Launcher
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static int Main(string[] args)
        {
            #region ProcessUtil

            if (args.Length > 1 && args[0] == "-pu")
            {
                if (args[1] == "-handle")
                {
                    #region -handle [-pid|-p|-n] [processId|"path to exe"|"process name"] "objectName" [exactMatch (0|1)]

                    if (args.Length != 6 || args[2] != "-pid" && args[2] != "-p" && args[2] != "-n")
                        return new ArgumentException().HResult;

                    bool isId = args[2] == "-pid";
                    bool isName = args[2] == "-n";
                    string path = args[3];
                    string name = args[4];
                    bool exactMatch = args[5] == "1";

                    Process gw2 = null;

                    if (!isId)
                    {
                        FileInfo fi;
                        if (isName)
                            fi = null;
                        else
                        {
                            try
                            {
                                fi = new FileInfo(path);
                                if (!fi.Exists)
                                    return new FileNotFoundException().HResult;
                            }
                            catch (Exception e)
                            {
                                return e.HResult;
                            }
                        }

                        Process[] ps = Process.GetProcessesByName(isName ? path : fi.Name.Substring(0, fi.Name.Length - fi.Extension.Length));
                        DateTime newest = DateTime.MinValue;

                        foreach (Process p in ps)
                        {
                            try
                            {
                                if (!p.HasExited && (isName || string.Equals(p.MainModule.FileName, fi.FullName, StringComparison.OrdinalIgnoreCase)))
                                {
                                    if (p.StartTime > newest)
                                        gw2 = p;
                                }
                            }
                            catch (Exception e)
                            {
                                Console.WriteLine(e.Message);
                            }
                        }
                    }
                    else
                    {
                        try
                        {
                            gw2 = Process.GetProcessById(Int32.Parse(path));
                        }
                        catch (Exception e)
                        {
                            Console.WriteLine(e.Message);
                        }
                    }

                    if (gw2 != null)
                    {
                        try
                        {
                            Win32Handles.IObjectHandle handle = Win32Handles.GetHandle(gw2.Id, name, exactMatch);
                            if (handle != null)
                            {
                                handle.Kill();
                                return 0;
                            }
                            return new FileNotFoundException().HResult;
                        }
                        catch (Exception e)
                        {
                            return e.HResult;
                        }
                    }
                    else
                    {
                        return new FileNotFoundException().HResult;
                    }

                    #endregion
                }
                else if (args[1] == "-user")
                {
                    #region -user -u "username" -p "password"

                    if (args.Length != 6 || args[2] != "-u" || args[4] != "-p")
                        return new ArgumentException().HResult;

                    string username = args[3];
                    string password = args[5];

                    try
                    {
                        ProcessStartInfo p = new ProcessStartInfo("net");
                        //p.RedirectStandardInput = true;
                        //p.RedirectStandardOutput = true;
                        p.UseShellExecute = false;
                        p.CreateNoWindow = true;

                        p.Arguments = "user /add \"" + username + "\" \"" + password + "\"";

                        int exitCode;

                        using (Process cmd = Process.Start(p))
                        {
                            //StreamReader reader = cmd.StandardOutput;

                            //while (!reader.EndOfStream)
                            //{
                            //    string line = reader.ReadLine().ToLower();
                            //}

                            cmd.WaitForExit();

                            exitCode = cmd.ExitCode;
                        }

                        //p.Arguments = "localgroup Administrators /add \"" + username + "\"";

                        //using (Process cmd = Process.Start(p))
                        //{
                        //    StreamReader reader = cmd.StandardOutput;

                        //    while (!reader.EndOfStream)
                        //    {
                        //        string line = reader.ReadLine().ToLower();
                        //    }
                        //}

                        p = new ProcessStartInfo("wmic");
                        //p.RedirectStandardInput = true;
                        //p.RedirectStandardOutput = true;
                        p.UseShellExecute = false;
                        p.CreateNoWindow = true;

                        p.Arguments = "USERACCOUNT WHERE Name='" + username + "' SET PasswordExpires=FALSE";

                        using (Process cmd = Process.Start(p))
                        {
                            //StreamReader reader = cmd.StandardOutput;

                            //while (!reader.EndOfStream)
                            //{
                            //    string line = reader.ReadLine().ToLower();
                            //}

                            cmd.WaitForExit();
                        }

                        return exitCode;
                    }
                    catch (Exception e)
                    {
                        return e.HResult;
                    }

                    #endregion
                }
                else if (args[1] == "-delgw2cache")
                {
                    #region -delgw2cache "path1" "path2" "..."

                    for (int i = 2; i < args.Length; i++)
                    {
                        if (Directory.Exists(args[i]))
                        {
                            try
                            {
                                foreach (var d in new DirectoryInfo(args[i]).GetDirectories("gw2cache-{*}", SearchOption.TopDirectoryOnly))
                                {
                                    try
                                    {
                                        d.Delete(true);
                                    }
                                    catch { }
                                }
                            }
                            catch { }
                        }
                    }

                    #endregion
                }
                else if (args[1] == "-task")
                {
                    #region -task [-create "name1" "xml1" "name2" "xml2" "..."]|[-run "name"]|[-delete "name1" "name2" "..."]

                    if (args.Length < 4)
                        return new ArgumentException().HResult;

                    if (args[2] == "-create")
                    {
                        if (args.Length < 5)
                            return new ArgumentException().HResult;

                        int exitCode = 0;

                        for (int i = 3; i < args.Length; i += 2)
                        {
                            try
                            {
                                ProcessStartInfo p = new ProcessStartInfo("schtasks");
                                p.UseShellExecute = false;
                                p.CreateNoWindow = true;

                                p.Arguments = "/delete /F /tn \"" + args[i] + "\"";

                                using (Process cmd = Process.Start(p))
                                {
                                    cmd.WaitForExit();
                                }
                            }
                            catch { }

                            try
                            {
                                ProcessStartInfo p = new ProcessStartInfo("schtasks");
                                p.UseShellExecute = false;
                                p.CreateNoWindow = true;

                                p.Arguments = "/create /tn \"" + args[i] + "\" /xml \"" + args[i + 1] + "\"";

                                using (Process cmd = Process.Start(p))
                                {
                                    cmd.WaitForExit();
                                    exitCode = cmd.ExitCode;
                                }
                            }
                            catch (Exception e)
                            {
                                return e.HResult;
                            }
                        }
                        return exitCode;
                    }
                    else if (args[2] == "-delete")
                    {
                        if (args.Length < 4)
                            return new ArgumentException().HResult;

                        int exitCode = 0;

                        for (int i = 3; i < args.Length; i++)
                        {
                            try
                            {
                                ProcessStartInfo p = new ProcessStartInfo("schtasks");
                                p.UseShellExecute = false;
                                p.CreateNoWindow = true;

                                p.Arguments = "/delete /F /tn \"" + args[i] + "\"";

                                using (Process cmd = Process.Start(p))
                                {
                                    cmd.WaitForExit();
                                    exitCode = cmd.ExitCode;
                                }
                            }
                            catch { }
                        }

                        return exitCode;
                    }
                    else if (args[2] == "-run")
                    {
                        if (args.Length < 4)
                            return new ArgumentException().HResult;

                        try
                        {
                            ProcessStartInfo p = new ProcessStartInfo("schtasks");
                            p.UseShellExecute = false;
                            p.CreateNoWindow = true;

                            p.Arguments = "/run /tn \"" + args[3] + "\"";

                            using (Process cmd = Process.Start(p))
                            {
                                cmd.WaitForExit();
                                return cmd.ExitCode;
                            }
                        }
                        catch (Exception e)
                        {
                            return e.HResult;
                        }
                    }

                    #endregion
                }
                else if (args[1] == "-users")
                {
                    #region -users -activate:yes|no "name1" "name2" "..."

                    if (args.Length < 4)
                        return new ArgumentException().HResult;

                    if (args[2].StartsWith("-activate:"))
                    {
                        string _args = "user /active:" + (args[2] == "-activate:yes" ? "yes " : "no ");

                        int exitCode = 0;

                        for (int i = 3; i < args.Length; i++)
                        {
                            try
                            {
                                ProcessStartInfo p = new ProcessStartInfo("net");
                                p.UseShellExecute = false;
                                p.CreateNoWindow = true;

                                p.Arguments = _args + "\"" + args[i] + "\"";

                                using (Process cmd = Process.Start(p))
                                {
                                    cmd.WaitForExit();
                                    exitCode = cmd.ExitCode;
                                }
                            }
                            catch { }
                        }

                        return exitCode;
                    }

                    #endregion
                }

                return 0;
            }

            #endregion

            #region Task: -users:active:yes|no

            if (args.Length > 0)
            {
                if (args[0].StartsWith("-users:active:"))
                {
                    bool activate = args[0] == "-users:active:yes";

                    try
                    {
                        string[] users = Settings.HiddenUserAccounts.GetKeys();
                        if (users.Length > 0)
                            Util.ProcessUtil.ActivateUsers(users, activate);
                    }
                    catch { }

                    return 0;
                }
            }

            #endregion

            #region Allow only 1 process

            Mutex mutex = new Mutex(true, "Gw2Launcher_Mutex");
            if (!mutex.WaitOne(TimeSpan.Zero, true))
            {
                try
                {
                    using (Process current = Process.GetCurrentProcess())
                    {
                        FileInfo fi = new FileInfo(current.MainModule.FileName);
                        Process[] ps = Process.GetProcessesByName(fi.Name.Substring(0, fi.Name.Length - fi.Extension.Length));
                        foreach (Process p in ps)
                        {
                            using (p)
                            {
                                if (p.Id != current.Id && !p.HasExited)
                                {
                                    try
                                    {
                                        if (string.Equals(p.MainModule.FileName, fi.FullName, StringComparison.OrdinalIgnoreCase))
                                        {
                                            IntPtr ptr = IntPtr.Zero;
                                            try
                                            {
                                                ptr =  Windows.FindWindow.Find(p.Id, null);

                                                if (ptr != IntPtr.Zero)
                                                {
                                                    Windows.FindWindow.ShowWindow(ptr, 1);
                                                    Windows.FindWindow.SetForegroundWindow(ptr);
                                                    Windows.FindWindow.BringWindowToTop(ptr);
                                                }

                                                //var placement = Windows.WindowSize.GetWindowPlacement(ptr);

                                                //if (placement.showCmd == (int)Windows.WindowSize.WindowState.SW_SHOWMINIMIZED)
                                                //    Windows.WindowSize.SetWindowPlacement(ptr, Rectangle.FromLTRB(placement.rcNormalPosition.left, placement.rcNormalPosition.top, placement.rcNormalPosition.right, placement.rcNormalPosition.bottom), Windows.WindowSize.WindowState.SW_RESTORE);

                                                //SetForegroundWindow(ptr);
                                            }
                                            catch { }

                                            return 0;
                                        }
                                    }
                                    catch { }
                                }
                            }
                        }
                    }
                }
                catch { }

                return 0;
            }

            #endregion

            try
            {
                if (Util.Users.UserName == null)
                {
                    //init
                }

                var store = Settings.StoreCredentials;
                store.ValueChanged += StoredCredentials_ValueChanged;
                Security.Credentials.StoreCredentials = store.Value;

                Application.EnableVisualStyles();
                Application.SetCompatibleTextRenderingDefault(false);

                var f = new UI.formMain();
                var s = Settings.WindowBounds[typeof(UI.formMain)];

                if (s.HasValue && !s.Value.Size.IsEmpty)
                {
                    f.AutoSizeGrid = false;
                    f.Size = s.Value.Size;
                }
                else
                    f.AutoSizeGrid = true;

                if (s.HasValue && !s.Value.Location.Equals(new Point(int.MinValue, int.MinValue)))
                    f.Location = Util.ScreenUtil.Constrain(s.Value.Location, f.Size);
                else
                {
                    var bounds = Screen.PrimaryScreen.WorkingArea;
                    f.Location = Point.Add(bounds.Location, new Size(bounds.Width / 2 - f.Size.Width / 2, bounds.Height / 3));
                }

                f.FormClosed += FormClosed;

                Util.Users.Activate(true);

                Application.Run(f);
            }
            finally
            {
                mutex.ReleaseMutex();
            }

            return 0;
        }

        private static string GetInstanceName(int processId)
        {
            string instanceName = Process.GetProcessById(processId).ProcessName;
            bool found = false;
            if (!string.IsNullOrEmpty(instanceName))
            {
                Process[] processes = Process.GetProcessesByName(instanceName);
                if (processes.Length > 0)
                {
                    int i = 0;
                    foreach (Process p in processes)
                    {
                        instanceName = FormatInstanceName(p.ProcessName, i);
                        if (PerformanceCounterCategory.CounterExists("ID Process", "Process"))
                        {
                            PerformanceCounter counter = new PerformanceCounter("Process", "ID Process", instanceName);

                            if (processId == counter.RawValue)
                            {
                                found = true;
                                break;
                            }
                        }
                        i++;
                    }
                }
            }

            if (!found)
                instanceName = string.Empty;

            return instanceName;
        }

        private static string FormatInstanceName(string processName, int count)
        {
            string instanceName = string.Empty;
            if (count == 0)
                instanceName = processName;
            else
                instanceName = string.Format("{0}#{1}", processName, count);
            return instanceName;
        } 

        static void FormClosed(object sender, FormClosedEventArgs e)
        {
            Form f = sender as Form;
            if (f != null)
            {
                try
                {
                    f.Visible = false;
                }
                catch (Exception ex)
                {
                    Util.Logging.Log(ex);
                }
            }

            Util.Users.Activate(false);
        }

        static void StoredCredentials_ValueChanged(object sender, EventArgs e)
        {
            Security.Credentials.StoreCredentials = ((Settings.ISettingValue<bool>)sender).Value;
        }
    }
}
