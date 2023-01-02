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
                var panel = (AutoScrollContainerPanel)container;

                DoLayout(panel, false);
                
                return panel.AutoSize;
            }

            public Size DoLayout(AutoScrollContainerPanel panel, bool measureOnly)
            {
                var flat = panel.UseFlatScrollBar && panel.FlatVScroll != null && !panel.DesignMode;
                var vscroll = flat ? panel.FlatVScroll.Visible : panel.VerticalScroll.Visible;
                var hscroll = flat ? false : panel.HorizontalScroll.Visible;

                if (panel.AutoSize)
                {
                    int mw = panel.MinimumSize.Width,
                        mh = panel.MinimumSize.Height;

                    foreach (Control c in panel.Controls)
                    {
                        if (flat && c is FlatVScrollBar)
                            continue;

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
                if (w <= 0)
                    return panel.Size;
                var h = panel.ClientSize.Height - panel.Padding.Vertical;
                var autoscroll = flat || panel.AutoScroll;
                var scrollW = flat ? panel.FlatVScroll.Width + panel.FlatVScroll.Margin.Horizontal : SystemInformation.VerticalScrollBarWidth;
                var scrollH = flat ? 0 : SystemInformation.HorizontalScrollBarHeight;
                var bottom = h;
                var _vscroll = vscroll;
                var _hscroll = hscroll;

                if (vscroll && flat)
                {
                    var c = panel.FlatVScroll;

                    w -= scrollW;
                    bottom -= c.Value;

                    c.Bounds = new Rectangle(w + c.Margin.Left, panel.Padding.Top + c.Margin.Top, c.Width, h - c.Margin.Vertical);
                }

                foreach (Control c in panel.Controls)
                {
                    if (flat && c is FlatVScrollBar || !c.Visible)
                        continue;

                    var cStretchW = (c.Anchor & (AnchorStyles.Left | AnchorStyles.Right)) == (AnchorStyles.Left | AnchorStyles.Right);
                    var cStretchH = (c.Anchor & (AnchorStyles.Top | AnchorStyles.Bottom)) == (AnchorStyles.Top | AnchorStyles.Bottom);
                    var cAutosized = c.AutoSize;
                    Size s;

                    if (cAutosized)
                    {
                        s = c.GetPreferredSize(new Size(w - c.Margin.Horizontal, int.MaxValue));
                    }
                    else if (cStretchW)
                    {
                        if (cStretchH)
                            s = c.MinimumSize;
                        else
                            s = new Size(c.MinimumSize.Width, c.Height);
                    }
                    else if (cStretchH)
                    {
                        s = new Size(c.Width, c.MinimumSize.Height);
                    }
                    else
                    {
                        s = c.Size;
                    }

                    if (autoscroll && !_hscroll && s.Width > w - c.Margin.Horizontal)
                    {
                        h -= scrollH;
                        _hscroll = true;
                    }

                    if (autoscroll && !_vscroll && s.Height > h - c.Margin.Vertical)
                    {
                        w -= scrollW;
                        _vscroll = true;

                        if (cAutosized)
                        {
                            s = c.GetPreferredSize(new Size(w - c.Margin.Horizontal, int.MaxValue));
                        }

                        if (!_hscroll && s.Width > w - c.Margin.Horizontal)
                        {
                            h -= scrollH;
                            _hscroll = true;
                        }
                    }

                    if (cStretchW)
                    {
                        var _w = w - c.Margin.Horizontal;
                        if (flat || _w > s.Width)
                            s.Width = _w;
                    }

                    if (cStretchH)
                    {
                        var _h = h - c.Margin.Vertical;
                        if (s.Height < _h)
                            s.Height = _h;
                    }

                    if (flat)
                    {
                        if (c.Left + s.Width > w)
                            s.Width = w - c.Left;

                        var b = c.Top + s.Height + c.Margin.Bottom;
                        if (b > bottom)
                        {
                            if (!vscroll)
                            {
                                panel.FlatVScroll.Visible = true;
                                return DoLayout(panel, measureOnly);
                            }

                            bottom = b;
                        }
                    }

                    c.Size = s;
                }

                if (flat)
                {
                    panel.FlatVScroll.Maximum = bottom + panel.FlatVScroll.Value - h - panel.Padding.Top;
                    _vscroll = panel.FlatVScroll.Maximum > 0;

                    if (_vscroll != vscroll)
                    {
                        panel.FlatVScroll.Visible = _vscroll;
                        return DoLayout(panel, measureOnly);
                    }
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
            this.SuspendLayout();

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
                if (value && !DesignMode && IsHandleCreated)
                {
                    UseFlatScrollBar = false;
                }
                base.AutoScroll = value;
            }
        }

        protected bool _UseFlatScrollBar;
        [DefaultValue(false)]
        public bool UseFlatScrollBar
        {
            get
            {
                return _UseFlatScrollBar;
            }
            set
            {
                if (_UseFlatScrollBar != value)
                {
                    _UseFlatScrollBar = value;

                    if (value)
                    {
                        FlatVScroll = new FlatVScrollBar()
                        {
                            Visible = false,
                            Margin = Padding.Empty,
                        };

                        if (!DesignMode)
                        {
                            FlatVScroll.ValueDifference += FlatVScroll_ValueDifference;
                            this.Controls.Add(FlatVScroll);
                        }
                    }
                    else
                    {
                        using (FlatVScroll)
                        {
                            FlatVScroll.Value = 0;
                            FlatVScroll = null;
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Computes the location of where the control is located within the panel
        /// </summary>
        public Point PointFromChild(Control child)
        {
            var p = child.Parent;
            var x = child.Left;
            var y = child.Top;

            while (p != null && p != this)
            {
                x += p.Left;
                y += p.Top;

                p = p.Parent;
            }

            if (p == null)
            {
                return Point.Empty;
            }

            return new Point(x, y);
        }

        public void ScrollIntoView(Control c)
        {
            if (UseFlatScrollBar)
            {
                var p = PointFromChild(c);
                var v = FlatVScroll;
                var y = p.Y + v.Value;

                if (y < 0 || y + c.Height > this.ClientSize.Height)
                {
                    v.Value = y - (this.ClientSize.Height - c.Height) / 2;
                }
            }
            else
            {
                base.ScrollControlIntoView(c);
            }
        }

        protected override void CreateHandle()
        {
            base.CreateHandle();

            if (_UseFlatScrollBar && !DesignMode)
            {
                this.AutoScroll = false;
            }

            this.ResumeLayout();
        }

        protected override void OnMouseWheel(MouseEventArgs e)
        {
            base.OnMouseWheel(e);

            if (FlatVScroll != null)
            {
                FlatVScroll.DoMouseWheel(e);
            }
        }

        void FlatVScroll_ValueDifference(object sender, int e)
        {
            this.SuspendLayout();

            foreach (Control c in this.Controls)
            {
                if (c is FlatVScrollBar)
                    continue;
                c.Top -= e;
            }

            this.ResumeLayout(false);
        }

        public FlatVScrollBar FlatVScroll
        {
            get;
            private set;
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
