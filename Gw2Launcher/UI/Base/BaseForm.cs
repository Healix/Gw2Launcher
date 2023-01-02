using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Design;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gw2Launcher.UI.Base
{

    [TypeDescriptionProvider(typeof(UiTypeDescriptionProvider))]
    public class BaseForm : System.Windows.Forms.Form, UiColors.IColors
    {
        private const float DPI = 96f;

        private float _dpi;
        private float _scale;
        private bool _initialized;

        public BaseForm()
        {
            SuspendLayout();

            AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
            Font = new System.Drawing.Font("Segoe UI", 8.25F);
            Icon = Properties.Resources.Gw2Launcher;

            UiColors.ColorsChanged += OnColorsChanged;

            _scale = 1f;
            _dpi = DPI;

            if (DesignMode)
            {
                InitializeComponents();
            }
        }

        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public new System.Windows.Forms.AutoScaleMode AutoScaleMode
        {
            get
            {
                return base.AutoScaleMode;
            }
            set
            {
                base.AutoScaleMode = value;
            }
        }

        [DefaultValue(typeof(Font), "Segoe UI, 8.25pt")]
        public override Font Font
        {
            get
            {
                return base.Font;
            }
            set
            {
                base.Font = value;
            }
        }

        /// <summary>
        /// Scaling for 96dpi
        /// </summary>
        protected float GetScaling()
        {
            return _scale;
        }

        /// <summary>
        /// Scaling for the previously applied DPI
        /// </summary>
        protected float GetCurrentScaling()
        {
            return this.CurrentAutoScaleDimensions.Width / _dpi;
        }

        /// <summary>
        /// Scales for 96dpi, rounded
        /// </summary>
        protected int Scale(int n)
        {
            return (int)(_scale * n + 0.5f);
        }

        /// <summary>
        /// Scales for 96dpi, rounded
        /// </summary>
        protected System.Windows.Forms.Padding Scale(int l, int t, int r, int b)
        {
            if (_scale != 1f)
            {
                l = (int)(_scale * l + 0.5f);
                t = (int)(_scale * t + 0.5f);
                r = (int)(_scale * r + 0.5f);
                b = (int)(_scale * b + 0.5f);
            }
            return new System.Windows.Forms.Padding(l, t, r, b);
        }

        protected Size Scale(int w, int h)
        {
            if (_scale != 1f)
            {
                w = (int)(_scale * w + 0.5f);
                h = (int)(_scale * h + 0.5f);
            }
            return new Size(w, h);
        }

        /// <summary>
        /// Scales the control for 96dpi
        /// </summary>
        protected void Scale(System.Windows.Forms.Control c)
        {
            if (_dpi != DPI)
                c.Scale(new System.Drawing.SizeF(_scale, _scale));
        }

        protected override void ScaleControl(System.Drawing.SizeF factor, System.Windows.Forms.BoundsSpecified specified)
        {
            base.ScaleControl(factor, specified);

            OnScale(factor.Width);
        }

        public void InitializeComponents()
        {
            if (_initialized)
                return;
            _initialized = true;

            OnInitializeComponents();


            if (AutoScalingEnabled && AutoScaleMode == System.Windows.Forms.AutoScaleMode.None)
            {
                UpdateScaling();
            }

            ResumeLayout();
        }

        public bool IsComponentsInitialized
        {
            get
            {
                return _initialized;
            }
        }

        protected virtual bool AutoScalingEnabled
        {
            get
            {
                return true;
            }
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            if (!_initialized)
            {

                InitializeComponents();
            }
        }

        protected void UpdateScaling()
        {
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;

            var current = CurrentAutoScaleDimensions;

            if (current.Width != _dpi)
            {
                var scale = GetCurrentScaling();

                _scale = current.Width / _dpi;
                _dpi = current.Width;

                Scale(new System.Drawing.SizeF(scale, scale));
                OnDpiChanged();
            }

            AutoScaleDimensions = current;
        }

        protected virtual void OnInitializeComponents()
        {

        }

        protected virtual void OnDpiChanged()
        {

        }

        protected virtual void OnScale(float scale)
        {

        }

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

        protected virtual void OnColorsChanged(object sender, EventArgs e)
        {
            UiColors.Update(this, true, true);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                UiColors.ColorsChanged -= OnColorsChanged;
            }

            base.Dispose(disposing);
        }
    }
}
