using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Threading;
using System.Runtime.InteropServices;
using System.Management;
using Gw2Launcher.Windows.Native;

namespace Gw2Launcher.Client
{
    static partial class Launcher
    {
        private class WindowWatcher : IDisposable
        {
            public class WindowChangedEventArgs : EventArgs
            {
                public enum EventType
                {
                    TitleChanged,
                    HandleChanged,
                    DxWindowCreated,
                    DxWindowReady,
                    LauncherCoherentUIReady,
                    InGameCoherentUIReady,

                    WatcherExited
                }

                public EventType Type
                {
                    get;
                    set;
                }

                public IntPtr Handle
                {
                    get;
                    set;
                }
            }

            public event EventHandler<WindowChangedEventArgs> WindowChanged;
            public event EventHandler<CrashReason> WindowCrashed;
            public event EventHandler<Tools.ArenaAccount> AuthenticationRequired;

            private const string DX_WINDOW_CLASSNAME = "ArenaNet_Dx_Window_Class";
            private const string LAUNCHER_WINDOW_CLASSNAME = "ArenaNet";
            private const string EDIT_CLASSNAME = "Edit";

            public enum CrashReason
            {
                Unknown,
                PatchRequired,
                NoPatchUI
            }

            private Process process;
            private Task watcher;
            private bool watchBounds, canReadMemory;
            private bool processWasAlreadyStarted;

            /// <summary>
            /// Watches the process and reports when the DX window is created
            /// </summary>
            public WindowWatcher(Account account, Process p, bool isProcessStarter, bool watchBounds, LaunchMode mode, string args)
            {
                this.process = p;
                this.processWasAlreadyStarted = !isProcessStarter;
                this.Account = account;
                this.watchBounds = watchBounds;
                //this.watchAutologin = account.Settings.AutomaticLogin && account.Settings.HasCredentials && !Settings.DisableAutomaticLogins;
                this.Mode = mode;
                this.Args = args;

                canReadMemory = Environment.Is64BitProcess || !Environment.Is64BitOperatingSystem;
            }

            public void Dispose()
            {
                if (watcher != null && watcher.IsCompleted)
                    watcher.Dispose();
            }

            public void Start()
            {
                if (watcher == null)
                {
                    watcher = new Task(WatchWindow2, TaskCreationOptions.LongRunning);
                    watcher.Start();
                }
            }

            public Process Process
            {
                get
                {
                    return this.process;
                }
            }

            public Account Account
            {
                get;
                private set;
            }

            public string Args
            {
                get;
                private set;
            }

            public ProcessOptions ProcessOptions
            {
                get;
                set;
            }

            public LaunchMode Mode
            {
                get;
                private set;
            }

            private int GetChildProcess(int pid)
            {
                if (canReadMemory)
                {
                    var noChild = true;
                    var processes = Process.GetProcesses();
                    var cid = 0;

                    using (var pi = new Windows.ProcessInfo())
                    {
                        foreach (var p in processes)
                        {
                            using (p)
                            {
                                if (noChild)
                                {
                                    try
                                    {
                                        if (pi.Open(p.Id))
                                        {
                                            if (pi.GetParent() == pid)
                                            {
                                                noChild = false;
                                                cid = p.Id;
                                            }
                                        }
                                    }
                                    catch (Exception e)
                                    {
                                        Util.Logging.Log(e);
                                    }
                                }
                            }
                        }
                    }

                    return cid;
                }

                using (var searcher = new ManagementObjectSearcher("SELECT ProcessId FROM Win32_Process WHERE ParentProcessID=" + pid))
                {
                    using (var results = searcher.Get())
                    {
                        foreach (ManagementObject o in results)
                        {
                            return (int)(uint)o["ProcessId"];
                        }
                    }
                }

                return 0;
            }

