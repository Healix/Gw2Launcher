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
            this.panelCloseAllSep = new System.Windows.Forms.Panel();
            this.panelPage = new Gw2Launcher.UI.formMenu.MenuItemPanel();
            this.label2 = new System.Windows.Forms.Label();
            this.arrowRight = new Gw2Launcher.UI.Controls.ArrowButton();
            this.arrowLeft = new Gw2Launcher.UI.Controls.ArrowButton();
            this.labelPage = new System.Windows.Forms.Label();
            this.labelSearch = new Gw2Launcher.UI.formMenu.MenuItemLabel();
            this.panel2 = new System.Windows.Forms.Panel();
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
            this.panelContainer.Padding = new System.Windows.Forms.Padding(0, 3, 0, 3);
            this.panelContainer.Size = new System.Drawing.Size(138, 123);
            // 
            // labelCloseAll
            // 
            this.labelCloseAll.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
            | System.Windows.Forms.AnchorStyles.Right)));
            this.labelCloseAll.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(244)))), ((int)(((byte)(198)))), ((int)(((byte)(198)))));
            this.labelCloseAll.BackColorHovered = System.Drawing.Color.FromArgb(((int)(((byte)(244)))), ((int)(((byte)(177)))), ((int)(((byte)(177)))));
            this.labelCloseAll.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(187)))), ((int)(((byte)(164)))), ((int)(((byte)(161)))));
            this.labelCloseAll.BorderColorHovered = System.Drawing.Color.FromArgb(((int)(((byte)(166)))), ((int)(((byte)(135)))), ((int)(((byte)(132)))));
            this.labelCloseAll.Location = new System.Drawing.Point(0, 5);
            this.labelCloseAll.Margin = new System.Windows.Forms.Padding(0, 2, 0, 1);
            this.labelCloseAll.Padding = new System.Windows.Forms.Padding(11, 0, 0, 0);
            this.labelCloseAll.Size = new System.Drawing.Size(138, 26);
            this.labelCloseAll.Text = "Close all accounts";
            this.labelCloseAll.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.labelCloseAll.Visible = false;
            this.labelCloseAll.Click += new System.EventHandler(this.labelCloseAll_Click);
            // 
            // panelCloseAllSep
            // 
            this.panelCloseAllSep.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
            | System.Windows.Forms.AnchorStyles.Right)));
            this.panelCloseAllSep.BackColor = System.Drawing.Color.Gainsboro;
            this.panelCloseAllSep.Location = new System.Drawing.Point(0, 34);
            this.panelCloseAllSep.Margin = new System.Windows.Forms.Padding(0, 2, 0, 2);
            this.panelCloseAllSep.Size = new System.Drawing.Size(138, 1);
            this.panelCloseAllSep.Visible = false;
            // 
            // panelPage
            // 
            this.panelPage.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
            | System.Windows.Forms.AnchorStyles.Right)));
            this.panelPage.Controls.Add(this.label2);
            this.panelPage.Controls.Add(this.arrowRight);
            this.panelPage.Controls.Add(this.arrowLeft);
            this.panelPage.Controls.Add(this.labelPage);
            this.panelPage.Location = new System.Drawing.Point(0, 37);
            this.panelPage.Margin = new System.Windows.Forms.Padding(0);
            this.panelPage.Size = new System.Drawing.Size(138, 26);
            // 
            // label2
            // 
            this.label2.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
            | System.Windows.Forms.AnchorStyles.Left)
            | System.Windows.Forms.AnchorStyles.Right)));
            this.label2.Location = new System.Drawing.Point(0, 0);
            this.label2.Padding = new System.Windows.Forms.Padding(11, 0, 0, 0);
            this.label2.Size = new System.Drawing.Size(63, 26);
            this.label2.Text = "Page";
            this.label2.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // arrowRight
            // 
            this.arrowRight.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.arrowRight.ColorArrow = System.Drawing.Color.FromArgb(((int)(((byte)(150)))), ((int)(((byte)(150)))), ((int)(((byte)(150)))));
            this.arrowRight.ColorHighlight = System.Drawing.Color.FromArgb(((int)(((byte)(20)))), ((int)(((byte)(20)))), ((int)(((byte)(20)))));
            this.arrowRight.Cursor = System.Windows.Forms.Cursors.Hand;
            this.arrowRight.Direction = Gw2Launcher.UI.Controls.ArrowButton.ArrowDirection.Right;
            this.arrowRight.Location = new System.Drawing.Point(116, 8);
            this.arrowRight.Size = new System.Drawing.Size(6, 11);
            this.arrowRight.MouseDown += new System.Windows.Forms.MouseEventHandler(this.arrowRight_MouseDown);
            // 
            // arrowLeft
            // 
            this.arrowLeft.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.arrowLeft.ColorArrow = System.Drawing.Color.FromArgb(((int)(((byte)(150)))), ((int)(((byte)(150)))), ((int)(((byte)(150)))));
            this.arrowLeft.ColorHighlight = System.Drawing.Color.FromArgb(((int)(((byte)(20)))), ((int)(((byte)(20)))), ((int)(((byte)(20)))));
            this.arrowLeft.Cursor = System.Windows.Forms.Cursors.Hand;
            this.arrowLeft.Direction = Gw2Launcher.UI.Controls.ArrowButton.ArrowDirection.Left;
            this.arrowLeft.Location = new System.Drawing.Point(69, 8);
            this.arrowLeft.Size = new System.Drawing.Size(6, 11);
            this.arrowLeft.MouseDown += new System.Windows.Forms.MouseEventHandler(this.arrowLeft_MouseDown);
            // 
            // labelPage
            // 
            this.labelPage.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
            | System.Windows.Forms.AnchorStyles.Right)));
            this.labelPage.Location = new System.Drawing.Point(81, 0);
            this.labelPage.Size = new System.Drawing.Size(29, 24);
            this.labelPage.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // labelSearch
            // 
            this.labelSearch.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
            | System.Windows.Forms.AnchorStyles.Right)));
            this.labelSearch.BorderColor = System.Drawing.Color.Empty;
            this.labelSearch.BorderColorHovered = System.Drawing.Color.Empty;
            this.labelSearch.Location = new System.Drawing.Point(0, 63);
            this.labelSearch.Margin = new System.Windows.Forms.Padding(0);
            this.labelSearch.Padding = new System.Windows.Forms.Padding(11, 0, 0, 0);
            this.labelSearch.Size = new System.Drawing.Size(138, 26);
            this.labelSearch.Text = "Search";
            this.labelSearch.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.labelSearch.Click += new System.EventHandler(this.labelSearch_Click);
            // 
            // panel2
            // 
            this.panel2.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
            | System.Windows.Forms.AnchorStyles.Right)));
            this.panel2.BackColor = System.Drawing.Color.Gainsboro;
            this.panel2.Location = new System.Drawing.Point(0, 91);
            this.panel2.Margin = new System.Windows.Forms.Padding(0, 2, 0, 2);
            this.panel2.Size = new System.Drawing.Size(138, 1);
            // 
            // labelSettings
            // 
            this.labelSettings.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
            | System.Windows.Forms.AnchorStyles.Right)));
            this.labelSettings.BorderColor = System.Drawing.Color.Empty;
            this.labelSettings.BorderColorHovered = System.Drawing.Color.Empty;
            this.labelSettings.Location = new System.Drawing.Point(0, 94);
            this.labelSettings.Margin = new System.Windows.Forms.Padding(0);
            this.labelSettings.Padding = new System.Windows.Forms.Padding(11, 0, 0, 0);
            this.labelSettings.Size = new System.Drawing.Size(138, 26);
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
            this.BackColor = System.Drawing.Color.White;
            this.ClientSize = new System.Drawing.Size(140, 185);
            this.Controls.Add(this.panelContainer);
            this.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.Icon = global::Gw2Launcher.Properties.Resources.Gw2Launcher;
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.Manual;
            this.panelContainer.ResumeLayout(false);
            this.panelPage.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private Controls.ArrowButton arrowLeft;
        private Controls.ArrowButton arrowRight;
        private System.Windows.Forms.Label labelPage;
        private MenuItemPanel panelPage;
        private Controls.StackPanel panelContainer;
        private System.Windows.Forms.Label label2;
        private MenuItemLabel labelSettings;
        private System.Windows.Forms.Panel panel2;
        private MenuItemLabel labelSearch;
        private formMenu.MenuItemLabel labelCloseAll;
        private System.Windows.Forms.Panel panelCloseAllSep;
    }
}
