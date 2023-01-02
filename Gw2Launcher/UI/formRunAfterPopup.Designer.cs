namespace Gw2Launcher.UI
{
    partial class formRunAfterPopup
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
            this.components = new System.ComponentModel.Container();
            this.panelContent = new Gw2Launcher.UI.Controls.StackPanel();
            this.labelHeader = new System.Windows.Forms.Label();
            this.labelStatus = new System.Windows.Forms.Label();
            this.panelScroll = new Gw2Launcher.UI.Controls.AutoScrollContainerPanel();
            this.buttonTemplate = new Gw2Launcher.UI.Controls.FlatButton();
            this.contextMenu = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.optionsToolStripMenuItem = new Gw2Launcher.UI.Controls.ToolStripMenuItemStayOpenOnClick();
            this.showIconToolStripMenuItem = new Gw2Launcher.UI.Controls.ToolStripMenuItemStayOpenOnClick();
            this.showProcessExitButtonToolStripMenuItem = new Gw2Launcher.UI.Controls.ToolStripMenuItemStayOpenOnClick();
            this.toolStripMenuItem4 = new System.Windows.Forms.ToolStripSeparator();
            this.keepWindowOpenToolStripMenuItem = new Gw2Launcher.UI.Controls.ToolStripMenuItemStayOpenOnClick();
            this.toolStripMenuItem2 = new System.Windows.Forms.ToolStripSeparator();
            this.sortByToolStripMenuItem = new Gw2Launcher.UI.Controls.ToolStripMenuItemStayOpenOnClick();
            this.activeToolStripMenuItem = new Gw2Launcher.UI.Controls.ToolStripMenuItemStayOpenOnClick();
            this.nameToolStripMenuItem = new Gw2Launcher.UI.Controls.ToolStripMenuItemStayOpenOnClick();
            this.typeToolStripMenuItem = new Gw2Launcher.UI.Controls.ToolStripMenuItemStayOpenOnClick();
            this.toolStripMenuItem1 = new System.Windows.Forms.ToolStripSeparator();
            this.descendingToolStripMenuItem = new Gw2Launcher.UI.Controls.ToolStripMenuItemStayOpenOnClick();
            this.filterByToolStripMenuItem = new Gw2Launcher.UI.Controls.ToolStripMenuItemStayOpenOnClick();
            this.isActiveToolStripMenuItem = new Gw2Launcher.UI.Controls.ToolStripMenuItemStayOpenOnClick();
            this.isNotActiveToolStripMenuItem = new Gw2Launcher.UI.Controls.ToolStripMenuItemStayOpenOnClick();
            this.isStartedManuallyToolStripMenuItem = new Gw2Launcher.UI.Controls.ToolStripMenuItemStayOpenOnClick();
            this.hasNotBeenStartedToolStripMenuItem = new Gw2Launcher.UI.Controls.ToolStripMenuItemStayOpenOnClick();
            this.toolStripMenuItem5 = new System.Windows.Forms.ToolStripSeparator();
            this.isAccountActiveToolStripMenuItem = new Gw2Launcher.UI.Controls.ToolStripMenuItemStayOpenOnClick();
            this.toolStripMenuItem3 = new System.Windows.Forms.ToolStripSeparator();
            this.closeToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.buttonTop = new Gw2Launcher.UI.Controls.FlatButton();
            this.buttonBottom = new Gw2Launcher.UI.Controls.TransparentFlatButton();
            this.panelScroll.SuspendLayout();
            this.contextMenu.SuspendLayout();
            this.SuspendLayout();
            // 
            // panelContent
            // 
            this.panelContent.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.panelContent.AutoSize = true;
            this.panelContent.Location = new System.Drawing.Point(0, 0);
            this.panelContent.Margin = new System.Windows.Forms.Padding(0);
            this.panelContent.MinimumSize = new System.Drawing.Size(0, 20);
            this.panelContent.Size = new System.Drawing.Size(193, 20);
            this.panelContent.MouseClick += new System.Windows.Forms.MouseEventHandler(this.OnMouseClick);
            // 
            // labelHeader
            // 
            this.labelHeader.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.labelHeader.AutoEllipsis = true;
            this.labelHeader.AutoSize = true;
            this.labelHeader.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(32)))), ((int)(((byte)(32)))), ((int)(((byte)(32)))));
            this.labelHeader.Font = new System.Drawing.Font("Segoe UI Semibold", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.labelHeader.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(224)))), ((int)(((byte)(224)))), ((int)(((byte)(224)))));
            this.labelHeader.Location = new System.Drawing.Point(0, 29);
            this.labelHeader.Margin = new System.Windows.Forms.Padding(0, 0, 0, 5);
            this.labelHeader.MinimumSize = new System.Drawing.Size(0, 20);
            this.labelHeader.Padding = new System.Windows.Forms.Padding(4, 4, 0, 4);
            this.labelHeader.Size = new System.Drawing.Size(193, 21);
            this.labelHeader.Text = "header";
            this.labelHeader.Visible = false;
            // 
            // labelStatus
            // 
            this.labelStatus.AutoSize = true;
            this.labelStatus.Font = new System.Drawing.Font("Segoe UI Semilight", 8.25F);
            this.labelStatus.ForeColor = System.Drawing.SystemColors.GrayText;
            this.labelStatus.Location = new System.Drawing.Point(0, 55);
            this.labelStatus.Margin = new System.Windows.Forms.Padding(5, 0, 5, 0);
            this.labelStatus.MinimumSize = new System.Drawing.Size(0, 30);
            this.labelStatus.Size = new System.Drawing.Size(35, 30);
            this.labelStatus.Text = "Status";
            this.labelStatus.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.labelStatus.Visible = false;
            // 
            // panelScroll
            // 
            this.panelScroll.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.panelScroll.Controls.Add(this.labelHeader);
            this.panelScroll.Controls.Add(this.labelStatus);
            this.panelScroll.Controls.Add(this.buttonTemplate);
            this.panelScroll.Controls.Add(this.panelContent);
            this.panelScroll.Location = new System.Drawing.Point(0, 15);
            this.panelScroll.Margin = new System.Windows.Forms.Padding(0, 5, 0, 5);
            this.panelScroll.Size = new System.Drawing.Size(193, 311);
            this.panelScroll.UseFlatScrollBar = true;
            this.panelScroll.MouseClick += new System.Windows.Forms.MouseEventHandler(this.OnMouseClick);
            // 
            // buttonTemplate
            // 
            this.buttonTemplate.BackColor = System.Drawing.Color.Gray;
            this.buttonTemplate.BackColorHovered = System.Drawing.Color.Empty;
            this.buttonTemplate.BackColorSelected = System.Drawing.Color.Empty;
            this.buttonTemplate.BorderColor = System.Drawing.Color.Empty;
            this.buttonTemplate.BorderColorHovered = System.Drawing.Color.Empty;
            this.buttonTemplate.BorderColorSelected = System.Drawing.Color.Empty;
            this.buttonTemplate.ForeColorHovered = System.Drawing.Color.Empty;
            this.buttonTemplate.ForeColorSelected = System.Drawing.Color.Empty;
            this.buttonTemplate.Location = new System.Drawing.Point(-1, 85);
            this.buttonTemplate.Margin = new System.Windows.Forms.Padding(0);
            this.buttonTemplate.Padding = new System.Windows.Forms.Padding(4, 4, 0, 4);
            this.buttonTemplate.Selected = false;
            this.buttonTemplate.Size = new System.Drawing.Size(185, 30);
            this.buttonTemplate.Visible = false;
            // 
            // contextMenu
            // 
            this.contextMenu.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.optionsToolStripMenuItem,
            this.toolStripMenuItem2,
            this.sortByToolStripMenuItem,
            this.filterByToolStripMenuItem,
            this.toolStripMenuItem3,
            this.closeToolStripMenuItem});
            this.contextMenu.Size = new System.Drawing.Size(117, 104);
            // 
            // optionsToolStripMenuItem
            // 
            this.optionsToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.showIconToolStripMenuItem,
            this.showProcessExitButtonToolStripMenuItem,
            this.toolStripMenuItem4,
            this.keepWindowOpenToolStripMenuItem});
            this.optionsToolStripMenuItem.Size = new System.Drawing.Size(116, 22);
            this.optionsToolStripMenuItem.StayOpenOnClick = false;
            this.optionsToolStripMenuItem.Text = "Options";
            // 
            // showIconToolStripMenuItem
            // 
            this.showIconToolStripMenuItem.Size = new System.Drawing.Size(207, 22);
            this.showIconToolStripMenuItem.StayOpenOnClick = true;
            this.showIconToolStripMenuItem.Text = "Show icon";
            this.showIconToolStripMenuItem.Click += new System.EventHandler(this.options_Click);
            // 
            // showProcessExitButtonToolStripMenuItem
            // 
            this.showProcessExitButtonToolStripMenuItem.Size = new System.Drawing.Size(207, 22);
            this.showProcessExitButtonToolStripMenuItem.StayOpenOnClick = true;
            this.showProcessExitButtonToolStripMenuItem.Text = "Show process exit button";
            this.showProcessExitButtonToolStripMenuItem.Click += new System.EventHandler(this.options_Click);
            // 
            // toolStripMenuItem4
            // 
            this.toolStripMenuItem4.Size = new System.Drawing.Size(204, 6);
            // 
            // keepWindowOpenToolStripMenuItem
            // 
            this.keepWindowOpenToolStripMenuItem.Size = new System.Drawing.Size(207, 22);
            this.keepWindowOpenToolStripMenuItem.StayOpenOnClick = true;
            this.keepWindowOpenToolStripMenuItem.Text = "Keep window open";
            this.keepWindowOpenToolStripMenuItem.Click += new System.EventHandler(this.options_Click);
            // 
            // toolStripMenuItem2
            // 
            this.toolStripMenuItem2.Size = new System.Drawing.Size(113, 6);
            // 
            // sortByToolStripMenuItem
            // 
            this.sortByToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.activeToolStripMenuItem,
            this.nameToolStripMenuItem,
            this.typeToolStripMenuItem,
            this.toolStripMenuItem1,
            this.descendingToolStripMenuItem});
            this.sortByToolStripMenuItem.Size = new System.Drawing.Size(116, 22);
            this.sortByToolStripMenuItem.StayOpenOnClick = false;
            this.sortByToolStripMenuItem.Text = "Sort by";
            // 
            // activeToolStripMenuItem
            // 
            this.activeToolStripMenuItem.Size = new System.Drawing.Size(166, 22);
            this.activeToolStripMenuItem.StayOpenOnClick = true;
            this.activeToolStripMenuItem.Text = "Active";
            this.activeToolStripMenuItem.Click += new System.EventHandler(this.sorting_Click);
            // 
            // nameToolStripMenuItem
            // 
            this.nameToolStripMenuItem.Size = new System.Drawing.Size(166, 22);
            this.nameToolStripMenuItem.StayOpenOnClick = true;
            this.nameToolStripMenuItem.Text = "Name";
            this.nameToolStripMenuItem.Click += new System.EventHandler(this.sorting_Click);
            // 
            // typeToolStripMenuItem
            // 
            this.typeToolStripMenuItem.Size = new System.Drawing.Size(166, 22);
            this.typeToolStripMenuItem.StayOpenOnClick = true;
            this.typeToolStripMenuItem.Text = "Startup condition";
            this.typeToolStripMenuItem.Click += new System.EventHandler(this.sorting_Click);
            // 
            // toolStripMenuItem1
            // 
            this.toolStripMenuItem1.Size = new System.Drawing.Size(163, 6);
            // 
            // descendingToolStripMenuItem
            // 
            this.descendingToolStripMenuItem.Size = new System.Drawing.Size(166, 22);
            this.descendingToolStripMenuItem.StayOpenOnClick = true;
            this.descendingToolStripMenuItem.Text = "Descending";
            this.descendingToolStripMenuItem.Click += new System.EventHandler(this.sorting_Click);
            // 
            // filterByToolStripMenuItem
            // 
            this.filterByToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.isActiveToolStripMenuItem,
            this.isNotActiveToolStripMenuItem,
            this.isStartedManuallyToolStripMenuItem,
            this.hasNotBeenStartedToolStripMenuItem,
            this.toolStripMenuItem5,
            this.isAccountActiveToolStripMenuItem});
            this.filterByToolStripMenuItem.Size = new System.Drawing.Size(116, 22);
            this.filterByToolStripMenuItem.StayOpenOnClick = false;
            this.filterByToolStripMenuItem.Text = "Filter by";
            // 
            // isActiveToolStripMenuItem
            // 
            this.isActiveToolStripMenuItem.Size = new System.Drawing.Size(183, 22);
            this.isActiveToolStripMenuItem.StayOpenOnClick = true;
            this.isActiveToolStripMenuItem.Text = "Is active";
            this.isActiveToolStripMenuItem.Click += new System.EventHandler(this.filter_Click);
            // 
            // isNotActiveToolStripMenuItem
            // 
            this.isNotActiveToolStripMenuItem.Size = new System.Drawing.Size(183, 22);
            this.isNotActiveToolStripMenuItem.StayOpenOnClick = true;
            this.isNotActiveToolStripMenuItem.Text = "Is not active";
            this.isNotActiveToolStripMenuItem.Click += new System.EventHandler(this.filter_Click);
            // 
            // isStartedManuallyToolStripMenuItem
            // 
            this.isStartedManuallyToolStripMenuItem.Size = new System.Drawing.Size(183, 22);
            this.isStartedManuallyToolStripMenuItem.StayOpenOnClick = true;
            this.isStartedManuallyToolStripMenuItem.Text = "Is started manually";
            this.isStartedManuallyToolStripMenuItem.Click += new System.EventHandler(this.filter_Click);
            // 
            // hasNotBeenStartedToolStripMenuItem
            // 
            this.hasNotBeenStartedToolStripMenuItem.Size = new System.Drawing.Size(183, 22);
            this.hasNotBeenStartedToolStripMenuItem.StayOpenOnClick = true;
            this.hasNotBeenStartedToolStripMenuItem.Text = "Has not been started";
            this.hasNotBeenStartedToolStripMenuItem.Click += new System.EventHandler(this.filter_Click);
            // 
            // toolStripMenuItem5
            // 
            this.toolStripMenuItem5.Size = new System.Drawing.Size(180, 6);
            // 
            // isAccountActiveToolStripMenuItem
            // 
            this.isAccountActiveToolStripMenuItem.Size = new System.Drawing.Size(183, 22);
            this.isAccountActiveToolStripMenuItem.StayOpenOnClick = true;
            this.isAccountActiveToolStripMenuItem.Text = "Is account active";
            this.isAccountActiveToolStripMenuItem.Click += new System.EventHandler(this.filter_Click);
            // 
            // toolStripMenuItem3
            // 
            this.toolStripMenuItem3.Size = new System.Drawing.Size(113, 6);
            // 
            // closeToolStripMenuItem
            // 
            this.closeToolStripMenuItem.Size = new System.Drawing.Size(116, 22);
            this.closeToolStripMenuItem.Text = "Close";
            this.closeToolStripMenuItem.Click += new System.EventHandler(this.closeToolStripMenuItem_Click);
            // 
            // buttonTop
            // 
            this.buttonTop.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonTop.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(15)))), ((int)(((byte)(15)))), ((int)(((byte)(15)))));
            this.buttonTop.BackColorHovered = System.Drawing.Color.FromArgb(((int)(((byte)(20)))), ((int)(((byte)(20)))), ((int)(((byte)(20)))));
            this.buttonTop.BackColorSelected = System.Drawing.Color.Empty;
            this.buttonTop.BorderColor = System.Drawing.Color.Empty;
            this.buttonTop.BorderColorHovered = System.Drawing.Color.Empty;
            this.buttonTop.BorderColorSelected = System.Drawing.Color.Empty;
            this.buttonTop.ForeColorHovered = System.Drawing.Color.Empty;
            this.buttonTop.ForeColorSelected = System.Drawing.Color.Empty;
            this.buttonTop.Location = new System.Drawing.Point(0, 0);
            this.buttonTop.Margin = new System.Windows.Forms.Padding(0);
            this.buttonTop.Selected = false;
            this.buttonTop.Size = new System.Drawing.Size(193, 10);
            this.buttonTop.MouseClick += new System.Windows.Forms.MouseEventHandler(this.OnMouseClick);
            this.buttonTop.MouseDown += new System.Windows.Forms.MouseEventHandler(this.buttonTop_MouseDown);
            // 
            // buttonBottom
            // 
            this.buttonBottom.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonBottom.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(15)))), ((int)(((byte)(15)))), ((int)(((byte)(15)))));
            this.buttonBottom.BackColorHovered = System.Drawing.Color.FromArgb(((int)(((byte)(20)))), ((int)(((byte)(20)))), ((int)(((byte)(20)))));
            this.buttonBottom.BackColorSelected = System.Drawing.Color.Empty;
            this.buttonBottom.BorderColor = System.Drawing.Color.Empty;
            this.buttonBottom.BorderColorHovered = System.Drawing.Color.Empty;
            this.buttonBottom.BorderColorSelected = System.Drawing.Color.Empty;
            this.buttonBottom.ForeColorHovered = System.Drawing.Color.Empty;
            this.buttonBottom.ForeColorSelected = System.Drawing.Color.Empty;
            this.buttonBottom.Location = new System.Drawing.Point(0, 331);
            this.buttonBottom.Margin = new System.Windows.Forms.Padding(0);
            this.buttonBottom.Selected = false;
            this.buttonBottom.Size = new System.Drawing.Size(193, 10);
            this.buttonBottom.Transparent = true;
            // 
            // formRunAfterPopup
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
            this.BackColor = System.Drawing.Color.Black;
            this.ClientSize = new System.Drawing.Size(193, 341);
            this.Controls.Add(this.buttonBottom);
            this.Controls.Add(this.buttonTop);
            this.Controls.Add(this.panelScroll);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.Opacity = 0D;
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.Manual;
            this.TopMost = true;
            this.MouseClick += new System.Windows.Forms.MouseEventHandler(this.OnMouseClick);
            this.panelScroll.ResumeLayout(false);
            this.panelScroll.PerformLayout();
            this.contextMenu.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private Controls.StackPanel panelContent;
        private System.Windows.Forms.Label labelHeader;
        private Controls.AutoScrollContainerPanel panelScroll;
        private System.Windows.Forms.ContextMenuStrip contextMenu;
        private Controls.ToolStripMenuItemStayOpenOnClick sortByToolStripMenuItem;
        private Controls.ToolStripMenuItemStayOpenOnClick activeToolStripMenuItem;
        private Controls.ToolStripMenuItemStayOpenOnClick nameToolStripMenuItem;
        private Controls.ToolStripMenuItemStayOpenOnClick typeToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripMenuItem1;
        private Controls.ToolStripMenuItemStayOpenOnClick descendingToolStripMenuItem;
        private System.Windows.Forms.Label labelStatus;
        private Controls.FlatButton buttonTop;
        private Controls.TransparentFlatButton buttonBottom;
        private Controls.FlatButton buttonTemplate;
        private System.Windows.Forms.ToolStripSeparator toolStripMenuItem2;
        private System.Windows.Forms.ToolStripSeparator toolStripMenuItem3;
        private System.Windows.Forms.ToolStripMenuItem closeToolStripMenuItem;
        private Controls.ToolStripMenuItemStayOpenOnClick optionsToolStripMenuItem;
        private Controls.ToolStripMenuItemStayOpenOnClick showIconToolStripMenuItem;
        private Controls.ToolStripMenuItemStayOpenOnClick showProcessExitButtonToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripMenuItem4;
        private Controls.ToolStripMenuItemStayOpenOnClick keepWindowOpenToolStripMenuItem;
        private Controls.ToolStripMenuItemStayOpenOnClick filterByToolStripMenuItem;
        private Controls.ToolStripMenuItemStayOpenOnClick isActiveToolStripMenuItem;
        private Controls.ToolStripMenuItemStayOpenOnClick isNotActiveToolStripMenuItem;
        private Controls.ToolStripMenuItemStayOpenOnClick isStartedManuallyToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripMenuItem5;
        private Controls.ToolStripMenuItemStayOpenOnClick isAccountActiveToolStripMenuItem;
        private Controls.ToolStripMenuItemStayOpenOnClick hasNotBeenStartedToolStripMenuItem;
    }
}
