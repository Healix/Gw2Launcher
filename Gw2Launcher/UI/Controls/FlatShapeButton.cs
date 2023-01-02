using System;
using System.Text;
using System.Windows.Forms;
using System.Drawing;
using System.ComponentModel;

namespace Gw2Launcher.UI.Controls
{
    class FlatShapeButton : FlatButton
    {
        public enum IconShape
        {
            Arrow,
            Diamond,
            Square,
            Ellipse,
            X,
            Plus,
            Minus,
            MenuLines,
            DoubleArrow,
            ArrowAndLine,
            Underscore,
            SquareAndLine,
            WindowTemplate,
        }

        public FlatShapeButton()
            : base()
        {
        }

        [System.ComponentModel.Browsable(false)]
        [System.ComponentModel.DesignerSerializationVisibility(System.ComponentModel.DesignerSerializationVisibility.Hidden)]
        public override string Text
        {
            get
            {
                return base.Text;
            }
            set
            {
                base.Text = value;
            }
        }

        protected ContentAlignment shapeAlignment = ContentAlignment.MiddleCenter;
        [DefaultValue(ContentAlignment.MiddleCenter)]
        public ContentAlignment ShapeAlignment
        {
            get
            {
                return shapeAlignment;
            }
            set
            {
                if (shapeAlignment != value)
                {
                    shapeAlignment = value;
                    OnRedrawRequired();
                }
            }
        }

        protected Size shapeSize;
        public Size ShapeSize
        {
            get
            {
                if (shapeSize.IsEmpty)
                    shapeSize = new Size(4, 8);
                return shapeSize;
            }
            set
            {
                if (shapeSize != value)
                {
                    shapeSize = value;
                    OnRedrawRequired();
                }
            }
        }

        protected int lineSize = 2;
        [DefaultValue(2)]
        public int LineSize
        {
            get
            {
                return lineSize;
            }
            set
            {
                if (lineSize != value)
                {
                    lineSize = value;
                    OnRedrawRequired();
                }
            }
        }

        protected ArrowDirection shapeDirection = ArrowDirection.Left;
        [DefaultValue(ArrowDirection.Left)]
        public ArrowDirection ShapeDirection
        {
            get
            {
                return shapeDirection;
            }
            set
            {
                if (shapeDirection != value)
                {
                    shapeDirection = value;
                    OnRedrawRequired();
                }
            }
        }

        protected IconShape shape = IconShape.Arrow;
        [DefaultValue(IconShape.Arrow)]
        public IconShape Shape
        {
            get
            {
                return shape;
            }
            set
            {
                if (shape != value)
                {
                    shape = value;
                    OnRedrawRequired();
                }
            }
        }

        private PointF[] GetShapeArrow(float x, float y, float w, float h)
        {
            switch (shapeDirection)
            {
                case System.Windows.Forms.ArrowDirection.Up:

                    return new PointF[]
                    {
                        new PointF(x + w / 2f, y),
                        new PointF(x + w, y + h),
                        new PointF(x, y + h),
                    };

                case System.Windows.Forms.ArrowDirection.Down:

                    return new PointF[]
                    {
                        new PointF(x, y),
                        new PointF(x+w, y),
                        new PointF(x + w / 2f, y+h),
                    };

                case System.Windows.Forms.ArrowDirection.Right:

                    return new PointF[]
                    {
                        new PointF(x, y),
                        new PointF(x + w, y + h / 2f),
                        new PointF(x, y + h),
                    };

                case System.Windows.Forms.ArrowDirection.Left:
                default:

                    return new PointF[]
                    {
                        new PointF(x, y + h /2f),
                        new PointF(x + w, y),
                        new PointF(x + w, y + h),
                    };
            }
        }

        private PointF[] GetShapeSquare(float x, float y, float w, float h)
        {
            return new PointF[]
                    {
                        new PointF(x, y),
                        new PointF(x + w, y),
                        new PointF(x + w, y + h),
                        new PointF(x, y+h),
                    };
        }

