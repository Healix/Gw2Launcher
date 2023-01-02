using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Drawing;
using System.Diagnostics;
using Gw2Launcher.Windows.Native;
using Gw2Launcher.Client;

namespace Gw2Launcher.UI
{
    class formMaskOverlay : Form
    {
        public class Manager : IDisposable
        {
            private class MaskProcess
            {
                public MaskProcess(Process p, IntPtr h, EnableFlags flags)
                {
                    this.Process = p;
                    this.Handle = h;
                    this.Flags = flags;
                }

                public Process Process
                {
                    get;
                    private set;
                }

                public IntPtr Handle
                {
                    get;
                    private set;
                }

                public EnableFlags Flags
                {
                    get;
                    private set;
                }
            }

            private Dictionary<Settings.IAccount, MaskProcess> accounts;
            
            public Manager()
            {
                accounts = new Dictionary<Settings.IAccount, MaskProcess>();
            }

            void Launcher_AccountProcessExited(Settings.IAccount account, Process e)
            {
                Remove(account, null);
            }

            void Launcher_AccountWindowEvent(Settings.IAccount account, Launcher.AccountWindowEventEventArgs e)
            {
                if (e.Type == Launcher.AccountWindowEventEventArgs.EventType.BoundsChanged)
                {
                    lock (accounts)
                    {
                        MaskProcess p;
                        if (accounts.TryGetValue(account, out p))
                        {
                            var h = Windows.FindWindow.Find(p.Process);
                            if (h != IntPtr.Zero)
                            {
                                NativeMethods.SetWindowPos(h, IntPtr.Zero, 0, 0, 0, 0, SetWindowPosFlags.SWP_NOMOVE | SetWindowPosFlags.SWP_NOSIZE | SetWindowPosFlags.SWP_NOACTIVATE | SetWindowPosFlags.SWP_ASYNCWINDOWPOS | SetWindowPosFlags.SWP_NOZORDER);
                            }
                        }
                    }
                }
            }

            public void Add(Settings.IAccount account, Process p, IntPtr window, Settings.WindowOptions options)
            {
                MaskProcess existing;
                Process current;

                lock (accounts)
                {
                    if (accounts.Count == 0)
                    {
                        existing = null;

                        Client.Launcher.AccountWindowEvent += Launcher_AccountWindowEvent;
                        Client.Launcher.AccountProcessExited += Launcher_AccountProcessExited;
                    }
                    else
                    {
                        accounts.TryGetValue(account, out existing);
                    }

                    EnableFlags flags;

                    if ((options & Settings.WindowOptions.DisableTitleBarButtons) == 0)
                    {
                        flags = EnableFlags.EnableClose | EnableFlags.EnableMinimize;
                    }
                    else
                    {
                        flags = EnableFlags.None;
                    }

                    if (existing != null && existing.Flags == flags && existing.Handle == window)
                    {
                        return;
                    }

                    try
                    {
                        accounts[account] = new MaskProcess(current = Util.ProcessUtil.ShowWindowMask(p, window, flags), window, flags);
                    }
                    catch
                    {
                        Remove(account);
                        return;
                    }

                    try
                    {
                        current.Exited += process_Exited;
                        current.EnableRaisingEvents = true;
                        if (current.HasExited)
                            throw new Exception("Exited");
                    }
                    catch
                    {
                        Remove(account, current);
                    }
                }

                if (existing != null)
                {
                    try
                    {
                        existing.Process.EnableRaisingEvents = false;
                        existing.Process.Kill();
                    }
                    catch { }
                }
            }

            void process_Exited(object sender, EventArgs e)
            {
                lock (accounts)
                {
                    var p = (Process)sender;

                    foreach (var account in accounts.Keys)
                    {
                        if (accounts[account].Process == p)
                        {
                            Remove(account, p);

                            break;
                        }
                    }
                }
            }

            private void Remove(Settings.IAccount account, Process p)
            {
                lock (accounts)
                {
                    MaskProcess existing;

                    if (accounts.TryGetValue(account, out existing))
                    {
                        if (p == null || existing.Process == p)
                        {
                            p = existing.Process;

                            if (accounts.Remove(account) && accounts.Count == 0)
                            {
                                Client.Launcher.AccountWindowEvent -= Launcher_AccountWindowEvent;
                                Client.Launcher.AccountProcessExited -= Launcher_AccountProcessExited;
                            }
                        }
                    }
                }

                if (p != null)
                {
                    try
                    {
                        p.EnableRaisingEvents = false;
                        p.Kill();
                    }
                    catch { }
                }
            }

