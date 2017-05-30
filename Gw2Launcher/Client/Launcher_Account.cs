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
            public event EventHandler<Account> Exited;

            public Account(Settings.IAccount settings)
            {
                this.Settings = settings;
                this.Process = new LinkedProcess(this);
            }

            public byte inQueueCount;
            public byte errors;

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
                                taskAnnounce = Task.Factory.StartNew(
                                    delegate
                                    {
                                        DoAnnounce();
                                    });
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
