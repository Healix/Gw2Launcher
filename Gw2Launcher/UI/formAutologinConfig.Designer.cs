namespace Gw2Launcher.UI
{
    partial class formAutologinConfig
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
            this.checkEmpty = new System.Windows.Forms.CheckBox();
            this.textEmpty = new System.Windows.Forms.TextBox();
            this.label4 = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.checkPlay = new System.Windows.Forms.CheckBox();
            this.textPlay = new System.Windows.Forms.TextBox();
            this.label5 = new System.Windows.Forms.Label();
            this.label7 = new System.Windows.Forms.Label();
            this.buttonOK = new System.Windows.Forms.Button();
            this.stackPanel1 = new Gw2Launcher.UI.Controls.StackPanel();
            this.stackPanel2 = new Gw2Launcher.UI.Controls.StackPanel();
            this.buttonSelectEmpty = new Gw2Launcher.UI.Controls.FlatShapeButton();
            this.stackPanel3 = new Gw2Launcher.UI.Controls.StackPanel();
            this.buttonSelectPlay = new Gw2Launcher.UI.Controls.FlatShapeButton();
            this.stackPanel1.SuspendLayout();
            this.stackPanel2.SuspendLayout();
            this.stackPanel3.SuspendLayout();
            this.SuspendLayout();
            // 
            // checkEmpty
            // 
            this.checkEmpty.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.checkEmpty.AutoSize = true;
            this.checkEmpty.Location = new System.Drawing.Point(0, 4);
            this.checkEmpty.Margin = new System.Windows.Forms.Padding(0);
            this.checkEmpty.Size = new System.Drawing.Size(15, 14);
            this.checkEmpty.UseVisualStyleBackColor = true;
            this.checkEmpty.CheckedChanged += new System.EventHandler(this.checkEmpty_CheckedChanged);
            // 
            // textEmpty
            // 
            this.textEmpty.Enabled = false;
            this.textEmpty.Font = new System.Drawing.Font("Segoe UI", 8.25F);
            this.textEmpty.Location = new System.Drawing.Point(23, 0);
            this.textEmpty.Margin = new System.Windows.Forms.Padding(8, 0, 0, 0);
            this.textEmpty.Size = new System.Drawing.Size(82, 22);
            this.textEmpty.Text = "0, 0";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Font = new System.Drawing.Font("Segoe UI Semibold", 9F, System.Drawing.FontStyle.Bold);
            this.label4.Location = new System.Drawing.Point(0, 60);
            this.label4.Margin = new System.Windows.Forms.Padding(0, 13, 0, 0);
            this.label4.Size = new System.Drawing.Size(74, 15);
            this.label4.Text = "Empty space";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("Segoe UI Semilight", 8.25F);
            this.label1.Location = new System.Drawing.Point(1, 76);
            this.label1.Margin = new System.Windows.Forms.Padding(1, 1, 0, 8);
            this.label1.Size = new System.Drawing.Size(264, 26);
            this.label1.Text = "Any empty area, such as to the right of the login or the sections used to move th" +
    "e launcher";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Font = new System.Drawing.Font("Segoe UI Semilight", 8.25F);
            this.label2.Location = new System.Drawing.Point(1, 161);
            this.label2.Margin = new System.Windows.Forms.Padding(1, 1, 0, 8);
            this.label2.MaximumSize = new System.Drawing.Size(300, 0);
            this.label2.Size = new System.Drawing.Size(225, 13);
            this.label2.Text = "Anywhere within the bounds of the play button";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Font = new System.Drawing.Font("Segoe UI Semibold", 9F, System.Drawing.FontStyle.Bold);
            this.label3.Location = new System.Drawing.Point(0, 145);
            this.label3.Margin = new System.Windows.Forms.Padding(0, 13, 0, 0);
            this.label3.Size = new System.Drawing.Size(68, 15);
            this.label3.Text = "Play button";
            // 
            // checkPlay
            // 
            this.checkPlay.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.checkPlay.AutoSize = true;
            this.checkPlay.Location = new System.Drawing.Point(0, 4);
            this.checkPlay.Margin = new System.Windows.Forms.Padding(0);
            this.checkPlay.Size = new System.Drawing.Size(15, 14);
            this.checkPlay.UseVisualStyleBackColor = true;
            this.checkPlay.CheckedChanged += new System.EventHandler(this.checkPlay_CheckedChanged);
            // 
            // textPlay
            // 
            this.textPlay.Enabled = false;
            this.textPlay.Font = new System.Drawing.Font("Segoe UI", 8.25F);
            this.textPlay.Location = new System.Drawing.Point(23, 0);
            this.textPlay.Margin = new System.Windows.Forms.Padding(8, 0, 0, 0);
            this.textPlay.Size = new System.Drawing.Size(82, 22);
            this.textPlay.Text = "0, 0";
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Font = new System.Drawing.Font("Segoe UI Semilight", 8.25F);
            this.label5.Location = new System.Drawing.Point(0, 0);
            this.label5.Margin = new System.Windows.Forms.Padding(0);
            this.label5.Size = new System.Drawing.Size(279, 26);
            this.label5.Text = "Begin by opening the launcher, then click and drag one of below targets to select" +
    " the area";
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Font = new System.Drawing.Font("Segoe UI Semilight", 8.25F);
            this.label7.ForeColor = System.Drawing.Color.Maroon;
            this.label7.Location = new System.Drawing.Point(0, 34);
            this.label7.Margin = new System.Windows.Forms.Padding(0, 8, 0, 0);
            this.label7.Size = new System.Drawing.Size(246, 13);
            this.label7.Text = "This should only be needed if the launcher changes";
            // 
            // buttonOK
            // 
            this.buttonOK.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonOK.Font = new System.Drawing.Font("Segoe UI", 8.25F);
            this.buttonOK.Location = new System.Drawing.Point(193, 214);
            this.buttonOK.Margin = new System.Windows.Forms.Padding(0, 10, 0, 0);
            this.buttonOK.Size = new System.Drawing.Size(86, 35);
            this.buttonOK.Text = "OK";
            this.buttonOK.UseVisualStyleBackColor = true;
            this.buttonOK.Click += new System.EventHandler(this.buttonOK_Click);
            // 
            // stackPanel1
            // 
            this.stackPanel1.AutoSize = true;
            this.stackPanel1.Controls.Add(this.label5);
            this.stackPanel1.Controls.Add(this.label7);
            this.stackPanel1.Controls.Add(this.label4);
            this.stackPanel1.Controls.Add(this.label1);
            this.stackPanel1.Controls.Add(this.stackPanel2);
            this.stackPanel1.Controls.Add(this.label3);
            this.stackPanel1.Controls.Add(this.label2);
            this.stackPanel1.Controls.Add(this.stackPanel3);
            this.stackPanel1.Controls.Add(this.buttonOK);
            this.stackPanel1.Location = new System.Drawing.Point(13, 13);
            this.stackPanel1.Margin = new System.Windows.Forms.Padding(13);
            this.stackPanel1.Size = new System.Drawing.Size(279, 249);
            // 
            // stackPanel2
            // 
            this.stackPanel2.AutoSize = true;
            this.stackPanel2.Controls.Add(this.checkEmpty);
            this.stackPanel2.Controls.Add(this.textEmpty);
            this.stackPanel2.Controls.Add(this.buttonSelectEmpty);
            this.stackPanel2.FlowDirection = System.Windows.Forms.FlowDirection.LeftToRight;
            this.stackPanel2.Location = new System.Drawing.Point(8, 110);
            this.stackPanel2.Margin = new System.Windows.Forms.Padding(8, 0, 0, 0);
            this.stackPanel2.Size = new System.Drawing.Size(131, 22);
            // 
            // buttonSelectEmpty
            // 
            this.buttonSelectEmpty.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.buttonSelectEmpty.BackColorHovered = System.Drawing.SystemColors.Control;
            this.buttonSelectEmpty.BackColorSelected = System.Drawing.SystemColors.Control;
            this.buttonSelectEmpty.BorderColor = System.Drawing.Color.Empty;
            this.buttonSelectEmpty.Cursor = System.Windows.Forms.Cursors.Cross;
            this.buttonSelectEmpty.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(49)))), ((int)(((byte)(121)))), ((int)(((byte)(242)))));
            this.buttonSelectEmpty.ForeColorHovered = System.Drawing.Color.FromArgb(((int)(((byte)(49)))), ((int)(((byte)(121)))), ((int)(((byte)(242)))));
            this.buttonSelectEmpty.ForeColorSelected = System.Drawing.Color.FromArgb(((int)(((byte)(49)))), ((int)(((byte)(121)))), ((int)(((byte)(242)))));
            this.buttonSelectEmpty.Location = new System.Drawing.Point(111, 1);
            this.buttonSelectEmpty.Margin = new System.Windows.Forms.Padding(6, 0, 0, 0);
            this.buttonSelectEmpty.Selected = false;
            this.buttonSelectEmpty.Shape = Gw2Launcher.UI.Controls.FlatShapeButton.IconShape.Plus;
            this.buttonSelectEmpty.ShapeAlignment = System.Drawing.ContentAlignment.MiddleCenter;
            this.buttonSelectEmpty.ShapeDirection = System.Windows.Forms.ArrowDirection.Left;
            this.buttonSelectEmpty.ShapeSize = new System.Drawing.Size(10, 10);
            this.buttonSelectEmpty.Size = new System.Drawing.Size(20, 20);
            this.buttonSelectEmpty.MouseUp += new System.Windows.Forms.MouseEventHandler(this.labelSelectEmpty_MouseUp);
            // 
            // stackPanel3
            // 
            this.stackPanel3.AutoSize = true;
            this.stackPanel3.Controls.Add(this.checkPlay);
            this.stackPanel3.Controls.Add(this.textPlay);
            this.stackPanel3.Controls.Add(this.buttonSelectPlay);
            this.stackPanel3.FlowDirection = System.Windows.Forms.FlowDirection.LeftToRight;
            this.stackPanel3.Location = new System.Drawing.Point(8, 182);
            this.stackPanel3.Margin = new System.Windows.Forms.Padding(8, 0, 0, 0);
            this.stackPanel3.Size = new System.Drawing.Size(131, 22);
            // 
            // buttonSelectPlay
            // 
            this.buttonSelectPlay.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.buttonSelectPlay.BackColorHovered = System.Drawing.SystemColors.Control;
            this.buttonSelectPlay.BackColorSelected = System.Drawing.SystemColors.Control;
            this.buttonSelectPlay.BorderColor = System.Drawing.Color.Empty;
            this.buttonSelectPlay.Cursor = System.Windows.Forms.Cursors.Cross;
            this.buttonSelectPlay.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(49)))), ((int)(((byte)(121)))), ((int)(((byte)(242)))));
            this.buttonSelectPlay.ForeColorHovered = System.Drawing.Color.FromArgb(((int)(((byte)(49)))), ((int)(((byte)(121)))), ((int)(((byte)(242)))));
            this.buttonSelectPlay.ForeColorSelected = System.Drawing.Color.FromArgb(((int)(((byte)(49)))), ((int)(((byte)(121)))), ((int)(((byte)(242)))));
            this.buttonSelectPlay.Location = new System.Drawing.Point(111, 1);
            this.buttonSelectPlay.Margin = new System.Windows.Forms.Padding(6, 0, 0, 0);
            this.buttonSelectPlay.Selected = false;
            this.buttonSelectPlay.Shape = Gw2Launcher.UI.Controls.FlatShapeButton.IconShape.Plus;
            this.buttonSelectPlay.ShapeAlignment = System.Drawing.ContentAlignment.MiddleCenter;
            this.buttonSelectPlay.ShapeDirection = System.Windows.Forms.ArrowDirection.Left;
            this.buttonSelectPlay.ShapeSize = new System.Drawing.Size(10, 10);
            this.buttonSelectPlay.Size = new System.Drawing.Size(20, 20);
            this.buttonSelectPlay.MouseUp += new System.Windows.Forms.MouseEventHandler(this.labelSelectPlay_MouseUp);
            // 
            // formAutologinConfig
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
            this.AutoSize = true;
            this.AutoSizeFill = Gw2Launcher.UI.Base.StackFormBase.AutoSizeFillMode.Height;
            this.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.ClientSize = new System.Drawing.Size(305, 275);
            this.Controls.Add(this.stackPanel1);
            this.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
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

        private System.Windows.Forms.CheckBox checkEmpty;
        private System.Windows.Forms.TextBox textEmpty;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.CheckBox checkPlay;
        private System.Windows.Forms.TextBox textPlay;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.Button buttonOK;
        private Controls.StackPanel stackPanel1;
        private Controls.StackPanel stackPanel2;
        private Controls.StackPanel stackPanel3;
        private Controls.FlatShapeButton buttonSelectEmpty;
        private Controls.FlatShapeButton buttonSelectPlay;
    }
}
