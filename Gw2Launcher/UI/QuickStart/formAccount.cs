using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Gw2Launcher.UI.QuickStart
{
    public partial class formAccount : Base.FlatBase
    {
        public formAccount()
        {
            InitializeComponents();
        }

        public formAccount(string name, string email, System.Security.SecureString password)
            : this()
        {
            textAccountName.Text = name;

            if (email != null && password != null)
            {
                checkLoginAuto.Checked = true;
                textEmail.TextBox.Text = email;
                textPassword.PasswordBox.Password = password;
            }
        }

        protected override void OnInitializeComponents()
        {
            base.OnInitializeComponents();

            InitializeComponent();
        }

        public string AccountName
        {
            get;
            private set;
        }

        public string Email
        {
            get;
            private set;
        }

        public System.Security.SecureString Password
        {
            get;
            private set;
        }

        private void buttonCancel_Click(object sender, EventArgs e)
        {
            this.DialogResult = System.Windows.Forms.DialogResult.Cancel;
        }

        private void buttonOk_Click(object sender, EventArgs e)
        {
            this.AccountName = textAccountName.Text;
            if (checkLoginAuto.Checked)
            {
                this.Email = textEmail.TextBox.Text;
                this.Password = textPassword.PasswordBox.Password;
            }
            this.DialogResult = System.Windows.Forms.DialogResult.OK;
        }

        private void checkLoginAuto_CheckedChanged(object sender, EventArgs e)
        {
            tableLogin.Enabled = checkLoginAuto.Checked;
        }
    }
}
