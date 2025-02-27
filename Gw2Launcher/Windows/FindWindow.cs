using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices;
using Gw2Launcher.Windows.Native;
using System.Diagnostics;

namespace Gw2Launcher.Windows
{
    static class FindWindow
    {
        [DllImport(NativeMethods.USER32)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool EnumWindows(EnumWindowsProc lpEnumFunc, IntPtr lParam);

        [DllImport(NativeMethods.USER32)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool EnumThreadWindows(uint dwThreadId, EnumWindowsProc lpfn, IntPtr lParam);

        [DllImport(NativeMethods.USER32)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool EnumChildWindows(IntPtr hwndParent, EnumWindowsProc lpEnumFunc, IntPtr lParam);

        [DllImport(NativeMethods.USER32, SetLastError = true)]
        private static extern IntPtr GetWindow(IntPtr hWnd, GetWindowType uCmd);

        private enum GetWindowType : uint
        {
            /// <summary>
            /// The retrieved handle identifies the window of the same type that is highest in the Z order.
            /// <para/>
            /// If the specified window is a topmost window, the handle identifies a topmost window.
            /// If the specified window is a top-level window, the handle identifies a top-level window.
            /// If the specified window is a child window, the handle identifies a sibling window.
            /// </summary>
            GW_HWNDFIRST = 0,
            /// <summary>
            /// The retrieved handle identifies the window of the same type that is lowest in the Z order.
            /// <para />
            /// If the specified window is a topmost window, the handle identifies a topmost window.
            /// If the specified window is a top-level window, the handle identifies a top-level window.
            /// If the specified window is a child window, the handle identifies a sibling window.
            /// </summary>
            GW_HWNDLAST = 1,
            /// <summary>
            /// The retrieved handle identifies the window below the specified window in the Z order.
            /// <para />
            /// If the specified window is a topmost window, the handle identifies a topmost window.
            /// If the specified window is a top-level window, the handle identifies a top-level window.
            /// If the specified window is a child window, the handle identifies a sibling window.
            /// </summary>
            GW_HWNDNEXT = 2,
            /// <summary>
            /// The retrieved handle identifies the window above the specified window in the Z order.
            /// <para />
            /// If the specified window is a topmost window, the handle identifies a topmost window.
            /// If the specified window is a top-level window, the handle identifies a top-level window.
            /// If the specified window is a child window, the handle identifies a sibling window.
            /// </summary>
            GW_HWNDPREV = 3,
            /// <summary>
            /// The retrieved handle identifies the specified window's owner window, if any.
            /// </summary>
            GW_OWNER = 4,
            /// <summary>
            /// The retrieved handle identifies the child window at the top of the Z order,
            /// if the specified window is a parent window; otherwise, the retrieved handle is NULL.
            /// The function examines only child windows of the specified window. It does not examine descendant windows.
            /// </summary>
            GW_CHILD = 5,
            /// <summary>
            /// The retrieved handle identifies the enabled popup window owned by the specified window (the
            /// search uses the first such window found using GW_HWNDNEXT); otherwise, if there are no enabled
            /// popup windows, the retrieved handle is that of the specified window.
            /// </summary>
            GW_ENABLEDPOPUP = 6
        }

        public delegate bool EnumWindowsProc(IntPtr hWnd, IntPtr lParam);
        public delegate bool TextComparer(string name, StringBuilder value);
        
        public class SearchData
        {
            public TextComparer className;
            public uint processId;
            public StringBuilder buffer;
            public IntPtr hWnd;
            public List<SearchResult> results;
            public int limit;
            public TextComparer text;
        }

        public class SearchResult
        {
            public SearchResult(IntPtr handle)
            {
                this.Handle = handle;
            }

            public IntPtr Handle;

            private string className;
            public string ClassName
            {
                get
                {
                    if (className == null)
                    {
                        StringBuilder buffer = new StringBuilder(250);
                        GetClassName(buffer);
                        className = buffer.ToString();
                    }
                    return className;
                }
                set
                {
                    className = value;
                }
            }

            private string text;
            public string Text
            {
                get
                {
                    if (text == null)
                    {
                        StringBuilder buffer = new StringBuilder(250);
                        GetText(buffer);
                        text = buffer.ToString();
                    }
                    return text;
                }
                set
                {
                    text = value;
                }
            }
            
            public void GetClassName(StringBuilder buffer)
            {
                buffer.Length = 0;
                buffer.EnsureCapacity(250);

                NativeMethods.GetClassName(this.Handle, buffer, buffer.Capacity + 1);
            }

            public void GetText(StringBuilder buffer)
            {
                int length = NativeMethods.GetWindowTextLength(this.Handle);
                if (length == 0)
                {
                    length = NativeMethods.SendMessage(this.Handle, (int)WindowMessages.WM_GETTEXTLENGTH, 0, null);
                    if (length == 0)
                        text = "";
                    else
                    {
                        buffer.Length = 0;
                        buffer.EnsureCapacity(length);

                        NativeMethods.SendMessage(this.Handle, (int)WindowMessages.WM_GETTEXT, length, buffer);
                        text = buffer.ToString();
                    }
                }
                else
                {
                    buffer.Length = 0;
                    buffer.EnsureCapacity(length);

                    NativeMethods.GetWindowText(this.Handle, buffer, buffer.Capacity + 1);
                    text = buffer.ToString();
                }
            }
        }

        public static IntPtr Find(int processId, string[] classNames, StringBuilder buffer = null)
        {
            SearchData sd = new SearchData
            {
                limit = 1,
                processId = (uint)processId,
                buffer = buffer != null ? buffer : new StringBuilder(classNames == null ? 250 : classNames[0].Length + 1),
                results = new List<SearchResult>()
            };

            if (classNames != null)
            {
                sd.className = new TextComparer(
                    delegate(string cn, StringBuilder sb)
                    {
                        for (var i = classNames.Length - 1; i >= 0;--i)
                        {
                            if (sb.Length == classNames[i].Length && sb.ToString().Equals(classNames[i]))
                            {
                                return true;
                            }
                        }
                        return false;
                    });
            }

            var h = GCHandle.Alloc(sd);
            try
            {
                EnumWindows(new EnumWindowsProc(EnumWindow), GCHandle.ToIntPtr(h));
            }
            finally
            {
                if (h.IsAllocated)
                    h.Free();
            }

            if (sd.results.Count > 0)
                return sd.results[0].Handle;

            return IntPtr.Zero;
        }

        public static IntPtr Find(int processId, string className, StringBuilder buffer = null)
        {
            SearchData sd = new SearchData
            {
                limit = 1,
                processId = (uint)processId,
                buffer = buffer != null ? buffer : new StringBuilder(className == null ? 250 : className.Length + 1),
                results = new List<SearchResult>()
            };

            if (className != null)
            {
                sd.className = new TextComparer(
                    delegate(string cn, StringBuilder sb)
                    {
                        return sb.Length == className.Length && sb.ToString().Equals(className);
                    });
            }

            var h = GCHandle.Alloc(sd);
            try
            {
                EnumWindows(new EnumWindowsProc(EnumWindow), GCHandle.ToIntPtr(h));
            }
            finally
            {
                if (h.IsAllocated)
                    h.Free();
            }

            if (sd.results.Count > 0)
                return sd.results[0].Handle;

            return IntPtr.Zero;
        }
        
        public static List<SearchResult> FindChildren(IntPtr parent)
        {
            return FindChildren(parent, null, null, 0);
        }

        public static List<SearchResult> FindChildren(IntPtr parent, TextComparer className, TextComparer text, int limit)
        {
            SearchData sd = new SearchData
            {
                limit = limit,
                className = className,
                text = text,
                buffer = new StringBuilder(250),
                results = new List<SearchResult>()
            };

            var h = GCHandle.Alloc(sd);
            try
            {
                EnumChildWindows(parent, new EnumWindowsProc(EnumWindow),  GCHandle.ToIntPtr(h));
            }
            finally
            {
                if (h.IsAllocated)
                    h.Free();
            }

            return sd.results;
        }
                
        /// <summary>
        /// Returns the main window handle; additionally searches when running Wine
        /// </summary>
        public static IntPtr FindMainWindow(System.Diagnostics.Process p)
        {
            var h = p.MainWindowHandle;
            if (h == IntPtr.Zero && Settings.IsRunningWine)
            {
                h = Find(p);
            }
            return h;
        }
        
        /// <summary>
        /// Enumerates the threads of the specified process to find a window
        /// </summary>
        public static IntPtr Find(System.Diagnostics.Process p)
        {
            var windows = new List<IntPtr>();
            var found = false;
            var threads = p.Threads;

            var proc = new EnumWindowsProc(
                delegate(IntPtr hwnd, IntPtr lparam)
                {
                    if (NativeMethods.IsWindowVisible(hwnd))
                    {
                        windows.Add(hwnd);

                        if (GetWindow(hwnd, GetWindowType.GW_OWNER) == IntPtr.Zero)
                        {
                            found = true;
                            return false;
                        }

                        return false;
                    }

                    return true;
                });

            for (int i = 0, count = threads.Count; i < count; ++i)
            {
                EnumThreadWindows((uint)threads[i].Id, proc, IntPtr.Zero);
                if (found)
                    break;
            }

            if (found)
            {
                return windows[windows.Count - 1];
            }
            else if (windows.Count > 0)
            {
                return windows[0];
            }

            return IntPtr.Zero;
        }

        /// <summary>
        /// Enumerates all windows to find a window for the specified process
        /// </summary>
        public static IntPtr Find(int processId)
        {
            var windows = new List<IntPtr>();
            var found = false;

            EnumWindows(new EnumWindowsProc(
                delegate(IntPtr hwnd, IntPtr lparam)
                {
                    uint pid;
                    NativeMethods.GetWindowThreadProcessId(hwnd, out pid);

                    if (pid == processId && NativeMethods.IsWindowVisible(hwnd))
                    {
                        windows.Add(hwnd);

                        if (GetWindow(hwnd, GetWindowType.GW_OWNER) == IntPtr.Zero)
                        {
                            found = true;
                            return false;
                        }
                    }

                    return true;
                }), IntPtr.Zero);

            if (found)
            {
                return windows[windows.Count - 1];
            }
            else if (windows.Count > 0)
            {
                return windows[0];
            }

            return IntPtr.Zero;
        }


        private static bool EnumWindow(IntPtr hWnd, IntPtr lParam)
        {
            var data = (SearchData)GCHandle.FromIntPtr(lParam).Target;

            if (data.processId != 0)
            {
                uint processId;
                NativeMethods.GetWindowThreadProcessId(hWnd, out processId);
                if (processId != data.processId)
                    return true;
            }

            SearchResult result = new SearchResult(hWnd);
            
            if (data.className != null || data.text != null)
            {
                result.GetClassName(data.buffer);
                if (data.className != null && !data.className(null, data.buffer))
                    return true;
                string name;
                result.ClassName = name = data.buffer.ToString();

                if (data.text != null)
                {
                    result.GetText(data.buffer);
                    if (!data.text(name, data.buffer))
                        return true;
                    result.Text = data.buffer.ToString();
                }

            }

            data.results.Add(result);

            return data.limit == 0 || data.limit < data.results.Count;
        }

        public static bool FocusWindow(System.Windows.Forms.Form f, bool force = false)
        {
            var b = FocusWindow(f.Handle, false);

            if (!b && force)
            {
                b = ForceWindowToFront(f);
            }

            return b;
        }

        public static bool FocusWindow(IntPtr handle, bool force = false)
        {
            var p = WindowSize.GetWindowPlacement(handle);
            if (p.showCmd == ShowWindowCommands.ShowMinimized)
                NativeMethods.ShowWindow(handle, ShowWindowCommands.Restore);
            var b = NativeMethods.SetForegroundWindow(handle);
            if (!b)
            {
                NativeMethods.BringWindowToTop(handle);
                if (force)
                {
                    b = ForceWindowToFront(handle);
                }
            }
            return b;
        }

        public static async Task<bool> FocusWindowAsync(System.Windows.Forms.Form f, bool force = false)
        {
            var h = f.Handle;

            var b = await Task.Run<bool>(
                delegate
                {
                    return FocusWindow(h, false);
                });

            if (!b && force)
            {
                var children = GetHandles(f.OwnedForms);

                b = await Task.Run<bool>(
                    delegate
                    {
                        return ForceWindowToFront(h, children);
                    });
            }

            return b;
        }

        public static Task<bool> FocusWindowAsync(IntPtr handle, bool force = false)
        {
            return Task.Run<bool>(
                delegate
                {
                    return FocusWindow(handle, force);
                });
        }

        public static bool FocusWindow(System.Diagnostics.Process p, bool force = false)
        {
            var h = Windows.FindWindow.FindMainWindow(p);
            if (h != IntPtr.Zero)
                return FocusWindow(h, force);
            return false;
        }

        private static IntPtr[] GetHandles(System.Windows.Forms.Form[] forms)
        {
            if (forms.Length == 0)
            {
                return null;
            }

            var handles = new IntPtr[forms.Length];

            for (var i = 0; i < forms.Length; i++)
            {
                if (forms[i].IsHandleCreated)
                {
                    handles[i] = forms[i].Handle;
                }
            }

            return handles;
        }

        public static bool ForceWindowToFront(System.Windows.Forms.Form f)
        {
            return ForceWindowToFront(f.Handle, GetHandles(f.OwnedForms));
        }

        public static bool ForceWindowToFront(IntPtr handle, IntPtr[] children = null)
        {
            try
            {
                var top = IsTopMost(handle);
                var flags = SetWindowPosFlags.SWP_ASYNCWINDOWPOS | SetWindowPosFlags.SWP_NOMOVE | SetWindowPosFlags.SWP_NOOWNERZORDER | SetWindowPosFlags.SWP_NOSENDCHANGING | SetWindowPosFlags.SWP_NOSIZE;

                NativeMethods.SetWindowPos(handle, (IntPtr)WindowZOrder.HWND_TOPMOST, 0, 0, 0, 0, flags);
                if (!top)
                {
                    NativeMethods.SetWindowPos(handle, (IntPtr)WindowZOrder.HWND_NOTOPMOST, 0, 0, 0, 0, flags);
                }

                if (children != null)
                {
                    for (var i = 0; i < children.Length; i++)
                    {
                        if (children[i] != IntPtr.Zero)
                        {
                            NativeMethods.SetWindowPos(children[i], (IntPtr)(IsTopMost(children[i]) ? WindowZOrder.HWND_TOPMOST : WindowZOrder.HWND_TOP), 0, 0, 0, 0, flags | SetWindowPosFlags.SWP_NOACTIVATE);
                        }
                    }
                }

                return true;
            }
            catch { }

            return false;
        }

        public static bool IsTopMost(IntPtr handle)
        {
            return WindowLong.HasValue(handle, GWL.GWL_EXSTYLE, WindowStyle.WS_EX_TOPMOST);
        }

        public static bool IsMinimized(IntPtr handle)
        {
            return NativeMethods.IsIconic(handle);
        }

        /// <summary>
        /// Returns if the window handle has higher privileges than the current process
        /// </summary>
        public static bool IsPrivilegedWindow(IntPtr handle)
        {
            uint pid;
            NativeMethods.GetWindowThreadProcessId(handle, out pid);
            if (pid > 0)
            {
                try
                {
                    using (var p = Process.GetProcessById((int)pid))
                    {
                        handle = p.Handle;
                        return false;
                    }
                }
                catch
                {
                    return true;
                }
            }
            return false;
        }

        public static bool IsWindowOverlapped(IntPtr h)
        {
            var w = GetWindow(h, GetWindowType.GW_HWNDFIRST);
            byte counter = 0;
            RECT r1;
            NativeMethods.GetWindowRect(h, out r1);

            while (w != IntPtr.Zero && w != h)
            {
                if (NativeMethods.IsWindowVisible(w))
                {
                    RECT r2;
                    if (NativeMethods.GetWindowRect(w, out r2))
                    {
                        if (r1.left < r2.right && r1.right > r2.left && r1.top < r2.bottom && r1.bottom > r2.top)
                        {
                            return true;
                        }
                    }
                }

                w = GetWindow(w, GetWindowType.GW_HWNDNEXT);
                
                //next can go back to a previous window, causing an infinite loop
                if (++counter == 255)
                    break;
            }

            return false;
        }
    }
}
