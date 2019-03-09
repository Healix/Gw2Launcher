using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.Windows.Forms;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;

namespace Gw2Launcher.UI.Controls
{
    public partial class AccountGridButton : UserControl
    {
        protected const string 
            TEXT_ACCOUNT= "Account",
            TEXT_LAST_USED = "Last used",
            TEXT_STATUS_NEVER = "never";

        protected static readonly Color BACK_COLOR = Color.White;
        protected static readonly Color BACK_COLOR_HOVER = Color.FromArgb(235, 235, 235);
        protected static readonly Color BACK_COLOR_SELECTED = Color.FromArgb(230, 236, 244);
        public static readonly Font FONT_LARGE = new Font("Segoe UI Semibold", 8.25F, FontStyle.Bold, GraphicsUnit.Point, 0);
        public static readonly Font FONT_SMALL = new Font("Segoe UI Semilight", 8.25F, FontStyle.Regular, GraphicsUnit.Point, 0);
        protected static readonly Point
            POINT_NAME = new Point(10, 10),
            POINT_ACCOUNT = new Point(10, 28),
            POINT_ACCOUNT_VALUE = new Point(62, 28),
            POINT_LAST_USED = new Point(10, 44),
            POINT_LAST_USED_VALUE = new Point(62, 44);

        protected const byte INDEX_ICON_MAIN = 0;
        protected const byte INDEX_ICON_NOTE = 1;

        public event EventHandler<bool> EndPressed;
        public event EventHandler Pressed;
        public event EventHandler<float> PressedProgress;
        public event EventHandler NoteClicked;

        protected class DisplayedIcon
        {
            public enum IconType : byte
            {
                None,
                Login,
                Daily,
                Note
            }

            [Flags]
            public enum IconOptions : byte
            {
                None = 0,
                Hovered = 1,
                Activated = 2,
            };

            public DisplayedIcon(IconType type)
            {
                this.type = type;
            }

            public Rectangle bounds;
            public IconType type;
            public IconOptions options;

            public bool Hovered
            {
                get
                {
                    return options.HasFlag(IconOptions.Hovered);
                }
                set
                {
                    SetOption(IconOptions.Hovered, value);
                }
            }

            public bool Activated
            {
                get
                {
                    return options.HasFlag(IconOptions.Activated);
                }
                set
                {
                    SetOption(IconOptions.Activated, value);
                }
            }

            public bool CanClick
            {
                get
                {
                    switch (type)
                    {
                        case IconType.Daily:
                        case IconType.Note:
                            return true;
                    }

                    return false;
                }
            }

            public void SetOption(IconOptions state, bool value)
            {
                if (value)
                    this.options |= state;
                else
                    this.options &= ~state;
            }

            public void ClearState()
            {
                options &= ~(IconOptions)3;
            }
        }

        protected Rectangle rectName, rectAccountValue, rectLastUsedValue, rectIcon;
        protected SolidBrush brushColor;
        protected SolidBrush brush;
        protected Pen pen;

        protected bool isHovered, isSelected, isPressed, isFocused;
        protected float pressedProgress;
        protected byte pressedState;
        protected DisplayedIcon[] icons;
        protected DisplayedIcon activeIcon;

        protected ushort fontLargeHeight, fontSmallHeight;
        protected Font fontLarge, fontSmall;
        protected Point pointName, pointAccount, pointAccountValue, pointLastUsed, pointLastUsedValue;
        protected bool resize, redraw;

        protected BufferedGraphics buffer;

        public AccountGridButton()
        {
            InitializeComponent();

            SetStyle(ControlStyles.AllPaintingInWmPaint, true);
            SetStyle(ControlStyles.UserPaint, true);

            icons = new DisplayedIcon[]
            {
                new DisplayedIcon(DisplayedIcon.IconType.None),
                new DisplayedIcon(DisplayedIcon.IconType.None),
            };

            fontLarge = FONT_LARGE;
            fontSmall = FONT_SMALL;

            brush = new SolidBrush(Color.LightGray);
            pen = new Pen(brush);
            redraw = true;

            _AccountName = "Example";
            _DisplayName = "example@example.com";
            _LastUsed = DateTime.MinValue;
            _ShowAccount = true;

            using (var g = this.CreateGraphics())
            {
                ResizeLabels(g);
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                pen.Dispose();
                brush.Dispose();
                if (buffer != null)
                    buffer.Dispose();
                if (components != null)
                    components.Dispose();
            }
            base.Dispose(disposing);
        }

