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

namespace Gw2Launcher.UI
{
    public partial class formBackgroundPatcher : Base.BaseForm
    {
        private readonly string[] LANG = new string[] { "EN", "DE", "ES", "FR" };
        private const int SPEED_LIMIT_MIN_0 = 102400;
        private const int SPEED_LIMIT_MAX_0 = 1048576 - SPEED_LIMIT_MIN_0;
        private const int SPEED_LIMIT_MIN_1 = 1048576;
        private const int SPEED_LIMIT_MAX_1 = 10485760 - SPEED_LIMIT_MIN_1;
        private const int THREADS_MIN = 1;
        private const int THREADS_MAX = 20 - THREADS_MIN;

        private int filesTotal, filesDownloaded;
        private uint downloadRate;
        private long bytesDownloaded, contentBytesTotal, contentBytesCore;
        private long estimatedBytesRemaining;
        private bool errored;
        private bool manifestsComplete;
        private ToolTip tooltip;
        private Label[] lang;
        private Windows.Taskbar taskBar;
        private DateTime nextAutoUpdate;
        private CancellationTokenSource cancelTime;

        public formBackgroundPatcher()
        {
            InitializeComponents();

            taskBar = new Windows.Taskbar();

            lang = new Label[] { labelLang1, labelLang2, labelLang3, labelLang4 };

            foreach (var l in lang)
            {
                l.Click += langSelect_Click;
            }

            labelLang.Text = LANG[GetLang()];
            labelTime.Text = "";

            panelAdvanced.SetContent(panelAdvancedContent);

            var bp = Tools.BackgroundPatcher.Instance;
            bp.DownloadProgress += bp_DownloadProgress;
            bp.Error += bp_Error;
            bp.Complete += bp_Complete;
            bp.Starting += bp_Starting;
            bp.StateChanged += bp_StateChanged;

            if (Settings.BackgroundPatchingMaximumThreads.HasValue)
                sliderThreads.Value = (float)(Settings.BackgroundPatchingMaximumThreads.Value - THREADS_MIN) / THREADS_MAX;
            else
                sliderThreads.Value = (float)(10 - THREADS_MIN) / THREADS_MAX;

            PatchingSpeedLimit_ValueChanged(Settings.PatchingSpeedLimit, null);
            PatchingOptions_ValueChanged(Settings.PatchingOptions, null);

            Settings.BackgroundPatchingEnabled.ValueChanged += BackgroundPatchingEnabled_ValueChanged;
            Settings.PatchingSpeedLimit.ValueChanged += PatchingSpeedLimit_ValueChanged;
            Settings.PatchingOptions.ValueChanged += PatchingOptions_ValueChanged;

            UpdateStatus();

            this.Disposed += formBackgroundPatcher_Disposed;
        }

        protected override void OnInitializeComponents()
        {
            base.OnInitializeComponents();

            InitializeComponent();
        }

        void formBackgroundPatcher_Disposed(object sender, EventArgs e)
        {
            if (taskBar != null)
            {
                taskBar.Dispose();
                taskBar = null;
            }
        }

        protected override void OnShown(EventArgs e)
        {
            base.OnShown(e);

            AutoUpdate_NextCheckChanged(null, Tools.AutoUpdate.NextCheck);
            Tools.AutoUpdate.NextCheckChanged += AutoUpdate_NextCheckChanged;
        }

        void AutoUpdate_NextCheckChanged(object sender, DateTime e)
        {
            if (cancelTime != null)
            {
                using (cancelTime)
                {
                    cancelTime.Cancel();
                }
                cancelTime = null;
            }
            cancelTime = new CancellationTokenSource();

            if (e != DateTime.MinValue)
            {
                nextAutoUpdate = e;

                if (Util.Invoke.IfRequired(this, DoTimer))
                {
                    return;
                }

                DoTimer();
            }
            else
                UpdateTime(null);
        }

        void BackgroundPatchingEnabled_ValueChanged(object sender, EventArgs e)
        {
            //var v = ((Settings.ISettingValue<bool>)sender).Value;

            UpdateStatus();
        }

        void PatchingOptions_ValueChanged(object sender, EventArgs e)
        {
            if (Util.Invoke.IfRequired(this,
                delegate
                {
                    PatchingOptions_ValueChanged(sender, e);
                }))
            {
                return;
            }

            var v = ((Settings.ISettingValue<Settings.PatchingFlags>)sender).Value;

            checkUseHttps.Checked = v.HasFlag(Settings.PatchingFlags.UseHttps);
        }

