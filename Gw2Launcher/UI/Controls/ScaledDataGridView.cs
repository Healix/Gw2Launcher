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

        public void ScrollToRow(int index)
        {
            if (index < 0 || index >= this.RowCount || this.Height == 0)
                return;

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
                    return;
                }

                if (index < 0)
                    index = 0;

                this.FirstDisplayedScrollingRowIndex = index;
            }
            catch
            {
            }
        }
    }
}
