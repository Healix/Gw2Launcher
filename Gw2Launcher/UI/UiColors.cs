using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using System.Windows.Forms;
using System.ComponentModel;
using System.IO;

namespace Gw2Launcher.UI
{
    public static class UiColors
    {
        public static event EventHandler ColorsChanged;

        public enum Theme : byte
        {
            /// <summary>
            /// The saved or default theme
            /// </summary>
            Settings = 0,
            /// <summary>
            /// Light colored theme
            /// </summary>
            Light = 1,
            /// <summary>
            /// Dark colored theme
            /// </summary>
            Dark = 2,
        }

        public enum ColorFormat
        {
            Hex = 0,
            RGB = 1,
        }

        public enum Colors : short
        {
            Custom = -1,
            AccountName = 0,
            AccountUser = 1,
            AccountStatusDefault = 2,
            AccountStatusError = 3,
            AccountStatusOK = 4,
            AccountStatusWaiting = 5,
            AccountBackColorDefault = 6,
            AccountBackColorHovered = 7,
            AccountBackColorSelected = 8,
            AccountForeColorHovered = 9,
            AccountForeColorSelected = 10,
            AccountBorderLightDefault = 11,
            AccountBorderLightHovered = 12,
            AccountBorderLightSelected = 13,
            AccountBorderDarkDefault = 14,
            AccountBorderDarkHovered = 15,
            AccountBorderDarkSelected = 16,
            AccountFocusedHighlight = 17,
            AccountFocusedBorder = 18,
            AccountActionExitFill = 19,
            AccountActionExitFlash = 20,
            AccountActionFocusFlash = 21,
            AccountBackColorActive = 22,
            AccountForeColorActive = 23,
            AccountBorderLightActive = 24,
            AccountBorderDarkActive = 25,
            AccountActiveHighlight = 26,
            AccountActiveBorder = 27,
            Text = 28,
            MainBackColor = 29,
            MainBorder = 30,
            BackColor100 = 31,
            BackColor95 = 32,
            BackColor90 = 33,
            BackColor80 = 34,
            BarBackColor = 35,
            BarBorder = 36,
            BarTitle = 37,
            BarTitleInactive = 38,
            BarBackColorHovered = 39,
            BarForeColorHovered = 40,
            BarForeColor = 41,
            BarForeColorInactive = 42,
            BarMinimizeForeColor = 43,
            BarMinimizeForeColorInactive = 44,
            BarCloseForeColor = 45,
            BarCloseForeColorHovered = 46,
            BarCloseForeColorInactive = 47,
            BarCloseBackColorHovered = 48,
            BarAccountBarBackColor = 49,
            BarAccountBarBackColorHovered = 50,
            BarAccountBarBorder = 51,
            BarAccountBarForeColor = 52,
            BarTemplatesBackColor = 53,
            BarTemplatesBackColorHovered = 54,
            BarTemplatesBorder = 55,
            BarTemplatesForeColor = 56,
            BarLaunchAllBackColor = 57,
            BarLaunchAllBackColorHovered = 58,
            BarLaunchAllBorder = 59,
            BarLaunchAllForeColor = 60,
            BarCloseAllBackColor = 61,
            BarCloseAllBackColorHovered = 62,
            BarCloseAllBorder = 63,
            BarCloseAllForeColor = 64,
            BarMinimizeAllBackColor = 65,
            BarMinimizeAllBackColorHovered = 66,
            BarMinimizeAllBorder = 67,
            BarMinimizeAllForeColor = 68,
            MenuBackColor = 69,
            MenuBackColorHovered = 70,
            MenuText = 71,
            MenuArrow = 72,
            MenuArrowHovered = 73,
            MenuSeparator = 74,
            MenuCloseAllForeColor = 75,
            MenuCloseAllBackColor = 76,
            MenuCloseAllBackColorHovered = 77,
            MenuCloseAllBorderColor = 78,
            MenuCloseAllBorderColorHovered = 79,
            ResizeHandle = 80,
            ShapeButtonForeColor = 81,
            ShapeButtonForeColorHovered = 82,
            ArrowForeColor = 83,
            ArrowForeColorHovered = 84,
            SelectButtonForeColor = 85,
            SelectButtonForeColorHovered = 86,
            SelectButtonBackColor = 87,
            SelectButtonBackColorHovered = 88,
            SelectButtonBorderColor = 89,
            SelectButtonBorderColorHovered = 90,
            DailiesText = 91,
            DailiesTextLight = 92,
            DailiesBackColor = 93,
            DailiesHeader = 94,
            DailiesHeaderHovered = 95,
            DailiesHeaderArrow = 96,
            DailiesSeparator = 97,
            DailiesMinimizeBackColor = 98,
            DailiesMinimizeBackColorHovered = 99,
            DailiesMinimizeArrow = 100,
            DailiesMinimizeArrowHovered = 101,
            ScrollTrack = 102,
            ScrollBar = 103,
            ScrollBarHovered = 104,
            Link = 105,
            LinkHovered = 106,
            TextGray = 107,
            TextBlue = 108,
            TextRed = 109,
            TextBoxBackColor = 110,
            TextBoxBorderColor = 111,
            TextBoxBorderColorFocused = 112,
            AccountPinnedColor = 113,
            AccountPinnedBorder = 114,
            AccountSelectionHighlight = 115,
            BarLaunchDailyBackColor = 116,
            BarLaunchDailyBackColorHovered = 117,
            BarLaunchDailyBorder = 118,
            BarLaunchDailyForeColor = 119,
        }

