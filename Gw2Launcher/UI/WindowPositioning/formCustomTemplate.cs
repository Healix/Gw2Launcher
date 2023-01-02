using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Gw2Launcher.Windows.Native;

namespace Gw2Launcher.UI.WindowPositioning
{
    public partial class formCustomTemplate : Base.BaseForm
    {
        private const int SIZING_SIZE = 10;
        private const int MAX_WINDOWS = 255;

        public class TemplateResult
        {
            public TemplateResult(BoundsType type, ScreenWindows[] screens, byte[] keys, byte[] order, KeyValuePair<byte,Settings.WindowTemplate.Assigned>[] assigned)
            {
                this.BoundsType = type;
                this.Screens = screens;
                this.Keys = keys;
                this.Order = order;
                this.Assigned = assigned;
            }

            public BoundsType BoundsType
            {
                get;
                private set;
            }

            public ScreenWindows[] Screens
            {
                get;
                private set;
            }

            public KeyValuePair<byte, Settings.WindowTemplate.Assigned>[] Assigned
            {
                get;
                private set;
            }

            public byte[] Order
            {
                get;
                private set;
            }

            public byte[] Keys
            {
                get;
                private set;
            }
        }

        public class ScreenWindows
        {
            public ScreenWindows(Rectangle screen, Rectangle[] windows)
            {
                this.Screen = screen;
                this.Windows = windows;
            }

            public Rectangle Screen
            {
                get;
                private set;
            }

            public Rectangle[] Windows
            {
                get;
                private set;
            }
        }
        
        private class WindowBlock : Form
        {
            public event EventHandler<int> IndexChanged;

            private BufferedGraphics background;
            private bool redraw;
            private Controls.IntegerTextBox textIndex;

            public WindowBlock()
            {
                SetStyle(ControlStyles.AllPaintingInWmPaint, true);
                SetStyle(ControlStyles.UserPaint, true);

                this.SourceKey = -1;
                this.SourceIndex = -1;
                this.TransparencyKey = this.BackColor = Color.Black;
                this.ForeColor = Color.FromArgb(20, 20, 20);
                this.StartPosition = FormStartPosition.Manual;
                this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
                this.ShowInTaskbar = false;
                this.TopMost = true;

                textIndex = new UI.Controls.IntegerTextBox()
                {
                    Anchor = AnchorStyles.Top | AnchorStyles.Right,
                    TextAlign = HorizontalAlignment.Right,
                    Visible = false,
                    Minimum = 0,
                    Maximum = MAX_WINDOWS,
                };
                textIndex.KeyDown += textIndex_KeyDown;
                textIndex.LostFocus += textIndex_LostFocus;
                this.Controls.Add(textIndex);

                redraw = true;
                Opacity = 0.85;

                this.Disposed += WindowBlock_Disposed;
                this.MouseMove += WindowBlock_MouseMove;
                this.MouseDown += WindowBlock_MouseDown;
            }

            public int[] WindowPadding
            {
                get;
                set;
            }

            public Rectangle SourceBounds
            {
                get;
                set;
            }

            public int SourceIndex
            {
                get;
                set;
            }

            private int _Index;
            public int Index
            {
                get
                {
                    return _Index;
                }
                set
                {
                    if (_Index != value)
                    {
                        _Index = value;
                        OnRedrawRequired();
                    }
                }
            }

            public int SourceKey
            {
                get;
                set;
            }

            public bool IsBoundsModified
            {
                get
                {
                    return this.Bounds != SourceBounds;
                }
            }

            void textIndex_LostFocus(object sender, EventArgs e)
            {
                OnIndexChanged();
                IndexTextVisible = false;
            }

            void textIndex_KeyDown(object sender, KeyEventArgs e)
            {
                if (e.KeyCode == Keys.Enter)
                {
                    e.SuppressKeyPress = true;
                    e.Handled = true;

                    OnIndexChanged();

                    if (textIndex.Value != this.Index + 1)
                    {
                        textIndex.Value = this.Index + 1;
                        textIndex.SelectAll();
                    }
                }
            }

            private void OnIndexChanged()
            {
                var i = textIndex.Value - 1;
                if (i < 0)
                    i = 0;

                if (i != this.Index)
                {
                    if (IndexChanged != null)
                        IndexChanged(this, i);
                }
            }

            void WindowBlock_MouseCaptureChanged(object sender, EventArgs e)
            {
                this.MouseMove += WindowBlock_MouseMove;
                this.MouseCaptureChanged -= WindowBlock_MouseCaptureChanged;
            }

            void WindowBlock_MouseDown(object sender, MouseEventArgs e)
            {
                this.MouseCaptureChanged += WindowBlock_MouseCaptureChanged;
                this.MouseMove -= WindowBlock_MouseMove;
            }

            void WindowBlock_MouseMove(object sender, MouseEventArgs e)
            {
                var a = AnchorStyles.None;

                if (e.X < SIZING_SIZE)
                {
                    a = AnchorStyles.Left;
                }
                else if (e.X > this.Width - SIZING_SIZE)
                {
                    a = AnchorStyles.Right;
                }

                if (e.Y < SIZING_SIZE)
                {
                    a |= AnchorStyles.Top;
                }
                else if (e.Y > this.Height - SIZING_SIZE)
                {
                    a |= AnchorStyles.Bottom;
                }

                switch (a)
                {
                    case AnchorStyles.None:
                        this.Cursor = Cursors.Default;
                        break;
                    case AnchorStyles.Left:
                    case AnchorStyles.Right:
                        this.Cursor = Cursors.SizeWE;
                        break;
                    case AnchorStyles.Top:
                    case AnchorStyles.Bottom:
                        this.Cursor = Cursors.SizeNS;
                        break;
                    case AnchorStyles.Left | AnchorStyles.Top:
                    case AnchorStyles.Right | AnchorStyles.Bottom:
                        this.Cursor = Cursors.SizeNWSE;
                        break;
                    case AnchorStyles.Left | AnchorStyles.Bottom:
                    case AnchorStyles.Right | AnchorStyles.Top:
                        this.Cursor = Cursors.SizeNESW;
                        break;
                }
            }

            void WindowBlock_Disposed(object sender, EventArgs e)
            {
                if (background != null)
                    background.Dispose();
            }

