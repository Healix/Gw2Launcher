using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Gw2Launcher.Api;

namespace Gw2Launcher.Tools.Api
{
    public class ApiRequestManager
    {
        [Flags]
        public enum RequestReason
        {
            None = 0,
            /// <summary>
            /// Queues everything
            /// </summary>
            Update = 1,
            /// <summary>
            /// Queues data that has no value
            /// </summary>
            Initial = 2,
            /// <summary>
            /// Queues data flagged as pending or has no value
            /// </summary>
            Pending = 3,
            /// <summary>
            /// Queues data that changes at daily reset (unused)
            /// </summary>
            DailyReset = 4,
            VaultOpened = 8,
            VaultClosed = 16,
        }

        public class DataRequest : ApiData.DataRequest
        {
            public DataRequest(ApiData.DataType type, Settings.IAccount account, Settings.ApiDataKey key, RequestOptions options = RequestOptions.None)
                : base(type, key, options)
            {
                this.Account = account;
            }

            public Settings.IAccount Account
            {
                get;
                private set;
            }

            public RequestReason Reason
            {
                get;
                set;
            }

            public object Tag
            {
                get;
                set;
            }
        }

        public class DataAvailableEventArgs : EventArgs
        {
            private ApiData.DataAvailableEventArgs source;

            public DataAvailableEventArgs(ApiData.DataAvailableEventArgs source, Settings.IAccount account, RequestReason reasons, IList<DataRequest> requests)
            {
                this.source = source;
                this.Account = account;
                this.Reasons = reasons;
                this.Requests = requests;
            }

            /// <summary>
            /// API key
            /// </summary>
            public Settings.ApiDataKey Key
            {
                get
                {
                    return (Settings.ApiDataKey)source.Key;
                }
            }

            /// <summary>
            /// API type
            /// </summary>
            public ApiData.DataType Type
            {
                get
                {
                    return source.Type;
                }
            }

            /// <summary>
            /// Response status
            /// </summary>
            public ApiData.DataStatus Status
            {
                get
                {
                    return source.Status;
                }
            }

            public ApiData.IData Data
            {
                get
                {
                    return source.Data;
                }
            }

            /// <summary>
            /// When the data was last modified on the server
            /// </summary>
            public DateTime LastModified
            {
                get
                {
                    return source.LastModified;
                }
            }

            /// <summary>
            /// When the data was last modified, adjusted to the local clock
            /// </summary>
            public DateTime LastModifiedInLocalTime
            {
                get
                {
                    return source.LastModifiedInLocalTime;
                }
            }

            /// If the LastModified date changed since the last update. LastModified is only accurate to the minute, allowing for the data to change while having the same timestamp
            public bool Modified
            {
                get
                {
                    return source.Modified;
                }
            }

            /// <summary>
            /// When the next API request can be made
            /// </summary>
            public DateTime NextRequest
            {
                get
                {
                    return source.NextRequest;
                }
            }

            public Settings.IAccount Account
            {
                get;
                private set;
            }

            public RequestReason Reasons
            {
                get;
                private set;
            }

            /// <summary>
            /// Requests for this data, in the order they were made
            /// </summary>
            public IList<DataRequest> Requests
            {
                get;
                private set;
            }

            public bool Contains(RequestReason reasons)
            {
                return (Reasons & reasons) == reasons;
            }

            public DataRequest GetFirst(RequestReason reason)
            {
                for (int i = 0, count = Requests.Count; i < count; i++)
                {
                    if (Requests[i].Reason == reason)
                    {
                        return Requests[i];
                    }
                }
                return null;
            }

            public DataRequest GetLast(RequestReason reason)
            {
                for (int i = Requests.Count-1; i >= 0; --i)
                {
                    if (Requests[i].Reason == reason)
                    {
                        return Requests[i];
                    }
                }
                return null;
            }

            public DataRequest[] GetReasons(RequestReason reasons)
            {
                var requests = new DataRequest[1];
                var j = 0;

                for (int i = 0, count = Requests.Count; i < count; i++)
                {
                    if ((Requests[i].Reason & reasons) != 0)
                    {
                        if (j == requests.Length)
                        {
                            Array.Resize(ref requests, j + 1);
                        }
                        requests[j++] = Requests[i];
                    }
                }

                if (j == 0)
                    return null;
                return requests;
            }

            public IEnumerable<DataRequest> GetReasonsEnumerable(RequestReason reasons)
            {
                for (int i = 0, count = Requests.Count; i < count; i++)
                {
                    if ((Requests[i].Reason & reasons) != 0)
                    {
                        yield return Requests[i];
                    }
                }
            }

