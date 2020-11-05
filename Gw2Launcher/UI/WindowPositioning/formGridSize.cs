using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Gw2Launcher.UI.WindowPositioning
{
    public partial class formGridSize : Base.FlatBase
    {
        public formGridSize(Size size)
        {
            InitializeComponent();

            textWidth.Value = size.Width;
            textHeight.Value = size.Height;
        }

        public Size Value
        {
            get
            {
                return new Size(textWidth.Value, textHeight.Value);
            }
        }

        private void buttonCancel_Click(object sender, EventArgs e)
        {
            this.DialogResult = System.Windows.Forms.DialogResult.Cancel;
        }

        private void buttonOk_Click(object sender, EventArgs e)
        {
            this.DialogResult = System.Windows.Forms.DialogResult.OK;
        }

        private void text_KeyDown(object sender, KeyEventArgs e)
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
