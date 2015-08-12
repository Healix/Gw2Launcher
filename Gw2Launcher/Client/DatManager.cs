using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Runtime.InteropServices;
using System.Security.AccessControl;

namespace Gw2Launcher.Client
{
    static class DatManager
    {
        private const string DAT_NAME = "Local.dat";

        enum DatFolder
        {
            AppData,
            Documents
        }

        public class UserAccountNotInitializedException : Exception
        {
            public UserAccountNotInitializedException(string username)
                : base("The user \"" + username + "\" must first ben logged in to before it can be used.")
            {
                this.UserName = username;
            }

            public string UserName
            {
                get;
                private set;
            }
        }

        static DatManager()
        {
        }

        private static string GetFolder(DatFolder folder)
        {
            string path;

            try
            {
                switch (folder)
                {
                    case DatFolder.Documents:
                        path = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments, Environment.SpecialFolderOption.DoNotVerify);
                        break;
                    default:
                        path = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData, Environment.SpecialFolderOption.Create);
                        break;
                }
            }
            catch
            {
                path = null;
            }

            if (string.IsNullOrWhiteSpace(path))
            {
                throw new UserAccountNotInitializedException(Environment.UserName);
            }

            return Path.Combine(path, "Guild Wars 2");
        }

        private static string GetFolder()
        {
            string path;

            try
            {
                path = GetFolder(DatFolder.Documents);
                if (File.Exists(Path.Combine(path, DAT_NAME)))
                    return path;
            }
            catch { }

            return GetFolder(DatFolder.AppData);
        }

        private static string GetPath(string folder, ushort uid)
        {
            return Path.Combine(folder, "Local." + uid + ".dat");
        }

        private static string GetPath(Settings.IDatFile dat)
        {
            if (dat == null || dat.Path == null)
                return "";
            return dat.Path;
        }

        private static bool Delete(string path)
        {
            try
            {
                if (File.Exists(path))
                    File.Delete(path);
                return true;
            }
            catch { }
            return false;
        }

        private static void Move(string from, string to)
        {
            if (!File.Exists(from))
                return;
            if (File.Exists(to))
                File.Delete(to);
            File.Move(from, to);
        }

        public static void Activate(Settings.IAccount account)
        {
            Security.Impersonation.IImpersonationToken impersonation;
            string username = Util.Users.GetUserName(account.WindowsAccount);

            if (Util.Users.IsCurrentUser(username))
                impersonation = null;
            else
                impersonation = Security.Impersonation.Impersonate(username, Security.Credentials.GetPassword(username));

            try
            {
                //appdata is the primary location, however if a Local.dat file exists under
                //the documents folder, it will take priority. If one was created under the
                //documents folder, it means that the Local.dat file that was under appdata
                //was corrupt
                string appdata = GetFolder(DatFolder.AppData);
                string documents = GetFolder(DatFolder.Documents);
                string localDat;

                if (!File.Exists(localDat = Path.Combine(documents, DAT_NAME)))
                {
                    //documents will be ignored
                    documents = null;
                    localDat = Path.Combine(appdata, DAT_NAME);
                }
                
                string datPath = GetPath(account.DatFile);

                if (datPath.Equals(localDat, StringComparison.OrdinalIgnoreCase)
                    || documents != null && datPath.Equals(Path.Combine(appdata, DAT_NAME), StringComparison.OrdinalIgnoreCase))
                {
                        //this dat file is already Local.dat
                        return;
                }

                string gfxName = "GFXSettings." + Path.GetFileName(Settings.GW2Path.Value);
                string gfxExt = ".xml";

                if (!Directory.Exists(appdata))
                {
                    try
                    {
                        Directory.CreateDirectory(appdata);
                    }
                    catch { }
                }

                foreach (var fid in Settings.DatFiles.GetKeys())
                {
                    var dat = Settings.DatFiles[fid];
                    var path = GetPath(dat.Value);

                    if (path.Equals(localDat, StringComparison.OrdinalIgnoreCase)
                        || documents != null && path.Equals(Path.Combine(appdata, DAT_NAME), StringComparison.OrdinalIgnoreCase))
                    {
                        try
                        {
                            string pathTo = GetPath(appdata, fid);

                            Move(localDat, pathTo);

                            try
                            {
                                Move(Path.Combine(Path.GetDirectoryName(path), gfxName + gfxExt), Path.Combine(appdata, gfxName + "." + fid + gfxExt));
                            }
                            catch { }

                            dat.Value.Path = pathTo;
                        }
                        catch (Exception e)
                        {
                            throw new Exception("Unable to move the current Local.dat file", e);
                        }

                        break;
                    }
                }

                if (File.Exists(localDat))
                {
                    try
                    {
                        File.Delete(localDat);
                    }
                    catch (Exception e)
                    {
                        throw new Exception("Unable to delete the current Local.dat file", e);
                    }
                }

                if (documents != null)
                {
                    string path = Path.Combine(appdata, DAT_NAME);

                    if (File.Exists(path))
                    {
                        try
                        {
                            File.Delete(path);
                        }
                        catch (Exception e)
                        {
                            throw new Exception("Unable to delete the current Local.dat file", e);
                        }
                    }
                }

                if (account.DatFile == null)
                {
                    //this account has no existing dat file
                    //the new Local.dat will be used
                    account.DatFile = Settings.CreateDatFile();
                    account.DatFile.Path = Path.Combine(appdata, DAT_NAME);
                }
                else
                {
                    string gfxSettings = Path.Combine(appdata, gfxName + gfxExt);

                    if (File.Exists(gfxSettings))
                    {
                        try
                        {
                            File.Delete(gfxSettings);
                        }
                        catch { }
                    }

                    if (documents != null)
                        localDat = Path.Combine(appdata, DAT_NAME);

                    try
                    {
                        if (File.Exists(datPath))
                            File.Move(datPath, localDat);
                        account.DatFile.Path = localDat;
                    }
                    catch (Exception e)
                    {
                        throw new Exception("Unable to move the current Local.dat file", e);
                    }

                    string gfxFrom = Path.Combine(Path.GetDirectoryName(datPath), gfxName + "." + account.DatFile.UID + gfxExt);

                    try
                    {
                        Move(gfxFrom, gfxSettings);
                    }
                    catch { }
                }
            }
            finally
            {
                if (impersonation != null)
                    impersonation.Dispose();
            }
        }
    }
}
