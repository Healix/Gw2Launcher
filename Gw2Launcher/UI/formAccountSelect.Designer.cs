namespace Gw2Launcher.UI
{
    partial class formAccountSelect
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
            this.labelTitle = new System.Windows.Forms.Label();
            this.gridAccounts = new Gw2Launcher.UI.Controls.ScaledDataGridView();
            this.columnCheck = new System.Windows.Forms.DataGridViewCheckBoxColumn();
            this.columnName = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.stackPanel2 = new Gw2Launcher.UI.Controls.StackPanel();
            this.buttonCancel = new System.Windows.Forms.Button();
            this.buttonOK = new System.Windows.Forms.Button();
            this.stackPanel1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.gridAccounts)).BeginInit();
            this.stackPanel2.SuspendLayout();
            this.SuspendLayout();
            // 
            // stackPanel1
            // 
            this.stackPanel1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.stackPanel1.Controls.Add(this.labelTitle);
            this.stackPanel1.Controls.Add(this.gridAccounts);
            this.stackPanel1.Controls.Add(this.stackPanel2);
            this.stackPanel1.Location = new System.Drawing.Point(13, 13);
            this.stackPanel1.Margin = new System.Windows.Forms.Padding(13);
            this.stackPanel1.Name = "stackPanel1";
            this.stackPanel1.Size = new System.Drawing.Size(259, 264);
            this.stackPanel1.TabIndex = 35;
            // 
            // labelTitle
            // 
            this.labelTitle.AutoSize = true;
            this.labelTitle.Font = new System.Drawing.Font("Segoe UI Semilight", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.labelTitle.Location = new System.Drawing.Point(0, 0);
            this.labelTitle.Margin = new System.Windows.Forms.Padding(0, 0, 0, 8);
            this.labelTitle.Name = "labelTitle";
            this.labelTitle.Size = new System.Drawing.Size(85, 13);
            this.labelTitle.TabIndex = 34;
            this.labelTitle.Text = "Which accounts?";
            this.labelTitle.SizeChanged += new System.EventHandler(this.labelTitle_SizeChanged);
            // 
            // gridAccounts
            // 
            this.gridAccounts.AllowUserToAddRows = false;
            this.gridAccounts.AllowUserToDeleteRows = false;
            this.gridAccounts.AllowUserToResizeColumns = false;
            this.gridAccounts.AllowUserToResizeRows = false;
            this.gridAccounts.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.gridAccounts.BackgroundColor = System.Drawing.SystemColors.Control;
            this.gridAccounts.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.gridAccounts.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.gridAccounts.ColumnHeadersVisible = false;
            this.gridAccounts.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.columnCheck,
            this.columnName});
            dataGridViewCellStyle1.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle1.BackColor = System.Drawing.SystemColors.Control;
            dataGridViewCellStyle1.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            dataGridViewCellStyle1.ForeColor = System.Drawing.SystemColors.ControlText;
            dataGridViewCellStyle1.SelectionBackColor = System.Drawing.SystemColors.Highlight;
            dataGridViewCellStyle1.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            dataGridViewCellStyle1.WrapMode = System.Windows.Forms.DataGridViewTriState.False;
            this.gridAccounts.DefaultCellStyle = dataGridViewCellStyle1;
            this.gridAccounts.EditMode = System.Windows.Forms.DataGridViewEditMode.EditProgrammatically;
            this.gridAccounts.Location = new System.Drawing.Point(0, 21);
            this.gridAccounts.Margin = new System.Windows.Forms.Padding(0);
            this.gridAccounts.MultiSelect = false;
            this.gridAccounts.Name = "gridAccounts";
            this.gridAccounts.ReadOnly = true;
            this.gridAccounts.RowHeadersVisible = false;
            this.gridAccounts.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.gridAccounts.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.CellSelect;
            this.gridAccounts.Size = new System.Drawing.Size(259, 193);
            this.gridAccounts.TabIndex = 0;
            this.gridAccounts.CellContentClick += new System.Windows.Forms.DataGridViewCellEventHandler(this.gridAccounts_CellContentClick);
            this.gridAccounts.SelectionChanged += new System.EventHandler(this.gridServers_SelectionChanged);
            // 
            // columnCheck
            // 
            this.columnCheck.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.AllCells;
            this.columnCheck.HeaderText = "Check";
            this.columnCheck.Name = "columnCheck";
            this.columnCheck.ReadOnly = true;
            this.columnCheck.Width = 5;
            // 
            // columnName
            // 
            this.columnName.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
            this.columnName.HeaderText = "Name";
            this.columnName.Name = "columnName";
            this.columnName.ReadOnly = true;
            // 
            // stackPanel2
            // 
            this.stackPanel2.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.stackPanel2.AutoSize = true;
            this.stackPanel2.Controls.Add(this.buttonCancel);
            this.stackPanel2.Controls.Add(this.buttonOK);
            this.stackPanel2.FlowDirection = System.Windows.Forms.FlowDirection.LeftToRight;
            this.stackPanel2.Location = new System.Drawing.Point(77, 229);
            this.stackPanel2.Margin = new System.Windows.Forms.Padding(0, 15, 0, 0);
            this.stackPanel2.Name = "stackPanel2";
            this.stackPanel2.Size = new System.Drawing.Size(182, 35);
            this.stackPanel2.TabIndex = 36;
            // 
            // buttonCancel
            // 
            this.buttonCancel.Font = new System.Drawing.Font("Segoe UI", 8.25F);
            this.buttonCancel.Location = new System.Drawing.Point(0, 0);
            this.buttonCancel.Margin = new System.Windows.Forms.Padding(0);
            this.buttonCancel.Name = "buttonCancel";
            this.buttonCancel.Size = new System.Drawing.Size(86, 35);
            this.buttonCancel.TabIndex = 32;
            this.buttonCancel.Text = "Cancel";
            this.buttonCancel.UseVisualStyleBackColor = true;
            this.buttonCancel.Click += new System.EventHandler(this.buttonCancel_Click);
            // 
            // buttonOK
            // 
            this.buttonOK.Font = new System.Drawing.Font("Segoe UI", 8.25F);
            this.buttonOK.Location = new System.Drawing.Point(96, 0);
            this.buttonOK.Margin = new System.Windows.Forms.Padding(10, 0, 0, 0);
            this.buttonOK.Name = "buttonOK";
            this.buttonOK.Size = new System.Drawing.Size(86, 35);
            this.buttonOK.TabIndex = 33;
            this.buttonOK.Text = "OK";
            this.buttonOK.UseVisualStyleBackColor = true;
            this.buttonOK.Click += new System.EventHandler(this.buttonOK_Click);
            // 
            // formAccountSelect
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
            this.ClientSize = new System.Drawing.Size(285, 290);
            this.Controls.Add(this.stackPanel1);
            this.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.Icon = global::Gw2Launcher.Properties.Resources.Gw2Launcher;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.MinimumSize = new System.Drawing.Size(301, 200);
            this.Name = "formAccountSelect";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Load += new System.EventHandler(this.formAccountSelect_Load);
            this.stackPanel1.ResumeLayout(false);
            this.stackPanel1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.gridAccounts)).EndInit();
            this.stackPanel2.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private Controls.ScaledDataGridView gridAccounts;
        private System.Windows.Forms.Button buttonCancel;
        private System.Windows.Forms.Button buttonOK;
        private System.Windows.Forms.Label labelTitle;
        private System.Windows.Forms.DataGridViewCheckBoxColumn columnCheck;
        private System.Windows.Forms.DataGridViewTextBoxColumn columnName;
        private Controls.StackPanel stackPanel1;
        private Controls.StackPanel stackPanel2;
    }
}
