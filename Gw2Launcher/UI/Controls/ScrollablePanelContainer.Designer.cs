namespace Gw2Launcher.UI.Controls
{
    partial class ScrollablePanelContainer
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
            this.panelContainer = new System.Windows.Forms.Panel();
            this.scrollV = new Gw2Launcher.UI.Controls.FlatVScrollBar();
            this.SuspendLayout();
            // 
            // panelContainer
            // 
            this.panelContainer.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.panelContainer.Location = new System.Drawing.Point(3, 3);
            this.panelContainer.Size = new System.Drawing.Size(323, 236);
            // 
            // scrollV
            // 
            this.scrollV.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.scrollV.Location = new System.Drawing.Point(330, 3);
            this.scrollV.Maximum = 100;
            this.scrollV.Size = new System.Drawing.Size(8, 236);
            this.scrollV.Value = 0;
            this.scrollV.ValueChanged += new System.EventHandler<int>(this.scrollV_ValueChanged);
            // 
            // ScrollablePanelContainer
            // 
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Inherit;
            this.Controls.Add(this.panelContainer);
            this.Controls.Add(this.scrollV);
            this.Size = new System.Drawing.Size(340, 242);
            this.ResumeLayout(false);

        }

        #endregion

        private FlatVScrollBar scrollV;
        private System.Windows.Forms.Panel panelContainer;
    }
}
