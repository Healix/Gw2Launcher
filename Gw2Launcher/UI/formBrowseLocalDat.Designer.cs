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
            this.radioAccountCopy = new System.Windows.Forms.RadioButton();
            this.radioAccountShare = new System.Windows.Forms.RadioButton();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.buttonBrowse = new System.Windows.Forms.Button();
            this.textBrowse = new System.Windows.Forms.TextBox();
            this.radioFileCopy = new System.Windows.Forms.RadioButton();
            this.buttonCancel = new System.Windows.Forms.Button();
            this.buttonOK = new System.Windows.Forms.Button();
            this.gridAccounts = new System.Windows.Forms.DataGridView();
            this.columnName = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.columnUser = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.columnShared = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.radioFileMove = new System.Windows.Forms.RadioButton();
            this.radioCreateNew = new System.Windows.Forms.RadioButton();
            this.label3 = new System.Windows.Forms.Label();
            this.label5 = new System.Windows.Forms.Label();
            ((System.ComponentModel.ISupportInitialize)(this.gridAccounts)).BeginInit();
            this.SuspendLayout();
            // 
            // radioAccountCopy
            // 
            this.radioAccountCopy.AutoSize = true;
            this.radioAccountCopy.Checked = true;
            this.radioAccountCopy.Location = new System.Drawing.Point(18, 69);
            this.radioAccountCopy.Name = "radioAccountCopy";
            this.radioAccountCopy.Size = new System.Drawing.Size(74, 17);
            this.radioAccountCopy.TabIndex = 15;
            this.radioAccountCopy.TabStop = true;
            this.radioAccountCopy.Text = "Copy from";
            this.radioAccountCopy.UseVisualStyleBackColor = true;
            this.radioAccountCopy.CheckedChanged += new System.EventHandler(this.radio_CheckedChanged);
            // 
            // radioAccountShare
            // 
            this.radioAccountShare.AutoSize = true;
            this.radioAccountShare.Location = new System.Drawing.Point(108, 69);
            this.radioAccountShare.Name = "radioAccountShare";
            this.radioAccountShare.Size = new System.Drawing.Size(75, 17);
            this.radioAccountShare.TabIndex = 16;
            this.radioAccountShare.Text = "Share with";
            this.radioAccountShare.UseVisualStyleBackColor = true;
            this.radioAccountShare.CheckedChanged += new System.EventHandler(this.radio_CheckedChanged);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("Segoe UI Semibold", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label1.Location = new System.Drawing.Point(10, 46);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(99, 15);
            this.label1.TabIndex = 17;
            this.label1.Text = "Existing accounts";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Font = new System.Drawing.Font("Segoe UI Semibold", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label2.Location = new System.Drawing.Point(10, 205);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(105, 15);
            this.label2.TabIndex = 18;
            this.label2.Text = "Use an existing file";
            // 
            // buttonBrowse
            // 
            this.buttonBrowse.Font = new System.Drawing.Font("Segoe UI", 8.25F);
            this.buttonBrowse.Location = new System.Drawing.Point(292, 253);
            this.buttonBrowse.Name = "buttonBrowse";
            this.buttonBrowse.Size = new System.Drawing.Size(43, 23);
            this.buttonBrowse.TabIndex = 21;
            this.buttonBrowse.Text = "...";
            this.buttonBrowse.UseVisualStyleBackColor = true;
            this.buttonBrowse.Click += new System.EventHandler(this.buttonBrowse_Click);
            // 
            // textBrowse
            // 
            this.textBrowse.Font = new System.Drawing.Font("Segoe UI", 8.25F);
            this.textBrowse.Location = new System.Drawing.Point(18, 254);
            this.textBrowse.Name = "textBrowse";
            this.textBrowse.ReadOnly = true;
            this.textBrowse.Size = new System.Drawing.Size(268, 22);
            this.textBrowse.TabIndex = 20;
            // 
            // radioFileCopy
            // 
            this.radioFileCopy.AutoSize = true;
            this.radioFileCopy.Location = new System.Drawing.Point(18, 226);
            this.radioFileCopy.Name = "radioFileCopy";
            this.radioFileCopy.Size = new System.Drawing.Size(74, 17);
            this.radioFileCopy.TabIndex = 22;
            this.radioFileCopy.Text = "Copy from";
            this.radioFileCopy.UseVisualStyleBackColor = true;
            this.radioFileCopy.CheckedChanged += new System.EventHandler(this.radio_CheckedChanged);
            // 
            // buttonCancel
            // 
            this.buttonCancel.Anchor = System.Windows.Forms.AnchorStyles.Bottom;
            this.buttonCancel.Font = new System.Drawing.Font("Segoe UI", 8.25F);
            this.buttonCancel.Location = new System.Drawing.Point(113, 349);
            this.buttonCancel.Name = "buttonCancel";
            this.buttonCancel.Size = new System.Drawing.Size(86, 35);
            this.buttonCancel.TabIndex = 24;
            this.buttonCancel.Text = "Cancel";
            this.buttonCancel.UseVisualStyleBackColor = true;
            this.buttonCancel.Click += new System.EventHandler(this.buttonCancel_Click);
            // 
            // buttonOK
            // 
            this.buttonOK.Anchor = System.Windows.Forms.AnchorStyles.Bottom;
            this.buttonOK.Font = new System.Drawing.Font("Segoe UI", 8.25F);
            this.buttonOK.Location = new System.Drawing.Point(209, 349);
            this.buttonOK.Name = "buttonOK";
            this.buttonOK.Size = new System.Drawing.Size(86, 35);
            this.buttonOK.TabIndex = 23;
            this.buttonOK.Text = "OK";
            this.buttonOK.UseVisualStyleBackColor = true;
            this.buttonOK.Click += new System.EventHandler(this.buttonOK_Click);
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
            this.gridAccounts.Location = new System.Drawing.Point(18, 97);
            this.gridAccounts.MultiSelect = false;
            this.gridAccounts.Name = "gridAccounts";
            this.gridAccounts.ReadOnly = true;
            this.gridAccounts.RowHeadersVisible = false;
            this.gridAccounts.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.gridAccounts.Size = new System.Drawing.Size(317, 95);
            this.gridAccounts.TabIndex = 25;
            this.gridAccounts.CellMouseEnter += new System.Windows.Forms.DataGridViewCellEventHandler(this.gridAccounts_CellMouseEnter);
            // 
            // columnName
            // 
            this.columnName.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
            dataGridViewCellStyle1.Font = new System.Drawing.Font("Segoe UI", 8.25F);
            this.columnName.DefaultCellStyle = dataGridViewCellStyle1;
            this.columnName.HeaderText = "Name";
            this.columnName.Name = "columnName";
            this.columnName.ReadOnly = true;
            // 
            // columnUser
            // 
            dataGridViewCellStyle2.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleRight;
            dataGridViewCellStyle2.Font = new System.Drawing.Font("Segoe UI Semilight", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            dataGridViewCellStyle2.ForeColor = System.Drawing.SystemColors.GrayText;
            this.columnUser.DefaultCellStyle = dataGridViewCellStyle2;
            this.columnUser.HeaderText = "User";
            this.columnUser.Name = "columnUser";
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
            this.columnShared.Name = "columnShared";
            this.columnShared.ReadOnly = true;
            this.columnShared.Width = 20;
            // 
            // radioFileMove
            // 
            this.radioFileMove.AutoSize = true;
            this.radioFileMove.Location = new System.Drawing.Point(108, 226);
            this.radioFileMove.Name = "radioFileMove";
            this.radioFileMove.Size = new System.Drawing.Size(77, 17);
            this.radioFileMove.TabIndex = 26;
            this.radioFileMove.Text = "Move from";
            this.radioFileMove.UseVisualStyleBackColor = true;
            this.radioFileMove.CheckedChanged += new System.EventHandler(this.radio_CheckedChanged);
            // 
            // radioCreateNew
            // 
            this.radioCreateNew.AutoSize = true;
            this.radioCreateNew.Location = new System.Drawing.Point(18, 310);
            this.radioCreateNew.Name = "radioCreateNew";
            this.radioCreateNew.Size = new System.Drawing.Size(120, 17);
            this.radioCreateNew.TabIndex = 29;
            this.radioCreateNew.Text = "Create a new {0} file";
            this.radioCreateNew.UseVisualStyleBackColor = true;
            this.radioCreateNew.CheckedChanged += new System.EventHandler(this.radio_CheckedChanged);
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Font = new System.Drawing.Font("Segoe UI Semibold", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label3.Location = new System.Drawing.Point(10, 289);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(93, 15);
            this.label3.TabIndex = 28;
            this.label3.Text = "Create a new file";
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Font = new System.Drawing.Font("Segoe UI Semilight", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label5.Location = new System.Drawing.Point(10, 10);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(108, 13);
            this.label5.TabIndex = 14;
            this.label5.Text = "Select a {0} file to use.";
            // 
            // formBrowseLocalDat
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(392, 396);
            this.Controls.Add(this.radioCreateNew);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.radioFileMove);
            this.Controls.Add(this.buttonCancel);
            this.Controls.Add(this.buttonOK);
            this.Controls.Add(this.radioFileCopy);
            this.Controls.Add(this.buttonBrowse);
            this.Controls.Add(this.textBrowse);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.radioAccountShare);
            this.Controls.Add(this.radioAccountCopy);
            this.Controls.Add(this.label5);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.gridAccounts);
            this.Font = new System.Drawing.Font("Segoe UI Semilight", 8.25F);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.Icon = global::Gw2Launcher.Properties.Resources.Gw2;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "formBrowseLocalDat";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Load += new System.EventHandler(this.formBrowseLocalDat_Load);
            ((System.ComponentModel.ISupportInitialize)(this.gridAccounts)).EndInit();
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
        private System.Windows.Forms.DataGridView gridAccounts;
        private System.Windows.Forms.RadioButton radioFileMove;
        private System.Windows.Forms.DataGridViewTextBoxColumn columnName;
        private System.Windows.Forms.DataGridViewTextBoxColumn columnUser;
        private System.Windows.Forms.DataGridViewTextBoxColumn columnShared;
        private System.Windows.Forms.RadioButton radioCreateNew;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label5;
    }
}