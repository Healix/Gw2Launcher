using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Threading;
using System.Threading.Tasks;
using System.Runtime.InteropServices;
using Gw2Launcher.Windows.Native;

namespace Gw2Launcher.UI.WindowPositioning
{
    public partial class formWindowSize : Base.BaseForm
    {
        private const int 
            MIN_WIDTH = 200,
            MIN_HEIGHT = 150,
            MOVE_SIZE = 25;

        private const string TEXT_INFO = "(right click for options)";

        protected enum SharedOption
        {
            All,
            IncludeFrame,
            EnforceLimiations,
            SnapToGrid,
            SnapToAccounts,
            SnapToScreen,
            Ratio,
        }

        public enum SnapType : byte
        {
            None,
            EdgeToEdge,
            EdgeToBorder,
            BorderToBorder,
        }

        public enum ScreenRatio
        {
            None,
            Widescreen16_9,
            Box4_3
        }

        private class Snap
        {
            public int left, top, right, bottom;
            public FrameType type;

            public enum FrameType
            {
                Window,
                Container
            }

            public Snap()
            {

            }

            public Snap(Rectangle r)
            {
                left = r.Left;
                top = r.Top;
                right = r.Right;
                bottom = r.Bottom;
            }
        }

        private class TextOverlay : Form
        {
            private string text;
            private TextFormatFlags format;
            private bool resize;

            public TextOverlay(Form parent)
            {
                SetStyle(ControlStyles.UserPaint, true);
                SetStyle(ControlStyles.AllPaintingInWmPaint, true);
                SetStyle(ControlStyles.OptimizedDoubleBuffer, true);

                text = string.Empty;

                this.StartPosition = FormStartPosition.Manual;
                this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
                this.ShowInTaskbar = false;
                this.TransparencyKey = this.BackColor = parent.BackColor;
                this.ForeColor = parent.ForeColor;
                this.Font = parent.Font;
            }

            protected override CreateParams CreateParams
            {
                get
                {
                    CreateParams createParams = base.CreateParams;
                    createParams.ExStyle |= (int)(WindowStyle.WS_EX_COMPOSITED | WindowStyle.WS_EX_TRANSPARENT | WindowStyle.WS_EX_LAYERED);
                    return createParams;
                }
            }

            public TextFormatFlags TextFormatFlags
            {
                get
                {
                    return format;
                }
                set
                {
                    format = value;
                }
            }

            public override string Text
            {
                get
                {
                    return text;
                }
                set
                {
                    text = value;
                    resize = true;
                    this.Invalidate();
                }
            }

            protected override void OnPaint(PaintEventArgs e)
            {
                if (resize)
                {
                    resize = false;
                    var size = TextRenderer.MeasureText(text, this.Font);
                    if (size.Width > this.Width || size.Height > this.Height)
                        this.Size = new Size(size.Width + 1, size.Height + 1);
                }

                TextRenderer.DrawText(e.Graphics, text, this.Font, this.ClientRectangle, this.ForeColor, format);

                base.OnPaint(e);
            }
        }

        private TextOverlay overlaySize, overlayLocation, overlayInfo;

        protected class SharedOptions
        {
            public event EventHandler<SharedOption> SharedOptionChanged;

            public bool
                includeFrame,
                enforceLimitations,
                snapToGrid,
                snapToScreen;

            public SnapType snapToAccounts;

            public ScreenRatio ratio;

            public HashSet<formWindowSize> activeWindows;

            public formTemplates.TemplateDisplayManager templateManager;

            public void InvokeSharedOptionChanged(SharedOption o)
            {
                if (SharedOptionChanged != null)
                    SharedOptionChanged(this, o);
            }
        }

        protected static SharedOptions options;

        private int
            borderSize,
            padding,
            captionHeight,
            gridSize = 10,
            snapSize = 30,
            snapX,
            snapY;

        private bool
            showInfo,
            snappedX,
            snappedY;

        private Point sizingOrigin;
        private RECT sizingBounds;

        private Snap[] snaps;

        private bool showContextMenu;
        protected Settings.IAccount account;
        protected Settings.AccountType type;
        private formTemplates templates;
        private byte moving;

        static formWindowSize()
        {
            //settings used between windows and sessions
            options = new SharedOptions()
            {
                includeFrame = true,
                snapToAccounts = SnapType.EdgeToEdge,
                snapToScreen = false,
                activeWindows = new HashSet<formWindowSize>(),
                enforceLimitations = false, //DX11 doesn't have the DX9 window limiations
            };
        }

        public formWindowSize(bool showContextMenu, Settings.AccountType type, Settings.IAccount account, string name)
            : this(showContextMenu, true, Color.White, type, account, name)
        {
        }

        public formWindowSize(bool showContextMenu, bool showInfo, Color backColor, Settings.AccountType type, Settings.IAccount account, string name)
        {
            InitializeComponents();

            SetStyle(ControlStyles.UserPaint, true);
            SetStyle(ControlStyles.AllPaintingInWmPaint, true);
            SetStyle(ControlStyles.OptimizedDoubleBuffer, true);

            this.showContextMenu = showContextMenu;
            this.account = account;
            this.type = type;

            this.Text = name;
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.Sizable;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.BackColor = backColor;
            this.Cursor = Cursors.SizeAll;
            this.MinimumSize = new System.Drawing.Size(MIN_WIDTH, MIN_HEIGHT);
            this.Opacity = 0;

            if (showContextMenu)
            {
                foreach (var a in Util.Accounts.GetAccounts())
                {
                    if (account == null || a.UID != account.UID)
                    {
                        showOtherAccountsToolStripMenuItem.Enabled = true;
                        break;
                    }
                }
            }

            var b = this.Bounds;
            var c = this.ClientRectangle;

            borderSize = (b.Width - c.Width) / 2;
            captionHeight = b.Height - c.Height - borderSize;
            padding = borderSize + 5;
            snaps = new Snap[0];

            var h = (int)this.Font.GetHeight() + 1;

            overlaySize = new TextOverlay(this)
            {
                Size = new Size(100, h),
                TextFormatFlags = TextFormatFlags.Bottom | TextFormatFlags.Right,
                Text = this.Width + " x " + this.Height,
            };

            overlaySize.SizeChanged += overlay_SizeChanged;

            overlayLocation = new TextOverlay(this)
            {
                Size = new Size(100, h),
                TextFormatFlags = TextFormatFlags.Bottom,
                Text = this.Left + ", " + this.Top,
            };
            overlayLocation.SizeChanged += overlay_SizeChanged;

            if (this.showInfo = showInfo)
            {
                overlayInfo = new TextOverlay(this)
                {
                    Size = new Size(200, h),
                    TextFormatFlags = TextFormatFlags.VerticalCenter | TextFormatFlags.HorizontalCenter,
                    Text = TEXT_INFO
                };
                overlayInfo.SizeChanged += overlay_SizeChanged;
            }

            OnSharedOptionChanged(SharedOption.All);

            options.SharedOptionChanged += formWindowSize_SharedOptionChanged;

            this.SizeChanged += formWindowSize_SizeChanged;
            this.LocationChanged += formWindowSize_LocationChanged;
            this.Disposed += formWindowSize_Disposed;
        }

