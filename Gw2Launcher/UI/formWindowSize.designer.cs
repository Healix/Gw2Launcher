namespace Gw2Launcher.UI
{
    partial class formWindowSize
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
            this.saveToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem1 = new System.Windows.Forms.ToolStripSeparator();
            this.cancelToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.showOtherAccountsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.saveAllToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem2 = new System.Windows.Forms.ToolStripSeparator();
            this.matchProcessToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem3 = new System.Windows.Forms.ToolStripSeparator();
            this.applyBoundsToProcessToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.contextMenu.SuspendLayout();
            this.SuspendLayout();
            // 
            // contextMenu
            // 
            this.contextMenu.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.saveToolStripMenuItem,
            this.toolStripMenuItem2,
            this.saveAllToolStripMenuItem,
            this.showOtherAccountsToolStripMenuItem,
            this.toolStripMenuItem1,
            this.matchProcessToolStripMenuItem,
            this.applyBoundsToProcessToolStripMenuItem,
            this.toolStripMenuItem3,
            this.cancelToolStripMenuItem});
            this.contextMenu.Name = "contextMenuStrip1";
            this.contextMenu.Size = new System.Drawing.Size(218, 176);
            // 
            // saveToolStripMenuItem
            // 
            this.saveToolStripMenuItem.Name = "saveToolStripMenuItem";
            this.saveToolStripMenuItem.Size = new System.Drawing.Size(110, 22);
            this.saveToolStripMenuItem.Text = "Save";
            this.saveToolStripMenuItem.Click += new System.EventHandler(this.saveToolStripMenuItem_Click);
            // 
            // toolStripMenuItem1
            // 
            this.toolStripMenuItem1.Name = "toolStripMenuItem1";
            this.toolStripMenuItem1.Size = new System.Drawing.Size(107, 6);
            // 
            // cancelToolStripMenuItem
            // 
            this.cancelToolStripMenuItem.Name = "cancelToolStripMenuItem";
            this.cancelToolStripMenuItem.Size = new System.Drawing.Size(110, 22);
            this.cancelToolStripMenuItem.Text = "Cancel";
            this.cancelToolStripMenuItem.Click += new System.EventHandler(this.cancelToolStripMenuItem_Click);
            // 
            // showOtherAccountsToolStripMenuItem
            // 
            this.showOtherAccountsToolStripMenuItem.Name = "showOtherAccountsToolStripMenuItem";
            this.showOtherAccountsToolStripMenuItem.Size = new System.Drawing.Size(185, 22);
            this.showOtherAccountsToolStripMenuItem.Text = "Show other accounts";
            this.showOtherAccountsToolStripMenuItem.Click += new System.EventHandler(this.showOtherAccountsToolStripMenuItem_Click);
            // 
            // saveAllToolStripMenuItem
            // 
            this.saveAllToolStripMenuItem.Name = "saveAllToolStripMenuItem";
            this.saveAllToolStripMenuItem.Size = new System.Drawing.Size(185, 22);
            this.saveAllToolStripMenuItem.Text = "Save all";
            this.saveAllToolStripMenuItem.Click += new System.EventHandler(this.saveAllToolStripMenuItem_Click);
            // 
            // toolStripMenuItem2
            // 
            this.toolStripMenuItem2.Name = "toolStripMenuItem2";
            this.toolStripMenuItem2.Size = new System.Drawing.Size(182, 6);
            // 
            // matchProcessToolStripMenuItem
            // 
            this.matchProcessToolStripMenuItem.Name = "matchProcessToolStripMenuItem";
            this.matchProcessToolStripMenuItem.Size = new System.Drawing.Size(217, 22);
            this.matchProcessToolStripMenuItem.Text = "Copy bounds from process";
            this.matchProcessToolStripMenuItem.Click += new System.EventHandler(this.matchProcessToolStripMenuItem_Click);
            // 
            // toolStripMenuItem3
            // 
            this.toolStripMenuItem3.Name = "toolStripMenuItem3";
            this.toolStripMenuItem3.Size = new System.Drawing.Size(182, 6);
            // 
            // applyBoundsToProcessToolStripMenuItem
            // 
            this.applyBoundsToProcessToolStripMenuItem.Name = "applyBoundsToProcessToolStripMenuItem";
            this.applyBoundsToProcessToolStripMenuItem.Size = new System.Drawing.Size(217, 22);
            this.applyBoundsToProcessToolStripMenuItem.Text = "Apply bounds to process";
            this.applyBoundsToProcessToolStripMenuItem.Click += new System.EventHandler(this.applyBoundsToProcessToolStripMenuItem_Click);
            // 
            // formWindowSize
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.White;
            this.ClientSize = new System.Drawing.Size(276, 225);
            this.Cursor = System.Windows.Forms.Cursors.SizeAll;
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.Icon = global::Gw2Launcher.Properties.Resources.Gw2;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.MinimumSize = new System.Drawing.Size(50, 50);
            this.Name = "formWindowSize";
            this.Opacity = 0.9D;
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.Manual;
            this.TopMost = true;
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.formWindowSize_FormClosing);
            this.Load += new System.EventHandler(this.formWindowSize_Load);
            this.Paint += new System.Windows.Forms.PaintEventHandler(this.formWindowSize_Paint);
            this.KeyDown += new System.Windows.Forms.KeyEventHandler(this.formWindowSize_KeyDown);
            this.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.formWindowSize_KeyPress);
            this.MouseDown += new System.Windows.Forms.MouseEventHandler(this.formWindowSize_MouseDown);
            this.MouseEnter += new System.EventHandler(this.formWindowSize_MouseEnter);
            this.MouseLeave += new System.EventHandler(this.formWindowSize_MouseLeave);
            this.MouseMove += new System.Windows.Forms.MouseEventHandler(this.formWindowSize_MouseMove);
            this.MouseUp += new System.Windows.Forms.MouseEventHandler(this.formWindowSize_MouseUp);
            this.contextMenu.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.ContextMenuStrip contextMenu;
        private System.Windows.Forms.ToolStripMenuItem saveToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripMenuItem1;
        private System.Windows.Forms.ToolStripMenuItem cancelToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem showOtherAccountsToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripMenuItem2;
        private System.Windows.Forms.ToolStripMenuItem saveAllToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem matchProcessToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripMenuItem3;
        private System.Windows.Forms.ToolStripMenuItem applyBoundsToProcessToolStripMenuItem;
    }
}