        public interface IColors
        {
            void RefreshColors();
        }

        public class ColorValues : IEquatable<ColorValues>
        {
            private Color[] colors;
            private ushort[] indexes;
            private bool isReadOnly;
            private Theme baseTheme;

            public ColorValues(Theme baseTheme)
            {
                this.baseTheme = baseTheme;
                this.colors = new Color[COLORS];
            }

            public ColorValues(Theme baseTheme, Color[] colors, ushort[] indexes = null, bool isReadOnly = false)
            {
                this.baseTheme = baseTheme;
                this.colors = colors;
                this.indexes = indexes;
                this.isReadOnly = isReadOnly;
            }

            public Theme BaseTheme
            {
                get
                {
                    return baseTheme;
                }
                set
                {
                    baseTheme = value;
                }
            }

            public Color[] Colors
            {
                get
                {
                    return colors;
                }
            }

            public ushort[] Indexes
            {
                get
                {
                    return indexes;
                }
            }

            public Color this[int i]
            {
                get
                {
                    return GetColor(i);
                }
                set
                {
                    if (isReadOnly)
                        return;

                    if (indexes != null)
                    {
                        if (i < 0 || i > indexes.Length)
                            return;
                        colors[indexes[i]] = value;
                    }
                    else
                    {
                        if (i < 0 || i > colors.Length)
                            return;
                        colors[i] = value;
                    }
                }
            }

            public Color this[Colors c]
            {
                get
                {
                    return GetColor(c);
                }
                set
                {
                    this[(int)c] = value;
                }
            }
            
            public Color GetColor(Colors c)
            {
                return GetColor((int)c);
            }

            public Color GetColor(int i)
            {
                if (indexes != null)
                {
                    if (i < 0 || i > indexes.Length)
                        return Color.Empty;
                    return colors[indexes[i]];
                }
                else
                {
                    if (i < 0 || i > colors.Length)
                        return Color.Empty;
                    return colors[i];
                }
            }

            public ColorValues Clone()
            {
                var colors = new Color[this.colors.Length];
                ushort[] indexes;

                if (this.indexes != null)
                {
                    indexes = new ushort[this.indexes.Length];
                    Array.Copy(this.indexes, indexes, indexes.Length);
                }
                else
                    indexes = null;

                Array.Copy(this.colors, colors, colors.Length);

                return new ColorValues(baseTheme, colors, indexes);
            }

