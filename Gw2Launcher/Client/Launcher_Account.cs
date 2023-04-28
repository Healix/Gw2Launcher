using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gw2Launcher.Client
{
    static partial class Launcher
    {
        private class Account
        {
            public class LaunchSession : IDisposable
            {
                public LaunchSession(Account a, LaunchMode mode, string args = null)
                {
                    this.Account = a;
                    this.Mode = mode;
                    this.Args = args;
                }

                ~LaunchSession()
                {
                    Dispose();
                }

                public Account Account
                {
                    get;
                    private set;
                }

                public LaunchMode Mode
                {
                    get;
                    private set;
                }

                public string Args
                {
                    get;
                    private set;
                }

                private Tools.Icons _Icons;
                public Tools.Icons Icons
                {
                    get
                    {
                        return _Icons;
                    }
                    set
                    {
                        lock (this)
                        {
                            if (_Icons != value)
                            {
                                if (_Icons != null)
                                    _Icons.Dispose();
                                _Icons = value;
                            }
                        }
                    }
                }

                private FileManager.FileLocker.ISharedFile _GfxLock;
                public FileManager.FileLocker.ISharedFile GfxLock
                {
                    get
                    {
                        return _GfxLock;
                    }
                    set
                    {
                        lock (this)
                        {
                            if (_GfxLock != value)
                            {
                                if (_GfxLock != null)
                                    _GfxLock.Dispose();
                                _GfxLock = value;
                            }
                        }
                    }
                }

                private WindowWatcher _Watcher;
                public WindowWatcher Watcher
                {
                    get
                    {
                        return _Watcher;
                    }
                    set
                    {
                        lock (this)
                        {
                            if (_Watcher != value)
                            {
                                if (_Watcher != null)
                                    _Watcher.Dispose();
                                _Watcher = value;
                            }
                        }
                    }
                }

                private Limiter.ISession _Limiter;
                public Limiter.ISession Limiter
                {
                    get
                    {
                        return _Limiter;
                    }
                    set
                    {
                        lock(this)
                        {
                            if (_Limiter != value)
                            {
                                if (_Limiter != null)
                                    _Limiter.Dispose();
                                _Limiter = value;
                            }
                        }
                    }
                }

                private IRunAfterManager _RunAfter;
                public IRunAfterManager RunAfter
                {
                    get
                    {
                        return _RunAfter;
                    }
                    set
                    {
                        lock (this)
                        {
                            if (_RunAfter != value)
                            {
                                using (_RunAfter)
                                {
                                    _RunAfter = value;
                                }
                            }
                        }
                    }
                }

                private ProcessSettings _ProcessSettings;
                public ProcessSettings ProcessSettings
                {
                    get
                    {
                        return _ProcessSettings;
                    }
                    set
                    {
                        lock (this)
                        {
                            if (_ProcessSettings != value)
                            {
                                if (isDisposed && value != null)
                                {
                                    value.Dispose();
                                }
                                else
                                {
                                    if (_ProcessSettings != null)
                                        _ProcessSettings.Dispose();
                                    _ProcessSettings = value;
                                }
                            }
                        }
                    }
                }

                private Tools.WindowManager.IWindowBounds _WindowTemplate;
                public Tools.WindowManager.IWindowBounds WindowTemplate
                {
                    get
                    {
                        return _WindowTemplate;
                    }
                    set
                    {
                        lock (this)
                        {
                            if (_WindowTemplate != value)
                            {
                                if (isDisposed && value != null)
                                {
                                    value.Dispose();
                                }
                                else
                                {
                                    if (_WindowTemplate != null)
                                        _WindowTemplate.Dispose();
                                    _WindowTemplate = value;
                                    if (value != null)
                                        WindowOptions = value.Options | Gw2Launcher.Settings.WindowOptions.Windowed;
                                }
                            }
                        }
                    }
                }

                public Settings.WindowOptions WindowOptions
                {
                    get;
                    set;
                }

                private Tools.Mumble.MumbleMonitor.IMumbleProcess _MumbleLink;
                public Tools.Mumble.MumbleMonitor.IMumbleProcess MumbleLink
                {
                    get
                    {
                        return _MumbleLink;
                    }
                    set
                    {
                        lock (this)
                        {
                            if (_MumbleLink != value)
                            {
                                if (_MumbleLink != null)
                                    _MumbleLink.Dispose();
                                _MumbleLink = value;

                                if (MumbleLinkChanged != null)
                                    MumbleLinkChanged(this.Account.Settings, value);
                            }
                        }
                    }
                }

                private WindowWatcher.HiddenWindow _Hidden;
                public WindowWatcher.HiddenWindow Hidden
                {
                    get
                    {
                        return _Hidden;
                    }
                    set
                    {
                        lock (this)
                        {
                            if (_Hidden != value)
                            {
                                if (_Hidden != null)
                                    _Hidden.Dispose();
                                _Hidden = value;
                            }
                        }
                    }
                }

                public Settings.LaunchProxy Proxy
                {
                    get;
                    set;
                }

                private bool isDisposed;
                public bool IsDisposed
                {
                    get
                    {
                        return isDisposed;
                    }
                }

                public void Dispose()
                {
                    GC.SuppressFinalize(this);

                    lock (this)
                    {
                        GfxLock = null;
                        Icons = null;
                        Watcher = null;
                        Limiter = null;
                        WindowTemplate = null;
                        MumbleLink = null;
                        RunAfter = null;

                        isDisposed = true;
                    }
                }
            }

            public event EventHandler<Account> Exited;

            public Account(Settings.IAccount settings)
            {
                this.Settings = settings;
                this.Process = new LinkedProcess(this);
            }

            public byte isRelaunch;
            public byte inQueueCount;
            public byte errors;
            public WindowWatcher.HostType hostType;
            public bool isLaunching;

            //public void Dispose()
            //{
            //    if (watcher != null)
            //        watcher.Dispose();
            //}

            //public bool InUse
            //{
            //    get
            //    {
            //        return InUseCount > 0;
            //    }
            //    set
            //    {
            //        if (value)
            //        {
            //            InUseCount++;
            //        }
            //        else if (InUseCount > 0)
            //            InUseCount--;
            //    }
            //}

            //public byte InUseCount
            //{
            //    get;
            //    set;
            //}

            public AccountType Type
            {
                get
                {
                    switch (Settings.Type)
                    {
                        case Gw2Launcher.Settings.AccountType.GuildWars2:
                            return AccountType.GuildWars2;
                        case Gw2Launcher.Settings.AccountType.GuildWars1:
                            return AccountType.GuildWars1;
                    }

                    return AccountType.Unknown;
                }
            }

            public Settings.IAccount Settings
            {
                get;
                private set;
            }

            public ProcessSettings ProcessSettings
            {
                get
                {
                    var s = session;
                    if (s != null)
                    {
                        return s.ProcessSettings;
                    }
                    return null;
                }
            }

            public Settings.WindowOptions WindowOptions
            {
                get
                {
                    var s = session;
                    if (s != null)
                    {
                        return s.WindowOptions;
                    }
                    return this.Settings.WindowOptions;
                }
            }

            public Tools.WindowManager.IWindowBounds WindowTemplate
            {
                get
                {
                    var s = session;
                    if (s != null)
                    {
                        return s.WindowTemplate;
                    }
                    return null;
                }
            }

            public Tools.Mumble.MumbleMonitor.IMumbleProcess MumbleLink
            {
                get
                {
                    var s = session;
                    if (s != null)
                    {
                        return s.MumbleLink;
                    }
                    return null;
                }
            }

            private RunAfterManager _RunAfter;
            public RunAfterManager RunAfter
            {
                get
                {
                    return _RunAfter;
                }
                set
                {
                    lock (this)
                    {
                        if (_RunAfter != value)
                        {
                            using (_RunAfter)
                            {
                                _RunAfter = value;
                            }
                        }
                    }
                }
            }

            public LinkedProcess Process
            {
                get;
                private set;
            }

            public AccountState State
            {
                get;
                private set;
            }

            private LaunchSession session;
            public LaunchSession Session
            {
                get
                {
                    return session;
                }
                set
                {
                    if (session != value)
                    {
                        if (session != null)
                            session.Dispose();
                        session = value;
                    }
                }
            }

            public bool IsActive
            {
                get
                {
                    switch (this.State)
                    {
                        case AccountState.Active:
                        case AccountState.ActiveGame:
                        case AccountState.Updating:
                        case AccountState.UpdatingVisible:
                            return true;
                    }
                    return false;
                }
            }

            public void SetState(AccountState state, bool announce, object data)
            {
                if (this.State != state)
                {
                    AccountState previousState = this.State;
                    this.State = state;
                    if (announce && AccountStateChanged != null)
                    {
                        lock (queueAnnounce)
                        {
                            queueAnnounce.Enqueue(new QueuedAnnounce(this.Settings, state, previousState, data));
                            if (taskAnnounce == null || taskAnnounce.IsCompleted)
                            {
                                taskAnnounce = new Task(
                                    delegate
                                    {
                                        DoAnnounce();
                                    });
                                taskAnnounce.Start();
                            }
                        }
                    }
                }
            }

            public void SetState(AccountState state, bool announce)
            {
                SetState(state, announce, null);
            }

            public void OnExited()
            {
                if (!isLaunching)
                {
                    this.Session = null;
                }

                if (Exited != null)
                    Exited(this, this);
            }
        }
    }
}
