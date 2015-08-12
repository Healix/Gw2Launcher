using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Runtime.InteropServices;

namespace Gw2Launcher.UI
{
    public partial class formWindowSizeOverlay : Form
    {
        [Flags()]
        public enum SetWindowPosFlags
        {
            SWP_NOSIZE = 0x1,
            SWP_NOMOVE = 0x2,
            SWP_NOZORDER = 0x4,
            SWP_NOREDRAW = 0x8,
            SWP_NOACTIVATE = 0x10,
            SWP_FRAMECHANGED = 0x20,
            SWP_DRAWFRAME = SWP_FRAMECHANGED,
            SWP_SHOWWINDOW = 0x40,
            SWP_HIDEWINDOW = 0x80,
            SWP_NOCOPYBITS = 0x100,
            SWP_NOOWNERZORDER = 0x200,
            SWP_NOREPOSITION = SWP_NOOWNERZORDER,
            SWP_NOSENDCHANGING = 0x400,
            SWP_DEFERERASE = 0x2000,
            SWP_ASYNCWINDOWPOS = 0x4000,
        }

        public enum WindowZOrder
        {
            HWND_TOP = 0,
            HWND_BOTTOM = 1,
            HWND_TOPMOST = -1,
            HWND_NOTOPMOST = -2,
        }

        const int GWL_EXSTYLE = -20;
        const int WS_EX_LAYERED = 0x00080000;
        const int WS_EX_TRANSPARENT = 0x00000020;

        [DllImport("user32.dll", SetLastError = true)]
        static extern int GetWindowLong(IntPtr hWnd, int nIndex);

        [DllImport("user32.dll")]
        static extern int SetWindowLong(IntPtr hWnd, int nIndex, int dwNewLong);

        [DllImport("user32.dll", SetLastError = true)]
        static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int X, int Y, int cx, int cy, SetWindowPosFlags uFlags);

        private const string TEXT_INFO = "(right click to continue)";

        private formWindowSize parent;
        private Pen pen;
        private Brush brush;

        private string _location, _size;
        private Size sizeLocation, sizeSize, sizeInfo;

        public formWindowSizeOverlay(formWindowSize parent)
        {
            InitializeComponent();

            SetStyle(ControlStyles.UserPaint, true);
            SetStyle(ControlStyles.AllPaintingInWmPaint, true);
            SetStyle(ControlStyles.OptimizedDoubleBuffer, true);

            this.parent = parent;

            pen = new Pen(brush = new SolidBrush(parent.ForeColor));

            sizeInfo = TextRenderer.MeasureText(TEXT_INFO, this.Font);

            this.ForeColor = parent.ForeColor;
            this.BackColor = parent.BackColor;
            this.TransparencyKey = parent.BackColor;

            int initialStyle = GetWindowLong(this.Handle, GWL_EXSTYLE);
            SetWindowLong(this.Handle, GWL_EXSTYLE, initialStyle | WS_EX_LAYERED | WS_EX_TRANSPARENT);
            SetWindowPos(this.Handle, (IntPtr)WindowZOrder.HWND_TOPMOST, 0, 0, 0, 0, SetWindowPosFlags.SWP_FRAMECHANGED | SetWindowPosFlags.SWP_NOSIZE | SetWindowPosFlags.SWP_NOMOVE);

            parent.LocationChanged += parent_LocationChanged;
            parent.SizeChanged += parent_SizeChanged;
        }

        void parent_SizeChanged(object sender, EventArgs e)
        {
            this.Bounds = parent.Bounds;
            _size = null;
            this.Invalidate();
        }

        void parent_LocationChanged(object sender, EventArgs e)
        {
            this.Bounds = parent.Bounds;
            _location = null;
            this.Invalidate();
        }

        protected override void OnPaintBackground(PaintEventArgs e)
        {
            base.OnPaintBackground(e);

            var g = e.Graphics;

            int w = this.Width;
            int h = this.Height;

            g.DrawRectangle(new Pen(new SolidBrush(Color.Black)), 0, 0, w - 1, h - 1);

            if (_size == null)
            {
                _size = w + " x " + h;
                sizeSize = TextRenderer.MeasureText(_size, this.Font);
            }

            if (_location == null)
            {
                _location = this.Location.X + ", " + this.Location.Y;
                sizeLocation = TextRenderer.MeasureText(_location, this.Font);
            }

            var rSize = new Rectangle(w - sizeSize.Width - 5, h - sizeSize.Height - 5, sizeSize.Width, sizeSize.Height);
            var rLocation = new Rectangle(5, h - sizeLocation.Height - 5, sizeLocation.Width, sizeLocation.Height);
            var rInfo = new Rectangle(w / 2 - sizeInfo.Width / 2, h / 2 - sizeInfo.Height / 2, sizeInfo.Width, sizeInfo.Height);

            if (rSize.Y > rInfo.Bottom)
            {
                if (rSize.X >= 0)
                    TextRenderer.DrawText(g, _size, this.Font, rSize.Location, Color.Black);
                if (rSize.X > rLocation.Right)
                    TextRenderer.DrawText(g, _location, this.Font, rLocation.Location, Color.Black);
            }

            if (rInfo.Bottom < h && rInfo.Right < w)
                TextRenderer.DrawText(g, TEXT_INFO, this.Font, rInfo.Location, Color.Black);
        }

        private void formWindowSizeOverlay_Load(object sender, EventArgs e)
        {

        }
    }
}
