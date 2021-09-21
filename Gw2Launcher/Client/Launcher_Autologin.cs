using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.Diagnostics;
using System.Drawing;
using System.IO;
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
                    var DataRequested = this.DataRequested;
                    if (DataRequested != null)
                    {
                        DataRequested.BeginInvoke(this, EventArgs.Empty,
                           delegate(IAsyncResult r)
                           {
                               try
                               {
                                   DataRequested.EndInvoke(r);
                               }
                               catch { }
                           }, null);
                    }

                    return base.GetData(format);
                }
            }

            public event EventHandler<Account> LoginEntered;

            private uint coordClient, coordPlay;
            private byte launcherType;

            private SynchronizationContext context;
            private int contextId;

            private Queue<QueuedAccount> queue;
            private Task task;

            public Autologin()
            {
                context = SynchronizationContext.Current;
                contextId = Thread.CurrentThread.ManagedThreadId;

                Settings.GuildWars2.LauncherAutologinPoints.ValueChanged += LauncherAutologinPoints_ValueChanged;
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
                        var gw2 = (Settings.IGw2Account)account.Settings;

                        if (gw2.AutomaticLogin && gw2.HasCredentials)
                        {
                            q.State = AccountState.WaitingOnLogin;
                        }
                        else if (gw2.AutomaticPlay && !Settings.DisableAutomaticLogins)
                        {
                            q.State = AccountState.WaitingOnPlay;
                            q.Time = DateTime.UtcNow;
                            q.Limit = q.Time.AddSeconds(60);
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
                            q.Handle = Windows.FindWindow.FindMainWindow(p);
                            if (q.Handle == IntPtr.Zero)
                                return;
                        }

                        var buffer = new StringBuilder(10);
                        NativeMethods.GetClassName(q.Handle, buffer, buffer.Capacity + 1);
                        if (!buffer.ToString().Equals(LAUNCHER_WINDOW_CLASSNAME))
                            return;

                        if (queue == null)
                            queue = new Queue<QueuedAccount>();
                        queue.Enqueue(q);
                        queued = true;
                    }
                    finally
                    {
                        if (!queued)
                            p.Dispose();
                    }

                    if (queued && (task == null || task.IsCompleted))
                    {
                        task = DoQueue();
                    }
                }
            }

            private async Task DoQueue()
            {
                //note logins could be prioritized to allow for more accurate entry, however, this causes other problems (delayed launches, failure to login due to too many connection)

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
                            queue = null;
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
                            var h = Windows.FindWindow.FindMainWindow(p);
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
                                            if (Settings.DisableAutomaticLogins || !((Settings.IGw2Account)q.Account.Settings).AutomaticPlay)
                                            {
                                                p.Dispose();
                                                continue;
                                            }

                                            if (LoginEntered != null)
                                            {
                                                try
                                                {
                                                    LoginEntered(this, q.Account);
                                                }
                                                catch { }
                                            }

                                            q.State = AccountState.WaitingOnPlay;
                                            q.Time = DateTime.UtcNow;
                                            q.Limit = q.Time.AddSeconds(60);
                                            q.Attempts = 0;
                                        }
                                        else
                                        {
                                            if (++q.Attempts > 10)
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

                return q.Handle == Windows.FindWindow.FindMainWindow(p);
            }

            private async Task<bool> WaitForKeys(System.Windows.Forms.Keys keys, int limit)
            {
                var start = Environment.TickCount;

                while ((System.Windows.Forms.Form.ModifierKeys & keys) != 0)
                {
                    if (Environment.TickCount - start > limit)
                        return false;
                    await Task.Delay(100);
                }

                return true;
            }

            private async Task<bool> DoLogin(QueuedAccount q)
            {
                #region Launcher type

                if (launcherType == 0)
                {
                    try
                    {
                        if (Directory.Exists(Path.Combine(Path.GetDirectoryName(Settings.GuildWars2.Path.Value), "Gw2.dat")))
                            launcherType = 2;
                        else
                            launcherType = 1;
                    }
                    catch { }
                }

                #endregion

                #region Find client area

                if (coordClient == 0)
                {
                    var v = Settings.GuildWars2.LauncherAutologinPoints.Value;

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

                            if (launcherType == 2)
                            {
                                x = w * 9 / 10;
                                y = h * 5 / 6;
                            }

                            if (coordPlay == 0)
                            {
                                if (v != null && !v.PlayButton.IsEmpty)
                                    coordPlay = (uint)v.EmptyArea.Y << 16 | v.EmptyArea.X;
                                else if (launcherType == 2)
                                    coordPlay = (uint)(((uint)(h * 0.766f) << 16) | (uint)(w * 0.905f));
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

                //disabling to prevent interference from clicking (changes focus) - it'll still process keyboard input
                Windows.WindowLong.Add(handle, GWL.GWL_STYLE, WindowStyle.WS_DISABLED);

                if (launcherType == 2)
                {
                    //"click" to show the login box
                    NativeMethods.SendMessage(handle, 0x0201, 1, coordPlay); //WM_LBUTTONDOWN
                    NativeMethods.SendMessage(handle, 0x0202, 0, coordPlay); //WM_LBUTTONUP

                    await Task.Delay(100);
                }

                try
                {
                    Action wait = delegate
                    {
                        NativeMethods.SendMessage(handle, 0, 0, 0);
                    };

                    //"click" the background area to remove focus - this wouldn't be needed if the email was blank
                    NativeMethods.SendMessage(handle, 0x0201, 1, coordClient); //WM_LBUTTONDOWN
                    NativeMethods.SendMessage(handle, 0x0202, 0, coordClient); //WM_LBUTTONUP

                    wait();

                    if (!await WaitForKeys(System.Windows.Forms.Keys.Shift | System.Windows.Forms.Keys.Control, 5000))
                        return false;

                    //tab back to the email field, which will highlight any existing text
                    for (var i = (launcherType == 2 ? 6 : 1); i > 0; --i)
                    {
                        NativeMethods.SendMessage(handle, WindowMessages.WM_KEYDOWN, (IntPtr)0x09, IntPtr.Zero); //VK_TAB
                    }

                    wait();

                    var clipboard = await GetClipboardText();

                    //paste
                    if (!await DoClipboard(handle, q.Account.Settings.Email, false, q))
                    {
                        await SetClipboardText(clipboard);

                        q.Process.Refresh();
                        
                        return false;
                    }

                    wait();

                    if (!await WaitForKeys(System.Windows.Forms.Keys.Shift | System.Windows.Forms.Keys.Control, 5000))
                        return false;

                    //tab to the password field
                    NativeMethods.SendMessage(handle, WindowMessages.WM_KEYDOWN, (IntPtr)0x09, IntPtr.Zero); //VK_TAB

                    //make sure the window hasn't changed
                    if (!IsHandleOkay(q))
                    {
                        await SetClipboardText(clipboard);

                        return false;
                    }

                    //paste
                    var c = Security.Credentials.ToCharArray(q.Account.Settings.Password.ToSecureString());

                    try
                    {
                        #region Password via posting

                        //pasting an empty character to verying it's processing text input
                        if (!await DoClipboard(handle, null, false, q))
                        {
                            await SetClipboardText(clipboard);

                            return false;
                        }

                        //first posted character will be ignored due to the prior keydown message without a following keyup
                        NativeMethods.PostMessage(handle, WindowMessages.WM_CHAR, IntPtr.Zero, IntPtr.Zero);

                        foreach (var ch in c)
                        {
                            NativeMethods.PostMessage(handle, WindowMessages.WM_CHAR, (IntPtr)ch, IntPtr.Zero);
                        }

                        await Task.Delay(500);

                        #endregion

                        #region Password via clipboard

                        //using the clipboard is more reliable, but makes the password visible

                        //var p = new string(c);
                        //if (!await DoClipboard(handle, p, true, q))
                        //{
                        //    return false;
                        //}

                        #endregion
                    }
                    finally
                    {
                        Array.Clear(c, 0, c.Length);
                    }

                    await SetClipboardText(clipboard);
                    
                    if (!Settings.DisableAutomaticLogins)
                    {
                        wait();

                        //using the home key to serve as an input update
                        //NativeMethods.SendMessage(handle, WindowMessages.WM_KEYDOWN, (IntPtr)0x24, IntPtr.Zero); //VK_HOME

                        if (!await WaitForKeys(System.Windows.Forms.Keys.Alt | System.Windows.Forms.Keys.Control, 5000))
                            return false;

                        //make sure the window hasn't changed, specifically here as it could auto submit a crash report
                        if (!IsHandleOkay(q))
                            return false;

                        //enter to login
                        NativeMethods.PostMessage(handle, WindowMessages.WM_KEYDOWN, (IntPtr)0x0D, IntPtr.Zero); //VK_RETURN
                    }

                    return true;
                }
                finally
                {
                    Windows.WindowLong.Remove(handle, GWL.GWL_STYLE, WindowStyle.WS_DISABLED);
                }
            }
            

            private async Task<string> GetClipboardText(int timeout = 1000)
            {
                var contains = false;
                string text = null;
                var start = Environment.TickCount;

                while (true)
                {
                    try
                    {
                        context.Send(
                            delegate
                            {
                                contains = System.Windows.Forms.Clipboard.ContainsText();
                                if (contains)
                                    text = System.Windows.Forms.Clipboard.GetText();
                            }, null);
                    }
                    catch
                    {
                        contains = true;
                        text = null;
                    }

                    if (contains)
                    {
                        if (!string.IsNullOrEmpty(text))
                            return text;
                    }
                    else
                    {
                        return null;
                    }

                    if (Environment.TickCount - start > timeout)
                        return null;

                    await Task.Delay(100);
                }
            }

            private async Task<bool> SetClipboardData(DataObject data, int timeout = 1000)
            {
                var start = Environment.TickCount;

                do
                {
                    try
                    {
                        context.Send(
                            delegate
                            {
                                System.Windows.Forms.Clipboard.SetDataObject(data);
                            }, null);

                        return true;
                    }
                    catch { }

                    if (Environment.TickCount - start > timeout)
                        return false;

                    await Task.Delay(100);
                }
                while (true);
            }

            private async Task<bool> SetClipboardText(string text, int timeout = 1000)
            {
                var start = Environment.TickCount;

                do
                {
                    try
                    {
                        context.Send(
                            delegate
                            {
                                if (string.IsNullOrEmpty(text))
                                    System.Windows.Forms.Clipboard.Clear();
                                else
                                    System.Windows.Forms.Clipboard.SetText(text);
                            }, null);

                        return true;
                    }
                    catch { }

                    if (Environment.TickCount - start > timeout)
                        return false;

                    await Task.Delay(100);
                }
                while (true);
            }

            private async Task<bool> DoClipboard(IntPtr handle, string text, bool reset, QueuedAccount q)
            {
                using (var cancel = new CancellationTokenSource())
                {
                    var data = new DataObject();

                    if (text != null)
                    {
                        data.SetText(text);
                    }
                    else
                    {
                        data.SetData(System.Windows.Forms.DataFormats.Text, "");
                    }

                    var ctext = reset ? await GetClipboardText() : null;
                    if (!await SetClipboardData(data))
                    {
                        return false;
                    }

                    var completed = false;
                    var isWaiting = true;

                    EventHandler onRequested = delegate
                    {
                        if (isWaiting)
                        {
                            cancel.Cancel(); //could fail, but will be caught by the event
                        }
                    };

                    data.DataRequested += onRequested;

                    try
                    {
                        NativeMethods.keybd_event(0x11, 0, 0, 0); //VK_CONTROL down
                        NativeMethods.SendMessage(handle, WindowMessages.WM_KEYDOWN, (IntPtr)0x56, IntPtr.Zero); //V
                    }
                    finally
                    {
                        NativeMethods.keybd_event(0x11, 0, 2, 0); //VK_CONTROL up
                    }

                    try
                    {
                        //gw2 could potentially take more than 5s to use the clipboard
                        await Task.Delay(5000, cancel.Token);
                    }
                    catch
                    {
                        completed = true;
                    }

                    isWaiting = false;
                    data.DataRequested -= onRequested;

                    if (completed)
                    {
                        await Task.Delay(100);
                    }

                    if (reset)
                    {
                        if (!await SetClipboardText(ctext))
                        {
                            //failed to restore clipboard
                        }
                    }

                    return completed;
                }
            }

            private void DoPlay(QueuedAccount q)
            {
                #region Find client area

                if (coordPlay == 0)
                {
                    var v = Settings.GuildWars2.LauncherAutologinPoints.Value;

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
