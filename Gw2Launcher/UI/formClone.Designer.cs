namespace Gw2Launcher.UI
{
    partial class formClone
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
            this.buttonCancel = new System.Windows.Forms.Button();
            this.buttonOK = new System.Windows.Forms.Button();
            this.label1 = new System.Windows.Forms.Label();
            this.radioDatShare = new System.Windows.Forms.RadioButton();
            this.radioDatCopy = new System.Windows.Forms.RadioButton();
            this.labelLocalDat = new System.Windows.Forms.Label();
            this.radioGfxShare = new System.Windows.Forms.RadioButton();
            this.radioGfxCopy = new System.Windows.Forms.RadioButton();
            this.labelGfxSettings = new System.Windows.Forms.Label();
            this.checkStatistics = new System.Windows.Forms.CheckBox();
            this.label5 = new System.Windows.Forms.Label();
            this.panelContainer = new System.Windows.Forms.Panel();
            this.pictureIcon = new System.Windows.Forms.PictureBox();
            this.textTemplate = new System.Windows.Forms.TextBox();
            this.checkTemplate = new System.Windows.Forms.CheckBox();
            this.label2 = new System.Windows.Forms.Label();
            this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            this.panelLocalDat = new Gw2Launcher.UI.Controls.StackPanel();
            this.panelGfxSettings = new Gw2Launcher.UI.Controls.StackPanel();
            this.stackPanel2 = new Gw2Launcher.UI.Controls.StackPanel();
            this.stackPanel4 = new Gw2Launcher.UI.Controls.StackPanel();
            this.panelScroll = new Gw2Launcher.UI.Controls.AutoScrollContainerPanel();
            this.panelContainer.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureIcon)).BeginInit();
            this.tableLayoutPanel1.SuspendLayout();
            this.panelLocalDat.SuspendLayout();
            this.panelGfxSettings.SuspendLayout();
            this.stackPanel2.SuspendLayout();
            this.stackPanel4.SuspendLayout();
            this.panelScroll.SuspendLayout();
            this.SuspendLayout();
            // 
            // buttonCancel
            // 
            this.buttonCancel.Anchor = System.Windows.Forms.AnchorStyles.Bottom;
            this.buttonCancel.Font = new System.Drawing.Font("Segoe UI", 8.25F);
            this.buttonCancel.Location = new System.Drawing.Point(109, 179);
            this.buttonCancel.Size = new System.Drawing.Size(86, 35);
            this.buttonCancel.Text = "Cancel";
            this.buttonCancel.UseVisualStyleBackColor = true;
            this.buttonCancel.Click += new System.EventHandler(this.buttonCancel_Click);
            // 
            // buttonOK
            // 
            this.buttonOK.Anchor = System.Windows.Forms.AnchorStyles.Bottom;
            this.buttonOK.Font = new System.Drawing.Font("Segoe UI", 8.25F);
            this.buttonOK.Location = new System.Drawing.Point(205, 179);
            this.buttonOK.Size = new System.Drawing.Size(86, 35);
            this.buttonOK.Text = "OK";
            this.buttonOK.UseVisualStyleBackColor = true;
            this.buttonOK.Click += new System.EventHandler(this.buttonOK_Click);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("Segoe UI Semibold", 9.75F, System.Drawing.FontStyle.Bold);
            this.label1.Location = new System.Drawing.Point(0, 0);
            this.label1.Margin = new System.Windows.Forms.Padding(0, 0, 0, 8);
            this.label1.Size = new System.Drawing.Size(153, 17);
            this.label1.Text = "Clone selected accounts";
            // 
            // radioDatShare
            // 
            this.radioDatShare.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.radioDatShare.AutoSize = true;
            this.radioDatShare.Checked = true;
            this.radioDatShare.Location = new System.Drawing.Point(57, 0);
            this.radioDatShare.Margin = new System.Windows.Forms.Padding(6, 0, 0, 0);
            this.radioDatShare.Size = new System.Drawing.Size(54, 17);
            this.radioDatShare.TabStop = true;
            this.radioDatShare.Text = "Share";
            this.radioDatShare.UseVisualStyleBackColor = true;
            // 
            // radioDatCopy
            // 
            this.radioDatCopy.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.radioDatCopy.AutoSize = true;
            this.radioDatCopy.Location = new System.Drawing.Point(0, 0);
            this.radioDatCopy.Margin = new System.Windows.Forms.Padding(0);
            this.radioDatCopy.Size = new System.Drawing.Size(51, 17);
            this.radioDatCopy.Text = "Copy";
            this.radioDatCopy.UseVisualStyleBackColor = true;
            // 
            // labelLocalDat
            // 
            this.labelLocalDat.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.labelLocalDat.AutoSize = true;
            this.labelLocalDat.Location = new System.Drawing.Point(0, 6);
            this.labelLocalDat.Margin = new System.Windows.Forms.Padding(0, 6, 0, 6);
            this.labelLocalDat.MaximumSize = new System.Drawing.Size(420, 0);
            this.labelLocalDat.Size = new System.Drawing.Size(53, 13);
            this.labelLocalDat.Text = "Local.dat";
            // 
            // radioGfxShare
            // 
            this.radioGfxShare.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.radioGfxShare.AutoSize = true;
            this.radioGfxShare.Checked = true;
            this.radioGfxShare.Location = new System.Drawing.Point(57, 0);
            this.radioGfxShare.Margin = new System.Windows.Forms.Padding(6, 0, 0, 0);
            this.radioGfxShare.Size = new System.Drawing.Size(54, 17);
            this.radioGfxShare.TabStop = true;
            this.radioGfxShare.Text = "Share";
            this.radioGfxShare.UseVisualStyleBackColor = true;
            // 
            // radioGfxCopy
            // 
            this.radioGfxCopy.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.radioGfxCopy.AutoSize = true;
            this.radioGfxCopy.Location = new System.Drawing.Point(0, 0);
            this.radioGfxCopy.Margin = new System.Windows.Forms.Padding(0);
            this.radioGfxCopy.Size = new System.Drawing.Size(51, 17);
            this.radioGfxCopy.Text = "Copy";
            this.radioGfxCopy.UseVisualStyleBackColor = true;
            // 
            // labelGfxSettings
            // 
            this.labelGfxSettings.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.labelGfxSettings.AutoSize = true;
            this.labelGfxSettings.Location = new System.Drawing.Point(0, 31);
            this.labelGfxSettings.Margin = new System.Windows.Forms.Padding(0, 6, 0, 6);
            this.labelGfxSettings.MaximumSize = new System.Drawing.Size(420, 0);
            this.labelGfxSettings.Size = new System.Drawing.Size(89, 13);
            this.labelGfxSettings.Text = "GFXSettings.xml";
            // 
            // checkStatistics
            // 
            this.checkStatistics.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.checkStatistics.AutoSize = true;
            this.checkStatistics.Location = new System.Drawing.Point(0, 1);
            this.checkStatistics.Margin = new System.Windows.Forms.Padding(0);
            this.checkStatistics.Size = new System.Drawing.Size(15, 14);
            this.checkStatistics.UseVisualStyleBackColor = true;
            // 
            // label5
            // 
            this.label5.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.label5.AutoSize = true;
            this.label5.Font = new System.Drawing.Font("Segoe UI Semilight", 9F);
            this.label5.ForeColor = System.Drawing.SystemColors.GrayText;
            this.label5.Location = new System.Drawing.Point(21, 0);
            this.label5.Margin = new System.Windows.Forms.Padding(6, 0, 0, 1);
            this.label5.Size = new System.Drawing.Size(90, 15);
            this.label5.Text = "(date, launches)";
            this.label5.Click += new System.EventHandler(this.label5_Click);
            // 
            // panelContainer
            // 
            this.panelContainer.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.panelContainer.AutoScroll = true;
            this.panelContainer.Controls.Add(this.pictureIcon);
            this.panelContainer.Controls.Add(this.textTemplate);
            this.panelContainer.Controls.Add(this.checkTemplate);
            this.panelContainer.Location = new System.Drawing.Point(0, 0);
            this.panelContainer.Margin = new System.Windows.Forms.Padding(0, 0, 10, 0);
            this.panelContainer.Size = new System.Drawing.Size(352, 33);
            // 
            // pictureIcon
            // 
            this.pictureIcon.Location = new System.Drawing.Point(25, 8);
            this.pictureIcon.Size = new System.Drawing.Size(16, 16);
            this.pictureIcon.TabStop = false;
            this.pictureIcon.Visible = false;
            // 
            // textTemplate
            // 
            this.textTemplate.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.textTemplate.Location = new System.Drawing.Point(48, 5);
            this.textTemplate.Size = new System.Drawing.Size(301, 22);
            this.textTemplate.Visible = false;
            // 
            // checkTemplate
            // 
            this.checkTemplate.AutoSize = true;
            this.checkTemplate.Location = new System.Drawing.Point(5, 9);
            this.checkTemplate.Size = new System.Drawing.Size(15, 14);
            this.checkTemplate.UseVisualStyleBackColor = true;
            this.checkTemplate.Visible = false;
            // 
            // label2
            // 
            this.label2.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(0, 56);
            this.label2.Margin = new System.Windows.Forms.Padding(0, 6, 0, 6);
            this.label2.MaximumSize = new System.Drawing.Size(420, 0);
            this.label2.Size = new System.Drawing.Size(52, 13);
            this.label2.Text = "Statistics";
            // 
            // tableLayoutPanel1
            // 
            this.tableLayoutPanel1.AutoSize = true;
            this.tableLayoutPanel1.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.tableLayoutPanel1.ColumnCount = 3;
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 20F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tableLayoutPanel1.Controls.Add(this.panelLocalDat, 2, 0);
            this.tableLayoutPanel1.Controls.Add(this.panelGfxSettings, 2, 1);
            this.tableLayoutPanel1.Controls.Add(this.stackPanel2, 2, 2);
            this.tableLayoutPanel1.Controls.Add(this.label2, 0, 2);
            this.tableLayoutPanel1.Controls.Add(this.labelLocalDat, 0, 0);
            this.tableLayoutPanel1.Controls.Add(this.labelGfxSettings, 0, 1);
            this.tableLayoutPanel1.Location = new System.Drawing.Point(3, 28);
            this.tableLayoutPanel1.RowCount = 3;
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
            this.tableLayoutPanel1.Size = new System.Drawing.Size(220, 75);
            // 
            // panelLocalDat
            // 
            this.panelLocalDat.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.panelLocalDat.AutoSize = true;
            this.panelLocalDat.AutoSizeFill = Gw2Launcher.UI.Controls.StackPanel.AutoSizeFillMode.NoWrap;
            this.panelLocalDat.Controls.Add(this.radioDatCopy);
            this.panelLocalDat.Controls.Add(this.radioDatShare);
            this.panelLocalDat.FlowDirection = System.Windows.Forms.FlowDirection.LeftToRight;
            this.panelLocalDat.Location = new System.Drawing.Point(109, 4);
            this.panelLocalDat.Margin = new System.Windows.Forms.Padding(0);
            this.panelLocalDat.Size = new System.Drawing.Size(111, 17);
            // 
            // panelGfxSettings
            // 
            this.panelGfxSettings.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.panelGfxSettings.AutoSize = true;
            this.panelGfxSettings.AutoSizeFill = Gw2Launcher.UI.Controls.StackPanel.AutoSizeFillMode.NoWrap;
            this.panelGfxSettings.Controls.Add(this.radioGfxCopy);
            this.panelGfxSettings.Controls.Add(this.radioGfxShare);
            this.panelGfxSettings.FlowDirection = System.Windows.Forms.FlowDirection.LeftToRight;
            this.panelGfxSettings.Location = new System.Drawing.Point(109, 29);
            this.panelGfxSettings.Margin = new System.Windows.Forms.Padding(0);
            this.panelGfxSettings.Size = new System.Drawing.Size(111, 17);
            // 
            // stackPanel2
            // 
            this.stackPanel2.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.stackPanel2.AutoSize = true;
            this.stackPanel2.AutoSizeFill = Gw2Launcher.UI.Controls.StackPanel.AutoSizeFillMode.NoWrap;
            this.stackPanel2.Controls.Add(this.checkStatistics);
            this.stackPanel2.Controls.Add(this.label5);
            this.stackPanel2.FlowDirection = System.Windows.Forms.FlowDirection.LeftToRight;
            this.stackPanel2.Location = new System.Drawing.Point(109, 54);
            this.stackPanel2.Margin = new System.Windows.Forms.Padding(0);
            this.stackPanel2.Size = new System.Drawing.Size(111, 16);
            // 
            // stackPanel4
            // 
            this.stackPanel4.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.stackPanel4.Controls.Add(this.label1);
            this.stackPanel4.Controls.Add(this.tableLayoutPanel1);
            this.stackPanel4.Controls.Add(this.panelScroll);
            this.stackPanel4.Location = new System.Drawing.Point(10, 10);
            this.stackPanel4.Size = new System.Drawing.Size(362, 163);
            // 
            // panelScroll
            // 
            this.panelScroll.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.panelScroll.Controls.Add(this.panelContainer);
            this.panelScroll.Location = new System.Drawing.Point(0, 119);
            this.panelScroll.Margin = new System.Windows.Forms.Padding(0, 13, 0, 0);
            this.panelScroll.Size = new System.Drawing.Size(362, 44);
            // 
            // formClone
            // 
            this.ClientSize = new System.Drawing.Size(384, 226);
            this.Controls.Add(this.stackPanel4);
            this.Controls.Add(this.buttonCancel);
            this.Controls.Add(this.buttonOK);
            this.Icon = global::Gw2Launcher.Properties.Resources.Gw2Launcher;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.MinimumSize = new System.Drawing.Size(400, 265);
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.panelContainer.ResumeLayout(false);
            this.panelContainer.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureIcon)).EndInit();
            this.tableLayoutPanel1.ResumeLayout(false);
            this.tableLayoutPanel1.PerformLayout();
            this.panelLocalDat.ResumeLayout(false);
            this.panelLocalDat.PerformLayout();
            this.panelGfxSettings.ResumeLayout(false);
            this.panelGfxSettings.PerformLayout();
            this.stackPanel2.ResumeLayout(false);
            this.stackPanel2.PerformLayout();
            this.stackPanel4.ResumeLayout(false);
            this.stackPanel4.PerformLayout();
            this.panelScroll.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Button buttonCancel;
        private System.Windows.Forms.Button buttonOK;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.RadioButton radioDatShare;
        private System.Windows.Forms.RadioButton radioDatCopy;
        private System.Windows.Forms.Label labelLocalDat;
        private System.Windows.Forms.RadioButton radioGfxShare;
        private System.Windows.Forms.RadioButton radioGfxCopy;
        private System.Windows.Forms.Label labelGfxSettings;
        private System.Windows.Forms.CheckBox checkStatistics;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.Panel panelContainer;
        private System.Windows.Forms.TextBox textTemplate;
        private System.Windows.Forms.CheckBox checkTemplate;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.PictureBox pictureIcon;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
        private Controls.StackPanel panelLocalDat;
        private Controls.StackPanel panelGfxSettings;
        private Controls.StackPanel stackPanel2;
        private Controls.StackPanel stackPanel4;
        private Controls.AutoScrollContainerPanel panelScroll;
    }
}