            private Windows.FindWindow.SearchResult Find(IntPtr handle, Windows.FindWindow.TextComparer classCallback, Windows.FindWindow.TextComparer textCallback)
            {
                try
                {
                    var r = Windows.FindWindow.FindChildren(handle, classCallback, textCallback, 1);
                    if (r.Count > 0)
                        return r[0];
                }
                catch (Exception e)
                { 
                    Util.Logging.Log(e);
                }

                return null;
            }

            private void WatchWindow2()
            {
                try
                {
                    WatchWindow();
                }
                catch (Exception e)
                {
                    Util.Logging.Log(e);
                }

                if (WindowChanged != null)
                {
                    WindowChanged(this, new WindowChangedEventArgs()
                        {
                            Type = WindowChangedEventArgs.EventType.WatcherExited
                        });
                }
            }

            private void WatchWindow()
            {
                IntPtr handle;
                try
                {
                    handle = process.MainWindowHandle;
                }
                catch (Exception e)
                {
                    Util.Logging.Log(e);
                    return;
                }

                Windows.FindWindow.TextComparer classCallback = new Windows.FindWindow.TextComparer(
                    delegate(string name, StringBuilder sb)
                    {
                        if (sb.Length == EDIT_CLASSNAME.Length)
                        {
                            return sb.ToString().Equals(EDIT_CLASSNAME);
                        }
                        return false;
                    });

                Windows.FindWindow.TextComparer textCallback = new Windows.FindWindow.TextComparer(
                    delegate(string name, StringBuilder sb)
                    {
                        if (name != null && name.Equals(EDIT_CLASSNAME))
                        {
                            return sb.Length > 500 && sb[0] == '*';
                        }

                        return true;
                    });

                StringBuilder buffer = new StringBuilder(DX_WINDOW_CLASSNAME.Length);
                int length = 0;

                var wce = new WindowChangedEventArgs();
                var _handle = IntPtr.Zero;

                do
                {
                    try
                    {
                        do
                        {
                            process.Refresh();
                            handle = process.MainWindowHandle;

                            if (handle == IntPtr.Zero)
                            {
                                if (!process.WaitForExit(10))
                                    process.WaitForInputIdle();
                            }
                            else
                                break;
                        }
                        while (!process.HasExited);
                    }
                    catch (Exception e)
                    {
                        Util.Logging.Log(e);
                    }

                    try
                    {
                        if (this.process.HasExited)
                        {
                            //no longer applicable - nopatchui exiting without creating a window likely meant the client wasn't up to date

                            //if (handle == IntPtr.Zero && (IsAutomaticLogin(this.Account.Settings) || Util.Args.Contains(this.Args, "nopatchui")))
                            //{
                            //    var elapsed = this.process.ExitTime.Subtract(this.process.StartTime).TotalSeconds;
                            //    if (elapsed < 60 && WindowCrashed != null)
                            //        WindowCrashed(this, CrashReason.NoPatchUI);
                            //}
                            break;
                        }
                    }
                    catch (Exception e)
                    {
                        Util.Logging.Log(e);
                        return;
                    }

                    if (handle != IntPtr.Zero)
                    {
                        if (_handle != handle)
                        {
                            _handle = handle;
                            wce.Handle = handle;

                            if (WindowChanged != null)
                            {
                                wce.Type = WindowChangedEventArgs.EventType.HandleChanged;
                                WindowChanged(this, wce);
                            }
                        }

                        buffer.Length = 0;
                        NativeMethods.GetClassName(handle, buffer, buffer.Capacity + 1);

                        int l;
                        if ((l = buffer.Length) != length)
                        {
                            length = l;

                            if (l > 0)
                            {
                                string className = buffer.ToString();
                                if (className.Equals(DX_WINDOW_CLASSNAME))
                                {
                                    if (WindowChanged != null)
                                    {
                                        wce.Type = WindowChangedEventArgs.EventType.DxWindowCreated;
                                        WindowChanged(this, wce);
                                    }

                                    //window initialization order:
                                    //title change > volume (window is ready) > coherentui

                                    int pidChild = 0;
                                    bool hasVolume = false;
                                    DateTime limit;

                                    using (var volumeControl = new Windows.Volume.VolumeControl(process.Id))
                                    {
                                        if (processWasAlreadyStarted && ((hasVolume = volumeControl.Query()) || (pidChild = GetChildProcess(process.Id)) > 0))
                                        {
                                            //window is already loaded

                                            if (WindowChanged != null)
                                            {
                                                wce.Type = WindowChangedEventArgs.EventType.TitleChanged;
                                                WindowChanged(this, wce);
                                            }

                                            limit = DateTime.UtcNow.AddMinutes(1);
                                        }
                                        else
                                        {
                                            try
                                            {
                                                limit = DateTime.UtcNow.AddSeconds(30);

                                                do
                                                {
                                                    buffer.Length = 0;
                                                    if (NativeMethods.GetWindowText(handle, buffer, 2) > 0 && buffer[0] != 'U') //window text is initially Untitled
                                                    {
                                                        if (WindowChanged != null)
                                                        {
                                                            wce.Type = WindowChangedEventArgs.EventType.TitleChanged;
                                                            WindowChanged(this, wce);
                                                        }

                                                        break;
                                                    }
                                                }
                                                while (DateTime.UtcNow < limit && !process.WaitForExit(100));
                                            }
                                            catch (Exception e)
                                            {
                                                Util.Logging.Log(e);
                                            }

                                            var nextCheck_Seconds = 3;
                                            var memoryUsage = process.PeakWorkingSet64;
                                            var nextCheck = DateTime.UtcNow.AddSeconds(nextCheck_Seconds);
                                            var memoryChecks = 0;
                                            //var verified = false;

                                            limit = DateTime.UtcNow.AddMinutes(3);

                                            do
                                            {
                                                if ((hasVolume = volumeControl.Query()) || (pidChild = GetChildProcess(process.Id)) > 0)
                                                {
                                                    break;
                                                }

                                                #region -nopatchui activity check (obsolete)

                                                //watching for changes in memory usage to check if it's still doing something
                                                //bypassing the launcher (-nopatchui) can cause it to get stuck on authentication if either the credentials are wrong or the network isn't authorized
                                                if (!hasVolume && DateTime.UtcNow > nextCheck)
                                                {
                                                    process.Refresh();

                                                    var memoryChange = process.PeakWorkingSet64 - memoryUsage;
                                                    memoryUsage += memoryChange;

                                                    if (memoryChange < 1000000)
                                                    {
                                                        if (++memoryChecks == 3)
                                                        {
                                                            //this account may be stuck trying to authenticate

                                                            var account = this.Account.Settings;
                                                            if (account.HasCredentials && account.NetworkAuthorizationState != Settings.NetworkAuthorizationState.Disabled && Settings.NetworkAuthorization.HasValue)
                                                            {
                                                                if (account.NetworkAuthorizationState == Settings.NetworkAuthorizationState.Unknown)
                                                                {
                                                                    if (AuthenticationRequired != null)
                                                                        AuthenticationRequired(this, null);

                                                                    return;
                                                                }
                                                                else
                                                                {
                                                                    Tools.ArenaAccount session;
                                                                    switch (NetworkAuthorization.Verify(account, true, null, out session))
                                                                    {
                                                                        case NetworkAuthorization.VerifyResult.Completed:
                                                                        case NetworkAuthorization.VerifyResult.Required:

                                                                            if (AuthenticationRequired != null)
                                                                                AuthenticationRequired(this, session);

                                                                            return;
                                                                        case NetworkAuthorization.VerifyResult.OK:

                                                                            //authentication was ok - assuming it's a slow load
                                                                            nextCheck_Seconds = 10;
                                                                            //verified = true;

                                                                            break;
                                                                        case NetworkAuthorization.VerifyResult.None:
                                                                        default:

                                                                            //authentication isn't being tracked - assuming it's stuck
                                                                            nextCheck_Seconds = 10;

                                                                            break;
                                                                    }
                                                                }
                                                            }
                                                            else
                                                            {
                                                                //authentication isn't enabled
                                                                nextCheck_Seconds = 10;
                                                            }
                                                        }
                                                        else if (memoryChecks > 6)
                                                        {
                                                            if (AuthenticationRequired != null)
                                                                AuthenticationRequired(this, null);

                                                            return;
                                                        }
                                                    }
                                                    else
                                                        memoryChecks = 0;

                                                    nextCheck = DateTime.UtcNow.AddSeconds(nextCheck_Seconds);
                                                }

                                                #endregion
                                            }
                                            while (DateTime.UtcNow < limit && !this.process.WaitForExit(500));
                                        }
                                    }

                                    if (WindowChanged != null && !this.process.HasExited)
                                    {
                                        wce.Type = WindowChangedEventArgs.EventType.DxWindowReady;
                                        WindowChanged(this, wce);
                                    }

                                    #region Find CoherentUI

                                    if (pidChild == 0)
                                    {
                                        do
                                        {
                                            if ((pidChild = GetChildProcess(process.Id)) > 0)
                                            {
                                                break;
                                            }
                                        }
                                        while (DateTime.UtcNow < limit && !this.process.WaitForExit(500));
                                    }

                                    if (pidChild > 0)
                                    {
                                        try
                                        {
                                            using (var child = Process.GetProcessById(pidChild))
                                            {
                                                limit = DateTime.UtcNow.AddSeconds(10);
                                                var pid = pidChild;

                                                do
                                                {
                                                    if ((pidChild = GetChildProcess(pid)) > 0)
                                                    {
                                                        break;
                                                    }
                                                }
                                                while (DateTime.UtcNow < limit && !child.WaitForExit(500));
                                            }

                                            if (pidChild > 0)
                                            {
                                                using (var child = Process.GetProcessById(pidChild))
                                                {
                                                    long mem = 0;
                                                    byte counter = 0;
                                                    limit = DateTime.UtcNow.AddSeconds(60);

                                                    do
                                                    {
                                                        if (mem > 0)
                                                            child.Refresh();
                                                        var _mem = child.PeakWorkingSet64;
                                                        if (_mem > mem)
                                                        {
                                                            mem = _mem;
                                                            counter = 0;
                                                        }
                                                        else if (++counter > 2)
                                                            break;
                                                    }
                                                    while (DateTime.UtcNow <limit && !child.WaitForExit(100));
                                                }
                                            }
                                        }
                                        catch (Exception ex)
                                        {
                                            Util.Logging.Log(ex);
                                        }
                                    }

                                    #endregion

                                    if (WindowChanged != null && !this.process.HasExited)
                                    {
                                        wce.Type = WindowChangedEventArgs.EventType.InGameCoherentUIReady;
                                        WindowChanged(this, wce);
                                    }

                                    return;
                                }
                                else if (className[0] == '#')
                                {
                                    var result = Find(handle, classCallback, textCallback);
                                    if (result != null)
                                    {
                                        CrashReason reason = CrashReason.Unknown;

                                        string text = result.Text;
                                        int i = text.IndexOf("Assertion:", 0, 100);
                                        if (i != -1)
                                        {
                                            int j = text.IndexOf('\n', i);
                                            if (text.IndexOf("Is your archive up to date?", i, j - i, StringComparison.OrdinalIgnoreCase) != -1
                                                || text.IndexOf("Client needs to be patched", i, j - i, StringComparison.OrdinalIgnoreCase) != -1)
                                            {
                                                reason = CrashReason.PatchRequired;
                                            }
                                        }

                                        if (WindowCrashed != null)
                                            WindowCrashed(this, reason);
                                    }
                                    return;
                                }
                                else if (className.Equals(LAUNCHER_WINDOW_CLASSNAME))
                                {
                                    if (WindowChanged != null)
                                    {
                                        wce.Type = WindowChangedEventArgs.EventType.TitleChanged;
                                        WindowChanged(this, wce);
                                    }

                                    #region Find CoherentUI

                                    var pidChild = 0;
                                    var limit = DateTime.UtcNow.AddSeconds(30);

                                        do
                                        {
                                            if ((pidChild = GetChildProcess(process.Id)) > 0)
                                            {
                                                break;
                                            }

                                            this.process.Refresh();
                                            if (_handle != this.process.MainWindowHandle)
                                                break;
                                        }
                                        while (DateTime.UtcNow < limit && !this.process.WaitForExit(500));

                                    if (pidChild > 0)
                                    {
                                        try
                                        {
                                            using (var child = Process.GetProcessById(pidChild))
                                            {
                                                limit = DateTime.UtcNow.AddSeconds(10);
                                                var pid = pidChild;

                                                do
                                                {
                                                    if ((pidChild = GetChildProcess(pid)) > 0)
                                                    {
                                                        break;
                                                    }

                                                    this.process.Refresh();
                                                    if (_handle != this.process.MainWindowHandle)
                                                        break;
                                                }
                                                while (DateTime.UtcNow < limit && !child.WaitForExit(500));
                                            }

                                            if (pidChild > 0)
                                            {
                                                using (var child = Process.GetProcessById(pidChild))
                                                {
                                                    long mem = 0;
                                                    byte counter = 0;
                                                    limit = DateTime.UtcNow.AddSeconds(60);

                                                    do
                                                    {
                                                        if (mem > 0)
                                                            child.Refresh();
                                                        var _mem = child.PeakWorkingSet64;
                                                        if (_mem > mem)
                                                        {
                                                            mem = _mem;
                                                            counter = 0;
                                                        }
                                                        else if (++counter > 2)
                                                            break;
                                                    }
                                                    while (DateTime.UtcNow < limit && !child.WaitForExit(100));

                                                    if (WindowChanged != null)
                                                    {
                                                        wce.Type = WindowChangedEventArgs.EventType.LauncherCoherentUIReady;
                                                        WindowChanged(this, wce);
                                                    }
                                                }
                                            }
                                        }
                                        catch (Exception ex)
                                        {
                                            Util.Logging.Log(ex);
                                        }
                                    }

                                    #endregion
                                }
                            }
                        }
                    }
                }
                while (!process.WaitForExit(500) && this.watcher != null);
            }

