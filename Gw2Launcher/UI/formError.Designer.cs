namespace Gw2Launcher.UI
{
    partial class formError
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
            this.textError = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.labelOpenLog = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // textError
            // 
            this.textError.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.textError.Location = new System.Drawing.Point(12, 42);
            this.textError.Multiline = true;
            this.textError.Name = "textError";
            this.textError.ReadOnly = true;
            this.textError.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.textError.Size = new System.Drawing.Size(563, 221);
            this.textError.TabIndex = 0;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(12, 14);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(335, 13);
            this.label1.TabIndex = 1;
            this.label1.Text = "The following error is preventing this program from continuing.";
            // 
            // labelOpenLog
            // 
            this.labelOpenLog.AutoSize = true;
            this.labelOpenLog.Cursor = System.Windows.Forms.Cursors.Hand;
            this.labelOpenLog.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(49)))), ((int)(((byte)(121)))), ((int)(((byte)(242)))));
            this.labelOpenLog.Location = new System.Drawing.Point(353, 14);
            this.labelOpenLog.Name = "labelOpenLog";
            this.labelOpenLog.Size = new System.Drawing.Size(89, 13);
            this.labelOpenLog.TabIndex = 24;
            this.labelOpenLog.Text = "show log folder";
            this.labelOpenLog.Click += new System.EventHandler(this.labelOpenLog_Click);
            // 
            // formError
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(587, 275);
            this.Controls.Add(this.labelOpenLog);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.textError);
            this.Font = new System.Drawing.Font("Segoe UI", 8.25F);
            this.Icon = global::Gw2Launcher.Properties.Resources.Gw2;
            this.MinimizeBox = false;
            this.MinimumSize = new System.Drawing.Size(500, 190);
            this.Name = "formError";
            this.ShowIcon = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "An error has occured";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TextBox textError;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label labelOpenLog;
    }
}