        void PatchingSpeedLimit_ValueChanged(object sender, EventArgs e)
        {
            if (Util.Invoke.IfRequired(this,
                delegate
                {
                    PatchingSpeedLimit_ValueChanged(sender, e);
                }))
            {
                return;
            }

            var v = ((Settings.ISettingValue<int>)sender);
            if (v.HasValue)
            {
                checkSpeedLimit.Checked = true;
                if (v.Value >= SPEED_LIMIT_MIN_1)
                    sliderSpeedLimit.Value = (float)(v.Value - SPEED_LIMIT_MIN_1) / SPEED_LIMIT_MAX_1 * 0.5f + 0.5f;
                else
                    sliderSpeedLimit.Value = (float)(v.Value - SPEED_LIMIT_MIN_0) / SPEED_LIMIT_MAX_0 * 0.5f;
            }
            else
            {
                checkSpeedLimit.Checked = false;
                sliderSpeedLimit.Value = 1;
            }
        }

        private async void DoTimer()
        {
            var cancel = cancelTime.Token;

            while (true)
            {
                var ts = nextAutoUpdate.Subtract(DateTime.UtcNow);
                var m = (int)(ts.TotalMinutes);
                int t;

                if (m > 90)
                {
                    UpdateTime((int)(ts.TotalHours + 0.5) + "h");
                    if (m - 60 > 90)
                        t = (int)(ts.TotalMilliseconds + 0.5) % 3600000 + 1000;
                    else
                        t = (int)(ts.TotalMilliseconds + 0.5) - 5400000 + 1000;
                }
                else if (m > 0)
                {
                    UpdateTime((int)(ts.TotalMinutes + 0.5) + "m");
                    t = ((int)(ts.TotalMilliseconds + 0.5) % 60000) + 1000;
                }
                else
                {
                    var s = (int)ts.TotalSeconds;
                    if (s > 0)
                    {
                        UpdateTime(s + "s");
                        t = 1000;
                    }
                    else
                    {
                        UpdateTime(null);
                        return;
                    }
                }

                try
                {
                    await Task.Delay(t, cancel);
                }
                catch (TaskCanceledException)
                {
                    return;
                }
            }
        }

        private void UpdateTime(string text)
        {
            if (Util.Invoke.IfRequired(this,
                delegate
                {
                    UpdateTime(text);
                }))
            {
                return;
            }

            if (text == null)
                labelTime.Text = "";
            else
                labelTime.Text = "(auto in " + text + ")";
        }

        private void UpdateStatus()
        {
            if (Util.Invoke.IfRequired(this, UpdateStatus))
            {
                return;
            }

            var v = Settings.BackgroundPatchingEnabled.Value;

            string status;
            Cursor cursor = Cursors.Default;
            Color color = SystemColors.ControlText;
            bool rescanEnabled = false;

            if (checkEnabled.Checked = v)
            {
                if (Tools.BackgroundPatcher.Instance.IsActive)
                {
                    status = "Active";
                }
                else
                {
                    status = "Inactive";
                    cursor = Cursors.Hand;
                    color = Color.FromArgb(49, 121, 242);
                    rescanEnabled = true;
                }
            }
            else
                status = "Disabled";

            labelRecheckManifests.Enabled = rescanEnabled;

            labelStatus.Cursor = cursor;
            labelStatus.ForeColor = color;
            labelStatus.Text = status;
        }

        private byte GetLang()
        {
            var s = Settings.BackgroundPatchingLang;
            if (s.HasValue)
            {
                var l = s.Value;
                if (l >= LANG.Length)
                    s.Value = l = 0;
                return l;
            }

            return 0;
        }

        void bp_StateChanged(object sender, EventArgs e)
        {
            if (Util.Invoke.IfRequired(this,
                delegate
                {
                    bp_StateChanged(sender, e);
                }))
            {
                return;
            }

            UpdateStatus();
        }

