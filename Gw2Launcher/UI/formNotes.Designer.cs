namespace Gw2Launcher.UI
{
    partial class formNotes
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
            this.panelContainer = new System.Windows.Forms.Panel();
            this.panelContent = new System.Windows.Forms.Panel();
            this.labelMessage = new System.Windows.Forms.Label();
            this.panelAdd = new System.Windows.Forms.Panel();
            this.labelAccountName = new System.Windows.Forms.Label();
            this.labelAdd = new System.Windows.Forms.Label();
            this.waitingBounce = new Gw2Launcher.UI.Controls.WaitingBounce();
            this.scrollV = new Gw2Launcher.UI.Controls.FlatVScrollBar();
            this.buttonMinimize = new Gw2Launcher.UI.Controls.FlatShapeButton();
            this.buttonExpired = new Gw2Launcher.UI.Controls.FlatVerticalButton();
            this.buttonMessages = new Gw2Launcher.UI.Controls.FlatVerticalButton();
            this.panelContainer.SuspendLayout();
            this.panelAdd.SuspendLayout();
            this.SuspendLayout();
            // 
            // panelContainer
            // 
            this.panelContainer.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.panelContainer.Controls.Add(this.panelContent);
            this.panelContainer.Controls.Add(this.waitingBounce);
            this.panelContainer.Controls.Add(this.labelMessage);
            this.panelContainer.Location = new System.Drawing.Point(1, 1);
            this.panelContainer.Name = "panelContainer";
            this.panelContainer.Size = new System.Drawing.Size(387, 303);
            this.panelContainer.TabIndex = 7;
            // 
            // panelContent
            // 
            this.panelContent.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.panelContent.BackColor = System.Drawing.SystemColors.Control;
            this.panelContent.Location = new System.Drawing.Point(0, 0);
            this.panelContent.Name = "panelContent";
            this.panelContent.Size = new System.Drawing.Size(387, 303);
            this.panelContent.TabIndex = 1;
            this.panelContent.Visible = false;
            // 
            // labelMessage
            // 
            this.labelMessage.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.labelMessage.AutoSize = true;
            this.labelMessage.Location = new System.Drawing.Point(11, -28);
            this.labelMessage.Name = "labelMessage";
            this.labelMessage.Size = new System.Drawing.Size(61, 15);
            this.labelMessage.TabIndex = 2;
            this.labelMessage.Text = "[message]";
            this.labelMessage.Visible = false;
            // 
            // panelAdd
            // 
            this.panelAdd.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.panelAdd.Controls.Add(this.labelAccountName);
            this.panelAdd.Controls.Add(this.labelAdd);
            this.panelAdd.Location = new System.Drawing.Point(1, 304);
            this.panelAdd.Name = "panelAdd";
            this.panelAdd.Size = new System.Drawing.Size(387, 31);
            this.panelAdd.TabIndex = 118;
            // 
            // labelAccountName
            // 
            this.labelAccountName.AutoEllipsis = true;
            this.labelAccountName.ForeColor = System.Drawing.SystemColors.GrayText;
            this.labelAccountName.Location = new System.Drawing.Point(45, 8);
            this.labelAccountName.Name = "labelAccountName";
            this.labelAccountName.Size = new System.Drawing.Size(336, 15);
            this.labelAccountName.TabIndex = 0;
            this.labelAccountName.Text = "-";
            this.labelAccountName.TextAlign = System.Drawing.ContentAlignment.TopRight;
            // 
            // labelAdd
            // 
            this.labelAdd.AutoSize = true;
            this.labelAdd.Cursor = System.Windows.Forms.Cursors.Hand;
            this.labelAdd.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(49)))), ((int)(((byte)(121)))), ((int)(((byte)(242)))));
            this.labelAdd.Location = new System.Drawing.Point(10, 8);
            this.labelAdd.Name = "labelAdd";
            this.labelAdd.Size = new System.Drawing.Size(29, 15);
            this.labelAdd.TabIndex = 117;
            this.labelAdd.Text = "new";
            this.labelAdd.Click += new System.EventHandler(this.labelAdd_Click);
            // 
            // waitingBounce
            // 
            this.waitingBounce.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.waitingBounce.ForeColor = System.Drawing.SystemColors.ControlDark;
            this.waitingBounce.Location = new System.Drawing.Point(143, 165);
            this.waitingBounce.Name = "waitingBounce";
            this.waitingBounce.Size = new System.Drawing.Size(100, 16);
            this.waitingBounce.TabIndex = 3;
            this.waitingBounce.Visible = false;
            // 
            // scrollV
            // 
            this.scrollV.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.scrollV.Location = new System.Drawing.Point(388, 1);
            this.scrollV.Maximum = 0;
            this.scrollV.Name = "scrollV";
            this.scrollV.Size = new System.Drawing.Size(6, 334);
            this.scrollV.TabIndex = 6;
            this.scrollV.Value = 0;
            this.scrollV.ValueChanged += new System.EventHandler<int>(this.scrollV_ValueChanged);
            // 
            // buttonMinimize
            // 
            this.buttonMinimize.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonMinimize.BackColorHovered = System.Drawing.SystemColors.ControlLight;
            this.buttonMinimize.BackColorSelected = System.Drawing.SystemColors.ControlLight;
            this.buttonMinimize.ForeColor = System.Drawing.SystemColors.GrayText;
            this.buttonMinimize.ForeColorHovered = System.Drawing.SystemColors.ControlText;
            this.buttonMinimize.ForeColorSelected = System.Drawing.SystemColors.ControlText;
            this.buttonMinimize.Location = new System.Drawing.Point(394, 315);
            this.buttonMinimize.Name = "buttonMinimize";
            this.buttonMinimize.Padding = new System.Windows.Forms.Padding(5);
            this.buttonMinimize.Selected = false;
            this.buttonMinimize.Shape = Gw2Launcher.UI.Controls.FlatShapeButton.IconShape.X;
            this.buttonMinimize.ShapeAlignment = System.Drawing.ContentAlignment.MiddleRight;
            this.buttonMinimize.ShapeDirection = System.Windows.Forms.ArrowDirection.Left;
            this.buttonMinimize.ShapeSize = new System.Drawing.Size(8, 8);
            this.buttonMinimize.Size = new System.Drawing.Size(30, 20);
            this.buttonMinimize.TabIndex = 10;
            this.buttonMinimize.Click += new System.EventHandler(this.buttonMinimize_Click);
            // 
            // buttonExpired
            // 
            this.buttonExpired.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonExpired.BackColorHovered = System.Drawing.SystemColors.ControlLight;
            this.buttonExpired.BackColorSelected = System.Drawing.SystemColors.ControlLight;
            this.buttonExpired.ForeColorHovered = System.Drawing.SystemColors.ControlText;
            this.buttonExpired.ForeColorSelected = System.Drawing.SystemColors.ControlText;
            this.buttonExpired.Location = new System.Drawing.Point(394, 121);
            this.buttonExpired.Name = "buttonExpired";
            this.buttonExpired.Selected = false;
            this.buttonExpired.Size = new System.Drawing.Size(30, 120);
            this.buttonExpired.TabIndex = 9;
            this.buttonExpired.Text = "Recent";
            this.buttonExpired.Click += new System.EventHandler(this.buttonExpired_Click);
            // 
            // buttonMessages
            // 
            this.buttonMessages.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonMessages.BackColorHovered = System.Drawing.SystemColors.ControlLight;
            this.buttonMessages.BackColorSelected = System.Drawing.SystemColors.ControlLight;
            this.buttonMessages.ForeColorHovered = System.Drawing.SystemColors.ControlText;
            this.buttonMessages.ForeColorSelected = System.Drawing.SystemColors.ControlText;
            this.buttonMessages.Location = new System.Drawing.Point(394, 1);
            this.buttonMessages.Name = "buttonMessages";
            this.buttonMessages.Selected = false;
            this.buttonMessages.Size = new System.Drawing.Size(30, 120);
            this.buttonMessages.TabIndex = 8;
            this.buttonMessages.Text = "Notes";
            this.buttonMessages.Click += new System.EventHandler(this.buttonMessages_Click);
            // 
            // formNotes
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.White;
            this.ClientSize = new System.Drawing.Size(425, 336);
            this.Controls.Add(this.panelContainer);
            this.Controls.Add(this.panelAdd);
            this.Controls.Add(this.scrollV);
            this.Controls.Add(this.buttonMinimize);
            this.Controls.Add(this.buttonExpired);
            this.Controls.Add(this.buttonMessages);
            this.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.Icon = global::Gw2Launcher.Properties.Resources.Gw2;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "formNotes";
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Messages";
            this.panelContainer.ResumeLayout(false);
            this.panelContainer.PerformLayout();
            this.panelAdd.ResumeLayout(false);
            this.panelAdd.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Panel panelContainer;
        private System.Windows.Forms.Panel panelContent;
        private Controls.WaitingBounce waitingBounce;
        private System.Windows.Forms.Label labelMessage;
        private Controls.FlatVScrollBar scrollV;
        private Controls.FlatShapeButton buttonMinimize;
        private Controls.FlatVerticalButton buttonExpired;
        private Controls.FlatVerticalButton buttonMessages;
        private System.Windows.Forms.Label labelAdd;
        private System.Windows.Forms.Panel panelAdd;
        private System.Windows.Forms.Label labelAccountName;

    }
}