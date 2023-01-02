namespace Gw2Launcher.UI
{
    partial class formNotes
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
            this.panelContainer = new System.Windows.Forms.Panel();
            this.panelContent = new System.Windows.Forms.Panel();
            this.labelMessage = new System.Windows.Forms.Label();
            this.panelAdd = new System.Windows.Forms.Panel();
            this.labelAccountName = new Gw2Launcher.UI.Base.BaseLabel();
            this.labelAdd = new Gw2Launcher.UI.Controls.LinkLabel();
            this.scrollV = new Gw2Launcher.UI.Controls.FlatVScrollBar();
            this.buttonMinimize = new Gw2Launcher.UI.Controls.FlatShapeButton();
            this.buttonExpired = new Gw2Launcher.UI.Controls.FlatVerticalButton();
            this.buttonMessages = new Gw2Launcher.UI.Controls.FlatVerticalButton();
            this.panelContainer.SuspendLayout();
            this.panelAdd.SuspendLayout();
            this.SuspendLayout();
            // 
            // panelContainer
            // 
            this.panelContainer.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.panelContainer.Controls.Add(this.panelContent);
            this.panelContainer.Controls.Add(this.labelMessage);
            this.panelContainer.Location = new System.Drawing.Point(1, 1);
            this.panelContainer.Size = new System.Drawing.Size(387, 303);
            // 
            // panelContent
            // 
            this.panelContent.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.panelContent.Location = new System.Drawing.Point(0, 0);
            this.panelContent.Size = new System.Drawing.Size(387, 303);
            this.panelContent.Visible = false;
            // 
            // labelMessage
            // 
            this.labelMessage.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.labelMessage.AutoSize = true;
            this.labelMessage.Location = new System.Drawing.Point(11, -28);
            this.labelMessage.Size = new System.Drawing.Size(61, 15);
            this.labelMessage.Text = "[message]";
            this.labelMessage.Visible = false;
            // 
            // panelAdd
            // 
            this.panelAdd.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.panelAdd.Controls.Add(this.labelAccountName);
            this.panelAdd.Controls.Add(this.labelAdd);
            this.panelAdd.Location = new System.Drawing.Point(1, 304);
            this.panelAdd.Size = new System.Drawing.Size(387, 31);
            // 
            // labelAccountName
            // 
            this.labelAccountName.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.labelAccountName.AutoEllipsis = true;
            this.labelAccountName.ForeColorName = Gw2Launcher.UI.UiColors.Colors.TextGray;
            this.labelAccountName.Location = new System.Drawing.Point(45, 8);
            this.labelAccountName.Size = new System.Drawing.Size(336, 15);
            this.labelAccountName.Text = "-";
            this.labelAccountName.TextAlign = System.Drawing.ContentAlignment.TopRight;
            // 
            // labelAdd
            // 
            this.labelAdd.AutoSize = true;
            this.labelAdd.ForeColorHoveredName = Gw2Launcher.UI.UiColors.Colors.LinkHovered;
            this.labelAdd.ForeColorName = Gw2Launcher.UI.UiColors.Colors.Link;
            this.labelAdd.Icon = null;
            this.labelAdd.Location = new System.Drawing.Point(10, 8);
            this.labelAdd.Size = new System.Drawing.Size(29, 15);
            this.labelAdd.Text = "new";
            this.labelAdd.Click += new System.EventHandler(this.labelAdd_Click);
            // 
            // scrollV
            // 
            this.scrollV.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.scrollV.ForeColorHoveredName = Gw2Launcher.UI.UiColors.Colors.ScrollBarHovered;
            this.scrollV.ForeColorName = Gw2Launcher.UI.UiColors.Colors.ScrollBar;
            this.scrollV.Location = new System.Drawing.Point(388, 1);
            this.scrollV.Maximum = 0;
            this.scrollV.ScrollChange = 0;
            this.scrollV.Size = new System.Drawing.Size(6, 334);
            this.scrollV.TrackColorName = Gw2Launcher.UI.UiColors.Colors.ScrollTrack;
            this.scrollV.Value = 0;
            this.scrollV.ValueChanged += new System.EventHandler<int>(this.scrollV_ValueChanged);
            // 
            // buttonMinimize
            // 
            this.buttonMinimize.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonMinimize.BackColorHoveredName = Gw2Launcher.UI.UiColors.Colors.DailiesHeader;
            this.buttonMinimize.ForeColorHoveredName = Gw2Launcher.UI.UiColors.Colors.DailiesMinimizeArrowHovered;
            this.buttonMinimize.ForeColorName = Gw2Launcher.UI.UiColors.Colors.DailiesMinimizeArrow;
            this.buttonMinimize.Location = new System.Drawing.Point(394, 315);
            this.buttonMinimize.Padding = new System.Windows.Forms.Padding(5);
            this.buttonMinimize.Shape = Gw2Launcher.UI.Controls.FlatShapeButton.IconShape.X;
            this.buttonMinimize.ShapeAlignment = System.Drawing.ContentAlignment.MiddleRight;
            this.buttonMinimize.ShapeSize = new System.Drawing.Size(7, 7);
            this.buttonMinimize.Size = new System.Drawing.Size(30, 20);
            this.buttonMinimize.Click += new System.EventHandler(this.buttonMinimize_Click);
            // 
            // buttonExpired
            // 
            this.buttonExpired.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonExpired.BackColorHoveredName = Gw2Launcher.UI.UiColors.Colors.DailiesHeader;
            this.buttonExpired.BackColorSelectedName = Gw2Launcher.UI.UiColors.Colors.DailiesHeader;
            this.buttonExpired.Location = new System.Drawing.Point(394, 121);
            this.buttonExpired.Size = new System.Drawing.Size(30, 120);
            this.buttonExpired.Text = "Recent";
            this.buttonExpired.Click += new System.EventHandler(this.buttonExpired_Click);
            // 
            // buttonMessages
            // 
            this.buttonMessages.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonMessages.BackColorHoveredName = Gw2Launcher.UI.UiColors.Colors.DailiesHeader;
            this.buttonMessages.BackColorSelectedName = Gw2Launcher.UI.UiColors.Colors.DailiesHeader;
            this.buttonMessages.Location = new System.Drawing.Point(394, 1);
            this.buttonMessages.Size = new System.Drawing.Size(30, 120);
            this.buttonMessages.Text = "Notes";
            this.buttonMessages.Click += new System.EventHandler(this.buttonMessages_Click);
            // 
            // formNotes
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
            this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(255)))), ((int)(((byte)(255)))));
            this.BackColorName = Gw2Launcher.UI.UiColors.Colors.DailiesBackColor;
            this.ClientSize = new System.Drawing.Size(425, 336);
            this.Controls.Add(this.panelContainer);
            this.Controls.Add(this.panelAdd);
            this.Controls.Add(this.scrollV);
            this.Controls.Add(this.buttonMinimize);
            this.Controls.Add(this.buttonExpired);
            this.Controls.Add(this.buttonMessages);
            this.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.ForeColorName = Gw2Launcher.UI.UiColors.Colors.DailiesText;
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.Icon = global::Gw2Launcher.Properties.Resources.Gw2Launcher;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.MinimumSize = new System.Drawing.Size(425, 336);
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Messages";
            this.panelContainer.ResumeLayout(false);
            this.panelContainer.PerformLayout();
            this.panelAdd.ResumeLayout(false);
            this.panelAdd.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Panel panelContainer;
        private System.Windows.Forms.Panel panelContent;
        private System.Windows.Forms.Label labelMessage;
        private Controls.FlatVScrollBar scrollV;
        private Controls.FlatShapeButton buttonMinimize;
        private Controls.FlatVerticalButton buttonExpired;
        private Controls.FlatVerticalButton buttonMessages;
        private Controls.LinkLabel labelAdd;
        private System.Windows.Forms.Panel panelAdd;
        private Base.BaseLabel labelAccountName;

    }
}
