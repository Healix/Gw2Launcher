using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Gw2Launcher.UI.ColorPicker
{
    public partial class formColorDialog : Base.BaseForm
    {
        /// <summary>
        /// Creates a color picker (alpha transparency allowed by default)
        /// </summary>
        public formColorDialog()
        {
            InitializeComponents();

            _AllowAlphaTransparency = true;

            textA.MouseWheel += textARGB_MouseWheel;
            textR.MouseWheel += textARGB_MouseWheel;
            textG.MouseWheel += textARGB_MouseWheel;
            textB.MouseWheel += textARGB_MouseWheel;

            panelPreview.Color1Changed += panelPreview_Color1Changed;
        }

        void panelPreview_Color1Changed(object sender, EventArgs e)
        {
            OnColorChanged();
        }

        protected override void OnInitializeComponents()
        {
            base.OnInitializeComponents();

            InitializeComponent();
        }

        protected override void OnScale(float scale)
        {
            base.OnScale(scale);

            var cs = Scale(14);
            panelAlpha.CursorSize = cs;
            panelHue.CursorSize = cs;
            panelShade.CursorSize = cs;
        }

        private void textHex_TextChanged(object sender, EventArgs e)
        {
            if (textHex.ContainsFocus)
            {
                if (textHex.TextLength > 0)
                {
                    try
                    {
                        var hex = textHex.Text;
                        if (hex[0] != '#')
                            hex = "#" + hex;
                        var c = ColorTranslator.FromHtml(hex);
                        panelAlpha.Alpha = _AllowAlphaTransparency ? c.A / 255f : 0.5f;
                        panelShade.SelectedColor = c;
                        panelHue.Hue = (int)(c.GetHue() / 360f * 240f);
                    }
                    catch { }
                }
            }
        }

        private void textHex_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (char.IsDigit(e.KeyChar))
            {
                e.Handled = false;
            }
            else if (char.IsLetter(e.KeyChar))
            {
                if (char.IsUpper(e.KeyChar))
                {
                    e.Handled = e.KeyChar > 'F';
                }
                else
                {
                    e.Handled = e.KeyChar > 'f';
                }
            }
            else if (!char.IsControl(e.KeyChar))
            {
                e.Handled = true;
            }
        }

        private void textRGB_ValueChanged(object sender, EventArgs e)
        {
            if (((Control)sender).ContainsFocus)
            {
                var c = Color.FromArgb((int)textR.Value, (int)textG.Value, (int)textB.Value);

                if (!_AllowAlphaTransparency)
                    panelAlpha.Alpha = 0.5f;
                panelShade.SelectedColor = c;
                panelHue.Hue = (int)(c.GetHue() / 360f * 240f);
            }
        }

        private void textA_ValueChanged(object sender, EventArgs e)
        {
            var c = (Gw2Launcher.UI.Controls.IntegerTextBox)sender;
            if (c.ContainsFocus)
                panelAlpha.Alpha = (float)c.Value / 255f;
        }

        void textARGB_MouseWheel(object sender, MouseEventArgs e)
        {
            var c = (Control)sender;
            if (!c.ContainsFocus)
                c.Focus();
        }

        private void OnColorChanged()
        {
            if (!textA.Focused && !textR.Focused && !textG.Focused && !textB.Focused)
            {
                textA.Value = panelPreview.Color1.A;
                textR.Value = panelPreview.Color1.R;
                textG.Value = panelPreview.Color1.G;
                textB.Value = panelPreview.Color1.B;
            }

            if (!textHex.Focused)
            {
                textHex.Text = panelPreview.Color1.ToArgb().ToString("x");
            }
        }

        private void panelHue_HueChanged(object sender, int e)
        {
            panelShade.Hue = e;
        }

        private void panelShade_ColorChanged(object sender, Color e)
        {
            if (_AllowAlphaTransparency)
            {
                var a = (int)(255 * panelAlpha.Alpha + 0.5f);

                panelPreview.Color1 = a != 255 ? Color.FromArgb(a, e) : e;
                panelAlpha.BackColor = Color.FromArgb(255, e);
            }
            else
            {
                var a = panelAlpha.Alpha - 0.5f;
                float c;

                if (a > 0)
                {
                    a = a * 2;
                    c = 255 * a;
                    a = 1 - a;
                }
                else
                {
                    c = 0;
                    a = 1 + a * 2;
                }

                panelPreview.Color1 = Color.FromArgb((int)(e.R * a + c), (int)(e.G * a + c), (int)(e.B * a + c));
                panelAlpha.BackColor = Color.FromArgb(255, e);
            }

            //OnColorChanged();
        }

        private void panelAlpha_AlphaChanged(object sender, float e)
        {
            if (_AllowAlphaTransparency)
            {
                panelPreview.Color1 = Color.FromArgb((int)(255 * e + 0.5f), panelShade.SelectedColor);
            }
            else
            {
                var a = e - 0.5f;
                float c;
                
                if (a > 0)
                {
                    a = a * 2;
                    c = 255 * a;
                    a = 1 - a;
                }
                else
                {
                    c = 0;
                    a = 1 + a * 2;
                }

                panelPreview.Color1 = Color.FromArgb((int)(panelAlpha.BackColor.R * a + c), (int)(panelAlpha.BackColor.G * a + c), (int)(panelAlpha.BackColor.B * a + c));
            }

            //OnColorChanged();
        }

        private void panelOriginal_Click(object sender, EventArgs e)
        {
            SetColor(((ColorPicker.Controls.ColorPreviewPanel)sender).Color1);
        }

        private void SetColor(Color c)
        {
            panelAlpha.Alpha = _AllowAlphaTransparency ? c.A / 255f : 0.5f;
            panelAlpha.BackColor = panelShade.SelectedColor = c.A != 255 ? Color.FromArgb(255, c) : c;
            panelHue.Hue = (int)(c.GetHue() / 360f * 240f);

            panelPreview.Color1 = c; //to ensure exact color is used
        }

        private bool _AllowAlphaTransparency;
        public bool AllowAlphaTransparency
        {
            get
            {
                return _AllowAlphaTransparency;
            }
            set
            {
                if (_AllowAlphaTransparency != value)
                {
                    _AllowAlphaTransparency = value;

                    textA.Enabled = value;
                    panelAlpha.AllowAlphaTransparency = value;
                }
            }
        }

        private Color _Color;
        /// <summary>
        /// Currently selected color (automatically sets the original color if it wasn't already set)
        /// </summary>
        public Color Color
        {
            get
            {
                return _Color;
            }
            set
            {
                _Color = value;
                SetColor(value);
                if (panelOriginal.Color1.IsEmpty)
                    panelOriginal.Color1 = value;
            }
        }

        /// <summary>
        /// Displayed as the original (previously set) color
        /// </summary>
        public Color OriginalColor
        {
            get
            {
                return panelOriginal.Color1;
            }
            set
            {
                panelOriginal.Color1 = value;
            }
        }

        /// <summary>
        /// Displayed as the default color
        /// </summary>
        public Color DefaultColor
        {
            get
            {
                return panelDefaultColor.Color1;
            }
            set
            {
                panelDefaultColor.Color1 = value;
                panelColors.Visible = true;
            }
        }

        private void buttonCancel_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
        }

        private void buttonOK_Click(object sender, EventArgs e)
        {
            _Color = panelPreview.Color1;
            if (!_AllowAlphaTransparency && _Color.A != 255)
                _Color = Color.FromArgb(255, _Color);
            this.DialogResult = DialogResult.OK;
        }
    }
}
