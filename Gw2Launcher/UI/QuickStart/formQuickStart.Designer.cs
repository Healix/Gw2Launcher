namespace Gw2Launcher.UI.QuickStart
{
    partial class formQuickStart
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
            this.components = new System.ComponentModel.Container();
            this.buttonBack = new System.Windows.Forms.Button();
            this.buttonOK = new System.Windows.Forms.Button();
            this.panelAccounts = new Gw2Launcher.UI.Controls.StackPanel();
            this.label3 = new System.Windows.Forms.Label();
            this.label5 = new System.Windows.Forms.Label();
            this.autoScrollContainerPanel1 = new Gw2Launcher.UI.Controls.AutoScrollContainerPanel();
            this.stackPanel1 = new Gw2Launcher.UI.Controls.StackPanel();
            this.panelAccountsContent = new Gw2Launcher.UI.Controls.StackPanel();
            this.panelAccount = new Gw2Launcher.UI.Controls.StackPanel();
            this.textName = new System.Windows.Forms.TextBox();
            this.buttonOptions = new Gw2Launcher.UI.Controls.FlatShapeButton();
            this.labelAdd = new Controls.LinkLabel();
            this.buttonNext = new System.Windows.Forms.Button();
            this.contextAccount = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.editToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.removeToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.panelAddons = new Gw2Launcher.UI.Controls.StackPanel();
            this.label6 = new System.Windows.Forms.Label();
            this.label11 = new System.Windows.Forms.Label();
            this.radioAddonsNo = new System.Windows.Forms.RadioButton();
            this.radioAddonsAuto = new System.Windows.Forms.RadioButton();
            this.radioAddonsManual = new System.Windows.Forms.RadioButton();
            this.panelDirectUpdates = new Gw2Launcher.UI.Controls.StackPanel();
            this.label8 = new System.Windows.Forms.Label();
            this.label9 = new System.Windows.Forms.Label();
            this.radioDirectUpdateNo = new System.Windows.Forms.RadioButton();
            this.radioDirectUpdateYes = new System.Windows.Forms.RadioButton();
            this.panelReady = new Gw2Launcher.UI.Controls.StackPanel();
            this.label10 = new System.Windows.Forms.Label();
            this.label12 = new System.Windows.Forms.Label();
            this.labelMessage = new System.Windows.Forms.Label();
            this.panelStart = new Gw2Launcher.UI.Controls.StackPanel();
            this.label13 = new System.Windows.Forms.Label();
            this.label14 = new System.Windows.Forms.Label();
            this.label16 = new System.Windows.Forms.Label();
            this.label15 = new System.Windows.Forms.Label();
            this.buttonCancel = new System.Windows.Forms.Button();
            this.panelGraphics = new Gw2Launcher.UI.Controls.StackPanel();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.radioGraphicsCustom = new System.Windows.Forms.RadioButton();
            this.radioGraphicsShared = new System.Windows.Forms.RadioButton();
            this.panelLogin = new Gw2Launcher.UI.Controls.StackPanel();
            this.label4 = new System.Windows.Forms.Label();
            this.label7 = new System.Windows.Forms.Label();
            this.radioLoginRemembered = new System.Windows.Forms.RadioButton();
            this.radioLoginManual = new System.Windows.Forms.RadioButton();
            this.panelExe = new Gw2Launcher.UI.Controls.StackPanel();
            this.labelGw2ExeTitle = new System.Windows.Forms.Label();
            this.label18 = new System.Windows.Forms.Label();
            this.stackPanel46 = new Gw2Launcher.UI.Controls.StackPanel();
            this.textGw2Path = new System.Windows.Forms.TextBox();
            this.buttonGw2Path = new System.Windows.Forms.Button();
            this.panelAccounts.SuspendLayout();
            this.autoScrollContainerPanel1.SuspendLayout();
            this.stackPanel1.SuspendLayout();
            this.panelAccountsContent.SuspendLayout();
            this.panelAccount.SuspendLayout();
            this.contextAccount.SuspendLayout();
            this.panelAddons.SuspendLayout();
            this.panelDirectUpdates.SuspendLayout();
            this.panelReady.SuspendLayout();
            this.panelStart.SuspendLayout();
            this.panelGraphics.SuspendLayout();
            this.panelLogin.SuspendLayout();
            this.panelExe.SuspendLayout();
            this.stackPanel46.SuspendLayout();
            this.SuspendLayout();
            // 
            // buttonBack
            // 
            this.buttonBack.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonBack.Font = new System.Drawing.Font("Segoe UI", 8.25F);
            this.buttonBack.Location = new System.Drawing.Point(190, 192);
            this.buttonBack.Size = new System.Drawing.Size(86, 35);
            this.buttonBack.Text = "Back";
            this.buttonBack.UseVisualStyleBackColor = true;
            this.buttonBack.Visible = false;
            this.buttonBack.Click += new System.EventHandler(this.buttonBack_Click);
            // 
            // buttonOK
            // 
            this.buttonOK.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonOK.Font = new System.Drawing.Font("Segoe UI", 8.25F);
            this.buttonOK.Location = new System.Drawing.Point(286, 192);
            this.buttonOK.Size = new System.Drawing.Size(86, 35);
            this.buttonOK.Text = "OK";
            this.buttonOK.UseVisualStyleBackColor = true;
            this.buttonOK.Visible = false;
            this.buttonOK.Click += new System.EventHandler(this.buttonOK_Click);
            // 
            // panelAccounts
            // 
            this.panelAccounts.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
            | System.Windows.Forms.AnchorStyles.Left)
            | System.Windows.Forms.AnchorStyles.Right)));
            this.panelAccounts.Controls.Add(this.label3);
            this.panelAccounts.Controls.Add(this.label5);
            this.panelAccounts.Controls.Add(this.autoScrollContainerPanel1);
            this.panelAccounts.Location = new System.Drawing.Point(6, 6);
            this.panelAccounts.Margin = new System.Windows.Forms.Padding(0);
            this.panelAccounts.Padding = new System.Windows.Forms.Padding(13, 10, 0, 0);
            this.panelAccounts.Size = new System.Drawing.Size(372, 170);
            this.panelAccounts.Visible = false;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Font = new System.Drawing.Font("Segoe UI Semibold", 9.75F, System.Drawing.FontStyle.Bold);
            this.label3.Location = new System.Drawing.Point(13, 10);
            this.label3.Margin = new System.Windows.Forms.Padding(0, 0, 0, 3);
            this.label3.Size = new System.Drawing.Size(64, 17);
            this.label3.Text = "Accounts";
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Font = new System.Drawing.Font("Segoe UI Semilight", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label5.Location = new System.Drawing.Point(14, 31);
            this.label5.Margin = new System.Windows.Forms.Padding(1, 1, 0, 13);
            this.label5.Size = new System.Drawing.Size(181, 13);
            this.label5.Text = "Create accounts using default options";
            // 
            // autoScrollContainerPanel1
            // 
            this.autoScrollContainerPanel1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
            | System.Windows.Forms.AnchorStyles.Left)
            | System.Windows.Forms.AnchorStyles.Right)));
            this.autoScrollContainerPanel1.Controls.Add(this.stackPanel1);
            this.autoScrollContainerPanel1.Location = new System.Drawing.Point(13, 57);
            this.autoScrollContainerPanel1.Margin = new System.Windows.Forms.Padding(0);
            this.autoScrollContainerPanel1.Size = new System.Drawing.Size(359, 113);
            // 
            // stackPanel1
            // 
            this.stackPanel1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
            | System.Windows.Forms.AnchorStyles.Right)));
            this.stackPanel1.AutoSize = true;
            this.stackPanel1.AutoSizeFill = Gw2Launcher.UI.Controls.StackPanel.AutoSizeFillMode.Width;
            this.stackPanel1.Controls.Add(this.panelAccountsContent);
            this.stackPanel1.Controls.Add(this.labelAdd);
            this.stackPanel1.Location = new System.Drawing.Point(0, 0);
            this.stackPanel1.Margin = new System.Windows.Forms.Padding(0);
            this.stackPanel1.Size = new System.Drawing.Size(359, 48);
            // 
            // panelAccountsContent
            // 
            this.panelAccountsContent.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
            | System.Windows.Forms.AnchorStyles.Right)));
            this.panelAccountsContent.AutoSize = true;
            this.panelAccountsContent.Controls.Add(this.panelAccount);
            this.panelAccountsContent.Location = new System.Drawing.Point(8, 0);
            this.panelAccountsContent.Margin = new System.Windows.Forms.Padding(8, 0, 8, 0);
            this.panelAccountsContent.Size = new System.Drawing.Size(343, 25);
            // 
            // panelAccount
            // 
            this.panelAccount.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
            | System.Windows.Forms.AnchorStyles.Right)));
            this.panelAccount.Controls.Add(this.textName);
            this.panelAccount.Controls.Add(this.buttonOptions);
            this.panelAccount.FlowDirection = System.Windows.Forms.FlowDirection.LeftToRight;
            this.panelAccount.Location = new System.Drawing.Point(0, 0);
            this.panelAccount.Margin = new System.Windows.Forms.Padding(0, 0, 0, 3);
            this.panelAccount.Size = new System.Drawing.Size(343, 22);
            this.panelAccount.Visible = false;
            // 
            // textName
            // 
            this.textName.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.textName.Location = new System.Drawing.Point(0, 0);
            this.textName.Margin = new System.Windows.Forms.Padding(0);
            this.textName.Size = new System.Drawing.Size(323, 22);
            // 
            // buttonOptions
            // 
            this.buttonOptions.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
            | System.Windows.Forms.AnchorStyles.Left)));
            this.buttonOptions.BackColorHovered = System.Drawing.SystemColors.Control;
            this.buttonOptions.BackColorSelected = System.Drawing.SystemColors.Control;
            this.buttonOptions.BorderColor = System.Drawing.Color.Empty;
            this.buttonOptions.Cursor = Windows.Cursors.Hand;
            this.buttonOptions.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(100)))), ((int)(((byte)(100)))), ((int)(((byte)(100)))));
            this.buttonOptions.ForeColorHovered = System.Drawing.Color.Black;
            this.buttonOptions.ForeColorSelected = System.Drawing.Color.FromArgb(((int)(((byte)(100)))), ((int)(((byte)(100)))), ((int)(((byte)(100)))));
            this.buttonOptions.LineSize = 1;
            this.buttonOptions.Location = new System.Drawing.Point(326, 3);
            this.buttonOptions.Selected = false;
            this.buttonOptions.Shape = Gw2Launcher.UI.Controls.FlatShapeButton.IconShape.MenuLines;
            this.buttonOptions.ShapeSize = new System.Drawing.Size(8, 8);
            this.buttonOptions.Size = new System.Drawing.Size(14, 16);
            this.buttonOptions.Click += new System.EventHandler(this.buttonOptions_Click);
            // 
            // labelAdd
            // 
            this.labelAdd.Anchor = System.Windows.Forms.AnchorStyles.Top;
            this.labelAdd.AutoSize = true;
            this.labelAdd.Location = new System.Drawing.Point(144, 35);
            this.labelAdd.Margin = new System.Windows.Forms.Padding(0, 10, 0, 0);
            this.labelAdd.Size = new System.Drawing.Size(71, 13);
            this.labelAdd.Text = "add account";
            this.labelAdd.Click += new System.EventHandler(this.labelAdd_Click);
            // 
            // buttonNext
            // 
            this.buttonNext.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonNext.Font = new System.Drawing.Font("Segoe UI", 8.25F);
            this.buttonNext.Location = new System.Drawing.Point(286, 192);
            this.buttonNext.Size = new System.Drawing.Size(86, 35);
            this.buttonNext.Text = "Next";
            this.buttonNext.UseVisualStyleBackColor = true;
            this.buttonNext.Visible = false;
            this.buttonNext.Click += new System.EventHandler(this.buttonNext_Click);
            // 
            // contextAccount
            // 
            this.contextAccount.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.editToolStripMenuItem,
            this.removeToolStripMenuItem});
            this.contextAccount.Size = new System.Drawing.Size(118, 48);
            // 
            // editToolStripMenuItem
            // 
            this.editToolStripMenuItem.Size = new System.Drawing.Size(117, 22);
            this.editToolStripMenuItem.Text = "Edit";
            this.editToolStripMenuItem.Click += new System.EventHandler(this.editToolStripMenuItem_Click);
            // 
            // removeToolStripMenuItem
            // 
            this.removeToolStripMenuItem.Size = new System.Drawing.Size(117, 22);
            this.removeToolStripMenuItem.Text = "Remove";
            this.removeToolStripMenuItem.Click += new System.EventHandler(this.removeToolStripMenuItem_Click);
            // 
            // panelAddons
            // 
            this.panelAddons.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
            | System.Windows.Forms.AnchorStyles.Left)
            | System.Windows.Forms.AnchorStyles.Right)));
            this.panelAddons.Controls.Add(this.label6);
            this.panelAddons.Controls.Add(this.label11);
            this.panelAddons.Controls.Add(this.radioAddonsNo);
            this.panelAddons.Controls.Add(this.radioAddonsAuto);
            this.panelAddons.Controls.Add(this.radioAddonsManual);
            this.panelAddons.Location = new System.Drawing.Point(6, 6);
            this.panelAddons.Margin = new System.Windows.Forms.Padding(0);
            this.panelAddons.Padding = new System.Windows.Forms.Padding(13, 10, 13, 0);
            this.panelAddons.Size = new System.Drawing.Size(372, 170);
            this.panelAddons.Visible = false;
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Font = new System.Drawing.Font("Segoe UI Semibold", 9.75F, System.Drawing.FontStyle.Bold);
            this.label6.Location = new System.Drawing.Point(13, 10);
            this.label6.Margin = new System.Windows.Forms.Padding(0, 0, 0, 3);
            this.label6.Size = new System.Drawing.Size(183, 17);
            this.label6.Text = "Do you want to use addons?";
            // 
            // label11
            // 
            this.label11.AutoSize = true;
            this.label11.Font = new System.Drawing.Font("Segoe UI Semilight", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label11.Location = new System.Drawing.Point(14, 31);
            this.label11.Margin = new System.Windows.Forms.Padding(1, 1, 0, 13);
            this.label11.Size = new System.Drawing.Size(261, 13);
            this.label11.Text = "Each account can have its own customizable bin folder";
            // 
            // radioAddonsNo
            // 
            this.radioAddonsNo.AutoSize = true;
            this.radioAddonsNo.Checked = true;
            this.radioAddonsNo.Location = new System.Drawing.Point(21, 57);
            this.radioAddonsNo.Margin = new System.Windows.Forms.Padding(8, 0, 0, 0);
            this.radioAddonsNo.Size = new System.Drawing.Size(40, 17);
            this.radioAddonsNo.TabStop = true;
            this.radioAddonsNo.Text = "No";
            this.radioAddonsNo.UseVisualStyleBackColor = true;
            // 
            // radioAddonsAuto
            // 
            this.radioAddonsAuto.AutoSize = true;
            this.radioAddonsAuto.Location = new System.Drawing.Point(21, 77);
            this.radioAddonsAuto.Margin = new System.Windows.Forms.Padding(8, 3, 0, 0);
            this.radioAddonsAuto.Size = new System.Drawing.Size(197, 17);
            this.radioAddonsAuto.Text = "Yes, automatically include addons";
            this.radioAddonsAuto.UseVisualStyleBackColor = true;
            // 
            // radioAddonsManual
            // 
            this.radioAddonsManual.AutoSize = true;
            this.radioAddonsManual.Location = new System.Drawing.Point(21, 97);
            this.radioAddonsManual.Margin = new System.Windows.Forms.Padding(8, 3, 0, 0);
            this.radioAddonsManual.Size = new System.Drawing.Size(243, 17);
            this.radioAddonsManual.Text = "Yes, but do not automatically include them";
            this.radioAddonsManual.UseVisualStyleBackColor = true;
            // 
            // panelDirectUpdates
            // 
            this.panelDirectUpdates.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
            | System.Windows.Forms.AnchorStyles.Left)
            | System.Windows.Forms.AnchorStyles.Right)));
            this.panelDirectUpdates.Controls.Add(this.label8);
            this.panelDirectUpdates.Controls.Add(this.label9);
            this.panelDirectUpdates.Controls.Add(this.radioDirectUpdateNo);
            this.panelDirectUpdates.Controls.Add(this.radioDirectUpdateYes);
            this.panelDirectUpdates.Location = new System.Drawing.Point(6, 6);
            this.panelDirectUpdates.Margin = new System.Windows.Forms.Padding(0);
            this.panelDirectUpdates.Padding = new System.Windows.Forms.Padding(13, 10, 13, 0);
            this.panelDirectUpdates.Size = new System.Drawing.Size(372, 170);
            this.panelDirectUpdates.Visible = false;
            // 
            // label8
            // 
            this.label8.AutoSize = true;
            this.label8.Font = new System.Drawing.Font("Segoe UI Semibold", 9.75F, System.Drawing.FontStyle.Bold);
            this.label8.Location = new System.Drawing.Point(13, 10);
            this.label8.Margin = new System.Windows.Forms.Padding(0, 0, 0, 3);
            this.label8.Size = new System.Drawing.Size(216, 17);
            this.label8.Text = "Allow directly updating the game?";
            // 
            // label9
            // 
            this.label9.AutoSize = true;
            this.label9.Font = new System.Drawing.Font("Segoe UI Semilight", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label9.Location = new System.Drawing.Point(14, 31);
            this.label9.Margin = new System.Windows.Forms.Padding(1, 1, 0, 13);
            this.label9.Size = new System.Drawing.Size(324, 26);
            this.label9.Text = "Reduces the time it takes to update multiple accounts and allows for simplified m" +
    "anagement";
            // 
            // radioDirectUpdateNo
            // 
            this.radioDirectUpdateNo.AutoSize = true;
            this.radioDirectUpdateNo.Checked = true;
            this.radioDirectUpdateNo.Location = new System.Drawing.Point(21, 70);
            this.radioDirectUpdateNo.Margin = new System.Windows.Forms.Padding(8, 0, 0, 0);
            this.radioDirectUpdateNo.Size = new System.Drawing.Size(40, 17);
            this.radioDirectUpdateNo.TabStop = true;
            this.radioDirectUpdateNo.Text = "No";
            this.radioDirectUpdateNo.UseVisualStyleBackColor = true;
            // 
            // radioDirectUpdateYes
            // 
            this.radioDirectUpdateYes.AutoSize = true;
            this.radioDirectUpdateYes.Location = new System.Drawing.Point(21, 90);
            this.radioDirectUpdateYes.Margin = new System.Windows.Forms.Padding(8, 3, 0, 0);
            this.radioDirectUpdateYes.Size = new System.Drawing.Size(40, 17);
            this.radioDirectUpdateYes.Text = "Yes";
            this.radioDirectUpdateYes.UseVisualStyleBackColor = true;
            // 
            // panelReady
            // 
            this.panelReady.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
            | System.Windows.Forms.AnchorStyles.Left)
            | System.Windows.Forms.AnchorStyles.Right)));
            this.panelReady.Controls.Add(this.label10);
            this.panelReady.Controls.Add(this.label12);
            this.panelReady.Controls.Add(this.labelMessage);
            this.panelReady.Location = new System.Drawing.Point(6, 6);
            this.panelReady.Margin = new System.Windows.Forms.Padding(0);
            this.panelReady.Padding = new System.Windows.Forms.Padding(13, 10, 13, 0);
            this.panelReady.Size = new System.Drawing.Size(372, 170);
            this.panelReady.Visible = false;
            this.panelReady.VisibleChanged += new System.EventHandler(this.panelReady_VisibleChanged);
            // 
            // label10
            // 
            this.label10.AutoSize = true;
            this.label10.Font = new System.Drawing.Font("Segoe UI Semibold", 9.75F, System.Drawing.FontStyle.Bold);
            this.label10.Location = new System.Drawing.Point(13, 10);
            this.label10.Margin = new System.Windows.Forms.Padding(0, 0, 0, 3);
            this.label10.Size = new System.Drawing.Size(51, 17);
            this.label10.Text = "Ready?";
            // 
            // label12
            // 
            this.label12.AutoSize = true;
            this.label12.Font = new System.Drawing.Font("Segoe UI Semilight", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label12.Location = new System.Drawing.Point(14, 31);
            this.label12.Margin = new System.Windows.Forms.Padding(1, 1, 0, 13);
            this.label12.Size = new System.Drawing.Size(83, 13);
            this.label12.Text = "Click OK to finish";
            // 
            // labelMessage
            // 
            this.labelMessage.AutoSize = true;
            this.labelMessage.Location = new System.Drawing.Point(14, 58);
            this.labelMessage.Margin = new System.Windows.Forms.Padding(1, 1, 0, 13);
            this.labelMessage.Size = new System.Drawing.Size(19, 13);
            this.labelMessage.Text = "---";
            // 
            // panelStart
            // 
            this.panelStart.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
            | System.Windows.Forms.AnchorStyles.Left)
            | System.Windows.Forms.AnchorStyles.Right)));
            this.panelStart.Controls.Add(this.label13);
            this.panelStart.Controls.Add(this.label14);
            this.panelStart.Controls.Add(this.label16);
            this.panelStart.Controls.Add(this.label15);
            this.panelStart.Location = new System.Drawing.Point(6, 6);
            this.panelStart.Margin = new System.Windows.Forms.Padding(0);
            this.panelStart.Padding = new System.Windows.Forms.Padding(13, 10, 13, 0);
            this.panelStart.Size = new System.Drawing.Size(372, 170);
            this.panelStart.Visible = false;
            // 
            // label13
            // 
            this.label13.AutoSize = true;
            this.label13.Font = new System.Drawing.Font("Segoe UI Semibold", 9.75F, System.Drawing.FontStyle.Bold);
            this.label13.Location = new System.Drawing.Point(13, 10);
            this.label13.Margin = new System.Windows.Forms.Padding(0, 0, 0, 3);
            this.label13.Size = new System.Drawing.Size(74, 17);
            this.label13.Text = "Quick start";
            // 
            // label14
            // 
            this.label14.AutoSize = true;
            this.label14.Font = new System.Drawing.Font("Segoe UI Semilight", 8.25F);
            this.label14.Location = new System.Drawing.Point(14, 31);
            this.label14.Margin = new System.Windows.Forms.Padding(1, 1, 0, 13);
            this.label14.Size = new System.Drawing.Size(325, 26);
            this.label14.Text = "Quickly setup multiple accounts and configure a few basic options to determine ho" +
    "w Gw2Launcher will operate";
            // 
            // label16
            // 
            this.label16.AutoSize = true;
            this.label16.Font = new System.Drawing.Font("Segoe UI Semilight", 8.25F);
            this.label16.ForeColor = System.Drawing.Color.MediumBlue;
            this.label16.Location = new System.Drawing.Point(14, 71);
            this.label16.Margin = new System.Windows.Forms.Padding(1, 1, 0, 13);
            this.label16.Size = new System.Drawing.Size(159, 13);
            this.label16.Text = "Everything can be changed later";
            // 
            // label15
            // 
            this.label15.AutoSize = true;
            this.label15.Location = new System.Drawing.Point(14, 107);
            this.label15.Margin = new System.Windows.Forms.Padding(1, 10, 0, 0);
            this.label15.Size = new System.Drawing.Size(209, 13);
            this.label15.Text = "Click next to continue, or cancel to skip";
            // 
            // buttonCancel
            // 
            this.buttonCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonCancel.Font = new System.Drawing.Font("Segoe UI", 8.25F);
            this.buttonCancel.Location = new System.Drawing.Point(190, 192);
            this.buttonCancel.Size = new System.Drawing.Size(86, 35);
            this.buttonCancel.Text = "Cancel";
            this.buttonCancel.UseVisualStyleBackColor = true;
            this.buttonCancel.Visible = false;
            this.buttonCancel.Click += new System.EventHandler(this.buttonCancel_Click);
            // 
            // panelGraphics
            // 
            this.panelGraphics.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
            | System.Windows.Forms.AnchorStyles.Left)
            | System.Windows.Forms.AnchorStyles.Right)));
            this.panelGraphics.Controls.Add(this.label1);
            this.panelGraphics.Controls.Add(this.label2);
            this.panelGraphics.Controls.Add(this.radioGraphicsCustom);
            this.panelGraphics.Controls.Add(this.radioGraphicsShared);
            this.panelGraphics.Location = new System.Drawing.Point(6, 6);
            this.panelGraphics.Margin = new System.Windows.Forms.Padding(0);
            this.panelGraphics.Padding = new System.Windows.Forms.Padding(13, 10, 13, 0);
            this.panelGraphics.Size = new System.Drawing.Size(372, 170);
            this.panelGraphics.Visible = false;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("Segoe UI Semibold", 9.75F, System.Drawing.FontStyle.Bold);
            this.label1.Location = new System.Drawing.Point(13, 10);
            this.label1.Margin = new System.Windows.Forms.Padding(0, 0, 0, 3);
            this.label1.Size = new System.Drawing.Size(332, 17);
            this.label1.Text = "Should each account use the same graphics settings?";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Font = new System.Drawing.Font("Segoe UI Semilight", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label2.Location = new System.Drawing.Point(14, 31);
            this.label2.Margin = new System.Windows.Forms.Padding(1, 1, 0, 13);
            this.label2.Size = new System.Drawing.Size(324, 13);
            this.label2.Text = "Changes will apply to all accounts sharing the same graphics settings";
            // 
            // radioGraphicsCustom
            // 
            this.radioGraphicsCustom.AutoSize = true;
            this.radioGraphicsCustom.Checked = true;
            this.radioGraphicsCustom.Location = new System.Drawing.Point(21, 57);
            this.radioGraphicsCustom.Margin = new System.Windows.Forms.Padding(8, 0, 0, 0);
            this.radioGraphicsCustom.Size = new System.Drawing.Size(40, 17);
            this.radioGraphicsCustom.TabStop = true;
            this.radioGraphicsCustom.Text = "No";
            this.radioGraphicsCustom.UseVisualStyleBackColor = true;
            // 
            // radioGraphicsShared
            // 
            this.radioGraphicsShared.AutoSize = true;
            this.radioGraphicsShared.Location = new System.Drawing.Point(21, 77);
            this.radioGraphicsShared.Margin = new System.Windows.Forms.Padding(8, 3, 0, 0);
            this.radioGraphicsShared.Size = new System.Drawing.Size(40, 17);
            this.radioGraphicsShared.Text = "Yes";
            this.radioGraphicsShared.UseVisualStyleBackColor = true;
            // 
            // panelLogin
            // 
            this.panelLogin.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
            | System.Windows.Forms.AnchorStyles.Left)
            | System.Windows.Forms.AnchorStyles.Right)));
            this.panelLogin.Controls.Add(this.label4);
            this.panelLogin.Controls.Add(this.label7);
            this.panelLogin.Controls.Add(this.radioLoginRemembered);
            this.panelLogin.Controls.Add(this.radioLoginManual);
            this.panelLogin.Location = new System.Drawing.Point(6, 6);
            this.panelLogin.Margin = new System.Windows.Forms.Padding(0);
            this.panelLogin.Padding = new System.Windows.Forms.Padding(13, 10, 13, 0);
            this.panelLogin.Size = new System.Drawing.Size(372, 170);
            this.panelLogin.Visible = false;
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Font = new System.Drawing.Font("Segoe UI Semibold", 9.75F, System.Drawing.FontStyle.Bold);
            this.label4.Location = new System.Drawing.Point(13, 10);
            this.label4.Margin = new System.Windows.Forms.Padding(0, 0, 0, 3);
            this.label4.Size = new System.Drawing.Size(174, 17);
            this.label4.Text = "How do you want to login?";
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Font = new System.Drawing.Font("Segoe UI Semilight", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label7.Location = new System.Drawing.Point(14, 31);
            this.label7.Margin = new System.Windows.Forms.Padding(1, 1, 0, 13);
            this.label7.Size = new System.Drawing.Size(186, 13);
            this.label7.Text = "For accounts without automated logins";
            // 
            // radioLoginRemembered
            // 
            this.radioLoginRemembered.AutoSize = true;
            this.radioLoginRemembered.Checked = true;
            this.radioLoginRemembered.Location = new System.Drawing.Point(21, 57);
            this.radioLoginRemembered.Margin = new System.Windows.Forms.Padding(8, 0, 0, 0);
            this.radioLoginRemembered.Size = new System.Drawing.Size(175, 17);
            this.radioLoginRemembered.TabStop = true;
            this.radioLoginRemembered.Text = "Remembered by the launcher";
            this.radioLoginRemembered.UseVisualStyleBackColor = true;
            // 
            // radioLoginManual
            // 
            this.radioLoginManual.AutoSize = true;
            this.radioLoginManual.Location = new System.Drawing.Point(21, 77);
            this.radioLoginManual.Margin = new System.Windows.Forms.Padding(8, 3, 0, 0);
            this.radioLoginManual.Size = new System.Drawing.Size(93, 17);
            this.radioLoginManual.Text = "Manual entry";
            this.radioLoginManual.UseVisualStyleBackColor = true;
            // 
            // panelExe
            // 
            this.panelExe.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
            | System.Windows.Forms.AnchorStyles.Left)
            | System.Windows.Forms.AnchorStyles.Right)));
            this.panelExe.Controls.Add(this.labelGw2ExeTitle);
            this.panelExe.Controls.Add(this.label18);
            this.panelExe.Controls.Add(this.stackPanel46);
            this.panelExe.Location = new System.Drawing.Point(6, 6);
            this.panelExe.Margin = new System.Windows.Forms.Padding(0);
            this.panelExe.Padding = new System.Windows.Forms.Padding(13, 10, 13, 0);
            this.panelExe.Size = new System.Drawing.Size(372, 170);
            this.panelExe.Visible = false;
            // 
            // labelGw2ExeTitle
            // 
            this.labelGw2ExeTitle.AutoSize = true;
            this.labelGw2ExeTitle.Font = new System.Drawing.Font("Segoe UI Semibold", 9.75F, System.Drawing.FontStyle.Bold);
            this.labelGw2ExeTitle.Location = new System.Drawing.Point(13, 10);
            this.labelGw2ExeTitle.Margin = new System.Windows.Forms.Padding(0, 0, 0, 3);
            this.labelGw2ExeTitle.Size = new System.Drawing.Size(75, 17);
            this.labelGw2ExeTitle.Text = "{0} location";
            // 
            // label18
            // 
            this.label18.AutoSize = true;
            this.label18.Font = new System.Drawing.Font("Segoe UI Semilight", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label18.Location = new System.Drawing.Point(14, 31);
            this.label18.Margin = new System.Windows.Forms.Padding(1, 1, 0, 13);
            this.label18.Size = new System.Drawing.Size(183, 13);
            this.label18.Text = "The location of the Guild Wars 2 client";
            // 
            // stackPanel46
            // 
            this.stackPanel46.AutoSize = true;
            this.stackPanel46.AutoSizeFill = Gw2Launcher.UI.Controls.StackPanel.AutoSizeFillMode.Width;
            this.stackPanel46.Controls.Add(this.textGw2Path);
            this.stackPanel46.Controls.Add(this.buttonGw2Path);
            this.stackPanel46.FlowDirection = System.Windows.Forms.FlowDirection.LeftToRight;
            this.stackPanel46.Location = new System.Drawing.Point(13, 57);
            this.stackPanel46.Margin = new System.Windows.Forms.Padding(0, 0, 10, 0);
            this.stackPanel46.Size = new System.Drawing.Size(336, 24);
            // 
            // textGw2Path
            // 
            this.textGw2Path.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.textGw2Path.Font = new System.Drawing.Font("Segoe UI", 8.25F);
            this.textGw2Path.Location = new System.Drawing.Point(8, 1);
            this.textGw2Path.Margin = new System.Windows.Forms.Padding(8, 1, 0, 1);
            this.textGw2Path.Size = new System.Drawing.Size(279, 22);
            // 
            // buttonGw2Path
            // 
            this.buttonGw2Path.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)));
            this.buttonGw2Path.AutoSize = true;
            this.buttonGw2Path.Font = new System.Drawing.Font("Segoe UI", 8.25F);
            this.buttonGw2Path.Location = new System.Drawing.Point(293, 0);
            this.buttonGw2Path.Margin = new System.Windows.Forms.Padding(6, 0, 0, 0);
            this.buttonGw2Path.Size = new System.Drawing.Size(43, 24);
            this.buttonGw2Path.Text = "...";
            this.buttonGw2Path.UseVisualStyleBackColor = true;
            this.buttonGw2Path.Click += new System.EventHandler(this.buttonGw2Path_Click);
            // 
            // formQuickStart
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
            this.ClientSize = new System.Drawing.Size(384, 239);
            this.Controls.Add(this.buttonOK);
            this.Controls.Add(this.buttonCancel);
            this.Controls.Add(this.buttonNext);
            this.Controls.Add(this.buttonBack);
            this.Controls.Add(this.panelAccounts);
            this.Controls.Add(this.panelStart);
            this.Controls.Add(this.panelReady);
            this.Controls.Add(this.panelExe);
            this.Controls.Add(this.panelLogin);
            this.Controls.Add(this.panelGraphics);
            this.Controls.Add(this.panelDirectUpdates);
            this.Controls.Add(this.panelAddons);
            this.Icon = global::Gw2Launcher.Properties.Resources.Gw2Launcher;
            this.Margin = new System.Windows.Forms.Padding(4);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.MinimumSize = new System.Drawing.Size(400, 265);
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.panelAccounts.ResumeLayout(false);
            this.panelAccounts.PerformLayout();
            this.autoScrollContainerPanel1.ResumeLayout(false);
            this.autoScrollContainerPanel1.PerformLayout();
            this.stackPanel1.ResumeLayout(false);
            this.stackPanel1.PerformLayout();
            this.panelAccountsContent.ResumeLayout(false);
            this.panelAccount.ResumeLayout(false);
            this.panelAccount.PerformLayout();
            this.contextAccount.ResumeLayout(false);
            this.panelAddons.ResumeLayout(false);
            this.panelAddons.PerformLayout();
            this.panelDirectUpdates.ResumeLayout(false);
            this.panelDirectUpdates.PerformLayout();
            this.panelReady.ResumeLayout(false);
            this.panelReady.PerformLayout();
            this.panelStart.ResumeLayout(false);
            this.panelStart.PerformLayout();
            this.panelGraphics.ResumeLayout(false);
            this.panelGraphics.PerformLayout();
            this.panelLogin.ResumeLayout(false);
            this.panelLogin.PerformLayout();
            this.panelExe.ResumeLayout(false);
            this.panelExe.PerformLayout();
            this.stackPanel46.ResumeLayout(false);
            this.stackPanel46.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Button buttonBack;
        private System.Windows.Forms.Button buttonOK;
        private Gw2Launcher.UI.Controls.StackPanel panelAccounts;
        private System.Windows.Forms.Label label3;
        private Gw2Launcher.UI.Controls.StackPanel panelAccountsContent;
        private System.Windows.Forms.Button buttonNext;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.ContextMenuStrip contextAccount;
        private System.Windows.Forms.ToolStripMenuItem editToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem removeToolStripMenuItem;
        private UI.Controls.StackPanel panelAddons;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.Label label11;
        private System.Windows.Forms.RadioButton radioAddonsNo;
        private System.Windows.Forms.RadioButton radioAddonsAuto;
        private System.Windows.Forms.RadioButton radioAddonsManual;
        private UI.Controls.StackPanel panelDirectUpdates;
        private System.Windows.Forms.Label label8;
        private System.Windows.Forms.Label label9;
        private System.Windows.Forms.RadioButton radioDirectUpdateNo;
        private System.Windows.Forms.RadioButton radioDirectUpdateYes;
        private UI.Controls.StackPanel panelReady;
        private System.Windows.Forms.Label label10;
        private System.Windows.Forms.Label label12;
        private System.Windows.Forms.Label labelMessage;
        private UI.Controls.StackPanel panelStart;
        private System.Windows.Forms.Label label13;
        private System.Windows.Forms.Label label14;
        private System.Windows.Forms.Label label16;
        private System.Windows.Forms.Label label15;
        private System.Windows.Forms.Button buttonCancel;
        private UI.Controls.StackPanel panelAccount;
        private System.Windows.Forms.TextBox textName;
        private UI.Controls.FlatShapeButton buttonOptions;
        private Controls.AutoScrollContainerPanel autoScrollContainerPanel1;
        private Controls.StackPanel stackPanel1;
        private Controls.LinkLabel labelAdd;
        private Controls.StackPanel panelGraphics;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.RadioButton radioGraphicsCustom;
        private System.Windows.Forms.RadioButton radioGraphicsShared;
        private Controls.StackPanel panelLogin;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.RadioButton radioLoginRemembered;
        private System.Windows.Forms.RadioButton radioLoginManual;
        private Controls.StackPanel panelExe;
        private System.Windows.Forms.Label labelGw2ExeTitle;
        private System.Windows.Forms.Label label18;
        private Controls.StackPanel stackPanel46;
        private System.Windows.Forms.TextBox textGw2Path;
        private System.Windows.Forms.Button buttonGw2Path;
    }
}
