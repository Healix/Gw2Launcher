namespace Gw2Launcher.UI
{
    partial class formSupport
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
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle3 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle1 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle2 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle6 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle4 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle5 = new System.Windows.Forms.DataGridViewCellStyle();
            this.buttonCancel = new System.Windows.Forms.Button();
            this.buttonOK = new System.Windows.Forms.Button();
            this.panelDiagnostics = new System.Windows.Forms.Panel();
            this.labelDeleteCrashLog = new System.Windows.Forms.Label();
            this.labelOpenCrashLogFolder = new System.Windows.Forms.Label();
            this.label11 = new System.Windows.Forms.Label();
            this.label12 = new System.Windows.Forms.Label();
            this.labelLoggingShowLog = new System.Windows.Forms.Label();
            this.buttonLaunchLog = new System.Windows.Forms.Button();
            this.buttonLaunchDiag = new System.Windows.Forms.Button();
            this.label9 = new System.Windows.Forms.Label();
            this.label10 = new System.Windows.Forms.Label();
            this.label5 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.panelRepair = new System.Windows.Forms.Panel();
            this.buttonReadTest = new System.Windows.Forms.Button();
            this.label13 = new System.Windows.Forms.Label();
            this.label14 = new System.Windows.Forms.Label();
            this.buttonClean = new System.Windows.Forms.Button();
            this.buttonLaunchRepair = new System.Windows.Forms.Button();
            this.label1 = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.label7 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.checkCleanBin = new System.Windows.Forms.CheckBox();
            this.checkCleanEverything = new System.Windows.Forms.CheckBox();
            this.panelAuthentication = new System.Windows.Forms.Panel();
            this.labelLoginServerNoIPs = new System.Windows.Forms.Label();
            this.progressLoginServer = new System.Windows.Forms.ProgressBar();
            this.buttonLoginServerLookup = new System.Windows.Forms.Button();
            this.gridLoginServers = new System.Windows.Forms.DataGridView();
            this.columnLoginServerEnable = new System.Windows.Forms.DataGridViewCheckBoxColumn();
            this.columnLoginServer = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.columnLoginServerRegion = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.columnLoginServerPing = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.columnLoginServerFill = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.label6 = new System.Windows.Forms.Label();
            this.label8 = new System.Windows.Forms.Label();
            this.panelPatching = new System.Windows.Forms.Panel();
            this.labelPatchServerNoIPs = new System.Windows.Forms.Label();
            this.labelPatchServerDns = new System.Windows.Forms.Label();
            this.buttonPatchServerLookup = new System.Windows.Forms.Button();
            this.label17 = new System.Windows.Forms.Label();
            this.label18 = new System.Windows.Forms.Label();
            this.gridPatchServers = new System.Windows.Forms.DataGridView();
            this.columnPatchServerEnable = new System.Windows.Forms.DataGridViewCheckBoxColumn();
            this.columnPatchServer = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.columnPatchServerResponseTime = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.columnPatchServerPing = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.columnPatchServerStatus = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.columnPatchServerFill = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.progressPatchServer = new System.Windows.Forms.ProgressBar();
            this.panelPatchIntercept = new System.Windows.Forms.Panel();
            this.checkPatchInterceptEnable = new System.Windows.Forms.CheckBox();
            this.labelPatchInterceptShowServer = new System.Windows.Forms.Label();
            this.label19 = new System.Windows.Forms.Label();
            this.label20 = new System.Windows.Forms.Label();
            this.sidebarPanel1 = new Gw2Launcher.UI.Controls.SidebarPanel();
            this.buttonPatchingIntercept = new Gw2Launcher.UI.Controls.SidebarButton();
            this.buttonPatching = new Gw2Launcher.UI.Controls.SidebarButton();
            this.buttonAuthentication = new Gw2Launcher.UI.Controls.SidebarButton();
            this.buttonRepair = new Gw2Launcher.UI.Controls.SidebarButton();
            this.buttonDiagnostics = new Gw2Launcher.UI.Controls.SidebarButton();
            this.panelDiagnostics.SuspendLayout();
            this.panelRepair.SuspendLayout();
            this.panelAuthentication.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.gridLoginServers)).BeginInit();
            this.panelPatching.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.gridPatchServers)).BeginInit();
            this.panelPatchIntercept.SuspendLayout();
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
            // panelDiagnostics
            // 
            this.panelDiagnostics.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.panelDiagnostics.Controls.Add(this.labelDeleteCrashLog);
            this.panelDiagnostics.Controls.Add(this.labelOpenCrashLogFolder);
            this.panelDiagnostics.Controls.Add(this.label11);
            this.panelDiagnostics.Controls.Add(this.label12);
            this.panelDiagnostics.Controls.Add(this.labelLoggingShowLog);
            this.panelDiagnostics.Controls.Add(this.buttonLaunchLog);
            this.panelDiagnostics.Controls.Add(this.buttonLaunchDiag);
            this.panelDiagnostics.Controls.Add(this.label9);
            this.panelDiagnostics.Controls.Add(this.label10);
            this.panelDiagnostics.Controls.Add(this.label5);
            this.panelDiagnostics.Controls.Add(this.label2);
            this.panelDiagnostics.Location = new System.Drawing.Point(183, 12);
            this.panelDiagnostics.Name = "panelDiagnostics";
            this.panelDiagnostics.Size = new System.Drawing.Size(373, 302);
            this.panelDiagnostics.TabIndex = 28;
            this.panelDiagnostics.Visible = false;
            // 
            // labelDeleteCrashLog
            // 
            this.labelDeleteCrashLog.AutoSize = true;
            this.labelDeleteCrashLog.Cursor = System.Windows.Forms.Cursors.Hand;
            this.labelDeleteCrashLog.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(49)))), ((int)(((byte)(121)))), ((int)(((byte)(242)))));
            this.labelDeleteCrashLog.Location = new System.Drawing.Point(104, 190);
            this.labelDeleteCrashLog.Name = "labelDeleteCrashLog";
            this.labelDeleteCrashLog.Size = new System.Drawing.Size(39, 13);
            this.labelDeleteCrashLog.TabIndex = 49;
            this.labelDeleteCrashLog.Text = "delete";
            this.labelDeleteCrashLog.Click += new System.EventHandler(this.labelDeleteCrashLog_Click);
            // 
            // labelOpenCrashLogFolder
            // 
            this.labelOpenCrashLogFolder.AutoSize = true;
            this.labelOpenCrashLogFolder.Cursor = System.Windows.Forms.Cursors.Hand;
            this.labelOpenCrashLogFolder.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(49)))), ((int)(((byte)(121)))), ((int)(((byte)(242)))));
            this.labelOpenCrashLogFolder.Location = new System.Drawing.Point(20, 190);
            this.labelOpenCrashLogFolder.Name = "labelOpenCrashLogFolder";
            this.labelOpenCrashLogFolder.Size = new System.Drawing.Size(68, 13);
            this.labelOpenCrashLogFolder.TabIndex = 48;
            this.labelOpenCrashLogFolder.Text = "open folder";
            this.labelOpenCrashLogFolder.Click += new System.EventHandler(this.labelOpenCrashLogFolder_Click);
            // 
            // label11
            // 
            this.label11.AutoSize = true;
            this.label11.Font = new System.Drawing.Font("Segoe UI Semilight", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label11.Location = new System.Drawing.Point(14, 170);
            this.label11.Name = "label11";
            this.label11.Size = new System.Drawing.Size(201, 13);
            this.label11.TabIndex = 47;
            this.label11.Text = "Opens the folder containing ArenaNet.log";
            // 
            // label12
            // 
            this.label12.AutoSize = true;
            this.label12.Font = new System.Drawing.Font("Segoe UI Semibold", 9F, System.Drawing.FontStyle.Bold);
            this.label12.Location = new System.Drawing.Point(13, 154);
            this.label12.Name = "label12";
            this.label12.Size = new System.Drawing.Size(88, 15);
            this.label12.TabIndex = 46;
            this.label12.Text = "Show crash log";
            // 
            // labelLoggingShowLog
            // 
            this.labelLoggingShowLog.AutoSize = true;
            this.labelLoggingShowLog.Cursor = System.Windows.Forms.Cursors.Hand;
            this.labelLoggingShowLog.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(49)))), ((int)(((byte)(121)))), ((int)(((byte)(242)))));
            this.labelLoggingShowLog.Location = new System.Drawing.Point(105, 123);
            this.labelLoggingShowLog.Name = "labelLoggingShowLog";
            this.labelLoggingShowLog.Size = new System.Drawing.Size(55, 13);
            this.labelLoggingShowLog.TabIndex = 45;
            this.labelLoggingShowLog.Text = "show log";
            this.labelLoggingShowLog.Visible = false;
            this.labelLoggingShowLog.Click += new System.EventHandler(this.labelLoggingShowLog_Click);
            // 
            // buttonLaunchLog
            // 
            this.buttonLaunchLog.Font = new System.Drawing.Font("Segoe UI", 8.25F);
            this.buttonLaunchLog.Location = new System.Drawing.Point(20, 118);
            this.buttonLaunchLog.Name = "buttonLaunchLog";
            this.buttonLaunchLog.Size = new System.Drawing.Size(74, 23);
            this.buttonLaunchLog.TabIndex = 44;
            this.buttonLaunchLog.Text = "Launch";
            this.buttonLaunchLog.UseVisualStyleBackColor = true;
            this.buttonLaunchLog.Click += new System.EventHandler(this.buttonLaunchLog_Click);
            // 
            // buttonLaunchDiag
            // 
            this.buttonLaunchDiag.Font = new System.Drawing.Font("Segoe UI", 8.25F);
            this.buttonLaunchDiag.Location = new System.Drawing.Point(20, 46);
            this.buttonLaunchDiag.Name = "buttonLaunchDiag";
            this.buttonLaunchDiag.Size = new System.Drawing.Size(74, 23);
            this.buttonLaunchDiag.TabIndex = 43;
            this.buttonLaunchDiag.Text = "Launch";
            this.buttonLaunchDiag.UseVisualStyleBackColor = true;
            this.buttonLaunchDiag.Click += new System.EventHandler(this.buttonLaunchDiag_Click);
            // 
            // label9
            // 
            this.label9.AutoSize = true;
            this.label9.Font = new System.Drawing.Font("Segoe UI Semilight", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label9.Location = new System.Drawing.Point(14, 98);
            this.label9.Name = "label9";
            this.label9.Size = new System.Drawing.Size(289, 13);
            this.label9.TabIndex = 33;
            this.label9.Text = "Launches using -log, for logging patching and login problems";
            // 
            // label10
            // 
            this.label10.AutoSize = true;
            this.label10.Font = new System.Drawing.Font("Segoe UI Semibold", 9F, System.Drawing.FontStyle.Bold);
            this.label10.Location = new System.Drawing.Point(13, 82);
            this.label10.Name = "label10";
            this.label10.Size = new System.Drawing.Size(51, 15);
            this.label10.TabIndex = 32;
            this.label10.Text = "Logging";
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Font = new System.Drawing.Font("Segoe UI Semilight", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label5.Location = new System.Drawing.Point(14, 26);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(204, 13);
            this.label5.TabIndex = 19;
            this.label5.Text = "Launches using -diag to generate a report";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Font = new System.Drawing.Font("Segoe UI Semibold", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label2.Location = new System.Drawing.Point(13, 10);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(91, 15);
            this.label2.TabIndex = 18;
            this.label2.Text = "Run diagnostics";
            // 
            // panelRepair
            // 
            this.panelRepair.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.panelRepair.Controls.Add(this.buttonReadTest);
            this.panelRepair.Controls.Add(this.label13);
            this.panelRepair.Controls.Add(this.label14);
            this.panelRepair.Controls.Add(this.buttonClean);
            this.panelRepair.Controls.Add(this.buttonLaunchRepair);
            this.panelRepair.Controls.Add(this.label1);
            this.panelRepair.Controls.Add(this.label4);
            this.panelRepair.Controls.Add(this.label7);
            this.panelRepair.Controls.Add(this.label3);
            this.panelRepair.Controls.Add(this.checkCleanBin);
            this.panelRepair.Controls.Add(this.checkCleanEverything);
            this.panelRepair.Location = new System.Drawing.Point(183, 12);
            this.panelRepair.Name = "panelRepair";
            this.panelRepair.Size = new System.Drawing.Size(373, 302);
            this.panelRepair.TabIndex = 35;
            this.panelRepair.Visible = false;
            // 
            // buttonReadTest
            // 
            this.buttonReadTest.Font = new System.Drawing.Font("Segoe UI", 8.25F);
            this.buttonReadTest.Location = new System.Drawing.Point(21, 227);
            this.buttonReadTest.Name = "buttonReadTest";
            this.buttonReadTest.Size = new System.Drawing.Size(74, 23);
            this.buttonReadTest.TabIndex = 53;
            this.buttonReadTest.Text = "Run test";
            this.buttonReadTest.UseVisualStyleBackColor = true;
            this.buttonReadTest.Click += new System.EventHandler(this.buttonReadTest_Click);
            // 
            // label13
            // 
            this.label13.AutoSize = true;
            this.label13.Font = new System.Drawing.Font("Segoe UI Semilight", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label13.Location = new System.Drawing.Point(14, 194);
            this.label13.Name = "label13";
            this.label13.Size = new System.Drawing.Size(316, 26);
            this.label13.TabIndex = 52;
            this.label13.Text = "Processes Gw2.dat for read errors. Attempting to read a corrupted\r\ndrive will cau" +
    "se it to stall";
            // 
            // label14
            // 
            this.label14.AutoSize = true;
            this.label14.Font = new System.Drawing.Font("Segoe UI Semibold", 9F, System.Drawing.FontStyle.Bold);
            this.label14.Location = new System.Drawing.Point(13, 178);
            this.label14.Name = "label14";
            this.label14.Size = new System.Drawing.Size(55, 15);
            this.label14.TabIndex = 51;
            this.label14.Text = "Read test";
            // 
            // buttonClean
            // 
            this.buttonClean.Font = new System.Drawing.Font("Segoe UI", 8.25F);
            this.buttonClean.Location = new System.Drawing.Point(20, 142);
            this.buttonClean.Name = "buttonClean";
            this.buttonClean.Size = new System.Drawing.Size(74, 23);
            this.buttonClean.TabIndex = 45;
            this.buttonClean.Text = "Clean";
            this.buttonClean.UseVisualStyleBackColor = true;
            this.buttonClean.Click += new System.EventHandler(this.buttonClean_Click);
            // 
            // buttonLaunchRepair
            // 
            this.buttonLaunchRepair.Font = new System.Drawing.Font("Segoe UI", 8.25F);
            this.buttonLaunchRepair.Location = new System.Drawing.Point(20, 46);
            this.buttonLaunchRepair.Name = "buttonLaunchRepair";
            this.buttonLaunchRepair.Size = new System.Drawing.Size(74, 23);
            this.buttonLaunchRepair.TabIndex = 44;
            this.buttonLaunchRepair.Text = "Launch";
            this.buttonLaunchRepair.UseVisualStyleBackColor = true;
            this.buttonLaunchRepair.Click += new System.EventHandler(this.buttonLaunchRepair_Click);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("Segoe UI Semilight", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label1.Location = new System.Drawing.Point(14, 98);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(223, 13);
            this.label1.TabIndex = 37;
            this.label1.Text = "Delete any unimportant files in the GW2 folder";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Font = new System.Drawing.Font("Segoe UI Semibold", 9F, System.Drawing.FontStyle.Bold);
            this.label4.Location = new System.Drawing.Point(13, 82);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(101, 15);
            this.label4.TabIndex = 36;
            this.label4.Text = "Directory cleanup";
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Font = new System.Drawing.Font("Segoe UI Semilight", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label7.Location = new System.Drawing.Point(14, 26);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(235, 13);
            this.label7.TabIndex = 21;
            this.label7.Text = "Launches using -repair to repair the data archive";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Font = new System.Drawing.Font("Segoe UI Semibold", 9F, System.Drawing.FontStyle.Bold);
            this.label3.Location = new System.Drawing.Point(13, 10);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(40, 15);
            this.label3.TabIndex = 20;
            this.label3.Text = "Repair";
            // 
            // checkCleanBin
            // 
            this.checkCleanBin.AutoSize = true;
            this.checkCleanBin.Location = new System.Drawing.Point(21, 118);
            this.checkCleanBin.Name = "checkCleanBin";
            this.checkCleanBin.Size = new System.Drawing.Size(123, 17);
            this.checkCleanBin.TabIndex = 48;
            this.checkCleanBin.Text = "Include bin folders";
            this.checkCleanBin.UseVisualStyleBackColor = true;
            this.checkCleanBin.CheckedChanged += new System.EventHandler(this.checkCleanBin_CheckedChanged);
            // 
            // checkCleanEverything
            // 
            this.checkCleanEverything.AutoSize = true;
            this.checkCleanEverything.Location = new System.Drawing.Point(148, 118);
            this.checkCleanEverything.Name = "checkCleanEverything";
            this.checkCleanEverything.Size = new System.Drawing.Size(80, 17);
            this.checkCleanEverything.TabIndex = 49;
            this.checkCleanEverything.Text = "Everything";
            this.checkCleanEverything.UseVisualStyleBackColor = true;
            this.checkCleanEverything.CheckedChanged += new System.EventHandler(this.checkCleanEverything_CheckedChanged);
            // 
            // panelAuthentication
            // 
            this.panelAuthentication.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.panelAuthentication.Controls.Add(this.labelLoginServerNoIPs);
            this.panelAuthentication.Controls.Add(this.progressLoginServer);
            this.panelAuthentication.Controls.Add(this.buttonLoginServerLookup);
            this.panelAuthentication.Controls.Add(this.gridLoginServers);
            this.panelAuthentication.Controls.Add(this.label6);
            this.panelAuthentication.Controls.Add(this.label8);
            this.panelAuthentication.Location = new System.Drawing.Point(183, 12);
            this.panelAuthentication.Name = "panelAuthentication";
            this.panelAuthentication.Size = new System.Drawing.Size(373, 302);
            this.panelAuthentication.TabIndex = 36;
            this.panelAuthentication.Visible = false;
            // 
            // labelLoginServerNoIPs
            // 
            this.labelLoginServerNoIPs.AutoSize = true;
            this.labelLoginServerNoIPs.Location = new System.Drawing.Point(129, 161);
            this.labelLoginServerNoIPs.Name = "labelLoginServerNoIPs";
            this.labelLoginServerNoIPs.Size = new System.Drawing.Size(120, 13);
            this.labelLoginServerNoIPs.TabIndex = 54;
            this.labelLoginServerNoIPs.Text = "Unable to find any IPs";
            this.labelLoginServerNoIPs.Visible = false;
            // 
            // progressLoginServer
            // 
            this.progressLoginServer.Location = new System.Drawing.Point(110, 47);
            this.progressLoginServer.Name = "progressLoginServer";
            this.progressLoginServer.Size = new System.Drawing.Size(144, 21);
            this.progressLoginServer.Style = System.Windows.Forms.ProgressBarStyle.Continuous;
            this.progressLoginServer.TabIndex = 53;
            this.progressLoginServer.Visible = false;
            // 
            // buttonLoginServerLookup
            // 
            this.buttonLoginServerLookup.Font = new System.Drawing.Font("Segoe UI", 8.25F);
            this.buttonLoginServerLookup.Location = new System.Drawing.Point(20, 46);
            this.buttonLoginServerLookup.Name = "buttonLoginServerLookup";
            this.buttonLoginServerLookup.Size = new System.Drawing.Size(74, 23);
            this.buttonLoginServerLookup.TabIndex = 51;
            this.buttonLoginServerLookup.Text = "Search";
            this.buttonLoginServerLookup.UseVisualStyleBackColor = true;
            this.buttonLoginServerLookup.Click += new System.EventHandler(this.buttonLoginServerLookup_Click);
            // 
            // gridLoginServers
            // 
            this.gridLoginServers.AllowUserToAddRows = false;
            this.gridLoginServers.AllowUserToDeleteRows = false;
            this.gridLoginServers.AllowUserToResizeColumns = false;
            this.gridLoginServers.AllowUserToResizeRows = false;
            this.gridLoginServers.BackgroundColor = System.Drawing.SystemColors.Control;
            this.gridLoginServers.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.gridLoginServers.CellBorderStyle = System.Windows.Forms.DataGridViewCellBorderStyle.None;
            this.gridLoginServers.ColumnHeadersBorderStyle = System.Windows.Forms.DataGridViewHeaderBorderStyle.None;
            this.gridLoginServers.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.gridLoginServers.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.columnLoginServerEnable,
            this.columnLoginServer,
            this.columnLoginServerRegion,
            this.columnLoginServerPing,
            this.columnLoginServerFill});
            dataGridViewCellStyle3.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle3.BackColor = System.Drawing.SystemColors.Control;
            dataGridViewCellStyle3.Font = new System.Drawing.Font("Segoe UI", 8.25F);
            dataGridViewCellStyle3.ForeColor = System.Drawing.SystemColors.ControlText;
            dataGridViewCellStyle3.SelectionBackColor = System.Drawing.SystemColors.Highlight;
            dataGridViewCellStyle3.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            dataGridViewCellStyle3.WrapMode = System.Windows.Forms.DataGridViewTriState.False;
            this.gridLoginServers.DefaultCellStyle = dataGridViewCellStyle3;
            this.gridLoginServers.EditMode = System.Windows.Forms.DataGridViewEditMode.EditProgrammatically;
            this.gridLoginServers.Location = new System.Drawing.Point(21, 85);
            this.gridLoginServers.MultiSelect = false;
            this.gridLoginServers.Name = "gridLoginServers";
            this.gridLoginServers.ReadOnly = true;
            this.gridLoginServers.RowHeadersVisible = false;
            this.gridLoginServers.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.gridLoginServers.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.gridLoginServers.Size = new System.Drawing.Size(326, 196);
            this.gridLoginServers.TabIndex = 52;
            this.gridLoginServers.Visible = false;
            this.gridLoginServers.CellContentClick += new System.Windows.Forms.DataGridViewCellEventHandler(this.gridLoginServers_CellContentClick);
            this.gridLoginServers.SelectionChanged += new System.EventHandler(this.gridLoginServers_SelectionChanged);
            this.gridLoginServers.SortCompare += new System.Windows.Forms.DataGridViewSortCompareEventHandler(this.gridLoginServers_SortCompare);
            // 
            // columnLoginServerEnable
            // 
            this.columnLoginServerEnable.HeaderText = "";
            this.columnLoginServerEnable.Name = "columnLoginServerEnable";
            this.columnLoginServerEnable.ReadOnly = true;
            this.columnLoginServerEnable.ToolTipText = "Use the selected server";
            this.columnLoginServerEnable.Width = 25;
            // 
            // columnLoginServer
            // 
            this.columnLoginServer.HeaderText = "Address";
            this.columnLoginServer.Name = "columnLoginServer";
            this.columnLoginServer.ReadOnly = true;
            // 
            // columnLoginServerRegion
            // 
            dataGridViewCellStyle1.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleCenter;
            this.columnLoginServerRegion.DefaultCellStyle = dataGridViewCellStyle1;
            this.columnLoginServerRegion.HeaderText = "Region";
            this.columnLoginServerRegion.Name = "columnLoginServerRegion";
            this.columnLoginServerRegion.ReadOnly = true;
            this.columnLoginServerRegion.Width = 70;
            // 
            // columnLoginServerPing
            // 
            dataGridViewCellStyle2.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleCenter;
            this.columnLoginServerPing.DefaultCellStyle = dataGridViewCellStyle2;
            this.columnLoginServerPing.HeaderText = "Ping";
            this.columnLoginServerPing.Name = "columnLoginServerPing";
            this.columnLoginServerPing.ReadOnly = true;
            this.columnLoginServerPing.ToolTipText = "Network response time";
            this.columnLoginServerPing.Width = 70;
            // 
            // columnLoginServerFill
            // 
            this.columnLoginServerFill.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
            this.columnLoginServerFill.HeaderText = "";
            this.columnLoginServerFill.Name = "columnLoginServerFill";
            this.columnLoginServerFill.ReadOnly = true;
            this.columnLoginServerFill.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable;
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Font = new System.Drawing.Font("Segoe UI Semilight", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label6.Location = new System.Drawing.Point(14, 26);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(198, 13);
            this.label6.TabIndex = 31;
            this.label6.Text = "Force the use of the selected login server";
            // 
            // label8
            // 
            this.label8.AutoSize = true;
            this.label8.Font = new System.Drawing.Font("Segoe UI Semibold", 9F, System.Drawing.FontStyle.Bold);
            this.label8.Location = new System.Drawing.Point(13, 10);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(119, 15);
            this.label8.TabIndex = 30;
            this.label8.Text = "Authorization servers";
            // 
            // panelPatching
            // 
            this.panelPatching.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.panelPatching.Controls.Add(this.labelPatchServerNoIPs);
            this.panelPatching.Controls.Add(this.labelPatchServerDns);
            this.panelPatching.Controls.Add(this.buttonPatchServerLookup);
            this.panelPatching.Controls.Add(this.label17);
            this.panelPatching.Controls.Add(this.label18);
            this.panelPatching.Controls.Add(this.gridPatchServers);
            this.panelPatching.Controls.Add(this.progressPatchServer);
            this.panelPatching.Location = new System.Drawing.Point(183, 12);
            this.panelPatching.Name = "panelPatching";
            this.panelPatching.Size = new System.Drawing.Size(373, 302);
            this.panelPatching.TabIndex = 37;
            this.panelPatching.Visible = false;
            // 
            // labelPatchServerNoIPs
            // 
            this.labelPatchServerNoIPs.AutoSize = true;
            this.labelPatchServerNoIPs.Location = new System.Drawing.Point(129, 174);
            this.labelPatchServerNoIPs.Name = "labelPatchServerNoIPs";
            this.labelPatchServerNoIPs.Size = new System.Drawing.Size(120, 13);
            this.labelPatchServerNoIPs.TabIndex = 50;
            this.labelPatchServerNoIPs.Text = "Unable to find any IPs";
            this.labelPatchServerNoIPs.Visible = false;
            // 
            // labelPatchServerDns
            // 
            this.labelPatchServerDns.AutoSize = true;
            this.labelPatchServerDns.Cursor = System.Windows.Forms.Cursors.Hand;
            this.labelPatchServerDns.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(49)))), ((int)(((byte)(121)))), ((int)(((byte)(242)))));
            this.labelPatchServerDns.Location = new System.Drawing.Point(105, 64);
            this.labelPatchServerDns.Name = "labelPatchServerDns";
            this.labelPatchServerDns.Size = new System.Drawing.Size(82, 13);
            this.labelPatchServerDns.TabIndex = 46;
            this.labelPatchServerDns.Text = "configure DNS";
            this.labelPatchServerDns.Click += new System.EventHandler(this.labelPatchServerDns_Click);
            // 
            // buttonPatchServerLookup
            // 
            this.buttonPatchServerLookup.Font = new System.Drawing.Font("Segoe UI", 8.25F);
            this.buttonPatchServerLookup.Location = new System.Drawing.Point(20, 59);
            this.buttonPatchServerLookup.Name = "buttonPatchServerLookup";
            this.buttonPatchServerLookup.Size = new System.Drawing.Size(74, 23);
            this.buttonPatchServerLookup.TabIndex = 42;
            this.buttonPatchServerLookup.Text = "Search";
            this.buttonPatchServerLookup.UseVisualStyleBackColor = true;
            this.buttonPatchServerLookup.Click += new System.EventHandler(this.buttonPatchServerLookup_Click);
            // 
            // label17
            // 
            this.label17.AutoSize = true;
            this.label17.Font = new System.Drawing.Font("Segoe UI Semilight", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label17.Location = new System.Drawing.Point(14, 26);
            this.label17.Name = "label17";
            this.label17.Size = new System.Drawing.Size(333, 26);
            this.label17.TabIndex = 31;
            this.label17.Text = "Force the use of the selected server when updating the client. Servers\r\nare not g" +
    "uaranteed to be available, causing a connection error if used";
            // 
            // label18
            // 
            this.label18.AutoSize = true;
            this.label18.Font = new System.Drawing.Font("Segoe UI Semibold", 9F, System.Drawing.FontStyle.Bold);
            this.label18.Location = new System.Drawing.Point(13, 10);
            this.label18.Name = "label18";
            this.label18.Size = new System.Drawing.Size(74, 15);
            this.label18.TabIndex = 30;
            this.label18.Text = "Asset servers";
            // 
            // gridPatchServers
            // 
            this.gridPatchServers.AllowUserToAddRows = false;
            this.gridPatchServers.AllowUserToDeleteRows = false;
            this.gridPatchServers.AllowUserToResizeColumns = false;
            this.gridPatchServers.AllowUserToResizeRows = false;
            this.gridPatchServers.BackgroundColor = System.Drawing.SystemColors.Control;
            this.gridPatchServers.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.gridPatchServers.CellBorderStyle = System.Windows.Forms.DataGridViewCellBorderStyle.None;
            this.gridPatchServers.ColumnHeadersBorderStyle = System.Windows.Forms.DataGridViewHeaderBorderStyle.None;
            this.gridPatchServers.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.gridPatchServers.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.columnPatchServerEnable,
            this.columnPatchServer,
            this.columnPatchServerResponseTime,
            this.columnPatchServerPing,
            this.columnPatchServerStatus,
            this.columnPatchServerFill});
            dataGridViewCellStyle6.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle6.BackColor = System.Drawing.SystemColors.Control;
            dataGridViewCellStyle6.Font = new System.Drawing.Font("Segoe UI", 8.25F);
            dataGridViewCellStyle6.ForeColor = System.Drawing.SystemColors.ControlText;
            dataGridViewCellStyle6.SelectionBackColor = System.Drawing.SystemColors.Highlight;
            dataGridViewCellStyle6.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            dataGridViewCellStyle6.WrapMode = System.Windows.Forms.DataGridViewTriState.False;
            this.gridPatchServers.DefaultCellStyle = dataGridViewCellStyle6;
            this.gridPatchServers.EditMode = System.Windows.Forms.DataGridViewEditMode.EditProgrammatically;
            this.gridPatchServers.Location = new System.Drawing.Point(21, 98);
            this.gridPatchServers.MultiSelect = false;
            this.gridPatchServers.Name = "gridPatchServers";
            this.gridPatchServers.ReadOnly = true;
            this.gridPatchServers.RowHeadersVisible = false;
            this.gridPatchServers.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.gridPatchServers.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.gridPatchServers.Size = new System.Drawing.Size(326, 183);
            this.gridPatchServers.TabIndex = 48;
            this.gridPatchServers.Visible = false;
            this.gridPatchServers.CellContentClick += new System.Windows.Forms.DataGridViewCellEventHandler(this.gridPatchServers_CellContentClick);
            this.gridPatchServers.SelectionChanged += new System.EventHandler(this.gridPatchServers_SelectionChanged);
            this.gridPatchServers.SortCompare += new System.Windows.Forms.DataGridViewSortCompareEventHandler(this.gridPatchServers_SortCompare);
            // 
            // columnPatchServerEnable
            // 
            this.columnPatchServerEnable.HeaderText = "";
            this.columnPatchServerEnable.Name = "columnPatchServerEnable";
            this.columnPatchServerEnable.ReadOnly = true;
            this.columnPatchServerEnable.ToolTipText = "Use the selected server";
            this.columnPatchServerEnable.Width = 25;
            // 
            // columnPatchServer
            // 
            this.columnPatchServer.HeaderText = "Address";
            this.columnPatchServer.Name = "columnPatchServer";
            this.columnPatchServer.ReadOnly = true;
            // 
            // columnPatchServerResponseTime
            // 
            dataGridViewCellStyle4.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleCenter;
            this.columnPatchServerResponseTime.DefaultCellStyle = dataGridViewCellStyle4;
            this.columnPatchServerResponseTime.HeaderText = "Response";
            this.columnPatchServerResponseTime.Name = "columnPatchServerResponseTime";
            this.columnPatchServerResponseTime.ReadOnly = true;
            this.columnPatchServerResponseTime.ToolTipText = "Web request response time";
            this.columnPatchServerResponseTime.Width = 70;
            // 
            // columnPatchServerPing
            // 
            dataGridViewCellStyle5.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleCenter;
            this.columnPatchServerPing.DefaultCellStyle = dataGridViewCellStyle5;
            this.columnPatchServerPing.HeaderText = "Ping";
            this.columnPatchServerPing.Name = "columnPatchServerPing";
            this.columnPatchServerPing.ReadOnly = true;
            this.columnPatchServerPing.ToolTipText = "Network response time";
            this.columnPatchServerPing.Width = 70;
            // 
            // columnPatchServerStatus
            // 
            this.columnPatchServerStatus.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
            this.columnPatchServerStatus.HeaderText = "Status";
            this.columnPatchServerStatus.Name = "columnPatchServerStatus";
            this.columnPatchServerStatus.ReadOnly = true;
            this.columnPatchServerStatus.Visible = false;
            // 
            // columnPatchServerFill
            // 
            this.columnPatchServerFill.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
            this.columnPatchServerFill.HeaderText = "";
            this.columnPatchServerFill.Name = "columnPatchServerFill";
            this.columnPatchServerFill.ReadOnly = true;
            this.columnPatchServerFill.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable;
            // 
            // progressPatchServer
            // 
            this.progressPatchServer.Location = new System.Drawing.Point(110, 60);
            this.progressPatchServer.Name = "progressPatchServer";
            this.progressPatchServer.Size = new System.Drawing.Size(144, 21);
            this.progressPatchServer.Style = System.Windows.Forms.ProgressBarStyle.Continuous;
            this.progressPatchServer.TabIndex = 49;
            this.progressPatchServer.Visible = false;
            this.progressPatchServer.VisibleChanged += new System.EventHandler(this.progressPatchServer_VisibleChanged);
            // 
            // panelPatchIntercept
            // 
            this.panelPatchIntercept.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.panelPatchIntercept.Controls.Add(this.checkPatchInterceptEnable);
            this.panelPatchIntercept.Controls.Add(this.labelPatchInterceptShowServer);
            this.panelPatchIntercept.Controls.Add(this.label19);
            this.panelPatchIntercept.Controls.Add(this.label20);
            this.panelPatchIntercept.Location = new System.Drawing.Point(183, 12);
            this.panelPatchIntercept.Name = "panelPatchIntercept";
            this.panelPatchIntercept.Size = new System.Drawing.Size(373, 302);
            this.panelPatchIntercept.TabIndex = 38;
            this.panelPatchIntercept.Visible = false;
            // 
            // checkPatchInterceptEnable
            // 
            this.checkPatchInterceptEnable.AutoSize = true;
            this.checkPatchInterceptEnable.Location = new System.Drawing.Point(22, 72);
            this.checkPatchInterceptEnable.Name = "checkPatchInterceptEnable";
            this.checkPatchInterceptEnable.Size = new System.Drawing.Size(181, 17);
            this.checkPatchInterceptEnable.TabIndex = 49;
            this.checkPatchInterceptEnable.Text = "Enable local asset server proxy";
            this.checkPatchInterceptEnable.UseVisualStyleBackColor = true;
            this.checkPatchInterceptEnable.CheckedChanged += new System.EventHandler(this.checkPatchInterceptEnable_CheckedChanged);
            // 
            // labelPatchInterceptShowServer
            // 
            this.labelPatchInterceptShowServer.AutoSize = true;
            this.labelPatchInterceptShowServer.Cursor = System.Windows.Forms.Cursors.Hand;
            this.labelPatchInterceptShowServer.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(49)))), ((int)(((byte)(121)))), ((int)(((byte)(242)))));
            this.labelPatchInterceptShowServer.Location = new System.Drawing.Point(209, 73);
            this.labelPatchInterceptShowServer.Name = "labelPatchInterceptShowServer";
            this.labelPatchInterceptShowServer.Size = new System.Drawing.Size(68, 13);
            this.labelPatchInterceptShowServer.TabIndex = 46;
            this.labelPatchInterceptShowServer.Text = "show server";
            this.labelPatchInterceptShowServer.Click += new System.EventHandler(this.labelPatchInterceptShowServer_Click);
            // 
            // label19
            // 
            this.label19.AutoSize = true;
            this.label19.Font = new System.Drawing.Font("Segoe UI Semilight", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label19.Location = new System.Drawing.Point(14, 26);
            this.label19.Name = "label19";
            this.label19.Size = new System.Drawing.Size(332, 39);
            this.label19.TabIndex = 31;
            this.label19.Text = "Locally redirects the asset server to allow for additional information to \r\nbe vi" +
    "ewed. Downloads will also be cached to speed up patching when\r\nusing multiple ac" +
    "counts";
            // 
            // label20
            // 
            this.label20.AutoSize = true;
            this.label20.Font = new System.Drawing.Font("Segoe UI Semibold", 9F, System.Drawing.FontStyle.Bold);
            this.label20.Location = new System.Drawing.Point(13, 10);
            this.label20.Name = "label20";
            this.label20.Size = new System.Drawing.Size(102, 15);
            this.label20.TabIndex = 30;
            this.label20.Text = "Asset server proxy";
            // 
            // sidebarPanel1
            // 
            this.sidebarPanel1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left)));
            this.sidebarPanel1.BackColor = System.Drawing.Color.White;
            this.sidebarPanel1.BorderColor = System.Drawing.SystemColors.ControlDark;
            this.sidebarPanel1.Controls.Add(this.buttonPatchingIntercept);
            this.sidebarPanel1.Controls.Add(this.buttonPatching);
            this.sidebarPanel1.Controls.Add(this.buttonAuthentication);
            this.sidebarPanel1.Controls.Add(this.buttonRepair);
            this.sidebarPanel1.Controls.Add(this.buttonDiagnostics);
            this.sidebarPanel1.Location = new System.Drawing.Point(0, 0);
            this.sidebarPanel1.Margin = new System.Windows.Forms.Padding(0);
            this.sidebarPanel1.Name = "sidebarPanel1";
            this.sidebarPanel1.Size = new System.Drawing.Size(180, 370);
            this.sidebarPanel1.TabIndex = 25;
            // 
            // buttonPatchingIntercept
            // 
            this.buttonPatchingIntercept.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonPatchingIntercept.ArrowColor = System.Drawing.SystemColors.Control;
            this.buttonPatchingIntercept.BackColor = System.Drawing.Color.White;
            this.buttonPatchingIntercept.BorderColor = System.Drawing.SystemColors.ControlDark;
            this.buttonPatchingIntercept.Cursor = System.Windows.Forms.Cursors.Hand;
            this.buttonPatchingIntercept.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.buttonPatchingIntercept.Location = new System.Drawing.Point(0, 200);
            this.buttonPatchingIntercept.Margin = new System.Windows.Forms.Padding(0);
            this.buttonPatchingIntercept.Name = "buttonPatchingIntercept";
            this.buttonPatchingIntercept.Selected = false;
            this.buttonPatchingIntercept.SelectedColor = System.Drawing.SystemColors.Control;
            this.buttonPatchingIntercept.Size = new System.Drawing.Size(180, 40);
            this.buttonPatchingIntercept.TabIndex = 7;
            this.buttonPatchingIntercept.Text = "Servers / Proxy";
            // 
            // buttonPatching
            // 
            this.buttonPatching.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonPatching.ArrowColor = System.Drawing.SystemColors.Control;
            this.buttonPatching.BackColor = System.Drawing.Color.White;
            this.buttonPatching.BorderColor = System.Drawing.SystemColors.ControlDark;
            this.buttonPatching.Cursor = System.Windows.Forms.Cursors.Hand;
            this.buttonPatching.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.buttonPatching.Location = new System.Drawing.Point(0, 160);
            this.buttonPatching.Margin = new System.Windows.Forms.Padding(0);
            this.buttonPatching.Name = "buttonPatching";
            this.buttonPatching.Selected = false;
            this.buttonPatching.SelectedColor = System.Drawing.SystemColors.Control;
            this.buttonPatching.Size = new System.Drawing.Size(180, 40);
            this.buttonPatching.TabIndex = 6;
            this.buttonPatching.Text = "Servers / Patching";
            // 
            // buttonAuthentication
            // 
            this.buttonAuthentication.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonAuthentication.ArrowColor = System.Drawing.SystemColors.Control;
            this.buttonAuthentication.BackColor = System.Drawing.Color.White;
            this.buttonAuthentication.BorderColor = System.Drawing.SystemColors.ControlDark;
            this.buttonAuthentication.Cursor = System.Windows.Forms.Cursors.Hand;
            this.buttonAuthentication.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.buttonAuthentication.Location = new System.Drawing.Point(0, 120);
            this.buttonAuthentication.Margin = new System.Windows.Forms.Padding(0);
            this.buttonAuthentication.Name = "buttonAuthentication";
            this.buttonAuthentication.Selected = false;
            this.buttonAuthentication.SelectedColor = System.Drawing.SystemColors.Control;
            this.buttonAuthentication.Size = new System.Drawing.Size(180, 40);
            this.buttonAuthentication.TabIndex = 5;
            this.buttonAuthentication.Text = "Servers / Authentication";
            // 
            // buttonRepair
            // 
            this.buttonRepair.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonRepair.ArrowColor = System.Drawing.SystemColors.Control;
            this.buttonRepair.BackColor = System.Drawing.Color.White;
            this.buttonRepair.BorderColor = System.Drawing.SystemColors.ControlDark;
            this.buttonRepair.Cursor = System.Windows.Forms.Cursors.Hand;
            this.buttonRepair.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.buttonRepair.Location = new System.Drawing.Point(0, 80);
            this.buttonRepair.Margin = new System.Windows.Forms.Padding(0);
            this.buttonRepair.Name = "buttonRepair";
            this.buttonRepair.Selected = false;
            this.buttonRepair.SelectedColor = System.Drawing.SystemColors.Control;
            this.buttonRepair.Size = new System.Drawing.Size(180, 40);
            this.buttonRepair.TabIndex = 4;
            this.buttonRepair.Text = "Repair";
            // 
            // buttonDiagnostics
            // 
            this.buttonDiagnostics.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonDiagnostics.ArrowColor = System.Drawing.SystemColors.Control;
            this.buttonDiagnostics.BackColor = System.Drawing.Color.White;
            this.buttonDiagnostics.BorderColor = System.Drawing.SystemColors.ControlDark;
            this.buttonDiagnostics.Cursor = System.Windows.Forms.Cursors.Hand;
            this.buttonDiagnostics.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.buttonDiagnostics.Location = new System.Drawing.Point(0, 40);
            this.buttonDiagnostics.Margin = new System.Windows.Forms.Padding(0);
            this.buttonDiagnostics.Name = "buttonDiagnostics";
            this.buttonDiagnostics.Selected = false;
            this.buttonDiagnostics.SelectedColor = System.Drawing.SystemColors.Control;
            this.buttonDiagnostics.Size = new System.Drawing.Size(180, 40);
            this.buttonDiagnostics.TabIndex = 3;
            this.buttonDiagnostics.Text = "Diagnostics";
            // 
            // formSupport
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(568, 370);
            this.Controls.Add(this.buttonCancel);
            this.Controls.Add(this.buttonOK);
            this.Controls.Add(this.sidebarPanel1);
            this.Controls.Add(this.panelDiagnostics);
            this.Controls.Add(this.panelPatchIntercept);
            this.Controls.Add(this.panelPatching);
            this.Controls.Add(this.panelAuthentication);
            this.Controls.Add(this.panelRepair);
            this.Font = new System.Drawing.Font("Segoe UI", 8.25F);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.Icon = global::Gw2Launcher.Properties.Resources.Gw2;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "formSupport";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.formSupport_FormClosing);
            this.Load += new System.EventHandler(this.formSupport_Load);
            this.panelDiagnostics.ResumeLayout(false);
            this.panelDiagnostics.PerformLayout();
            this.panelRepair.ResumeLayout(false);
            this.panelRepair.PerformLayout();
            this.panelAuthentication.ResumeLayout(false);
            this.panelAuthentication.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.gridLoginServers)).EndInit();
            this.panelPatching.ResumeLayout(false);
            this.panelPatching.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.gridPatchServers)).EndInit();
            this.panelPatchIntercept.ResumeLayout(false);
            this.panelPatchIntercept.PerformLayout();
            this.sidebarPanel1.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Button buttonCancel;
        private System.Windows.Forms.Button buttonOK;
        private Controls.SidebarPanel sidebarPanel1;
        private Controls.SidebarButton buttonAuthentication;
        private Controls.SidebarButton buttonRepair;
        private Controls.SidebarButton buttonDiagnostics;
        private System.Windows.Forms.Panel panelDiagnostics;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label9;
        private System.Windows.Forms.Label label10;
        private System.Windows.Forms.Panel panelRepair;
        private System.Windows.Forms.Panel panelAuthentication;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.Label label8;
        private Controls.SidebarButton buttonPatching;
        private System.Windows.Forms.Panel panelPatching;
        private System.Windows.Forms.Label label17;
        private System.Windows.Forms.Label label18;
        private System.Windows.Forms.Button buttonPatchServerLookup;
        private System.Windows.Forms.Button buttonLaunchLog;
        private System.Windows.Forms.Button buttonLaunchDiag;
        private System.Windows.Forms.Label labelLoggingShowLog;
        private System.Windows.Forms.Button buttonClean;
        private System.Windows.Forms.Button buttonLaunchRepair;
        private System.Windows.Forms.CheckBox checkCleanBin;
        private System.Windows.Forms.CheckBox checkCleanEverything;
        private System.Windows.Forms.Button buttonReadTest;
        private System.Windows.Forms.Label label13;
        private System.Windows.Forms.Label label14;
        private System.Windows.Forms.Label labelPatchServerDns;
        private System.Windows.Forms.DataGridView gridPatchServers;
        private System.Windows.Forms.ProgressBar progressPatchServer;
        private System.Windows.Forms.Label labelPatchServerNoIPs;
        private System.Windows.Forms.Label labelDeleteCrashLog;
        private System.Windows.Forms.Label labelOpenCrashLogFolder;
        private System.Windows.Forms.Label label11;
        private System.Windows.Forms.Label label12;
        private System.Windows.Forms.Label labelLoginServerNoIPs;
        private System.Windows.Forms.ProgressBar progressLoginServer;
        private System.Windows.Forms.Button buttonLoginServerLookup;
        private System.Windows.Forms.DataGridView gridLoginServers;
        private Controls.SidebarButton buttonPatchingIntercept;
        private System.Windows.Forms.Panel panelPatchIntercept;
        private System.Windows.Forms.CheckBox checkPatchInterceptEnable;
        private System.Windows.Forms.Label labelPatchInterceptShowServer;
        private System.Windows.Forms.Label label19;
        private System.Windows.Forms.Label label20;
        private System.Windows.Forms.DataGridViewCheckBoxColumn columnLoginServerEnable;
        private System.Windows.Forms.DataGridViewTextBoxColumn columnLoginServer;
        private System.Windows.Forms.DataGridViewTextBoxColumn columnLoginServerRegion;
        private System.Windows.Forms.DataGridViewTextBoxColumn columnLoginServerPing;
        private System.Windows.Forms.DataGridViewTextBoxColumn columnLoginServerFill;
        private System.Windows.Forms.DataGridViewCheckBoxColumn columnPatchServerEnable;
        private System.Windows.Forms.DataGridViewTextBoxColumn columnPatchServer;
        private System.Windows.Forms.DataGridViewTextBoxColumn columnPatchServerResponseTime;
        private System.Windows.Forms.DataGridViewTextBoxColumn columnPatchServerPing;
        private System.Windows.Forms.DataGridViewTextBoxColumn columnPatchServerStatus;
        private System.Windows.Forms.DataGridViewTextBoxColumn columnPatchServerFill;
    }
}