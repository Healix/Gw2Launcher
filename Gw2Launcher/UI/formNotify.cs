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

namespace Gw2Launcher.UI
{
    public partial class formNotify : Base.BaseForm
    {
        public enum NotifyType
        {
            PatchReady,
            DownloadingFiles,
            DownloadingManifests,
            Error,
            Note,
        }

        private const float BACKGROUND_OPACITY = 0.95f;

        private Base.ShowWithoutActivationForm background;
        private bool animate, attached;
        private Tools.BackgroundPatcher.DownloadProgressEventArgs progress;
        private NotifyType notifyType;
        private Screen screen;
        private Settings.ScreenAnchor anchor;
        //private string formatSize;

        private formNotify(NotifyType t, int screen, Settings.ScreenAnchor anchor)
        {
            InitializeComponents();

            notifyType = t;

            var screens = Screen.AllScreens;
            if (screens.Length <= screen)
                this.screen = Screen.PrimaryScreen;
            else
                this.screen = screens[screen];

            this.anchor = anchor;
        }

        private formNotify(NotifyType t, int screen, Settings.ScreenAnchor anchor, Image image, bool dispose)
            : this(t, screen, anchor)
        {
            this.Controls.Clear();

            var b = this.screen.Bounds;
            var ms = new System.Drawing.Size(b.Width / 5, b.Height / 5);
            var size = Util.RectangleConstraint.Scale(image.Size, ms);

            var p = new PictureBox()
            {
                Location = new Point(2, 2),
                Size = new Size(size.Width - 4, size.Height - 4),
                SizeMode = PictureBoxSizeMode.StretchImage,
                Image = image,
            };

            if (dispose)
            {
                this.Disposed += delegate
                {
                    image.Dispose();
                };
            }

            this.Controls.Add(p);

            var _h = this.Handle; //force
            this.Size = size;

            Initialize(t);
        }

        private formNotify(NotifyType t, int screen, Settings.ScreenAnchor anchor, string text1, string text2)
            : this(t, screen, anchor)
        {
            var b = this.screen.WorkingArea;
            var ms = new System.Drawing.Size(b.Width / 4, b.Height / 4);
            int w, h;
            var spacing = labelBuildCaption.Top - labelTitle.Bottom;
            var xpad = labelTitle.Left * 2;
            var ypad = labelTitle.Top + this.Height - labelElapsedCaption.Bottom;

            labelTitle.MaximumSize = ms;
            labelBuildCaption.MaximumSize = ms;
            labelBuildCaption.AutoEllipsis = true;
            labelTitle.AutoEllipsis = true;
            labelTitle.Text = text1;
            labelBuildCaption.Text = text2;

            w = labelTitle.Width;
            h = labelTitle.Height;

            switch (t)
            {
                default:
                case NotifyType.Note:

                    if (!string.IsNullOrEmpty(text2))
                    {
                        labelBuildCaption.Top = labelTitle.Bottom + labelBuildCaption.Height / 2;
                        //labelBuild.Location = new Point(labelBuildCaption.Left, labelTitle.Bottom + labelBuild.Height / 2);
                        h += labelBuildCaption.Bottom - labelTitle.Bottom;
                        labelBuildCaption.Visible = true;
                        //labelBuildCaption.ForeColor = Color.FromArgb(115, 115, 115);

                        if (labelBuildCaption.Width > w)
                            w = labelBuildCaption.Width;
                    }

                    labelTitle.Visible = true;

                    break;
            }

            if (w < this.Width)
                w = this.Width;

            var _h = this.Handle; //force

            this.Size = new Size(w + xpad, h + ypad);

            Initialize(t);
        }

