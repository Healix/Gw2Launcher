using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices;
using Gw2Launcher.Windows.Native;

namespace Gw2Launcher.Windows
{
    static class FindWindow
    {
        [DllImport(NativeMethods.DLL.USER32)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool EnumWindows(EnumWindowsProc lpEnumFunc, ref SearchData data);

        [DllImport(NativeMethods.DLL.USER32)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool EnumChildWindows(IntPtr hwndParent, EnumWindowsProc lpEnumFunc, ref SearchData data);

        public delegate bool EnumWindowsProc(IntPtr hWnd, ref SearchData data);
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

        public static IntPtr Find(int processId, string className)
        {
            SearchData sd = new SearchData
            {
                limit = 1,
                processId = (uint)processId,
                buffer = new StringBuilder(className == null ? 250 : className.Length + 1),
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

            EnumWindows(new EnumWindowsProc(EnumWindow), ref sd);

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

            EnumChildWindows(parent, new EnumWindowsProc(EnumWindow), ref sd);

            return sd.results;
        }

        private static bool EnumWindow(IntPtr hWnd, ref SearchData data)
        {
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

        public static string GetWindowTitle(IntPtr hwnd)
        {
            int length = NativeMethods.GetWindowTextLength(hwnd);
            if (length == 0)
                return "";

            StringBuilder buffer = new StringBuilder(length);
            NativeMethods.GetWindowText(hwnd, buffer, length + 1);

            return buffer.ToString();
        }

        //public static bool EnumProc(IntPtr hWnd, ref SearchData data)
        //{
        //    uint processId;
        //    GetWindowThreadProcessId(hWnd, out processId);

        //    if (processId == data.processId)
        //    {
        //        data.buffer.Length = 0;
        //        GetClassName(hWnd, data.buffer, data.buffer.Capacity + 1);

        //        if (data.className == null || data.className.Length == data.buffer.Length && data.className.Equals(data.buffer.ToString()))
        //        {
        //            data.hWnd = hWnd;
        //            return false;
        //        }
        //    }

        //    return true;
        //}

        public static void FocusWindow(IntPtr handle)
        {
            var p = WindowSize.GetWindowPlacement(handle);
            if (p.showCmd == ShowWindowCommands.ShowMinimized)
                NativeMethods.ShowWindow(handle, ShowWindowCommands.Restore);
            if (!NativeMethods.SetForegroundWindow(handle))
                NativeMethods.BringWindowToTop(handle);
        }
    }
}
