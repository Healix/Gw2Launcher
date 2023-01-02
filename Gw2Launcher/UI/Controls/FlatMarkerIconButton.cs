using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using System.Drawing.Imaging;
using System.Windows.Forms;

namespace Gw2Launcher.UI.Controls
{
    class FlatMarkerIconButton : FlatButton
    {
        public FlatMarkerIconButton()
        {
        }

        private Settings.MarkerIconType _Marker;
        public Settings.MarkerIconType Marker
        {
            get
            {
                return _Marker;
            }
            set
            {
                if (_Marker != value)
                {
                    _Marker = value;
                    OnRedrawRequired();
                }
            }
        }

        [System.ComponentModel.Browsable(false)]
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

        protected override void OnPaintBuffer(Graphics g)
        {
            int x = this.Padding.Left,
                y = this.Padding.Top,
                w = this.Width - this.Padding.Horizontal,
                h = this.Height - this.Padding.Vertical;
            var c = this.ForeColorCurrent;

            switch (_Marker)
            {
                case Settings.MarkerIconType.Square:

                    using (var brush = new SolidBrush(c))
                    {
                        g.FillRectangle(brush, x + 1, y + 1, w - 2, h - 2);
                    }

                    using (var pen = new Pen(Color.FromArgb((int)(c.R * 0.75f), (int)(c.G * 0.75f), (int)(c.B * 0.75f))))
                    {
                        g.DrawRectangle(pen, x, y, w - 1, h - 1);
                    }

                    break;
                case Settings.MarkerIconType.Circle:

                    g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;

                    using (var pen = new Pen(Color.FromArgb((int)(c.R * 0.75f), (int)(c.G * 0.75f), (int)(c.B * 0.75f))))
                    {
                        using (var brush = new SolidBrush(c))
                        {
                            g.FillEllipse(brush, x, y, w, h);
                            g.DrawEllipse(pen, x, y, w, h);
                        }
                    }

                    g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.None;

                    break;
                case Settings.MarkerIconType.Icon:

                    g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.High;
                    
                    if (isHovered || _ImageGrayscale || _ImageOpacity != 255)
                    {
                        var mat = new float[][] 
                        {
                            null,
                            null,
                            null,
                            new float[] {0, 0, 0, ImageOpacity, 0},
                            new float[] {0, 0, 0, 0, 1}
                        };

                        if (_ImageGrayscale)
                        {
                            mat[0] = new float[] { .3f, .3f, .3f, 0, 0 };
                            mat[1] = new float[] { .6f, .6f, .6f, 0, 0 };
                            mat[2] = new float[] { .1f, .1f, .1f, 0, 0 };
                        }
                        else
                        {
                            mat[0] = new float[] { 1, 0, 0, 0, 0 };
                            mat[1] = new float[] { 0, 1, 0, 0, 0 };
                            mat[2] = new float[] { 0, 0, 1, 0, 0 };
                        }

                        if (isHovered)
                        {
                            mat[0][0] *= 1.5f;
                            mat[1][1] *= 1.5f;
                            mat[2][2] *= 1.5f;
                        }

                        using (var ia = new ImageAttributes())
                        {
                            ia.SetColorMatrix(new ColorMatrix(mat));
                            g.DrawImage(this.BackgroundImage, new Rectangle(x, y, w, h), 0, 0, this.BackgroundImage.Width, this.BackgroundImage.Height, GraphicsUnit.Pixel, ia);
                        }
                    }
                    else
                    {
                        g.DrawImage(this.BackgroundImage, x, y, w, h);
                    }


                    break;
            }
        }

        private byte _ImageOpacity = 255;
        [System.ComponentModel.DefaultValue(1)]
        public float ImageOpacity
        {
            get
            {
                return _ImageOpacity / 255f;
            }
            set
            {
                byte v;

                if (value >= 1)
                {
                    v = 255;
                }
                else if (value <= 0)
                {
                    v = 0;
                }
                else
                {
                    v = (byte)(255 * value + 0.5f);
                }

                if (_ImageOpacity != v)
                {
                    _ImageOpacity = v;
                    OnRedrawRequired();
                }
            }
        }

        private bool _ImageGrayscale = false;
        [System.ComponentModel.DefaultValue(false)]
        public bool ImageGrayscale
        {
            get
            {
                return _ImageGrayscale;
            }
            set
            {
                if (_ImageGrayscale != value)
                {
                    _ImageGrayscale = value;
                    OnRedrawRequired();
                }
            }
        }

        private Tools.Shared.Images.SourceValue _ImageSource;
        public Tools.Shared.Images.SourceValue ImageSource
        {
            get
            {
                return _ImageSource;
            }
            set
            {
                if (_ImageSource != value)
                {
                    using (_ImageSource)
                    {
                        _ImageSource = value;

                        if (value != null)
                        {
                            value.SourceLoaded += image_SourceLoaded;

                            this.BackgroundImage = value.GetValue();
                        }
                        else
                        {
                            this.BackgroundImage = null;
                        }
                    }
                }
            }
        }

        void image_SourceLoaded(object sender, EventArgs e)
        {
            var source = (Tools.Shared.Images.SourceValue)sender;
            if (source.RefreshValue(true))
            {
                this.BackgroundImage = source.GetValue();
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                ImageSource = null;
            }
            base.Dispose(disposing);
        }
    }
}
