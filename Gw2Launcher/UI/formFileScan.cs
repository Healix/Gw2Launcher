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
using System.Threading;

namespace Gw2Launcher.UI
{
    public partial class formFileScan : Base.BaseForm
    {
        private FileInfo file;
        private CancellationTokenSource cancelToken;

        public formFileScan(FileInfo fi)
        {
            InitializeComponents();

            labelStatus.Tag = labelStatus.Height;

            labelInfo.Text = string.Format(labelInfo.Text, fi.Name);
            labelStatus.Text = "---";
            labelSpeed.Text = "---";

            labelStatus.MaximumSize = new Size(buttonCancel.Location.X - 5 - labelStatus.Location.X, 0);
            labelStatus.SizeChanged += labelStatus_SizeChanged;

            this.file = fi;

            this.Shown += formFileScan_Shown;
        }

        protected override void OnInitializeComponents()
        {
            base.OnInitializeComponents();

            InitializeComponent();
        }

        void labelStatus_SizeChanged(object sender, EventArgs e)
        {
            var h = (int)labelStatus.Tag;
            var height = labelStatus.Height;

            if (h > 0 && height != h)
            {
                labelSpeed.Location = new Point(labelSpeed.Location.X, labelSpeed.Location.Y + height - h);
                this.Height += height - h;
            }
        }

        void formFileScan_Shown(object sender, EventArgs e)
        {
            this.Shown -= formFileScan_Shown;

            cancelToken = new CancellationTokenSource();
            ScanFile(cancelToken.Token);
        }

        private async void ReadWarning(CancellationToken cancel)
        {
            await Task.Delay(10000, cancel);

            labelInfo.Text = "Waiting for drive to respond";
            labelSpeed.Text = "Warning";
        }

        private async void ScanFile(CancellationToken cancel)
        {
            Stream stream;
            try
            {
                stream = File.Open(file.FullName, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
            }
            catch (Exception e)
            {
                Util.Logging.Log(e);
                labelStatus.Text = e.Message;
                labelSpeed.Text = "Failed";
                labelSpeed.ForeColor = Color.Maroon;
                return;
            }

            using (stream)
            {
                int length = 1024 * 1024 * 5;
                long fileLength = stream.Length;
                long totalRead = 0;
                long last = 0;
                byte[] buffer = new byte[length];
                DateTime startTime = DateTime.UtcNow;
                DateTime next = DateTime.UtcNow.AddMilliseconds(100);
                DateTime nextUpdate = DateTime.MinValue;

                Action warning = async delegate
                {
                    while (totalRead < fileLength)
                    {
                        if (DateTime.UtcNow.Subtract(next).TotalSeconds > 10)
                        {
                            labelStatus.Text = "Reading from disk is taking too long";
                            labelSpeed.Text = "Warning";
                        }

                        try
                        {
                            await Task.Delay(5000, cancel);
                        }
                        catch (TaskCanceledException)
                        {
                            return;
                        }
                    }
                };

                warning();

                while (totalRead < fileLength)
                {
                    try
                    {
                        int read = await stream.ReadAsync(buffer, 0, length, cancel);
                        totalRead += read;
                    }
                    catch (TaskCanceledException)
                    {
                        return;
                    }
                    catch (Exception e)
                    {
                        Util.Logging.Log(e);
                        labelStatus.Text = e.Message;
                        labelSpeed.Text = "Failed";
                        labelSpeed.ForeColor = Color.Maroon;
                        return;
                    }

                    var now = DateTime.UtcNow;
                    if (now > next || totalRead >= fileLength)
                    {
                        float elapsed = (float)((now.Subtract(next).TotalMilliseconds + 100) / 1000);
                        if (elapsed <= 0)
                            elapsed = 1;

                        float MBps = (float)((totalRead - last) / elapsed / 1048576);

                        if (now > nextUpdate)
                        {
                            labelStatus.Text = string.Format("{0} of {1}", Util.Text.FormatBytes(totalRead), Util.Text.FormatBytes(fileLength));
                            labelSpeed.Text = string.Format("{0:0.##} MB/s", MBps);

                            nextUpdate = now.AddSeconds(1);
                        }

                        progressGraph1.AddSample((float)((double)totalRead / fileLength), MBps);

                        last = totalRead;

                        next = DateTime.UtcNow.AddMilliseconds(100);
                    }
                }

                var ts = DateTime.UtcNow.Subtract(startTime);
                string duration;

                if (ts.TotalHours >= 1)
                    duration = (int)ts.TotalHours + "h " + ts.Minutes + "m";
                else if (ts.Minutes >= 1)
                    duration = ts.Minutes + "m " + ts.Seconds + "s";
                else
                    duration = ts.Seconds + "s";

                labelStatus.Text = string.Format("Completed in {0}", duration);
                labelSpeed.Text = "Complete";
            }
        }

        private void formFileScan_Load(object sender, EventArgs e)
        {

        }

        private void buttonCancel_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
        }

        private void formFileScan_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (cancelToken != null)
                cancelToken.Cancel();
        }
    }
}
