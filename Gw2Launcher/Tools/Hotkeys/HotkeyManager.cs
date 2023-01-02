using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using Gw2Launcher.Windows;

namespace Gw2Launcher.Tools.Hotkeys
{
    class HotkeyManager : IDisposable
    {
        public class AccountHotkeysEventArgs : EventArgs
        {
            public AccountHotkeysEventArgs()
            {

            }
        }

        private class AccountHotkeys
        {
            public event EventHandler<HotkeyPressEventArgs> Pressed;

            public AccountHotkeys(Settings.IAccount account)
            {
                this.Account = account;
            }

            public Settings.IAccount Account
            {
                get;
                private set;
            }

            public bool HasHotkeys
            {
                get;
                set;
            }

            public bool IsActive
            {
                get;
                set;
            }

            public Settings.Hotkey[] Hotkeys
            {
                get
                {
                    return Account.Hotkeys;
                }
            }

            public Windows.Hotkeys.IHotkey[] Registered
            {
                get;
                set;
            }

            public void OnKeyPress(object sender, Windows.Hotkeys.HotkeyEventArgs e)
            {
                if (Pressed != null)
                    Pressed(this, new HotkeyPressEventArgs(e, (Settings.Hotkey)((Windows.Hotkeys.IHotkey)sender).Data, Account));
            }
        }

        public class HotkeyPressEventArgs : EventArgs
        {
            private Windows.Hotkeys.HotkeyEventArgs source;

            public HotkeyPressEventArgs(Windows.Hotkeys.HotkeyEventArgs source, Settings.Hotkey hotkey, Settings.IAccount account)
            {
                this.source = source;

                Hotkey = hotkey;
                Account = account;
            }

            public Settings.Hotkey Hotkey
            {
                get;
                private set;
            }

            public Settings.IAccount Account
            {
                get;
                private set;
            }

            public bool SuppressHotkey
            {
                get
                {
                    return source.SuppressHotkey;
                }
                set
                {
                    source.SuppressHotkey = value;
                }
            }
        }

        public event EventHandler<HotkeyPressEventArgs> Pressed;
        public event EventHandler EnabledChanged;

        private Dictionary<ushort, AccountHotkeys> accounts;
        private bool refresh;
        private bool enabled;
        private Windows.Hotkeys hotkeys;
        private Windows.Hotkeys.IHotkey[] registered;

        public HotkeyManager(IntPtr window)
        {
            hotkeys = new Windows.Hotkeys(window);
            accounts = new Dictionary<ushort, AccountHotkeys>();

            Settings.Accounts.ValueChanged += Accounts_ValueChanged;
            Settings.Accounts.ValueAdded += Accounts_ValueAdded;
            Settings.Accounts.ValueRemoved += Accounts_ValueRemoved;
            Settings.Hotkeys.ValueChanged += Hotkeys_ValueChanged;
            Client.Launcher.AccountProcessChanged += Launcher_AccountProcessChanged;

            lock (this)
            {
                foreach (var uid in Settings.Accounts.GetKeys())
                {
                    var a = Settings.Accounts[uid];
                    if (a.HasValue)
                    {
                        OnAccountAdded(a, a.Value);
                    }
                }
            }
        }

        void Launcher_AccountProcessChanged(Settings.IAccount account, System.Diagnostics.Process e)
        {
            lock (this)
            {
                AccountHotkeys a;
                if (accounts.TryGetValue(account.UID, out a))
                {
                    var isActive = e != null;
                    if (a.IsActive != isActive)
                    {
                        a.IsActive = isActive;

                        hotkeys.Invoke(
                            delegate
                            {
                                if (isActive)
                                    ++ActiveAccounts;
                                else
                                    --ActiveAccounts;

                                Refresh(a);
                            });
                    }
                }
            }
        }

        private byte _ActiveAccounts;
        private byte ActiveAccounts
        {
            get
            {
                return _ActiveAccounts;
            }
            set
            {
                if (_ActiveAccounts != value)
                {
                    _ActiveAccounts = value;
                    if (value == 1 || value == 0)
                        Refresh();
                }
            }
        }

