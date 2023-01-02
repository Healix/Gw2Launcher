using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Gw2Launcher.Windows.Native;

namespace Gw2Launcher.UI.WindowPositioning
{
    public partial class formTemplatesCompact : Base.BaseForm
    {
        private class BaseTemplate
        {
            public BaseTemplate(Settings.WindowTemplate source)
            {
                this.Source = source;
                this.Assigned = new List<TemplateDisplay>();
            }

            public Settings.WindowTemplate Source
            {
                get;
                set;
            }

            public List<TemplateDisplay> Assigned
            {
                get;
                set;
            }

            public int Index
            {
                get;
                set;
            }

            public int NextChildIndex
            {
                get;
                set;
            }
        }

        private class BarButton : UI.Controls.FlatButton
        {
            private bool transparent;

            public bool Transparent
            {
                get
                {
                    return transparent;
                }
                set
                {
                    transparent = value;
                }
            }

            protected override void WndProc(ref Message m)
            {
                base.WndProc(ref m);

                switch ((Windows.Native.WindowMessages)m.Msg)
                {
                    case Windows.Native.WindowMessages.WM_NCHITTEST:

                        if (transparent)
                            m.Result = (IntPtr)HitTest.Transparent;

                        break;
                }
            }
        }

        private class TemplateDisplayIndexed : TemplateDisplay
        {
            public BaseTemplate ParentTemplate
            {
                get;
                set;
            }

            public int Index
            {
                get;
                set;
            }
        }

        private Size defaultAssignedSize;
        private int assignedTextHeight;
        private int buttonHeight;
        private int nextTemplateIndex;
        private Dictionary<Settings.WindowTemplate, BaseTemplate> templates;
        private Dictionary<Settings.WindowTemplate.Assignment, TemplateDisplay> assigned;
        private Orientation orientation;
        private bool sorting;

        public formTemplatesCompact()
        {
            orientation = Orientation.Vertical;
            
            InitializeComponents();
        }

        protected override void OnInitializeComponents()
        {
            base.OnInitializeComponents();

            InitializeComponent();

            this.Opacity = 0;

            this.panelTemplatesScroll.MouseDown += OnMouseDown;
            this.panelTemplates.MouseDown += OnMouseDown;
            this.panelTemplatesList.MouseDown += OnMouseDown;
        }

        protected override void OnShown(EventArgs e)
        {
            panelTemplates.SuspendLayout();
            
            var screen = Screen.PrimaryScreen.Bounds;

            defaultAssignedSize = Util.RectangleConstraint.Scale(screen.Size, new Size(Scale(120), Scale(120)));
            assignedTextHeight = this.FontHeight * 2;

            _DisplaySize = Settings.TemplateBar.DisplaySize.Value;
            _HideNames = (Settings.TemplateBar.Options.Value & Settings.TemplateBarOptions.HideNames) != 0;
            Sorting_ValueChanged(Settings.TemplateBar.Sorting, EventArgs.Empty);
            Options_ValueChanged(Settings.TemplateBar.Options, EventArgs.Empty);

            templates = new Dictionary<Settings.WindowTemplate, BaseTemplate>();
            assigned = new Dictionary<Settings.WindowTemplate.Assignment, TemplateDisplay>();

            var isList = (Settings.TemplateBar.Options.Value & Settings.TemplateBarOptions.DisplayAsList) != 0;

            listToolStripMenuItem.Checked = isList;
            templatesToolStripMenuItem.Checked = !isList;

            if (isList)
            {
                panelTemplatesScroll.SuspendLayout();
                InitButtons();
                panelTemplates.Visible = false;
                panelTemplatesList.Visible = true;
                panelTemplatesScroll.ResumeLayout();
            }
            
            for (int i = 0, count = Settings.WindowTemplates.Count; i < count; i++)
            {
                OnTemplateAdded(Settings.WindowTemplates[i]);
            }
            
            if (Settings.TemplateBar.Sorting.HasValue && Settings.TemplateBar.Sorting.Value != Settings.TemplateBar.SortMode.None)
                Sort();

            panelTemplates.ResumeLayout();

            Settings.WindowTemplates.ValueAdded += WindowTemplates_ValueAdded;
            Settings.WindowTemplates.ValueRemoved += WindowTemplates_ValueRemoved;
            Settings.TemplateBar.Sorting.ValueChanged += Sorting_ValueChanged;
            Settings.TemplateBar.Options.ValueChanged += Options_ValueChanged;

            if (Settings.WindowBounds[this.GetType()].HasValue)
                this.Bounds = Util.ScreenUtil.Constrain(Settings.WindowBounds[this.GetType()].Value);
            else
                this.Location = Cursor.Position;

            OnSizeChanged();

            base.OnShown(e);

            this.Refresh();
            this.Opacity = 0.95f;
        }

