using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Runtime.InteropServices;

namespace Gw2Launcher.UI
{
    public partial class formAccountType : Base.FlatBase
    {
        private class SelectButton : Control
        {
            private bool hovered;

            public SelectButton()
            {
                this.ForeColor = Color.FromArgb(50, 50, 50);
                this.ForeColorHovered = Color.Black;
                this.BackColor = Color.FromArgb(240, 240, 240);
                this.BorderColor = Color.FromArgb(200, 200, 200);
                this.BackColorHovered = Color.FromArgb(230, 236, 244);
                this.BorderColorHovered = Color.FromArgb(190, 196, 204);
                this.Cursor = Cursors.Hand;
                this.DoubleBuffered = true;
            }

            public Color ForeColorHovered
            {
                get;
                set;
            }

            public Color BackColorHovered
            {
                get;
                set;
            }

            public Color BorderColor
            {
                get;
                set;
            }

            public Color BorderColorHovered
            {
                get;
                set;
            }

            protected override void OnMouseEnter(EventArgs e)
            {
                base.OnMouseEnter(e);
                hovered = true;
                this.Invalidate();
            }

            protected override void OnMouseLeave(EventArgs e)
            {
                base.OnMouseLeave(e);
                hovered = false;
                this.Invalidate();
            }

            protected override void OnPaintBackground(PaintEventArgs e)
            {
                var g = e.Graphics;

                Color fc, bc, rc;

                if (hovered)
                {
                    bc = this.BackColorHovered;
                    fc = this.ForeColorHovered;
                    rc = this.BorderColorHovered;
                }
                else
                {
                    bc = this.BackColor;
                    fc = this.ForeColor;
                    rc = this.BorderColor;
                }

                g.SetClip(e.ClipRectangle);
                g.Clear(bc);

                using (var p = new Pen(rc, 2))
                {
                    e.Graphics.DrawRectangle(p, 1, 1, this.Width - 2, this.Height - 2);
                }

                var x = 0;
                var i = this.BackgroundImage;

                if (i != null)
                {
                    int iw, ih;

                    ih = iw = this.Height / 2;

                    g.InterpolationMode = InterpolationMode.HighQualityBicubic;

                    using (var ia = new ImageAttributes())
                    {
                        ia.SetColorMatrix(new ColorMatrix()
                        {
                            Matrix33 = 0.75f,
                        });

                        g.DrawImage(i, new Rectangle(15, this.Height / 2 - ih / 2, iw, ih), 0, 0, i.Width, i.Height, GraphicsUnit.Pixel, ia);
                    }

                    x = 20 + iw;
                }
                else
                {
                    x = this.FontHeight * 2;
                }

                //x += 5;
                x = 0;
                TextRenderer.DrawText(g, this.Text, this.Font, Rectangle.FromLTRB(x, 0, this.Width, this.Height), fc, bc, TextFormatFlags.VerticalCenter |  TextFormatFlags.HorizontalCenter);
            }
        }

        public formAccountType()
        {
            InitializeComponents();

            using (var icon = new Icon(Properties.Resources.Gw2, 48, 48))
            {
                buttonGw2.BackgroundImage = icon.ToBitmap();
            }

            using (var icon = new Icon(Properties.Resources.Gw1, 48, 48))
            {
                buttonGw1.BackgroundImage = icon.ToBitmap();
            }
        }

        protected override void OnInitializeComponents()
        {
            base.OnInitializeComponents();

            InitializeComponent();
        }

        public Settings.AccountType Selected
        {
            get;
            private set;
        }

        protected override bool CloseOnEscape
        {
            get
            {
                return true;
            }
        }

        private void buttonGw1_Click(object sender, EventArgs e)
        {
            this.Selected = Settings.AccountType.GuildWars1;
            this.DialogResult = System.Windows.Forms.DialogResult.OK;
        }

        private void buttonGw2_Click(object sender, EventArgs e)
        {
            this.Selected = Settings.AccountType.GuildWars2;
            this.DialogResult = System.Windows.Forms.DialogResult.OK;
        }
    }
}
