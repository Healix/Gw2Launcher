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

namespace Gw2Launcher.UI
{
    public partial class formBrowseLocalDatQuick : Form
    {
        public formBrowseLocalDatQuick(bool showDat, bool showGfx)
        {
            InitializeComponent();

            int _space = buttonOK.Location.X - buttonCancel.Bounds.Right;
            buttonCancel.Location = new Point(this.ClientSize.Width / 2 - (buttonOK.Bounds.Right - buttonCancel.Bounds.Left) / 2, buttonCancel.Location.Y);
            buttonOK.Location = new Point(buttonCancel.Bounds.Right + _space, buttonCancel.Location.Y);

            var h = 0;

            if (!showDat)
            {
                panelDat.Visible = false;
                h += panelDat.Height;

                panelGfx.Top -= panelDat.Height;
            }

            if (!showGfx)
            {
                panelGfx.Visible = false;
                h += panelGfx.Height;
            }

            if (h > 0)
                this.Height -= h;
        }

        public Settings.IDatFile DatFile
        {
            get;
            private set;
        }

        public Settings.IGfxFile GfxFile
        {
            get;
            private set;
        }

        private void buttonCancel_Click(object sender, EventArgs e)
        {
            this.DialogResult = System.Windows.Forms.DialogResult.Cancel;
        }

        private void buttonOK_Click(object sender, EventArgs e)
        {
            string dat = null,
                   gfx = null;

            if (panelDat.Visible)
            {
                if (radioDatNew.Checked)
                {
                    dat = Path.GetTempFileName();
                }
                else
                {
                    var defaultFile = Client.FileManager.GetDefaultPath(Client.FileManager.FileType.Dat);

                    if (radioDatCopyDefault.Checked)
                    {
                        var tmp = Path.GetTempFileName();

                        try
                        {
                            File.Copy(defaultFile, tmp, true);
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show(this, "Unable to copy the default Local.dat file:\n\n" + ex.Message, "Unable to copy Local.dat", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            return;
                        }

                        dat = tmp;
                    }
                    else if (radioDatShareDefault.Checked)
                    {
                        var f = Client.FileManager.FindFile(Client.FileManager.FileType.Dat, defaultFile);
                        if (f != null)
                            this.DatFile = (Settings.IDatFile)f;
                        else if (File.Exists(defaultFile))
                            dat = defaultFile;
                        else
                        {
                            MessageBox.Show(this, "Unable to share the default Local.dat file, it does not exist", "Local.dat not found", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            return;
                        }
                    }
                }
            }

            if (panelGfx.Visible)
            {
                if (radioGfxNew.Checked)
                {
                    gfx = Path.GetTempFileName();
                }
                else
                {
                    var defaultFile = Client.FileManager.GetDefaultPath(Client.FileManager.FileType.Gfx);

                    if (radioGfxCopyDefault.Checked)
                    {
                        var tmp = Path.GetTempFileName();

                        try
                        {
                            File.Copy(defaultFile, tmp, true);
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show(this, "Unable to copy the default GFXSettings.xml file:\n\n" + ex.Message, "Unable to copy GFXSettings.xml", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            return;
                        }

                        gfx = tmp;
                    }
                    else if (radioGfxShareDefault.Checked)
                    {
                        var f = Client.FileManager.FindFile(Client.FileManager.FileType.Gfx, defaultFile);
                        if (f != null)
                            this.GfxFile = (Settings.IGfxFile)f;
                        else if (File.Exists(defaultFile))
                            gfx = defaultFile;
                        else
                        {
                            MessageBox.Show(this, "Unable to share the default GFXSettings.xml file, it does not exist", "GFXSettings.xml not found", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            return;
                        }
                    }
                }
            }

            if (dat != null)
            {
                this.DatFile = Settings.CreateDatFile();
                this.DatFile.Path = dat;
            }

            if (gfx != null)
            {
                this.GfxFile = Settings.CreateGfxFile();
                this.GfxFile.Path = gfx;
            }

            this.DialogResult = System.Windows.Forms.DialogResult.OK;
        }
    }
}
