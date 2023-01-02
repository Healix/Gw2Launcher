namespace Gw2Launcher.UI.WindowPositioning
{
    partial class formCustomTemplate
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
            this.contextMenu = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.saveAsNewToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.saveAsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.newBaseTemplateToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.newAssignedTemplateToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.saveToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem3 = new System.Windows.Forms.ToolStripSeparator();
            this.assignToToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.accountToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.anyToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.noneToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.accountsIncludingToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.accountsExcludingToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.optionsToolStripMenuItem = new Gw2Launcher.UI.Controls.ToolStripMenuItemStayOpenOnClick();
            this.preventChangesToolStripMenuItem = new Gw2Launcher.UI.Controls.ToolStripMenuItemStayOpenOnClick();
            this.blockMinimizecloseButtonsToolStripMenuItem = new Gw2Launcher.UI.Controls.ToolStripMenuItemStayOpenOnClick();
            this.showOnTopOfOtherWindowsToolStripMenuItem = new Gw2Launcher.UI.Controls.ToolStripMenuItemStayOpenOnClick();
            this.deleteToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem1 = new System.Windows.Forms.ToolStripSeparator();
            this.resizeToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.includeWindowBordersToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.excludeWindowBordersToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.gridSizeToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem2 = new System.Windows.Forms.ToolStripSeparator();
            this.cancelToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.contextMenu.SuspendLayout();
            this.SuspendLayout();
            // 
            // contextMenu
            // 
            this.contextMenu.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.saveAsNewToolStripMenuItem,
            this.saveAsToolStripMenuItem,
            this.saveToolStripMenuItem,
            this.toolStripMenuItem3,
            this.assignToToolStripMenuItem,
            this.optionsToolStripMenuItem,
            this.deleteToolStripMenuItem,
            this.toolStripMenuItem1,
            this.resizeToolStripMenuItem,
            this.gridSizeToolStripMenuItem,
            this.toolStripMenuItem2,
            this.cancelToolStripMenuItem});
            this.contextMenu.Size = new System.Drawing.Size(188, 220);
            // 
            // saveAsNewToolStripMenuItem
            // 
            this.saveAsNewToolStripMenuItem.Size = new System.Drawing.Size(187, 22);
            this.saveAsNewToolStripMenuItem.Text = "Save as new template";
            this.saveAsNewToolStripMenuItem.Click += new System.EventHandler(this.saveAsNewToolStripMenuItem_Click);
            // 
            // saveAsToolStripMenuItem
            // 
            this.saveAsToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.newBaseTemplateToolStripMenuItem,
            this.newAssignedTemplateToolStripMenuItem});
            this.saveAsToolStripMenuItem.Size = new System.Drawing.Size(187, 22);
            this.saveAsToolStripMenuItem.Text = "Save as";
            this.saveAsToolStripMenuItem.Visible = false;
            // 
            // newBaseTemplateToolStripMenuItem
            // 
            this.newBaseTemplateToolStripMenuItem.Size = new System.Drawing.Size(197, 22);
            this.newBaseTemplateToolStripMenuItem.Text = "New base template";
            this.newBaseTemplateToolStripMenuItem.Click += new System.EventHandler(this.saveAsNewToolStripMenuItem_Click);
            // 
            // newAssignedTemplateToolStripMenuItem
            // 
            this.newAssignedTemplateToolStripMenuItem.Size = new System.Drawing.Size(197, 22);
            this.newAssignedTemplateToolStripMenuItem.Text = "New assigned template";
            this.newAssignedTemplateToolStripMenuItem.Click += new System.EventHandler(this.saveAsReferenceToolStripMenuItem_Click);
            // 
            // saveToolStripMenuItem
            // 
            this.saveToolStripMenuItem.Size = new System.Drawing.Size(187, 22);
            this.saveToolStripMenuItem.Text = "Save";
            this.saveToolStripMenuItem.Click += new System.EventHandler(this.saveToolStripMenuItem_Click);
            // 
            // toolStripMenuItem3
            // 
            this.toolStripMenuItem3.Size = new System.Drawing.Size(184, 6);
            // 
            // assignToToolStripMenuItem
            // 
            this.assignToToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.accountToolStripMenuItem,
            this.anyToolStripMenuItem,
            this.noneToolStripMenuItem,
            this.accountsIncludingToolStripMenuItem,
            this.accountsExcludingToolStripMenuItem});
            this.assignToToolStripMenuItem.Size = new System.Drawing.Size(187, 22);
            this.assignToToolStripMenuItem.Text = "Assign to";
            this.assignToToolStripMenuItem.Visible = false;
            // 
            // accountToolStripMenuItem
            // 
            this.accountToolStripMenuItem.Size = new System.Drawing.Size(152, 22);
            this.accountToolStripMenuItem.Text = "Account";
            this.accountToolStripMenuItem.Click += new System.EventHandler(this.accountToolStripMenuItem_Click);
            // 
            // anyToolStripMenuItem
            // 
            this.anyToolStripMenuItem.Size = new System.Drawing.Size(152, 22);
            this.anyToolStripMenuItem.Text = "Any";
            this.anyToolStripMenuItem.Click += new System.EventHandler(this.anyToolStripMenuItem_Click);
            // 
            // noneToolStripMenuItem
            // 
            this.noneToolStripMenuItem.Size = new System.Drawing.Size(152, 22);
            this.noneToolStripMenuItem.Text = "None";
            this.noneToolStripMenuItem.Click += new System.EventHandler(this.noneToolStripMenuItem_Click);
            // 
            // accountsIncludingToolStripMenuItem
            // 
            this.accountsIncludingToolStripMenuItem.Size = new System.Drawing.Size(152, 22);
            this.accountsIncludingToolStripMenuItem.Text = "Only...";
            this.accountsIncludingToolStripMenuItem.Click += new System.EventHandler(this.accountsIncludingToolStripMenuItem_Click);
            // 
            // accountsExcludingToolStripMenuItem
            // 
            this.accountsExcludingToolStripMenuItem.Size = new System.Drawing.Size(152, 22);
            this.accountsExcludingToolStripMenuItem.Text = "Any, except...";
            this.accountsExcludingToolStripMenuItem.Click += new System.EventHandler(this.accountsExcludingToolStripMenuItem_Click);
            // 
            // optionsToolStripMenuItem
            // 
            this.optionsToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.preventChangesToolStripMenuItem,
            this.blockMinimizecloseButtonsToolStripMenuItem,
            this.showOnTopOfOtherWindowsToolStripMenuItem});
            this.optionsToolStripMenuItem.Size = new System.Drawing.Size(187, 22);
            this.optionsToolStripMenuItem.StayOpenOnClick = true;
            this.optionsToolStripMenuItem.Text = "Window options";
            this.optionsToolStripMenuItem.Visible = false;
            // 
            // preventChangesToolStripMenuItem
            // 
            this.preventChangesToolStripMenuItem.Size = new System.Drawing.Size(236, 22);
            this.preventChangesToolStripMenuItem.StayOpenOnClick = true;
            this.preventChangesToolStripMenuItem.Text = "Prevent resizing/moving";
            this.preventChangesToolStripMenuItem.Click += new System.EventHandler(this.preventChangesToolStripMenuItem_Click);
            // 
            // blockMinimizecloseButtonsToolStripMenuItem
            // 
            this.blockMinimizecloseButtonsToolStripMenuItem.Size = new System.Drawing.Size(236, 22);
            this.blockMinimizecloseButtonsToolStripMenuItem.StayOpenOnClick = true;
            this.blockMinimizecloseButtonsToolStripMenuItem.Text = "Block minimize/close buttons";
            this.blockMinimizecloseButtonsToolStripMenuItem.Click += new System.EventHandler(this.blockMinimizecloseButtonsToolStripMenuItem_Click);
            // 
            // showOnTopOfOtherWindowsToolStripMenuItem
            // 
            this.showOnTopOfOtherWindowsToolStripMenuItem.Size = new System.Drawing.Size(236, 22);
            this.showOnTopOfOtherWindowsToolStripMenuItem.StayOpenOnClick = true;
            this.showOnTopOfOtherWindowsToolStripMenuItem.Text = "Show on top of other windows";
            this.showOnTopOfOtherWindowsToolStripMenuItem.Click += new System.EventHandler(this.showOnTopOfOtherWindowsToolStripMenuItem_Click);
            // 
            // deleteToolStripMenuItem
            // 
            this.deleteToolStripMenuItem.Size = new System.Drawing.Size(187, 22);
            this.deleteToolStripMenuItem.Text = "Delete";
            this.deleteToolStripMenuItem.Click += new System.EventHandler(this.deleteToolStripMenuItem_Click);
            // 
            // toolStripMenuItem1
            // 
            this.toolStripMenuItem1.Size = new System.Drawing.Size(184, 6);
            // 
            // resizeToolStripMenuItem
            // 
            this.resizeToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.includeWindowBordersToolStripMenuItem,
            this.excludeWindowBordersToolStripMenuItem});
            this.resizeToolStripMenuItem.Size = new System.Drawing.Size(187, 22);
            this.resizeToolStripMenuItem.Text = "Resize";
            // 
            // includeWindowBordersToolStripMenuItem
            // 
            this.includeWindowBordersToolStripMenuItem.Size = new System.Drawing.Size(203, 22);
            this.includeWindowBordersToolStripMenuItem.Text = "Include window borders";
            this.includeWindowBordersToolStripMenuItem.Click += new System.EventHandler(this.includeWindowBordersToolStripMenuItem_Click);
            // 
            // excludeWindowBordersToolStripMenuItem
            // 
            this.excludeWindowBordersToolStripMenuItem.Size = new System.Drawing.Size(203, 22);
            this.excludeWindowBordersToolStripMenuItem.Text = "Exclude window borders";
            this.excludeWindowBordersToolStripMenuItem.Click += new System.EventHandler(this.includeWindowBordersToolStripMenuItem_Click);
            // 
            // gridSizeToolStripMenuItem
            // 
            this.gridSizeToolStripMenuItem.Size = new System.Drawing.Size(187, 22);
            this.gridSizeToolStripMenuItem.Text = "Grid size...";
            this.gridSizeToolStripMenuItem.Click += new System.EventHandler(this.gridSizeToolStripMenuItem_Click);
            // 
            // toolStripMenuItem2
            // 
            this.toolStripMenuItem2.Size = new System.Drawing.Size(184, 6);
            // 
            // cancelToolStripMenuItem
            // 
            this.cancelToolStripMenuItem.Size = new System.Drawing.Size(187, 22);
            this.cancelToolStripMenuItem.Text = "Cancel";
            this.cancelToolStripMenuItem.Click += new System.EventHandler(this.cancelToolStripMenuItem_Click);
            // 
            // formCustomTemplate
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
            this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(248)))), ((int)(((byte)(248)))), ((int)(((byte)(248)))));
            this.ClientSize = new System.Drawing.Size(284, 261);
            this.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.Icon = global::Gw2Launcher.Properties.Resources.Gw2Launcher;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.Manual;
            this.contextMenu.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.ContextMenuStrip contextMenu;
        private System.Windows.Forms.ToolStripMenuItem saveAsNewToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem saveToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripMenuItem3;
        private System.Windows.Forms.ToolStripMenuItem deleteToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripMenuItem1;
        private System.Windows.Forms.ToolStripMenuItem gridSizeToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripMenuItem2;
        private System.Windows.Forms.ToolStripMenuItem cancelToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem assignToToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem accountToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem anyToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem resizeToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem includeWindowBordersToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem excludeWindowBordersToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem noneToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem accountsIncludingToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem accountsExcludingToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem saveAsToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem newBaseTemplateToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem newAssignedTemplateToolStripMenuItem;
        private UI.Controls.ToolStripMenuItemStayOpenOnClick optionsToolStripMenuItem;
        private UI.Controls.ToolStripMenuItemStayOpenOnClick preventChangesToolStripMenuItem;
        private UI.Controls.ToolStripMenuItemStayOpenOnClick blockMinimizecloseButtonsToolStripMenuItem;
        private UI.Controls.ToolStripMenuItemStayOpenOnClick showOnTopOfOtherWindowsToolStripMenuItem;
    }
}
