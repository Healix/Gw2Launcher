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
    public partial class formPassword : Form
    {
        public formPassword()
        {
            InitializeComponent();
            this.Disposed += formPassword_Disposed;
        }

        public formPassword(string caption) : this()
        {
            label1.Text = caption;
        }

        protected override void OnShown(EventArgs e)
        {
            base.OnShown(e);
            textPassword.Focus();
            textPassword.SelectAll();
            if (this.Password != null)
                this.Password.Dispose();
        }

        void formPassword_Disposed(object sender, EventArgs e)
        {
            if (this.Password != null)
                this.Password.Dispose();
        }

        public System.Security.SecureString Password
        {
            get;
            private set;
        }

        private void formPassword_Load(object sender, EventArgs e)
        {
            
        }

        private void buttonOK_Click(object sender, EventArgs e)
        {
            this.Password = textPassword.Password;
            this.DialogResult = DialogResult.OK;
        }

        private void textPassword_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                e.SuppressKeyPress = true;
                e.Handled = true;
                buttonOK_Click(sender, EventArgs.Empty);
            }
        }

        private void textPassword_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                e.SuppressKeyPress = true;
                e.Handled = true;
            }
        }
    }
}
