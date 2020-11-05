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
    public partial class formScreenPosition : Base.BaseForm
    {
        public event EventHandler<ScreenPositionChangedEventArgs> ScreenPositionChanged;

        public class ScreenPositionChangedEventArgs : EventArgs
        {
            public ScreenPositionChangedEventArgs(int screen, Settings.ScreenAnchor anchor)
            {
                this.Screen = screen;
                this.Anchor = anchor;
            }

            public int Screen
            {
                get;
                private set;
            }

            public Settings.ScreenAnchor Anchor
            {
                get;
                private set;
            }
        }

        public enum NotificationType
        {
            Patch,
            Note,
            Screenshot
        }

        private ushort focus;
        private Screen[] screens;
        private int currentScreen;
        private Settings.ScreenAnchor currentAnchor;
        private Tools.BackgroundPatcher.DownloadProgressEventArgs pe;
        private bool sample;
        private NotificationType type;
        private Image image;

        public formScreenPosition(NotificationType type, int screen, Settings.ScreenAnchor anchor)
        {
            InitializeComponents();

            this.type = type;

            switch (type)
            {
                case NotificationType.Patch:

                    pe = new Tools.BackgroundPatcher.DownloadProgressEventArgs()
                    {
                        build = 12345,
                        filesTotal = 123,
                        startTime = DateTime.UtcNow,
                        contentBytesTotal = 12345
                    };

                    break;
                case NotificationType.Note:
                    break;
                case NotificationType.Screenshot:

                    break;
            }

            screens = Screen.AllScreens;
            currentScreen = screen;

            if (currentScreen >= screens.Length)
                currentScreen = 0;

            if (screens.Length > 1)
            {
                arrowLeft.Visible = true;
                arrowRight.Visible = true;
            }

            labelScreen.Text = (currentScreen + 1).ToString();

            radioBL.Tag = Settings.ScreenAnchor.BottomLeft;
            radioBR.Tag = Settings.ScreenAnchor.BottomRight;
            radioTL.Tag = Settings.ScreenAnchor.TopLeft;
            radioTR.Tag = Settings.ScreenAnchor.TopRight;

            radioBR.Checked = true;
            currentAnchor = Settings.ScreenAnchor.BottomRight;

            foreach (Control c in this.Controls)
            {
                c.LostFocus += c_LostFocus;
                if (c is RadioButton)
                {
                    var r = (RadioButton)c;
                    if (r.Tag != null && (Settings.ScreenAnchor)r.Tag == anchor)
                    {
                        r.Checked = true;
                        currentAnchor = anchor;
                    }
                    r.CheckedChanged += radio_CheckedChanged;
                }
            }
        }

        protected override void OnInitializeComponents()
        {
            base.OnInitializeComponents();

            InitializeComponent();
        }

        public int SelectedScreen
        {
            get
            {
                return currentScreen;
            }
        }

        public Settings.ScreenAnchor SelectedAnchor
        {
            get
            {
                return currentAnchor;
            }
        }

        protected override void OnShown(EventArgs e)
        {
            base.OnShown(e);
            this.Focus();

            radioBL.Click += radio_Click;
            radioTL.Click += radio_Click;
            radioBR.Click += radio_Click;
            radioTR.Click += radio_Click;
        }

        void radio_Click(object sender, EventArgs e)
        {
            ShowSample();
        }

        void radio_CheckedChanged(object sender, EventArgs e)
        {
            var r = (RadioButton)sender;
            if (r.Checked)
            {
                currentAnchor = (Settings.ScreenAnchor)r.Tag;
                OnScreenPositionChanged();
            }
        }

        async void OnLostFocus()
        {
            var f = ++focus;
            await Task.Delay(100);
            if (f == focus && !this.ContainsFocus)
                this.Close();
        }

        void c_LostFocus(object sender, EventArgs e)
        {
            if (!this.ContainsFocus)
                OnLostFocus();
        }

        private void formScreenPosition_Load(object sender, EventArgs e)
        {

        }

        private async void ShowScreenshot()
        {
            try
            {
                var image = this.image;
                var screen = Screen.AllScreens[currentScreen].Bounds;

                if (this.image == null || this.image.Size != screen.Size)
                {
                    image = await Task.Run<Image>(new Func<Image>(
                        delegate
                        {
                            var i = new Bitmap(screen.Width, screen.Height);
                            using (var g = Graphics.FromImage(i))
                            {
                                g.CopyFromScreen(0, 0, 0, 0, i.Size);
                            }
                            return i;
                        }));

                    if (this.image != null)
                    {
                        using (this.image) { }
                    }
                }
                this.image = image;
            }
            catch { }

            formNotify.ShowImage(currentScreen, currentAnchor, "", this.image, false);
        }

        private void ShowSample()
        {
            switch (type)
            {
                case NotificationType.Patch:

                    if (sample = !sample)
                        formNotify.Show(formNotify.NotifyType.DownloadingManifests, currentScreen, currentAnchor, pe);
                    else
                        formNotify.Show(formNotify.NotifyType.PatchReady, currentScreen, currentAnchor, pe);

                    break;
                case NotificationType.Note:

                    string message;
                    if (sample = !sample)
                        message = "Sample message shown when a note expires";
                    else
                        message = "Sample message...\n\n1\n2\n3";
                    formNotify.ShowNote(currentScreen, currentAnchor, message, "Example");

                    break;
                case NotificationType.Screenshot:

                    ShowScreenshot();

                    break;
            }

            this.Focus();
        }

        private void OnScreenChanged()
        {
            labelScreen.Text = (currentScreen + 1).ToString();
            ShowSample();
            OnScreenPositionChanged();
        }

        private void OnScreenPositionChanged()
        {
            if (ScreenPositionChanged != null)
                ScreenPositionChanged(this, new ScreenPositionChangedEventArgs(currentScreen, currentAnchor));
        }

        private void arrowRight_Click(object sender, EventArgs e)
        {
            if (--currentScreen < 0)
                currentScreen = screens.Length - 1;
            OnScreenChanged();
        }

        private void arrowLeft_Click(object sender, EventArgs e)
        {
            if (++currentScreen >= screens.Length)
                currentScreen = 0;
            OnScreenChanged();
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (components != null)
                    components.Dispose();
            }

            base.Dispose(disposing);

            if (disposing)
            {
                using (this.image) { }
                ScreenPositionChanged = null;
            }
        }
    }
}
