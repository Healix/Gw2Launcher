using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace Gw2Launcher.Tools
{
    static class Gw2Logs
    {
        private const string CRASH_LOG_NAME = "ArenaNet.log";

        public static void Delete()
        {
            if (Client.FileManager.IsDataLinkingSupported)
            {
                foreach (var uid in Settings.Accounts.GetKeys())
                {
                    var a = Settings.Accounts[uid];
                    if (!a.HasValue)
                        continue;
                    var account = a.Value;

                    DeleteFile(Path.Combine(Client.FileManager.GetPath(Client.FileManager.SpecialPath.AppData, account), CRASH_LOG_NAME));
                }

                DeleteFile(Path.Combine(Client.FileManager.GetPath(Client.FileManager.SpecialPath.AppData), CRASH_LOG_NAME));
                DeleteFolder(Path.Combine(Client.FileManager.GetPath(Client.FileManager.SpecialPath.Dumps)));
            }
            else
            {
                foreach (var uid in Settings.Accounts.GetKeys())
                {
                    var a = Settings.Accounts[uid];
                    if (!a.HasValue)
                        continue;
                    var account = a.Value;

                    DeleteFile(Path.Combine(Client.FileManager.GetPath(Client.FileManager.SpecialPath.AppData, account), CRASH_LOG_NAME));
                    DeleteFolder(Path.Combine(Client.FileManager.GetPath(Client.FileManager.SpecialPath.Dumps, account)));
                }
            }
        }

        private static void DeleteFile(string path)
        {
            try
            {
                var fi = new FileInfo(path);
                if (fi.Exists && DateTime.UtcNow.Subtract(fi.LastWriteTimeUtc).TotalDays >= 3)
                    File.Delete(path);
            }
            catch (Exception e)
            {
                Util.Logging.Log(e);
            }
        }

        private static void DeleteFolder(string path)
        {
            try
            {
                foreach (var f in Directory.GetFiles(path, "*.dmp"))
                {
                    DeleteFile(f);
                }
            }
            catch (Exception e)
            {
                Util.Logging.Log(e);
            }
        }
    }
}
