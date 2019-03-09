namespace Gw2Launcher.UI.Controls
{
    partial class NoteMessage
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

        #region Component Designer generated code

        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.labelSep = new System.Windows.Forms.Label();
            this.labelEdit = new System.Windows.Forms.Label();
            this.labelDelete = new System.Windows.Forms.Label();
            this.labelExpiresValue = new System.Windows.Forms.Label();
            this.panelBottom = new System.Windows.Forms.Panel();
            this.labelMessage = new System.Windows.Forms.Label();
            this.panelBottom.SuspendLayout();
            this.SuspendLayout();
            // 
            // labelSep
            // 
            this.labelSep.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.labelSep.AutoSize = true;
            this.labelSep.Font = new System.Drawing.Font("Segoe UI Semilight", 8.25F);
            this.labelSep.Location = new System.Drawing.Point(305, 10);
            this.labelSep.Name = "labelSep";
            this.labelSep.Size = new System.Drawing.Size(11, 13);
            this.labelSep.TabIndex = 117;
            this.labelSep.Text = "/";
            // 
            // labelEdit
            // 
            this.labelEdit.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.labelEdit.AutoSize = true;
            this.labelEdit.Cursor = System.Windows.Forms.Cursors.Hand;
            this.labelEdit.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(49)))), ((int)(((byte)(121)))), ((int)(((byte)(242)))));
            this.labelEdit.Location = new System.Drawing.Point(276, 10);
            this.labelEdit.Name = "labelEdit";
            this.labelEdit.Size = new System.Drawing.Size(27, 13);
            this.labelEdit.TabIndex = 116;
            this.labelEdit.Text = "edit";
            this.labelEdit.Click += new System.EventHandler(this.labelEdit_Click);
            // 
            // labelDelete
            // 
            this.labelDelete.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.labelDelete.AutoSize = true;
            this.labelDelete.Cursor = System.Windows.Forms.Cursors.Hand;
            this.labelDelete.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(49)))), ((int)(((byte)(121)))), ((int)(((byte)(242)))));
            this.labelDelete.Location = new System.Drawing.Point(317, 10);
            this.labelDelete.Name = "labelDelete";
            this.labelDelete.Size = new System.Drawing.Size(39, 13);
            this.labelDelete.TabIndex = 115;
            this.labelDelete.Text = "delete";
            this.labelDelete.Click += new System.EventHandler(this.labelDelete_Click);
            // 
            // labelExpiresValue
            // 
            this.labelExpiresValue.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.labelExpiresValue.AutoSize = true;
            this.labelExpiresValue.Font = new System.Drawing.Font("Segoe UI Semilight", 8.25F);
            this.labelExpiresValue.ForeColor = System.Drawing.SystemColors.GrayText;
            this.labelExpiresValue.Location = new System.Drawing.Point(0, 10);
            this.labelExpiresValue.Name = "labelExpiresValue";
            this.labelExpiresValue.Size = new System.Drawing.Size(19, 13);
            this.labelExpiresValue.TabIndex = 113;
            this.labelExpiresValue.Text = "---";
            // 
            // panelBottom
            // 
            this.panelBottom.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.panelBottom.Controls.Add(this.labelSep);
            this.panelBottom.Controls.Add(this.labelExpiresValue);
            this.panelBottom.Controls.Add(this.labelEdit);
            this.panelBottom.Controls.Add(this.labelDelete);
            this.panelBottom.Location = new System.Drawing.Point(5, 115);
            this.panelBottom.Name = "panelBottom";
            this.panelBottom.Size = new System.Drawing.Size(356, 23);
            this.panelBottom.TabIndex = 118;
            // 
            // labelMessage
            // 
            this.labelMessage.AutoSize = true;
            this.labelMessage.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.labelMessage.Location = new System.Drawing.Point(5, 15);
            this.labelMessage.MaximumSize = new System.Drawing.Size(356, 0);
            this.labelMessage.Name = "labelMessage";
            this.labelMessage.Size = new System.Drawing.Size(0, 15);
            this.labelMessage.TabIndex = 119;
            // 
            // NoteMessage
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            this.BackColor = System.Drawing.Color.White;
            this.Controls.Add(this.labelMessage);
            this.Controls.Add(this.panelBottom);
            this.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.Name = "NoteMessage";
            this.Padding = new System.Windows.Forms.Padding(5, 15, 5, 15);
            this.Size = new System.Drawing.Size(366, 153);
            this.panelBottom.ResumeLayout(false);
            this.panelBottom.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label labelSep;
        private System.Windows.Forms.Label labelEdit;
        private System.Windows.Forms.Label labelDelete;
        private System.Windows.Forms.Label labelExpiresValue;
        private System.Windows.Forms.Panel panelBottom;
        private System.Windows.Forms.Label labelMessage;
    }
}
