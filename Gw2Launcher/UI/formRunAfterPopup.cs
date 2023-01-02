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
using System.Diagnostics;

namespace Gw2Launcher.UI
{
    public partial class formRunAfterPopup : Base.BaseForm
    {
        private class RunAfterBarButton : Controls.AccountBarButton
        {
            public int Index
            {
                get;
                set;
            }

            public Settings.RunAfter RunAfter
            {
                get;
                set;
            }

            private Client.Launcher.IRunAfterProcess _RunAfterProcess;
            public Client.Launcher.IRunAfterProcess RunAfterProcess
            {
                get
                {
                    return _RunAfterProcess;
                }
                set
                {
                    if (_RunAfterProcess != value)
                    {
                        if (_RunAfterProcess != null)
                        {
                            _RunAfterProcess.Started -= process_Started;
                            _RunAfterProcess.Exited -= process_Exited;
                        }

                        _RunAfterProcess = value;

                        bool b;

                        if (b = value != null)
                        {
                            value.Started += process_Started;
                            value.Exited += process_Exited;

                            b = value.IsActive;
                        }

                        this.Selected = b;
                    }
                }
            }

            void process_Exited(object sender, EventArgs e)
            {
                this.Selected = false;
            }

            void process_Started(object sender, EventArgs e)
            {
                this.Selected = true;
            }

            private Tools.Shared.Images.SourceValue _ImageSource;
            public Tools.Shared.Images.SourceValue ImageSource
            {
                get
                {
                    return _ImageSource;
                }
                set
                {
                    if (_ImageSource != value)
                    {
                        using (_ImageSource)
                        {
                            _ImageSource = value;

                            if (value != null)
                            {
                                value.SourceLoaded += image_SourceLoaded;

                                this.BackgroundImage = value.GetValue();
                            }
                            else
                            {
                                this.BackgroundImage = null;
                            }
                        }
                    }
                }
            }

            void image_SourceLoaded(object sender, EventArgs e)
            {
                var source = (Tools.Shared.Images.SourceValue)sender;
                if (source.RefreshValue(true))
                {
                    this.BackgroundImage = source.GetValue();
                }
            }

            protected override void Dispose(bool disposing)
            {
                if (disposing)
                {
                    ImageSource = null;
                    RunAfterProcess = null;
                }

                base.Dispose(disposing);
            }
        }

        private class AccountContainerPanel : Controls.StackPanel
        {
            /// <summary>
            /// Occurs when the manager or its processes change
            /// </summary>
            public event EventHandler ManagerProcessesChanged;

            public AccountContainerPanel(Settings.IAccount account, Label header, Label status)
            {
                AutoSize = true;
                Margin = new Padding(0, 5, 0, 0);
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
                FlowDirection = FlowDirection.TopDown;

                this.Account = account;
                this.Header = header;
                this.Status = status;
                this.Container = new BarContainerPanel();

                header.Text = account.Name;
                account.NameChanged += account_NameChanged;

                header.MouseClick += OnMouseClick;
                status.MouseClick += OnMouseClick;

                this.Controls.AddRange(new Control[] { header, status, this.Container });
            }

            void OnMouseClick(object sender, MouseEventArgs e)
            {
                this.OnMouseClick(e);
            }

            public Settings.IAccount Account
            {
                get;
                private set;
            }

            public Process Process
            {
                get;
                set;
            }

            public SortingAction Sorting
            {
                get;
                set;
            }

            private Client.Launcher.IRunAfterManager _Manager;
            public Client.Launcher.IRunAfterManager Manager
            {
                get
                {
                    return _Manager;
                }
                set
                {
                    if (_Manager != value)
                    {
                        using (_Manager)
                        {
                            _Manager = value;
                            if (value != null)
                            {
                                value.ProcessesChanged += OnManagerProcessesChanged;
                                OnManagerProcessesChanged(value, EventArgs.Empty);
                            }
                        }
                    }
                }
            }

            void OnManagerProcessesChanged(object sender, EventArgs e)
            {
                if (ManagerProcessesChanged != null)
                {
                    try
                    {
                        Util.Invoke.Required(this,
                            delegate
                            {
                                ManagerProcessesChanged(this, e);
                            });
                    }
                    catch { }
                }
            }

            public Label Header
            {
                get;
                private set;
            }

            public Label Status
            {
                get;
                private set;
            }

            public BarContainerPanel Container
            {
                get;
                private set;
            }

