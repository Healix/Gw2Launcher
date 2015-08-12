namespace Gw2Launcher.UI.Controls
{
    partial class AccountGridButtonContainer
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
            this.panelContents = new System.Windows.Forms.Panel();
            this.verticalScroll = new System.Windows.Forms.VScrollBar();
            this.SuspendLayout();
            // 
            // panelContents
            // 
            this.panelContents.Location = new System.Drawing.Point(0, 0);
            this.panelContents.Name = "panelContents";
            this.panelContents.Size = new System.Drawing.Size(200, 100);
            this.panelContents.TabIndex = 0;
            this.panelContents.Paint += new System.Windows.Forms.PaintEventHandler(this.panelContents_Paint);
            // 
            // verticalScroll
            // 
            this.verticalScroll.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.verticalScroll.Location = new System.Drawing.Point(266, 0);
            this.verticalScroll.Name = "verticalScroll";
            this.verticalScroll.Size = new System.Drawing.Size(17, 245);
            this.verticalScroll.TabIndex = 1;
            this.verticalScroll.Visible = false;
            this.verticalScroll.Scroll += new System.Windows.Forms.ScrollEventHandler(this.verticalScroll_Scroll);
            this.verticalScroll.ValueChanged += new System.EventHandler(this.verticalScroll_ValueChanged);
            // 
            // AccountGridButtonContainer
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.White;
            this.Controls.Add(this.verticalScroll);
            this.Controls.Add(this.panelContents);
            this.Name = "AccountGridButtonContainer";
            this.Size = new System.Drawing.Size(283, 245);
            this.Load += new System.EventHandler(this.AccountGridButtonContainer_Load);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Panel panelContents;
        private System.Windows.Forms.VScrollBar verticalScroll;
    }
}
