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

namespace Gw2Launcher.UI
{
    public partial class formAssetProxy : Base.BaseForm
    {
        private const int SPEED_LIMIT_MIN_0 = 102400;
        private const int SPEED_LIMIT_MAX_0 = 1048576 - SPEED_LIMIT_MIN_0;
        private const int SPEED_LIMIT_MIN_1 = 1048576;
        private const int SPEED_LIMIT_MAX_1 = 10485760 - SPEED_LIMIT_MIN_1;

        private static Thread threadDelete;

        private readonly string PATH_TEMP;
        private Dictionary<ushort, DataGridViewRow> activeRows;
        private int nextIndex;
        private Net.AssetProxy.ProxyServer server;
        private long bytesDownloaded, bytesUploaded;
        private string lastRequest;
        private CancellationTokenSource cancelToken;
        private long cacheStorage;
        private Net.AssetProxy.Cache.PurgeProgressEventArgs purgeProgress;
        private bool forceShowPanel;
        private int clients;

        private class DataRecord
        {
            public int id;
            public long bytes;

            public override string ToString()
            {
                return Util.Text.FormatBytes(bytes);
            }
        }

        public formAssetProxy()
        {
            InitializeComponents();

            try
            {
                var t = threadDelete;
                if (t != null && t.IsAlive)
                {
                    t.Abort();
                }
            }
            catch (Exception e)
            {
                Util.Logging.Log(e);
            }

            PATH_TEMP = Path.Combine(Path.GetTempPath(), "assetproxy");
            
            try
            {
                DirectoryInfo di = new DirectoryInfo(PATH_TEMP);
                if (!di.Exists)
                    di.Create();
            }
            catch (Exception ex)
            {
                Util.Logging.Log(ex);
            }

            nextIndex = 1;
            activeRows = new Dictionary<ushort, DataGridViewRow>();

            bool isEnabled = Net.AssetProxy.ServerController.Enabled;

            checkEnabled.Checked = isEnabled;
            checkEnabled.CheckedChanged += checkEnabled_CheckedChanged;

            PatchingSpeedLimit_ValueChanged(Settings.PatchingSpeedLimit, null);
            PatchingOptions_ValueChanged(Settings.PatchingOptions, null);
            PatchingPort_ValueChanged(Settings.PatchingPort, null);

            UpdateStatus();

            this.Shown += formAssetProxy_Shown;
        }

        protected override void OnInitializeComponents()
        {
            base.OnInitializeComponents();

            InitializeComponent();
        }

        void formAssetProxy_Shown(object sender, EventArgs e)
        {
            this.Shown -= formAssetProxy_Shown;

            if (Net.AssetProxy.ServerController.Server != null)
                ServerController_Created(null, Net.AssetProxy.ServerController.Server);
            Net.AssetProxy.ServerController.Created += ServerController_Created;
            Net.AssetProxy.ServerController.EnabledChanged += ServerController_EnabledChanged;

            cacheStorage = Net.AssetProxy.Cache.Bytes;
            Net.AssetProxy.Cache.CacheStorage += Cache_CacheStorage;
            Net.AssetProxy.Cache.CachePurged += Cache_CachePurged;
            Net.AssetProxy.Cache.PurgeProgress += Cache_PurgeProgress;

            Settings.PatchingSpeedLimit.ValueChanged += PatchingSpeedLimit_ValueChanged;
            Settings.PatchingOptions.ValueChanged += PatchingOptions_ValueChanged;
            Settings.PatchingPort.ValueChanged += PatchingPort_ValueChanged;

            cancelToken = new CancellationTokenSource();
            UpdateStats(cancelToken.Token);
        }

        void Cache_PurgeProgress(object sender, Net.AssetProxy.Cache.PurgeProgressEventArgs e)
        {
            purgeProgress = e;
        }

        void Cache_CachePurged(object sender, EventArgs e)
        {
            Util.Invoke.Required(this,
                delegate
                {
                    labelCached.Text = Util.Text.FormatBytes(cacheStorage);
                    labelCached.Enabled = true;
                });
        }

