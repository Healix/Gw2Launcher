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
    public partial class formDnsDialog : Form
    {
        static formDnsDialog()
        {
        }

        public formDnsDialog()
            : this(null)
        {

        }

        public formDnsDialog(IEnumerable<IPAddress> selected)
        {
            InitializeComponent();

            gridServers.AdvancedCellBorderStyle.All = DataGridViewAdvancedCellBorderStyle.None;

            bool defaultSelect = selected == null;
            HashSet<IPAddress> ips = defaultSelect ? null : new HashSet<IPAddress>(selected);
            
            foreach (var server in Net.DnsServers.Servers)
            {
                var row = (DataGridViewRow)gridServers.RowTemplate.Clone();
                row.CreateCells(gridServers);

                row.Cells[columnCheck.Index].Value = defaultSelect || ips.Contains(server.IP[0]);
                row.Cells[columnServer.Index].Value = server;

                gridServers.Rows.Add(row);
            }
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
                if ((bool)row.Cells[columnCheck.Index].Value == true)
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
                var c = gridServers.Rows[e.RowIndex].Cells[columnCheck.Index];
                c.Value = !(bool)c.Value;
            }
        }
    }
}
