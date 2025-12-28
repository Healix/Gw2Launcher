using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using System.Drawing.Drawing2D;

namespace Gw2Launcher.UI.Controls
{
    public class ApiTimer : IDisposable
    {
        public const Gw2Launcher.Api.ApiData.DataType ANY_TYPE = (Gw2Launcher.Api.ApiData.DataType)(-1);

        public enum DelayType : byte
        {
            /// <summary>
            /// Request is delayed due to caching
            /// </summary>
            Cached = 0,
            /// <summary>
            /// Request is delayed due to a specified delay
            /// </summary>
            Pending = 1,
        }

        public event EventHandler Tick;
        public event EventHandler BeginRequest;

        private Settings.ApiDataKey api;
        private sbyte current;
        private DateTime[] dates;
        private int start;
        private float limit;
        private Rectangle bounds;
        private bool resize;
        private bool active;
        private bool pending;
        private bool updating;
        private bool requesting;
        private Api.ApiData manager;
        private Api.ApiData.DataType type;
        private Api.ApiData.DataRequest request;
        private bool enabled;

        public ApiTimer(Settings.IAccount account = null, Api.ApiData manager = null, Api.ApiData.DataType type = ANY_TYPE)
        {
            this.manager = manager;
            this.enabled = true;
            this.current = -1;

            resize = true;
            dates = new DateTime[2];
            this.type = type;

            if (account != null)
            {
                SetApi(account);
            }

            if (manager != null)
            {
                manager.PendingChanged += manager_PendingChanged;
                manager.NextRequestChanged += manager_NextRequestChanged;
                manager.DelayChanged += manager_DelayChanged;
                manager.EndUpdate += manager_EndUpdate;
                manager.BeginUpdate += manager_BeginUpdate;
            }
        }

        void manager_BeginUpdate(object sender, Api.ApiData.ApiDataEventArgs e)
        {
            if (object.ReferenceEquals(this.api, e.Key))
            {
                updating = true;

                if (BeginRequest != null)
                    BeginRequest(this, EventArgs.Empty);

                if (pending && enabled)
                {
                    if (Tick != null)
                        Tick(this, EventArgs.Empty);
                }
            }
        }

        void manager_EndUpdate(object sender, Api.ApiData.ApiDataEventArgs e)
        {
            if (object.ReferenceEquals(this.api, e.Key))
            {
                updating = false;

                if (pending && enabled)
                {
                    if (Tick != null)
                        Tick(this, EventArgs.Empty);
                }
            }
        }

        void manager_DelayChanged(object sender, Api.ApiData.ApiDataEventArgs e)
        {
            if (object.ReferenceEquals(this.api, e.Key))
            {
                SetTimer(DelayType.Pending, e.Delay, DateTime.MinValue);
            }
        }

        void manager_NextRequestChanged(object sender, Api.ApiData.ApiDataEventArgs e)
        {
            if (object.ReferenceEquals(this.api, e.Key) && DateTime.UtcNow < e.NextRequest)
            {
                SetTimer(DelayType.Cached, e.NextRequest, DateTime.MinValue);
            }
        }

        void manager_PendingChanged(object sender, Api.ApiData.ApiDataEventArgs e)
        {
            if (object.ReferenceEquals(this.api, e.Key))
            {
                var pending = type == ANY_TYPE ? e.Pending != 0 : e.GetPending(type) != 0;

                if (e.Refreshing || !pending)
                {
                    if (this.pending != pending)
                    {
                        this.pending = pending;

                        if (pending)
                        {
                            Restart(DateTime.MinValue);
                        }
                    }

                    if (e.Delay != DateTime.MinValue)
                    {
                        SetTimer(DelayType.Pending, e.Delay, DateTime.MinValue);
                    }
                }
            }
        }

        /// <summary>
        /// Enables Tick events
        /// </summary>
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

