using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.Windows.Forms;
using System.Drawing;
using Gw2Launcher.Windows.Native;

namespace Gw2Launcher.UI.Controls
{
    class ExtendableTextBox : TextBox
    {
        private formExtendedTextBox extendedWindow;
        private TextBox extendedBox;
        private IntPtr handleText;

        protected override void OnEnter(EventArgs e)
        {
            base.OnEnter(e);

            if (!this.Focused)
            {
                this.Focus();
            }
            else
            {
                ShowExtended(true);
            }
        }

        public Size MaximumExtendedSize
        {
            get;
            set;
        }

        public Size ExtendedSize
        {
            get;
            set;
        }

        private void ShowExtended(bool focus)
        {
            if (extendedBox != null)
                return;

            if (extendedWindow == null || extendedWindow.IsDisposed)
            {
                extendedWindow = new formExtendedTextBox(this);
                extendedWindow.ExtensionOpened += extendedWindow_Opened;
                extendedWindow.ExtensionClosing += extendedWindow_Closing;
            }

            extendedWindow.MaximumSize = this.MaximumExtendedSize;
            extendedWindow.MinimumSize = this.Size;

            if (this.ExtendedSize.IsEmpty)
                extendedWindow.Height = this.Height * 2;
            else
                extendedWindow.Size = this.ExtendedSize;
            
            extendedWindow.Show(this.FindForm(), focus);
        }

        void extendedWindow_Closing(object sender, EventArgs e)
        {
            var f = sender as Form;
            if (f != null)
            {
                this.TabStop = true;
                this.ExtendedSize = f.Size;

                var p = this.Parent;
                while (p != null && p.Parent is Panel)
                {
                    p = p.Parent;
                }
                p.Focus();

                extendedBox = null;
                handleText = IntPtr.Zero;
            }
        }

        void extendedWindow_Opened(object sender, EventArgs e)
        {
            this.TabStop = false;

            extendedBox = extendedWindow.GetTextBox();
            handleText = extendedBox.Handle;
        }

        protected override void OnMouseHover(EventArgs e)
        {
            base.OnMouseHover(e);

            ShowExtended(false);
        }

        protected override void WndProc(ref Message m)
        {
            switch ((WindowMessages)m.Msg)
            {
                case WindowMessages.WM_SETFOCUS:

                    base.WndProc(ref m);
                    ShowExtended(true);

                    return;
                case WindowMessages.WM_LBUTTONDOWN:
                case WindowMessages.WM_RBUTTONDOWN:

                    ShowExtended(false);

                    if (handleText != IntPtr.Zero)
                    {
                        m.HWnd = handleText;
                        extendedWindow.CallTextBoxWndProc(ref m);
                    }

                    return;
            }

            base.WndProc(ref m);
        }

        public override int TextLength
        {
            get
            {
                if (extendedBox != null)
                    return extendedBox.TextLength;
                return base.TextLength;
            }
        }

        public override string Text
        {
            get
            {
                if (extendedBox != null)
                    return extendedBox.Text;
                return base.Text;
            }
            set
            {
                if (extendedBox != null)
                    extendedBox.Text = value;
                else
                    base.Text = value;
            }
        }

        public override string SelectedText
        {
            get
            {
                if (extendedBox != null)
                    return extendedBox.SelectedText;
                return base.SelectedText;
            }
            set
            {
                if (extendedBox != null)
                    extendedBox.SelectedText = value;
                else
                    base.SelectedText = value;
            }
        }

        public override int SelectionLength
        {
            get
            {
                if (extendedBox != null)
                    return extendedBox.SelectionLength;
                return base.SelectionLength;
            }
            set
            {
                if (extendedBox != null)
                    extendedBox.SelectionLength = value;
                else
                    base.SelectionLength = value;
            }
        }

        public new int SelectionStart
        {
            get
            {
                if (extendedBox != null)
                    return extendedBox.SelectionStart;
                return base.SelectionStart;
            }
            set
            {
                if (extendedBox != null)
                    extendedBox.SelectionStart = value;
                else
                    base.SelectionStart = value;
            }
        }

        protected new void Select(int start, int length)
        {
            if (extendedBox != null)
                extendedBox.Select(start, length);
            else
                base.Select(start, length);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (extendedWindow != null)
                    extendedWindow.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
