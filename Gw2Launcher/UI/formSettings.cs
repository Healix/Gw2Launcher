using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Gw2Launcher.UI.Controls;

namespace Gw2Launcher.UI
{
    public partial class formSettings : Form
    {
        private SidebarButton selectedButton;
        private Form activeWindow;

        public formSettings()
        {
            InitializeComponent();

            buttonGeneral.Tag = panelGeneral;
            buttonArguments.Tag = panelArguments;
            buttonPasswords.Tag = panelPasswords;
            buttonStyle.Tag = panelStyle;
            buttonUpdates.Tag = panelUpdates;

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

            var path = Settings.GW2Path;
            if (path.HasValue)
                textGW2Path.Text = path.Value;
            else
                textGW2Path.Text = "";

            var args = Settings.GW2Arguments;
            if (args.HasValue)
                textArguments.Text = args.Value;
            else
                textArguments.Text = "";

            checkMinimizeToTray.Checked = Settings.MinimizeToTray.Value;
            checkShowTrayIcon.Checked = !Settings.ShowTray.HasValue || Settings.ShowTray.Value;
            checkBringToFrontOnExit.Checked = Settings.BringToFrontOnExit.Value;
            checkStoreCredentials.Checked = Settings.StoreCredentials.Value;
            checkShowUser.Checked = !Settings.ShowAccount.HasValue || Settings.ShowAccount.Value;

            buttonSample.ShowAccount = checkShowUser.Checked;
            if (Settings.FontLarge.HasValue)
                buttonSample.FontLarge = Settings.FontLarge.Value;
            if (Settings.FontSmall.HasValue)
                buttonSample.FontSmall = Settings.FontSmall.Value;

            labelFontRestoreDefaults.Visible = Settings.FontLarge.HasValue || Settings.FontSmall.HasValue;

            checkCheckBuildOnLaunch.Checked = Settings.CheckForNewBuilds.Value;
            checkAutoUpdate.Checked = Settings.AutoUpdate.Value;
            checkAutoUpdateDownload.Checked = Settings.BackgroundPatchingEnabled.Value;
            if (Settings.BackgroundPatchingNotifications.HasValue)
            {
                checkAutoUpdateDownloadNotifications.Checked = true;
                checkAutoUpdateDownloadNotifications.Tag = Settings.BackgroundPatchingNotifications.Value;
            }
            else
            {
                checkAutoUpdateDownloadNotifications.Tag = new Settings.ScreenAttachment()
                {
                    screen = 0,
                    anchor = Settings.ScreenAnchor.BottomRight
                };
            }
            if (Settings.AutoUpdateInterval.HasValue)
                numericUpdateInterval.Value = Settings.AutoUpdateInterval.Value;
            checkVolume.Checked = Settings.Volume.HasValue;
            if (Settings.Volume.HasValue)
                sliderVolume.Value = Settings.Volume.Value;
            if (Settings.RunAfterLaunching.HasValue)
                textRunAfterLaunch.Text = Settings.RunAfterLaunching.Value;
            else
                textRunAfterLaunch.Text = "";

            if (Settings.LastProgramVersion.HasValue)
            {
                checkCheckVersionOnStart.Checked = true;
                labelVersionUpdate.Visible = Settings.LastProgramVersion.Value.version > Program.RELEASE_VERSION;
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

        private void buttonClearPasswords_Click(object sender, EventArgs e)
        {
            try
            {
                Security.Credentials.Clear();
                MessageBox.Show(this, "All stored passwords have been cleared", "Cleared", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                Util.Logging.Log(ex);
                MessageBox.Show(this, "Unable to clear all stored passwords\n\n" + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void formSettings_Load(object sender, EventArgs e)
        {

        }

        private void buttonGW2Path_Click(object sender, EventArgs e)
        {
            OpenFileDialog f = new OpenFileDialog();

            f.Filter = "Guild Wars 2|Gw2*.exe|All executables|*.exe";
            if (Environment.Is64BitOperatingSystem)
                f.Title = "Open Gw2-64.exe";
            else
                f.Title = "Open Gw2.exe";

            if (textGW2Path.TextLength != 0)
            {
                f.InitialDirectory = System.IO.Path.GetDirectoryName(textGW2Path.Text);
            }

            if (f.ShowDialog(this) == System.Windows.Forms.DialogResult.OK)
            {
                textGW2Path.Text = f.FileName;
            }
        }

        private void buttonOK_Click(object sender, EventArgs e)
        {
            if (activeWindow != null && !activeWindow.IsDisposed)
                activeWindow.Close();

            Settings.GW2Path.Value = textGW2Path.Text;
            Settings.GW2Arguments.Value = textArguments.Text;
            Settings.MinimizeToTray.Value = checkMinimizeToTray.Checked;
            Settings.ShowTray.Value = checkShowTrayIcon.Checked;
            Settings.BringToFrontOnExit.Value = checkBringToFrontOnExit.Checked;
            Settings.StoreCredentials.Value = checkStoreCredentials.Checked;
            Settings.CheckForNewBuilds.Value = checkCheckBuildOnLaunch.Checked;

            Settings.ShowAccount.Value = checkShowUser.Checked;

            if (buttonSample.FontLarge.Equals(UI.Controls.AccountGridButton.FONT_LARGE))
                Settings.FontLarge.Clear();
            else
                Settings.FontLarge.Value = buttonSample.FontLarge;

            if (buttonSample.FontSmall.Equals(UI.Controls.AccountGridButton.FONT_SMALL))
                Settings.FontSmall.Clear();
            else
                Settings.FontSmall.Value = buttonSample.FontSmall;

            if (checkVolume.Checked)
                Settings.Volume.Value = sliderVolume.Value;
            else
                Settings.Volume.Clear();

            Settings.AutoUpdateInterval.Value = (ushort)numericUpdateInterval.Value;
            Settings.AutoUpdate.Value = checkAutoUpdate.Checked;
            Settings.BackgroundPatchingEnabled.Value = checkAutoUpdateDownload.Checked;
            if (checkAutoUpdateDownloadNotifications.Checked)
                Settings.BackgroundPatchingNotifications.Value = (Settings.ScreenAttachment)checkAutoUpdateDownloadNotifications.Tag;
            else
                Settings.BackgroundPatchingNotifications.Clear();
            if (!string.IsNullOrEmpty(textRunAfterLaunch.Text))
                Settings.RunAfterLaunching.Value = textRunAfterLaunch.Text;
            else
                Settings.RunAfterLaunching.Clear();

            if (checkCheckVersionOnStart.Checked)
            {
                if (!Settings.LastProgramVersion.HasValue)
                    Settings.LastProgramVersion.Value = new Settings.LastCheckedVersion();
            }
            else
                Settings.LastProgramVersion.Clear();

            this.DialogResult = System.Windows.Forms.DialogResult.OK;
        }

        private void checkShowTrayIcon_CheckedChanged(object sender, EventArgs e)
        {
            checkMinimizeToTray.Enabled = checkShowTrayIcon.Checked;
        }

        private void buttonWindowReset_Click(object sender, EventArgs e)
        {
            Settings.WindowBounds[typeof(formMain)].Clear();
        }

        private void buttonCancel_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
        }

        private Font ShowFontDialog(Font font)
        {
            using (FontDialog f = new FontDialog())
            {
                f.FontMustExist = true;
                f.AllowSimulations = false;
                f.AllowVerticalFonts = false;
                f.Font = font;

                try
                {
                    if (f.ShowDialog(this) == DialogResult.OK)
                    {
                        return f.Font;
                    }
                }
                catch (Exception ex)
                {
                    Util.Logging.Log(ex);
                    MessageBox.Show(this, ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }

            return null;
        }

        private void buttonFontTitle_Click(object sender, EventArgs e)
        {
            Font f = ShowFontDialog(buttonSample.FontLarge);
            if (f != null)
            {
                buttonSample.FontLarge = f;
                labelFontRestoreDefaults.Visible = true;
            }
        }

        private void buttonFontDescriptors_Click(object sender, EventArgs e)
        {
            Font f = ShowFontDialog(buttonSample.FontSmall);
            if (f != null)
            {
                buttonSample.FontSmall = f;
                labelFontRestoreDefaults.Visible = true;
            }
        }

        private void checkShowUser_CheckedChanged(object sender, EventArgs e)
        {
            buttonSample.ShowAccount = checkShowUser.Checked;
        }

        private void labelFontRestoreDefaults_Click(object sender, EventArgs e)
        {
            labelFontRestoreDefaults.Visible = false;
            buttonSample.FontLarge = UI.Controls.AccountGridButton.FONT_LARGE;
            buttonSample.FontSmall = UI.Controls.AccountGridButton.FONT_SMALL;
        }

        private void checkVolume_CheckedChanged(object sender, EventArgs e)
        {
            sliderVolume.Enabled = checkVolume.Checked;
        }

        private void label30_Click(object sender, EventArgs e)
        {
            textRunAfterLaunch.SelectedText = ((Label)sender).Text;
        }

        private void labelVersionUpdate_Click(object sender, EventArgs e)
        {
            using (formVersionUpdate f = new formVersionUpdate())
            {
                f.ShowDialog(this);
            }
        }

        private void sliderVolume_ValueChanged(object sender, float e)
        {
            labelVolume.Text = (int)(e * 100 + 0.5f) + "%";
        }

        private void checkAutoUpdateDownload_CheckedChanged(object sender, EventArgs e)
        {
            checkAutoUpdateDownloadNotifications.Enabled = checkAutoUpdateDownload.Checked;
        }

        private Form SetActive(Form form)
        {
            if (activeWindow != null && !activeWindow.IsDisposed)
                activeWindow.Close();
            activeWindow = form;
            return form;
        }

        private void labelAutoUpdateDownloadNotificationsConfig_Click(object sender, EventArgs e)
        {
            var v = (Settings.ScreenAttachment)checkAutoUpdateDownloadNotifications.Tag;
            var f = SetActive(new formScreenPosition(v.screen, v.anchor));
            f.FormClosing += screenPosition_FormClosing;
            f.StartPosition = FormStartPosition.Manual;
            var source = labelAutoUpdateDownloadNotificationsConfig;
            f.Location = Point.Add(source.Parent.PointToScreen(Point.Empty), new Size(source.Location.X - f.Width / 2, source.Location.Y - f.Height / 2));
            f.Show();
        }

        void screenPosition_FormClosing(object sender, FormClosingEventArgs e)
        {
            var f = sender as formScreenPosition;
            if (f != null)
            {
                checkAutoUpdateDownloadNotifications.Tag = new Settings.ScreenAttachment()
                {
                    screen = (byte)f.SelectedScreen,
                    anchor = f.SelectedAnchor
                };
            }
        }
    }
}
