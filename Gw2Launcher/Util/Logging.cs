using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;
using System.Diagnostics;
using System.IO;

namespace Gw2Launcher.Util
{
    static class Logging
    {
        public static readonly string PATH;

        static Logging()
        {
            PATH = Path.Combine(DataPath.AppData, "Gw2Launcher.log");

            try
            {
                var fi = new FileInfo(PATH);
                if (fi.Exists && fi.Length > 5120000)
                    fi.Delete();
            }
            catch { }
        }

        public static void Log(Exception e)
        {
            #if DEBUG
            try
            {
                var methodBase = new StackTrace().GetFrame(1).GetMethod();
                Debug.WriteLine(e.Message + "\n----------------------------->\n" + e.StackTrace + "\n==============================", "[" + DateTime.Now.ToString("MM/dd/yyyy HH:mm:ss.fff") + "][" + methodBase.ReflectedType.Name + "." + methodBase.Name + "]");
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
            #endif
        }

        public static void Log(string message)
        {
            #if DEBUG
            try
            {
                var methodBase = new StackTrace().GetFrame(1).GetMethod();
                Debug.WriteLine(message, "[" + DateTime.Now.ToString("MM/dd/yyyy HH:mm:ss.fff") + "][" + methodBase.ReflectedType.Name + "." + methodBase.Name + "]");
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
            #endif
        }

        public static bool Crash(Exception e)
        {
            return Write(e.ToString());
        }

        public static bool Crash(string message)
        {
            return Write(message + "\r\n" + new StackTrace(1).ToString());
        }

        public static bool Write(string message)
        {
            try
            {
                lock (PATH)
                {
                    using (var writer = new StreamWriter(File.Open(PATH, FileMode.Append, FileAccess.Write, FileShare.Read)))
                    {
                        writer.WriteLine("[" + DateTime.Now.ToString("MM/dd/yyyy HH:mm:ss.fff") + "] " + message);
                    }
                }

                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}