        void bp_Starting(object sender, EventArgs e)
        {
            if (Util.Invoke.IfRequired(this,
                delegate
                {
                    bp_Starting(sender, e);
                }))
            {
                return;
            }

            var s = "---";
            labelFiles.Text = s;
            labelDownloaded.Text = s;
            labelSize.Text = s;
            progressDownload.Value = 0;
            labelSizeLabel.Text = "Remaining";

            labelSizeEstimated.Visible = false;
            labelSize.Visible = true;
            labelSizeContent.Visible = false;
            labelSizeContentValue.Visible = false;
            labelSizeCore.Visible = false;
            labelSizeCoreValue.Visible = false;

            UpdateStatus();

            errored = false;
            manifestsComplete = false;
            filesTotal = filesDownloaded = 0;
            downloadRate = 0;
            bytesDownloaded = estimatedBytesRemaining = contentBytesTotal = contentBytesCore = 0;

            if (taskBar != null)
                taskBar.SetState(this.Handle, Windows.Taskbar.TaskbarStates.Indeterminate);
        }

        void bp_Complete(object sender, EventArgs e)
        {
            if (Util.Invoke.IfRequired(this,
                delegate
                {
                    bp_Complete(sender, e);
                }))
            {
                return;
            }

            UpdateStatus();

            labelSizeLabel.Text = "Size";
            if (!errored)
                labelFiles.Text = string.Format("{0:#,##0}", filesDownloaded);
            labelDownloaded.Text = Util.Text.FormatBytes(bytesDownloaded);

            labelSizeEstimated.Visible = false;
            labelSize.Visible = false;
            labelSizeContent.Visible = true;
            labelSizeContentValue.Visible = true;
            labelSizeCore.Visible = true;
            labelSizeCoreValue.Visible = true;

            labelSizeContentValue.Text = Util.Text.FormatBytes(contentBytesTotal - contentBytesCore);
            labelSizeCoreValue.Text = Util.Text.FormatBytes(contentBytesCore);


            if (filesDownloaded == 0 || filesTotal != filesDownloaded)
                if (taskBar != null)
                    taskBar.SetState(this.Handle, Windows.Taskbar.TaskbarStates.NoProgress);
        }

        void bp_Error(object sender, string e, Exception ex)
        {
            if (Util.Invoke.IfRequired(this,
                delegate
                {
                    bp_Error(sender, e, ex);
                }))
            {
                return;
            }

            labelFiles.Text = e;
            labelFiles.Tag = ex;
            errored = true;

            if (taskBar != null)
                taskBar.SetState(this.Handle, Windows.Taskbar.TaskbarStates.Error);
        }

        void bp_DownloadProgress(object sender, Tools.BackgroundPatcher.DownloadProgressEventArgs e)
        {
            if (Util.Invoke.IfRequired(this,
                delegate
                {
                    bp_DownloadProgress(sender, e);
                }))
            {
                return;
            }

            bool update = false;
            if (e.downloadRate != this.downloadRate)
            {
                this.downloadRate = e.downloadRate;
                update = true;
            }

            this.contentBytesTotal = e.contentBytesTotal;
            this.contentBytesCore = e.contentBytesCore;

            if (e.bytesDownloaded != this.bytesDownloaded)
            {
                this.bytesDownloaded = e.bytesDownloaded;
                update = true;
            }

            if (update)
            {
                var t = Util.Text.FormatBytes(bytesDownloaded);
                if (downloadRate > 0)
                    labelDownloaded.Text = t + " @ " + Util.Text.FormatBytes(downloadRate) + "/s";
                else
                    labelDownloaded.Text = t;
            }

            if (e.estimatedBytesRemaining != this.estimatedBytesRemaining)
            {
                this.estimatedBytesRemaining = e.estimatedBytesRemaining;
                labelSize.Text = Util.Text.FormatBytes(estimatedBytesRemaining);
            }

            int files = e.filesDownloaded + e.manifestsDownloaded;
            int total = e.filesTotal + e.manifestsTotal;

            if (files != this.filesDownloaded || total != this.filesTotal)
            {
                this.filesDownloaded = files;
                this.filesTotal = total;

                if (!manifestsComplete && e.manifestsDownloaded == e.manifestsTotal)
                {
                    manifestsComplete = true;
                    labelSizeEstimated.Visible = true;
                    if (taskBar != null)
                        taskBar.SetState(this.Handle, Windows.Taskbar.TaskbarStates.Normal);
                }

                if (manifestsComplete)
                {
                    if (!errored)
                        labelFiles.Text = string.Format("{0:#,##0} of {1:#,##0}", files, total);

                    if (progressDownload.Maximum != e.filesTotal)
                        progressDownload.Maximum = e.filesTotal;
                    if (progressDownload.Value != e.filesDownloaded)
                        progressDownload.Value = e.filesDownloaded;

                    if (taskBar != null)
                        taskBar.SetValue(this.Handle, (ulong)e.filesDownloaded, (ulong)e.filesTotal);
                }
                else if (!errored)
                    labelFiles.Text = string.Format("{0:#,##0} of ...", files);
            }
        }

