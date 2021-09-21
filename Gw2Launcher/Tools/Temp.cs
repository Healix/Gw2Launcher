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

        private static void DeleteFileSystemEntries(string path, int days, bool includeSubfolders, bool deleteSelf)
        {
            var date = DateTime.UtcNow.AddDays(-days);
            var q = new Stack<QueuedPath>();
            var skipped = 0;

            q.Push(new QueuedPath()
                {
                    path = path,
                });

            do
            {
                var f = q.Pop();
                var count = 0;

                foreach (var p in Directory.EnumerateFileSystemEntries(f.path))
                {
                    ++count;

                    var a = File.GetAttributes(p);

                    if (days == 0 || File.GetLastWriteTimeUtc(p) < date)
                    {
                        if ((a & FileAttributes.Directory) == FileAttributes.Directory)
                        {
                            if ((a & FileAttributes.ReparsePoint) == FileAttributes.ReparsePoint)
                            {
                                try
                                {
                                    Directory.Delete(p);
                                    --count;
                                }
                                catch 
                                {
                                    ++skipped;
                                }
                            }
                            else
                            {
                                q.Push(new QueuedPath()
                                    {
                                        path = p,
                                        attributes = a,
                                        delete = true,
                                    });
                            }
                        }
                        else
                        {
                            try
                            {
                                File.Delete(p);
                                --count;
                            }
                            catch
                            {
                                ++skipped;
                            }
                        }
                    }
                    else if (includeSubfolders && (a & (FileAttributes.Directory | FileAttributes.ReparsePoint)) == FileAttributes.Directory)
                    {
                        q.Push(new QueuedPath()
                        {
                            path = p,
                            attributes = a,
                        });
                    }
                    else
                    {
                        ++skipped;
                    }
                }

                if (f.delete && count == 0)
                {
                    try
                    {
                        Directory.Delete(f.path);
                    }
                    catch { }
                }
            }
            while (q.Count > 0);

            if (deleteSelf && skipped == 0)
            {
                try
                {
                    Directory.Delete(path);
                }
                catch { }
            }
        }

        public static void Purge(int days = 0)
        {
            var date = DateTime.UtcNow.AddDays(-days);

            foreach (var path in Directory.EnumerateDirectories(DataPath.AppDataAccountDataTemp))
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

            DeleteFileSystemEntries(DataPath.AppDataAccountDataTemp, days, false, false);

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
