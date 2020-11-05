namespace Gw2Launcher.UI
{
    partial class formAccountType
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
            this.buttonGw1 = new Gw2Launcher.UI.formAccountType.SelectButton();
            this.buttonGw2 = new Gw2Launcher.UI.formAccountType.SelectButton();
            this.stackPanel1 = new Gw2Launcher.UI.Controls.StackPanel();
            this.stackPanel1.SuspendLayout();
            this.SuspendLayout();
            // 
            // buttonGw1
            // 
            this.buttonGw1.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(240)))), ((int)(((byte)(240)))), ((int)(((byte)(240)))));
            this.buttonGw1.BackColorHovered = System.Drawing.Color.FromArgb(((int)(((byte)(230)))), ((int)(((byte)(236)))), ((int)(((byte)(244)))));
            this.buttonGw1.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(200)))), ((int)(((byte)(200)))), ((int)(((byte)(200)))));
            this.buttonGw1.BorderColorHovered = System.Drawing.Color.FromArgb(((int)(((byte)(190)))), ((int)(((byte)(196)))), ((int)(((byte)(204)))));
            this.buttonGw1.Cursor = System.Windows.Forms.Cursors.Hand;
            this.buttonGw1.Font = new System.Drawing.Font("Segoe UI", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.buttonGw1.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(50)))), ((int)(((byte)(50)))), ((int)(((byte)(50)))));
            this.buttonGw1.ForeColorHovered = System.Drawing.Color.Black;
            this.buttonGw1.Location = new System.Drawing.Point(0, 0);
            this.buttonGw1.Margin = new System.Windows.Forms.Padding(0);
            this.buttonGw1.Size = new System.Drawing.Size(230, 60);
            this.buttonGw1.Text = "Guild Wars 1";
            this.buttonGw1.Click += new System.EventHandler(this.buttonGw1_Click);
            // 
            // buttonGw2
            // 
            this.buttonGw2.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(240)))), ((int)(((byte)(240)))), ((int)(((byte)(240)))));
            this.buttonGw2.BackColorHovered = System.Drawing.Color.FromArgb(((int)(((byte)(230)))), ((int)(((byte)(236)))), ((int)(((byte)(244)))));
            this.buttonGw2.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(200)))), ((int)(((byte)(200)))), ((int)(((byte)(200)))));
            this.buttonGw2.BorderColorHovered = System.Drawing.Color.FromArgb(((int)(((byte)(190)))), ((int)(((byte)(196)))), ((int)(((byte)(204)))));
            this.buttonGw2.Cursor = System.Windows.Forms.Cursors.Hand;
            this.buttonGw2.Font = new System.Drawing.Font("Segoe UI", 9.75F);
            this.buttonGw2.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(50)))), ((int)(((byte)(50)))), ((int)(((byte)(50)))));
            this.buttonGw2.ForeColorHovered = System.Drawing.Color.Black;
            this.buttonGw2.Location = new System.Drawing.Point(0, 70);
            this.buttonGw2.Margin = new System.Windows.Forms.Padding(0, 10, 0, 0);
            this.buttonGw2.Size = new System.Drawing.Size(230, 60);
            this.buttonGw2.Text = "Guild Wars 2";
            this.buttonGw2.Click += new System.EventHandler(this.buttonGw2_Click);
            // 
            // stackPanel1
            // 
            this.stackPanel1.AutoSize = true;
            this.stackPanel1.Controls.Add(this.buttonGw1);
            this.stackPanel1.Controls.Add(this.buttonGw2);
            this.stackPanel1.Location = new System.Drawing.Point(25, 70);
            this.stackPanel1.Margin = new System.Windows.Forms.Padding(25, 70, 25, 25);
            this.stackPanel1.Size = new System.Drawing.Size(230, 130);
            // 
            // formAccountType
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
            this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(248)))), ((int)(((byte)(248)))), ((int)(((byte)(248)))));
            this.ClientSize = new System.Drawing.Size(280, 225);
            this.Controls.Add(this.stackPanel1);
            this.Icon = global::Gw2Launcher.Properties.Resources.Gw2Launcher;
            this.Text = "Select account type";
            this.stackPanel1.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private SelectButton buttonGw1;
        private SelectButton buttonGw2;
        private Controls.StackPanel stackPanel1;


    }
}