            public bool StatusVisible
            {
                get
                {
                    return Status.Visible;
                }
                set
                {
                    this.SuspendLayout();

                    Status.Visible = value;
                    Container.Visible = !value;

                    this.ResumeLayout();
                }
            }

            void account_NameChanged(object sender, EventArgs e)
            {
                Util.Invoke.Required(this, new Action(
                    delegate
                    {
                        this.Header.Text = ((Settings.IAccount)sender).Name;
                    }));
            }

            protected override void Dispose(bool disposing)
            {
                if (disposing)
                {
                    this.Account.NameChanged -= account_NameChanged;
                    this.Manager = null;
                }

                base.Dispose(disposing);
            }

            public override Size GetPreferredSize(Size proposedSize)
            {
                int w = 0,
                    h = 0;

                foreach (Control c in this.Controls)
                {
                    if (!c.Visible)
                        continue;

                    var sz = c.GetPreferredSize(proposedSize);
                    var szw = sz.Width + c.Margin.Horizontal;

                    if (szw > w)
                        w = szw;
                    h += c.Height + c.Margin.Vertical;
                }

                return new Size(w, h);
            }
        }

        private class BarContainerPanel : Controls.StackPanel
        {
            public BarContainerPanel()
            {
                AutoSize = true;
                Margin = new Padding(0);
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
                FlowDirection = FlowDirection.TopDown;
            }

            private RunAfterBarButton[] _Buttons;
            public RunAfterBarButton[] Buttons
            {
                get
                {
                    return _Buttons;
                }
                set
                {
                    this.SuspendLayout();

                    _Buttons = value;
                    if (value != null)
                        this.Controls.AddRange(value);
                    else
                        this.Controls.Clear();

                    this.ResumeLayout();
                }
            }

            public Label Header
            {
                get;
                set;
            }

            public Label Status
            {
                get;
                set;
            }

            public int VisibleCount
            {
                get;
                set;
            }

            public override Size GetPreferredSize(Size proposedSize)
            {
                int w = 0,
                    h = 0;

                foreach (Control c in this.Controls)
                {
                    if (!c.Visible)
                        continue;

                    var sz = c.GetPreferredSize(proposedSize);
                    var szw = sz.Width + c.Margin.Horizontal;

                    if (szw > w)
                        w = szw;
                    h += c.Height + c.Margin.Vertical;
                }

                if (proposedSize.Width < w && proposedSize.Width > 0)
                {
                    w = proposedSize.Width;
                }

                return new Size(w, h);
            }
        }
        
        private IList<Settings.IAccount> accounts;
        private System.Diagnostics.Process process;
        private Dictionary<Settings.IAccount, AccountContainerPanel> panels;
        private Tools.Shared.Images images;

        [Flags]
        private enum SortingAction : byte
        {
            None = 0,
            Sort = 1,
            Filter = 2,
        }

        private Settings.RunAfterPopupFilter filter;
        private Settings.RunAfterPopupOptions options;
        private Settings.RunAfterPopupSorting sorting;
        private SortingAction _sorting;
        private bool _autosizing;

        public formRunAfterPopup(IList<Settings.IAccount> accounts)
        {
            this.accounts = accounts;
            this.images = new Tools.Shared.Images(false);

            InitializeComponents();
        }

        public formRunAfterPopup(Settings.IAccount account, System.Diagnostics.Process p)
            : this(new Settings.IAccount[] { account })
        {
            this.process = p;
        }

        protected override CreateParams CreateParams
        {
            get
            {
                var cp = base.CreateParams;
                cp.ExStyle |= (int)(Windows.Native.WindowStyle.WS_EX_LAYERED);
                return cp;
            }
        }

