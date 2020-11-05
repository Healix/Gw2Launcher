namespace Gw2Launcher.UI
{
    partial class formUpdating
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
            this.labelName = new System.Windows.Forms.Label();
            this.progressUpdating = new System.Windows.Forms.ProgressBar();
            this.label1 = new System.Windows.Forms.Label();
            this.labelAbort = new Controls.LinkLabel();
            this.labelWritten = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.labelTotalSize = new System.Windows.Forms.Label();
            this.panelWarning = new System.Windows.Forms.Panel();
            this.label5 = new System.Windows.Forms.Label();
            this.label6 = new System.Windows.Forms.Label();
            this.panelWarning.SuspendLayout();
            this.SuspendLayout();
            // 
            // labelName
            // 
            this.labelName.AutoSize = true;
            this.labelName.Font = new System.Drawing.Font("Segoe UI Semilight", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.labelName.Location = new System.Drawing.Point(10, 10);
            this.labelName.Size = new System.Drawing.Size(125, 13);
            this.labelName.Text = "Updating [account name]";
            // 
            // progressUpdating
            // 
            this.progressUpdating.Location = new System.Drawing.Point(13, 36);
            this.progressUpdating.Size = new System.Drawing.Size(342, 31);
            // 
            // label1
            // 
            this.label1.Font = new System.Drawing.Font("Segoe UI Semilight", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label1.Location = new System.Drawing.Point(10, 151);
            this.label1.Size = new System.Drawing.Size(304, 13);
            this.label1.Text = "Cancel all pending launches and close any active processes";
            this.label1.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // labelAbort
            // 
            this.labelAbort.AutoSize = true;
            this.labelAbort.Location = new System.Drawing.Point(320, 151);
            this.labelAbort.Size = new System.Drawing.Size(35, 13);
            this.labelAbort.Text = "abort";
            this.labelAbort.Click += new System.EventHandler(this.labelAbort_Click);
            // 
            // labelWritten
            // 
            this.labelWritten.AutoSize = true;
            this.labelWritten.Font = new System.Drawing.Font("Segoe UI Semilight", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.labelWritten.Location = new System.Drawing.Point(80, 123);
            this.labelWritten.Size = new System.Drawing.Size(32, 13);
            this.labelWritten.Text = "0 MB";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Font = new System.Drawing.Font("Segoe UI Semibold", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label2.Location = new System.Drawing.Point(10, 87);
            this.label2.Size = new System.Drawing.Size(112, 13);
            this.label2.Text = "Gw2.dat information";
            this.label2.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Font = new System.Drawing.Font("Segoe UI Semilight", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label3.Location = new System.Drawing.Point(10, 105);
            this.label3.Size = new System.Drawing.Size(51, 13);
            this.label3.Text = "Total Size";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Font = new System.Drawing.Font("Segoe UI Semilight", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label4.Location = new System.Drawing.Point(10, 123);
            this.label4.Size = new System.Drawing.Size(57, 13);
            this.label4.Text = "Disk usage";
            // 
            // labelTotalSize
            // 
            this.labelTotalSize.AutoSize = true;
            this.labelTotalSize.Font = new System.Drawing.Font("Segoe UI Semilight", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.labelTotalSize.Location = new System.Drawing.Point(80, 105);
            this.labelTotalSize.Size = new System.Drawing.Size(32, 13);
            this.labelTotalSize.Text = "0 MB";
            // 
            // panelWarning
            // 
            this.panelWarning.Controls.Add(this.label5);
            this.panelWarning.Controls.Add(this.label6);
            this.panelWarning.Location = new System.Drawing.Point(13, 73);
            this.panelWarning.Size = new System.Drawing.Size(342, 75);
            this.panelWarning.Visible = false;
            // 
            // label5
            // 
            this.label5.Font = new System.Drawing.Font("Segoe UI Semilight", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label5.Location = new System.Drawing.Point(0, 32);
            this.label5.Size = new System.Drawing.Size(320, 31);
            this.label5.Text = "There have been no changes for over a minute. You may want to abort and launch th" +
    "e client normally instead.";
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Font = new System.Drawing.Font("Segoe UI Semibold", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label6.Location = new System.Drawing.Point(0, 14);
            this.label6.Size = new System.Drawing.Size(50, 13);
            this.label6.Text = "Warning";
            this.label6.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // formUpdating
            // 
            this.ClientSize = new System.Drawing.Size(368, 172);
            this.Controls.Add(this.labelTotalSize);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.labelWritten);
            this.Controls.Add(this.labelAbort);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.progressUpdating);
            this.Controls.Add(this.labelName);
            this.Controls.Add(this.panelWarning);
            this.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.Icon = global::Gw2Launcher.Properties.Resources.Gw2Launcher;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.formUpdating_FormClosing);
            this.Load += new System.EventHandler(this.formUpdating_Load);
            this.panelWarning.ResumeLayout(false);
            this.panelWarning.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label labelName;
        private System.Windows.Forms.ProgressBar progressUpdating;
        private System.Windows.Forms.Label label1;
        private Controls.LinkLabel labelAbort;
        private System.Windows.Forms.Label labelWritten;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Label labelTotalSize;
        private System.Windows.Forms.Panel panelWarning;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.Label label6;
    }
}