        protected override void OnInitializeComponents()
        {
            base.OnInitializeComponents();

            InitializeComponent();
        }

        void formWindowSize_Disposed(object sender, EventArgs e)
        {
            options.activeWindows.Remove(this);
            options.SharedOptionChanged -= formWindowSize_SharedOptionChanged;
        }

        void formWindowSize_LocationChanged(object sender, EventArgs e)
        {
            UpdateLocations();
            OnLocationChanged();

            if (Settings.IsRunningWine)
            {
                if (moving == 1)
                {
                    moving = 2;
                    OnBeginMove();
                }
            }
        }

        void formWindowSize_SizeChanged(object sender, EventArgs e)
        {
            UpdateLocations();
            OnSizeChanged();
        }

        void overlay_SizeChanged(object sender, EventArgs e)
        {
            UpdateLocations();
        }

        private void UpdateLocations()
        {
            overlaySize.Location = new Point(this.Right - overlaySize.Width - padding, this.Bottom - overlaySize.Height - padding);
            overlayLocation.Location = new Point(this.Left + padding, this.Bottom - overlaySize.Height - padding);
            if (showInfo)
                overlayInfo.Location = new Point(this.Left + this.Width / 2 - overlayInfo.Width / 2, this.Top + this.ClientSize.Height / 2 - overlayInfo.Height / 2 + captionHeight);
        }

        private void OnLocationChanged()
        {
            Point location;
            if (options.includeFrame)
                location = this.Location;
            else
                location = this.PointToScreen(Point.Empty);
            overlayLocation.Text = location.X + ", " + location.Y;
        }

        private void OnSizeChanged()
        {
            Size size;
            if (options.includeFrame)
                size = this.Size;
            else
                size = this.ClientSize;
            overlaySize.Text = size.Width + " x " + size.Height;
        }

        private void OnBeginMove()
        {
            if (options.templateManager != null)
            {
                options.templateManager.Enabled = true;
            }
        }

        private void OnExitSizeMove()
        {
            if (options.templateManager != null)
            {
                var t = options.templateManager;
                if (t.Enabled && t.IsHovered)
                {
                    if (t.SnapToEdge)
                    {
                        var hb = t.HoveredBounds;
                        var border = (this.Width - this.ClientSize.Width) / 2 - 1;
                        this.Bounds = new Rectangle(hb.Left - border, hb.Top, hb.Width + border * 2, hb.Height + border);
                    }
                    else
                    {
                        this.Bounds = t.HoveredBounds;
                    }
                }
                t.Enabled = false;
            }

            if (options.enforceLimitations)
            {
                var screen = Screen.FromControl(this);
                Rectangle bounds;

                switch (this.type)
                {
                    case Settings.AccountType.GuildWars1:

                        //gw1 cannot go above the top of the screen it's on
                        bounds = screen.Bounds;
                        if (this.Top < bounds.Top)
                        {
                            this.Top = bounds.Top;
                        }

                        break;
                    case Settings.AccountType.GuildWars2:

                        //gw2 cannot be larger than the working area
                        bounds = screen.WorkingArea;
                        if (this.Width > bounds.Width || this.Height > bounds.Height)
                        {
                            this.Size = new Size(this.Width > bounds.Width ? bounds.Width : this.Width, this.Height > bounds.Height ? bounds.Height : this.Height);
                        }

                        break;
                }
            }
        }

        private int Abs(int x)
        {
            if (x < 0)
                return -x;
            return x;
        }

