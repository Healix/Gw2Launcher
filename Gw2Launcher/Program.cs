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
using System.Runtime.InteropServices;
using Gw2Launcher.Windows;
using Gw2Launcher.Windows.Native;

namespace Gw2Launcher
{
    static class Program
    {
        public const byte RELEASE_VERSION = 16;
        public const uint BUILD = 6511;
        public const long RELEASE_TIMESTAMP = 5249366402427387904;

        [STAThread]
        static int Main(string[] args)
        {
            List<Action<IntPtr>> messages = null;

            #region Launcher proxy

            if (args.Length > 1 && args[0] == "-pl")
            {
                int pid;
                if (!Int32.TryParse(args[1], out pid))
                    return -1;

                try
                {
                    using (var mf = MemoryMappedFile.OpenExisting(Messaging.MappedMessage.BASE_ID + "PL:" + pid))
                    {
                        using (var stream = mf.CreateViewStream())
                        {
                            var po = Client.ProxyLauncher.ProxyOptions.FromStream(stream);
                            var options = po.Options;
                            stream.Position = 0;

                            try
                            {
                                var startInfo = new ProcessStartInfo(options.FileName, options.Arguments);
                                startInfo.UseShellExecute = true;
                                startInfo.WorkingDirectory = options.WorkingDirectory;

                                foreach (var key in options.Variables.Keys)
                                {
                                    var value = options.Variables[key];
                                    if (!string.IsNullOrEmpty(value))
                                        System.Environment.SetEnvironmentVariable(key, value, EnvironmentVariableTarget.Process);
                                }

                                using (var p = new Process())
                                {
                                    p.StartInfo = startInfo;
                                    if (p.Start())
                                    {
                                        pid = p.Id;

                                        using (var writer = new BinaryWriter(stream, System.Text.Encoding.UTF8, true))
                                        {
                                            writer.Write((byte)Client.ProxyLauncher.LaunchResult.Success);
                                            writer.Write(pid);
                                        }
                                    }
                                    else
                                        throw new Exception("Failed to launch");
                                }
                            }
                            catch (Exception ex)
                            {
                                Util.Logging.Log(ex);

                                using (var writer = new BinaryWriter(stream, System.Text.Encoding.UTF8, true))
                                {
                                    writer.Write((byte)Client.ProxyLauncher.LaunchResult.Failed);
                                    writer.Write(ex.Message);
                                }

                                return -1;
                            }

                        }
                    }
                }
                catch (Exception e)
                {
                    Util.Logging.Log(e);
                    return -1;
                }

                return 0;
            }

            #endregion

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
                    #region -handle [-pid|-p|-n|-d] [processId|"path to exe"|"process name"|"directory of exe"] "objectName" [match (#)]

                    if (args.Length != 6 || args[2] != "-pid" && args[2] != "-p" && args[2] != "-n" && args[2] != "-d")
                        return new ArgumentException().HResult;

                    bool isId = args[2] == "-pid";
                    bool isName = args[2] == "-n";
                    bool isDir = args[2] == "-d";
                    string path = args[3];
                    string name = args[4];
                    byte match;
                    byte.TryParse(args[5], out match);
                    int pid = -1;

                    if (!isId)
                    {
                        FileInfo fi;
                        DirectoryInfo di;
                        if (isName)
                        {
                            fi = null;
                            di = null;
                        }
                        else
                        {
                            try
                            {
                                if (isDir)
                                {
                                    di = new DirectoryInfo(path);
                                    if (!di.Exists)
                                        return new DirectoryNotFoundException().HResult;
                                    fi = null;
                                }
                                else
                                {
                                    fi = new FileInfo(path);
                                    if (!fi.Exists)
                                        return new FileNotFoundException().HResult;
                                    di = null;
                                }
                            }
                            catch (Exception e)
                            {
                                return e.HResult;
                            }
                        }

                        Process[] ps;
                        if (isDir)
                            ps = Process.GetProcesses();
                        else
                            ps = Process.GetProcessesByName(isName ? path : fi.Name.Substring(0, fi.Name.Length - fi.Extension.Length));
                        DateTime newest = DateTime.MinValue;

                        foreach (Process p in ps)
                        {
                            using (p)
                            {
                                try
                                {
                                    if (!p.HasExited)
                                    {
                                        if (isName || (!isDir && string.Equals(p.MainModule.FileName, fi.FullName, StringComparison.OrdinalIgnoreCase)) || (isDir && Path.GetDirectoryName(p.MainModule.FileName).Equals(di.FullName, StringComparison.OrdinalIgnoreCase)))
                                        {
                                            if (p.StartTime > newest)
                                            {
                                                pid = p.Id;
                                                newest = p.StartTime;
                                            }
                                        }
                                    }
                                }
                                catch (Exception e)
                                {
                                    Console.WriteLine(e.Message);
                                }
                            }
                        }
                    }
                    else
                    {
                        try
                        {
                            pid = Int32.Parse(path);
                        }
                        catch (Exception e)
                        {
                            Console.WriteLine(e.Message);
                        }
                    }

                    if (pid != -1)
                    {
                        try
                        {
                            Win32Handles.IObjectHandle handle = Win32Handles.GetHandle(pid, name, (Win32Handles.MatchMode)match);
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
                        p.UseShellExecute = false;
                        p.CreateNoWindow = true;

                        p.Arguments = "user /add \"" + username + "\" \"" + password + "\"";

                        int exitCode;

                        using (Process cmd = Process.Start(p))
                        {
                            cmd.WaitForExit();

                            exitCode = cmd.ExitCode;
                        }

                        p = new ProcessStartInfo("wmic");
                        p.UseShellExecute = false;
                        p.CreateNoWindow = true;

                        p.Arguments = "USERACCOUNT WHERE Name='" + username + "' SET PasswordExpires=FALSE";

                        using (Process cmd = Process.Start(p))
                        {
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
                else if (args[1] == "-junction")
                {
                    #region -junction "link" "target"

                    if (args.Length < 4)
                        return new ArgumentException().HResult;

                    var link = args[2];
                    var target = args[3];

                    try
                    {
                        var di = new DirectoryInfo(link);
                        if (di.Exists && di.Attributes.HasFlag(FileAttributes.ReparsePoint))
                            di.Delete();

                        if (!Directory.Exists(target))
                            throw new DirectoryNotFoundException();

                        Windows.Symlink.CreateJunction(link, target);

                        Util.FileUtil.AllowFolderAccess(link, System.Security.AccessControl.FileSystemRights.Modify);
                    }
                    catch (Exception e)
                    {
                        return e.HResult;
                    }

                    #endregion
                }
                else if (args[1] == "-folder")
                {
                    #region -folder "path"

                    if (args.Length < 3)
                        return new ArgumentException().HResult;

                    var path = args[2];

                    try
                    {
                        var di = new DirectoryInfo(path);
                        if (!di.Exists)
                            di.Create();

                        Util.FileUtil.AllowFolderAccess(path, System.Security.AccessControl.FileSystemRights.Modify);
                    }
                    catch (Exception e)
                    {
                        return e.HResult;
                    }

                    #endregion
                }
                else if (args[1] == "-perfcounter")
                {
                    #region -perfcounter [processId]

                    int pid;

                    if (int.TryParse(args[2], out pid))
                    {
                        try
                        {
                            var instanceName = Util.PerfCounter.GetInstanceName(pid);
                            if (instanceName != null)
                            {
                                using (var counter = Util.PerfCounter.GetCounter(Util.PerfCounter.CategoryName.Process, Util.PerfCounter.CounterName.IOWriteByesPerSecond, instanceName)) { }
                                using (var counterRead = Util.PerfCounter.GetCounter(Util.PerfCounter.CategoryName.Process, Util.PerfCounter.CounterName.IOReadBytesPerSecond, instanceName)) { }
                            }
                        }
                        catch { }

                        return pid;
                    }

                    #endregion
                }
                else if (args[1] == "-hosts")
                {
                    #region -hosts [-add|-remove] ["hostname"] ["address"]

                    if (args.Length > 3)
                    {
                        var remove = false;
                        if (args[2] == "-add" || (remove = args[2] == "-remove"))
                        {
                            string address = null;
                            if (args.Length > 4)
                                address = args[4];

                            try
                            {
                                if (remove)
                                {
                                    Windows.Hosts.Remove(args[3], address);
                                }
                                else
                                {
                                    if (address == null)
                                        address = "127.0.0.1";
                                    Windows.Hosts.Add(args[3], address);
                                }
                            }
                            catch (Exception e)
                            {
                                return e.HResult;
                            }
                        }
                    }

                    #endregion
                }
                else if (args[1] == "-windowmask")
                {
                    #region -windowmask [parent_pid] [pid] [window] [flags]

                    try
                    {
                        var w = IntPtr.Size == 4 ? (IntPtr)int.Parse(args[4]) : (IntPtr)long.Parse(args[4]);
                        var flags = (UI.formMaskOverlay.EnableFlags)byte.Parse(args[5]);

                        using (var p = Process.GetProcessById(int.Parse(args[3])))
                        {
                            using (var parent = Process.GetProcessById(int.Parse(args[2])))
                            {
                                EventHandler onExit = delegate
                                {
                                    Application.Exit();
                                };
                                parent.Exited += onExit;
                                parent.EnableRaisingEvents = true;

                                if (p.HasExited || parent.HasExited)
                                    return 0;

                                Application.Run(new UI.formMaskOverlay(p, w, flags));
                            }
                        }
                    }
                    catch (Exception e)
                    {
                        Util.Logging.Log(e);
                        return e.HResult;
                    }

                    return 0;

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
                        Settings.ReadOnly = true;
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

            #region -update [path] [path] [etc]

            if (args.Length > 0 && args[0] == "-update")
            {
                var files = new List<string>();

                for (var i = 1; i < args.Length; i++)
                {
                    try
                    {
                        if (File.Exists(args[i]))
                        {
                            files.Add(Path.GetFullPath(args[i]));
                        }
                        else if (Directory.Exists(args[i]))
                        {
                            files.AddRange(Directory.GetFiles(args[i], "*.dat"));
                        }
                        else
                        {
                            var d = Path.GetDirectoryName(args[i]);
                            var searchAll = Path.GetFileName(d) == "*";
                            if (searchAll)
                                d = Path.GetDirectoryName(d);
                            var n = Path.GetFileName(args[i]);
                            if (n.Length == 0)
                                n = "*.dat";

                            files.AddRange(Directory.GetFiles(d, n, searchAll ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly));
                        }
                    }
                    catch { }
                }

                if (files.Count > 0)
                {
                    var update = new Messaging.UpdateMessage(files.Count);

                    update.files.AddRange(files);

                    Add(ref messages, delegate(IntPtr handle)
                    {
                        if (handle == IntPtr.Zero)
                            update.Send();
                        else
                            update.Send(handle);
                    });

                    Settings.Silent = true;
                }
                else
                {
                    return 0;
                }
            }

            #endregion

            #region Launch from args

            if (args.Length > 0)
            {
                Messaging.LaunchMessage launch = null;
                bool hasArgs = false;

                foreach (var arg in args)
                {
                    if (arg == "-l:silent")
                        Settings.Silent = true;
                    else if (arg.StartsWith("-l:uid:"))
                    {
                        var i = 7;
                        var limit = arg.IndexOf(' ', i);
                        if (limit == -1)
                            limit = arg.Length;

                        while (true)
                        {
                            var j = arg.IndexOf(',', i, limit - i);
                            if (j == -1)
                                j = limit;

                            ushort uid;
                            if (ushort.TryParse(arg.Substring(i, j - i), out uid))
                            {
                                if (launch == null)
                                    launch = new Messaging.LaunchMessage(1);
                                launch.accounts.Add(uid);
                            }
                            else
                                break;

                            i = ++j;
                            if (i >= limit)
                                break;
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

                    Add(ref messages, delegate(IntPtr handle)
                    {
                        if (handle == IntPtr.Zero)
                            launch.Send();
                        else
                            launch.Send(handle);
                    });
                }
            }

            #endregion

            #region -exportAccounts [path]

            if (args.Length > 0 && args[0] == "-exportcsv")
            {
                Settings.Load(); // Maybe I shouldn't do this here?!
                var gw1Accounts = new List<Settings.IGw1Account>();
                var gw2Accounts = new List<Settings.IGw2Account>();
                foreach(var uid in Settings.Accounts.GetKeys())
                {
                    switch (Settings.Accounts[uid].Value.Type)
                    {
                        case Settings.AccountType.GuildWars1:
                            gw1Accounts.Add((Settings.IGw1Account)Settings.Accounts[uid].Value);
                            break;
                        case Settings.AccountType.GuildWars2:
                            gw2Accounts.Add((Settings.IGw2Account)Settings.Accounts[uid].Value);
                            break;
                    }
                }

                // Create Gw1Accounts.csv
                var sb = new System.Text.StringBuilder();
                sb.AppendLine("sep=,");
                sb.AppendLine("UID,Name,Email");
                foreach (var account in gw1Accounts) 
                {
                    sb.AppendFormat("{0},{1},{2}", account.UID, account.Name, account.Email).AppendLine();
                }
                System.IO.File.WriteAllText(Path.Combine(Gw2Launcher.DataPath.AppData, "Gw1Accounts.csv"), sb.ToString());

                // Create Gw2Accounts.csv
                sb = new System.Text.StringBuilder();
                sb.AppendLine("sep=,");
                sb.AppendLine("UID,Name,Email,API Key");
                foreach (var account in gw2Accounts)
                {
                    sb.AppendFormat("{0},{1},{2},{3}", account.UID, account.Name, account.Email, account.ApiKey).AppendLine();
                }
                System.IO.File.WriteAllText(Path.Combine(Gw2Launcher.DataPath.AppData, "Gw2Accounts.csv"), sb.ToString());
            }

            #endregion

            #region QuickLaunch

            if (args.Length > 0 && args[0] == "-quicklaunch")
            {
                int v = -1;
                if (args.Length > 1)
                {
                    switch (args[1].ToLower())
                    {
                        case "gw1":
                            v = (int)Settings.AccountType.GuildWars1;
                            break;
                        case "gw2":
                            v = (int)Settings.AccountType.GuildWars2;
                            break;
                    }
                }

                Add(ref messages, delegate(IntPtr handle)
                {
                    if (handle == IntPtr.Zero)
                        Messaging.Messager.Post(Messaging.Messager.MessageType.QuickLaunch, v);
                    else
                        Messaging.Messager.Post(handle, Messaging.Messager.MessageType.QuickLaunch, v);
                });

                Settings.Silent = true;
            }

            #endregion

            #region TOTP code

            if (args.Length > 0 && args[0] == "-totpcode")
            {
                Add(ref messages, delegate(IntPtr handle)
                {
                    if (handle == IntPtr.Zero)
                        Messaging.Messager.Post(Messaging.Messager.MessageType.TotpCode, 0);
                    else
                        Messaging.Messager.Post(handle, Messaging.Messager.MessageType.TotpCode, 0);
                });
                Settings.Silent = true;
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
                                try
                                {
                                    Windows.FindWindow.FocusWindow(hWnd);
                                }
                                catch (Exception ex)
                                {
                                    Util.Logging.Log(ex);
                                }

                                return true;
                            }
                            return false;
                        });

                    #region By process

                    #endregion
                }

                if (messages != null)
                {
                    foreach (var m in messages)
                    {
                        m(IntPtr.Zero);
                    }
                }

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
                }

                Settings.Load();

                #region Wine detection

                try
                {
                    var l = NativeMethods.LoadLibrary("ntdll.dll");
                    if (l != IntPtr.Zero)
                    {
                        try
                        {
                            Settings.IsRunningWine = NativeMethods.GetProcAddress(l, "wine_get_version") != IntPtr.Zero;
                        }
                        finally
                        {
                            NativeMethods.FreeLibrary(l);
                        }
                    }
                }
                catch(Exception e)
                {
                    Util.Logging.Log(e);
                }

                #endregion

                var store = Settings.StoreCredentials;
                store.ValueChanged += StoredCredentials_ValueChanged;
                Security.Credentials.StoreCredentials = store.Value;

                Application.EnableVisualStyles();
                Application.SetCompatibleTextRenderingDefault(false);

                if (!Settings.Silent && Settings.Accounts.Count == 0 || args.Length > 0 && args[0] == "-quickstart")
                {
                    using (var quickstart = new UI.QuickStart.formQuickStart())
                    {
                        Application.Run(quickstart);
                    }
                }

                var f = new UI.formMain();

                Util.Users.Activate(true);

                if (messages != null)
                {
                    Task.Run(
                        delegate
                        {
                            foreach (var m in messages)
                            {
                                m(f.Handle);
                            }

                            messages = null;
                        });
                }

                Application.Run(f);

                OnExit();
            }
#if !DEBUG
            catch (Exception e)
            {
                Util.Logging.Crash(e);
                return -1;
            }
#endif
            finally
            {
                mutex.ReleaseMutex();
                mutex.Dispose();
            }

            return 0;
        }

        private static void Add(ref List<Action<IntPtr>> l, Action<IntPtr> action)
        {
            if (l == null)
                l = new List<Action<IntPtr>>();
            l.Add(action);
        }

        private static void OnExit()
        {
            try
            {
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

#if x86

        public static int GetValue(this IntPtr ptr)
        {
            return (int)ptr;
        }

        public static int GetValue32(this IntPtr ptr)
        {
            return (int)ptr;
        }

        public static uint GetValue(this UIntPtr ptr)
        {
            return (uint)ptr;
        }

#else

        public static long GetValue(this IntPtr ptr)
        {
            return (long)ptr;
        }

        public static int GetValue32(this IntPtr ptr)
        {
            return (int)(long)ptr;
        }

        public static ulong GetValue(this UIntPtr ptr)
        {
            return (ulong)ptr;
        }

#endif

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
