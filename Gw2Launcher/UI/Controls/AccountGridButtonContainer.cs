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
using System.Windows.Forms.Layout;
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

        private class AccountGridButtonComparer : Util.AccountComparer, IComparer<AccountGridButton>
        {
            public AccountGridButtonComparer(Settings.SortingOptions options)
                : base(options)
            {
            }

            public int Compare(AccountGridButton a, AccountGridButton b)
            {
                int result;

                if (a.Pinned != b.Pinned)
                {
                    if (a.Pinned)
                        return -1;
                    else
                        return 1;
                }

                if (groupingEnabled)
                {
                    result = 0;

                    if ((grouping & Settings.GroupMode.Active) == Settings.GroupMode.Active)
                    {
                        var b1 = a.AccountData != null ? Client.Launcher.IsActive(a.AccountData) : false;
                        var b2 = b.AccountData != null ? Client.Launcher.IsActive(b.AccountData) : false;

                        result = -b1.CompareTo(b2);
                    }

                    if (result == 0 && (grouping & Settings.GroupMode.Type) == Settings.GroupMode.Type)
                    {
                        result = a.AccountType.CompareTo(b.AccountType);
                    }

                    if (result != 0)
                    {
                        if (groupingReversed)
                            return -result;
                        return result;
                    }
                }

                switch (sorting)
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
                    case Settings.SortMode.CustomGrid:
                    case Settings.SortMode.CustomList:

                        ushort ka, kb;
                        ka = a.SortKey;
                        kb = b.SortKey;
                        //if (a.AccountData != null)
                        //    ka = a.AccountData.SortKey;
                        //else
                        //    ka = ushort.MaxValue;

                        //if (b.AccountData != null)
                        //    kb = b.AccountData.SortKey;
                        //else
                        //    kb = ushort.MaxValue;

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

                if (sortingReversed)
                    return -result;

                return result;
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

            private bool changed, disposed, scaled;

            private SelectionBox(Control container, Size size, Point offset, Control source, Point location)
            {
                this.pen = new Pen(new SolidBrush(Color.FromArgb((int)(255 * 0.75f), UiColors.GetColor(UiColors.Colors.AccountSelectionHighlight))));
                this.pen.Alignment = System.Drawing.Drawing2D.PenAlignment.Inset;
                this.brush = new SolidBrush(Color.FromArgb((int)(255 * 0.25f), UiColors.GetColor(UiColors.Colors.AccountSelectionHighlight)));

                using (var g = container.CreateGraphics())
                {
                    this.pen.Width = (int)(g.DpiX / 96f + 0.5f);
                }

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

                    if (!c.Visible)
                        continue;

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
                var w = selection.Width;
                var h = selection.Height;
                if (pen.Width < 1.5f)
                {
                    --w;
                    --h;
                }
                g.DrawRectangle(pen, selection.X, selection.Y, w, h);
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
                    if (pen.Width < 1.5f)
                    {
                        --w;
                        --h;
                    }
                    g.DrawRectangle(pen, x, y, w, h);
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

            public float BorderSize
            {
                get
                {
                    return pen.Width;
                }
                set
                {
                    pen.Width = value;
                }
            }

            public Rectangle SelectionBounds
            {
                get
                {
                    return selection;
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
                public Control control;
                public Hotspot[] hotspots;
                public Rectangle bounds;
                public int index;

                public Hotspot Select(int x, int y)
                {
                    foreach (var h in hotspots)
                    {
                        if (h.visible && h.bounds.Contains(x, y))
                            return h;
                    }

                    return null;
                }
            }

            public Bounds[] bounds;
            public AccountGridButton source;
            public Bounds active, sourceb;
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
                activeHotspot = null;
                if (target != null)
                    target.Visible = false;
            }
        }

        private class AccountGridButtonContainerLayout : LayoutEngine
        {
            public override bool Layout(object container, LayoutEventArgs args)
            {
                var c = (AccountGridButtonContainer)container;
                var s = DoLayout(c, c.ClientSize, true, true);

                return false;
            }

            public Size DoLayout(AccountGridButtonContainer panel, Size proposed, bool apply, bool measure)
            {
                var panelContents = panel.panelContents;
                var scrollV = panel.scrollV;
                var height = panel.ClientSize.Height;

                scrollV.SetBounds(panel.ClientSize.Width - scrollV.Width, 0, scrollV.Width, height);

                while (true)
                {
                    var ch = panelContents.ContentHeight;
                    var h = ch - height;
                    Size size;

                    if (h > 0)
                    {
                        scrollV.Visible = true;
                        scrollV.Maximum = h;

                        size = new Size(scrollV.Left, ch);

                        if (panel.returnToScroll != -1)
                        {
                            if (panelContents.NewButtonVisible)
                            {
                                scrollV.Value = scrollV.Maximum;
                            }
                            else
                            {
                                if (panel.returnToScroll > scrollV.Maximum)
                                    scrollV.Value = scrollV.Maximum;
                                else
                                    scrollV.Value = panel.returnToScroll;

                                panel.returnToScroll = -1;
                            }
                        }
                    }
                    else
                    {
                        scrollV.Visible = false;
                        scrollV.Value = 0;

                        size = panel.ClientSize; // new Size(panel.ClientSize.Width, panelContents.ContentHeight); //filling height to support custom dragging anywhere
                    }

                    if (panel.gridColumnsAuto)
                        panel.AutoColumns(size.Width);

                    panelContents.ClientSize = size;

                    if (panelContents.ContentHeight != ch)
                    {
                        continue;
                    }
                    else
                    {
                        break;
                    }
                }

                return panel.Size;
            }
        }

        private class SearchFilter
        {
            private bool[] types;
            private string[] text;
            private int[] numbers;

            public SearchFilter()
                : this(0, null)
            {
            }

            public SearchFilter(byte page, string filter)
            {
                Page = page;
                Filter = filter;
            }

            private string _Filter;
            public string Filter
            {
                get
                {
                    return _Filter;
                }
                set
                {
                    if (_Filter == value)
                        return;

                    _Filter = value;
                    _DailyLogin = false;

                    if (value == null)
                    {
                        this.types = null;
                        this.text = null;
                        this.numbers = null;
                    }
                    else
                    {
                        var filter = value.ToLower();

                        int i = 0,
                            l = filter.Length;
                        var items = new List<string>();
                        List<int> numbers = null;
                        bool[] types = null;

                        while (i < l)
                        {
                            while (filter[i] == ' ')
                            {
                                if (++i == l)
                                    break;
                            }
                            if (i == l)
                                break;

                            int j;
                            var quoted = false;

                            if (filter[i] == '"')
                            {
                                j = filter.IndexOf('"', i + 1);
                                if (j == -1)
                                    j = filter.IndexOf(' ', i + 1);
                                else
                                    quoted = true;
                            }
                            else
                            {
                                j = filter.IndexOf(' ', i + 1);
                            }

                            string f;

                            if (j == -1)
                            {
                                j = filter.Length;
                                if (i == 0)
                                    f = filter;
                                else
                                    f = filter.Substring(i, j - i);
                            }
                            else if (quoted)
                            {
                                f = filter.Substring(i + 1, j - i - 1);
                            }
                            else
                            {
                                f = filter.Substring(i, j - i);
                            }

                            switch (f)
                            {
                                case "gw1":

                                    if (types == null)
                                        types = new bool[2];
                                    types[(byte)Settings.AccountType.GuildWars1] = true;

                                    break;
                                case "gw2":

                                    if (types == null)
                                        types = new bool[2];
                                    types[(byte)Settings.AccountType.GuildWars2] = true;

                                    break;
                                case "daily":
                                case "login":

                                    _DailyLogin = true;

                                    break;
                                default:

                                    items.Add(f);

                                    if (!quoted && char.IsDigit(f[0]))
                                    {
                                        int n;
                                        if (int.TryParse(f, out n))
                                        {
                                            if (numbers == null)
                                                numbers = new List<int>();
                                            numbers.Add(n);
                                        }
                                    }

                                    break;
                            }

                            i = j + 1;
                        }

                        if (items.Count > 0)
                            this.text = items.ToArray();
                        else
                            this.text = null;

                        if (numbers != null)
                            this.numbers = numbers.ToArray();
                        else
                            this.numbers = null;

                        this.types = types;
                    }
                }
            }

            private byte _Page;
            public byte Page
            {
                get
                {
                    return _Page;
                }
                set
                {
                    _Page = value;
                }
            }

            private bool _DailyLogin;
            public bool DailyLogin
            {
                get
                {
                    return _DailyLogin;
                }
                set
                {
                    _DailyLogin = value;
                }
            }

            public bool HasTextFilter
            {
                get
                {
                    return text != null;
                }
            }

            public bool HasNumberFilter
            {
                get
                {
                    return numbers != null;
                }
            }

            public bool MatchDailyLogin(AccountGridButton button)
            {
                if (_DailyLogin)
                {
                    return button.ShowDailyLogin && button.LastDailyLoginUtc.Date != DateTime.UtcNow.Date;
                }

                return true;
            }

            public bool MatchPage(AccountGridButton button)
            {
                if (_Page > 0)
                {
                    if (button.Paging == null || button.Paging.Page == _Page && button.Paging.Current == null)
                    {
                        return false;
                    }
                    else
                    {
                        return button.Paging.SetCurrent(_Page);
                    }
                }
                else if (button.Paging != null && button.Paging.Page != 0)
                {
                    button.Paging.Current = null;
                }

                return true;
            }

            public bool MatchFilters(AccountGridButton button)
            {
                if (types != null && !types[(byte)button.AccountType])
                    return false;

                return (numbers != null && MatchNumbers(button) || MatchText(button)) && MatchDailyLogin(button);
            }

            public bool MatchNumbers(AccountGridButton button)
            {
                if (numbers == null)
                    return true;

                foreach (var i in numbers)
                {
                    if (i == button.DailyLoginDay)
                    {
                        if (i > 0 && button.ShowDailyLogin && button.LastDailyLoginUtc.Date != DateTime.UtcNow.Date)
                            return true;
                    }
                }

                return false;
            }

            public bool MatchText(AccountGridButton button)
            {
                if (text == null)
                    return true;

                var name = button.DisplayName;
                var account = button.AccountName;

                if (name != null)
                    name = name.ToLower();
                if (account != null)
                    account = account.ToLower();

                foreach (var t in text)
                {
                    if (name != null && name.IndexOf(t, StringComparison.Ordinal) != -1 
                        || account != null && account.Equals(t, StringComparison.Ordinal))
                    {
                        return true;
                    }
                }

                return false;
            }

            public bool MatchAll(AccountGridButton button)
            {
                return MatchPage(button) && MatchFilters(button);
            }

            public bool IsActive
            {
                get
                {
                    return _Page != 0 || HasFilters;
                }
            }

            public bool HasFilters
            {
                get
                {
                    return _Filter != null || _DailyLogin;
                }
            }
        }

        public struct Style : IEquatable<Style>
        {
            public Font FontName, FontStatus, FontUser;
            public bool ShowAccount, ShowColor, ShowIcon, ShowActiveHighlight;
            public UiColors.ColorValues Colors;
            public Settings.AccountGridButtonOffsets Offsets;
            public Settings.DailyLoginDayIconFlags ShowDailyLoginDay;
            public AccountGridButton.Icons.DisplayOrder IconDisplayOrder;

            public void Apply(AccountGridButton b)
            {
                b.FontName = FontName;
                b.FontStatus = FontStatus;
                b.ShowAccount = ShowAccount;
                b.ShowColorKey = ShowColor;
                b.ShowImage = ShowIcon || b.Image != null;
                b.Colors = Colors;
                b.Offsets = Offsets;
                b.IsActiveHighlight = ShowActiveHighlight;
                b.ShowDailyLoginDay = b.AccountType == Settings.AccountType.GuildWars2 ? ShowDailyLoginDay : Settings.DailyLoginDayIconFlags.None;
                b.SetOrder(IconDisplayOrder);
            }

            public bool Equals(Style s)
            {
                return s.FontName == FontName
                    && s.FontStatus == FontStatus
                    && s.FontUser == FontUser

                    && s.ShowAccount == ShowAccount 
                    && s.ShowColor == ShowColor
                    && s.ShowIcon == ShowIcon
                    && s.ShowActiveHighlight == ShowActiveHighlight 

                    && s.Colors == Colors
                    && s.Offsets == Offsets 
                    && s.ShowDailyLoginDay == ShowDailyLoginDay
                    && s.IconDisplayOrder == IconDisplayOrder;
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
        public event EventHandler<AccountGridButton.IconClickedEventArgs> AccountIconClicked;

        public class MousePressedEventArgs : MouseEventArgs
        {
            public MousePressedEventArgs(MouseButtons button, int clicks, int x, int y, int delta)
                : base(button, clicks, x, y, delta)
            {

            }

            /// <summary>
            /// True to use animations or to continue with the pressed event
            /// </summary>
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

        private int lastSelected = -1;
        private int returnToScroll;
        private int buttonsOnPage;
        private bool hideNewAccount;

        private Style style;

        private SelectionBox selection;
        private Rectangle dragBounds;
        private DragHotspots dragspots;
        private CancellationTokenSource cancelPressed;

        private System.Windows.Forms.MouseButtons buttonState;
        private ButtonAction action;

        private AccountGridButtonComparer comparer;
        private AccountGridButtonContainerLayout layout;
        private List<AccountGridButton> pendingAdd;

        public AccountGridButtonContainer()
        {
            this.layout = new AccountGridButtonContainerLayout();
            this.DoubleBuffered = true;

            InitializeComponent();

            gridColumnsAuto = true;
            buttonState = System.Windows.Forms.MouseButtons.None;
            returnToScroll = -1;

            style = new Style()
            {
                FontName = AccountGridButton.FONT_NAME,
                FontStatus = AccountGridButton.FONT_STATUS,
                FontUser = AccountGridButton.FONT_USER,
                ShowAccount = true,
            };

            var buttonNewAccount = new NewAccountGridButton();
            buttonNewAccount.Visible = false;
            buttonNewAccount.Click += buttonNewAccount_Click;
            buttonNewAccount.VisibleChanged += buttonNewAccount_VisibleChanged;
            buttonNewAccount.LostFocus += buttonNewAccount_LostFocus;
            panelContents.NewButton = buttonNewAccount;
            panelContents.NewButtonVisible = true;

            this.Disposed += AccountGridButtonContainer_Disposed;
            this.MouseDown += panelContents_MouseDown;

            panelContents.MouseDown += panelContents_MouseDown;
            panelContents.ContentHeightChanged += panelContents_ContentHeightChanged;
            Application.AddMessageFilter(this);
        }

        void panelContents_ContentHeightChanged(object sender, EventArgs e)
        {
            OnContentHeightChanged();
        }

        void panelContents_MouseDown(object sender, MouseEventArgs e)
        {
            this.Focus();
            if (e.Button == System.Windows.Forms.MouseButtons.Left || e.Button == System.Windows.Forms.MouseButtons.Right)
                OnBeginSelection(sender, e);
        }

        protected override void OnParentChanged(EventArgs e)
        {
            base.OnParentChanged(e);

            //to include the parent in the draggable area
            //this.Parent.MouseDown += panelContents_MouseDown;
        }

        void ParentForm_Deactivate(object sender, EventArgs e)
        {
            ShowNewAccountButton(false);
        }

        protected override void OnHandleCreated(EventArgs e)
        {
            base.OnHandleCreated(e);

            float scale;
            using (var g = this.CreateGraphics())
            {
                scale = g.DpiX / 96f;
            }

            panelContents.GridColumnMinimumWidth = (int)(10 * scale);

            if (panelContents.NewButton != null)
            {
                panelContents.GridRowHeight = panelContents.NewButton.MinimumSize.Height;
            }
            else
            {
                panelContents.GridRowHeight = (int)(65 * scale);
            }

            var f = this.ParentForm;
            if (f != null)
                this.ParentForm.Deactivate += ParentForm_Deactivate;
        }

        private void OnBeginSelection(object sender, MouseEventArgs e)
        {
            if (selection != null)
                selection.Dispose();

            Cursor.Current = Cursors.Default;

            if (!ModifierKeys.HasFlag(Keys.Control))
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
                //if (panelContents.NewButtonVisible)
                //    panelContents.NewButton.Focus();
            }
            else
            {
                ShowNewAccountButton(false);
            }
        }

        void buttonNewAccount_VisibleChanged(object sender, EventArgs e)
        {
            //var b = panelContents.NewButton;
            //if (b.Visible)
            //{
            //    var p = this.ParentForm;
            //    if (p != null && p.ContainsFocus)
            //        b.Focus();
            //}
        }

        void buttonNewAccount_Click(object sender, EventArgs e)
        {
            if (AddAccountClick != null)
            {
                try
                {
                    AddAccountClick(this, EventArgs.Empty);
                }
                catch (Exception ex)
                {
                    Util.Logging.Log(ex);
                }
            }
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

        public void DoMouseWheel(MouseEventArgs e)
        {
            if (scrollV.Visible)
            {
                int largeChange = (panelContents.GridRowHeight + panelContents.GridSpacing) / 2;

                if (e.Delta > 0)
                {
                    scrollV.Value -= largeChange;
                }
                else
                {
                    scrollV.Value += largeChange;
                }

                if (e is HandledMouseEventArgs)
                    ((HandledMouseEventArgs)e).Handled = true;
            }
        }

        protected override void OnMouseWheel(MouseEventArgs e)
        {
            base.OnMouseWheel(e);
        }

        public bool PreFilterMessage(ref Message m)
        {
            switch ((WindowMessages)m.Msg)
            {
                //case WindowMessages.WM_MOUSEWHEEL:

                //    var pos = new Point(m.LParam.GetValue32());
                //    var wParam = m.WParam.GetValue32();
                //    var delta = wParam >> 16;
                //    var e = new MouseEventArgs(GetMouseButtons(wParam), 0, pos.X, pos.Y, delta);
                //    var f = this.ParentForm;

                //    if (f.CanFocus && f.DesktopBounds.Contains(pos))
                //        OnMsgMouseWheel(e);

                //    break;
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

                    if (panelContents.NewButtonVisible && m.WParam == (IntPtr)Keys.Menu)
                        ShowNewAccountButton(false);

                    break;
                case WindowMessages.WM_NCLBUTTONDOWN:

                    hideNewAccount = panelContents.NewButtonVisible;

                    break;
                default:

                    if (hideNewAccount)
                    {
                        hideNewAccount = false;
                        ShowNewAccountButton(false);
                    }

                    break;
            }

            return false;
        }

        private void ShowNewAccountButton(bool visible)
        {
            if (panelContents.NewButtonVisible)
            {
                if (visible)
                    return;
            }
            else if (visible)
            {
                returnToScroll = scrollV.Value;
            }
            else
            {
                return;
            }

            panelContents.NewButtonVisible = visible || panelContents.Buttons.Length == 0 || _Filter != null && buttonsOnPage == 0;
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
                int largeChange = (panelContents.GridRowHeight + panelContents.GridSpacing) / 2;

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

        public bool AllowCustomOrdering
        {
            get;
            set;
        }

        /// <summary>
        /// Number of columns to display
        /// </summary>
        public int GridColumns
        {
            get
            {
                return panelContents.GridColumns;
            }
            set
            {
                panelContents.GridColumns = value;
            }
        }

        ///// <summary>
        ///// Automatically sets number of columns based on width
        ///// </summary>
        //public bool GridColumnsAuto
        //{
        //    get
        //    {
        //        return panelContents.GridColumnsAuto;
        //    }
        //    set
        //    {
        //        panelContents.GridColumnsAuto = value;
        //    }
        //}

        private bool gridColumnsAuto;
        /// <summary>
        /// Automatically sets number of columns based on width
        /// </summary>
        [DefaultValue(true)]
        public bool GridColumnsAuto
        {
            get
            {
                return gridColumnsAuto;
            }
            set
            {
                if (gridColumnsAuto != value)
                {
                    gridColumnsAuto = value;
                    if (value)
                        AutoColumns(panelContents.Width);
                }
            }
        }

        private int gridColumnAutoWidth;
        /// <summary>
        /// Minimum width when using auto columns
        /// </summary>
        public int GridColumnAutoWidth
        {
            get
            {
                return gridColumnAutoWidth;
            }
            set
            {
                if (gridColumnAutoWidth != value)
                {
                    gridColumnAutoWidth = value;
                    if (gridColumnsAuto)
                        AutoColumns(panelContents.Width);
                }
            }
        }

        /// <summary>
        /// Space between grid buttons
        /// </summary>
        public int GridSpacing
        {
            get
            {
                return panelContents.GridSpacing;
            }
            set
            {
                panelContents.GridSpacing = value;
            }
        }

        public int CurrentButtonWidth
        {
            get
            {
                foreach (var b in panelContents.Buttons)
                {
                    if (b.GridVisibility && b.Width > 0)
                        return b.Width;
                }
                return panelContents.NewButton.Width;
            }
        }

        public int ContentHeight
        {
            get
            {
                return panelContents.Height;
            }
        }

        public int GetContentHeight(int columns)
        {
            return panelContents.GetContentHeight(columns);
        }

        public void ClearSelected()
        {
            foreach (var button in panelContents.Buttons)
            {
                button.Selected = false;
            }

            lastSelected = -1;
        }

        public void SelectAll()
        {
            foreach (var button in panelContents.Buttons)
            {
                button.Selected = button.GridVisibility;
            }

            lastSelected = -1;
        }

        public IList<AccountGridButton> GetVisible()
        {
            if (_Filter == null)
            {
                return panelContents.Buttons;
            }

            var buttons = new List<AccountGridButton>(panelContents.Buttons.Length);
            
            foreach (var button in panelContents.Buttons)
            {
                if (button.GridVisibility)
                    buttons.Add(button);
            }

            return buttons;
        }

        public IList<AccountGridButton> GetSelected()
        {
            var buttons = new List<AccountGridButton>();

            foreach (var button in panelContents.Buttons)
            {
                if (button.Selected && button.GridVisibility)
                    buttons.Add(button);
            }

            return buttons;
        }

        public IList<AccountGridButton> GetButtons()
        {
            return panelContents.Buttons;
        }

        public void Sort(Settings.SortingOptions options)
        {
            if (comparer == null)
            {
                comparer = new AccountGridButtonComparer(options);
            }
            else
            {
                comparer.Options = options;
            }

            var buttons = panelContents.Buttons;

            Array.Sort<AccountGridButton>(buttons, comparer);

            for (int i = buttons.Length - 1; i >= 0; --i)
            {
                buttons[i].Index = i;
            }

            var filtered = _Filter != null && _Filter.HasFilters;
            if (filtered || comparer.Options.Sorting.Mode != Settings.SortMode.CustomGrid)
            {
                panelContents.GridLayout = AccountGridButtonPanel.GridLayoutMode.Auto;
            }
            else
            {
                panelContents.GridLayout = AccountGridButtonPanel.GridLayoutMode.Indexed;
            }

            comparer.Clear();

            panelContents.UpdateLayout();
        }

        public void SuspendAdd()
        {
            if (pendingAdd == null)
                pendingAdd = new List<AccountGridButton>();
        }

        public void ResumeAdd()
        {
            if (pendingAdd != null)
            {
                if (pendingAdd.Count > 0)
                {
                    panelContents.SuspendLayout();
                    panelContents.NewButtonVisible = false;

                    if (pendingAdd.Count > 1)
                    {
                        panelContents.AddRange(pendingAdd.ToArray());
                    }
                    else
                    {
                        panelContents.Add(pendingAdd[0]);
                    }

                    panelContents.ResumeLayout();
                }

                pendingAdd = null;
            }
        }

        public void Add(AccountGridButton button)
        {
            this.style.Apply(button);

            button.MouseClick += button_MouseClick;
            button.MouseDown += button_MouseDown;
            button.MouseUp += button_MouseUp;
            button.IconClicked += button_IconClicked;

            button.Visible = button.GridVisibility = _Filter == null || _Filter.MatchAll(button);

            if (button.GridVisibility)
                ++buttonsOnPage;

            if (pendingAdd == null)
            {
                panelContents.SuspendLayout();
                panelContents.NewButtonVisible = false;
                panelContents.Add(button);
                panelContents.ResumeLayout();
            }
            else
            {
                pendingAdd.Add(button);
            }
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
                    try
                    {
                        AccountBeginMousePressed(button, mp);
                    }
                    catch (Exception ex)
                    {
                        Util.Logging.Log(ex);
                    }
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
            {
                try
                {
                    AccountMousePressed(sender, e);
                }
                catch (Exception ex)
                {
                    Util.Logging.Log(ex);
                }
            }
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
                            if (b.Selected && selection != null && selection.SelectionBounds.Width > 5)
                            {
                                selected = true;
                            }
                            else if (AccountMouseClick != null)
                            {
                                try
                                {
                                    AccountMouseClick(b, e);
                                }
                                catch (Exception ex)
                                {
                                    Util.Logging.Log(ex);
                                }
                            }
                        }
                        else if (b.Selected)
                        {
                            selected = true;
                        }
                        else
                        {
                            foreach (var button in panelContents.Buttons)
                            {
                                if (button.Selected)
                                {
                                    selected = true;
                                    break;
                                }
                            }
                        }

                        if (selected)
                        {
                            try
                            {
                                AccountSelection(b, e);
                            }
                            catch (Exception ex)
                            {
                                Util.Logging.Log(ex);
                            }
                        }
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
                            foreach (var button in panelContents.Buttons)
                            {
                                if (button.Bounds.Contains(p))
                                {
                                    b = button;
                                    break;
                                }
                            }
                        }

                        if (b != null)
                        {
                            try
                            {
                                AccountMouseClick(b, e);
                            }
                            catch (Exception ex)
                            {
                                Util.Logging.Log(ex);
                            }
                        }
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
                            if (comparer != null && comparer.IsSortingCustom)
                            {
                                InitializeDragHotspots(button);

                                this.AllowDrop = true;
                                button.GiveFeedback += AccountGridButtonContainer_GiveFeedback;
                            }

                            var he = new HandledMouseEventArgs(e.Button, e.Clicks, e.X, e.Y, e.Delta, false);
                            try
                            {
                                AccountBeginDrag(sender, he);
                            }
                            catch (Exception ex)
                            {
                                Util.Logging.Log(ex);
                            }

                            if (!he.Handled)
                            {
                                try
                                {
                                    DoDragDrop("", DragDropEffects.Copy);
                                    he.Handled = true;
                                }
                                catch (Exception ex)
                                {
                                    Util.Logging.Log(ex);
                                }
                            }

                            if (he.Handled)
                                buttonState = System.Windows.Forms.MouseButtons.None; //dragging causes the mouse events to be lost
                        }
                        finally
                        {
                            if (dragspots != null)
                            {
                                panelContents.SuspendLayout();

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
                                if (dragspots.bounds != null)
                                {
                                    foreach (var b in dragspots.bounds)
                                    {
                                        if (b.control != null)
                                        {
                                            panelContents.Controls.Remove(b.control);
                                            b.control.Dispose();
                                        }
                                    }
                                }
                                dragspots = null;

                                panelContents.ResumeLayout();

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

        private class Placeholder : Control
        {
            protected override void OnPaint(PaintEventArgs e)
            {
                base.OnPaint(e);

                using (var p = new Pen(Color.FromArgb(230, 230, 230), 2))
                {
                    e.Graphics.DrawRectangle(p, 1, 1, this.Width - 2, this.Height - 2);
                }
            }
        }

        private void InitializeDragHotspots(AccountGridButton source)
        {
            var count = panelContents.Buttons.Length;

            var spacing = panelContents.GridSpacing;
            var PADDING = spacing;
            var PANEL_SIZE = 1;
            var PANEL_SPACING = (PADDING - PANEL_SIZE) / 2;

            if (panelContents.IsLayoutPending)
                panelContents.PerformLayout();

            int columns = panelContents.GridColumns;

            dragspots = new DragHotspots();
            dragspots.source = source;

            var offset = panelContents.PointToScreen(Point.Empty);
            var reverse = comparer != null && comparer.Options.Sorting.Descending;
            var gridmode = comparer == null || comparer.Options.Sorting.Mode != Settings.SortMode.CustomList;
            var sourceIndex = -1;
            var buttons = panelContents.Buttons;

            var rw = (float)(panelContents.Width - spacing) / columns;
            if (rw < spacing + panelContents.GridColumnMinimumWidth)
                rw = spacing + panelContents.GridColumnMinimumWidth;
            var rh = panelContents.GridRowHeight;
            var rows = panelContents.Height / rh;

            dragspots.highlight = new Panel()
            {
                BackColor = Util.Color.Lighten(Util.Color.Invert(this.BackColor), 0.5f),
                Visible = false,
            };

            panelContents.SuspendLayout();

            dragspots.bounds = new DragHotspots.Bounds[columns * rows];

            foreach (var b in panelContents.Buttons)
            {
                if (b.GridVisibility)
                {
                    dragspots.bounds[b.GridIndex] = new DragHotspots.Bounds()
                    {
                        button = b,
                    };
                }
            }

            for (var i = dragspots.bounds.Length - 1; i >= 0; --i)
            {
                var db = dragspots.bounds[i];
                Control c;

                if (db == null)
                {
                    var column = i % columns;
                    var row = i / columns;

                    db = dragspots.bounds[i] = new DragHotspots.Bounds();

                    var _y = spacing + row * (rh + spacing);
                    var _x = spacing + (int)(column * rw);
                    var _rw = (int)((column + 1) * rw) - _x;

                    db.control = c = new Placeholder()
                    {
                        Bounds = new Rectangle(_x, _y, _rw, rh),
                    };

                    c.Visible = gridmode;

                    panelContents.Controls.Add(c);
                }
                else
                {
                    c = db.button;
                }

                db.index = i;

                int w = c.Width,
                    h = c.Height,
                    x = c.Left,
                    y = c.Top;

                int left = x - PADDING / 2 + offset.X,
                    top = y - PADDING / 2 + offset.Y,
                    width = w + PADDING,
                    height = h + PADDING;

                var visible = c != source;

                if (!visible)
                {
                    sourceIndex = i;
                    dragspots.sourceb = db;
                    dragspots.highlight.Bounds = new Rectangle(x - PANEL_SIZE, y - PANEL_SIZE, w + PANEL_SIZE*2, h+PANEL_SIZE*2);
                    dragspots.highlight.Visible = true;
                }

                db.bounds = new Rectangle(left, top, width, height);

                if (db.control == null)
                {
                    db.hotspots = new DragHotspots.Hotspot[]
                    {
                        new DragHotspots.Hotspot() //center
                        {
                            bounds = new Rectangle(left,top + height / 6,width,height * 2 / 3),
                            target = new Rectangle(x - PANEL_SIZE, y - PANEL_SIZE, w + PANEL_SIZE*2, h+PANEL_SIZE*2),
                            type = Settings.AccountSorting.SortType.Swap,
                            visible = visible,
                        },
                        new DragHotspots.Hotspot() //top
                        {
                            bounds = new Rectangle(left,top,width,height / 6),
                            target = new Rectangle(x,y - PANEL_SIZE - PANEL_SPACING,w,PANEL_SIZE),
                            type = Settings.AccountSorting.SortType.Before,
                            visible = visible,
                        },
                        new DragHotspots.Hotspot() //bottom
                        {
                            bounds = new Rectangle(left,top + height * 5 / 6,width,height / 6),
                            target = new Rectangle(x,y + h + PANEL_SPACING,w,PANEL_SIZE),
                            type = Settings.AccountSorting.SortType.After,
                            visible = visible,
                        }
                    };
                }
                else
                {
                    db.hotspots = new DragHotspots.Hotspot[]
                    {
                        new DragHotspots.Hotspot() //center
                        {
                            bounds = new Rectangle(left,top + height / 6,width,height * 2 / 3),
                            target = new Rectangle(x - PANEL_SIZE, y - PANEL_SIZE, w + PANEL_SIZE*2, h+PANEL_SIZE*2),
                            type = Settings.AccountSorting.SortType.Swap,
                            visible = visible && gridmode,
                        },
                        new DragHotspots.Hotspot() //top
                        {
                            bounds = new Rectangle(left,top,width,height / 6),
                            target = new Rectangle(x,y - PANEL_SIZE - PANEL_SPACING,w,PANEL_SIZE),
                            type = Settings.AccountSorting.SortType.Before,
                            visible = visible && gridmode,
                        },
                        new DragHotspots.Hotspot() //bottom
                        {
                            bounds = new Rectangle(left,top + height * 5 / 6,width,height / 6),
                            target = new Rectangle(x,y + h + PANEL_SPACING,w,PANEL_SIZE),
                            type = Settings.AccountSorting.SortType.After,
                            visible = visible && gridmode,
                        }
                    };
                }
            }

            if (sourceIndex != -1)
            {
                var row = sourceIndex / columns;
                if (sourceIndex > columns)
                {
                    //disable inbetween drop on the button above the source
                    var db = dragspots.bounds[sourceIndex - columns];
                    var c = db.button != null ? db.button : db.control;
                    var bh = db.hotspots[2].bounds.Height;

                    db.hotspots[2].visible = false;
                    db.hotspots[0].bounds.Height += bh; //height * 3 / 4;
                }
                if (sourceIndex + columns < dragspots.bounds.Length)
                {
                    //disable inbetween drop on the button below the source
                    var db = dragspots.bounds[sourceIndex + columns];
                    var c = db.button != null ? db.button : db.control;
                    var bh = db.hotspots[1].bounds.Height;

                    db.hotspots[1].visible = false;
                    db.hotspots[0].bounds.Y -= bh; //= top;
                    db.hotspots[0].bounds.Height += bh;// height * 3 / 4;
                }
            }

            dragspots.target = new Panel()
            {
                BackColor = Util.Color.Invert(this.BackColor),
                Visible = false,
            };

            panelContents.Controls.AddRange(new Control[] { dragspots.target, dragspots.highlight });

            panelContents.ResumeLayout();
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
                    var active = dragspots.active;
                    var source = dragspots.sourceb;
                    var b = active.button;

                    if (dragspots.highlight != null)
                        dragspots.highlight.Visible = false;

                    if (b != source.button)
                    {
                        var from = source.button.AccountData;
                        var to = b != null ? b.AccountData : null;
                        var columns = panelContents.GridColumns;
                        var column = active.index % columns;
                        var iactive = active.index;
                        var isource = source.index;

                        switch (dragspots.activeHotspot.type)
                        {
                            case Settings.AccountSorting.SortType.After:

                                if (true || to != null) //true to support grid sorting
                                {
                                    if (iactive % columns != isource % columns)
                                    {
                                        //item is being removed from a column, everything after is shifted up
                                        for (var i = isource + columns; i < dragspots.bounds.Length; i += columns)
                                        {
                                            dragspots.bounds[i].index -= columns;
                                        }
                                        //item is being inserted, everything after is shifted down
                                        for (var i = iactive + columns; i < dragspots.bounds.Length; i += columns)
                                        {
                                            dragspots.bounds[i].index += columns;
                                        }
                                    }
                                    else if (isource < iactive)
                                    {
                                        //item is being moved down within the column
                                        for (var i = isource + columns; i <= iactive; i += columns)
                                        {
                                            dragspots.bounds[i].index -= columns;
                                        }
                                    }
                                    else
                                    {
                                        //item is being moved up within the column
                                        for (var i = iactive + columns; i < isource; i += columns)
                                        {
                                            dragspots.bounds[i].index += columns;
                                        }
                                    }

                                    source.index = active.index + columns;
                                }

                                break;
                            case Settings.AccountSorting.SortType.Before:

                                if (true || to != null) //true to support grid sorting
                                {
                                    if (iactive % columns != isource % columns)
                                    {
                                        //item is being removed from a column, everything after is shifted up
                                        for (var i = isource + columns; i < dragspots.bounds.Length; i += columns)
                                        {
                                            dragspots.bounds[i].index -= columns;
                                        }
                                        //item is being inserted, everything after is shifted down
                                        for (var i = iactive; i < dragspots.bounds.Length; i += columns)
                                        {
                                            dragspots.bounds[i].index += columns;
                                        }
                                    }
                                    else if (isource < iactive)
                                    {
                                        //item is being moved down within the column
                                        for (var i = isource + columns; i < iactive; i += columns)
                                        {
                                            dragspots.bounds[i].index -= columns;
                                        }
                                    }
                                    else
                                    {
                                        //item is being moved up within the column
                                        for (var i = iactive; i < isource; i += columns)
                                        {
                                            dragspots.bounds[i].index += columns;
                                        }
                                    }

                                    source.index = active.index - columns;
                                }

                                break;
                            case Settings.AccountSorting.SortType.Swap:

                                var ai = active.index;
                                active.index = source.index;
                                source.index = ai;

                                break;
                        }

                        var reversed = comparer != null && comparer.Options.Sorting.Descending;
                        if (reversed)
                        {
                            var lasti = 0;

                            foreach (var _b in dragspots.bounds)
                            {
                                if (_b.button != null && _b.button.AccountData != null)
                                {
                                    if (_b.index > lasti)
                                        lasti = _b.index;
                                }
                            }

                            lasti += columns - lasti % columns - 1;

                            foreach (var _b in dragspots.bounds)
                            {
                                if (_b.button != null && _b.button.AccountData != null)
                                {
                                    _b.button.SortKey = (ushort)(lasti - _b.index + 1);
                                }
                            }
                        }
                        else
                        {
                            foreach (var _b in dragspots.bounds)
                            {
                                if (_b.button != null && _b.button.AccountData != null)
                                {
                                    _b.button.SortKey = (ushort)(_b.index + 1);
                                }
                            }
                        }

                        Settings.AccountSorting.Update(Page);
                    }

                    dragspots.Hide();
                }
            }
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
                    foreach (var button in panelContents.Buttons)
                    {
                        if (button.Bounds.Contains(p))
                        {
                            b = button;
                            break;
                        }
                    }
                }

                if (b != null)
                {
                    try
                    {
                        AccountMouseClick(b, e);
                    }
                    catch (Exception ex)
                    {
                        Util.Logging.Log(ex);
                    }
                }
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

        void button_IconClicked(object sender, AccountGridButton.IconClickedEventArgs e)
        {
            if (AccountIconClicked != null)
            {
                try
                {
                    AccountIconClicked(sender, e);
                }
                catch (Exception ex)
                {
                    Util.Logging.Log(ex);
                }
            }
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
                var buttons = panelContents.Buttons;

                if (lastSelected == -1)
                {
                    for (var i = buttons.Length - 1; i >= 0; --i)
                    {
                        if (buttons[i].GridVisibility)
                        {
                            buttons[i].Selected = i == index;
                        }
                    }
                    lastSelected = index;
                }
                else
                {
                    for (var i = buttons.Length - 1; i >= 0; --i)
                    {
                        if (buttons[i].GridVisibility)
                        {
                            if (lastSelected > index)
                                buttons[i].Selected = (i >= index && i <= lastSelected);
                            else
                                buttons[i].Selected = (i >= lastSelected && i <= index);
                        }
                    }
                }
            }
            else
            {
                if (AccountBeginMouseClick != null)
                {
                    SetAction(ButtonAction.None);

                    var mp = new MousePressedEventArgs(buttonState, e.Clicks, e.X, e.Y, e.Delta);
                    try
                    {
                        AccountBeginMouseClick(button, mp);
                    }
                    catch (Exception ex)
                    {
                        Util.Logging.Log(ex);
                    }
                    if (mp.Handled)
                    {
                        SetAction(ButtonAction.Pressed);
                        button.EndPressed += button_EndPressed;
                        button.BeginPressed(AccountGridButton.PressedState.Pressed, GetPressedCancelToken().Token, mp);
                    }
                }

                if (AccountMouseClick != null)
                {
                    try
                    {
                        AccountMouseClick(sender, e);
                    }
                    catch (Exception ex)
                    {
                        Util.Logging.Log(ex);
                    }
                }
            }
        }

        public void Remove(AccountGridButton button)
        {
            panelContents.SuspendLayout();
            panelContents.Remove(button);
            if (buttonsOnPage > 0)
            {
                if (_Filter == null || _Filter.Page == 0 || button.Paging != null && button.Paging.Current != null && button.Paging.Current.Page == _Filter.Page)
                    --buttonsOnPage;
            }
            if (panelContents.Buttons.Length == 0 || buttonsOnPage == 0)
                panelContents.NewButtonVisible = true;
            panelContents.ResumeLayout();
        }

        public override LayoutEngine LayoutEngine
        {
            get
            {
                return layout;
            }
        }

        public void SetStyle(Style s)
        {
            var buttonNewAccount = panelContents.NewButton;

            if (s.Colors == null)
                s.Colors = UiColors.GetTheme();

            if (this.style.Equals(s) && this.IsHandleCreated)
                return;

            this.style = s;

            foreach (var b in panelContents.Buttons)
            {
                s.Apply(b);
            }

            s.Apply(buttonNewAccount);

            buttonNewAccount.ShowColorKey = false;
            buttonNewAccount.ShowImage = false;
            buttonNewAccount.ShowDailyLoginDay = Settings.DailyLoginDayIconFlags.None;

            buttonNewAccount.ResizeLabels();

            panelContents.GridRowHeight = buttonNewAccount.MinimumSize.Height;
            using (var g = this.CreateGraphics())
            {
                GridColumnAutoWidth = TextRenderer.MeasureText(g, "www", s.FontName).Width * 5;
            }
        }

        private void scrollV_ValueChanged(object sender, int e)
        {
            if (panelContents.NewButtonVisible && returnToScroll != -1 && scrollV.Value < scrollV.Maximum)
            {
                returnToScroll = -1;
            }

            panelContents.Location = new Point(0, -e);
        }

        private void OnContentHeightChanged()
        {
            if (ContentHeightChanged != null)
            {
                try
                {
                    ContentHeightChanged(this, EventArgs.Empty);
                }
                catch (Exception ex)
                {
                    Util.Logging.Log(ex);
                }
            }
        }

        private void AutoColumns(int width)
        {
            int columns;

            if (gridColumnAutoWidth == 0)
            {
                columns = 1;
            }
            else
            {
                columns = (width - panelContents.GridSpacing) / (gridColumnAutoWidth + panelContents.GridSpacing);
                if (columns < 1)
                    columns = 1;
            }

            GridColumns = columns;
        }

        protected void OnFilterChanged(bool pageChanged)
        {
            var buttons = panelContents.Buttons;
            var enabled = _Filter != null;
            var filtered = enabled && _Filter.HasFilters;
            var count = enabled ? 0 : buttons.Length;

            this.SuspendLayout();

            foreach (var b in buttons)
            {
                bool v;

                if (enabled)
                {
                    if (_Filter.MatchPage(b))
                    {
                        ++count;

                        v = _Filter.MatchFilters(b);
                    }
                    else
                        v = false;
                }
                else
                {
                    if (pageChanged && b.Paging != null)
                    {
                        b.Paging.Current = null;
                    }
                    v = true;
                }

                b.Visible = b.GridVisibility = v;
            }

            if (buttonsOnPage != count)
            {
                var b = buttonsOnPage == 0 || count == 0;
                buttonsOnPage = count;
                if (b)
                {
                    ShowNewAccountButton(count == 0);
                }
            }

            if (pageChanged && count > 0)
            {
                if (comparer != null)
                    Sort(comparer.Options);
                else
                    Sort(Settings.Sorting.Value);
            }

            //if (sort)
            //{
            //    if (comparer != null)
            //        Sort(comparer.Options);
            //    else
            //        Sort(Settings.Sorting.Value);
            //}
            //else if (pageChanged && count > 0)
            //{
            //    if (comparer != null && comparer.IsSortingCustom)
            //        Sort(comparer.Options);
            //}

            if (filtered || comparer == null || comparer.Options.Sorting.Mode != Settings.SortMode.CustomGrid)
            {
                panelContents.GridLayout = AccountGridButtonPanel.GridLayoutMode.Auto;
            }
            else
            {
                panelContents.GridLayout = AccountGridButtonPanel.GridLayoutMode.Indexed;
            }

            panelContents.UpdateLayout();

            this.ResumeLayout();
        }

        public byte Page
        {
            get
            {
                if (_Filter == null)
                    return 0;
                return _Filter.Page;
            }
            set
            {
                if (_Filter == null)
                {
                    if (value > 0)
                    {
                        _Filter = new SearchFilter(value, null);
                        OnFilterChanged(true);
                    }
                }
                else
                {
                    if (_Filter.Page != value)
                    {
                        _Filter.Page = value;
                        if (!_Filter.IsActive)
                            _Filter = null;
                        OnFilterChanged(true);
                    }
                }
            }
        }

        private SearchFilter _Filter;
        public string Filter
        {
            get
            {
                if (_Filter == null)
                    return null;
                return _Filter.Filter;
            }
            set
            {
                if (string.IsNullOrEmpty(value))
                    value = null;

                if (_Filter == null)
                {
                    if (!string.IsNullOrEmpty(value))
                    {
                        _Filter = new SearchFilter(0, value);
                        OnFilterChanged(false);
                    }
                }
                else
                {
                    if (_Filter.Filter != value)
                    {
                        _Filter.Filter = value;
                        if (!_Filter.IsActive)
                            _Filter = null;
                        OnFilterChanged(false);
                    }
                }
            }
        }

        public bool HasNumberFilter
        {
            get
            {
                if (_Filter == null)
                    return false;
                return _Filter.HasNumberFilter;
            }
        }

        public bool FilterDailyLogin
        {
            get
            {
                if (_Filter == null)
                    return false;
                return _Filter.DailyLogin;
            }
            set
            {
                if (_Filter == null)
                {
                    if (value)
                    {
                        _Filter = new SearchFilter()
                        {
                            DailyLogin = value,
                        };
                        OnFilterChanged(false);
                    }
                }
                else
                {
                    if (_Filter.DailyLogin != value)
                    {
                        _Filter.DailyLogin = value;
                        if (!_Filter.IsActive)
                            _Filter = null;
                        OnFilterChanged(false);
                    }
                }
            }
        }

        public bool IsFilterEnabled
        {
            get
            {
                return _Filter != null;
            }
        }

        public void RefreshFilter()
        {
            OnFilterChanged(false);
        }

        public void RefreshFilter(AccountGridButton b, bool update = true)
        {
            var visible = _Filter == null || _Filter.MatchAll(b);

            if (b.GridVisibility != visible)
            {
                this.SuspendLayout();

                b.Visible = b.GridVisibility = visible;

                if (update)
                    panelContents.UpdateLayout();

                this.ResumeLayout();
            }
        }
    }
}
