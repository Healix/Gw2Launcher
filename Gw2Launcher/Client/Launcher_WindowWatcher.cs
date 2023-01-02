using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Threading;
using System.Runtime.InteropServices;
using System.Management;
using Gw2Launcher.Windows.Native;
using System.IO;

namespace Gw2Launcher.Client
{
    static partial class Launcher
    {
        private class WindowWatcher : IDisposable
        {

            public class TimeoutEventArgs : EventArgs
            {
                public enum TimeoutReason
                {
                    Launcher,
                    DxWindow,
                }

                public TimeoutEventArgs(TimeoutReason reason)
                {
                    this.Reason = reason;
                }

                public TimeoutReason Reason
                {
                    get;
                    private set;
                }

                /// <summary>
                /// If unhandled, the watcher will continue, otherwise it'll be aborted
                /// </summary>
                public bool Handled
                {
                    get;
                    set;
                }
            }

            public class WindowChangedEventArgs : EventArgs
            {
                public enum EventType
                {
                    TitleChanged,
                    HandleChanged,
                    DxWindowHandleCreated,
                    DxWindowStyleChanged,
                    DxWindowCreated,
                    DxWindowInitialized,
                    DxWindowLoaded,
                    LauncherWindowHandleCreated,
                    LauncherWindowLoaded,
                    LauncherLoginComplete,
                    LauncherLoginCodeRequired,
                    LauncherLoginError,

                    WatcherExited
                }

                public EventType Type
                {
                    get;
                    set;
                }

                public IntPtr Handle
                {
                    get;
                    set;
                }

                public bool WasAlreadyStarted
                {
                    get;
                    set;
                }
            }

            public class WindowChangedEventArgs<T> : WindowChangedEventArgs
            {
                public T Data
                {
                    get;
                    set;
                }
            }

            class BoundsWatcher
            {
                private class WindowOptions : IDisposable
                {
                    public WindowOptions(Settings.IAccount account, Process p, IntPtr window, System.Drawing.Rectangle bounds, int timeout, WindowEvents.Events events, Action<IntPtr> onChanged, Action<IntPtr> onComplete, bool preventSizing, bool topMost)
                    {
                        this.account = account;
                        this.process = p;
                        this.window = window;
                        this.bounds = bounds;
                        this.timeout = timeout;
                        this.events = events;
                        this.onChanged = onChanged;
                        this.onComplete = onComplete;
                        this.preventSizing = preventSizing;
                        this.topMost = topMost;

                        time = Environment.TickCount;

                        if (events != null)
                        {
                            events.MoveSizeBegin += onEvent;
                            events.MinimizeStart += onEvent;
                        }
                    }

                    public int time;
                    public Settings.IAccount account;
                    public Process process;
                    public IntPtr window;
                    public System.Drawing.Rectangle bounds;
                    public int timeout;
                    public WindowEvents.Events events;
                    public Action<IntPtr> onChanged, onComplete;
                    public bool preventSizing;
                    public bool topMost;
                    public bool abort;
                    public bool removed;
                    public bool changed;
                    public int attempts;

                    void onEvent(object sender, WindowEvents.WindowEventsEventArgs e)
                    {
                        abort = true;
                    }

                    public void Dispose()
                    {
                        abort = true;

                        if (events != null)
                        {
                            events.MoveSizeBegin -= onEvent;
                            events.MinimizeStart -= onEvent;
                            events = null;
                        }

                        if (onComplete != null)
                            onComplete(window);
                    }
                }

                private Dictionary<IntPtr, WindowOptions> windows;
                private Queue<WindowOptions> queue;

                public void Cancel(IntPtr window)
                {
                    lock (this)
                    {
                        if (windows != null)
                        {
                            WindowOptions w;
                            if (windows.TryGetValue(window, out w))
                            {
                                w.Dispose();
                            }
                        }
                    }
                }

                public void SetBounds(Settings.IAccount account, Process p, IntPtr window, System.Drawing.Rectangle bounds, int timeout)
                {
                    SetBounds(account, p, window, bounds, timeout, null, null, null, false, false);
                }

                public void SetBounds(Settings.IAccount account, Process p, IntPtr window, System.Drawing.Rectangle bounds, int timeout, WindowEvents.Events events, Action<IntPtr> onChanged, Action<IntPtr> onComplete, bool preventSizing, bool topMost)
                {
                    bool b;

                    if (window == IntPtr.Zero)
                    {
                        window = FindDxWindow(p);
                        if (window == IntPtr.Zero)
                        {
                            if (onComplete != null)
                                onComplete(window);
                            return;
                        }
                    }

                    bounds = Util.ScreenUtil.ToDesktopBounds(bounds);

                    lock (this)
                    {
                        if (b = queue == null)
                        {
                            if (timeout > 0)
                            {
                                queue = new Queue<WindowOptions>();
                                windows = new Dictionary<IntPtr, WindowOptions>();
                            }
                        }

                        WindowOptions w;
                        if (!b && windows.TryGetValue(window, out w))
                        {
                            w.removed = true;
                            w.Dispose();
                        }

                        if (timeout > 0)
                        {
                            windows[window] = w = new WindowOptions(account, p, window, bounds, timeout, events, onChanged, onComplete, preventSizing, topMost);
                            queue.Enqueue(w);
                        }
                    }

                    if (timeout > 0)
                    {
                        if (b)
                        {
                            Task.Factory.StartNew(DoWatch);
                        }
                    }
                    else
                    {
                        try
                        {
                            using (var w = new WindowOptions(account, p, window, bounds, timeout, events, onChanged, onComplete, preventSizing, topMost))
                            {
                                SetBounds(w);
                            }
                        }
                        catch (Exception e)
                        {
                            Util.Logging.Log(e);
                        }
                    }
                }

                private async void DoWatch()
                {
                    while (true)
                    {
                        await Task.Delay(500);

                        int count = -1;

                        do
                        {
                            WindowOptions w;
                            bool b;

                            lock (this)
                            {
                                if (count == -1)
                                    count = queue.Count;

                                w = queue.Dequeue();

                                if (w.abort || Environment.TickCount - w.time > w.timeout && (w.attempts > 1 || w.changed))
                                {
                                    var skip = w.abort;
                                    w.Dispose();

                                    if (!w.removed)
                                    {
                                        windows.Remove(w.window);
                                        w.removed = true;
                                    }

                                    if (skip)
                                        continue;
                                }
                                else
                                    queue.Enqueue(w);
                            }

                            try
                            {
                                b = SetBounds(w);
                            }
                            catch (Exception e)
                            {
                                Util.Logging.Log(e);
                                b = false;
                            }

                            if (!b)
                            {
                                w.Dispose();
                            }
                        }
                        while (--count > 0);

                        lock (this)
                        {
                            if (queue.Count == 0)
                            {
                                queue = null;
                                windows = null;
                                return;
                            }
                        }
                    }
                }

                private bool SetBounds(WindowOptions o)
                {
                    var placement = new WINDOWPLACEMENT();
                    if (!NativeMethods.GetWindowPlacement(o.window, ref placement))
                    {
                        ++o.attempts;
                        return false;
                    }

                    if (!placement.rcNormalPosition.Equals(o.bounds))
                    {
                        ++o.attempts;

                        if (Util.ScreenUtil.IsFullScreen(placement.rcNormalPosition.ToRectangle()))
                            return false;

                        o.changed = false;

                        var async = o.timeout > 0;
                        if (async)
                            placement.flags = WindowPlacementFlags.WPF_ASYNCWINDOWPLACEMENT;
                        placement.rcNormalPosition = RECT.FromRectangle(o.bounds);
                        if (!NativeMethods.SetWindowPlacement(o.window, ref placement))
                            return false;

                        if (!async)
                        {
                            o.changed = true;

                            if (o.onChanged != null)
                                o.onChanged(o.window);

                            if (AccountWindowEvent != null)
                            {
                                try
                                {
                                    AccountWindowEvent(o.account, new AccountWindowEventEventArgs(AccountWindowEventEventArgs.EventType.BoundsChanged, o.process, o.window));
                                }
                                catch { }
                            }
                        }

                        return true;
                    }
                    else if (!o.changed)
                    {
                        o.changed = true;

                        if (o.onChanged != null)
                            o.onChanged(o.window);

                        if (AccountWindowEvent != null)
                        {
                            try
                            {
                                AccountWindowEvent(o.account, new AccountWindowEventEventArgs(AccountWindowEventEventArgs.EventType.BoundsChanged, o.process, o.window));
                            }
                            catch { }
                        }
                    }

                    if (o.preventSizing && Windows.WindowLong.HasValue(o.window, GWL.GWL_STYLE, WindowStyle.WS_MAXIMIZEBOX)
                        || o.topMost && !HasTopMost(o.window))
                    {
                        if (o.onChanged != null)
                            o.onChanged(o.window);
                    }

                    return true;
                }

                public void Dispose()
                {
                    lock(this)
                    {
                        if (queue != null)
                        {
                            foreach (var q in queue)
                            {
                                q.abort = true;
                            }
                        }
                    }
                }
            }

            class AuthWatcher
            {
                private Dictionary<Account, IntPtr> accounts;
                private Dictionary<IntPtr, Account> windows;
                private bool isActive;

                public AuthWatcher()
                {
                    accounts = new Dictionary<Account, IntPtr>();
                    windows = new Dictionary<IntPtr, Account>();
                }

