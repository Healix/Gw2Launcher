using System;
using System.Windows.Forms;
using System.Drawing;
using System.Drawing.Drawing2D;
using Gw2Launcher.Windows.Native;

namespace Gw2Launcher.UI.ColorPicker.Controls
{
    public class ShadePanel : BaseColorPanel
    {
        public event EventHandler<Color> ColorChanged;

        private bool redrawBackground;
        private Color colorBase;

        public ShadePanel()
        {
            colorBase = Color.Red;
        }

        protected override void OnSizeChanged(EventArgs e)
        {
            base.OnSizeChanged(e);

            if (background != null)
            {
                background.Dispose();
                background = null;
            }

            if (!_SelectedColor.IsEmpty)
                SetCursorPosition(_SelectedColor);
        }

        protected override void OnPaintBackground()
        {
            if (background == null)
            {
                background = new Bitmap(this.ClientSize.Width, this.ClientSize.Height);
                redrawBackground = true;
            }

            if (redrawBackground)
            {
                var w = this.ClientSize.Width;
                var h = this.ClientSize.Height;
                var bh = h / 255f;

                using (var g = Graphics.FromImage(background))
                {
                    using (var brush = new LinearGradientBrush(this.DisplayRectangle, Color.Transparent, Color.Transparent, LinearGradientMode.Horizontal))
                    {
                        var count = (int)(h / bh);

                        for (var i = 0; i < count; i++)
                        {
                            var y1 = (int)(i * bh);
                            var y2 = i == count - 1 ? h : (int)((i + 1) * bh);

                            var p = 1 - (float)y1 / h;
                            var c = (int)(255 * p);
                            var cr = (int)(colorBase.R * p);
                            var cg = (int)(colorBase.G * p);
                            var cb = (int)(colorBase.B * p);

                            brush.LinearColors = new Color[] 
                            {
                                Color.FromArgb(c,c,c),
                                Color.FromArgb(cr,cg,cb),
                            };

                            g.FillRectangle(brush, 0, y1, w, y2 - y1);
                        }
                    }
                }
            }

            base.OnPaintBackground();
        }

        protected override void OnCursorChanged()
        {
            var x = (float)cursor.X / this.ClientSize.Width;
            var y = 1 - (float)cursor.Y / this.ClientSize.Height;

            var c = 255 * y * (1 - x);
            var cr = (int)(colorBase.R * y * x + c);
            var cg = (int)(colorBase.G * y * x + c);
            var cb = (int)(colorBase.B * y * x + c);

            var color = Color.FromArgb(cr, cg, cb);

            if (_SelectedColor != color)
            {
                _SelectedColor = color;
                if (ColorChanged != null)
                    ColorChanged(this, color);
            }
        }

        private int _Hue;
        public int Hue
        {
            get
            {
                return _Hue;
            }
            set
            {
                if (_Hue != value)
                {
                    _Hue = value;

                    colorBase = Util.Color.FromHSL(value, 240, 120);

                    redrawBackground = true;
                    OnRedrawRequired();
                    OnCursorChanged();
                }
            }
        }

        private Color _SelectedColor;
        public Color SelectedColor
        {
            get
            {
                return _SelectedColor;
            }
            set
            {
                if (value.A != 255)
                    value = Color.FromArgb(255, value);

                if (_SelectedColor != value)
                {
                    _SelectedColor = value;

                    var h = (int)(value.GetHue() / 360f * 240f);

                    if (_Hue != h)
                    {
                        _Hue = h;
                        colorBase = Util.Color.FromHSL(h, 240, 120);

                        redrawBackground = true;
                        OnRedrawRequired();
                    }

                    SetCursorPosition(value);

                    if (ColorChanged != null)
                        ColorChanged(this, value);
                }
            }
        }

        private void SetCursorPosition(Color c)
        {
            int min, max;

            if (c.R > c.G)
            {
                max = c.R;
                min = c.G;
            }
            else
            {
                max = c.G;
                min = c.R;
            }

            if (c.B > max)
                max = c.B;
            else if (c.B < min)
                min = c.B;

            var saturation = max == 0 ? 0f : 1f - 1f * min / max;
            var brightness = max / 255f;

            SetCursorPosition((int)(this.ClientSize.Width * saturation), (int)(this.ClientSize.Height * (1 - brightness)));
        }
    }
}
