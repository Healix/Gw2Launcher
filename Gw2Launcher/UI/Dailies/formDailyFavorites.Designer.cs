namespace Gw2Launcher.UI.Dailies
{
    partial class formDailyFavorites
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
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle2 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle1 = new System.Windows.Forms.DataGridViewCellStyle();
            this.textAdvanced = new System.Windows.Forms.TextBox();
            this.gridAchievements = new Gw2Launcher.UI.Controls.ScaledDataGridView();
            this.columnSelected = new System.Windows.Forms.DataGridViewCheckBoxColumn();
            this.columnId = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.columnName = new System.Windows.Forms.DataGridViewTextBoxColumn();
            ((System.ComponentModel.ISupportInitialize)(this.gridAchievements)).BeginInit();
            this.SuspendLayout();
            // 
            // textAdvanced
            // 
            this.textAdvanced.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.textAdvanced.Location = new System.Drawing.Point(9, 269);
            this.textAdvanced.Margin = new System.Windows.Forms.Padding(0, 10, 0, 0);
            this.textAdvanced.Size = new System.Drawing.Size(473, 22);
            this.textAdvanced.TextChanged += new System.EventHandler(this.textAdvanced_TextChanged);
            // 
            // gridAchievements
            // 
            this.gridAchievements.AllowUserToAddRows = false;
            this.gridAchievements.AllowUserToDeleteRows = false;
            this.gridAchievements.AllowUserToResizeColumns = false;
            this.gridAchievements.AllowUserToResizeRows = false;
            this.gridAchievements.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.gridAchievements.BackgroundColor = System.Drawing.SystemColors.Window;
            this.gridAchievements.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.gridAchievements.CellBorderStyle = System.Windows.Forms.DataGridViewCellBorderStyle.None;
            this.gridAchievements.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.gridAchievements.ColumnHeadersVisible = false;
            this.gridAchievements.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.columnSelected,
            this.columnId,
            this.columnName});
            dataGridViewCellStyle2.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle2.BackColor = System.Drawing.SystemColors.Window;
            dataGridViewCellStyle2.Font = new System.Drawing.Font("Segoe UI", 8.25F);
            dataGridViewCellStyle2.ForeColor = System.Drawing.SystemColors.ControlText;
            dataGridViewCellStyle2.SelectionBackColor = System.Drawing.SystemColors.ControlLight;
            dataGridViewCellStyle2.SelectionForeColor = System.Drawing.SystemColors.ControlText;
            dataGridViewCellStyle2.WrapMode = System.Windows.Forms.DataGridViewTriState.False;
            this.gridAchievements.DefaultCellStyle = dataGridViewCellStyle2;
            this.gridAchievements.EditMode = System.Windows.Forms.DataGridViewEditMode.EditProgrammatically;
            this.gridAchievements.Location = new System.Drawing.Point(9, 9);
            this.gridAchievements.Margin = new System.Windows.Forms.Padding(0);
            this.gridAchievements.MultiSelect = false;
            this.gridAchievements.ReadOnly = true;
            this.gridAchievements.RowHeadersVisible = false;
            this.gridAchievements.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.gridAchievements.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.gridAchievements.Size = new System.Drawing.Size(473, 255);
            this.gridAchievements.CellClick += new System.Windows.Forms.DataGridViewCellEventHandler(this.gridAchievements_CellClick);
            // 
            // columnSelected
            // 
            this.columnSelected.HeaderText = "";
            this.columnSelected.ReadOnly = true;
            this.columnSelected.Resizable = System.Windows.Forms.DataGridViewTriState.False;
            this.columnSelected.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.Automatic;
            this.columnSelected.Width = 25;
            // 
            // columnId
            // 
            dataGridViewCellStyle1.ForeColor = System.Drawing.SystemColors.GrayText;
            this.columnId.DefaultCellStyle = dataGridViewCellStyle1;
            this.columnId.HeaderText = "";
            this.columnId.ReadOnly = true;
            this.columnId.Resizable = System.Windows.Forms.DataGridViewTriState.False;
            this.columnId.Width = 50;
            // 
            // columnName
            // 
            this.columnName.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
            this.columnName.HeaderText = "";
            this.columnName.ReadOnly = true;
            this.columnName.Resizable = System.Windows.Forms.DataGridViewTriState.False;
            // 
            // formDailyFavorites
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
            this.BackColorName = Gw2Launcher.UI.UiColors.Colors.Custom;
            this.ClientSize = new System.Drawing.Size(494, 300);
            this.Controls.Add(this.textAdvanced);
            this.Controls.Add(this.gridAchievements);
            this.ForeColorName = Gw2Launcher.UI.UiColors.Colors.Custom;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            ((System.ComponentModel.ISupportInitialize)(this.gridAchievements)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private Controls.ScaledDataGridView gridAchievements;
        private System.Windows.Forms.TextBox textAdvanced;
        private System.Windows.Forms.DataGridViewCheckBoxColumn columnSelected;
        private System.Windows.Forms.DataGridViewTextBoxColumn columnId;
        private System.Windows.Forms.DataGridViewTextBoxColumn columnName;
    }
}
