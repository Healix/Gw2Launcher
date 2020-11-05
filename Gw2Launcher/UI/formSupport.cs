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
using System.Diagnostics;
using Gw2Launcher.UI.Controls;
using System.Net;
using System.Net.NetworkInformation;

namespace Gw2Launcher.UI
{
    public partial class formSupport : Base.BaseForm
    {
        private const string PATCH_SERVER = Net.AssetProxy.ProxyServer.PATCH_SERVER;
        private const string AUTH_NA_SERVER = "auth1.101.ArenaNetworks.com";
        private const string AUTH_EU_SERVER = "auth2.101.ArenaNetworks.com";
        private const byte LOOKUP_CONNECTION_LIMIT = 10;

        protected enum GridType
        {
            None,
            Login,
            Asset
        }

        private GridType inGridView;

        private SidebarButton selectedButton;
        private Process activeProcess;
        private DataGridViewRow rowSelectedPatchServer, rowSelectedLoginServer;
        private CancellationTokenSource cancelToken;
        private IPAddress existingAsset, existingLogin;

        private struct MillisCell : IComparable
        {
            public MillisCell(float value)
            {
                this.value = value;
            }

            public float value;

            public override string ToString()
            {
                return string.Format("{0:0}", value);
            }

            public int CompareTo(object obj)
            {
                if (obj is MillisCell)
                    return value.CompareTo(((MillisCell)obj).value);
                else
                    return -1;
            }
        }

        private struct AccountData<T>
        {
            public Settings.IAccount account;
            public T data;
        }

        private struct TaskPingResult
        {
            public int http;
            public int ping;
            public DataGridViewRow row;
        }

        public formSupport()
        {
            InitializeComponents();

            cancelToken = new CancellationTokenSource();

            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.formSupport_FormClosing);

            buttonDiagnostics.Tag = panelDiagnostics;
            buttonRepair.Tag = panelRepair;
            buttonAuthentication.Tag = panelAuthentication;
            buttonPatching.Tag = panelPatching;
            buttonPatchingIntercept.Tag = panelPatchIntercept;

            foreach (Control c in sidebarPanel1.Controls)
            {
                SidebarButton b = c as SidebarButton;
                if (b != null)
                {
                    b.SelectedChanged += sidebarButton_SelectedChanged;
                    b.Click += sidebarButton_Click;
                }
            }

            buttonDiagnostics.Selected = true;

            gridPatchServers.AdvancedCellBorderStyle.All = DataGridViewAdvancedCellBorderStyle.None;
            //progressPatchServer.Location = new Point(gridPatchServers.Location.X + gridPatchServers.Width / 2 - progressPatchServer.Width / 2, gridPatchServers.Location.Y + gridPatchServers.Height / 2 - progressPatchServer.Height / 2);
            labelPatchServerNoIPs.Location = new Point(gridPatchServers.Location.X + gridPatchServers.Width / 2 - labelPatchServerNoIPs.Width / 2, gridPatchServers.Location.Y + gridPatchServers.Height / 2 - labelPatchServerNoIPs.Height / 2);

            gridLoginServers.AdvancedCellBorderStyle.All = DataGridViewAdvancedCellBorderStyle.None;
            //progressLoginServer.Location = new Point(gridLoginServers.Location.X + gridLoginServers.Width / 2 - progressLoginServer.Width / 2, gridLoginServers.Location.Y + gridLoginServers.Height / 2 - progressLoginServer.Height / 2);
            labelLoginServerNoIPs.Location = new Point(gridLoginServers.Location.X + gridLoginServers.Width / 2 - labelLoginServerNoIPs.Width / 2, gridLoginServers.Location.Y + gridLoginServers.Height / 2 - labelLoginServerNoIPs.Height / 2);
            
            if (Settings.GuildWars2.Arguments.HasValue && !string.IsNullOrEmpty(Settings.GuildWars2.Arguments.Value))
            {
                string existing;

                existing = Util.Args.GetValue(Settings.GuildWars2.Arguments.Value, "assetsrv");
                if (!string.IsNullOrEmpty(existing))
                    IPAddress.TryParse(existing, out existingAsset);

                existing = Util.Args.GetValue(Settings.GuildWars2.Arguments.Value, "authsrv");
                if (!string.IsNullOrEmpty(existing))
                    IPAddress.TryParse(existing, out existingLogin);
            }

            //buttonPatchServerLookup.Tag = Settings.GetDnsServers();

            checkPatchInterceptEnable.Checked = Net.AssetProxy.ServerController.Enabled;
            Net.AssetProxy.ServerController.EnabledChanged += ServerController_EnabledChanged;
        }

        protected formSupport(GridType t, DataGridView grid, Form parent)
        {
            InitializeComponents();

            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.Sizable;

            inGridView = t;
            DataGridView grid1;

            if (t == GridType.Asset)
            {
                grid1 = this.gridPatchServers;
                this.Text = "Asset Servers";
            }
            else //GridType.Login;
            {
                grid1 = this.gridLoginServers;
                this.Text = "Authentication Servers";
            }

            Settings.GuildWars2.Arguments.ValueChanged += GW2Arguments_ValueChanged;

            grid1.AdvancedCellBorderStyle.All = DataGridViewAdvancedCellBorderStyle.None;

            this.Controls.Clear();
            this.Controls.Add(grid1);

            var padding = new Size(this.Width - this.ClientSize.Width,  this.Height - this.ClientSize.Height);
            grid1.Location = new Point(10, 10);
            grid1.Size = new Size(grid1.Width, this.ClientSize.Height * 3 / 4 - grid1.Location.Y * 2);
            grid1.Visible = true;

            this.Size = new System.Drawing.Size(grid1.Location.X * 2 + grid1.Width + padding.Width, grid1.Location.Y * 2 + grid1.Height + padding.Height);
            this.MaximumSize = new System.Drawing.Size(this.Width, Int32.MaxValue);

            grid1.Anchor = AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Bottom | AnchorStyles.Top;

            var rows = new DataGridViewRow[grid.Rows.Count];
            var r = 0;
            int indexEnabled;

            if (inGridView == GridType.Asset)
                indexEnabled = columnPatchServerEnable.Index;
            else
                indexEnabled = columnLoginServerEnable.Index;

            foreach (DataGridViewRow row in grid.Rows)
            {
                var _row = rows[r++] = (DataGridViewRow)grid1.RowTemplate.Clone();
                _row.CreateCells(grid1);

                foreach (DataGridViewCell cell in row.Cells)
                {
                    var _cell = _row.Cells[cell.ColumnIndex];
                    _cell.Value = cell.Value;
                    if (cell.HasStyle)
                        _cell.Style = cell.Style;
                }

                if ((bool)row.Cells[indexEnabled].Value)
                {
                    if (inGridView == GridType.Asset)
                        rowSelectedPatchServer = _row;
                    else
                        rowSelectedLoginServer = _row;
                }
            }

            grid1.Rows.AddRange(rows);

            this.StartPosition = FormStartPosition.Manual;
            var screen = Screen.FromControl(parent).WorkingArea;
            var y = parent.Location.Y + parent.Height / 2 - this.Height / 2;
            var x = parent.Location.X + parent.Width + 5;
            if (x + this.Width > screen.Width)
                x = screen.X + screen.Width - this.Width;
            this.Location = new Point(x, y);
        }