        private PointF[] GetShapeDiamond(float x, float y, float w, float h)
        {
            return new PointF[]
                    {
                        new PointF(x + w / 2f, y),
                        new PointF(x + w, y + h / 2f),
                        new PointF(x + w / 2f, y + h),
                        new PointF(x, y+h / 2f),
                    };
        }

        private Point GetPosition(int w, int h)
        {
            int x, y;

            switch (shapeAlignment)
            {
                case ContentAlignment.BottomLeft:
                case ContentAlignment.MiddleLeft:
                case ContentAlignment.TopLeft:

                    x = Padding.Left;

                    break;
                case ContentAlignment.BottomRight:
                case ContentAlignment.MiddleRight:
                case ContentAlignment.TopRight:

                    x = this.Width - Padding.Right - w;

                    break;
                case ContentAlignment.BottomCenter:
                case ContentAlignment.MiddleCenter:
                case ContentAlignment.TopCenter:
                default:

                    x = Padding.Left + (this.Width - Padding.Horizontal - w) / 2;

                    break;
            }

            switch (shapeAlignment)
            {
                case ContentAlignment.BottomCenter:
                case ContentAlignment.BottomLeft:
                case ContentAlignment.BottomRight:

                    y = this.Height - h - Padding.Bottom;

                    break;
                case ContentAlignment.TopCenter:
                case ContentAlignment.TopLeft:
                case ContentAlignment.TopRight:

                    y = Padding.Top;

                    break;
                case ContentAlignment.MiddleCenter:
                case ContentAlignment.MiddleLeft:
                case ContentAlignment.MiddleRight:
                default:

                    y = Padding.Top + (this.Height - Padding.Vertical - h) / 2;

                    break;
            }

            return new Point(x, y);
        }

