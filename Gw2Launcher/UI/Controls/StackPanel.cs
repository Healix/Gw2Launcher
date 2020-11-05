using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Drawing;
using System.Windows.Forms.Layout;
using System.ComponentModel;

namespace Gw2Launcher.UI.Controls
{
    /// <summary>
    /// Lays out controls horizontally or vertically.
    /// 
    /// Special anchoring:
    /// Left|Right - stretches to fill the remaining horizontal space
    /// Top|Bottom - stretches to fill the vertical space
    /// 
    /// Special docking:
    /// Fill - fills the remaining horizontal space, but only up to the width of the control (horizontal only)
    /// Left/Right - floats (vertical only)
    /// </summary>
    class StackPanel : FlowLayoutPanel
    {
        private class StackPanelLayout : LayoutEngine
        {
            public class Cache
            {
                public Rectangle[] bounds;
                public Size size;
                public Size proposed;

                public void Invalidate()
                {
                    size = Size.Empty;
                    proposed = Size.Empty;
                }
            }

            public override bool Layout(object container, LayoutEventArgs args)
            {
                var panel = (StackPanel)container;
                var size = panel.ClientSize;

                if (args == null || args.AffectedControl != container)
                {
                    panel.cache.Invalidate();

                    if (panel.AutoSize && panel.AutoSizeFill == AutoSizeFillMode.Width)
                    {
                        var parent = panel.Parent;
                        if (parent != null)
                        {
                            var w = parent.ClientSize.Width - parent.Padding.Horizontal;
                            if (w < size.Width)
                                size = new Size(w, size.Height);
                        }
                    }
                }
                else if (panel.cache.size == panel.ClientSize)
                {
                    var bounds = panel.cache.bounds;
                    var count = panel.Controls.Count;
                    var ok = bounds != null && bounds.Length == count;

                    if (ok)
                    {
                        for (var i = 0; i < count; i++)
                        {
                            var c = panel.Controls[i];

                            if (bounds[i].IsEmpty)
                            {
                                if (c.Visible)
                                {
                                    ok = false;
                                    break;
                                }
                            }
                            else
                            {
                                if (bounds[i] != c.Bounds || !c.Visible)
                                {
                                    ok = false;
                                    break;
                                }
                            }
                        }
                    }

                    if (ok)
                    {
                        return false;
                    }
                    else
                    {
                        panel.cache.Invalidate();
                    }
                }

                var s = DoLayout(panel, size, true, panel.cache.proposed.IsEmpty, panel.AutoSize, false);

                if (panel.AutoSize)
                {
                    if (panel.ClientSize != s)
                    {
                        //panel.ClientSize = s;

                        return true;
                    }
                }

                return false;
            }

            private bool IsAutoSized(Control c)
            {
                if (c.AutoSize)
                {
                    if (c is TextBox)
                        return false;
                    return true;
                }
                return false;
            }

