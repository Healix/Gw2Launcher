using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Drawing;
using Gw2Launcher.UI.Controls;

namespace Gw2Launcher.UI.Base
{
    public class StackFormBase : Base.BaseForm
    {
        private class StackFormBaseLayout : System.Windows.Forms.Layout.LayoutEngine
        {
            public override bool Layout(object container, LayoutEventArgs layoutEventArgs)
            {
                var f = (StackFormBase)container;
                var sz = DoLayout(f, f.ClientSize, false);

                return f.ClientSize != sz;
            }

            public Size DoLayout(StackFormBase form, Size proposed, bool measureOnly)
            {
                var w = proposed.Width;
                var h = proposed.Height;
                int mw = form.MinimumSize.Width,
                    mh = form.MinimumSize.Height;
                var autow = w == 0 || w > ushort.MaxValue;
                var autoh = h == 0 || h > ushort.MaxValue;

                if (autow)
                {
                    if (form.MaximumSize.Width > 0)
                    {
                        w = form.MaximumSize.Width - form.SizeFromClientSize(Size.Empty).Width - form.Padding.Horizontal;
                    }
                    else
                    {
                        w = int.MaxValue;
                    }
                }

                if (autoh)
                {
                    if (form.MaximumSize.Height > 0)
                    {
                        h = form.MaximumSize.Height - form.SizeFromClientSize(Size.Empty).Height - form.Padding.Vertical;
                    }
                    else
                    {
                        h = int.MaxValue;
                    }
                }

                w -= form.Padding.Horizontal;
                h -= form.Padding.Vertical;

                if (form.AutoSize && form.AutoSizeMode == AutoSizeMode.GrowOnly)
                {
                    mw = form.ClientSize.Width;
                    mh = form.ClientSize.Height;
                }

                for (int i = 0, count = form.Controls.Count; i < count; )
                {
                    var c = form.Controls[i++];
                    var stretchW = c.Anchor.HasFlag(AnchorStyles.Left | AnchorStyles.Right);
                    var stretchH = c.Anchor.HasFlag(AnchorStyles.Top | AnchorStyles.Bottom);
                    Size s;

                    if (c.AutoSize)
                    {
                        if (!form.Visible && c is StackPanel)
                            s = ((StackPanel)c).GetPreferredSize(new Size(w - c.Margin.Horizontal, int.MaxValue), true);
                        else
                            s = c.GetPreferredSize(new Size(w - c.Margin.Horizontal, int.MaxValue));
                    }
                    else
                    {
                        s = c.Size;
                    }

                    if (stretchW)
                    {
                        if (autow)
                        {
                            var _w = s.Width + c.Margin.Horizontal + form.Padding.Horizontal;
                            if (_w < w)
                                w = _w;
                            autow = false;
                        }

                        s.Width = w - c.Margin.Horizontal;
                    }

                    if (stretchH)
                    {
                        if (autoh)
                        {
                            var _h = s.Height + c.Margin.Vertical + form.Padding.Vertical;
                            if (_h < h)
                                h = _h;
                            autoh = false;
                        }

                        s.Height = h - c.Margin.Vertical;
                    }

                    var bounds = new Rectangle(form.Padding.Left + c.Margin.Left, form.Padding.Top + c.Margin.Top, s.Width, s.Height);
                    var r = bounds.Right + c.Margin.Right;
                    var b = bounds.Bottom + c.Margin.Bottom;

                    if (r > mw)
                        mw = r;
                    if (b > mh)
                        mh = b;

                    if (!measureOnly)
                    {
                        c.Bounds = bounds;
                    }
                }

                return new Size(mw, mh);
            }
        }

        private static readonly StackFormBaseLayout layout = new StackFormBaseLayout();

        public enum AutoSizeFillMode
        {
            Default,
            Height,
        }

        public StackFormBase()
        {
        }

        public AutoSizeFillMode AutoSizeFill
        {
            get;
            set;
        }

        public override System.Windows.Forms.Layout.LayoutEngine LayoutEngine
        {
            get
            {
                return layout;
            }
        }

        public override Size GetPreferredSize(Size proposedSize)
        {
            if (AutoSizeFill == AutoSizeFillMode.Height)
            {
                proposedSize.Width = this.ClientSize.Width;
            }

            return SizeFromClientSize(layout.DoLayout(this, proposedSize, true));
        }
    }
}
