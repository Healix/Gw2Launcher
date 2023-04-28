using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.Windows.Forms;
using Gw2Launcher.Windows.Native;

namespace Gw2Launcher.UI.WindowPositioning
{
    public partial class formTemplates : Base.BaseForm
    {
        public class TemplateDisplayManager : IDisposable
        {
            private class WindowDisplay : Form
            {
                private Pen pen;
                private SolidBrush brush;

                public WindowDisplay()
                {
                    SetStyle(ControlStyles.AllPaintingInWmPaint, true);
                    SetStyle(ControlStyles.UserPaint, true);
                    SetStyle(ControlStyles.OptimizedDoubleBuffer, true);
                    SetStyle(ControlStyles.ResizeRedraw, true);

                    pen = new Pen(Color.Transparent, 4);
                    brush = new SolidBrush(Color.Transparent);

                    FormBorderStyle = FormBorderStyle.None;
                    ShowInTaskbar = false;
                    StartPosition = FormStartPosition.Manual;
                    BackColor = Color.Black;
                    TransparencyKey = Color.Black;

                    OnHighlightedChanged();
                }

                protected override bool ShowWithoutActivation
                {
                    get
                    {
                        return true;
                    }
                }

                protected override CreateParams CreateParams
                {
                    get
                    {
                        var cp = base.CreateParams;
                        cp.ExStyle |= (int)(WindowStyle.WS_EX_COMPOSITED | WindowStyle.WS_EX_LAYERED | WindowStyle.WS_EX_TRANSPARENT);
                        return cp;
                    }
                }

                private void OnHighlightedChanged()
                {
                    if (_Highlighted)
                    {
                        Opacity = 0.5;
                        pen.Color = Color.FromArgb(0, 100, 0);
                        brush.Color = Color.FromArgb(158, 196, 158);
                    }
                    else
                    {
                        Opacity = 0.25;
                        pen.Color = Color.FromArgb(0, 0, 100);
                        brush.Color = Color.FromArgb(158, 158, 196);
                    }

                    this.Invalidate();
                }

                public bool Hidden
                {
                    get;
                    set;
                }

                private bool _Highlighted;
                public bool Highlighted
                {
                    get
                    {
                        return _Highlighted;
                    }
                    set
                    {
                        if (_Highlighted != value)
                        {
                            _Highlighted = value;
                            OnHighlightedChanged();
                        }
                    }
                }

                public WindowDisplay NextWindow
                {
                    get;
                    set;
                }

                protected override void OnPaint(PaintEventArgs e)
                {
                    const int PADDING = 5;

                    var g = e.Graphics;
                    var pw = (int)pen.Width;
                    var r = new Rectangle(pw / 2 + PADDING, pw / 2 + PADDING, this.Width - pw - PADDING * 2, this.Height - pw - PADDING * 2);

                    g.Clear(this.TransparencyKey);
                    g.FillRectangle(brush, r);
                    g.DrawRectangle(pen, r);
                }

                protected override void OnPaintBackground(PaintEventArgs e)
                {
                    e.Graphics.Clear(this.TransparencyKey);
                }

                protected override void Dispose(bool disposing)
                {
                    base.Dispose(disposing);

                    if (disposing)
                    {
                        pen.Dispose();
                        brush.Dispose();
                    }
                }
            }

            public event EventHandler<Form> TopLevelChanged;

            private bool disposed;
            private List<WindowDisplay>[] windows;
            private Dictionary<Point, int>[] points;
            private Queue<WindowDisplay> unused;
            private Rectangle[] zones;
            private Task task;
            private CancellationTokenSource cancel;
            private formTemplates owner;

            public TemplateDisplayManager(formTemplates owner, int screens)
            {
                windows = new List<WindowDisplay>[screens];
                points = new Dictionary<Point, int>[screens];
                unused = new Queue<WindowDisplay>();
                zones = new Rectangle[screens];

                this.owner = owner;

                _RequiredModifierKey = Keys.Shift | Keys.ShiftKey | Keys.RShiftKey | Keys.LShiftKey | Keys.Control | Keys.ControlKey | Keys.LControlKey | Keys.RControlKey | Keys.Alt;

                for (var i = 0; i < screens; i++)
                {
                    windows[i] = new List<WindowDisplay>();
                    points[i] = new Dictionary<Point, int>();
                }
            }

            private WindowDisplay CreateWindow()
            {
                if (unused.Count > 0)
                {
                    var f1 = unused.Dequeue();
                    while (f1.IsDisposed && unused.Count > 0)
                    {
                        f1 = unused.Dequeue();
                        f1.Highlighted = false;
                    }
                    if (!f1.IsDisposed)
                        return f1;
                }

                return new WindowDisplay();
            }

            public void Set(int screen, Rectangle[] rects)
            {
                var windows = this.windows[screen];
                var points = this.points[screen];
                var existing = windows.Count;
                int l = int.MaxValue,
                    t = int.MaxValue,
                    r = int.MinValue,
                    b = int.MinValue;
                IntPtr dwp;

                var lowest = owner.master.GetLowest().Handle;

                if (rects.Length == existing)
                {
                    dwp = NativeMethods.BeginDeferWindowPos(rects.Length);

                    points.Clear();

                    for (var j = 0; j < rects.Length; j++)
                    {
                        var w = windows[j];

                        if (w.Hidden = rects[j].Width == 0 || rects[j].Height == 0)
                        {
                            Hide(w);
                        }
                        else
                        {
                            if (rects[j].X < l)
                                l = rects[j].X;
                            if (rects[j].Y < t)
                                t = rects[j].Y;
                            if (rects[j].Right > r)
                                r = rects[j].Right;
                            if (rects[j].Bottom > b)
                                b = rects[j].Bottom;
                        }

                        points[rects[j].Location] = j;

                        dwp = NativeMethods.DeferWindowPos(dwp, w.Handle, lowest, rects[j].X, rects[j].Y, rects[j].Width, rects[j].Height, (uint)(w.Hidden ? SetWindowPosFlags.SWP_NOACTIVATE | SetWindowPosFlags.SWP_NOSIZE : SetWindowPosFlags.SWP_NOACTIVATE));
                    }
                }
                else
                {
                    var indexes = new int[rects.Length];
                    var used = new bool[existing];
                    var uindex = 0;

                    dwp = NativeMethods.BeginDeferWindowPos(rects.Length);

                    for (var j = 0; j < rects.Length; j++)
                    {
                        int w;

                        if (points.TryGetValue(rects[j].Location, out w))
                        {
                            points.Remove(rects[j].Location);
                            indexes[j] = w;
                            used[w] = true;
                        }
                        else
                        {
                            indexes[j] = -1;
                        }
                    }

                    points.Clear();

                    for (var j = 0; j < rects.Length; j++)
                    {
                        WindowDisplay w;
                        int i;

                        if (indexes[j] == -1)
                        {
                            i = -1;
                            w = null;

                            for (; uindex < existing; )
                            {
                                if (!used[uindex])
                                {
                                    i = uindex++;

                                    used[i] = true;
                                    w = windows[i];
                                    w.Highlighted = false;

                                    break;
                                }
                                else
                                {
                                    ++uindex;
                                }
                            }

                            if (i == -1)
                            {
                                i = uindex++;
                                w = CreateWindow();
                                windows.Add(w);
                            }
                        }
                        else
                        {
                            i = indexes[j];
                            w = windows[i];
                            w.Highlighted = false;
                        }

                        if (w.Hidden = rects[j].Width == 0 || rects[j].Height == 0)
                        {
                            Hide(w);
                        }
                        else
                        {
                            if (rects[j].X < l)
                                l = rects[j].X;
                            if (rects[j].Y < t)
                                t = rects[j].Y;
                            if (rects[j].Right > r)
                                r = rects[j].Right;
                            if (rects[j].Bottom > b)
                                b = rects[j].Bottom;
                        }

                        points[rects[j].Location] = i;

                        dwp = NativeMethods.DeferWindowPos(dwp, w.Handle, lowest, rects[j].X, rects[j].Y, rects[j].Width, rects[j].Height, (uint)(w.Hidden ? SetWindowPosFlags.SWP_NOACTIVATE | SetWindowPosFlags.SWP_NOSIZE : SetWindowPosFlags.SWP_NOACTIVATE));
                    }

                    var remove = true;

                    for (var j = existing - 1; j >= uindex; --j)
                    {
                        if (!used[j])
                        {
                            var w = windows[j];

                            if (remove)
                            {
                                unused.Enqueue(windows[j]);
                                windows.RemoveAt(j);
                            }
                            else
                            {
                                w.Hidden = true;
                            }

                            Hide(w);
                        }
                        else
                        {
                            remove = false;
                        }
                    }
                }

                //for (var j = 0; j < rects.Length; j++)
                //{
                //    if (rects[j].Width == 0 || rects[j].Height == 0)
                //        continue;

                //    WindowDisplay w;

                //    if (i < existing)
                //    {
                //        w = windows[i++];
                //        w.Highlighted = false;
                //    }
                //    else
                //    {
                //        w = CreateWindow();
                //        windows.Add(w);
                //    }

                //    dwp = NativeMethods.DeferWindowPos(dwp, w.Handle, owner.Handle, rects[j].X, rects[j].Y, rects[j].Width, rects[j].Height, (uint)(SetWindowPosFlags.SWP_NOACTIVATE));

                //    if (rects[j].X < l)
                //        l = rects[j].X;
                //    if (rects[j].Y < t)
                //        t = rects[j].Y;
                //    if (rects[j].Right > r)
                //        r = rects[j].Right;
                //    if (rects[j].Bottom > b)
                //        b = rects[j].Bottom;
                //}

                zones[screen] = new Rectangle(l, t, r - l, b - t);

                //if (i < existing)
                //{
                //    for (var j = i; j < existing; j++)
                //    {
                //        unused.Enqueue(windows[j]);
                //        Hide(windows[j]);
                //    }
                //    windows.RemoveRange(i, existing - i);
                //}

                if (dwp == IntPtr.Zero || !NativeMethods.EndDeferWindowPos(dwp))
                {
                    for (var j = 0; j < rects.Length; j++)
                    {
                        windows[j].Bounds = rects[j];
                    }
                }
            }

            public void Remove(int screen)
            {
                foreach (var w in windows[screen])
                {
                    Hide(w);
                    w.Hidden = true;
                    //unused.Enqueue(w);
                }
                //windows[screen].Clear();
                //points[screen].Clear();
            }