        private void formBackgroundPatcher_Load(object sender, EventArgs e)
        {
            
        }

        void langSelect_Click(object sender, EventArgs e)
        {
            var l = sender as Label;
            if (l != null)
            {
                labelLang.Text = l.Text;
                Settings.BackgroundPatchingLang.Value = (byte)l.Tag;
                labelLang.Visible = true;
                panelLang.Visible = false;
            }
        }

        private void labelStatus_Click(object sender, EventArgs e)
        {
            var bp = Tools.BackgroundPatcher.Instance;
            if (checkEnabled.Checked && bp != null && !bp.IsActive)
                bp.Start();
        }

        private void checkEnabled_CheckedChanged(object sender, EventArgs e)
        {
            Settings.BackgroundPatchingEnabled.Value = checkEnabled.Checked;
        }

        private void labelLang_Click(object sender, EventArgs e)
        {
            ShowLang();
        }

        private async void ShowLang()
        {
            byte j = 1;
            var _lang = GetLang();
            for (byte i = 0; i < lang.Length; i++)
            {
                Label l;
                if (i == _lang)
                    l = lang[0];
                else
                    l = lang[j++];

                l.Text = LANG[i];
                l.Tag = i;
            }

            await ShowPanel(labelLang, panelLang);
        }

        private void labelSize_SizeChanged(object sender, EventArgs e)
        {
            labelSizeEstimated.Location = new Point(labelSize.Location.X + labelSize.Width + 5, labelSize.Location.Y);
        }

        private void labelFiles_MouseEnter(object sender, EventArgs e)
        {
            if (errored)
            {
                var ex =  labelFiles.Tag as Exception;
                if (ex != null)
                {
                    if (tooltip == null)
                        tooltip = new ToolTip();

                    tooltip.Show(ex.Message, labelFiles);
                }
            }
        }

        private void formBackgroundPatcher_FormClosed(object sender, FormClosedEventArgs e)
        {
            Settings.BackgroundPatchingEnabled.ValueChanged -= BackgroundPatchingEnabled_ValueChanged;
            Settings.PatchingSpeedLimit.ValueChanged -= PatchingSpeedLimit_ValueChanged;
            Settings.PatchingOptions.ValueChanged -= PatchingOptions_ValueChanged;

            Tools.AutoUpdate.NextCheckChanged -= AutoUpdate_NextCheckChanged;

            if (cancelTime != null)
            {
                using (cancelTime)
                {
                    cancelTime.Cancel();
                }
                cancelTime = null;
            }

            var bp = Tools.BackgroundPatcher.Instance;
            bp.DownloadProgress -= bp_DownloadProgress;
            bp.Error -= bp_Error;
            bp.Complete -= bp_Complete;
            bp.Starting -= bp_Starting;
            bp.StateChanged -= bp_StateChanged;
        }

        private void checkEnabled_Click(object sender, EventArgs e)
        {
            if (checkEnabled.Checked && !Settings.LocalAssetServerEnabled.Value)
            {
                if (MessageBox.Show(this, "The asset proxy is required to use this.\n\nEnable it now?", "", MessageBoxButtons.YesNo, MessageBoxIcon.Exclamation) == System.Windows.Forms.DialogResult.Yes)
                    Settings.LocalAssetServerEnabled.Value = true;
            }
        }

        private void labelStatus_SizeChanged(object sender, EventArgs e)
        {
            labelTime.Location = new Point(labelStatus.Location.X + labelStatus.Width + 5, labelStatus.Location.Y);
        }

        private void sliderThreads_ValueChanged(object sender, EventArgs e)
        {
            var v = ((UI.Controls.FlatSlider)sender).Value;
            labelThreads.Text = ((int)(THREADS_MIN + v * THREADS_MAX + 0.5f)).ToString();
        }

