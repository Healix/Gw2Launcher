using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Gw2Launcher.Windows.Native;

namespace Gw2Launcher.UI
{
    public partial class formAutologinConfig : Base.StackFormBase
    {
        public formAutologinConfig(Settings.Point<ushort> empty, Settings.Point<ushort> play)
        {
            InitializeComponents();

            checkEmpty.Checked = !empty.IsEmpty;
            checkPlay.Checked = !play.IsEmpty;

            textEmpty.Text = empty.X + ", " + empty.Y;
            textPlay.Text = play.X + ", " + play.Y;
        }

        protected override void OnInitializeComponents()
        {
            base.OnInitializeComponents();

            InitializeComponent();
        }

        protected override void OnLayout(LayoutEventArgs levent)
        {
            var w = this.ClientSize.Width;
            var h = this.ClientSize.Height;

            stackPanel1.MaximumSize = new Size(w - stackPanel1.Margin.Horizontal, 0);

            base.OnLayout(levent);

            var _h = stackPanel1.Height + stackPanel1.Margin.Vertical;
            if (_h != h)
            {
                this.ClientSize = new Size(w, _h);
            }
        }

        public Settings.Point<ushort> PlayLocation
        {
            get;
            private set;
        }

        public Settings.Point<ushort> EmptyLocation
        {
            get;
            private set;
        }

        private void checkEmpty_CheckedChanged(object sender, EventArgs e)
        {
            textEmpty.Enabled = checkEmpty.Checked;
        }

        private void checkPlay_CheckedChanged(object sender, EventArgs e)
        {
            textPlay.Enabled = checkPlay.Checked;
        }

        private Settings.Point<ushort> ParsePoint(string text)
        {
            try
            {
                var i = text.IndexOf(',');
                if (i != -1)
                {
                    var x = int.Parse(text.Substring(0, i));
                    var y = int.Parse(text.Substring(i + 1));

                    return new Settings.Point<ushort>((ushort)x, (ushort)y);
                }
            }
            catch
            {
            }

            return new Settings.Point<ushort>();
        }

        private string GetPoint()
        {
            RECT r;
            var h = NativeMethods.WindowFromPoint(Cursor.Position);
            if (h != IntPtr.Zero && NativeMethods.GetWindowRect(h, out r))
            {
                var p = Cursor.Position;
                var x = p.X - r.left;
                var y = p.Y - r.top;


                return x + ", " + y;
            }

            return "";
        }

        private void buttonOK_Click(object sender, EventArgs e)
        {
            if (checkEmpty.Checked)
                this.EmptyLocation = ParsePoint(textEmpty.Text);

            if (checkPlay.Checked)
                this.PlayLocation = ParsePoint(textPlay.Text);

            this.DialogResult = System.Windows.Forms.DialogResult.OK;
        }

        private void labelSelectEmpty_MouseUp(object sender, MouseEventArgs e)
        {
            textEmpty.Text = GetPoint();
        }

        private void labelSelectPlay_MouseUp(object sender, MouseEventArgs e)
        {
            textPlay.Text = GetPoint();
        }
    }
}