                private async void DoWatch()
                {
                    var h = NativeMethods.GetForegroundWindow();
                    var w = IntPtr.Zero;
                    Account a = null;

                    while (true)
                    {
                        if (w != h)
                        {
                            w = h;

                            lock (this)
                            {
                                if (!windows.TryGetValue(h, out a))
                                {
                                    if (windows.Count == 0)
                                    {
                                        isActive = false;
                                        return;
                                    }
                                }
                            }
                        }
                        
                        if (a == null)
                        {
                            await Task.Delay(1000);

                            h = NativeMethods.GetForegroundWindow();
                        }
                        else
                        {
                            while (true)
                            {
                                if ((System.Windows.Forms.Form.MouseButtons & System.Windows.Forms.MouseButtons.Right) == System.Windows.Forms.MouseButtons.Right)
                                {
                                    var start = Environment.TickCount;
                                    var handled = false;
                                    var p = System.Windows.Forms.Cursor.Position;
                                    var r = new System.Drawing.Rectangle(p.X - 10, p.Y - 10, 20, 20);
                                    var counter = 0;

                                    //watch for the right button being held down and not moved -- won't stop until released

                                    do
                                    {
                                        await Task.Delay(100);

                                        if ((System.Windows.Forms.Form.MouseButtons & System.Windows.Forms.MouseButtons.Right) == System.Windows.Forms.MouseButtons.Right)
                                        {
                                            if (!handled)
                                            {
                                                ++counter;
                                                if (Environment.TickCount - start > 500)
                                                {
                                                    handled = true;
                                                    p = System.Windows.Forms.Cursor.Position;
                                                    //ensure the window is still active and the cursor is within it
                                                    if (counter > 2 && NativeMethods.GetForegroundWindow() == w && r.Contains(p))
                                                    {
                                                        RECT bounds;
                                                        if (NativeMethods.GetWindowRect(w, out bounds) && bounds.ToRectangle().Contains(p))
                                                        {
                                                            DoTotp(w, a.Settings.TotpKey);
                                                        }
                                                    }
                                                }
                                            }
                                        }
                                        else
                                        {
                                            break;
                                        }
                                    }
                                    while (true);
                                }

                                await Task.Delay(200);

                                h = NativeMethods.GetForegroundWindow();
                                if (h != w)
                                    break;
                            }
                        }
                    }
                }

                private void DoTotp(IntPtr w, byte[] key)
                {
                    if (key == null)
                        return;

                    try
                    {
                        foreach (var c in Tools.Totp.Generate(key))
                        {
                            NativeMethods.PostMessage(w, (uint)WindowMessages.WM_CHAR, (IntPtr)c, IntPtr.Zero);
                        }
                    }
                    catch { }
                }

                public void Add(Account account, IntPtr window)
                {
                    lock (this)
                    {
                        IntPtr h;
                        if (accounts.TryGetValue(account, out h))
                        {
                            if (h == window)
                                return;
                            accounts.Remove(account);
                            windows.Remove(h);
                        }
                        accounts[account] = window;
                        windows[window] = account;

                        if (!isActive)
                        {
                            isActive = true;
                            DoWatch();
                        }
                    }
                }

                public void Remove(Account account)
                {
                    lock (this)
                    {
                        IntPtr h;
                        if (accounts.TryGetValue(account, out h))
                        {
                            accounts.Remove(account);
                            windows.Remove(h);
                        }
                    }
                }

                public void Dispose()
                {
                    lock(this)
                    {
                        accounts.Clear();
                        windows.Clear();
                    }
                }
            }

            class Watchers
            {
                private BoundsWatcher _BoundsWatcher;
                public BoundsWatcher BoundsWatcher
                {
                    get
                    {
                        if (_BoundsWatcher == null)
                        {
                            lock (this)
                            {
                                if (_BoundsWatcher == null)
                                    _BoundsWatcher = new BoundsWatcher();
                            }
                        }
                        return _BoundsWatcher;
                    }
                }

                private AuthWatcher _AuthWatcher;
                public AuthWatcher AuthWatcher
                {
                    get
                    {
                        if (_AuthWatcher == null)
                        {
                            lock (this)
                            {
                                if (_AuthWatcher == null)
                                    _AuthWatcher = new AuthWatcher();
                            }
                        }
                        return _AuthWatcher;
                    }
                }

                public void Dispose()
                {
                    lock(this)
                    {
                        if (_BoundsWatcher != null)
                        {
                            _BoundsWatcher.Dispose();
                            _BoundsWatcher = null;
                        }

                        if (_AuthWatcher != null)
                        {
                            _AuthWatcher.Dispose();
                            _AuthWatcher = null;
                        }
                    }
                }
            }

            public class HiddenWindow : IDisposable
            {
                private bool wasLayered;
                private Windows.DebugEvents.IDebugEventsToken dt;

                public HiddenWindow(int pid, ProcessOptions options = null)
                {
                    dt = Windows.DebugEvents.Instance.Add(pid, options != null ? options.UserName : null, options != null ? options.Password : null);
                    //if (h != IntPtr.Zero)
                    //{
                    //    this.Handle = h;
                    //}
                }

                ~HiddenWindow()
                {
                    Dispose();
                }

                public void Hide(IntPtr handle)
                {
                    this.Handle = handle;

                    NativeMethods.ShowWindow(handle, ShowWindowCommands.ForceMinimize);

                    //hides window by setting opacity to 0 - will freeze if the window is not responding
                    Task.Run(new Action(delegate
                    {
                        wasLayered = Windows.WindowLong.Add(handle, GWL.GWL_EXSTYLE, WindowStyle.WS_EX_LAYERED) == IntPtr.Zero;
                        var b2 = NativeMethods.SetLayeredWindowAttributes(handle, 0, 0, LayeredWindowFlags.LWA_ALPHA);

                        Util.Logging.Log(handle + ": " + wasLayered + ", " + b2);
                    }));
                }

                public IntPtr Handle
                {
                    get;
                    private set;
                }

                public bool Disposing
                {
                    get;
                    private set;
                }

                public async void DisposeAfter(int ms)
                {
                    if (Disposing)
                        return;
                    Disposing = true;
                    await Task.Delay(ms);
                    Dispose();
                }

                public void Dispose()
                {
                    var h = this.Handle;
                    Disposing = true;

                    if (dt != null)
                    {
                        dt.Dispose();
                        dt = null;
                    }

                    if (h != IntPtr.Zero)
                    {
                        this.Handle = IntPtr.Zero;

                        if (wasLayered)
                            NativeMethods.SetLayeredWindowAttributes(h, 0, 0, (LayeredWindowFlags)0);
                        else
                            Windows.WindowLong.Remove(h, GWL.GWL_EXSTYLE, WindowStyle.WS_EX_LAYERED);
                    }
                }
            }

            public event EventHandler<WindowChangedEventArgs> WindowChanged;
            public event EventHandler<CrashReason> WindowCrashed;
            public event EventHandler<int> LoginComplete;
            public event EventHandler<TimeoutEventArgs> Timeout;
            /// <summary>
            /// OBSOLETE: was used with "-nopatchui -email -password", which would stay on a black screen when authentication failed
            /// </summary>
            //public event EventHandler<Tools.ArenaAccount> AuthenticationRequired;

            private const string DX_WINDOW_CLASSNAME = "ArenaNet_Dx_Window_Class"; //gw2 and gw1 main game window
            private const string DX_WINDOW_CLASSNAME_DX11BETA = "ArenaNet_Gr_Window_Class"; //gw2 dx11 main game window
            private const int DX_WINDOW_CLASSNAME_LENGTH = 24;
            private const string DIALOG_WINDOW_CLASSNAME = "ArenaNet_Dialog_Class"; //gw1 patcher, gw2 launcher error window (Gw2.dat/Local.dat errors, "needs to be patched before using -sharearchive", etc)
            private const int DIALOG_WINDOW_CLASSNAME_LENGTH = 21;
            private const string LAUNCHER_WINDOW_CLASSNAME = "ArenaNet"; //gw2 launcher
            private const int LAUNCHER_WINDOW_CLASSNAME_LENGTH = 8;
            private const string EDIT_CLASSNAME = "Edit"; //textbox crash report
            private const int EDIT_CLASSNAME_LENGTH = 4;

            public enum CrashReason
            {
                Unknown,
                PatchRequired,
                NoPatchUI,
                ErrorDialog,
            }

            private static Watchers watchers;

            private Process process;
            private Task watcher;
            private bool canReadMemory;
            private bool processWasAlreadyStarted;

            static WindowWatcher()
            {
                watchers = new Watchers();

                Launcher.AllQueuedLaunchesCompleteAllAccountsExited += OnAllQueuedLaunchesCompleteAllAccountsExited;
            }

            private static void OnAllQueuedLaunchesCompleteAllAccountsExited(object sender, EventArgs e)
            {
                watchers.Dispose();
            }

            /// <summary>
            /// Watches the process and reports when the DX window is created
            /// </summary>
            public WindowWatcher(Account account, Process p, bool isProcessStarter, bool windowed, Account.LaunchSession session)
            {
                this.process = p;
                this.processWasAlreadyStarted = !isProcessStarter;
                this.Account = account;
                //this.watchBounds = watchBounds;
                //this.watchAutologin = account.Settings.AutomaticLogin && account.Settings.HasCredentials && !Settings.DisableAutomaticLogins;
                this.Session = session;

                canReadMemory = Environment.Is64BitProcess || !Environment.Is64BitOperatingSystem;
            }

            public void Dispose()
            {
                if (watcher != null && watcher.IsCompleted)
                    watcher.Dispose();
            }

            public void Start()
            {
                if (watcher == null)
                {
                    watcher = new Task(WatchWindow2, TaskCreationOptions.LongRunning);
                    watcher.Start();
                }
            }

            public Process Process
            {
                get
                {
                    return this.process;
                }
            }

            public Account Account
            {
                get;
                private set;
            }

            public ProcessOptions ProcessOptions
            {
                get;
                set;
            }

            public Account.LaunchSession Session
            {
                get;
                private set;
            }

            public WindowEvents.Events Events
            {
                get;
                set;
            }

            public bool SupportsLoginEvents
            {
                get;
                private set;
            }

