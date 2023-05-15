using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using Gw2Launcher.Windows.Native;

namespace Gw2Launcher.Windows
{
    static class Clipboard
    {
        private const uint CF_UNICODE = 13;

        public class DataRequestedEventArgs : EventArgs
        {
            public enum ResponseAction
            {
                /// <summary>
                /// Sets the clipboard, allowing the requester to retrieve the data
                /// </summary>
                Accept,
                /// <summary>
                /// Clipboard is left unchanged and the operation is aborted
                /// </summary>
                Abort,
                /// <summary>
                /// Clipboard is left unchanged, allowing for another requester
                /// </summary>
                Skip,
            }

            /// <summary>
            /// True if clipboard data was already set and shouldn't be changed
            /// </summary>
            public bool Handled
            {
                get;
                set;
            }

            public ResponseAction Action
            {
                get;
                set;
            }

            /// <summary>
            /// Returns the current process requesting the data
            /// </summary>
            public uint GetProcess()
            {
                return GetClipboardProcess(false);
            }
        }

        public class DataText : IDisposable
        {
            public event EventHandler<DataRequestedEventArgs> DataRequested;
            public event EventHandler<bool> Complete;

            public DataText()
            {
            }

            public string Text
            {
                get;
                set;
            }

            public void OnDataRequested(DataRequestedEventArgs e)
            {
                if (DataRequested != null)
                {
                    try
                    {
                        DataRequested(this, e);
                    }
                    catch { }
                }

                if (!e.Handled)
                {
                    switch (e.Action)
                    {
                        case DataRequestedEventArgs.ResponseAction.Accept:

                            var t = GetData();

                            if (t == null)
                                t = string.Empty;

                            var ptr = Marshal.StringToHGlobalUni(pending.Text);

                            try
                            {
                                if (NativeMethods.SetClipboardData(CF_UNICODE, ptr) != IntPtr.Zero)
                                {
                                    //system owns it
                                    ptr = IntPtr.Zero;
                                }
                            }
                            finally
                            {
                                if (ptr != IntPtr.Zero)
                                    Marshal.FreeHGlobal(ptr);
                            }

                            OnComplete(true);

                            break;
                        case DataRequestedEventArgs.ResponseAction.Skip:

                            //do nothing

                            break;
                        case DataRequestedEventArgs.ResponseAction.Abort:
                        default:

                            OnComplete(false);
                            e.Handled = true;

                            break;
                    }
                }
                else
                {
                    OnComplete(e.Action == DataRequestedEventArgs.ResponseAction.Accept);
                    e.Handled = true;
                }
            }

            public void OnComplete(bool success)
            {
                if (Complete != null)
                {
                    try
                    {
                        Complete(this, success);
                    }
                    catch { }
                }
            }

            public virtual string GetData()
            {
                return this.Text;
            }

            public void Dispose()
            {
                Complete = null;
                DataRequested = null;
            }
        }

        public interface IListener : IDisposable
        {
            event EventHandler Changed;

            /// <summary>
            /// Returns true if listening for clipboard changes (listening may not be supported)
            /// </summary>
            bool Enabled
            {
                get;
            }

            /// <summary>
            /// Returns true if the clipboard has changed
            /// </summary>
            bool HasChanged
            {
                get;
            }

            /// <summary>
            /// Resets if the clipboard has changed
            /// </summary>
            void Reset();

            /// <summary>
            /// Waits until the clipboard changes
            /// </summary>
            /// <param name="timeout">Milliseconds to wait</param>
            /// <param name="reset">True to reset if the clipboard has already been changed</param>
            /// <returns>True if the clipboard was changed</returns>
            Task<bool> Wait(int timeout, bool reset = true);
        }

        private class ClipboardListener : IListener
        {
            public event EventHandler Changed;
            private bool disposed;
            private bool hasChanged;
            private bool enabled;

            public ClipboardListener(bool enabled)
            {
                this.enabled = enabled;
                Clipboard.ClipboardChanged += Clipboard_ClipboardChanged;
            }

            ~ClipboardListener()
            {
                Dispose();
            }

            public bool Enabled
            {
                get
                {
                    return enabled;
                }
            }

            void Clipboard_ClipboardChanged(object sender, EventArgs e)
            {
                hasChanged = true;
                if (Changed != null)
                    Changed(this, e);
            }

            public bool HasChanged
            {
                get
                {
                    return hasChanged;
                }
            }

            /// <summary>
            /// Waits until the clipboard changes
            /// </summary>
            /// <param name="timeout">Milliseconds to wait</param>
            /// <param name="reset">True to reset if the clipboard has already been changed</param>
            /// <returns>True if the clipboard was changed</returns>
            public async Task<bool> Wait(int timeout, bool reset = true)
            {
                if (hasChanged)
                {
                    if (reset)
                        hasChanged = false;
                    else
                        return true;
                }

                using (var c = new System.Threading.CancellationTokenSource())
                {
                    EventHandler onChange = delegate
                    {
                        c.Cancel();
                    };

                    Changed += onChange;

                    try
                    {
                        if (hasChanged)
                            return true;
                        await Task.Delay(timeout, c.Token);
                    }
                    catch 
                    { 
                    }
                    finally
                    {
                        Changed -= onChange;
                    }

                    return c.IsCancellationRequested;
                }
            }