        void Cache_CacheStorage(long bytes)
        {
            cacheStorage = bytes;
        }

        void ServerController_EnabledChanged(object sender, bool e)
        {
            UpdateStatus();
        }

        private async void UpdateStats(CancellationToken cancel)
        {
            long lastDownloaded = 0;
            long lastUploaded = 0;
            long lastDownloadSample = 0;
            int lastSpeed = 0;
            DateTime nextUpdate = DateTime.UtcNow.AddSeconds(3);
            string dcache = Util.Text.FormatBytes(0), ucache = Util.Text.FormatBytes(0), dscache = "";
            long cacheStorage = -1;
            bool cacheInit = false;
            int lastPurge = 0;
            int lastClients = -1;

            while (true)
            {
                try
                {
                    await Task.Delay(1000, cancel);
                }
                catch (TaskCanceledException)
                {
                    return;
                }

                if (lastRequest != null)
                {
                    lock (activeRows)
                    {
                        labelLastRequest.Text = lastRequest;
                        lastRequest = null;
                    }
                }

                bool update = false;
                var bytesDownloaded = this.bytesDownloaded;

                if (lastDownloaded != bytesDownloaded)
                {
                    lastDownloaded = bytesDownloaded;
                    dcache = Util.Text.FormatBytes(lastDownloaded);
                    update = true;
                }
                if (lastUploaded != bytesUploaded)
                {
                    lastUploaded = bytesUploaded;
                    ucache = Util.Text.FormatBytes(lastUploaded);
                    update = true;
                }
                if (lastClients != clients)
                {
                    lastClients = clients;
                    labelClients.Text = lastClients.ToString();
                }
                if (purgeProgress != null)
                {
                    if (purgeProgress.purged == purgeProgress.total)
                    {
                        lastPurge = 0;
                        purgeProgress = null;
                    }
                    else
                    {
                        if (labelCached.Enabled)
                            labelCached.Enabled = false;
                        if (lastPurge != 0 || (float)purgeProgress.purged / purgeProgress.total < 0.25f) //only show progress if it'll take more than a few seconds to complete
                        {
                            if (lastPurge != purgeProgress.purged)
                            {
                                lastPurge = purgeProgress.purged;
                                labelCached.Text = string.Format("{0:#,##0} of {1:#,##0}", lastPurge, purgeProgress.total);
                            }
                        }
                    }
                }
                else if (cacheStorage != this.cacheStorage)
                {
                    cacheStorage = this.cacheStorage;

                    if (cacheInit)
                    {
                        labelCached.Text = Util.Text.FormatBytes(cacheStorage);
                    }
                    else
                    {
                        if (cacheInit = Net.AssetProxy.Cache.IsInitialized)
                            cacheStorage = -1;
                        labelCached.Text = "---";
                    }
                }
                else if (!cacheInit && Net.AssetProxy.Cache.IsInitialized)
                {
                    cacheInit = true;
                    cacheStorage = -1;
                }

                if (bytesDownloaded > 0)
                {
                    var now = DateTime.UtcNow;
                    if (now > nextUpdate)
                    {
                        var s = now.Subtract(nextUpdate).TotalSeconds + 3;
                        if (s > 0)
                        {
                            var sample = (int)((bytesDownloaded - lastDownloadSample) / s + 0.5);
                            if (sample < 1024)
                                sample = 0;

                            if (sample != lastSpeed)
                            {
                                lastSpeed = sample;
                                if (sample != 0)
                                    dscache = " @ " + Util.Text.FormatBytes(sample) + "/s";
                                else
                                    dscache = "";
                                update = true;
                            }

                            nextUpdate = now.AddSeconds(3);
                            lastDownloadSample = bytesDownloaded;
                        }
                    }
                }

                if (update)
                    labelDownloaded.Text = ucache + " / " + dcache + dscache;
            }
        }

