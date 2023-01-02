using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Drawing;

namespace Gw2Launcher.UI.Controls.ListPanel
{
    /// <summary>
    /// Basic list of labels that hold values
    /// </summary>
    class LabelListPanel : ControlListPanel
    {
        public event EventHandler<ListItemEventArgs> LabelClick;

        public class LabelListItem : ListItem
        {
            public LabelListItem(ControlListPanel parent, Control c, object value)
                : base(parent, c, value)
            {
            }

            private byte IconKey
            {
                get;
                set;
            }

            public Icon Icon
            {
                get
                {
                    return ((LabelListPanel)parent).GetIcon(c);
                }
                set
                {
                    ((LabelListPanel)parent).SetIcon(c, value);
                }
            }

            public async void LoadIconAsync(string path)
            {
                if (string.IsNullOrEmpty(path))
                    return;

                try
                {
                    var key = ++IconKey;
                    var p = (LabelListPanel)parent;
                    var il = p.IconSource;
                    if (il == null)
                        p.IconSource = il = new IconLoader();

                    p.SetIconSize(c, il.GetSize());

                    var icon = await il.LoadIconAsync(path);

                    if (key == IconKey)
                        this.Icon = icon;
                }
                catch (Exception e)
                {
                    Util.Logging.Log(e);
                }
            }
        }

        public class IconLoader : IDisposable
        {
            private Dictionary<string, object> icons;
            private Size size;

            public Size GetSize()
            {
                if (size.Width != 0)
                    return size;
                return size = Windows.ShellIcons.GetSize(Windows.ShellIcons.IconSize.Small);
            }

            public async Task<Icon> LoadIconAsync(string path)
            {
                Icon icon;

                if (!string.IsNullOrEmpty(path))
                {
                    object o;

                    if (icons == null)
                        icons = new Dictionary<string, object>();

                    if (!icons.TryGetValue(path, out o))
                    {
                        var t = Task.Run<Icon>(
                            delegate
                            {
                                try
                                {
                                    return Windows.ShellIcons.GetIcon(path, Windows.ShellIcons.IconSize.Small);
                                }
                                catch
                                {
                                    return null;
                                }
                            });

                        using (t)
                        {
                            icons[path] = t;

                            icon = await t;

                            if (this.IsDisposed && icon != null)
                            {
                                icon.Dispose();
                                return null;
                            }

                            icons[path] = icon;
                        }
                    }
                    else if (o is Task<Icon>)
                    {
                        icon = await (Task<Icon>)o;

                        if (this.IsDisposed)
                        {
                            return null;
                        }
                    }
                    else
                    {
                        icon = (Icon)o;
                    }
                }
                else
                {
                    icon = null;
                }

                return icon;
            }

            public bool IsDisposed
            {
                get;
                private set;
            }

            public void Dispose()
            {
                if (!IsDisposed)
                {
                    IsDisposed = true;

                    if (icons != null)
                    {
                        foreach (object o in icons.Values)
                        {
                            using (o as Icon) { }
                        }
                    }
                }
            }
        }

        public LabelListItem Add(string text, object value, bool modified)
        {
            var l = (LabelListItem)base.Add(CreateLabel(), value, modified);

            l.Text = text;

            return l;
        }

        protected override void OnScaleChanged(float scale)
        {
            base.OnScaleChanged(scale);

            IconSpacing = (int)(IconSpacing * scale + 0.5f);
        }

        protected override ControlListPanel.ListItem CreateListItem(Control c, object value)
        {
            return new LabelListItem(this, c, value);
        }

        protected virtual Control CreateLabel()
        {
            var c = new Controls.LinkLabel()
            {
                AutoSize = true,
                Margin = _ContentMargin,
                TextAlign = System.Drawing.ContentAlignment.MiddleLeft,
            };
            c.Click += label_Click;
            return c;
        }

        void label_Click(object sender, EventArgs e)
        {
            if (LabelClick != null)
            {
                var i = GetItem((Control)sender);
                if (i != null)
                    LabelClick(this, new ListItemEventArgs(i));
            }
        }

        protected virtual Icon GetIcon(Control c)
        {
            return ((LinkLabel)c).Icon;
        }

        protected virtual void SetIcon(Control c, Icon icon)
        {
            var l = (LinkLabel)c;

            if (icon == null)
            {
                SetIconSize(c, Size.Empty);
            }
            else
            {
                SetIconSize(c, icon.Size);
            }

            l.Icon = icon;
        }

        protected virtual void SetIconSize(Control c, Size sz)
        {
            if (sz.Width == 0)
            {
                c.Padding = Padding.Empty;
            }
            else
            {
                c.Padding = new Padding(sz.Width + _IconSpacing, 0, 0, 0);
                var h = sz.Height + 2;
                if (c.MinimumSize.Height < h)
                    c.MinimumSize = new Size(0, h);
            }
        }

        /// <summary>
        /// IconLoader source that can be shared with other panels
        /// </summary>
        public IconLoader IconSource
        {
            get;
            set;
        }

        protected virtual void OnIconSpacingChanged()
        {
            foreach (Control c in this.Controls)
            {
                var l = (LinkLabel)c;

                if (l.Icon != null)
                {
                    l.Padding = new Padding(l.Icon.Width + _IconSpacing, l.Padding.Top, l.Padding.Right, l.Padding.Bottom);
                }
            }
        }

        private int _IconSpacing = 3;
        [System.ComponentModel.DefaultValue(3)]
        public int IconSpacing
        {
            get
            {
                return _IconSpacing;
            }
            set
            {
                if (_IconSpacing != value)
                {
                    _IconSpacing = value;
                    OnIconSpacingChanged();
                }
            }
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);

            if (disposing)
            {
                using (IconSource)
                {
                    IconSource = null;
                }
            }
        }
    }
}
