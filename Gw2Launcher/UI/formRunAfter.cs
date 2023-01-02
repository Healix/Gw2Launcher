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

namespace Gw2Launcher.UI
{
    public partial class formRunAfter : Base.BaseForm
    {
        private Settings.AccountType type;

        public formRunAfter(Settings.RunAfter ra, Settings.AccountType type)
        {
            this.type = type;

            InitializeComponents();

            if (ra != null)
            {
                if (ra.Name != null)
                    textName.Text = ra.Name;

                textPath.Text = ra.Path;
                textPath.Select(textPath.TextLength, 0);

                checkUseCurrentUser.Checked = (ra.Options & Settings.RunAfter.RunAfterOptions.UseCurrentUser) != 0;
                checkWaitUntilComplete.Checked = (ra.Options & Settings.RunAfter.RunAfterOptions.WaitUntilComplete) != 0;

                if (ra.Type == Settings.RunAfter.RunAfterType.ShellCommands)
                {
                    radioCommands.Checked = true;
                    textCommands.Text = ra.Arguments;
                }
                else
                {
                    textArguments.Text = ra.Arguments;
                }
            }

            if (ra != null && (!ra.Enabled || Util.ComboItem<Settings.RunAfter.RunAfterWhen>.Select(comboRunAfter, ra.When) == -1) || ra == null && Util.ComboItem<Settings.RunAfter.RunAfterWhen>.Select(comboRunAfter, Settings.RunAfter.RunAfterWhen.AfterLaunching) == -1)
                comboRunAfter.SelectedIndex = 0;

            if (ra == null || Util.ComboItem<Settings.RunAfter.RunAfterOptions>.Select(comboOnExit, ra.Options & (Settings.RunAfter.RunAfterOptions.CloseOnExit | Settings.RunAfter.RunAfterOptions.KillOnExit)) == -1)
                comboOnExit.SelectedIndex = 0;

            ActiveControl = textName;
        }

        public formRunAfter(Settings.AccountType type)
            : this(null, type)
        {

        }

        protected override void OnInitializeComponents()
        {
            base.OnInitializeComponents();

            InitializeComponent();

            comboRunAfter.Items.AddRange(new object[]
                {
                    new Util.ComboItem<Settings.RunAfter.RunAfterWhen>(Settings.RunAfter.RunAfterWhen.None, "Disabled"),
                    new Util.ComboItem<Settings.RunAfter.RunAfterWhen>(Settings.RunAfter.RunAfterWhen.Manual, "Manual"),
                    new Util.ComboItem<Settings.RunAfter.RunAfterWhen>(Settings.RunAfter.RunAfterWhen.BeforeLaunching, "Before launching"),
                    new Util.ComboItem<Settings.RunAfter.RunAfterWhen>(Settings.RunAfter.RunAfterWhen.AfterLaunching, "After launching")
                });

            if (this.type == Settings.AccountType.GuildWars1)
            {
                comboRunAfter.Items.AddRange(new object[]
                    {
                        new Util.ComboItem<Settings.RunAfter.RunAfterWhen>(Settings.RunAfter.RunAfterWhen.LoadedCharacterSelect, "Loaded the game"),
                    });
            }
            else
            {
                comboRunAfter.Items.AddRange(new object[]
                    {
                        new Util.ComboItem<Settings.RunAfter.RunAfterWhen>(Settings.RunAfter.RunAfterWhen.LoadedLauncher, "Loaded the launcher"),
                        new Util.ComboItem<Settings.RunAfter.RunAfterWhen>(Settings.RunAfter.RunAfterWhen.LoadedCharacterSelect, "Loaded character select"),
                        new Util.ComboItem<Settings.RunAfter.RunAfterWhen>(Settings.RunAfter.RunAfterWhen.LoadedCharacter, "Loaded a character"),
                    });
            }

            comboOnExit.Items.AddRange(new object[]
                {
                    new Util.ComboItem<Settings.RunAfter.RunAfterOptions>(Settings.RunAfter.RunAfterOptions.None, "Do nothing"),
                    new Util.ComboItem<Settings.RunAfter.RunAfterOptions>(Settings.RunAfter.RunAfterOptions.CloseOnExit, "Close the program"),
                    new Util.ComboItem<Settings.RunAfter.RunAfterOptions>(Settings.RunAfter.RunAfterOptions.KillOnExit, "Forcibly close the program"),
                });
        }