        private formNotify(NotifyType t, int screen, Settings.ScreenAnchor anchor, Tools.BackgroundPatcher.PatchEventArgs pe)
            : this(t,screen,anchor)
        {
            labelBuild.Text = string.Format(labelBuild.Text, pe.Build);

            switch (t)
            {
                case NotifyType.PatchReady:

                    var elapsed = pe.Elapsed;
                    string _elapsed;
                    if (elapsed.TotalMinutes >= 1)
                    {
                        int m = (int)elapsed.TotalMinutes;
                        _elapsed = m + "m";
                        if (elapsed.Seconds > 0)
                            _elapsed += " " + elapsed.Seconds + "s";
                    }
                    else
                    {
                        _elapsed = elapsed.Seconds + "s";
                    }

                    labelTitle.Text = "Patch ready";
                    labelSize.Text = string.Format(labelSize.Text, pe.Files, Util.Text.FormatBytes(pe.Size), pe.Files != 1 ? "files" : "file");
                    labelElapsed.Text = string.Format(labelElapsed.Text, _elapsed);

                    labelBuild.Visible = true;
                    labelBuildCaption.Visible = true;
                    labelElapsed.Visible = true;
                    labelElapsedCaption.Visible = true;
                    labelSize.Visible = true;
                    labelSizeCaption.Visible = true;
                    labelTitle.Visible = true;

                    break;
                case NotifyType.DownloadingFiles:
                    
                    labelTitle.Text = "Downloading";
                    labelElapsed.Text = "(estimated)";
                    labelElapsed.ForeColor = Color.DimGray;
                    labelSize.Text = string.Format(labelSize.Text, pe.Files, Util.Text.FormatBytes(pe.Size), pe.Files != 1 ? "files" : "file");

                    labelBuild.Visible = true;
                    labelBuildCaption.Visible = true;
                    labelElapsed.Visible = true;
                    labelSize.Visible = true;
                    labelSizeCaption.Visible = true;
                    labelTitle.Visible = true;

                    break;
                case NotifyType.DownloadingManifests:
                    
                    labelTitle.Text = "Downloading build " + labelBuild.Text;
                    labelTitle.Visible = true;

                    this.Height = labelTitle.Location.Y * 2 + labelTitle.Height;
                    this.SizeChanged += formNotify_SizeChanged;

                    break;
                case NotifyType.Error:

                    labelTitle.Text = "Failed to download update";
                    labelTitle.Visible = true;

                    this.Height = labelTitle.Location.Y * 2 + labelTitle.Height;
                    this.SizeChanged += formNotify_SizeChanged;

                    break;
            }

            Initialize(t);
        }

        protected override void OnInitializeComponents()
        {
            base.OnInitializeComponents();

            InitializeComponent();
        }

        protected override bool ShowWithoutActivation
        {
            get
            {
                return true;
            }
        }

        protected override CreateParams CreateParams
        {
            get
            {
                var createParams = base.CreateParams;
                createParams.ExStyle |= (int)(WindowStyle.WS_EX_TOPMOST | WindowStyle.WS_EX_TOOLWINDOW | WindowStyle.WS_EX_NOACTIVATE | WindowStyle.WS_EX_LAYERED);
                return createParams;
            }
        }

        void formNotify_SizeChanged(object sender, EventArgs e)
        {
            labelTitle.Location = new Point(labelTitle.Location.X, this.Height / 2 - labelTitle.Height / 2);
        }

        private void Initialize(NotifyType t)
        {
            background = new Base.ShowWithoutActivationForm()
            {
                BackColor = Color.Black,
                Opacity = BACKGROUND_OPACITY,
                ShowInTaskbar = false,
                FormBorderStyle = System.Windows.Forms.FormBorderStyle.None,
                StartPosition = FormStartPosition.Manual,
                Location = this.Location,
                Size = this.Size,
                Cursor = Cursors.Hand,
            };

            foreach (Control c in this.Controls)
            {
                c.Cursor = Cursors.Hand;
                c.Click += control_Click;
            }

            this.Cursor = Cursors.Hand;
            this.Click += control_Click;
            background.Click += control_Click;

            this.TransparencyKey = this.BackColor;
            this.Owner = background;

            this.VisibleChanged += formNotify_VisibleChanged;
            this.FormClosed += formNotify_FormClosed;
            this.LocationChanged += formNotify_LocationChanged;
            this.Shown += formNotify_Shown;
        }

        void formNotify_FormClosing(object sender, FormClosingEventArgs e)
        {
            Tools.BackgroundPatcher.Instance.DownloadProgress -= bp_DownloadProgress;
        }

        void bp_DownloadProgress(object sender, Tools.BackgroundPatcher.DownloadProgressEventArgs e)
        {
            progress = e;
        }

        void control_Click(object sender, EventArgs e)
        {
            FadeOut(100);
        }

