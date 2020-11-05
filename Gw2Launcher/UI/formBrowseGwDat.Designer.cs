namespace Gw2Launcher.UI
{
    partial class formBrowseGwDat
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
            this.label5 = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.radioFileAutoCopy = new System.Windows.Forms.RadioButton();
            this.label2 = new System.Windows.Forms.Label();
            this.label82 = new System.Windows.Forms.Label();
            this.stackPanel2 = new Gw2Launcher.UI.Controls.StackPanel();
            this.radioFileExisting = new System.Windows.Forms.RadioButton();
            this.textExisting = new System.Windows.Forms.TextBox();
            this.buttonExisting = new System.Windows.Forms.Button();
            this.stackPanel3 = new Gw2Launcher.UI.Controls.StackPanel();
            this.buttonCancel = new System.Windows.Forms.Button();
            this.buttonOK = new System.Windows.Forms.Button();
            this.stackPanel1.SuspendLayout();
            this.stackPanel2.SuspendLayout();
            this.stackPanel3.SuspendLayout();
            this.SuspendLayout();
            // 
            // stackPanel1
            // 
            this.stackPanel1.AutoSize = true;
            this.stackPanel1.Controls.Add(this.label5);
            this.stackPanel1.Controls.Add(this.label1);
            this.stackPanel1.Controls.Add(this.radioFileAutoCopy);
            this.stackPanel1.Controls.Add(this.label2);
            this.stackPanel1.Controls.Add(this.label82);
            this.stackPanel1.Controls.Add(this.stackPanel2);
            this.stackPanel1.Controls.Add(this.stackPanel3);
            this.stackPanel1.Location = new System.Drawing.Point(13, 13);
            this.stackPanel1.Margin = new System.Windows.Forms.Padding(13);
            this.stackPanel1.MinimumSize = new System.Drawing.Size(350, 0);
            this.stackPanel1.Size = new System.Drawing.Size(350, 209);
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Font = new System.Drawing.Font("Segoe UI Semilight", 8.25F);
            this.label5.Location = new System.Drawing.Point(0, 0);
            this.label5.Margin = new System.Windows.Forms.Padding(0, 0, 0, 23);
            this.label5.Size = new System.Drawing.Size(125, 13);
            this.label5.Text = "Select a Gw.dat file to use";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("Segoe UI Semibold", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label1.Location = new System.Drawing.Point(0, 36);
            this.label1.Margin = new System.Windows.Forms.Padding(0, 0, 0, 8);
            this.label1.Size = new System.Drawing.Size(103, 15);
            this.label1.Text = "Create a new copy";
            // 
            // radioFileAutoCopy
            // 
            this.radioFileAutoCopy.AutoSize = true;
            this.radioFileAutoCopy.Checked = true;
            this.radioFileAutoCopy.Location = new System.Drawing.Point(8, 59);
            this.radioFileAutoCopy.Margin = new System.Windows.Forms.Padding(8, 0, 0, 0);
            this.radioFileAutoCopy.Size = new System.Drawing.Size(190, 17);
            this.radioFileAutoCopy.TabStop = true;
            this.radioFileAutoCopy.Text = "Automatically set up a new copy";
            this.radioFileAutoCopy.UseVisualStyleBackColor = true;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Font = new System.Drawing.Font("Segoe UI Semibold", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label2.Location = new System.Drawing.Point(0, 90);
            this.label2.Margin = new System.Windows.Forms.Padding(0, 14, 0, 0);
            this.label2.Size = new System.Drawing.Size(105, 15);
            this.label2.Text = "Use an existing file";
            // 
            // label82
            // 
            this.label82.AutoSize = true;
            this.label82.Font = new System.Drawing.Font("Segoe UI Semilight", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label82.Location = new System.Drawing.Point(1, 106);
            this.label82.Margin = new System.Windows.Forms.Padding(1, 1, 0, 8);
            this.label82.Size = new System.Drawing.Size(313, 13);
            this.label82.Text = "To launch multiple accounts, each one must use its own Gw.dat file";
            // 
            // stackPanel2
            // 
            this.stackPanel2.AutoSize = true;
            this.stackPanel2.Controls.Add(this.radioFileExisting);
            this.stackPanel2.Controls.Add(this.textExisting);
            this.stackPanel2.Controls.Add(this.buttonExisting);
            this.stackPanel2.FlowDirection = System.Windows.Forms.FlowDirection.LeftToRight;
            this.stackPanel2.Location = new System.Drawing.Point(0, 127);
            this.stackPanel2.Margin = new System.Windows.Forms.Padding(0);
            this.stackPanel2.Size = new System.Drawing.Size(347, 24);
            // 
            // radioFileExisting
            // 
            this.radioFileExisting.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.radioFileExisting.AutoSize = true;
            this.radioFileExisting.Location = new System.Drawing.Point(8, 6);
            this.radioFileExisting.Margin = new System.Windows.Forms.Padding(8, 0, 0, 0);
            this.radioFileExisting.Size = new System.Drawing.Size(14, 13);
            this.radioFileExisting.UseVisualStyleBackColor = true;
            // 
            // textExisting
            // 
            this.textExisting.Font = new System.Drawing.Font("Segoe UI", 8.25F);
            this.textExisting.Location = new System.Drawing.Point(30, 1);
            this.textExisting.Margin = new System.Windows.Forms.Padding(8, 1, 0, 1);
            this.textExisting.ReadOnly = true;
            this.textExisting.Size = new System.Drawing.Size(268, 22);
            // 
            // buttonExisting
            // 
            this.buttonExisting.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left)));
            this.buttonExisting.AutoSize = true;
            this.buttonExisting.Font = new System.Drawing.Font("Segoe UI", 8.25F);
            this.buttonExisting.Location = new System.Drawing.Point(304, 0);
            this.buttonExisting.Margin = new System.Windows.Forms.Padding(6, 0, 0, 0);
            this.buttonExisting.Size = new System.Drawing.Size(43, 24);
            this.buttonExisting.Text = "...";
            this.buttonExisting.UseVisualStyleBackColor = true;
            this.buttonExisting.Click += new System.EventHandler(this.buttonExisting_Click);
            // 
            // stackPanel3
            // 
            this.stackPanel3.Anchor = System.Windows.Forms.AnchorStyles.Top;
            this.stackPanel3.AutoSize = true;
            this.stackPanel3.Controls.Add(this.buttonCancel);
            this.stackPanel3.Controls.Add(this.buttonOK);
            this.stackPanel3.FlowDirection = System.Windows.Forms.FlowDirection.LeftToRight;
            this.stackPanel3.Location = new System.Drawing.Point(87, 174);
            this.stackPanel3.Margin = new System.Windows.Forms.Padding(0, 23, 0, 0);
            this.stackPanel3.Size = new System.Drawing.Size(177, 35);
            // 
            // buttonCancel
            // 
            this.buttonCancel.AutoSize = true;
            this.buttonCancel.Font = new System.Drawing.Font("Segoe UI", 8.25F);
            this.buttonCancel.Location = new System.Drawing.Point(0, 0);
            this.buttonCancel.Margin = new System.Windows.Forms.Padding(0, 0, 5, 0);
            this.buttonCancel.Size = new System.Drawing.Size(86, 35);
            this.buttonCancel.Text = "Cancel";
            this.buttonCancel.UseVisualStyleBackColor = true;
            this.buttonCancel.Click += new System.EventHandler(this.buttonCancel_Click);
            // 
            // buttonOK
            // 
            this.buttonOK.AutoSize = true;
            this.buttonOK.Font = new System.Drawing.Font("Segoe UI", 8.25F);
            this.buttonOK.Location = new System.Drawing.Point(91, 0);
            this.buttonOK.Margin = new System.Windows.Forms.Padding(0);
            this.buttonOK.Size = new System.Drawing.Size(86, 35);
            this.buttonOK.Text = "OK";
            this.buttonOK.UseVisualStyleBackColor = true;
            this.buttonOK.Click += new System.EventHandler(this.buttonOK_Click);
            // 
            // formBrowseGwDat
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
            this.AutoSize = true;
            this.AutoSizeFill = Gw2Launcher.UI.Base.StackFormBase.AutoSizeFillMode.Height;
            this.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.ClientSize = new System.Drawing.Size(376, 235);
            this.Controls.Add(this.stackPanel1);
            this.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.Icon = global::Gw2Launcher.Properties.Resources.Gw2Launcher;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.stackPanel1.ResumeLayout(false);
            this.stackPanel1.PerformLayout();
            this.stackPanel2.ResumeLayout(false);
            this.stackPanel2.PerformLayout();
            this.stackPanel3.ResumeLayout(false);
            this.stackPanel3.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.Button buttonCancel;
        private System.Windows.Forms.Button buttonOK;
        private System.Windows.Forms.RadioButton radioFileAutoCopy;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label82;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.RadioButton radioFileExisting;
        private System.Windows.Forms.TextBox textExisting;
        private System.Windows.Forms.Button buttonExisting;
        private Controls.StackPanel stackPanel1;
        private Controls.StackPanel stackPanel2;
        private Controls.StackPanel stackPanel3;
    }
}
