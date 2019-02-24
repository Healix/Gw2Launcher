using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.Diagnostics;
using System.Drawing;
using Gw2Launcher.Windows.Native;

namespace Gw2Launcher.Client
{
    static partial class Launcher
    {
        private class Autologin
        {
            private const string LAUNCHER_WINDOW_CLASSNAME = "ArenaNet";

            private enum AccountState
            {
                WaitingOnLogin,
                WaitingOnPlay,
            }

            private class QueuedAccount
            {
                public QueuedAccount(Account account, Process p)
                {
                    this.Account = account;
                    this.Process = p;
                }

                public Account Account
                {
                    get;
                    private set;
                }

                public Process Process
                {
                    get;
                    set;
                }

                public IntPtr Handle
                {
                    get;
                    set;
                }

                public AccountState State
                {
                    get;
                    set;
                }

                public DateTime Time
                {
                    get;
                    set;
                }

                public DateTime Limit
                {
                    get;
                    set;
                }

                public byte Attempts
                {
                    get;
                    set;
                }
            }

            private class DataObject : System.Windows.Forms.DataObject
            {
                public event EventHandler DataRequested;

                public override object GetData(string format)
                {
                    if (DataRequested != null)
                        DataRequested(this, EventArgs.Empty);
                    return base.GetData(format);
                }
            }
            
            private uint coordClient, coordPlay;

            private SynchronizationContext context;
            private int contextId;

            private Queue<QueuedAccount> queue;
            private Task task;

            public Autologin()
            {
                context = SynchronizationContext.Current;
                contextId = Thread.CurrentThread.ManagedThreadId;
                queue = new Queue<QueuedAccount>();

                Settings.LauncherAutologinPoints.ValueChanged += LauncherAutologinPoints_ValueChanged;
            }

            void LauncherAutologinPoints_ValueChanged(object sender, EventArgs e)
            {
                coordClient = 0;
                coordPlay = 0;
            }

            public void Queue(Account account, Process p)
            {
                lock (this)
                {
                    try
                    {
                        p = Process.GetProcessById(p.Id);
                    }
                    catch
                    {
                        return;
                    }

                    var queued = false;

                    try
                    {
                        var q = new QueuedAccount(account, p);
                        var s = account.Settings;

                        if (s.AutomaticLogin && s.HasCredentials)
                        {
                            q.State = AccountState.WaitingOnLogin;
                        }
                        else if (s.AutomaticPlay && !Settings.DisableAutomaticLogins)
                        {
                            q.State = AccountState.WaitingOnPlay;
                            q.Time = DateTime.UtcNow;
                            q.Limit = q.Time.AddSeconds(30);
                        }
                        else
                        {
                            return;
                        }

                        if (p.HasExited)
                        {
                            return;
                        }
                        else
                        {
                            q.Handle = p.MainWindowHandle;
                            if (q.Handle == IntPtr.Zero)
                                return;
                        }

                        var buffer = new StringBuilder(10);
                        NativeMethods.GetClassName(q.Handle, buffer, buffer.Capacity + 1);
                        if (!buffer.ToString().Equals(LAUNCHER_WINDOW_CLASSNAME))
                            return;

                        queue.Enqueue(q);
                        queued = true;
                    }
                    finally
                    {
                        if (!queued)
                            p.Dispose();
                    }

                    if (task == null || task.IsCompleted)
                        task = Task.Factory.StartNew(DoQueue);
                }
            }