        private static formNotify _instance;
        private static void Show(formNotify f)
        {
            formNotify existing = null;
            try
            {
                if (_instance != null && !_instance.IsDisposed)
                    existing = _instance;
            }
            catch { }

            _instance = f;

            f.animate = true;

            if (existing != null)
            {
                if (existing.anchor == f.anchor && existing.screen == f.screen)
                {
                    EventHandler onShown = null;
                    onShown = delegate
                    {
                        f.Shown -= onShown;

                        bool attached = false;
                        EventHandler onLocationChanged = null;

                        if (f.anchor == Settings.ScreenAnchor.BottomLeft || f.anchor == Settings.ScreenAnchor.Left || f.anchor == Settings.ScreenAnchor.TopLeft || f.anchor == Settings.ScreenAnchor.Top)
                        {
                            //left to right
                            onLocationChanged = delegate
                            {
                                try
                                {
                                    if (attached || f.Location.X > existing.Location.X - f.Width - 5)
                                    {
                                        if (!attached)
                                        {
                                            attached = true;
                                            existing.attached = true;
                                            existing.FadeOut(500);
                                        }
                                        existing.Location = new Point(f.Location.X + f.Width + 5, existing.Location.Y);
                                    }
                                }
                                catch { };
                            };
                        }
                        else
                        {
                            //right to left
                            onLocationChanged = delegate
                            {
                                try
                                {
                                    if (attached || f.Location.X < existing.Location.X + existing.Width - 5)
                                    {
                                        if (!attached)
                                        {
                                            attached = true;
                                            existing.attached = true;
                                            existing.FadeOut(500);
                                        }
                                        existing.Location = new Point(f.Location.X - existing.Width - 5, existing.Location.Y);
                                    }
                                }
                                catch { };
                            };
                        }
                        f.LocationChanged += onLocationChanged;

                        FormClosingEventHandler onClosing = null;
                        onClosing = delegate
                        {
                            f.LocationChanged -= onLocationChanged;
                        };
                        existing.FormClosing += onClosing;
                    };
                    f.Shown += onShown;
                }
                else
                {
                    existing.Close();
                }
            }

            f.Show();
        }

        public static void Show(NotifyType t, int screen, Settings.ScreenAnchor anchor, Tools.BackgroundPatcher.PatchEventArgs pe)
        {
            Show(new formNotify(t, screen, anchor, pe));
        }

        public static void Show(NotifyType t, int screen, Settings.ScreenAnchor anchor, Tools.BackgroundPatcher.DownloadProgressEventArgs pe)
        {
            Show(new formNotify(t, screen, anchor, new Tools.BackgroundPatcher.PatchEventArgs()
                {
                    Build = pe.build,
                    Files = pe.filesTotal + pe.manifestsTotal,
                    Size = pe.estimatedBytesRemaining + pe.contentBytesTotal,
                    Elapsed = DateTime.UtcNow.Subtract(pe.startTime)
                }));
        }

        public static void ShowNote(int screen, Settings.ScreenAnchor anchor, string text, string accountName)
        {
            Show(new formNotify(NotifyType.Note, screen, anchor, text, accountName));
        }

        public static void ShowImage(int screen, Settings.ScreenAnchor anchor, string text, Image image, bool dispose)
        {
            Show(new formNotify(NotifyType.Note, screen, anchor, image, dispose));
        }

        private void formNotify_Load(object sender, EventArgs e)
        {
            var working = screen.WorkingArea;
            int x, y;

            switch (anchor)
            {
                case Settings.ScreenAnchor.Top:
                case Settings.ScreenAnchor.Left:
                case Settings.ScreenAnchor.TopLeft:
                    y = working.Top + 10;
                    x = working.Left - this.Width;
                    break;
                case Settings.ScreenAnchor.Right:
                case Settings.ScreenAnchor.TopRight:
                    y = working.Top + 10;
                    x = working.Right;
                    break;
                case Settings.ScreenAnchor.BottomLeft:
                    y = working.Bottom - this.Height - 10;
                    x = working.Left - this.Width;
                    break;
                case Settings.ScreenAnchor.Bottom:
                case Settings.ScreenAnchor.BottomRight:
                default:
                    y = working.Bottom - this.Height - 10;
                    x = working.Right;
                    break;
            }

            this.Location = new Point(x, y);
        }

        void formNotify_LocationChanged(object sender, EventArgs e)
        {
            background.Location = this.Location;
        }

        void formNotify_FormClosed(object sender, FormClosedEventArgs e)
        {
            background.Dispose();
        }

