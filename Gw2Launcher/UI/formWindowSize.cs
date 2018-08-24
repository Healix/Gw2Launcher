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

        private formWindowSizeOverlay overlay;

        public formWindowSize(bool showContextMenu)
        {
            InitializeComponent();

            this.showContextMenu = showContextMenu;

            overlay = new formWindowSizeOverlay(this);

            this.Shown += formWindowSize_Shown;
            this.GotFocus += new EventHandler(formWindowSize_GotFocus);
            this.LostFocus += new EventHandler(formWindowSize_LostFocus);

            ResizeWindow();

            //resizeThread = new Thread(new ThreadStart(ResizeWindow));
            //resizeThread.Start();
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
            mouseButtonDown = true;

            formMouseDownPoint = new Point(e.X, e.Y);
            formMouseDownSize = this.Bounds;
        }

        private void formWindowSize_MouseUp(object sender, MouseEventArgs e)
        {
            mouseButtonDown = false;

            if (e.Button == MouseButtons.Right)
            {
                saveToolStripMenuItem.Visible = this.showContextMenu;

                contextMenu.Show(Cursor.Position);
            }
        }

        private void formWindowSize_MouseMove(object sender, MouseEventArgs e)
        {
            if (mouseButtonDown)
                return;

            if (e.X < MOVE_SIZE && e.Y < MOVE_SIZE)
            {
                resizeMode = ResizeMode.TopLeft;
            }
            else if (e.X > this.Width - MOVE_SIZE && e.Y < MOVE_SIZE)
            {
                resizeMode = ResizeMode.TopRight;
            }
            else if (e.X < MOVE_SIZE && e.Y > this.Height - MOVE_SIZE)
            {
                resizeMode = ResizeMode.BottomLeft;
            }
            else if (e.X > this.Width - MOVE_SIZE && e.Y > this.Height - MOVE_SIZE)
            {
                resizeMode = ResizeMode.BottomRight;
            }
            else if (e.X < MOVE_SIZE)
            {
                resizeMode = ResizeMode.Left;
            }
            else if (e.X > this.Width - MOVE_SIZE)
            {
                resizeMode = ResizeMode.Right;
            }
            else if (e.Y < MOVE_SIZE)
            {
                resizeMode = ResizeMode.Top;
            }
            else if (e.Y > this.Height - MOVE_SIZE)
            {
                resizeMode = ResizeMode.Bottom;
            }
            else
            {
                resizeMode = ResizeMode.Move;
            }

            if (resizeMode == ResizeMode.Top || resizeMode == ResizeMode.Bottom)
            {
                this.Cursor = Cursors.SizeNS;
            }
            else if (resizeMode == ResizeMode.Left || resizeMode == ResizeMode.Right)
            {
                this.Cursor = Cursors.SizeWE;
            }
            else if (resizeMode == ResizeMode.TopLeft || resizeMode == ResizeMode.BottomRight)
            {
                this.Cursor = Cursors.SizeNWSE;
            }
            else if (resizeMode == ResizeMode.TopRight || resizeMode == ResizeMode.BottomLeft)
            {
                this.Cursor = Cursors.SizeNESW;
            }
            else
            {
                this.Cursor = Cursors.SizeAll;
            }
        }

        private void formWindowSize_FormClosing(object sender, FormClosingEventArgs e)
        {
            formClosing = true;
            overlay.Dispose();
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
            if (e.KeyCode == Keys.Escape)
            {
                this.DialogResult = DialogResult.Cancel;
            }
        }

        private void cancelToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
        }
    }
}