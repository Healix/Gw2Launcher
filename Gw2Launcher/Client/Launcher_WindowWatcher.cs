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
                    LauncherCefLoginEvent,

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
                            events.MoveSizeBegin += OnMoveSizeBegin;
                            events.MinimizeStart += OnMinimizeStart;
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

                    private void OnMoveSizeBegin(object sender, WindowEvents.WindowEventsEventArgs e)
                    {
                        if (!abort)
                        {
                            abort = true;

                            if (Util.Logging.Enabled)
                            {
                                Util.Logging.LogEvent(account, "Window was manually resized, aborting window bounds");
                            }
                        }
                    }

                    private void OnMinimizeStart(object sender, WindowEvents.WindowEventsEventArgs e)
                    {
                        if (!abort)
                        {
                            abort = true;

                            if (Util.Logging.Enabled)
                            {
                                Util.Logging.LogEvent(account, "Window was minimized, aborting window bounds");
                            }
                        }
                    }

                    public void Dispose()
                    {
                        abort = true;

                        if (events != null)
                        {
                            events.MoveSizeBegin -= OnMoveSizeBegin;
                            events.MinimizeStart -= OnMinimizeStart;
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
                public event EventHandler Disposed;

                private bool wasLayered;
                private Windows.DebugEvents.IDebugEventsToken dt;

                public HiddenWindow(int pid, ProcessOptions options = null)
                {
                    dt = Windows.DebugEvents.Instance.Add(pid, options != null ? options.UserName : null, options != null ? options.Password : null);
                }

                ~HiddenWindow()
                {
                    Dispose();
                }

                public void Hide(IntPtr handle)
                {
                    this.Handle = handle;

                    //NativeMethods.ShowWindow(handle, ShowWindowCommands.ForceMinimize);

                    Task.Run(new Action(delegate
                    {
                        wasLayered = Windows.WindowLong.Add(handle, GWL.GWL_EXSTYLE, WindowStyle.WS_EX_LAYERED) == IntPtr.Zero;
                        NativeMethods.SetLayeredWindowAttributes(handle, 0, 0, LayeredWindowFlags.LWA_ALPHA);
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

                        //force reset
                        Windows.WindowLong.Remove(h, GWL.GWL_EXSTYLE, WindowStyle.WS_EX_LAYERED);
                        if (wasLayered)
                            Windows.WindowLong.Add(h, GWL.GWL_EXSTYLE, WindowStyle.WS_EX_LAYERED);

                    }

                    if (Disposed != null)
                    {
                        Disposed(this, EventArgs.Empty);
                        Disposed = null;
                    }
                }
            }

            struct ProcessInfo : IDisposable
            {
                public Process Process;
                private object info;

                public ProcessInfo(Process process, Windows.ProcessInfo info)
                {
                    this.Process = process;
                    this.info = info;
                }

                public ProcessInfo(Process process, string commandLine)
                {
                    this.Process = process;
                    this.info = commandLine;
                }

                public string GetCommandLine()
                {
                    if (info is string)
                    {
                        return (string)info;
                    }
                    else
                    {
                        return ((Windows.ProcessInfo)info).GetCommandLine();
                    }
                }

                public void Dispose()
                {
                    Process.Dispose();
                }
            }

            struct ModuleInfo
            {
                public int Index;
                public int Timestamp;
            }

            struct WaitForFontsResult
            {
                public int PID;
            }

            public event EventHandler<WindowChangedEventArgs> WindowChanged;
            public event EventHandler<CrashReason> WindowCrashed;
            public event EventHandler<int> LoginComplete;
            public event EventHandler<TimeoutEventArgs> Timeout;
            public event EventHandler<Process> DxHostProcess;
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

            public enum HostType
            {
                Unknown,
                CoherentUI,
                CEF,
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

            /// <summary>
            /// Changes the watched process, abandoning the current watcher thread
            /// </summary>
            public void SetProcess(Process p)
            {
                process = p;
                if (watcher != null && watcher.IsCompleted)
                    watcher.Dispose();
                watcher = null;
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
            private IEnumerable<ProcessInfo> EnumerateChildProcesses(Process parent)
            {
                var pid = parent.Id;
                var startTime = parent.StartTime;

                if (canReadMemory)
                {
                    var processes = Process.GetProcesses();

                    using (var pi = new Windows.ProcessInfo())
                    {
                        foreach (var p in processes)
                        {
                            bool b;

                            try
                            {
                                b = pi.Open(p.Id) && pi.GetParent() == pid && p.StartTime >= startTime;
                            }
                            catch (Exception e)
                            {
                                b = false;
                                Util.Logging.Log(e);
                            }

                            if (b)
                            {
                                yield return new ProcessInfo(p, pi);
                            }
                            else
                            {
                                p.Dispose();
                            }
                        }
                    }
                }
                else
                {
                    using (var searcher = new ManagementObjectSearcher("SELECT ProcessId, CommandLine FROM Win32_Process WHERE ParentProcessID=" + pid))
                    {
                        using (var results = searcher.Get())
                        {
                            foreach (ManagementObject o in results)
                            {
                                Process p = null;
                                bool b;

                                try
                                {
                                    p = Process.GetProcessById((int)(uint)o["ProcessId"]);
                                    b = p.StartTime >= startTime;
                                }
                                catch
                                {
                                    b = false;
                                }

                                if (b)
                                {
                                    yield return new ProcessInfo(p, o["CommandLine"] as string);
                                }
                                else if (p != null)
                                {
                                    p.Dispose();
                                }
                            }
                        }
                    }
                }
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

            private Process GetHostProcess(Process parent)
            {
                foreach (var p in EnumerateChildProcesses(parent))
                {
                    try
                    {
                        if (GetHostType(p.Process.ProcessName) != HostType.Unknown)
                        {
                            return p.Process;
                        }
                    }
                    catch { }

                    p.Dispose();
                }

                return null;
            }

            private int GetRendererProcess(Process parent, Process[] result)
            {
                int i = 0;

                foreach (var p in EnumerateChildProcesses(parent))
                {
                    try
                    {
                        if (GetHostType(p.Process.ProcessName) != HostType.Unknown)
                        {
                            if (p.GetCommandLine().IndexOf("-type=renderer") != -1)
                            {
                                result[i] = p.Process;

                                if (++i == result.Length)
                                    return i;

                                continue;
                            }
                        }
                    }
                    catch { }

                    p.Dispose();
                }

                return i;

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
                    if (Util.Logging.Enabled)
                    {
                        Util.Logging.LogEvent(this.Account.Settings, "Error while watching window", e);
                    }

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

            private void WatchUpdate()
            {
                var exited = false;
                var changed = false;
                var process = this.process;
                var path = ((Settings.IGw2Account)this.Account.Settings).DatFile.Path;

                EventHandler<Process> onChanged = delegate(object o, Process p)
                {
                    changed = true;
                };

                this.Account.Process.Changed += onChanged;
                //this.Account.Exited

                try
                {
                    var t = Environment.TickCount;
                    var since = t;
                    var counter = 0;

                    while (true)
                    {
                        if (exited)
                        {
                            if (changed)
                            {
                                changed = false;
                                process = this.Account.Process.Process;
                                exited = process != null;
                                counter = 0;
                            }

                            Thread.Sleep(500);
                        }
                        else
                        {
                            try
                            {
                                if (process.WaitForExit(500))
                                {
                                    exited = true;
                                    continue;
                                }
                            }
                            catch { }

                            if (Environment.TickCount - t > 3000)
                            {
                                if (Util.FileUtil.IsFileLocked(path))
                                {
                                    counter = 0;
                                }
                                else
                                {
                                    if (counter == 0)
                                    {
                                        since = Environment.TickCount;
                                    }
                                    else if (Environment.TickCount - since > 10000)
                                    {
                                        //Local.dat has been unlocked for over 10s, GW2 is probably stuck
                                    }

                                    ++counter;
                                }

                                t = Environment.TickCount;
                            }
                        }
                    }
                }
                finally
                {
                    this.Account.Process.Changed -= onChanged;
                }
            }

            private void WatchWindow()
            {
                var process = this.process;
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

                var _wasAlreadyStarted = processWasAlreadyStarted && handle != IntPtr.Zero;

                FileSystemWatcher fwatcher = null;
                var authWatch = Settings.AuthenticatorPastingEnabled.Value && isGw2 && this.Account.Settings.TotpKey != null;
                var weventwaiter = new ManualResetEvent(false);
                var hasEvents = false;
                var weventT = WindowChangedEventArgs.EventType.WatcherExited;
                var hProcess = (Settings.HideInitialWindow.Value || Settings.RepaintInitialWindow.Value) ? NativeMethods.OpenProcess(ProcessAccessFlags.SuspendResume, false, process.Id) : IntPtr.Zero;
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

                                                    var suspended = hProcess != IntPtr.Zero && NativeMethods.NtSuspendProcess(hProcess) == IntPtr.Zero;

                                                    try
                                                    {
                                                        var s = weventbuffer.ToString();
                                                        if (s.Equals(DX_WINDOW_CLASSNAME_DX11BETA) || s.Equals(DX_WINDOW_CLASSNAME))
                                                        {
                                                            t = WindowChangedEventArgs.EventType.DxWindowHandleCreated;

                                                            if (suspended && t != weventT)
                                                            {
                                                                weventT = t;

                                                                WindowChanged(this, new WindowChangedEventArgs()
                                                                {
                                                                    Handle = hwnd,
                                                                    Type = t,
                                                                    WasAlreadyStarted = _wasAlreadyStarted,
                                                                });

                                                                break;
                                                            }
                                                        }
                                                    }
                                                    finally
                                                    {
                                                        if (suspended)
                                                        {
                                                            NativeMethods.NtResumeProcess(hProcess);
                                                        }
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

                                    //int pidChild = 0;
                                    Process host = null;
                                    bool hasVolume = false;
                                    DateTime limit;

                                    using (var volumeControl = new Windows.Volume.VolumeControl(process.Id))
                                    {
                                        if (Util.Logging.Enabled && !volumeControl.IsSupported)
                                        {
                                            Util.Logging.LogEvent(null, "Volumne control is not supported");
                                        }

                                        if (wasAlreadyStarted && ((hasVolume = volumeControl.Query()) || isGw2 && (host = GetHostProcess(process)) != null))
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
                                                    if (hasVolume = volumeControl.Query() || (host = GetHostProcess(process)) != null)
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
                                                        if (hasVolume = volumeControl.Query() || FindModules(process, canRead ? pi : null, modules, false))
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
                                            var canRead = pi.Open(process.Id);
                                            var foundModules = false;
                                            var hostType = HostType.Unknown;

                                            if (Util.Logging.Enabled)
                                            {
                                                Util.Logging.LogEvent(Account.Settings, "Waiting for host or modules to load...");
                                            }

                                            if (host != null)
                                            {
                                                try
                                                {
                                                    hostType = GetHostType(host.ProcessName);
                                                }
                                                catch
                                                {
                                                    host.Dispose();
                                                    host = null;
                                                }
                                            }

                                            if (host == null)
                                            {
                                                host = WaitForHostProcess(process, _handle, out hostType);
                                            }

                                            string[] modules;

                                            if (hostType == HostType.CEF)
                                                modules = new string[] { "icm32.dll" }; //other modules are loaded on launch
                                            else
                                                modules = new string[] { "icm32.dll", "mscms.dll", "USERENV.dll" };

                                            using (host)
                                            {
                                                this.Account.hostType = hostType;

                                                if (DxHostProcess != null && host != null)
                                                {
                                                    DxHostProcess(this, host);
                                                }

                                                limit = DateTime.UtcNow.AddSeconds(Settings.DxTimeout.Value > 0 ? Settings.DxTimeout.Value : 30);

                                                Dictionary<string, ModuleInfo> moduleInfo = null;
                                                var mts = Environment.TickCount;

                                                if (Util.Logging.Enabled)
                                                {
                                                    moduleInfo = new Dictionary<string, ModuleInfo>();
                                                }

                                                WaitForHostProcesses(process, _handle, hostType, hostType == HostType.CoherentUI ? host : process, false, 1,
                                                    delegate
                                                    {
                                                        //alternatively look for loaded modules; icm32.dll is loaded after CoherentUI, but before it's finished loading
                                                        //either way, character select is loaded at this point

                                                        if (FindModules(process, canRead ? pi : null, modules, false))
                                                        {
                                                            foundModules = true;
                                                            return false;
                                                        }

                                                        if (DateTime.UtcNow > limit)
                                                        {
                                                            return false;
                                                        }

                                                        return true;
                                                    });

                                                if (DateTime.UtcNow > limit)
                                                {
                                                    if (Util.Logging.Enabled)
                                                    {
                                                        Util.Logging.LogEvent(Account.Settings, "Waiting for host or modules to load... FAILED");

                                                        var sb = new StringBuilder(1000);

                                                        if (moduleInfo != null)
                                                        {
                                                            foreach (var m in moduleInfo.Keys)
                                                            {
                                                                sb.Append(moduleInfo[m].Index);
                                                                sb.Append(":");
                                                                sb.Append(m);
                                                                sb.Append("(");
                                                                sb.Append(moduleInfo[m].Timestamp - mts);
                                                                sb.Append("), ");
                                                            }
                                                        }
                                                        else
                                                        {
                                                            try
                                                            {
                                                                if (pi != null)
                                                                {
                                                                    foreach (var m in pi.GetModules())
                                                                    {
                                                                        sb.Append(Path.GetFileName(m));
                                                                        sb.Append(", ");
                                                                    }
                                                                }
                                                                else
                                                                {
                                                                    process.Refresh();
                                                                    foreach (ProcessModule m in process.Modules)
                                                                    {
                                                                        sb.Append(m.ModuleName);
                                                                        sb.Append(", ");
                                                                    }
                                                                }
                                                            }
                                                            catch (Exception e)
                                                            {
                                                                sb.Append(e.Message);
                                                            }
                                                        }

                                                        if (sb.Length > 0)
                                                        {
                                                            sb.Length -= 2;
                                                        }

                                                        Util.Logging.LogEvent(Account.Settings, "Modules: " + sb.ToString());
                                                    }

                                                    if (Settings.DxTimeout.Value > 0 && Timeout != null)
                                                    {
                                                        var te = new TimeoutEventArgs(TimeoutEventArgs.TimeoutReason.DxWindow);
                                                        Timeout(this, te);
                                                        if (te.Handled)
                                                            return;
                                                    }
                                                }
                                            }

                                            if (!foundModules && !Settings.IsRunningWine && (DateTime.UtcNow < limit || Settings.DxTimeout.Value == 0))
                                            {
                                                if (Util.Logging.Enabled)
                                                {
                                                    Util.Logging.LogEvent(Account.Settings, "Waiting for modules to load...");
                                                }

                                                //limit = DateTime.UtcNow.AddSeconds(Settings.DxTimeout.Value > 0 ? Settings.DxTimeout.Value : 30);

                                                Dictionary<string, ModuleInfo> moduleInfo = null;
                                                var mts = Environment.TickCount;

                                                if (Util.Logging.Enabled)
                                                {
                                                    moduleInfo = new Dictionary<string, ModuleInfo>();
                                                }

                                                do
                                                {
                                                    if (FindModules(process, canRead ? pi : null, modules, false, 0, moduleInfo))
                                                    {
                                                        if (Util.Logging.Enabled)
                                                        {
                                                            Util.Logging.LogEvent(Account.Settings, "Waiting for modules to load... OK");
                                                        }
                                                        foundModules = true;
                                                        break;
                                                    }

                                                    if (DateTime.UtcNow > limit)
                                                    {
                                                        if (Util.Logging.Enabled)
                                                        {
                                                            Util.Logging.LogEvent(Account.Settings, "Waiting for modules to load... FAILED");

                                                            var sb = new StringBuilder(1000);

                                                            if (moduleInfo != null)
                                                            {
                                                                foreach (var m in moduleInfo.Keys)
                                                                {
                                                                    sb.Append(moduleInfo[m].Index);
                                                                    sb.Append(":");
                                                                    sb.Append(m);
                                                                    sb.Append("(");
                                                                    sb.Append(moduleInfo[m].Timestamp - mts);
                                                                    sb.Append("), ");
                                                                }
                                                            }
                                                            else
                                                            {
                                                                try
                                                                {
                                                                    if (pi != null)
                                                                    {
                                                                        foreach (var m in pi.GetModules())
                                                                        {
                                                                            sb.Append(Path.GetFileName(m));
                                                                            sb.Append(", ");
                                                                        }
                                                                    }
                                                                    else
                                                                    {
                                                                        process.Refresh();
                                                                        foreach (ProcessModule m in process.Modules)
                                                                        {
                                                                            sb.Append(m.ModuleName);
                                                                            sb.Append(", ");
                                                                        }
                                                                    }
                                                                }
                                                                catch (Exception e)
                                                                {
                                                                    sb.Append(e.Message);
                                                                }
                                                            }

                                                            if (sb.Length > 0)
                                                            {
                                                                sb.Length -= 2;
                                                            }

                                                            Util.Logging.LogEvent(Account.Settings, "Modules: " + sb.ToString());
                                                        }

                                                        if (Settings.DxTimeout.Value > 0 && Timeout != null)
                                                        {
                                                            var te = new TimeoutEventArgs(TimeoutEventArgs.TimeoutReason.DxWindow);
                                                            Timeout(this, te);
                                                            if (te.Handled)
                                                                return;
                                                        }
                                                        else
                                                        {
                                                            break;
                                                        }
                                                    }

                                                    int r;
                                                    if (WaitForExit(process, process, 1000, _handle, null, out r))
                                                    {
                                                        break;
                                                    }
                                                }
                                                while (true);
                                            }
                                            else
                                            {
                                                if (Util.Logging.Enabled)
                                                {
                                                    Util.Logging.LogEvent(Account.Settings, "Waiting for host or modules to load... OK");
                                                }
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
                                                if (FindModules(process, canRead ? pi : null, modules, false))
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
                                    var hostType = HostType.Unknown;
                                    CoherentWatcher cw = null;

                                    using (var host = WaitForHostProcess(process, _handle, out hostType))
                                    {
                                        this.Account.hostType = hostType;

                                        if (host == null)
                                            break;

                                        if (!Settings.Tweaks.Launcher.HasValue || (Settings.Tweaks.Launcher.Value.CoherentOptions & (Settings.LauncherTweaks.CoherentFlags.WaitForLoaded | Settings.LauncherTweaks.CoherentFlags.WaitForMemory)) != 0)
                                        {
                                            if (hostType == HostType.CoherentUI)
                                            {
                                                if (!Settings.Tweaks.Launcher.HasValue || Settings.Tweaks.Launcher.Value.WaitForCoherentLoaded)
                                                {
                                                    try
                                                    {
                                                        cw = WaitForCoherentHost(process, _handle, out r, host);
                                                        if (cw == null && r == -1)
                                                            break;
                                                    }
                                                    catch (Exception e)
                                                    {
                                                        Util.Logging.Log(e);
                                                    }

                                                    if (cw != null)
                                                    {
                                                        if (Util.Logging.Enabled)
                                                        {
                                                            Util.Logging.LogEvent(Account.Settings, "Waiting for Coherent event...");
                                                        }

                                                        var cwe = WaitForCoherentEvent(process, handle, cw);

                                                        if (cwe == CoherentWatcher.EventType.Error || cwe == CoherentWatcher.EventType.Exited)
                                                        {
                                                            if (Util.Logging.Enabled)
                                                            {
                                                                Util.Logging.LogEvent(Account.Settings, "Waiting for Coherent event... aborting (" + cwe + ")");
                                                            }

                                                            cw.Dispose();
                                                            cw = null;
                                                        }
                                                        else
                                                        {
                                                            if (Util.Logging.Enabled)
                                                            {
                                                                Util.Logging.LogEvent(Account.Settings, "Waiting for Coherent event... OK (" + cwe + ")");
                                                            }
                                                        }
                                                    }
                                                }

                                                if (cw == null)
                                                {
                                                    do
                                                    {
                                                        //it's possible for the launcher to never fully load, such as when failing to initially connect
                                                        r = WaitForHostProcesses(process, _handle, hostType, host, true, 1, null);
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
                                                do
                                                {
                                                    //it's possible for the launcher to never fully load, such as when failing to initially connect
                                                    r = WaitForHostProcesses(process, _handle, hostType, process, true, 2, null);
                                                    if (r > 0 || r == -1)
                                                        break;
                                                }
                                                while (!process.WaitForExit(500));
                                            }
                                        }
                                        else
                                        {
                                            r = 0;

                                            if (Util.Logging.Enabled)
                                            {
                                                Util.Logging.LogEvent(Account.Settings, "Skipping all CoherentUI checks due to settings");
                                            }
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

                                        #region CoherentUI login watch

                                        if (!wasAlreadyStarted)
                                        {
                                            //CEF has delayed writing
                                            if (this.Account.hostType == HostType.CoherentUI && (LoginComplete != null || cw == null))
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

                                        #endregion

                                        if (authWatch)
                                        {
                                            watchers.AuthWatcher.Add(this.Account, handle);
                                        }

                                        if (WindowChanged != null)
                                        {
                                            wce.Type = WindowChangedEventArgs.EventType.LauncherWindowLoaded;
                                            WindowChanged(this, wce);
                                        }

                                        #region CoherentWatcher

                                        if (cw != null)
                                        {
                                            using (cw)
                                            {
                                                var loginStart = Environment.TickCount;
                                                r = 0;

                                                while (r == 0)
                                                {
                                                    var cwe = WaitForCoherentEvent(process, handle, cw,
                                                        delegate
                                                        {
                                                            return timeout == 0 || Environment.TickCount - timeoutStart <= timeout;
                                                        });

                                                    if (Util.Logging.Enabled)
                                                    {
                                                        if (cwe != CoherentWatcher.EventType.None)
                                                        {
                                                            Util.Logging.LogEvent(Account.Settings, "Coherent event: " + cwe);
                                                        }
                                                    }

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

                                        #endregion

                                        #region CEF watcher

                                        if (hostType == HostType.CEF)
                                        {
                                            var loginStart = Environment.TickCount;
                                            var et = WatchCefEvents(process, handle);

                                            switch (et)
                                            {
                                                case CoherentWatcher.EventType.LoginError:

                                                    if (WindowChanged != null)
                                                    {
                                                        wce.Type = WindowChangedEventArgs.EventType.LauncherLoginError;
                                                        WindowChanged(this, wce);
                                                    }

                                                    break;
                                                case CoherentWatcher.EventType.LoginCode:

                                                    if (WindowChanged != null)
                                                    {
                                                        wce.Type = WindowChangedEventArgs.EventType.LauncherCefLoginEvent;
                                                        WindowChanged(this, wce);
                                                    }

                                                    if (LoginComplete != null)
                                                        LoginComplete(this, Environment.TickCount - loginStart);

                                                    break;
                                            }
                                        }

                                        #endregion

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
                                }

                                #endregion

                                break;
                            default:

                                #region Crash

                                if (isGw2 && buffer[0] == '#')
                                {
                                    if (Util.Logging.Enabled)
                                    {
                                        Util.Logging.LogEvent(Account.Settings, "Crash detected");
                                    }

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

                    if (hProcess != IntPtr.Zero)
                    {
                        NativeMethods.CloseHandle(hProcess);
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
            private bool FindModule(Process process, string module, int limit = 0)
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
            private bool FindModules(Process process, string[] modules, bool all, int limit = 0, Dictionary<string, ModuleInfo> info = null)
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

                            if (info != null)
                            {
                                if (!info.ContainsKey(n))
                                {
                                    info[n] = new ModuleInfo()
                                    {
                                        Index = info.Count,
                                        Timestamp = Environment.TickCount,
                                    };
                                }
                            }

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
            private bool FindModules(Process process, Windows.ProcessInfo pi, string[] modules, bool all, int limit = 0, Dictionary<string,ModuleInfo> info = null)
            {
                if (pi == null)
                {
                    return FindModules(process, modules, all, limit, info);
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

                            if (info != null)
                            {
                                if (!info.ContainsKey(n))
                                {
                                    info[n] = new ModuleInfo()
                                    {
                                        Index = info.Count,
                                        Timestamp = Environment.TickCount,
                                    };
                                }
                            }

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

                    if (!hasRead && FindModules(process, modules, all, 0, info))
                        return true;

                    if (limit == 0 || limit > 0 && Environment.TickCount - start > limit || process.WaitForExit(500))
                        break;
                }

                return false;
            }

            private WaitForFontsResult WaitForFonts(Process process, IntPtr window, Process host, Process[] renderers, int renderersLength, bool ignoreExisting = true, int timeout = 0, Func<Windows.Win32Handles.HandleMonitor.IHandle, bool> onFontLoaded = null)
            {
                if (!Util.Users.IsCurrentUser(Account.Settings.WindowsAccount))
                {
                    try
                    {
                        var username = Util.Users.GetUserName(Account.Settings.WindowsAccount);
                        var password = Security.Credentials.GetPassword(username);

                        using (var identity = Security.Impersonation.GetIdentity(username, password))
                        {
                            return _WaitForFonts(process, window, host, renderers, renderersLength, identity, ignoreExisting, timeout, onFontLoaded);
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
                    return new WaitForFontsResult();
                }
                else
                {
                    return _WaitForFonts(process, window, host, renderers, renderersLength, null, ignoreExisting, timeout, onFontLoaded);
                }
            }

            private WaitForFontsResult _WaitForFonts(Process process, IntPtr window, Process host, Process[] renderers, int renderersLength, Security.Impersonation.IIdentity identity = null, bool ignoreExisting = true, int timeout = 0, Func<Windows.Win32Handles.HandleMonitor.IHandle, bool> onFontLoaded = null)
            {
                DateTime limit;

                if (timeout == 0)
                {
                    limit = DateTime.UtcNow.AddMinutes(3);
                }
                else
                {
                    limit = DateTime.UtcNow.AddMilliseconds(timeout);
                }

                var result = new WaitForFontsResult();
                var monitors = new Windows.Win32Handles.HandleMonitor.IMonitor[renderersLength];
                var counter = 0;
                var pid = 0;

                EventHandler<Windows.Win32Handles.HandleMonitor.IHandle> onAdded = delegate(object o, Windows.Win32Handles.HandleMonitor.IHandle h)
                {
                    var n = h.GetName(identity);
                    if (n == null)
                        return;
                    var l = n.Length;

                    ++counter;

                    if (l > 3 && n[l - 4] == '.')
                    {
                        //only ttf is used, but checking for others

                        var isFont = false;

                        #region Extension: ttf, otf, fnt

                        switch (n[l - 1])
                        {
                            case 'f':
                            case 'F':

                                switch (n[l - 2])
                                {
                                    case 't':
                                    case 'T':

                                        switch (n[l - 3])
                                        {
                                            case 't':
                                            case 'T':
                                            case 'o':
                                            case 'O':

                                                isFont = true;

                                                break;
                                        }

                                        break;
                                }

                                break;
                            case 't':
                            case 'T':

                                switch (n[l - 2])
                                {
                                    case 'n':
                                    case 'N':

                                        switch (n[l - 3])
                                        {
                                            case 'f':
                                            case 'F':

                                                isFont = true;

                                                break;
                                        }

                                        break;
                                }

                                break;
                        }

                        #endregion

                        if (isFont)
                        {
                            if (onFontLoaded == null || onFontLoaded(h))
                            {
                                using (var m = (Windows.Win32Handles.HandleMonitor.IMonitor)o)
                                {
                                    pid = m.PID;
                                }
                            }
                        }
                    }
                };

                for (var i = 0; i < renderersLength; i++)
                {
                    monitors[i] = Windows.Win32Handles.HandleMonitor.Create(Windows.Win32Handles.HandleType.File, renderers[i].Id, ignoreExisting);
                    monitors[i].HandleAdded += onAdded;
                    monitors[i].Start();
                }

                var ticks = 0;

                EventHandler onTick = delegate
                {
                    ++ticks;
                };

                try
                {
                    Windows.Win32Handles.HandleMonitor.Tick += onTick;

                    do
                    {
                        if (pid != 0)
                        {
                            result.PID = pid;
                            return result;
                        }

                        process.Refresh();
                        if (window != Windows.FindWindow.FindMainWindow(process))
                        {
                            break;
                        }

                        for (var i = 1; i < renderersLength; i++)
                        {
                            try
                            {
                                if (renderers[i].HasExited)
                                {
                                    return result;
                                }
                            }
                            catch { }
                        }

                        if (DateTime.UtcNow > limit && (ticks > 1 || monitors[0].Cycles > 0) || !Windows.Win32Handles.HandleMonitor.IsActive)
                            break;
                    }
                    while (!renderers[0].WaitForExit(500));
                }
                finally
                {
                    Windows.Win32Handles.HandleMonitor.Tick -= onTick;

                    for (var i = 0; i < renderersLength;i++)
                    {
                        using (monitors[i]) { }
                    }
                }

                return result;




            }

            private bool WaitForExit(Process process, Process p, int delay, IntPtr window, Func<bool> onContinue, out int result)
            {
                if (p != null && p.WaitForExit(delay))
                {
                    result = 0;
                    return true;
                }

                process.Refresh();
                if (window != Windows.FindWindow.FindMainWindow(process) || onContinue != null && !onContinue())
                {
                    result = -1;
                    return true;
                }

                result = 0;
                return false;
            }

            private CoherentWatcher.EventType WaitForCoherentEvent(Process process, IntPtr window, CoherentWatcher cw, Func<bool> onContinue = null)
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
                            if (WaitForExit(process, process, 0, window, null, out w))
                                return CoherentWatcher.EventType.Error;
                            if (onContinue != null && !onContinue())
                                return CoherentWatcher.EventType.None;

                            break;
                    }

                    e = ev;
                }
                while (true);
            }

            private HostType GetHostType(string n)
            {
                //CefHost
                //CoherentUI_Host

                if (n != null && n.Length > 3 && n[0] == 'C')
                {
                    switch (n[2])
                    {
                        case 'h': return HostType.CoherentUI;
                        case 'f': return HostType.CEF;
                    }
                }

                return HostType.Unknown;
            }

            private Process WaitForHostProcess(Process process, IntPtr window, out HostType type, Func<bool> onContinue = null)
            {
                int r;
                HashSet<int> ids = null;

                if (Util.Logging.Enabled)
                {
                    Util.Logging.LogEvent(Account.Settings, "Waiting for host process...");
                }

                while (true)
                {
                    foreach (var p in EnumerateChildProcesses(process))
                    {
                        try
                        {
                            var n = p.Process.ProcessName;

                            if (Util.Logging.Enabled)
                            {
                                if (ids == null)
                                    ids = new HashSet<int>();
                                if (ids.Add(p.Process.Id))
                                {
                                    string path;

                                    try
                                    {
                                        path = Util.Logging.GetDisplayPath(Util.ProcessUtil.GetPath(p.Process), 1);
                                    }
                                    catch
                                    {
                                        path = "";
                                    }

                                    Util.Logging.LogEvent(Account.Settings, n + " (" + p.Process.Id + ") [" + path + "]");
                                }
                            }

                            type = GetHostType(n);

                            if (type == HostType.Unknown)
                            {
                                p.Dispose();
                                continue;
                            }

                            if (Util.Logging.Enabled)
                            {
                                Util.Logging.LogEvent(Account.Settings, "Found " + type + " host process");
                            }

                            return p.Process;
                        }
                        catch
                        {
                            p.Dispose();
                        }
                    }

                    if (WaitForExit(process, process, 500, window, onContinue, out r))
                    {
                        if (Util.Logging.Enabled)
                        {
                            Util.Logging.LogEvent(Account.Settings, "Waiting for host process... cancelled");
                        }

                        type = HostType.Unknown;
                        return null;
                    }
                }

            }

            private CoherentWatcher WaitForCoherentHost(Process process, IntPtr window, out int result, Process host = null, Func<bool> onContinue = null)
            {
                if (!Util.Users.IsCurrentUser(Account.Settings.WindowsAccount))
                {
                    try
                    {
                        var username = Util.Users.GetUserName(Account.Settings.WindowsAccount);
                        var password = Security.Credentials.GetPassword(username);

                        using (Security.Impersonation.Impersonate(username, password))
                        {
                            return _WaitForCoherentHost(process, window, out result, host, onContinue);
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
                    return _WaitForCoherentHost(process, window, out result, host, onContinue);
                }
            }

            private CoherentWatcher _WaitForCoherentHost(Process process, IntPtr window, out int result, Process host, Func<bool> onContinue)
            {
                var limit = DateTime.UtcNow.AddMinutes(3);
                var dispose = host == null;
                result = 0;

                if (Util.Logging.Enabled)
                {
                    Util.Logging.LogEvent(Account.Settings, "Waiting for Coherent view...");
                }

                //note the CoherentUI host process can change after it's initially started - if something goes wrong, it will start a new process until it's successful

                while (true)
                {
                    if (host == null)
                    {
                        do
                        {
                            host = GetHostProcess(process);

                            if (host != null)
                            {
                                break;
                            }
                            //if ((pidChild = GetChildProcess(process.Id)) > 0)
                            //{
                            //    break;
                            //}

                            if (WaitForExit(process, process, 500, window, onContinue, out result))
                                return null;
                        }
                        while (DateTime.UtcNow < limit);
                    }

                    if (host != null)
                    {
                        var errors = 0;
                        var pids = new UIntPtr[] 
                        { 
                            (UIntPtr)host.Id
                        };

                        CoherentWatcher.FileMap m = null;

                        do
                        {
                            try
                            {
                                m = CoherentWatcher.Find(pids);
                                if (m != null)
                                {
                                    if (dispose)
                                    {
                                        host.Dispose();
                                    }

                                    if (Util.Logging.Enabled)
                                    {
                                        Util.Logging.LogEvent(Account.Settings, "Waiting for Coherent view... OK");
                                    }

                                    return new CoherentWatcher(m);
                                }
                            }
                            catch
                            {
                                if (++errors == 5)
                                    throw;
                            }

                            if (WaitForExit(process, process, 500, window, onContinue, out result))
                                return null;

                            try
                            {
                                if (host.HasExited)
                                    throw new Exception();
                            }
                            catch
                            {
                                if (dispose)
                                {
                                    host.Dispose();
                                }
                                else
                                {
                                    dispose = true;
                                }
                                host = null;
                                break;
                            }
                        }
                        while (DateTime.UtcNow < limit);

                        if (host == null)
                        {
                            continue;
                        }
                    }

                    if (Util.Logging.Enabled)
                    {
                        Util.Logging.LogEvent(Account.Settings, "Waiting for Coherent view... not found");
                    }

                    return null;
                }
            }

            private int WaitForHostProcesses(Process process, IntPtr window, HostType hostType, Process host, bool waitUntilLoaded = true, int childCount = 1, Func<bool> onContinue = null)
            {
                byte r = 1;
                bool skipFiles = false;

                if (hostType == HostType.CEF && host == null)
                {
                    //CEF has all (5) processes under GW2
                    host = process;
                }

                do
                {
                    if (Util.Logging.Enabled)
                    {
                        Util.Logging.LogEvent(Account.Settings, "Waiting for host process (" + r + ")");
                    }

                    var limit = DateTime.UtcNow.AddSeconds(30 * r);

                    if (hostType == HostType.CoherentUI && host == null)
                    {
                        //CoherentUI has a host process (1) under GW2 and all other processes (2) under the host
                        do
                        {
                            host = GetHostProcess(process);

                            if (host != null)
                            {
                                break;
                            }

                            int w;
                            if (WaitForExit(process, process, 500 * r, window, onContinue, out w))
                                return w;
                        }
                        while (DateTime.UtcNow < limit);
                    }

                    if (host != null)
                    {
                        if (Util.Logging.Enabled)
                        {
                            Util.Logging.LogEvent(Account.Settings, "Waiting for renderer under PID " + host.Id);
                        }

                        var renderers = new Process[childCount];
                        var t = Environment.TickCount;

                        while (true)
                        {
                            var count = GetRendererProcess(host, renderers);

                            try
                            {
                                if (count > 0)
                                {
                                    if (Util.Logging.Enabled)
                                    {
                                        var ids = renderers[0].Id.ToString();

                                        for (var i = 1; i < count; i++)
                                        {
                                            ids += ", " + renderers[i].Id;
                                        }

                                        Util.Logging.LogEvent(Account.Settings, "Found renderer (" + ids + ")");
                                    }

                                    if (!waitUntilLoaded)
                                    {
                                        return renderers[0].Id;
                                    }

                                    if (!skipFiles)
                                    {
                                        if (!Settings.Tweaks.Launcher.HasValue || Settings.Tweaks.Launcher.Value.WaitForCoherentLoaded)
                                        {
                                            if (Util.Logging.Enabled)
                                            {
                                                Util.Logging.LogEvent(Account.Settings, "Waiting for files to load");
                                            }
                                            try
                                            {
                                                var pid = WaitForFonts(process, window, host, renderers, count, false, count == childCount ? 0 : -1).PID;

                                                if (pid != 0)
                                                {
                                                    if (Util.Logging.Enabled)
                                                    {
                                                        Util.Logging.LogEvent(Account.Settings, "Waiting for files to load... OK");
                                                    }
                                                    return pid;
                                                }
                                                else if (count != childCount)
                                                {
                                                    continue;
                                                }
                                                else
                                                {
                                                    if (Util.Logging.Enabled)
                                                    {
                                                        Util.Logging.LogEvent(Account.Settings, "Waiting for files to load... FAILED");
                                                    }
                                                }
                                            }
                                            catch (NotSupportedException)
                                            {
                                                if (count < childCount && Environment.TickCount - t < 30000)
                                                {
                                                    continue;
                                                }
                                                if (Util.Logging.Enabled)
                                                {
                                                    Util.Logging.LogEvent(Account.Settings, "Waiting for files to load was not supported");
                                                }
                                                skipFiles = true;
                                            }
                                        }
                                        else
                                        {
                                            skipFiles = true;
                                            if (Util.Logging.Enabled)
                                            {
                                                Util.Logging.LogEvent(Account.Settings, "Skipping wait for files to load due to settings");
                                            }
                                            if (!Settings.Tweaks.Launcher.Value.WaitForCoherentMemory)
                                                return renderers[0].Id;
                                        }
                                    }

                                    //this should only be used as a backup - first priority is checking for fonts (when detecting the launcher) or modules (when detecting character select)

                                    if (!Settings.Tweaks.Launcher.HasValue || Settings.Tweaks.Launcher.Value.WaitForCoherentMemory)
                                    {
                                        if (Util.Logging.Enabled)
                                        {
                                            Util.Logging.LogEvent(Account.Settings, "Waiting for host to load (using fallback)");
                                        }
                                        long mem = 0;
                                        byte counter = 0;
                                        byte check = 0;

                                        limit = DateTime.UtcNow.AddSeconds(300);

                                        do
                                        {
                                            if (++check == 5)
                                            {
                                                int w;
                                                if (WaitForExit(process, null, 0, window, onContinue, out w))
                                                    return w;
                                                check = 0;
                                            }

                                            long _mem = 0;

                                            renderers[0].Refresh();

                                            try
                                            {
                                                _mem = renderers[0].PrivateMemorySize64;
                                            }
                                            catch { }

                                            if (_mem > mem)
                                            {
                                                mem = _mem + 10000;
                                                counter = 0;
                                            }
                                            else if (++counter > 10)
                                            {
                                                //CoherentUI can stall and never fully load

                                                break;
                                            }
                                        }
                                        while (DateTime.UtcNow < limit && !renderers[0].WaitForExit(100));
                                    }
                                    else
                                    {
                                        if (Util.Logging.Enabled)
                                        {
                                            Util.Logging.LogEvent(Account.Settings, "Skipping wait for host to load due to settings");
                                        }
                                        int w;
                                        if (WaitForExit(process, null, 0, window, onContinue, out w))
                                            return w;
                                    }

                                    return renderers[0].Id;
                                }
                            }
                            finally
                            {
                                for (var i = 0; i < count; i++)
                                {
                                    using (renderers[i]) { };
                                    renderers[i] = null;
                                }
                            }

                            int we;
                            if (WaitForExit(process, host, 500, window, onContinue, out we))
                            {
                                if (we == 0)
                                {
                                    //host process has exited

                                    if (hostType == HostType.CoherentUI)
                                    {
                                        //CoherentUI can restart its host process
                                        host = null;
                                        break;
                                    }
                                }

                                return r;
                            }
                        }
                    }

                }
                while (++r < 3);

                Util.Logging.Log("Unable to find host");

                return 0;
            }

            private CoherentWatcher.EventType WatchCefEvents(Process process, IntPtr window)
            {
                var renderers = new Process[2];
                var count = GetRendererProcess(process, renderers);
                var monitors = new Windows.Win32Handles.HandleMonitor.IMonitor[count];
                var t = CoherentWatcher.EventType.None;

                Func<Windows.Win32Handles.HandleMonitor.IHandle, bool> onFont;

                onFont = delegate(Windows.Win32Handles.HandleMonitor.IHandle h)
                {
                    var n = Path.GetFileName(h.GetName());


                    if (n.Equals("timesi.ttf", StringComparison.OrdinalIgnoreCase))
                    {
                        //occurs when an error is showing
                        t = CoherentWatcher.EventType.LoginError;
                        return true;
                    }
                    else
                    {
                        //assuming any change is a login

                        if (n.Equals("segoeuii.ttf", StringComparison.OrdinalIgnoreCase))
                        {
                            //occurs when it changes to the code entry or on successful login
                        }
                        else
                        {
                            if (Util.Logging.Enabled)
                            {
                                Util.Logging.LogEvent(Account.Settings, "Unexpected: " + n);
                            }
                        }

                        t = CoherentWatcher.EventType.LoginCode;
                        return true;
                    }

                    //return false;
                };


                try
                {
                    var r = WaitForFonts(process, window, process, renderers, count, true, 90000, onFont);
                }
                catch (NotSupportedException)
                {
                    if (Util.Logging.Enabled)
                    {
                        Util.Logging.LogEvent(Account.Settings, "Watching for CEF events was not supported");
                    }
                }
                catch (Exception e)
                {
                    Util.Logging.Log(e);
                }

                return t;
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
