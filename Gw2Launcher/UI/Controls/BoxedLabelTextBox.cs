using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Drawing;
using System.ComponentModel;

namespace Gw2Launcher.UI.Controls
{
    class BoxedLabelTextBox : BoxedLabel
    {
        public event EventHandler<bool> TextBoxVisibleChanging;
        public event EventHandler BeforeShowTextBoxClicked;

        protected TextBox textbox;

        public BoxedLabelTextBox()
        {
            this.textbox = CreateTextBox();
            this.textbox.Visible = _TextVisible;

            textbox.SizeChanged += textbox_SizeChanged;

            this.Padding = new Padding(5, 0, 0, 1);
            this.TextAlign = ContentAlignment.MiddleLeft;
            this.Cursor = Cursors.Hand;

            this.Controls.Add(textbox);
        }

        protected virtual TextBox CreateTextBox()
        {
            return new TextBox()
            {
            };
        }

        void textbox_SizeChanged(object sender, EventArgs e)
        {
            this.Size = textbox.Size;
        }

        protected override void OnClick(EventArgs e)
        {
            if (BeforeShowTextBoxClicked != null)
                BeforeShowTextBoxClicked(this, EventArgs.Empty);

            TextVisible = true;
            textbox.Focus();

            base.OnClick(e);
        }

        protected override void OnLayout(LayoutEventArgs levent)
        {
            textbox.SetBounds(0, 0, this.Width, this.Height);

            base.OnLayout(levent);
        }

        protected override void OnSizeChanged(EventArgs e)
        {
            textbox.SetBounds(0, 0, this.Width, this.Height);

            base.OnSizeChanged(e);
        }

        public TextBox TextBox
        {
            get
            {
                return textbox;
            }
        }

        public Font FontText
        {
            get
            {
                return textbox.Font;
            }
            set
            {
                textbox.Font = value;
            }
        }

        public Color ForeColorText
        {
            get
            {
                return textbox.ForeColor;
            }
            set
            {
                textbox.ForeColor = value;
            }
        }

        protected bool _TextVisible;
        [DefaultValue(false)]
        public bool TextVisible
        {
            get
            {
                return _TextVisible;
            }
            set
            {
                if (_TextVisible != value)
                {
                    _TextVisible = value;

                    SetStyle(ControlStyles.Selectable, !value);

                    if (TextBoxVisibleChanging != null)
                        TextBoxVisibleChanging(this, value);

                    textbox.Visible = value;

                    OnRedrawRequired();
                }
            }
        }

        public override Size GetPreferredSize(Size proposedSize)
        {
            return textbox.GetPreferredSize(proposedSize);
        }

        protected override bool ScaleChildren
        {
            get
            {
                return false;
            }
        }

        protected override void OnPaintBackground(Graphics g)
        {
            if (_TextVisible)
            {
                g.Clear(this.BackColor);
            }
            else
            {
                base.OnPaintBackground(g);
            }
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);

            if (disposing)
            {
                textbox.Dispose();
            }
        }
    }
}