        protected virtual void ResizeLabels(Graphics g)
        {
            fontSmallHeight = (ushort)(fontSmall.GetHeight(g) + 0.5f);
            fontLargeHeight = (ushort)(fontLarge.GetHeight(g) + 0.5f);
            var scale = g.DpiX / 96f;

            var pointName = new Point((int)(POINT_NAME.X * scale + 0.5f), (int)(POINT_NAME.Y * scale + 0.5f));

            pointAccount = new Point(pointName.X, pointName.Y + fontLargeHeight + 3);

            if (_ShowAccount)
                pointLastUsed = new Point(pointAccount.X, pointAccount.Y + fontSmallHeight + 1);
            else
                pointLastUsed = pointAccount;

            Size lastUsed = TextRenderer.MeasureText(g, TEXT_LAST_USED, fontSmall);

            pointAccountValue = new Point(pointAccount.X + lastUsed.Width + 3, pointAccount.Y);
            pointLastUsedValue = new Point(pointAccountValue.X, pointLastUsed.Y);

            rectName = new Rectangle(pointName, new Size(this.Width - pointAccount.X * 2, fontLargeHeight + 1));
            rectAccountValue = new Rectangle(pointAccountValue, new Size(this.Width - pointAccount.X - pointAccountValue.X, fontSmallHeight + 1));
            rectLastUsedValue = new Rectangle(pointLastUsedValue, new Size(this.Width - pointLastUsed.X - pointLastUsedValue.X, fontSmallHeight + 1));

            int height = rectLastUsedValue.Bottom + pointName.Y - 2;

            if (this.MinimumSize.Height != height)
            {
                this.MinimumSize = new Size(this.MinimumSize.Width, height);
                this.Height = height;
            }
        }

        public void ResizeLabels()
        {
            using (var g = this.CreateGraphics())
            {
                ResizeLabels(g);
            }
        }

        /// <summary>
        /// The smaller font used for "last used" and "account"
        /// </summary>
        public Font FontSmall
        {
            get
            {
                return fontSmall;
            }
            set
            {
                if (fontSmall != value)
                {
                    fontSmall = value;
                    resize = true;
                    OnRedrawRequired();
                }
            }
        }

        /// <summary>
        /// The larger font used for the name
        /// </summary>
        public Font FontLarge
        {
            get
            {
                return fontLarge;
            }
            set
            {
                if (fontLarge != value)
                {
                    fontLarge = value;
                    resize = true;
                    OnRedrawRequired();
                }
            }
        }

        public async void BeginPressed(int state, CancellationToken cancel, AccountGridButtonContainer.MousePressedEventArgs e)
        {
            isPressed = true;
            pressedProgress = 0;

            var start = DateTime.UtcNow.Ticks;
            const int LIMIT = 10;
            int duration;

            switch (state)
            {
                case 1:
                    pressedState = 1;
                    duration = 500;
                    brush.Color = e.FlashColor;
                    break;
                case 0:
                default:
                    pressedState = 0;
                    duration = 500;
                    brush.Color = e.FillColor;
                    break;
            }

            try
            {
                while (true)
                {
                    int remaining = duration - (int)((DateTime.UtcNow.Ticks - start) / 10000);
                    if (remaining > LIMIT)
                        await Task.Delay(LIMIT, cancel);
                    else if (remaining > 0)
                        await Task.Delay(remaining, cancel);

                    if (cancel.IsCancellationRequested)
                    {
                        throw new TaskCanceledException();
                    }
                    else
                    {
                        pressedProgress = (float)((DateTime.UtcNow.Ticks - start) / 10000) / duration;

                        OnRedrawRequired();

                        if (pressedProgress >= 1)
                        {
                            pressedProgress = 1;

                            if (pressedState == 0)
                            {
                                if (PressedProgress != null)
                                    PressedProgress(this, pressedProgress);

                                await Task.Delay(50, cancel);

                                if (cancel.IsCancellationRequested)
                                    throw new TaskCanceledException();

                                pressedState = 1;
                                pressedProgress = 0;
                                duration = 500;
                                start = DateTime.UtcNow.Ticks;
                                brush.Color = e.FlashColor;

                                OnRedrawRequired();

                                if (Pressed != null)
                                    Pressed(this, EventArgs.Empty);
                            }
                            else
                            {
                                isPressed = false;

                                break;
                            }
                        }
                        else if (pressedState == 1)
                        {
                            brush.Color = Color.FromArgb(255 - (int)(255 * pressedProgress), e.FlashColor);
                        }
                        else if (PressedProgress != null)
                            PressedProgress(this, pressedProgress);
                    }
                }
            }
            catch
            {
                isPressed = false;
                OnRedrawRequired();
            }
            finally
            {
                if (EndPressed != null)
                    EndPressed(this, cancel.IsCancellationRequested);
            }
        }

