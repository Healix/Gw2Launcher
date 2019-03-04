using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Net;
using System.IO;
using System.IO.Compression;
using System.Runtime.InteropServices;
using System.Drawing.Drawing2D;
using Gw2Launcher.Windows.Native;

namespace Gw2Launcher.UI
{
    public partial class formChangelog : Form
    {
        private class BulletLabel : Label
        {
            public BulletLabel()
                : base()
            {
                this.Padding = new Padding(8, 0, 0, 0);
            }

            protected override void OnPaintBackground(PaintEventArgs pevent)
            {
                base.OnPaintBackground(pevent);

                var g = pevent.Graphics;
                var fh = this.Font.GetHeight(g);
                var sz = 5f;

                g.FillRectangle(Brushes.DarkGray, this.Padding.Left / 2 - sz / 2, fh / 2 - sz / 2 + 1, sz, sz);
            }
        }

        private class LineSeperator : Control
        {
            protected override void OnPaintBackground(PaintEventArgs pevent)
            {
                base.OnPaintBackground(pevent);

                using (var brush = new LinearGradientBrush(this.DisplayRectangle, Color.Empty, Color.Empty, LinearGradientMode.Horizontal))
                {
                    brush.InterpolationColors = new ColorBlend(4)
                    {
                        Positions = new float[] { 0f, 0.5f, 0.5f, 1f },
                        Colors = new Color[] { this.BackColor, this.ForeColor, this.ForeColor, this.BackColor },
                    };
                    pevent.Graphics.FillRectangle(brush, 0, 0, this.Width, this.Height);
                }
            }
        }

        public formChangelog()
        {
            InitializeComponent();

            try
            {
                var sii = new SHSTOCKICONINFO()
                {
                    cbSize = (uint)Marshal.SizeOf(typeof(SHSTOCKICONINFO))
                };
                if (NativeMethods.SHGetStockIconInfo(SHSTOCKICONID.SIID_HELP, SHGSI.SHGSI_ICON, ref sii) == 0)
                {
                    using (var ico = Icon.FromHandle(sii.hIcon))
                    {
                        pictureIcon.Image = ico.ToBitmap();
                    }
                    NativeMethods.DestroyIcon(sii.hIcon);
                }
            }
            catch (Exception e)
            {
                Util.Logging.Log(e);
            }

            if (pictureIcon.Image == null)
            {
                pictureIcon.Image = SystemIcons.Question.ToBitmap();
            }
        }

        protected override void OnMouseWheel(MouseEventArgs e)
        {
            if (e.Delta > 0)
                scrollV.Value -= panelContainer.Height / 3;
            else
                scrollV.Value += panelContainer.Height / 3;

            base.OnMouseWheel(e);
        }

        public async Task<bool> LoadChangelog()
        {
            try
            {
                var request = HttpWebRequest.CreateHttp(formVersionUpdate.UPDATE_BASE_URL + "changelog");
                request.Timeout = 5000;
                request.AutomaticDecompression = DecompressionMethods.Deflate | DecompressionMethods.GZip;

                using (var response = await request.GetResponseAsync())
                {
                    using (var reader = new StreamReader(new GZipStream(response.GetResponseStream(), CompressionMode.Decompress, false)))
                    {
                        return await LoadChangelog(reader);
                    }
                }
            }
            catch (WebException e)
            {
                Util.Logging.Log(e);
                using (e.Response) { }
            }
            catch (Exception e)
            {
                Util.Logging.Log(e);
            }

            return false;
        }

