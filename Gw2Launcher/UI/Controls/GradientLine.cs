using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.ComponentModel;

namespace Gw2Launcher.UI.Controls
{
    class GradientLine : Control
    {
        [Flags]
        public enum EndCapType
        {
            None = 0,
            Left = 1,
            Right = 2,
            LeftAndRight = 3,
        }

        public enum LineType
        {
            Fixed = 0,
            Scaled = 1,
        }

        public GradientLine()
        {
            SetStyle(ControlStyles.AllPaintingInWmPaint, true);
            SetStyle(ControlStyles.UserMouse, true);
            SetStyle(ControlStyles.ResizeRedraw, true);

            ForeColor = Color.FromArgb(225, 225, 225);
        }

        [DefaultValue(typeof(Color), "225,225,225")]
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

        private EndCapType _EndCap = EndCapType.Left | EndCapType.Right;
        [DefaultValue(EndCapType.Left | EndCapType.Right)]
        public EndCapType EndCap
        {
            get
            {
                return _EndCap;
            }
            set
            {
                _EndCap = value;
                this.Invalidate();
            }
        }

        private float _EndCapSize = 40f;
        /// <summary>
        /// Size in pixels if using fixed size, or % if scaled
        /// </summary>
        [DefaultValue(40f)]
        public float EndCapSize
        {
            get
            {
                return _EndCapSize;
            }
            set
            {
                _EndCapSize = value;
                this.Invalidate();
            }
        }

        private LineType _GradientLineType = LineType.Fixed;
        [DefaultValue(LineType.Fixed)]
        public LineType GradientLineType
        {
            get
            {
                return _GradientLineType;
            }
            set
            {
                _GradientLineType = value;
                this.Invalidate();
            }
        }

        protected override void OnPaintBackground(PaintEventArgs e)
        {
            var g = e.Graphics;

            if (_GradientLineType == LineType.Scaled)
            {
                Color[] colors;
                float[] positions;

                if (_EndCap == (EndCapType.Left | EndCapType.Right))
                {
                    colors = new Color[] { this.BackColor, this.ForeColor, this.ForeColor, this.BackColor };
                    positions = new float[] { 0f, _EndCapSize, 1 - _EndCapSize, 1f };
                }
                else if (_EndCap == EndCapType.Left)
                {
                    colors = new Color[] { this.BackColor, this.ForeColor, this.ForeColor };
                    positions = new float[] { 0f, _EndCapSize, 1f };
                }
                else if (_EndCap == EndCapType.Right)
                {
                    colors = new Color[] { this.ForeColor, this.ForeColor, this.BackColor };
                    positions = new float[] { 0f, 1 - _EndCapSize, 1f };
                }
                else
                {
                    colors = null;
                    positions = null;
                    g.Clear(this.ForeColor);
                    return;
                }

                using (var brush = new LinearGradientBrush(new Rectangle(0, 0, this.Width, this.Height), Color.Empty, Color.Empty, LinearGradientMode.Horizontal))
                {
                    brush.InterpolationColors = new ColorBlend(colors.Length)
                    {
                        Colors = colors,
                        Positions = positions,
                    };
                    g.FillRectangle(brush, 0, 0, this.Width, this.Height);
                }
            }
            else
            {
                var _EndCapSize = (int)(this._EndCapSize * g.DpiX / 96f + 0.5f);

                g.Clear(this.ForeColor);

                if ((_EndCap & EndCapType.Left) != 0)
                {
                    using (var brush = new LinearGradientBrush(new Rectangle(0, 0, _EndCapSize, this.Height), this.BackColor, this.ForeColor, LinearGradientMode.Horizontal))
                    {
                        brush.WrapMode = WrapMode.TileFlipX;
                        g.FillRectangle(brush, 0, 0, _EndCapSize, this.Height);
                    }
                }

                if ((_EndCap & EndCapType.Right) != 0)
                {
                    using (var brush = new LinearGradientBrush(new Rectangle(this.Width - _EndCapSize, 0, _EndCapSize, this.Height), this.ForeColor, this.BackColor, LinearGradientMode.Horizontal))
                    {
                        brush.WrapMode = WrapMode.TileFlipX;
                        g.FillRectangle(brush, this.Width - _EndCapSize, 0, _EndCapSize, this.Height);
                    }
                }
            }
        }
    }
}
