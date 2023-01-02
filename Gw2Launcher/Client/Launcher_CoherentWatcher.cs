using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using Gw2Launcher.Windows.Native;
using System.Runtime.InteropServices;
using System.IO;

namespace Gw2Launcher.Client
{
    static partial class Launcher
    {
        class CoherentWatcher : IDisposable
        {
            public class FileMap : IDisposable
            {
                private IntPtr file, view;
                private Process process;

                public FileMap(Process process, IntPtr file, IntPtr view)
                {
                    this.process = process;
                    this.file = file;
                    this.view = view;
                }

                public FileMap(Process process, string name, int length = 256)
                {
                    this.process = process;

                    Open(name, length);
                }

                ~FileMap()
                {
                    Dispose();
                }

                public Process Process
                {
                    get
                    {
                        return process;
                    }
                }

                public bool Open(string name, int length = 256)
                {
                    Close();

                    file = NativeMethods.OpenFileMapping(FileMapAccess.FileMapRead, false, name);
                    if (file == IntPtr.Zero)
                        return false;

                    return Open(length);
                }

                public bool Open(int length = 256)
                {
                    if (file == IntPtr.Zero)
                    {
                        return false;
                    }

                    if (view != IntPtr.Zero)
                    {
                        NativeMethods.UnmapViewOfFile(view);
                    }

                    view = NativeMethods.MapViewOfFile(file, FileMapAccess.FileMapRead, 0, 0, length);

                    return view != IntPtr.Zero;
                }

                public bool IsOpen
                {
                    get
                    {
                        return view != IntPtr.Zero;
                    }
                }

                public IntPtr View
                {
                    get
                    {
                        return view;
                    }
                }

                public void Close()
                {
                    if (view != IntPtr.Zero)
                    {
                        NativeMethods.UnmapViewOfFile(view);
                        view = IntPtr.Zero;
                    }
                    if (file != IntPtr.Zero)
                    {
                        NativeMethods.CloseHandle(file);
                        file = IntPtr.Zero;
                    }
                }

                public void Dispose()
                {
                    Close();
                    using (process) { }
                }
            }

            public enum EventType
            {
                None,
                /// <summary>
                /// Unable to read memory
                /// </summary>
                Error,
                /// <summary>
                /// CoherentUI has exited
                /// </summary>
                Exited,
                /// <summary>
                /// Login is ready to be entered
                /// </summary>
                LoginReady,
                /// <summary>
                /// Login was successful
                /// </summary>
                LoginComplete,
                /// <summary>
                /// Login failed (excludes connection errors)
                /// </summary>
                LoginError,
                /// <summary>
                /// An authentication code has been requested (email, sms or totp)
                /// </summary>
                LoginCode,
            }

            private FileMap file;
            private int position;
            private int length;
            private byte[] search;

            public CoherentWatcher(FileMap file)
            {
                this.file = file;
                this.search = Encoding.Unicode.GetBytes("coui://");
                this.length = Marshal.ReadInt32(file.View);
            }

            public static FileMap Find(UIntPtr[] pids)
            {
                var search = Encoding.ASCII.GetBytes("PID:");
                var counter = 0;
                FileMap m = null;

                Windows.Win32Handles.GetHandle(pids, Gw2Launcher.Windows.Win32Handles.HandleType.Section, new Func<Windows.Win32Handles.IObject, Windows.Win32Handles.CallbackResponse>(
                    delegate(Windows.Win32Handles.IObject o)
                    {
                        ++counter;

                        var n = Path.GetFileName(o.Name);

                        if (n.LastIndexOf('-') != -1)
                        {
                            IntPtr file = NativeMethods.OpenFileMapping(FileMapAccess.FileMapRead, false, n), 
                                   view = IntPtr.Zero;

                            if (file != IntPtr.Zero)
                            {
                                try
                                {
                                    view = NativeMethods.MapViewOfFile(file, FileMapAccess.FileMapRead, 0, 0, 256);

                                    if (view != IntPtr.Zero)
                                    {
                                        var i = IndexOf(view, search, 0, 256);

                                        if (i != -1)
                                        {
                                            lock (search)
                                            {
                                                if (m == null)
                                                {
                                                    var p = Process.GetProcessById((int)o.PID.GetValue());

                                                    m = new FileMap(p, file, view);
                                                    m.Open(0);

                                                    file = IntPtr.Zero;
                                                    view = IntPtr.Zero;
                                                }
                                            }

                                            return Windows.Win32Handles.CallbackResponse.Abort;
                                        }
                                    }
                                }
                                catch (Exception e)
                                {
                                    Util.Logging.Log(e);
                                }
                                finally
                                {
                                    if (view != IntPtr.Zero)
                                        NativeMethods.UnmapViewOfFile(view);
                                    if (file != IntPtr.Zero)
                                        NativeMethods.CloseHandle(file);
                                }
                            }
                        }
                        return Windows.Win32Handles.CallbackResponse.Continue;
                    }), false);

                if (counter == 0)
                {
                    throw new NotSupportedException();
                }

                return m;
            }