            protected override CreateParams CreateParams
            {
                get
                {
                    var cp = base.CreateParams;
                    cp.ExStyle |= (int)(WindowStyle.WS_EX_COMPOSITED | WindowStyle.WS_EX_LAYERED);
                    return cp;
                }
            }

            protected override void OnShown(EventArgs e)
            {
                base.OnShown(e);

                using (var g = this.CreateGraphics())
                {
                    var scale = g.DpiX / 96f;
                    var o = (int)(scale * 5 + 0.5f);
                    var w = (int)(scale * 30);

                    textIndex.Bounds = new Rectangle(this.Width - w - o, o, w, textIndex.Height);
                }
            }

            protected override void OnMouseDown(MouseEventArgs e)
            {
                base.OnMouseDown(e);

                if (textIndex.Bounds.Contains(e.Location))
                {
                    textIndex.Value = this.Index + 1;
                    IndexTextVisible = true;
                }
                else if (!this.Focused)
                    this.Focus();
            }

            protected override void OnMouseEnter(EventArgs e)
            {
                Hovered = true;
                base.OnMouseEnter(e);
            }

            protected override void OnMouseLeave(EventArgs e)
            {
                Hovered = false;
                base.OnMouseLeave(e);
            }

            protected override void OnActivated(EventArgs e)
            {
                Selected = true;
                base.OnActivated(e);
            }

            protected override void OnDeactivate(EventArgs e)
            {
                Selected = false;
                base.OnDeactivate(e);
            }

            protected override void OnLocationChanged(EventArgs e)
            {
                base.OnLocationChanged(e);

                OnRedrawRequired();
            }

            protected override void OnSizeChanged(EventArgs e)
            {
                base.OnSizeChanged(e);

                if (background != null)
                {
                    background.Dispose();
                    background = null;
                }
                OnRedrawRequired();
            }

            protected override void OnTextChanged(EventArgs e)
            {
                base.OnTextChanged(e);
                OnRedrawRequired();
            }

            private void OnRedrawRequired()
            {
                if (redraw)
                    return;
                redraw = true;
                this.Invalidate();
            }

            private bool _Hovered;
            public bool Hovered
            {
                get
                {
                    return _Hovered;
                }
                set
                {
                    if (_Hovered != value)
                    {
                        _Hovered = value;
                        if (!_Selected && !_Error)
                            OnRedrawRequired();
                    }
                }
            }

            private bool _Error;
            public bool Error
            {
                get
                {
                    return _Error;
                }
                set
                {
                    if (_Error != value)
                    {
                        _Error = value;
                        OnRedrawRequired();
                    }
                }
            }

            private bool _Selected;
            public bool Selected
            {
                get
                {
                    return _Selected;
                }
                set
                {
                    if (_Selected != value)
                    {
                        _Selected = value;
                        if (!_Error)
                            OnRedrawRequired();
                    }
                }
            }

            private bool _HasAssignedTo;
            public bool HasAssignedTo
            {
                get
                {
                    return _HasAssignedTo;
                }
            }

            private Settings.WindowOptions _WindowOptions;
            public Settings.WindowOptions WindowOptions
            {
                get
                {
                    return _WindowOptions;
                }
                set
                {
                    if (_WindowOptions != value)
                    {
                        _WindowOptions = value;
                        _HasAssignedTo = true;
                    }
                }
            }

            private Settings.WindowTemplate.Assigned _AssignedTo;
            public Settings.WindowTemplate.Assigned AssignedTo
            {
                get
                {
                    return _AssignedTo;
                }
                set
                {
                    if (_AssignedTo != value)
                    {
                        _AssignedTo = value;
                        OnAssignedAccountsChanged();
                        OnRedrawRequired();
                    }

                    _HasAssignedTo = true;
                }
            }

            private void OnAssignedAccountsChanged()
            {
                if (_AssignedTo == null)
                {
                    this.Text = string.Empty;
                }
                else
                {
                    switch (_AssignedTo.Type)
                    {
                        case Settings.WindowTemplate.Assigned.AssignedType.Account:
                            string name = null;
                            foreach (var uid in _AssignedTo.Accounts)
                            {
                                var a = Settings.Accounts[uid];
                                if (a.HasValue)
                                {
                                    name = a.Value.Name;
                                    break;
                                }
                            }
                            this.Text = name;
                            break;
                        case Settings.WindowTemplate.Assigned.AssignedType.AccountsExcluding:
                        case Settings.WindowTemplate.Assigned.AssignedType.AccountsIncluding:
                            var limit = _AssignedTo.Accounts.Length;
                            if (limit > 4)
                                limit = 3;
                            var count = 0;
                            var sb = new StringBuilder(limit * 20);
                            var n = _AssignedTo.Type == Settings.WindowTemplate.Assigned.AssignedType.AccountsExcluding ? "excluding" : "including";
                            foreach (var uid in _AssignedTo.Accounts)
                            {
                                var a = Settings.Accounts[uid];
                                if (a.HasValue)
                                {
                                    sb.Append(a.Value.Name);
                                    sb.Append(", ");
                                    if (++count == limit)
                                        break;
                                }
                            }
                            if (count > 0)
                            {
                                sb.Length -= 2;
                                if (count < _AssignedTo.Accounts.Length)
                                {
                                    sb.Append('\n');
                                    sb.Append(n);
                                    sb.Append(' ');
                                    sb.Append(_AssignedTo.Accounts.Length - count);
                                    sb.Append(" more");
                                }
                                else
                                {
                                    sb.Append("\n(");
                                    sb.Append(n);
                                    sb.Append(')');
                                }
                                this.Text = sb.ToString();
                            }
                            else
                            {
                                sb.Append("\n(");
                                sb.Append(n);
                                sb.Append(')');
                            }
                            break;
                        case Settings.WindowTemplate.Assigned.AssignedType.Disabled:
                            this.Text = "None";
                            break;
                        case Settings.WindowTemplate.Assigned.AssignedType.Any:
                        default:
                            this.Text = string.Empty;
                            break;
                    }
                }
            }

            public void SetAssigned(Settings.WindowTemplate.Assigned.AssignedType type, ushort[] uids)
            {
                Settings.WindowOptions options;
                if (_AssignedTo != null)
                    options = _AssignedTo.Options;
                else
                    options = Settings.WindowOptions.None;

                if (type == Settings.WindowTemplate.Assigned.AssignedType.Any && options == Settings.WindowOptions.None)
                {
                    AssignedTo = null;
                }
                else
                {
                    var assigned = new Settings.WindowTemplate.Assigned(type, uids);
                    if (!assigned.Equals(_AssignedTo))
                        AssignedTo = assigned;
                }
            }

