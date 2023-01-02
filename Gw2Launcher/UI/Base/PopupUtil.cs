using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Gw2Launcher.Windows.Native;

namespace Gw2Launcher.UI.Base
{
    class PopupUtil : NativeWindow, IDisposable
    {
        /// <summary>
        /// Occurs when the popup should be hidden
        /// </summary>
        public event EventHandler Deactivating;

        private Form owner;
        private Form popup;
        private bool enabled;
        private bool deactivated;

        public PopupUtil(Form owner, Form popup)
        {
            this.owner = owner;
            this.popup = popup;
        }

        public Form Owner
        {
            get
            {
                return owner;
            }
        }

        public Form Popup
        {
            get
            {
                return popup;
            }
        }

        public bool Enabled
        {
            get
            {
                return enabled;
            }
            set
            {
                if (enabled != value)
                {
                    deactivated = !value;

                    if (value)
                    {
                        if (owner.IsHandleCreated)
                        {
                            AssignHandle(owner.Handle);
                        }

                        owner.HandleCreated += owner_HandleCreated;
                        owner.HandleDestroyed += owner_HandleDestroyed;
                        popup.Deactivate += popup_Deactivate;
                        popup.VisibleChanged += popup_VisibleChanged;
                    }
                    else
                    {
                        ReleaseHandle();

                        owner.HandleCreated -= owner_HandleCreated;
                        owner.HandleDestroyed -= owner_HandleDestroyed;
                        popup.Deactivate -= popup_Deactivate;
                        popup.VisibleChanged -= popup_VisibleChanged;
                    }

                    enabled = value;
                }
            }
        }

        void owner_HandleDestroyed(object sender, EventArgs e)
        {
            ReleaseHandle();
        }

        void owner_HandleCreated(object sender, EventArgs e)
        {
            AssignHandle(owner.Handle);
        }

        void popup_VisibleChanged(object sender, EventArgs e)
        {
            if (!popup.Visible)
                this.Enabled = false;
        }

        void popup_Deactivate(object sender, EventArgs e)
        {
            try
            {
                owner.BeginInvoke(new Action(OnDeactivate));
            }
            catch
            {
                OnDeactivate();
            }
        }

        void OnDeactivate()
        {
            if (!popup.Visible)
                return;

            if (!owner.ContainsFocus)
            {
                var m = new Message()
                {
                    HWnd = owner.Handle,
                    Msg = (int)WindowMessages.WM_NCACTIVATE,
                };
                base.WndProc(ref m);
            }

            OnDeactivating();
        }

        private void OnDeactivating()
        {
            if (!deactivated)
            {
                deactivated = true;
                if (Deactivating != null)
                    Deactivating(this, EventArgs.Empty);
                else
                    popup.Hide();
            }
        }

        public void Dispose()
        {
            this.Enabled = false;
        }

        protected override void WndProc(ref Message m)
        {
            switch ((WindowMessages)m.Msg)
            {
                case WindowMessages.WM_NCACTIVATE:

                    if (m.WParam == IntPtr.Zero && popup.Visible)
                    {
                        m.WParam = (IntPtr)1;
                    }

                    break;
                case WindowMessages.WM_ACTIVATEAPP:

                    if (m.WParam == IntPtr.Zero && popup.Visible)
                    {
                        OnDeactivating();

                        base.WndProc(ref m);

                        var m2 = new Message()
                        {
                            HWnd = owner.Handle,
                            Msg = (int)WindowMessages.WM_NCACTIVATE,
                        };

                        base.WndProc(ref m2);

                        return;
                    }

                    break;
                case WindowMessages.WM_ACTIVATE:

                    if (m.WParam != IntPtr.Zero)
                    {
                        OnDeactivating();
                    }

                    break;
            }

            base.WndProc(ref m);
        }

        public void Show()
        {
            this.Enabled = true;
            if (!popup.Visible)
                popup.Show();
        }
    }
}
