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
            this.panelContents = new Gw2Launcher.UI.Controls.AccountGridButtonContainer.BufferedPanel();
            this.verticalScroll = new System.Windows.Forms.VScrollBar();
            this.scrollV = new Gw2Launcher.UI.Controls.FlatVScrollBar();
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
            // scrollV
            // 
            this.scrollV.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.scrollV.Location = new System.Drawing.Point(277, 0);
            this.scrollV.Maximum = 100;
            this.scrollV.Name = "scrollV";
            this.scrollV.Size = new System.Drawing.Size(6, 245);
            this.scrollV.TabIndex = 1;
            this.scrollV.Value = 0;
            this.scrollV.Visible = false;
            this.scrollV.ValueChanged += new System.EventHandler<int>(this.scrollV_ValueChanged);
            // 
            // AccountGridButtonContainer
            // 
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Inherit;
            this.BackColor = System.Drawing.Color.White;
            this.Controls.Add(this.scrollV);
            this.Controls.Add(this.verticalScroll);
            this.Controls.Add(this.panelContents);
            this.Name = "AccountGridButtonContainer";
            this.Size = new System.Drawing.Size(283, 245);
            this.Load += new System.EventHandler(this.AccountGridButtonContainer_Load);
            this.ResumeLayout(false);

        }

        #endregion

        private BufferedPanel panelContents;
        private System.Windows.Forms.VScrollBar verticalScroll;
        private FlatVScrollBar scrollV;
    }
}