            /// <summary>
            /// Returns the number of other colors sharing the same value
            /// </summary>
            public int GetSharedCount(Colors c)
            {
                var i = (int)c;
                if (indexes == null || i < 0 || i > indexes.Length)
                    return 0;

                i = indexes[i];

                var count = 0;

                for (var j = 0; j < indexes.Length; j++)
                {
                    if (indexes[j] == i)
                        ++count;
                }

                return count - 1;
            }

            /// <summary>
            /// Returns colors that are sharing the same value
            /// </summary>
            public IEnumerable<Colors> GetShared(Colors c)
            {
                var i = (int)c;
                if (indexes == null || i < 0 || i > indexes.Length)
                    yield break;

                var v = indexes[i];

                for (var j = 0; j < indexes.Length; j++)
                {
                    if (indexes[j] == v && j != i)
                        yield return (Colors)j;
                }
            }

            /// <summary>
            /// Returns which colors are being shared for each color, or nothing if sharing is not used
            /// </summary>
            public Colors[][] GetShared()
            {
                var indexes = this.Indexes;

                if (indexes == null)
                    return new UiColors.Colors[0][];

                var sharedByIndex = new UiColors.Colors[indexes.Length][];
                var sharedByColor = new UiColors.Colors[this.Colors.Length][];
                var counts = new int[sharedByColor.Length];

                for (var i = 0; i < indexes.Length; i++)
                {
                    counts[indexes[i]]++;
                }

                for (var i = 0; i < this.Indexes.Length; i++)
                {
                    var j = indexes[i];

                    if (sharedByColor[j] == null)
                    {
                        sharedByColor[j] = new Colors[counts[j]];
                        counts[j] = 0; //reusing counts for indexing
                    }

                    sharedByColor[j][counts[j]++] = (Colors)i;
                    sharedByIndex[i] = sharedByColor[j];
                }

                return sharedByIndex;
            }

            public bool IsReadOnly
            {
                get
                {
                    return isReadOnly;
                }
            }

            public bool IsCompressed
            {
                get
                {
                    return indexes != null;
                }
            }

            public int Count
            {
                get
                {
                    if (indexes != null)
                        return indexes.Length;
                    return colors.Length;
                }
            }

            /// <summary>
            /// Decompresses the array or returns a copy of the array if not compressed
            /// </summary>
            public Color[] Decompress()
            {
                if (indexes == null)
                {
                    var colors = new Color[this.colors.Length];
                    Array.Copy(this.colors, colors, colors.Length);
                    return colors;
                }
                else
                {
                    var colors = new Color[this.indexes.Length];

                    for (var i = 0; i < indexes.Length; i++)
                    {
                        colors[i] = this.colors[indexes[i]];
                    }

                    return colors;
                }
            }

            public bool Equals(ColorValues o)
            {
                var count = this.Count;
                if (o.Count != count || o.BaseTheme != this.BaseTheme)
                    return false;
                
                for (var i = 0; i < count; i++)
                {
                    if (o.GetColor(i).ToArgb() != GetColor(i).ToArgb())
                        return false;
                }

                return true;
            }
        }

        private static ColorValues current;
        private static readonly ushort COLORS;

        static UiColors()
        {
            COLORS = (ushort)(Enum.GetValues(typeof(Colors)).Length - 1);
            Settings.StyleColors.ValueChanged += StyleColors_ValueChanged;

#if DEBUG
            Load();
#endif
        }

        public static void Load()
        {
            current = GetTheme(Theme.Settings);
        }

        /// <summary>
        /// Total number of colors
        /// </summary>
        public static ushort Count
        {
            get
            {
                return COLORS;
            }
        }

