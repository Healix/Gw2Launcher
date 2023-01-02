namespace Gw2Launcher.UI
{
    partial class formScreenPosition
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.radioTL = new System.Windows.Forms.RadioButton();
            this.radioBL = new System.Windows.Forms.RadioButton();
            this.radioBR = new System.Windows.Forms.RadioButton();
            this.radioTR = new System.Windows.Forms.RadioButton();
            this.radioR = new System.Windows.Forms.RadioButton();
            this.radioL = new System.Windows.Forms.RadioButton();
            this.labelScreen = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.arrowLeft = new Gw2Launcher.UI.Controls.ArrowButton();
            this.arrowRight = new Gw2Launcher.UI.Controls.ArrowButton();
            this.SuspendLayout();
            // 
            // radioTL
            // 
            this.radioTL.AutoSize = true;
            this.radioTL.Location = new System.Drawing.Point(12, 12);
            this.radioTL.Size = new System.Drawing.Size(14, 13);
            this.radioTL.TabStop = true;
            this.radioTL.UseVisualStyleBackColor = true;
            // 
            // radioBL
            // 
            this.radioBL.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.radioBL.AutoSize = true;
            this.radioBL.Location = new System.Drawing.Point(12, 81);
            this.radioBL.Size = new System.Drawing.Size(14, 13);
            this.radioBL.TabStop = true;
            this.radioBL.UseVisualStyleBackColor = true;
            // 
            // radioBR
            // 
            this.radioBR.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.radioBR.AutoSize = true;
            this.radioBR.Location = new System.Drawing.Point(164, 81);
            this.radioBR.Size = new System.Drawing.Size(14, 13);
            this.radioBR.TabStop = true;
            this.radioBR.UseVisualStyleBackColor = true;
            // 
            // radioTR
            // 
            this.radioTR.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.radioTR.AutoSize = true;
            this.radioTR.Location = new System.Drawing.Point(164, 12);
            this.radioTR.Size = new System.Drawing.Size(14, 13);
            this.radioTR.TabStop = true;
            this.radioTR.UseVisualStyleBackColor = true;
            // 
            // radioR
            // 
            this.radioR.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.radioR.AutoSize = true;
            this.radioR.Location = new System.Drawing.Point(164, 46);
            this.radioR.Size = new System.Drawing.Size(14, 13);
            this.radioR.TabStop = true;
            this.radioR.UseVisualStyleBackColor = true;
            this.radioR.Visible = false;
            // 
            // radioL
            // 
            this.radioL.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.radioL.AutoSize = true;
            this.radioL.Location = new System.Drawing.Point(12, 46);
            this.radioL.Size = new System.Drawing.Size(14, 13);
            this.radioL.TabStop = true;
            this.radioL.UseVisualStyleBackColor = true;
            this.radioL.Visible = false;
            // 
            // labelScreen
            // 
            this.labelScreen.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.labelScreen.AutoSize = true;
            this.labelScreen.Font = new System.Drawing.Font("Segoe UI", 18F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.labelScreen.Location = new System.Drawing.Point(82, 36);
            this.labelScreen.Size = new System.Drawing.Size(28, 32);
            this.labelScreen.Text = "1";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("Segoe UI Semilight", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label1.ForeColor = System.Drawing.SystemColors.GrayText;
            this.label1.Location = new System.Drawing.Point(77, 23);
            this.label1.Size = new System.Drawing.Size(38, 13);
            this.label1.Text = "screen";
            // 
            // arrowLeft
            // 
            this.arrowLeft.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.arrowLeft.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(150)))), ((int)(((byte)(150)))), ((int)(((byte)(150)))));
            this.arrowLeft.ForeColorHighlight = System.Drawing.Color.FromArgb(((int)(((byte)(20)))), ((int)(((byte)(20)))), ((int)(((byte)(20)))));
            this.arrowLeft.Cursor = System.Windows.Forms.Cursors.Hand;
            this.arrowLeft.Direction = Gw2Launcher.UI.Controls.ArrowButton.ArrowDirection.Left;
            this.arrowLeft.Location = new System.Drawing.Point(70, 48);
            this.arrowLeft.Size = new System.Drawing.Size(6, 11);
            this.arrowLeft.Visible = false;
            this.arrowLeft.Click += new System.EventHandler(this.arrowLeft_Click);
            // 
            // arrowRight
            // 
            this.arrowRight.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.arrowRight.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(150)))), ((int)(((byte)(150)))), ((int)(((byte)(150)))));
            this.arrowRight.ForeColorHighlight = System.Drawing.Color.FromArgb(((int)(((byte)(20)))), ((int)(((byte)(20)))), ((int)(((byte)(20)))));
            this.arrowRight.Cursor = System.Windows.Forms.Cursors.Hand;
            this.arrowRight.Direction = Gw2Launcher.UI.Controls.ArrowButton.ArrowDirection.Right;
            this.arrowRight.Location = new System.Drawing.Point(116, 48);
            this.arrowRight.Size = new System.Drawing.Size(6, 11);
            this.arrowRight.Visible = false;
            this.arrowRight.Click += new System.EventHandler(this.arrowRight_Click);
            // 
            // formScreenPosition
            // 
            this.ClientSize = new System.Drawing.Size(190, 106);
            this.ControlBox = false;
            this.Controls.Add(this.label1);
            this.Controls.Add(this.arrowLeft);
            this.Controls.Add(this.arrowRight);
            this.Controls.Add(this.labelScreen);
            this.Controls.Add(this.radioL);
            this.Controls.Add(this.radioR);
            this.Controls.Add(this.radioTR);
            this.Controls.Add(this.radioBR);
            this.Controls.Add(this.radioBL);
            this.Controls.Add(this.radioTL);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Load += new System.EventHandler(this.formScreenPosition_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.RadioButton radioTL;
        private System.Windows.Forms.RadioButton radioBL;
        private System.Windows.Forms.RadioButton radioBR;
        private System.Windows.Forms.RadioButton radioTR;
        private System.Windows.Forms.RadioButton radioR;
        private System.Windows.Forms.RadioButton radioL;
        private System.Windows.Forms.Label labelScreen;
        private Controls.ArrowButton arrowRight;
        private Controls.ArrowButton arrowLeft;
        private System.Windows.Forms.Label label1;
    }
}
