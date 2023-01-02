namespace Gw2Launcher.UI
{
    partial class formProcessSettingsPopup
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
            this.stackPanel1 = new Gw2Launcher.UI.Controls.StackPanel();
            this.panelWindowOptions = new Gw2Launcher.UI.Controls.StackPanel();
            this.label1 = new System.Windows.Forms.Label();
            this.checkTopMost = new System.Windows.Forms.CheckBox();
            this.checkPreventResizing = new System.Windows.Forms.CheckBox();
            this.checkBlockMinimizeClose = new System.Windows.Forms.CheckBox();
            this.label2 = new System.Windows.Forms.Label();
            this.comboPriority = new System.Windows.Forms.ComboBox();
            this.label3 = new System.Windows.Forms.Label();
            this.comboAffinity = new System.Windows.Forms.ComboBox();
            this.afdProcess = new Gw2Launcher.UI.Controls.AffinityDisplay();
            this.panelVolume = new Gw2Launcher.UI.Controls.StackPanel();
            this.label4 = new System.Windows.Forms.Label();
            this.sliderVolume = new Gw2Launcher.UI.Controls.FlatSlider();
            this.stackPanel1.SuspendLayout();
            this.panelWindowOptions.SuspendLayout();
            this.panelVolume.SuspendLayout();
            this.SuspendLayout();
            // 
            // stackPanel1
            // 
            this.stackPanel1.AutoSize = true;
            this.stackPanel1.Controls.Add(this.panelWindowOptions);
            this.stackPanel1.Controls.Add(this.label2);
            this.stackPanel1.Controls.Add(this.comboPriority);
            this.stackPanel1.Controls.Add(this.label3);
            this.stackPanel1.Controls.Add(this.comboAffinity);
            this.stackPanel1.Controls.Add(this.afdProcess);
            this.stackPanel1.Controls.Add(this.panelVolume);
            this.stackPanel1.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(230)))), ((int)(((byte)(230)))), ((int)(((byte)(230)))));
            this.stackPanel1.Location = new System.Drawing.Point(0, 0);
            this.stackPanel1.Margin = new System.Windows.Forms.Padding(0, 0, 0, 5);
            this.stackPanel1.Size = new System.Drawing.Size(200, 264);
            // 
            // panelWindowOptions
            // 
            this.panelWindowOptions.AutoSize = true;
            this.panelWindowOptions.AutoSizeFill = Gw2Launcher.UI.Controls.StackPanel.AutoSizeFillMode.Width;
            this.panelWindowOptions.Controls.Add(this.label1);
            this.panelWindowOptions.Controls.Add(this.checkTopMost);
            this.panelWindowOptions.Controls.Add(this.checkPreventResizing);
            this.panelWindowOptions.Controls.Add(this.checkBlockMinimizeClose);
            this.panelWindowOptions.Location = new System.Drawing.Point(0, 0);
            this.panelWindowOptions.Margin = new System.Windows.Forms.Padding(0, 0, 0, 5);
            this.panelWindowOptions.Size = new System.Drawing.Size(200, 83);
            this.panelWindowOptions.Visible = false;
            // 
            // label1
            // 
            this.label1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.label1.AutoSize = true;
            this.label1.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(32)))), ((int)(((byte)(32)))), ((int)(((byte)(32)))));
            this.label1.Font = new System.Drawing.Font("Segoe UI Semibold", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label1.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(224)))), ((int)(((byte)(224)))), ((int)(((byte)(224)))));
            this.label1.Location = new System.Drawing.Point(0, 0);
            this.label1.Margin = new System.Windows.Forms.Padding(0, 0, 0, 2);
            this.label1.Padding = new System.Windows.Forms.Padding(4, 4, 0, 4);
            this.label1.Size = new System.Drawing.Size(200, 21);
            this.label1.Text = "window options";
            // 
            // checkTopMost
            // 
            this.checkTopMost.AutoSize = true;
            this.checkTopMost.Location = new System.Drawing.Point(10, 26);
            this.checkTopMost.Margin = new System.Windows.Forms.Padding(10, 3, 10, 0);
            this.checkTopMost.Size = new System.Drawing.Size(93, 17);
            this.checkTopMost.Text = "Show on top";
            this.checkTopMost.UseVisualStyleBackColor = true;
            // 
            // checkPreventResizing
            // 
            this.checkPreventResizing.AutoSize = true;
            this.checkPreventResizing.Location = new System.Drawing.Point(10, 46);
            this.checkPreventResizing.Margin = new System.Windows.Forms.Padding(10, 3, 10, 0);
            this.checkPreventResizing.Size = new System.Drawing.Size(107, 17);
            this.checkPreventResizing.Text = "Prevent resizing";
            this.checkPreventResizing.UseVisualStyleBackColor = true;
            this.checkPreventResizing.CheckedChanged += new System.EventHandler(this.checkPreventResizing_CheckedChanged);
            // 
            // checkBlockMinimizeClose
            // 
            this.checkBlockMinimizeClose.AutoSize = true;
            this.checkBlockMinimizeClose.Enabled = false;
            this.checkBlockMinimizeClose.Location = new System.Drawing.Point(10, 66);
            this.checkBlockMinimizeClose.Margin = new System.Windows.Forms.Padding(10, 3, 10, 0);
            this.checkBlockMinimizeClose.Size = new System.Drawing.Size(131, 17);
            this.checkBlockMinimizeClose.Text = "Block minimize/close";
            this.checkBlockMinimizeClose.UseVisualStyleBackColor = true;
            this.checkBlockMinimizeClose.CheckedChanged += new System.EventHandler(this.checkBlockMinimizeClose_CheckedChanged);
            // 
            // label2
            // 
            this.label2.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.label2.AutoSize = true;
            this.label2.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(32)))), ((int)(((byte)(32)))), ((int)(((byte)(32)))));
            this.label2.Font = new System.Drawing.Font("Segoe UI Semibold", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label2.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(224)))), ((int)(((byte)(224)))), ((int)(((byte)(224)))));
            this.label2.Location = new System.Drawing.Point(0, 88);
            this.label2.Margin = new System.Windows.Forms.Padding(0, 0, 0, 5);
            this.label2.Padding = new System.Windows.Forms.Padding(4, 4, 0, 4);
            this.label2.Size = new System.Drawing.Size(200, 21);
            this.label2.Text = "priority";
            // 
            // comboPriority
            // 
            this.comboPriority.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.comboPriority.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboPriority.FormattingEnabled = true;
            this.comboPriority.Location = new System.Drawing.Point(10, 114);
            this.comboPriority.Margin = new System.Windows.Forms.Padding(10, 0, 10, 0);
            this.comboPriority.Size = new System.Drawing.Size(180, 21);
            this.comboPriority.SelectedIndexChanged += new System.EventHandler(this.comboPriority_SelectedIndexChanged);
            // 
            // label3
            // 
            this.label3.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.label3.AutoSize = true;
            this.label3.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(32)))), ((int)(((byte)(32)))), ((int)(((byte)(32)))));
            this.label3.Font = new System.Drawing.Font("Segoe UI Semibold", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label3.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(224)))), ((int)(((byte)(224)))), ((int)(((byte)(224)))));
            this.label3.Location = new System.Drawing.Point(0, 140);
            this.label3.Margin = new System.Windows.Forms.Padding(0, 5, 0, 5);
            this.label3.Padding = new System.Windows.Forms.Padding(4, 4, 0, 4);
            this.label3.Size = new System.Drawing.Size(200, 21);
            this.label3.Text = "affinity";
            // 
            // comboAffinity
            // 
            this.comboAffinity.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.comboAffinity.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboAffinity.FormattingEnabled = true;
            this.comboAffinity.Location = new System.Drawing.Point(10, 166);
            this.comboAffinity.Margin = new System.Windows.Forms.Padding(10, 0, 10, 0);
            this.comboAffinity.Size = new System.Drawing.Size(180, 21);
            this.comboAffinity.SelectedIndexChanged += new System.EventHandler(this.comboAffinity_SelectedIndexChanged);
            // 
            // afdProcess
            // 
            this.afdProcess.Affinity = ((long)(0));
            this.afdProcess.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.afdProcess.AutoSize = true;
            this.afdProcess.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(60)))), ((int)(((byte)(60)))));
            this.afdProcess.BorderColorHightlight = System.Drawing.Color.LightSlateGray;
            this.afdProcess.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(30)))), ((int)(((byte)(30)))), ((int)(((byte)(30)))));
            this.afdProcess.Location = new System.Drawing.Point(10, 192);
            this.afdProcess.Margin = new System.Windows.Forms.Padding(10, 5, 10, 0);
            this.afdProcess.Size = new System.Drawing.Size(180, 16);
            // 
            // panelVolume
            // 
            this.panelVolume.AutoSize = true;
            this.panelVolume.AutoSizeFill = Gw2Launcher.UI.Controls.StackPanel.AutoSizeFillMode.Width;
            this.panelVolume.Controls.Add(this.label4);
            this.panelVolume.Controls.Add(this.sliderVolume);
            this.panelVolume.Location = new System.Drawing.Point(0, 213);
            this.panelVolume.Margin = new System.Windows.Forms.Padding(0, 5, 0, 5);
            this.panelVolume.Size = new System.Drawing.Size(200, 46);
            this.panelVolume.Visible = false;
            // 
            // label4
            // 
            this.label4.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.label4.AutoSize = true;
            this.label4.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(32)))), ((int)(((byte)(32)))), ((int)(((byte)(32)))));
            this.label4.Font = new System.Drawing.Font("Segoe UI Semibold", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label4.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(224)))), ((int)(((byte)(224)))), ((int)(((byte)(224)))));
            this.label4.Location = new System.Drawing.Point(0, 0);
            this.label4.Margin = new System.Windows.Forms.Padding(0, 0, 0, 2);
            this.label4.Padding = new System.Windows.Forms.Padding(4, 4, 0, 4);
            this.label4.Size = new System.Drawing.Size(200, 21);
            this.label4.Text = "volume";
            // 
            // sliderVolume
            // 
            this.sliderVolume.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.sliderVolume.Location = new System.Drawing.Point(10, 26);
            this.sliderVolume.Margin = new System.Windows.Forms.Padding(10, 3, 10, 0);
            this.sliderVolume.Size = new System.Drawing.Size(180, 20);
            this.sliderVolume.TabStop = false;
            this.sliderVolume.Value = 0F;
            this.sliderVolume.ValueChanged += new System.EventHandler(this.sliderVolume_ValueChanged);
            // 
            // formProcessSettingsPopup
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
            this.AutoSize = true;
            this.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.BackColor = System.Drawing.Color.Black;
            this.ClientSize = new System.Drawing.Size(200, 341);
            this.Controls.Add(this.stackPanel1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.MinimumSize = new System.Drawing.Size(150, 0);
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.Manual;
            this.TopMost = true;
            this.stackPanel1.ResumeLayout(false);
            this.stackPanel1.PerformLayout();
            this.panelWindowOptions.ResumeLayout(false);
            this.panelWindowOptions.PerformLayout();
            this.panelVolume.ResumeLayout(false);
            this.panelVolume.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private Controls.StackPanel stackPanel1;
        private Controls.StackPanel panelWindowOptions;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.CheckBox checkTopMost;
        private System.Windows.Forms.CheckBox checkPreventResizing;
        private System.Windows.Forms.CheckBox checkBlockMinimizeClose;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.ComboBox comboPriority;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.ComboBox comboAffinity;
        private Controls.StackPanel panelVolume;
        private System.Windows.Forms.Label label4;
        private Controls.FlatSlider sliderVolume;
        private Controls.AffinityDisplay afdProcess;
    }
}