            private void Hide(WindowDisplay w)
            {
                if (w.NextWindow != null)
                {
                    var owner = w.Owner;
                    w.Owner = null;
                    w.NextWindow.Owner = owner;
                    w.NextWindow = null;
                }
                else if (_TopLevel == w)
                {
                    TopLevel = w.Owner;
                    w.Owner = null;
                }
                else
                {
                    w.Owner = null;
                }

                w.Hide();
            }

            public void Clear()
            {
                for (var i = 0; i < windows.Length; i++)
                {
                    Remove(i);
                }
            }

            public void Show()
            {
                WindowDisplay owner = null;

                foreach (var w in windows)
                {
                    foreach (var f in w)
                    {
                        if (f.Hidden)
                        {
                            f.Owner = null;
                            continue;
                        }

                        if (owner != null)
                            owner.NextWindow = f;
                        f.Owner = null;
                        f.Owner = owner;
                        owner = f;

                        if (!f.IsDisposed && !f.Visible)
                        {
                            f.Show();
                        }
                    }
                }

                if (owner != null)
                    owner.NextWindow = null;

                TopLevel = owner;
            }

            public void Hide()
            {
                TopLevel = null;

                foreach (var w in windows)
                {
                    foreach (var f in w)
                    {
                        f.Owner = null;
                        f.NextWindow = null;
                        f.Hide();
                    }
                }
            }

            public bool IsDisposed
            {
                get
                {
                    return disposed;
                }
            }

            public void Dispose()
            {
                if (!disposed)
                {
                    disposed = true;

                    Enabled = false;
                    TopLevel = null;

                    foreach (var w in windows)
                    {
                        foreach (var f in w)
                        {
                            f.Dispose();
                        }
                    }
                    windows = null;

                    foreach (var f in unused)
                    {
                        f.Dispose();
                    }
                    unused = null;

                    if (task != null && task.IsCompleted)
                        task.Dispose();
                }
            }

            private Form _TopLevel;
            public Form TopLevel
            {
                get
                {
                    return _TopLevel;
                }
                set
                {
                    if (_TopLevel != value)
                    {
                        _TopLevel = value;
                        if (TopLevelChanged != null)
                            TopLevelChanged(this, _TopLevel);
                    }
                }
            }

            private WindowDisplay _Highlighted;
            private WindowDisplay Highlighted
            {
                get
                {
                    return _Highlighted;
                }
                set
                {
                    if (_Highlighted != value)
                    {
                        var h = _Highlighted;
                        if (h != null)
                        {
                            Util.Invoke.Required(h,
                                delegate
                                {
                                    h.Highlighted = false;
                                });
                        }
                        if (value != null)
                        {
                            Util.Invoke.Required(value,
                                delegate
                                {
                                    value.Highlighted = true;
                                });
                        }
                        _Highlighted = value;
                    }
                }
            }

            private Keys _RequiredModifierKey;
            public Keys RequiredModifierKey
            {
                get
                {
                    return _RequiredModifierKey;
                }
                set
                {
                    _RequiredModifierKey = value;
                }
            }

            private bool _Enabled;
            public bool Enabled
            {
                get
                {
                    return _Enabled;
                }
                set
                {
                    if (_Enabled != value)
                    {
                        _Enabled = value;

                        if (value)
                        {
                            cancel = new CancellationTokenSource();

                            var t = cancel.Token;
                            task = Task.Factory.StartNew(
                                delegate
                                {
                                    DoMonitor(t);
                                }, t, TaskCreationOptions.LongRunning);
                        }
                        else
                        {
                            if (cancel != null)
                            {
                                using (cancel)
                                {
                                    cancel.Cancel();
                                    cancel = null;
                                }
                            }

                            Highlighted = null;
                        }
                    }
                }
            }

            public bool IsHovered
            {
                get
                {
                    return _Highlighted != null;
                }
            }

            public Rectangle HoveredBounds
            {
                get
                {
                    if (_Highlighted == null)
                        return Rectangle.Empty;
                    return _Highlighted.Bounds;
                }
            }

            public bool SnapToEdge
            {
                get;
                set;
            }

            private WindowDisplay GetWindowFromPoint(Point p)
            {
                for (var i = zones.Length - 1; i >= 0; --i)
                {
                    if (zones[i].Contains(p))
                    {
                        var windows = this.windows[i];

                        for (var j = windows.Count - 1; j >= 0; --j)
                        {
                            if (!windows[j].Hidden && windows[j].Bounds.Contains(p))
                            {
                                return windows[j];
                            }
                        }
                    }
                }

                return null;
            }

            public List<Rectangle> GetArea(Rectangle bounds)
            {
                var r = new List<Rectangle>();

                for (var i = zones.Length - 1; i >= 0; --i)
                {
                    if (bounds.IntersectsWith(zones[i]) || bounds.Contains(zones[i]))
                    {
                        var windows = this.windows[i];

                        for (var j = windows.Count - 1; j >= 0; --j)
                        {
                            if (!windows[j].Hidden)
                            {
                                var b = windows[j].Bounds;
                                if (bounds.Contains(b) || bounds.IntersectsWith(b))
                                    r.Add(b);
                            }
                        }
                    }
                }

                return r;
            }

            public bool GetBoundsFromPoint(Point p, out Rectangle bounds)
            {
                var w = GetWindowFromPoint(p);

                if (w != null)
                {
                    bounds = w.Bounds;
                    return true;
                }

                bounds = Rectangle.Empty;
                return false;
            }

            private void DoMonitor(CancellationToken cancel)
            {
                var _w = Rectangle.Empty;

                while (true)
                {
                    try
                    {
                        cancel.WaitHandle.WaitOne(300);
                        if (cancel.IsCancellationRequested)
                            break;
                    }
                    catch
                    {
                        return;
                    }

                    if (_RequiredModifierKey != 0 && (ModifierKeys & _RequiredModifierKey) == 0)
                    {
                        if (_w.Width != 0)
                        {
                            Highlighted = null;
                            _w = Rectangle.Empty;
                        }
                        continue;
                    }

                    var p = Cursor.Position;

                    if (_w.Contains(p))
                        continue;

                    var w = GetWindowFromPoint(p);

                    if (w != null)
                    {
                        _w = w.Bounds;
                    }
                    else
                    {
                        _w = new Rectangle(p.X - 2, p.Y - 2, 4, 4);
                    }

                    Highlighted = w;
                }
            }
        }
        
        private class TemplateDisplay : Control
        {
            private BufferedGraphics buffer;
            private bool redraw;
            private ScreenTemplate template;
            private Rectangle[] display;

            public TemplateDisplay()
            {
                redraw = true;
                SetColor();
                Cursor = Windows.Cursors.Hand;
                //Padding = new Padding(3, 3, 3, 3);
            }

            private void OnRedrawRequired()
            {
                if (redraw)
                    return;
                redraw = true;
                this.Invalidate();
            }

            protected override void OnMouseEnter(EventArgs e)
            {
                Hovered = true;
                base.OnMouseEnter(e);
            }

            protected override void OnMouseLeave(EventArgs e)
            {
                Hovered = false;
                base.OnMouseLeave(e);
            }

            public ScreenTemplate Template
            {
                get
                {
                    return template;
                }
                set
                {
                    template = value;
                    display = null;
                    OnRedrawRequired();
                }
            }

            protected override void OnTextChanged(EventArgs e)
            {
                base.OnTextChanged(e);

                if (_TextHeight > 0)
                    OnRedrawRequired();
            }

            private int _TextHeight;
            public int TextHeight
            {
                get
                {
                    return _TextHeight;
                }
                set
                {
                    if (_TextHeight != value)
                    {
                        _TextHeight = value;
                        OnRedrawRequired();
                    }
                }
            }

            public Settings.WindowTemplate Source
            {
                get;
                set;
            }

            public List<TemplateDisplay> Shared
            {
                get;
                set;
            }

            public Settings.WindowTemplate.Assignment AssignedTo
            {
                get;
                set;
            }

            private bool _Activated;
            public bool Activated
            {
                get
                {
                    return _Activated;
                }
                set
                {
                    if (_Activated != value)
                    {
                        _Activated = value;
                        OnRedrawRequired();
                    }
                }
            }

            private void SetColor()
            {
                if (_Selected || _Hovered)
                {
                    ForeColor = Color.FromArgb(220, 220, 220);
                    _BorderColor = Color.FromArgb(160, 160, 160);
                }
                else
                {
                    ForeColor = Color.FromArgb(200, 200, 200);
                    _BorderColor = Color.FromArgb(140, 140, 140);
                }
            }

            public Template TemplateData
            {
                get;
                set;
            }

            protected override void OnBackColorChanged(EventArgs e)
            {
                base.OnBackColorChanged(e);
                OnRedrawRequired();
            }

            protected override void OnForeColorChanged(EventArgs e)
            {
                base.OnForeColorChanged(e);
                OnRedrawRequired();
            }

            private bool _Selected;
            public bool Selected
            {
                get
                {
                    return _Selected;
                }
                set
                {
                    if (_Selected != value)
                    {
                        _Selected = value;
                        SetColor();
                        OnRedrawRequired();
                    }
                }
            }

            private bool _Hovered;
            private bool Hovered
            {
                get
                {
                    return _Hovered;
                }
                set
                {
                    if (_Hovered != value)
                    {
                        _Hovered = value;
                        SetColor();
                        OnRedrawRequired();
                    }
                }
            }

            private Color _BorderColor;
            private Color BorderColor
            {
                get
                {
                    return _BorderColor;
                }
                set
                {
                    _BorderColor = value;
                    OnRedrawRequired();
                }
            }

            protected override void OnSizeChanged(EventArgs e)
            {
                base.OnSizeChanged(e);

                if (buffer != null)
                {
                    buffer.Dispose();
                    buffer = null;
                }
                display = null;
                OnRedrawRequired();
            }

            private bool _FlipHorizontal;
            public bool FlipHorizontal
            {
                get
                {
                    return _FlipHorizontal;
                }
                set
                {
                    if (_FlipHorizontal != value)
                    {
                        _FlipHorizontal = value;
                        display = null;
                        OnRedrawRequired();
                    }
                }
            }

            private bool _FlipVertical;
            public bool FlipVertical
            {
                get
                {
                    return _FlipVertical;
                }
                set
                {
                    if (_FlipVertical != value)
                    {
                        _FlipVertical = value;
                        display = null;
                        OnRedrawRequired();
                    }
                }
            }