        public async Task<bool> LoadChangelog(StreamReader reader)
        {
            const int INDENT = 15;

            var fontHeader = new System.Drawing.Font("Segoe UI Semibold", 9f, System.Drawing.FontStyle.Bold);
            var fontLight = new System.Drawing.Font("Segoe UI Semilight", 8.25f, System.Drawing.FontStyle.Regular);
            var fontDefault = this.Font;

            int x = 0,
                y = 0;

            Func<bool> onComplete = delegate
            {
                panelContent.Height = y;
                scrollV.Maximum = panelContent.Height - panelContainer.Height;
                scrollV.Visible = scrollV.Maximum > 0;

                return y > 0;
            };

            while (!reader.EndOfStream)
            {
                var line = await reader.ReadLineAsync();

                if (line.Length == 0)
                    continue;

                Label l;

                switch (line[0])
                {
                    case '=':

                        if (line.Length > 1)
                        {
                            int depth = 0;
                            for (int i = 1, li = line.Length; i < li; i++)
                            {
                                if (line[i] == '=')
                                    depth++;
                                else
                                    break;
                            }

                            x = depth * INDENT;

                            if (depth < line.Length)
                            {
                                y += 5;

                                l = new Label()
                                {
                                    Font = fontHeader,
                                    Text = line.Substring(depth + 1),
                                    AutoSize = true,
                                    Location = new Point(x, y),
                                    MaximumSize = new Size(panelContent.Width - x, 0)
                                };
                                panelContent.Controls.Add(l);

                                x += INDENT;
                                y += l.Height + 10;
                            }
                        }
                        else
                            x = 0;

                        break;
                    case '*':
                        
                        l = new BulletLabel()
                        {
                            Font = fontDefault,
                            Text = line.Substring(1),
                            AutoSize = true,
                            Location = new Point(x, y),
                            MaximumSize = new Size(panelContent.Width - x, 0),
                            Padding = new Padding(INDENT, 0, 0, 0),
                        };
                        panelContent.Controls.Add(l);

                        y += l.Height + 5;

                        break;
                    case '#':

                        if (line.StartsWith("#build"))
                        {
                            var parts = line.Split(';');
                            var release = int.Parse(parts[1]);

                            if (release <= Program.RELEASE_VERSION)
                                return onComplete();

                            x = 0;

                            if (panelContent.Controls.Count > 0)
                            {
                                y += 10;

                                var ls = new LineSeperator()
                                {
                                    Location = new Point(panelContent.Width / 8, y),
                                    Size = new Size(panelContent.Width * 3  / 4, 1),
                                    ForeColor = Color.LightGray,
                                };
                                panelContent.Controls.Add(ls);

                                y += ls.Height + 20;
                            }

                            l = new Label()
                            {
                                Font = fontDefault,
                                Text = parts[3],
                                AutoSize = true,
                                Location = new Point(x, y),
                                MaximumSize = new Size(panelContent.Width - x, 0),
                            };
                            panelContent.Controls.Add(l);
                            y += l.Height + 5;

                            l = new Label()
                            {
                                Font = fontLight,
                                ForeColor = SystemColors.GrayText,
                                Text = "build " + parts[2] + " | release " + parts[1],
                                AutoSize = true,
                                Location = new Point(l.Right + 1, l.Top),
                                MaximumSize = new Size(panelContent.Width - x, 0),
                            };
                            panelContent.Controls.Add(l);
                        }

                        break;
                    default:

                        l = new Label()
                        {
                            Font = fontDefault,
                            Text = line,
                            AutoSize = true,
                            Location = new Point(x, y),
                            MaximumSize = new Size(panelContent.Width - x, 0),
                        };
                        panelContent.Controls.Add(l);

                        y += l.Height + 5;

                        break;
                }
            }

            return onComplete();
        }

        private void buttonNo_Click(object sender, EventArgs e)
        {
            this.DialogResult = System.Windows.Forms.DialogResult.No;
        }

        private void buttonYes_Click(object sender, EventArgs e)
        {
            this.DialogResult = System.Windows.Forms.DialogResult.Yes;
        }

        private void scrollV_ValueChanged(object sender, int e)
        {
            panelContent.Top = -e;
        }

        protected override void OnSizeChanged(EventArgs e)
        {
            base.OnSizeChanged(e);

            scrollV.Maximum = panelContent.Height - panelContainer.Height;
            scrollV.Visible = scrollV.Maximum > 0;
        }

        protected override void WndProc(ref Message m)
        {
            base.WndProc(ref m);

            switch ((WindowMessages)m.Msg)
            {
                case WindowMessages.WM_NCHITTEST:

                    switch ((HitTest)m.Result)
                    {
                        case HitTest.BottomLeft:
                        case HitTest.BottomRight:

                            m.Result = (IntPtr)HitTest.Bottom;

                            break;
                        case HitTest.TopLeft:
                        case HitTest.TopRight:

                            m.Result = (IntPtr)HitTest.Top;

                            break;
                        case HitTest.Left:
                        case HitTest.Right:

                            m.Result = (IntPtr)HitTest.Border;

                            break;
                    }

                    break;
            }
        }
    }
}
