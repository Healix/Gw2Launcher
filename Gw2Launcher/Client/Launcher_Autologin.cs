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

            public enum EventAction
            {
                /// <summary>
                /// Automatically determines action based on settings and continues to next action
                /// </summary>
                Auto,
                /// <summary>
                /// Only enter the email/password
                /// </summary>
                Login,
                /// <summary>
                /// Only click play
                /// </summary>
                Play,
                /// <summary>
                /// Only press enter to login (remembered login)
                /// </summary>
                Autologin,
                /// <summary>
                /// Enters the totp code
                /// </summary>
                Totp,
            }

            private enum AccountState
            {
                WaitingOnLogin,
                WaitingOnPlay,
                WaitingOnAutologin,
                WaitingOnTotp,
            }

            private enum CoordinateType
            {
                Empty,
                Play
            }

            private enum LauncherType : byte
            {
                Unknown = 0,
                US = 1,
                CN = 2,
            }

            private enum ClipboardState
            {
                Unknown = 0,
                Success = 1,
                Failed = 2,
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

                public bool Automatic
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

            private class ClipboardText
            {
                private string text;

                public string Text
                {
                    get
                    {
                        return text;
                    }
                    set
                    {
                        if (value == null)
                            text = string.Empty;
                        else
                            text = value;
                    }
                }

                public bool HasText
                {
                    get
                    {
                        return text != null;
                    }
                }

                public void Clear()
                {
                    text = null;
                }
            }

            public event EventHandler<Account> LoginEntered;

            private uint coordClient, coordPlay;
            private int widthClient, heightClient;
            private LauncherType launcherType;
            private ClipboardState clipboardState;

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
                widthClient = 0;
                heightClient = 0;
            }

            private LauncherType GetLauncherType()
            {
                if (FileManager.IsGw2China)
                    return LauncherType.CN;
                else
                    return LauncherType.US;
            }

            private async Task<bool> DoTotp(QueuedAccount q)
            {
                try
                {
                    var key = q.Account.Settings.TotpKey;
                    if (key == null)
                        return false;

                    //note entering the code has no easy way to reset it (compared to the login being the first tab) - either tab through all the links on the launcher, or click the textbox

                    foreach (var c in Tools.Totp.Generate(key))
                    {
                        NativeMethods.PostMessage(q.Handle, (uint)WindowMessages.WM_CHAR, (IntPtr)c, IntPtr.Zero);
                    }

                    //wait for the submit button to enable
                    await Task.Delay(500);

                    if ((q.Account.Settings.NetworkAuthorization & Settings.NetworkAuthorizationOptions.Remember) != 0)
                    {
                        //skipping remember if keys are blocking the ability to tab
                        if (await WaitForKeys(System.Windows.Forms.Keys.Alt | System.Windows.Forms.Keys.Control, 1000))
                        {
                            //tab to the submit button - if code failed to enter / submit is disabled, this will tab to the cancel button
                            NativeMethods.PostMessage(q.Handle, WindowMessages.WM_KEYDOWN, (IntPtr)0x09, IntPtr.Zero); //VK_TAB
                            //tab to the check
                            NativeMethods.PostMessage(q.Handle, WindowMessages.WM_KEYDOWN, (IntPtr)0x09, IntPtr.Zero); //VK_TAB
                            //check requires a key down+up to trigger
                            NativeMethods.PostMessage(q.Handle, WindowMessages.WM_KEYDOWN, (IntPtr)0x20, IntPtr.Zero); //VK_SPACE
                            NativeMethods.PostMessage(q.Handle, WindowMessages.WM_KEYUP, (IntPtr)0x20, IntPtr.Zero); //VK_SPACE

                            await Task.Delay(500);
                        }
                    }

                    if (!IsHandleOkay(q))
                        return false;

                    //pressing enter on the checkbox will return to the login if the code wasn't entered properly
                    NativeMethods.PostMessage(q.Handle, WindowMessages.WM_KEYDOWN, (IntPtr)0x0D, IntPtr.Zero); //VK_RETURN

                    return true;
                }
                catch { }

                return false;
            }

            public void Queue(Account account, Process p, EventAction a = EventAction.Auto, bool forceRememberedAutologin = false)
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
                        var gw2 = (Settings.IGw2Account)account.Settings;
                        var q = new QueuedAccount(account, p)
                        {
                            Time = DateTime.UtcNow,
                            Automatic = a == EventAction.Auto,
                        };

                        if (a == EventAction.Auto)
                        {
                            if (gw2.AutomaticLogin && gw2.HasCredentials)
                            {
                                a = EventAction.Login;
                            }
                            else if (forceRememberedAutologin || gw2.AutomaticRememberedLogin && !Settings.DisableAutomaticLogins.Value)
                            {
                                a = EventAction.Autologin;
                            }
                            else if (gw2.AutomaticPlay && !Settings.DisableAutomaticLogins.Value)
                            {
                                a = EventAction.Play;
                            }
                        }

                        switch (a)
                        {
                            case EventAction.Login:

                                q.State = AccountState.WaitingOnLogin;

                                break;
                            case EventAction.Play:

                                q.State = AccountState.WaitingOnPlay;
                                q.Limit = q.Time.AddSeconds(5);

                                break;
                            case EventAction.Autologin:

                                q.State = AccountState.WaitingOnAutologin;

                                break;
                            case EventAction.Totp:

                                q.State = AccountState.WaitingOnTotp;

                                break;
                            default:

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
                                case AccountState.WaitingOnAutologin:

                                    if (Settings.DisableAutomaticLogins.Value)
                                    {
                                        p.Dispose();
                                        continue;
                                    }

                                    if (!Settings.Tweaks.Login.HasValue || s >= Settings.Tweaks.Login.Value.Delay)
                                    {
                                        DoAutoLogin(q);

                                        if (LoginEntered != null)
                                        {
                                            try
                                            {
                                                LoginEntered(this, q.Account);
                                            }
                                            catch { }
                                        }

                                        if (!q.Automatic || Settings.DisableAutomaticLogins.Value || !((Settings.IGw2Account)q.Account.Settings).AutomaticPlay)
                                        {
                                            p.Dispose();
                                            continue;
                                        }

                                        q.State = AccountState.WaitingOnPlay;
                                        q.Time = DateTime.UtcNow;
                                        q.Limit = q.Time.AddSeconds(60);
                                        q.Attempts = 0;
                                    }

                                    _queue.Enqueue(q);

                                    break;
                                case AccountState.WaitingOnLogin:

                                    if (s >= (q.Attempts > 0 ? 1 : Settings.Tweaks.Login.HasValue ? Settings.Tweaks.Login.Value.Delay : 0))
                                    {
                                        if (await DoLogin(q))
                                        {
                                            if (!Settings.DisableAutomaticLogins.Value && LoginEntered != null)
                                            {
                                                try
                                                {
                                                    LoginEntered(this, q.Account);
                                                }
                                                catch { }
                                            }

                                            if (!q.Automatic || Settings.DisableAutomaticLogins.Value || !((Settings.IGw2Account)q.Account.Settings).AutomaticPlay)
                                            {
                                                p.Dispose();
                                                continue;
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
                                case AccountState.WaitingOnTotp:

                                    await DoTotp(q);
                                    p.Dispose();

                                    break;
                                case AccountState.WaitingOnPlay:

                                    if (s >= 1 || q.Attempts == 0)
                                    {
                                        DoPlay(q);

                                        if (DateTime.UtcNow > q.Limit)
                                        {
                                            p.Dispose();
                                            continue;
                                        }

                                        q.Time = DateTime.UtcNow;
                                        ++q.Attempts;
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

            /// <summary>
            /// Checks if the currently focused window has higher privileges; can't control the keyboard if it does
            /// </summary>
            private bool IsPrivilegedProcessFocused()
            {
                var h = NativeMethods.GetForegroundWindow();

                if (h != IntPtr.Zero)
                {
                    return Windows.FindWindow.IsPrivilegedWindow(h);
                }

                return false;
            }

            /// <summary>
            /// Checks if the clipboard and the ability to press control is functional
            /// </summary>
            /// <returns></returns>
            private async Task<bool> CanUseClipboard()
            {
                if (!Settings.IsRunningWine)
                {
                    return true;
                }
                else if (clipboardState != ClipboardState.Unknown)
                {
                    return clipboardState == ClipboardState.Success;
                }

                var clipboard = await GetClipboardText();
                var t = Environment.TickCount.ToString();
                var retry = 5;

                do
                {
                    if (await SetClipboardText(t))
                    {
                        if (await GetClipboardText(1000) == t)
                        {
                            await SetClipboardText(clipboard);

                            if (!IsPrivilegedProcessFocused())
                            {
                                try
                                {
                                    NativeMethods.keybd_event(0x11, 0, 0, 0); //VK_CONTROL down

                                    if ((NativeMethods.GetAsyncKeyState(0x11) & 0x8000) == 0x8000)
                                    {
                                        await SetClipboardText(clipboard);

                                        clipboardState = ClipboardState.Success;
                                        return true;
                                    }

                                    break;
                                }
                                finally
                                {
                                    NativeMethods.keybd_event(0x11, 0, 2, 0); //VK_CONTROL up
                                }
                            }
                            else
                            {
                                await SetClipboardText(clipboard);

                                return false;
                            }
                        }
                    }
                }
                while (--retry > 0);

                await SetClipboardText(clipboard);

                clipboardState = ClipboardState.Failed;
                return false;
            }

            /// <summary>
            /// Holding shift/ctrl will interfere with pressing tab, alt will interfere with copy/paste
            /// </summary>
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

            private async Task<bool> WaitForFocusedWindow(int limit)
            {
                var start = Environment.TickCount;
                var h = IntPtr.Zero;

                while (true)
                {
                    var w = NativeMethods.GetForegroundWindow();
                    
                    if (w == IntPtr.Zero)
                    {
                        return true;
                    }
                    else if (w != h)
                    {
                        if (!Windows.FindWindow.IsPrivilegedWindow(w))
                            return true;
                        h = w;
                    }

                    if (Environment.TickCount - start > limit)
                        return false;
                    await Task.Delay(100);
                }
            }

            private Process[] GetChildProcesses(int pid, int count)
            {
                var i = 0;
                var result = new Process[count];

                var noChild = true;
                var processes = Process.GetProcesses();

                using (var pi = new Windows.ProcessInfo())
                {
                    foreach (var p in processes)
                    {
                        if (noChild)
                        {
                            try
                            {
                                if (pi.Open(p.Id))
                                {
                                    if (pi.GetParent() == pid)
                                    {
                                        result[i++] = p;
                                        noChild = i < count;

                                        continue;
                                    }
                                }
                            }
                            catch (Exception e)
                            {
                                Util.Logging.Log(e);
                            }
                        }
                        p.Dispose();
                    }
                }

                return result;
            }

            private List<Process> GetProcesses(Process parent)
            {
                var result = new List<Process>(4);

                result.Add(Process.GetProcessById(parent.Id));

                foreach (var p in GetChildProcesses(parent.Id, 1))
                {
                    if (p == null)
                        continue;

                    result.Add(p);

                    foreach (var child in GetChildProcesses(p.Id, 2))
                    {
                        if (child == null)
                            continue;

                        result.Add(child);
                    }
                }

                return result;
            }

            private bool DoAutoLogin(QueuedAccount q)
            {
                //enter to login
                NativeMethods.PostMessage(q.Handle, WindowMessages.WM_KEYDOWN, (IntPtr)0x0D, IntPtr.Zero); //VK_RETURN

                return true;
            }

            private bool GetCoordinates(IntPtr handle, CoordinateType type, out uint coords)
            {
                RECT r;
                if (NativeMethods.GetWindowRect(handle, out r))
                {
                    var h = r.bottom - r.top;
                    var w = r.right - r.left;

                    if (w == 0 || h == 0)
                    {
                        coords = 0;
                        return false;
                    }

                    if (w == widthClient && h == heightClient)
                    {
                        switch (type)
                        {
                            case CoordinateType.Empty:

                                if (this.coordClient != 0)
                                {
                                    coords = this.coordClient;
                                    return true;
                                }

                                break;
                            case CoordinateType.Play:

                                if (this.coordPlay != 0)
                                {
                                    coords = this.coordPlay;
                                    return true;
                                }

                                break;
                        }
                    }

                    var v = Settings.GuildWars2.LauncherAutologinPoints.Value;
                    uint coordClient = 0,
                         coordPlay = 0;

                    if (v != null && !v.EmptyArea.IsEmpty)
                    {
                        coordClient = (uint)v.EmptyArea.Y << 16 | v.EmptyArea.X;
                        if (!v.PlayButton.IsEmpty)
                            coordPlay = (uint)v.PlayButton.Y << 16 | v.PlayButton.X;
                    }

                    if (coordClient == 0)
                    {
                        var x = w * 4 / 5;
                        var y = h / 2;// -100;

                        if (GetLauncherType() == LauncherType.CN)
                        {
                            x = w * 9 / 10;
                            y = h * 5 / 6;
                        }

                        do
                        {
                            uint coord = (uint)(((y + r.top) << 16) | (x + r.left));
                            var result = NativeMethods.SendMessage(handle, 0x0084, 0, coord);
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
                                coords = 0;
                                return false;
                            }
                        }
                        while (true);
                    }

                    if (coordPlay == 0)
                    {
                        if (v != null && !v.PlayButton.IsEmpty)
                            coordPlay = (uint)v.EmptyArea.Y << 16 | v.EmptyArea.X;
                        else if (GetLauncherType() == LauncherType.CN)
                            coordPlay = (uint)(((uint)(h * 0.766f) << 16) | (uint)(w * 0.905f));
                        else
                            coordPlay = (uint)(((uint)(h * 0.725f) << 16) | (uint)(w * 0.738f));
                    }

                    this.coordClient = coordClient;
                    this.coordPlay = coordPlay;
                    this.widthClient = w;
                    this.heightClient = h;

                    switch (type)
                    {
                        case CoordinateType.Empty:

                            coords = this.coordClient;
                            return this.coordClient != 0;

                        case CoordinateType.Play:

                            coords = this.coordPlay;
                            return this.coordPlay != 0;
                    }
                }

                coords = 0;
                return false;
            }


            private async Task<bool> DoLogin(QueuedAccount q)
            {
                var handle = q.Handle;
                uint coord;

                Action wait = delegate
                {
                    NativeMethods.SendMessage(handle, 0, 0, 0);
                };

                //disabling to prevent interference from clicking (changes focus) - it'll still process keyboard input
                Windows.WindowLong.Add(handle, GWL.GWL_STYLE, WindowStyle.WS_DISABLED);

                if (GetLauncherType() == LauncherType.CN)
                {
                    if (!GetCoordinates(handle, CoordinateType.Play, out coord))
                        return false;

                    //"click" to show the login box
                    NativeMethods.SendMessage(handle, 0x0201, 1, coord); //WM_LBUTTONDOWN
                    NativeMethods.SendMessage(handle, 0x0202, 0, coord); //WM_LBUTTONUP

                    await Task.Delay(100);
                }

                var processes = GetProcesses(q.Process);

                //prioritizing child processes (CoherentUI) to reduce input delays
                foreach (var p in processes)
                {
                    try
                    {
                        p.PriorityClass = ProcessPriorityClass.High;
                    }
                    catch { }
                }

                try
                {
                    if (!GetCoordinates(handle, CoordinateType.Empty, out coord))
                        return false;

                    //"click" the background area to remove focus - this wouldn't be needed if the email was blank
                    NativeMethods.SendMessage(handle, 0x0201, 1, coord); //WM_LBUTTONDOWN
                    NativeMethods.SendMessage(handle, 0x0202, 0, coord); //WM_LBUTTONUP

                    wait();

                    if (!await WaitForKeys(System.Windows.Forms.Keys.Shift | System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.Alt, 5000))
                        return false;

                    //tab back to the email field, which will highlight any existing text
                    for (var i = (GetLauncherType() == LauncherType.CN ? 6 : 1); i > 0; --i)
                    {
                        NativeMethods.SendMessage(handle, WindowMessages.WM_KEYDOWN, (IntPtr)0x09, IntPtr.Zero); //VK_TAB
                    }
                    NativeMethods.SendMessage(handle, WindowMessages.WM_KEYUP, (IntPtr)0x09, IntPtr.Zero); //VK_TAB

                    var clipboard = new ClipboardText();
                    var canClipboard = await CanUseClipboard();
                    Settings.LoginInputType method;

                    if (Settings.Tweaks.Login.HasValue)
                        method = Settings.Tweaks.Login.Value.Email;
                    else
                    {
                        if (canClipboard && !IsPrivilegedProcessFocused())
                            method = Settings.LoginInputType.Clipboard;
                        else
                            method = Settings.LoginInputType.Post;
                    }

                    //enter email
                    if (!await DoTextEntry(method, handle, q.Account.Settings.Email, clipboard))
                    {
                        q.Process.Refresh();
                        return false;
                    }

                    wait();

                    if (!await WaitForKeys(System.Windows.Forms.Keys.Shift | System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.Alt, 5000))
                        return false;

                    //tab to the password field
                    NativeMethods.SendMessage(handle, WindowMessages.WM_KEYDOWN, (IntPtr)0x09, IntPtr.Zero); //VK_TAB
                    NativeMethods.SendMessage(handle, WindowMessages.WM_KEYUP, (IntPtr)0x09, IntPtr.Zero); //VK_TAB

                    //make sure the window hasn't changed
                    if (!IsHandleOkay(q))
                    {
                        if (clipboard.HasText)
                            await SetClipboardText(clipboard.Text);

                        return false;
                    }

                    //paste
                    var c = Security.Credentials.ToCharArray(q.Account.Settings.Password.ToSecureString());

                    try
                    {
                        if (!Settings.Tweaks.Login.HasValue && canClipboard)
                        {
                            //pasting an empty character to verify it's processing text input
                            if (!IsPrivilegedProcessFocused() && !await DoTextEntry(Settings.LoginInputType.Clipboard, handle, null, clipboard))
                            {
                                return false;
                            }
                        }

                        if (!await DoTextEntry(Settings.Tweaks.Login.HasValue ? Settings.Tweaks.Login.Value.Password : Settings.LoginInputType.Post, handle, c, clipboard))
                        {
                            return false;
                        }
                    }
                    finally
                    {
                        Array.Clear(c, 0, c.Length);
                        c = null;
                    }

                    if (canClipboard && !Settings.Tweaks.Login.HasValue && !IsPrivilegedProcessFocused() || Settings.Tweaks.Login.HasValue && Settings.Tweaks.Login.Value.Verify)
                    {
                        //verify
                        //"click" the background area to remove focus - this wouldn't be needed if the email was blank
                        NativeMethods.SendMessage(handle, 0x0201, 1, coord); //WM_LBUTTONDOWN
                        NativeMethods.SendMessage(handle, 0x0202, 0, coord); //WM_LBUTTONUP

                        wait();

                        if (!await WaitForKeys(System.Windows.Forms.Keys.Shift | System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.Alt, 5000))
                            return false;

                        //tab back to the email field, which will highlight any existing text
                        for (var i = (GetLauncherType() == LauncherType.CN ? 6 : 1); i > 0; --i)
                        {
                            NativeMethods.SendMessage(handle, WindowMessages.WM_KEYDOWN, (IntPtr)0x09, IntPtr.Zero); //VK_TAB
                        }
                        NativeMethods.SendMessage(handle, WindowMessages.WM_KEYUP, (IntPtr)0x09, IntPtr.Zero); //VK_TAB

                        wait();

                        if (!clipboard.HasText)
                            clipboard.Text = await GetClipboardText();

                        if (!await SetClipboardText(""))
                        {
                            if (clipboard.HasText)
                                await SetClipboardText(clipboard.Text);

                            return false;
                        }

                        //copy to clipboard and verify
                        try
                        {
                            NativeMethods.keybd_event(0x11, 0, 0, 0); //VK_CONTROL down
                            NativeMethods.SendMessage(handle, WindowMessages.WM_KEYDOWN, (IntPtr)67, IntPtr.Zero); //C
                            NativeMethods.SendMessage(handle, WindowMessages.WM_KEYUP, (IntPtr)67, IntPtr.Zero); //C
                        }
                        finally
                        {
                            NativeMethods.keybd_event(0x11, 0, 2, 0); //VK_CONTROL up
                        }

                        wait();

                        var clipboard2 = await GetClipboardText(5000);

                        if (!q.Account.Settings.Email.Equals(clipboard2, StringComparison.Ordinal))
                        {
                            if (clipboard.HasText)
                                await SetClipboardText(clipboard.Text);

                            return false;
                        }
                    }

                    if (clipboard.HasText)
                        await SetClipboardText(clipboard.Text);

                    if (!Settings.DisableAutomaticLogins.Value)
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
                    foreach (var p in processes)
                    {
                        try
                        {
                            using (p)
                            {
                                p.PriorityClass = ProcessPriorityClass.Normal;
                            }
                        }
                        catch { }
                    }

                    Windows.WindowLong.Remove(handle, GWL.GWL_STYLE, WindowStyle.WS_DISABLED);
                }
            }
            

            private async Task<string> GetClipboardText(int timeout = 0)
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

            private async Task<bool> DoTextEntry(Settings.LoginInputType type, IntPtr handle, IEnumerable<char> text, ClipboardText clipboard)
            {
                switch (type)
                {
                    case Settings.LoginInputType.Clipboard:
                        {
                            string t;

                            if (text is string)
                                t = (string)text;
                            else if (text is char[])
                                t = new string((char[])text);
                            else if (text == null)
                                t = null;
                            else
                                t = new string(text.ToArray());

                            if (!clipboard.HasText)
                                clipboard.Text = await GetClipboardText();

                            if (!await DoClipboard(handle, t, false))
                            {
                                if (await SetClipboardText(clipboard.Text))
                                {
                                    clipboard.Clear();
                                }
                                return false;
                            }

                            //resets key after key down event
                            NativeMethods.SendMessage(handle, WindowMessages.WM_CHAR, IntPtr.Zero, IntPtr.Zero);

                            return true;
                        }
                    default:

                        if (type == Settings.LoginInputType.Send)
                        {
                            foreach (char c in text)
                            {
                                NativeMethods.SendMessage(handle, WindowMessages.WM_CHAR, (IntPtr)c, IntPtr.Zero);
                            }

                            return true;
                        }
                        else
                        {
                            foreach (char c in text)
                            {
                                NativeMethods.PostMessage(handle, WindowMessages.WM_CHAR, (IntPtr)c, IntPtr.Zero);
                            }

                            await Task.Delay(500);

                            return true;
                        }
                }
            }

            private async Task<bool> DoClipboard(IntPtr handle, string text, bool reset, string resetText = null)
            {
                if (!await WaitForKeys(System.Windows.Forms.Keys.Alt, 5000))
                {
                    return false;
                }

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

                    var ctext = reset && resetText == null ? await GetClipboardText() : reset ? resetText : null;
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
                        NativeMethods.SendMessage(handle, WindowMessages.WM_KEYUP, (IntPtr)0x56, IntPtr.Zero); //V
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
                DoPlay(q.Handle);
            }

            public void DoPlay(IntPtr handle)
            {
                uint coord;
                if (GetCoordinates(handle, CoordinateType.Play, out coord))
                {
                    NativeMethods.PostMessage(handle, 0x201, 1, coord);
                    NativeMethods.PostMessage(handle, 0x202, 0, coord);
                }
            }
        }
    }
}
