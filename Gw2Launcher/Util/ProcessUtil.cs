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

        public const string VAR_PARENT_PID = "l:PID";

        private static string GetPath()
        {
            return Assembly.GetExecutingAssembly().Location;
        }

        /// <summary>
        /// Launches with the specified args
        /// </summary>
        public static bool Execute(string args, bool admin, bool waitExit = true)
        {
            var p = new ProcessStartInfo(GetPath(), args);
            p.UseShellExecute = true;
            if (admin)
                p.Verb = "runas";

            using (var proc = Process.Start(p))
            {
                if (!waitExit)
                    return true;

                proc.WaitForExit();
                return proc.ExitCode == 0;
            }
        }

        /// <summary>
        /// Attempts to create the user account and initialize its user directory
        /// Runs as an administrator
        /// </summary>
        /// <param name="name"></param>
        /// <param name="password"></param>
        public static void CreateAccount(string name, string password)
        {
            Execute("-pu -user -u \"" + name + "\" -p \"" + password + "\"", true);
        }

        /// <summary>
        /// Creates the user's home folders
        /// </summary>
        /// <param name="username"></param>
        /// <param name="password"></param>
        public static void InitializeAccount(string username, System.Security.SecureString password)
        {
            var path = GetPath();

            try
            {
                //other users will be using this
                Util.FileUtil.AllowFileAccess(path, System.Security.AccessControl.FileSystemRights.Modify);
            }
            catch (Exception ex)
            {
                Util.Logging.Log(ex);
            }

            var p = new ProcessStartInfo(path, "-pu -userinit");

            p.UseShellExecute = false;
            p.UserName = username;
            p.LoadUserProfile = true;
            p.Password = password;
            p.WorkingDirectory = DataPath.AppDataAccountData; //Path.GetPathRoot(a.Path);

            //creating the user's profile will be handled by Windows
            //the process doesn't actually do anything itself

            using (var proc = Process.Start(p))
            {
                if (proc != null)
                {
                    proc.WaitForExit();
                }
            }
        }

        public static string GetMutexName(Settings.AccountType type)
        {
            switch (type)
            {
                case Settings.AccountType.GuildWars1:

                    return "AN-Mutex-Window-Guild Wars";

                case Settings.AccountType.GuildWars2:

                    return "AN-Mutex-Window-Guild Wars 2";

                default:

                    throw new NotSupportedException();
            }
        }

        /// <summary>
        /// Finds all processes that match the path and attempts to kill the specified type of mutex
        /// Runs as an administrator
        /// </summary>
        public static void KillMutexWindow(Settings.AccountType type, string gw2Path)
        {
            Execute("-pu -handle -p \"" + gw2Path + "\" \"" + GetMutexName(type) + "\" " + (byte)Windows.Win32Handles.MatchMode.EndsWith, true);
        }

        /// <summary>
        /// Finds all processes under the directory and attempts to kill the specified type of mutex
        /// Runs as an administrator
        /// </summary>
        /// <param name="gw2Path"></param>
        public static void KillMutexWindowByDirectory(Settings.AccountType type, string gw2Path)
        {
            Execute("-pu -handle -d \"" + gw2Path + "\" \"" + GetMutexName(type) + "\" " + (byte)Windows.Win32Handles.MatchMode.EndsWith, true);
        }

        /// <summary>
        /// Finds all processes that match the name and attempts to kill the specified type of mutex
        /// Runs as an administrator
        /// </summary>
        /// <param name="name"></param>
        public static void KillMutexWindowByProcessName(Settings.AccountType type, string name)
        {
            Execute("-pu -handle -n \"" + name + "\" \"" + GetMutexName(type) + "\" " + (byte)Windows.Win32Handles.MatchMode.EndsWith, true);
        }

        /// <summary>
        /// Scans entire system and attempts to kill the specified mutex type ("AN-Mutex-Window-Guild Wars 2" or "AN-Mutex-Window-Guild Wars")
        /// </summary>
        public static bool KillMutexWindow(Settings.AccountType type)
        {
            return KillMutexWindow(type, true);
        }

        public static bool KillMutexWindow(Settings.AccountType type, bool runAsAdmin)
        {
            return Execute("-pu -handle -pid 0 \"" + GetMutexName(type) + "\" " + (byte)Windows.Win32Handles.MatchMode.EndsWith, runAsAdmin);
        }

        public static bool KillMutex(string mutex, Windows.Win32Handles.MatchMode matchMode, bool runAsAdmin)
        {
            return Execute("-pu -handle -pid 0 \"" + mutex + "\" " + (byte)matchMode, runAsAdmin);
        }

        /// <summary>
        /// Finds the process with the ID and attempts to kill the specified mutex type
        /// Specify a null username to run as an administrator
        /// </summary>
        /// <param name="pid"></param>
        /// <param name="username"></param>
        /// <param name="password">The password for the username or null to run as the current user</param>
        public static void KillMutexWindow(Settings.AccountType type, int pid, string username, System.Security.SecureString password)
        {
            string n;
            if (type == Settings.AccountType.GuildWars1)
                n = "Guild Wars";
            else
                n = "Guild Wars 2";
            var path = GetPath();
            var p = new ProcessStartInfo(path, "-pu -handle -pid " + pid + " \"" + GetMutexName(type) + "\" " + (byte)Windows.Win32Handles.MatchMode.EndsWith);
            if (username != null)
            {
                p.UseShellExecute = false;
                if (password != null)
                {
                    try
                    {
                        //other users will be using this
                        Util.FileUtil.AllowFileAccess(path, System.Security.AccessControl.FileSystemRights.Modify);
                    }
                    catch (Exception ex)
                    {
                        Util.Logging.Log(ex);
                    }

                    p.UserName = username;
                    p.LoadUserProfile = true;
                    p.Password = password;
                    p.WorkingDirectory = Path.GetPathRoot(path); //the user may not have access to default working directory
                }
            }
            else
            {
                p.UseShellExecute = true;
                p.Verb = "runas";
            }

            using (var proc = Process.Start(p))
            {
                proc.WaitForExit();
            }
        }

        /// <summary>
        /// Deletes all gw2cache-{*} folders located in the specified folder
        /// </summary>
        /// <param name="roots">Folders containing gw2cache folders</param>
        public static void DeleteCacheFolders(IEnumerable<string> roots)
        {
            var args = new StringBuilder(512);

            args.Append("-pu -delgw2cache");

            foreach (string root in roots)
            {
                args.Append(" \"");
                args.Append(root);
                args.Append('"');
            }

            Execute(args.ToString(), true);
        }

        /// <summary>
        /// Creates the scheduled tasks "gw2launcher-users-active-yes" and "gw2launcher-users-active-no"
        /// that start this program with the options -users:active:yes or -users:active:no as
        /// a high priviledge user
        /// </summary>
        public static bool CreateInactiveUsersTask()
        {
            var path1 = Path.GetTempFileName();
            var path2 = Path.GetTempFileName();

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

                return Execute(args.ToString(), true);
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

        /// <summary>
        /// Runs the specified scheduled task
        /// </summary>
        /// <param name="name">The name of the task</param>
        public static void RunTask(string name)
        {
            Execute("-pu -task -run \"" + name + "\"", false);
        }

        /// <summary>
        /// Deletes the specified scheduled tasks
        /// </summary>
        /// <param name="names">The name of the task</param>
        public static void DeleteTask(IEnumerable<string> names)
        {
            var args = new StringBuilder(512);

            args.Append("-pu -task -delete");

            foreach (string name in names)
            {
                args.Append(" \"");
                args.Append(name);
                args.Append('"');
            }

            Execute(args.ToString(), true);
        }

        /// <summary>
        /// Activates or deactivates the specified users
        /// </summary>
        /// <param name="users"></param>
        /// <param name="activate"></param>
        public static void ActivateUsers(IEnumerable<string> users, bool activate)
        {
            var args = new StringBuilder(512);

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

            Execute(args.ToString(), true);
        }

        /// <summary>
        /// Creates the folder with all users modify rights
        /// </summary>
        public static bool CreateJunction(string link, string target)
        {
            var args = new StringBuilder(512);

            args.Append("-pu -junction \"");
            args.Append(link);
            args.Append("\" \"");
            args.Append(target);
            args.Append('"');

            return Execute(args.ToString(), true);
        }

        /// <summary>
        /// Creates the folder (if it doesn't exist) and gives modify rights to all users
        /// </summary>
        public static bool CreateFolder(string path)
        {
            var args = new StringBuilder(512);

            args.Append("-pu -folder \"");
            args.Append(path);
            args.Append('"');

            return Execute(args.ToString(), true);
        }

        /// <summary>
        /// Returns true if performance counters can be used
        /// </summary>
        public static bool QueryPerformanceCounters(int pid)
        {
            return Execute("-pu -perfcounter " + pid, false);
        }

        /// <summary>
        /// Shows the quick launch
        /// </summary>
        public static void ShowQuickLaunch()
        {
            Execute("-quicklaunch", false);
        }

        /// <summary>
        /// Adds or updates the entry to the hosts file
        /// </summary>
        public static bool AddHostsEntry(string hostname, string address)
        {
            return Execute("-pu -hosts -add \"" + hostname + "\" \"" + address + "\"", true);
        }

        /// <summary>
        /// Removes the entry from the host file
        /// </summary>
        public static bool RemoveHostsEntry(string hostname, string address = null)
        {
            return Execute("-pu -hosts -remove \"" + hostname + (address == null ? "\"" : "\" \"" + address + "\""), true);
        }

        /// <summary>
        /// Shows a window mask for the specified process and window
        /// </summary>
        public static Process ShowWindowMask(Process process, IntPtr window, UI.formMaskOverlay.EnableFlags flags)
        {
            using (var self = Process.GetCurrentProcess())
            {
                var p = new Process()
                {
                    StartInfo = new ProcessStartInfo(GetPath(), "-pu -windowmask " + self.Id + " " + process.Id + " " + window + " " + (byte)flags)
                    {
                        UseShellExecute = false,
                    },
                };

                try
                {
                    p.Start();
                }
                catch
                {
                    p.Dispose();
                    throw;
                }

                return p;
            }
        }

        /// <summary>
        /// Restores Gw2Launcher using the specified backup file
        /// </summary>
        /// <param name="path">Backup file</param>
        public static void Restore(string path)
        {
            using (var self = Process.GetCurrentProcess())
            {
                Execute("-restore \"" + path + "\" " + self.Id, false, false);
            }
        }

        /// <summary>
        /// Returns the first running process found that matches the path
        /// </summary>
        public static Process FindProcess(string path)
        {
            path = Path.GetFullPath(path);

            foreach (var p in Process.GetProcessesByName(Path.GetFileNameWithoutExtension(path)))
            {
                try
                {
                    if (p.MainModule.FileName.Equals(path, StringComparison.OrdinalIgnoreCase))
                    {
                        return p;
                    }
                }
                catch { }

                p.Dispose();
            }

            return null;
        }
    }
}
