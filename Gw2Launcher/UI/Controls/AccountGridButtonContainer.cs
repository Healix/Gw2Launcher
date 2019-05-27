using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using Gw2Launcher.Windows.Native;

namespace Gw2Launcher.UI.Controls
{
    public partial class AccountGridButtonContainer : UserControl, IMessageFilter
    {
        private enum ButtonAction : byte
        {
            None,

            Delayed,
            Pressing,
            Pressed,

            Dragging,

            Selecting,

            Reset,
        }

        private class AccountGridButtonComparer : IComparer<AccountGridButton>
        {
            private Settings.SortMode mode;
            private Settings.SortOrder order;
            private Util.AlphanumericStringComparer stringComparer;

            public AccountGridButtonComparer(Settings.SortMode mode, Settings.SortOrder order)
            {
                this.mode = mode;
                this.order = order;

                switch (mode)
                {
                    case Settings.SortMode.Account:
                    case Settings.SortMode.Name:
                        this.stringComparer = new Util.AlphanumericStringComparer();
                        break;
                }

            }

            public int Compare(AccountGridButton a, AccountGridButton b)
            {
                int result;

                switch (mode)
                {
                    case Settings.SortMode.Account:
                        result = stringComparer.Compare(a.AccountName, b.AccountName);
                        break;
                    case Settings.SortMode.LastUsed:
                        result = a.LastUsedUtc.CompareTo(b.LastUsedUtc);
                        break;
                    case Settings.SortMode.Name:
                        result = stringComparer.Compare(a.DisplayName, b.DisplayName);
                        break;
                    case Settings.SortMode.Custom:
                        ushort ka, kb;
                        if (a.AccountData != null)
                            ka = a.AccountData.SortKey;
                        else
                            ka = ushort.MaxValue;

                        if (b.AccountData != null)
                            kb = b.AccountData.SortKey;
                        else
                            kb = ushort.MaxValue;

                        result = ka.CompareTo(kb);
                        break;
                    case Settings.SortMode.None:
                    default:
                        result = 0;
                        break;
                }

                if (result == 0)
                {
                    ushort ua, ub;
                    if (a.AccountData != null)
                        ua = a.AccountData.UID;
                    else
                        ua = 0;

                    if (b.AccountData != null)
                        ub = b.AccountData.UID;
                    else
                        ub = 0;

                    result = ua.CompareTo(ub);
                }

                if (order == Settings.SortOrder.Ascending)
                    return result;

                return -result;
            }

            public void Clear()
            {
                if (stringComparer != null)
                    stringComparer.Clear();
            }

            public Settings.SortMode Mode
            {
                get
                {
                    return mode;
                }
                set
                {
                    if (mode != value)
                    {
                        mode = value;

                        switch (value)
                        {
                            case Settings.SortMode.Account:
                            case Settings.SortMode.Name:
                                if (this.stringComparer == null)
                                    this.stringComparer = new Util.AlphanumericStringComparer();
                                break;
                            default:
                                this.stringComparer = null;
                                break;
                        }
                    }
                }
            }

            public Settings.SortOrder Order
            {
                get
                {
                    return order;
                }
                set
                {
                    order = value;
                }
            }
        }

        private class BufferedPanel : Panel
        {
            public BufferedPanel()
                : base()
            {
                base.DoubleBuffered = true;
            }
        }
        
        private class SelectionBox : IDisposable
        {
            private class PaintedControl : IDisposable
            {
                public event EventHandler<PaintEventArgs> Paint;

                public enum PaintState : byte
                {
                    None,
                    Partial,
                    Full
                }

                public Control control;
                public PaintState state;

                public int x, y, w, h;

                public PaintedControl(Control c)
                {
                    this.control = c;

                    c.Paint += c_Paint;
                }

                void c_Paint(object sender, PaintEventArgs e)
                {
                    Paint(this, e);
                }

                public void Dispose()
                {
                    this.control.Paint -= c_Paint;
                }
            }

            /// <summary>
            /// Returns all controls that were selected
            /// </summary>
            public event EventHandler<IList<Control>> SelectionComplete;

            /// <summary>
            /// Reports when the selected state of a control changes
            /// </summary>
            public event EventHandler<SelectionChangedEventArgs> SelectionChanged;

            public event EventHandler Disposed;

            public class SelectionChangedEventArgs : EventArgs
            {
                private Control control;
                private bool selected;

                public SelectionChangedEventArgs(Control control, bool selected)
                {
                    this.control = control;
                    this.selected = selected;
                }

                public SelectionChangedEventArgs(IList<Control> selected)
                {
                }

                public Control Control
                {
                    get
                    {
                        return control;
                    }
                }

                public bool Selected
                {
                    get
                    {
                        return selected;
                    }
                }
            }

            private Control container, source;
            private List<PaintedControl> controls;

            private Point p1, p2;
            private Rectangle selection, clip;
            private int offsetX, offsetY, containerX, containerY, containerWidth, containerHeight;

            private Pen pen;
            private SolidBrush brush;

            private bool changed, disposed;

            private SelectionBox(Control container, Size size, Point offset, Control source, Point location)
            {
                this.pen = new Pen(new SolidBrush(Color.FromArgb((int)(255 * 0.75f), SystemColors.Highlight)));
                this.brush = new SolidBrush(Color.FromArgb((int)(255 * 0.25f), SystemColors.Highlight));

                this.source = source;
                
                source.MouseMove += source_MouseMove;
                source.MouseUp += source_MouseUp;
                source.LostFocus += source_LostFocus;

                containerX = source.Left;
                containerY = source.Top;

                while (source != container && source != null)
                {
                    offsetX += source.Left;
                    offsetY += source.Top;

                    source = source.Parent;
                }

                containerX = offset.X;
                containerY = offset.Y;
                containerWidth = size.Width + containerX;
                containerHeight = size.Height + containerY;

                this.container = container;
                this.controls = new List<PaintedControl>(container.Controls.Count);

                AddControls(this.controls, container, 0, 0);

                container.Paint += container_Paint;

                p1 = p2 = new Point(location.X + offsetX, location.Y + offsetY);

                DoUpdate();
            }

