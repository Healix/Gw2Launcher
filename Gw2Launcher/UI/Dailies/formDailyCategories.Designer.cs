namespace Gw2Launcher.UI.Dailies
{
    partial class formDailyCategories
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
            this.stackPanel1 = new Gw2Launcher.UI.Controls.StackPanel();
            this.radioCategories = new System.Windows.Forms.RadioButton();
            this.radioAdvanced = new System.Windows.Forms.RadioButton();
            this.buttonCancel = new System.Windows.Forms.Button();
            this.labelRefresh = new Gw2Launcher.UI.Controls.LinkLabel();
            this.buttonOK = new System.Windows.Forms.Button();
            this.panelCategoriesUpDown = new Gw2Launcher.UI.Controls.StackPanel();
            this.buttonOrderUp = new Gw2Launcher.UI.Controls.FlatShapeButton();
            this.buttonOrderDown = new Gw2Launcher.UI.Controls.FlatShapeButton();
            this.panelCategories = new Gw2Launcher.UI.Controls.StackPanel();
            this.stackPanel2 = new Gw2Launcher.UI.Controls.StackPanel();
            this.gridCategories = new Gw2Launcher.UI.Controls.ScaledDataGridView();
            this.columnCategory = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.textCategories = new System.Windows.Forms.TextBox();
            this.panelCategoriesLeftRight = new Gw2Launcher.UI.Controls.StackPanel();
            this.buttonAdd = new Gw2Launcher.UI.Controls.FlatShapeButton();
            this.buttonRemove = new Gw2Launcher.UI.Controls.FlatShapeButton();
            this.gridSelected = new Gw2Launcher.UI.Controls.ScaledDataGridView();
            this.columnSelected = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.textAdvanced = new System.Windows.Forms.TextBox();
            this.checkShowAll = new System.Windows.Forms.CheckBox();
            this.panelAllCategories = new System.Windows.Forms.Panel();
            this.stackPanel1.SuspendLayout();
            this.panelCategoriesUpDown.SuspendLayout();
            this.panelCategories.SuspendLayout();
            this.stackPanel2.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.gridCategories)).BeginInit();
            this.panelCategoriesLeftRight.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.gridSelected)).BeginInit();
            this.panelAllCategories.SuspendLayout();
            this.SuspendLayout();
            // 
            // stackPanel1
            // 
            this.stackPanel1.AutoSize = true;
            this.stackPanel1.Controls.Add(this.radioCategories);
            this.stackPanel1.Controls.Add(this.radioAdvanced);
            this.stackPanel1.FlowDirection = System.Windows.Forms.FlowDirection.LeftToRight;
            this.stackPanel1.Location = new System.Drawing.Point(13, 10);
            this.stackPanel1.Size = new System.Drawing.Size(165, 17);
            // 
            // radioCategories
            // 
            this.radioCategories.AutoSize = true;
            this.radioCategories.Checked = true;
            this.radioCategories.Location = new System.Drawing.Point(0, 0);
            this.radioCategories.Margin = new System.Windows.Forms.Padding(0);
            this.radioCategories.Size = new System.Drawing.Size(80, 17);
            this.radioCategories.TabStop = true;
            this.radioCategories.Text = "Categories";
            this.radioCategories.UseVisualStyleBackColor = true;
            this.radioCategories.CheckedChanged += new System.EventHandler(this.radioCategories_CheckedChanged);
            // 
            // radioAdvanced
            // 
            this.radioAdvanced.AutoSize = true;
            this.radioAdvanced.Location = new System.Drawing.Point(90, 0);
            this.radioAdvanced.Margin = new System.Windows.Forms.Padding(10, 0, 0, 0);
            this.radioAdvanced.Size = new System.Drawing.Size(75, 17);
            this.radioAdvanced.Text = "Advanced";
            this.radioAdvanced.UseVisualStyleBackColor = true;
            this.radioAdvanced.CheckedChanged += new System.EventHandler(this.radioCategories_CheckedChanged);
            // 
            // buttonCancel
            // 
            this.buttonCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonCancel.Font = new System.Drawing.Font("Segoe UI", 8.25F);
            this.buttonCancel.Location = new System.Drawing.Point(231, 246);
            this.buttonCancel.Size = new System.Drawing.Size(86, 35);
            this.buttonCancel.Text = "Cancel";
            this.buttonCancel.UseVisualStyleBackColor = true;
            this.buttonCancel.Click += new System.EventHandler(this.buttonCancel_Click);
            // 
            // labelRefresh
            // 
            this.labelRefresh.AutoSize = true;
            this.labelRefresh.Icon = null;
            this.labelRefresh.Location = new System.Drawing.Point(0, 1);
            this.labelRefresh.Margin = new System.Windows.Forms.Padding(0, 5, 0, 0);
            this.labelRefresh.Size = new System.Drawing.Size(99, 13);
            this.labelRefresh.Text = "refresh categories";
            this.labelRefresh.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.labelRefresh.Click += new System.EventHandler(this.labelRefresh_Click);
            // 
            // buttonOK
            // 
            this.buttonOK.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonOK.Font = new System.Drawing.Font("Segoe UI", 8.25F);
            this.buttonOK.Location = new System.Drawing.Point(327, 246);
            this.buttonOK.Size = new System.Drawing.Size(86, 35);
            this.buttonOK.Text = "OK";
            this.buttonOK.UseVisualStyleBackColor = true;
            this.buttonOK.Click += new System.EventHandler(this.buttonOK_Click);
            // 
            // panelCategoriesUpDown
            // 
            this.panelCategoriesUpDown.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.panelCategoriesUpDown.AutoSize = true;
            this.panelCategoriesUpDown.Controls.Add(this.buttonOrderUp);
            this.panelCategoriesUpDown.Controls.Add(this.buttonOrderDown);
            this.panelCategoriesUpDown.FlowDirection = System.Windows.Forms.FlowDirection.LeftToRight;
            this.panelCategoriesUpDown.Location = new System.Drawing.Point(360, 16);
            this.panelCategoriesUpDown.Margin = new System.Windows.Forms.Padding(0);
            this.panelCategoriesUpDown.Size = new System.Drawing.Size(52, 21);
            // 
            // buttonOrderUp
            // 
            this.buttonOrderUp.Cursor = Gw2Launcher.Windows.Cursors.Hand;
            this.buttonOrderUp.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(150)))), ((int)(((byte)(150)))), ((int)(((byte)(150)))));
            this.buttonOrderUp.ForeColorHovered = System.Drawing.SystemColors.ControlText;
            this.buttonOrderUp.Location = new System.Drawing.Point(3, 3);
            this.buttonOrderUp.ShapeDirection = System.Windows.Forms.ArrowDirection.Up;
            this.buttonOrderUp.ShapeSize = new System.Drawing.Size(8, 4);
            this.buttonOrderUp.Size = new System.Drawing.Size(20, 15);
            this.buttonOrderUp.MouseDown += new System.Windows.Forms.MouseEventHandler(this.buttonOrder_MouseDown);
            // 
            // buttonOrderDown
            // 
            this.buttonOrderDown.Cursor = Gw2Launcher.Windows.Cursors.Hand;
            this.buttonOrderDown.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(150)))), ((int)(((byte)(150)))), ((int)(((byte)(150)))));
            this.buttonOrderDown.ForeColorHovered = System.Drawing.SystemColors.ControlText;
            this.buttonOrderDown.Location = new System.Drawing.Point(29, 3);
            this.buttonOrderDown.ShapeDirection = System.Windows.Forms.ArrowDirection.Down;
            this.buttonOrderDown.ShapeSize = new System.Drawing.Size(8, 4);
            this.buttonOrderDown.Size = new System.Drawing.Size(20, 15);
            this.buttonOrderDown.MouseDown += new System.Windows.Forms.MouseEventHandler(this.buttonOrder_MouseDown);
            // 
            // panelCategories
            // 
            this.panelCategories.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.panelCategories.Controls.Add(this.stackPanel2);
            this.panelCategories.Controls.Add(this.panelCategoriesLeftRight);
            this.panelCategories.Controls.Add(this.gridSelected);
            this.panelCategories.FlowDirection = System.Windows.Forms.FlowDirection.LeftToRight;
            this.panelCategories.Location = new System.Drawing.Point(13, 37);
            this.panelCategories.Size = new System.Drawing.Size(399, 198);
            this.panelCategories.SizeChanged += new System.EventHandler(this.panelCategories_SizeChanged);
            // 
            // stackPanel2
            // 
            this.stackPanel2.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left)));
            this.stackPanel2.Controls.Add(this.gridCategories);
            this.stackPanel2.Controls.Add(this.textCategories);
            this.stackPanel2.Location = new System.Drawing.Point(0, 0);
            this.stackPanel2.Margin = new System.Windows.Forms.Padding(0);
            this.stackPanel2.Size = new System.Drawing.Size(189, 198);
            // 
            // gridCategories
            // 
            this.gridCategories.AllowUserToAddRows = false;
            this.gridCategories.AllowUserToDeleteRows = false;
            this.gridCategories.AllowUserToResizeColumns = false;
            this.gridCategories.AllowUserToResizeRows = false;
            this.gridCategories.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.gridCategories.BackgroundColor = System.Drawing.SystemColors.Window;
            this.gridCategories.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.gridCategories.CellBorderStyle = System.Windows.Forms.DataGridViewCellBorderStyle.None;
            this.gridCategories.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.gridCategories.ColumnHeadersVisible = false;
            this.gridCategories.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.columnCategory});
            dataGridViewCellStyle1.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle1.BackColor = System.Drawing.SystemColors.Window;
            dataGridViewCellStyle1.Font = new System.Drawing.Font("Segoe UI", 8.25F);
            dataGridViewCellStyle1.ForeColor = System.Drawing.SystemColors.ControlText;
            dataGridViewCellStyle1.SelectionBackColor = System.Drawing.SystemColors.ControlLight;
            dataGridViewCellStyle1.SelectionForeColor = System.Drawing.SystemColors.ControlText;
            dataGridViewCellStyle1.WrapMode = System.Windows.Forms.DataGridViewTriState.False;
            this.gridCategories.DefaultCellStyle = dataGridViewCellStyle1;
            this.gridCategories.EditMode = System.Windows.Forms.DataGridViewEditMode.EditProgrammatically;
            this.gridCategories.Location = new System.Drawing.Point(0, 0);
            this.gridCategories.Margin = new System.Windows.Forms.Padding(0);
            this.gridCategories.MultiSelect = false;
            this.gridCategories.ReadOnly = true;
            this.gridCategories.RowHeadersVisible = false;
            this.gridCategories.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.gridCategories.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.gridCategories.Size = new System.Drawing.Size(189, 171);
            this.gridCategories.CellDoubleClick += new System.Windows.Forms.DataGridViewCellEventHandler(this.grid_CellDoubleClick);
            this.gridCategories.KeyDown += new System.Windows.Forms.KeyEventHandler(this.grid_KeyDown);
            this.gridCategories.KeyUp += new System.Windows.Forms.KeyEventHandler(this.grid_KeyUp);
            // 
            // columnCategory
            // 
            this.columnCategory.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
            this.columnCategory.HeaderText = "";
            this.columnCategory.ReadOnly = true;
            // 
            // textCategories
            // 
            this.textCategories.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.textCategories.Location = new System.Drawing.Point(0, 176);
            this.textCategories.Margin = new System.Windows.Forms.Padding(0, 5, 0, 0);
            this.textCategories.Size = new System.Drawing.Size(189, 22);
            this.textCategories.TextChanged += new System.EventHandler(this.textCategories_TextChanged);
            // 
            // panelCategoriesLeftRight
            // 
            this.panelCategoriesLeftRight.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.panelCategoriesLeftRight.AutoSize = true;
            this.panelCategoriesLeftRight.AutoSizeFill = Gw2Launcher.UI.Controls.StackPanel.AutoSizeFillMode.NoWrap;
            this.panelCategoriesLeftRight.Controls.Add(this.buttonAdd);
            this.panelCategoriesLeftRight.Controls.Add(this.buttonRemove);
            this.panelCategoriesLeftRight.Location = new System.Drawing.Point(189, 73);
            this.panelCategoriesLeftRight.Margin = new System.Windows.Forms.Padding(0);
            this.panelCategoriesLeftRight.Size = new System.Drawing.Size(21, 52);
            // 
            // buttonAdd
            // 
            this.buttonAdd.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.buttonAdd.Cursor = Gw2Launcher.Windows.Cursors.Hand;
            this.buttonAdd.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(150)))), ((int)(((byte)(150)))), ((int)(((byte)(150)))));
            this.buttonAdd.ForeColorHovered = System.Drawing.SystemColors.ControlText;
            this.buttonAdd.Location = new System.Drawing.Point(3, 3);
            this.buttonAdd.ShapeDirection = System.Windows.Forms.ArrowDirection.Right;
            this.buttonAdd.ShapeSize = new System.Drawing.Size(4, 8);
            this.buttonAdd.Size = new System.Drawing.Size(15, 20);
            this.buttonAdd.Click += new System.EventHandler(this.buttonAddRemove_Click);
            // 
            // buttonRemove
            // 
            this.buttonRemove.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.buttonRemove.Cursor = Gw2Launcher.Windows.Cursors.Hand;
            this.buttonRemove.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(150)))), ((int)(((byte)(150)))), ((int)(((byte)(150)))));
            this.buttonRemove.ForeColorHovered = System.Drawing.SystemColors.ControlText;
            this.buttonRemove.Location = new System.Drawing.Point(3, 29);
            this.buttonRemove.ShapeSize = new System.Drawing.Size(4, 8);
            this.buttonRemove.Size = new System.Drawing.Size(15, 20);
            this.buttonRemove.Click += new System.EventHandler(this.buttonAddRemove_Click);
            // 
            // gridSelected
            // 
            this.gridSelected.AllowUserToAddRows = false;
            this.gridSelected.AllowUserToDeleteRows = false;
            this.gridSelected.AllowUserToResizeColumns = false;
            this.gridSelected.AllowUserToResizeRows = false;
            this.gridSelected.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.gridSelected.BackgroundColor = System.Drawing.SystemColors.Window;
            this.gridSelected.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.gridSelected.CellBorderStyle = System.Windows.Forms.DataGridViewCellBorderStyle.None;
            this.gridSelected.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.gridSelected.ColumnHeadersVisible = false;
            this.gridSelected.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.columnSelected});
            dataGridViewCellStyle2.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle2.BackColor = System.Drawing.SystemColors.Window;
            dataGridViewCellStyle2.Font = new System.Drawing.Font("Segoe UI", 8.25F);
            dataGridViewCellStyle2.ForeColor = System.Drawing.SystemColors.ControlText;
            dataGridViewCellStyle2.SelectionBackColor = System.Drawing.SystemColors.ControlLight;
            dataGridViewCellStyle2.SelectionForeColor = System.Drawing.SystemColors.ControlText;
            dataGridViewCellStyle2.WrapMode = System.Windows.Forms.DataGridViewTriState.False;
            this.gridSelected.DefaultCellStyle = dataGridViewCellStyle2;
            this.gridSelected.EditMode = System.Windows.Forms.DataGridViewEditMode.EditProgrammatically;
            this.gridSelected.Location = new System.Drawing.Point(210, 0);
            this.gridSelected.Margin = new System.Windows.Forms.Padding(0);
            this.gridSelected.MultiSelect = false;
            this.gridSelected.ReadOnly = true;
            this.gridSelected.RowHeadersVisible = false;
            this.gridSelected.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.gridSelected.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.gridSelected.Size = new System.Drawing.Size(189, 198);
            this.gridSelected.CellDoubleClick += new System.Windows.Forms.DataGridViewCellEventHandler(this.grid_CellDoubleClick);
            this.gridSelected.KeyDown += new System.Windows.Forms.KeyEventHandler(this.grid_KeyDown);
            this.gridSelected.KeyUp += new System.Windows.Forms.KeyEventHandler(this.grid_KeyUp);
            // 
            // columnSelected
            // 
            this.columnSelected.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
            this.columnSelected.HeaderText = "";
            this.columnSelected.ReadOnly = true;
            // 
            // textAdvanced
            // 
            this.textAdvanced.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.textAdvanced.Location = new System.Drawing.Point(13, 37);
            this.textAdvanced.Margin = new System.Windows.Forms.Padding(0, 10, 26, 0);
            this.textAdvanced.Multiline = true;
            this.textAdvanced.Size = new System.Drawing.Size(399, 198);
            // 
            // checkShowAll
            // 
            this.checkShowAll.AutoSize = true;
            this.checkShowAll.Location = new System.Drawing.Point(3, 0);
            this.checkShowAll.Size = new System.Drawing.Size(126, 17);
            this.checkShowAll.Text = "Show all categories";
            this.checkShowAll.UseVisualStyleBackColor = true;
            this.checkShowAll.Visible = false;
            this.checkShowAll.CheckedChanged += new System.EventHandler(this.checkShowAll_CheckedChanged);
            // 
            // panelAllCategories
            // 
            this.panelAllCategories.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.panelAllCategories.Controls.Add(this.labelRefresh);
            this.panelAllCategories.Controls.Add(this.checkShowAll);
            this.panelAllCategories.Location = new System.Drawing.Point(10, 245);
            this.panelAllCategories.Size = new System.Drawing.Size(192, 36);
            // 
            // formDailyCategories
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
            this.BackColorName = Gw2Launcher.UI.UiColors.Colors.Custom;
            this.ClientSize = new System.Drawing.Size(425, 293);
            this.Controls.Add(this.stackPanel1);
            this.Controls.Add(this.buttonCancel);
            this.Controls.Add(this.buttonOK);
            this.Controls.Add(this.panelCategoriesUpDown);
            this.Controls.Add(this.panelCategories);
            this.Controls.Add(this.textAdvanced);
            this.Controls.Add(this.panelAllCategories);
            this.ForeColorName = Gw2Launcher.UI.UiColors.Colors.Custom;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.MinimumSize = new System.Drawing.Size(400, 244);
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.stackPanel1.ResumeLayout(false);
            this.stackPanel1.PerformLayout();
            this.panelCategoriesUpDown.ResumeLayout(false);
            this.panelCategories.ResumeLayout(false);
            this.panelCategories.PerformLayout();
            this.stackPanel2.ResumeLayout(false);
            this.stackPanel2.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.gridCategories)).EndInit();
            this.panelCategoriesLeftRight.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.gridSelected)).EndInit();
            this.panelAllCategories.ResumeLayout(false);
            this.panelAllCategories.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TextBox textAdvanced;
        private System.Windows.Forms.Button buttonCancel;
        private System.Windows.Forms.Button buttonOK;
        private Controls.StackPanel panelCategories;
        private Controls.ScaledDataGridView gridSelected;
        private Controls.StackPanel panelCategoriesUpDown;
        private Controls.FlatShapeButton buttonOrderUp;
        private Controls.FlatShapeButton buttonOrderDown;
        private Controls.ScaledDataGridView gridCategories;
        private Controls.StackPanel panelCategoriesLeftRight;
        private Controls.FlatShapeButton buttonAdd;
        private Controls.FlatShapeButton buttonRemove;
        private Controls.LinkLabel labelRefresh;
        private System.Windows.Forms.DataGridViewTextBoxColumn columnCategory;
        private System.Windows.Forms.DataGridViewTextBoxColumn columnSelected;
        private Controls.StackPanel stackPanel1;
        private System.Windows.Forms.RadioButton radioCategories;
        private System.Windows.Forms.RadioButton radioAdvanced;
        private System.Windows.Forms.CheckBox checkShowAll;
        private System.Windows.Forms.Panel panelAllCategories;
        private Controls.StackPanel stackPanel2;
        private System.Windows.Forms.TextBox textCategories;
    }
}
