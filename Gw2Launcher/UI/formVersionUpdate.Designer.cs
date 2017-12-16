namespace Gw2Launcher.UI
{
    partial class formVersionUpdate
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
            this.progressUpdating = new System.Windows.Forms.ProgressBar();
            this.labelName = new System.Windows.Forms.Label();
            this.labelStatus = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // progressUpdating
            // 
            this.progressUpdating.Location = new System.Drawing.Point(13, 36);
            this.progressUpdating.Name = "progressUpdating";
            this.progressUpdating.Size = new System.Drawing.Size(342, 31);
            this.progressUpdating.TabIndex = 21;
            // 
            // labelName
            // 
            this.labelName.AutoSize = true;
            this.labelName.Font = new System.Drawing.Font("Segoe UI Semilight", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.labelName.Location = new System.Drawing.Point(10, 10);
            this.labelName.Name = "labelName";
            this.labelName.Size = new System.Drawing.Size(137, 13);
            this.labelName.TabIndex = 22;
            this.labelName.Text = "Downloading Gw2Launcher";
            // 
            // labelStatus
            // 
            this.labelStatus.AutoSize = true;
            this.labelStatus.Font = new System.Drawing.Font("Segoe UI Semilight", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.labelStatus.Location = new System.Drawing.Point(60, 88);
            this.labelStatus.Name = "labelStatus";
            this.labelStatus.Size = new System.Drawing.Size(69, 13);
            this.labelStatus.TabIndex = 30;
            this.labelStatus.Text = "Downloading";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Font = new System.Drawing.Font("Segoe UI Semibold", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label3.Location = new System.Drawing.Point(10, 88);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(38, 13);
            this.label3.TabIndex = 29;
            this.label3.Text = "Status";
            // 
            // formVersionUpdate
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(368, 113);
            this.Controls.Add(this.labelStatus);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.labelName);
            this.Controls.Add(this.progressUpdating);
            this.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.Icon = global::Gw2Launcher.Properties.Resources.Gw2;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "formVersionUpdate";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Load += new System.EventHandler(this.formVersionUpdate_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.ProgressBar progressUpdating;
        private System.Windows.Forms.Label labelName;
        private System.Windows.Forms.Label labelStatus;
        private System.Windows.Forms.Label label3;
    }
}