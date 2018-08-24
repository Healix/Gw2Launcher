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
                StackTrace stackTrace = new StackTrace();
                MethodBase methodBase = stackTrace.GetFrame(1).GetMethod();
                var now = DateTime.Now;

                Debug.WriteLine(e.Message + "\n----------------------------->\n" + e.StackTrace + "\n==============================", "[" + now.ToShortDateString() + " " + now.ToLongTimeString() +  "][" + methodBase.ReflectedType.Name + "." + methodBase.Name + "]");
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message + "\n" + e.StackTrace);
            }
            #endif
        }

        public static void Log(string message)
        {
            #if DEBUG
            try
            {
                StackTrace stackTrace = new StackTrace();
                MethodBase methodBase = stackTrace.GetFrame(1).GetMethod();
                var now = DateTime.Now;

                Debug.WriteLine(message, "[" + now.ToShortDateString() + " " + now.ToShortTimeString() + "][" + methodBase.ReflectedType.Name + "." + methodBase.Name + "]");
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message + "\n" + ex.StackTrace);
            }
            #endif
        }

        public static void Crash(Exception e)
        {
            Write(e.GetType().ToString() + ": " + e.Message + "\r\n" + new StackTrace(1).ToString());
        }

        public static void Crash(string message)
        {
            Write(message + "\r\n" + new StackTrace(1).ToString());
        }

        public static void Write(string message)
        {
            try
            {
                lock (PATH)
                {
                    var now = DateTime.Now;

                    using (var writer = new StreamWriter(File.Open(PATH, FileMode.Append, FileAccess.Write, FileShare.Read)))
                    {
                        writer.WriteLine("[" + now.ToString("MM/dd/yyyy HH:mm:ss.fff") + "] " + message);
                    }
                }
            }
            catch
            {

            }
        }
    }
}
