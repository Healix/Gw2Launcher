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
            this.panelContents = new Gw2Launcher.UI.Controls.AccountGridButtonPanel();
            this.scrollV = new Gw2Launcher.UI.Controls.FlatVScrollBar();
            this.SuspendLayout();
            // 
            // panelContents
            // 
            this.panelContents.GridLayout = Gw2Launcher.UI.Controls.AccountGridButtonPanel.GridLayoutMode.Auto;
            this.panelContents.GridRowHeight = 60;
            this.panelContents.Location = new System.Drawing.Point(0, 0);
            this.panelContents.NewButton = null;
            this.panelContents.Size = new System.Drawing.Size(200, 0);
            // 
            // scrollV
            // 
            this.scrollV.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.scrollV.ForeColorHoveredName = Gw2Launcher.UI.UiColors.Colors.ScrollBarHovered;
            this.scrollV.ForeColorName = Gw2Launcher.UI.UiColors.Colors.ScrollBar;
            this.scrollV.Location = new System.Drawing.Point(277, 0);
            this.scrollV.Maximum = 100;
            this.scrollV.ScrollChange = 0;
            this.scrollV.Size = new System.Drawing.Size(6, 245);
            this.scrollV.TrackColorName = Gw2Launcher.UI.UiColors.Colors.ScrollTrack;
            this.scrollV.Value = 0;
            this.scrollV.Visible = false;
            this.scrollV.ValueChanged += new System.EventHandler<int>(this.scrollV_ValueChanged);
            // 
            // AccountGridButtonContainer
            // 
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Inherit;
            this.Controls.Add(this.scrollV);
            this.Controls.Add(this.panelContents);
            this.Size = new System.Drawing.Size(283, 245);
            this.ResumeLayout(false);

        }

        #endregion

        private AccountGridButtonPanel panelContents;
        private FlatVScrollBar scrollV;
    }
}