        void Options_ValueChanged(object sender, EventArgs e)
        {
            var s = (Settings.ISettingValue<Settings.TemplateBarOptions>)sender;
            var v = s.Value;

            showNameToolStripMenuItem.Checked = (v & Settings.TemplateBarOptions.HideNames) == 0;
            showOnTopToolStripMenuItem.Checked = (v & Settings.TemplateBarOptions.TopMost) != 0;
        }

        void Sorting_ValueChanged(object sender, EventArgs e)
        {
            var s = (Settings.ISettingValue<Settings.TemplateBar.SortMode>)sender;
            var m = Settings.TemplateBar.SortMode.None;
            var d = false;

            if (s.HasValue)
            {
                m = s.Value & ~Settings.TemplateBar.SortMode.Descending;
                d = (s.Value & Settings.TemplateBar.SortMode.Descending) != 0;
            }

            activeToolStripMenuItem.Checked = (m & Settings.TemplateBar.SortMode.Active) != 0;
            nameToolStripMenuItem.Checked = (m & Settings.TemplateBar.SortMode.Name) != 0;
            descendingToolStripMenuItem.Checked = d;
        }

        private void InitButtons()
        {
            buttonHeight = this.FontHeight + Scale(5) * 2;

            panelTemplatesList.SuspendLayout();

            foreach (TemplateDisplay td in panelTemplates.Controls)
            {
                AddButton(td);
            }

            panelTemplatesList.ResumeLayout();
        }

        private string GetText(Settings.WindowTemplate.Assignment a)
        {
            return a == null || string.IsNullOrEmpty(a.Name) ? "(no name)" : a.Name;
        }

        private void AddButton(TemplateDisplay td)
        {
            var b = new UI.Controls.AccountBarButton()
            {
                ForeColor = Util.Color.Darken(Color.White, 0.10f),
                ForeColorHovered = Color.White,
                BackColor = Util.Color.Lighten(Color.Black, 0.025f),
                BackColorHovered = td.Activated ? Color.FromArgb(0, 60, 0) : Util.Color.Lighten(Color.Black, 0.1f),
                BackColorSelected = Color.FromArgb(0, 50, 0),
                Text = GetText(td.AssignedTo),
                Tag = td,
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right,
                Height = GetScaledButtonHeight(),
                Selected = td.Activated,
                Margin = Padding.Empty,
                IconVisible = false,
                CloseVisible = false,
                TextVisible = true,
                Padding = new Padding(Scale(5), 0, 0, 0),
            };

            b.MouseUp += button_MouseUp;
            b.MouseDown += OnMouseDown;

            panelTemplatesList.Controls.Add(b);

            td.Tag = b;
        }

        private byte _DisplaySize;
        private byte DisplaySize
        {
            get
            {
                return _DisplaySize;
            }
            set
            {
                if (value == 20) //both 0 and 20 = 100% (% = value * 5 / 100f)
                    value = 0;

                if (_DisplaySize != value)
                {
                    Settings.TemplateBar.DisplaySize.Value = value;
                    _DisplaySize = value;
                    OnDisplaySizeChanged();
                }
            }
        }