        protected override void OnInitializeComponents()
        {
            base.OnInitializeComponents();

            InitializeComponent();
        }

        void GW2Arguments_ValueChanged(object sender, EventArgs e)
        {
            var v = (Settings.ISettingValue<string>)sender;
            DataGridView grid;
            DataGridViewRow selected;
            int indexEnable, indexValue;
            string _ip;
            if (inGridView == GridType.Asset)
            {
                _ip = Util.Args.GetValue(v.Value, "assetsrv");
                grid = this.gridPatchServers;
                selected = rowSelectedPatchServer;
                indexEnable = columnPatchServerEnable.Index;
                indexValue = columnPatchServer.Index;
            }
            else
            {
                _ip = Util.Args.GetValue(v.Value, "authsrv");
                grid = this.gridLoginServers;
                selected = rowSelectedLoginServer;
                indexEnable = columnLoginServerEnable.Index;
                indexValue=columnLoginServer.Index;
            }

            IPAddress ip = null;
            if (!string.IsNullOrEmpty(_ip))
            {
                int i;
                if ((i = _ip.IndexOf(':')) == -1)
                    IPAddress.TryParse(_ip, out ip);
                else
                    IPAddress.TryParse(_ip.Substring(0, i), out ip);
            }

            if (selected != null)
            {
                if ((IPAddress)selected.Cells[indexValue].Value == ip)
                    return;
                selected.Cells[indexEnable].Value = false;
                selected = null;
            }
            else if (ip == null)
                return;

            if (ip != null)
            {
                foreach (DataGridViewRow row in grid.Rows)
                {
                    var ip2 = (IPAddress)row.Cells[indexValue].Value;
                    if (ip2.Equals(ip))
                    {
                        selected = row;
                        row.Cells[indexEnable].Value = true;
                        break;
                    }
                }
            }

            if (inGridView == GridType.Asset)
                rowSelectedPatchServer = selected;
            else
                rowSelectedLoginServer = selected;
        }

        void ServerController_EnabledChanged(object sender, bool e)
        {
            if (this.InvokeRequired)
            {
                try
                {
                    this.Invoke(new MethodInvoker(
                        delegate
                        {
                            checkPatchInterceptEnable.Checked = e;
                        }));
                }
                catch (Exception ex)
                {
                    Util.Logging.Log(ex);
                }

                return;
            }

            checkPatchInterceptEnable.Checked = e;
        }

        void sidebarButton_SelectedChanged(object sender, EventArgs e)
        {
            SidebarButton button = (SidebarButton)sender;
            if (button.Selected)
            {
                if (selectedButton != null)
                {
                    ((Panel)selectedButton.Tag).Visible = false;
                    selectedButton.Selected = false;
                }
                selectedButton = button;
                ((Panel)button.Tag).Visible = true;
            }
        }

        private void sidebarButton_Click(object sender, EventArgs e)
        {
            SidebarButton button = (SidebarButton)sender;
            button.Selected = true;
        }

        private void formSupport_Load(object sender, EventArgs e)
        {

        }

        private void buttonOK_Click(object sender, EventArgs e)
        {
            string args = Settings.GuildWars2.Arguments.Value;
            if (string.IsNullOrEmpty(args))
                args = "";

            if (gridLoginServers.Rows.Count > 0)
            {
                if (rowSelectedLoginServer != null && (bool)rowSelectedLoginServer.Cells[columnLoginServerEnable.Index].Value)
                    args = Util.Args.AddOrReplace(args, "authsrv", "-authsrv " + ((IPAddress)rowSelectedLoginServer.Cells[columnLoginServer.Index].Value).ToString());
                else if (existingLogin != null)
                    args = Util.Args.AddOrReplace(args, "authsrv", "");
            }

            if (gridPatchServers.Rows.Count > 0)
            {
                if (rowSelectedPatchServer != null && (bool)rowSelectedPatchServer.Cells[columnPatchServerEnable.Index].Value)
                    args = Util.Args.AddOrReplace(args, "assetsrv", "-assetsrv " + ((IPAddress)rowSelectedPatchServer.Cells[columnPatchServer.Index].Value).ToString());
                else if (existingAsset != null)
                    args = Util.Args.AddOrReplace(args, "assetsrv", "");
            }

            Settings.GuildWars2.Arguments.Value = args;

            this.DialogResult = System.Windows.Forms.DialogResult.OK;
        }

