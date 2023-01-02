using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using System.IO;
using Gw2Launcher.Windows.Native;

namespace Gw2Launcher.Tools
{
    class Icons : IDisposable
    {
        private Icons(Icon small, Icon big)
        {
            this.Small = small;
            this.Big = big;
        }

        ~Icons()
        {
            Dispose();
        }

        public Icon Small
        {
            get;
            private set;
        }

        public Icon Big
        {
            get;
            private set;
        }

        public void Dispose()
        {
            GC.SuppressFinalize(this);

            if (this.Small != null)
            {
                this.Small.Dispose();
                this.Small = null;
            }
            if (this.Big != null)
            {
                this.Big.Dispose();
                this.Big = null;
            }
        }

        public static Icons From(Color color, bool useIcon)
        {
            var widthBig = NativeMethods.GetSystemMetrics(SystemMetric.SM_CXICON);
            var widthSmall = NativeMethods.GetSystemMetrics(SystemMetric.SM_CXSMICON);

            if (useIcon)
            {
                using (var icon = Properties.Resources.Gw2Launcher)
                {
                    Icon small, big;

                    using (var icon2 = new Icon(icon, widthSmall, widthSmall))
                    {
                        using (var bmp = icon2.ToBitmap())
                        {
                            Util.Bitmap.BlendColor(bmp, color, true, 150);
                            small = GetIcon(bmp, widthSmall, widthSmall);
                        }
                    }

                    using (var icon2 = new Icon(icon, widthBig, widthBig))
                    {
                        using (var bmp = icon2.ToBitmap())
                        {
                            Util.Bitmap.BlendColor(bmp, color, true, 150);
                            big = GetIcon(bmp, widthBig, widthBig);
                        }
                    }

                    return new Icons(small, big);
                }
            }
            else
            {
                Icon small, big;

                using (var bmp = new Bitmap(widthSmall, widthSmall, System.Drawing.Imaging.PixelFormat.Format1bppIndexed))
                {
                    var p = bmp.Palette;
                    p.Entries[0] = color;
                    bmp.Palette = p;
                    
                    small = GetIcon(bmp, widthSmall, widthSmall);
                }

                using (var bmp = new Bitmap(widthBig, widthBig, System.Drawing.Imaging.PixelFormat.Format1bppIndexed))
                {
                    var p = bmp.Palette;
                    p.Entries[0] = color;
                    bmp.Palette = p;

                    big = GetIcon(bmp, widthBig, widthBig);
                }

                return new Icons(small, big);
            }
        }

        public static Icons From(string path)
        {
            var widthBig = NativeMethods.GetSystemMetrics(SystemMetric.SM_CXICON);
            var widthSmall = NativeMethods.GetSystemMetrics(SystemMetric.SM_CXSMICON);

            if (path.EndsWith(".ico", StringComparison.OrdinalIgnoreCase))
            {
                try
                {
                    using (var icon = new Icon(path))
                    {
                        var small = new Icon(icon, widthSmall, widthSmall);
                        var big = new Icon(icon, widthBig, widthBig);

                        return new Icons(small, big);
                    }
                }
                catch (Exception e)
                {
                    Util.Logging.Log(e);
                }
            }

            using (var image = Image.FromFile(path))
            {
                var small = GetIcon(image, widthSmall, widthSmall);
                var big = GetIcon(image, widthBig, widthBig);

                return new Icons(small, big);
            }
        }

        public static Icon GetIcon(Image image, int width, int height)
        {
            Bitmap bmp;
            IntPtr handle;

            if (image.Width != width || image.Height != height)
            {
                bmp = new Bitmap(image, width, height);
                handle = bmp.GetHicon();
            }
            else
            {
                bmp = null;
                handle = ((Bitmap)image).GetHicon();
            }

            using (bmp)
            {
                try
                {
                    using (var icon = Icon.FromHandle(handle))
                    {
                        return (Icon)icon.Clone();
                    }
                }
                finally
                {
                    NativeMethods.DestroyIcon(handle);
                }
            }
        }

        public static Icons FromExecutable(string path, bool small = true, bool big = true, int index = 0)
        {
            var _small = small ? new IntPtr[1] : null;
            var _big = big ? new IntPtr[1] : null;

            try
            {
                if (NativeMethods.ExtractIconEx(path, index, _big, _small, 1) > 0)
                {
                    Icon ismall, ibig;

                    ismall = small && _small[0] != IntPtr.Zero ? (Icon)Icon.FromHandle(_small[0]).Clone() : null;
                    ibig = big && _big[0] != IntPtr.Zero ? (Icon)Icon.FromHandle(_big[0]).Clone() : null;

                    return new Icons(ismall, ibig);
                }
            }
            catch (Exception e)
            {
                Util.Logging.Log(e);
            }
            finally
            {
                if (small)
                {
                    for (var i = _small.Length - 1; i >= 0; --i)
                    {
                        if (_small[i] != IntPtr.Zero)
                            NativeMethods.DestroyIcon(_small[i]);
                    }
                }

                if (big)
                {
                    for (var i = _big.Length - 1; i >= 0; --i)
                    {
                        if (_big[i] != IntPtr.Zero)
                            NativeMethods.DestroyIcon(_big[i]);
                    }
                }
            }

            return null;
        }
    }
}
