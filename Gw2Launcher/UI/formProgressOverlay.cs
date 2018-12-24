using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Drawing;
using System.Runtime.InteropServices;
using Gw2Launcher.UI.Controls;
using Gw2Launcher.Windows.Native;

namespace Gw2Launcher.UI
{
    class formProgressOverlay : Form
    {
        private FlatProgressBar progress;
        private formProgressOverlay foreground;

        public formProgressOverlay()
        {
            SetStyle(ControlStyles.UserPaint, true);
            SetStyle(ControlStyles.AllPaintingInWmPaint, true);
            SetStyle(ControlStyles.Opaque, true);

            this.Opacity = 0.75;
            this.BackColor = Color.Black;
            this.ShowInTaskbar = false;
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.StartPosition = FormStartPosition.Manual;

            this.Paint += background_Paint;

            foreground = new formProgressOverlay(this);
            foreground.Owner = this;

            progress = foreground.progress;

            this.MinimumSizeChanged += background_MinimumSizeChanged;
            this.MaximumSizeChanged += background_MaximumSizeChanged;

            this.Size = new Size(200, 7);
        }

        public formProgressOverlay(formProgressOverlay background)
        {
            SetStyle(ControlStyles.UserPaint, true);
            SetStyle(ControlStyles.AllPaintingInWmPaint, true);

            this.Opacity = 0.95;
            this.TransparencyKey = this.BackColor = Color.Magenta;
            this.ShowInTaskbar = false;
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.StartPosition = FormStartPosition.Manual;

            progress = new FlatProgressBar()
            {
                Anchor = AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Top,
                Bounds = this.ClientRectangle,
                ForeColor = Color.LimeGreen,
                Animated = true,
            };

            this.Controls.Add(progress);
        }

        protected override CreateParams CreateParams
        {
            get
            {
                var cp = base.CreateParams;
                cp.ExStyle |= (int)(WindowStyle.WS_EX_LAYERED | WindowStyle.WS_EX_TRANSPARENT | WindowStyle.WS_EX_TOOLWINDOW);
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

        void background_Paint(object sender, PaintEventArgs e)
        {
            e.Graphics.Clear(this.BackColor);
        }

        void background_MaximumSizeChanged(object sender, EventArgs e)
        {
            foreground.MaximumSize = this.MaximumSize;
        }

        void background_MinimumSizeChanged(object sender, EventArgs e)
        {
            foreground.MinimumSize = this.MinimumSize;
        }

        public FlatProgressBar Progress
        {
            get
            {
                return progress;
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (foreground == null && progress != null)
                    progress.Dispose();
            }
            base.Dispose(disposing);
        }

        protected override void SetBoundsCore(int x, int y, int width, int height, BoundsSpecified specified)
        {
            if (foreground == null)
                return;

            var d = NativeMethods.BeginDeferWindowPos(2);
            if (d != IntPtr.Zero)
            {
                var flags = SetWindowPosFlags.SWP_NOACTIVATE | SetWindowPosFlags.SWP_NOREDRAW;

                if (!specified.HasFlag(BoundsSpecified.Size))
                    flags |= SetWindowPosFlags.SWP_NOSIZE;
                if (!specified.HasFlag(BoundsSpecified.Location))
                    flags |= SetWindowPosFlags.SWP_NOMOVE;

                d = NativeMethods.DeferWindowPos(d, this.Handle, (IntPtr)WindowZOrder.HWND_TOPMOST, x, y, width, height, (uint)(flags));
                if (d != IntPtr.Zero)
                {
                    d = NativeMethods.DeferWindowPos(d, foreground.Handle, (IntPtr)WindowZOrder.HWND_TOPMOST, x, y, width, height, (uint)(flags | SetWindowPosFlags.SWP_NOZORDER | SetWindowPosFlags.SWP_NOOWNERZORDER));
                    if (d != IntPtr.Zero)
                        NativeMethods.EndDeferWindowPos(d);
                }
            }
        }

        protected override void WndProc(ref Message m)
        {
            if (foreground != null)
            {
                switch ((WindowMessages)m.Msg)
                {
                    case WindowMessages.WM_SHOW:

                        var d = NativeMethods.BeginDeferWindowPos(2);
                        if (d != IntPtr.Zero)
                        {
                            var flags = SetWindowPosFlags.SWP_NOACTIVATE | SetWindowPosFlags.SWP_NOREDRAW | SetWindowPosFlags.SWP_NOMOVE | SetWindowPosFlags.SWP_NOSIZE | SetWindowPosFlags.SWP_NOOWNERZORDER;

                            d = NativeMethods.DeferWindowPos(d, this.Handle, (IntPtr)WindowZOrder.HWND_TOPMOST, 0, 0, 0, 0, (uint)(flags));
                            if (d != IntPtr.Zero)
                            {
                                d = NativeMethods.DeferWindowPos(d, foreground.Handle, (IntPtr)WindowZOrder.HWND_TOPMOST, 0, 0, 0, 0, (uint)(flags));
                                if (d != IntPtr.Zero)
                                    NativeMethods.EndDeferWindowPos(d);
                            }
                        }

                        foreground.Show();

                        break;
                    //case WindowMessages.WM_WINDOWPOSCHANGING:

                    //    var pos = (WINDOWPOS)Marshal.PtrToStructure(m.LParam, typeof(WINDOWPOS));

                    //    var noresize = SetWindowPosFlags.SWP_NOMOVE | SetWindowPosFlags.SWP_NOSIZE;
                    //    if ((pos.flags & noresize) != noresize)
                    //    {
                    //        if (pos.hwndInsertAfter != this.Handle)
                    //        {
                    //            var d = NativeMethods.BeginDeferWindowPos(2);
                    //            if (d != IntPtr.Zero)
                    //            {
                    //                var flags = pos.flags | SetWindowPosFlags.SWP_NOZORDER | SetWindowPosFlags.SWP_NOREDRAW;

                    //                d = NativeMethods.DeferWindowPos(d, foreground.Handle, this.Handle, pos.x, pos.y, pos.cx, pos.cy, (uint)(flags));
                    //                if (d != IntPtr.Zero)
                    //                {
                    //                    d = NativeMethods.DeferWindowPos(d, this.Handle, this.Handle, pos.x, pos.y, pos.cx, pos.cy, (uint)flags);
                    //                    if (d != IntPtr.Zero)
                    //                        NativeMethods.EndDeferWindowPos(d);
                    //                }
                    //            }

                    //            pos.flags |= SetWindowPosFlags.SWP_NOMOVE | SetWindowPosFlags.SWP_NOSIZE;
                    //            Marshal.StructureToPtr(pos, m.LParam, false);

                    //            return;
                    //        }
                    //    }

                    //    break;
                }
            }

            base.WndProc(ref m);
        }
    }
}
