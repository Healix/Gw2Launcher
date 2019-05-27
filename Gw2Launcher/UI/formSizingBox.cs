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
    class formSizingBox : ShowWithoutActivationForm
    {
        private Form form;
        private short offsetLocation, offsetSize;

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

        protected override void OnPaintBackground(PaintEventArgs e)
        {
            var g = e.Graphics;
            g.Clear(Color.Red);

            var p = Pens.Black;
            var b = Brushes.DarkGray;

            var w = this.Width;
            var h = this.Height;

            g.DrawRectangle(Pens.Black, 4, 4, w - 9, h - 9);
            g.DrawRectangle(Pens.Black, 6, 6, w - 13, h - 13);
            g.DrawRectangle(Pens.White, 5, 5, w - 11, h - 11);

            var bh = 25;
            //left bar
            g.FillRectangle(b, 0, h / 2 - bh / 2, 11, bh);
            //right bar
            g.FillRectangle(b, w - 11, h / 2 - bh / 2, 11, bh);
            //top bar
            g.FillRectangle(b, w / 2 - bh / 2, 0, bh, 11);
            //bottom bar
            g.FillRectangle(b, w / 2 - bh / 2, h - 11, bh, 11);

            var y = h / 2;
            var x = 10 / 2;

            //left bar
            g.DrawLine(p, x - 2, y - 2, x + 2, y - 2);
            g.DrawLine(p, x - 2, y, x + 2, y);
            g.DrawLine(p, x - 2, y + 2, x + 2, y + 2);

            //right bar
            x = w - 10 / 2 - 1;
            g.DrawLine(p, x - 2, y - 2, x + 2, y - 2);
            g.DrawLine(p, x - 2, y, x + 2, y);
            g.DrawLine(p, x - 2, y + 2, x + 2, y + 2);

            //top bar
            y = 11 / 2;
            x = w / 2;
            g.DrawLine(p, x - 2, y - 2, x - 2, y + 2);
            g.DrawLine(p, x, y - 2, x, y + 2);
            g.DrawLine(p, x + 2, y - 2, x + 2, y + 2);

            //bottom bar
            y = h - 11 / 2 - 1;
            g.DrawLine(p, x - 2, y - 2, x - 2, y + 2);
            g.DrawLine(p, x, y - 2, x, y + 2);
            g.DrawLine(p, x + 2, y - 2, x + 2, y + 2);

            //top left
            g.FillRectangle(b, 0, 0, 20, 11);
            g.FillRectangle(b, 0, 11, 11, 9);

            //top right
            g.FillRectangle(b, w - 20, 0, 20, 11);
            g.FillRectangle(b, w - 11, 11, 11, 9);

            //bottom left
            g.FillRectangle(b, 0, h - 11, 20, 11);
            g.FillRectangle(b, 0, h - 20, 11, 9);

            //bottom right
            g.FillRectangle(b, w - 20, h - 11, 20, 11);
            g.FillRectangle(b, w - 11, h - 20, 11, 9);

            b = Brushes.Black;

            //top left arrow
            g.FillPolygon(Brushes.Black, new Point[]
            {
                new Point(5,5),
                new Point(10,5),
                new Point(5,10),
            });

            //top right arrow
            g.FillPolygon(b, new Point[]
            {
                new Point(w-5,5),
                new Point(w-5,10),
                new Point(w-10,5),
            });

            //bottom left arrow
            g.FillPolygon(b, new Point[]
            {
                new Point(5,h-5),
                new Point(10,h-5),
                new Point(5,h-10),
            });

            //bottom right arrow
            g.FillPolygon(b, new Point[] 
            {
                new Point(w-5,h-5),
                new Point(w-5,h-10),
                new Point(w-10,h-5) 
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

                    if (p.X <= 20)
                    {
                        if (p.Y <= 20)
                            m.Result = (IntPtr)HitTest.TopLeft;
                        else if (p.Y >= size.Height - 20)
                            m.Result = (IntPtr)HitTest.BottomLeft;
                        else
                            m.Result = (IntPtr)HitTest.Caption;
                    }
                    else if (p.X >= size.Width - 20)
                    {
                        if (p.Y <= 20)
                            m.Result = (IntPtr)HitTest.TopRight;
                        else if (p.Y >= size.Height - 20)
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
