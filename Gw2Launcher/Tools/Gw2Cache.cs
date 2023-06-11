using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace Gw2Launcher.Tools
{
    static class Gw2Cache
    {
        public const string USERNAME_GW2LAUNCHER = "@";

        private const string GW2CACHE = "gw2cache*";

        /// <summary>
        /// Deletes cache folders for the account and any other accounts sharing the same dat file
        /// </summary>
        /// <param name="all">True to delete all folders, otherwise folders will be checked to ensure they're not needed</param>
        /// <param name="options">Type of cache files to delete</param>
        public static void DeleteByDat(Settings.IGw2Account account, bool all, Settings.DeleteCacheOptions options = Settings.DeleteCacheOptions.All)
        {
            if (options == Settings.DeleteCacheOptions.None)
                return;

            var dat = account.DatFile;

            if (dat == null || dat.References <= 1)
            {
                Delete(account.UID, all, options);
            }
            else
            {
                var count = dat.References;

                foreach (var a in Util.Accounts.GetGw2Accounts())
                {
                    if (a.DatFile == dat)
                    {
                        try
                        {
                            Delete(a.UID, all, options);
                        }
                        catch { }

                        if (--count == 0)
                            break;
                    }
                }
            }
        }

        /// <summary>
        /// Deletes cache folders for the specified account
        /// </summary>
        /// <param name="uid">Account's UID</param>
        /// <param name="all">True to delete all folders, otherwise folders will be checked to ensure they're not needed</param>
        /// <param name="options">Type of cache files to delete</param>
        public static bool Delete(ushort uid, bool all, Settings.DeleteCacheOptions options = Settings.DeleteCacheOptions.All)
        {
            if (options == Settings.DeleteCacheOptions.None)
                return true;

            var b = true;
            DateTime t;

            if (all)
                t = DateTime.MinValue;
            else
                t = DateTime.UtcNow.AddMinutes(-5);

            foreach (var d in Directory.GetDirectories(Path.Combine(DataPath.AppDataAccountDataTemp, uid.ToString()), GW2CACHE, SearchOption.TopDirectoryOnly))
            {
                try
                {
                    if (!DeleteCacheFolder(d, t, options))
                        b = false;
                }
                catch 
                {
                    b = false;
                }
            }

            return b;
        }

        public static void Delete(ushort uid)
        {
            Delete(new DirectoryInfo(Path.Combine(DataPath.AppDataAccountDataTemp, uid.ToString())).GetDirectories(GW2CACHE, SearchOption.TopDirectoryOnly));
        }

        /// <summary>
        /// Deletes the login counter for the UID
        /// </summary>
        /// <param name="uid"></param>
        public static void DeleteLoginCounter(ushort uid)
        {
            DeleteLoginCounter(Path.Combine(DataPath.AppDataAccountDataTemp, uid.ToString()));
        }

        /// <summary>
        /// Deletes the login counter from all cache folders under the specified path
        /// </summary>
        public static void DeleteLoginCounter(string path)
        {
            foreach (var root in Directory.GetDirectories(path, GW2CACHE, SearchOption.TopDirectoryOnly))
            {
                //CoherentUI
                path = Path.Combine(root, "user", "Local Storage", "coui_web_0.localstorage");

                try
                {
                    if (File.Exists(path))
                        File.Delete(path);
                }
                catch { }

                //CEF
                path = Path.Combine(root, "user", "Cache", "Local Storage", "leveldb", "MANIFEST-000001");

                try
                {
                    if (File.Exists(path))
                        File.Delete(path);
                }
                catch { }
            }
        }

        /// <summary>
        /// Deletes the login counter from the %temp% folder
        /// </summary>
        public static void DeleteLoginCounter()
        {
            var path = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            if (string.IsNullOrWhiteSpace(path))
                return;
            DeleteLoginCounter(Path.Combine(path, "temp"));
        }

        public static void Delete(string username)
        {
            Delete(GetFolders(username));
        }

        public static void Delete()
        {
            var users = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            try
            {
                Delete(USERNAME_GW2LAUNCHER);
            }
            catch (Exception ex)
            {
                Util.Logging.Log(ex);
            }

            foreach (var a in Util.Accounts.GetGw2Accounts())
            {
                var username = Util.Users.GetUserName(a.WindowsAccount);
                if (users.Add(username))
                {
                    try
                    {
                        Delete(username);
                    }
                    catch (Exception ex)
                    {
                        Util.Logging.Log(ex);
                    }
                }
            }
        }

        public static DirectoryInfo[] GetFolders(string username)
        {
            IDisposable impersonation;
            if (username == USERNAME_GW2LAUNCHER || Util.Users.IsCurrentUser(username))
                impersonation = null;
            else
                impersonation = Security.Impersonation.Impersonate(username, Security.Credentials.GetPassword(username));

            string path;

            try
            {
                SearchOption searchOption;

                if (username == USERNAME_GW2LAUNCHER)
                {
                    path = DataPath.AppDataAccountDataTemp;
                    searchOption = SearchOption.AllDirectories;
                }
                else
                {
                    path = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
                    if (string.IsNullOrWhiteSpace(path))
                        return new DirectoryInfo[0];
                    path = Path.Combine(path, "temp");
                    if (!Directory.Exists(path))
                        return new DirectoryInfo[0];
                    searchOption = SearchOption.TopDirectoryOnly;
                }

                var _folders = new List<DirectoryInfo>();

                foreach (var folder in new DirectoryInfo(path).EnumerateDirectories(GW2CACHE, searchOption))
                {
                    if ((folder.Attributes & FileAttributes.ReparsePoint) != FileAttributes.ReparsePoint)
                        _folders.Add(folder);

                    if (impersonation != null)
                    {
                        try
                        {
                            Util.FileUtil.AllowFolderAccess(folder.FullName, System.Security.AccessControl.FileSystemRights.Modify);
                        }
                        catch (Exception ex)
                        {
                            Util.Logging.Log(ex);
                        }
                    }
                }

                //var folders = new DirectoryInfo(path).GetDirectories(GW2CACHE, searchOption);

                return _folders.ToArray();
            }
            finally
            {
                if (impersonation != null)
                    impersonation.Dispose();
            }
        }

        /// <summary>
        /// Deletes the cache folder if it was last used past the specified date
        /// </summary>
        /// <param name="date">Any folders written to past this date will be ignored; use DateTime.MinValue to not check</param>
        public static bool DeleteCacheFolder(string path, DateTime date, Settings.DeleteCacheOptions options = Settings.DeleteCacheOptions.All)
        {
            if (options == Settings.DeleteCacheOptions.None || File.GetAttributes(path).HasFlag(FileAttributes.ReparsePoint))
                return true;

            try
            {
                var user = Path.Combine(path, "user");
                var hasUser = Directory.Exists(user);

                //note the path may be the gw2cache folder or the user folder as gw2cache-user
                if (!hasUser)
                    user = path;

                string index;

                if (File.Exists(index = Path.Combine(user, "Cache", "index"))) //CoherentUI
                {
                    if ((options & Settings.DeleteCacheOptions.Web) != 0)
                    {
                        if (date.Ticks == 0 || File.GetLastWriteTimeUtc(Path.Combine(user, "Cache", "data_1")) < date)
                        {
                            File.Delete(index);
                        }
                        else
                        {
                            return false;
                        }
                    }
                }
                else if (File.Exists(index = Path.Combine(user, "Cache", "LOCK"))) //CEF
                {
                    if ((options & Settings.DeleteCacheOptions.Web) != 0)
                    {
                        if (date.Ticks == 0 || File.GetLastWriteTimeUtc(Path.Combine(user, "Cache", "Cache", "Cache_Data", "data_1")) < date)
                        {
                            File.Delete(index);
                        }
                        else
                        {
                            return false;
                        }
                    }
                }
                else
                {
                    user = null;
                }

                switch (options)
                {
                    case Settings.DeleteCacheOptions.All:

                        Util.FileUtil.DeleteDirectory(path, true);

                        break;
                    case Settings.DeleteCacheOptions.Web:

                        if (user != null)
                        {
                            Util.FileUtil.DeleteDirectory(user, true);
                        }

                        break;
                    case Settings.DeleteCacheOptions.Binaries:

                        if (hasUser || user == null)
                        {
                            foreach (var f in Directory.GetFiles(path))
                            {
                                File.Delete(f);
                            }
                        }

                        break;
                }

                return true;
            }
            catch 
            {
                return false;
            }
        }

        private static void Delete(DirectoryInfo[] folders)
        {
            //gw2 will remember its cache location in Local.dat and reuse it if possible,
            //but it will forgotten with the next patch. If gw2 can't update Local.dat, the
            //cache folder will be recreated every time

            var time = DateTime.UtcNow.AddMinutes(-5);

            foreach (var d in folders)
            {
                DeleteCacheFolder(d.FullName, time);
            }
        }

        /// <summary>
        /// Returns the path to the gw2cache folder currently being used, or null if not available
        /// </summary>
        public static string FindPath(ushort uid)
        {
            foreach (var root in Directory.GetDirectories(Path.Combine(DataPath.AppDataAccountDataTemp, uid.ToString()), GW2CACHE, SearchOption.TopDirectoryOnly))
            {
                try
                {
                    string path;

                    //CEF
                    path = Path.Combine(root, "user", "Cache", "LOCK");

                    if (File.Exists(path))
                    {
                        using (File.Open(path, FileMode.Open, FileAccess.Read, FileShare.None))
                        {
                        }
                    }

                    //CoherentUI
                    path = Path.Combine(root, "user", "Cache", "index");

                    if (File.Exists(path))
                    {
                        using (File.Open(path, FileMode.Open, FileAccess.Read, FileShare.None))
                        {
                        }
                    }
                }
                catch (IOException e)
                {
                    switch (e.HResult & 0xFFFF)
                    {
                        case 32: //ERROR_SHARING_VIOLATION

                            return root;
                    }
                }
                catch { }
            }

            return null;
        }
    }
}