        private void UpdateCell(DataGridViewCell cell, Color foreColor, object value, string tooltip)
        {
            if (Util.Invoke.IfRequired(this,
                delegate
                {
                    UpdateCell(cell, foreColor, value, tooltip);
                }))
            {
                return;
            }

            cell.Value = value;
            cell.Style.ForeColor = foreColor;
            cell.ToolTipText = tooltip;
        }

        private void UpdateCell(DataGridViewLinkCell cell, bool enabled)
        {
            if (Util.Invoke.IfRequired(this,
                delegate
                {
                    UpdateCell(cell, enabled);
                }))
            {
                return;
            }

            if (enabled)
            {
                var def = (DataGridViewLinkCell)cell.DataGridView.RowTemplate.Cells[cell.ColumnIndex];
                cell.LinkBehavior = def.LinkBehavior;
                cell.LinkColor = cell.ActiveLinkColor = def.LinkColor;
            }
            else
            {
                cell.LinkBehavior = LinkBehavior.NeverUnderline;
                cell.LinkColor = cell.ActiveLinkColor = SystemColors.GrayText;
            }
        }

        private void AddRow(DataGridViewRow row)
        {
            if (Util.Invoke.IfRequired(this,
                delegate
                {
                    AddRow(row);
                }))
            {
                return;
            }

            row.Cells[columnIndex.Index].Value = nextIndex;
            ((DataRecord)row.Cells[columnResponse.Index].Value).id = nextIndex;

            nextIndex++;

            labelClear.Visible = true;
            gridRecord.Rows.Add(row);
        }

        void ServerController_Created(object sender, Net.AssetProxy.ProxyServer e)
        {
            server = e;

            e.ClientClosed += server_ClientClosed;
            e.ClientError += server_ClientError;
            e.ResponseHeaderReceived += server_ResponseHeaderReceived;
            e.RequestHeaderReceived += server_RequestHeaderReceived;
            e.ResponseDataReceived += server_ResponseDataReceived;
            e.RequestDataReceived += server_RequestDataReceived;
            e.ServerStarted += server_ServerStarted;
            e.ServerStopped += server_ServerStopped;
            e.ServerRestarted += server_ServerRestarted;
            e.ServerError += server_ServerError;
            e.ClientsChanged += server_ClientsChanged;

            clients = e.Clients;

            UpdateStatus();
        }

        void server_ClientsChanged(object sender, int e)
        {
            clients = e;
        }

        void server_ServerError(object sender, Exception e)
        {
            Util.Invoke.Required(labelError,
                delegate
                {
                    labelError.Visible = true;
                });
        }

        void server_ServerRestarted(object sender, EventArgs e)
        {
            UpdateStatus();
        }

        void server_ServerStarted(object sender, EventArgs e)
        {
            lock (activeRows)
            {
                activeRows.Clear();
            }

            UpdateStatus();
        }

        void server_ServerStopped(object sender, EventArgs e)
        {
            UpdateStatus();
        }

        void server_ClientError(object sender, Exception e)
        {
            if (!checkRecord.Checked)
                return;

            var client = (Net.AssetProxy.Client)sender;

            DataGridViewRow row;
            lock (activeRows)
            {
                if (!activeRows.TryGetValue(client.ID, out row))
                    row = null;
            }

            if (row == null)
            {
                row = (DataGridViewRow)gridRecord.RowTemplate.Clone();
                row.CreateCells(gridRecord);

                var c = row.Cells[columnResponseCode.Index];
                c.Value = "---";
                c.ToolTipText = e.Message;
                c.Style.ForeColor = Color.DarkRed;

                c = row.Cells[columnUrl.Index];
                c.Value = "-";
                c.Style.ForeColor = Color.Gray;

                c = row.Cells[columnResponse.Index];
                c.Value = new DataRecord();

                var lc = (DataGridViewLinkCell)c;
                lc.LinkBehavior = LinkBehavior.NeverUnderline;
                lc.LinkColor = lc.ActiveLinkColor = SystemColors.GrayText;

                lock (activeRows)
                {
                    activeRows[client.ID] = row;
                }

                AddRow(row);
            }
            else
            {
                var c = row.Cells[columnResponseCode.Index];
                if (c.Value is string)
                {
                    UpdateCell(c, Color.DarkRed, "---", e.Message);
                }
            }
        }

