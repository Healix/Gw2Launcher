namespace Gw2Launcher.UI
{
    partial class formDailies
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.scrollV = new Gw2Launcher.UI.Controls.FlatVScrollBar();
            this.panelContainer = new System.Windows.Forms.Panel();
            this.panelContent = new System.Windows.Forms.Panel();
            this.waitingBounce = new Gw2Launcher.UI.Controls.WaitingBounce();
            this.labelMessage = new System.Windows.Forms.Label();
            this.buttonToday = new Gw2Launcher.UI.Controls.FlatVerticalButton();
            this.buttonTomorrow = new Gw2Launcher.UI.Controls.FlatVerticalButton();
            this.buttonMinimize = new Gw2Launcher.UI.Controls.FlatShapeButton();
            this.panelContainer.SuspendLayout();
            this.SuspendLayout();
            // 
            // scrollV
            // 
            this.scrollV.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.scrollV.Location = new System.Drawing.Point(388, 1);
            this.scrollV.Maximum = 0;
            this.scrollV.Name = "scrollV";
            this.scrollV.Size = new System.Drawing.Size(6, 418);
            this.scrollV.TabIndex = 0;
            this.scrollV.Value = 0;
            this.scrollV.ValueChanged += new System.EventHandler<int>(this.scrollV_ValueChanged);
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
            this.panelContainer.Size = new System.Drawing.Size(387, 418);
            this.panelContainer.TabIndex = 1;
            // 
            // panelContent
            // 
            this.panelContent.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.panelContent.BackColor = System.Drawing.SystemColors.Control;
            this.panelContent.Location = new System.Drawing.Point(0, 0);
            this.panelContent.Name = "panelContent";
            this.panelContent.Size = new System.Drawing.Size(387, 418);
            this.panelContent.TabIndex = 1;
            this.panelContent.Visible = false;
            // 
            // waitingBounce
            // 
            this.waitingBounce.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.waitingBounce.ForeColor = System.Drawing.SystemColors.ControlDark;
            this.waitingBounce.Location = new System.Drawing.Point(143, 201);
            this.waitingBounce.Name = "waitingBounce";
            this.waitingBounce.Size = new System.Drawing.Size(100, 16);
            this.waitingBounce.TabIndex = 3;
            this.waitingBounce.Visible = false;
            // 
            // labelMessage
            // 
            this.labelMessage.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.labelMessage.AutoSize = true;
            this.labelMessage.Location = new System.Drawing.Point(11, 8);
            this.labelMessage.Name = "labelMessage";
            this.labelMessage.Size = new System.Drawing.Size(61, 15);
            this.labelMessage.TabIndex = 2;
            this.labelMessage.Text = "[message]";
            this.labelMessage.Visible = false;
            // 
            // buttonToday
            // 
            this.buttonToday.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonToday.BackColorHovered = System.Drawing.SystemColors.ControlLight;
            this.buttonToday.BackColorSelected = System.Drawing.SystemColors.ControlLight;
            this.buttonToday.ForeColorHovered = System.Drawing.SystemColors.ControlText;
            this.buttonToday.ForeColorSelected = System.Drawing.SystemColors.ControlText;
            this.buttonToday.Location = new System.Drawing.Point(394, 1);
            this.buttonToday.Name = "buttonToday";
            this.buttonToday.Selected = false;
            this.buttonToday.Size = new System.Drawing.Size(30, 150);
            this.buttonToday.TabIndex = 2;
            this.buttonToday.Text = "Today";
            this.buttonToday.Click += new System.EventHandler(this.buttonToday_Click);
            // 
            // buttonTomorrow
            // 
            this.buttonTomorrow.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonTomorrow.BackColorHovered = System.Drawing.SystemColors.ControlLight;
            this.buttonTomorrow.BackColorSelected = System.Drawing.SystemColors.ControlLight;
            this.buttonTomorrow.ForeColorHovered = System.Drawing.SystemColors.ControlText;
            this.buttonTomorrow.ForeColorSelected = System.Drawing.SystemColors.ControlText;
            this.buttonTomorrow.Location = new System.Drawing.Point(394, 151);
            this.buttonTomorrow.Name = "buttonTomorrow";
            this.buttonTomorrow.Selected = false;
            this.buttonTomorrow.Size = new System.Drawing.Size(30, 150);
            this.buttonTomorrow.TabIndex = 3;
            this.buttonTomorrow.Text = "Tomorrow";
            this.buttonTomorrow.Click += new System.EventHandler(this.buttonTomorrow_Click);
            // 
            // buttonMinimize
            // 
            this.buttonMinimize.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonMinimize.BackColorHovered = System.Drawing.SystemColors.ControlLight;
            this.buttonMinimize.BackColorSelected = System.Drawing.SystemColors.ControlLight;
            this.buttonMinimize.ForeColor = System.Drawing.SystemColors.GrayText;
            this.buttonMinimize.ForeColorHovered = System.Drawing.SystemColors.ControlText;
            this.buttonMinimize.ForeColorSelected = System.Drawing.SystemColors.ControlText;
            this.buttonMinimize.Location = new System.Drawing.Point(394, 399);
            this.buttonMinimize.Name = "buttonMinimize";
            this.buttonMinimize.Padding = new System.Windows.Forms.Padding(5);
            this.buttonMinimize.Selected = false;
            this.buttonMinimize.Shape = Gw2Launcher.UI.Controls.FlatShapeButton.IconShape.Arrow;
            this.buttonMinimize.ShapeAlignment = System.Drawing.ContentAlignment.MiddleRight;
            this.buttonMinimize.ShapeDirection = System.Windows.Forms.ArrowDirection.Left;
            this.buttonMinimize.ShapeSize = new System.Drawing.Size(5, 9);
            this.buttonMinimize.Size = new System.Drawing.Size(30, 20);
            this.buttonMinimize.TabIndex = 5;
            this.buttonMinimize.Click += new System.EventHandler(this.buttonMinimize_Click);
            // 
            // formDailies
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            this.BackColor = System.Drawing.Color.White;
            this.ClientSize = new System.Drawing.Size(425, 420);
            this.Controls.Add(this.buttonMinimize);
            this.Controls.Add(this.buttonTomorrow);
            this.Controls.Add(this.buttonToday);
            this.Controls.Add(this.panelContainer);
            this.Controls.Add(this.scrollV);
            this.DoubleBuffered = true;
            this.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.Icon = global::Gw2Launcher.Properties.Resources.Gw2Launcher;
            this.MinimumSize = new System.Drawing.Size(330, 340);
            this.Name = "formDailies";
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.Manual;
            this.panelContainer.ResumeLayout(false);
            this.panelContainer.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private Controls.FlatVScrollBar scrollV;
        private System.Windows.Forms.Panel panelContainer;
        private System.Windows.Forms.Panel panelContent;
        private System.Windows.Forms.Label labelMessage;
        private Controls.WaitingBounce waitingBounce;
        private Controls.FlatVerticalButton buttonToday;
        private Controls.FlatVerticalButton buttonTomorrow;
        private Controls.FlatShapeButton buttonMinimize;
    }
}