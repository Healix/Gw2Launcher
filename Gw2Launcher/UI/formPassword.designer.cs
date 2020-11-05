namespace Gw2Launcher.UI
{
    partial class formPassword
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
            System.Security.SecureString secureString1 = new System.Security.SecureString();
            this.label1 = new System.Windows.Forms.Label();
            this.textUsername = new System.Windows.Forms.TextBox();
            this.panelContent = new Gw2Launcher.UI.Controls.StackPanel();
            this.textPassword = new Gw2Launcher.UI.Controls.PasswordBox();
            this.stackPanel2 = new Gw2Launcher.UI.Controls.StackPanel();
            this.buttonCancel = new System.Windows.Forms.Button();
            this.buttonOK = new System.Windows.Forms.Button();
            this.panelContent.SuspendLayout();
            this.stackPanel2.SuspendLayout();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(0, 0);
            this.label1.Margin = new System.Windows.Forms.Padding(0, 0, 0, 10);
            this.label1.Size = new System.Drawing.Size(74, 13);
            this.label1.Text = "Password for";
            // 
            // textUsername
            // 
            this.textUsername.Location = new System.Drawing.Point(5, 23);
            this.textUsername.Margin = new System.Windows.Forms.Padding(5, 0, 5, 0);
            this.textUsername.ReadOnly = true;
            this.textUsername.Size = new System.Drawing.Size(234, 22);
            // 
            // panelContent
            // 
            this.panelContent.AutoSize = true;
            this.panelContent.Controls.Add(this.label1);
            this.panelContent.Controls.Add(this.textUsername);
            this.panelContent.Controls.Add(this.textPassword);
            this.panelContent.Controls.Add(this.stackPanel2);
            this.panelContent.Location = new System.Drawing.Point(15, 60);
            this.panelContent.Margin = new System.Windows.Forms.Padding(15, 60, 15, 15);
            this.panelContent.Size = new System.Drawing.Size(244, 133);
            // 
            // textPassword
            // 
            this.textPassword.Location = new System.Drawing.Point(5, 50);
            this.textPassword.Margin = new System.Windows.Forms.Padding(5, 5, 5, 0);
            this.textPassword.Password = secureString1;
            this.textPassword.PasswordChar = '*';
            this.textPassword.Size = new System.Drawing.Size(234, 22);
            this.textPassword.UseSystemPasswordChar = true;
            this.textPassword.KeyDown += new System.Windows.Forms.KeyEventHandler(this.textPassword_KeyDown);
            // 
            // stackPanel2
            // 
            this.stackPanel2.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.stackPanel2.AutoSize = true;
            this.stackPanel2.Controls.Add(this.buttonCancel);
            this.stackPanel2.Controls.Add(this.buttonOK);
            this.stackPanel2.FlowDirection = System.Windows.Forms.FlowDirection.LeftToRight;
            this.stackPanel2.Location = new System.Drawing.Point(30, 92);
            this.stackPanel2.Margin = new System.Windows.Forms.Padding(0, 20, 0, 0);
            this.stackPanel2.Size = new System.Drawing.Size(184, 41);
            // 
            // buttonCancel
            // 
            this.buttonCancel.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.buttonCancel.Font = new System.Drawing.Font("Segoe UI", 8.25F);
            this.buttonCancel.Location = new System.Drawing.Point(3, 3);
            this.buttonCancel.Size = new System.Drawing.Size(86, 35);
            this.buttonCancel.Text = "Cancel";
            this.buttonCancel.UseVisualStyleBackColor = true;
            this.buttonCancel.Click += new System.EventHandler(this.buttonCancel_Click);
            // 
            // buttonOK
            // 
            this.buttonOK.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.buttonOK.Font = new System.Drawing.Font("Segoe UI", 8.25F);
            this.buttonOK.Location = new System.Drawing.Point(95, 3);
            this.buttonOK.Size = new System.Drawing.Size(86, 35);
            this.buttonOK.Text = "OK";
            this.buttonOK.UseVisualStyleBackColor = true;
            this.buttonOK.Click += new System.EventHandler(this.buttonOK_Click);
            // 
            // formPassword
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
            this.AutoSize = true;
            this.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.ClientSize = new System.Drawing.Size(274, 208);
            this.Controls.Add(this.panelContent);
            this.Text = "Verify password";
            this.panelContent.ResumeLayout(false);
            this.panelContent.PerformLayout();
            this.stackPanel2.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox textUsername;
        private Controls.StackPanel panelContent;
        private Controls.PasswordBox textPassword;
        private Controls.StackPanel stackPanel2;
        private System.Windows.Forms.Button buttonCancel;
        private System.Windows.Forms.Button buttonOK;

    }
}
