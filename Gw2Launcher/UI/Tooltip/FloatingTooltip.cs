using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using System.Windows.Forms;
using Gw2Launcher.Windows.Native;

namespace Gw2Launcher.UI.Tooltip
{
    class FloatingTooltip : Base.BaseForm
    {
        private string text;
        private Rectangle bounds;
        private Point cursor;
        private Control control;

        public FloatingTooltip()
        {
            SetStyle(ControlStyles.AllPaintingInWmPaint, true);
            SetStyle(ControlStyles.OptimizedDoubleBuffer, true);
            SetStyle(ControlStyles.ResizeRedraw, true);
            SetStyle(ControlStyles.UserPaint, true);

            this.ShowInTaskbar = false;
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.Opacity = 0.9;
            this.Padding = new System.Windows.Forms.Padding(5, 5, 5, 5);
            this.ForeColor = Color.White;
            this.BackColor = Color.Black;

            InitializeComponents();
        }

        protected override void OnInitializeComponents()
        {
            base.OnInitializeComponents();
        }

        protected override CreateParams CreateParams
        {
            get
            {
                var cp = base.CreateParams;

                cp.ExStyle |= (int)(WindowStyle.WS_EX_NOACTIVATE | WindowStyle.WS_EX_TOOLWINDOW | WindowStyle.WS_EX_TRANSPARENT);

                return cp;
            }
        }

        protected override bool ShowWithoutActivation
        {
            get
            {
                return true;
            }
        }

        public void ShowTooltip(Control control, string text)
        {
            var location = control.PointToScreen(Point.Empty);

            AttachedTo = control;

            this.text = text;

            using (var g = this.CreateGraphics())
            {
                var screen = Screen.FromPoint(location).WorkingArea;
                var mw = this.MaximumSize.Width;
                if (mw == 0)
                    mw = screen.Width / 4;

                var sz = TextRenderer.MeasureText(g, text, this.Font, new Size(mw, 0));
                this.bounds = new Rectangle(this.Padding.Left, this.Padding.Top, sz.Width, sz.Height);
            }

            this.Invalidate();
            MoveToCursor();

            if (!this.Visible)
                this.Show(control);
        }

        private Control AttachedTo
        {
            get
            {
                return control;
            }
            set
            {
                if (control != null)
                {
                    control.MouseMove -= control_MouseMove;
                    control.MouseLeave -= control_MouseLeave;
                }
                if (value != null)
                {
                    value.MouseMove += control_MouseMove;
                    value.MouseLeave += control_MouseLeave;
                }
                control = value;
            }
        }

        protected override void OnVisibleChanged(EventArgs e)
        {
            base.OnVisibleChanged(e);

            if (!this.Visible)
                AttachedTo = null;
        }

        void control_MouseLeave(object sender, EventArgs e)
        {
            this.Hide();
        }

        void control_MouseMove(object sender, MouseEventArgs e)
        {
            MoveToCursor();
        }

        private void MoveToCursor()
        {
            var location = Cursor.Position;

            if (cursor == location)
                return;
            cursor = location;

            var cw = Cursor.Size.Width / 2;
            var screen = Screen.FromPoint(location).WorkingArea;

            int x,
                y,
                w = this.bounds.Width + this.Padding.Horizontal,
                h = this.bounds.Height + this.Padding.Vertical;

            x = location.X + cw;
            y = location.Y - cw;

            if (y + h > screen.Height)
                y = screen.Height - h;

            if (x + w > screen.Width)
                x = location.X - w - cw;

            this.Bounds = new Rectangle(x, y, bounds.Width + this.Padding.Horizontal, bounds.Height + this.Padding.Vertical);
        }

        protected override void OnPaint(System.Windows.Forms.PaintEventArgs e)
        {
            base.OnPaint(e);

            if (text != null)
            {
                TextRenderer.DrawText(e.Graphics, text, this.Font, bounds, this.ForeColor, this.BackColor, TextFormatFlags.Left);
            }
        }
    }
}
