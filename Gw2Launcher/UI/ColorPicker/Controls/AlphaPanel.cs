using System;
using System.Windows.Forms;
using System.Drawing;
using System.Drawing.Drawing2D;

namespace Gw2Launcher.UI.ColorPicker.Controls
{
    public class AlphaPanel : BaseColorPanel
    {
        public event EventHandler<float> AlphaChanged;

        private Bitmap checkers;
        private bool redrawBackground;

        public AlphaPanel()
        {
            _AllowAlphaTransparency = true;
        }

        protected override void OnSizeChanged(EventArgs e)
        {
            base.OnSizeChanged(e);

            if (background != null)
            {
                background.Dispose();
                background = null;
            }

            if (checkers != null)
            {
                checkers.Dispose();
                checkers = null;
            }

            OnCursorMoving(cursor.X, cursor.Y);
        }

        protected override void OnPaintBackground()
        {
            if (checkers == null)
            {
                var w = this.ClientSize.Width;
                var h = this.ClientSize.Height;

                checkers = new Bitmap(w, h);

                using (var g = Graphics.FromImage(checkers))
                {
                    g.Clear(Color.White);

                    var bs = 10 * g.DpiX / 96f; 
                    var rows = (int)(h / (float)bs + 0.999f);
                    var cols = (int)(w / (float)bs + 0.999f);

                    for (var r = 0; r < rows; r++)
                    {
                        var b = r % 2 == 0;

                        for (var c = 0; c < cols; c++)
                        {
                            if (b)
                            {
                                g.FillRectangle(Brushes.LightGray, c * bs, r * bs, bs, bs);
                            }

                            b = !b;
                        }
                    }
                }
            }

            if (background == null)
            {
                background = new Bitmap(this.ClientSize.Width, this.ClientSize.Height);
                redrawBackground = true;
            }

            if (redrawBackground)
            {
                redrawBackground = false;

                using (var g = Graphics.FromImage(background))
                {
                    if (_AllowAlphaTransparency)
                        g.DrawImageUnscaled(checkers, 0, 0);

                    using (var brush = new LinearGradientBrush(this.DisplayRectangle, this.BackColor, Color.Transparent, LinearGradientMode.Vertical))
                    {
                        if (!_AllowAlphaTransparency)
                        {
                            brush.InterpolationColors = new ColorBlend()
                            {
                                Colors = new Color[] { Color.White, this.BackColor, Color.Black },
                                Positions = new float[] { 0f, 0.5f, 1f },
                            };
                        }

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
            if (selecting)
            {
                this.Alpha = 1 - (float)cursor.Y / this.ClientSize.Height;
            }
        }

        protected override void OnBackColorChanged(EventArgs e)
        {
            redrawBackground = true;
            OnRedrawRequired();
            
            base.OnBackColorChanged(e);
        }

        protected override void DrawCursor(Graphics g)
        {
            if (this.Enabled)
                base.DrawCursor(g);
        }

        private float _Alpha;
        public float Alpha
        {
            get
            {
                return _Alpha;
            }
            set
            {
                if (_Alpha != value)
                {
                    _Alpha = value;

                    var h = this.ClientSize.Height;
                    var y = h - (int)(h * value + 0.5f);

                    if (cursor.Y != y)
                        OnCursorMoving(cursor.X, y);

                    if (AlphaChanged != null)
                        AlphaChanged(this, value);
                }
            }
        }

        private bool _AllowAlphaTransparency;
        public bool AllowAlphaTransparency
        {
            get
            {
                return _AllowAlphaTransparency;
            }
            set
            {
                if (_AllowAlphaTransparency != value)
                {
                    _AllowAlphaTransparency = value;
                    redrawBackground = true;
                    OnRedrawRequired();
                }
            }
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);

            if (disposing)
            {
                if (checkers != null)
                {
                    checkers.Dispose();
                    checkers = null;
                }
            }
        }
    }
}
