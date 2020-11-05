using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Gw2Launcher.UI.Controls
{
    public partial class ProgressGraph : UserControl
    {
        private const byte PIXELS_PER_INDEX = 2;

        private Bitmap graph, marker;
        private List<PointF> samples;
        private float maxValue;
        private float minValue;
        private double currentAvg;
        private int currentAvgCount;
        private int currentIndex;
        private System.Drawing.Drawing2D.GraphicsPath pathGraph;
        private SolidBrush brushGraph, brushText;
        private Pen penGraph, penLine;
        private int drawIndex;
        private byte borderSize;
        private int maxIndex;
        private Rectangle textBoundsUpper, textBoundsLower;
        private string textUpper, textLower;
        private string textFormat;

        public ProgressGraph()
        {
            samples = new List<PointF>();
            InitializeComponent();

            textFormat = "{0}";

            borderSize = 1;
            pathGraph = new System.Drawing.Drawing2D.GraphicsPath();
            minValue = float.MaxValue;
            brushGraph = new SolidBrush(Color.FromArgb((int)(255*0.3f),0,255,0));
            penGraph = new Pen(Color.Green);

            brushText = new SolidBrush(Color.FromArgb((int)(255 * 0.6f), 0, 0, 0));
            penLine = new Pen(Color.FromArgb((int)(255 * 0.3f), 0, 0, 0));
            penLine.DashStyle = System.Drawing.Drawing2D.DashStyle.Custom;
            penLine.DashPattern = new float[] { 3, 5 };
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (components != null)
                    components.Dispose();
            }

            base.Dispose(disposing);

            if (disposing)
            {
                penGraph.Dispose();
                penLine.Dispose();
                brushGraph.Dispose();
                brushText.Dispose();
                pathGraph.Dispose();
            }
        }

        private void ProgressGraph_Load(object sender, EventArgs e)
        {

        }

        protected override void OnBackColorChanged(EventArgs e)
        {
            base.OnBackColorChanged(e);

            if (samples != null && samples.Count > 0)
            {
                throw new NotSupportedException();
            }
        }

        public string TextFormat
        {
            get
            {
                return textFormat;
            }
            set
            {
                textFormat = value;
            }
        }

        public Color GraphColor
        {
            get
            {
                return brushGraph.Color;
            }
            set
            {
                brushGraph.Color = value;
            }
        }

        public Color GraphLineColor
        {
            get
            {
                return penGraph.Color;
            }
            set
            {
                penGraph.Color = value;
            }
        }

        protected override void OnSizeChanged(EventArgs e)
        {
            base.OnSizeChanged(e);

            if (samples != null && samples.Count > 0)
            {
                throw new NotSupportedException();
            }
        }

        public void Clear()
        {
            if (samples.Count > 0)
            {
                samples.Clear();
                maxValue = 0;
                minValue = float.MaxValue;
                currentIndex = 0;
                currentAvgCount = 0;
                currentAvg = 0;

                if (graph != null)
                {
                    using (var g = Graphics.FromImage(graph))
                    {
                        g.Clear(this.BackColor);
                    }
                }

                this.Invalidate();
            }
        }

        public void AddSample(float progress, float value)
        {
            if (graph == null)
            {
                graph = new Bitmap(this.Width - 2 * borderSize, this.Height - 2 * borderSize);
                marker = new Bitmap(graph.Width, (int)(this.Font.GetHeight(graph.VerticalResolution) + 0.5f));
                drawIndex = -1;
                maxIndex = (int)((graph.Width - 1) * 1f / PIXELS_PER_INDEX);
            }

            int width = graph.Width;
            int index = (int)((graph.Width - 1) * progress / PIXELS_PER_INDEX);
            int count;
            float valueAvg;
            bool redrawBounds = false;

            if (currentIndex == index)
            {
                currentAvg += value;
                currentAvgCount++;

                return;
            }
            else if (currentAvgCount > 0)
            {
                valueAvg = (float)(currentAvg / currentAvgCount);

                count = samples.Count;

                if (count == 0 && currentIndex != 0)
                {
                    samples.Add(new PointF(0, valueAvg));
                    count++;
                }

                samples.Add(new PointF(currentIndex, valueAvg));
                count++;

                if (valueAvg >= maxValue)
                {
                    maxValue = valueAvg * 1.01f;
                    drawIndex = -1;
                    redrawBounds = true;
                }
                if (valueAvg < minValue)
                {
                    minValue = valueAvg * 0.99f;
                    drawIndex = -1;
                    redrawBounds = true;
                }

                currentIndex = index;
                currentAvg = value;
                currentAvgCount = 1;
            }
            else
            {
                currentIndex = index;
                currentAvg = value;
                currentAvgCount = 1;

                return;
            }

            using (var g = Graphics.FromImage(graph))
            {
                int height = graph.Height;
                float range = maxValue - minValue;
                int i = 0;
                Point[] lines;
                Point startpoint, endpoint;

                if (drawIndex < 0)
                    drawIndex = 0;
                drawIndex = 0;
                redrawBounds = true;

                lines = new Point[count - drawIndex + 1];

                //previous lines within the area that needs to be updated
                for (int j = drawIndex; j < count; j++)
                {
                    PointF s = samples[j];
                    lines[i++] = new Point(
                        (int)(s.X * PIXELS_PER_INDEX),
                        (int)(height * (1 - (s.Y - minValue) / range)));
                }

                //adding on the current point using the previous average value, unless it's the last point, then use the current average and move it to the end of the graph
                float currentValue;
                if (index == maxIndex)
                {
                    currentValue = (float)(currentAvg / currentAvgCount);
                    lines[i++] = new Point(
                            width,
                            (int)(height * (1 - (currentValue - minValue) / range)));
                }
                else
                {
                    currentValue = valueAvg;
                    lines[i++] = new Point(
                            (int)(index * PIXELS_PER_INDEX),
                            (int)(height * (1 - (currentValue - minValue) / range)));
                }

                startpoint = lines[0];
                endpoint = lines[count - drawIndex];

                pathGraph.Reset();

                //lines path with added beginning and end
                pathGraph.AddLine(new Point(startpoint.X, height), startpoint);
                pathGraph.AddLines(lines);
                pathGraph.AddLine(endpoint, new Point(endpoint.X + 1, height));

                //clipping the area to prevent aliased paths from overlapping
                Rectangle clip;
                if (redrawBounds)
                {
                    clip = new Rectangle(0, 0, width, height);
                    g.SetClip(clip);
                    g.Clear(this.BackColor);

                    textUpper = string.Format(textFormat, range * 0.75 + minValue);
                    textLower = string.Format(textFormat, range * 0.25 + minValue);

                    SizeF size;
                    int w, h;

                    size = g.MeasureString(textLower, this.Font);
                    w = (int)(size.Width + 0.5f);
                    h = (int)(size.Height + 0.5f);
                    textBoundsLower = new Rectangle(width - 2 - w, (int)(height * 0.75f) - h / 2, w, h);

                    size = g.MeasureString(textUpper, this.Font);
                    w = (int)(size.Width + 0.5f);
                    h = (int)(size.Height + 0.5f);
                    textBoundsUpper = new Rectangle(width - 2 - w, (int)(height * 0.25f) - h / 2, w, h);
                }
                else
                {
                    clip = new Rectangle(startpoint.X, 0, endpoint.X - startpoint.X + 1, height);
                    g.SetClip(clip);
                    g.Clear(this.BackColor);
                }

                #region draw clip background

                int x2 = clip.Right;

                if (x2 >= textBoundsLower.X && clip.X < textBoundsLower.Right)
                    g.DrawString(textLower, this.Font, brushText, textBoundsLower.X, textBoundsLower.Y);

                if (x2 >= textBoundsUpper.X && clip.X < textBoundsUpper.Right)
                    g.DrawString(textUpper, this.Font, brushText, textBoundsUpper.X, textBoundsUpper.Y);

                if (clip.X < textBoundsLower.X - 5)
                {
                    int y1 = textBoundsLower.Y + textBoundsLower.Height / 2;
                    g.DrawLine(penLine, 0, y1, textBoundsLower.X - 5, y1);
                    //if (x2 >= textBoundsLower.X - 5)
                    //    g.DrawLine(penLine, clip.X, y1, textBoundsLower.X - 5, y1);
                    //else
                    //    g.DrawLine(penLine, clip.X, y1, x2, y1);
                }

                if (clip.X < textBoundsUpper.X - 5)
                {
                    int y1 = textBoundsUpper.Y + textBoundsUpper.Height / 2;
                    g.DrawLine(penLine, 0, y1, textBoundsUpper.X - 5, y1);
                    //if (x2 >= textBoundsUpper.X - 5)
                    //    g.DrawLine(penLine, clip.X, y1, textBoundsUpper.X - 5, y1);
                    //else
                    //    g.DrawLine(penLine, clip.X, y1, x2, y1);
                }

                #endregion

                g.FillPath(brushGraph, pathGraph);

                g.CompositingQuality = System.Drawing.Drawing2D.CompositingQuality.HighSpeed;
                g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;

                pathGraph.Reset();

                if (drawIndex > 0)
                {
                    //add the previous point to create an accurate line
                    //this point is outside the clip and will not be drawn
                    PointF s = samples[drawIndex - 1];
                    var previous = new Point(
                        (int)(s.X * PIXELS_PER_INDEX),
                        (int)(height * (1 - (s.Y - minValue) / range)));
                    pathGraph.AddLine(previous, startpoint);
                }

                pathGraph.AddLines(lines);

                g.DrawPath(penGraph, pathGraph);

                //the next draw includes the previous point to overwrite the current value
                drawIndex = count - 1;

                this.Invalidate(new Rectangle(clip.X + borderSize, clip.Y + borderSize, clip.Width, clip.Height));

                //this.Invalidate(new Rectangle(lineLocation, new Size(width, 1)));
                //this.Invalidate(textBounds);

                //lineLocation = new Point(borderSize + 1, endpoint.Y);

                //text = string.Format("{0:0} MB/s", currentValue);
                //SizeF size1 = g.MeasureString(text, this.Font);
                //textBounds = new Rectangle(borderSize + width - (int)(size1.Width + 0.5f) - 1, endpoint.Y - (int)(size1.Height + 0.5f) / 2, (int)(size1.Width + 0.5f), (int)(size1.Height + 0.5f));
                //if (textBounds.Y < 1 + borderSize)
                //    textBounds.Y = 1 + borderSize;
                //else if (textBounds.Bottom > height + borderSize - 1)
                //    textBounds.Y = height - borderSize - 1 - textBounds.Height;

                //if (pathText == null)
                //    pathText = new System.Drawing.Drawing2D.GraphicsPath();
                //else
                //    pathText.Reset();

                //int st = (int)this.Font.Style;
                //float emSize = g.DpiY * this.Font.SizeInPoints / 72;
                //pathText.AddString(text, this.Font.FontFamily, st, emSize, textBounds.Location, StringFormat.GenericDefault);

                //this.Invalidate(new Rectangle(lineLocation, new Size(width, 1)));
                //this.Invalidate(textBounds);                
            }
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);

            var g = e.Graphics;

            g.DrawRectangle(Pens.Black, 0, 0, this.Width - 1, this.Height - 1);

            if (graph != null)
            {
                g.DrawImage(graph, e.ClipRectangle, new Rectangle(e.ClipRectangle.X - borderSize, e.ClipRectangle.Y - borderSize, e.ClipRectangle.Width, e.ClipRectangle.Height), GraphicsUnit.Pixel);

                //g.DrawString(text, this.Font, Brushes.Black, textBounds.X, textBounds.Y);
                //g.DrawLine(new Pen(new SolidBrush(Color.FromArgb((int)(255 * 0.2), 0, 0, 0))), lineLocation, new Point(textBounds.X - lineLocation.X - 5, lineLocation.Y));

                //pathText.FillMode = System.Drawing.Drawing2D.FillMode.Alternate;
                //g.CompositingQuality = System.Drawing.Drawing2D.CompositingQuality.HighSpeed;
                //g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality ;
                //g.PixelOffsetMode = System.Drawing.Drawing2D.PixelOffsetMode.HighQuality;

                //g.DrawPath(new Pen(Color.White, 1f), pathText);
                //g.FillPath(Brushes.Black, pathText);
            }
        }
    }
}
