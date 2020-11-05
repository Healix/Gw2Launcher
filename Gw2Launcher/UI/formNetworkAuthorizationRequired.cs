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
    partial class formNetworkAuthorizationRequired : Base.FlatBase
    {
        private Client.Launcher.NetworkAuthorizationRequiredEventsArgs auth;

        public formNetworkAuthorizationRequired(Client.Launcher.NetworkAuthorizationRequiredEventsArgs e)
        {
            InitializeComponents();

            auth = e;

            switch (e.Authentication)
            {
                case Tools.ArenaAccount.AuthenticationType.Email:
                    labelTitle.Text = "Enter your email authentication code";
                    break;
                case Tools.ArenaAccount.AuthenticationType.SMS:
                    labelTitle.Text = "Enter your SMS authentication code";
                    break;
                case Tools.ArenaAccount.AuthenticationType.TOTP:
                default:
                    labelTitle.Text = "Enter your authentication code";
                    break;
            }

            labelEmail.Text = e.Account.Email;
        }

        protected override void OnInitializeComponents()
        {
            base.OnInitializeComponents();

            InitializeComponent();
        }

        private async void buttonOK_Click(object sender, EventArgs e)
        {
            if (textCode.TextLength == 0)
                return;

            buttonOK.Enabled = false;
            waiting.Visible = true;
            textCode.Visible = false;

            try
            {
                await auth.Authenticate(textCode.Text);

                this.DialogResult = System.Windows.Forms.DialogResult.OK;
            }
            catch (Tools.ArenaAccount.AuthenticationException)
            {
                waiting.Visible = false;
                labelError.Text = "Authentication failed";
                labelError.Visible = true;
                buttonRetry.Visible = true;
                buttonOK.Visible = false;
                buttonOK.Enabled = true;
            }
            catch (Exception ex)
            {
                Util.Logging.Log(ex);

                this.DialogResult = System.Windows.Forms.DialogResult.Abort;
            }
        }

        private async void buttonRetry_Click(object sender, EventArgs e)
        {
            buttonRetry.Enabled=false;
            waiting.Visible = true;

            bool ok;

            try
            {
                ok = await auth.Retry();
            }
            catch (Exception ex)
            {
                ok = false;
                Util.Logging.Log(ex);
            }
            
            waiting.Visible = false;
            if (ok)
            {
                buttonRetry.Visible = false;
                labelError.Visible = false;
                buttonOK.Visible = true;
                textCode.Text = "";
                textCode.Visible = true;
                textCode.Focus();
            }
            else
            {
                labelError.Text = "Login failed";
            }
            buttonRetry.Enabled = true;
        }

        private void textCode_KeyDown(object sender, KeyEventArgs e)
        {
            switch (e.KeyCode)
            {
                case Keys.Enter:

                    e.Handled = true;
                    e.SuppressKeyPress = true;

                    buttonOK_Click(sender, e);

                    break;
                case Keys.Escape:

                    e.Handled = true;
                    e.SuppressKeyPress = true;

                    buttonCancel_Click(sender, e);

                    break;
            }
        }

        private void buttonCancel_Click(object sender, EventArgs e)
        {
            this.DialogResult = System.Windows.Forms.DialogResult.Cancel;
        }
    }
}
