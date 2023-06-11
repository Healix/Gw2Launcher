using System;
using System.Windows.Forms;
using System.Drawing;
using System.Drawing.Drawing2D;

namespace Gw2Launcher.UI.ColorPicker.Controls
{
    public class BaseColorPanel : Panel
    {
        protected BufferedGraphics buffer;
        protected bool redraw;
        protected Bitmap background;
        protected Point cursor;
        protected int cursorSize;
        protected bool selecting;

        public BaseColorPanel()
        {
            SetStyle(ControlStyles.AllPaintingInWmPaint, true);
            SetStyle(ControlStyles.UserPaint, true);

            cursorSize = 14;
            redraw = true;
        }

        public int CursorSize
        {
            get
            {
                return cursorSize;
            }
            set
            {
                cursorSize = value;
                OnRedrawRequired();
            }
        }

        protected void OnRedrawRequired()
        {
            if (redraw)
                return;
            redraw = true;
            this.Invalidate();
        }

        protected override void OnSizeChanged(EventArgs e)
        {
            base.OnSizeChanged(e);

            if (buffer != null)
            {
                buffer.Dispose();
                buffer = null;
            }

            OnRedrawRequired();
        }

        protected override void OnMouseDown(MouseEventArgs e)
        {
            base.OnMouseDown(e);

            this.Focus();
            selecting = true;
            OnCursorMoving(e.X, e.Y);
        }

        protected override void OnMouseUp(MouseEventArgs e)
        {
            base.OnMouseUp(e);

            selecting = false;
        }

        protected override void OnMouseCaptureChanged(EventArgs e)
        {
            base.OnMouseCaptureChanged(e);

            selecting = false;
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);

            if (selecting)
            {
                OnCursorMoving(e.X, e.Y);
            }
        }

        protected virtual void OnCursorMoving(int x, int y)
        {
            if (x < 0)
                x = 0;
            else if (x > this.ClientSize.Width)
                x = this.ClientSize.Width;

            if (y < 0)
                y = 0;
            else if (y > this.ClientSize.Height)
                y = this.ClientSize.Height;

            if (cursor.X == x && cursor.Y == y)
                return;

            SetCursorPosition(x, y);
            OnCursorChanged();
        }

        protected void SetCursorPosition(int x, int y)
        {
            cursor = new Point(x, y);
            OnRedrawRequired();
        }

        protected virtual void OnCursorChanged()
        {

        }

        protected virtual void OnPaintBackground()
        {
            if (background != null)
            {
                buffer.Graphics.DrawImageUnscaled(background, 0, 0);
            }
        }

        protected virtual void DrawCursor(Graphics g)
        {
            var x = cursor.X - cursorSize / 2;
            var y = cursor.Y - cursorSize / 2;

            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.CompositingQuality = CompositingQuality.HighQuality;

            g.DrawEllipse(Pens.White, x + 1, y + 1, cursorSize - 2, cursorSize - 2);
            g.DrawEllipse(Pens.Black, x, y, cursorSize, cursorSize);
        }

        protected override void OnPaintBackground(PaintEventArgs e)
        {
            if (redraw)
            {
                redraw = false;

                if (buffer == null)
                    buffer = BufferedGraphicsManager.Current.Allocate(e.Graphics, this.DisplayRectangle);

                OnPaintBackground();

                DrawCursor(buffer.Graphics);
            }

            buffer.Render(e.Graphics);
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);

            if (disposing)
            {
                if (buffer != null)
                {
                    buffer.Dispose();
                    buffer = null;
                }
                if (background != null)
                {
                    background.Dispose();
                    background = null;
                }
            }
        }
    }
}