        protected override void OnSizeChanged(EventArgs e)
        {
            base.OnSizeChanged(e);

            if (buffer != null)
            {
                buffer.Dispose();
                buffer = null;
            }

            OnSizeChanged();
        }

        protected virtual void OnSizeChanged()
        {
            rectName = new Rectangle(POINT_NAME, new Size(this.Width - POINT_NAME.X * 2, fontLargeHeight + 1));
            rectAccountValue = new Rectangle(pointAccountValue, new Size(this.Width - pointAccount.X - pointAccountValue.X, fontSmallHeight + 1));
            rectLastUsedValue = new Rectangle(pointLastUsedValue, new Size(this.Width - pointLastUsed.X - pointLastUsedValue.X, fontSmallHeight + 1));

            if (icons != null)
            {
                foreach (var icon in icons)
                    icon.type = DisplayedIcon.IconType.None;
            }

            redraw = true;
            if (!resize)
                this.Invalidate();
        }

        public void Redraw()
        {
            OnRedrawRequired();
        }

        private int OnDelayedRefresh()
        {
            if (this.IsDisposed)
                return -1;

            OnRedrawRequired();

            return GetNextRefresh();
        }

        private int GetNextRefresh()
        {
            var ms = (int)DateTime.UtcNow.Subtract(_LastUsed).TotalMilliseconds;
            //displayed time is rounded, so refreshes need to occur at 30s (1m), 90s (2m), etc, until it gets to 24 hours

            if (ms < 0)
            {
            }
            else if (ms < 3600000)
            {
                return 60000 - (ms + 30000) % 60000;
            }
            else if (ms < 82800000)
            {
                return 3600000 - (ms + 1800000) % 3600000;
            }
            else if (ms < 86400000)
            {
                return 86400000 - ms;
            }

            return -1;
        }

        private void DelayedRefresh()
        {
            var refresh = GetNextRefresh();
            if (refresh != -1)
                Util.ScheduledEvents.Register(OnDelayedRefresh, refresh);
        }

        protected override void OnMouseEnter(EventArgs e)
        {
            base.OnMouseEnter(e);

            isHovered = true;
            OnRedrawRequired();
        }

        protected override void OnMouseLeave(EventArgs e)
        {
            base.OnMouseLeave(e);

            if (activeIcon != null)
            {
                activeIcon.ClearState();
                activeIcon = null;
            }
            isHovered = false;
            OnRedrawRequired();
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);

            for (var i = icons.Length - 1; i >= 0;i--)
            {
                var icon = icons[i];

                if (icon.type != DisplayedIcon.IconType.None && icon.bounds.Contains(e.Location))
                {
                    bool redraw;

                    if (activeIcon != null)
                    {
                        if (activeIcon == icon)
                            return;
                        else
                        {
                            redraw = activeIcon.CanClick || icon.CanClick;
                            activeIcon.ClearState();
                        }
                    }
                    else
                        redraw = icon.CanClick;

                    activeIcon = icon;
                    icon.Hovered = true;

                    if (redraw)
                    {
                        OnRedrawRequired();
                    }

                    return;
                }
            }

