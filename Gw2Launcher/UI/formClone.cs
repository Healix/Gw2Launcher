using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;
using System.Drawing.Imaging;

namespace Gw2Launcher.UI
{
    public partial class formClone : Base.BaseForm
    {
        private class ImageControl : Control
        {
            public Image Image;

            private bool _Grayscale;
            public bool Grayscale
            {
                get
                {
                    return _Grayscale;
                }
                set
                {
                    if (_Grayscale != value)
                    {
                        _Grayscale = value;
                        this.Invalidate();
                    }
                }
            }

            protected override void OnPaint(PaintEventArgs e)
            {
                var g = e.Graphics;

                if (_Grayscale)
                {
                    using (var ia = new ImageAttributes())
                    {
                        ia.SetColorMatrix(new ColorMatrix(new float[][] 
                        {
                            new float[] {.3f, .3f, .3f, 0, 0},
                            new float[] {.6f, .6f, .6f, 0, 0},
                            new float[] {.1f, .1f, .1f, 0, 0},
                            new float[] {0, 0, 0, .6f, 0},
                            new float[] {0, 0, 0, 0, 1}
                        }));

                        g.DrawImage(Image, this.DisplayRectangle, 0, 0, Image.Width, Image.Height, GraphicsUnit.Pixel, ia);
                    }
                }
                else
                {
                    g.DrawImage(Image, this.DisplayRectangle, 0, 0, Image.Width, Image.Height, GraphicsUnit.Pixel);
                }

                base.OnPaint(e);
            }
        }

        private class Row
        {
            public CheckBox check;
            public ImageControl icon;
            public TextBox text;
            public Settings.IAccount account;
        }

        private List<Row> rows;
        private Image[] defaultImage;
        private IList<Settings.IAccount> accounts;

        public formClone(IList<Settings.IAccount> accounts)
        {
            this.accounts = accounts;
            this.rows = new List<Row>(accounts.Count);

            InitializeComponents();

        }

        protected override void OnInitializeComponents()
        {
            base.OnInitializeComponents();

            InitializeComponent();
        }

        protected override void OnHandleCreated(EventArgs e)
        {
            base.OnHandleCreated(e);

            this.SuspendLayout();

            var types = new bool[2];
            var height = this.Height;

            foreach (var a in accounts)
            {
                var row = AddRow();
                var name = a.Name;

                if (string.IsNullOrEmpty(name))
                {
                    name = "(" + a.UID + ")";
                }
                else if (!name.EndsWith("(clone)"))
                {
                    name += " (clone)";
                }

                switch (a.Type)
                {
                    case Settings.AccountType.GuildWars1:

                        types[(int)a.Type] = true;

                        break;
                    case Settings.AccountType.GuildWars2:

                        types[(int)a.Type] = true;
                        row.icon.Top--;

                        break;
                }

                row.icon.Image = GetDefaultImage(a.Type, pictureIcon.Size);
                row.text.Text = name;
                row.account = a;
            }

            if (!types[(int)Settings.AccountType.GuildWars1])
            {
                //var ch = 0;

                //height -= ch;
                //this.MinimumSize = new System.Drawing.Size(this.MinimumSize.Width, this.MinimumSize.Height - ch);
            }

            if (!types[(int)Settings.AccountType.GuildWars2])
            {
                var ch = 0;

                labelLocalDat.Visible = false;
                panelLocalDat.Visible = false;
                ch += labelLocalDat.Height + labelLocalDat.Margin.Vertical;

                labelGfxSettings.Visible = false;
                panelGfxSettings.Visible = false;
                ch += labelGfxSettings.Height + labelGfxSettings.Margin.Vertical;

                height -= ch;
                this.MinimumSize = new System.Drawing.Size(this.MinimumSize.Width, this.MinimumSize.Height - ch);
            }

            if (rows.Count > 1)
            {
                var y = rows[rows.Count - 1].text.Bottom + rows[0].text.Top;
                var ch = y - panelContainer.Height;

                panelContainer.Height = y;

                var h = height + ch;
                var mh = Screen.FromControl(this).WorkingArea.Height * 3 / 4;

                if (h > mh)
                    h = mh;

                height = h;
            }

            this.Height = height;

            this.ResumeLayout();
        }

