using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.Windows.Forms;
using System.Drawing;

namespace Gw2Launcher.UI.Controls
{
    class ExtendableTextBox : TextBox
    {
        const byte WM_SETFOCUS = 7;

        private formExtendedTextBox extendedWindow;
        private TextBox extendedBox;
        private IntPtr handleText;
        private bool redirect;

        protected override void OnEnter(EventArgs e)
        {
            base.OnEnter(e);
            ShowExtended(true);
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
            extendedWindow = new formExtendedTextBox(this)
            {
                MaximumSize = this.MaximumExtendedSize,
            };

            if (this.ExtendedSize.IsEmpty)
                extendedWindow.Height = this.Height * 2;
            else
                extendedWindow.Size = this.ExtendedSize;

            extendedWindow.FormClosing += extendedBox_FormClosing;
            extendedBox = extendedWindow.GetTextBox();
            handleText = extendedBox.Handle;

            EventHandler onShown = null;
            onShown = delegate
            {
                extendedWindow.Shown -= onShown;

                PaintEventHandler onPaint = null;
                onPaint = delegate
                {
                    extendedWindow.Paint -= onPaint;
                    this.Visible = false;
                };

                extendedWindow.Paint += onPaint;
            };

            extendedWindow.Shown += onShown;

            extendedWindow.Show(this.FindForm(), focus);
        }

        protected override void OnMouseEnter(EventArgs e)
        {
            base.OnMouseEnter(e);

            DelayedOnMouseEnter();
        }

        async void DelayedOnMouseEnter()
        {
            CancellationTokenSource cancel = new CancellationTokenSource();

            EventHandler onLeave = null;
            onLeave = delegate
            {
                this.MouseLeave -= onLeave;
                using (cancel)
                {
                    cancel.Cancel();
                }
            };
            this.MouseLeave += onLeave;

            try
            {
                await Task.Delay(500, cancel.Token);
            }
            catch (TaskCanceledException)
            {
                return;
            }

            this.MouseLeave -= onLeave;
            using (cancel) { }

            ShowExtended(false);
        }

        protected override void OnMouseDown(MouseEventArgs e)
        {
            base.OnMouseDown(e);
            if (extendedWindow != null)
                redirect = true;
        }

        void extendedBox_FormClosing(object sender, FormClosingEventArgs e)
        {
            var f = sender as Form;
            if (f != null)
            {
                f.FormClosing -= extendedBox_FormClosing;
                this.Visible = true;
                this.ExtendedSize = f.Size;
                redirect = false;
                extendedWindow = null;
                extendedBox = null;
            }
        }

        protected override void WndProc(ref Message m)
        {
            if (m.Msg == WM_SETFOCUS)
                return;

            base.WndProc(ref m);

            if (redirect)
            {
                m.HWnd = handleText;
                extendedWindow.CallTextBoxWndProc(ref m);
            }
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
    }
}
