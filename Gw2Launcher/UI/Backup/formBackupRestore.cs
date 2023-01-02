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
using System.IO.MemoryMappedFiles;

namespace Gw2Launcher.UI.Backup
{
    public partial class formBackupRestore : Base.BaseForm
    {
        private class Account
        {
            public string Name
            {
                get;
                set;
            }

            public ushort UID
            {
                get;
                set;
            }

            public DateTime LastUsed
            {
                get;
                set;
            }

            public Settings.AccountType Type
            {
                get;
                set;
            }
        }

        private Tools.Backup.Backup.RestoreInformation ri;
        private Image[] defaultImage;

        public formBackupRestore(Tools.Backup.Backup.RestoreInformation ri)
        {
            InitializeComponents();

            this.ri = ri;

            labelName.Text = Path.GetFileName(ri.Path);
            try
            {
                labelCreated.Text = string.Format(labelCreated.Text, ri.Created);
            }
            catch
            {
                labelCreated.Text = ri.Created.ToString();
            }

            gridFiles.SuspendLayout();

            foreach (var fi in ri.Files)
            {
                var row = (DataGridViewRow)gridFiles.RowTemplate.Clone();
                row.CreateCells(gridFiles);
                row.Tag = fi;

                var cell = row.Cells[columnPath.Index];
                
                cell.Value = fi.Path;

                if (!fi.Input.Exists)
                {
                    cell.Style.ForeColor = Color.Gray;
                    cell.ToolTipText = "Unable to import; file not found";
                }
                else if (fi.Output.Exists)
                {
                    cell.Style.ForeColor = Color.Maroon;
                    cell.ToolTipText = "Existing file will be lost";
                }

                gridFiles.Rows.Add(row);
            }

            gridFiles.ResumeLayout();
        }

        protected override void OnInitializeComponents()
        {
            InitializeComponent();
        }

        private void gridFiles_SelectionChanged(object sender, EventArgs e)
        {
            gridFiles.ClearSelection();
        }

        private void buttonCancel_Click(object sender, EventArgs e)
        {
            this.DialogResult = System.Windows.Forms.DialogResult.Cancel;
        }

        private void buttonRestore_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show(this, "Are you sure you want to restore \"" + Path.GetFileName(ri.Path) + "\"?", "Are you sure?", MessageBoxButtons.YesNo, MessageBoxIcon.Exclamation) != System.Windows.Forms.DialogResult.Yes)
                return;

            while (Client.Launcher.GetActiveProcessCount(Client.Launcher.AccountType.GuildWars2) > 0)
            {
                if (MessageBox.Show(this, "Guild Wars 2 will be closed", "Close to continue", MessageBoxButtons.OKCancel, MessageBoxIcon.Exclamation) != System.Windows.Forms.DialogResult.OK)
                {
                    return;
                }

                Client.Launcher.CancelAndKillActiveLaunches(Client.Launcher.AccountType.GuildWars2);
                Client.Launcher.KillAllActiveProcesses(Client.Launcher.AccountType.GuildWars2);
            }

            if (ri.HasSettings)
            {
                try
                {
                    Program.Uninstall(true);
                }
                catch { }
            }

            try
            {
                Util.ProcessUtil.Restore(ri.Path);
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            Application.Exit();
        }

        private async void Terminate(int delay)
        {
            await Task.Delay(5000);

            using (var p = System.Diagnostics.Process.GetCurrentProcess())
            {
                p.Kill();
            }
        }

        private Image GetDefaultImage(Settings.AccountType type, Size sz)
        {
            var i = (int)type;

            if (defaultImage == null)
                defaultImage = new Image[2];

            if (defaultImage[i] == null)
                defaultImage[i] = Util.Bitmap.ResizeIcon(type == Settings.AccountType.GuildWars1 ? Properties.Resources.Gw1 : Properties.Resources.Gw2, new Size(48, 48), sz);

            return defaultImage[i];
        }

