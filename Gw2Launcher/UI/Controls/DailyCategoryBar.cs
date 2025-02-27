using System;
using System.Text;
using System.Collections.Generic;
using System.Windows.Forms;
using System.Drawing;
using System.ComponentModel;

namespace Gw2Launcher.UI.Controls
{
    class DailyCategoryBar : Base.BaseControl
    {
        public event EventHandler Collapsed;
        public event EventHandler Expanded;
        public event EventHandler DropDownSelectedItemChanged;
        public event EventHandler EyeClicked;

        private enum AreaType : byte
        {
            None,
            Text,
            Collapse,
            Eye,
        }

        private struct Area
        {
            public AreaType type;
            public Rectangle rect;
        }

        [Flags]
        private enum RedrawType : byte
        {
            None,
            Full = 1,
            Partial = 2,
        }

        private BufferedGraphics buffer;
        private bool 
            collapsed,
            layout;
        private RedrawType redraw;
        private Area[] areas;
        private int widthText;
        private sbyte hovered;
        private ComboBox combo;
        private ApiTimer apiTimer;

        public DailyCategoryBar()
        {
            SetStyle(ControlStyles.UserPaint, true);
            SetStyle(ControlStyles.AllPaintingInWmPaint, true);

            hovered = -1;
            layout = true;
            areas = new Area[2];
        }

        private bool _ButtonEyeVisible;
        [DefaultValue(false)]
        public bool ButtonEyeVisible
        {
            get
            {
                return _ButtonEyeVisible;
            }
            set
            {
                if (_ButtonEyeVisible != value)
                {
                    _ButtonEyeVisible = value;
                    if (!value && apiTimer != null)
                    {
                        apiTimer.Dispose();
                        apiTimer = null;
                    }
                    OnRedrawRequired(true);
                }
            }
        }

        private bool _ButtonDropDownArrowVisible;
        [DefaultValue(false)]
        public bool ButtonDropDownArrowVisible
        {
            get
            {
                return _ButtonDropDownArrowVisible;
            }
            set
            {
                if (_ButtonDropDownArrowVisible != value)
                {
                    widthText = -1;
                    _ButtonDropDownArrowVisible = value;
                    OnRedrawRequired(GetIndex(AreaType.Text));
                }
            }
        }

        private bool _ButtonEyeEnabled;
        [DefaultValue(false)]
        public bool ButtonEyeEnabled
        {
            get
            {
                return _ButtonEyeEnabled;
            }
            set
            {
                if (_ButtonEyeEnabled != value)
                {
                    _ButtonEyeEnabled = value;
                    OnRedrawRequired(GetIndex(AreaType.Eye));
                }
            }
        }

        private bool _ButtonEyeTimerEnabled;
        [DefaultValue(false)]
        public bool ButtonEyeTimerEnabled
        {
            get
            {
                return _ButtonEyeTimerEnabled;
            }
            set
            {
                if (_ButtonEyeTimerEnabled != value)
                {
                    _ButtonEyeTimerEnabled = value;
                    if (apiTimer != null)
                    {
                        apiTimer.Enabled = value;
                    }
                    if (_ButtonEyeEnabled && _ButtonEyeVisible)
                    {
                        OnRedrawRequired(GetIndex(AreaType.Eye));
                    }
                }
            }
        }

        private object[] _DropDownItems;
        [DefaultValue(null)]
        [Browsable(false)]
        public object[] DropDownItems
        {
            get
            {
                return _DropDownItems;
            }
            set
            {
                if (_DropDownItems != value)
                {
                    _DropDownItems = value;

                    if (combo != null)
                    {
                        combo.Items.Clear();
                        if (combo.DroppedDown)
                            combo.DroppedDown = false;
                    }
                }
            }
        }

        [DefaultValue(null)]
        [Browsable(false)]
        public object DropDownSelectedItem
        {
            get
            {
                if (_DropDownSelectedIndex != -1 && _DropDownItems != null && _DropDownSelectedIndex < _DropDownItems.Length)
                {
                    return _DropDownItems[_DropDownSelectedIndex];
                }
                else
                {
                    return null;
                }
            }
            set
            {
                if (value == null)
                {
                    _DropDownSelectedIndex = -1;
                }
                else if (DropDownSelectedItem != value)
                {
                    _DropDownSelectedIndex = -1;

                    if (_DropDownItems != null)
                    {
                        for (var i = 0; i < _DropDownItems.Length; i++)
                        {
                            if (_DropDownItems[i] == value)
                            {
                                _DropDownSelectedIndex = i;
                                break;
                            }
                        }
                    }
                }
            }
        }

