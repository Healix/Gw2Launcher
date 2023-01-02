using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Gw2Launcher.Client
{
    static class Steam
    {
        public const int APPID_GW2 = 1284210;

        public class AlreadyRunningSteamException : Exception
        {
            public AlreadyRunningSteamException()
                : base("Unable to launch; Steam is already in use")
            { }
        }

        public static int GetAppId(Settings.AccountType t)
        {
            switch (t)
            {
                case Settings.AccountType.GuildWars2:
                    return APPID_GW2;
            }

            return 0;
        }

        public static Process GetSteamProcess(string path = null)
        {
            if (path == null)
            {
                if (Settings.Steam.Path.HasValue)
                    path = Settings.Steam.Path.Value;
                else
                    path = Steam.Path;
            }

            try
            {
                if (!string.IsNullOrEmpty(path) && File.Exists(path))
                    return Util.ProcessUtil.FindProcess(path);
            }
            catch
            {
            }

            return null;
        }

        public static bool IsRunning(int appId)
        {
            if (appId > 0)
            {
                try
                {
                    using (var k = Registry.CurrentUser.OpenSubKey(@"SOFTWARE\Valve\Steam\Apps\" + appId))
                    {
                        return IsRunning(k);
                    }
                }
                catch (Exception e)
                {
                    Util.Logging.Log(e);
                }
            }

            return false;
        }

        private static bool IsRunning(RegistryKey k)
        {
            if (k != null)
            {
                var v = k.GetValue("Running");

                if (v is int)
                {
                    return (int)v == 1;
                }
            }

            return false;
        }

        /// <summary>
        /// Path to steam.exe
        /// </summary>
        public static string Path
        {
            get
            {
                try
                {
                    using (var k = Registry.CurrentUser.OpenSubKey(@"SOFTWARE\Valve\Steam"))
                    {
                        return (string)k.GetValue("SteamExe");
                    }
                }
                catch
                {
                    return null;
                }
            }
        }

        public static bool Launch(string path, int appId, string args, CancellationToken cancel)
        {
            RegistryKey k;

            try
            {
                k = Registry.CurrentUser.OpenSubKey(@"SOFTWARE\Valve\Steam\Apps\" + appId);
            }
            catch 
            {
                k = null;
            }

            using (k)
            {
                if (IsRunning(k))
                {
                    //the running state could be invalid, assuming it isn't if steam is active
                    using (var p = GetSteamProcess(path))
                    {
                        if (p != null)
                        {
                            throw new AlreadyRunningSteamException();
                        }
                    }
                }

                using (var p = new Process())
                {
                    p.StartInfo = new ProcessStartInfo(path, "-applaunch " + appId + (string.IsNullOrEmpty(args) ? "" : " " + args))
                    {
                        UseShellExecute = true,
                    };

                    if (p.Start())
                    {
                        var timeout = k != null ? 30000 : 3000;
                        var t = Environment.TickCount;

                        try
                        {
                            //if steam doesn't immediately close, it wasn't already running and may need to initialize/update
                            while (!p.WaitForExit(500))
                            {
                                if (IsRunning(k) || Environment.TickCount - t > timeout || cancel.IsCancellationRequested)
                                    break;
                            }
                        }
                        catch { }

                        return true;
                    }

                    return false;
                }
            }
        }
    }
}
