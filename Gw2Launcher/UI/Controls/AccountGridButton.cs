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
using ColorNames = Gw2Launcher.Settings.AccountGridButtonColors.Colors;

namespace Gw2Launcher.UI.Controls
{
    public class AccountGridButton : Control
    {
        protected const string
            TEXT_ACCOUNT = "Account",
            TEXT_LAST_USED = "Last used",
            TEXT_STATUS_NEVER = "never";

        public static readonly Settings.AccountGridButtonColors DefaultColors;

        static AccountGridButton()
        {
            var colors = DefaultColors = new Settings.AccountGridButtonColors();

            colors[ColorNames.Name] = Color.Black;
            colors[ColorNames.User] = Color.FromArgb(68, 79, 129);
            colors[ColorNames.StatusDefault] = Color.Gray;
            colors[ColorNames.StatusError] = Color.DarkRed;
            colors[ColorNames.StatusWaiting] = Color.DarkBlue;
            colors[ColorNames.StatusOK] = Color.DarkGreen;

            colors[ColorNames.BackColorDefault] = Color.White;
            colors[ColorNames.BackColorHovered] = Color.FromArgb(235, 235, 235);
            colors[ColorNames.BackColorSelected] = Color.FromArgb(230, 236, 244);
            colors[ColorNames.BorderLightDefault] = Color.FromArgb(240, 240, 240);
            colors[ColorNames.BorderLightHovered] = Color.FromArgb(225, 225, 225);
            colors[ColorNames.BorderLightSelected] = Color.FromArgb(220, 226, 234);
            colors[ColorNames.BorderDarkDefault] = Color.FromArgb(230, 230, 230);
            colors[ColorNames.BorderDarkHovered] = Color.FromArgb(215, 215, 215);
            colors[ColorNames.BorderDarkSelected] = Color.FromArgb(210, 216, 224);

            colors[ColorNames.ForeColorHovered] = Color.FromArgb(128, 215, 215, 215);
            colors[ColorNames.ForeColorSelected] = Color.FromArgb(128, 205, 217, 233);

            colors[ColorNames.FocusedHighlight] = Color.FromArgb(189, 194, 202);
            colors[ColorNames.FocusedBorder] = Color.FromArgb(63, 72, 204);

            colors[ColorNames.ActionExitFill] = Color.FromArgb(244, 225, 230);
            colors[ColorNames.ActionExitFlash] = Color.FromArgb(255, 207, 212);
            colors[ColorNames.ActionFocusFlash] = Color.FromArgb(221, 224, 255);
        }

        protected static Image[] DefaultImage;

        public static readonly Font FONT_NAME = new Font("Segoe UI", 10.25F, FontStyle.Regular, GraphicsUnit.Point, 0);
        public static readonly Font FONT_STATUS = new Font("Segoe UI Semilight", 8.25F, FontStyle.Regular, GraphicsUnit.Point, 0);
        public static readonly Font FONT_USER = new Font("Calibri", 7.25F, FontStyle.Regular, GraphicsUnit.Point, 0);

        protected static readonly Point
            POINT_NAME = new Point(10, 10),
            POINT_ACCOUNT = new Point(10, 28),
            POINT_ACCOUNT_VALUE = new Point(62, 28),
            POINT_LAST_USED = new Point(10, 44),
            POINT_LAST_USED_VALUE = new Point(62, 44);

        protected const byte INDEX_ICON_MAIN = 0;
        protected const byte INDEX_ICON_NOTE = 1;

        protected const byte INDEX_COLOR_STATUS_DEFAULT = 0;
        protected const byte INDEX_COLOR_STATUS_OK = 1;
        protected const byte INDEX_COLOR_STATUS_WAIT = 2;
        protected const byte INDEX_COLOR_STATUS_ERROR = 3;

        protected const int BORDER_SIZE = 2;
        protected const int BORDER_HORIZONTAL = BORDER_SIZE * 2;
        protected const int BORDER_VERTICAL = BORDER_SIZE * 2;

        public event EventHandler<bool> EndPressed;
        public event EventHandler Pressed;
        public event EventHandler<float> PressedProgress;
        public event EventHandler NoteClicked;

        public enum StatusColors
        {
            Default,
            Ok,
            Waiting,
            Error
        }

