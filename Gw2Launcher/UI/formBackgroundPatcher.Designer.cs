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
            this.labelStatus = new System.Windows.Forms.Label();
            this.progressDownload = new System.Windows.Forms.ProgressBar();
            this.panelLang = new System.Windows.Forms.Panel();
            this.labelLang4 = new System.Windows.Forms.Label();
            this.labelLang3 = new System.Windows.Forms.Label();
            this.labelLang2 = new System.Windows.Forms.Label();
            this.labelLang1 = new System.Windows.Forms.Label();
            this.labelSizeEstimated = new System.Windows.Forms.Label();
            this.labelTime = new System.Windows.Forms.Label();
            this.labelRecheckManifests = new System.Windows.Forms.Label();
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
            this.panelAdvancedContent = new System.Windows.Forms.Panel();
            this.sliderThreads = new Gw2Launcher.UI.Controls.FlatSlider();
            this.sliderSpeedLimit = new Gw2Launcher.UI.Controls.FlatSlider();
            this.panelAdvanced = new Gw2Launcher.UI.Controls.ScrollablePanelContainer();
            this.labelAdvanced = new Gw2Launcher.UI.Controls.LabelButton();
            this.labelLang = new Gw2Launcher.UI.Controls.LabelButton();
            this.panelLang.SuspendLayout();
            this.panelAdvancedContent.SuspendLayout();
            this.SuspendLayout();
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Font = new System.Drawing.Font("Segoe UI Semilight", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label4.Location = new System.Drawing.Point(10, 35);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(35, 13);
            this.label4.TabIndex = 58;
            this.label4.Text = "Status";
            // 
            // checkEnabled
            // 
            this.checkEnabled.AutoSize = true;
            this.checkEnabled.Location = new System.Drawing.Point(10, 10);
            this.checkEnabled.Name = "checkEnabled";
            this.checkEnabled.Size = new System.Drawing.Size(176, 17);
            this.checkEnabled.TabIndex = 57;
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
            this.labelDownloaded.Name = "labelDownloaded";
            this.labelDownloaded.Size = new System.Drawing.Size(19, 13);
            this.labelDownloaded.TabIndex = 56;
            this.labelDownloaded.Text = "---";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("Segoe UI Semilight", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label1.Location = new System.Drawing.Point(10, 89);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(67, 13);
            this.label1.TabIndex = 55;
            this.label1.Text = "Downloaded";
            // 
            // labelSize
            // 
            this.labelSize.AutoSize = true;
            this.labelSize.Font = new System.Drawing.Font("Segoe UI Semilight", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.labelSize.Location = new System.Drawing.Point(89, 71);
            this.labelSize.Name = "labelSize";
            this.labelSize.Size = new System.Drawing.Size(19, 13);
            this.labelSize.TabIndex = 63;
            this.labelSize.Text = "---";
            this.labelSize.SizeChanged += new System.EventHandler(this.labelSize_SizeChanged);
            // 
            // labelFiles
            // 
            this.labelFiles.AutoSize = true;
            this.labelFiles.Font = new System.Drawing.Font("Segoe UI Semilight", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.labelFiles.Location = new System.Drawing.Point(89, 53);
            this.labelFiles.Name = "labelFiles";
            this.labelFiles.Size = new System.Drawing.Size(19, 13);
            this.labelFiles.TabIndex = 62;
            this.labelFiles.Text = "---";
            this.labelFiles.MouseEnter += new System.EventHandler(this.labelFiles_MouseEnter);
            // 
            // labelSizeLabel
            // 
            this.labelSizeLabel.AutoSize = true;
            this.labelSizeLabel.Font = new System.Drawing.Font("Segoe UI Semilight", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.labelSizeLabel.Location = new System.Drawing.Point(10, 71);
            this.labelSizeLabel.Name = "labelSizeLabel";
            this.labelSizeLabel.Size = new System.Drawing.Size(56, 13);
            this.labelSizeLabel.TabIndex = 61;
            this.labelSizeLabel.Text = "Remaining";
            // 
            // label26
            // 
            this.label26.AutoSize = true;
            this.label26.Font = new System.Drawing.Font("Segoe UI Semilight", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label26.Location = new System.Drawing.Point(10, 53);
            this.label26.Name = "label26";
            this.label26.Size = new System.Drawing.Size(26, 13);
            this.label26.TabIndex = 60;
            this.label26.Text = "Files";
            // 
            // labelStatus
            // 
            this.labelStatus.AutoSize = true;
            this.labelStatus.Cursor = System.Windows.Forms.Cursors.Hand;
            this.labelStatus.Font = new System.Drawing.Font("Segoe UI Semilight", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.labelStatus.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(49)))), ((int)(((byte)(121)))), ((int)(((byte)(242)))));
            this.labelStatus.Location = new System.Drawing.Point(89, 35);
            this.labelStatus.Name = "labelStatus";
            this.labelStatus.Size = new System.Drawing.Size(19, 13);
            this.labelStatus.TabIndex = 64;
            this.labelStatus.Text = "---";
            this.labelStatus.SizeChanged += new System.EventHandler(this.labelStatus_SizeChanged);
            this.labelStatus.Click += new System.EventHandler(this.labelStatus_Click);
            // 
            // progressDownload
            // 
            this.progressDownload.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.progressDownload.Location = new System.Drawing.Point(11, 125);
            this.progressDownload.Name = "progressDownload";
            this.progressDownload.Size = new System.Drawing.Size(479, 31);
            this.progressDownload.TabIndex = 65;
            // 
            // panelLang
            // 
            this.panelLang.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.panelLang.Controls.Add(this.labelLang4);
            this.panelLang.Controls.Add(this.labelLang3);
            this.panelLang.Controls.Add(this.labelLang2);
            this.panelLang.Controls.Add(this.labelLang1);
            this.panelLang.Location = new System.Drawing.Point(192, 8);
            this.panelLang.Name = "panelLang";
            this.panelLang.Size = new System.Drawing.Size(33, 70);
            this.panelLang.TabIndex = 68;
            this.panelLang.Visible = false;
            // 
            // labelLang4
            // 
            this.labelLang4.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.labelLang4.Cursor = System.Windows.Forms.Cursors.Hand;
            this.labelLang4.Font = new System.Drawing.Font("Segoe UI Semilight", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.labelLang4.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(49)))), ((int)(((byte)(121)))), ((int)(((byte)(242)))));
            this.labelLang4.Location = new System.Drawing.Point(3, 51);
            this.labelLang4.Name = "labelLang4";
            this.labelLang4.Size = new System.Drawing.Size(25, 13);
            this.labelLang4.TabIndex = 70;
            this.labelLang4.Text = "EN";
            this.labelLang4.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // labelLang3
            // 
            this.labelLang3.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.labelLang3.Cursor = System.Windows.Forms.Cursors.Hand;
            this.labelLang3.Font = new System.Drawing.Font("Segoe UI Semilight", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.labelLang3.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(49)))), ((int)(((byte)(121)))), ((int)(((byte)(242)))));
            this.labelLang3.Location = new System.Drawing.Point(3, 35);
            this.labelLang3.Name = "labelLang3";
            this.labelLang3.Size = new System.Drawing.Size(25, 13);
            this.labelLang3.TabIndex = 69;
            this.labelLang3.Text = "EN";
            this.labelLang3.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // labelLang2
            // 
            this.labelLang2.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.labelLang2.Cursor = System.Windows.Forms.Cursors.Hand;
            this.labelLang2.Font = new System.Drawing.Font("Segoe UI Semilight", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.labelLang2.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(49)))), ((int)(((byte)(121)))), ((int)(((byte)(242)))));
            this.labelLang2.Location = new System.Drawing.Point(3, 19);
            this.labelLang2.Name = "labelLang2";
            this.labelLang2.Size = new System.Drawing.Size(25, 13);
            this.labelLang2.TabIndex = 68;
            this.labelLang2.Text = "EN";
            this.labelLang2.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // labelLang1
            // 
            this.labelLang1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.labelLang1.Cursor = System.Windows.Forms.Cursors.Hand;
            this.labelLang1.Font = new System.Drawing.Font("Segoe UI Semilight", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.labelLang1.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(49)))), ((int)(((byte)(121)))), ((int)(((byte)(242)))));
            this.labelLang1.Location = new System.Drawing.Point(3, 3);
            this.labelLang1.Name = "labelLang1";
            this.labelLang1.Size = new System.Drawing.Size(25, 13);
            this.labelLang1.TabIndex = 67;
            this.labelLang1.Text = "EN";
            this.labelLang1.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // labelSizeEstimated
            // 
            this.labelSizeEstimated.AutoSize = true;
            this.labelSizeEstimated.Font = new System.Drawing.Font("Segoe UI Semilight", 8.25F);
            this.labelSizeEstimated.ForeColor = System.Drawing.SystemColors.GrayText;
            this.labelSizeEstimated.Location = new System.Drawing.Point(113, 71);
            this.labelSizeEstimated.Name = "labelSizeEstimated";
            this.labelSizeEstimated.Size = new System.Drawing.Size(58, 13);
            this.labelSizeEstimated.TabIndex = 69;
            this.labelSizeEstimated.Text = "(estimated)";
            this.labelSizeEstimated.Visible = false;
            // 
            // labelTime
            // 
            this.labelTime.AutoSize = true;
            this.labelTime.Font = new System.Drawing.Font("Segoe UI Semilight", 8.25F);
            this.labelTime.ForeColor = System.Drawing.SystemColors.GrayText;
            this.labelTime.Location = new System.Drawing.Point(114, 35);
            this.labelTime.Name = "labelTime";
            this.labelTime.Size = new System.Drawing.Size(11, 13);
            this.labelTime.TabIndex = 70;
            this.labelTime.Text = "-";
            // 
            // labelRecheckManifests
            // 
            this.labelRecheckManifests.AutoSize = true;
            this.labelRecheckManifests.Cursor = System.Windows.Forms.Cursors.Hand;
            this.labelRecheckManifests.Font = new System.Drawing.Font("Segoe UI Semilight", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.labelRecheckManifests.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(49)))), ((int)(((byte)(121)))), ((int)(((byte)(242)))));
            this.labelRecheckManifests.Location = new System.Drawing.Point(11, 170);
            this.labelRecheckManifests.Name = "labelRecheckManifests";
            this.labelRecheckManifests.Size = new System.Drawing.Size(27, 13);
            this.labelRecheckManifests.TabIndex = 65;
            this.labelRecheckManifests.Text = "start";
            this.labelRecheckManifests.Click += new System.EventHandler(this.labelRecheckManifests_Click);
            // 
            // labelSpeedLimit
            // 
            this.labelSpeedLimit.AutoSize = true;
            this.labelSpeedLimit.Font = new System.Drawing.Font("Segoe UI Semilight", 8.25F);
            this.labelSpeedLimit.Location = new System.Drawing.Point(187, 78);
            this.labelSpeedLimit.Name = "labelSpeedLimit";
            this.labelSpeedLimit.Size = new System.Drawing.Size(44, 13);
            this.labelSpeedLimit.TabIndex = 58;
            this.labelSpeedLimit.Text = "10 MB/s";
            // 
            // checkSpeedLimit
            // 
            this.checkSpeedLimit.AutoSize = true;
            this.checkSpeedLimit.Font = new System.Drawing.Font("Segoe UI", 8.25F);
            this.checkSpeedLimit.Location = new System.Drawing.Point(14, 78);
            this.checkSpeedLimit.Name = "checkSpeedLimit";
            this.checkSpeedLimit.Size = new System.Drawing.Size(15, 14);
            this.checkSpeedLimit.TabIndex = 57;
            this.checkSpeedLimit.UseVisualStyleBackColor = true;
            this.checkSpeedLimit.CheckedChanged += new System.EventHandler(this.checkSpeedLimit_CheckedChanged);
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Font = new System.Drawing.Font("Segoe UI Semilight", 8.25F);
            this.label3.Location = new System.Drawing.Point(7, 56);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(107, 13);
            this.label3.TabIndex = 56;
            this.label3.Text = "Download speed limit";
            // 
            // labelThreads
            // 
            this.labelThreads.AutoSize = true;
            this.labelThreads.Font = new System.Drawing.Font("Segoe UI Semilight", 8.25F);
            this.labelThreads.Location = new System.Drawing.Point(120, 29);
            this.labelThreads.Name = "labelThreads";
            this.labelThreads.Size = new System.Drawing.Size(17, 13);
            this.labelThreads.TabIndex = 54;
            this.labelThreads.Text = "10";
            // 
            // label21
            // 
            this.label21.AutoSize = true;
            this.label21.Font = new System.Drawing.Font("Segoe UI Semilight", 8.25F);
            this.label21.Location = new System.Drawing.Point(7, 7);
            this.label21.Name = "label21";
            this.label21.Size = new System.Drawing.Size(170, 13);
            this.label21.TabIndex = 52;
            this.label21.Text = "Maximum simultaneous downloads";
            // 
            // labelSizeContent
            // 
            this.labelSizeContent.AutoSize = true;
            this.labelSizeContent.Font = new System.Drawing.Font("Segoe UI Semilight", 8.25F);
            this.labelSizeContent.ForeColor = System.Drawing.SystemColors.GrayText;
            this.labelSizeContent.Location = new System.Drawing.Point(89, 71);
            this.labelSizeContent.Name = "labelSizeContent";
            this.labelSizeContent.Size = new System.Drawing.Size(42, 13);
            this.labelSizeContent.TabIndex = 73;
            this.labelSizeContent.Text = "content";
            this.labelSizeContent.Visible = false;
            // 
            // labelSizeContentValue
            // 
            this.labelSizeContentValue.AutoSize = true;
            this.labelSizeContentValue.Font = new System.Drawing.Font("Segoe UI Semilight", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.labelSizeContentValue.Location = new System.Drawing.Point(131, 71);
            this.labelSizeContentValue.Name = "labelSizeContentValue";
            this.labelSizeContentValue.Size = new System.Drawing.Size(40, 13);
            this.labelSizeContentValue.TabIndex = 74;
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
            this.labelSizeCore.Name = "labelSizeCore";
            this.labelSizeCore.Size = new System.Drawing.Size(28, 13);
            this.labelSizeCore.TabIndex = 75;
            this.labelSizeCore.Text = "core";
            this.labelSizeCore.Visible = false;
            // 
            // labelSizeCoreValue
            // 
            this.labelSizeCoreValue.AutoSize = true;
            this.labelSizeCoreValue.Font = new System.Drawing.Font("Segoe UI Semilight", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.labelSizeCoreValue.Location = new System.Drawing.Point(199, 71);
            this.labelSizeCoreValue.Name = "labelSizeCoreValue";
            this.labelSizeCoreValue.Size = new System.Drawing.Size(40, 13);
            this.labelSizeCoreValue.TabIndex = 76;
            this.labelSizeCoreValue.Text = "0 bytes";
            this.labelSizeCoreValue.Visible = false;
            // 
            // checkUseHttps
            // 
            this.checkUseHttps.AutoSize = true;
            this.checkUseHttps.Font = new System.Drawing.Font("Segoe UI Semilight", 8.25F);
            this.checkUseHttps.Location = new System.Drawing.Point(14, 124);
            this.checkUseHttps.Name = "checkUseHttps";
            this.checkUseHttps.Size = new System.Drawing.Size(78, 17);
            this.checkUseHttps.TabIndex = 67;
            this.checkUseHttps.Text = "Use HTTPS";
            this.checkUseHttps.UseVisualStyleBackColor = true;
            this.checkUseHttps.Click += new System.EventHandler(this.checkUseHttps_Click);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Font = new System.Drawing.Font("Segoe UI Semilight", 8.25F);
            this.label2.Location = new System.Drawing.Point(7, 151);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(105, 13);
            this.label2.TabIndex = 60;
            this.label2.Text = "Recheck all manifests";
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Font = new System.Drawing.Font("Segoe UI Semilight", 8.25F);
            this.label5.Location = new System.Drawing.Point(7, 105);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(56, 13);
            this.label5.TabIndex = 68;
            this.label5.Text = "Encryption";
            // 
            // panelAdvancedContent
            // 
            this.panelAdvancedContent.Controls.Add(this.label5);
            this.panelAdvancedContent.Controls.Add(this.label21);
            this.panelAdvancedContent.Controls.Add(this.checkUseHttps);
            this.panelAdvancedContent.Controls.Add(this.sliderThreads);
            this.panelAdvancedContent.Controls.Add(this.labelRecheckManifests);
            this.panelAdvancedContent.Controls.Add(this.labelThreads);
            this.panelAdvancedContent.Controls.Add(this.label2);
            this.panelAdvancedContent.Controls.Add(this.label3);
            this.panelAdvancedContent.Controls.Add(this.labelSpeedLimit);
            this.panelAdvancedContent.Controls.Add(this.sliderSpeedLimit);
            this.panelAdvancedContent.Controls.Add(this.checkSpeedLimit);
            this.panelAdvancedContent.Location = new System.Drawing.Point(232, 12);
            this.panelAdvancedContent.Name = "panelAdvancedContent";
            this.panelAdvancedContent.Size = new System.Drawing.Size(261, 192);
            this.panelAdvancedContent.TabIndex = 77;
            // 
            // sliderThreads
            // 
            this.sliderThreads.Location = new System.Drawing.Point(14, 26);
            this.sliderThreads.Name = "sliderThreads";
            this.sliderThreads.Size = new System.Drawing.Size(100, 20);
            this.sliderThreads.TabIndex = 55;
            this.sliderThreads.TabStop = false;
            this.sliderThreads.Value = 0.5F;
            this.sliderThreads.ValueChanged += new System.EventHandler<float>(this.sliderThreads_ValueChanged);
            // 
            // sliderSpeedLimit
            // 
            this.sliderSpeedLimit.Enabled = false;
            this.sliderSpeedLimit.Location = new System.Drawing.Point(36, 75);
            this.sliderSpeedLimit.Name = "sliderSpeedLimit";
            this.sliderSpeedLimit.Size = new System.Drawing.Size(145, 20);
            this.sliderSpeedLimit.TabIndex = 59;
            this.sliderSpeedLimit.TabStop = false;
            this.sliderSpeedLimit.Value = 1F;
            this.sliderSpeedLimit.ValueChanged += new System.EventHandler<float>(this.sliderSpeedLimit_ValueChanged);
            // 
            // panelAdvanced
            // 
            this.panelAdvanced.Location = new System.Drawing.Point(229, 8);
            this.panelAdvanced.Name = "panelAdvanced";
            this.panelAdvanced.Size = new System.Drawing.Size(261, 153);
            this.panelAdvanced.TabIndex = 78;
            this.panelAdvanced.Visible = false;
            // 
            // labelAdvanced
            // 
            this.labelAdvanced.AutoSize = true;
            this.labelAdvanced.Cursor = System.Windows.Forms.Cursors.Hand;
            this.labelAdvanced.Font = new System.Drawing.Font("Segoe UI Semilight", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.labelAdvanced.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(49)))), ((int)(((byte)(121)))), ((int)(((byte)(242)))));
            this.labelAdvanced.Location = new System.Drawing.Point(229, 12);
            this.labelAdvanced.Name = "labelAdvanced";
            this.labelAdvanced.Padding = new System.Windows.Forms.Padding(6, 0, 6, 0);
            this.labelAdvanced.Size = new System.Drawing.Size(65, 13);
            this.labelAdvanced.TabIndex = 71;
            this.labelAdvanced.Text = "advanced";
            this.labelAdvanced.Click += new System.EventHandler(this.labelAdvanced_Click);
            // 
            // labelLang
            // 
            this.labelLang.AutoSize = true;
            this.labelLang.Cursor = System.Windows.Forms.Cursors.Hand;
            this.labelLang.Font = new System.Drawing.Font("Segoe UI Semilight", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.labelLang.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(49)))), ((int)(((byte)(121)))), ((int)(((byte)(242)))));
            this.labelLang.Location = new System.Drawing.Point(192, 12);
            this.labelLang.Name = "labelLang";
            this.labelLang.Padding = new System.Windows.Forms.Padding(6, 0, 6, 0);
            this.labelLang.Size = new System.Drawing.Size(33, 13);
            this.labelLang.TabIndex = 66;
            this.labelLang.Text = "EN";
            this.labelLang.Click += new System.EventHandler(this.labelLang_Click);
            // 
            // formBackgroundPatcher
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
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
            this.Font = new System.Drawing.Font("Segoe UI", 8.25F);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.Icon = global::Gw2Launcher.Properties.Resources.Gw2;
            this.MaximizeBox = false;
            this.Name = "formBackgroundPatcher";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Background Patch Downloader";
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.formBackgroundPatcher_FormClosed);
            this.Load += new System.EventHandler(this.formBackgroundPatcher_Load);
            this.panelLang.ResumeLayout(false);
            this.panelAdvancedContent.ResumeLayout(false);
            this.panelAdvancedContent.PerformLayout();
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
        private System.Windows.Forms.Label labelStatus;
        private System.Windows.Forms.ProgressBar progressDownload;
        private System.Windows.Forms.Panel panelLang;
        private System.Windows.Forms.Label labelLang1;
        private System.Windows.Forms.Label labelLang3;
        private System.Windows.Forms.Label labelLang2;
        private System.Windows.Forms.Label labelSizeEstimated;
        private System.Windows.Forms.Label labelLang4;
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
        private System.Windows.Forms.Label labelRecheckManifests;
        private System.Windows.Forms.CheckBox checkUseHttps;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.Panel panelAdvancedContent;
        private Controls.ScrollablePanelContainer panelAdvanced;
    }
}