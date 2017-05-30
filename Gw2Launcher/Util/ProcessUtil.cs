using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;
using System.IO;
using System.Diagnostics;
using System.IO.Pipes;

namespace Gw2Launcher.Util
{
    class ProcessUtil
    {
        private class ApplicationObject : IDisposable
        {
            public ApplicationObject()
            {

            }

            public string Path
            {
                get
                {
                    return System.IO.Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
                }
            }

            public void Dispose()
            {

            }

            /*//ProcessUtil.exe (no longer used)
            protected static object _lock;
            protected static int references;

            static ApplicationObject()
            {
                references = 0;
                _lock = new object();
            }

            public ApplicationObject()
            {
                lock (_lock)
                {
                    references++;
                    Create();
                }
            }

            private void Create()
            {
                string path = System.IO.Path.Combine(DataPath.AppData, "ProcessUtil.exe");
                Assembly assembly = Assembly.GetExecutingAssembly();
                
                using (Stream stream = assembly.GetManifestResourceStream(typeof(Program).Namespace + ".Resources.ProcessUtil.exe"))
                {
                    FileInfo fi = new FileInfo(path);

                    if (fi.Exists)
                    {
                        int i = 0;

                        do
                        {
                            if (fi.Exists)
                            {
                                if (fi.Length == stream.Length)
                                {
                                    this.Path = path;
                                    return;
                                }
                                else
                                {
                                    try
                                    {
                                        fi.Delete();
                                        break;
                                    }
                                    catch { }

                                    path = System.IO.Path.Combine(DataPath.AppData, "ProcessUtil." + ++i + ".exe");
                                    fi = new FileInfo(path);
                                }
                            }
                            else
                                break;
                        }
                        while (true);
                    }

                    int retries = 0;

                    do
                    {
                        try
                        {
                            using (Stream output = File.Create(path))
                            {

                                int read;
                                byte[] buffer = new byte[stream.Length];
                                while ((read = stream.Read(buffer, 0, buffer.Length)) > 0)
                                {
                                    output.Write(buffer, 0, read);
                                }
                            }

                            this.Path = path;
                            return;
                        }
                        catch 
                        {
                            try
                            {
                                File.Delete(path);
                            }
                            catch { }
                        }

                        path = System.IO.Path.Combine(DataPath.AppData, "ProcessUtil." + ++retries + ".exe");

                        if (retries > 5)
                        {
                            this.Path = null;
                            return;
                        }
                    }
                    while (true);
                }
            }

            public string Path
            {
                get;
                private set;
            }

            public void Dispose()
            {
                lock (_lock)
                {
                    references--;
                    if (references == 0)
                    {
                        try
                        {
                            File.Delete(this.Path);
                        }
                        catch { }
                    }
                }
            }
            */
        }

        static ProcessUtil()
        {

        }

        /// <summary>
        /// Attempts to create the user account and initialize its user directory
        /// Runs as an administrator
        /// </summary>
        /// <param name="name"></param>
        /// <param name="password"></param>
        public static void CreateAccount(string name, string password)
        {
            using (ApplicationObject a = new ApplicationObject())
            {
                ProcessStartInfo p = new ProcessStartInfo(a.Path, "-pu -user -u \"" + name + "\" -p \"" + password + "\"");
                p.UseShellExecute = true;
                p.Verb = "runas";

                Process proc = Process.Start(p);
                if (proc != null)
                {
                    proc.WaitForExit();
                }
            }
        }

        /// <summary>
        /// Creates the user's home folders
        /// </summary>
        /// <param name="username"></param>
        /// <param name="password"></param>
        public static void InitializeAccount(string username, System.Security.SecureString password)
        {
            using (ApplicationObject a = new ApplicationObject())
            {
                try
                {
                    //other users will be using this
                    Util.FileUtil.AllowFileAccess(a.Path, System.Security.AccessControl.FileSystemRights.Modify);
                }
                catch (Exception ex)
                {
                    Util.Logging.Log(ex);
                }

                ProcessStartInfo p = new ProcessStartInfo(a.Path, "-pu -userinit");

                p.UseShellExecute = false;
                p.UserName = username;
                p.LoadUserProfile = true;
                p.Password = password;
                p.WorkingDirectory = Path.GetPathRoot(a.Path);

                //creating the user's profile will be handled by Windows
                //the process doesn't actually do anything itself

                Process proc = Process.Start(p);
                if (proc != null)
                {
                    proc.WaitForExit();
                }
            }
        }

