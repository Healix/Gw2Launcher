using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices;
using System.Diagnostics;

namespace Gw2Launcher.Windows
{
    static class WindowSize
    {
        [DllImport("user32.dll", SetLastError = true, EntryPoint="SetWindowPlacement")]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool _SetWindowPlacement(IntPtr hWnd, ref WINDOWPLACEMENT lpwndpl);
        [DllImport("user32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool GetWindowPlacement(IntPtr hWnd, ref WINDOWPLACEMENT lpwndpl);
        [DllImport("user32.dll", SetLastError = true)]
        static extern int SetWindowLong(IntPtr hWnd, int nIndex, int dwNewLong);
        [DllImport("user32.dll", SetLastError = true)]
        static extern int GetWindowLong(IntPtr hWnd, int nIndex);
        [DllImport("user32.dll", SetLastError = true)]
        public static extern bool GetClientRect(IntPtr hWnd, out RECT lpRect);
        [DllImport("user32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool GetWindowRect(IntPtr hWnd, out RECT lpRect);
        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool IsWindow(IntPtr hWnd);
        [DllImport("user32.dll", SetLastError = true)]
        static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int X, int Y, int cx, int cy, SetWindowPosFlags uFlags);

        [Flags()]
        public enum SetWindowPosFlags
        {
            SWP_NOSIZE = 0x1,
            SWP_NOMOVE = 0x2,
            SWP_NOZORDER = 0x4,
            SWP_NOREDRAW = 0x8,
            SWP_NOACTIVATE = 0x10,
            SWP_FRAMECHANGED = 0x20,
            SWP_DRAWFRAME = SWP_FRAMECHANGED,
            SWP_SHOWWINDOW = 0x40,
            SWP_HIDEWINDOW = 0x80,
            SWP_NOCOPYBITS = 0x100,
            SWP_NOOWNERZORDER = 0x200,
            SWP_NOREPOSITION = SWP_NOOWNERZORDER,
            SWP_NOSENDCHANGING = 0x400,
            SWP_DEFERERASE = 0x2000,
            SWP_ASYNCWINDOWPOS = 0x4000,
        }

        public struct POINTAPI
        {
            public int x;
            public int y;
        }

        public struct RECT
        {
            public int left;
            public int top;
            public int right;
            public int bottom;

            public bool Equals(RECT rect)
            {
                return this.left == rect.left && this.top == rect.top && this.right == rect.right && this.bottom == rect.bottom;
            }

            public bool Equals(System.Drawing.Rectangle rect)
            {
                return this.left == rect.Left && this.top == rect.Top && this.right == rect.Right && this.bottom == rect.Bottom;
            }

            public System.Drawing.Rectangle ToRectangle()
            {
                return new System.Drawing.Rectangle(this.left, this.top, this.right - this.left, this.bottom - this.top);
            }

            public static RECT FromRectangle(System.Drawing.Rectangle rect)
            {
                RECT r = new RECT();
                r.left = rect.Left;
                r.right = rect.Right;
                r.top = rect.Top;
                r.bottom = rect.Bottom;
                return r;
            }
        }

        public struct WINDOWPLACEMENT
        {
            public int length;
            public int flags;
            public int showCmd;
            public POINTAPI ptMinPosition;
            public POINTAPI ptMaxPosition;
            public RECT rcNormalPosition;
        }

        public enum WindowState
        {
            None = -1,
            SW_HIDE = 0,
            SW_SHOWNORMAL = 1,
            SW_SHOWMINIMIZED = 2,
            SW_SHOWMAXIMIZED = 3,
            SW_SHOWNOACTIVATE = 4,
            SW_SHOW = 5,
            SW_MINIMIZE = 6,
            SW_SHOWMINNOACTIVE = 7,
            SW_SHOWNA = 8,
            SW_RESTORE = 9,
            SW_SHOWDEFAULT = 10,
            SW_FORCEMINIMIZE = 11
        }

        public static System.Drawing.Rectangle GetWindowRect(Process p)
        {
            return GetWindowRect(p.MainWindowHandle);
        }

        public static System.Drawing.Rectangle GetWindowRect(IntPtr handle)
        {
            RECT windowRect = new RECT();
            if (!GetWindowRect(handle, out windowRect))
                throw new System.ComponentModel.Win32Exception(Marshal.GetLastWin32Error());

            return new System.Drawing.Rectangle(windowRect.left, windowRect.top, windowRect.right - windowRect.left, windowRect.bottom - windowRect.top);
        }

        public static void SetWindowPlacement(Process p, System.Drawing.Rectangle r, WindowState state)
        {
            SetWindowPlacement(p.MainWindowHandle, r, state);
        }

        public static void SetWindowPlacement(IntPtr handle, System.Drawing.Rectangle r, WindowState state)
        {
            WINDOWPLACEMENT windowPlacement = new WINDOWPLACEMENT();
            if (!GetWindowPlacement(handle, ref windowPlacement))
                throw new System.ComponentModel.Win32Exception(Marshal.GetLastWin32Error());

            RECT rect = new RECT();
            rect.left = r.Left;
            rect.top = r.Top;
            rect.right = r.Right;
            rect.bottom = r.Bottom;

            windowPlacement.rcNormalPosition = rect;
            if (state != WindowState.None)
                windowPlacement.showCmd = (int)state;

            if (!_SetWindowPlacement(handle, ref windowPlacement))
                throw new System.ComponentModel.Win32Exception(Marshal.GetLastWin32Error());
        }

        public static void SetWindowPlacement(IntPtr handle, ref WINDOWPLACEMENT placement)
        {
            if (!_SetWindowPlacement(handle, ref placement))
                throw new System.ComponentModel.Win32Exception(Marshal.GetLastWin32Error());
        }

        public static WINDOWPLACEMENT GetWindowPlacement(IntPtr handle)
        {
            WINDOWPLACEMENT windowPlacement = new WINDOWPLACEMENT();
            if (!GetWindowPlacement(handle, ref windowPlacement))
                throw new System.ComponentModel.Win32Exception(Marshal.GetLastWin32Error());
            return windowPlacement;
        }
    }
}
