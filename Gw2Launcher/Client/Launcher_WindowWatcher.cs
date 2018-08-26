using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Threading;
using System.Runtime.InteropServices;

namespace Gw2Launcher.Client
{
    static partial class Launcher
    {
        private class WindowWatcher
        {
            [DllImport("User32.dll", CharSet = CharSet.Auto, SetLastError = true)]
            [return: MarshalAs(UnmanagedType.Bool)]
            static extern bool PeekMessage(ref NativeMessage message, IntPtr handle, uint filterMin, uint filterMax, uint flags);

            [DllImport("User32.dll", CharSet = CharSet.Auto, SetLastError = true)]
            [return: MarshalAs(UnmanagedType.Bool)]
            static extern bool GetMessage(ref NativeMessage message, IntPtr handle, uint filterMin, uint filterMax);

            [DllImport("user32.dll")]
            static extern IntPtr DispatchMessage([In] ref NativeMessage lpmsg);

            [DllImport("user32.dll")]
            static extern bool TranslateMessage([In] ref NativeMessage lpMsg);

            [StructLayout(LayoutKind.Sequential)]
            public struct NativeMessage
            {
                public IntPtr handle;
                public uint msg;
                public IntPtr wParam;
                public IntPtr lParam;
                public uint time;
                public System.Drawing.Point p;
            }

            private delegate void WinEventDelegate(IntPtr hWinEventHook, uint eventType, IntPtr hwnd, int idObject, int idChild, uint dwEventThread, uint dwmsEventTime);

            [DllImport("user32.dll")]
            static extern IntPtr SetWinEventHook(uint eventMin, uint eventMax, IntPtr hmodWinEventProc, WinEventDelegate lpfnWinEventProc, uint idProcess, uint idThread, uint dwFlags);
            [DllImport("user32.dll")]
            static extern bool UnhookWinEvent(IntPtr hWinEventHook);

            public event EventHandler<IntPtr> WindowChanged;
            public event EventHandler<CrashReason> WindowCrashed;
            public event EventHandler WindowCreated;

            private const string WINDOW_CLASSNAME = "ArenaNet_Dx_Window_Class";
            private const string EDIT_CLASSNAME = "Edit";

            public enum CrashReason
            {
                Unknown,
                PatchRequired
            }

            private Process process;
            private Task watcher;
            private bool watchBounds;