        void formNotify_VisibleChanged(object sender, EventArgs e)
        {
            var visible = this.Visible;
            background.Visible = visible;
            if (visible)
            {
                NativeMethods.SetWindowPos(this.Handle, (IntPtr)0, 0, 0, 0, 0, SetWindowPosFlags.SWP_NOACTIVATE | SetWindowPosFlags.SWP_NOMOVE | SetWindowPosFlags.SWP_NOSIZE | SetWindowPosFlags.SWP_NOOWNERZORDER);
                //this.BringToFront();
            }
        }

        void formNotify_Shown(object sender, EventArgs e)
        {
            this.Shown -= formNotify_Shown;

            if (animate)
            {
                int x2;

                switch (anchor)
                {
                    case Settings.ScreenAnchor.BottomLeft:
                    case Settings.ScreenAnchor.Left:
                    case Settings.ScreenAnchor.Top:
                    case Settings.ScreenAnchor.TopLeft:
                        x2 = this.Location.X + this.Width + 10;
                        break;
                    default:
                        x2 = this.Location.X - this.Width - 10;
                        break;
                }

                ShiftX(this.Location.X, x2, 500);
            }

            //if (notifyType == NotifyType.Downloading)
            //{
            //    Tools.BackgroundPatcher.Instance.DownloadProgress += bp_DownloadProgress;
            //    this.FormClosing += formNotify_FormClosing;

            //    DoRefresh();
            //}

            AutoClose();
        }

        private async void DoRefresh()
        {
            int files = 0;
            long total = 0;

            while (!this.IsDisposed)
            {
                if (progress != null)
                {
                    if (progress.errored)
                    {
                        break;
                    }
                    else if (progress.manifestsDownloaded == progress.manifestsTotal)
                    {
                        int ft = progress.filesTotal;
                        int fd = progress.filesDownloaded;
                        long t = progress.estimatedBytesRemaining + progress.contentBytesTotal;

                        if (files != ft || total != t)
                        {
                            files = ft;
                            total = t;

                            //labelSize.Text = string.Format(formatSize, ft, Util.Text.FormatBytes(t));
                        }

                        if (fd == ft)
                            break;
                    }
                    else
                    {
                        int ft = progress.manifestsTotal;
                        int fd = progress.manifestsDownloaded;
                        int f = fd + ft;

                        if (files != f)
                        {
                            files = f;

                            labelSize.Text = string.Format("{0} of {1}", fd, ft);
                        }
                    }
                }

                try
                {
                    await Task.Delay(500);
                }
                catch
                {
                    return;
                }
            }
        }

        protected async void ShiftX(int fromX, int toX, int duration)
        {
            DateTime start = DateTime.UtcNow;
            var pi = Math.PI / 2;

            do
            {
                await Task.Delay(10);
                if (attached)
                    return;
                var p = DateTime.UtcNow.Subtract(start).TotalMilliseconds / duration;
                if (p >= 1)
                {
                    this.Location = new Point(toX, this.Location.Y);
                    return;
                }
                else
                    this.Location = new Point(fromX + (int)((toX - fromX) * Math.Sin(pi * p)), this.Location.Y);
            }
            while (true);
        }

        protected async void FadeOut(int duration)
        {
            var start = Environment.TickCount;

            do
            {
                await Task.Delay(10);

                if (this.IsDisposed)
                    return;

                var p = (double)(Environment.TickCount - start) / duration;

                if (p >= 1)
                {
                    this.Close();
                    return;
                }
                else
                {
                    this.Opacity = 1 - p;
                    background.Opacity = BACKGROUND_OPACITY * (1 - p);
                }
            }
            while (true);
        }

        protected async void FadeIn(int duration)
        {
            var start = Environment.TickCount;

            do
            {
                await Task.Delay(10);

                var p = (double)(Environment.TickCount - start) / duration;

                if (p >= 1)
                {
                    this.Opacity = 1;
                    background.Opacity = BACKGROUND_OPACITY;
                    return;
                }
                else
                {
                    this.Opacity = p;
                    background.Opacity = BACKGROUND_OPACITY * p;
                }
            }
            while (true);
        }

        private async void AutoClose()
        {
            try
            {
                DateTime closeAt = DateTime.UtcNow.AddSeconds(10);
                do
                {
                    int t = (int)closeAt.Subtract(DateTime.UtcNow).TotalMilliseconds + 1;
                    if (t <= 0)
                        break;
                    await Task.Delay(t);
                }
                while (DateTime.UtcNow < closeAt);

                if (!this.IsDisposed)
                    FadeOut(100);
            }
            catch (Exception e)
            {
                Util.Logging.Log(e);
            }
        }
    }
}