        private Image GetDefaultImage(Settings.AccountType type, Size sz)
        {
            var i = (int)type;

            if (defaultImage == null)
                defaultImage = new Image[2];

            if (defaultImage[i] == null)
            {
                using (var icon = new Icon(type == Settings.AccountType.GuildWars1 ? Properties.Resources.Gw1 : Properties.Resources.Gw2, 48, 48))
                {
                    var image = defaultImage[i] = new Bitmap(sz.Width, sz.Height, System.Drawing.Imaging.PixelFormat.Format32bppArgb);

                    using (var g = Graphics.FromImage(image))
                    {
                        g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
                        g.DrawIcon(icon, new Rectangle(0, 0, image.Width, image.Height));
                    }

                    return image;
                }
            }

            return defaultImage[i];
        }

        private Row AddRow()
        {
            var row = new Row();
            var rowCount = rows.Count;
            var y = textTemplate.Top + (textTemplate.Bottom) * rowCount;

            row.check = new CheckBox()
            {
                Location = new Point(checkTemplate.Left, y + (checkTemplate.Top - textTemplate.Top)),
                Size = checkTemplate.Size,
                Checked = true,
                Tag = row,
            };

            row.check.CheckedChanged += check_CheckedChanged;

            row.icon = new ImageControl()
            {
                Location = new Point(pictureIcon.Left, y + (pictureIcon.Top - textTemplate.Top)),
                Size = pictureIcon.Size,
                Anchor = pictureIcon.Anchor,
                Tag = row,
            };

            row.icon.Click += icon_Click;

            row.text = new TextBox()
            {
                Location = new Point(textTemplate.Left, y),
                Size = textTemplate.Size,
                Anchor = textTemplate.Anchor,
            };

            rows.Add(row);

            panelContainer.Controls.AddRange(new Control[] { row.check, row.icon, row.text });

            return row;
        }

        void icon_Click(object sender, EventArgs e)
        {
            var c = (Control)sender;
            var row = (Row)c.Tag;

            row.check.Checked = !row.check.Checked;
        }

        void check_CheckedChanged(object sender, EventArgs e)
        {
            var c = ((CheckBox)sender);
            var row = (Row)c.Tag;

            row.text.Enabled = c.Checked;
            row.icon.Grayscale = !c.Checked;
        }

        private void buttonCancel_Click(object sender, EventArgs e)
        {
            this.DialogResult = System.Windows.Forms.DialogResult.Cancel;
        }

        private List<Settings.IAccount> DoClone()
        {
            var accounts = new List<Settings.IAccount>(rows.Count);

            foreach (var row in rows)
            {
                if (row.check.Checked)
                {
                    var a = row.account;
                    var b = Settings.Clone(a);

                    b.Name = row.text.Text;

                    if (!checkStatistics.Checked)
                    {
                        b.CreatedUtc = DateTime.UtcNow;
                        b.LastUsedUtc = DateTime.MinValue;
                        b.TotalUses = 0;
                    }

                    if (b.Type == Settings.AccountType.GuildWars2)
                    {
                        if (radioDatCopy.Checked)
                        {
                            var gw2 = (Settings.IGw2Account)b;
                            if (gw2.DatFile != null)
                            {
                                var path = gw2.DatFile.Path;

                                if (File.Exists(path))
                                {
                                    try
                                    {
                                        var tmp = Path.GetTempFileName();
                                        File.Copy(gw2.DatFile.Path, tmp, true);

                                        gw2.DatFile = Settings.CreateDatFile();
                                        gw2.DatFile.Path = tmp;
                                    }
                                    catch (Exception ex)
                                    {
                                        Util.Logging.Log(ex);
                                    }
                                }
                                else
                                {
                                    gw2.DatFile = Settings.CreateDatFile();
                                }
                            }
                        }

                        if (radioGfxCopy.Checked)
                        {
                            var gw2 = (Settings.IGw2Account)b;
                            if (gw2.GfxFile != null)
                            {
                                var path = gw2.GfxFile.Path;

                                if (File.Exists(path))
                                {
                                    try
                                    {
                                        var tmp = Path.GetTempFileName();
                                        File.Copy(gw2.GfxFile.Path, tmp, true);

                                        gw2.GfxFile = Settings.CreateGfxFile();
                                        gw2.GfxFile.Path = tmp;
                                    }
                                    catch (Exception ex)
                                    {
                                        Util.Logging.Log(ex);
                                    }
                                }
                                else
                                {
                                    gw2.GfxFile = Settings.CreateGfxFile();
                                }
                            }
                        }
                    }
                    else if (b.Type == Settings.AccountType.GuildWars1)
                    {

                    }

                    accounts.Add(b);
                }
            }

            return accounts;
        }

