using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.IO;
using System.Drawing;
using System.Drawing.Imaging;

namespace Gw2Launcher.Tools
{
    class Screenshots : IDisposable
    {
        public event EventHandler<string> ScreenshotProcessed;

        public class Formatter
        {
            public class FormatPart
            {
                public enum PartType
                {
                    Text,
                    Index,
                    Date
                }

                public FormatPart(PartType type, string value)
                {
                    this.Type = type;
                    this.Value = value;
                }

                public PartType Type
                {
                    get;
                    private set;
                }

                public string Value
                {
                    get;
                    private set;
                }
            }

            private Formatter()
            {

            }

            public Formatter(FormatPart[] parts, bool indexed, bool dated)
            {
                this.Format = parts;
                this.Indexed = indexed;
                this.Dated = dated;
            }

            public bool Indexed
            {
                get;
                private set;
            }

            public bool Dated
            {
                get;
                private set;
            }

            public FormatPart[] Format
            {
                get;
                private set;
            }

            private bool Match(string text, int index, string match)
            {
                int l = match.Length;
                if (index + l > text.Length)
                    return false;
                for (int i = 0; i < l; i++)
                {
                    if (text[i + index] != match[i])
                        return false;
                }
                return true;
            }

            public bool TryParse(string text, out int index)
            {
                int i = 0,
                    l = text.Length,
                    j;

                var parts = this.Format;
                var pl = parts.Length;
                
                index = -1;

                //note that the formatter is setup so that there will always be text in-between an index/date

                for (int p = 0; p < pl; p++)
                {
                    var part = parts[p];

                    switch (part.Type)
                    {
                        case FormatPart.PartType.Text:
                            
                            if (!Match(text, i, part.Value))
                                return false;

                            i += part.Value.Length;

                            if (index > 0)
                                return true;

                            break;
                        case FormatPart.PartType.Index:
                            
                            j = part.Value.Length;
                            if (i + j > l)
                                return false;

                            if (text[i] == '0')
                            {
                                if (!int.TryParse(text.Substring(i, j), out index))
                                    return false;

                                i += j;
                            }
                            else
                            {
                                if (p + 1 < pl)
                                {
                                    var next = parts[p + 1].Value;

                                    while (i + j < l && char.IsDigit(text[i + j]))
                                    {
                                        if (!Match(text, i, next))
                                            j++;
                                        else
                                            break;
                                    }
                                }
                                else
                                {
                                    while (i + j < l && char.IsDigit(text[i + j]))
                                        j++;
                                }
                                if (!int.TryParse(text.Substring(i, j), out index))
                                    return false;
                                i += j;
                            }

                            break;
                        case FormatPart.PartType.Date:

                            if (p + 1 < pl)
                            {
                                var next = parts[p + 1].Value;
                                var format = part.Value;
                                var k = i - 1;

                                while ((k = text.IndexOf(next, k + 1, StringComparison.OrdinalIgnoreCase)) != -1)
                                {
                                    DateTime date;
                                    if (DateTime.TryParseExact(text.Substring(i, k - i), part.Value, null, System.Globalization.DateTimeStyles.AssumeLocal, out date))
                                        break;
                                }

                                if (k == -1)
                                    return false;
                                i = k;
                            }
                            else
                            {
                                DateTime date;
                                if (!DateTime.TryParseExact(text.Substring(i), part.Value, null, System.Globalization.DateTimeStyles.AssumeLocal, out date))
                                    return false;
                            }

                            break;
                    }
                }

                if (index > 0)
                {
                    //there's no more parts to parse, but there's still data

                    if (i < l)
                    {
                        if (Indexed) //indexed formats shouldn't have extra data
                            return false;
                        if (text[i] == '-') //non-indexed formats may have copies
                        {
                            i++;
                            int copy;
                            if (!int.TryParse(text.Substring(i), out copy))
                                return false;
                        }
                    }

                    return true;
                }

                return false;
            }

            public string ToFilter()
            {
                var sb = new StringBuilder(50);
                var wildcard = false;

                foreach (var part in this.Format)
                {
                    switch (part.Type)
                    {
                        case FormatPart.PartType.Text:

                            sb.Append(part.Value);
                            wildcard = false;

                            break;
                        case FormatPart.PartType.Index:
                        case FormatPart.PartType.Date:

                            if (!wildcard)
                            {
                                sb.Append('*');
                                wildcard = true;
                            }

                            break;
                    }
                }

                return sb.ToString();
            }

