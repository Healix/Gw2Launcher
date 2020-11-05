using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Forms.Layout;
using System.ComponentModel;
using System.Drawing;

namespace Gw2Launcher.UI.Controls
{
    class AccountGridButtonPanel : Panel
    {
        private class AccountGridButtonContainerLayout : LayoutEngine
        {
            public override bool Layout(object container, LayoutEventArgs args)
            {
                var c = (AccountGridButtonPanel)container;
                if (!c.Visible || c.Parent == null)
                    return false;

                if (c.pending)
                {
                    c.pending = false;

                    var s = DoLayout(c, c.ClientSize, true, true, c.gridColumns);

                    if (c.ContentHeight != s.Height)
                    {
                        c.ContentHeight = s.Height;

                        if (s.Height > 0)
                        {
                            c.GridRows = (s.Height - c.gridSpacing * 2) / c.gridRowHeight;
                        }
                        else
                        {
                            c.GridRows = 0;
                        }

                        return true;
                    }
                }

                return false;
            }

            public Size DoLayout(AccountGridButtonPanel panel, Size proposed, bool apply, bool measure, int columns)
            {
                var count = panel.buttons.Length;
                var spacing = panel.GridSpacing;
                var rh = panel.GridRowHeight;
                var rw = (float)(proposed.Width - spacing) / columns;
                if (rw < spacing + panel.gridColumnMinimumWidth)
                    rw = spacing + panel.gridColumnMinimumWidth;

                var index = 0;
                var prev = -1;

                for (var i = 0; i < count; i++)
                {
                    var c = panel.buttons[i];
                    if (!c.GridVisibility)
                        continue;

                    int column, row, _x, _y, _rw;

                    if (panel.gridLayout == GridLayoutMode.Indexed)
                    {
                        ushort pk;
                        int ofs;

                        if (prev != -1)
                        {
                            pk = panel.buttons[prev].SortKey;
                            ofs = c.SortKey - pk;
                        }
                        else
                        {
                            var last = panel.buttons.Length - 1;

                            do
                            {
                                if (panel.buttons[last].GridVisibility)
                                    break;
                                --last;
                            }
                            while (last > 0);

                            if (panel.buttons[last].SortKey < c.SortKey)
                            {
                                pk = c.SortKey;
                                if (pk > 0)
                                    --pk;
                                ofs = columns - pk % columns;
                                pk = (ushort)(pk + columns - pk % columns - 1);
                            }
                            else
                            {
                                pk = 1;
                                ofs = c.SortKey;
                            }
                        }

                        prev = i;

                        if (ofs < 0)
                            ofs = -ofs;

                        if (ofs > 100)
                        {
                            if (c.SortKey == ushort.MaxValue || pk == ushort.MaxValue)
                            {
                                ofs = 0;
                            }
                        }

                        if (ofs > 1)
                        {
                            index += ofs - 1;
                        }
                    }

                    row = index / columns;
                    column = index % columns;
                    _y = spacing + row * (rh + spacing);
                    _x = spacing + (int)(column * rw);
                    _rw = (int)((column + 1) * rw) - _x;

                    if (apply)
                    {
                        c.SetBounds(_x, _y, _rw, rh);
                        c.GridIndex = (ushort)(columns * row + column);
                    }

                    ++index;
                }

                int h;

                if (index > 0)
                    h = spacing + ((index - 1) / columns) * (rh + spacing) + rh;
                else
                    h = 0;

                if (panel.isNewVisible)
                {
                    var column = 0;
                    var _y = spacing + h;
                    var _x = spacing;
                    var _rw = (int)((column + 1) * rw) - _x;
                    //var _rh = panel.buttonNew.Height;

                    if (apply)
                    {
                        panel.buttonNew.SetBounds(_x, _y, _rw, rh);
                        panel.buttonNew.Visible = true;
                    }

                    h = _y + rh;
                }

                return new Size((int)(rw * columns) + spacing, h + spacing);
            }
        }

        private AccountGridButtonContainerLayout layout;

        public event EventHandler ContentHeightChanged;

        public enum GridLayoutMode
        {
            /// <summary>
            /// Items will fill the grid from left to right, top down
            /// </summary>
            Auto,
            /// <summary>
            /// Items will be placed based on their indexed position
            /// </summary>
            Indexed
        }

        private int
            gridColumns,
            gridSpacing,
            gridColumnMinimumWidth,
            gridRowHeight;
        private GridLayoutMode gridLayout;
        private AccountGridButton[] buttons;
        private AccountGridButton buttonNew;
        private bool pending;
        private bool isNewVisible;
        private int contentHeight;
        private byte page;

        public AccountGridButtonPanel()
        {
            buttons = new AccountGridButton[0];
            layout = new AccountGridButtonContainerLayout();

            this.DoubleBuffered = true;

            gridColumns = 1;
            gridSpacing = 5;
            gridColumnMinimumWidth = 10;
            gridRowHeight = 60;
        }

        public override LayoutEngine LayoutEngine
        {
            get
            {
                return layout;
            }
        }

