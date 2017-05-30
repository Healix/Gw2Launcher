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
        private const string GFX_NAME = "GFXSettings.{0}.xml";

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
            catch (Exception e)
            {
                Util.Logging.Log(e);
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
            catch (Exception ex)
            {
                Util.Logging.Log(ex);
            }

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
            catch (Exception ex)
            {
                Util.Logging.Log(ex);
            }
            return false;
        }
        
        public static void Delete(Settings.IDatFile dat)
        {
            Delete(dat, true);
        }

        private static void Delete(Settings.IDatFile dat, bool impersonate)
        {
            string userprofile = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
            string path = dat.Path;

            if (path.StartsWith(userprofile, StringComparison.OrdinalIgnoreCase))
            {
                string documents = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
                string appdata = Path.Combine(GetFolder(DatFolder.AppData), dat.UID.ToString());
                if (path.StartsWith(appdata, StringComparison.OrdinalIgnoreCase))
                {
                    if (documents.StartsWith(userprofile, StringComparison.OrdinalIgnoreCase))
                    {
                        var d = new DirectoryInfo(Path.Combine(appdata, documents.Substring(userprofile.Length + 1)));
                        if (d.Exists)
                        {
                            var f = d.GetFiles();
                            int count = d.Attributes.HasFlag(FileAttributes.ReparsePoint) ? 0 : d.GetFiles().Length;
                            if (count == 0)
                            {
                                try
                                {
                                    d.Delete();
                                }
                                catch (Exception ex)
                                {
                                    Util.Logging.Log(ex);
                                }
                            }
                            else
                            {
                                d.Attributes |= FileAttributes.ReadOnly;
                            }
                        }
                    }

                    try
                    {
                        if (Directory.Exists(appdata))
                            Directory.Delete(appdata, true);
                    }
                    catch (Exception ex)
                    {
                        Util.Logging.Log(ex);
                    }
                }
            }
            else if (impersonate)
            {
                var users = Path.GetDirectoryName(userprofile);
                if (path.StartsWith(users, StringComparison.OrdinalIgnoreCase))
                {
                    int i = users.Length + 1;
                    string username = path.Substring(i, path.IndexOf(Path.DirectorySeparatorChar, i) - i);

                    Security.Impersonation.IImpersonationToken impersonation = null;

                    try
                    {
                        impersonation = Security.Impersonation.Impersonate(username, Security.Credentials.GetPassword(username));

                        Delete(dat, false);

                        return;
                    }
                    catch (Exception e)
                    {
                        Util.Logging.Log(e);
                    }
                    finally
                    {
                        if (impersonation != null)
                            impersonation.Dispose();
                    }
                }
            }

            if (File.Exists(path))
                File.Delete(path);
        }

        private static List<string> GetParents(string path)
        {
            List<string> parents = new List<string>();
            DirectoryInfo di = new DirectoryInfo(path);

            while (di.Parent != di.Root)
            {
                parents.Add(di.Name);
                di = di.Parent;
            }

            parents.Reverse();
            return parents;
        }

        private static void Move(string from, string to)
        {
            if (!File.Exists(from))
                return;
            if (File.Exists(to))
                File.Delete(to);
            File.Move(from, to);
        }

        private static bool CreateCustomUserprofile(ushort uid)
        {
            string appdata = GetFolder(DatFolder.AppData);
            string documents = GetFolder(DatFolder.Documents);
            
            string userprofile = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile, Environment.SpecialFolderOption.Create);
            string custom;

            if (appdata.StartsWith(userprofile, StringComparison.OrdinalIgnoreCase))
            {
                custom = Path.Combine(appdata, uid.ToString());

                //initialize a fake userprofile directory, linking the documents folder back to the main user (for screenshots)
                if (!Directory.Exists(custom))
                {
                    new DirectoryInfo(Path.Combine(custom, appdata.Substring(userprofile.Length + 1))).Create();

                    if (documents.StartsWith(userprofile, StringComparison.OrdinalIgnoreCase))
                    {
                        var di = new DirectoryInfo(Path.Combine(custom, documents.Substring(userprofile.Length + 1))).Parent;
                        if (!di.Exists)
                        {
                            if (!di.Parent.Exists)
                                di.Parent.Create();

                            try
                            {
                                Windows.Symlink.CreateJunction(di.FullName, Path.GetDirectoryName(documents));
                            }
                            catch (Exception ex)
                            {
                                Util.Logging.Log(ex);
                            }
                        }
                    }
                }

                return true;
            }

            return false;
        }

        /// <summary>
        /// Activates the account's Local.dat file
        /// </summary>
        /// <returns>true if a custom profile is being used</returns>
        public static bool Activate(Settings.IAccount account)
        {
            Security.Impersonation.IImpersonationToken impersonation;
            string username = Util.Users.GetUserName(account.WindowsAccount);

            if (Util.Users.IsCurrentUser(username))
                impersonation = null;
            else
                impersonation = Security.Impersonation.Impersonate(username, Security.Credentials.GetPassword(username));

            //documents overrides appdata. if documents exists, move it to appdata and overwrite
            //if the default Local.dat already exists, create a custom userprofile for this account
            //if this account doesn't have a Local.dat, use the existing one if it has no owner, otherwise copy it

            try
            {
                string appdata = GetFolder(DatFolder.AppData);
                string documents = GetFolder(DatFolder.Documents);
                string localDat;

                string userprofile = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile, Environment.SpecialFolderOption.Create);
                string custom;

                if (appdata.StartsWith(userprofile, StringComparison.OrdinalIgnoreCase))
                {
                    string uid;
                    if (account.DatFile != null)
                        uid = account.DatFile.UID.ToString();
                    else
                        uid = "{0}";
                    custom = Path.Combine(appdata, uid, appdata.Substring(userprofile.Length + 1));
                }
                else
                    custom = null;

                //if Local.dat is located under documents, move it back to appdata
                if (File.Exists(localDat = Path.Combine(documents, DAT_NAME)))
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
                            Util.Logging.Log(e);
                            throw new Exception("Unable to delete the old Local.dat file from appdata", e);
                        }
                    }

                    try
                    {
                        Move(localDat, path);
                    }
                    catch (Exception e)
                    {
                        Util.Logging.Log(e);
                        throw new Exception("Unable to move the current Local.dat file from documents to appdata", e);
                    }

                    foreach (var fid in Settings.DatFiles.GetKeys())
                    {
                        var dat = Settings.DatFiles[fid];
                        if (GetPath(dat.Value).Equals(localDat, StringComparison.OrdinalIgnoreCase))
                        {
                            account.DatFile.Path = path;
                            break;
                        }
                    }

                    localDat = path;
                }
                else
                {
                    documents = null;
                    localDat = Path.Combine(appdata, DAT_NAME);
                }

                string datPath = GetPath(account.DatFile);

                if (datPath.Equals(localDat, StringComparison.OrdinalIgnoreCase))
                {
                    //this dat file is already Local.dat
                    if (account.DatFile.IsInitialized && !File.Exists(account.DatFile.Path))
                        account.DatFile.IsInitialized = false;
                    return false;
                }

                if (custom != null && datPath.Equals(Path.Combine(custom, DAT_NAME), StringComparison.OrdinalIgnoreCase))
                {
                    //this dat file is using a custom userprofile
                    if (account.DatFile.IsInitialized && !File.Exists(account.DatFile.Path))
                        account.DatFile.IsInitialized = false;
                    return true;
                }

                string exeName = Path.GetFileName(Settings.GW2Path.Value);

                if (!Directory.Exists(appdata))
                {
                    try
                    {
                        new DirectoryInfo(appdata).Create();
                    }
                    catch (Exception ex)
                    {
                        Util.Logging.Log(ex);
                    }
                }

                bool inUse = false;

                //find if the default Local.dat is being used
                foreach (var fid in Settings.DatFiles.GetKeys())
                {
                    var dat = Settings.DatFiles[fid];
                    var path = GetPath(dat.Value);

                    if (path.Equals(localDat, StringComparison.OrdinalIgnoreCase))
                    {
                        inUse = true;
                        break;
                    }
                }

                if (inUse && custom == null)
                {
                    throw new Exception("Unknown user directory structure");
                }

                if (account.DatFile == null)
                {
                    //this account has no existing dat file

                    if (inUse)
                    {
                        //a custom userprofile will be used
                        account.DatFile = Settings.CreateDatFile();
                        custom = string.Format(custom, account.DatFile.UID);

                        try
                        {
                            CreateCustomUserprofile(account.DatFile.UID);
                        }
                        catch (Exception e)
                        {
                            Util.Logging.Log(e);
                            throw new Exception("Unable to create profile directory", e);
                        }

                        account.DatFile.Path = Path.Combine(custom, DAT_NAME);

                        return true;
                    }
                    else
                    {
                        //the default Local.dat will be used
                        account.DatFile = Settings.CreateDatFile();
                        account.DatFile.Path = Path.Combine(appdata, DAT_NAME);

                        return false;
                    }
                }
                else
                {
                    //move to a custom userprofile if inUse=true

                    if (inUse)
                    {
                        try
                        {
                            CreateCustomUserprofile(account.DatFile.UID);
                        }
                        catch (Exception e)
                        {
                            Util.Logging.Log(e);
                            throw new Exception("Unable to create profile directory", e);
                        }

                        localDat = Path.Combine(custom, DAT_NAME);
                    }

                    try
                    {
                        Move(datPath, localDat);
                        account.DatFile.Path = localDat;
                    }
                    catch (Exception e)
                    {
                        Util.Logging.Log(e);
                        throw new Exception("Unable to move the current Local.dat file", e);
                    }

                    try
                    {
                        string path = Path.Combine(Path.GetDirectoryName(datPath), string.Format(GFX_NAME, exeName + "." + account.DatFile.UID));
                        if (!File.Exists(path))
                        {
                            if (datPath.EndsWith("Local.dat", StringComparison.OrdinalIgnoreCase))
                                path = Path.Combine(Path.GetDirectoryName(datPath), string.Format(GFX_NAME, exeName));
                        }
                        Move(path, Path.Combine(Path.GetDirectoryName(localDat), string.Format(GFX_NAME, exeName)));
                    }
                    catch (Exception ex)
                    {
                        Util.Logging.Log(ex);
                    }

                    if (account.DatFile.IsInitialized && !File.Exists(account.DatFile.Path))
                        account.DatFile.IsInitialized = false;

                    return inUse;
                }
            }
            finally
            {
                if (impersonation != null)
                    impersonation.Dispose();
            }
        }

        #region Activate by moving Local.dat > Local.#.dat
        //public static void Activate(Settings.IAccount account)
        //{
        //    Security.Impersonation.IImpersonationToken impersonation;
        //    string username = Util.Users.GetUserName(account.WindowsAccount);

        //    if (Util.Users.IsCurrentUser(username))
        //        impersonation = null;
        //    else
        //        impersonation = Security.Impersonation.Impersonate(username, Security.Credentials.GetPassword(username));

        //    try
        //    {
        //        //appdata is the primary location, however if a Local.dat file exists under
        //        //the documents folder, it will take priority. If one was created under the
        //        //documents folder, it means that the Local.dat file that was under appdata
        //        //was corrupt
        //        string appdata = GetFolder(DatFolder.AppData);
        //        string documents = GetFolder(DatFolder.Documents);
        //        string localDat;

        //        if (!File.Exists(localDat = Path.Combine(documents, DAT_NAME)))
        //        {
        //            //documents will be ignored
        //            documents = null;
        //            localDat = Path.Combine(appdata, DAT_NAME);
        //        }
                
        //        string datPath = GetPath(account.DatFile);

        //        if (datPath.Equals(localDat, StringComparison.OrdinalIgnoreCase)
        //            || documents != null && datPath.Equals(Path.Combine(appdata, DAT_NAME), StringComparison.OrdinalIgnoreCase))
        //        {
        //                //this dat file is already Local.dat
        //                return;
        //        }

        //        string gfxName = "GFXSettings." + Path.GetFileName(Settings.GW2Path.Value);
        //        string gfxExt = ".xml";

        //        if (!Directory.Exists(appdata))
        //        {
        //            try
        //            {
        //                Directory.CreateDirectory(appdata);
        //            }
        //            catch { }
        //        }

        //        foreach (var fid in Settings.DatFiles.GetKeys())
        //        {
        //            var dat = Settings.DatFiles[fid];
        //            var path = GetPath(dat.Value);

        //            if (path.Equals(localDat, StringComparison.OrdinalIgnoreCase)
        //                || documents != null && path.Equals(Path.Combine(appdata, DAT_NAME), StringComparison.OrdinalIgnoreCase))
        //            {
        //                try
        //                {
        //                    string pathTo = GetPath(appdata, fid);

        //                    Move(localDat, pathTo);

        //                    try
        //                    {
        //                        Move(Path.Combine(Path.GetDirectoryName(path), gfxName + gfxExt), Path.Combine(appdata, gfxName + "." + fid + gfxExt));
        //                    }
        //                    catch { }

        //                    dat.Value.Path = pathTo;
        //                }
        //                catch (Exception e)
        //                {
        //                    throw new Exception("Unable to move the current Local.dat file", e);
        //                }

        //                break;
        //            }
        //        }

        //        if (File.Exists(localDat))
        //        {
        //            try
        //            {
        //                File.Delete(localDat);
        //            }
        //            catch (Exception e)
        //            {
        //                throw new Exception("Unable to delete the current Local.dat file", e);
        //            }
        //        }

        //        if (documents != null)
        //        {
        //            string path = Path.Combine(appdata, DAT_NAME);

        //            if (File.Exists(path))
        //            {
        //                try
        //                {
        //                    File.Delete(path);
        //                }
        //                catch (Exception e)
        //                {
        //                    throw new Exception("Unable to delete the current Local.dat file", e);
        //                }
        //            }
        //        }

        //        if (account.DatFile == null)
        //        {
        //            //this account has no existing dat file
        //            //the new Local.dat will be used
        //            account.DatFile = Settings.CreateDatFile();
        //            account.DatFile.Path = Path.Combine(appdata, DAT_NAME);
        //        }
        //        else
        //        {
        //            string gfxSettings = Path.Combine(appdata, gfxName + gfxExt);

        //            if (File.Exists(gfxSettings))
        //            {
        //                try
        //                {
        //                    File.Delete(gfxSettings);
        //                }
        //                catch { }
        //            }

        //            if (documents != null)
        //                localDat = Path.Combine(appdata, DAT_NAME);

        //            try
        //            {
        //                if (File.Exists(datPath))
        //                    File.Move(datPath, localDat);
        //                account.DatFile.Path = localDat;
        //            }
        //            catch (Exception e)
        //            {
        //                throw new Exception("Unable to move the current Local.dat file", e);
        //            }

        //            string gfxFrom = Path.Combine(Path.GetDirectoryName(datPath), gfxName + "." + account.DatFile.UID + gfxExt);

        //            try
        //            {
        //                Move(gfxFrom, gfxSettings);
        //            }
        //            catch { }

        //            var t = DateTime.UtcNow.AddSeconds(3);
        //            while (true)
        //            {
        //                try
        //                {
        //                    using (var f = File.Open(localDat, FileMode.Open, FileAccess.Read, FileShare.None))
        //                    {
        //                        break;
        //                    }
        //                }
        //                catch (Exception e)
        //                {
        //                }

        //                System.Threading.Thread.Sleep(100);
        //                if (DateTime.UtcNow > t)
        //                    return;
        //            }
        //        }
        //    }
        //    finally
        //    {
        //        if (impersonation != null)
        //            impersonation.Dispose();
        //    }
        //}
        #endregion
    }
}
