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
    public partial class formLog : Form
    {
        public formLog()
        {
            InitializeComponent();

            this.Shown += formLog_Shown;
        }

        void formLog_Shown(object sender, EventArgs e)
        {

        }

        public void AddLine(string message)
        {
            if (this.InvokeRequired)
            {
                try
                {
                    this.Invoke(new MethodInvoker(
                        delegate
                        {
                            AddLine(message);
                        }));
                }
                catch { };
                return;
            }

            textBox1.AppendText(message + "\r\n");
            textBox1.Select(textBox1.TextLength, 0);
            textBox1.ScrollToCaret();
        }
    }
}
