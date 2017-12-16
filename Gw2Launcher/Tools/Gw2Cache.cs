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
                        catch (Exception ex)
                        {
                            Util.Logging.Log(ex);
                        }
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
            //a cache folder will be created for every launch and again once the game starts
            //previously, only older folders were being deleted, but GW2 doesn't really reuse these
            //now, all folders are simply deleted, except those in use

            //DirectoryInfo newest = null;
            //DateTime utc = DateTime.MinValue;
            //DateTime limit = DateTime.UtcNow.Subtract(new TimeSpan(6, 0, 0));

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


                //DateTime lastWrite = DateTime.MinValue;
                //try
                //{
                //    foreach (var f in d.GetFiles("*", SearchOption.AllDirectories))
                //    {
                //        if (f.LastWriteTimeUtc > lastWrite)
                //        {
                //            lastWrite = f.LastWriteTimeUtc;
                //            if (lastWrite > utc)
                //                break;
                //        }
                //    }
                //}
                //catch (Exception e)
                //{
                //    Util.Logging.Log(e);
                //}

                //if (lastWrite > utc)
                //{
                //    if (newest != null && utc < limit)
                //    {
                //        try
                //        {
                //            newest.Delete(true);
                //        }
                //        catch (Exception ex)
                //        {
                //            Util.Logging.Log(ex);
                //        }
                //    }

                //    utc = lastWrite;
                //    newest = d;
                //}
                //else if (utc < limit)
                //{
                //    try
                //    {
                //        d.Delete(true);
                //    }
                //    catch (Exception ex)
                //    {
                //        Util.Logging.Log(ex);
                //    }
                //}
            }
        }
    }
}