            /// <summary>
            /// Finds child processes
            /// </summary>
            /// <param name="parent">Parent process</param>
            /// <param name="cids">Array to place IDs of child processes (array size determines how many to search for)</param>
            /// <returns>Number of child processes found up to the length of the array</returns>
            private int GetChildProcesses(Process parent, int[] cids)
            {
                var i = 0;
                var pid = parent.Id;
                var startTime = parent.StartTime;

                if (canReadMemory)
                {
                    var noChild = true;
                    var processes = Process.GetProcesses();

                    using (var pi = new Windows.ProcessInfo())
                    {
                        foreach (var p in processes)
                        {
                            using (p)
                            {
                                if (noChild)
                                {
                                    try
                                    {
                                        if (pi.Open(p.Id))
                                        {
                                            if (pi.GetParent() == pid && p.StartTime >= startTime)
                                            {
                                                cids[i++] = p.Id;
                                                noChild = i < cids.Length;
                                            }
                                        }
                                    }
                                    catch (Exception e)
                                    {
                                        Util.Logging.Log(e);
                                    }
                                }
                            }
                        }
                    }

                    return i;
                }

                using (var searcher = new ManagementObjectSearcher("SELECT ProcessId FROM Win32_Process WHERE ParentProcessID=" + pid))
                {
                    using (var results = searcher.Get())
                    {
                        foreach (ManagementObject o in results)
                        {
                            try
                            {
                                var cid = (int)(uint)o["ProcessId"];
                                using (var p = Process.GetProcessById(cid))
                                {
                                    if (p.StartTime >= startTime)
                                    {
                                        cids[i++] = cid;
                                        if (i == cids.Length)
                                            break;
                                    }
                                }
                            }
                            catch { }
                        }
                    }
                }

                return i;
            }

            private int GetChildProcess(int pid)
            {
                if (canReadMemory)
                {
                    var noChild = true;
                    var processes = Process.GetProcesses();
                    var cid = 0;

                    using (var pi = new Windows.ProcessInfo())
                    {
                        foreach (var p in processes)
                        {
                            using (p)
                            {
                                if (noChild)
                                {
                                    try
                                    {
                                        if (pi.Open(p.Id))
                                        {
                                            if (pi.GetParent() == pid)
                                            {
                                                noChild = false;
                                                cid = p.Id;
                                            }
                                        }
                                    }
                                    catch (Exception e)
                                    {
                                        Util.Logging.Log(e);
                                    }
                                }
                            }
                        }
                    }

                    return cid;
                }

                using (var searcher = new ManagementObjectSearcher("SELECT ProcessId FROM Win32_Process WHERE ParentProcessID=" + pid))
                {
                    using (var results = searcher.Get())
                    {
                        foreach (ManagementObject o in results)
                        {
                            return (int)(uint)o["ProcessId"];
                        }
                    }
                }

                return 0;
            }

            private Windows.FindWindow.SearchResult Find(IntPtr handle, Windows.FindWindow.TextComparer classCallback, Windows.FindWindow.TextComparer textCallback)
            {
                try
                {
                    var r = Windows.FindWindow.FindChildren(handle, classCallback, textCallback, 1);
                    if (r.Count > 0)
                        return r[0];
                }
                catch (Exception e)
                { 
                    Util.Logging.Log(e);
                }

                return null;
            }

            private void WatchWindow2()
            {
                try
                {
                    WatchWindow();
                }
                catch (Exception e)
                {
                    Util.Logging.Log(e);
                }

                if (WindowChanged != null)
                {
                    WindowChanged(this, new WindowChangedEventArgs()
                        {
                            Type = WindowChangedEventArgs.EventType.WatcherExited
                        });
                }
            }

