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
        public event EventHandler BeginExpand;
        public event EventHandler BeginCollapse;
        public event EventHandler SelectedChanged;
        public event EventHandler<int> SubitemSelected;

        private Pen penBorder;
        private Size textSize;
        private Color colorArrow, colorSelected, colorSubitem;
        private bool isExpanded;
        private bool canExpand;

        private Label hovered;
        private Label selected;

        private Label label;
        private Label[] labels;
        private string[] subitems;
        private int height, width;

        private class Label
        {
            public int index;
            public string text;
            public Rectangle bounds;
            public bool hovered;
        }

        public SidebarButton()
        {
            label = new Label();
            Index = -1;

            InitializeComponent();

            SetStyle(ControlStyles.UserPaint, true);
            SetStyle(ControlStyles.AllPaintingInWmPaint, true);
            SetStyle(ControlStyles.OptimizedDoubleBuffer, true);

            colorArrow = SystemColors.Control;
            colorSelected = Color.FromArgb(240, 240, 240);
            colorSubitem = Color.FromArgb(75, 75, 75);

            penBorder = new Pen(new SolidBrush(SystemColors.WindowFrame));
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (components != null)
                    components.Dispose();
            }

            base.Dispose(disposing);

            if (disposing)
            {
                penBorder.Dispose();
            }
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
                if (selected != null)
                    this.Invalidate();
            }
        }

        public Color SubitemForeColor
        {
            get
            {
                return colorSubitem;
            }
            set
            {
                colorSubitem = value;
                if (isExpanded)
                    this.Invalidate();
            }
        }

        protected override void OnPaintBackground(PaintEventArgs e)
        {
            var g = e.Graphics;

            if (selected != null)
            {
                g.Clear(colorSelected);

                g.DrawLine(penBorder, 0, 0, this.Width - 1, 0);
                g.DrawLine(penBorder, 0, this.Height - 1, this.Width - 1, this.Height - 1);

                g.DrawLine(penBorder, this.Width - 1, 0, this.Width - 1, this.Height - 1);

                int h = this.height / 2;
                int w = this.Width;

                if (h % 2 == 1)
                    h++;

                int o = 0;

                o = selected.bounds.Y + selected.bounds.Height / 2 - h;

                Point[] arrow = new Point[]
                {
                    new Point(w,h/2 + o),
                    new Point(w-h / 2, h + o),
                    new Point(w, h + h/2 + o)
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
                textSize = TextRenderer.MeasureText(g, this.label.text, this.Font);
            }

            int h = this.height / 2;
            int x = 10;

            TextRenderer.DrawText(g, this.Text, this.Font, this.label.bounds, this.ForeColor, TextFormatFlags.VerticalCenter);

            if (isExpanded)
            {
                int i = 0;
                int fh = this.Font.Height;
                x += 0;
                var ch = (int)(fh * 1.5f);

                foreach (var label in labels)
                {
                    label.bounds = new Rectangle(10, h + fh + i * ch, this.width, ch);

                    Color color;
                    if (label.hovered)
                        color = this.ForeColor;
                    else
                        color = colorSubitem;

                    TextRenderer.DrawText(g, label.text, this.Font, label.bounds, color, TextFormatFlags.VerticalCenter);

                    i++;
                }
            }
        }

        [Browsable(true)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
        public override string Text
        {
            get
            {
                return label.text;
            }
            set
            {
                if (label.text != value)
                {
                    label.text = value;
                    textSize = Size.Empty;
                    this.Invalidate();
                }
            }
        }

        public void SelectSubItem(int index)
        {
            if (this.selected == null)
                this.Selected = true;

            selected = labels[index];
            this.Invalidate();

            if (SubitemSelected != null)
                SubitemSelected(this, selected.index);
        }

        public void SelectPanel(Panel panel)
        {
            var i = GetIndex(panel);
            if (i != -1)
            {
                if (this.selected == null)
                    this.Selected = true;

                if (i > 0)
                {
                    selected = labels[i - 1];
                }
                else
                {
                    selected = label;
                }

                this.Invalidate();

                if (SubitemSelected != null)
                    SubitemSelected(this, selected.index);
            }
        }

        private int GetIndex(Panel panel)
        {
            int i = 0;
            foreach (var p in this.Panels)
            {
                if (p == panel)
                {
                    return i;
                }
                i++;
            }
            return -1;
        }

        /// <summary>
        /// Selects an unlabeled panel while showing the parent as selected
        /// </summary>
        public void SelectPanel(Panel parent, Panel child)
        {
            var i = GetIndex(parent);
            if (i != -1)
            {
                var j = GetIndex(child);
                if (j > 0)
                {
                    if (this.selected == null)
                        this.Selected = true;

                    if (i > 0)
                    {
                        selected = labels[i - 1];
                    }

                    selected = new Label()
                    {
                        bounds = selected.bounds,
                        index = selected.index,
                    };

                    if (SubitemSelected != null)
                        SubitemSelected(this, j);
                }
            }
        }

        public bool Selected
        {
            get
            {
                return this.selected != null;
            }
            set
            {
                if (value)
                {
                    if (selected != null)
                        return;
                    else
                        selected = label;
                }
                else
                    selected = null;

                if (canExpand)
                {
                    if (value)
                        Expand();
                    else
                        Shrink();
                }

                this.Invalidate();

                if (SelectedChanged != null)
                    SelectedChanged(this, EventArgs.Empty);
            }
        }

        public string[] SubItems
        {
            get
            {
                return subitems;
            }
            set
            {
                this.subitems = value;
                if (value == null)
                {
                    this.labels = null;
                    this.canExpand = false;
                }
                else
                {
                    int l = value.Length;
                    this.labels = new Label[l];
                    this.canExpand = l > 0;

                    for (var i = 0; i < l; i++)
                    {
                        this.labels[i] = new Label()
                        {
                            text = value[i],
                            index = i + 1
                        };
                    }
                }
            }
        }

        public int ExpandedHeight
        {
            get
            {
                var fh = this.Font.Height;
                var ch = (int)(fh * 1.5f);

                return this.height + fh / 2 + this.subitems.Length * ch;
            }
        }

        public int CollapsedHeight
        {
            get
            {
                return this.height;
            }
        }

        public int Index
        {
            get;
            set;
        }

        public Panel[] Panels
        {
            get;
            set;
        }

        protected void Expand()
        {
            this.isExpanded = true;

            if (BeginExpand != null)
            {
                BeginExpand(this, EventArgs.Empty);
            }
            else
            {
                this.Height = this.ExpandedHeight;
            }
        }

        protected void Shrink()
        {
            if (BeginCollapse != null)
            {
                BeginCollapse(this, EventArgs.Empty);
            }
            else
            {
                this.isExpanded = false;
                this.Height = this.height;
            }
        }

        protected override void OnSizeChanged(EventArgs e)
        {
            base.OnSizeChanged(e);
            if (!isExpanded)
            {
                this.height = this.Height;
                this.width = this.Width;
                label.bounds = new Rectangle(10, 0, this.Width - 10, this.Height);
            }
            else if (this.Height == this.height)
                this.isExpanded = false;
            this.Invalidate();
        }

        protected override void OnMouseUp(MouseEventArgs e)
        {
            base.OnMouseUp(e);

            if (isExpanded && e.Button == System.Windows.Forms.MouseButtons.Left && selected != null)
            {
                if (hovered != null)
                {
                    if (selected != hovered)
                    {
                        selected = hovered;
                        this.Invalidate();

                        if (SubitemSelected != null)
                            SubitemSelected(this, selected.index);

                    }
                }
                else if (selected != label)
                {
                    selected = label;
                    this.Invalidate();

                    if (SubitemSelected != null)
                        SubitemSelected(this, label.index);
                }
            }
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);

            if (isExpanded)
            {
                var b = this.labels[0].bounds;

                if (hovered != null)
                {
                    if (hovered.bounds.Contains(e.Location))
                    {
                        return;
                    }
                    else
                    {
                        hovered.hovered = false;
                        hovered = null;
                        this.Invalidate();
                    }
                }

                if (e.X >= b.X && e.Y >= b.Y)
                {
                    foreach (var l in this.labels)
                    {
                        if (l.bounds.Contains(e.Location))
                        {
                            l.hovered = true;
                            hovered = l;
                            this.Invalidate();

                            break;
                        }
                    }
                }
            }
        }

        protected override void OnMouseLeave(EventArgs e)
        {
            base.OnMouseLeave(e);

            if (hovered != null)
            {
                hovered.hovered = false;
                hovered = null;
                this.Invalidate();
            }
        }

        private void SidebarButton_Load(object sender, EventArgs e)
        {

        }
    }
}
