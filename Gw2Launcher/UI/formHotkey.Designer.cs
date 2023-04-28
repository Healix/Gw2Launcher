namespace Gw2Launcher.UI
{
    partial class formHotkey
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
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle1 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle2 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle3 = new System.Windows.Forms.DataGridViewCellStyle();
            this.panelContainer = new Gw2Launcher.UI.Controls.StackPanel();
            this.stackPanel1 = new Gw2Launcher.UI.Controls.StackPanel();
            this.panelCategory = new Gw2Launcher.UI.Controls.StackPanel();
            this.label2 = new System.Windows.Forms.Label();
            this.gridCategory = new Gw2Launcher.UI.Controls.ScaledDataGridView();
            this.columnCategory = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.panelAction = new Gw2Launcher.UI.Controls.StackPanel();
            this.label4 = new System.Windows.Forms.Label();
            this.gridAction = new Gw2Launcher.UI.Controls.ScaledDataGridView();
            this.columnAction = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.labelTemplate = new System.Windows.Forms.Label();
            this.keysHotkey = new Gw2Launcher.UI.Controls.HotkeyInput();
            this.panelScroll = new Gw2Launcher.UI.Controls.AutoScrollContainerPanel();
            this.panelContent = new Gw2Launcher.UI.Controls.StackPanel();
            this.panelAccounts = new Gw2Launcher.UI.Controls.StackPanel();
            this.label6 = new System.Windows.Forms.Label();
            this.gridAccounts = new Gw2Launcher.UI.Controls.ScaledDataGridView();
            this.columnAccount = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.panelProgram = new Gw2Launcher.UI.Controls.StackPanel();
            this.label5 = new System.Windows.Forms.Label();
            this.stackPanel3 = new Gw2Launcher.UI.Controls.StackPanel();
            this.textPath = new System.Windows.Forms.TextBox();
            this.buttonPath = new System.Windows.Forms.Button();
            this.label3 = new System.Windows.Forms.Label();
            this.textArguments = new System.Windows.Forms.TextBox();
            this.panelKeyPress = new Gw2Launcher.UI.Controls.StackPanel();
            this.label1 = new System.Windows.Forms.Label();
            this.keysKeyPress = new Gw2Launcher.UI.Controls.HotkeyInput();
            this.label7 = new System.Windows.Forms.Label();
            this.stackPanel2 = new Gw2Launcher.UI.Controls.StackPanel();
            this.buttonCancel = new System.Windows.Forms.Button();
            this.buttonOk = new System.Windows.Forms.Button();
            this.comboKeyPressMethod = new System.Windows.Forms.ComboBox();
            this.panelContainer.SuspendLayout();
            this.stackPanel1.SuspendLayout();
            this.panelCategory.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.gridCategory)).BeginInit();
            this.panelAction.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.gridAction)).BeginInit();
            this.panelScroll.SuspendLayout();
            this.panelContent.SuspendLayout();
            this.panelAccounts.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.gridAccounts)).BeginInit();
            this.panelProgram.SuspendLayout();
            this.stackPanel3.SuspendLayout();
            this.panelKeyPress.SuspendLayout();
            this.stackPanel2.SuspendLayout();
            this.SuspendLayout();
            // 
            // panelContainer
            // 
            this.panelContainer.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.panelContainer.Controls.Add(this.stackPanel1);
            this.panelContainer.Controls.Add(this.labelTemplate);
            this.panelContainer.Controls.Add(this.keysHotkey);
            this.panelContainer.Controls.Add(this.panelScroll);
            this.panelContainer.Controls.Add(this.stackPanel2);
            this.panelContainer.Location = new System.Drawing.Point(15, 15);
            this.panelContainer.Margin = new System.Windows.Forms.Padding(15);
            this.panelContainer.Size = new System.Drawing.Size(299, 381);
            // 
            // stackPanel1
            // 
            this.stackPanel1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.stackPanel1.AutoSize = true;
            this.stackPanel1.AutoSizeFill = Gw2Launcher.UI.Controls.StackPanel.AutoSizeFillMode.Width;
            this.stackPanel1.Controls.Add(this.panelCategory);
            this.stackPanel1.Controls.Add(this.panelAction);
            this.stackPanel1.FlowDirection = System.Windows.Forms.FlowDirection.LeftToRight;
            this.stackPanel1.Location = new System.Drawing.Point(0, 0);
            this.stackPanel1.Margin = new System.Windows.Forms.Padding(0);
            this.stackPanel1.Size = new System.Drawing.Size(299, 169);
            // 
            // panelCategory
            // 
            this.panelCategory.AutoSize = true;
            this.panelCategory.Controls.Add(this.label2);
            this.panelCategory.Controls.Add(this.gridCategory);
            this.panelCategory.Location = new System.Drawing.Point(0, 0);
            this.panelCategory.Margin = new System.Windows.Forms.Padding(0, 0, 5, 0);
            this.panelCategory.Size = new System.Drawing.Size(103, 169);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Font = new System.Drawing.Font("Segoe UI Semilight", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label2.Location = new System.Drawing.Point(0, 0);
            this.label2.Margin = new System.Windows.Forms.Padding(0, 0, 0, 6);
            this.label2.Size = new System.Drawing.Size(50, 13);
            this.label2.Text = "Category";
            // 
            // gridCategory
            // 
            this.gridCategory.AllowUserToAddRows = false;
            this.gridCategory.AllowUserToDeleteRows = false;
            this.gridCategory.AllowUserToResizeColumns = false;
            this.gridCategory.AllowUserToResizeRows = false;
            this.gridCategory.BackgroundColor = System.Drawing.SystemColors.Window;
            this.gridCategory.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.gridCategory.CellBorderStyle = System.Windows.Forms.DataGridViewCellBorderStyle.None;
            this.gridCategory.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.gridCategory.ColumnHeadersVisible = false;
            this.gridCategory.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.columnCategory});
            dataGridViewCellStyle1.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle1.BackColor = System.Drawing.SystemColors.Window;
            dataGridViewCellStyle1.Font = new System.Drawing.Font("Segoe UI", 8.25F);
            dataGridViewCellStyle1.ForeColor = System.Drawing.SystemColors.ControlText;
            dataGridViewCellStyle1.SelectionBackColor = System.Drawing.SystemColors.ControlLight;
            dataGridViewCellStyle1.SelectionForeColor = System.Drawing.SystemColors.ControlText;
            dataGridViewCellStyle1.WrapMode = System.Windows.Forms.DataGridViewTriState.False;
            this.gridCategory.DefaultCellStyle = dataGridViewCellStyle1;
            this.gridCategory.EditMode = System.Windows.Forms.DataGridViewEditMode.EditProgrammatically;
            this.gridCategory.Location = new System.Drawing.Point(3, 19);
            this.gridCategory.Margin = new System.Windows.Forms.Padding(3, 0, 0, 0);
            this.gridCategory.MultiSelect = false;
            this.gridCategory.ReadOnly = true;
            this.gridCategory.RowHeadersVisible = false;
            this.gridCategory.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.gridCategory.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.gridCategory.Size = new System.Drawing.Size(100, 150);
            // 
            // columnCategory
            // 
            this.columnCategory.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
            this.columnCategory.HeaderText = "";
            this.columnCategory.ReadOnly = true;
            // 
            // panelAction
            // 
            this.panelAction.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.panelAction.AutoSize = true;
            this.panelAction.AutoSizeFill = Gw2Launcher.UI.Controls.StackPanel.AutoSizeFillMode.Width;
            this.panelAction.Controls.Add(this.label4);
            this.panelAction.Controls.Add(this.gridAction);
            this.panelAction.Location = new System.Drawing.Point(108, 0);
            this.panelAction.Margin = new System.Windows.Forms.Padding(0);
            this.panelAction.Size = new System.Drawing.Size(191, 169);
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Font = new System.Drawing.Font("Segoe UI Semilight", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label4.Location = new System.Drawing.Point(0, 0);
            this.label4.Margin = new System.Windows.Forms.Padding(0, 0, 0, 6);
            this.label4.Size = new System.Drawing.Size(36, 13);
            this.label4.Text = "Action";
            // 
            // gridAction
            // 
            this.gridAction.AllowUserToAddRows = false;
            this.gridAction.AllowUserToDeleteRows = false;
            this.gridAction.AllowUserToResizeColumns = false;
            this.gridAction.AllowUserToResizeRows = false;
            this.gridAction.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.gridAction.BackgroundColor = System.Drawing.SystemColors.Window;
            this.gridAction.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.gridAction.CellBorderStyle = System.Windows.Forms.DataGridViewCellBorderStyle.None;
            this.gridAction.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.gridAction.ColumnHeadersVisible = false;
            this.gridAction.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.columnAction});
            dataGridViewCellStyle2.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle2.BackColor = System.Drawing.SystemColors.Window;
            dataGridViewCellStyle2.Font = new System.Drawing.Font("Segoe UI", 8.25F);
            dataGridViewCellStyle2.ForeColor = System.Drawing.SystemColors.ControlText;
            dataGridViewCellStyle2.SelectionBackColor = System.Drawing.SystemColors.ControlLight;
            dataGridViewCellStyle2.SelectionForeColor = System.Drawing.SystemColors.ControlText;
            dataGridViewCellStyle2.WrapMode = System.Windows.Forms.DataGridViewTriState.False;
            this.gridAction.DefaultCellStyle = dataGridViewCellStyle2;
            this.gridAction.EditMode = System.Windows.Forms.DataGridViewEditMode.EditProgrammatically;
            this.gridAction.Location = new System.Drawing.Point(3, 19);
            this.gridAction.Margin = new System.Windows.Forms.Padding(3, 0, 3, 0);
            this.gridAction.MultiSelect = false;
            this.gridAction.ReadOnly = true;
            this.gridAction.RowHeadersVisible = false;
            this.gridAction.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.gridAction.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.gridAction.Size = new System.Drawing.Size(185, 150);
            // 
            // columnAction
            // 
            this.columnAction.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
            this.columnAction.HeaderText = "";
            this.columnAction.ReadOnly = true;
            // 
            // labelTemplate
            // 
            this.labelTemplate.AutoSize = true;
            this.labelTemplate.Font = new System.Drawing.Font("Segoe UI Semilight", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.labelTemplate.Location = new System.Drawing.Point(0, 175);
            this.labelTemplate.Margin = new System.Windows.Forms.Padding(0, 6, 0, 6);
            this.labelTemplate.Size = new System.Drawing.Size(40, 13);
            this.labelTemplate.Text = "Hotkey";
            // 
            // keysHotkey
            // 
            this.keysHotkey.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.keysHotkey.Cursor = Windows.Cursors.Hand;
            this.keysHotkey.Keys = System.Windows.Forms.Keys.None;
            this.keysHotkey.Location = new System.Drawing.Point(3, 194);
            this.keysHotkey.Margin = new System.Windows.Forms.Padding(3, 0, 3, 0);
            this.keysHotkey.Size = new System.Drawing.Size(293, 23);
            // 
            // panelScroll
            // 
            this.panelScroll.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.panelScroll.Controls.Add(this.panelContent);
            this.panelScroll.Location = new System.Drawing.Point(0, 225);
            this.panelScroll.Margin = new System.Windows.Forms.Padding(0, 8, 0, 0);
            this.panelScroll.Size = new System.Drawing.Size(299, 101);
            // 
            // panelContent
            // 
            this.panelContent.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.panelContent.AutoSize = true;
            this.panelContent.Controls.Add(this.panelAccounts);
            this.panelContent.Controls.Add(this.panelProgram);
            this.panelContent.Controls.Add(this.panelKeyPress);
            this.panelContent.Location = new System.Drawing.Point(0, 0);
            this.panelContent.Margin = new System.Windows.Forms.Padding(0);
            this.panelContent.MinimumSize = new System.Drawing.Size(0, 10);
            this.panelContent.Size = new System.Drawing.Size(282, 294);
            // 
            // panelAccounts
            // 
            this.panelAccounts.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.panelAccounts.Controls.Add(this.label6);
            this.panelAccounts.Controls.Add(this.gridAccounts);
            this.panelAccounts.Location = new System.Drawing.Point(0, 0);
            this.panelAccounts.Margin = new System.Windows.Forms.Padding(0);
            this.panelAccounts.MinimumSize = new System.Drawing.Size(0, 100);
            this.panelAccounts.Size = new System.Drawing.Size(282, 100);
            this.panelAccounts.Visible = false;
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Font = new System.Drawing.Font("Segoe UI Semilight", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label6.Location = new System.Drawing.Point(0, 6);
            this.label6.Margin = new System.Windows.Forms.Padding(0, 6, 0, 6);
            this.label6.Size = new System.Drawing.Size(45, 13);
            this.label6.Text = "Account";
            // 
            // gridAccounts
            // 
            this.gridAccounts.AllowUserToAddRows = false;
            this.gridAccounts.AllowUserToDeleteRows = false;
            this.gridAccounts.AllowUserToResizeColumns = false;
            this.gridAccounts.AllowUserToResizeRows = false;
            this.gridAccounts.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.gridAccounts.BackgroundColor = System.Drawing.SystemColors.Window;
            this.gridAccounts.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.gridAccounts.CellBorderStyle = System.Windows.Forms.DataGridViewCellBorderStyle.None;
            this.gridAccounts.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.gridAccounts.ColumnHeadersVisible = false;
            this.gridAccounts.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.columnAccount});
            dataGridViewCellStyle3.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle3.BackColor = System.Drawing.SystemColors.Window;
            dataGridViewCellStyle3.Font = new System.Drawing.Font("Segoe UI", 8.25F);
            dataGridViewCellStyle3.ForeColor = System.Drawing.SystemColors.ControlText;
            dataGridViewCellStyle3.SelectionBackColor = System.Drawing.SystemColors.ControlLight;
            dataGridViewCellStyle3.SelectionForeColor = System.Drawing.SystemColors.ControlText;
            dataGridViewCellStyle3.WrapMode = System.Windows.Forms.DataGridViewTriState.False;
            this.gridAccounts.DefaultCellStyle = dataGridViewCellStyle3;
            this.gridAccounts.EditMode = System.Windows.Forms.DataGridViewEditMode.EditProgrammatically;
            this.gridAccounts.Enabled = false;
            this.gridAccounts.Location = new System.Drawing.Point(3, 25);
            this.gridAccounts.Margin = new System.Windows.Forms.Padding(3, 0, 3, 0);
            this.gridAccounts.MultiSelect = false;
            this.gridAccounts.ReadOnly = true;
            this.gridAccounts.RowHeadersVisible = false;
            this.gridAccounts.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.gridAccounts.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.gridAccounts.Size = new System.Drawing.Size(276, 75);
            // 
            // columnAccount
            // 
            this.columnAccount.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
            this.columnAccount.HeaderText = "";
            this.columnAccount.ReadOnly = true;
            // 
            // panelProgram
            // 
            this.panelProgram.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.panelProgram.AutoSize = true;
            this.panelProgram.Controls.Add(this.label5);
            this.panelProgram.Controls.Add(this.stackPanel3);
            this.panelProgram.Controls.Add(this.label3);
            this.panelProgram.Controls.Add(this.textArguments);
            this.panelProgram.Location = new System.Drawing.Point(0, 100);
            this.panelProgram.Margin = new System.Windows.Forms.Padding(0);
            this.panelProgram.Size = new System.Drawing.Size(282, 98);
            this.panelProgram.Visible = false;
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Font = new System.Drawing.Font("Segoe UI Semilight", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label5.Location = new System.Drawing.Point(0, 6);
            this.label5.Margin = new System.Windows.Forms.Padding(0, 6, 0, 6);
            this.label5.Size = new System.Drawing.Size(100, 13);
            this.label5.Text = "The program to run";
            // 
            // stackPanel3
            // 
            this.stackPanel3.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.stackPanel3.AutoSize = true;
            this.stackPanel3.AutoSizeFill = Gw2Launcher.UI.Controls.StackPanel.AutoSizeFillMode.NoWrap;
            this.stackPanel3.Controls.Add(this.textPath);
            this.stackPanel3.Controls.Add(this.buttonPath);
            this.stackPanel3.FlowDirection = System.Windows.Forms.FlowDirection.LeftToRight;
            this.stackPanel3.Location = new System.Drawing.Point(0, 25);
            this.stackPanel3.Margin = new System.Windows.Forms.Padding(0);
            this.stackPanel3.Size = new System.Drawing.Size(282, 24);
            // 
            // textPath
            // 
            this.textPath.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.textPath.Font = new System.Drawing.Font("Segoe UI", 8.25F);
            this.textPath.Location = new System.Drawing.Point(3, 1);
            this.textPath.Margin = new System.Windows.Forms.Padding(3, 1, 0, 1);
            this.textPath.Size = new System.Drawing.Size(227, 22);
            // 
            // buttonPath
            // 
            this.buttonPath.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)));
            this.buttonPath.AutoSize = true;
            this.buttonPath.Font = new System.Drawing.Font("Segoe UI", 8.25F);
            this.buttonPath.Location = new System.Drawing.Point(236, 0);
            this.buttonPath.Margin = new System.Windows.Forms.Padding(6, 0, 3, 0);
            this.buttonPath.Padding = new System.Windows.Forms.Padding(8, 0, 8, 0);
            this.buttonPath.Size = new System.Drawing.Size(43, 24);
            this.buttonPath.Text = "...";
            this.buttonPath.UseVisualStyleBackColor = true;
            this.buttonPath.Click += new System.EventHandler(this.buttonPath_Click);
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Font = new System.Drawing.Font("Segoe UI Semilight", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label3.Location = new System.Drawing.Point(0, 55);
            this.label3.Margin = new System.Windows.Forms.Padding(0, 6, 0, 6);
            this.label3.Size = new System.Drawing.Size(168, 13);
            this.label3.Text = "Optional command line arguments";
            // 
            // textArguments
            // 
            this.textArguments.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.textArguments.Font = new System.Drawing.Font("Segoe UI", 8.25F);
            this.textArguments.Location = new System.Drawing.Point(3, 75);
            this.textArguments.Margin = new System.Windows.Forms.Padding(3, 1, 3, 1);
            this.textArguments.Size = new System.Drawing.Size(276, 22);
            // 
            // panelKeyPress
            // 
            this.panelKeyPress.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.panelKeyPress.AutoSize = true;
            this.panelKeyPress.Controls.Add(this.label1);
            this.panelKeyPress.Controls.Add(this.keysKeyPress);
            this.panelKeyPress.Controls.Add(this.label7);
            this.panelKeyPress.Controls.Add(this.comboKeyPressMethod);
            this.panelKeyPress.Location = new System.Drawing.Point(0, 198);
            this.panelKeyPress.Margin = new System.Windows.Forms.Padding(0);
            this.panelKeyPress.Size = new System.Drawing.Size(282, 96);
            this.panelKeyPress.Visible = false;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("Segoe UI Semilight", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label1.Location = new System.Drawing.Point(0, 6);
            this.label1.Margin = new System.Windows.Forms.Padding(0, 6, 0, 6);
            this.label1.Size = new System.Drawing.Size(63, 13);
            this.label1.Text = "Key to press";
            // 
            // keysKeyPress
            // 
            this.keysKeyPress.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.keysKeyPress.Cursor = Windows.Cursors.Hand;
            this.keysKeyPress.Keys = System.Windows.Forms.Keys.None;
            this.keysKeyPress.Location = new System.Drawing.Point(3, 25);
            this.keysKeyPress.Margin = new System.Windows.Forms.Padding(3, 0, 3, 0);
            this.keysKeyPress.Size = new System.Drawing.Size(276, 23);
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Font = new System.Drawing.Font("Segoe UI Semilight", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label7.Location = new System.Drawing.Point(0, 54);
            this.label7.Margin = new System.Windows.Forms.Padding(0, 6, 0, 6);
            this.label7.Size = new System.Drawing.Size(44, 13);
            this.label7.Text = "Method";
            // 
            // stackPanel2
            // 
            this.stackPanel2.Anchor = System.Windows.Forms.AnchorStyles.Bottom;
            this.stackPanel2.AutoSize = true;
            this.stackPanel2.Controls.Add(this.buttonCancel);
            this.stackPanel2.Controls.Add(this.buttonOk);
            this.stackPanel2.FlowDirection = System.Windows.Forms.FlowDirection.LeftToRight;
            this.stackPanel2.Location = new System.Drawing.Point(58, 346);
            this.stackPanel2.Margin = new System.Windows.Forms.Padding(0, 20, 0, 0);
            this.stackPanel2.Size = new System.Drawing.Size(182, 35);
            // 
            // buttonCancel
            // 
            this.buttonCancel.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.buttonCancel.Font = new System.Drawing.Font("Segoe UI", 8.25F);
            this.buttonCancel.Location = new System.Drawing.Point(0, 0);
            this.buttonCancel.Margin = new System.Windows.Forms.Padding(0);
            this.buttonCancel.Size = new System.Drawing.Size(86, 35);
            this.buttonCancel.Text = "Cancel";
            this.buttonCancel.UseVisualStyleBackColor = true;
            this.buttonCancel.Click += new System.EventHandler(this.buttonCancel_Click);
            // 
            // buttonOk
            // 
            this.buttonOk.Font = new System.Drawing.Font("Segoe UI", 8.25F);
            this.buttonOk.Location = new System.Drawing.Point(96, 0);
            this.buttonOk.Margin = new System.Windows.Forms.Padding(10, 0, 0, 0);
            this.buttonOk.Size = new System.Drawing.Size(86, 35);
            this.buttonOk.Text = "OK";
            this.buttonOk.UseVisualStyleBackColor = true;
            this.buttonOk.Click += new System.EventHandler(this.buttonOk_Click);
            // 
            // comboKeyPressMethod
            // 
            this.comboKeyPressMethod.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboKeyPressMethod.FormattingEnabled = true;
            this.comboKeyPressMethod.Location = new System.Drawing.Point(3, 74);
            this.comboKeyPressMethod.Margin = new System.Windows.Forms.Padding(3, 1, 3, 1);
            this.comboKeyPressMethod.Size = new System.Drawing.Size(165, 21);
            // 
            // formHotkey
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
            this.BackColorName = Gw2Launcher.UI.UiColors.Colors.Custom;
            this.ClientSize = new System.Drawing.Size(329, 411);
            this.Controls.Add(this.panelContainer);
            this.ForeColorName = Gw2Launcher.UI.UiColors.Colors.Custom;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.MinimumSize = new System.Drawing.Size(345, 450);
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.panelContainer.ResumeLayout(false);
            this.panelContainer.PerformLayout();
            this.stackPanel1.ResumeLayout(false);
            this.stackPanel1.PerformLayout();
            this.panelCategory.ResumeLayout(false);
            this.panelCategory.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.gridCategory)).EndInit();
            this.panelAction.ResumeLayout(false);
            this.panelAction.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.gridAction)).EndInit();
            this.panelScroll.ResumeLayout(false);
            this.panelScroll.PerformLayout();
            this.panelContent.ResumeLayout(false);
            this.panelContent.PerformLayout();
            this.panelAccounts.ResumeLayout(false);
            this.panelAccounts.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.gridAccounts)).EndInit();
            this.panelProgram.ResumeLayout(false);
            this.panelProgram.PerformLayout();
            this.stackPanel3.ResumeLayout(false);
            this.stackPanel3.PerformLayout();
            this.panelKeyPress.ResumeLayout(false);
            this.panelKeyPress.PerformLayout();
            this.stackPanel2.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private Controls.StackPanel panelContainer;
        private Controls.StackPanel panelProgram;
        private System.Windows.Forms.Label label5;
        private Controls.StackPanel stackPanel3;
        private System.Windows.Forms.TextBox textPath;
        private System.Windows.Forms.Button buttonPath;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.TextBox textArguments;
        private System.Windows.Forms.Label labelTemplate;
        private Controls.HotkeyInput keysHotkey;
        private Controls.StackPanel stackPanel1;
        private Controls.StackPanel panelCategory;
        private System.Windows.Forms.Label label2;
        private Controls.ScaledDataGridView gridCategory;
        private System.Windows.Forms.DataGridViewTextBoxColumn columnCategory;
        private Controls.StackPanel panelAction;
        private System.Windows.Forms.Label label4;
        private Controls.ScaledDataGridView gridAction;
        private Controls.StackPanel panelAccounts;
        private System.Windows.Forms.Label label6;
        private Controls.ScaledDataGridView gridAccounts;
        private System.Windows.Forms.DataGridViewTextBoxColumn columnAccount;
        private Controls.StackPanel stackPanel2;
        private System.Windows.Forms.Button buttonCancel;
        private System.Windows.Forms.Button buttonOk;
        private System.Windows.Forms.DataGridViewTextBoxColumn columnAction;
        private Controls.AutoScrollContainerPanel panelScroll;
        private Controls.StackPanel panelContent;
        private Controls.StackPanel panelKeyPress;
        private System.Windows.Forms.Label label1;
        private Controls.HotkeyInput keysKeyPress;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.ComboBox comboKeyPressMethod;
    }
}
