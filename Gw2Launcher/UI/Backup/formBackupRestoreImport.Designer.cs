namespace Gw2Launcher.UI.Backup
{
    partial class formBackupRestoreImport
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
            this.progressRestore = new Gw2Launcher.UI.Controls.FlatProgressBar();
            this.labelProgress = new System.Windows.Forms.Label();
            this.stackPanel1 = new Gw2Launcher.UI.Controls.StackPanel();
            this.stackPanel1.SuspendLayout();
            this.SuspendLayout();
            // 
            // progressRestore
            // 
            this.progressRestore.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.progressRestore.Animated = false;
            this.progressRestore.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(235)))), ((int)(((byte)(235)))), ((int)(((byte)(235)))));
            this.progressRestore.ForeColor = System.Drawing.Color.LightSteelBlue;
            this.progressRestore.Location = new System.Drawing.Point(3, 0);
            this.progressRestore.Margin = new System.Windows.Forms.Padding(3, 0, 3, 0);
            this.progressRestore.Maximum = ((long)(1000));
            this.progressRestore.Size = new System.Drawing.Size(260, 50);
            this.progressRestore.Value = ((long)(0));
            // 
            // labelProgress
            // 
            this.labelProgress.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.labelProgress.AutoEllipsis = true;
            this.labelProgress.Location = new System.Drawing.Point(0, 55);
            this.labelProgress.Margin = new System.Windows.Forms.Padding(0, 5, 0, 0);
            this.labelProgress.Size = new System.Drawing.Size(266, 15);
            this.labelProgress.Text = "Waiting...";
            // 
            // stackPanel1
            // 
            this.stackPanel1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.stackPanel1.Controls.Add(this.progressRestore);
            this.stackPanel1.Controls.Add(this.labelProgress);
            this.stackPanel1.Location = new System.Drawing.Point(9, 12);
            this.stackPanel1.Size = new System.Drawing.Size(266, 70);
            // 
            // formBackupRestoreImport
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(284, 94);
            this.Controls.Add(this.stackPanel1);
            this.Font = new System.Drawing.Font("Segoe UI Semilight", 8.25F);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Restoring...";
            this.stackPanel1.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private Controls.FlatProgressBar progressRestore;
        private System.Windows.Forms.Label labelProgress;
        private Controls.StackPanel stackPanel1;
    }
}
