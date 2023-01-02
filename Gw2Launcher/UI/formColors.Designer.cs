namespace Gw2Launcher.UI
{
    partial class formColors
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
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle2 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle1 = new System.Windows.Forms.DataGridViewCellStyle();
            this.stackPanel1 = new Gw2Launcher.UI.Controls.StackPanel();
            this.gridColors = new Gw2Launcher.UI.Controls.ScaledDataGridView();
            this.columnColor = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.columnName = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.textColorsFilter = new System.Windows.Forms.TextBox();
            this.menuStrip1 = new System.Windows.Forms.MenuStrip();
            this.fileToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.importToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.exportToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem1 = new System.Windows.Forms.ToolStripSeparator();
            this.saveToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.optionsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.previewToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.changeAllSharedColorsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem3 = new System.Windows.Forms.ToolStripSeparator();
            this.defaultToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.lightToolStripMenuItem1 = new System.Windows.Forms.ToolStripMenuItem();
            this.darkToolStripMenuItem1 = new System.Windows.Forms.ToolStripMenuItem();
            this.presetsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.lightToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.darkToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem2 = new System.Windows.Forms.ToolStripSeparator();
            this.resetToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.contextColor = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.showSharedColorsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.stackPanel1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.gridColors)).BeginInit();
            this.menuStrip1.SuspendLayout();
            this.contextColor.SuspendLayout();
            this.SuspendLayout();
            // 
            // stackPanel1
            // 
            this.stackPanel1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.stackPanel1.Controls.Add(this.gridColors);
            this.stackPanel1.Controls.Add(this.textColorsFilter);
            this.stackPanel1.Location = new System.Drawing.Point(12, 27);
            this.stackPanel1.Size = new System.Drawing.Size(366, 327);
            // 
            // gridColors
            // 
            this.gridColors.AllowUserToAddRows = false;
            this.gridColors.AllowUserToDeleteRows = false;
            this.gridColors.AllowUserToResizeColumns = false;
            this.gridColors.AllowUserToResizeRows = false;
            this.gridColors.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.gridColors.BackgroundColor = System.Drawing.SystemColors.Window;
            this.gridColors.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.gridColors.CellBorderStyle = System.Windows.Forms.DataGridViewCellBorderStyle.None;
            this.gridColors.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.gridColors.ColumnHeadersVisible = false;
            this.gridColors.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.columnColor,
            this.columnName});
            dataGridViewCellStyle2.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle2.BackColor = System.Drawing.SystemColors.Window;
            dataGridViewCellStyle2.Font = new System.Drawing.Font("Segoe UI", 8.25F);
            dataGridViewCellStyle2.ForeColor = System.Drawing.SystemColors.ControlText;
            dataGridViewCellStyle2.SelectionBackColor = System.Drawing.SystemColors.ControlLight;
            dataGridViewCellStyle2.SelectionForeColor = System.Drawing.SystemColors.ControlText;
            dataGridViewCellStyle2.WrapMode = System.Windows.Forms.DataGridViewTriState.False;
            this.gridColors.DefaultCellStyle = dataGridViewCellStyle2;
            this.gridColors.EditMode = System.Windows.Forms.DataGridViewEditMode.EditProgrammatically;
            this.gridColors.Location = new System.Drawing.Point(0, 0);
            this.gridColors.Margin = new System.Windows.Forms.Padding(0);
            this.gridColors.MultiSelect = false;
            this.gridColors.ReadOnly = true;
            this.gridColors.RowHeadersVisible = false;
            this.gridColors.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.gridColors.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.gridColors.Size = new System.Drawing.Size(366, 299);
            this.gridColors.CellClick += new System.Windows.Forms.DataGridViewCellEventHandler(this.gridColors_CellClick);
            this.gridColors.CellMouseDown += new System.Windows.Forms.DataGridViewCellMouseEventHandler(this.gridColors_CellMouseDown);
            this.gridColors.CellPainting += new System.Windows.Forms.DataGridViewCellPaintingEventHandler(this.gridColors_CellPainting);
            // 
            // columnColor
            // 
            dataGridViewCellStyle1.Padding = new System.Windows.Forms.Padding(5, 2, 0, 0);
            this.columnColor.DefaultCellStyle = dataGridViewCellStyle1;
            this.columnColor.HeaderText = "";
            this.columnColor.ReadOnly = true;
            this.columnColor.Width = 30;
            // 
            // columnName
            // 
            this.columnName.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
            this.columnName.HeaderText = "";
            this.columnName.ReadOnly = true;
            // 
            // textColorsFilter
            // 
            this.textColorsFilter.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.textColorsFilter.Location = new System.Drawing.Point(0, 305);
            this.textColorsFilter.Margin = new System.Windows.Forms.Padding(0, 6, 0, 0);
            this.textColorsFilter.Size = new System.Drawing.Size(366, 22);
            this.textColorsFilter.TextChanged += new System.EventHandler(this.textColorsFilter_TextChanged);
            // 
            // menuStrip1
            // 
            this.menuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.fileToolStripMenuItem,
            this.optionsToolStripMenuItem,
            this.presetsToolStripMenuItem});
            this.menuStrip1.Location = new System.Drawing.Point(0, 0);
            this.menuStrip1.Size = new System.Drawing.Size(390, 24);
            this.menuStrip1.Text = "menuStrip1";
            // 
            // fileToolStripMenuItem
            // 
            this.fileToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.importToolStripMenuItem,
            this.exportToolStripMenuItem,
            this.toolStripMenuItem1,
            this.saveToolStripMenuItem});
            this.fileToolStripMenuItem.Size = new System.Drawing.Size(37, 20);
            this.fileToolStripMenuItem.Text = "File";
            // 
            // importToolStripMenuItem
            // 
            this.importToolStripMenuItem.Size = new System.Drawing.Size(152, 22);
            this.importToolStripMenuItem.Text = "Import";
            this.importToolStripMenuItem.Click += new System.EventHandler(this.importToolStripMenuItem_Click);
            // 
            // exportToolStripMenuItem
            // 
            this.exportToolStripMenuItem.Size = new System.Drawing.Size(152, 22);
            this.exportToolStripMenuItem.Text = "Export";
            this.exportToolStripMenuItem.Click += new System.EventHandler(this.exportToolStripMenuItem_Click);
            // 
            // toolStripMenuItem1
            // 
            this.toolStripMenuItem1.Size = new System.Drawing.Size(149, 6);
            // 
            // saveToolStripMenuItem
            // 
            this.saveToolStripMenuItem.Size = new System.Drawing.Size(152, 22);
            this.saveToolStripMenuItem.Text = "Save";
            this.saveToolStripMenuItem.Click += new System.EventHandler(this.saveToolStripMenuItem_Click);
            // 
            // optionsToolStripMenuItem
            // 
            this.optionsToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.previewToolStripMenuItem,
            this.changeAllSharedColorsToolStripMenuItem,
            this.toolStripMenuItem3,
            this.defaultToolStripMenuItem});
            this.optionsToolStripMenuItem.Size = new System.Drawing.Size(61, 20);
            this.optionsToolStripMenuItem.Text = "Options";
            // 
            // previewToolStripMenuItem
            // 
            this.previewToolStripMenuItem.Checked = true;
            this.previewToolStripMenuItem.CheckState = System.Windows.Forms.CheckState.Checked;
            this.previewToolStripMenuItem.Size = new System.Drawing.Size(203, 22);
            this.previewToolStripMenuItem.Text = "Preview";
            this.previewToolStripMenuItem.Click += new System.EventHandler(this.previewToolStripMenuItem_Click);
            // 
            // changeAllSharedColorsToolStripMenuItem
            // 
            this.changeAllSharedColorsToolStripMenuItem.Size = new System.Drawing.Size(203, 22);
            this.changeAllSharedColorsToolStripMenuItem.Text = "Change all shared colors";
            this.changeAllSharedColorsToolStripMenuItem.Click += new System.EventHandler(this.changeAllSharedColorsToolStripMenuItem_Click);
            // 
            // toolStripMenuItem3
            // 
            this.toolStripMenuItem3.Size = new System.Drawing.Size(200, 6);
            // 
            // defaultToolStripMenuItem
            // 
            this.defaultToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.lightToolStripMenuItem1,
            this.darkToolStripMenuItem1});
            this.defaultToolStripMenuItem.Size = new System.Drawing.Size(203, 22);
            this.defaultToolStripMenuItem.Text = "Defaults";
            // 
            // lightToolStripMenuItem1
            // 
            this.lightToolStripMenuItem1.Checked = true;
            this.lightToolStripMenuItem1.CheckState = System.Windows.Forms.CheckState.Checked;
            this.lightToolStripMenuItem1.Size = new System.Drawing.Size(152, 22);
            this.lightToolStripMenuItem1.Text = "Light";
            this.lightToolStripMenuItem1.Click += new System.EventHandler(this.lightToolStripMenuItem1_Click);
            // 
            // darkToolStripMenuItem1
            // 
            this.darkToolStripMenuItem1.Size = new System.Drawing.Size(152, 22);
            this.darkToolStripMenuItem1.Text = "Dark";
            this.darkToolStripMenuItem1.Click += new System.EventHandler(this.darkToolStripMenuItem1_Click);
            // 
            // presetsToolStripMenuItem
            // 
            this.presetsToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.lightToolStripMenuItem,
            this.darkToolStripMenuItem,
            this.toolStripMenuItem2,
            this.resetToolStripMenuItem});
            this.presetsToolStripMenuItem.Size = new System.Drawing.Size(56, 20);
            this.presetsToolStripMenuItem.Text = "Presets";
            // 
            // lightToolStripMenuItem
            // 
            this.lightToolStripMenuItem.Size = new System.Drawing.Size(102, 22);
            this.lightToolStripMenuItem.Text = "Light";
            this.lightToolStripMenuItem.Click += new System.EventHandler(this.lightToolStripMenuItem_Click);
            // 
            // darkToolStripMenuItem
            // 
            this.darkToolStripMenuItem.Size = new System.Drawing.Size(102, 22);
            this.darkToolStripMenuItem.Text = "Dark";
            this.darkToolStripMenuItem.Click += new System.EventHandler(this.darkToolStripMenuItem_Click);
            // 
            // toolStripMenuItem2
            // 
            this.toolStripMenuItem2.Size = new System.Drawing.Size(99, 6);
            // 
            // resetToolStripMenuItem
            // 
            this.resetToolStripMenuItem.Size = new System.Drawing.Size(102, 22);
            this.resetToolStripMenuItem.Text = "Reset";
            this.resetToolStripMenuItem.Click += new System.EventHandler(this.resetToolStripMenuItem_Click);
            // 
            // contextColor
            // 
            this.contextColor.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.showSharedColorsToolStripMenuItem});
            this.contextColor.Size = new System.Drawing.Size(177, 26);
            // 
            // showSharedColorsToolStripMenuItem
            // 
            this.showSharedColorsToolStripMenuItem.Size = new System.Drawing.Size(176, 22);
            this.showSharedColorsToolStripMenuItem.Text = "Show shared colors";
            this.showSharedColorsToolStripMenuItem.Click += new System.EventHandler(this.showSharedColorsToolStripMenuItem_Click);
            // 
            // formColors
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
            this.BackColorName = Gw2Launcher.UI.UiColors.Colors.Custom;
            this.ClientSize = new System.Drawing.Size(390, 366);
            this.Controls.Add(this.stackPanel1);
            this.Controls.Add(this.menuStrip1);
            this.ForeColorName = Gw2Launcher.UI.UiColors.Colors.Custom;
            this.MainMenuStrip = this.menuStrip1;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Colors";
            this.stackPanel1.ResumeLayout(false);
            this.stackPanel1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.gridColors)).EndInit();
            this.menuStrip1.ResumeLayout(false);
            this.menuStrip1.PerformLayout();
            this.contextColor.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private Controls.StackPanel stackPanel1;
        private Controls.ScaledDataGridView gridColors;
        private System.Windows.Forms.DataGridViewTextBoxColumn columnColor;
        private System.Windows.Forms.DataGridViewTextBoxColumn columnName;
        private System.Windows.Forms.TextBox textColorsFilter;
        private System.Windows.Forms.MenuStrip menuStrip1;
        private System.Windows.Forms.ToolStripMenuItem fileToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem importToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem exportToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripMenuItem1;
        private System.Windows.Forms.ToolStripMenuItem saveToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem optionsToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem previewToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem changeAllSharedColorsToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem presetsToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem lightToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem darkToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripMenuItem2;
        private System.Windows.Forms.ToolStripMenuItem resetToolStripMenuItem;
        private System.Windows.Forms.ContextMenuStrip contextColor;
        private System.Windows.Forms.ToolStripMenuItem showSharedColorsToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripMenuItem3;
        private System.Windows.Forms.ToolStripMenuItem defaultToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem lightToolStripMenuItem1;
        private System.Windows.Forms.ToolStripMenuItem darkToolStripMenuItem1;
    }
}
