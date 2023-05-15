using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.IO;

namespace Gw2Launcher.Client
{
    public static partial class FileManager
    {
        class GfxMonitor : IDisposable
        {
            private class Watcher : IDisposable
            {
                public event EventHandler Error;

                public string path, username;
                public FileSystemWatcher watcher;
                public GfxMonitor parent;

                public Watcher(GfxMonitor parent, string path, string username = null)
                {
                    this.path = path;
                    this.username = username;
                    this.parent = parent;
                }

                public bool Enabled
                {
                    get
                    {
                        return watcher != null && watcher.EnableRaisingEvents;
                    }
                    set
                    {
                        if (value)
                        {
                            if (!Directory.Exists(path))
                            {
                                try
                                {
                                    Directory.CreateDirectory(path);
                                }
                                catch
                                {
                                    if (username != null)
                                    {
                                        try
                                        {
                                            using (var impersonation = Security.Impersonation.Impersonate(username, Security.Credentials.GetPassword(username)))
                                            {
                                                Directory.CreateDirectory(path);
                                                Util.FileUtil.AllowFolderAccess(path, System.Security.AccessControl.FileSystemRights.Modify);
                                            }
                                        }
                                        catch
                                        {
                                            return;
                                        }
                                    }
                                    else
                                    {
                                        return;
                                    }
                                }
                            }

                            try
                            {
                                if (watcher == null)
                                {
                                    watcher = new FileSystemWatcher(path, "GFXSettings.*.xml");
                                    watcher.Changed += watcher_Changed;
                                    watcher.Error += watcher_Error;
                                    watcher.NotifyFilter = NotifyFilters.LastWrite;
                                }
                                watcher.EnableRaisingEvents = true;
                            }
                            catch { }
                        }
                        else
                        {
                            if (watcher != null)
                            {
                                watcher.Dispose();
                                watcher = null;
                            }
                        }
                    }
                }

                void watcher_Error(object sender, ErrorEventArgs e)
                {
                    try
                    {
                        watcher.EnableRaisingEvents = false;
                        watcher.EnableRaisingEvents = true;
                    }
                    catch
                    {
                        using (watcher)
                        {
                            watcher = null;
                        }

                        if (Error != null)
                            Error(this, EventArgs.Empty);
                    }
                }

                void watcher_Changed(object sender, FileSystemEventArgs e)
                {
                    parent.Queue(e.FullPath);
                }

                public void Dispose()
                {
                    using (watcher)
                    {
                        watcher = null;
                    }
                }
            }

            private Task task;
            private Dictionary<string, Watcher> watchers;
            private HashSet<string> queue;
            private bool disposing;

            public GfxMonitor()
            {
                watchers = new Dictionary<string, Watcher>();
                queue = new HashSet<string>();
            }

            /// <summary>
            /// Adds the path, but it will not be monitored until activated
            /// </summary>
            public void Add(string path, string username)
            {
                lock (watchers)
                {
                    if (!watchers.ContainsKey(path))
                    {
                        try
                        {
                            var w = new Watcher(this, path, username);
                            w.Error += watcher_Error;

                            watchers.Add(path, w);
                        }
                        catch { }
                    }
                }
            }

            void watcher_Error(object sender, EventArgs e)
            {
                //prevent deadlock when watcher raises an error while disposing
                while (!Monitor.TryEnter(watchers, 100))
                {
                    if (disposing)
                        return;
                }

                try
                {
                    var watcher = (Watcher)sender;

                    Watcher w;
                    if (watchers.TryGetValue(watcher.path, out w) && object.ReferenceEquals(w, watcher))
                    {
                        watchers.Remove(watcher.path);
                    }
                }
                finally
                {
                    Monitor.Exit(watchers);
                }
            }

            public void Remove(string path)
            {
                lock (watchers)
                {
                    Watcher w;
                    if (watchers.TryGetValue(path, out w))
                    {
                        watchers.Remove(path);
                        w.Dispose();
                    }
                }
            }

            private void Queue(string path)
            {
                lock (queue)
                {
                    if (queue.Add(path))
                    {
                        if (task == null || task.IsCompleted)
                            task = Task.Run(new Action(DoQueue));
                    }
                }
            }

            private void DoQueue()
            {
                while (true)
                {
                    string path = null;

                    lock (queue)
                    {
                        if (queue.Count == 0)
                        {
                            task = null;
                            return;
                        }

                        foreach (var p in queue)
                        {
                            path = p;
                            break;
                        }

                        queue.Remove(path);
                    }

                    byte[] data;
                    var limit = DateTime.UtcNow.AddSeconds(1);

                    while (true)
                    {
                        try
                        {
                            data = File.ReadAllBytes(path);

                            break;
                        }
                        catch
                        {
                            if (DateTime.UtcNow > limit)
                            {
                                data = null;
                                break;
                            }
                        }

                        System.Threading.Thread.Sleep(100);
                    }

                    if (data != null)
                    {
                        try
                        {
                            ProcessData(data);
                        }
                        catch (Exception e)
                        {
                            Util.Logging.Log(e);
                        }
                    }
                }
            }

            private void ProcessData(byte[] data)
            {
                using (var reader = new StreamReader(new MemoryStream(data)))
                {
                    string line;
                    while ((line = reader.ReadLine()) != null)
                    {
                        var i = line.IndexOf('<');
                        if (i == -1)
                            continue;

                        if (line.Length > 7 && line.Substring(i + 1, 7).Equals("EXECCMD", StringComparison.Ordinal))
                        {
                            i = line.IndexOf("-l:id:");
                            if (i == -1)
                                return;

                            i += 6;

                            var j = i + 1;
                            while (char.IsDigit(line[j]))
                            {
                                j++;
                            }

                            ushort uid;
                            if (ushort.TryParse(line.Substring(i, j - i), out uid))
                            {
                                var account = (Settings.IGw2Account)Settings.Accounts[uid].Value;
                                if (Launcher.GetState(account) != Launcher.AccountState.ActiveGame)
                                    return;
                                var path = account.GfxFile.Path;

                                using (var stream = File.Open(path, FileMode.Open, FileAccess.Write, FileShare.Read))
                                {
                                    stream.Write(data, 0, data.Length);
                                    stream.SetLength(stream.Position);
                                }
                            }

                            return;
                        }
                    }
                }
            }

            public int Count
            {
                get
                {
                    return watchers.Count;
                }
            }

            /// <summary>
            /// Activates any added paths
            /// </summary>
            public void Activate()
            {
                lock (watchers)
                {
                    foreach (var w in watchers.Values)
                    {
                        w.Enabled = true;
                    }
                }
            }

            public void Dispose()
            {
                disposing = true;

                lock (watchers)
                {
                    foreach (var w in watchers.Values)
                    {
                        w.Dispose();
                    }
                    if (task != null && task.IsCompleted)
                    {
                        task.Dispose();
                    }
                    task = null;
                }
            }
        }

        public const string LOCALIZED_EXE_FOLDER_NAME = "Gw2Launcher";

        private const string GW1_DAT_NAME = "Gw.dat";
        private const string DAT_NAME = "Local.dat";
        private const string GFX_NAME = "GFXSettings.{0}.xml";
        private const string ALT_DATA = "alt";
        private const string SCREENS_FOLDER_NAME = "Screens";
        private const string MUSIC_FOLDER_NAME = "Music";
        private const string COHERENT_DUMPS_FOLDER_NAME = "Coherent Dumps";
        private const string GW1_SCREENS_FOLDER_NAME = "Screens";
        private const string GW1_TEMPLATES_FOLDER_NAME = "Templates";
        private const string GW2_FOLDER_NAME = "Guild Wars 2";
        private const string GW2_BASIC_FOLDER_NAME = "Guild Wars 2-Gw2Launcher";

        private static IsSupportedState isSupported;
        private static byte exebits;
        private static GfxMonitor gfxMonitor;

        [Flags]
        public enum PathSupportType
        {
            None = 0,
            Files = 1,
            Folders = 2,
        }

        public enum SpecialPath
        {
            AppData,
            Documents,
            Screens,
            Music,
            Dumps,
        }

        private enum PathType
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
            GwDat,
        }

        [Flags]
        private enum IsSupportedState : byte
        {
            DataTested = 1,
            DataSupported = 2,

            Gw2Tested = 4,
            Gw2Supported = 8,

            FoldersTested = 16,
            FoldersSupported = 32,

            Gw2ChinaTested = 64,
            Gw2China = 128,
        }

        public interface IProfileInformation : IDisposable
        {
            /// <summary>
            /// %userprofile% location or null if not used
            /// </summary>
            string UserProfile
            {
                get;
            }

            /// <summary>
            /// %appdata% location or null if not used
            /// </summary>
            string AppData
            {
                get;
            }

            Settings.ProfileMode Mode
            {
                get;
            }

            bool IsBasic
            {
                get;
            }
        }

        private class PathData
        {
            public enum SpecialPath
            {
                /// <summary>
                /// The root folder for the profile
                /// </summary>
                ProfileRoot,
                /// <summary>
                /// The current location of the "Guild Wars 2" folder in %appdata%
                /// </summary>
                Gw2AppData,
                /// <summary>
                /// The default location of the "Guild Wars 2" folder in %appdata%
                /// </summary>
                Gw2AppDataDefault,
                /// <summary>
                /// The renambed location of the "Guild Wars 2" folder in %appdata%
                /// </summary>
                Gw2AppDataAlternate,
                /// <summary>
                /// The location of the "Guild Wars 2" documents folder
                /// </summary>
                Gw2Documents,
                /// <summary>
                /// The location of the local %appdata% folder
                /// </summary>
                LocalAppData,
                /// <summary>
                /// The location of the roaming %appdata% folder (generally the same as %appdata%)
                /// </summary>
                RoamingAppData,
                /// <summary>
                /// The location of the %appdata% folder
                /// </summary>
                AppData,
                /// <summary>
                /// The location of the documents folder
                /// </summary>
                Documents,
                /// <summary>
                /// The location of data files stored by Gw2Launcher for each account
                /// </summary>
                Gw2LauncherAccountData,
                /// <summary>
                /// The location of Local.dat
                /// </summary>
                LocalDatFile,
                /// <summary>
                /// The location of GFXSettings.xml
                /// </summary>
                GfxSettingsFile,
            }

            private string
                accountdata,
                userprofile,
                userappdata,
                userlocalappdata,
                userdocuments,
                gfxName,
                defaultFolderName;

            public bool isBasic,
                isDefaultFolderNameBasic;
            
            public PathData()
                : this(Settings.GuildWars2.ProfileMode.Value == Settings.ProfileMode.Basic)
            {
            }

            public PathData(Settings.IGw2Account account)
                : this(Settings.GuildWars2.ProfileMode.Value == Settings.ProfileMode.Basic)
            {
                //if (account.Proxy == Settings.LaunchProxy.Steam)
                //{
                //    this.isBasic = true;
                //}
            }

            public PathData(bool isBasic)
            {
                this.isBasic = isBasic;

                accountdata = DataPath.AppDataAccountData;
                userprofile = GetEnvironmentPath(Environment.SpecialFolder.UserProfile, Environment.SpecialFolderOption.Create);
                userappdata = GetEnvironmentPath(Environment.SpecialFolder.ApplicationData, Environment.SpecialFolderOption.Create);
                userlocalappdata = GetEnvironmentPath(Environment.SpecialFolder.LocalApplicationData, Environment.SpecialFolderOption.DoNotVerify);
                userdocuments = GetEnvironmentPath(Environment.SpecialFolder.MyDocuments, Environment.SpecialFolderOption.DoNotVerify);

                string exe;
                if (!string.IsNullOrEmpty(Settings.GuildWars2.Path.Value))
                    exe = Path.GetFileName(Settings.GuildWars2.Path.Value);
                else if (Environment.Is64BitOperatingSystem)
                    exe = "Gw2-64.exe";
                else
                    exe = "Gw2.exe";

                gfxName = string.Format(GFX_NAME, exe);
            }

            private string GetEnvironmentPath(Environment.SpecialFolder folder, Environment.SpecialFolderOption option)
            {
                string path;

                try
                {
                    path = Environment.GetFolderPath(folder, option);
                }
                catch
                {
                    path = null;
                }

                if (string.IsNullOrWhiteSpace(path))
                {
                    throw new UserAccountNotInitializedException(Environment.UserName);
                }

                return path;
            }