        void server_ClientClosed(object sender, Net.AssetProxy.Client e)
        {
            if (!checkRecord.Checked)
                return;

            DataGridViewRow row;
            lock (activeRows)
            {
                if (!activeRows.TryGetValue(e.ID, out row))
                    return;
            }

            var c = row.Cells[columnResponse.Index];
            if (row.Index != -1)
            {
                try
                {
                    gridRecord.InvalidateCell(c);
                }
                catch (Exception ex)
                {
                    Util.Logging.Log(ex);
                }
            }
        }

        void server_ResponseDataReceived(object sender, ArraySegment<byte> e)
        {
            if (!checkRecord.Checked)
            {
                lock (activeRows)
                {
                    bytesDownloaded += e.Count;
                }
                return;
            }

            var client = (Net.AssetProxy.Client)sender;

            DataGridViewRow row;
            lock (activeRows)
            {
                bytesDownloaded += e.Count;

                if (!activeRows.TryGetValue(client.ID, out row))
                    return;
            }

            var c = row.Cells[columnResponse.Index];
            DataRecord data = (DataRecord)c.Value;
            data.bytes += e.Count;

            if (checkRecordData.Checked)
            {
                try
                {
                    using (var stream = File.Open(Path.Combine(PATH_TEMP, ((DataRecord)row.Cells[columnResponse.Index].Value).id.ToString()), FileMode.Append, FileAccess.Write, FileShare.ReadWrite))
                    {
                        stream.Write(e.Array, e.Offset, e.Count);
                    }
                }
                catch (Exception ex)
                {
                    Util.Logging.Log(ex);
                }
            }
        }

        void server_RequestDataReceived(object sender, ArraySegment<byte> e)
        {
            if (!checkRecord.Checked)
            {
                lock (activeRows)
                {
                    bytesUploaded += e.Count;
                }
                return;
            }

            var client = (Net.AssetProxy.Client)sender;

            DataGridViewRow row;
            lock (activeRows)
            {
                bytesUploaded += e.Count;

                if (!activeRows.TryGetValue(client.ID, out row))
                    return;
            }

            var c = row.Cells[columnResponse.Index];
            DataRecord data = (DataRecord)c.Value;
            data.bytes += e.Count;

            if (checkRecordData.Checked)
            {
                try
                {
                    using (var stream = File.Open(Path.Combine(PATH_TEMP, ((DataRecord)row.Cells[columnResponse.Index].Value).id.ToString()), FileMode.Append, FileAccess.Write, FileShare.ReadWrite))
                    {
                        stream.Write(e.Array, e.Offset, e.Count);
                    }
                }
                catch (Exception ex)
                {
                    Util.Logging.Log(ex);
                }
            }
        }

        void server_RequestHeaderReceived(object sender, Net.AssetProxy.HttpStream.HttpRequestHeader e)
        {
            if (!checkRecord.Checked)
            {
                lock (activeRows)
                {
                    lastRequest = e.Location;
                }
                return;
            }

            var client = (Net.AssetProxy.Client)sender;
            var row = (DataGridViewRow)gridRecord.RowTemplate.Clone();
            row.CreateCells(gridRecord);

            var c = row.Cells[columnResponseCode.Index];
            c.Value = "-";
            c.Style.ForeColor = Color.Gray;

            row.Cells[columnUrl.Index].Value = e.Location;

            c = row.Cells[columnResponse.Index];
            DataRecord dr;
            c.Value = dr = new DataRecord();

            if (!checkRecordData.Checked)
            {
                var lc = (DataGridViewLinkCell)c;
                lc.LinkBehavior = LinkBehavior.NeverUnderline;
                lc.LinkColor = lc.ActiveLinkColor = SystemColors.GrayText;// row.DefaultCellStyle.ForeColor;
            }

            lock (activeRows)
            {
                lastRequest = e.Location;
                activeRows[client.ID] = row;
            }

            AddRow(row);

            if (checkRecordData.Checked)
            {

                try
                {
                    File.Delete(Path.Combine(PATH_TEMP, dr.id.ToString()));
                }
                catch (Exception ex)
                {
                    Util.Logging.Log(ex);
                }
            }
        }

