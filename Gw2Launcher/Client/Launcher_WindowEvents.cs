using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using Gw2Launcher.Windows.Native;

namespace Gw2Launcher.Client
{
    static partial class Launcher
    {
        private class WindowEvents : IDisposable
        {
            public class WindowEventsEventArgs : EventArgs
            {
                public WindowEventsEventArgs(Account account, IntPtr handle, uint eventType, int idObject, int idChild, uint dwEventThread)
                {
                    this.Account = account;
                    this.Handle = handle;
                    this.EventType = eventType;
                    this.EventObject = idObject;
                    this.EventChild = idChild;
                    this.EventThread = dwEventThread;
                }

                public Account Account
                {
                    get;
                    private set;
                }

                public IntPtr Handle
                {
                    get;
                    private set;
                }

                public uint EventType
                {
                    get;
                    private set;
                }

                public int EventObject
                {
                    get;
                    private set;
                }

                public int EventChild
                {
                    get;
                    private set;
                }

                public uint EventThread
                {
                    get;
                    private set;
                }
            }

            public class Events
            {
                private class RegisteredEvent : IDisposable
                {
                    private WinEventDelegate proc;
                    private IntPtr handle;

                    public RegisteredEvent(int pid, uint eventMin, uint eventMax, WinEventDelegate proc)
                    {
                        this.proc = proc;
                        this.handle = NativeMethods.SetWinEventHook(eventMin, eventMax, IntPtr.Zero, proc, (uint)pid, 0, 0);
                    }

                    ~RegisteredEvent()
                    {
                        Dispose();
                    }

                    public void Dispose()
                    {
                        GC.SuppressFinalize(this);

                        if (handle != IntPtr.Zero)
                        {
                            try
                            {
                                if (!NativeMethods.UnhookWinEvent(handle))
                                    throw new System.ComponentModel.Win32Exception(System.Runtime.InteropServices.Marshal.GetLastWin32Error());
                                handle = IntPtr.Zero;

                            }
                            catch (Exception e)
                            {
                                Util.Logging.Log(e);
                            }
                        }
                    }
                }

                public event EventHandler<WindowEventsEventArgs> MinimizeStart,
                                                                 MinimizeEnd,
                                                                 ForegroundChanged,
                                                                 MoveSizeEnd,
                                                                 MoveSizeBegin,
                                                                 LocationChanged;

                private Account account;
                private RegisteredEvent[] events;
                private WinEventDelegate proc;
                private bool waitForeground;
                private bool waitMinimize;

                public Events(Account account)
                {
                    this.account = account;
                    this.events = new RegisteredEvent[3];
                    this.proc = new WinEventDelegate(WinEventProc);
                }

                ~Events()
                {
                    Destroy();
                }

                public Account Account
                {
                    get
                    {
                        return account;
                    }
                }

                public void Initialize(int pid)
                {
                    for (var i = 0; i < this.events.Length; i++)
                    {
                        using (this.events[i])
                        {
                            this.events[i] = null;
                        }
                    }

                    try
                    {
                        waitForeground = true;

                        this.events[0] = new RegisteredEvent(pid, 0x0003, 0x0017, proc);
                        this.events[1] = new RegisteredEvent(pid, 0x8004, 0x8004, proc);
                    }
                    catch (Exception e)
                    {
                        Util.Logging.Log(e);
                    }
                }

