using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Gw2Launcher.Windows.Native;
using System.Runtime.InteropServices;
using System.Diagnostics;

namespace Gw2Launcher.Client
{
    static partial class Launcher
    {
        private enum LockCode : uint
        {
            Lock = 1,
            Unlock = 2,
        }

        [DllImport("user32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool LockSetForegroundWindow(LockCode uLockCode);

        private class WindowLock
        {
            private IntPtr window;
            private byte locks;

            public interface ILock : IDisposable
            {
                void Release(IntPtr handle);
            }

            private class Lock_ : ILock
            {
                private WindowLock _lock;

                public Lock_(WindowLock l)
                {
                    this._lock = l;
                }

                ~Lock_()
                {
                    Release(IntPtr.Zero);
                }

                public void Release(IntPtr handle)
                {
                    GC.SuppressFinalize(this);

                    WindowLock _l;

                    lock (this)
                    {
                        _l = this._lock;

                        if (_l != null)
                        {
                            this._lock = null;
                        }
                        else
                        {
                            return;
                        }
                    }

                    _l.Unlock(handle);
                }

                public void Dispose()
                {
                    Release(IntPtr.Zero);
                }
            }

            public ILock Lock()
            {
                lock (this)
                {
                    if (++locks == 1)
                    {
                        window = NativeMethods.GetForegroundWindow();
                        LockSetForegroundWindow(LockCode.Lock);
                    }
                }

                return new Lock_(this);
            }

            private static IntPtr FindZOrder(HashSet<IntPtr> handles, bool highest = false)
            {
                var count = handles.Count;
                var window = IntPtr.Zero;

                if (count > 1)
                {
                    NativeMethods.EnumWindows(
                        delegate(IntPtr h, IntPtr p)
                        {
                            if (handles.Remove(h))
                            {
                                window = h;
                                if (--count == 0 || highest)
                                    return false;
                            }
                            return true;
                        }, IntPtr.Zero);
                }
                else
                {
                    foreach (var h in handles)
                    {
                        return h;
                    }
                }

                return window;
            }

            private void Unlock(IntPtr handle)
            {
                lock (this)
                {
                    if (locks == 0)
                    {
                        return;
                    }

                    if (--locks == 0)
                    {
                        LockSetForegroundWindow(LockCode.Unlock);
                    }

                    if (handle == IntPtr.Zero)
                    {
                        return;
                    }

                    try
                    {
                        var active = LinkedProcess.GetActive();
                        var handles = new HashSet<IntPtr>();
                        var w = IntPtr.Zero;

                        sbyte foreground;

                        if (NativeMethods.GetForegroundWindow() == handle)
                            foreground = 0;
                        else
                            foreground = -1;

                        foreach (var a in active)
                        {
                            if (a.Account.State == AccountState.ActiveGame)
                            {
                                try
                                {
                                    w = Windows.FindWindow.FindMainWindow(a.Process);

                                    if (w != IntPtr.Zero && w != handle)
                                    {
                                        if (foreground == 0 && w == window)
                                            foreground = 1;

                                        if ((a.Account.Settings.WindowOptions & Settings.WindowOptions.TopMost) == 0)
                                        {
                                            handles.Add(w);
                                        }
                                    }
                                }
                                catch { }
                            }
                        }

                        if (foreground == 1)
                        {
                            NativeMethods.SetForegroundWindow(window);
                        }

                        if (handles.Count > 0)
                        {
                            w = FindZOrder(handles);
                        }
                        else
                        {
                            w = UI.formMain.MainWindowHandle;
                            if (Windows.WindowLong.HasValue(w, GWL.GWL_EXSTYLE, WindowStyle.WS_EX_TOPMOST))
                                w = IntPtr.Zero;
                        }

                        if (w != IntPtr.Zero)
                        {
                            SetZOrder(handle, w);
                        }
                    }
                    catch
                    {

                    }
                }
            }

            private static bool SetZOrder(IntPtr handle, IntPtr zorder, bool async = true)
            {
                var flags = SetWindowPosFlags.SWP_DEFERERASE | SetWindowPosFlags.SWP_NOACTIVATE | SetWindowPosFlags.SWP_NOMOVE | SetWindowPosFlags.SWP_NOOWNERZORDER | SetWindowPosFlags.SWP_NOREDRAW | SetWindowPosFlags.SWP_NOREPOSITION | SetWindowPosFlags.SWP_NOSENDCHANGING | SetWindowPosFlags.SWP_NOSIZE;
                if (async)
                    flags |= SetWindowPosFlags.SWP_ASYNCWINDOWPOS;
                return NativeMethods.SetWindowPos(handle, zorder, 0, 0, 0, 0, flags);
            }

            /// <summary>
            /// Places the window behind all other windows
            /// </summary>
            public static void ToBottom(IntPtr handle, bool async = true)
            {
                SetZOrder(handle, (IntPtr)WindowZOrder.HWND_BOTTOM, async);
            }

            /// <summary>
            /// Forces the window above other windows
            /// </summary>
            public static void ToTop(IntPtr handle, bool async = true)
            {
                SetZOrder(handle, (IntPtr)WindowZOrder.HWND_TOPMOST, async);
                SetZOrder(handle, (IntPtr)WindowZOrder.HWND_NOTOPMOST, async);
            }
            
            /// <summary>
            /// Places the window behind other active windows
            /// </summary>
            public static void ToBackground(IntPtr handle, bool async = true, bool highest = false)
            {
                var active = LinkedProcess.GetActive();
                var handles = new HashSet<IntPtr>();
                var w = IntPtr.Zero;

                foreach (var a in active)
                {
                    if (a.Account.State == AccountState.ActiveGame)
                    {
                        try
                        {
                            w = Windows.FindWindow.FindMainWindow(a.Process);

                            if (w != IntPtr.Zero && w != handle)
                            {
                                if ((a.Account.Settings.WindowOptions & Settings.WindowOptions.TopMost) == 0)
                                {
                                    handles.Add(w);
                                }
                            }
                        }
                        catch { }
                    }
                }

                if (handles.Count > 0)
                {
                    w = FindZOrder(handles, highest);
                }
                else
                {
                    w = UI.formMain.MainWindowHandle;
                    if (Windows.WindowLong.HasValue(w, GWL.GWL_EXSTYLE, WindowStyle.WS_EX_TOPMOST))
                        w = IntPtr.Zero;
                }

                if (w != IntPtr.Zero)
                {
                    SetZOrder(handle, w, async);
                }
            }
        }
    }
}
