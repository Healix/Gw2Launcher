using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace Gw2Launcher.Client
{
    public static class FileManager
    {
        public const string LOCALIZED_EXE_FOLDER_NAME = "Gw2Launcher";

        private const string DAT_NAME = "Local.dat";
        private const string GFX_NAME = "GFXSettings.{0}.xml";
        private const string ALT_DATA = "alt";
        private const string SCREENS_FOLDER_NAME = "Screens";
        private const string MUSIC_FOLDER_NAME = "Music";
        private const string COHERENT_DUMPS_FOLDER_NAME = "Coherent Dumps";

        private static IsSupportedState isSupported;
        private static byte exebits;

        public enum SpecialPath
        {
            AppData,
            Documents,
            Screens,
            Music,
            Dumps,
        }

        enum DatFolder
        {
            AppData,
            Documents
        }

        enum PathType
        {
            Unknown,
            /// <summary>
            /// Default path for a normal installation
            /// </summary>
            Default,
            /// <summary>
            /// Within the user's directory
            /// </summary>
            CurrentUser,
            /// <summary>
            /// Within a different user's directory
            /// </summary>
            DifferentUser,
            /// <summary>
            /// Not within a user's directory
            /// </summary>
            Other,

            /// <summary>
            /// Matches the custom path that was supplied
            /// </summary>
            Custom,
            /// <summary>
            /// Old version, data stored by Dat ID within GW2 folder
            /// </summary>
            DataByGw2,
            /// <summary>
            /// Data stored by Dat ID within alt folder
            /// </summary>
            DataByDat,
            /// <summary>
            /// Data stored by Account ID within data folder
            /// </summary>
            DataByAccount,
        }

        public enum FileType
        {
            Dat,
            Gfx,
        }

        private enum IsSupportedState : byte
        {
            DataTested = 1,
            DataSupported = 2,
            Gw2Tested = 4,
            Gw2Supported = 8,
        }

        public interface IProfileInformation
        {
            string UserProfile
            {
                get;
            }

            string AppData
            {
                get;
            }
        }

        private class PathData : IProfileInformation
        {
            public string
                accountdata,
                userprofile, 
                appdata, 
                documents,
                gfxName,
 
                profileAppData, 
                profileUserProfile;

            public PathData()
            {
                accountdata = DataPath.AppDataAccountData;
                userprofile = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile, Environment.SpecialFolderOption.Create);
                appdata = GetFolder(DatFolder.AppData);
                documents = GetFolder(DatFolder.Documents);

                string exe;
                if (!string.IsNullOrEmpty(Settings.GW2Path.Value))
                    exe = Path.GetFileName(Settings.GW2Path.Value);
                else if (Environment.Is64BitOperatingSystem)
                    exe = "Gw2-64.exe";
                else
                    exe = "Gw2.exe";
                gfxName = string.Format(GFX_NAME, exe);
            }

            public string GetCustomPath(FileType type, Settings.IAccount account, bool link)
            {
                switch (type)
                {
                    case FileType.Dat:

                        if (IsDataLinkingSupported)
                        {
                            if (link)
                                return Path.Combine(accountdata, account.DatFile.UID.ToString(), appdata.Substring(userprofile.Length + 1), DAT_NAME);
                            else
                                return Path.Combine(accountdata, account.DatFile.UID + ".dat");
                        }
                        else
                            return Path.Combine(accountdata, ALT_DATA, account.DatFile.UID.ToString(), appdata.Substring(userprofile.Length + 1), DAT_NAME);

                    case FileType.Gfx:

                        if (IsDataLinkingSupported)
                        {
                            if (link)
                                return Path.Combine(accountdata, account.GfxFile.UID.ToString(), appdata.Substring(userprofile.Length + 1), gfxName);
                            else
                                return Path.Combine(accountdata, account.GfxFile.UID + ".xml");
                        }
                        else
                            return Path.Combine(accountdata, ALT_DATA, account.DatFile.UID.ToString(), appdata.Substring(userprofile.Length + 1), gfxName);

                }

                return null;
            }
            
            public PathType GetPathType(string path, string defaultName)
            {
                return GetPathType(path, null, defaultName);
            }

            public PathType GetPathType(string path, string customPath, string defaultName)
            {
                if (string.IsNullOrEmpty(path))
                {
                    return PathType.Default;
                }
                else if (customPath != null && path.Equals(customPath, StringComparison.OrdinalIgnoreCase))
                {
                    return PathType.Custom;
                }
                else if (path.StartsWith(accountdata, StringComparison.OrdinalIgnoreCase))
                {
                    int i, j;

                    i = accountdata.Length + 1;
                    j = path.IndexOf(Path.DirectorySeparatorChar, i);

                    if (j == -1)
                    {
                        j = path.IndexOf('.', i);
                        if (j != -1)
                            return PathType.DataByAccount;
                        j = path.Length;
                    }

                    if (char.IsDigit(path[i]))
                    {
                        var k = i + 1;
                        for (; k < j; k++)
                        {
                            if (!char.IsDigit(path[k]))
                                break;
                        }
                        if (k == j)
                            return PathType.DataByAccount;
                    }

                    var s = path.Substring(i, j - i);

                    if (s.Equals(ALT_DATA, StringComparison.OrdinalIgnoreCase))
                        return PathType.DataByDat;

                    return PathType.CurrentUser;
                }
                else
                {
                    if (path.StartsWith(appdata, StringComparison.OrdinalIgnoreCase))
                    {
                        if (path.EndsWith(defaultName, StringComparison.OrdinalIgnoreCase) && path.Length == appdata.Length + defaultName.Length + 1)
                        {
                            return PathType.Default;
                        }
                        else
                        {
                            var i = appdata.Length + 1;
                            var j = path.IndexOf(Path.DirectorySeparatorChar, i);

                            if (j != -1 && int.TryParse(path.Substring(i, j - i), out i))
                            {
                                return PathType.DataByGw2;
                            }
                            else
                            {
                                return PathType.CurrentUser;
                            }
                        }
                    }
                    else
                    {
                        if (path.StartsWith(userprofile, StringComparison.OrdinalIgnoreCase))
                        {
                            return PathType.CurrentUser;
                        }
                        else
                        {
                            var users = Path.GetDirectoryName(userprofile);
                            if (path.StartsWith(users, StringComparison.OrdinalIgnoreCase))
                            {
                                var i = users.Length + 1;
                                var j = path.IndexOf(Path.DirectorySeparatorChar, i);

                                if (j != -1)
                                {
                                    //string username = path.Substring(i, j-i);
                                    return PathType.DifferentUser;
                                }
                            }

                            return PathType.Other;
                        }
                    }
                }
            }

            public string GetUsername(string path)
            {
                var root = Path.GetDirectoryName(userprofile);
                if (path.StartsWith(root, StringComparison.OrdinalIgnoreCase))
                {
                    var i = root.Length + 1;
                    var j = path.IndexOf(Path.DirectorySeparatorChar, i);
                    if (j != -1)
                    {
                        return path.Substring(i, j - i);
                    }
                }
                return null;
            }

            public bool IsDocumentsInUserFolder
            {
                get
                {
                    return documents.StartsWith(userprofile, StringComparison.OrdinalIgnoreCase);
                }
            }

            public bool IsAppDataInUserFolder
            {
                get
                {
                    return appdata.StartsWith(userprofile, StringComparison.OrdinalIgnoreCase);
                }
            }

            /// <summary>
            /// The resulting AppData location for the custom profile
            /// </summary>
            public string AppData
            {
                get
                {
                    return profileAppData;
                }
            }

            /// <summary>
            /// The resulting UserProfile location for the custom profile
            /// </summary>
            public string UserProfile
            {
                get
                {
                    return profileUserProfile;
                }
            }
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

        static FileManager()
        {
            //linking is only supported on NTFS

            try
            {
                var path = DataPath.AppData;
                IsDataLinkingSupported = IsPathSupported(path, true);
            }
            catch (Exception e)
            {
                Util.Logging.Log(e);
            }

            DoPendingLocalizeAccountExecution();

            Settings.GW2Path.ValueChanged += GW2Path_ValueChanged;
            Settings.ScreenshotsLocation.ValueChanged += Setting_ValueChanged;
            Settings.LocalizeAccountExecution.ValueChanged += LocalizeAccountExecution_ValueChanged;

            Client.Launcher.ActiveProcessCountChanged += Launcher_ActiveProcessCountChanged;
        }

        static void Launcher_ActiveProcessCountChanged(object sender, int e)
        {
            if (e == 0)
                DoPendingLocalizeAccountExecution();
        }

        private static void DoPendingLocalizeAccountExecution()
        {
            try
            {
                var v = Settings.LocalizeAccountExecution;
                if (v.IsPending && !v.Value)
                {
                    if (DeleteExecutableRoot())
                        v.Commit();
                }
            }
            catch { }
        }

        /// <summary>
        /// Tests if the path supports hard links
        /// </summary>
        /// <param name="path">The path to test</param>
        /// <param name="test">If true, a link will be created to confirm</param>
        /// <returns>True if the drive of the path supports linking</returns>
        public static bool IsPathSupported(string path, bool test)
        {
            bool isSupported;
            try
            {
                isSupported = path[1] == ':' && new DriveInfo(path.Substring(0, 1)).DriveFormat.Equals("NTFS", StringComparison.OrdinalIgnoreCase);
            }
            catch (Exception e)
            {
                Util.Logging.Log(e);
                isSupported = false;
            }

            if (!isSupported && test)
            {
                var l = Path.Combine(path, "l");
                var a = l + ".a";
                var b = l + ".b";

                try
                {
                    File.WriteAllBytes(a, new byte[0]);
                    Windows.Symlink.CreateHardLink(b, a);
                    isSupported = true;
                }
                catch (Exception e)
                {
                    Util.Logging.Log(e);
                    isSupported = false;
                }
                finally
                {
                    if (Delete(a))
                        Delete(b);
                }
            }

            return isSupported;
        }
        
        static void LocalizeAccountExecution_ValueChanged(object sender, EventArgs e)
        {
            DoPendingLocalizeAccountExecution();
        }

        static void Setting_ValueChanged(object sender, EventArgs e)
        {
            FlagAllAccountForPendingFiles();
        }

        static void GW2Path_ValueChanged(object sender, EventArgs e)
        {
            isSupported &= ~(IsSupportedState.Gw2Tested | IsSupportedState.Gw2Supported);
            exebits = 0;
            FlagAllAccountForPendingFiles();
        }

        public static bool IsDataLinkingSupported
        {
            get
            {
                return (isSupported & IsSupportedState.DataSupported) == IsSupportedState.DataSupported;
            }
            private set
            {
                if (value)
                    isSupported |= IsSupportedState.DataSupported;
                else
                    isSupported &= ~IsSupportedState.DataSupported;
            }
        }

        public static bool IsGw2LinkingSupported
        {
            get
            {
                if ((isSupported & IsSupportedState.Gw2Tested) != IsSupportedState.Gw2Tested)
                {
                    var path = Settings.GW2Path.Value;
                    if (!string.IsNullOrEmpty(path))
                    {
                        IsGw2LinkingSupported = IsPathSupported(path, false);
                    }
                    else
                        return false;
                }
                return (isSupported & IsSupportedState.Gw2Supported) == IsSupportedState.Gw2Supported;
            }
            private set
            {
                if (value)
                    isSupported |= IsSupportedState.Gw2Supported | IsSupportedState.Gw2Tested;
                else
                    isSupported = (isSupported & ~IsSupportedState.Gw2Supported) | IsSupportedState.Gw2Tested;
            }
        }

        private static void FlagAllAccountForPendingFiles()
        {
            foreach (var uid in Settings.Accounts.GetKeys())
            {
                var a = Settings.Accounts[uid];
                if (a.HasValue)
                    a.Value.PendingFiles = true;
            }
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

        private static bool IsCustomized(Settings.IAccount account)
        {
            return Settings.ScreenshotsLocation.HasValue || IsDataLinkingSupported && (!string.IsNullOrEmpty(account.ScreenshotsLocation) || Settings.VirtualUserPath.HasValue);
        }

        private static bool Move(string from, string to)
        {
            if (!File.Exists(from))
                return false;
            if (File.Exists(to))
                File.Delete(to);
            File.Move(from, to);
            return true;
        }

        private static bool CreateProfile(Settings.IAccount account, PathData pd)
        {
            string path;
            if (IsDataLinkingSupported)
                path = Path.Combine(pd.accountdata, account.UID.ToString());
            else
                path = Path.Combine(pd.accountdata, ALT_DATA, account.DatFile.UID.ToString());

            if (!Directory.Exists(path))
            {
                if (pd.IsAppDataInUserFolder)
                {
                    new DirectoryInfo(Path.Combine(path, pd.appdata.Substring(pd.userprofile.Length + 1))).Create();

                    return true;
                }
            }

            return false;
        }

        private static bool CreateProfileOrThrow(Settings.IAccount account, PathData pd)
        {
            try
            {
                return CreateProfile(account, pd);
            }
            catch
            {
                throw new Exception("Unable to create profile directory");
            }
        }

        private static void UpdateProfile(Settings.IAccount account, PathData pd)
        {
            //note: added links must be mirrored in DeleteProfile
            
            string path;
            if (IsDataLinkingSupported)
                path = Path.Combine(pd.accountdata, account.UID.ToString());
            else
                path = Path.Combine(pd.accountdata, ALT_DATA, account.DatFile.UID.ToString());

            var isCurrentUser = Util.Users.IsCurrentUser(account.WindowsAccount);
            if (!isCurrentUser) //different users will need permision to access GW2's appdata and documents
            {
                Util.FileUtil.AllowFolderAccess(pd.appdata, System.Security.AccessControl.FileSystemRights.Modify);
                Util.FileUtil.AllowFolderAccess(DataPath.AppDataAccountData, System.Security.AccessControl.FileSystemRights.Modify);
            }

            if (pd.IsAppDataInUserFolder)
            {
                //appdata is now the custom profile's appdata/guild wars 2
                var appdata = Path.Combine(path, pd.appdata.Substring(pd.userprofile.Length + 1));
                new DirectoryInfo(appdata).Create();

                #region Local.dat

                if (account.DatFile != null && !string.IsNullOrEmpty(account.DatFile.Path))
                {
                    var dat = account.DatFile.Path;
                    var fi = new FileInfo(Path.Combine(appdata, DAT_NAME));

                    if (!fi.FullName.Equals(dat, StringComparison.OrdinalIgnoreCase))
                    {
                        if (fi.Exists)
                            fi.Delete();

                        if (IsDataLinkingSupported)
                        {
                            if (!File.Exists(dat))
                            {
                                account.DatFile.IsInitialized = false;
                                File.WriteAllBytes(dat, new byte[0]);
                            }
                            Windows.Symlink.CreateHardLink(fi.FullName, dat);
                        }
                        else
                        {
                            if (!Move(dat, fi.FullName))
                                account.DatFile.IsInitialized = false;
                            account.DatFile.Path = fi.FullName;
                        }
                    }
                }

                #endregion

                #region GFXSettings.xml

                //while setting up unknown DatFile, if GfxFile is null, check if GfxFile is in same folder and if anyone owns it -- take ownership

                if (account.GfxFile != null && !string.IsNullOrEmpty(account.GfxFile.Path))
                {
                    var gfx = account.GfxFile.Path;
                    var fi = new FileInfo(Path.Combine(appdata, pd.gfxName));

                    if (!fi.FullName.Equals(gfx, StringComparison.OrdinalIgnoreCase))
                    {
                        if (fi.Exists)
                            fi.Delete();

                        if (IsDataLinkingSupported)
                        {
                            if (!File.Exists(gfx))
                            {
                                account.GfxFile.IsInitialized = false;
                                File.WriteAllBytes(gfx, new byte[0]);
                            }
                            Windows.Symlink.CreateHardLink(fi.FullName, gfx);
                        }
                        else
                        {
                            if (!Move(gfx, fi.FullName))
                                account.GfxFile.IsInitialized = false;
                            account.GfxFile.Path = fi.FullName;
                        }
                    }
                }

                #endregion

                if (IsDataLinkingSupported)
                {
                    #region Link Coherent Dumps

                    var diDumps = new DirectoryInfo(Path.Combine(appdata, COHERENT_DUMPS_FOLDER_NAME));
                    var create = true;

                    if (diDumps.Exists)
                    {
                        if (diDumps.Attributes.HasFlag(FileAttributes.ReparsePoint))
                        {
                            create = false;
                        }
                        else
                        {
                            diDumps.Delete(true);
                        }
                    }

                    if (create)
                    {
                        try
                        {
                            var diTo = new DirectoryInfo(Path.Combine(pd.appdata, COHERENT_DUMPS_FOLDER_NAME));
                            if (!diTo.Exists)
                                diTo.Create();
                            Windows.Symlink.CreateJunction(diDumps.FullName, diTo.FullName);
                        }
                        catch (Exception e)
                        {
                            Util.Logging.Log(e);
                        }
                    }

                    #endregion

                    #region Link LocalAppData

                    var localappdata = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData, Environment.SpecialFolderOption.DoNotVerify);
                    if (localappdata.StartsWith(pd.userprofile, StringComparison.OrdinalIgnoreCase))
                    {
                        var diLocal = new DirectoryInfo(Path.Combine(path, localappdata.Substring(pd.userprofile.Length + 1)));
                        create = true;

                        if (diLocal.Exists)
                        {
                            if (diLocal.Attributes.HasFlag(FileAttributes.ReparsePoint))
                            {
                                create = false;
                            }
                            else
                            {
                                diLocal.Delete(true);
                            }
                        }

                        if (create)
                        {
                            try
                            {
                                var diTo = new DirectoryInfo(localappdata);
                                if (!diTo.Exists)
                                    diTo.Create();
                                Windows.Symlink.CreateJunction(diLocal.FullName, diTo.FullName);
                            }
                            catch (Exception e)
                            {
                                Util.Logging.Log(e);
                            }
                        }
                    }

                    #endregion
                }

                if (pd.IsDocumentsInUserFolder)
                {
                    if (IsDataLinkingSupported)
                    {
                        #region Link Documents or subfolders

                        var diScreens = new DirectoryInfo(Path.Combine(path, pd.documents.Substring(pd.userprofile.Length + 1), SCREENS_FOLDER_NAME));
                        var diMusic = new DirectoryInfo(Path.Combine(diScreens.Parent.FullName, MUSIC_FOLDER_NAME));
                        var diDocs = diScreens.Parent.Parent;

                        string linkTo;
                        var isCustom = !string.IsNullOrEmpty(linkTo = account.ScreenshotsLocation) || !string.IsNullOrEmpty(linkTo = Settings.ScreenshotsLocation.Value);

                        if (!isCustom && !isCurrentUser)
                        {
                            Util.FileUtil.AllowFolderAccess(pd.documents, System.Security.AccessControl.FileSystemRights.Modify);
                            linkTo = Path.Combine(pd.documents, SCREENS_FOLDER_NAME);
                            isCustom = true;
                        }

                        if (diDocs.Exists)
                        {
                            //the documents folder should either completely link to the real documents
                            //or only include the guild wars 2 folder, which has screens linking elsewhere
                            var deleteDocs = true;

                            if (diDocs.Attributes.HasFlag(FileAttributes.ReparsePoint))
                            {
                                diDocs.Delete();
                                deleteDocs = false;
                            }
                            else if (diScreens.Exists)
                            {
                                if (diScreens.Attributes.HasFlag(FileAttributes.ReparsePoint))
                                {
                                    diScreens.Delete();
                                }
                                else
                                {
                                    //this is a real folder - only delete if it's empty
                                    // -- could move the files to the new path
                                    try
                                    {
                                        diScreens.Delete();
                                    }
                                    catch (Exception e)
                                    {
                                        deleteDocs = false;
                                        Util.Logging.Log(e);
                                    }
                                }
                            }

                            if (deleteDocs && !isCustom)
                            {
                                if (diMusic.Exists)
                                    diMusic.Delete();
                                diDocs.Delete(true);
                            }
                        }

                        try
                        {
                            if (isCustom)
                            {
                                diScreens.Parent.Create();

                                var diTo = new DirectoryInfo(linkTo);
                                if (!diTo.Exists)
                                    diTo.Create();
                                Windows.Symlink.CreateJunction(diScreens.FullName, diTo.FullName);

                                if (!isCurrentUser && !linkTo.StartsWith(pd.documents, StringComparison.OrdinalIgnoreCase))
                                    Util.FileUtil.AllowFolderAccess(diTo.FullName, System.Security.AccessControl.FileSystemRights.Modify);

                                if (!diMusic.Exists)
                                {
                                    diTo = new DirectoryInfo(Path.Combine(pd.documents, MUSIC_FOLDER_NAME));
                                    if (diTo.Exists)
                                        Windows.Symlink.CreateJunction(diMusic.FullName, diTo.FullName);
                                }
                            }
                            else
                            {
                                Windows.Symlink.CreateJunction(diDocs.FullName, Path.GetDirectoryName(pd.documents));
                            }
                        }
                        catch (Exception ex)
                        {
                            Util.Logging.Log(ex);
                        }

                        #endregion
                    }
                    else
                    {
                        //create the gw2 documents folder
                        new DirectoryInfo(Path.Combine(path, pd.documents.Substring(pd.userprofile.Length + 1))).Create();
                    }
                }
            }
            else
            {
                throw new Exception("Unknown user profile structure");
            }
        }

        private static void OnFilePathMoved(FileType type, Settings.IFile file)
        {
            if (file.References <= 1)
                return;

            foreach (var a in FindAccounts(type, file))
            {
                a.PendingFiles = true;
            }
        }

        /// <summary>
        /// Returns the path of the specified type
        /// </summary>
        /// <param name="type">The path to retrieve</param>
        /// <param name="account">The account or null to use the default path</param>
        public static string GetPath(SpecialPath type, Settings.IAccount account)
        {
            return GetPath(type, account, new PathData());
        }

        /// <summary>
        /// Return the default path of the specified type
        /// </summary>
        /// <param name="type">The path to retrieve</param>
        public static string GetPath(SpecialPath type)
        {
            return GetPath(type, null, new PathData());
        }

        private static string GetPath(SpecialPath type, Settings.IAccount account, PathData pd)
        {
            switch (type)
            {
                case SpecialPath.AppData:

                    if (account == null)
                        return pd.appdata;
                    if (IsDataLinkingSupported)
                        return Path.Combine(pd.accountdata, account.UID.ToString(), pd.appdata.Substring(pd.userprofile.Length + 1));
                    else
                        return Path.Combine(pd.accountdata, ALT_DATA, account.DatFile.UID.ToString(), pd.appdata.Substring(pd.userprofile.Length + 1));

                case SpecialPath.Dumps:

                    if (account == null)
                        return Path.Combine(pd.appdata, COHERENT_DUMPS_FOLDER_NAME);
                    if (IsDataLinkingSupported)
                        return Path.Combine(pd.accountdata, account.UID.ToString(), pd.appdata.Substring(pd.userprofile.Length + 1), COHERENT_DUMPS_FOLDER_NAME);
                    else
                        return Path.Combine(pd.accountdata, ALT_DATA, account.DatFile.UID.ToString(), pd.appdata.Substring(pd.userprofile.Length + 1), COHERENT_DUMPS_FOLDER_NAME);

                case SpecialPath.Documents:

                    if (account == null)
                        return pd.documents;
                    if (IsDataLinkingSupported)
                        return Path.Combine(pd.accountdata, account.UID.ToString(), pd.documents.Substring(pd.userprofile.Length + 1));
                    else
                        return Path.Combine(pd.accountdata, ALT_DATA, account.DatFile.UID.ToString(), pd.documents.Substring(pd.userprofile.Length + 1));

                case SpecialPath.Music:

                    if (account == null)
                        return Path.Combine(pd.documents, MUSIC_FOLDER_NAME);
                    if (IsDataLinkingSupported)
                        return Path.Combine(pd.accountdata, account.UID.ToString(), pd.documents.Substring(pd.userprofile.Length + 1), MUSIC_FOLDER_NAME);
                    else
                        return Path.Combine(pd.accountdata, ALT_DATA, account.DatFile.UID.ToString(), pd.documents.Substring(pd.userprofile.Length + 1), MUSIC_FOLDER_NAME);

                case SpecialPath.Screens:

                    if (account == null)
                        return Path.Combine(pd.documents, SCREENS_FOLDER_NAME);
                    if (IsDataLinkingSupported)
                        return Path.Combine(pd.accountdata, account.UID.ToString(), pd.documents.Substring(pd.userprofile.Length + 1), SCREENS_FOLDER_NAME);
                    else
                        return Path.Combine(pd.accountdata, ALT_DATA, account.DatFile.UID.ToString(), pd.documents.Substring(pd.userprofile.Length + 1), SCREENS_FOLDER_NAME);
            }

            return null;
        }

        public static string GetDefaultPath(FileType type)
        {
            var pd = new PathData();

            switch (type)
            {
                case FileType.Dat:

                    return Path.Combine(pd.appdata, DAT_NAME);

                case FileType.Gfx:

                    return Path.Combine(pd.appdata, pd.gfxName);
            }

            return null;
        }

        /// <summary>
        /// Returns all files of the given type
        /// </summary>
        /// <param name="type">The type of file</param>
        public static IEnumerable<Settings.IFile> GetFiles(FileType type)
        {
            switch (type)
            {
                case FileType.Dat:

                    var dat = Settings.DatFiles;
                    foreach (var fid in dat.GetKeys())
                    {
                        var f = dat[fid].Value;
                        if (f != null)
                            yield return f;
                    }

                    break;
                case FileType.Gfx:

                    var gfx = Settings.GfxFiles;
                    foreach (var fid in gfx.GetKeys())
                    {
                        var f = gfx[fid].Value;
                        if (f != null)
                            yield return f;
                    }

                    break;
            }
        }

        /// <summary>
        /// Finds an existing file with the path
        /// </summary>
        /// <param name="type">The type of file</param>
        /// <param name="path">The path to search for</param>
        public static Settings.IFile FindFile(FileType type, string path)
        {
            if (path == "*")
            {
                foreach (var file in GetFiles(type))
                {
                    if (file != null && !string.IsNullOrEmpty(file.Path) && File.Exists(file.Path))
                    {
                        return file;
                    }
                }

                return null;
            }

            foreach (var file in GetFiles(type))
            {
                if (file != null && path.Equals(file.Path, StringComparison.OrdinalIgnoreCase))
                {
                    return file;
                }
            }

            return null;
        }

        /// <summary>
        /// Finds all accounts that are using the file
        /// </summary>
        /// <param name="type">The type of file</param>
        /// <param name="file">The file to search for</param>
        public static IEnumerable<Settings.IAccount> FindAccounts(FileType type, Settings.IFile file)
        {
            var count = file.References;

            if (count > 0)
            {
                foreach (var uid in Settings.Accounts.GetKeys())
                {
                    var a = Settings.Accounts[uid].Value;
                    if (a != null)
                    {
                        Settings.IFile _file;

                        switch (type)
                        {
                            case FileType.Dat:

                                _file = a.DatFile;

                                break;
                            case FileType.Gfx:

                                _file = a.GfxFile;

                                break;
                            default:

                                _file = null;

                                break;
                        }

                        if (_file != null && _file == file)
                        {
                            yield return a;

                            if (--count == 0)
                                break;
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Deletes the dat file and profile if applicable
        /// </summary>
        /// <param name="file">The file to delete</param>
        public static void Delete(Settings.IDatFile file)
        {
            var path = file.Path;
            if (string.IsNullOrEmpty(path) || !File.Exists(path))
                return;

            Delete(FileType.Dat, path, new PathData());
        }
        
        /// <summary>
        /// Deletes the gfx file
        /// </summary>
        /// <param name="file">The file to delete</param>
        public static void Delete(Settings.IGfxFile file)
        {
            var path = file.Path;
            if (string.IsNullOrEmpty(path) || !File.Exists(path))
                return;

            Delete(FileType.Gfx, file.Path, new PathData());
        }

        private static void Delete(FileType type, string path, PathData pd)
        {
            switch (type)
            {
                case FileType.Dat:

                    var datType = pd.GetPathType(path, DAT_NAME);

                    switch (datType)
                    {
                        case PathType.DataByDat:
                        case PathType.DataByGw2:

                            DeleteProfile(datType, path, pd);

                            return;

                        case PathType.DifferentUser:

                            var username = pd.GetUsername(path);
                            var password = Security.Credentials.GetPassword(username);
                            if (password != null)
                            {
                                using (var impersonation = Security.Impersonation.Impersonate(username, Security.Credentials.GetPassword(username)))
                                {
                                    Delete(type, path, new PathData());
                                    return;
                                }
                            }

                            break;

                        case PathType.Default:

                            //not deleting the default file
                            return;
                    }

                    break;
                case FileType.Gfx:

                    var gfxType = pd.GetPathType(path, pd.gfxName);

                    switch (gfxType)
                    {
                        case PathType.Default:

                            //not deleting the default file
                            return;
                    }

                    break;

                default:

                    return;
            }

            Delete(path);
        }

        /// <summary>
        /// Deletes the account's profile
        /// </summary>
        /// <param name="file">The account to delete</param>
        public static void Delete(Settings.IAccount account)
        {
            var pd = new PathData();

            if (account.DatFile != null)
            {
                if (account.DatFile.References == 1)
                {
                    Delete(FileType.Dat, account.DatFile.Path, pd);
                    Settings.DatFiles[account.DatFile.UID].Clear();
                }
                account.DatFile = null;
            }

            if (account.GfxFile != null)
            {
                if (account.GfxFile.References == 1)
                {
                    Delete(FileType.Gfx, account.GfxFile.Path, pd);
                    Settings.GfxFiles[account.GfxFile.UID].Clear();
                }
                account.GfxFile = null;
            }

            if (Settings.LocalizeAccountExecution.Value)
                DeleteExecutable(account.UID);

            if (IsDataLinkingSupported)
            {
                var path = Path.Combine(pd.accountdata, account.UID.ToString());
                if (Directory.Exists(path))
                    DeleteProfile(PathType.DataByAccount, account.UID, pd);
            }
        }

        private static bool Delete(string path)
        {
            if (string.IsNullOrEmpty(path) || !File.Exists(path))
                return true;

            try
            {
                File.Delete(path);
                return true;
            }
            catch (Exception e)
            {
                Util.Logging.Log(e);
            }
            return false;
        }

        /// <summary>
        /// Deletes the profile directory
        /// </summary>
        /// <param name="type">The profile type</param>
        /// <param name="path">Any path in the profile's directory</param>
        /// <param name="pd">Path data</param>
        /// <returns>If the directory is deleted</returns>
        private static bool DeleteProfile(PathType type, string path, PathData pd)
        {
            string root;

            if (type == PathType.Custom)
            {
                if (IsDataLinkingSupported)
                    type = PathType.DataByAccount;
                else
                    type = PathType.DataByDat;
            }

            switch (type)
            {
                case PathType.DataByDat:

                    root = Path.Combine(pd.accountdata, ALT_DATA);

                    break;
                case PathType.DataByAccount:

                    root = pd.accountdata;

                    break;
                case PathType.DataByGw2:

                    root = pd.appdata;

                    break;
                default:

                    return false;
            }

            var i = root.Length + 1;
            var j = path.IndexOf(Path.DirectorySeparatorChar, i);

            if (j == -1)
                j = path.Length;

            ushort uid;
            if (j > i && ushort.TryParse(path.Substring(i, j - i), out uid))
                return DeleteProfile(type, uid, pd);

            return false;
        }

        private static void DeleteProfile(Settings.IAccount account, PathData pd)
        {
            if (IsDataLinkingSupported)
                DeleteProfile(PathType.DataByAccount, account.UID, pd);
            else if (account.DatFile != null)
                DeleteProfile(PathType.DataByDat, account.DatFile.UID, pd);
        }

        /// <summary>
        /// Deletes the profile directory
        /// </summary>
        /// <param name="type">The profile type</param>
        /// <param name="uid">The ID of the profile</param>
        /// <param name="pd">Path data</param>
        /// <returns>If the directory is deleted</returns>
        private static bool DeleteProfile(PathType type, ushort uid, PathData pd)
        {
            string root;

            switch (type)
            {
                case PathType.DataByDat:

                    root = Path.Combine(pd.accountdata, ALT_DATA, uid.ToString());

                    break;
                case PathType.DataByAccount:

                    root = Path.Combine(pd.accountdata, uid.ToString());

                    break;
                case PathType.DataByGw2:

                    root = Path.Combine(pd.appdata, uid.ToString());

                    break;
                case PathType.Custom:
                    
                    if (IsDataLinkingSupported)
                        type = PathType.DataByAccount;
                    else
                        type = PathType.DataByDat;

                    return DeleteProfile(type, uid, pd);

                default:

                    return false;
            }

            try
            {
                if (!Directory.Exists(root))
                    return true;

                var gw2docs = pd.documents.Substring(pd.userprofile.Length + 1);
                var diDocs = new DirectoryInfo(Path.Combine(root, Path.GetDirectoryName(gw2docs)));
                var diDumps = new DirectoryInfo(Path.Combine(root, pd.appdata.Substring(pd.userprofile.Length + 1), COHERENT_DUMPS_FOLDER_NAME));

                if (diDumps.Exists && diDumps.Attributes.HasFlag(FileAttributes.ReparsePoint))
                    diDumps.Delete();

                var localappdata = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData, Environment.SpecialFolderOption.DoNotVerify);
                if (localappdata.StartsWith(pd.userprofile, StringComparison.OrdinalIgnoreCase))
                {
                    var diLocal = new DirectoryInfo(Path.Combine(root, localappdata.Substring(pd.userprofile.Length + 1)));
                    if (diLocal.Exists && diLocal.Attributes.HasFlag(FileAttributes.ReparsePoint))
                    {
                        diLocal.Delete();
                    }
                }

                if (diDocs.Exists)
                {
                    var deleteAll = true;

                    if (diDocs.Attributes.HasFlag(FileAttributes.ReparsePoint))
                    {
                        diDocs.Delete();
                    }
                    else
                    {
                        var diScreens = new DirectoryInfo(Path.Combine(root, gw2docs, SCREENS_FOLDER_NAME));
                        var diMusic = new DirectoryInfo(Path.Combine(root, gw2docs, MUSIC_FOLDER_NAME));

                        if (diScreens.Exists)
                        {
                            if (diScreens.Attributes.HasFlag(FileAttributes.ReparsePoint))
                            {
                                diScreens.Delete();
                            }
                            else
                            {
                                //move any screenshots that were under this account

                                var gw2screens = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), diScreens.FullName.Substring(diDocs.FullName.Length + 1));
                                if (!Directory.Exists(gw2screens))
                                    Directory.CreateDirectory(gw2screens);

                                var files = Directory.GetFiles(diScreens.FullName, "gw*.*");
                                var exts = new Dictionary<string, ushort>();

                                if (files.Length > 0)
                                {
                                    gw2screens = Path.Combine(gw2screens, "recovered");
                                    if (!Directory.Exists(gw2screens))
                                        Directory.CreateDirectory(gw2screens);
                                }

                                foreach (var f in files)
                                {
                                    var ext = Path.GetExtension(f);
                                    ushort sid;

                                    if (!exts.TryGetValue(ext, out sid))
                                    {
                                        var existing = Directory.GetFiles(gw2screens, "*" + ext);

                                        for (var i = existing.Length - 1; i >= 0; i--)
                                        {
                                            var last = Path.GetFileNameWithoutExtension(existing[i]);
                                            if (ushort.TryParse(last, out sid))
                                                break;
                                        }
                                    }

                                    try
                                    {
                                        File.Move(f, Path.Combine(gw2screens, string.Format("{0:00000}" + ext, ++sid)));
                                    }
                                    catch { }

                                    exts[ext] = sid;
                                }
                            }
                        }

                        if (diMusic.Exists && diMusic.Attributes.HasFlag(FileAttributes.ReparsePoint))
                            diMusic.Delete();

                        if (diDocs.Attributes.HasFlag(FileAttributes.ReadOnly))
                            diDocs.Attributes = diDocs.Attributes & ~FileAttributes.ReadOnly;
                    }

                    if (deleteAll)
                        Directory.Delete(root, true);
                }
                else
                {
                    Directory.Delete(root, true);
                }

                return true;
            }
            catch (Exception e)
            {
                Util.Logging.Log(e);
            }

            return false;
        }

        private static bool CreateProfileRoot(PathData pd, string path)
        {
            var displayName = Settings.VirtualUserPath;

            try
            {
                Windows.Symlink.CreateJunction(path, pd.accountdata);

                displayName.Commit();
                return true;
            }
            catch (Exception e)
            {
                Util.Logging.Log(e);
            }

            //retry as admin
            try
            {
                if (Util.ProcessUtil.CreateJunction(path, pd.accountdata))
                {
                    displayName.Commit();
                    return true;
                }
            }
            catch (Exception e)
            {
                Util.Logging.Log(e);
            }

            return false;
        }

        /// <summary>
        /// Activates any data files for the account
        /// </summary>
        /// <returns>Paths used by the profile, or null if a profile isn't needed</returns>
        public static IProfileInformation Activate(Settings.IAccount account)
        {
            var requiresCustom = IsCustomized(account);

            var pd = new PathData();

            var datType = PathType.Unknown;
            var gfxType = PathType.Unknown;

            #region Custom username

            string profileRoot = null;

            if (IsDataLinkingSupported)
            {
                Func<string, string> getPath = delegate(string v)
                {
                    if (Path.IsPathRooted(v) && Path.GetPathRoot(v).Equals(Path.GetPathRoot(pd.userprofile), StringComparison.OrdinalIgnoreCase))
                    {
                        return Path.GetFullPath(v);
                    }

                    return Path.Combine(Path.GetDirectoryName(pd.userprofile), v);
                };

                var displayName = Settings.VirtualUserPath;
                if (displayName.IsPending)
                {
                    var createUser = displayName.HasValue;

                    if (!string.IsNullOrEmpty(displayName.ValueCommit))
                    {
                        var di = new DirectoryInfo(getPath(displayName.ValueCommit));

                        if (di.Exists && di.Attributes.HasFlag(FileAttributes.ReparsePoint))
                        {
                            //don't delete if it's in use
                            if (Launcher.GetActiveProcessCount() == 0)
                            {
                                try
                                {
                                    di.Delete();
                                }
                                catch (Exception e)
                                {
                                    Util.Logging.Log(e);
                                    createUser = false;
                                }
                            }
                            else
                                createUser = false;
                        }
                    }

                    if (createUser)
                    {
                        var path = getPath(displayName.Value);
                        var exists = Directory.Exists(path);

                        if (exists)
                        {
                            try
                            {
                                //only deleting if it's a link or empty folder
                                Directory.Delete(path);
                                exists = false;
                            }
                            catch (Exception e)
                            {
                                Util.Logging.Log(e);
                            }
                        }

                        if (!exists && CreateProfileRoot(pd, path))
                            profileRoot = path;
                    }
                    else if (!displayName.HasValue)
                        displayName.Commit();

                    FlagAllAccountForPendingFiles();
                }
                else if (displayName.HasValue)
                {
                    profileRoot = getPath(displayName.Value);

                    if (!Directory.Exists(profileRoot) && !CreateProfileRoot(pd, profileRoot))
                    {
                        //failed to create the folder, commiting the path to null to reflag it as pending for next time
                        var v = displayName.Value;
                        displayName.Clear();
                        displayName.Commit();
                        displayName.Value = v;
                        profileRoot = null;
                    }
                }
            }

            #endregion

            string customDatPath, customGfxPath;

            if (!string.IsNullOrEmpty(account.WindowsAccount) && !Util.Users.IsCurrentUser(account.WindowsAccount))
            {
                //verify the user is initialized
                GetFolder(DatFolder.AppData);
            }

            if (account.DatFile != null)
            {
                var path = account.DatFile.Path;
                customDatPath = pd.GetCustomPath(FileType.Dat, account, false);
                datType = pd.GetPathType(path, customDatPath, DAT_NAME);

                #region Old account upgrade

                if (datType == PathType.DataByGw2)
                {
                    if (account.GfxFile == null)
                    {
                        //attempt to find the gfx file that was linked to this dat file
                        var gfx = Path.Combine(Path.GetDirectoryName(path), pd.gfxName);
                        if (File.Exists(gfx))
                        {
                            var file = (Settings.IGfxFile)FindFile(FileType.Gfx, gfx);
                            if (file == null)
                            {
                                file = Settings.CreateGfxFile();
                                file.Path = gfx;
                            }

                            account.GfxFile = file;

                            //update other accounts sharing this dat file to also share the gfx file
                            foreach (var a in FindAccounts(FileType.Dat, account.DatFile))
                            {
                                if (a.GfxFile == null)
                                    a.GfxFile = file;
                            }
                        }
                    }
                }

                #endregion
            }
            else
            {
                datType = PathType.Default;
                customDatPath = null;
            }

            #region Handle default datType

            if (datType == PathType.Default)
            {
                if (account.DatFile == null || string.IsNullOrEmpty(account.DatFile.Path))
                {
                    var defaultPath = Path.Combine(pd.appdata, DAT_NAME);
                    var file = (Settings.IDatFile)FindFile(FileType.Dat, defaultPath);

                    if (file == null)
                    {
                        file = account.DatFile;
                        if (file == null)
                            account.DatFile = file = Settings.CreateDatFile();
                        file.Path = defaultPath;
                    }
                    else
                    {
                        account.DatFile = file;
                    }
                }
            }
            else
            {
                requiresCustom = true;
            }

            #endregion

            if (account.GfxFile != null)
            {
                var path = account.GfxFile.Path;
                customGfxPath = pd.GetCustomPath(FileType.Gfx, account, false);
                gfxType = pd.GetPathType(path, customGfxPath, pd.gfxName);
            }
            else
            {
                gfxType = PathType.Default;
                customGfxPath = null;
            }

            #region Handle default gfxType

            if (gfxType == PathType.Default)
            {
                if (account.GfxFile == null || string.IsNullOrEmpty(account.GfxFile.Path))
                {
                    var defaultPath = Path.Combine(pd.appdata, pd.gfxName);
                    var file = (Settings.IGfxFile)FindFile(FileType.Gfx, defaultPath);

                    if (file == null)
                    {
                        file = account.GfxFile;
                        if (file == null)
                            account.GfxFile = file = Settings.CreateGfxFile();
                        file.Path = defaultPath;
                    }
                    else
                    {
                        account.GfxFile = file;
                    }
                }
            }
            else
            {
                requiresCustom = true;
            }

            #endregion

            #region Move datType

            switch (datType)
            {
                case PathType.Custom:

                    if (pd.IsDocumentsInUserFolder)
                    {
                        string path;
                        if (IsDataLinkingSupported)
                            path = Path.Combine(pd.accountdata, account.UID.ToString(), pd.documents.Substring(pd.userprofile.Length + 1), DAT_NAME);
                        else
                            path = Path.Combine(pd.accountdata, ALT_DATA, account.DatFile.UID.ToString(), pd.documents.Substring(pd.userprofile.Length + 1), DAT_NAME);

                        //documents acts as an override, overwrite the original
                        try
                        {
                            if (Move(path, customDatPath))
                            {
                                OnFilePathMoved(FileType.Dat, account.DatFile);
                            }
                        }
                        catch (Exception e)
                        {
                            Util.Logging.Log(e);
                            throw new Exception("Unable to move " + DAT_NAME + " from documents profile");
                        }
                    }

                    break;
                case PathType.DifferentUser:

                    CreateProfileOrThrow(account, pd);

                    Security.Impersonation.IImpersonationToken impersonation = null;
                    var retry = true;

                    try
                    {
                        var path = account.DatFile.Path;
                        string username = null;

                        while (true)
                        {
                            try
                            {
                                if (Move(path, customDatPath))
                                {
                                    OnFilePathMoved(FileType.Dat, account.DatFile);
                                }

                                break;
                            }
                            catch (Exception e)
                            {
                                Util.Logging.Log(e);

                                if (retry)
                                {
                                    username = pd.GetUsername(path);
                                    if (username != null && Util.Users.IsCurrentUser(username))
                                        username = null;
                                    retry = false;
                                }

                                if (username == null)
                                    throw new Exception("Unable to move " + DAT_NAME);
                            }

                            impersonation = Security.Impersonation.Impersonate(username, Security.Credentials.GetPassword(username));
                        }
                    }
                    finally
                    {
                        if (impersonation != null)
                            impersonation.Dispose();
                    }

                    account.DatFile.Path = customDatPath;
                    datType = PathType.DataByAccount;

                    break;
                case PathType.Default:

                    if (pd.IsDocumentsInUserFolder)
                    {
                        //documents acts as an override, overwrite the original
                        try
                        {
                            if (Move(Path.Combine(pd.documents, DAT_NAME), Path.Combine(pd.appdata, DAT_NAME)))
                            {
                                OnFilePathMoved(FileType.Dat, account.DatFile);
                            }
                        }
                        catch (Exception e)
                        {
                            Util.Logging.Log(e);
                            throw new Exception("Unable to move " + DAT_NAME + " from documents to appdata");
                        }
                    }

                    break;
                default:

                    CreateProfileOrThrow(account, pd);

                    try
                    {
                        if (Move(account.DatFile.Path, customDatPath))
                        {
                            OnFilePathMoved(FileType.Dat, account.DatFile);
                        }
                    }
                    catch (Exception e)
                    {
                        Util.Logging.Log(e);
                        throw new Exception("Unable to move " + DAT_NAME);
                    }

                    account.DatFile.Path = customDatPath;
                    datType = PathType.Custom;

                    break;
            }

            #endregion

            #region Move gfxType

            switch (gfxType)
            {
                case PathType.Default:
                case PathType.Custom:

                    //no need to move

                    break;
                default: 

                    //no need to handle impersonation / was never used
                    CreateProfileOrThrow(account, pd);

                    try
                    {
                        if (Move(account.GfxFile.Path, customGfxPath))
                        {
                            OnFilePathMoved(FileType.Gfx, account.GfxFile);
                        }
                    }
                    catch (Exception e)
                    {
                        Util.Logging.Log(e);
                        throw new Exception("Unable to move existing " + pd.gfxName);
                    }

                    #region Delete old account data

                    if (gfxType == PathType.DataByGw2 && pd.IsDocumentsInUserFolder)
                    {
                        try
                        {
                            DeleteProfile(gfxType, account.GfxFile.Path, pd);
                        }
                        catch (Exception e)
                        {
                            Util.Logging.Log(e);
                        }
                    }

                    #endregion


                    account.GfxFile.Path = customGfxPath;
                    gfxType = PathType.Custom;

                    break;
            }

            #endregion

            bool isVerified = false,
                 isPending = account.PendingFiles;

            if (!isPending && requiresCustom)
            {
                string path;
                if (IsDataLinkingSupported)
                    path = Path.Combine(pd.accountdata, account.UID.ToString());
                else
                    path = Path.Combine(pd.accountdata, ALT_DATA, account.DatFile.UID.ToString());
                isPending = !Directory.Exists(path);
            }

            if (isPending)
            {
                if (requiresCustom)
                {
                    UpdateProfile(account, pd);
                    isVerified = true;
                }
                else if (IsDataLinkingSupported)
                {
                    //this account no longer needs its custom profile; delete if one exists
                    try
                    {
                        DeleteProfile(account, pd);
                    }
                    catch (Exception e)
                    {
                        Util.Logging.Log(e);
                    }
                }
                account.PendingFiles = false;
            }

            if (!isVerified)
            {
                if (account.DatFile.IsInitialized && !File.Exists(account.DatFile.Path))
                    account.DatFile.IsInitialized = false;

                if (account.GfxFile.IsInitialized && !File.Exists(account.GfxFile.Path))
                    account.GfxFile.IsInitialized = false;
            }

            if (requiresCustom)
            {
                if (IsDataLinkingSupported)
                {
                    if (profileRoot == null)
                        pd.profileUserProfile = Path.Combine(pd.accountdata, account.UID.ToString());
                    else
                        pd.profileUserProfile = Path.Combine(profileRoot, account.UID.ToString());
                }
                else
                    pd.profileUserProfile = Path.Combine(pd.accountdata, ALT_DATA, account.DatFile.UID.ToString());

                pd.profileAppData = Path.Combine(pd.profileUserProfile, pd.appdata.Substring(pd.userprofile.Length + 1));

                return pd;
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// Activates the account's specific executable path
        /// </summary>
        /// <param name="fi">Path to Gw2.exe</param>
        /// <returns>Path to this account's executable</returns>
        public static string ActivateExecutable(Settings.IAccount account, FileInfo fi)
        {
            if (!IsGw2LinkingSupported)
                throw new NotSupportedException();

            var gw2root = fi.DirectoryName;
            var localroot = Path.Combine(gw2root, LOCALIZED_EXE_FOLDER_NAME);
            var root = Path.Combine(localroot, account.UID.ToString());
            var exe = Path.Combine(root, fi.Name);

            if (File.Exists(exe))
            {
                if (File.GetLastWriteTimeUtc(exe) == fi.LastWriteTimeUtc)
                    return exe;
                File.Delete(exe);
            }
            else if (!Directory.Exists(localroot))
            {
                try
                {
                    Directory.CreateDirectory(localroot);
                }
                catch
                {
                    //the entire GW2 folder needs permission
                    if (!Util.ProcessUtil.CreateFolder(gw2root))
                        throw;
                    Directory.CreateDirectory(localroot);
                }
            }

            var v = Settings.LocalizeAccountExecution;
            if (v.IsPending && v.Value)
                v.Commit();

            if (exebits == 0)
                exebits = Util.FileUtil.GetExecutableBits(fi.FullName);

            if (exebits == 32)
            {
                var bin = Path.Combine(gw2root, "bin");
                if (Directory.Exists(bin))
                    CopyBinFolder(bin, Path.Combine(root, "bin"));
            }
            else
            {
                var bin64 = Path.Combine(gw2root, "bin64");
                if (Directory.Exists(bin64))
                    CopyBinFolder(bin64, Path.Combine(root, "bin64"));
            }

            MakeLink(root, gw2root, "Gw2.dat");
            MakeLink(root, gw2root, "THIRDPARTYSOFTWAREREADME.txt");

            Windows.Symlink.CreateHardLink(exe, fi.FullName);

            return exe;
        }

        private static bool DeleteExecutable(ushort uid)
        {
            var path = Settings.GW2Path.Value;
            if (string.IsNullOrEmpty(path))
                return false;
            var root = Path.Combine(Path.GetDirectoryName(path), LOCALIZED_EXE_FOLDER_NAME, uid.ToString());

            if (Directory.Exists(root))
            {
                var name = Path.GetFileName(path);
                path = Path.Combine(root,name);
                if (File.Exists(path))
                {
                    try
                    {
                        File.Delete(path);
                    }
                    catch
                    {
                        return false;
                    }
                }

                try
                {
                    Directory.Delete(root, true);
                }
                catch
                {
                    return false;
                }
            }

            return true;
        }

        private static bool DeleteExecutableRoot()
        {
            var path = Settings.GW2Path.Value;
            if (string.IsNullOrEmpty(path) || Client.Launcher.GetActiveProcessCount() != 0)
                return false;
            var root = Path.Combine(Path.GetDirectoryName(path), LOCALIZED_EXE_FOLDER_NAME);

            if (Directory.Exists(root))
            {
                try
                {
                    Directory.Delete(root, true);
                }
                catch
                {
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// Makes a hard link
        /// </summary>
        /// <param name="link">The directory where the link will be created</param>
        /// <param name="target">The directory where the target file is located</param>
        /// <param name="name">The name of the target file</param>
        private static void MakeLink(string link, string target, string name)
        {
            link = Path.Combine(link, name);
            target = Path.Combine(target, name);
            if (File.Exists(link))
                File.Delete(link);
            if (File.Exists(target))
                Windows.Symlink.CreateHardLink(link, target);
        }

        private static void CopyBinFolder(string from, string to)
        {
            var existing = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            if (Directory.Exists(to))
            {
                foreach (var path in Directory.GetFiles(to))
                {
                    existing.Add(Path.GetFileName(path));
                }
            }
            else
            {
                Directory.CreateDirectory(to);
            }

            foreach (var path in Directory.GetFiles(from))
            {
                var name = Path.GetFileName(path);
                var output = Path.Combine(to, name);

                switch (name)
                {
                    //these files can be linked
                    case "CoherentUI_Host.exe":
                    case "CoherentUI64.dll":
                    case "CoherentUI.dll":

                        if (existing.Contains(name))
                            File.Delete(output);
                        Windows.Symlink.CreateHardLink(output, path);

                        break;
                    //these files require exclusive access
                    case "d3dcompiler_43.dll":
                    case "ffmpegsumo.dll":
                    case "icudt.dll":
                    case "libEGL.dll":
                    case "libGLESv2.dll":

                        File.Copy(path, output, true);

                        break;
                    //these files are unknown
                    default:

                        if (!existing.Contains(name))
                            Windows.Symlink.CreateHardLink(output, path);

                        break;
                }
            }
        }
    }
}
