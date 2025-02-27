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
    public partial class formBrowseGwDat : Base.StackFormBase
    {
        public class SelectedFile
        {
            [Flags]
            public enum FileFlags
            {
                None = 0,
                /// <summary>
                /// Path was created
                /// </summary>
                Created = 1,
            }

            private FileFlags cancel;
            
            public SelectedFile(Settings.IGwDatFile dat)
            {
                this.File = dat;
            }

            public SelectedFile(string path, FileFlags flags)
            {
                this.Path = path;
                this.Flags = flags;
            }

            public string Path
            {
                get;
                private set;
            }

            public FileFlags Flags
            {
                get;
                set;
            }

            public Settings.IGwDatFile File
            {
                get;
                private set;
            }

            public void Cancel()
            {
                if (this.Flags.HasFlag(FileFlags.Created))
                {
                    try
                    {
                        Util.FileUtil.DeleteDirectory(System.IO.Path.GetDirectoryName(this.Path));
                    }
                    catch(Exception e)
                    {
                        Util.Logging.Log(e);
                    }

                    this.Flags = FileFlags.None;
                }
            }
        }

        private Settings.IGwDatFile existing;

        public formBrowseGwDat()
            : this(null, null)
        {
        }

        public formBrowseGwDat(string path, Settings.IGwDatFile existing)
        {
            InitializeComponents();

            Util.CheckedButton.Group(radioFileAutoCopy, radioFileExisting);

            this.existing = existing;

            if (path == null && existing != null)
                path = existing.Path;

            if (path != null)
            {
                radioFileExisting.Checked = true;
                textExisting.Text = path;
                textExisting.Tag = path;
                textExisting.Select(textExisting.TextLength, 0);
            }
            else
            {
                radioFileAutoCopy.Checked = true;
                radioFileAutoCopy.Select();
            }
        }

        protected override void OnInitializeComponents()
        {
            base.OnInitializeComponents();

            InitializeComponent();
        }

        private bool CreateLink(string name, string destFolder, string sourceFolder, bool isFolder)
        {
            if (isFolder)
            {
                var source = Path.Combine(sourceFolder, name);
                if (!Directory.Exists(source))
                    return false;
                var dest = Path.Combine(destFolder, name);
                if (Directory.Exists(dest))
                    Directory.Delete(dest);
                Windows.Symlink.CreateJunction(dest, source);
            }
            else
            {
                var source = Path.Combine(sourceFolder, name);
                if (!File.Exists(source))
                    return false;
                var dest = Path.Combine(destFolder, name);
                if (File.Exists(dest))
                    File.Delete(dest);
                Windows.Symlink.CreateHardLink(dest, source);
            }

            return true;
        }

        public SelectedFile Result
        {
            get;
            private set;
        }

        private void buttonOK_Click(object sender, EventArgs e)
        {
            if (radioFileAutoCopy.Checked)
            {
                if (!Settings.GuildWars1.Path.HasValue || !File.Exists(Settings.GuildWars1.Path.Value))
                {
                    using (var f = new OpenFileDialog())
                    {
                        f.Filter = "Guild Wars 1|Gw.exe|All executables|*.exe";
                        f.Title = "Open Gw.exe";

                        if (f.ShowDialog(this) != System.Windows.Forms.DialogResult.OK)
                            return;

                        Settings.GuildWars1.Path.Value = f.FileName;
                    }
                }

                var gwpath = Path.GetDirectoryName(Settings.GuildWars1.Path.Value);
                var exe = Path.GetFileName(Settings.GuildWars1.Path.Value);
                var path = Path.Combine(gwpath, Client.FileManager.LOCALIZED_EXE_FOLDER_NAME);

                if (!Directory.Exists(path))
                {
                    try
                    {
                        Directory.CreateDirectory(path);
                        if (!Util.FileUtil.AllowFolderAccess(path, System.Security.AccessControl.FileSystemRights.Modify))
                            throw new Exception("Unable to set folder permissions");
                    }
                    catch (Exception ex)
                    {
                        if (!Util.ProcessUtil.CreateFolder(path))
                        {
                            MessageBox.Show(this, "Unable to create folder\n\n" + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            return;
                        }
                    }
                }

                var success = false;
                var created = false;

                try
                {
                    path = Util.FileUtil.GetTemporaryFolderName(path);
                    if (path == null)
                        throw new Exception("Unable to create temporary folder");
                    Directory.CreateDirectory(path);

                    created = true;

                    //bool canLink;

                    //try
                    //{
                    //    CreateLink(exe, path, gwpath, false);
                    //    canLink = true;
                    //}
                    //catch
                    //{
                    //    canLink = false;
                    //}

                    //var files = new List<formCopyFileDialog.FilePath>();

                    //if (canLink)
                    //{
                    //    CreateLink("GwLoginClient.dll", path, gwpath, false);
                    //    if (checkLinkScreenshots.Checked)
                    //        CreateLink("Screens", path, gwpath, true);
                    //    if (checkLinkTemplates.Checked)
                    //        CreateLink("Templates", path, gwpath, true);
                    //}
                    //else
                    //{
                    //    files.Add(new formCopyFileDialog.FilePath(Path.Combine(gwpath, exe), Path.Combine(path, exe)));
                    //    files.Add(new formCopyFileDialog.FilePath(Path.Combine(gwpath, "GwLoginClient.dll"), Path.Combine(path, "GwLoginClient.dll")));
                    //}

                    var datSource = Path.Combine(gwpath, "Gw.dat");
                    var dat = Path.Combine(path, "Gw.dat");

                    if (!File.Exists(datSource))
                    {
                        MessageBox.Show(this, "Unable to location Gw.dat", "Gw.dat not found", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }

                    var files = new formCopyFileDialog.FilePath[]
                        {
                            new formCopyFileDialog.FilePath(datSource, dat)
                        };

                    using (var f = new formCopyFileDialog(files))
                    {
                        var r = f.ShowDialog(this);

                        if (r == System.Windows.Forms.DialogResult.OK)
                        {
                            success = true;
                            this.Result = new SelectedFile(dat, SelectedFile.FileFlags.Created);
                            this.DialogResult = System.Windows.Forms.DialogResult.OK;
                            return;
                        }
                        else if (r == System.Windows.Forms.DialogResult.Cancel && f.CancelReason != null)
                        {
                            if (f.CancelReason is System.ComponentModel.Win32Exception)
                            {
                                switch (((System.ComponentModel.Win32Exception)f.CancelReason).NativeErrorCode)
                                {
                                    case 32:

                                        throw new IOException("Gw.dat is currently being used and cannot be copied");
                                }
                            }

                            throw f.CancelReason;
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show(this, ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                finally
                {
                    if (!success)
                    {
                        if (created)
                        {
                            try
                            {
                                Util.FileUtil.DeleteDirectory(path);
                            }
                            catch { }
                        }
                    }
                }
            }
            else if (textExisting.Text.Equals((string)textExisting.Tag, StringComparison.OrdinalIgnoreCase))
            {
                this.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            }
            else
            {
                if (textExisting.TextLength > 0 && File.Exists(textExisting.Text))
                {
                    var path = Path.GetFullPath(textExisting.Text);
                    Settings.IGwDatFile dat = null;

                    foreach (var d in Settings.GwDatFiles.GetValues())
                    {
                        if (d.HasValue && d.Value.Path.Equals(path, StringComparison.OrdinalIgnoreCase))
                        {
                            dat = d.Value;
                            break;
                        }
                    }

                    if (dat != null)
                    {
                        if (dat == existing)
                        {
                            this.DialogResult = System.Windows.Forms.DialogResult.Cancel;
                            return;
                        }

                        if (dat.References == 1) //assuming if more than 1 account is already using it, the user doesn't care
                        {
                            if (MessageBox.Show(this, "Another account is already using this file.\n\nAre you sure?", "Are you sure?", MessageBoxButtons.YesNo, MessageBoxIcon.Exclamation) != System.Windows.Forms.DialogResult.Yes)
                                return;
                        }

                        this.Result = new SelectedFile(dat);
                    }
                    else
                    {
                        this.Result = new SelectedFile(path, SelectedFile.FileFlags.None);
                    }

                    this.DialogResult = System.Windows.Forms.DialogResult.OK;
                }
                else
                {
                    MessageBox.Show(this, "The selected path does not exist", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void buttonCancel_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
        }

        private void buttonExisting_Click(object sender, EventArgs e)
        {
            using (var f = new OpenFileDialog())
            {
                f.Filter = "Gw.dat|Gw.dat|All .dat files|*.dat";
                f.ValidateNames = false;

                if (textExisting.TextLength != 0)
                {
                    f.InitialDirectory = Path.GetDirectoryName(textExisting.Text);
                    f.FileName = textExisting.Text;
                }
                else if (Settings.GuildWars1.Path.HasValue)
                {
                    f.InitialDirectory = Settings.GuildWars1.Path.Value;
                }

                if (f.ShowDialog(this) == System.Windows.Forms.DialogResult.OK)
                {
                    radioFileExisting.Checked = true;
                    textExisting.Text = f.FileName;
                }
            }
        }
    }
}