            public void SetAssigned(Settings.WindowOptions options)
            {
                if (_AssignedTo == null)
                {
                    AssignedTo = new Settings.WindowTemplate.Assigned(options);
                }
                else if (_AssignedTo.Type == Settings.WindowTemplate.Assigned.AssignedType.Any && options == Settings.WindowOptions.None)
                {
                    AssignedTo = null;
                }
                else
                {
                    var assigned = new Settings.WindowTemplate.Assigned(_AssignedTo.Type, _AssignedTo.Accounts, options);
                    if (!assigned.Equals(_AssignedTo))
                    {
                        _AssignedTo = assigned;
                        OnRedrawRequired();
                    }
                }
            }

            public Settings.WindowTemplate.Assigned GetAssigned()
            {
                if (_AssignedTo == null)
                {
                    if (_WindowOptions == Settings.WindowOptions.None)
                        return null;
                    return new Settings.WindowTemplate.Assigned(_WindowOptions);
                }
                else if (_AssignedTo.Type == Settings.WindowTemplate.Assigned.AssignedType.Any && _WindowOptions == Settings.WindowOptions.None)
                {
                    return null;
                }
                else if (_AssignedTo.Options == _WindowOptions)
                {
                    return _AssignedTo;
                }
                else
                {
                    return new Settings.WindowTemplate.Assigned(_AssignedTo.Type, _AssignedTo.Accounts, _WindowOptions);
                }
            }

            public bool IndexTextVisible
            {
                get
                {
                    return textIndex.Visible;
                }
                set
                {
                    if (value)
                    {
                        if (!textIndex.Visible)
                        {
                            textIndex.Value = this.Index + 1;
                            textIndex.SelectAll();
                            textIndex.Visible = true;
                            textIndex.Focus();
                        }
                    }
                    else
                        textIndex.Visible = false;
                }
            }

            public void ClearAssignedTo()
            {
                AssignedTo = null;
                _HasAssignedTo = false;
            }

            protected override void OnPaintBackground(PaintEventArgs e)
            {
                if (redraw)
                {
                    redraw = false;

                    if (background == null)
                        background = BufferedGraphicsManager.Current.Allocate(e.Graphics, this.ClientRectangle);

                    var g = background.Graphics;
                    var scale = g.DpiX / 96f;

                    Color c;

                    if (_Error)
                    {
                        g.Clear(Color.FromArgb(235, 201, 217));
                        c = Color.FromArgb(189, 153, 170);
                    }
                    else if (_Selected)
                    {
                        g.Clear(Color.FromArgb(201, 217, 235));
                        c = Color.FromArgb(153, 170, 189);
                    }
                    else if (_Hovered)
                    {
                        g.Clear(Color.FromArgb(220, 220, 220));
                        c = Color.FromArgb(160, 160, 160);
                    }
                    else
                    {
                        g.Clear(Color.FromArgb(200, 200, 200));
                        c = Color.FromArgb(140, 140, 140);
                    }

                    using (var p = new Pen(c, 2))
                    {
                        g.DrawRectangle(p, 1, 1, this.Width - 2, this.Height - 2);
                    }

                    var o  = -(int)(5 * scale + 0.5f);
                    var r = Rectangle.Inflate(this.ClientRectangle, o, o);
                    var sb = new StringBuilder();
                    
                    sb.Append(this.Left + ", " + this.Top);

                    if (this._AssignedTo != null)
                    {
                        var wo = this._AssignedTo.Options;
                        if ((wo & (Settings.WindowOptions.PreventChanges | Settings.WindowOptions.DisableTitleBarButtons | Settings.WindowOptions.TopMost)) != 0)
                        {
                            sb.AppendLine();
                            sb.Append('(');
                            if ((wo & Settings.WindowOptions.PreventChanges) != 0)
                                sb.Append("prevent resizing, ");
                            if ((wo & Settings.WindowOptions.DisableTitleBarButtons) != 0)
                                sb.Append("block buttons, ");
                            if ((wo & Settings.WindowOptions.TopMost) != 0)
                                sb.Append("on top, ");
                            sb.Length -= 2;
                            sb.Append(')');
                        }
                    }

                    TextRenderer.DrawText(g, this.Text, this.Font, r, this.ForeColor, TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter | TextFormatFlags.EndEllipsis | TextFormatFlags.WordBreak);
                    TextRenderer.DrawText(g, sb.ToString(), this.Font, r, this.ForeColor, TextFormatFlags.Left | TextFormatFlags.Top);
                    TextRenderer.DrawText(g, (this.Index+1).ToString(), this.Font, r, this.ForeColor, TextFormatFlags.Right | TextFormatFlags.Top);
                    TextRenderer.DrawText(g, this.Width + " x " + this.Height, this.Font, r, this.ForeColor, TextFormatFlags.Right | TextFormatFlags.Bottom);
                }

                background.Render(e.Graphics);
            }
        }

        public enum SaveMode
        {
            New,
            Overwrite,
            Reference,
        }

        public enum BoundsType
        {
            Unknown,
            ClientBounds,
            WindowBounds,
        }

        private BufferedGraphics background;
        private bool redraw;

        private Screen[] screens;
        private formCustomTemplate[] backgrounds;
        private formCustomTemplate master;
        private WindowBlock activeWindow;
        private Size minimum;
        private List<WindowBlock> windows;
        private Point origin;
        private int[] offset;
        private AnchorStyles sizing;
        private Rectangle currentScreen;
        private Rectangle originRect;
        private bool isModified;
        private byte[] sourceKeysByIndex;
        private int sourceCount;
        private int referenceCount;
        private Settings.WindowTemplate.Assignment.AccountType accountType;

        private static Size _GridSizeDefault = new Size(10, 10);
        
