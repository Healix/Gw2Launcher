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

        private const string GW2CACHE = "gw2cache-{*}";

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

            foreach (ushort uid in Settings.Accounts.GetKeys())
            {
                var account = Settings.Accounts[uid];
                if (account.HasValue)
                {
                    string username = Util.Users.GetUserName(account.Value.WindowsAccount);
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
        }

        public static DirectoryInfo[] GetFolders(string username)
        {
            Security.Impersonation.IImpersonationToken impersonation;
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

                var folders = new DirectoryInfo(path).GetDirectories(GW2CACHE, searchOption);

                if (impersonation != null)
                {
                    foreach (var folder in folders)
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

                return folders;
            }
            finally
            {
                if (impersonation != null)
                    impersonation.Dispose();
            }
        }

        private static void Delete(DirectoryInfo[] folders)
        {
            //gw2 will remember its cache location in Local.dat and reuse it if possible,
            //but it will forgotten with the next patch. If gw2 can't update Local.dat, the
            //cache folder will be recreated every time

            var time = DateTime.UtcNow.AddMinutes(-1);

            foreach (var d in folders)
            {
                try
                {
                    if (d.LastWriteTimeUtc < time)
                    {
                        var index = Path.Combine(d.FullName, "user", "Cache", "index");
                        if (File.Exists(index))
                            File.Delete(index);
                        d.Delete(true);
                    }
                }
                catch (Exception e)
                {
                    Util.Logging.Log(e);
                }
            }
        }
    }
}
