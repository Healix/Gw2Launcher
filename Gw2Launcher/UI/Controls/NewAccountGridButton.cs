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
    public partial class NewAccountGridButton : AccountGridButton
    {
        protected static readonly Font FONT_REGULAR = new Font("Segoe UI", 8.25F, FontStyle.Regular, GraphicsUnit.Point, 0);

        private const string TEXT_ADD_ACCOUNT = "add account";
        private static readonly Color COLOR_TEXT = Color.FromArgb(49, 121, 242);

        public NewAccountGridButton()
        {
            InitializeComponent();

            Size size = TextRenderer.MeasureText(TEXT_ADD_ACCOUNT, FONT_REGULAR);
            rectName = new Rectangle(Point.Subtract(rectName.Location, new Size(size.Width / 2, size.Height / 2)), size);
        }

        protected override void OnSizeChanged()
        {
            rectName = new Rectangle(new Point(this.Size.Width / 2 - rectName.Width / 2, this.Size.Height / 2 - rectName.Height / 2), rectName.Size);
            this.Invalidate();
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            var g = e.Graphics;

            TextRenderer.DrawText(g, TEXT_ADD_ACCOUNT, FONT_REGULAR, rectName, COLOR_TEXT);
        }

        private void NewAccountGridButton_Load(object sender, EventArgs e)
        {

        }
    }
}
