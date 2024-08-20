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
using System.Runtime.InteropServices;

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

            private class ModifierMonitor : IDisposable
            {
                private bool disposed;

                public ModifierMonitor()
                {
                    DoMonitor();
                }

                private async void DoMonitor()
                {
                    var keys = System.Windows.Forms.Keys.Shift | System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.Alt;

                    while (!disposed)
                    {
                        await Task.Delay(50);

                        if ((System.Windows.Forms.Form.ModifierKeys & keys) != 0)
                        {
                            Pressed = true;
                            break;
                        }
                    }
                }

                public bool Pressed
                {
                    get;
                    private set;
                }

                public void Dispose()
                {
                    disposed = true;
                }
            }

            public event EventHandler<Account> LoginEntered;

            private uint coordClient, coordPlay;
            private int widthClient, heightClient;
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

                    if (q.Account.hostType == WindowWatcher.HostType.CEF)
                    {
                        if (!await WaitForKeys(System.Windows.Forms.Keys.Shift | System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.Alt, 5000))
                            return false;

                        var clipboard = new ClipboardText();

                        if (!await DoTextEntry(q, Settings.LoginInputType.Clipboard, q.Handle, Tools.Totp.Generate(key), clipboard))
                        {
                            return false;
                        }

                        if (clipboard.HasText)
                            await Windows.Clipboard.SetClipboardText(clipboard.Text);
                    }
                    else
                    {
                        foreach (var c in Tools.Totp.Generate(key))
                        {
                            NativeMethods.PostMessage(q.Handle, (uint)WindowMessages.WM_CHAR, (IntPtr)c, IntPtr.Zero);
                        }
                    }

                    //wait for the submit button to enable
                    await Task.Delay(500);

                    if ((q.Account.Settings.NetworkAuthorization & Settings.NetworkAuthorizationOptions.Remember) != 0)
                    {
                        //skipping remember if keys are blocking the ability to tab
                        if (await WaitForKeys(System.Windows.Forms.Keys.Alt | System.Windows.Forms.Keys.Control, 1000))
                        {
                            //tab to the submit button - if code failed to enter / submit is disabled, this will tab to the cancel button
                            Windows.Keyboard.SendKey(q.Handle, System.Windows.Forms.Keys.Tab, Windows.Keyboard.KeyMessage.Down, true);
                            //tab to the check
                            Windows.Keyboard.SendKey(q.Handle, System.Windows.Forms.Keys.Tab, Windows.Keyboard.KeyMessage.Down, true);
                            //check requires a key down+up to trigger
                            Windows.Keyboard.SendKey(q.Handle, System.Windows.Forms.Keys.F1, Windows.Keyboard.KeyMessage.Press, true);

                            await Task.Delay(500);
                        }

                    }

                    if (!IsHandleOkay(q))
                        return false;

                    if (q.Account.hostType == WindowWatcher.HostType.CEF)
                    {
                        //pressing enter to submit is bugged for CEF
                        uint coord;
                        if (!GetCoordinates(q.Handle, 0.205357, 0.503074, out coord))
                            return false;

                        NativeMethods.SendMessage(q.Handle, 0x0201, 1, coord); //WM_LBUTTONDOWN
                        NativeMethods.SendMessage(q.Handle, 0x0202, 0, coord); //WM_LBUTTONUP
                    }
                    else
                    {
                        //pressing enter on the checkbox will return to the login if the code wasn't entered properly
                        Windows.Keyboard.SendKey(q.Handle, System.Windows.Forms.Keys.Return, Windows.Keyboard.KeyMessage.Down, true);
                    }

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

                                    if (q.Account.hostType == WindowWatcher.HostType.CEF)
                                    {
                                        if (!Settings.DisableAutomaticLogins.Value && ((Settings.IGw2Account)q.Account.Settings).AutomaticPlay)
                                        {
                                            DoPlay(q);

                                            await Task.Delay(500);

                                            if (!IsHandleOkay(q))
                                            {
                                                p.Dispose();
                                                continue;
                                            }
                                        }
                                    }

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

                var clipboard = await Windows.Clipboard.GetClipboardText();
                var t = Environment.TickCount.ToString();
                var retry = 5;

                do
                {
                    if (await Windows.Clipboard.SetClipboardText(t))
                    {
                        if (await Windows.Clipboard.GetClipboardText(1000) == t)
                        {
                            await Windows.Clipboard.SetClipboardText(clipboard);

                            if (!IsPrivilegedProcessFocused())
                            {
                                try
                                {
                                    Windows.Keyboard.SetKeyState(System.Windows.Forms.Keys.ControlKey, true);

                                    if (Windows.Keyboard.GetKeyState(System.Windows.Forms.Keys.ControlKey))
                                    {
                                        await Windows.Clipboard.SetClipboardText(clipboard);

                                        clipboardState = ClipboardState.Success;
                                        return true;
                                    }

                                    break;
                                }
                                finally
                                {
                                    Windows.Keyboard.SetKeyState(System.Windows.Forms.Keys.ControlKey, false);
                                }
                            }
                            else
                            {
                                await Windows.Clipboard.SetClipboardText(clipboard);

                                return false;
                            }
                        }
                    }
                }
                while (--retry > 0);

                await Windows.Clipboard.SetClipboardText(clipboard);

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

            private int GetAllChildProcesses(int pid, List<Process> output)
            {
                var pids = new HashSet<uint>();
                var processes = Process.GetProcesses();
                var length = processes.Length;
                var ppids = new uint[length];
                var first = true;

                pids.Add((uint)pid);

                using (var pi = new Windows.ProcessInfo())
                {
                    do
                    {
                        var l = length;
                        length = 0;

                        for (var i = 0; i < l; i++)
                        {
                            try
                            {
                                if (first)
                                {
                                    if (pi.Open(processes[i].Id))
                                    {
                                        ppids[i] = pi.GetParent();
                                    }
                                    else
                                    {
                                        continue;
                                    }
                                }

                                if (pids.Contains(ppids[i]))
                                {
                                    if (pids.Add((uint)processes[i].Id))
                                    {
                                        output.Add(processes[i]);

                                        if (i > length)
                                            length = i;
                                    }
                                }
                            }
                            catch (Exception e)
                            {
                                Util.Logging.Log(e);
                            }
                        }

                        first = false;
                    }
                    while (length > 0);
                }

                return pids.Count - 1;
            }

            private List<Process> GetProcesses(Process parent)
            {
                var result = new List<Process>(6);

                result.Add(Process.GetProcessById(parent.Id));

                GetAllChildProcesses(parent.Id, result);

                return result;

            }

            private bool DoAutoLogin(QueuedAccount q)
            {
                //enter to login
                Windows.Keyboard.SendKey(q.Handle, System.Windows.Forms.Keys.Return, Windows.Keyboard.KeyMessage.Down, true);

                return true;
            }

            private bool GetCoordinates(IntPtr handle, double x, double y, out uint coords)
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

                    coords = (uint)(((uint)(h * y) << 16) | (uint)(w * x));
                    return true;
                }

                coords = 0;
                return false;
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

                    if (Util.Logging.Enabled)
                    {
                        Util.Logging.LogEvent(q.Account.Settings, "Beginning login entry for " + GetLauncherType() + " type at " + coord + " (" + widthClient + " x " + heightClient + ")");
                    }

                    int tabs; //number of tabs to reach the email

                    if (GetLauncherType() == LauncherType.CN)
                        tabs = 6;
                    else if (q.Account.hostType == WindowWatcher.HostType.CEF)
                        tabs = 14;
                    else
                        tabs = 1;

                    if (!await WaitForKeys(System.Windows.Forms.Keys.Shift | System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.Alt, 5000))
                        return false;

                    while (true)
                    {
                        using (var mm = new ModifierMonitor())
                        {
                            //"click" the background area to remove focus
                            NativeMethods.SendMessage(handle, 0x0201, 1, coord); //WM_LBUTTONDOWN
                            NativeMethods.SendMessage(handle, 0x0202, 0, coord); //WM_LBUTTONUP

                            wait();

                            //tab back to the email field, which will highlight any existing text
                            Windows.Keyboard.SendKey(handle, System.Windows.Forms.Keys.Tab, Windows.Keyboard.KeyMessage.Down, false, tabs);
                            Windows.Keyboard.SendKey(handle, System.Windows.Forms.Keys.Tab, Windows.Keyboard.KeyMessage.Up, false);

                            if (mm.Pressed)
                            {
                                if (!IsHandleOkay(q))
                                {
                                    return false;
                                }

                                if (Util.Logging.Enabled)
                                {
                                    Util.Logging.LogEvent(q.Account.Settings, "Interrupted by modifier key, retrying");
                                }
                            }
                            else
                                break;
                        }
                    }

                    var clipboard = new ClipboardText();
                    var canClipboard = await CanUseClipboard();
                    Settings.LoginInputType method;

                    if (Util.Logging.Enabled && !canClipboard)
                    {
                        Util.Logging.LogEvent(q.Account.Settings, "Clipboard not available");
                    }

                    if (Settings.Tweaks.Login.HasValue)
                        method = Settings.Tweaks.Login.Value.Email;
                    else
                    {
                        if (canClipboard && !IsPrivilegedProcessFocused())
                            method = Settings.LoginInputType.Clipboard;
                        else
                            method = Settings.LoginInputType.Post;
                    }

                    if (Util.Logging.Enabled)
                    {
                        Util.Logging.LogEvent(q.Account.Settings, "Entering email using " + method);
                    }

                    //enter email
                    if (!await DoTextEntry(q, method, handle, q.Account.Settings.Email, clipboard))
                    {
                        if (Util.Logging.Enabled)
                        {
                            Util.Logging.LogEvent(q.Account.Settings, "Failed to enter email");
                        }
                        q.Process.Refresh();
                        return false;
                    }

                    wait();

                    if (!await WaitForKeys(System.Windows.Forms.Keys.Shift | System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.Alt, 5000))
                        return false;

                    //tab to the password field
                    Windows.Keyboard.SendKey(handle, System.Windows.Forms.Keys.Tab, Windows.Keyboard.KeyMessage.Press, false);

                    //make sure the window hasn't changed
                    if (!IsHandleOkay(q))
                    {
                        if (clipboard.HasText)
                            await Windows.Clipboard.SetClipboardText(clipboard.Text);

                        return false;
                    }

                    //paste
                    var c = Security.Credentials.ToCharArray(q.Account.Settings.Password.ToSecureString());

                    try
                    {
                        if (Util.Logging.Enabled)
                        {
                            Util.Logging.LogEvent(q.Account.Settings, "Entering password using " + (Settings.Tweaks.Login.HasValue ? Settings.Tweaks.Login.Value.Password : Settings.LoginInputType.Post));
                        }

                        if (!Settings.Tweaks.Login.HasValue && canClipboard)
                        {
                            //pasting an empty character to verify it's processing text input
                            if (!IsPrivilegedProcessFocused() && !await DoTextEntry(q, Settings.LoginInputType.Clipboard, handle, null, clipboard))
                            {
                                if (Util.Logging.Enabled)
                                {
                                    Util.Logging.LogEvent(q.Account.Settings, "Failed to verify password entry");
                                }
                                return false;
                            }
                        }

                        if (!await WaitForKeys(System.Windows.Forms.Keys.Shift | System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.Alt, 5000))
                            return false;

                        if (!await DoTextEntry(q, Settings.Tweaks.Login.HasValue ? Settings.Tweaks.Login.Value.Password : Settings.LoginInputType.Post, handle, c, clipboard))
                        {
                            if (Util.Logging.Enabled)
                            {
                                Util.Logging.LogEvent(q.Account.Settings, "Failed to enter password");
                            }

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
                        if (Util.Logging.Enabled)
                        {
                            Util.Logging.LogEvent(q.Account.Settings, "Verifying login");
                        }

                        //verify
                        //"click" the background area to remove focus
                        NativeMethods.SendMessage(handle, 0x0201, 1, coord); //WM_LBUTTONDOWN
                        NativeMethods.SendMessage(handle, 0x0202, 0, coord); //WM_LBUTTONUP

                        wait();

                        if (!await WaitForKeys(System.Windows.Forms.Keys.Shift | System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.Alt, 5000))
                            return false;

                        //tab back to the email field, which will highlight any existing text
                        Windows.Keyboard.SendKey(handle, System.Windows.Forms.Keys.Tab, Windows.Keyboard.KeyMessage.Down, false, tabs);
                        Windows.Keyboard.SendKey(handle, System.Windows.Forms.Keys.Tab, Windows.Keyboard.KeyMessage.Up, false);

                        wait();

                        if (!clipboard.HasText)
                            clipboard.Text = await Windows.Clipboard.GetClipboardText();

                        if (!await Windows.Clipboard.SetClipboardText(""))
                        {
                            if (clipboard.HasText)
                                await Windows.Clipboard.SetClipboardText(clipboard.Text);

                            return false;
                        }

                        var verify = 0;

                        using (var listener = Windows.Clipboard.AddListener())
                        {
                            var pid = q.Process.Id;
                            var lpid = uint.MaxValue;

                            listener.Changed += delegate
                            {
                                lpid = Windows.Clipboard.GetClipboardProcess(true);
                            };

                            while (true)
                            {
                                if (++verify > 1)
                                {
                                    if (!IsHandleOkay(q))
                                    {
                                        if (clipboard.HasText)
                                            await Windows.Clipboard.SetClipboardText(clipboard.Text);

                                        return false;
                                    }

                                    if (Util.Logging.Enabled)
                                    {
                                        Util.Logging.LogEvent(q.Account.Settings, "Retrying login verification");
                                    }
                                }

                                //copy to clipboard and verify
                                try
                                {
                                    Windows.Keyboard.SetKeyState(System.Windows.Forms.Keys.ControlKey, true);
                                    if (verify > 1)
                                    {
                                        Windows.Keyboard.SendKey(handle, System.Windows.Forms.Keys.A, Windows.Keyboard.KeyMessage.Down, false);
                                    }
                                    listener.Reset();
                                    Windows.Keyboard.SendKey(handle, System.Windows.Forms.Keys.C, Windows.Keyboard.KeyMessage.Press, false);
                                }
                                finally
                                {
                                    Windows.Keyboard.SetKeyState(System.Windows.Forms.Keys.ControlKey, false);
                                }

                                wait();

                                if (q.Account.hostType == WindowWatcher.HostType.CEF)
                                {
                                    Windows.Clipboard.SetBlocked();
                                }

                                string clipboard2;

                                if (listener.Enabled)
                                {
                                    if (await listener.Wait(verify * 1500, false))
                                    {
                                        clipboard2 = await Windows.Clipboard.GetClipboardText(500);
                                    }
                                    else
                                    {
                                        clipboard2 = null;
                                    }
                                }
                                else
                                {
                                    clipboard2 = await Windows.Clipboard.GetClipboardText(verify * 1500);
                                }

                                if (clipboard2 == null)
                                {
                                    if (verify >= 2)
                                    {
                                        if (Util.Logging.Enabled)
                                        {
                                            Util.Logging.LogEvent(q.Account.Settings, "Unable to verify email, no data was available");
                                        }

                                        if (clipboard.HasText)
                                            await Windows.Clipboard.SetClipboardText(clipboard.Text);

                                        return false;
                                    }

                                    continue;
                                }

                                if (!q.Account.Settings.Email.Replace(" ", "").Equals(clipboard2, StringComparison.OrdinalIgnoreCase))
                                {
                                    if (Util.Logging.Enabled)
                                    {
                                        if (lpid == q.Process.Id)
                                            Util.Logging.LogEvent(q.Account.Settings, "Email does not match, clipboard was set to \"" + clipboard2 + "\"");
                                        else
                                            Util.Logging.LogEvent(q.Account.Settings, "Email does not match, clipboard was changed by PID " + (lpid == uint.MaxValue ? "[unknown]" : lpid.ToString()));
                                    }

                                    if (lpid != uint.MaxValue && lpid != pid && verify == 1)
                                    {
                                        //something else set the clipboard, try again
                                        await Windows.Clipboard.SetClipboardText("");

                                        continue;
                                    }

                                    if (clipboard.HasText)
                                        await Windows.Clipboard.SetClipboardText(clipboard.Text);

                                    return false;
                                }
                                else
                                {
                                    break;
                                }
                            }
                        }

                        if (q.Account.hostType == WindowWatcher.HostType.CEF)
                        {
                            Windows.Clipboard.SetBlocked();
                        }
                    }

                    if (clipboard.HasText)
                        await Windows.Clipboard.SetClipboardText(clipboard.Text, 1000);

                    if (Util.Logging.Enabled)
                    {
                        Util.Logging.LogEvent(q.Account.Settings, "Login entry complete");
                    }

                    if (!Settings.DisableAutomaticLogins.Value)
                    {
                        wait();

                        if (!await WaitForKeys(System.Windows.Forms.Keys.Alt | System.Windows.Forms.Keys.Control, 5000))
                            return false;

                        //make sure the window hasn't changed, specifically here as it could auto submit a crash report
                        if (!IsHandleOkay(q))
                            return false;

                        //enter to login
                        Windows.Keyboard.SendKey(handle, System.Windows.Forms.Keys.Return, Windows.Keyboard.KeyMessage.Down, true);
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
            

            private async Task<bool> DoTextEntry(QueuedAccount q, Settings.LoginInputType type, IntPtr handle, IEnumerable<char> text, ClipboardText clipboard)
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
                                clipboard.Text = await Windows.Clipboard.GetClipboardText();

                            if (!await DoClipboard(q, handle, t, false))
                            {
                                if (await Windows.Clipboard.SetClipboardText(clipboard.Text))
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

                        using (var mm = new ModifierMonitor())
                        {
                            if (type == Settings.LoginInputType.Send)
                            {
                                foreach (char c in text)
                                {
                                    NativeMethods.SendMessage(handle, WindowMessages.WM_CHAR, (IntPtr)c, IntPtr.Zero);
                                }
                            }
                            else
                            {
                                foreach (char c in text)
                                {
                                    NativeMethods.PostMessage(handle, WindowMessages.WM_CHAR, (IntPtr)c, IntPtr.Zero);
                                }

                                await Task.Delay(500);
                            }

                            if (mm.Pressed && Util.Logging.Enabled)
                            {
                                Util.Logging.LogEvent(q.Account.Settings, "Interrupted by modifier key");
                            }

                            return !mm.Pressed;
                        }
                }
            }

            private async Task<bool> DoClipboard(QueuedAccount q, IntPtr handle, string text, bool reset, string resetText = null)
            {
                if (!await WaitForKeys(System.Windows.Forms.Keys.Alt, 5000))
                {
                    return false;
                }

                var ctext = reset && resetText == null ? await Windows.Clipboard.GetClipboardText() : reset ? resetText : null;
                var attempt = 0;
                var pid = q.Process.Id;
                var self = Process.GetCurrentProcess().Id;

                using (var listener = Windows.Clipboard.AddListener())
                {
                    for (; ; ++attempt)
                    {
                        var cancel = new CancellationTokenSource();
                        var data = new Windows.Clipboard.DataText()
                        {
                            Text = text,
                        };

                        try
                        {
                            var completed = false;
                            var isWaiting = true;

                            data.DataRequested += delegate(object o, Windows.Clipboard.DataRequestedEventArgs e)
                            {
                                var rpid = e.GetProcess();

                                //CoherentUI uses 0
                                //CEF uses GW2

                                if (rpid != 0 && rpid != pid)
                                {
                                    if (Util.Logging.Enabled)
                                    {
                                        Util.Logging.LogEvent(q.Account.Settings, "Clipboard intercepted by PID " + rpid);
                                    }

                                    e.Action = Windows.Clipboard.DataRequestedEventArgs.ResponseAction.Skip;
                                }
                            };

                            data.Complete += delegate(object o, bool success)
                            {
                                completed = success;

                                if (isWaiting)
                                {
                                    cancel.Cancel(); //could fail, but will be caught by the event
                                }
                            };

                            if (Windows.Clipboard.IsBlocked)
                            {
                                if (Util.Logging.Enabled)
                                {
                                    Util.Logging.LogEvent(q.Account.Settings, "Waiting for clipboard to unblock");
                                }
                                await Windows.Clipboard.WaitForBlocked();
                            }

                            listener.Reset();

                            if (!await Windows.Clipboard.SetClipboardData(data))
                            {
                                return false;
                            }

                            //there may be a slight delay until the clipboard is set

                            if (listener.Enabled && !await listener.Wait(5000, false))
                            {
                                //clipboard wasn't set
                                //using the clipboard is probably not support

                                if (Util.Logging.Enabled)
                                {
                                    Util.Logging.LogEvent(q.Account.Settings, "Warning: use of the clipboard may not be supported");
                                }
                            }

                            EventHandler onChanged = delegate
                            {
                                if (isWaiting)
                                {
                                    var lpid = Windows.Clipboard.GetClipboardProcess(true);

                                    if (lpid != self)
                                    {
                                        if (Util.Logging.Enabled)
                                        {
                                            Util.Logging.LogEvent(q.Account.Settings, "Clipboard was changed by PID " + lpid.ToString());
                                        }
                                        cancel.Cancel();
                                    }
                                }
                            };

                            listener.Reset();
                            listener.Changed += onChanged;

                            try
                            {
                                Windows.Keyboard.SetKeyState(System.Windows.Forms.Keys.ControlKey, true);
                                if (attempt > 0)
                                {
                                    Windows.Keyboard.SendKey(handle, System.Windows.Forms.Keys.A, Windows.Keyboard.KeyMessage.Down, false);
                                }
                                Windows.Keyboard.SendKey(handle, System.Windows.Forms.Keys.V, Windows.Keyboard.KeyMessage.Press, false);
                            }
                            finally
                            {
                                Windows.Keyboard.SetKeyState(System.Windows.Forms.Keys.ControlKey, false);
                            }

                            var t = Environment.TickCount;
                            var abort = false;

                            do
                            {
                                try
                                {
                                    //gw2 could potentially take more than 5s to use the clipboard
                                    await Task.Delay(500, cancel.Token);

                                    if (!IsHandleOkay(q))
                                    {
                                        abort = true;
                                        break;
                                    }
                                }
                                catch 
                                {
                                    break;
                                }
                            }
                            while (!completed && Environment.TickCount - t < 5000);

                            isWaiting = false;

                            listener.Changed -= onChanged;

                            if (completed)
                            {
                                await Task.Delay(100);
                            }
                            else if (listener.HasChanged)
                            {
                                //something else set the clipboard

                                if (attempt < 3 && !abort)
                                {
                                    await Task.Delay(100);

                                    continue;
                                }
                            }

                            if (reset)
                            {
                                if (!await Windows.Clipboard.SetClipboardText(ctext))
                                {
                                    //failed to restore clipboard
                                }
                            }

                            return completed;
                        }
                        finally
                        {
                            data.Dispose();
                            cancel.Dispose();
                        }
                    }
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
