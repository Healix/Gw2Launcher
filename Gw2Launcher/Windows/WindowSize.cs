using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices;
using System.Diagnostics;
using Gw2Launcher.Windows.Native;
using System.Windows.Forms;

namespace Gw2Launcher.Windows
{
    static class WindowSize
    {
        public static System.Drawing.Rectangle GetWindowRect(Process p)
        {
            return GetWindowRect(Windows.FindWindow.FindMainWindow(p));
        }

        public static System.Drawing.Rectangle GetWindowRect(IntPtr handle)
        {
            RECT windowRect = new RECT();
            if (!NativeMethods.GetWindowRect(handle, out windowRect))
                throw new System.ComponentModel.Win32Exception(Marshal.GetLastWin32Error());

            return new System.Drawing.Rectangle(windowRect.left, windowRect.top, windowRect.right - windowRect.left, windowRect.bottom - windowRect.top);
        }

        /// <summary>
        /// Sets the window's bounds relative to the desktop's working area
        /// </summary>
        public static void SetWindowPlacement(Process p, System.Drawing.Rectangle r, ShowWindowCommands state)
        {
            SetWindowPlacement(Windows.FindWindow.FindMainWindow(p), r, state);
        }

        /// <summary>
        /// Sets the window's bounds relative to the desktop's working area
        /// </summary>
        public static void SetWindowPlacement(IntPtr handle, System.Drawing.Rectangle r, ShowWindowCommands state)
        {
            WINDOWPLACEMENT windowPlacement = new WINDOWPLACEMENT();
            if (!NativeMethods.GetWindowPlacement(handle, ref windowPlacement))
                throw new System.ComponentModel.Win32Exception(Marshal.GetLastWin32Error());

            RECT rect = new RECT();
            rect.left = r.Left;
            rect.top = r.Top;
            rect.right = r.Right;
            rect.bottom = r.Bottom;

            windowPlacement.rcNormalPosition = rect;
            windowPlacement.showCmd = state;

            if (!NativeMethods.SetWindowPlacement(handle, ref windowPlacement))
                throw new System.ComponentModel.Win32Exception(Marshal.GetLastWin32Error());
        }

        /// <summary>
        /// Sets the window's bounds relative to the desktop's working area
        /// </summary>
        public static void SetWindowPlacement(IntPtr handle, ref WINDOWPLACEMENT placement)
        {
            if (!NativeMethods.SetWindowPlacement(handle, ref placement))
                throw new System.ComponentModel.Win32Exception(Marshal.GetLastWin32Error());
        }

        /// <summary>
        /// Gets the window's bounds relative to the desktop's working area
        /// </summary>
        public static WINDOWPLACEMENT GetWindowPlacement(IntPtr handle)
        {
            WINDOWPLACEMENT windowPlacement = new WINDOWPLACEMENT();
            if (!NativeMethods.GetWindowPlacement(handle, ref windowPlacement))
                throw new System.ComponentModel.Win32Exception(Marshal.GetLastWin32Error());
            return windowPlacement;
        }
    }
}
