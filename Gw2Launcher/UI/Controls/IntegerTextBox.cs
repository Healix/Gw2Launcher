using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Gw2Launcher.Windows.Native;

namespace Gw2Launcher.UI.Controls
{
    class IntegerTextBox : TextBox
    {
        public IntegerTextBox()
        {
            base.Text = "0";
            _Maximum = 100;
            _Increment = 1;
        }

        public int Value
        {
            get
            {
                int v;
                int.TryParse(base.Text,out v);
                return v;
            }
            set
            {
                if (value > _Maximum)
                    value = _Maximum;
                else if (value < _Minimum)
                    value = _Minimum;
                base.Text = value.ToString();
            }
        }

        private int _Minimum;
        public int Minimum
        {
            get
            {
                return _Minimum;
            }
            set
            {
                if (_Minimum != value)
                {
                    _Minimum = value;
                    if (this.Value < value)
                        this.Value = value;
                }
            }
        }

        private int _Maximum;
        public int Maximum
        {
            get
            {
                return _Maximum;
            }
            set
            {
                if (_Maximum != value)
                {
                    _Maximum = value;
                    if (this.Value > value)
                        this.Value = value;
                }
            }
        }

        private int _Increment;
        public int Increment
        {
            get
            {
                return _Increment;
            }
            set
            {
                _Increment = value;
            }
        }

        protected override void OnMouseWheel(MouseEventArgs e)
        {
            base.OnMouseWheel(e);

            if (e.Delta > 0)
            {
                this.Value += Increment;
            }
            else if (e.Delta < 0)
            {
                this.Value -= Increment;
            }

            if (e is HandledMouseEventArgs)
                ((HandledMouseEventArgs)e).Handled = true;
        }

        protected override void OnKeyPress(System.Windows.Forms.KeyPressEventArgs e)
        {
            var c = e.KeyChar;

            if (!char.IsControl(c) && !char.IsHighSurrogate(c) && !char.IsLowSurrogate(c))
            {
                if (!char.IsDigit(e.KeyChar))
                    e.Handled = true;
            }

            base.OnKeyPress(e);
        }

        protected override void OnLeave(EventArgs e)
        {
            base.OnLeave(e);

            if (base.TextLength > 0)
            {
                int v;
                if (int.TryParse(base.Text, out v))
                {
                    if (v > _Maximum)
                    {
                        Value = _Maximum;
                    }
                    else if (v < _Minimum)
                    {
                        Value = _Minimum;
                    }
                }
                else
                {
                    Value = 0;
                }
            }
            else
            {
                Value = 0;
            }
        }

        protected override void WndProc(ref Message m)
        {
            if (m.Msg == (int)WindowMessages.WM_PASTE)
            {
                if (Clipboard.ContainsText())
                {
                    int v;
                    if (int.TryParse(Clipboard.GetText(), out v))
                    {
                        this.SelectedText = v.ToString();
                    }
                }
                return;
            }
            base.WndProc(ref m);
        }
    }
}