            private async void DoQueue()
            {
                var _queue = new Queue<QueuedAccount>();

                while (true)
                {
                    lock (this)
                    {
                        while (queue.Count > 0)
                        {
                            _queue.Enqueue(queue.Dequeue());
                        }

                        if (_queue.Count == 0)
                        {
                            task = null;
                            return;
                        }
                    }

                    for (var i = _queue.Count - 1; i >= 0; i--)
                    {
                        var q = _queue.Dequeue();
                        var p = q.Process;
                        p.Refresh();

                        try
                        {
                            var h = p.MainWindowHandle;
                            if (p.HasExited || q.Handle != h)
                            {
                                p.Dispose();
                                continue;
                            }

                            var s = DateTime.UtcNow.Subtract(q.Time).TotalSeconds;

                            switch (q.State)
                            {
                                case AccountState.WaitingOnLogin:

                                    if (s >= 1)
                                    {
                                        if (await DoLogin(q))
                                        {
                                            if (Settings.DisableAutomaticLogins || !q.Account.Settings.AutomaticPlay)
                                            {
                                                p.Dispose();
                                                continue;
                                            }

                                            q.State = AccountState.WaitingOnPlay;
                                            q.Time = DateTime.UtcNow;
                                            q.Limit = q.Time.AddSeconds(30);
                                            q.Attempts = 0;
                                        }
                                        else
                                        {
                                            if (++q.Attempts > 3)
                                            {
                                                p.Dispose();
                                                continue;
                                            }
                                            else
                                            {
                                                q.Time = DateTime.UtcNow;
                                            }
                                        }
                                    }

                                    _queue.Enqueue(q);

                                    break;
                                case AccountState.WaitingOnPlay:

                                    if (s >= 1)
                                    {
                                        DoPlay(q);

                                        if (DateTime.UtcNow > q.Limit)
                                        {
                                            p.Dispose();
                                            continue;
                                        }

                                        q.Time = DateTime.UtcNow;
                                    }

                                    _queue.Enqueue(q);

                                    break;
                                default:

                                    p.Dispose();

                                    break;
                            }
                        }
                        catch
                        {
                            p.Dispose();
                            continue;
                        }
                    }

                    await Task.Delay(500);
                }
            }

            private bool IsHandleOkay(QueuedAccount q)
            {
                var p = q.Process;
                p.Refresh();

                return q.Handle == p.MainWindowHandle;
            }

            private async Task<bool> DoLogin(QueuedAccount q)
            {
                //note: this makes a lot of assumptions and doesn't check anything. a simple test would
                //to be watch for the login and play buttons to change color, confirming that 1) the credentials were entered
                //and 2) the login was successful

                #region Find client area

                if (coordClient == 0)
                {
                    var v = Settings.LauncherAutologinPoints.Value;

                    if (v != null && !v.EmptyArea.IsEmpty)
                    {
                        coordClient = (uint)v.EmptyArea.Y << 16 | v.EmptyArea.X;
                        if (!v.PlayButton.IsEmpty)
                            coordPlay = (uint)v.PlayButton.Y << 16 | v.PlayButton.X;
                    }
                    else
                    {
                        RECT r;
                        if (NativeMethods.GetWindowRect(q.Handle, out r))
                        {
                            var h = r.bottom - r.top;
                            var w = r.right - r.left;
                            var x = w * 4 / 5;
                            var y = h / 2;// -100;

                            if (coordPlay == 0)
                            {
                                if (v != null && !v.PlayButton.IsEmpty)
                                    coordPlay = (uint)v.EmptyArea.Y << 16 | v.EmptyArea.X;
                                else
                                    coordPlay = (uint)(((uint)(h * 0.725f) << 16) | (uint)(w * 0.738f));
                            }

                            do
                            {
                                uint coord = (uint)(((y + r.top) << 16) | (x + r.left));
                                var result = NativeMethods.SendMessage(q.Handle, 0x0084, 0, coord);
                                if (result == (IntPtr)1)
                                {
                                    coordClient = (uint)((y << 16) | x);
                                    break;
                                }
                                else if (x > 50)
                                {
                                    x -= 50;
                                }
                                else
                                {
                                    return false;
                                }
                            }
                            while (true);
                        }
                    }
                }

                #endregion

                var handle = q.Handle;

                //"click" the background area to remove focus - this wouldn't be needed if the email was blank
                NativeMethods.SendMessage(handle, 0x0201, 1, coordClient); //WM_LBUTTONDOWN
                NativeMethods.SendMessage(handle, 0x0202, 0, coordClient); //WM_LBUTTONUP

                //tab back to the email field, which will highlight any existing text
                NativeMethods.SendMessage(handle, 0x0100, 0x09, 0); //WM_KEYDOWN (VK_TAB)

                //paste
                try
                {
                    NativeMethods.keybd_event(0x11, 0, 0, 0); //VK_CONTROL down

                    if (!await DoClipboard(handle, q.Account.Settings.Email, false))
                        return false;
                }
                finally
                {
                    NativeMethods.keybd_event(0x11, 0, 2, 0); //VK_CONTROL up
                }

                //tab to the password field
                NativeMethods.SendMessage(handle, 0x0100, 0x09, 0); //WM_KEYDOWN (VK_TAB)

                //make sure the window hasn't changed
                if (!IsHandleOkay(q))
                    return false;

                //paste
                try
                {
                    NativeMethods.keybd_event(0x11, 0, 0, 0); //VK_CONTROL down
                    var c = Security.Credentials.ToCharArray(q.Account.Settings.Password);
                    var p = new string(c);
                    Array.Clear(c, 0, c.Length);
                    if (!await DoClipboard(handle, p, true))
                        return false;
                }
                finally
                {
                    NativeMethods.keybd_event(0x11, 0, 2, 0); //VK_CONTROL up
                }

                if (!Settings.DisableAutomaticLogins)
                {
                    //using the home key to serve as an update
                    NativeMethods.SendMessage(handle, 0x0100, 0x24, 0); //WM_KEYDOWN (VK_HOME)

                    //make sure the window hasn't changed, specifically here as it could auto submit a crash report
                    if (!IsHandleOkay(q))
                        return false;

                    //enter to login
                    NativeMethods.PostMessage(handle, 0x0100, 0x0D, 0); //WM_KEYDOWN (VK_RETURN)
                }

                return true;
            }