            /// <summary>
            /// Attempts to set the volume once available
            /// </summary>
            public async void SetVolume(float percent)
            {
                var t = DateTime.UtcNow.AddMilliseconds(30000);

                do
                {
                    int processId;
                    try
                    {
                        processId = this.process.Id;
                        if (this.process.HasExited)
                            return;
                    }
                    catch
                    {
                        return;
                    }

                    var r = await Task.Run<bool>(
                        delegate
                        {
                            try
                            {
                                return Windows.Volume.SetVolume(processId, percent);
                            }
                            catch
                            {
                                return false;
                            }
                        });

                    if (r)
                        break;
                    else
                        await Task.Delay(1000);
                }
                while (DateTime.UtcNow < t);
            }

            public static bool SetText(IntPtr window, string text)
            {
                try
                {
                    return NativeMethods.SetWindowText(window, text);
                }
                catch (Exception e)
                {
                    Util.Logging.Log(e);
                }

                return false;
            }

            /// <summary>
            /// Sets the bounds of the window and watches for it to revert back to its original bounds.
            /// If reverted, the bounds are set again.
            /// </summary>
            public async void SetBounds(IntPtr window, System.Drawing.Rectangle bounds, int timeout)
            {
                await Task.Factory.StartNew(
                    delegate
                    {
                        try
                        {
                            WatchBounds(window, bounds, timeout);
                        }
                        catch (Exception e)
                        {
                            Util.Logging.Log(e);
                        }
                    }, TaskCreationOptions.LongRunning);
            }

