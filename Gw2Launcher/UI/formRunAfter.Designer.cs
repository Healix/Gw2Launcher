namespace Gw2Launcher.UI
{
    partial class formRunAfter
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
            this.panelContainer = new Gw2Launcher.UI.Controls.StackPanel();
            this.stackPanel5 = new Gw2Launcher.UI.Controls.StackPanel();
            this.radioProgram = new System.Windows.Forms.RadioButton();
            this.radioCommands = new System.Windows.Forms.RadioButton();
            this.label9 = new System.Windows.Forms.Label();
            this.textName = new System.Windows.Forms.TextBox();
            this.panelProgram = new Gw2Launcher.UI.Controls.StackPanel();
            this.label5 = new System.Windows.Forms.Label();
            this.stackPanel3 = new Gw2Launcher.UI.Controls.StackPanel();
            this.textPath = new System.Windows.Forms.TextBox();
            this.buttonPath = new System.Windows.Forms.Button();
            this.label3 = new System.Windows.Forms.Label();
            this.textArguments = new System.Windows.Forms.TextBox();
            this.panelCommands = new Gw2Launcher.UI.Controls.StackPanel();
            this.label11 = new System.Windows.Forms.Label();
            this.textCommands = new System.Windows.Forms.TextBox();
            this.labelArgumentsVariables = new Gw2Launcher.UI.Controls.RoundedLabelButton();
            this.label7 = new System.Windows.Forms.Label();
            this.comboRunAfter = new System.Windows.Forms.ComboBox();
            this.checkUseCurrentUser = new System.Windows.Forms.CheckBox();
            this.panelOnExit = new Gw2Launcher.UI.Controls.StackPanel();
            this.label8 = new System.Windows.Forms.Label();
            this.comboOnExit = new System.Windows.Forms.ComboBox();
            this.stackPanel2 = new Gw2Launcher.UI.Controls.StackPanel();
            this.buttonCancel = new System.Windows.Forms.Button();
            this.buttonOk = new System.Windows.Forms.Button();
            this.panelContainer.SuspendLayout();
            this.stackPanel5.SuspendLayout();
            this.panelProgram.SuspendLayout();
            this.stackPanel3.SuspendLayout();
            this.panelCommands.SuspendLayout();
            this.panelOnExit.SuspendLayout();
            this.stackPanel2.SuspendLayout();
            this.SuspendLayout();
            // 
            // panelContainer
            // 
            this.panelContainer.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
            | System.Windows.Forms.AnchorStyles.Left)
            | System.Windows.Forms.AnchorStyles.Right)));
            this.panelContainer.Controls.Add(this.stackPanel5);
            this.panelContainer.Controls.Add(this.label9);
            this.panelContainer.Controls.Add(this.textName);
            this.panelContainer.Controls.Add(this.panelProgram);
            this.panelContainer.Controls.Add(this.panelCommands);
            this.panelContainer.Controls.Add(this.labelArgumentsVariables);
            this.panelContainer.Controls.Add(this.label7);
            this.panelContainer.Controls.Add(this.comboRunAfter);
            this.panelContainer.Controls.Add(this.checkUseCurrentUser);
            this.panelContainer.Controls.Add(this.panelOnExit);
            this.panelContainer.Controls.Add(this.stackPanel2);
            this.panelContainer.Location = new System.Drawing.Point(15, 15);
            this.panelContainer.Margin = new System.Windows.Forms.Padding(15);
            this.panelContainer.Size = new System.Drawing.Size(299, 373);
            // 
            // stackPanel5
            // 
            this.stackPanel5.AutoSize = true;
            this.stackPanel5.Controls.Add(this.radioProgram);
            this.stackPanel5.Controls.Add(this.radioCommands);
            this.stackPanel5.FlowDirection = System.Windows.Forms.FlowDirection.LeftToRight;
            this.stackPanel5.Location = new System.Drawing.Point(0, 0);
            this.stackPanel5.Margin = new System.Windows.Forms.Padding(0, 0, 0, 10);
            this.stackPanel5.Size = new System.Drawing.Size(189, 17);
            // 
            // radioProgram
            // 
            this.radioProgram.AutoSize = true;
            this.radioProgram.Checked = true;
            this.radioProgram.Location = new System.Drawing.Point(3, 0);
            this.radioProgram.Margin = new System.Windows.Forms.Padding(3, 0, 0, 0);
            this.radioProgram.Size = new System.Drawing.Size(68, 17);
            this.radioProgram.TabStop = true;
            this.radioProgram.Text = "Program";
            this.radioProgram.UseVisualStyleBackColor = true;
            this.radioProgram.CheckedChanged += new System.EventHandler(this.radioTab_CheckedChanged);
            // 
            // radioCommands
            // 
            this.radioCommands.AutoSize = true;
            this.radioCommands.Location = new System.Drawing.Point(81, 0);
            this.radioCommands.Margin = new System.Windows.Forms.Padding(10, 0, 0, 0);
            this.radioCommands.Size = new System.Drawing.Size(108, 17);
            this.radioCommands.Text = "Shell commands";
            this.radioCommands.UseVisualStyleBackColor = true;
            this.radioCommands.CheckedChanged += new System.EventHandler(this.radioTab_CheckedChanged);
            // 
            // label9
            // 
            this.label9.AutoSize = true;
            this.label9.Font = new System.Drawing.Font("Segoe UI Semilight", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label9.Location = new System.Drawing.Point(1, 27);
            this.label9.Margin = new System.Windows.Forms.Padding(1, 0, 0, 8);
            this.label9.Size = new System.Drawing.Size(122, 13);
            this.label9.Text = "Optional name to display";
            // 
            // textName
            // 
            this.textName.Font = new System.Drawing.Font("Segoe UI", 8.25F);
            this.textName.Location = new System.Drawing.Point(8, 48);
            this.textName.Margin = new System.Windows.Forms.Padding(8, 0, 8, 0);
            this.textName.Size = new System.Drawing.Size(234, 22);
            // 
            // panelProgram
            // 
            this.panelProgram.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
            | System.Windows.Forms.AnchorStyles.Right)));
            this.panelProgram.AutoSize = true;
            this.panelProgram.Controls.Add(this.label5);
            this.panelProgram.Controls.Add(this.stackPanel3);
            this.panelProgram.Controls.Add(this.label3);
            this.panelProgram.Controls.Add(this.textArguments);
            this.panelProgram.Location = new System.Drawing.Point(0, 70);
            this.panelProgram.Margin = new System.Windows.Forms.Padding(0);
            this.panelProgram.Size = new System.Drawing.Size(299, 106);
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Font = new System.Drawing.Font("Segoe UI Semilight", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label5.Location = new System.Drawing.Point(1, 8);
            this.label5.Margin = new System.Windows.Forms.Padding(1, 8, 0, 8);
            this.label5.Size = new System.Drawing.Size(100, 13);
            this.label5.Text = "The program to run";
            // 
            // stackPanel3
            // 
            this.stackPanel3.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
            | System.Windows.Forms.AnchorStyles.Right)));
            this.stackPanel3.AutoSize = true;
            this.stackPanel3.AutoSizeFill = Gw2Launcher.UI.Controls.StackPanel.AutoSizeFillMode.NoWrap;
            this.stackPanel3.Controls.Add(this.textPath);
            this.stackPanel3.Controls.Add(this.buttonPath);
            this.stackPanel3.FlowDirection = System.Windows.Forms.FlowDirection.LeftToRight;
            this.stackPanel3.Location = new System.Drawing.Point(0, 29);
            this.stackPanel3.Margin = new System.Windows.Forms.Padding(0);
            this.stackPanel3.Size = new System.Drawing.Size(299, 24);
            // 
            // textPath
            // 
            this.textPath.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
            | System.Windows.Forms.AnchorStyles.Right)));
            this.textPath.Font = new System.Drawing.Font("Segoe UI", 8.25F);
            this.textPath.Location = new System.Drawing.Point(8, 1);
            this.textPath.Margin = new System.Windows.Forms.Padding(8, 1, 0, 1);
            this.textPath.Size = new System.Drawing.Size(234, 22);
            // 
            // buttonPath
            // 
            this.buttonPath.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)));
            this.buttonPath.Font = new System.Drawing.Font("Segoe UI", 8.25F);
            this.buttonPath.Location = new System.Drawing.Point(248, 0);
            this.buttonPath.Margin = new System.Windows.Forms.Padding(6, 0, 8, 0);
            this.buttonPath.Size = new System.Drawing.Size(43, 24);
            this.buttonPath.Text = "...";
            this.buttonPath.UseVisualStyleBackColor = true;
            this.buttonPath.Click += new System.EventHandler(this.buttonPath_Click);
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Font = new System.Drawing.Font("Segoe UI Semilight", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label3.Location = new System.Drawing.Point(1, 61);
            this.label3.Margin = new System.Windows.Forms.Padding(1, 8, 0, 8);
            this.label3.Size = new System.Drawing.Size(168, 13);
            this.label3.Text = "Optional command line arguments";
            // 
            // textArguments
            // 
            this.textArguments.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
            | System.Windows.Forms.AnchorStyles.Right)));
            this.textArguments.Font = new System.Drawing.Font("Segoe UI", 8.25F);
            this.textArguments.Location = new System.Drawing.Point(8, 83);
            this.textArguments.Margin = new System.Windows.Forms.Padding(8, 1, 8, 1);
            this.textArguments.Size = new System.Drawing.Size(283, 22);
            // 
            // panelCommands
            // 
            this.panelCommands.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
            | System.Windows.Forms.AnchorStyles.Left)
            | System.Windows.Forms.AnchorStyles.Right)));
            this.panelCommands.Controls.Add(this.label11);
            this.panelCommands.Controls.Add(this.textCommands);
            this.panelCommands.Location = new System.Drawing.Point(0, 176);
            this.panelCommands.Margin = new System.Windows.Forms.Padding(0);
            this.panelCommands.Size = new System.Drawing.Size(299, 0);
            this.panelCommands.Visible = false;
            // 
            // label11
            // 
            this.label11.AutoSize = true;
            this.label11.Font = new System.Drawing.Font("Segoe UI Semilight", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label11.Location = new System.Drawing.Point(1, 8);
            this.label11.Margin = new System.Windows.Forms.Padding(1, 8, 0, 8);
            this.label11.Size = new System.Drawing.Size(91, 13);
            this.label11.Text = "Commands to run";
            // 
            // textCommands
            // 
            this.textCommands.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
            | System.Windows.Forms.AnchorStyles.Left)
            | System.Windows.Forms.AnchorStyles.Right)));
            this.textCommands.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.textCommands.Location = new System.Drawing.Point(8, 29);
            this.textCommands.Margin = new System.Windows.Forms.Padding(8, 0, 8, 0);
            this.textCommands.Multiline = true;
            this.textCommands.ScrollBars = System.Windows.Forms.ScrollBars.Both;
            this.textCommands.Size = new System.Drawing.Size(283, 0);
            this.textCommands.WordWrap = false;
            // 
            // labelArgumentsVariables
            // 
            this.labelArgumentsVariables.AutoSize = true;
            this.labelArgumentsVariables.BorderSize = ((byte)(1));
            this.labelArgumentsVariables.Cursor = System.Windows.Forms.Cursors.Hand;
            this.labelArgumentsVariables.Font = new System.Drawing.Font("Calibri", 8.25F);
            this.labelArgumentsVariables.Location = new System.Drawing.Point(8, 181);
            this.labelArgumentsVariables.Margin = new System.Windows.Forms.Padding(8, 5, 0, 0);
            this.labelArgumentsVariables.Padding = new System.Windows.Forms.Padding(10, 2, 10, 2);
            this.labelArgumentsVariables.Size = new System.Drawing.Size(71, 17);
            this.labelArgumentsVariables.Text = "variables";
            this.labelArgumentsVariables.Click += new System.EventHandler(this.labelArgumentsVariables_Click);
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Font = new System.Drawing.Font("Segoe UI Semilight", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label7.Location = new System.Drawing.Point(1, 208);
            this.label7.Margin = new System.Windows.Forms.Padding(1, 10, 0, 6);
            this.label7.Size = new System.Drawing.Size(55, 13);
            this.label7.Text = "Run after...";
            // 
            // comboRunAfter
            // 
            this.comboRunAfter.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboRunAfter.FormattingEnabled = true;
            this.comboRunAfter.Location = new System.Drawing.Point(8, 227);
            this.comboRunAfter.Margin = new System.Windows.Forms.Padding(8, 0, 0, 0);
            this.comboRunAfter.Size = new System.Drawing.Size(183, 21);
            // 
            // checkUseCurrentUser
            // 
            this.checkUseCurrentUser.AutoSize = true;
            this.checkUseCurrentUser.Location = new System.Drawing.Point(8, 254);
            this.checkUseCurrentUser.Margin = new System.Windows.Forms.Padding(8, 6, 0, 0);
            this.checkUseCurrentUser.Size = new System.Drawing.Size(201, 17);
            this.checkUseCurrentUser.Text = "Run on the current Windows user";
            this.checkUseCurrentUser.UseVisualStyleBackColor = true;
            // 
            // panelOnExit
            // 
            this.panelOnExit.AutoSize = true;
            this.panelOnExit.Controls.Add(this.label8);
            this.panelOnExit.Controls.Add(this.comboOnExit);
            this.panelOnExit.Location = new System.Drawing.Point(0, 271);
            this.panelOnExit.Margin = new System.Windows.Forms.Padding(0);
            this.panelOnExit.Size = new System.Drawing.Size(191, 48);
            // 
            // label8
            // 
            this.label8.AutoSize = true;
            this.label8.Font = new System.Drawing.Font("Segoe UI Semilight", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label8.Location = new System.Drawing.Point(1, 8);
            this.label8.Margin = new System.Windows.Forms.Padding(1, 8, 0, 6);
            this.label8.Size = new System.Drawing.Size(112, 13);
            this.label8.Text = "When the game exits...";
            // 
            // comboOnExit
            // 
            this.comboOnExit.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboOnExit.FormattingEnabled = true;
            this.comboOnExit.Location = new System.Drawing.Point(8, 27);
            this.comboOnExit.Margin = new System.Windows.Forms.Padding(8, 0, 0, 0);
            this.comboOnExit.Size = new System.Drawing.Size(183, 21);
            // 
            // stackPanel2
            // 
            this.stackPanel2.Anchor = System.Windows.Forms.AnchorStyles.Bottom;
            this.stackPanel2.AutoSize = true;
            this.stackPanel2.Controls.Add(this.buttonCancel);
            this.stackPanel2.Controls.Add(this.buttonOk);
            this.stackPanel2.FlowDirection = System.Windows.Forms.FlowDirection.LeftToRight;
            this.stackPanel2.Location = new System.Drawing.Point(58, 338);
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
            // formRunAfter
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
            this.ClientSize = new System.Drawing.Size(329, 403);
            this.Controls.Add(this.panelContainer);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.MinimumSize = new System.Drawing.Size(345, 442);
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.panelContainer.ResumeLayout(false);
            this.panelContainer.PerformLayout();
            this.stackPanel5.ResumeLayout(false);
            this.stackPanel5.PerformLayout();
            this.panelProgram.ResumeLayout(false);
            this.panelProgram.PerformLayout();
            this.stackPanel3.ResumeLayout(false);
            this.stackPanel3.PerformLayout();
            this.panelCommands.ResumeLayout(false);
            this.panelCommands.PerformLayout();
            this.panelOnExit.ResumeLayout(false);
            this.panelOnExit.PerformLayout();
            this.stackPanel2.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private Controls.StackPanel panelContainer;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.TextBox textPath;
        private Controls.StackPanel stackPanel2;
        private System.Windows.Forms.Button buttonCancel;
        private System.Windows.Forms.Button buttonOk;
        private Controls.StackPanel stackPanel3;
        private System.Windows.Forms.Button buttonPath;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.TextBox textArguments;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.ComboBox comboRunAfter;
        private System.Windows.Forms.Label label8;
        private System.Windows.Forms.ComboBox comboOnExit;
        private Controls.RoundedLabelButton labelArgumentsVariables;
        private System.Windows.Forms.CheckBox checkUseCurrentUser;
        private Controls.StackPanel stackPanel5;
        private System.Windows.Forms.RadioButton radioProgram;
        private System.Windows.Forms.RadioButton radioCommands;
        private Controls.StackPanel panelProgram;
        private System.Windows.Forms.TextBox textCommands;
        private Controls.StackPanel panelOnExit;
        private Controls.StackPanel panelCommands;
        private System.Windows.Forms.Label label11;
        private System.Windows.Forms.Label label9;
        private System.Windows.Forms.TextBox textName;
    }
}
