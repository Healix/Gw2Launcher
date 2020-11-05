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
using System.Runtime.InteropServices;
using Gw2Launcher.Windows.Native;

namespace Gw2Launcher.UI
{
    public partial class formExtendedTextBox : Form, IMessageFilter
    {
        private class TextBoxM : TextBox
        {
            public void CallWndProc(ref Message m)
            {
                base.WndProc(ref m);
            }

            protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
            {
                var keyCode = (Keys)msg.WParam.GetValue() & Keys.KeyCode;
                if ((msg.Msg == (int)WindowMessages.WM_KEYDOWN && keyCode == Keys.A) && (ModifierKeys == Keys.Control) && this.Focused)
                {
                    this.SelectAll();
                    return true;
                }
                return base.ProcessCmdKey(ref msg, keyData);
            }

            protected override void WndProc(ref Message m)
            {
                base.WndProc(ref m);

                switch ((Windows.Native.WindowMessages)m.Msg)
                {
                    case Windows.Native.WindowMessages.WM_NCHITTEST:

                        switch ((HitTest)m.Result.GetValue())
                        {
                            case HitTest.GrowBox:

                                m.Result = (IntPtr)HitTest.Transparent;

                                break;
                        }

                        break;
                }
            }
        }

        public event EventHandler ExtensionOpened;
        public event EventHandler ExtensionClosing;

        private TextBox source;
        private bool messaging;
        private CancellationTokenSource cancel;

        public formExtendedTextBox(TextBox source)
        {
            InitializeComponent();

            this.source = source;

            buttonResize.Location = new Point(textText.Right - buttonResize.Width - 1, textText.Bottom - buttonResize.Height - 1);
            buttonResize.Enabled = false;

            this.Size = source.Size;
            this.MinimumSize = source.Size;
        }

