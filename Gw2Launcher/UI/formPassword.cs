using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Security;

namespace Gw2Launcher.UI
{
    public partial class formPassword : Base.FlatBase
    {
        private SecureString existing;

        private formPassword()
        {
            InitializeComponents();
            textPassword.Select();
        }

        /// <summary>
        /// For password verification
        /// </summary>
        public formPassword(string username, SecureString password)
            : this()
        {
            textUsername.Text = username;
            existing = password;
        }

        /// <summary>
        /// For password entry
        /// </summary>
        public formPassword(string username)
            : this(username, null)
        {
        }

        protected override void OnInitializeComponents()
        {
            base.OnInitializeComponents();

            InitializeComponent();
        }

        private void buttonCancel_Click(object sender, EventArgs e)
        {
            this.DialogResult = System.Windows.Forms.DialogResult.Cancel;
        }

        public SecureString Password
        {
            get;
            private set;
        }

        private void buttonOK_Click(object sender, EventArgs e)
        {
            if (existing == null)
            {
                this.Password = textPassword.Password;
                this.DialogResult = System.Windows.Forms.DialogResult.OK;
            }
            else
            {
                using (var p = textPassword.Password)
                {
                    if (Gw2Launcher.Security.Credentials.Compare(existing, p))
                    {
                        this.DialogResult = System.Windows.Forms.DialogResult.OK;
                    }
                    else
                    {
                        MessageBox.Show(this, "Unable to verify password", "Failed", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        textPassword.Select();
                    }
                }
            }
        }

        private void textPassword_KeyDown(object sender, KeyEventArgs e)
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
    }
}
