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

namespace Gw2Launcher.UI
{
    public partial class formBrowseLocalDat : Form
    {
        public class SelectedFile
        {
            private string movedFrom;

            public SelectedFile(string path)
            {
                this.Path = path;
            }

            public SelectedFile(string path, string movedFrom) : this(path)
            {
                this.movedFrom = movedFrom;
            }

            public SelectedFile(Settings.IDatFile dat)
            {
                this.DatFile = dat;
            }

            public string Path
            {
                get;
                private set;
            }

            public Settings.IDatFile DatFile
            {
                get;
                private set;
            }

            public void Cancel()
            {
                if (movedFrom != null)
                {
                    try
                    {
                        File.Move(this.Path, this.movedFrom);
                    }
                    catch (Exception ex)
                    {
                        Util.Logging.Log(ex);
                    }
                    movedFrom = null;
                }
                else if (this.Path != null)
                {
                    try
                    {
                        File.Delete(this.Path);
                    }
                    catch (Exception ex)
                    {
                        Util.Logging.Log(ex);
                    }
                }
            }
        }

        public formBrowseLocalDat(Settings.IAccount account)
        {
            InitializeComponent();

            int _space = buttonOK.Location.X - buttonCancel.Bounds.Right;
            buttonCancel.Location = new Point(this.ClientSize.Width / 2 - (buttonOK.Bounds.Right - buttonCancel.Bounds.Left) / 2, buttonCancel.Location.Y);
            buttonOK.Location = new Point(buttonCancel.Bounds.Right + _space, buttonCancel.Location.Y);

            var shared = new Dictionary<string, KeyValuePair<int, List<DataGridViewRow>>>();

            foreach (var uid in Settings.Accounts.GetKeys())
            {
                if (account != null && uid == account.UID)
                    continue;

                var _account = Settings.Accounts[uid];
                if (_account.HasValue)
                {
                    var dat = _account.Value.DatFile;

                    if (dat != null && !string.IsNullOrEmpty(dat.Path))
                    {
                        var row = gridAccounts.Rows[gridAccounts.Rows.Add()];
                        row.Tag = _account.Value;

                        string user = _account.Value.WindowsAccount;
                        if (string.IsNullOrEmpty(user))
                            user = "(current user)";
                        row.Cells[columnName.Index].Value = _account.Value.Name;
                        row.Cells[columnUser.Index].Value = user;

                        KeyValuePair<int, List<DataGridViewRow>> sharedWith;
                        if (shared.TryGetValue(dat.Path, out sharedWith))
                        {
                            row.Cells[columnShared.Index].Value = sharedWith.Key;
                            row.Cells[columnShared.Index].Tag = sharedWith.Value;

                            if (sharedWith.Value.Count == 1)
                            {
                                sharedWith.Value[0].Cells[columnShared.Index].Value = sharedWith.Key;
                                sharedWith.Value[0].Cells[columnShared.Index].Tag = sharedWith.Value;
                            }
                        }
                        else
                        {
                            shared[dat.Path] = sharedWith = new KeyValuePair<int, List<DataGridViewRow>>(shared.Count + 1, new List<DataGridViewRow>());
                        }

                        sharedWith.Value.Add(row);
                    }
                }
            }

            if (gridAccounts.Rows.Count == 0)
            {
                gridAccounts.SelectionChanged += delegate
                {
                    gridAccounts.ClearSelection();
                };

                gridAccounts.Enabled = false;
                var row = gridAccounts.Rows[gridAccounts.Rows.Add()];
                row.Cells[columnName.Index].Value = "None available";
                row.DefaultCellStyle.ForeColor = Color.Gray;
                gridAccounts.ClearSelection();

                radioAccountCopy.Enabled = false;
                radioAccountShare.Enabled = false;
                radioFileCopy.Checked = true;
            }
        }

        private void radio_CheckedChanged(object sender, EventArgs e)
        {
            if (((RadioButton)sender).Checked)
            {
                radioAccountCopy.Checked = sender == radioAccountCopy;
                radioAccountShare.Checked = sender == radioAccountShare;
                radioFileCopy.Checked = sender == radioFileCopy;
                radioFileMove.Checked = sender == radioFileMove;
                radioCreateNew.Checked = sender == radioCreateNew;
            }
        }

