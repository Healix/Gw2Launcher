using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gw2Launcher.Client
{
    class ProcessSettings : IDisposable
    {
        public event EventHandler WindowTemplateChanged, 
                                  WindowOptionsChanged,
                                  ProcessPriorityChanged,
                                  ProcessAffinityChanged;

        public ProcessSettings()
        {

        }

        private Tools.WindowManager.IWindowBounds _WindowTemplate;
        public Tools.WindowManager.IWindowBounds WindowTemplate
        {
            get
            {
                return _WindowTemplate;
            }
            set
            {
                if (_WindowTemplate != value)
                {
                    _WindowTemplate = value;
                    if (WindowOptionsChanged != null)
                        WindowOptionsChanged(this, EventArgs.Empty);
                }
            }
        }

        private Settings.WindowOptions _WindowOptions;
        public Settings.WindowOptions WindowOptions
        {
            get
            {
                return _WindowOptions;
            }
            set
            {
                if (_WindowOptions != value)
                {
                    _WindowOptions = value;
                    if (WindowOptionsChanged != null)
                        WindowOptionsChanged(this, EventArgs.Empty);
                }
            }
        }

        private Settings.ProcessPriorityClass _ProcessPriority;
        public Settings.ProcessPriorityClass ProcessPriority
        {
            get
            {
                return _ProcessPriority;
            }
            set
            {
                if (_ProcessPriority != value)
                {
                    _ProcessPriority = value;
                    if (ProcessPriorityChanged != null)
                        ProcessPriorityChanged(this, EventArgs.Empty);
                }
            }
        }

        private long _ProcessAffinity;
        public long ProcessAffinity
        {
            get
            {
                return _ProcessAffinity;
            }
            set
            {
                if (_ProcessAffinity != value)
                {
                    _ProcessAffinity = value;
                    if (ProcessAffinityChanged != null)
                        ProcessAffinityChanged(this, EventArgs.Empty);
                }
            }
        }

        public void Dispose()
        {

        }
    }
}
