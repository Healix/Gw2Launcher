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
        private Util.AccountComparer comparer;
        
        private formAccountSelect(string title, bool multiSelect)
        {
            InitializeComponents();

            comparer = new Util.AccountComparer(new Settings.SortingOptions(Settings.SortMode.Name));
            nameToolStripMenuItem.Checked = true;

            gridAccounts.MultiSelect = multiSelect;
            labelHeight = labelTitle.Height;

            //labelTitle.MaximumSize = new Size(this.ClientSize.Width - labelTitle.Location.X * 2, this.Height);
            labelTitle.Text = title;
        }

        public formAccountSelect(string title, IEnumerable<KeyValuePair<Settings.IAccount, bool>> accounts, bool multiSelect)
            : this(title, multiSelect)
        {
            var selected = false;
            gridAccounts.SuspendLayout();

            foreach (var kv in accounts)
            {
                var row = (DataGridViewRow)gridAccounts.RowTemplate.Clone();
                row.CreateCells(gridAccounts);

                var isChecked = kv.Value && (multiSelect || !selected);

                row.Cells[columnName.Index].Value = new AccountCell(kv.Key);
                
                gridAccounts.Rows.Add(row);

                if (isChecked)
                {
                    selected = true;
                    gridAccounts.SelectRow(row, isChecked);
                }
            }

            Sort();

            gridAccounts.ResumeLayout();
        }

        public formAccountSelect(string title, IEnumerable<Settings.IAccount> accounts, bool isDefaultChecked, bool multiSelect, Settings.AccountType? filter = null, Func<Settings.IAccount, bool> isSelected = null)
            : this(title, multiSelect)
        {
            DataGridViewRow selected = null;
            gridAccounts.SuspendLayout();

            if (filter.HasValue)
            {
                guildWars1ToolStripMenuItem.Checked = filter.Value == Settings.AccountType.GuildWars1;
                guildWars2ToolStripMenuItem.Checked = filter.Value == Settings.AccountType.GuildWars2;
            }

            foreach (var account in accounts)
            {
                var row = (DataGridViewRow)gridAccounts.RowTemplate.Clone();
                row.CreateCells(gridAccounts);

                var isChecked = isDefaultChecked && (multiSelect || selected == null);

                if (isSelected != null)
                {
                    if (isChecked = isSelected(account))
                        selected = row;
                }

                row.Cells[columnName.Index].Value = new AccountCell(account);

                if (filter.HasValue)
                {
                    row.Visible = account.Type == filter.Value;
                }

                gridAccounts.Rows.Add(row);

                if (isChecked)
                {
                    if (multiSelect)
                        gridAccounts.SelectRow(row, isChecked);
                    else if (selected == null)
                        selected = row;
                }
            }

            if (!multiSelect && selected != null)
                gridAccounts.SelectRow(selected, true);

            Sort();

            gridAccounts.ResumeLayout();
        }

        protected override void OnHandleCreated(EventArgs e)
        {
            base.OnHandleCreated(e);

            ResizeGrid();
        }

        private void Sort()
        {
            gridAccounts.Sort(Comparer<DataGridViewRow>.Create(new Comparison<DataGridViewRow>(
                delegate(DataGridViewRow a, DataGridViewRow b)
                {
                    var c1 = (AccountCell)a.Cells[columnName.Index].Value;
                    var c2 = (AccountCell)b.Cells[columnName.Index].Value;

                    return comparer.Compare(c1.account, c2.account);
                })));
        }

        protected override void OnInitializeComponents()
        {
            base.OnInitializeComponents();

            InitializeComponent();
        }

        private void ResizeGrid()
        {
            var sz = gridAccounts.GetPreferredSize(new Size(gridAccounts.Width, int.MaxValue));

            if (sz.Height < gridAccounts.Height)
            {
                this.Height -= gridAccounts.Height - sz.Height;
            }
            else
            {
                var max = Screen.FromControl(this).WorkingArea.Height * 3 / 4;
                var h = this.Height + sz.Height - gridAccounts.Height;
                if (h < max)
                    this.Height = h;
            }

            foreach (var row in gridAccounts.GetSelectedEnumerable())
            {
                gridAccounts.ScrollToRow(row.Index);
                break;
            }
        }

        public List<Settings.IAccount> Selected
        {
            get;
            private set;
        }

        private void buttonOK_Click(object sender, EventArgs e)
        {
            var rows = gridAccounts.GetSelected();
            var accounts = Selected = new List<Settings.IAccount>(rows.Count);

            foreach (var row in rows)
            {
                accounts.Add(((AccountCell)row.Cells[columnName.Index].Value).account);
            }
            
            this.DialogResult = DialogResult.OK;
        }

        private void buttonCancel_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
        }

        private void sortMenuItem_Click(object sender, EventArgs e)
        {
            var c = ((ToolStripMenuItem)sender);
            c.Checked = !c.Checked;

            var items = new ToolStripMenuItem[] { nameToolStripMenuItem, lastUsedToolStripMenuItem };
            if (Array.IndexOf<ToolStripMenuItem>(items, c) != -1)
            {
                foreach (var t in items)
                {
                    if (t != c)
                    {
                        t.Checked = false;
                    }
                }
            }

            var descending = descendingToolStripMenuItem.Checked;
            var sorting = Settings.SortMode.None;

            if (nameToolStripMenuItem.Checked)
                sorting = Settings.SortMode.Name;
            else if (lastUsedToolStripMenuItem.Checked)
                sorting = Settings.SortMode.LastUsed;

            comparer = new Util.AccountComparer(new Settings.SortingOptions(sorting, descending));

            Sort();
        }

        private void filterMenuItem_Click(object sender, EventArgs e)
        {
            var c = ((ToolStripMenuItem)sender);
            c.Checked = !c.Checked;

            if (c == guildWars1ToolStripMenuItem)
                guildWars2ToolStripMenuItem.Checked = false;
            else
                guildWars1ToolStripMenuItem.Checked = false;

            var type = guildWars1ToolStripMenuItem.Checked ? Settings.AccountType.GuildWars1 : Settings.AccountType.GuildWars2;
            var all = !guildWars1ToolStripMenuItem.Checked && !guildWars2ToolStripMenuItem.Checked;

            gridAccounts.SuspendLayout();
            foreach (DataGridViewRow row in gridAccounts.Rows)
            {
                var a = ((AccountCell)row.Cells[columnName.Index].Value).account;
                row.Visible = all || (a != null && a.Type == type);
            }
            gridAccounts.ResumeLayout();
        }

        private void clearSelectionToolStripMenuItem_Click(object sender, EventArgs e)
        {
            gridAccounts.ClearSelected();
        }
    }
}
