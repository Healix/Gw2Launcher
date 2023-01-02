namespace Gw2Launcher.UI.Affinity
{
    partial class formAffinitySelectDialog
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
            this.buttonCancel = new System.Windows.Forms.Button();
            this.buttonOK = new System.Windows.Forms.Button();
            this.stackPanel1 = new Gw2Launcher.UI.Controls.StackPanel();
            this.panelSave = new Gw2Launcher.UI.Controls.StackPanel();
            this.label3 = new System.Windows.Forms.Label();
            this.textName = new System.Windows.Forms.TextBox();
            this.adSave = new Gw2Launcher.UI.Controls.AffinityDisplay();
            this.gradientLine1 = new Gw2Launcher.UI.Controls.GradientLine();
            this.panelScroll = new Gw2Launcher.UI.Controls.AutoScrollContainerPanel();
            this.panelExisting = new Gw2Launcher.UI.Controls.StackPanel();
            this.label1 = new System.Windows.Forms.Label();
            this.labelNone = new System.Windows.Forms.Label();
            this.panelTemplate = new Gw2Launcher.UI.Controls.StackPanel();
            this.labelTemplate = new System.Windows.Forms.Label();
            this.adTemplate = new Gw2Launcher.UI.Controls.AffinityDisplay();
            this.contextMenu = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.editToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.deleteToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.stackPanel1.SuspendLayout();
            this.panelSave.SuspendLayout();
            this.panelScroll.SuspendLayout();
            this.panelExisting.SuspendLayout();
            this.panelTemplate.SuspendLayout();
            this.contextMenu.SuspendLayout();
            this.SuspendLayout();
            // 
            // buttonCancel
            // 
            this.buttonCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonCancel.Font = new System.Drawing.Font("Segoe UI", 8.25F);
            this.buttonCancel.Location = new System.Drawing.Point(132, 222);
            this.buttonCancel.Size = new System.Drawing.Size(86, 35);
            this.buttonCancel.Text = "Cancel";
            this.buttonCancel.UseVisualStyleBackColor = true;
            this.buttonCancel.Click += new System.EventHandler(this.buttonCancel_Click);
            // 
            // buttonOK
            // 
            this.buttonOK.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonOK.Font = new System.Drawing.Font("Segoe UI", 8.25F);
            this.buttonOK.Location = new System.Drawing.Point(228, 222);
            this.buttonOK.Size = new System.Drawing.Size(86, 35);
            this.buttonOK.Text = "OK";
            this.buttonOK.UseVisualStyleBackColor = true;
            this.buttonOK.Click += new System.EventHandler(this.buttonOK_Click);
            // 
            // stackPanel1
            // 
            this.stackPanel1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.stackPanel1.Controls.Add(this.panelSave);
            this.stackPanel1.Controls.Add(this.panelScroll);
            this.stackPanel1.Location = new System.Drawing.Point(12, 12);
            this.stackPanel1.Size = new System.Drawing.Size(304, 199);
            // 
            // panelSave
            // 
            this.panelSave.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.panelSave.AutoSize = true;
            this.panelSave.AutoSizeFill = Gw2Launcher.UI.Controls.StackPanel.AutoSizeFillMode.Width;
            this.panelSave.Controls.Add(this.label3);
            this.panelSave.Controls.Add(this.textName);
            this.panelSave.Controls.Add(this.adSave);
            this.panelSave.Controls.Add(this.gradientLine1);
            this.panelSave.Location = new System.Drawing.Point(0, 0);
            this.panelSave.Margin = new System.Windows.Forms.Padding(0, 0, 2, 0);
            this.panelSave.Size = new System.Drawing.Size(302, 85);
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Font = new System.Drawing.Font("Segoe UI Semibold", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label3.Location = new System.Drawing.Point(4, 0);
            this.label3.Margin = new System.Windows.Forms.Padding(4, 0, 0, 8);
            this.label3.Size = new System.Drawing.Size(45, 13);
            this.label3.Text = "Save as";
            // 
            // textName
            // 
            this.textName.Location = new System.Drawing.Point(8, 21);
            this.textName.Margin = new System.Windows.Forms.Padding(8, 0, 8, 0);
            this.textName.Size = new System.Drawing.Size(228, 22);
            // 
            // adSave
            // 
            this.adSave.Affinity = ((long)(0));
            this.adSave.AutoSize = true;
            this.adSave.Location = new System.Drawing.Point(8, 53);
            this.adSave.Margin = new System.Windows.Forms.Padding(8, 10, 8, 10);
            this.adSave.Size = new System.Drawing.Size(108, 16);
            this.adSave.Text = "affinityDisplay1";
            // 
            // gradientLine1
            // 
            this.gradientLine1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.gradientLine1.Location = new System.Drawing.Point(0, 79);
            this.gradientLine1.Margin = new System.Windows.Forms.Padding(0, 0, 40, 5);
            this.gradientLine1.Size = new System.Drawing.Size(262, 1);
            this.gradientLine1.Text = "gradientLine1";
            // 
            // panelScroll
            // 
            this.panelScroll.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.panelScroll.Controls.Add(this.panelExisting);
            this.panelScroll.Location = new System.Drawing.Point(0, 85);
            this.panelScroll.Margin = new System.Windows.Forms.Padding(0);
            this.panelScroll.Size = new System.Drawing.Size(304, 114);
            // 
            // panelExisting
            // 
            this.panelExisting.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.panelExisting.AutoSize = true;
            this.panelExisting.AutoSizeFill = Gw2Launcher.UI.Controls.StackPanel.AutoSizeFillMode.Width;
            this.panelExisting.Controls.Add(this.label1);
            this.panelExisting.Controls.Add(this.labelNone);
            this.panelExisting.Controls.Add(this.panelTemplate);
            this.panelExisting.Location = new System.Drawing.Point(0, 0);
            this.panelExisting.Margin = new System.Windows.Forms.Padding(0);
            this.panelExisting.Padding = new System.Windows.Forms.Padding(0, 0, 0, 10);
            this.panelExisting.Size = new System.Drawing.Size(304, 95);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("Segoe UI Semibold", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label1.Location = new System.Drawing.Point(5, 5);
            this.label1.Margin = new System.Windows.Forms.Padding(5);
            this.label1.Size = new System.Drawing.Size(114, 13);
            this.label1.Text = "Saved configurations";
            // 
            // labelNone
            // 
            this.labelNone.AutoSize = true;
            this.labelNone.ForeColor = System.Drawing.SystemColors.GrayText;
            this.labelNone.Location = new System.Drawing.Point(5, 28);
            this.labelNone.Margin = new System.Windows.Forms.Padding(5, 5, 5, 0);
            this.labelNone.Size = new System.Drawing.Size(85, 13);
            this.labelNone.Text = "Nothing found";
            this.labelNone.Visible = false;
            // 
            // panelTemplate
            // 
            this.panelTemplate.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.panelTemplate.AutoSize = true;
            this.panelTemplate.Controls.Add(this.labelTemplate);
            this.panelTemplate.Controls.Add(this.adTemplate);
            this.panelTemplate.Location = new System.Drawing.Point(0, 41);
            this.panelTemplate.Margin = new System.Windows.Forms.Padding(0, 0, 2, 0);
            this.panelTemplate.Padding = new System.Windows.Forms.Padding(5);
            this.panelTemplate.Size = new System.Drawing.Size(302, 44);
            this.panelTemplate.Visible = false;
            // 
            // labelTemplate
            // 
            this.labelTemplate.AutoSize = true;
            this.labelTemplate.Font = new System.Drawing.Font("Segoe UI Semilight", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.labelTemplate.Location = new System.Drawing.Point(5, 5);
            this.labelTemplate.Margin = new System.Windows.Forms.Padding(0, 0, 0, 5);
            this.labelTemplate.Size = new System.Drawing.Size(36, 13);
            this.labelTemplate.Text = "Name";
            // 
            // adTemplate
            // 
            this.adTemplate.Affinity = ((long)(0));
            this.adTemplate.AutoSize = true;
            this.adTemplate.Location = new System.Drawing.Point(8, 23);
            this.adTemplate.Margin = new System.Windows.Forms.Padding(3, 0, 3, 0);
            this.adTemplate.Size = new System.Drawing.Size(108, 16);
            this.adTemplate.Text = "affinityDisplay1";
            // 
            // contextMenu
            // 
            this.contextMenu.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.editToolStripMenuItem,
            this.deleteToolStripMenuItem});
            this.contextMenu.Size = new System.Drawing.Size(108, 48);
            // 
            // editToolStripMenuItem
            // 
            this.editToolStripMenuItem.Size = new System.Drawing.Size(107, 22);
            this.editToolStripMenuItem.Text = "Edit";
            this.editToolStripMenuItem.Click += new System.EventHandler(this.editToolStripMenuItem_Click);
            // 
            // deleteToolStripMenuItem
            // 
            this.deleteToolStripMenuItem.Size = new System.Drawing.Size(107, 22);
            this.deleteToolStripMenuItem.Text = "Delete";
            this.deleteToolStripMenuItem.Click += new System.EventHandler(this.deleteToolStripMenuItem_Click);
            // 
            // formAffinitySelectDialog
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
            this.ClientSize = new System.Drawing.Size(326, 269);
            this.Controls.Add(this.buttonCancel);
            this.Controls.Add(this.buttonOK);
            this.Controls.Add(this.stackPanel1);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.MinimumSize = new System.Drawing.Size(300, 250);
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.stackPanel1.ResumeLayout(false);
            this.stackPanel1.PerformLayout();
            this.panelSave.ResumeLayout(false);
            this.panelSave.PerformLayout();
            this.panelScroll.ResumeLayout(false);
            this.panelScroll.PerformLayout();
            this.panelExisting.ResumeLayout(false);
            this.panelExisting.PerformLayout();
            this.panelTemplate.ResumeLayout(false);
            this.panelTemplate.PerformLayout();
            this.contextMenu.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private Controls.AutoScrollContainerPanel panelScroll;
        private Controls.StackPanel panelExisting;
        private System.Windows.Forms.Button buttonCancel;
        private System.Windows.Forms.Button buttonOK;
        private Controls.StackPanel panelSave;
        private Controls.AffinityDisplay adSave;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.TextBox textName;
        private System.Windows.Forms.Label label1;
        private Controls.StackPanel stackPanel1;
        private Controls.GradientLine gradientLine1;
        private System.Windows.Forms.Label labelTemplate;
        private Controls.StackPanel panelTemplate;
        private Controls.AffinityDisplay adTemplate;
        private System.Windows.Forms.Label labelNone;
        private System.Windows.Forms.ContextMenuStrip contextMenu;
        private System.Windows.Forms.ToolStripMenuItem deleteToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem editToolStripMenuItem;
    }
}
