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
using System.Drawing.Imaging;

namespace Gw2Launcher.UI.QuickStart
{
    public partial class formQuickStart : Base.BaseForm
    {
        private class Account
        {
            private TextBox text;

            public Account(TextBox text)
            {
                this.text = text;
            }

            public string Name
            {
                get
                {
                    return text.Text;
                }
                set
                {
                    text.Text = value;
                }
            }

            public string Email
            {
                get;
                private set;
            }

            public System.Security.SecureString Password
            {
                get;
                private set;
            }

            public void SetLogin(string email, System.Security.SecureString password)
            {
                if (!string.IsNullOrEmpty(email) && password != null && password.Length > 0)
                {
                    if (!object.ReferenceEquals(password, Password))
                        using (Password) { }
                    Email = email;
                    Password = password;
                }
                else
                {
                    using (Password) { }
                    Email = null;
                    Password = null;
                }
            }

            public bool HasCredentials
            {
                get
                {
                    return !string.IsNullOrEmpty(Email) && Password != null && Password.Length > 0;
                }
            }
        }

        private class PanelItem
        {
            public PanelItem(Panel panel)
            {
                this.panel = panel;
            }

            public Panel panel;
            public Func<bool> isAvailable;
        }

        private PanelItem[] panels;
        private int current;

        public formQuickStart()
        {
            InitializeComponents();

            panels = new PanelItem[]
            {
                new PanelItem(panelStart),
                new PanelItem(panelAccounts),
                new PanelItem(panelExe),
                new PanelItem(panelAddons),
                new PanelItem(panelDirectUpdates),
                new PanelItem(panelGraphics),
                new PanelItem(panelLogin),

                new PanelItem(panelReady),
            };

            labelGw2ExeTitle.Text = string.Format(labelGw2ExeTitle.Text, Environment.Is64BitProcess ? "Gw2-64.exe" : "Gw2.exe");

            panelDirectUpdates.Enabled = BitConverter.IsLittleEndian;

            if (Settings.GuildWars2.Path.HasValue && File.Exists(Settings.GuildWars2.Path.Value))
                panelAddons.Enabled = (Client.FileManager.IsPathSupported(Path.GetDirectoryName(Settings.GuildWars2.Path.Value), false) & Client.FileManager.PathSupportType.Files) != 0;
            else
                panelAddons.Enabled = Client.FileManager.IsDataLinkingSupported;

            if (Settings.GuildWars2.DatUpdaterEnabled.Value)
                radioDirectUpdateYes.Checked = true;

            if (Settings.GuildWars2.LocalizeAccountExecution.HasValue)
            {
                var v = Settings.GuildWars2.LocalizeAccountExecution.Value;

                if (v.HasFlag(Settings.LocalizeAccountExecutionOptions.Enabled))
                {
                    if (v.HasFlag(Settings.LocalizeAccountExecutionOptions.AutoSync))
                        radioAddonsAuto.Checked = true;
                    else if (v.HasFlag(Settings.LocalizeAccountExecutionOptions.ExcludeUnknownFiles))
                        radioAddonsManual.Checked = true;
                }
            }

            if (Settings.GuildWars2.Path.HasValue && File.Exists(Settings.GuildWars2.Path.Value))
            {
                textGw2Path.Text = Settings.GuildWars2.Path.Value;
            }
            else
            {
                var path = GetDefaultGw2Path();
                if (path != null)
                    textGw2Path.Text = path;
            }

            SetPanel(0);
        }

        protected override void OnInitializeComponents()
        {
            base.OnInitializeComponents();

            InitializeComponent();
        }

        private int GetNextPanel()
        {
            for (int i = current + 1, count = panels.Length; i < count; ++i)
            {
                if (panels[i].panel.Enabled)
                {
                    if (panels[i].isAvailable == null || panels[i].isAvailable())
                        return i;
                }
            }
            return -1;
        }

        private int GetPreviousPanel()
        {
            for (int i = current - 1; i >= 0; --i)
            {
                if (panels[i].panel.Enabled)
                {
                    if (panels[i].isAvailable == null || panels[i].isAvailable())
                        return i;
                }
            }
            return -1;
        }

