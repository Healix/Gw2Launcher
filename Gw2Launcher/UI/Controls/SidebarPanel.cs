using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Drawing;

namespace Gw2Launcher.UI.Controls
{
    class SidebarPanel : Panel
    {
        public event EventHandler<SidebarButton> Selected;

        private class ButtonAnimation
        {
            public SidebarButton button;
            public Animation animation;
        }

        private Color colorBorder;
        private Pen penBorder;
        private ButtonAnimation[] buttons;
        private Task taskAnimation;
        private int first, last;
        private SidebarButton selectedButton;
        private Panel selectedPanel;

        private class Animation
        {
            public Animation()
            {
                startAt = long.MaxValue;
            }

            public long startAt;
            public int from, to, duration, delay;
        }

        public SidebarPanel()
            : base()
        {
            colorBorder = SystemColors.WindowFrame;
            penBorder = new Pen(Color.Black);

            base.Disposed += SidebarPanel_Disposed;
        }

        public void Initialize(SidebarButton[] buttons)
        {
            if (this.buttons != null)
                throw new NotSupportedException();

            this.buttons = new ButtonAnimation[first = buttons.Length];

            int i = 0;
            foreach (var button in buttons)
            {
                this.buttons[i] = new ButtonAnimation()
                {
                    button = button,
                };

                button.Index = i++;

                button.BeginExpand += button_BeginExpand;
                button.BeginCollapse += button_BeginCollapse;
                button.Click += button_Click;
                button.SelectedChanged += button_SelectedChanged;
                button.SubitemSelected += button_SubitemSelected;
            }
        }

        private void OnPanelChanged(Panel panel)
        {
            var previous = selectedPanel;
            selectedPanel = panel;

            panel.Visible = true;

            if (previous != null)
                previous.Visible = false;
        }

        void button_SubitemSelected(object sender, int e)
        {
            SidebarButton button = (SidebarButton)sender;
            var panel = button.Panels[e];
            if (panel != selectedPanel)
                OnPanelChanged(panel);
        }

        void button_SelectedChanged(object sender, EventArgs e)
        {
            SidebarButton button = (SidebarButton)sender;

            if (button.Selected)
            {
                if (selectedButton != null)
                    selectedButton.Selected = false;
                selectedButton = button;

                OnPanelChanged(button.Panels[0]);

                if (Selected != null)
                    Selected(this, button);
            }
        }

        void button_Click(object sender, EventArgs e)
        {
            ((SidebarButton)sender).Selected = true;
        }

        void button_BeginCollapse(object sender, EventArgs e)
        {
            var button = (SidebarButton)sender;
            var i = button.Index;

            if (i < first)
                first = i;
            if (i > last)
                last = i;

            buttons[i].animation = new Animation()
            {
                from = button.Height,
                to = button.CollapsedHeight,
                duration = 1500000,
                delay = 500000,
            };

            Animate();
        }

        void button_BeginExpand(object sender, EventArgs e)
        {
            var button = (SidebarButton)sender;
            var i = button.Index;

            if (i < first)
                first = i;
            if (i > last)
                last = i;

            buttons[i].animation = new Animation()
            {
                from = button.Height,
                to = button.ExpandedHeight,
                duration = 1500000,
                delay = 500000,
            };

            Animate();
        }

        private void Animate()
        {
            if (taskAnimation == null || taskAnimation.IsCompleted)
            {
                if (taskAnimation != null)
                    taskAnimation.Dispose();
                taskAnimation = DoAnimation();
            }
        }

        private async Task DoAnimation()
        {
            const int LIMIT = 50;

            var t = Math.Sinh(Math.PI);
            var delay = int.MaxValue;
            int active = 0,
                delayed = 0;

            while (true)
            {
                if (delay != int.MaxValue && delayed == active)
                {
                    delay /= 10000;
                    if (delay > LIMIT)
                        delay = LIMIT;
                    await Task.Delay(delay);
                    delay = int.MaxValue;
                }
                else
                    await Task.Delay(10);


                var now = DateTime.UtcNow.Ticks;
                var changed = false;
                var y = buttons[first].button.Top;
                active = 0;

                for (int i = first, l = buttons.Length; i < l; i++)
                {
                    var b = buttons[i];
                    var h = b.button.Height;
                    var bs = BoundsSpecified.Y;

                    if (b.animation != null)
                    {
                        active++;

                        var a = b.animation;
                        if (now > a.startAt)
                        {
                            var elapsed = now - a.startAt;
                            var p = (double)elapsed / a.duration;
                            int h2;

                            if (p >= 1)
                            {
                                b.animation = null;

                                h2 = a.to;
                            }
                            else
                            {
                                p = Math.Sinh(Math.PI * p) / t;

                                h2 = a.from + (int)((a.to - a.from) * p);
                            }

                            if (h2 != h)
                            {
                                h = h2;
                                changed = true;

                                bs |= BoundsSpecified.Height;
                            }
                        }
                        else if (a.startAt == long.MaxValue)
                        {
                            a.startAt = now + a.delay;
                            if (a.delay < delay)
                                delay = a.delay;
                            delayed++;
                        }

                        if (i == last)
                        {
                            //this is the last animated button, break if nothing has changed
                            if (!changed)
                                break;
                            if (i < l - 1)
                            {
                                //if the change doesn't cause the next button to move, break after this
                                if (y + h == buttons[i + 1].button.Top)
                                    l = 0;
                            }
                        }
                    }

                    if (changed)
                        b.button.SetBounds(0, y, 0, h, bs);

                    y += h;
                }

                if (active == 0)
                {
                    first = buttons.Length;
                    last = 0;
                    return;
                }
            }
        }

        void SidebarPanel_Disposed(object sender, EventArgs e)
        {
            penBorder.Dispose();
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

        protected override void OnPaintBackground(PaintEventArgs e)
        {
            base.OnPaintBackground(e);

            var g = e.Graphics;
            g.DrawLine(penBorder, this.Width - 1, 0, this.Width - 1, this.Height - 1);
        }

        protected override void OnSizeChanged(EventArgs e)
        {
            this.Invalidate();

            base.OnSizeChanged(e);
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
        }
    }
}