        private void buttonCancel_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
        }

        private bool HasActive()
        {
            if (activeProcess != null)
            {
                bool hasExited = false;
                bool hasStarted = false;
                try
                {
                    hasStarted = activeProcess.Id != 0;
                    hasExited = activeProcess.HasExited;
                }
                catch (Exception e)
                {
                    Util.Logging.Log(e);
                }

                if (hasStarted && !hasExited)
                {
                    return true;
                }
            }

            if (Client.Launcher.GetActiveProcessCount(Client.Launcher.AccountType.GuildWars2) > 0)
                return true;

            return false;
        }

        private Process Launch(string args)
        {
            Client.Launcher.CancelPendingLaunches();

            if (HasActive())
            {
                if (MessageBox.Show(this, "All clients will be closed.\n\nAre you sure?", "Close GW2 to continue", MessageBoxButtons.YesNo, MessageBoxIcon.Exclamation) == DialogResult.Yes)
                {
                    Client.Launcher.KillAllActiveProcesses(Client.Launcher.AccountType.GuildWars2);

                    if (activeProcess != null)
                    {
                        try
                        {
                            activeProcess.Kill();
                        }
                        catch (Exception e)
                        {
                            Util.Logging.Log(e);
                        }
                        activeProcess = null;
                    }
                }
                else
                    return null;
            }

            FileInfo fi = null;

            if (Settings.GuildWars2.Path.HasValue && !string.IsNullOrEmpty(Settings.GuildWars2.Path.Value))
            {
                try
                {
                    fi = new FileInfo(Settings.GuildWars2.Path.Value);
                }
                catch (Exception e)
                {
                    Util.Logging.Log(e);
                }
            }

            while (fi == null || !fi.Exists)
            {
                OpenFileDialog f = new OpenFileDialog();

                f.Filter = "Guild Wars 2|Gw2*.exe|All executables|*.exe";
                f.Title = "Open Gw2.exe";

                if (fi != null)
                {
                    try
                    {
                        f.InitialDirectory = fi.DirectoryName;
                    }
                    catch (Exception ex)
                    {
                        Util.Logging.Log(ex);
                    }
                }

                if (f.ShowDialog(this) == System.Windows.Forms.DialogResult.OK)
                {
                    try
                    {
                        fi = new FileInfo(f.FileName);
                        Settings.GuildWars2.Path.Value = f.FileName;
                    }
                    catch (Exception e)
                    {
                        Util.Logging.Log(e);
                    }
                }
                else
                {
                    return null;
                }
            }

            try
            {
                Process p = activeProcess = new Process();
                p.EnableRaisingEvents = true;
                p.StartInfo.FileName = fi.FullName;
                p.StartInfo.WorkingDirectory = fi.Directory.FullName;
                p.StartInfo.Arguments = args;

                if (p.Start())
                {
                    return p;
                }
            }
            catch(Exception e)
            {
                Util.Logging.Log(e);
            }

            return null;
        }

        private Process Launch(Button sender, string args)
        {
            sender.Enabled = false;

            var p = Launch(args);
            if (p == null)
            {
                sender.Enabled = true;
            }
            else
            {
                try
                {
                    p.Exited += delegate
                    {
                        this.Invoke(new MethodInvoker(
                            delegate
                            {
                                sender.Enabled = true;
                            }));
                        p.Dispose();
                    };
                    sender.Enabled = p.HasExited;
                }
                catch (Exception ex)
                {
                    Util.Logging.Log(ex);
                }
            }

            return p;
        }

        private void buttonLaunchDiag_Click(object sender, EventArgs e)
        {
            Launch(buttonLaunchDiag, "-diag");
        }

        private async void buttonLaunchLog_Click(object sender, EventArgs e)
        {
            var p = Launch(buttonLaunchLog, "-log");

            if (p == null)
                return;

            do
            {
                await Task.Delay(5000);

                try
                {
                    if (p.Id != 0)
                    {
                        FileInfo fi = new FileInfo(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Guild Wars 2", "Gw2.log"));
                        if (fi.Exists)
                        {
                            labelLoggingShowLog.Visible = true;
                            return;
                        }
                        else if (p.HasExited)
                            return;
                    }
                    else
                        return;
                }
                catch (Exception ex)
                {
                    Util.Logging.Log(ex);
                    return;
                }
            }
            while (true);
        }

        private void labelLoggingShowLog_Click(object sender, EventArgs e)
        {
            FileInfo fi;
            try
            {
                fi = new FileInfo(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Guild Wars 2", "Gw2.log"));
            }
            catch (Exception ex)
            {
                fi = null;
                Util.Logging.Log(ex);
            }

            if (fi == null || !fi.Exists)
            {
                labelLoggingShowLog.Visible = false;
                MessageBox.Show(this, "Gw2.log could not be located", "Gw2.log not found", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                return;
            }

            Util.Explorer.OpenFolderAndSelect(fi.FullName);
        }

        private void buttonClean_Click(object sender, EventArgs e)
        {
            if (!Settings.GuildWars2.Path.HasValue || string.IsNullOrEmpty(Settings.GuildWars2.Path.Value))
                return;

            DirectoryInfo di;
            try
            {
                var fi = new FileInfo(Settings.GuildWars2.Path.Value);
                if (fi.Exists)
                    di = fi.Directory;
                else
                    di = null;
            }
            catch (Exception ex)
            {
                Util.Logging.Log(ex);
                return;
            }

            if (di == null || !di.Exists)
                return;

            List<FileInfo> files = new List<FileInfo>();
            try
            {
                foreach (var file in di.GetFiles())
                {
                    var ext = file.Extension.ToLower();
                    if (checkCleanEverything.Checked)
                    {
                        if (ext == ".exe" || ext == ".dat")
                            continue;
                        files.Add(file);
                    }
                    else if (ext == ".tmp" || ext == ".dmp" || ext == ".log")
                        files.Add(file);
                }
            }
            catch (Exception ex)
            {
                Util.Logging.Log(ex);
            }

            if (checkCleanBin.Checked || checkCleanEverything.Checked)
            {
                foreach (var bin in new string[] { "bin", "bin64" })
                {
                    try
                    {
                        files.AddRange(new DirectoryInfo(Path.Combine(di.FullName, bin)).GetFiles());
                    }
                    catch (Exception ex)
                    {
                        Util.Logging.Log(ex);
                    }
                }
            }

            using (var f = new formCleanup(files))
            {
                if (f.ShowDialog(this) != System.Windows.Forms.DialogResult.OK)
                    return;
            }

            int deleted = 0, 
                failed = 0;

            foreach (var file in files)
            {
                try
                {
                    if (file.Exists)
                    {
                        file.Delete();
                        deleted++;
                    }
                }
                catch (Exception ex)
                {
                    Util.Logging.Log(ex);
                    failed++;
                }
            }
        }

        private void checkCleanBin_CheckedChanged(object sender, EventArgs e)
        {

        }

        private void checkCleanEverything_CheckedChanged(object sender, EventArgs e)
        {
            checkCleanBin.Enabled = !checkCleanBin.Enabled;
        }

        private void buttonReadTest_Click(object sender, EventArgs e)
        {
            try
            {
                var fi = new FileInfo(Settings.GuildWars2.Path.Value);
                if (fi.Exists)
                {
                    var path = Path.Combine(fi.Directory.FullName, "Gw2.dat");
                    fi = new FileInfo(path);

                    if (fi.Exists)
                    {
                        using (formFileScan f = new formFileScan(fi))
                        {
                            f.ShowDialog(this);
                            return;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Util.Logging.Log(ex);
            }

            MessageBox.Show(this, "Gw2.dat could not be located", "Gw2.dat not found", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
        }

        private void labelPatchServerDns_Click(object sender, EventArgs e)
        {
            using (formDnsDialog f = new formDnsDialog((List<IPAddress>)buttonPatchServerLookup.Tag))
            {
                if (f.ShowDialog(this) == DialogResult.OK)
                {
                    buttonPatchServerLookup.Tag = f.IPs;
                }
            }
        }

        private void buttonPatchServerLookup_Click(object sender, EventArgs e)
        {
            buttonPatchServerLookup.Enabled = false;
            gridPatchServers.Visible = false;
            arrowAssetExpand.Visible = false;
            copyAsset.Visible = false;
            labelPatchServerNoIPs.Visible = false;
            progressPatchServer.Visible = false;
            gridPatchServers.Rows.Clear();

            var ips = buttonPatchServerLookup.Tag as List<IPAddress>;
            if (ips == null)
                ips = Net.DnsServers.GetIPs();

            LookupPatchServers(ips, cancelToken.Token);
        }

        private HashSet<IPAddress> GetIPs(string address, IEnumerable<IPAddress> dns)
        {
            return Net.Dns.GetHostAddresses(address, dns);
        }

        private int DoHttpAssetPing(IPAddress ip)
        {
            HttpWebRequest request = HttpWebRequest.CreateHttp(string.Format("http://{0}/latest64/101", ip.ToString()));
            request.Host = PATCH_SERVER;
            request.AllowAutoRedirect = false;
            request.Proxy = null;
            request.Timeout = 3000;

            //CDNetworks servers will reject the request if the md5 isn't supplied
            request.Headers.Add(HttpRequestHeader.Cookie, Settings.ASSET_COOKIE);

            var startTime = DateTime.UtcNow;

            try
            {
                using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
                {
                    using (StreamReader reader = new StreamReader(response.GetResponseStream()))
                    {
                        var data = reader.ReadToEnd();
                    }
                }

                return (int)(DateTime.UtcNow.Subtract(startTime).TotalMilliseconds + 0.5);
            }
            catch (Exception e)
            {
                if (e is WebException)
                    using (((WebException)e).Response) { }
                Util.Logging.Log(e);
                return -1;
            }
        }

        private int DoHttpLoginPing(IPAddress ip)
        {
            //Login servers running from Amazon's cloud don't support pings
            //Original ArenaNet authserver: 64.25.38.54

            HttpWebRequest request = HttpWebRequest.CreateHttp(string.Format("http://{0}/", ip.ToString()));
            request.Host = PATCH_SERVER;
            request.AllowAutoRedirect = false;
            request.Proxy = null;
            request.Timeout = 3000;

            var startTime = DateTime.UtcNow;

            HttpWebResponse response;
            try
            {
                response = (HttpWebResponse)request.GetResponse();
            }
            catch (WebException e)
            {
                response = e.Response as HttpWebResponse;
                if (response == null)
                    return -1;
            }
            catch (Exception e)
            {
                Util.Logging.Log(e);
                return -1;
            }

            try
            {
                using (response)
                {
                    using (StreamReader reader = new StreamReader(response.GetResponseStream()))
                    {
                        var data = reader.ReadToEnd();
                    }
                }

                return (int)(DateTime.UtcNow.Subtract(startTime).TotalMilliseconds + 0.5);
            }
            catch (Exception e)
            {
                Util.Logging.Log(e);
                return -1;
            }
        }

        private int DoTcpLoginPing(IPAddress ip)
        {
            try
            {
                using (var socket = new System.Net.Sockets.Socket(System.Net.Sockets.AddressFamily.InterNetwork, System.Net.Sockets.SocketType.Stream, System.Net.Sockets.ProtocolType.Tcp))
                {
                    var startTime = DateTime.UtcNow;
                    var ia = socket.BeginConnect(ip, 80, null, null);
                    ia.AsyncWaitHandle.WaitOne(3000);

                    var elapsed = (int)(DateTime.UtcNow.Subtract(startTime).TotalMilliseconds + 0.5);
                    if (!socket.Connected)
                        elapsed = -1;

                    socket.Close();
                    return elapsed;
                }
            }
            catch (Exception e)
            {
                Util.Logging.Log(e);
                return -1;
            }
        }

        private int DoPing(IPAddress ip, byte[] data, PingOptions options)
        {
            long total = 0;
            int count = 0;

            for (var i = 0; i < 3; i++)
            {
                try
                {
                    Ping ping = new Ping();
                    PingReply reply = ping.Send(ip, 1000, data, options);
                    if (reply.Status == IPStatus.Success)
                    {
                        total += reply.RoundtripTime;
                        count++;
                    }
                    else if (i > 0 && count == 0)
                        break;
                }
                catch (Exception e)
                {
                    Util.Logging.Log(e);
                }
            }

            if (count == 0)
                return -1;

            return (int)((double)total / count + 0.5);
        }

        private async void LookupLoginServers(IEnumerable<IPAddress> dns, CancellationToken cancel)
        {
            progressLoginServer.Maximum = 1;
            progressLoginServer.Value = 0;
            progressLoginServer.Visible = true;

            var na = await Task.Run<HashSet<IPAddress>>(
                delegate
                {
                    return GetIPs(AUTH_NA_SERVER, dns);
                });

            var eu = await Task.Run<HashSet<IPAddress>>(
                delegate
                {
                    return GetIPs(AUTH_EU_SERVER, dns);
                });

            if (cancel.IsCancellationRequested)
                return;

            ////the original ArenaNet login servers, which may eventually stop working - GW2 switched to using Amazon -- original servers taken down on November 9, 2017
            //foreach (var ip in new string[] { "64.25.38.54", "64.25.38.171", "64.25.38.185", "64.25.38.51", "64.25.38.172" })
            //    na.Add(IPAddress.Parse(ip));
            //foreach (var ip in new string[] { "206.127.159.109", "206.127.146.73", "206.127.159.107", "206.127.146.67", "206.127.146.74" })
            //    eu.Add(IPAddress.Parse(ip));

            IPAddress selected;
            if (rowSelectedLoginServer != null)
            {
                selected = (IPAddress)rowSelectedLoginServer.Cells[columnLoginServer.Index].Value;
                rowSelectedLoginServer = null;
            }
            else
                selected = existingLogin;

            int ipCount = na.Count + eu.Count;
            if (ipCount > 0)
            {
                progressLoginServer.Maximum = ipCount;
                progressLoginServer.Value = 0;

                DataGridViewRow[] rows = new DataGridViewRow[ipCount];
                int r = 0;
                Ping ping = new Ping();
                PingOptions options = new PingOptions();
                byte[] pingData = new byte[0];

                for (var n = 0; n < 2; n++)
                {
                    HashSet<IPAddress> ips = n == 0 ? na : eu;

                    foreach (var ip in ips)
                    {
                        var row = rows[r++] = (DataGridViewRow)gridLoginServers.RowTemplate.Clone();
                        row.CreateCells(gridLoginServers);

                        bool isSelected = selected != null && ip.Equals(selected);
                        if (isSelected)
                        {
                            if (rowSelectedLoginServer != null)
                                rowSelectedLoginServer.Cells[columnLoginServerEnable.Index].Value = false;
                            rowSelectedLoginServer = row;
                        }

                        row.Cells[columnLoginServer.Index].Value = ip;
                        row.Cells[columnLoginServerEnable.Index].Value = isSelected;
                        row.Cells[columnLoginServerRegion.Index].Value = n == 0 ? "NA" : "EU";

                        var c = row.Cells[columnLoginServerPing.Index];
                        c.Style.ForeColor = Color.Gray;
                        c.Value = "...";

                        c = row.Cells[columnLoginServerResponseTime.Index];
                        c.Style.ForeColor = Color.Gray;
                        c.Value = "...";
                    }
                }

                gridLoginServers.Rows.AddRange(rows);
                gridLoginServers.Visible = true;

                Func<IPAddress, DataGridViewRow, bool, TaskPingResult> doPing =
                    delegate(IPAddress ip, DataGridViewRow row, bool doPings)
                    {
                        return new TaskPingResult()
                        {
                            ping = doPings ? DoPing(ip, pingData, options) : -1,
                            http = DoTcpLoginPing(ip),
                            row = row
                        };
                    };

                int limit = LOOKUP_CONNECTION_LIMIT;
                if (ipCount < limit)
                    limit = ipCount;
                int queued = 0;
                int i = 0;
                int failedPings = 0;

                Task<TaskPingResult>[] tasks = new Task<TaskPingResult>[limit];

                while (!cancel.IsCancellationRequested)
                {
                    int k = await Task.Run<int>(
                        delegate
                        {
                            while (true)
                            {
                                int nulls = 0;
                                for (var j = 0; j < limit; j++)
                                {
                                    var task = tasks[j];
                                    try
                                    {
                                        if (task == null)
                                        {
                                            if (++nulls == limit)
                                                return -1;
                                        }
                                        else if (task.Wait(50, cancel))
                                        {
                                            return j;
                                        }
                                    }
                                    catch (OperationCanceledException)
                                    {
                                        return -1;
                                    }
                                }
                            }
                        });

                    if (cancel.IsCancellationRequested)
                        break;

                    if (k != -1)
                    {
                        queued--;

                        var t = tasks[k];
                        var result = t.Result;

                        if (result.http >= 0)
                        {
                            var c = result.row.Cells[columnLoginServerResponseTime.Index];
                            c.Value = new MillisCell(result.http);
                            c.Style.ForeColor = Color.Empty;
                        }
                        else
                        {
                            var c = result.row.Cells[columnLoginServerResponseTime.Index];
                            c.Value = "failed";
                            c.Style.ForeColor = Color.Maroon;
                        }

                        if (result.ping >= 0)
                        {
                            var c = result.row.Cells[columnLoginServerPing.Index];
                            c.Value = new MillisCell(result.ping);
                            c.Style.ForeColor = Color.Empty;
                            failedPings = -1;
                        }
                        else
                        {
                            var c = result.row.Cells[columnLoginServerPing.Index];
                            c.Value = result.http >= 0 ? "---" : "failed";
                            c.Style.ForeColor = Color.Maroon;
                            if (failedPings != -1 && result.http >= 0)
                                failedPings++;
                        }

                        SortRow(gridLoginServers, result.row, RowComparisonLogin, gridLoginServers.SortOrder);

                        progressLoginServer.Value++;
                    }
                    else
                    {
                        if (i == ipCount)
                            break;

                        k = 0;
                    }

                    if (i < ipCount)
                    {
                        while (i < ipCount && queued < limit)
                        {
                            queued++;
                            var row = rows[i++];
                            var ip = (IPAddress)row.Cells[columnLoginServer.Index].Value;

                            var t = tasks[k++] = new Task<TaskPingResult>(
                                delegate
                                {
                                    return doPing(ip, row, failedPings <= 3);
                                });
                            t.Start();
                        }
                    }
                    else
                    {
                        tasks[k] = null;
                    }
                }

                arrowLoginExpand.Visible = true;
                copyLogin.Visible = true;
            }
            else
            {
                labelLoginServerNoIPs.Visible = true;
            }

            progressLoginServer.Visible = false;
            buttonLoginServerLookup.Enabled = true;
        }

        private async void LookupPatchServers(IEnumerable<IPAddress> dns, CancellationToken cancel)
        {
            HashSet<IPAddress> ips;
            
            progressPatchServer.Maximum = 1;
            progressPatchServer.Value = 0;
            progressPatchServer.Visible = true;

            ips = await Task.Run<HashSet<IPAddress>>(
                delegate
                {
                    return GetIPs(PATCH_SERVER, dns);
                });

            if (cancel.IsCancellationRequested)
                return;

            if (existingAsset != null)
                ips.Add(existingAsset);

            IPAddress selected;
            if (rowSelectedPatchServer != null)
            {
                selected = (IPAddress)rowSelectedPatchServer.Cells[columnPatchServer.Index].Value;
                rowSelectedPatchServer = null;
            }
            else
                selected = existingAsset;

            int ipCount = ips.Count;
            if (ipCount > 0)
            {
                progressPatchServer.Maximum = ipCount;
                progressPatchServer.Value = 0;

                DataGridViewRow[] rows = new DataGridViewRow[ipCount];
                int r = 0;
                PingOptions options = new PingOptions();
                byte[] pingData = new byte[0];

                foreach (var ip in ips)
                {
                    var row = rows[r++] = (DataGridViewRow)gridPatchServers.RowTemplate.Clone();
                    row.CreateCells(gridPatchServers);

                    bool isSelected = selected != null && ip.Equals(selected);
                    if (isSelected)
                    {
                        if (rowSelectedPatchServer != null)
                            rowSelectedPatchServer.Cells[columnPatchServerEnable.Index].Value = false;
                        rowSelectedPatchServer = row;
                    }

                    row.Cells[columnPatchServer.Index].Value = ip;
                    row.Cells[columnPatchServerEnable.Index].Value = isSelected;

                    DataGridViewCell c;
                    c = row.Cells[columnPatchServerResponseTime.Index];
                    c.Style.ForeColor = Color.Gray;
                    c.Value = "...";

                    c = row.Cells[columnPatchServerPing.Index];
                    c.Style.ForeColor = Color.Gray;
                    c.Value = "...";
                }

                gridPatchServers.Rows.AddRange(rows);
                gridPatchServers.Visible = true;

                Func<IPAddress, DataGridViewRow, bool, TaskPingResult> doPing =
                    delegate(IPAddress ip, DataGridViewRow row, bool doPings)
                    {
                        return new TaskPingResult()
                        {
                            ping = doPings ? DoPing(ip, pingData, options) : -1,
                            http = DoHttpAssetPing(ip),
                            row = row
                        };
                    };

                int limit = LOOKUP_CONNECTION_LIMIT;
                if (ipCount < limit)
                    limit = ipCount;
                int queued = 0;
                int i = 0;
                int failedPings = 0;

                Task<TaskPingResult>[] tasks = new Task<TaskPingResult>[limit];

                while (!cancel.IsCancellationRequested)
                {
                    int k = await Task.Run<int>(
                        delegate
                        {
                            while (true)
                            {
                                int nulls = 0;
                                for (var j = 0; j < limit; j++)
                                {
                                    var task = tasks[j];
                                    try
                                    {
                                        if (task == null)
                                        {
                                            if (++nulls == limit)
                                                return -1;
                                        }
                                        else if (task.Wait(50, cancel))
                                        {
                                            return j;
                                        }
                                    }
                                    catch (OperationCanceledException)
                                    {
                                        return -1;
                                    }
                                }
                            }
                        });

                    if (cancel.IsCancellationRequested)
                        break;

                    if (k != -1)
                    {
                        queued--;

                        var t = tasks[k];
                        var result = t.Result;

                        if (result.http >= 0)
                        {
                            var c = result.row.Cells[columnPatchServerResponseTime.Index];
                            c.Value = new MillisCell(result.http);
                            c.Style.ForeColor = Color.Empty;
                        }
                        else
                        {
                            var c = result.row.Cells[columnPatchServerResponseTime.Index];
                            c.Value = "failed";
                            c.Style.ForeColor = Color.Maroon;
                        }

                        if (result.ping >= 0)
                        {
                            var c = result.row.Cells[columnPatchServerPing.Index];
                            c.Value = new MillisCell(result.ping);
                            c.Style.ForeColor = Color.Empty;
                            failedPings = -1;
                        }
                        else
                        {
                            var c = result.row.Cells[columnPatchServerPing.Index];
                            c.Value = result.http >= 0 ? "---" : "failed";
                            c.Style.ForeColor = Color.Maroon;
                            if (failedPings != -1 && result.http >= 0)
                                failedPings++;
                        }

                        SortRow(gridPatchServers, result.row, RowComparisonPatch, gridPatchServers.SortOrder);

                        progressPatchServer.Value++;
                    }
                    else
                    {
                        if (i == ipCount)
                            break;

                        k = 0;
                    }

                    if (i < ipCount)
                    {
                        while (i < ipCount && queued < limit)
                        {
                            queued++;
                            var row = rows[i++];
                            var ip = (IPAddress)row.Cells[columnPatchServer.Index].Value;

                            var t = tasks[k++] = new Task<TaskPingResult>(
                                delegate
                                {
                                    return doPing(ip, row, failedPings <= 3);
                                });
                            t.Start();
                        }
                    }
                    else
                    {
                        tasks[k] = null;
                    }
                }

                arrowAssetExpand.Visible = true;
                copyAsset.Visible = true;
            }
            else
            {
                labelPatchServerNoIPs.Visible = true;
            }

            progressPatchServer.Visible = false;
            buttonPatchServerLookup.Enabled = true;
        }

        private int RowComparisonPatch(DataGridViewRow a, DataGridViewRow b)
        {
            var c = gridPatchServers.SortedColumn;
            if (c == null)
                c = columnPatchServerResponseTime;

            int result;
            var c1 = a.Cells[c.Index].Value as IComparable;
            var v2 = b.Cells[c.Index].Value;

            if (c1 != null)
            {
                if (c1.GetType() == v2.GetType())
                    result = c1.CompareTo(v2);
                else if (c1 is MillisCell)
                    result = -1;
                else if (v2 is MillisCell)
                    result = 1;
                else
                    result = 0;
            }
            else
                result = 0;

            if (c == columnPatchServerResponseTime && result == 0 && c1 is MillisCell)
                result = ((IComparable)a.Cells[columnPatchServerPing.Index].Value).CompareTo(b.Cells[columnPatchServerPing.Index].Value);

            return result;
        }

        private int RowComparisonLogin(DataGridViewRow a, DataGridViewRow b)
        {

            var c = gridLoginServers.SortedColumn;
            if (c == null)
                c = columnLoginServerResponseTime;

            int result;
            var c1 = a.Cells[c.Index].Value as IComparable;
            var v2 = b.Cells[c.Index].Value;

            if (c1 != null)
            {
                if (c1.GetType() == v2.GetType())
                    result = c1.CompareTo(v2);
                else if (c1 is MillisCell)
                    result = -1;
                else if (v2 is MillisCell)
                    result = 1;
                else
                    result = 0;
            }
            else
                result = 0;

            if (c == columnLoginServerResponseTime && result == 0 && c1 is MillisCell)
                result = ((IComparable)a.Cells[columnLoginServerPing.Index].Value).CompareTo(b.Cells[columnLoginServerPing.Index].Value);

            return result;


            //var c = gridLoginServers.SortedColumn;
            //if (c == null)
            //    c = columnLoginServerPing;

            //int result;
            //var c1 = a.Cells[c.Index].Value as IComparable;
            //var v2 = b.Cells[c.Index].Value;

            //if (c1 != null)
            //{
            //    if (c1.GetType() == v2.GetType())
            //        result = c1.CompareTo(v2);
            //    else if (c1 is string)
            //        result = 1;
            //    else if (v2 is string)
            //        result = -1;
            //    else
            //        result = 0;
            //}
            //else
            //    result = 0;

            //return result;
        }

        private void gridPatchServers_SortCompare(object sender, DataGridViewSortCompareEventArgs e)
        {
            if (e.Column == columnPatchServerPing || e.Column == columnPatchServerResponseTime)
            {
                e.Handled = true;

                e.SortResult = RowComparisonPatch(gridPatchServers.Rows[e.RowIndex1], gridPatchServers.Rows[e.RowIndex2]);
            }
        }

        private void gridPatchServers_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0)
            {
                var row =gridPatchServers.Rows[e.RowIndex];
                var c = row.Cells[columnPatchServerEnable.Index];
                bool v;
                c.Value = v = !(bool)c.Value;

                if (row != rowSelectedPatchServer)
                {
                    if (v && rowSelectedPatchServer != null)
                        rowSelectedPatchServer.Cells[columnPatchServerEnable.Index].Value = false;

                    rowSelectedPatchServer = row;
                }
                else if (!v)
                {
                    rowSelectedPatchServer = null;
                }

                if (inGridView == GridType.Asset)
                {
                    var s = Settings.GuildWars2.Arguments;
                    string args = s.Value;
                    if (args == null)
                        args = "";

                    if (rowSelectedPatchServer != null && (bool)rowSelectedPatchServer.Cells[columnPatchServerEnable.Index].Value)
                        args = Util.Args.AddOrReplace(args, "assetsrv", "-assetsrv " + ((IPAddress)rowSelectedPatchServer.Cells[columnPatchServer.Index].Value).ToString());
                    else
                        args = Util.Args.AddOrReplace(args, "assetsrv", "");

                    s.Value = args;
                }
            }
        }

        private void gridPatchServers_SelectionChanged(object sender, EventArgs e)
        {
            gridPatchServers.ClearSelection();
        }

        private List<AccountData<FileInfo>> GetCrashLogs()
        {
            HashSet<string> files = new HashSet<string>();
            List<AccountData<FileInfo>> accounts = new List<AccountData<FileInfo>>();
            FileInfo fi;

            //not checking gw1

            foreach (var gw2 in Util.Accounts.GetGw2Accounts())
            {
                if (gw2.DatFile != null && gw2.DatFile.IsInitialized)
                {
                    try
                    {
                        DirectoryInfo di = new FileInfo(gw2.DatFile.Path).Directory;
                        if (di.Exists)
                        {
                            fi = new FileInfo(Path.Combine(di.FullName, "ArenaNet.log"));
                            if (fi.Exists && files.Add(fi.FullName))
                            {
                                accounts.Add(new AccountData<FileInfo>()
                                {
                                    account = gw2,
                                    data = fi
                                });
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Util.Logging.Log(ex);
                    }
                }
            }

            fi = new FileInfo(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Guild Wars 2", "ArenaNet.log"));
            if (fi.Exists && files.Add(fi.FullName))
            {
                accounts.Add(new AccountData<FileInfo>()
                    {
                        account = null,
                        data = fi
                    });
            }

            return accounts;
        }

        private void labelOpenCrashLogFolder_Click(object sender, EventArgs e)
        {
            var logs = GetCrashLogs();

            if (logs.Count > 0)
            {
                if (logs.Count > 1)
                {
                    Dictionary<ushort, AccountData<FileInfo>> accounts = new Dictionary<ushort, AccountData<FileInfo>>();
                    Settings.IAccount[] _accounts = new Settings.IAccount[logs.Count];
                    int i = 0;

                    foreach (var l in logs)
                    {
                        if (l.account != null)
                            accounts[l.account.UID] = l;
                        else
                            accounts[0] = l;
                        _accounts[i++]=l.account;
                    }
                    using (formAccountSelect f = new formAccountSelect("Multiple logs exist. Which accounts would you like to view?", _accounts, true, true))
                    {
                        if (f.ShowDialog(this) != System.Windows.Forms.DialogResult.OK)
                            return;
                        if (f.Selected.Count == 0)
                            return;
                        logs.Clear();
                        foreach (var account in f.Selected)
                        {
                            ushort id;
                            if (account == null)
                                id = 0;
                            else
                                id = account.UID;
                            logs.Add(accounts[id]);
                        }
                    }
                }

                foreach (var l in logs)
                {
                    Util.Explorer.OpenFolderAndSelect(l.data.FullName);
                }
            }
            else
            {
                var path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Guild Wars 2");
                Util.Explorer.OpenFolder(path);
            }
        }

        private void labelDeleteCrashLog_Click(object sender, EventArgs e)
        {
            var logs = GetCrashLogs();
            int count = 0;
            long size = 0;

            foreach (var log in logs)
            {
                try
                {
                    size += log.data.Length;
                    count++;
                }
                catch (Exception ex)
                {
                    Util.Logging.Log(ex);
                }
            }

            if (count > 0)
            {
                if (MessageBox.Show(this, string.Format("{0} log{1} ({2}) will be deleted.\n\nAre you sure?", count, count != 1 ? "s" : "", Util.Text.FormatBytes(size)), "Confirm deletion", MessageBoxButtons.YesNo, MessageBoxIcon.Exclamation) == System.Windows.Forms.DialogResult.Yes)
                {
                    foreach (var log in logs)
                    {
                        try
                        {
                            log.data.Delete();
                        }
                        catch (Exception ex)
                        {
                            Util.Logging.Log(ex);
                        }
                    }
                }
            }
            else
            {
                MessageBox.Show(this, "Unable to find any crash logs", "Nothing to delete", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        private void buttonLoginServerLookup_Click(object sender, EventArgs e)
        {
            buttonLoginServerLookup.Enabled = false;
            gridLoginServers.Visible = false;
            arrowLoginExpand.Visible = false;
            copyLogin.Visible = false;
            labelLoginServerNoIPs.Visible = false;
            progressLoginServer.Visible = false;
            gridLoginServers.Rows.Clear();

            var ips = buttonPatchServerLookup.Tag as List<IPAddress>;
            if (ips == null)
                ips = Net.DnsServers.GetIPs();

            LookupLoginServers(ips, cancelToken.Token);
        }

        private void gridLoginServers_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0)
            {
                var row = gridLoginServers.Rows[e.RowIndex];
                var c = row.Cells[columnLoginServerEnable.Index];
                bool v;
                c.Value = v = !(bool)c.Value;

                if (row != rowSelectedLoginServer)
                {
                    if (v && rowSelectedLoginServer != null)
                        rowSelectedLoginServer.Cells[columnLoginServerEnable.Index].Value = false;

                    rowSelectedLoginServer = row;
                }
                else if (!v)
                {
                    rowSelectedLoginServer = null;
                }

                if (inGridView == GridType.Login)
                {
                    var s = Settings.GuildWars2.Arguments;
                    string args = s.Value;
                    if (args == null)
                        args = "";

                    if (rowSelectedLoginServer != null && (bool)rowSelectedLoginServer.Cells[columnLoginServerEnable.Index].Value)
                        args = Util.Args.AddOrReplace(args, "authsrv", "-authsrv " + ((IPAddress)rowSelectedLoginServer.Cells[columnLoginServer.Index].Value).ToString());
                    else
                        args = Util.Args.AddOrReplace(args, "authsrv", "");

                    s.Value = args;
                }
            }
        }

        private void gridLoginServers_SelectionChanged(object sender, EventArgs e)
        {
            gridLoginServers.ClearSelection();
        }

        private void gridLoginServers_SortCompare(object sender, DataGridViewSortCompareEventArgs e)
        {
            if (e.Column == columnLoginServerPing)
            {
                e.Handled = true;

                e.SortResult = RowComparisonLogin(gridLoginServers.Rows[e.RowIndex1], gridLoginServers.Rows[e.RowIndex2]);
            }
        }

        private void formSupport_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (activeProcess != null)
            {
                bool hasStarted = false;
                bool hasExited = false;

                try
                {
                    hasStarted = activeProcess.Id != 0;
                    hasExited = activeProcess.HasExited;
                }
                catch (Exception ex)
                {
                    Util.Logging.Log(ex);
                }

                if (hasStarted && !hasExited)
                {
                    if (MessageBox.Show(this, "A process is still active and will also be closed.\n\nAre you sure?", "Process is still active", MessageBoxButtons.YesNo, MessageBoxIcon.Exclamation) != System.Windows.Forms.DialogResult.Yes)
                    {
                        e.Cancel = true;
                        return;
                    }

                    try
                    {
                        activeProcess.Kill();
                    }
                    catch (Exception ex)
                    {
                        Util.Logging.Log(ex);
                    }
                }

                activeProcess.Dispose();
                activeProcess = null;
            }

            cancelToken.Cancel();
        }

        private void buttonLaunchRepair_Click(object sender, EventArgs e)
        {
            Launch(buttonLaunchRepair, "-repair");
        }

        private void SortRow(DataGridView grid, DataGridViewRow row, Comparison<DataGridViewRow> comparison, SortOrder order)
        {
            grid.Rows.Remove(row);

            int from, 
                to = grid.Rows.Count - 1;

            Func<DataGridViewRow, DataGridViewRow, int> doComparison =
                delegate(DataGridViewRow a, DataGridViewRow b)
                {
                    var _c = comparison(a, b);
                    if (order == SortOrder.Descending)
                        _c = -_c;
                    return _c;
                };

            if (to == -1)
            {
                grid.Rows.Add(row);
            }
            else if (to == 0)
            {
                var c = doComparison(row, grid.Rows[0]);

                if (c < 0)
                    grid.Rows.Insert(0, row);
                else
                    grid.Rows.Add(row);
            }
            else
            {
                from = 0;

                while (true)
                {
                    int index = (to - from) / 2 + from;
                    int c;

                    DataGridViewRow mid = grid.Rows[index];

                    c = doComparison(row, mid);

                    if (c == 0)
                    {
                        grid.Rows.Insert(index, row);
                        return;
                    }
                    else if ((from == index && c < 0) || (to == index && c > 0))
                    {
                        if (c < 0)
                            grid.Rows.Insert(index, row);
                        else
                            grid.Rows.Insert(index + 1, row);
                        return;
                    }
                    else if (c > 0)
                        from = index + 1;
                    else if (c < 0)
                        to = index - 1;
                }
            }
        }

        private void progressPatchServer_VisibleChanged(object sender, EventArgs e)
        {
            labelPatchServerDns.Visible = !progressPatchServer.Visible;
        }

        private void checkPatchInterceptEnable_CheckedChanged(object sender, EventArgs e)
        {
            Net.AssetProxy.ServerController.Enabled = checkPatchInterceptEnable.Checked;
        }

        private void labelPatchInterceptShowServer_Click(object sender, EventArgs e)
        {
            var parent = this.Owner as formMain;
            if (parent != null)
            {
                parent.ShowPatchProxy();
            }
        }

        private void labelLoginServerDns_Click(object sender, EventArgs e)
        {
            using (formDnsDialog f = new formDnsDialog((List<IPAddress>)buttonPatchServerLookup.Tag))
            {
                if (f.ShowDialog(this) == DialogResult.OK)
                {
                    buttonPatchServerLookup.Tag = f.IPs;
                }
            }
        }

        private void progressLoginServer_VisibleChanged(object sender, EventArgs e)
        {
            labelLoginServerDns.Visible = !progressLoginServer.Visible;
        }

        private void arrowLoginExpand_Click(object sender, EventArgs e)
        {
            var _f = arrowLoginExpand.Tag as formSupport;
            if (_f != null && !_f.IsDisposed)
                _f.Close();
            
            formMain owner = this.Owner as formMain;
            if (owner != null)
            {
                owner.Invoke(new MethodInvoker(
                    delegate
                    {
                        formSupport f = new formSupport(GridType.Login, gridLoginServers, this);
                        owner.AddWindow(f).Show(owner);
                        arrowLoginExpand.Tag = f;
                    }));
            }
        }

        private void arrowAssetExpand_Click(object sender, EventArgs e)
        {
            var _f = arrowAssetExpand.Tag as formSupport;
            if (_f != null && !_f.IsDisposed)
                _f.Close();

            formMain owner = this.Owner as formMain;
            if (owner != null)
            {
                owner.Invoke(new MethodInvoker(
                    delegate
                    {
                        formSupport f = new formSupport(GridType.Asset, gridPatchServers, this);
                        owner.AddWindow(f).Show(owner);
                        arrowAssetExpand.Tag = f;
                    }));
            }
        }

        private string GridToString(DataGridView grid)
        {
            StringBuilder sb = new StringBuilder();

            foreach (DataGridViewRow row in grid.Rows)
            {
                foreach (DataGridViewCell cell in row.Cells)
                {
                    if (cell is DataGridViewCheckBoxCell || cell.Value == null)
                        continue;
                    sb.Append(cell.Value.ToString());
                    sb.Append('\t');
                }
                sb.Length--;
                sb.AppendLine();
            }

            return sb.ToString();
        }

        private void copyAsset_Click(object sender, EventArgs e)
        {
            Clipboard.SetData(DataFormats.Text, GridToString(gridPatchServers));
        }

        private void copyLogin_Click(object sender, EventArgs e)
        {
            Clipboard.SetData(DataFormats.Text, GridToString(gridLoginServers));
        }
    }
}
