namespace Gw2Launcher.UI.Backup
{
    partial class formBackupRestore
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
            this.components = new System.ComponentModel.Container();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle1 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle2 = new System.Windows.Forms.DataGridViewCellStyle();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(formBackupRestore));
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle3 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle4 = new System.Windows.Forms.DataGridViewCellStyle();
            this.buttonCancel = new System.Windows.Forms.Button();
            this.buttonRestore = new System.Windows.Forms.Button();
            this.panelFiles = new Gw2Launcher.UI.Controls.StackPanel();
            this.stackPanel2 = new Gw2Launcher.UI.Controls.StackPanel();
            this.labelName = new System.Windows.Forms.Label();
            this.stackPanel52 = new Gw2Launcher.UI.Controls.StackPanel();
            this.labelShowAccounts = new System.Windows.Forms.Label();
            this.arrowButton3 = new Gw2Launcher.UI.Controls.ArrowButton();
            this.labelCreated = new System.Windows.Forms.Label();
            this.gridFiles = new Gw2Launcher.UI.Controls.ScaledDataGridView();
            this.columnPath = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.panelAccounts = new Gw2Launcher.UI.Controls.StackPanel();
            this.stackPanel5 = new Gw2Launcher.UI.Controls.StackPanel();
            this.arrowButton1 = new Gw2Launcher.UI.Controls.ArrowButton();
            this.labelAccountsBack = new System.Windows.Forms.Label();
            this.label5 = new System.Windows.Forms.Label();
            this.labelAccountsFailed = new System.Windows.Forms.Label();
            this.panelAccountsWaiting = new Gw2Launcher.UI.Controls.StackPanel();
            this.waitingBounce1 = new Gw2Launcher.UI.Controls.WaitingBounce();
            this.gridAccounts = new Gw2Launcher.UI.Controls.ScaledDataGridView();
            this.columnIcon = new System.Windows.Forms.DataGridViewImageColumn();
            this.columnName = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.columnLastUsed = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.contextFiles = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.extractToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.extractAllToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.panelFiles.SuspendLayout();
            this.stackPanel2.SuspendLayout();
            this.stackPanel52.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.gridFiles)).BeginInit();
            this.panelAccounts.SuspendLayout();
            this.stackPanel5.SuspendLayout();
            this.panelAccountsWaiting.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.gridAccounts)).BeginInit();
            this.contextFiles.SuspendLayout();
            this.SuspendLayout();
            // 
            // buttonCancel
            // 
            this.buttonCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonCancel.Font = new System.Drawing.Font("Segoe UI", 8.25F);
            this.buttonCancel.Location = new System.Drawing.Point(215, 211);
            this.buttonCancel.Size = new System.Drawing.Size(86, 35);
            this.buttonCancel.Text = "Cancel";
            this.buttonCancel.UseVisualStyleBackColor = true;
            this.buttonCancel.Click += new System.EventHandler(this.buttonCancel_Click);
            // 
            // buttonRestore
            // 
            this.buttonRestore.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonRestore.Font = new System.Drawing.Font("Segoe UI", 8.25F);
            this.buttonRestore.Location = new System.Drawing.Point(311, 211);
            this.buttonRestore.Size = new System.Drawing.Size(86, 35);
            this.buttonRestore.Text = "Restore";
            this.buttonRestore.UseVisualStyleBackColor = true;
            this.buttonRestore.Click += new System.EventHandler(this.buttonRestore_Click);
            // 
            // panelFiles
            // 
            this.panelFiles.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.panelFiles.Controls.Add(this.stackPanel2);
            this.panelFiles.Controls.Add(this.labelCreated);
            this.panelFiles.Controls.Add(this.gridFiles);
            this.panelFiles.Location = new System.Drawing.Point(12, 12);
            this.panelFiles.Size = new System.Drawing.Size(385, 188);
            // 
            // stackPanel2
            // 
            this.stackPanel2.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.stackPanel2.AutoSize = true;
            this.stackPanel2.AutoSizeFill = Gw2Launcher.UI.Controls.StackPanel.AutoSizeFillMode.Width;
            this.stackPanel2.Controls.Add(this.labelName);
            this.stackPanel2.Controls.Add(this.stackPanel52);
            this.stackPanel2.FlowDirection = System.Windows.Forms.FlowDirection.LeftToRight;
            this.stackPanel2.Location = new System.Drawing.Point(0, 0);
            this.stackPanel2.Margin = new System.Windows.Forms.Padding(0);
            this.stackPanel2.Size = new System.Drawing.Size(385, 13);
            // 
            // labelName
            // 
            this.labelName.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.labelName.AutoSize = true;
            this.labelName.Font = new System.Drawing.Font("Segoe UI Semibold", 8.25F);
            this.labelName.Location = new System.Drawing.Point(0, 0);
            this.labelName.Margin = new System.Windows.Forms.Padding(0);
            this.labelName.Size = new System.Drawing.Size(329, 13);
            this.labelName.Text = "(name)";
            // 
            // stackPanel52
            // 
            this.stackPanel52.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.stackPanel52.AutoSize = true;
            this.stackPanel52.AutoSizeFill = Gw2Launcher.UI.Controls.StackPanel.AutoSizeFillMode.NoWrap;
            this.stackPanel52.Controls.Add(this.labelShowAccounts);
            this.stackPanel52.Controls.Add(this.arrowButton3);
            this.stackPanel52.FlowDirection = System.Windows.Forms.FlowDirection.LeftToRight;
            this.stackPanel52.Location = new System.Drawing.Point(329, 0);
            this.stackPanel52.Margin = new System.Windows.Forms.Padding(0);
            this.stackPanel52.Size = new System.Drawing.Size(56, 13);
            // 
            // labelShowAccounts
            // 
            this.labelShowAccounts.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.labelShowAccounts.AutoSize = true;
            this.labelShowAccounts.Cursor = System.Windows.Forms.Cursors.Hand;
            this.labelShowAccounts.Font = new System.Drawing.Font("Segoe UI Semilight", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.labelShowAccounts.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(45)))), ((int)(((byte)(45)))), ((int)(((byte)(45)))));
            this.labelShowAccounts.Location = new System.Drawing.Point(0, 0);
            this.labelShowAccounts.Margin = new System.Windows.Forms.Padding(0);
            this.labelShowAccounts.Padding = new System.Windows.Forms.Padding(0, 0, 3, 0);
            this.labelShowAccounts.Size = new System.Drawing.Size(51, 13);
            this.labelShowAccounts.Text = "accounts";
            this.labelShowAccounts.Click += new System.EventHandler(this.labelShowAccounts_Click);
            // 
            // arrowButton3
            // 
            this.arrowButton3.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.arrowButton3.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(150)))), ((int)(((byte)(150)))), ((int)(((byte)(150)))));
            this.arrowButton3.ForeColorHighlight = System.Drawing.Color.FromArgb(((int)(((byte)(150)))), ((int)(((byte)(150)))), ((int)(((byte)(150)))));
            this.arrowButton3.Cursor = System.Windows.Forms.Cursors.Hand;
            this.arrowButton3.Direction = Gw2Launcher.UI.Controls.ArrowButton.ArrowDirection.Right;
            this.arrowButton3.Location = new System.Drawing.Point(51, 3);
            this.arrowButton3.Margin = new System.Windows.Forms.Padding(0, 1, 0, 0);
            this.arrowButton3.Size = new System.Drawing.Size(5, 9);
            this.arrowButton3.Click += new System.EventHandler(this.labelShowAccounts_Click);
            // 
            // labelCreated
            // 
            this.labelCreated.AutoSize = true;
            this.labelCreated.Font = new System.Drawing.Font("Segoe UI Semilight", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.labelCreated.Location = new System.Drawing.Point(1, 14);
            this.labelCreated.Margin = new System.Windows.Forms.Padding(1, 1, 0, 0);
            this.labelCreated.Size = new System.Drawing.Size(157, 13);
            this.labelCreated.Text = "Created on {0:MMMM dd, yyyy}";
            // 
            // gridFiles
            // 
            this.gridFiles.AllowUserToAddRows = false;
            this.gridFiles.AllowUserToDeleteRows = false;
            this.gridFiles.AllowUserToResizeColumns = false;
            this.gridFiles.AllowUserToResizeRows = false;
            this.gridFiles.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.gridFiles.BackgroundColor = System.Drawing.SystemColors.Window;
            this.gridFiles.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.gridFiles.CellBorderStyle = System.Windows.Forms.DataGridViewCellBorderStyle.None;
            this.gridFiles.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.gridFiles.ColumnHeadersVisible = false;
            this.gridFiles.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.columnPath});
            this.gridFiles.EditMode = System.Windows.Forms.DataGridViewEditMode.EditProgrammatically;
            this.gridFiles.Location = new System.Drawing.Point(3, 37);
            this.gridFiles.Margin = new System.Windows.Forms.Padding(3, 10, 3, 0);
            this.gridFiles.MultiSelect = false;
            this.gridFiles.ReadOnly = true;
            this.gridFiles.RowHeadersVisible = false;
            this.gridFiles.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.gridFiles.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.gridFiles.Size = new System.Drawing.Size(379, 151);
            this.gridFiles.CellMouseDown += new System.Windows.Forms.DataGridViewCellMouseEventHandler(this.gridFiles_CellMouseDown);
            this.gridFiles.SelectionChanged += new System.EventHandler(this.gridFiles_SelectionChanged);
            // 
            // columnPath
            // 
            this.columnPath.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
            dataGridViewCellStyle1.Padding = new System.Windows.Forms.Padding(3, 0, 0, 0);
            this.columnPath.DefaultCellStyle = dataGridViewCellStyle1;
            this.columnPath.HeaderText = "";
            this.columnPath.ReadOnly = true;
            // 
            // panelAccounts
            // 
            this.panelAccounts.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.panelAccounts.Controls.Add(this.stackPanel5);
            this.panelAccounts.Controls.Add(this.label5);
            this.panelAccounts.Controls.Add(this.labelAccountsFailed);
            this.panelAccounts.Controls.Add(this.panelAccountsWaiting);
            this.panelAccounts.Controls.Add(this.gridAccounts);
            this.panelAccounts.Enabled = false;
            this.panelAccounts.Location = new System.Drawing.Point(12, 12);
            this.panelAccounts.Size = new System.Drawing.Size(385, 188);
            this.panelAccounts.Visible = false;
            // 
            // stackPanel5
            // 
            this.stackPanel5.AutoSize = true;
            this.stackPanel5.AutoSizeFill = Gw2Launcher.UI.Controls.StackPanel.AutoSizeFillMode.NoWrap;
            this.stackPanel5.Controls.Add(this.arrowButton1);
            this.stackPanel5.Controls.Add(this.labelAccountsBack);
            this.stackPanel5.Dock = System.Windows.Forms.DockStyle.Right;
            this.stackPanel5.FlowDirection = System.Windows.Forms.FlowDirection.LeftToRight;
            this.stackPanel5.Location = new System.Drawing.Point(348, 0);
            this.stackPanel5.Margin = new System.Windows.Forms.Padding(0);
            this.stackPanel5.Size = new System.Drawing.Size(37, 13);
            // 
            // arrowButton1
            // 
            this.arrowButton1.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.arrowButton1.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(150)))), ((int)(((byte)(150)))), ((int)(((byte)(150)))));
            this.arrowButton1.ForeColorHighlight = System.Drawing.Color.FromArgb(((int)(((byte)(150)))), ((int)(((byte)(150)))), ((int)(((byte)(150)))));
            this.arrowButton1.Cursor = System.Windows.Forms.Cursors.Hand;
            this.arrowButton1.Direction = Gw2Launcher.UI.Controls.ArrowButton.ArrowDirection.Left;
            this.arrowButton1.Location = new System.Drawing.Point(0, 3);
            this.arrowButton1.Margin = new System.Windows.Forms.Padding(0, 1, 0, 0);
            this.arrowButton1.Size = new System.Drawing.Size(5, 9);
            this.arrowButton1.Click += new System.EventHandler(this.labelAccountsBack_Click);
            // 
            // labelAccountsBack
            // 
            this.labelAccountsBack.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.labelAccountsBack.AutoSize = true;
            this.labelAccountsBack.Cursor = System.Windows.Forms.Cursors.Hand;
            this.labelAccountsBack.Font = new System.Drawing.Font("Segoe UI Semilight", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.labelAccountsBack.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(45)))), ((int)(((byte)(45)))), ((int)(((byte)(45)))));
            this.labelAccountsBack.Location = new System.Drawing.Point(5, 0);
            this.labelAccountsBack.Margin = new System.Windows.Forms.Padding(0);
            this.labelAccountsBack.Padding = new System.Windows.Forms.Padding(3, 0, 0, 0);
            this.labelAccountsBack.Size = new System.Drawing.Size(32, 13);
            this.labelAccountsBack.Text = "back";
            this.labelAccountsBack.Click += new System.EventHandler(this.labelAccountsBack_Click);
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Font = new System.Drawing.Font("Segoe UI Semibold", 9F, System.Drawing.FontStyle.Bold);
            this.label5.Location = new System.Drawing.Point(0, 0);
            this.label5.Margin = new System.Windows.Forms.Padding(0);
            this.label5.Size = new System.Drawing.Size(57, 15);
            this.label5.Text = "Accounts";
            // 
            // labelAccountsFailed
            // 
            this.labelAccountsFailed.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.labelAccountsFailed.AutoSize = true;
            this.labelAccountsFailed.ForeColor = System.Drawing.SystemColors.GrayText;
            this.labelAccountsFailed.Location = new System.Drawing.Point(0, 15);
            this.labelAccountsFailed.Margin = new System.Windows.Forms.Padding(0);
            this.labelAccountsFailed.Size = new System.Drawing.Size(385, 107);
            this.labelAccountsFailed.Text = "Unable to preview accounts";
            this.labelAccountsFailed.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.labelAccountsFailed.Visible = false;
            // 
            // panelAccountsWaiting
            // 
            this.panelAccountsWaiting.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)));
            this.panelAccountsWaiting.AutoSize = true;
            this.panelAccountsWaiting.Controls.Add(this.waitingBounce1);
            this.panelAccountsWaiting.FlowDirection = System.Windows.Forms.FlowDirection.LeftToRight;
            this.panelAccountsWaiting.Location = new System.Drawing.Point(142, 122);
            this.panelAccountsWaiting.Margin = new System.Windows.Forms.Padding(0, 0, 0, 20);
            this.panelAccountsWaiting.Size = new System.Drawing.Size(100, 16);
            // 
            // waitingBounce1
            // 
            this.waitingBounce1.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.waitingBounce1.ForeColor = System.Drawing.SystemColors.ControlDark;
            this.waitingBounce1.Location = new System.Drawing.Point(0, 0);
            this.waitingBounce1.Margin = new System.Windows.Forms.Padding(0);
            this.waitingBounce1.Size = new System.Drawing.Size(100, 16);
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
            this.gridAccounts.BackgroundColor = System.Drawing.SystemColors.Window;
            this.gridAccounts.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.gridAccounts.CellBorderStyle = System.Windows.Forms.DataGridViewCellBorderStyle.None;
            this.gridAccounts.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.gridAccounts.ColumnHeadersVisible = false;
            this.gridAccounts.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.columnIcon,
            this.columnName,
            this.columnLastUsed});
            this.gridAccounts.EditMode = System.Windows.Forms.DataGridViewEditMode.EditProgrammatically;
            this.gridAccounts.Location = new System.Drawing.Point(3, 168);
            this.gridAccounts.Margin = new System.Windows.Forms.Padding(3, 10, 3, 0);
            this.gridAccounts.MinimumSize = new System.Drawing.Size(0, 20);
            this.gridAccounts.MultiSelect = false;
            this.gridAccounts.ReadOnly = true;
            this.gridAccounts.RowHeadersVisible = false;
            this.gridAccounts.RowTemplate.Height = 25;
            this.gridAccounts.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.gridAccounts.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.gridAccounts.Size = new System.Drawing.Size(379, 20);
            this.gridAccounts.Visible = false;
            this.gridAccounts.SelectionChanged += new System.EventHandler(this.gridAccounts_SelectionChanged);
            // 
            // columnIcon
            // 
            this.columnIcon.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.AllCells;
            dataGridViewCellStyle2.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleCenter;
            dataGridViewCellStyle2.NullValue = ((object)(resources.GetObject("dataGridViewCellStyle2.NullValue")));
            dataGridViewCellStyle2.Padding = new System.Windows.Forms.Padding(3, 0, 0, 0);
            this.columnIcon.DefaultCellStyle = dataGridViewCellStyle2;
            this.columnIcon.HeaderText = "";
            this.columnIcon.ReadOnly = true;
            this.columnIcon.Width = 5;
            // 
            // columnName
            // 
            this.columnName.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
            dataGridViewCellStyle3.Padding = new System.Windows.Forms.Padding(3, 0, 0, 0);
            this.columnName.DefaultCellStyle = dataGridViewCellStyle3;
            this.columnName.HeaderText = "";
            this.columnName.ReadOnly = true;
            // 
            // columnLastUsed
            // 
            this.columnLastUsed.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.AllCells;
            dataGridViewCellStyle4.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleRight;
            dataGridViewCellStyle4.ForeColor = System.Drawing.SystemColors.GrayText;
            dataGridViewCellStyle4.Padding = new System.Windows.Forms.Padding(0, 0, 3, 0);
            this.columnLastUsed.DefaultCellStyle = dataGridViewCellStyle4;
            this.columnLastUsed.HeaderText = "";
            this.columnLastUsed.ReadOnly = true;
            this.columnLastUsed.Width = 5;
            // 
            // contextFiles
            // 
            this.contextFiles.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.extractToolStripMenuItem,
            this.extractAllToolStripMenuItem});
            this.contextFiles.Size = new System.Drawing.Size(153, 70);
            // 
            // extractToolStripMenuItem
            // 
            this.extractToolStripMenuItem.Size = new System.Drawing.Size(152, 22);
            this.extractToolStripMenuItem.Text = "Extract";
            this.extractToolStripMenuItem.Click += new System.EventHandler(this.extractToolStripMenuItem_Click);
            // 
            // extractAllToolStripMenuItem
            // 
            this.extractAllToolStripMenuItem.Size = new System.Drawing.Size(152, 22);
            this.extractAllToolStripMenuItem.Text = "Extract all";
            this.extractAllToolStripMenuItem.Click += new System.EventHandler(this.extractAllToolStripMenuItem_Click);
            // 
            // formBackupRestore
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
            this.ClientSize = new System.Drawing.Size(409, 258);
            this.Controls.Add(this.buttonCancel);
            this.Controls.Add(this.buttonRestore);
            this.Controls.Add(this.panelFiles);
            this.Controls.Add(this.panelAccounts);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.MinimumSize = new System.Drawing.Size(250, 200);
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.panelFiles.ResumeLayout(false);
            this.panelFiles.PerformLayout();
            this.stackPanel2.ResumeLayout(false);
            this.stackPanel2.PerformLayout();
            this.stackPanel52.ResumeLayout(false);
            this.stackPanel52.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.gridFiles)).EndInit();
            this.panelAccounts.ResumeLayout(false);
            this.panelAccounts.PerformLayout();
            this.stackPanel5.ResumeLayout(false);
            this.stackPanel5.PerformLayout();
            this.panelAccountsWaiting.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.gridAccounts)).EndInit();
            this.contextFiles.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private Controls.StackPanel panelFiles;
        private System.Windows.Forms.Label labelName;
        private System.Windows.Forms.Label labelCreated;
        private Controls.ScaledDataGridView gridFiles;
        private System.Windows.Forms.Button buttonCancel;
        private System.Windows.Forms.Button buttonRestore;
        private Controls.StackPanel stackPanel2;
        private Controls.StackPanel stackPanel52;
        private System.Windows.Forms.Label labelShowAccounts;
        private Controls.StackPanel panelAccounts;
        private Controls.StackPanel stackPanel5;
        private Controls.ArrowButton arrowButton1;
        private System.Windows.Forms.Label labelAccountsBack;
        private System.Windows.Forms.Label label5;
        private Controls.ScaledDataGridView gridAccounts;
        private Controls.ArrowButton arrowButton3;
        private Controls.WaitingBounce waitingBounce1;
        private Controls.StackPanel panelAccountsWaiting;
        private System.Windows.Forms.Label labelAccountsFailed;
        private System.Windows.Forms.DataGridViewTextBoxColumn columnPath;
        private System.Windows.Forms.DataGridViewImageColumn columnIcon;
        private System.Windows.Forms.DataGridViewTextBoxColumn columnName;
        private System.Windows.Forms.DataGridViewTextBoxColumn columnLastUsed;
        private System.Windows.Forms.ContextMenuStrip contextFiles;
        private System.Windows.Forms.ToolStripMenuItem extractToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem extractAllToolStripMenuItem;
    }
}
