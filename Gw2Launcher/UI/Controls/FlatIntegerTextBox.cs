using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Gw2Launcher.UI.Controls
{
    class FlatIntegerTextBox : FlatTextBox
    {
        public event EventHandler ValueChanged;

        public FlatIntegerTextBox()
            : base()
        {
            ((IntegerTextBox)TextBox).ValueChanged += OnValueChanged;
        }

        protected override System.Windows.Forms.TextBox CreateTextBox()
        {
            return new IntegerTextBox()
            {
                BackColor = this.BackColor,
                ForeColor = this.ForeColor,
                BorderStyle = BorderStyle.None,
                Margin = new Padding(3)
            };
        }

        void OnValueChanged(object sender, EventArgs e)
        {
            if (ValueChanged != null)
                ValueChanged(this, e);
        }

        [DefaultValue(0)]
        public int Value
        {
            get
            {
                return ((IntegerTextBox)TextBox).Value;
            }
            set
            {
                ((IntegerTextBox)TextBox).Value = value;
            }
        }

        [DefaultValue(0)]
        public int Minimum
        {
            get
            {
                return ((IntegerTextBox)TextBox).Minimum;
            }
            set
            {
                ((IntegerTextBox)TextBox).Minimum = value;
            }
        }

        [DefaultValue(100)]
        public int Maximum
        {
            get
            {
                return ((IntegerTextBox)TextBox).Maximum;
            }
            set
            {
                ((IntegerTextBox)TextBox).Maximum = value;
            }
        }

        [DefaultValue(1)]
        public int Increment
        {
            get
            {
                return ((IntegerTextBox)TextBox).Increment;
            }
            set
            {
                ((IntegerTextBox)TextBox).Increment = value;
            }
        }

        [DefaultValue(false)]
        public bool ReverseMouseWheelDirection
        {
            get
            {
                return ((IntegerTextBox)TextBox).ReverseMouseWheelDirection;
            }
            set
            {
                ((IntegerTextBox)TextBox).ReverseMouseWheelDirection = value;
            }
        }
    }
}
