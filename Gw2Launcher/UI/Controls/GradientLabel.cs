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
    class GradientLabel : Label
    {
        public GradientLabel()
        {
            SetStyle(ControlStyles.ResizeRedraw, true);
        }

        private Color _BackColorGradient = SystemColors.ControlLight;
        [DefaultValue(typeof(Color), "ControlLight")]
        public Color BackColorGradient
        {
            get
            {
                return _BackColorGradient;
            }
            set
            {
                _BackColorGradient = value;
                this.Invalidate();
            }
        }

        private float _GradientEndPercent = 0.75f;
        [DefaultValue(0.75f)]
        public float GradientEndPercent
        {
            get
            {
                return _GradientEndPercent;
            }
            set
            {
                _GradientEndPercent = value;
                this.Invalidate();
            }
        }

        protected override void OnPaintBackground(PaintEventArgs e)
        {
            base.OnPaintBackground(e);

            var w = (int)(this.Width * _GradientEndPercent);
            using (var brush = new System.Drawing.Drawing2D.LinearGradientBrush(new System.Drawing.Rectangle(0, 0, w, this.Height), _BackColorGradient, this.BackColor, System.Drawing.Drawing2D.LinearGradientMode.Horizontal))
            {
                e.Graphics.FillRectangle(brush, 0, 0, w, this.Height);
            }
        }
    }
}
