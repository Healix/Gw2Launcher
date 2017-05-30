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
    public partial class formWaiting : Form
    {
        private Rectangle circleRect;
        private DateTime startTime;
        private Form parent;

        public formWaiting(Form parent)
            : this(parent, null)
        {
        }

        public formWaiting(Form parent, string message)
        {
            InitializeComponent();

            //this.Owner = parent;
            this.parent = parent;
            this.startTime = DateTime.UtcNow;
            this.DoubleBuffered = true;
            this.StartPosition = FormStartPosition.Manual;
            this.SizeChanged += formWaiting_SizeChanged;

            this.Location = parent.PointToScreen(new Point(0, 0));
            this.Size = parent.ClientSize;
            this.Opacity = 0.9;

            if (message != null)
                label8.Text = message;

            formWaiting_SizeChanged(null, EventArgs.Empty);

            parent.SizeChanged += parent_SizeChanged;
            parent.LocationChanged += parent_LocationChanged;
            parent.VisibleChanged += parent_VisibleChanged;
            label8.SizeChanged += label8_SizeChanged;
        }

        void label8_SizeChanged(object sender, EventArgs e)
        {
            label8.Location = new Point(this.Width / 2 - label8.Width / 2, this.Height - label8.Height - 20);
        }

        protected override bool ShowWithoutActivation
        {
            get
            {
                return true;
            }
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            foreach (var f in this.OwnedForms)
            {
                this.RemoveOwnedForm(f);
                f.Owner = this.Owner;
            }

            base.OnClosing(e);
        }

        void parent_VisibleChanged(object sender, EventArgs e)
        {
            this.Visible = parent.Visible;
        }

        void parent_LocationChanged(object sender, EventArgs e)
        {
            this.Location = parent.PointToScreen(new Point(0, 0));
        }

        void parent_SizeChanged(object sender, EventArgs e)
        {
            this.Size = parent.ClientSize;
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            base.OnFormClosing(e);

            parent.SizeChanged -= parent_SizeChanged;
            parent.LocationChanged -= parent_LocationChanged;
            parent.VisibleChanged -= parent_VisibleChanged;
        }

        void formWaiting_SizeChanged(object sender, EventArgs e)
        {
            label8.Location = new Point(this.Width / 2 - label8.Width / 2, this.Height - label8.Height - 20);

            int w = this.Width / 2;
            if (!circleRect.IsEmpty)
                this.Invalidate(circleRect);
            circleRect = new Rectangle(this.Width / 2 - w / 2, this.Height / 2 - 10, w, 10);
        }

        protected override void OnShown(EventArgs e)
        {
            Animate();

            base.OnShown(e);
        }

        public void SetMessage(string message)
        {
            if (this.InvokeRequired)
            {
                this.Invoke(new MethodInvoker(
                    delegate
                    {
                        SetMessage(message);
                    }));
                return;
            }

            label8.Text = message;
        }

        private void formWaiting_Load(object sender, EventArgs e)
        {

        }

        private async void Animate()
        {
            while (!this.IsDisposed)
            {
                this.Invalidate(circleRect);
                await Task.Delay(50);
            }
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);

            float ms = (float)DateTime.UtcNow.Subtract(startTime).TotalMilliseconds;
            int x = circleRect.X + (int)((circleRect.Width - 8) * (Math.Sin((ms / 500) % 360) + 1) / 2);

            e.Graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
            e.Graphics.FillEllipse(Brushes.White, x, circleRect.Y, 8, 8);
        }
    }
}
