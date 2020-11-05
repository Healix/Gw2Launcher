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
    public partial class formWaiting : Base.BaseForm
    {
        private Form parent;

        public formWaiting(Form parent)
            : this(parent, null)
        {
        }

        public formWaiting(Form parent, string message)
        {
            InitializeComponents();

            //this.Owner = parent;
            this.parent = parent;
            this.StartPosition = FormStartPosition.Manual;
            this.SizeChanged += formWaiting_SizeChanged;

            this.Location = parent.PointToScreen(new Point(0, 0));
            this.Size = parent.ClientSize;
            this.Opacity = 0.9;

            if (message != null)
                label8.Text = message;

            formWaiting_SizeChanged(null, null);

            parent.SizeChanged += parent_SizeChanged;
            parent.LocationChanged += parent_LocationChanged;
            parent.VisibleChanged += parent_VisibleChanged;
            label8.SizeChanged += label8_SizeChanged;

            this.Disposed += formWaiting_Disposed;
        }

        protected override void OnInitializeComponents()
        {
            base.OnInitializeComponents();

            InitializeComponent();
        }

        void formWaiting_Disposed(object sender, EventArgs e)
        {
            parent.SizeChanged -= parent_SizeChanged;
            parent.LocationChanged -= parent_LocationChanged;
            parent.VisibleChanged -= parent_VisibleChanged;
        }

        void label8_SizeChanged(object sender, EventArgs e)
        {
            formWaiting_SizeChanged(null, null);
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
        }

        void formWaiting_SizeChanged(object sender, EventArgs e)
        {
            label8.Location = new Point((this.Width - label8.Width) / 2, this.Height - label8.Height - Scale(20));

            var w = this.Width / 2;
            var h = Scale(12);
            var y = (this.Height - h) / 2;

            if (y + h > label8.Top)
                y = label8.Top - h;

            waitingBounce1.Bounds = new Rectangle((this.Width - w) / 2, y, w, h);
        }

        public void SetMessage(string message)
        {
            if (Util.Invoke.IfRequired(this,
                delegate
                {
                    SetMessage(message);
                }))
                return;

            label8.Text = message;
        }

        private void formWaiting_Load(object sender, EventArgs e)
        {

        }
    }
}
