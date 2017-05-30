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
    public partial class formCleanup : Form
    {
        public formCleanup(IEnumerable<FileInfo> files)
        {
            InitializeComponent();

            string path = null;
            try
            {
                path = Path.GetDirectoryName(Settings.GW2Path.Value);
            }
            catch (Exception e)
            {
                Util.Logging.Log(e);
            }

            StringBuilder sb = new StringBuilder(1024);
            foreach (var f in files)
            {
                if (f.FullName.StartsWith(path))
                    sb.AppendLine(f.FullName.Substring(path.Length + 1));
                else
                    sb.AppendLine(f.FullName);
            }

            textConfirm.Text = sb.ToString();

            textConfirm.Select(0, 0);
        }

        private void buttonCancel_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
        }

        private void buttonOK_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.OK;
        }
    }
}