        public IList<Settings.IAccount> Accounts
        {
            get;
            private set;
        }

        private Task<Settings.IFile> CopyAsync(Settings.IFile file)
        {
            return Task.Run(
                delegate
                {
                    return Copy(file);
                });
        }

        private Settings.IFile Create(Settings.IFile file)
        {
            if (file is Settings.IDatFile)
            {
                return Settings.CreateDatFile();
            }
            else if (file is Settings.IGfxFile)
            {
                return Settings.CreateGfxFile();
            }
            else
            {
                throw new NotSupportedException();
            }
        }

        private Settings.IFile Copy(Settings.IFile file)
        {
            if (File.Exists(file.Path))
            {
                var tmp = Path.GetTempFileName();
                File.Copy(file.Path, tmp, true);

                var _file = Create(file);
                
                _file.Path = tmp;

                return _file;
            }
            else
            {
                return Create(file);
            }
        }

        private async void buttonOK_Click(object sender, EventArgs e)
        {
            this.Enabled = false;

            Controls.FlatProgressBar p = null;
            Base.FlatBase f = null;

            if (radioDatCopy.Visible && radioDatCopy.Checked)
            {
                int max = 0;

                foreach (var row in rows)
                {
                    if (row.check.Checked && row.account.Type == Settings.AccountType.GuildWars2)
                    {
                        max++;
                    }
                }

                if (max > 0)
                {
                    f = new Base.FlatBase()
                    {
                        Text = "",
                        StartPosition = FormStartPosition.Manual,
                    };

                    p = new Controls.FlatProgressBar()
                    {
                        Anchor = AnchorStyles.Left | AnchorStyles.Top | AnchorStyles.Right | AnchorStyles.Bottom,
                        Animated = false,
                        BackColor = Color.FromArgb(240, 240, 240),
                        ForeColor = Color.LightSteelBlue,
                        Maximum = max,
                        Margin = new Padding(5, 5, 5, 5),
                    };

                    f.Controls.Add(p);

                    f.Size = new System.Drawing.Size(300, 50);
                    f.InitializeComponents(); //note scaling occurs here

                    f.Location = new Point(this.Left + this.Width / 2 - f.Width / 2, this.Top + this.Height / 2 - f.Height / 2);

                    f.Show(this);
                }
            }

            using (f)
            {
                var accounts = new List<Settings.IAccount>(rows.Count);

                foreach (var row in rows)
                {
                    if (row.check.Checked)
                    {
                        var a = row.account;
                        var b = Settings.Clone(a);

                        b.Name = row.text.Text;

                        if (!checkStatistics.Checked)
                        {
                            b.CreatedUtc = DateTime.UtcNow;
                            b.LastUsedUtc = DateTime.MinValue;
                            b.TotalUses = 0;
                        }

                        if (b.Type == Settings.AccountType.GuildWars2)
                        {
                            if (radioDatCopy.Checked)
                            {
                                var gw2 = (Settings.IGw2Account)b;

                                if (gw2.DatFile != null)
                                {
                                    try
                                    {
                                        gw2.DatFile = (Settings.IDatFile)await CopyAsync(gw2.DatFile);
                                    }
                                    catch (Exception ex)
                                    {
                                        Util.Logging.Log(ex);
                                    }
                                }
                            }

                            if (radioGfxCopy.Checked)
                            {
                                var gw2 = (Settings.IGw2Account)b;

                                if (gw2.GfxFile != null)
                                {
                                    try
                                    {
                                        gw2.GfxFile = (Settings.IGfxFile)await CopyAsync(gw2.GfxFile);
                                    }
                                    catch (Exception ex)
                                    {
                                        Util.Logging.Log(ex);
                                    }
                                }
                            }

                            if (p != null)
                                p.Value++;
                        }
                        else if (b.Type == Settings.AccountType.GuildWars1)
                        {

                        }

                        accounts.Add(b);
                    }
                }

                this.Accounts = accounts;
            }

            //this.Accounts = await Task.Run<List<Settings.IAccount>>(new Func<List<Settings.IAccount>>(DoClone));

            this.DialogResult = System.Windows.Forms.DialogResult.OK;
        }

        private void label5_Click(object sender, EventArgs e)
        {
            checkStatistics.Checked = !checkStatistics.Checked;
        }
    }
}