            protected override void OnPaintBackground(PaintEventArgs e)
            {
                if (redraw)
                {
                    redraw = false;

                    if (buffer == null)
                        buffer = BufferedGraphicsManager.Current.Allocate(e.Graphics, this.ClientRectangle);

                    var g = buffer.Graphics;

                    if (_Selected)
                    {
                        g.Clear(Color.FromArgb(248, 248, 248));
                        using (var pen = new Pen(Color.FromArgb(220, 220, 220)))
                        {
                            g.DrawRectangle(pen, 0, 0, this.Width - 1, this.Height - 1);
                        }
                    }
                    else
                    {
                        g.Clear(this.BackColor);

                        g.DrawRectangle(Pens.DarkGray, this.Padding.Left, this.Padding.Top, this.Width - this.Padding.Horizontal - 1, this.Height - this.Padding.Vertical - _TextHeight - 1);
                    }

                    if (template != null)
                    {
                        if (display == null)
                            display = template.GetWindows(new Rectangle(this.Padding.Left, this.Padding.Top, this.Width - this.Padding.Horizontal, this.Height - this.Padding.Vertical - _TextHeight), _FlipHorizontal, _FlipVertical);

                        using (var brush = new SolidBrush(this.ForeColor))
                        {
                            using (var pen = new Pen(_BorderColor, 1f))
                            {
                                foreach (var r in display)
                                {
                                    g.FillRectangle(brush, r);
                                    g.DrawRectangle(pen, r.X, r.Y, r.Width - 1, r.Height - 1);
                                }
                            }
                        }
                    }

                    if (_Activated)
                    {
                        using (var brush = new SolidBrush(Color.FromArgb(180, 0, 90, 0)))
                        {
                            var w = this.Width-2;
                            var h = (int)(this.Font.GetHeight(g) * 1.1f);
                            var y = this.Padding.Top + (this.Height - this.Padding.Vertical - _TextHeight - h) / 2;
                            var r = new Rectangle((this.Width - w) / 2, y, w, h);

                            g.FillRectangle(brush, r);
                            TextRenderer.DrawText(g, "active", this.Font, r, Color.White, Color.Transparent, TextFormatFlags.SingleLine | TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);
                        }
                    }

                    if (_TextHeight > 0)
                    {
                        Color c;
                        var text = this.Text;
                        if (string.IsNullOrEmpty(text))
                        {
                            text = "(no name)";
                            c = SystemColors.GrayText;
                        }
                        else
                        {
                            c = SystemColors.ControlText;
                        }

                        var sz = TextRenderer.MeasureText(g, text, Font, new Size(this.Width - this.Padding.Horizontal, 0), TextFormatFlags.WordBreak | TextFormatFlags.NoPadding | TextFormatFlags.TextBoxControl);

                        if (sz.Height > _TextHeight - this.Padding.Vertical)
                        {
                            TextRenderer.DrawText(g, text, Font, new Rectangle(this.Padding.Left, this.Height - _TextHeight, this.Width - this.Padding.Horizontal, _TextHeight), c, Color.Transparent, TextFormatFlags.HorizontalCenter | TextFormatFlags.Top | TextFormatFlags.WordBreak | TextFormatFlags.EndEllipsis | TextFormatFlags.TextBoxControl);
                        }
                        else
                        {
                            TextRenderer.DrawText(g, text, Font, new Rectangle(this.Padding.Left, this.Height - _TextHeight + (_TextHeight - sz.Height) / 2 - this.Padding.Bottom, this.Width - this.Padding.Horizontal, _TextHeight), c, Color.Transparent, TextFormatFlags.HorizontalCenter | TextFormatFlags.Top | TextFormatFlags.WordBreak | TextFormatFlags.EndEllipsis | TextFormatFlags.TextBoxControl);
                        }
                    }
                }

                buffer.Render(e.Graphics);
            }

            protected override void Dispose(bool disposing)
            {
                base.Dispose(disposing);

                if (disposing)
                {
                    if (buffer != null)
                        buffer.Dispose();
                }
            }

            protected override void OnDoubleClick(EventArgs e)
            {
                if (AssignedTo != null)
                {
                    Activated = !_Activated;
                }

                base.OnDoubleClick(e);
            }
        }

        public event EventHandler<TemplateDisplayManager> TemplateDisplayManagerChanged;

        public enum TemplateMode
        {
            WindowTemplates,
            Manager
        }

        public event EventHandler ShowCompact;
        private event EventHandler<TemplateDisplay> TemplateDisplayAdded;

        private Dictionary<Template, object[]> defaultValues;
        private CheckBox[] checkScreen;
        private TemplateDisplay[] basicTemplates;
        private Dictionary<Rectangle, int> screens;
        private TemplateDisplayManager displayManager;
        private formWindowSize master;
        private int delayedChange;
        private TemplateMode mode;
        private Size defaultDisplaySize, defaultAssignedSize;
        private int assignedTextHeight;
        private Dictionary<Settings.WindowTemplate, TemplateDisplay> customTemplates;
        private Dictionary<Settings.WindowTemplate.Assignment, TemplateDisplay> assignedTemplates;
        private TextBox textName;

        /// <summary>
        /// Initializes the templates form to work with the window positioning tool
        /// </summary>
        public formTemplates(formWindowSize master)
        {
            mode = TemplateMode.WindowTemplates;

            InitializeComponents();

            contextCustom.Items.Remove(toolStripMenuItem1);
            contextCustom.Items.Remove(snapToToolStripMenuItem);
            contextCustom.Items.Remove(accountTypeToolStripMenuItem);
            contextCustom.Items.Remove(toolStripMenuItem2);
            contextCustom.Items.Remove(enabledToolStripMenuItem);

            panelSidebar.Visible = false;
            panelSidebarLine.Visible = false;

            this.screens = new Dictionary<Rectangle, int>();
            this.master = master;

            var screens = Screen.AllScreens;
            checkScreen = new CheckBox[screens.Length];
            checkScreen[0] = checkScreen1;
            checkScreen1.Tag = 0;
            basicTemplates = new TemplateDisplay[screens.Length];

            panelScreen.SuspendLayout();
            for (var i = 0; i < screens.Length; i++)
            {
                this.screens[screens[i].Bounds] = i;

                if (i != 0)
                {
                    var c = new CheckBox()
                    {
                        Text = "Screen " + (i + 1),
                        Margin = checkFlipX.Margin,
                        AutoSize = checkScreen1.AutoSize,
                        UseVisualStyleBackColor = checkScreen1.UseVisualStyleBackColor,
                        Tag = i,
                        AutoCheck = false,
                    };
                    c.Click += checkScreen_Click;
                    checkScreen[i] = c;
                    panelScreen.Controls.Add(c);
                }
            }
            panelScreen.ResumeLayout();
        }

        /// <summary>
        /// Initializes the templates form to work with the window manager
        /// </summary>
        public formTemplates()
        {
            mode = TemplateMode.Manager;

            InitializeComponents();

            this.ShowInTaskbar = true;
            this.ShowIcon = true;
            this.Text = "Templates";
            this.MinimizeBox = true;

            panelTemplateWindowOptions.Visible = false;
            toolStripMenuItem1.Visible = true;
            contextCustom.Closed += contextCustom_Closed;

            SelectTab(buttonManager);
        }

        void WindowManagerOptions_ValueChanged(object sender, EventArgs e)
        {
            if (Util.Invoke.IfRequired(this,
                delegate
                {
                    WindowManagerOptions_ValueChanged(sender, e);
                }))
                return;

            var v = ((Settings.ISettingValue<Settings.WindowManagerFlags>)sender).Value;

            checkDelayLaunching.Checked = (v & Settings.WindowManagerFlags.DelayLaunchUntilAvailable) != 0;
            checkReorderOnRelease.Checked = (v & Settings.WindowManagerFlags.ReorderOnRelease) != 0;
            checkAllowActiveChanges.Checked = (v & Settings.WindowManagerFlags.AllowActiveChanges) != 0;
        }

        protected override void OnInitializeComponents()
        {
            base.OnInitializeComponents();

            InitializeComponent();
        }