        private bool OnMoving(ref RECT r)
        {
            int w = r.right - r.left,
                h = r.bottom - r.top,
                pe;

            var p = Point.Subtract(Cursor.Position, (Size)sizingOrigin);

            r.left = sizingBounds.left + p.X;
            r.top = sizingBounds.top + p.Y;

            if (options.snapToGrid)
            {
                pe = r.left % gridSize;
                if (pe >= gridSize / 2)
                    r.left += gridSize - pe;
                else
                    r.left -= pe;

                pe = r.top % gridSize;
                if (pe >= gridSize / 2)
                    r.top += gridSize - pe;
                else
                    r.top -= pe;
            }

            r.right = r.left + w;
            r.bottom = r.top + h;

            if (options.snapToAccounts != SnapType.None)
            {
                if (snappedX && Abs(r.left - snapX) > snapSize)
                    snappedX = false;

                if (snappedY && Abs(r.top - snapY) > snapSize)
                    snappedY = false;

                int dX = Int32.MaxValue,
                    dY = Int32.MaxValue,
                    d;

                foreach (var s in snaps)
                {
                    if (r.top < s.bottom + snapSize && r.bottom > s.top - snapSize)
                    {
                        if ((d = Abs(r.right - s.left)) < snapSize)
                        {
                            if (d < dX)
                            {
                                dX = d;
                                snapX = s.left - w;
                                if (options.snapToAccounts == SnapType.EdgeToEdge || s.type == Snap.FrameType.Container)
                                    snapX += borderSize - 1;
                                snappedX = true;
                            }
                        }
                        else if ((d = Abs(r.left - s.right)) < snapSize)
                        {
                            if (d < dX)
                            {
                                dX = d;
                                snapX = s.right;
                                if (options.snapToAccounts == SnapType.EdgeToEdge || s.type == Snap.FrameType.Container)
                                    snapX -= borderSize - 1;
                                snappedX = true;
                            }
                        }
                        else if (s.type == Snap.FrameType.Container || r.top > s.bottom - snapSize || r.bottom < s.top + snapSize)
                        {
                            if ((d = Abs(r.left - s.left)) < snapSize)
                            {
                                if (d < dX)
                                {
                                    dX = d;
                                    snapX = s.left;
                                    if (options.snapToAccounts == SnapType.EdgeToEdge || options.snapToAccounts == SnapType.EdgeToBorder || s.type == Snap.FrameType.Container)
                                        snapX -= borderSize;
                                    snappedX = true;
                                }
                            }
                            else if ((d = Abs(r.right - s.right)) < snapSize)
                            {
                                if (d < dX)
                                {
                                    dX = d;
                                    snapX = s.right - w;
                                    if (options.snapToAccounts == SnapType.EdgeToEdge || options.snapToAccounts == SnapType.EdgeToBorder || s.type == Snap.FrameType.Container)
                                        snapX += borderSize;
                                    snappedX = true;
                                }
                            }
                        }
                    }

                    if (r.left < s.right + snapSize && r.right > s.left - snapSize)
                    {
                        if ((d = Abs(s.top - r.bottom)) < snapSize)
                        {
                            if (d < dY)
                            {
                                dY = d;
                                snapY = s.top - h;
                                if (options.snapToAccounts == SnapType.EdgeToEdge || s.type == Snap.FrameType.Container)
                                    snapY += borderSize;
                                snappedY = true;
                            }
                        }
                        else if ((d = Abs(s.bottom - r.top)) < snapSize)
                        {
                            if (d < dY)
                            {
                                dY = d;
                                snapY = s.bottom;
                                snappedY = true;
                            }
                        }
                        else if (s.type == Snap.FrameType.Container || r.left > s.right - snapSize || r.right < s.left + snapSize)
                        {
                            if ((d = Abs(s.top - r.top)) < snapSize)
                            {
                                if (d < dY)
                                {
                                    dY = d;
                                    snapY = s.top;
                                    snappedY = true;
                                }
                            }
                            else if ((d = Abs(s.bottom - r.bottom)) < snapSize)
                            {
                                if (d < dY)
                                {
                                    dY = d;
                                    snapY = s.bottom - h;
                                    if (options.snapToAccounts == SnapType.EdgeToEdge || s.type == Snap.FrameType.Container)
                                        snapY += borderSize;
                                    snappedY = true;
                                }
                            }
                        }
                    }
                }

                if (snappedX)
                {
                    r.left = snapX;
                    r.right = snapX + w;
                }
                if (snappedY)
                {
                    r.top = snapY;
                    r.bottom = snapY + h;
                }
            }

            return true;
        }

