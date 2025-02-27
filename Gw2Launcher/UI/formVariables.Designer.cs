namespace Gw2Launcher.UI
{
    partial class formVariables
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
            this.panelVariablesContainer = new System.Windows.Forms.Panel();
            this.panelVariables = new Gw2Launcher.UI.Controls.StackPanel();
            this.labelTemplate = new System.Windows.Forms.Label();
            this.scrollV = new Gw2Launcher.UI.Controls.FlatVScrollBar();
            this.panelVariablesContainer.SuspendLayout();
            this.panelVariables.SuspendLayout();
            this.SuspendLayout();
            // 
            // panelVariablesContainer
            // 
            this.panelVariablesContainer.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.panelVariablesContainer.Controls.Add(this.panelVariables);
            this.panelVariablesContainer.Controls.Add(this.scrollV);
            this.panelVariablesContainer.Location = new System.Drawing.Point(5, 5);
            this.panelVariablesContainer.Margin = new System.Windows.Forms.Padding(5);
            this.panelVariablesContainer.Size = new System.Drawing.Size(121, 90);
            // 
            // panelVariables
            // 
            this.panelVariables.AutoSize = true;
            this.panelVariables.AutoSizeFill = Gw2Launcher.UI.Controls.StackPanel.AutoSizeFillMode.NoWrap;
            this.panelVariables.Controls.Add(this.labelTemplate);
            this.panelVariables.Location = new System.Drawing.Point(0, 0);
            this.panelVariables.Margin = new System.Windows.Forms.Padding(0);
            this.panelVariables.Size = new System.Drawing.Size(76, 19);
            // 
            // labelTemplate
            // 
            this.labelTemplate.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.labelTemplate.AutoSize = true;
            this.labelTemplate.Cursor = Gw2Launcher.Windows.Cursors.Hand;
            this.labelTemplate.Font = new System.Drawing.Font("Segoe UI Semibold", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.labelTemplate.Location = new System.Drawing.Point(0, 0);
            this.labelTemplate.Margin = new System.Windows.Forms.Padding(0);
            this.labelTemplate.Padding = new System.Windows.Forms.Padding(0, 3, 5, 3);
            this.labelTemplate.Size = new System.Drawing.Size(76, 19);
            this.labelTemplate.Text = "%template%";
            this.labelTemplate.Visible = false;
            // 
            // scrollV
            // 
            this.scrollV.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.scrollV.Location = new System.Drawing.Point(113, 0);
            this.scrollV.Margin = new System.Windows.Forms.Padding(1, 0, 0, 0);
            this.scrollV.Maximum = 100;
            this.scrollV.ScrollChange = 0;
            this.scrollV.Size = new System.Drawing.Size(8, 90);
            this.scrollV.Value = 0;
            this.scrollV.Visible = false;
            // 
            // formVariables
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
            this.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.ClientSize = new System.Drawing.Size(131, 100);
            this.Controls.Add(this.panelVariablesContainer);
            this.ForeColor = System.Drawing.Color.Black;
            this.StartPosition = System.Windows.Forms.FormStartPosition.Manual;
            this.panelVariablesContainer.ResumeLayout(false);
            this.panelVariablesContainer.PerformLayout();
            this.panelVariables.ResumeLayout(false);
            this.panelVariables.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Panel panelVariablesContainer;
        private Controls.StackPanel panelVariables;
        private System.Windows.Forms.Label labelTemplate;
        private Controls.FlatVScrollBar scrollV;


    }
}
