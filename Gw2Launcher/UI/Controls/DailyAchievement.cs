using System;
using System.Text;
using System.Windows.Forms;
using System.Drawing;
using Gw2Launcher.Api;

namespace Gw2Launcher.UI.Controls
{
    class DailyAchievement : System.Windows.Forms.Control
    {
        private BufferedGraphics buffer;
        private bool redraw, resize;

        private class Control
        {
            public Rectangle bounds;
            public bool visible;
        }

        private class Label : Control
        {
            public string value;
            public Font font;
            public Color foreColor;

            public Size Measure(Graphics g, Size proposedSize)
            {
                if (!visible || string.IsNullOrEmpty(value))
                    return Size.Empty;
                return TextRenderer.MeasureText(g, value, font, proposedSize, TextFormatFlags.WordBreak | TextFormatFlags.WordEllipsis);
            }

            public void Draw(Graphics g)
            {
                if (visible && bounds.Height > 0)
                    TextRenderer.DrawText(g, value, font, bounds, foreColor, TextFormatFlags.WordBreak | TextFormatFlags.WordEllipsis);
            }
        }

        private Label labelName, labelDescription, labelLevel;
        private Control icon;
        private Image image;
        private DailyAchievements.Daily daily;

        public DailyAchievement()
        {
            SetStyle(ControlStyles.UserPaint, true);
            SetStyle(ControlStyles.AllPaintingInWmPaint, true);

            labelName = new Label()
            {
                foreColor = this.ForeColor,
                font = this.Font,
            };
            labelDescription = new Label()
            {
                foreColor = this.ForeColor,
                font = this.Font,
            };
            labelLevel = new Label()
            {
                foreColor = SystemColors.GrayText,
                font = this.Font,
            };
            icon = new Control();
        }