            private void WatchWindow()
            {
                IntPtr handle;
                try
                {
                    handle = Windows.FindWindow.FindMainWindow(process);
                }
                catch (Exception e)
                {
                    Util.Logging.Log(e);
                    return;
                }

                Windows.FindWindow.TextComparer classCallback = new Windows.FindWindow.TextComparer(
                    delegate(string name, StringBuilder sb)
                    {
                        if (sb.Length == EDIT_CLASSNAME_LENGTH)
                        {
                            return sb.ToString().Equals(EDIT_CLASSNAME);
                        }
                        return false;
                    });

                Windows.FindWindow.TextComparer textCallback = new Windows.FindWindow.TextComparer(
                    delegate(string name, StringBuilder sb)
                    {
                        if (name != null && name.Equals(EDIT_CLASSNAME))
                        {
                            return sb.Length > 500 && sb[0] == '*';
                        }

                        return true;
                    });
                
                var isGw2 = this.Account.Type == AccountType.GuildWars2;
                var buffer = new StringBuilder(DX_WINDOW_CLASSNAME_LENGTH + 1);
                var weventbuffer = new StringBuilder(DX_WINDOW_CLASSNAME_LENGTH + 1);

                var wce = new WindowChangedEventArgs();
                var _handle = IntPtr.Zero;

                int timeout = 0, 
                    timeoutStart = 0, 
                    delay = 500;

                var _wasAlreadyStarted = processWasAlreadyStarted;

                FileSystemWatcher fwatcher = null;
                var authWatch = Settings.AuthenticatorPastingEnabled.Value && isGw2 && this.Account.Settings.TotpKey != null;
                var weventwaiter = new ManualResetEvent(false);
                var hasEvents = false;
                var weventT = WindowChangedEventArgs.EventType.WatcherExited;
                var wevent = windowEvents.Register(process.Id, 0x8000, 0x8002,
                    delegate(IntPtr hWinEventHook, uint eventType, IntPtr hwnd, int idObject, int idChild, uint dwEventThread, uint dwmsEventTime)
                    {
                        if (idObject == 0 && idChild == 0)
                        {
                            switch (eventType)
                            {
                                case 0x8000: //EVENT_OBJECT_CREATE

                                    if (WindowChanged != null)
                                    {
                                        try
                                        {
                                            var t = (WindowChangedEventArgs.EventType)0;

                                            switch (NativeMethods.GetClassName(hwnd, weventbuffer, weventbuffer.Capacity + 1))
                                            {
                                                case LAUNCHER_WINDOW_CLASSNAME_LENGTH:

                                                    if (weventbuffer.ToString().Equals(LAUNCHER_WINDOW_CLASSNAME))
                                                    {
                                                        t = WindowChangedEventArgs.EventType.LauncherWindowHandleCreated;
                                                    }

                                                    break;
                                                case DIALOG_WINDOW_CLASSNAME_LENGTH:

                                                    if (!isGw2 && weventbuffer.ToString().Equals(DIALOG_WINDOW_CLASSNAME))
                                                    {
                                                        t = WindowChangedEventArgs.EventType.LauncherWindowHandleCreated;
                                                    }

                                                    break;
                                                case DX_WINDOW_CLASSNAME_LENGTH:

                                                    var s = weventbuffer.ToString();
                                                    if (s.Equals(DX_WINDOW_CLASSNAME_DX11BETA) || s.Equals(DX_WINDOW_CLASSNAME))
                                                    {
                                                        t = WindowChangedEventArgs.EventType.DxWindowHandleCreated;
                                                    }

                                                    break;
                                            }

                                            if (t != 0 && t != weventT)
                                            {
                                                weventT = t;

                                                WindowChanged(this, new WindowChangedEventArgs()
                                                {
                                                    Handle = hwnd,
                                                    Type = t,
                                                    WasAlreadyStarted = _wasAlreadyStarted,
                                                });
                                            }
                                        }
                                        catch { }
                                    }

                                    break;
                                case 0x8002: //EVENT_OBJECT_SHOW

                                    try
                                    {
                                        if (weventwaiter != null)
                                            weventwaiter.Set();
                                    }
                                    catch { }

                                    break;
                                default:

                                    return;
                            }
                        }
                    });
                EventHandler<Account> onExit = delegate
                {
                    try
                    {
                        if (weventwaiter != null)
                            weventwaiter.Set();
                    }
                    catch { }
                };
                EventHandler onThreadExit = delegate
                {
                    try
                    {
                        using (wevent) { }
                    }
                    catch { }
                };
                System.Windows.Forms.Application.ThreadExit += onThreadExit;
                this.Account.Process.Exited += onExit;

                try
                {
                    var first = false;

                    do
                    {
                        try
                        {
                            do
                            {
                                if (first)
                                {
                                    if (weventwaiter.WaitOne(delay))
                                    {
                                        weventwaiter.Reset();
                                        if (!hasEvents)
                                        {
                                            hasEvents = true;
                                            delay = 1000;
                                        }
                                    }
                                }
                                else
                                {
                                    first = true;
                                }

                                if (process.HasExited)
                                    return;

                                process.Refresh();
                                handle = Windows.FindWindow.FindMainWindow(process);

                                if (handle != IntPtr.Zero)
                                {
                                    break;
                                }
                            }
                            while (true);
                        }
                        catch (Exception e)
                        {
                            Util.Logging.Log(e);

                            return;
                        }

                        try
                        {
                            //if (this.process.HasExited)
                            //{
                            //    //no longer applicable - nopatchui exiting without creating a window likely meant the client wasn't up to date

                            //    //if (handle == IntPtr.Zero && (IsAutomaticLogin(this.Account.Settings) || Util.Args.Contains(this.Args, "nopatchui")))
                            //    //{
                            //    //    var elapsed = this.process.ExitTime.Subtract(this.process.StartTime).TotalSeconds;
                            //    //    if (elapsed < 60 && WindowCrashed != null)
                            //    //        WindowCrashed(this, CrashReason.NoPatchUI);
                            //    //}
                            //    break;
                            //}

                            if (timeout != 0 && Environment.TickCount - timeoutStart > timeout)
                            {
                                if (Timeout != null)
                                {
                                    var te = new TimeoutEventArgs(TimeoutEventArgs.TimeoutReason.Launcher);
                                    Timeout(this, te);
                                    if (te.Handled)
                                        return;
                                }

                                timeout = 0;
                            }
                        }
                        catch (Exception e)
                        {
                            Util.Logging.Log(e);
                            return;
                        }

                        if (_handle == handle)
                            continue;

                        _handle = handle;
                        wce.Handle = handle;

                        var wasAlreadyStarted = _wasAlreadyStarted;
                        wce.WasAlreadyStarted = wasAlreadyStarted;
                        _wasAlreadyStarted = false;

                        if (WindowChanged != null)
                        {
                            wce.Type = WindowChangedEventArgs.EventType.HandleChanged;
                            WindowChanged(this, wce);
                        }

                        switch (NativeMethods.GetClassName(handle, buffer, buffer.Capacity + 1))
                        { 
                            case 0:

                                continue;
                            //main game window (gw1 and gw2)
                            case DX_WINDOW_CLASSNAME_LENGTH:

                                #region DX_WINDOW_CLASSNAME_LENGTH
                                
                                var s = buffer.ToString();
                                if (s.Equals(DX_WINDOW_CLASSNAME_DX11BETA) || s.Equals(DX_WINDOW_CLASSNAME))
                                {
                                    timeout = 0;

                                    if (authWatch)
                                    {
                                        watchers.AuthWatcher.Remove(this.Account);
                                    }

                                    if (WindowChanged != null)
                                    {
                                        if (weventT != WindowChangedEventArgs.EventType.DxWindowHandleCreated)
                                        {
                                            //backup for when wevents aren't supported
                                            wce.Type = weventT = WindowChangedEventArgs.EventType.DxWindowHandleCreated;
                                            WindowChanged(this, wce);
                                        }
                                        wce.Type = WindowChangedEventArgs.EventType.DxWindowCreated;
                                        WindowChanged(this, wce);
                                    }

                                    //window initialization order:
                                    //gw2: title change > volume (window is ready) > coherentui
                                    //gw1: ArenaNet_Dialog_Class > ArenaNet_Dx_Window_Class > volume (window is ready)

                                    int pidChild = 0;
                                    bool hasVolume = false;
                                    DateTime limit;

                                    using (var volumeControl = new Windows.Volume.VolumeControl(process.Id))
                                    {
                                        if (wasAlreadyStarted && ((hasVolume = volumeControl.Query()) || isGw2 && (pidChild = GetChildProcess(process.Id)) > 0))
                                        {
                                            //window is already loaded

                                            if (WindowChanged != null)
                                            {
                                                wce.Type = WindowChangedEventArgs.EventType.TitleChanged;
                                                WindowChanged(this, wce);
                                            }
                                        }
                                        else
                                        {
                                            try
                                            {
                                                limit = DateTime.UtcNow.AddSeconds(30);
                                                var ws1 = IntPtr.Zero;

                                                do
                                                {
                                                    //window style changes when loading into fullscreen mode
                                                    var ws2 = NativeMethods.GetWindowLongPtr(handle, (int)GWL.GWL_STYLE);
                                                    if (ws2 != ws1)
                                                    {
                                                        if (ws1 != IntPtr.Zero && WindowChanged != null)
                                                        {
                                                            wce.Type = WindowChangedEventArgs.EventType.DxWindowStyleChanged;
                                                            WindowChanged(this, wce);
                                                        }

                                                        ws1 = ws2;
                                                    }

                                                    if (NativeMethods.GetWindowText(handle, buffer, 2) > 0 && buffer[0] != 'U') //window text is initially Untitled (gw2)
                                                    {
                                                        if (WindowChanged != null)
                                                        {
                                                            wce.Type = WindowChangedEventArgs.EventType.TitleChanged;
                                                            WindowChanged(this, wce);
                                                        }

                                                        break;
                                                    }
                                                }
                                                while (DateTime.UtcNow < limit && !process.WaitForExit(100));
                                            }
                                            catch (Exception e)
                                            {
                                                Util.Logging.Log(e);
                                            }

                                            //var nextCheck_Seconds = 3;
                                            //var memoryUsage = process.PeakWorkingSet64;
                                            //var nextCheck = DateTime.UtcNow.AddSeconds(nextCheck_Seconds);
                                            //var memoryChecks = 0;
                                            //var verified = false;

                                            limit = DateTime.UtcNow.AddMinutes(3);

                                            if (isGw2)
                                            {

                                                do
                                                {
                                                    if (hasVolume = volumeControl.Query() || (pidChild = GetChildProcess(process.Id)) > 0)
                                                    {
                                                        break;
                                                    }

                                                    #region -nopatchui activity check (obsolete)

                                                    //watching for changes in memory usage to check if it's still doing something
                                                    //bypassing the launcher (-nopatchui) can cause it to get stuck on authentication if either the credentials are wrong or the network isn't authorized
                                                    //if (!hasVolume && DateTime.UtcNow > nextCheck)
                                                    //{
                                                    //    process.Refresh();

                                                    //    var memoryChange = process.PeakWorkingSet64 - memoryUsage;
                                                    //    memoryUsage += memoryChange;

                                                    //    if (memoryChange < 1000000)
                                                    //    {
                                                    //        if (++memoryChecks == 3)
                                                    //        {
                                                    //            //this account may be stuck trying to authenticate

                                                    //            var account = this.Account.Settings;
                                                    //            if (account.HasCredentials && account.NetworkAuthorizationState != Settings.NetworkAuthorizationState.Disabled && Settings.NetworkAuthorization.HasValue)
                                                    //            {
                                                    //                if (account.NetworkAuthorizationState == Settings.NetworkAuthorizationState.Unknown)
                                                    //                {
                                                    //                    if (AuthenticationRequired != null)
                                                    //                        AuthenticationRequired(this, null);

                                                    //                    return;
                                                    //                }
                                                    //                else
                                                    //                {
                                                    //                    Tools.ArenaAccount session;
                                                    //                    switch (NetworkAuthorization.Verify(account, true, null, out session))
                                                    //                    {
                                                    //                        case NetworkAuthorization.VerifyResult.Completed:
                                                    //                        case NetworkAuthorization.VerifyResult.Required:

                                                    //                            if (AuthenticationRequired != null)
                                                    //                                AuthenticationRequired(this, session);

                                                    //                            return;
                                                    //                        case NetworkAuthorization.VerifyResult.OK:

                                                    //                            //authentication was ok - assuming it's a slow load
                                                    //                            nextCheck_Seconds = 10;
                                                    //                            //verified = true;

                                                    //                            break;
                                                    //                        case NetworkAuthorization.VerifyResult.None:
                                                    //                        default:

                                                    //                            //authentication isn't being tracked - assuming it's stuck
                                                    //                            nextCheck_Seconds = 10;

                                                    //                            break;
                                                    //                    }
                                                    //                }
                                                    //            }
                                                    //            else
                                                    //            {
                                                    //                //authentication isn't enabled
                                                    //                nextCheck_Seconds = 10;
                                                    //            }
                                                    //        }
                                                    //        else if (memoryChecks > 6)
                                                    //        {
                                                    //            if (AuthenticationRequired != null)
                                                    //                AuthenticationRequired(this, null);

                                                    //            return;
                                                    //        }
                                                    //    }
                                                    //    else
                                                    //        memoryChecks = 0;

                                                    //    nextCheck = DateTime.UtcNow.AddSeconds(nextCheck_Seconds);
                                                    //}

                                                    #endregion
                                                }
                                                while (DateTime.UtcNow < limit && !process.WaitForExit(500));
                                            }
                                            else
                                            {
                                                using (var pi = new Windows.ProcessInfo())
                                                {
                                                    var modules = new string[] { "GwLoginClient.dll" };
                                                    var canRead = pi.Open(process.Id);

                                                    do
                                                    {
                                                        if (hasVolume = volumeControl.Query() || FindModules(canRead ? pi : null, modules, false))
                                                        {
                                                            break;
                                                        }
                                                    }
                                                    while (DateTime.UtcNow < limit && !process.WaitForExit(500));
                                                }
                                            }
                                        }
                                    }

                                    if (WindowChanged != null && !process.HasExited)
                                    {
                                        wce.Type = WindowChangedEventArgs.EventType.DxWindowInitialized;
                                        WindowChanged(this, wce);
                                    }

                                    if (isGw2)
                                    {
                                        using (var pi = new Windows.ProcessInfo())
                                        {
                                            var modules = new string[] { "icm32.dll", "mscms.dll" };
                                            var canRead = pi.Open(process.Id);
                                            var foundModules = false;

                                            //coherentui's child process will exit after loading a character, making this unsuitable for clients that were already started
                                            FindCoherentChildProcess(_handle, pidChild,
                                                delegate
                                                {
                                                    //alternatively look for loaded modules; icm32.dll is loaded after CoherentUI, but before it's finished loading
                                                    //either way, character select is loaded at this point

                                                    if (FindModules(canRead ? pi : null, modules, false))
                                                    {
                                                        foundModules = true;
                                                        return false;
                                                    }

                                                    return true;
                                                });

                                            //forcing module check - after loading CoherentUI, the game game can hang prior to loading modules
                                            if (!foundModules)
                                            {
                                                limit = DateTime.UtcNow.AddSeconds(Settings.DxTimeout.Value > 0 ? Settings.DxTimeout.Value : 180);

                                                do
                                                {
                                                    if (FindModules(canRead ? pi : null, modules, false))
                                                    {
                                                        foundModules = true;
                                                        break;
                                                    }

                                                    if (DateTime.UtcNow > limit)
                                                    {
                                                        if (Settings.DxTimeout.Value > 0 && Timeout != null)
                                                        {
                                                            var te = new TimeoutEventArgs(TimeoutEventArgs.TimeoutReason.DxWindow);
                                                            Timeout(this, te);
                                                            if (te.Handled)
                                                                return;
                                                        }

                                                        limit = DateTime.UtcNow.AddMinutes(1);
                                                    }

                                                    process.Refresh();
                                                    if (_handle != Windows.FindWindow.FindMainWindow(this.process))
                                                        break;
                                                }
                                                while (!process.WaitForExit(1000));
                                            }
                                        }
                                    }
                                    else
                                    {
                                        using (var pi = new Windows.ProcessInfo())
                                        {
                                            //note audioses.dll/midimap.dll won't be loaded when -nosound is used, wintypes.dll isn't loaded until the window is focused
                                            var modules = new string[] { "AUDIOSES.DLL", "midimap.dll", "wintypes.dll" };
                                            var canRead = pi.Open(process.Id);

                                            limit = DateTime.UtcNow.AddSeconds(10);

                                            do
                                            {
                                                if (FindModules(canRead ? pi : null, modules, false))
                                                {
                                                    break;
                                                }
                                            }
                                            while (DateTime.UtcNow < limit && !process.WaitForExit(500));
                                        }
                                    }

                                    if (WindowChanged != null && !process.HasExited)
                                    {
                                        wce.Type = WindowChangedEventArgs.EventType.DxWindowLoaded;
                                        WindowChanged(this, wce);
                                    }

                                    return;
                                }

                                #endregion

                                break;
                            //gw1 patcher, gw2 error dialog
                            case DIALOG_WINDOW_CLASSNAME_LENGTH:

                                #region DIALOG_WINDOW_CLASSNAME

                                if (buffer.ToString().Equals(DIALOG_WINDOW_CLASSNAME))
                                {
                                    if (isGw2)
                                    {
                                        //gw2 error (probably needs to patch)

                                        if (WindowCrashed != null)
                                            WindowCrashed(this, CrashReason.ErrorDialog);

                                        return;
                                    }
                                    else
                                    {
                                        //gw1 patcher

                                        if (WindowChanged != null)
                                        {
                                            wce.Type = WindowChangedEventArgs.EventType.TitleChanged;
                                            WindowChanged(this, wce);
                                        }
                                    }
                                }


                                #endregion

                                break;
                            //gw2 launcher
                            case LAUNCHER_WINDOW_CLASSNAME_LENGTH:

                                #region LAUNCHER_WINDOW_CLASSNAME_LENGTH

                                if (isGw2 && buffer.ToString().Equals(LAUNCHER_WINDOW_CLASSNAME))
                                {
                                    if (WindowChanged != null)
                                    {
                                        if (weventT != WindowChangedEventArgs.EventType.LauncherWindowHandleCreated)
                                        {
                                            //backup for when wevents aren't supported
                                            wce.Type = weventT = WindowChangedEventArgs.EventType.LauncherWindowHandleCreated;
                                            WindowChanged(this, wce);
                                        }
                                        wce.Type = WindowChangedEventArgs.EventType.TitleChanged;
                                        WindowChanged(this, wce);
                                    }

                                    int r;
                                    CoherentWatcher cw = null;

                                    if (!Settings.Tweaks.Launcher.HasValue || (Settings.Tweaks.Launcher.Value.CoherentOptions & (Settings.LauncherTweaks.CoherentFlags.WaitForLoaded | Settings.LauncherTweaks.CoherentFlags.WaitForMemory)) != 0)
                                    {
                                        if (!Settings.Tweaks.Launcher.HasValue || Settings.Tweaks.Launcher.Value.WaitForCoherentLoaded)
                                        {
                                            try
                                            {
                                                cw = WaitForCoherentHost(_handle, out r);
                                                if (cw == null && r == -1)
                                                    break;
                                            }
                                            catch (Exception e)
                                            {
                                                Util.Logging.Log(e);
                                            }

                                            if (cw != null)
                                            {
                                                var cwe = WaitForCoherentEvent(handle, cw);

                                                if (cwe == CoherentWatcher.EventType.Error || cwe == CoherentWatcher.EventType.Exited)
                                                {
                                                    cw.Dispose();
                                                    cw = null;
                                                }
                                            }
                                        }

                                        if (cw == null)
                                        {
                                            do
                                            {
                                                //it's possible for the launcher to never fully load, such as when failing to initially connect
                                                r = FindCoherentChildProcesses(_handle, 0, 2, null);
                                                if (r > 0 || r == -1)
                                                    break;
                                            }
                                            while (!process.WaitForExit(500));
                                        }
                                        else
                                        {
                                            r = 0;
                                        }
                                    }
                                    else
                                    {
                                        r = 0;
                                        Util.Logging.LogEvent(Account.Settings, "Skipping all CoherentUI checks due to settings");
                                    }

                                    SupportsLoginEvents = cw != null;

                                    if (r != -1 && Settings.Tweaks.Launcher.HasValue && Settings.Tweaks.Launcher.Value.Delay > 0)
                                    {
                                        var d = DateTime.UtcNow.AddSeconds(Settings.Tweaks.Launcher.Value.Delay);
                                        while (!process.WaitForExit(500) && DateTime.UtcNow < d) { }
                                    }

                                    if (process.HasExited || r == -1)
                                    {
                                        using (cw) { }
                                        break;
                                    }

                                    var loginComplete = false;

                                    if (!wasAlreadyStarted)
                                    {
                                        if (LoginComplete != null || cw == null)
                                        {
                                            var gw2cache = Tools.Gw2Cache.FindPath(this.Account.Settings.UID);
                                            if (gw2cache != null)
                                            {
                                                try
                                                {
                                                    fwatcher = new FileSystemWatcher(Path.Combine(gw2cache, "user", "Local Storage"), "coui_web_0.localstorage");
                                                    fwatcher.NotifyFilter = NotifyFilters.CreationTime | NotifyFilters.LastWrite;

                                                    var start = Environment.TickCount;
                                                    FileSystemEventHandler onWrite = delegate(object o, FileSystemEventArgs e)
                                                    {
                                                        try
                                                        {
                                                            fwatcher.Dispose();
                                                            fwatcher = null;
                                                        }
                                                        catch { }

                                                        lock (this)
                                                        {
                                                            if (weventwaiter == null || loginComplete)
                                                                return;
                                                            loginComplete = true;
                                                        }

                                                        timeout = 0;

                                                        if (LoginComplete != null)
                                                            LoginComplete(this, Environment.TickCount - start);

                                                        if (WindowChanged != null)
                                                        {
                                                            WindowChanged(this, new WindowChangedEventArgs()
                                                            {
                                                                Handle = handle,
                                                                Type = WindowChangedEventArgs.EventType.LauncherLoginComplete,
                                                                WasAlreadyStarted = wasAlreadyStarted,
                                                            });
                                                        }
                                                    };
                                                    fwatcher.Changed += onWrite;
                                                    fwatcher.Created += onWrite;
                                                    fwatcher.EnableRaisingEvents = true;
                                                }
                                                catch
                                                {
                                                    if (fwatcher != null)
                                                    {
                                                        fwatcher.Dispose();
                                                        fwatcher = null;
                                                    }
                                                }

                                                if (fwatcher == null)
                                                {
                                                    WatchLogin(Path.Combine(gw2cache, "user", "Local Storage", "coui_web_0.localstorage"),
                                                        delegate
                                                        {
                                                            return weventwaiter != null && !loginComplete;
                                                        },
                                                        delegate(int duration)
                                                        {
                                                            lock (this)
                                                            {
                                                                if (weventwaiter == null || loginComplete)
                                                                    return;
                                                                loginComplete = true;
                                                            }

                                                            timeout = 0;

                                                            if (LoginComplete != null)
                                                                LoginComplete(this, duration);

                                                            if (WindowChanged != null)
                                                            {
                                                                WindowChanged(this, new WindowChangedEventArgs()
                                                                {
                                                                    Handle = handle,
                                                                    Type = WindowChangedEventArgs.EventType.LauncherLoginComplete,
                                                                    WasAlreadyStarted = wasAlreadyStarted,
                                                                });
                                                            }
                                                        });
                                                }
                                            }
                                        }

                                        if (Settings.LaunchTimeout.HasValue)
                                        {
                                            timeout = Settings.LaunchTimeout.Value * 1000;
                                            timeoutStart = Environment.TickCount;
                                        }
                                    }

                                    if (authWatch)
                                    {
                                        watchers.AuthWatcher.Add(this.Account, handle);
                                    }

                                    if (WindowChanged != null)
                                    {
                                        wce.Type = WindowChangedEventArgs.EventType.LauncherWindowLoaded;
                                        WindowChanged(this, wce);
                                    }

                                    if (cw != null)
                                    {
                                        using (cw)
                                        {
                                            var loginStart = Environment.TickCount;
                                            r = 0;

                                            while (r == 0)
                                            {
                                                var cwe = WaitForCoherentEvent(handle, cw,
                                                    delegate
                                                    {
                                                        return timeout == 0 || Environment.TickCount - timeoutStart <= timeout;
                                                    });

                                                switch (cwe)
                                                {
                                                    case CoherentWatcher.EventType.Exited:
                                                    case CoherentWatcher.EventType.Error:

                                                        r = -1;

                                                        break;
                                                    case CoherentWatcher.EventType.LoginCode:

                                                        if (WindowChanged != null)
                                                        {
                                                            wce.Type = WindowChangedEventArgs.EventType.LauncherLoginCodeRequired;
                                                            WindowChanged(this, wce);
                                                        }

                                                        break;
                                                    case CoherentWatcher.EventType.LoginComplete:

                                                        var b = false;

                                                        lock (this)
                                                        {
                                                            if (!loginComplete)
                                                            {
                                                                loginComplete = true;
                                                                b = true;
                                                            }
                                                        }

                                                        if (b)
                                                        {
                                                            timeout = 0;

                                                            if (LoginComplete != null)
                                                                LoginComplete(this, Environment.TickCount - loginStart);

                                                            if (WindowChanged != null)
                                                            {
                                                                wce.Type = WindowChangedEventArgs.EventType.LauncherLoginComplete;
                                                                WindowChanged(this, wce);
                                                            }
                                                        }

                                                        break;
                                                    case CoherentWatcher.EventType.LoginError:

                                                        if (WindowChanged != null)
                                                        {
                                                            wce.Type = WindowChangedEventArgs.EventType.LauncherLoginError;
                                                            WindowChanged(this, wce);
                                                        }

                                                        r = -1;

                                                        break;
                                                    case CoherentWatcher.EventType.LoginReady:

                                                        //this will trigger if the launcher returns to the login box (logout or code entry cancelled)
                                                        //could retry the login or restart the launcher

                                                        break;
                                                    case CoherentWatcher.EventType.None:

                                                        if (timeout != 0 && Environment.TickCount - timeoutStart > timeout)
                                                        {
                                                            if (Timeout != null)
                                                            {
                                                                var te = new TimeoutEventArgs(TimeoutEventArgs.TimeoutReason.Launcher);
                                                                Timeout(this, te);
                                                                if (te.Handled)
                                                                    return;
                                                            }

                                                            timeout = 0;
                                                        }

                                                        break;
                                                }
                                            }
                                        }
                                    }

                                    #region Detect remembered logins

                                    //DPAPI.dll is loaded after clicking login, but only when opting to save the login
                                    //this would only be useful for detecting manual logins

                                    //if (LoginBegin != null)
                                    //{
                                    //    var b = false;

                                    //    do
                                    //    {
                                    //        try
                                    //        {
                                    //            if (b && handle != Windows.FindWindow.FindMainWindow(process))
                                    //            {
                                    //                break;
                                    //            }
                                    //            else if (FindModule("DPAPI.dll"))
                                    //            {
                                    //                if (b) //only report if it wasn't already available
                                    //                    LoginBegin(this, EventArgs.Empty);
                                    //                break;
                                    //            }

                                    //            b = true;
                                    //            process.Refresh();
                                    //        }
                                    //        catch
                                    //        {
                                    //            break;
                                    //        }
                                    //    }
                                    //    while (!process.WaitForExit(500));
                                    //}

                                    #endregion
                                }

                                #endregion

                                break;
                            default:

                                #region Crash

                                if (isGw2 && buffer[0] == '#')
                                {
                                    timeout = 0;

                                    var result = Find(handle, classCallback, textCallback);
                                    if (result != null)
                                    {
                                        CrashReason reason = CrashReason.Unknown;

                                        string text = result.Text;
                                        int i = text.IndexOf("Assertion:", 0, 100);
                                        if (i != -1)
                                        {
                                            int j = text.IndexOf('\n', i);
                                            if (text.IndexOf("Is your archive up to date?", i, j - i, StringComparison.OrdinalIgnoreCase) != -1
                                                || text.IndexOf("Client needs to be patched", i, j - i, StringComparison.OrdinalIgnoreCase) != -1)
                                            {
                                                reason = CrashReason.PatchRequired;
                                            }
                                        }

                                        if (WindowCrashed != null)
                                            WindowCrashed(this, reason);
                                    }
                                    return;
                                }

                                #endregion

                                break;
                        }
                    }
                    while (this.watcher != null);
                }
                finally
                {
                    //abort = true;
                    if (fwatcher != null)
                    {
                        fwatcher.EnableRaisingEvents = false;
                        fwatcher.Dispose();
                    }

                    System.Windows.Forms.Application.ThreadExit -= onThreadExit;

                    lock (this)
                    {
                        this.Account.Process.Exited -= onExit;

                        using (wevent) { }

                        var _weventwaiter = weventwaiter;
                        if (_weventwaiter != null)
                        {
                            weventwaiter = null;
                            _weventwaiter.Dispose();
                        }
                    }

                    if (authWatch)
                    {
                        watchers.AuthWatcher.Remove(this.Account);
                    }
                }
            }