        void Hotkeys_ValueChanged(object sender, EventArgs e)
        {
            Refresh();
        }

        private void OnAccountAdded(Settings.ISettingValue<Settings.IAccount> v, Settings.IAccount account)
        {
            v.ValueCleared += Account_ValueCleared;
            account.HotkeysChanged += account_HotkeysChanged;

            lock (this)
            {
                AccountHotkeys a;

                accounts[account.UID] = a = new AccountHotkeys(account);

                a.Pressed += OnAccountHotkeyPressed;

                a.IsActive = Client.Launcher.IsActive(account);
                if (a.IsActive)
                    ++ActiveAccounts;

                if (account.Hotkeys != null)
                {
                    a.HasHotkeys = true;
                    Refresh(a);
                }
            }
        }

        private void OnAccountRemoved(Settings.ISettingValue<Settings.IAccount> v, AccountHotkeys h)
        {
            if (v != null)
                v.ValueCleared -= Account_ValueCleared;

            if (h != null)
            {
                h.Account.HotkeysChanged -= account_HotkeysChanged;
                Dispose(h.Registered);
                h.Registered = null;
            }
        }

        void account_HotkeysChanged(object sender, EventArgs e)
        {
            var a = (Settings.IAccount)sender;
            var v = a.Hotkeys;
            AccountHotkeys h;

            lock (this)
            {
                if (accounts.TryGetValue(a.UID, out h))
                {
                    if (v != null)
                    {
                        h.HasHotkeys = true;
                        Refresh(h);
                    }
                    else if (h.HasHotkeys)
                    {
                        h.HasHotkeys = false;
                        Dispose(h.Registered);
                        h.Registered = null;
                    }
                }
            }
        }

        void Accounts_ValueRemoved(object sender, ushort uid)
        {
            lock (this)
            {
                AccountHotkeys h;
                if (accounts.TryGetValue(uid, out h))
                {
                    OnAccountRemoved(sender as Settings.ISettingValue<Settings.IAccount>, h);
                    accounts.Remove(uid);
                }
            }
        }

        void Account_ValueCleared(object sender, Settings.IAccount account)
        {
            Accounts_ValueRemoved(sender, account.UID);
        }

        void Accounts_ValueAdded(object sender, KeyValuePair<ushort, Settings.ISettingValue<Settings.IAccount>> e)
        {
            lock (this)
            {
                OnAccountAdded(e.Value, e.Value.Value);
            }
        }

        void Accounts_ValueChanged(object sender, KeyValuePair<ushort, Settings.ISettingValue<Settings.IAccount>> e)
        {
            lock (this)
            {
                Accounts_ValueRemoved(sender, e.Key);

                if (e.Value.HasValue)
                {
                    Accounts_ValueAdded(sender, e);
                }
            }
        }

        public bool Enabled
        {
            get
            {
                return enabled;
            }
            set
            {
                if (enabled != value)
                {
                    enabled = value;

                    if (EnabledChanged != null)
                        EnabledChanged(this, EventArgs.Empty);

                    hotkeys.Invoke(
                        delegate
                        {
                            if (value)
                                RefreshAllAsync();
                            else
                                RefreshAll();
                        });
                }
            }
        }

        private async void RefreshAllAsync()
        {
            lock (this)
            {
                if (refresh || !enabled)
                    return;
                refresh = true;
            }

            EventHandler onEnabledChanged = null;
            onEnabledChanged = delegate
            {
                EnabledChanged -= onEnabledChanged;
                onEnabledChanged = null;
                refresh = false;
            };
            EnabledChanged += onEnabledChanged;

            await Task.Delay(500);

            if (onEnabledChanged != null)
            {
                EnabledChanged -= onEnabledChanged;

                lock (this)
                {
                    refresh = false;
                    if (enabled)
                        RefreshAll();
                }
            }
        }

        private void RefreshAll()
        {
            lock (this)
            {
                Refresh();

                foreach (var a in accounts.Values)
                {
                    Refresh(a);
                }
            }
        }

        private void Refresh()
        {
            registered = Register(Settings.Hotkeys.Value, registered);
        }

