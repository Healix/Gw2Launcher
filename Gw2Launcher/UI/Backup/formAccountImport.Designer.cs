namespace Gw2Launcher.UI.Backup
{
    partial class formAccountImport
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
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle2 = new System.Windows.Forms.DataGridViewCellStyle();
            this.contextAccounts = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.removeToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.panel1 = new System.Windows.Forms.Panel();
            this.panelNav = new Gw2Launcher.UI.Controls.StackPanel();
            this.buttonCancel = new System.Windows.Forms.Button();
            this.buttonBack = new System.Windows.Forms.Button();
            this.buttonNext = new System.Windows.Forms.Button();
            this.buttonImport = new System.Windows.Forms.Button();
            this.panelImportScroll = new Gw2Launcher.UI.Controls.AutoScrollContainerPanel();
            this.panelImport = new Gw2Launcher.UI.Controls.StackPanel();
            this.label12 = new Gw2Launcher.UI.Controls.GradientLabel();
            this.panelBrowse = new Gw2Launcher.UI.Controls.StackPanel();
            this.label9 = new System.Windows.Forms.Label();
            this.label10 = new System.Windows.Forms.Label();
            this.stackPanel1 = new Gw2Launcher.UI.Controls.StackPanel();
            this.textBrowse = new System.Windows.Forms.TextBox();
            this.buttonBrowse = new System.Windows.Forms.Button();
            this.panelAccounts = new Gw2Launcher.UI.Controls.StackPanel();
            this.label13 = new Gw2Launcher.UI.Controls.GradientLabel();
            this.gridAccounts = new Gw2Launcher.UI.Controls.SelectionDataGridView();
            this.columnName = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.panelRequiredScroll = new Gw2Launcher.UI.Controls.AutoScrollContainerPanel();
            this.panelRequired = new Gw2Launcher.UI.Controls.StackPanel();
            this.label3 = new Gw2Launcher.UI.Controls.GradientLabel();
            this.panelType = new Gw2Launcher.UI.Controls.StackPanel();
            this.label7 = new System.Windows.Forms.Label();
            this.radioGw2 = new System.Windows.Forms.RadioButton();
            this.radioGw1 = new System.Windows.Forms.RadioButton();
            this.panelFiles = new Gw2Launcher.UI.Controls.StackPanel();
            this.label2 = new System.Windows.Forms.Label();
            this.radioFilesCopy = new System.Windows.Forms.RadioButton();
            this.radioFilesMove = new System.Windows.Forms.RadioButton();
            this.panelImportConfirm = new Gw2Launcher.UI.Controls.StackPanel();
            this.label8 = new System.Windows.Forms.Label();
            this.checkImportLastUsed = new System.Windows.Forms.CheckBox();
            this.panelMappingsScroll = new Gw2Launcher.UI.Controls.AutoScrollContainerPanel();
            this.panelMappings = new Gw2Launcher.UI.Controls.StackPanel();
            this.label11 = new Gw2Launcher.UI.Controls.GradientLabel();
            this.tableMapping = new System.Windows.Forms.TableLayoutPanel();
            this.label4 = new System.Windows.Forms.Label();
            this.label5 = new System.Windows.Forms.Label();
            this.label6 = new System.Windows.Forms.Label();
            this.labelTemplateField = new System.Windows.Forms.Label();
            this.comboTemplateMapping = new System.Windows.Forms.ComboBox();
            this.labelTemplateSample = new System.Windows.Forms.Label();
            this.labelNoMappings = new System.Windows.Forms.Label();
            this.labelWarningBlankMapping = new System.Windows.Forms.Label();
            this.labelWarningIgnoredMappings = new System.Windows.Forms.Label();
            this.contextAccounts.SuspendLayout();
            this.panelNav.SuspendLayout();
            this.panelImportScroll.SuspendLayout();
            this.panelImport.SuspendLayout();
            this.panelBrowse.SuspendLayout();
            this.stackPanel1.SuspendLayout();
            this.panelAccounts.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.gridAccounts)).BeginInit();
            this.panelRequiredScroll.SuspendLayout();
            this.panelRequired.SuspendLayout();
            this.panelType.SuspendLayout();
            this.panelFiles.SuspendLayout();
            this.panelImportConfirm.SuspendLayout();
            this.panelMappingsScroll.SuspendLayout();
            this.panelMappings.SuspendLayout();
            this.tableMapping.SuspendLayout();
            this.SuspendLayout();
            // 
            // contextAccounts
            // 
            this.contextAccounts.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.removeToolStripMenuItem});
            this.contextAccounts.Size = new System.Drawing.Size(164, 26);
            // 
            // removeToolStripMenuItem
            // 
            this.removeToolStripMenuItem.Size = new System.Drawing.Size(163, 22);
            this.removeToolStripMenuItem.Text = "Remove selected";
            this.removeToolStripMenuItem.Click += new System.EventHandler(this.removeToolStripMenuItem_Click);
            // 
            // panel1
            // 
            this.panel1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.panel1.BackColor = System.Drawing.SystemColors.ControlDark;
            this.panel1.Location = new System.Drawing.Point(0, 267);
            this.panel1.Size = new System.Drawing.Size(522, 1);
            // 
            // panelNav
            // 
            this.panelNav.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.panelNav.AutoSize = true;
            this.panelNav.BackColor = System.Drawing.SystemColors.Control;
            this.panelNav.Controls.Add(this.buttonCancel);
            this.panelNav.Controls.Add(this.buttonBack);
            this.panelNav.Controls.Add(this.buttonNext);
            this.panelNav.Controls.Add(this.buttonImport);
            this.panelNav.FlowDirection = System.Windows.Forms.FlowDirection.LeftToRight;
            this.panelNav.Location = new System.Drawing.Point(145, 275);
            this.panelNav.Margin = new System.Windows.Forms.Padding(0);
            this.panelNav.Size = new System.Drawing.Size(368, 41);
            // 
            // buttonCancel
            // 
            this.buttonCancel.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.buttonCancel.Font = new System.Drawing.Font("Segoe UI", 8.25F);
            this.buttonCancel.Location = new System.Drawing.Point(3, 3);
            this.buttonCancel.Size = new System.Drawing.Size(86, 35);
            this.buttonCancel.Text = "Cancel";
            this.buttonCancel.UseVisualStyleBackColor = true;
            this.buttonCancel.Click += new System.EventHandler(this.buttonCancel_Click);
            // 
            // buttonBack
            // 
            this.buttonBack.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.buttonBack.Font = new System.Drawing.Font("Segoe UI", 8.25F);
            this.buttonBack.Location = new System.Drawing.Point(95, 3);
            this.buttonBack.Size = new System.Drawing.Size(86, 35);
            this.buttonBack.Text = "Back";
            this.buttonBack.UseVisualStyleBackColor = true;
            this.buttonBack.Visible = false;
            this.buttonBack.Click += new System.EventHandler(this.buttonBack_Click);
            // 
            // buttonNext
            // 
            this.buttonNext.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.buttonNext.Font = new System.Drawing.Font("Segoe UI", 8.25F);
            this.buttonNext.Location = new System.Drawing.Point(187, 3);
            this.buttonNext.Size = new System.Drawing.Size(86, 35);
            this.buttonNext.Text = "Next";
            this.buttonNext.UseVisualStyleBackColor = true;
            this.buttonNext.Click += new System.EventHandler(this.buttonNext_Click);
            // 
            // buttonImport
            // 
            this.buttonImport.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.buttonImport.Font = new System.Drawing.Font("Segoe UI", 8.25F);
            this.buttonImport.Location = new System.Drawing.Point(279, 3);
            this.buttonImport.Size = new System.Drawing.Size(86, 35);
            this.buttonImport.Text = "Import";
            this.buttonImport.UseVisualStyleBackColor = true;
            this.buttonImport.Visible = false;
            this.buttonImport.Click += new System.EventHandler(this.buttonImport_Click);
            // 
            // panelImportScroll
            // 
            this.panelImportScroll.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.panelImportScroll.Controls.Add(this.panelImport);
            this.panelImportScroll.Location = new System.Drawing.Point(0, 10);
            this.panelImportScroll.Size = new System.Drawing.Size(522, 251);
            this.panelImportScroll.Visible = false;
            // 
            // panelImport
            // 
            this.panelImport.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.panelImport.AutoSize = true;
            this.panelImport.AutoSizeFill = Gw2Launcher.UI.Controls.StackPanel.AutoSizeFillMode.Width;
            this.panelImport.Controls.Add(this.label12);
            this.panelImport.Controls.Add(this.panelBrowse);
            this.panelImport.Location = new System.Drawing.Point(0, 0);
            this.panelImport.Margin = new System.Windows.Forms.Padding(0);
            this.panelImport.Size = new System.Drawing.Size(522, 116);
            // 
            // label12
            // 
            this.label12.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.label12.AutoSize = true;
            this.label12.Font = new System.Drawing.Font("Segoe UI Semibold", 9.75F, System.Drawing.FontStyle.Bold);
            this.label12.Location = new System.Drawing.Point(0, 0);
            this.label12.Margin = new System.Windows.Forms.Padding(0, 0, 0, 10);
            this.label12.Padding = new System.Windows.Forms.Padding(10, 10, 0, 10);
            this.label12.Size = new System.Drawing.Size(522, 37);
            this.label12.Text = "Import";
            // 
            // panelBrowse
            // 
            this.panelBrowse.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.panelBrowse.AutoSize = true;
            this.panelBrowse.AutoSizeFill = Gw2Launcher.UI.Controls.StackPanel.AutoSizeFillMode.Width;
            this.panelBrowse.Controls.Add(this.label9);
            this.panelBrowse.Controls.Add(this.label10);
            this.panelBrowse.Controls.Add(this.stackPanel1);
            this.panelBrowse.Location = new System.Drawing.Point(10, 47);
            this.panelBrowse.Margin = new System.Windows.Forms.Padding(10, 0, 10, 0);
            this.panelBrowse.Size = new System.Drawing.Size(502, 69);
            // 
            // label9
            // 
            this.label9.AutoSize = true;
            this.label9.Font = new System.Drawing.Font("Segoe UI Semibold", 9F, System.Drawing.FontStyle.Bold);
            this.label9.Location = new System.Drawing.Point(0, 0);
            this.label9.Margin = new System.Windows.Forms.Padding(0, 0, 0, 3);
            this.label9.Size = new System.Drawing.Size(120, 15);
            this.label9.Text = "Select a file to import";
            // 
            // label10
            // 
            this.label10.AutoSize = true;
            this.label10.Font = new System.Drawing.Font("Segoe UI Semilight", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label10.Location = new System.Drawing.Point(1, 19);
            this.label10.Margin = new System.Windows.Forms.Padding(1, 1, 0, 13);
            this.label10.Size = new System.Drawing.Size(98, 13);
            this.label10.Text = "XML or CSV format";
            // 
            // stackPanel1
            // 
            this.stackPanel1.AutoSize = true;
            this.stackPanel1.Controls.Add(this.textBrowse);
            this.stackPanel1.Controls.Add(this.buttonBrowse);
            this.stackPanel1.FlowDirection = System.Windows.Forms.FlowDirection.LeftToRight;
            this.stackPanel1.Location = new System.Drawing.Point(8, 45);
            this.stackPanel1.Margin = new System.Windows.Forms.Padding(8, 0, 0, 0);
            this.stackPanel1.Size = new System.Drawing.Size(352, 24);
            // 
            // textBrowse
            // 
            this.textBrowse.Location = new System.Drawing.Point(0, 1);
            this.textBrowse.Margin = new System.Windows.Forms.Padding(0, 1, 0, 1);
            this.textBrowse.ReadOnly = true;
            this.textBrowse.Size = new System.Drawing.Size(300, 22);
            // 
            // buttonBrowse
            // 
            this.buttonBrowse.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left)));
            this.buttonBrowse.AutoSize = true;
            this.buttonBrowse.Font = new System.Drawing.Font("Segoe UI", 8.25F);
            this.buttonBrowse.Location = new System.Drawing.Point(306, 0);
            this.buttonBrowse.Margin = new System.Windows.Forms.Padding(6, 0, 0, 0);
            this.buttonBrowse.Size = new System.Drawing.Size(46, 24);
            this.buttonBrowse.Text = "...";
            this.buttonBrowse.UseVisualStyleBackColor = true;
            this.buttonBrowse.Click += new System.EventHandler(this.buttonBrowse_Click);
            // 
            // panelAccounts
            // 
            this.panelAccounts.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.panelAccounts.AutoSizeFill = Gw2Launcher.UI.Controls.StackPanel.AutoSizeFillMode.Width;
            this.panelAccounts.Controls.Add(this.label13);
            this.panelAccounts.Controls.Add(this.gridAccounts);
            this.panelAccounts.Location = new System.Drawing.Point(0, 10);
            this.panelAccounts.Size = new System.Drawing.Size(522, 251);
            this.panelAccounts.Visible = false;
            // 
            // label13
            // 
            this.label13.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.label13.AutoSize = true;
            this.label13.Font = new System.Drawing.Font("Segoe UI Semibold", 9.75F, System.Drawing.FontStyle.Bold);
            this.label13.Location = new System.Drawing.Point(0, 0);
            this.label13.Margin = new System.Windows.Forms.Padding(0, 0, 0, 10);
            this.label13.Padding = new System.Windows.Forms.Padding(10, 10, 0, 10);
            this.label13.Size = new System.Drawing.Size(522, 37);
            this.label13.Text = "Accounts";
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
            this.columnName});
            dataGridViewCellStyle2.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle2.BackColor = System.Drawing.SystemColors.Window;
            dataGridViewCellStyle2.Font = new System.Drawing.Font("Segoe UI", 8.25F);
            dataGridViewCellStyle2.ForeColor = System.Drawing.SystemColors.ControlText;
            dataGridViewCellStyle2.SelectionBackColor = System.Drawing.SystemColors.ControlLight;
            dataGridViewCellStyle2.SelectionForeColor = System.Drawing.SystemColors.ControlText;
            dataGridViewCellStyle2.WrapMode = System.Windows.Forms.DataGridViewTriState.False;
            this.gridAccounts.DefaultCellStyle = dataGridViewCellStyle2;
            this.gridAccounts.EditMode = System.Windows.Forms.DataGridViewEditMode.EditProgrammatically;
            this.gridAccounts.Location = new System.Drawing.Point(10, 47);
            this.gridAccounts.Margin = new System.Windows.Forms.Padding(10, 0, 10, 5);
            this.gridAccounts.ReadOnly = true;
            this.gridAccounts.RowHeadersVisible = false;
            this.gridAccounts.RowHighlightDeselectedColor = System.Drawing.Color.FromArgb(((int)(((byte)(222)))), ((int)(((byte)(176)))), ((int)(((byte)(196)))));
            this.gridAccounts.RowHighlightSelectedColor = System.Drawing.Color.LightSteelBlue;
            this.gridAccounts.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.gridAccounts.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.gridAccounts.Size = new System.Drawing.Size(502, 199);
            this.gridAccounts.CellMouseDown += new System.Windows.Forms.DataGridViewCellMouseEventHandler(this.gridAccounts_CellMouseDown);
            // 
            // columnName
            // 
            this.columnName.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
            this.columnName.HeaderText = "";
            this.columnName.ReadOnly = true;
            // 
            // panelRequiredScroll
            // 
            this.panelRequiredScroll.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.panelRequiredScroll.Controls.Add(this.panelRequired);
            this.panelRequiredScroll.Location = new System.Drawing.Point(0, 10);
            this.panelRequiredScroll.Size = new System.Drawing.Size(522, 251);
            this.panelRequiredScroll.Visible = false;
            // 
            // panelRequired
            // 
            this.panelRequired.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.panelRequired.AutoSize = true;
            this.panelRequired.AutoSizeFill = Gw2Launcher.UI.Controls.StackPanel.AutoSizeFillMode.Width;
            this.panelRequired.Controls.Add(this.label3);
            this.panelRequired.Controls.Add(this.panelType);
            this.panelRequired.Controls.Add(this.panelFiles);
            this.panelRequired.Controls.Add(this.panelImportConfirm);
            this.panelRequired.Location = new System.Drawing.Point(0, 0);
            this.panelRequired.Margin = new System.Windows.Forms.Padding(0);
            this.panelRequired.Size = new System.Drawing.Size(522, 246);
            // 
            // label3
            // 
            this.label3.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.label3.AutoSize = true;
            this.label3.Font = new System.Drawing.Font("Segoe UI Semibold", 9.75F, System.Drawing.FontStyle.Bold);
            this.label3.Location = new System.Drawing.Point(0, 0);
            this.label3.Margin = new System.Windows.Forms.Padding(0, 0, 0, 10);
            this.label3.Padding = new System.Windows.Forms.Padding(10, 10, 0, 10);
            this.label3.Size = new System.Drawing.Size(522, 37);
            this.label3.Text = "Options";
            // 
            // panelType
            // 
            this.panelType.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.panelType.AutoSize = true;
            this.panelType.Controls.Add(this.label7);
            this.panelType.Controls.Add(this.radioGw2);
            this.panelType.Controls.Add(this.radioGw1);
            this.panelType.Location = new System.Drawing.Point(10, 47);
            this.panelType.Margin = new System.Windows.Forms.Padding(10, 0, 10, 13);
            this.panelType.Size = new System.Drawing.Size(502, 60);
            this.panelType.Visible = false;
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Font = new System.Drawing.Font("Segoe UI Semibold", 9F, System.Drawing.FontStyle.Bold);
            this.label7.Location = new System.Drawing.Point(0, 0);
            this.label7.Margin = new System.Windows.Forms.Padding(0);
            this.label7.Size = new System.Drawing.Size(78, 15);
            this.label7.Text = "Account type";
            // 
            // radioGw2
            // 
            this.radioGw2.AutoSize = true;
            this.radioGw2.Checked = true;
            this.radioGw2.Location = new System.Drawing.Point(8, 23);
            this.radioGw2.Margin = new System.Windows.Forms.Padding(8, 8, 0, 0);
            this.radioGw2.Size = new System.Drawing.Size(91, 17);
            this.radioGw2.TabStop = true;
            this.radioGw2.Text = "Guild Wars 2";
            this.radioGw2.UseVisualStyleBackColor = true;
            // 
            // radioGw1
            // 
            this.radioGw1.AutoSize = true;
            this.radioGw1.Location = new System.Drawing.Point(8, 43);
            this.radioGw1.Margin = new System.Windows.Forms.Padding(8, 3, 0, 0);
            this.radioGw1.Size = new System.Drawing.Size(91, 17);
            this.radioGw1.Text = "Guild Wars 1";
            this.radioGw1.UseVisualStyleBackColor = true;
            // 
            // panelFiles
            // 
            this.panelFiles.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.panelFiles.AutoSize = true;
            this.panelFiles.Controls.Add(this.label2);
            this.panelFiles.Controls.Add(this.radioFilesCopy);
            this.panelFiles.Controls.Add(this.radioFilesMove);
            this.panelFiles.Location = new System.Drawing.Point(10, 120);
            this.panelFiles.Margin = new System.Windows.Forms.Padding(10, 0, 10, 13);
            this.panelFiles.Size = new System.Drawing.Size(502, 60);
            this.panelFiles.Visible = false;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Font = new System.Drawing.Font("Segoe UI Semibold", 9F, System.Drawing.FontStyle.Bold);
            this.label2.Location = new System.Drawing.Point(0, 0);
            this.label2.Margin = new System.Windows.Forms.Padding(0);
            this.label2.Size = new System.Drawing.Size(168, 15);
            this.label2.Text = "How should files be imported?";
            // 
            // radioFilesCopy
            // 
            this.radioFilesCopy.AutoSize = true;
            this.radioFilesCopy.Checked = true;
            this.radioFilesCopy.Location = new System.Drawing.Point(8, 23);
            this.radioFilesCopy.Margin = new System.Windows.Forms.Padding(8, 8, 0, 0);
            this.radioFilesCopy.Size = new System.Drawing.Size(51, 17);
            this.radioFilesCopy.TabStop = true;
            this.radioFilesCopy.Text = "Copy";
            this.radioFilesCopy.UseVisualStyleBackColor = true;
            // 
            // radioFilesMove
            // 
            this.radioFilesMove.AutoSize = true;
            this.radioFilesMove.Location = new System.Drawing.Point(8, 43);
            this.radioFilesMove.Margin = new System.Windows.Forms.Padding(8, 3, 0, 0);
            this.radioFilesMove.Size = new System.Drawing.Size(53, 17);
            this.radioFilesMove.Text = "Move";
            this.radioFilesMove.UseVisualStyleBackColor = true;
            // 
            // panelImportConfirm
            // 
            this.panelImportConfirm.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.panelImportConfirm.AutoSize = true;
            this.panelImportConfirm.Controls.Add(this.label8);
            this.panelImportConfirm.Controls.Add(this.checkImportLastUsed);
            this.panelImportConfirm.Location = new System.Drawing.Point(10, 193);
            this.panelImportConfirm.Margin = new System.Windows.Forms.Padding(10, 0, 10, 13);
            this.panelImportConfirm.Size = new System.Drawing.Size(502, 40);
            this.panelImportConfirm.Visible = false;
            // 
            // label8
            // 
            this.label8.AutoSize = true;
            this.label8.Font = new System.Drawing.Font("Segoe UI Semibold", 9F, System.Drawing.FontStyle.Bold);
            this.label8.Location = new System.Drawing.Point(0, 0);
            this.label8.Margin = new System.Windows.Forms.Padding(0, 0, 0, 5);
            this.label8.Size = new System.Drawing.Size(256, 15);
            this.label8.Text = "Are you sure you want to import the following?";
            // 
            // checkImportLastUsed
            // 
            this.checkImportLastUsed.AutoSize = true;
            this.checkImportLastUsed.Location = new System.Drawing.Point(8, 23);
            this.checkImportLastUsed.Margin = new System.Windows.Forms.Padding(8, 3, 0, 0);
            this.checkImportLastUsed.Size = new System.Drawing.Size(100, 17);
            this.checkImportLastUsed.Text = "Last used date";
            this.checkImportLastUsed.UseVisualStyleBackColor = true;
            // 
            // panelMappingsScroll
            // 
            this.panelMappingsScroll.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.panelMappingsScroll.Controls.Add(this.panelMappings);
            this.panelMappingsScroll.Location = new System.Drawing.Point(0, 10);
            this.panelMappingsScroll.Size = new System.Drawing.Size(522, 251);
            this.panelMappingsScroll.Visible = false;
            // 
            // panelMappings
            // 
            this.panelMappings.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.panelMappings.AutoSize = true;
            this.panelMappings.Controls.Add(this.label11);
            this.panelMappings.Controls.Add(this.tableMapping);
            this.panelMappings.Controls.Add(this.labelNoMappings);
            this.panelMappings.Controls.Add(this.labelWarningBlankMapping);
            this.panelMappings.Controls.Add(this.labelWarningIgnoredMappings);
            this.panelMappings.Location = new System.Drawing.Point(0, 0);
            this.panelMappings.Margin = new System.Windows.Forms.Padding(0);
            this.panelMappings.Size = new System.Drawing.Size(522, 175);
            // 
            // label11
            // 
            this.label11.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.label11.AutoSize = true;
            this.label11.Font = new System.Drawing.Font("Segoe UI Semibold", 9.75F, System.Drawing.FontStyle.Bold);
            this.label11.Location = new System.Drawing.Point(0, 0);
            this.label11.Margin = new System.Windows.Forms.Padding(0, 0, 0, 10);
            this.label11.Padding = new System.Windows.Forms.Padding(10, 10, 0, 10);
            this.label11.Size = new System.Drawing.Size(522, 37);
            this.label11.Text = "Mappings";
            // 
            // tableMapping
            // 
            this.tableMapping.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.tableMapping.AutoSize = true;
            this.tableMapping.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.tableMapping.ColumnCount = 3;
            this.tableMapping.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tableMapping.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tableMapping.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableMapping.Controls.Add(this.label4, 2, 0);
            this.tableMapping.Controls.Add(this.label5, 1, 0);
            this.tableMapping.Controls.Add(this.label6, 0, 0);
            this.tableMapping.Controls.Add(this.labelTemplateField, 0, 1);
            this.tableMapping.Controls.Add(this.comboTemplateMapping, 1, 1);
            this.tableMapping.Controls.Add(this.labelTemplateSample, 2, 1);
            this.tableMapping.Location = new System.Drawing.Point(10, 47);
            this.tableMapping.Margin = new System.Windows.Forms.Padding(10, 0, 10, 0);
            this.tableMapping.RowCount = 2;
            this.tableMapping.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableMapping.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableMapping.Size = new System.Drawing.Size(502, 56);
            // 
            // label4
            // 
            this.label4.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.label4.Font = new System.Drawing.Font("Segoe UI Semibold", 9F, System.Drawing.FontStyle.Bold);
            this.label4.Location = new System.Drawing.Point(244, 0);
            this.label4.Margin = new System.Windows.Forms.Padding(0, 0, 0, 10);
            this.label4.Size = new System.Drawing.Size(258, 15);
            this.label4.Text = "Sample";
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Font = new System.Drawing.Font("Segoe UI Semibold", 9F, System.Drawing.FontStyle.Bold);
            this.label5.Location = new System.Drawing.Point(49, 0);
            this.label5.Margin = new System.Windows.Forms.Padding(5, 0, 0, 10);
            this.label5.Size = new System.Drawing.Size(55, 15);
            this.label5.Text = "Mapping";
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Font = new System.Drawing.Font("Segoe UI Semibold", 9F, System.Drawing.FontStyle.Bold);
            this.label6.Location = new System.Drawing.Point(0, 0);
            this.label6.Margin = new System.Windows.Forms.Padding(0, 0, 0, 10);
            this.label6.Size = new System.Drawing.Size(44, 15);
            this.label6.Text = "Source";
            // 
            // labelTemplateField
            // 
            this.labelTemplateField.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.labelTemplateField.AutoSize = true;
            this.labelTemplateField.Location = new System.Drawing.Point(0, 34);
            this.labelTemplateField.Margin = new System.Windows.Forms.Padding(0);
            this.labelTemplateField.Size = new System.Drawing.Size(11, 13);
            this.labelTemplateField.Text = "-";
            this.labelTemplateField.Visible = false;
            // 
            // comboTemplateMapping
            // 
            this.comboTemplateMapping.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.comboTemplateMapping.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboTemplateMapping.FormattingEnabled = true;
            this.comboTemplateMapping.Location = new System.Drawing.Point(54, 30);
            this.comboTemplateMapping.Margin = new System.Windows.Forms.Padding(10, 5, 10, 5);
            this.comboTemplateMapping.Size = new System.Drawing.Size(180, 21);
            this.comboTemplateMapping.Visible = false;
            // 
            // labelTemplateSample
            // 
            this.labelTemplateSample.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.labelTemplateSample.AutoEllipsis = true;
            this.labelTemplateSample.ForeColor = System.Drawing.SystemColors.GrayText;
            this.labelTemplateSample.Location = new System.Drawing.Point(244, 34);
            this.labelTemplateSample.Margin = new System.Windows.Forms.Padding(0);
            this.labelTemplateSample.Size = new System.Drawing.Size(258, 13);
            this.labelTemplateSample.Text = "-";
            this.labelTemplateSample.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.labelTemplateSample.UseMnemonic = false;
            this.labelTemplateSample.Visible = false;
            // 
            // labelNoMappings
            // 
            this.labelNoMappings.Anchor = System.Windows.Forms.AnchorStyles.Top;
            this.labelNoMappings.AutoSize = true;
            this.labelNoMappings.ForeColor = System.Drawing.SystemColors.GrayText;
            this.labelNoMappings.Location = new System.Drawing.Point(209, 123);
            this.labelNoMappings.Margin = new System.Windows.Forms.Padding(10, 20, 0, 0);
            this.labelNoMappings.Size = new System.Drawing.Size(124, 13);
            this.labelNoMappings.Text = "No mappings available";
            this.labelNoMappings.Visible = false;
            // 
            // labelWarningBlankMapping
            // 
            this.labelWarningBlankMapping.AutoSize = true;
            this.labelWarningBlankMapping.Font = new System.Drawing.Font("Segoe UI Semilight", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.labelWarningBlankMapping.ForeColor = System.Drawing.SystemColors.GrayText;
            this.labelWarningBlankMapping.Location = new System.Drawing.Point(10, 146);
            this.labelWarningBlankMapping.Margin = new System.Windows.Forms.Padding(10, 10, 0, 0);
            this.labelWarningBlankMapping.Size = new System.Drawing.Size(151, 13);
            this.labelWarningBlankMapping.Text = "Blank mappings will be ignored";
            // 
            // labelWarningIgnoredMappings
            // 
            this.labelWarningIgnoredMappings.AutoSize = true;
            this.labelWarningIgnoredMappings.Font = new System.Drawing.Font("Segoe UI Semilight", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.labelWarningIgnoredMappings.ForeColor = System.Drawing.SystemColors.GrayText;
            this.labelWarningIgnoredMappings.Location = new System.Drawing.Point(10, 162);
            this.labelWarningIgnoredMappings.Margin = new System.Windows.Forms.Padding(10, 3, 0, 0);
            this.labelWarningIgnoredMappings.Size = new System.Drawing.Size(178, 13);
            this.labelWarningIgnoredMappings.Text = "ID and Created can not be imported";
            // 
            // formAccountImport
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
            this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(248)))), ((int)(((byte)(248)))), ((int)(((byte)(248)))));
            this.ClientSize = new System.Drawing.Size(522, 325);
            this.Controls.Add(this.panel1);
            this.Controls.Add(this.panelNav);
            this.Controls.Add(this.panelMappingsScroll);
            this.Controls.Add(this.panelImportScroll);
            this.Controls.Add(this.panelAccounts);
            this.Controls.Add(this.panelRequiredScroll);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.MinimumSize = new System.Drawing.Size(400, 320);
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.contextAccounts.ResumeLayout(false);
            this.panelNav.ResumeLayout(false);
            this.panelImportScroll.ResumeLayout(false);
            this.panelImportScroll.PerformLayout();
            this.panelImport.ResumeLayout(false);
            this.panelImport.PerformLayout();
            this.panelBrowse.ResumeLayout(false);
            this.panelBrowse.PerformLayout();
            this.stackPanel1.ResumeLayout(false);
            this.stackPanel1.PerformLayout();
            this.panelAccounts.ResumeLayout(false);
            this.panelAccounts.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.gridAccounts)).EndInit();
            this.panelRequiredScroll.ResumeLayout(false);
            this.panelRequiredScroll.PerformLayout();
            this.panelRequired.ResumeLayout(false);
            this.panelRequired.PerformLayout();
            this.panelType.ResumeLayout(false);
            this.panelType.PerformLayout();
            this.panelFiles.ResumeLayout(false);
            this.panelFiles.PerformLayout();
            this.panelImportConfirm.ResumeLayout(false);
            this.panelImportConfirm.PerformLayout();
            this.panelMappingsScroll.ResumeLayout(false);
            this.panelMappingsScroll.PerformLayout();
            this.panelMappings.ResumeLayout(false);
            this.panelMappings.PerformLayout();
            this.tableMapping.ResumeLayout(false);
            this.tableMapping.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private Controls.StackPanel panelNav;
        private System.Windows.Forms.Button buttonCancel;
        private System.Windows.Forms.Button buttonNext;
        private Controls.StackPanel panelBrowse;
        private Controls.StackPanel panelType;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.RadioButton radioGw2;
        private System.Windows.Forms.RadioButton radioGw1;
        private Controls.StackPanel panelFiles;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.RadioButton radioFilesMove;
        private System.Windows.Forms.RadioButton radioFilesCopy;
        private Controls.AutoScrollContainerPanel panelImportScroll;
        private System.Windows.Forms.TableLayoutPanel tableMapping;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.Label labelTemplateField;
        private System.Windows.Forms.ComboBox comboTemplateMapping;
        private System.Windows.Forms.Label labelTemplateSample;
        private Controls.SelectionDataGridView gridAccounts;
        private System.Windows.Forms.Button buttonBack;
        private System.Windows.Forms.Button buttonImport;
        private System.Windows.Forms.DataGridViewTextBoxColumn columnName;
        private Controls.StackPanel panelImportConfirm;
        private System.Windows.Forms.Label label8;
        private System.Windows.Forms.CheckBox checkImportLastUsed;
        private System.Windows.Forms.ContextMenuStrip contextAccounts;
        private System.Windows.Forms.ToolStripMenuItem removeToolStripMenuItem;
        private System.Windows.Forms.Label label9;
        private System.Windows.Forms.Label label10;
        private UI.Controls.GradientLabel label11;
        private UI.Controls.GradientLabel label3;
        private UI.Controls.GradientLabel label12;
        private Controls.StackPanel panelImport;
        private Controls.StackPanel panelMappings;
        private Controls.StackPanel panelRequired;
        private Controls.StackPanel panelAccounts;
        private UI.Controls.GradientLabel label13;
        private Controls.StackPanel stackPanel1;
        private System.Windows.Forms.TextBox textBrowse;
        private System.Windows.Forms.Button buttonBrowse;
        private Controls.AutoScrollContainerPanel panelMappingsScroll;
        private Controls.AutoScrollContainerPanel panelRequiredScroll;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.Label labelWarningBlankMapping;
        private System.Windows.Forms.Label labelWarningIgnoredMappings;
        private System.Windows.Forms.Label labelNoMappings;

    }
}