            /// <summary>
            /// Returns the path for the specified file type
            /// </summary>
            /// <param name="type">Type of file</param>
            /// <param name="account">Account linked to the file</param>
            /// <param name="link">True to return the link of the file, false to return the real location</param>
            /// <returns>The path to the file</returns>
            public string GetCustomPath(FileType type, Settings.IGw2Account account, bool link)
            {
                switch (type)
                {
                    case FileType.Dat:

                        if (IsDataLinkingSupported)
                        {
                            if (link)
                                return GetProfilePath(SpecialPath.LocalDatFile, account);
                            else
                                return Path.Combine(accountdata, account.DatFile.UID + ".dat");
                        }
                        else
                            return GetProfilePath(SpecialPath.LocalDatFile, account);

                    case FileType.Gfx:

                        if (IsDataLinkingSupported)
                        {
                            if (link)
                                return GetProfilePath(SpecialPath.GfxSettingsFile, account);
                            else
                                return Path.Combine(accountdata, account.GfxFile.UID + ".xml");
                        }
                        else
                            return GetProfilePath(SpecialPath.GfxSettingsFile, account);

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
                    var appdata = GetUserPath(SpecialPath.AppData);

                    if (path.StartsWith(appdata, StringComparison.OrdinalIgnoreCase))
                    {
                        var i = appdata.Length + 1;
                        var j = path.IndexOf(Path.DirectorySeparatorChar, i);
                        var n = path.Substring(i, j - i);

                        if (n.Equals(GW2_FOLDER_NAME, StringComparison.OrdinalIgnoreCase) || n.Equals(GW2_BASIC_FOLDER_NAME, StringComparison.OrdinalIgnoreCase))
                        {
                            i = j + 1;
                            j = path.IndexOf(Path.DirectorySeparatorChar, i);

                            if (j == -1 && path.Substring(i).Equals(defaultName, StringComparison.OrdinalIgnoreCase))
                            {
                                return PathType.Default;
                            }
                            else if (j > 0 && int.TryParse(path.Substring(i, j - i), out i))
                            {
                                return PathType.DataByGw2;
                            }
                        }

                        return PathType.CurrentUser;
                    }
                    else
                    {
                        var p = GetUserPath(SpecialPath.ProfileRoot);

                        if (path.StartsWith(p, StringComparison.OrdinalIgnoreCase))
                        {
                            return PathType.CurrentUser;
                        }
                        else
                        {
                            p = Path.GetDirectoryName(p);

                            if (path.StartsWith(p, StringComparison.OrdinalIgnoreCase))
                            {
                                var i = p.Length + 1;
                                var j = path.IndexOf(Path.DirectorySeparatorChar, i);

                                if (j != -1)
                                {
                                    return PathType.DifferentUser;
                                }
                            }
                        }

                        return PathType.Unknown;
                    }
                }
            }

            public string GetUserPath(SpecialPath type)
            {
                switch (type)
                {
                    case SpecialPath.ProfileRoot:

                        return userprofile;

                    case SpecialPath.Gw2LauncherAccountData:

                        return accountdata;

                    case SpecialPath.RoamingAppData:
                    case SpecialPath.AppData:

                        return userappdata;

                    case SpecialPath.Gw2AppData:

                        return Path.Combine(userappdata, CurrentDefaultGw2DirectoryName);

                    case SpecialPath.Gw2AppDataDefault:

                        return Path.Combine(userappdata, GW2_FOLDER_NAME);

                    case SpecialPath.Gw2AppDataAlternate:

                        return Path.Combine(userappdata, GW2_BASIC_FOLDER_NAME);

                    case SpecialPath.Documents:

                        return userdocuments;

                    case SpecialPath.Gw2Documents:

                        return Path.Combine(userdocuments, GW2_FOLDER_NAME);

                    case SpecialPath.LocalAppData:

                        return userlocalappdata;

                    case SpecialPath.LocalDatFile:

                        return Path.Combine(userappdata, CurrentDefaultGw2DirectoryName, DAT_NAME);

                    case SpecialPath.GfxSettingsFile:

                        return Path.Combine(userappdata, CurrentDefaultGw2DirectoryName, gfxName);
                }

                return null;
            }

            public string GetProfilePath(SpecialPath type, Settings.IGw2Account account, bool create)
            {
                var path = GetProfilePath(type, account);
                if (create && path != null)
                {
                    Directory.CreateDirectory(path);
                }
                return path;
            }
            
            /// <summary>
            /// Returns the path relative to the root folder
            /// </summary>
            public string GetRelativeProfilePath(SpecialPath type)
            {
                return GetRelativeProfilePath(type, isBasic);
            }

            /// <summary>
            /// Returns the path relative to the root folder
            /// </summary>
            public string GetRelativeProfilePath(SpecialPath type, bool isBasic)
            {
                if (isBasic)
                {
                    switch (type)
                    {
                        case SpecialPath.Gw2AppData:
                        case SpecialPath.ProfileRoot:

                            return "";

                        case SpecialPath.LocalDatFile:

                            return DAT_NAME;

                        case SpecialPath.GfxSettingsFile:

                            return gfxName;
                    }
                }
                else
                {
                    switch (type)
                    {
                        case SpecialPath.ProfileRoot:

                            return "";

                        case SpecialPath.RoamingAppData:
                        case SpecialPath.AppData:

                            return userappdata.Substring(userprofile.Length + 1);

                        case SpecialPath.Gw2AppData:

                            return Path.Combine(userappdata.Substring(userprofile.Length + 1), GW2_FOLDER_NAME);

                        case SpecialPath.Documents:

                            return Path.Combine(userdocuments.Substring(userprofile.Length + 1));

                        case SpecialPath.Gw2Documents:

                            return Path.Combine(userdocuments.Substring(userprofile.Length + 1), GW2_FOLDER_NAME);

                        case SpecialPath.LocalAppData:

                            return Path.Combine(userlocalappdata.Substring(userprofile.Length + 1));

                        case SpecialPath.LocalDatFile:

                            return Path.Combine(userappdata.Substring(userprofile.Length + 1), GW2_FOLDER_NAME, DAT_NAME);

                        case SpecialPath.GfxSettingsFile:

                            return Path.Combine(userappdata.Substring(userprofile.Length + 1), GW2_FOLDER_NAME, gfxName);

                    }
                }

                return null;
            }

            public string GetProfilePath(SpecialPath type, Settings.IGw2Account account)
            {
                var path = GetRelativeProfilePath(type);

                if (path != null)
                {
                    if (IsDataLinkingSupported)
                        path = Path.Combine(accountdata, account.UID.ToString(), path);
                    else
                        path = Path.Combine(accountdata, ALT_DATA, account.DatFile.UID.ToString(), path);
                }

                return path;
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
                    return userdocuments.StartsWith(userprofile, StringComparison.OrdinalIgnoreCase);
                }
            }

            public bool IsAppDataInUserFolder
            {
                get
                {
                    return userappdata.StartsWith(userprofile, StringComparison.OrdinalIgnoreCase);
                }
            }

            public bool IsLocalAppDataInUserFolder
            {
                get
                {
                    return userlocalappdata.StartsWith(userprofile, StringComparison.OrdinalIgnoreCase);
                }
            }

            public string DefaultGfxFileName
            {
                get
                {
                    return gfxName;
                }
            }

            private void UpdateGw2DirectoryName()
            {
                if (isDefaultFolderNameBasic = Directory.Exists(Path.Combine(userappdata, GW2_BASIC_FOLDER_NAME)))
                    defaultFolderName = GW2_BASIC_FOLDER_NAME;
                else
                    defaultFolderName = GW2_FOLDER_NAME;
            }

            public bool IsCurrentDefaultGw2DirectoryNameBasic
            {
                get
                {
                    if (defaultFolderName == null)
                        UpdateGw2DirectoryName();
                    return isDefaultFolderNameBasic;
                }
            }

            public string CurrentDefaultGw2DirectoryName
            {
                get
                {
                    if (defaultFolderName == null)
                        UpdateGw2DirectoryName();
                    return defaultFolderName;
                }
            }

