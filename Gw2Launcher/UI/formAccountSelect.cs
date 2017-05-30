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
    public partial class formAccountSelect : Form
    {
        private struct AccountCell
        {
            public AccountCell(Settings.IAccount account)
            {
                this.account = account;
            }

            public Settings.IAccount account;

            public override string ToString()
            {
                if (account != null)
                    return account.Name;
                else
                    return "Default";
            }
        }

        private int labelHeight;
        private DataGridViewCell lastSelected;
        private bool multiSelect;

        public formAccountSelect(string title, IEnumerable<Settings.IAccount> accounts, bool isDefaultChecked, bool multiSelect)
        {
            InitializeComponent();

            this.multiSelect = multiSelect;
            labelHeight = labelTitle.Height;

            gridAccounts.AdvancedCellBorderStyle.All = DataGridViewAdvancedCellBorderStyle.None;

            foreach (var account in accounts)
            {
                var row = (DataGridViewRow)gridAccounts.RowTemplate.Clone();
                row.CreateCells(gridAccounts);

                bool isChecked = isDefaultChecked && (multiSelect || lastSelected == null);

                row.Cells[columnCheck.Index].Value = isChecked;
                row.Cells[columnName.Index].Value = new AccountCell(account);

                if (isChecked)
                    lastSelected = row.Cells[columnCheck.Index];

                gridAccounts.Rows.Add(row);
            }

            labelTitle.MaximumSize = new Size(this.ClientSize.Width - labelTitle.Location.X, this.Height);
            labelTitle.Text = title;
        }

        public List<Settings.IAccount> Selected
        {
            get;
            private set;
        }

        private void formAccountSelect_Load(object sender, EventArgs e)
        {

        }

        private void gridServers_SelectionChanged(object sender, EventArgs e)
        {
            gridAccounts.ClearSelection();
        }

        private void buttonOK_Click(object sender, EventArgs e)
        {
            var accounts = Selected = new List<Settings.IAccount>();
            foreach (DataGridViewRow row in gridAccounts.Rows)
            {
                if ((bool)row.Cells[columnCheck.Index].Value == true)
                {
                    Settings.IAccount account = ((AccountCell)row.Cells[columnName.Index].Value).account;
                    accounts.Add(account);
                }
            }
            this.DialogResult = DialogResult.OK;
        }

        private void buttonCancel_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
        }

        private void gridAccounts_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0)
            {
                var c = gridAccounts.Rows[e.RowIndex].Cells[columnCheck.Index];
                if (!multiSelect && lastSelected != c && lastSelected != null)
                    lastSelected.Value = false;
                c.Value = !(bool)c.Value;
            }
        }

        private void labelTitle_SizeChanged(object sender, EventArgs e)
        {
            if (labelHeight > 0)
            {
                var h = labelTitle.Height;
                this.Height += (h - labelHeight);
                labelHeight = h;
            }
        }
    }
}
