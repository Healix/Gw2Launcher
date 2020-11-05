using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Drawing;
using System.Runtime.InteropServices;
using Gw2Launcher.Windows.Native;

namespace Gw2Launcher.UI
{
    class formSizingBox : Base.ShowWithoutActivationForm
    {
        private Form form;
        private short offsetLocation, offsetSize;
        private int barSize;

        public formSizingBox(Form form)
        {
            SetStyle(ControlStyles.UserPaint, true);
            SetStyle(ControlStyles.AllPaintingInWmPaint, true);
            SetStyle(ControlStyles.OptimizedDoubleBuffer, true);
            SetStyle(ControlStyles.ResizeRedraw, true);

            this.Opacity = 0;
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.ShowInTaskbar = false;
            this.TransparencyKey = this.BackColor = Color.Red;

            this.form = form;

            offsetSize = 70;
            offsetLocation = 35;

            this.MinimumSize = new Size(form.MinimumSize.Width + offsetSize, form.MinimumSize.Height + offsetSize);
            this.MaximumSize = new Size(form.MaximumSize.Width > 0 ? form.MaximumSize.Width + offsetSize : 0, form.MaximumSize.Height > 0 ? form.MaximumSize.Height + offsetSize : 0);

            var handle = this.Handle; //force

            this.Bounds = new Rectangle(form.Left - offsetLocation, form.Top - offsetLocation, form.Width + offsetSize, form.Height + offsetSize);
        }

        protected override void OnScale(float scale)
        {
            base.OnScale(scale);

            barSize = Scale(20);
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            if (e.CloseReason == CloseReason.UserClosing)
            {
                e.Cancel = true;
            }
            base.OnFormClosing(e);
        }

        protected override void OnPaintBackground(PaintEventArgs e)
        {
            var g = e.Graphics;
            var scale = g.DpiX / 96f;

            g.Clear(Color.Red);

            var b = Brushes.DarkGray;

            var w = this.Width;
            var h = this.Height;
            var bthickness = (int)(11 * scale + 0.5f);
            var bsize = (int)(25 * scale + 0.5f);
            var border = bthickness - bthickness / 3 * 2; //bthickness - (int)(3 * scale + 0.5f) * 2;
            var btb2 = (bthickness - border) / 2;

            //left border
            g.FillRectangle(Brushes.Black, btb2, bthickness, border, h - bthickness * 2);
            //right border
            g.FillRectangle(Brushes.Black, w - border - btb2, bthickness, border, h - bthickness * 2);
            //top border
            g.FillRectangle(Brushes.Black, bthickness, btb2, w - bthickness * 2, border);
            //bottom border
            g.FillRectangle(Brushes.Black, bthickness, h - border - btb2, w - bthickness * 2, border);

            //left bar
            g.FillRectangle(b, 0, h / 2 - bsize / 2, bthickness, bsize);
            //right bar
            g.FillRectangle(b, w - bthickness, h / 2 - bsize / 2, bthickness, bsize);
            //top bar
            g.FillRectangle(b, w / 2 - bsize / 2, 0, bsize, bthickness);
            //bottom bar
            g.FillRectangle(b, w / 2 - bsize / 2, h - bthickness, bsize, bthickness);

            //top left
            g.FillRectangle(b, 0, 0, bthickness * 2, bthickness);
            g.FillRectangle(b, 0, bthickness, bthickness, bthickness);

            //top right
            g.FillRectangle(b, w - bthickness * 2, 0, bthickness * 2, bthickness);
            g.FillRectangle(b, w - bthickness, bthickness, bthickness, bthickness);

            //bottom left
            g.FillRectangle(b, 0, h - bthickness, bthickness * 2, bthickness);
            g.FillRectangle(b, 0, h - bthickness * 2, bthickness, bthickness);

            //bottom right
            g.FillRectangle(b, w - bthickness * 2, h - bthickness, bthickness * 2, bthickness);
            g.FillRectangle(b, w - bthickness, h - bthickness * 2, bthickness, bthickness);

            b = Brushes.Black;

            var bt2 = bthickness / 2;
            var bspacing = (bthickness - bt2) / 2;
            var px = (int)(scale + 0.5f);
            var px2 = px * 2;
            int x , y;

            bsize = bthickness - bspacing * 2;
            y = h / 2 - px / 2;

            //left bar
            x = bspacing;
            g.FillRectangle(b, x, y - px2, bsize, px);
            g.FillRectangle(b, x, y, bsize, px);
            g.FillRectangle(b, x, y + px2, bsize, px);

            //right bar
            x = w - bthickness + bspacing;
            g.FillRectangle(b, x, y - px2, bsize, px);
            g.FillRectangle(b, x, y, bsize, px);
            g.FillRectangle(b, x, y + px2, bsize, px);

            x = w / 2 - px / 2;

            //top bar
            g.FillRectangle(b, x - px2, bspacing, px, bsize);
            g.FillRectangle(b, x, bspacing, px, bsize);
            g.FillRectangle(b, x + px2, bspacing, px, bsize);

            //bottom bar
            y = h - bthickness + bspacing;
            g.FillRectangle(b, x - px2, y, px, bsize);
            g.FillRectangle(b, x, y, px, bsize);
            g.FillRectangle(b, x + px2, y, px, bsize);


            //top left arrow
            g.FillPolygon(b, new Point[]
            {
                new Point(bt2,bt2),
                new Point(bthickness,bt2),
                new Point(bt2,bthickness),
            });

            //top right arrow
            g.FillPolygon(b, new Point[]
            {
                new Point(w-bt2,bt2),
                new Point(w-bt2,bthickness),
                new Point(w-bthickness,bt2),
            });

            //bottom left arrow
            g.FillPolygon(b, new Point[]
            {
                new Point(bt2,h-bt2),
                new Point(bthickness+1,h-bt2),
                new Point(bt2,h-bthickness-1),
            });

            //bottom right arrow
            g.FillPolygon(b, new Point[] 
            {
                new Point(w-bt2,h-bt2),
                new Point(w-bt2,h-bthickness-1),
                new Point(w-bthickness-1,h-bt2) 
            });
        }

