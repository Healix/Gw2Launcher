namespace Gw2Launcher.UI
{
    partial class formMoveToPage
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
            this.textPage = new Gw2Launcher.UI.Controls.IntegerTextBox();
            this.stackPanel2 = new Gw2Launcher.UI.Controls.StackPanel();
            this.buttonCancel = new System.Windows.Forms.Button();
            this.buttonOk = new System.Windows.Forms.Button();
            this.stackPanel1 = new Gw2Launcher.UI.Controls.StackPanel();
            this.stackPanel3 = new Gw2Launcher.UI.Controls.StackPanel();
            this.arrowFirst = new Gw2Launcher.UI.Controls.FlatShapeButton();
            this.arrowPrevious = new Gw2Launcher.UI.Controls.FlatShapeButton();
            this.arrowNext = new Gw2Launcher.UI.Controls.FlatShapeButton();
            this.arrowLast = new Gw2Launcher.UI.Controls.FlatShapeButton();
            this.checkRemoveCurrent = new System.Windows.Forms.CheckBox();
            this.stackPanel2.SuspendLayout();
            this.stackPanel1.SuspendLayout();
            this.stackPanel3.SuspendLayout();
            this.SuspendLayout();
            // 
            // textPage
            // 
            this.textPage.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.textPage.Increment = 1;
            this.textPage.Location = new System.Drawing.Point(46, 0);
            this.textPage.Margin = new System.Windows.Forms.Padding(10, 0, 10, 0);
            this.textPage.Maximum = 99;
            this.textPage.Minimum = 0;
            this.textPage.ReverseMouseWheelDirection = true;
            this.textPage.Size = new System.Drawing.Size(62, 22);
            this.textPage.Text = "0";
            this.textPage.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            this.textPage.Value = 0;
            this.textPage.KeyDown += new System.Windows.Forms.KeyEventHandler(this.textPage_KeyDown);
            // 
            // stackPanel2
            // 
            this.stackPanel2.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.stackPanel2.AutoSize = true;
            this.stackPanel2.Controls.Add(this.buttonCancel);
            this.stackPanel2.Controls.Add(this.buttonOk);
            this.stackPanel2.FlowDirection = System.Windows.Forms.FlowDirection.LeftToRight;
            this.stackPanel2.Location = new System.Drawing.Point(0, 60);
            this.stackPanel2.Margin = new System.Windows.Forms.Padding(0, 5, 0, 0);
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
            // stackPanel1
            // 
            this.stackPanel1.AutoSize = true;
            this.stackPanel1.Controls.Add(this.stackPanel3);
            this.stackPanel1.Controls.Add(this.checkRemoveCurrent);
            this.stackPanel1.Controls.Add(this.stackPanel2);
            this.stackPanel1.Location = new System.Drawing.Point(15, 60);
            this.stackPanel1.Margin = new System.Windows.Forms.Padding(15, 60, 15, 15);
            this.stackPanel1.Size = new System.Drawing.Size(184, 101);
            // 
            // stackPanel3
            // 
            this.stackPanel3.Anchor = System.Windows.Forms.AnchorStyles.Top;
            this.stackPanel3.AutoSize = true;
            this.stackPanel3.AutoSizeFill = Gw2Launcher.UI.Controls.StackPanel.AutoSizeFillMode.NoWrap;
            this.stackPanel3.Controls.Add(this.arrowFirst);
            this.stackPanel3.Controls.Add(this.arrowPrevious);
            this.stackPanel3.Controls.Add(this.textPage);
            this.stackPanel3.Controls.Add(this.arrowNext);
            this.stackPanel3.Controls.Add(this.arrowLast);
            this.stackPanel3.FlowDirection = System.Windows.Forms.FlowDirection.LeftToRight;
            this.stackPanel3.Location = new System.Drawing.Point(15, 3);
            this.stackPanel3.Size = new System.Drawing.Size(154, 22);
            // 
            // arrowFirst
            // 
            this.arrowFirst.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left)));
            this.arrowFirst.BackColorHovered = System.Drawing.Color.FromArgb(((int)(((byte)(248)))), ((int)(((byte)(248)))), ((int)(((byte)(248)))));
            this.arrowFirst.BackColorSelected = System.Drawing.Color.FromArgb(((int)(((byte)(248)))), ((int)(((byte)(248)))), ((int)(((byte)(248)))));
            this.arrowFirst.BorderColor = System.Drawing.Color.Empty;
            this.arrowFirst.Cursor = System.Windows.Forms.Cursors.Hand;
            this.arrowFirst.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(150)))), ((int)(((byte)(150)))), ((int)(((byte)(150)))));
            this.arrowFirst.ForeColorHovered = System.Drawing.Color.FromArgb(((int)(((byte)(20)))), ((int)(((byte)(20)))), ((int)(((byte)(20)))));
            this.arrowFirst.ForeColorSelected = System.Drawing.Color.FromArgb(((int)(((byte)(150)))), ((int)(((byte)(150)))), ((int)(((byte)(150)))));
            this.arrowFirst.Location = new System.Drawing.Point(0, 0);
            this.arrowFirst.Margin = new System.Windows.Forms.Padding(0);
            this.arrowFirst.Selected = false;
            this.arrowFirst.Shape = Gw2Launcher.UI.Controls.FlatShapeButton.IconShape.ArrowAndLine;
            this.arrowFirst.ShapeSize = new System.Drawing.Size(5, 11);
            this.arrowFirst.Size = new System.Drawing.Size(24, 22);
            this.arrowFirst.TabStop = false;
            this.arrowFirst.Click += new System.EventHandler(this.arrowFirst_Click);
            // 
            // arrowPrevious
            // 
            this.arrowPrevious.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left)));
            this.arrowPrevious.BackColorHovered = System.Drawing.Color.FromArgb(((int)(((byte)(248)))), ((int)(((byte)(248)))), ((int)(((byte)(248)))));
            this.arrowPrevious.BackColorSelected = System.Drawing.Color.FromArgb(((int)(((byte)(248)))), ((int)(((byte)(248)))), ((int)(((byte)(248)))));
            this.arrowPrevious.BorderColor = System.Drawing.Color.Empty;
            this.arrowPrevious.Cursor = System.Windows.Forms.Cursors.Hand;
            this.arrowPrevious.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(150)))), ((int)(((byte)(150)))), ((int)(((byte)(150)))));
            this.arrowPrevious.ForeColorHovered = System.Drawing.Color.FromArgb(((int)(((byte)(20)))), ((int)(((byte)(20)))), ((int)(((byte)(20)))));
            this.arrowPrevious.ForeColorSelected = System.Drawing.Color.FromArgb(((int)(((byte)(150)))), ((int)(((byte)(150)))), ((int)(((byte)(150)))));
            this.arrowPrevious.Location = new System.Drawing.Point(24, 0);
            this.arrowPrevious.Margin = new System.Windows.Forms.Padding(0);
            this.arrowPrevious.Selected = false;
            this.arrowPrevious.ShapeSize = new System.Drawing.Size(5, 11);
            this.arrowPrevious.Size = new System.Drawing.Size(12, 22);
            this.arrowPrevious.TabStop = false;
            this.arrowPrevious.Click += new System.EventHandler(this.arrowPrevious_Click);
            // 
            // arrowNext
            // 
            this.arrowNext.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left)));
            this.arrowNext.BackColorHovered = System.Drawing.Color.FromArgb(((int)(((byte)(248)))), ((int)(((byte)(248)))), ((int)(((byte)(248)))));
            this.arrowNext.BackColorSelected = System.Drawing.Color.FromArgb(((int)(((byte)(248)))), ((int)(((byte)(248)))), ((int)(((byte)(248)))));
            this.arrowNext.BorderColor = System.Drawing.Color.Empty;
            this.arrowNext.Cursor = System.Windows.Forms.Cursors.Hand;
            this.arrowNext.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(150)))), ((int)(((byte)(150)))), ((int)(((byte)(150)))));
            this.arrowNext.ForeColorHovered = System.Drawing.Color.FromArgb(((int)(((byte)(20)))), ((int)(((byte)(20)))), ((int)(((byte)(20)))));
            this.arrowNext.ForeColorSelected = System.Drawing.Color.FromArgb(((int)(((byte)(150)))), ((int)(((byte)(150)))), ((int)(((byte)(150)))));
            this.arrowNext.Location = new System.Drawing.Point(118, 0);
            this.arrowNext.Margin = new System.Windows.Forms.Padding(0);
            this.arrowNext.Selected = false;
            this.arrowNext.ShapeDirection = System.Windows.Forms.ArrowDirection.Right;
            this.arrowNext.ShapeSize = new System.Drawing.Size(5, 11);
            this.arrowNext.Size = new System.Drawing.Size(12, 22);
            this.arrowNext.TabStop = false;
            this.arrowNext.Click += new System.EventHandler(this.arrowNext_Click);
            // 
            // arrowLast
            // 
            this.arrowLast.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left)));
            this.arrowLast.BackColorHovered = System.Drawing.Color.FromArgb(((int)(((byte)(248)))), ((int)(((byte)(248)))), ((int)(((byte)(248)))));
            this.arrowLast.BackColorSelected = System.Drawing.Color.FromArgb(((int)(((byte)(248)))), ((int)(((byte)(248)))), ((int)(((byte)(248)))));
            this.arrowLast.BorderColor = System.Drawing.Color.Empty;
            this.arrowLast.Cursor = System.Windows.Forms.Cursors.Hand;
            this.arrowLast.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(150)))), ((int)(((byte)(150)))), ((int)(((byte)(150)))));
            this.arrowLast.ForeColorHovered = System.Drawing.Color.FromArgb(((int)(((byte)(20)))), ((int)(((byte)(20)))), ((int)(((byte)(20)))));
            this.arrowLast.ForeColorSelected = System.Drawing.Color.FromArgb(((int)(((byte)(150)))), ((int)(((byte)(150)))), ((int)(((byte)(150)))));
            this.arrowLast.Location = new System.Drawing.Point(130, 0);
            this.arrowLast.Margin = new System.Windows.Forms.Padding(0);
            this.arrowLast.Selected = false;
            this.arrowLast.Shape = Gw2Launcher.UI.Controls.FlatShapeButton.IconShape.ArrowAndLine;
            this.arrowLast.ShapeDirection = System.Windows.Forms.ArrowDirection.Right;
            this.arrowLast.ShapeSize = new System.Drawing.Size(5, 11);
            this.arrowLast.Size = new System.Drawing.Size(24, 22);
            this.arrowLast.TabStop = false;
            this.arrowLast.Click += new System.EventHandler(this.arrowLast_Click);
            // 
            // checkRemoveCurrent
            // 
            this.checkRemoveCurrent.AutoSize = true;
            this.checkRemoveCurrent.Location = new System.Drawing.Point(5, 38);
            this.checkRemoveCurrent.Margin = new System.Windows.Forms.Padding(5, 10, 0, 0);
            this.checkRemoveCurrent.Size = new System.Drawing.Size(162, 17);
            this.checkRemoveCurrent.Text = "Remove from current page";
            this.checkRemoveCurrent.UseVisualStyleBackColor = true;
            // 
            // formMoveToPage
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
            this.AutoSize = true;
            this.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.ClientSize = new System.Drawing.Size(214, 176);
            this.Controls.Add(this.stackPanel1);
            this.Text = "Page selection";
            this.stackPanel2.ResumeLayout(false);
            this.stackPanel1.ResumeLayout(false);
            this.stackPanel1.PerformLayout();
            this.stackPanel3.ResumeLayout(false);
            this.stackPanel3.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private Controls.IntegerTextBox textPage;
        private Controls.StackPanel stackPanel2;
        private System.Windows.Forms.Button buttonCancel;
        private System.Windows.Forms.Button buttonOk;
        private Controls.StackPanel stackPanel1;
        private Controls.StackPanel stackPanel3;
        private Controls.FlatShapeButton arrowFirst;
        private Controls.FlatShapeButton arrowPrevious;
        private Controls.FlatShapeButton arrowNext;
        private Controls.FlatShapeButton arrowLast;
        private System.Windows.Forms.CheckBox checkRemoveCurrent;
    }
}