        private void SetPanel(int i)
        {
            if (i < 0 || i >= panels.Length)
                return;

            this.SuspendLayout();

            panels[i].panel.Visible = true;
            if (current != i)
                panels[current].panel.Visible = false;

            var change = i - current;
            current = i;

            var hasNext = GetNextPanel() != -1;
            buttonNext.Visible = hasNext;
            buttonOK.Visible = !hasNext;
            buttonBack.Visible = i > 0;
            buttonCancel.Visible = i == 0;

            if (i == 0 && change < 0)
            {
                EnableOnLeave(buttonCancel, 1000);
            }
            else if (!hasNext && change > 0)
            {
                EnableOnLeave(buttonOK, 1000);
            }

            this.ResumeLayout();
        }

        private void editToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var a = (Account)((Control)contextAccount.Tag).Parent.Tag;

            using (var f = new formAccount(a.Name, a.Email, a.Password))
            {
                if (f.ShowDialog(this) == System.Windows.Forms.DialogResult.OK)
                {
                    a.Name = f.AccountName;
                    a.SetLogin(f.Email, f.Password);
                }
            }
        }

        private void removeToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var c = ((Control)contextAccount.Tag).Parent;

            panelAccountsContent.SuspendLayout();
            panelAccountsContent.Controls.Remove(c);
            //labelNoAccounts.Visible = !HasAccounts();
            panelAccountsContent.ResumeLayout();

