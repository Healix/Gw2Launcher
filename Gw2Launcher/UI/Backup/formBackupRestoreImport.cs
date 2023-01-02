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

namespace Gw2Launcher.UI.Backup
{
    public partial class formBackupRestoreImport : Base.BaseForm
    {
        private string path;
        private Tools.Backup.Backup.Importer backup;
        private int pid;

        public formBackupRestoreImport(string path, int pid = 0)
        {
            this.path = path;
            this.pid = pid;

            backup = new Tools.Backup.Backup.Importer(path);
            backup.FileError += backup_FileError;
            backup.ProgressChanged += backup_ProgressChanged;
            backup.Importing += backup_Importing;

            InitializeComponents();
        }

        protected override void OnInitializeComponents()
        {
            InitializeComponent();
        }

        protected override void OnShown(EventArgs e)
        {
            base.OnShown(e);

            Restore();
        }

        void backup_Importing(object sender, Tools.Backup.Backup.IFileInformation e)
        {
            Util.Invoke.Required(this,
                delegate
                {
                    labelProgress.Text = e.Path;
                });
        }

        void backup_FileError(object sender, Tools.Backup.Backup.FileErrorEventArgs e)
        {
            Util.Invoke.Required(this,
                delegate
                {
                    var r = MessageBox.Show(this, e.Exception.Message + "\n\n\"" + e.Path + "\"", "Error", MessageBoxButtons.AbortRetryIgnore, MessageBoxIcon.Error, MessageBoxDefaultButton.Button2);
                    e.Retry = r == System.Windows.Forms.DialogResult.Retry;
                    e.Abort = r == System.Windows.Forms.DialogResult.Abort;
                });
        }

        void backup_ProgressChanged(object sender, EventArgs e)
        {
            progressRestore.Value = (int)(progressRestore.Maximum * backup.Progress);
        }

        private async void Restore()
        {
            try
            {
                var ri = await Task.Run(new Func<Tools.Backup.Backup.RestoreInformation>(
                    delegate
                    {
                        if (pid != 0)
                        {
                            try
                            {
                                using (var p = Process.GetProcessById(pid))
                                {
                                    p.WaitForExit();
                                }
                            }
                            catch { }
                        }
                        return backup.Import();
                    }));

                if (ri != null)
                {
                    //restart
                    Util.ProcessUtil.Execute("", false, false);
                }
            }
            catch (Exception e)
            {
                MessageBox.Show(this, e.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

            this.Close();
        }
    }
}
