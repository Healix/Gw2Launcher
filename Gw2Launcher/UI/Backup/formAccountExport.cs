using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;
using Gw2Launcher.Tools.Backup;
using System.Security;

namespace Gw2Launcher.UI.Backup
{
    public partial class formAccountExport : Base.StackFormBase
    {
        private AccountExporter exporter;
        private AccountExporter.EncodingType level;

        public formAccountExport()
        {
            exporter = new AccountExporter();
            exporter.CanWriteAuthorization = level = GetLevel();

            InitializeComponents();
        }

        protected override void OnInitializeComponents()
        {
            base.OnInitializeComponents();

            InitializeComponent();

            gridData.SuspendLayout();

            foreach (var f in exporter.Fields)
            {
                var row = (DataGridViewRow)gridData.RowTemplate.Clone();
                row.CreateCells(gridData);
                row.Cells[columnName.Index].Value = f;

                gridData.Rows.Add(row);

                if ((f.Flags & AccountExporter.FieldFlags.Default) != 0)
                    gridData.SelectRow(row, true);
            }

            gridData.ResumeLayout();
        }

        protected override void OnHandleCreated(EventArgs e)
        {
            base.OnHandleCreated(e);

            ResizeGrid();
        }

        private AccountExporter.EncodingType GetLevel()
        {
            if (Settings.Encryption.HasValue)
            {
                return GetLevel(Settings.Encryption.Value.Scope);
            }
            return AccountExporter.EncodingType.None;
        }

        private AccountExporter.EncodingType GetLevel(Settings.EncryptionScope scope)
        {
            switch (Settings.Encryption.Value.Scope)
            {
                case Settings.EncryptionScope.Unencrypted:
                    return AccountExporter.EncodingType.Text;
                case Settings.EncryptionScope.Portable:
                    return AccountExporter.EncodingType.Encoded;
            }
            return AccountExporter.EncodingType.None;
        }

        private void ResizeGrid()
        {
            var sz = gridData.GetPreferredSize(new Size(gridData.Width, int.MaxValue));

            if (sz.Height < gridData.Height)
            {
                this.Height -= gridData.Height - sz.Height;
            }
            else
            {
                var max = Screen.FromControl(this).WorkingArea.Height * 3 / 4;
                var h = this.Height + sz.Height - gridData.Height;
                if (h < max)
                    this.Height = h;
            }
        }

        private bool IsAuthorized()
        {
            var requested = checkTextPasswords.Checked ? AccountExporter.EncodingType.Text : AccountExporter.EncodingType.Encoded;

            if (level == AccountExporter.EncodingType.Text || level == requested)
                return true;

            HashSet<ushort> ignored = null;

            while (true)
            {
                Settings.IAccount account = null;
                var date = DateTime.MaxValue;
                var puid = ushort.MaxValue;
                bool b;

                foreach (var a in Util.Accounts.GetAccounts())
                {
                    if (!a.HasCredentials)
                        continue;

                    if (a.Password.UID < puid || a.Password.UID == puid && a.CreatedUtc < date)
                    {
                        if (ignored == null || !ignored.Contains(a.UID))
                        {
                            account = a;
                            puid = a.Password.UID;
                            date = a.CreatedUtc;
                        }
                    }
                }

                if (!(b = account == null))
                {
                    SecureString p;

                    try
                    {
                        p = account.Password.ToSecureString();
                    }
                    catch
                    {
                        if (ignored == null)
                            ignored = new HashSet<ushort>();
                        ignored.Add(account.UID);
                        continue;
                    }

                    using (var f = new formPassword(account.Email, p))
                    {
                        b = f.ShowDialog(this) == System.Windows.Forms.DialogResult.OK;
                        if (b)
                            level = AccountExporter.EncodingType.Text;
                    }
                }

                return b;
            }
        }

        private List<Settings.IAccount> GetAccounts()
        {
            if (checkSelect.Checked)
            {
                return (List<Settings.IAccount>)checkSelect.Tag;
            }
            else if (checkGw1.Checked && checkGw2.Checked)
            {
                return new List<Settings.IAccount>(Util.Accounts.GetAccounts());
            }
            else if (checkGw1.Checked)
            {
                return new List<Settings.IAccount>(Util.Accounts.GetAccounts((Settings.AccountType.GuildWars1)));
            }
            else if (checkGw2.Checked)
            {
                return new List<Settings.IAccount>(Util.Accounts.GetAccounts((Settings.AccountType.GuildWars2)));
            }
            else
            {
                return null;
            }
        }