            public void Repeat(DataRequest r)
            {
                if (Repeating == null)
                {
                    Repeating = new HashSet<DataRequest>();
                }
                Repeating.Add(r);
            }

            public ICollection<DataRequest> Repeating
            {
                get;
                private set;
            }

            public Exception GetException()
            {
                return source.GetException();
            }
        }

        public event EventHandler<DataAvailableEventArgs> DataAvailable;

        private ApiData api;
        private Queue<ApiData.RequestDataAvailableEventArgs> pending;

        public ApiRequestManager(ApiData api)
        {
            pending = new Queue<ApiData.RequestDataAvailableEventArgs>();

            this.api = api;
            api.EndDataAvailable += OnEndDataAvailable;
        }

        public ApiData DataSource
        {
            get
            {
                return api;
            }
        }

        public void Queue(params DataRequest[] r)
        {
            for (var i = 0; i < r.Length; i++)
            {
                if (r[i] == null)
                {
                    break;
                }

                r[i].DataAvailable += OnDataAvailable;

                api.Queue(r[i]);
            }
        }

        public void Queue(Settings.IGw2Account account, RequestReason reason)
        {
            Queue(account, account.Api, reason);
        }

        /// <summary>
        /// Queues API requests applicable for the given reason
        /// </summary>
        /// <param name="account">Account that initiated the request</param>
        /// <param name="k">API key/data</param>
        /// <param name="reason">Reason for the request</param>
        public void Queue(Settings.IGw2Account account, Settings.ApiDataKey k, RequestReason reason)
        {
            if (account.ApiTracking == Settings.ApiTracking.None)
                return;

            if (reason == RequestReason.VaultOpened && (k.Permissions & TokenInfo.Permissions.Progression) != 0)
            {
                foreach (var r in api.GetRequests(k.Key))
                {
                    if (r is DataRequest)
                    {
                        if (((DataRequest)r).Reason == RequestReason.VaultClosed)
                        {
                            Util.Logging.LogEvent(account, "Aborting previous [" + r.Type + "] for [VaultClosed]");
                            r.Abort();
                        }
                    }
                }
            }

            var types = GetTypes(k, reason);

            if (types != null)
            {
                var options = ApiData.DataRequest.RequestOptions.None;
                var date = DateTime.UtcNow;
                var requests = new DataRequest[types.Length];
                var delay = DateTime.MinValue;

                switch (reason)
                {
                    case RequestReason.VaultClosed:

                        //the API updates every 5m and on map change
                        options = ApiData.DataRequest.RequestOptions.NoCache | ApiData.DataRequest.RequestOptions.Delay | ApiData.DataRequest.RequestOptions.IgnoreDelayIfModified;
                        delay = api.GetNextEstimatedUpdate(k.Key);
                        if (delay == DateTime.MinValue)
                            delay = DateTime.UtcNow.AddMinutes(1);

                        SetState(types, k, Settings.ApiCacheState.Pending);

                        break;
                    case RequestReason.Update:

                        options = ApiData.DataRequest.RequestOptions.NoCache | ApiData.DataRequest.RequestOptions.Delay | ApiData.DataRequest.RequestOptions.IgnoreDelayIfModified;
                        delay = DateTime.UtcNow.AddSeconds(30);

                        SetState(types, k, Settings.ApiCacheState.Pending);

                        break;
                    case RequestReason.Pending:

                        options = ApiData.DataRequest.RequestOptions.NoCache;

                        break;
                }

                for (var i = 0; i < types.Length; i++)
                {
                    var r = requests[i] = new DataRequest(types[i], account, k, options)
                    {
                        Reason = reason,
                        Date = date,
                        Delay = delay,
                    };
                    r.DataAvailable += OnDataAvailable;
                }

                api.Queue(requests);
            }
        }

        private void SetState(ApiData.DataType[] types, Settings.ApiDataKey k, Settings.ApiCacheState state)
        {
            const int 
                ACCOUNT = 0,
                WALLET = 1;

            var data = new Settings.IApiValue[]
            {
                k.Data.Account,
                k.Data.Wallet,
            };

            var b = new bool[data.Length];

            for (var i = 0; i < types.Length; i++)
            {
                switch (types[i])
                {
                    case ApiData.DataType.Account:
                    case ApiData.DataType.VaultDaily:
                    case ApiData.DataType.VaultWeekly:

                        b[ACCOUNT] = true;

                        break;
                    case ApiData.DataType.Wallet:

                        b[WALLET] = true;

                        break;
                }
            }

            for (var i = 0; i < data.Length;i++)
            {
                if (b[i] && data[i] != null)
                {
                    if (data[i].State != Settings.ApiCacheState.None)
                    {
                        data[i].State = state;
                    }
                }
            }
        }