        private bool _HideNames;
        private bool HideNames
        {
            get
            {
                return _HideNames;
            }
            set
            {
                if (_HideNames != value)
                {
                    _HideNames = value;
                    OnDisplaySizeChanged();
                }
            }
        }

        private Size GetScaledTemplateSize()
        {
            var w = defaultAssignedSize.Width;
            var h = defaultAssignedSize.Height;

            if (_DisplaySize != 0)
            {
                var scale = (_DisplaySize * 5) / 100f;
                w = (int)(w * scale + 0.5f);
                h = (int)(h * scale + 0.5f);
            }

            if (!_HideNames)
                h += assignedTextHeight;

            return new Size(w, h);
        }

        private int GetScaledButtonHeight()
        {
            var h = buttonHeight;

            if (_DisplaySize != 0)
            {
                var scale = (_DisplaySize * 5) / 100f;
                h = (int)(h * scale + 0.5f);
            }

            return h;
        }

        private void OnDisplaySizeChanged()
        {
            var sz = GetScaledTemplateSize();
            panelTemplates.SuspendLayout();

            foreach (TemplateDisplay c in panelTemplates.Controls)
            {
                c.Size = sz;
                c.TextHeight = _HideNames ? 0 : assignedTextHeight;
            }

            panelTemplates.ResumeLayout();

            if (panelTemplatesList.Controls.Count > 0)
            {
                var h = GetScaledButtonHeight();
                panelTemplatesList.SuspendLayout();

                foreach (Control c in panelTemplatesList.Controls)
                {
                    c.Height = h;
                }

                panelTemplatesList.ResumeLayout();
            }

            OnSizeChanged();
        }

        private async void SortAsync()
        {
            if (sorting)
                return;
            sorting = true;

            await Task.Delay(100);

            sorting = false;
            Sort();
        }

        private void Sort()
        {
            var sorting = Settings.TemplateBar.Sorting.Value & ~Settings.TemplateBar.SortMode.Descending;
            var descending = (Settings.TemplateBar.Sorting.Value & Settings.TemplateBar.SortMode.Descending) != 0;
            var controls = new Control[panelTemplates.Controls.Count];
            panelTemplates.Controls.CopyTo(controls, 0);

            Array.Sort(controls,
                delegate(Control a, Control b)
                {
                    var td1 = (TemplateDisplayIndexed)a;
                    var td2 = (TemplateDisplayIndexed)b;
                    int o;

                    switch (sorting)
                    {
                        case Settings.TemplateBar.SortMode.Active:
                            o = td1.AssignedTo.EnabledKey.CompareTo(td2.AssignedTo.EnabledKey);
                            break;
                        case Settings.TemplateBar.SortMode.Name:
                            o = td1.Text.CompareTo(td2.Text);
                            break;
                        default:
                            o = 0;
                            break;
                    }

                    if (o == 0)
                    {
                        o = td1.ParentTemplate.Index.CompareTo(td2.ParentTemplate.Index);
                        if (o == 0)
                            o = td1.Index.CompareTo(td2.Index);
                    }

                    if (descending)
                        o = -o;

                    return o;
                });

            panelTemplates.SuspendLayout();

            for (var i = 0; i < controls.Length; i++)
            {
                panelTemplates.Controls.SetChildIndex(controls[i], i);
                controls[i] = (Control)controls[i].Tag;
            }

            panelTemplates.ResumeLayout();

            if (controls.Length > 0 && controls[0] != null)
            {
                panelTemplatesList.SuspendLayout();
                panelTemplatesList.Controls.AddRange(controls);
                panelTemplatesList.ResumeLayout();
            }
        }

        void WindowTemplates_ValueRemoved(object sender, Settings.WindowTemplate e)
        {
            Util.Invoke.Required(this,
                delegate
                {
                    OnTemplateRemoved(e);
                });
        }

        void WindowTemplates_ValueAdded(object sender, Settings.WindowTemplate e)
        {
            Util.Invoke.Required(this,
                delegate
                {
                    OnTemplateAdded(e);
                });
        }