            if (activeIcon != null)
            {
                activeIcon.ClearState();

                if (activeIcon.CanClick)
                {
                    OnRedrawRequired();
                }

                activeIcon = null;
            }
        }

        protected override void OnMouseDown(MouseEventArgs e)
        {
            if (activeIcon != null && activeIcon.CanClick && activeIcon.Hovered && e.Button == System.Windows.Forms.MouseButtons.Left)
            {
                activeIcon.Activated = true;
                return;
            }

            base.OnMouseDown(e);
        }

        protected override void OnMouseUp(MouseEventArgs e)
        {
            if (activeIcon != null && activeIcon.CanClick && activeIcon.Activated)
            {
                activeIcon.Activated = false;
                return;
            }
            
            base.OnMouseUp(e);
        }

        protected override void OnMouseClick(MouseEventArgs e)
        {
            if (activeIcon != null && activeIcon.Activated)
            {
                if (activeIcon.Hovered)
                {
                    switch (activeIcon.type)
                    {
                        case DisplayedIcon.IconType.Daily:
                            
                            var a = this._AccountData;
                            if (a != null)
                                a.LastDailyCompletionUtc = this._LastDailyCompletion = DateTime.UtcNow;
                            OnRedrawRequired();

                            break;
                        case DisplayedIcon.IconType.Note:

                            if (NoteClicked != null)
                                NoteClicked(this, e);

                            break;
                    }
                }
                return;
            }

            base.OnMouseClick(e);
        }

        private string FormatLastUsed()
        {
            var elapsed = DateTime.UtcNow.Subtract(_LastUsed);
            var d = elapsed.TotalHours;

            if (d >= 24)
            {
                int days = (int)(elapsed.TotalDays + 0.5);

                if (days > 1)
                    return days + " days ago";
                else
                    return "a day ago";
            }
            if (d >= 1)
            {
                int hours = (int)(d + 0.5);

                if (hours > 1)
                    return hours + " hours ago";
                else
                    return "an hour ago";
            }
            else
            {
                int minutes = (int)(elapsed.TotalMinutes + 0.5);

                if (minutes > 1)
                    return minutes + " minutes ago";
                else if (minutes == 1)
                    return "a minute ago";
                else
                    return "just now";

            }

        }

        protected void OnPaintIcon(Graphics g, DisplayedIcon.IconType type, Image image, int offsetX, int offsetY, bool grayscale, byte opacity)
        {
            DisplayedIcon icon;

            int w = image.Width;
            int h = image.Height;

            if (type == DisplayedIcon.IconType.Note)
            {
                icon = icons[INDEX_ICON_NOTE];

                if (icon.type != type)
                {
                    icon.type = type;
                    icon.bounds = new Rectangle(this.Width - w - 15, this.Height / 2 - h / 2, w, h);

                    var iconMain = icons[INDEX_ICON_MAIN];
                    if (iconMain != null && iconMain.type != DisplayedIcon.IconType.None)
                    {
                        var x = iconMain.bounds.X - w / 3;
                        if (x < icon.bounds.X)
                            icon.bounds.X = x;
                        icon.bounds.Y = this.Height - h - 10;
                    }
                }
            }
            else
            {
                icon = icons[INDEX_ICON_MAIN];

                if (icon.type != type)
                {
                    icon.type = type;
                    icons[INDEX_ICON_NOTE].type = DisplayedIcon.IconType.None;

                    if (h > this.Height * 0.8)
                    {
                        int rh = (int)(this.Height * 0.8);
                        int rw = rh * w / h;

                        icon.bounds = new Rectangle(this.Width - rw / 2 + offsetX, this.Height / 2 - rh / 2 + offsetY, rw / 2 - offsetX, rh);
                    }
                    else
                    {
                        icon.bounds = new Rectangle(this.Width - w / 2 + offsetX, this.Height / 2 - h / 2 + offsetY, w / 2 - offsetX, h);
                    }
                }

                w = w / 2 - offsetX;
            }

            if (grayscale)
            {
                using (var ia = new ImageAttributes())
                {
                    ia.SetColorMatrix(new ColorMatrix(new float[][] 
                        {
                            new float[] {.3f, .3f, .3f, 0, 0},
                            new float[] {.6f, .6f, .6f, 0, 0},
                            new float[] {.1f, .1f, .1f, 0, 0},
                            new float[] {0, 0, 0, opacity / 255f, 0},
                            new float[] {0, 0, 0, 0, 1}
                        }));

                    g.DrawImage(image, icon.bounds, 0, 0, w, h, GraphicsUnit.Pixel, ia);
                }
            }
            else if (opacity != 255)
            {
                using (var ia = new ImageAttributes())
                {
                    ia.SetColorMatrix(new ColorMatrix()
                        {
                            Matrix33 = opacity / 255f,
                        });

                    g.DrawImage(image, icon.bounds, 0, 0, w, h, GraphicsUnit.Pixel, ia);
                }
            }
            else
            {
                g.DrawImage(image, icon.bounds, 0, 0, w, h, GraphicsUnit.Pixel);
            }
        }

