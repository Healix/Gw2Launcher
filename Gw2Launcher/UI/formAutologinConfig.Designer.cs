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
            this.labelSelectEmpty = new System.Windows.Forms.Label();
            this.labelSelectPlay = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // checkEmpty
            // 
            this.checkEmpty.AutoSize = true;
            this.checkEmpty.Location = new System.Drawing.Point(17, 125);
            this.checkEmpty.Name = "checkEmpty";
            this.checkEmpty.Size = new System.Drawing.Size(15, 14);
            this.checkEmpty.TabIndex = 3;
            this.checkEmpty.UseVisualStyleBackColor = true;
            this.checkEmpty.CheckedChanged += new System.EventHandler(this.checkEmpty_CheckedChanged);
            // 
            // textEmpty
            // 
            this.textEmpty.Enabled = false;
            this.textEmpty.Font = new System.Drawing.Font("Segoe UI", 8.25F);
            this.textEmpty.Location = new System.Drawing.Point(39, 122);
            this.textEmpty.Name = "textEmpty";
            this.textEmpty.Size = new System.Drawing.Size(82, 22);
            this.textEmpty.TabIndex = 4;
            this.textEmpty.Text = "0, 0";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Font = new System.Drawing.Font("Segoe UI Semibold", 9F, System.Drawing.FontStyle.Bold);
            this.label4.Location = new System.Drawing.Point(13, 72);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(74, 15);
            this.label4.TabIndex = 5;
            this.label4.Text = "Empty space";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("Segoe UI Semilight", 8.25F);
            this.label1.Location = new System.Drawing.Point(14, 88);
            this.label1.MaximumSize = new System.Drawing.Size(280, 0);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(264, 26);
            this.label1.TabIndex = 6;
            this.label1.Text = "Any empty area, such as to the right of the login or the sections used to move th" +
    "e launcher";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Font = new System.Drawing.Font("Segoe UI Semilight", 8.25F);
            this.label2.Location = new System.Drawing.Point(14, 173);
            this.label2.MaximumSize = new System.Drawing.Size(300, 0);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(225, 13);
            this.label2.TabIndex = 10;
            this.label2.Text = "Anywhere within the bounds of the play button";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Font = new System.Drawing.Font("Segoe UI Semibold", 9F, System.Drawing.FontStyle.Bold);
            this.label3.Location = new System.Drawing.Point(13, 157);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(68, 15);
            this.label3.TabIndex = 9;
            this.label3.Text = "Play button";
            // 
            // checkPlay
            // 
            this.checkPlay.AutoSize = true;
            this.checkPlay.Location = new System.Drawing.Point(17, 197);
            this.checkPlay.Name = "checkPlay";
            this.checkPlay.Size = new System.Drawing.Size(15, 14);
            this.checkPlay.TabIndex = 7;
            this.checkPlay.UseVisualStyleBackColor = true;
            this.checkPlay.CheckedChanged += new System.EventHandler(this.checkPlay_CheckedChanged);
            // 
            // textPlay
            // 
            this.textPlay.Enabled = false;
            this.textPlay.Font = new System.Drawing.Font("Segoe UI", 8.25F);
            this.textPlay.Location = new System.Drawing.Point(39, 194);
            this.textPlay.Name = "textPlay";
            this.textPlay.Size = new System.Drawing.Size(82, 22);
            this.textPlay.TabIndex = 8;
            this.textPlay.Text = "0, 0";
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Font = new System.Drawing.Font("Segoe UI Semilight", 8.25F);
            this.label5.Location = new System.Drawing.Point(13, 10);
            this.label5.MaximumSize = new System.Drawing.Size(280, 0);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(279, 26);
            this.label5.TabIndex = 11;
            this.label5.Text = "Begin by opening the launcher, then click and drag one of below targets to select" +
    " the area";
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Font = new System.Drawing.Font("Segoe UI Semilight", 8.25F);
            this.label7.ForeColor = System.Drawing.Color.Maroon;
            this.label7.Location = new System.Drawing.Point(13, 46);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(246, 13);
            this.label7.TabIndex = 13;
            this.label7.Text = "This should only be needed if the launcher changes";
            // 
            // buttonOK
            // 
            this.buttonOK.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonOK.Font = new System.Drawing.Font("Segoe UI", 8.25F);
            this.buttonOK.Location = new System.Drawing.Point(228, 207);
            this.buttonOK.Name = "buttonOK";
            this.buttonOK.Size = new System.Drawing.Size(86, 35);
            this.buttonOK.TabIndex = 101;
            this.buttonOK.Text = "OK";
            this.buttonOK.UseVisualStyleBackColor = true;
            this.buttonOK.Click += new System.EventHandler(this.buttonOK_Click);
            // 
            // labelSelectEmpty
            // 
            this.labelSelectEmpty.AutoSize = true;
            this.labelSelectEmpty.Cursor = System.Windows.Forms.Cursors.Cross;
            this.labelSelectEmpty.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.labelSelectEmpty.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(49)))), ((int)(((byte)(121)))), ((int)(((byte)(242)))));
            this.labelSelectEmpty.Location = new System.Drawing.Point(127, 120);
            this.labelSelectEmpty.Name = "labelSelectEmpty";
            this.labelSelectEmpty.Size = new System.Drawing.Size(21, 21);
            this.labelSelectEmpty.TabIndex = 102;
            this.labelSelectEmpty.Text = "+";
            this.labelSelectEmpty.MouseUp += new System.Windows.Forms.MouseEventHandler(this.labelSelectEmpty_MouseUp);
            // 
            // labelSelectPlay
            // 
            this.labelSelectPlay.AutoSize = true;
            this.labelSelectPlay.Cursor = System.Windows.Forms.Cursors.Cross;
            this.labelSelectPlay.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.labelSelectPlay.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(49)))), ((int)(((byte)(121)))), ((int)(((byte)(242)))));
            this.labelSelectPlay.Location = new System.Drawing.Point(127, 192);
            this.labelSelectPlay.Name = "labelSelectPlay";
            this.labelSelectPlay.Size = new System.Drawing.Size(21, 21);
            this.labelSelectPlay.TabIndex = 103;
            this.labelSelectPlay.Text = "+";
            this.labelSelectPlay.MouseUp += new System.Windows.Forms.MouseEventHandler(this.labelSelectPlay_MouseUp);
            // 
            // formAutologinConfig
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(326, 254);
            this.Controls.Add(this.labelSelectPlay);
            this.Controls.Add(this.labelSelectEmpty);
            this.Controls.Add(this.buttonOK);
            this.Controls.Add(this.label7);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.checkPlay);
            this.Controls.Add(this.textPlay);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.checkEmpty);
            this.Controls.Add(this.textEmpty);
            this.Controls.Add(this.label5);
            this.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.Icon = global::Gw2Launcher.Properties.Resources.Gw2Launcher;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "formAutologinConfig";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
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
        private System.Windows.Forms.Label labelSelectEmpty;
        private System.Windows.Forms.Label labelSelectPlay;
    }
}