        /// <summary>
        /// Initializes windows with absolute positioning
        /// </summary>
        public formCustomTemplate(Size minimum, BoundsType type, Rectangle[] windows, byte[] keys, byte[] order, bool isModified, int referenceCount, bool canAssign, KeyValuePair<byte, Settings.WindowTemplate.Assigned>[] assigned, Settings.WindowTemplate.Assignment.AccountType accountType)
            : this((formCustomTemplate)null)
        {
            var windowCount = windows != null ? windows.Length : 0;
            if (windowCount > MAX_WINDOWS)
                windowCount = MAX_WINDOWS;

            this.minimum = new Size(minimum.Width > 0 ? minimum.Width : 200, minimum.Height > 0 ? minimum.Height : 200);
            this.sourceCount = windowCount;
            this.referenceCount = referenceCount;
            this.isModified = isModified;
            this.accountType = accountType;

            assignToToolStripMenuItem.Visible = canAssign;
            optionsToolStripMenuItem.Visible = canAssign;
            saveAsToolStripMenuItem.Visible = canAssign;
            saveAsNewToolStripMenuItem.Visible = !canAssign;
            CanAssign = canAssign;
            ShowModificationWarning = referenceCount > 0;

            if (type == BoundsType.WindowBounds)
            {
                includeWindowBordersToolStripMenuItem.Enabled = false;
            }
            else if (type == BoundsType.ClientBounds)
            {
                excludeWindowBordersToolStripMenuItem.Enabled = false;
            }

            if (windows != null)
            {
                Dictionary<byte, Settings.WindowTemplate.Assigned> _assigned = null;
                
                if (assigned != null)
                {
                    _assigned = new Dictionary<byte, Settings.WindowTemplate.Assigned>(assigned.Length);

                    for (var i = 0; i < assigned.Length; i++)
                    {
                        _assigned[assigned[i].Key] = assigned[i].Value;
                    }
                }

                sourceKeysByIndex = new byte[windowCount];

                for (byte i = 0; i < windowCount; i++)
                {
                    var w = windows[i];
                    var index = order != null ? order[i] : i;
                    var key = keys != null ? keys[i] : i;
                    
                    var wb = new WindowBlock()
                    {
                        Bounds = w,
                        SourceBounds = w,
                        SourceIndex = index,
                        SourceKey = key,
                        Index = index,
                        MinimumSize = this.minimum,
                    };

                    wb.IndexChanged += wb_IndexChanged;

                    sourceKeysByIndex[index] = key;

                    Settings.WindowTemplate.Assigned assignedTo;
                    if (canAssign && _assigned != null && _assigned.TryGetValue(key, out assignedTo))
                    {
                        wb.AssignedTo = assignedTo;
                    }

                    this.windows.Add(wb);
                }

                if (order != null)
                    Sort(this.windows);
            }
        }

        private formCustomTemplate(formCustomTemplate master)
        {
            InitializeComponents();

            redraw = true;
            Opacity = 0.85;
            TopMost = true;

            if (master == null)
            {
                screens = Screen.AllScreens;
                backgrounds = new formCustomTemplate[screens.Length];

                windows = new List<WindowBlock>();
                _GridSize = _GridSizeDefault;

                backgrounds[0] = this;
                this.Bounds = screens[0].Bounds;

                this.MouseDown += formCustomTemplate_MouseDown;

                for (var i = 1; i < backgrounds.Length; i++)
                {
                    backgrounds[i] = new formCustomTemplate(this)
                    {
                        Bounds = screens[i].Bounds,
                        _GridSize = _GridSize,
                    };
                    backgrounds[i].MouseDown += formCustomTemplate_MouseDown;
                }
            }
            else
            {
                this.master = master;
            }

            this.Disposed += formCustomTemplate_Disposed;
        }

        protected override void OnInitializeComponents()
        {
            base.OnInitializeComponents();

            InitializeComponent();
        }

        void formCustomTemplate_Disposed(object sender, EventArgs e)
        {
            if (background != null)
                background.Dispose();
        }

        private formCustomTemplate GetCurrentBackground()
        {
            if (backgrounds != null)
            {
                foreach (var b in backgrounds)
                {
                    if (b.Bounds.Contains(Cursor.Position))
                        return b;
                }
            }

            return this;
        }

        protected override CreateParams CreateParams
        {
            get
            {
                var cp = base.CreateParams;
                cp.ExStyle |= (int)(WindowStyle.WS_EX_COMPOSITED | WindowStyle.WS_EX_LAYERED);
                return cp;
            }
        }

        public bool CanAssign
        {
            get;
            private set;
        }

        private bool IsAssigned
        {
            get
            {
                foreach (var w in windows)
                {
                    if (w.HasAssignedTo)
                    {
                        return true;
                    }
                }
                return false;
            }
        }

        public bool ShowModificationWarning
        {
            get;
            set;
        }

        void formCustomTemplate_MouseDown(object sender, MouseEventArgs e)
        {
            var f = (Form)sender;

            if (e.Button == System.Windows.Forms.MouseButtons.Left)
            {
                if (windows.Count == MAX_WINDOWS)
                    return;

                var p = Cursor.Position;

                currentScreen = Screen.FromPoint(p).Bounds;
                originRect = new Rectangle(p.X - 4, p.Y - 4, 8, 8);

                activeWindow = new WindowBlock()
                {
                    Location = new Point((p.X - currentScreen.X) / _GridSize.Width * _GridSize.Width + currentScreen.X, (p.Y - currentScreen.Y) / _GridSize.Height * _GridSize.Height + currentScreen.Y),
                    Error = true,
                    Selected = true,
                    MinimumSize = _GridSize,
                    Index = windows.Count,
                };

                activeWindow.IndexChanged += wb_IndexChanged;

                var h = activeWindow.Handle;
                activeWindow.Size = _GridSize;
                activeWindow.Show(backgrounds[backgrounds.Length - 1]);

                f.MouseMove += formCustomTemplate_MouseMove;
                f.MouseCaptureChanged += formCustomTemplate_MouseCaptureChanged;
                activeWindow.SizeChanged += activeWindow_SizeChanged;
            }
            else if (e.Button == System.Windows.Forms.MouseButtons.Right)
            {
                NativeMethods.ReleaseCapture();

                deleteToolStripMenuItem.Enabled = false;
                assignToToolStripMenuItem.Enabled = false;
                optionsToolStripMenuItem.Enabled = false;
                contextMenu.Show(Cursor.Position);
            }
        }

