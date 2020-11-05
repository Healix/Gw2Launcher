namespace Gw2Launcher.UI
{
    partial class formCopyFileDialog
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
            this.flatProgressBar1 = new Gw2Launcher.UI.Controls.FlatProgressBar();
            this.label82 = new System.Windows.Forms.Label();
            this.labelRemaining = new System.Windows.Forms.Label();
            this.stackPanel1 = new Gw2Launcher.UI.Controls.StackPanel();
            this.stackPanel2 = new Gw2Launcher.UI.Controls.StackPanel();
            this.stackPanel1.SuspendLayout();
            this.stackPanel2.SuspendLayout();
            this.SuspendLayout();
            // 
            // flatProgressBar1
            // 
            this.flatProgressBar1.Animated = true;
            this.flatProgressBar1.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(240)))), ((int)(((byte)(240)))), ((int)(((byte)(240)))));
            this.flatProgressBar1.ForeColor = System.Drawing.Color.LightSteelBlue;
            this.flatProgressBar1.Location = new System.Drawing.Point(2, 0);
            this.flatProgressBar1.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.flatProgressBar1.Maximum = ((long)(100));
            this.flatProgressBar1.Size = new System.Drawing.Size(300, 40);
            this.flatProgressBar1.Value = ((long)(0));
            // 
            // label82
            // 
            this.label82.AutoSize = true;
            this.label82.Font = new System.Drawing.Font("Segoe UI Semilight", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label82.Location = new System.Drawing.Point(0, 0);
            this.label82.Margin = new System.Windows.Forms.Padding(0);
            this.label82.Size = new System.Drawing.Size(56, 13);
            this.label82.Text = "Remaining";
            // 
            // labelRemaining
            // 
            this.labelRemaining.AutoSize = true;
            this.labelRemaining.Font = new System.Drawing.Font("Segoe UI Semilight", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.labelRemaining.Location = new System.Drawing.Point(61, 0);
            this.labelRemaining.Margin = new System.Windows.Forms.Padding(5, 0, 0, 0);
            this.labelRemaining.Size = new System.Drawing.Size(13, 13);
            this.labelRemaining.Text = "...";
            // 
            // stackPanel1
            // 
            this.stackPanel1.AutoSize = true;
            this.stackPanel1.Controls.Add(this.flatProgressBar1);
            this.stackPanel1.Controls.Add(this.stackPanel2);
            this.stackPanel1.Location = new System.Drawing.Point(18, 66);
            this.stackPanel1.Margin = new System.Windows.Forms.Padding(18, 66, 18, 20);
            this.stackPanel1.Size = new System.Drawing.Size(304, 58);
            // 
            // stackPanel2
            // 
            this.stackPanel2.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.stackPanel2.AutoSize = true;
            this.stackPanel2.AutoSizeFill = Gw2Launcher.UI.Controls.StackPanel.AutoSizeFillMode.Width;
            this.stackPanel2.Controls.Add(this.label82);
            this.stackPanel2.Controls.Add(this.labelRemaining);
            this.stackPanel2.FlowDirection = System.Windows.Forms.FlowDirection.LeftToRight;
            this.stackPanel2.Location = new System.Drawing.Point(0, 45);
            this.stackPanel2.Margin = new System.Windows.Forms.Padding(0, 5, 0, 0);
            this.stackPanel2.Size = new System.Drawing.Size(304, 13);
            // 
            // formCopyFileDialog
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
            this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(248)))), ((int)(((byte)(248)))), ((int)(((byte)(248)))));
            this.ClientSize = new System.Drawing.Size(340, 144);
            this.Controls.Add(this.stackPanel1);
            this.Icon = global::Gw2Launcher.Properties.Resources.Gw2Launcher;
            this.Text = "Copying...";
            this.stackPanel1.ResumeLayout(false);
            this.stackPanel1.PerformLayout();
            this.stackPanel2.ResumeLayout(false);
            this.stackPanel2.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private Controls.FlatProgressBar flatProgressBar1;
        private System.Windows.Forms.Label label82;
        private System.Windows.Forms.Label labelRemaining;
        private Controls.StackPanel stackPanel1;
        private Controls.StackPanel stackPanel2;

    }
}