        private bool VerifyAuthorization(IList<Settings.IAccount> accounts, IList<DataGridViewRow> selected)
        {
            var hasAuthenticated = false;

            foreach (var row in selected)
            {
                if ((((AccountExporter.FieldData)row.Cells[columnName.Index].Value).Flags & AccountExporter.FieldFlags.Authenticated) != 0)
                {
                    hasAuthenticated = true;
                    break;
                }
            }

            if (!hasAuthenticated)
                return true;

            var hasCredentials = false;

            foreach (var a in accounts)
            {
                if (a.HasCredentials)
                {
                    hasCredentials = true;
                    break;
                }
            }

            if (hasCredentials)
            {
                if (!IsAuthorized())
                {
                    if (MessageBox.Show(this, "Unable to export encrypted settings.\n\nDo you want to continue anyways?", "Error", MessageBoxButtons.YesNo, MessageBoxIcon.Error) != System.Windows.Forms.DialogResult.Yes)
                        return false;
                }
                else
                {
                    exporter.CanWriteAuthorization = checkTextPasswords.Checked ? AccountExporter.EncodingType.Text : AccountExporter.EncodingType.Encoded;
                }
            }

            return true;
        }

        private void buttonCancel_Click(object sender, EventArgs e)
        {
            this.DialogResult = System.Windows.Forms.DialogResult.Cancel;
        }

        private async void buttonSave_Click(object sender, EventArgs e)
        {
            var selected = gridData.GetSelected();
            var accounts = GetAccounts();

            if (selected.Count == 0 || accounts == null || accounts.Count == 0)
            {
                string m;
                if (selected.Count == 0)
                    m = "No data has been selected";
                else
                    m = "There are no accounts to export";
                MessageBox.Show(this, m, "Nothing to export", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                return;
            }
            if (!VerifyAuthorization(accounts, selected))
                return;

            var fields = new AccountExporter.FieldData[selected.Count];

            for (var i = selected.Count - 1; i >= 0; --i)
            {
                fields[i] = (AccountExporter.FieldData)selected[i].Cells[columnName.Index].Value;
            }

            using (var f = new SaveFileDialog())
            {
                f.Title = "Export accounts";
                f.Filter = "XML|*.xml|CSV|*.csv";

                if (f.ShowDialog() != System.Windows.Forms.DialogResult.OK)
                    return;

                try
                {
                    AccountExporter.FileType type;

                    switch (Path.GetExtension(f.FileName).ToLower())
                    {
                        case ".xml":
                            type = AccountExporter.FileType.Xml;
                            break;
                        case ".csv":
                            type = AccountExporter.FileType.Csv;
                            break;
                        default:
                            if (f.FilterIndex <= 1)
                                type = AccountExporter.FileType.Xml;
                            else
                                type = AccountExporter.FileType.Csv;
                            break;
                    }

                    this.Enabled = false;
                    buttonSave.Enabled = false;

                    try
                    {
                        await Task.Run(delegate
                        {
                            exporter.Export(f.FileName, type, fields, accounts);
                        });
                    }
                    finally
                    {
                        buttonSave.Enabled = true;
                        this.Enabled = true;
                    }

                    MessageBox.Show(this, accounts.Count + (accounts.Count == 1 ? " account" : " accounts") + " exported successfully", "Complete", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show(this, ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void OnAccountTypeChanged()
        {
            gridData.SuspendLayout();

            foreach (DataGridViewRow row in gridData.Rows)
            {
                var f = (AccountExporter.FieldData)row.Cells[columnName.Index].Value;
                row.Visible = f.Type == AccountExporter.AccountType.Any || f.Type == AccountExporter.AccountType.Gw1 && checkGw1.Checked || f.Type == AccountExporter.AccountType.Gw2 && checkGw2.Checked;
            }

            gridData.ResumeLayout();
        }

        private void checkAccountType_CheckedChanged(object sender, EventArgs e)
        {
            if (((CheckBox)sender).Enabled)
                OnAccountTypeChanged();
        }

        private void checkSelect_CheckedChanged(object sender, EventArgs e)
        {
            if (checkSelect.Checked)
            {
                HashSet<Settings.IAccount> selected = null;
                Func<Settings.IAccount, bool> isSelected = null;

                if (checkSelect.Tag != null)
                {
                    selected = new HashSet<Settings.IAccount>((List<Settings.IAccount>)checkSelect.Tag);
                    isSelected = new Func<Settings.IAccount, bool>(
                        delegate(Settings.IAccount a)
                        {
                            return selected.Contains(a);
                        });
                }

                using (var f = new formAccountSelect("Select which accounts to export", Util.Accounts.GetAccounts(), false, true, null, isSelected))
                {
                    if (f.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                    {
                        checkSelect.Tag = f.Selected;
                        checkSelect.Text = f.Selected.Count + (f.Selected.Count == 1 ? " account" : " accounts");

                        var hasType = new bool[2];

                        foreach (var a in f.Selected)
                        {
                            var i = (int)a.Type;
                            if (!hasType[i])
                            {
                                hasType[i] = true;
                                if (++i > 1)
                                    i = 0;
                                if (hasType[i])
                                    break;
                            }
                        }

                        checkGw1.Enabled = false;
                        checkGw2.Enabled = false;
                        checkGw1.Checked = hasType[(int)Settings.AccountType.GuildWars1];
                        checkGw2.Checked = hasType[(int)Settings.AccountType.GuildWars2];

                        OnAccountTypeChanged();
                    }
                    else
                    {
                        checkSelect.Checked = false;
                    }
                }
            }
            else
            {
                checkGw1.Enabled = true;
                checkGw2.Enabled = true;
            }
        }
    }
}
