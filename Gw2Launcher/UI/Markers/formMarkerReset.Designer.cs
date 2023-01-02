namespace Gw2Launcher.UI.Markers
{
    partial class formMarkerReset
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
            this.comboReset = new System.Windows.Forms.ComboBox();
            this.panelContainer = new Gw2Launcher.UI.Controls.StackPanel();
            this.label1 = new System.Windows.Forms.Label();
            this.numericDays = new Gw2Launcher.UI.Controls.IntegerTextBox();
            this.checkRelative = new System.Windows.Forms.CheckBox();
            this.panelSpecific = new Gw2Launcher.UI.Controls.StackPanel();
            this.dateCustom = new System.Windows.Forms.DateTimePicker();
            this.comboZone = new System.Windows.Forms.ComboBox();
            this.stackPanel2 = new Gw2Launcher.UI.Controls.StackPanel();
            this.buttonCancel = new System.Windows.Forms.Button();
            this.buttonOk = new System.Windows.Forms.Button();
            this.timeCustom = new Controls.TimeTextBox();
            this.stackPanel1 = new Gw2Launcher.UI.Controls.StackPanel();
            this.panelDuration = new Gw2Launcher.UI.Controls.StackPanel();
            this.stackPanel4 = new Gw2Launcher.UI.Controls.StackPanel();
            this.numericHours = new Gw2Launcher.UI.Controls.IntegerTextBox();
            this.label4 = new System.Windows.Forms.Label();
            this.stackPanel5 = new Gw2Launcher.UI.Controls.StackPanel();
            this.numericMinutes = new Gw2Launcher.UI.Controls.IntegerTextBox();
            this.label5 = new System.Windows.Forms.Label();
            this.panelContainer.SuspendLayout();
            this.panelSpecific.SuspendLayout();
            this.stackPanel2.SuspendLayout();
            this.stackPanel1.SuspendLayout();
            this.panelDuration.SuspendLayout();
            this.stackPanel4.SuspendLayout();
            this.stackPanel5.SuspendLayout();
            this.SuspendLayout();
            // 
            // comboReset
            // 
            this.comboReset.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboReset.FormattingEnabled = true;
            this.comboReset.Location = new System.Drawing.Point(0, 0);
            this.comboReset.Margin = new System.Windows.Forms.Padding(0, 0, 0, 6);
            this.comboReset.Size = new System.Drawing.Size(162, 21);
            this.comboReset.SelectedIndexChanged += new System.EventHandler(this.comboReset_SelectedIndexChanged);
            // 
            // panelContainer
            // 
            this.panelContainer.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.panelContainer.AutoSize = true;
            this.panelContainer.Controls.Add(this.comboReset);
            this.panelContainer.Controls.Add(this.panelDuration);
            this.panelContainer.Controls.Add(this.checkRelative);
            this.panelContainer.Controls.Add(this.panelSpecific);
            this.panelContainer.Controls.Add(this.stackPanel2);
            this.panelContainer.Location = new System.Drawing.Point(10, 10);
            this.panelContainer.Margin = new System.Windows.Forms.Padding(10);
            this.panelContainer.Size = new System.Drawing.Size(246, 172);
            // 
            // label1
            // 
            this.label1.Anchor = System.Windows.Forms.AnchorStyles.Top;
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("Segoe UI Semilight", 8.25F);
            this.label1.Location = new System.Drawing.Point(11, 26);
            this.label1.Margin = new System.Windows.Forms.Padding(0, 4, 0, 0);
            this.label1.Size = new System.Drawing.Size(28, 13);
            this.label1.Text = "days";
            this.label1.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // numericDays
            // 
            this.numericDays.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.numericDays.Increment = 1;
            this.numericDays.Location = new System.Drawing.Point(0, 0);
            this.numericDays.Margin = new System.Windows.Forms.Padding(0, 0, 0, 0);
            this.numericDays.Maximum = 1000;
            this.numericDays.Minimum = 0;
            this.numericDays.MinimumSize = new System.Drawing.Size(50, 0);
            this.numericDays.ReverseMouseWheelDirection = false;
            this.numericDays.Size = new System.Drawing.Size(50, 22);
            this.numericDays.Text = "0";
            this.numericDays.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            this.numericDays.Value = 0;
            // 
            // checkRelative
            // 
            this.checkRelative.AutoSize = true;
            this.checkRelative.Location = new System.Drawing.Point(0, 72);
            this.checkRelative.Margin = new System.Windows.Forms.Padding(0, 6, 0, 6);
            this.checkRelative.Size = new System.Drawing.Size(199, 17);
            this.checkRelative.Text = "Keep relative to the specified time";
            this.checkRelative.UseVisualStyleBackColor = true;
            // 
            // panelSpecific
            // 
            this.panelSpecific.AutoSize = true;
            this.panelSpecific.Controls.Add(this.dateCustom);
            this.panelSpecific.Controls.Add(this.timeCustom);
            this.panelSpecific.Controls.Add(this.comboZone);
            this.panelSpecific.FlowDirection = System.Windows.Forms.FlowDirection.LeftToRight;
            this.panelSpecific.Location = new System.Drawing.Point(0, 95);
            this.panelSpecific.Margin = new System.Windows.Forms.Padding(0);
            this.panelSpecific.Size = new System.Drawing.Size(228, 22);
            // 
            // dateCustom
            // 
            this.dateCustom.CustomFormat = "MM/dd/yyyy";
            this.dateCustom.Format = System.Windows.Forms.DateTimePickerFormat.Custom;
            this.dateCustom.Location = new System.Drawing.Point(0, 0);
            this.dateCustom.Margin = new System.Windows.Forms.Padding(0, 0, 6, 0);
            this.dateCustom.Size = new System.Drawing.Size(106, 22);
            // 
            // comboZone
            // 
            this.comboZone.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.comboZone.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboZone.FormattingEnabled = true;
            this.comboZone.Items.AddRange(new object[] {
            "Local",
            "UTC"});
            this.comboZone.Location = new System.Drawing.Point(163, 0);
            this.comboZone.Margin = new System.Windows.Forms.Padding(6, 0, 0, 0);
            this.comboZone.Size = new System.Drawing.Size(65, 21);
            // 
            // stackPanel2
            // 
            this.stackPanel2.Anchor = System.Windows.Forms.AnchorStyles.Bottom;
            this.stackPanel2.AutoSize = true;
            this.stackPanel2.Controls.Add(this.buttonCancel);
            this.stackPanel2.Controls.Add(this.buttonOk);
            this.stackPanel2.FlowDirection = System.Windows.Forms.FlowDirection.LeftToRight;
            this.stackPanel2.Location = new System.Drawing.Point(32, 137);
            this.stackPanel2.Margin = new System.Windows.Forms.Padding(0, 20, 0, 0);
            this.stackPanel2.Size = new System.Drawing.Size(182, 35);
            // 
            // buttonCancel
            // 
            this.buttonCancel.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.buttonCancel.Font = new System.Drawing.Font("Segoe UI", 8.25F);
            this.buttonCancel.Location = new System.Drawing.Point(0, 0);
            this.buttonCancel.Margin = new System.Windows.Forms.Padding(0);
            this.buttonCancel.Size = new System.Drawing.Size(86, 35);
            this.buttonCancel.Text = "Cancel";
            this.buttonCancel.UseVisualStyleBackColor = true;
            this.buttonCancel.Click += new System.EventHandler(this.buttonCancel_Click);
            // 
            // buttonOk
            // 
            this.buttonOk.Font = new System.Drawing.Font("Segoe UI", 8.25F);
            this.buttonOk.Location = new System.Drawing.Point(96, 0);
            this.buttonOk.Margin = new System.Windows.Forms.Padding(10, 0, 0, 0);
            this.buttonOk.Size = new System.Drawing.Size(86, 35);
            this.buttonOk.Text = "OK";
            this.buttonOk.UseVisualStyleBackColor = true;
            this.buttonOk.Click += new System.EventHandler(this.buttonOk_Click);
            // 
            // textTime
            // 
            this.timeCustom.Location = new System.Drawing.Point(112, 0);
            this.timeCustom.Margin = new System.Windows.Forms.Padding(0);
            this.timeCustom.Size = new System.Drawing.Size(45, 22);
            this.timeCustom.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            // 
            // stackPanel1
            // 
            this.stackPanel1.AutoSize = true;
            this.stackPanel1.Controls.Add(this.numericDays);
            this.stackPanel1.Controls.Add(this.label1);
            this.stackPanel1.Location = new System.Drawing.Point(0, 0);
            this.stackPanel1.Margin = new System.Windows.Forms.Padding(0);
            this.stackPanel1.Size = new System.Drawing.Size(50, 39);
            // 
            // panelDuration
            // 
            this.panelDuration.AutoSize = true;
            this.panelDuration.Controls.Add(this.stackPanel1);
            this.panelDuration.Controls.Add(this.stackPanel4);
            this.panelDuration.Controls.Add(this.stackPanel5);
            this.panelDuration.FlowDirection = System.Windows.Forms.FlowDirection.LeftToRight;
            this.panelDuration.Location = new System.Drawing.Point(0, 27);
            this.panelDuration.Margin = new System.Windows.Forms.Padding(0);
            this.panelDuration.Size = new System.Drawing.Size(162, 39);
            // 
            // stackPanel4
            // 
            this.stackPanel4.AutoSize = true;
            this.stackPanel4.Controls.Add(this.numericHours);
            this.stackPanel4.Controls.Add(this.label4);
            this.stackPanel4.Location = new System.Drawing.Point(56, 0);
            this.stackPanel4.Margin = new System.Windows.Forms.Padding(6, 0, 0, 0);
            this.stackPanel4.Size = new System.Drawing.Size(50, 39);
            // 
            // numericHours
            // 
            this.numericHours.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.numericHours.Increment = 1;
            this.numericHours.Location = new System.Drawing.Point(0, 0);
            this.numericHours.Margin = new System.Windows.Forms.Padding(0);
            this.numericHours.Maximum = 1000;
            this.numericHours.Minimum = 0;
            this.numericHours.MinimumSize = new System.Drawing.Size(50, 0);
            this.numericHours.ReverseMouseWheelDirection = false;
            this.numericHours.Size = new System.Drawing.Size(50, 22);
            this.numericHours.Text = "0";
            this.numericHours.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            this.numericHours.Value = 0;
            // 
            // label4
            // 
            this.label4.Anchor = System.Windows.Forms.AnchorStyles.Top;
            this.label4.AutoSize = true;
            this.label4.Font = new System.Drawing.Font("Segoe UI Semilight", 8.25F);
            this.label4.Location = new System.Drawing.Point(8, 26);
            this.label4.Margin = new System.Windows.Forms.Padding(0, 4, 0, 0);
            this.label4.Size = new System.Drawing.Size(33, 13);
            this.label4.Text = "hours";
            this.label4.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // stackPanel5
            // 
            this.stackPanel5.AutoSize = true;
            this.stackPanel5.Controls.Add(this.numericMinutes);
            this.stackPanel5.Controls.Add(this.label5);
            this.stackPanel5.Location = new System.Drawing.Point(112, 0);
            this.stackPanel5.Margin = new System.Windows.Forms.Padding(6, 0, 0, 0);
            this.stackPanel5.Size = new System.Drawing.Size(50, 39);
            // 
            // numericMinutes
            // 
            this.numericMinutes.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.numericMinutes.Increment = 1;
            this.numericMinutes.Location = new System.Drawing.Point(0, 0);
            this.numericMinutes.Margin = new System.Windows.Forms.Padding(0);
            this.numericMinutes.Maximum = 1000;
            this.numericMinutes.Minimum = 0;
            this.numericMinutes.MinimumSize = new System.Drawing.Size(50, 0);
            this.numericMinutes.ReverseMouseWheelDirection = false;
            this.numericMinutes.Size = new System.Drawing.Size(50, 22);
            this.numericMinutes.Text = "0";
            this.numericMinutes.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            this.numericMinutes.Value = 0;
            // 
            // label5
            // 
            this.label5.Anchor = System.Windows.Forms.AnchorStyles.Top;
            this.label5.AutoSize = true;
            this.label5.Font = new System.Drawing.Font("Segoe UI Semilight", 8.25F);
            this.label5.Location = new System.Drawing.Point(3, 26);
            this.label5.Margin = new System.Windows.Forms.Padding(0, 4, 0, 0);
            this.label5.Size = new System.Drawing.Size(43, 13);
            this.label5.Text = "minutes";
            this.label5.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // formMarkerReset
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
            this.AutoSize = true;
            this.ClientSize = new System.Drawing.Size(266, 191);
            this.Controls.Add(this.panelContainer);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.panelContainer.ResumeLayout(false);
            this.panelContainer.PerformLayout();
            this.panelSpecific.ResumeLayout(false);
            this.panelSpecific.PerformLayout();
            this.stackPanel2.ResumeLayout(false);
            this.stackPanel1.ResumeLayout(false);
            this.stackPanel1.PerformLayout();
            this.panelDuration.ResumeLayout(false);
            this.panelDuration.PerformLayout();
            this.stackPanel4.ResumeLayout(false);
            this.stackPanel4.PerformLayout();
            this.stackPanel5.ResumeLayout(false);
            this.stackPanel5.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.ComboBox comboReset;
        private Controls.StackPanel panelContainer;
        private System.Windows.Forms.Label label1;
        private Controls.IntegerTextBox numericDays;
        private Controls.StackPanel stackPanel2;
        private System.Windows.Forms.Button buttonCancel;
        private System.Windows.Forms.Button buttonOk;
        private System.Windows.Forms.CheckBox checkRelative;
        private Controls.StackPanel panelSpecific;
        private System.Windows.Forms.DateTimePicker dateCustom;
        private System.Windows.Forms.ComboBox comboZone;
        private Controls.TimeTextBox timeCustom;
        private Controls.StackPanel panelDuration;
        private Controls.StackPanel stackPanel1;
        private Controls.StackPanel stackPanel4;
        private Controls.IntegerTextBox numericHours;
        private System.Windows.Forms.Label label4;
        private Controls.StackPanel stackPanel5;
        private Controls.IntegerTextBox numericMinutes;
        private System.Windows.Forms.Label label5;
    }
}