        public static ApiData.DataRequest.RequestOptions GetOptions(RequestReason reason)
        {
            switch (reason)
            {
                case RequestReason.DailyReset:
                case RequestReason.Initial:
                case RequestReason.VaultOpened:

                    return ApiData.DataRequest.RequestOptions.None;
            }

            return ApiData.DataRequest.RequestOptions.NoCache;
        }

        private static bool IsState(Settings.IApiValue v, Settings.ApiCacheState state)
        {
            return v != null && (v.State == state || state == Settings.ApiCacheState.Pending && v.State == Settings.ApiCacheState.None);
        }

        public static ApiData.DataType[] GetTypes(Settings.ApiDataKey k, RequestReason reason)
        {
            if (k == null || k.Permissions == TokenInfo.Permissions.None || k.Tracking == Settings.ApiTracking.None)
                return null;

            var all = GetTracking(k, Settings.ApiTracking.All);
            const int ACCOUNT = 0,
                      INFO = 1,
                      WALLET = 2,
                      VAULT_DAILY = 3,
                      VAULT_WEEKLY = 4;

            var d = new ApiData.DataType[] 
            { 
                ApiData.DataType.Account, 
                ApiData.DataType.TokenInfo, 
                ApiData.DataType.Wallet, 
                ApiData.DataType.VaultDaily, 
                ApiData.DataType.VaultWeekly 
            };

            var t = new Settings.ApiTracking[d.Length];

            if (reason == RequestReason.Initial || reason == RequestReason.Pending)
            {
                Settings.ApiCacheState state;

                if (reason == RequestReason.Initial)
                {
                    state = Settings.ApiCacheState.None;
                }
                else
                {
                    state = Settings.ApiCacheState.Pending;
                }

                if ((k.Permissions & TokenInfo.Permissions.Unknown) != 0)
                {
                    t[INFO] = Settings.ApiTracking.All;
                }

                if (IsState(k.Data.Wallet, state))
                {
                    t[WALLET] = Settings.ApiTracking.Wallet;
                }

                if (IsState(k.Data.Account, state))
                {
                    t[ACCOUNT] = Settings.ApiTracking.Account;

                    t[VAULT_DAILY] = all & Settings.ApiTracking.Daily;
                    t[VAULT_WEEKLY] = all & Settings.ApiTracking.Weekly;
                }
            }
            else
            {
                //var ap = points == null ? ushort.MaxValue : points.Value;
                
                t[ACCOUNT] = all & Settings.ApiTracking.Login;

                if ((k.Permissions & TokenInfo.Permissions.Wallet) != 0)
                {
                    switch (reason)
                    {
                        case RequestReason.VaultClosed:

                            //wallet changes are used to detect if the daily/weekly should be queried
                            t[WALLET] = all & (Settings.ApiTracking.Astral | Settings.ApiTracking.Daily | Settings.ApiTracking.Weekly);

                            break;
                        case RequestReason.Update:

                            t[WALLET] = all & Settings.ApiTracking.Astral;

                            break;
                    }
                }

                if ((k.Permissions & TokenInfo.Permissions.Progression) != 0)
                {
                    switch (reason)
                    {
                        case RequestReason.VaultClosed:
                            
                            if (t[WALLET] == 0)
                            {
                                if ((all & Settings.ApiTracking.Daily) != 0)
                                {
                                    //wallet isn't available to detect astral changes
                                    //use ap if under the cap, otherwise the vault will always be requested

                                    var a = k.Data.Account;
                                    var ap = a == null ? ushort.MaxValue : a.Value.DailyPoints;

                                    if (ap > 0 && ap < Account.MAX_AP)
                                        t[ACCOUNT] |= Settings.ApiTracking.Daily;
                                    else
                                        t[VAULT_DAILY] = Settings.ApiTracking.Daily;
                                }
                                t[VAULT_WEEKLY] = all & Settings.ApiTracking.Weekly;
                            }

                            break;
                        case RequestReason.Update:

                            t[VAULT_DAILY] = all & Settings.ApiTracking.Daily;
                            t[VAULT_WEEKLY] = all & Settings.ApiTracking.Weekly;

                            break;
                    }
                }
                else if ((k.Permissions & TokenInfo.Permissions.Wallet) != 0)
                {
                    //the wallet can alternatively be used to estimate completion
                    //needs to be requested before and after using the vault

                    t[WALLET] |= all & (Settings.ApiTracking.Daily | Settings.ApiTracking.Weekly);
                }

                //no longer need to track AP for daily completion

                //if (ap >= Gw2Launcher.Api.Account.MAX_AP)
                //{
                //    //unable to track daily via ap; not allowing tracking via ap
                //    if ((t[ACCOUNT] & Settings.ApiTracking.Daily) != 0)
                //    {
                //        t[ACCOUNT] &= ~Settings.ApiTracking.Daily;
                //    }
                //}
                //else if (ap > 0)
                //{
                //    //able to track daily via ap; not allowing tracking via wallet
                //    if ((t[WALLET] & Settings.ApiTracking.Daily) != 0)
                //    {
                //        t[WALLET] &= ~Settings.ApiTracking.Daily;
                //    }
                //}

                //if (t[WALLET] != 0)
                //{
                //    switch (reason)
                //    {
                //        case RequestReason.VaultOpened:

            }


            //var doWallet = false;
            //var doAccount = false;
            //var count = 0;



            var count = 0;

            for (var i = 0; i < t.Length; i++)
            {
                if (t[i] != 0)
                {
                    if (k.Permissions != TokenInfo.Permissions.Unknown)
                    {
                        var p = ApiData.GetPermissions(d[i]);

                        if ((k.Permissions & p) != p)
                        {
                            t[i] = 0;
                            continue;
                        }
                    }

                    ++count;
                }
            }

            if (count > 0)
            {
                if (t[ACCOUNT] == 0)
                {
                    //account is always requested if anything else is
                    t[ACCOUNT] = Settings.ApiTracking.Account;
                    ++count;
                }

                if (count == t.Length)
                {
                    return d;
                }
                else
                {
                    var types = new ApiData.DataType[count];

                    for (var i = t.Length - 1; i >= 0; --i)
                    {
                        if (t[i] != 0)
                        {
                            types[--count] = d[i];
                            if (count == 0)
                                break;
                        }
                    }

                    return types;
                }
            }

            return null;
        }