        protected void DrawIcon(Graphics g)
        {

            if (this._ShowDailyLogin || this._ShowDailyCompletion)
            {
                var date = DateTime.UtcNow.Date;

                if (this._ShowDailyLogin && this._LastUsed < date)
                {
                    OnPaintIcon(g, DisplayedIcon.IconType.Login, Properties.Resources.login, -1, 2, false, 255);

                    return;
                }
                if (this._ShowDailyCompletion && this._LastDailyCompletion < date)
                {
                    var hovered = activeIcon != null && activeIcon.type == DisplayedIcon.IconType.Daily && activeIcon.Hovered;
                    
                    OnPaintIcon(g, DisplayedIcon.IconType.Daily, Properties.Resources.daily, -2, 0, hovered, 255);

                    return;
                }
                if (this._ShowDailyLogin && this._LastDailyLogin < date)
                {
                    OnPaintIcon(g, DisplayedIcon.IconType.Login, Properties.Resources.login, -1, 2, true, 127);

                    return;
                }
            }

            if (icons[INDEX_ICON_MAIN].type != DisplayedIcon.IconType.None)
            {
                icons[INDEX_ICON_MAIN].type = DisplayedIcon.IconType.None;
                icons[INDEX_ICON_NOTE].type = DisplayedIcon.IconType.None;
            }
        }

        protected override void OnPaintBackground(PaintEventArgs e)
        {
            if (resize)
            {
                ResizeLabels(e.Graphics);
                resize = false;
            }

            if (redraw)
            {
                if (buffer == null)
                    buffer = BufferedGraphicsManager.Current.Allocate(e.Graphics, this.DisplayRectangle);

                var g = buffer.Graphics;

                if (isSelected)
                    g.Clear(BACK_COLOR_SELECTED);
                else if (isHovered)
                    g.Clear(BACK_COLOR_HOVER);
                else
                    g.Clear(BACK_COLOR);

                int w = this.Width - 1;
                int h = this.Height - 1;

                if (isPressed)
                {
                    if (pressedState == 1)
                    {
                        g.FillRectangle(brush, 0, 0, w, h);
                    }
                    else
                    {
                        g.FillRectangle(brush, 0, 0, (int)(w * pressedProgress), h);
                    }
                }

                DrawIcon(g);

                if (DateTime.UtcNow < _LastNote)
                {
                    var hovered = activeIcon != null && activeIcon.type == DisplayedIcon.IconType.Note && activeIcon.Hovered;

                    OnPaintIcon(g, DisplayedIcon.IconType.Note, Properties.Resources.mailfull, 0, 0, hovered, 255);
                }
                else
                {
                    icons[INDEX_ICON_NOTE].type = DisplayedIcon.IconType.None;
                }

                g.DrawRectangle(pen, 0, 0, w, h);

                if (isFocused)
                {
                    var x = w / 5;

                    using (var gradient = new LinearGradientBrush(new Rectangle(x - 1, 0, w + 1, h), Color.Transparent, Util.Color.Darken(BACK_COLOR_SELECTED, 0.1f), 0f))
                    {
                        g.FillRectangle(gradient, x, 1, w - x, h);

                        gradient.LinearColors = new Color[] { Color.Transparent, Color.FromArgb(63, 72, 204) };

                        using (var p = new Pen(gradient))
                        {
                            g.DrawLines(p, new Point[]
                                {
                                    new Point(x,0),
                                    new Point(w,0),
                                    new Point(w,h),
                                    new Point(x,h),
                                });
                        }
                    }
                }

                if (_ShowColorKey && _ColorKey.A != 0)
                {
                    g.FillRectangle(brushColor, 0, 0, 5, h + 1);
                }
            }
        }

