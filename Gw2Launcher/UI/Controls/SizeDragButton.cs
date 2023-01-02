using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Gw2Launcher.Windows.Native;

namespace Gw2Launcher.UI.Controls
{
    class SizeDragButton : Base.BaseControl
    {
        public event EventHandler<Point> DragOffsetChanged;
        public event EventHandler BeginDrag;
        public event EventHandler EndDrag;

        private bool resizing;
        private Point origin;

        public SizeDragButton()
        {
            SetStyle(ControlStyles.Selectable, false);
            SetStyle(ControlStyles.ResizeRedraw, true);

            this.Cursor = System.Windows.Forms.Cursors.SizeNWSE;
            this.Size = new Size(SystemInformation.VerticalScrollBarWidth, SystemInformation.HorizontalScrollBarHeight);
            this.ForeColor = Color.DarkGray;
            this.Padding = new Padding(0, 0, 2, 2);
        }

        public bool Transparent
        {
            get;
            set;
        }

        [System.ComponentModel.Browsable(false)]
        [System.ComponentModel.DesignerSerializationVisibility(System.ComponentModel.DesignerSerializationVisibility.Hidden)]
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

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);

            var g = e.Graphics;
            var scale = g.DpiX / 96f;
            var px = (int)(scale + 0.5f);
            var sz = px * 2;

            using (var brush = new SolidBrush(this.ForeColor))
            {
                var x = this.Width - this.Padding.Right - sz;
                var h = this.Height - this.Padding.Bottom - sz;

                for (var ix = 0; ix < 3; ++ix)
                {
                    var y = h;

                    for (var iy = 2 - ix; iy >= 0; --iy)
                    {
                        g.FillRectangle(brush, x, y, sz, sz);

                        y -= px + sz;
                    }

                    x -= px + sz;
                }
            }
        }

        protected override void OnMouseDown(MouseEventArgs e)
        {
            base.OnMouseDown(e);

            if (e.Button == System.Windows.Forms.MouseButtons.Left)
            {
                origin = Cursor.Position;
                resizing = true;
                if (BeginDrag != null)
                    BeginDrag(this, EventArgs.Empty);
            }
        }

        protected override void OnMouseUp(MouseEventArgs e)
        {
            base.OnMouseUp(e);

            if (e.Button == System.Windows.Forms.MouseButtons.Left)
            {
                if (resizing)
                {
                    resizing = false;
                    if (EndDrag != null)
                        EndDrag(this, EventArgs.Empty);
                }
            }
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);

            if (resizing)
            {
                if (DragOffsetChanged != null)
                    DragOffsetChanged(this, new Point(Cursor.Position.X - origin.X, Cursor.Position.Y - origin.Y));
            }
        }

        protected override void WndProc(ref Message m)
        {
            base.WndProc(ref m);

            if (m.Msg == (int)WindowMessages.WM_NCHITTEST)
            {
                if (Transparent)
                {
                    m.Result = (IntPtr)HitTest.Transparent;
                }
            }
        }
    }
}