            c.Dispose();
        }

        private bool HasAccounts()
        {
            foreach (Control c in panelAccountsContent.Controls)
            {
                if (c.Tag != null && c.Tag is Account)
                {
                    return true;
                }
            }

            return false;
        }

        private List<Account> GetAccounts()
        {
            var accounts = new List<Account>(panelAccountsContent.Controls.Count - 1);

            foreach (Control c in panelAccountsContent.Controls)
            {
                if (c.Tag != null && c.Tag is Account)
                {
                    var a = (Account)c.Tag;
                    if (!string.IsNullOrEmpty(a.Name))
                        accounts.Add(a);
                }
            }

            return accounts;
        }

        private void buttonNext_Click(object sender, EventArgs e)
        {
            var _current = panels[current].panel;

            if (_current == panelAccounts)
            {
                var accounts = GetAccounts();
                var autologins = 0;
                var manuallogins = 0;

                foreach (var a in accounts)
                {
                    if (a.HasCredentials)
                        ++autologins;
                    else
                        ++manuallogins;
                }

                panelLogin.Enabled = manuallogins > 1;
                panelGraphics.Enabled = accounts.Count > 1;
            }

            SetPanel(GetNextPanel());
        }

        private void buttonBack_Click(object sender, EventArgs e)
        {
            SetPanel(GetPreviousPanel());
        }

        private string CreateTempFile()
        {
            try
            {
                return Path.GetTempFileName();
            }
            catch (Exception ex)
            {
                Util.Logging.Log(ex);
                var path = Util.FileUtil.GetTemporaryFileName(DataPath.AppDataAccountDataTemp);
                if (path == null)
                {
                    path = "";
                }
                return path;
            }
        }

        private byte[] ReadGfxData()
        {
            try
            {
                var f = GetGfxFile();

                if (f != null)
                {
                    return File.ReadAllBytes(f);
                }
            }
            catch { }

            return new byte[0];
        }

        private string GetGfxFile()
        {
            var lw = DateTime.MinValue;
            string fnewest = null;

            foreach (var f in Directory.GetFiles(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Guild Wars 2"), "GFXSettings.*.xml", SearchOption.TopDirectoryOnly))
            {
                var d = File.GetLastWriteTimeUtc(f);
                if (d > lw)
                {
                    lw = d;
                    fnewest = f;
                }
            }

            return fnewest;
        }

        private string GetDefaultGw2Path()
        {
            try
            {
                var f = GetGfxFile();
                string path = null,
                       exe = null;

                if (f != null)
                {
                    using (var r = new StreamReader(new BufferedStream(File.Open(f, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))))
                    {
                        var b = true;

                        while (!r.EndOfStream && b)
                        {
                            var l = r.ReadLine();
                            var i = l.IndexOf('<');
                            if (i == -1)
                                continue;
                            ++i;
                            var j = l.IndexOf(' ', i);
                            if (j == -1)
                                continue;

                            switch (l.Substring(i, j - i))
                            {
                                case "INSTALLPATH":
                                    path = GetValue(l);
                                    b = exe == null;
                                    break;
                                case "EXECUTABLE":
                                    exe = GetValue(l);
                                    b = path == null;
                                    break;
                            }
                        }
                    }
                }

                if (path != null && exe != null)
                {
                    path = Path.Combine(path, exe);
                    if (File.Exists(path))
                        return path;
                }
            }
            catch { }

            return null;
        }

        private string GetValue(string l)
        {
            var i = l.LastIndexOf('"');
            if (i == -1)
                return null;
            var j = l.LastIndexOf('"', i - 1);
            if (j == -1)
                return null;
            ++j;

            return l.Substring(j, i - j);
        }

        private void buttonOK_Click(object sender, EventArgs e)
        {
            const int INDEX_DEFAULT = 0;
            const int INDEX_SHARED = 1;

            var accounts = GetAccounts();
            byte[] gfxfile = null;
            var manualcount = 0;

            var dats = new Settings.IDatFile[2];
            var gfxs = new Settings.IGfxFile[2];
            var checkDat = new bool[] { true, true };
            var checkGfx = new bool[] { true, true };

            if (textGw2Path.TextLength > 0 && File.Exists(textGw2Path.Text))
            {
                try
                {
                    var bits = Util.FileUtil.GetExecutableBits(textGw2Path.Text);

                    if (Environment.Is64BitOperatingSystem && bits == 32)
                    {
                        if (Settings.GuildWars2.Arguments.HasValue)
                            Settings.GuildWars2.Arguments.Value = Util.Args.AddOrReplace(Settings.GuildWars2.Arguments.Value, "32", "-32");
                        else
                            Settings.GuildWars2.Arguments.Value = "-32";
                    }

                    Settings.GuildWars2.Path.Value = textGw2Path.Text;
                }
                catch { }
            }

            checkGfx[INDEX_DEFAULT] = Settings.GuildWars2.Path.HasValue;

            foreach (var a in accounts)
            {
                if (!a.HasCredentials)
                    ++manualcount;
            }

            foreach (var a in accounts)
            {
                var _account = (Settings.IGw2Account)Settings.CreateAccount(Settings.AccountType.GuildWars2);
                var dat = INDEX_DEFAULT;
                var gfx = INDEX_DEFAULT;

                _account.Name = a.Name;

                if (a.HasCredentials)
                {
                    _account.Email = a.Email;
                    _account.Password = Settings.PasswordString.Create(a.Password);
                    _account.AutologinOptions = Settings.AutologinOptions.Login | Settings.AutologinOptions.Play;

                    dat = INDEX_SHARED;
                }
                else if (panelLogin.Enabled)
                {
                    if (radioLoginRemembered.Checked)
                    {
                        _account.AutomaticRememberedLogin = true;

                        dat = -1;
                    }
                }

                if (panelGraphics.Enabled)
                {
                    if (radioGraphicsShared.Checked)
                    {
                        gfx = INDEX_SHARED;
                    }
                    else
                    {
                        gfx = -1;
                    }
                }

                if (dat == -1 || checkDat[dat] && dats[dat] == null)
                {
                    if (dat != -1)
                    {
                        checkDat[dat] = false;
                    }

                    if (dat == INDEX_DEFAULT)
                    {
                        var path = Client.FileManager.GetDefaultPath(Client.FileManager.FileType.Dat);
                        var f = dats[dat] = (Settings.IDatFile)Client.FileManager.FindFile(Client.FileManager.FileType.Dat, path);

                        if (f == null && File.Exists(path))
                        {
                            dats[dat] = f = Settings.CreateDatFile();
                            f.Path = path;
                        }

                        _account.DatFile = dats[dat];
                    }
                    else
                    {
                        var path = CreateTempFile();

                        if (!string.IsNullOrEmpty(path))
                        {
                            var f = Settings.CreateDatFile();
                            f.Path = path;
                            if (dat != -1)
                                dats[dat] = f;
                            _account.DatFile = f;
                        }
                    }
                }
                else
                {
                    _account.DatFile = dats[dat];
                }

                if (gfx == -1 || checkGfx[gfx] && gfxs[gfx] == null)
                {
                    if (gfx != -1)
                    {
                        checkGfx[gfx] = false;
                    }

                    if (gfx == INDEX_DEFAULT)
                    {
                        var path = Client.FileManager.GetDefaultPath(Client.FileManager.FileType.Gfx);
                        var f = gfxs[gfx] = (Settings.IGfxFile)Client.FileManager.FindFile(Client.FileManager.FileType.Gfx, path);

                        if (f == null && File.Exists(path))
                        {
                            gfxs[gfx] = f = Settings.CreateGfxFile();
                            f.Path = path;
                        }

                        _account.GfxFile = gfxs[gfx];
                    }
                    else
                    {
                        var path = CreateTempFile();

                        if (!string.IsNullOrEmpty(path))
                        {
                            if (gfxfile == null)
                                gfxfile = ReadGfxData();

                            if (gfxfile.Length > 0)
                            {
                                try
                                {
                                    File.WriteAllBytes(path, gfxfile);
                                }
                                catch { }
                            }

                            var f = Settings.CreateGfxFile();
                            f.Path = path;
                            if (gfx != -1)
                                gfxs[gfx] = f;
                            _account.GfxFile = f;
                        }
                    }
                }
                else
                {
                    _account.GfxFile = gfxs[gfx];
                }

                //note some accounts may be given null dat/gfx files - these will be automatically set on launch to the default files used by gw2
            }

            if (radioAddonsAuto.Checked || radioAddonsManual.Checked)
            {
                var o = Settings.LocalizeAccountExecutionOptions.Enabled;

                if (panelDirectUpdates.Enabled && radioDirectUpdateYes.Checked)
                    o |= Settings.LocalizeAccountExecutionOptions.OnlyIncludeBinFolders;
                if (radioAddonsAuto.Checked)
                    o |= Settings.LocalizeAccountExecutionOptions.AutoSync | Settings.LocalizeAccountExecutionOptions.AutoSyncDeleteUnknowns;

                Settings.GuildWars2.LocalizeAccountExecution.Value = o;
            }

            if (panelDirectUpdates.Enabled && radioDirectUpdateYes.Checked)
            {
                Settings.GuildWars2.DatUpdaterEnabled.Value = true;
                Settings.GuildWars2.UseCustomGw2Cache.Value = true;
            }

            if (!Client.FileManager.IsVirtualModeSupported)
            {
                if (Client.FileManager.IsBasicModeSupported)
                {
                    Settings.GuildWars2.ProfileMode.Value = Settings.ProfileMode.Basic;
                    Settings.GuildWars2.ProfileOptions.Value = Settings.ProfileModeOptions.None;
                }
            }

            this.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.Close();
        }

        private void panelReady_VisibleChanged(object sender, EventArgs e)
        {
            if (panelReady.Visible)
            {
                var count = GetAccounts().Count;
                var sb = new StringBuilder(30);

                if (count == 0)
                    sb.Append("No");
                else
                    sb.Append(count);
                sb.Append(" account");
                if (count != 1)
                    sb.Append('s');
                sb.Append(" will be created");

                labelMessage.Text = sb.ToString();
                labelMessage.Visible = true;
            }
            else
            {
                labelMessage.Visible = false;
            }
        }

        private void labelAdd_Click(object sender, EventArgs e)
        {
            buttonAdd_Click(sender, e);
        }

        private void buttonAdd_Click(object sender, EventArgs e)
        {
            using (var f = new formAccount())
            {
                if (f.ShowDialog(this) == System.Windows.Forms.DialogResult.OK)
                {
                    var panel = new UI.Controls.StackPanel()
                    {
                        Anchor = panelAccount.Anchor,
                        Margin = panelAccount.Margin,
                        FlowDirection = FlowDirection.LeftToRight,
                        Size = new Size(panelAccountsContent.Width, textName.Height),
                    };
                    var text = new TextBox()
                    {
                        Margin = textName.Margin,
                        Anchor = textName.Anchor,
                    };
                    var button = new UI.Controls.FlatShapeButton()
                    {
                        Shape = buttonOptions.Shape,
                        ShapeAlignment = buttonOptions.ShapeAlignment,
                        ShapeSize = buttonOptions.ShapeSize,
                        LineSize = buttonOptions.LineSize,
                        Margin = buttonOptions.Margin,
                        Padding = buttonOptions.Padding,
                        Anchor = buttonOptions.Anchor,
                        Size = buttonOptions.Size,
                        Cursor = buttonOptions.Cursor,
                    };
                    panel.Controls.AddRange(new Control[] { text, button });

                    var a = new Account(text)
                    {
                        Name = f.AccountName,
                    };
                    a.SetLogin(f.Email, f.Password);

                    button.Click += buttonOptions_Click;
                    panel.Tag = a;

                    panelAccountsContent.SuspendLayout();
                    //labelNoAccounts.Visible = false;
                    panelAccountsContent.Controls.Add(panel);
                    panelAccountsContent.ResumeLayout();

                    ((Panel)labelAdd.Parent.Parent).ScrollControlIntoView(labelAdd);
                }
            }
        }

        private void buttonOptions_Click(object sender, EventArgs e)
        {
            contextAccount.Tag = (Control)sender;
            contextAccount.Show(Cursor.Position);
        }

        private async void EnableOnLeave(Control c, int limit)
        {
            c.Enabled = false;

            var r = new Rectangle(c.PointToScreen(Point.Empty), c.Size);
            var start = Environment.TickCount;

            while (r.Contains(Cursor.Position) && (Environment.TickCount - start) < limit)
            {
                await Task.Delay(100);
            }

            c.Enabled = true;
        }

        private void buttonCancel_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void buttonGw2Path_Click(object sender, EventArgs e)
        {
            using (var f = new OpenFileDialog())
            {
                f.ValidateNames = false;
                f.Filter = "Guild Wars 2|Gw2*.exe|All executables|*.exe";
                if (Environment.Is64BitOperatingSystem)
                    f.Title = "Open Gw2-64.exe";
                else
                    f.Title = "Open Gw2.exe";

                if (textGw2Path.TextLength != 0)
                {
                    f.InitialDirectory = System.IO.Path.GetDirectoryName(textGw2Path.Text);
                }

                if (f.ShowDialog(this) == System.Windows.Forms.DialogResult.OK)
                {
                    var bits = Util.FileUtil.GetExecutableBits(f.FileName);
                    if (Environment.Is64BitOperatingSystem && bits == 32)
                    {
                        if (MessageBox.Show(this, "You've selected to use the 32-bit version of Guild Wars 2 on a 64-bit system.\n\nAre you sure?", "32-bit?", MessageBoxButtons.YesNo, MessageBoxIcon.Exclamation) != System.Windows.Forms.DialogResult.Yes)
                            return;
                    }

                    textGw2Path.Text = f.FileName;
                    textGw2Path.Select(textGw2Path.TextLength, 0);

                    try
                    {
                        var path = Path.GetDirectoryName(f.FileName);
                        if (!Util.FileUtil.HasFolderPermissions(path, System.Security.AccessControl.FileSystemRights.Modify) && !Util.FileUtil.CanWriteToFolder(path))
                        {
                            if (MessageBox.Show(this, "The selected path will require elevated permissions to modify.\n\nAllow normal access to the folder?", "Warning", MessageBoxButtons.YesNo, MessageBoxIcon.Exclamation) == System.Windows.Forms.DialogResult.Yes)
                                Util.ProcessUtil.CreateFolder(path);
                        }
                    }
                    catch (Exception ex)
                    {
                        Util.Logging.Log(ex);
                    }
                }
            }
        }
    }
}