        protected override void OnInitializeComponents()
        {
            InitializeComponent();

            panels = new Dictionary<Settings.IAccount, AccountContainerPanel>(accounts.Count);

            sorting = Settings.RunAfterPopup.Sorting.HasValue
                ? Settings.RunAfterPopup.Sorting.Value
                : Settings.RunAfterPopupSorting.None;

            nameToolStripMenuItem.Checked = (sorting & Settings.RunAfterPopupSorting.Name) != 0;
            activeToolStripMenuItem.Checked = (sorting & Settings.RunAfterPopupSorting.Active) != 0;
            typeToolStripMenuItem.Checked = (sorting & Settings.RunAfterPopupSorting.Type) != 0;
            descendingToolStripMenuItem.Checked = (sorting & Settings.RunAfterPopupSorting.Descending) != 0;

            filter = Settings.RunAfterPopup.Filter.HasValue
                ? Settings.RunAfterPopup.Filter.Value
                : Settings.RunAfterPopupFilter.None;

            isAccountActiveToolStripMenuItem.Checked = (filter & Settings.RunAfterPopupFilter.ActiveAccount) != 0;
            isActiveToolStripMenuItem.Checked = (filter & Settings.RunAfterPopupFilter.ProcessActive) != 0;
            isNotActiveToolStripMenuItem.Checked = (filter & Settings.RunAfterPopupFilter.ProcessInactive) != 0;
            isStartedManuallyToolStripMenuItem.Checked = (filter & Settings.RunAfterPopupFilter.ManualStartup) != 0;
            hasNotBeenStartedToolStripMenuItem.Checked = (filter & Settings.RunAfterPopupFilter.ProcessHasNotStarted) != 0;

            options = Settings.RunAfterPopup.Options.HasValue
                ? Settings.RunAfterPopup.Options.Value
                : Settings.RunAfterPopupOptions.ShowIcon | Settings.RunAfterPopupOptions.ShowClose;

            keepWindowOpenToolStripMenuItem.Checked = (options & Settings.RunAfterPopupOptions.KeepOpen) != 0;
            showIconToolStripMenuItem.Checked = (options & Settings.RunAfterPopupOptions.ShowIcon) != 0;
            showProcessExitButtonToolStripMenuItem.Checked = (options & Settings.RunAfterPopupOptions.ShowClose) != 0;

            Client.Launcher.AccountProcessChanged += Launcher_AccountProcessChanged;

            panelContent.SuspendLayout();

            var first = true;

            foreach (var a in accounts)
            {
                var m = Client.Launcher.GetRunAfter(a);

                var panel = new AccountContainerPanel(a, CreateHeader(), CreateStatus())
                {
                    Visible = true,
                    Manager = m,
                    Process = Client.Launcher.GetProcess(a),
                };

                panel.MouseClick += OnMouseClick;
                panel.ManagerProcessesChanged += panel_ManagerProcessesChanged;

                if (first)
                {
                    first = false;
                    panel.Margin = new Padding(0);
                }

                panels[a] = panel;

                AddButtons(panel);

                panelContent.Controls.Add(panel);
            }

            panelContent.ResumeLayout();
        }

        void Launcher_AccountProcessChanged(Settings.IAccount account, System.Diagnostics.Process e)
        {
            Util.Invoke.Required(this,
                delegate
                {
                    AccountContainerPanel panel;
                    if (panels.TryGetValue(account, out panel))
                    {
                        var changed = panel.Process == null && e != null || panel.Process != null && e == null;

                        panel.Process = e;

                        if (changed && (filter & Settings.RunAfterPopupFilter.ActiveAccount) != 0)
                            SortDelayed(SortingAction.Filter, panel);
                    }
                });
        }

        void panel_ManagerProcessesChanged(object sender, EventArgs e)
        {
            AddButtons((AccountContainerPanel)sender);

            if (!_autosizing)
            {
                _autosizing = true;
                Util.Invoke.Async(this,
                    delegate
                    {
                        _autosizing = false;
                        this.Bounds = GetPreferredWindowBounds(this.Location, true);
                    });
            }
        }

        private void AddButtons(AccountContainerPanel panel)
        {
            panel.Container.SuspendLayout();

            var buttons = panel.Container.Buttons;
            var a = panel.Account;
            var m = panel.Manager;
            var visible = 0;
            var count = 0;

            if (m != null)
            {
                var processes = m.GetProcesses();
                if (buttons == null)
                    buttons = new RunAfterBarButton[processes.Length];
                else if (buttons.Length < processes.Length)
                    Array.Resize<RunAfterBarButton>(ref buttons, processes.Length);

                AddButtons(panel, buttons, a, processes, ref count, ref visible);
            }

            if (buttons != null && count != buttons.Length)
            {
                for (var i = count; i < buttons.Length; i++)
                {
                    if (buttons[i] == null)
                        break;
                    buttons[i].Dispose();
                }
                if (count > 0)
                    Array.Resize<RunAfterBarButton>(ref buttons, count);
                else
                    buttons = null;
            }

            if (buttons != null && (options & Settings.RunAfterPopupOptions.ShowIcon) != 0)
            {
                foreach (var b in buttons)
                {
                    LoadIconAsync(b);
                }
            }

            Sort(buttons);

            panel.Container.Buttons = buttons;
            panel.Container.VisibleCount = visible;
            panel.Status.Text = "None";
            panel.StatusVisible = visible == 0;

            panel.Container.ResumeLayout();
        }