        /// <summary>
        /// Finds all processes that match the path and attempts to kill "AN-Mutex-Window-Guild Wars 2"
        /// Runs as an administrator
        /// </summary>
        /// <param name="gw2Path"></param>
        public static void KillMutexWindow(string gw2Path)
        {
            using (ApplicationObject a = new ApplicationObject())
            {
                ProcessStartInfo p = new ProcessStartInfo(a.Path, "-pu -handle -p \"" + gw2Path + "\" \"AN-Mutex-Window-Guild Wars 2\" 0");
                p.UseShellExecute = true;
                p.Verb = "runas";

                Process proc = Process.Start(p);
                if (proc != null)
                {
                    proc.WaitForExit();
                }
            }
        }

        /// <summary>
        /// Finds all processes that match the name and attempts to kill "AN-Mutex-Window-Guild Wars 2"
        /// Runs as an administrator
        /// </summary>
        /// <param name="name"></param>
        public static void KillMutexWindowByProcessName(string name)
        {
            using (ApplicationObject a = new ApplicationObject())
            {
                ProcessStartInfo p = new ProcessStartInfo(a.Path, "-pu -handle -n \"" + name + "\" \"AN-Mutex-Window-Guild Wars 2\" 0");
                p.UseShellExecute = true;
                p.Verb = "runas";

                Process proc = Process.Start(p);
                if (proc != null)
                {
                    proc.WaitForExit();
                }
            }
        }

        /// <summary>
        /// Scans entire system and attempts to kill "AN-Mutex-Window-Guild Wars 2"
        /// </summary>
        public static void KillMutexWindow()
        {
            using (ApplicationObject a = new ApplicationObject())
            {
                ProcessStartInfo p = new ProcessStartInfo(a.Path, "-pu -handle -pid 0 \"AN-Mutex-Window-Guild Wars 2\" 0");
                p.UseShellExecute = true;
                p.Verb = "runas";

                Process proc = Process.Start(p);
                if (proc != null)
                {
                    proc.WaitForExit();
                }
            }
        }

        /// <summary>
        /// Finds the process with the ID and attempts to kill "AN-Mutex-Window-Guild Wars 2"
        /// Specify a null username to run as an administrator
        /// </summary>
        /// <param name="pid"></param>
        /// <param name="username"></param>
        /// <param name="password">The password for the username or null to run as the current user</param>
        public static void KillMutexWindow(int pid, string username, System.Security.SecureString password)
        {
            using (ApplicationObject a = new ApplicationObject())
            {
                ProcessStartInfo p = new ProcessStartInfo(a.Path, "-pu -handle -pid " + pid + " \"AN-Mutex-Window-Guild Wars 2\" 0");
                if (username != null)
                {
                    p.UseShellExecute = false;
                    if (password != null)
                    {
                        try
                        {
                            //other users will be using this
                            Util.FileUtil.AllowFileAccess(a.Path, System.Security.AccessControl.FileSystemRights.Modify);
                        }
                        catch (Exception ex)
                        {
                            Util.Logging.Log(ex);
                        }

                        p.UserName = username;
                        p.LoadUserProfile = true;
                        p.Password = password;
                        p.WorkingDirectory = Path.GetPathRoot(a.Path);
                    }
                }
                else
                {
                    p.UseShellExecute = true;
                    p.Verb = "runas";
                }

                Process proc = Process.Start(p);
                if (proc != null)
                {
                    proc.WaitForExit();
                }
            }
        }

        /// <summary>
        /// Deletes all gw2cache-{*} folders located in the specified folder
        /// </summary>
        /// <param name="roots">Folders containing gw2cache folders</param>
        public static void DeleteCacheFolders(IEnumerable<string> roots)
        {
            using (ApplicationObject a = new ApplicationObject())
            {
                StringBuilder args = new StringBuilder(512);

                args.Append("-pu -delgw2cache");

                foreach (string root in roots)
                {
                    args.Append(" \"");
                    args.Append(root);
                    args.Append('"');
                }

                ProcessStartInfo p = new ProcessStartInfo(a.Path, args.ToString());
                p.UseShellExecute = true;
                p.Verb = "runas";

                Process proc = Process.Start(p);
                if (proc != null)
                {
                    proc.WaitForExit();
                }
            }
        }