            void source_LostFocus(object sender, EventArgs e)
            {
                this.Dispose();
            }

            private void AddControls(List<PaintedControl> controls, Control container, int offsetX, int offsetY)
            {
                foreach (Control c in container.Controls)
                {
                    var pc = new PaintedControl(c)
                    {
                        w = c.Width,
                        h = c.Height,
                        x = offsetX + c.Left,
                        y = offsetY + c.Top
                    };

                    controls.Add(pc);
                    pc.Paint += control_Paint;

                    if (c.HasChildren)
                    {
                        AddControls(controls, c, c.Left + offsetX, c.Top + offsetY);
                    }
                }
            }

            async void DoUpdate()
            {
                while (!disposed)
                {
                    if (changed)
                    {
                        OnChanged();

                        container.Invalidate(clip, false);
                        container.Update();
                    }

                    await Task.Delay(10);
                }
            }

            /// <summary>
            /// Creates a new instance of a selection box
            /// </summary>
            /// <param name="container">The container control where the selection box will be drawn</param>
            /// <param name="size">The maximum size of the available selection area</param>
            /// <param name="offset">The initial offset of the available selection area</param>
            /// <param name="source">The control where the event originated</param>
            /// <param name="location">The location where the box should appear from</param>
            /// <returns></returns>
            public static SelectionBox Create(Control container, Size size, Point offset, Control source, Point location)
            {
                return new SelectionBox(container, size, offset, source, location);
            }

            void source_MouseUp(object sender, MouseEventArgs e)
            {
                if (SelectionComplete != null)
                {
                    List<Control> selected = new List<Control>();

                    foreach (var c in controls)
                    {
                        c.control.Paint -= control_Paint;
                        if (c.state != PaintedControl.PaintState.None)
                        {
                            c.control.Invalidate();
                            selected.Add(c.control);
                        }
                    }

                    SelectionComplete(this, selected);
                }

                this.Dispose();
            }

            void source_MouseMove(object sender, MouseEventArgs e)
            {
                int x = e.Location.X + offsetX,
                    y = e.Location.Y + offsetY;

                if (x < containerX)
                    x = containerX;
                else if (x > containerWidth)
                    x = containerWidth;

                if (y < containerY)
                    y = containerY;
                else if (y > containerHeight)
                    y = containerHeight;

                if (p2.X != x || p2.Y != y)
                {
                    p2 = new Point(x, y);
                    changed = true;
                }
            }

            void OnChanged()
            {
                changed = false;

                int x, y, w, h, r, b, cx, cy, cr, cb, cw, ch;

                x = p1.X;
                y = p1.Y;
                w = p2.X - x;
                h = p2.Y - y;

                if (w < 0)
                {
                    w = -w;
                    x = p2.X;
                }

                if (h < 0)
                {
                    h = -h;
                    y = p2.Y;
                }

                r = x + w;
                b = y + h;

                cx = selection.X;
                cy = selection.Y;
                cr = selection.Right;
                cb = selection.Bottom;

                if (x < cx)
                    cx = x;
                if (y < cy)
                    cy = y;
                if (r > cr)
                    cr = r;
                if (b > cb)
                    cb = b;

                cw = cr - cx;
                ch = cb - cy;

                clip = new Rectangle(cx, cy, cw, ch);
                selection = new Rectangle(x, y, w, h);

                foreach (var pc in controls)
                {
                    var c = pc.control;

                    cr = pc.x + pc.w;
                    cb = pc.y + pc.h;

                    if (r >= pc.x && x <= cr && b >= pc.y && y <= cb)
                    {
                        if (pc.state == PaintedControl.PaintState.None)
                        {
                            if (SelectionChanged != null)
                                SelectionChanged(this, new SelectionChangedEventArgs(pc.control, true));
                        }

                        if (r > cr && x < pc.x && b > cb && y < pc.y)
                        {
                            if (pc.state != PaintedControl.PaintState.Full)
                            {
                                //fully highlighted
                                pc.state = PaintedControl.PaintState.Full;
                                c.Invalidate(false);
                            }
                        }
                        else
                        {
                            //partially highlighted
                            pc.state = PaintedControl.PaintState.Partial;

                            int cx1, cy1, cr1, cb1;

                            cx1 = cx - pc.x;
                            if (cx1 < 0)
                                cx1 = 0;

                            cy1 = cy - pc.y;
                            if (cy1 < 0)
                                cy1 = 0;

                            cr1 = cx + cw - pc.x;
                            if (cr1 > pc.w)
                                cr1 = pc.w;

                            cb1 = cy + ch - pc.y;
                            if (cb1 > pc.h)
                                cb1 = pc.h;

                            c.Invalidate(new Rectangle(cx1, cy1, cr1 - cx1, cb1 - cy1), false);
                        }
                    }
                    else if (pc.state != PaintedControl.PaintState.None)
                    {
                        if (SelectionChanged != null)
                            SelectionChanged(pc.control, new SelectionChangedEventArgs(pc.control, false));

                        //not highlighted
                        pc.state = PaintedControl.PaintState.None;
                        c.Invalidate(false);
                    }
                }
            }

            void container_Paint(object sender, PaintEventArgs e)
            {
                var g = e.Graphics;

                g.SetClip(e.ClipRectangle);
                g.FillRectangle(brush, selection);
                g.DrawRectangle(pen, selection.X, selection.Y, selection.Width - 1, selection.Height - 1);
            }