        void Launcher_RunAfterChanged(Settings.IAccount account)
        {
            AccountContainerPanel panel;
            if (panels.TryGetValue(account, out panel))
            {
                panel.Manager = Client.Launcher.GetRunAfter(account);
            }
        }

        protected override void OnShown(EventArgs e)
        {
            base.OnShown(e);

            if ((options & Settings.RunAfterPopupOptions.ShowIcon) != 0)
                LoadIcons();

            panelScroll.FlatVScroll.ScrollChange = labelStatus.MinimumSize.Height;

            this.MinimumSize = new System.Drawing.Size(100, labelStatus.MinimumSize.Height + panelScroll.Top * 2 + labelHeader.Height + labelHeader.Margin.Vertical);
            this.Bounds = GetPreferredWindowBounds(Cursor.Position, false);

            Client.Launcher.AccountProcessExited += Launcher_AccountProcessExited;
            Client.Launcher.RunAfterChanged += Launcher_RunAfterChanged;

            if (process != null)
            {
                try
                {
                    if (process.HasExited)
                    {
                        this.Dispose();
                        return;
                    }
                }
                catch { }
            }

            if (!NativeMethods.SetForegroundWindow(this.Handle))
            {
                AutoClose();
            }

            FadeIn(0.9f, 100);
        }

        private Rectangle GetPreferredWindowBounds(Point initialLocation, bool keepLocation = false)
        {
            var screen = Screen.FromPoint(initialLocation).WorkingArea;
            var maxW = screen.Width / 4;
            var pad = Scale(40);
            var contentW = this.Width;

            if (contentW < maxW)
            {
                foreach (Control c in panelContent.Controls)
                {
                    int _w = 0;

                    //if (c is Label)
                    //{
                    //    _w = c.GetPreferredSize(new Size(maxW, int.MaxValue)).Width;
                    //}
                    //else if (c is BarContainerPanel)
                    //{
                    //}

                    _w = c.GetPreferredSize(new Size(maxW, int.MaxValue)).Width + pad;

                    if (_w > contentW)
                    {
                        contentW = _w;
                        if (contentW >= maxW)
                        {
                            break;
                        }
                    }
                }
            }

            if (contentW < this.MinimumSize.Width)
                contentW = this.MinimumSize.Width;
            else if (contentW > maxW)
                contentW = maxW;

            var sz = panelContent.GetPreferredSize(new Size(contentW, screen.Height), true);
            var szh = sz.Height + panelScroll.Top * 2;
            var h = screen.Height * 3 / 4;
            var w = sz.Width;

            if (keepLocation)
            {
                var _h = screen.Bottom - initialLocation.Y;
                if (_h > 0 && _h < h)
                    h = _h;
            }

            if (szh < h)
                h = szh;
            else
                w += panelScroll.FlatVScroll.Width + panelScroll.FlatVScroll.Margin.Horizontal;

            if (w < this.MinimumSize.Width)
                w = this.MinimumSize.Width;
            if (h < this.MinimumSize.Height)
                h = this.MinimumSize.Height;

            var b = keepLocation ? new Rectangle(initialLocation.X, initialLocation.Y, w, h) : new Rectangle(Cursor.Position.X - w / 2, Cursor.Position.Y - h / 2, w, h);
            var hasConstraint = false;

            if (!keepLocation && process != null)
            {
                try
                {
                    var wh = Windows.FindWindow.FindMainWindow(process);
                    if (wh != IntPtr.Zero)
                    {
                        var p = Windows.WindowSize.GetWindowPlacement(wh).rcNormalPosition.ToRectangle();
                        if (hasConstraint = p.Width > 0)
                            b = Util.RectangleConstraint.Constrain(p, b);
                    }
                }
                catch { }
            }

            if (!hasConstraint)
                b = Util.RectangleConstraint.ConstrainToScreen(b);

            return b;
        }

