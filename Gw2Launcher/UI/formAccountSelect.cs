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
    public partial class formAccountSelect : Base.BaseForm
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
        
        private formAccountSelect(string title, bool multiSelect)
        {
            InitializeComponents();

            this.multiSelect = multiSelect;
            labelHeight = labelTitle.Height;

            gridAccounts.RowTemplate.Height = gridAccounts.Font.Height * 3 / 2;
            gridAccounts.AdvancedCellBorderStyle.All = DataGridViewAdvancedCellBorderStyle.None;

            var row = (DataGridViewRow)gridAccounts.RowTemplate.Clone();
            row.CreateCells(gridAccounts);
            row.Cells[columnCheck.Index].Value = false;
            gridAccounts.Rows.Add(row);

            labelTitle.MaximumSize = new Size(this.ClientSize.Width - labelTitle.Location.X * 2, this.Height);
            labelTitle.Text = title;
        }

        public formAccountSelect(string title, IEnumerable<KeyValuePair<Settings.IAccount, bool>> accounts, bool multiSelect)
            : this(title, multiSelect)
        {
            gridAccounts.SuspendLayout();

            foreach (var kv in accounts)
            {
                var row = (DataGridViewRow)gridAccounts.RowTemplate.Clone();
                row.CreateCells(gridAccounts);

                bool isChecked = kv.Value && (multiSelect || lastSelected == null);

                row.Cells[columnCheck.Index].Value = isChecked;
                row.Cells[columnName.Index].Value = new AccountCell(kv.Key);

                if (isChecked)
                    lastSelected = row.Cells[columnCheck.Index];

                gridAccounts.Rows.Add(row);
            }

            gridAccounts.ResumeLayout();

            ResizeGrid();
        }

        public formAccountSelect(string title, IEnumerable<Settings.IAccount> accounts, bool isDefaultChecked, bool multiSelect)
            : this(title, multiSelect)
        {
            gridAccounts.SuspendLayout();

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

            gridAccounts.ResumeLayout();

            ResizeGrid();
        }

        protected override void OnInitializeComponents()
        {
            base.OnInitializeComponents();

            InitializeComponent();
        }

        private void ResizeGrid()
        {
            var h = gridAccounts.Rows.Count * gridAccounts.RowTemplate.Height;
            var sz = gridAccounts.GetPreferredSize(new Size(gridAccounts.Width, int.MaxValue));

            if (sz.Height < gridAccounts.Height)
            {
                this.Height -= gridAccounts.Height - sz.Height;
            }
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
                var b = !(bool)c.Value;
                
                c.Value = b;

                if (multiSelect && e.RowIndex == 0)
                {
                    for (var i = gridAccounts.Rows.Count - 1; i > 0; --i)
                    {
                        gridAccounts.Rows[i].Cells[columnCheck.Index].Value = b;
                    }
                }
                else
                {
                    if (!multiSelect && lastSelected != c && lastSelected != null)
                        lastSelected.Value = false;
                    lastSelected = c;
                }
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
