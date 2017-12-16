using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Net;
using System.IO;
using System.IO.Compression;
using System.Reflection;
using System.Diagnostics;

namespace Gw2Launcher.UI
{
    public partial class formVersionUpdate : Form
    {
        private const byte PAK_VERSION = 1;
        private const byte KEY_LENGTH = 16;

        public formVersionUpdate()
        {
            InitializeComponent();

            this.Shown += formVersionUpdate_Shown;
        }

        private string GetTempName(string path)
        {
            string n;
            int retry = 0;

            do
            {
                n = path + (retry > 0 ? string.Format(".{0:000}", retry) : ".tmp");
                if (File.Exists(n))
                {
                    try
                    {
                        File.Delete(n);
                        break;
                    }
                    catch (Exception e)
                    {
                        Util.Logging.Log(e);

                        if (++retry == 10)
                        {
                            n = null;
                            break;
                        }
                    }
                }
                else
                    break;
            }
            while (true);

            return n;
        }

        private void ShowError(string message)
        {
            labelStatus.Text = "Error";

            MessageBox.Show(this, message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);

            this.Close();
        }

        private async Task Decompress(byte[] data, string to)
        {
            if (data[0] != PAK_VERSION)
                throw new Exception("Unknown version");

            var keyStart = 1;
            var dataStart = keyStart + KEY_LENGTH + 8;

            var sum = BitConverter.ToInt32(data, keyStart + KEY_LENGTH);
            var length = BitConverter.ToInt32(data, keyStart + KEY_LENGTH + 4);

            for (var i = data.Length - 1; i >= dataStart; i--)
            {
                data[i] -= data[keyStart + (i - dataStart) % KEY_LENGTH];
                sum -= data[i];
            }

            if (sum != 0)
                throw new Exception("Checksum failed");

            using (var stream = new MemoryStream(data, false))
            {
                stream.Position = dataStart;
                using (var compressor = new DeflateStream(stream, CompressionMode.Decompress))
                {
                    using (var fs = File.Create(to))
                    {
                        await compressor.CopyToAsync(fs);

                        if (fs.Length != length)
                            throw new Exception("Unexpected decompressed file size");
                    }
                }
            }
        }

        private async void DoUpdate()
        {
            var client = new WebClient();
            int progress = 0, _progress = 0;
            bool dataReceived = false;

            labelStatus.Text = "...";

            client.DownloadProgressChanged += delegate(object o, DownloadProgressChangedEventArgs e)
            {
                progress = e.ProgressPercentage;
                if (!dataReceived)
                {
                    dataReceived = true;
                    string message = "Downloading";
                    if (e.TotalBytesToReceive > 0)
                        message += " " + Util.Text.FormatBytes(e.TotalBytesToReceive);

                    labelStatus.Text = message;
                }
            };

            var v64 = Environment.Is64BitProcess;

            if (!v64 && Environment.Is64BitOperatingSystem)
            {
                if (MessageBox.Show(this, "Upgrade to the 64-bit version?", "64-bit?", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == System.Windows.Forms.DialogResult.Yes)
                    v64 = true;
            }

            var current = Assembly.GetExecutingAssembly().Location;
            var path = Path.GetDirectoryName(current);
            var temp = GetTempName(Path.Combine(path, "update"));
            var url = "https://raw.githubusercontent.com/Healix/Gw2Launcher/master/Gw2Launcher/update/core-" + (v64 ? "64" : "32") + ".pak";

            if (temp == null)
            {
                ShowError("Unable to start. Existing files could not be deleted.");
                return;
            }

            var t = client.DownloadDataTaskAsync(url);

            while (!t.IsCompleted)
            {
                await Task.Delay(500);

                if (_progress != progress)
                {
                    if ((_progress = progress) > 0)
                        progressUpdating.Value = _progress;
                }
            }

            client.Dispose();
            t.Dispose();

            progressUpdating.Value = progressUpdating.Maximum;

            await Task.Delay(500);

            if (t.Status == TaskStatus.RanToCompletion)
            {
                labelStatus.Text = "Decompressing";

                await Task.Delay(500);

                try
                {
                    await Decompress(t.Result, temp);
                }
                catch (Exception e)
                {
                    try
                    {
                        File.Delete(temp);
                    }
                    catch { }

                    ShowError("Update failed. The file could not be decompressed.\n\n" + e.Message);
                    return;
                }

                string n = GetTempName(current);

                if (n != null)
                {
                    byte state = 0;
                    try
                    {
                        File.Move(current, n);
                        state++;
                        File.Move(temp, current);
                        state++;
                    }
                    catch
                    {

                    }

                    switch (state)
                    {
                        case 0: //failed to move self
                            break;
                        case 1: //failed to move update
                            try
                            {
                                File.Move(n, current);
                            }
                            catch
                            {
                            }

                            break;
                        case 2: //success

                            labelStatus.Text = "Restarting";

                            await Task.Delay(1000);

                            try
                            {
                                using (Process p = new Process())
                                {
                                    int pid;
                                    try
                                    {
                                        pid = Process.GetCurrentProcess().Id;
                                    }
                                    catch
                                    {
                                        pid = 0;
                                    }

                                    p.StartInfo = new ProcessStartInfo()
                                    {
                                        FileName = current,
                                        Arguments = "-updated " + pid + " \"" + Path.GetFileName(n) + "\"",
                                        UseShellExecute = false
                                    };

                                    if (p.Start())
                                    {
                                        Application.Exit();
                                    }
                                    else
                                    {
                                        labelStatus.Text = "Complete";

                                        MessageBox.Show(this, "Update complete. A manual restart is required.", "Update ready", MessageBoxButtons.OK, MessageBoxIcon.Information);
                                        Application.Exit();
                                    }
                                }

                                return;
                            }
                            catch (Exception e)
                            {
                                Util.Logging.Log(e);

                                labelStatus.Text = "Error";

                                if (MessageBox.Show(this, "The process could not be started. Revert the update?\n\n" + e.Message, "Error", MessageBoxButtons.YesNo, MessageBoxIcon.Error) == System.Windows.Forms.DialogResult.Yes)
                                {
                                    try
                                    {
                                        File.Delete(current);
                                        File.Move(n, current);
                                    }
                                    catch (Exception ex)
                                    {
                                        Util.Logging.Log(ex);
                                    }
                                }

                                this.Close();
                                return;
                            }
                    }
                }

                if (File.Exists(temp))
                {
                    n = Path.Combine(Path.GetDirectoryName(temp), Path.GetFileName(current) + ".tmp");
                    try
                    {
                        if (File.Exists(n))
                            File.Delete(n);
                        File.Move(temp, n);
                        temp = n;
                    }
                    catch { }

                    if (Util.Explorer.OpenFolderAndSelect(temp))
                    {
                        await Task.Delay(1000);
                        Windows.FindWindow.SetForegroundWindow(this.Handle);
                    }

                    ShowError("Unable to complete the update. Manually finish by overwriting:\n\n" + Path.GetFileName(current) + " with " + Path.GetFileName(temp));
                    Application.Exit();
                }
                else
                {
                    ShowError("Unable to complete the update.");
                }
            }
            else
            {
                string message = "Unable to download the latest version.";
                if (t.Exception != null && t.Exception.InnerException != null)
                    message += "\n\n" + t.Exception.InnerException.Message;

                ShowError(message);
            }
        }

        void formVersionUpdate_Shown(object sender, EventArgs e)
        {
            this.Shown -= formVersionUpdate_Shown;

            DoUpdate();
        }

        private void formVersionUpdate_Load(object sender, EventArgs e)
        {

        }
    }
}
