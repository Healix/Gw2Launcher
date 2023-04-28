namespace Gw2Launcher.UI
{
    partial class formBackgroundPatcher
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
            this.label4 = new System.Windows.Forms.Label();
            this.checkEnabled = new System.Windows.Forms.CheckBox();
            this.labelDownloaded = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.labelSize = new System.Windows.Forms.Label();
            this.labelFiles = new System.Windows.Forms.Label();
            this.labelSizeLabel = new System.Windows.Forms.Label();
            this.label26 = new System.Windows.Forms.Label();
            this.labelStatus = new Controls.LinkLabel();
            this.progressDownload = new System.Windows.Forms.ProgressBar();
            this.panelLang = new System.Windows.Forms.Panel();
            this.labelLang4 = new Controls.LinkLabel();
            this.labelLang3 = new Controls.LinkLabel();
            this.labelLang2 = new Controls.LinkLabel();
            this.labelLang1 = new Controls.LinkLabel();
            this.labelSizeEstimated = new System.Windows.Forms.Label();
            this.labelTime = new System.Windows.Forms.Label();
            this.labelRecheckManifests = new Controls.LinkLabel();
            this.labelSpeedLimit = new System.Windows.Forms.Label();
            this.checkSpeedLimit = new System.Windows.Forms.CheckBox();
            this.label3 = new System.Windows.Forms.Label();
            this.labelThreads = new System.Windows.Forms.Label();
            this.label21 = new System.Windows.Forms.Label();
            this.labelSizeContent = new System.Windows.Forms.Label();
            this.labelSizeContentValue = new System.Windows.Forms.Label();
            this.labelSizeCore = new System.Windows.Forms.Label();
            this.labelSizeCoreValue = new System.Windows.Forms.Label();
            this.checkUseHttps = new System.Windows.Forms.CheckBox();
            this.label2 = new System.Windows.Forms.Label();
            this.label5 = new System.Windows.Forms.Label();
            this.sliderThreads = new Gw2Launcher.UI.Controls.FlatSlider();
            this.sliderSpeedLimit = new Gw2Launcher.UI.Controls.FlatSlider();
            this.panelAdvanced = new Gw2Launcher.UI.Controls.ScrollablePanelContainer();
            this.labelAdvanced = new Gw2Launcher.UI.Controls.LabelButton();
            this.labelLang = new Gw2Launcher.UI.Controls.LabelButton();
            this.panelAdvancedContent = new Gw2Launcher.UI.Controls.StackPanel();
            this.stackPanel2 = new Gw2Launcher.UI.Controls.StackPanel();
            this.stackPanel3 = new Gw2Launcher.UI.Controls.StackPanel();
            this.panelLang.SuspendLayout();
            this.panelAdvancedContent.SuspendLayout();
            this.stackPanel2.SuspendLayout();
            this.stackPanel3.SuspendLayout();
            this.SuspendLayout();
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Font = new System.Drawing.Font("Segoe UI Semilight", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label4.Location = new System.Drawing.Point(10, 35);
            this.label4.Size = new System.Drawing.Size(35, 13);
            this.label4.Text = "Status";
            // 
            // checkEnabled
            // 
            this.checkEnabled.AutoSize = true;
            this.checkEnabled.Location = new System.Drawing.Point(10, 10);
            this.checkEnabled.Size = new System.Drawing.Size(176, 17);
            this.checkEnabled.Text = "Enable background patching";
            this.checkEnabled.UseVisualStyleBackColor = true;
            this.checkEnabled.CheckedChanged += new System.EventHandler(this.checkEnabled_CheckedChanged);
            this.checkEnabled.Click += new System.EventHandler(this.checkEnabled_Click);
            // 
            // labelDownloaded
            // 
            this.labelDownloaded.AutoSize = true;
            this.labelDownloaded.Font = new System.Drawing.Font("Segoe UI Semilight", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.labelDownloaded.Location = new System.Drawing.Point(89, 89);
            this.labelDownloaded.Size = new System.Drawing.Size(19, 13);
            this.labelDownloaded.Text = "---";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("Segoe UI Semilight", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label1.Location = new System.Drawing.Point(10, 89);
            this.label1.Size = new System.Drawing.Size(67, 13);
            this.label1.Text = "Downloaded";
            // 
            // labelSize
            // 
            this.labelSize.AutoSize = true;
            this.labelSize.Font = new System.Drawing.Font("Segoe UI Semilight", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.labelSize.Location = new System.Drawing.Point(89, 71);
            this.labelSize.Size = new System.Drawing.Size(19, 13);
            this.labelSize.Text = "---";
            this.labelSize.SizeChanged += new System.EventHandler(this.labelSize_SizeChanged);
            // 
            // labelFiles
            // 
            this.labelFiles.AutoSize = true;
            this.labelFiles.Font = new System.Drawing.Font("Segoe UI Semilight", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.labelFiles.Location = new System.Drawing.Point(89, 53);
            this.labelFiles.Size = new System.Drawing.Size(19, 13);
            this.labelFiles.Text = "---";
            this.labelFiles.MouseEnter += new System.EventHandler(this.labelFiles_MouseEnter);
            // 
            // labelSizeLabel
            // 
            this.labelSizeLabel.AutoSize = true;
            this.labelSizeLabel.Font = new System.Drawing.Font("Segoe UI Semilight", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.labelSizeLabel.Location = new System.Drawing.Point(10, 71);
            this.labelSizeLabel.Size = new System.Drawing.Size(56, 13);
            this.labelSizeLabel.Text = "Remaining";
            // 
            // label26
            // 
            this.label26.AutoSize = true;
            this.label26.Font = new System.Drawing.Font("Segoe UI Semilight", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label26.Location = new System.Drawing.Point(10, 53);
            this.label26.Size = new System.Drawing.Size(26, 13);
            this.label26.Text = "Files";
            // 
            // labelStatus
            // 
            this.labelStatus.AutoSize = true;
            this.labelStatus.Font = new System.Drawing.Font("Segoe UI Semilight", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.labelStatus.Location = new System.Drawing.Point(89, 35);
            this.labelStatus.Size = new System.Drawing.Size(19, 13);
            this.labelStatus.Text = "---";
            this.labelStatus.SizeChanged += new System.EventHandler(this.labelStatus_SizeChanged);
            this.labelStatus.Click += new System.EventHandler(this.labelStatus_Click);
            // 
            // progressDownload
            // 
            this.progressDownload.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.progressDownload.Location = new System.Drawing.Point(11, 125);
            this.progressDownload.Size = new System.Drawing.Size(479, 31);
            // 
            // panelLang
            // 
            this.panelLang.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.panelLang.Controls.Add(this.labelLang4);
            this.panelLang.Controls.Add(this.labelLang3);
            this.panelLang.Controls.Add(this.labelLang2);
            this.panelLang.Controls.Add(this.labelLang1);
            this.panelLang.Location = new System.Drawing.Point(192, 8);
            this.panelLang.Size = new System.Drawing.Size(33, 70);
            this.panelLang.Visible = false;
            // 
            // labelLang4
            // 
            this.labelLang4.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.labelLang4.Font = new System.Drawing.Font("Segoe UI Semilight", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.labelLang4.Location = new System.Drawing.Point(3, 51);
            this.labelLang4.Size = new System.Drawing.Size(25, 13);
            this.labelLang4.Text = "EN";
            this.labelLang4.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // labelLang3
            // 
            this.labelLang3.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.labelLang3.Font = new System.Drawing.Font("Segoe UI Semilight", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.labelLang3.Location = new System.Drawing.Point(3, 35);
            this.labelLang3.Size = new System.Drawing.Size(25, 13);
            this.labelLang3.Text = "EN";
            this.labelLang3.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // labelLang2
            // 
            this.labelLang2.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.labelLang2.Font = new System.Drawing.Font("Segoe UI Semilight", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.labelLang2.Location = new System.Drawing.Point(3, 19);
            this.labelLang2.Size = new System.Drawing.Size(25, 13);
            this.labelLang2.Text = "EN";
            this.labelLang2.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // labelLang1
            // 
            this.labelLang1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.labelLang1.Font = new System.Drawing.Font("Segoe UI Semilight", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.labelLang1.Location = new System.Drawing.Point(3, 3);
            this.labelLang1.Size = new System.Drawing.Size(25, 13);
            this.labelLang1.Text = "EN";
            this.labelLang1.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // labelSizeEstimated
            // 
            this.labelSizeEstimated.AutoSize = true;
            this.labelSizeEstimated.Font = new System.Drawing.Font("Segoe UI Semilight", 8.25F);
            this.labelSizeEstimated.ForeColor = System.Drawing.SystemColors.GrayText;
            this.labelSizeEstimated.Location = new System.Drawing.Point(113, 71);
            this.labelSizeEstimated.Size = new System.Drawing.Size(58, 13);
            this.labelSizeEstimated.Text = "(estimated)";
            this.labelSizeEstimated.Visible = false;
            // 
            // labelTime
            // 
            this.labelTime.AutoSize = true;
            this.labelTime.Font = new System.Drawing.Font("Segoe UI Semilight", 8.25F);
            this.labelTime.ForeColor = System.Drawing.SystemColors.GrayText;
            this.labelTime.Location = new System.Drawing.Point(114, 35);
            this.labelTime.Size = new System.Drawing.Size(11, 13);
            this.labelTime.Text = "-";
            // 
            // labelRecheckManifests
            // 
            this.labelRecheckManifests.AutoSize = true;
            this.labelRecheckManifests.Font = new System.Drawing.Font("Segoe UI Semilight", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.labelRecheckManifests.Location = new System.Drawing.Point(13, 173);
            this.labelRecheckManifests.Size = new System.Drawing.Size(27, 13);
            this.labelRecheckManifests.Text = "start";
            this.labelRecheckManifests.Click += new System.EventHandler(this.labelRecheckManifests_Click);
            // 
            // labelSpeedLimit
            // 
            this.labelSpeedLimit.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.labelSpeedLimit.AutoSize = true;
            this.labelSpeedLimit.Font = new System.Drawing.Font("Segoe UI Semilight", 8.25F);
            this.labelSpeedLimit.Location = new System.Drawing.Point(181, 4);
            this.labelSpeedLimit.Margin = new System.Windows.Forms.Padding(6, 0, 0, 0);
            this.labelSpeedLimit.Size = new System.Drawing.Size(44, 13);
            this.labelSpeedLimit.Text = "10 MB/s";
            // 
            // checkSpeedLimit
            // 
            this.checkSpeedLimit.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.checkSpeedLimit.AutoSize = true;
            this.checkSpeedLimit.Font = new System.Drawing.Font("Segoe UI", 8.25F);
            this.checkSpeedLimit.Location = new System.Drawing.Point(8, 3);
            this.checkSpeedLimit.Margin = new System.Windows.Forms.Padding(8, 0, 0, 0);
            this.checkSpeedLimit.Size = new System.Drawing.Size(15, 14);
            this.checkSpeedLimit.UseVisualStyleBackColor = true;
            this.checkSpeedLimit.CheckedChanged += new System.EventHandler(this.checkSpeedLimit_CheckedChanged);
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Font = new System.Drawing.Font("Segoe UI Semilight", 8.25F);
            this.label3.Location = new System.Drawing.Point(10, 59);
            this.label3.Margin = new System.Windows.Forms.Padding(0, 10, 0, 6);
            this.label3.Size = new System.Drawing.Size(107, 13);
            this.label3.Text = "Download speed limit";
            // 
            // labelThreads
            // 
            this.labelThreads.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.labelThreads.AutoSize = true;
            this.labelThreads.Font = new System.Drawing.Font("Segoe UI Semilight", 8.25F);
            this.labelThreads.Location = new System.Drawing.Point(113, 4);
            this.labelThreads.Margin = new System.Windows.Forms.Padding(6, 0, 0, 0);
            this.labelThreads.Size = new System.Drawing.Size(17, 13);
            this.labelThreads.Text = "10";
            // 
            // label21
            // 
            this.label21.AutoSize = true;
            this.label21.Font = new System.Drawing.Font("Segoe UI Semilight", 8.25F);
            this.label21.Location = new System.Drawing.Point(10, 10);
            this.label21.Margin = new System.Windows.Forms.Padding(0, 0, 0, 6);
            this.label21.Size = new System.Drawing.Size(170, 13);
            this.label21.Text = "Maximum simultaneous downloads";
            // 
            // labelSizeContent
            // 
            this.labelSizeContent.AutoSize = true;
            this.labelSizeContent.Font = new System.Drawing.Font("Segoe UI Semilight", 8.25F);
            this.labelSizeContent.ForeColor = System.Drawing.SystemColors.GrayText;
            this.labelSizeContent.Location = new System.Drawing.Point(89, 71);
            this.labelSizeContent.Size = new System.Drawing.Size(42, 13);
            this.labelSizeContent.Text = "content";
            this.labelSizeContent.Visible = false;
            // 
            // labelSizeContentValue
            // 
            this.labelSizeContentValue.AutoSize = true;
            this.labelSizeContentValue.Font = new System.Drawing.Font("Segoe UI Semilight", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.labelSizeContentValue.Location = new System.Drawing.Point(131, 71);
            this.labelSizeContentValue.Size = new System.Drawing.Size(40, 13);
            this.labelSizeContentValue.Text = "0 bytes";
            this.labelSizeContentValue.Visible = false;
            this.labelSizeContentValue.SizeChanged += new System.EventHandler(this.labelSizeContentValue_SizeChanged);
            // 
            // labelSizeCore
            // 
            this.labelSizeCore.AutoSize = true;
            this.labelSizeCore.Font = new System.Drawing.Font("Segoe UI Semilight", 8.25F);
            this.labelSizeCore.ForeColor = System.Drawing.SystemColors.GrayText;
            this.labelSizeCore.Location = new System.Drawing.Point(171, 71);
            this.labelSizeCore.Size = new System.Drawing.Size(28, 13);
            this.labelSizeCore.Text = "core";
            this.labelSizeCore.Visible = false;
            // 
            // labelSizeCoreValue
            // 
            this.labelSizeCoreValue.AutoSize = true;
            this.labelSizeCoreValue.Font = new System.Drawing.Font("Segoe UI Semilight", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.labelSizeCoreValue.Location = new System.Drawing.Point(199, 71);
            this.labelSizeCoreValue.Size = new System.Drawing.Size(40, 13);
            this.labelSizeCoreValue.Text = "0 bytes";
            this.labelSizeCoreValue.Visible = false;
            // 
            // checkUseHttps
            // 
            this.checkUseHttps.AutoSize = true;
            this.checkUseHttps.Font = new System.Drawing.Font("Segoe UI Semilight", 8.25F);
            this.checkUseHttps.Location = new System.Drawing.Point(18, 127);
            this.checkUseHttps.Margin = new System.Windows.Forms.Padding(8, 0, 0, 0);
            this.checkUseHttps.Size = new System.Drawing.Size(78, 17);
            this.checkUseHttps.Text = "Use HTTPS";
            this.checkUseHttps.UseVisualStyleBackColor = true;
            this.checkUseHttps.Click += new System.EventHandler(this.checkUseHttps_Click);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Font = new System.Drawing.Font("Segoe UI Semilight", 8.25F);
            this.label2.Location = new System.Drawing.Point(10, 154);
            this.label2.Margin = new System.Windows.Forms.Padding(0, 10, 0, 6);
            this.label2.Size = new System.Drawing.Size(105, 13);
            this.label2.Text = "Recheck all manifests";
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Font = new System.Drawing.Font("Segoe UI Semilight", 8.25F);
            this.label5.Location = new System.Drawing.Point(10, 108);
            this.label5.Margin = new System.Windows.Forms.Padding(0, 10, 0, 6);
            this.label5.Size = new System.Drawing.Size(56, 13);
            this.label5.Text = "Encryption";
            // 
            // sliderThreads
            // 
            this.sliderThreads.Location = new System.Drawing.Point(7, 0);
            this.sliderThreads.Margin = new System.Windows.Forms.Padding(7, 0, 0, 0);
            this.sliderThreads.Size = new System.Drawing.Size(100, 20);
            this.sliderThreads.TabStop = false;
            this.sliderThreads.Value = 0.5F;
            this.sliderThreads.ValueChanged += new System.EventHandler(this.sliderThreads_ValueChanged);
            // 
            // sliderSpeedLimit
            // 
            this.sliderSpeedLimit.Enabled = false;
            this.sliderSpeedLimit.Location = new System.Drawing.Point(30, 0);
            this.sliderSpeedLimit.Margin = new System.Windows.Forms.Padding(7, 0, 0, 0);
            this.sliderSpeedLimit.Size = new System.Drawing.Size(145, 20);
            this.sliderSpeedLimit.TabStop = false;
            this.sliderSpeedLimit.Value = 1F;
            this.sliderSpeedLimit.ValueChanged += new System.EventHandler(this.sliderSpeedLimit_ValueChanged);
            // 
            // panelAdvanced
            // 
            this.panelAdvanced.Location = new System.Drawing.Point(229, 8);
            this.panelAdvanced.Margin = new System.Windows.Forms.Padding(48, 22, 48, 22);
            this.panelAdvanced.Size = new System.Drawing.Size(261, 153);
            this.panelAdvanced.Visible = false;
            // 
            // labelAdvanced
            // 
            this.labelAdvanced.AutoSize = true;
            this.labelAdvanced.Cursor = Windows.Cursors.Hand;
            this.labelAdvanced.Font = new System.Drawing.Font("Segoe UI Semilight", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.labelAdvanced.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(49)))), ((int)(((byte)(121)))), ((int)(((byte)(242)))));
            this.labelAdvanced.Location = new System.Drawing.Point(229, 12);
            this.labelAdvanced.Padding = new System.Windows.Forms.Padding(6, 0, 6, 0);
            this.labelAdvanced.Size = new System.Drawing.Size(65, 13);
            this.labelAdvanced.Text = "advanced";
            this.labelAdvanced.Click += new System.EventHandler(this.labelAdvanced_Click);
            // 
            // labelLang
            // 
            this.labelLang.AutoSize = true;
            this.labelLang.Cursor = Windows.Cursors.Hand;
            this.labelLang.Font = new System.Drawing.Font("Segoe UI Semilight", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.labelLang.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(49)))), ((int)(((byte)(121)))), ((int)(((byte)(242)))));
            this.labelLang.Location = new System.Drawing.Point(192, 12);
            this.labelLang.Padding = new System.Windows.Forms.Padding(6, 0, 6, 0);
            this.labelLang.Size = new System.Drawing.Size(33, 13);
            this.labelLang.Text = "EN";
            this.labelLang.Click += new System.EventHandler(this.labelLang_Click);
            // 
            // panelAdvancedContent
            // 
            this.panelAdvancedContent.AutoSize = true;
            this.panelAdvancedContent.Controls.Add(this.label21);
            this.panelAdvancedContent.Controls.Add(this.stackPanel2);
            this.panelAdvancedContent.Controls.Add(this.label3);
            this.panelAdvancedContent.Controls.Add(this.stackPanel3);
            this.panelAdvancedContent.Controls.Add(this.label5);
            this.panelAdvancedContent.Controls.Add(this.checkUseHttps);
            this.panelAdvancedContent.Controls.Add(this.label2);
            this.panelAdvancedContent.Controls.Add(this.labelRecheckManifests);
            this.panelAdvancedContent.Location = new System.Drawing.Point(232, 12);
            this.panelAdvancedContent.Margin = new System.Windows.Forms.Padding(0);
            this.panelAdvancedContent.Padding = new System.Windows.Forms.Padding(10);
            this.panelAdvancedContent.Size = new System.Drawing.Size(245, 196);
            // 
            // stackPanel2
            // 
            this.stackPanel2.AutoSize = true;
            this.stackPanel2.AutoSizeFill = Gw2Launcher.UI.Controls.StackPanel.AutoSizeFillMode.NoWrap;
            this.stackPanel2.Controls.Add(this.sliderThreads);
            this.stackPanel2.Controls.Add(this.labelThreads);
            this.stackPanel2.FlowDirection = System.Windows.Forms.FlowDirection.LeftToRight;
            this.stackPanel2.Location = new System.Drawing.Point(10, 29);
            this.stackPanel2.Margin = new System.Windows.Forms.Padding(0);
            this.stackPanel2.Size = new System.Drawing.Size(130, 20);
            // 
            // stackPanel3
            // 
            this.stackPanel3.AutoSize = true;
            this.stackPanel3.AutoSizeFill = Gw2Launcher.UI.Controls.StackPanel.AutoSizeFillMode.NoWrap;
            this.stackPanel3.Controls.Add(this.checkSpeedLimit);
            this.stackPanel3.Controls.Add(this.sliderSpeedLimit);
            this.stackPanel3.Controls.Add(this.labelSpeedLimit);
            this.stackPanel3.FlowDirection = System.Windows.Forms.FlowDirection.LeftToRight;
            this.stackPanel3.Location = new System.Drawing.Point(10, 78);
            this.stackPanel3.Margin = new System.Windows.Forms.Padding(0);
            this.stackPanel3.Size = new System.Drawing.Size(225, 20);
            // 
            // formBackgroundPatcher
            // 
            this.ClientSize = new System.Drawing.Size(505, 169);
            this.Controls.Add(this.panelAdvancedContent);
            this.Controls.Add(this.panelAdvanced);
            this.Controls.Add(this.panelLang);
            this.Controls.Add(this.labelAdvanced);
            this.Controls.Add(this.labelTime);
            this.Controls.Add(this.labelSizeEstimated);
            this.Controls.Add(this.progressDownload);
            this.Controls.Add(this.labelStatus);
            this.Controls.Add(this.labelSize);
            this.Controls.Add(this.labelFiles);
            this.Controls.Add(this.labelSizeLabel);
            this.Controls.Add(this.label26);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.checkEnabled);
            this.Controls.Add(this.labelDownloaded);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.labelLang);
            this.Controls.Add(this.labelSizeCoreValue);
            this.Controls.Add(this.labelSizeCore);
            this.Controls.Add(this.labelSizeContentValue);
            this.Controls.Add(this.labelSizeContent);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.Icon = global::Gw2Launcher.Properties.Resources.Gw2Launcher;
            this.MaximizeBox = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Background Patch Downloader";
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.formBackgroundPatcher_FormClosed);
            this.Load += new System.EventHandler(this.formBackgroundPatcher_Load);
            this.panelLang.ResumeLayout(false);
            this.panelAdvancedContent.ResumeLayout(false);
            this.panelAdvancedContent.PerformLayout();
            this.stackPanel2.ResumeLayout(false);
            this.stackPanel2.PerformLayout();
            this.stackPanel3.ResumeLayout(false);
            this.stackPanel3.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.CheckBox checkEnabled;
        private System.Windows.Forms.Label labelDownloaded;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label labelSize;
        private System.Windows.Forms.Label labelFiles;
        private System.Windows.Forms.Label labelSizeLabel;
        private System.Windows.Forms.Label label26;
        private Controls.LinkLabel labelStatus;
        private System.Windows.Forms.ProgressBar progressDownload;
        private System.Windows.Forms.Panel panelLang;
        private Controls.LinkLabel labelLang1;
        private Controls.LinkLabel labelLang3;
        private Controls.LinkLabel labelLang2;
        private System.Windows.Forms.Label labelSizeEstimated;
        private Controls.LinkLabel labelLang4;
        private System.Windows.Forms.Label labelTime;
        private Controls.LabelButton labelLang;
        private Controls.LabelButton labelAdvanced;
        private System.Windows.Forms.Label labelThreads;
        private System.Windows.Forms.Label label21;
        private Controls.FlatSlider sliderThreads;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label labelSpeedLimit;
        private System.Windows.Forms.CheckBox checkSpeedLimit;
        private Controls.FlatSlider sliderSpeedLimit;
        private System.Windows.Forms.Label labelSizeContent;
        private System.Windows.Forms.Label labelSizeContentValue;
        private System.Windows.Forms.Label labelSizeCore;
        private System.Windows.Forms.Label labelSizeCoreValue;
        private Controls.LinkLabel labelRecheckManifests;
        private System.Windows.Forms.CheckBox checkUseHttps;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label5;
        private Controls.ScrollablePanelContainer panelAdvanced;
        private Controls.StackPanel panelAdvancedContent;
        private Controls.StackPanel stackPanel2;
        private Controls.StackPanel stackPanel3;
    }
}
