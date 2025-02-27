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

        private Settings.ApiDataKey api;
        private DateTime date;
        private DateTime[] dates;
        private int start;
        private float limit;
        private Rectangle bounds;
        private bool resize;
        private bool active;
        private bool pending;
        private Api.ApiData manager;
        private Api.ApiData.DataType type;
        private bool enabled;

        public ApiTimer(Settings.IAccount account = null, Api.ApiData manager = null, Api.ApiData.DataType type = ANY_TYPE)
        {
            this.manager = manager;
            this.enabled = true;

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
            }
        }

        void manager_DelayChanged(object sender, Api.ApiData.ApiDataEventArgs e)
        {
            if (object.ReferenceEquals(this.api, e.Key))
            {
                SetTimer(DelayType.Pending, e.Delay);
            }
        }

        void manager_NextRequestChanged(object sender, Api.ApiData.ApiDataEventArgs e)
        {
            if (object.ReferenceEquals(this.api, e.Key) && DateTime.UtcNow < e.NextRequest)
            {
                SetTimer(DelayType.Cached, e.NextRequest);
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
                            Restart();
                        }
                    }

                    if (e.Delay != DateTime.MinValue)
                    {
                        SetTimer(DelayType.Pending, e.Delay);
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
                    api = a;
                    this.type = type;

                    if (a != null && manager != null)
                    {
                        var cache = manager.GetCache(a.Key);

                        if (cache != null)
                        {
                            this.pending = type == ANY_TYPE ? cache.Pending != 0 : cache.GetPending(type) != 0;
                            
                            dates[0] = cache.NextRequest;
                            dates[1] = cache.Delay;

                            Restart();
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

                if (active)
                {
                    Reset();
                }
            }
        }

        private bool Start()
        {
            var n = DateTime.UtcNow;
            var i = 0;

            while (n >= dates[i])
            {
                if (++i == dates.Length)
                {
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
            }

            if ((date.Ticks - dates[i].Ticks) / 10000000 != 0)
            {
                date = dates[i];
                start = Environment.TickCount;
                limit = (float)date.Subtract(n).TotalMilliseconds + 1;
                active = limit > 0;

                if (enabled)
                {
                    if (Tick != null)
                        Tick(this, EventArgs.Empty);

                    Util.ScheduledEvents.Register(OnScheduledTick, Frequency);
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
                    if (Tick != null)
                        Tick(this, EventArgs.Empty);
                }

                if (Active)
                {
                    return new Util.ScheduledEvents.Ticks(Frequency);
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
            date = DateTime.MinValue;
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

        public void Restart()
        {
            date = DateTime.MinValue;
            Start();
        }

        private void Start(DateTime d)
        {
            date = d;
            start = Environment.TickCount;
            limit = (float)d.Subtract(DateTime.UtcNow).TotalMilliseconds + 1;
            if (limit < 1)
                limit = 1;
        }

        public void SetTimer(DelayType t, DateTime d)
        {
            var b = d > date || dates[(byte)t] == date && DateTime.UtcNow < date;
            dates[(byte)t] = d;
            if (b)
                Start();
        }

        public bool Active
        {
            get
            {
                if (active)
                {
                    if (Environment.TickCount - start > limit)
                    {
                        Start();
                    }
                }

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
                        Start();
                        return 1;
                    }
                    else if (v < 0)
                        return 0;
                    else
                        return v;
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
                manager = null;
            }

            if (active && enabled)
            {
                Util.ScheduledEvents.Unregister(OnScheduledTick);
            }

            Tick = null;
        }
    }
}