        protected override void OnPaintBuffer(Graphics g)
        {
            var scale = g.DpiX / 96f;
            int w = (int)(shapeSize.Width * scale + 0.5f),
                h = (int)(shapeSize.Height * scale + 0.5f),
                lineSize;
            Point p;

            switch (shape)
            {
                case IconShape.WindowTemplate:

                    using (var brush = new SolidBrush(ForeColorCurrent))
                    {
                        using (var pen = new Pen(brush))
                        {
                            lineSize = (int)(this.lineSize * scale + 0.5f);
                            var spacing = (int)(scale * 2 + 0.5f);

                            switch (shapeDirection)
                            {
                                case ArrowDirection.Up:
                                case ArrowDirection.Down:
                                    p = GetPosition(w, h + lineSize + spacing - 1);
                                    break;
                                case ArrowDirection.Right:
                                case ArrowDirection.Left:
                                default:
                                    p = GetPosition(w + lineSize + spacing - 1, h);
                                    break;
                            }

                            Rectangle r1, r2;
                            int sz;

                            switch (shapeDirection)
                            {
                                case ArrowDirection.Left:
                                    sz = h / 2 - spacing / 2;
                                    r1 = new Rectangle(p.X, p.Y, lineSize, sz);
                                    r2 = new Rectangle(p.X, p.Y + h - sz, lineSize, sz);
                                    p.X += lineSize + spacing;
                                    break;
                                case ArrowDirection.Right:
                                    sz = h / 2 - spacing / 2;
                                    r1 = new Rectangle(p.X + w + spacing, p.Y, lineSize, sz);
                                    r2 = new Rectangle(p.X + w + spacing, p.Y + h - sz, lineSize, sz);
                                    break;
                                case ArrowDirection.Up:
                                    sz = w / 2 - spacing / 2;
                                    r1 = new Rectangle(p.X, p.Y, sz, lineSize);
                                    r2 = new Rectangle(p.X + w - sz, p.Y, sz, lineSize);
                                    p.Y += lineSize + spacing;
                                    break;
                                default:
                                case ArrowDirection.Down:
                                    sz = w / 2 - spacing / 2;
                                    r1 = new Rectangle(p.X, p.Y + h + spacing, sz, lineSize);
                                    r2 = new Rectangle(p.X + w - sz, p.Y + h + spacing, sz, lineSize);
                                    break;
                            }

                            if (borderColor.A > 0)
                            {
                                pen.Color = borderColor;

                                g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
                                g.DrawRectangle(pen, p.X - 0.5f, p.Y - 0.5f, w, h);
                                g.DrawRectangle(pen, r1.X - 0.5f, r1.Y - 0.5f, r1.Width, r1.Height);
                                g.DrawRectangle(pen, r2.X - 0.5f, r2.Y - 0.5f, r2.Width, r2.Height);
                                g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.Default;
                            }

                            g.FillRectangle(brush, p.X, p.Y, w, h);
                            g.FillRectangle(brush, r1);
                            g.FillRectangle(brush, r2);
                        }
                    }

                    break;
                case IconShape.SquareAndLine:

                    using (var brush = new SolidBrush(ForeColorCurrent))
                    {
                        using (var pen = new Pen(brush))
                        {
                            lineSize = (int)(this.lineSize * scale + 0.5f);

                            switch (shapeDirection)
                            {
                                case ArrowDirection.Up:
                                case ArrowDirection.Down:
                                    p = GetPosition(w, h + lineSize);
                                    break;
                                case ArrowDirection.Right:
                                case ArrowDirection.Left:
                                default:
                                    p = GetPosition(w + lineSize, h);
                                    break;
                            }

                            var spacing = (int)(scale * 2 + 0.5f);
                            Rectangle r;

                            switch (shapeDirection)
                            {
                                case ArrowDirection.Left:
                                    r = new Rectangle(p.X - spacing, p.Y, lineSize, h);
                                    p.X += lineSize + 1;
                                    break;
                                case ArrowDirection.Right:
                                    r = new Rectangle(p.X + w + spacing, p.Y, lineSize, h);
                                    w -= 1;
                                    break;
                                case ArrowDirection.Up:
                                    r = new Rectangle(p.X, p.Y - spacing, w, lineSize);
                                    p.Y += lineSize + 1;
                                    break;
                                default:
                                case ArrowDirection.Down:
                                    r = new Rectangle(p.X, p.Y + h + spacing, w, lineSize);
                                    h -= 1;
                                    break;
                            }

                            if (borderColor.A > 0)
                            {
                                pen.Color = borderColor;

                                g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
                                g.DrawPolygon(pen, GetShapeSquare(p.X - 0.5f, p.Y - 0.5f, w, h));
                                g.DrawRectangle(pen, r.X - 0.5f, r.Y - 0.5f, r.Width, r.Height);
                                g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.Default;
                            }

                            g.FillRectangle(brush, r);
                            g.FillPolygon(brush, GetShapeSquare(p.X, p.Y, w, h));
                        }
                    }

                    break;
                case IconShape.ArrowAndLine:

                    using (var brush = new SolidBrush(ForeColorCurrent))
                    {
                        using (var pen = new Pen(brush))
                        {
                            lineSize = (int)(this.lineSize * scale + 0.5f);

                            switch (shapeDirection)
                            {
                                case ArrowDirection.Up:
                                case ArrowDirection.Down:
                                    p = GetPosition(w, h + lineSize);
                                    break;
                                case ArrowDirection.Right:
                                case ArrowDirection.Left:
                                default:
                                    p = GetPosition(w + lineSize, h);
                                    break;
                            }

                            var points1 = GetShapeArrow(p.X, p.Y, w, h);

                            g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
                            g.FillPolygon(brush, points1);
                            g.DrawPolygon(pen, points1);

                            g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.Default;

                            switch (shapeDirection)
                            {
                                case ArrowDirection.Left:
                                    g.FillRectangle(brush, p.X - lineSize, p.Y, lineSize, h + 1);
                                    break;
                                case ArrowDirection.Right:
                                    g.FillRectangle(brush, p.X + w, p.Y, lineSize, h + 1);
                                    break;
                                case ArrowDirection.Up:
                                    g.FillRectangle(brush, p.X, p.Y - lineSize, w + 1, lineSize);
                                    break;
                                case ArrowDirection.Down:
                                    g.FillRectangle(brush, p.X, p.Y + h, w + 1, lineSize);
                                    break;
                            }
                        }
                    }

                    break;
                case IconShape.DoubleArrow:

                    using (var brush = new SolidBrush(ForeColorCurrent))
                    {
                        using (var pen = new Pen(brush))
                        {
                            switch (shapeDirection)
                            {
                                case ArrowDirection.Up:
                                case ArrowDirection.Down:
                                    p = GetPosition(w, h * 2);
                                    break;
                                case ArrowDirection.Right:
                                case ArrowDirection.Left:
                                default:
                                    p = GetPosition(w * 2, h);
                                    break;
                            }

                            PointF[] points1;

                            g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;

                            points1 = GetShapeArrow(p.X, p.Y, w, h);
                            g.FillPolygon(brush, points1);
                            g.DrawPolygon(pen, points1);

                            switch (shapeDirection)
                            {
                                case ArrowDirection.Up:
                                case ArrowDirection.Down:
                                    points1 = GetShapeArrow(p.X, p.Y + h, w, h);
                                    break;
                                case ArrowDirection.Right:
                                case ArrowDirection.Left:
                                default:
                                    points1 = GetShapeArrow(p.X + w, p.Y, w, h);
                                    break;
                            }

                            g.FillPolygon(brush, points1);
                            g.DrawPolygon(pen, points1);
                        }
                    }

                    break;
                case IconShape.Underscore:

                    lineSize = (int)(this.lineSize * scale + 0.5f);

                    using (var pen = new Pen(ForeColorCurrent, lineSize))
                    {
                        pen.StartCap = System.Drawing.Drawing2D.LineCap.Flat;
                        pen.EndCap = System.Drawing.Drawing2D.LineCap.Flat;

                        p = GetPosition(w, h);

                        var y = p.Y + h - this.lineSize * scale / 2f;
                        if (borderColor.A > 0)
                        {
                            g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
                            pen.Color = borderColor;
                            pen.Width = lineSize + 1f;

                            g.DrawLine(pen, p.X - 0.5f, y - 0.5f, p.X + w + 0.5f, y - 0.5f);

                            pen.Color = ForeColorCurrent;
                            pen.Width = lineSize;
                        }

                        g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.Default;
                        g.DrawLine(pen, p.X, y, p.X + w, y);
                    }

                    break;
                case IconShape.Minus:

                    using (var brush = new SolidBrush(ForeColorCurrent))
                    {
                        lineSize = (int)(this.lineSize * scale + 0.5f);
                        p = GetPosition(w, lineSize);

                        g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.Default;
                        g.FillRectangle(brush, p.X, p.Y, w, lineSize);
                    }

                    break;
                case IconShape.Plus:

                    lineSize = (int)(this.lineSize * scale + 0.5f);

                    using (var pen = new Pen(ForeColorCurrent, lineSize))
                    {
                        pen.Alignment = System.Drawing.Drawing2D.PenAlignment.Center;
                        w = (w - lineSize) / 2;
                        h = (h - lineSize) / 2;

                        p = GetPosition(w * 2 + lineSize, h * 2 + lineSize);

                        if (borderColor.A > 0)
                        {
                            g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
                            pen.Color = borderColor;
                            pen.Width = lineSize + 1f;

                            float xf, yf;

                            xf = p.X + w + lineSize / 2f - 0.5f;
                            yf = p.Y + h * 2 + lineSize;
                            g.DrawLine(pen, xf, p.Y - 0.5f, xf, yf);

                            xf = p.X + w * 2 + lineSize;
                            yf = p.Y + h + lineSize / 2f - 0.5f;
                            g.DrawLine(pen, p.X - 0.5f, yf, xf, yf);

                            pen.Color = ForeColorCurrent;
                            pen.Width = lineSize;
                        }

                        g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.Default;

                        int x, y;
                        x = p.X + w + lineSize / 2;
                        y = p.Y + h * 2 + lineSize;
                        if (lineSize == 1)
                            --y;
                        g.DrawLine(pen, x, p.Y, x, y);

                        x = p.X + w * 2 + lineSize;
                        y = p.Y + h + lineSize / 2;
                        if (lineSize == 1)
                            --x;
                        g.DrawLine(pen, p.X, y, x, y);
                    }

                    break;
                case IconShape.MenuLines:

                    using (var brush = new SolidBrush(ForeColorCurrent))
                    {
                        lineSize = (int)(this.lineSize * scale + 0.5f);
                        h = (h - lineSize * 3) / 2;
                        p = GetPosition(w, h * 2 + lineSize * 3);


                        var y = p.Y;

                        if (borderColor.A > 0)
                        {
                            using (var pen = new Pen(borderColor))
                            {
                                g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
                                g.DrawRectangle(pen, p.X - 0.5f, y - 0.5f, w, lineSize);
                                y += lineSize + h;
                                g.DrawRectangle(pen, p.X - 0.5f, y - 0.5f, w, lineSize);
                                y += lineSize + h;
                                g.DrawRectangle(pen, p.X - 0.5f, y - 0.5f, w, lineSize);
                            }
                            y = p.Y;
                        }

                        g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.Default;

                        g.FillRectangle(brush, p.X, y, w, lineSize);
                        y += lineSize + h;
                        g.FillRectangle(brush, p.X, y, w, lineSize);
                        y += lineSize + h;
                        g.FillRectangle(brush, p.X, y, w, lineSize);
                    }

                    break;
                case IconShape.Ellipse:

                    using (var brush = new SolidBrush(ForeColorCurrent))
                    {
                        using (var pen = new Pen(brush))
                        {
                            p = GetPosition(w, h);

                            g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
                            g.FillEllipse(brush, p.X, p.Y, w, h);
                            g.DrawEllipse(pen, p.X, p.Y, w, h);
                        }
                    }

                    break;
                case IconShape.X:

                    using (var brush = new SolidBrush(ForeColorCurrent))
                    {
                        lineSize = (int)(this.lineSize * scale + 0.5f);

                        using (var pen = new Pen(brush, lineSize))
                        {
                            p = GetPosition(w, h);

                            g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
                            pen.StartCap = System.Drawing.Drawing2D.LineCap.Flat;
                            pen.EndCap = System.Drawing.Drawing2D.LineCap.Flat;

                            if (borderColor.A > 0)
                            {
                                pen.Width = lineSize + 1.5f;
                                pen.Color = borderColor;

                                var xf = p.X - 0.5f;
                                var yf = p.Y - 0.5f;
                                var wf = w + 1f;
                                var hf = h + 1f;

                                g.DrawLine(pen, xf, yf, xf + wf, yf + hf);
                                g.DrawLine(pen, xf, yf + hf, xf + wf, yf);

                                pen.Width = lineSize;
                                pen.Color = ForeColorCurrent;
                            }

                            g.DrawLine(pen, p.X, p.Y, p.X + w, p.Y + h);
                            g.DrawLine(pen, p.X, p.Y + h, p.X + w, p.Y);
                        }
                    }

                    break;
                default:

                    p = GetPosition(w, h);
                    PointF[] points;

                    switch (shape)
                    {
                        case FlatShapeButton.IconShape.Diamond:

                            points = GetShapeDiamond(p.X, p.Y, w, h);

                            break;
                        case FlatShapeButton.IconShape.Square:

                            points = GetShapeSquare(p.X, p.Y, w, h);

                            break;
                        case FlatShapeButton.IconShape.Arrow:
                        default:

                            points = GetShapeArrow(p.X, p.Y, w, h);

                            break;
                    }

                    using (var brush = new SolidBrush(ForeColorCurrent))
                    {
                        using (var pen = new Pen(brush))
                        {
                            g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;

                            if (borderColor.A > 0 && shape == IconShape.Arrow)
                            {
                                pen.Width = 1.5f;
                                pen.Color = borderColor;

                                g.DrawPolygon(pen, GetShapeArrow(p.X - 0.5f, p.Y - 0.5f, w + 1f, h + 1f));

                                pen.Width = 1f;
                                pen.Color = ForeColorCurrent;
                            }

                            g.FillPolygon(brush, points);
                            g.DrawPolygon(pen, points);
                        }
                    }

                    break;
            }
        }
    }
}
