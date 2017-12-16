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

namespace Gw2Launcher.UI.Controls
{
    public partial class AccountGridButton : UserControl
    {
        protected const long TICKS_PER_DAY = 864000000000;
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

        protected Rectangle rectName, rectAccountValue, rectLastUsedValue;
        protected SolidBrush brush;
        protected Pen pen;

        protected bool isHovered, isSelected;
        protected CancellationTokenSource cancelRefresh;

        protected ushort fontLargeHeight, fontSmallHeight;
        protected Font fontLarge, fontSmall;
        protected Point pointName, pointAccount, pointAccountValue, pointLastUsed, pointLastUsedValue;
        protected bool isDirty;

        public AccountGridButton()
        {
            InitializeComponent();

            SetStyle(ControlStyles.AllPaintingInWmPaint, true);
            SetStyle(ControlStyles.UserPaint, true);
            SetStyle(ControlStyles.OptimizedDoubleBuffer, true);

            fontLarge = FONT_LARGE;
            fontSmall = FONT_SMALL;

            brush = new SolidBrush(Color.LightGray);
            pen = new Pen(brush);

            _AccountName = "Example";
            _DisplayName = "example@example.com";
            _LastUsed = DateTime.MinValue;
            _ShowAccount = true;

            using (var g = this.CreateGraphics())
            {
                ResizeLabels(g);
            }

            this.Disposed += AccountGridButton_Disposed;
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                pen.Dispose();
                brush.Dispose();
                if (components != null)
                    components.Dispose();
            }
            base.Dispose(disposing);
        }