        private int _DropDownSelectedIndex = -1;
        [DefaultValue(-1)]
        [Browsable(false)]
        public int DropDownSelectedIndex
        {
            get
            {
                return _DropDownSelectedIndex;
            }
            set
            {
                if (_DropDownItems != null && value < _DropDownItems.Length)
                {
                    _DropDownSelectedIndex = value;
                }
                else
                {
                    _DropDownSelectedIndex = -1;
                }
            }
        }

        public bool IsCollapsed
        {
            get
            {
                return collapsed;
            }
        }

        public int ArrowBarWidth
        {
            get;
            set;
        }

        protected override void OnSizeChanged(EventArgs e)
        {
            base.OnSizeChanged(e);

            if (buffer != null)
            {
                buffer.Dispose();
                buffer = null;
            }

            OnRedrawRequired(true);
        }

        private void OnRedrawRequired(sbyte area)
        {
            if (area != -1)
            {
                if ((redraw & RedrawType.Full) == 0)
                {
                    redraw |= RedrawType.Partial;
                    this.Invalidate(areas[area].rect);
                }
            }
        }

        private void OnRedrawRequired(bool layout = false)
        {
            if (layout)
            {
                this.layout = true;
            }

            if ((redraw & RedrawType.Full) == 0)
            {
                redraw = RedrawType.Full;
                this.Invalidate();
            }
        }

        private void DoLayout()
        {
            var h = this.Height;
            var w = this.Width;
            var count = 1;

            var types = new AreaType[]
            {
                AreaType.Eye,
                AreaType.Collapse,
            };

            var visible = new bool[]
            {
                _ButtonEyeVisible,
                true, //collapse
            };

            for (var i = 0; i < visible.Length; i++)
            {
                if (visible[i])
                    ++count;
            }

            if (areas.Length != count)
            {
                areas = new Area[count];
            }

            var index = 1;

            for (var i = visible.Length - 1; index < count; --i)
            {
                if (visible[i])
                {
                    w -= ArrowBarWidth;
                    areas[count - index++] = new Area()
                    {
                        type = types[i],
                        rect = new Rectangle(w, 0, ArrowBarWidth, h),
                    };
                }
            }

            areas[0] = new Area()
            {
                type = AreaType.Text,
                rect = new Rectangle(0, 0, w, h),
            };

            if (apiTimer != null)
            {
                apiTimer.Resize = true;
            }
        }

        protected override void OnTextChanged(EventArgs e)
        {
            base.OnTextChanged(e);
            widthText = -1;
            OnRedrawRequired();
        }

        protected override void OnFontChanged(EventArgs e)
        {
            base.OnFontChanged(e);
            widthText = -1;
            OnRedrawRequired();
        }

        protected override void OnMouseWheel(MouseEventArgs e)
        {
            base.OnMouseWheel(e);

            if (hovered != -1 && areas[hovered].type == AreaType.Text && _ButtonDropDownArrowVisible && _DropDownItems != null && _DropDownItems.Length > 1)
            {
                if (e is HandledMouseEventArgs)
                    ((HandledMouseEventArgs)e).Handled = true;

                if (e.Delta < 0)
                {
                    if (_DropDownSelectedIndex < _DropDownItems.Length - 1)
                    {
                        ++_DropDownSelectedIndex;
                    }
                    else
                    {
                        return;
                    }
                }
                else
                {
                    if (_DropDownSelectedIndex > 0)
                    {
                        --_DropDownSelectedIndex;
                    }
                    else
                    {
                        return;
                    }
                }

                if (DropDownSelectedItemChanged != null)
                {
                    try
                    {
                        DropDownSelectedItemChanged(this, EventArgs.Empty);
                    }
                    catch (Exception x)
                    {
                        Util.Logging.Log(x);
                    }
                }
            }
        }

        protected override void OnMouseLeave(EventArgs e)
        {
            base.OnMouseLeave(e);

            if (hovered != -1)
            {
                if (hovered != 0)
                {
                    OnRedrawRequired(hovered);
                }
                hovered = -1;
            }
        }