            public Size DoLayout(StackPanel panel, Size proposed, bool apply, bool measure, bool autosize, bool force)
            {
                var parent = panel.Parent;

                bool cached;

                if (force)
                {
                    cached = false;
                    force = !panel.Visible;
                }
                else
                {
                    if (!panel.Visible || parent == null)
                        return panel.Size;

                    if (panel.cache.proposed == proposed)
                    {
                        if (panel.cache.size == proposed)
                        {
                            return proposed;
                        }

                        cached = true;
                    }
                    else
                    {
                        cached = false;
                    }
                }

                var count = panel.Controls.Count;
                var bounds = panel.cache.bounds;
                if (bounds == null || bounds.Length != count)
                {
                    panel.cache.bounds = bounds = new Rectangle[count];
                    cached = false;
                }

                if (cached && apply)
                {
                    for (var i = 0; i < count; ++i)
                    {
                        var c = panel.Controls[i];
                        if (!c.Visible)
                        {
                            if (!bounds[i].Size.IsEmpty)
                            {
                                cached = false;
                                break;
                            }
                            continue;
                        }

                        c.Bounds = bounds[i];
                    }

                    if (cached)
                    {
                        return panel.cache.size = panel.cache.proposed;
                    }
                }

                int x,
                    y;
                int w = proposed.Width,
                    h = proposed.Height;
                int minimumWidth = panel.MinimumSize.Width,
                    minimumHeight = panel.MinimumSize.Height;
                int maximumWidth = panel.MaximumSize.Width,
                    maximumHeight = panel.MaximumSize.Height;
                var direction = panel.FlowDirection;

                if (maximumWidth == 0)
                    maximumWidth = int.MaxValue;
                if (maximumHeight == 0)
                    maximumHeight = int.MaxValue;

                if (w == 0)
                {
                    w = maximumWidth;
                }
                else if (w < ushort.MaxValue)
                {
                    w -= panel.Padding.Right;
                    minimumWidth = w;
                }
                else if (w > maximumWidth)
                {
                    w = maximumWidth;
                }

                if (h == 0)
                {
                    h = maximumHeight;
                }
                else if (h < ushort.MaxValue)
                {
                    h -= panel.Padding.Bottom;
                    minimumHeight = h;
                }
                else if (h > maximumHeight)
                {
                    h = maximumHeight;
                }

                if (panel.AutoSizeFill == AutoSizeFillMode.NoWrap)
                {
                    if (panel.MaximumSize.Width > 0)
                        w = panel.MaximumSize.Width - panel.Padding.Right;
                    else
                        w = int.MaxValue;
                }

                if (autosize)
                {
                    if (panel.AutoSizeFill != AutoSizeFillMode.Width && !panel.Anchor.HasFlag(AnchorStyles.Left | AnchorStyles.Right))
                        minimumWidth = panel.MinimumSize.Width;
                    if (!panel.Anchor.HasFlag(AnchorStyles.Top | AnchorStyles.Bottom))
                        minimumHeight = panel.MinimumSize.Height;

                    if (panel.AutoSizeMode != AutoSizeMode.GrowAndShrink)
                    {
                        var _w = panel.Width - panel.Padding.Right;
                        var _h = panel.Height - panel.Padding.Bottom;

                        if (_w > minimumWidth)
                        {
                            minimumWidth = _w;
                            if (_w > w)
                                w = _w;
                        }
                        if (_h > minimumHeight)
                        {
                            minimumHeight = _h;
                            if (_h > h)
                                h = _h;
                        }
                    }
                }
                else
                {
                    if (w == int.MaxValue)
                        w = panel.Width - panel.Padding.Right;
                    if (h == int.MaxValue)
                        h = panel.Height - panel.Padding.Bottom;
                    minimumWidth = w;
                    minimumHeight = h;
                }

                do
                {
                    x = panel.Padding.Left;
                    y = panel.Padding.Top;

                    int i;

                    for (i = 0; i < count; ++i)
                    {
                        var c = panel.Controls[i];
                        if (!force && !c.Visible)
                        {
                            bounds[i] = Rectangle.Empty;
                            continue;
                        }

                        Point l;
                        Size s;

                        if (cached)
                        {
                            l = new Point(bounds[i].X, bounds[i].Y);
                            s = new Size(bounds[i].Width, bounds[i].Height);
                        }
                        else
                        {
                            l = new Point(x + c.Margin.Left, y + c.Margin.Top);

                            if (c.Anchor.HasFlag(AnchorStyles.Left | AnchorStyles.Right) || c.Dock == DockStyle.Fill)
                            {
                                s = c.MinimumSize;

                                if (!IsAutoSized(c))
                                {
                                    if (!c.Anchor.HasFlag(AnchorStyles.Left | AnchorStyles.Right) && c.Dock != DockStyle.Fill)
                                    {
                                        s.Width = c.Width;
                                    }
                                    if (!c.Anchor.HasFlag(AnchorStyles.Top | AnchorStyles.Bottom))
                                    {
                                        s.Height = c.Height;
                                    }
                                }
                            }
                            else if (IsAutoSized(c))
                            {
                                var _w = w - l.X - c.Margin.Right;

                                if (direction == FlowDirection.LeftToRight && maximumWidth < ushort.MaxValue)
                                {
                                    for (var j = i + 1; j < count; j++)
                                    {
                                        var _c = panel.Controls[j];

                                        if (!force && !_c.Visible)
                                            continue;

                                        if (IsAutoSized(_c))
                                        {
                                            _w -= _c.MinimumSize.Width + _c.Margin.Horizontal;
                                        }
                                        else
                                        {
                                            _w -= _c.Width + _c.Margin.Horizontal;
                                        }
                                    }

                                    if (_w < 0)
                                        _w = 0;
                                }

                                s = new Size(_w, int.MaxValue);
                                if (force && c is StackPanel)
                                    s = ((StackPanel)c).GetPreferredSize(s, true);
                                else
                                    s = c.GetPreferredSize(s);
                            }
                            else
                            {
                                s = c.Size;
                            }

                            bounds[i] = new Rectangle(l, s);
                        }

                        if (direction == FlowDirection.TopDown)
                        {
                            if (autosize)
                            {
                                var r = l.X + s.Width + c.Margin.Right;

                                if (r > maximumWidth)
                                    r = maximumWidth;

                                if (r > minimumWidth)
                                {
                                    minimumWidth = r;
                                    if (minimumWidth > w)
                                    {
                                        w = minimumWidth;
                                        break;
                                    }
                                }
                            }

                            if (c.Dock == DockStyle.None)
                            {
                                y += s.Height + c.Margin.Vertical;
                            }
                        }
                        else
                        {
                            if (c.Anchor.HasFlag(AnchorStyles.Top | AnchorStyles.Bottom))
                            {
                                s.Height = c.MinimumSize.Height;
                            }

                            if (autosize)
                            {
                                var b = l.Y + s.Height + c.Margin.Bottom;

                                if (b > maximumHeight)
                                    b = maximumHeight;

                                if (b > minimumHeight)
                                {
                                    minimumHeight = b;
                                    if (minimumHeight > h)
                                    {
                                        h = minimumHeight;
                                        //break;
                                    }
                                }
                            }

                            if (c.Dock != DockStyle.Left)
                            {
                                x += s.Width + c.Margin.Horizontal;
                            }
                        }
                    }

                    if (i < count)
                        continue;

                    if (autosize)
                    {
                        switch (direction)
                        {
                            case FlowDirection.BottomUp:
                            case FlowDirection.TopDown:

                                if (y > minimumHeight)
                                {
                                    if (y > maximumHeight)
                                        minimumHeight = maximumHeight;
                                    else
                                        minimumHeight = y;
                                }

                                break;
                            case FlowDirection.LeftToRight:
                            case FlowDirection.RightToLeft:

                                if (x > minimumWidth)
                                {
                                    if (x > maximumWidth)
                                        minimumWidth = maximumWidth;
                                    else
                                        minimumWidth = x;
                                }

                                break;
                        }
                    }

                    if (minimumWidth == 0 && autosize)
                    {
                        for (i = 0; i < count; ++i)
                        {
                            var c = panel.Controls[i];
                            if (!force && !c.Visible)
                                continue;

                            int r;

                            if (bounds[i].Width == 0)
                            {
                                if (IsAutoSized(c))
                                {
                                    var s = new Size(w - bounds[i].X - c.Margin.Right, int.MaxValue);
                                    if (force && c is StackPanel)
                                        s = ((StackPanel)c).GetPreferredSize(s, true);
                                    else
                                        s = c.GetPreferredSize(s);
                                    r = bounds[i].X + s.Width + c.Margin.Right;
                                }
                                else
                                {
                                    r = c.Right + c.Margin.Right;
                                }
                            }
                            else
                            {
                                r = bounds[i].Right + c.Margin.Right;
                            }

                            if (r > maximumWidth)
                            {
                                minimumWidth = maximumWidth;
                                break;
                            }

                            if (r > minimumWidth)
                                minimumWidth = r;
                        }
                    }

                    for (i = 0; i < count; ++i)
                    {
                        var c = panel.Controls[i];
                        if (!force && !c.Visible)
                            continue;

                        if (c.Dock == DockStyle.None)
                        {
                            if (direction == FlowDirection.TopDown)
                            {
                                switch (c.Anchor & (AnchorStyles.Left | AnchorStyles.Right))
                                {
                                    //right aligned
                                    case AnchorStyles.Right:

                                        bounds[i].X = minimumWidth - bounds[i].Width - c.Margin.Right;

                                        break;
                                    //stretch horizontally
                                    case AnchorStyles.Left | AnchorStyles.Right:

                                        if (IsAutoSized(c))
                                        {
                                            var r = bounds[i].X + c.Margin.Right;
                                            var _w = minimumWidth - r;
                                            var s = new Size(_w, int.MaxValue);

                                            if (force && c is StackPanel)
                                                s = ((StackPanel)c).GetPreferredSize(s, true);
                                            else
                                                s = c.GetPreferredSize(s);

                                            if (s.Width < _w)
                                                s.Width = _w;

                                            r += s.Width;

                                            var _hdiff = s.Height - bounds[i].Height;

                                            for (var j = i + 1; j < count; ++j)
                                            {
                                                bounds[j].Y += _hdiff;
                                            }

                                            y += _hdiff;
                                            if (autosize)
                                            {
                                                if (y > maximumHeight)
                                                    minimumHeight = maximumHeight;
                                                else
                                                    minimumHeight = y;
                                            }

                                            bounds[i].Size = s;

                                            if (autosize)
                                            {
                                                if (r > maximumWidth)
                                                    r = maximumWidth;

                                                if (r > minimumWidth)
                                                {
                                                    minimumWidth = r;

                                                    if (minimumWidth > w)
                                                    {
                                                        w = minimumWidth;
                                                        i = int.MaxValue - 1;
                                                    }
                                                    else if (i > 0)
                                                    {
                                                        i = -1;
                                                    }

                                                    continue;
                                                }
                                            }
                                        }
                                        else
                                        {
                                            bounds[i].Size = new Size(minimumWidth - bounds[i].X - c.Margin.Right, bounds[i].Height);
                                        }

                                        break;
                                    //centered horizontally
                                    case AnchorStyles.None:

                                        bounds[i].X = panel.Padding.Left + ((minimumWidth - panel.Padding.Left) - bounds[i].Width) / 2 + c.Margin.Left - c.Margin.Right;

                                        break;
                                }

                                switch (c.Anchor & (AnchorStyles.Top | AnchorStyles.Bottom))
                                {
                                    case AnchorStyles.Top | AnchorStyles.Bottom:
                                        
                                        var _h = minimumHeight - bounds[i].Y - c.Margin.Bottom;

                                        for (var j = i + 1; j < count; ++j)
                                        {
                                            var _c = panel.Controls[j];
                                            if (!force && !_c.Visible)
                                                continue;
                                            _h -= _c.Margin.Vertical + bounds[j].Height;
                                        }

                                        var _hdiff = _h - bounds[i].Height;
                                        if (_hdiff > 0)
                                        {
                                            for (var j = i + 1; j < count; ++j)
                                            {
                                                bounds[j].Y += _hdiff;
                                            }
                                        }

                                        bounds[i].Height = _h;

                                        break;
                                    case AnchorStyles.Bottom:

                                        bounds[i].Y = minimumHeight - bounds[i].Height - c.Margin.Bottom;

                                        break;
                                }
                            }
                            else
                            {
                                switch (c.Anchor & (AnchorStyles.Left | AnchorStyles.Right))
                                {
                                    //right aligned
                                    case AnchorStyles.Right:

                                        bounds[i].X = minimumWidth - bounds[i].Width - c.Margin.Right;

                                        break;
                                    //stretch to fill horizontal space
                                    case AnchorStyles.Left | AnchorStyles.Right:

                                        var _wdiff = minimumWidth - x;

                                        if (_wdiff > 0)
                                        {
                                            bounds[i].Width += _wdiff;
                                            x = minimumWidth;

                                            for (var j = i + 1; j < count; ++j)
                                            {
                                                bounds[j].X += _wdiff;
                                            }
                                        }

                                        if (IsAutoSized(c))
                                        {
                                            var s = new Size(bounds[i].Width, int.MaxValue);
                                            if (force && c is StackPanel)
                                                s = ((StackPanel)c).GetPreferredSize(s, true);
                                            else
                                                s = c.GetPreferredSize(s);

                                            if (s.Height > bounds[i].Height)
                                            {
                                                bounds[i].Height = s.Height;

                                                if (autosize)
                                                {
                                                    var b = bounds[i].Y + s.Height + c.Margin.Bottom;

                                                    if (b > maximumHeight)
                                                        b = maximumHeight;

                                                    if (b > minimumHeight)
                                                    {
                                                        minimumHeight = b;

                                                        if (minimumHeight > h)
                                                        {
                                                            h = minimumHeight;
                                                            i = int.MaxValue - 1;
                                                        }
                                                        else if (i > 0)
                                                        {
                                                            i = -1;
                                                        }

                                                        continue;
                                                    }
                                                }
                                            }
                                        }

                                        break;
                                }

                                switch (c.Anchor & (AnchorStyles.Top | AnchorStyles.Bottom))
                                {
                                    //bottom aligned
                                    case AnchorStyles.Bottom:

                                        bounds[i].Y = minimumHeight - bounds[i].Height - c.Margin.Bottom;

                                        break;
                                    //stretch vertically
                                    case AnchorStyles.Top | AnchorStyles.Bottom:

                                        bounds[i].Height = minimumHeight - bounds[i].Y - c.Margin.Bottom;

                                        break;
                                    //centered vertically
                                    case AnchorStyles.None:

                                        bounds[i].Y = panel.Padding.Top + ((minimumHeight - panel.Padding.Top) - bounds[i].Height) / 2 + c.Margin.Top - c.Margin.Bottom;

                                        break;
                                }
                            }
                        }
                        else
                        {
                            switch (c.Dock)
                            {
                                case DockStyle.Right:

                                    bounds[i].X = minimumWidth - bounds[i].Width - c.Margin.Right;

                                    break;
                                case DockStyle.Fill:

                                    if (direction == FlowDirection.LeftToRight) //similar to left-right anchoring, except the control is only stretched if needed
                                    {
                                        var _wdiff = minimumWidth - x;
                                        int _w;

                                        if (IsAutoSized(c))
                                        {
                                            var s = new Size(int.MaxValue, int.MaxValue);
                                            if (force && c is StackPanel)
                                                s = ((StackPanel)c).GetPreferredSize(s, true);
                                            else
                                                s = c.GetPreferredSize(s);

                                            if (s.Height > bounds[i].Height)
                                            {
                                                bounds[i].Height = s.Height;

                                                if (autosize)
                                                {
                                                    var b = bounds[i].Y + s.Height + c.Margin.Bottom;

                                                    if (b > maximumHeight)
                                                        b = maximumHeight;

                                                    if (b > minimumHeight)
                                                    {
                                                        minimumHeight = b;

                                                        if (minimumHeight > h)
                                                        {
                                                            h = minimumHeight;
                                                            i = int.MaxValue - 1;
                                                        }
                                                        else if (i > 0)
                                                        {
                                                            i = -1;
                                                        }

                                                        continue;
                                                    }
                                                }
                                            }

                                            _w = s.Width;
                                        }
                                        else
                                        {
                                            _w = c.Width;
                                        }

                                        if (_w - bounds[i].Width < _wdiff)
                                        {
                                            _wdiff = _w - bounds[i].Width;
                                        }

                                        bounds[i].Y = panel.Padding.Top + ((minimumHeight - panel.Padding.Top) - bounds[i].Height) / 2 + c.Margin.Top - c.Margin.Bottom;

                                        if (_wdiff > 0)
                                        {
                                            bounds[i].Width += _wdiff;
                                            x += _wdiff;

                                            for (var j = i + 1; j < count; ++j)
                                            {
                                                bounds[j].X += _wdiff;
                                            }
                                        }
                                    }

                                    break;
                            }
                        }
                    }

                    if (i == count)
                        break;
                }
                while (true);

                if (apply)
                {
                    for (var i = 0; i < count; ++i)
                    {
                        var c = panel.Controls[i];
                        if (!force && !c.Visible)
                            continue;
                        c.Bounds = bounds[i];
                    }
                }

                panel.cache.proposed = new Size(minimumWidth + panel.Padding.Right, minimumHeight + panel.Padding.Bottom);
                if (apply)
                    panel.cache.size = panel.cache.proposed;
                return panel.cache.proposed;
            }
        }

