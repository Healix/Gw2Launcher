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
    }
}