        private async void AutoClose()
        {
            if (keepWindowOpenToolStripMenuItem.Checked)
                return;

            var wasActive = false;
            var boundsChanged = true;
            var bounds = new Rectangle();

            EventHandler onActivated = delegate
            {
                wasActive = true;
            };
            EventHandler onBoundsChanged = delegate
            {
                boundsChanged = true;
            };

            this.Activated += onActivated;
            this.SizeChanged += onBoundsChanged;
            this.LocationChanged += onBoundsChanged;

            while (true)
            {
                await Task.Delay(500);

                if (this.IsDisposed)
                    return;
                if (wasActive)
                    break;
                if (boundsChanged)
                {
                    boundsChanged = false;
                    bounds = Rectangle.Inflate(this.Bounds, Cursor.Size.Width / 2, Cursor.Size.Height / 2);
                }
                if (!bounds.Contains(Cursor.Position) && !contextMenu.Visible)
                {
                    this.Dispose();
                    break;
                }
            }

            this.Activated -= onActivated;
            this.SizeChanged -= onBoundsChanged;
            this.LocationChanged -= onBoundsChanged;
        }

        private async void FadeIn(float o, int duration)
        {
            var t = Environment.TickCount;

            do
            {
                await Task.Delay(10);

                var e = Environment.TickCount - t;
                if (e > duration)
                    break;
                this.Opacity = o * e / duration;
            }
            while (true);

            this.Opacity = o;
        }

        protected override void OnDeactivate(EventArgs e)
        {
            base.OnDeactivate(e);

            AutoClose();
        }

        private void LoadIcons()
        {
            foreach (Control c in panelContent.Controls)
            {
                if (c is AccountContainerPanel)
                {
                    var buttons = ((AccountContainerPanel)c).Container.Buttons;
                    if (buttons == null)
                        continue;
                    foreach (var b in buttons)
                    {
                        LoadIconAsync(b);
                    }
                }
            }
        }

        private Task<Image> LoadIconAsync(string path)
        {
            return Task.Run<Image>(
                delegate
                {
                    try
                    {
                        using (var icon = Windows.ShellIcons.GetIcon(path, Windows.ShellIcons.IconSize.Small))
                        {
                            if (icon == null)
                                return null;

                            return icon.ToBitmap();
                        }
                    }
                    catch
                    {
                        return null;
                    }
                });
        }

        private async void LoadIconAsync(RunAfterBarButton b)
        {
            var path = b.RunAfter.GetPath();

            if (!string.IsNullOrEmpty(path))
            {
                var v = images.GetValue(path);
                var l = v.BeginLoad();

                b.ImageSource = new Tools.Shared.Images.SourceValue(v);

                if (l != null)
                {
                    using (l)
                    {
                        l.SetValue(await LoadIconAsync(path));
                    }
                }
            }
            else
            {
                b.ImageSource = null;
            }
        }

        private async void SortDelayed(SortingAction s, AccountContainerPanel panel)
        {
            if (panel.Sorting != SortingAction.None)
            {
                panel.Sorting |= s;
                return;
            }

            panel.Sorting = s;

            await Task.Delay(100);

            s = panel.Sorting;
            panel.Sorting = SortingAction.None;

            Sort(s, panel);
        }

        private async void SortDelayed(SortingAction s)
        {
            if (_sorting != SortingAction.None)
            {
                _sorting |= s;
                return;
            }

            _sorting = s;

            await Task.Delay(100);

            s = _sorting;
            _sorting = SortingAction.None;

            Sort(s);
        }

        private void Sort(SortingAction s)
        {
            panelContent.SuspendLayout();

            foreach (Control c in panelContent.Controls)
            {
                if (c is AccountContainerPanel)
                {
                    Sort(s, (AccountContainerPanel)c);
                }
            }

            panelContent.ResumeLayout();
        }

        private void Sort(RunAfterBarButton[] buttons)
        {
            if (buttons == null || buttons.Length <= 1)
                return;

            Array.Sort<RunAfterBarButton>(buttons,
                delegate(RunAfterBarButton a, RunAfterBarButton b)
                {
                    int r = 0;

                    if (r == 0 && (sorting & Settings.RunAfterPopupSorting.Active) != 0)
                    {
                        r = a.Selected.CompareTo(b.Selected); //(a.RunAfterProcess != null && a.RunAfterProcess.IsActive).CompareTo(b.RunAfterProcess != null && b.RunAfterProcess.IsActive);
                    }

                    if (r == 0 && (sorting & Settings.RunAfterPopupSorting.Type) != 0)
                    {
                        r = a.RunAfter.When.CompareTo(b.RunAfter.When);
                    }

                    if (r == 0 && (sorting & Settings.RunAfterPopupSorting.Name) != 0)
                    {
                        r = a.Text.CompareTo(b.Text);
                    }

                    if (r == 0)
                    {
                        r = a.Index.CompareTo(b.Index);
                    }

                    if (r != 0 && (sorting & Settings.RunAfterPopupSorting.Descending) != 0)
                    {
                        r = -r;
                    }

                    return r;
                });
        }