        private void formBrowseLocalDat_Load(object sender, EventArgs e)
        {

        }

        private void gridAccounts_CellMouseEnter(object sender, DataGridViewCellEventArgs e)
        {
            if (e.ColumnIndex == columnShared.Index)
            {
                var cell = gridAccounts.Rows[e.RowIndex].Cells[e.ColumnIndex];
                if (cell.Tag != null && cell.ToolTipText.Length==0)
                {
                    StringBuilder b = new StringBuilder();
                    var sharedWith = (List<DataGridViewRow>)cell.Tag;

                    b.Append("Shared with ");

                    foreach (var row in sharedWith)
                    {
                        if (row.Index != e.RowIndex)
                        {
                            b.Append(((Settings.IAccount)row.Tag).Name);
                            b.Append(", ");
                        }
                    }

                    b.Length -= 2;

                    cell.ToolTipText = b.ToString();
                }
            }
        }

        private void buttonBrowse_Click(object sender, EventArgs e)
        {
            using (OpenFileDialog f = new OpenFileDialog())
            {
                f.Filter = "Local.dat|Local.dat|All .dat files|*.dat";
                if (textBrowse.TextLength != 0)
                {
                    f.InitialDirectory = Path.GetDirectoryName(textBrowse.Text);
                    f.FileName = textBrowse.Text;
                }
                else
                {
                    f.InitialDirectory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Guild Wars 2");
                }

                if (f.ShowDialog(this) == DialogResult.OK)
                {
                    if (radioFileCopy.Checked == radioFileMove.Checked)
                        radioFileCopy.Checked = true;
                    textBrowse.Text = Path.GetFullPath(f.FileName);
                }
            }
        }

        private void buttonOK_Click(object sender, EventArgs e)
        {
            string path;

            if (radioAccountShare.Checked)
            {
                this.Result = new SelectedFile(((Settings.IAccount)gridAccounts.SelectedRows[0].Tag).DatFile);
                this.DialogResult = DialogResult.OK;
                return;
            }
            else if (radioAccountCopy.Checked)
            {
                path = ((Settings.IAccount)gridAccounts.SelectedRows[0].Tag).DatFile.Path;
            }
            else if (radioCreateNew.Checked)
            {
                this.Result = new SelectedFile(null, null);
                this.DialogResult = DialogResult.OK;
                return;
            }
            else
            {
                path = textBrowse.Text;
            }

            if (!File.Exists(path))
            {
                MessageBox.Show(this, "The selected file does not exist", "File not found", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            string temp;

            try
            {
                temp = Path.GetTempFileName();
            }
            catch (Exception ex)
            {
                Util.Logging.Log(ex);
                temp = Util.FileUtil.GetTemporaryFileName(Path.GetDirectoryName(path));
            }

            if (radioFileMove.Checked)
            {
                try
                {
                    try
                    {
                        File.Delete(temp);
                    }
                    catch (Exception ex)
                    {
                        Util.Logging.Log(ex);
                    }
                    File.Move(path, temp);

                    this.Result = new SelectedFile(temp, path);
                }
                catch (Exception ex)
                {
                    Util.Logging.Log(ex);
                    MessageBox.Show(this, "An error occured while trying to move the file.\n\n" + ex.Message, "Failed to move file", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
            }
            else
            {
                try
                {
                    File.Copy(path, temp, true);

                    this.Result = new SelectedFile(temp);
                }
                catch (Exception ex)
                {
                    Util.Logging.Log(ex);
                    try
                    {
                        File.Delete(temp);
                    }
                    catch (Exception ex2)
                    {
                        Util.Logging.Log(ex2);
                    }
                    MessageBox.Show(this, "An error occured while trying to copy the file.\n\n" + ex.Message, "Failed to copy file", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
            }

            Util.FileUtil.AllowFileAccess(temp, System.Security.AccessControl.FileSystemRights.Modify);

            this.DialogResult = DialogResult.OK;
        }

        public SelectedFile Result
        {
            get;
            private set;
        }

        private void buttonCancel_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
        }
    }
}
