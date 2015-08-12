using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Threading;
using System.Windows.Forms;
using System.Drawing;
using System.Diagnostics;
using System.IO;
using System.IO.MemoryMappedFiles;

namespace Gw2Launcher
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main(string[] args)
        {
            #region Task: -users:active:yes|no

            if (args.Length > 0)
            {
                if (args[0].StartsWith("-users:active:"))
                {
                    bool activate = args[0] == "-users:active:yes";

                    try
                    {
                        string[] users = Settings.HiddenUserAccounts.GetKeys();
                        if (users.Length > 0)
                            Util.ProcessUtil.ActivateUsers(users, activate);
                    }
                    catch { }

                    return;
                }
            }

            #endregion

            #region Allow only 1 process

            try
            {
                using (Process current = Process.GetCurrentProcess())
                {
                    FileInfo fi = new FileInfo(current.MainModule.FileName);
                    Process[] ps = Process.GetProcessesByName(fi.Name.Substring(0, fi.Name.Length - fi.Extension.Length));
                    foreach (Process p in ps)
                    {
                        using (p)
                        {
                            if (p.Id != current.Id && !p.HasExited)
                            {
                                try
                                {
                                    if (string.Equals(p.MainModule.FileName, fi.FullName, StringComparison.OrdinalIgnoreCase))
                                    {
                                        IntPtr ptr = IntPtr.Zero;
                                        try
                                        {
                                            ptr = Windows.FindWindow.Find(p.Id, null);

                                            var placement = Windows.WindowSize.GetWindowPlacement(ptr);

                                            if (placement.showCmd == (int)Windows.WindowSize.WindowState.SW_SHOWMINIMIZED)
                                                Windows.WindowSize.SetWindowPlacement(ptr, Rectangle.FromLTRB(placement.rcNormalPosition.left, placement.rcNormalPosition.top, placement.rcNormalPosition.right, placement.rcNormalPosition.bottom), Windows.WindowSize.WindowState.SW_RESTORE);
                                            
                                            SetForegroundWindow(ptr);
                                        }
                                        catch { }
                                        return;
                                    }
                                }
                                catch { }
                            }
                        }
                    }
                }
            }
            catch { }

            #endregion

            if (Util.Users.UserName == null)
            {
                //init
            }

            var store = Settings.StoreCredentials;
            store.ValueChanged += StoredCredentials_ValueChanged;
            Security.Credentials.StoreCredentials = store.Value;

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            var f = new UI.formMain();
            var s = Settings.WindowBounds[typeof(UI.formMain)];

            if (s.HasValue && !s.Value.Size.IsEmpty)
            {
                f.AutoSizeGrid = false;
                f.Size = s.Value.Size;
            }
            else
                f.AutoSizeGrid = true;

            if (s.HasValue && !s.Value.Location.Equals(new Point(int.MinValue, int.MinValue)))
                f.Location = Util.ScreenUtil.Constrain(s.Value.Location, f.Size);
            else
            {
                var bounds = Screen.PrimaryScreen.WorkingArea;
                f.Location = Point.Add(bounds.Location, new Size(bounds.Width / 2 - f.Size.Width / 2, bounds.Height / 3));
            }

            f.FormClosed += FormClosed;

            Util.Users.Activate(true);

            Application.Run(f);
        }

        [System.Runtime.InteropServices.DllImport("user32.dll")]
        private static extern bool SetForegroundWindow(IntPtr hWnd);

        static void FormClosed(object sender, FormClosedEventArgs e)
        {
            Form f = sender as Form;
            if (f != null)
            {
                try
                {
                    f.Visible = false;
                }
                catch { }
            }

            Util.Users.Activate(false);
        }

        static void StoredCredentials_ValueChanged(object sender, EventArgs e)
        {
            Security.Credentials.StoreCredentials = ((Settings.ISettingValue<bool>)sender).Value;
        }
    }
}
