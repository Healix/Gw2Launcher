namespace Gw2Launcher.UI.WindowPositioning
{
    partial class formTemplates
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
            this.components = new System.ComponentModel.Container();
            this.panelTemplatesScroll = new Gw2Launcher.UI.Controls.AutoScrollContainerPanel();
            this.panelTemplatesContainer = new Gw2Launcher.UI.Controls.StackPanel();
            this.panelTemplates = new System.Windows.Forms.FlowLayoutPanel();
            this.stackPanel1 = new Gw2Launcher.UI.Controls.StackPanel();
            this.labelCustomNew = new Controls.LinkLabel();
            this.label7 = new System.Windows.Forms.Label();
            this.labelCustomNewFromLayout = new Controls.LinkLabel();
            this.autoScrollContainerPanel1 = new Gw2Launcher.UI.Controls.AutoScrollContainerPanel();
            this.panelOptionsContainer = new Gw2Launcher.UI.Controls.StackPanel();
            this.label1 = new System.Windows.Forms.Label();
            this.checkOverlapTaskbar = new System.Windows.Forms.CheckBox();
            this.checkFlipY = new System.Windows.Forms.CheckBox();
            this.checkFlipX = new System.Windows.Forms.CheckBox();
            this.labelTemplate = new System.Windows.Forms.Label();
            this.textTemplate = new Gw2Launcher.UI.Controls.IntegerTextBox();
            this.panelOptions = new Gw2Launcher.UI.Controls.StackPanel();
            this.panelCommands = new Gw2Launcher.UI.Controls.StackPanel();
            this.labelCustomize = new Controls.LinkLabel();
            this.labelCustomizeSep = new System.Windows.Forms.Label();
            this.labelDelete = new Controls.LinkLabel();
            this.labelReset = new Controls.LinkLabel();
            this.label2 = new System.Windows.Forms.Label();
            this.panelScreen = new Gw2Launcher.UI.Controls.StackPanel();
            this.checkScreen1 = new System.Windows.Forms.CheckBox();
            this.label5 = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.checkSnapEdge = new System.Windows.Forms.CheckBox();
            this.checkSnapShift = new System.Windows.Forms.CheckBox();
            this.contextCustom = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.editToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.deleteToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.panelTemplatesScroll.SuspendLayout();
            this.panelTemplatesContainer.SuspendLayout();
            this.stackPanel1.SuspendLayout();
            this.autoScrollContainerPanel1.SuspendLayout();
            this.panelOptionsContainer.SuspendLayout();
            this.panelCommands.SuspendLayout();
            this.panelScreen.SuspendLayout();
            this.contextCustom.SuspendLayout();
            this.SuspendLayout();
            // 
            // panelTemplatesScroll
            // 
            this.panelTemplatesScroll.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.panelTemplatesScroll.Controls.Add(this.panelTemplatesContainer);
            this.panelTemplatesScroll.Location = new System.Drawing.Point(12, 12);
            this.panelTemplatesScroll.Size = new System.Drawing.Size(369, 255);
            // 
            // panelTemplatesContainer
            // 
            this.panelTemplatesContainer.AutoSize = true;
            this.panelTemplatesContainer.AutoSizeFill = Gw2Launcher.UI.Controls.StackPanel.AutoSizeFillMode.Width;
            this.panelTemplatesContainer.Controls.Add(this.panelTemplates);
            this.panelTemplatesContainer.Controls.Add(this.stackPanel1);
            this.panelTemplatesContainer.Location = new System.Drawing.Point(0, 0);
            this.panelTemplatesContainer.Margin = new System.Windows.Forms.Padding(0);
            this.panelTemplatesContainer.Size = new System.Drawing.Size(369, 43);
            // 
            // panelTemplates
            // 
            this.panelTemplates.AutoSize = true;
            this.panelTemplates.Location = new System.Drawing.Point(0, 0);
            this.panelTemplates.Margin = new System.Windows.Forms.Padding(0);
            this.panelTemplates.MinimumSize = new System.Drawing.Size(20, 20);
            this.panelTemplates.Size = new System.Drawing.Size(20, 20);
            // 
            // stackPanel1
            // 
            this.stackPanel1.AutoSize = true;
            this.stackPanel1.AutoSizeFill = Gw2Launcher.UI.Controls.StackPanel.AutoSizeFillMode.NoWrap;
            this.stackPanel1.Controls.Add(this.labelCustomNew);
            this.stackPanel1.Controls.Add(this.label7);
            this.stackPanel1.Controls.Add(this.labelCustomNewFromLayout);
            this.stackPanel1.FlowDirection = System.Windows.Forms.FlowDirection.LeftToRight;
            this.stackPanel1.Location = new System.Drawing.Point(0, 30);
            this.stackPanel1.Margin = new System.Windows.Forms.Padding(0, 10, 0, 0);
            this.stackPanel1.Size = new System.Drawing.Size(151, 13);
            // 
            // labelCustomNew
            // 
            this.labelCustomNew.AutoSize = true;
            this.labelCustomNew.Location = new System.Drawing.Point(0, 0);
            this.labelCustomNew.Margin = new System.Windows.Forms.Padding(0);
            this.labelCustomNew.Size = new System.Drawing.Size(29, 13);
            this.labelCustomNew.Text = "new";
            this.labelCustomNew.Click += new System.EventHandler(this.labelCustomNew_Click);
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.ForeColor = System.Drawing.SystemColors.GrayText;
            this.label7.Location = new System.Drawing.Point(32, 0);
            this.label7.Size = new System.Drawing.Size(10, 13);
            this.label7.Text = "|";
            // 
            // labelCustomNewFromLayout
            // 
            this.labelCustomNewFromLayout.AutoSize = true;
            this.labelCustomNewFromLayout.Location = new System.Drawing.Point(45, 0);
            this.labelCustomNewFromLayout.Margin = new System.Windows.Forms.Padding(0);
            this.labelCustomNewFromLayout.Size = new System.Drawing.Size(106, 13);
            this.labelCustomNewFromLayout.Text = "from current layout";
            this.labelCustomNewFromLayout.Click += new System.EventHandler(this.labelCustomNewFromLayout_Click);
            // 
            // autoScrollContainerPanel1
            // 
            this.autoScrollContainerPanel1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.autoScrollContainerPanel1.Controls.Add(this.panelOptionsContainer);
            this.autoScrollContainerPanel1.Location = new System.Drawing.Point(387, 12);
            this.autoScrollContainerPanel1.Size = new System.Drawing.Size(174, 255);
            // 
            // panelOptionsContainer
            // 
            this.panelOptionsContainer.AutoSize = true;
            this.panelOptionsContainer.AutoSizeFill = Gw2Launcher.UI.Controls.StackPanel.AutoSizeFillMode.Width;
            this.panelOptionsContainer.Controls.Add(this.label1);
            this.panelOptionsContainer.Controls.Add(this.checkOverlapTaskbar);
            this.panelOptionsContainer.Controls.Add(this.checkFlipY);
            this.panelOptionsContainer.Controls.Add(this.checkFlipX);
            this.panelOptionsContainer.Controls.Add(this.labelTemplate);
            this.panelOptionsContainer.Controls.Add(this.textTemplate);
            this.panelOptionsContainer.Controls.Add(this.panelOptions);
            this.panelOptionsContainer.Controls.Add(this.panelCommands);
            this.panelOptionsContainer.Controls.Add(this.label2);
            this.panelOptionsContainer.Controls.Add(this.panelScreen);
            this.panelOptionsContainer.Controls.Add(this.label5);
            this.panelOptionsContainer.Controls.Add(this.label4);
            this.panelOptionsContainer.Controls.Add(this.checkSnapEdge);
            this.panelOptionsContainer.Controls.Add(this.checkSnapShift);
            this.panelOptionsContainer.Location = new System.Drawing.Point(0, 0);
            this.panelOptionsContainer.Margin = new System.Windows.Forms.Padding(0);
            this.panelOptionsContainer.Size = new System.Drawing.Size(157, 304);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("Segoe UI Semibold", 8.75F, System.Drawing.FontStyle.Bold);
            this.label1.Location = new System.Drawing.Point(0, 0);
            this.label1.Margin = new System.Windows.Forms.Padding(0, 0, 0, 8);
            this.label1.Size = new System.Drawing.Size(49, 15);
            this.label1.Text = "Options";
            // 
            // checkOverlapTaskbar
            // 
            this.checkOverlapTaskbar.AutoSize = true;
            this.checkOverlapTaskbar.Location = new System.Drawing.Point(3, 23);
            this.checkOverlapTaskbar.Margin = new System.Windows.Forms.Padding(3, 0, 0, 0);
            this.checkOverlapTaskbar.Size = new System.Drawing.Size(107, 17);
            this.checkOverlapTaskbar.Text = "Overlap taskbar";
            this.checkOverlapTaskbar.UseVisualStyleBackColor = true;
            this.checkOverlapTaskbar.CheckedChanged += new System.EventHandler(this.checkExcludeTaskbar_CheckedChanged);
            // 
            // checkFlipY
            // 
            this.checkFlipY.AutoSize = true;
            this.checkFlipY.Location = new System.Drawing.Point(3, 43);
            this.checkFlipY.Margin = new System.Windows.Forms.Padding(3, 3, 0, 0);
            this.checkFlipY.Size = new System.Drawing.Size(92, 17);
            this.checkFlipY.Text = "Flip vertically";
            this.checkFlipY.UseVisualStyleBackColor = true;
            this.checkFlipY.CheckedChanged += new System.EventHandler(this.checkFlipY_CheckedChanged);
            // 
            // checkFlipX
            // 
            this.checkFlipX.AutoSize = true;
            this.checkFlipX.Location = new System.Drawing.Point(3, 63);
            this.checkFlipX.Margin = new System.Windows.Forms.Padding(3, 3, 0, 0);
            this.checkFlipX.Size = new System.Drawing.Size(109, 17);
            this.checkFlipX.Text = "Flip horizontally";
            this.checkFlipX.UseVisualStyleBackColor = true;
            this.checkFlipX.CheckedChanged += new System.EventHandler(this.checkFlipX_CheckedChanged);
            // 
            // labelTemplate
            // 
            this.labelTemplate.AutoSize = true;
            this.labelTemplate.Location = new System.Drawing.Point(0, 88);
            this.labelTemplate.Margin = new System.Windows.Forms.Padding(0, 8, 0, 0);
            this.labelTemplate.Size = new System.Drawing.Size(29, 13);
            this.labelTemplate.Text = "Title";
            this.labelTemplate.Visible = false;
            // 
            // textTemplate
            // 
            this.textTemplate.Increment = 1;
            this.textTemplate.Location = new System.Drawing.Point(3, 107);
            this.textTemplate.Margin = new System.Windows.Forms.Padding(3, 6, 0, 0);
            this.textTemplate.Maximum = 100;
            this.textTemplate.Minimum = 0;
            this.textTemplate.ReverseMouseWheelDirection = false;
            this.textTemplate.Size = new System.Drawing.Size(69, 22);
            this.textTemplate.Text = "0";
            this.textTemplate.Value = 0;
            this.textTemplate.Visible = false;
            // 
            // panelOptions
            // 
            this.panelOptions.AutoSize = true;
            this.panelOptions.AutoSizeFill = Gw2Launcher.UI.Controls.StackPanel.AutoSizeFillMode.Width;
            this.panelOptions.Location = new System.Drawing.Point(0, 131);
            this.panelOptions.Margin = new System.Windows.Forms.Padding(0, 2, 0, 0);
            this.panelOptions.MinimumSize = new System.Drawing.Size(0, 10);
            this.panelOptions.Size = new System.Drawing.Size(157, 10);
            this.panelOptions.Visible = false;
            // 
            // panelCommands
            // 
            this.panelCommands.AutoSize = true;
            this.panelCommands.AutoSizeFill = Gw2Launcher.UI.Controls.StackPanel.AutoSizeFillMode.NoWrap;
            this.panelCommands.Controls.Add(this.labelCustomize);
            this.panelCommands.Controls.Add(this.labelCustomizeSep);
            this.panelCommands.Controls.Add(this.labelDelete);
            this.panelCommands.Controls.Add(this.labelReset);
            this.panelCommands.FlowDirection = System.Windows.Forms.FlowDirection.LeftToRight;
            this.panelCommands.Location = new System.Drawing.Point(0, 151);
            this.panelCommands.Margin = new System.Windows.Forms.Padding(0, 10, 0, 0);
            this.panelCommands.Size = new System.Drawing.Size(145, 13);
            // 
            // labelCustomize
            // 
            this.labelCustomize.AutoSize = true;
            this.labelCustomize.Location = new System.Drawing.Point(0, 0);
            this.labelCustomize.Margin = new System.Windows.Forms.Padding(0);
            this.labelCustomize.Size = new System.Drawing.Size(58, 13);
            this.labelCustomize.Text = "customize";
            this.labelCustomize.Click += new System.EventHandler(this.labelCustomize_Click);
            // 
            // labelCustomizeSep
            // 
            this.labelCustomizeSep.AutoSize = true;
            this.labelCustomizeSep.ForeColor = System.Drawing.SystemColors.GrayText;
            this.labelCustomizeSep.Location = new System.Drawing.Point(61, 0);
            this.labelCustomizeSep.Size = new System.Drawing.Size(10, 13);
            this.labelCustomizeSep.Text = "|";
            this.labelCustomizeSep.Visible = false;
            // 
            // labelDelete
            // 
            this.labelDelete.AutoSize = true;
            this.labelDelete.Location = new System.Drawing.Point(74, 0);
            this.labelDelete.Margin = new System.Windows.Forms.Padding(0);
            this.labelDelete.Size = new System.Drawing.Size(39, 13);
            this.labelDelete.Text = "delete";
            this.labelDelete.Visible = false;
            this.labelDelete.Click += new System.EventHandler(this.labelDelete_Click);
            // 
            // labelReset
            // 
            this.labelReset.AutoSize = true;
            this.labelReset.Location = new System.Drawing.Point(113, 0);
            this.labelReset.Margin = new System.Windows.Forms.Padding(0);
            this.labelReset.Size = new System.Drawing.Size(32, 13);
            this.labelReset.Text = "reset";
            this.labelReset.Visible = false;
            this.labelReset.Click += new System.EventHandler(this.labelReset_Click);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Font = new System.Drawing.Font("Segoe UI Semibold", 8.75F, System.Drawing.FontStyle.Bold);
            this.label2.Location = new System.Drawing.Point(0, 177);
            this.label2.Margin = new System.Windows.Forms.Padding(0, 13, 0, 8);
            this.label2.Size = new System.Drawing.Size(72, 15);
            this.label2.Text = "Display on...";
            // 
            // panelScreen
            // 
            this.panelScreen.AutoSize = true;
            this.panelScreen.AutoSizeFill = Gw2Launcher.UI.Controls.StackPanel.AutoSizeFillMode.Width;
            this.panelScreen.Controls.Add(this.checkScreen1);
            this.panelScreen.Location = new System.Drawing.Point(0, 200);
            this.panelScreen.Margin = new System.Windows.Forms.Padding(0);
            this.panelScreen.MinimumSize = new System.Drawing.Size(0, 10);
            this.panelScreen.Size = new System.Drawing.Size(157, 17);
            // 
            // checkScreen1
            // 
            this.checkScreen1.AutoCheck = false;
            this.checkScreen1.AutoSize = true;
            this.checkScreen1.Location = new System.Drawing.Point(3, 0);
            this.checkScreen1.Margin = new System.Windows.Forms.Padding(3, 0, 0, 0);
            this.checkScreen1.Size = new System.Drawing.Size(69, 17);
            this.checkScreen1.Text = "Screen 1";
            this.checkScreen1.UseVisualStyleBackColor = true;
            this.checkScreen1.Click += new System.EventHandler(this.checkScreen_Click);
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Font = new System.Drawing.Font("Segoe UI Semibold", 8.75F, System.Drawing.FontStyle.Bold);
            this.label5.Location = new System.Drawing.Point(0, 230);
            this.label5.Margin = new System.Windows.Forms.Padding(0, 13, 0, 1);
            this.label5.Size = new System.Drawing.Size(58, 15);
            this.label5.Text = "Snapping";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Font = new System.Drawing.Font("Segoe UI Semilight", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label4.ForeColor = System.Drawing.SystemColors.GrayText;
            this.label4.Location = new System.Drawing.Point(0, 246);
            this.label4.Margin = new System.Windows.Forms.Padding(0, 0, 0, 8);
            this.label4.Size = new System.Drawing.Size(111, 13);
            this.label4.Text = "Drop windows to snap";
            // 
            // checkSnapEdge
            // 
            this.checkSnapEdge.AutoSize = true;
            this.checkSnapEdge.Checked = true;
            this.checkSnapEdge.CheckState = System.Windows.Forms.CheckState.Checked;
            this.checkSnapEdge.Location = new System.Drawing.Point(3, 267);
            this.checkSnapEdge.Margin = new System.Windows.Forms.Padding(3, 0, 0, 0);
            this.checkSnapEdge.Size = new System.Drawing.Size(100, 17);
            this.checkSnapEdge.Text = "Snap to edges";
            this.checkSnapEdge.UseVisualStyleBackColor = true;
            this.checkSnapEdge.CheckedChanged += new System.EventHandler(this.checkSnapEdge_CheckedChanged);
            // 
            // checkSnapShift
            // 
            this.checkSnapShift.AutoSize = true;
            this.checkSnapShift.Checked = true;
            this.checkSnapShift.CheckState = System.Windows.Forms.CheckState.Checked;
            this.checkSnapShift.Location = new System.Drawing.Point(3, 287);
            this.checkSnapShift.Margin = new System.Windows.Forms.Padding(3, 3, 0, 0);
            this.checkSnapShift.Size = new System.Drawing.Size(118, 17);
            this.checkSnapShift.Text = "Hold Shift/Ctrl/Alt";
            this.checkSnapShift.UseVisualStyleBackColor = true;
            this.checkSnapShift.CheckedChanged += new System.EventHandler(this.checkSnapShift_CheckedChanged);
            // 
            // contextCustom
            // 
            this.contextCustom.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.editToolStripMenuItem,
            this.deleteToolStripMenuItem});
            this.contextCustom.Size = new System.Drawing.Size(108, 48);
            // 
            // editToolStripMenuItem
            // 
            this.editToolStripMenuItem.Size = new System.Drawing.Size(107, 22);
            this.editToolStripMenuItem.Text = "Edit";
            this.editToolStripMenuItem.Click += new System.EventHandler(this.editToolStripMenuItem_Click);
            // 
            // deleteToolStripMenuItem
            // 
            this.deleteToolStripMenuItem.Size = new System.Drawing.Size(107, 22);
            this.deleteToolStripMenuItem.Text = "Delete";
            this.deleteToolStripMenuItem.Click += new System.EventHandler(this.deleteToolStripMenuItem_Click);
            // 
            // formTemplates
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
            this.ClientSize = new System.Drawing.Size(573, 279);
            this.Controls.Add(this.autoScrollContainerPanel1);
            this.Controls.Add(this.panelTemplatesScroll);
            this.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.Icon = global::Gw2Launcher.Properties.Resources.Gw2Launcher;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.MinimumSize = new System.Drawing.Size(360, 200);
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.Manual;
            this.panelTemplatesScroll.ResumeLayout(false);
            this.panelTemplatesScroll.PerformLayout();
            this.panelTemplatesContainer.ResumeLayout(false);
            this.panelTemplatesContainer.PerformLayout();
            this.stackPanel1.ResumeLayout(false);
            this.stackPanel1.PerformLayout();
            this.autoScrollContainerPanel1.ResumeLayout(false);
            this.autoScrollContainerPanel1.PerformLayout();
            this.panelOptionsContainer.ResumeLayout(false);
            this.panelOptionsContainer.PerformLayout();
            this.panelCommands.ResumeLayout(false);
            this.panelCommands.PerformLayout();
            this.panelScreen.ResumeLayout(false);
            this.panelScreen.PerformLayout();
            this.contextCustom.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private Controls.AutoScrollContainerPanel panelTemplatesScroll;
        private Controls.StackPanel panelTemplatesContainer;
        private System.Windows.Forms.FlowLayoutPanel panelTemplates;
        private Controls.AutoScrollContainerPanel autoScrollContainerPanel1;
        private Controls.StackPanel panelOptionsContainer;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.CheckBox checkFlipY;
        private System.Windows.Forms.CheckBox checkFlipX;
        private Controls.StackPanel panelOptions;
        private Controls.LinkLabel labelCustomNew;
        private Controls.StackPanel panelCommands;
        private Controls.LinkLabel labelCustomize;
        private System.Windows.Forms.Label labelCustomizeSep;
        private Controls.LinkLabel labelReset;
        private System.Windows.Forms.Label labelTemplate;
        private Controls.IntegerTextBox textTemplate;
        private System.Windows.Forms.ContextMenuStrip contextCustom;
        private System.Windows.Forms.ToolStripMenuItem editToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem deleteToolStripMenuItem;
        private Controls.LinkLabel labelDelete;
        private System.Windows.Forms.CheckBox checkOverlapTaskbar;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.CheckBox checkScreen1;
        private Controls.StackPanel panelScreen;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.CheckBox checkSnapShift;
        private Controls.StackPanel stackPanel1;
        private System.Windows.Forms.Label label7;
        private Controls.LinkLabel labelCustomNewFromLayout;
        private System.Windows.Forms.CheckBox checkSnapEdge;
    }
}