        /// <summary>
        /// Returns colors for the desired theme
        /// </summary>
        public static ColorValues GetTheme(Theme s)
        {
            switch (s)
            {
                case Theme.Settings:

                    if (Settings.StyleColors.HasValue)
                    {
                        var v = Settings.StyleColors.Value;

                        if (v.Theme == Theme.Settings)
                        {
                            return v.Colors;
                        }
                        else
                        {
                            return GetTheme(v.Theme);
                        }
                    }
                    else
                    {
                        return GetTheme(Theme.Light);
                    }

                case Theme.Dark:

                    return GetThemeDark();

                case Theme.Light:
                default:

                    return GetThemeLight();
            }
        }

        /// <summary>
        /// Returns colors for the current theme
        /// </summary>
        public static ColorValues GetTheme()
        {
            return current;
        }

        private static ColorValues GetThemeLight()
        {
            var colors = new Color[]
            {
                Color.FromArgb(-16777216), Color.FromArgb(-12300415), Color.FromArgb(-8355712), Color.FromArgb(-7667712), Color.FromArgb(-16751616), Color.FromArgb(-16777077), Color.FromArgb(-1), 
                Color.FromArgb(-1315861), Color.FromArgb(-1643276), Color.FromArgb(-2133338153), Color.FromArgb(-2133992983), Color.FromArgb(-986896), Color.FromArgb(-1973791), Color.FromArgb(-2301206), 
                Color.FromArgb(-1644826), Color.FromArgb(-2631721), Color.FromArgb(-2959136), Color.FromArgb(-4341046), Color.FromArgb(-12629812), Color.FromArgb(-728602), Color.FromArgb(-12332), 
                Color.FromArgb(-2236161), Color.FromArgb(-2133988903), Color.FromArgb(-4339006), Color.FromArgb(-13207498), Color.FromArgb(-460552), Color.FromArgb(-8882056), Color.FromArgb(-1842205), 
                Color.FromArgb(-3355444), Color.FromArgb(-3618616), Color.FromArgb(-7566196), Color.FromArgb(-2302756), Color.FromArgb(-10197916), Color.FromArgb(-6908266), Color.FromArgb(-5723992), 
                Color.FromArgb(-1568477), Color.FromArgb(-7895161), Color.FromArgb(-14145496), Color.FromArgb(-15430636), Color.FromArgb(-15295466), Color.FromArgb(-15714549), Color.FromArgb(-2744545), 
                Color.FromArgb(-1039838), Color.FromArgb(-11266031), Color.FromArgb(-4276546), Color.FromArgb(-2763307), Color.FromArgb(-11908534), Color.FromArgb(-13487566), Color.FromArgb(-735546), 
                Color.FromArgb(-740943), Color.FromArgb(-4479839), Color.FromArgb(-5863548), Color.FromArgb(-5658199), Color.FromArgb(-9605779), Color.FromArgb(-13158601), Color.FromArgb(-4274996), 
                Color.FromArgb(-2697514), Color.FromArgb(-10855846), Color.FromArgb(-13534734), Color.FromArgb(-14527063), Color.FromArgb(-16776961), Color.FromArgb(-8388608), Color.FromArgb(-14803426), 
                Color.FromArgb(-16746281), Color.FromArgb(-15572098), Color.FromArgb(-15303780), Color.FromArgb(-16045001), 
            };

            var indexes = new ushort[]
            {
                0,1,2,3,4,5,6,7,8,9,10,11,12,13,14,15,16,17,18,19,20,21,6,22,11,14,23,24,0,25,26,6,11,27,28,7,29,0,30,31,0,32,33,2,34,32,6,33,35,26,36,37,6,26,36,37,6,38,39,40,6,41,
                42,43,6,44,45,46,6,6,11,0,33,47,31,0,48,49,50,51,52,53,54,33,47,47,0,11,8,29,55,0,53,6,27,28,53,11,6,27,53,54,56,33,57,58,59,53,60,61,6,26,62,2,6,63,64,65,66,6,
            };

            return new ColorValues(Theme.Light, colors, indexes, false);
        }