        private void Sort(SortingAction s, AccountContainerPanel panel)
        {
            panel.Container.SuspendLayout();

            if ((s & SortingAction.Filter) != 0)
            {
                Filter(panel);
            }

            if ((s & SortingAction.Sort) != 0)
            {
                var buttons = panel.Container.Buttons;

                Sort(buttons);

                panel.Container.Buttons = buttons;
            }

            panel.Container.ResumeLayout();
        }

        private Label CreateStatus()
        {
            return new Label()
            {
                Margin = labelStatus.Margin,
                ForeColor = SystemColors.GrayText,
                AutoSize = true,
                Font = labelStatus.Font,
                MinimumSize = labelStatus.MinimumSize,
                TextAlign = labelStatus.TextAlign,
            };
        }

        private Label CreateHeader()
        {
            return new Label()
            {
                Font = labelHeader.Font,
                Padding = labelHeader.Padding,
                Margin = labelHeader.Margin,
                BackColor = labelHeader.BackColor,
                ForeColor = labelHeader.ForeColor,
                AutoSize = true,
                Anchor = AnchorStyles.Left | AnchorStyles.Top | AnchorStyles.Right,
            };
        }

        private RunAfterBarButton CreateButton()
        {
            return new RunAfterBarButton()
            {
                Anchor = AnchorStyles.Left | AnchorStyles.Top | AnchorStyles.Right,
                Margin = new Padding(0),
                BackColor = Util.Color.Lighten(Color.Black, 0.025f),
                BackColorHovered = Util.Color.Lighten(Color.Black, 0.1f),
                BackColorSelected = Util.Color.Darken(Color.SteelBlue, 0.75f),
                Padding = (options & Settings.RunAfterPopupOptions.ShowIcon) == 0 ? buttonTemplate.Padding : new Padding(0, buttonTemplate.Padding.Top, buttonTemplate.Padding.Right, buttonTemplate.Padding.Bottom),
                ForeColor = Util.Color.Darken(Color.White, 0.10f),
                ForeColorHovered = Color.White,
                IconOpacity = 255,
                Visible = false,
                IconVisible = (options & Settings.RunAfterPopupOptions.ShowIcon) != 0,
                TextVisible = true,
                Height = buttonTemplate.Height,
            };
        }

        private bool WasStarted(Client.Launcher.IRunAfterProcess p)
        {
            return p != null && p.WasStarted;
        }

        private bool IsFilteredVisible(RunAfterBarButton b, AccountContainerPanel panel)
        {
            if ((filter & Settings.RunAfterPopupFilter.ManualStartup) != 0)
            {
                if (b.RunAfter.When != Settings.RunAfter.RunAfterWhen.Manual)
                    return false;
            }

            if ((filter & Settings.RunAfterPopupFilter.ProcessActive) != 0)
            {
                if (!b.Selected)
                    return false;
            }

            if ((filter & Settings.RunAfterPopupFilter.ProcessInactive) != 0)
            {
                if (b.Selected)
                    return false;
            }

            if ((filter & Settings.RunAfterPopupFilter.ActiveAccount) != 0)
            {
                if (panel.Process == null)
                    return false;
            }

            if ((filter & Settings.RunAfterPopupFilter.ProcessHasNotStarted) != 0)
            {
                if (WasStarted(b.RunAfterProcess))
                    return false;
            }

            return true;
        }

        private int AddButtons(AccountContainerPanel panel, RunAfterBarButton[] buttons, Settings.IAccount account, IEnumerable<Client.Launcher.IRunAfterProcess> ra, ref int index, ref int visible)
        {
            if (ra == null)
                return 0;

            var c = index;

            foreach (var p in ra)
            {
                var r = p.RunAfterSettings;
                if (!r.Enabled)
                    continue;

                var b = buttons[index];

                if (b == null)
                {
                    buttons[index] = b = CreateButton();

                    b.BackgroundImageChanged += b_BackgroundImageChanged;
                    b.CloseClicked += b_CloseClicked;
                    b.BarClicked += b_BarClicked;
                }
                else
                {
                    b.SelectedChanged -= b_SelectedChanged;
                    b.RunAfterProcess = null;
                }

                b.Text = r.GetName();
                b.Account = account;
                b.RunAfter = r;
                b.Index = index;
                b.RunAfterProcess = p;

                var v = IsFilteredVisible(b, panel);
                if (v)
                    ++visible;

                b.Visible = v;
                b.SelectedChanged += b_SelectedChanged;

                ++index;
            }

            return index - c;
        }

