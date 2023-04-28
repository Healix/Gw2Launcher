namespace Gw2Launcher.UI.QuickStart
{
    partial class formAccount
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
            this.stackPanel2 = new Gw2Launcher.UI.Controls.StackPanel();
            this.buttonCancel = new System.Windows.Forms.Button();
            this.buttonOk = new System.Windows.Forms.Button();
            this.stackPanel1 = new Gw2Launcher.UI.Controls.StackPanel();
            this.label1 = new System.Windows.Forms.Label();
            this.label5 = new System.Windows.Forms.Label();
            this.textAccountName = new System.Windows.Forms.TextBox();
            this.label4 = new System.Windows.Forms.Label();
            this.checkLoginAuto = new System.Windows.Forms.CheckBox();
            this.tableLogin = new Gw2Launcher.UI.Controls.TableContainerPanel();
            this.label68 = new System.Windows.Forms.Label();
            this.label69 = new System.Windows.Forms.Label();
            this.textEmail = new Gw2Launcher.UI.Controls.BoxedLabelTextBox();
            this.textPassword = new Gw2Launcher.UI.Controls.BoxedLabelPasswordBox();
            this.stackPanel2.SuspendLayout();
            this.stackPanel1.SuspendLayout();
            this.tableLogin.SuspendLayout();
            this.SuspendLayout();
            // 
            // stackPanel2
            // 
            this.stackPanel2.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.stackPanel2.AutoSize = true;
            this.stackPanel2.Controls.Add(this.buttonCancel);
            this.stackPanel2.Controls.Add(this.buttonOk);
            this.stackPanel2.FlowDirection = System.Windows.Forms.FlowDirection.LeftToRight;
            this.stackPanel2.Location = new System.Drawing.Point(40, 192);
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
            // buttonOk
            // 
            this.buttonOk.Font = new System.Drawing.Font("Segoe UI", 8.25F);
            this.buttonOk.Location = new System.Drawing.Point(95, 3);
            this.buttonOk.Size = new System.Drawing.Size(86, 35);
            this.buttonOk.Text = "OK";
            this.buttonOk.UseVisualStyleBackColor = true;
            this.buttonOk.Click += new System.EventHandler(this.buttonOk_Click);
            // 
            // stackPanel1
            // 
            this.stackPanel1.AutoSize = true;
            this.stackPanel1.Controls.Add(this.label1);
            this.stackPanel1.Controls.Add(this.label5);
            this.stackPanel1.Controls.Add(this.textAccountName);
            this.stackPanel1.Controls.Add(this.label4);
            this.stackPanel1.Controls.Add(this.checkLoginAuto);
            this.stackPanel1.Controls.Add(this.tableLogin);
            this.stackPanel1.Controls.Add(this.stackPanel2);
            this.stackPanel1.Location = new System.Drawing.Point(15, 60);
            this.stackPanel1.Margin = new System.Windows.Forms.Padding(15, 60, 15, 15);
            this.stackPanel1.Size = new System.Drawing.Size(265, 233);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("Segoe UI Semibold", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label1.Location = new System.Drawing.Point(0, 0);
            this.label1.Margin = new System.Windows.Forms.Padding(0, 0, 0, 1);
            this.label1.Size = new System.Drawing.Size(39, 15);
            this.label1.Text = "Name";
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Font = new System.Drawing.Font("Segoe UI Semilight", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label5.Location = new System.Drawing.Point(1, 17);
            this.label5.Margin = new System.Windows.Forms.Padding(1, 1, 0, 8);
            this.label5.Size = new System.Drawing.Size(112, 13);
            this.label5.Text = "To identify the account";
            // 
            // textAccountName
            // 
            this.textAccountName.Font = new System.Drawing.Font("Segoe UI", 8.25F);
            this.textAccountName.Location = new System.Drawing.Point(8, 38);
            this.textAccountName.Margin = new System.Windows.Forms.Padding(8, 0, 0, 0);
            this.textAccountName.Size = new System.Drawing.Size(253, 22);
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Font = new System.Drawing.Font("Segoe UI Semibold", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label4.Location = new System.Drawing.Point(0, 73);
            this.label4.Margin = new System.Windows.Forms.Padding(0, 13, 0, 8);
            this.label4.Size = new System.Drawing.Size(37, 15);
            this.label4.Text = "Login";
            // 
            // checkLoginAuto
            // 
            this.checkLoginAuto.AutoSize = true;
            this.checkLoginAuto.Location = new System.Drawing.Point(8, 96);
            this.checkLoginAuto.Margin = new System.Windows.Forms.Padding(8, 0, 0, 0);
            this.checkLoginAuto.Size = new System.Drawing.Size(125, 17);
            this.checkLoginAuto.Text = "Automatically login";
            this.checkLoginAuto.UseVisualStyleBackColor = true;
            this.checkLoginAuto.CheckedChanged += new System.EventHandler(this.checkLoginAuto_CheckedChanged);
            // 
            // tableLogin
            // 
            this.tableLogin.AutoSize = true;
            this.tableLogin.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.tableLogin.ColumnCount = 2;
            this.tableLogin.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tableLogin.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tableLogin.Controls.Add(this.label68, 0, 0);
            this.tableLogin.Controls.Add(this.label69, 0, 2);
            this.tableLogin.Controls.Add(this.textEmail, 1, 0);
            this.tableLogin.Controls.Add(this.textPassword, 1, 2);
            this.tableLogin.Enabled = false;
            this.tableLogin.Location = new System.Drawing.Point(8, 122);
            this.tableLogin.Margin = new System.Windows.Forms.Padding(8, 9, 0, 0);
            this.tableLogin.RowCount = 3;
            this.tableLogin.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLogin.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 6F));
            this.tableLogin.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLogin.Size = new System.Drawing.Size(257, 50);
            // 
            // label68
            // 
            this.label68.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.label68.AutoSize = true;
            this.label68.Font = new System.Drawing.Font("Segoe UI Semilight", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label68.Location = new System.Drawing.Point(0, 4);
            this.label68.Margin = new System.Windows.Forms.Padding(0);
            this.label68.Size = new System.Drawing.Size(32, 13);
            this.label68.Text = "Email";
            // 
            // label69
            // 
            this.label69.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.label69.AutoSize = true;
            this.label69.Font = new System.Drawing.Font("Segoe UI Semilight", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label69.Location = new System.Drawing.Point(0, 32);
            this.label69.Margin = new System.Windows.Forms.Padding(0);
            this.label69.Size = new System.Drawing.Size(51, 13);
            this.label69.Text = "Password";
            // 
            // textEmail
            // 
            this.textEmail.BackColor = System.Drawing.SystemColors.Window;
            this.textEmail.Cursor = Windows.Cursors.Hand;
            this.textEmail.Font = new System.Drawing.Font("Segoe UI Semilight", 8.25F);
            this.textEmail.FontText = new System.Drawing.Font("Segoe UI", 8.25F);
            this.textEmail.ForeColor = System.Drawing.SystemColors.GrayText;
            this.textEmail.ForeColorText = System.Drawing.SystemColors.WindowText;
            this.textEmail.Location = new System.Drawing.Point(62, 0);
            this.textEmail.Margin = new System.Windows.Forms.Padding(11, 0, 0, 0);
            this.textEmail.Padding = new System.Windows.Forms.Padding(5, 0, 0, 1);
            this.textEmail.Size = new System.Drawing.Size(195, 22);
            this.textEmail.Text = "change email";
            this.textEmail.TextVisible = true;
            // 
            // textPassword
            // 
            this.textPassword.BackColor = System.Drawing.SystemColors.Window;
            this.textPassword.Cursor = Windows.Cursors.Hand;
            this.textPassword.Font = new System.Drawing.Font("Segoe UI Semilight", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.textPassword.FontText = new System.Drawing.Font("Segoe UI", 8.25F);
            this.textPassword.ForeColor = System.Drawing.SystemColors.GrayText;
            this.textPassword.ForeColorText = System.Drawing.SystemColors.WindowText;
            this.textPassword.Location = new System.Drawing.Point(62, 28);
            this.textPassword.Margin = new System.Windows.Forms.Padding(11, 0, 0, 0);
            this.textPassword.Padding = new System.Windows.Forms.Padding(5, 0, 0, 1);
            this.textPassword.Size = new System.Drawing.Size(195, 22);
            this.textPassword.Text = "change password";
            this.textPassword.TextVisible = true;
            // 
            // formAccount
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
            this.AutoSize = true;
            this.ClientSize = new System.Drawing.Size(295, 308);
            this.Controls.Add(this.stackPanel1);
            this.Text = "Account";
            this.stackPanel2.ResumeLayout(false);
            this.stackPanel1.ResumeLayout(false);
            this.stackPanel1.PerformLayout();
            this.tableLogin.ResumeLayout(false);
            this.tableLogin.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private Gw2Launcher.UI.Controls.StackPanel stackPanel2;
        private System.Windows.Forms.Button buttonCancel;
        private System.Windows.Forms.Button buttonOk;
        private Gw2Launcher.UI.Controls.StackPanel stackPanel1;
        private System.Windows.Forms.Label label4;
        private UI.Controls.TableContainerPanel tableLogin;
        private System.Windows.Forms.Label label68;
        private System.Windows.Forms.Label label69;
        private UI.Controls.BoxedLabelTextBox textEmail;
        private UI.Controls.BoxedLabelPasswordBox textPassword;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.TextBox textAccountName;
        private System.Windows.Forms.CheckBox checkLoginAuto;
    }
}
