using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Drawing;
using System.ComponentModel;
using System.Windows.Forms.Layout;

namespace Gw2Launcher.UI.Controls
{
    class AutoScrollContainerPanel : Panel
    {
        private class AutoScrollContainerPanelLayout : LayoutEngine
        {
            public override bool Layout(object container, LayoutEventArgs layoutEventArgs)
            {
                DoLayout((AutoScrollContainerPanel)container, false);

                return true;
            }

            public Size DoLayout(Panel panel, bool measureOnly)
            {
                var vscroll = panel.VerticalScroll.Visible;

                if (panel.AutoSize)
                {
                    int mw = panel.MinimumSize.Width,
                        mh = panel.MinimumSize.Height;

                    foreach (Control c in panel.Controls)
                    {
                        int r, b;

                        if (c.AutoSize)
                        {
                            var s = c.GetPreferredSize(new Size(int.MaxValue, int.MaxValue));
                            r = c.Left + s.Width + c.Margin.Right;
                            b = c.Top + s.Height + c.Margin.Bottom;
                        }
                        else
                        {
                            r = c.Right + c.Margin.Right;
                            b = c.Bottom + c.Margin.Bottom;
                        }

                        if (r > mw)
                            mw = r;
                        if (b > mh)
                            mh = b;
                    }

                    mw += panel.Width - panel.ClientSize.Width;
                    mh += panel.Height - panel.ClientSize.Height;

                    if (panel.MaximumSize.Width > 0 && panel.MaximumSize.Width < mw)
                        mw = panel.MaximumSize.Width;
                    if (panel.MaximumSize.Height > 0 && panel.MaximumSize.Height < mw)
                        mw = panel.MaximumSize.Height;

                    if (panel.AutoSizeMode == AutoSizeMode.GrowOnly)
                    {
                        if (panel.Width > mw)
                            mw = panel.Width;
                        if (panel.Height > mh)
                            mh = panel.Height;
                    }

                    if (measureOnly)
                        return new Size(mw, mh);

                    panel.Size = new Size(mw, mh);
                }
                else if (measureOnly)
                {
                    return panel.Size;
                }

                var w = panel.ClientSize.Width - panel.Padding.Horizontal;

                foreach (Control c in panel.Controls)
                {
                    var stretch = c.Anchor.HasFlag(AnchorStyles.Left | AnchorStyles.Right);
                    Size s;

                    if (c.AutoSize)
                    {
                        s = c.GetPreferredSize(new Size(w - c.Margin.Horizontal, int.MaxValue));

                        if (panel.AutoScroll && !vscroll && s.Height > panel.ClientSize.Height - c.Margin.Vertical - panel.Padding.Vertical)
                        {
                            w -= SystemInformation.VerticalScrollBarWidth;
                            vscroll = true;

                            s = c.GetPreferredSize(new Size(w - c.Margin.Horizontal, int.MaxValue));
                        }
                    }
                    else if (stretch)
                    {
                        s = new Size(0, c.Height);

                        if (panel.AutoScroll && !vscroll && s.Height > panel.ClientSize.Height - c.Margin.Vertical - panel.Padding.Vertical)
                        {
                            w -= SystemInformation.VerticalScrollBarWidth;
                            vscroll = true;
                        }
                    }
                    else
                    {
                        continue;
                    }

                    if (stretch)
                    {
                        var _w = w - c.Margin.Horizontal;
                        if (_w > s.Width)
                            s.Width = _w;
                    }

                    c.Size = s;
                }

                return panel.Size;
            }
        }

        public event EventHandler<bool> PreVisiblePropertyChanged;
        public event EventHandler<Size> PreSizePropertyChanged;

        private static readonly AutoScrollContainerPanelLayout layout = new AutoScrollContainerPanelLayout();

        private bool visible;

        public AutoScrollContainerPanel()
            : base()
        {
            this.AutoScroll = true;
        }

        public override LayoutEngine LayoutEngine
        {
            get
            {
                return layout;
            }
        }

        [DefaultValue(true)]
        public override bool AutoScroll
        {
            get
            {
                return base.AutoScroll;
            }
            set
            {
                base.AutoScroll = value;
            }
        }

        protected override void SetBoundsCore(int x, int y, int width, int height, BoundsSpecified specified)
        {
            if (PreSizePropertyChanged != null)
            {
                int w, h;

                if (specified == BoundsSpecified.None)
                {
                    w = width;
                    h = height;
                }
                else
                {
                    if (specified.HasFlag(BoundsSpecified.Width))
                        w = width;
                    else
                        w = this.Width;

                    if (specified.HasFlag(BoundsSpecified.Height))
                        h = height;
                    else
                        h = this.Height;
                }

                PreSizePropertyChanged(this, new Size(w, h));
            }

            base.SetBoundsCore(x, y, width, height, specified);
        }

        protected override void SetVisibleCore(bool value)
        {
            if (visible != value)
            {
                visible = value;
                if (PreVisiblePropertyChanged != null)
                    PreVisiblePropertyChanged(this, value);
            }

            base.SetVisibleCore(value);
        }

        public override Size GetPreferredSize(Size proposedSize)
        {
            return layout.DoLayout(this, true);
        }
    }
}