        private static ColorValues GetThemeDark()
        {
            var colors = new Color[]
            {
                Color.FromArgb(-3618616), Color.FromArgb(-12952734), Color.FromArgb(-10461088), Color.FromArgb(-5231315), Color.FromArgb(-10645667), Color.FromArgb(-10652785), Color.FromArgb(-15263977), 
                Color.FromArgb(-14606047), Color.FromArgb(-14802134), Color.FromArgb(-13948117), Color.FromArgb(-14340291), Color.FromArgb(-15461356), Color.FromArgb(-14869219), Color.FromArgb(-15065563), 
                Color.FromArgb(-15790321), Color.FromArgb(-15329770), Color.FromArgb(-15394786), Color.FromArgb(-13615790), Color.FromArgb(-13215883), Color.FromArgb(-13364461), Color.FromArgb(-11131611), 
                Color.FromArgb(-13881003), Color.FromArgb(-14336721), Color.FromArgb(-13610434), Color.FromArgb(-13208266), Color.FromArgb(-15592942), Color.FromArgb(-14079703), Color.FromArgb(-12763843), 
                Color.FromArgb(-14671840), Color.FromArgb(-16777216), Color.FromArgb(-5263441), Color.FromArgb(-9211021), Color.FromArgb(-13684945), Color.FromArgb(-6250336), Color.FromArgb(-9539986), 
                Color.FromArgb(-7895161), Color.FromArgb(-10526881), Color.FromArgb(-4934476), Color.FromArgb(-1), Color.FromArgb(-8224126), Color.FromArgb(-1568477), Color.FromArgb(-10855846), 
                Color.FromArgb(-10132123), Color.FromArgb(-14803426), Color.FromArgb(-1644826), Color.FromArgb(-15565294), Color.FromArgb(-15430636), Color.FromArgb(-15846898), Color.FromArgb(-986896), 
                Color.FromArgb(-4121572), Color.FromArgb(-2613473), Color.FromArgb(-11790833), Color.FromArgb(-8882056), Color.FromArgb(-7368817), Color.FromArgb(-13882324), Color.FromArgb(-10197916), 
                Color.FromArgb(-7697782), Color.FromArgb(-14474461), Color.FromArgb(-2302756), Color.FromArgb(-6738900), Color.FromArgb(-5428694), Color.FromArgb(-11004144), Color.FromArgb(-9605779), 
                Color.FromArgb(-13158601), Color.FromArgb(-14145496), Color.FromArgb(-15066598), Color.FromArgb(-8947849), Color.FromArgb(-4812528), Color.FromArgb(-8428277), Color.FromArgb(-16776961), 
                Color.FromArgb(-8388608), Color.FromArgb(-13487566), Color.FromArgb(-8355712), Color.FromArgb(-16754272), Color.FromArgb(-15573378), Color.FromArgb(-15438196), Color.FromArgb(-15849422), 
            };

            var indexes = new ushort[]
            {
                0,1,2,3,4,5,6,7,8,9,10,11,12,13,14,15,16,17,18,19,20,21,6,22,11,14,23,24,0,25,6,26,7,26,27,28,29,30,31,32,0,33,34,35,36,37,38,39,40,41,42,43,44,41,42,43,44,45,46,47,
                48,49,50,51,48,52,53,54,44,6,7,0,55,56,57,58,59,60,51,61,55,62,63,55,56,0,48,43,64,15,43,0,53,6,26,27,39,65,6,7,2,0,28,55,66,67,68,62,69,70,71,29,29,72,38,73,74,75,76,
                48,
            };

            return new ColorValues(Theme.Dark, colors, indexes, false);
        }

        static void StyleColors_ValueChanged(object sender, EventArgs e)
        {
            current = GetTheme(Theme.Settings);

            OnColorsChanged();
        }

        static void OnColorsChanged()
        {
            if (ColorsChanged != null)
                ColorsChanged(null, EventArgs.Empty);
        }