            public string ToString(int index, DateTime date)
            {
                var sb = new StringBuilder(50);

                foreach (var part in this.Format)
                {
                    switch (part.Type)
                    {
                        case FormatPart.PartType.Text:

                            sb.Append(part.Value);

                            break;
                        case FormatPart.PartType.Index:

                            sb.Append(index.ToString(part.Value));

                            break;
                        case FormatPart.PartType.Date:

                            sb.Append(date.ToString(part.Value));

                            break;
                    }
                }

                return sb.ToString();
            }

            private static string ParseDateFormat(string format)
            {
                var sb = new StringBuilder(format.Length * 2);
                var l = format.Length;
                var p = '\0';
                var pcount = 0;
                var pmin = 0;

                for (var i = 0; i < l; i++)
                {
                    char c = format[i];

                    if (c != p)
                    {
                        if (pcount < pmin)
                            sb.Append(new string(p, pmin - pcount));
                        pmin = 0;
                        p = c;
                        pcount = 1;
                    }
                    else
                        pcount++;

                    switch (c)
                    {
                        case 'f':
                            pmin = 1;
                            if (pcount < 8)
                                sb.Append(c);
                            break;
                        case 'd':
                        case 'h':
                        case 'H':
                        case 'm':
                        case 'M':
                        case 's':
                        case 't':
                            pmin = 2;
                            if (pcount < 3)
                                sb.Append(c);
                            break;
                        case 'y':
                            pmin = 2;
                            if (pcount < 6)
                                sb.Append(c);
                            break;
                        case '.':
                        case '-':
                        case '_':
                        case ' ':
                            pmin = 0;
                            sb.Append(c);
                            break;
                    }
                }

                if (pcount < pmin)
                    sb.Append(new string(p, pmin - pcount));

                return sb.ToString();
            }

            public static Formatter Convert(string format)
            {
                var formatter = new Formatter();
                var parts = new List<Formatter.FormatPart>(10);

                var previous = FormatPart.PartType.Text;
                int i = -1;
                int j = 0;

                while ((i = format.IndexOf('<', i + 1)) != -1)
                {
                    if (i < j)
                        return null;
                    if (i > j)
                    {
                        var part=format.Substring(j, i - j);
                        if (previous == FormatPart.PartType.Index && char.IsDigit(part[0]))
                            part = " " + part;
                        parts.Add(new FormatPart(previous = FormatPart.PartType.Text, part));
                    }
                    i++;
                    j = format.IndexOf('>', i);

                    var _format = format.Substring(i, j - i);

                    if (_format.Length > 0)
                    {
                        switch (_format[0])
                        {
                            case '0':
                            case '#':

                                if (previous != FormatPart.PartType.Text)
                                    parts.Add(new FormatPart(FormatPart.PartType.Text, " "));
                                parts.Add(new FormatPart(previous = FormatPart.PartType.Index, new string('0', _format.Length)));
                                formatter.Indexed = true;

                                break;
                            default:

                                _format = ParseDateFormat(_format);
                                if (_format.Length > 0)
                                {
                                    if (previous != FormatPart.PartType.Text)
                                        parts.Add(new FormatPart(FormatPart.PartType.Text, " "));
                                    parts.Add(new FormatPart(previous = FormatPart.PartType.Date, _format));
                                    formatter.Dated = true;
                                }

                                break;
                        }
                    }

                    i = j++;
                }

                i = format.Length;
                if (i > j)
                    parts.Add(new FormatPart(FormatPart.PartType.Text, format.Substring(j, i - j)));

                formatter.Format = parts.ToArray();

                return formatter;
            }
        }

        private class Watcher : IDisposable
        {
            public event EventHandler Error;

            private class Node
            {
                public string value;
                public Node next;
            }

            public string path;
            public ushort subscribers;
            public FileSystemWatcher watcher;

            private bool updated;
            private int index;
            private Node first, last;
            private Screenshots parent;

            public Watcher(Screenshots parent, string path)
            {
                this.parent = parent;
                this.path = path;
                this.subscribers = 1;

                watcher = new FileSystemWatcher(path, "gw*.*");
                watcher.Created += watcher_Created;
                watcher.Error += watcher_Error;
                watcher.NotifyFilter = NotifyFilters.FileName;
                watcher.EnableRaisingEvents = true;

                updated = true;

                Settings.ScreenshotConversion.ValueChanged += ScreenshotSetting_ValueChanged;
                Settings.ScreenshotNaming.ValueChanged += ScreenshotSetting_ValueChanged;
            }