            private async Task<bool> DoClipboard(IntPtr handle, string text, bool clear)
            {
                using (var cancel = new CancellationTokenSource())
                {
                    var data = new DataObject();

                    data.SetText(text);

                    data.DataRequested += delegate
                    {
                        if (cancel != null)
                            cancel.Cancel();
                    };

                    try
                    {
                        context.Send(
                            delegate
                            {
                                System.Windows.Forms.Clipboard.SetDataObject(data);
                            }, null);
                    }
                    catch
                    {
                        return false;
                    }

                    NativeMethods.SendMessage(handle, 0x0100, 0x56, 0); //WM_KEYDOWN (V)

                    var completed = false;

                    try
                    {
                        await Task.Delay(1000, cancel.Token);
                    }
                    catch
                    {
                        completed = true;
                    }

                    if (completed)
                    {
                        await Task.Delay(1);
                    }

                    if (clear)
                    {
                        try
                        {
                            context.Send(
                                delegate
                                {
                                    System.Windows.Forms.Clipboard.Clear();
                                }, null);
                        }
                        catch { }
                    }

                    return completed;
                }
            }

            private void DoPlay(QueuedAccount q)
            {
                #region Find client area

                if (coordPlay == 0)
                {
                    var v = Settings.LauncherAutologinPoints.Value;

                    if (v != null && !v.PlayButton.IsEmpty)
                    {
                        coordPlay = (uint)v.PlayButton.Y << 16 | v.PlayButton.X;
                    }
                    else
                    {
                        RECT r;
                        if (NativeMethods.GetWindowRect(q.Handle, out r))
                        {
                            var h = r.bottom - r.top;
                            var w = r.right - r.left;

                            coordPlay = (uint)(((uint)(h * 0.725f) << 16) | (uint)(w * 0.738f));
                        }
                    }
                }

                #endregion

                NativeMethods.PostMessage(q.Handle, 0x201, 1, coordPlay);
                NativeMethods.PostMessage(q.Handle, 0x202, 0, coordPlay);
            }
        }
    }
}
