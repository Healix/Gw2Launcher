namespace Gw2Launcher.UI
{
    partial class formGw2Cache
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
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle3 = new System.Windows.Forms.DataGridViewCellStyle();
            this.labelSize = new System.Windows.Forms.Label();
            this.buttonDelete = new System.Windows.Forms.Button();
            this.gridCache = new System.Windows.Forms.DataGridView();
            this.columnUser = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.columnFolders = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.columnSize = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.label1 = new System.Windows.Forms.Label();
            this.checkDeleteCacheOnLaunch = new System.Windows.Forms.CheckBox();
            this.contextMenu = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.showInExplorerToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            ((System.ComponentModel.ISupportInitialize)(this.gridCache)).BeginInit();
            this.contextMenu.SuspendLayout();
            this.SuspendLayout();
            // 
            // labelSize
            // 
            this.labelSize.AutoSize = true;
            this.labelSize.Font = new System.Drawing.Font("Segoe UI Semilight", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.labelSize.Location = new System.Drawing.Point(9, 269);
            this.labelSize.Name = "labelSize";
            this.labelSize.Size = new System.Drawing.Size(85, 13);
            this.labelSize.TabIndex = 19;
            this.labelSize.Text = "0 MB in 0 folders";
            // 
            // buttonDelete
            // 
            this.buttonDelete.Font = new System.Drawing.Font("Segoe UI", 8.25F);
            this.buttonDelete.Location = new System.Drawing.Point(304, 247);
            this.buttonDelete.Name = "buttonDelete";
            this.buttonDelete.Size = new System.Drawing.Size(110, 35);
            this.buttonDelete.TabIndex = 21;
            this.buttonDelete.Text = "Delete Cache";
            this.buttonDelete.UseVisualStyleBackColor = true;
            this.buttonDelete.Click += new System.EventHandler(this.buttonDelete_Click);
            // 
            // gridCache
            // 
            this.gridCache.AllowUserToAddRows = false;
            this.gridCache.AllowUserToDeleteRows = false;
            this.gridCache.AllowUserToResizeColumns = false;
            this.gridCache.AllowUserToResizeRows = false;
            this.gridCache.BackgroundColor = System.Drawing.SystemColors.Window;
            this.gridCache.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.gridCache.CellBorderStyle = System.Windows.Forms.DataGridViewCellBorderStyle.None;
            this.gridCache.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.gridCache.ColumnHeadersVisible = false;
            this.gridCache.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.columnUser,
            this.columnFolders,
            this.columnSize});
            this.gridCache.EditMode = System.Windows.Forms.DataGridViewEditMode.EditProgrammatically;
            this.gridCache.Location = new System.Drawing.Point(12, 85);
            this.gridCache.MultiSelect = false;
            this.gridCache.Name = "gridCache";
            this.gridCache.ReadOnly = true;
            this.gridCache.RowHeadersVisible = false;
            this.gridCache.RowTemplate.Height = 24;
            this.gridCache.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.gridCache.Size = new System.Drawing.Size(400, 144);
            this.gridCache.TabIndex = 26;
            this.gridCache.CellContentClick += new System.Windows.Forms.DataGridViewCellEventHandler(this.gridCache_CellContentClick);
            this.gridCache.CellMouseClick += new System.Windows.Forms.DataGridViewCellMouseEventHandler(this.gridCache_CellMouseClick);
            this.gridCache.SelectionChanged += new System.EventHandler(this.gridCache_SelectionChanged);
            // 
            // columnUser
            // 
            this.columnUser.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
            dataGridViewCellStyle1.Font = new System.Drawing.Font("Segoe UI", 8.25F);
            dataGridViewCellStyle1.Padding = new System.Windows.Forms.Padding(5, 0, 0, 0);
            this.columnUser.DefaultCellStyle = dataGridViewCellStyle1;
            this.columnUser.HeaderText = "User";
            this.columnUser.Name = "columnUser";
            this.columnUser.ReadOnly = true;
            // 
            // columnFolders
            // 
            dataGridViewCellStyle2.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleRight;
            this.columnFolders.DefaultCellStyle = dataGridViewCellStyle2;
            this.columnFolders.HeaderText = "Folders";
            this.columnFolders.Name = "columnFolders";
            this.columnFolders.ReadOnly = true;
            this.columnFolders.Width = 80;
            // 
            // columnSize
            // 
            dataGridViewCellStyle3.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleRight;
            dataGridViewCellStyle3.Padding = new System.Windows.Forms.Padding(0, 0, 5, 0);
            this.columnSize.DefaultCellStyle = dataGridViewCellStyle3;
            this.columnSize.HeaderText = "Size";
            this.columnSize.Name = "columnSize";
            this.columnSize.ReadOnly = true;
            this.columnSize.Width = 80;
            // 
            // label1
            // 
            this.label1.Font = new System.Drawing.Font("Segoe UI Semilight", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label1.Location = new System.Drawing.Point(10, 10);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(404, 34);
            this.label1.TabIndex = 28;
            this.label1.Text = "The web browser used within the game may create a new cache folder with every lau" +
    "nch. These files can be safely deleted.";
            // 
            // checkDeleteCacheOnLaunch
            // 
            this.checkDeleteCacheOnLaunch.AutoSize = true;
            this.checkDeleteCacheOnLaunch.Font = new System.Drawing.Font("Segoe UI", 8.25F);
            this.checkDeleteCacheOnLaunch.Location = new System.Drawing.Point(18, 52);
            this.checkDeleteCacheOnLaunch.Name = "checkDeleteCacheOnLaunch";
            this.checkDeleteCacheOnLaunch.Size = new System.Drawing.Size(303, 17);
            this.checkDeleteCacheOnLaunch.TabIndex = 29;
            this.checkDeleteCacheOnLaunch.Text = "Automatically delete old cache folders on each launch";
            this.checkDeleteCacheOnLaunch.UseVisualStyleBackColor = true;
            this.checkDeleteCacheOnLaunch.CheckedChanged += new System.EventHandler(this.checkDeleteCacheOnLaunch_CheckedChanged);
            // 
            // contextMenu
            // 
            this.contextMenu.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.showInExplorerToolStripMenuItem});
            this.contextMenu.Name = "contextMenu";
            this.contextMenu.Size = new System.Drawing.Size(162, 26);
            // 
            // showInExplorerToolStripMenuItem
            // 
            this.showInExplorerToolStripMenuItem.Name = "showInExplorerToolStripMenuItem";
            this.showInExplorerToolStripMenuItem.Size = new System.Drawing.Size(161, 22);
            this.showInExplorerToolStripMenuItem.Text = "Show in Explorer";
            this.showInExplorerToolStripMenuItem.Click += new System.EventHandler(this.showInExplorerToolStripMenuItem_Click);
            // 
            // formGw2Cache
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(426, 295);
            this.Controls.Add(this.checkDeleteCacheOnLaunch);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.buttonDelete);
            this.Controls.Add(this.gridCache);
            this.Controls.Add(this.labelSize);
            this.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.Icon = global::Gw2Launcher.Properties.Resources.Gw2Launcher;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "formGw2Cache";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Load += new System.EventHandler(this.formGw2Cache_Load);
            ((System.ComponentModel.ISupportInitialize)(this.gridCache)).EndInit();
            this.contextMenu.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label labelSize;
        private System.Windows.Forms.Button buttonDelete;
        private System.Windows.Forms.DataGridView gridCache;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.CheckBox checkDeleteCacheOnLaunch;
        private System.Windows.Forms.DataGridViewTextBoxColumn columnUser;
        private System.Windows.Forms.DataGridViewTextBoxColumn columnFolders;
        private System.Windows.Forms.DataGridViewTextBoxColumn columnSize;
        private System.Windows.Forms.ContextMenuStrip contextMenu;
        private System.Windows.Forms.ToolStripMenuItem showInExplorerToolStripMenuItem;

    }
}