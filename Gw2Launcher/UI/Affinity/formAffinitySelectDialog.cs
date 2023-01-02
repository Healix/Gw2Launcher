using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Gw2Launcher.UI.Controls;
using Gw2Launcher.Windows.Native;

namespace Gw2Launcher.UI.Affinity
{
    public partial class formAffinitySelectDialog : Base.BaseForm
    {
        private class TransparentAffinityDisplay : AffinityDisplay
        {
            protected override void WndProc(ref Message m)
            {
                base.WndProc(ref m);

                if (m.Msg == (int)WindowMessages.WM_NCHITTEST)
                {
                    m.Result = (IntPtr)HitTest.Transparent;
                }
            }
        }

        private class TransparentLabel : Label
        {
            protected override void WndProc(ref Message m)
            {
                base.WndProc(ref m);

                if (m.Msg == (int)WindowMessages.WM_NCHITTEST)
                {
                    m.Result = (IntPtr)HitTest.Transparent;
                }
            }
        }

        public enum DialogMode
        {
            Save,
            Select,
            Update,
        }

        private int initialCount;
        private DialogMode mode;
        private Settings.AffinityValue existing;

        public formAffinitySelectDialog(DialogMode mode)
        {
            this.mode = mode;

            InitializeComponents();
        }

        public formAffinitySelectDialog(long affinity, string name = null)
            : this(DialogMode.Save)
        {
            if (name != null)
                textName.Text = name;
            adSave.Affinity = affinity;
        }

        private formAffinitySelectDialog(Settings.AffinityValue affinity)
            : this(DialogMode.Update)
        {
            existing = affinity;

            textName.Text = affinity.Name;
            adSave.Affinity = affinity.Affinity;
        }

        protected override void OnInitializeComponents()
        {
            InitializeComponent();

            contextMenu.Closed += contextMenu_Closed;

            if (mode != DialogMode.Update)
            {
                initialCount = panelExisting.Controls.Count;

                for (int i = 0, count = Settings.AffinityValues.Count; i < count; i++)
                {
                    var v = Settings.AffinityValues[i];

                    var container = new StackPanel()
                    {
                        Margin = panelTemplate.Margin,
                        Padding = panelTemplate.Padding,
                        FlowDirection = FlowDirection.TopDown,
                        AutoSize = true,
                        Tag = v,
                        Anchor = AnchorStyles.Left | AnchorStyles.Top | AnchorStyles.Right,
                    };

                    var label = new TransparentLabel()
                    {
                        Margin = labelTemplate.Margin,
                        Text = v.Name,
                        Font = labelTemplate.Font,
                        AutoSize = true,
                    };

                    var display = new TransparentAffinityDisplay()
                    {
                        Margin = adTemplate.Margin,
                        Affinity = v.Affinity,
                        ReadOnly = true,
                        AutoSize = true,
                    };

                    container.MouseEnter += container_MouseEnter;
                    container.MouseLeave += container_MouseLeave;
                    container.MouseDown += container_MouseDown;

                    container.Controls.AddRange(new Control[] { label, display });
                    panelExisting.Controls.Add(container);
                }

                OnAffinityControlsChanged();

                panelSave.Visible = mode == DialogMode.Save;
            }
            else
            {
                panelExisting.Visible = false;
            }
        }

        protected override void OnHandleCreated(EventArgs e)
        {
            base.OnHandleCreated(e);

            if (mode == DialogMode.Update)
            {
                var sz = panelSave.GetPreferredSize(new Size(panelSave.Width, int.MaxValue), true);
                this.MinimumSize = new Size(this.MinimumSize.Width, this.Height + sz.Height - panelScroll.Parent.Height);
                this.Height = this.MinimumSize.Height;
            }
        }

        protected override void OnShown(EventArgs e)
        {
            base.OnShown(e);

            if (mode == DialogMode.Save)
            {
                textName.SelectAll();
                textName.Focus();
            }
        }

        void contextMenu_Closed(object sender, ToolStripDropDownClosedEventArgs e)
        {
            Selected = null;
        }

        private void OnAffinityControlsChanged()
        {
            labelNone.Visible = panelExisting.Controls.Count == initialCount;
        }

        private Control _Selected;
        private Control Selected
        {
            get
            {
                return _Selected;
            }
            set
            {
                if (_Selected != value)
                {
                    if (_Selected != null && _Selected != _Hovered && !_Selected.IsDisposed)
                    {
                        _Selected.BackColor = Color.Empty;
                    }
                    if (value != null)
                    {
                        value.BackColor = Color.FromArgb(217, 222, 228);
                    }
                    _Selected = value;
                }
            }
        }

