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
    public partial class NoteMessage : UserControl
    {
        public event EventHandler EditClick;
        public event EventHandler DeleteClick;

        private DateTime expires;
        private bool entered;
        private Color colorBg;

        public NoteMessage()
        {
            InitializeComponent();

            labelEdit.Visible = false;
            labelSep.Visible = false;
            labelDelete.Visible = false;

            foreach (Control control in panelBottom.Controls)
            {
                control.MouseLeave += control_MouseLeave;
                control.MouseEnter += control_MouseEnter;
            }
        }

        public string Message
        {
            get
            {
                return labelMessage.Text;
            }
            set
            {
                if (value == null)
                {
                    labelMessage.ForeColor = SystemColors.GrayText;
                    labelMessage.Text = "(not available)";
                }
                else
                {
                    labelMessage.ForeColor = Color.Empty;
                    labelMessage.Text = value;
                }
            }
        }

        public DateTime Expires
        {
            get
            {
                return expires;
            }
            set
            {
                expires = value;

                if (expires == DateTime.MinValue || expires == DateTime.MaxValue)
                {
                    labelExpiresValue.Visible = false;
                }
                else
                {
                    labelExpiresValue.Visible = true;
                    labelExpiresValue.Text = expires.ToLocalTime().ToString("MM/dd/yyyy hh:mm:ss tt");
                }
            }
        }

        private void labelMessage_SizeChanged(object sender, EventArgs e)
        {
            panelBottom.Top = labelMessage.Bottom;
            this.Height = panelBottom.Bottom + this.Padding.Bottom;
        }

        protected override void OnControlAdded(ControlEventArgs e)
        {
            base.OnControlAdded(e);

            e.Control.MouseLeave += control_MouseLeave;
            e.Control.MouseEnter += control_MouseEnter;
        }

        void control_MouseLeave(object sender, EventArgs e)
        {
            if (!entered)
                return;

            var r = this.Bounds;
            Control p = null, c = this.Parent;

            do
            {
                if (p != null)
                    r.Offset(p.Location);
                r.Intersect(c.ClientRectangle);
                p = c;
                c = c.Parent;
            }
            while (c != null && !(c is Form));

            if (!p.RectangleToScreen(r).Contains(Cursor.Position))
            {
                entered = false;
                OnEnteredEnd();
            }
        }

        void OnEnteredBegin()
        {
            labelEdit.Visible = true;
            labelSep.Visible = true;
            labelDelete.Visible = true;

            colorBg = this.BackColor;
            this.BackColor = Util.Color.Darken(this.BackColor, 0.025f);
        }

        void OnEnteredEnd()
        {
            labelEdit.Visible = false;
            labelSep.Visible = false;
            labelDelete.Visible = false;

            this.BackColor = colorBg;
        }

        void control_MouseEnter(object sender, EventArgs e)
        {
            if (!entered)
            {
                entered = true;
                OnEnteredBegin();
            }
        }

        protected override void OnMouseEnter(EventArgs e)
        {
            base.OnMouseEnter(e);

            control_MouseEnter(this, e);
        }

        protected override void OnVisibleChanged(EventArgs e)
        {
            base.OnVisibleChanged(e);

            if (entered && !this.Visible)
            {
                entered = false;
                OnEnteredEnd();
            }
        }

        protected override void OnMouseLeave(EventArgs e)
        {
            base.OnMouseLeave(e);
            control_MouseLeave(this, e);
        }

        private void labelEdit_Click(object sender, EventArgs e)
        {
            if (EditClick != null)
                EditClick(this, e);
            control_MouseLeave(this, e);
        }

        private void labelDelete_Click(object sender, EventArgs e)
        {
            if (DeleteClick != null)
                DeleteClick(this, e);
            control_MouseLeave(this, e);
        }
    }
}