        void server_ResponseHeaderReceived(object sender, Net.AssetProxy.HttpStream.HttpResponseHeader e)
        {
            if (!checkRecord.Checked)
                return;

            var client = (Net.AssetProxy.Client)sender;

            DataGridViewRow row;
            lock (activeRows)
            {
                if (!activeRows.TryGetValue(client.ID, out row))
                    return;
            }

            int code = (int)e.StatusCode;
            Color color;
            object value;
            string tooltip;

            if (code == Net.AssetProxy.HttpStream.HttpResponseHeader.STATUS_CODE_CACHED)
            {
                value = "cache";
                tooltip = "Response from cache";
                color = SystemColors.GrayText;// Color.Gray;

                UpdateCell((DataGridViewLinkCell)row.Cells[columnResponse.Index], false);
            }
            else
            {
                value = code;
                tooltip = e.StatusDescription;

                if (code / 500 == 1)
                    color = Color.DarkRed;
                else
                    color = Color.Empty;
            }

            UpdateCell(row.Cells[columnResponseCode.Index], color, value, tooltip);

        }

        void checkEnabled_CheckedChanged(object sender, EventArgs e)
        {
            Net.AssetProxy.ServerController.Enabled = checkEnabled.Checked;
        }

        private void formAssetProxy_Load(object sender, EventArgs e)
        {

        }

        private void UpdateStatus()
        {
            if (Util.Invoke.IfRequired(this, UpdateStatus))
            {
                return;
            }

            string status;
            Cursor cursor = Cursors.Default;
            Color color = SystemColors.ControlText;

            if (checkEnabled.Checked = Net.AssetProxy.ServerController.Enabled)
            {
                if (server != null && server.IsListening)
                {
                    status = "Listening on port " + server.CurrentPort;
                    labelError.Visible = false;
                }
                else
                {
                    status = "Inactive";
                    cursor = Windows.Cursors.Hand;
                    color = Color.FromArgb(49, 121, 242);
                }
            }
            else
            {
                status = "Disabled";
                labelError.Visible = false;
            }

            labelStatus.Cursor = cursor;
            labelStatus.ForeColor = color;
            labelStatus.Text = status;
        }

        private void checkRecord_CheckedChanged(object sender, EventArgs e)
        {
            checkRecordData.Enabled = checkRecord.Checked;
        }