            public void Reset()
            {
                hasChanged = false;
            }

            public void Dispose()
            {
                if (!disposed)
                {
                    GC.SuppressFinalize(this);

                    disposed = true;
                    Clipboard.ClipboardChanged -= Clipboard_ClipboardChanged;
                    Changed = null;

                    if (enabled)
                    {
                        enabled = false;

                        lock (UI.formMain.MainWindow)
                        {
                            if (--listeners == 0)
                            {
                                NativeMethods.RemoveClipboardFormatListener(UI.formMain.MainWindowHandle);
                            }
                        }
                    }
                }
            }
        }

        private static event EventHandler ClipboardChanged;
        private static DataText pending;
        private static byte listeners;

        /// <summary>
        /// Retrieves text from the clipboard
        /// </summary>
        /// <param name="timeout">Returns null after the specified number of milliseconds</param>
        /// <param name="minimumLength">Waits until the clipboard contains text with length >= minimum</param>
        public static async Task<string> GetClipboardText(int timeout = 0, int minimumLength = 1)
        {
            var start = Environment.TickCount;

            if (minimumLength > 0)
            {
                minimumLength = (minimumLength + 1) * 2; //unicode length
            }

            while (true)
            {
                try
                {
                    if (NativeMethods.OpenClipboard(IntPtr.Zero))
                    {
                        try
                        {
                            if (NativeMethods.IsClipboardFormatAvailable(CF_UNICODE))
                            {
                                var ptr = NativeMethods.GetClipboardData(CF_UNICODE);

                                if (ptr != IntPtr.Zero)
                                {
                                    var l = NativeMethods.GlobalLock(ptr);

                                    if (l != IntPtr.Zero)
                                    {
                                        try
                                        {
                                            var sz = NativeMethods.GlobalSize(l);

                                            if (sz >= minimumLength)
                                            {
                                                return Marshal.PtrToStringUni(l);
                                            }
                                        }
                                        finally
                                        {
                                            NativeMethods.GlobalUnlock(l);
                                        }
                                    }
                                }
                            }
                        }
                        finally
                        {
                            NativeMethods.CloseClipboard();
                        }
                    }
                }
                catch { }

                if (Environment.TickCount - start > timeout)
                    return null;

                await Task.Delay(100);
            }
        }

        public static string GetClipboardText(IntPtr hwnd)
        {
            try
            {
                if (NativeMethods.OpenClipboard(hwnd))
                {
                    try
                    {
                        if (NativeMethods.IsClipboardFormatAvailable(CF_UNICODE))
                        {
                            var ptr = NativeMethods.GetClipboardData(CF_UNICODE);

                            if (ptr != IntPtr.Zero)
                            {
                                var l = NativeMethods.GlobalLock(ptr);

                                if (l != IntPtr.Zero)
                                {
                                    try
                                    {
                                        return Marshal.PtrToStringUni(l);
                                    }
                                    finally
                                    {
                                        NativeMethods.GlobalUnlock(l);
                                    }
                                }
                            }
                        }
                    }
                    finally
                    {
                        NativeMethods.CloseClipboard();
                    }
                }
            }
            catch { }

            return null;
        }

        /// <summary>
        /// Empties the clipboard
        /// </summary>
        /// <param name="timeout">Returns false after the specified number of milliseconds</param>
        public static async Task<bool> EmptyClipboard(int timeout = 0)
        {
            var start = Environment.TickCount;

            while (true)
            {
                try
                {
                    if (NativeMethods.OpenClipboard(IntPtr.Zero))
                    {
                        try
                        {
                            if (NativeMethods.EmptyClipboard())
                            {
                                return true;
                            }
                        }
                        finally
                        {
                            NativeMethods.CloseClipboard();
                        }
                    }
                }
                catch { }

                if (Environment.TickCount - start > timeout)
                    return false;

                await Task.Delay(100);
            }
        }

        /// <summary>
        /// Sets data on the clipboard for delayed rendering
        /// </summary>
        /// <param name="timeout">Timeout in milliseconds</param>
        /// <returns>True if successful</returns>
        public static async Task<bool> SetClipboardData(DataText data, int timeout = 1000)
        {
            var start = Environment.TickCount;

            do
            {
                try
                {
                    if (NativeMethods.OpenClipboard(UI.formMain.MainWindowHandle))
                    {
                        try
                        {
                            //using the clipboard to copy from CEF will cause EmptyClipboard() to block for 5s
                            if (NativeMethods.EmptyClipboard())
                            {
                                if (pending != null && !object.ReferenceEquals(pending, data))
                                    pending.OnComplete(false);
                                pending = data;

                                if (NativeMethods.SetClipboardData(CF_UNICODE, IntPtr.Zero) != IntPtr.Zero)
                                {
                                    pending = null;

                                    return false;
                                }

                                return true;
                            }
                        }
                        finally
                        {
                            NativeMethods.CloseClipboard();
                        }
                    }
                }
                catch { }

                if (Environment.TickCount - start > timeout)
                    return false;

                await Task.Delay(100);
            }
            while (true);
        }