        protected override void OnMouseClick(MouseEventArgs e)
        {
            base.OnMouseClick(e);

            if (e.Button == System.Windows.Forms.MouseButtons.Left)
            {
                if (hovered != -1)
                {
                    switch (areas[hovered].type)
                    {
                        case AreaType.Collapse:

                            if (collapsed)
                            {
                                collapsed = false;

                                if (Expanded != null)
                                {
                                    try
                                    {
                                        Expanded(this, EventArgs.Empty);
                                    }
                                    catch (Exception x)
                                    {
                                        Util.Logging.Log(x);
                                    }
                                }
                            }
                            else
                            {
                                collapsed = true;

                                if (Collapsed != null)
                                {
                                    try
                                    {
                                        Collapsed(this, EventArgs.Empty);
                                    }
                                    catch (Exception x)
                                    {
                                        Util.Logging.Log(x);
                                    }
                                }
                            }
                            OnRedrawRequired(hovered);

                            break;
                        case AreaType.Eye:

                            ButtonEyeEnabled ^= true;

                            if (EyeClicked != null)
                            {
                                try
                                {
                                    EyeClicked(this, EventArgs.Empty);
                                }
                                catch (Exception x)
                                {
                                    Util.Logging.Log(x);
                                }
                            }

                            break;
                        case AreaType.Text:

                            ShowDropDown();

                            break;
                    }
                }
            }
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);

            if (hovered != -1)
            {
                if (areas[hovered].rect.Contains(e.Location))
                {
                    return;
                }
                else if (hovered != 0)
                {
                    OnRedrawRequired(hovered);
                }
            }

            for (sbyte i = 0; i < areas.Length; i++)
            {
                if (i != hovered && areas[i].rect.Contains(e.Location))
                {
                    hovered = i;

                    if (i != 0)
                    {
                        OnRedrawRequired(hovered);
                    }

                    return;
                }
            }

            hovered = -1;
        }

        public void SetState(bool collapsed)
        {
            if (collapsed != this.collapsed)
            {
                this.collapsed = collapsed;

                OnRedrawRequired(GetIndex(AreaType.Collapse));
            }
        }

