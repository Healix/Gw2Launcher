namespace Gw2Launcher.UI
{
    partial class formShortcutType
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
            this.labelTitle = new System.Windows.Forms.Label();
            this.stackPanel1 = new Gw2Launcher.UI.Controls.StackPanel();
            this.radioMultiple = new System.Windows.Forms.RadioButton();
            this.stackPanel4 = new Gw2Launcher.UI.Controls.StackPanel();
            this.radioSingle = new System.Windows.Forms.RadioButton();
            this.label2 = new System.Windows.Forms.Label();
            this.stackPanel2 = new Gw2Launcher.UI.Controls.StackPanel();
            this.buttonCancel = new System.Windows.Forms.Button();
            this.buttonOK = new System.Windows.Forms.Button();
            this.stackPanel1.SuspendLayout();
            this.stackPanel4.SuspendLayout();
            this.stackPanel2.SuspendLayout();
            this.SuspendLayout();
            // 
            // labelTitle
            // 
            this.labelTitle.AutoSize = true;
            this.labelTitle.Font = new System.Drawing.Font("Segoe UI Semibold", 9F, System.Drawing.FontStyle.Bold);
            this.labelTitle.Location = new System.Drawing.Point(0, 0);
            this.labelTitle.Margin = new System.Windows.Forms.Padding(0, 0, 0, 1);
            this.labelTitle.Size = new System.Drawing.Size(170, 15);
            this.labelTitle.Text = "Enter your authentication code";
            // 
            // stackPanel1
            // 
            this.stackPanel1.AutoSize = true;
            this.stackPanel1.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowOnly;
            this.stackPanel1.Controls.Add(this.radioMultiple);
            this.stackPanel1.Controls.Add(this.stackPanel4);
            this.stackPanel1.Controls.Add(this.stackPanel2);
            this.stackPanel1.Location = new System.Drawing.Point(15, 60);
            this.stackPanel1.Margin = new System.Windows.Forms.Padding(15, 60, 15, 15);
            this.stackPanel1.Size = new System.Drawing.Size(217, 102);
            // 
            // radioMultiple
            // 
            this.radioMultiple.AutoSize = true;
            this.radioMultiple.Checked = true;
            this.radioMultiple.Location = new System.Drawing.Point(3, 3);
            this.radioMultiple.Size = new System.Drawing.Size(119, 17);
            this.radioMultiple.TabStop = true;
            this.radioMultiple.Text = "Multiple shortcuts";
            this.radioMultiple.UseVisualStyleBackColor = true;
            // 
            // stackPanel4
            // 
            this.stackPanel4.AutoSize = true;
            this.stackPanel4.AutoSizeFill = Gw2Launcher.UI.Controls.StackPanel.AutoSizeFillMode.NoWrap;
            this.stackPanel4.Controls.Add(this.radioSingle);
            this.stackPanel4.Controls.Add(this.label2);
            this.stackPanel4.FlowDirection = System.Windows.Forms.FlowDirection.LeftToRight;
            this.stackPanel4.Location = new System.Drawing.Point(0, 23);
            this.stackPanel4.Margin = new System.Windows.Forms.Padding(0);
            this.stackPanel4.Size = new System.Drawing.Size(183, 23);
            // 
            // radioSingle
            // 
            this.radioSingle.AutoSize = true;
            this.radioSingle.Location = new System.Drawing.Point(3, 3);
            this.radioSingle.Size = new System.Drawing.Size(103, 17);
            this.radioSingle.Text = "Single shortcut";
            this.radioSingle.UseVisualStyleBackColor = true;
            // 
            // label2
            // 
            this.label2.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.label2.AutoSize = true;
            this.label2.Font = new System.Drawing.Font("Segoe UI Semilight", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label2.ForeColor = System.Drawing.SystemColors.GrayText;
            this.label2.Location = new System.Drawing.Point(109, 4);
            this.label2.Margin = new System.Windows.Forms.Padding(0);
            this.label2.Size = new System.Drawing.Size(74, 15);
            this.label2.Text = "multi-launch";
            // 
            // stackPanel2
            // 
            this.stackPanel2.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.stackPanel2.AutoSize = true;
            this.stackPanel2.Controls.Add(this.buttonCancel);
            this.stackPanel2.Controls.Add(this.buttonOK);
            this.stackPanel2.FlowDirection = System.Windows.Forms.FlowDirection.LeftToRight;
            this.stackPanel2.Location = new System.Drawing.Point(16, 61);
            this.stackPanel2.Margin = new System.Windows.Forms.Padding(0, 15, 0, 0);
            this.stackPanel2.Size = new System.Drawing.Size(184, 41);
            // 
            // buttonCancel
            // 
            this.buttonCancel.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.buttonCancel.Font = new System.Drawing.Font("Segoe UI", 8.25F);
            this.buttonCancel.Location = new System.Drawing.Point(3, 3);
            this.buttonCancel.Size = new System.Drawing.Size(86, 35);
            this.buttonCancel.Text = "Cancel";
            this.buttonCancel.UseVisualStyleBackColor = true;
            this.buttonCancel.Click += new System.EventHandler(this.buttonCancel_Click);
            // 
            // buttonOK
            // 
            this.buttonOK.Font = new System.Drawing.Font("Segoe UI", 8.25F);
            this.buttonOK.Location = new System.Drawing.Point(95, 3);
            this.buttonOK.Size = new System.Drawing.Size(86, 35);
            this.buttonOK.Text = "OK";
            this.buttonOK.UseVisualStyleBackColor = true;
            this.buttonOK.Click += new System.EventHandler(this.buttonOK_Click);
            // 
            // formShortcutType
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
            this.AutoSize = true;
            this.ClientSize = new System.Drawing.Size(247, 177);
            this.Controls.Add(this.stackPanel1);
            this.Text = "Create shortcut...";
            this.stackPanel1.ResumeLayout(false);
            this.stackPanel1.PerformLayout();
            this.stackPanel4.ResumeLayout(false);
            this.stackPanel4.PerformLayout();
            this.stackPanel2.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label labelTitle;
        private Controls.StackPanel stackPanel1;
        private Controls.StackPanel stackPanel2;
        private System.Windows.Forms.Button buttonCancel;
        private System.Windows.Forms.RadioButton radioMultiple;
        private System.Windows.Forms.Button buttonOK;
        private Controls.StackPanel stackPanel4;
        private System.Windows.Forms.RadioButton radioSingle;
        private System.Windows.Forms.Label label2;

    }
}
