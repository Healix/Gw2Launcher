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
        public const byte RELEASE_VERSION = 3;

        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static int Main(string[] args)
        {
            #region Updated: -updated [pid] ["oldfilename"]

            if (args.Length > 2 && args[0] == "-updated")
            {
                int pid;
                if (Int32.TryParse(args[1], out pid))
                {
                    string filename = args[2];

                    try
                    {
                        using (var p = Process.GetProcessById(pid))
                        {
                            p.WaitForExit();
                        }
                    }
                    catch
                    {
                    }

                    try
                    {
                        if (File.Exists(filename))
                            File.Delete(filename);
                    }
                    catch { }
                }
            }

            #endregion

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
                        Settings.Load();
                        string[] users = Settings.HiddenUserAccounts.GetKeys();
                        if (users.Length > 0)
                            Util.ProcessUtil.ActivateUsers(users, activate);
                    }
                    catch { }

                    return 0;
                }
            }

            #endregion

            #region Launch from args

            Messaging.LaunchMessage launch = null;
            if (args.Length > 0)
            {
                bool hasArgs = false;

                foreach (var arg in args)
                {
                    if (arg == "-l:silent")
                        Settings.Silent = true;
                    else if (arg.StartsWith("-l:uid:"))
                    {
                        ushort uid;
                        if (UInt16.TryParse(arg.Substring(7), out uid))
                        {
                            if (launch == null)
                                launch = new Messaging.LaunchMessage(1);
                            launch.accounts.Add(uid);
                        }
                    }
                    else
                        hasArgs = true;
                }

                if (launch != null)
                {
                    if (hasArgs)
                    {
                        var _args = Environment.CommandLine;
                        foreach (var a in args)
                            _args += a + " ";
                        int i;
                        if (_args[0] == '"')
                            i = _args.IndexOf('"', 1);
                        else
                            i = _args.IndexOf(' ');
                        if (i != -1 && i + 1 < _args.Length)
                        {
                            if (_args[i + 1] == ' ')
                                i++;

                            _args = _args.Substring(i + 1);

                            var sb = new System.Text.StringBuilder(_args.Length);
                            var last = 0;
                            i = -1;

                            do
                            {
                                i = _args.IndexOf("-l:", i + 1);
                                if (i == -1)
                                {
                                    if ((sb.Length == 0 || sb[sb.Length - 1] == ' ') && _args[last] == ' ')
                                        last++;
                                    sb.Append(_args.Substring(last, _args.Length - last));
                                    break;
                                }
                                var j = _args.IndexOf(' ', i);
                                if (j == -1)
                                    j = _args.Length;

                                if (i - last > 1)
                                {
                                    if ((sb.Length == 0 || sb[sb.Length - 1] == ' ') && _args[last] == ' ')
                                        last++;
                                    sb.Append(_args.Substring(last, i - last));
                                }
                                last = i = j;
                            }
                            while (i + 1 < _args.Length);

                            if (sb.Length > 0 && sb[sb.Length - 1] == ' ')
                                sb.Length--;
                            launch.args = sb.ToString();
                        }
                    }
                }
            }

            #endregion

            #region Allow only 1 process

            Mutex mutex = new Mutex(true, "Gw2Launcher_Mutex");
            if (!mutex.WaitOne(TimeSpan.Zero, true))
            {
                if (!Settings.Silent)
                {
                    Messaging.Messager.SendCallback(Messaging.Messager.MessageType.Show, 0, 1000,
                        delegate(IntPtr hWnd, uint uMsg, IntPtr lResult)
                        {
                            if (lResult != IntPtr.Zero && lResult == hWnd)
                            {
                                Windows.FindWindow.ShowWindow(hWnd, 5);
                                Windows.FindWindow.BringWindowToTop(hWnd);
                                Windows.FindWindow.SetForegroundWindow(hWnd);

                                return true;
                            }
                            return false;
                        });

                    #region By process

                    //required for some situations

                    //try
                    //{
                    //    using (Process current = Process.GetCurrentProcess())
                    //    {
                    //        FileInfo fi = new FileInfo(current.MainModule.FileName);
                    //        Process[] ps = Process.GetProcessesByName(fi.Name.Substring(0, fi.Name.Length - fi.Extension.Length));
                    //        foreach (Process p in ps)
                    //        {
                    //            using (p)
                    //            {
                    //                try
                    //                {
                    //                    if (p.Id != current.Id && !p.HasExited)
                    //                    {
                    //                        if (string.Equals(p.MainModule.FileName, fi.FullName, StringComparison.OrdinalIgnoreCase))
                    //                        {
                    //                            IntPtr ptr = Windows.FindWindow.Find(p.Id, null);

                    //                            if (ptr != IntPtr.Zero)
                    //                            {
                    //                                Windows.FindWindow.ShowWindow(ptr, 5);
                    //                                Windows.FindWindow.BringWindowToTop(ptr);
                    //                                Windows.FindWindow.SetForegroundWindow(ptr);
                    //                            }

                    //                            //var placement = Windows.WindowSize.GetWindowPlacement(ptr);

                    //                            //if (placement.showCmd == (int)Windows.WindowSize.WindowState.SW_SHOWMINIMIZED)
                    //                            //    Windows.WindowSize.SetWindowPlacement(ptr, Rectangle.FromLTRB(placement.rcNormalPosition.left, placement.rcNormalPosition.top, placement.rcNormalPosition.right, placement.rcNormalPosition.bottom), Windows.WindowSize.WindowState.SW_RESTORE);

                    //                            //SetForegroundWindow(ptr);

                    //                            break;
                    //                        }
                    //                    }
                    //                }
                    //                catch { }
                    //            }
                    //        }
                    //    }
                    //}
                    //catch { }

                    #endregion
                }

                if (launch != null)
                    launch.Send();

                mutex.Dispose();
                return 0;
            }

            #endregion

