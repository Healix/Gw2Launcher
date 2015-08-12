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

        public formSettings()
        {
            InitializeComponent();

            buttonGeneral.Tag = panelGeneral;
            buttonArguments.Tag = panelArguments;
            buttonPasswords.Tag = panelPasswords;
            buttonStyle.Tag = panelStyle;

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
            checkCheckBuildOnLaunch.Checked = Settings.CheckForNewBuilds;
            checkShowUser.Checked = !Settings.ShowAccount.HasValue || Settings.ShowAccount.Value;

            buttonSample.ShowAccount = checkShowUser.Checked;
            if (Settings.FontLarge.HasValue)
                buttonSample.FontLarge = Settings.FontLarge.Value;
            if (Settings.FontSmall.HasValue)
                buttonSample.FontSmall = Settings.FontSmall.Value;

            labelFontRestoreDefaults.Visible = Settings.FontLarge.HasValue || Settings.FontSmall.HasValue;
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
                MessageBox.Show(this, "Unable to clear all stored passwords\n\n" + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void formSettings_Load(object sender, EventArgs e)
        {

        }

        private void buttonGW2Path_Click(object sender, EventArgs e)
        {
            OpenFileDialog f = new OpenFileDialog();
            f.Filter = "Guild Wars 2|Gw2.exe|All executables|*.exe";
            f.Title = "Open Gw2.exe";

            if (f.ShowDialog(this) == System.Windows.Forms.DialogResult.OK)
            {
                textGW2Path.Text = f.FileName;
            }
        }

        private void buttonOK_Click(object sender, EventArgs e)
        {
            Settings.GW2Path.Value = textGW2Path.Text;
            Settings.GW2Arguments.Value = textArguments.Text;
            Settings.MinimizeToTray.Value = checkMinimizeToTray.Checked;
            Settings.ShowTray.Value = checkShowTrayIcon.Checked;
            Settings.BringToFrontOnExit.Value = checkBringToFrontOnExit.Checked;
            Settings.StoreCredentials.Value = checkStoreCredentials.Checked;

            if (checkCheckBuildOnLaunch.Checked != Settings.CheckForNewBuilds)
            {
                if (checkCheckBuildOnLaunch.Checked)
                    Settings.LastKnownBuild.Value = 0;
                else
                    Settings.LastKnownBuild.Clear();
            }

            Settings.ShowAccount.Value = checkShowUser.Checked;

            if (buttonSample.FontLarge.Equals(UI.Controls.AccountGridButton.FONT_LARGE))
                Settings.FontLarge.Clear();
            else
                Settings.FontLarge.Value = buttonSample.FontLarge;

            if (buttonSample.FontSmall.Equals(UI.Controls.AccountGridButton.FONT_SMALL))
                Settings.FontSmall.Clear();
            else
                Settings.FontSmall.Value = buttonSample.FontSmall;

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
    }
}
