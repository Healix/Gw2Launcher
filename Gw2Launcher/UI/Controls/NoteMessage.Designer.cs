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
            this.labelEdit = new Controls.LinkLabel();
            this.labelDelete = new Controls.LinkLabel();
            this.labelExpiresValue = new System.Windows.Forms.Label();
            this.labelMessage = new System.Windows.Forms.Label();
            this.stackPanel1 = new Gw2Launcher.UI.Controls.StackPanel();
            this.stackPanel2 = new Gw2Launcher.UI.Controls.StackPanel();
            this.stackPanel3 = new Gw2Launcher.UI.Controls.StackPanel();
            this.stackPanel1.SuspendLayout();
            this.stackPanel2.SuspendLayout();
            this.stackPanel3.SuspendLayout();
            this.SuspendLayout();
            // 
            // labelSep
            // 
            this.labelSep.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.labelSep.AutoSize = true;
            this.labelSep.Font = new System.Drawing.Font("Segoe UI Semilight", 8.25F);
            this.labelSep.Location = new System.Drawing.Point(27, 0);
            this.labelSep.Margin = new System.Windows.Forms.Padding(0);
            this.labelSep.Size = new System.Drawing.Size(11, 13);
            this.labelSep.Text = "/";
            // 
            // labelEdit
            // 
            this.labelEdit.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.labelEdit.AutoSize = true;
            this.labelEdit.Location = new System.Drawing.Point(0, 0);
            this.labelEdit.Margin = new System.Windows.Forms.Padding(0);
            this.labelEdit.Size = new System.Drawing.Size(27, 13);
            this.labelEdit.Text = "edit";
            this.labelEdit.Click += new System.EventHandler(this.labelEdit_Click);
            // 
            // labelDelete
            // 
            this.labelDelete.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.labelDelete.AutoSize = true;
            this.labelDelete.Location = new System.Drawing.Point(38, 0);
            this.labelDelete.Margin = new System.Windows.Forms.Padding(0);
            this.labelDelete.Size = new System.Drawing.Size(39, 13);
            this.labelDelete.Text = "delete";
            this.labelDelete.Click += new System.EventHandler(this.labelDelete_Click);
            // 
            // labelExpiresValue
            // 
            this.labelExpiresValue.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.labelExpiresValue.AutoSize = true;
            this.labelExpiresValue.Font = new System.Drawing.Font("Segoe UI Semilight", 8.25F);
            this.labelExpiresValue.ForeColor = System.Drawing.SystemColors.GrayText;
            this.labelExpiresValue.Location = new System.Drawing.Point(0, 0);
            this.labelExpiresValue.Margin = new System.Windows.Forms.Padding(0);
            this.labelExpiresValue.Size = new System.Drawing.Size(279, 13);
            this.labelExpiresValue.Text = "---";
            // 
            // labelMessage
            // 
            this.labelMessage.AutoSize = true;
            this.labelMessage.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.labelMessage.Location = new System.Drawing.Point(5, 15);
            this.labelMessage.Margin = new System.Windows.Forms.Padding(5, 15, 5, 10);
            this.labelMessage.MaximumSize = new System.Drawing.Size(356, 0);
            this.labelMessage.Size = new System.Drawing.Size(0, 15);
            // 
            // stackPanel1
            // 
            this.stackPanel1.AutoSize = true;
            this.stackPanel1.Controls.Add(this.labelMessage);
            this.stackPanel1.Controls.Add(this.stackPanel2);
            this.stackPanel1.Location = new System.Drawing.Point(0, 0);
            this.stackPanel1.Margin = new System.Windows.Forms.Padding(0);
            this.stackPanel1.MinimumSize = new System.Drawing.Size(366, 0);
            this.stackPanel1.Size = new System.Drawing.Size(366, 68);
            // 
            // stackPanel2
            // 
            this.stackPanel2.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.stackPanel2.AutoSize = true;
            this.stackPanel2.AutoSizeFill = Gw2Launcher.UI.Controls.StackPanel.AutoSizeFillMode.Width;
            this.stackPanel2.Controls.Add(this.labelExpiresValue);
            this.stackPanel2.Controls.Add(this.stackPanel3);
            this.stackPanel2.FlowDirection = System.Windows.Forms.FlowDirection.LeftToRight;
            this.stackPanel2.Location = new System.Drawing.Point(5, 40);
            this.stackPanel2.Margin = new System.Windows.Forms.Padding(5, 0, 5, 15);
            this.stackPanel2.Size = new System.Drawing.Size(356, 13);
            // 
            // stackPanel3
            // 
            this.stackPanel3.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.stackPanel3.AutoSize = true;
            this.stackPanel3.AutoSizeFill = Gw2Launcher.UI.Controls.StackPanel.AutoSizeFillMode.NoWrap;
            this.stackPanel3.Controls.Add(this.labelEdit);
            this.stackPanel3.Controls.Add(this.labelSep);
            this.stackPanel3.Controls.Add(this.labelDelete);
            this.stackPanel3.FlowDirection = System.Windows.Forms.FlowDirection.LeftToRight;
            this.stackPanel3.Location = new System.Drawing.Point(279, 0);
            this.stackPanel3.Margin = new System.Windows.Forms.Padding(0);
            this.stackPanel3.Size = new System.Drawing.Size(77, 13);
            // 
            // NoteMessage
            // 
            this.BackColor = System.Drawing.Color.White;
            this.Controls.Add(this.stackPanel1);
            this.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.Padding = new System.Windows.Forms.Padding(5, 15, 5, 15);
            this.Size = new System.Drawing.Size(366, 68);
            this.stackPanel1.ResumeLayout(false);
            this.stackPanel1.PerformLayout();
            this.stackPanel2.ResumeLayout(false);
            this.stackPanel2.PerformLayout();
            this.stackPanel3.ResumeLayout(false);
            this.stackPanel3.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label labelSep;
        private Controls.LinkLabel labelEdit;
        private Controls.LinkLabel labelDelete;
        private System.Windows.Forms.Label labelExpiresValue;
        private System.Windows.Forms.Label labelMessage;
        private StackPanel stackPanel1;
        private StackPanel stackPanel2;
        private StackPanel stackPanel3;
    }
}