        protected override CreateParams CreateParams
        {
            get
            {
                CreateParams createParams = base.CreateParams;
                createParams.ExStyle |= (int)(WindowStyle.WS_EX_TOOLWINDOW | WindowStyle.WS_EX_COMPOSITED);
                return createParams;
            }
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

        private void OnHide()
        {
            if (!this.Visible)
                return;

            EnableMessaging(false);

            textText.LostFocus -= textText_LostFocus;
            textText.GotFocus -= textText_GotFocus;
            textText.MouseLeave -= textText_MouseLeave;

            if (cancel != null)
            {
                using (cancel)
                {
                    cancel.Cancel();
                    cancel = null;
                }
            }

            Control c = source;
            do
            {
                c.LocationChanged -= source_LocationChanged;
                c = c.Parent;
            }
            while (c != null);

            //source.LocationChanged -= source_LocationChanged;

            //if (this.Owner != null)
            //    this.Owner.LocationChanged -= parent_LocationChanged;

            if (ExtensionClosing != null)
                ExtensionClosing(this, EventArgs.Empty);

            source.Text = textText.Text;
            source.SelectionStart = textText.SelectionStart;
            source.SelectionLength = textText.SelectionLength;

            base.Hide();
        }

        void textText_LostFocus(object sender, EventArgs e)
        {
            if (!this.ContainsFocus)
                OnHide();
        }

        private void EnableMessaging(bool enabled)
        {
            if (messaging == enabled)
                return;
            messaging = enabled;

            if (enabled)
            {
                Application.AddMessageFilter(this);
            }
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

        protected override void OnVisibleChanged(EventArgs e)
        {
            if (this.Visible)
            {
                textText.LostFocus += textText_LostFocus;
                textText.GotFocus += textText_GotFocus;
                textText.MouseLeave += textText_MouseLeave;

                Control c = source;
                do
                {
                    c.LocationChanged += source_LocationChanged;
                    c = c.Parent;
                }
                while (c != null);

                //source.LocationChanged += source_LocationChanged;

                //if (this.Owner != null)
                //    this.Owner.LocationChanged += parent_LocationChanged;

                if (!this.ContainsFocus)
                    EnableMessaging(true);

                if (ExtensionOpened != null)
                    ExtensionOpened(this, EventArgs.Empty);
            }
            else
            {
            }

            base.OnVisibleChanged(e);
        }

        void textText_MouseLeave(object sender, EventArgs e)
        {
            if (this.Owner == null || !this.ContainsFocus && !this.Owner.ContainsFocus)
                OnHide();
        }

        public void Show(Form parent, bool focus)
        {
            this.Location = source.Parent.PointToScreen(source.Location);
            this.Owner = parent;

            var screen = Screen.FromControl(this).WorkingArea;
            if (this.Right > screen.Right)
                this.Width = screen.Right - this.Left;
            if (this.Bottom > screen.Bottom)
                this.Height = screen.Bottom - this.Top;

            textText.Text = source.Text;
            textText.SelectionStart = source.SelectionStart;
            textText.SelectionLength = source.SelectionLength;

            NativeMethods.SetWindowPos(parent.Handle, this.Handle, 0, 0, 0, 0, SetWindowPosFlags.SWP_NOMOVE | SetWindowPosFlags.SWP_NOACTIVATE | SetWindowPosFlags.SWP_NOSIZE);

            this.Show();

            if (focus)
                textText.Focus();
        }

        void source_LocationChanged(object sender, EventArgs e)
        {
            var l = source.Location;
            Control p, c = source.Parent;

            do
            {
                l.Offset(c.Location);
                p = c;
                c = c.Parent;
            }
            while (c != null && !(c is Form));

            if (p.ClientRectangle.IntersectsWith(new Rectangle(l, source.Size)))
            {
                this.Location = source.Parent.PointToScreen(source.Location);
            }
            else
            {
                OnHide();
            }
        }

        void parent_LocationChanged(object sender, EventArgs e)
        {
            OnHide();
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

        async void DelayedMouseLeave()
        {
            cancel = new CancellationTokenSource();
            var token = cancel.Token;
            byte padding = 10;

            do
            {
                try
                {
                    await Task.Delay(500, token);
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

            OnHide();
        }

        public bool PreFilterMessage(ref Message m)
        {
            switch ((WindowMessages)m.Msg)
            {
                case WindowMessages.WM_NCLBUTTONDOWN:
                case WindowMessages.WM_NCRBUTTONDOWN:
                case WindowMessages.WM_NCMBUTTONDOWN:
                case WindowMessages.WM_NCXBUTTONDOWN:
                case WindowMessages.WM_LBUTTONDOWN:
                case WindowMessages.WM_RBUTTONDOWN:
                case WindowMessages.WM_MBUTTONDOWN:
                case WindowMessages.WM_XBUTTONDOWN:

                    if (!this.DesktopBounds.Contains(Cursor.Position))
                        OnHide();

                    break;
                case WindowMessages.WM_MOUSEMOVE:
                case WindowMessages.WM_NCMOUSEMOVE:

                    if (cancel != null && this.DesktopBounds.Contains(Cursor.Position))
                    {
                        using (cancel)
                        {
                            cancel.Cancel();
                            cancel = null;
                        }
                    }

                    break;
                case WindowMessages.WM_MOUSELEAVE:
                case WindowMessages.WM_NCMOUSELEAVE:

                    if (!this.DesktopBounds.Contains(Cursor.Position) && cancel == null)
                        DelayedMouseLeave();

                    break;
            }

            return false;
        }

        protected override void WndProc(ref Message m)
        {
            switch ((WindowMessages)m.Msg)
            {
                case WindowMessages.WM_WINDOWPOSCHANGING:

                    var pos = (WINDOWPOS)Marshal.PtrToStructure(m.LParam, typeof(WINDOWPOS));

                    if (!pos.flags.HasFlag(SetWindowPosFlags.SWP_NOSIZE))
                    {
                        pos.flags |= SetWindowPosFlags.SWP_NOREDRAW;
                        Marshal.StructureToPtr(pos, m.LParam, false);
                        return;
                    }

                    break;
                case Windows.Native.WindowMessages.WM_NCHITTEST:

                    //the only nc area is the corner of the textbox
                    m.Result = (IntPtr)Windows.Native.HitTest.BottomRight;

                    return;
            }

            base.WndProc(ref m);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (components != null)
                    components.Dispose();
                if (cancel != null)
                {
                    using (cancel)
                    {
                        cancel.Cancel();
                        cancel = null;
                    }
                }
            }
            base.Dispose(disposing);
        }
    }
}
