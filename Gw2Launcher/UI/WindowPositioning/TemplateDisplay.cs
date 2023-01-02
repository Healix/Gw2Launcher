using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Drawing;

namespace Gw2Launcher.UI.WindowPositioning
{
    public class TemplateDisplay : Control
    {
        private BufferedGraphics buffer;
        private bool redraw;
        private ScreenTemplate template;
        private Rectangle[] display;

        public TemplateDisplay()
        {
            redraw = true;
            this.ForeColor = Color.FromArgb(200, 200, 200);
            Cursor = Cursors.Hand;
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

        private Color _TextColor = SystemColors.ControlText;
        public Color TextColor
        {
            get
            {
                return _TextColor;
            }
            set
            {
                if (_TextColor != value)
                {
                    _TextColor = value;
                    OnRedrawRequired();
                }
            }
        }
        
        private Color _ForeColorHighlight = Color.FromArgb(220, 220, 220);
        public Color ForeColorHighlight
        {
            get
            {
                return _ForeColorHighlight;
            }
            set
            {
                if (_ForeColorHighlight != value)
                {
                    _ForeColorHighlight = value;
                    if (_Selected || _Hovered)
                        OnRedrawRequired();
                }
            }
        }

        private Color _ScreenBackColor = Color.Transparent;
        public Color ScreenBackColor
        {
            get
            {
                return _ScreenBackColor;
            }
            set
            {
                if (_ScreenBackColor != value)
                {
                    _ScreenBackColor = value;
                    OnRedrawRequired();
                }
            }
        }

        private Color _ScreenBorderColor = Color.DarkGray;
        public Color ScreenBorderColor
        {
            get
            {
                return _ScreenBorderColor;
            }
            set
            {
                if (_ScreenBorderColor != value)
                {
                    _ScreenBorderColor = value;
                    OnRedrawRequired();
                }
            }
        }

        private Color _WindowBorderColor = Color.FromArgb(140, 140, 140);
        public Color WindowBorderColor
        {
            get
            {
                return _WindowBorderColor;
            }
            set
            {
                _WindowBorderColor = value;
                OnRedrawRequired();
            }
        }

        private Color _WindowBorderColorHighlight = Color.FromArgb(160, 160, 160);
        public Color WindowBorderColorHighlight
        {
            get
            {
                return _WindowBorderColorHighlight;
            }
            set
            {
                if (_WindowBorderColorHighlight != value)
                {
                    _WindowBorderColorHighlight = value;
                    if (_Selected || _Hovered)
                        OnRedrawRequired();
                }
            }
        }

        private Color _BackColorHighlight = Color.FromArgb(248, 248, 248);
        public Color BackColorHighlight
        {
            get
            {
                return _BackColorHighlight;
            }
            set
            {
                if (_BackColorHighlight != value)
                {
                    _BackColorHighlight = value;
                    if (_Selected || _Hovered)
                        OnRedrawRequired();
                }
            }
        }

        private Color _BorderColorHighlight = Color.FromArgb(220, 220, 220);
        public Color BorderColorHighlight
        {
            get
            {
                return _BorderColorHighlight;
            }
            set
            {
                if (_BorderColorHighlight != value)
                {
                    _BorderColorHighlight = value;
                    if (_Selected || _Hovered)
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
                    OnRedrawRequired();
                }
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
                SolidBrush brush;
                Pen pen;

                brush = new SolidBrush(_ScreenBackColor);

                if (_Selected)
                {
                    pen = new Pen(_BorderColorHighlight);

                    g.Clear(_BackColorHighlight);
                    g.DrawRectangle(pen, 0, 0, this.Width - 1, this.Height - 1);
                }
                else
                {
                    pen = new Pen(_ScreenBorderColor);

                    g.Clear(this.BackColor);
                    var r = new Rectangle(this.Padding.Left, this.Padding.Top, this.Width - this.Padding.Horizontal - 1, this.Height - this.Padding.Vertical - _TextHeight - 1);
                    if (_ScreenBackColor.A != 0)
                    {
                        g.FillRectangle(brush, r);
                    }
                    g.DrawRectangle(pen, r);
                }

                if (template != null)
                {
                    if (display == null)
                        display = template.GetWindows(new Rectangle(this.Padding.Left, this.Padding.Top, this.Width - this.Padding.Horizontal, this.Height - this.Padding.Vertical - _TextHeight), _FlipHorizontal, _FlipVertical);

                    if (_Hovered || _Selected)
                    {
                        brush.Color = _ForeColorHighlight;
                        pen.Color = _WindowBorderColorHighlight;
                    }
                    else
                    {
                        brush.Color = this.ForeColor;
                        pen.Color = _WindowBorderColor;
                    }

                    foreach (var r in display)
                    {
                        g.FillRectangle(brush, r);
                        g.DrawRectangle(pen, r.X, r.Y, r.Width - 1, r.Height - 1);
                    }
                }

                if (_Activated)
                {
                    brush.Color = Color.FromArgb(180, 0, 90, 0);

                    var w = this.Width - 2;
                    var h = (int)(this.Font.GetHeight(g) * 1.1f);
                    var y = this.Padding.Top + (this.Height - this.Padding.Vertical - _TextHeight - h) / 2;
                    var r = new Rectangle((this.Width - w) / 2, y, w, h);

                    g.FillRectangle(brush, r);
                    TextRenderer.DrawText(g, "active", this.Font, r, Color.White, Color.Transparent, TextFormatFlags.SingleLine | TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);
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
                        c = _TextColor;
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

                pen.Dispose();
                brush.Dispose();
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

    public class Template
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

    public enum VariableType
    {
        CountAsByte,
        PercentAsFloat,
    }

    public enum TemplateLayout
    {
        Columns,
        Rows
    }

    public class TemplateVariables
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

    public interface ITemplateVariable
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

    public class TemplateVariable<T> : ITemplateVariable
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

    public interface ITemplateSize
    {
        float Size
        {
            get;
        }
    }

    public class TemplateSection : ITemplateSize
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

    public class Column
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

    public class ScreenTemplate
    {
        private Rectangle bounds;
        private int windows;
        private Settings.WindowTemplate source;

        public ScreenTemplate(Settings.WindowTemplate source)
        {
            this.source = source;

            Initialize();
        }

        public ScreenTemplate(params Settings.WindowTemplate.Screen[] screens)
            : this(new Settings.WindowTemplate(screens))
        {
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

        public Settings.WindowTemplate Source
        {
            get
            {
                return source;
            }
            set
            {
                source = value;
                Initialize();
            }
        }

        public Settings.WindowTemplate.Screen[] Screens
        {
            get
            {
                return source.Screens;
            }
        }

        private void Initialize()
        {
            if (source.Screens.Length == 1)
            {
                bounds = source.Screens[0].Bounds;
                windows = source.Screens[0].Windows.Length;
            }
            else
            {
                int l = int.MaxValue,
                    t = int.MaxValue,
                    r = int.MinValue,
                    b = int.MinValue;
                windows = 0;

                foreach (var s in source.Screens)
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
            var b = new Rectangle[source.Screens.Length];

            for (var i = source.Screens.Length - 1; i >= 0; --i)
            {
                if (useWorkingArea)
                    b[i] = System.Windows.Forms.Screen.FromRectangle(source.Screens[i].Bounds).WorkingArea;
                else
                    b[i] = System.Windows.Forms.Screen.FromRectangle(source.Screens[i].Bounds).Bounds;
            }

            return GetWindows(b, flipX, flipY);
        }

        public Rectangle[] GetWindows(Rectangle bounds, bool flipX, bool flipY)
        {
            var scaleX = (double)bounds.Width / this.bounds.Width;
            var scaleY = (double)bounds.Height / this.bounds.Height;
            var b = new Rectangle[source.Screens.Length];

            for (var i = source.Screens.Length - 1; i >= 0; --i)
            {
                var s = source.Screens[i].Bounds;

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
            var screens = source.Screens;

            if (screens[screen].Bounds.Size == bounds.Size)
            {
                if (flipX || flipY || !bounds.Location.IsEmpty)
                {
                    var l = screens[screen].Windows.Length;

                    for (var i = 0; i < l; i++)
                    {
                        var w = screens[screen].Windows[i];

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

                var l = screens[screen].Windows.Length;

                for (var i = 0; i < l; i++)
                {
                    var w = screens[screen].Windows[i];
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
            var windows = new Rectangle[source.Screens[screen].Windows.Length];

            GetWindows(windows, 0, screen, bounds, flipX, flipY);

            return windows;
        }

        public Rectangle[] GetWindows(Rectangle[] bounds, bool flipX, bool flipY)
        {
            var windows = new Rectangle[this.windows];
            var wi = 0;

            for (var i = 0; i < source.Screens.Length; i++)
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

}
