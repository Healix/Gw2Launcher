using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gw2Launcher.Client
{
    static partial class Launcher
    {

        //private static bool OnFocused(WindowEvents.WindowEventsEventArgs e)
        //{
        //    var t = Environment.TickCount;

        //    Util.Logging.LogEvent(e.Account.Settings, "[OnFocused] Focusing [" + e.Account.Settings.Name + "]");

        //    if (focused == e.Account)
        //    {
        //        if (t - focusedTime < 100)
        //        {
        //            Util.Logging.LogEvent(e.Account.Settings, "[SetFocused] Skipping [" + e.Account.Settings.Name + "] due to limiter (" + (t - focusedTime) + "ms)");
        //            return false;
        //        }
        //    }

        //    OnFocused(e.Account, e.Handle);

        //    return true;
        //}

        //public static void OnFocused(Account a, IntPtr window)
        //{
        //    var s = a.Session;

        //    if (s != null)
        //    {
        //        s.LastFocus = DateTime.UtcNow;
        //    }

        //    try
        //    {
        //        if (AccountWindowEvent != null)
        //            AccountWindowEvent(a.Settings, new AccountWindowEventEventArgs(AccountWindowEventEventArgs.EventType.Focused, a.Process.Process, window));
        //    }
        //    catch (Exception e)
        //    {
        //        Util.Logging.Log(e);
        //    }
        //}

    }
}