            void control_Paint(object sender, PaintEventArgs e)
            {
                var c = (PaintedControl)sender;

                int x, y, w, h;
                x = c.x;
                y = c.y;

                if (selection.Right >= x && selection.Bottom >= y && selection.Left <= x + c.w && selection.Top <= y + c.h)
                {
                    var g = e.Graphics;

                    x = selection.X - x;
                    y = selection.Y - y;
                    w = selection.Width;
                    h = selection.Height;

                    g.SetClip(e.ClipRectangle);
                    g.FillRectangle(brush, x, y, w, h);
                    g.DrawRectangle(pen, x, y, w - 1, h - 1);
                }
            }

            public void Dispose()
            {
                if (!disposed)
                {
                    disposed = true;

                    this.container.Paint -= container_Paint;
                    source.MouseMove -= source_MouseMove;
                    source.MouseUp -= source_MouseUp;
                    source.LostFocus -= source_LostFocus;

                    pen.Dispose();
                    brush.Dispose();

                    foreach (var c in controls)
                    {
                        if (c.state != PaintedControl.PaintState.None)
                            c.control.Invalidate();
                        c.Dispose();
                    }

                    this.container.Invalidate();

                    if (Disposed != null)
                        Disposed(this, EventArgs.Empty);
                }
            }
        }

        private class DragHotspots
        {
            public class Hotspot
            {
                public Rectangle bounds;
                public Rectangle target;
                public Settings.AccountSorting.SortType type;
                public bool visible;
            }

            public class Bounds
            {
                public AccountGridButton button;
                public Hotspot[] hotspots;
                public Rectangle bounds;

                public Hotspot Select(int x, int y)
                {
                    foreach (var h in hotspots)
                    {
                        if (h.bounds.Contains(x, y))
                            return h;
                    }

                    return null;
                }
            }

            public Bounds[] bounds;
            public AccountGridButton source;
            public Bounds active;
            public Hotspot activeHotspot;
            public Panel target, highlight;
            public bool entered, cursor;

            public void Select(int x, int y)
            {
                if (activeHotspot != null)
                {
                    if (activeHotspot.bounds.Contains(x, y))
                    {
                        return;
                    }
                    else
                    {
                        if (active.bounds.Contains(x, y))
                        {
                            activeHotspot = active.Select(x, y);
                            if (activeHotspot != null)
                            {
                                Show(activeHotspot);
                                return;
                            }

                            Hide();
                            return;
                        }
                        else
                        {
                            active = null;
                            activeHotspot = null;
                        }
                    }
                }
                else if (active != null)
                {
                    if (active.bounds.Contains(x,y))
                    {
                        activeHotspot = active.Select(x, y);
                        if (activeHotspot != null)
                        {
                            Show(activeHotspot);
                            return;
                        }

                        Hide();
                        return;
                    }
                    else
                    {
                        active = null;
                    }
                }

                foreach (var b in bounds)
                {
                    if (b.bounds.Contains(x, y))
                    {
                        active = b;
                        activeHotspot = b.Select(x, y);
                        if (activeHotspot != null)
                        {
                            Console.WriteLine(b.button.AccountData.Name + ", " + activeHotspot.visible);
                            Show(activeHotspot);
                            return;
                        }

                        break;
                    }
                }

                Hide();
            }

            private void Show(Hotspot h)
            {
                if (target != null)
                {
                    target.Bounds = h.target;
                    target.Visible = h.visible;
                }
            }

            public void Hide()
            {
                active = null;
                if (target != null)
                    target.Visible = false;
            }
        }

        public event EventHandler AddAccountClick;
        public event MouseEventHandler AccountMouseClick;
        public event EventHandler ContentHeightChanged;
        public event EventHandler<HandledMouseEventArgs> AccountBeginDrag;
        public event EventHandler<MousePressedEventArgs> AccountBeginMouseClick;
        public event EventHandler<MousePressedEventArgs> AccountBeginMousePressed;
        public event EventHandler AccountMousePressed;
        public event MouseEventHandler AccountSelection;
        public event EventHandler AccountNoteClicked;

        public class MousePressedEventArgs : MouseEventArgs
        {
            public MousePressedEventArgs(MouseButtons button, int clicks, int x, int y, int delta)
                : base(button, clicks, x, y, delta)
            {

            }

            public bool Handled
            {
                get;
                set;
            }

            public Color FillColor
            {
                get;
                set;
            }

            public Color FlashColor
            {
                get;
                set;
            }
        }

        private List<AccountGridButton> buttons;
        private Size gridSize;
        private int lastSelected = -1;

        private bool isDirty;
        private bool isNewAccountVisible;
        private NewAccountGridButton buttonNewAccount;
        private int returnToScroll;

        private Font fontLarge, fontSmall;
        private bool showAccount, showColor;

        private SelectionBox selection;
        private Rectangle dragBounds;
        private DragHotspots dragspots;
        private CancellationTokenSource cancelPressed;

        private System.Windows.Forms.MouseButtons buttonState;
        private ButtonAction action;

        private AccountGridButtonComparer comparer;

        public AccountGridButtonContainer()
        {
            buttons = new List<AccountGridButton>();

            InitializeComponent();

            this.DoubleBuffered = true;

            buttonState = System.Windows.Forms.MouseButtons.None;

            returnToScroll = -1;
            fontLarge = AccountGridButton.FONT_LARGE;
            fontSmall = AccountGridButton.FONT_SMALL;
            showAccount = true;

            panelContents.Size = Size.Empty;

            isNewAccountVisible = false;

            buttonNewAccount = new NewAccountGridButton();
            buttonNewAccount.Visible = false;
            buttonNewAccount.Click += buttonNewAccount_Click;
            buttonNewAccount.VisibleChanged += buttonNewAccount_VisibleChanged;
            buttonNewAccount.LostFocus += buttonNewAccount_LostFocus;
            panelContents.Controls.Add(buttonNewAccount);

            this.GridSize = new Size(fontLarge.Height * 30 / 2, buttonNewAccount.MinimumSize.Height);
            //this.GridSize = new Size(200, 67);

            Application.AddMessageFilter(this);
            this.Disposed += AccountGridButtonContainer_Disposed;

            panelContents.MouseDown += panelContents_MouseDown;
            this.MouseDown += panelContents_MouseDown;

            panelContents.Paint+=panelContents_Paint;

            isDirty = true;
        }

