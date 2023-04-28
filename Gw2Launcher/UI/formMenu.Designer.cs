namespace Gw2Launcher.UI
{
    partial class formMenu
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
            this.panelContainer = new Gw2Launcher.UI.Controls.StackPanel();
            this.labelCloseAll = new Gw2Launcher.UI.formMenu.MenuItemLabel();
            this.panelCloseAllSep = new Gw2Launcher.UI.Base.BaseControl();
            this.panelPage = new Gw2Launcher.UI.formMenu.MenuItemPanel();
            this.label2 = new System.Windows.Forms.Label();
            this.arrowLeft = new Gw2Launcher.UI.Controls.ArrowButton();
            this.textPage = new Gw2Launcher.UI.formMenu.PageTextBox();
            this.arrowRight = new Gw2Launcher.UI.Controls.ArrowButton();
            this.labelSearch = new Gw2Launcher.UI.formMenu.MenuItemLabel();
            this.panel2 = new Gw2Launcher.UI.Base.BaseControl();
            this.labelSettings = new Gw2Launcher.UI.formMenu.MenuItemLabel();
            this.panelContainer.SuspendLayout();
            this.panelPage.SuspendLayout();
            this.SuspendLayout();
            // 
            // panelContainer
            // 
            this.panelContainer.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.panelContainer.AutoSize = true;
            this.panelContainer.AutoSizeFill = Gw2Launcher.UI.Controls.StackPanel.AutoSizeFillMode.NoWrap;
            this.panelContainer.Controls.Add(this.labelCloseAll);
            this.panelContainer.Controls.Add(this.panelCloseAllSep);
            this.panelContainer.Controls.Add(this.panelPage);
            this.panelContainer.Controls.Add(this.labelSearch);
            this.panelContainer.Controls.Add(this.panel2);
            this.panelContainer.Controls.Add(this.labelSettings);
            this.panelContainer.Location = new System.Drawing.Point(1, 11);
            this.panelContainer.Margin = new System.Windows.Forms.Padding(1, 11, 1, 1);
            this.panelContainer.Name = "panelContainer";
            this.panelContainer.Padding = new System.Windows.Forms.Padding(0, 3, 0, 3);
            this.panelContainer.Size = new System.Drawing.Size(138, 123);
            this.panelContainer.TabIndex = 0;
            // 
            // labelCloseAll
            // 
            this.labelCloseAll.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.labelCloseAll.BackColorHovered = System.Drawing.Color.FromArgb(((int)(((byte)(244)))), ((int)(((byte)(177)))), ((int)(((byte)(177)))));
            this.labelCloseAll.BackColorHoveredName = Gw2Launcher.UI.UiColors.Colors.MenuCloseAllBackColorHovered;
            this.labelCloseAll.BackColorName = Gw2Launcher.UI.UiColors.Colors.MenuCloseAllBackColor;
            this.labelCloseAll.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(187)))), ((int)(((byte)(164)))), ((int)(((byte)(161)))));
            this.labelCloseAll.BorderColorHovered = System.Drawing.Color.FromArgb(((int)(((byte)(166)))), ((int)(((byte)(135)))), ((int)(((byte)(132)))));
            this.labelCloseAll.BorderColorHoveredName = Gw2Launcher.UI.UiColors.Colors.MenuCloseAllBorderColorHovered;
            this.labelCloseAll.BorderColorName = Gw2Launcher.UI.UiColors.Colors.MenuCloseAllBorderColor;
            this.labelCloseAll.ForeColorName = Gw2Launcher.UI.UiColors.Colors.MenuCloseAllForeColor;
            this.labelCloseAll.Location = new System.Drawing.Point(0, 5);
            this.labelCloseAll.Margin = new System.Windows.Forms.Padding(0, 2, 0, 1);
            this.labelCloseAll.Name = "labelCloseAll";
            this.labelCloseAll.Padding = new System.Windows.Forms.Padding(11, 7, 0, 6);
            this.labelCloseAll.Size = new System.Drawing.Size(138, 26);
            this.labelCloseAll.TabIndex = 0;
            this.labelCloseAll.Text = "Close all accounts";
            this.labelCloseAll.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.labelCloseAll.Visible = false;
            this.labelCloseAll.Click += new System.EventHandler(this.labelCloseAll_Click);
            // 
            // panelCloseAllSep
            // 
            this.panelCloseAllSep.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.panelCloseAllSep.BackColorName = Gw2Launcher.UI.UiColors.Colors.MenuSeparator;
            this.panelCloseAllSep.Location = new System.Drawing.Point(0, 34);
            this.panelCloseAllSep.Margin = new System.Windows.Forms.Padding(0, 2, 0, 2);
            this.panelCloseAllSep.Name = "panelCloseAllSep";
            this.panelCloseAllSep.Size = new System.Drawing.Size(138, 1);
            this.panelCloseAllSep.TabIndex = 1;
            this.panelCloseAllSep.Visible = false;
            // 
            // panelPage
            // 
            this.panelPage.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.panelPage.Controls.Add(this.label2);
            this.panelPage.Controls.Add(this.arrowLeft);
            this.panelPage.Controls.Add(this.textPage);
            this.panelPage.Controls.Add(this.arrowRight);
            this.panelPage.FlowDirection = System.Windows.Forms.FlowDirection.LeftToRight;
            this.panelPage.Location = new System.Drawing.Point(0, 37);
            this.panelPage.Margin = new System.Windows.Forms.Padding(0);
            this.panelPage.Name = "panelPage";
            this.panelPage.Size = new System.Drawing.Size(138, 26);
            this.panelPage.TabIndex = 2;
            // 
            // label2
            // 
            this.label2.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(0, 0);
            this.label2.Margin = new System.Windows.Forms.Padding(0);
            this.label2.Name = "label2";
            this.label2.Padding = new System.Windows.Forms.Padding(11, 7, 0, 6);
            this.label2.Size = new System.Drawing.Size(69, 26);
            this.label2.TabIndex = 0;
            this.label2.Text = "Page";
            this.label2.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // arrowLeft
            // 
            this.arrowLeft.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.arrowLeft.Cursor = Windows.Cursors.Hand;
            this.arrowLeft.Direction = Gw2Launcher.UI.Controls.ArrowButton.ArrowDirection.Left;
            this.arrowLeft.ForeColorHighlightName = Gw2Launcher.UI.UiColors.Colors.MenuArrowHovered;
            this.arrowLeft.ForeColorName = Gw2Launcher.UI.UiColors.Colors.MenuArrow;
            this.arrowLeft.Location = new System.Drawing.Point(69, 8);
            this.arrowLeft.Margin = new System.Windows.Forms.Padding(0, 1, 0, 0);
            this.arrowLeft.Name = "arrowLeft";
            this.arrowLeft.Size = new System.Drawing.Size(6, 11);
            this.arrowLeft.TabIndex = 1;
            this.arrowLeft.MouseDown += new System.Windows.Forms.MouseEventHandler(this.arrowLeft_MouseDown);
            // 
            // textPage
            // 
            this.textPage.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left)));
            this.textPage.Cursor = System.Windows.Forms.Cursors.IBeam;
            this.textPage.Location = new System.Drawing.Point(81, 0);
            this.textPage.Margin = new System.Windows.Forms.Padding(6, 0, 6, 0);
            this.textPage.Name = "textPage";
            this.textPage.Size = new System.Drawing.Size(29, 26);
            this.textPage.TabIndex = 2;
            this.textPage.Value = 0;
            // 
            // arrowRight
            // 
            this.arrowRight.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.arrowRight.Cursor = Windows.Cursors.Hand;
            this.arrowRight.Direction = Gw2Launcher.UI.Controls.ArrowButton.ArrowDirection.Right;
            this.arrowRight.ForeColorHighlightName = Gw2Launcher.UI.UiColors.Colors.MenuArrowHovered;
            this.arrowRight.ForeColorName = Gw2Launcher.UI.UiColors.Colors.MenuArrow;
            this.arrowRight.Location = new System.Drawing.Point(116, 8);
            this.arrowRight.Margin = new System.Windows.Forms.Padding(0, 1, 16, 0);
            this.arrowRight.Name = "arrowRight";
            this.arrowRight.Size = new System.Drawing.Size(6, 11);
            this.arrowRight.TabIndex = 3;
            this.arrowRight.MouseDown += new System.Windows.Forms.MouseEventHandler(this.arrowRight_MouseDown);
            // 
            // labelSearch
            // 
            this.labelSearch.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.labelSearch.AutoSize = true;
            this.labelSearch.BackColorHoveredName = Gw2Launcher.UI.UiColors.Colors.MenuBackColorHovered;
            this.labelSearch.Location = new System.Drawing.Point(0, 63);
            this.labelSearch.Margin = new System.Windows.Forms.Padding(0);
            this.labelSearch.Name = "labelSearch";
            this.labelSearch.Padding = new System.Windows.Forms.Padding(11, 7, 0, 6);
            this.labelSearch.Size = new System.Drawing.Size(138, 26);
            this.labelSearch.TabIndex = 3;
            this.labelSearch.Text = "Search";
            this.labelSearch.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.labelSearch.Click += new System.EventHandler(this.labelSearch_Click);
            // 
            // panel2
            // 
            this.panel2.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.panel2.BackColorName = Gw2Launcher.UI.UiColors.Colors.MenuSeparator;
            this.panel2.Location = new System.Drawing.Point(0, 91);
            this.panel2.Margin = new System.Windows.Forms.Padding(0, 2, 0, 2);
            this.panel2.Name = "panel2";
            this.panel2.Size = new System.Drawing.Size(138, 1);
            this.panel2.TabIndex = 4;
            // 
            // labelSettings
            // 
            this.labelSettings.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.labelSettings.AutoSize = true;
            this.labelSettings.BackColorHoveredName = Gw2Launcher.UI.UiColors.Colors.MenuBackColorHovered;
            this.labelSettings.Location = new System.Drawing.Point(0, 94);
            this.labelSettings.Margin = new System.Windows.Forms.Padding(0);
            this.labelSettings.Name = "labelSettings";
            this.labelSettings.Padding = new System.Windows.Forms.Padding(11, 7, 0, 6);
            this.labelSettings.Size = new System.Drawing.Size(138, 26);
            this.labelSettings.TabIndex = 5;
            this.labelSettings.Text = "Settings";
            this.labelSettings.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.labelSettings.Click += new System.EventHandler(this.labelSettings_Click);
            // 
            // formMenu
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
            this.AutoSize = true;
            this.AutoSizeFill = Gw2Launcher.UI.Base.StackFormBase.AutoSizeFillMode.Height;
            this.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.BackColorName = Gw2Launcher.UI.UiColors.Colors.MenuBackColor;
            this.ClientSize = new System.Drawing.Size(140, 185);
            this.Controls.Add(this.panelContainer);
            this.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.ForeColorName = Gw2Launcher.UI.UiColors.Colors.MenuText;
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.Icon = global::Gw2Launcher.Properties.Resources.Gw2Launcher;
            this.Name = "formMenu";
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.Manual;
            this.panelContainer.ResumeLayout(false);
            this.panelContainer.PerformLayout();
            this.panelPage.ResumeLayout(false);
            this.panelPage.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private Controls.ArrowButton arrowLeft;
        private Controls.ArrowButton arrowRight;
        private PageTextBox textPage;
        private MenuItemPanel panelPage;
        private Controls.StackPanel panelContainer;
        private System.Windows.Forms.Label label2;
        private MenuItemLabel labelSettings;
        private Base.BaseControl panel2;
        private MenuItemLabel labelSearch;
        private formMenu.MenuItemLabel labelCloseAll;
        private Base.BaseControl panelCloseAllSep;
    }
}
