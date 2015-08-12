using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Threading;

namespace Gw2Launcher.Client
{
    static partial class Launcher
    {
        private class WindowWatcher
        {
            public event EventHandler<IntPtr> WindowChanged;

            private const string WINDOW_CLASSNAME = "ArenaNet_Dx_Window_Class";

            private Process process;
            private Task watcher;

            /// <summary>
            /// Watches the process and reports when the DX window is created
            /// </summary>
            public WindowWatcher(Account account, Process p)
            {
                this.process = p;
                this.Account = account;

                watcher = Task.Factory.StartNew(WatchWindow);
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

            private void WatchWindow()
            {
                IntPtr handle;
                try
                {
                    handle = process.MainWindowHandle;
                }
                catch
                {
                    return;
                }

                IntPtr ptr = Windows.FindWindow.Find(this.process.Id, WINDOW_CLASSNAME);

                if (ptr != IntPtr.Zero)
                {
                    //already exists
                    if (WindowChanged != null)
                        WindowChanged(this, ptr);
                    return;
                }

                bool hasLaunched = false;
                byte count = 0;

                while (!this.process.HasExited && this.watcher != null)
                {
                    try
                    {
                        if (!hasLaunched)
                        {
                            try
                            {
                                if (!Windows.WindowSize.IsWindow(handle))
                                {
                                    //waiting for the launcher window to close
                                    hasLaunched = true;
                                }
                                else if (count == 100)
                                {
                                    //occassionally test to see if it was missed
                                    ptr = Windows.FindWindow.Find(this.process.Id, WINDOW_CLASSNAME);
                                    if (ptr != IntPtr.Zero)
                                    {
                                        if (WindowChanged != null)
                                            WindowChanged(this, ptr);
                                        return;
                                    }
                                    count = 0;
                                }
                                else
                                    count++;
                            }
                            catch
                            {
                            }

                            Thread.Sleep(100);
                        }
                        else
                        {
                            ptr = Windows.FindWindow.Find(this.process.Id, WINDOW_CLASSNAME);
                            if (ptr != IntPtr.Zero)
                            {
                                if (WindowChanged != null)
                                    WindowChanged(this, ptr);
                                return;
                            }
                            Thread.Sleep(100);
                        }
                    }
                    catch (Exception e)
                    {
                    }
                }
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
                        }
                    });
            }

            private void WatchBounds(IntPtr window, System.Drawing.Rectangle bounds, int timeout)
            {
                var _placement = Windows.WindowSize.GetWindowPlacement(window);
                if (_placement.rcNormalPosition.Equals(bounds))
                {
                    return;
                }

                Windows.WindowSize.SetWindowPlacement(window, bounds, Windows.WindowSize.WindowState.SW_SHOWNORMAL);

                timeout = Environment.TickCount + timeout;

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
                while (Environment.TickCount < timeout);
            }
        }
    }
}
