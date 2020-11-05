using System;
using System.Windows.Forms;
using System.Drawing;
using System.Drawing.Drawing2D;

namespace Gw2Launcher.UI.ColorPicker.Controls
{
    public class ColorPreviewPanel : Panel
    {
        public event EventHandler Color1Changed, Color2Changed;

        private BufferedGraphics buffer;
        private Bitmap background;
        private bool redraw;

        public ColorPreviewPanel()
        {
            SetStyle(ControlStyles.AllPaintingInWmPaint, true);
            SetStyle(ControlStyles.UserPaint, true);

            redraw = true;
            _LabelAlignment = ContentAlignment.BottomRight;
        }

        private Color _Color1;
        public Color Color1
        {
            get
            {
                return _Color1;
            }
            set
            {
                if (_Color1 != value)
                {
                    _Color1 = value;
                    OnRedrawRequired();

                    if (Color1Changed != null)
                        Color1Changed(this, EventArgs.Empty);
                }
            }
        }

        private Color _Color2;
        public Color Color2
        {
            get
            {
                return _Color2;
            }
            set
            {
                if (_Color2 != value)
                {
                    _Color2 = value;
                    OnRedrawRequired();

                    if (Color2Changed != null)
                        Color2Changed(this, EventArgs.Empty);
                }
            }
        }

        private bool _ShowColor2;
        public bool ShowColor2
        {
            get
            {
                return _ShowColor2;
            }
            set
            {
                if (_ShowColor2 != value)
                {
                    _ShowColor2 = value;
                    OnRedrawRequired();
                }
            }
        }

        private byte _OffsetCheckers;
        public bool OffsetCheckers
        {
            get
            {
                return _OffsetCheckers == 1;
            }
            set
            {
                if (OffsetCheckers != value)
                {
                    _OffsetCheckers = value ? (byte)1 : (byte)0;
                    if (background != null)
                    {
                        background.Dispose();
                        background = null;
                        OnRedrawRequired();
                    }
                }
            }
        }

        private string _Label;
        public string Label
        {
            get
            {
                return _Label;
            }
            set
            {
                _Label = value;
                OnRedrawRequired();
            }
        }

        private System.Drawing.ContentAlignment _LabelAlignment;
        public System.Drawing.ContentAlignment LabelAlignment
        {
            get
            {
                return _LabelAlignment;
            }
            set
            {
                _LabelAlignment = value;
                OnRedrawRequired();
            }
        }

        private void OnRedrawRequired()
        {
            if (redraw)
                return;
            redraw = true;
            this.Invalidate();
        }

        protected override void OnSizeChanged(EventArgs e)
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
            OnRedrawRequired();

            base.OnSizeChanged(e);
        }

        protected override void OnPaintBackground(PaintEventArgs e)
        {
            if (redraw)
            {
                redraw = false;

                if (buffer == null)
                    buffer = BufferedGraphicsManager.Current.Allocate(e.Graphics, this.ClientRectangle);

                var w = this.ClientSize.Width;
                var h = this.ClientSize.Height;
                var drawBackground = _Color1.A < 255 || _ShowColor2 && _Color2.A < 255;

                if (drawBackground && background == null)
                {
                    background = new Bitmap(w, h);

                    using (var g = Graphics.FromImage(background))
                    {
                        g.Clear(Color.White);

                        var bs = 10 * g.DpiX / 96f; 
                        var rows = (int)(h / (float)bs + 0.999f);
                        var cols = (int)(w / (float)bs + 0.999f);

                        for (var r = 0; r < rows; r++)
                        {
                            var b = r % 2 == _OffsetCheckers;

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

                var bg = buffer.Graphics;

                if (drawBackground)
                {
                    bg.DrawImageUnscaled(background, 0, 0);
                }

                using (var brush = new SolidBrush(_Color1))
                {
                    if (_ShowColor2)
                    {
                        var s = h / 2;

                        bg.FillRectangle(brush, 0, 0, w, s);
                        brush.Color = _Color2;
                        bg.FillRectangle(brush, 0, s, w, h - s);
                    }
                    else
                    {
                        bg.FillRectangle(brush, 0, 0, w, h);
                    }
                }

                if (_Label != null)
                {
                    var cr = _Color1.R;
                    var cg = _Color1.G;
                    var cb = _Color1.B;

                    if (_Color1.A != 255)
                    {
                        var ca = _Color1.A / 255f;
                        var cbg = 255 * (1 - ca);

                        cr = (byte)(cr * ca + cbg);
                        cg = (byte)(cg * ca + cbg);
                        cb = (byte)(cb * ca + cbg);
                    }

                    var l = 0.299 * cr + 0.587 * cg + 0.114 * cb;

                    if (l <= 127)
                    {
                        TextRenderer.DrawText(bg, _Label, this.Font, Rectangle.Inflate(this.DisplayRectangle, 1, 1), Color.Black, Util.Text.GetAlignmentFlags(_LabelAlignment));
                    }
                    TextRenderer.DrawText(bg, _Label, this.Font, this.DisplayRectangle, l > 127 ? Color.Black : Color.WhiteSmoke, Util.Text.GetAlignmentFlags(_LabelAlignment));
                }
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
