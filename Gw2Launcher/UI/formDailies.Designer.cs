namespace Gw2Launcher.UI
{
    partial class formDailies
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
            this.scrollV = new Gw2Launcher.UI.Controls.FlatVScrollBar();
            this.panelContainer = new System.Windows.Forms.Panel();
            this.panelContent = new System.Windows.Forms.Panel();
            this.waitingBounce = new Gw2Launcher.UI.Controls.WaitingBounce();
            this.labelMessage = new System.Windows.Forms.Label();
            this.buttonToday = new Gw2Launcher.UI.Controls.FlatVerticalButton();
            this.buttonTomorrow = new Gw2Launcher.UI.Controls.FlatVerticalButton();
            this.buttonMinimize = new Gw2Launcher.UI.Controls.FlatShapeButton();
            this.panelContainer.SuspendLayout();
            this.SuspendLayout();
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
            this.scrollV.Size = new System.Drawing.Size(6, 418);
            this.scrollV.TrackColorName = Gw2Launcher.UI.UiColors.Colors.ScrollTrack;
            this.scrollV.Value = 0;
            this.scrollV.ValueChanged += new System.EventHandler<int>(this.scrollV_ValueChanged);
            // 
            // panelContainer
            // 
            this.panelContainer.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.panelContainer.Controls.Add(this.panelContent);
            this.panelContainer.Controls.Add(this.waitingBounce);
            this.panelContainer.Controls.Add(this.labelMessage);
            this.panelContainer.Location = new System.Drawing.Point(1, 1);
            this.panelContainer.Size = new System.Drawing.Size(387, 418);
            // 
            // panelContent
            // 
            this.panelContent.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.panelContent.Location = new System.Drawing.Point(0, 0);
            this.panelContent.Size = new System.Drawing.Size(387, 418);
            this.panelContent.Visible = false;
            // 
            // waitingBounce
            // 
            this.waitingBounce.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.waitingBounce.ForeColorName = Gw2Launcher.UI.UiColors.Colors.DailiesHeaderHovered;
            this.waitingBounce.Location = new System.Drawing.Point(143, 201);
            this.waitingBounce.Size = new System.Drawing.Size(100, 16);
            this.waitingBounce.Visible = false;
            // 
            // labelMessage
            // 
            this.labelMessage.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.labelMessage.AutoSize = true;
            this.labelMessage.Location = new System.Drawing.Point(11, 8);
            this.labelMessage.Size = new System.Drawing.Size(61, 15);
            this.labelMessage.Text = "[message]";
            this.labelMessage.Visible = false;
            // 
            // buttonToday
            // 
            this.buttonToday.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonToday.BackColorHoveredName = Gw2Launcher.UI.UiColors.Colors.DailiesHeader;
            this.buttonToday.BackColorSelectedName = Gw2Launcher.UI.UiColors.Colors.DailiesHeader;
            this.buttonToday.Location = new System.Drawing.Point(394, 1);
            this.buttonToday.Size = new System.Drawing.Size(30, 150);
            this.buttonToday.Text = "Today";
            this.buttonToday.Click += new System.EventHandler(this.buttonToday_Click);
            // 
            // buttonTomorrow
            // 
            this.buttonTomorrow.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonTomorrow.BackColorHoveredName = Gw2Launcher.UI.UiColors.Colors.DailiesHeader;
            this.buttonTomorrow.BackColorSelectedName = Gw2Launcher.UI.UiColors.Colors.DailiesHeader;
            this.buttonTomorrow.Location = new System.Drawing.Point(394, 151);
            this.buttonTomorrow.Size = new System.Drawing.Size(30, 150);
            this.buttonTomorrow.Text = "Tomorrow";
            this.buttonTomorrow.Click += new System.EventHandler(this.buttonTomorrow_Click);
            // 
            // buttonMinimize
            // 
            this.buttonMinimize.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonMinimize.BackColorHoveredName = Gw2Launcher.UI.UiColors.Colors.DailiesHeader;
            this.buttonMinimize.ForeColorHoveredName = Gw2Launcher.UI.UiColors.Colors.DailiesMinimizeArrowHovered;
            this.buttonMinimize.ForeColorName = Gw2Launcher.UI.UiColors.Colors.DailiesMinimizeArrow;
            this.buttonMinimize.Location = new System.Drawing.Point(394, 399);
            this.buttonMinimize.Padding = new System.Windows.Forms.Padding(5);
            this.buttonMinimize.ShapeAlignment = System.Drawing.ContentAlignment.MiddleRight;
            this.buttonMinimize.ShapeSize = new System.Drawing.Size(4, 8);
            this.buttonMinimize.Size = new System.Drawing.Size(30, 20);
            this.buttonMinimize.Click += new System.EventHandler(this.buttonMinimize_Click);
            // 
            // formDailies
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
            this.BackColorName = Gw2Launcher.UI.UiColors.Colors.DailiesBackColor;
            this.ClientSize = new System.Drawing.Size(425, 420);
            this.Controls.Add(this.buttonMinimize);
            this.Controls.Add(this.buttonTomorrow);
            this.Controls.Add(this.buttonToday);
            this.Controls.Add(this.panelContainer);
            this.Controls.Add(this.scrollV);
            this.DoubleBuffered = true;
            this.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.ForeColorName = Gw2Launcher.UI.UiColors.Colors.DailiesText;
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.Icon = global::Gw2Launcher.Properties.Resources.Gw2Launcher;
            this.MinimumSize = new System.Drawing.Size(330, 340);
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.Manual;
            this.panelContainer.ResumeLayout(false);
            this.panelContainer.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private Controls.FlatVScrollBar scrollV;
        private System.Windows.Forms.Panel panelContainer;
        private System.Windows.Forms.Panel panelContent;
        private System.Windows.Forms.Label labelMessage;
        private Controls.WaitingBounce waitingBounce;
        private Controls.FlatVerticalButton buttonToday;
        private Controls.FlatVerticalButton buttonTomorrow;
        private Controls.FlatShapeButton buttonMinimize;
    }
}
