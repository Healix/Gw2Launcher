namespace Gw2Launcher.UI.ColorPicker
{
    partial class formColorDialog
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
            this.panel8 = new System.Windows.Forms.Panel();
            this.panelOriginal = new Gw2Launcher.UI.ColorPicker.Controls.ColorPreviewPanel();
            this.panelPreview = new Gw2Launcher.UI.ColorPicker.Controls.ColorPreviewPanel();
            this.panel7 = new System.Windows.Forms.Panel();
            this.panelAlpha = new Gw2Launcher.UI.ColorPicker.Controls.AlphaPanel();
            this.panel6 = new System.Windows.Forms.Panel();
            this.panelHue = new Gw2Launcher.UI.ColorPicker.Controls.HuePanel();
            this.panel5 = new System.Windows.Forms.Panel();
            this.panelShade = new Gw2Launcher.UI.ColorPicker.Controls.ShadePanel();
            this.label5 = new System.Windows.Forms.Label();
            this.textHex = new System.Windows.Forms.TextBox();
            this.label4 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.buttonCancel = new System.Windows.Forms.Button();
            this.buttonOK = new System.Windows.Forms.Button();
            this.panelValues = new System.Windows.Forms.Panel();
            this.textA = new Gw2Launcher.UI.Controls.IntegerTextBox();
            this.textR = new Gw2Launcher.UI.Controls.IntegerTextBox();
            this.textG = new Gw2Launcher.UI.Controls.IntegerTextBox();
            this.textB = new Gw2Launcher.UI.Controls.IntegerTextBox();
            this.panelColors = new Gw2Launcher.UI.Controls.StackPanel();
            this.panel12 = new System.Windows.Forms.Panel();
            this.panelDefaultColor = new Gw2Launcher.UI.ColorPicker.Controls.ColorPreviewPanel();
            this.label6 = new System.Windows.Forms.Label();
            this.panel8.SuspendLayout();
            this.panel7.SuspendLayout();
            this.panel6.SuspendLayout();
            this.panel5.SuspendLayout();
            this.panelValues.SuspendLayout();
            this.panelColors.SuspendLayout();
            this.panel12.SuspendLayout();
            this.SuspendLayout();
            // 
            // panel8
            // 
            this.panel8.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(60)))), ((int)(((byte)(60)))));
            this.panel8.Controls.Add(this.panelOriginal);
            this.panel8.Controls.Add(this.panelPreview);
            this.panel8.Location = new System.Drawing.Point(0, 0);
            this.panel8.Size = new System.Drawing.Size(94, 106);
            // 
            // panelOriginal
            // 
            this.panelOriginal.Color1 = System.Drawing.Color.Empty;
            this.panelOriginal.Color2 = System.Drawing.Color.Empty;
            this.panelOriginal.Cursor = System.Windows.Forms.Cursors.Hand;
            this.panelOriginal.Label = "old";
            this.panelOriginal.LabelAlignment = System.Drawing.ContentAlignment.BottomRight;
            this.panelOriginal.Location = new System.Drawing.Point(2, 54);
            this.panelOriginal.OffsetCheckers = true;
            this.panelOriginal.Padding = new System.Windows.Forms.Padding(2);
            this.panelOriginal.ShowColor2 = false;
            this.panelOriginal.Size = new System.Drawing.Size(90, 50);
            this.panelOriginal.Click += new System.EventHandler(this.panelOriginal_Click);
            // 
            // panelPreview
            // 
            this.panelPreview.Color1 = System.Drawing.Color.Empty;
            this.panelPreview.Color2 = System.Drawing.Color.Empty;
            this.panelPreview.Label = "new";
            this.panelPreview.LabelAlignment = System.Drawing.ContentAlignment.BottomRight;
            this.panelPreview.Location = new System.Drawing.Point(2, 2);
            this.panelPreview.OffsetCheckers = false;
            this.panelPreview.Padding = new System.Windows.Forms.Padding(2);
            this.panelPreview.ShowColor2 = false;
            this.panelPreview.Size = new System.Drawing.Size(90, 50);
            // 
            // panel7
            // 
            this.panel7.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(60)))), ((int)(((byte)(60)))));
            this.panel7.Controls.Add(this.panelAlpha);
            this.panel7.Location = new System.Drawing.Point(352, 12);
            this.panel7.Size = new System.Drawing.Size(33, 264);
            // 
            // panelAlpha
            // 
            this.panelAlpha.AllowAlphaTransparency = true;
            this.panelAlpha.Alpha = 1F;
            this.panelAlpha.BackColor = System.Drawing.Color.White;
            this.panelAlpha.CursorSize = 14;
            this.panelAlpha.Location = new System.Drawing.Point(2, 2);
            this.panelAlpha.Size = new System.Drawing.Size(29, 260);
            this.panelAlpha.AlphaChanged += new System.EventHandler<float>(this.panelAlpha_AlphaChanged);
            // 
            // panel6
            // 
            this.panel6.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(60)))), ((int)(((byte)(60)))));
            this.panel6.Controls.Add(this.panelHue);
            this.panel6.Location = new System.Drawing.Point(313, 12);
            this.panel6.Size = new System.Drawing.Size(33, 264);
            // 
            // panelHue
            // 
            this.panelHue.CursorSize = 14;
            this.panelHue.Hue = 240;
            this.panelHue.Location = new System.Drawing.Point(2, 2);
            this.panelHue.Size = new System.Drawing.Size(29, 260);
            this.panelHue.HueChanged += new System.EventHandler<int>(this.panelHue_HueChanged);
            // 
            // panel5
            // 
            this.panel5.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(60)))), ((int)(((byte)(60)))));
            this.panel5.Controls.Add(this.panelShade);
            this.panel5.Location = new System.Drawing.Point(12, 12);
            this.panel5.Size = new System.Drawing.Size(296, 264);
            // 
            // panelShade
            // 
            this.panelShade.CursorSize = 14;
            this.panelShade.Hue = 0;
            this.panelShade.Location = new System.Drawing.Point(2, 2);
            this.panelShade.SelectedColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))));
            this.panelShade.Size = new System.Drawing.Size(292, 260);
            this.panelShade.ColorChanged += new System.EventHandler<System.Drawing.Color>(this.panelShade_ColorChanged);
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(7, 237);
            this.label5.Size = new System.Drawing.Size(14, 13);
            this.label5.Text = "#";
            // 
            // textHex
            // 
            this.textHex.Location = new System.Drawing.Point(27, 234);
            this.textHex.MaxLength = 8;
            this.textHex.Size = new System.Drawing.Size(59, 22);
            this.textHex.Text = "00000000";
            this.textHex.TextChanged += new System.EventHandler(this.textHex_TextChanged);
            this.textHex.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.textHex_KeyPress);
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(7, 208);
            this.label4.Size = new System.Drawing.Size(13, 13);
            this.label4.Text = "B";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(7, 180);
            this.label3.Size = new System.Drawing.Size(15, 13);
            this.label3.Text = "G";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(7, 152);
            this.label2.Size = new System.Drawing.Size(14, 13);
            this.label2.Text = "R";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(7, 124);
            this.label1.Size = new System.Drawing.Size(14, 13);
            this.label1.Text = "A";
            // 
            // buttonCancel
            // 
            this.buttonCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonCancel.Font = new System.Drawing.Font("Segoe UI", 8.25F);
            this.buttonCancel.Location = new System.Drawing.Point(312, 298);
            this.buttonCancel.Size = new System.Drawing.Size(86, 35);
            this.buttonCancel.Text = "Cancel";
            this.buttonCancel.UseVisualStyleBackColor = true;
            this.buttonCancel.Click += new System.EventHandler(this.buttonCancel_Click);
            // 
            // buttonOK
            // 
            this.buttonOK.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonOK.Font = new System.Drawing.Font("Segoe UI", 8.25F);
            this.buttonOK.Location = new System.Drawing.Point(408, 298);
            this.buttonOK.Size = new System.Drawing.Size(86, 35);
            this.buttonOK.Text = "OK";
            this.buttonOK.UseVisualStyleBackColor = true;
            this.buttonOK.Click += new System.EventHandler(this.buttonOK_Click);
            // 
            // panelValues
            // 
            this.panelValues.Controls.Add(this.panel8);
            this.panelValues.Controls.Add(this.label1);
            this.panelValues.Controls.Add(this.label2);
            this.panelValues.Controls.Add(this.label5);
            this.panelValues.Controls.Add(this.label3);
            this.panelValues.Controls.Add(this.textHex);
            this.panelValues.Controls.Add(this.label4);
            this.panelValues.Controls.Add(this.textA);
            this.panelValues.Controls.Add(this.textR);
            this.panelValues.Controls.Add(this.textG);
            this.panelValues.Controls.Add(this.textB);
            this.panelValues.Location = new System.Drawing.Point(391, 12);
            this.panelValues.Size = new System.Drawing.Size(94, 264);
            // 
            // textA
            // 
            this.textA.Increment = 1;
            this.textA.Location = new System.Drawing.Point(27, 122);
            this.textA.Maximum = 255;
            this.textA.MaxLength = 3;
            this.textA.Minimum = 0;
            this.textA.ReverseMouseWheelDirection = false;
            this.textA.Size = new System.Drawing.Size(59, 22);
            this.textA.Text = "0";
            this.textA.Value = 0;
            this.textA.ValueChanged += new System.EventHandler(this.textA_ValueChanged);
            // 
            // textR
            // 
            this.textR.Increment = 1;
            this.textR.Location = new System.Drawing.Point(27, 150);
            this.textR.Maximum = 255;
            this.textR.MaxLength = 3;
            this.textR.Minimum = 0;
            this.textR.ReverseMouseWheelDirection = false;
            this.textR.Size = new System.Drawing.Size(59, 22);
            this.textR.Text = "0";
            this.textR.Value = 0;
            this.textR.ValueChanged += new System.EventHandler(this.textRGB_ValueChanged);
            // 
            // textG
            // 
            this.textG.Increment = 1;
            this.textG.Location = new System.Drawing.Point(27, 178);
            this.textG.Maximum = 255;
            this.textG.MaxLength = 3;
            this.textG.Minimum = 0;
            this.textG.ReverseMouseWheelDirection = false;
            this.textG.Size = new System.Drawing.Size(59, 22);
            this.textG.Text = "0";
            this.textG.Value = 0;
            this.textG.ValueChanged += new System.EventHandler(this.textRGB_ValueChanged);
            // 
            // textB
            // 
            this.textB.Increment = 1;
            this.textB.Location = new System.Drawing.Point(27, 206);
            this.textB.Maximum = 255;
            this.textB.MaxLength = 3;
            this.textB.Minimum = 0;
            this.textB.ReverseMouseWheelDirection = false;
            this.textB.Size = new System.Drawing.Size(59, 22);
            this.textB.Text = "0";
            this.textB.Value = 0;
            this.textB.ValueChanged += new System.EventHandler(this.textRGB_ValueChanged);
            // 
            // panelColors
            // 
            this.panelColors.Controls.Add(this.panel12);
            this.panelColors.Controls.Add(this.label6);
            this.panelColors.FlowDirection = System.Windows.Forms.FlowDirection.LeftToRight;
            this.panelColors.Location = new System.Drawing.Point(12, 282);
            this.panelColors.Size = new System.Drawing.Size(296, 51);
            this.panelColors.Visible = false;
            // 
            // panel12
            // 
            this.panel12.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.panel12.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(60)))), ((int)(((byte)(60)))));
            this.panel12.Controls.Add(this.panelDefaultColor);
            this.panel12.Location = new System.Drawing.Point(0, 10);
            this.panel12.Margin = new System.Windows.Forms.Padding(0, 0, 3, 0);
            this.panel12.Size = new System.Drawing.Size(30, 30);
            // 
            // panelDefaultColor
            // 
            this.panelDefaultColor.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.panelDefaultColor.Color1 = System.Drawing.Color.Empty;
            this.panelDefaultColor.Color2 = System.Drawing.Color.Empty;
            this.panelDefaultColor.Cursor = System.Windows.Forms.Cursors.Hand;
            this.panelDefaultColor.Label = null;
            this.panelDefaultColor.LabelAlignment = System.Drawing.ContentAlignment.BottomRight;
            this.panelDefaultColor.Location = new System.Drawing.Point(2, 2);
            this.panelDefaultColor.OffsetCheckers = false;
            this.panelDefaultColor.ShowColor2 = false;
            this.panelDefaultColor.Size = new System.Drawing.Size(26, 26);
            this.panelDefaultColor.Click += new System.EventHandler(this.panelOriginal_Click);
            // 
            // label6
            // 
            this.label6.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.label6.AutoSize = true;
            this.label6.ForeColor = System.Drawing.SystemColors.GrayText;
            this.label6.Location = new System.Drawing.Point(36, 18);
            this.label6.Margin = new System.Windows.Forms.Padding(3, 0, 3, 1);
            this.label6.Size = new System.Drawing.Size(44, 13);
            this.label6.Text = "default";
            // 
            // formColorDialog
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
            this.ClientSize = new System.Drawing.Size(506, 345);
            this.Controls.Add(this.panelColors);
            this.Controls.Add(this.panelValues);
            this.Controls.Add(this.buttonCancel);
            this.Controls.Add(this.buttonOK);
            this.Controls.Add(this.panel7);
            this.Controls.Add(this.panel6);
            this.Controls.Add(this.panel5);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.Icon = global::Gw2Launcher.Properties.Resources.Gw2Launcher;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.panel8.ResumeLayout(false);
            this.panel7.ResumeLayout(false);
            this.panel6.ResumeLayout(false);
            this.panel5.ResumeLayout(false);
            this.panelValues.ResumeLayout(false);
            this.panelValues.PerformLayout();
            this.panelColors.ResumeLayout(false);
            this.panelColors.PerformLayout();
            this.panel12.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Panel panel8;
        private System.Windows.Forms.Panel panel7;
        private System.Windows.Forms.Panel panel6;
        private System.Windows.Forms.Panel panel5;
        private Controls.HuePanel panelHue;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.TextBox textHex;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label1;
        private Controls.ColorPreviewPanel panelOriginal;
        private Controls.ColorPreviewPanel panelPreview;
        private Controls.AlphaPanel panelAlpha;
        private Controls.ShadePanel panelShade;
        private System.Windows.Forms.Button buttonCancel;
        private System.Windows.Forms.Button buttonOK;
        private System.Windows.Forms.Panel panelValues;
        private UI.Controls.IntegerTextBox textB;
        private UI.Controls.IntegerTextBox textG;
        private UI.Controls.IntegerTextBox textR;
        private UI.Controls.IntegerTextBox textA;
        private UI.Controls.StackPanel panelColors;
        private System.Windows.Forms.Panel panel12;
        private Controls.ColorPreviewPanel panelDefaultColor;
        private System.Windows.Forms.Label label6;
    }
}