        private void checkSpeedLimit_CheckedChanged(object sender, EventArgs e)
        {
            sliderSpeedLimit.Enabled = checkSpeedLimit.Checked;
        }

        private void sliderSpeedLimit_ValueChanged(object sender, EventArgs e)
        {
            var v = ((UI.Controls.FlatSlider)sender).Value;
            if (v >= 0.5f)
                labelSpeedLimit.Text = Util.Text.FormatBytes(SPEED_LIMIT_MIN_1 + (int)(SPEED_LIMIT_MAX_1 * (v - 0.5f) / 0.5f + 0.5f)) + "/s";
            else
                labelSpeedLimit.Text = Util.Text.FormatBytes(SPEED_LIMIT_MIN_0 + (int)(SPEED_LIMIT_MAX_0 * v / 0.5f + 0.5f)) + "/s";
        }

        private async void labelAdvanced_Click(object sender, EventArgs e)
        {
            await ShowPanel(labelAdvanced, panelAdvanced);

            if (checkSpeedLimit.Checked)
            {
                var value = sliderSpeedLimit.Value;
                if (value >= 0.5f)
                    Settings.PatchingSpeedLimit.Value = SPEED_LIMIT_MIN_1 + (int)(SPEED_LIMIT_MAX_1 * (value - 0.5f) / 0.5f + 0.5f);
                else
                    Settings.PatchingSpeedLimit.Value = SPEED_LIMIT_MIN_0 + (int)(SPEED_LIMIT_MAX_0 * value / 0.5f + 0.5f);
            }
            else
                Settings.PatchingSpeedLimit.Clear();

            Settings.BackgroundPatchingMaximumThreads.Value = (byte)(THREADS_MIN + sliderThreads.Value * THREADS_MAX + 0.5f);

            var options = Settings.PatchingOptions.Value;

            if (checkUseHttps.Checked)
                options |= Settings.PatchingFlags.UseHttps;
            else
                options &= ~Settings.PatchingFlags.UseHttps;

            if (options != Settings.PatchingFlags.None)
                Settings.PatchingOptions.Value = options;
            else
                Settings.PatchingOptions.Clear();
        }

        private async Task ShowPanel(Label label, Control panel)
        {
            panel.BringToFront();
            panel.Visible = true;
            label.Visible = false;
            panel.Focus();

            var cancel = new System.Threading.CancellationTokenSource();
            var token = cancel.Token;

            EventHandler visibleChanged = null;
            visibleChanged = delegate
            {
                cancel.Cancel();
            };

            panel.VisibleChanged += visibleChanged;

            var r = this.RectangleToScreen(panel.Bounds);
            do
            {
                try
                {
                    await Task.Delay(500, token);
                }
                catch (TaskCanceledException)
                {
                    break;
                }
            }
            while (r.Contains(Cursor.Position) || Control.MouseButtons.HasFlag(MouseButtons.Left));

            panel.VisibleChanged -= visibleChanged;

            if (!cancel.IsCancellationRequested)
            {
                panel.Visible = false;
                label.Visible = true;
            }

            cancel.Dispose();
        }

        private void labelSizeContentValue_SizeChanged(object sender, EventArgs e)
        {
            labelSizeCore.Location = new Point(labelSizeContentValue.Location.X + labelSizeContentValue.Width, labelSizeContentValue.Location.Y);
            labelSizeCoreValue.Location = new Point(labelSizeCore.Location.X + labelSizeCore.Width, labelSizeCore.Location.Y);
        }

        private void labelRecheckManifests_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show(this, "All update manifests will be downloaded. Are you sure?\n\nThis should only be used when Gw2.dat has been partially updated to the most recent build, but not completely.", "Are you sure?", MessageBoxButtons.YesNo, MessageBoxIcon.Exclamation) != System.Windows.Forms.DialogResult.Yes)
                return;

            var bp = Tools.BackgroundPatcher.Instance;
            if (checkEnabled.Checked && bp != null && !bp.IsActive)
                bp.Start(true);
        }

        private void checkUseHttps_Click(object sender, EventArgs e)
        {
            if (checkUseHttps.Checked)
            {
                MessageBox.Show(this, "Downloading patches over an encrypted connection is not officially supported.\n\nAvailability will vary by server.", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            }
        }
    }
}