        private async void LoadAccounts()
        {
            var _accounts = await Task.Run<List<Account>>(new Func<List<Account>>(
                delegate
                {
                    try
                    {
                        if (!ri.HasSettings)
                            return null;
                        var size = ri.GetSize(ri.Settings);
                        if (size == 0)
                            return null;
                        var pid = System.Diagnostics.Process.GetCurrentProcess().Id;

                        using (var mf = MemoryMappedFile.CreateNew(Messaging.MappedMessage.BASE_ID + "Restore:" + pid, size))
                        {
                            if (Util.ProcessUtil.Execute("-restore \"" + ri.Path + "\" " + pid + " -preview", false, true))
                            {
                                using (var reader = new BinaryReader(mf.CreateViewStream()))
                                {
                                    if (reader.ReadInt32() != pid)
                                        return null;

                                    var accounts = new List<Account>();

                                    while (true)
                                    {
                                        var uid = reader.ReadUInt16();
                                        if (uid == 0)
                                            break;

                                        var a = new Account();
                                        a.UID = uid;
                                        a.Type = (Settings.AccountType)reader.ReadByte();
                                        a.Name = reader.ReadString();
                                        a.LastUsed = DateTime.FromBinary(reader.ReadInt64());

                                        accounts.Add(a);
                                    }

                                    return accounts;
                                }
                            }
                        }
                    }
                    catch (Exception e)
                    {
                        Util.Logging.Log(e);
                    }

                    return null;
                }));

            panelAccounts.SuspendLayout();
            panelAccountsWaiting.Visible = false;
            if (_accounts == null)
            {
                labelAccountsFailed.Visible = true;
            }
            else
            {
                var sz = Scale(16);

                foreach (var a in _accounts)
                {
                    var row = (DataGridViewRow)gridAccounts.RowTemplate.Clone();
                    row.CreateCells(gridAccounts);

                    string lastUsed;
                    if (a.LastUsed.Ticks <= 1)
                    {
                        lastUsed = "never";
                    }
                    else
                    {
                        var t = DateTime.UtcNow.Subtract(a.LastUsed);
                        var d = (int)t.TotalDays;

                        if (d > 1)
                        {
                            lastUsed = d + " days";
                        }
                        else
                        {
                            d = (int)(t.TotalHours);
                            if (d > 1)
                                lastUsed = d + " hours";
                            else
                                lastUsed = "today";
                        }

                        row.Cells[columnLastUsed.Index].ToolTipText = "Last used on " + a.LastUsed.ToLongDateString();
                    }

                    row.Cells[columnName.Index].Value = a.Name;
                    row.Cells[columnLastUsed.Index].Value = lastUsed;
                    row.Cells[columnIcon.Index].Value = GetDefaultImage(a.Type, new Size(sz, sz));

                    gridAccounts.Rows.Add(row);
                }

                gridAccounts.Visible = true;
            }
            panelAccounts.ResumeLayout();
        }

        private void labelShowAccounts_Click(object sender, EventArgs e)
        {
            if (!panelAccounts.Enabled)
            {
                panelAccounts.Enabled = true;
                LoadAccounts();
            }

            panelAccounts.SendToBack();
            panelAccounts.Visible = true;
            panelFiles.Visible = false;
        }

        private void labelAccountsBack_Click(object sender, EventArgs e)
        {
            panelFiles.SendToBack();
            panelFiles.Visible = true;
            panelAccounts.Visible = false;
        }

        private void gridAccounts_SelectionChanged(object sender, EventArgs e)
        {
            gridAccounts.ClearSelection();
        }

        private async void Extract(Tools.Backup.Backup.IFileInformation fi, string path)
        {
            var t = Task.Run(new Action(
                delegate
                {
                    ri.Extract(fi, path);
                }));

            try
            {
                await t;
            }
            catch (Exception e)
            {
                MessageBox.Show(this, e.Message, Path.GetFileName(fi.Path), MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void extractToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var row = (DataGridViewRow)contextFiles.Tag;
            var fi = (Tools.Backup.Backup.IFileInformation)row.Tag;

            using (var f = new SaveFileDialog())
            {
                f.FileName = Path.GetFileName(fi.Path);
                f.Filter = "|" + Path.GetExtension(fi.Path);

                if (f.ShowDialog(this)== System.Windows.Forms.DialogResult.OK)
                {
                    Extract(fi, f.FileName);
                }
            }
        }

        private void gridFiles_CellMouseDown(object sender, DataGridViewCellMouseEventArgs e)
        {
            if (e.RowIndex < 0)
                return;

            var row = gridFiles.Rows[e.RowIndex];
            var fi = (Tools.Backup.Backup.IFileInformation)row.Tag;

            extractToolStripMenuItem.Enabled = fi.Input.Exists;
            extractToolStripMenuItem.Text = "Extact " + Path.GetFileName(fi.Path);

            contextFiles.Tag = row;
            contextFiles.Show(Cursor.Position);
        }

        private async void extractAllToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var f = new Windows.Dialogs.SaveFolderDialog();

            if (f.ShowDialog(this) == System.Windows.Forms.DialogResult.OK)
            {
                var root = f.FileName;
                var count = gridFiles.Rows.Count;

                this.Enabled = false;

                using (var p = new formProgressBar()
                    {
                        Maximum = count,
                    })
                {
                    p.CenterAt(this);
                    p.Show(this);

                    foreach (DataGridViewRow row in gridFiles.Rows)
                    {
                        var fi = (Tools.Backup.Backup.IFileInformation)row.Tag;

                        var t = Task.Run(new Action(
                            delegate
                            {
                                var name = fi.Path;
                                if (Path.IsPathRooted(name))
                                    name = Path.GetFileName(name);
                                var path = Path.Combine(root, name);
                                var i = 0;
                                while (File.Exists(path))
                                {
                                    path = Path.Combine(Path.GetDirectoryName(path), ++i + "-" + Path.GetFileName(fi.Path));
                                }
                                Directory.CreateDirectory(Path.GetDirectoryName(path));
                                ri.Extract(fi, path);
                            }));

                        try
                        {
                            await t;
                        }
                        catch { }

                        if (p.IsDisposed)
                            break;
                        ++p.Value;
                    }
                }

                this.Enabled = true;
            }
        }
    }
}