            void ScreenshotSetting_ValueChanged(object sender, EventArgs e)
            {
                updated = true;
            }

            void watcher_Error(object sender, ErrorEventArgs e)
            {
                if (Util.Logging.Enabled)
                {
                    Util.Logging.LogEvent("Error while monitoring screenshots", e.GetException());
                }

                try
                {
                    watcher.EnableRaisingEvents = false;
                    watcher.EnableRaisingEvents = true;
                }
                catch (Exception ex)
                {
                    if (Util.Logging.Enabled)
                    {
                        Util.Logging.LogEvent("Unable to continue monitoring screenshots", ex);
                    }

                    using (watcher)
                    {
                        watcher = null;
                    }

                    if (Error != null)
                        Error(this, EventArgs.Empty);
                }
            }

            void watcher_Created(object sender, FileSystemEventArgs e)
            {
                if (e.ChangeType.HasFlag(WatcherChangeTypes.Created))
                {
                    bool doQueue;

                    if (Util.Logging.Enabled)
                    {
                        Util.Logging.LogEvent(null, "Queued " + e.Name + " for processing");
                    }

                    lock (this)
                    {
                        var n = new Node()
                        {
                            value = e.Name
                        };

                        if (doQueue = first == null)
                        {
                            first = last = n;
                        }
                        else
                        {
                            last.next = n;
                            last = n;
                        }
                    }

                    if (doQueue)
                        parent.Queue(this);
                }
            }