            private async void WatchLogin(string path, Func<bool> onContinue, Action<int> onComplete)
            {
                var b = File.Exists(path);
                var lastWrite = DateTime.MinValue;
                var start = Environment.TickCount;

                if (b)
                {
                    try
                    {
                        lastWrite = File.GetLastWriteTimeUtc(path);
                    }
                    catch 
                    {
                        return;
                    }
                }

                while (true)
                {
                    await Task.Delay(1000);

                    if (b)
                    {
                        try
                        {
                            if (File.GetLastWriteTimeUtc(path) != lastWrite)
                            {
                                onComplete(Environment.TickCount - start);
                                return;
                            }
                        }
                        catch
                        {
                            return;
                        }
                    }
                    else
                    {
                        if (File.Exists(path))
                        {
                            onComplete(Environment.TickCount - start);
                            return;
                        }
                    }

                    if (!onContinue())
                        return;
                }
            }

            /// <summary>
            /// Finds the specified module
            /// </summary>
            /// <param name="module">Module to find</param>
            /// <param name="limit">Timeout in milliseconds; 0 to run once, -1 to run until found</param>
            /// <returns>True if found</returns>
            private bool FindModule(string module, int limit = 0)
            {
                var start = limit > 0 ? Environment.TickCount : 0;

                while (true)
                {
                    try
                    {
                        var pm = process.Modules;
                        for (var i = pm.Count - 1; i >= 0; --i)
                        {
                            if (module.Equals(pm[i].ModuleName, StringComparison.OrdinalIgnoreCase))
                                return true;
                        }
                    }
                    catch (System.ComponentModel.Win32Exception e)
                    {
                        switch (e.NativeErrorCode)
                        {
                            case 5:     //access denied
                            case 299:   //32-bit can't read 64-bit

                                return false;
                        }
                    }
                    catch { }

                    if (limit == 0 || limit > 0 && Environment.TickCount - start > limit || process.WaitForExit(500))
                        break;

                    process.Refresh();
                }

                return false;
            }

