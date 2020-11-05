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
                public LaunchSession(LaunchMode mode, string args = null)
                {
                    this.Mode = mode;
                    this.Args = args;
                }

                ~LaunchSession()
                {
                    Dispose();
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

                public NetworkAuthorization.ISession AuthSession
                {
                    get;
                    set;
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
                                if (_RunAfter != null)
                                    _RunAfter.Dispose();
                                _RunAfter = value;
                            }
                        }
                    }
                }
                
                public bool IsDisposed
                {
                    get;
                    private set;
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
                        RunAfter = null;

                        if (AuthSession != null)
                        {
                            AuthSession.Release();
                            AuthSession = null;
                        }

                        IsDisposed = true;
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
                this.Session = null;

                if (Exited != null)
                    Exited(this, this);
            }
        }
    }
}