            public void DoQueue(bool convert, bool rename, Formatter formatter, ImageConverter converter)
            {
                if (!convert && !rename)
                {
                    lock(this)
                    {
                        this.first = last = null;
                        return;
                    }
                }

                var indexed = !rename || formatter.Indexed;

                #region Find next index

                if (updated && indexed)
                {
                    updated = false;

                    string filter, ext;

                    if (rename)
                        filter = formatter.ToFilter();
                    else
                        filter = "gw*";

                    if (convert)
                    {
                        switch (converter.Format)
                        {
                            case Settings.ScreenshotConversionOptions.ImageFormat.Png:
                                ext = ".png";
                                break;
                            case Settings.ScreenshotConversionOptions.ImageFormat.Jpg:
                            default:
                                if (rename)
                                    ext = ".jpg";
                                else
                                    ext = ".*";
                                break;
                        }
                    }
                    else
                    {
                        ext = ".*";
                    }

                    index = 0;

                    try
                    {
                        var files = Directory.GetFiles(path, filter + ext, SearchOption.TopDirectoryOnly);

                        foreach (var f in files)
                        {
                            var name = Path.GetFileNameWithoutExtension(f);

                            if (rename)
                            {
                                int i;
                                if (formatter.TryParse(name, out i) && i > index)
                                    index = i;
                            }
                            else
                            {
                                int i;
                                if (name.Length == 5 && int.TryParse(name.Substring(2), out i) && i > index) //default gw000 format
                                    index = i;
                            }
                        }
                    }
                    catch (Exception e)
                    {
                        Util.Logging.Log(e);
                    }
                }

                #endregion

                do
                {
                    var first = this.first;
                    byte retry = 10;

                    var path = Path.Combine(this.path, first.value);
                    string finalpath = null;
                    var canConvert = false;
                    var success = false;
                    string tmp = null;

                    #region Get exclusive file access / convert

                    do
                    {
                        try
                        {
                            if (first.value.Length != 9) //standard screenshots should only be in the format gw000.ext
                            {
                                if (Util.Logging.Enabled)
                                {
                                    Util.Logging.LogEvent(null, "Skipping " + first.value);
                                }
                                break;
                            }
                            if (!(canConvert = first.value.EndsWith(".bmp", StringComparison.OrdinalIgnoreCase)) &&
                                !first.value.EndsWith(".jpg", StringComparison.OrdinalIgnoreCase) &&
                                !first.value.EndsWith(".png", StringComparison.OrdinalIgnoreCase))
                            {
                                if (Util.Logging.Enabled)
                                {
                                    Util.Logging.LogEvent(null, "Skipping " + first.value);
                                }
                                break;
                            }

                            using (var stream = File.Open(path, FileMode.Open, FileAccess.Read, FileShare.None))
                            {
                                if (convert && canConvert)
                                {
                                    Stream output = null;

                                    for (var attempt = 0; ; ++attempt)
                                    {
                                        try
                                        {
                                            output = File.Open(tmp = Path.Combine(this.path, "tmp_" + first.value.Substring(0, first.value.Length - 4)) + "_" + attempt, FileMode.Create, FileAccess.Write, FileShare.None);

                                            break;
                                        }
                                        catch (Exception e)
                                        {
                                            Util.Logging.Log(e);

                                            if (attempt > 3)
                                            {
                                                output = null;
                                                tmp = null;

                                                Util.Logging.LogEvent("Failed to create temp screenshot", e);

                                                break;
                                            }
                                        }
                                    }

                                    if (tmp != null)
                                    {
                                        try
                                        {
                                            using (output)
                                            {
                                                using (var image = Bitmap.FromStream(stream))
                                                {
                                                    if (!converter.Save(image, output))
                                                        throw new NotSupportedException("Unknown conversion format");
                                                }
                                            }

                                            try
                                            {
                                                File.SetLastWriteTimeUtc(tmp, File.GetLastWriteTimeUtc(path));
                                                File.SetCreationTimeUtc(tmp, File.GetCreationTimeUtc(path));
                                            }
                                            catch { }

                                            finalpath = tmp;
                                        }
                                        catch (Exception ex)
                                        {
                                            try
                                            {
                                                File.Delete(tmp);
                                            }
                                            catch { }
                                            tmp = null;
                                            Util.Logging.Log(ex);
                                            if (Util.Logging.Enabled)
                                            {
                                                Util.Logging.LogEvent("Failed to convert screenshot", ex);
                                            }
                                        }
                                    }
                                }
                            }

                            success = true;

                            break;
                        }
                        catch (FileNotFoundException)
                        {
                            break;
                        }
                        catch (IOException e)
                        {
                            bool isLocked;

                            switch (e.HResult & 0xFFFF)
                            {
                                case 32: //ERROR_SHARING_VIOLATION
                                case 33: //ERROR_LOCK_VIOLATION
                                    isLocked = true;
                                    break;
                                default:
                                    isLocked = false;
                                    break;
                            }

                            if (!isLocked || --retry == 0)
                            {
                                if (Util.Logging.Enabled)
                                {
                                    Util.Logging.LogEvent("Error while accessing screenshot (" + first.value + ")", e);
                                }

                                break;
                            }
                        }
                        catch (Exception e)
                        {
                            if (Util.Logging.Enabled)
                            {
                                Util.Logging.LogEvent("Error while accessing screenshot (" + first.value + ")", e);
                            }

                            Util.Logging.Log(e);
                            break;
                        }

                        System.Threading.Thread.Sleep(1000);
                    }
                    while (retry > 0);

                    #endregion

                    #region Rename file

                    if (success && (rename && (!convert || !canConvert) || tmp != null))
                    {
                        try
                        {
                            string from, to, name, ext;
                            DateTime date;

                            if (rename)
                            {
                                if (formatter.Dated)
                                {
                                    try
                                    {
                                        date = File.GetLastWriteTime(path);
                                    }
                                    catch
                                    {
                                        date = DateTime.Now;
                                    }
                                }
                                else
                                {
                                    date = DateTime.MinValue;
                                }
                                name = formatter.ToString(++index, date);
                            }
                            else
                            {
                                date = DateTime.MinValue;
                                name = "gw" + (++index).ToString("000");
                            }

                            if (tmp != null)
                            {
                                switch (converter.Format)
                                {
                                    case Settings.ScreenshotConversionOptions.ImageFormat.Png:
                                        ext = ".png";
                                        break;
                                    case Settings.ScreenshotConversionOptions.ImageFormat.Jpg:
                                    default:
                                        ext = ".jpg";
                                        if (!rename)
                                        {
                                            //gw2 increases the index regardless of it it was a bmp or jpg, so if
                                            //x.bmp exists, x.jpg shouldn't and the index can be used for this conversion
                                            name = first.value.Substring(0, first.value.Length - 4);
                                            index--;
                                        }
                                        break;
                                }

                                from = tmp;
                            }
                            else
                            {
                                ext = Path.GetExtension(path);
                                from = path;
                            }

                            to = Path.Combine(this.path, name + ext);
                            int count = 1;

                            while (true)
                            {
                                try
                                {
                                    File.Move(from, to);

                                    if (Util.Logging.Enabled)
                                    {
                                        if (tmp != null)
                                            Util.Logging.LogEvent(null, first.value + " converted to " + name + ext);
                                        else
                                            Util.Logging.LogEvent(null, first.value + " renamed to " + name + ext);
                                    }

                                    finalpath = to;

                                    if (tmp != null && converter.DeleteOriginal)
                                    {
                                        try
                                        {
                                            File.Delete(path);
                                        }
                                        catch { }
                                    }

                                    break;
                                }
                                catch (IOException ex)
                                {
                                    var code = ex.HResult & 0xFFFF;
                                    if (code == 183) //ERROR_ALREADY_EXISTS
                                    {
                                        count++;

                                        if (rename)
                                        {
                                            if (formatter.Indexed)
                                                to = Path.Combine(this.path, formatter.ToString(++index, date) + ext);
                                            else
                                                to = Path.Combine(this.path, name + "-" + count + ext);
                                        }
                                        else
                                            to = Path.Combine(this.path, "gw" + (++index).ToString("000") + ext);
                                    }
                                    else
                                    {
                                        if (Util.Logging.Enabled)
                                        {
                                            Util.Logging.LogEvent("Unable to move screenshot (" + first.value + ")", ex);
                                        }
                                        index--;
                                        break;
                                    }
                                }
                                catch (Exception ex)
                                {
                                    if (Util.Logging.Enabled)
                                    {
                                        Util.Logging.LogEvent("Unable to move screenshot (" + first.value + ")", ex);
                                    }
                                    Util.Logging.Log(ex);
                                    index--;
                                    break;
                                }
                            }
                        }
                        catch (Exception e)
                        {
                            if (Util.Logging.Enabled)
                            {
                                Util.Logging.LogEvent("Error while handling screenshot (" + first.value + ")", e);
                            }
                            Util.Logging.Log(e);
                        }
                    }

                    #endregion

                    if (finalpath != null)
                    {
                        parent.OnScreenshotProcessed(finalpath);
                    }

                    lock (this)
                    {
                        if (first.next == null)
                        {
                            this.first = last = null;
                            return;
                        }
                        else
                            this.first = first = first.next;
                    }
                }
                while (true);
            }