        public static async void SetText(string text)
        {
            await SetClipboardText(text);
        }

        /// <summary>
        /// Sets text on the clipboard
        /// </summary>
        /// <param name="timeout">Timeout in milliseconds</param>
        /// <returns>True if successful</returns>
        public static async Task<bool> SetClipboardText(string text, int timeout = 1000)
        {
            var start = Environment.TickCount;

            if (text == null)
            {
                text = string.Empty;
            }

            do
            {
                try
                {
                    if (NativeMethods.OpenClipboard(IntPtr.Zero))
                    {
                        try
                        {
                            if (string.IsNullOrEmpty(text))
                            {
                                if (!NativeMethods.IsClipboardFormatAvailable(CF_UNICODE))
                                {
                                    return true;
                                }
                            }

                            var ptr = Marshal.StringToHGlobalUni(text);

                            try
                            {
                                if (NativeMethods.SetClipboardData(CF_UNICODE, ptr) != IntPtr.Zero)
                                {
                                    //system owns it
                                    ptr = IntPtr.Zero;
                                    return true;
                                }
                            }
                            finally
                            {
                                if (ptr != IntPtr.Zero)
                                    Marshal.FreeHGlobal(ptr);
                            }
                        }
                        finally
                        {
                            NativeMethods.CloseClipboard();
                        }
                    }
                }
                catch { }

                if (Environment.TickCount - start > timeout)
                    return false;

                await Task.Delay(100);
            }
            while (true);
        }

        public static bool ProcessMessage(ref System.Windows.Forms.Message m)
        {
            switch ((WindowMessages)m.Msg)
            {
                case WindowMessages.WM_RENDERFORMAT:
                    
                    var data = pending;

                    if (data == null)
                    {
                        return false;
                    }

                    var e = new DataRequestedEventArgs();

                    data.OnDataRequested(e);

                    if (e.Handled)
                    {
                        pending = null;
                    }

                    return true;
                case WindowMessages.WM_RENDERALLFORMATS:

                    //occurs when the clipboard owner is exiting
                    //not needed

                    break;
                case WindowMessages.WM_CLIPBOARDUPDATE:

                    if (ClipboardChanged != null)
                    {
                        try
                        {
                            ClipboardChanged(null, EventArgs.Empty);
                        }
                        catch { }
                    }

                    break;
            }

            return false;
        }

        //CEF will block the clipboard for 5s after copying something to the clipboard
        //any attempt to empty the clipboard during this time will take an additional 5s
        //for example, if EmptyClipboard() is called 4.9s after CEF set the clipboard, it'll take 5s to complete, but if it's called 5s after CEF set the clipboard, it'll take 0s
        //this will also cause other program that try to set the clipboard to freeze for 5s, for example, copying from the trading post, then copying to notepad will freeze notepad for 5s
        private static int blocked;
        private static IntPtr owner;

        public static async Task WaitForBlocked()
        {
            if (blocked != 0)
            {
                var t = blocked - Environment.TickCount + 5000;

                if (t > 0 && t <= 5000)
                {
                    var _owner = NativeMethods.GetClipboardOwner();

                    if (_owner == owner)
                    {
                        await Task.Delay(t);
                    }
                }
            }
        }

        public static void SetBlocked()
        {
            var _owner = NativeMethods.GetClipboardOwner();
            if (_owner != IntPtr.Zero)
                owner = _owner;

            //could check if the current owner is tied to an account to confirm
            //if (_owner != IntPtr.Zero)
            //{
            //    uint pid;
            //    NativeMethods.GetWindowThreadProcessId(_owner, out pid);
            //    if (pid != 0)
            //    {
            //        var a = Client.Launcher.GetAccountFromProcessId((int)pid);
            //        if (a != null)
            //        {
            //            //...
            //        }
            //    }
            //}

            blocked = Environment.TickCount;
        }

        public static bool IsBlocked
        {
            get
            {
                if (blocked != 0)
                {
                    var t = Environment.TickCount - blocked;

                    return t > 0 && t <= 5000;
                }

                return false;
            }
        }

        public static IListener AddListener()
        {
            lock (UI.formMain.MainWindow)
            {
                if (listeners == 0)
                {
                    if (!NativeMethods.AddClipboardFormatListener(UI.formMain.MainWindowHandle))
                        return new ClipboardListener(false);
                }
                ++listeners;
                return new ClipboardListener(true);
            }
        }

        /// <summary>
        /// Returns the ID of the process using the clipboard
        /// </summary>
        /// <param name="owner">True: returns the process owning the clipboard, False: returns the process with the clipboard open</param>
        public static uint GetClipboardProcess(bool owner)
        {
            var hwnd = owner ? NativeMethods.GetClipboardOwner() : NativeMethods.GetOpenClipboardWindow();
            uint pid;

            if (hwnd != IntPtr.Zero)
            {
                NativeMethods.GetWindowThreadProcessId(hwnd, out pid);
            }
            else
            {
                pid = 0;
            }

            return pid;
        }
    }
}
