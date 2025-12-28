namespace Gw2Launcher.UI.Dailies
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
            this.components = new System.ComponentModel.Container();
            this.scrollV = new Gw2Launcher.UI.Controls.FlatVScrollBar();
            this.panelContainer = new System.Windows.Forms.Panel();
            this.panelContent = new System.Windows.Forms.Panel();
            this.waitingBounce = new Gw2Launcher.UI.Controls.WaitingBounce();
            this.panelMessage = new Gw2Launcher.UI.Controls.StackPanel();
            this.labelMessage = new System.Windows.Forms.Label();
            this.labelRetry = new System.Windows.Forms.Label();
            this.buttonToday = new Gw2Launcher.UI.Controls.FlatVerticalButton();
            this.buttonTomorrow = new Gw2Launcher.UI.Controls.FlatVerticalButton();
            this.buttonMinimize = new Gw2Launcher.UI.Controls.FlatShapeButton();
            this.contextMenu = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.categoriesToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.favoritesToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.ignoredToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.combineAccountsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.autoSelectCurrentAccountToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.autoScrollToCurrentAccountToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.sortGroupsByToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.focusedToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.groupToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.accountToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem2 = new System.Windows.Forms.ToolStripSeparator();
            this.descendingToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.sortObjectivesByToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.objectiveNameToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.objectiveProgressToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem3 = new System.Windows.Forms.ToolStripSeparator();
            this.objectiveDescendingToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem1 = new System.Windows.Forms.ToolStripSeparator();
            this.showOnTopToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.splitVaultObjectivesToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.refreshDailiesToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.buttonInfoToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.buttonVault = new Gw2Launcher.UI.Controls.FlatVerticalButton();
            this.panelTabs = new Gw2Launcher.UI.Dailies.formDailies.TransparentStackPanel();
            this.buttonDaySwap = new Gw2Launcher.UI.Controls.FlatShapeButton();
            this.panelSep = new Gw2Launcher.UI.Controls.StackPanel();
            this.buttonWeekly = new Gw2Launcher.UI.Controls.FlatVerticalButton();
            this.buttonSpecial = new Gw2Launcher.UI.Controls.FlatVerticalButton();
            this.contextMenuDaily = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.ignoreDailyToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.objectiveIdToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.panelContainer.SuspendLayout();
            this.panelMessage.SuspendLayout();
            this.contextMenu.SuspendLayout();
            this.panelTabs.SuspendLayout();
            this.contextMenuDaily.SuspendLayout();
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
            this.panelContainer.Controls.Add(this.panelMessage);
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
            // panelMessage
            // 
            this.panelMessage.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.panelMessage.Controls.Add(this.labelMessage);
            this.panelMessage.Controls.Add(this.labelRetry);
            this.panelMessage.Location = new System.Drawing.Point(0, 0);
            this.panelMessage.Margin = new System.Windows.Forms.Padding(0);
            this.panelMessage.Size = new System.Drawing.Size(387, 418);
            this.panelMessage.Visible = false;
            // 
            // labelMessage
            // 
            this.labelMessage.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.labelMessage.Location = new System.Drawing.Point(3, 0);
            this.labelMessage.Size = new System.Drawing.Size(381, 400);
            this.labelMessage.Text = "[message]";
            this.labelMessage.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // labelRetry
            // 
            this.labelRetry.Anchor = System.Windows.Forms.AnchorStyles.Bottom;
            this.labelRetry.AutoSize = true;
            this.labelRetry.Font = new System.Drawing.Font("Segoe UI Semilight", 8.25F);
            this.labelRetry.Location = new System.Drawing.Point(193, 400);
            this.labelRetry.Margin = new System.Windows.Forms.Padding(3, 0, 3, 5);
            this.labelRetry.Size = new System.Drawing.Size(0, 13);
            // 
            // buttonToday
            // 
            this.buttonToday.Alignment = System.Windows.Forms.HorizontalAlignment.Center;
            this.buttonToday.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonToday.AutoSize = true;
            this.buttonToday.BackColorHoveredName = Gw2Launcher.UI.UiColors.Colors.DailiesHeader;
            this.buttonToday.BackColorSelectedName = Gw2Launcher.UI.UiColors.Colors.DailiesHeader;
            this.buttonToday.Location = new System.Drawing.Point(0, 0);
            this.buttonToday.Margin = new System.Windows.Forms.Padding(0);
            this.buttonToday.Padding = new System.Windows.Forms.Padding(0, 5, 0, 5);
            this.buttonToday.Size = new System.Drawing.Size(30, 68);
            this.buttonToday.Text = "Today";
            this.buttonToday.SelectedChanged += new System.EventHandler(this.buttonToday_SelectedChanged);
            this.buttonToday.MouseEnteredChanged += new System.EventHandler(this.buttonToday_MouseEnteredChanged);
            this.buttonToday.MouseDown += new System.Windows.Forms.MouseEventHandler(this.buttonToday_MouseDown);
            // 
            // buttonTomorrow
            // 
            this.buttonTomorrow.Alignment = System.Windows.Forms.HorizontalAlignment.Center;
            this.buttonTomorrow.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonTomorrow.AutoSize = true;
            this.buttonTomorrow.BackColorHoveredName = Gw2Launcher.UI.UiColors.Colors.DailiesHeader;
            this.buttonTomorrow.BackColorSelectedName = Gw2Launcher.UI.UiColors.Colors.DailiesHeader;
            this.buttonTomorrow.Location = new System.Drawing.Point(0, 68);
            this.buttonTomorrow.Margin = new System.Windows.Forms.Padding(0);
            this.buttonTomorrow.Padding = new System.Windows.Forms.Padding(0, 5, 0, 5);
            this.buttonTomorrow.Size = new System.Drawing.Size(30, 91);
            this.buttonTomorrow.Text = "Tomorrow";
            this.buttonTomorrow.Visible = false;
            this.buttonTomorrow.SelectedChanged += new System.EventHandler(this.buttonToday_SelectedChanged);
            this.buttonTomorrow.MouseEnteredChanged += new System.EventHandler(this.buttonToday_MouseEnteredChanged);
            this.buttonTomorrow.SizeChanged += new System.EventHandler(this.buttonTomorrow_SizeChanged);
            this.buttonTomorrow.MouseDown += new System.Windows.Forms.MouseEventHandler(this.buttonTomorrow_MouseDown);
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
            // contextMenu
            // 
            this.contextMenu.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.categoriesToolStripMenuItem,
            this.favoritesToolStripMenuItem,
            this.ignoredToolStripMenuItem,
            this.combineAccountsToolStripMenuItem,
            this.autoSelectCurrentAccountToolStripMenuItem,
            this.autoScrollToCurrentAccountToolStripMenuItem,
            this.sortGroupsByToolStripMenuItem,
            this.sortObjectivesByToolStripMenuItem,
            this.toolStripMenuItem1,
            this.showOnTopToolStripMenuItem,
            this.splitVaultObjectivesToolStripMenuItem,
            this.refreshDailiesToolStripMenuItem,
            this.buttonInfoToolStripMenuItem});
            this.contextMenu.Size = new System.Drawing.Size(308, 274);
            // 
            // categoriesToolStripMenuItem
            // 
            this.categoriesToolStripMenuItem.Size = new System.Drawing.Size(307, 22);
            this.categoriesToolStripMenuItem.Text = "Categories";
            this.categoriesToolStripMenuItem.Click += new System.EventHandler(this.categoriesToolStripMenuItem_Click);
            // 
            // favoritesToolStripMenuItem
            // 
            this.favoritesToolStripMenuItem.Size = new System.Drawing.Size(307, 22);
            this.favoritesToolStripMenuItem.Text = "Favorites";
            this.favoritesToolStripMenuItem.Click += new System.EventHandler(this.favoritesToolStripMenuItem_Click);
            // 
            // ignoredToolStripMenuItem
            // 
            this.ignoredToolStripMenuItem.Size = new System.Drawing.Size(307, 22);
            this.ignoredToolStripMenuItem.Text = "Ignored";
            this.ignoredToolStripMenuItem.Click += new System.EventHandler(this.ignoredToolStripMenuItem_Click);
            // 
            // combineAccountsToolStripMenuItem
            // 
            this.combineAccountsToolStripMenuItem.Checked = true;
            this.combineAccountsToolStripMenuItem.CheckState = System.Windows.Forms.CheckState.Checked;
            this.combineAccountsToolStripMenuItem.Size = new System.Drawing.Size(307, 22);
            this.combineAccountsToolStripMenuItem.Text = "Combine accounts with the same objectives";
            this.combineAccountsToolStripMenuItem.Click += new System.EventHandler(this.combineAccountsToolStripMenuItem_Click);
            // 
            // autoSelectCurrentAccountToolStripMenuItem
            // 
            this.autoSelectCurrentAccountToolStripMenuItem.Size = new System.Drawing.Size(307, 22);
            this.autoSelectCurrentAccountToolStripMenuItem.Text = "Auto select current account";
            this.autoSelectCurrentAccountToolStripMenuItem.Click += new System.EventHandler(this.autoSelectCurrentAccountToolStripMenuItem_Click);
            // 
            // autoScrollToCurrentAccountToolStripMenuItem
            // 
            this.autoScrollToCurrentAccountToolStripMenuItem.Size = new System.Drawing.Size(307, 22);
            this.autoScrollToCurrentAccountToolStripMenuItem.Text = "Auto scroll to current account";
            this.autoScrollToCurrentAccountToolStripMenuItem.Click += new System.EventHandler(this.autoScrollToCurrentAccountToolStripMenuItem_Click);
            // 
            // sortGroupsByToolStripMenuItem
            // 
            this.sortGroupsByToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.focusedToolStripMenuItem,
            this.groupToolStripMenuItem,
            this.accountToolStripMenuItem,
            this.toolStripMenuItem2,
            this.descendingToolStripMenuItem});
            this.sortGroupsByToolStripMenuItem.Size = new System.Drawing.Size(307, 22);
            this.sortGroupsByToolStripMenuItem.Text = "Sort groups by";
            // 
            // focusedToolStripMenuItem
            // 
            this.focusedToolStripMenuItem.Size = new System.Drawing.Size(152, 22);
            this.focusedToolStripMenuItem.Text = "Focused";
            this.focusedToolStripMenuItem.Click += new System.EventHandler(this.sortingGroupsToolStripMenuItem_Click);
            // 
            // groupToolStripMenuItem
            // 
            this.groupToolStripMenuItem.Size = new System.Drawing.Size(152, 22);
            this.groupToolStripMenuItem.Text = "Group";
            this.groupToolStripMenuItem.Click += new System.EventHandler(this.sortingGroupsToolStripMenuItem_Click);
            // 
            // accountToolStripMenuItem
            // 
            this.accountToolStripMenuItem.Size = new System.Drawing.Size(152, 22);
            this.accountToolStripMenuItem.Text = "Account";
            this.accountToolStripMenuItem.Click += new System.EventHandler(this.sortingGroupsToolStripMenuItem_Click);
            // 
            // toolStripMenuItem2
            // 
            this.toolStripMenuItem2.Size = new System.Drawing.Size(149, 6);
            // 
            // descendingToolStripMenuItem
            // 
            this.descendingToolStripMenuItem.Size = new System.Drawing.Size(152, 22);
            this.descendingToolStripMenuItem.Text = "Descending";
            this.descendingToolStripMenuItem.Click += new System.EventHandler(this.sortingGroupsToolStripMenuItem_Click);
            // 
            // sortObjectivesByToolStripMenuItem
            // 
            this.sortObjectivesByToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.objectiveIdToolStripMenuItem,
            this.objectiveNameToolStripMenuItem,
            this.objectiveProgressToolStripMenuItem,
            this.toolStripMenuItem3,
            this.objectiveDescendingToolStripMenuItem});
            this.sortObjectivesByToolStripMenuItem.Size = new System.Drawing.Size(307, 22);
            this.sortObjectivesByToolStripMenuItem.Text = "Sort objectives by";
            // 
            // objectiveNameToolStripMenuItem
            // 
            this.objectiveNameToolStripMenuItem.Size = new System.Drawing.Size(152, 22);
            this.objectiveNameToolStripMenuItem.Text = "Name";
            this.objectiveNameToolStripMenuItem.Click += new System.EventHandler(this.sortingObjectivesToolStripMenuItem_Click);
            // 
            // objectiveProgressToolStripMenuItem
            // 
            this.objectiveProgressToolStripMenuItem.Size = new System.Drawing.Size(152, 22);
            this.objectiveProgressToolStripMenuItem.Text = "Progress";
            this.objectiveProgressToolStripMenuItem.Click += new System.EventHandler(this.sortingObjectivesToolStripMenuItem_Click);
            // 
            // toolStripMenuItem3
            // 
            this.toolStripMenuItem3.Size = new System.Drawing.Size(149, 6);
            // 
            // objectiveDescendingToolStripMenuItem
            // 
            this.objectiveDescendingToolStripMenuItem.Size = new System.Drawing.Size(152, 22);
            this.objectiveDescendingToolStripMenuItem.Text = "Descending";
            this.objectiveDescendingToolStripMenuItem.Click += new System.EventHandler(this.sortingObjectivesToolStripMenuItem_Click);
            // 
            // toolStripMenuItem1
            // 
            this.toolStripMenuItem1.Size = new System.Drawing.Size(304, 6);
            // 
            // showOnTopToolStripMenuItem
            // 
            this.showOnTopToolStripMenuItem.Size = new System.Drawing.Size(307, 22);
            this.showOnTopToolStripMenuItem.Text = "Show on top of other windows";
            this.showOnTopToolStripMenuItem.Click += new System.EventHandler(this.showOnTopToolStripMenuItem_Click);
            // 
            // splitVaultObjectivesToolStripMenuItem
            // 
            this.splitVaultObjectivesToolStripMenuItem.Size = new System.Drawing.Size(307, 22);
            this.splitVaultObjectivesToolStripMenuItem.Text = "Split vault objectives";
            this.splitVaultObjectivesToolStripMenuItem.Click += new System.EventHandler(this.splitVaultObjectivesToolStripMenuItem_Click);
            // 
            // refreshDailiesToolStripMenuItem
            // 
            this.refreshDailiesToolStripMenuItem.Size = new System.Drawing.Size(307, 22);
            this.refreshDailiesToolStripMenuItem.Text = "Refresh dailies";
            this.refreshDailiesToolStripMenuItem.Click += new System.EventHandler(this.refreshDailiesToolStripMenuItem_Click);
            // 
            // buttonInfoToolStripMenuItem
            // 
            this.buttonInfoToolStripMenuItem.Size = new System.Drawing.Size(307, 22);
            this.buttonInfoToolStripMenuItem.Text = "Button info";
            this.buttonInfoToolStripMenuItem.Click += new System.EventHandler(this.buttonInfoToolStripMenuItem_Click);
            // 
            // buttonVault
            // 
            this.buttonVault.Alignment = System.Windows.Forms.HorizontalAlignment.Center;
            this.buttonVault.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonVault.AutoSize = true;
            this.buttonVault.BackColorHoveredName = Gw2Launcher.UI.UiColors.Colors.DailiesHeader;
            this.buttonVault.BackColorSelectedName = Gw2Launcher.UI.UiColors.Colors.DailiesHeader;
            this.buttonVault.Location = new System.Drawing.Point(0, 193);
            this.buttonVault.Margin = new System.Windows.Forms.Padding(0, 0, 0, 2);
            this.buttonVault.Padding = new System.Windows.Forms.Padding(0, 5, 0, 5);
            this.buttonVault.Size = new System.Drawing.Size(30, 63);
            this.buttonVault.Text = "Vault";
            this.buttonVault.MouseDown += new System.Windows.Forms.MouseEventHandler(this.buttonVault_MouseDown);
            // 
            // panelTabs
            // 
            this.panelTabs.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.panelTabs.Controls.Add(this.buttonToday);
            this.panelTabs.Controls.Add(this.buttonTomorrow);
            this.panelTabs.Controls.Add(this.buttonDaySwap);
            this.panelTabs.Controls.Add(this.panelSep);
            this.panelTabs.Controls.Add(this.buttonVault);
            this.panelTabs.Controls.Add(this.buttonWeekly);
            this.panelTabs.Controls.Add(this.buttonSpecial);
            this.panelTabs.Location = new System.Drawing.Point(394, 1);
            this.panelTabs.Size = new System.Drawing.Size(30, 392);
            // 
            // buttonDaySwap
            // 
            this.buttonDaySwap.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonDaySwap.BackColorHoveredName = Gw2Launcher.UI.UiColors.Colors.DailiesHeader;
            this.buttonDaySwap.BackColorSelectedName = Gw2Launcher.UI.UiColors.Colors.DailiesHeader;
            this.buttonDaySwap.DisableMouseEnterHover = true;
            this.buttonDaySwap.Location = new System.Drawing.Point(0, 159);
            this.buttonDaySwap.Margin = new System.Windows.Forms.Padding(0);
            this.buttonDaySwap.Padding = new System.Windows.Forms.Padding(5);
            this.buttonDaySwap.ShapeDirection = System.Windows.Forms.ArrowDirection.Right;
            this.buttonDaySwap.ShapeSize = new System.Drawing.Size(4, 8);
            this.buttonDaySwap.Size = new System.Drawing.Size(30, 20);
            this.buttonDaySwap.MouseEnteredChanged += new System.EventHandler(this.buttonDaySwap_MouseEnteredChanged);
            this.buttonDaySwap.Click += new System.EventHandler(this.buttonDaySwap_Click);
            // 
            // panelSep
            // 
            this.panelSep.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.panelSep.BackColorName = Gw2Launcher.UI.UiColors.Colors.DailiesHeader;
            this.panelSep.Location = new System.Drawing.Point(0, 184);
            this.panelSep.Margin = new System.Windows.Forms.Padding(0, 5, 0, 5);
            this.panelSep.Size = new System.Drawing.Size(30, 4);
            // 
            // buttonWeekly
            // 
            this.buttonWeekly.Alignment = System.Windows.Forms.HorizontalAlignment.Center;
            this.buttonWeekly.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonWeekly.AutoSize = true;
            this.buttonWeekly.BackColorHoveredName = Gw2Launcher.UI.UiColors.Colors.DailiesHeader;
            this.buttonWeekly.BackColorSelectedName = Gw2Launcher.UI.UiColors.Colors.DailiesHeader;
            this.buttonWeekly.Location = new System.Drawing.Point(0, 258);
            this.buttonWeekly.Margin = new System.Windows.Forms.Padding(0, 0, 0, 2);
            this.buttonWeekly.Padding = new System.Windows.Forms.Padding(0, 5, 0, 5);
            this.buttonWeekly.Size = new System.Drawing.Size(30, 75);
            this.buttonWeekly.Text = "Weekly";
            this.buttonWeekly.MouseDown += new System.Windows.Forms.MouseEventHandler(this.buttonVault_MouseDown);
            // 
            // buttonSpecial
            // 
            this.buttonSpecial.Alignment = System.Windows.Forms.HorizontalAlignment.Center;
            this.buttonSpecial.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonSpecial.AutoSize = true;
            this.buttonSpecial.BackColorHoveredName = Gw2Launcher.UI.UiColors.Colors.DailiesHeader;
            this.buttonSpecial.BackColorSelectedName = Gw2Launcher.UI.UiColors.Colors.DailiesHeader;
            this.buttonSpecial.Location = new System.Drawing.Point(0, 335);
            this.buttonSpecial.Margin = new System.Windows.Forms.Padding(0);
            this.buttonSpecial.Padding = new System.Windows.Forms.Padding(0, 5, 0, 5);
            this.buttonSpecial.Size = new System.Drawing.Size(30, 74);
            this.buttonSpecial.Text = "Special";
            this.buttonSpecial.MouseDown += new System.Windows.Forms.MouseEventHandler(this.buttonVault_MouseDown);
            // 
            // contextMenuDaily
            // 
            this.contextMenuDaily.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.ignoreDailyToolStripMenuItem});
            this.contextMenuDaily.Size = new System.Drawing.Size(109, 26);
            // 
            // ignoreDailyToolStripMenuItem
            // 
            this.ignoreDailyToolStripMenuItem.Size = new System.Drawing.Size(108, 22);
            this.ignoreDailyToolStripMenuItem.Text = "Ignore";
            this.ignoreDailyToolStripMenuItem.Click += new System.EventHandler(this.ignoreDailyToolStripMenuItem_Click);
            // 
            // objectiveIdToolStripMenuItem
            // 
            this.objectiveIdToolStripMenuItem.Size = new System.Drawing.Size(152, 22);
            this.objectiveIdToolStripMenuItem.Text = "ID";
            this.objectiveIdToolStripMenuItem.Click += new System.EventHandler(this.sortingObjectivesToolStripMenuItem_Click);
            // 
            // formDailies
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
            this.BackColorName = Gw2Launcher.UI.UiColors.Colors.DailiesBackColor;
            this.ClientSize = new System.Drawing.Size(425, 420);
            this.Controls.Add(this.buttonMinimize);
            this.Controls.Add(this.panelContainer);
            this.Controls.Add(this.scrollV);
            this.Controls.Add(this.panelTabs);
            this.DoubleBuffered = true;
            this.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.ForeColorName = Gw2Launcher.UI.UiColors.Colors.DailiesText;
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.Icon = global::Gw2Launcher.Properties.Resources.Gw2Launcher;
            this.MinimumSize = new System.Drawing.Size(330, 340);
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.Manual;
            this.panelContainer.ResumeLayout(false);
            this.panelMessage.ResumeLayout(false);
            this.panelMessage.PerformLayout();
            this.contextMenu.ResumeLayout(false);
            this.panelTabs.ResumeLayout(false);
            this.panelTabs.PerformLayout();
            this.contextMenuDaily.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private Controls.FlatVScrollBar scrollV;
        private System.Windows.Forms.Panel panelContainer;
        private System.Windows.Forms.Panel panelContent;
        private Controls.WaitingBounce waitingBounce;
        private Controls.FlatVerticalButton buttonToday;
        private Controls.FlatVerticalButton buttonTomorrow;
        private Controls.FlatShapeButton buttonMinimize;
        private Controls.StackPanel panelMessage;
        private System.Windows.Forms.Label labelMessage;
        private System.Windows.Forms.Label labelRetry;
        private System.Windows.Forms.ContextMenuStrip contextMenu;
        private System.Windows.Forms.ToolStripMenuItem categoriesToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem favoritesToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripMenuItem1;
        private System.Windows.Forms.ToolStripMenuItem showOnTopToolStripMenuItem;
        private Controls.FlatVerticalButton buttonVault;
        private TransparentStackPanel panelTabs;
        private Controls.StackPanel panelSep;
        private Controls.FlatVerticalButton buttonWeekly;
        private Controls.FlatVerticalButton buttonSpecial;
        private Controls.FlatShapeButton buttonDaySwap;
        private System.Windows.Forms.ToolStripMenuItem ignoredToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem refreshDailiesToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem buttonInfoToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem combineAccountsToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem splitVaultObjectivesToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem autoSelectCurrentAccountToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem autoScrollToCurrentAccountToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem sortGroupsByToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem groupToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem focusedToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem accountToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripMenuItem2;
        private System.Windows.Forms.ToolStripMenuItem descendingToolStripMenuItem;
        private System.Windows.Forms.ContextMenuStrip contextMenuDaily;
        private System.Windows.Forms.ToolStripMenuItem ignoreDailyToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem sortObjectivesByToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem objectiveNameToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem objectiveProgressToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripMenuItem3;
        private System.Windows.Forms.ToolStripMenuItem objectiveDescendingToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem objectiveIdToolStripMenuItem;
    }
}
