namespace Gw2Launcher.UI
{
    partial class formBuildNotify
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
            this.labelTitle = new System.Windows.Forms.Label();
            this.labelBuildCaption = new System.Windows.Forms.Label();
            this.labelBuild = new System.Windows.Forms.Label();
            this.labelSize = new System.Windows.Forms.Label();
            this.labelSizeCaption = new System.Windows.Forms.Label();
            this.labelElapsedCaption = new System.Windows.Forms.Label();
            this.labelElapsed = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // labelTitle
            // 
            this.labelTitle.AutoSize = true;
            this.labelTitle.Font = new System.Drawing.Font("Segoe UI", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.labelTitle.ForeColor = System.Drawing.Color.LightGray;
            this.labelTitle.Location = new System.Drawing.Point(12, 9);
            this.labelTitle.Name = "labelTitle";
            this.labelTitle.Size = new System.Drawing.Size(23, 17);
            this.labelTitle.TabIndex = 0;
            this.labelTitle.Text = "{0}";
            // 
            // labelBuildCaption
            // 
            this.labelBuildCaption.AutoSize = true;
            this.labelBuildCaption.Font = new System.Drawing.Font("Segoe UI Semibold", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.labelBuildCaption.ForeColor = System.Drawing.Color.Gray;
            this.labelBuildCaption.Location = new System.Drawing.Point(13, 31);
            this.labelBuildCaption.Name = "labelBuildCaption";
            this.labelBuildCaption.Size = new System.Drawing.Size(33, 13);
            this.labelBuildCaption.TabIndex = 1;
            this.labelBuildCaption.Text = "Build";
            // 
            // labelBuild
            // 
            this.labelBuild.AutoSize = true;
            this.labelBuild.BackColor = System.Drawing.Color.Black;
            this.labelBuild.Font = new System.Drawing.Font("Segoe UI Semibold", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.labelBuild.ForeColor = System.Drawing.Color.Silver;
            this.labelBuild.Location = new System.Drawing.Point(70, 31);
            this.labelBuild.Name = "labelBuild";
            this.labelBuild.Size = new System.Drawing.Size(54, 13);
            this.labelBuild.TabIndex = 2;
            this.labelBuild.Text = "{0:#,##0}";
            // 
            // labelSize
            // 
            this.labelSize.AutoSize = true;
            this.labelSize.BackColor = System.Drawing.Color.Black;
            this.labelSize.Font = new System.Drawing.Font("Segoe UI Semibold", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.labelSize.ForeColor = System.Drawing.Color.Silver;
            this.labelSize.Location = new System.Drawing.Point(70, 47);
            this.labelSize.Name = "labelSize";
            this.labelSize.Size = new System.Drawing.Size(94, 13);
            this.labelSize.TabIndex = 3;
            this.labelSize.Text = "{1} ({0:#,##0} {2})";
            // 
            // labelSizeCaption
            // 
            this.labelSizeCaption.AutoSize = true;
            this.labelSizeCaption.Font = new System.Drawing.Font("Segoe UI Semibold", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.labelSizeCaption.ForeColor = System.Drawing.Color.Gray;
            this.labelSizeCaption.Location = new System.Drawing.Point(13, 47);
            this.labelSizeCaption.Name = "labelSizeCaption";
            this.labelSizeCaption.Size = new System.Drawing.Size(27, 13);
            this.labelSizeCaption.TabIndex = 4;
            this.labelSizeCaption.Text = "Size";
            // 
            // labelElapsedCaption
            // 
            this.labelElapsedCaption.AutoSize = true;
            this.labelElapsedCaption.Font = new System.Drawing.Font("Segoe UI Semibold", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.labelElapsedCaption.ForeColor = System.Drawing.Color.Gray;
            this.labelElapsedCaption.Location = new System.Drawing.Point(13, 63);
            this.labelElapsedCaption.Name = "labelElapsedCaption";
            this.labelElapsedCaption.Size = new System.Drawing.Size(47, 13);
            this.labelElapsedCaption.TabIndex = 5;
            this.labelElapsedCaption.Text = "Elapsed";
            // 
            // labelElapsed
            // 
            this.labelElapsed.AutoSize = true;
            this.labelElapsed.BackColor = System.Drawing.Color.Black;
            this.labelElapsed.Font = new System.Drawing.Font("Segoe UI Semibold", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.labelElapsed.ForeColor = System.Drawing.Color.Silver;
            this.labelElapsed.Location = new System.Drawing.Point(70, 63);
            this.labelElapsed.Name = "labelElapsed";
            this.labelElapsed.Size = new System.Drawing.Size(21, 13);
            this.labelElapsed.TabIndex = 6;
            this.labelElapsed.Text = "{0}";
            // 
            // formBuildNotify
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.Black;
            this.ClientSize = new System.Drawing.Size(219, 87);
            this.Controls.Add(this.labelElapsed);
            this.Controls.Add(this.labelElapsedCaption);
            this.Controls.Add(this.labelSizeCaption);
            this.Controls.Add(this.labelSize);
            this.Controls.Add(this.labelBuild);
            this.Controls.Add(this.labelBuildCaption);
            this.Controls.Add(this.labelTitle);
            this.Font = new System.Drawing.Font("Segoe UI", 8.25F);
            this.ForeColor = System.Drawing.Color.White;
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.Icon = global::Gw2Launcher.Properties.Resources.Gw2;
            this.Name = "formBuildNotify";
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.Manual;
            this.Load += new System.EventHandler(this.formBuildNotify_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label labelTitle;
        private System.Windows.Forms.Label labelBuildCaption;
        private System.Windows.Forms.Label labelBuild;
        private System.Windows.Forms.Label labelSize;
        private System.Windows.Forms.Label labelSizeCaption;
        private System.Windows.Forms.Label labelElapsedCaption;
        private System.Windows.Forms.Label labelElapsed;
    }
}