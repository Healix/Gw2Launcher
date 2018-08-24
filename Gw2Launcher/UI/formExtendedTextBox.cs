using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.Windows.Forms;

namespace Gw2Launcher.UI
{
    public partial class formExtendedTextBox : Form, IMessageFilter
    {
        const short WM_NCMOUSEMOVE = 160;
        const short WM_MOUSEMOVE = 512;
        const short WM_NCMOUSELEAVE = 674;
        const short WM_MOUSELEAVE = 675;
        const short WM_NCLBUTTONDOWN = 161;
        const short WM_NCRBUTTONDOWN = 164;
        const short WM_NCMBUTTONDOWN = 167;
        const short WM_NCXBUTTONDOWN = 171;
        const short WM_LBUTTONDOWN = 513;
        const short WM_RBUTTONDOWN = 516;
        const short WM_MBUTTONDOWN = 519;
        const short WM_XBUTTONDOWN = 523;

        private class TextBoxM : TextBox
        {
            const short WM_KEYDOWN = 256;
            //const byte WM_ERASEBKGND = 20;

            //public bool disableErase;

            public void CallWndProc(ref Message m)
            {
                base.WndProc(ref m);
            }

            //protected override void WndProc(ref Message m)
            //{
            //    if (m.Msg == WM_ERASEBKGND && disableErase)
            //        return;


            //    base.WndProc(ref m);
            //}

            protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
            {
                var keyCode = (Keys)((int)msg.WParam.ToInt64() & Convert.ToInt32(Keys.KeyCode));
                if ((msg.Msg == WM_KEYDOWN && keyCode == Keys.A) && (ModifierKeys == Keys.Control) && this.Focused)
                {
                    this.SelectAll();
                    return true;
                }
                return base.ProcessCmdKey(ref msg, keyData);
            }
        }

        private Size originSize;
        private TextBox source;
        private bool messaging;
        private CancellationTokenSource cancel;

        public formExtendedTextBox(TextBox source)
        {
            InitializeComponent();

            this.source = source;
            textText.Text = source.Text;
            textText.SelectionStart = source.SelectionStart;
            textText.SelectionLength = source.SelectionLength;

            this.Size = source.Size;
            this.MinimumSize = source.Size;
            this.Location = Point.Add(source.Parent.PointToScreen(Point.Empty), new Size(source.Location.X, source.Location.Y));
        }

        protected override bool ShowWithoutActivation
        {
            get
            {
                return true;
            }
        }

        public void CallTextBoxWndProc(ref Message m)
        {
            textText.CallWndProc(ref m);
        }

        void textText_LostFocus(object sender, EventArgs e)
        {
            this.Close();
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            base.OnFormClosing(e);

            if (this.Owner != null)
                this.Owner.LocationChanged -= parent_LocationChanged;
            source.Text = textText.Text;
            source.SelectionStart = textText.SelectionStart;
            source.SelectionLength = textText.SelectionLength;

            EnableMessaging(false);
        }

        private void EnableMessaging(bool enabled)
        {
            if (messaging == enabled)
                return;
            messaging = enabled;

            if (enabled)
                Application.AddMessageFilter(this);
            else
            {
                Application.RemoveMessageFilter(this);

                if (cancel != null)
                {
                    using (cancel)
                    {
                        cancel.Cancel();
                        cancel = null;
                    }
                }
            }
        }

        protected override void OnShown(EventArgs e)
        {
            base.OnShown(e);

            textText.LostFocus += textText_LostFocus;
            textText.GotFocus += textText_GotFocus;
            textText.MouseLeave += textText_MouseLeave;

            if (!this.ContainsFocus)
                EnableMessaging(true);
        }

        void textText_MouseLeave(object sender, EventArgs e)
        {
            if (this.Owner == null || !this.ContainsFocus && !this.Owner.ContainsFocus)
                this.Close();
        }

        public void Show(Form parent, bool focus)
        {
            this.Owner = parent;
            //this.Opacity = 0;
            //this.VisibleChanged += formExtendedTextBox_VisibleChanged;
            this.Show();

            parent.LocationChanged += parent_LocationChanged;

            if (focus)
                textText.Focus();
        }

        async void formExtendedTextBox_VisibleChanged(object sender, EventArgs e)
        {
            this.VisibleChanged -= formExtendedTextBox_VisibleChanged;
            await Task.Delay(1);
            this.Opacity = 1;
        }

        void parent_LocationChanged(object sender, EventArgs e)
        {
            this.Close();
        }

        void textText_GotFocus(object sender, EventArgs e)
        {
            EnableMessaging(false);
        }

        void textText_DragSizeChanged(object sender, Size e)
        {
            this.Size = e;
        }

        public TextBox GetTextBox()
        {
            return textText;
        }

        private void buttonResize_DragOffsetChanged(object sender, Point e)
        {
            this.Size = new Size(originSize.Width + e.X, originSize.Height + e.Y);
        }

        protected override void OnSizeChanged(EventArgs e)
        {
            base.OnSizeChanged(e);
            textText.Refresh();
        }

        private void buttonResize_BeginDrag(object sender, EventArgs e)
        {
            originSize = this.Size;
            //textText.disableErase = true;
        }

        private void buttonResize_EndDrag(object sender, EventArgs e)
        {
            //textText.disableErase = false;
            //textText.Invalidate();
        }

        async void DelayedMouseLeave()
        {
            cancel = new CancellationTokenSource();
            byte padding = 10;

            do
            {
                try
                {
                    await Task.Delay(500, cancel.Token);
                }
                catch (TaskCanceledException)
                {
                    return;
                }

                var b = this.DesktopBounds;
                var p = Cursor.Position;
                if (p.X < b.X - padding || p.X > b.Right + padding || p.Y < b.Y - padding || p.Y > b.Bottom + padding)
                    break;
            }
            while (true);

            using (cancel) 
            {
                cancel = null;
            }
            this.Close();
        }

        public bool PreFilterMessage(ref Message m)
        {
            switch (m.Msg)
            {
                case WM_NCLBUTTONDOWN:
                case WM_NCRBUTTONDOWN:
                case WM_NCMBUTTONDOWN:
                case WM_NCXBUTTONDOWN:
                case WM_LBUTTONDOWN:
                case WM_RBUTTONDOWN:
                case WM_MBUTTONDOWN:
                case WM_XBUTTONDOWN:

                    if (!this.DesktopBounds.Contains(Cursor.Position))
                        this.Close();

                    break;
                case WM_MOUSEMOVE:
                case WM_NCMOUSEMOVE:

                    if (cancel != null && this.DesktopBounds.Contains(Cursor.Position))
                    {
                        using (cancel)
                        {
                            cancel.Cancel();
                            cancel = null;
                        }
                    }

                    break;
                case WM_MOUSELEAVE:
                case WM_NCMOUSELEAVE:

                    if (!this.DesktopBounds.Contains(Cursor.Position) && cancel == null)
                        DelayedMouseLeave();

                    break;
            }

            return false;
        }
    }
}
