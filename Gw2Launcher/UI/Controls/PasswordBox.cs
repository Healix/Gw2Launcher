using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Security;
using System.Windows.Forms;
using Gw2Launcher.Windows.Native;

namespace Gw2Launcher.UI.Controls
{
    public partial class PasswordBox : TextBox
    {
        private SecureString _password;

        public PasswordBox()
            : base()
        {
            _password = new SecureString();
            this.PasswordChar = '*';
        }

        public SecureString Password
        {
            get
            {
                var p = _password.Copy();
                p.MakeReadOnly();
                return p;
            }
            set
            {
                if (_password != null)
                    _password.Dispose();
                _password = value.Copy();
                base.Text = new string(this.PasswordChar, _password.Length);
            }
        }

        protected override void WndProc(ref Message m)
        {
            if (m.Msg == (int)WindowMessages.WM_PASTE)
            {
                if (Clipboard.ContainsText())
                {
                    if (base.Multiline)
                    {
                        this.SelectedText = Clipboard.GetText();
                    }
                    else
                    {
                        string text = Clipboard.GetText();
                        int i = text.IndexOfAny(new char[] { '\r', '\n' });
                        if (i == -1)
                            this.SelectedText = text;
                        else if (i > 0)
                            this.SelectedText = text.Substring(0, i);
                    }
                }
                else
                    this.SelectedText = "";
                return;
            }
            base.WndProc(ref m);
        }

        protected override void OnKeyDown(KeyEventArgs e)
        {
            switch (e.KeyCode)
            {
                case Keys.Delete:
                case Keys.Back:

                    int length = this.TextLength;
                    if (length == 0)
                        return;

                    int selLength = this.SelectionLength;
                    if (selLength == length)
                    {
                        _password.Clear();
                        return;
                    }

                    int selStart = this.SelectionStart;
                    if (selLength > 0)
                    {
                        for (int i = selStart + selLength - 1; i >= selStart; i--)
                            _password.RemoveAt(i);
                    }
                    else
                    {
                        if (e.KeyCode == Keys.Delete)
                        {
                            if (selStart < length)
                                _password.RemoveAt(selStart);
                        }
                        else
                        {
                            if (selStart > 0)
                                _password.RemoveAt(selStart - 1);
                        }
                    }

                    break;
            }

            base.OnKeyDown(e);
        }

        protected override void OnKeyUp(KeyEventArgs e)
        {
            base.OnKeyUp(e);
        }

        protected override void OnKeyPress(KeyPressEventArgs e)
        {
            char charCode = e.KeyChar;

            if (!char.IsControl(charCode) && !char.IsHighSurrogate(charCode) && !char.IsLowSurrogate(charCode))
            {
                var selStart = this.SelectionStart;
                var selLength = this.SelectionLength;
                var length = this.TextLength;

                if (selLength == 0)
                {
                    if (selStart == length)
                        _password.AppendChar(charCode);
                    else
                        _password.InsertAt(selStart, charCode);
                }
                else if (selLength == length)
                {
                    _password.Clear();
                    _password.AppendChar(charCode);
                }
                else
                {
                    for (int i = selStart + selLength - 1; i >= selStart; i--)
                        _password.RemoveAt(i);
                    _password.InsertAt(selStart, charCode);
                }

                e.KeyChar = this.PasswordChar;
            }

            base.OnKeyPress(e);
        }

        public override string SelectedText
        {
            set
            {
                var selStart = this.SelectionStart;
                var selLength = this.SelectionLength;
                var length = this.TextLength;

                if (selLength == length)
                {
                    _password.Clear();
                }
                else if (selLength > 0)
                {
                    for (int i = selStart + selLength - 1; i >= selStart; i--)
                        _password.RemoveAt(i);
                }

                if (value != null)
                {
                    if (selStart + selLength == length)
                    {
                        foreach (char c in value)
                            _password.AppendChar(c);
                    }
                    else
                    {
                        foreach (char c in value)
                            _password.InsertAt(selStart++, c);
                    }
                }

                base.SelectedText = value;
            }
        }

        public override string Text
        {
            set
            {
                _password.Clear();
                if (value != null)
                {
                    foreach (char c in value)
                        _password.AppendChar(c);
                }

                base.Text = value;
            }
        }

        public override void ResetText()
        {
            base.ResetText();
            _password.Clear();
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            if (disposing)
                _password.Dispose();
        }
    }
}
