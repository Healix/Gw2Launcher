using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Drawing;

namespace Gw2Launcher.UI.Controls
{
    class SelectionDataGridView : ScaledDataGridView
    {
        private class SelectionState
        {
            public bool Selected;
            public HightlightMode Highlight;
        }

        private enum HightlightMode
        {
            None,
            Add,
            Remove,
        }

        private DataGridViewRow selected;
        private DataGridViewColumn columnState;

        private int selectionStart = -1;
        private bool selectionValue;
        private int selectionLast;
        private MouseButtons selectionButton;

        public SelectionDataGridView()
            : base()
        {
            columnState = new DataGridViewTextBoxColumn()
            {
                AutoSizeMode = DataGridViewAutoSizeColumnMode.None,
                Width = 0,
                Visible = false,
            };

            EditMode = DataGridViewEditMode.EditProgrammatically;
            DefaultCellStyle.SelectionBackColor = SystemColors.ControlLight;
            DefaultCellStyle.SelectionForeColor = DefaultCellStyle.ForeColor;
            AllowUserToAddRows = false;
            AllowUserToDeleteRows = false;
            AllowUserToResizeColumns = false;
            AllowUserToResizeRows = false;
            BackgroundColor = System.Drawing.SystemColors.Window;
            BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            CellBorderStyle = System.Windows.Forms.DataGridViewCellBorderStyle.None;
            ColumnHeadersVisible = false;
            ReadOnly = true;
            RowHeadersVisible = false;
            ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            DoubleBuffered = true;

            RowHighlightSelectedColor = Color.LightSteelBlue;
            RowHighlightDeselectedColor = Color.FromArgb(Color.LightSteelBlue.B, Color.LightSteelBlue.R, Color.LightSteelBlue.G);

#if DEBUG
            this.ColumnAdded += Initialize_ColumnAdded;
#else
            this.Columns.Add(columnState);
#endif
        }


#if DEBUG
        void Initialize_ColumnAdded(object sender, DataGridViewColumnEventArgs e)
        {
            this.ColumnAdded -= Initialize_ColumnAdded;
            if (!DesignMode)
                this.Columns.Add(columnState);
        }
#endif

        public Color RowHighlightSelectedColor
        {
            get;
            set;
        }

        public Color RowHighlightDeselectedColor
        {
            get;
            set;
        }

        protected override void OnSelectionChanged(EventArgs e)
        {
            base.OnSelectionChanged(e);
            ClearSelection();
        }

        private Color GetRowColor(SelectionState state)
        {
            switch (state.Highlight)
            {
                case HightlightMode.Add:

                    return RowHighlightSelectedColor;

                case HightlightMode.Remove:

                    return RowHighlightDeselectedColor;
            }

            return state.Selected ? DefaultCellStyle.SelectionBackColor : DefaultCellStyle.BackColor;
        }

        private void HighlightRow(DataGridViewRow row, HightlightMode m)
        {
            var state = GetSelectionState(row);

            if (m == HightlightMode.None)
            {
                if (state.Highlight == HightlightMode.None)
                    return;
                state.Highlight = HightlightMode.None;
            }
            else
            {
                if (state.Highlight == m)
                    return;
                state.Highlight = m;
            }

            row.DefaultCellStyle.BackColor = GetRowColor(state);
        }

        public void SelectRow(DataGridViewRow row, bool selected)
        {
            var state = GetSelectionState(row);
            if (state.Selected == selected)
                return;
            if (selected && !MultiSelect)
                this.selected = row;
            state.Selected = selected;
            row.DefaultCellStyle.BackColor = GetRowColor(state);
        }

        private SelectionState GetSelectionState(DataGridViewRow row)
        {
            var state = (SelectionState)row.Cells[columnState.Index].Value;
            if (state == null)
                row.Cells[columnState.Index].Value = state = new SelectionState();

            return state;
        }

        protected override void OnKeyDown(KeyEventArgs e)
        {
            if (e.Control && e.KeyCode == Keys.A && MultiSelect)
            {
                e.Handled = true;
                e.SuppressKeyPress = true;

                if (selectionStart != -1)
                {
                    grid_MouseUp(null, null);
                }

                for (var i = Rows.Count - 1; i >= 0; --i)
                {
                    SelectRow(Rows[i], true);
                }
            }

            base.OnKeyDown(e);
        }

