using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace Gw2Launcher.Tools
{
    static class Temp
    {
        private class QueuedPath
        {
            public string path;
            public FileAttributes attributes;
            public bool delete;
        }

        public static IEnumerable<string> GetAccountFolders()
        {
            foreach (var path in Directory.EnumerateDirectories(DataPath.AppDataAccountDataTemp))
            {
                ushort uid;
                if (ushort.TryParse(Path.GetFileName(path), out uid))
                {
                    yield return path;
                }
            }
        }

        public static void Purge(ushort uid)
        {
            var path = Path.Combine(DataPath.AppDataAccountDataTemp, uid.ToString());
            var date = DateTime.UtcNow.AddDays(-1);

            foreach (var p in Directory.EnumerateFileSystemEntries(path))
            {
                var a = File.GetAttributes(p);

                if ((a & FileAttributes.Directory) == FileAttributes.Directory)
                {   
                    if ((a & FileAttributes.ReparsePoint) == FileAttributes.ReparsePoint)
                    {
                        if (File.GetLastWriteTimeUtc(p) < date)
                        {
                            try
                            {
                                if (Client.Launcher.GetState(uid) == Client.Launcher.AccountState.None)
                                    Directory.Delete(p);
                            }
                            catch { }
                        }
                    }
                    else
                    {
                        var name = Path.GetFileName(p);
                        
                        if (name.Length >= 8 && name.Substring(0, 8).Equals("gw2cache"))
                        {
                            Gw2Cache.DeleteCacheFolder(p, date);
                        }
                        else
                        {
                            DeleteFileSystemEntries(p, 1, true, true);
                        }
                    }
                }
                else
                {
                    if (File.GetLastWriteTimeUtc(p) < date)
                    {
                        try
                        {
                            File.Delete(p);
                        }
                        catch { }
                    }
                }
            }

            try
            {
                Directory.Delete(path);
            }
            catch { }
        }

        private static void DeleteFiles(string path, int days)
        {
            var date = DateTime.UtcNow.AddDays(-days);

            foreach (var p in Directory.EnumerateFiles(path))
            {
                try
                {
                    if (days == 0 || File.GetLastWriteTimeUtc(p) < date)
                    {
                        File.Delete(path);
                    }
                }
                catch { }
            }
        }
        
        private static int DeleteFileSystemEntries(string path, int days, bool includeSubfolders, bool deleteSelf)
        {
            DateTime d;
            if (days == 0)
                d = DateTime.MinValue;
            else
                d = DateTime.UtcNow.AddDays(-days);
            return DeleteFileSystemEntries(path, d, includeSubfolders, deleteSelf);
        }

        /// <summary>
        /// Deletes files/folders that are older than the given date
        /// </summary>
        /// <param name="path">Path to search for files/folders</param>
        /// <param name="date">Anything before this date will be deleted, or DateTime.MinValue to not check</param>
        /// <param name="includeSubfolders">True to search through subfolders</param>
        /// <param name="deleteSelf">True to delete the initial path if the folder is empty</param>
        /// <returns>Number of files/folders remaining, including self</returns>
        private static int DeleteFileSystemEntries(string path, DateTime date, bool includeSubfolders, bool deleteSelf)
        {
            var count = 0;

            if (includeSubfolders)
            {
                foreach (var p in Directory.EnumerateFileSystemEntries(path))
                {
                    try
                    {
                        var a = File.GetAttributes(p);
                        var delete = date == DateTime.MinValue || File.GetLastWriteTimeUtc(p) < date;

                        if ((a & FileAttributes.Directory) == FileAttributes.Directory)
                        {
                            if ((a & FileAttributes.ReparsePoint) == FileAttributes.ReparsePoint || !includeSubfolders)
                            {
                                if (delete)
                                    Directory.Delete(p);
                                else
                                    ++count;
                            }
                            else
                            {
                                count += DeleteFileSystemEntries(p, date, includeSubfolders, delete);
                            }
                        }
                        else
                        {
                            if (delete)
                                File.Delete(p);
                            else
                                ++count;
                        }
                    }
                    catch
                    {
                        ++count;
                    }
                }
            }
            else
            {
                foreach (var p in Directory.EnumerateFiles(path))
                {
                    try
                    {
                        if (date == DateTime.MinValue || File.GetLastWriteTimeUtc(p) < date)
                            File.Delete(p);
                        else
                            ++count;
                    }
                    catch
                    {
                        ++count;
                    }
                }
            }

            if (deleteSelf && count == 0)
            {
                try
                {
                    Directory.Delete(path);
                }
                catch
                {
                    ++count;
                }
            }
            else
                ++count;

            return count;
        }

        public static void Purge(int days = 0)
        {
            var date = DateTime.UtcNow.AddDays(-days);

            foreach (var path in Directory.EnumerateDirectories(DataPath.AppDataAccountDataTemp))
            {
                try
                {
                    ushort uid;
                    if (ushort.TryParse(Path.GetFileName(path), out uid))
                    {
                        Purge(uid);
                    }
                    else
                    {
                        DeleteFileSystemEntries(path, days, true, true);
                    }
                }
                catch { }
            }

            if (Settings.TEMP_SETTINGS_ENABLED)
            {
                try
                {
                    File.SetLastWriteTimeUtc(Path.Combine(DataPath.AppDataAccountDataTemp, "settings.txt"), DateTime.UtcNow);
                }
                catch { }
            }

            try
            {
                DeleteFileSystemEntries(DataPath.AppDataAccountDataTemp, days, false, false);
            }
            catch { }

            if ((Settings.GuildWars2.LocalizeAccountExecution.Value & Settings.LocalizeAccountExecutionOptions.Enabled) == Settings.LocalizeAccountExecutionOptions.Enabled)
            {
                var gw2 = Settings.GuildWars2.Path.Value;
                if (!string.IsNullOrEmpty(gw2))
                {
                    try
                    {
                        Util.FileUtil.DeleteDirectory(Path.Combine(Path.GetDirectoryName(gw2), Client.FileManager.LOCALIZED_EXE_FOLDER_NAME, "temp"), true);
                    }
                    catch (Exception e)
                    {
                        Util.Logging.Log(e);
                    }
                }
            }
        }
    }
}
