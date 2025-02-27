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
    class AffinityDisplay : Control
    {
        public event EventHandler AffinityChanged;

        private int processors;
        private int cols;
        private bool resize;
        private Rectangle mouseRect;
        private bool enableState;

        public AffinityDisplay()
        {
            SetStyle(ControlStyles.AllPaintingInWmPaint, true);
            SetStyle(ControlStyles.UserPaint, true);
            SetStyle(ControlStyles.OptimizedDoubleBuffer, true);

            this.ForeColor = Color.White;

            processors = Environment.ProcessorCount;
            if (processors > 64)
                processors = 64;
        }

        protected override void OnSizeChanged(EventArgs e)
        {
            base.OnSizeChanged(e);
            resize = true;
            this.Invalidate();
        }

        private long _Affinity;
        public long Affinity
        {
            get
            {
                return _Affinity;
            }
            set
            {
                value = value & (long)(processors >= 64 ? ulong.MaxValue : ((ulong)1 << processors) - 1);

                if (_Affinity != value)
                {
                    _Affinity = value;
                    this.Invalidate();

                    if (AffinityChanged != null)
                        AffinityChanged(this, EventArgs.Empty);
                }
            }
        }

        private int _BoxSize = 16;
        [DefaultValue(16)]
        public int BoxSize
        {
            get
            {
                return _BoxSize;
            }
            set
            {
                if (_BoxSize != value)
                {
                    _BoxSize = value;
                    if (this.AutoSize)
                        PerformLayout();
                    this.Invalidate();
                }
            }
        }

        private int _BoxMargin = 2;
        [DefaultValue(2)]
        public int BoxMargin
        {
            get
            {
                return _BoxMargin;
            }
            set
            {
                if (_BoxMargin != value)
                {
                    _BoxMargin = value;
                    if (this.AutoSize)
                        PerformLayout();
                    this.Invalidate();
                }
            }
        }

        private bool _ReadOnly;
        [DefaultValue(false)]
        public bool ReadOnly
        {
            get
            {
                return _ReadOnly;
            }
            set
            {
                if (_ReadOnly != value)
                {
                    _ReadOnly = value;
                    this.Invalidate();
                }
            }
        }

        private int _Selected = -1;
        private int Selected
        {
            get
            {
                return _Selected;
            }
            set
            {
                if (value >= processors)
                    value = -1;

                if (_Selected != value)
                {
                    Invalidate(_Selected);

                    _Selected = value;

                    Invalidate(value);
                }
            }
        }

        [Browsable(true)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
        public override bool AutoSize
        {
            get
            {
                return base.AutoSize;
            }
            set
            {
                base.AutoSize = value;
            }
        }

        private void Invalidate(int index)
        {
            if (index == -1 || index > processors)
                return;

            var c = index % cols;
            var r = index / cols;

            this.Invalidate(new Rectangle(c * (_BoxSize + _BoxMargin), r * (_BoxSize + _BoxMargin), _BoxSize, _BoxSize));
        }

        protected override void ScaleControl(SizeF factor, BoundsSpecified specified)
        {
            base.ScaleControl(factor, specified);

            var scale = factor.Width;
            if (scale != 1f && specified == BoundsSpecified.All)
            {
                _BoxSize = (int)(_BoxSize * scale + 0.5f);
                _BoxMargin = (int)(_BoxMargin * scale + 0.5f);
            }
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);

            if (mouseRect.Contains(e.Location))
                return;

            var col = e.X / (_BoxSize + _BoxMargin);
            var row = e.Y / (_BoxSize + _BoxMargin);
            var i = col >= cols ? -1 : col + row * cols;

            mouseRect = new Rectangle(col * (_BoxSize + _BoxMargin), row * (_BoxSize + _BoxMargin), _BoxSize, _BoxSize);

            if (_Selected != i)
            {
                if (e.Button == System.Windows.Forms.MouseButtons.Left && !_ReadOnly)
                {
                    SetState(i, enableState);
                }

                Selected = i;
            }
        }

        protected override void OnMouseDown(MouseEventArgs e)
        {
            base.OnMouseDown(e);

            if (e.Button == System.Windows.Forms.MouseButtons.Left && !_ReadOnly)
            {
                enableState = _Selected == -1 || ((_Affinity >> _Selected) & 1) == 0;
                SetState(_Selected, enableState);
                Invalidate(_Selected);
            }
        }

        private void SetState(int index, bool value)
        {
            if (value)
                _Affinity = _Affinity | (long)((ulong)1 << index);
            else
                _Affinity = _Affinity & ~(long)((ulong)1 << index);

            if (AffinityChanged != null)
                AffinityChanged(this, EventArgs.Empty);
        }

        protected override void OnMouseLeave(EventArgs e)
        {
            base.OnMouseLeave(e);

            Selected = -1;
            mouseRect = Rectangle.Empty;
        }

        protected override void OnPaintBackground(PaintEventArgs e)
        {
            base.OnPaintBackground(e);
        }

        private Color _ForeColorSelected = Color.LightSteelBlue;
        [DefaultValue(typeof(Color), "LightSteelBlue")]
        public Color ForeColorSelected
        {
            get
            {
                return _ForeColorSelected;
            }
            set
            {
                if (_ForeColorSelected != value)
                {
                    _ForeColorSelected = value;
                    this.Invalidate();
                }
            }
        }

        private Color _ForeColorDisabled = Color.FromArgb(225, 225, 225);
        [DefaultValue(typeof(Color), "225, 225, 225")]
        public Color ForeColorDisabled
        {
            get
            {
                return _ForeColorDisabled;
            }
            set
            {
                if (_ForeColorDisabled != value)
                {
                    _ForeColorDisabled = value;
                    this.Invalidate();
                }
            }
        }

        [DefaultValue(typeof(Color), "White")]
        public override Color ForeColor
        {
            get
            {
                return base.ForeColor;
            }
            set
            {
                base.ForeColor = value;
            }
        }

        private Color _BorderColorHightlight = Color.Black;
        [DefaultValue(typeof(Color), "Black")]
        public Color BorderColorHightlight
        {
            get
            {
                return _BorderColorHightlight;
            }
            set
            {
                if (_BorderColorHightlight != value)
                {
                    _BorderColorHightlight = value;
                    this.Invalidate();
                }
            }
        }

        private Color _BorderColor = Color.Gray;
        [DefaultValue(typeof(Color), "Gray")]
        public Color BorderColor
        {
            get
            {
                return _BorderColor;
            }
            set
            {
                if (_BorderColor != value)
                {
                    _BorderColor = value;
                    this.Invalidate();
                }
            }
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);

            var g = e.Graphics;
            var enabled = this.Enabled;

            if (resize)
            {
                resize = false;
                cols = this.Width / (_BoxSize + _BoxMargin);
            }

            using (var brush = new SolidBrush(enabled ? this.ForeColor : this._ForeColorDisabled))
            {
                using (var pen = new Pen(_BorderColor))
                {
                    var state = false;
                    var col = 0;
                    var x = 0;
                    var y = 0;

                    for (var i = 0; i < processors; i++)
                    {
                        if (enabled)
                        {
                            var isUsed = ((_Affinity >> i) & 1) == 1;

                            if (state != isUsed)
                            {
                                state = isUsed;
                                brush.Color = isUsed ? _ForeColorSelected : this.ForeColor;
                            }

                            g.FillRectangle(brush, x + 1, y + 1, _BoxSize - 2, _BoxSize - 2);

                            if (_Selected == i && !_ReadOnly)
                            {
                                pen.Color = _BorderColorHightlight;
                                g.DrawRectangle(pen, x, y, _BoxSize - 1, _BoxSize - 1);
                                pen.Color = _BorderColor;
                            }
                            else
                            {
                                g.DrawRectangle(pen, x, y, _BoxSize - 1, _BoxSize - 1);
                            }
                        }
                        else
                        {
                            g.FillRectangle(brush, x + 1, y + 1, _BoxSize - 2, _BoxSize - 2);
                            g.DrawRectangle(pen, x, y, _BoxSize - 1, _BoxSize - 1);
                        }

                        if (++col == cols)
                        {
                            col = 0;
                            x = 0;
                            y += _BoxSize + _BoxMargin;
                        }
                        else
                            x += _BoxSize + _BoxMargin;
                    }
                }
            }
        }

        public override Size GetPreferredSize(Size proposed)
        {
            var sz = (_BoxSize + _BoxMargin);
            int w, h;

            if (proposed.Width > ushort.MaxValue || proposed.Width == 0)
            {
                w = processors * sz;
                h = sz;
            }
            else
            {
                var cols = proposed.Width / sz;
                if (cols > processors)
                    cols = processors;
                var rows = (processors - 1) / cols + 1;
                //try to fit equal columns per row
                if (rows > 1)
                    cols = (processors - 1) / rows + 1;

                w = cols * sz;
                h = rows * sz - _BoxMargin;
            }

            if (this.MinimumSize.Width > w)
            {
                w = this.MinimumSize.Width;
            }
            else if (this.MaximumSize.Width > 0 && this.MaximumSize.Width < w)
            {
                w = this.MaximumSize.Width;
            }

            if (this.MinimumSize.Height > h)
            {
                h = this.MinimumSize.Height;
            }
            else if (this.MaximumSize.Height > 0 && this.MaximumSize.Height < h)
            {
                h = this.MaximumSize.Height;
            }

            return new Size(w, h);
        }
    }
}