        protected override void OnCellMouseDown(DataGridViewCellMouseEventArgs e)
        {
            base.OnCellMouseDown(e);

            if (e.Button == System.Windows.Forms.MouseButtons.Left && e.RowIndex >= 0)
            {
                var row = this.Rows[e.RowIndex];
                var state = GetSelectionState(row);

                if (MultiSelect)
                {
                    state.Selected = !state.Selected;
                    state.Highlight = state.Selected ? HightlightMode.Add : HightlightMode.Remove;
                }
                else
                {
                    state.Selected = true;

                    if (selected != row)
                    {
                        if (selected != null)
                        {
                            SelectRow(selected, false);
                        }

                        selected = row;
                    }
                }

                selectionButton = e.Button;
                selectionStart = selectionLast = e.RowIndex;
                selectionValue = state.Selected;

                CellMouseMove += grid_CellMouseMove;
                LostFocus += grid_LostFocus;
                MouseUp += grid_MouseUp;

                row.DefaultCellStyle.BackColor = GetRowColor(state);
            }
        }

        void grid_MouseUp(object sender, MouseEventArgs e)
        {
            CellMouseMove -= grid_CellMouseMove;
            LostFocus -= grid_LostFocus;
            MouseUp -= grid_MouseUp;

            if (MultiSelect)
            {
                int from, to;

                if (selectionLast > selectionStart)
                {
                    from = selectionStart;
                    to = selectionLast;
                }
                else
                {
                    from = selectionLast;
                    to = selectionStart;
                }

                var b = selectionValue;

                for (var i = from; i <= to; i++)
                {
                    var row = this.Rows[i];
                    var state = GetSelectionState(row);
                    state.Highlight = HightlightMode.None;
                    state.Selected = b;
                    row.DefaultCellStyle.BackColor = GetRowColor(state);
                }
            }

            selectionStart = -1;
        }

        void grid_LostFocus(object sender, EventArgs e)
        {
            grid_MouseUp(null, null);
        }

        void grid_CellMouseMove(object sender, DataGridViewCellMouseEventArgs e)
        {
            if (e.Button != selectionButton)
            {
                grid_MouseUp(null, null);
                return;
            }

            var ri = e.RowIndex;

            if (ri >= 0 && selectionLast != ri)
            {
                if (!MultiSelect)
                {
                    SelectRow(selected, false);
                    SelectRow(selected = this.Rows[ri], true);
                    selectionLast = ri;
                    return;
                }

                int from, to;

                if (ri > selectionLast)
                {
                    from = selectionLast;
                    to = ri;
                    if (ri < selectionStart)
                        to--;
                }
                else
                {
                    from = ri;
                    to = selectionLast;
                    if (ri > selectionStart)
                        from++;
                }

                for (var i = from; i <= to; i++)
                {
                    var row = this.Rows[i];

                    if (i != selectionStart)
                    {
                        bool b;
                        if (ri > selectionStart)
                            b = i >= selectionStart && i <= ri;
                        else
                            b = i <= selectionStart && i >= ri;

                        HighlightRow(row, b ? (selectionValue ? HightlightMode.Add : HightlightMode.Remove) : HightlightMode.None);
                    }
                }

                selectionLast = ri;
            }
        }

        public IEnumerable<DataGridViewRow> GetSelectedEnumerable()
        {
            if (MultiSelect)
            {
                var l = this.Rows.Count;
                for (var i = 0; i < l; i++)
                {
                    var row = this.Rows[i];
                    var state = (SelectionState)row.Cells[columnState.Index].Value;

                    if (state != null && state.Selected && row.Visible)
                        yield return row;
                }
            }
            else if (selected != null && selected.Visible)
            {
                yield return selected;
            }
        }

        public bool IsSelected(int row)
        {
            var _row = this.Rows[row];
            var state = (SelectionState)_row.Cells[columnState.Index].Value;

            return (state != null && state.Selected && _row.Visible);
        }

        public IList<DataGridViewRow> GetSelected()
        {
            if (MultiSelect)
            {
                var l = this.Rows.Count;
                var rows = new List<DataGridViewRow>(l);
                for (var i = 0; i < l; i++)
                {
                    var row = this.Rows[i];
                    var state = (SelectionState)row.Cells[columnState.Index].Value;

                    if (state != null && state.Selected && row.Visible)
                        rows.Add(row);
                }
                return rows;
            }
            else if (selected != null && selected.Visible)
            {
                return new DataGridViewRow[] { selected };
            }
            else
            {
                return new DataGridViewRow[0];
            }
        }

        public void ClearSelected()
        {
            if (MultiSelect)
            {
                var l = this.Rows.Count;
                for (var i = 0; i < l; i++)
                {
                    var row = this.Rows[i];
                    var state = (SelectionState)row.Cells[columnState.Index].Value;

                    if (state != null && state.Selected)
                    {
                        state.Selected = false;
                        row.DefaultCellStyle.BackColor = GetRowColor(state);
                    }
                }
            }
            else if (selected != null)
            {
                var state = (SelectionState)selected.Cells[columnState.Index].Value;

                if (state != null && state.Selected)
                {
                    state.Selected = false;
                    selected.DefaultCellStyle.BackColor = GetRowColor(state);
                }
            }

            selected = null;
        }
    }
}
