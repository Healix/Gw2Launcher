using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gw2Launcher.Tools.Markers
{
    class MarkerMonitor
    {
        private class MarkerData : IDisposable
        {
            [Flags]
            public enum TriggerType
            {
                None = 0,
                Date = 1,
                Mumble = 2,
            }

            public event EventHandler UsesMumbleChanged;
            public event EventHandler DateChanged;

            public MarkerData(Settings.AccountMarker marker)
            {
                this.Marker = marker;
                //this.Date = GetNext();

                OnTriggersChanged();

                marker.LastSetUtcChanged += marker_LastSetUtcChanged;
                marker.VisibleChanged += marker_VisibleChanged;
            }

            private void OnTriggersChanged()
            {
                var triggers = GetTriggers();
                var mumble = false;
                var d = DateTime.MaxValue;

                if (triggers != null)
                {
                    foreach (var t in triggers)
                    {
                        switch (GetType(t))
                        {
                            case TriggerType.Mumble:

                                mumble = true;

                                break;
                            case TriggerType.Date:

                                var tt = (Settings.MarkerTriggerTime)t;
                                var _d = tt.GetNext(this.Marker.LastSetUtc);
                                if (_d < d)
                                {
                                    d = _d;
                                }

                                break;
                        }
                    }
                }

                UsesMumble = mumble;
                Date = d;
            }

            void marker_VisibleChanged(object sender, EventArgs e)
            {
                OnTriggersChanged();
            }

            void marker_LastSetUtcChanged(object sender, EventArgs e)
            {
                Date = GetNext();
            }

            public Settings.AccountMarker Marker
            {
                get;
                private set;
            }

            private DateTime _Date;
            public DateTime Date
            {
                get
                {
                    return _Date;
                }
                set
                {
                    if (_Date != value)
                    {
                        _Date = value;
                        if (DateChanged != null)
                            DateChanged(this, EventArgs.Empty);
                    }
                }
            }

            private bool _UsesMumble;
            public bool UsesMumble
            {
                get
                {
                    return _UsesMumble;
                }
                private set
                {
                    if (_UsesMumble != value)
                    {
                        _UsesMumble = value;
                        if (UsesMumbleChanged != null)
                            UsesMumbleChanged(this, EventArgs.Empty);
                    }
                }
            }

            /// <summary>
            /// Returns the currently active triggers (while visible: triggers to hide, while hidden: triggers to show)
            /// </summary>
            private Settings.MarkerTriggerCondition[] GetTriggers()
            {
                return this.Marker.Visible ? this.Marker.Settings.TriggersHide : this.Marker.Settings.TriggersShow;
            }

            private TriggerType GetType(Settings.MarkerTriggerCondition[] triggers)
            {
                var type = TriggerType.None;

                if (triggers != null)
                {
                    foreach (var t in triggers)
                    {
                        type |= GetType(t);
                    }
                }

                return type;
            }

            private TriggerType GetType(Settings.MarkerTriggerCondition t)
            {
                switch (t.Type)
                {
                    case Settings.MarkerTriggerCondition.TriggerType.DayOfMonth:
                    case Settings.MarkerTriggerCondition.TriggerType.DayOfWeek:
                    case Settings.MarkerTriggerCondition.TriggerType.DayOfYear:
                    case Settings.MarkerTriggerCondition.TriggerType.DurationInMinutes:
                    case Settings.MarkerTriggerCondition.TriggerType.DurationInMinutesInterval:
                    case Settings.MarkerTriggerCondition.TriggerType.TimeOfDay:

                        return TriggerType.Date;

                    case Settings.MarkerTriggerCondition.TriggerType.Map:
                    case Settings.MarkerTriggerCondition.TriggerType.MapCoordinate:

                        return TriggerType.Mumble;
                }

                return TriggerType.None;
            }

            public DateTime GetNext()
            {
                var triggers = GetTriggers();
                var d = DateTime.MaxValue;

                if (triggers != null)
                {
                    foreach (var t in triggers)
                    {
                        if (GetType(t) == TriggerType.Date)
                        {
                            var tt = (Settings.MarkerTriggerTime)t;
                            var _d = tt.GetNext(this.Marker.LastSetUtc);
                            if (_d < d)
                            {
                                d = _d;
                            }
                        }
                    }
                }

                return d;
            }

            public void Dispose()
            {
                var m = this.Marker;
                if (m != null)
                {
                    this.Marker = null;

                    m.LastSetUtcChanged -= marker_LastSetUtcChanged;
                    m.VisibleChanged -= marker_VisibleChanged;
                }
            }
        }

        private class AccountData : IDisposable
        {
            public event EventHandler DateChanged;
            public event EventHandler UsesMumbleChanged;

            private MarkerData[] markers;
            private Settings.AccountMarker[] _markers;
            private bool refresh;
            private MarkerData first;

            public AccountData(Settings.IAccount account)
            {
                this.Account = account;

                SetMarkers(account.Markers);
            }

            private void SetMarkers(Settings.AccountMarker[] markers)
            {
                this._markers = markers;

                if (this.markers != null)
                {
                    foreach (var m in this.markers)
                    {
                        using (m)
                        {
                            m.DateChanged -= m_DateChanged;
                            m.UsesMumbleChanged -= m_UsesMumbleChanged;
                        }
                    }
                    this.markers = null;
                }

                if (markers == null)
                {
                    this.first = null;
                }
                else
                {
                    var _markers = new MarkerData[markers.Length];
                    var d = DateTime.MaxValue;
                    MarkerData first = null;

                    for (var i = _markers.Length - 1; i >= 0; --i)
                    {
                        var m = new MarkerData(markers[i]);
                        m.DateChanged += m_DateChanged;
                        m.UsesMumbleChanged += m_UsesMumbleChanged;
                        _markers[i] = m;

                        if (m.Date < d)
                        {
                            first = m;
                            d = m.Date;
                        }
                    }

                    this.markers = _markers;
                    this.first = first;
                }
            }

            void m_UsesMumbleChanged(object sender, EventArgs e)
            {

            }

            void m_DateChanged(object sender, EventArgs e)
            {
                var m = (MarkerData)sender;

                if (first == null || object.ReferenceEquals(m, first) || first.Date > m.Date)
                {
                    RefreshAsync();
                }
            }

            public Settings.IAccount Account
            {
                get;
                private set;
            }

            public bool IsActive
            {
                get;
                set;
            }

            public bool HasMarkers
            {
                get;
                set;
            }

            public bool HasMumble
            {
                get;
                set;
            }

            private DateTime _Date;
            public DateTime Date
            {
                get
                {
                    return _Date;
                }
                set
                {
                    if (_Date != value)
                    {
                        _Date = value;
                        if (DateChanged != null)
                            DateChanged(this, EventArgs.Empty);
                    }
                }
            }

            private bool _UsesMumble;
            public bool UsesMumble
            {
                get
                {
                    return _UsesMumble;
                }
                private set
                {
                    if (_UsesMumble != value)
                    {
                        _UsesMumble = value;
                        if (UsesMumbleChanged != null)
                            UsesMumbleChanged(this, EventArgs.Empty);
                    }
                }
            }

            public Settings.AccountMarker[] Markers
            {
                get
                {
                    return _markers;
                }
                set
                {
                    SetMarkers(value);
                }
            }

            private void Refresh()
            {
                var markers = this.markers;
                if (markers == null)
                    return;

                foreach (var m in markers)
                {
                    
                }
            }

            private async void RefreshAsync()
            {
                if (refresh)
                    return;
                refresh = true;

                await Task.Delay(500);

                refresh = false;

                //var markers = this.markers;
                //if (markers != null)
                //{
                //    foreach (var m in markers)
                //    {
                //        m.LastSetUtc
                //    }
                //}
            }

            //public DateTime Process(DateTime t)
            //{
            //    if (refresh)
            //    {
            //        //resync
            //    }

            //    if (first != null)
            //    {
            //        if (t < first.Date && !_UsesMumble)
            //            return first.Date;
            //    }
            //    else if (!_UsesMumble)
            //    {
            //        return DateTime.MaxValue;
            //    }
            //}

            public void Dispose()
            {
                SetMarkers(null);
            }
        }

        private Dictionary<ushort, AccountData> accounts;
        private List<int> active;

        public MarkerMonitor()
        {
            var sq = new Util.SortedQueue<int, string>();

            Settings.Accounts.ValueChanged += Accounts_ValueChanged;
            Settings.Accounts.ValueAdded += Accounts_ValueAdded;
            Settings.Accounts.ValueRemoved += Accounts_ValueRemoved;

            Client.Launcher.AccountStateChanged += Launcher_AccountStateChanged;
        }

        private async void DoMonitor()
        {
            //loop through accounts with active markers
            //each account should return when the next update should be
            //minimum 1s delay on polling accounts
        }

        private void Refresh()
        {
            lock (accounts)
            {
                foreach (var uid in accounts.Keys)
                {
                    var a = accounts[uid];
                    if (a.HasMarkers)
                    {

                    }
                }
            }
        }

        private void RefreshAsync()
        {
            //build list of accounts with active markers
        }

        void Launcher_AccountStateChanged(Settings.IAccount account, Client.Launcher.AccountStateEventArgs e)
        {
            if (e.State == Client.Launcher.AccountState.ActiveGame || e.PreviousState == Client.Launcher.AccountState.ActiveGame)
            {
                lock (accounts)
                {
                    AccountData a;
                    if (accounts.TryGetValue(account.UID, out a))
                    {
                        a.IsActive = e.State == Client.Launcher.AccountState.ActiveGame;
                    }
                }
            }
        }

        private void OnAccountAdded(Settings.ISettingValue<Settings.IAccount> v, Settings.IAccount account)
        {
            v.ValueCleared += Account_ValueCleared;
            account.MarkersChanged += account_MarkersChanged;

            lock (this)
            {
                AccountData a;

                accounts[account.UID] = a = new AccountData(account);

                if (account.Markers != null)
                {
                    a.HasMarkers = true;
                    RefreshAsync();
                }
            }
        }

        private void OnAccountRemoved(Settings.ISettingValue<Settings.IAccount> v, Settings.IAccount account)
        {
            if (v != null)
                v.ValueCleared -= Account_ValueCleared;

            if (account != null)
            {
                account.MarkersChanged -= account_MarkersChanged;
            }
        }

        void account_MarkersChanged(object sender, EventArgs e)
        {
            var a = (Settings.IAccount)sender;
            var v = a.Markers;
            var d = accounts[a.UID];

            if (v != null)
            {
                d.HasMarkers = true;
                RefreshAsync();
            }
            else if (d.HasMarkers)
            {
                d.HasMarkers = false;
                RefreshAsync();
            }
        }

        void Accounts_ValueRemoved(object sender, ushort uid)
        {
            lock (this)
            {
                AccountData d;
                if (accounts.TryGetValue(uid, out d))
                {
                    OnAccountRemoved(sender as Settings.ISettingValue<Settings.IAccount>, d.Account);
                    accounts.Remove(uid);

                    if (d.HasMarkers)
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
    }
}