                    if (active)
                    {
                        Util.ScheduledEvents.Register(OnScheduledTick, Frequency);
                    }
                    else if (value)
                    {
                        Start(DateTime.MinValue);
                    }
                }
            }
        }

        public void SetApi(Settings.IAccount account, Api.ApiData.DataType type = ANY_TYPE)
        {
            if (account.Type == Settings.AccountType.GuildWars2)
            {
                var a = ((Settings.IGw2Account)account).Api;

                if (this.type != type || api != a)
                {
                    this.updating = this.updating && api == a;
                    this.type = type;
                    this.api = a;

                    if (a != null && manager != null)
                    {
                        var cache = manager.GetCache(a.Key);

                        if (cache != null)
                        {
                            this.pending = type == ANY_TYPE ? cache.Pending != 0 : cache.GetPending(type) != 0;

                            dates[0] = cache.NextRequest;
                            dates[1] = cache.Delay;

                            if (DateTime.UtcNow.Subtract(cache.LastRequest).TotalMinutes < 5)
                                Restart(cache.LastRequest);
                            else
                                Restart(DateTime.MinValue);

                        }
                        else
                        {
                            Reset();
                        }
                    }
                    else if (active)
                    {
                        Reset();
                    }
                }
            }
            else if (api != null)
            {
                api = null;
                updating = false;

                if (active)
                {
                    Reset();
                }
            }
        }

        public void SetRequest(Api.ApiData.DataRequest r)
        {
            if (request != null)
            {
                request.DataAvailable -= request_DataAvailable;
                request.Complete -= request_Complete;
            }

            request = r;

            if (r != null)
            {
                requesting = true;

                r.DataAvailable += request_DataAvailable;
                r.Complete += request_Complete;

                if ((r.State & (Gw2Launcher.Api.ApiData.DataRequest.RequestState.Complete | Gw2Launcher.Api.ApiData.DataRequest.RequestState.Aborted)) != 0)
                {
                    SetRequest(null);
                }
            }
            else
            {
                requesting = false;
            }
        }

        void request_Complete(object sender, EventArgs e)
        {
            SetRequest(null);
        }

        void request_DataAvailable(object sender, Api.ApiData.RequestDataAvailableEventArgs e)
        {
            SetRequest(null);
        }

        private bool Start(DateTime last)
        {
            var n = DateTime.UtcNow;

            for (sbyte i = 0; i < dates.Length; i++)
            {
                if (n >= dates[i])
                    continue;

                if (last != DateTime.MinValue)
                {
                    start = Environment.TickCount - (int)n.Subtract(last).TotalMilliseconds;
                    limit = (float)dates[i].Subtract(last).TotalMilliseconds + 1;
                }
                else
                {
                    start = Environment.TickCount;
                    limit = (float)dates[i].Subtract(n).TotalMilliseconds + 1;
                }

                current = i;
                active = true;

                if (enabled)
                {
                    if (Tick != null)
                        Tick(this, EventArgs.Empty);

                    Util.ScheduledEvents.Register(OnScheduledTick, Frequency);
                }

                return true;
            }

            if (active)
            {
                active = false;

                if (enabled)
                {
                    if (Tick != null)
                        Tick(this, EventArgs.Empty);
                }

                return true;
            }

            return false;
        }

        private Util.ScheduledEvents.Ticks OnScheduledTick()
        {
            if (enabled)
            {
                if (active)
                {
                    if (Environment.TickCount - start > limit)
                    {
                        Start(DateTime.MinValue);
                    }
                    else
                    {
                        if (Tick != null)
                            Tick(this, EventArgs.Empty);

                        return new Util.ScheduledEvents.Ticks(Frequency);
                    }
                }
            }

            return Util.ScheduledEvents.Ticks.None;
        }

        public void Reset()
        {
            for (var i = 0; i < dates.Length; i++)
            {
                dates[i] = DateTime.MinValue;
            }
            current = -1;
            if (active)
            {
                active = false;
                if (enabled)
                {
                    if (Tick != null)
                        Tick(this, EventArgs.Empty);
                }
            }
        }

        public void Restart(DateTime last)
        {
            current = -1;
            Start(last);
        }

        public void SetTimer(DelayType t, DateTime d, DateTime last)
        {
            //var b = date == -1 || d > dates[date] || date == (byte)t && DateTime.UtcNow < dates[date];

            if (dates[(byte)t] != d)
            {
                var n = DateTime.UtcNow;

                if (n < d)
                {
                    var b = current == -1 || current >= (byte)t && dates[current] != d || n > dates[current];

                    dates[(byte)t] = d;

                    if (b)
                        Start(last);
                }
            }


            //b = date == (byte)t && d != dates[date] || d > n && (date == -1 || n > dates[date]);

            //dates[(byte)t] = d;
            //if (b)
            //    Start(last);
        }

        public void SetTimer(DelayType t, DateTime d)
        {
            SetTimer(t, d, DateTime.MinValue);
        }

        public bool Active
        {
            get
            {
                return active;
            }
        }

        /// <summary>
        /// Refresh rate for drawing
        /// </summary>
        public int Frequency
        {
            get
            {
                var f = (int)(limit / 60);
                if (f > 5000)
                    return 5000;
                else if (f < 1000)
                    return 1000;
                else
                    return f;
            }
        }

        public float Progress
        {
            get
            {
                if (active)
                {
                    var v = (Environment.TickCount - start) / limit;

                    if (v > 1)
                    {
                        return 1;
                    }
                    else if (v < 0)
                        return 0;
                    else
                        return v;
                }
                else if (requesting)
                {
                    return 1;
                }
                else
                {
                    return 0;
                }
            }
        }

        public Settings.ApiDataKey Api
        {
            get
            {
                return api;
            }
            set
            {
                api = value;
            }
        }

        /// <summary>
        /// API has pending requests
        /// </summary>
        public bool Pending
        {
            get
            {
                return pending;
            }
            set
            {
                pending = value;
            }
        }

        /// <summary>
        /// API is being updated
        /// </summary>
        public bool Updating
        {
            get
            {
                return updating;
            }
            set
            {
                updating = value;
            }
        }

        /// <summary>
        /// The supplied API request is pending
        /// </summary>
        public bool Requesting
        {
            get
            {
                return requesting;
            }
            set
            {
                requesting = value;
            }
        }

        /// <summary>
        /// A timer is active
        /// </summary>
        public bool Ticking
        {
            get
            {
                return active;
            }
        }

        /// <summary>
        /// Pending bounds update
        /// </summary>
        public bool Resize
        {
            get
            {
                return resize;
            }
            set
            {
                resize = value;
            }
        }

        public Rectangle Bounds
        {
            get
            {
                return bounds;
            }
            set
            {
                bounds = value;
            }
        }

        public void Draw(Graphics g)
        {
            using (var p = new Pen(Color.FromArgb(120, 120, 120), 1))
            {
                var m = g.SmoothingMode;

                g.SmoothingMode = SmoothingMode.HighQuality;

                var psize = p.Width + 1;
                var phalf = (psize - 1) / 2;

                //warning: DrawArc can cause out of memory error on certain values of sweepAngle when value is < 1.0
                g.DrawArc(p, bounds.X + phalf, bounds.Y + phalf, bounds.Width - psize, bounds.Height - psize, 0f, 360f);

                if (active)
                {
                    p.Width = 2;
                    p.Color = Color.FromArgb(200, 200, 200);
                    g.DrawArc(p, bounds.X + phalf, bounds.Y + phalf, bounds.Width - psize, bounds.Height - psize, -90f, (int)(360 * Progress));
                }

                g.SmoothingMode = m;
            }
        }

        public void Dispose()
        {
            if (manager != null)
            {
                manager.PendingChanged -= manager_PendingChanged;
                manager.NextRequestChanged -= manager_NextRequestChanged;
                manager.DelayChanged -= manager_DelayChanged;
                manager.EndUpdate -= manager_EndUpdate;
                manager.BeginUpdate -= manager_BeginUpdate;
                manager = null;
            }

            SetRequest(null);

            if (active && enabled)
            {
                Util.ScheduledEvents.Unregister(OnScheduledTick);
            }

            Tick = null;
        }
    }
}
