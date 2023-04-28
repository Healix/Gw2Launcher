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
        public static event EventHandler<LogEventArgs> LogMessage;

        public static bool Enabled;

        public class LogEventArgs : EventArgs
        {
            public LogEventArgs(Settings.IAccount account, string message)
            {
                this.Account = account;
                this.Message = message;
                this.Timestamp = DateTime.UtcNow;
            }

            public Settings.IAccount Account
            {
                get;
                set;
            }

            public string Message
            {
                get;
                set;
            }

            public DateTime Timestamp
            {
                get;
                set;
            }
        }

        static Logging()
        {
            PATH = Path.Combine(DataPath.AppData, "Gw2Launcher.log");

            try
            {
                var fi = new FileInfo(PATH);
                if (fi.Exists && fi.Length > 1048576)
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
#if DEBUG
            Log(e);
            Debugger.Break();
            return true;
#else
            return Write(e.ToString());
#endif
        }

        public static bool Crash(string message)
        {
#if DEBUG
            Log(message + "\r\n" + new StackTrace(1).ToString());
            Debugger.Break();
            return true;
#else
            return Write(message + "\r\n" + new StackTrace(1).ToString());
#endif
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

        public static void LogEvent(Settings.IAccount account, string message)
        {
            if (LogMessage != null)
            {
                try
                {
                    LogMessage(null, new LogEventArgs(account, message));
                }
                catch { }
            }
        }

        /// <summary>
        /// Returns the name of the file and the specified depth. For example, C:\1\2\3.txt at 1 depth becomes \2\3.txt.
        /// </summary>
        public static string GetDisplayPath(string path, int depth)
        {
            if (depth == 0)
            {
                return Path.GetFileName(path);
            }
            else
            {
                var folder = path;

                while (depth-- >= 0 && !string.IsNullOrEmpty(folder))
                {
                    folder = Path.GetDirectoryName(folder);
                }

                if (folder == null)
                    return path;

                return path.Substring(folder.Length);
            }
        }
    }
}