        protected virtual void OnPrePaint(PaintEventArgs e)
        {
            if (redraw)
            {
                redraw = false;

                var g = buffer.Graphics;

                TextRenderer.DrawText(g, _DisplayName, fontLarge, rectName, Color.Black, TextFormatFlags.EndEllipsis);

                if (_ShowAccount)
                {
                    TextRenderer.DrawText(g, TEXT_ACCOUNT, fontSmall, pointAccount, Color.Gray);
                    TextRenderer.DrawText(g, _AccountName, fontSmall, rectAccountValue, Color.Gray, TextFormatFlags.EndEllipsis);
                }

                TextRenderer.DrawText(g, TEXT_LAST_USED, fontSmall, pointLastUsed, Color.Gray);

                if (_Status != null)
                {
                    TextRenderer.DrawText(g, _Status, fontSmall, rectLastUsedValue, _StatusColor, TextFormatFlags.EndEllipsis);
                }
                else if (this._LastUsed == DateTime.MinValue)
                {
                    TextRenderer.DrawText(g, TEXT_STATUS_NEVER, fontSmall, rectLastUsedValue, Color.DarkRed, TextFormatFlags.EndEllipsis);
                }
                else
                {
                    TextRenderer.DrawText(g, FormatLastUsed(), fontSmall, rectLastUsedValue, Color.Gray, TextFormatFlags.EndEllipsis);
                }
            }
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            OnPrePaint(e);

            buffer.Render(e.Graphics);

            base.OnPaint(e);
        }

        private void AccountButtonGrid_Load(object sender, EventArgs e)
        {

        }

        protected void OnRedrawRequired()
        {
            redraw = true;
            this.Invalidate();
        }

        public void SetStatus(string status, Color color)
        {
            _Status = status;
            _StatusColor = color;
            OnRedrawRequired();
        }

        public int Index
        {
            get;
            set;
        }

        private string _Status;
        public string Status
        {
            get
            {
                return _Status;
            }
            set
            {
                if (_Status != value)
                {
                    _Status = value;
                    OnRedrawRequired();
                }
            }
        }

        private Color _StatusColor;
        public Color StatusColor
        {
            get
            {
                return _StatusColor;
            }
            set
            {
                if (_StatusColor != value)
                {
                    _StatusColor = value;
                    OnRedrawRequired();
                }
            }
        }

        private bool _ShowAccount;
        public bool ShowAccount
        {
            get
            {
                return _ShowAccount;
            }
            set
            {
                if (_ShowAccount != value)
                {
                    _ShowAccount = value;
                    resize = true;
                    OnRedrawRequired();
                }
            }
        }

        private bool _ShowColorKey;
        public bool ShowColorKey
        {
            get
            {
                return _ShowColorKey;
            }
            set
            {
                if (_ShowColorKey != value)
                {
                    _ShowColorKey = value;
                    OnRedrawRequired();
                }
            }
        }

