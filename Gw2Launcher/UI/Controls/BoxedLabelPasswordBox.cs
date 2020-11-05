using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Drawing;
using System.ComponentModel;

namespace Gw2Launcher.UI.Controls
{
    class BoxedLabelPasswordBox : BoxedLabelTextBox
    {
        protected override TextBox CreateTextBox()
        {
            return new PasswordBox()
            {
                UseSystemPasswordChar = true,
            };
        }

        public PasswordBox PasswordBox
        {
            get
            {
                return (PasswordBox)textbox;
            }
        }
    }
}