        private bool OnSizing(Sizing sz, ref RECT r)
        {
            var p = Point.Subtract(Cursor.Position, (Size)sizingOrigin);
            int pe;

            switch (sz)
            {
                case Sizing.BottomRight:
                case Sizing.Right:
                case Sizing.TopRight:

                    //right

                    r.right = sizingBounds.right + p.X;

                    if (options.snapToGrid)
                    {
                        pe = (r.right - r.left) % gridSize;
                        if (pe >= gridSize / 2)
                            r.right += gridSize - pe;
                        else
                            r.right -= pe;
                    }

                    if (r.right - r.left < MIN_WIDTH)
                        r.right = r.left + MIN_WIDTH;

                    if (snappedX && Abs(r.right - snapX) > snapSize)
                        snappedX = false;

                    break;
                case Sizing.BottomLeft:
                case Sizing.Left:
                case Sizing.TopLeft:

                    //left

                    r.left = sizingBounds.left + p.X;

                    if (options.snapToGrid)
                    {
                        pe = r.left % gridSize;
                        if (pe >= gridSize / 2)
                            r.left += gridSize - pe;
                        else
                            r.left -= pe;
                    }

                    if (r.right - r.left < MIN_WIDTH)
                        r.left = r.right - MIN_WIDTH;

                    if (snappedX && Abs(r.left - snapX) > snapSize)
                        snappedX = false;

                    break;
            }

            switch (sz)
            {
                case Sizing.Bottom:
                case Sizing.BottomLeft:
                case Sizing.BottomRight:

                    //bottom

                    r.bottom = sizingBounds.bottom + p.Y;

                    if (options.snapToGrid)
                    {
                        pe = (r.bottom - r.top) % gridSize;
                        if (pe >= gridSize / 2)
                            r.bottom += gridSize - pe;
                        else
                            r.bottom -= pe;
                    }

                    if (r.bottom - r.top < MIN_HEIGHT)
                        r.bottom = r.top + MIN_HEIGHT;

                    if (snappedY && Abs(r.bottom - snapY) > snapSize)
                        snappedY = false;

                    break;
                case Sizing.Top:
                case Sizing.TopLeft:
                case Sizing.TopRight:

                    //top

                    r.top = sizingBounds.top + p.Y;

                    if (options.snapToGrid)
                    {
                        pe = r.top % gridSize;
                        if (pe >= gridSize / 2)
                            r.top += gridSize - pe;
                        else
                            r.top -= pe;
                    }

                    if (r.bottom - r.top < MIN_HEIGHT)
                        r.top = r.bottom - MIN_HEIGHT;

                    if (snappedY && Abs(r.top - snapY) > snapSize)
                        snappedY = false;

                    break;
            }

            if (options.snapToAccounts != SnapType.None)
            {
                int dX = Int32.MaxValue,
                    dY = Int32.MaxValue,
                    d;

                foreach (var s in snaps)
                {
                    if (r.top < s.bottom + snapSize && r.bottom > s.top - snapSize)
                    {
                        switch (sz)
                        {
                            case Sizing.BottomRight:
                            case Sizing.Right:
                            case Sizing.TopRight:

                                //right

                                if ((d = Abs(r.right - s.left)) < snapSize)
                                {
                                    if (d < dX)
                                    {
                                        snapX = s.left;
                                        if (options.snapToAccounts == SnapType.EdgeToEdge || s.type == Snap.FrameType.Container)
                                            snapX += borderSize - 1;
                                        snappedX = true;
                                    }
                                }
                                else if (s.type == Snap.FrameType.Container || r.top > s.bottom - snapSize || r.bottom < s.top + snapSize)
                                {
                                    if ((d = Abs(r.right - s.right)) < snapSize)
                                    {
                                        if (d < dX)
                                        {
                                            snapX = s.right;
                                            if (options.snapToAccounts == SnapType.EdgeToEdge || options.snapToAccounts == SnapType.EdgeToBorder || s.type == Snap.FrameType.Container)
                                                snapX += borderSize;
                                            snappedX = true;
                                        }
                                    }
                                }

                                break;
                            case Sizing.BottomLeft:
                            case Sizing.Left:
                            case Sizing.TopLeft:

                                //left

                                if ((d = Abs(r.left - s.right)) < snapSize)
                                {
                                    if (d < dX)
                                    {
                                        snapX = s.right;
                                        if (options.snapToAccounts == SnapType.EdgeToEdge || s.type == Snap.FrameType.Container)
                                            snapX -= borderSize - 1;
                                        snappedX = true;
                                    }
                                }
                                else if (s.type == Snap.FrameType.Container || r.top > s.bottom - snapSize || r.bottom < s.top + snapSize)
                                {
                                    if ((d = Abs(r.left - s.left)) < snapSize)
                                    {
                                        if (d < dX)
                                        {
                                            snapX = s.left;
                                            if (options.snapToAccounts == SnapType.EdgeToEdge || options.snapToAccounts == SnapType.EdgeToBorder || s.type == Snap.FrameType.Container)
                                                snapX -= borderSize;
                                            snappedX = true;
                                        }
                                    }
                                }

                                break;
                        }
                    }

                    if (r.left < s.right + snapSize && r.right > s.left - snapSize)
                    {
                        switch (sz)
                        {
                            case Sizing.Bottom:
                            case Sizing.BottomLeft:
                            case Sizing.BottomRight:

                                //bottom

                                if ((d = Abs(r.bottom - s.top)) < snapSize)
                                {
                                    if (d < dY)
                                    {
                                        snapY = s.top;
                                        if (options.snapToAccounts == SnapType.EdgeToEdge || s.type == Snap.FrameType.Container)
                                            snapY += borderSize;
                                        snappedY = true;
                                    }
                                }
                                else if (s.type == Snap.FrameType.Container || r.left > s.right - snapSize || r.right < s.left + snapSize)
                                {
                                    if ((d = Abs(r.bottom - s.bottom)) < snapSize)
                                    {
                                        if (d < dY)
                                        {
                                            snapY = s.bottom;
                                            if (options.snapToAccounts == SnapType.EdgeToEdge || s.type == Snap.FrameType.Container)
                                                snapY += borderSize;
                                            snappedY = true;
                                        }
                                    }
                                }

                                break;
                            case Sizing.Top:
                            case Sizing.TopLeft:
                            case Sizing.TopRight:

                                //top

                                if ((d = Abs(r.top - s.bottom)) < snapSize)
                                {
                                    if (d < dY)
                                    {
                                        snapY = s.bottom;
                                        snappedY = true;
                                    }
                                }
                                else if (s.type == Snap.FrameType.Container || r.left > s.right - snapSize || r.right < s.left + snapSize)
                                {
                                    if ((d = Abs(r.top - s.top)) < snapSize)
                                    {
                                        if (d < dY)
                                        {
                                            snapY = s.top;
                                            snappedY = true;
                                        }
                                    }
                                }

                                break;
                        }
                    }
                }

                if (snappedX)
                {
                    switch (sz)
                    {
                        case Sizing.BottomRight:
                        case Sizing.Right:
                        case Sizing.TopRight:

                            //right

                            r.right = snapX;

                            break;
                        case Sizing.BottomLeft:
                        case Sizing.Left:
                        case Sizing.TopLeft:

                            //left

                            r.left = snapX;

                            break;
                    }
                }

                if (snappedY)
                {
                    switch (sz)
                    {
                        case Sizing.Bottom:
                        case Sizing.BottomLeft:
                        case Sizing.BottomRight:

                            //bottom

                            r.bottom = snapY;

                            break;
                        case Sizing.Top:
                        case Sizing.TopLeft:
                        case Sizing.TopRight:

                            //top

                            r.top = snapY;

                            break;
                    }
                }
            }

            return true;
        }

        private Snap[] GetSnaps()
        {
            Screen[] screens;

            if (options.snapToScreen)
                screens = Screen.AllScreens;
            else
                screens = new Screen[0];

            var snaps = new Snap[options.activeWindows.Count - 1 + screens.Length];
            int i = 0;

            foreach (var f in options.activeWindows)
            {
                if (f == this)
                    continue;

                if (options.snapToAccounts == SnapType.BorderToBorder)
                {
                    snaps[i++] = new Snap()
                    {
                        left = f.Left,
                        top = f.Top,
                        right = f.Right,
                        bottom = f.Bottom,
                        type = Snap.FrameType.Window,
                    };
                }
                else
                {
                    snaps[i++] = new Snap()
                    {
                        left = f.Left + borderSize,
                        top = f.Top,
                        right = f.Right - borderSize,
                        bottom = options.snapToAccounts == SnapType.EdgeToBorder ? f.Bottom : f.Bottom - borderSize,
                        type = Snap.FrameType.Window,
                    };
                }
            }

            foreach (var screen in screens)
            {
                snaps[i++] = new Snap(screen.Bounds)
                {
                    type = Snap.FrameType.Container,
                };
            }

            return snaps;
        }