            /// <summary>
            /// Finds the specified modules
            /// </summary>
            /// <param name="modules">Modules to find</param>
            /// <param name="all">True to find all of the specified modules, otherwise only 1</param>
            /// <param name="limit">Timeout in milliseconds; 0 to run once, -1 to run until found</param>
            /// <returns>True if found</returns>
            private bool FindModules(string[] modules, bool all, int limit = 0)
            {
                var start = limit > 0 ? Environment.TickCount : 0;

                while (true)
                {
                    try
                    {
                        var pm = process.Modules;
                        var count = modules.Length;

                        for (var i = pm.Count - 1; i >= 0; --i)
                        {
                            var n = pm[i].ModuleName;
                            for (var j = modules.Length - 1; j >= 0; --j)
                            {
                                if (modules[j].Equals(n, StringComparison.OrdinalIgnoreCase))
                                {
                                    if (all)
                                    {
                                        if (--count == 0)
                                            return true;
                                    }
                                    else
                                    {
                                        return true;
                                    }
                                }
                            }
                        }
                    }
                    catch (System.ComponentModel.Win32Exception e)
                    {
                        switch (e.NativeErrorCode)
                        {
                            case 5:     //access denied
                            case 299:   //32-bit can't read 64-bit

                                return false;
                        }
                    }
                    catch { }

                    if (limit == 0 || limit > 0 && Environment.TickCount - start > limit || process.WaitForExit(500))
                        break;

                    process.Refresh();
                }

                return false;
            }

            /// <summary>
            /// Finds the specified modules
            /// </summary>
            /// <param name="pi">Process info</param>
            /// <param name="modules">Modules to find</param>
            /// <param name="all">True to find all of the specified modules, otherwise only 1</param>
            /// <param name="limit">Timeout in milliseconds; 0 to run once, -1 to run until found</param>
            /// <returns>True if found</returns>
            private bool FindModules(Windows.ProcessInfo pi, string[] modules, bool all, int limit = 0)
            {
                if (pi == null)
                {
                    return FindModules(modules, all, limit);
                }

                var start = limit > 0 ? Environment.TickCount : 0;
                var hasRead = false;

                while (true)
                {
                    try
                    {
                        var pm = pi.GetModules();
                        var count = modules.Length;

                        if (pm.Length > 0)
                            hasRead = true;

                        for (var i = pm.Length - 1; i >= 0; --i)
                        {
                            var n = Path.GetFileName(pm[i]);
                            for (var j = modules.Length - 1; j >= 0; --j)
                            {
                                if (modules[j].Equals(n, StringComparison.OrdinalIgnoreCase))
                                {
                                    if (all)
                                    {
                                        if (--count == 0)
                                            return true;
                                    }
                                    else
                                    {
                                        return true;
                                    }
                                }
                            }
                        }
                    }
                    catch (System.ComponentModel.Win32Exception e)
                    {
                        switch (e.NativeErrorCode)
                        {
                            case 5:     //access denied
                            case 299:   //32-bit can't read 64-bit

                                return false;
                        }
                    }
                    catch { }

                    if (!hasRead && FindModules(modules, all, 0))
                        return true;

                    if (limit == 0 || limit > 0 && Environment.TickCount - start > limit || process.WaitForExit(500))
                        break;
                }

                return false;
            }

            private bool WaitForFonts(IntPtr window, Process parent, Process[] processes)
            {
                if (!Util.Users.IsCurrentUser(Account.Settings.WindowsAccount))
                {
                    try
                    {
                        var username = Util.Users.GetUserName(Account.Settings.WindowsAccount);
                        var password = Security.Credentials.GetPassword(username);

                        using (Security.Impersonation.Impersonate(username, password))
                        {
                            return _WaitForFonts(window, parent, processes);
                        }
                    }
                    catch (NotSupportedException)
                    {
                        throw;
                    }
                    catch (Exception e)
                    {
                        Util.Logging.Log(e);
                    }
                    return false;
                }
                else
                {
                    return _WaitForFonts(window, parent, processes);
                }
            }

            private bool _WaitForFonts(IntPtr window, Process parent, Process[] processes)
            {
                var pids = new UIntPtr[processes.Length];

                for (var i = 0; i < processes.Length; i++)
                {
                    pids[i] = (UIntPtr)processes[i].Id;
                }

                var limit = DateTime.UtcNow.AddMinutes(3);

                do
                {
                    int counter = 0;

                    var h = Windows.Win32Handles.GetHandle(pids, Windows.Win32Handles.HandleType.File, new Func<Windows.Win32Handles.IObject, Windows.Win32Handles.CallbackResponse>(
                        delegate(Windows.Win32Handles.IObject o)
                        {
                            ++counter;

                            if (o.Name.Length > 3 && o.Name[o.Name.Length - 4] == '.')
                            {
                                switch (o.Name.Substring(o.Name.Length - 3).ToLower())
                                {
                                    case "ttf":
                                    case "otf":
                                    case "fnt":

                                        return Windows.Win32Handles.CallbackResponse.Return;
                                }
                            }

                            return Windows.Win32Handles.CallbackResponse.Continue;
                        }));

                    if (h != null)
                        return true;

                    if (counter == 0)
                    {
                        throw new NotSupportedException();
                    }

                    for (var i = 1; i < processes.Length; i++)
                    {
                        try
                        {
                            if (processes[i].HasExited)
                            {
                                return false;
                            }
                        }
                        catch { }
                    }

                    this.process.Refresh();
                    if (window != Windows.FindWindow.FindMainWindow(this.process))
                    {
                        return false;
                    }
                }
                while (DateTime.UtcNow < limit && !processes[0].WaitForExit(500));

                return false;
            }

            private bool WaitForExit(Process p, int delay, IntPtr window, Func<bool> onContinue, out int result)
            {
                if (p != null && p.WaitForExit(delay))
                {
                    result = 0;
                    return true;
                }

                this.process.Refresh();
                if (window != Windows.FindWindow.FindMainWindow(this.process) || onContinue != null && !onContinue())
                {
                    result = -1;
                    return true;
                }

                result = 0;
                return false;
            }