        /// <summary>
        /// Displays number of columns
        /// </summary>
        [DefaultValue(1)]
        public int GridColumns
        {
            get
            {
                return gridColumns;
            }
            set
            {
                if (gridColumns != value)
                {
                    gridColumns = value;
                    OnLayoutChanged();
                }
            }
        }

        public int GridRows
        {
            get;
            private set;
        }

        public GridLayoutMode GridLayout
        {
            get
            {
                return gridLayout;
            }
            set
            {
                if (gridLayout != value)
                {
                    gridLayout = value;
                    OnLayoutChanged();
                }
            }
        }

        /// <summary>
        /// Spacing between buttons
        /// </summary>
        [DefaultValue(5)]
        public int GridSpacing
        {
            get
            {
                return gridSpacing;
            }
            set
            {
                if (gridSpacing != value)
                {
                    gridSpacing = value;
                    OnLayoutChanged();
                }
            }
        }

        /// <summary>
        /// Minimum button width
        /// </summary>
        [DefaultValue(10)]
        public int GridColumnMinimumWidth
        {
            get
            {
                return gridColumnMinimumWidth;
            }
            set
            {
                if (gridColumnMinimumWidth != value)
                {
                    gridColumnMinimumWidth = value;
                    OnLayoutChanged();
                }
            }
        }

        /// <summary>
        /// Button row height
        /// </summary>
        [DefaultValue(65)]
        public int GridRowHeight
        {
            get
            {
                return gridRowHeight;
            }
            set
            {
                if (gridRowHeight != value)
                {
                    gridRowHeight = value;
                    OnLayoutChanged();
                }
            }
        }

        public int ContentHeight
        {
            get
            {
                return contentHeight;
            }
            private set
            {
                if (contentHeight != value)
                {
                    contentHeight = value;
                    if (ContentHeightChanged != null)
                        ContentHeightChanged(this, EventArgs.Empty);
                }
            }
        }

        public AccountGridButton[] Buttons
        {
            get
            {
                return buttons;
            }
        }

        public AccountGridButton NewButton
        {
            get
            {
                return buttonNew;
            }
            set
            {
                if (buttonNew != value)
                {
                    buttonNew = value;
                    this.Controls.Add(value);
                }
            }
        }

        [DefaultValue(false)]
        public bool NewButtonVisible
        {
            get
            {
                return isNewVisible;
            }
            set
            {
                if (isNewVisible != value)
                {
                    isNewVisible = value;
                    pending = true;
                    buttonNew.Visible = value;
                }
            }
        }

        public void Add(AccountGridButton button)
        {
            var existing = buttons.Length;
            var b = new AccountGridButton[existing + 1];

            Array.Copy(buttons, b, existing);
            b[existing] = button;

            button.Index = existing;

            this.buttons = b;
            this.pending = true;
            this.Controls.Add(button);
        }

        public void AddRange(params AccountGridButton[] buttons)
        {
            var existing = this.buttons.Length;
            var b = new AccountGridButton[existing + buttons.Length];

            if (existing > 0)
                Array.Copy(this.buttons, b, existing);
            Array.Copy(buttons, 0, b, existing, buttons.Length);

            for (var i = b.Length - 1; i >= existing; --i)
            {
                b[i].Index = i;
            }

            this.buttons = b;
            this.pending = true;
            this.Controls.AddRange(buttons);
        }

        public void Remove(AccountGridButton button)
        {
            var i = Array.IndexOf<AccountGridButton>(buttons, button);

            if (i != -1)
            {
                var b = new AccountGridButton[buttons.Length - 1];

                if (b.Length > 0)
                {
                    if (i > 0)
                    {
                        Array.Copy(buttons, b, i);
                    }

                    if (i < buttons.Length - 1)
                    {
                        Array.Copy(buttons, i + 1, b, i, b.Length - i);

                        for (var j = b.Length - 1; j >= i; --j)
                        {
                            b[j].Index = j;
                        }
                    }
                }

                this.buttons = b;
                this.pending = true;
                this.Controls.Remove(button);
            }
        }

        private void OnLayoutChanged()
        {
            if (!pending)
            {
                pending = true;
                if (this.IsHandleCreated)
                    this.BeginInvoke(new MethodInvoker(this.PerformLayout));
            }
        }

        public void UpdateLayout()
        {
            OnLayoutChanged();
        }

        public bool IsLayoutPending
        {
            get
            {
                return pending;
            }
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            if (pending)
            {
                this.PerformLayout();
            }

            base.OnPaint(e);
        }

        protected override void SetBoundsCore(int x, int y, int width, int height, BoundsSpecified specified)
        {
            if ((specified & BoundsSpecified.Width) == BoundsSpecified.Width)
            {
                if (!this.pending && this.Width != width)
                {
                    this.pending = true;
                }
            }

            base.SetBoundsCore(x, y, width, height, specified);
        }

        public int GetContentHeight(int columns)
        {
            return layout.DoLayout(this, this.Size, false, true, columns).Height;
        }

        public override Size GetPreferredSize(Size proposedSize)
        {
            return layout.DoLayout(this, proposedSize, false, true, gridColumns);
        }
    }
}