        void b_BackgroundImageChanged(object sender, EventArgs e)
        {
            var b = (RunAfterBarButton)sender;

            if (b.BackgroundImage != null)
                b.Padding = new Padding(0, b.Padding.Top, b.Padding.Right, b.Padding.Bottom);
            else
                b.Padding = buttonTemplate.Padding;
        }
        
        void b_SelectedChanged(object sender, EventArgs e)
        {
            var b = (RunAfterBarButton)sender;
            var s = SortingAction.None;

            b.CloseVisible = b.Selected && (options & Settings.RunAfterPopupOptions.ShowClose) != 0;

            if (b.Selected)
            {
                b.BackColorHovered = Util.Color.Darken(Color.SteelBlue, 0.675f);

                if ((filter & (Settings.RunAfterPopupFilter.ProcessInactive | Settings.RunAfterPopupFilter.ProcessActive | Settings.RunAfterPopupFilter.ProcessHasNotStarted)) != 0)
                    s |= SortingAction.Filter;
            }
            else
            {
                b.BackColorHovered = Util.Color.Lighten(Color.Black, 0.1f);

                if ((filter & (Settings.RunAfterPopupFilter.ProcessInactive | Settings.RunAfterPopupFilter.ProcessActive)) != 0)
                    s |= SortingAction.Filter;
            }

            if ((sorting & Settings.RunAfterPopupSorting.Active) != 0)
                s|= SortingAction.Sort;

            if (s != SortingAction.None)
            {
                var p = FindContainer(b.Parent);

                if (p != null)
                {
                    Util.Invoke.Required(this, delegate
                    {
                        SortDelayed(s, p);
                    });
                }
            }
        }

        private Client.Launcher.IRunAfterManager FindManager(Control c)
        {
            var p = FindContainer(c);
            if (p != null)
                return p.Manager;
            return null;
        }

        private AccountContainerPanel FindContainer(Control c)
        {
            while (c != null && !(c is AccountContainerPanel))
            {
                c = c.Parent;
            }

            return (AccountContainerPanel)c;
        }

        void b_BarClicked(object sender, MouseEventArgs e)
        {
            if (e.Button == System.Windows.Forms.MouseButtons.Right)
            {
                contextMenu.Show(Cursor.Position);
                return;
            }

            var b = (RunAfterBarButton)sender;
            var m = FindManager(b.Parent);

            if (m != null)
            {
                m.Start(b.RunAfterProcess);
            }
        }

        void b_CloseClicked(object sender, MouseEventArgs e)
        {
            var b = (RunAfterBarButton)sender;
            var bp = b.RunAfterProcess;

            if (bp != null)
            {
                var p = bp.Process;
                if (p != null)
                {
                    try
                    {
                        p.Kill();
                    }
                    catch { }
                }
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                Client.Launcher.AccountProcessExited -= Launcher_AccountProcessExited;
                Client.Launcher.RunAfterChanged -= Launcher_RunAfterChanged;
                Client.Launcher.AccountProcessChanged += Launcher_AccountProcessChanged;

                if (components != null)
                    components.Dispose();

                images.Dispose();
            }

            base.Dispose(disposing);
        }

        void Launcher_AccountProcessExited(Settings.IAccount account, System.Diagnostics.Process e)
        {
            if (process != null)
            {
                try
                {
                    if (process.Id == e.Id)
                    {
                        Util.Invoke.Async(this, Dispose);
                    }
                }
                catch { }
            }
        }
        
        private void Filter(AccountContainerPanel p)
        {
            var visible = 0;
            var buttons = p.Container.Buttons;

            if (buttons == null)
                return;

            p.SuspendLayout();
            p.Container.SuspendLayout();

            foreach (var b in buttons)
            {
                var v = IsFilteredVisible(b, p);
                if (v)
                    ++visible;
                b.Visible = v;
            }

            p.Container.VisibleCount = visible;
            p.StatusVisible = visible == 0;

            p.Container.ResumeLayout();
            p.ResumeLayout();
        }

        private void buttonTop_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == System.Windows.Forms.MouseButtons.Left)
            {
                NativeMethods.ReleaseCapture();
                NativeMethods.SendMessage(this.Handle, (uint)WindowMessages.WM_NCLBUTTONDOWN, (IntPtr)HitTest.Caption, IntPtr.Zero);
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

                        if (buttonTop.Bounds.Contains(p))
                        {
                            m.Result = (IntPtr)HitTest.Caption;
                        }
                        else
                        {
                            var b = buttonBottom.Bounds;
                            if (b.Contains(p))
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

                    break;
                default:

                    base.WndProc(ref m);

                    break;
            }
        }

