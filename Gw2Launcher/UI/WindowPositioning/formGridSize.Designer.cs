namespace Gw2Launcher.UI.WindowPositioning
{
    partial class formGridSize
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
            this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            this.label3 = new System.Windows.Forms.Label();
            this.label5 = new System.Windows.Forms.Label();
            this.textWidth = new Gw2Launcher.UI.Controls.IntegerTextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.label6 = new System.Windows.Forms.Label();
            this.textHeight = new Gw2Launcher.UI.Controls.IntegerTextBox();
            this.stackPanel2 = new Gw2Launcher.UI.Controls.StackPanel();
            this.buttonCancel = new System.Windows.Forms.Button();
            this.buttonOk = new System.Windows.Forms.Button();
            this.stackPanel1.SuspendLayout();
            this.tableLayoutPanel1.SuspendLayout();
            this.stackPanel2.SuspendLayout();
            this.SuspendLayout();
            // 
            // stackPanel1
            // 
            this.stackPanel1.AutoSize = true;
            this.stackPanel1.Controls.Add(this.tableLayoutPanel1);
            this.stackPanel1.Controls.Add(this.stackPanel2);
            this.stackPanel1.Location = new System.Drawing.Point(15, 60);
            this.stackPanel1.Margin = new System.Windows.Forms.Padding(15, 60, 15, 15);
            this.stackPanel1.Size = new System.Drawing.Size(184, 111);
            // 
            // tableLayoutPanel1
            // 
            this.tableLayoutPanel1.AutoSize = true;
            this.tableLayoutPanel1.ColumnCount = 3;
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tableLayoutPanel1.Controls.Add(this.label3, 0, 2);
            this.tableLayoutPanel1.Controls.Add(this.label5, 2, 0);
            this.tableLayoutPanel1.Controls.Add(this.textWidth, 1, 0);
            this.tableLayoutPanel1.Controls.Add(this.label1, 0, 0);
            this.tableLayoutPanel1.Controls.Add(this.label6, 2, 2);
            this.tableLayoutPanel1.Controls.Add(this.textHeight, 1, 2);
            this.tableLayoutPanel1.Location = new System.Drawing.Point(3, 3);
            this.tableLayoutPanel1.RowCount = 3;
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 5F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel1.Size = new System.Drawing.Size(155, 49);
            // 
            // label3
            // 
            this.label3.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(3, 31);
            this.label3.Size = new System.Drawing.Size(42, 13);
            this.label3.Text = "Height";
            // 
            // label5
            // 
            this.label5.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.label5.AutoSize = true;
            this.label5.ForeColor = System.Drawing.SystemColors.GrayText;
            this.label5.Location = new System.Drawing.Point(133, 4);
            this.label5.Size = new System.Drawing.Size(19, 13);
            this.label5.Text = "px";
            // 
            // textWidth
            // 
            this.textWidth.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.textWidth.Increment = 1;
            this.textWidth.Location = new System.Drawing.Point(58, 0);
            this.textWidth.Margin = new System.Windows.Forms.Padding(10, 0, 10, 0);
            this.textWidth.Maximum = 100;
            this.textWidth.Minimum = 1;
            this.textWidth.ReverseMouseWheelDirection = true;
            this.textWidth.Size = new System.Drawing.Size(62, 22);
            this.textWidth.Text = "1";
            this.textWidth.Value = 1;
            this.textWidth.KeyDown += new System.Windows.Forms.KeyEventHandler(this.text_KeyDown);
            // 
            // label1
            // 
            this.label1.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(3, 4);
            this.label1.Size = new System.Drawing.Size(39, 13);
            this.label1.Text = "Width";
            // 
            // label6
            // 
            this.label6.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.label6.AutoSize = true;
            this.label6.ForeColor = System.Drawing.SystemColors.GrayText;
            this.label6.Location = new System.Drawing.Point(133, 31);
            this.label6.Size = new System.Drawing.Size(19, 13);
            this.label6.Text = "px";
            // 
            // textHeight
            // 
            this.textHeight.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.textHeight.Increment = 1;
            this.textHeight.Location = new System.Drawing.Point(58, 27);
            this.textHeight.Margin = new System.Windows.Forms.Padding(10, 0, 10, 0);
            this.textHeight.Maximum = 100;
            this.textHeight.Minimum = 1;
            this.textHeight.ReverseMouseWheelDirection = true;
            this.textHeight.Size = new System.Drawing.Size(62, 22);
            this.textHeight.Text = "1";
            this.textHeight.Value = 1;
            this.textHeight.KeyDown += new System.Windows.Forms.KeyEventHandler(this.text_KeyDown);
            // 
            // stackPanel2
            // 
            this.stackPanel2.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.stackPanel2.AutoSize = true;
            this.stackPanel2.Controls.Add(this.buttonCancel);
            this.stackPanel2.Controls.Add(this.buttonOk);
            this.stackPanel2.FlowDirection = System.Windows.Forms.FlowDirection.LeftToRight;
            this.stackPanel2.Location = new System.Drawing.Point(0, 70);
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
            // buttonOk
            // 
            this.buttonOk.Font = new System.Drawing.Font("Segoe UI", 8.25F);
            this.buttonOk.Location = new System.Drawing.Point(95, 3);
            this.buttonOk.Size = new System.Drawing.Size(86, 35);
            this.buttonOk.Text = "OK";
            this.buttonOk.UseVisualStyleBackColor = true;
            this.buttonOk.Click += new System.EventHandler(this.buttonOk_Click);
            // 
            // formGridSize
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
            this.AutoSize = true;
            this.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.ClientSize = new System.Drawing.Size(214, 186);
            this.Controls.Add(this.stackPanel1);
            this.Text = "Grid size";
            this.stackPanel1.ResumeLayout(false);
            this.stackPanel1.PerformLayout();
            this.tableLayoutPanel1.ResumeLayout(false);
            this.tableLayoutPanel1.PerformLayout();
            this.stackPanel2.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private Controls.StackPanel stackPanel1;
        private Controls.StackPanel stackPanel2;
        private System.Windows.Forms.Button buttonCancel;
        private System.Windows.Forms.Button buttonOk;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label5;
        private Controls.IntegerTextBox textWidth;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label6;
        private Controls.IntegerTextBox textHeight;
    }
}