            /// <summary>
            /// Watches the process and reports when the DX window is created
            /// </summary>
            public WindowWatcher(Account account, Process p, bool watchBounds, string args)
            {
                this.process = p;
                this.Account = account;
                this.watchBounds = watchBounds;
                this.Args = args;
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

                StringBuilder buffer = new StringBuilder(WINDOW_CLASSNAME.Length);
                int length = 0;

                while (this.watcher != null)
                {
                    try
                    {
                        process.Refresh();
                        handle = process.MainWindowHandle;
                    }
                    catch (Exception e) 
                    {
                        Util.Logging.Log(e);
                    }
                    
                    try
                    {
                        if (this.process.HasExited)
                        {
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
                        buffer.Length = 0;
                        Windows.FindWindow.GetClassName(handle, buffer, buffer.Capacity + 1);

                        int l;
                        if ((l = buffer.Length) != length)
                        {
                            length = l;

                            if (l > 0)
                            {
                                string className = buffer.ToString();
                                if (className.Equals(WINDOW_CLASSNAME))
                                {
                                    if (WindowCreated != null)
                                        WindowCreated(this, EventArgs.Empty);

                                    if (watchBounds)
                                    {
                                        using (var volumeControl = new Windows.Volume.VolumeControl(process.Id))
                                        {
                                            //already loaded if audio is initialized (only checks default playback device)
                                            var hasVolume = volumeControl.Query();

                                            if (hasVolume)
                                            {
                                                if (WindowChanged != null && !this.process.HasExited)
                                                    WindowChanged(this, handle);

                                                return;
                                            }

                                            var hasMessage = false;
                                            var message = new NativeMessage();

                                            WinEventDelegate callback =
                                                delegate(IntPtr hWinEventHook, uint eventType, IntPtr hwnd, int idObject, int idChild, uint dwEventThread, uint dwmsEventTime)
                                                {
                                                    hasMessage = true;
                                                };

                                            var _message = GCHandle.Alloc(message);
                                            var _callback = GCHandle.Alloc(callback);
                                            var _hook = IntPtr.Zero;

                                            try
                                            {
                                                _hook = SetWinEventHook(0, uint.MaxValue, IntPtr.Zero, callback, (uint)process.Id, 0, 0);

                                                var t = DateTime.UtcNow.AddSeconds(30);
                                                var c = 0;
                                                do
                                                {
                                                    Thread.Sleep(100);

                                                    if (c++ > 3)
                                                    {
                                                        if (volumeControl.Query())
                                                        {
                                                            break;
                                                        }
                                                        c = 0;
                                                    }

                                                    PeekMessage(ref message, IntPtr.Zero, 0, 0, 0);
                                                }
                                                while (!hasMessage && DateTime.UtcNow < t && !this.process.HasExited);
                                            }
                                            catch (Exception e)
                                            {
                                                Util.Logging.Log(e);
                                            }
                                            finally
                                            {
                                                if (_hook != IntPtr.Zero)
                                                    UnhookWinEvent(_hook);

                                                _message.Free();
                                                _callback.Free();
                                            }

                                            //already exists
                                            if (WindowChanged != null && !this.process.HasExited)
                                                WindowChanged(this, handle);
                                        }
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
                            }
                        }
                    }

                    Thread.Sleep(500);
                }
            }

            private bool HasVolume(int processId)
            {
                try
                {
                    float v;
                    return Windows.Volume.GetVolume(processId, out v);
                }
                catch
                {
                    return false;
                }
            }

            /// <summary>
            /// Attempts to set the volume once available
            /// </summary>
            public async void SetVolume(float percent)
            {
                DateTime t = DateTime.UtcNow.AddMilliseconds(30000);
                bool first = false;

                do
                {
                    if (first)
                        await Task.Delay(1000);
                    else
                        first = true;

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
                }
                while (DateTime.UtcNow < t);
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
                    IntPtr ptr = Windows.FindWindow.Find(process.Id, WINDOW_CLASSNAME);
                    if (ptr != IntPtr.Zero)
                    {
                        Windows.WindowSize.SetWindowPlacement(ptr, bounds, Windows.WindowSize.WindowState.SW_SHOWNORMAL);
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
                {
                    return;
                }

                var attempts = 3;

                Windows.WindowSize.SetWindowPlacement(window, bounds, Windows.WindowSize.WindowState.SW_SHOWNORMAL);

                var t = DateTime.UtcNow.AddMilliseconds(timeout);

                do
                {
                    Thread.Sleep(100);

                    //when gw2 is launched using -nopatchui, the window will reset its position while loading

                    var placement = Windows.WindowSize.GetWindowPlacement(window);

                    if (placement.showCmd == (int)Windows.WindowSize.WindowState.SW_SHOWNORMAL)
                    {
                        if (placement.rcNormalPosition.Equals(bounds))
                        {

                        }
                        else if (placement.rcNormalPosition.Equals(_placement.rcNormalPosition))
                        {
                            if (--attempts > 0) //not all bounds are accepted; gw2 will do a best fit
                            {
                                Windows.WindowSize.SetWindowPlacement(window, bounds, Windows.WindowSize.WindowState.SW_SHOWNORMAL);
                            }
                            else
                            {
                                break;
                            }
                        }
                        else
                        {
                            if (attempts > 0)
                            {
                                attempts = 0;

                                //retry once, assuming the original placement was changed
                                _placement = Windows.WindowSize.GetWindowPlacement(window);
                                Windows.WindowSize.SetWindowPlacement(window, bounds, Windows.WindowSize.WindowState.SW_SHOWNORMAL);
                            }
                            else
                            {
                                //size was changed to something other than the original or set size
                                //assuming something else moved the window - cancelling
                                break;
                            }
                        }
                    }
                }
                while (DateTime.UtcNow < t);
            }
        }
    }
}
