using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;
using System.Diagnostics;

namespace Gw2Launcher.Util
{
    static class Logging
    {
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
    }
}