        protected override void OnShown(EventArgs e)
        {
            panelTemplates.SuspendLayout();

            var screen = Screen.PrimaryScreen.Bounds;

            defaultDisplaySize = Util.RectangleConstraint.Scale(screen.Size, new Size(Scale(80), Scale(80)));
            defaultAssignedSize = Util.RectangleConstraint.Scale(screen.Size, new Size(Scale(120), Scale(120)));
            assignedTextHeight = this.FontHeight * 2;

            labelNoTemplates.MinimumSize = new System.Drawing.Size(0, defaultAssignedSize.Height + assignedTextHeight);

            defaultValues = new Dictionary<Template, object[]>();
            customTemplates = new Dictionary<Settings.WindowTemplate, TemplateDisplay>();
            assignedTemplates = new Dictionary<Settings.WindowTemplate.Assignment, TemplateDisplay>();

            foreach (var t in CreateTemplates())
            {
                var td = CreateDisplay();

                td.TemplateData = t;
                td.Template = new ScreenTemplate(new Settings.WindowTemplate.Screen(screen, t.Create(screen.Size)));

                if (mode == TemplateMode.Manager)
                {
                    td.Visible = false;
                }

                defaultValues[t] = t.Variables.GetValues();

                panelTemplates.Controls.Add(td);
            }

            var custom = Settings.WindowTemplates.Count;

            if (custom > 0)
            {
                for (var i = 0; i < custom; i++)
                {
                    OnTemplateAdded(Settings.WindowTemplates[i]);
                }
            }

            panelTemplates.ResumeLayout();

            Settings.WindowTemplates.ValueAdded += WindowTemplates_ValueAdded;
            Settings.WindowTemplates.ValueRemoved += WindowTemplates_ValueRemoved;

            if (mode == TemplateMode.Manager)
            {
                Settings.WindowManagerOptions.ValueChanged += WindowManagerOptions_ValueChanged;
                WindowManagerOptions_ValueChanged(Settings.WindowManagerOptions, null);
            }

            base.OnShown(e);
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

        private void OnTemplateRemoved(Settings.WindowTemplate t)
        {
            if (mode == TemplateMode.Manager)
            {
                t.Assignments.ValueAdded -= Assigned_ValueAdded;
                t.Assignments.ValueRemoved -= Assigned_ValueRemoved;
            }

            TemplateDisplay td;

            if (customTemplates.TryGetValue(t, out td))
            {
                customTemplates.Remove(t);

                t.ScreensChanged -= template_ScreensChanged;

                for (var i = t.Assignments.Count - 1; i >= 0; --i)
                {
                    t.Assignments[i].EnabledChanged -= assigned_EnabledChanged;
                }

                panelTemplates.SuspendLayout();

                if (td.Shared != null)
                {
                    foreach (var c in td.Shared)
                    {
                        if (c.Selected)
                        {
                            Selected = null;
                            LoadOptions(null);
                        }
                        panelTemplates.Controls.Remove(c);
                        c.Dispose();
                    }
                }
                else
                {
                    if (td.Selected)
                    {
                        Selected = null;
                        LoadOptions(null);
                    }
                    panelTemplates.Controls.Remove(td);
                    td.Dispose();
                }

                panelTemplates.ResumeLayout();
            }
        }

        private void OnTemplateAdded(Settings.WindowTemplate t)
        {
            panelTemplates.SuspendLayout();

            var td = CreateDisplay();

            customTemplates[t] = td;

            t.ScreensChanged += template_ScreensChanged;

            td.Source = t;
            td.Template = new ScreenTemplate(t);

            panelTemplates.Controls.Add(td);

            if (mode == TemplateMode.Manager)
            {
                td.Shared = new List<TemplateDisplay>();
                td.Shared.Add(td);

                td.Visible = IsFilterVisible(td);

                t.Assignments.ValueAdded += Assigned_ValueAdded;
                t.Assignments.ValueRemoved += Assigned_ValueRemoved;

                var assigned = t.Assignments;
                var count = assigned.Count;

                for (var j = 0; j < count; j++)
                {
                    OnAssignedAdded(td, assigned[j]);
                }
            }

            if (TemplateDisplayAdded != null)
                TemplateDisplayAdded(this, td);

            panelTemplates.ResumeLayout();
        }

        void template_ScreensChanged(object sender, EventArgs e)
        {
            var t = (Settings.WindowTemplate)sender;
            TemplateDisplay parent;

            if (customTemplates.TryGetValue(t, out parent))
            {
                var st = new ScreenTemplate(t);

                if (parent.Shared != null)
                {
                    foreach (var td in parent.Shared)
                    {
                        td.Template = st;
                    }
                }
                else
                {
                    parent.Template = st;
                }
            }
        }

        private void OnAssignedAdded(TemplateDisplay parent, Settings.WindowTemplate.Assignment a)
        {
            var previous = parent.Shared[parent.Shared.Count - 1];
            var td = CreateDisplay();
            bool v;

            assignedTemplates[a] = td;

            a.EnabledChanged += assigned_EnabledChanged;

            td.Source = parent.Source;
            td.Template = parent.Template;
            td.AssignedTo = a;
            td.Shared = parent.Shared;
            td.Shared.Add(td);
            td.Visible = v = IsFilterVisible(td);
            td.Activated = a.Enabled;
            td.TextHeight = assignedTextHeight;
            td.Size = new System.Drawing.Size(defaultAssignedSize.Width, defaultAssignedSize.Height + assignedTextHeight);
            td.Text = a.Name;

            td.MouseUp += assigned_MouseUp;

            ShowNoTemplates = _ShowNoTemplates && !v;

            panelTemplates.SuspendLayout();

            panelTemplates.Controls.Add(td);
            panelTemplates.Controls.SetChildIndex(td, panelTemplates.Controls.GetChildIndex(previous) + 1);

            panelTemplates.ResumeLayout();

            if (TemplateDisplayAdded != null)
                TemplateDisplayAdded(this, td);
        }

        private void OnAssignedRemoved(Settings.WindowTemplate.Assignment a)
        {
            TemplateDisplay td;
            if (assignedTemplates.TryGetValue(a, out td))
            {
                assignedTemplates.Remove(a);

                a.EnabledChanged -= assigned_EnabledChanged;

                if (td.Selected)
                {
                    Selected = null;
                    LoadOptions(null);
                }

                td.Shared.Remove(td);
                panelTemplates.Controls.Remove(td);
                td.Dispose();

                if (buttonManager.Selected)
                {
                    var b = true;

                    foreach (var t in assignedTemplates.Values)
                    {
                        if (t.Visible)
                        {
                            b = false;
                            break;
                        }
                    }

                    ShowNoTemplates = b;
                }
            }
        }

        void contextCustom_Closed(object sender, ToolStripDropDownClosedEventArgs e)
        {
            var c = _Selected;
            if (c != null && c.AssignedTo != null)
            {
                Selected = null;
            }
        }

        private TemplateDisplay FindAssigned(object l)
        {
            foreach (var td in customTemplates.Values)
            {
                if (object.ReferenceEquals(td.Source.Assignments, l))
                {
                    return td;
                }
            }

            return null;
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                Settings.WindowTemplates.ValueAdded -= WindowTemplates_ValueAdded;
                Settings.WindowTemplates.ValueRemoved -= WindowTemplates_ValueRemoved;

                if (mode == TemplateMode.Manager)
                    Settings.WindowManagerOptions.ValueChanged -= WindowManagerOptions_ValueChanged;

                if (customTemplates != null)
                {
                    foreach (var t in customTemplates.Keys)
                    {
                        if (mode == TemplateMode.Manager)
                        {
                            t.Assignments.ValueAdded -= Assigned_ValueAdded;
                            t.Assignments.ValueRemoved -= Assigned_ValueRemoved;
                            for (var i = t.Assignments.Count - 1; i >= 0; --i)
                            {
                                t.Assignments[i].EnabledChanged -= assigned_EnabledChanged;
                            }
                        }
                        t.ScreensChanged -= template_ScreensChanged;
                    }
                }

                if (displayManager != null)
                {
                    displayManager.Dispose();
                    displayManager = null;
                    if (TemplateDisplayManagerChanged != null)
                        TemplateDisplayManagerChanged(this, displayManager);
                }

                using ((Form)labelOptions.Tag) { }

                if (components != null)
                {
                    components.Dispose();
                }
            }

            base.Dispose(disposing);
        }

        private TemplateDisplay CreateDisplay()
        {
            var td = new TemplateDisplay()
            {
                Margin = Padding.Empty,
                Size = defaultDisplaySize,
                Padding = new Padding(Scale(3)),
            };

            td.MouseDown += td_MouseDown;

            return td;
        }

        public TemplateDisplayManager DisplayManager
        {
            get
            {
                return displayManager;
            }
        }

        private TemplateDisplay _Selected;
        private TemplateDisplay Selected
        {
            get
            {
                return _Selected;
            }
            set
            {
                if (_Selected != value)
                {
                    if (_Selected != null)
                    {
                        _Selected.Selected = false;
                    }
                    if (value != null)
                    {
                        value.Selected = true;
                    }
                    _Selected = value;
                }
            }
        }

        void td_MouseDown(object sender, MouseEventArgs e)
        {
            var td = (TemplateDisplay)sender;

            td.Focus();

            if (td.AssignedTo == null && _Selected != td)
            {
                Selected = td;
                LoadOptions(td);
            }

            if (e.Button == System.Windows.Forms.MouseButtons.Right)
            {
                if (td.TemplateData == null)
                {
                    var hasSource = td.Source != null;
                    var isAssigned = td.AssignedTo != null;

                    toolStripMenuItem1.Visible = hasSource || isAssigned;
                    snapToToolStripMenuItem.Visible = hasSource;

                    if (hasSource)
                    {
                        windowEdgesToolStripMenuItem.Checked = td.Source.SnapToEdges == Settings.WindowTemplate.SnapType.WindowEdgeInner;
                        windowEdgesOuterToolStripMenuItem.Checked = td.Source.SnapToEdges == Settings.WindowTemplate.SnapType.WindowEdgeOuter;
                        clientEdgesToolStripMenuItem.Checked = td.Source.SnapToEdges == Settings.WindowTemplate.SnapType.ClientEdge;
                        snapToToolStripMenuItem.Checked = td.Source.SnapToEdges != Settings.WindowTemplate.SnapType.None;
                    }

                    toolStripMenuItem2.Visible = isAssigned;
                    enabledToolStripMenuItem.Visible = isAssigned;
                    renameToolStripMenuItem.Visible = isAssigned;
                    accountTypeToolStripMenuItem.Visible = isAssigned;

                    if (isAssigned)
                    {
                        Selected = td;

                        enabledToolStripMenuItem.Checked = td.AssignedTo.Enabled;
                        guildWars1ToolStripMenuItem.Checked = td.AssignedTo.Type == Settings.WindowTemplate.Assignment.AccountType.GuildWars1;
                        guildWars2ToolStripMenuItem.Checked = td.AssignedTo.Type == Settings.WindowTemplate.Assignment.AccountType.GuildWars2;
                        accountTypeToolStripMenuItem.Checked = td.AssignedTo.Type != Settings.WindowTemplate.Assignment.AccountType.Any;
                    }

                    contextCustom.Tag = td;
                    contextCustom.Show(Cursor.Position);
                }
            }
        }

