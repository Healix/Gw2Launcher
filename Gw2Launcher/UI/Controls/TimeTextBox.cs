using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Gw2Launcher.UI.Controls
{
    class TimeTextBox : TextBox
    {
        private bool isEmpty;
        private DateTime previous;

        public TimeTextBox()
        {
            IsEmpty = true;
        }

        protected override void OnCreateControl()
        {
            base.OnCreateControl();

            if (base.TextLength == 0)
                base.Text = previous.ToString(_CustomFormat);
        }

        protected override void OnLostFocus(EventArgs e)
        {
            DateTime d;
            if (DateTime.TryParse(this.Text, out d))
            {
                previous = d;
                this.Text = d.ToString(_CustomFormat);
            }
            else
            {
                IsEmpty = previous == DateTime.MinValue;
                base.Text = previous.ToString(_CustomFormat);
            }

            base.OnLostFocus(e);
        }

        protected override void OnEnter(EventArgs e)
        {
            IsEmpty = false;
            base.OnEnter(e);
        }

        private bool IsEmpty
        {
            get
            {
                return isEmpty;
            }
            set
            {
                if (isEmpty != value)
                {
                    if (!value)
                        base.Text = "";
                    isEmpty = value;
                    //this.ForeColor = value ? SystemColors.GrayText : SystemColors.ControlText;
                }
            }
        }

        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public override string Text
        {
            get
            {
                return base.Text;
            }
            set
            {
                IsEmpty = string.IsNullOrEmpty(value);
                base.Text = value;
            }
        }

        private string _CustomFormat = "H:mm";
        [DefaultValue("H:mm")]
        public string CustomFormat
        {
            get
            {
                return _CustomFormat;
            }
            set
            {
                if (_CustomFormat != value)
                {
                    _CustomFormat = value;
                    base.Text = previous.ToString(_CustomFormat);
                }
            }
        }

        [System.ComponentModel.Browsable(false)]
        [System.ComponentModel.DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public DateTime Value
        {
            get
            {
                DateTime d;
                DateTime.TryParse(this.Text, out d);
                return d;
            }
            set
            {
                previous = value;
                base.Text = previous.ToString(_CustomFormat);
            }
        }
    }
}
