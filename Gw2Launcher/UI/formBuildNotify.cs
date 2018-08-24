using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Gw2Launcher.UI
{
    public partial class formBuildNotify : ShowWithoutActivationForm
    {
        public enum NotifyType
        {
            PatchReady,
            DownloadingFiles,
            DownloadingManifests,
            Error
        }

        private const float BACKGROUND_OPACITY = 0.95f;

        private ShowWithoutActivationForm background;
        private bool animate, attached;
        private Tools.BackgroundPatcher.DownloadProgressEventArgs progress;
        private NotifyType notifyType;
        private Screen screen;
        private Settings.ScreenAnchor anchor;
        //private string formatSize;

        private formBuildNotify(NotifyType t, int screen, Settings.ScreenAnchor anchor, Tools.BackgroundPatcher.PatchEventArgs pe)
        {
            InitializeComponent();

            notifyType = t;
            labelBuild.Text = string.Format(labelBuild.Text, pe.Build);

            var screens = Screen.AllScreens;
            if (screens.Length <= screen)
                this.screen = Screen.PrimaryScreen;
            else
                this.screen = screens[screen];

            this.anchor = anchor;

            if (t == NotifyType.PatchReady)
            {
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
            }
            else if (t == NotifyType.DownloadingFiles)
            {
                labelTitle.Text = "Downloading";
                labelElapsedCaption.Visible = false;
                labelElapsed.Text = "(estimated)";
                labelElapsed.ForeColor = Color.DimGray;
                //formatSize = labelSize.Text;
                //labelSize.Text = "...";

                labelSize.Text = string.Format(labelSize.Text, pe.Files, Util.Text.FormatBytes(pe.Size), pe.Files != 1 ? "files" : "file");
            }
            else if (t == NotifyType.DownloadingManifests)
            {
                labelTitle.Text = "Downloading build " + labelBuild.Text;
                labelElapsed.Visible = false;
                labelElapsedCaption.Visible = false;
                labelSize.Visible = false;
                labelSizeCaption.Visible = false;
                labelBuild.Visible = false;
                labelBuildCaption.Visible = false;
                this.Height = labelTitle.Location.Y * 2 + labelTitle.Height;

                this.SizeChanged += formBuildNotify_SizeChanged;
            }
            else if (t == NotifyType.Error)
            {
                labelTitle.Text = "Failed to download update";
                labelElapsed.Visible = false;
                labelElapsedCaption.Visible = false;
                labelSize.Visible = false;
                labelSizeCaption.Visible = false;
                labelBuild.Visible = false;
                labelBuildCaption.Visible = false;
                this.Height = labelTitle.Location.Y * 2 + labelTitle.Height;

                this.SizeChanged += formBuildNotify_SizeChanged;
            }

            Initialize(t);
        }

        void formBuildNotify_SizeChanged(object sender, EventArgs e)
        {
            labelTitle.Location = new Point(labelTitle.Location.X, this.Height / 2 - labelTitle.Height / 2);
        }

        private void Initialize(NotifyType t)
        {
            background = new ShowWithoutActivationForm()
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

            this.VisibleChanged += formBuildNotify_VisibleChanged;
            this.FormClosed += formBuildNotify_FormClosed;
            this.LocationChanged += formBuildNotify_LocationChanged;
            this.Shown += formBuildNotify_Shown;
        }

        void formBuildNotify_FormClosing(object sender, FormClosingEventArgs e)
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

        private static formBuildNotify _instance;
        private static void Show(formBuildNotify f)
        {
            formBuildNotify existing = null;
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
            Show(new formBuildNotify(t, screen, anchor, pe));
        }

        public static void Show(NotifyType t, int screen, Settings.ScreenAnchor anchor, Tools.BackgroundPatcher.DownloadProgressEventArgs pe)
        {
            Show(new formBuildNotify(t, screen, anchor, new Tools.BackgroundPatcher.PatchEventArgs()
                {
                    Build = pe.build,
                    Files = pe.filesTotal + pe.manifestsTotal,
                    Size = pe.estimatedBytesRemaining + pe.contentBytesTotal,
                    Elapsed = DateTime.UtcNow.Subtract(pe.startTime)
                }));
        }

        private void formBuildNotify_Load(object sender, EventArgs e)
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

        void formBuildNotify_LocationChanged(object sender, EventArgs e)
        {
            background.Location = this.Location;
        }

        void formBuildNotify_FormClosed(object sender, FormClosedEventArgs e)
        {
            background.Dispose();
        }

        void formBuildNotify_VisibleChanged(object sender, EventArgs e)
        {
            var visible = this.Visible;
            background.Visible = visible;
            if (visible)
                this.BringToFront();
        }

        void formBuildNotify_Shown(object sender, EventArgs e)
        {
            this.Shown -= formBuildNotify_Shown;

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
            //    this.FormClosing += formBuildNotify_FormClosing;

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
            DateTime start = DateTime.UtcNow;

            do
            {
                await Task.Delay(10);

                if (this.IsDisposed)
                    return;

                var p = DateTime.UtcNow.Subtract(start).TotalMilliseconds / duration;
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
            DateTime start = DateTime.UtcNow;

            do
            {
                await Task.Delay(10);

                var p = DateTime.UtcNow.Subtract(start).TotalMilliseconds / duration;
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