            public void Dispose()
            {
                if (watcher != null)
                {
                    Settings.ScreenshotConversion.ValueChanged -= ScreenshotSetting_ValueChanged;
                    Settings.ScreenshotNaming.ValueChanged -= ScreenshotSetting_ValueChanged;

                    using (watcher)
                    {
                        watcher = null;
                    }
                }
            }
        }

        private class ImageConverter
        {
            public ImageConverter(Settings.ScreenshotConversionOptions options)
            {
                this.DeleteOriginal = options.DeleteOriginal;
                this.Format = options.Format;

                switch (options.Format)
                {
                    case Settings.ScreenshotConversionOptions.ImageFormat.Png:
                        encoder = GetEncoder(ImageFormat.Png);
                        colors = options.Options;
                        break;
                    case Settings.ScreenshotConversionOptions.ImageFormat.Jpg:
                        encoder = GetEncoder(ImageFormat.Jpeg);
                        if (encoder != null)
                        {
                            parameters = new EncoderParameters(1);
                            parameters.Param[0] = new EncoderParameter(System.Drawing.Imaging.Encoder.Quality, (long)options.Options);
                        }
                        break;
                }
            }

            private ImageCodecInfo encoder;
            private EncoderParameters parameters;
            private byte colors;

            public Settings.ScreenshotConversionOptions.ImageFormat Format
            {
                get;
                private set;
            }

            public bool DeleteOriginal
            {
                get;
                private set;
            }

            public bool CanConvert
            {
                get
                {
                    return encoder != null;
                }
            }

            private ImageCodecInfo GetEncoder(ImageFormat format)
            {
                var codecs = ImageCodecInfo.GetImageDecoders();
                foreach (ImageCodecInfo codec in codecs)
                {
                    if (codec.FormatID == format.Guid)
                        return codec;
                }
                return null;
            }

            public bool Save(Image image, Stream output)
            {
                switch (this.Format)
                {
                    case Settings.ScreenshotConversionOptions.ImageFormat.Jpg:

                        image.Save(output, encoder, parameters);

                        return true;
                    case Settings.ScreenshotConversionOptions.ImageFormat.Png:

                        if (colors == 16)
                        {
                            using (var bmp16 = new Bitmap(image.Width, image.Height, PixelFormat.Format16bppRgb565))
                            {
                                using (var g = Graphics.FromImage(bmp16))
                                {
                                    g.DrawImage(image, 0, 0);
                                }

                                using (var bmp24 = bmp16.Clone(new Rectangle(new Point(0, 0), bmp16.Size), PixelFormat.Format24bppRgb))
                                {
                                    bmp24.Save(output, encoder, parameters);
                                }
                            }
                        }
                        else
                        {
                            image.Save(output, encoder, parameters);
                        }

                        return true;
                }

                return false;
            }
        }

