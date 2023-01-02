using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Gw2Launcher.Windows.Native;
using System.ComponentModel;

namespace Gw2Launcher.UI.Controls
{
    class IntegerTextBox : TextBox
    {
        public event EventHandler ValueChanged;

        private int _value;
        private bool cached;
        private bool _locked;

        public IntegerTextBox()
        {
            base.Text = "0";
            _Maximum = 100;
            _Increment = 1;
        }

        [DefaultValue(0)]
        public int Value
        {
            get
            {
                if (cached)
                    return _value;
                if (int.TryParse(base.Text, out _value))
                    cached = true;
                return _value;
            }
            set
            {
                if (value > _Maximum)
                    value = _Maximum;
                else if (value < _Minimum)
                    value = _Minimum;

                if (cached && _value == value)
                    return;

                cached = true;
                _value = value;

                _locked = true;
                base.Text = value.ToString();
                _locked = false;

                if (ValueChanged != null)
                    ValueChanged(this, EventArgs.Empty);
            }
        }

        private int _Minimum;
        [DefaultValue(0)]
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
        [DefaultValue(100)]
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
        [DefaultValue(1)]
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

        private bool _ReverseMouseWheelDirection;
        [DefaultValue(false)]
        public bool ReverseMouseWheelDirection
        {
            get
            {
                return _ReverseMouseWheelDirection;
            }
            set
            {
                _ReverseMouseWheelDirection = value;
            }
        }

        protected override void OnMouseWheel(MouseEventArgs e)
        {
            base.OnMouseWheel(e);

            bool b;
            if (_ReverseMouseWheelDirection)
                b = e.Delta < 0;
            else
                b = e.Delta > 0;

            if (b)
            {
                this.Value += Increment;
            }
            else
            {
                this.Value -= Increment;
            }

            if (e is HandledMouseEventArgs)
                ((HandledMouseEventArgs)e).Handled = true;
        }

        protected override void OnTextChanged(EventArgs e)
        {
            base.OnTextChanged(e);

            if (_locked)
                return;

            int v;
            if (int.TryParse(base.Text, out v))
            {
                if (cached && _value == v)
                    return;
                if (v > _Maximum)
                {
                    var i = base.SelectionStart;
                    base.Text = _Maximum.ToString();
                    base.SelectionStart = i;
                    return;
                }
                if (v < _Minimum)
                {
                    var i = base.SelectionStart;
                    base.Text = _Minimum.ToString();
                    base.SelectionStart = i;
                    return;
                }
                _value = v;
                cached = true;
                if (ValueChanged != null)
                    ValueChanged(this, EventArgs.Empty);
            }
            else
                cached = false;
        }

        protected override void OnKeyPress(System.Windows.Forms.KeyPressEventArgs e)
        {
            var c = e.KeyChar;

            if (!char.IsControl(c) && !char.IsHighSurrogate(c) && !char.IsLowSurrogate(c))
            {
                if (!char.IsDigit(c))
                {
                    if (_Minimum >= 0 || SelectionStart != 0 || c != '-')
                    {
                        e.Handled = true;
                    }
                }
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
                    else if (cached && _value == v)
                    {
                        return;
                    }
                    else
                    {
                        cached = true;
                        _value = v;
                        if (ValueChanged != null)
                            ValueChanged(this, EventArgs.Empty);
                    }
                }
                else
                {
                    Value = _Minimum;
                }
            }
            else
            {
                Value = _Minimum;
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
