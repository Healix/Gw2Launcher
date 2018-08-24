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
using Gw2Launcher.UI.Controls;

namespace Gw2Launcher.UI
{
    public partial class formAccount : Form
    {
        private SidebarButton selectedButton;
        private Settings.IAccount account;
        private formBrowseLocalDat.SelectedFile selectedFile;
        private Settings.IDatFile datFile;

        public formAccount()
        {
            InitializeComponent();

            labelCreatedDate.Text = "n/a";
            labelLastLaunch.Text = "never";
            labelLaunchCount.Text = "0";
            labelAccountId.Text = "n/a";

            Rectangle r = new Rectangle();
            Rectangle bounds = Screen.PrimaryScreen.Bounds;

            r.Width = bounds.Width / 3;
            r.Height = bounds.Height / 3;
            r.X = bounds.X + bounds.Width / 2 - r.Width / 2;
            r.Y = bounds.Y + bounds.Height / 2 - r.Height / 2;

            textWindowed.Text = ToString(r);

            buttonGeneral.Tag = panelGeneral;
            buttonArguments.Tag = panelArguments;
            buttonLocalDat.Tag = panelLocalDat;
            buttonStatistics.Tag = panelStatistics;
            buttonLaunchOptions.Tag = panelLaunchOptions;

            foreach (Control c in sidebarPanel1.Controls)
            {
                SidebarButton b = c as SidebarButton;
                if (b != null)
                {
                    b.SelectedChanged += sidebarButton_SelectedChanged;
                    b.Click += sidebarButton_Click;
                }
            }

            buttonGeneral.Selected = true;
        }

        public formAccount(Settings.IAccount account)
            : this()
        {
            this.account = account;

            if (account.TotalUses == 1)
            {
                labelLaunchCount.Text = "once";
                labelLaunchCountEnd.Visible = false;
            }
            else
                labelLaunchCount.Text = account.TotalUses.ToString();

            if (account.LastUsedUtc != DateTime.MinValue)
            {
                var local = account.LastUsedUtc.ToLocalTime();
                labelLastLaunch.Text = local.ToLongDateString() + " " + local.ToLongTimeString();
            }

            if (account.CreatedUtc != DateTime.MinValue)
            {
                var local = account.CreatedUtc.ToLocalTime();
                labelCreatedDate.Text = local.ToLongDateString() + " " + local.ToLongTimeString();
            }

            labelAccountId.Text = account.UID.ToString();

            labelExportRecordedLaunch.Visible = File.Exists(Path.Combine(DataPath.AppData, "statistics.dat"));
            checkRecordLaunch.Checked = account.RecordLaunches;

            checkAutomaticLogin.Checked=account.AutomaticLogin;
            if (account.AutomaticLogin)
            {
                textAutoLoginEmail.Text = account.AutomaticLoginEmail;
                textAutoLoginPassword.Text = account.AutomaticLoginPassword;
            }

            textAccountName.Text = account.Name;
            textWindowsAccount.Text = account.WindowsAccount;
            textArguments.Text = account.Arguments;

            checkWindowed.Checked = account.Windowed;
            if (!account.Windowed && account.WindowBounds.IsEmpty)
            {
                Rectangle bounds = Screen.PrimaryScreen.Bounds;
                Rectangle r = new Rectangle();
                r.Width = bounds.Width / 3;
                r.Height = bounds.Height / 3;
                r.X = bounds.X + bounds.Width / 2 - r.Width / 2;
                r.Y = bounds.Y + bounds.Height / 2 - r.Height / 2;
                textWindowed.Text = ToString(r);
            }
            else
                textWindowed.Text = ToString(account.WindowBounds);

            checkShowDaily.Checked = account.ShowDaily;

            if (account.DatFile != null)
            {
                textLocalDat.Text = account.DatFile.Path;
                textLocalDat.Select(textLocalDat.TextLength, 0);
            }
            else
                textLocalDat.Text = "";

            if (account.VolumeEnabled)
            {
                checkVolume.Checked = true;
                sliderVolume.Value = account.Volume;
            }

            if (!string.IsNullOrEmpty(account.RunAfterLaunching))
                textRunAfterLaunch.Text = account.RunAfterLaunching;
        }