            private CoherentWatcher.EventType WaitForCoherentEvent(IntPtr window, CoherentWatcher cw, Func<bool> onContinue = null)
            {
                var e = CoherentWatcher.EventType.None;

                do
                {
                    var ev = cw.GetNextEvent();

                    switch (ev)
                    {
                        case CoherentWatcher.EventType.Error:
                        case CoherentWatcher.EventType.Exited:

                            return ev;

                        case CoherentWatcher.EventType.None:

                            if (e != ev)
                                return e;
                            
                            int w;
                            if (WaitForExit(this.process, 0, window, null, out w))
                                return CoherentWatcher.EventType.Error;
                            if (onContinue != null && !onContinue())
                                return CoherentWatcher.EventType.None;

                            break;
                    }

                    e = ev;
                }
                while (true);
            }

            private CoherentWatcher WaitForCoherentHost(IntPtr window, out int result, int pidChild = 0, Func<bool> onContinue = null)
            {
                if (!Util.Users.IsCurrentUser(Account.Settings.WindowsAccount))
                {
                    try
                    {
                        var username = Util.Users.GetUserName(Account.Settings.WindowsAccount);
                        var password = Security.Credentials.GetPassword(username);

                        using (Security.Impersonation.Impersonate(username, password))
                        {
                            return _WaitForCoherentHost(window, out result, pidChild, onContinue);
                        }
                    }
                    catch (NotSupportedException)
                    {
                        throw;
                    }
                    catch (Exception e)
                    {
                        Util.Logging.Log(e);
                    }
                    result = 0;
                    return null;
                }
                else
                {
                    return _WaitForCoherentHost(window, out result, pidChild, onContinue);
                }
            }

            private CoherentWatcher _WaitForCoherentHost(IntPtr window, out int result, int pidChild, Func<bool> onContinue)
            {
                var limit = DateTime.UtcNow.AddMinutes(3);
                result = 0;

                //note the CoherentUI host process can change after it's initially started - if something goes wrong, it will start a new process until it's successful

                while (true)
                {
                    if (pidChild == 0)
                    {
                        do
                        {
                            if ((pidChild = GetChildProcess(process.Id)) > 0)
                            {
                                break;
                            }

                            if (WaitForExit(this.process, 500, window, onContinue, out result))
                                return null;
                        }
                        while (DateTime.UtcNow < limit);
                    }

                    if (pidChild > 0)
                    {
                        Process p;
                        try
                        {
                            p = Process.GetProcessById(pidChild);
                        }
                        catch
                        {
                            p = null;
                        }

                        var errors = 0;
                        var pids = new UIntPtr[] 
                        { 
                            (UIntPtr)pidChild
                        };

                        CoherentWatcher.FileMap m = null;

                        do
                        {
                            try
                            {
                                m = CoherentWatcher.Find(pids);
                                if (m != null)
                                {
                                    using (p) { }
                                    return new CoherentWatcher(m);
                                }
                            }
                            catch
                            {
                                if (++errors == 5)
                                    throw;
                            }

                            if (WaitForExit(this.process, 500, window, onContinue, out result))
                                return null;

                            if (p == null)
                            {
                                break;
                            }
                            else
                            {
                                try
                                {
                                    if (p.HasExited)
                                        throw new Exception();
                                }
                                catch
                                {
                                    p.Dispose();
                                    p = null;
                                    break;
                                }
                            }
                        }
                        while (DateTime.UtcNow < limit);

                        if (p == null)
                        {
                            pidChild = 0;
                            continue;
                        }
                    }

                    return null;
                }
            }

