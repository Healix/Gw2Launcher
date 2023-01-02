using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;

namespace Gw2Launcher.Tools.Markers
{
    public class MarkerIcons : IDisposable
    {
        private const string KEY_ERROR = ":error";

        private Shared.Images icons;

        public MarkerIcons(bool removeOnRelease = true)
        {
            icons = new Shared.Images(removeOnRelease);
        }

        public Size IconSize
        {
            get;
            set;
        }

        private async void LoadAsync(Shared.Images.IUpdate l, string path)
        {
            if (l == null)
                return;

            using (l)
            {
                try
                {
                    l.SetValue(await Images.LoadAsync(path, IconSize.Width, IconSize.Height));

                    return;
                }
                catch (Exception e)
                {
                    Util.Logging.Log(e);
                }

                try
                {
                    l.SetValue(GetErrorIcon());
                }
                catch(Exception e)
                {
                    Util.Logging.Log(e);
                }
            }
        }

        public Shared.Images.IValueSource GetErrorIcon()
        {
            var source = icons.GetValue(KEY_ERROR);
            if (!source.IsLoaded)
            {
                using (var l = source.BeginUpdate())
                {
                    if (l != null)
                    {
                        l.SetValue(Util.Bitmap.CreateErrorImage(IconSize.Width, IconSize.Height));
                    }
                }
            }
            return source;
        }

        /// <summary>
        /// Returns an icon bound to the marker (the icon for the marker can change)
        /// </summary>
        public Shared.Images.IValueSource GetIcon(Settings.IMarker marker)
        {
            if (string.IsNullOrEmpty(marker.IconPath))
                return null;

            var source = icons.GetValue(marker);
            if (!source.IsLoaded)
            {
                using (var l = source.BeginUpdate())
                {
                    if (l != null)
                    {
                        var source2 = icons.GetValue(marker.IconPath);

                        l.SetValue(source2);

                        if (!source2.IsLoaded)
                        {
                            LoadAsync(source2.BeginUpdate(), marker.IconPath);
                        }
                    }
                }
            }
            return source;
        }

        /// <summary>
        /// Returns an icon for the path
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public Shared.Images.IValueSource GetIcon(string path)
        {
            if (string.IsNullOrEmpty(path))
                return null;

            var source = icons.GetValue(path);
            if (!source.IsLoaded)
            {
                LoadAsync(source.BeginUpdate(), path);
            }
            return source;
        }

        public bool TryGetIcon(object key, out Shared.Images.IValueSource icon)
        {
            return icons.TryGetValue(key, out icon);
        }

        /// <summary>
        /// Adds the icon without returning it; icon will be disposed if disposeOnRelease = true
        /// </summary>
        public void Add(string path)
        {
            using (GetIcon(path))
            {

            }
        }

        public int Count
        {
            get
            {
                return icons.Count;
            }
        }

        public object[] Keys
        {
            get
            {
                return icons.Keys;
            }
        }

        public void Dispose()
        {
            using (icons)
            {
                icons = null;
            }
        }
    }
}