        public static Settings.ApiTracking GetTracking(Settings.ApiTracking[] tracking)
        {
            if (tracking.Length == 1)
            {
                return tracking[0];
            }
            else
            {
                var t = Settings.ApiTracking.None;

                for (var i = 0; i < tracking.Length; i++)
                {
                    t |= tracking[i];
                }

                return t;
            }
        }
        
        public struct TrackingSummary
        {
            public Settings.ApiTracking Summary;
            public Settings.ApiTracking[] Tracking;

            public bool Contains(Settings.ApiTracking t)
            {
                return (Summary & t) == t;
            }

            public bool Contains(int i, Settings.ApiTracking t)
            {
                return (Tracking[i] & t) == t;
            }

            public bool ContainsAny(Settings.ApiTracking t)
            {
                return (Summary & t) != 0;
            }
        }

        public static TrackingSummary GetTracking(Settings.IGw2Account[] accounts, Settings.ApiTracking types)
        {
            var t = new TrackingSummary()
            {
                Tracking = new Settings.ApiTracking[accounts.Length],
            };

            for (var i = 0; i < accounts.Length; i++)
            {
                t.Tracking[i] = GetTracking(accounts[i], types);
                t.Summary |= t.Tracking[i];
            }

            return t;
        }

        public static Settings.ApiTracking GetTracking(Settings.IGw2Account a, Settings.ApiTracking types)
        {
            var t = Settings.ApiTracking.None;

            if (a.Api == null)
            {
                return t;
            }

            types &= a.ApiTracking;

            if ((types & Settings.ApiTracking.Astral) != 0)
            {
                if (a.ShowAstral)
                {
                    t |= Settings.ApiTracking.Astral;
                }
            }

            if ((types & Settings.ApiTracking.Weekly) != 0)
            {
                if (a.ShowWeeklyCompletion && DateTime.UtcNow >= Util.Date.GetNextWeek(a.LastWeeklyCompletionUtc, DayOfWeek.Monday, 7, 30))
                {
                    t |= Settings.ApiTracking.Weekly;
                }
            }

            if ((types & Settings.ApiTracking.Daily) != 0)
            {
                if (a.ShowDailyCompletion && DateTime.UtcNow.Date != a.LastDailyCompletionUtc.Date)
                {
                    t |= Settings.ApiTracking.Daily;
                }
            }

            if ((types & Settings.ApiTracking.Login) != 0)
            {
                if (a.ShowDailyLogin && DateTime.UtcNow.Date != a.LastDailyLoginUtc.Date)
                {
                    t |= Settings.ApiTracking.Login;
                }
            }

            return t;
        }

