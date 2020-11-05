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
    public partial class formShortcutType : Base.FlatBase
    {
        public formShortcutType()
        {
            InitializeComponents();

            Util.CheckedButton.Group(radioMultiple, radioSingle);
        }

        protected override void OnInitializeComponents()
        {
            base.OnInitializeComponents();

            InitializeComponent();
        }

        public bool CreateSingle
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
            CreateSingle = radioSingle.Checked;

            this.DialogResult = System.Windows.Forms.DialogResult.OK;
        }
    }
}
