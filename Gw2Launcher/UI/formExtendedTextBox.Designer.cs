namespace Gw2Launcher.UI
{
    partial class formExtendedTextBox
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
            this.textText = new Gw2Launcher.UI.formExtendedTextBox.TextBoxM();
            this.buttonResize = new Gw2Launcher.UI.Controls.SizeDragButton();
            this.SuspendLayout();
            // 
            // textText
            // 
            this.textText.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.textText.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.textText.Location = new System.Drawing.Point(0, 0);
            this.textText.Multiline = true;
            this.textText.Name = "textText";
            this.textText.ScrollBars = System.Windows.Forms.ScrollBars.Both;
            this.textText.Size = new System.Drawing.Size(284, 261);
            this.textText.TabIndex = 56;
            this.textText.WordWrap = false;
            // 
            // buttonResize
            // 
            this.buttonResize.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonResize.Cursor = System.Windows.Forms.Cursors.SizeNWSE;
            this.buttonResize.Enabled = false;
            this.buttonResize.Location = new System.Drawing.Point(271, 248);
            this.buttonResize.Name = "buttonResize";
            this.buttonResize.Size = new System.Drawing.Size(12, 12);
            this.buttonResize.TabIndex = 57;
            this.buttonResize.TabStop = false;
            // 
            // formExtendedTextBox
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(284, 261);
            this.Controls.Add(this.buttonResize);
            this.Controls.Add(this.textText);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.Name = "formExtendedTextBox";
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.Manual;
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private TextBoxM textText;
        private Controls.SizeDragButton buttonResize;
    }
}