        public enum PressedState : byte
        {
            Pressing = 0,
            Pressed = 1,
        }

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
                StateFlags = 3,
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
                options &= ~IconOptions.StateFlags;
            }
        }

        public class PagingData
        {
            public PagingData(Settings.PageData[] pages)
            {
                _Pages = pages;
            }

            private Settings.PageData[] _Pages;
            public Settings.PageData[] Pages
            {
                get
                {
                    return _Pages;
                }
                set
                {
                    _Pages = value;
                    _Current = null;
                    _Page = 0;
                }
            }

            private Settings.PageData _Current;
            public Settings.PageData Current
            {
                get
                {
                    return _Current;
                }
                set
                {
                    _Current = value;
                    if (value == null)
                        _Page = 0;
                    else
                        _Page = value.Page;
                }
            }

            private byte _Page;
            public byte Page
            {
                get
                {
                    return _Page;
                }
                set
                {
                    _Page = value;
                }
            }

            public bool SetCurrent(byte page)
            {
                _Current = Find(page);
                _Page = page;

                return _Current != null;
            }

            public Settings.PageData Find(byte page)
            {
                if (_Pages != null)
                {
                    for (var i = 0; i < _Pages.Length; i++)
                    {
                        if (_Pages[i].Page == page)
                            return _Pages[i];
                    }
                }

                return null;
            }

            public int IndexOf(byte page)
            {
                if (_Pages != null)
                {
                    for (var i = 0; i < _Pages.Length; i++)
                    {
                        if (_Pages[i].Page == page)
                            return i;
                    }
                }
                return -1;
            }
        }

        protected class DisplayedTotp
        {
            public DisplayedTotp(string code, int remaining)
            {
                this.code = code;
                this.limit = Environment.TickCount + remaining;
            }

            public string code;
            public int limit;
            public Size size;
        }

        protected Rectangle rectName, rectUser, rectStatus, rectImage;

        protected bool isHovered, isSelected, isPressed, isFocused;
        protected float pressedProgress;
        protected PressedState pressedState;
        private Color pressedColor;
        protected DisplayedIcon[] icons;
        protected DisplayedIcon activeIcon;
        protected DisplayedTotp totp;

        protected ushort fontNameHeight, fontStatusHeight, fontUserHeight;
        protected Font fontName, fontStatus, fontUser;
        //protected Point pointName, pointUser, pointStatus;
        protected bool resize, redraw;
        protected ushort margin;

        protected BufferedGraphics buffer;

        public AccountGridButton()
        {
            SetStyle(ControlStyles.AllPaintingInWmPaint, true);
            SetStyle(ControlStyles.UserPaint, true);
            SetStyle(ControlStyles.OptimizedDoubleBuffer, true);
            SetStyle(ControlStyles.Selectable, true);

            this.Cursor = System.Windows.Forms.Cursors.Hand;

            icons = new DisplayedIcon[]
            {
                new DisplayedIcon(DisplayedIcon.IconType.None),
                new DisplayedIcon(DisplayedIcon.IconType.None),
            };

            fontName = FONT_NAME;
            fontStatus = FONT_STATUS;
            fontUser = FONT_USER;

            _Colors = DefaultColors;

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
            base.Dispose(disposing);

            if (disposing)
            {
                totp = null;

                if (buffer != null)
                    buffer.Dispose();
                if (_Image != null)
                    _Image.Dispose();
            }
        }

        protected virtual void ResizeLabels(Graphics g)
        {
            var scale = g.DpiX / 96f;

            fontNameHeight = (ushort)(fontName.GetHeight(g) + 0.5f);
            fontStatusHeight = (ushort)(fontStatus.GetHeight(g) + 0.5f);
            fontUserHeight = (ushort)(fontUser.GetHeight(g) + 0.5f);
            margin = (ushort)(5 * scale);

            if (totp != null)
                totp.size = Size.Empty;

            Point pointName, pointUser, pointStatus;
            var paddingStatus = (int)(1 * scale);

            int ch = fontNameHeight + fontStatusHeight;
            if (_ShowAccount)
                ch += fontUserHeight;

            int h = ch + margin * 4 + paddingStatus;
            int w = this.Width;
            if (!_ShowAccount)
                h += (int)(8 * scale);

            int x;

            if (!_ShowImage)
            {
                x = BORDER_SIZE + (int)(10 * scale + 0.5f);
            }
            else
            {
                const int DEFAULT_ICON_SIZE = 26;

                if (_Image == null)
                {
                    var sz = (int)(DEFAULT_ICON_SIZE * scale);

                    rectImage = new Rectangle(BORDER_SIZE + margin * 2, BORDER_SIZE + (h - BORDER_VERTICAL) / 2 - sz / 2, sz, sz);
                    x = rectImage.Right + margin * 2;
                }
                else
                {
                    var sz = _Image.Size;

                    if (_ShowBackgroundImage)
                    {
                        Size max;
                        if (_ImagePlacement == Settings.ImagePlacement.Overflow)
                            max = new Size(w, h);
                        else
                            max = new Size((int)(128 * scale), h);
                        if (sz.Width > max.Width || sz.Height > max.Height)
                            sz = Util.RectangleConstraint.Scale(sz, max);
                        rectImage = new Rectangle(0, h / 2 - sz.Height / 2, sz.Width, sz.Height);
                    }
                    else
                    {
                        Size max;
                        if (_ImagePlacement == Settings.ImagePlacement.Overflow)
                            max = new Size(w - BORDER_HORIZONTAL, h - BORDER_VERTICAL);
                        else
                            max = new Size((int)(128 * scale), h - BORDER_VERTICAL);
                        if (sz.Width > max.Width || sz.Height > max.Height)
                            sz = Util.RectangleConstraint.Scale(sz, max);
                        rectImage = new Rectangle(BORDER_SIZE, BORDER_SIZE + (h - BORDER_VERTICAL) / 2 - sz.Height / 2, sz.Width, sz.Height);
                    }

                    switch (_ImagePlacement)
                    {
                        case Settings.ImagePlacement.Overflow:

                            x = BORDER_SIZE + (int)(DEFAULT_ICON_SIZE * scale) + margin * 4;

                            break;
                        case Settings.ImagePlacement.Shift:
                        default:

                            x = rectImage.Right + margin;

                            break;
                    }
                }
            }

            int y = h / 2 - ch / 2;
            int rw = w - x - margin;

            if (_ShowAccount)
            {
                pointUser = new Point(x, y);
                rectUser = new Rectangle(pointUser, new Size(rw, fontUserHeight));
                y += fontUserHeight;
            }

            pointName = new Point(x, y);
            pointStatus = new Point(x, y + fontNameHeight + paddingStatus);

            rectName = new Rectangle(pointName, new Size(rw, fontNameHeight));
            rectStatus = new Rectangle(pointStatus, new Size(rw, fontStatusHeight));

            if (this.MinimumSize.Height != h)
            {
                this.MinimumSize = new Size(this.MinimumSize.Width, h);
                this.Height = h;
            }
        }

        public void ResizeLabels()
        {
            using (var g = this.CreateGraphics())
            {
                ResizeLabels(g);
            }
        }

        protected static Image GetDefaultImage(Settings.AccountType type, float scale = 1)
        {
            var i = (int)type;

            if (DefaultImage == null)
                DefaultImage = new Image[2];

            if (DefaultImage[i] == null)
            {
                using (var icon = new Icon(type == Settings.AccountType.GuildWars1 ? Properties.Resources.Gw1 : Properties.Resources.Gw2, 48, 48))
                {
                    var image = DefaultImage[i] = new Bitmap((int)(26 * scale), (int)(26 * scale), System.Drawing.Imaging.PixelFormat.Format32bppArgb);

                    using (var g = Graphics.FromImage(image))
                    {
                        g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
                        g.DrawIcon(icon, new Rectangle(0, 0, image.Width, image.Height));
                    }

                    return image;
                }
            }

            return DefaultImage[i];
        }

        /// <summary>
        /// The font used for the status
        /// </summary>
        [DefaultValue(typeof(Font), "Segoe UI Semilight, 8.25pt")]
        public Font FontStatus
        {
            get
            {
                return fontStatus;
            }
            set
            {
                if (fontStatus != value)
                {
                    fontStatus = value;
                    resize = true;
                    OnRedrawRequired();
                }
            }
        }

        /// <summary>
        /// The font used for the user name
        /// </summary>
        [DefaultValue(typeof(Font), "Calibri, 7.25pt")]
        public Font FontUser
        {
            get
            {
                return fontUser;
            }
            set
            {
                if (fontUser != value)
                {
                    fontUser = value;
                    resize = true;
                    OnRedrawRequired();
                }
            }
        }

        /// <summary>
        /// The font used for the name
        /// </summary>
        [DefaultValue(typeof(Font), "Segoe UI, 10.25pt")]
        public Font FontName
        {
            get
            {
                return fontName;
            }
            set
            {
                if (fontName != value)
                {
                    fontName = value;
                    resize = true;
                    OnRedrawRequired();
                }
            }
        }

        private Settings.AccountGridButtonColors _Colors;
        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public Settings.AccountGridButtonColors Colors
        {
            get
            {
                return _Colors;
            }
            set
            {
                _Colors = value;
                OnRedrawRequired();
            }
        }

        public async void BeginPressed(PressedState state, CancellationToken cancel, AccountGridButtonContainer.MousePressedEventArgs e)
        {
            isPressed = true;
            pressedProgress = 0;

            var start = Environment.TickCount;
            const int LIMIT = 10;
            int duration;

            switch (state)
            {
                case PressedState.Pressed:
                    pressedState = PressedState.Pressed;
                    duration = 500;
                    pressedColor = e.FlashColor;
                    break;
                case PressedState.Pressing:
                default:
                    pressedState = PressedState.Pressing;
                    duration = 500;
                    pressedColor = e.FillColor;
                    break;
            }

            try
            {
                while (true)
                {
                    int remaining = duration - (Environment.TickCount - start);
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
                        pressedProgress = (float)(Environment.TickCount - start) / duration;

                        OnRedrawRequired();

                        if (pressedProgress >= 1)
                        {
                            pressedProgress = 1;

                            if (pressedState == PressedState.Pressing)
                            {
                                if (PressedProgress != null)
                                    PressedProgress(this, pressedProgress);

                                await Task.Delay(50, cancel);

                                if (cancel.IsCancellationRequested)
                                    throw new TaskCanceledException();

                                pressedState = PressedState.Pressed;
                                pressedProgress = 0;
                                duration = 500;
                                start = Environment.TickCount;
                                pressedColor = e.FlashColor;

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
                        else if (pressedState == PressedState.Pressed)
                        {
                            pressedColor = Color.FromArgb((int)(e.FlashColor.A * (1 - pressedProgress)), e.FlashColor);
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
            int rw = this.Width - rectName.Left - margin;

            rectName = new Rectangle(rectName.X, rectName.Y, rw, rectName.Height);
            rectStatus = new Rectangle(rectStatus.X, rectStatus.Y, rw, rectStatus.Height);
            rectUser = new Rectangle(rectUser.X, rectUser.Y, rw, rectUser.Height);

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

            if (ms < 0)
            {

            }
            else if (ms < 3600000) //<60m
            {
                return 60000 - ms % 60000;
            }
            else if (ms < 172800000) //<48h
            {
                return 3600000 - ms % 3600000;
            }
            else
            {
                //anything older isn't important / will be updated on daily reset
            }

            //old: displayed time is rounded, so refreshes need to occur at 30s (1m), 90s (2m), etc, until it gets to 24 hours
            //if (ms < 0)
            //{
            //}
            //else if (ms < 3600000) //<60m
            //{
            //    return 60000 - (ms + 30000) % 60000;
            //}
            //else if (ms < 82800000) //<23h
            //{
            //    return 3600000 - (ms + 1800000) % 3600000;
            //}
            //else if (ms < 86400000) //<24h
            //{
            //    return 86400000 - ms;
            //}

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

            for (var i = icons.Length - 1; i >= 0; i--)
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
            this.Focus();

            #region Wine

            //Wine will break enter/leave events when moving the cursor outside of the control while holding down the button
            //Use of DoDragDrop prevents this, otherwise this is needed, but enabling this will break DoDragDrop

            //if (Settings.IsRunningWine)
            //{
            //    EventHandler onLeave = null;
            //    onLeave = delegate
            //    {
            //        this.MouseLeave -= onLeave;
            //        this.MouseCaptureChanged -= onLeave;

            //        Windows.Native.NativeMethods.ReleaseCapture();
            //    };
            //    this.MouseLeave += onLeave;
            //    this.MouseCaptureChanged += onLeave;
            //}

            #endregion

            if (totp != null && e.Button == System.Windows.Forms.MouseButtons.Left)
            {
                return;
            }

            if (activeIcon != null && activeIcon.CanClick && activeIcon.Hovered && e.Button == System.Windows.Forms.MouseButtons.Left)
            {
                activeIcon.Activated = true;
                return;
            }

            base.OnMouseDown(e);
        }

        protected override void OnMouseUp(MouseEventArgs e)
        {
            if (totp != null && e.Button == System.Windows.Forms.MouseButtons.Left)
            {
                return;
            }

            if (activeIcon != null && activeIcon.CanClick && activeIcon.Activated)
            {
                activeIcon.Activated = false;
                return;
            }

            base.OnMouseUp(e);
        }

        protected override void OnMouseClick(MouseEventArgs e)
        {
            if (totp != null && e.Button == System.Windows.Forms.MouseButtons.Left)
            {
                totp = null;
                OnRedrawRequired();
                return;
            }

            if (activeIcon != null && activeIcon.Activated)
            {
                if (activeIcon.Hovered)
                {
                    switch (activeIcon.type)
                    {
                        case DisplayedIcon.IconType.Daily:

                            var a = this._AccountData;
                            if (a != null)
                            {
                                this._LastDailyCompletion = DateTime.UtcNow;
                                if (a.Type == Settings.AccountType.GuildWars2)
                                    ((Settings.IGw2Account)a).LastDailyCompletionUtc = this._LastDailyCompletion;
                            }
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
            var e = elapsed.TotalMinutes;

            if (e >= 2880) //48h
            {
                int days = (int)(e / 1440);

                if (days > 1)
                    return days + " days ago";
                else
                    return "a day ago";
            }
            if (e >= 60)
            {
                int hours = (int)(e / 60);

                if (hours > 1)
                    return hours + " hours ago";
                else
                    return "an hour ago";
            }
            else
            {
                int minutes = (int)(e);

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

            var scale = g.DpiX / 96f;
            int iw = image.Width;
            int ih = image.Height;
            int w = (int)(iw * scale + 0.5f);
            int h = (int)(ih * scale + 0.5f);
            offsetX = (int)(offsetX * scale + 0.5f);
            offsetY = (int)(offsetY * scale + 0.5f);

            if (type == DisplayedIcon.IconType.Note)
            {
                icon = icons[INDEX_ICON_NOTE];

                if (icon.type != type)
                {
                    icon.type = type;
                    icon.bounds = new Rectangle(this.Width - w - (int)(15 * scale + 0.5f), (this.Height - h) / 2, w, h);

                    var iconMain = icons[INDEX_ICON_MAIN];
                    if (iconMain != null && iconMain.type != DisplayedIcon.IconType.None)
                    {
                        var x = iconMain.bounds.X - w / 3;
                        if (x < icon.bounds.X)
                            icon.bounds.X = x;
                        icon.bounds.Y = this.Height - h - (int)(10 * scale + 0.5f);
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

                    int rh = (int)(this.Height * 0.8f);

                    if (h > rh)
                    {
                        int rw = rh * w / h;

                        icon.bounds = new Rectangle(this.Width - rw / 2 + offsetX, (this.Height - rh) / 2 + offsetY, rw / 2 - offsetX, rh);
                    }
                    else
                    {
                        icon.bounds = new Rectangle(this.Width - w / 2 + offsetX, (this.Height - h) / 2 + offsetY, w / 2 - offsetX, h);
                    }
                }

                iw = iw / 2 - offsetX;
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

                    g.DrawImage(image, icon.bounds, 0, 0, iw, ih, GraphicsUnit.Pixel, ia);
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

                    g.DrawImage(image, icon.bounds, 0, 0, iw, ih, GraphicsUnit.Pixel, ia);
                }
            }
            else
            {
                g.DrawImage(image, icon.bounds, 0, 0, iw, ih, GraphicsUnit.Pixel);
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
                Color borderLight, borderDark, background, foreground;

                int width = this.Width;
                int height = this.Height;

                if (isSelected)
                {
                    background = _Colors[ColorNames.BackColorSelected];
                    borderLight = _Colors[ColorNames.BorderLightSelected];
                    borderDark = _Colors[ColorNames.BorderDarkSelected];
                    foreground = _Colors[ColorNames.ForeColorSelected];
                }
                else if (isHovered)
                {
                    background = _Colors[ColorNames.BackColorHovered];
                    borderLight = _Colors[ColorNames.BorderLightHovered];
                    borderDark = _Colors[ColorNames.BorderDarkHovered];
                    foreground = _Colors[ColorNames.ForeColorHovered];
                }
                else
                {
                    background = _Colors[ColorNames.BackColorDefault];
                    borderLight = _Colors[ColorNames.BorderLightDefault];
                    borderDark = _Colors[ColorNames.BorderDarkDefault];
                    foreground = Color.Empty;
                }

                using (var brush = new SolidBrush(borderLight))
                {
                    if (background.A < 255)
                    {
                        g.Clear(_Colors[ColorNames.BackColorDefault]);

                        if (background.A != 0)
                        {
                            brush.Color = background;
                            g.FillRectangle(brush, 0, 0, width, height);
                        }
                    }
                    else
                    {
                        g.Clear(background);
                    }

                    if (_ShowBackgroundImage)
                    {
                        try
                        {
                            var image = this.BackgroundImage;
                            if (image == null)
                                image = _DefaultBackgroundImage;
                            g.DrawImage(image, 0, 0, width, height);
                        }
                        catch { }

                        if (foreground.A != 0)
                        {
                            brush.Color = foreground;
                            g.FillRectangle(brush, 0, 0, width, height);
                        }
                    }

                    if (isPressed)
                    {
                        brush.Color = pressedColor;
                        if (pressedState == PressedState.Pressed)
                        {
                            g.FillRectangle(brush, 0, 0, width, height);
                        }
                        else
                        {
                            g.FillRectangle(brush, 0, 0, (int)(width * pressedProgress), height);
                        }
                    }

                    if (_ShowImage)
                    {
                        if (_Image == null)
                        {
                            var image = GetDefaultImage(_AccountType, g.DpiX / 96f);

                            using (var ia = new ImageAttributes())
                            {
                                ia.SetColorMatrix(new ColorMatrix()
                                {
                                    Matrix33 = 0.85f,
                                });

                                g.DrawImage(image, rectImage, 0, 0, image.Width, image.Height, GraphicsUnit.Pixel, ia);
                            }
                        }
                        else
                        {
                            try
                            {
                                g.DrawImage(_Image, rectImage);
                            }
                            catch { }
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

                    if (borderLight.A != 0)
                    {
                        brush.Color = borderLight;
                        g.FillRectangle(brush, 0, 0, width - BORDER_SIZE, BORDER_SIZE); //top
                        g.FillRectangle(brush, 0, BORDER_SIZE, BORDER_SIZE, height-BORDER_VERTICAL); //left
                    }

                    if (borderDark.A != 0)
                    {
                        brush.Color = borderDark;
                        g.FillRectangle(brush, 0, height - BORDER_SIZE, width, BORDER_SIZE); //bottom
                        g.FillRectangle(brush, width - BORDER_SIZE, 0, BORDER_SIZE, height - BORDER_SIZE); //right
                    }

                    if (isFocused)
                    {
                        var hasBorder = _Colors[ColorNames.FocusedBorder].A != 0;
                        var hasFill = _Colors[ColorNames.FocusedHighlight].A != 0;

                        if (hasBorder || hasFill)
                        {
                            var x = (width-BORDER_HORIZONTAL) / 5;
                            using (var gradient = new LinearGradientBrush(new Rectangle(x - BORDER_SIZE, 0, width, height), Color.Transparent, _Colors[ColorNames.FocusedHighlight], LinearGradientMode.Horizontal))
                            {
                                if (hasFill)
                                {
                                    int fw = width - x - BORDER_SIZE, fh = height - BORDER_VERTICAL, fy = BORDER_SIZE;

                                    if (!hasBorder)
                                    {
                                        if (borderLight.A == 0)
                                        {
                                            fy = 0;
                                            fh += BORDER_SIZE;
                                        }
                                        if (borderDark.A == 0)
                                        {
                                            fh += BORDER_SIZE;
                                            fw += BORDER_SIZE;
                                        }
                                    }

                                    g.FillRectangle(gradient, x, fy, fw, fh);
                                }

                                if (hasBorder)
                                {
                                    gradient.LinearColors = new Color[] { Color.Transparent, _Colors[ColorNames.FocusedBorder] };

                                    using (var p = new Pen(gradient, BORDER_SIZE))
                                    {
                                        var bh2 = BORDER_SIZE / 2;

                                        g.DrawLines(p, new Point[]
                                        {
                                            new Point(x, bh2),
                                            new Point(width - bh2, bh2),
                                            new Point(width - bh2,height - bh2),
                                            new Point(x,height - bh2),
                                        });
                                    }
                                }
                            }
                        }
                    }

                    if (_ShowColorKey && _ColorKey.A != 0)
                    {
                        brush.Color = _ColorKey;
                        g.FillRectangle(brush, 0, 0, (int)(g.DpiX / 96f * 5), height);
                    }
                }
            }
        }

        protected Color GetStatusColor(StatusColors color)
        {
            switch (_StatusColor)
            {
                case StatusColors.Error:

                    return _Colors[ColorNames.StatusError];

                case StatusColors.Ok:

                    return _Colors[ColorNames.StatusOK];

                case StatusColors.Waiting:

                    return _Colors[ColorNames.StatusWaiting];

                case StatusColors.Default:
                default:

                    return _Colors[ColorNames.StatusDefault];

            }
        }

        protected virtual void OnPrePaint(PaintEventArgs e)
        {
            if (redraw)
            {
                redraw = false;

                var g = buffer.Graphics;

                if (_ShowAccount)
                {
                    TextRenderer.DrawText(g, _AccountName, fontUser, rectUser, _Colors[ColorNames.User], TextFormatFlags.EndEllipsis | TextFormatFlags.NoPadding);
                }

                TextRenderer.DrawText(g, _DisplayName, fontName, rectName, _Colors[ColorNames.Name], TextFormatFlags.EndEllipsis | TextFormatFlags.NoPadding);

                if (_Status != null)
                {
                    TextRenderer.DrawText(g, _Status, fontStatus, rectStatus, GetStatusColor(_StatusColor), TextFormatFlags.EndEllipsis | TextFormatFlags.NoPadding);
                }
                else if (this._LastUsed == DateTime.MinValue)
                {
                    TextRenderer.DrawText(g, TEXT_STATUS_NEVER, fontStatus, rectStatus, _Colors[ColorNames.StatusError], TextFormatFlags.EndEllipsis | TextFormatFlags.NoPadding);
                }
                else
                {
                    TextRenderer.DrawText(g, FormatLastUsed(), fontStatus, rectStatus, _Colors[ColorNames.StatusDefault], TextFormatFlags.EndEllipsis | TextFormatFlags.NoPadding);
                }

                if (totp != null)
                {
                    using (var brush = new SolidBrush(Color.FromArgb(220, 0, 0, 0)))
                    {
                        var w = this.Width;
                        var h = this.Height;
                        var y = h - margin;
                        var bw = totp.limit - Environment.TickCount;

                        g.FillRectangle(brush, 0, 0, w, h);

                        if (bw > 0)
                        {
                            if (bw < 3000)
                            {
                                int c;
                                if (bw <= 2500)
                                    c = 0;
                                else
                                    c = (int)(90 * (bw - 2500) / 500f);
                                brush.Color = Color.FromArgb(90, c, c);
                            }
                            else
                                brush.Color = Color.FromArgb(90, 90, 90);

                            bw = (int)((bw / 30000f) * w);
                            if (bw >= w)
                            {
                                g.FillRectangle(brush, 0, y, w, margin);
                            }
                            else
                            {
                                if (bw > 0)
                                    g.FillRectangle(brush, 0, y, bw, margin);
                                brush.Color = Color.FromArgb(60, 60, 60);
                                g.FillRectangle(brush, bw, y, w - bw, margin);
                            }
                        }

                        if (totp.size.IsEmpty)
                            totp.size = TextRenderer.MeasureText(g, totp.code, fontName, Size.Empty, TextFormatFlags.NoPadding);

                        var p = new Point((w - totp.size.Width) / 2, (h - totp.size.Height) / 2);

                        TextRenderer.DrawText(g, totp.code, fontName, new Point(p.X + 1, p.Y + 1), Color.Black, TextFormatFlags.NoPadding);
                        TextRenderer.DrawText(g, totp.code, fontName, p, Color.WhiteSmoke, TextFormatFlags.NoPadding);
                    }
                }
            }
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            OnPrePaint(e);

            buffer.Render(e.Graphics);

            base.OnPaint(e);
        }

        protected void OnRedrawRequired()
        {
            redraw = true;
            this.Invalidate();
        }

        public void SetStatus(string status, StatusColors color)
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

        public ushort SortKey
        {
            get
            {
                if (_Paging != null && _Paging.Current != null)
                {
                    return _Paging.Current.SortKey;
                }
                else if (_AccountData != null)
                {
                    return _AccountData.SortKey;
                }
                else
                {
                    return ushort.MaxValue;
                }
            }
            set
            {
                if (_Paging != null && _Paging.Current != null)
                {
                    _Paging.Current.SortKey = value;
                }
                else if (_AccountData != null)
                {
                    _AccountData.SortKey = value;
                }
            }
        }

        public ushort GridIndex
        {
            get;
            set;
        }

        public bool GridVisibility
        {
            get;
            set;
        }

        private async void UpdateTotpCode()
        {
            do
            {
                await Task.Delay(50);

                if (totp == null)
                {
                    break;
                }

                var t = Environment.TickCount;
                if (t > totp.limit)
                {
                    totp = null;
                    OnRedrawRequired();
                    break;
                }

                OnRedrawRequired();
            }
            while (true);
        }

        public void ShowTotpCode(string code, int remainingMillis)
        {
            var b = totp == null;

            if (code != null)
            {
                totp = new DisplayedTotp(code, remainingMillis);
            }
            else
            {
                totp = null;
            }
            
            if (b && code != null)
            {
                UpdateTotpCode();
            }

            OnRedrawRequired();
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

        private StatusColors _StatusColor;
        public StatusColors StatusColor
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

        private bool _ShowImage;
        public bool ShowImage
        {
            get
            {
                return _ShowImage;
            }
            set
            {
                if (_ShowImage != value)
                {
                    _ShowImage = value;
                    resize = true;
                    OnRedrawRequired();
                }
            }
        }

        private string _ImagePath;
        private Image _Image;
        public Image Image
        {
            get
            {
                return _Image;
            }
            set
            {
                if (_Image != value)
                {
                    _ImagePath = null;
                    _Image = value;
                    if (_ShowImage)
                    {
                        resize = true;
                        OnRedrawRequired();
                    }
                }
            }
        }
        private Settings.ImagePlacement _ImagePlacement;
        public Settings.ImagePlacement ImagePlacement
        {
            get
            {
                return _ImagePlacement;
            }
            set
            {
                if (_ImagePlacement != value)
                {
                    _ImagePlacement = value;
                    if (_ShowImage)
                    {
                        resize = true;
                        OnRedrawRequired();
                    }
                }
            }
        }

        private Image _DefaultBackgroundImage;
        public Image DefaultBackgroundImage
        {
            get
            {
                return _DefaultBackgroundImage;
            }
            set
            {
                if (_DefaultBackgroundImage != value)
                {
                    _ShowBackgroundImage = value != null || BackgroundImage != null;
                    _DefaultBackgroundImage = value;
                    resize = true;
                    OnRedrawRequired();
                }
            }
        }

        private bool _ShowBackgroundImage;
        private string _BackgroundImagePath;
        public override Image BackgroundImage
        {
            get
            {
                return base.BackgroundImage;
            }
            set
            {
                _BackgroundImagePath = null;
                _ShowBackgroundImage = value != null || _DefaultBackgroundImage != null;
                base.BackgroundImage = value;
                resize = true;
                OnRedrawRequired();
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
                totp = null;

                if (value != null)
                {
                    this.DisplayName = value.Name;
                    this.AccountName = value.WindowsAccount;
                    this.AccountType = value.Type;
                    if (value.Pages == null)
                        this.Paging = null;
                    else
                        this.Paging = new PagingData(value.Pages);
                    if (changed || value.LastUsedUtc > _LastUsed)
                        this.LastUsedUtc = value.LastUsedUtc;
                    this.ShowDailyLogin = value.ShowDailyLogin;

                    if (!object.ReferenceEquals(_ImagePath, value.Image != null ? value.Image.Path : null))
                    {
                        if (_ImagePath != null && this._Image != null)
                        {
                            this._Image.Dispose();
                            this._Image = null;
                        }

                        if (value.Image != null)
                        {
                            try
                            {
                                this._ImagePlacement = value.Image.Placement;
                                this.Image = Bitmap.FromFile(value.Image.Path);
                                this.ShowImage = true;
                            }
                            catch { }

                            _ImagePath = value.Image.Path;
                        }
                        else
                        {
                            _ImagePath = null;

                            if (_ShowImage)
                            {
                                resize = true;
                                OnRedrawRequired();
                            }
                        }
                    }
                    else if (_ImagePath != null)
                    {
                        this.ImagePlacement = value.Image.Placement;
                    }

                    if (!object.ReferenceEquals(_BackgroundImagePath, value.BackgroundImage))
                    {
                        if (_BackgroundImagePath != null && this.BackgroundImage != null)
                        {
                            this.BackgroundImage.Dispose();
                            this.BackgroundImage = null;
                        }

                        if (value.BackgroundImage != null)
                        {
                            try
                            {
                                this.BackgroundImage = Bitmap.FromFile(value.BackgroundImage);
                            }
                            catch { }
                        }

                        _BackgroundImagePath = value.BackgroundImage;
                    }

                    if (value.Type == Settings.AccountType.GuildWars2)
                    {
                        var gw2 = (Settings.IGw2Account)value;
                        this.ShowDailyCompletion = gw2.ShowDailyCompletion;
                        this.LastDailyCompletionUtc = gw2.LastDailyCompletionUtc;
                        if (gw2.ApiData != null && gw2.ApiData.Played != null)
                            this.LastDailyLoginUtc = gw2.ApiData.Played.LastChange;
                        else
                            this.LastDailyLoginUtc = value.LastUsedUtc;
                    }
                    else if (value.Type == Settings.AccountType.GuildWars1)
                    {
                        var gw1 = (Settings.IGw1Account)value;
                        this.ShowDailyCompletion = false;
                        this.LastDailyCompletionUtc = DateTime.MinValue;
                        this.LastDailyLoginUtc = value.LastUsedUtc;
                    }

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

        private Settings.AccountType _AccountType;
        public Settings.AccountType AccountType
        {
            get
            {
                return _AccountType;
            }
            set
            {
                if (_AccountType != value)
                {
                    _AccountType = value;
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
                    var b = _AccountData == null;

                    if (!b)
                    {
                        switch (_AccountType)
                        {
                            case Settings.AccountType.GuildWars2:

                                var gw2 = (Settings.IGw2Account)_AccountData;
                                b = gw2.ApiData == null || gw2.ApiData.Played == null;

                                break;
                            case Settings.AccountType.GuildWars1:

                                b = true;

                                break;
                        }
                    }

                    if (b)
                    {
                        _LastDailyLogin = value;
                    }

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
                    _ColorKey = value;
                    OnRedrawRequired();
                }
            }
        }

        private PagingData _Paging;
        public PagingData Paging
        {
            get
            {
                return _Paging;
            }
            set
            {
                _Paging = value;
            }
        }

        public bool IsHovered
        {
            get
            {
                return isHovered;
            }
            set
            {
                if (isHovered != value)
                {
                    isHovered = value;
                    OnRedrawRequired();
                }
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