        private class Node
        {
            public Watcher value;
            public Node next;
        }

        private Dictionary<string, Watcher> watchers;
        private Dictionary<ushort, Watcher> accounts;
        private Node first, last;
        private Task task;
        private bool disposing;

        public Screenshots()
        {
            watchers = new Dictionary<string, Watcher>(StringComparer.OrdinalIgnoreCase);
            accounts = new Dictionary<ushort, Watcher>();
        }

        public void Add(Settings.IAccount account)
        {
            lock (watchers)
            {
                var path = GetPath(account);
                if (Util.Logging.Enabled)
                {
                    Util.Logging.LogEvent(account, "Using \"" + path + "\" for screenshots");
                }
                Watcher watcher;
                if (accounts.TryGetValue(account.UID, out watcher))
                {
                    if (watcher.path.Equals(path, StringComparison.OrdinalIgnoreCase))
                        return;
                    else 
                        Unsub(watcher);
                }
                if (!watchers.TryGetValue(path, out watcher))
                {
                    try
                    {
                        watcher = new Watcher(this, path);
                        watcher.Error += watcher_Error;
                    }
                    catch (Exception ex)
                    {
                        if (Util.Logging.Enabled)
                        {
                            Util.Logging.LogEvent(account, "Unable to monitor screenshots", ex);
                        }
                        return;
                    }
                    watchers[path] = watcher;
                }
                else
                    watcher.subscribers++;
                accounts[account.UID] = watcher;
            }
        }

        void watcher_Error(object sender, EventArgs e)
        {
            //prevent deadlock when watcher raises an error while disposing
            while (!Monitor.TryEnter(watchers, 100))
            {
                if (disposing)
                    return;
            }

            try
            {
                var watcher = (Watcher)sender;
                var keys = new ushort[accounts.Count];
                var count = 0;

                foreach (var uid in accounts.Keys)
                {
                    if (accounts[uid] == watcher)
                        keys[count++] = uid;
                }

                for (var i = 0; i < count; i++)
                    accounts.Remove(keys[i]);

                watchers.Remove(watcher.path);
            }
            finally
            {
                Monitor.Exit(watchers);
            }
        }

        public void Remove(Settings.IAccount account)
        {
            lock (watchers)
            {
                Watcher watcher;
                if (accounts.TryGetValue(account.UID, out watcher))
                {
                    accounts.Remove(account.UID);
                    Unsub(watcher);
                }
            }
        }

        private void Unsub(Watcher watcher)
        {
            if (--watcher.subscribers == 0)
            {
                watchers.Remove(watcher.path);
                watcher.Dispose();
            }
        }

        private string GetPath(Settings.IAccount account)
        {
            if (!string.IsNullOrEmpty(account.ScreenshotsLocation))
                return account.ScreenshotsLocation;

            var s = Settings.GetSettings(account.Type);
            if (s.ScreenshotsLocation.HasValue)
            {
                var path = s.ScreenshotsLocation.Value;
                if (!string.IsNullOrEmpty(path))
                    return path;
            }

            return Client.FileManager.GetPath(Client.FileManager.SpecialPath.Screens);
        }

        private void Queue(Watcher watcher)
        {
            bool doQueue;

            lock (this)
            {
                var n = new Node()
                {
                    value = watcher
                };
                if (doQueue = first == null)
                {
                    first = last = n;
                }
                else
                {
                    last.next = n;
                    last = n;
                }
            }

            if (doQueue)
            {
                if (task != null && task.IsCompleted)
                    task.Dispose();
                task = Task.Run(new Action(DoQueue));
            }
        }

        private void OnScreenshotProcessed(string path)
        {
            if (ScreenshotProcessed != null)
            {
                try
                {
                    ScreenshotProcessed(this, path);
                }
                catch { }
            }
        }

