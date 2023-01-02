using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Drawing;
using System.ComponentModel;
using Gw2Launcher.Windows;

namespace Gw2Launcher.UI.Controls
{
    class HotkeyInput : Control
    {
        private Keys keys;
        private Pen penBorder;
        private Keys verified;
        private bool invalid;
        private bool valid;
        private Font fontEmpty;

        public HotkeyInput()
        {
            SetStyle(ControlStyles.OptimizedDoubleBuffer, true);
            SetStyle(ControlStyles.AllPaintingInWmPaint, true);
            SetStyle(ControlStyles.UserPaint, true);
            SetStyle(ControlStyles.Selectable, true);
            SetStyle(ControlStyles.ResizeRedraw, true);

            this.Text = null;
            this.Cursor = Cursors.Hand;

            penBorder = new Pen(Color.DarkGray, 1);
        }

        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public override string Text
        {
            get
            {
                return base.Text;
            }
            set
            {
                base.Text = value;
            }
        }

        public Keys Keys
        {
            get
            {
                return keys;
            }
            set
            {
                if (keys != value)
                {
                    keys = value;

                    if (keys != Keys.None)
                    {
                        if (valid = Hotkeys.IsValid(keys))
                        {
                            invalid = !Hotkeys.IsAvailable(this.Handle, keys);
                            this.Text = Hotkeys.ToString(keys);
                        }
                        else
                        {
                            keys = Keys.None;
                            this.Text = null;
                        }
                    }

                    penBorder.Color = invalid ? Color.DarkRed : Color.DarkGray;
                }
            }
        }

        protected override void OnClick(EventArgs e)
        {
            base.OnClick(e);
            this.Focus();
        }

        protected override void OnKeyDown(KeyEventArgs e)
        {
            base.OnKeyDown(e);

            e.Handled = true;
            e.SuppressKeyPress = true;

            if (keys != e.KeyData)
            {
                keys = e.KeyData;

                valid = Hotkeys.IsValid(keys);
                this.Text = Hotkeys.ToString(keys);
            }
        }

        protected override void OnKeyUp(KeyEventArgs e)
        {
            base.OnKeyUp(e);

            e.Handled = true;
            e.SuppressKeyPress = true;
        }

        protected override void OnPreviewKeyDown(PreviewKeyDownEventArgs e)
        {
            base.OnPreviewKeyDown(e);

            e.IsInputKey = true;
        }

        protected override void OnTextChanged(EventArgs e)
        {
            base.OnTextChanged(e);
            this.Invalidate();
        }

        protected override void OnGotFocus(EventArgs e)
        {
            base.OnGotFocus(e);

            if (invalid)
            {
                invalid = false;
                verified = Keys.None;
                valid = false;
            }

            penBorder.Color = SystemColors.Highlight;
            this.Invalidate();
        }

        protected override void OnLostFocus(EventArgs e)
        {
            base.OnLostFocus(e);

            if (keys != Keys.None)
            {
                if (valid = Hotkeys.IsValid(keys))
                {
                    verified = keys;
                    invalid = !Hotkeys.IsAvailable(this.Handle, keys);
                }
                else
                {
                    keys = Keys.None;
                    this.Text = null;
                }
            }

            penBorder.Color = invalid ? Color.DarkRed : Color.DarkGray;
            this.Invalidate();
        }

        protected override void OnMouseEnter(EventArgs e)
        {
            base.OnMouseEnter(e);

            if (!this.Focused && !invalid)
            {
                penBorder.Color = Color.Gray;
                this.Invalidate();
            }
        }

        protected override void OnMouseLeave(EventArgs e)
        {
            base.OnMouseLeave(e);

            if (!this.Focused && !invalid)
            {
                penBorder.Color = Color.DarkGray;
                this.Invalidate();
            }
        }

        protected override void OnPaintBackground(PaintEventArgs e)
        {
            base.OnPaintBackground(e);

            e.Graphics.DrawRectangle(penBorder, 0, 0, this.Width - 1, this.Height - 1);
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);

            Color forecolor;
            var f = this.Font;
            var t = this.Text;

            if (keys == Keys.None)
            {
                if (fontEmpty == null)
                    fontEmpty = new System.Drawing.Font("Calibri", f.SizeInPoints, FontStyle.Regular, GraphicsUnit.Point, 0);
                f = fontEmpty;
                t = "(not set)";
                forecolor = SystemColors.GrayText;
            }
            else if (invalid)
            {
                forecolor = Color.DarkRed;
            }
            else if (valid)
            {
                forecolor = SystemColors.ControlText;
            }
            else
            {
                forecolor = SystemColors.GrayText;
            }

            TextRenderer.DrawText(e.Graphics, t, f, this.ClientRectangle, forecolor, Color.Transparent, TextFormatFlags.HorizontalCenter | TextFormatFlags.SingleLine | TextFormatFlags.VerticalCenter | TextFormatFlags.EndEllipsis);
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);

            if (disposing)
            {
                if (penBorder != null)
                {
                    penBorder.Dispose();
                    penBorder = null;
                }
                if (fontEmpty != null)
                {
                    fontEmpty.Dispose();
                    fontEmpty = null;
                }
            }
        }
    }
}
