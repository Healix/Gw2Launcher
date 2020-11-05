using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.Windows.Forms;
using System.IO;
using System.Runtime.InteropServices;

namespace Gw2Launcher.UI
{
    public partial class formCopyFileDialog : Base.FlatBase
    {
        public class FilePath
        {
            public FilePath(string from, string to)
            {
                this.From = from;
                this.To = to;
            }

            public string From
            {
                get;
                private set;
            }

            public string To
            {
                get;
                private set;
            }
        }

        private FilePath[] files;
        private CancellationTokenSource cancel;
        private bool paused;

        public formCopyFileDialog(params FilePath[] files)
        {
            InitializeComponents();

            this.DoubleBuffered = true;
            this.files = files;
        }

        protected override void OnInitializeComponents()
        {
            base.OnInitializeComponents();

            InitializeComponent();
        }

        protected override bool OnEscapePressed()
        {
            paused = true;

            if (MessageBox.Show(this, "Are you sure you want to cancel?", "Cancel?", MessageBoxButtons.YesNo, MessageBoxIcon.Exclamation) == System.Windows.Forms.DialogResult.Yes)
            {
                this.DialogResult = System.Windows.Forms.DialogResult.Cancel;
                return true;
            }

            paused = false;

            return base.OnEscapePressed();
        }

        protected override void OnShown(EventArgs e)
        {
            base.OnShown(e);

            cancel=new CancellationTokenSource();
            CopyFilesAsync(cancel.Token);
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            base.OnFormClosing(e);

            using (cancel)
            {
                cancel.Cancel();
            }
        }

        public Exception CancelReason
        {
            get;
            private set;
        }

        private async void CopyFilesAsync(CancellationToken cancel)
        {
            long totalBytes;

            try
            {
                totalBytes = await Task.Run(new Func<long>(GetTotalBytes));
            }
            catch (Exception e)
            {
                Util.Logging.Log(e);
                MessageBox.Show(this, e.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                this.DialogResult = System.Windows.Forms.DialogResult.Cancel;
                return;
            }

            flatProgressBar1.Maximum = totalBytes;

            long total = 0;
            bool updated = false;

            var task = new Task(
                delegate
                {
                    long v = 0;

                    foreach (var f in files)
                    {
                        long length = 0;

                        var b = Gw2Launcher.Windows.FileCopy.Copy(f.From, f.To,
                            delegate(long t, long transferred)
                            {
                                if (length == 0)
                                    length = t;

                                total = v + transferred;
                                updated = true;

                                while (paused)
                                {
                                    if (cancel.WaitHandle.WaitOne(500))
                                        return false;
                                }
                                
                                return !cancel.IsCancellationRequested;
                            });

                        if (!b)
                            break;

                        v += length;
                    }
                }, TaskCreationOptions.LongRunning);

            task.Start();
            
            while (!task.IsCompleted)
            {
                try
                {
                    await Task.Delay(200, cancel);
                }
                catch
                {
                    break;
                }

                if (updated)
                {

                    flatProgressBar1.Value = total;
                    labelRemaining.Text = Util.Text.FormatBytes(totalBytes - total);

                    updated = false;
                }
            }

            if (task.IsFaulted)
            {
                var e = task.Exception.InnerException;
                if (e != null)
                {
                    this.CancelReason = e;
                }

                this.DialogResult = System.Windows.Forms.DialogResult.Cancel;
                return;
            }

            if (cancel.IsCancellationRequested)
                this.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            else
                this.DialogResult = System.Windows.Forms.DialogResult.OK;
        }

        private long GetTotalBytes()
        {
            long totalBytes = 0;

            foreach (var f in files)
            {
                totalBytes += new FileInfo(f.From).Length;
            }

            return totalBytes;
        }
    }
}