        public Settings.IAccount Account
        {
            get
            {
                return this.account;
            }
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

        private void formAccount_Load(object sender, EventArgs e)
        {
            textAccountName.Focus();
        }

        private void checkAutomaticLogin_CheckedChanged(object sender, EventArgs e)
        {
            textAutoLoginEmail.Enabled = checkAutomaticLogin.Checked;
            textAutoLoginPassword.Enabled = checkAutomaticLogin.Checked;
        }

        private void labelLaunchCount_SizeChanged(object sender, EventArgs e)
        {
            labelLaunchCountEnd.Location = new Point(labelLaunchCount.Location.X + labelLaunchCount.Width + 1, labelLaunchCountEnd.Location.Y);
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            base.OnFormClosing(e);

            if (selectedFile != null)
            {
                selectedFile.Cancel();
                selectedFile = null;
            }
        }

        private void buttonUsername_Click(object sender, EventArgs e)
        {
            using (formWindowsAccount f = new formWindowsAccount(textWindowsAccount.Text))
            {
                if (f.ShowDialog(this) == DialogResult.OK)
                    textWindowsAccount.Text = f.AccountName;
            }
        }

        private Settings.IDatFile GetDatFile()
        {
            if (selectedFile == null)
            {
                if (account != null)
                    return account.DatFile;
                else
                    return null;
            }
            else if (selectedFile.DatFile != null)
            {
                return selectedFile.DatFile;
            }
            else if (selectedFile.Path == null)
            {
                return null;
            }

            Security.Impersonation.IImpersonationToken impersonation = null;

            string username = Util.Users.GetUserName(textWindowsAccount.Text);
            bool isCurrent = Util.Users.IsCurrentUser(username);
            bool userExists = isCurrent;
            bool wasMoved = false;

            string path = null;

            if (!isCurrent)
            {
                try
                {
                    using (var user = Util.Users.GetPrincipal(username))
                    {
                        userExists = user != null;
                    }
                }
                catch (Exception ex)
                {
                    Util.Logging.Log(ex);
                }

                if (userExists)
                {
                    var password = Security.Credentials.GetPassword(username);

                    while (true)
                    {
                        if (password == null)
                        {
                            using (formPassword f = new formPassword("Password for " + username))
                            {
                                if (f.ShowDialog(this) == DialogResult.OK)
                                {
                                    password = f.Password;
                                    Security.Credentials.SetPassword(username, password);
                                }
                                else
                                {
                                    break;
                                }
                            }
                        }

                        try
                        {
                            impersonation = Security.Impersonation.Impersonate(username, Security.Credentials.GetPassword(username));
                            break;
                        }
                        catch (Win32Exception ex)
                        {
                            Util.Logging.Log(ex);

                            if (ex.NativeErrorCode == 1326)
                            {
                                password = null;
                                continue;
                            }

                            break;
                        }
                        catch (Exception ex)
                        {
                            Util.Logging.Log(ex);
                            break;
                        }
                    }
                }
            }

            if (isCurrent || impersonation != null)
            {
                try
                {
                    string folder = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData, Environment.SpecialFolderOption.Create);
                    if (!string.IsNullOrWhiteSpace(folder))
                    {
                        folder = Path.Combine(folder, "Guild Wars 2");
                        if (!Directory.Exists(folder))
                            Directory.CreateDirectory(folder);

                        path = Util.FileUtil.GetTemporaryFileName(folder);

                        try
                        {
                            File.Move(selectedFile.Path, path);
                            wasMoved = true;
                        }
                        catch (Exception ex)
                        {
                            Util.Logging.Log(ex);

                            try
                            {
                                File.Copy(selectedFile.Path, path, true);
                            }
                            catch (Exception e)
                            {
                                Util.Logging.Log(e);
                                throw;
                            }
                        }
                    }
                    else
                    {
                        //the user exists, but the account hasn't been set up yet
                    }
                }
                finally
                {
                    if (impersonation != null)
                        impersonation.Dispose();
                }
            }

            if (datFile == null)
                datFile = Settings.CreateDatFile();

            if (path == null)
                datFile.Path = Path.GetFullPath(selectedFile.Path);
            else
            {
                try
                {
                    //now that datFile setting has been created, rename the file from its temp name
                    string _path = Path.Combine(Path.GetDirectoryName(path), "Local." + datFile.UID + ".dat");
                    File.Move(path, _path);
                    path = _path;
                }
                catch (Exception ex)
                {
                    Util.Logging.Log(ex);
                }
                datFile.Path = Path.GetFullPath(path);
            }

            if (!wasMoved)
            {
                try
                {
                    File.Delete(selectedFile.Path);
                }
                catch (Exception ex)
                {
                    Util.Logging.Log(ex);
                }
            }

            return datFile;
        }