        /// <summary>
        /// Returns the current color
        /// </summary>
        public static Color GetColor(Colors c)
        {
            return current.GetColor(c);
        }

        /// <summary>
        /// Changes a color
        /// </summary>
        public static void SetColor(Colors c, Color v)
        {
            if (current.IsReadOnly)
            {
                current = current.Clone();
            }

            current[c] = v;

            OnColorsChanged();
        }

        public static void SetColors(ColorValues colors)
        {
            current = colors;

            OnColorsChanged();
        }

        /// <summary>
        /// Calls RefreshColors on the control, if supported
        /// </summary>
        /// <param name="self">True to include the first control</param>
        /// <param name="recursive">True to include child controls</param>
        public static void Update(Control c, bool self, bool recursive)
        {
            if (self && c is IColors)
            {
                ((IColors)c).RefreshColors();
            }

            if (recursive)
            {
                foreach (Control child in c.Controls)
                {
                    Update(child, true, true);
                }
            }
        }

        /// <summary>
        /// Updates all colors on the control and all controls within
        /// </summary>
        public static void Update(Control c)
        {
            var properties = TypeDescriptor.GetProperties(c);

            foreach (PropertyDescriptor p in properties)
            {
                var a = (UiPropertyColor)p.Attributes[typeof(UiPropertyColor)];
                if (a != null)
                {
                    var n = a.PropertyName;
                    if (n == null)
                        n = p.Name + "Value";
                    var p2 = properties[n];
                    if (p2 != null)
                    {
                        var v = p2.GetValue(c);
                        if (v is UiColors.Colors)
                        {
                            if ((UiColors.Colors)v != UiColors.Colors.Custom)
                            {
                                p.SetValue(c, UiColors.GetColor((UiColors.Colors)v));
                            }
                        }
                    }
                }
            }
        }

        public static bool SupportsAlpha(Colors c)
        {
            switch (c)
            {
                case Colors.AccountBorderLightDefault:
                case Colors.AccountBorderDarkDefault:
                case Colors.AccountBackColorHovered:
                case Colors.AccountBorderLightHovered:
                case Colors.AccountBorderDarkHovered:
                case Colors.AccountForeColorHovered:
                case Colors.AccountBackColorSelected:
                case Colors.AccountBorderLightSelected:
                case Colors.AccountBorderDarkSelected:
                case Colors.AccountForeColorSelected:
                case Colors.AccountBackColorActive:
                case Colors.AccountBorderLightActive:
                case Colors.AccountBorderDarkActive:
                case Colors.AccountForeColorActive:
                case Colors.AccountFocusedHighlight:
                case Colors.AccountFocusedBorder:
                case Colors.AccountActiveHighlight:
                case Colors.AccountActiveBorder:
                case Colors.AccountActionFocusFlash:
                case Colors.AccountActionExitFill:
                case Colors.AccountActionExitFlash:

                    return true;
            }

            return false;
        }

        /// <summary>
        /// Warning: overwrites the input array. Returns the compressed array of colors and indexes.
        /// </summary>
        public static ColorValues Compress(Theme baseTheme, Color[] colors, bool readOnly = false)
        {
            var d = new Dictionary<int, ushort>(colors.Length);
            var indexes = new ushort[colors.Length];
            ushort count = 0;

            for (var i = 0; i < colors.Length; i++)
            {
                var c = colors[i].ToArgb();
                ushort index;

                if (!d.TryGetValue(c, out index))
                {
                    d[c] = index = count++;

                    if (index != i)
                    {
                        colors[index] = colors[i];
                    }
                }

                indexes[i] = index;
            }

            if (count == colors.Length)
            {
                return new ColorValues(baseTheme, colors, null, readOnly);
            }

            Array.Resize<Color>(ref colors, count);

            return new ColorValues(baseTheme, colors, indexes, readOnly);
        }
        
        public static ColorValues Compress(ColorValues cv)
        {
            if (cv.IsCompressed)
                return cv;
            else
                return Compress(cv.BaseTheme, cv.Colors, cv.IsReadOnly);
        }

