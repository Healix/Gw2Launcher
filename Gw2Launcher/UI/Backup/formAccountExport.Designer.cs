namespace Gw2Launcher.UI.Backup
{
    partial class formAccountExport
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
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle1 = new System.Windows.Forms.DataGridViewCellStyle();
            this.stackPanel1 = new Gw2Launcher.UI.Controls.StackPanel();
            this.label56 = new System.Windows.Forms.Label();
            this.stackPanel3 = new Gw2Launcher.UI.Controls.StackPanel();
            this.checkGw2 = new System.Windows.Forms.CheckBox();
            this.checkGw1 = new System.Windows.Forms.CheckBox();
            this.checkSelect = new System.Windows.Forms.CheckBox();
            this.label1 = new System.Windows.Forms.Label();
            this.gridData = new Gw2Launcher.UI.Controls.SelectionDataGridView();
            this.columnName = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.checkTextPasswords = new System.Windows.Forms.CheckBox();
            this.stackPanel2 = new Gw2Launcher.UI.Controls.StackPanel();
            this.buttonCancel = new System.Windows.Forms.Button();
            this.buttonSave = new System.Windows.Forms.Button();
            this.stackPanel1.SuspendLayout();
            this.stackPanel3.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.gridData)).BeginInit();
            this.stackPanel2.SuspendLayout();
            this.SuspendLayout();
            // 
            // stackPanel1
            // 
            this.stackPanel1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.stackPanel1.Controls.Add(this.label56);
            this.stackPanel1.Controls.Add(this.stackPanel3);
            this.stackPanel1.Controls.Add(this.checkSelect);
            this.stackPanel1.Controls.Add(this.label1);
            this.stackPanel1.Controls.Add(this.gridData);
            this.stackPanel1.Controls.Add(this.checkTextPasswords);
            this.stackPanel1.Controls.Add(this.stackPanel2);
            this.stackPanel1.Location = new System.Drawing.Point(13, 13);
            this.stackPanel1.Margin = new System.Windows.Forms.Padding(13);
            this.stackPanel1.Size = new System.Drawing.Size(288, 395);
            // 
            // label56
            // 
            this.label56.AutoSize = true;
            this.label56.Font = new System.Drawing.Font("Segoe UI Semibold", 9F, System.Drawing.FontStyle.Bold);
            this.label56.Location = new System.Drawing.Point(0, 0);
            this.label56.Margin = new System.Windows.Forms.Padding(0);
            this.label56.Size = new System.Drawing.Size(105, 15);
            this.label56.Text = "Included accounts";
            // 
            // stackPanel3
            // 
            this.stackPanel3.AutoSize = true;
            this.stackPanel3.Controls.Add(this.checkGw2);
            this.stackPanel3.Controls.Add(this.checkGw1);
            this.stackPanel3.FlowDirection = System.Windows.Forms.FlowDirection.LeftToRight;
            this.stackPanel3.Location = new System.Drawing.Point(0, 23);
            this.stackPanel3.Margin = new System.Windows.Forms.Padding(0, 8, 0, 0);
            this.stackPanel3.Size = new System.Drawing.Size(200, 17);
            // 
            // checkGw2
            // 
            this.checkGw2.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.checkGw2.AutoSize = true;
            this.checkGw2.Checked = true;
            this.checkGw2.CheckState = System.Windows.Forms.CheckState.Checked;
            this.checkGw2.Font = new System.Drawing.Font("Segoe UI", 8.25F);
            this.checkGw2.Location = new System.Drawing.Point(8, 0);
            this.checkGw2.Margin = new System.Windows.Forms.Padding(8, 0, 0, 0);
            this.checkGw2.Size = new System.Drawing.Size(92, 17);
            this.checkGw2.Text = "Guild Wars 2";
            this.checkGw2.UseVisualStyleBackColor = true;
            this.checkGw2.CheckedChanged += new System.EventHandler(this.checkAccountType_CheckedChanged);
            // 
            // checkGw1
            // 
            this.checkGw1.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.checkGw1.AutoSize = true;
            this.checkGw1.Checked = true;
            this.checkGw1.CheckState = System.Windows.Forms.CheckState.Checked;
            this.checkGw1.Font = new System.Drawing.Font("Segoe UI", 8.25F);
            this.checkGw1.Location = new System.Drawing.Point(108, 0);
            this.checkGw1.Margin = new System.Windows.Forms.Padding(8, 0, 0, 0);
            this.checkGw1.Size = new System.Drawing.Size(92, 17);
            this.checkGw1.Text = "Guild Wars 1";
            this.checkGw1.UseVisualStyleBackColor = true;
            this.checkGw1.CheckedChanged += new System.EventHandler(this.checkAccountType_CheckedChanged);
            // 
            // checkSelect
            // 
            this.checkSelect.AutoSize = true;
            this.checkSelect.Font = new System.Drawing.Font("Segoe UI", 8.25F);
            this.checkSelect.Location = new System.Drawing.Point(8, 43);
            this.checkSelect.Margin = new System.Windows.Forms.Padding(8, 3, 0, 0);
            this.checkSelect.Size = new System.Drawing.Size(105, 17);
            this.checkSelect.Text = "Select accounts";
            this.checkSelect.UseVisualStyleBackColor = true;
            this.checkSelect.CheckedChanged += new System.EventHandler(this.checkSelect_CheckedChanged);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("Segoe UI Semibold", 9F, System.Drawing.FontStyle.Bold);
            this.label1.Location = new System.Drawing.Point(0, 68);
            this.label1.Margin = new System.Windows.Forms.Padding(0, 8, 0, 0);
            this.label1.Size = new System.Drawing.Size(126, 15);
            this.label1.Text = "Included account data";
            // 
            // gridData
            // 
            this.gridData.AllowUserToAddRows = false;
            this.gridData.AllowUserToDeleteRows = false;
            this.gridData.AllowUserToResizeColumns = false;
            this.gridData.AllowUserToResizeRows = false;
            this.gridData.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.gridData.BackgroundColor = System.Drawing.SystemColors.Window;
            this.gridData.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.gridData.CellBorderStyle = System.Windows.Forms.DataGridViewCellBorderStyle.None;
            this.gridData.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.gridData.ColumnHeadersVisible = false;
            this.gridData.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.columnName});
            dataGridViewCellStyle1.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle1.BackColor = System.Drawing.SystemColors.Window;
            dataGridViewCellStyle1.Font = new System.Drawing.Font("Segoe UI", 8.25F);
            dataGridViewCellStyle1.ForeColor = System.Drawing.SystemColors.ControlText;
            dataGridViewCellStyle1.SelectionBackColor = System.Drawing.SystemColors.ControlLight;
            dataGridViewCellStyle1.SelectionForeColor = System.Drawing.SystemColors.ControlText;
            dataGridViewCellStyle1.WrapMode = System.Windows.Forms.DataGridViewTriState.False;
            this.gridData.DefaultCellStyle = dataGridViewCellStyle1;
            this.gridData.EditMode = System.Windows.Forms.DataGridViewEditMode.EditProgrammatically;
            this.gridData.Location = new System.Drawing.Point(8, 91);
            this.gridData.Margin = new System.Windows.Forms.Padding(8);
            this.gridData.ReadOnly = true;
            this.gridData.RowHeadersVisible = false;
            this.gridData.RowHighlightDeselectedColor = System.Drawing.Color.FromArgb(((int)(((byte)(222)))), ((int)(((byte)(176)))), ((int)(((byte)(196)))));
            this.gridData.RowHighlightSelectedColor = System.Drawing.Color.LightSteelBlue;
            this.gridData.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.gridData.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.gridData.Size = new System.Drawing.Size(272, 230);
            // 
            // columnName
            // 
            this.columnName.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
            this.columnName.HeaderText = "";
            this.columnName.ReadOnly = true;
            // 
            // checkTextPasswords
            // 
            this.checkTextPasswords.AutoSize = true;
            this.checkTextPasswords.Font = new System.Drawing.Font("Segoe UI", 8.25F);
            this.checkTextPasswords.Location = new System.Drawing.Point(9, 329);
            this.checkTextPasswords.Margin = new System.Windows.Forms.Padding(9, 0, 8, 8);
            this.checkTextPasswords.Size = new System.Drawing.Size(181, 17);
            this.checkTextPasswords.Text = "Export passwords in plain text";
            this.checkTextPasswords.UseVisualStyleBackColor = true;
            // 
            // stackPanel2
            // 
            this.stackPanel2.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.stackPanel2.AutoSize = true;
            this.stackPanel2.Controls.Add(this.buttonCancel);
            this.stackPanel2.Controls.Add(this.buttonSave);
            this.stackPanel2.FlowDirection = System.Windows.Forms.FlowDirection.LeftToRight;
            this.stackPanel2.Location = new System.Drawing.Point(99, 354);
            this.stackPanel2.Margin = new System.Windows.Forms.Padding(0);
            this.stackPanel2.Size = new System.Drawing.Size(189, 41);
            // 
            // buttonCancel
            // 
            this.buttonCancel.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.buttonCancel.Font = new System.Drawing.Font("Segoe UI", 8.25F);
            this.buttonCancel.Location = new System.Drawing.Point(3, 3);
            this.buttonCancel.Size = new System.Drawing.Size(86, 35);
            this.buttonCancel.Text = "Cancel";
            this.buttonCancel.UseVisualStyleBackColor = true;
            this.buttonCancel.Click += new System.EventHandler(this.buttonCancel_Click);
            // 
            // buttonSave
            // 
            this.buttonSave.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.buttonSave.Font = new System.Drawing.Font("Segoe UI", 8.25F);
            this.buttonSave.Location = new System.Drawing.Point(95, 3);
            this.buttonSave.Margin = new System.Windows.Forms.Padding(3, 3, 8, 3);
            this.buttonSave.Size = new System.Drawing.Size(86, 35);
            this.buttonSave.Text = "Export";
            this.buttonSave.UseVisualStyleBackColor = true;
            this.buttonSave.Click += new System.EventHandler(this.buttonSave_Click);
            // 
            // formAccountExport
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
            this.ClientSize = new System.Drawing.Size(314, 421);
            this.Controls.Add(this.stackPanel1);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.MinimumSize = new System.Drawing.Size(300, 300);
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.stackPanel1.ResumeLayout(false);
            this.stackPanel1.PerformLayout();
            this.stackPanel3.ResumeLayout(false);
            this.stackPanel3.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.gridData)).EndInit();
            this.stackPanel2.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private Controls.StackPanel stackPanel1;
        private System.Windows.Forms.Label label56;
        private System.Windows.Forms.CheckBox checkGw1;
        private System.Windows.Forms.CheckBox checkGw2;
        private System.Windows.Forms.Label label1;
        private Controls.SelectionDataGridView gridData;
        private Controls.StackPanel stackPanel2;
        private System.Windows.Forms.Button buttonCancel;
        private System.Windows.Forms.Button buttonSave;
        private Controls.StackPanel stackPanel3;
        private System.Windows.Forms.CheckBox checkSelect;
        private System.Windows.Forms.DataGridViewTextBoxColumn columnName;
        private System.Windows.Forms.CheckBox checkTextPasswords;

    }
}
