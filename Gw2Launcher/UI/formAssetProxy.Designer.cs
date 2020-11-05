namespace Gw2Launcher.UI
{
    partial class formAssetProxy
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
            this.labelLastRequest = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.labelDownloaded = new System.Windows.Forms.Label();
            this.checkRecord = new System.Windows.Forms.CheckBox();
            this.gridRecord = new Controls.ScaledDataGridView();
            this.columnIndex = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.columnUrl = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.columnResponseCode = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.columnResponse = new System.Windows.Forms.DataGridViewLinkColumn();
            this.checkEnabled = new System.Windows.Forms.CheckBox();
            this.checkRecordData = new System.Windows.Forms.CheckBox();
            this.labelStatus = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.labelClear = new Controls.LinkLabel();
            this.labelCached = new Controls.LinkLabel();
            this.label5 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.checkUseHttps = new System.Windows.Forms.CheckBox();
            this.label7 = new System.Windows.Forms.Label();
            this.labelSpeedLimit = new System.Windows.Forms.Label();
            this.sliderSpeedLimit = new Gw2Launcher.UI.Controls.FlatSlider();
            this.checkSpeedLimit = new System.Windows.Forms.CheckBox();
            this.labelAdvanced = new Gw2Launcher.UI.Controls.LabelButton();
            this.panelAdvanced = new Gw2Launcher.UI.Controls.AutoScrollContainerPanel();
            this.stackPanel1 = new Gw2Launcher.UI.Controls.StackPanel();
            this.stackPanel2 = new Gw2Launcher.UI.Controls.StackPanel();
            this.label6 = new System.Windows.Forms.Label();
            this.stackPanel3 = new Gw2Launcher.UI.Controls.StackPanel();
            this.checkPort = new System.Windows.Forms.CheckBox();
            this.numericPort = new System.Windows.Forms.NumericUpDown();
            this.stackPanel4 = new Gw2Launcher.UI.Controls.StackPanel();
            this.checkOverrideHosts = new System.Windows.Forms.CheckBox();
            this.label8 = new System.Windows.Forms.Label();
            this.stackPanel5 = new Gw2Launcher.UI.Controls.StackPanel();
            this.labelError = new System.Windows.Forms.Label();
            ((System.ComponentModel.ISupportInitialize)(this.gridRecord)).BeginInit();
            this.panelAdvanced.SuspendLayout();
            this.stackPanel1.SuspendLayout();
            this.stackPanel2.SuspendLayout();
            this.stackPanel3.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numericPort)).BeginInit();
            this.stackPanel4.SuspendLayout();
            this.stackPanel5.SuspendLayout();
            this.SuspendLayout();
            // 
            // labelLastRequest
            // 
            this.labelLastRequest.AutoSize = true;
            this.labelLastRequest.Font = new System.Drawing.Font("Segoe UI Semilight", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.labelLastRequest.Location = new System.Drawing.Point(89, 71);
            this.labelLastRequest.Size = new System.Drawing.Size(19, 13);
            this.labelLastRequest.Text = "---";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Font = new System.Drawing.Font("Segoe UI Semilight", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label3.Location = new System.Drawing.Point(10, 71);
            this.label3.Size = new System.Drawing.Size(63, 13);
            this.label3.Text = "Last request";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("Segoe UI Semilight", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label1.Location = new System.Drawing.Point(10, 89);
            this.label1.Size = new System.Drawing.Size(42, 13);
            this.label1.Text = "UL / DL";
            // 
            // labelDownloaded
            // 
            this.labelDownloaded.AutoSize = true;
            this.labelDownloaded.Font = new System.Drawing.Font("Segoe UI Semilight", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.labelDownloaded.Location = new System.Drawing.Point(89, 89);
            this.labelDownloaded.Size = new System.Drawing.Size(19, 13);
            this.labelDownloaded.Text = "---";
            // 
            // checkRecord
            // 
            this.checkRecord.AutoSize = true;
            this.checkRecord.Location = new System.Drawing.Point(13, 115);
            this.checkRecord.Size = new System.Drawing.Size(109, 17);
            this.checkRecord.Text = "Record requests";
            this.checkRecord.UseVisualStyleBackColor = true;
            this.checkRecord.CheckedChanged += new System.EventHandler(this.checkRecord_CheckedChanged);
            // 
            // gridRecord
            // 
            this.gridRecord.AllowUserToAddRows = false;
            this.gridRecord.AllowUserToDeleteRows = false;
            this.gridRecord.AllowUserToResizeColumns = false;
            this.gridRecord.AllowUserToResizeRows = false;
            this.gridRecord.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.gridRecord.BackgroundColor = System.Drawing.SystemColors.Control;
            this.gridRecord.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.gridRecord.CellBorderStyle = System.Windows.Forms.DataGridViewCellBorderStyle.None;
            this.gridRecord.ColumnHeadersBorderStyle = System.Windows.Forms.DataGridViewHeaderBorderStyle.None;
            this.gridRecord.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.gridRecord.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.columnIndex,
            this.columnUrl,
            this.columnResponseCode,
            this.columnResponse});
            dataGridViewCellStyle3.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle3.BackColor = System.Drawing.SystemColors.Control;
            dataGridViewCellStyle3.Font = new System.Drawing.Font("Segoe UI", 8.25F);
            dataGridViewCellStyle3.ForeColor = System.Drawing.SystemColors.ControlText;
            dataGridViewCellStyle3.SelectionBackColor = System.Drawing.SystemColors.Highlight;
            dataGridViewCellStyle3.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            dataGridViewCellStyle3.WrapMode = System.Windows.Forms.DataGridViewTriState.False;
            this.gridRecord.DefaultCellStyle = dataGridViewCellStyle3;
            this.gridRecord.EditMode = System.Windows.Forms.DataGridViewEditMode.EditProgrammatically;
            this.gridRecord.Location = new System.Drawing.Point(12, 138);
            this.gridRecord.MultiSelect = false;
            this.gridRecord.ReadOnly = true;
            this.gridRecord.RowHeadersVisible = false;
            this.gridRecord.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.gridRecord.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.gridRecord.Size = new System.Drawing.Size(481, 169);
            this.gridRecord.CellContentClick += new System.Windows.Forms.DataGridViewCellEventHandler(this.gridRecord_CellContentClick);
            this.gridRecord.SelectionChanged += new System.EventHandler(this.gridRecord_SelectionChanged);
            // 
            // columnIndex
            // 
            this.columnIndex.HeaderText = "";
            this.columnIndex.ReadOnly = true;
            this.columnIndex.Resizable = System.Windows.Forms.DataGridViewTriState.True;
            this.columnIndex.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable;
            this.columnIndex.Width = 40;
            // 
            // columnUrl
            // 
            this.columnUrl.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
            this.columnUrl.HeaderText = "URL";
            this.columnUrl.ReadOnly = true;
            this.columnUrl.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable;
            // 
            // columnResponseCode
            // 
            dataGridViewCellStyle1.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleCenter;
            this.columnResponseCode.DefaultCellStyle = dataGridViewCellStyle1;
            this.columnResponseCode.HeaderText = "Code";
            this.columnResponseCode.ReadOnly = true;
            this.columnResponseCode.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable;
            this.columnResponseCode.ToolTipText = "Response code";
            this.columnResponseCode.Width = 40;
            // 
            // columnResponse
            // 
            this.columnResponse.ActiveLinkColor = System.Drawing.Color.FromArgb(((int)(((byte)(49)))), ((int)(((byte)(121)))), ((int)(((byte)(242)))));
            dataGridViewCellStyle2.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleRight;
            this.columnResponse.DefaultCellStyle = dataGridViewCellStyle2;
            this.columnResponse.HeaderText = "Content";
            this.columnResponse.LinkBehavior = System.Windows.Forms.LinkBehavior.HoverUnderline;
            this.columnResponse.LinkColor = System.Drawing.Color.FromArgb(((int)(((byte)(49)))), ((int)(((byte)(121)))), ((int)(((byte)(242)))));
            this.columnResponse.ReadOnly = true;
            this.columnResponse.Resizable = System.Windows.Forms.DataGridViewTriState.True;
            this.columnResponse.TrackVisitedState = false;
            // 
            // checkEnabled
            // 
            this.checkEnabled.AutoSize = true;
            this.checkEnabled.Location = new System.Drawing.Point(10, 10);
            this.checkEnabled.Size = new System.Drawing.Size(181, 17);
            this.checkEnabled.Text = "Enable local asset server proxy";
            this.checkEnabled.UseVisualStyleBackColor = true;
            // 
            // checkRecordData
            // 
            this.checkRecordData.AutoSize = true;
            this.checkRecordData.Enabled = false;
            this.checkRecordData.Location = new System.Drawing.Point(133, 115);
            this.checkRecordData.Size = new System.Drawing.Size(105, 17);
            this.checkRecordData.Text = "Record content";
            this.checkRecordData.UseVisualStyleBackColor = true;
            // 
            // labelStatus
            // 
            this.labelStatus.AutoSize = true;
            this.labelStatus.Font = new System.Drawing.Font("Segoe UI Semilight", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.labelStatus.Location = new System.Drawing.Point(0, 0);
            this.labelStatus.Margin = new System.Windows.Forms.Padding(0);
            this.labelStatus.Size = new System.Drawing.Size(19, 13);
            this.labelStatus.Text = "---";
            this.labelStatus.Click += new System.EventHandler(this.labelStatus_Click);
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Font = new System.Drawing.Font("Segoe UI Semilight", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label4.Location = new System.Drawing.Point(10, 35);
            this.label4.Size = new System.Drawing.Size(35, 13);
            this.label4.Text = "Status";
            // 
            // labelClear
            // 
            this.labelClear.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.labelClear.AutoSize = true;
            this.labelClear.Location = new System.Drawing.Point(462, 116);
            this.labelClear.Size = new System.Drawing.Size(31, 13);
            this.labelClear.Text = "clear";
            this.labelClear.Visible = false;
            this.labelClear.Click += new System.EventHandler(this.labelClear_Click);
            // 
            // labelCached
            // 
            this.labelCached.AutoSize = true;
            this.labelCached.Font = new System.Drawing.Font("Segoe UI Semilight", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.labelCached.Location = new System.Drawing.Point(89, 53);
            this.labelCached.Size = new System.Drawing.Size(19, 13);
            this.labelCached.Text = "---";
            this.labelCached.Click += new System.EventHandler(this.labelCached_Click);
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Font = new System.Drawing.Font("Segoe UI Semilight", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label5.Location = new System.Drawing.Point(10, 53);
            this.label5.Size = new System.Drawing.Size(37, 13);
            this.label5.Text = "Cache";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Font = new System.Drawing.Font("Segoe UI Semilight", 8.25F);
            this.label2.Location = new System.Drawing.Point(10, 59);
            this.label2.Margin = new System.Windows.Forms.Padding(0, 10, 0, 6);
            this.label2.Size = new System.Drawing.Size(56, 13);
            this.label2.Text = "Encryption";
            // 
            // checkUseHttps
            // 
            this.checkUseHttps.AutoSize = true;
            this.checkUseHttps.Font = new System.Drawing.Font("Segoe UI Semilight", 8.25F);
            this.checkUseHttps.Location = new System.Drawing.Point(18, 78);
            this.checkUseHttps.Margin = new System.Windows.Forms.Padding(8, 0, 0, 0);
            this.checkUseHttps.Size = new System.Drawing.Size(78, 17);
            this.checkUseHttps.Text = "Use HTTPS";
            this.checkUseHttps.UseVisualStyleBackColor = true;
            this.checkUseHttps.Click += new System.EventHandler(this.checkUseHttps_Click);
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Font = new System.Drawing.Font("Segoe UI Semilight", 8.25F);
            this.label7.Location = new System.Drawing.Point(10, 10);
            this.label7.Margin = new System.Windows.Forms.Padding(0, 0, 0, 6);
            this.label7.Size = new System.Drawing.Size(107, 13);
            this.label7.Text = "Download speed limit";
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
            // sliderSpeedLimit
            // 
            this.sliderSpeedLimit.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.sliderSpeedLimit.Enabled = false;
            this.sliderSpeedLimit.Location = new System.Drawing.Point(30, 0);
            this.sliderSpeedLimit.Margin = new System.Windows.Forms.Padding(7, 0, 0, 0);
            this.sliderSpeedLimit.Size = new System.Drawing.Size(145, 20);
            this.sliderSpeedLimit.TabStop = false;
            this.sliderSpeedLimit.Value = 1F;
            this.sliderSpeedLimit.ValueChanged += new System.EventHandler(this.sliderSpeedLimit_ValueChanged);
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
            // labelAdvanced
            // 
            this.labelAdvanced.AutoSize = true;
            this.labelAdvanced.Cursor = System.Windows.Forms.Cursors.Hand;
            this.labelAdvanced.Font = new System.Drawing.Font("Segoe UI Semilight", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.labelAdvanced.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(49)))), ((int)(((byte)(121)))), ((int)(((byte)(242)))));
            this.labelAdvanced.Location = new System.Drawing.Point(197, 12);
            this.labelAdvanced.Padding = new System.Windows.Forms.Padding(6, 0, 6, 0);
            this.labelAdvanced.Size = new System.Drawing.Size(65, 13);
            this.labelAdvanced.Text = "advanced";
            this.labelAdvanced.Click += new System.EventHandler(this.labelAdvanced_Click);
            // 
            // panelAdvanced
            // 
            this.panelAdvanced.AutoScroll = false;
            this.panelAdvanced.AutoSize = true;
            this.panelAdvanced.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.panelAdvanced.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.panelAdvanced.Controls.Add(this.stackPanel1);
            this.panelAdvanced.Location = new System.Drawing.Point(197, 8);
            this.panelAdvanced.MinimumSize = new System.Drawing.Size(261, 2);
            this.panelAdvanced.Size = new System.Drawing.Size(263, 181);
            this.panelAdvanced.Visible = false;
            // 
            // stackPanel1
            // 
            this.stackPanel1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.stackPanel1.AutoSize = true;
            this.stackPanel1.AutoSizeFill = Gw2Launcher.UI.Controls.StackPanel.AutoSizeFillMode.Width;
            this.stackPanel1.Controls.Add(this.label7);
            this.stackPanel1.Controls.Add(this.stackPanel2);
            this.stackPanel1.Controls.Add(this.label2);
            this.stackPanel1.Controls.Add(this.checkUseHttps);
            this.stackPanel1.Controls.Add(this.label6);
            this.stackPanel1.Controls.Add(this.stackPanel3);
            this.stackPanel1.Controls.Add(this.stackPanel4);
            this.stackPanel1.Location = new System.Drawing.Point(0, 0);
            this.stackPanel1.Margin = new System.Windows.Forms.Padding(0);
            this.stackPanel1.Padding = new System.Windows.Forms.Padding(10);
            this.stackPanel1.Size = new System.Drawing.Size(261, 179);
            // 
            // stackPanel2
            // 
            this.stackPanel2.AutoSize = true;
            this.stackPanel2.AutoSizeFill = Gw2Launcher.UI.Controls.StackPanel.AutoSizeFillMode.NoWrap;
            this.stackPanel2.Controls.Add(this.checkSpeedLimit);
            this.stackPanel2.Controls.Add(this.sliderSpeedLimit);
            this.stackPanel2.Controls.Add(this.labelSpeedLimit);
            this.stackPanel2.FlowDirection = System.Windows.Forms.FlowDirection.LeftToRight;
            this.stackPanel2.Location = new System.Drawing.Point(10, 29);
            this.stackPanel2.Margin = new System.Windows.Forms.Padding(0);
            this.stackPanel2.Size = new System.Drawing.Size(225, 20);
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Font = new System.Drawing.Font("Segoe UI Semilight", 8.25F);
            this.label6.Location = new System.Drawing.Point(10, 105);
            this.label6.Margin = new System.Windows.Forms.Padding(0, 10, 0, 6);
            this.label6.Size = new System.Drawing.Size(26, 13);
            this.label6.Text = "Port";
            // 
            // stackPanel3
            // 
            this.stackPanel3.AutoSize = true;
            this.stackPanel3.AutoSizeFill = Gw2Launcher.UI.Controls.StackPanel.AutoSizeFillMode.NoWrap;
            this.stackPanel3.Controls.Add(this.checkPort);
            this.stackPanel3.Controls.Add(this.numericPort);
            this.stackPanel3.FlowDirection = System.Windows.Forms.FlowDirection.LeftToRight;
            this.stackPanel3.Location = new System.Drawing.Point(10, 124);
            this.stackPanel3.Margin = new System.Windows.Forms.Padding(0, 0, 0, 3);
            this.stackPanel3.Size = new System.Drawing.Size(81, 22);
            // 
            // checkPort
            // 
            this.checkPort.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.checkPort.AutoSize = true;
            this.checkPort.Font = new System.Drawing.Font("Segoe UI", 8.25F);
            this.checkPort.Location = new System.Drawing.Point(8, 4);
            this.checkPort.Margin = new System.Windows.Forms.Padding(8, 0, 0, 0);
            this.checkPort.Size = new System.Drawing.Size(15, 14);
            this.checkPort.UseVisualStyleBackColor = true;
            this.checkPort.CheckedChanged += new System.EventHandler(this.checkPort_CheckedChanged);
            // 
            // numericPort
            // 
            this.numericPort.Enabled = false;
            this.numericPort.Location = new System.Drawing.Point(30, 0);
            this.numericPort.Margin = new System.Windows.Forms.Padding(7, 0, 0, 0);
            this.numericPort.Maximum = new decimal(new int[] {
            65535,
            0,
            0,
            0});
            this.numericPort.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.numericPort.Size = new System.Drawing.Size(51, 22);
            this.numericPort.Value = new decimal(new int[] {
            80,
            0,
            0,
            0});
            // 
            // stackPanel4
            // 
            this.stackPanel4.AutoSize = true;
            this.stackPanel4.AutoSizeFill = Gw2Launcher.UI.Controls.StackPanel.AutoSizeFillMode.NoWrap;
            this.stackPanel4.Controls.Add(this.checkOverrideHosts);
            this.stackPanel4.Controls.Add(this.label8);
            this.stackPanel4.FlowDirection = System.Windows.Forms.FlowDirection.LeftToRight;
            this.stackPanel4.Location = new System.Drawing.Point(10, 149);
            this.stackPanel4.Margin = new System.Windows.Forms.Padding(0, 0, 0, 3);
            this.stackPanel4.Size = new System.Drawing.Size(188, 17);
            // 
            // checkOverrideHosts
            // 
            this.checkOverrideHosts.AutoSize = true;
            this.checkOverrideHosts.Font = new System.Drawing.Font("Segoe UI Semilight", 8.25F);
            this.checkOverrideHosts.Location = new System.Drawing.Point(8, 0);
            this.checkOverrideHosts.Margin = new System.Windows.Forms.Padding(8, 0, 0, 0);
            this.checkOverrideHosts.Size = new System.Drawing.Size(92, 17);
            this.checkOverrideHosts.Text = "Override DNS";
            this.checkOverrideHosts.UseVisualStyleBackColor = true;
            this.checkOverrideHosts.CheckedChanged += new System.EventHandler(this.checkOverrideHosts_CheckedChanged);
            this.checkOverrideHosts.Click += new System.EventHandler(this.checkOverrideHosts_Click);
            // 
            // label8
            // 
            this.label8.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.label8.AutoSize = true;
            this.label8.Font = new System.Drawing.Font("Segoe UI Semilight", 8.25F);
            this.label8.ForeColor = System.Drawing.SystemColors.GrayText;
            this.label8.Location = new System.Drawing.Point(106, 1);
            this.label8.Margin = new System.Windows.Forms.Padding(6, 0, 0, 1);
            this.label8.Size = new System.Drawing.Size(82, 13);
            this.label8.Text = "requires port 80";
            // 
            // stackPanel5
            // 
            this.stackPanel5.AutoSize = true;
            this.stackPanel5.AutoSizeFill = Gw2Launcher.UI.Controls.StackPanel.AutoSizeFillMode.NoWrap;
            this.stackPanel5.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowOnly;
            this.stackPanel5.Controls.Add(this.labelStatus);
            this.stackPanel5.Controls.Add(this.labelError);
            this.stackPanel5.FlowDirection = System.Windows.Forms.FlowDirection.LeftToRight;
            this.stackPanel5.Location = new System.Drawing.Point(89, 35);
            this.stackPanel5.Size = new System.Drawing.Size(92, 13);
            // 
            // labelError
            // 
            this.labelError.AutoSize = true;
            this.labelError.Font = new System.Drawing.Font("Segoe UI Semilight", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.labelError.ForeColor = System.Drawing.SystemColors.GrayText;
            this.labelError.Location = new System.Drawing.Point(25, 0);
            this.labelError.Margin = new System.Windows.Forms.Padding(6, 0, 0, 0);
            this.labelError.Size = new System.Drawing.Size(67, 13);
            this.labelError.Text = "failed to start";
            this.labelError.Visible = false;
            // 
            // formAssetProxy
            // 
            this.ClientSize = new System.Drawing.Size(505, 319);
            this.Controls.Add(this.panelAdvanced);
            this.Controls.Add(this.labelAdvanced);
            this.Controls.Add(this.labelClear);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.checkRecordData);
            this.Controls.Add(this.checkEnabled);
            this.Controls.Add(this.gridRecord);
            this.Controls.Add(this.checkRecord);
            this.Controls.Add(this.labelDownloaded);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.labelLastRequest);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.labelCached);
            this.Controls.Add(this.label5);
            this.Controls.Add(this.stackPanel5);
            this.Icon = global::Gw2Launcher.Properties.Resources.Gw2Launcher;
            this.MinimumSize = new System.Drawing.Size(429, 296);
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Asset Proxy";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.formAssetProxy_FormClosing);
            this.Load += new System.EventHandler(this.formAssetProxy_Load);
            ((System.ComponentModel.ISupportInitialize)(this.gridRecord)).EndInit();
            this.panelAdvanced.ResumeLayout(false);
            this.panelAdvanced.PerformLayout();
            this.stackPanel1.ResumeLayout(false);
            this.stackPanel1.PerformLayout();
            this.stackPanel2.ResumeLayout(false);
            this.stackPanel2.PerformLayout();
            this.stackPanel3.ResumeLayout(false);
            this.stackPanel3.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numericPort)).EndInit();
            this.stackPanel4.ResumeLayout(false);
            this.stackPanel4.PerformLayout();
            this.stackPanel5.ResumeLayout(false);
            this.stackPanel5.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label labelLastRequest;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label labelDownloaded;
        private System.Windows.Forms.CheckBox checkRecord;
        private Controls.ScaledDataGridView gridRecord;
        private System.Windows.Forms.CheckBox checkEnabled;
        private System.Windows.Forms.CheckBox checkRecordData;
        private System.Windows.Forms.Label labelStatus;
        private System.Windows.Forms.Label label4;
        private Controls.LinkLabel labelClear;
        private System.Windows.Forms.DataGridViewTextBoxColumn columnIndex;
        private System.Windows.Forms.DataGridViewTextBoxColumn columnUrl;
        private System.Windows.Forms.DataGridViewTextBoxColumn columnResponseCode;
        private System.Windows.Forms.DataGridViewLinkColumn columnResponse;
        private Controls.LinkLabel labelCached;
        private System.Windows.Forms.Label label5;
        private Controls.LabelButton labelAdvanced;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.CheckBox checkUseHttps;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.Label labelSpeedLimit;
        private Controls.FlatSlider sliderSpeedLimit;
        private System.Windows.Forms.CheckBox checkSpeedLimit;
        private Controls.AutoScrollContainerPanel panelAdvanced;
        private Controls.StackPanel stackPanel1;
        private Controls.StackPanel stackPanel2;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.CheckBox checkOverrideHosts;
        private Controls.StackPanel stackPanel3;
        private System.Windows.Forms.CheckBox checkPort;
        private System.Windows.Forms.NumericUpDown numericPort;
        private Controls.StackPanel stackPanel4;
        private System.Windows.Forms.Label label8;
        private Controls.StackPanel stackPanel5;
        private System.Windows.Forms.Label labelError;
    }
}