            private static int IndexOf(IntPtr ptr, byte[] value, int startIndex, int count)
            {
                var limit = count - startIndex - value.Length;
                var zero = 0;

                while (startIndex <= limit)
                {
                    var b = Marshal.ReadByte(ptr, startIndex);

                    if (b == value[0])
                    {
                        int i;

                        for (i = 1; i < value.Length; i++)
                        {
                            if (Marshal.ReadByte(ptr, startIndex + i) != value[i])
                            {
                                break;
                            }
                        }

                        if (i == value.Length)
                        {
                            return startIndex;
                        }
                    }

                    if (b == 0)
                    {
                        //most of the file will be empty
                        if (++zero == 512)
                            return -1;
                    }
                    else
                        zero = 0;

                    ++startIndex;
                }

                return -1;
            }

            private string GetString(int i)
            {
                var l = Marshal.ReadInt32(file.View, i - 8);

                if (l > 0)
                {
                    return Marshal.PtrToStringUni((IntPtr)(file.View.GetValue() + i), l);
                }
                else
                {
                    return null;
                }
            }

            private bool WaitFor(string page)
            {
                while (true)
                {
                    var i = IndexOf(file.View, search, position, length - position);

                    if (i != -1)
                    {
                        var s = GetString(i);

                        position = i + s.Length * 2;

                        if (s.EndsWith(page, StringComparison.Ordinal))
                        {
                            return true;
                        }
                    }
                    else if (file.Process.WaitForExit(500))
                    {
                        return false;
                    }
                }
            }

            public EventType GetNextEvent()
            {
                //search through a list of files that have been requested - looking for the last request
                try
                {
                    var e = EventType.None;
                    var i = IndexOf(file.View, search, position, length - position);

                    if (i != -1)
                    {
                        var s = GetString(i);
                        var j = s.LastIndexOf('/');

                        if (j != -1)
                        {
                            var n = s.Substring(j + 1);

                            switch (n)
                            {
                                case "login":
                                    e = EventType.LoginReady;
                                    break;
                                case "info":
                                    e = EventType.LoginComplete;
                                    break;
                                case "code":
                                    e = EventType.LoginCode;
                                    break;
                                default:
                                    //only includes errors returned by the server (doesn't include connection errors)
                                    if (n.StartsWith("login?error", StringComparison.Ordinal))
                                    {
                                        e = EventType.LoginError;
                                    }
                                    break;
                            }
                        }

                        position = i + s.Length * 2;
                    }
                    else if (e != EventType.None)
                    {
                        return e;
                    }
                    else if (file.Process.WaitForExit(500))
                    {
                        return EventType.Exited;
                    }

                    return e;
                }
                catch
                {
                    return EventType.Error;
                }
            }

            public EventType WaitForEvent()
            {
                try
                {
                    var e = EventType.None;

                    while (true)
                    {
                        var i = IndexOf(file.View, search, position, length - position);

                        if (i != -1)
                        {
                            var s = GetString(i);
                            var j = s.LastIndexOf('/');

                            if (j != -1)
                            {
                                var n = s.Substring(j + 1);

                                switch (n)
                                {
                                    case "login":
                                        e = EventType.LoginReady;
                                        break;
                                    case "info":
                                        e = EventType.LoginComplete;
                                        break;
                                    case "code":
                                        e = EventType.LoginCode;
                                        break;
                                    default:
                                        //only includes errors returned by the server (doesn't include connection errors)
                                        if (n.StartsWith("login?error", StringComparison.Ordinal))
                                        {
                                            e = EventType.LoginError;
                                        }
                                        break;
                                }
                            }

                            position = i + s.Length * 2;
                        }
                        else if (e != EventType.None)
                        {
                            return e;
                        }
                        else if (file.Process.WaitForExit(500))
                        {
                            return EventType.Exited;
                        }
                    }
                }
                catch
                {
                    return EventType.Error;
                }
            }

            public void Dispose()
            {
                using (file) { }
            }
        }
    }
}