        public static void Export(string path, ColorValues cv, ColorFormat format = ColorFormat.Hex)
        {
            var colors = cv.Colors;
            var names = new string[colors.Length];
            var indexes = new int[colors.Length];

            for (var i = 0; i < colors.Length; i++)
            {
                names[i] = ((UiColors.Colors)i).ToString();
                indexes[i] = i;
            }

            Array.Sort<string, int>(names, indexes);

            using (var w = new StreamWriter(new BufferedStream(File.Create(path))))
            {
                for (var i = 0; i < names.Length; i++)
                {
                    w.Write(names[i]);
                    w.Write('=');

                    var ci = indexes[i];
                    var c = colors[ci];

                    if (format == ColorFormat.RGB)
                    {
                        if (UiColors.SupportsAlpha((UiColors.Colors)ci))
                        {
                            w.Write(c.A);
                            w.Write(',');
                        }

                        w.Write(c.R);
                        w.Write(',');
                        w.Write(c.G);
                        w.Write(',');
                        w.Write(c.B);
                    }
                    else
                    {
                        var x = c.ToArgb().ToString("x").PadLeft(8);

                        if (!UiColors.SupportsAlpha((UiColors.Colors)ci))
                        {
                            x = x.Substring(2);
                        }

                        w.Write('#');
                        w.Write(x);
                    }

                    w.WriteLine();
                }
            }
        }

        public static IEnumerable<KeyValuePair<Colors,Color>> Import(string path)
        {
            var names = new Dictionary<string, int>();
            var count = current.Count;

            for (var i = 0; i < count; i++)
            {
                names[((UiColors.Colors)i).ToString()] = i;
            }

            using (var r = new StreamReader(new BufferedStream(File.OpenRead(path))))
            {
                while (!r.EndOfStream)
                {
                    var l = r.ReadLine();
                    if (l == null)
                        break;
                    var j = l.IndexOf('=');

                    if (j != -1)
                    {
                        var n = l.Substring(0, j).Trim();
                        var v = l.Substring(j + 1).Trim();

                        if (v.Length == 0)
                            continue;

                        int i;

                        if (!names.TryGetValue(n, out i))
                            continue;

                        Color c;

                        try
                        {
                            //supports [#aarrggbb] [a,r,g,b] [colorname]
                            c = ColorTranslator.FromHtml(v);
                        }
                        catch
                        {
                            continue;
                        }

                        if (c.A == 0 && c.R == 0 && c.G == 0 && c.B == 0)
                            continue;

                        var cn = (Colors)i;

                        if (c.A != 255 && UiColors.SupportsAlpha(cn))
                        {
                            c = Color.FromArgb(255, c);
                        }

                        yield return new KeyValuePair<Colors, Color>(cn, c);
                    }
                }
            }
        }

        /// <summary>
        /// Adds any missing colors
        /// </summary>
        public static ColorValues EnsureColors(ColorValues cv, ushort version = 0)
        {
            if (cv.Count >= COLORS)
                return cv;

            var theme = cv.BaseTheme;

            switch (theme)
            {
                case Theme.Dark:
                case Theme.Light:
                    break;
                default:
                    theme = Theme.Light;
                    break;
            }

            var colors = GetTheme(theme).Decompress();
            var settings = cv.IsCompressed ? cv.Decompress() : cv.Colors;

            Array.Copy(settings, colors, settings.Length);

            if (version <= 13)
            {
                colors[(int)Colors.AccountBackColorActive] = colors[(int)Colors.AccountBackColorDefault];
                colors[(int)Colors.AccountBorderLightActive] = colors[(int)Colors.AccountBorderLightDefault];
                colors[(int)Colors.AccountBorderDarkActive] = colors[(int)Colors.AccountBorderDarkDefault];
            }

            return Compress(theme, colors, cv.IsReadOnly);
        }
    }
}
