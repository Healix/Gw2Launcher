using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Threading;
using System.Threading.Tasks;

namespace Gw2Launcher.UI
{
    public partial class formWindowSize : Form
    {
        private const int MOVE_SIZE = 10;

        private enum ResizeMode { Move, Left, Right, Top, Bottom, TopLeft, TopRight, BottomLeft, BottomRight };

        //private Thread resizeThread;
        private bool formClosing;

        private ResizeMode resizeMode;

        private bool mouseButtonDown;
        private Point formMouseDownPoint;
        private Rectangle formMouseDownSize;
        private bool showContextMenu;
        protected Settings.IAccount account;

        private formWindowSizeOverlay overlay;

        public formWindowSize(bool showContextMenu, Settings.IAccount account)
            : this(showContextMenu, true, Color.White, account)
        {
        }

        public formWindowSize(bool showContextMenu, bool showInfo, Color backColor, Settings.IAccount account)
        {
            InitializeComponent();

            SetStyle(ControlStyles.UserPaint, true);
            SetStyle(ControlStyles.AllPaintingInWmPaint, true);
            SetStyle(ControlStyles.OptimizedDoubleBuffer, true);

            this.showContextMenu = showContextMenu;
            this.account = account;

            this.Text = account.Name;

            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.Sizable;
            this.BackColor = backColor;
            this.Cursor = Cursors.SizeAll;
            this.resizeMode = ResizeMode.Move;

            var b = this.Bounds;
            var c = this.ClientRectangle;

            var borderSize = (b.Width - c.Width) / 2;
            var captionHeight = b.Height - c.Height - borderSize;

            overlay = new formWindowSizeOverlay(this, showInfo, captionHeight, borderSize);

            this.Shown += formWindowSize_Shown;
            this.GotFocus += new EventHandler(formWindowSize_GotFocus);
            this.LostFocus += new EventHandler(formWindowSize_LostFocus);

            ResizeWindow();
        }

        protected formWindowSize PreviousForm
        {
            get;
            set;
        }

        protected formWindowSize NextForm
        {
            get;
            set;
        }

        void formWindowSize_Shown(object sender, EventArgs e)
        {
            overlay.Show(this);
        }

        private void formWindowSize_LostFocus(object sender, EventArgs e)
        {
            //this.BackColor = Color.Gray;
        }

        private void formWindowSize_GotFocus(object sender, EventArgs e)
        {
            //this.BackColor = Color.White;
        }

        private void formWindowSize_Load(object sender, EventArgs e)
        {
        }

        public Rectangle Result
        {
            get
            {
                return this.Bounds;
            }
        }

        public void SetWindowLocation(Rectangle rect)
        {
            if (this.InvokeRequired)
            {
                try
                {
                    this.Invoke(new MethodInvoker(
                        delegate
                        {
                            SetWindowLocation(rect);
                        }));
                }
                catch (Exception ex)
                {
                    Util.Logging.Log(ex);
                }

                return;
            }

            if (rect.Width < 20)
            {
                rect = new Rectangle(this.Bounds.X, rect.Y, 20, rect.Height);
            }
            if (rect.Height < 20)
            {
                rect = new Rectangle(rect.X, this.Bounds.Y, rect.Width, 20);
            }

            if (rect.Width >= 20 && rect.Height >= 20)
            {
                this.Bounds = rect;
                this.Refresh();
            }
        }

        private async void ResizeWindow()
        {
            while (!formClosing && !this.Disposing && !this.IsDisposed)
            {
                if (mouseButtonDown)
                {
                    int x = this.Location.X;
                    int y = this.Location.Y;
                    int width = this.Width;
                    int height = this.Height;

                    if (resizeMode == ResizeMode.Move)
                    {
                        x = Cursor.Position.X - formMouseDownPoint.X;
                        y = Cursor.Position.Y - formMouseDownPoint.Y;
                    }

                    if (resizeMode == ResizeMode.Left || resizeMode == ResizeMode.TopLeft || resizeMode == ResizeMode.BottomLeft)
                    {
                        x = Cursor.Position.X - formMouseDownPoint.X;
                        width = formMouseDownSize.Width + (formMouseDownSize.X - x);
                    }

                    if (resizeMode == ResizeMode.Right || resizeMode == ResizeMode.TopRight || resizeMode == ResizeMode.BottomRight)
                    {
                        width = (Cursor.Position.X - (formMouseDownPoint.X - formMouseDownSize.Width)) - this.Location.X;
                    }

                    if (resizeMode == ResizeMode.Top || resizeMode == ResizeMode.TopLeft || resizeMode == ResizeMode.TopRight)
                    {
                        y = Cursor.Position.Y - formMouseDownPoint.Y;
                        height = formMouseDownSize.Height + (formMouseDownSize.Y - (Cursor.Position.Y - formMouseDownPoint.Y));
                    }

                    if (resizeMode == ResizeMode.Bottom || resizeMode == ResizeMode.BottomLeft || resizeMode == ResizeMode.BottomRight)
                    {
                        height = (Cursor.Position.Y - (formMouseDownPoint.Y - formMouseDownSize.Height)) - this.Location.Y;
                    }

                    SetWindowLocation(new Rectangle(x, y, width, height));
                }

                await Task.Delay(10);
            }
        }

