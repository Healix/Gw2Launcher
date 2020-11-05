using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using System.Windows.Forms;
using System.ComponentModel;

namespace Gw2Launcher.UI
{
    class formCompassOverlay : Base.BaseForm
    {
        private BufferedGraphics buffer;
        private bool redraw;
        private Rectangle bounds;
        private Point[] shape1;
        private Point[] shape2;
        private Control pointAt;
        private Form parent;

        public formCompassOverlay(Form parent, Control pointAt)
        {
            InitializeComponents();

            SetStyle(ControlStyles.UserPaint, true);
            SetStyle(ControlStyles.AllPaintingInWmPaint, true);

            this.pointAt = pointAt;

            pointAt.LocationChanged += pointAt_LocationChanged;
            pointAt.SizeChanged += pointAt_SizeChanged;

            this.parent = parent;

            this.Location = parent.PointToScreen(new Point(0, 0));
            this.Size = parent.ClientSize;

            parent.SizeChanged += parent_SizeChanged;
            parent.LocationChanged += parent_LocationChanged;
            parent.VisibleChanged += parent_VisibleChanged;
        }

        void pointAt_SizeChanged(object sender, EventArgs e)
        {
            OnRedrawRequired();
        }

        void pointAt_LocationChanged(object sender, EventArgs e)
        {
            OnRedrawRequired();
        }

        protected override void OnInitializeComponents()
        {
            base.OnInitializeComponents();

            redraw = true;

            this.StartPosition = FormStartPosition.Manual;
            this.ShowInTaskbar = false;
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.Opacity = 0.9;

            this.BackColor = Color.Black;
            this.ForeColor = Color.White;
        }

        protected override void OnSizeChanged(EventArgs e)
        {
            base.OnSizeChanged(e);
            if (buffer != null)
            {
                buffer.Dispose();
                buffer = null;
            }
            OnRedrawRequired();
        }

        private void OnRedrawRequired()
        {
            if (redraw)
                return;
            redraw = true;
            Invalidate();
        }

        protected override void OnPaintBackground(System.Windows.Forms.PaintEventArgs e)
        {
            if (redraw)
            {
                redraw = false;

                if (buffer == null)
                {
                    buffer = BufferedGraphicsManager.Current.Allocate(e.Graphics, this.ClientRectangle);
                    buffer.Graphics.Clear(this.BackColor);

                    var h = Scale(30);
                    var w = h * 3 / 4;
                    var sz = h * 3;

                    bounds = new Rectangle((this.Width - sz) / 2, (this.Height - sz) / 2, sz, sz);

                    shape1 = new Point[]
                    {
                        new Point(0, -h),
                        new Point(w, h),
                        new Point(0, h / 2),
                        new Point(-w, h),
                    };

                    shape2 = new Point[3];
                    Array.Copy(shape1, shape2, shape2.Length);
                }
                else
                {
                    buffer.Graphics.FillRectangle(Brushes.Black, bounds);
                }

                var g = buffer.Graphics;

                var c = PointToScreen(new Point(bounds.Left + bounds.Width / 2, bounds.Top + bounds.Height / 2));
                var p = pointAt.PointToScreen(new Point(pointAt.Width / 2, pointAt.Height / 2));
                var a = Math.Atan2(p.Y - c.Y,  p.X - c.X) * 180 / Math.PI + 90;

                g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
                g.TranslateTransform(this.Width / 2, this.Height / 2);
                g.RotateTransform((float)a);

                using (var brush = new SolidBrush(Color.FromArgb(240, 240, 240)))
                {
                    using (var pen = new Pen(Color.FromArgb(140, 140, 140), 2))
                    {
                        g.FillPolygon(brush, shape1);
                        g.FillPolygon(Brushes.White, shape2);
                        g.DrawPolygon(pen, shape1);
                    }
                }

                g.ResetTransform();
            }

            buffer.Render(e.Graphics);
        }

        protected override void OnPaint(System.Windows.Forms.PaintEventArgs e)
        {
            base.OnPaint(e);
        }

        protected override bool ShowWithoutActivation
        {
            get
            {
                return true;
            }
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            foreach (var f in this.OwnedForms)
            {
                this.RemoveOwnedForm(f);
                f.Owner = this.Owner;
            }

            base.OnClosing(e);
        }

        void parent_VisibleChanged(object sender, EventArgs e)
        {
            this.Visible = parent.Visible;
        }

        void parent_LocationChanged(object sender, EventArgs e)
        {
            this.Location = parent.PointToScreen(new Point(0, 0));
        }

        void parent_SizeChanged(object sender, EventArgs e)
        {
            this.Size = parent.ClientSize;
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (parent != null)
                {
                    parent.SizeChanged -= parent_SizeChanged;
                    parent.LocationChanged -= parent_LocationChanged;
                    parent.VisibleChanged -= parent_VisibleChanged;
                    parent = null;
                }
                if (pointAt != null)
                {
                    pointAt.LocationChanged -= pointAt_LocationChanged;
                    pointAt.SizeChanged -= pointAt_SizeChanged;
                    pointAt = null;
                }
            }

            base.Dispose(disposing);

            if (disposing)
            {
                if (buffer != null)
                    buffer.Dispose();
            }
        }
    }
}