        private Control _Hovered;
        private Control Hovered
        {
            get
            {
                return _Hovered;
            }
            set
            {
                if (_Hovered != value)
                {
                    if (_Hovered != null && _Hovered != _Selected && !_Hovered.IsDisposed)
                    {
                        _Hovered.BackColor = Color.Empty;
                    }
                    if (value != null && value != _Selected)
                    {
                        value.BackColor = Color.FromArgb(225, 225, 225);
                    }
                    _Hovered = value;
                }
            }
        }

        void container_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == System.Windows.Forms.MouseButtons.Right)
            {
                Selected = (Control)sender;
                contextMenu.Tag = sender;
                contextMenu.Show(Cursor.Position);
            }
            else if (e.Button == System.Windows.Forms.MouseButtons.Left && mode == DialogMode.Select)
            {
                Selected = (Control)sender;
            }
        }

        void container_MouseLeave(object sender, EventArgs e)
        {
            if (_Hovered == sender)
                Hovered = null;
        }

        void container_MouseEnter(object sender, EventArgs e)
        {
            Hovered = (Control)sender;
        }

        private void deleteToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var c = (Control)contextMenu.Tag;
            if (c != null)
            {
                Settings.AffinityValues.Remove((Settings.AffinityValue)c.Tag);
                panelExisting.Controls.Remove(c);
                c.Dispose();
                Selected = null;
                OnAffinityControlsChanged();
            }
        }

        public Settings.AffinityValue SelectedAffinity
        {
            get;
            private set;
        }

        private void buttonOK_Click(object sender, EventArgs e)
        {
            if (mode == DialogMode.Update)
            {
                if (string.IsNullOrWhiteSpace(textName.Text))
                {
                    textName.SelectAll();
                    textName.Focus();
                    return;
                }

                if (adSave.Affinity != existing.Affinity || textName.Text != existing.Name)
                {
                    SelectedAffinity = new Settings.AffinityValue(textName.Text, adSave.Affinity);
                    Settings.AffinityValues.ReplaceOrAdd(existing, SelectedAffinity);
                }
            }
            else if (mode == DialogMode.Save)
            {
                if (string.IsNullOrWhiteSpace(textName.Text))
                {
                    textName.SelectAll();
                    textName.Focus();
                    return;
                }

                var affinity = adSave.Affinity;
                var add = true;

                foreach (Control c in panelExisting.Controls)
                {
                    if (c.Tag == null)
                        continue;

                    var v = (Settings.AffinityValue)c.Tag;

                    if (v.Affinity == affinity)
                    {
                        ((Panel)panelExisting.Parent).ScrollControlIntoView(c);

                        switch (MessageBox.Show(this, "The affinity has already been saved as\n\n\"" + v.Name + "\"\n\nDo you want to rename it?", "Already exists", MessageBoxButtons.YesNoCancel, MessageBoxIcon.Exclamation, MessageBoxDefaultButton.Button3))
                        {
                            case System.Windows.Forms.DialogResult.No:
                                break;
                            case System.Windows.Forms.DialogResult.Yes:

                                Settings.AffinityValues.ReplaceOrAdd(v, new Settings.AffinityValue(textName.Text, adSave.Affinity));
                                add = false;

                                break;
                            default:
                                return;
                        }

                        break;
                    }
                }

                if (add)
                    Settings.AffinityValues.Add(new Settings.AffinityValue(textName.Text, adSave.Affinity));
            }
            else
            {
                if (_Selected == null)
                    return;
                SelectedAffinity = (Settings.AffinityValue)_Selected.Tag;
            }

            this.DialogResult = System.Windows.Forms.DialogResult.OK;
        }

        private void editToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var c = (Control)contextMenu.Tag;
            if (c != null)
            {
                var v = (Settings.AffinityValue)c.Tag;

                using (var f = new formAffinitySelectDialog(v))
                {
                    if (f.ShowDialog(this) == System.Windows.Forms.DialogResult.OK)
                    {
                        c.Tag = f.SelectedAffinity;
                        c.Controls[0].Text = f.SelectedAffinity.Name;
                        ((TransparentAffinityDisplay)c.Controls[1]).Affinity = f.SelectedAffinity.Affinity;
                    }
                }
            }
        }

        private void buttonCancel_Click(object sender, EventArgs e)
        {
            this.DialogResult = System.Windows.Forms.DialogResult.Cancel;
        }
    }
}