        void activeWindow_SizeChanged(object sender, EventArgs e)
        {
            var c = (WindowBlock)sender;

            if (c.Error)
            {
                c.MinimumSize = new Size(c.Width > minimum.Width ? minimum.Width : c.Width, c.Height > minimum.Height ? minimum.Height : c.Height);
                if (c.MinimumSize == minimum)
                    c.Error = false;
            }
            else
            {
                c.SizeChanged -= activeWindow_SizeChanged;
            }
        }

        void formCustomTemplate_MouseCaptureChanged(object sender, EventArgs e)
        {
            var f = (Form)sender;

            if (activeWindow.Error)
            {
                activeWindow.Dispose();
            }
            else
            {
                windows.Add(activeWindow);
                activeWindow.MouseDown += activeWindow_MouseDown;
                activeWindow.FormClosing += activeWindow_FormClosing;
                activeWindow.KeyDown += activeWindow_KeyDown;
                activeWindow = null;
            }

            f.MouseMove -= formCustomTemplate_MouseMove;
            f.MouseCaptureChanged -= formCustomTemplate_MouseCaptureChanged;
        }

        void activeWindow_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Delete)
            {
                this.Focus();
                Remove((WindowBlock)sender);
            }
        }

        private int SnapX(int x)
        {
            if (_GridSize.Width > 1)
            {
                var sx = (int)((float)(x - currentScreen.X) / _GridSize.Width + 0.5f) * _GridSize.Width + currentScreen.X;
                var dx = currentScreen.Right - x;

                if (dx < 0)
                    dx = -dx;
                if (dx < _GridSize.Width / 2)
                    sx = currentScreen.Right;

                return sx;
            }

            return x;
        }

        private int SnapY(int y)
        {
            if (_GridSize.Height > 1)
            {
                var sy = (int)((float)(y - currentScreen.Y) / _GridSize.Height + 0.5f) * _GridSize.Height + currentScreen.Y;
                var dy = currentScreen.Bottom - y;

                if (dy < 0)
                    dy = -dy;
                if (dy < _GridSize.Height / 2)
                    sy = currentScreen.Bottom;

                return sy;
            }

            return y;
        }

        void activeWindow_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (e.CloseReason == CloseReason.UserClosing)
            {
                this.Close();
            }
        }

        void activeWindow_MouseDown(object sender, MouseEventArgs e)
        {
            var c = (Form)sender;

            if (e.Button == System.Windows.Forms.MouseButtons.Left)
            {
                origin = Cursor.Position;
                sizing = AnchorStyles.None;
                currentScreen = Screen.FromPoint(origin).Bounds;
                originRect = new Rectangle(origin.X - 4, origin.Y - 4, 8, 8);

                if (e.X < SIZING_SIZE)
                {
                    sizing = AnchorStyles.Left;
                }
                else if (e.X > c.Width - SIZING_SIZE)
                {
                    sizing = AnchorStyles.Right;
                }

                if (e.Y < SIZING_SIZE)
                {
                    sizing |= AnchorStyles.Top;
                }
                else if (e.Y > c.Height - SIZING_SIZE)
                {
                    sizing |= AnchorStyles.Bottom;
                }

                if (sizing == AnchorStyles.None)
                    offset = new int[] { c.Left, c.Top };
                else
                    offset = new int[] { c.Left, c.Top, c.Right, c.Bottom };

                c.MouseMove += activeWindow_MouseMove;
                c.MouseCaptureChanged += activeWindow_MouseCaptureChanged;
            }
            else if (e.Button == System.Windows.Forms.MouseButtons.Right)
            {
                NativeMethods.ReleaseCapture();

                deleteToolStripMenuItem.Enabled = true;
                assignToToolStripMenuItem.Enabled = true;
                optionsToolStripMenuItem.Enabled = true;

                var w = (WindowBlock)c;
                Settings.WindowOptions o;
                if (w.AssignedTo != null)
                    o = w.AssignedTo.Options;
                else
                    o = Settings.WindowOptions.None;

                preventChangesToolStripMenuItem.Checked = (o & Settings.WindowOptions.PreventChanges) != 0;
                blockMinimizecloseButtonsToolStripMenuItem.Checked = (o & Settings.WindowOptions.DisableTitleBarButtons) != 0;
                showOnTopOfOtherWindowsToolStripMenuItem.Checked = (o & Settings.WindowOptions.TopMost) != 0;

                contextMenu.Show(Cursor.Position);
            }
        }

        void activeWindow_MouseCaptureChanged(object sender, EventArgs e)
        {
            var f = (Form)sender;

            f.MouseMove -= activeWindow_MouseMove;
            f.MouseCaptureChanged -= activeWindow_MouseCaptureChanged;
        }

        void activeWindow_MouseMove(object sender, MouseEventArgs e)
        {
            var f = (Form)sender;
            var p = Cursor.Position;
            if (originRect.Width != 0)
            {
                if (originRect.Contains(p))
                    return;
                originRect = Rectangle.Empty;
            }
            if (!currentScreen.Contains(p))
                currentScreen = Screen.FromPoint(p).Bounds;

            if (sizing == AnchorStyles.None)
            {
                f.Location = new Point(SnapX(p.X - origin.X + offset[0]), SnapY(p.Y - origin.Y + offset[1]));
            }
            else
            {
                int x, y, r, b;

                x = offset[0];
                y = offset[1];
                r = offset[2];
                b = offset[3];

                switch (sizing & (AnchorStyles.Left | AnchorStyles.Right))
                {
                    case AnchorStyles.Left:
                        x = SnapX(p.X - origin.X + x);
                        if (x + minimum.Width > r)
                            x = r - minimum.Width;
                        break;
                    case AnchorStyles.Right:
                        r = SnapX(p.X - origin.X + r);
                        break;
                }

                switch (sizing & (AnchorStyles.Top | AnchorStyles.Bottom))
                {
                    case AnchorStyles.Top:
                        y = SnapY(p.Y - origin.Y + y);
                        if (y + minimum.Height > b)
                            y = b - minimum.Height;
                        break;
                    case AnchorStyles.Bottom:
                        b = SnapY(p.Y - origin.Y + b);
                        break;
                }

                f.Bounds = new Rectangle(x, y, r - x, b - y);
            }
        }

        void formCustomTemplate_MouseMove(object sender, MouseEventArgs e)
        {
            var p = Cursor.Position;
            if (!currentScreen.Contains(p))
                currentScreen = Screen.FromPoint(p).Bounds;

            var w = SnapX(p.X) - activeWindow.Left;
            var h = SnapY(p.Y) - activeWindow.Top;

            if (w < _GridSize.Width)
                w = _GridSize.Width;

            if (h < _GridSize.Height)
                h = _GridSize.Height;

            activeWindow.Size = new Size(w, h);
        }

        protected override void OnShown(EventArgs e)
        {
            base.OnShown(e);

            if (backgrounds != null)
            {
                for (var i = 1; i < backgrounds.Length; i++)
                {
                    backgrounds[i].Show(backgrounds[i - 1]);
                }

                var last = backgrounds[backgrounds.Length - 1];

                foreach (var w in windows)
                {
                    w.Show(last);

                    w.MouseDown += activeWindow_MouseDown;
                    w.FormClosing += activeWindow_FormClosing;
                    w.KeyDown += activeWindow_KeyDown;
                }

                saveToolStripMenuItem.Enabled = Overwrite == SaveMode.Overwrite;

                this.Focus();
            }
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            base.OnFormClosing(e);

            if (backgrounds == null)
            {
                if (e.CloseReason == CloseReason.UserClosing && master != null)
                {
                    master.Close();
                }
            }
            else
            {
                for (var i = 1; i < backgrounds.Length; i++)
                {
                    backgrounds[i].Dispose();
                }

                foreach (var w in windows)
                {
                    w.Dispose();
                }
            }
        }

        protected override void OnSizeChanged(EventArgs e)
        {
            base.OnSizeChanged(e);

            if (background != null)
            {
                background.Dispose();
                background = null;
            }
            OnRedrawRequired();
        }

        private void OnRedrawRequired()
        {
            if (redraw)
                return;
            redraw = true;
            this.Invalidate();
        }

        protected override void OnPaintBackground(PaintEventArgs e)
        {
            if (redraw)
            {
                redraw = false;

                if (background == null)
                    background = BufferedGraphicsManager.Current.Allocate(e.Graphics, this.ClientRectangle);

                var g = background.Graphics;

                g.Clear(this.BackColor);

                using (var pen = new Pen(Color.FromArgb(240, 240, 240)))
                {
                    if (_GridSize.Width > 2)
                    {
                        for (int x = _GridSize.Width, w = this.Width - 1; x < w; x += _GridSize.Width)
                        {
                            g.DrawLine(pen, x, 0, x, this.Height);
                        }
                    }

                    if (_GridSize.Height > 2)
                    {
                        for (int y = _GridSize.Height, h = this.Height - 1; y < h; y += _GridSize.Height)
                        {
                            g.DrawLine(pen, 0, y, this.Width, y);
                        }
                    }
                }
            }

            background.Render(e.Graphics);
        }

        private Size _GridSize;
        private Size GridSize
        {
            get
            {
                return _GridSize;
            }
            set
            {
                if (master == null)
                {
                    foreach (var f in backgrounds)
                    {
                        f._GridSize = value;
                        f.OnRedrawRequired();
                    }
                }
                else
                {
                    master.GridSize = value;
                }
            }
        }

        public TemplateResult Result
        {
            get;
            private set;
        }

        public SaveMode Overwrite
        {
            get;
            set;
        }

        public bool CanOverwriteAssign
        {
            get;
            set;
        }

        private void Sort(List<WindowBlock> windows)
        {
            windows.Sort(new Comparison<WindowBlock>(
                delegate(WindowBlock a, WindowBlock b)
                {
                    return a.Index.CompareTo(b.Index);
                }));
        }

        /// <summary>
        /// Gets the resulting window template
        /// </summary>
        /// <param name="createNew">Removes any references</param>
        /// <param name="setAssigned">True to add a reference to this template</param>
        private TemplateResult GetResult(SaveMode mode, bool setAssigned)
        {
            var screens = new Dictionary<Screen, List<WindowBlock>>();
            List<WindowBlock> windowsByScreen = null;
            var currentScreen = Rectangle.Empty;
            var windowCount = windows.Count;
            if (windowCount > MAX_WINDOWS)
                windowCount = MAX_WINDOWS;
            var existing = new bool[sourceCount];
            var existingUsed = 0;
            var hasAssigned = false;
            var usedKeys = referenceCount > 0 ? new bool[256] : null;
            byte[] unusedKeys = null;

            for (var i = 0; i < windowCount; i++)
            {
                //sorting windows by screens
                var w = windows[i];

                if (!currentScreen.Contains(w.Bounds))
                {
                    var screen = Screen.FromRectangle(w.Bounds);
                    if (!screens.TryGetValue(screen, out windowsByScreen))
                        screens[screen] = windowsByScreen = new List<WindowBlock>();
                    currentScreen = screen.Bounds;
                }

                windowsByScreen.Add(w);

                if (w.HasAssignedTo)
                {
                    hasAssigned = true;
                }

                if (referenceCount > 0 && w.SourceKey != -1)
                {
                    usedKeys[w.SourceKey] = true;
                    existing[w.SourceIndex] = true;
                    existingUsed++;
                }
            }

            #region Generate array of unused keys for windows without keys

            //existing keys that were dropped will be used first, then the lowest unused key will be used

            if (referenceCount > 0 && windowCount - existingUsed > 0)
            {
                unusedKeys = new byte[windowCount - existingUsed];
                var ei = 0;

                for (var i = 0; i < unusedKeys.Length; i++)
                {
                    byte key = 0;

                    if (existingUsed < sourceCount)
                    {
                        do
                        {
                            if (!existing[ei])
                            {
                                existing[ei] = true;
                                key = sourceKeysByIndex[ei];
                                usedKeys[key] = true;
                                
                                ++ei;
                                break;
                            }

                            ++ei;
                        }
                        while (ei < sourceCount);

                        if (++existingUsed == sourceCount)
                            ei = 0;
                    }
                    else
                    {
                        do
                        {
                            if (!usedKeys[ei])
                            {
                                usedKeys[ei] = true;
                                key = (byte)ei;

                                ++ei;
                                break;
                            }

                            ++ei;
                        }
                        while (ei < usedKeys.Length);
                    }

                    unusedKeys[i] = key;
                }
            }

            #endregion

            if (setAssigned && !hasAssigned && mode != SaveMode.Reference)
            {
                setAssigned = false;
            }

            var result = new ScreenWindows[screens.Count];
            var assigned = setAssigned ? new KeyValuePair<byte, Settings.WindowTemplate.Assigned>[windowCount] : null;
            var indexes = new byte[windowCount];
            var keys = new byte[windowCount];
            var defaultIndexes = true;
            var defaultKeys = true;
            var ri = 0;
            var wi = 0;
            var ki = 0;
            var ai = 0;

            foreach (var screen in screens.Keys)
            {
                var w = screens[screen];
                var rs = new Rectangle[w.Count];

                for (var j = 0; j < rs.Length; j++)
                {
                    rs[j] = new Rectangle(w[j].Left - screen.Bounds.X, w[j].Top - screen.Bounds.Y, w[j].Width, w[j].Height);

                    byte key, index;

                    index = (byte)w[j].Index;
                    indexes[wi] = index;
                    if (index != wi)
                        defaultIndexes = false;

                    if (referenceCount == 0)
                    {
                        key = (byte)wi;
                    }
                    else
                    {
                        if (w[j].SourceKey == -1)
                            key = unusedKeys[ki++];
                        else
                            key = (byte)w[j].SourceKey;

                        if (key != wi)
                            defaultKeys = false;
                    }
                    keys[wi] = key;

                    if (setAssigned && w[j].AssignedTo != null)
                    {
                        assigned[ai++] = new KeyValuePair<byte, Settings.WindowTemplate.Assigned>(key, w[j].AssignedTo);
                    }

                    ++wi;
                }

                result[ri++] = new ScreenWindows(screen.Bounds, rs);
            }

            if (setAssigned)
            {
                if (ai == 0)
                    assigned = null;
                else if (ai < assigned.Length)
                    Array.Resize(ref assigned, ai);
            }

            var type = BoundsType.Unknown;
            if (includeWindowBordersToolStripMenuItem.Checked)
                type = BoundsType.WindowBounds;
            else if (excludeWindowBordersToolStripMenuItem.Checked)
                type = BoundsType.ClientBounds;
            else if (!excludeWindowBordersToolStripMenuItem.Enabled)
                type = BoundsType.ClientBounds;
            else if (!includeWindowBordersToolStripMenuItem.Enabled)
                type = BoundsType.WindowBounds;

            return new TemplateResult(type, result, defaultKeys ? null : keys, defaultIndexes ? null : indexes, assigned);
        }

        private bool ShowTemplateModificationWarning()
        {
            if (ShowModificationWarning && IsModified)
            {
                if (MessageBox.Show(GetCurrentBackground(), "Changes made to the base template will apply to all linked templates.\n\nAre you sure?", "Warning", MessageBoxButtons.YesNo, MessageBoxIcon.Exclamation) != System.Windows.Forms.DialogResult.Yes)
                    return false;
            }

            return true;
        }

        private bool ShowAssignedWarning()
        {
            if (CanAssign && IsAssigned)
            {
                if (MessageBox.Show(this, "Window assignments will not be saved.\n\nAre you sure?", "Warning", MessageBoxButtons.YesNo, MessageBoxIcon.Exclamation) != System.Windows.Forms.DialogResult.Yes)
                    return false;
            }

            return true;
        }

        private void saveAsNewToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (!ShowAssignedWarning())
                return;

            Overwrite = SaveMode.New;
            Result = GetResult(SaveMode.New, false);
            this.DialogResult = System.Windows.Forms.DialogResult.OK;
        }

        private void saveToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (!ShowTemplateModificationWarning())
                return;

            Overwrite = SaveMode.Overwrite;
            Result = GetResult(SaveMode.Overwrite, CanAssign && IsAssigned);
            this.DialogResult = System.Windows.Forms.DialogResult.OK;
        }

        private WindowBlock GetFocusedWindow()
        {
            foreach (var w in windows)
            {
                if (w.Focused)
                {
                    return w;
                }
            }

            return null;
        }

        private void Remove(WindowBlock w)
        {
            if (windows.Remove(w))
            {
                isModified = true;

                for (var i = windows.Count - 1; i >= 0; --i)
                {
                    if (windows[i].Index == i)
                        break;
                    windows[i].Index = i;
                }
            }

            w.Dispose();
        }

        private void deleteToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var w = GetFocusedWindow();
            if (w != null)
            {
                this.Focus();
                Remove(w);
            }
        }

        private void gridSizeToolStripMenuItem_Click(object sender, EventArgs e)
        {
            using (var f = new formGridSize(_GridSize))
            {
                f.StartPosition = FormStartPosition.Manual;
                f.Location = Util.ScreenUtil.CenterScreen(f.Size, Screen.FromPoint(Cursor.Position));

                if (f.ShowDialog(GetCurrentBackground()) == System.Windows.Forms.DialogResult.OK)
                {
                    GridSize = _GridSizeDefault = f.Value;
                }
            }
        }

        private void cancelToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.DialogResult = System.Windows.Forms.DialogResult.Cancel;
        }

        private void anyToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ShowAssignTo(Settings.WindowTemplate.Assigned.AssignedType.Any);
        }

        private void SetWindowOption(Settings.WindowOptions o, bool value)
        {
            var w = GetFocusedWindow();
            if (w != null)
            {
                Settings.WindowOptions options;
                if (w.AssignedTo != null)
                    options = w.AssignedTo.Options;
                else
                    options = Settings.WindowOptions.None;

                if (value)
                    options |= o;
                else
                    options &= ~o;

                w.SetAssigned(options);
            }
        }

        private void ShowAssignTo(Settings.WindowTemplate.Assigned.AssignedType type)
        {
            var w = GetFocusedWindow();
            if (w != null)
            {
                string title;
                switch (type)
                {
                    case Settings.WindowTemplate.Assigned.AssignedType.AccountsExcluding:
                        title = "Select which accounts to exclude from using this template";
                        break;
                    case Settings.WindowTemplate.Assigned.AssignedType.AccountsIncluding:
                        title = "Select which accounts can use this template";
                        break;
                    case Settings.WindowTemplate.Assigned.AssignedType.Disabled:
                    case Settings.WindowTemplate.Assigned.AssignedType.Any:
                        w.SetAssigned(type, null);
                        return;
                    case Settings.WindowTemplate.Assigned.AssignedType.Account:
                        title = "Select which account can use this template";
                        break;
                    default:
                        return;
                }
                Settings.AccountType? accountType;
                switch (this.accountType)
                {
                    case Settings.WindowTemplate.Assignment.AccountType.GuildWars1:
                        accountType = Settings.AccountType.GuildWars1;
                        break;
                    case Settings.WindowTemplate.Assignment.AccountType.GuildWars2:
                        accountType = Settings.AccountType.GuildWars2;
                        break;
                    default:
                        accountType = null;
                        break;
                }
                HashSet<ushort> accounts = null;
                if (w.AssignedTo != null && w.AssignedTo.Accounts != null)
                    accounts = new HashSet<ushort>(w.AssignedTo.Accounts);
                var multi = type == Settings.WindowTemplate.Assigned.AssignedType.AccountsExcluding || type == Settings.WindowTemplate.Assigned.AssignedType.AccountsIncluding;
                using (var f = new formAccountSelect(title, Util.Accounts.GetAccounts(), false, multi, accountType,
                    delegate(Settings.IAccount a)
                    {
                        return accounts != null && accounts.Contains(a.UID);
                    }))
                {
                    var bg = GetCurrentBackground();
                    f.TopMost = bg.TopMost;
                    if (f.ShowDialog(bg) == System.Windows.Forms.DialogResult.OK)
                    {
                        if (f.Selected.Count > 0)
                        {
                            var uids = new ushort[f.Selected.Count];
                            for (var i = 0; i < uids.Length; i++)
                                uids[i] = f.Selected[i].UID;
                            Array.Sort<ushort>(uids);
                            w.SetAssigned(type, uids);
                        }
                        else
                        {
                            w.SetAssigned(Settings.WindowTemplate.Assigned.AssignedType.Any, null);
                        }
                    }
                }
            }
        }

        private void accountToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ShowAssignTo(Settings.WindowTemplate.Assigned.AssignedType.Account);
        }

        private bool IsModified
        {
            get
            {
                var modified = isModified;

                if (!modified)
                {
                    foreach (var w in windows)
                    {
                        if (w.IsBoundsModified)
                        {
                            modified = true;
                            break;
                        }
                    }
                }

                return modified;
            }
        }

        private void saveAsReferenceToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (!ShowTemplateModificationWarning())
                return;

            Overwrite = SaveMode.Reference;
            Result = GetResult(SaveMode.Reference, true);
            this.DialogResult = System.Windows.Forms.DialogResult.OK;
        }

        private void ResizeWindowsByBorders(bool includeBorders, bool excludeBorders)
        {
            var screens = new Dictionary<Screen, int[]>();
            var currentScreen = Rectangle.Empty;
            int[] padding = null;
            int[] zero = new int[4];
            var windowCount = windows.Count;

            if (includeBorders || excludeBorders)
            {
                for (var i = windows.Count - 1; i >= 0; --i)
                {
                    var w = windows[i];

                    if (!currentScreen.Contains(w.Bounds))
                    {
                        var screen = Screen.FromRectangle(w.Bounds);
                        if (!screens.TryGetValue(screen, out padding))
                        {
                            var _p = Util.Dpi.GetWindowFrame(screen);
                            if (includeBorders)
                                padding = new int[] { -_p.Left, -_p.Top, _p.Horizontal, _p.Vertical };
                            else
                                padding = new int[] { _p.Left, _p.Top, -_p.Horizontal, -_p.Vertical };
                            screens[screen] = padding;
                        }
                        currentScreen = screen.Bounds;
                    }

                    var p = w.WindowPadding;
                    if (p == null)
                    {
                        p = zero;
                    }

                    w.Bounds = new Rectangle(w.Left + padding[0] - p[0], w.Top + padding[1] - p[1], w.Width + padding[2] - p[2], w.Height + padding[3] - p[3]);
                    w.WindowPadding = padding;
                }
            }
            else
            {
                for (var i = windows.Count - 1; i >= 0; --i)
                {
                    var w = windows[i];
                    var p = w.WindowPadding;
                    
                    if (p != null)
                    {
                        w.Bounds = new Rectangle(w.Left - p[0], w.Top - p[1], w.Width - p[2], w.Height - p[3]);
                        w.WindowPadding = null;
                    }
                }
            }
        }

        private void includeWindowBordersToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var t = (ToolStripMenuItem)sender;
            t.Checked = !t.Checked;

            if (t == includeWindowBordersToolStripMenuItem)
                excludeWindowBordersToolStripMenuItem.Checked = false;
            else
                includeWindowBordersToolStripMenuItem.Checked = false;

            ResizeWindowsByBorders(includeWindowBordersToolStripMenuItem.Checked && includeWindowBordersToolStripMenuItem.Enabled, excludeWindowBordersToolStripMenuItem.Checked && excludeWindowBordersToolStripMenuItem.Enabled);
        }

        void wb_IndexChanged(object sender, int index)
        {
            var wb = (WindowBlock)sender;
            var last = windows.Count - 1;
            var wi = wb.Index;

            if (index > last)
                index = last;
            if (wi == index)
                return;
            var b = index > wi;

            foreach (var w in windows)
            {
                if (b)
                {
                    if (w.Index > wi && w.Index <= index)
                    {
                        --w.Index;
                    }
                }
                else
                {
                    if (w.Index < wi && w.Index >= index)
                    {
                        ++w.Index;
                    }
                }
            }

            wb.Index = index;

            Sort(windows);
        }

        private void noneToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ShowAssignTo(Settings.WindowTemplate.Assigned.AssignedType.Disabled);
        }

        private void accountsIncludingToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ShowAssignTo(Settings.WindowTemplate.Assigned.AssignedType.AccountsIncluding);
        }

        private void accountsExcludingToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ShowAssignTo(Settings.WindowTemplate.Assigned.AssignedType.AccountsExcluding);
        }

        private void preventChangesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var c = (ToolStripMenuItem)sender;
            c.Checked = !c.Checked;
            SetWindowOption(Settings.WindowOptions.PreventChanges, c.Checked);
        }

        private void blockMinimizecloseButtonsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var c = (ToolStripMenuItem)sender;
            c.Checked = !c.Checked;
            SetWindowOption(Settings.WindowOptions.DisableTitleBarButtons, c.Checked);
        }

        private void showOnTopOfOtherWindowsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var c = (ToolStripMenuItem)sender;
            c.Checked = !c.Checked;
            SetWindowOption(Settings.WindowOptions.TopMost, c.Checked);
        }
    }
}