        void panelContents_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == System.Windows.Forms.MouseButtons.Left || e.Button == System.Windows.Forms.MouseButtons.Right)
                OnBeginSelection(sender, e);
        }

        protected override void OnParentChanged(EventArgs e)
        {
            base.OnParentChanged(e);

            //to include the parent in the draggable area
            //this.Parent.MouseDown += panelContents_MouseDown;
        }

        private void OnBeginSelection(object sender, MouseEventArgs e)
        {
            if (selection != null)
                selection.Dispose();

            Cursor.Current = Cursors.Default;
            ClearSelected();

            Size size;
            if (scrollV.Visible)
                size = panelContents.Size;
            else
                size = this.Size;

            //size = this.Parent.ClientSize; //to include the parent in the draggable area

            selection = SelectionBox.Create(this, size, panelContents.Location, (Control)sender, e.Location);
            selection.SelectionChanged += selection_SelectionChanged;
            selection.Disposed += selection_Disposed;
        }

        void buttonNewAccount_LostFocus(object sender, EventArgs e)
        {
            if (this.ParentForm.ContainsFocus)
            {
                if (isNewAccountVisible)
                    buttonNewAccount.Focus();
            }
            else
            {
                ShowNewAccountButton(false);
            }
        }

        void buttonNewAccount_VisibleChanged(object sender, EventArgs e)
        {
            if (buttonNewAccount.Visible)
                buttonNewAccount.Focus();
        }

        void buttonNewAccount_Click(object sender, EventArgs e)
        {
            if (AddAccountClick != null)
                AddAccountClick(this, EventArgs.Empty);
        }

        void AccountGridButtonContainer_Disposed(object sender, EventArgs e)
        {
            Application.RemoveMessageFilter(this);
        }

        private MouseButtons GetMouseButtons(int wParam)
        {
            MouseButtons buttons = MouseButtons.None;

            if (HasFlag(wParam, 0x0001)) buttons |= MouseButtons.Left;
            if (HasFlag(wParam, 0x0010)) buttons |= MouseButtons.Middle;
            if (HasFlag(wParam, 0x0002)) buttons |= MouseButtons.Right;
            if (HasFlag(wParam, 0x0020)) buttons |= MouseButtons.XButton1;
            if (HasFlag(wParam, 0x0040)) buttons |= MouseButtons.XButton2;
            /*MK_SHIFT=0x0004;MK_CONTROL=0x0008*/

            return buttons;
        }

        private bool HasFlag(int value, int flag)
        {
            return (value & flag) == flag;
        }

        public bool PreFilterMessage(ref Message m)
        {
            switch ((WindowMessages)m.Msg)
            {
                case WindowMessages.WM_MOUSEWHEEL:

                    var pos = new Point(m.LParam.GetValue32());
                    var wParam = m.WParam.GetValue32();
                    var delta = wParam >> 16;
                    var e = new MouseEventArgs(GetMouseButtons(wParam), 0, pos.X, pos.Y, delta);
                    var f = this.ParentForm;

                    if (f.CanFocus && f.DesktopBounds.Contains(pos))
                        OnMsgMouseWheel(e);

                    break;
                case WindowMessages.WM_KEYDOWN:

                    ShowNewAccountButton(false);

                    break;
                case WindowMessages.WM_KEYUP:

                    OnMsgKeyUp(new KeyEventArgs((Keys)m.WParam.GetValue()));

                    break;

                case WindowMessages.WM_SYSKEYDOWN:

                    if (m.WParam == (IntPtr)Keys.Menu)
                    {
                        if (this.ParentForm.ContainsFocus)
                        {
                            if (selection == null)
                                ShowNewAccountButton(true);
                            return true;
                        }
                    }
                    else
                        ShowNewAccountButton(false);

                    break;

                case WindowMessages.WM_SYSKEYUP:

                    if (isNewAccountVisible && m.WParam == (IntPtr)Keys.Menu)
                        ShowNewAccountButton(false);

                    break;

                case WindowMessages.WM_NCLBUTTONDOWN:

                    ShowNewAccountButton(false);

                    break;
            }

            return false;
        }

        private void ShowNewAccountButton(bool visible)
        {
            if (visible)
            {
                if (!isNewAccountVisible)
                {
                    returnToScroll = scrollV.Value;
                    isNewAccountVisible = true;
                    isDirty = true;
                    panelContents.Invalidate();
                }
            }
            else if (isNewAccountVisible)
            {
                isNewAccountVisible = false;
                isDirty = true;
                panelContents.Invalidate();
            }
        }

        void OnMsgKeyUp(KeyEventArgs e)
        {
            if (e.KeyCode == Keys.ShiftKey)
            {
                lastSelected = -1;
            }
        }

        void OnMsgMouseWheel(MouseEventArgs e)
        {
            if (scrollV.Visible)
            {
                int largeChange = (gridSize.Height + 5) / 2;

                if (e.Delta > 0)
                {
                    scrollV.Value -= largeChange;
                }
                else
                {
                    scrollV.Value += largeChange;
                }
            }
        }

        private void AccountGridButtonContainer_Load(object sender, EventArgs e)
        {
        }

        void panelContents_Click(object sender, EventArgs e)
        {
        }

        public Size GridSize
        {
            get
            {
                return this.gridSize;
            }
            set
            {
                this.gridSize = value;
                isDirty = true;
                panelContents.Invalidate();
            }
        }

        public void ClearSelected()
        {
            foreach (var button in buttons)
            {
                if (button.Selected)
                {
                    button.Selected = false;
                    button.Invalidate();
                }
            }

            lastSelected = -1;
        }

        public IList<AccountGridButton> GetSelected()
        {
            List<AccountGridButton> selected = new List<AccountGridButton>();

            foreach (var button in buttons)
            {
                if (button.Selected)
                    selected.Add(button);
            }

            return selected;
        }

        protected override void OnSizeChanged(EventArgs e)
        {
            base.OnSizeChanged(e);
            isDirty = true;
            panelContents.Invalidate();
        }

        public int Count
        {
            get
            {
                bool isNewAccountVisible = buttonNewAccount != null && (buttons.Count == 0 || this.isNewAccountVisible);
                return buttons.Count + (isNewAccountVisible ? 1 : 0);
            }
        }

        public int ContentHeight
        {
            get
            {
                if (this.gridSize.IsEmpty)
                    return 0;

                bool isNewAccountVisible = buttonNewAccount != null && (buttons.Count == 0 || this.isNewAccountVisible);
                int count = buttons.Count + (isNewAccountVisible ? 1 : 0);

                int w = this.Width - 5;
                int columns = w / this.gridSize.Width;
                if (columns < 1)
                    columns = 1;
                int rows = (count - 1) / columns + 1;
                int rowHeight = gridSize.Height + 5;

                return rowHeight * rows + 5;
            }
        }

        public void Sort(Settings.SortMode mode, Settings.SortOrder order)
        {
            if (comparer == null)
            {
                comparer = new AccountGridButtonComparer(mode, order);
            }
            else
            {
                comparer.Mode = mode;
                comparer.Order = order;
            }

            buttons.Sort(comparer);
            comparer.Clear();

            for (int i = 0, count = buttons.Count; i < count; i++)
            {
                buttons[i].Index = i;
            }

            isDirty = true;
            panelContents.Invalidate();
        }

        public void Add(AccountGridButton button)
        {
            button.FontLarge = fontLarge;
            button.FontSmall = fontSmall;
            button.ShowAccount = showAccount;
            button.ShowColorKey = showColor;

            panelContents.Controls.Add(button);
            button.Index = buttons.Count;
            buttons.Add(button);
            isDirty = true;
            panelContents.Invalidate();

            button.MouseClick += button_MouseClick;
            button.MouseDown += button_MouseDown;
            button.MouseUp += button_MouseUp;
            button.NoteClicked += button_NoteClicked;
        }

        private CancellationTokenSource GetPressedCancelToken()
        {
            if (cancelPressed != null)
            {
                if (cancelPressed.IsCancellationRequested)
                {
                    cancelPressed.Dispose();
                    return cancelPressed = new CancellationTokenSource();
                }

                if (action != ButtonAction.None)
                {
                    using (cancelPressed)
                    {
                        cancelPressed.Cancel();
                        return cancelPressed = new CancellationTokenSource();
                    }
                }

                return cancelPressed;
            }

            return cancelPressed = new CancellationTokenSource();
        }

        void button_MouseDown(object sender, MouseEventArgs e)
        {
            if (buttonState != System.Windows.Forms.MouseButtons.None)
                return;
            buttonState = e.Button;

            SetAction(ButtonAction.None);

            var c = (Control)sender;

            if (buttonState == System.Windows.Forms.MouseButtons.Left || buttonState == System.Windows.Forms.MouseButtons.Right)
            {
                c.MouseMove += button_MouseMove;

                var size = SystemInformation.DragSize;
                dragBounds = new Rectangle(e.X - size.Width / 2, e.Y - size.Height / 2, size.Width, size.Height);

                if (buttonState == System.Windows.Forms.MouseButtons.Left && !ModifierKeys.HasFlag(Keys.Control))
                    DelayedPressed((AccountGridButton)c, e, GetPressedCancelToken().Token);
            }
        }

        async void DelayedPressed(AccountGridButton button, MouseEventArgs e, CancellationToken cancel)
        {
            SetAction(ButtonAction.Delayed);

            try
            {
                await Task.Delay(100, cancel);
            }
            catch
            {
                return;
            }

            if (!cancel.IsCancellationRequested)
            {
                if (AccountBeginMousePressed != null)
                {
                    var mp = new MousePressedEventArgs(buttonState, e.Clicks, e.X, e.Y, e.Delta);
                    AccountBeginMousePressed(button, mp);

                    if (mp.Handled)
                    {
                        button.PressedProgress += button_PressedProgress;
                        button.EndPressed += button_EndPressed;
                        button.Pressed += button_Pressed;

                        button.BeginPressed(0, cancel, mp);
                    }
                    else
                    {
                        SetAction(ButtonAction.None);
                    }
                }
            }
        }

        private void SetAction(ButtonAction action)
        {
            switch (this.action)
            {
                case ButtonAction.Delayed:
                case ButtonAction.Pressing:
                case ButtonAction.Pressed:

                    switch (action)
                    {
                        case ButtonAction.Pressing:
                        case ButtonAction.Pressed:
                            break;
                        default:

                            if (cancelPressed != null)
                            {
                                using (cancelPressed)
                                {
                                    cancelPressed.Cancel();
                                    cancelPressed = null;
                                }
                            }

                            break;
                    }

                    break;
            }

            this.action = action;
        }

        void button_Pressed(object sender, EventArgs e)
        {
            var b = (AccountGridButton)sender;
            b.MouseMove -= button_MouseMove;

            SetAction(ButtonAction.Pressed);

            if (AccountMousePressed != null)
                AccountMousePressed(sender, e);
        }

        void button_EndPressed(object sender, bool cancelled)
        {
            var b = (AccountGridButton)sender;
            b.EndPressed -= button_EndPressed;
            b.Pressed -= button_Pressed;
            b.PressedProgress -= button_PressedProgress;

            if (!cancelled)
                action = ButtonAction.Reset;
        }

        void button_PressedProgress(object sender, float e)
        {
            if (e > 0.25f)
            {
                var b = (AccountGridButton)sender;
                b.PressedProgress -= button_PressedProgress;

                if (action == ButtonAction.Delayed)
                    SetAction(ButtonAction.Pressing);
            }
        }

        void pressed_MouseDown(object sender, MouseEventArgs e)
        {
            var b = (AccountGridButton)sender;
            b.MouseDown -= pressed_MouseDown;
            b.MouseClick += button_MouseClick;
        }

        void button_MouseUp(object sender, MouseEventArgs e)
        {
            var b = (AccountGridButton)sender;
            b.MouseMove -= button_MouseMove;

            buttonState = System.Windows.Forms.MouseButtons.None;

            switch (action)
            {
                case ButtonAction.Delayed:
                case ButtonAction.Pressing:

                    SetAction(ButtonAction.None);

                    break;
                case ButtonAction.Selecting:

                    if (AccountSelection != null)
                    {
                        var p = this.PointToClient(Cursor.Position);
                        if (!this.DisplayRectangle.Contains(p))
                            return;

                        p.Y -= panelContents.Top;
                        
                        var selected = false;

                        if (b.Bounds.Contains(p))
                        {
                            if (AccountMouseClick != null)
                                AccountMouseClick(b, e);
                        }
                        else if (b.Selected)
                        {
                            selected = true;
                        }
                        else
                        {
                            foreach (var button in buttons)
                            {
                                if (button.Selected)
                                {
                                    selected = true;
                                    break;
                                }
                            }
                        }

                        if (selected)
                            AccountSelection(b, e);
                    }
                    else if (AccountMouseClick != null)
                    {
                        var p = this.PointToClient(Cursor.Position);
                        if (!this.DisplayRectangle.Contains(p))
                            return;

                        p.Y -= panelContents.Top;

                        if (!b.Bounds.Contains(p))
                        {
                            b = null;
                            foreach (var button in buttons)
                            {
                                if (button.Bounds.Contains(p))
                                {
                                    b = button;
                                    break;
                                }
                            }
                        }

                        if (b != null)
                            AccountMouseClick(b, e);
                    }

                    break;
            }
        }

        void button_MouseMove(object sender, MouseEventArgs e)
        {
            if (!dragBounds.Contains(e.Location))
            {
                var c = (Control)sender;
                c.MouseMove -= button_MouseMove;

                if (buttonState == System.Windows.Forms.MouseButtons.Right || ModifierKeys.HasFlag(Keys.Control))
                {
                    SetAction(ButtonAction.Selecting);

                    OnBeginSelection(sender, e);
                }
                else
                {
                    SetAction(ButtonAction.Dragging);

                    if (AccountBeginDrag != null)
                    {
                        var button = (AccountGridButton)sender;
                        try
                        {
                            if (comparer != null && comparer.Mode == Settings.SortMode.Custom)
                            {
                                InitializeDragHotspots(button);

                                this.AllowDrop = true;
                                button.GiveFeedback += AccountGridButtonContainer_GiveFeedback;
                            }

                            var he = new HandledMouseEventArgs(e.Button, e.Clicks, e.X, e.Y, e.Delta, false);
                            AccountBeginDrag(sender, he);
                            if (he.Handled)
                                buttonState = System.Windows.Forms.MouseButtons.None; //dragging causes the mouse events to be lost
                        }
                        finally
                        {
                            if (dragspots != null)
                            {
                                if (dragspots.target != null)
                                {
                                    panelContents.Controls.Remove(dragspots.target);
                                    dragspots.target.Dispose();
                                }
                                if (dragspots.highlight != null)
                                {
                                    panelContents.Controls.Remove(dragspots.highlight);
                                    dragspots.highlight.Dispose();
                                }
                                dragspots = null;

                                this.AllowDrop = false;
                                button.GiveFeedback -= AccountGridButtonContainer_GiveFeedback;
                            }
                        }
                    }
                }
            }
        }

        void AccountGridButtonContainer_GiveFeedback(object sender, GiveFeedbackEventArgs e)
        {
            if (dragspots.entered)
            {
                e.UseDefaultCursors = false;

                if (!dragspots.cursor)
                {
                    Cursor.Current = Cursors.Arrow;
                    dragspots.cursor = true;
                }
            }
        }

        private void InitializeDragHotspots(AccountGridButton source)
        {
            var count = buttons.Count;

            const int PADDING = 5;
            const int PANEL_SIZE = 3;
            const int PANEL_SPACING = (PADDING - PANEL_SIZE) / 2;

            int wContainer = this.Width - PADDING;
            if (scrollV.Visible)
                wContainer -= scrollV.Width;
            int columns = wContainer / this.gridSize.Width;
            if (columns < 1)
                columns = 1;
            int rows = (count - 1) / columns + 1;

            dragspots = new DragHotspots();
            dragspots.source = source;
            dragspots.bounds = new DragHotspots.Bounds[count];

            int index = 0;
            var offset = panelContents.PointToScreen(Point.Empty);
            var reverse = comparer != null && comparer.Order == Settings.SortOrder.Descending;
            var sourceIndex = -1;

            dragspots.highlight = new Panel()
            {
                BackColor = Color.Gray,
                Visible = false,
            };

            for (int i = 0; i < count; i++)
            {
                var b = buttons[i];

                int w = b.Width,
                    h = b.Height,
                    x = b.Left,
                    y = b.Top,
                    column = i % columns,
                    row = i / columns;

                var visible = b != source;
                if (!visible)
                {
                    sourceIndex = i;
                    dragspots.highlight.Bounds = new Rectangle(x - PANEL_SIZE + 1, y - PANEL_SIZE + 1, w + (PANEL_SIZE-1) * 2, h + (PANEL_SIZE-1) * 2);
                    dragspots.highlight.Visible = true;
                }

                int left = x - PADDING / 2 + offset.X,
                    top = y - PADDING / 2 + offset.Y,
                    width = w + PADDING,
                    height = h + PADDING;

                var db = dragspots.bounds[index++] = new DragHotspots.Bounds()
                {
                    bounds = new Rectangle(left, top, width, height),
                    button = b,
                };

                if (columns == 1)
                {
                    //vertical layout, drop indicator will be shown above/below

                    db.hotspots = new DragHotspots.Hotspot[]
                    {
                        new DragHotspots.Hotspot()
                        {
                            bounds = new Rectangle(left,top + height / 4,width,height / 2),
                            target = new Rectangle(x - PANEL_SIZE, y - PANEL_SIZE, w + PANEL_SIZE*2, h+PANEL_SIZE*2),
                            type = Settings.AccountSorting.SortType.Swap,
                            visible = visible,
                        },
                        new DragHotspots.Hotspot()
                        {
                            bounds = new Rectangle(left,top,width,height / 4),
                            target = new Rectangle(x,y - PANEL_SIZE - PANEL_SPACING,w,PANEL_SIZE),
                            type = reverse ? Settings.AccountSorting.SortType.After : Settings.AccountSorting.SortType.Before,
                            visible = visible,
                        },
                        new DragHotspots.Hotspot()
                        {
                            bounds = new Rectangle(left,top + height * 3 / 4,width,height / 4),
                            target = new Rectangle(x,y + h + PANEL_SPACING,w,PANEL_SIZE),
                            type = reverse ? Settings.AccountSorting.SortType.Before : Settings.AccountSorting.SortType.After,
                            visible = visible,
                        }
                    };
                }
                else
                {
                    //horizontal layout, drop indicator will be shown to the sides

                    db.hotspots = new DragHotspots.Hotspot[]
                    {
                        new DragHotspots.Hotspot()
                        {
                            bounds = new Rectangle(left + width / 4,top,width / 2,height),
                            target = new Rectangle(x - PANEL_SIZE, y - PANEL_SIZE, w + PANEL_SIZE*2, h+PANEL_SIZE*2),
                            type = Settings.AccountSorting.SortType.Swap,
                            visible = visible,
                        },
                        new DragHotspots.Hotspot()
                        {
                            bounds = new Rectangle(left,top,width / 4,height),
                            target = new Rectangle(x - PANEL_SIZE - PANEL_SPACING,y,PANEL_SIZE,h),
                            type = reverse ? Settings.AccountSorting.SortType.After : Settings.AccountSorting.SortType.Before,
                            visible = visible,
                        },
                        new DragHotspots.Hotspot()
                        {
                            bounds = new Rectangle(left + width * 3 / 4,top,width / 4,height),
                            target = new Rectangle(x + w + PANEL_SPACING,y,PANEL_SIZE,h),
                            type = reverse ? Settings.AccountSorting.SortType.Before : Settings.AccountSorting.SortType.After,
                            visible = visible,
                        }
                    };
                }
            }

            if (sourceIndex != -1)
            {
                if (sourceIndex > 0)
                {
                    dragspots.bounds[sourceIndex - 1].hotspots[2].visible = false;
                }
                if (sourceIndex < count - 1)
                    dragspots.bounds[sourceIndex + 1].hotspots[1].visible = false;
            }

            dragspots.target = new Panel()
            {
                BackColor = Color.Black,
                Visible = false,
            };

            panelContents.Controls.AddRange(new Control[] { dragspots.target, dragspots.highlight });
        }

        protected override void OnDragEnter(DragEventArgs drgevent)
        {
            base.OnDragEnter(drgevent);

            if (dragspots != null)
            {
                dragspots.entered = true;
                drgevent.Effect = DragDropEffects.Copy;
            }
        }

        protected override void OnDragOver(DragEventArgs drgevent)
        {
            base.OnDragOver(drgevent);

            if (dragspots != null)
                dragspots.Select(drgevent.X, drgevent.Y);
        }

        protected override void OnDragLeave(EventArgs e)
        {
            base.OnDragLeave(e);

            if (dragspots != null)
            {
                dragspots.entered = false;
                dragspots.cursor = false;
                dragspots.Hide();
            }
        }

        protected override void OnDragDrop(DragEventArgs drgevent)
        {
            base.OnDragDrop(drgevent);

            if (dragspots != null)
            {
                if (dragspots.activeHotspot != null)
                {
                    var b = dragspots.active.button;
                    if (b != dragspots.source)
                    {
                        var from = dragspots.source.AccountData;
                        var to = b.AccountData;

                        if (from != null && to != null)
                            from.Sort(to, dragspots.activeHotspot.type);
                    }

                    dragspots.Hide();
                }
            }
        }

        public bool AllowCustomOrdering
        {
            get;
            set;
        }

        void selection_MouseUp(object sender, MouseEventArgs e)
        {
            var b = (AccountGridButton)sender;
            b.MouseUp -= selection_MouseUp;

            if (AccountMouseClick != null)
            {
                var p = panelContents.PointToClient(Cursor.Position);
                if (!panelContents.Bounds.Contains(p))
                    return;

                if (!b.Bounds.Contains(p))
                {
                    b = null;
                    foreach (var button in buttons)
                    {
                        if (button.Bounds.Contains(p))
                        {
                            b = button;
                            break;
                        }
                    }
                }

                if (b != null)
                    AccountMouseClick(b, e);
            }
        }

        void selection_SelectionChanged(object sender, AccountGridButtonContainer.SelectionBox.SelectionChangedEventArgs e)
        {
            if (e.Control.GetType() == typeof(AccountGridButton))
            {
                var button = ((AccountGridButton)e.Control);
                button.Selected = e.Selected;
            }
        }

        void selection_Disposed(object sender, EventArgs e)
        {
            selection = null;
        }

        void button_NoteClicked(object sender, EventArgs e)
        {
            if (AccountNoteClicked != null)
                AccountNoteClicked(sender, e);
        }

        void button_MouseClick(object sender, MouseEventArgs e)
        {
            var button = (AccountGridButton)sender;
            int index = button.Index;

            switch (action)
            {
                case ButtonAction.Dragging:
                case ButtonAction.Pressing:
                case ButtonAction.Pressed:
                case ButtonAction.Selecting:
                case ButtonAction.Reset:
                    return;
            }

            if (ModifierKeys.HasFlag(Keys.Control))
            {
                if (selection == null)
                {
                    button.Selected = !button.Selected;
                    lastSelected = index;
                }
            }
            else if (ModifierKeys.HasFlag(Keys.Shift))
            {
                if (lastSelected == -1)
                {
                    for (int i = 0; i < buttons.Count; i++)
                    {
                        button=buttons[i];
                        button.Selected = i == index;
                    }
                    lastSelected = index;
                }
                else
                {
                    for (int i = 0; i < buttons.Count;i++)
                    {
                        button=buttons[i];
                        if (lastSelected > index)
                            button.Selected = (i >= index && i <= lastSelected);
                        else
                            button.Selected = (i >= lastSelected && i <= index);
                    }
                }
            }
            else
            {
                if (AccountBeginMouseClick != null)
                {
                    SetAction(ButtonAction.None);

                    var mp = new MousePressedEventArgs(buttonState, e.Clicks, e.X, e.Y, e.Delta);
                    AccountBeginMouseClick(button, mp);

                    if (mp.Handled)
                    {
                        SetAction(ButtonAction.Pressed);
                        button.EndPressed += button_EndPressed;
                        button.BeginPressed(1, GetPressedCancelToken().Token, mp);
                    }
                }

                if (AccountMouseClick != null)
                    AccountMouseClick(sender, e);
            }
        }

        public void Remove(AccountGridButton button)
        {
            if (buttons.Remove(button))
            {
                panelContents.Controls.Remove(button);
                isDirty = true;
                panelContents.Invalidate();
            }
        }

        protected override void OnPaintBackground(PaintEventArgs e)
        {
            if (isDirty)
            {
                ArrangeGrid();
            }

            base.OnPaintBackground(e);
        }

        public void ArrangeGrid()
        {
            isDirty = false;

            if (this.gridSize.IsEmpty)
                return;

            bool isNewAccountVisible = buttonNewAccount != null && (buttons.Count == 0 || this.isNewAccountVisible);
            int count = buttons.Count + (isNewAccountVisible ? 1 : 0);

            buttonNewAccount.Visible = isNewAccountVisible;

            int w = this.Width - 5;
            int columns = w / this.gridSize.Width;
            if (columns < 1)
                columns = 1;
            int columnWidth = w / columns;
            int rows = (count - 1) / columns + 1;
            int rowHeight = gridSize.Height + 5;

            int panelWidth = columnWidth * columns + 5;
            int panelHeight = rowHeight * rows + 5;

            if (ContentHeightChanged != null && panelContents.Height != panelHeight)
                ContentHeightChanged(this, EventArgs.Empty);

            if (panelHeight > this.Height)
            {
                scrollV.Visible = true;

                w -= scrollV.Width;
                columns = w / this.gridSize.Width;
                if (columns < 1)
                    columns = 1;
                columnWidth = w / columns;
                rows = (count - 1) / columns + 1;

                panelWidth = columnWidth * columns + 5;
                panelHeight = rowHeight * rows + 5;

                scrollV.Maximum = panelHeight - this.Height;

                if (returnToScroll != -1)
                {
                    if (isNewAccountVisible)
                        scrollV.Value = scrollV.Maximum;
                    else
                    {
                        if (returnToScroll > scrollV.Maximum)
                            scrollV.Value = scrollV.Maximum;
                        else
                            scrollV.Value = returnToScroll;

                        returnToScroll = -1;
                    }
                }
            }
            else
            {
                scrollV.Visible = false;
                scrollV.Value = 0;
            }

            panelContents.Size = new Size(panelWidth, panelHeight);

            for (int i = 0; i < count; i++)
            {
                int column = i % columns;
                int row = i / columns;

                AccountGridButton b;
                if (i == count - 1 && isNewAccountVisible)
                    b = buttonNewAccount;
                else
                    b = buttons[i];

                b.Size = new Size(columnWidth - 5, rowHeight - 5);
                b.Location = new Point(columnWidth * column + 5, rowHeight * row + 5);
            }
        }

        private void panelContents_Paint(object sender, PaintEventArgs e)
        {
            if (isDirty)
            {
                ArrangeGrid();
            }
        }

        private void verticalScroll_Scroll(object sender, ScrollEventArgs e)
        {
        }

        private void verticalScroll_ValueChanged(object sender, EventArgs e)
        {
            panelContents.Location = new Point(0, -verticalScroll.Value);

            if (isNewAccountVisible && returnToScroll != -1 && verticalScroll.Value < verticalScroll.Maximum)
            {
                returnToScroll = -1;
            }
        }

        public void SetStyle(Font fontLarge, Font fontSmall, bool showAccount, bool showColor)
        {
            if (this.fontLarge == fontLarge && this.fontSmall == fontSmall && this.showAccount == showAccount && this.showColor == showColor && this.IsHandleCreated)
                return;

            this.fontLarge = fontLarge;
            this.fontSmall = fontSmall;
            this.showAccount = showAccount;
            this.showColor = showColor;

            if (buttons.Count > 0)
            {
                foreach (var b in buttons)
                {
                    b.FontLarge = fontLarge;
                    b.FontSmall = fontSmall;
                    b.ShowAccount = showAccount;
                    b.ShowColorKey = showColor;
                }
            }

            buttonNewAccount.FontLarge = fontLarge;
            buttonNewAccount.FontSmall = fontSmall;
            buttonNewAccount.ShowAccount = showAccount;
            buttonNewAccount.ShowColorKey = false;
            buttonNewAccount.ResizeLabels();

            this.GridSize = new Size(fontLarge.Height * 30 / 2, buttonNewAccount.MinimumSize.Height);
        }

        private void scrollV_ValueChanged(object sender, int e)
        {
            panelContents.Location = new Point(0, -e);

            if (isNewAccountVisible && returnToScroll != -1 && scrollV.Value < scrollV.Maximum)
            {
                returnToScroll = -1;
            }
        }
    }
}