        private Settings.IAccount _AccountData;
        public Settings.IAccount AccountData
        {
            get
            {
                return _AccountData;
            }
            set
            {
                var changed = _AccountData != value;

                _AccountData = value;

                if (value != null)
                {
                    this.DisplayName = value.Name;
                    this.AccountName = value.WindowsAccount;
                    if (changed || value.LastUsedUtc > _LastUsed)
                        this.LastUsedUtc = value.LastUsedUtc;
                    this.ShowDailyLogin = value.ShowDailyLogin;
                    this.ShowDailyCompletion = value.ShowDailyCompletion;
                    this.LastDailyCompletionUtc = value.LastDailyCompletionUtc;
                    if (value.ApiData != null && value.ApiData.Played != null)
                        this.LastDailyLoginUtc = value.ApiData.Played.LastChange;
                    else
                        this.LastDailyLoginUtc = value.LastUsedUtc;
                    if (value.Notes != null)
                        this.LastNoteUtc = value.Notes.ExpiresLast;
                    if (value.ColorKey.IsEmpty)
                        this.ColorKey = Util.Color.FromUID(value.UID);
                    else
                        this.ColorKey = value.ColorKey;
                }
            }
        }

        private string _AccountName;
        public string AccountName
        {
            get
            {
                return _AccountName;
            }
            set
            {
                if (_AccountName != value)
                {
                    _AccountName = value;
                    OnRedrawRequired();
                }
            }
        }

        private string _DisplayName;
        public string DisplayName
        {
            get
            {
                return _DisplayName;
            }
            set
            {
                if (_DisplayName != value)
                {
                    _DisplayName = value;
                    OnRedrawRequired();
                }
            }
        }

        private DateTime _LastUsed;
        public DateTime LastUsedUtc
        {
            get
            {
                return _LastUsed;
            }
            set
            {
                if (_LastUsed != value)
                {
                    if (_AccountData == null || _AccountData.ApiData == null || _AccountData.ApiData.Played == null)
                        _LastDailyLogin = value;
                    _LastUsed = value;
                    DelayedRefresh();
                    OnRedrawRequired();
                }
            }
        }

        private bool _ShowDailyLogin;
        public bool ShowDailyLogin
        {
            get
            {
                return _ShowDailyLogin;
            }
            set
            {
                if (_ShowDailyLogin != value)
                {
                    _ShowDailyLogin = value;
                    OnRedrawRequired();
                }
            }
        }

        private bool _ShowDailyCompletion;
        public bool ShowDailyCompletion
        {
            get
            {
                return _ShowDailyCompletion;
            }
            set
            {
                if (_ShowDailyCompletion != value)
                {
                    _ShowDailyCompletion = value;
                    OnRedrawRequired();
                }
            }
        }

        private DateTime _LastNote;
        public DateTime LastNoteUtc
        {
            get
            {
                return _LastNote;
            }
            set
            {
                if (_LastNote != value)
                {
                    _LastNote = value;
                    OnRedrawRequired();
                }
            }
        }

        private DateTime _LastDailyCompletion;
        public DateTime LastDailyCompletionUtc
        {
            get
            {
                return _LastDailyCompletion;
            }
            set
            {
                if (_LastDailyCompletion != value)
                {
                    _LastDailyCompletion = value;
                    OnRedrawRequired();
                }
            }
        }

        private DateTime _LastDailyLogin;
        public DateTime LastDailyLoginUtc
        {
            get
            {
                return _LastDailyLogin;
            }
            set
            {
                if (_LastDailyLogin != value)
                {
                    _LastDailyLogin = value;
                    OnRedrawRequired();
                }
            }
        }

        private Color _ColorKey;
        public Color ColorKey
        {
            get
            {
                return _ColorKey;
            }
            set
            {
                if (_ColorKey != value)
                {
                    if (brushColor == null)
                        brushColor = new SolidBrush(value);
                    else
                        brushColor.Color = value;

                    _ColorKey = value;

                    OnRedrawRequired();
                }
            }
        }

        public bool IsHovered
        {
            get
            {
                return isHovered;
            }
        }

        public bool IsFocused
        {
            get
            {
                return isFocused;
            }
            set
            {
                if (isFocused != value)
                {
                    isFocused = value;
                    OnRedrawRequired();
                }
            }
        }

        public bool Selected
        {
            get
            {
                return isSelected;
            }
            set
            {
                if (isSelected != value)
                {
                    isSelected = value;
                    OnRedrawRequired();
                }
            }
        }
    }
}
