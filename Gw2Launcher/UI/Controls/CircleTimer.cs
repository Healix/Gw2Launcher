using System;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.ComponentModel;

namespace Gw2Launcher.UI.Controls
{
    class CircleTimer : Control
    {
        protected const int ANIM_MS = 1500;
        protected const int DURATION_MS = 30000;
        protected const float PROGRESS_ANIM_BEGIN = (float)(ANIM_MS * 0.25f) / DURATION_MS;
        protected const float PROGRESS_ANIM_END = (float)(DURATION_MS - ANIM_MS * 0.75f) / DURATION_MS;
        protected const float PROGRESS_ANIM_SUM = 1 - PROGRESS_ANIM_END + PROGRESS_ANIM_BEGIN;

        private event EventHandler Stopped;

        protected Pen penStroke, penTrack;

        protected float _Value;
        //protected byte _b;
        //protected string _text;

        public CircleTimer()
        {
            SetStyle(ControlStyles.AllPaintingInWmPaint, true);
            SetStyle(ControlStyles.UserPaint, true);
            SetStyle(ControlStyles.OptimizedDoubleBuffer, true);

            //_b = 255;
            //_text = "";

            penStroke = new Pen(Color.SteelBlue, 6)
            {
                StartCap = LineCap.Round,
                EndCap = LineCap.Round,
            };
            penTrack = new Pen(Color.FromArgb(230, 230, 230), 8)
            {
                StartCap = LineCap.Round,
                EndCap = LineCap.Round,
            };
        }

        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public override string Text
        {
            get
            {
                return base.Text;
            }
            set
            {
                base.Text = value;
            }
        }

        [DefaultValue(6)]
        public float StrokeSize
        {
            get
            {
                return penStroke.Width;
            }
            set
            {
                if (penStroke.Width != value)
                {
                    penStroke.Width = value;
                    this.Invalidate();
                }
            }
        }

        [DefaultValue(typeof(Color), "SteelBlue")]
        public Color StrokeColor
        {
            get
            {
                return penStroke.Color;
            }
            set
            {
                if (penStroke.Color != value)
                {
                    penStroke.Color = value;
                    this.Invalidate();
                }
            }
        }

        [DefaultValue(8)]
        public float TrackSize
        {
            get
            {
                return penTrack.Width;
            }
            set
            {
                if (penTrack.Width != value)
                {
                    penTrack.Width = value;
                    this.Invalidate();
                }
            }
        }

        [DefaultValue(typeof(Color), "230, 230, 230")]
        public Color TrackColor
        {
            get
            {
                return penTrack.Color;
            }
            set
            {
                if (penTrack.Color != value)
                {
                    penTrack.Color = value;
                    this.Invalidate();
                }
            }
        }

        protected override void ScaleControl(SizeF factor, BoundsSpecified specified)
        {
            base.ScaleControl(factor, specified);

            var scale = factor.Width;
            if (scale != 1f && specified == BoundsSpecified.All)
            {
                TrackSize = TrackSize * scale;
                StrokeSize = StrokeSize * scale;
            }
        }

        private bool isActive;
        public async void Start()
        {
            if (isActive)
                return;
            isActive = true;

            var b = true;
            EventHandler onStopped = delegate
            {
                b = false;
            };
            Stopped += onStopped;

            while (b)
            {
                var t = (DateTime.UtcNow.Ticks / 10000) % DURATION_MS;
                //var b = (byte)(t / 1000);

                //if (b != _b)
                //{
                //    _b = b;
                //    _text = (DURATION_MS / 1000 - b).ToString();
                //}

                this._Value = 1 - t / (float)DURATION_MS;
                this.Invalidate();

                await Task.Delay(50);
            }

            Stopped -= onStopped;
        }

        public void Stop()
        {
            if (!isActive)
                return;
            isActive = false;
            this.Invalidate();

            if (Stopped != null)
                Stopped(this, EventArgs.Empty);
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);

            var g = e.Graphics;

            g.SmoothingMode = SmoothingMode.HighQuality;

            var psize = penTrack.Width + 1;
            var phalf = (psize - 1) / 2;

            g.DrawArc(penTrack, phalf, phalf, this.Width - psize, this.Height - psize, 0f, 360f);

            if (isActive)
            {
                //warning: DrawArc can cause out of memory error on certain values of sweepAngle when value is < 1.0
                if (_Value > PROGRESS_ANIM_END)
                {
                    var ofs = (float)(360 * (Math.Sin(Math.PI * (_Value - PROGRESS_ANIM_END) / PROGRESS_ANIM_SUM - Math.PI / 2) / 2 + 0.5));
                    g.DrawArc(penStroke, phalf, phalf, this.Width - psize, this.Height - psize, -90f + ofs, (int)(360 * _Value - ofs));
                }
                else if (_Value < PROGRESS_ANIM_BEGIN)
                {
                    var ofs = (float)(360 * (Math.Sin(Math.PI * (_Value + (1 - PROGRESS_ANIM_END)) / PROGRESS_ANIM_SUM - Math.PI / 2) / 2 + 0.5));
                    g.DrawArc(penStroke, phalf, phalf, this.Width - psize, this.Height - psize, -90f + ofs,  (int)(360 - ofs + 360 * _Value));
                }
                else
                {
                    g.DrawArc(penStroke, phalf, phalf, this.Width - psize, this.Height - psize, -90f,  (int)(360 * _Value));
                }
            }

            //TextRenderer.DrawText(g, _text, this.Font, new Rectangle(0, 0, this.Width, this.Height), this.ForeColor, Color.Transparent, TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter | TextFormatFlags.NoPadding);
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);

            if (disposing)
            {
                penStroke.Dispose();
                penTrack.Dispose();
            }
        }
    }
}