#if DEBUG
            if (!Debugger.IsAttached)
                Debugger.Launch();
#else

            System.Windows.Forms.Application.SetUnhandledExceptionMode(System.Windows.Forms.UnhandledExceptionMode.ThrowException);
            AppDomain.CurrentDomain.UnhandledException += new UnhandledExceptionEventHandler(CurrentDomain_UnhandledException);
            System.Windows.Forms.Application.ThreadException += new System.Threading.ThreadExceptionEventHandler(Application_ThreadException);

#endif

            try
            {
                if (Util.Users.UserName == null)
                {
                    //init
                }

                Settings.Load();

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

                Util.Users.Activate(true);

                if (Settings.Silent)
                {
                    f.WindowState = FormWindowState.Minimized;
                    if (Settings.ShowTray.Value && Settings.MinimizeToTray.Value)
                        f.DisableNextVisibilityChange = true;
                }

                if (launch != null)
                {
                    launch.Send();
                    launch = null;
                }

                Application.Run(f);

                OnExit();
            }
            finally
            {
                mutex.ReleaseMutex();
                mutex.Dispose();
            }

            return 0;
        }

        private static void OnExit()
        {
            try
            {
                //if any recorded accounts are still active, add a record for exiting the program (ID 0)
                var active = Client.Launcher.GetActiveProcesses();
                foreach (var account in active)
                {
                    if (account.RecordLaunches)
                    {
                        Tools.Statistics.Record(Tools.Statistics.RecordType.Exited, 0);
                        break;
                    }
                }
            }
            catch (Exception ex)
            {
                Util.Logging.Log(ex);
            }

            Settings.Save();
            Tools.Statistics.Save();

            Util.Users.Activate(false);
        }

        static void StoredCredentials_ValueChanged(object sender, EventArgs e)
        {
            Security.Credentials.StoreCredentials = ((Settings.ISettingValue<bool>)sender).Value;
        }

        static void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            var ex = e.ExceptionObject as Exception;

            if (ex != null)
                Util.Logging.Crash(ex);
            else if (e.ExceptionObject != null)
                Util.Logging.Crash(e.ExceptionObject.ToString());

            System.Environment.Exit(-1);
        }

        static void Application_ThreadException(object sender, System.Threading.ThreadExceptionEventArgs e)
        {
            Util.Logging.Crash(e.Exception);

            System.Environment.Exit(-1);
        }

        private class PassedLaunch
        {
            public PassedLaunch()
            {
                accounts = new List<ushort>();
            }

            public List<ushort> accounts;
            public string args;

            public IntPtr ToPointer()
            {
                byte[] _args;
                if (!string.IsNullOrEmpty(args))
                    _args = System.Text.Encoding.UTF8.GetBytes(args);
                else
                    _args = new byte[0];

                byte count;
                if (accounts.Count > byte.MaxValue)
                    count = byte.MaxValue;
                else
                    count = (byte)accounts.Count;

                var ptr = Marshal.AllocHGlobal(3 + count * 2 + _args.Length);

                Marshal.WriteByte(ptr, count);

                var i = 1;
                for (var j = 0; j < count; j++, i += 2)
                    Marshal.WriteInt16(ptr, i, (short)accounts[j]);

                Marshal.WriteInt16(ptr, i, (short)_args.Length);
                i += 2;

                Marshal.Copy(_args, 0, (IntPtr)(ptr + i), _args.Length);

                return ptr;
            }

            public static PassedLaunch FromPointer(IntPtr ptr, bool free)
            {
                PassedLaunch l = new PassedLaunch();

                var count = Marshal.ReadByte(ptr);
                
                var i = 1;
                for (var j = 0; j < count; j++, i += 2)
                    l.accounts.Add((ushort)Marshal.ReadInt16(ptr, i));

                byte[] args = new byte[(ushort)Marshal.ReadInt16(ptr, i)];
                i += 2;

                if (args.Length > 0)
                {
                    Marshal.Copy((IntPtr)(ptr + i), args, 0, args.Length);
                    l.args = System.Text.Encoding.UTF8.GetString(args);
                }

                if (free)
                    Marshal.FreeHGlobal(ptr);

                return l;
            }
        }
    }
}
