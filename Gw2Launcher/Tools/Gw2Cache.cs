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
        /// Deletes cache folders for the specified account
        /// </summary>
        /// <param name="uid">Account's UID</param>
        /// <param name="all">True to delete all folders, otherwise folders will be checked to ensure they're not needed</param>
        public static void Delete(ushort uid, bool all)
        {
            if (all)
            {
                foreach (var d in Directory.GetDirectories(Path.Combine(DataPath.AppDataAccountDataTemp, uid.ToString()), GW2CACHE, SearchOption.TopDirectoryOnly))
                {
                    try
                    {
                        DeleteCacheFolder(d, DateTime.MinValue);
                    }
                    catch { }
                }
            }
            else
            {
                Delete(uid);
            }
        }

        public static void Delete(ushort uid)
        {
            Delete(new DirectoryInfo(Path.Combine(DataPath.AppDataAccountDataTemp, uid.ToString())).GetDirectories(GW2CACHE, SearchOption.TopDirectoryOnly));
        }

        public static void DeleteLoginCounter(ushort uid)
        {
            foreach (var root in Directory.GetDirectories(Path.Combine(DataPath.AppDataAccountDataTemp, uid.ToString()), GW2CACHE, SearchOption.TopDirectoryOnly))
            {
                var path = Path.Combine(root, "user", "Local Storage",  "coui_web_0.localstorage");

                try
                {
                    if (File.Exists(path))
                        File.Delete(path);
                }
                catch { }
            }
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
        public static bool DeleteCacheFolder(string path, DateTime date)
        {
            try
            {
                var user = Path.Combine(path, "user");

                for (var i = 0; i < 2; i++)
                {
                    var index = Path.Combine(user, "Cache", "index");

                    if (File.Exists(index))
                    {
                        if (date.Ticks == 0 || File.GetLastWriteTimeUtc(Path.Combine(user, "Cache", "data_1")) < date)
                        {
                            File.Delete(index);
                        }
                        else
                        {
                            return false;
                        }

                        break;
                    }
                    else if (!object.ReferenceEquals(user, path))
                    {
                        user = path;
                    }
                    else
                    {
                        break;
                    }
                }

                Util.FileUtil.DeleteDirectory(path, true);

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
                var path = Path.Combine(root, "user", "Cache", "index");

                try
                {
                    using (File.Open(path, FileMode.Open, FileAccess.Read, FileShare.None))
                    {
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
