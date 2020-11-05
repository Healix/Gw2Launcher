using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gw2Launcher.Tools
{
    class JumpList : IDisposable
    {
        private class JumpItem : Windows.JumpList.IJumpTask
        {
            public JumpItem(Settings.IAccount account)
            {
                this.Account = account;
            }

            public Settings.IAccount Account
            {
                get;
                set;
            }

            public bool Visible
            {
                get;
                set;
            }

            public bool IsActive
            {
                get;
                set;
            }

            public string ApplicationPath
            {
                get
                {
                    return System.Reflection.Assembly.GetExecutingAssembly().Location;
                }
            }

            public string Arguments
            {
                get
                {
                    return "-l:silent -l:uid:" + Account.UID;
                }
            }

            public string Description
            {
                get
                {
                    return null;
                }
            }

            public string IconResourcePath
            {
                get
                {
                    return ApplicationPath;
                }
            }

            public int IconResourceIndex
            {
                get
                {
                    return 0;
                }
            }

            public string Title
            {
                get
                {
                    return Account.Name;
                }
            }

            public string WorkingDirectory
            {
                get
                {
                    return null;
                }
            }

            public string CustomCategory
            {
                get
                {
                    return customCategory;
                }
            }
        }

        private class JumpItemComparer : Util.AccountComparer, IComparer<JumpItem>
        {
            public JumpItemComparer(Settings.SortingOptions options)
                : base(options)
            {
            }

            public int Compare(JumpItem a, JumpItem b)
            {
                return base.Compare(a.Account, b.Account);
            }
        }

        private static string customCategory = "Accounts";

        private Dictionary<ushort, JumpItem> accounts;
        private Windows.JumpList jumplist;
        private bool refresh;

        public JumpList(IntPtr window)
        {
            accounts = new Dictionary<ushort, JumpItem>();

            jumplist = new Windows.JumpList(window);
            jumplist.Removed += jumplist_Removed;

            Settings.Accounts.ValueChanged += Accounts_ValueChanged;
            Settings.Accounts.ValueAdded += Accounts_ValueAdded;
            Settings.Accounts.ValueRemoved += Accounts_ValueRemoved;
            Settings.Sorting.ValueChanged += Sorting_ValueChanged;
            Settings.JumpList.ValueChanged += JumpList_ValueChanged;
            Client.Launcher.AccountLaunched += Launcher_AccountLaunched;
            Client.Launcher.AccountExited += Launcher_AccountExited;

            lock (this)
            {
                foreach (var uid in Settings.Accounts.GetKeys())
                {
                    var a = Settings.Accounts[uid];
                    if (a.HasValue)
                    {
                        var account = a.Value;
                        var j = new JumpItem(account);

                        accounts[account.UID] = j;

                        j.IsActive = Client.Launcher.IsActive(account);

                        OnAccountAdded(a, account);
                    }
                }
            }

            if (accounts.Count > 0)
            {
                RefreshAsync();
            }
        }

        private void OnAccountAdded(Settings.ISettingValue<Settings.IAccount> v, Settings.IAccount account)
        {
            v.ValueCleared += Account_ValueCleared;

            account.NameChanged += account_NameChanged;
            account.LastUsedUtcChanged += account_LastUsedUtcChanged;
            account.JumpListPinningChanged += account_JumpListPinningChanged;
        }

        private void OnAccountRemoved(Settings.ISettingValue<Settings.IAccount> v, Settings.IAccount account)
        {
            if (v != null)
                v.ValueCleared -= Account_ValueCleared;

            if (account != null)
            {
                account.NameChanged -= account_NameChanged;
                account.LastUsedUtcChanged -= account_LastUsedUtcChanged;
                account.JumpListPinningChanged -= account_JumpListPinningChanged;
            }
        }

        private bool IsEnabled(JumpItem j)
        {
            if (j.Account.JumpListPinning == Settings.JumpListPinning.Disabled)
                return false;

            var v = Settings.JumpList.Value;

            if ((v & Settings.JumpListOptions.OnlyShowDaily) == Settings.JumpListOptions.OnlyShowDaily)
            {
                if (j.IsActive || j.Account.LastUsedUtc.Date == DateTime.UtcNow.Date)
                    return false;
            }
            else if ((v & Settings.JumpListOptions.OnlyShowInactive) == Settings.JumpListOptions.OnlyShowInactive)
            {
                return !j.IsActive;
            }

            return true;
        }

        void Launcher_AccountExited(Settings.IAccount account)
        {
            lock (this)
            {
                JumpItem j;
                if (accounts.TryGetValue(account.UID, out j))
                {
                    j.IsActive = false;

                    if (j.Visible != IsEnabled(j))
                    {
                        RefreshAsync();
                    }
                }
            }
        }

        void Launcher_AccountLaunched(Settings.IAccount account)
        {
            lock (this)
            {
                JumpItem j;
                if (accounts.TryGetValue(account.UID, out j))
                {
                    j.IsActive = true;

                    if (j.Visible != IsEnabled(j))
                    {
                        RefreshAsync();
                    }
                }
            }
        }

        void JumpList_ValueChanged(object sender, EventArgs e)
        {
            var v = Settings.JumpList;
            if (v.HasValue && v.Value.HasFlag(Settings.JumpListOptions.Enabled))
            {
                RefreshAsync();
            }
        }

        void Sorting_ValueChanged(object sender, EventArgs e)
        {
            RefreshAsync();
        }

        void jumplist_Removed(object sender, IList<Windows.JumpList.IJumpItem> e)
        {
            foreach (var i in e)
            {
                if (i is JumpItem)
                {
                    var j = (JumpItem)i;

                    j.Account.JumpListPinning = Settings.JumpListPinning.Disabled;
                    j.Visible = false;
                }
            }
        }

        void Accounts_ValueRemoved(object sender, ushort uid)
        {
            lock (this)
            {
                JumpItem j;
                if (accounts.TryGetValue(uid, out j))
                {
                    OnAccountRemoved(sender as Settings.ISettingValue<Settings.IAccount>, j.Account);
                    accounts.Remove(uid);

                    if (j.Visible)
                    {
                        RefreshAsync();
                    }
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
                var account = e.Value.Value;

                if (!accounts.ContainsKey(account.UID))
                {
                    var j = new JumpItem(account);
                    
                    accounts[account.UID] = j;
                    OnAccountAdded(e.Value, account);

                    if (IsEnabled(j))
                    {
                        RefreshAsync();
                    }
                }
            }
        }

        void Accounts_ValueChanged(object sender, KeyValuePair<ushort, Settings.ISettingValue<Settings.IAccount>> e)
        {
            lock (this)
            {
                JumpItem j;
                if (accounts.TryGetValue(e.Key, out j))
                {
                    OnAccountRemoved(null, j.Account);
                    accounts.Remove(e.Key);

                    if (j.Visible)
                    {
                        RefreshAsync();
                    }
                }

                if (e.Value.HasValue)
                {
                    Accounts_ValueAdded(sender, e);
                }
            }
        }

        void account_JumpListPinningChanged(object sender, EventArgs e)
        {
            lock (this)
            {
                var a = (Settings.IAccount)sender;

                JumpItem j;
                if (accounts.TryGetValue(a.UID, out j))
                {
                    if (j.Visible != IsEnabled(j))
                        RefreshAsync();
                }
            }
        }

        void account_NameChanged(object sender, EventArgs e)
        {
            lock (this)
            {
                var a = (Settings.IAccount)sender;

                JumpItem j;
                if (accounts.TryGetValue(a.UID, out j))
                {
                    if (j.Visible)
                        RefreshAsync();
                }
            }
        }

        void account_LastUsedUtcChanged(object sender, EventArgs e)
        {
            if (Settings.Sorting.Value.Sorting.Mode == Settings.SortMode.LastUsed || (Settings.JumpList.Value & Settings.JumpListOptions.OnlyShowDaily) == Settings.JumpListOptions.OnlyShowDaily)
            {
                lock (this)
                {
                    var a = (Settings.IAccount)sender;

                    JumpItem j;
                    if (accounts.TryGetValue(a.UID, out j))
                    {
                        if ((Settings.JumpList.Value & Settings.JumpListOptions.OnlyShowDaily) == Settings.JumpListOptions.OnlyShowDaily)
                        {
                            if (j.Visible != IsEnabled(j))
                            {
                                RefreshAsync();
                            }
                        }
                        else if (j.Visible && Settings.Sorting.Value.Sorting.Mode == Settings.SortMode.LastUsed)
                        {
                            RefreshAsync();
                        }
                    }
                }
            }
        }

        public async void RefreshAsync()
        {
            lock (this)
            {
                if (refresh)
                    return;
                refresh = true;
            }

            await Task.Delay(500);

            lock (this)
            {
                refresh = false;
                Refresh();
            }
        }

        private void Refresh()
        {
            if (jumplist == null)
                return;

            jumplist.JumpItems.Clear();

            var l = new List<JumpItem>(accounts.Count);

            foreach (var j in accounts.Values)
            {
                if (j.Visible = IsEnabled(j))
                {
                    l.Add(j);
                }
            }

            l.Sort(new JumpItemComparer(Settings.Sorting.Value));

            jumplist.JumpItems.AddRange(l);

            try
            {
                jumplist.Apply();
            }
            catch (Windows.JumpList.CustomCategoryException e)
            {
                if (customCategory != null)
                {
                    customCategory = null;
                    Refresh();
                }
                else
                {
                    Util.Logging.Log(e);
                }
            }
            catch (Exception e)
            {
                Util.Logging.Log(e);
            }
        }

        public void Clear()
        {
            lock (this)
            {
                jumplist.JumpItems.Clear();

                try
                {
                    jumplist.Apply();
                }
                catch (Windows.JumpList.CustomCategoryException e)
                {
                    if (customCategory != null)
                    {
                        customCategory = null;
                        Clear();
                    }
                    else
                    {
                        Util.Logging.Log(e);
                    }
                }
                catch (Exception e)
                {
                    Util.Logging.Log(e);
                }
            }
        }

        public void Dispose()
        {
            Dispose(false);
        }

        public void Dispose(bool clear)
        {
            lock (this)
            {
                if (clear)
                {
                    Clear();
                }

                if (accounts != null)
                {
                    Settings.Accounts.ValueChanged -= Accounts_ValueChanged;
                    Settings.Accounts.ValueAdded -= Accounts_ValueAdded;
                    Settings.Accounts.ValueRemoved -= Accounts_ValueRemoved;
                    Settings.Sorting.ValueChanged -= Sorting_ValueChanged;
                    Settings.JumpList.ValueChanged -= JumpList_ValueChanged;
                    Client.Launcher.AccountLaunched -= Launcher_AccountLaunched;
                    Client.Launcher.AccountExited -= Launcher_AccountExited;

                    foreach (var uid in accounts.Keys)
                    {
                        var a = Settings.Accounts[uid];
                        OnAccountRemoved(a, a.Value);
                    }

                    accounts = null;
                }

                jumplist = null;
            }
        }
    }
}