        private sbyte GetIndex(AreaType t)
        {
            for (sbyte i = 0; i < areas.Length; i++)
            {
                if (areas[i].type == t)
                {
                    return i;
                }
            }

            return -1;
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            if (redraw != RedrawType.None)
            {
                redraw = RedrawType.None;

                if (layout)
                {
                    layout = false;
                    widthText = -1;
                    hovered = -1;
                    DoLayout();
                }

                int cx, cr;
                Graphics g;

                if (buffer == null)
                {
                    buffer = BufferedGraphicsManager.Current.Allocate(e.Graphics, this.DisplayRectangle);

                    g = buffer.Graphics;
                    cx = 0;
                    cr = int.MaxValue;
                }
                else
                {
                    g = buffer.Graphics;
                    //cr = e.ClipRectangle.Right;
                    cr = int.MaxValue;
                    cx = 0;
                    //if (cr == 0)
                    //{
                    //}
                    //else
                    //{
                    //    cx = e.ClipRectangle.X;
                    //    g.SetClip(e.ClipRectangle);
                    //}
                }

                var scale = g.DpiX / 96f;

                g.Clear(this.BackColor);

                if (cr > areas[0].rect.X && cx < areas[0].rect.Right)
                {
                    var wofs = this.Padding.Left;

                    if (_ButtonDropDownArrowVisible)
                    {
                        wofs -= (int)(10 * scale + 0.5f);
                    }

                    if (widthText == -1)
                    {
                        widthText = TextRenderer.MeasureText(g, this.Text, this.Font, new Size(areas[0].rect.Width - wofs, this.Height), TextFormatFlags.EndEllipsis | TextFormatFlags.VerticalCenter).Width;
                    }

                    TextRenderer.DrawText(g, this.Text, this.Font, new Rectangle(this.Padding.Left, 0, areas[0].rect.Width - wofs, this.Height), this.ForeColor, TextFormatFlags.EndEllipsis | TextFormatFlags.VerticalCenter);

                    if (_ButtonDropDownArrowVisible)
                    {
                        float
                            w = 7 * scale,
                            h = 3 * scale,
                            x = this.Padding.Left + widthText,
                            y = this.Height / 2f - h / 2f + 1;

                        var points = new PointF[]
                        {
                            new PointF(x, y),
                            new PointF(x + w, y),
                            new PointF(x + w / 2f, y + h),
                        };

                        g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;

                        using (var brush = new SolidBrush(UiColors.GetColor(UiColors.Colors.DailiesHeaderArrow)))
                        {
                            using (var pen = new Pen(brush))
                            {
                                g.FillPolygon(brush, points);
                                g.DrawPolygon(pen, points);
                            }
                        }

                        g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.Default;
                    }
                }

                if (areas.Length > 1 && cr > areas[1].rect.X && cx < areas[areas.Length - 1].rect.Right)
                {
                    using (var brush = new SolidBrush(UiColors.GetColor(UiColors.Colors.DailiesHeaderArrow)))
                    {
                        using (var pen = new Pen(brush))
                        {
                            for (var i = areas.Length - 1; i >= 1; --i)
                            {
                                if (cr <= areas[i].rect.X || cx >= areas[i].rect.Right)
                                    continue;

                                if (hovered == i)
                                {
                                    var c = brush.Color;

                                    brush.Color = UiColors.GetColor(UiColors.Colors.DailiesHeaderHovered);
                                    g.FillRectangle(brush, areas[i].rect);

                                    brush.Color = c;
                                }
                                else if ((i > 1 || i < areas.Length - 1) && this.Parent != null)
                                {
                                    var c = brush.Color;

                                    brush.Color = this.Parent.BackColor;

                                    if (i > 1)
                                    {
                                        g.FillRectangle(brush, areas[i].rect.X, areas[i].rect.Y, 1, areas[i].rect.Height);
                                    }
                                    if (i < areas.Length - 1)
                                    {
                                        g.FillRectangle(brush, areas[i].rect.Right - 1, areas[i].rect.Y, 1, areas[i].rect.Height);
                                    }

                                    brush.Color = c;
                                }

                                switch (areas[i].type)
                                {
                                    case AreaType.Collapse:

                                        PointF[] points;

                                        float
                                            w = 9 * scale,
                                            h = 5 * scale,
                                            x = areas[i].rect.X + areas[i].rect.Width / 2f - w / 2f,
                                            y = areas[i].rect.Y + areas[i].rect.Height / 2f - h / 2f;

                                        if (collapsed)
                                        {
                                            points = new PointF[]
                                            {
                                                new PointF(x, y),
                                                new PointF(x + w, y),
                                                new PointF(x + w / 2f, y + h),
                                            };
                                        }
                                        else
                                        {
                                            points = new PointF[]
                                            {
                                                new PointF(x + w / 2f, y),
                                                new PointF(x + w, y + h),
                                                new PointF(x, y + h),
                                            };
                                        }

                                        g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;

                                        g.FillPolygon(brush, points);
                                        g.DrawPolygon(pen, points);

                                        g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.Default;

                                        break;
                                    case AreaType.Eye:

                                        var c = brush.Color;

                                        if (_ButtonEyeEnabled && _ButtonEyeTimerEnabled && apiTimer != null && apiTimer.Pending)
                                        {
                                            var b = apiTimer.Bounds;

                                            if (apiTimer.Resize)
                                            {
                                                apiTimer.Resize = false;
                                                var bh = (int)(2 * scale + 0.5f);
                                                var bw = areas[i].rect.Width * 8 / 10;

                                                apiTimer.Bounds = b = new Rectangle(
                                                    areas[i].rect.Left + (areas[i].rect.Width - bw) / 2,
                                                    areas[i].rect.Bottom - bh * 2,
                                                    bw,
                                                    bh);
                                            }

                                            var bw1 = (int)(b.Width * apiTimer.Progress + 0.5f);
                                            var bw2 = b.Width - bw1;

                                            if (bw1 > 0)
                                            {
                                                brush.Color = Color.FromArgb(200, c);

                                                g.FillRectangle(brush, b.X, b.Y, bw1, b.Height);
                                            }

                                            if (bw2 > 0)
                                            {
                                                brush.Color = Color.FromArgb(100, c);

                                                g.FillRectangle(brush, b.X + bw1, b.Y, bw2, b.Height);
                                            }
                                            brush.Color = c;
                                        }

                                        g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;

                                        var cEye = new Point(areas[i].rect.X + areas[i].rect.Width / 2, areas[i].rect.Y + areas[i].rect.Height / 2);
                                        var eyeW = 9 * scale;
                                        var eyeH = 5 * scale;

                                        g.FillClosedCurve(brush, new PointF[] 
                                        {
                                            new PointF(cEye.X - eyeW, cEye.Y),
                                            new PointF(cEye.X, cEye.Y-eyeH),
                                            new PointF(cEye.X + eyeW, cEye.Y),
                                            new PointF(cEye.X, cEye.Y+eyeH),
                                        }, System.Drawing.Drawing2D.FillMode.Winding, 0.65f);

                                        eyeH = eyeH * 3 / 4;
                                        brush.Color = this.BackColor;
                                        g.FillEllipse(brush, cEye.X - eyeH, cEye.Y - eyeH, eyeH * 2, eyeH * 2);

                                        brush.Color = c;

                                        if (!_ButtonEyeEnabled)
                                        {
                                            c = pen.Color;

                                            pen.Color = Color.FromArgb(200, 0, 0);
                                            pen.Width = 2;
                                            pen.EndCap = System.Drawing.Drawing2D.LineCap.Flat;

                                            g.DrawLine(pen, cEye.X - eyeW, cEye.Y + eyeH, cEye.X + eyeW, cEye.Y - eyeH);

                                            pen.Color = c;
                                        }

                                        g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.Default;

                                        break;
                                }
                            }
                        }
                    }
                }
            }

            buffer.Render(e.Graphics);

            base.OnPaint(e);
        }

