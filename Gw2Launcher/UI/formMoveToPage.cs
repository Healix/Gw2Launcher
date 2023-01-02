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
    public partial class formMoveToPage : Base.FlatBase
    {
        private byte pages;

        public formMoveToPage(byte page, byte pages)
        {
            InitializeComponents();

            this.pages = pages;

            checkRemoveCurrent.Enabled = page > 0;
            textPage.Value = page;

            textPage.TextBox.KeyDown += textPage_KeyDown;
        }

        protected override void OnInitializeComponents()
        {
            base.OnInitializeComponents();

            InitializeComponent();
        }

        public byte Page
        {
            get;
            private set;
        }

        public bool RemoveCurrent
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
            this.Page = (byte)textPage.Value;
            this.RemoveCurrent = checkRemoveCurrent.Checked;

            this.DialogResult = System.Windows.Forms.DialogResult.OK;
        }

        private void arrowFirst_Click(object sender, EventArgs e)
        {
            textPage.Value = 0;
        }

        private void arrowPrevious_Click(object sender, EventArgs e)
        {
            --textPage.Value;
        }

        private void arrowNext_Click(object sender, EventArgs e)
        {
            ++textPage.Value;
        }

        private void arrowLast_Click(object sender, EventArgs e)
        {
            textPage.Value = textPage.Value == pages ? textPage.Maximum : pages;
        }

        private void textPage_KeyDown(object sender, KeyEventArgs e)
        {
            switch (e.KeyCode)
            {
                case Keys.Enter:

                    e.Handled = true;
                    e.SuppressKeyPress = true;

                    buttonOk_Click(sender, e);

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