                private void WinEventProc(IntPtr hWinEventHook, uint eventType, IntPtr hwnd, int idObject, int idChild, uint dwEventThread, uint dwmsEventTime)
                {
                    switch (eventType)
                    {
                        case 0x000A: //EVENT_SYSTEM_MOVESIZEBEGIN

                            if (MoveSizeBegin != null)
                                MoveSizeBegin(this, new WindowEventsEventArgs(account, hwnd, eventType, idObject, idChild, dwEventThread));

                            break;
                        case 0x0016: //EVENT_SYSTEM_MINIMIZESTART
                            
                            if (MinimizeStart != null)
                                MinimizeStart(this, new WindowEventsEventArgs(account, hwnd, eventType, idObject, idChild, dwEventThread));

                            break;
                        case 0x0017: //EVENT_SYSTEM_MINIMIZEEND

                            if (this.events[2] == null)
                            {
                                try
                                {
                                    this.events[2] = new RegisteredEvent(account.Process.Process.Id, 0x800B, 0x800B, proc);
                                }
                                catch { }
                            }

                            break;
                        case 0x000B: //EVENT_SYSTEM_MOVESIZEEND

                            if (MoveSizeEnd != null)
                                MoveSizeEnd(this, new WindowEventsEventArgs(account, hwnd, eventType, idObject, idChild, dwEventThread));

                            break;
                        case 0x8004: //EVENT_OBJECT_REORDER

                            if (idObject == -4) //OBJID_CLIENT
                            {
                                //backup for EVENT_SYSTEM_FOREGROUND, which  isn't fired if the window was loaded in the background
                                try
                                {
                                    var f = NativeMethods.GetForegroundWindow();
                                    if (dwEventThread == NativeMethods.GetWindowThreadId(f))
                                    {
                                        if (ForegroundChanged != null)
                                            ForegroundChanged(this, new WindowEventsEventArgs(account, f, eventType, idObject, idChild, dwEventThread));
                                    }
                                }
                                catch { }
                            }

                            break;
                        case 0x800B: //EVENT_OBJECT_LOCATIONCHANGE

                            //calling MinimizeEnd after the window has been restored, rather than as it's being restored

                            using (var e = events[2])
                            {
                                if (e != null)
                                {
                                    events[2] = null;

                                    if (MinimizeEnd != null)
                                        MinimizeEnd(this, new WindowEventsEventArgs(account, hwnd, eventType, idObject, idChild, dwEventThread));
                                }
                            }

                            //calling ForegroundChanged, as reordering may not occur after restoring the window

                            if (ForegroundChanged != null)
                            {
                                try
                                {
                                    var f = NativeMethods.GetForegroundWindow();
                                    if (dwEventThread == NativeMethods.GetWindowThreadId(f))
                                    {
                                        if (ForegroundChanged != null)
                                            ForegroundChanged(this, new WindowEventsEventArgs(account, f, eventType, idObject, idChild, dwEventThread));
                                    }
                                }
                                catch { }
                            }

                            break;
                        case 0x0003: //EVENT_SYSTEM_FOREGROUND

                            //Note: unreliable. The event will fail to fire when multiple windows are opened, and can be fired after another window has already taken the foreground

                            //if (ForegroundChanged != null)
                            //    ForegroundChanged(this, new WindowEventsEventArgs(account, hwnd, eventType, idObject, idChild, dwEventThread));

                            break;
                    }

                }

                public void Destroy()
                {
                    GC.SuppressFinalize(this);

                    if (this.events != null)
                    {
                        foreach (var e in this.events)
                        {
                            using (e) { }
                        }
                        //this.events = null;
                    }
                }
            }

            private Dictionary<int, Events> events;
            private SynchronizationContext context;
            private int contextId;

            public WindowEvents()
            {
                events = new Dictionary<int, Events>();
                context = SynchronizationContext.Current;
                contextId = Thread.CurrentThread.ManagedThreadId;
                System.Windows.Forms.Application.ThreadExit += Application_ThreadExit;
            }

            public Events Add(int pid, Account account)
            {
                Events ae;

                lock (this)
                {
                    if (events.TryGetValue(pid, out ae))
                        return ae;
                    else
                        events[pid] = ae = new Events(account);
                }

                if (contextId != Thread.CurrentThread.ManagedThreadId)
                {
                    if (context != null)
                    {
                        context.Send(
                            delegate
                            {
                                ae.Initialize(pid);
                            }, null);
                    }
                }
                else
                {
                    ae.Initialize(pid);
                }

                return ae;
            }

            public bool Contains(int pid)
            {
                lock (this)
                {
                    return events.ContainsKey(pid);
                }
            }

            public void Remove(int pid)
            {
                Events e;

                lock (this)
                {
                    if (events.TryGetValue(pid, out e))
                        events.Remove(pid);
                }

                if (e != null)
                {
                    if (contextId != Thread.CurrentThread.ManagedThreadId)
                    {
                        if (context != null)
                        {
                            context.Post(
                                delegate
                                {
                                    e.Destroy();
                                }, null);
                        }
                    }
                    else
                        e.Destroy();
                }
            }

            ~WindowEvents()
            {
                Dispose();
            }

            void Application_ThreadExit(object sender, EventArgs e)
            {
                Dispose();
            }

            public void Dispose()
            {
                GC.SuppressFinalize(this);

                if (context != null)
                {
                    SendOrPostCallback callback = delegate
                    {
                        foreach (var e in events.Values)
                        {
                            e.Destroy();
                        }
                    };

                    if (contextId != Thread.CurrentThread.ManagedThreadId)
                    {
                        try
                        {
                            context.Send(callback, null);
                        }
                        catch
                        {
                            callback(null);
                        }
                    }
                    else
                    {
                        callback(null);
                    }

                    context = null;
                }
            }
        }
    }
}
