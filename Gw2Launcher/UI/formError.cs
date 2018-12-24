using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Gw2Launcher.UI
{
    public partial class formError : Form
    {
        public formError()
        {
            InitializeComponent();
        }

        public static void Show(string message)
        {
            using (var f = new formError())
            {
                f.textError.Text = message;
                f.textError.Select(0, 0);
                f.textError.ScrollToCaret();
                f.ShowDialog();
            }
        }

        private void labelOpenLog_Click(object sender, EventArgs e)
        {
            Util.Explorer.OpenFolderAndSelect(Util.Logging.PATH);
        }
    }
}
