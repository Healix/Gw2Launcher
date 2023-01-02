using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Design;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Gw2Launcher.UI.Base
{
    [TypeDescriptionProvider(typeof(UiTypeDescriptionProvider))]
    class BaseControl : Control, UiColors.IColors
    {
        protected UiColors.Colors _BackColorName = UiColors.Colors.Custom;
        [DefaultValue(UiColors.Colors.Custom)]
        [UiPropertyColor()]
        [TypeConverter(typeof(UiColorTypeConverter))]
        [Editor(typeof(UiColorTypeEditor), typeof(UITypeEditor))]
        public UiColors.Colors BackColorName
        {
            get
            {
                return _BackColorName;
            }
            set
            {
                if (_BackColorName != value)
                {
                    _BackColorName = value;
                    RefreshColors();
                }
            }
        }

        protected UiColors.Colors _ForeColorName = UiColors.Colors.Custom;
        [DefaultValue(UiColors.Colors.Custom)]
        [UiPropertyColor()]
        [TypeConverter(typeof(UiColorTypeConverter))]
        [Editor(typeof(UiColorTypeEditor), typeof(UITypeEditor))]
        public UiColors.Colors ForeColorName
        {
            get
            {
                return _ForeColorName;
            }
            set
            {
                if (_ForeColorName != value)
                {
                    _ForeColorName = value;
                    RefreshColors();
                }
            }
        }

        public virtual void RefreshColors()
        {
            if (_ForeColorName != UiColors.Colors.Custom)
                this.ForeColor = UiColors.GetColor(_ForeColorName);
            if (_BackColorName != UiColors.Colors.Custom)
                this.BackColor = UiColors.GetColor(_BackColorName);
        }
    }
}