        void PatchingPort_ValueChanged(object sender, EventArgs e)
        {
            if (Util.Invoke.IfRequired(this,
                delegate
                {
                    PatchingPort_ValueChanged(sender, e);
                }))
            {
                return;
            }

            var v = ((Settings.ISettingValue<ushort>)sender);

            checkPort.Checked = v.HasValue;
            if (v.HasValue)
                Util.NumericUpDown.SetValue(numericPort, v.Value);
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

            checkDisableCaching.Checked = v.HasFlag(Settings.PatchingFlags.DisableCaching);
            checkUseHttps.Checked = v.HasFlag(Settings.PatchingFlags.UseHttps);
            checkOverrideHosts.Checked = v.HasFlag(Settings.PatchingFlags.OverrideHosts);
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

        private void formAssetProxy_FormClosing(object sender, FormClosingEventArgs e)
        {
            Settings.PatchingSpeedLimit.ValueChanged -= PatchingSpeedLimit_ValueChanged;
            Settings.PatchingOptions.ValueChanged -= PatchingOptions_ValueChanged;
            Settings.PatchingPort.ValueChanged -= PatchingPort_ValueChanged;

            if (cancelToken != null)
                cancelToken.Cancel();

            if (server != null)
            {
                server.ClientClosed -= server_ClientClosed;
                server.ClientError -= server_ClientError;
                server.ResponseHeaderReceived -= server_ResponseHeaderReceived;
                server.RequestHeaderReceived -= server_RequestHeaderReceived;
                server.ResponseDataReceived -= server_ResponseDataReceived;
                server.RequestDataReceived -= server_ResponseDataReceived;
                server.ServerStarted -= server_ServerStarted;
                server.ServerStopped -= server_ServerStopped;
                server.ServerRestarted -= server_ServerRestarted;
                server.ServerError -= server_ServerError;
                server.ClientsChanged -= server_ClientsChanged;
            }

            Net.AssetProxy.ServerController.EnabledChanged -= ServerController_EnabledChanged;
            Net.AssetProxy.ServerController.Created -= ServerController_Created;
            Net.AssetProxy.Cache.CacheStorage -= Cache_CacheStorage;
            Net.AssetProxy.Cache.CachePurged -= Cache_CachePurged;
            Net.AssetProxy.Cache.PurgeProgress -= Cache_PurgeProgress;

            var t = threadDelete = new Thread(new ThreadStart(DeleteTemp));
            t.Start();
        }

        private void DeleteTemp()
        {
            try
            {
                DirectoryInfo di = new DirectoryInfo(PATH_TEMP);
                if (di.Exists)
                {
                    di.Delete(true);
                }
            }
            catch (Exception ex)
            {
                Util.Logging.Log(ex);
            }
        }

        private bool OpenData(FileInfo fi)
        {
            var p = new System.Diagnostics.Process();
            p.StartInfo.UseShellExecute = true;
            p.StartInfo.FileName = "notepad.exe";
            p.StartInfo.Arguments = '"' + fi.FullName + '"';

            try
            {
                using (p)
                {
                    return p.Start();
                }
            }
            catch (Exception ex)
            {
                Util.Logging.Log(ex);
            }

            //using (StreamReader reader = fi.OpenText())
            //{
            //    int limit = 1024 * 1024;
            //    if (fi.Length < limit)
            //        limit = (int)fi.Length;
            //    var buffer = new char[limit];

            //    int read = reader.ReadBlock(buffer, 0, limit);

            //    var s = new string(buffer, 0, limit);
            //}

            return false;
        }

        private void gridRecord_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.ColumnIndex == columnResponse.Index && e.RowIndex >= 0)
            {
                var cell = gridRecord.Rows[e.RowIndex].Cells[e.ColumnIndex];
                var d = (DataRecord)cell.Value;
                if (d.bytes > 0)
                {
                    try
                    {
                        FileInfo fi = new FileInfo(Path.Combine(PATH_TEMP, d.id.ToString()));
                        if (fi.Exists && fi.Length > 0)
                        {
                            OpenData(fi);
                        }
                        else
                        {
                            UpdateCell((DataGridViewLinkCell)cell, false);
                        }
                    }
                    catch (Exception ex)
                    {
                        Util.Logging.Log(ex);
                    }
                }
            }
        }

        private void gridRecord_SelectionChanged(object sender, EventArgs e)
        {
            gridRecord.ClearSelection();
        }

        private void labelStatus_Click(object sender, EventArgs e)
        {
            if (Net.AssetProxy.ServerController.Enabled)
            {
                var server = Net.AssetProxy.ServerController.Active;
                if (server != null)
                    server.Start();
            }
        }

        private void labelClear_Click(object sender, EventArgs e)
        {
            labelClear.Visible = false;
            gridRecord.Rows.Clear();
        }