        protected override void OnForeColorChanged(EventArgs e)
        {
            base.OnForeColorChanged(e);

            labelName.foreColor = this.ForeColor;
            labelDescription.foreColor = this.ForeColor;
            redraw = resize = true;
            this.Invalidate();
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

        public DailyAchievements.Daily Daily
        {
            get
            {
                return daily;
            }
            set
            {
                if (value == null)
                {
                    daily = null;
                }
                else if (daily != value)
                {
                    daily = value;
                    labelName.value = daily.Name;
                    labelDescription.value = daily.Requirement;
                    labelLevel.value = "Level " + daily.MinLevel + " to " + daily.MaxLevel;
                    image = daily.Icon;
                    redraw = resize = true;
                    this.Invalidate();
                }
            }
        }

        public string NameValue
        {
            get
            {
                return labelName.value;
            }
            set
            {
                labelName.value = value;
                redraw = resize = true;
                this.Invalidate();
            }
        }

        public bool NameVisible
        {
            get
            {
                return labelName.visible;
            }
            set
            {
                labelName.visible = value;
                redraw = resize = true;
                this.Invalidate();
            }
        }

        public Font NameFont
        {
            get
            {
                return labelName.font;
            }
            set
            {
                labelName.font = value;
                redraw = resize = true;
                this.Invalidate();
            }
        }

        public string DescriptionValue
        {
            get
            {
                return labelDescription.value;
            }
            set
            {
                labelDescription.value = value;
                redraw = resize = true;
                this.Invalidate();
            }
        }

        public bool DescriptionVisible
        {
            get
            {
                return labelDescription.visible;
            }
            set
            {
                labelDescription.visible = value;
                redraw = resize = true;
                this.Invalidate();
            }
        }

        public Font DescriptionFont
        {
            get
            {
                return labelDescription.font;
            }
            set
            {
                labelDescription.font = value;
                redraw = resize = true;
                this.Invalidate();
            }
        }

        public string LevelValue
        {
            get
            {
                return labelLevel.value;
            }
            set
            {
                labelLevel.value = value;
                redraw = resize = true;
                this.Invalidate();
            }
        }

        public bool LevelVisible
        {
            get
            {
                return labelLevel.visible;
            }
            set
            {
                labelLevel.visible = value;
                redraw = resize = true;
                this.Invalidate();
            }
        }

        public Font LevelFont
        {
            get
            {
                return labelLevel.font;
            }
            set
            {
                labelLevel.font = value;
                redraw = resize = true;
                this.Invalidate();
            }
        }

        public Image IconValue
        {
            get
            {
                return image;
            }
            set
            {
                image = value;
                if (image != null && icon.bounds.Size.IsEmpty)
                {
                    icon.bounds.Size = image.Size;
                    resize = true;
                }
                redraw = true;
                this.Invalidate();
            }
        }

        public Size IconSize
        {
            get
            {
                return icon.bounds.Size;
            }
            set
            {
                icon.bounds.Size = value;
                redraw = resize = true;
                this.Invalidate();
            }
        }

        public bool IconVisible
        {
            get
            {
                return icon.visible;
            }
            set
            {
                icon.visible = value;
                redraw = resize = true;
                this.Invalidate();
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

            resize = redraw = true;
            this.Invalidate();
        }

        protected void PerformLayout(Graphics g)
        {
            int w = this.Width,
                h = this.Height,
                lw, lx;

            lx = icon.bounds.Width + 20;
            lw = w - lx - 10;

            var sizeName = labelName.Measure(g, new Size(lw, h));

            if (labelDescription.visible)
            {
                labelName.bounds = new Rectangle(new Point(lx, 10), sizeName);
                icon.bounds.Location = new Point(10, 10);

                var gap = (int)(labelName.font.GetHeight(g) / 4 + 0.5f);
                var max = h - sizeName.Height - gap - 20;
                var ly = labelName.bounds.Bottom;

                if (labelLevel.visible)
                {
                    var sizeLevel = labelLevel.Measure(g, new Size(lw, max));
                    labelLevel.bounds = new Rectangle(icon.bounds.X + icon.bounds.Width / 2 - sizeLevel.Width / 2, icon.bounds.Bottom - sizeLevel.Height / 2, sizeLevel.Width, sizeLevel.Height);
                    //labelLevel.bounds = new Rectangle(new Point(lx, ly + gap / 2), sizeLevel);

                    //max -= gap / 2 + sizeLevel.Height;
                    //ly = labelLevel.bounds.Bottom;
                }

                var sizeDescription = labelDescription.Measure(g, new Size(lw, max));
                if (sizeDescription.Height > max)
                    sizeDescription.Height = max;
                labelDescription.bounds = new Rectangle(new Point(lx, ly + gap), sizeDescription);
            }
            else
            {
                labelName.bounds = new Rectangle(new Point(lx, h / 2 - sizeName.Height / 2), sizeName);
                labelDescription.bounds = Rectangle.Empty;
                icon.bounds.Location = new Point(10, h / 2 - icon.bounds.Height / 2);
            }
        }

        public override Size GetPreferredSize(Size proposedSize)
        {
            if (proposedSize.Width > 0)
            {
                int w = proposedSize.Width,
                    h = proposedSize.Height;
                if (h == 0)
                    h = int.MaxValue;

                using (var g = this.CreateGraphics())
                {
                    var lx = icon.bounds.Width + 20;
                    var lw = w - lx - 10;
                    var sizeName = labelName.Measure(g, new Size(lw, h));

                    if (labelDescription.visible)
                    {
                        var gap = (int)(labelName.font.GetHeight(g) / 4 + 0.5f);
                        var max = h - sizeName.Height - gap - 20;

                        //if (labelLevel.visible)
                        //{
                        //    var sizeLevel = labelLevel.Measure(g, new Size(lw, max));
                        //    max -= gap / 2 + sizeLevel.Height;
                        //}

                        var sizeDescription = labelDescription.Measure(g, new Size(lw, max));
                        if (sizeDescription.Height > max)
                            sizeDescription.Height = max;

                        h = sizeDescription.Height + sizeName.Height + gap;
                    }
                    else
                    {
                        h = sizeName.Height;
                    }

                    if (icon.bounds.Height > h)
                        h = icon.bounds.Height;
                    return new Size(w, h + 20);
                }
            }

            return base.GetPreferredSize(proposedSize);
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            if (redraw)
            {
                redraw = false;

                var g = buffer.Graphics;

                if (resize)
                {
                    resize = false;
                    PerformLayout(g);
                }

                if (icon.visible)
                {
                    g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
                    if (image.PixelFormat != System.Drawing.Imaging.PixelFormat.Undefined)
                    {
                        g.DrawImage(image, icon.bounds);
                    }
                }

                labelName.Draw(g);
                labelDescription.Draw(g);
                labelLevel.Draw(g);
            }

            buffer.Render(e.Graphics);

            base.OnPaint(e);
        }

        protected override void OnPaintBackground(PaintEventArgs e)
        {
            if (redraw)
            {
                if (buffer == null)
                    buffer = BufferedGraphicsManager.Current.Allocate(e.Graphics, this.DisplayRectangle);

                buffer.Graphics.Clear(this.BackColor);
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (buffer != null)
                {
                    buffer.Dispose();
                    buffer = null;
                }
            }
            base.Dispose(disposing);
        }
    }
}
