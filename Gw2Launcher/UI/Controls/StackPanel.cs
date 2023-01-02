using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Drawing;
using System.Windows.Forms.Layout;
using System.ComponentModel;
using System.Drawing.Design;
using System.Windows.Forms.Design;

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
    [TypeDescriptionProvider(typeof(UiTypeDescriptionProvider))]
    class StackPanel : FlowLayoutPanel, UiColors.IColors
    {
        class AutoSizeStretchTypeEditor : UITypeEditor
        {
            private class ListValue
            {
                private string s;

                public ListValue(AutoSizeStretchMode v, string s = null)
                {
                    this.Value = v;
                    this.s = s;
                }

                public AutoSizeStretchMode Value
                {
                    get;
                    set;
                }

                public override string ToString()
                {
                    if (s != null)
                        return s;
                    return Value.ToString();
                }
            }

            public override UITypeEditorEditStyle GetEditStyle(ITypeDescriptorContext context)
            {
                return UITypeEditorEditStyle.DropDown;
            }

            public override bool IsDropDownResizable
            {
                get
                {
                    return true;
                }
            }

            public override object EditValue(ITypeDescriptorContext context, IServiceProvider provider, object value)
            {
                var s = (IWindowsFormsEditorService)provider.GetService(typeof(IWindowsFormsEditorService));

                AutoSizeStretchMode mode;
                if (value is AutoSizeStretchMode)
                    mode = (AutoSizeStretchMode)value;
                else
                    mode = (AutoSizeStretchMode)0;

                if (s != null)
                {
                    var list = new CheckedListBox()
                    {
                        Width = 200,
                        Height = 150,
                        CheckOnClick = true,
                    };

                    Func<AutoSizeStretchMode, AutoSizeStretchMode, CheckState> getState = delegate(AutoSizeStretchMode v, AutoSizeStretchMode flag)
                    {
                        switch (flag)
                        {
                            case AutoSizeStretchMode.NoPreferredSize:

                                if (v == flag)
                                    return CheckState.Checked;
                                else
                                    return CheckState.Indeterminate;

                            case AutoSizeStretchMode.MinimumPreferredWidth:
                            case AutoSizeStretchMode.MinimumPreferredHeight:

                                if ((v & AutoSizeStretchMode.MinimumPreferredSize) == AutoSizeStretchMode.MinimumPreferredSize)
                                    return CheckState.Indeterminate;

                                break;
                            case AutoSizeStretchMode.MaximumPreferredWidth:
                            case AutoSizeStretchMode.MaximumPreferredHeight:

                                if ((v & AutoSizeStretchMode.MaximumPreferredSize) == AutoSizeStretchMode.MaximumPreferredSize)
                                    return CheckState.Indeterminate;

                                break;
                            case AutoSizeStretchMode.MinimumPreferredSize:
                            case AutoSizeStretchMode.MaximumPreferredSize:

                                var vf = v & flag;
                                if (vf == flag)
                                    return CheckState.Checked;
                                else if (vf != 0)
                                    return CheckState.Indeterminate;

                                break;
                        }

                        return (v & flag) == flag ? CheckState.Checked : CheckState.Unchecked;
                    };

                    foreach (var i in new ListValue[] 
                        {
                            new ListValue(AutoSizeStretchMode.NoPreferredSize),
                        
                            new ListValue(AutoSizeStretchMode.MinimumPreferredSize),
                            new ListValue(AutoSizeStretchMode.MinimumPreferredWidth, "   Width"),
                            new ListValue(AutoSizeStretchMode.MinimumPreferredHeight, "   Height"),
                        
                            new ListValue(AutoSizeStretchMode.MaximumPreferredSize),
                            new ListValue(AutoSizeStretchMode.MaximumPreferredWidth, "   Width"),
                            new ListValue(AutoSizeStretchMode.MaximumPreferredHeight, "   Height"),
                        })
                    {
                        list.Items.Add(i, getState(mode, i.Value));
                    }

                    var blocked = false;

                    list.ItemCheck += delegate(object o, ItemCheckEventArgs e)
                    {
                        if (blocked)
                            return;
                        blocked = true;

                        if (e.Index == 0)
                        {
                            if (e.NewValue == CheckState.Checked || e.CurrentValue == CheckState.Indeterminate)
                            {
                                e.NewValue = CheckState.Checked;

                                for (var i = 1; i < list.Items.Count; i++)
                                {
                                    list.SetItemChecked(i, false);
                                }
                            }
                        }
                        else
                        {
                            mode = (AutoSizeStretchMode)0;

                            for (var i = 1; i < list.Items.Count; i++)
                            {
                                if (i != e.Index && list.GetItemCheckState(i) == CheckState.Checked || i == e.Index && e.NewValue == CheckState.Checked)
                                    mode |= ((ListValue)list.Items[i]).Value;
                            }

                            if (e.CurrentValue == CheckState.Indeterminate)
                            {
                                mode &= ~((ListValue)list.Items[e.Index]).Value;
                            }

                            for (var i = 0; i < list.Items.Count; i++)
                            {
                                var state = getState(mode, ((ListValue)list.Items[i]).Value);

                                if (i == e.Index)
                                    e.NewValue = state;
                                else
                                    list.SetItemCheckState(i, state);
                            }
                        }

                        blocked = false;
                    };

                    list.Leave += delegate
                    {
                        mode = (AutoSizeStretchMode)0;

                        for (var i = 1; i < list.Items.Count; i++)
                        {
                            if (list.GetItemCheckState(i) == CheckState.Checked)
                                mode |= ((ListValue)list.Items[i]).Value;
                        }

                        value = mode;
                    };

                    s.DropDownControl(list);
                }

                return value;
            }
        }

        class AutoSizeStretchTypeConverter : TypeConverter
        {
            public override bool CanConvertTo(ITypeDescriptorContext context, Type destinationType)
            {
                return destinationType == typeof(string) || base.CanConvertTo(context, destinationType);
            }
            public override object ConvertTo(ITypeDescriptorContext context, System.Globalization.CultureInfo culture, object value, Type destinationType)
            {
                if (destinationType == typeof(string))
                {
                    StackPanel.AutoSizeStretchMode mode;
                    if (value is StackPanel.AutoSizeStretchMode)
                        mode = (StackPanel.AutoSizeStretchMode)value;
                    else
                        return value.ToString();

                    if (mode == 0)
                    {
                        return value.ToString();
                    }

                    StringBuilder sb = null;

                    foreach (var v in new StackPanel.AutoSizeStretchMode[]
                    {
                        StackPanel.AutoSizeStretchMode.MinimumPreferredSize,
                        StackPanel.AutoSizeStretchMode.MaximumPreferredSize,
                    })
                    {
                        if ((mode & v) == v)
                        {
                            if (sb == null)
                                sb = new StringBuilder(50);
                            sb.Append(v.ToString());
                            sb.Append(", ");
                            mode &= ~v;
                        }
                    }

                    if (sb == null)
                    {
                        return mode.ToString();
                    }
                    else
                    {
                        if (mode != 0)
                            sb.Append(mode.ToString());
                        else if (sb.Length > 0)
                            sb.Length -= 2;

                        return sb.ToString();
                    }
                }

                return base.ConvertTo(context, culture, value, destinationType);
            }
        }

        private class StackPanelLayout : LayoutEngine
        {
            public class Cache
            {
                public Rectangle[] bounds;
                public Size size;
                public Size proposed;
                public Size minimum;

                public void Invalidate()
                {
                    size = Size.Empty;
                    proposed = Size.Empty;
                }
            }

            public bool Layout(StackPanel panel, LayoutEventArgs args, bool force)
            {
                var size = panel.ClientSize;

                if (args == null || args.AffectedControl != panel)
                {
                    panel.cache.Invalidate();

                    if (panel.AutoSize)
                    {
                        if (panel.AutoSizeFill == AutoSizeFillMode.Width)
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

                var m = panel.cache.minimum;
                var s = DoLayout(panel, size, true, panel.cache.proposed.IsEmpty, panel.AutoSize, force);

                if (panel.AutoSize)
                {
                    if (panel.ClientSize != s || m != panel.cache.minimum)
                    {
                        if (force)
                            panel.ClientSize = s;

                        return true;
                    }
                }

                return false;
            }

            public override bool Layout(object container, LayoutEventArgs args)
            {
                return Layout((StackPanel)container, args, false);
            }

            private Size GetPreferredSize(Control c, Size proposed, bool force)
            {
                if (force && c is StackPanel)
                    return ((StackPanel)c).GetPreferredSize(proposed, true);
                else
                    return c.GetPreferredSize(proposed);
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

                    if (panel.cache.proposed == proposed && !proposed.IsEmpty)
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
                int minimumWidth = panel.MinimumSize.Width, //calculated minimum size when auto sizing
                    minimumHeight = panel.MinimumSize.Height;
                int maximumWidth = panel.MaximumSize.Width,
                    maximumHeight = panel.MaximumSize.Height;
                var direction = panel.FlowDirection;
                var vertical = direction == FlowDirection.TopDown || direction == FlowDirection.BottomUp;
                var stretch = panel.AutoSizeStretch;

                if (maximumWidth == 0)
                    maximumWidth = int.MaxValue;
                if (maximumHeight == 0)
                    maximumHeight = int.MaxValue;

                if (w <= 0)
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

                if (h <= 0)
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

                if (autosize)
                {
                    if (panel.AutoSizeFill != AutoSizeFillMode.Width && (panel.Anchor & (AnchorStyles.Left | AnchorStyles.Right)) != (AnchorStyles.Left | AnchorStyles.Right))
                        minimumWidth = panel.MinimumSize.Width;
                    if ((panel.Anchor & (AnchorStyles.Top | AnchorStyles.Bottom)) != (AnchorStyles.Top | AnchorStyles.Bottom))
                        minimumHeight = panel.MinimumSize.Height;

                    if (panel.AutoSizeFill == AutoSizeFillMode.NoWrap)
                    {
                        if (panel.MaximumSize.Width > 0)
                            w = panel.MaximumSize.Width - panel.Padding.Right;
                        else
                            w = int.MaxValue;
                    }

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

                int firstW, firstH;
                int minimumY, minimumX;
                int visible;

                firstW = count;
                firstH = count;

                do
                {
                    x = panel.Padding.Left;
                    y = panel.Padding.Top;

                    int i;

                    minimumY = 0;
                    minimumX = 0;
                    visible = 0;

                    for (i = 0; i < count; ++i)
                    {
                        var c = panel.Controls[i];

                        if (!force && !c.Visible)
                        {
                            bounds[i] = Rectangle.Empty;
                            continue;
                        }

                        ++visible;

                        var cStretchW = (c.Anchor & (AnchorStyles.Left | AnchorStyles.Right)) == (AnchorStyles.Left | AnchorStyles.Right);
                        var cStretchH = (c.Anchor & (AnchorStyles.Top | AnchorStyles.Bottom)) == (AnchorStyles.Top | AnchorStyles.Bottom);

                        if (c.Anchor == AnchorStyles.None)
                        {
                            if (i < firstW)
                                firstW = i;
                            if (i < firstH)
                                firstH = i;
                        }
                        else
                        {
                            if (i < firstW && ((c.Anchor & AnchorStyles.Right) != 0 || (c.Anchor & AnchorStyles.Left) == 0))
                                firstW = i;
                            if (i < firstH && ((c.Anchor & AnchorStyles.Bottom) != 0 || (c.Anchor & AnchorStyles.Top) == 0))
                                firstH = i;
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

                            if (IsAutoSized(c))
                            {
                                //horizontal layouts must always calculate stretched widths last

                                if (cStretchW && (!vertical || cStretchH && stretch == AutoSizeStretchMode.NoPreferredSize) || c.Dock == DockStyle.Fill)
                                {
                                    s = c.MinimumSize;
                                }
                                else
                                {
                                    var remainingWidth = w - l.X - c.Margin.Right;

                                    //factor fixed sizes of other controls for horizontal layout
                                    if (!vertical && w < ushort.MaxValue)
                                    {
                                        for (var j = i + 1; j < count; j++)
                                        {
                                            var _c = panel.Controls[j];

                                            if (!force && !_c.Visible)
                                                continue;

                                            var _cStretchW = (_c.Anchor & (AnchorStyles.Left | AnchorStyles.Right)) == (AnchorStyles.Left | AnchorStyles.Right);

                                            if (_cStretchW || IsAutoSized(_c))
                                            {
                                                remainingWidth -= _c.MinimumSize.Width + _c.Margin.Horizontal;
                                            }
                                            else
                                            {
                                                remainingWidth -= _c.Width + _c.Margin.Horizontal;
                                            }
                                        }

                                        if (remainingWidth < 0)
                                            remainingWidth = 0;
                                    }

                                    s = GetPreferredSize(c, new Size(remainingWidth, int.MaxValue), force);

                                    if (cStretchW && (stretch & AutoSizeStretchMode.PreferredWidth) == AutoSizeStretchMode.NoPreferredSize)
                                    {
                                        s.Width = c.MinimumSize.Width;
                                    }

                                    if (cStretchH && (stretch & AutoSizeStretchMode.PreferredHeight) == AutoSizeStretchMode.NoPreferredSize)
                                    {
                                        s.Height = c.MinimumSize.Height;
                                    }
                                }
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

                            bounds[i] = new Rectangle(l, s);
                        }

                        if (vertical)
                        {
                            if (autosize)
                            {
                                var r = l.X + s.Width + c.Margin.Right;

                                if (r > minimumX)
                                    minimumX = r;

                                if (r > minimumWidth)
                                {
                                    minimumWidth = r;
                                    if (r > w)
                                    {
                                        w = r;
                                        if (visible > 1)
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
                            if (autosize)
                            {
                                var b = l.Y + s.Height + c.Margin.Bottom;

                                if (b > minimumY)
                                    minimumY = b;

                                if (b > maximumHeight)
                                    b = maximumHeight;

                                if (b > minimumHeight)
                                {
                                    minimumHeight = b;
                                    if (b > h)
                                        h = b;
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

                    if (vertical)
                        minimumY = y;
                    else
                        minimumX = x;

                    if (autosize)
                    {
                        if (vertical)
                        {
                            if (y > minimumHeight)
                            {
                                if (y > maximumHeight)
                                    minimumHeight = maximumHeight;
                                else
                                    minimumHeight = y;
                            }
                        }
                        else
                        {
                            if (x > minimumWidth)
                            {
                                if (x > maximumWidth)
                                    minimumWidth = maximumWidth;
                                else
                                    minimumWidth = x;
                            }
                        }

                        if (minimumWidth == 0) //even possible? (no controls / only stretch?) -- happens when no controls / only invisible
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
                                        var s = GetPreferredSize(c, new Size(w - bounds[i].X - c.Margin.Right, int.MaxValue), force);
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
                    }

                    if (visible == 0)
                        break;

                    for (i = 0; i < count; ++i)
                    {
                        var c = panel.Controls[i];

                        if (!force && !c.Visible)
                            continue;

                        var cStretchW = (c.Anchor & (AnchorStyles.Left | AnchorStyles.Right)) == (AnchorStyles.Left | AnchorStyles.Right);
                        var cStretchH = (c.Anchor & (AnchorStyles.Top | AnchorStyles.Bottom)) == (AnchorStyles.Top | AnchorStyles.Bottom);

                        if (c.Dock == DockStyle.None)
                        {
                            if (cStretchW || cStretchH)
                            {
                                var cAutosized = IsAutoSized(c);
                                var ri = i;
                                var s = bounds[i].Size;

                                if (cStretchW)
                                {
                                    int remainingWidth;

                                    if (vertical)
                                    {
                                        remainingWidth = minimumWidth - bounds[i].X - c.Margin.Right;
                                    }
                                    else
                                    {
                                        remainingWidth = bounds[i].Width + minimumWidth - x;
                                    }

                                    if (cAutosized)
                                    {
                                        if (vertical)
                                        {
                                            //vertical was already handled
                                        }
                                        else
                                        {
                                            if (stretch != AutoSizeStretchMode.NoPreferredSize || !cStretchH)
                                            {
                                                s = c.GetPreferredSize(new Size(remainingWidth, int.MaxValue));

                                                if (cStretchW & (stretch & AutoSizeStretchMode.PreferredWidth) == AutoSizeStretchMode.NoPreferredSize)
                                                {
                                                    s.Width = c.MinimumSize.Width;
                                                }
                                                else
                                                {
                                                    minimumX += s.Width - bounds[i].Width;
                                                }

                                                if (cStretchH && (stretch & AutoSizeStretchMode.PreferredHeight) == AutoSizeStretchMode.NoPreferredSize)
                                                {
                                                    s.Height = c.MinimumSize.Height;
                                                }
                                                else if (autosize)
                                                {
                                                    //horizontal layout - bottom is absolute
                                                    var b = bounds[i].Y + c.Margin.Bottom + s.Height;

                                                    if (b > maximumHeight)
                                                        b = maximumHeight;

                                                    if (b > minimumY)
                                                        minimumY = b;

                                                    if (b > minimumHeight)
                                                    {
                                                        minimumHeight = b;
                                                        if (b > h)
                                                            h = b;
                                                        if (ri > firstH)
                                                        {
                                                            //recalculate anchors
                                                            ri = firstH;
                                                        }
                                                    }
                                                }
                                            }
                                        }

                                        if (remainingWidth >= s.Width)
                                        {
                                            if ((stretch & AutoSizeStretchMode.MaximumPreferredWidth) == 0)
                                            {
                                                s.Width = remainingWidth;
                                            }
                                        }
                                        else if ((stretch & AutoSizeStretchMode.PreferredWidth) == AutoSizeStretchMode.NoPreferredSize)
                                        {
                                            s.Width = remainingWidth;
                                        }
                                        else
                                        {
                                            //keeping larger autosized width; will need recalculate total width

                                            if (autosize)
                                            {
                                                var r = s.Width + minimumWidth - remainingWidth;

                                                ////alternative if using remainingWidth isn't viable
                                                //if (vertical)
                                                //{
                                                //    r = bounds[i].X + c.Margin.Right + s.Width;
                                                //}
                                                //else
                                                //{
                                                //    r = x + s.Width - bounds[i].Width;
                                                //}

                                                if (r > maximumWidth)
                                                    r = maximumWidth;

                                                if (r > minimumWidth)
                                                {
                                                    minimumWidth = r;

                                                    if (r > w)
                                                    {
                                                        //width changed, everything needs to be recalculated (vertical layout)
                                                        w = r;
                                                        if (vertical && visible != 1)
                                                            break;
                                                    }
                                                    else if (vertical && ri > firstW)
                                                    {
                                                        //recalculate anchors (vertical layout)
                                                        ri = firstW;
                                                    }
                                                }
                                            }
                                        }
                                    }
                                    else
                                        s.Width = remainingWidth;
                                }

                                if (cStretchH)
                                {
                                    int remainingHeight;

                                    if (vertical)
                                    {
                                        remainingHeight = bounds[i].Height + minimumHeight - y;
                                    }
                                    else
                                    {
                                        remainingHeight = minimumHeight - bounds[i].Y - c.Margin.Bottom;
                                    }

                                    if (cAutosized)
                                    {
                                        if (remainingHeight >= s.Height || (stretch & AutoSizeStretchMode.PreferredHeight) == AutoSizeStretchMode.NoPreferredSize)
                                        {
                                            if ((stretch & AutoSizeStretchMode.MaximumPreferredHeight) == 0)
                                                s.Height = remainingHeight;
                                        }
                                        else
                                        {
                                            //keeping larger autosized height (already handled - only possible when stretching width in horizontal, as vertical was already calculated)
                                        }
                                    }
                                    else
                                        s.Height = remainingHeight;
                                }

                                //shift remaining controls
                                if (vertical)
                                {
                                    var yDiff = s.Height - bounds[i].Height;

                                    if (yDiff != 0)
                                    {
                                        for (var j = i + 1; j < count; ++j)
                                        {
                                            bounds[j].Y += yDiff;
                                        }

                                        y += yDiff;
                                    }
                                }
                                else
                                {
                                    var xDiff = s.Width - bounds[i].Width;

                                    if (xDiff != 0)
                                    {
                                        for (var j = i + 1; j < count; ++j)
                                        {
                                            bounds[j].X += xDiff;
                                        }

                                        x += xDiff;
                                    }
                                }

                                bounds[i].Size = s;

                                if (ri != i)
                                {
                                    i = ri - 1;
                                    continue;
                                }
                            }

                            if (vertical)
                            {
                                //vertical layout

                                if (!cStretchW)
                                {
                                    switch (c.Anchor & (AnchorStyles.Left | AnchorStyles.Right))
                                    {
                                        //right aligned
                                        case AnchorStyles.Right:

                                            bounds[i].X = minimumWidth - bounds[i].Width - c.Margin.Right;

                                            break;
                                        //stretch horizontally
                                        //case AnchorStyles.Left | AnchorStyles.Right:

                                        //centered horizontally
                                        case AnchorStyles.None:

                                            bounds[i].X = panel.Padding.Left + ((minimumWidth - panel.Padding.Left) - bounds[i].Width) / 2 + c.Margin.Left - c.Margin.Right;

                                            break;
                                    }
                                }

                                if (!cStretchH)
                                {
                                    switch (c.Anchor & (AnchorStyles.Top | AnchorStyles.Bottom))
                                    {
                                        //stretched vertically
                                        //case AnchorStyles.Top | AnchorStyles.Bottom:

                                        //bottom aligned
                                        case AnchorStyles.Bottom:

                                            bounds[i].Y = minimumHeight - bounds[i].Height - c.Margin.Bottom;

                                            break;
                                    }
                                }
                            }
                            else
                            {
                                //horizontal layout

                                if (!cStretchW)
                                {
                                    switch (c.Anchor & (AnchorStyles.Left | AnchorStyles.Right))
                                    {
                                        //right aligned
                                        case AnchorStyles.Right:

                                            bounds[i].X = minimumWidth - bounds[i].Width - c.Margin.Right;

                                            break;
                                        //stretch to fill horizontal space
                                        //case AnchorStyles.Left | AnchorStyles.Right:
                                    }
                                }

                                if (!cStretchH)
                                {
                                    switch (c.Anchor & (AnchorStyles.Top | AnchorStyles.Bottom))
                                    {
                                        //bottom aligned
                                        case AnchorStyles.Bottom:

                                            bounds[i].Y = minimumHeight - bounds[i].Height - c.Margin.Bottom;

                                            break;
                                        //stretch vertically
                                        //case AnchorStyles.Top | AnchorStyles.Bottom:

                                        //centered vertically
                                        case AnchorStyles.None:

                                            bounds[i].Y = panel.Padding.Top + ((minimumHeight - panel.Padding.Top) - bounds[i].Height) / 2 + c.Margin.Top - c.Margin.Bottom;

                                            break;
                                    }
                                }
                            }
                        }
                        else
                        {
                            switch (c.Dock)
                            {
                                //control is right-aligned floated (does not affect other controls)
                                case DockStyle.Right:

                                    bounds[i].X = minimumWidth - bounds[i].Width - c.Margin.Right;

                                    break;
                                //horizontal layout: control will fill remaining space up to its maximum preferred size
                                case DockStyle.Fill:

                                    if (!vertical)
                                    {
                                        var _wdiff = minimumWidth - x;
                                        int _w;

                                        if (IsAutoSized(c))
                                        {
                                            var s = GetPreferredSize(c, new Size(int.MaxValue, int.MaxValue), force);

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

                                                        minimumHeight = b;
                                                        if (b > h)
                                                            h = b;
                                                        if (i > firstH)
                                                        {
                                                            //recalculate anchors
                                                            i = firstH - 1;
                                                            continue;
                                                        }
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

                minimumX += panel.Padding.Right;
                minimumY += panel.Padding.Bottom;
                if (minimumX < panel.MinimumSize.Width)
                    minimumX = panel.MinimumSize.Width;
                if (minimumY < panel.MinimumSize.Height)
                    minimumY = panel.MinimumSize.Height;

                panel.cache.minimum = new Size(minimumX, minimumY);
                panel.cache.proposed = new Size(minimumWidth + panel.Padding.Right, minimumHeight + panel.Padding.Bottom);
                if (apply)
                    panel.cache.size = panel.cache.proposed;
                return panel.cache.proposed;
            }
        }

        public enum AutoSizeFillMode : byte
        {
            None,
            /// <summary>
            /// Expands to fill the width of the parent
            /// </summary>
            Width,
            /// <summary>
            /// Expands to the largest control
            /// </summary>
            NoWrap,
        }

        [Flags]
        public enum AutoSizeStretchMode : byte
        {
            /// <summary>
            /// Preferred sizes will be ignored
            /// </summary>
            NoPreferredSize = 0,

            /// <summary>
            /// Width will not be stretched below the preferred size
            /// </summary>
            MinimumPreferredWidth = 1,
            /// <summary>
            /// Width/height will not be stretched below the preferred size
            /// </summary>
            MinimumPreferredHeight = 2,
            /// <summary>
            /// Width/height will not be stretched below the preferred size
            /// </summary>
            MinimumPreferredSize = 3,

            /// <summary>
            /// Width will not be stretched beyond the preferred size
            /// </summary>
            MaximumPreferredWidth = 4,
            /// <summary>
            /// Height will not be stretched beyond the preferred size
            /// </summary>
            MaximumPreferredHeight = 8,
            /// <summary>
            /// Width/height will not be stretched beyond the preferred size
            /// </summary>
            MaximumPreferredSize = 12,

            /// <summary>
            /// MinimumPreferredWidth and MaximumPreferredWidth
            /// </summary>
            PreferredWidth = 5,
            /// <summary>
            /// MinimumPreferredHeight and MaximumPreferredHeight
            /// </summary>
            PreferredHeight = 10,
            /// <summary>
            /// MinimumPreferredSize and MaximumPreferredSize
            /// </summary>
            PreferredSize = 15,
        }

        private static readonly StackPanelLayout layout = new StackPanelLayout();

        private AutoSizeFillMode fillMode;
        private AutoSizeStretchMode stretchMode;
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

        [DefaultValue(AutoSizeStretchMode.NoPreferredSize)]
        [Editor(typeof(AutoSizeStretchTypeEditor), typeof(UITypeEditor))]
        [TypeConverter(typeof(AutoSizeStretchTypeConverter))]
        public AutoSizeStretchMode AutoSizeStretch
        {
            get
            {
                return stretchMode;
            }
            set
            {
                if (stretchMode != value)
                {
                    stretchMode = value;
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
            private bool canSetIndex;

            public ControlCollection(Control owner)
                : base(owner)
            {
            }

            public override void AddRange(Control[] controls)
            {
                canSetIndex = true;
                base.AddRange(controls);
                canSetIndex = false;
            }

            public override void SetChildIndex(Control child, int newIndex)
            {
                //prevent invisible controls from changing order when the control is created
                if (canSetIndex)
                    base.SetChildIndex(child, newIndex);
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

        /// <summary>
        /// Sets the index of the control; note Controls.SetChildIndex is ignored
        /// </summary>
        public void SetChildIndex(Control child, int newIndex)
        {
            ((ControlCollection)this.Controls).SetBaseChildIndex(child, newIndex);
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
                if (proposedSize.Height == int.MaxValue)
                {
                    if (cache.minimum.Height == cache.proposed.Height)
                    {
                        return cache.proposed;
                    }
                    else
                    {
                        var min = new Size(cache.proposed.Width, cache.minimum.Height);

                        return min;
                    }
                }
                else if (proposedSize.Height == cache.proposed.Height)
                {
                    return cache.proposed;
                }
            }

            return layout.DoLayout(this, proposedSize, false, true, true, false);
        }

        /// <summary>
        /// Performs the layout of child controls; invisible controls will be ignored
        /// </summary>
        /// <param name="force">Optionally forces the layout of invisible controls</param>
        public void PerformLayout(bool force)
        {
            layout.Layout(this, null, force);
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

        protected UiColors.Colors _BackColorName = UiColors.Colors.Custom;
        [DefaultValue(UiColors.Colors.Custom)]
        [UiPropertyColor()]
        [TypeConverter(typeof(UiColorTypeConverter))]
        [Editor(typeof(UiColorTypeEditor), typeof(UITypeEditor))]
        public UiColors.Colors BackColorName
        {
            get
            {
                return _BackColorName;
            }
            set
            {
                if (_BackColorName != value)
                {
                    _BackColorName = value;
                    RefreshColors();
                }
            }
        }

        protected UiColors.Colors _ForeColorName = UiColors.Colors.Custom;
        [DefaultValue(UiColors.Colors.Custom)]
        [UiPropertyColor()]
        [TypeConverter(typeof(UiColorTypeConverter))]
        [Editor(typeof(UiColorTypeEditor), typeof(UITypeEditor))]
        public UiColors.Colors ForeColorName
        {
            get
            {
                return _ForeColorName;
            }
            set
            {
                if (_ForeColorName != value)
                {
                    _ForeColorName = value;
                    RefreshColors();
                }
            }
        }

        public void RefreshColors()
        {
            if (_ForeColorName != UiColors.Colors.Custom)
                base.ForeColor = UiColors.GetColor(_ForeColorName);
            if (_BackColorName != UiColors.Colors.Custom)
                base.BackColor = UiColors.GetColor(_BackColorName);
        }
    }
}
