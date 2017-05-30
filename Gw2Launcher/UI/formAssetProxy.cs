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
    public partial class formAssetProxy : Form
    {
        private static Thread threadDelete;

        private readonly string PATH_TEMP;
        private Dictionary<ushort, DataGridViewRow> activeRows;
        private int nextIndex;
        private Net.AssetProxy.ProxyServer server;
        private long bytesDownloaded, bytesUploaded;
        private string lastRequest;
        private CancellationTokenSource cancelToken;
        private long cacheStorage;

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
            InitializeComponent();

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

            UpdateStatus();

            checkEnabled.Checked = isEnabled;
            checkEnabled.CheckedChanged += checkEnabled_CheckedChanged;

            this.Shown += formAssetProxy_Shown;
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

            cancelToken = new CancellationTokenSource();
            UpdateStats(cancelToken.Token);
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

                if (cacheStorage != this.cacheStorage)
                {
                    cacheStorage = this.cacheStorage;
                    labelCached.Text = Util.Text.FormatBytes(cacheStorage);
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
            if (this.InvokeRequired)
            {
                try
                {
                    this.Invoke(new MethodInvoker(
                        delegate
                        {
                            UpdateCell(cell, foreColor, value, tooltip);
                        }));
                }
                catch (Exception e)
                {
                    Util.Logging.Log(e);
                }

                return;
            }

            cell.Value = value;
            cell.Style.ForeColor = foreColor;
            cell.ToolTipText = tooltip;
        }

        private void UpdateCell(DataGridViewLinkCell cell, bool enabled)
        {
            if (this.InvokeRequired)
            {
                try
                {
                    this.Invoke(new MethodInvoker(
                        delegate
                        {
                            UpdateCell(cell, enabled);
                        }));
                }
                catch (Exception e)
                {
                    Util.Logging.Log(e);
                }

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
            if (this.InvokeRequired)
            {
                try
                {
                    this.Invoke(new MethodInvoker(
                        delegate
                        {
                            AddRow(row);
                        }));
                }
                catch (Exception e)
                {
                    Util.Logging.Log(e);
                }

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
            if (this.InvokeRequired)
            {
                try
                {
                    this.Invoke(new MethodInvoker(UpdateStatus));
                }
                catch (Exception e)
                {
                    Util.Logging.Log(e);
                }

                return;
            }

            string status;
            Cursor cursor = Cursors.Default;
            Color color = SystemColors.ControlText;

            if (checkEnabled.Checked = Net.AssetProxy.ServerController.Enabled)
            {
                if (server != null && server.IsListening)
                {
                    status = "Listening on port " + server.Port;
                }
                else
                {
                    status = "Inactive";
                    cursor = Cursors.Hand;
                    color = Color.FromArgb(49, 121, 242);
                }
            }
            else
                status = "Disabled";

            labelStatus.Cursor = cursor;
            labelStatus.ForeColor = color;
            labelStatus.Text = status;
        }

        private void checkRecord_CheckedChanged(object sender, EventArgs e)
        {
            checkRecordData.Enabled = checkRecord.Checked;
        }

        private void formAssetProxy_FormClosing(object sender, FormClosingEventArgs e)
        {
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
            }

            Net.AssetProxy.ServerController.EnabledChanged -= ServerController_EnabledChanged;
            Net.AssetProxy.ServerController.Created -= ServerController_Created;
            Net.AssetProxy.Cache.CacheStorage -= Cache_CacheStorage;

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
            Net.AssetProxy.Cache.Clear();
        }
    }
}