            public void RefreshCurrentDefaultGw2DirectoryName()
            {
                defaultFolderName = null;
            }
        }

        private class ProfileData : IProfileInformation
        {
            public ProfileData(PathData source)
            {
                this.Source = source;
            }

            /// <summary>
            /// The resulting AppData location for the custom profile
            /// </summary>
            public string AppData
            {
                get;
                set;
            }

            /// <summary>
            /// The resulting UserProfile location for the custom profile
            /// </summary>
            public string UserProfile
            {
                get;
                set;
            }

            public PathData Source
            {
                get;
                set;
            }

            public Settings.ProfileMode Mode
            {
                get
                {
                    if (Source.isBasic)
                    {
                        return Settings.ProfileMode.Basic;
                    }

                    return Settings.ProfileMode.Advanced;
                }
            }

            public bool IsBasic
            {
                get
                {
                    return Source.isBasic;
                }
            }

            public List<FileLocker.ISharedFile> Files
            {
                get;
                set;
            }

            public void Dispose()
            {
                if (Files != null)
                {
                    foreach (var f in Files)
                    {
                        f.Dispose();
                    }
                    Files = null;
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
                var s = IsPathSupported(path, true);

                IsDataLinkingSupported = (s & PathSupportType.Files) != 0;
                IsFolderLinkingSupported = (s & PathSupportType.Folders) != 0;
            }
            catch (Exception e)
            {
                Util.Logging.Log(e);
            }

            DoPendingLocalizeAccountExecution();

            Settings.GuildWars2.Path.ValueChanged += Gw2Path_ValueChanged;
            Settings.GuildWars2.ScreenshotsLocation.ValueChanged += Gw2ScreenshotsLocation_ValueChanged;
            Settings.GuildWars2.ProfileMode.ValueChanged += Gw2ProfileMode_ValueChanged;
            Settings.GuildWars2.ProfileOptions.ValueChanged += ProfileOptions_ValueChanged;
            //Settings.GuildWars1.Path.ValueChanged += GW1Path_ValueChanged;
            Settings.GuildWars1.ScreenshotsLocation.ValueChanged += Gw1ScreenshotsLocation_ValueChanged;
            Settings.GuildWars2.LocalizeAccountExecution.ValueChanged += Gw2LocalizeAccountExecution_ValueChanged;
            Settings.GuildWars2.UseCustomGw2Cache.ValueChanged += Gw2UseCustomCache_ValueChanged;

            Client.Launcher.ActiveProcessCountChanged += Launcher_ActiveProcessCountChanged;
        }

        static void Gw2UseCustomCache_ValueChanged(object sender, EventArgs e)
        {
            if (Settings.GuildWars2.UseCustomGw2Cache.Value)
            {
                foreach (var f in GetFiles(FileType.Dat))
                {
                    f.IsPending = true;
                }
            }
        }

        static void Gw2ProfileMode_ValueChanged(object sender, EventArgs e)
        {
            if (Settings.GuildWars2.VirtualUserPath.HasValue)
            {
                Settings.GuildWars2.VirtualUserPath.SetPending();
            }

            FlagAccountsForPendingFiles(Settings.AccountType.GuildWars2);
        }

        static void ProfileOptions_ValueChanged(object sender, EventArgs e)
        {
            if (Settings.GuildWars2.ProfileMode.Value == Settings.ProfileMode.Basic && Settings.GuildWars2.ProfileOptions.HasValue)
            {
                Task.Run(new Action(OnProfileOptionsChanged));
            }
        }

        static void OnProfileOptionsChanged()
        {
            if (Settings.GuildWars2.ProfileMode.Value == Settings.ProfileMode.Basic && Settings.GuildWars2.ProfileOptions.HasValue)
            {
                var o = Settings.GuildWars2.ProfileOptions.Value;
                var pd = new PathData();
                var users = new HashSet<string>();

                foreach (var a in Util.Accounts.GetGw2Accounts())
                {
                    if ((o & Settings.ProfileModeOptions.ClearTemporaryFiles) == Settings.ProfileModeOptions.ClearTemporaryFiles)
                    {
                        DeleteProfile(pd.GetProfilePath(PathData.SpecialPath.ProfileRoot, a));
                        a.PendingFiles = true;
                    }

                    if ((o & Settings.ProfileModeOptions.RestoreOriginalPath) == Settings.ProfileModeOptions.RestoreOriginalPath)
                    {
                        if (users.Add(a.WindowsAccount) && !Launcher.IsUserActive(Launcher.AccountType.GuildWars2, Util.Users.GetUserName(a.WindowsAccount)))
                        {
                            if (Util.Users.IsCurrentUser(a.WindowsAccount))
                            {
                                DeactivateBasicPath(1000);
                            }
                            else
                            {
                                try
                                {
                                    using (var impersonation = Security.Impersonation.Impersonate(a.WindowsAccount, Security.Credentials.GetPassword(a.WindowsAccount)))
                                    {
                                        DeactivateBasicPath(1000);
                                    }
                                }
                                catch { }
                            }
                        }
                    }
                }
            }
        }

        static void Launcher_AllQueuedLaunchesComplete(object sender, EventArgs e)
        {
            if (gfxMonitor != null)
            {
                gfxMonitor.Activate();
            }
        }

        static void Launcher_ActiveProcessCountChanged(Launcher.AccountType type, ushort count)
        {
            if (type == Launcher.AccountType.GuildWars2 && count == 0)
            {
                DoPendingLocalizeAccountExecution();

                if (gfxMonitor != null)
                {
                    Launcher.AllQueuedLaunchesComplete -= Launcher_AllQueuedLaunchesComplete;

                    gfxMonitor.Dispose();
                    gfxMonitor = null;
                }
            }
        }

        static void Gw2LocalizeAccountExecution_ValueChanged(object sender, EventArgs e)
        {
            DoPendingLocalizeAccountExecution();
        }

        static void Gw2ScreenshotsLocation_ValueChanged(object sender, EventArgs e)
        {
            FlagAccountsForPendingFiles(Settings.AccountType.GuildWars2);
        }

        static void Gw1ScreenshotsLocation_ValueChanged(object sender, EventArgs e)
        {
            FlagAccountsForPendingFiles(Settings.AccountType.GuildWars1);
        }

        static void Gw2Path_ValueChanged(object sender, EventArgs e)
        {
            isSupported &= ~(IsSupportedState.Gw2Tested | IsSupportedState.Gw2Supported | IsSupportedState.Gw2China | IsSupportedState.Gw2ChinaTested);
            exebits = 0;
            FlagAccountsForPendingFiles(Settings.AccountType.GuildWars2);
        }

        private static void DoPendingLocalizeAccountExecution()
        {
            try
            {
                var v = Settings.GuildWars2.LocalizeAccountExecution;

                if (v.IsPending)
                {
                    if ((v.Value & Settings.LocalizeAccountExecutionOptions.Enabled) != Settings.LocalizeAccountExecutionOptions.Enabled)
                    {
                        if (DeleteLocalizedRoot(false))
                            v.Commit();
                    }
                    else
                    {
                        var changed = v.Value ^ v.ValueCommit;

                        if ((changed & Settings.LocalizeAccountExecutionOptions.OnlyIncludeBinFolders) != Settings.LocalizeAccountExecutionOptions.None)
                        {
                            if (DeleteLocalizedRoot(true))
                                v.Commit();
                        }
                        else if ((changed & (Settings.LocalizeAccountExecutionOptions.Enabled | Settings.LocalizeAccountExecutionOptions.OnlyIncludeBinFolders)) == Settings.LocalizeAccountExecutionOptions.None)
                        {
                            v.Commit();
                        }
                        else if ((changed & Settings.LocalizeAccountExecutionOptions.Enabled) == Settings.LocalizeAccountExecutionOptions.Enabled)
                        {
                            v.Commit();
                        }
                    }
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
        public static PathSupportType IsPathSupported(string path, bool test)
        {
            try
            {
                if (!Settings.IsRunningWine)
                {
                    if (path[1] == ':' && new DriveInfo(path.Substring(0, 1)).DriveFormat.Equals("NTFS", StringComparison.OrdinalIgnoreCase))
                        return PathSupportType.Files | PathSupportType.Folders;
                }
            }
            catch (Exception e)
            {
                Util.Logging.Log(e);
            }

            var support = PathSupportType.None;

            if (test)
            {
                var l = Util.FileUtil.GetTemporaryFileName(path);
                if (l == null)
                    return support;
                var a = l + "-a";
                var b = l + "-b";

                try
                {
                    File.WriteAllBytes(a, new byte[0]);
                    Windows.Symlink.CreateHardLink(b, a);
                    support |= PathSupportType.Files;
                }
                catch (Exception e)
                {
                    Util.Logging.Log(e);
                    return support;
                }
                finally
                {
                    if (Delete(a))
                        Delete(b);
                }

                l = Util.FileUtil.GetTemporaryFolderName(path);
                if (l == null)
                    return support;
                a = l + "-a";
                b = l + "-b";

                try
                {
                    MakeJunction(b, a, true, true);
                    support |= PathSupportType.Folders;
                }
                catch (Exception e)
                {
                    Util.Logging.Log(e);
                }
                finally
                {
                    if (Directory.Exists(b))
                        Directory.Delete(b);
                    if (Directory.Exists(a))
                        Directory.Delete(a);
                }
            }

            return support;
        }

        public static bool IsBasicModeSupported
        {
            get
            {
                return IsFolderLinkingSupported;
            }
        }

        public static bool IsVirtualModeSupported
        {
            get
            {
                try
                {
                    using (var k = Microsoft.Win32.Registry.CurrentUser.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Explorer\User Shell Folders"))
                    {
                        if (k != null)
                        {
                            var v = k.GetValue("AppData", null, Microsoft.Win32.RegistryValueOptions.DoNotExpandEnvironmentNames) as string;

                            if (v != null && !v.StartsWith("%userprofile%", StringComparison.OrdinalIgnoreCase))
                            {
                                return false;
                            }
                        }
                    }
                }
                catch { }

                return true;
            }
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

        public static bool IsFolderLinkingSupported
        {
            get
            {
                return (isSupported & IsSupportedState.FoldersSupported) == IsSupportedState.FoldersSupported;
            }
            private set
            {
                if (value)
                    isSupported |= IsSupportedState.FoldersSupported;
                else
                    isSupported &= ~IsSupportedState.FoldersSupported;
            }
        }

        public static bool IsGw2LinkingSupported
        {
            get
            {
                if ((isSupported & IsSupportedState.Gw2Tested) != IsSupportedState.Gw2Tested)
                {
                    var path = Settings.GuildWars2.Path.Value;
                    if (!string.IsNullOrEmpty(path))
                    {
                        var s = IsPathSupported(path, false);
                        IsGw2LinkingSupported = (s & PathSupportType.Files) != 0;
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

        public static bool IsGw2China
        {
            get
            {
                if ((isSupported & IsSupportedState.Gw2ChinaTested) != IsSupportedState.Gw2ChinaTested)
                {
                    IsGw2China = IsPathGw2China(Settings.GuildWars2.Path.Value);
                }
                return (isSupported & IsSupportedState.Gw2China) == IsSupportedState.Gw2China;
            }
            private set
            {
                if (value)
                    isSupported |= IsSupportedState.Gw2China | IsSupportedState.Gw2ChinaTested;
                else
                    isSupported = (isSupported & ~IsSupportedState.Gw2China) | IsSupportedState.Gw2ChinaTested;
            }
        }

        private static void FlagAccountsForPendingFiles(Settings.AccountType t)
        {
            foreach (var a in Util.Accounts.GetAccounts(t))
            {
                a.PendingFiles = true;
            }
        }

        private static void FlagAllAccountForPendingFiles()
        {
            foreach (var a in Util.Accounts.GetAccounts())
            {
                a.PendingFiles = true;
            }
        }

        private static void VerifyUserInitialized()
        {
            string path;

            try
            {
                path = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData, Environment.SpecialFolderOption.Create);
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
        }

        private static bool IsCustomized(Settings.IGw2Account account)
        {
            return Settings.GuildWars2.ScreenshotsLocation.HasValue || IsFolderLinkingSupported && (!string.IsNullOrEmpty(account.ScreenshotsLocation) || Settings.GuildWars2.VirtualUserPath.HasValue);
        }

        private static bool Move(string from, string to)
        {
            if (!File.Exists(from))
                return false;
            if (File.Exists(to))
                File.Delete(to);
            Util.FileUtil.MoveFile(from, to, false, true);
            return true;
        }

        private static bool CreateProfile(Settings.IGw2Account account, PathData pd)
        {
            var path = pd.GetProfilePath(PathData.SpecialPath.ProfileRoot, account);

            if (!Directory.Exists(path))
            {
                if (pd.isBasic)
                {
                    Directory.CreateDirectory(path);

                    return true;
                }
                else if (pd.IsAppDataInUserFolder)
                {
                    Directory.CreateDirectory(pd.GetProfilePath(PathData.SpecialPath.Gw2AppData, account));

                    return true;
                }
            }

            return false;
        }

        private static bool CreateProfileOrThrow(Settings.IGw2Account account, PathData pd)
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

        private static void UpdateProfile(Settings.IGw1Account account)
        {
            var defaultPath = GetDefaultPath(FileType.GwDat);
            defaultPath = Path.GetDirectoryName(defaultPath);

            var path = Path.GetDirectoryName(account.DatFile.Path);
            string linkFrom, linkTo;

            linkFrom = Path.Combine(path, GW1_SCREENS_FOLDER_NAME);
            if (!string.IsNullOrEmpty(account.ScreenshotsLocation))
                linkTo = account.ScreenshotsLocation;
            else
                linkTo = Path.Combine(defaultPath, GW1_SCREENS_FOLDER_NAME);

            try
            {
                if (Directory.Exists(linkFrom))
                    Directory.Delete(linkFrom);
                Windows.Symlink.CreateJunction(linkFrom, linkTo);
            }
            catch { }

            linkFrom = Path.Combine(path, GW1_TEMPLATES_FOLDER_NAME);
            linkTo = Path.Combine(defaultPath, GW1_TEMPLATES_FOLDER_NAME);

            try
            {
                if (Directory.Exists(linkFrom))
                    Directory.Delete(linkFrom);
                Windows.Symlink.CreateJunction(linkFrom, linkTo);
            }
            catch { }

            foreach (var fn in new string[] { "GwLoginClient.dll", Path.GetFileName(Settings.GuildWars1.Path.Value) })
            {
                linkFrom = Path.Combine(path, fn);
                if (File.Exists(linkFrom))
                    continue;

                linkTo = Path.Combine(defaultPath, fn);

                try
                {
                    Windows.Symlink.CreateHardLink(linkFrom, linkTo);
                    continue;
                }
                catch { }

                try
                {
                    File.Copy(linkFrom, linkTo);
                }
                catch { }
            }
        }

        private static bool IsBasicProfile(Settings.IGw2Account account, PathData pd)
        {
            return File.Exists(Path.Combine(pd.GetProfilePath(PathData.SpecialPath.ProfileRoot, account), DAT_NAME));
        }

        private static void ConvertProfile(Settings.IGw2Account account, PathData pd)
        {
            var root = pd.GetProfilePath(PathData.SpecialPath.ProfileRoot, account);

            if (!pd.isBasic)
            {
                RecoverFiles(root, pd);
            }

            DeleteProfile(root);
        }

        private static void UpdateProfile(Settings.IGw2Account account, Security.Impersonation.IIdentity identity, PathData pd)
        {
            //note: added links must be mirrored in DeleteProfile

            var isBasic = pd.isBasic;

            if (isBasic != IsBasicProfile(account, pd))
            {
                ConvertProfile(account, pd);
            }

            if (pd.isBasic)
            {
                Directory.CreateDirectory(pd.GetProfilePath(PathData.SpecialPath.ProfileRoot, account));
            }
            else
            {
                Directory.CreateDirectory(pd.GetProfilePath(PathData.SpecialPath.Gw2AppData, account));
            }

            var isCurrentUser = Util.Users.IsCurrentUser(account.WindowsAccount);
            if (!isCurrentUser) //different users will need permision to access GW2's appdata and documents
            {
                Util.FileUtil.AllowFolderAccess(pd.GetUserPath(PathData.SpecialPath.Gw2AppData), System.Security.AccessControl.FileSystemRights.Modify);
                Util.FileUtil.AllowFolderAccess(DataPath.AppDataAccountData, System.Security.AccessControl.FileSystemRights.Modify);
            }

            if (pd.IsAppDataInUserFolder || isBasic)
            {
                #region Local.dat

                if (account.DatFile != null && !string.IsNullOrEmpty(account.DatFile.Path))
                {
                    var dat = account.DatFile.Path;
                    var fi = new FileInfo(pd.GetProfilePath(PathData.SpecialPath.LocalDatFile, account));

                    if (!fi.FullName.Equals(dat, StringComparison.OrdinalIgnoreCase))
                    {
                        var link = IsDataLinkingSupported;

                        if (fi.Exists)
                        {
                            if (!RecoverFile(FileType.Dat, fi.FullName))
                                fi.Delete();
                        }

                        if (link)
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
                            {
                                account.DatFile.IsInitialized = false;
                                File.WriteAllBytes(fi.FullName, new byte[0]);
                            }
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
                    var fi = new FileInfo(pd.GetProfilePath(PathData.SpecialPath.GfxSettingsFile, account));

                    if (!fi.FullName.Equals(gfx, StringComparison.OrdinalIgnoreCase))
                    {
                        var link = IsDataLinkingSupported;

                        if (fi.Exists)
                        {
                            if (!RecoverFile(FileType.Gfx, fi.FullName))
                                fi.Delete();
                        }

                        if (link)
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

                if (IsFolderLinkingSupported)
                {
                    #region Link Coherent Dumps

                    MakeJunction(Path.Combine(pd.GetProfilePath(PathData.SpecialPath.Gw2AppData, account), COHERENT_DUMPS_FOLDER_NAME), DataPath.AppDataAccountDataTemp, true, true, true); //default: Path.Combine(pd.GetUserPath(PathData.SpecialPath.Gw2AppData), COHERENT_DUMPS_FOLDER_NAME)

                    #endregion

                    if (!pd.isBasic)
                    {
                        #region Link LocalAppData

                        if (pd.IsLocalAppDataInUserFolder)
                        {
                            MakeJunction(pd.GetProfilePath(PathData.SpecialPath.LocalAppData, account), pd.GetUserPath(PathData.SpecialPath.LocalAppData), true, false, true);
                        }

                        #endregion

                        #region Link applicable RoamingAppData folders

                        var userappdata = pd.GetUserPath(PathData.SpecialPath.RoamingAppData);
                        var profileappdata = pd.GetProfilePath(PathData.SpecialPath.RoamingAppData, account);

                        foreach (var foldername in new string[] { "Mozilla" })
                        {
                            var path = Path.Combine(userappdata, foldername);
                            if (Directory.Exists(path))
                                MakeJunction(Path.Combine(profileappdata, foldername), path, true, false, true);
                        }

                        #endregion
                    }
                }

                if (pd.IsDocumentsInUserFolder)
                {
                    if (pd.isBasic)
                    {
                        if (identity != null && IsFolderLinkingSupported)
                        {
                            IDisposable impersonation = null;
                            try
                            {
                                impersonation = identity.Impersonate();

                                var impersonated_pd = new PathData(pd.isBasic);
                                var impersonated_gw2documents = impersonated_pd.GetUserPath(PathData.SpecialPath.Gw2Documents);
                                var gw2documents = pd.GetUserPath(PathData.SpecialPath.Gw2Documents);

                                if (!Directory.Exists(impersonated_gw2documents))
                                {
                                    Windows.Symlink.CreateJunction(impersonated_gw2documents, gw2documents);
                                    Util.FileUtil.AllowFolderAccess(gw2documents, System.Security.AccessControl.FileSystemRights.Modify);
                                }
                            }
                            catch
                            {
                            }
                            finally
                            {
                                if (impersonation != null)
                                    impersonation.Dispose();
                            }
                        }
                    }
                    else
                    {
                        if (IsFolderLinkingSupported)
                        {
                            #region Link Documents or subfolders

                            var usergw2documents = pd.GetUserPath(PathData.SpecialPath.Gw2Documents);

                            string linkTo;
                            var isCustom = !string.IsNullOrEmpty(linkTo = account.ScreenshotsLocation) || !string.IsNullOrEmpty(linkTo = Settings.GuildWars2.ScreenshotsLocation.Value);

                            if (!isCustom && !isCurrentUser)
                            {
                                Util.FileUtil.AllowFolderAccess(usergw2documents, System.Security.AccessControl.FileSystemRights.Modify);
                                linkTo = Path.Combine(usergw2documents, SCREENS_FOLDER_NAME);
                                isCustom = true;
                            }

                            var localdocuments = pd.GetProfilePath(PathData.SpecialPath.Documents, account);
                            var localgw2documents = pd.GetProfilePath(PathData.SpecialPath.Gw2Documents, account);
                            var localscreens = Path.Combine(localgw2documents, SCREENS_FOLDER_NAME);

                            if (Directory.Exists(localdocuments))
                            {
                                //the documents folder should either completely link to the real documents
                                //or only include the guild wars 2 folder, which has screens linking elsewhere
                                var deleteDocs = true;

                                if (File.GetAttributes(localdocuments).HasFlag(FileAttributes.ReparsePoint))
                                {
                                    Directory.Delete(localdocuments);
                                    deleteDocs = false;
                                }
                                else if (Directory.Exists(localscreens))
                                {
                                    if (File.GetAttributes(localscreens).HasFlag(FileAttributes.ReparsePoint))
                                    {
                                        Directory.Delete(localscreens);
                                    }
                                    else
                                    {
                                        //this is a real folder - only delete if it's empty
                                        try
                                        {
                                            RecoverScreenshots(localscreens, Path.Combine(pd.GetUserPath(PathData.SpecialPath.Gw2Documents), SCREENS_FOLDER_NAME));
                                            Util.FileUtil.DeleteDirectory(localscreens);
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
                                    Util.FileUtil.DeleteDirectory(localdocuments);
                                }
                            }

                            try
                            {
                                if (isCustom)
                                {
                                    Directory.CreateDirectory(localgw2documents);
                                    Windows.Symlink.CreateJunction(localscreens, linkTo);

                                    if (!isCurrentUser && !linkTo.StartsWith(usergw2documents, StringComparison.OrdinalIgnoreCase))
                                        Util.FileUtil.AllowFolderAccess(linkTo, System.Security.AccessControl.FileSystemRights.Modify);

                                    foreach (var path in Directory.GetDirectories(usergw2documents))
                                    {
                                        var name = Path.GetFileName(path);
                                        if (name.Equals(SCREENS_FOLDER_NAME, StringComparison.OrdinalIgnoreCase))
                                            continue;
                                        var local = Path.Combine(localgw2documents, name);

                                        if (Directory.Exists(local))
                                        {
                                            try
                                            {
                                                Directory.Delete(local);
                                            }
                                            catch
                                            {
                                                continue;
                                            }
                                        }

                                        Windows.Symlink.CreateJunction(local, path);
                                    }
                                }
                                else
                                {
                                    Windows.Symlink.CreateJunction(localdocuments, Path.GetDirectoryName(usergw2documents));
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
                            Directory.CreateDirectory(pd.GetProfilePath(PathData.SpecialPath.Gw2Documents, account));
                        }
                    }
                }
            }
            else
            {
                if (IsVirtualModeSupported)
                {
                    throw new NotSupportedException("Unknown user profile structure");
                }
                else
                {
                    throw new NotSupportedException("Virtual mode is not supported under the current user");
                }
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
        public static string GetPath(SpecialPath type, Settings.IGw2Account account)
        {
            return GetPath(type, account, new PathData(account));
        }

        /// <summary>
        /// Return the default path of the specified type
        /// </summary>
        /// <param name="type">The path to retrieve</param>
        public static string GetPath(SpecialPath type)
        {
            return GetPath(type, null, new PathData());
        }

        private static string GetPath(SpecialPath type, Settings.IGw2Account account, PathData pd)
        {
            switch (type)
            {
                case SpecialPath.AppData:

                    if (account == null)
                        return pd.GetUserPath(PathData.SpecialPath.Gw2AppData);

                    return pd.GetProfilePath(PathData.SpecialPath.Gw2AppData, account);

                case SpecialPath.Dumps:
                    
                    if (account == null)
                        return Path.Combine(pd.GetUserPath(PathData.SpecialPath.Gw2AppData), COHERENT_DUMPS_FOLDER_NAME);

                    return Path.Combine(pd.GetProfilePath(PathData.SpecialPath.Gw2AppData, account), COHERENT_DUMPS_FOLDER_NAME);

                case SpecialPath.Documents:
                    
                    if (account == null)
                        return pd.GetUserPath(PathData.SpecialPath.Gw2Documents);

                    return pd.GetProfilePath(PathData.SpecialPath.Gw2Documents, account);

                case SpecialPath.Music:
                    
                    if (account == null)
                        return Path.Combine(pd.GetUserPath(PathData.SpecialPath.Gw2Documents), MUSIC_FOLDER_NAME);

                    return Path.Combine(pd.GetProfilePath(PathData.SpecialPath.Gw2Documents, account), MUSIC_FOLDER_NAME);

                case SpecialPath.Screens:

                    if (account == null)
                        return Path.Combine(pd.GetUserPath(PathData.SpecialPath.Gw2Documents), SCREENS_FOLDER_NAME);

                    return Path.Combine(pd.GetProfilePath(PathData.SpecialPath.Gw2Documents, account), SCREENS_FOLDER_NAME);
            }

            return null;
        }

        public static bool IsDefaultPath(FileType type, string path)
        {
            return IsDefaultPath(type, path, new PathData());
        }

        public static string GetDefaultPath(FileType type)
        {
            return GetDefaultPath(type, new PathData());
        }

        private static string GetDefaultPath(FileType type, PathData pd)
        {
            switch (type)
            {
                case FileType.Dat:

                    return pd.GetUserPath(PathData.SpecialPath.LocalDatFile);

                case FileType.Gfx:

                    return pd.GetUserPath(PathData.SpecialPath.GfxSettingsFile);

                case FileType.GwDat:

                    return Path.Combine(Path.GetDirectoryName(Settings.GuildWars1.Path.Value), GW1_DAT_NAME);
            }

            return null;
        }

        /// <summary>
        /// Updates files under the path
        /// </summary>
        /// <param name="current">The current path of the folder</param>
        /// <param name="previous">The previous path of the folder</param>
        private static void OnPathChanged(string current, string previous)
        {
            var l = previous.Length;

            foreach (var f in GetFiles(FileType.Dat))
            {
                var path = f.Path;

                if (path.Length > l && (path[l] == Path.DirectorySeparatorChar || path[l] == Path.AltDirectorySeparatorChar) && path.Substring(0, l).Equals(previous, StringComparison.OrdinalIgnoreCase))
                {
                    f.Path = Path.Combine(current, path.Substring(l + 1));
                    OnFilePathMoved(FileType.Dat, f);
                }
            }

            foreach (var f in GetFiles(FileType.Gfx))
            {
                var path = f.Path;

                if (path.Length > l && (path[l] == Path.DirectorySeparatorChar || path[l] == Path.AltDirectorySeparatorChar) && path.Substring(0, l).Equals(previous, StringComparison.OrdinalIgnoreCase))
                {
                    f.Path = Path.Combine(current, path.Substring(l + 1));
                    OnFilePathMoved(FileType.Gfx, f);
                }
            }
        }

        private static bool IsDefaultPath(FileType type, string path, PathData pd)
        {
            switch (type)
            {
                case FileType.Gfx:
                case FileType.Dat:

                    string defaultName;
                    if (type == FileType.Gfx)
                        defaultName = pd.DefaultGfxFileName;
                    else
                        defaultName = DAT_NAME;

                    if (Path.GetFileName(path).Equals(defaultName, StringComparison.OrdinalIgnoreCase))
                    {
                        path = Path.GetDirectoryName(path);
                        var n = Path.GetFileName(path);

                        if (n.Equals(GW2_FOLDER_NAME, StringComparison.OrdinalIgnoreCase) || n.Equals(GW2_BASIC_FOLDER_NAME, StringComparison.OrdinalIgnoreCase))
                        {
                            n = Path.GetDirectoryName(path);
                            if (n.Equals(pd.GetUserPath(PathData.SpecialPath.AppData), StringComparison.OrdinalIgnoreCase))
                            {
                                return true;
                            }
                        }
                    }
                    
                    return false;

                case FileType.GwDat:

                    return path.Equals(GetDefaultPath(type, pd), StringComparison.OrdinalIgnoreCase);
            }

            return false;
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
        /// <param name="path">The path to search for or * to return the first file that exists</param>
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
        public static IEnumerable<Settings.IGw2Account> FindAccounts(FileType type, Settings.IFile file)
        {
            var count = file.References;

            if (count > 0)
            {
                foreach (var a in Util.Accounts.GetGw2Accounts())
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
                                    Delete(type, path, new PathData(pd.isBasic));
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

                    var gfxType = pd.GetPathType(path, pd.DefaultGfxFileName);

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
            switch (account.Type)
            {
                case Settings.AccountType.GuildWars1:

                    Delete((Settings.IGw1Account)account);

                    break;
                case Settings.AccountType.GuildWars2:

                    Delete((Settings.IGw2Account)account);

                    break;
            }
        }

        private static void Delete(Settings.IGw1Account account)
        {
            if (account.DatFile != null)
            {
                if (account.DatFile.References == 1 && Settings.GuildWars1.Path.HasValue && File.Exists(Settings.GuildWars1.Path.Value))
                {
                    var defaultPath = Path.GetDirectoryName(Settings.GuildWars1.Path.Value);
                    var localPath = Path.Combine(defaultPath, LOCALIZED_EXE_FOLDER_NAME);
                    var d = Path.GetDirectoryName(account.DatFile.Path);

                    //only paths under the auto-generated path should be deleted; everything else was manually created

                    if (d.StartsWith(localPath, StringComparison.OrdinalIgnoreCase) && !d.Equals(defaultPath, StringComparison.OrdinalIgnoreCase))
                    {
                        try
                        {
#warning not checking for real screenshots/templates
                            Util.FileUtil.DeleteDirectory(d);
                            try
                            {
                                var parent = Path.GetDirectoryName(d);
                                if (Path.GetFileName(parent).Equals(LOCALIZED_EXE_FOLDER_NAME))
                                    Directory.Delete(parent);
                            }
                            catch { }
                        }
                        catch (Exception e)
                        {
                            Util.Logging.Log(e);
                        }
                    }
                }

                account.DatFile = null;
            }
        }

        private static void Delete(Settings.IGw2Account account)
        {
            var pd = new PathData(account);

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

            if (Settings.GuildWars2.LocalizeAccountExecution.Value.HasFlag(Settings.LocalizeAccountExecutionOptions.Enabled))
                DeleteLocalized(account.UID);

            if (IsDataLinkingSupported)
            {
                var path = pd.GetProfilePath(PathData.SpecialPath.ProfileRoot, account);
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

                    root = Path.Combine(pd.GetUserPath(PathData.SpecialPath.Gw2LauncherAccountData), ALT_DATA);

                    break;
                case PathType.DataByAccount:

                    root = pd.GetUserPath(PathData.SpecialPath.Gw2LauncherAccountData);

                    break;
                case PathType.DataByGw2:

                    root = pd.GetUserPath(PathData.SpecialPath.Gw2AppData);

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

        private static void DeleteProfile(Settings.IGw2Account account, PathData pd)
        {
            if (IsDataLinkingSupported)
                DeleteProfile(PathType.DataByAccount, account.UID, pd);
            else if (account.DatFile != null)
                DeleteProfile(PathType.DataByDat, account.DatFile.UID, pd);
        }

        private static void RecoverScreenshots(string from, string to)
        {
            string[] files;
            try
            {
                files = Directory.GetFiles(from, "gw*.*");
            }
            catch
            {
                return;
            }

            if (files.Length > 0)
            {
                to = Path.Combine(to, "recovered");
                Directory.CreateDirectory(to);
            }
            else
            {
                return;
            }

            var exts = new Dictionary<string, ushort>();

            foreach (var f in files)
            {
                var ext = Path.GetExtension(f);
                ushort sid;

                if (!exts.TryGetValue(ext, out sid))
                {
                    var existing = Directory.GetFiles(to, "*" + ext);

                    for (var i = existing.Length - 1; i >= 0; i--)
                    {
                        var last = Path.GetFileNameWithoutExtension(existing[i]);
                        if (ushort.TryParse(last, out sid))
                            break;
                    }
                }

                try
                {
                    File.Move(f, Path.Combine(to, string.Format("{0:00000}" + ext, ++sid)));
                }
                catch { }

                exts[ext] = sid;
            }
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

                    root = Path.Combine(pd.GetUserPath(PathData.SpecialPath.Gw2LauncherAccountData), ALT_DATA, uid.ToString());

                    break;
                case PathType.DataByAccount:

                    root = Path.Combine(pd.GetUserPath(PathData.SpecialPath.Gw2LauncherAccountData), uid.ToString());

                    break;
                case PathType.DataByGw2:

                    root = Path.Combine(pd.GetUserPath(PathData.SpecialPath.Gw2AppData), uid.ToString());

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

                //basic profiles don't have these folders, but could have been left over from changing profiles
                var localdocuments = Path.Combine(root, pd.GetRelativeProfilePath(PathData.SpecialPath.Documents, false));
                var localgw2documents = Path.Combine(root, pd.GetRelativeProfilePath(PathData.SpecialPath.Gw2Documents, false));
                var localscreens = Path.Combine(localgw2documents, SCREENS_FOLDER_NAME);

                if (Directory.Exists(localscreens) && !File.GetAttributes(localscreens).HasFlag(FileAttributes.ReparsePoint) && !File.GetAttributes(localdocuments).HasFlag(FileAttributes.ReparsePoint))
                {
                    RecoverScreenshots(localscreens, Path.Combine(pd.GetUserPath(PathData.SpecialPath.Gw2Documents), SCREENS_FOLDER_NAME));
                }

                if (type == PathType.DataByAccount)
                    RecoverFiles(root, pd);

                DeleteProfile(root);

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
            var displayName = Settings.GuildWars2.VirtualUserPath;
            string target;

            if (pd.isBasic)
                target = pd.GetUserPath(PathData.SpecialPath.ProfileRoot);
            else
                target = pd.GetUserPath(PathData.SpecialPath.Gw2LauncherAccountData);

            try
            {
                Windows.Symlink.CreateJunction(path, target);

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
                if (Util.ProcessUtil.CreateJunction(path, target))
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

        private static bool RecoverFile(FileType type, string path)
        {
            if (!File.Exists(path))
                return false;

            var file = FindFile(type, path);
            if (file == null)
                return false;

            var to = Path.Combine(DataPath.AppDataAccountData, file.UID.ToString() + Path.GetExtension(path));

            Move(path, to);

            file.Path = to;

            OnFilePathMoved(type, file);

            return true;
        }

        private static void RecoverFiles(string path, PathData pd)
        {
            RecoverFile(FileType.Dat, Path.Combine(path, DAT_NAME));
            RecoverFile(FileType.Gfx, Path.Combine(path, pd.DefaultGfxFileName));
        }

        public static bool DeleteProfile(string path)
        {
            byte count = 0;

            do
            {
                try
                {
                    if (Directory.Exists(path))
                        Util.FileUtil.DeleteDirectory(path);

                    return true;
                }
                catch { }

                try
                {
                    var entries = 0;

                    foreach (var f in Directory.EnumerateFileSystemEntries(path, "*", SearchOption.AllDirectories))
                    {
                        entries++;
                        var a1 = File.GetAttributes(f);
                        var a2 = a1 & ~(FileAttributes.ReadOnly | FileAttributes.System);
                        if (a2 != a1)
                            File.SetAttributes(f, a2);
                    }

                    if (entries == 0)
                        return true;
                }
                catch { }
            }
            while (++count < 2);

            return false;
        }

        public static IProfileInformation Activate(Settings.IGw1Account account)
        {
            if (!string.IsNullOrEmpty(account.WindowsAccount) && !Util.Users.IsCurrentUser(account.WindowsAccount))
            {
                VerifyUserInitialized();
            }

            if (account.DatFile == null)
            {
                throw new Exception("No Gw.dat file selected");
            }

            if (account.PendingFiles)
            {
                UpdateProfile(account);
                account.PendingFiles = false;
            }

            if (File.Exists(account.DatFile.Path))
            {
                try
                {
                    using (var f = File.Open(account.DatFile.Path, FileMode.Open, FileAccess.Read, FileShare.None))
                    {

                    }
                }
                catch (IOException e)
                {
                    switch (e.HResult & 0xFFFF)
                    {
                        case 32: //ERROR_SHARING_VIOLATION

                            throw new Exception("Gw.dat is in use; exclusive access is required");
                    }

                    throw;
                }
            }
            else if (account.DatFile.IsInitialized)
            {
                account.DatFile.IsInitialized = false;
            }

            return null; //gw1 doesn't use special folders
        }

        private static void ActivateBasic(Settings.IGw2Account account, PathData pd, bool setpermissions, int timeout)
        {
            //activating a basic profile has a race condition - active accounts can overwrite the folder - must ensure that files are read-only during the process
            
            var gw2appdata = Path.Combine(pd.GetUserPath(PathData.SpecialPath.AppData), GW2_FOLDER_NAME);
            var gw2altappdata = Path.Combine(pd.GetUserPath(PathData.SpecialPath.AppData), GW2_BASIC_FOLDER_NAME);
            var profileappdata = pd.GetProfilePath(PathData.SpecialPath.Gw2AppData, account);
            var limit = DateTime.UtcNow.AddMilliseconds(timeout);

            if (gfxMonitor != null)
                gfxMonitor.Remove(gw2appdata);

            if (!Directory.Exists(gw2altappdata))
            {
                bool move;

                try
                {
                    if (Directory.Exists(gw2appdata))
                    {
                        move = (File.GetAttributes(gw2appdata) & FileAttributes.ReparsePoint) != FileAttributes.ReparsePoint;
                        if (!move)
                            Util.FileUtil.DeleteDirectory(gw2appdata, true);
                    }
                    else
                    {
                        Directory.CreateDirectory(gw2altappdata);
                        move = false;
                    }
                }
                catch
                {
                    move = false;
                }

                if (move)
                {
                    Directory.Move(gw2appdata, gw2altappdata);
                    OnPathChanged(gw2altappdata, gw2appdata);
                }
            }

            do
            {
                //note: may have to retry a few times, as multiple clients could be writing (temporary files) to the folder

                try
                {
                    if (Directory.Exists(gw2appdata))
                    {
                        if ((File.GetAttributes(gw2appdata) & FileAttributes.ReparsePoint) == FileAttributes.ReparsePoint)
                            Directory.Delete(gw2appdata);
                        else
                            Util.FileUtil.DeleteDirectory(gw2appdata);
                    }

                    Windows.Symlink.CreateJunction(gw2appdata, profileappdata);

                    if (setpermissions)
                    {
                        Util.FileUtil.AllowFolderAccess(gw2appdata, System.Security.AccessControl.FileSystemRights.Modify);
                    }

                    break;
                }
                catch (Exception e)
                {
                    Util.Logging.Log(e);

                    if (DateTime.UtcNow > limit)
                        throw;
                }

                System.Threading.Thread.Sleep(100);
            }
            while (true);
        }
        
        public static void ActivateBasic(Settings.IGw2Account account, Security.Impersonation.IIdentity identity, IProfileInformation profile)
        {
            var p = (ProfileData)profile;
            var gfx = FileLocker.Lock(account.GfxFile.Path, 5000);

            using (Security.Impersonation.Impersonate(identity))
            {
                try
                {
                    ActivateBasic(account, p.Source, identity != null, 5000);
                }
                catch
                {
                    gfx.Dispose();
                    throw;
                }
            }

            if (p.Files == null)
                p.Files = new List<FileLocker.ISharedFile>();
            p.Files.Add(gfx);
        }

        private static bool DeactivateBasicPath(int timeout)
        {
            var pd = new PathData(true);
            var gw2altappdata = Path.Combine(pd.GetUserPath(PathData.SpecialPath.AppData), GW2_BASIC_FOLDER_NAME);

            if (Directory.Exists(gw2altappdata))
            {
                var gw2appdata = Path.Combine(pd.GetUserPath(PathData.SpecialPath.AppData), GW2_FOLDER_NAME);
                var path = Path.Combine(gw2appdata, pd.DefaultGfxFileName);
                var limit = DateTime.UtcNow.AddMilliseconds(timeout);

                while (true)
                {
                    try
                    {
                        if (Directory.Exists(gw2appdata))
                        {
                            Util.FileUtil.DeleteDirectory(gw2appdata, true);
                        }

                        if (Directory.Exists(gw2altappdata))
                        {
                            Directory.Move(gw2altappdata, gw2appdata);
                            OnPathChanged(gw2appdata, gw2altappdata);
                            return true;
                        }

                        break;
                    }
                    catch 
                    {
                        if (DateTime.UtcNow > limit)
                            break;
                    }

                    System.Threading.Thread.Sleep(100);

                    if (Util.Users.IsCurrentEnvironmentUser())
                    {
                        if (Launcher.GetActiveProcessCount(Launcher.AccountType.GuildWars2) > 0)
                            break;
                    }
                    else
                    {
                        if (Launcher.IsUserActive(Launcher.AccountType.GuildWars2, Environment.UserName))
                            break;
                    }
                }
            }

            return false;
        }

        private static void CheckPending(Settings.IDatFile dat)
        {
            if (dat.IsPending && (Settings.GuildWars2.DatUpdaterEnabled.Value || Settings.GuildWars2.UseCustomGw2Cache.Value))
            {
                if (Settings.GuildWars2.DatUpdaterEnabled.Value && (!File.Exists(dat.Path) || new FileInfo(dat.Path).Length == 0))
                {
                    try
                    {
                        var files = new List<Settings.Values<int, Settings.IDatFile>>();

                        foreach (var file in GetFiles(FileType.Dat))
                        {
                            if (file == dat)
                                continue;

                            int length;
                            try
                            {
                                length = (int)(new FileInfo(file.Path).Length);
                                if (length == 0)
                                    continue;
                            }
                            catch
                            {
                                continue;
                            }

                            files.Add(new Settings.Values<int, Settings.IDatFile>()
                                {
                                    value1 = length,
                                    value2 = (Settings.IDatFile)file,
                                });
                        }

                        if (files.Count == 0)
                            return;

                        files.Sort(
                            delegate(Settings.Values<int, Settings.IDatFile> a, Settings.Values<int, Settings.IDatFile>b)
                            {
                                return a.value1.CompareTo(b.value1);
                            });

                        var build = Tools.Gw2Build.Build;

                        foreach (var f in files)
                        {
                            if (Tools.DatUpdater.GetBuild(f.value2) == build)
                            {
                                Tools.DatUpdater.Create(f.value2, dat);

                                break;
                            }
                        }
                    }
                    catch { }
                }
                else if (Settings.GuildWars2.UseCustomGw2Cache.Value)
                {
                    try
                    {
                        if (Tools.DatUpdater.Create().UpdateCache(dat))
                            dat.IsPending = false;
                    }
                    catch { }
                }
            }
        }

        /// <summary>
        /// Activates any data files for the account
        /// </summary>
        /// <returns>Paths used by the profile, or null if a profile isn't needed</returns>
        public static IProfileInformation Activate(Settings.IGw2Account account, Security.Impersonation.IIdentity identity)
        {
            var pd = new PathData(account);

            if (pd.isBasic && !IsFolderLinkingSupported)
                throw new NotSupportedException();

            var requiresCustom = pd.isBasic || IsCustomized(account);
            var requiresDefaultPaths = account.Proxy == Settings.LaunchProxy.Steam;
            var datType = PathType.Unknown;
            var gfxType = PathType.Unknown;

            #region Custom username

            string profileRoot = null;

            if (IsFolderLinkingSupported && !requiresDefaultPaths)
            {
                Func<string, string> getPath = delegate(string v)
                {
                    if (Path.IsPathRooted(v) && Path.GetPathRoot(v).Equals(Path.GetPathRoot(pd.GetUserPath(PathData.SpecialPath.ProfileRoot)), StringComparison.OrdinalIgnoreCase))
                    {
                        return Path.GetFullPath(v);
                    }

                    return Path.Combine(Path.GetDirectoryName(pd.GetUserPath(PathData.SpecialPath.ProfileRoot)), v);
                };

                var displayName = Settings.GuildWars2.VirtualUserPath;
                if (displayName.IsPending)
                {
                    var createUser = displayName.HasValue;

                    if (!string.IsNullOrEmpty(displayName.ValueCommit))
                    {
                        var di = new DirectoryInfo(getPath(displayName.ValueCommit));

                        if (di.Exists && di.Attributes.HasFlag(FileAttributes.ReparsePoint))
                        {
                            //don't delete if it's in use
                            if (Launcher.GetActiveProcessCount(Launcher.AccountType.GuildWars2) == 0)
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

                    FlagAccountsForPendingFiles(Settings.AccountType.GuildWars2);
                }
                else if (displayName.HasValue)
                {
                    profileRoot = getPath(displayName.Value);

                    if (!Directory.Exists(profileRoot) && !CreateProfileRoot(pd, profileRoot))
                    {
                        //failed to create the folder, commiting the path to null to reflag it as pending for next time
                        //var v = displayName.Value;
                        //displayName.Clear();
                        //displayName.Commit();
                        //displayName.Value = v;
                        displayName.SetPending();
                        profileRoot = null;
                    }
                }
            }

            #endregion

            string customDatPath, customGfxPath;

            if (!string.IsNullOrEmpty(account.WindowsAccount) && !Util.Users.IsCurrentUser(account.WindowsAccount))
            {
                VerifyUserInitialized();
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
                        var gfx = Path.Combine(Path.GetDirectoryName(path), pd.DefaultGfxFileName);
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
                    var defaultPath = pd.GetUserPath(PathData.SpecialPath.LocalDatFile);
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
                else if (!File.Exists(account.DatFile.Path))
                {
                    var path = Path.GetDirectoryName(account.DatFile.Path);
                    var l = GW2_BASIC_FOLDER_NAME.Length;
                    string n;

                    if (path.Substring(path.Length - l, l).Equals(GW2_BASIC_FOLDER_NAME, StringComparison.OrdinalIgnoreCase))
                        n = GW2_FOLDER_NAME;
                    else
                        n = GW2_BASIC_FOLDER_NAME;

                    var gw2altappdata = Path.Combine(pd.GetUserPath(PathData.SpecialPath.AppData), n);
                    if (File.Exists(Path.Combine(gw2altappdata, pd.GetRelativeProfilePath(PathData.SpecialPath.LocalDatFile))))
                    {
                        if ((File.GetAttributes(gw2altappdata) & FileAttributes.ReparsePoint) == 0)
                        {
                            OnPathChanged(gw2altappdata, path);// Path.Combine(pd.GetUserPath(PathData.SpecialPath.AppData), GW2_FOLDER_NAME));
                        }
                    }
                }

                if (!pd.isBasic && pd.IsCurrentDefaultGw2DirectoryNameBasic && DeactivateBasicPath(1000))
                {
                    pd.RefreshCurrentDefaultGw2DirectoryName();
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
                gfxType = pd.GetPathType(path, customGfxPath, pd.DefaultGfxFileName);
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
                    var defaultPath = pd.GetUserPath(PathData.SpecialPath.GfxSettingsFile);
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
                else if (!File.Exists(account.GfxFile.Path))
                {
                    var path = Path.GetDirectoryName(account.GfxFile.Path);
                    var l = GW2_BASIC_FOLDER_NAME.Length;
                    string n;

                    if (path.Substring(path.Length - l, l).Equals(GW2_BASIC_FOLDER_NAME, StringComparison.OrdinalIgnoreCase))
                        n = GW2_FOLDER_NAME;
                    else
                        n = GW2_BASIC_FOLDER_NAME;

                    var gw2altappdata = Path.Combine(pd.GetUserPath(PathData.SpecialPath.AppData), n);
                    if (File.Exists(Path.Combine(gw2altappdata, pd.GetRelativeProfilePath(PathData.SpecialPath.GfxSettingsFile))))
                    {
                        if ((File.GetAttributes(gw2altappdata) & FileAttributes.ReparsePoint) == 0)
                        {
                            OnPathChanged(gw2altappdata, path);
                        }
                    }
                }

                if (!pd.isBasic && pd.IsCurrentDefaultGw2DirectoryNameBasic && DeactivateBasicPath(1000))
                {
                    pd.RefreshCurrentDefaultGw2DirectoryName();
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
                        string from, to;

                        if (pd.isBasic)
                        {
                            from = Path.Combine(pd.GetUserPath(PathData.SpecialPath.Gw2Documents), DAT_NAME);
                            to = Path.Combine(pd.GetUserPath(PathData.SpecialPath.Gw2AppData), DAT_NAME);
                        }
                        else
                        {
                            from = Path.Combine(pd.GetProfilePath(PathData.SpecialPath.Gw2Documents, account), DAT_NAME);
                            to = customDatPath;
                        }

                        //documents acts as an override, overwrite the original
                        try
                        {
                            if (Move(from, to))
                            {
                                if (pd.isBasic)
                                {
                                    var dat = FindFile(FileType.Dat, to);
                                    if (dat != null)
                                        OnFilePathMoved(FileType.Dat, dat);
                                }
                                else
                                {
                                    OnFilePathMoved(FileType.Dat, account.DatFile);
                                }
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

                    IDisposable impersonation = null;
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
                            if (Move(Path.Combine(pd.GetUserPath(PathData.SpecialPath.Gw2Documents), DAT_NAME), Path.Combine(pd.GetUserPath(PathData.SpecialPath.Gw2AppData), DAT_NAME)))
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
                        throw new Exception("Unable to move existing " + pd.DefaultGfxFileName);
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
                isPending = !File.Exists(pd.GetProfilePath(PathData.SpecialPath.LocalDatFile, account));
            }

            if (isPending)
            {
                if (requiresCustom)
                {
                    UpdateProfile(account, identity, pd);
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

            CheckPending(account.DatFile);

            if (pd.isBasic)
            {
                if (identity != null)
                {
                    using (identity.Impersonate())
                    {
                        var pdi = new PathData(pd.isBasic);
                        var profile = new ProfileData(pdi);

                        profile.UserProfile = pdi.GetUserPath(PathData.SpecialPath.ProfileRoot);
                        profile.AppData = pdi.GetUserPath(PathData.SpecialPath.AppData);

                        return profile;
                    }
                }
                else if (IsDataLinkingSupported && profileRoot != null)
                {
                    //note that other users can't use this, since in basic mode, the folder links to the current user's directory, 
                    //thus allowing it would require changing permissions on the user's folders

                    var profile = new ProfileData(pd);

                    profile.UserProfile = profileRoot;
                    profile.AppData = Path.Combine(profileRoot, pd.GetRelativeProfilePath(PathData.SpecialPath.AppData));

                    return profile;
                }
                else
                {
                    return new ProfileData(pd);
                }
            }
            else if (requiresCustom)
            {
                var profile = new ProfileData(pd);

                if (IsDataLinkingSupported && profileRoot != null)
                {
                    profile.UserProfile = Path.Combine(profileRoot, account.UID.ToString());
                }
                else
                {
                    profile.UserProfile = pd.GetProfilePath(PathData.SpecialPath.ProfileRoot, account);
                }

                profile.AppData = Path.Combine(profile.UserProfile, pd.GetRelativeProfilePath(PathData.SpecialPath.AppData));

                return profile;
            }
            else
            {
                return null;
            }
        }

        public static void DeactivateBasic(Security.Impersonation.IIdentity identity, IProfileInformation profile = null)
        {
            using (Security.Impersonation.Impersonate(identity))
            {
                PathData pd;

                if (profile != null)
                {
                    pd = ((ProfileData)profile).Source;
                }
                else
                {
                    pd = new PathData(true);
                }

                var appdata = pd.GetUserPath(PathData.SpecialPath.AppData);
                var gw2appdata = Path.Combine(appdata, GW2_FOLDER_NAME);

                try
                {
                    Directory.Delete(gw2appdata);

                    //Directory.CreateDirectory(gw2appdata);
                    //if (identity != null)
                    //    Util.FileUtil.AllowFolderAccess(gw2appdata, System.Security.AccessControl.FileSystemRights.Modify);

                    if (gfxMonitor == null)
                    {
                        gfxMonitor = new GfxMonitor();
                        Launcher.AllQueuedLaunchesComplete += Launcher_AllQueuedLaunchesComplete;
                    }
                    gfxMonitor.Add(gw2appdata, identity != null ? Environment.UserName : null);
                }
                catch { }
            }
        }

        public static void Deactivate(Settings.AccountType type)
        {
            if (type == Settings.AccountType.GuildWars2)
            {
                var o = Settings.GuildWars2.ProfileOptions.Value;

                if ((o & Settings.ProfileModeOptions.RestoreOriginalPath) == Settings.ProfileModeOptions.RestoreOriginalPath)
                {
                    DeactivateBasicPath(1000);
                }
            }
        }

        public static void Uninstall()
        {
            if (Settings.GuildWars2.VirtualUserPath.HasValue)
            {
                try
                {
                    var pd = new PathData();
                    var path = Settings.GuildWars2.VirtualUserPath.ValueCommit;

                    if (Path.IsPathRooted(path) && Path.GetPathRoot(path).Equals(Path.GetPathRoot(pd.GetUserPath(PathData.SpecialPath.ProfileRoot)), StringComparison.OrdinalIgnoreCase))
                    {
                        path = Path.GetFullPath(path);
                    }
                    else
                    {
                        path = Path.Combine(Path.GetDirectoryName(pd.GetUserPath(PathData.SpecialPath.ProfileRoot)), path);
                    }

                    var di = new DirectoryInfo(path);

                    if (di.Exists && di.Attributes.HasFlag(FileAttributes.ReparsePoint))
                    {
                        di.Delete();
                    }
                }
                catch { }
            }

            DeactivateBasicPath(1000);
        }

        public static void Deactivate(Settings.IAccount account)
        {
            if (account == null)
                return;

            if (account.Type == Settings.AccountType.GuildWars2)
            {
                if (Settings.GuildWars2.ProfileMode.Value == Settings.ProfileMode.Basic && Settings.GuildWars2.ProfileOptions.HasValue)
                {
                    var pd = new PathData((Settings.IGw2Account)account);
                    var o = Settings.GuildWars2.ProfileOptions.Value;

                    if ((o & Settings.ProfileModeOptions.ClearTemporaryFiles) == Settings.ProfileModeOptions.ClearTemporaryFiles)
                    {
                        DeleteProfile(pd.GetProfilePath(PathData.SpecialPath.ProfileRoot, (Settings.IGw2Account)account));
                        account.PendingFiles = true;
                    }

                    if ((o & Settings.ProfileModeOptions.RestoreOriginalPath) == Settings.ProfileModeOptions.RestoreOriginalPath)
                    {
                        if (!Util.Users.IsCurrentUser(account.WindowsAccount) && !Launcher.IsUserActive(Launcher.AccountType.GuildWars2, account.WindowsAccount))
                        {
                            try
                            {
                                using (var impersonation = Security.Impersonation.Impersonate(account.WindowsAccount, Security.Credentials.GetPassword(account.WindowsAccount)))
                                {
                                    DeactivateBasicPath(1000);
                                }
                            }
                            catch { }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Activates a profile for the dat file
        /// </summary>
        /// <param name="path">The dat file</param>
        /// <returns></returns>
        public static IProfileInformation Activate(string dat)
        {
            var pd = new PathData(false);
            if (!pd.IsAppDataInUserFolder)
                throw new NotSupportedException();
            var path = Util.FileUtil.GetTemporaryFolderName(Path.GetDirectoryName(dat), "temp-{0}");
            if (path == null)
                throw new IOException();

            var appdata = Path.Combine(path, pd.GetRelativeProfilePath(PathData.SpecialPath.Gw2AppData));
            var fi = new FileInfo(Path.Combine(appdata, DAT_NAME));

            try
            {
                Directory.CreateDirectory(appdata);
                if (pd.IsDocumentsInUserFolder)
                    Directory.CreateDirectory(Path.Combine(path, pd.GetRelativeProfilePath(PathData.SpecialPath.Gw2Documents)));
                if (fi.Exists)
                    fi.Delete();

                Windows.Symlink.CreateHardLink(fi.FullName, dat);
            }
            catch
            {
                Util.FileUtil.DeleteDirectory(path);
                throw;
            }

            var profile = new ProfileData(pd)
            {
                UserProfile = path,
                AppData = Path.Combine(path, pd.GetRelativeProfilePath(PathData.SpecialPath.AppData))
            };

            return profile;
        }

        public static bool IsGw264Bit
        {
            get
            {
                var b = exebits;

                if (b == 0)
                {
                    exebits = b = Util.FileUtil.GetExecutableBits(Settings.GuildWars2.Path.Value);
                }

                return b == 64;
            }
        }

        public interface ILocalizedPath : IDisposable
        {
            string Path
            {
                get;
            }
        }

        public class LocalizedPath : ILocalizedPath
        {
            public string Path
            {
                get;
                set;
            }

            public FileLocker.ISharedFile[] FileLocks
            {
                get;
                set;
            }

            public void Dispose()
            {
                if (FileLocks != null)
                {
                    foreach (var f in FileLocks)
                    {
                        f.Dispose();
                    }
                    FileLocks = null;
                }
            }
        }

        /// <summary>
        /// Activates the account's specific executable path
        /// </summary>
        /// <param name="fi">Path to Gw2.exe</param>
        public static string ActivateLocalizedPath(Settings.IGw2Account account, FileInfo fi)
        {
            if (!IsGw2LinkingSupported)
                throw new NotSupportedException();

            const byte RESYNC_STATE_REFRESH_ONLY = 2;

            var gw2root = fi.DirectoryName;
            var localroot = Path.Combine(gw2root, LOCALIZED_EXE_FOLDER_NAME);
            var root = Path.Combine(localroot, account.UID.ToString());
            var exe = Path.Combine(root, fi.Name);

            var v = Settings.GuildWars2.LocalizeAccountExecution;
            var onlyBin = v.Value.HasFlag(Settings.LocalizeAccountExecutionOptions.OnlyIncludeBinFolders);
            byte resync = v.Value.HasFlag(Settings.LocalizeAccountExecutionOptions.AutoSync) ? (byte)1 : (byte)0;
            var bin = IsGw264Bit ? "bin64" : "bin";

            if (File.Exists(exe))
            {
                //existing Full mode

                if (onlyBin)
                {
                    if (!DeleteLocalized(account.UID, true))
                    {
                        throw new IOException("Unable to change localized execution while active");
                    }
                }
                else
                {
                    if (!v.IsPending && File.GetLastWriteTimeUtc(exe) == fi.LastWriteTimeUtc)
                    {
                        if (File.GetLastWriteTimeUtc(Path.Combine(root, "Gw2.dat")) == File.GetLastWriteTimeUtc(Path.Combine(gw2root, "Gw2.dat")))
                        {
                            if (resync == 0)
                                return exe;
                            ++resync;
                        }
                    }
                    else
                    {
                        try
                        {
                            File.Delete(exe);
                        }
                        catch { }
                    }
                }
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
            else if (onlyBin)
            {
                //note when using only bins: temp/gw2cache links to the the localized folder and it links back to the temp folder - both sides could be deleted and must be checked between launches

                var gw2cache = Path.Combine(DataPath.AppDataAccountDataTemp, account.UID.ToString(), Tools.DatUpdater.CUSTOM_GW2CACHE_FOLDER_NAME);
                var profilebinuser = Path.Combine(root, bin, "user");

                try
                {
                    if ((File.GetAttributes(gw2cache) & FileAttributes.ReparsePoint) == FileAttributes.ReparsePoint && Directory.Exists(profilebinuser) && Directory.Exists(gw2cache + "-user"))
                    {
                        if (!v.IsPending)
                        {
                            if (resync == 0)
                                return null;
                            ++resync;
                        }
                    }
                }
                catch { }
            }

            if (v.IsPending && ((v.Value ^ v.ValueCommit) & (Settings.LocalizeAccountExecutionOptions.Enabled | Settings.LocalizeAccountExecutionOptions.OnlyIncludeBinFolders)) == Settings.LocalizeAccountExecutionOptions.None)
                v.Commit(); //only commiting here if the options specific to the handle of folders haven't changed, otherwise it should be handled by the conversion of all folders

            var excludeUnknown = v.Value.HasFlag(Settings.LocalizeAccountExecutionOptions.ExcludeUnknownFiles);
            var fullsync = resync != RESYNC_STATE_REFRESH_ONLY;
            var deleteUnknowns = resync != 0 && v.Value.HasFlag(Settings.LocalizeAccountExecutionOptions.AutoSyncDeleteUnknowns);
            var temp = Path.Combine(localroot, "temp");

            var gw2bin = Path.Combine(gw2root, bin);
            if (Directory.Exists(gw2bin))
                CopyBinFolder(gw2bin, Path.Combine(root, bin), excludeUnknown, onlyBin, deleteUnknowns, v.Value.HasFlag(Settings.LocalizeAccountExecutionOptions.AutoSync), temp);

            if (onlyBin)
            {
                if (!fullsync)
                    return null;

                var gw2cache = Path.Combine(DataPath.AppDataAccountDataTemp, account.UID.ToString(), Tools.DatUpdater.CUSTOM_GW2CACHE_FOLDER_NAME);
                var gw2cacheuser = gw2cache + "-user";
                var profilebin = Path.Combine(root, bin);
                var profilebinuser = Path.Combine(profilebin, "user");

                Directory.CreateDirectory(profilebin);

                try
                {
                    if (Directory.Exists(gw2cache))
                        Util.FileUtil.DeleteDirectory(gw2cache, true);

                    Windows.Symlink.CreateJunction(gw2cache, profilebin);

                    if (Directory.Exists(profilebinuser))
                        Util.FileUtil.DeleteDirectory(profilebinuser, true);

                    Directory.CreateDirectory(gw2cacheuser);
                    Windows.Symlink.CreateJunction(profilebinuser, gw2cacheuser);

                    File.SetAttributes(profilebinuser, FileAttributes.Directory | FileAttributes.ReparsePoint | FileAttributes.Hidden);
                }
                catch { }

                return null;
            }
            else
            {
                CopyRootFolder(gw2root, root, excludeUnknown, onlyBin, deleteUnknowns, v.Value.HasFlag(Settings.LocalizeAccountExecutionOptions.AutoSync), temp);

                if (fullsync)
                {
                    MakeLink(root, gw2root, "Gw2.dat", true, temp);
                    MakeLink(root, gw2root, "THIRDPARTYSOFTWAREREADME.txt", true, temp);
                    MakeLink(root, gw2root, fi.Name, true, temp);

                    //Windows.Symlink.CreateHardLink(exe, fi.FullName);
                }

                return exe;
            }
        }

        private static bool DeleteLocalized(ushort uid)
        {
            var path = Settings.GuildWars2.Path.Value;
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
                    Util.FileUtil.DeleteDirectory(root);
                }
                catch
                {
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// Deletes the localized path, if not in use
        /// </summary>
        /// <param name="convert">If true, the type of localized path will be converted instead of deleted</param>
        private static bool DeleteLocalized(ushort uid, bool convert)
        {
            var path = Settings.GuildWars2.Path.Value;
            if (string.IsNullOrEmpty(path))
                return false;
            var root = Path.Combine(Path.GetDirectoryName(path), LOCALIZED_EXE_FOLDER_NAME, uid.ToString());
            var onlyBin = (Settings.GuildWars2.LocalizeAccountExecution.Value & Settings.LocalizeAccountExecutionOptions.OnlyIncludeBinFolders) == Settings.LocalizeAccountExecutionOptions.OnlyIncludeBinFolders;
            var bin = IsGw264Bit ? "bin64" : "bin";

            if (Directory.Exists(root))
            {
                try
                {
                    var binpath = Path.Combine(root, bin);
                    var dll = Path.Combine(binpath, "icudt.dll");

                    if (File.Exists(dll))
                    {
                        File.Delete(dll);
                    }

                    foreach (var f in Directory.GetFiles(root))
                    {
                        File.Delete(f);
                    }

                    foreach (var f in Directory.GetDirectories(root))
                    {
                        if (Path.GetFileName(f).Equals(bin, StringComparison.OrdinalIgnoreCase))
                        {
                            continue;
                        }

                        Util.FileUtil.DeleteDirectory(f, true);
                    }

                    if (Directory.Exists(binpath))
                    {
                        var gw2cache = Path.Combine(DataPath.AppDataAccountDataTemp, uid.ToString(), Tools.DatUpdater.CUSTOM_GW2CACHE_FOLDER_NAME);

                        if (Directory.Exists(gw2cache) && File.GetAttributes(gw2cache).HasFlag(FileAttributes.ReparsePoint))
                            Directory.Delete(gw2cache);

                        if (convert)
                        {
                            if (onlyBin)
                            {

                            }
                            else
                            {
                                var user = Path.Combine(binpath, "user");
                                if (Directory.Exists(user))
                                    Util.FileUtil.DeleteDirectory(user);
                            }
                        }
                    }

                    if (!convert)
                        Util.FileUtil.DeleteDirectory(root);
                }
                catch
                {
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// Deletes all localized paths
        /// </summary>
        /// <param name="convert">If true, converts them instead</param>
        /// <returns></returns>
        private static bool DeleteLocalizedRoot(bool convert)
        {
            var path = Settings.GuildWars2.Path.Value;
            if (string.IsNullOrEmpty(path) || Client.Launcher.GetActiveProcessCount(Launcher.AccountType.GuildWars2) != 0)
                return false;
            var root = Path.Combine(Path.GetDirectoryName(path), LOCALIZED_EXE_FOLDER_NAME);
            var onlyBin = (Settings.GuildWars2.LocalizeAccountExecution.Value & Settings.LocalizeAccountExecutionOptions.OnlyIncludeBinFolders) == Settings.LocalizeAccountExecutionOptions.OnlyIncludeBinFolders;

            if (Directory.Exists(root))
            {
                foreach (var d in Directory.GetDirectories(root))
                {
                    ushort uid;
                    if (ushort.TryParse(Path.GetFileName(d), out uid))
                    {
                        if (!DeleteLocalized(uid, convert))
                            return false;
                    }
                }

                if (!convert)
                {
                    try
                    {
                        Directory.Delete(root);
                    }
                    catch { }
                }
            }

            if (!onlyBin)
            {
                var temp = DataPath.AppDataAccountDataTemp;

                //delete any gw2cache folders that linked to the bin folder

                foreach (var a in Util.Accounts.GetGw2Accounts())
                {
                    try
                    {
                        var gw2cache = Path.Combine(temp, a.UID.ToString(), Tools.DatUpdater.CUSTOM_GW2CACHE_FOLDER_NAME);
                        if (Directory.Exists(gw2cache) && File.GetAttributes(gw2cache).HasFlag(FileAttributes.ReparsePoint))
                            Directory.Delete(gw2cache);
                    }
                    catch { }
                }
            }

            return true;
        }

        /// <summary>
        /// Attempts to delete the file, or moves it if it fails
        /// </summary>
        /// <param name="target">Path to delete</param>
        /// <param name="temp">Optional temp folder to move the file to if it can't be deleted</param>
        private static void DeleteOrMove(string target, string temp = null)
        {
            try
            {
                File.Delete(target);
            }
            catch
            {
                if (temp == null)
                    throw;

                Directory.CreateDirectory(temp);
                var n = Util.FileUtil.GetTemporaryFileName(temp);
                File.Move(target, Path.Combine(temp, n));
            }
        }

        /// <summary>
        /// Makes a hard link
        /// </summary>
        /// <param name="link">The directory where the link will be created</param>
        /// <param name="target">The directory where the target file is located</param>
        /// <param name="name">The name of the target file</param>
        /// <param name="verifyExisting">If true, compares existing files and skips if already exists</param>
        /// <param name="temp">Optional temp folder to move files when can't be deleted</param>
        private static void MakeLink(string link, string target, string name, bool verifyExisting = false, string temp = null)
        {
            link = Path.Combine(link, name);
            target = Path.Combine(target, name);

            if (File.Exists(link))
            {
                if (verifyExisting)
                {
                    if (File.GetLastWriteTimeUtc(link) == File.GetLastWriteTimeUtc(target))
                        return;

                    try
                    {
                        File.Delete(link);
                    }
                    catch
                    {
                        if (temp == null)
                            throw;

                        Directory.CreateDirectory(temp);
                        var n = Util.FileUtil.GetTemporaryFileName(temp);
                        File.Move(link, Path.Combine(temp, n));
                    }
                }
                else
                {
                    File.Delete(link);
                }
            }

            if (File.Exists(target))
                Windows.Symlink.CreateHardLink(link, target);
        }

        /// <summary>
        /// Makes a directory link
        /// </summary>
        /// <param name="link">The directory where the link will be created</param>
        /// <param name="target">The directory where the target file is located</param>
        /// <param name="name">The name of the target directory</param>
        /// <param name="silent">Ignores errors</param>
        private static void MakeJunction(string link, string target, string name, bool silent = false)
        {
            try
            {
                link = Path.Combine(link, name);
                target = Path.Combine(target, name);
                if (Directory.Exists(link))
                    Directory.Delete(link);
                if (Directory.Exists(target))
                {
                    try
                    {
                        Windows.Symlink.CreateJunction(link, target);
                    }
                    catch (Exception e)
                    {
                        Util.Logging.Log(e);
                        Windows.Symlink.CreateSymbolicDirectory(link, target);
                    }
                }
            }
            catch
            {
                if (!silent)
                    throw;
            }
        }

        /// <summary>
        /// Creates a directory junction
        /// </summary>
        /// <param name="deleteExisting">If the from folder already exists, delete it</param>
        /// <param name="createTarget">If the to folder doesn't exist, create it</param>
        /// <param name="silent">Ignores errors</param>
        /// <returns>False if the "from" folder could not be deleted due to exisitng files or the "to" folder does not exist, otherwise true</returns>
        private static bool MakeJunction(string link, string target, bool deleteExisting, bool createTarget, bool silent = false)
        {
            try
            {
                if (Directory.Exists(link))
                {
                    if ((File.GetAttributes(link) & FileAttributes.ReparsePoint) == FileAttributes.ReparsePoint)
                    {
                        if (deleteExisting)
                        {
                            Directory.Delete(link);
                        }
                        else
                        {
                            return false;
                        }
                    }
                    else
                    {
                        if (deleteExisting)
                        {
                            Util.FileUtil.DeleteDirectory(link);
                        }
                        else
                        {
                            return false;
                        }
                    }
                }

                if (!Directory.Exists(target))
                {
                    if (createTarget)
                        Directory.CreateDirectory(target);
                    else
                        return false;
                }

                try
                {
                    Windows.Symlink.CreateJunction(link, target);
                }
                catch (Exception e)
                {
                    Util.Logging.Log(e);
                    Windows.Symlink.CreateSymbolicDirectory(link, target);
                }
            }
            catch
            {
                if (!silent)
                    throw;

                return false;
            }

            return true;
        }

        private static void CopyRootFolder(string from, string to, bool excludeUnknown, bool onlyBin, bool deleteUnknowns, bool autosync, string temp)
        {
            if (onlyBin)
                return;

            var existing = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            foreach (var d in Directory.GetDirectories(to))
            {
                existing.Add(Path.GetFileName(d));
            }

            if (!excludeUnknown)
            {
                foreach (var d in Directory.GetDirectories(from))
                {
                    var name = Path.GetFileName(d);
                    var exists = existing.Remove(name);

                    switch (name)
                    {
                        case "bin":
                        case "bin64":
                        case LOCALIZED_EXE_FOLDER_NAME:
                            break;
                        case "d912pxy":

                            try
                            {
                                CopyDx912ProxyFolder(d, Path.Combine(to, name), temp);
                            }
                            catch (Exception ex)
                            {
                                Util.Logging.Log(ex);

                                try
                                {
                                    Util.FileUtil.DeleteDirectory(Path.Combine(to, name));
                                    MakeJunction(to, from, name);
                                }
                                catch (Exception ex2)
                                {
                                    Util.Logging.Log(ex2);
                                }
                            }

                            break;
                        case "addons":

                            try
                            {
                                CopyAddonsFolder(d, Path.Combine(to, name), temp);
                            }
                            catch (Exception ex)
                            {
                                Util.Logging.Log(ex);
                            }

                            break;
                        case "Gw2.dat": //Chinese version splits Gw2.dat into multiple files within multiple subfolders under a Gw2.dat folder and uses Lock.dat within that folder as a lock check

                            try
                            {
                                var gw2datroot = Path.Combine(from, name);
                                var datroot = Path.Combine(to, name);
                                var lw = Directory.GetLastWriteTimeUtc(gw2datroot);

                                if (exists)
                                {
                                    if (Directory.GetLastWriteTimeUtc(datroot) == lw)
                                        continue;
                                    Util.FileUtil.DeleteDirectory(datroot, true);
                                }

                                Directory.CreateDirectory(datroot);

                                foreach (var d1 in Directory.GetDirectories(gw2datroot))
                                {
                                    MakeJunction(datroot, gw2datroot, Path.GetFileName(d1));
                                }

                                foreach (var f1 in Directory.GetFiles(gw2datroot))
                                {
                                    var n = Path.GetFileName(f1);

                                    switch (n)
                                    {
                                        case "Lock.dat":

                                            try
                                            {
                                                File.Copy(f1, Path.Combine(datroot, n), true);
                                            }
                                            catch (Exception ex)
                                            {
                                                Util.Logging.Log(ex);
                                            }

                                            break;
                                        default:

                                            MakeLink(datroot, gw2datroot, n);

                                            break;
                                    }
                                }

                                Directory.SetLastWriteTimeUtc(datroot, lw);
                            }
                            catch (Exception ex)
                            {
                                Util.Logging.Log(ex);
                            }

                            break;
                        default:

                            try
                            {
                                var d2 = Path.Combine(to, name);

                                if (exists)
                                {
                                    if (Directory.GetLastWriteTimeUtc(d) == Directory.GetLastWriteTimeUtc(d2))
                                        continue;
                                    Directory.Delete(d2);
                                }

                                MakeJunction(to, from, name);
                            }
                            catch (Exception ex)
                            {
                                Util.Logging.Log(ex);
                            }

                            break;
                    }
                }
            }

            if (deleteUnknowns)
            {
                foreach (var name in existing)
                {
                    try
                    {
                        Util.FileUtil.DeleteDirectory(Path.Combine(to, name), true);
                    }
                    catch { }
                }
            }

            if (!excludeUnknown || autosync || deleteUnknowns)
            {
                existing.Clear();

                foreach (var path in Directory.GetFiles(to, "*.dll"))
                {
                    existing.Add(Path.GetFileName(path));
                }
            }

            if (!excludeUnknown || autosync && existing.Count > 0)
            {
                foreach (var path in Directory.GetFiles(from, "*.dll"))
                {
                    var name = Path.GetFileName(path);
                    var output = Path.Combine(to, name);
                    var exists = existing.Remove(name);

                    if (!excludeUnknown || exists && autosync)
                    {
                        if (exists)
                        {
                            try
                            {
                                if (File.GetLastWriteTimeUtc(output) == File.GetLastWriteTimeUtc(path))
                                    continue;
                                DeleteOrMove(output, temp);
                            }
                            catch (UnauthorizedAccessException)
                            {
                                continue;
                            }
                        }

                        try
                        {
                            Windows.Symlink.CreateHardLink(output, path);
                        }
                        catch
                        {
                            File.Copy(path, output);
                        }
                    }
                }
            }

            if (deleteUnknowns)
            {
                foreach (var name in existing)
                {
                    try
                    {
                        DeleteOrMove(Path.Combine(to, name), temp);
                    }
                    catch { }
                }
            }
        }

        private static void CopyBinFolder(string from, string to, bool excludeUnknown, bool onlyBin, bool deleteUnknowns, bool autosync, string temp)
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

            if (!excludeUnknown)
            {
                foreach (var path in Directory.GetDirectories(from))
                {
                    try
                    {
                        var name = Path.GetFileName(path);
                        MakeJunction(to, from, name);
                    }
                    catch (Exception ex)
                    {
                        Util.Logging.Log(ex);
                    }
                }
            }

            foreach (var path in Directory.GetFiles(from))
            {
                var name = Path.GetFileName(path);
                var output = Path.Combine(to, name);
                var exists = existing.Remove(name);

                switch (name)
                {
                    //these files can be linked
                    case "CoherentUI64.dll":
                    case "CoherentUI.dll":
                    case "CoherentUI_Host.exe":
                    case "pdf.dll":
                    case "theme_resources_standard.pak":

                        if (exists)
                        {
                            try
                            {
                                if (File.GetLastWriteTimeUtc(output) == File.GetLastWriteTimeUtc(path))
                                    continue;
                                DeleteOrMove(output, temp);
                            }
                            catch (UnauthorizedAccessException)
                            {
                                continue;
                            }
                        }

                        Windows.Symlink.CreateHardLink(output, path);

                        break;
                    //these files require exclusive access
                    case "d3dcompiler_43.dll":
                    case "ffmpegsumo.dll":
                    case "icudt.dll":
                    case "libEGL.dll":
                    case "libGLESv2.dll":
                    case "icudtl.dat":
                    case "d3dcompiler_46.dll":
                    case "blink_resources.pak":
                    case "content_resources_100_percent.pak":
                    case "ui_resources_100_percent.pak":

                        //note these files don't need to be copied; they will be created on launch regardless of their current state

                        break;
                    //these files are unknown
                    default:

                        if (!excludeUnknown || exists && autosync)
                        {
                            if (exists)
                            {
                                try
                                {
                                    if (File.GetLastWriteTimeUtc(output) == File.GetLastWriteTimeUtc(path))
                                        continue;
                                    DeleteOrMove(output, temp);
                                }
                                catch
                                {
                                    continue;
                                }
                            }

                            if (onlyBin)
                            {
                                //plugins for arcdps will cause it to crash when in binaries mode

                                if (name.EndsWith(".dll", StringComparison.OrdinalIgnoreCase))
                                {
                                    if (name.IndexOf("arcdps", StringComparison.OrdinalIgnoreCase) != -1)
                                    {
                                        continue;
                                    }
                                    else if (name == "d3d9_uploader.dll")
                                    {
                                        continue;
                                    }
                                }
                            }

                            Windows.Symlink.CreateHardLink(output, path);
                        }

                        break;
                }
            }

            if (deleteUnknowns)
            {
                foreach (var name in existing)
                {
                    try
                    {
                        DeleteOrMove(Path.Combine(to, name), temp);
                    }
                    catch { }
                }
            }
        }

        private static void CopyAddonsFolder(string from, string to, string temp)
        {
            if (Directory.Exists(to))
            {
                if (File.GetAttributes(to).HasFlag(FileAttributes.ReparsePoint))
                {
                    Directory.Delete(to);
                    Directory.CreateDirectory(to);
                }
                else
                {
                    return;
                }
            }
            else
            {
                Directory.CreateDirectory(to);
            }

            foreach (var d in Directory.GetDirectories(from))
            {
                var name = Path.GetFileName(d);

                switch (name)
                {
                    case "d912pxy":

                        try
                        {
                            CopyDx912ProxyFolder(d, Path.Combine(to, name), temp);
                        }
                        catch (Exception ex)
                        {
                            Util.Logging.Log(ex);

                            try
                            {
                                Util.FileUtil.DeleteDirectory(Path.Combine(to, name));
                            }
                            catch (Exception ex2)
                            {
                                Util.Logging.Log(ex2);
                            }
                        }

                        break;
                    case "arcdps":

                        try
                        {
                            CopyGenericAddonFolder(d, Path.Combine(to, name), temp, false);
                        }
                        catch (Exception ex)
                        {
                            Util.Logging.Log(ex);
                        }

                        break;
                    default:

                        MakeJunction(to, from, name, true);

                        break;
                }
            }
        }

        private static void CopyGenericAddonFolder(string from, string to, string temp, bool onlyLinkSubfolders)
        {
            if (Directory.Exists(to))
            {
                if (File.GetAttributes(to).HasFlag(FileAttributes.ReparsePoint))
                {
                    Directory.Delete(to);
                    Directory.CreateDirectory(to);
                }
                else
                {
                    return;
                }
            }
            else
            {
                Directory.CreateDirectory(to);
            }

            foreach (var f in Directory.GetFiles(from))
            {
                var name = Path.GetFileName(f);
                var ext = Path.GetExtension(f).ToLower();

                switch (ext)
                {
                    case ".ini":
                    case ".json":

                        try
                        {
                            File.Copy(f, Path.Combine(to, name));
                        }
                        catch { }

                        break;
                    case ".log":

                        break;
                    //case ".dll":
                    //case ".pdb":
                    //case ".lib":
                    //case ".exe":
                    default:
                
                        MakeLink(to, from, name, true, temp);

                        break;
                }
            }

            foreach (var d in Directory.GetDirectories(from))
            {
                var name = Path.GetFileName(d);

                if (onlyLinkSubfolders)
                {
                    MakeJunction(to, from, name, true);
                }
                else
                {
                    CopyGenericAddonFolder(d, Path.Combine(to, name), temp, onlyLinkSubfolders);
                }
            }
        }

        private static void CopyDx912ProxyFolder(string from, string to, string temp)
        {
            if (Directory.Exists(to))
            {
                if (File.GetAttributes(to).HasFlag(FileAttributes.ReparsePoint))
                {
                    Directory.Delete(to);
                    Directory.CreateDirectory(to);
                }
                else
                {
                    return;
                }
            }
            else
            {
                Directory.CreateDirectory(to);
            }

            Action<string, bool> copyPath = null;
            copyPath = delegate(string relative, bool noCopy)
            {
                var root = Path.Combine(from, relative);

                Directory.CreateDirectory(Path.Combine(to, relative));

                foreach (var f in Directory.GetFiles(root))
                {
                    var name = Path.GetFileName(f);

                    switch (name)
                    {
                        case "do_not_delete.txt":
                        case "shader_profiles.pck":
                        case "common.hlsli":

                            MakeLink(to, from, Path.Combine(relative, name), true, temp);

                            break;
                        case "pid.lock":
                            break;
                        default:

                            if (!noCopy)
                            {
                                try
                                {
                                    File.Copy(f, Path.Combine(to, relative, name), true);
                                }
                                catch (Exception ex)
                                {
                                    Util.Logging.Log(ex);

                                    MakeLink(to, from, Path.Combine(relative, name), true, temp);
                                }
                            }

                            break;
                    }
                }

                foreach (var d in Directory.GetDirectories(root))
                {
                    var name = Path.GetFileName(d);

                    switch (name)
                    {
                        case "hlsl":

                            copyPath(Path.Combine(relative, name), true);

                            break;
                        default:

                            MakeJunction(to, from, Path.Combine(relative, name), true);

                            break;
                    }
                }
            };

            //MakeLink(to, from, "config.ini");

            foreach (var f in Directory.GetFiles(from))
            {
                var name = Path.GetFileName(f);

                MakeLink(to, from, name, true, temp);
            }

            foreach (var d in Directory.GetDirectories(from))
            {
                var name = Path.GetFileName(d);

                switch (name)
                {
                    case "pck":
                    case "shaders":

                        copyPath(name, false);

                        break;
                    default:

                        MakeJunction(to, from, name, true);

                        break;
                }
            }
        }

        /// <summary>
        /// Verifies all links to this file and repairs them if needed
        /// </summary>
        public static void VerifyLinks(FileType type, Settings.IFile file)
        {
            if (!IsDataLinkingSupported)
                return;

            var path = file.Path;
            DateTime lastWrite;

            try
            {
                if (File.Exists(path))
                {
                    lastWrite = File.GetLastWriteTimeUtc(path);
                }
                else
                {
                    lastWrite = DateTime.MinValue;
                }
            }
            catch
            {
                lastWrite = DateTime.MinValue;
            }

            var pd = new PathData();
            var newest = lastWrite;
            string newestPath = null;

            foreach (var a in FindAccounts(type, file))
            {
                var p = pd.GetCustomPath(type, a, true);

                if (!File.Exists(p))
                {
                    a.PendingFiles = true;

                    continue;
                }

                var d = File.GetLastWriteTimeUtc(p);

                if (d != lastWrite)
                {
                    a.PendingFiles = true;

                    if (d > newest)
                    {
                        newest = d;
                        newestPath = p;
                    }
                }
            }

            if (newestPath != null)
            {
                try
                {
                    using (var copy = File.Open(newestPath, FileMode.Open, FileAccess.Read, FileShare.Read))
                    {
                        using (var main = File.Open(path, FileMode.OpenOrCreate, FileAccess.Write, FileShare.Read))
                        {
                            copy.CopyTo(main);
                        }
                    }
                }
                catch (Exception e)
                {
                    Util.Logging.Log(e);
                }
            }
        }

        /// <summary>
        /// Verifies all Local.dat files are the same build, using the default Local.dat as a reference, or if it doesn't exist, the build from the server
        /// </summary>
        /// <param name="verify">True to verify, False to only update the cache</param>
        /// <returns>True if a Local.dat needs to be updated</returns>
        public static bool VerifyLocalDatBuild(bool verify = true)
        {
            var path = Settings.GuildWars2.Path.Value;
            if (string.IsNullOrEmpty(path) || !File.Exists(path))
                return true;

            var d = (uint)((File.GetLastWriteTimeUtc(path).Ticks / 1000) % uint.MaxValue);

            verify = verify && Settings.GuildWars2.LastModified.HasValue && Settings.GuildWars2.LastModified.Value != d;

            Settings.GuildWars2.LastModified.Value = d;

            if (verify)
            {
                var pd = new PathData();
                var build = 0;
                var changed = false;
                var defaultPath = Path.Combine(pd.GetUserPath(PathData.SpecialPath.Gw2AppDataDefault), DAT_NAME);
                var defaultBuild = 0;

                if (File.Exists(defaultPath))
                {
                    try
                    {
                        defaultBuild = Tools.Dat.DatFile.ReadBuild(defaultPath);
                    }
                    catch (Exception e)
                    {
                        Util.Logging.Log(e);

                        return true;
                    }
                }
                else
                {
                    return true;
                }

                foreach (var uid in Settings.DatFiles.GetKeys())
                {
                    var dat = Settings.DatFiles[uid].Value;
                    if (dat != null && dat.References > 0)
                    {
                        try
                        {
                            if (File.Exists(dat.Path))
                            {
                                var b = Tools.Dat.DatFile.ReadBuild(dat.Path);
                                if (build != b)
                                {
                                    if (changed = build != 0 || b < defaultBuild)
                                        break;
                                    build = b;
                                }
                            }
                        }
                        catch (Exception e)
                        {
                            Util.Logging.Log(e);
                            break;
                        }
                    }
                }

                if (changed)
                {
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// Checks if the path is for the Chinese version of GW2
        /// </summary>
        /// <param name="path">Path to Gw2-64.exe</param>
        public static bool IsPathGw2China(string path)
        {
            try
            {
                if (!string.IsNullOrEmpty(path) && Directory.Exists(Path.Combine(Path.GetDirectoryName(path), "Gw2.dat")))
                {
                    return true;
                }
            }
            catch (Exception e)
            {
                Util.Logging.Log(e);
            }

            return false;
        }
    }
}