        void Assigned_ValueRemoved(object sender, Settings.WindowTemplate.Assignment e)
        {
            Util.Invoke.Required(this,
                delegate
                {
                    OnAssignedRemoved(e);
                });
        }

        void Assigned_ValueAdded(object sender, Settings.WindowTemplate.Assignment e)
        {
            Util.Invoke.Required(this,
                delegate
                {
                    var parent = FindAssigned(sender);
                    if (parent != null)
                        OnAssignedAdded(parent, e);
                });
        }

        private BaseTemplate FindAssigned(object l)
        {
            foreach (var bt in templates.Values)
            {
                if (object.ReferenceEquals(bt.Source.Assignments, l))
                {
                    return bt;
                }
            }

            return null;
        }

        private void OnTemplateRemoved(Settings.WindowTemplate t)
        {
            t.Assignments.ValueAdded -= Assigned_ValueAdded;
            t.Assignments.ValueRemoved -= Assigned_ValueRemoved;

            BaseTemplate bt;

            if (templates.TryGetValue(t, out bt))
            {
                templates.Remove(t);

                t.ScreensChanged -= template_ScreensChanged;

                for (var i = t.Assignments.Count - 1; i >= 0; --i)
                {
                    t.Assignments[i].EnabledChanged -= assigned_EnabledChanged;
                    t.Assignments[i].NameChanged -= assigned_NameChanged;
                }

                panelTemplates.SuspendLayout();

                foreach (var td in bt.Assigned)
                {
                    Remove(td);
                }

                panelTemplates.ResumeLayout();
            }
        }

        private void OnTemplateAdded(Settings.WindowTemplate t)
        {
            panelTemplates.SuspendLayout();

            var bt = templates[t] = new BaseTemplate(t)
            {
                Index = nextTemplateIndex++,
            };

            t.ScreensChanged += template_ScreensChanged;

            t.Assignments.ValueAdded += Assigned_ValueAdded;
            t.Assignments.ValueRemoved += Assigned_ValueRemoved;

            var assigned = t.Assignments;
            var count = assigned.Count;

            for (var j = 0; j < count; j++)
            {
                OnAssignedAdded(bt, assigned[j]);
            }

            panelTemplates.ResumeLayout();
        }

        void template_ScreensChanged(object sender, EventArgs e)
        {
            var t = (Settings.WindowTemplate)sender;
            BaseTemplate bt;

            if (templates.TryGetValue(t, out bt) && bt.Assigned.Count > 0)
            {
                var st = new ScreenTemplate(t);

                foreach (var td in bt.Assigned)
                {
                    td.Template = st;
                }
            }
        }

        private void OnAssignedAdded(BaseTemplate parent, Settings.WindowTemplate.Assignment a)
        {
            var previous = parent.Assigned.Count > 0 ? parent.Assigned[parent.Assigned.Count - 1] : null;

            var td = new TemplateDisplayIndexed()
            {
                Margin = Padding.Empty,
                Padding = new Padding(Scale(3)),
                Source = parent.Source,
                Template = previous == null ? new ScreenTemplate(parent.Source) : previous.Template,
                AssignedTo = a,
                Activated = a.Enabled,
                TextHeight = _HideNames ? 0 : assignedTextHeight,
                Size = GetScaledTemplateSize(),
                Text = a.Name,
                Shared = parent.Assigned,

                ForeColorHighlight = Color.FromArgb(160, 160, 160),
                WindowBorderColorHighlight = Color.FromArgb(100, 100, 100),
                ForeColor = Color.FromArgb(140, 140, 140),
                WindowBorderColor = Color.FromArgb(80, 80, 80),
                ScreenBackColor = Color.FromArgb(30, 30, 30),
                ScreenBorderColor = Color.FromArgb(90, 90, 90),
                TextColor = Color.FromArgb(200, 200, 200),

                ParentTemplate = parent,
                Index = parent.NextChildIndex++,
            };

            if (buttonHeight != 0)
                AddButton(td);

            assigned[a] = td;
            parent.Assigned.Add(td);

            a.EnabledChanged += assigned_EnabledChanged;
            a.NameChanged += assigned_NameChanged;

            td.MouseUp += assigned_MouseUp;
            td.MouseDown += OnMouseDown;

            panelTemplates.SuspendLayout();

            panelTemplates.Controls.Add(td);

            if ((Settings.TemplateBar.Sorting.Value & ~Settings.TemplateBar.SortMode.Descending) == Settings.TemplateBar.SortMode.None && previous != null)
            {
                if ((Settings.TemplateBar.Sorting.Value & Settings.TemplateBar.SortMode.Descending) != 0)
                    panelTemplates.Controls.SetChildIndex(td, panelTemplates.Controls.GetChildIndex(parent.Assigned[0]));
                else
                    panelTemplates.Controls.SetChildIndex(td, panelTemplates.Controls.GetChildIndex(previous) + 1);
            }

            panelTemplates.ResumeLayout();

            if ((Settings.TemplateBar.Sorting.Value & ~Settings.TemplateBar.SortMode.Descending) != Settings.TemplateBar.SortMode.None)
                SortAsync();
        }