        private void Refresh(AccountHotkeys a)
        {
            a.Registered = Register(a.Hotkeys, a.Registered, a);
        }

        private Windows.Hotkeys.IHotkey[] Register(Settings.Hotkey[] hotkeys, Windows.Hotkeys.IHotkey[] existing, AccountHotkeys account = null)
        {
            if (hotkeys == null || !enabled)
            {
                Dispose(existing);
                return null;
            }

            var registered = new Windows.Hotkeys.IHotkey[hotkeys.Length];
            var count = 0;
            var isActive = account == null ? _ActiveAccounts > 0 : account.IsActive;

            for (var i = 0; i < hotkeys.Length;i++)
            {
                var flags = HotkeyInfo.GetInfo(hotkeys[i].Action).Flags;

                if (isActive)
                {
                    if ((flags & HotkeyFlags.OnlyWhenInactive) != 0)
                        continue;
                }
                else if ((flags & HotkeyFlags.OnlyWhenActive) != 0)
                    continue;

                try
                {
                    var r = this.hotkeys.Register(hotkeys[i].Keys);
                    r.Data = hotkeys[i];
                    if (account != null)
                        r.KeyPress += account.OnKeyPress;
                    else
                        r.KeyPress += OnKeyPress;
                    registered[count++] = r;
                }
                catch (Exception e)
                {
                    Util.Logging.Log("Failed to register hotkey " + Windows.Hotkeys.ToString(hotkeys[i].Keys));
                    Util.Logging.Log(e);
                }
            }

            if (registered.Length != count)
            {
                Array.Resize(ref registered, count);
            }

            Dispose(existing);

            return registered;
        }

        void OnKeyPress(object sender, Windows.Hotkeys.HotkeyEventArgs e)
        {
            if (Pressed != null)
                Pressed(this, new HotkeyPressEventArgs(e, (Settings.Hotkey)((Windows.Hotkeys.IHotkey)sender).Data, null));
        }

        void OnAccountHotkeyPressed(object sender, HotkeyPressEventArgs e)
        {
            if (Pressed != null)
                Pressed(this, e);
        }

        public void Process(ref System.Windows.Forms.Message m)
        {
            if (hotkeys != null)
                hotkeys.Process(ref m);
        }

        public bool IsRegistered(System.Windows.Forms.Keys keys)
        {
            return hotkeys.IsRegistered(keys);
        }

        public bool SendKeyPress(System.Windows.Forms.Keys hotkey, System.Windows.Forms.Keys keys)
        {
            return hotkeys.SendKeyPress(hotkey, keys);
        }

        private void Dispose(Windows.Hotkeys.IHotkey[] hotkeys)
        {
            if (hotkeys != null)
            {
                for (var i = 0; i < hotkeys.Length; i++)
                {
                    hotkeys[i].Dispose();
                }
            }
        }

        public void Dispose()
        {
            lock (this)
            {
                if (hotkeys != null)
                {
                    Settings.Accounts.ValueChanged -= Accounts_ValueChanged;
                    Settings.Accounts.ValueAdded -= Accounts_ValueAdded;
                    Settings.Accounts.ValueRemoved -= Accounts_ValueRemoved;
                    Settings.Hotkeys.ValueChanged -= Hotkeys_ValueChanged;
                    Client.Launcher.AccountProcessChanged -= Launcher_AccountProcessChanged;

                    using (hotkeys)
                    {
                        hotkeys = null;
                    }
                }

                if (accounts != null)
                {
                    foreach (var h in accounts.Values)
                    {
                        OnAccountRemoved(Settings.Accounts[h.Account.UID], h);
                    }

                    accounts = null;
                }
            }
        }

        public static Settings.Hotkey From(HotkeyAction action, System.Windows.Forms.Keys keys)
        {
            switch (action)
            {
                case HotkeyAction.RunProgram:
                    return new RunProgramHotkey(action, keys);
                case HotkeyAction.KeyPress:
                    return new KeyPressHotkey(action, keys);
            }
            return new Settings.Hotkey(action, keys);
        }
    }
}