        public static Settings.ApiTracking GetTracking(Settings.ApiDataKey k, Settings.ApiTracking types)
        {
            var t = Settings.ApiTracking.None;
            Settings.IGw2Account[] a;

            if (k == null || (a = k.Accounts) == null)
            {
                return t;
            }

            types &= k.Tracking;

            if ((types & Settings.ApiTracking.Astral) != 0)
            {
                for (var i = 0; i < a.Length; i++)
                {
                    if (a[i].ShowAstral)
                    {
                        t |= Settings.ApiTracking.Astral;

                        break;
                    }
                }
            }

            if ((types & Settings.ApiTracking.Weekly) != 0)
            {
                for (var i = 0; i < a.Length; i++)
                {
                    if (a[i].ShowWeeklyCompletion && DateTime.UtcNow >= Util.Date.GetNextWeek(a[i].LastWeeklyCompletionUtc, DayOfWeek.Monday, 7, 30))
                    {
                        t |= Settings.ApiTracking.Weekly;

                        break;
                    }
                }
            }

            if ((types & Settings.ApiTracking.Daily) != 0)
            {
                for (var i = 0; i < a.Length; i++)
                {
                    if (a[i].ShowDailyCompletion && DateTime.UtcNow.Date != a[i].LastDailyCompletionUtc.Date)
                    {
                        t |= Settings.ApiTracking.Daily;

                        break;
                    }
                }
            }

            if ((types & Settings.ApiTracking.Login) != 0)
            {
                for (var i = 0; i < a.Length; i++)
                {
                    if (a[i].ShowDailyLogin && DateTime.UtcNow.Date != a[i].LastDailyLoginUtc.Date)
                    {
                        t |= Settings.ApiTracking.Login;

                        break;
                    }
                }
            }

            return t;
        }

        private void OnDataAvailable(object sender, ApiData.RequestDataAvailableEventArgs e)
        {
            if (Util.Logging.Enabled)
            {
                var r = (DataRequest)e.Request;
                Util.Logging.LogEvent(r.Account, "Data available for " + e.Type + " with reason " + r.Reason + " | " + e.Status);
            }

            pending.Enqueue(e);
        }

        private void OnEndDataAvailable(object sender, ApiData.DataAvailableEventArgs e)
        {
            //this will be triggered after an API key is queried, so all pending requests should always be for the same key and type

            var count = pending.Count;

            while (count > 0)
            {
                //sort by type (all types should already be the same) and accounts (will only happen if multiple accounts are using the same key)

                var l = new List<DataRequest>(count);
                var first = pending.Dequeue();
                var type = first.Type;
                var fr = (DataRequest)first.Request;
                var account = fr.Account;
                var reasons = fr.Reason;

                l.Add(fr);

                if (!fr.Pending)
                {
                    fr.DataAvailable -= OnDataAvailable;
                }
                else
                {
                    if (Util.Logging.Enabled)
                    {
                        Util.Logging.LogEvent(account, "First request is still pending [" + fr.Reason + "]");
                    }
                }

                for (var i = 1; i < count; i++)
                {
                    var q = pending.Dequeue();
                    var r = (DataRequest)q.Request;

                    if (r.Account == account && q.Type == type)
                    {
                        l.Add(r);
                        reasons |= r.Reason;

                        if (r.Pending)
                        {
                            //request will be repeated
                            if (Util.Logging.Enabled)
                            {
                                Util.Logging.LogEvent(account, "Request is still pending [" + fr.Reason + "]");
                            }
                        }
                        else
                        {
                            r.DataAvailable -= OnDataAvailable;   
                        }
                    }
                    else
                    {
                        pending.Enqueue(q);
                    }
                }

                if (DataAvailable != null)
                {
                    var de = new DataAvailableEventArgs(first, account, reasons, l);

                    try
                    {
                        DataAvailable(this, de);
                    }
                    catch { }

                    if (de.Repeating != null)
                    {
                        foreach (var r in de.Repeating)
                        {
                            if (r.Repeat())
                            {
                                Queue(r);
                            }
                            else 
                            {
                                if (Util.Logging.Enabled)
                                {
                                    Util.Logging.LogEvent(account, "Unable to repeat request for [" + r.Reason + "]");
                                }
                            }
                        }
                    }
                }

                count = pending.Count;
            }
        }
    }
}