            public static bool SetBounds(Process process, System.Drawing.Rectangle bounds)
            {
                IntPtr handle;
                try
                {
                    handle = process.MainWindowHandle;
                    IntPtr ptr = Windows.FindWindow.Find(process.Id, DX_WINDOW_CLASSNAME);
                    if (ptr != IntPtr.Zero)
                    {
                        Windows.WindowSize.SetWindowPlacement(ptr, bounds, ShowWindowCommands.ShowNormal);
                        return true;
                    }
                }
                catch (Exception e)
                {
                    Util.Logging.Log(e);
                }

                return false;
            }

            private void WatchBounds(IntPtr window, System.Drawing.Rectangle bounds, int timeout)
            {
                var _placement = Windows.WindowSize.GetWindowPlacement(window);
                if (_placement.rcNormalPosition.Equals(bounds))
                    return;

                Windows.WindowSize.SetWindowPlacement(window, bounds, ShowWindowCommands.ShowNormal);

                //var attempts = 3;
                //var t = DateTime.UtcNow.AddMilliseconds(timeout);

                //do
                //{
                //    Thread.Sleep(500);

                //    //when gw2 is launched using -nopatchui, the window will reset its position while loading

                //    var placement = Windows.WindowSize.GetWindowPlacement(window);

                //    if (placement.showCmd == ShowWindowCommands.ShowNormal)
                //    {
                //        if (placement.rcNormalPosition.Equals(bounds))
                //        {

                //        }
                //        else if (placement.rcNormalPosition.Equals(_placement.rcNormalPosition))
                //        {
                //            if (--attempts > 0) //not all bounds are accepted; gw2 will do a best fit
                //            {
                //                Windows.WindowSize.SetWindowPlacement(window, bounds, ShowWindowCommands.ShowNormal);
                //            }
                //            else
                //            {
                //                break;
                //            }
                //        }
                //        else
                //        {
                //            if (attempts > 0)
                //            {
                //                attempts = 0;

                //                //retry once, assuming the original placement was changed
                //                _placement = Windows.WindowSize.GetWindowPlacement(window);
                //                Windows.WindowSize.SetWindowPlacement(window, bounds, ShowWindowCommands.ShowNormal);
                //            }
                //            else
                //            {
                //                //size was changed to something other than the original or set size
                //                //assuming something else moved the window - cancelling
                //                break;
                //            }
                //        }
                //    }
                //}
                //while (DateTime.UtcNow < t);
            }

            public static void SetTopMost(IntPtr handle)
            {
                try
                {
                    NativeMethods.SetWindowPos(handle, (IntPtr)WindowZOrder.HWND_TOPMOST, 0, 0, 0, 0, SetWindowPosFlags.SWP_NOMOVE | SetWindowPosFlags.SWP_NOSIZE);
                }
                catch (Exception ex)
                {
                    Util.Logging.Log(ex);
                }
            }

            public static void SetTopMost(Process process)
            {
                try
                {
                    SetTopMost(process.MainWindowHandle);
                }
                catch (Exception ex)
                {
                    Util.Logging.Log(ex);
                }
            }

            public static void RemoveWindowStyle(IntPtr handle, WindowStyle style)
            {
                var l1 = NativeMethods.GetWindowLongPtr(handle, (int)GWL.GWL_STYLE);
                IntPtr l2;

                if (IntPtr.Size == 4)
                    l2 = (IntPtr)((int)l1 & ~(int)style);
                else
                    l2 = (IntPtr)((long)l1 & ~(long)style);
                
                if (l2 != l1)
                    NativeMethods.SetWindowLongPtr(handle, (int)GWL.GWL_STYLE, l2);
            }
        }
    }
}
