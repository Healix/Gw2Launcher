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
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle6 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle4 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle5 = new System.Windows.Forms.DataGridViewCellStyle();
            this.labelLastRequest = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.labelDownloaded = new System.Windows.Forms.Label();
            this.checkRecord = new System.Windows.Forms.CheckBox();
            this.gridRecord = new System.Windows.Forms.DataGridView();
            this.columnIndex = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.columnUrl = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.columnResponseCode = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.columnResponse = new System.Windows.Forms.DataGridViewLinkColumn();
            this.checkEnabled = new System.Windows.Forms.CheckBox();
            this.checkRecordData = new System.Windows.Forms.CheckBox();
            this.labelStatus = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.labelClear = new System.Windows.Forms.Label();
            this.labelCached = new System.Windows.Forms.Label();
            this.label5 = new System.Windows.Forms.Label();
            ((System.ComponentModel.ISupportInitialize)(this.gridRecord)).BeginInit();
            this.SuspendLayout();
            // 
            // labelLastRequest
            // 
            this.labelLastRequest.AutoSize = true;
            this.labelLastRequest.Font = new System.Drawing.Font("Segoe UI Semilight", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.labelLastRequest.Location = new System.Drawing.Point(89, 71);
            this.labelLastRequest.Name = "labelLastRequest";
            this.labelLastRequest.Size = new System.Drawing.Size(19, 13);
            this.labelLastRequest.TabIndex = 30;
            this.labelLastRequest.Text = "---";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Font = new System.Drawing.Font("Segoe UI Semilight", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label3.Location = new System.Drawing.Point(10, 71);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(63, 13);
            this.label3.TabIndex = 29;
            this.label3.Text = "Last request";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("Segoe UI Semilight", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label1.Location = new System.Drawing.Point(10, 89);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(42, 13);
            this.label1.TabIndex = 31;
            this.label1.Text = "UL / DL";
            // 
            // labelDownloaded
            // 
            this.labelDownloaded.AutoSize = true;
            this.labelDownloaded.Font = new System.Drawing.Font("Segoe UI Semilight", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.labelDownloaded.Location = new System.Drawing.Point(89, 89);
            this.labelDownloaded.Name = "labelDownloaded";
            this.labelDownloaded.Size = new System.Drawing.Size(19, 13);
            this.labelDownloaded.TabIndex = 32;
            this.labelDownloaded.Text = "---";
            // 
            // checkRecord
            // 
            this.checkRecord.AutoSize = true;
            this.checkRecord.Location = new System.Drawing.Point(13, 115);
            this.checkRecord.Name = "checkRecord";
            this.checkRecord.Size = new System.Drawing.Size(109, 17);
            this.checkRecord.TabIndex = 33;
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
            dataGridViewCellStyle6.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle6.BackColor = System.Drawing.SystemColors.Control;
            dataGridViewCellStyle6.Font = new System.Drawing.Font("Segoe UI", 8.25F);
            dataGridViewCellStyle6.ForeColor = System.Drawing.SystemColors.ControlText;
            dataGridViewCellStyle6.SelectionBackColor = System.Drawing.SystemColors.Highlight;
            dataGridViewCellStyle6.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            dataGridViewCellStyle6.WrapMode = System.Windows.Forms.DataGridViewTriState.False;
            this.gridRecord.DefaultCellStyle = dataGridViewCellStyle6;
            this.gridRecord.EditMode = System.Windows.Forms.DataGridViewEditMode.EditProgrammatically;
            this.gridRecord.Location = new System.Drawing.Point(12, 138);
            this.gridRecord.MultiSelect = false;
            this.gridRecord.Name = "gridRecord";
            this.gridRecord.ReadOnly = true;
            this.gridRecord.RowHeadersVisible = false;
            this.gridRecord.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.gridRecord.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.gridRecord.Size = new System.Drawing.Size(481, 169);
            this.gridRecord.TabIndex = 49;
            this.gridRecord.CellContentClick += new System.Windows.Forms.DataGridViewCellEventHandler(this.gridRecord_CellContentClick);
            this.gridRecord.SelectionChanged += new System.EventHandler(this.gridRecord_SelectionChanged);
            // 
            // columnIndex
            // 
            this.columnIndex.HeaderText = "";
            this.columnIndex.Name = "columnIndex";
            this.columnIndex.ReadOnly = true;
            this.columnIndex.Resizable = System.Windows.Forms.DataGridViewTriState.True;
            this.columnIndex.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable;
            this.columnIndex.Width = 40;
            // 
            // columnUrl
            // 
            this.columnUrl.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
            this.columnUrl.HeaderText = "URL";
            this.columnUrl.Name = "columnUrl";
            this.columnUrl.ReadOnly = true;
            this.columnUrl.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable;
            // 
            // columnResponseCode
            // 
            dataGridViewCellStyle4.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleCenter;
            this.columnResponseCode.DefaultCellStyle = dataGridViewCellStyle4;
            this.columnResponseCode.HeaderText = "Code";
            this.columnResponseCode.Name = "columnResponseCode";
            this.columnResponseCode.ReadOnly = true;
            this.columnResponseCode.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable;
            this.columnResponseCode.ToolTipText = "Response code";
            this.columnResponseCode.Width = 40;
            // 
            // columnResponse
            // 
            this.columnResponse.ActiveLinkColor = System.Drawing.Color.FromArgb(((int)(((byte)(49)))), ((int)(((byte)(121)))), ((int)(((byte)(242)))));
            dataGridViewCellStyle5.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleRight;
            this.columnResponse.DefaultCellStyle = dataGridViewCellStyle5;
            this.columnResponse.HeaderText = "Content";
            this.columnResponse.LinkBehavior = System.Windows.Forms.LinkBehavior.HoverUnderline;
            this.columnResponse.LinkColor = System.Drawing.Color.FromArgb(((int)(((byte)(49)))), ((int)(((byte)(121)))), ((int)(((byte)(242)))));
            this.columnResponse.Name = "columnResponse";
            this.columnResponse.ReadOnly = true;
            this.columnResponse.Resizable = System.Windows.Forms.DataGridViewTriState.True;
            this.columnResponse.TrackVisitedState = false;
            // 
            // checkEnabled
            // 
            this.checkEnabled.AutoSize = true;
            this.checkEnabled.Location = new System.Drawing.Point(10, 10);
            this.checkEnabled.Name = "checkEnabled";
            this.checkEnabled.Size = new System.Drawing.Size(181, 17);
            this.checkEnabled.TabIndex = 50;
            this.checkEnabled.Text = "Enable local asset server proxy";
            this.checkEnabled.UseVisualStyleBackColor = true;
            // 
            // checkRecordData
            // 
            this.checkRecordData.AutoSize = true;
            this.checkRecordData.Enabled = false;
            this.checkRecordData.Location = new System.Drawing.Point(133, 115);
            this.checkRecordData.Name = "checkRecordData";
            this.checkRecordData.Size = new System.Drawing.Size(105, 17);
            this.checkRecordData.TabIndex = 51;
            this.checkRecordData.Text = "Record content";
            this.checkRecordData.UseVisualStyleBackColor = true;
            // 
            // labelStatus
            // 
            this.labelStatus.AutoSize = true;
            this.labelStatus.Font = new System.Drawing.Font("Segoe UI Semilight", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.labelStatus.Location = new System.Drawing.Point(89, 35);
            this.labelStatus.Name = "labelStatus";
            this.labelStatus.Size = new System.Drawing.Size(19, 13);
            this.labelStatus.TabIndex = 54;
            this.labelStatus.Text = "---";
            this.labelStatus.Click += new System.EventHandler(this.labelStatus_Click);
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Font = new System.Drawing.Font("Segoe UI Semilight", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label4.Location = new System.Drawing.Point(10, 35);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(35, 13);
            this.label4.TabIndex = 53;
            this.label4.Text = "Status";
            // 
            // labelClear
            // 
            this.labelClear.AutoSize = true;
            this.labelClear.Cursor = System.Windows.Forms.Cursors.Hand;
            this.labelClear.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(49)))), ((int)(((byte)(121)))), ((int)(((byte)(242)))));
            this.labelClear.Location = new System.Drawing.Point(462, 116);
            this.labelClear.Name = "labelClear";
            this.labelClear.Size = new System.Drawing.Size(31, 13);
            this.labelClear.TabIndex = 55;
            this.labelClear.Text = "clear";
            this.labelClear.Visible = false;
            this.labelClear.Click += new System.EventHandler(this.labelClear_Click);
            // 
            // labelCached
            // 
            this.labelCached.AutoSize = true;
            this.labelCached.Cursor = System.Windows.Forms.Cursors.Hand;
            this.labelCached.Font = new System.Drawing.Font("Segoe UI Semilight", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.labelCached.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(49)))), ((int)(((byte)(121)))), ((int)(((byte)(242)))));
            this.labelCached.Location = new System.Drawing.Point(89, 53);
            this.labelCached.Name = "labelCached";
            this.labelCached.Size = new System.Drawing.Size(19, 13);
            this.labelCached.TabIndex = 59;
            this.labelCached.Text = "---";
            this.labelCached.Click += new System.EventHandler(this.labelCached_Click);
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Font = new System.Drawing.Font("Segoe UI Semilight", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label5.Location = new System.Drawing.Point(10, 53);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(37, 13);
            this.label5.TabIndex = 58;
            this.label5.Text = "Cache";
            // 
            // formAssetProxy
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(505, 319);
            this.Controls.Add(this.labelClear);
            this.Controls.Add(this.labelStatus);
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
            this.Font = new System.Drawing.Font("Segoe UI", 8.25F);
            this.Icon = global::Gw2Launcher.Properties.Resources.Gw2;
            this.MinimumSize = new System.Drawing.Size(429, 296);
            this.Name = "formAssetProxy";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Asset Proxy";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.formAssetProxy_FormClosing);
            this.Load += new System.EventHandler(this.formAssetProxy_Load);
            ((System.ComponentModel.ISupportInitialize)(this.gridRecord)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label labelLastRequest;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label labelDownloaded;
        private System.Windows.Forms.CheckBox checkRecord;
        private System.Windows.Forms.DataGridView gridRecord;
        private System.Windows.Forms.CheckBox checkEnabled;
        private System.Windows.Forms.CheckBox checkRecordData;
        private System.Windows.Forms.Label labelStatus;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Label labelClear;
        private System.Windows.Forms.DataGridViewTextBoxColumn columnIndex;
        private System.Windows.Forms.DataGridViewTextBoxColumn columnUrl;
        private System.Windows.Forms.DataGridViewTextBoxColumn columnResponseCode;
        private System.Windows.Forms.DataGridViewLinkColumn columnResponse;
        private System.Windows.Forms.Label labelCached;
        private System.Windows.Forms.Label label5;
    }
}