        private void ShowVariables(TextBox text, object label, AnchorStyles anchor, params Client.Variables.VariableType[] type)
        {
            var f = new formVariables(Client.Variables.GetVariables(type));

            f.VariableSelected += delegate(object o, Client.Variables.Variable v)
            {
                text.SelectedText = v.Name;
            };

            f.Show(this, (Control)label, anchor);
        }

        private void buttonPath_Click(object sender, EventArgs e)
        {
            using (var f = new OpenFileDialog())
            {
                f.ValidateNames = false;
                f.Filter = "Executables|*.exe;*.bat|All files|*.*";

                if (textPath.TextLength != 0)
                {
                    try
                    {
                        f.InitialDirectory = System.IO.Path.GetDirectoryName(textPath.Text);
                    }
                    catch { }
                }

                if (f.ShowDialog(this) == System.Windows.Forms.DialogResult.OK)
                {
                    textPath.Text = f.FileName;
                    textPath.Select(textPath.TextLength, 0);
                }
            }
        }

        private void labelArgumentsVariables_Click(object sender, EventArgs e)
        {
            ShowVariables(radioProgram.Checked ? textArguments : textCommands, sender, AnchorStyles.Top, Client.Variables.VariableType.Account, Client.Variables.VariableType.Process);
        }

        public Settings.RunAfter Result
        {
            get;
            private set;
        }

        private void buttonCancel_Click(object sender, EventArgs e)
        {
            this.DialogResult = System.Windows.Forms.DialogResult.Cancel;
        }

        private void buttonOk_Click(object sender, EventArgs e)
        {
            if (radioProgram.Checked)
            {
                if (textPath.TextLength == 0 || !File.Exists(textPath.Text))
                {
                    MessageBox.Show(this, "The selected program is not valid", "Invalid program", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    textPath.Focus();
                    return;
                }
            }

            var flags = Settings.RunAfter.RunAfterOptions.None;
            var when = Util.ComboItem<Settings.RunAfter.RunAfterWhen>.SelectedValue(comboRunAfter, Settings.RunAfter.RunAfterWhen.Manual);

            if (when != Settings.RunAfter.RunAfterWhen.None)
            {
                flags |= Settings.RunAfter.RunAfterOptions.Enabled;
            }

            if (checkWaitUntilComplete.Enabled && checkWaitUntilComplete.Checked)
            {
                flags |= Settings.RunAfter.RunAfterOptions.WaitUntilComplete;
            }

            if (checkUseCurrentUser.Checked)
            {
                flags |= Settings.RunAfter.RunAfterOptions.UseCurrentUser;
            }

            if (radioCommands.Checked)
            {
                this.Result = new Settings.RunAfter(textName.Text, null, textCommands.Text, flags, when);
            }
            else
            {
                flags |= Util.ComboItem<Settings.RunAfter.RunAfterOptions>.SelectedValue(comboOnExit, Settings.RunAfter.RunAfterOptions.None);

                this.Result = new Settings.RunAfter(textName.Text, textPath.Text, textArguments.Text, flags, when);
            }

            this.DialogResult = System.Windows.Forms.DialogResult.OK;
        }

        private void radioTab_CheckedChanged(object sender, EventArgs e)
        {
            if (!((RadioButton)sender).Checked)
                return;

            panelContainer.SuspendLayout();
            panelProgram.Visible = radioProgram.Checked;
            panelCommands.Visible = radioCommands.Checked;
            panelOnExit.Visible = radioProgram.Checked;
            panelContainer.ResumeLayout();
        }

        private void comboRunAfter_SelectedIndexChanged(object sender, EventArgs e)
        {
            var b = false;

            switch (Util.ComboItem<Settings.RunAfter.RunAfterWhen>.SelectedValue(comboRunAfter, Settings.RunAfter.RunAfterWhen.None))
            {
                case Settings.RunAfter.RunAfterWhen.BeforeLaunching:
                case Settings.RunAfter.RunAfterWhen.AfterLaunching:

                    b = true;

                    break;
            }

            checkWaitUntilComplete.Enabled = b;
        }
    }
}