        protected override void WndProc(ref Message m)
        {
            switch ((WindowMessages)m.Msg)
            {
                case WindowMessages.WM_NCLBUTTONDBLCLK:

                    break;
                case WindowMessages.WM_NCRBUTTONUP:

                    base.WndProc(ref m);

                    OnRightButtonUp();

                    break;
                case WindowMessages.WM_NCHITTEST:

                    base.WndProc(ref m);

                    switch ((HitTest)m.Result.GetValue())
                    {
                        case HitTest.Client:

                            var p = this.PointToClient(new Point(m.LParam.GetValue32()));
                            var size = this.ClientSize;

                            if (p.Y < MOVE_SIZE)
                            {
                                if (p.X < MOVE_SIZE)
                                {
                                    m.Result = (IntPtr)HitTest.TopLeft;
                                }
                                else if (p.X > size.Width - MOVE_SIZE)
                                {
                                    m.Result = (IntPtr)HitTest.TopRight;
                                }
                                else
                                {
                                    m.Result = (IntPtr)HitTest.Top;
                                }
                            }
                            else if (p.Y > size.Height - MOVE_SIZE)
                            {
                                if (p.X < MOVE_SIZE)
                                {
                                    m.Result = (IntPtr)HitTest.BottomLeft;
                                }
                                else if (p.X > size.Width - MOVE_SIZE)
                                {
                                    m.Result = (IntPtr)HitTest.BottomRight;
                                }
                                else
                                {
                                    m.Result = (IntPtr)HitTest.Bottom;
                                }
                            }
                            else if (p.X < MOVE_SIZE)
                            {
                                m.Result = (IntPtr)HitTest.Left;
                            }
                            else if (p.X > size.Width - MOVE_SIZE)
                            {
                                m.Result = (IntPtr)HitTest.Right;
                            }
                            else if (!Settings.IsRunningWine)
                            {
                                m.Result = (IntPtr)HitTest.Caption;
                            }

                            break;
                    }

                    break;
                case WindowMessages.WM_ENTERSIZEMOVE:

                    base.WndProc(ref m);

                    snaps = GetSnaps();
                    sizingOrigin = Cursor.Position;
                    sizingBounds = new RECT()
                    {
                        left = this.Left,
                        right = this.Right,
                        top = this.Top,
                        bottom = this.Bottom,
                    };
                    snappedX = snappedY = false;
                    moving = 1;

                    break;
                case WindowMessages.WM_SIZING:

                    base.WndProc(ref m);

                    if (options.snapToGrid || options.snapToAccounts != SnapType.None)
                    {
                        var r = (RECT)m.GetLParam(typeof(RECT));
                        if (OnSizing((Sizing)m.WParam.GetValue(), ref r))
                            Marshal.StructureToPtr(r, m.LParam, false);
                    }

                    break;
                case WindowMessages.WM_MOVING:

                    base.WndProc(ref m);

                    if (options.snapToGrid || options.snapToAccounts != SnapType.None)
                    {
                        var r = (RECT)m.GetLParam(typeof(RECT));
                        if (OnMoving(ref r))
                            Marshal.StructureToPtr(r, m.LParam, false);
                    }

                    if (moving == 1)
                    {
                        moving = 2;
                        OnBeginMove();
                    }

                    break;
                case WindowMessages.WM_EXITSIZEMOVE:

                    base.WndProc(ref m);

                    OnExitSizeMove();
                    moving = 0;

                    break;
                default:

                    base.WndProc(ref m);

                    break;
            }
        }

        protected override void OnShown(EventArgs e)
        {
            UpdateLocations();

            overlaySize.Show(this);
            overlayLocation.Show(this);
            if (showInfo)
                overlayInfo.Show(this);

            this.Opacity = 0.9;

            options.activeWindows.Add(this);
            
            base.OnShown(e);
        }

        void OnSharedOptionChanged(SharedOption o)
        {
            switch (o)
            {
                case SharedOption.All:

                    includeFrameSizeInDisplayedValuesToolStripMenuItem.Checked = options.includeFrame;
                    enforceLimitationsToolStripMenuItem.Checked = options.enforceLimitations;
                    gridToolStripMenuItem.Checked = options.snapToGrid;
                    fromEdgeToEdgeToolStripMenuItem.Checked = options.snapToAccounts == SnapType.EdgeToEdge;
                    fromEdgeToBorderToolStripMenuItem.Checked = options.snapToAccounts == SnapType.EdgeToBorder;
                    fromBorderToBorderToolStripMenuItem.Checked = options.snapToAccounts == SnapType.BorderToBorder;
                    otherAccountsToolStripMenuItem.Checked = options.snapToAccounts != SnapType.None;
                    screenToolStripMenuItem.Checked = options.snapToScreen;
                    keep169RatioToolStripMenuItem.Checked = options.ratio == ScreenRatio.Widescreen16_9;
                    keep43RatioToolStripMenuItem.Checked = options.ratio == ScreenRatio.Box4_3;

                    break;
                case SharedOption.IncludeFrame:

                    includeFrameSizeInDisplayedValuesToolStripMenuItem.Checked = options.includeFrame;

                    OnLocationChanged();
                    OnSizeChanged();

                    break;
                case SharedOption.EnforceLimiations:

                    enforceLimitationsToolStripMenuItem.Checked = options.enforceLimitations;

                    OnExitSizeMove();

                    break;
                case SharedOption.SnapToAccounts:

                    fromEdgeToEdgeToolStripMenuItem.Checked = options.snapToAccounts == SnapType.EdgeToEdge;
                    fromEdgeToBorderToolStripMenuItem.Checked = options.snapToAccounts == SnapType.EdgeToBorder;
                    fromBorderToBorderToolStripMenuItem.Checked = options.snapToAccounts == SnapType.BorderToBorder;
                    otherAccountsToolStripMenuItem.Checked = options.snapToAccounts != SnapType.None;

                    break;
                case SharedOption.SnapToGrid:

                    gridToolStripMenuItem.Checked = options.snapToGrid;

                    break;
                case SharedOption.SnapToScreen:

                    screenToolStripMenuItem.Checked = options.snapToScreen;

                    break;
                case SharedOption.Ratio:

                    keep169RatioToolStripMenuItem.Checked = options.ratio == ScreenRatio.Widescreen16_9;
                    keep43RatioToolStripMenuItem.Checked = options.ratio == ScreenRatio.Box4_3;

                    break;
            }
        }