        /// <summary>
        /// Creates the scheduled tasks "gw2launcher-users-active-yes" and "gw2launcher-users-active-no"
        /// that start this program with the options -users:active:yes or -users:active:no as
        /// a high priviledge user
        /// </summary>
        public static bool CreateInactiveUsersTask()
        {
            using (ApplicationObject a = new ApplicationObject())
            {
                string path1 = Path.GetTempFileName();
                string path2 = Path.GetTempFileName();

                try
                {
                    string _xml1, _xml2;

                    _xml1 = Properties.Resources.Task.Replace("{path}", Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location));

                    _xml2 = _xml1.Replace("{args}", "-users:active:no");
                    _xml1 = _xml1.Replace("{args}", "-users:active:yes");

                    File.WriteAllText(path1, _xml1);
                    File.WriteAllText(path2, _xml2);

                    StringBuilder args = new StringBuilder(512);
                    args.Append("-pu -task -create \"gw2launcher-users-active-yes\" \"");
                    args.Append(path1);
                    args.Append("\" \"gw2launcher-users-active-no\" \"");
                    args.Append(path2);
                    args.Append('"');

                    ProcessStartInfo p = new ProcessStartInfo(a.Path, args.ToString());
                    p.UseShellExecute = true;
                    p.Verb = "runas";

                    Process proc = Process.Start(p);
                    if (proc != null)
                    {
                        proc.WaitForExit();
                        return proc.ExitCode == 0;
                    }

                    return false;
                }
                finally
                {
                    try
                    {
                        File.Delete(path1);
                    }
                    catch (Exception ex)
                    {
                        Util.Logging.Log(ex);
                    }
                    try
                    {
                        File.Delete(path2);
                    }
                    catch (Exception ex)
                    {
                        Util.Logging.Log(ex);
                    }
                }
            }
        }

        /// <summary>
        /// Runs the specified scheduled task
        /// </summary>
        /// <param name="name">The name of the task</param>
        public static void RunTask(string name)
        {
            using (ApplicationObject a = new ApplicationObject())
            {
                ProcessStartInfo p = new ProcessStartInfo(a.Path, "-pu -task -run \"" + name + "\"");
                p.UseShellExecute = true;

                Process proc = Process.Start(p);
                if (proc != null)
                {
                    proc.WaitForExit();
                }
            }
        }

        /// <summary>
        /// Deletes the specified scheduled tasks
        /// </summary>
        /// <param name="names">The name of the task</param>
        public static void DeleteTask(IEnumerable<string> names)
        {
            using (ApplicationObject a = new ApplicationObject())
            {
                StringBuilder args = new StringBuilder(512);
                args.Append("-pu -task -delete");
                foreach (string name in names)
                {
                    args.Append(" \"");
                    args.Append(name);
                    args.Append('"');
                }

                ProcessStartInfo p = new ProcessStartInfo(a.Path, args.ToString());
                p.UseShellExecute = true;
                p.Verb = "runas";

                Process proc = Process.Start(p);
                if (proc != null)
                {
                    proc.WaitForExit();
                }
            }
        }

        /// <summary>
        /// Activates or deactivates the specified users
        /// </summary>
        /// <param name="users"></param>
        /// <param name="activate"></param>
        public static void ActivateUsers(IEnumerable<string> users, bool activate)
        {
            using (ApplicationObject a = new ApplicationObject())
            {
                StringBuilder args = new StringBuilder(512);
                args.Append("-pu -users -activate:");
                if (activate)
                    args.Append("yes");
                else
                    args.Append("no");

                foreach (string user in users)
                {
                    args.Append(" \"");
                    args.Append(user);
                    args.Append('"');
                }

                ProcessStartInfo p = new ProcessStartInfo(a.Path, args.ToString());
                p.UseShellExecute = true;
                p.Verb = "runas";

                Process proc = Process.Start(p);
                if (proc != null)
                {
                    proc.WaitForExit();
                }
            }
        }
    }
}
