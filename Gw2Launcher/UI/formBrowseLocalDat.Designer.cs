namespace Gw2Launcher.UI
{
    partial class formBrowseLocalDat
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
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle2 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle3 = new System.Windows.Forms.DataGridViewCellStyle();
            this.stackPanel1 = new Gw2Launcher.UI.Controls.StackPanel();
            this.label5 = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.stackPanel2 = new Gw2Launcher.UI.Controls.StackPanel();
            this.radioAccountCopy = new System.Windows.Forms.RadioButton();
            this.radioAccountShare = new System.Windows.Forms.RadioButton();
            this.gridAccounts = new Controls.ScaledDataGridView();
            this.columnName = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.columnUser = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.columnShared = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.label2 = new System.Windows.Forms.Label();
            this.stackPanel3 = new Gw2Launcher.UI.Controls.StackPanel();
            this.radioFileCopy = new System.Windows.Forms.RadioButton();
            this.radioFileMove = new System.Windows.Forms.RadioButton();
            this.stackPanel4 = new Gw2Launcher.UI.Controls.StackPanel();
            this.textBrowse = new System.Windows.Forms.TextBox();
            this.buttonBrowse = new System.Windows.Forms.Button();
            this.label3 = new System.Windows.Forms.Label();
            this.radioCreateNew = new System.Windows.Forms.RadioButton();
            this.stackPanel5 = new Gw2Launcher.UI.Controls.StackPanel();
            this.buttonCancel = new System.Windows.Forms.Button();
            this.buttonOK = new System.Windows.Forms.Button();
            this.stackPanel1.SuspendLayout();
            this.stackPanel2.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.gridAccounts)).BeginInit();
            this.stackPanel3.SuspendLayout();
            this.stackPanel4.SuspendLayout();
            this.stackPanel5.SuspendLayout();
            this.SuspendLayout();
            // 
            // stackPanel1
            // 
            this.stackPanel1.AutoSize = true;
            this.stackPanel1.Controls.Add(this.label5);
            this.stackPanel1.Controls.Add(this.label1);
            this.stackPanel1.Controls.Add(this.stackPanel2);
            this.stackPanel1.Controls.Add(this.gridAccounts);
            this.stackPanel1.Controls.Add(this.label2);
            this.stackPanel1.Controls.Add(this.stackPanel3);
            this.stackPanel1.Controls.Add(this.stackPanel4);
            this.stackPanel1.Controls.Add(this.label3);
            this.stackPanel1.Controls.Add(this.radioCreateNew);
            this.stackPanel1.Controls.Add(this.stackPanel5);
            this.stackPanel1.Location = new System.Drawing.Point(13, 13);
            this.stackPanel1.Margin = new System.Windows.Forms.Padding(13);
            this.stackPanel1.MinimumSize = new System.Drawing.Size(350, 0);
            this.stackPanel1.Size = new System.Drawing.Size(350, 375);
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Font = new System.Drawing.Font("Segoe UI Semilight", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label5.Location = new System.Drawing.Point(0, 0);
            this.label5.Margin = new System.Windows.Forms.Padding(0, 0, 0, 23);
            this.label5.Size = new System.Drawing.Size(108, 13);
            this.label5.Text = "Select a {0} file to use.";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("Segoe UI Semibold", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label1.Location = new System.Drawing.Point(0, 36);
            this.label1.Margin = new System.Windows.Forms.Padding(0, 0, 0, 8);
            this.label1.Size = new System.Drawing.Size(99, 15);
            this.label1.Text = "Existing accounts";
            // 
            // stackPanel2
            // 
            this.stackPanel2.AutoSize = true;
            this.stackPanel2.Controls.Add(this.radioAccountCopy);
            this.stackPanel2.Controls.Add(this.radioAccountShare);
            this.stackPanel2.FlowDirection = System.Windows.Forms.FlowDirection.LeftToRight;
            this.stackPanel2.Location = new System.Drawing.Point(0, 59);
            this.stackPanel2.Margin = new System.Windows.Forms.Padding(0);
            this.stackPanel2.Size = new System.Drawing.Size(173, 17);
            // 
            // radioAccountCopy
            // 
            this.radioAccountCopy.AutoSize = true;
            this.radioAccountCopy.Checked = true;
            this.radioAccountCopy.Location = new System.Drawing.Point(8, 0);
            this.radioAccountCopy.Margin = new System.Windows.Forms.Padding(8, 0, 0, 0);
            this.radioAccountCopy.Size = new System.Drawing.Size(74, 17);
            this.radioAccountCopy.TabStop = true;
            this.radioAccountCopy.Text = "Copy from";
            this.radioAccountCopy.UseVisualStyleBackColor = true;
            this.radioAccountCopy.CheckedChanged += new System.EventHandler(this.radio_CheckedChanged);
            // 
            // radioAccountShare
            // 
            this.radioAccountShare.AutoSize = true;
            this.radioAccountShare.Location = new System.Drawing.Point(98, 0);
            this.radioAccountShare.Margin = new System.Windows.Forms.Padding(16, 0, 0, 0);
            this.radioAccountShare.Size = new System.Drawing.Size(75, 17);
            this.radioAccountShare.Text = "Share with";
            this.radioAccountShare.UseVisualStyleBackColor = true;
            this.radioAccountShare.CheckedChanged += new System.EventHandler(this.radio_CheckedChanged);
            // 
            // gridAccounts
            // 
            this.gridAccounts.AllowUserToAddRows = false;
            this.gridAccounts.AllowUserToDeleteRows = false;
            this.gridAccounts.AllowUserToResizeColumns = false;
            this.gridAccounts.AllowUserToResizeRows = false;
            this.gridAccounts.BackgroundColor = System.Drawing.SystemColors.Window;
            this.gridAccounts.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.gridAccounts.CellBorderStyle = System.Windows.Forms.DataGridViewCellBorderStyle.None;
            this.gridAccounts.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.gridAccounts.ColumnHeadersVisible = false;
            this.gridAccounts.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.columnName,
            this.columnUser,
            this.columnShared});
            this.gridAccounts.EditMode = System.Windows.Forms.DataGridViewEditMode.EditProgrammatically;
            this.gridAccounts.Location = new System.Drawing.Point(8, 84);
            this.gridAccounts.Margin = new System.Windows.Forms.Padding(8, 8, 0, 0);
            this.gridAccounts.MultiSelect = false;
            this.gridAccounts.ReadOnly = true;
            this.gridAccounts.RowHeadersVisible = false;
            this.gridAccounts.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.gridAccounts.Size = new System.Drawing.Size(317, 95);
            this.gridAccounts.CellMouseEnter += new System.Windows.Forms.DataGridViewCellEventHandler(this.gridAccounts_CellMouseEnter);
            // 
            // columnName
            // 
            this.columnName.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
            dataGridViewCellStyle1.Font = new System.Drawing.Font("Segoe UI", 8.25F);
            this.columnName.DefaultCellStyle = dataGridViewCellStyle1;
            this.columnName.HeaderText = "Name";
            this.columnName.ReadOnly = true;
            // 
            // columnUser
            // 
            dataGridViewCellStyle2.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleRight;
            dataGridViewCellStyle2.Font = new System.Drawing.Font("Segoe UI Semilight", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            dataGridViewCellStyle2.ForeColor = System.Drawing.SystemColors.GrayText;
            this.columnUser.DefaultCellStyle = dataGridViewCellStyle2;
            this.columnUser.HeaderText = "User";
            this.columnUser.ReadOnly = true;
            this.columnUser.Width = 50;
            // 
            // columnShared
            // 
            dataGridViewCellStyle3.Alignment = System.Windows.Forms.DataGridViewContentAlignment.TopLeft;
            dataGridViewCellStyle3.Font = new System.Drawing.Font("Segoe UI", 6F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            dataGridViewCellStyle3.ForeColor = System.Drawing.Color.Blue;
            this.columnShared.DefaultCellStyle = dataGridViewCellStyle3;
            this.columnShared.HeaderText = "Shared";
            this.columnShared.ReadOnly = true;
            this.columnShared.Width = 20;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Font = new System.Drawing.Font("Segoe UI Semibold", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label2.Location = new System.Drawing.Point(0, 192);
            this.label2.Margin = new System.Windows.Forms.Padding(0, 13, 0, 8);
            this.label2.Size = new System.Drawing.Size(105, 15);
            this.label2.Text = "Use an existing file";
            // 
            // stackPanel3
            // 
            this.stackPanel3.AutoSize = true;
            this.stackPanel3.Controls.Add(this.radioFileCopy);
            this.stackPanel3.Controls.Add(this.radioFileMove);
            this.stackPanel3.FlowDirection = System.Windows.Forms.FlowDirection.LeftToRight;
            this.stackPanel3.Location = new System.Drawing.Point(0, 215);
            this.stackPanel3.Margin = new System.Windows.Forms.Padding(0);
            this.stackPanel3.Size = new System.Drawing.Size(175, 17);
            // 
            // radioFileCopy
            // 
            this.radioFileCopy.AutoSize = true;
            this.radioFileCopy.Location = new System.Drawing.Point(8, 0);
            this.radioFileCopy.Margin = new System.Windows.Forms.Padding(8, 0, 0, 0);
            this.radioFileCopy.Size = new System.Drawing.Size(74, 17);
            this.radioFileCopy.Text = "Copy from";
            this.radioFileCopy.UseVisualStyleBackColor = true;
            this.radioFileCopy.CheckedChanged += new System.EventHandler(this.radio_CheckedChanged);
            // 
            // radioFileMove
            // 
            this.radioFileMove.AutoSize = true;
            this.radioFileMove.Location = new System.Drawing.Point(98, 0);
            this.radioFileMove.Margin = new System.Windows.Forms.Padding(16, 0, 0, 0);
            this.radioFileMove.Size = new System.Drawing.Size(77, 17);
            this.radioFileMove.Text = "Move from";
            this.radioFileMove.UseVisualStyleBackColor = true;
            this.radioFileMove.CheckedChanged += new System.EventHandler(this.radio_CheckedChanged);
            // 
            // stackPanel4
            // 
            this.stackPanel4.AutoSize = true;
            this.stackPanel4.Controls.Add(this.textBrowse);
            this.stackPanel4.Controls.Add(this.buttonBrowse);
            this.stackPanel4.FlowDirection = System.Windows.Forms.FlowDirection.LeftToRight;
            this.stackPanel4.Location = new System.Drawing.Point(0, 240);
            this.stackPanel4.Margin = new System.Windows.Forms.Padding(0, 8, 0, 0);
            this.stackPanel4.Size = new System.Drawing.Size(325, 24);
            // 
            // textBrowse
            // 
            this.textBrowse.Font = new System.Drawing.Font("Segoe UI", 8.25F);
            this.textBrowse.Location = new System.Drawing.Point(8, 1);
            this.textBrowse.Margin = new System.Windows.Forms.Padding(8, 1, 0, 1);
            this.textBrowse.ReadOnly = true;
            this.textBrowse.Size = new System.Drawing.Size(268, 22);
            // 
            // buttonBrowse
            // 
            this.buttonBrowse.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left)));
            this.buttonBrowse.Font = new System.Drawing.Font("Segoe UI", 8.25F);
            this.buttonBrowse.Location = new System.Drawing.Point(282, 0);
            this.buttonBrowse.Margin = new System.Windows.Forms.Padding(6, 0, 0, 0);
            this.buttonBrowse.Size = new System.Drawing.Size(43, 24);
            this.buttonBrowse.Text = "...";
            this.buttonBrowse.UseVisualStyleBackColor = true;
            this.buttonBrowse.Click += new System.EventHandler(this.buttonBrowse_Click);
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Font = new System.Drawing.Font("Segoe UI Semibold", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label3.Location = new System.Drawing.Point(0, 277);
            this.label3.Margin = new System.Windows.Forms.Padding(0, 13, 0, 8);
            this.label3.Size = new System.Drawing.Size(93, 15);
            this.label3.Text = "Create a new file";
            // 
            // radioCreateNew
            // 
            this.radioCreateNew.AutoSize = true;
            this.radioCreateNew.Location = new System.Drawing.Point(8, 300);
            this.radioCreateNew.Margin = new System.Windows.Forms.Padding(8, 0, 0, 0);
            this.radioCreateNew.Size = new System.Drawing.Size(120, 17);
            this.radioCreateNew.Text = "Create a new {0} file";
            this.radioCreateNew.UseVisualStyleBackColor = true;
            this.radioCreateNew.CheckedChanged += new System.EventHandler(this.radio_CheckedChanged);
            // 
            // stackPanel5
            // 
            this.stackPanel5.Anchor = System.Windows.Forms.AnchorStyles.Top;
            this.stackPanel5.AutoSize = true;
            this.stackPanel5.Controls.Add(this.buttonCancel);
            this.stackPanel5.Controls.Add(this.buttonOK);
            this.stackPanel5.FlowDirection = System.Windows.Forms.FlowDirection.LeftToRight;
            this.stackPanel5.Location = new System.Drawing.Point(87, 340);
            this.stackPanel5.Margin = new System.Windows.Forms.Padding(0, 23, 0, 0);
            this.stackPanel5.Size = new System.Drawing.Size(177, 35);
            // 
            // buttonCancel
            // 
            this.buttonCancel.Anchor = System.Windows.Forms.AnchorStyles.Bottom;
            this.buttonCancel.Font = new System.Drawing.Font("Segoe UI", 8.25F);
            this.buttonCancel.Location = new System.Drawing.Point(0, 0);
            this.buttonCancel.Margin = new System.Windows.Forms.Padding(0, 0, 5, 0);
            this.buttonCancel.Size = new System.Drawing.Size(86, 35);
            this.buttonCancel.Text = "Cancel";
            this.buttonCancel.UseVisualStyleBackColor = true;
            this.buttonCancel.Click += new System.EventHandler(this.buttonCancel_Click);
            // 
            // buttonOK
            // 
            this.buttonOK.Anchor = System.Windows.Forms.AnchorStyles.Bottom;
            this.buttonOK.Font = new System.Drawing.Font("Segoe UI", 8.25F);
            this.buttonOK.Location = new System.Drawing.Point(91, 0);
            this.buttonOK.Margin = new System.Windows.Forms.Padding(0);
            this.buttonOK.Size = new System.Drawing.Size(86, 35);
            this.buttonOK.Text = "OK";
            this.buttonOK.UseVisualStyleBackColor = true;
            this.buttonOK.Click += new System.EventHandler(this.buttonOK_Click);
            // 
            // formBrowseLocalDat
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
            this.AutoSize = true;
            this.AutoSizeFill = Gw2Launcher.UI.Base.StackFormBase.AutoSizeFillMode.Height;
            this.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.ClientSize = new System.Drawing.Size(376, 401);
            this.Controls.Add(this.stackPanel1);
            this.Font = new System.Drawing.Font("Segoe UI Semilight", 8.25F);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.Icon = global::Gw2Launcher.Properties.Resources.Gw2Launcher;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Load += new System.EventHandler(this.formBrowseLocalDat_Load);
            this.stackPanel1.ResumeLayout(false);
            this.stackPanel1.PerformLayout();
            this.stackPanel2.ResumeLayout(false);
            this.stackPanel2.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.gridAccounts)).EndInit();
            this.stackPanel3.ResumeLayout(false);
            this.stackPanel3.PerformLayout();
            this.stackPanel4.ResumeLayout(false);
            this.stackPanel4.PerformLayout();
            this.stackPanel5.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.RadioButton radioAccountCopy;
        private System.Windows.Forms.RadioButton radioAccountShare;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Button buttonBrowse;
        private System.Windows.Forms.TextBox textBrowse;
        private System.Windows.Forms.RadioButton radioFileCopy;
        private System.Windows.Forms.Button buttonCancel;
        private System.Windows.Forms.Button buttonOK;
        private Controls.ScaledDataGridView gridAccounts;
        private System.Windows.Forms.RadioButton radioFileMove;
        private System.Windows.Forms.DataGridViewTextBoxColumn columnName;
        private System.Windows.Forms.DataGridViewTextBoxColumn columnUser;
        private System.Windows.Forms.DataGridViewTextBoxColumn columnShared;
        private System.Windows.Forms.RadioButton radioCreateNew;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label5;
        private Controls.StackPanel stackPanel1;
        private Controls.StackPanel stackPanel2;
        private Controls.StackPanel stackPanel3;
        private Controls.StackPanel stackPanel4;
        private Controls.StackPanel stackPanel5;
    }
}