        private void LoadOptions(TemplateDisplay td)
        {
            panelOptionsContainer.SuspendLayout();
            panelOptions.SuspendLayout();

            bool isEmpty = td == null,
                 isCustom = !isEmpty && td.TemplateData == null;

            checkFlipX.Checked = !isEmpty && td.FlipHorizontal;
            checkFlipY.Checked = !isEmpty && td.FlipVertical;

            if (checkScreen != null)
            {
                for (var i = checkScreen.Length - 1; i >= 0; --i)
                {
                    var c = checkScreen[i];
                    c.Enabled = !isCustom || td.Template.Screens.Length == 1 || GetTemplateScreenIndex(td.Template, i) != -1;
                    if (c.Enabled && !isEmpty && basicTemplates[i] == td)
                        c.CheckState = CheckState.Checked;
                    else if (basicTemplates[i] != null)
                        c.CheckState = CheckState.Indeterminate;
                    else
                        c.CheckState = CheckState.Unchecked;
                }
            }

            panelOptions.Visible = false;

            foreach (Control c in panelOptions.Controls)
            {
                c.Dispose();
            }
            panelOptions.Controls.Clear();

            Template t;
            if (td != null)
                t = td.TemplateData;
            else
                t = null;

            checkOverlapTaskbar.Enabled = !isCustom;

            panelCommands.SuspendLayout();
            labelCustomizeSep.Visible = !isEmpty;
            labelReset.Visible = !isEmpty && !isCustom;
            labelDelete.Visible = !isEmpty && isCustom;
            panelCommands.ResumeLayout();

            if (t == null)
            {
                panelOptions.ResumeLayout();
                panelOptionsContainer.ResumeLayout();
                return;
            }

            var vs = t.Variables;
            var vars = vs.Variables;
            var controls = new List<Control>();

            for (var i = 0; i < vars.Length; i++)
            {
                Control c;

                switch (vars[i].Type)
                {
                    case VariableType.CountAsByte:
                        {
                            var index = i;

                            var n = new Gw2Launcher.UI.Controls.IntegerTextBox()
                            {
                                Minimum = vs.GetMinimum<byte>(i),
                                Maximum = 6,
                                Value = vs.GetValue<byte>(i),
                                Anchor = textTemplate.Anchor,
                                Size = textTemplate.Size,
                                Margin = textTemplate.Margin,
                            };

                            n.ValueChanged += delegate
                            {
                                vs.SetVariable<byte>(index, (byte)n.Value);
                                var bounds = td.Template.Screens[0].Bounds;
                                td.Template = new ScreenTemplate(new Settings.WindowTemplate.Screen(bounds, t.Create(bounds.Size)));
                                OnTemplateChanged();
                            };

                            c = n;
                        }
                        break;
                    case VariableType.PercentAsFloat:
                        {
                            var index = i;

                            var n = new Gw2Launcher.UI.Controls.IntegerTextBox()
                            {
                                Minimum = (int)(vs.GetMinimum<float>(i) * 100 + 0.5f),
                                Maximum = 100,
                                Value = (int)(vs.GetValue<float>(i) * 100 + 0.5f),
                                Anchor = textTemplate.Anchor,
                                Size = textTemplate.Size,
                                Margin = textTemplate.Margin,
                            };

                            n.ValueChanged += delegate
                            {
                                vs.SetVariable<float>(index, (float)(n.Value / 100f));
                                var bounds = td.Template.Screens[0].Bounds;
                                td.Template = new ScreenTemplate(new Settings.WindowTemplate.Screen(bounds, t.Create(bounds.Size)));
                                OnTemplateChanged();
                            };

                            c = n;
                        }
                        break;
                    default:
                        Util.Logging.Log("Unknown template variable: " + vars[i].Type);
                        continue;
                }

                var l = new Label()
                {
                    Text = vars[i].Name,
                    AutoSize = true,
                    Anchor = labelTemplate.Anchor,
                    Margin = labelTemplate.Margin,
                };

                panelOptions.Controls.Add(l);
                panelOptions.Controls.Add(c);
            }

            panelOptions.ResumeLayout();

            if (panelOptions.Controls.Count > 0)
                panelOptions.Visible = true;

            panelOptionsContainer.ResumeLayout();
        }

        private async void DelayedTemplateChanged()
        {
            do
            {
                var t = delayedChange;

                OnDisplayedTemplateUpdate();

                await Task.Delay(100);

                if (delayedChange == t)
                {
                    delayedChange = 0;
                    break;
                }
                else
                {
                    t = delayedChange;
                }
            }
            while (true);
        }

        private void OnDisplayedTemplateUpdate()
        {
            var useWorking = checkOverlapTaskbar.Enabled && !checkOverlapTaskbar.Checked;

            for (var i = 0; i < basicTemplates.Length; i++)
            {
                if (basicTemplates[i] == null)
                    continue;

                var t = basicTemplates[i].Template;

                if (t.Screens.Length == 1)
                {
                    var screens = Screen.AllScreens;
                    Screen screen;
                    if (i < screens.Length)
                        screen = screens[i];
                    else
                        screen = screens[0];
                    displayManager.Set(i, t.GetWindows(useWorking ? screen.WorkingArea : screen.Bounds, checkFlipX.Checked, checkFlipY.Checked));
                    displayManager.Show();
                }
                else
                {
                    var j = GetTemplateScreenIndex(t, i);

                    if (j != -1)
                    {
                        var screen = Screen.FromRectangle(t.Screens[j].Bounds);
                        displayManager.Set(i, t.GetWindows(j, useWorking ? screen.WorkingArea : screen.Bounds, checkFlipX.Checked, checkFlipY.Checked));
                        displayManager.Show();
                    }
                    else
                    {
                        displayManager.Remove(i);
                    }
                }
            }
        }

        private void OnTemplateChanged()
        {
            if (displayManager == null)
                return;

            if (delayedChange == 0)
            {
                delayedChange = Environment.TickCount;
                DelayedTemplateChanged();
            }
            else
            {
                delayedChange = Environment.TickCount;
            }
        }

        private List<Template> CreateTemplates()
        {
            var templates = new List<Template>(10);

            //columns
            templates.Add(new Template(
                new TemplateVariables(new TemplateVariable<byte>(VariableType.CountAsByte, "Columns", 2, 1)
                ),
                delegate(Template t, Size s)
                {
                    return CreateGridTemplate(s, t.Variables.GetValue<byte>(0), 1);
                }));

            //rows
            templates.Add(new Template(new TemplateVariables(
                new TemplateVariable<byte>(VariableType.CountAsByte, "Rows", 2, 1)
                ),
                delegate(Template t, Size s)
                {
                    return CreateGridTemplate(s, 1, t.Variables.GetValue<byte>(0));
                }));

            //columns x rows
            templates.Add(new Template(new TemplateVariables(
                new TemplateVariable<byte>(VariableType.CountAsByte, "Columns", 2, 1),
                new TemplateVariable<byte>(VariableType.CountAsByte, "Rows", 2, 1)),
                delegate(Template t, Size s)
                {
                    return CreateGridTemplate(s, t.Variables.GetValue<byte>(0), t.Variables.GetValue<byte>(1));
                }));

            //2 columns, variable rows
            templates.Add(new Template(new TemplateVariables(
                new TemplateVariable<byte>(VariableType.CountAsByte, "Left rows", 1, 1),
                new TemplateVariable<byte>(VariableType.CountAsByte, "Right rows", 2, 1),
                new TemplateVariable<float>(VariableType.PercentAsFloat, "Size", 0.5f, 0f)
                ),
                delegate(Template t, Size s)
                {
                    var v = t.Variables;
                    return CreateColumnsTemplate(s, TemplateLayout.Columns,
                        new Column[]
                        {
                            new Column(TemplateLayout.Rows, v.GetValue<float>(2), v.GetValue<byte>(0)),
                            new Column(TemplateLayout.Rows, 1f - v.GetValue<float>(2), v.GetValue<byte>(1)),
                        });
                }));

            //2 rows, variable columns
            templates.Add(new Template(new TemplateVariables(
                new TemplateVariable<byte>(VariableType.CountAsByte, "Top columns", 1, 1),
                new TemplateVariable<byte>(VariableType.CountAsByte, "Bottom columns", 2, 1),
                new TemplateVariable<float>(VariableType.PercentAsFloat, "Size", 0.5f, 0f)
                ),
                delegate(Template t, Size s)
                {
                    var v = t.Variables;
                    return CreateColumnsTemplate(s, TemplateLayout.Rows,
                        new Column[]
                        {
                            new Column(TemplateLayout.Columns, v.GetValue<float>(2), v.GetValue<byte>(0)),
                            new Column(TemplateLayout.Columns, 1f - v.GetValue<float>(2), v.GetValue<byte>(1)),
                        });
                }));

            //2 columns: 65%, 35% variable rows
            templates.Add(new Template(new TemplateVariables(
                new TemplateVariable<byte>(VariableType.CountAsByte, "Rows", 2, 1),
                new TemplateVariable<float>(VariableType.PercentAsFloat, "Size", 0.65f, 0f)
                ),
                delegate(Template t, Size s)
                {
                    var v = t.Variables;
                    return CreateColumnsTemplate(s, TemplateLayout.Columns,
                        new Column[]
                        {
                            new Column(TemplateLayout.Rows, v.GetValue<float>(1), 1),
                            new Column(TemplateLayout.Rows, 1f - v.GetValue<float>(1), v.GetValue<byte>(0)),
                        });
                }));

            //3 columns: 27% variable rows, 45%, 27% varialbe rows
            templates.Add(new Template(new TemplateVariables(
                new TemplateVariable<byte>(VariableType.CountAsByte, "Rows", 2, 1),
                new TemplateVariable<float>(VariableType.PercentAsFloat, "Size", 0.45f, 0f)
                ),
                delegate(Template t, Size s)
                {
                    var v = t.Variables;
                    var ssize = (1f - v.GetValue<float>(1)) / 2;
                    return CreateColumnsTemplate(s, TemplateLayout.Columns,
                        new Column[]
                        {
                            new Column(TemplateLayout.Rows, ssize, v.GetValue<byte>(0)),
                            new Column(TemplateLayout.Rows, v.GetValue<float>(1), 1),
                            new Column(TemplateLayout.Rows, ssize, v.GetValue<byte>(0)),
                        });
                }));

            //3 columns, 20% variable rows, 60%, 20% variable rows
            templates.Add(new Template(new TemplateVariables(
                new TemplateVariable<byte>(VariableType.CountAsByte, "Side rows", 2, 1),
                new TemplateVariable<byte>(VariableType.CountAsByte, "Middle columns", 2, 1),
                new TemplateVariable<float>(VariableType.PercentAsFloat, "Middle width", 0.5f, 0f),
                new TemplateVariable<float>(VariableType.PercentAsFloat, "Middle height", 0.6f, 0f)
                ),
                delegate(Template t, Size s)
                {
                    var v = t.Variables;
                    var sidesW = (1f - v.GetValue<float>(2)) / 2;
                    return CreateColumnsTemplate(s, TemplateLayout.Columns,
                        new Column[]
                        {
                            new Column(TemplateLayout.Rows, sidesW, v.GetValue<byte>(0)),
                            new Column(TemplateLayout.Columns, v.GetValue<float>(2), new float[] { v.GetValue<float>(3), 1f - v.GetValue<float>(3) }, new byte[] { 1, v.GetValue<byte>(1) }),
                            new Column(TemplateLayout.Rows, sidesW, v.GetValue<byte>(0)),
                        });
                }));

            //3 columns, 27% variable rows, 45%, 27% variable rows
            templates.Add(new Template(new TemplateVariables(
                new TemplateVariable<byte>(VariableType.CountAsByte, "Side rows", 3, 1),
                new TemplateVariable<byte>(VariableType.CountAsByte, "Middle columns", 2, 1),
                new TemplateVariable<float>(VariableType.PercentAsFloat, "Middle width", 0.5f, 0f),
                new TemplateVariable<float>(VariableType.PercentAsFloat, "Middle height", 0.45f, 0f)
                ),
                delegate(Template t, Size s)
                {
                    var v = t.Variables;
                    var sidesW = (1f - v.GetValue<float>(2)) / 2;
                    var middleH = (1f - v.GetValue<float>(3)) / 2;
                    return CreateColumnsTemplate(s, TemplateLayout.Columns,
                        new Column[]
                        {
                            new Column(TemplateLayout.Rows, sidesW, v.GetValue<byte>(0)),
                            new Column(TemplateLayout.Columns, v.GetValue<float>(2), new float[] { middleH, v.GetValue<float>(3), middleH }, new byte[] { v.GetValue<byte>(1), 1, v.GetValue<byte>(1) }),
                            new Column(TemplateLayout.Rows, sidesW, v.GetValue<byte>(0)),
                        });
                }));

            //2 columns, 67%, 33% variable rows
            templates.Add(new Template(new TemplateVariables(
                new TemplateVariable<byte>(VariableType.CountAsByte, "Rows", 3, 1),
                new TemplateVariable<byte>(VariableType.CountAsByte, "Columns", 2, 1),
                new TemplateVariable<float>(VariableType.PercentAsFloat, "Middle size", 0.67f, 0f)
                ),
                delegate(Template t, Size s)
                {
                    var v = t.Variables;
                    var rows = v.GetValue<byte>(0);
                    var ch = rows == 1 ? 0.3f : 1f / rows;

                    return CreateColumnsTemplate(s, TemplateLayout.Columns,
                        new Column[]
                        {
                            new Column(TemplateLayout.Columns, v.GetValue<float>(2), new float[] { 1f-ch, ch }, new byte[] { 1, v.GetValue<byte>(1) }),
                            new Column(TemplateLayout.Rows, 1f - v.GetValue<float>(2), rows),
                        });
                }));

            return templates;
        }