        private void DoQueue()
        {
            Formatter formatter = null;
            ImageConverter converter = null;
            bool convert = false,
                 rename = false;

            if (Settings.ScreenshotNaming.HasValue)
            {
                try
                {
                    formatter = Formatter.Convert(Settings.ScreenshotNaming.Value);
                    rename = formatter != null;
                }
                catch (Exception e)
                {
                    Util.Logging.Log(e);

                    if (Util.Logging.Enabled)
                    {
                        Util.Logging.LogEvent("Unable to initialize screenshot formatter using \"" + Settings.ScreenshotNaming.Value + "\"", e);
                    }
                }
            }

            if (Settings.ScreenshotConversion.HasValue)
            {
                var options = Settings.ScreenshotConversion.Value;
                if (options.Format != Settings.ScreenshotConversionOptions.ImageFormat.None)
                {
                    try
                    {
                        converter = new ImageConverter(options);
                        convert = converter.CanConvert;
                    }
                    catch (Exception e)
                    {
                        if (Util.Logging.Enabled)
                        {
                            Util.Logging.LogEvent("Unable to initialize screenshot converter", e);
                        }
                    }
                }
            }

            do
            {
                var first = this.first;

                try
                {
                    first.value.DoQueue(convert, rename, formatter, converter);
                }
                catch (Exception e)
                {
                    if (Util.Logging.Enabled)
                    {
                        Util.Logging.LogEvent("Error while handling screenshot", e);
                    }
                }

                lock(this)
                {
                    if (first.next == null)
                    {
                        this.first = this.last = null;
                        return;
                    }
                    else
                        this.first = first = first.next;
                }
            }
            while (true);
        }