        protected override void OnShown(EventArgs e)
        {
            base.OnShown(e);

            this.Opacity = 0.95;
        }

        protected override void WndProc(ref Message m)
        {
            switch ((WindowMessages)m.Msg)
            {
                case WindowMessages.WM_NCLBUTTONDBLCLK:

                    break;
                case WindowMessages.WM_NCHITTEST:
                    
                    var p = this.PointToClient(new Point(m.LParam.GetValue32()));
                    var size = this.ClientSize;

                    if (p.X <= barSize)
                    {
                        if (p.Y <= barSize)
                            m.Result = (IntPtr)HitTest.TopLeft;
                        else if (p.Y >= size.Height - barSize)
                            m.Result = (IntPtr)HitTest.BottomLeft;
                        else
                            m.Result = (IntPtr)HitTest.Caption;
                    }
                    else if (p.X >= size.Width - barSize)
                    {
                        if (p.Y <= barSize)
                            m.Result = (IntPtr)HitTest.TopRight;
                        else if (p.Y >= size.Height - barSize)
                            m.Result = (IntPtr)HitTest.BottomRight;
                        else
                            m.Result = (IntPtr)HitTest.Caption;
                    }
                    else
                    {
                        m.Result = (IntPtr)HitTest.Caption;
                    }

                    break;
                case WindowMessages.WM_WINDOWPOSCHANGING:

                    var pos = (WINDOWPOS)Marshal.PtrToStructure(m.LParam, typeof(WINDOWPOS));
                    var bs = BoundsSpecified.None;

                    if (!pos.flags.HasFlag(SetWindowPosFlags.SWP_NOMOVE))
                        bs |= BoundsSpecified.Location;

                    if (!pos.flags.HasFlag(SetWindowPosFlags.SWP_NOSIZE))
                    {
                        bs |= BoundsSpecified.Size;

                        bool cw, ch;

                        if (cw = this.MinimumSize.Width > 0 && pos.cx < this.MinimumSize.Width)
                            pos.cx = this.MinimumSize.Width;
                        if (!cw && (cw = this.MaximumSize.Width > 0 && pos.cx > this.MaximumSize.Width))
                            pos.cx = this.MaximumSize.Width;

                        if (ch = this.MinimumSize.Height > 0 && pos.cy < this.MinimumSize.Height)
                            pos.cy = this.MinimumSize.Height;
                        if (!ch && (ch = this.MaximumSize.Height > 0 && pos.cy > this.MaximumSize.Height))
                            pos.cy = this.MaximumSize.Height;

                        if (cw || ch)
                            Marshal.StructureToPtr(pos, m.LParam, false);
                    }

                    if (bs != BoundsSpecified.None)
                    {
                        form.SetBounds(
                            pos.x + offsetLocation,
                            pos.y + offsetLocation,
                            pos.cx - offsetSize,
                            pos.cy - offsetSize, 
                            bs);
                    }

                    break;
                default:

                    base.WndProc(ref m);

                    break;
            }
        }
    }
}