        public enum AutoSizeFillMode
        {
            None,
            /// <summary>
            /// Expands to fill the width of the parent
            /// </summary>
            Width,
            /// <summary>
            /// Expands to the widest control
            /// </summary>
            NoWrap
        }

        private static readonly StackPanelLayout layout = new StackPanelLayout();

        private AutoSizeFillMode fillMode;
        private StackPanelLayout.Cache cache;

        public StackPanel()
        {
            this.SuspendLayout();

            cache = new StackPanelLayout.Cache();

            base.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            base.FlowDirection = FlowDirection.TopDown;
            base.WrapContents = false;

            this.ResumeLayout();
        }

        [DefaultValue(false)]
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

        [DefaultValue(FlowDirection.TopDown)]
        public new FlowDirection FlowDirection
        {
            get
            {
                return base.FlowDirection;
            }
            set
            {
                switch (value)
                {
                    case System.Windows.Forms.FlowDirection.TopDown:
                    case System.Windows.Forms.FlowDirection.LeftToRight:

                        base.FlowDirection = value;

                        break;
                    default:

                        throw new NotSupportedException();
                }
            }
        }

        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        [DefaultValue(false)]
        public new bool WrapContents
        {
            get
            {
                return base.WrapContents;
            }
            set
            {
                base.WrapContents = value;
            }
        }


