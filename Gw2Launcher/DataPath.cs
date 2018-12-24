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
        public static string AppData
        {
            get
            {
                var di = new DirectoryInfo(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData, Environment.SpecialFolderOption.Create), "Gw2Launcher"));
                if (!di.Exists)
                {
                    try
                    {
                        di.Create();
                    }
                    catch (Exception ex)
                    {
                        Util.Logging.Log(ex);
                    }
                }
                return di.FullName;
            }
        }

        public static string AppDataAccountData
        {
            get
            {
                var di = new DirectoryInfo(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData, Environment.SpecialFolderOption.Create), "Gw2Launcher", "data"));
                if (!di.Exists)
                {
                    try
                    {
                        di.Create();

                        Util.FileUtil.AllowFolderAccess(di.FullName, System.Security.AccessControl.FileSystemRights.Modify);
                    }
                    catch (Exception ex)
                    {
                        Util.Logging.Log(ex);
                    }
                }
                return di.FullName;
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
