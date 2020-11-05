using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Net;

namespace Gw2Launcher.UI
{
    public partial class formDnsDialog : Base.BaseForm
    {
        private int selectedCount;
        
        public formDnsDialog()
            : this(null)
        {

        }

        public formDnsDialog(IEnumerable<IPAddress> selected)
        {
            InitializeComponents();

            gridServers.AdvancedCellBorderStyle.All = DataGridViewAdvancedCellBorderStyle.None;

            bool defaultSelect = selected == null;
            HashSet<IPAddress> ips = defaultSelect ? null : new HashSet<IPAddress>(selected);

            var row = (DataGridViewRow)gridServers.RowTemplate.Clone();
            row.CreateCells(gridServers);
            row.Cells[columnCheck.Index].Value = CheckState.Checked;
            row.Cells[columnServer.Index].Value = "";
            gridServers.Rows.Add(row);

            foreach (var server in Net.DnsServers.Servers)
            {
                row = (DataGridViewRow)gridServers.RowTemplate.Clone();
                row.CreateCells(gridServers);

                bool isChecked = defaultSelect || ips.Contains(server.IP[0]);
                row.Cells[columnCheck.Index].Value = isChecked ? CheckState.Checked : CheckState.Unchecked;
                row.Cells[columnServer.Index].Value = server;

                if (isChecked)
                    selectedCount++;

                gridServers.Rows.Add(row);
            }

            var count = gridServers.Rows.Count - 1;
            var c = (DataGridViewCheckBoxCell)gridServers.Rows[0].Cells[columnCheck.Index];
            if (count == selectedCount)
                c.Value = CheckState.Checked;
            else if (selectedCount == 0)
                c.Value = CheckState.Unchecked;
            else
                c.Value = CheckState.Indeterminate;
        }

        protected override void OnInitializeComponents()
        {
            base.OnInitializeComponents();

            InitializeComponent();
        }

        public List<IPAddress> IPs
        {
            get;
            private set;
        }

        private void formDnsDialog_Load(object sender, EventArgs e)
        {

        }

        private void gridServers_SelectionChanged(object sender, EventArgs e)
        {
            gridServers.ClearSelection();
        }

        private void buttonOK_Click(object sender, EventArgs e)
        {
            var ips = IPs = new List<IPAddress>();
            foreach (DataGridViewRow row in gridServers.Rows)
            {
                if (row.Index == 0)
                    continue;

                if ((CheckState)row.Cells[columnCheck.Index].Value == CheckState.Checked)
                {
                    var dns = (Net.DnsServers.DnsServer)row.Cells[columnServer.Index].Value;
                    ips.AddRange(dns.IP);
                }
            }
            this.DialogResult = DialogResult.OK;
        }

        private void buttonCancel_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
        }

        private void gridServers_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0)
            {
                var c = (DataGridViewCheckBoxCell)gridServers.Rows[e.RowIndex].Cells[columnCheck.Index];
                bool isChecked = (CheckState)c.Value != CheckState.Checked;
                c.Value = isChecked ? CheckState.Checked : CheckState.Unchecked;

                if (e.RowIndex == 0)
                {
                    foreach (DataGridViewRow row in gridServers.Rows)
                    {
                        row.Cells[columnCheck.Index].Value = c.Value;
                    }

                    if (isChecked)
                        selectedCount = gridServers.Rows.Count - 1;
                    else
                        selectedCount = 0;
                }
                else
                {
                    if (isChecked)
                        selectedCount++;
                    else
                        selectedCount--;

                    var count = gridServers.Rows.Count - 1;
                    c = (DataGridViewCheckBoxCell)gridServers.Rows[0].Cells[columnCheck.Index];
                    if (count == selectedCount)
                        c.Value = CheckState.Checked;
                    else if (selectedCount == 0)
                        c.Value = CheckState.Unchecked;
                    else
                        c.Value = CheckState.Indeterminate;
                }
            }
        }
    }
}
