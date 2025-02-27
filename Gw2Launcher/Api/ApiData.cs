using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gw2Launcher.Api
{
    public class ApiData
    {
        private const int CACHE_EXPIRY_SECONDS = 5 * 60 + 30; //cache expires after 5 minutes, but the server can be delayed

        public event EventHandler<DataAvailableEventArgs> DataAvailable;
        public event EventHandler<ApiDataEventArgs> NextRequestChanged;
        public event EventHandler<ApiDataEventArgs> PendingChanged;
        public event EventHandler<ApiDataEventArgs> DelayChanged;
        /// <summary>
        /// Occurs after all DataAvailable events
        /// </summary>
        public event EventHandler<DataAvailableEventArgs> EndDataAvailable;

        public interface IApiKey
        {
            string Key
            {
                get;
            }

            Api.TokenInfo.Permissions Permissions
            {
                get;
            }
        }

        public enum LastModifiedType
        {
            Local = 0,
            Server = 1,
            Cache = 2,
        }

        public enum DataType
        {
            Account = 0,
            TokenInfo = 1,
            Wallet = 2,
            VaultDaily = 3,
            VaultWeekly = 4,
            VaultSpecial = 5,
        }

        private const int TYPE_LENGTH = 6;

        public static TokenInfo.Permissions GetPermissions(DataType t)
        {
            switch (t)
            {
                case DataType.VaultDaily:
                case DataType.VaultWeekly:
                case DataType.VaultSpecial:

                    return TokenInfo.Permissions.Progression;

                case DataType.Wallet:

                    return TokenInfo.Permissions.Wallet;
            }

            return TokenInfo.Permissions.None;
        }

        public enum DataStatus
        {
            Changed,
            NotModified,
            Error,
            Cache,
        }

        public interface ISubscriber : IDisposable
        {
            ICache Cache
            {
                get;
            }
        }

        private class CacheSubscriber : ISubscriber
        {
            private DataCache cache;

            public CacheSubscriber(DataCache cache)
            {
                this.cache = cache;

                lock (cache)
                {
                    ++cache.Subscribers;
                }
            }

            ~CacheSubscriber()
            {
                Dispose();
            }

            public ICache Cache
            {
                get
                {
                    return cache;
                }
            }

            public void Dispose()
            {
                DataCache cache;

                lock (this)
                {
                    if (this.cache != null)
                    {
                        GC.SuppressFinalize(this);
                        cache = this.cache;
                        this.cache = null;
                    }
                    else
                    {
                        return;
                    }
                }

                lock (cache)
                {
                    --cache.Subscribers;
                }
            }
        }

        public class DataRequest
        {
            [Flags]
            public enum RequestState : byte
            {
                None = 0,
                Pending = 1,
                Complete = 2,
                Aborted = 4,
                Delayed = 8,
            }

            [Flags]
            public enum RequestOptions
            {
                None = 0,
                /// <summary>
                /// Delays the request until the specified date/time
                /// </summary>
                Delay = 1,
                /// <summary>
                /// Delays the request until the current cache is expired
                /// </summary>
                NoCache = 2,
                /// <summary>
                /// Completes the request if data becomes available while the request is delayed
                /// </summary>
                IgnoreDelayIfModified = 4,
            }

            public event EventHandler<RequestDataAvailableEventArgs> DataAvailable;
            /// <summary>
            /// Occurs when the request has ended and is no longer queued
            /// </summary>
            public event EventHandler Complete;
            /// <summary>
            /// Occurs when a request needs to be updated
            /// </summary>
            public event EventHandler RequestChanged;

            public DataRequest(DataType type, IApiKey key, RequestOptions options = RequestOptions.None)
            {
                this.Type = type;
                this.Key = key;
                this.Options = options;
            }

            /// <summary>
            /// API key
            /// </summary>
            public IApiKey Key
            {
                get;
                private set;
            }

            public RequestOptions Options
            {
                get;
                private set;
            }

            /// <summary>
            /// API type
            /// </summary>
            public DataType Type
            {
                get;
                private set;
            }

            /// <summary>
            /// Optional date to pass
            /// </summary>
            public DateTime Date
            {
                get;
                set;
            }

            /// <summary>
            /// Optional date to delay until, if the option to delay is set
            /// </summary>
            public DateTime Delay
            {
                get;
                set;
            }

            /// <summary>
            /// Optional date to pass
            /// </summary>
            public DateTime Expires
            {
                get;
                set;
            }

            /// <summary>
            /// State of the request
            /// </summary>
            public RequestState State
            {
                get;
                private set;
            }

            /// <summary>
            /// Number of times the request has been repeated
            /// </summary>
            public byte RepeatCount
            {
                get;
                private set;
            }

            protected bool GetState(RequestState state)
            {
                return (this.State & state) == state;
            }

            protected void SetState(RequestState state, bool value)
            {
                if (value)
                    this.State |= state;
                else
                    this.State &= ~state;
            }

            /// <summary>
            /// Request will be skipped
            /// </summary>
            public void Abort()
            {
                SetState(RequestState.Aborted, true);

                if (RequestChanged != null)
                    RequestChanged(this, EventArgs.Empty);
            }

            public bool Aborted
            {
                get
                {
                    return GetState(RequestState.Aborted);
                }
            }

            public bool Pending
            {
                get
                {
                    return GetState(RequestState.Pending);
                }
            }

            public bool Delayed
            {
                get
                {
                    return GetState(RequestState.Delayed);
                }
                set
                {
                    SetState(RequestState.Delayed, value);
                }
            }

            /// <summary>
            /// Allows repeating a completed request, returning true if eligible
            /// </summary>
            /// <returns></returns>
            public bool Repeat()
            {
                if (State == RequestState.Complete)
                {
                    ++RepeatCount;
                    State = RequestState.Pending;
                    return true;
                }
                return false;
            }

            public void OnComplete()
            {
                if (Complete != null)
                {
                    try
                    {
                        Complete(this, EventArgs.Empty);
                    }
                    catch (Exception e)
                    {
                        Util.Logging.Log(e);
                    }
                }
            }

            public void OnDataAvailable(DataType type, DataStatus status, ICache data, object value)
            {
                if (DataAvailable != null)
                {
                    try
                    {
                        var e = new RequestDataAvailableEventArgs(this, type, status, data, value);

                        DataAvailable(this, e);

                        if (e.Repeat && !this.Aborted)
                        {
                            ++RepeatCount;
                            SetState(RequestState.Pending, true);

                            return;
                        }
                    }
                    catch (Exception e)
                    {
                        Util.Logging.Log(e);
                    }
                }

                this.State = RequestState.Complete;
            }
        }

        public class RequestDataAvailableEventArgs : DataAvailableEventArgs
        {
            public RequestDataAvailableEventArgs(DataRequest request, DataType type, DataStatus status, ICache data, object value)
                : base(type, status, data, value)
            {
                this.Request = request;
            }

            public DataRequest Request
            {
                get;
                private set;
            }

            /// <summary>
            /// Requeues the request
            /// </summary>
            public bool Repeat
            {
                get;
                set;
            }
        }

        public class DataAvailableEventArgs : EventArgs
        {
            private ICache data;
            private object value;

            public DataAvailableEventArgs(DataType type, DataStatus status, ICache data, object value)
            {
                this.Type = type;
                this.Status = status;
                this.data = data;
                this.value = value;
            }

            /// <summary>
            /// API type
            /// </summary>
            public DataType Type
            {
                get;
                private set;
            }

            /// <summary>
            /// Response status
            /// </summary>
            public DataStatus Status
            {
                get;
                private set;
            }

            /// <summary>
            /// API key
            /// </summary>
            public IApiKey Key
            {
                get
                {
                    return data.Key;
                }
            }

            public IData Data
            {
                get
                {
                    return data.GetData(Type);
                }
            }

            /// <summary>
            /// When the data was last modified, adjusted to the local clock
            /// </summary>
            public DateTime LastModifiedInLocalTime
            {
                get
                {
                    return data.LastModifiedLocal;
                }
            }

            /// <summary>
            /// When the data was last modified on the server
            /// </summary>
            public DateTime LastModified
            {
                get
                {
                    return data.LastModifiedServer;
                }
            }

            /// <summary>
            /// If the LastModified date changed since the last update. LastModified is only accurate to the minute, allowing for the data to change while having the same timestamp
            /// </summary>
            public bool Modified
            {
                get
                {
                    return data.Modified;
                }
            }

            /// <summary>
            /// When the next API request can be made
            /// </summary>
            public DateTime NextRequest
            {
                get
                {
                    return data.NextRequest;
                }
            }

            public DateTime GetLastModified(LastModifiedType type)
            {
                switch (type)
                {
                    case LastModifiedType.Local:

                        return data.LastModifiedLocal;

                    case LastModifiedType.Server:

                        return data.LastModifiedServer;

                    case LastModifiedType.Cache:
                    default:

                        return data.LastResponse;
                }
            }

            public Exception GetException()
            {
                if (Status == DataStatus.Error)
                {
                    return value as Exception;
                }

                return null;
            }
        }

        public class ApiDataEventArgs : EventArgs
        {
            protected ICache cache;

            public ApiDataEventArgs(ICache cache, bool refresh = false)
            {
                this.cache = cache;
                this.Refreshing = refresh;
            }

            /// <summary>
            /// API key
            /// </summary>
            public IApiKey Key
            {
                get
                {
                    return cache.Key;
                }
            }

            /// <summary>
            /// Next time the API is allowed to be requested
            /// </summary>
            public DateTime NextRequest
            {
                get
                {
                    return cache.NextRequest;
                }
            }

            /// <summary>
            /// The earliest delay
            /// </summary>
            public DateTime Delay
            {
                get
                {
                    return cache.Delay;
                }
            }

            /// <summary>
            /// Number of pending requests
            /// </summary>
            public ushort Pending
            {
                get
                {
                    return cache.Pending;
                }
            }

            /// <summary>
            /// Number of pending requests for the specified type
            /// </summary>
            public int GetPending(DataType t)
            {
                return cache.GetPending(t);
            }

            /// <summary>
            /// Requests have been added
            /// </summary>
            public bool Refreshing
            {
                get;
                private set;
            }
        }

        public interface ICache
        {
            /// <summary>
            /// API key
            /// </summary>
            IApiKey Key
            {
                get;
            }

            /// <summary>
            /// Returns cached data for the specified type
            /// </summary>
            IData GetData(DataType type);

            /// <summary>
            /// Next time the API is allowed to be requested
            /// </summary>
            DateTime NextRequest
            {
                get;
            }

            /// <summary>
            /// The earliest delay
            /// </summary>
            DateTime Delay
            {
                get;
            }

            /// <summary>
            /// Number of requests waiting
            /// </summary>
            ushort Pending
            {
                get;
            }

            /// <summary>
            /// When the API was last queried
            /// </summary>
            DateTime LastResponse
            {
                get;
            }

            /// <summary>
            /// When the data was last modified on the server
            /// </summary>
            DateTime LastModifiedServer
            {
                get;
            }

            /// <summary>
            /// When the data was last modified, adjusted to the local clock
            /// </summary>
            DateTime LastModifiedLocal
            {
                get;
            }

            /// <summary>
            /// If the LastModified date changed since the last update. LastModified is only accurate to the minute, allowing for the data to change while having the same timestamp
            /// </summary>
            bool Modified
            {
                get;
            }

            /// <summary>
            /// Number of requests waiting for the specified type of data
            /// </summary>
            int GetPending(DataType type);

            /// <summary>
            /// If this cache has been removed
            /// </summary>
            bool Disposed
            {
                get;
            }
        }

        public interface IData
        {
            /// <summary>
            /// Data from the API
            /// </summary>
            object Value
            {
                get;
            }

            /// <summary>
            /// Date the value was cached
            /// </summary>
            DateTime Date
            {
                get;
            }
        }

        private class DataObject : IData
        {
            public byte CacheKey
            {
                get;
                set;
            }

            /// <summary>
            /// Last time the API was requested
            /// </summary>
            public DateTime LastResponse
            {
                get;
                set;
            }

            /// <summary>
            /// Next time the API is allowed to be requested
            /// </summary>
            public DateTime NextRequest
            {
                get;
                set;
            }

            /// <summary>
            /// The earliest delayed request
            /// </summary>
            public DateTime Delay
            {
                get;
                set;
            }

            /// <summary>
            /// Number of errors that have occured since the last successful request
            /// </summary>
            public byte Errors
            {
                get;
                set;
            }

            public Queue<DataRequest> Requests
            {
                get;
                set;
            }

            /// <summary>
            /// Number of requests that are delayed
            /// </summary>
            public ushort Delayed
            {
                get;
                set;
            }

            /// <summary>
            /// Number of requests
            /// </summary>
            public int Pending
            {
                get
                {
                    if (Requests != null)
                    {
                        return Requests.Count;
                    }

                    return 0;
                }
            }

            public object Value
            {
                get;
                set;
            }

            public DateTime Date
            {
                get
                {
                    return LastResponse;
                }
            }
        }

        private class DataCache : ICache
        {
            [Flags]
            public enum Refreshing : byte
            {
                Requests = 1,
                Abort = 2,
                Delay = 4,
            }

            private Refreshing refreshing;

            public DataCache(IApiKey key)
            {
                this.Key = key;

                this.Data = new DataObject[TYPE_LENGTH];
                this.Data[0] = new DataObject()
                    {
                        NextRequest = DateTime.UtcNow,
                    };
            }

            /// <summary>
            /// Key to sync caches
            /// </summary>
            public byte CacheKey
            {
                get;
                set;
            }

            /// <summary>
            /// API key
            /// </summary>
            public IApiKey Key
            {
                get;
                set;
            }

            public DateTime NextRequest
            {
                get
                {
                    return Data[0].NextRequest;
                }
            }

            public DateTime LastResponse
            {
                get
                {
                    return Data[0].LastResponse;
                }
            }

            /// <summary>
            /// When the data was last modified on the server
            /// </summary>
            public DateTime LastModifiedServer
            {
                get;
                set;
            }

            /// <summary>
            /// When the data was last modified, adjusted to the local clock
            /// </summary>
            public DateTime LastModifiedLocal
            {
                get;
                set;
            }

            /// <summary>
            /// If the LastModified date changed since the last update. LastModified is only accurate to the minute, allowing for the data to change while having the same timestamp
            /// </summary>
            public bool Modified
            {
                get;
                set;
            }

            /// <summary>
            /// Number of minutes between the current and previous LastModified timestamps
            /// </summary>
            public byte LastModifiedElapsed
            {
                get;
                set;
            }

            /// <summary>
            /// The earliest delayed request
            /// </summary>
            public DateTime Delay
            {
                get;
                set;
            }

            /// <summary>
            /// Total number of requests from all data sources
            /// </summary>
            public ushort Pending
            {
                get;
                set;
            }

            public byte Subscribers
            {
                get;
                set;
            }

            /// <summary>
            /// Number of requests that are delayed
            /// </summary>
            public ushort Delayed
            {
                get;
                set;
            }

            /// <summary>
            /// Requests have changed
            /// </summary>
            public Refreshing Refresh
            {
                get
                {
                    return refreshing;
                }
                set
                {
                    refreshing = value;
                }
            }

            public DataObject[] Data
            {
                get;
                set;
            }

            public IData GetData(DataType type)
            {
                return GetDataObject(type);
            }

            public int GetPending(DataType type)
            {
                var d = Data[(int)type];

                if (d != null)
                {
                    lock (this)
                    {
                        return d.Pending;
                    }
                }

                return 0;
            }

            public DataObject GetDataObject(DataType type)
            {
                lock (this)
                {
                    var i = (int)type;

                    if (Data[i] == null)
                    {
                        Data[i] = new DataObject();
                    }

                    return Data[i];
                }
            }

            public void SetData(DataType type, object o)
            {
                lock (this)
                {
                    var d = GetDataObject(type);

                    d.CacheKey = this.CacheKey;
                    d.Value = o;

                }
            }

            public void OnDataAvailable(DataType type, DataStatus status, object value)
            {
                Queue<DataRequest> requests;
                DataObject d;
                ushort count;
                bool changed = false;

                lock (this)
                {
                    d = GetDataObject(type);

                    if (status == DataStatus.Changed)
                    {
                        d.CacheKey = this.CacheKey;
                        d.Value = value;
                    }

                    requests = d.Requests;

                    if (requests != null)
                    {
                        count = (ushort)requests.Count;
                    }
                    else
                    {
                        count = 0;
                    }
                }

                if (count > 0)
                {
                    for (var i = count; i > 0; --i)
                    {
                        DataRequest r;

                        lock (this)
                        {
                            r = requests.Dequeue();
                        }

                        if (!r.Aborted)
                        {
                            var requeue = true;

                            if (status != DataStatus.Cache || r.State == DataRequest.RequestState.None && (r.Options & DataRequest.RequestOptions.NoCache) == 0)
                            {
                                if (!r.Delayed || (r.Options & DataRequest.RequestOptions.IgnoreDelayIfModified) != 0)
                                {
                                    if (r.Delayed)
                                    {
                                        RemoveDelay(d, r);
                                    }
                                    r.OnDataAvailable(type, status, this, value);
                                    requeue = r.Pending;
                                }
                            }

                            if (requeue)
                            {
                                lock (this)
                                {
                                    requests.Enqueue(r);
                                    --count;
                                }
                            }
                            else
                            {
                                OnComplete(d, r);
                            }
                        }
                        else
                        {
                            OnComplete(d, r);
                        }
                    }

                    if (count > 0)
                    {
                        lock (this)
                        {
                            this.Pending -= count;
                        }
                    }
                }
            }

            private bool RemoveDelay(DataObject d, DataRequest r)
            {
                lock (this)
                {
                    if (r.Delayed)
                    {
                        r.Delayed = false;

                        --this.Delayed;
                        --d.Delayed;
                        this.refreshing |= Refreshing.Delay;

                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }
            }

            public void OnComplete(DataObject d, DataRequest r)
            {
                if (r.Delayed)
                {
                    RemoveDelay(d, r);
                }

                r.RequestChanged -= OnRequestChanged;
                r.OnComplete();
            }

            public void Queue(DataRequest request)
            {
                lock (this)
                {
                    var d = GetDataObject(request.Type);

                    if (d.Requests == null)
                    {
                        d.Requests = new Queue<DataRequest>();
                    }

                    d.Requests.Enqueue(request);

                    request.RequestChanged += OnRequestChanged;

                    ++this.Pending;
                    this.refreshing |= Refreshing.Requests;

                    if ((request.Options & DataRequest.RequestOptions.Delay) != 0)
                    {
                        request.Delayed = true;

                        if (d.Delayed == 0 || request.Delay < d.Delay)
                        {
                            d.Delay = request.Delay;

                            if (this.Delayed == 0 || request.Delay < this.Delay)
                            {
                                //this.Delay = request.Delay;
                                this.Delay = DateTime.MinValue;
                            }
                        }

                        ++this.Delayed;
                        ++d.Delayed;
                    }

                    if (Util.Logging.Enabled)
                    {
                        if (request is Tools.Api.ApiRequestManager.DataRequest)
                        {
                            var r = (Tools.Api.ApiRequestManager.DataRequest)request;

                            Util.Logging.LogEvent(r.Account, "Queued [" + r.Type + "] with reason [" + r.Reason + "] (" + this.Pending + " pending, " + this.Delayed + " delayed)");
                        }
                        else
                        {
                            Util.Logging.LogEvent("Queued [" + request.Type + "] (" + this.Pending + " pending, " + this.Delayed + " delayed)");
                        }
                    }



                }
            }

            void OnRequestChanged(object sender, EventArgs e)
            {
                if ((refreshing & Refreshing.Abort) == 0)
                {
                    lock (this)
                    {
                        refreshing |= Refreshing.Abort;
                    }
                }
            }

            public DataRequest Dequeue(DataType type)
            {
                lock (this)
                {
                    var d = GetDataObject(type);

                    if (d.Requests != null && d.Requests.Count > 0)
                    {
                        var r = d.Requests.Dequeue();

                        r.RequestChanged -= OnRequestChanged;

                        --this.Pending;

                        return r;
                    }

                    return null;
                }
            }

            public bool Disposed
            {
                get;
                set;
            }
        }

        private Dictionary<string, DataCache> cache;
        private Task task;
        private DateTime purge;

        public ApiData()
        {
            cache = new Dictionary<string, DataCache>();
        }

        /// <summary>
        /// Queues requests, ingoring any nulls
        /// </summary>
        public void Queue(DataRequest[] requests)
        {
            lock (cache)
            {
                var count = requests.Length;
                DataCache data = null;

                for (var i = 0; i < count; i++)
                {
                    if (requests[i] == null)
                    {
                        --count;
                    }
                    else
                    {
                        if ((data == null || !object.ReferenceEquals(data.Key, requests[i].Key)) && !cache.TryGetValue(requests[i].Key.Key, out data))
                        {
                            cache[requests[i].Key.Key] = data = new DataCache(requests[i].Key);
                        }

                        if (Util.Logging.Enabled)
                        {
                            if (requests[i] is Tools.Api.ApiRequestManager.DataRequest)
                            {
                                var r = (Tools.Api.ApiRequestManager.DataRequest)requests[i];

                                Util.Logging.LogEvent(r.Account, "Queuing [" + r.Type + "] with reason [" + r.Reason + "]");
                            }
                            else
                            {
                                Util.Logging.LogEvent("Queuing [" + requests[i].Type + "]");
                            }
                        }

                        data.Queue(requests[i]);
                    }
                }

                if ((task == null || task.IsCompleted) && count > 0)
                {
                    task = DoQueue();
                    Util.ScheduledEvents.Register(Purge, 30 * 60 * 1000);
                }
            }
        }

        public void Queue(DataRequest request)
        {
            lock (cache)
            {
                DataCache data;

                if (!cache.TryGetValue(request.Key.Key, out data))
                {
                    cache[request.Key.Key] = data = new DataCache(request.Key);
                }

                if (request is Tools.Api.ApiRequestManager.DataRequest)
                {
                    var r = (Tools.Api.ApiRequestManager.DataRequest)request;

                    Util.Logging.LogEvent(r.Account, "Queuing [" + r.Type + "] with reason [" + r.Reason + "]");
                }
                else
                {
                    Util.Logging.LogEvent("Queuing [" + request.Type + "]");
                }

                data.Queue(request);

                if (task == null || task.IsCompleted)
                {
                    task = DoQueue();
                    Util.ScheduledEvents.Register(Purge, 30 * 60 * 1000);
                }
            }
        }

        private Util.ScheduledEvents.Ticks Purge()
        {
            lock (cache)
            {
                if (task == null || task.IsCompleted)
                {
                    task = DoQueue();
                }
            }

            return Util.ScheduledEvents.Ticks.None;
        }

        /// <summary>
        /// Updates delay timee
        /// </summary>
        /// <param name="changed">True to ignore what is currently set for data.Delay</param>
        /// <param name="modified">True to handle requests with RequestOptions.IgnoreDelayIfModified</param>
        /// <returns></returns>
        private bool DoDelayed(DataCache data, DataObject[] _data, bool changed = false, bool modified = false)
        {
            var refresh = false;

            if (data.Delayed != 0)
            {
                if (changed || DateTime.UtcNow >= data.Delay)
                {
                    var next = DateTime.MaxValue;
                    var count = _data != null ? _data.Length : 0;

                    for (var i = 0; i < count; i++)
                    {
                        var d = _data[i];

                        if (d == null)
                        {
                            continue;
                        }

                        if (d.Delayed > 0)
                        {
                            if (modified && i > 0 && data.NextRequest < d.Delay || DateTime.UtcNow >= d.Delay)
                            {
                                var delayed = DateTime.MaxValue;

                                lock (data)
                                {
                                    foreach (var r in d.Requests)
                                    {
                                        if (r.Delayed)
                                        {
                                            if (modified && (r.Options & DataRequest.RequestOptions.IgnoreDelayIfModified) != 0 && data.NextRequest < r.Delay || DateTime.UtcNow >= r.Delay)
                                            {
                                                r.Delayed = false;

                                                if (!refresh && r.State == DataRequest.RequestState.None)
                                                {
                                                    if ((r.Options & DataRequest.RequestOptions.NoCache) == 0)
                                                        refresh = true;
                                                }

                                                --data.Delayed;
                                                if (--d.Delayed == 0)
                                                    break;
                                            }
                                            else if (r.Delay < delayed)
                                            {
                                                delayed = r.Delay;
                                            }
                                        }
                                    }
                                }

                                if (delayed == DateTime.MaxValue)
                                {
                                    d.Delay = DateTime.MinValue;
                                }
                                else
                                {
                                    d.Delay = delayed;

                                    if (delayed < next)
                                    {
                                        next = delayed;
                                    }
                                }
                            }
                            else if (d.Delay < next)
                            {
                                next = d.Delay;
                            }
                        }
                    }

                    if (next == DateTime.MaxValue)
                    {
                        next = DateTime.MinValue;
                    }

                    if (data.Delay != next)
                    {
                        data.Delay = next;
                        OnDelayChanged(data);
                    }
                }
            }
            else if (data.Delay != DateTime.MinValue)
            {
                data.Delay = DateTime.MinValue;
                OnDelayChanged(data);
            }

            return refresh;
        }

        private async Task DoQueue()
        {
            await Task.Delay(100);

            var queue = new Queue<DataCache>();
            var disabled = DateTime.MinValue;

            while (true)
            {
                var refresh = false;

                lock (cache)
                {
                    foreach (DataCache data in cache.Values)
                    {
                        if (data.Pending != 0)
                        {
                            queue.Enqueue(data);

                            if (data.Refresh != 0)
                            {
                                refresh = true;
                            }
                        }
                        else if (data.Refresh != 0)
                        {
                            queue.Enqueue(data);
                        }
                    }

                    if (queue.Count == 0)
                    {
                        task = null;


                        #region Purge

                        if (DateTime.UtcNow > purge)
                        {
                            purge = DateTime.UtcNow.AddMinutes(30);

                            var expired = DateTime.UtcNow.AddMinutes(-20);

                            foreach (DataCache data in cache.Values)
                            {
                                if (data.Pending == 0 && data.Subscribers == 0 && expired > data.NextRequest)
                                {
                                    queue.Enqueue(data);
                                }
                            }

                            if (Util.Logging.Enabled && queue.Count > 0)
                            {
                                var sb = new StringBuilder();
                                foreach (var d in queue)
                                {
                                    sb.Append("Purging [");
                                    sb.Append(GetAccountNames(d.Key));
                                    sb.Append("] from API cache");
                                    if (d.LastResponse != DateTime.MinValue)
                                    {
                                        sb.Append(" (cached ");
                                        sb.Append(DateTime.UtcNow.Subtract(d.LastResponse).TotalMinutes.ToString("0.#"));
                                        sb.Append(" minutes ago)");
                                    }
                                    else
                                    {
                                        sb.Append(" (unused)");
                                    }
                                    Util.Logging.LogEvent(sb.ToString());
                                    sb.Length = 0;
                                }
                                Util.Logging.LogEvent((cache.Count - queue.Count) + " remaining in API cache");
                            }

                            if (cache.Count == queue.Count)
                            {
                                foreach (var d in queue)
                                {
                                    d.Disposed = true;
                                }

                                cache.Clear();
                            }
                            else
                            {
                                foreach (var d in queue)
                                {
                                    d.Disposed = true;
                                    cache.Remove(d.Key.Key);
                                }

                                queue = null;
                            }
                        }

                        #endregion

                        break;
                    }
                }

                if (refresh)
                {
                    //notify all request changes prior to querying to avoid delays
                    foreach (var data in queue)
                    {
                        if ((data.Refresh & DataCache.Refreshing.Requests) != 0)
                        {
                            OnPendingChanged(data, true);
                        }
                    }
                }

                while (queue.Count > 0)
                {
                    var data = queue.Dequeue();
                    var pending = data.Pending;
                    var _data = data.Data;

                    if (data.Refresh != 0)
                    {
                        var delayChanged = false;

                        #region Refresh

                        System.Threading.Monitor.Enter(data);
                        try
                        {
                            if ((data.Refresh & DataCache.Refreshing.Abort) != 0)
                            {
                                data.Refresh &= ~DataCache.Refreshing.Abort;

                                for (var i = 0; i < _data.Length; i++)
                                {
                                    var d = _data[i];

                                    if (d == null)
                                    {
                                        continue;
                                    }

                                    var requests = d.Requests;

                                    if (requests != null)
                                    {
                                        var count = requests.Count;

                                        while (count > 0)
                                        {
                                            var r = requests.Dequeue();

                                            if (r.Aborted)
                                            {
                                                if (r.Delayed)
                                                {
                                                    r.Delayed = false;

                                                    --data.Delayed;
                                                    --d.Delayed;
                                                    delayChanged = true;
                                                }
                                                System.Threading.Monitor.Exit(data);

                                                Util.Logging.LogEvent("Aborting [" + r.Type + "] for " + r.Key.Key);

                                                data.OnComplete(d, r);

                                                System.Threading.Monitor.Enter(data);
                                                --data.Pending;
                                            }
                                            else
                                            {
                                                requests.Enqueue(r);
                                            }

                                            --count;
                                        }
                                    }
                                }
                            }

                            if ((data.Refresh & DataCache.Refreshing.Delay) != 0)
                            {
                                data.Refresh &= ~DataCache.Refreshing.Delay;
                                delayChanged = true;
                            }

                            if (refresh = (data.Refresh & DataCache.Refreshing.Requests) != 0)
                            {
                                data.Refresh &= ~DataCache.Refreshing.Requests;
                            }
                        }
                        finally
                        {
                            System.Threading.Monitor.Exit(data);
                        }

                        #endregion

                        if (DoDelayed(data, _data, delayChanged))
                        {
                            refresh = true;
                        }
                    }
                    else
                    {
                        refresh = DoDelayed(data, _data);
                    }

                    if (pending != 0)
                    {
                        #region Query API

                        //note data provided by the API is cached for 5 minutes once requested
                        //the account API can be used to check if the data has been modified, so it (0) will be used to determine if other APIs should be checked for updated data

                        for (var i = 0; i < _data.Length; i++)
                        {
                            var d = _data[i];

                            if (d == null)
                            {
                                continue;
                            }

                            bool b;

                            if (b = DateTime.UtcNow > d.NextRequest)
                            {
                                if (i == 0)
                                {
                                    b = data.Pending != data.Delayed;
                                }
                                else
                                {
                                    b = (d.CacheKey != data.CacheKey || d.Value == null) && d.Pending != d.Delayed;
                                }
                            }

                            if (b)
                            {
                                var t = (DataType)i;
                                var status = DataStatus.Changed;

                                object o = null;

                                try
                                {
                                    if (Util.Logging.Enabled)
                                    {
                                        Util.Logging.LogEvent("Querying [" + t + "] API for [" + GetAccountNames(data.Key) + "]");
                                    }

                                    switch (t)
                                    {
                                        case DataType.Account:

                                            if (disabled != DateTime.MinValue)
                                            {
                                                if (DateTime.UtcNow < disabled)
                                                {
                                                    throw new Exceptions.ServiceUnavailableException();
                                                }
                                                else
                                                {
                                                    disabled = DateTime.MinValue;
                                                }
                                            }

                                            var account = await Account.GetAccountAsync(data.Key.Key);
                                            
                                            //warning: the last modified from the server is only accurate to the minute, so it takes an extra request to verify it actually hasn't changed
                                            //to sample this, change maps to force an update, request the API, do something to trigger an update, then logout within the same minute

                                            if (data.LastModifiedServer != account.LastModifiedServer)
                                            {
                                                var m = (account.LastModifiedServer.Ticks - data.LastModifiedServer.Ticks) / 600000000;

                                                if (m <= 0)
                                                {
                                                    data.LastModifiedElapsed = 0;
                                                }
                                                else if (m > 255)
                                                {
                                                    data.LastModifiedElapsed = 255;
                                                }
                                                else
                                                {
                                                    data.LastModifiedElapsed = (byte)m;
                                                }

                                                if (Util.Logging.Enabled)
                                                {
                                                    Util.Logging.LogEvent("[" + t + "] API for [" + GetAccountNames(data.Key) + "] updated at [" + account.LastModifiedServer + "] (" + (data.LastModifiedElapsed == 255 ? "?" : data.LastModifiedElapsed + "m") + ") (" + (data.CacheKey + 1) + ")");
                                                }

                                                data.LastModifiedServer = account.LastModifiedServer;
                                                data.LastModifiedLocal = account.LastModified;
                                                data.Modified = true;
                                            }
                                            else
                                            {
                                                if (false && data.Modified && DateTime.UtcNow > data.LastModifiedLocal.AddSeconds(CACHE_EXPIRY_SECONDS))
                                                {
                                                    data.Modified = false;

                                                    if (Util.Logging.Enabled)
                                                    {
                                                        Util.Logging.LogEvent("[" + t + "] API for [" + GetAccountNames(data.Key) + "] was not modified (expired)");
                                                    }
                                                }
                                                else
                                                {
                                                    if (Util.Logging.Enabled)
                                                    {
                                                        if (data.Modified)
                                                            Util.Logging.LogEvent("[" + t + "] API for [" + GetAccountNames(data.Key) + "] was not modified (unverified)");
                                                        else
                                                            Util.Logging.LogEvent("[" + t + "] API for [" + GetAccountNames(data.Key) + "] was not modified");
                                                    }
                                                }

                                                data.LastModifiedElapsed = 0;

                                                if (data.Modified)
                                                {
                                                    data.Modified = false;
                                                }
                                                else
                                                {
                                                    status = DataStatus.NotModified;
                                                }
                                            }

                                            if (status != DataStatus.NotModified)
                                            {
                                                if (++data.CacheKey == 0)
                                                {
                                                    //rollover, reset all other keys
                                                    data.CacheKey = 1;

                                                    for (var j = 1; j < _data.Length; j++)
                                                    {
                                                        if (_data[j] != null)
                                                        {
                                                            _data[j].CacheKey = 0;
                                                        }
                                                    }
                                                }
                                            }

                                            o = account;

                                            d.NextRequest = DateTime.UtcNow.AddSeconds(CACHE_EXPIRY_SECONDS);

                                            if (status == DataStatus.Changed)
                                            {
                                                if (data.Delayed > 0)
                                                {
                                                    //DoDelayed(data, _data, true, true);

                                                    for (var j = 1; j < _data.Length; j++)
                                                    {
                                                        if (_data[j] != null && _data[j].Pending > 0 && _data[j].Delayed == _data[j].Pending)
                                                        {
                                                            lock (data)
                                                            {
                                                                var requests = _data[j].Requests;
                                                                if (requests != null)
                                                                {
                                                                    foreach (var r in requests)
                                                                    {
                                                                        if (r.Delayed && (r.Options & DataRequest.RequestOptions.IgnoreDelayIfModified) != 0 && d.NextRequest < r.Delay)
                                                                        {
                                                                            r.Delayed = false;
                                                                            --data.Delayed;
                                                                            --_data[j].Delayed;
                                                                            data.Refresh |= DataCache.Refreshing.Delay;
                                                                        }
                                                                    }
                                                                }
                                                            }
                                                        }
                                                    }
                                                }
                                            }

                                            break;
                                        case DataType.TokenInfo:

                                            o = await TokenInfo.GetPermissionsAsync(data.Key.Key);

                                            break;
                                        case DataType.Wallet:

                                            o = await Wallet.GetWalletAsync(data.Key.Key);

                                            break;
                                        case DataType.VaultDaily:

                                            o = await Vault.GetVaultAsync(Vault.VaultType.Daily, data.Key.Key);

                                            break;
                                        case DataType.VaultWeekly:

                                            o = await Vault.GetVaultAsync(Vault.VaultType.Weekly, data.Key.Key);

                                            break;
                                        case DataType.VaultSpecial:

                                            o = await Vault.GetVaultAsync(Vault.VaultType.Special, data.Key.Key);

                                            break;
                                    }
                                }
                                catch (Exception e)
                                {
                                    if (Util.Logging.Enabled)
                                    {
                                        Util.Logging.LogEvent("Error querying [" + t + "] API using " + data.Key, e);
                                    }

                                    if (e is Exceptions.ServiceUnavailableException)
                                    {
                                        d.NextRequest = DateTime.UtcNow.AddMinutes(10);

                                        if (disabled == DateTime.MinValue)
                                        {
                                            disabled = d.NextRequest;
                                        }
                                    }
                                    else
                                    {
                                        d.NextRequest = DateTime.UtcNow.AddMinutes(1);
                                    }

                                    if (e is Exceptions.ApiException && (e is Exceptions.ServiceUnavailableException || e is Exceptions.InvalidKeyException || e is Exceptions.PermissionRequiredException) || ++d.Errors > 5)
                                    {
                                        OnError(t, data, e);

                                        if (i == 0)
                                        {
                                            //key is invalid

                                            for (var j = 1; j < _data.Length; j++)
                                            {
                                                if (_data[j] != null && _data[j].CacheKey == data.CacheKey)
                                                {
                                                    OnError((DataType)j, data, e);
                                                }
                                            }
                                        }
                                    }

                                    if (i == 0)
                                    {
                                        OnNextRequestChanged(data);
                                    }

                                    continue;
                                }

                                d.Errors = 0;
                                d.LastResponse = DateTime.UtcNow;

                                OnDataAvailable(t, status, data, o);

                                if (i == 0)
                                {
                                    OnNextRequestChanged(data);

                                    if (status == DataStatus.NotModified)
                                    {
                                        //data wasn't modified, announce to pending queries

                                        for (var j = 1; j < _data.Length; j++)
                                        {
                                            if (_data[j] != null && _data[j].CacheKey == data.CacheKey)
                                            {
                                                OnDataAvailable((DataType)j, DataStatus.NotModified, data);
                                            }
                                        }
                                    }
                                }

                                //if (data.Pending == 0)
                                //    break;
                            }
                            else if (refresh && d.CacheKey != 0)
                            {
                                //announce the cached data to requests that haven't been handled yet

                                OnDataAvailable((DataType)i, DataStatus.Cache, data);
                            }
                        }

                        #endregion

                        //#region Query account API

                        ////note the server will cache data provided by the API for 5 minutes once a request is made

                        //if (DateTime.UtcNow > data.NextRequest)
                        //{
                        //    Account account;

                        //    try
                        //    {
                        //        account = await Account.GetAccountAsync(data.Key);
                        //    }
                        //    catch (Exception e)
                        //    {
                        //        if (Util.Logging.Enabled)
                        //        {
                        //            Util.Logging.LogEvent("Error querying account API using " + data.Key, e);
                        //        }

                        //        if (e is Exceptions.ApiException && (e is Exceptions.InvalidKeyException || e is Exceptions.PermissionRequiredException) || ++data.Errors > 5)
                        //        {
                        //            lock (data)
                        //            {
                        //                cache.Remove(data.Key);
                        //            }
                        //        }
                        //        else
                        //        {
                        //            data.NextRequest = DateTime.UtcNow.AddMinutes(1);
                        //        }

                        //        continue;
                        //    }

                        //    if (data.LastModifiedServer != account.LastModifiedServer)
                        //    {
                        //        data.LastModifiedServer = account.LastModifiedServer;
                        //        ++data.ReferenceKey;
                        //    }

                        //    data.Errors = 0;
                        //    data.LastResponse = DateTime.UtcNow;
                        //    data.NextRequest = DateTime.UtcNow.AddSeconds(CACHE_EXPIRY_SECONDS);
                        //    data.SetData(DataType.Account, account);

                        //    OnDataAvailable(DataType.Account, data);

                        //    if (data.Pending == 0)
                        //        continue;
                        //}

                        //#endregion

                        //#region Do pending API queries

                        //var _data = data.Data;

                        //for (var i = 1; i < _data.Length; i++)
                        //{
                        //    if (_data[i] != null && _data[i].Pending != 0 && DateTime.UtcNow > _data[i].NextRequest)
                        //    {
                        //        var t = (DataType)i;

                        //        if (_data[i].ReferenceKey != data.ReferenceKey || _data[i].Data == null)
                        //        {
                        //            object o = _data[i].Data;

                        //            try
                        //            {
                        //                switch (t)
                        //                {
                        //                    case DataType.Wallet:

                        //                        o = await Wallet.GetWalletAsync(data.Key);

                        //                        break;
                        //                }
                        //            }
                        //            catch (Exception e)
                        //            {
                        //                if (Util.Logging.Enabled)
                        //                {
                        //                    Util.Logging.LogEvent("Error querying [" + t + "] API using " + data.Key, e);
                        //                }

                        //                if (e is Exceptions.ApiException && (e is Exceptions.InvalidKeyException || e is Exceptions.PermissionRequiredException))
                        //                {
                        //                    //invalid key
                        //                    data.SetData(t, null);
                        //                }
                        //                else
                        //                {
                        //                    _data[i].NextRequest = DateTime.UtcNow.AddMinutes(1);

                        //                    if (++_data[i].Errors > 5)
                        //                    {
                        //                        data.SetData(t, null);
                        //                    }
                        //                }

                        //                continue;
                        //            }

                        //            _data[i].Errors = 0;
                        //            _data[i].LastResponse = DateTime.UtcNow;

                        //            data.SetData(t, o);

                        //            OnDataAvailable(t, data);
                        //        }
                        //        else
                        //        {
                        //            //data hasn't changed
                        //            //skip if account isn't pending

                        //            if (data.Data[0].Pending == 0)
                        //            {
                        //                data.SetData(t, _data[i].Data);
                        //            }
                        //        }
                        //    }
                        //}

                        //#endregion
                    }

                    if (data.Pending != pending)
                    {
                        OnPendingChanged(data);
                    }
                }

                await Task.Delay(5000);
            }

            if (queue == null)
            {
                Util.ScheduledEvents.Register(Purge, 30 * 60 * 1000);
            }
        }

        private string GetAccountNames(IApiKey d)
        {
            var sb = new StringBuilder();
            var s = d as Settings.ApiDataKey;

            if (s != null)
            {
                var accounts = s.Accounts;

                if (accounts != null)
                {
                    foreach (var a in accounts)
                    {
                        if (sb.Length > 0)
                            sb.Append(", ");
                        sb.Append(a.Name);
                    }
                }
            }

            if (sb.Length == 0)
            {
                sb.Append(d.Key);
            }

            return sb.ToString();
        }

        private void OnError(DataType type, DataCache data, Exception error)
        {
            OnDataAvailable(type, DataStatus.Error, data, error);
        }

        private void OnDataAvailable(DataType type, DataStatus status, DataCache data, object value = null)
        {
            var p = data.Pending;
            
            data.OnDataAvailable(type, status, value);

            var doEnd = EndDataAvailable != null;
            var doAvailable = DataAvailable != null && status != DataStatus.Cache;

            if (doEnd || doAvailable)
            {
                var e = new DataAvailableEventArgs(type, status, data, value);

                if (doAvailable)
                {
                    try
                    {
                        DataAvailable(this, e);
                    }
                    catch { }
                }

                if (doEnd)
                {
                    try
                    {
                        EndDataAvailable(this, e);
                    }
                    catch { }
                }
            }

            if ((data.Refresh & DataCache.Refreshing.Delay) != 0)
            {
            }

            if ((data.Refresh & DataCache.Refreshing.Requests) == 0 && p != data.Pending)
            {
            }
        }

        private void OnNextRequestChanged(DataCache data)
        {
            if (NextRequestChanged != null)
            {
                try
                {
                    NextRequestChanged(this, new ApiDataEventArgs(data));
                }
                catch { }
            }
        }

        private void OnPendingChanged(DataCache data, bool refresh = false)
        {

            if (PendingChanged != null)
            {
                try
                {
                    PendingChanged(this, new ApiDataEventArgs(data, refresh));
                }
                catch { }
            }
        }

        private void OnDelayChanged(DataCache data)
        {

            if (DelayChanged != null)
            {
                try
                {
                    DelayChanged(this, new ApiDataEventArgs(data));
                }
                catch { }
            }
        }

        public ISubscriber Register(IApiKey key)
        {
            lock (cache)
            {
                DataCache data;

                if (!cache.TryGetValue(key.Key, out data))
                {
                    cache[key.Key] = new DataCache(key);
                }

                return new CacheSubscriber(data);
            }
        }

        /// <summary>
        /// Returns when the next API request can be made
        /// </summary>
        /// <param name="key">API key</param>
        public DateTime GetNext(string key)
        {
            lock (cache)
            {
                DataCache data;

                if (cache.TryGetValue(key, out data))
                {
                    return data.NextRequest;
                }
            }

            return DateTime.MinValue;
        }

        /// <summary>
        /// Returns an estimate for when the data provided by the API will be updated
        /// </summary>
        /// <param name="key">API key</param>
        public DateTime GetNextEstimatedUpdate(string key)
        {
            //the API is only updated once every 5 minutes (from the last modified date) and on map changes (including logout / chararcter select)

            lock (cache)
            {
                DataCache data;

                if (cache.TryGetValue(key, out data))
                {
                    return GetNextEstimatedUpdate(data.LastModifiedLocal);
                }
            }

            return DateTime.MinValue;
        }

        public DateTime GetNextEstimatedUpdate(DateTime lastModified)
        {
            var d = DateTime.UtcNow;
            return d.AddSeconds(300 - (int)(d.Subtract(lastModified).TotalSeconds + 0.5) % 300 + 30);
        }

        public ICache GetCache(string key)
        {
            lock (cache)
            {
                DataCache data;

                if (cache.TryGetValue(key, out data))
                {
                    return data;
                }
            }

            return null;
        }

        public int GetPending(string key)
        {
            lock (cache)
            {
                DataCache data;

                if (cache.TryGetValue(key, out data))
                {
                    return data.Pending;
                }
            }

            return 0;
        }

        //public int GetPending(string key, DataType type)
        //{
        //    lock (cache)
        //    {
        //        DataCache data;

        //        if (cache.TryGetValue(key, out data))
        //        {
        //            var d = data.Data[(int)type];

        //            if (d != null)
        //            {
        public DataRequest[] GetRequests(string key)
        {
            DataCache data;

            lock (cache)
            {
                if (!cache.TryGetValue(key, out data))
                {
                    return new DataRequest[0];
                }
            }

            lock (data)
            {
                var count = data.Pending;
                var requests = new DataRequest[count];

                if (count > 0)
                {
                    var index = 0;
                    var _data = data.Data;

                    for (var i = 0; i < _data.Length; i++)
                    {
                        var d = _data[i];

                        if (d != null && d.Requests != null)
                        {
                            foreach (var r in d.Requests)
                            {
                                requests[index] = r;

                                if (++index == count)
                                    return requests;
                            }
                        }
                    }

                    if (index != count)
                    {
                        Array.Resize<DataRequest>(ref requests, index);
                    }
                }

                return requests;
            }
        }
    }
}
