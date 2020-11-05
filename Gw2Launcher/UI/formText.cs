using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Gw2Launcher.UI
{
    public partial class formText : Base.BaseForm
    {
        protected TextBox textSource;

        private formText()
        {
            InitializeComponents();
        }

        private formText(string text, FormStartPosition sp)
            : this()
        {
            textText.Text = text;
            this.StartPosition = sp;
        }

        private formText(string text, IEnumerable<string[]> variables, FormStartPosition sp)
            : this(text, sp)
        {
            if (variables != null)
            {
                SetVariables(variables);
            }
            else
            {
                buttonVariables.Visible = false;
            }
        }

        protected override void OnInitializeComponents()
        {
            base.OnInitializeComponents();

            InitializeComponent();
        }

        void panelVariablesContainer_VisibleChanged(object sender, EventArgs e)
        {
            if (panelVariablesContainer.Visible)
            {
                textText.MouseDown += textText_MouseDown;
            }
            else
            {
                textText.MouseDown -= textText_MouseDown;
            }
        }

        void variable_Click(object sender, EventArgs e)
        {
            textText.SelectedText = ((Label)sender).Text;
        }

        void description_Click(object sender, EventArgs e)
        {
            variable_Click(((Label)sender).Tag, e);
        }

        /// <param name="text">Initial value</param>
        /// <param name="variables">Optional variable names and descriptions</param>
        public formText(string text, IEnumerable<string[]> variables = null)
            : this(text, variables, FormStartPosition.CenterParent)
        {
        }

        /// <param name="text">Initial value</param>
        /// <param name="variables">Optional variables</param>
        public formText(string text, IEnumerable<Client.Variables.Variable> variables)
            : this(text, GetVariables(variables))
        {
        }

        /// <param name="source">Source textbox that will be copied from and saved to</param>
        /// <param name="variables">Optional variable names and descriptions, or null to disable</param>
        public formText(TextBox source, IEnumerable<string[]> variables = null)
            : this(source.Text, variables, FormStartPosition.Manual)
        {
            textSource = source;
            textText.Select(source.SelectionStart, source.SelectionLength);

            var p = textSource.PointToScreen(Point.Empty);
            var screen = Screen.FromPoint(p).WorkingArea;

            var x = p.X + source.Width / 2 - this.Width / 2;
            var y = p.Y + source.Height / 2 - this.Height / 2;

            this.Bounds = Util.RectangleConstraint.ConstrainToScreen(new Point(x, y), this.Size);
        }
        
        /// <param name="source">Source textbox that will be copied from and saved to</param>
        /// <param name="variables">Optional variables</param>
        public formText(TextBox source, IEnumerable<Client.Variables.Variable> variables)
            : this(source, GetVariables(variables))
        {
        }

        private static IEnumerable<string[]> GetVariables(IEnumerable<Client.Variables.Variable> variables)
        {
            if (variables != null)
            {
                foreach (var v in variables)
                {
                    yield return new string[] { v.Name, v.Description };
                }
            }
        }

        private void SetVariables(IEnumerable<string[]> variables)
        {
            panelVariablesContainer.VisibleChanged += panelVariablesContainer_VisibleChanged;

            panelVariablesContainer.SuspendLayout();

            int w = 0;

            foreach (var v in variables)
            {
                Label l;

                l = new Label()
                {
                    Font = labelVariableTemplate.Font,
                    Margin = labelVariableTemplate.Margin,
                    Padding = labelVariableTemplate.Padding,
                    Anchor = labelVariableTemplate.Anchor,
                    ForeColor = labelVariableTemplate.ForeColor,
                    Text = v[0],
                    Cursor = labelVariableTemplate.Cursor,
                    AutoSize = true,
                };

                l.Click += variable_Click;
                l.MouseEnter += variable_MouseEnter;
                l.MouseLeave += variable_MouseLeave;

                panelVariables.Controls.Add(l);

                if (w < panelVariables.MaximumSize.Width)
                {
                    var s = l.GetPreferredSize(Size.Empty);
                    if (s.Width > w)
                        w = s.Width;
                }

                if (v.Length > 1 && !string.IsNullOrEmpty(v[1]))
                {
                    var ld = new Label()
                    {
                        Font = labelVariableDescriptionTemplate.Font,
                        Margin = labelVariableDescriptionTemplate.Margin,
                        Padding = labelVariableDescriptionTemplate.Padding,
                        Anchor = labelVariableDescriptionTemplate.Anchor,
                        ForeColor = labelVariableDescriptionTemplate.ForeColor,
                        Text = v[1],
                        Cursor = labelVariableTemplate.Cursor,
                        AutoSize = true,
                        Tag = l,
                    };

                    if (w < panelVariables.MaximumSize.Width)
                    {
                        var s = ld.GetPreferredSize(Size.Empty);
                        if (s.Width > w)
                            w = s.Width;
                    }

                    ld.Click += description_Click;
                    ld.MouseEnter += variable_MouseEnter;
                    ld.MouseLeave += variable_MouseLeave;

                    l.Tag = ld;

                    panelVariables.Controls.Add(ld);
                }
            }

            panelVariablesContainer.Width = w + panelVariables.Margin.Horizontal + panelVariables.Padding.Horizontal + panelVariables.Parent.Margin.Horizontal + SystemInformation.VerticalScrollBarWidth;

            panelVariablesContainer.ResumeLayout();
        }

        void variable_MouseLeave(object sender, EventArgs e)
        {
            var l = (Label)sender;
            var c = panelVariables.BackColor;
            
            l.BackColor = c;

            if (l.Tag != null)
            {
                l = (Label)l.Tag;
                l.BackColor = c;
            }
        }

        void variable_MouseEnter(object sender, EventArgs e)
        {
            var l = (Label)sender;
            var c = Util.Color.Darken(panelVariables.BackColor, 0.05f);

            l.BackColor = c;

            if (l.Tag != null)
            {
                l = (Label)l.Tag;
                l.BackColor = c;
            }
        }

        void textText_MouseDown(object sender, MouseEventArgs e)
        {
            panelVariablesContainer.Visible = false;
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            base.OnFormClosing(e);

            if (textSource != null)
            {
                textSource.Text = textText.Text;
                textSource.Select(textText.SelectionStart, textText.SelectionLength);
            }
        }

        public TextBox TextBox
        {
            get
            {
                return textText;
            }
        }

        private void buttonVariables_Click(object sender, EventArgs e)
        {
            panelVariablesContainer.BringToFront();
            panelVariablesContainer.Visible = !panelVariablesContainer.Visible;
        }

        private void panelVariables_SizeChanged(object sender, EventArgs e)
        {
            //panelVariablesContainer.Width = panelVariables.Width + panelVariables.Margin.Horizontal + panelVariables.Parent.Margin.Horizontal + SystemInformation.VerticalScrollBarWidth;
            //panelVariables.Anchor |= AnchorStyles.Right;
        }

        void panelVariables_VisibleChanged(object sender, EventArgs e)
        {
            //if (!panelVariables.Visible)
            //    return;

            //panelVariables.VisibleChanged -= panelVariables_VisibleChanged;

            //panelVariables.SuspendLayout();

            //panelVariables.AutoSizeFill = UI.Controls.StackPanel.AutoSizeFillMode.NoWrap;
            //var s = panelVariables.GetPreferredSize(new Size(panelVariables.MaximumSize.Width, int.MaxValue));

            //panelVariables.Anchor |= AnchorStyles.Right;

            //panelVariablesContainer.Width = s.Width + panelVariables.Margin.Horizontal + panelVariables.Padding.Horizontal + panelVariables.Parent.Margin.Horizontal + SystemInformation.VerticalScrollBarWidth;

            //panelVariables.ResumeLayout();
        }
    }
}
