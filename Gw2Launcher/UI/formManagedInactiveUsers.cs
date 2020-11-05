using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Gw2Launcher.UI
{
    public partial class formManagedInactiveUsers : Base.BaseForm
    {
        public formManagedInactiveUsers()
        {
            InitializeComponents();

            int _space = buttonOK.Location.X - buttonCancel.Bounds.Right;
            buttonCancel.Location = new Point(this.ClientSize.Width / 2 - (buttonOK.Bounds.Right - buttonCancel.Bounds.Left) / 2, buttonCancel.Location.Y);
            buttonOK.Location = new Point(buttonCancel.Bounds.Right + _space, buttonCancel.Location.Y);

            HashSet<string> users = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            foreach (var a in Util.Accounts.GetAccounts())
            {
                users.Add(Util.Users.GetUserName(a.WindowsAccount));
            }

            foreach (string user in Settings.HiddenUserAccounts.GetKeys())
            {
                users.Add(user);
            }

            users.Remove(Util.Users.UserName);

            var _users = users.ToArray<string>();
            Array.Sort(_users, StringComparer.OrdinalIgnoreCase);
            DataGridViewRow[] rows = new DataGridViewRow[_users.Length];
            int i = 0;

            foreach (string user in _users)
            {
                var row = new DataGridViewRow();
                row.CreateCells(gridUsers);
                row.Height = gridUsers.RowTemplate.Height;

                row.Cells[columnHidden.Index].Value = Settings.HiddenUserAccounts.Contains(user) && Settings.HiddenUserAccounts[user].Value;
                row.Cells[columnUser.Index].Value = user;

                rows[i++] = row;
            }

            gridUsers.Rows.AddRange(rows);
        }

        protected override void OnInitializeComponents()
        {
            base.OnInitializeComponents();

            InitializeComponent();
        }

        private void formManagedInactiveUsers_Load(object sender, EventArgs e)
        {

        }

        private void gridUsers_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.ColumnIndex == columnHidden.Index)
            {
                var cell = (DataGridViewCheckBoxCell)gridUsers.Rows[e.RowIndex].Cells[e.ColumnIndex];
                cell.Value = !((bool)cell.Value);
            }
        }

        private void gridUsers_SelectionChanged(object sender, EventArgs e)
        {
            gridUsers.ClearSelection();
        }

        private void buttonCancel_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
        }

        private void buttonOK_Click(object sender, EventArgs e)
        {
            bool wasEnabled = false;
            bool isEnabled = false;

            foreach (DataGridViewRow row in gridUsers.Rows)
            {
                string username = (string)row.Cells[columnUser.Index].Value;
                bool enabled = (bool)row.Cells[columnHidden.Index].Value;

                if (Settings.HiddenUserAccounts.Contains(username) && Settings.HiddenUserAccounts[username].Value)
                    wasEnabled = true;

                if (enabled)
                    isEnabled = true;

                if (wasEnabled && isEnabled)
                    break;
            }

            if (isEnabled)
            {
                try
                {
                    if (!Util.ProcessUtil.CreateInactiveUsersTask())
                        throw new Exception();
                }
                catch (Exception ex)
                {
                    Util.Logging.Log(ex);
                    MessageBox.Show(this, "The tasks required to use this feature could not be created", "Unable to create tasks", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
            }
            else if (wasEnabled)
            {
                try
                {
                    Util.ProcessUtil.DeleteTask(new string[] { "gw2launcher-users-active-yes", "gw2launcher-users-active-no" });
                }
                catch (Exception ex)
                {
                    Util.Logging.Log(ex);
                    MessageBox.Show(this, "The tasks used by this feature could not be deleted.\n\nTo delete them manually, open the Windows Task Scheduler and look for tasks with the name gw2launcher.", "Unable to delete tasks", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            else
            {
                this.DialogResult = DialogResult.OK;
                return;
            }

            foreach (DataGridViewRow row in gridUsers.Rows)
            {
                string username = (string)row.Cells[columnUser.Index].Value;
                bool enabled = (bool)row.Cells[columnHidden.Index].Value;

                if (enabled)
                    Settings.HiddenUserAccounts[username].Value = true;
                else if (Settings.HiddenUserAccounts.Contains(username))
                    Settings.HiddenUserAccounts[username].Clear();
            }

            this.DialogResult = DialogResult.OK;
        }

        private void gridUsers_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.ColumnIndex == columnUser.Index)
            {
                var cell = gridUsers.Rows[e.RowIndex].Cells[columnHidden.Index];
                cell.Value = !(bool)cell.Value;
            }
        }
    }
}
