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
        
        private class ScreenTemplate : Settings.WindowTemplate
        {
            private Rectangle bounds;
            private int windows;

            public ScreenTemplate(params Screen[] screens) : base(screens)
            {
                Initialize();
            }

            public Rectangle Bounds
            {
                get
                {
                    return bounds;
                }
            }

            public int WindowCount
            {
                get
                {
                    return windows;
                }
            }

            private void Initialize()
            {
                if (base.Screens.Length == 1)
                {
                    bounds = base.Screens[0].Bounds;
                    windows = base.Screens[0].Windows.Length;
                }
                else
                {
                    int l = int.MaxValue,
                        t = int.MaxValue,
                        r = int.MinValue,
                        b = int.MinValue;
                    windows = 0;

                    foreach (var s in base.Screens)
                    {
                        if (s.Bounds.X < l)
                            l = s.Bounds.X;
                        if (s.Bounds.Y < t)
                            t = s.Bounds.Y;
                        if (s.Bounds.Right > r)
                            r = s.Bounds.Right;
                        if (s.Bounds.Bottom > b)
                            b = s.Bounds.Bottom;

                        windows += s.Windows.Length;
                    }

                    bounds = new Rectangle(l, t, r - l, b - t);
                }
            }

            public Rectangle[] GetWindows(bool useWorkingArea, bool flipX, bool flipY)
            {
                var b = new Rectangle[base.Screens.Length];

                for (var i = base.Screens.Length - 1; i >= 0; --i)
                {
                    if (useWorkingArea)
                        b[i] = System.Windows.Forms.Screen.FromRectangle(base.Screens[i].Bounds).WorkingArea;
                    else
                        b[i] = System.Windows.Forms.Screen.FromRectangle(base.Screens[i].Bounds).Bounds;
                }

                return GetWindows(b, flipX, flipY);
            }

            public Rectangle[] GetWindows(Rectangle bounds, bool flipX, bool flipY)
            {
                var scaleX = (double)bounds.Width / this.bounds.Width;
                var scaleY = (double)bounds.Height / this.bounds.Height;
                var b = new Rectangle[base.Screens.Length];

                for (var i = base.Screens.Length - 1; i >= 0; --i)
                {
                    var s = base.Screens[i].Bounds;

                    var x1 = (int)((s.X - this.bounds.X) * scaleX + 0.5) + bounds.X;
                    var y1 = (int)((s.Y - this.bounds.Y) * scaleY + 0.5) + bounds.Y;
                    var x2 = (int)((s.Right - this.bounds.X) * scaleX + 0.5) + bounds.X;
                    var y2 = (int)((s.Bottom - this.bounds.Y) * scaleY + 0.5) + bounds.Y;

                    b[i] = new Rectangle(x1, y1, x2 - x1, y2 - y1);
                }

                return GetWindows(b, flipX, flipY);
            }

            private int GetWindows(Rectangle[] windows, int offset, int screen, Rectangle bounds, bool flipX, bool flipY)
            {
                var wi = offset;
                var screens = base.Screens;

                if (screens[screen].Bounds.Size == bounds.Size)
                {
                    if (flipX || flipY || !bounds.Location.IsEmpty)
                    {
                        foreach (var w in screens[screen].Windows)
                        {
                            int x = w.X,
                                y = w.Y;

                            if (flipX)
                            {
                                x = bounds.Width - w.Right;
                            }

                            if (flipY)
                            {
                                y = bounds.Height - w.Bottom;
                            }

                            windows[wi++] = new Rectangle(x + bounds.X, y + bounds.Y, w.Width, w.Height);
                        }
                    }
                    else
                    {
                        Array.Copy(screens[screen].Windows, 0, windows, wi, screens[screen].Windows.Length);
                        wi += screens[screen].Windows.Length;
                    }
                }
                else
                {
                    var scaleX = (double)bounds.Width / screens[screen].Bounds.Width;
                    var scaleY = (double)bounds.Height / screens[screen].Bounds.Height;

                    foreach (var w in screens[screen].Windows)
                    {
                        int x, y, r, b;

                        if (flipX)
                        {
                            x = screens[screen].Bounds.Width - w.Right;
                            r = screens[screen].Bounds.Width - w.X;
                        }
                        else
                        {
                            x = w.X;
                            r = w.Right;
                        }

                        if (flipY)
                        {
                            y = screens[screen].Bounds.Height - w.Bottom;
                            b = screens[screen].Bounds.Height - w.Y;
                        }
                        else
                        {
                            y = w.Y;
                            b = w.Bottom;
                        }

                        x = (int)(x * scaleX + 0.5);
                        r = (int)(r * scaleX + 0.5);
                        y = (int)(y * scaleY + 0.5);
                        b = (int)(b * scaleY + 0.5);

                        windows[wi++] = new Rectangle(x + bounds.X, y + bounds.Y, r - x, b - y);
                    }
                }

                return wi - offset;
            }

            public Rectangle[] GetWindows(int screen, Rectangle bounds, bool flipX, bool flipY)
            {
                var windows = new Rectangle[base.Screens[screen].Windows.Length];

                GetWindows(windows, 0, screen, bounds, flipX, flipY);

                return windows;
            }

            public Rectangle[] GetWindows(Rectangle[] bounds, bool flipX, bool flipY)
            {
                var windows = new Rectangle[this.windows];
                var wi = 0;

                for (var i = base.Screens.Length - 1; i >= 0; --i)
                {
                    wi += GetWindows(windows, wi, i, bounds[i], flipX, flipY);


                    //if (screens[i].ScreenBounds.Size == bounds[i].Size)
                    //{
                    //    if (flipX || flipY || !bounds[i].Location.IsEmpty)
                    //    {
                    //        foreach (var w in screens[i].Windows)
                    //        {
                    //            int x = w.X,
                    //                y = w.Y;

                    //            if (flipX)
                    //            {
                    //                x = bounds[i].Width - w.Right;
                    //            }

                    //            if (flipY)
                    //            {
                    //                y = bounds[i].Height - w.Bottom;
                    //            }

                    //            windows[wi++] = new Rectangle(x + bounds[i].X, y + bounds[i].Y, w.Width, w.Height);
                    //        }
                    //    }
                    //    else
                    //    {
                    //        Array.Copy(screens[i].Windows, 0, windows, wi, screens[i].Windows.Length);
                    //        wi += screens[i].Windows.Length;
                    //    }
                    //}
                    //else
                    //{
                    //    var scaleX = (double)bounds[i].Width / screens[i].ScreenBounds.Width;
                    //    var scaleY = (double)bounds[i].Height / screens[i].ScreenBounds.Height;

                    //    foreach (var w in screens[i].Windows)
                    //    {
                    //        int x, y, r, b;

                    //        if (flipX)
                    //        {
                    //            x = screens[i].ScreenBounds.Width - w.Right;
                    //            r = screens[i].ScreenBounds.Width - w.X;
                    //        }
                    //        else
                    //        {
                    //            x = w.X;
                    //            r = w.Right;
                    //        }

                    //        if (flipY)
                    //        {
                    //            y = screens[i].ScreenBounds.Height - w.Bottom;
                    //            b = screens[i].ScreenBounds.Height - w.Y;
                    //        }
                    //        else
                    //        {
                    //            y = w.Y;
                    //            b = w.Bottom;
                    //        }

                    //        x = (int)(x * scaleX + 0.5);
                    //        r = (int)(r * scaleX + 0.5);
                    //        y = (int)(y * scaleY + 0.5);
                    //        b = (int)(b * scaleY + 0.5);

                    //        windows[wi++] = new Rectangle(x + bounds[i].X, y + bounds[i].Y, r - x, b - y);
                    //    }
                    //}
                }

                return windows;
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
                Cursor = Cursors.Hand;
                Padding = new Padding(3, 3, 3, 3);
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

            public Settings.WindowTemplate Source
            {
                get;
                set;
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
                        g.Clear(Color.FromArgb(201, 217, 235));
                        using (var pen = new Pen(Color.FromArgb(153, 170, 189)))
                        {
                            g.DrawRectangle(pen, 0, 0, this.Width - 1, this.Height - 1);
                        }
                    }
                    else
                    {
                        g.Clear(this.BackColor);
                    }

                    if (template != null)
                    {
                        if (display == null)
                            display = template.GetWindows(new Rectangle(this.Padding.Left, this.Padding.Top, this.Width - this.Padding.Horizontal, this.Height - this.Padding.Vertical), _FlipHorizontal, _FlipVertical);

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
        }

        private enum VariableType
        {
            CountAsByte,
            PercentAsFloat,
        }

        private enum TemplateLayout
        {
            Columns,
            Rows
        }

        private class TemplateVariables
        {
            public TemplateVariables(params ITemplateVariable[] variables)
            {
                this.Variables = variables;
            }

            public ITemplateVariable[] Variables
            {
                get;
                private set;
            }

            public void SetVariable<T>(int index, T value)
            {
                ((TemplateVariable<T>)Variables[index]).Value = value;
            }

            public TemplateVariable<T> GetVariable<T>(int index)
            {
                return (TemplateVariable<T>)Variables[index];
            }

            public T GetValue<T>(int index)
            {
                return GetVariable<T>(index).Value;
            }

            public T GetMinimum<T>(int index)
            {
                return GetVariable<T>(index).Minimum;
            }

            public object[] GetValues()
            {
                var values = new object[this.Variables.Length];

                for (var i = this.Variables.Length - 1; i >= 0; --i)
                {
                    values[i] = this.Variables[i].GetValue();
                }

                return values;
            }

            public void SetValues(object[] values)
            {
                for (var i = values.Length - 1; i >= 0; --i)
                {
                    this.Variables[i].SetValue(values[i]);
                }
            }
        }

        private interface ITemplateVariable
        {
            VariableType Type
            {
                get;
            }

            string Name
            {
                get;
            }

            Type ValueType
            {
                get;
            }

            object GetValue();

            void SetValue(object o);
        }

        private class TemplateVariable<T> : ITemplateVariable
        {
            private Action<TemplateVariable<T>> onChanged;
            private T value;

            public TemplateVariable(VariableType type, string name, T value, T minimum, Action<TemplateVariable<T>> onChanged = null)
            {
                this.Type = type;
                this.Name = name;
                this.Value = value;
                this.Minimum = minimum;
                this.onChanged = onChanged;
            }

            public VariableType Type
            {
                get;
                private set;
            }

            public string Name
            {
                get;
                private set;
            }

            public T Value
            {
                get
                {
                    return value;
                }
                set
                {
                    if (!object.Equals(this.value, value))
                    {
                        this.value = value;
                        if (onChanged != null)
                            onChanged(this);
                    }
                }
            }

            public object GetValue()
            {
                return value;
            }

            public void SetValue(object o)
            {
                this.Value = (T)o;
            }

            public T Minimum
            {
                get;
                private set;
            }

            public Type ValueType
            {
                get
                {
                    return typeof(T);
                }
            }
        }

        private class Template
        {
            private Func<Template, Size, Rectangle[]> generator;
            private TemplateVariables variables;

            public Template(TemplateVariables variables, Func<Template, Size, Rectangle[]> generator)
            {
                this.generator = generator;
                this.variables = variables;
            }

            public TemplateVariables Variables
            {
                get
                {
                    return variables;
                }
            }

            public Rectangle[] Create(Size size)
            {
                return generator(this, size);
            }
        }

        private interface ITemplateSize
        {
            float Size
            {
                get;
            }
        }

        private class TemplateSection : ITemplateSize
        {
            public class TemplateArea : ITemplateSize
            {
                public TemplateArea(float size, TemplateLayout layout, byte count)
                {
                    this.Size = size;
                    this.AreaLayout = layout;
                    this.Count = count;
                }

                public float Size
                {
                    get;
                    private set;
                }

                public TemplateLayout AreaLayout
                {
                    get;
                    private set;
                }

                public byte Count
                {
                    get;
                    private set;
                }
            }

            public TemplateSection(float size, params TemplateArea[] areas)
            {
                this.Size = size;
                this.Areas = areas;
            }

            public float Size
            {
                get;
                private set;
            }

            public TemplateArea[] Areas
            {
                get;
                private set;
            }

            public int Count
            {
                get
                {
                    var count = 0;

                    foreach (var a in Areas)
                    {
                        count += a.Count;
                    }

                    return count;
                }
            }
        }

        private class Column
        {
            public Column(TemplateLayout layout, float width, float[] heights, byte[] counts)
            {
                this.layout = layout;
                this.width = width;
                this.heights = heights;
                this.counts = counts;
            }

            public Column(TemplateLayout layout, float width, byte count)
                : this(layout, width, new float[] { width }, new byte[] { count })
            {
            }

            public TemplateLayout layout;
            public float width;
            public float[] heights;
            public byte[] counts;
        }

        public event EventHandler<TemplateDisplayManager> TemplateDisplayManagerChanged;

        private Dictionary<Template, object[]> defaultValues;
        private CheckBox[] checkScreen;
        private TemplateDisplay[] displayed;
        private Dictionary<Rectangle, int> screens;
        private TemplateDisplayManager displayManager;
        private formWindowSize master;
        private int delayedChange;

        public formTemplates(formWindowSize master)
        {
            InitializeComponents();

            this.screens = new Dictionary<Rectangle, int>();
            this.master = master;

            var screens = Screen.AllScreens;
            checkScreen = new CheckBox[screens.Length];
            checkScreen[0] = checkScreen1;
            checkScreen1.Tag = 0;
            displayed = new TemplateDisplay[screens.Length];

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

            this.Disposed += formTemplates_Disposed;
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
            var size = Util.RectangleConstraint.Scale(screen.Size, new Size(Scale(80), Scale(80)));

            defaultValues = new Dictionary<Template, object[]>();

            foreach (var t in CreateTemplates())
            {
                var td = CreateDisplay();

                td.Size = size;
                td.TemplateData = t;
                td.Template = new ScreenTemplate(new Settings.WindowTemplate.Screen(screen, t.Create(screen.Size)));

                defaultValues[t] = t.Variables.GetValues();

                panelTemplates.Controls.Add(td);
            }

            var custom = Settings.WindowTemplates.Count;

            if (custom > 0)
            {
                for (var i = 0; i < custom; i++)
                {
                    var t = Settings.WindowTemplates[i];
                    var td = CreateDisplay();

                    var stt = new Settings.WindowTemplate.Screen[t.Screens.Length];
                    for (var j = t.Screens.Length - 1; j >= 0; --j)
                    {
                        stt[j] = new Settings.WindowTemplate.Screen(t.Screens[j].Bounds, t.Screens[j].Windows);
                    }

                    var st = new ScreenTemplate(stt);

                    td.Size = size;
                    td.Template = st;
                    td.Source = t;

                    panelTemplates.Controls.Add(td);
                }
            }

            panelTemplates.ResumeLayout();

            base.OnShown(e);
        }

        protected override void OnFormClosed(FormClosedEventArgs e)
        {
            base.OnFormClosed(e);
        }

        void formTemplates_Disposed(object sender, EventArgs e)
        {
            if (displayManager != null)
            {
                displayManager.Dispose();
                displayManager = null;
                if (TemplateDisplayManagerChanged != null)
                    TemplateDisplayManagerChanged(this, displayManager);
            }
        }

        private TemplateDisplay CreateDisplay()
        {
            var td = new TemplateDisplay()
            {
                Margin = Padding.Empty,
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

            if (_Selected != td)
            {
                Selected = td;
                LoadOptions(td);
            }

            if (e.Button == System.Windows.Forms.MouseButtons.Right)
            {
                if (td.TemplateData == null)
                    contextCustom.Show(Cursor.Position);
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

            for (var i = checkScreen.Length - 1; i >= 0; --i)
            {
                var c = checkScreen[i];
                c.Enabled = !isCustom || td.Template.Screens.Length == 1 || GetTemplateScreenIndex(td.Template, i) != -1;
                if (c.Enabled && !isEmpty && displayed[i] == td)
                    c.CheckState = CheckState.Checked;
                else if (displayed[i] != null)
                    c.CheckState = CheckState.Indeterminate;
                else
                    c.CheckState = CheckState.Unchecked;
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

            if (panelOptions.Controls.Count > 0)
                panelOptions.Visible = true;

            panelOptions.ResumeLayout();
            panelOptionsContainer.ResumeLayout();
        }

        byte tmode = 0;

        private async void DelayedTemplateChanged()
        {
            do
            {
                if (tmode==0)
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
                else if (tmode == 1)
                {
                    var t = delayedChange;

                    while (true)
                    {
                        await Task.Delay(500);
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

                    OnDisplayedTemplateUpdate();

                    break;
                }
            }
            while (true);
        }

        private void OnDisplayedTemplateUpdate()
        {
            var useWorking = checkOverlapTaskbar.Enabled && !checkOverlapTaskbar.Checked;

            for (var i = 0; i < displayed.Length; i++)
            {
                if (displayed[i] == null)
                    continue;

                var t = displayed[i].Template;

                if (t.Screens.Length == 1)
                {
                    var screen = Screen.AllScreens[i];
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
            ShowCustom(_Selected, false);
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
            ShowCustom(null, null, false);
        }

        private void labelCustomNewFromLayout_Click(object sender, EventArgs e)
        {
            var w = master.GetWindows();
            var rects = new Rectangle[w.Count];
            var i = 0;

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

            ShowCustom(null, rects, false);
        }
        
        private void ShowCustom(TemplateDisplay td, bool overwrite)
        {
            Rectangle[] rects;
            if (td != null)
            {
                if (td.TemplateData != null)
                    rects = td.Template.GetWindows(new Rectangle[] { checkOverlapTaskbar.Checked ? Screen.FromControl(this).Bounds : Screen.FromControl(this).WorkingArea }, checkFlipX.Checked, checkFlipY.Checked);
                else
                    rects = td.Template.GetWindows(false, checkFlipX.Checked, checkFlipY.Checked);
            }
            else
                rects = null;

            ShowCustom(td, rects, overwrite);
        }

        private void ShowCustom(TemplateDisplay td, Rectangle[] rects, bool overwrite)
        {
            if (displayManager != null)
                displayManager.Hide();

            using (var f = new formCustomTemplate(master.MinimumSize, rects))
            {
                f.Overwrite = overwrite;

                if (f.ShowDialog(this) == System.Windows.Forms.DialogResult.OK)
                {
                    if (f.Result == null || f.Result.Length == 0)
                        return;

                    var stt = new Settings.WindowTemplate.Screen[f.Result.Length];

                    for (var i = f.Result.Length - 1; i >= 0;--i )
                    {
                        stt[i] = new Settings.WindowTemplate.Screen(f.Result[i].Screen, f.Result[i].Windows);
                    }

                    var st = new ScreenTemplate(stt);

                    if (f.Overwrite && td != null && td.TemplateData == null)
                    {
                        Settings.WindowTemplates.ReplaceOrAdd(td.Source, st);

                        td.Template = st;
                        td.Source = st;

                        LoadOptions(td);
                        OnTemplateChanged();
                    }
                    else
                    {
                        td = CreateDisplay();
                        td.Template = st;
                        td.Source = st;

                        Settings.WindowTemplates.Add(st);

                        td.Size = Util.RectangleConstraint.Scale(f.Size, new Size(Scale(80),Scale(80)));

                        Selected = td;
                        LoadOptions(td);

                        panelTemplates.Controls.Add(td);
                        OnTemplateChanged();
                        //panelCustom.Visible = true;
                    }
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
            ShowCustom(_Selected, true);
        }

        private void deleteToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var c = _Selected;
            if (c != null)
            {
                if (c.Source != null)
                    Settings.WindowTemplates.Remove(c.Source);
                panelTemplates.Controls.Remove(c);
                c.Dispose();
                Selected = null;
                LoadOptions(null);
            }
        }

        private void labelDelete_Click(object sender, EventArgs e)
        {
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

                if (displayed[i] != _Selected)
                {
                    displayed[i] = _Selected;

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
            else if (displayed[i] != null)
            {
                displayed[i] = null;
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
    }
}