        void formWindowSize_SharedOptionChanged(object sender, formWindowSize.SharedOption e)
        {
            OnSharedOptionChanged(e);
        }

        protected formWindowSize PreviousForm
        {
            get;
            set;
        }

        protected formWindowSize NextForm
        {
            get;
            set;
        }

        public Rectangle Result
        {
            get
            {
                return this.Bounds;
            }
        }

        public void SetWindowLocation(Rectangle rect)
        {
            if (Util.Invoke.IfRequired(this,
                delegate
                {
                    SetWindowLocation(rect);
                }))
                return;

            var min = MOVE_SIZE * 2;
            if (rect.Width < min)
            {
                rect = new Rectangle(this.Bounds.X, rect.Y, min, rect.Height);
            }
            if (rect.Height < min)
            {
                rect = new Rectangle(rect.X, this.Bounds.Y, rect.Width, min);
            }

            if (rect.Width >= min && rect.Height >= min)
            {
                this.Bounds = rect;
                this.Refresh();
            }
        }

        private void OnRightButtonUp()
        {
            if (!this.showContextMenu)
                return;

            saveAllToolStripMenuItem.Enabled = PreviousForm != null;

            IntPtr handle = IntPtr.Zero;

            if (account != null)
            {
                try
                {
                    var p = Client.Launcher.GetProcess(account);
                    if (p != null && !p.HasExited)
                    {
                        handle = Windows.FindWindow.FindMainWindow(p);
                        if (!NativeMethods.IsWindow(handle))
                            handle = IntPtr.Zero;
                    }
                }
                catch
                {
                    handle = IntPtr.Zero;
                }
            }

            matchProcessToolStripMenuItem.Enabled = handle != IntPtr.Zero;
            matchProcessToolStripMenuItem.Tag = handle;
            applyBoundsToProcessToolStripMenuItem.Enabled = handle != IntPtr.Zero;

            contextMenu.Show(Cursor.Position);
        }

        protected override void OnMouseDown(MouseEventArgs e)
        {
            base.OnMouseDown(e);

            if (e.Button == System.Windows.Forms.MouseButtons.Left && Settings.IsRunningWine)
            {
                NativeMethods.SendMessage(this.Handle, 0x112, 0xf012, 0);
            }
        }

        private void formWindowSize_MouseUp(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right && this.showContextMenu)
            {
                OnRightButtonUp();
            }
        }

