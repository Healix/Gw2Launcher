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
using System.Diagnostics;

namespace Gw2Launcher.UI
{
    public partial class formGw2Cache : Form
    {
        private Dictionary<string, DataGridViewRow> rows;
        private List<DirectoryInfo> folders;
        private DataGridViewRow activeRow;

        public formGw2Cache()
        {
            InitializeComponent();

            labelSize.Text = "";
            buttonDelete.Enabled = false;

            checkDeleteCacheOnLaunch.Checked = Settings.DeleteCacheOnLaunch.Value;

            rows = new Dictionary<string, DataGridViewRow>(StringComparer.OrdinalIgnoreCase);

            this.Shown += formGw2Cache_Shown;
        }

        void formGw2Cache_Shown(object sender, EventArgs e)
        {
            this.Shown -= formGw2Cache_Shown;
            Scan();
        }

        private async void Scan()
        {
            this.folders = new List<DirectoryInfo>();

            foreach (ushort uid in Settings.Accounts.GetKeys())
            {
                var account = Settings.Accounts[uid];
                if (account.HasValue)
                {
                    string username = Util.Users.GetUserName(account.Value.WindowsAccount);
                    if (!rows.ContainsKey(username))
                    {
                        DataGridViewRow row = new DataGridViewRow();
                        row.CreateCells(gridCache);
                        row.Height = gridCache.RowTemplate.Height;

                        row.Cells[columnUser.Index].Value = username;
                        DataGridViewCell cell;
                        cell = row.Cells[columnSize.Index];
                        cell.Value = "...";
                        cell.Style.ForeColor = Color.Gray;
                        cell = row.Cells[columnFolders.Index];
                        cell.Value = "...";
                        cell.Style.ForeColor = Color.Gray;

                        gridCache.Rows.Add(row);

                        rows.Add(username, row);
                    }
                }
            }

            ulong totalSize = 0;
            int totalFolders = 0;

            foreach (string username in rows.Keys)
            {
                var o = await Task.Factory.StartNew<object[]>(new Func<object[]>(
                    delegate
                    {
                        try
                        {
                            var folders = Tools.Gw2Cache.GetFolders(username);
                            ulong size = 0;

                            foreach (var folder in folders)
                            {
                                this.folders.Add(folder);

                                try
                                {
                                    foreach (var file in folder.EnumerateFiles("*", SearchOption.AllDirectories))
                                    {
                                        try
                                        {
                                            size += (ulong)file.Length;
                                        }
                                        catch { }
                                    }
                                }
                                catch { }
                            }

                            return new object[] { folders.Length, size, folders };
                        }
                        catch
                        {
                            return new object[] { -1, (ulong)0, null };
                        }
                    }));

                var row = rows[username];
                var count = (int)o[0];
                DataGridViewCell cell;

                if (count > 0)
                {
                    totalSize += (ulong)o[1];
                    totalFolders += count;

                    cell = row.Cells[columnSize.Index];
                    cell.Value = FormatSize((ulong)o[1]);
                    cell.Style.ForeColor = gridCache.DefaultCellStyle.ForeColor;

                    cell = row.Cells[columnFolders.Index];
                    cell.Value = count + (count != 1 ? " folders" : " folder");
                    cell.Style.ForeColor = gridCache.DefaultCellStyle.ForeColor;
                    cell.Tag = o[2];
                }
                else if (count == -1)
                {
                    cell = row.Cells[columnSize.Index];
                    cell.Value = "failed";
                    cell.Style.ForeColor = Color.DarkRed;

                    cell = row.Cells[columnFolders.Index];
                    cell.Value = "---";
                    cell.Style.ForeColor = Color.DarkRed;
                }
                else
                {
                    cell = row.Cells[columnSize.Index];
                    cell.Value = "0 bytes";
                    cell.Style.ForeColor = Color.Gray;

                    cell = row.Cells[columnFolders.Index];
                    cell.Value = "no folders";
                    cell.Style.ForeColor = Color.Gray;
                }
            }

            labelSize.Text = FormatSize(totalSize) + " in " + totalFolders + (totalFolders != 1 ? " folders" : " folder");

            buttonDelete.Enabled = true;
        }

        private List<string> Delete()
        {
            List<string> failed = new List<string>();

            foreach (var folder in folders)
            {
                try
                {
                    foreach (var f in folder.EnumerateFiles("*", SearchOption.AllDirectories))
                    {
                        try
                        {
                            f.Delete();
                        }
                        catch 
                        {
                            try
                            {
                                f.Attributes = FileAttributes.Temporary;
                            }
                            catch { }
                        }
                    }

                    folder.Delete(true);
                }
                catch
                {
                    failed.Add(folder.FullName);
                }
            }

            return failed;
        }

        private string FormatSize(ulong size)
        {
            if (size > 858993459) //0.8 GB
            {
                return string.Format("{0:0.##} GB", size / 1073741824d);
            }
            else if (size > 838860) //0.8 MB
            {
                return string.Format("{0:0.##} MB", size / 1048576d);
            }
            else if (size > 819) //0.8 KB
            {
                return string.Format("{0:0.##} KB", size / 1024d);
            }
            else
            {
                return size + " bytes";
            }
        }

        private void formGw2Cache_Load(object sender, EventArgs e)
        {

        }

        private async void buttonDelete_Click(object sender, EventArgs e)
        {
            buttonDelete.Enabled = false;

            var failed = await Task.Factory.StartNew<List<string>>(new Func<List<string>>(
                delegate
                {
                    return Delete();
                }));

            if (failed.Count > 0)
            {
                HashSet<string> roots = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
                foreach (var f in failed)
                {
                    roots.Add(Path.GetDirectoryName(f));
                }
                try
                {
                    Util.ProcessUtil.DeleteCacheFolders(roots);
                }
                catch { }
            }

            Scan();
        }

        private void gridCache_SelectionChanged(object sender, EventArgs e)
        {
            gridCache.ClearSelection();
        }

        private void checkDeleteCacheOnLaunch_CheckedChanged(object sender, EventArgs e)
        {
            Settings.DeleteCacheOnLaunch.Value = checkDeleteCacheOnLaunch.Checked;
        }

        private void showInExplorerToolStripMenuItem_Click(object sender, EventArgs e)
        {
            DirectoryInfo[] folders=(DirectoryInfo[])activeRow.Cells[columnFolders.Index].Tag;
            string[] f = new string[folders.Length];

            for (int i = 0; i < folders.Length;i++)
            {
                f[i] = folders[i].FullName;
            }

            try
            {
                Util.ShowInExplorer.SelectFiles(folders[0].Parent.FullName, f);
            }
            catch { }
        }

        private void gridCache_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            
        }

        private void gridCache_CellMouseClick(object sender, DataGridViewCellMouseEventArgs e)
        {
            if (e.Button == System.Windows.Forms.MouseButtons.Right)
            {
                activeRow = gridCache.Rows[e.RowIndex];
                if (activeRow.Cells[columnFolders.Index].Tag != null)
                    contextMenu.Show(Cursor.Position);
            }
        }
    }
}
