namespace Gw2Launcher.UI
{
    partial class formDnsDialog
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
            this.gridServers = new Controls.ScaledDataGridView();
            this.columnCheck = new System.Windows.Forms.DataGridViewCheckBoxColumn();
            this.columnServer = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.buttonCancel = new System.Windows.Forms.Button();
            this.buttonOK = new System.Windows.Forms.Button();
            this.label5 = new System.Windows.Forms.Label();
            ((System.ComponentModel.ISupportInitialize)(this.gridServers)).BeginInit();
            this.SuspendLayout();
            // 
            // gridServers
            // 
            this.gridServers.AllowUserToAddRows = false;
            this.gridServers.AllowUserToDeleteRows = false;
            this.gridServers.AllowUserToResizeColumns = false;
            this.gridServers.AllowUserToResizeRows = false;
            this.gridServers.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.gridServers.BackgroundColor = System.Drawing.SystemColors.Control;
            this.gridServers.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.gridServers.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.gridServers.ColumnHeadersVisible = false;
            this.gridServers.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.columnCheck,
            this.columnServer});
            dataGridViewCellStyle1.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle1.BackColor = System.Drawing.SystemColors.Control;
            dataGridViewCellStyle1.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            dataGridViewCellStyle1.ForeColor = System.Drawing.SystemColors.ControlText;
            dataGridViewCellStyle1.SelectionBackColor = System.Drawing.SystemColors.Highlight;
            dataGridViewCellStyle1.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            dataGridViewCellStyle1.WrapMode = System.Windows.Forms.DataGridViewTriState.False;
            this.gridServers.DefaultCellStyle = dataGridViewCellStyle1;
            this.gridServers.EditMode = System.Windows.Forms.DataGridViewEditMode.EditProgrammatically;
            this.gridServers.Location = new System.Drawing.Point(12, 36);
            this.gridServers.MultiSelect = false;
            this.gridServers.ReadOnly = true;
            this.gridServers.RowHeadersVisible = false;
            this.gridServers.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.gridServers.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.CellSelect;
            this.gridServers.Size = new System.Drawing.Size(261, 222);
            this.gridServers.CellContentClick += new System.Windows.Forms.DataGridViewCellEventHandler(this.gridServers_CellContentClick);
            this.gridServers.SelectionChanged += new System.EventHandler(this.gridServers_SelectionChanged);
            // 
            // columnCheck
            // 
            this.columnCheck.HeaderText = "Check";
            this.columnCheck.ReadOnly = true;
            this.columnCheck.ThreeState = true;
            this.columnCheck.Width = 25;
            // 
            // columnServer
            // 
            this.columnServer.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
            this.columnServer.HeaderText = "Server";
            this.columnServer.ReadOnly = true;
            // 
            // buttonCancel
            // 
            this.buttonCancel.Anchor = System.Windows.Forms.AnchorStyles.Bottom;
            this.buttonCancel.Font = new System.Drawing.Font("Segoe UI", 8.25F);
            this.buttonCancel.Location = new System.Drawing.Point(91, 275);
            this.buttonCancel.Size = new System.Drawing.Size(86, 35);
            this.buttonCancel.Text = "Cancel";
            this.buttonCancel.UseVisualStyleBackColor = true;
            this.buttonCancel.Click += new System.EventHandler(this.buttonCancel_Click);
            // 
            // buttonOK
            // 
            this.buttonOK.Anchor = System.Windows.Forms.AnchorStyles.Bottom;
            this.buttonOK.Font = new System.Drawing.Font("Segoe UI", 8.25F);
            this.buttonOK.Location = new System.Drawing.Point(187, 275);
            this.buttonOK.Size = new System.Drawing.Size(86, 35);
            this.buttonOK.Text = "OK";
            this.buttonOK.UseVisualStyleBackColor = true;
            this.buttonOK.Click += new System.EventHandler(this.buttonOK_Click);
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Font = new System.Drawing.Font("Segoe UI Semilight", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label5.Location = new System.Drawing.Point(10, 10);
            this.label5.Size = new System.Drawing.Size(157, 13);
            this.label5.Text = "Select which DNS servers to use";
            // 
            // formDnsDialog
            // 
            this.ClientSize = new System.Drawing.Size(285, 322);
            this.Controls.Add(this.label5);
            this.Controls.Add(this.buttonCancel);
            this.Controls.Add(this.buttonOK);
            this.Controls.Add(this.gridServers);
            this.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.Icon = global::Gw2Launcher.Properties.Resources.Gw2Launcher;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Load += new System.EventHandler(this.formDnsDialog_Load);
            ((System.ComponentModel.ISupportInitialize)(this.gridServers)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private Controls.ScaledDataGridView gridServers;
        private System.Windows.Forms.Button buttonCancel;
        private System.Windows.Forms.Button buttonOK;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.DataGridViewCheckBoxColumn columnCheck;
        private System.Windows.Forms.DataGridViewTextBoxColumn columnServer;
    }
}