        private Rectangle[] CreateColumnsTemplate(Size size, TemplateLayout layout, Column[] columns)
        {
            var sections = new TemplateSection[columns.Length];

            for (var i = sections.Length - 1; i >= 0; --i)
            {
                var c = columns[i];
                var areas = new TemplateSection.TemplateArea[c.heights.Length];

                for (var j = areas.Length - 1; j >= 0; --j)
                {
                    areas[j] = new TemplateSection.TemplateArea(c.heights[j], c.layout, c.counts[j]);
                }

                sections[i] = new TemplateSection(c.width, areas);
            }

            return CreateTemplate(size, layout, sections);
        }

        private Rectangle[] CreateGridTemplate(Size size, byte columns, byte rows)
        {
            var sections = new TemplateSection[columns];

            for (var i = sections.Length - 1; i >= 0; --i)
            {
                sections[i] = new TemplateSection(1f / columns, new TemplateSection.TemplateArea(1f, TemplateLayout.Rows, rows));
            }

            return CreateTemplate(size, TemplateLayout.Columns, sections);
        }

        /// <summary>
        /// Fills the specified bounds with rectangles
        /// </summary>
        /// <param name="rects">Output array</param>
        /// <param name="index">Index where output begins</param>
        /// <param name="count">Number of rectangles to create</param>
        private void Fill(Rectangle bounds, TemplateLayout layout, Rectangle[] rects, int index, int count)
        {
            if (count <= 1)
            {
                if (count != 0)
                    rects[index] = bounds;
                return;
            }

            if (layout == TemplateLayout.Rows)
            {
                var w = bounds.Width;
                var x = bounds.X;
                var y = bounds.Y;
                var h = (float)bounds.Height / count;

                for (var i = 0; i < count; i++)
                {
                    var y1 = y + (int)(i * h);
                    var y2 = i == count - 1 ? bounds.Bottom : y + (int)((i + 1) * h);

                    rects[i + index] = new Rectangle(x, y1, w, y2 - y1);
                }
            }
            else
            {
                var w = (float)bounds.Width / count;
                var x = bounds.X;
                var y = bounds.Y;
                var h = bounds.Height;

                for (var i = 0; i < count; i++)
                {
                    var x1 = x + (int)(i * w);
                    var x2 = i == count - 1 ? bounds.Right : x + (int)((i + 1) * w);

                    rects[i + index] = new Rectangle(x1, y, x2 - x1, h);
                }
            }
        }

        private Rectangle[] CreateTemplate(Size screen, TemplateLayout layout, params TemplateSection[] sections)
        {
            var sizes = GetSizing(layout == TemplateLayout.Columns ? screen.Width : screen.Height, sections);
            var rects = new Rectangle[GetCount(sections)];
            var innersize = layout == TemplateLayout.Columns ? screen.Height : screen.Width;
            var index = 0;
            int x = 0,
                y = 0;

            if (layout == TemplateLayout.Columns)
            {
                for (var i = 0; i < sizes.Length; i++)
                {
                    var areas = sections[i].Areas;
                    var innersizes = GetSizing(innersize, areas);

                    for (var j = 0; j < innersizes.Length; j++)
                    {
                        var count = areas[j].Count;

                        Fill(new Rectangle(x, y, sizes[i], innersizes[j]), areas[j].AreaLayout, rects, index, count);

                        index += count;
                        y += innersizes[j];
                    }

                    x += sizes[i];
                    y = 0;
                }
            }
            else
            {
                for (var i = 0; i < sizes.Length; i++)
                {
                    var areas = sections[i].Areas;
                    var innersizes = GetSizing(innersize, areas);

                    for (var j = 0; j < innersizes.Length; j++)
                    {
                        var count = areas[j].Count;

                        Fill(new Rectangle(x, y, innersizes[j], sizes[i]), areas[j].AreaLayout, rects, index, count);

                        index += count;
                        x += innersizes[j];
                    }

                    x = 0;
                    y += sizes[i];
                }
            }

            return rects;
        }

        private int[] GetSizing(int total, ITemplateSize[] sections)
        {
            var sizes = new int[sections.Length];
            var max = 0f;
            var maxi = -1;
            var remaining = total;

            for (var i = 0; i < sections.Length; i++)
            {
                var sz = sections[i].Size;

                if (sz >= max)
                {
                    maxi = i;
                    max = sz;
                }

                sizes[i] = (int)(sz * total);
                remaining -= sizes[i];
            }

            sizes[maxi] += remaining;

            return sizes;
        }

        private int GetCount(TemplateSection[] sections)
        {
            var count = 0;

            foreach (var s in sections)
            {
                count += s.Count;
            }

            return count;
        }

        private void labelCustomize_Click(object sender, EventArgs e)
        {
            ShowCustom(_Selected, _Selected != null && _Selected.Source != null);
        }

        private void labelReset_Click(object sender, EventArgs e)
        {
            if (_Selected != null && _Selected.TemplateData != null)
            {
                object[] values;
                if (defaultValues.TryGetValue(_Selected.TemplateData, out values))
                {
                    _Selected.TemplateData.Variables.SetValues(values);
                    var bounds = _Selected.Template.Screens[0].Bounds;
                    _Selected.Template = new ScreenTemplate(new Settings.WindowTemplate.Screen(bounds, _Selected.TemplateData.Create(bounds.Size)));
                    LoadOptions(_Selected);
                    OnTemplateChanged();
                }
            }
        }

        private void checkFlipY_CheckedChanged(object sender, EventArgs e)
        {
            if (_Selected != null)
            {
                _Selected.FlipVertical = checkFlipY.Checked;
                OnTemplateChanged();
            }
        }

        private void checkFlipX_CheckedChanged(object sender, EventArgs e)
        {
            if (_Selected != null)
            {
                _Selected.FlipHorizontal = checkFlipX.Checked;
                OnTemplateChanged();
            }
        }

        private void labelCustomNew_Click(object sender, EventArgs e)
        {
            ShowCustom(null, formCustomTemplate.BoundsType.Unknown, null, null, null, false, mode == TemplateMode.Manager, false, null);
        }

