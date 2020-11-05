using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gw2Launcher.UI.Base
{

    public class BaseForm : System.Windows.Forms.Form
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

            _scale = 1f;
            _dpi = DPI;

            if (DesignMode)
            {
                InitializeComponents();
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
    }
}
