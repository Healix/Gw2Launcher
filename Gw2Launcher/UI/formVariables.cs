using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Gw2Launcher.UI
{
    public partial class formVariables : Base.FlatBase
    {
        public event EventHandler<Client.Variables.Variable> VariableSelected;

        private List<Control> controls;

        public formVariables(IEnumerable<Client.Variables.Variable> variables)
            : base()
        {
            InitializeComponents();

            scrollV.ValueChanged += scrollV_ValueChanged;

            panelVariables.SuspendLayout();

            foreach (var v in variables)
            {
                var l = new Label()
                {
                    AutoSize = true,
                    Font = labelTemplate.Font,
                    ForeColor = labelTemplate.ForeColor,
                    Margin = labelTemplate.Margin,
                    Text = v.Name,
                    Cursor = labelTemplate.Cursor,
                    Padding = labelTemplate.Padding,
                    Anchor = labelTemplate.Anchor,
                    Tag = v,
                };

                l.MouseEnter += l_MouseEnter;
                l.MouseLeave += l_MouseLeave;
                l.Click += l_Click;

                panelVariables.Controls.Add(l);
            }

            panelVariables.ResumeLayout();

            panelVariables.VisibleChanged += panelVariables_VisibleChanged;
        }

        void scrollV_ValueChanged(object sender, int e)
        {
            panelVariables.Top = -e;
        }

        protected override void OnInitializeComponents()
        {
            base.OnInitializeComponents();

            InitializeComponent();

            this.ClientSize = Size.Empty;
        }

        void l_Click(object sender, EventArgs e)
        {
            if (VariableSelected != null)
            {
                var l = (Label)sender;
                var v = (Client.Variables.Variable)l.Tag;

                VariableSelected(this, v);
            }
        }

        void l_MouseLeave(object sender, EventArgs e)
        {
            var l = (Label)sender;
            l.BackColor = this.BackColor;
        }

        void l_MouseEnter(object sender, EventArgs e)
        {
            var l = (Label)sender;
            l.BackColor = Util.Color.Darken(this.BackColor, 0.05f);
        }

        void panelVariables_VisibleChanged(object sender, EventArgs e)
        {
            if (panelVariables.Visible)
            {
                var sz = panelVariables.GetPreferredSize(Size.Empty);
                var h = sz.Height;
                var w = sz.Width;

                if (sz.Height + panelVariablesContainer.Margin.Vertical > this.MaximumSize.Height)
                    h = this.MaximumSize.Height - panelVariablesContainer.Margin.Vertical;

                var m = panelVariables.Height - h;
                if (m > 0)
                {
                    scrollV.Maximum = m;
                    scrollV.Visible = true;

                    w += scrollV.Width + scrollV.Margin.Horizontal;
                }
                else
                {
                    scrollV.Visible = false;
                }

                panelVariables.Size = sz;

                this.MinimumSize = new Size(w + panelVariablesContainer.Margin.Horizontal, h + panelVariablesContainer.Margin.Vertical);
            }
        }

        protected override void OnMouseWheel(MouseEventArgs e)
        {
            base.OnMouseWheel(e);

            if (scrollV.Visible)
                scrollV.DoMouseWheel(e);
        }

        protected override void OnDeactivate(EventArgs e)
        {
            base.OnDeactivate(e);

            this.Close();
        }

        private void AttachControls(Control from)
        {
            controls = new List<Control>();

            while (from != null)
            {
                controls.Add(from);

                from.LocationChanged += control_LocationChanged;

                if (from is ScrollableControl)
                {
                    ((ScrollableControl)from).Scroll += control_Scroll;
                }

                from = from.Parent;
            }
        }

        void control_LocationChanged(object sender, EventArgs e)
        {
            this.Close();
        }

        void control_Scroll(object sender, ScrollEventArgs e)
        {
            this.Close();
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            base.OnFormClosing(e);

            if (controls != null)
            {
                foreach (var c in controls)
                {
                    c.LocationChanged -= control_LocationChanged;

                    if (c is ScrollableControl)
                    {
                        ((ScrollableControl)c).Scroll -= control_Scroll;
                    }
                }
                controls = null;
            }

            this.Dispose();
        }

        public void ShowAtCursor()
        {
            var l = Cursor.Position;
            var screen = Screen.FromPoint(l).WorkingArea;
            var mh = screen.Height / 2;

            this.MaximumSize = new Size(0, mh);

            this.SizeChanged += delegate
            {
                var x = l.X - Scale(10);
                var y = l.Y - Scale(10);

                if (x + this.Width > screen.Right)
                    x = screen.Right - this.Width;
                if (y + this.Height > screen.Bottom)
                    y = screen.Bottom - this.Height;

                this.Location = new Point(x, y);
            };
        }

        public async void CloseOnMouseLeave()
        {
            var rp = Rectangle.Inflate(new Rectangle(Cursor.Position, Size.Empty), Scale(20), Scale(20));
            var r = rp;

            do
            {
                try
                {
                    await Task.Delay(500);
                }
                catch (TaskCanceledException)
                {
                    break;
                }

                if (rp.Width > 0)
                {
                    if (!rp.Contains(Cursor.Position))
                    {
                        rp.Width = 0;
                        r = Rectangle.Inflate(this.Bounds, Scale(10), Scale(10));
                    }
                    else
                        continue;
                }
            }
            while (r.Contains(Cursor.Position));

            this.Close();
        }

        public void Show(Form owner, Control target, AnchorStyles anchor)
        {
            var l = target.PointToScreen(Point.Empty);
            var screen = Screen.FromControl(owner).WorkingArea;
            var mh = 0;

            if (anchor.HasFlag(AnchorStyles.Top))
            {
                mh = screen.Bottom - l.Y;
            }
            else if (anchor.HasFlag(AnchorStyles.Bottom))
            {
                mh = l.Y - screen.Top;
            }
            else
            {
                mh = screen.Height / 2;
            }

            this.MaximumSize = new Size(0, mh);

            this.SizeChanged += delegate
            {
                var p = target.PointToScreen(Point.Empty);

                if (anchor.HasFlag(AnchorStyles.Left))
                {
                }
                else if (anchor.HasFlag(AnchorStyles.Right))
                {
                    p.X += target.Width - this.Width;
                }
                else
                {
                    p.X += target.Width / 2 - this.Width / 2;
                }

                if (anchor.HasFlag(AnchorStyles.Top))
                {
                }
                else if (anchor.HasFlag(AnchorStyles.Bottom))
                {
                    p.Y += target.Height - this.Height;
                }
                else
                {
                    p.Y += target.Height / 2 - this.Height / 2;
                }

                this.Location = p;
            };

            AttachControls(target);

            this.Show(owner);
        }
    }
}
