using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Gw2Launcher.UI.Controls
{
    [DefaultEvent("Click")]
    public partial class SidebarButton : UserControl
    {
        public event EventHandler SelectedChanged;

        private Pen penBorder;
        private string text;
        private Size textSize;
        private bool selected;
        private Color colorArrow, colorSelected;

        public SidebarButton()
        {
            InitializeComponent();

            SetStyle(ControlStyles.UserPaint, true);
            SetStyle(ControlStyles.AllPaintingInWmPaint, true);
            SetStyle(ControlStyles.OptimizedDoubleBuffer, true);

            colorArrow = SystemColors.Control;
            colorSelected = Color.FromArgb(240, 240, 240);

            penBorder = new Pen(new SolidBrush(SystemColors.WindowFrame));
        }

        public Color ArrowColor
        {
            get
            {
                return colorArrow;
            }
            set
            {
                colorArrow = value;
                this.Invalidate();
            }
        }

        public Color BorderColor
        {
            get
            {
                return penBorder.Color;
            }
            set
            {
                penBorder.Color = value;
                this.Invalidate();
            }
        }

        public Color SelectedColor
        {
            get
            {
                return colorSelected;
            }
            set
            {
                colorSelected = value;
                if (selected)
                    this.Invalidate();
            }
        }

        protected override void OnPaintBackground(PaintEventArgs e)
        {
            var g = e.Graphics;

            if (selected)
            {
                g.Clear(colorSelected);

                g.DrawLine(penBorder, 0, 0, this.Width - 1, 0);
                g.DrawLine(penBorder, 0, this.Height - 1, this.Width - 1, this.Height - 1);

                g.DrawLine(penBorder, this.Width - 1, 0, this.Width - 1, this.Height - 1);

                int h = this.Height / 2;
                int w = this.Width;

                if (h % 2 == 1)
                    h++;

                Point[] arrow = new Point[]
                {
                    new Point(w,h/2),
                    new Point(w-h / 2, h),
                    new Point(w, h + h/2)
                };
                g.FillPolygon(new SolidBrush(this.colorArrow), arrow);
                g.DrawLines(penBorder, arrow);
            }
            else
            {
                g.Clear(this.BackColor);
                g.DrawLine(penBorder, this.Width - 1, 0, this.Width - 1, this.Height - 1);
            }
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            var g = e.Graphics;

            if (textSize.IsEmpty)
            {
                textSize = TextRenderer.MeasureText(g, this.text, this.Font);
            }

            TextRenderer.DrawText(g, this.Text, this.Font, new Point(10, this.Height / 2 - textSize.Height / 2), this.ForeColor);
        }

        [Browsable(true)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
        public override string Text
        {
            get
            {
                return this.text;
            }
            set
            {
                if (this.text != value)
                {
                    this.text = value;
                    textSize = Size.Empty;
                    this.Invalidate();
                }
            }
        }

        public bool Selected
        {
            get
            {
                return this.selected;
            }
            set
            {
                if (this.selected != value)
                {
                    this.selected = value;
                    this.Invalidate();

                    if (SelectedChanged != null)
                        SelectedChanged(this, EventArgs.Empty);
                }
            }
        }

        private void SidebarButton_Load(object sender, EventArgs e)
        {

        }
    }
}
