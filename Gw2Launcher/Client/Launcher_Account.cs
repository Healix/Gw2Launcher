﻿using System;
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
            public event EventHandler<Account> Exited;

            public Account(Settings.IAccount settings)
            {
                this.Settings = settings;
                this.Process = new LinkedProcess(this);
            }

            public byte isRelaunch;
            public byte inQueueCount;
            public byte errors;

            public WindowWatcher watcher;
            public EventHandler<string> watcherCallback;

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
                            queueAnnounce.Enqueue(new QueuedAnnounce(this.Settings.UID, state, previousState, data));
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
                if (Exited != null)
                    Exited(this, this);
            }
        }
    }
}
