using System;
using System.Text;
using System.Windows.Forms;
using System.Drawing;

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
            X
        }

        public FlatShapeButton()
            : base()
        {

        }

        [System.ComponentModel.Browsable(false)]
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

        protected ContentAlignment shapeAlignment;
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
                    return new Size(5, 9);
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

        protected ArrowDirection shapeDirection;
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

        protected IconShape shape;
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

        protected override void OnPaintBuffer(Graphics g)
        {
            float x, y, w, h;

            w = shapeSize.Width - 1;
            h = shapeSize.Height - 1;

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

                    x = this.Width / 2f - w / 2f;

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

                    y = this.Height / 2f - h / 2f;

                    break;
            }

            switch (shape)
            {
                case IconShape.Ellipse:

                    using (var brush = new SolidBrush(ForeColorCurrent))
                    {
                        using (var pen = new Pen(brush))
                        {
                            g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
                            g.FillEllipse(brush, x, y, w, h);
                            g.DrawEllipse(pen, x, y, w, h);
                        }
                    }

                    break;
                case IconShape.X:

                    using (var brush = new SolidBrush(ForeColorCurrent))
                    {
                        using (var pen = new Pen(brush, 2f))
                        {
                            pen.StartCap = System.Drawing.Drawing2D.LineCap.Flat;
                            pen.EndCap = System.Drawing.Drawing2D.LineCap.Flat;

                            g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;

                            if (borderColor.A > 0)
                            {
                                pen.Width = 3.5f;
                                pen.Color = borderColor;

                                x -= 0.5f;
                                y -= 0.5f;
                                w += 1f;
                                h += 1f;

                                g.DrawLine(pen, x, y, x + w, y + h);
                                g.DrawLine(pen, x, y + h, x + w, y);

                                pen.Width = 2f;
                                pen.Color = ForeColorCurrent;

                                x += 0.5f;
                                y += 0.5f;
                                w -= 1f;
                                h -= 1f;

                                g.DrawLine(pen, x, y, x + w, y + h);
                                g.DrawLine(pen, x, y + h, x + w, y);
                            }

                            g.DrawLine(pen, x, y, x + w, y + h);
                            g.DrawLine(pen, x, y + h, x + w, y);
                        }
                    }

                    break;
                default:

                    PointF[] points;

                    switch (shape)
                    {
                        case FlatShapeButton.IconShape.Diamond:

                            points = GetShapeDiamond(x, y, w, h);

                            break;
                        case FlatShapeButton.IconShape.Square:

                            points = GetShapeSquare(x, y, w, h);

                            break;
                        case FlatShapeButton.IconShape.Arrow:
                        default:

                            points = GetShapeArrow(x, y, w, h);

                            break;
                    }

                    using (var brush = new SolidBrush(ForeColorCurrent))
                    {
                        using (var pen = new Pen(brush))
                        {
                            g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
                            g.FillPolygon(brush, points);
                            g.DrawPolygon(pen, points);
                        }
                    }

                    break;
            }
        }
    }
}