        protected void ResizeLabels(Graphics g)
        {
            fontSmallHeight = (ushort)(fontSmall.GetHeight(g) + 0.5f);
            fontLargeHeight = (ushort)(fontLarge.GetHeight(g) + 0.5f);

            pointAccount = new Point(POINT_NAME.X, POINT_NAME.Y + fontLargeHeight + 3);

            if (_ShowAccount)
                pointLastUsed = new Point(POINT_NAME.X, pointAccount.Y + fontSmallHeight + 1);
            else
                pointLastUsed = pointAccount;

            Size lastUsed = TextRenderer.MeasureText(g, TEXT_LAST_USED, fontSmall);

            pointAccountValue = new Point(pointAccount.X + lastUsed.Width + 3, pointAccount.Y);
            pointLastUsedValue = new Point(pointAccountValue.X, pointLastUsed.Y);

            rectName = new Rectangle(POINT_NAME, new Size(this.Width - POINT_NAME.X * 2, fontLargeHeight + 1));
            rectAccountValue = new Rectangle(pointAccountValue, new Size(this.Width - pointAccount.X - pointAccountValue.X, fontSmallHeight + 1));
            rectLastUsedValue = new Rectangle(pointLastUsedValue, new Size(this.Width - pointLastUsed.X - pointLastUsedValue.X, fontSmallHeight + 1));

            int height = pointLastUsed.Y + fontSmallHeight + POINT_NAME.X - 2;

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
                    isDirty = true;
                    this.Invalidate();
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
                    isDirty = true;
                    this.Invalidate();
                }
            }
        }

        void AccountGridButton_Disposed(object sender, EventArgs e)
        {
            if (cancelRefresh != null)
            {
                cancelRefresh.Cancel();
            }
        }

        protected override void OnSizeChanged(EventArgs e)
        {
            base.OnSizeChanged(e);
            OnSizeChanged();
        }

        protected virtual void OnSizeChanged()
        {
            rectName = new Rectangle(POINT_NAME, new Size(this.Width - POINT_NAME.X * 2, fontLargeHeight + 1));
            rectAccountValue = new Rectangle(pointAccountValue, new Size(this.Width - pointAccount.X - pointAccountValue.X, fontSmallHeight + 1));
            rectLastUsedValue = new Rectangle(pointLastUsedValue, new Size(this.Width - pointLastUsed.X - pointLastUsedValue.X, fontSmallHeight + 1));

            this.Invalidate();
        }

        protected override void OnVisibleChanged(EventArgs e)
        {
            base.OnVisibleChanged(e);

            if (this.Visible)
            {
                DelayedRefresh();
            }
            else if (cancelRefresh != null)
            {
                cancelRefresh.Cancel();
            }
        }

        private async void DelayedRefresh()
        {
            if (cancelRefresh != null)
                return;
            else if (DateTime.UtcNow.Subtract(_LastUsed).TotalDays > 1)
                return;

            cancelRefresh = new CancellationTokenSource();
            var token = cancelRefresh.Token;

            do
            {
                try
                {
                    int millisToNextDay = (int)(((_LastUsed.Ticks / TICKS_PER_DAY + 1) * TICKS_PER_DAY - DateTime.UtcNow.Ticks) / 10000);
                    if (millisToNextDay > 60000 || millisToNextDay < 0)
                    {
                        if (millisToNextDay < -TICKS_PER_DAY / 10000)
                            throw new TaskCanceledException();
                        await Task.Delay(60000, token);
                    }
                    else
                        await Task.Delay(millisToNextDay + 1, token);
                }
                catch (TaskCanceledException e)
                {
                    Util.Logging.Log(e);
                    cancelRefresh = null;
                    return;
                }

                if (this.IsDisposed)
                {
                    cancelRefresh = null;
                    return;
                }
                else if (this.Visible)
                    this.Invalidate();
            }
            while (true);
        }

        protected override void OnMouseEnter(EventArgs e)
        {
            base.OnMouseEnter(e);

            isHovered = true;
            this.Invalidate();
        }

        protected override void OnMouseLeave(EventArgs e)
        {
            base.OnMouseLeave(e);

            isHovered = false;
            this.Invalidate();
        }

        private string FormatLastUsed()
        {
            var elapsed = DateTime.UtcNow.Subtract(_LastUsed);
            var d = elapsed.TotalHours;

            if (d > 24)
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

        protected override void OnPaintBackground(PaintEventArgs e)
        {
            var g = e.Graphics;

            if (isSelected)
                g.Clear(BACK_COLOR_SELECTED);
            else if (isHovered)
                g.Clear(BACK_COLOR_HOVER);
            else
                g.Clear(BACK_COLOR);

            if (this._ShowDaily && this._LastUsed != DateTime.MinValue && this._LastUsed.Day != DateTime.UtcNow.Day)
            {
                var daily = Properties.Resources.daily;
                int w = daily.Width;
                int h = daily.Height;

                if (h > this.Height * 0.8)
                {
                    int rh = (int)(this.Height * 0.8);
                    int rw = rh * w / h;
                    g.DrawImage(daily, new Rectangle(this.Width - rw / 2 - 2, this.Height / 2 - rh / 2, rw/2 + 1, rh), new Rectangle(0, 0, w / 2, h), GraphicsUnit.Pixel);
                }
                else
                    g.DrawImageUnscaledAndClipped(daily, new Rectangle(this.Width - w / 2 - 1, this.Height / 2 - h / 2, w / 2 + 1, h));
            }

            g.DrawRectangle(pen, 0, 0, this.Width - 1, this.Height - 1);
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            var g = e.Graphics;

            if (isDirty)
            {
                isDirty = false;
                ResizeLabels(g);
            }

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

        private void AccountButtonGrid_Load(object sender, EventArgs e)
        {

        }

        public void SetStatus(string status, Color color)
        {
            _Status = status;
            _StatusColor = color;
            this.Invalidate();
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
                    this.Invalidate();
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
                    this.Invalidate();
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
                    isDirty = true;
                    this.Invalidate();
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
                _AccountData = value;
                if (value != null)
                {
                    this.DisplayName = value.Name;
                    this.AccountName = value.WindowsAccount;
                    this.LastUsedUtc = value.LastUsedUtc;
                    this.ShowDaily = value.ShowDaily;
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
                    this.Invalidate();
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
                    this.Invalidate();
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
                    _LastUsed = value;
                    DelayedRefresh();
                    this.Invalidate();
                }
            }
        }

        private bool _ShowDaily;
        public bool ShowDaily
        {
            get
            {
                return _ShowDaily;
            }
            set
            {
                if (_ShowDaily != value)
                {
                    _ShowDaily = value;
                    this.Invalidate();
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
                    this.Invalidate();
                }
            }
        }
    }
}