        private void formWindowSize_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                mouseButtonDown = true;

                formMouseDownPoint = new Point(Cursor.Position.X - this.Location.X, Cursor.Position.Y - this.Location.Y); // new Point(e.X, e.Y);
                formMouseDownSize = this.Bounds;
            }
        }

        private void formWindowSize_MouseUp(object sender, MouseEventArgs e)
        {
            mouseButtonDown = false;

            if (e.Button == MouseButtons.Right && this.showContextMenu)
            {
                saveAllToolStripMenuItem.Enabled = PreviousForm != null;

                IntPtr handle = IntPtr.Zero;
                try
                {
                    var p = Client.Launcher.FindProcess(account);
                    if (p != null && !p.HasExited)
                    {
                        handle = p.MainWindowHandle;
                        if (!Windows.WindowSize.IsWindow(handle))
                            handle = IntPtr.Zero;
                    }
                }
                catch 
                {
                    handle = IntPtr.Zero;
                }

                matchProcessToolStripMenuItem.Enabled = handle != IntPtr.Zero;
                matchProcessToolStripMenuItem.Tag = handle;
                applyBoundsToProcessToolStripMenuItem.Enabled = handle != IntPtr.Zero;

                contextMenu.Show(Cursor.Position);
            }
        }

        private void formWindowSize_MouseMove(object sender, MouseEventArgs e)
        {
            if (mouseButtonDown)
                return;

            //if (e.X < MOVE_SIZE && e.Y < MOVE_SIZE)
            //{
            //    resizeMode = ResizeMode.TopLeft;
            //}
            //else if (e.X > this.Width - MOVE_SIZE && e.Y < MOVE_SIZE)
            //{
            //    resizeMode = ResizeMode.TopRight;
            //}
            //else if (e.X < MOVE_SIZE && e.Y > this.Height - MOVE_SIZE)
            //{
            //    resizeMode = ResizeMode.BottomLeft;
            //}
            //else if (e.X > this.Width - MOVE_SIZE && e.Y > this.Height - MOVE_SIZE)
            //{
            //    resizeMode = ResizeMode.BottomRight;
            //}
            //else if (e.X < MOVE_SIZE)
            //{
            //    resizeMode = ResizeMode.Left;
            //}
            //else if (e.X > this.Width - MOVE_SIZE)
            //{
            //    resizeMode = ResizeMode.Right;
            //}
            //else if (e.Y < MOVE_SIZE)
            //{
            //    resizeMode = ResizeMode.Top;
            //}
            //else if (e.Y > this.Height - MOVE_SIZE)
            //{
            //    resizeMode = ResizeMode.Bottom;
            //}
            //else
            //{
            //    resizeMode = ResizeMode.Move;
            //}

            //if (resizeMode == ResizeMode.Top || resizeMode == ResizeMode.Bottom)
            //{
            //    this.Cursor = Cursors.SizeNS;
            //}
            //else if (resizeMode == ResizeMode.Left || resizeMode == ResizeMode.Right)
            //{
            //    this.Cursor = Cursors.SizeWE;
            //}
            //else if (resizeMode == ResizeMode.TopLeft || resizeMode == ResizeMode.BottomRight)
            //{
            //    this.Cursor = Cursors.SizeNWSE;
            //}
            //else if (resizeMode == ResizeMode.TopRight || resizeMode == ResizeMode.BottomLeft)
            //{
            //    this.Cursor = Cursors.SizeNESW;
            //}
            //else
            //{
            //    this.Cursor = Cursors.SizeAll;
            //}
        }

        private void formWindowSize_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (e.CloseReason == CloseReason.FormOwnerClosing && (this.PreviousForm != null || this.NextForm != null))
            {

            }
            else
            {
                if (this.PreviousForm != null || this.NextForm != null)
                {
                    if (this.PreviousForm != null)
                    {
                        this.PreviousForm.NextForm = this.NextForm;
                    }

                    if (this.NextForm != null)
                    {
                        this.NextForm.PreviousForm = this.PreviousForm;

                        var owner = this.Owner;
                        this.Owner = null;
                        this.NextForm.Owner = owner;
                    }
                }

                if (this.PreviousForm != null && this.NextForm == null)
                {
                    CloseForms();
                }

                formClosing = true;
                overlay.Dispose();
            }
        }

        private void saveToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.OK;
        }

        private void formWindowSize_Paint(object sender, PaintEventArgs e)
        {
        }

        private void formWindowSize_MouseEnter(object sender, EventArgs e)
        {
        }

        private void formWindowSize_MouseLeave(object sender, EventArgs e)
        {
        }

        private void formWindowSize_KeyPress(object sender, KeyPressEventArgs e)
        {

        }

        private void formWindowSize_KeyDown(object sender, KeyEventArgs e)
        {
            switch (e.KeyCode)
            {
                case Keys.Escape:
                    this.DialogResult = DialogResult.Cancel;
                    break;
                case Keys.Left:
                    this.Location = new Point(this.Location.X - 1, this.Location.Y);
                    break;
                case Keys.Right:
                    this.Location = new Point(this.Location.X + 1, this.Location.Y);
                    break;
                case Keys.Up:
                    this.Location = new Point(this.Location.X, this.Location.Y - 1);
                    break;
                case Keys.Down:
                    this.Location = new Point(this.Location.X, this.Location.Y + 1);
                    break;

            }
        }

        private void cancelToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
        }

        private void showOtherAccountsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (showOtherAccountsToolStripMenuItem.Checked = !showOtherAccountsToolStripMenuItem.Checked)
            {
                Form last = null;

                var accounts = Settings.Accounts;
                foreach (var uid in accounts.GetKeys())
                {
                    var account = accounts[uid].Value;

                    if (account != null && account.Windowed && !account.WindowBounds.IsEmpty)
                    {
                        if (account == this.account)
                            continue;

                        var f = new formWindowSize(false, false, Color.LightGray, account);

                        var owner = this.Owner;
                        this.Owner = f;

                        f.PreviousForm = this.PreviousForm;
                        if (f.PreviousForm != null)
                        {
                            f.PreviousForm.NextForm = f;
                            f.Owner = owner;// f.PreviousForm;
                        }
                        else
                            f.Owner = owner;

                        f.NextForm = this;
                        this.PreviousForm = f;

                        f.FormClosed += f_FormClosed;

                        f.Bounds = account.WindowBounds;
                        f.Show();

                        last = f;
                    }
                }
            }
            else
            {
                CloseForms();
            }
        }

        void f_FormClosed(object sender, FormClosedEventArgs e)
        {
            var f = (formWindowSize)sender;

            if (this.PreviousForm == null)
                showOtherAccountsToolStripMenuItem.Checked = false;
        }

        private void CloseForms()
        {
            this.Owner = null;

            var f = this;
            Form owner = null;

            do
            {
                var previous = f.PreviousForm;

                f.PreviousForm = null;
                f.NextForm = null;

                if (f != this)
                {
                    owner = f.Owner;
                    f.Owner = null;
                    f.Dispose();
                }

                f = previous;
            }
            while (f != null);

            this.Owner = owner;
        }

        private void saveAllToolStripMenuItem_Click(object sender, EventArgs e)
        {
            formWindowSize f = this.PreviousForm;
            while (f != null)
            {
                if (f.account.WindowBounds != f.Bounds)
                {
                    f.account.WindowBounds = f.Bounds;
                }
                f = f.PreviousForm;
            }

            this.DialogResult = DialogResult.OK;
        }

        private void matchProcessToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                var rect = Windows.WindowSize.GetWindowRect((IntPtr)matchProcessToolStripMenuItem.Tag);
                this.Bounds = rect;
            }
            catch { }
        }

        private void applyBoundsToProcessToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                Windows.WindowSize.SetWindowPlacement((IntPtr)matchProcessToolStripMenuItem.Tag, this.Bounds, Windows.WindowSize.WindowState.SW_SHOWNORMAL);
            }
            catch { }
        }
    }
}