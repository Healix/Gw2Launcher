namespace Gw2Launcher.UI
{
    partial class formSettings
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.buttonCancel = new System.Windows.Forms.Button();
            this.buttonOK = new System.Windows.Forms.Button();
            this.panelGeneral = new System.Windows.Forms.Panel();
            this.label13 = new System.Windows.Forms.Label();
            this.label14 = new System.Windows.Forms.Label();
            this.buttonWindowReset = new System.Windows.Forms.Button();
            this.checkBringToFrontOnExit = new System.Windows.Forms.CheckBox();
            this.checkMinimizeToTray = new System.Windows.Forms.CheckBox();
            this.checkShowTrayIcon = new System.Windows.Forms.CheckBox();
            this.label9 = new System.Windows.Forms.Label();
            this.label10 = new System.Windows.Forms.Label();
            this.label5 = new System.Windows.Forms.Label();
            this.textGW2Path = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.buttonGW2Path = new System.Windows.Forms.Button();
            this.panelArguments = new System.Windows.Forms.Panel();
            this.label31 = new System.Windows.Forms.Label();
            this.label30 = new System.Windows.Forms.Label();
            this.label29 = new System.Windows.Forms.Label();
            this.label27 = new System.Windows.Forms.Label();
            this.textRunAfterLaunch = new Gw2Launcher.UI.Controls.ExtendableTextBox();
            this.label28 = new System.Windows.Forms.Label();
            this.labelVolume = new System.Windows.Forms.Label();
            this.label20 = new System.Windows.Forms.Label();
            this.label21 = new System.Windows.Forms.Label();
            this.checkVolume = new System.Windows.Forms.CheckBox();
            this.label7 = new System.Windows.Forms.Label();
            this.textArguments = new System.Windows.Forms.TextBox();
            this.label3 = new System.Windows.Forms.Label();
            this.sliderVolume = new Gw2Launcher.UI.Controls.FlatSlider();
            this.panelPasswords = new System.Windows.Forms.Panel();
            this.buttonClearPasswords = new System.Windows.Forms.Button();
            this.label11 = new System.Windows.Forms.Label();
            this.label12 = new System.Windows.Forms.Label();
            this.checkStoreCredentials = new System.Windows.Forms.CheckBox();
            this.label6 = new System.Windows.Forms.Label();
            this.label8 = new System.Windows.Forms.Label();
            this.panelStyle = new System.Windows.Forms.Panel();
            this.labelFontRestoreDefaults = new System.Windows.Forms.Label();
            this.buttonFontDescriptors = new System.Windows.Forms.Button();
            this.buttonFontTitle = new System.Windows.Forms.Button();
            this.label19 = new System.Windows.Forms.Label();
            this.label16 = new System.Windows.Forms.Label();
            this.checkShowUser = new System.Windows.Forms.CheckBox();
            this.buttonSample = new Gw2Launcher.UI.Controls.AccountGridButton();
            this.label15 = new System.Windows.Forms.Label();
            this.label17 = new System.Windows.Forms.Label();
            this.label18 = new System.Windows.Forms.Label();
            this.panelUpdates = new System.Windows.Forms.Panel();
            this.labelAutoUpdateDownloadNotificationsConfig = new System.Windows.Forms.Label();
            this.checkAutoUpdateDownloadNotifications = new System.Windows.Forms.CheckBox();
            this.labelVersionUpdate = new System.Windows.Forms.Label();
            this.label25 = new System.Windows.Forms.Label();
            this.label24 = new System.Windows.Forms.Label();
            this.label23 = new System.Windows.Forms.Label();
            this.numericUpdateInterval = new System.Windows.Forms.NumericUpDown();
            this.label1 = new System.Windows.Forms.Label();
            this.checkAutoUpdateDownload = new System.Windows.Forms.CheckBox();
            this.checkAutoUpdate = new System.Windows.Forms.CheckBox();
            this.label4 = new System.Windows.Forms.Label();
            this.checkCheckBuildOnLaunch = new System.Windows.Forms.CheckBox();
            this.checkCheckVersionOnStart = new System.Windows.Forms.CheckBox();
            this.label26 = new System.Windows.Forms.Label();
            this.sidebarPanel1 = new Gw2Launcher.UI.Controls.SidebarPanel();
            this.buttonUpdates = new Gw2Launcher.UI.Controls.SidebarButton();
            this.buttonStyle = new Gw2Launcher.UI.Controls.SidebarButton();
            this.buttonPasswords = new Gw2Launcher.UI.Controls.SidebarButton();
            this.buttonArguments = new Gw2Launcher.UI.Controls.SidebarButton();
            this.buttonGeneral = new Gw2Launcher.UI.Controls.SidebarButton();
            this.panelGeneral.SuspendLayout();
            this.panelArguments.SuspendLayout();
            this.panelPasswords.SuspendLayout();
            this.panelStyle.SuspendLayout();
            this.panelUpdates.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpdateInterval)).BeginInit();
            this.sidebarPanel1.SuspendLayout();
            this.SuspendLayout();
            // 
            // buttonCancel
            // 
            this.buttonCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonCancel.Font = new System.Drawing.Font("Segoe UI", 8.25F);
            this.buttonCancel.Location = new System.Drawing.Point(374, 323);
            this.buttonCancel.Name = "buttonCancel";
            this.buttonCancel.Size = new System.Drawing.Size(86, 35);
            this.buttonCancel.TabIndex = 26;
            this.buttonCancel.Text = "Cancel";
            this.buttonCancel.UseVisualStyleBackColor = true;
            this.buttonCancel.Click += new System.EventHandler(this.buttonCancel_Click);
            // 
            // buttonOK
            // 
            this.buttonOK.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonOK.Font = new System.Drawing.Font("Segoe UI", 8.25F);
            this.buttonOK.Location = new System.Drawing.Point(470, 323);
            this.buttonOK.Name = "buttonOK";
            this.buttonOK.Size = new System.Drawing.Size(86, 35);
            this.buttonOK.TabIndex = 27;
            this.buttonOK.Text = "OK";
            this.buttonOK.UseVisualStyleBackColor = true;
            this.buttonOK.Click += new System.EventHandler(this.buttonOK_Click);
            // 
            // panelGeneral
            // 
            this.panelGeneral.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.panelGeneral.Controls.Add(this.label13);
            this.panelGeneral.Controls.Add(this.label14);
            this.panelGeneral.Controls.Add(this.buttonWindowReset);
            this.panelGeneral.Controls.Add(this.checkBringToFrontOnExit);
            this.panelGeneral.Controls.Add(this.checkMinimizeToTray);
            this.panelGeneral.Controls.Add(this.checkShowTrayIcon);
            this.panelGeneral.Controls.Add(this.label9);
            this.panelGeneral.Controls.Add(this.label10);
            this.panelGeneral.Controls.Add(this.label5);
            this.panelGeneral.Controls.Add(this.textGW2Path);
            this.panelGeneral.Controls.Add(this.label2);
            this.panelGeneral.Controls.Add(this.buttonGW2Path);
            this.panelGeneral.Location = new System.Drawing.Point(183, 12);
            this.panelGeneral.Name = "panelGeneral";
            this.panelGeneral.Size = new System.Drawing.Size(373, 302);
            this.panelGeneral.TabIndex = 28;
            this.panelGeneral.Visible = false;
            // 
            // label13
            // 
            this.label13.AutoSize = true;
            this.label13.Font = new System.Drawing.Font("Segoe UI Semilight", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label13.Location = new System.Drawing.Point(14, 188);
            this.label13.Name = "label13";
            this.label13.Size = new System.Drawing.Size(258, 13);
            this.label13.TabIndex = 42;
            this.label13.Text = "Resets the position and automatically sizes the window";
            // 
            // label14
            // 
            this.label14.AutoSize = true;
            this.label14.Font = new System.Drawing.Font("Segoe UI Semibold", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label14.Location = new System.Drawing.Point(13, 172);
            this.label14.Name = "label14";
            this.label14.Size = new System.Drawing.Size(129, 15);
            this.label14.TabIndex = 41;
            this.label14.Text = "Reset launcher window";
            // 
            // buttonWindowReset
            // 
            this.buttonWindowReset.Font = new System.Drawing.Font("Segoe UI", 8.25F);
            this.buttonWindowReset.Location = new System.Drawing.Point(20, 208);
            this.buttonWindowReset.Name = "buttonWindowReset";
            this.buttonWindowReset.Size = new System.Drawing.Size(74, 23);
            this.buttonWindowReset.TabIndex = 40;
            this.buttonWindowReset.Text = "Reset";
            this.buttonWindowReset.UseVisualStyleBackColor = true;
            this.buttonWindowReset.Click += new System.EventHandler(this.buttonWindowReset_Click);
            // 
            // checkBringToFrontOnExit
            // 
            this.checkBringToFrontOnExit.AutoSize = true;
            this.checkBringToFrontOnExit.Font = new System.Drawing.Font("Segoe UI", 8.25F);
            this.checkBringToFrontOnExit.Location = new System.Drawing.Point(21, 142);
            this.checkBringToFrontOnExit.Name = "checkBringToFrontOnExit";
            this.checkBringToFrontOnExit.Size = new System.Drawing.Size(252, 17);
            this.checkBringToFrontOnExit.TabIndex = 34;
            this.checkBringToFrontOnExit.Text = "Bring to front when all processes are closed";
            this.checkBringToFrontOnExit.UseVisualStyleBackColor = true;
            // 
            // checkMinimizeToTray
            // 
            this.checkMinimizeToTray.AutoSize = true;
            this.checkMinimizeToTray.Font = new System.Drawing.Font("Segoe UI", 8.25F);
            this.checkMinimizeToTray.Location = new System.Drawing.Point(134, 119);
            this.checkMinimizeToTray.Name = "checkMinimizeToTray";
            this.checkMinimizeToTray.Size = new System.Drawing.Size(108, 17);
            this.checkMinimizeToTray.TabIndex = 31;
            this.checkMinimizeToTray.Text = "Minimize to tray";
            this.checkMinimizeToTray.UseVisualStyleBackColor = true;
            // 
            // checkShowTrayIcon
            // 
            this.checkShowTrayIcon.AutoSize = true;
            this.checkShowTrayIcon.Font = new System.Drawing.Font("Segoe UI", 8.25F);
            this.checkShowTrayIcon.Location = new System.Drawing.Point(21, 119);
            this.checkShowTrayIcon.Name = "checkShowTrayIcon";
            this.checkShowTrayIcon.Size = new System.Drawing.Size(102, 17);
            this.checkShowTrayIcon.TabIndex = 30;
            this.checkShowTrayIcon.Text = "Show tray icon";
            this.checkShowTrayIcon.UseVisualStyleBackColor = true;
            this.checkShowTrayIcon.CheckedChanged += new System.EventHandler(this.checkShowTrayIcon_CheckedChanged);
            // 
            // label9
            // 
            this.label9.AutoSize = true;
            this.label9.Font = new System.Drawing.Font("Segoe UI Semilight", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label9.Location = new System.Drawing.Point(14, 98);
            this.label9.Name = "label9";
            this.label9.Size = new System.Drawing.Size(237, 13);
            this.label9.TabIndex = 33;
            this.label9.Text = "Show an icon in the area next to the system clock";
            // 
            // label10
            // 
            this.label10.AutoSize = true;
            this.label10.Font = new System.Drawing.Font("Segoe UI Semibold", 9F, System.Drawing.FontStyle.Bold);
            this.label10.Location = new System.Drawing.Point(13, 82);
            this.label10.Name = "label10";
            this.label10.Size = new System.Drawing.Size(95, 15);
            this.label10.TabIndex = 32;
            this.label10.Text = "System tray icon";
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Font = new System.Drawing.Font("Segoe UI Semilight", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label5.Location = new System.Drawing.Point(14, 26);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(137, 13);
            this.label5.TabIndex = 19;
            this.label5.Text = "The path to GW2\'s launcher";
            // 
            // textGW2Path
            // 
            this.textGW2Path.Font = new System.Drawing.Font("Segoe UI", 8.25F);
            this.textGW2Path.Location = new System.Drawing.Point(21, 47);
            this.textGW2Path.Name = "textGW2Path";
            this.textGW2Path.Size = new System.Drawing.Size(286, 22);
            this.textGW2Path.TabIndex = 16;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Font = new System.Drawing.Font("Segoe UI Semibold", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label2.Location = new System.Drawing.Point(13, 10);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(98, 15);
            this.label2.TabIndex = 18;
            this.label2.Text = "Gw2.exe location";
            // 
            // buttonGW2Path
            // 
            this.buttonGW2Path.Font = new System.Drawing.Font("Segoe UI", 8.25F);
            this.buttonGW2Path.Location = new System.Drawing.Point(313, 47);
            this.buttonGW2Path.Name = "buttonGW2Path";
            this.buttonGW2Path.Size = new System.Drawing.Size(43, 23);
            this.buttonGW2Path.TabIndex = 17;
            this.buttonGW2Path.Text = "...";
            this.buttonGW2Path.UseVisualStyleBackColor = true;
            this.buttonGW2Path.Click += new System.EventHandler(this.buttonGW2Path_Click);
            // 
            // panelArguments
            // 
            this.panelArguments.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.panelArguments.Controls.Add(this.label31);
            this.panelArguments.Controls.Add(this.label30);
            this.panelArguments.Controls.Add(this.label29);
            this.panelArguments.Controls.Add(this.label27);
            this.panelArguments.Controls.Add(this.textRunAfterLaunch);
            this.panelArguments.Controls.Add(this.label28);
            this.panelArguments.Controls.Add(this.labelVolume);
            this.panelArguments.Controls.Add(this.label20);
            this.panelArguments.Controls.Add(this.label21);
            this.panelArguments.Controls.Add(this.checkVolume);
            this.panelArguments.Controls.Add(this.label7);
            this.panelArguments.Controls.Add(this.textArguments);
            this.panelArguments.Controls.Add(this.label3);
            this.panelArguments.Controls.Add(this.sliderVolume);
            this.panelArguments.Location = new System.Drawing.Point(183, 12);
            this.panelArguments.Name = "panelArguments";
            this.panelArguments.Size = new System.Drawing.Size(373, 302);
            this.panelArguments.TabIndex = 35;
            this.panelArguments.Visible = false;
            // 
            // label31
            // 
            this.label31.AutoSize = true;
            this.label31.Cursor = System.Windows.Forms.Cursors.Hand;
            this.label31.Font = new System.Drawing.Font("Segoe UI Semibold", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label31.Location = new System.Drawing.Point(149, 180);
            this.label31.Name = "label31";
            this.label31.Size = new System.Drawing.Size(74, 13);
            this.label31.TabIndex = 49;
            this.label31.Text = "%accountid%";
            this.label31.Click += new System.EventHandler(this.label30_Click);
            // 
            // label30
            // 
            this.label30.AutoSize = true;
            this.label30.Cursor = System.Windows.Forms.Cursors.Hand;
            this.label30.Font = new System.Drawing.Font("Segoe UI Semibold", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label30.Location = new System.Drawing.Point(69, 180);
            this.label30.Name = "label30";
            this.label30.Size = new System.Drawing.Size(74, 13);
            this.label30.TabIndex = 48;
            this.label30.Text = "%processid%";
            this.label30.Click += new System.EventHandler(this.label30_Click);
            // 
            // label29
            // 
            this.label29.AutoSize = true;
            this.label29.Font = new System.Drawing.Font("Segoe UI Semilight", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label29.Location = new System.Drawing.Point(14, 180);
            this.label29.Name = "label29";
            this.label29.Size = new System.Drawing.Size(49, 13);
            this.label29.TabIndex = 47;
            this.label29.Text = "Variables";
            // 
            // label27
            // 
            this.label27.AutoSize = true;
            this.label27.Font = new System.Drawing.Font("Segoe UI Semilight", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label27.Location = new System.Drawing.Point(14, 165);
            this.label27.Name = "label27";
            this.label27.Size = new System.Drawing.Size(216, 13);
            this.label27.TabIndex = 46;
            this.label27.Text = "Executes the shell commands after launching";
            // 
            // textRunAfterLaunch
            // 
            this.textRunAfterLaunch.ExtendedSize = new System.Drawing.Size(0, 0);
            this.textRunAfterLaunch.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.textRunAfterLaunch.Location = new System.Drawing.Point(20, 201);
            this.textRunAfterLaunch.MaximumExtendedSize = new System.Drawing.Size(0, 0);
            this.textRunAfterLaunch.Multiline = true;
            this.textRunAfterLaunch.Name = "textRunAfterLaunch";
            this.textRunAfterLaunch.Size = new System.Drawing.Size(335, 51);
            this.textRunAfterLaunch.TabIndex = 44;
            this.textRunAfterLaunch.WordWrap = false;
            // 
            // label28
            // 
            this.label28.AutoSize = true;
            this.label28.Font = new System.Drawing.Font("Segoe UI Semibold", 9F, System.Drawing.FontStyle.Bold);
            this.label28.Location = new System.Drawing.Point(13, 149);
            this.label28.Name = "label28";
            this.label28.Size = new System.Drawing.Size(111, 15);
            this.label28.TabIndex = 45;
            this.label28.Text = "Run after launching";
            // 
            // labelVolume
            // 
            this.labelVolume.AutoSize = true;
            this.labelVolume.Font = new System.Drawing.Font("Segoe UI Semilight", 8.25F);
            this.labelVolume.Location = new System.Drawing.Point(199, 119);
            this.labelVolume.Name = "labelVolume";
            this.labelVolume.Size = new System.Drawing.Size(32, 13);
            this.labelVolume.TabIndex = 43;
            this.labelVolume.Text = "100%";
            // 
            // label20
            // 
            this.label20.AutoSize = true;
            this.label20.Font = new System.Drawing.Font("Segoe UI Semilight", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label20.Location = new System.Drawing.Point(14, 97);
            this.label20.Name = "label20";
            this.label20.Size = new System.Drawing.Size(209, 13);
            this.label20.TabIndex = 40;
            this.label20.Text = "Sets the volume in Windows for the process";
            // 
            // label21
            // 
            this.label21.AutoSize = true;
            this.label21.Font = new System.Drawing.Font("Segoe UI Semibold", 9F, System.Drawing.FontStyle.Bold);
            this.label21.Location = new System.Drawing.Point(13, 81);
            this.label21.Name = "label21";
            this.label21.Size = new System.Drawing.Size(48, 15);
            this.label21.TabIndex = 39;
            this.label21.Text = "Volume";
            // 
            // checkVolume
            // 
            this.checkVolume.AutoSize = true;
            this.checkVolume.Font = new System.Drawing.Font("Segoe UI", 8.25F);
            this.checkVolume.Location = new System.Drawing.Point(21, 119);
            this.checkVolume.Name = "checkVolume";
            this.checkVolume.Size = new System.Drawing.Size(15, 14);
            this.checkVolume.TabIndex = 38;
            this.checkVolume.UseVisualStyleBackColor = true;
            this.checkVolume.CheckedChanged += new System.EventHandler(this.checkVolume_CheckedChanged);
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Font = new System.Drawing.Font("Segoe UI Semilight", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label7.Location = new System.Drawing.Point(14, 26);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(303, 13);
            this.label7.TabIndex = 21;
            this.label7.Text = "Optional command line arguments that will apply to all accounts";
            // 
            // textArguments
            // 
            this.textArguments.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.textArguments.Location = new System.Drawing.Point(21, 46);
            this.textArguments.Name = "textArguments";
            this.textArguments.Size = new System.Drawing.Size(335, 22);
            this.textArguments.TabIndex = 19;
            this.textArguments.Text = "-autologin";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Font = new System.Drawing.Font("Segoe UI Semibold", 9F, System.Drawing.FontStyle.Bold);
            this.label3.Location = new System.Drawing.Point(13, 10);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(66, 15);
            this.label3.TabIndex = 20;
            this.label3.Text = "Arguments";
            // 
            // sliderVolume
            // 
            this.sliderVolume.Enabled = false;
            this.sliderVolume.Location = new System.Drawing.Point(43, 116);
            this.sliderVolume.Name = "sliderVolume";
            this.sliderVolume.Size = new System.Drawing.Size(150, 20);
            this.sliderVolume.TabIndex = 51;
            this.sliderVolume.TabStop = false;
            this.sliderVolume.Value = 1F;
            this.sliderVolume.ValueChanged += new System.EventHandler<float>(this.sliderVolume_ValueChanged);
            // 
            // panelPasswords
            // 
            this.panelPasswords.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.panelPasswords.Controls.Add(this.buttonClearPasswords);
            this.panelPasswords.Controls.Add(this.label11);
            this.panelPasswords.Controls.Add(this.label12);
            this.panelPasswords.Controls.Add(this.checkStoreCredentials);
            this.panelPasswords.Controls.Add(this.label6);
            this.panelPasswords.Controls.Add(this.label8);
            this.panelPasswords.Location = new System.Drawing.Point(183, 12);
            this.panelPasswords.Name = "panelPasswords";
            this.panelPasswords.Size = new System.Drawing.Size(373, 302);
            this.panelPasswords.TabIndex = 36;
            this.panelPasswords.Visible = false;
            // 
            // buttonClearPasswords
            // 
            this.buttonClearPasswords.Font = new System.Drawing.Font("Segoe UI", 8.25F);
            this.buttonClearPasswords.Location = new System.Drawing.Point(21, 127);
            this.buttonClearPasswords.Name = "buttonClearPasswords";
            this.buttonClearPasswords.Size = new System.Drawing.Size(74, 23);
            this.buttonClearPasswords.TabIndex = 34;
            this.buttonClearPasswords.Text = "Clear";
            this.buttonClearPasswords.UseVisualStyleBackColor = true;
            this.buttonClearPasswords.Click += new System.EventHandler(this.buttonClearPasswords_Click);
            // 
            // label11
            // 
            this.label11.AutoSize = true;
            this.label11.Font = new System.Drawing.Font("Segoe UI Semilight", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label11.Location = new System.Drawing.Point(14, 93);
            this.label11.MaximumSize = new System.Drawing.Size(340, 0);
            this.label11.Name = "label11";
            this.label11.Size = new System.Drawing.Size(317, 26);
            this.label11.TabIndex = 33;
            this.label11.Text = "All stored passwords will be cleared, including those cached by the current sessi" +
    "on";
            // 
            // label12
            // 
            this.label12.AutoSize = true;
            this.label12.Font = new System.Drawing.Font("Segoe UI Semibold", 9F, System.Drawing.FontStyle.Bold);
            this.label12.Location = new System.Drawing.Point(13, 77);
            this.label12.Name = "label12";
            this.label12.Size = new System.Drawing.Size(142, 15);
            this.label12.TabIndex = 32;
            this.label12.Text = "Clear all stored passwords";
            // 
            // checkStoreCredentials
            // 
            this.checkStoreCredentials.AutoSize = true;
            this.checkStoreCredentials.Font = new System.Drawing.Font("Segoe UI", 8.25F);
            this.checkStoreCredentials.Location = new System.Drawing.Point(21, 47);
            this.checkStoreCredentials.Name = "checkStoreCredentials";
            this.checkStoreCredentials.Size = new System.Drawing.Size(138, 17);
            this.checkStoreCredentials.TabIndex = 29;
            this.checkStoreCredentials.Text = "Remember passwords";
            this.checkStoreCredentials.UseVisualStyleBackColor = true;
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Font = new System.Drawing.Font("Segoe UI Semilight", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label6.Location = new System.Drawing.Point(14, 26);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(282, 13);
            this.label6.TabIndex = 31;
            this.label6.Text = "Passwords will be saved and encrypted for the current user";
            // 
            // label8
            // 
            this.label8.AutoSize = true;
            this.label8.Font = new System.Drawing.Font("Segoe UI Semibold", 9F, System.Drawing.FontStyle.Bold);
            this.label8.Location = new System.Drawing.Point(13, 10);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(297, 15);
            this.label8.TabIndex = 30;
            this.label8.Text = "Remember username and password for Windows users";
            // 
            // panelStyle
            // 
            this.panelStyle.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.panelStyle.Controls.Add(this.labelFontRestoreDefaults);
            this.panelStyle.Controls.Add(this.buttonFontDescriptors);
            this.panelStyle.Controls.Add(this.buttonFontTitle);
            this.panelStyle.Controls.Add(this.label19);
            this.panelStyle.Controls.Add(this.label16);
            this.panelStyle.Controls.Add(this.checkShowUser);
            this.panelStyle.Controls.Add(this.buttonSample);
            this.panelStyle.Controls.Add(this.label15);
            this.panelStyle.Controls.Add(this.label17);
            this.panelStyle.Controls.Add(this.label18);
            this.panelStyle.Location = new System.Drawing.Point(183, 12);
            this.panelStyle.Name = "panelStyle";
            this.panelStyle.Size = new System.Drawing.Size(373, 302);
            this.panelStyle.TabIndex = 37;
            this.panelStyle.Visible = false;
            // 
            // labelFontRestoreDefaults
            // 
            this.labelFontRestoreDefaults.AutoSize = true;
            this.labelFontRestoreDefaults.Cursor = System.Windows.Forms.Cursors.Hand;
            this.labelFontRestoreDefaults.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(49)))), ((int)(((byte)(121)))), ((int)(((byte)(242)))));
            this.labelFontRestoreDefaults.Location = new System.Drawing.Point(199, 51);
            this.labelFontRestoreDefaults.Name = "labelFontRestoreDefaults";
            this.labelFontRestoreDefaults.Size = new System.Drawing.Size(32, 13);
            this.labelFontRestoreDefaults.TabIndex = 44;
            this.labelFontRestoreDefaults.Text = "reset";
            this.labelFontRestoreDefaults.Visible = false;
            this.labelFontRestoreDefaults.Click += new System.EventHandler(this.labelFontRestoreDefaults_Click);
            // 
            // buttonFontDescriptors
            // 
            this.buttonFontDescriptors.Font = new System.Drawing.Font("Segoe UI", 8.25F);
            this.buttonFontDescriptors.Location = new System.Drawing.Point(114, 46);
            this.buttonFontDescriptors.Name = "buttonFontDescriptors";
            this.buttonFontDescriptors.Size = new System.Drawing.Size(74, 23);
            this.buttonFontDescriptors.TabIndex = 43;
            this.buttonFontDescriptors.Text = "Browse";
            this.buttonFontDescriptors.UseVisualStyleBackColor = true;
            this.buttonFontDescriptors.Click += new System.EventHandler(this.buttonFontDescriptors_Click);
            // 
            // buttonFontTitle
            // 
            this.buttonFontTitle.Font = new System.Drawing.Font("Segoe UI", 8.25F);
            this.buttonFontTitle.Location = new System.Drawing.Point(24, 46);
            this.buttonFontTitle.Name = "buttonFontTitle";
            this.buttonFontTitle.Size = new System.Drawing.Size(74, 23);
            this.buttonFontTitle.TabIndex = 42;
            this.buttonFontTitle.Text = "Browse";
            this.buttonFontTitle.UseVisualStyleBackColor = true;
            this.buttonFontTitle.Click += new System.EventHandler(this.buttonFontTitle_Click);
            // 
            // label19
            // 
            this.label19.AutoSize = true;
            this.label19.Font = new System.Drawing.Font("Segoe UI Semibold", 9F, System.Drawing.FontStyle.Bold);
            this.label19.Location = new System.Drawing.Point(13, 134);
            this.label19.Name = "label19";
            this.label19.Size = new System.Drawing.Size(47, 15);
            this.label19.TabIndex = 41;
            this.label19.Text = "Sample";
            // 
            // label16
            // 
            this.label16.AutoSize = true;
            this.label16.Font = new System.Drawing.Font("Segoe UI Semibold", 9F, System.Drawing.FontStyle.Bold);
            this.label16.Location = new System.Drawing.Point(13, 81);
            this.label16.Name = "label16";
            this.label16.Size = new System.Drawing.Size(67, 15);
            this.label16.TabIndex = 40;
            this.label16.Text = "Descriptors";
            // 
            // checkShowUser
            // 
            this.checkShowUser.AutoSize = true;
            this.checkShowUser.Location = new System.Drawing.Point(24, 104);
            this.checkShowUser.Name = "checkShowUser";
            this.checkShowUser.Size = new System.Drawing.Size(176, 17);
            this.checkShowUser.TabIndex = 39;
            this.checkShowUser.Text = "Show Windows user account";
            this.checkShowUser.UseVisualStyleBackColor = true;
            this.checkShowUser.CheckedChanged += new System.EventHandler(this.checkShowUser_CheckedChanged);
            // 
            // buttonSample
            // 
            this.buttonSample.AccountData = null;
            this.buttonSample.AccountName = "Example";
            this.buttonSample.BackColor = System.Drawing.Color.White;
            this.buttonSample.Cursor = System.Windows.Forms.Cursors.Hand;
            this.buttonSample.DisplayName = "example@example.com";
            this.buttonSample.FontLarge = new System.Drawing.Font("Segoe UI Semibold", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.buttonSample.FontSmall = new System.Drawing.Font("Segoe UI Semilight", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.buttonSample.Index = 0;
            this.buttonSample.LastUsedUtc = new System.DateTime(((long)(0)));
            this.buttonSample.Location = new System.Drawing.Point(24, 157);
            this.buttonSample.MinimumSize = new System.Drawing.Size(0, 67);
            this.buttonSample.Name = "buttonSample";
            this.buttonSample.Selected = false;
            this.buttonSample.ShowAccount = true;
            this.buttonSample.ShowDaily = false;
            this.buttonSample.Size = new System.Drawing.Size(225, 67);
            this.buttonSample.Status = null;
            this.buttonSample.StatusColor = System.Drawing.Color.Empty;
            this.buttonSample.TabIndex = 38;
            // 
            // label15
            // 
            this.label15.AutoSize = true;
            this.label15.Font = new System.Drawing.Font("Segoe UI Semilight", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label15.Location = new System.Drawing.Point(104, 26);
            this.label15.Name = "label15";
            this.label15.Size = new System.Drawing.Size(59, 13);
            this.label15.TabIndex = 35;
            this.label15.Text = "Descriptors";
            // 
            // label17
            // 
            this.label17.AutoSize = true;
            this.label17.Font = new System.Drawing.Font("Segoe UI Semilight", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label17.Location = new System.Drawing.Point(14, 26);
            this.label17.Name = "label17";
            this.label17.Size = new System.Drawing.Size(26, 13);
            this.label17.TabIndex = 31;
            this.label17.Text = "Title";
            // 
            // label18
            // 
            this.label18.AutoSize = true;
            this.label18.Font = new System.Drawing.Font("Segoe UI Semibold", 9F, System.Drawing.FontStyle.Bold);
            this.label18.Location = new System.Drawing.Point(13, 10);
            this.label18.Name = "label18";
            this.label18.Size = new System.Drawing.Size(31, 15);
            this.label18.TabIndex = 30;
            this.label18.Text = "Font";
            // 
            // panelUpdates
            // 
            this.panelUpdates.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.panelUpdates.Controls.Add(this.labelAutoUpdateDownloadNotificationsConfig);
            this.panelUpdates.Controls.Add(this.checkAutoUpdateDownloadNotifications);
            this.panelUpdates.Controls.Add(this.labelVersionUpdate);
            this.panelUpdates.Controls.Add(this.label25);
            this.panelUpdates.Controls.Add(this.label24);
            this.panelUpdates.Controls.Add(this.label23);
            this.panelUpdates.Controls.Add(this.numericUpdateInterval);
            this.panelUpdates.Controls.Add(this.label1);
            this.panelUpdates.Controls.Add(this.checkAutoUpdateDownload);
            this.panelUpdates.Controls.Add(this.checkAutoUpdate);
            this.panelUpdates.Controls.Add(this.label4);
            this.panelUpdates.Controls.Add(this.checkCheckBuildOnLaunch);
            this.panelUpdates.Controls.Add(this.checkCheckVersionOnStart);
            this.panelUpdates.Controls.Add(this.label26);
            this.panelUpdates.Location = new System.Drawing.Point(183, 12);
            this.panelUpdates.Name = "panelUpdates";
            this.panelUpdates.Size = new System.Drawing.Size(373, 302);
            this.panelUpdates.TabIndex = 38;
            this.panelUpdates.Visible = false;
            // 
            // labelAutoUpdateDownloadNotificationsConfig
            // 
            this.labelAutoUpdateDownloadNotificationsConfig.AutoSize = true;
            this.labelAutoUpdateDownloadNotificationsConfig.Cursor = System.Windows.Forms.Cursors.Hand;
            this.labelAutoUpdateDownloadNotificationsConfig.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(49)))), ((int)(((byte)(121)))), ((int)(((byte)(242)))));
            this.labelAutoUpdateDownloadNotificationsConfig.Location = new System.Drawing.Point(148, 176);
            this.labelAutoUpdateDownloadNotificationsConfig.Name = "labelAutoUpdateDownloadNotificationsConfig";
            this.labelAutoUpdateDownloadNotificationsConfig.Size = new System.Drawing.Size(57, 13);
            this.labelAutoUpdateDownloadNotificationsConfig.TabIndex = 72;
            this.labelAutoUpdateDownloadNotificationsConfig.Text = "configure";
            this.labelAutoUpdateDownloadNotificationsConfig.Click += new System.EventHandler(this.labelAutoUpdateDownloadNotificationsConfig_Click);
            // 
            // checkAutoUpdateDownloadNotifications
            // 
            this.checkAutoUpdateDownloadNotifications.AutoSize = true;
            this.checkAutoUpdateDownloadNotifications.Enabled = false;
            this.checkAutoUpdateDownloadNotifications.Location = new System.Drawing.Point(21, 175);
            this.checkAutoUpdateDownloadNotifications.Name = "checkAutoUpdateDownloadNotifications";
            this.checkAutoUpdateDownloadNotifications.Size = new System.Drawing.Size(123, 17);
            this.checkAutoUpdateDownloadNotifications.TabIndex = 71;
            this.checkAutoUpdateDownloadNotifications.Text = "Show notifications";
            this.checkAutoUpdateDownloadNotifications.UseVisualStyleBackColor = true;
            // 
            // labelVersionUpdate
            // 
            this.labelVersionUpdate.AutoSize = true;
            this.labelVersionUpdate.Cursor = System.Windows.Forms.Cursors.Hand;
            this.labelVersionUpdate.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(49)))), ((int)(((byte)(121)))), ((int)(((byte)(242)))));
            this.labelVersionUpdate.Location = new System.Drawing.Point(127, 230);
            this.labelVersionUpdate.Name = "labelVersionUpdate";
            this.labelVersionUpdate.Size = new System.Drawing.Size(70, 13);
            this.labelVersionUpdate.TabIndex = 56;
            this.labelVersionUpdate.Text = "update now";
            this.labelVersionUpdate.Visible = false;
            this.labelVersionUpdate.Click += new System.EventHandler(this.labelVersionUpdate_Click);
            // 
            // label25
            // 
            this.label25.AutoSize = true;
            this.label25.Font = new System.Drawing.Font("Segoe UI Semilight", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label25.ForeColor = System.Drawing.SystemColors.GrayText;
            this.label25.Location = new System.Drawing.Point(171, 156);
            this.label25.Name = "label25";
            this.label25.Size = new System.Drawing.Size(106, 13);
            this.label25.TabIndex = 47;
            this.label25.Text = "(requires asset proxy)";
            // 
            // label24
            // 
            this.label24.AutoSize = true;
            this.label24.Font = new System.Drawing.Font("Segoe UI Semilight", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label24.Location = new System.Drawing.Point(14, 80);
            this.label24.Name = "label24";
            this.label24.Size = new System.Drawing.Size(240, 13);
            this.label24.TabIndex = 46;
            this.label24.Text = "Periodically check for updates at the given interval";
            // 
            // label23
            // 
            this.label23.AutoSize = true;
            this.label23.Font = new System.Drawing.Font("Segoe UI", 8.25F);
            this.label23.Location = new System.Drawing.Point(78, 105);
            this.label23.Name = "label23";
            this.label23.Size = new System.Drawing.Size(48, 13);
            this.label23.TabIndex = 45;
            this.label23.Text = "minutes";
            // 
            // numericUpdateInterval
            // 
            this.numericUpdateInterval.Location = new System.Drawing.Point(21, 101);
            this.numericUpdateInterval.Maximum = new decimal(new int[] {
            1440,
            0,
            0,
            0});
            this.numericUpdateInterval.Minimum = new decimal(new int[] {
            5,
            0,
            0,
            0});
            this.numericUpdateInterval.Name = "numericUpdateInterval";
            this.numericUpdateInterval.Size = new System.Drawing.Size(51, 22);
            this.numericUpdateInterval.TabIndex = 44;
            this.numericUpdateInterval.Value = new decimal(new int[] {
            30,
            0,
            0,
            0});
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("Segoe UI Semibold", 9F, System.Drawing.FontStyle.Bold);
            this.label1.Location = new System.Drawing.Point(13, 64);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(236, 15);
            this.label1.TabIndex = 43;
            this.label1.Text = "Periodically check for Guild Wars 2 updates";
            // 
            // checkAutoUpdateDownload
            // 
            this.checkAutoUpdateDownload.AutoSize = true;
            this.checkAutoUpdateDownload.Font = new System.Drawing.Font("Segoe UI", 8.25F);
            this.checkAutoUpdateDownload.Location = new System.Drawing.Point(21, 155);
            this.checkAutoUpdateDownload.Name = "checkAutoUpdateDownload";
            this.checkAutoUpdateDownload.Size = new System.Drawing.Size(144, 17);
            this.checkAutoUpdateDownload.TabIndex = 42;
            this.checkAutoUpdateDownload.Text = "Pre-download updates";
            this.checkAutoUpdateDownload.UseVisualStyleBackColor = true;
            this.checkAutoUpdateDownload.CheckedChanged += new System.EventHandler(this.checkAutoUpdateDownload_CheckedChanged);
            // 
            // checkAutoUpdate
            // 
            this.checkAutoUpdate.AutoSize = true;
            this.checkAutoUpdate.Font = new System.Drawing.Font("Segoe UI", 8.25F);
            this.checkAutoUpdate.Location = new System.Drawing.Point(21, 135);
            this.checkAutoUpdate.Name = "checkAutoUpdate";
            this.checkAutoUpdate.Size = new System.Drawing.Size(283, 17);
            this.checkAutoUpdate.TabIndex = 41;
            this.checkAutoUpdate.Text = "Automatically launch and update when not in use";
            this.checkAutoUpdate.UseVisualStyleBackColor = true;
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Font = new System.Drawing.Font("Segoe UI Semibold", 9F, System.Drawing.FontStyle.Bold);
            this.label4.Location = new System.Drawing.Point(13, 10);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(229, 15);
            this.label4.TabIndex = 39;
            this.label4.Text = "Check for Guild Wars 2 updates on launch";
            // 
            // checkCheckBuildOnLaunch
            // 
            this.checkCheckBuildOnLaunch.AutoSize = true;
            this.checkCheckBuildOnLaunch.Font = new System.Drawing.Font("Segoe UI", 8.25F);
            this.checkCheckBuildOnLaunch.Location = new System.Drawing.Point(21, 34);
            this.checkCheckBuildOnLaunch.Name = "checkCheckBuildOnLaunch";
            this.checkCheckBuildOnLaunch.Size = new System.Drawing.Size(149, 17);
            this.checkCheckBuildOnLaunch.TabIndex = 38;
            this.checkCheckBuildOnLaunch.Text = "Check before launching";
            this.checkCheckBuildOnLaunch.UseVisualStyleBackColor = true;
            // 
            // checkCheckVersionOnStart
            // 
            this.checkCheckVersionOnStart.AutoSize = true;
            this.checkCheckVersionOnStart.Font = new System.Drawing.Font("Segoe UI", 8.25F);
            this.checkCheckVersionOnStart.Location = new System.Drawing.Point(21, 229);
            this.checkCheckVersionOnStart.Name = "checkCheckVersionOnStart";
            this.checkCheckVersionOnStart.Size = new System.Drawing.Size(100, 17);
            this.checkCheckVersionOnStart.TabIndex = 29;
            this.checkCheckVersionOnStart.Text = "Check on start";
            this.checkCheckVersionOnStart.UseVisualStyleBackColor = true;
            // 
            // label26
            // 
            this.label26.AutoSize = true;
            this.label26.Font = new System.Drawing.Font("Segoe UI Semibold", 9F, System.Drawing.FontStyle.Bold);
            this.label26.Location = new System.Drawing.Point(13, 205);
            this.label26.Name = "label26";
            this.label26.Size = new System.Drawing.Size(213, 15);
            this.label26.TabIndex = 30;
            this.label26.Text = "Periodically check for software updates";
            // 
            // sidebarPanel1
            // 
            this.sidebarPanel1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left)));
            this.sidebarPanel1.BackColor = System.Drawing.Color.White;
            this.sidebarPanel1.BorderColor = System.Drawing.SystemColors.ControlDark;
            this.sidebarPanel1.Controls.Add(this.buttonUpdates);
            this.sidebarPanel1.Controls.Add(this.buttonStyle);
            this.sidebarPanel1.Controls.Add(this.buttonPasswords);
            this.sidebarPanel1.Controls.Add(this.buttonArguments);
            this.sidebarPanel1.Controls.Add(this.buttonGeneral);
            this.sidebarPanel1.Location = new System.Drawing.Point(0, 0);
            this.sidebarPanel1.Margin = new System.Windows.Forms.Padding(0);
            this.sidebarPanel1.Name = "sidebarPanel1";
            this.sidebarPanel1.Size = new System.Drawing.Size(180, 370);
            this.sidebarPanel1.TabIndex = 25;
            // 
            // buttonUpdates
            // 
            this.buttonUpdates.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonUpdates.ArrowColor = System.Drawing.SystemColors.Control;
            this.buttonUpdates.BackColor = System.Drawing.Color.White;
            this.buttonUpdates.BorderColor = System.Drawing.SystemColors.ControlDark;
            this.buttonUpdates.Cursor = System.Windows.Forms.Cursors.Hand;
            this.buttonUpdates.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.buttonUpdates.Location = new System.Drawing.Point(0, 200);
            this.buttonUpdates.Margin = new System.Windows.Forms.Padding(0);
            this.buttonUpdates.Name = "buttonUpdates";
            this.buttonUpdates.Selected = false;
            this.buttonUpdates.SelectedColor = System.Drawing.SystemColors.Control;
            this.buttonUpdates.Size = new System.Drawing.Size(180, 40);
            this.buttonUpdates.TabIndex = 7;
            this.buttonUpdates.Text = "Updates";
            // 
            // buttonStyle
            // 
            this.buttonStyle.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonStyle.ArrowColor = System.Drawing.SystemColors.Control;
            this.buttonStyle.BackColor = System.Drawing.Color.White;
            this.buttonStyle.BorderColor = System.Drawing.SystemColors.ControlDark;
            this.buttonStyle.Cursor = System.Windows.Forms.Cursors.Hand;
            this.buttonStyle.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.buttonStyle.Location = new System.Drawing.Point(0, 160);
            this.buttonStyle.Margin = new System.Windows.Forms.Padding(0);
            this.buttonStyle.Name = "buttonStyle";
            this.buttonStyle.Selected = false;
            this.buttonStyle.SelectedColor = System.Drawing.SystemColors.Control;
            this.buttonStyle.Size = new System.Drawing.Size(180, 40);
            this.buttonStyle.TabIndex = 6;
            this.buttonStyle.Text = "Style";
            // 
            // buttonPasswords
            // 
            this.buttonPasswords.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonPasswords.ArrowColor = System.Drawing.SystemColors.Control;
            this.buttonPasswords.BackColor = System.Drawing.Color.White;
            this.buttonPasswords.BorderColor = System.Drawing.SystemColors.ControlDark;
            this.buttonPasswords.Cursor = System.Windows.Forms.Cursors.Hand;
            this.buttonPasswords.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.buttonPasswords.Location = new System.Drawing.Point(0, 120);
            this.buttonPasswords.Margin = new System.Windows.Forms.Padding(0);
            this.buttonPasswords.Name = "buttonPasswords";
            this.buttonPasswords.Selected = false;
            this.buttonPasswords.SelectedColor = System.Drawing.SystemColors.Control;
            this.buttonPasswords.Size = new System.Drawing.Size(180, 40);
            this.buttonPasswords.TabIndex = 5;
            this.buttonPasswords.Text = "Passwords";
            // 
            // buttonArguments
            // 
            this.buttonArguments.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonArguments.ArrowColor = System.Drawing.SystemColors.Control;
            this.buttonArguments.BackColor = System.Drawing.Color.White;
            this.buttonArguments.BorderColor = System.Drawing.SystemColors.ControlDark;
            this.buttonArguments.Cursor = System.Windows.Forms.Cursors.Hand;
            this.buttonArguments.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.buttonArguments.Location = new System.Drawing.Point(0, 80);
            this.buttonArguments.Margin = new System.Windows.Forms.Padding(0);
            this.buttonArguments.Name = "buttonArguments";
            this.buttonArguments.Selected = false;
            this.buttonArguments.SelectedColor = System.Drawing.SystemColors.Control;
            this.buttonArguments.Size = new System.Drawing.Size(180, 40);
            this.buttonArguments.TabIndex = 4;
            this.buttonArguments.Text = "Launch options";
            // 
            // buttonGeneral
            // 
            this.buttonGeneral.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonGeneral.ArrowColor = System.Drawing.SystemColors.Control;
            this.buttonGeneral.BackColor = System.Drawing.Color.White;
            this.buttonGeneral.BorderColor = System.Drawing.SystemColors.ControlDark;
            this.buttonGeneral.Cursor = System.Windows.Forms.Cursors.Hand;
            this.buttonGeneral.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.buttonGeneral.Location = new System.Drawing.Point(0, 40);
            this.buttonGeneral.Margin = new System.Windows.Forms.Padding(0);
            this.buttonGeneral.Name = "buttonGeneral";
            this.buttonGeneral.Selected = false;
            this.buttonGeneral.SelectedColor = System.Drawing.SystemColors.Control;
            this.buttonGeneral.Size = new System.Drawing.Size(180, 40);
            this.buttonGeneral.TabIndex = 3;
            this.buttonGeneral.Text = "General";
            // 
            // formSettings
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(568, 370);
            this.Controls.Add(this.buttonCancel);
            this.Controls.Add(this.buttonOK);
            this.Controls.Add(this.sidebarPanel1);
            this.Controls.Add(this.panelUpdates);
            this.Controls.Add(this.panelPasswords);
            this.Controls.Add(this.panelArguments);
            this.Controls.Add(this.panelGeneral);
            this.Controls.Add(this.panelStyle);
            this.Font = new System.Drawing.Font("Segoe UI", 8.25F);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.Icon = global::Gw2Launcher.Properties.Resources.Gw2;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "formSettings";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Load += new System.EventHandler(this.formSettings_Load);
            this.panelGeneral.ResumeLayout(false);
            this.panelGeneral.PerformLayout();
            this.panelArguments.ResumeLayout(false);
            this.panelArguments.PerformLayout();
            this.panelPasswords.ResumeLayout(false);
            this.panelPasswords.PerformLayout();
            this.panelStyle.ResumeLayout(false);
            this.panelStyle.PerformLayout();
            this.panelUpdates.ResumeLayout(false);
            this.panelUpdates.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpdateInterval)).EndInit();
            this.sidebarPanel1.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Button buttonCancel;
        private System.Windows.Forms.Button buttonOK;
        private Controls.SidebarPanel sidebarPanel1;
        private Controls.SidebarButton buttonPasswords;
        private Controls.SidebarButton buttonArguments;
        private Controls.SidebarButton buttonGeneral;
        private System.Windows.Forms.Panel panelGeneral;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.TextBox textGW2Path;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Button buttonGW2Path;
        private System.Windows.Forms.CheckBox checkBringToFrontOnExit;
        private System.Windows.Forms.CheckBox checkMinimizeToTray;
        private System.Windows.Forms.CheckBox checkShowTrayIcon;
        private System.Windows.Forms.Label label9;
        private System.Windows.Forms.Label label10;
        private System.Windows.Forms.Panel panelArguments;
        private System.Windows.Forms.Panel panelPasswords;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.TextBox textArguments;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label11;
        private System.Windows.Forms.Label label12;
        private System.Windows.Forms.CheckBox checkStoreCredentials;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.Label label8;
        private System.Windows.Forms.Button buttonClearPasswords;
        private System.Windows.Forms.Label label13;
        private System.Windows.Forms.Label label14;
        private System.Windows.Forms.Button buttonWindowReset;
        private Controls.SidebarButton buttonStyle;
        private System.Windows.Forms.Panel panelStyle;
        private System.Windows.Forms.Label label19;
        private System.Windows.Forms.Label label16;
        private System.Windows.Forms.CheckBox checkShowUser;
        private Controls.AccountGridButton buttonSample;
        private System.Windows.Forms.Label label15;
        private System.Windows.Forms.Label label17;
        private System.Windows.Forms.Label label18;
        private System.Windows.Forms.Button buttonFontDescriptors;
        private System.Windows.Forms.Button buttonFontTitle;
        private System.Windows.Forms.Label labelFontRestoreDefaults;
        private System.Windows.Forms.Label label20;
        private System.Windows.Forms.Label label21;
        private System.Windows.Forms.CheckBox checkVolume;
        private System.Windows.Forms.Label labelVolume;
        private Controls.SidebarButton buttonUpdates;
        private System.Windows.Forms.Panel panelUpdates;
        private System.Windows.Forms.Label label25;
        private System.Windows.Forms.Label label24;
        private System.Windows.Forms.Label label23;
        private System.Windows.Forms.NumericUpDown numericUpdateInterval;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.CheckBox checkAutoUpdateDownload;
        private System.Windows.Forms.CheckBox checkAutoUpdate;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.CheckBox checkCheckBuildOnLaunch;
        private System.Windows.Forms.CheckBox checkCheckVersionOnStart;
        private System.Windows.Forms.Label label26;
        private System.Windows.Forms.Label label31;
        private System.Windows.Forms.Label label30;
        private System.Windows.Forms.Label label29;
        private System.Windows.Forms.Label label27;
        private System.Windows.Forms.Label label28;
        private System.Windows.Forms.Label labelVersionUpdate;
        private Controls.FlatSlider sliderVolume;
        private System.Windows.Forms.CheckBox checkAutoUpdateDownloadNotifications;
        private System.Windows.Forms.Label labelAutoUpdateDownloadNotificationsConfig;
        private Controls.ExtendableTextBox textRunAfterLaunch;
    }
}