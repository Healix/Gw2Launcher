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

            var st = Environment.TickCount;
            var ip = await Net.IP.GetPublicAddress();

            if (Environment.TickCount - st < 500)
                await Task.Delay(500);

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

            buttonRetry.Enabled = true;
        }

        private void buttonRemember_Click(object sender, EventArgs e)
        {
            net.Remember(ip);

            this.DialogResult = System.Windows.Forms.DialogResult.OK;
        }
    }
}
