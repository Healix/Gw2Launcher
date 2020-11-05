using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace Gw2Launcher
{
    static class DataPath
    {
        private static readonly string appdata;

        static DataPath()
        {
            appdata = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData, Environment.SpecialFolderOption.Create), "Gw2Launcher");
        }

        public static string AppData
        {
            get
            {
                if (!Directory.Exists(appdata))
                {
                    try
                    {
                        Directory.CreateDirectory(appdata);
                    }
                    catch (Exception ex)
                    {
                        Util.Logging.Log(ex);
                    }
                }

                return appdata;
            }
        }

        public static string AppDataAccountData
        {
            get
            {
                var path = Path.Combine(appdata, "data");

                if (!Directory.Exists(path))
                {
                    try
                    {
                        Directory.CreateDirectory(path);

                        Util.FileUtil.AllowFolderAccess(path, System.Security.AccessControl.FileSystemRights.Modify);
                    }
                    catch (Exception ex)
                    {
                        Util.Logging.Log(ex);
                    }
                }

                return path;
            }
        }

        public static string AppDataAccountDataTemp
        {
            get
            {
                var path = Path.Combine(AppDataAccountData, "temp");

                if (!Directory.Exists(path))
                {
                    try
                    {
                        Directory.CreateDirectory(path);
                    }
                    catch (Exception e)
                    {
                        Util.Logging.Log(e);
                    }
                }

                return path;
            }
        }
    }
}