        private void OnAssignedRemoved(Settings.WindowTemplate.Assignment a)
        {
            TemplateDisplay td;
            if (assigned.TryGetValue(a, out td))
            {
                assigned.Remove(a);

                a.EnabledChanged -= assigned_EnabledChanged;
                a.NameChanged -= assigned_NameChanged;

                td.Shared.Remove(td);
                Remove(td);
            }
        }

        private void Remove(TemplateDisplay td)
        {
            if (td.Tag != null)
            {
                var b = (Control)td.Tag;
                panelTemplatesList.Controls.Remove(b);
                b.Dispose();
            }
            panelTemplates.Controls.Remove(td);
            td.Dispose();
        }

        private void SetActive(TemplateDisplay td, bool enabled)
        {
            var a = td.AssignedTo;
            if (a != null)
            {
                if (a.Enabled == enabled)
                    return;
                if (enabled)
                    Tools.WindowManager.Instance.Activate(td.Source, td.AssignedTo);
                else
                    Tools.WindowManager.Instance.Deactivate(td.Source, td.AssignedTo);
            }
        }

        void assigned_EnabledChanged(object sender, EventArgs e)
        {
            var a = (Settings.WindowTemplate.Assignment)sender;
            TemplateDisplay td;

            if (assigned.TryGetValue(a, out td))
            {
                td.Activated = a.Enabled;

                var b = (UI.Controls.FlatButton)td.Tag;
                if (b != null)
                {
                    b.BackColorHovered = a.Enabled ? Color.FromArgb(0, 60, 0) : Util.Color.Lighten(Color.Black, 0.1f);
                    b.Selected = a.Enabled;
                }

                if ((Settings.TemplateBar.Sorting.Value & Settings.TemplateBar.SortMode.Active) != 0)
                    SortAsync();
            }
        }

        void assigned_NameChanged(object sender, EventArgs e)
        {
            var a = (Settings.WindowTemplate.Assignment)sender;
            TemplateDisplay td;

            if (assigned.TryGetValue(a, out td))
            {
                td.Text = a.Name;

                var b = (UI.Controls.FlatButton)td.Tag;
                if (b != null)
                {
                    b.Text = GetText(a);
                }

                if ((Settings.TemplateBar.Sorting.Value & Settings.TemplateBar.SortMode.Name) != 0)
                    SortAsync();
            }
        }

        void assigned_MouseUp(object sender, MouseEventArgs e)
        {
            if (e.Button == System.Windows.Forms.MouseButtons.Left)
            {
                var td = (TemplateDisplay)sender;
                SetActive(td, !td.AssignedTo.Enabled);
            }
        }

        void button_MouseUp(object sender, MouseEventArgs e)
        {
            if (e.Button == System.Windows.Forms.MouseButtons.Left)
            {
                var td = (TemplateDisplay)((Control)sender).Tag;
                SetActive(td, !td.AssignedTo.Enabled);
            }
        }

