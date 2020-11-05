using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Gw2Launcher.Util
{
    static class CheckedButton
    {
        /// <summary>
        /// Groups radio buttons to allow for only 1 to be checked
        /// </summary>
        public static void Group(params System.Windows.Forms.RadioButton[] buttons)
        {
            EventHandler onChanged = delegate(object sender, EventArgs e)
            {
                var o = (System.Windows.Forms.RadioButton)sender;
                if (o.Checked)
                {
                    foreach (var b in buttons)
                    {
                        if (b == o)
                            continue;
                        b.Checked = false;
                    }
                }
            };

            foreach (var b in buttons)
            {
                b.CheckedChanged += onChanged;
            }
        }

        /// <summary>
        /// Groups checkbox buttons to allow for only 1 to be checked
        /// </summary>
        public static void Group(params System.Windows.Forms.CheckBox[] buttons)
        {
            EventHandler onChanged = delegate(object sender, EventArgs e)
            {
                var o = (System.Windows.Forms.CheckBox)sender;
                if (o.Checked)
                {
                    foreach (var b in buttons)
                    {
                        if (b == o)
                            continue;
                        b.Checked = false;
                    }
                }
            };

            foreach (var b in buttons)
            {
                b.CheckedChanged += onChanged;
            }
        }
    }
}