        private void labelCustomNewFromLayout_Click(object sender, EventArgs e)
        {
            Rectangle[] rects;
            Settings.WindowTemplate.Assignment assigned = null;
            var type = formCustomTemplate.BoundsType.Unknown;

            if (master == null)
            {
                var active = Client.Launcher.GetActiveProcesses();
                rects = new Rectangle[active.Count];
                var uids = new ushort[rects.Length];
                var border = (this.Width - this.ClientSize.Width) / 2 - 1;
                var i = 0;

                foreach (var a in active)
                {
                    if (Client.Launcher.GetState(a) == Client.Launcher.AccountState.ActiveGame)
                    {
                        var p = Client.Launcher.GetProcess(a);
                        if (p != null)
                        {
                            try
                            {
                                var h = Windows.FindWindow.FindMainWindow(p);
                                if (Windows.WindowLong.HasValue(h, GWL.GWL_STYLE, WindowStyle.WS_MINIMIZEBOX))
                                {
                                    var placement = Windows.WindowSize.GetWindowPlacement(h);
                                    var r = Util.ScreenUtil.FromDesktopBounds(placement.rcNormalPosition.ToRectangle());

                                    rects[i] = r;
                                    uids[i] = a.UID;

                                    ++i;
                                }
                            }
                            catch { }
                        }
                    }
                }

                if (i > 0)
                {
                    type = formCustomTemplate.BoundsType.WindowBounds;

                    if (i != rects.Length)
                    {
                        Array.Resize(ref rects, i);
                    }

                    var accounts = new KeyValuePair<byte, Settings.WindowTemplate.Assigned>[i];

                    for (var j = 0; j < i; j++)
                    {
                        accounts[j] = new KeyValuePair<byte, Settings.WindowTemplate.Assigned>((byte)j, new Settings.WindowTemplate.Assigned(Settings.WindowTemplate.Assigned.AssignedType.Account, new ushort[] { uids[j] }));
                    }

                    assigned = new Settings.WindowTemplate.Assignment()
                    {
                        Assigned = accounts,
                    };
                }
                else
                {
                    MessageBox.Show(this, "No windowed accounts are currently active", "No windows found", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                    return;
                    //rects = null;
                }
            }
            else
            {
                var w = master.GetWindows();
                rects = new Rectangle[w.Count];
                var i = 0;

                if (w.Count > 0)
                {
                    if (checkSnapEdge.Checked)
                        type = formCustomTemplate.BoundsType.ClientBounds;
                    else
                        type = formCustomTemplate.BoundsType.WindowBounds;
                }

                foreach (var r in w)
                {
                    if (checkSnapEdge.Checked)
                    {
                        var border = (r.Width - r.ClientSize.Width) / 2 - 1;
                        rects[i++] = new Rectangle(r.Left + border, r.Top, r.Width - border * 2, r.Height - border);
                    }
                    else
                    {
                        rects[i++] = r.Bounds;
                    }
                }
            }

            ShowCustom(null, type, rects, null, null, false, mode == TemplateMode.Manager, false, assigned);
        }

        private void ShowCustom(TemplateDisplay td, bool overwrite)
        {
            Rectangle[] windows = null;
            Settings.WindowTemplate.Assignment assigned = null;
            byte[] keys = null;
            byte[] order = null;

            if (td != null)
            {
                if (td.TemplateData != null)
                    windows = td.Template.GetWindows(new Rectangle[] { checkOverlapTaskbar.Checked ? Screen.FromControl(this).Bounds : Screen.FromControl(this).WorkingArea }, checkFlipX.Checked, checkFlipY.Checked);
                else
                    windows = td.Template.GetWindows(false, checkFlipX.Checked, checkFlipY.Checked);

                if (td.Source != null)
                {
                    assigned = td.AssignedTo;
                    order = td.Source.Order;
                    keys = td.Source.Keys;
                }
            }

            ShowCustom(td, formCustomTemplate.BoundsType.Unknown, windows, keys, order, overwrite, mode == TemplateMode.Manager, checkFlipX.Checked || checkFlipY.Checked, assigned);
        }

        private void ShowCustom(TemplateDisplay td, formCustomTemplate.BoundsType type, Rectangle[] rects, byte[] keys, byte[] order, bool canOverwrite, bool canAssign, bool isModified, Settings.WindowTemplate.Assignment assigned)
        {
            if (displayManager != null)
                displayManager.Hide();

            var isCustomSource = td != null && td.TemplateData == null;
            var referenceCount = isCustomSource ? td.Source.Assignments.Count : 0;
            var canOverwriteAssign = isCustomSource && assigned != null && td.AssignedTo == assigned;

            using (var f = new formCustomTemplate(master != null ? master.MinimumSize : Size.Empty, type, rects, keys, order, isModified, referenceCount, canAssign, assigned != null ? assigned.Assigned : null, assigned != null ? assigned.Type : Settings.WindowTemplate.Assignment.AccountType.Any))
            {
                f.Overwrite = canOverwrite ? formCustomTemplate.SaveMode.Overwrite : formCustomTemplate.SaveMode.New;
                f.CanOverwriteAssign = canOverwriteAssign;
                f.ShowModificationWarning = referenceCount > (canOverwriteAssign ? 1 : 0);

                if (f.ShowDialog(this) == System.Windows.Forms.DialogResult.OK)
                {
                    if (f.Result == null || f.Result.Screens.Length == 0)
                        return;

                    var stt = new Settings.WindowTemplate.Screen[f.Result.Screens.Length];
                    var addedReference = false;

                    for (var i = f.Result.Screens.Length - 1; i >= 0; --i )
                    {
                        stt[i] = new Settings.WindowTemplate.Screen(f.Result.Screens[i].Screen, f.Result.Screens[i].Windows);
                    }

                    Settings.WindowTemplate source = null;

                    EventHandler<TemplateDisplay> onAdded = delegate(object o, TemplateDisplay t)
                    {
                        if (t.Visible)
                        {
                            Selected = t.AssignedTo == null ? t : null;
                        }
                    };
                    TemplateDisplayAdded += onAdded;

                    if (isCustomSource && td.Source != null && f.Overwrite != formCustomTemplate.SaveMode.New)
                    {
                        source = td.Source;
                        source.Screens = stt;
                    }
                    else
                    {
                        source = new Settings.WindowTemplate(stt)
                        {
                            SnapToEdges = td != null && td.Source != null ? td.Source.SnapToEdges : Settings.WindowTemplate.SnapType.WindowEdgeOuter,
                        };
                    }

                    if (f.Result.BoundsType != formCustomTemplate.BoundsType.Unknown)
                    {
                        source.SnapToEdges = f.Result.BoundsType == formCustomTemplate.BoundsType.ClientBounds ? Settings.WindowTemplate.SnapType.WindowEdgeOuter : Settings.WindowTemplate.SnapType.None;
                    }

                    source.Order = f.Result.Order;
                    source.Keys = f.Result.Keys;

                    panelTemplates.SuspendLayout();

                    if (canAssign)
                    {
                        if (f.Overwrite == formCustomTemplate.SaveMode.New || f.Overwrite == formCustomTemplate.SaveMode.Overwrite && f.Result.Assigned == null && !canOverwriteAssign)
                        {
                            canAssign = false;
                        }
                    }

                    if (canAssign)
                    {
                        if (!canOverwriteAssign || f.Overwrite != formCustomTemplate.SaveMode.Overwrite)
                        {
                            assigned = null;
                        }

                        if (assigned == null)
                        {
                            if (f.Overwrite == formCustomTemplate.SaveMode.Overwrite && f.Result.Assigned == null)
                            {
                                //template is already not assigned
                            }
                            else if (f.Overwrite == formCustomTemplate.SaveMode.Reference || f.Result.Assigned != null)
                            {
                                addedReference = true;
                                
                                if (mode == TemplateMode.Manager)
                                {
                                    SelectTab(buttonManager);
                                }

                                assigned = new Settings.WindowTemplate.Assignment()
                                {
                                    Assigned = f.Result.Assigned,
                                };

                                source.Assignments.Add(assigned);
                            }
                        }
                        else
                        {
                            assigned.Assigned = f.Result.Assigned;
                        }
                    }

                    if (!isCustomSource || f.Overwrite == formCustomTemplate.SaveMode.New)
                    {
                        if (mode == TemplateMode.Manager && !addedReference)
                        {
                            SelectTab(buttonTemplates);
                        }

                        Settings.WindowTemplates.Add(source);
                    }
                    else if (td != null && td.Shared != null)
                    {
                        foreach (var c in td.Shared)
                        {
                            c.Visible = IsFilterVisible(c);
                        }
                    }

                    TemplateDisplayAdded -= onAdded;

                    LoadOptions(Selected);
                    panelTemplates.ResumeLayout();
                    OnTemplateChanged();
                }
            }

            if (displayManager != null)
                displayManager.Show();
        }

        private int GetScreenIndex(Rectangle r)
        {
            int i;

            if (!screens.TryGetValue(r, out i))
            {
                r = Screen.FromRectangle(r).Bounds;

                if (!screens.TryGetValue(r, out i))
                {
                    return -1;
                }

                screens[r] = i;
            }

            return i;
        }

        private int GetTemplateScreenIndex(ScreenTemplate t, int i)
        {
            for (var j = t.Screens.Length - 1; j >= 0; --j)
            {
                if (GetScreenIndex(t.Screens[j].Bounds) == i)
                {
                    return j;
                }
            }

            return -1;
        }

        private void editToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ShowCustom((TemplateDisplay)contextCustom.Tag, true);
        }

        private void deleteToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var c = (TemplateDisplay)contextCustom.Tag;
            if (c != null)
            {
                panelTemplates.SuspendLayout();

                if (c.Source != null)
                {
                    if (c.AssignedTo == null)
                    {
                        var count = c.Source.Assignments.Count;
                        if (count > 0)
                        {
                            if (MessageBox.Show(this, count + " linked " + (count == 1 ? "template" : "templates") + " will be deleted. Are you sure?", "Warning", MessageBoxButtons.YesNo, MessageBoxIcon.Exclamation) != System.Windows.Forms.DialogResult.Yes)
                            {
                                panelTemplates.ResumeLayout();
                                return;
                            }
                        }
                        Settings.WindowTemplates.Remove(c.Source);
                    }
                    else
                    {
                        if (c.Shared != null)
                        {
                            c.Shared.Remove(c);
                        }

                        if (c.AssignedTo.Enabled)
                        {
                            Tools.WindowManager.Instance.Deactivate(c.Source, c.AssignedTo);
                        }

                        c.Source.Assignments.Remove(c.AssignedTo);
                    }
                }

                panelTemplates.ResumeLayout();

                if (_Selected != null)
                {
                    Selected = null;
                    LoadOptions(null);
                }
            }
        }

        private void labelDelete_Click(object sender, EventArgs e)
        {
            contextCustom.Tag = _Selected;
            deleteToolStripMenuItem_Click(sender, e);
        }

        void checkScreen_Click(object sender, EventArgs e)
        {
            var c = (CheckBox)sender;
            var i = (int)c.Tag;

            if (_Selected == null)
            {
                c.CheckState = CheckState.Unchecked;
            }
            else
            {
                switch (c.CheckState)
                {
                    case CheckState.Checked:
                        c.CheckState = CheckState.Unchecked;
                        break;
                    case CheckState.Indeterminate:
                    case CheckState.Unchecked:
                        c.CheckState = CheckState.Checked;
                        break;
                }
            }

            if (c.Checked)
            {
                if (displayManager == null)
                {
                    displayManager = new TemplateDisplayManager(this, screens.Count);
                    displayManager.SnapToEdge = checkSnapEdge.Checked;
                    if (!checkSnapShift.Checked)
                        displayManager.RequiredModifierKey = Keys.None;
                    if (TemplateDisplayManagerChanged != null)
                        TemplateDisplayManagerChanged(this, displayManager);
                }

                if (basicTemplates[i] != _Selected)
                {
                    basicTemplates[i] = _Selected;

                    var useWorking = checkOverlapTaskbar.Enabled && !checkOverlapTaskbar.Checked;

                    if (_Selected.Template.Screens.Length == 1)
                    {
                        var screen = Screen.AllScreens[i];
                        displayManager.Set(i, _Selected.Template.GetWindows(useWorking ? screen.WorkingArea : screen.Bounds, checkFlipX.Checked, checkFlipY.Checked));
                        displayManager.Show();
                    }
                    else
                    {
                        var j = GetTemplateScreenIndex(_Selected.Template, i);

                        if (j != -1)
                        {
                            var screen = Screen.FromRectangle(_Selected.Template.Screens[j].Bounds);
                            displayManager.Set(i, _Selected.Template.GetWindows(j, useWorking ? screen.WorkingArea : screen.Bounds, checkFlipX.Checked, checkFlipY.Checked));
                            displayManager.Show();
                        }
                        else
                        {
                            displayManager.Remove(i);
                        }
                    }
                }
            }
            else if (basicTemplates[i] != null)
            {
                basicTemplates[i] = null;
                displayManager.Remove(i);
            }
        }

