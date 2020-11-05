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
    public partial class formBrowseLocalDat : Base.StackFormBase
    {
        private Client.FileManager.FileType fileType;

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

            public SelectedFile(Settings.IFile file)
            {
                this.File = file;
            }

            public string Path
            {
                get;
                private set;
            }

            public Settings.IFile File
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
                        Util.FileUtil.MoveFile(this.Path, this.movedFrom, false, true);
                    }
                    catch (Exception ex)
                    {
                        Util.Logging.Log(ex);
                    }
                    movedFrom = null;
                    Path = null;
                }
                else if (this.Path != null)
                {
                    try
                    {
                        System.IO.File.Delete(this.Path);
                    }
                    catch (Exception ex)
                    {
                        Util.Logging.Log(ex);
                    }
                }
            }

            public void Update(string path)
            {
                this.Path = path;
            }
        }

        public formBrowseLocalDat(Client.FileManager.FileType type, Settings.IAccount account)
        {
            InitializeComponents();

            radioAccountShare.Enabled = Client.FileManager.IsDataLinkingSupported;

            Util.CheckedButton.Group(radioAccountCopy, radioAccountShare, radioCreateNew, radioFileCopy, radioFileMove);

            this.fileType = type;

            string fileName;
            if (type == Client.FileManager.FileType.Gfx)
                fileName = "GFXSettings.xml";
            else
                fileName = "Local.dat";

            label5.Text = string.Format(label5.Text, fileName);
            radioCreateNew.Text = string.Format(radioCreateNew.Text, fileName);

            int _space = buttonOK.Location.X - buttonCancel.Bounds.Right;
            buttonCancel.Location = new Point(this.ClientSize.Width / 2 - (buttonOK.Bounds.Right - buttonCancel.Bounds.Left) / 2, buttonCancel.Location.Y);
            buttonOK.Location = new Point(buttonCancel.Bounds.Right + _space, buttonCancel.Location.Y);

            var shared = new Dictionary<string, KeyValuePair<int, List<DataGridViewRow>>>();
            var hiddenRows = 0;

            foreach (var a in Util.Accounts.GetGw2Accounts())
            {
                Settings.IFile file;
                switch (type)
                {
                    case Client.FileManager.FileType.Dat:

                        file = a.DatFile;

                        break;
                    case Client.FileManager.FileType.Gfx:

                        file = a.GfxFile;

                        break;
                    default:

                        file = null;

                        break;
                }

                if (file != null && !string.IsNullOrEmpty(file.Path))
                {
                    var row = gridAccounts.Rows[gridAccounts.Rows.Add()];
                    row.Tag = a;

                    string user = a.WindowsAccount;
                    if (string.IsNullOrEmpty(user))
                        user = "(current user)";
                    row.Cells[columnName.Index].Value = a.Name;
                    row.Cells[columnUser.Index].Value = user;

                    KeyValuePair<int, List<DataGridViewRow>> sharedWith;
                    if (shared.TryGetValue(file.Path, out sharedWith))
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
                        shared[file.Path] = sharedWith = new KeyValuePair<int, List<DataGridViewRow>>(shared.Count + 1, new List<DataGridViewRow>());
                    }

                    if (account != null && a == account)
                    {
                        row.Visible = false;
                        hiddenRows++;
                    }

                    sharedWith.Value.Add(row);
                }
            }

            if (gridAccounts.Rows.Count - hiddenRows == 0)
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

        protected override void OnInitializeComponents()
        {
            base.OnInitializeComponents();

            InitializeComponent();

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
                switch (fileType)
                {
                    case Client.FileManager.FileType.Dat:

                        f.Filter = "Local.dat|Local.dat|All .dat files|*.dat";

                        break;
                    case Client.FileManager.FileType.Gfx:

                        f.Filter = "GFXSettings.xml|GFXSettings.*.xml|All .xml files|*.xml";

                        break;
                }

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
                var gw2 = ((Settings.IGw2Account)gridAccounts.SelectedRows[0].Tag);
                Settings.IFile file;

                switch (fileType)
                {
                    case Client.FileManager.FileType.Dat:
                        file = gw2.DatFile;
                        break;
                    case Client.FileManager.FileType.Gfx:
                        file = gw2.GfxFile;
                        break;
                    default:
                        return;
                }

                this.Result = new SelectedFile(file);
                this.DialogResult = DialogResult.OK;
                return;
            }
            else if (radioAccountCopy.Checked)
            {
                var gw2 = ((Settings.IGw2Account)gridAccounts.SelectedRows[0].Tag);
                switch (fileType)
                {
                    case Client.FileManager.FileType.Dat:
                        path = gw2.DatFile.Path;
                        break;
                    case Client.FileManager.FileType.Gfx:
                        path = gw2.GfxFile.Path;
                        break;
                    default:
                        return;
                }
            }
            else if (radioCreateNew.Checked)
            {
                try
                {
                    path = Path.GetTempFileName();
                }
                catch (Exception ex)
                {
                    Util.Logging.Log(ex);
                    path = Util.FileUtil.GetTemporaryFileName(DataPath.AppDataAccountDataTemp);
                    if (path == null)
                    {
                        MessageBox.Show(this, "Unable to create temporary file path", "Failed to create file", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return;
                    }
                }

                switch (fileType)
                {
                    case Client.FileManager.FileType.Dat:
                        break;
                    case Client.FileManager.FileType.Gfx:

                        var gfxs = Settings.GfxFiles.GetKeys();

                        for (var i = -1; i < gfxs.Length; i++)
                        {
                            string gfx;
                            if (i == -1)
                                gfx = Client.FileManager.GetDefaultPath(Client.FileManager.FileType.Gfx);
                            else
                            {
                                var _gfx = Settings.GfxFiles[gfxs[i]];
                                if (_gfx.HasValue && !string.IsNullOrEmpty(_gfx.Value.Path))
                                    gfx = _gfx.Value.Path;
                                else
                                    continue;
                            }

                            if (File.Exists(gfx))
                            {
                                try
                                {
                                    File.Copy(gfx, path, true);
                                    break;
                                }
                                catch (Exception ex)
                                {
                                    Util.Logging.Log(ex);
                                }
                            }
                        }

                        break;
                }


                this.Result = new SelectedFile(path);
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
                if (temp == null)
                {
                    MessageBox.Show(this, "Unable to create temporary file path", "Failed to create file", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
            }

            if (radioFileMove.Checked)
            {
                try
                {
                    try
                    {
                        if (File.Exists(temp))
                            File.Delete(temp);
                    }
                    catch (Exception ex)
                    {
                        Util.Logging.Log(ex);
                    }
                    Util.FileUtil.MoveFile(path, temp, false, true);

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
