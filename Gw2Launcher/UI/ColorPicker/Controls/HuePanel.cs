using System;
using System.Windows.Forms;
using System.Drawing;
using System.Drawing.Drawing2D;
using Gw2Launcher.Windows.Native;

namespace Gw2Launcher.UI.ColorPicker.Controls
{
    public class HuePanel : BaseColorPanel
    {
        public event EventHandler<int> HueChanged;

        protected override void OnSizeChanged(EventArgs e)
        {
            base.OnSizeChanged(e);

            if (background != null)
            {
                background.Dispose();
                background = null;
            }

            OnCursorMoving(cursor.X, cursor.Y);
        }

        protected override void OnPaintBackground()
        {
            if (background == null)
            {
                background = new Bitmap(this.ClientSize.Width, this.ClientSize.Height);

                using (var brush = new LinearGradientBrush(this.DisplayRectangle, Color.Transparent, Color.Transparent, LinearGradientMode.Vertical))
                {
                    const int COUNT = 30;

                    var colors = new Color[COUNT + 1];
                    var positions = new float[COUNT + 1];

                    for (var i = 0; i <= COUNT; i++)
                    {
                        var p = (float)i / COUNT;

                        colors[i] = Util.Color.FromHSL(240 - i * 240 / COUNT, 240, 120);
                        positions[i] = p;
                    }

                    brush.InterpolationColors = new ColorBlend()
                    {
                        Colors = colors,
                        Positions = positions,
                    };

                    using (var g = Graphics.FromImage(background))
                    {
                        g.FillRectangle(brush, this.DisplayRectangle);
                    }
                }
            }

            base.OnPaintBackground();
        }

        protected override void OnCursorMoving(int x, int y)
        {
            base.OnCursorMoving(this.ClientSize.Width / 2, y);
        }

        protected override void OnCursorChanged()
        {
            this.Hue = 240 - (int)(240f * cursor.Y / this.ClientSize.Height);
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

                    var h = this.ClientSize.Height;
                    var y = h - (int)(value / 240f * h);

                    if (cursor.Y != y)
                        OnCursorMoving(cursor.X, y);

                    if (HueChanged != null)
                        HueChanged(this, value);
                }
            }
        }
    }
}
