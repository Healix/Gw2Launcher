using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Gw2Launcher.UI.Controls.ListPanel
{
    /// <summary>
    /// Basic list of controls that hold values
    /// </summary>
    abstract class ControlListPanel : StackPanel
    {
        public class ListItem
        {
            public event EventHandler ValueChanged, ModifiedChanged;

            protected Control c;
            protected ControlListPanel parent;

            public ListItem(ControlListPanel parent, Control c, object value)
            {
                this.c = c;
                this.parent = parent;
                this._Value = value;
            }

            protected virtual void OnValueChanged()
            {
                if (ValueChanged != null)
                    ValueChanged(this, EventArgs.Empty);
            }

            protected virtual void OnModifiedChanged()
            {
                parent.OnItemModifiedChanged(this);
                if (ModifiedChanged != null)
                    ModifiedChanged(this, EventArgs.Empty);
            }

            protected virtual void OnHiddenChanged()
            {
                parent.OnItemHiddenChanged(this);
            }

            public virtual string Text
            {
                get
                {
                    return parent.GetText(c);
                }
                set
                {
                    parent.SetText(c, value);
                }
            }

            public object InitialValue
            {
                get;
                set;
            }

            protected object _Value;
            public object Value
            {
                get
                {
                    return _Value;
                }
                set
                {
                    if (!object.ReferenceEquals(_Value,value))
                    {
                        _Value = value;
                        Modified = true;
                        OnValueChanged();
                    }
                }
            }

            protected bool _Modified;
            public bool Modified
            {
                get
                {
                    return _Modified;
                }
                set
                {
                    if (_Modified != value)
                    {
                        _Modified = value;
                        OnModifiedChanged();
                    }
                }
            }

            protected bool _Hidden;
            public bool Hidden
            {
                get
                {
                    return _Hidden;
                }
                set
                {
                    if (_Hidden != value)
                    {
                        _Hidden = value;
                        OnHiddenChanged();
                    }
                }
            }

            public ControlListPanel Parent
            {
                get
                {
                    return parent;
                }
            }

            public Control Control
            {
                get
                {
                    return c;
                }
            }

            public void Remove()
            {
                parent.Remove(c);
            }
        }

        public class ListItemEventArgs : EventArgs
        {
            public ListItemEventArgs(ListItem item)
            {
                this.Item = item;
            }

            public ListItem Item
            {
                get;
                private set;
            }
        }

        public class ListItemMouseEventArgs : ListItemEventArgs
        {
            public ListItemMouseEventArgs(ListItem item, MouseEventArgs e)
                : base(item)
            {
                this.MouseEvent = e;
            }

            public MouseEventArgs MouseEvent
            {
                get;
                private set;
            }
        }

        public event EventHandler CountChanged;

        private int hiddenCount;

        public ControlListPanel()
        {
        }

        protected ListItem Add(Control c, object value, bool modified)
        {
            var l = CreateListItem(c, value);

            if (!modified)
                l.InitialValue = value;
            else
                l.Modified = true;

            c.Tag = l;
            c.Margin = _ContentMargin;

            this.Controls.Add(c);

            return l;
        }

        protected virtual ListItem CreateListItem(Control c, object value)
        {
            return new ListItem(this, c, value);
        }

        protected virtual string GetText(Control c)
        {
            return c.Text;
        }

        protected virtual void SetText(Control c, string value)
        {
            c.Text = value;
        }

        void OnItemModifiedChanged(ListItem i)
        {
            if (i.Modified)
            {
                Modified = true;
            }
        }

        void OnItemHiddenChanged(ListItem i)
        {
            if (i.Hidden)
                ++hiddenCount;
            else
                --hiddenCount;
            i.Control.Visible = !i.Hidden;
            OnCountChanged();
        }

        protected override void OnControlAdded(ControlEventArgs e)
        {
            base.OnControlAdded(e);
            OnCountChanged();
        }

        protected override void OnControlRemoved(ControlEventArgs e)
        {
            base.OnControlRemoved(e);
            OnCountChanged();
        }

        public void Remove(object value)
        {
            var c = Find(value);
            if (c != null)
            {
                if (c.Hidden)
                    --hiddenCount;
                Remove(c.Control);
            }
        }

        public void Remove(Control c)
        {
            this.Controls.Remove(c);
            c.Dispose();
        }

        protected void OnCountChanged()
        {
            Modified = true;
            if (CountChanged != null)
                CountChanged(this, EventArgs.Empty);
        }

        public ListItem Find(object value)
        {
            for (var i = this.Controls.Count - 1; i >= 0; --i)
            {
                var l = GetItem(this.Controls[i]);
                if (l != null && object.ReferenceEquals(l.Value, value))
                {
                    return l;
                }
            }
            return null;
        }

        public ListItem[] GetItems()
        {
            var items = new ListItem[this.Controls.Count];
            for (var i = this.Controls.Count - 1; i >= 0; --i)
            {
                items[i] = GetItem(this.Controls[i]);
            }
            return items;
        }

        public ListItem GetItem(Control c)
        {
            return c.Tag as ListItem;
        }

        protected virtual void OnScaleChanged(float scale)
        {
            _ContentMargin = new Padding((int)(_ContentMargin.Left * scale + 0.5f), (int)(_ContentMargin.Top * scale + 0.5f), (int)(_ContentMargin.Right * scale + 0.5f), (int)(_ContentMargin.Bottom * scale + 0.5f));
        }

        protected override void ScaleControl(System.Drawing.SizeF factor, BoundsSpecified specified)
        {
            base.ScaleControl(factor, specified);

            var scale = factor.Width;
            if (scale != 1f && specified == BoundsSpecified.All)
            {
                OnScaleChanged(scale);
            }
        }

        protected Padding _ContentMargin;
        public Padding ContentMargin
        {
            get
            {
                return _ContentMargin;
            }
            set
            {
                if (_ContentMargin != value)
                {
                    _ContentMargin = value;

                    var count = this.Controls.Count;
                    if (count > 0)
                    {
                        this.SuspendLayout();
                        for (var i = 0; i < count; ++i)
                        {
                            this.Controls[i].Margin = value;
                        }
                        this.ResumeLayout();
                    }
                }
            }
        }

        public int Count
        {
            get
            {
                return this.Controls.Count - hiddenCount;
            }
        }

        public bool Modified
        {
            get;
            set;
        }

        public void ResetModified()
        {
            this.Modified = false;

            foreach (Control c in this.Controls)
            {
                GetItem(c).Modified = false;
            }
        }
    }
}