        protected override void OnPaintBackground(PaintEventArgs e)
        {
        }

        public void ShowDropDown()
        {
            if (!_ButtonDropDownArrowVisible || _DropDownItems == null || _DropDownItems.Length <= 1)
                return;

            if (combo == null)
            {
                combo = new ComboBox()
                {
                    Visible = false,
                    Width = 0,
                    Anchor = AnchorStyles.Left | AnchorStyles.Right,
                    DropDownStyle = ComboBoxStyle.DropDownList,
                    DropDownWidth = this.Width - this.Padding.Left * 2,
                };
                this.Controls.Add(combo);
                combo.Location = new Point(this.Padding.Left, (this.Height - combo.Height) / 2);
                combo.DropDownClosed += combo_DropDownClosed;
            }

            if (combo.Items.Count == 0)
            {
                combo.Items.AddRange(_DropDownItems);
            }

            if (combo.SelectedIndex != _DropDownSelectedIndex && _DropDownSelectedIndex < _DropDownItems.Length)
            {
                combo.SelectedIndex = _DropDownSelectedIndex;
            }

            combo.Visible = true;
            combo.Focus();
            combo.DroppedDown = true;
        }

        void combo_DropDownClosed(object sender, EventArgs e)
        {
            combo.Visible = false;

            if (combo.SelectedIndex != _DropDownSelectedIndex && _DropDownItems != null && combo.SelectedIndex < _DropDownItems.Length && combo.Items.Count != 0)
            {
                _DropDownSelectedIndex = combo.SelectedIndex;

                if (DropDownSelectedItemChanged != null)
                {
                    try
                    {
                        DropDownSelectedItemChanged(this, EventArgs.Empty);
                    }
                    catch (Exception x)
                    {
                        Util.Logging.Log(x);
                    }
                }
            }
        }

        void apiTimer_Tick(object sender, EventArgs e)
        {
            if (_ButtonEyeEnabled && apiTimer != null)
            {
                OnRedrawRequired(GetIndex(AreaType.Eye));

            }
        }

        public void SetApi(Settings.IAccount account, Api.ApiData manager, Api.ApiData.DataType type = ApiTimer.ANY_TYPE)
        {
            if (account != null && account.Type == Settings.AccountType.GuildWars2)
            {
                if (apiTimer == null)
                {
                    apiTimer = new ApiTimer(null, manager, type);
                    apiTimer.Enabled = _ButtonEyeTimerEnabled;
                    apiTimer.Tick += apiTimer_Tick;
                }
                apiTimer.SetApi(account, type);
            }
            else if (apiTimer != null)
            {
                apiTimer.Dispose();
                apiTimer = null;
            }
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);

            if (disposing)
            {
                if (apiTimer != null)
                {
                    apiTimer.Dispose();
                    apiTimer = null;
                }

                if (buffer != null)
                {
                    buffer.Dispose();
                    buffer = null;
                }
            }
        }

        public override void RefreshColors()
        {
            OnRedrawRequired();

            base.RefreshColors();
        }
    }
}
