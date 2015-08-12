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
        private const string GW2CACHE = "gw2cache-{*}";

        public static void Delete(string username)
        {
            Delete(GetFolders(username));
        }

        public static void Delete()
        {
            HashSet<string> users = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

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
                        catch { }
                    }
                }
            }
        }

        public static DirectoryInfo[] GetFolders(string username)
        {
            Security.Impersonation.IImpersonationToken impersonation;
            if (Util.Users.IsCurrentUser(username))
                impersonation = null;
            else
                impersonation = Security.Impersonation.Impersonate(username, Security.Credentials.GetPassword(username));

            string path;

            try
            {
                path = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
                
                if (string.IsNullOrWhiteSpace(path))
                {
                    return new DirectoryInfo[0];
                }
                else
                {
                    path = Path.Combine(path, "temp");
                    if (!Directory.Exists(path))
                        return new DirectoryInfo[0];

                    var folders = new DirectoryInfo(path).GetDirectories(GW2CACHE, SearchOption.TopDirectoryOnly);

                    foreach (var folder in folders)
                    {
                        try
                        {
                            Util.FileUtil.AllowFolderAccess(folder.FullName, System.Security.AccessControl.FileSystemRights.Modify);
                        }
                        catch { }
                    }

                    return folders;
                }
            }
            finally
            {
                if (impersonation != null)
                    impersonation.Dispose();
            }
        }

        private static void Delete(DirectoryInfo[] folders)
        {
            DirectoryInfo newest = null;
            DateTime utc = DateTime.MinValue;

            foreach (var d in folders)
            {
                DateTime lastWrite = DateTime.MinValue;

                try
                {
                    foreach (var f in d.GetFiles("*", SearchOption.AllDirectories))
                    {
                        if (f.LastWriteTimeUtc > lastWrite)
                        {
                            lastWrite = f.LastWriteTimeUtc;
                            if (lastWrite > utc)
                                break;
                        }
                    }
                }
                catch
                {
                }

                if (lastWrite > utc)
                {
                    if (newest != null)
                    {
                        try
                        {
                            newest.Delete(true);
                        }
                        catch { }
                    }

                    utc = lastWrite;
                    newest = d;
                }
                else
                {
                    try
                    {
                        d.Delete(true);
                    }
                    catch { }
                }
            }
        }
    }
}
