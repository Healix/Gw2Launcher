using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Gw2Launcher.UI.Controls
{
    public partial class SizeDragButton : Control
    {
        public event EventHandler<Point> DragOffsetChanged;
        public event EventHandler BeginDrag;
        public event EventHandler EndDrag;

        private bool resizing;
        private Point origin;

        public SizeDragButton()
        {
            InitializeComponent();

            SetStyle(ControlStyles.Selectable, false);
            this.Size = new Size(SystemInformation.VerticalScrollBarWidth, SystemInformation.HorizontalScrollBarHeight);
        }

        [System.ComponentModel.Browsable(false)]
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

        protected override void OnSizeChanged(EventArgs e)
        {
            base.OnSizeChanged(e);
            this.Invalidate();
        }

        protected override void OnPaintBackground(PaintEventArgs e)
        {
            base.OnPaintBackground(e);

            var g = e.Graphics;
            var w = this.Width;
            var h = this.Height;

            for (var ix = 0; ix < 3; ix++)
            {
                var x = w - 4 - ix * 3;

                for (var iy = 2-ix; iy >= 0; iy--)
                {
                    var y = h - 4 - iy * 3;

                    g.FillRectangle(Brushes.DarkGray, x, y, 2, 2);
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

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (components != null)
                    components.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