        private void OnMouseClick(object sender, MouseEventArgs e)
        {
            if (e.Button == System.Windows.Forms.MouseButtons.Right)
            {
                contextMenu.Show(Cursor.Position);
            }
        }

        private void closeToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.Dispose();
        }

        private void options_Click(object sender, EventArgs e)
        {
            var flags = Settings.RunAfterPopupOptions.None;
            var m = (ToolStripMenuItem)sender;

            m.Checked ^= true;

            if (keepWindowOpenToolStripMenuItem.Checked)
                flags |= Settings.RunAfterPopupOptions.KeepOpen;
            if (showIconToolStripMenuItem.Checked)
                flags |= Settings.RunAfterPopupOptions.ShowIcon;
            if (showProcessExitButtonToolStripMenuItem.Checked)
                flags |= Settings.RunAfterPopupOptions.ShowClose;

            if (m == showIconToolStripMenuItem || m == showProcessExitButtonToolStripMenuItem)
            {
                var showIcon = (flags & Settings.RunAfterPopupOptions.ShowIcon) != 0;
                var showClose = (flags & Settings.RunAfterPopupOptions.ShowClose) != 0;

                panelContent.SuspendLayout();

                foreach (Control c in panelContent.Controls)
                {
                    if (c is AccountContainerPanel)
                    {
                        var buttons = ((AccountContainerPanel)c).Container.Buttons;
                        if (buttons == null)
                            continue;

                        foreach (var b in buttons)
                        {
                            b.CloseVisible = showClose && b.Selected;
                            if (b.IconVisible != showIcon)
                            {
                                if (showIcon)
                                {
                                    if (b.BackgroundImage != null)
                                        b.Padding = new Padding(0, b.Padding.Top, b.Padding.Right, b.Padding.Bottom);
                                    LoadIconAsync(b);
                                }
                                else
                                    b.Padding = buttonTemplate.Padding;
                                b.IconVisible = showIcon;
                            }
                        }
                    }
                }

                panelContent.ResumeLayout();
            }

            options = flags;
            Settings.RunAfterPopup.Options.Value = flags;
        }

        private void filter_Click(object sender, EventArgs e)
        {
            var flags = Settings.RunAfterPopupFilter.None;
            var m = (ToolStripMenuItem)sender;

            m.Checked ^= true;
            
            if (m.Checked)
            {
                if (m == isActiveToolStripMenuItem)
                    isNotActiveToolStripMenuItem.Checked = false;
                else if (m == isNotActiveToolStripMenuItem)
                    isActiveToolStripMenuItem.Checked = false;
            }

            if (isActiveToolStripMenuItem.Checked)
                flags |= Settings.RunAfterPopupFilter.ProcessActive;
            if (isNotActiveToolStripMenuItem.Checked)
                flags |= Settings.RunAfterPopupFilter.ProcessInactive;
            if (isStartedManuallyToolStripMenuItem.Checked)
                flags |= Settings.RunAfterPopupFilter.ManualStartup;
            if (isAccountActiveToolStripMenuItem.Checked)
                flags |= Settings.RunAfterPopupFilter.ActiveAccount;
            if (hasNotBeenStartedToolStripMenuItem.Checked)
                flags |= Settings.RunAfterPopupFilter.ProcessHasNotStarted;

            filter = flags;
            Settings.RunAfterPopup.Filter.Value = flags;

            SortDelayed(SortingAction.Filter);
        }

        private void sorting_Click(object sender, EventArgs e)
        {
            var flags = Settings.RunAfterPopupSorting.None;
            var m = (ToolStripMenuItem)sender;

            m.Checked ^= true;

            if (nameToolStripMenuItem.Checked)
                flags |= Settings.RunAfterPopupSorting.Name;
            if (activeToolStripMenuItem.Checked)
                flags |= Settings.RunAfterPopupSorting.Active;
            if (typeToolStripMenuItem.Checked)
                flags |= Settings.RunAfterPopupSorting.Type;
            if (descendingToolStripMenuItem.Checked)
                flags |= Settings.RunAfterPopupSorting.Descending;

            sorting = flags;
            Settings.RunAfterPopup.Sorting.Value = sorting;

            SortDelayed(SortingAction.Sort);
        }
    }
}
