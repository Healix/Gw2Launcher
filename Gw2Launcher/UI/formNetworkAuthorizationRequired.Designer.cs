namespace Gw2Launcher.UI
{
    partial class formNetworkAuthorizationRequired
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
            this.textCode = new System.Windows.Forms.TextBox();
            this.labelTitle = new System.Windows.Forms.Label();
            this.buttonOK = new System.Windows.Forms.Button();
            this.labelEmail = new System.Windows.Forms.Label();
            this.waiting = new Gw2Launcher.UI.Controls.WaitingBounce();
            this.labelError = new System.Windows.Forms.Label();
            this.buttonRetry = new System.Windows.Forms.Button();
            this.stackPanel1 = new Gw2Launcher.UI.Controls.StackPanel();
            this.panel1 = new System.Windows.Forms.Panel();
            this.stackPanel2 = new Gw2Launcher.UI.Controls.StackPanel();
            this.buttonCancel = new System.Windows.Forms.Button();
            this.panel2 = new System.Windows.Forms.Panel();
            this.stackPanel1.SuspendLayout();
            this.panel1.SuspendLayout();
            this.stackPanel2.SuspendLayout();
            this.panel2.SuspendLayout();
            this.SuspendLayout();
            // 
            // textCode
            // 
            this.textCode.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.textCode.Location = new System.Drawing.Point(35, 0);
            this.textCode.Margin = new System.Windows.Forms.Padding(0);
            this.textCode.Size = new System.Drawing.Size(161, 29);
            this.textCode.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            this.textCode.KeyDown += new System.Windows.Forms.KeyEventHandler(this.textCode_KeyDown);
            // 
            // labelTitle
            // 
            this.labelTitle.AutoSize = true;
            this.labelTitle.Font = new System.Drawing.Font("Segoe UI Semibold", 9F, System.Drawing.FontStyle.Bold);
            this.labelTitle.Location = new System.Drawing.Point(0, 0);
            this.labelTitle.Margin = new System.Windows.Forms.Padding(0, 0, 0, 1);
            this.labelTitle.Size = new System.Drawing.Size(170, 15);
            this.labelTitle.Text = "Enter your authentication code";
            // 
            // buttonOK
            // 
            this.buttonOK.Font = new System.Drawing.Font("Segoe UI", 8.25F);
            this.buttonOK.Location = new System.Drawing.Point(0, 0);
            this.buttonOK.Margin = new System.Windows.Forms.Padding(0);
            this.buttonOK.Size = new System.Drawing.Size(86, 35);
            this.buttonOK.Text = "OK";
            this.buttonOK.UseVisualStyleBackColor = true;
            this.buttonOK.Click += new System.EventHandler(this.buttonOK_Click);
            // 
            // labelEmail
            // 
            this.labelEmail.AutoSize = true;
            this.labelEmail.Font = new System.Drawing.Font("Segoe UI Semilight", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.labelEmail.Location = new System.Drawing.Point(0, 17);
            this.labelEmail.Margin = new System.Windows.Forms.Padding(0, 1, 0, 8);
            this.labelEmail.Size = new System.Drawing.Size(35, 15);
            this.labelEmail.Text = "email";
            // 
            // waiting
            // 
            this.waiting.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.waiting.ForeColor = System.Drawing.SystemColors.GrayText;
            this.waiting.Location = new System.Drawing.Point(59, 7);
            this.waiting.Margin = new System.Windows.Forms.Padding(0);
            this.waiting.Size = new System.Drawing.Size(112, 15);
            this.waiting.Visible = false;
            // 
            // labelError
            // 
            this.labelError.AutoSize = true;
            this.labelError.Font = new System.Drawing.Font("Segoe UI Semilight", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.labelError.ForeColor = System.Drawing.Color.Maroon;
            this.labelError.Location = new System.Drawing.Point(0, 7);
            this.labelError.Margin = new System.Windows.Forms.Padding(0);
            this.labelError.Size = new System.Drawing.Size(12, 15);
            this.labelError.Text = "-";
            this.labelError.Visible = false;
            // 
            // buttonRetry
            // 
            this.buttonRetry.Font = new System.Drawing.Font("Segoe UI", 8.25F);
            this.buttonRetry.Location = new System.Drawing.Point(0, 0);
            this.buttonRetry.Margin = new System.Windows.Forms.Padding(0);
            this.buttonRetry.Size = new System.Drawing.Size(86, 35);
            this.buttonRetry.Text = "Retry";
            this.buttonRetry.UseVisualStyleBackColor = true;
            this.buttonRetry.Visible = false;
            this.buttonRetry.Click += new System.EventHandler(this.buttonRetry_Click);
            // 
            // stackPanel1
            // 
            this.stackPanel1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.stackPanel1.AutoSize = true;
            this.stackPanel1.Controls.Add(this.labelTitle);
            this.stackPanel1.Controls.Add(this.labelEmail);
            this.stackPanel1.Controls.Add(this.panel1);
            this.stackPanel1.Controls.Add(this.stackPanel2);
            this.stackPanel1.Location = new System.Drawing.Point(15, 60);
            this.stackPanel1.Margin = new System.Windows.Forms.Padding(15, 60, 15, 15);
            this.stackPanel1.MinimumSize = new System.Drawing.Size(237, 0);
            this.stackPanel1.Size = new System.Drawing.Size(237, 131);
            // 
            // panel1
            // 
            this.panel1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.panel1.AutoSize = true;
            this.panel1.Controls.Add(this.textCode);
            this.panel1.Controls.Add(this.waiting);
            this.panel1.Controls.Add(this.labelError);
            this.panel1.Location = new System.Drawing.Point(3, 43);
            this.panel1.Size = new System.Drawing.Size(231, 29);
            // 
            // stackPanel2
            // 
            this.stackPanel2.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.stackPanel2.AutoSize = true;
            this.stackPanel2.Controls.Add(this.buttonCancel);
            this.stackPanel2.Controls.Add(this.panel2);
            this.stackPanel2.FlowDirection = System.Windows.Forms.FlowDirection.LeftToRight;
            this.stackPanel2.Location = new System.Drawing.Point(26, 90);
            this.stackPanel2.Margin = new System.Windows.Forms.Padding(0, 15, 0, 0);
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
            // panel2
            // 
            this.panel2.AutoSize = true;
            this.panel2.Controls.Add(this.buttonOK);
            this.panel2.Controls.Add(this.buttonRetry);
            this.panel2.Location = new System.Drawing.Point(95, 3);
            this.panel2.Size = new System.Drawing.Size(86, 35);
            // 
            // formNetworkAuthorizationRequired
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
            this.AutoSize = true;
            this.AutoSizeFill = Gw2Launcher.UI.Base.StackFormBase.AutoSizeFillMode.Height;
            this.ClientSize = new System.Drawing.Size(267, 206);
            this.Controls.Add(this.stackPanel1);
            this.Text = "Authentication";
            this.stackPanel1.ResumeLayout(false);
            this.stackPanel1.PerformLayout();
            this.panel1.ResumeLayout(false);
            this.panel1.PerformLayout();
            this.stackPanel2.ResumeLayout(false);
            this.stackPanel2.PerformLayout();
            this.panel2.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TextBox textCode;
        private System.Windows.Forms.Label labelTitle;
        private System.Windows.Forms.Button buttonOK;
        private System.Windows.Forms.Label labelEmail;
        private Controls.WaitingBounce waiting;
        private System.Windows.Forms.Label labelError;
        private System.Windows.Forms.Button buttonRetry;
        private Controls.StackPanel stackPanel1;
        private Controls.StackPanel stackPanel2;
        private System.Windows.Forms.Button buttonCancel;
        private System.Windows.Forms.Panel panel2;
        private System.Windows.Forms.Panel panel1;
    }
}