            /// <summary>
            /// Returns a CoherentUI child process
            /// </summary>
            /// <param name="window">Main window handle</param>
            /// <param name="pidChild">The main CoherentUI process to start from, or 0 to find it</param>
            /// <param name="childCount">Number of child processes to search for; launcher has 1 main process with 2 child processes, game has 1 main with 1 child until used</param>
            /// <param name="onContinue">Loop callback, return false to interrupt</param>
            /// <returns>Main CoherentUI process ID</returns>
            private int FindCoherentChildProcesses(IntPtr window, int pidChild, int childCount = 1, Func<bool> onContinue = null)
            {
                byte r = 1;
                sbyte once = 0;
                bool skipFiles = false;

                do
                {
                    Util.Logging.LogEvent(Account.Settings, "Waiting for CoherentUI (" + r + ")");

                    var limit = DateTime.UtcNow.AddSeconds(30 * r);

                    if (pidChild == 0)
                    {
                        do
                        {
                            if ((pidChild = GetChildProcess(process.Id)) > 0)
                            {
                                break;
                            }

                            int w;
                            if (WaitForExit(this.process, 500 * r, window, onContinue, out w))
                                return w;
                        }
                        while (DateTime.UtcNow < limit);
                    }

                    if (pidChild > 0)
                    {
                        var cids = new int[childCount];
                        var _childCount = 0;

                        Util.Logging.LogEvent(Account.Settings, "CoherentUI found (" + pidChild + "), waiting for sub processes (" + childCount + ")");

                        try
                        {
                            using (var child = Process.GetProcessById(pidChild))
                            {
                                limit = DateTime.UtcNow.AddSeconds(30 * r);

                                if (once == 0)
                                {
                                    try
                                    {
                                        if (DateTime.Now.Subtract(child.StartTime).TotalMinutes >= 1)
                                        {
                                            once = -1;
                                        }
                                        else
                                        {
                                            once = 1;
                                        }
                                    }
                                    catch
                                    {
                                        once = -1;
                                    }
                                }

                                do
                                {
                                    _childCount = GetChildProcesses(child, cids);

                                    if (_childCount == childCount)
                                    {
                                        break;
                                    }
                                    //else if (_childCount > 0)
                                    //{
                                    //    break;
                                    //}
                                    else if (once == -1)
                                    {
                                        return pidChild;
                                    }

                                    int w;
                                    if (WaitForExit(child, 500 * r, window, onContinue, out w))
                                        return w;
                                }
                                while (DateTime.UtcNow < limit);
                            }

                            Util.Logging.LogEvent(Account.Settings, "Found " + _childCount + " CoherentUI sub processes");

                            if (_childCount > 0)
                            {
                                var processes = new Process[_childCount];
                                var oldest = 0;

                                if (_childCount > 1)
                                {
                                    for (var i = 0; i < _childCount; ++i)
                                    {
                                        var p = processes[i] = Process.GetProcessById(cids[i]);
                                        if (i != 0 && p.StartTime < processes[oldest].StartTime)
                                        {
                                            oldest = i;
                                        }
                                    }

                                    if (!skipFiles)
                                    {
                                        if (!Settings.Tweaks.Launcher.HasValue || Settings.Tweaks.Launcher.Value.WaitForCoherentLoaded)
                                        {
                                            Util.Logging.LogEvent(Account.Settings, "Waiting for CoherentUI to load files");

                                            try
                                            {
                                                if (WaitForFonts(window, process, processes))
                                                {
                                                    Util.Logging.LogEvent(Account.Settings, "Waiting for CoherentUI to load files... OK");
                                                    return pidChild;
                                                }
                                                else
                                                {
                                                    Util.Logging.LogEvent(Account.Settings, "Waiting for CoherentUI to load files... FAILED");
                                                }
                                            }
                                            catch (NotSupportedException)
                                            {
                                                Util.Logging.LogEvent(Account.Settings, "Waiting for CoherentUI to load files was not supported");
                                                skipFiles = true;
                                            }
                                        }
                                        else
                                        {
                                            skipFiles = true;
                                            Util.Logging.LogEvent(Account.Settings, "Skipping wait for CoherentUI to load files due to settings");
                                            if (!Settings.Tweaks.Launcher.Value.WaitForCoherentMemory)
                                                return pidChild;
                                        }
                                    }
                                }
                                else
                                {
                                    processes[0] = Process.GetProcessById(cids[0]);
                                }

                                //this should only be used as a backup - first priority is checking for fonts (when detecting the launcher) or modules (when detecting character select)

                                try
                                {
                                    if (!Settings.Tweaks.Launcher.HasValue || Settings.Tweaks.Launcher.Value.WaitForCoherentMemory)
                                    {
                                        Util.Logging.LogEvent(Account.Settings, "Waiting for CoherentUI to load");

                                        long mem = 0;
                                        byte counter = 0;
                                        byte check = 0;

                                        limit = DateTime.UtcNow.AddSeconds(300);

                                        do
                                        {
                                            if (++check == 5)
                                            {
                                                int w;
                                                if (WaitForExit(null, 0, window, onContinue, out w))
                                                    return w;
                                                check = 0;
                                            }

                                            long _mem = 0;

                                            for (var i = processes.Length - 1; i >= 0; --i)
                                            {
                                                var p = processes[i];

                                                p.Refresh();

                                                try
                                                {
                                                    if (i == oldest)
                                                        _mem += p.PrivateMemorySize64;
                                                }
                                                catch { }
                                            }

                                            if (_mem > mem)
                                            {
                                                mem = _mem + 102400;
                                                counter = 0;
                                            }
                                            else if (++counter > 10)
                                            {
                                                //CoherentUI can stall and never fully load

                                                //if (mem < 40000000)
                                                //{
                                                //    counter = 0;
                                                //    continue;
                                                //}

                                                break;
                                            }
                                        }
                                        while (DateTime.UtcNow < limit && !processes[oldest].WaitForExit(100));
                                    }
                                    else
                                    {
                                        Util.Logging.LogEvent(Account.Settings, "Skipping wait for CoherentUI to load due to settings");

                                        int w;
                                        if (WaitForExit(null, 0, window, onContinue, out w))
                                            return w;
                                    }

                                    if (_childCount == childCount)
                                    {
                                        return pidChild;
                                    }
                                    else if (r > 1)
                                    {
                                        return pidChild;
                                    }
                                }
                                finally
                                {
                                    foreach (var p in processes)
                                    {
                                        using (p) { }
                                    }
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            Util.Logging.Log(ex);
                            Util.Logging.LogEvent(Account.Settings, "Waiting for CoherentUI failed: " + ex.Message);
                        }
                    }
                    else
                    {
                        Util.Logging.LogEvent(Account.Settings, "CoherentUI not found");
                    }

                    pidChild = 0;
                }
                while (++r < 3);

                Util.Logging.Log("Unable to find CoherentUI for " + window);

                return 0;
            }

            private int FindCoherentChildProcess(IntPtr window, int pidChild, Func<bool> onContinue = null)
            {
                byte r = 1;
                sbyte once = 0;

                do
                {
                    var limit = DateTime.UtcNow.AddSeconds(30 * r);

                    if (pidChild == 0)
                    {
                        do
                        {
                            if ((pidChild = GetChildProcess(process.Id)) > 0)
                            {
                                break;
                            }

                            int w;
                            if (WaitForExit(this.process, 500 * r, window, onContinue, out w))
                                return w;
                        }
                        while (DateTime.UtcNow < limit);
                    }

                    if (pidChild > 0)
                    {
                        try
                        {
                            using (var child = Process.GetProcessById(pidChild))
                            {
                                limit = DateTime.UtcNow.AddSeconds(30 * r);
                                var pid = pidChild;

                                if (once == 0)
                                {
                                    try
                                    {
                                        if (DateTime.Now.Subtract(child.StartTime).TotalMinutes >= 1)
                                        {
                                            once = -1;
                                        }
                                        else
                                        {
                                            once = 1;
                                        }
                                    }
                                    catch
                                    {
                                        once = -1;
                                    }
                                }

                                do
                                {
                                    if ((pidChild = GetChildProcess(pid)) > 0)
                                    {
                                        break;
                                    }

                                    if (once == -1)
                                    {
                                        return pid;
                                    }

                                    int w;
                                    if (WaitForExit(child, 500 * r, window, onContinue, out w))
                                        return w;
                                }
                                while (DateTime.UtcNow < limit);
                            }

                            if (pidChild > 0)
                            {
                                using (var child = Process.GetProcessById(pidChild))
                                {
                                    long mem = 0;
                                    byte counter = 0;
                                    byte check = 0;
                                    limit = DateTime.UtcNow.AddSeconds(60);

                                    do
                                    {
                                        if (++check == 5)
                                        {
                                            int w;
                                            if (WaitForExit(null, 0, window, onContinue, out w))
                                                return w;
                                            check = 0;
                                        }
                                        if (mem > 0)
                                            child.Refresh();
                                        var _mem = child.PeakWorkingSet64;
                                        if (_mem > mem)
                                        {
                                            mem = _mem;
                                            counter = 0;
                                        }
                                        else if (++counter > 2)
                                            break;
                                    }
                                    while (DateTime.UtcNow < limit && !child.WaitForExit(100));
                                }

                                return pidChild;
                            }
                        }
                        catch (Exception ex)
                        {
                            Util.Logging.Log(ex);
                        }
                    }

                    pidChild = 0;
                }
                while (++r < 3);

                Util.Logging.Log("Unable to find CoherentUI for " + window);

                return 0;
            }

            /// <summary>
            /// Attempts to set the volume once available
            /// </summary>
            public async void SetVolume(float percent)
            {
                var t = DateTime.UtcNow.AddMilliseconds(30000);

                do
                {
                    int processId;
                    try
                    {
                        processId = this.process.Id;
                        if (this.process.HasExited)
                            return;
                    }
                    catch
                    {
                        return;
                    }

                    var r = await Task.Run<bool>(
                        delegate
                        {
                            try
                            {
                                return Windows.Volume.SetVolume(processId, percent);
                            }
                            catch
                            {
                                return false;
                            }
                        });

                    if (r)
                        break;
                    else
                        await Task.Delay(1000);
                }
                while (DateTime.UtcNow < t);
            }

            public static bool SetText(IntPtr window, string text)
            {
                try
                {
                    return NativeMethods.SetWindowText(window, text);
                }
                catch (Exception e)
                {
                    Util.Logging.Log(e);
                }

                return false;
            }

            /// <summary>
            /// Sets the bounds of the window and watches for it to revert back to its original bounds.
            /// If reverted, the bounds are set again.
            /// </summary>
            public void SetBounds(Settings.IAccount account, Process p, IntPtr window, System.Drawing.Rectangle bounds, int timeout, WindowEvents.Events events, Action<IntPtr> onChanged, Action<IntPtr> onComplete)
            {
                SetBounds(account, p, window, bounds, timeout, events, onChanged, onComplete, false, false);
            }

            /// <summary>
            /// Sets the bounds of the window and watches for it to revert back to its original bounds.
            /// If reverted, the bounds are set again.
            /// </summary>
            public static void SetBounds(Settings.IAccount account, Process p, IntPtr window, System.Drawing.Rectangle bounds, int timeout, WindowEvents.Events events, Action<IntPtr> onChanged, Action<IntPtr> onComplete, bool preventSizing, bool topMost)
            {
                watchers.BoundsWatcher.SetBounds(account, p, window, bounds, timeout, events, onChanged, onComplete, preventSizing, topMost);
            }

            public static IntPtr FindDxWindow(Process process)
            {
                var handle = Windows.FindWindow.FindMainWindow(process);
                if (handle != IntPtr.Zero)
                {
                    //ensure it's the main game window
                    var sb = new StringBuilder(DX_WINDOW_CLASSNAME_LENGTH + 1);
                    string s;
                    if (NativeMethods.GetClassName(handle, sb, sb.Capacity + 1) != DX_WINDOW_CLASSNAME_LENGTH || !((s = sb.ToString()).Equals(DX_WINDOW_CLASSNAME_DX11BETA) || s.Equals(DX_WINDOW_CLASSNAME)))
                    {
                        handle = Windows.FindWindow.Find(process.Id, new string[] { DX_WINDOW_CLASSNAME_DX11BETA, DX_WINDOW_CLASSNAME }, sb);
                    }
                }
                return handle;
            }

            public static bool SetBounds(Settings.IAccount account, Process process, System.Drawing.Rectangle bounds)
            {
                var handle = FindDxWindow(process);
                if (handle == IntPtr.Zero)
                    return false;
                return SetBounds(account, process, handle, bounds);
            }

            public static bool SetBounds(Settings.IAccount account, Process process, IntPtr handle, System.Drawing.Rectangle bounds)
            {
                try
                {
                    watchers.BoundsWatcher.Cancel(handle);

                    var placement = new WINDOWPLACEMENT();
                    if (!NativeMethods.GetWindowPlacement(handle, ref placement))
                        return false;

                    bounds = Util.ScreenUtil.ToDesktopBounds(bounds);

                    if (!placement.rcNormalPosition.Equals(bounds))
                    {
                        //placement.flags = WindowPlacementFlags.WPF_ASYNCWINDOWPLACEMENT;
                        placement.rcNormalPosition = RECT.FromRectangle(bounds);
                        if (!NativeMethods.SetWindowPlacement(handle, ref placement))
                            return false;

                        if (AccountWindowEvent != null)
                        {
                            try
                            {
                                AccountWindowEvent(account, new AccountWindowEventEventArgs(AccountWindowEventEventArgs.EventType.BoundsChanged, process, handle));
                            }
                            catch { }
                        }
                    }

                    return true;
                }
                catch (Exception e)
                {
                    Util.Logging.Log(e);
                }

                return false;
            }

            public static void SetIcons(IntPtr window, Tools.Icons icons)
            {
                //var h = NativeMethods.SendMessage(window, WindowMessages.WM_GETICON, IntPtr.Zero, IntPtr.Zero);

                //if (h == icons.Small.Handle)
                //{
                //    //force update
                //    NativeMethods.PostMessage(window, WindowMessages.WM_SETICON, (IntPtr)0, IntPtr.Zero);
                //    NativeMethods.PostMessage(window, WindowMessages.WM_SETICON, (IntPtr)1, IntPtr.Zero);
                //}

                NativeMethods.PostMessage(window, WindowMessages.WM_SETICON, (IntPtr)0, icons.Small.Handle);
                NativeMethods.PostMessage(window, WindowMessages.WM_SETICON, (IntPtr)1, icons.Big.Handle);
            }


            /// <summary>
            /// Sets the window to be top most with other windows on top of it
            /// </summary>
            /// <param name="handle">The main window being set to top most</param>
            /// <param name="windows">Other windows to display on top (last will be the top most window)</param>
            /// <returns>True if successful</returns>
            public static bool SetTopMost(IntPtr handle, ICollection<IntPtr> windows)
            {
                try
                {
                    IntPtr d;

                    d = NativeMethods.BeginDeferWindowPos(1 + windows.Count);
                    if (d == IntPtr.Zero)
                        return false;

                    var hwnd = (IntPtr)WindowZOrder.HWND_TOPMOST;
                    var flags = (uint)(SetWindowPosFlags.SWP_NOMOVE | SetWindowPosFlags.SWP_NOSIZE);

                    d = NativeMethods.DeferWindowPos(d, handle, hwnd, 0, 0, 0, 0, flags);
                    if (d == IntPtr.Zero)
                        return false;
                    foreach (var w in windows)
                    {
                        d = NativeMethods.DeferWindowPos(d, w, hwnd, 0, 0, 0, 0, flags);
                        if (d == IntPtr.Zero)
                            return false;
                    }

                    return NativeMethods.EndDeferWindowPos(d);
                }
                catch (Exception ex)
                {
                    Util.Logging.Log(ex);
                }

                return false;
            }

            public static bool SetTopMost(IntPtr handle)
            {
                try
                {
                    return NativeMethods.SetWindowPos(handle, (IntPtr)WindowZOrder.HWND_TOPMOST, 0, 0, 0, 0, SetWindowPosFlags.SWP_NOMOVE | SetWindowPosFlags.SWP_NOSIZE | SetWindowPosFlags.SWP_NOACTIVATE);
                }
                catch (Exception ex)
                {
                    Util.Logging.Log(ex);
                }
                return false;
            }

            public static bool HasTopMost(IntPtr handle)
            {
                return Windows.WindowLong.HasValue(handle, GWL.GWL_EXSTYLE, WindowStyle.WS_EX_TOPMOST);
            }

            public static void SetTopMost(Process process)
            {
                try
                {
                    SetTopMost(Windows.FindWindow.FindMainWindow(process));
                }
                catch (Exception ex)
                {
                    Util.Logging.Log(ex);
                }
            }

            public HiddenWindow CreateHidden()
            {
                return new HiddenWindow(this.process.Id, this.ProcessOptions);
            }
        }
    }
}
