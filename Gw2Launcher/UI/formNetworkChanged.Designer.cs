namespace Gw2Launcher.UI
{
    partial class formNetworkChanged
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
            this.buttonIgnore = new Gw2Launcher.UI.Controls.FlatButton();
            this.labelMessage = new System.Windows.Forms.Label();
            this.stackPanel1 = new Gw2Launcher.UI.Controls.StackPanel();
            this.panelButtons = new Gw2Launcher.UI.Controls.StackPanel();
            this.buttonAbort = new Gw2Launcher.UI.Controls.FlatButton();
            this.buttonRetry = new Gw2Launcher.UI.Controls.FlatButton();
            this.buttonRemember = new Gw2Launcher.UI.Controls.FlatButton();
            this.stackPanel1.SuspendLayout();
            this.panelButtons.SuspendLayout();
            this.SuspendLayout();
            // 
            // buttonIgnore
            // 
            this.buttonIgnore.Alignment = System.Windows.Forms.HorizontalAlignment.Center;
            this.buttonIgnore.BackColorHoveredName = Gw2Launcher.UI.UiColors.Colors.SelectButtonBackColorHovered;
            this.buttonIgnore.BackColorName = Gw2Launcher.UI.UiColors.Colors.SelectButtonBackColor;
            this.buttonIgnore.BorderColorHoveredName = Gw2Launcher.UI.UiColors.Colors.SelectButtonBorderColorHovered;
            this.buttonIgnore.BorderColorName = Gw2Launcher.UI.UiColors.Colors.SelectButtonBorderColor;
            this.buttonIgnore.BorderStyle = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonIgnore.Font = new System.Drawing.Font("Segoe UI", 8.25F);
            this.buttonIgnore.ForeColorHoveredName = Gw2Launcher.UI.UiColors.Colors.SelectButtonForeColorHovered;
            this.buttonIgnore.ForeColorName = Gw2Launcher.UI.UiColors.Colors.SelectButtonForeColor;
            this.buttonIgnore.Location = new System.Drawing.Point(95, 3);
            this.buttonIgnore.Size = new System.Drawing.Size(86, 35);
            this.buttonIgnore.Text = "Ignore";
            this.buttonIgnore.Click += new System.EventHandler(this.buttonIgnore_Click);
            // 
            // labelMessage
            // 
            this.labelMessage.AutoSize = true;
            this.labelMessage.Font = new System.Drawing.Font("Segoe UI Semilight", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.labelMessage.Location = new System.Drawing.Point(0, 0);
            this.labelMessage.Margin = new System.Windows.Forms.Padding(0, 0, 0, 8);
            this.labelMessage.Size = new System.Drawing.Size(230, 15);
            this.labelMessage.Text = "Unable to determine the current IP address";
            // 
            // stackPanel1
            // 
            this.stackPanel1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.stackPanel1.AutoSize = true;
            this.stackPanel1.Controls.Add(this.labelMessage);
            this.stackPanel1.Controls.Add(this.panelButtons);
            this.stackPanel1.Location = new System.Drawing.Point(15, 60);
            this.stackPanel1.Margin = new System.Windows.Forms.Padding(15, 60, 15, 15);
            this.stackPanel1.MinimumSize = new System.Drawing.Size(237, 0);
            this.stackPanel1.Size = new System.Drawing.Size(332, 79);
            // 
            // panelButtons
            // 
            this.panelButtons.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.panelButtons.AutoSize = true;
            this.panelButtons.Controls.Add(this.buttonAbort);
            this.panelButtons.Controls.Add(this.buttonIgnore);
            this.panelButtons.Controls.Add(this.buttonRetry);
            this.panelButtons.Controls.Add(this.buttonRemember);
            this.panelButtons.Enabled = false;
            this.panelButtons.FlowDirection = System.Windows.Forms.FlowDirection.LeftToRight;
            this.panelButtons.Location = new System.Drawing.Point(0, 38);
            this.panelButtons.Margin = new System.Windows.Forms.Padding(0, 15, 0, 0);
            this.panelButtons.Size = new System.Drawing.Size(368, 41);
            // 
            // buttonAbort
            // 
            this.buttonAbort.Alignment = System.Windows.Forms.HorizontalAlignment.Center;
            this.buttonAbort.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.buttonAbort.BackColorHoveredName = Gw2Launcher.UI.UiColors.Colors.SelectButtonBackColorHovered;
            this.buttonAbort.BackColorName = Gw2Launcher.UI.UiColors.Colors.SelectButtonBackColor;
            this.buttonAbort.BorderColorHoveredName = Gw2Launcher.UI.UiColors.Colors.SelectButtonBorderColorHovered;
            this.buttonAbort.BorderColorName = Gw2Launcher.UI.UiColors.Colors.SelectButtonBorderColor;
            this.buttonAbort.BorderStyle = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonAbort.Font = new System.Drawing.Font("Segoe UI", 8.25F);
            this.buttonAbort.ForeColorHoveredName = Gw2Launcher.UI.UiColors.Colors.SelectButtonForeColorHovered;
            this.buttonAbort.ForeColorName = Gw2Launcher.UI.UiColors.Colors.SelectButtonForeColor;
            this.buttonAbort.Location = new System.Drawing.Point(3, 3);
            this.buttonAbort.Size = new System.Drawing.Size(86, 35);
            this.buttonAbort.Text = "Abort";
            this.buttonAbort.Click += new System.EventHandler(this.buttonAbort_Click);
            // 
            // buttonRetry
            // 
            this.buttonRetry.Alignment = System.Windows.Forms.HorizontalAlignment.Center;
            this.buttonRetry.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.buttonRetry.BackColorHoveredName = Gw2Launcher.UI.UiColors.Colors.SelectButtonBackColorHovered;
            this.buttonRetry.BackColorName = Gw2Launcher.UI.UiColors.Colors.SelectButtonBackColor;
            this.buttonRetry.BorderColorHoveredName = Gw2Launcher.UI.UiColors.Colors.SelectButtonBorderColorHovered;
            this.buttonRetry.BorderColorName = Gw2Launcher.UI.UiColors.Colors.SelectButtonBorderColor;
            this.buttonRetry.BorderStyle = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonRetry.Font = new System.Drawing.Font("Segoe UI", 8.25F);
            this.buttonRetry.ForeColorHoveredName = Gw2Launcher.UI.UiColors.Colors.SelectButtonForeColorHovered;
            this.buttonRetry.ForeColorName = Gw2Launcher.UI.UiColors.Colors.SelectButtonForeColor;
            this.buttonRetry.Location = new System.Drawing.Point(187, 3);
            this.buttonRetry.Size = new System.Drawing.Size(86, 35);
            this.buttonRetry.Text = "Retry";
            this.buttonRetry.Visible = false;
            this.buttonRetry.Click += new System.EventHandler(this.buttonRetry_Click);
            // 
            // buttonRemember
            // 
            this.buttonRemember.Alignment = System.Windows.Forms.HorizontalAlignment.Center;
            this.buttonRemember.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.buttonRemember.BackColorHoveredName = Gw2Launcher.UI.UiColors.Colors.SelectButtonBackColorHovered;
            this.buttonRemember.BackColorName = Gw2Launcher.UI.UiColors.Colors.SelectButtonBackColor;
            this.buttonRemember.BorderColorHoveredName = Gw2Launcher.UI.UiColors.Colors.SelectButtonBorderColorHovered;
            this.buttonRemember.BorderColorName = Gw2Launcher.UI.UiColors.Colors.SelectButtonBorderColor;
            this.buttonRemember.BorderStyle = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonRemember.Font = new System.Drawing.Font("Segoe UI", 8.25F);
            this.buttonRemember.ForeColorHoveredName = Gw2Launcher.UI.UiColors.Colors.SelectButtonForeColorHovered;
            this.buttonRemember.ForeColorName = Gw2Launcher.UI.UiColors.Colors.SelectButtonForeColor;
            this.buttonRemember.Location = new System.Drawing.Point(279, 3);
            this.buttonRemember.Size = new System.Drawing.Size(86, 35);
            this.buttonRemember.Text = "Remember";
            this.buttonRemember.Visible = false;
            this.buttonRemember.Click += new System.EventHandler(this.buttonRemember_Click);
            // 
            // formNetworkChanged
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
            this.AutoSize = true;
            this.AutoSizeFill = Gw2Launcher.UI.Base.StackFormBase.AutoSizeFillMode.Height;
            this.BackColorName = Gw2Launcher.UI.UiColors.Colors.MainBackColor;
            this.BorderColorName = Gw2Launcher.UI.UiColors.Colors.MainBorder;
            this.ClientSize = new System.Drawing.Size(362, 151);
            this.Controls.Add(this.stackPanel1);
            this.ForeColorName = Gw2Launcher.UI.UiColors.Colors.Text;
            this.Text = "Unknown";
            this.TitleBackColorName = Gw2Launcher.UI.UiColors.Colors.BarBackColor;
            this.TitleBorderColorName = Gw2Launcher.UI.UiColors.Colors.BarBorder;
            this.TitleForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))));
            this.TitleForeColorName = Gw2Launcher.UI.UiColors.Colors.BarTitle;
            this.stackPanel1.ResumeLayout(false);
            this.stackPanel1.PerformLayout();
            this.panelButtons.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private Controls.FlatButton buttonIgnore;
        private System.Windows.Forms.Label labelMessage;
        private Controls.StackPanel stackPanel1;
        private Controls.StackPanel panelButtons;
        private Controls.FlatButton buttonAbort;
        private Controls.FlatButton buttonRetry;
        private Controls.FlatButton buttonRemember;
    }
}
