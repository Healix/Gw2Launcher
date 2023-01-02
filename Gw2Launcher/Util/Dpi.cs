using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using System.Windows.Forms;
using Gw2Launcher.Windows.Native;

namespace Gw2Launcher.Util
{
    static class Dpi
    {
        public static Point GetDpi(Screen screen)
        {
            try
            {
                uint dpiX, dpiY;

                //requires 8.1+
                if (NativeMethods.GetDpiForMonitor((IntPtr)screen.GetHashCode(), MonitorDpiType.Effective, out dpiX, out dpiY) == 0)
                {
                    return new Point((int)dpiX, (int)dpiY);
                }
            }
            catch { }

            var dc = NativeMethods.CreateDC(screen.DeviceName, null, null, IntPtr.Zero);

            if (dc == IntPtr.Zero)
            {
                using (var g = Graphics.FromHwnd(IntPtr.Zero))
                {
                    return new Point((int)g.DpiX, (int)g.DpiY);
                }
            }
            else
            {
                try
                {
                    using (var g = Graphics.FromHdc(dc))
                    {
                        return new Point((int)g.DpiX, (int)g.DpiY);
                    }
                }
                finally
                {
                    NativeMethods.DeleteDC(dc);
                }
            }
        }

        /// <summary>
        /// Returns the size of a window's frame when scaled for the given screen
        /// </summary>
        /// <param name="includeCaption">Includes the window's caption in the frame size</param>
        public static Padding GetWindowFrame(Screen screen, bool includeCaption = false)
        {
            if (!screen.Primary)
            {
                uint dpiX, dpiY;

                try
                {
                    //requires 8.1+
                    NativeMethods.GetDpiForMonitor((IntPtr)screen.GetHashCode(), MonitorDpiType.Effective, out dpiX, out dpiY);

                    try
                    {
                        //requires 10
                        var padding = NativeMethods.GetSystemMetricsForDpi(SystemMetric.SM_CXPADDEDBORDER, dpiX);
                        var bw = NativeMethods.GetSystemMetricsForDpi(SystemMetric.SM_CXFRAME, dpiX) + padding;
                        var bh = NativeMethods.GetSystemMetricsForDpi(SystemMetric.SM_CYFRAME, dpiX) + padding;
                        
                        if (includeCaption)
                        {
                            return new Padding(bw, NativeMethods.GetSystemMetricsForDpi(SystemMetric.SM_CYCAPTION, dpiX) + bh, bw, bh);
                        }

                        return new Padding(bw, 0, bw, bh);
                    }
                    catch
                    {
                        uint dpipX, dpipY;
                        if (NativeMethods.GetDpiForMonitor((IntPtr)Screen.PrimaryScreen.GetHashCode(), MonitorDpiType.Effective, out dpipX, out dpipY) == 0 && dpipX != dpiX)
                        {
                            using (var f = new Form())
                            {
                                f.StartPosition = FormStartPosition.Manual;
                                f.Location = screen.WorkingArea.Location;
                                f.ShowInTaskbar = false;

                                var h = f.Handle;

                                var bw = (f.Width - f.ClientSize.Width) / 2;

                                if (includeCaption)
                                {
                                    var bh = f.Height - f.ClientSize.Height;
                                    return new Padding(bw, bh - bw, bw, bw);
                                }

                                return new Padding(bw, 0, bw, bw);
                            }
                        }
                    }
                }
                catch
                {
                }
            }

            {
                var padding = NativeMethods.GetSystemMetrics(SystemMetric.SM_CXPADDEDBORDER);
                var bw = NativeMethods.GetSystemMetrics(SystemMetric.SM_CXFRAME) + padding;
                var bh = NativeMethods.GetSystemMetrics(SystemMetric.SM_CYFRAME) + padding;
                if (includeCaption)
                {
                    return new Padding(bw, NativeMethods.GetSystemMetrics(SystemMetric.SM_CYCAPTION) + bh, bw, bh);
                }
                return new Padding(bw, 0, bw, bh);
            }
        }
    }
}
