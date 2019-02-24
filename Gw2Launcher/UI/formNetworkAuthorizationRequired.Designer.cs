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
            this.SuspendLayout();
            // 
            // textCode
            // 
            this.textCode.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.textCode.Location = new System.Drawing.Point(15, 49);
            this.textCode.Name = "textCode";
            this.textCode.Size = new System.Drawing.Size(168, 29);
            this.textCode.TabIndex = 0;
            this.textCode.KeyDown += new System.Windows.Forms.KeyEventHandler(this.textCode_KeyDown);
            // 
            // labelTitle
            // 
            this.labelTitle.AutoSize = true;
            this.labelTitle.Font = new System.Drawing.Font("Segoe UI Semibold", 9F, System.Drawing.FontStyle.Bold);
            this.labelTitle.Location = new System.Drawing.Point(12, 9);
            this.labelTitle.Name = "labelTitle";
            this.labelTitle.Size = new System.Drawing.Size(170, 15);
            this.labelTitle.TabIndex = 3;
            this.labelTitle.Text = "Enter your authentication code";
            // 
            // buttonOK
            // 
            this.buttonOK.Font = new System.Drawing.Font("Segoe UI", 8.25F);
            this.buttonOK.Location = new System.Drawing.Point(214, 46);
            this.buttonOK.Name = "buttonOK";
            this.buttonOK.Size = new System.Drawing.Size(75, 35);
            this.buttonOK.TabIndex = 7;
            this.buttonOK.Text = "OK";
            this.buttonOK.UseVisualStyleBackColor = true;
            this.buttonOK.Click += new System.EventHandler(this.buttonOK_Click);
            // 
            // labelEmail
            // 
            this.labelEmail.AutoSize = true;
            this.labelEmail.Font = new System.Drawing.Font("Segoe UI Semilight", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.labelEmail.Location = new System.Drawing.Point(12, 26);
            this.labelEmail.Name = "labelEmail";
            this.labelEmail.Size = new System.Drawing.Size(35, 15);
            this.labelEmail.TabIndex = 8;
            this.labelEmail.Text = "email";
            // 
            // waiting
            // 
            this.waiting.BackColor = System.Drawing.SystemColors.Control;
            this.waiting.ForeColor = System.Drawing.SystemColors.GrayText;
            this.waiting.Location = new System.Drawing.Point(43, 56);
            this.waiting.Name = "waiting";
            this.waiting.Size = new System.Drawing.Size(112, 15);
            this.waiting.TabIndex = 9;
            this.waiting.Visible = false;
            // 
            // labelError
            // 
            this.labelError.AutoSize = true;
            this.labelError.Font = new System.Drawing.Font("Segoe UI Semilight", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.labelError.ForeColor = System.Drawing.Color.Maroon;
            this.labelError.Location = new System.Drawing.Point(12, 56);
            this.labelError.Name = "labelError";
            this.labelError.Size = new System.Drawing.Size(12, 15);
            this.labelError.TabIndex = 10;
            this.labelError.Text = "-";
            this.labelError.Visible = false;
            // 
            // buttonRetry
            // 
            this.buttonRetry.Font = new System.Drawing.Font("Segoe UI", 8.25F);
            this.buttonRetry.Location = new System.Drawing.Point(214, 46);
            this.buttonRetry.Name = "buttonRetry";
            this.buttonRetry.Size = new System.Drawing.Size(75, 35);
            this.buttonRetry.TabIndex = 11;
            this.buttonRetry.Text = "Retry";
            this.buttonRetry.UseVisualStyleBackColor = true;
            this.buttonRetry.Visible = false;
            this.buttonRetry.Click += new System.EventHandler(this.buttonRetry_Click);
            // 
            // formNetworkAuthorizationRequired
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(303, 95);
            this.Controls.Add(this.textCode);
            this.Controls.Add(this.waiting);
            this.Controls.Add(this.labelEmail);
            this.Controls.Add(this.labelTitle);
            this.Controls.Add(this.labelError);
            this.Controls.Add(this.buttonOK);
            this.Controls.Add(this.buttonRetry);
            this.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.Icon = global::Gw2Launcher.Properties.Resources.Gw2Launcher;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "formNetworkAuthorizationRequired";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
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
    }
}