namespace Gw2Launcher.UI
{
    partial class formNote
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
            this.label74 = new System.Windows.Forms.Label();
            this.textMessage = new System.Windows.Forms.TextBox();
            this.buttonOK = new System.Windows.Forms.Button();
            this.comboExpires = new System.Windows.Forms.ComboBox();
            this.dateCustom = new System.Windows.Forms.DateTimePicker();
            this.comboZone = new System.Windows.Forms.ComboBox();
            this.panelDuration = new System.Windows.Forms.Panel();
            this.label4 = new System.Windows.Forms.Label();
            this.numericSeconds = new Gw2Launcher.UI.Controls.IntegerTextBox();
            this.label3 = new System.Windows.Forms.Label();
            this.numericMinutes = new Gw2Launcher.UI.Controls.IntegerTextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.numericHours = new Gw2Launcher.UI.Controls.IntegerTextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.numericDays = new Gw2Launcher.UI.Controls.IntegerTextBox();
            this.panelSpecific = new System.Windows.Forms.Panel();
            this.checkNotify = new System.Windows.Forms.CheckBox();
            this.panelDuration.SuspendLayout();
            this.panelSpecific.SuspendLayout();
            this.SuspendLayout();
            // 
            // label74
            // 
            this.label74.AutoSize = true;
            this.label74.Font = new System.Drawing.Font("Segoe UI Semilight", 8.25F);
            this.label74.Location = new System.Drawing.Point(12, 124);
            this.label74.Name = "label74";
            this.label74.Size = new System.Drawing.Size(70, 13);
            this.label74.TabIndex = 104;
            this.label74.Text = "Expires after...";
            // 
            // textMessage
            // 
            this.textMessage.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.textMessage.Location = new System.Drawing.Point(12, 12);
            this.textMessage.Multiline = true;
            this.textMessage.Name = "textMessage";
            this.textMessage.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.textMessage.Size = new System.Drawing.Size(366, 99);
            this.textMessage.TabIndex = 0;
            // 
            // buttonOK
            // 
            this.buttonOK.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonOK.Font = new System.Drawing.Font("Segoe UI", 8.25F);
            this.buttonOK.Location = new System.Drawing.Point(292, 204);
            this.buttonOK.Name = "buttonOK";
            this.buttonOK.Size = new System.Drawing.Size(86, 35);
            this.buttonOK.TabIndex = 5;
            this.buttonOK.Text = "OK";
            this.buttonOK.UseVisualStyleBackColor = true;
            this.buttonOK.Click += new System.EventHandler(this.buttonOK_Click);
            // 
            // comboExpires
            // 
            this.comboExpires.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboExpires.FormattingEnabled = true;
            this.comboExpires.Location = new System.Drawing.Point(18, 145);
            this.comboExpires.Name = "comboExpires";
            this.comboExpires.Size = new System.Drawing.Size(162, 21);
            this.comboExpires.TabIndex = 1;
            this.comboExpires.SelectedIndexChanged += new System.EventHandler(this.comboExpires_SelectedIndexChanged);
            // 
            // dateCustom
            // 
            this.dateCustom.CustomFormat = "MM/dd/yyyy HH:mm:ss";
            this.dateCustom.Format = System.Windows.Forms.DateTimePickerFormat.Custom;
            this.dateCustom.Location = new System.Drawing.Point(0, 0);
            this.dateCustom.Name = "dateCustom";
            this.dateCustom.Size = new System.Drawing.Size(147, 22);
            this.dateCustom.TabIndex = 0;
            // 
            // comboZone
            // 
            this.comboZone.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboZone.FormattingEnabled = true;
            this.comboZone.Items.AddRange(new object[] {
            "Local",
            "UTC"});
            this.comboZone.Location = new System.Drawing.Point(153, 0);
            this.comboZone.Name = "comboZone";
            this.comboZone.Size = new System.Drawing.Size(65, 21);
            this.comboZone.TabIndex = 1;
            // 
            // panelDuration
            // 
            this.panelDuration.Controls.Add(this.label4);
            this.panelDuration.Controls.Add(this.numericSeconds);
            this.panelDuration.Controls.Add(this.label3);
            this.panelDuration.Controls.Add(this.numericMinutes);
            this.panelDuration.Controls.Add(this.label2);
            this.panelDuration.Controls.Add(this.numericHours);
            this.panelDuration.Controls.Add(this.label1);
            this.panelDuration.Controls.Add(this.numericDays);
            this.panelDuration.Location = new System.Drawing.Point(18, 175);
            this.panelDuration.Name = "panelDuration";
            this.panelDuration.Size = new System.Drawing.Size(238, 41);
            this.panelDuration.TabIndex = 3;
            this.panelDuration.Visible = false;
            // 
            // label4
            // 
            this.label4.Font = new System.Drawing.Font("Segoe UI Semilight", 8.25F);
            this.label4.Location = new System.Drawing.Point(168, 25);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(50, 13);
            this.label4.TabIndex = 130;
            this.label4.Text = "seconds";
            this.label4.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // numericSeconds
            // 
            this.numericSeconds.Increment = 1;
            this.numericSeconds.Location = new System.Drawing.Point(168, 0);
            this.numericSeconds.Maximum = 1000;
            this.numericSeconds.Minimum = 0;
            this.numericSeconds.Name = "numericSeconds";
            this.numericSeconds.Size = new System.Drawing.Size(50, 22);
            this.numericSeconds.TabIndex = 3;
            this.numericSeconds.Text = "0";
            this.numericSeconds.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            this.numericSeconds.Value = 0;
            // 
            // label3
            // 
            this.label3.Font = new System.Drawing.Font("Segoe UI Semilight", 8.25F);
            this.label3.Location = new System.Drawing.Point(112, 25);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(50, 13);
            this.label3.TabIndex = 128;
            this.label3.Text = "minutes";
            this.label3.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // numericMinutes
            // 
            this.numericMinutes.Increment = 1;
            this.numericMinutes.Location = new System.Drawing.Point(112, 0);
            this.numericMinutes.Maximum = 1000;
            this.numericMinutes.Minimum = 0;
            this.numericMinutes.Name = "numericMinutes";
            this.numericMinutes.Size = new System.Drawing.Size(50, 22);
            this.numericMinutes.TabIndex = 2;
            this.numericMinutes.Text = "0";
            this.numericMinutes.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            this.numericMinutes.Value = 0;
            // 
            // label2
            // 
            this.label2.Font = new System.Drawing.Font("Segoe UI Semilight", 8.25F);
            this.label2.Location = new System.Drawing.Point(56, 25);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(50, 13);
            this.label2.TabIndex = 126;
            this.label2.Text = "hours";
            this.label2.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // numericHours
            // 
            this.numericHours.Increment = 1;
            this.numericHours.Location = new System.Drawing.Point(56, 0);
            this.numericHours.Maximum = 1000;
            this.numericHours.Minimum = 0;
            this.numericHours.Name = "numericHours";
            this.numericHours.Size = new System.Drawing.Size(50, 22);
            this.numericHours.TabIndex = 1;
            this.numericHours.Text = "0";
            this.numericHours.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            this.numericHours.Value = 0;
            // 
            // label1
            // 
            this.label1.Font = new System.Drawing.Font("Segoe UI Semilight", 8.25F);
            this.label1.Location = new System.Drawing.Point(3, 25);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(47, 13);
            this.label1.TabIndex = 124;
            this.label1.Text = "days";
            this.label1.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // numericDays
            // 
            this.numericDays.Increment = 1;
            this.numericDays.Location = new System.Drawing.Point(0, 0);
            this.numericDays.Maximum = 1000;
            this.numericDays.Minimum = 0;
            this.numericDays.Name = "numericDays";
            this.numericDays.Size = new System.Drawing.Size(50, 22);
            this.numericDays.TabIndex = 0;
            this.numericDays.Text = "0";
            this.numericDays.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            this.numericDays.Value = 0;
            // 
            // panelSpecific
            // 
            this.panelSpecific.Controls.Add(this.dateCustom);
            this.panelSpecific.Controls.Add(this.comboZone);
            this.panelSpecific.Location = new System.Drawing.Point(18, 175);
            this.panelSpecific.Name = "panelSpecific";
            this.panelSpecific.Size = new System.Drawing.Size(238, 22);
            this.panelSpecific.TabIndex = 2;
            this.panelSpecific.Visible = false;
            // 
            // checkNotify
            // 
            this.checkNotify.AutoSize = true;
            this.checkNotify.Font = new System.Drawing.Font("Segoe UI", 8.25F);
            this.checkNotify.Location = new System.Drawing.Point(18, 225);
            this.checkNotify.Name = "checkNotify";
            this.checkNotify.Size = new System.Drawing.Size(130, 17);
            this.checkNotify.TabIndex = 4;
            this.checkNotify.Text = "Notify when expired";
            this.checkNotify.UseVisualStyleBackColor = true;
            this.checkNotify.CheckedChanged += new System.EventHandler(this.checkNotify_CheckedChanged);
            // 
            // formNote
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(390, 251);
            this.Controls.Add(this.panelSpecific);
            this.Controls.Add(this.comboExpires);
            this.Controls.Add(this.panelDuration);
            this.Controls.Add(this.checkNotify);
            this.Controls.Add(this.buttonOK);
            this.Controls.Add(this.textMessage);
            this.Controls.Add(this.label74);
            this.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.Icon = global::Gw2Launcher.Properties.Resources.Gw2;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "formNote";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Message";
            this.panelDuration.ResumeLayout(false);
            this.panelDuration.PerformLayout();
            this.panelSpecific.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label74;
        private System.Windows.Forms.TextBox textMessage;
        private System.Windows.Forms.Button buttonOK;
        private System.Windows.Forms.ComboBox comboExpires;
        private System.Windows.Forms.DateTimePicker dateCustom;
        private System.Windows.Forms.ComboBox comboZone;
        private System.Windows.Forms.Panel panelDuration;
        private Gw2Launcher.UI.Controls.IntegerTextBox numericDays;
        private System.Windows.Forms.Panel panelSpecific;
        private System.Windows.Forms.CheckBox checkNotify;
        private System.Windows.Forms.Label label4;
        private Gw2Launcher.UI.Controls.IntegerTextBox numericSeconds;
        private System.Windows.Forms.Label label3;
        private Gw2Launcher.UI.Controls.IntegerTextBox numericMinutes;
        private System.Windows.Forms.Label label2;
        private Gw2Launcher.UI.Controls.IntegerTextBox numericHours;
        private System.Windows.Forms.Label label1;
    }
}