        private void checkExcludeTaskbar_CheckedChanged(object sender, EventArgs e)
        {
            if (_Selected != null && _Selected.TemplateData != null)
                OnTemplateChanged();
        }

        private void checkSnapShift_CheckedChanged(object sender, EventArgs e)
        {
            if (displayManager != null)
            {
                if (checkSnapShift.Checked)
                    displayManager.RequiredModifierKey = Keys.Shift | Keys.ShiftKey | Keys.RShiftKey | Keys.LShiftKey | Keys.Control | Keys.ControlKey | Keys.LControlKey | Keys.RControlKey | Keys.Alt;
                else
                    displayManager.RequiredModifierKey = Keys.None;
            }
        }

        private void checkSnapEdge_CheckedChanged(object sender, EventArgs e)
        {
            if (displayManager != null)
                displayManager.SnapToEdge = checkSnapEdge.Checked;
        }

        private void buttonSidebar_Click(object sender, EventArgs e)
        {
            SelectTab((Controls.FlatVerticalButton)sender);
        }

        /// <summary>
        /// Changes the displayed tab based on the sidebar buttons (buttonManager, buttonTemplates)
        /// </summary>
        private void SelectTab(Controls.FlatVerticalButton button)
        {
            if (button.Selected)
                return;

            var isManager = button == buttonManager;

            labelOptions.Parent.SuspendLayout();
            labelOptions.Visible = isManager;
            label3.Visible = isManager;
            label6.Visible = isManager;
            labelCompact.Visible = isManager;
            labelOptions.Parent.ResumeLayout();

            buttonManager.Selected = isManager;
            buttonTemplates.Selected = !isManager;

            panelContainer.SuspendLayout();
            panelTemplatesContainer.SuspendLayout();

            panelTemplateOptionsScroll.Visible = !isManager;

            OnFilterChanged();

            panelTemplatesContainer.ResumeLayout();
            panelContainer.ResumeLayout();
        }

        private bool _ShowNoTemplates;
        private bool ShowNoTemplates
        {
            get
            {
                return _ShowNoTemplates;
            }
            set
            {
                if (_ShowNoTemplates != value)
                {
                    _ShowNoTemplates = value;

                    panelTemplatesContainer.SuspendLayout();

                    labelNoTemplates.Visible = value;
                    panelTemplates.Visible = !value;

                    panelTemplatesContainer.ResumeLayout();
                }
            }
        }

        private bool IsFilterVisible(TemplateDisplay td)
        {
            if (buttonManager.Selected)
                return td.AssignedTo != null;
            return td.AssignedTo == null;
        }

        private void OnFilterChanged()
        {
            panelTemplates.SuspendLayout();

            var count = 0;

            foreach (Control c in panelTemplates.Controls)
            {
                var td = (TemplateDisplay)c;
                var v = IsFilterVisible(td);

                td.Visible = v;

                if (v)
                {
                    ++count;
                }
                else if (_Selected == td)
                {
                    Selected = null;
                    LoadOptions(null);
                }

            }

            ShowNoTemplates = count == 0;

            panelTemplates.ResumeLayout();
        }

        private void ToggleActive(TemplateDisplay td)
        {
            var a = td.AssignedTo;
            if (a != null)
                SetActive(td, !a.Enabled);
        }

        void assigned_MouseUp(object sender, MouseEventArgs e)
        {
            if (e.Button == System.Windows.Forms.MouseButtons.Left)
            {
                ToggleActive((TemplateDisplay)sender);
            }
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

            if (assignedTemplates.TryGetValue(a, out td))
            {
                td.Activated = a.Enabled;
            }
        }

        private void enabledToolStripMenuItem_Click(object sender, EventArgs e)
        {
            enabledToolStripMenuItem.Checked = !enabledToolStripMenuItem.Checked;

            var c = (TemplateDisplay)contextCustom.Tag;
            if (c != null && c.Source != null)
            {
                SetActive(c, enabledToolStripMenuItem.Checked);
            }
        }

        private void ShowRename(TemplateDisplay td)
        {
            if (td.TextHeight == 0)
                return;

            if (textName == null)
            {
                textName = new TextBox()
                {
                    Visible = false,
                };
                textName.LostFocus += textName_LostFocus;
                textName.KeyDown += textName_KeyDown;
            }

            td.Controls.Add(textName);

            textName.Bounds = new Rectangle(td.Padding.Left, td.Height - td.Padding.Bottom - td.TextHeight + (td.TextHeight - textName.Height) / 2, td.Width - td.Padding.Horizontal, td.TextHeight);
            textName.Text = td.Text;
            textName.Tag = td;
            td.Text = string.Empty;
            textName.SelectAll();
            textName.Visible = true;
            textName.Focus();
        }

        void textName_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                e.Handled = true;
                e.SuppressKeyPress = true;
                textName.Visible = false;
            }
        }

        void textName_LostFocus(object sender, EventArgs e)
        {
            var td = (TemplateDisplay)textName.Tag;
            if (td == null)
                return;
            var a = td.AssignedTo;
            if (a != null && !td.IsDisposed)
            {
                td.Text = a.Name = textName.Text;
            }
            textName.Tag = null;
            textName.Visible = false;
            this.Controls.Add(textName);
        }

        private void renameToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ShowRename((TemplateDisplay)contextCustom.Tag);
        }

        private void labelOptions_Click(object sender, EventArgs e)
        {
            var f = (Form)labelOptions.Tag;

            if (f == null || f.IsDisposed)
            {
                labelOptions.Tag = f = new UI.Base.PopupFlatBase(this, false)
                {
                    StartPosition = FormStartPosition.Manual,
                };

                f.Controls.Add(panelSettings);

                var h = f.Handle;

                panelSettings.Size = panelSettingsContent.Size;
                f.Size = new Size(panelSettingsContent.Width + panelSettings.Margin.Horizontal, panelSettingsContent.Height + panelSettings.Margin.Vertical - 1);

                f.FormClosing += delegate
                {
                    panelSettings.Visible = false;
                    this.Controls.Add(panelSettings);
                    f.Dispose();
                };

                panelSettings.Visible = true;
            }

            var l = labelOptions.PointToScreen(Point.Empty);
            var screen = Screen.FromPoint(l).WorkingArea;
            var x = l.X - 10;
            var y = l.Y - 10;

            if (x + f.Width > screen.Right)
                x = screen.Right - f.Width;
            if (y + f.Height > screen.Bottom)
                y = screen.Bottom - f.Height;

            f.Location = new Point(x, y);
            if (!f.Visible)
                f.Show(this);
        }

        private void checkDelayLaunching_CheckedChanged(object sender, EventArgs e)
        {
            Tools.WindowManager.Instance.DelayLaunchUntilAvailable = ((CheckBox)sender).Checked;
        }

        private void checkReorderOnRelease_CheckedChanged(object sender, EventArgs e)
        {
            Tools.WindowManager.Instance.ReorderOnRelease = ((CheckBox)sender).Checked;
        }

        private void checkAllowActiveChanges_CheckedChanged(object sender, EventArgs e)
        {
            Tools.WindowManager.Instance.AllowActiveChanges = ((CheckBox)sender).Checked;
        }

        protected override void WndProc(ref Message m)
        {
            base.WndProc(ref m);

            switch ((WindowMessages)m.Msg)
            {
                case WindowMessages.WM_EXITSIZEMOVE:

                    if (mode == TemplateMode.Manager)
                        Settings.WindowBounds[this.GetType()].Value = this.Bounds;

                    break;
            }
        }

        private void snapToToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var t = (ToolStripMenuItem)sender;
            var c = (TemplateDisplay)contextCustom.Tag;
            var b = !t.Checked;

            if (t == snapToToolStripMenuItem)
            {
                return;
            }

            if (c != null && c.Source != null)
            {
                Settings.WindowTemplate.SnapType v;
                if (t == windowEdgesToolStripMenuItem)
                    v = Settings.WindowTemplate.SnapType.WindowEdgeInner;
                else if (t == clientEdgesToolStripMenuItem)
                    v = Settings.WindowTemplate.SnapType.ClientEdge;
                else if (t == windowEdgesOuterToolStripMenuItem)
                    v = Settings.WindowTemplate.SnapType.WindowEdgeOuter;
                else
                    v = Settings.WindowTemplate.SnapType.None;
                c.Source.SnapToEdges = b ? v : Settings.WindowTemplate.SnapType.None;
            }
        }

        private void accountTypeToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var t = (ToolStripMenuItem)sender;
            var c = (TemplateDisplay)contextCustom.Tag;
            var b = !t.Checked;

            if (t == accountTypeToolStripMenuItem)
            {
                return;
            }

            if (c != null && c.AssignedTo != null)
            {
                Settings.WindowTemplate.Assignment.AccountType v;
                if (t == guildWars2ToolStripMenuItem)
                    v = Settings.WindowTemplate.Assignment.AccountType.GuildWars2;
                else if (t == guildWars1ToolStripMenuItem)
                    v = Settings.WindowTemplate.Assignment.AccountType.GuildWars1;
                else
                    v = Settings.WindowTemplate.Assignment.AccountType.Any;
                c.AssignedTo.Type = b ? v : Settings.WindowTemplate.Assignment.AccountType.Any;
            }
        }

        private void labelCompact_Click(object sender, EventArgs e)
        {
            if (ShowCompact != null)
            {
                ShowCompact(this, EventArgs.Empty);
            }
            else
            {
                var f = (formTemplatesCompact)labelCompact.Tag;
                if (f == null || f.IsDisposed)
                {
                    labelCompact.Tag = f = new formTemplatesCompact();
                    f.Show();
                }
                else if (!f.Visible)
                {
                    f.Show();
                }
            }
        }
    }
}
