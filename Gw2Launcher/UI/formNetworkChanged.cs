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
    partial class formNetworkChanged : Base.FlatBase
    {
        private Client.Launcher.NetworkChangedEventArgs net;
        private System.Net.IPAddress ip;
        private byte retries;

        public formNetworkChanged(Client.Launcher.NetworkChangedEventArgs e)
        {
            net = e;
            ip = e.Address;

            InitializeComponents();
        }

        protected override void OnInitializeComponents()
        {
            base.OnInitializeComponents();

            InitializeComponent();

            Show(ip, false);
        }

        private void Show(System.Net.IPAddress ip, bool remembered)
        {
            var b = ip == null;

            if (!b)
            {
                this.Text = ip.ToString();
                labelMessage.Text = remembered ? "OK" : "The current network has not been remembered";
            }

            panelButtons.SuspendLayout();

            buttonRemember.Visible = !b;
            buttonRetry.Visible = b;

            panelButtons.ResumeLayout();
        }

        protected override void OnShown(EventArgs e)
        {
            base.OnShown(e);

            DelayedEnable();
        }

        private async void DelayedEnable()
        {
            await Task.Delay(500);

            panelButtons.Enabled = true;

            if (buttonRetry.Visible)
            {
                AutoRetry();
            }
        }

        private async void AutoRetry()
        {
            if (retries > 20)
                return;
            retries++;

            if (buttonRetry.Tag == null)
            {
                buttonRetry.Tag = buttonRetry.Text;
            }

            var abort = false;

            EventHandler onEvent = delegate
            {
                abort = true;
            };

            MouseEventHandler onDown = delegate
            {
                abort = true;
            };

            FormClosingEventHandler onClosing = delegate
            {
                abort = true;
            };

            foreach (Control c in panelButtons.Controls)
            {
                c.Click += onEvent;
                c.MouseDown += onDown;
            }

            buttonRetry.VisibleChanged += onEvent;
            this.FormClosing += onClosing;

            var t = Environment.TickCount + 30000;
            var delay = 500;

            while (true)
            {
                await Task.Delay(delay);

                if (abort)
                {
                    break;
                }

                var ms = t - Environment.TickCount;
                var s = ms / 1000;

                if (s > 0)
                {
                    buttonRetry.Text = (string)buttonRetry.Tag + " (" + s + ")";
                    delay = ms % 1000 + 500;
                }
                else
                {
                    break;
                }
            }

            buttonRetry.Text = (string)buttonRetry.Tag;

            foreach (Control c in panelButtons.Controls)
            {
                c.Click -= onEvent;
                c.MouseDown -= onDown;
            }

            buttonRetry.VisibleChanged -= onEvent;
            this.FormClosing -= onClosing;

            if (!abort)
            {
                buttonRetry_Click(null, null);
            }
        }

        private void buttonIgnore_Click(object sender, EventArgs e)
        {
            this.DialogResult = System.Windows.Forms.DialogResult.OK;
        }

        private void buttonAbort_Click(object sender, EventArgs e)
        {
            this.DialogResult = System.Windows.Forms.DialogResult.Abort;
        }

        private async void buttonRetry_Click(object sender, EventArgs e)
        {
            buttonRetry.Enabled = false;

            stackPanel1.SuspendLayout();

            waitingBounce.Margin = new System.Windows.Forms.Padding(0, 0, 0, labelMessage.Height + labelMessage.Margin.Bottom - waitingBounce.Height);
            labelMessage.Visible = false;
            waitingBounce.Visible = true;

            stackPanel1.ResumeLayout();

            var st = Environment.TickCount;
            var ip = await Net.IP.GetPublicAddress();

            if (Environment.TickCount - st < 500)
                await Task.Delay(500);

            if (!this.Visible)
                return;

            stackPanel1.SuspendLayout();

            labelMessage.Visible = true;
            waitingBounce.Visible = false;

            stackPanel1.ResumeLayout();

            if (ip != null)
            {
                this.ip = ip;

                if (Net.IP.IsMatch(ip, Settings.IPAddresses.Value))
                {
                    panelButtons.Enabled = false;
                    Show(ip, true);

                    await Task.Delay(500);

                    this.DialogResult = System.Windows.Forms.DialogResult.OK;

                    return;
                }

                Show(ip, false);
            }
            else
            {
                AutoRetry();
            }

            buttonRetry.Enabled = true;
        }

        private void buttonRemember_Click(object sender, EventArgs e)
        {
            net.Remember(ip);

            this.DialogResult = System.Windows.Forms.DialogResult.OK;
        }
    }
}