        private void labelCached_Click(object sender, EventArgs e)
        {
            labelCached.Enabled = false;
            Net.AssetProxy.Cache.Clear();
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
            while (forceShowPanel || r.Contains(Cursor.Position) || Control.MouseButtons.HasFlag(MouseButtons.Left));

            panel.VisibleChanged -= visibleChanged;

            if (!cancel.IsCancellationRequested)
            {
                panel.Visible = false;
                label.Visible = true;
            }

            cancel.Dispose();
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

            var options = Settings.PatchingOptions.Value;

            if (checkDisableCaching.Checked)
                options |= Settings.PatchingFlags.DisableCaching;
            else
                options &= Settings.PatchingFlags.DisableCaching;

            if (checkUseHttps.Checked)
                options |= Settings.PatchingFlags.UseHttps;
            else
                options &= ~Settings.PatchingFlags.UseHttps;

            if (checkOverrideHosts.Checked)
                options |= Settings.PatchingFlags.OverrideHosts;
            else
                options &= ~Settings.PatchingFlags.OverrideHosts;

            if (checkPort.Checked)
                Settings.PatchingPort.Value = (ushort)numericPort.Value;
            else
                Settings.PatchingPort.Clear();

            if (options != Settings.PatchingFlags.None)
                Settings.PatchingOptions.Value = options;
            else
                Settings.PatchingOptions.Clear();
        }

        private void checkUseHttps_Click(object sender, EventArgs e)
        {
            if (checkUseHttps.Checked)
            {
                MessageBox.Show(this, "Downloading patches over an encrypted connection is not officially supported.\n\nAvailability will vary by server.", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            }
        }

        private void sliderSpeedLimit_ValueChanged(object sender, EventArgs e)
        {
            var v = ((UI.Controls.FlatSlider)sender).Value;
            if (v >= 0.5f)
                labelSpeedLimit.Text = Util.Text.FormatBytes(SPEED_LIMIT_MIN_1 + (int)(SPEED_LIMIT_MAX_1 * (v - 0.5f) / 0.5f + 0.5f)) + "/s";
            else
                labelSpeedLimit.Text = Util.Text.FormatBytes(SPEED_LIMIT_MIN_0 + (int)(SPEED_LIMIT_MAX_0 * v / 0.5f + 0.5f)) + "/s";
        }

        private void checkSpeedLimit_CheckedChanged(object sender, EventArgs e)
        {
            sliderSpeedLimit.Enabled = checkSpeedLimit.Checked;
        }

        private void checkPort_CheckedChanged(object sender, EventArgs e)
        {
            numericPort.Enabled = checkPort.Checked;
        }

        private void checkOverrideHosts_CheckedChanged(object sender, EventArgs e)
        {
            checkPort.Parent.Enabled = !checkOverrideHosts.Checked;
        }

        private Task<bool> SetHostsOverride(bool enabled)
        {
            return Task.Run<bool>(
                delegate
                {
                    var b = true;

                    var address = "127.0.0.1";
                    var host = Settings.ASSET_HOST;

                    if (enabled)
                    {
                        if (!Windows.Hosts.Contains(host, address))
                        {
                            try
                            {
                                b = Util.ProcessUtil.AddHostsEntry(host, address);
                            }
                            catch
                            {
                                b = false;
                            }
                        }
                    }
                    else 
                    {
                        if (Windows.Hosts.Contains(host, address))
                        {
                            try
                            {
                                b = Util.ProcessUtil.RemoveHostsEntry(host, address);
                            }
                            catch
                            {
                                b = false;
                            }
                        }
                    }

                    return b;
                });
        }

        private async void checkOverrideHosts_Click(object sender, EventArgs e)
        {
            checkOverrideHosts.Enabled = false;

            bool b;

            try
            {
                forceShowPanel = true;

                try
                {
                    b = await SetHostsOverride(checkOverrideHosts.Checked);
                }
                catch
                {
                    b = false;
                }

                if (!b)
                {
                    checkOverrideHosts.Checked = !checkOverrideHosts.Checked;
                }
            }
            finally
            {
                forceShowPanel = false;
            }

            checkOverrideHosts.Enabled = true;
        }
    }
}
