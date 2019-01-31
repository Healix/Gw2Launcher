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
                if (username == USERNAME_GW2LAUNCHER)
                {
                    path = DataPath.AppDataAccountDataTemp;
                }
                else
                {
                    path = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
                    if (string.IsNullOrWhiteSpace(path))
                        return new DirectoryInfo[0];
                    path = Path.Combine(path, "temp");
                    if (!Directory.Exists(path))
                        return new DirectoryInfo[0];
                }

                var folders = new DirectoryInfo(path).GetDirectories(GW2CACHE, SearchOption.AllDirectories);

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
            //a cache folder will be created for every launch and again once the game starts
            //previously, only older folders were being deleted, but GW2 doesn't really reuse these
            //now, all folders are simply deleted, except those in use

            foreach (var d in folders)
            {
                try
                {
                    var index = new FileInfo(Path.Combine(d.FullName, "user", "Cache", "index"));
                    if (index.Exists)
                        index.Delete();
                    d.Delete(true);
                }
                catch (Exception e)
                {
                    Util.Logging.Log(e);
                }
            }
        }
    }
}