        private void formWindowSize_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (e.CloseReason == CloseReason.FormOwnerClosing && (this.PreviousForm != null || this.NextForm != null))
            {

            }
            else
            {
                if (this.PreviousForm != null || this.NextForm != null)
                {
                    if (this.PreviousForm != null)
                    {
                        this.PreviousForm.NextForm = this.NextForm;
                    }

                    if (this.NextForm != null)
                    {
                        this.NextForm.PreviousForm = this.PreviousForm;

                        var owner = this.Owner;
                        this.Owner = null;
                        this.NextForm.Owner = owner;
                    }
                }

                if (this.PreviousForm != null && this.NextForm == null)
                {
                    CloseForms();
                }

                if (showInfo)
                    overlayInfo.Dispose();
                overlayLocation.Dispose();
                overlaySize.Dispose();
            }
        }

        private void saveToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.OK;
        }

        private void formWindowSize_KeyDown(object sender, KeyEventArgs e)
        {
            switch (e.KeyCode)
            {
                case Keys.Escape:
                    this.DialogResult = DialogResult.Cancel;
                    break;
                case Keys.Left:
                    this.Location = new Point(this.Location.X - (e.Control ? 10 : 1), this.Location.Y);
                    break;
                case Keys.Right:
                    this.Location = new Point(this.Location.X + (e.Control ? 10 : 1), this.Location.Y);
                    break;
                case Keys.Up:
                    this.Location = new Point(this.Location.X, this.Location.Y - (e.Control ? 10 : 1));
                    break;
                case Keys.Down:
                    this.Location = new Point(this.Location.X, this.Location.Y + (e.Control ? 10 : 1));
                    break;

            }
        }

        private void cancelToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
        }

        private void ShowOtherAccounts(List<Settings.IAccount> accounts)
        {
            Form last = null;

            var existing = new Dictionary<ushort, Rectangle>(options.activeWindows.Count);

            foreach (var w in options.activeWindows)
            {
                if (w.account != null)
                    existing[w.account.UID] = w.Bounds;
            }

            if (existing.Count > 1)
                CloseForms();

            foreach (var account in accounts)
            {
                if (account == this.account)
                    continue;

                var f = new formWindowSize(false, false, Color.LightGray, account.Type, account, account.Name);

                var owner = this.Owner;
                this.Owner = f;

                f.PreviousForm = this.PreviousForm;
                if (f.PreviousForm != null)
                {
                    f.PreviousForm.NextForm = f;
                    f.Owner = owner;// f.PreviousForm;
                }
                else
                    f.Owner = owner;

                f.NextForm = this;
                this.PreviousForm = f;

                f.FormClosed += f_FormClosed;

                Rectangle b;
                if (existing.TryGetValue(account.UID, out b))
                    f.Bounds = b;
                else if (!account.WindowBounds.IsEmpty)
                    f.Bounds = account.WindowBounds;
                else
                    f.StartPosition = FormStartPosition.WindowsDefaultBounds;
                f.Show();

                last = f;
            }

            if (last != null)
            {
                last.Shown += delegate
                {
                    this.BringToFront();
                    this.Focus();
                };
            }
        }

        private IEnumerable<KeyValuePair<Settings.IAccount, bool>> GetShowOtherAccountsEnumerable()
        {
            foreach (var a in Util.Accounts.GetAccounts())
            {
                if (a.UID == account.UID)
                    continue;

                var b = a.Type == account.Type && a.Windowed;

                yield return new KeyValuePair<Settings.IAccount, bool>(a, b);
            }
        }

        private void showOtherAccountsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var c = (ToolStripMenuItem)sender;
            var b = !c.Checked;

            if (b)
            {
                List<Settings.IAccount> selected;

                using (var f = new formAccountSelect("Select which accounts to show", GetShowOtherAccountsEnumerable(), true))
                {
                    if (f.ShowDialog(this) == System.Windows.Forms.DialogResult.OK)
                    {
                        selected = f.Selected;
                    }
                    else
                    {
                        return;
                    }
                }

                ShowOtherAccounts(selected);
            }
            else
            {
                CloseForms();
            }

            c.Checked = b;
        }

        void f_FormClosed(object sender, FormClosedEventArgs e)
        {
            var f = (formWindowSize)sender;

            if (this.PreviousForm == null)
            {
                showOtherAccountsToolStripMenuItem.Checked = false;
            }
        }

        private void CloseForms()
        {
            var f = this;
            Form owner = this.Owner;

            this.Owner = null;

            do
            {
                var previous = f.PreviousForm;

                f.PreviousForm = null;
                f.NextForm = null;

                if (f != this)
                {
                    owner = f.Owner;
                    f.Owner = null;
                    f.Dispose();
                }

                f = previous;
            }
            while (f != null);

            this.Owner = owner;
        }

        private void saveAllToolStripMenuItem_Click(object sender, EventArgs e)
        {
            formWindowSize f = this.PreviousForm;
            while (f != null)
            {
                if (f.account != null && f.account.WindowBounds != f.Bounds)
                {
                    f.account.WindowBounds = f.Bounds;
                }
                f = f.PreviousForm;
            }

            this.DialogResult = DialogResult.OK;
        }

        private void matchProcessToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                var p = Windows.WindowSize.GetWindowPlacement((IntPtr)matchProcessToolStripMenuItem.Tag);
                this.Bounds = Util.ScreenUtil.FromDesktopBounds(p.rcNormalPosition.ToRectangle());
            }
            catch { }
        }

        private void applyBoundsToProcessToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                if (!Client.Launcher.ApplyWindowedBounds(account, this.Bounds))
                    Windows.WindowSize.SetWindowPlacement((IntPtr)matchProcessToolStripMenuItem.Tag, Util.ScreenUtil.ToDesktopBounds(this.Bounds), ShowWindowCommands.ShowNormal);
            }
            catch { }
        }

        protected override void OnPaintBackground(PaintEventArgs e)
        {
            base.OnPaintBackground(e);

            var g = e.Graphics;

            g.DrawLine(SystemPens.ActiveBorder, 0, 0, this.ClientSize.Width, 0);
        }

        private void includeFrameSizeInDisplayedValuesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            options.includeFrame = !((ToolStripMenuItem)sender).Checked;
            options.InvokeSharedOptionChanged(SharedOption.IncludeFrame);
        }

        private void gridToolStripMenuItem_Click(object sender, EventArgs e)
        {
            options.snapToGrid = !((ToolStripMenuItem)sender).Checked;
            options.InvokeSharedOptionChanged(SharedOption.SnapToGrid);
        }

        private void fromEdgeToEdgeToolStripMenuItem_Click(object sender, EventArgs e)
        {
            options.snapToAccounts = !((ToolStripMenuItem)sender).Checked ? SnapType.EdgeToEdge : SnapType.None;
            options.InvokeSharedOptionChanged(SharedOption.SnapToAccounts);
        }

        private void fromEdgeToBorderToolStripMenuItem_Click(object sender, EventArgs e)
        {
            options.snapToAccounts = !((ToolStripMenuItem)sender).Checked ? SnapType.EdgeToBorder : SnapType.None;
            options.InvokeSharedOptionChanged(SharedOption.SnapToAccounts);
        }

        private void fromBorderToBorderToolStripMenuItem_Click(object sender, EventArgs e)
        {
            options.snapToAccounts = !((ToolStripMenuItem)sender).Checked ? SnapType.BorderToBorder : SnapType.None;
            options.InvokeSharedOptionChanged(SharedOption.SnapToAccounts);
        }

        private void otherAccountsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            options.snapToAccounts = !((ToolStripMenuItem)sender).Checked ? SnapType.EdgeToBorder : SnapType.None;
            options.InvokeSharedOptionChanged(SharedOption.SnapToAccounts);
        }

        private void screenToolStripMenuItem_Click(object sender, EventArgs e)
        {
            options.snapToScreen = !((ToolStripMenuItem)sender).Checked;
            options.InvokeSharedOptionChanged(SharedOption.SnapToScreen);
        }

        private void keep43RatioToolStripMenuItem_Click(object sender, EventArgs e)
        {
            options.ratio = !((ToolStripMenuItem)sender).Checked ? ScreenRatio.Box4_3 : ScreenRatio.None;
            options.InvokeSharedOptionChanged(SharedOption.Ratio);
        }

        private void keep169RatioToolStripMenuItem_Click(object sender, EventArgs e)
        {
            options.ratio = !((ToolStripMenuItem)sender).Checked ? ScreenRatio.Widescreen16_9 : ScreenRatio.None;
            options.InvokeSharedOptionChanged(SharedOption.Ratio);
        }

        private void enforceLimitationsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            options.enforceLimitations = !((ToolStripMenuItem)sender).Checked;
            options.InvokeSharedOptionChanged(SharedOption.EnforceLimiations);
        }

        private void AutoArrange(Rectangle screen, ICollection<formWindowSize> windows)
        {
            int count = windows.Count,
                cols = count,
                rows = 0,
                w = 0,
                h,
                lw = 0,
                lh = 0,
                a,
                la = 0;

            float
                ra,
                lra = float.MaxValue,
                sra;


            switch (options.ratio)
            {
                case ScreenRatio.Box4_3:
                    sra = 4f / 3f;
                    break;
                case ScreenRatio.Widescreen16_9:
                    sra = 16f / 9f;
                    break;
                default:
                    sra = (float)screen.Width / screen.Height;
                    break;
            }

            Func<int, int, int> calcHeight = new Func<int, int, int>(
                delegate(int width, int defaultHeight)
                {
                    int _h;
                    switch (options.ratio)
                    {
                        case ScreenRatio.Box4_3:
                            _h = (width - 1) * 3 / 4 + captionHeight + 1;
                            break;
                        case ScreenRatio.Widescreen16_9:
                            _h = (width - 1) * 9 / 16 + captionHeight + 1;
                            break;
                        default:
                            _h = defaultHeight;
                            break;
                    }
                    if (_h < MIN_HEIGHT)
                        _h = MIN_HEIGHT;
                    return _h;
                });

            while (cols > 1)
            {
                w = screen.Width / cols;
                if (w < MIN_WIDTH)
                {
                    cols = (screen.Width + 1) / MIN_WIDTH;
                    w = screen.Width / cols;
                }

                rows = (count - 1) / cols + 1;
                h = calcHeight(w, screen.Height / rows);

                if (h * rows > screen.Height)
                {
                    //not enough space

                    if (lw == 0)
                    {
                        lw = w;
                        lh = h;
                    }

                    break;
                }
                else
                {
                    a = w * h;

                    ra = sra - (float)w / h;
                    if (ra < 0)
                        ra = -ra;

                    var d = lra - ra;
                    if (d < 0)
                        d = -d;

                    if (ra < lra && (d > 0.25f || a > la) || d < 0.25f && a > la)
                    {
                        lw = w;
                        lh = h;

                        lra = ra;
                        la = a;
                    }

                    cols--;
                }
            }

            if (lw == 0)
            {
                lw = screen.Width;
                lh = calcHeight(lw, screen.Height);
                if (lh > screen.Height)
                {
                    lh = screen.Height;

                    switch (options.ratio)
                    {
                        case ScreenRatio.Box4_3:
                            lw = (lh - borderSize - captionHeight) * 4 / 3 + 1;
                            break;
                        case ScreenRatio.Widescreen16_9:
                            lw = (lh - borderSize - captionHeight) * 16 / 9 + 1;
                            break;
                    }
                }
            }

            int x = screen.Left - borderSize,
                y = screen.Top;
            foreach (var f in windows)
            {
                f.Location = new Point(x, y);
                f.Size = new Size(lw - 1 + borderSize * 2, lh + borderSize - 1);

                x += lw + 1;

                if (x + lw - 1 > screen.Right)
                {
                    x = screen.Left - borderSize;
                    y += lh;
                }
            }
        }

        private void onThisScreenToolStripMenuItem_Click(object sender, EventArgs e)
        {
        }

        private void allToThisScreenToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var screen = Screen.FromControl(this).Bounds;

            if (useTemplateToolStripMenuItem.Checked && AutoArrageByTemplate(screen, options.activeWindows))
                return;

            AutoArrange(screen, options.activeWindows);
        }

        private void onlyOnThisScreenToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var screen = Screen.FromControl(this).Bounds;
            var windows = new List<formWindowSize>();

            foreach (var f in options.activeWindows)
            {
                if (screen.IntersectsWith(f.DesktopBounds))
                    windows.Add(f);
            }

            if (useTemplateToolStripMenuItem.Checked && AutoArrageByTemplate(screen, windows))
                return;

            AutoArrange(screen, windows);
        }

        private bool AutoArrageByTemplate(Rectangle screen, ICollection<formWindowSize> windows)
        {
            var t = options.templateManager;

            if (t != null)
            {
                var rects = t.GetArea(screen);
                if (rects.Count > 0)
                {
                    var i = 0;

                    rects.Sort(new Comparison<Rectangle>(
                        delegate(Rectangle a, Rectangle b)
                        {
                            var c = a.X.CompareTo(b.X);

                            if (c == 0 
                                && (c = a.Y.CompareTo(b.Y)) == 0 
                                && (c = a.Width.CompareTo(b.Width)) == 0 
                                && (c = a.Height.CompareTo(b.Height)) == 0)
                            {
                                return 0;
                            }

                            return c;
                        }));

                    foreach (var w in windows)
                    {
                        if (t.SnapToEdge)
                        {
                            var r = rects[i++];
                            var border = (this.Width - this.ClientSize.Width) / 2 - 1;
                            w.Bounds = new Rectangle(r.Left - border, r.Top, r.Width + border * 2, r.Height + border);
                        }
                        else
                        {
                            w.Bounds = rects[i++];
                        }

                        if (i == rects.Count)
                            break;
                    }

                    return true;
                }
            }

            return false;
        }

        void templates_TemplateDisplayManagerChanged(object sender, formTemplates.TemplateDisplayManager e)
        {
            options.templateManager = e;

            if (e != null)
            {
                e.TopLevelChanged += templates_TopLevelChanged;
            }
        }

        void templates_TopLevelChanged(object sender, Form e)
        {
            var f = GetLowest();
            if (e != null)
                f.Owner = null;
            f.Owner = e;
            useTemplateToolStripMenuItem.Enabled = e != null;
        }

        private void templatesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (templates == null || templates.IsDisposed)
            {
                var screen = Screen.FromControl(this).WorkingArea;

                templates = new formTemplates(this);
                templates.TemplateDisplayManagerChanged += templates_TemplateDisplayManagerChanged;
                templates.Location = new Point(screen.Right - templates.Width, screen.Bottom - templates.Height);
                templates.FormClosed += templates_FormClosed;
                templates.Show(this);
            }
            else
            {
                if (!templates.Visible)
                    templates.Show(this);
                else
                    templates.BringToFront();
            }
        }

        void templates_FormClosed(object sender, FormClosedEventArgs e)
        {
            options.templateManager = null;
            templates.Dispose();
            templates = null;
        }

        public ICollection<formWindowSize> GetWindows()
        {
            return options.activeWindows;
        }

        public formWindowSize GetLowest()
        {
            var f = this;
            while (f.PreviousForm != null)
            {
                f = f.PreviousForm;
            }
            return f;
        }

        private void useTemplateToolStripMenuItem_Click(object sender, EventArgs e)
        {
            useTemplateToolStripMenuItem.Checked = !useTemplateToolStripMenuItem.Checked;
        }

        private void makeFullScreenToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var screen = Screen.FromControl(this).Bounds;
            var border = (this.Width - this.ClientSize.Width) / 2;
            var caption = this.Height - this.ClientSize.Height - border;

            this.Bounds = new Rectangle(screen.Left - border, screen.Top - caption, screen.Width + border * 2, screen.Height + caption + border);
        }
    }
}
