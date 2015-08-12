using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices;

namespace Gw2Launcher.Windows
{
    static class FindWindow
    {
        [DllImport("user32.dll", SetLastError = true)]
        static extern uint GetWindowThreadProcessId(IntPtr hWnd, out uint processId);

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool EnumWindows(EnumWindowsProc lpEnumFunc, ref SearchData data);

        [DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        public static extern int GetClassName(IntPtr hWnd, StringBuilder lpClassName, int nMaxCount);

        [DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        static extern int GetWindowTextLength(IntPtr hWnd);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern int GetWindowText(IntPtr hWnd, StringBuilder lpString, int nMaxCount);

        public delegate bool EnumWindowsProc(IntPtr hWnd, ref SearchData data);

        public class SearchData
        {
            public string className;
            public uint processId;
            public StringBuilder buffer;
            public IntPtr hWnd;
        }

        public static IntPtr Find(int processId, string className)
        {
            SearchData sd = new SearchData
            {
                className = className,
                processId = (uint)processId,
                buffer = new StringBuilder(className == null ? 0 : className.Length)
            };

            EnumWindows(new EnumWindowsProc(EnumProc), ref sd);

            return sd.hWnd;
        }

        public static string GetWindowTitle(IntPtr hwnd)
        {
            int length = GetWindowTextLength(hwnd);
            if (length == 0)
                return "";

            StringBuilder buffer = new StringBuilder(length);
            GetWindowText(hwnd, buffer, length + 1);

            return buffer.ToString();
        }

        public static bool EnumProc(IntPtr hWnd, ref SearchData data)
        {
            uint processId;
            GetWindowThreadProcessId(hWnd, out processId);

            if (processId == data.processId)
            {
                data.buffer.Length = 0;
                GetClassName(hWnd, data.buffer, data.buffer.Capacity + 1);

                if (data.className == null || data.buffer.ToString().Equals(data.className))
                {
                    data.hWnd = hWnd;
                    return false;
                }
            }

            return true;
        }
    }
}