        [DefaultValue(AutoSizeMode.GrowAndShrink)]
        public override AutoSizeMode AutoSizeMode
        {
            get
            {
                return base.AutoSizeMode;
            }
            set
            {
                base.AutoSizeMode = value;
            }
        }

        [DefaultValue(AutoSizeFillMode.None)]
        public AutoSizeFillMode AutoSizeFill
        {
            get
            {
                return fillMode;
            }
            set
            {
                if (fillMode != value)
                {
                    fillMode = value;
                    PerformLayout();
                }
            }
        }

        public override LayoutEngine LayoutEngine
        {
            get
            {
                return layout;
            }
        }

        private class ControlCollection : Control.ControlCollection
        {
            public ControlCollection(Control owner)
                : base(owner)
            {
            }

            public override void SetChildIndex(Control child, int newIndex)
            {

            }

            public void SetBaseChildIndex(Control child, int newIndex)
            {
                base.SetChildIndex(child, newIndex);
            }
        }

        protected override Control.ControlCollection CreateControlsInstance()
        {
            if (DesignMode)
            {
                return base.CreateControlsInstance();
            }
            else
            {
                return new ControlCollection(this);
            }
        }

        public Size GetPreferredSize(Size proposedSize, bool force)
        {
            return layout.DoLayout(this, proposedSize, false, true, true, force);
        }

        public override Size GetPreferredSize(Size proposedSize)
        {
            if (proposedSize.Width == int.MaxValue)
            {
                if (cache.proposed.Width != 0)
                {
                    return cache.proposed;
                }
            }
            else if (proposedSize.Width == cache.proposed.Width)
            {
                return cache.proposed;
            }

            return layout.DoLayout(this, proposedSize, false, true, true, false);
        }
    }
}