            public void Remove(Settings.IAccount account)
            {
                Remove(account, null);
            }

            public void Dispose()
            {
                lock (accounts)
                {
                    if (accounts.Count > 0)
                    {
                        Client.Launcher.AccountWindowEvent -= Launcher_AccountWindowEvent;
                        Client.Launcher.AccountProcessExited -= Launcher_AccountProcessExited;

                        foreach (var m in accounts.Values)
                        {
                            try
                            {
                                var p = m.Process;
                                p.EnableRaisingEvents = false;
                                p.Kill();
                            }
                            catch { }
                        }

                        accounts.Clear();
                    }
                }
            }
        }


        [Flags]
        public enum EnableFlags : byte
        {
            None = 0,
            EnableClose = 1,
            EnableMinimize = 2,
        }

        private Process process;
        private IntPtr attachedTo;
        private RECT rect;
        private EnableFlags flags;
        private bool resizing;
        private int delayed;

        public formMaskOverlay(Process p, IntPtr window, EnableFlags flags)
        {
            SetStyle(ControlStyles.Opaque, true);

            this.process = p;
            this.flags = flags;

            this.BackColor = Color.Black;
            this.ShowInTaskbar = false;
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.StartPosition = FormStartPosition.Manual;

            this.attachedTo = window;
        }

        protected override void OnMouseDown(MouseEventArgs e)
        {
            base.OnMouseDown(e);

            if (attachedTo != IntPtr.Zero)
            {
                NativeMethods.ReleaseCapture();
                NativeMethods.SetForegroundWindow(attachedTo);
            }
        }

        private async void DelayedResize()
        {
            if (delayed == 0)
            {
                delayed = Environment.TickCount;
            }
            else
            {
                delayed = Environment.TickCount;
                return;
            }

            do
            {
                await Task.Delay(1000);
            }
            while (Environment.TickCount - delayed < 1000);

            delayed = 0;

            ResizeToParent(true);
        }
        
        private void ResizeToParent(bool delayed = false)
        {
            if (resizing)
                return;
            resizing = true;

            try
            {
                DoResizeToParent(delayed);
            }
            finally
            {
                resizing = false;
            }
        }