        private void buttonOK_Click(object sender, EventArgs e)
        {
            if (textAccountName.Text.Length == 0)
            {
                buttonGeneral.Selected = true;
                MessageBox.Show(this, "An identifier / account or display name is required", "Identifier / name required", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                return;
            }

            Rectangle windowBounds;

            if (checkWindowed.Checked)
            {
                Rectangle r = ParseWindowSize(textWindowed.Text);
                windowBounds = FixSize(r);
            }
            else
                windowBounds = Rectangle.Empty;

            Settings.IDatFile datFile;

            try
            {
                datFile = GetDatFile();
                selectedFile = null;
            }
            catch (Exception ex)
            {
                Util.Logging.Log(ex);
                MessageBox.Show(this, "An error occured while handling Local.dat.\n\n" + ex.Message, "Failed handling Local.dat", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            if (this.account == null)
            {
                this.account = Settings.CreateAccount();
                this.account.LastUsedUtc = DateTime.MinValue;
                this.account.CreatedUtc = DateTime.UtcNow;
            }

            this.account.Windowed = checkWindowed.Checked;
            this.account.Name = textAccountName.Text;
            this.account.WindowsAccount = textWindowsAccount.Text;
            this.account.Arguments = textArguments.Text;
            this.account.ShowDaily = checkShowDaily.Checked;
            this.account.DatFile = datFile;
            this.account.WindowBounds = windowBounds;
            this.account.RecordLaunches = checkRecordLaunch.Checked;

            if (checkAutomaticLogin.Checked && textAutoLoginEmail.TextLength > 0 && textAutoLoginPassword.TextLength > 0)
            {
                this.account.AutomaticLoginEmail = textAutoLoginEmail.Text;
                this.account.AutomaticLoginPassword = textAutoLoginPassword.Text;
            }
            else
            {
                this.account.AutomaticLoginEmail = null;
                this.account.AutomaticLoginPassword = null;
            }

            if (checkVolume.Checked)
                this.account.Volume = sliderVolume.Value;
            else
                this.account.VolumeEnabled = false;

            if (!string.IsNullOrEmpty(textRunAfterLaunch.Text))
                this.account.RunAfterLaunching = textRunAfterLaunch.Text;
            else
                this.account.RunAfterLaunching = null;

            this.DialogResult = System.Windows.Forms.DialogResult.OK;
        }

        private void checkWindowed_CheckedChanged(object sender, EventArgs e)
        {
            textWindowed.Enabled = buttonWindowed.Enabled = checkWindowed.Checked;
        }

        private string ToString(Rectangle r)
        {
            return string.Format("{0}x, {1}y, {2}w, {3}h", r.X, r.Y, r.Width, r.Height);
        }

        private Rectangle ParseWindowSize(string s)
        {
            try
            {
                Rectangle r = new Rectangle();
                char current = char.MinValue;
                int last = 0;

                for (int i = s.Length - 1; i >= 0; i--)
                {
                    char c = s[i];

                    if (c == ',' || i == 0)
                    {
                        if (current != char.MinValue)
                        {
                            int n = 0;
                            int j = i > 0 ? i + 1 : i;

                            if (last > j && Int32.TryParse(s.Substring(j, last - j), out n))
                            {
                                switch (current)
                                {
                                    case 'w':
                                        r.Width = n;
                                        break;
                                    case 'h':
                                        r.Height = n;
                                        break;
                                    case 'x':
                                        r.X = n;
                                        break;
                                    case 'y':
                                        r.Y = n;
                                        break;
                                }
                            }
                        }

                        current = char.MinValue;
                    }
                    else if (char.IsNumber(c) || c == '-' || c == '.')
                    {

                    }
                    else
                    {
                        switch (c)
                        {
                            case 'w':
                            case 'h':
                            case 'x':
                            case 'y':
                                current = s[i];
                                last = i;
                                break;
                        }
                    }
                }

                return r;
            }
            catch (Exception e)
            {
                Util.Logging.Log(e);
                return Rectangle.Empty;
            }
        }

        private Rectangle FixSize(Rectangle r)
        {
            if (r.Width < 50)
                r.Width = 50;
            if (r.Height < 50)
                r.Height = 50;

            return r;
        }

        private void buttonWindowed_Click(object sender, EventArgs e)
        {
            using (formWindowSize f = new formWindowSize(true))
            {
                Rectangle r = FixSize(ParseWindowSize(textWindowed.Text));

                f.SetBounds(r.X, r.Y, r.Width, r.Height);
                if (f.ShowDialog(this) == DialogResult.OK)
                    textWindowed.Text = ToString(f.Bounds);

                this.Focus();
            }
        }

        private void buttonCancel_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
        }

        private void buttonBrowseLocalDat_Click(object sender, EventArgs e)
        {
            using (formBrowseLocalDat f = new formBrowseLocalDat(null))
            {
                if (f.ShowDialog(this) == DialogResult.OK)
                {
                    if (selectedFile != null)
                        selectedFile.Cancel();

                    selectedFile = f.Result;
                    if (selectedFile == null)
                        textLocalDat.Text = "";
                    else if (selectedFile.DatFile != null)
                        textLocalDat.Text = selectedFile.DatFile.Path;
                    else
                        textLocalDat.Text = selectedFile.Path;
                }
            }
        }

        private async void labelExportRecordedLaunch_Click(object sender, EventArgs e)
        {
            labelExportRecordedLaunch.Enabled = false;
            Color c = labelExportRecordedLaunch.ForeColor;
            labelExportRecordedLaunch.ForeColor = SystemColors.GrayText;

            string path = await Task.Factory.StartNew<string>(new Func<string>(
                delegate
                {
                    return Tools.Statistics.Export(this.account);
                }));

            if (path == null)
            {
                MessageBox.Show(this, this.account.Name + " has no data to export", "Nothing to export", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            }
            else
            {
                using (SaveFileDialog f = new SaveFileDialog())
                {
                    f.Filter = "CSV|*.csv";

                    if (f.ShowDialog() == DialogResult.OK)
                    {
                        try
                        {
                            if (File.Exists(f.FileName))
                                File.Delete(f.FileName);

                            File.Move(path, f.FileName);
                        }
                        catch (Exception ex)
                        {
                            Util.Logging.Log(ex);
                            MessageBox.Show(this, "An error occured while trying to save the file:\n\n" + ex.Message, "Unable to save file", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        }
                    }
                    else
                    {
                        try
                        {
                            File.Delete(path);
                        }
                        catch (Exception ex)
                        {
                            Util.Logging.Log(ex);
                        }
                    }
                }
            }

            labelExportRecordedLaunch.ForeColor = c;
            labelExportRecordedLaunch.Enabled = true;
        }

        private void labelViewRecordedLaunch_Click(object sender, EventArgs e)
        {

        }

        private void label30_Click(object sender, EventArgs e)
        {
            textRunAfterLaunch.SelectedText = ((Label)sender).Text;
        }

        private void checkVolume_CheckedChanged(object sender, EventArgs e)
        {
            sliderVolume.Enabled = checkVolume.Checked;
        }

        private void sliderVolume_ValueChanged(object sender, float e)
        {
            labelVolume.Text = (int)(e * 100 + 0.5f) + "%";
        }

    }
}
