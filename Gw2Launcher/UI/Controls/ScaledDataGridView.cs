using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Gw2Launcher.UI.Controls
{
    class ScaledDataGridView : DataGridView
    {
        public class GridUpdate : IDisposable
        {
            public GridUpdate(ScaledDataGridView grid, bool copy)
            {
                grid.SuspendLayout();

                this.Grid = grid;
                if (copy)
                {
                    this.Rows = grid.GetRows();
                }
                this.Selected = grid.GetSelected();
                this.Scroll = true;

                grid.Rows.Clear();
            }

            /// <summary>
            /// Owning DataGrid
            /// </summary>
            public ScaledDataGridView Grid
            {
                get;
                private set;
            }

            /// <summary>
            /// Rows to be added to the grid
            /// </summary>
            public DataGridViewRow[] Rows
            {
                get;
                set;
            }

            /// <summary>
            /// Rows to be selected
            /// </summary>
            public DataGridViewRow[] Selected
            {
                get;
                set;
            }

            /// <summary>
            /// Scrolls to the first selected row
            /// </summary>
            public bool Scroll
            {
                get;
                set;
            }

            public void Dispose()
            {
                if (Grid != null)
                {
                    Grid.EndUpdate(this);
                    Grid = null;
                }
            }
        }

        public ScaledDataGridView()
            : base()
        {
        }

        [System.ComponentModel.DesignerSerializationVisibility(System.ComponentModel.DesignerSerializationVisibility.Hidden)]
        public bool UseTemplate
        {
            get
            {
                return EditMode == DataGridViewEditMode.EditProgrammatically;
            }
            set
            {
                if (value)
                {
                    EditMode = DataGridViewEditMode.EditProgrammatically;
                    DefaultCellStyle.SelectionBackColor = System.Drawing.SystemColors.ControlLight;
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
                }
                else
                {
                    EditMode = DataGridViewEditMode.EditOnEnter;
                }
            }
        }

        protected override void ScaleControl(System.Drawing.SizeF factor, BoundsSpecified specified)
        {
            base.ScaleControl(factor, specified);

            var scale = factor.Width;
            if (scale != 1f && specified == BoundsSpecified.All)
            {
                foreach (DataGridViewColumn column in this.Columns)
                {
                    if (column.AutoSizeMode == DataGridViewAutoSizeColumnMode.NotSet && column.InheritedAutoSizeMode == DataGridViewAutoSizeColumnMode.None || column.AutoSizeMode == DataGridViewAutoSizeColumnMode.None)
                    {
                        column.Width = (int)(column.Width * scale + 0.5f);
                    }
                }

                if (AutoSizeRowsMode == DataGridViewAutoSizeRowsMode.None)
                {
                    foreach (DataGridViewRow row in this.Rows)
                    {
                        row.Height = (int)(row.Height * scale + 0.5f);
                    }
                }

                RowTemplate.Height = (int)(RowTemplate.Height * scale + 0.5f);
            }
        }

        public bool ScrollToRow(params DataGridViewRow[] rows)
        {
            for (var i = 0; i < rows.Length; i++)
            {
                if (rows[i] == null)
                {
                    break;
                }
                else if (rows[i].Visible && ScrollToRow(rows[i].Index))
                {
                    return true;
                }
            }

            return false;
        }

        public bool ScrollToRow(int index)
        {
            if (index < 0 || index >= this.RowCount || this.Height == 0)
                return false;

            try
            {
                if (!IsHandleCreated)
                    CreateHandle();
                var displayed = this.DisplayedRowCount(false);
                var current = this.FirstDisplayedScrollingRowIndex;

                if (index < current)
                {
                    //when the row requires scrolling up to see
                    index = index - displayed / 2; //centered
                }
                else if (index >= current + displayed)
                {
                    //when the row requires scrolling down to see
                    index = index - displayed / 2; //centered
                }
                else
                {
                    return false;
                }

                if (index < 0)
                    index = 0;

                this.FirstDisplayedScrollingRowIndex = index;

                return true;
            }
            catch
            {
            }

            return false;
        }

        public bool ScrollToSelected()
        {
            foreach (DataGridViewRow row in this.SelectedRows)
            {
                return ScrollToRow(row.Index);
            }

            return false;
        }

        /// <summary>
        /// Returns selected rows in index order
        /// </summary>
        /// <returns></returns>
        public DataGridViewRow[] GetSelected()
        {
            var selected = new DataGridViewRow[this.SelectedRows.Count];

            if (selected.Length > 0)
            {
                this.SelectedRows.CopyTo(selected, 0);

                if (selected.Length > 1)
                {
                    Array.Sort(selected, delegate(DataGridViewRow a, DataGridViewRow b)
                    {
                        return a.Index.CompareTo(b.Index);
                    });
                }
            }

            return selected;
        }

        public DataGridViewRow[] GetRows()
        {
            var rows = new DataGridViewRow[this.Rows.Count];
            this.Rows.CopyTo(rows, 0);
            return rows;
        }

        public void Filter(Func<DataGridViewRow, bool> isVisible)
        {
            using (var o = BeginUpdate(true))
            {
                var rows = o.Rows;

                for (var i = 0; i < rows.Length; i++)
                {
                    rows[i].Visible = isVisible(rows[i]);
                }
            }
        }

        /// <summary>
        /// Begins a mass update to all rows, removing all rows to be re-added on completion
        /// </summary>
        /// <param name="copyRows">Copies all current rows before clearing</param>
        public GridUpdate BeginUpdate(bool copyRows)
        {
            return new GridUpdate(this, copyRows);
        }

        private void EndUpdate(GridUpdate o)
        {
            if (o.Grid == this)
            {
                if (o.Rows != null)
                {
                    this.Rows.AddRange(o.Rows);
                }

                if (o.Selected != null)
                {
                    Select(o.Selected);
                }

                ScrollToSelected();

                this.ResumeLayout();
            }
        }

        public void Select(params DataGridViewRow[] rows)
        {
            for (var i = 0; i < rows.Length; i++)
            {
                if (rows[i].Visible)
                {
                    rows[i].Selected = true;
                }
            }
        }

        /// <summary>
        /// Moves selected rows up/down by 1 row, returning true if rows were changed
        /// </summary>
        /// <param name="scroll">Scrolls to the selected row</param>
        public bool MoveSelectedRows(bool up, bool scroll)
        {
            var selected = GetSelected();

            if (selected.Length == 0)
            {
                return false;
            }

            var count = this.Rows.Count;
            var ofs = up ? -1 : 1;
            var changed = false;

            this.SuspendLayout();

            for (var i = 0; i < selected.Length; i++)
            {
                DataGridViewRow r1, r2;

                if (up)
                {
                    r1 = selected[i];
                }
                else
                {
                    r1 = selected[selected.Length - i - 1];
                }

                if (up)
                {
                    if (r1.Index <= 0)
                        continue;
                }
                else
                {
                    if (r1.Index == count - 1)
                        continue;
                }

                var iofs = r1.Index + ofs;

                r2 = this.Rows[iofs];

                if (r2.Selected)
                    continue;

                if (selected.Length == 1)
                {
                    //select the row to prevent auto selecting another
                    r2.Selected = true;

                    this.Rows.Remove(r1);
                    this.Rows.Insert(iofs, r1);

                    r2.Selected = false;
                }
                else
                {
                    this.Rows.Remove(r1);
                    this.Rows.Insert(iofs, r1);
                }

                r1.Selected = true;
                changed = true;
            }

            if (scroll)
            {
                ScrollToRow(selected[up ? 0 : selected.Length - 1].Index);
            }

            this.ResumeLayout();

            return changed;
        }
    }
}