        void OnMouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == System.Windows.Forms.MouseButtons.Right)
            {
                contextMenu.Show(Cursor.Position);
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                Settings.WindowTemplates.ValueAdded -= WindowTemplates_ValueAdded;
                Settings.WindowTemplates.ValueRemoved -= WindowTemplates_ValueRemoved;
                Settings.TemplateBar.Sorting.ValueChanged -= Sorting_ValueChanged;
                Settings.TemplateBar.Options.ValueChanged -= Options_ValueChanged;

                foreach (var t in templates.Keys)
                {
                    t.Assignments.ValueAdded -= Assigned_ValueAdded;
                    t.Assignments.ValueRemoved -= Assigned_ValueRemoved;
                    t.ScreensChanged -= template_ScreensChanged;

                    for (var i = t.Assignments.Count - 1; i >= 0; --i)
                    {
                        t.Assignments[i].EnabledChanged -= assigned_EnabledChanged;
                        t.Assignments[i].NameChanged -= assigned_NameChanged;
                    }
                }

                if (components != null)
                {
                    components.Dispose();
                }
            }

            base.Dispose(disposing);
        }

        public Orientation Orientation
        {
            get
            {
                return orientation;
            }
            set
            {
                if (orientation != value)
                {
                    orientation = value;
                    OnOrientationChanged();
                }
            }
        }

        private void OnOrientationChanged()
        {
            var bw = Scale(10);
            var margin = Scale(5);

            this.SuspendLayout();
            panelTemplates.SuspendLayout();

            if (orientation == System.Windows.Forms.Orientation.Horizontal)
            {
                barTop.SetBounds(0, 0, bw, this.Height);
                barTop.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Bottom;

                barBottom.SetBounds(this.Width - bw, 0, bw, this.Height);
                barBottom.Anchor = AnchorStyles.Top | AnchorStyles.Right | AnchorStyles.Bottom;

                var w = this.Width - bw * 2 - margin * 2;
                var h = this.Height - margin * 2;
                panelTemplatesScroll.SetBounds(bw + margin, margin, w, h);
                panelTemplates.FlowDirection = FlowDirection.TopDown;
                panelTemplates.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Bottom;
                panelTemplates.Size = new Size(0, h);
            }
            else
            {
                barTop.SetBounds(0, 0, this.Width, bw);
                barTop.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;

                barBottom.SetBounds(0, this.Height - bw, this.Width, bw);
                barBottom.Anchor = AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Bottom;

                var w = this.Width - margin * 2;
                var h = this.Height - bw * 2 - margin * 2;
                panelTemplatesScroll.SetBounds(margin, bw + margin, w, h);
                panelTemplates.FlowDirection = FlowDirection.LeftToRight;
                panelTemplates.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
                panelTemplates.Size = new Size(w, 0);
            }

            panelTemplates.ResumeLayout();
            this.ResumeLayout();
        }

        protected override void OnMouseWheel(MouseEventArgs e)
        {
            base.OnMouseWheel(e);
            
            Panel p = panelTemplatesList.Visible ? panelTemplatesList : panelTemplates;
            int x, y;

            if (orientation == System.Windows.Forms.Orientation.Horizontal)
            {
                y = 0;

                if (p.Width > panelTemplatesScroll.Width)
                {
                    x = p.Left;

                    var change = p.Width / 4;

                    if (e.Delta > 0)
                    {
                        x += change;
                        if (x > 0)
                            x = 0;
                    }
                    else
                    {
                        x -= change;
                        var min = panelTemplatesScroll.Width - p.Width;
                        if (min > 0)
                            x = 0;
                        else if (x < min)
                            x = min;
                    }
                }
                else
                {
                    x = 0;
                }
            }
            else
            {
                x = 0;

                if (p.Height > panelTemplatesScroll.Height)
                {
                    y = p.Top;
                    var change = p.Height / 4;

                    if (e.Delta > 0)
                    {
                        y += change;
                        if (y > 0)
                            y = 0;
                    }
                    else
                    {
                        y -= change;
                        var min = panelTemplatesScroll.Height - p.Height;
                        if (min > 0)
                            y = 0;
                        else if (y < min)
                            y = min;
                    }
                }
                else
                {
                    y = 0;
                }
            }

            p.Location = new Point(x, y);
        }

        protected override void OnSizeChanged(EventArgs e)
        {
            base.OnSizeChanged(e);

            OnSizeChanged();
        }

        private void OnSizeChanged()
        {
            Panel p = panelTemplatesList.Visible ? panelTemplatesList : panelTemplates;
            int x = p.Left,
                y = p.Top,
                min;

            min = panelTemplatesScroll.Width - p.Width;
            if (min > 0)
                x = 0;
            else if (x < min)
                x = min;

            min = panelTemplatesScroll.Height - p.Height;
            if (min > 0)
                y = 0;
            else if (y < min)
                y = min;

            p.Location = new Point(x, y);

            if (this.Width > this.Height && !panelTemplatesList.Visible)
            {
                Orientation = System.Windows.Forms.Orientation.Horizontal;
            }
            else
            {
                Orientation = System.Windows.Forms.Orientation.Vertical;
            }
        }

        protected override void WndProc(ref Message m)
        {
            switch ((WindowMessages)m.Msg)
            {
                case WindowMessages.WM_NCLBUTTONDBLCLK:

                    break;
                case WindowMessages.WM_NCHITTEST:

                    base.WndProc(ref m);

                    if (m.Result == (IntPtr)HitTest.Client)
                    {
                        var p = this.PointToClient(new Point(m.LParam.GetValue32()));

                        if (barTop.Bounds.Contains(p))
                        {
                            m.Result = (IntPtr)HitTest.Caption;
                        }
                        else
                        {
                            var b = barBottom.Bounds;
                            if (b.Contains(p))
                            {
                                if (orientation == System.Windows.Forms.Orientation.Horizontal)
                                {
                                    if (p.Y > b.Bottom - 10)
                                        m.Result = (IntPtr)HitTest.BottomRight;
                                    else if (p.Y < b.Top + 10)
                                        m.Result = (IntPtr)HitTest.TopRight;
                                    else
                                        m.Result = (IntPtr)HitTest.Right;
                                }
                                else
                                {
                                    if (p.X > b.Right - 10)
                                        m.Result = (IntPtr)HitTest.BottomRight;
                                    else if (p.X < b.Left + 10)
                                        m.Result = (IntPtr)HitTest.BottomLeft;
                                    else
                                        m.Result = (IntPtr)HitTest.Bottom;
                                }
                            }
                        }
                    }

                    break;
                case WindowMessages.WM_EXITSIZEMOVE:

                    base.WndProc(ref m);

                    Settings.WindowBounds[this.GetType()].Value = this.Bounds;

                    break;
                default:

                    base.WndProc(ref m);

                    break;
            }
        }

        protected override void OnMouseDown(MouseEventArgs e)
        {
            base.OnMouseDown(e);

            OnMouseDown(this, e);
        }

        private void barTop_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == System.Windows.Forms.MouseButtons.Left)
            {
                NativeMethods.ReleaseCapture();
                NativeMethods.SendMessage(this.Handle, (uint)WindowMessages.WM_NCLBUTTONDOWN, (IntPtr)HitTest.Caption, IntPtr.Zero);
            }
            else if (e.Button == System.Windows.Forms.MouseButtons.Right)
            {
                contextMenu.Show(Cursor.Position);
            }
        }

        private void displayAsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (((ToolStripMenuItem)sender).Checked)
                return;

            var isList = sender == listToolStripMenuItem;

            listToolStripMenuItem.Checked = isList;
            templatesToolStripMenuItem.Checked = !isList;

            SetOption(Settings.TemplateBarOptions.DisplayAsList, isList);

            if (isList && buttonHeight == 0)
            {
                InitButtons();
            }

            panelTemplatesScroll.SuspendLayout();
            panelTemplates.Visible = !isList;
            panelTemplatesList.Visible = isList;
            OnSizeChanged();
            panelTemplatesScroll.ResumeLayout();
        }

        private void SetOption(Settings.TemplateBarOptions o, bool value)
        {
            var v = Settings.TemplateBar.Options.Value;

            if (value)
                v |= o;
            else
                v &= ~o;

            Settings.TemplateBar.Options.Value = v;
        }

        private void showOnTopToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var b = showOnTopToolStripMenuItem.Checked = !showOnTopToolStripMenuItem.Checked;

            SetOption(Settings.TemplateBarOptions.TopMost, b);

            this.TopMost = b;
        }

        private void sizeToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var f = new Base.StackFormBase()
            {
                StartPosition = FormStartPosition.Manual,
                ShowInTaskbar = false,
                FormBorderStyle = System.Windows.Forms.FormBorderStyle.None,
                AutoSize = true,
                AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink,
                BackColor = this.BackColor,
                Opacity = 0,
            };

            var panel = new Controls.StackPanel()
            {
                FlowDirection = FlowDirection.LeftToRight,
                Margin = new Padding(5),
                AutoSize = true,
                AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowOnly,
                AutoSizeFill = UI.Controls.StackPanel.AutoSizeFillMode.NoWrap,
                Bounds = Rectangle.Empty,
            };

            var slider = new Controls.FlatSlider()
            {
                Width = 150,
                Margin = Padding.Empty,
                Anchor = AnchorStyles.Left,
                Value = ((_DisplaySize == 0 ? 100 : _DisplaySize * 5) / 100f - 0.25f) / 1.75f,
            };

            var label = new Label()
            {
                AutoSize = true,
                ForeColor = Color.White,
                Margin = new Padding(5, 0, 0, 0),
                Anchor = AnchorStyles.Left,
                Text = (_DisplaySize == 0 ? 100 : _DisplaySize * 5) + "%",
            };

            slider.ValueChanged += delegate
            {
                var v = (byte)((0.25f + slider.Value * 1.75f) * 100 / 5);

                if (_DisplaySize != v)
                {
                    DisplaySize = v;
                    label.Text = (_DisplaySize == 0 ? 100 : _DisplaySize * 5) + "%";
                }
            };

            f.Deactivate += delegate
            {
                f.Dispose();
            };

            f.Shown += delegate
            {
                f.Location = new Point(Cursor.Position.X, Cursor.Position.Y - f.Height / 2);
                f.Opacity = this.Opacity;
            };

            panel.Controls.AddRange(new Control[] { slider, label });
            f.Controls.Add(panel);
            
            f.Show(this);
        }

        private void showNameToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var b = showNameToolStripMenuItem.Checked = !showNameToolStripMenuItem.Checked;

            SetOption(Settings.TemplateBarOptions.HideNames, !b);

            HideNames = !b;
        }

        private void closeToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void sortToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var s = (ToolStripMenuItem)sender;
            var m = Settings.TemplateBar.Sorting.Value & ~Settings.TemplateBar.SortMode.Descending;
            var d = Settings.TemplateBar.Sorting.Value & Settings.TemplateBar.SortMode.Descending;

            s.Checked = !s.Checked;

            if (s == descendingToolStripMenuItem)
            {
                if (!s.Checked)
                    d = Settings.TemplateBar.SortMode.None;
            }
            else if (s.Checked)
            {
                if (s == activeToolStripMenuItem)
                    m = Settings.TemplateBar.SortMode.Active;
                else if (s == nameToolStripMenuItem)
                    m = Settings.TemplateBar.SortMode.Name;
                else
                    m = Settings.TemplateBar.SortMode.None;
            }
            else
            {
                m = Settings.TemplateBar.SortMode.None;
            }

            Settings.TemplateBar.Sorting.Value = m | d;

            Sort();
        }
    }
}