        private void DoResizeToParent(bool delayed)
        {
            var p = new WINDOWPLACEMENT();

            if (NativeMethods.GetWindowPlacement(attachedTo, ref p))
            {
                if (p.showCmd == ShowWindowCommands.ShowMinimized)
                    return;

                if (p.rcNormalPosition.Equals(rect))
                {
                    if (!delayed)
                    {
                        //some changes will only complete after
                        //this isn't really needed, since it only occurs when changing between fullscreen and windowed

                        DelayedResize();
                    }
                    return;
                }

                rect = p.rcNormalPosition;
                var b = rect.ToRectangle();

                if (Util.ScreenUtil.IsFullScreen(b))
                {
                    p.rcNormalPosition = new RECT()
                    {
                        left = rect.left,
                        top = rect.top,
                        right = rect.left,
                        bottom = rect.top
                    };
                    NativeMethods.SetWindowPlacement(this.Handle, ref p);
                    return;
                }

                var padding = NativeMethods.GetSystemMetrics(SystemMetric.SM_CXPADDEDBORDER);
                var border = new Size(NativeMethods.GetSystemMetrics(SystemMetric.SM_CXFRAME) + padding, NativeMethods.GetSystemMetrics(SystemMetric.SM_CYFRAME) + padding);
                var captionH = NativeMethods.GetSystemMetrics(SystemMetric.SM_CYCAPTION);

                var rectView = new Rectangle(border.Width, captionH + border.Height, b.Width - border.Width * 2, b.Height - captionH - border.Height * 2);

                NativeMethods.SetWindowPlacement(this.Handle, ref p);

                using (this.Region)
                {
                    Region rg;

                    rg = new Region(new Rectangle(0, 0, b.Width, b.Height));
                    rg.Exclude(rectView);

                    if (flags != EnableFlags.None)
                    {
                        var screenRect = Util.ScreenUtil.FromDesktopBounds(b);

                        try
                        {
                            var tbi = NativeMethods.GetTitleBarInfoEx(attachedTo);

                            if ((flags & EnableFlags.EnableClose) != 0)
                            {
                                var rclose = tbi.rgrect[(int)TitleBarElement.Close];
                                var rectClose = new Rectangle(rclose.left - screenRect.Left, rclose.top - screenRect.Top + 1, rclose.right - rclose.left - 1, rclose.bottom - rclose.top);

                                rg.Exclude(rectClose);
                            }
                            if ((flags & EnableFlags.EnableMinimize) != 0)
                            {
                                var rmin = tbi.rgrect[(int)TitleBarElement.Minimize];
                                var rectMinimize = new Rectangle(rmin.left - screenRect.Left, rmin.top - screenRect.Top + 1, rmin.right - rmin.left - 1, rmin.bottom - rmin.top);

                                rg.Exclude(rectMinimize);
                            }
                        }
                        catch (Exception e)
                        {
                            Util.Logging.Log(e);
                        }
                    }

                    this.Region = rg;

                    if (Windows.WindowLong.HasValue(attachedTo, GWL.GWL_EXSTYLE, WindowStyle.WS_EX_TOPMOST))
                    {
                        NativeMethods.SetWindowPos(this.Handle, (IntPtr)WindowZOrder.HWND_TOPMOST, 0, 0, 0, 0, SetWindowPosFlags.SWP_NOMOVE | SetWindowPosFlags.SWP_NOOWNERZORDER | SetWindowPosFlags.SWP_NOSIZE | SetWindowPosFlags.SWP_NOACTIVATE);
                    }
                }
            }
            else
            {
                this.Dispose();
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
            }

            base.Dispose(disposing);
        }

        protected override void OnShown(EventArgs e)
        {
            base.OnShown(e);

            try
            {
                if (process.HasExited)
                {
                    this.Dispose();
                    return;
                }
                else
                {

                    ResizeToParent();

                    NativeMethods.SetWindowLongPtr(this.Handle, (int)GWL.GWL_HWNDPARENT, this.attachedTo);
                    //NativeMethods.SetWindowPos(this.Handle, (IntPtr)this.attachedTo, 0, 0, 0, 0, SetWindowPosFlags.SWP_NOACTIVATE | SetWindowPosFlags.SWP_NOMOVE | SetWindowPosFlags.SWP_NOOWNERZORDER | SetWindowPosFlags.SWP_NOSIZE | SetWindowPosFlags.SWP_NOSENDCHANGING);
                }
            }
            catch (Exception ex)
            {
                Util.Logging.Log(ex);

                this.Dispose();
                return;
            }

            //NativeMethods.BringWindowToTop(this.Handle);
        }

        private void WinEventProc(IntPtr hWinEventHook, uint eventType, IntPtr hwnd, int idObject, int idChild, uint dwEventThread, uint dwmsEventTime)
        {
            switch (eventType)
            {
                case 0x0017:

                    NativeMethods.BringWindowToTop(this.Handle);

                    break;
                case 0x800B:

                    if (idObject == 0)
                    {
                        ResizeToParent();
                    }

                    break;
            }
        }

        protected override bool ShowWithoutActivation
        {
            get
            {
                return true;
            }
        }

        protected override CreateParams CreateParams
        {
            get
            {
                var cp = base.CreateParams;
                var ex = WindowStyle.WS_EX_TRANSPARENT | WindowStyle.WS_EX_NOACTIVATE;
                

                cp.ExStyle |= (int)(ex);

                return cp;
            }
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            //transparent
        }

        protected override void OnPaintBackground(PaintEventArgs e)
        {
            //transparent
        }

        protected override void WndProc(ref Message m)
        {
            switch ((WindowMessages)m.Msg)
            {
                case WindowMessages.WM_MOUSEACTIVATE:
                    
                    m.Result = (IntPtr)3; //MA_NOACTIVATE

                    return;
                case WindowMessages.WM_WINDOWPOSCHANGING:
                    
                    base.WndProc(ref m);
                    ResizeToParent();

                    return;
            }

            base.WndProc(ref m);
        }
    }
}