        /// <summary>
        /// Converts and/or renames screenshots
        /// </summary>
        /// <param name="path">The path where screenshots should be stored</param>
        /// <param name="files">Screenshots to process</param>
        /// <param name="rename">True if the files whould be renamed</param>
        /// <param name="convert">True if *.bmp files should be converted</param>
        /// <param name="formatter">Name formatter</param>
        /// <param name="conversion">Image conversion options</param>
        public static void ConvertRename(string path, string[] files, bool rename, bool convert, Formatter formatter, Settings.ScreenshotConversionOptions conversion)
        {
            var index = 0;

            ImageConverter converter;
            if (convert)
                converter = new ImageConverter(conversion);
            else
                converter = null;

            if (rename)
            {
                if (formatter.Indexed)
                {
                    var _files = new Dictionary<string, int>(files.Length, StringComparer.OrdinalIgnoreCase);
                    var existing = Directory.GetFiles(path, formatter.ToFilter() + ".*");
                    var dates = new DateTime[files.Length];

                    for (var i = files.Length - 1; i >= 0; i--)
                    {
                        _files[files[i]] = i;
                        dates[i] = File.GetLastWriteTimeUtc(files[i]);
                    }

                    foreach (var file in existing)
                    {
                        int _index;
                        if (formatter.TryParse(Path.GetFileNameWithoutExtension(file), out _index))
                        {
                            if (_files.ContainsKey(file))
                            {
                                var i = 0;
                                var name = Path.GetFileName(file);
                                string from = file,
                                       to = Path.Combine(path, i + "-" + name);

                                while (true)
                                {
                                    try
                                    {
                                        File.Move(from, to);

                                        var j = _files[file];
                                        files[j] = to;
                                        _files.Remove(file);
                                        _files[to] = j;

                                        break;
                                    }
                                    catch (IOException e)
                                    {
                                        var code = e.HResult & 0xFFFF;
                                        if (code != 183) //ERROR_ALREADY_EXISTS
                                            throw;
                                        to = Path.Combine(path, ++i + "-" + name);
                                    }
                                }
                            }
                            else if (_index > index)
                                index = _index;
                        }
                    }

                    Array.Sort(dates, files);
                }
            }
            else if (convert)
            {
                string ext;
                if (conversion.Format == Settings.ScreenshotConversionOptions.ImageFormat.Png)
                    ext = ".png";
                else
                    ext = ".*";
                var existing = Directory.GetFiles(path, "gw*" + ext);

                foreach (var file in existing)
                {
                    var name = Path.GetFileNameWithoutExtension(file);
                    int _index;
                    if (name.Length == 5 && int.TryParse(name.Substring(2), out _index) && _index > index)
                        index = _index;
                }
            }

            foreach (var file in files)
            {
                bool canConvert;
                if (!(canConvert = file.EndsWith(".bmp", StringComparison.OrdinalIgnoreCase)) &&
                    !file.EndsWith(".jpg", StringComparison.OrdinalIgnoreCase) &&
                    !file.EndsWith(".png", StringComparison.OrdinalIgnoreCase))
                    continue;

                string tmp = null,
                       name = Path.GetFileName(file);

                #region Convert file

                if (convert && canConvert)
                {
                    try
                    {
                        using (var stream = File.Open(file, FileMode.Open, FileAccess.Read, FileShare.None))
                        {
                            Stream output;
                            try
                            {
                                output = File.Open(tmp = Path.Combine(path, "tmp_" + name.Substring(0, name.Length - 4)), FileMode.Create, FileAccess.Write, FileShare.None);
                            }
                            catch (Exception e)
                            {
                                Util.Logging.Log(e);
                                output = null;
                                tmp = null;
                            }

                            if (tmp != null)
                            {
                                try
                                {
                                    using (output)
                                    {
                                        using (var image = Bitmap.FromStream(stream))
                                        {
                                            if (!converter.Save(image, output))
                                                throw new NotSupportedException("Unknown conversion format");
                                        }
                                    }

                                    try
                                    {
                                        File.SetLastWriteTimeUtc(tmp, File.GetLastWriteTimeUtc(file));
                                        File.SetCreationTimeUtc(tmp, File.GetCreationTimeUtc(file));
                                    }
                                    catch { }
                                }
                                catch (Exception ex)
                                {
                                    try
                                    {
                                        File.Delete(tmp);
                                    }
                                    catch { }
                                    tmp = null;
                                    Util.Logging.Log(ex);
                                }
                            }
                        }
                    }
                    catch (Exception e)
                    {
                        Util.Logging.Log(e);
                        continue;
                    }
                }

                #endregion

                #region Rename file

                if (rename && (!convert || !canConvert) || tmp != null)
                {
                    try
                    {
                        string from, to, _name, ext;
                        DateTime date;

                        if (rename)
                        {
                            if (formatter.Dated)
                            {
                                try
                                {
                                    date = File.GetLastWriteTime(file);
                                }
                                catch
                                {
                                    date = DateTime.Now;
                                }
                            }
                            else
                            {
                                date = DateTime.MinValue;
                            }
                            _name = formatter.ToString(++index, date);
                        }
                        else
                        {
                            date = DateTime.MinValue;
                            _name = "gw" + (++index).ToString("000");
                        }

                        if (tmp != null)
                        {
                            switch (converter.Format)
                            {
                                case Settings.ScreenshotConversionOptions.ImageFormat.Png:
                                    ext = ".png";
                                    break;
                                case Settings.ScreenshotConversionOptions.ImageFormat.Jpg:
                                default:
                                    ext = ".jpg";
                                    if (!rename)
                                    {
                                        //gw2 increases the index regardless of it it was a bmp or jpg, so if
                                        //x.bmp exists, x.jpg shouldn't and the index can be used for this conversion
                                        _name = name.Substring(0, name.Length - 4);
                                        index--;
                                    }
                                    break;
                            }

                            from = tmp;
                        }
                        else
                        {
                            ext = Path.GetExtension(file);
                            from = file;
                        }

                        to = Path.Combine(path, _name + ext);
                        int count = 1;

                        while (true)
                        {
                            try
                            {
                                File.Move(from, to);

                                if (tmp != null && converter.DeleteOriginal)
                                {
                                    try
                                    {
                                        File.Delete(file);
                                    }
                                    catch { }
                                }

                                break;
                            }
                            catch (IOException ex)
                            {
                                var code = ex.HResult & 0xFFFF;
                                if (code == 183) //ERROR_ALREADY_EXISTS
                                {
                                    count++;

                                    if (rename)
                                    {
                                        if (formatter.Indexed)
                                            to = Path.Combine(path, formatter.ToString(++index, date) + ext);
                                        else
                                            to = Path.Combine(path, _name + "-" + count + ext);
                                    }
                                    else
                                        to = Path.Combine(path, "gw" + (++index).ToString("000") + ext);
                                }
                                else
                                {
                                    index--;
                                    break;
                                }
                            }
                            catch (Exception ex)
                            {
                                Util.Logging.Log(ex);
                                index--;
                                break;
                            }
                        }
                    }
                    catch (Exception e)
                    {
                        Util.Logging.Log(e);
                    }
                }

                #endregion
            }
        }

        public void Dispose()
        {
            disposing = true;

            lock(watchers)
            {
                foreach (var watcher in watchers.Values)
                {
                    watcher.Dispose();
                }
                if (task != null && task.IsCompleted)
                {
                    task.Dispose();
                }
                task = null;
            }
        }
    }
}
