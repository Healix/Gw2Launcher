using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Drawing;
using System.ComponentModel;

namespace Gw2Launcher.UI.Controls
{
    class AccountSquares : Base.BaseControl
    {
        public event EventHandler SelectedChanged;

        public enum DisplayStyle
        {
            Squares,
            Stretch,
        }

        private List<Settings.IAccount> accounts;
        private float size;
        private int firstX;
        private TargetArea target;
        private Tooltip.FloatingTooltip tooltip;

        private struct TargetArea
        {
            public int index;
            public ushort x1, x2;

            public void Reset()
            {
                index = -1;
                x1 = 0;
                x2 = 0;
            }
        }

        public AccountSquares()
        {
            SetStyle(ControlStyles.AllPaintingInWmPaint, true);
            SetStyle(ControlStyles.UserPaint, true);
            SetStyle(ControlStyles.ResizeRedraw, true);

            accounts = new List<Settings.IAccount>();
        }

        private DisplayStyle _Style;
        [DefaultValue(DisplayStyle.Squares)]
        public DisplayStyle Style
        {
            get
            {
                return _Style;
            }
            set
            {
                if (_Style != value)
                {
                    _Style = value;
                    target.Reset();
                    this.Invalidate();
                }
            }
        }

        public void SetAccounts(Settings.IAccount[] accounts)
        {
            var b = this.accounts.Count > 0;

            if (b)
            {
                Clear();
            }

            if (Add(accounts))
            {
                b = true;
            }

            if (b)
            {
                OnAccountsChanged();
            }
        }

        private bool Add(Settings.IAccount[] accounts)
        {
            var count = accounts == null ? 0 : accounts.Length;

            for (var i = 0; i < count; i++)
            {
                if (accounts[i] == null)
                {
                    count = i;
                    break;
                }
                Add(accounts[i]);
            }

            return count > 0;
        }

        private void Add(Settings.IAccount account)
        {
            accounts.Add(account);
            account.ColorKeyChanged += OnColorKeyChanged;
        }

        private void Clear()
        {
            foreach (var a in accounts)
            {
                a.ColorKeyChanged -= OnColorKeyChanged;
            }
            accounts.Clear();
        }

        public int Count
        {
            get
            {
                return accounts.Count;
            }
        }

        public Settings.IAccount Selected
        {
            get;
            set;
        }

        void OnColorKeyChanged(object sender, EventArgs e)
        {
            this.Invalidate();
        }

        private void OnAccountsChanged()
        {
            target.Reset();

            this.Invalidate();
        }

        private TargetArea GetTargetFromPoint(int x)
        {
            var count = accounts.Count;

            if (count == 0)
            {
                return new TargetArea()
                {
                    index = -1,
                    x1 = 0,
                    x2 = ushort.MaxValue,
                };
            }
            else if (_Style == DisplayStyle.Squares)
            {
                var sz = (int)(this.size + 0.5f);
                var w = this.Width - this.Padding.Right;
                var spaces = (int)(w / sz);
                var ofs = w - (int)(spaces * sz);
                var a = new TargetArea()
                {
                    index = spaces - (int)((x - ofs) / sz) - 1,
                };

                if (a.index < 0)
                {
                    a.x1 = (ushort)w;
                    a.x2 = (ushort)this.Width;
                    a.index = -1;
                }
                else if (a.index >= count)
                {
                    a.x1 = 0;
                    a.x2 = (ushort)(w - (count * sz));
                    a.index = -1;
                }
                else
                {
                    a.x2 = (ushort)(w - (sz * a.index));
                    a.x1 = (ushort)(a.x2 - sz);
                }

                return a;
            }
            else
            {
                var w = this.Width;
                var i = (int)(x / this.size);
                var x1 = (int)((i + 1) * this.size) - 1;

                if (x > x1)
                {
                    ++i;
                    x1 = (int)((i + 1) * this.size) - 1;
                }

                if (i < 0)
                {
                    i = 0;
                }
                else if (i >= count)
                {
                    i = count - 1;
                }

                if (x1 >= w - 1)
                {
                    x1 = w;
                }

                return new TargetArea()
                {
                    index = i,
                    x1 = (ushort)(i * this.size),
                    x2 = (ushort)x1,
                };
            }
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);

            if (e.X < target.x1 || e.X > target.x2)
            {
                target = GetTargetFromPoint(e.X);

                if (target.index != -1)
                {
                    OnAccountHovered(accounts[target.index]);
                }
                else
                {
                    ShowTooltip(null);
                }
            }
        }

        private void ShowTooltip(string text)
        {
            if (string.IsNullOrEmpty(text))
            {
                if (tooltip != null && tooltip.Visible)
                {
                    tooltip.Hide();
                }
            }
            else
            {
                if (tooltip == null)
                {
                    tooltip = new Tooltip.FloatingTooltip();
                }

                tooltip.ShowTooltip(this, text);
            }
        }

        private void OnAccountHovered(Settings.IAccount account)
        {
            ShowTooltip(account.Name);
        }

        private void OnAccountClicked(Settings.IAccount account)
        {
            if (Selected != account)
            {
                Selected = account;

                if (SelectedChanged != null)
                    SelectedChanged(this, EventArgs.Empty);
            }
        }

        protected override void OnMouseLeave(EventArgs e)
        {
            base.OnMouseLeave(e);

            target.Reset();
        }

        protected override void OnMouseUp(MouseEventArgs e)
        {
            base.OnMouseUp(e);

            if (target.index != -1)
            {
                OnAccountClicked(accounts[target.index]);
            }
        }

        protected override void OnSizeChanged(EventArgs e)
        {
            base.OnSizeChanged(e);

            target.Reset();
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            var count = accounts.Count;

            if (count > 0)
            {
                var w = this.Width;
                var h = this.Height;
                var g = e.Graphics;
                var scale = g.DpiX / 96f;

                if (_Style == DisplayStyle.Squares)
                {
                    var sz = h - this.Padding.Vertical;
                    var spacing = (int)(scale * 2 + 0.5f);
                    var y = this.Padding.Top;

                    size = sz + spacing;

                    using (var b = new SolidBrush(Color.Empty))
                    {
                        var x = w - this.Padding.Right;

                        for (var i = 0; i < count; i++)
                        {
                            b.Color = accounts[i].ColorKey;

                            if (x > sz)
                            {
                                x -= sz;

                                g.FillRectangle(b, x, y, sz, sz);

                                x -= spacing;
                            }
                            else
                            {
                                break;
                            }
                        }

                        firstX = x;
                    }
                }
                else
                {
                    var minW = scale * 10;
                    var sz = (float)w / count;
                    var spacing = (int)(scale + 0.5f);

                    if (sz < minW)
                    {
                        sz = minW;
                    }

                    size = sz;

                    using (var b = new SolidBrush(Color.Empty))
                    {
                        var x = 0;

                        for (var i = 0; i < count; i++)
                        {
                            b.Color = accounts[i].ColorKey;

                            var x1 = (int)((i + 1) * sz);

                            if (x1 < w - 1)
                            {
                                g.FillRectangle(b, x, 0, x1 - x - spacing, h);
                            }
                            else
                            {
                                g.FillRectangle(b, x, 0, w - x, h);

                                break;
                            }

                            x = x1;
                        }
                    }
                }
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                Clear();
            }

            base.Dispose(disposing);
        }
    }
}
