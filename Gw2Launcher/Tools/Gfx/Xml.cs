using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.IO;

namespace Gw2Launcher.Tools.Gfx
{
    class Xml
    {
        private XmlDocument xml;
        private string path;
        private List<Option> options;
        private bool modified;

        private Xml(XmlDocument xml, string path, XmlNode node)
        {
            this.xml = xml;
            this.path = path;

            var options = this.options = new List<Option>();

            foreach (XmlNode n in node.ChildNodes)
            {
                if (n.NodeType == XmlNodeType.Element)
                {
                    if (n.Name.Equals(Strings.Tag.Option, StringComparison.OrdinalIgnoreCase))
                    {
                        var o = Option.From(n);
                        options.Add(o);

                        o.Modified += option_Modified;
                    }
                    else if (n.Name.Equals(Strings.Tag.Resolution, StringComparison.OrdinalIgnoreCase))
                    {
                        var o = new Option.Resolution(n);
                        options.Add(o);

                        o.Modified += option_Modified;
                    }
                }
            }
        }

        public class DisplayName
        {
            private Dictionary<string, string> names;
            private StringBuilder buffer;

            public DisplayName()
            {
                var d = names = new Dictionary<string, string>();
                buffer = new StringBuilder();

                d["frameLimit"] = "Frame Limiter";
                d["charModelLimit"] = "Character Model Limit";
                d["screenMode"] = "Resolution";
                d["antiAliasing"] = "Antialiasing";
                d["textureDetail"] = "Textures";
                d["charModelQuality"] = "Character Model Quality";
                d["lodDistance"] = "LOD Distance";
                d["postProc"] = "Postprocessing";
                d["sampling"] = "Render Sampling";
                d["gamma"] = "Full-Screen Gamma";
                d["effectLod"] = "Effect LOD";
                d["highResCharacter"] = "High-Res Character Textures";
                d["bestTextureFiltering"] = "Best Texture Filtering";
                d["verticalSync"] = "Vertical Sync";
                d["dpiScaling"] = "DPI Scaling";
                d["fxaa"] = "FXAA";
                d["smaa"] = "SMAA";
            }

            private void AppendWord(string word)
            {
                string n;
                if (names.TryGetValue(word, out n))
                    buffer.Append(n);
                else
                {
                    buffer.Append(char.ToUpper(word[0]));
                    if (word.Length > 1)
                        buffer.Append(word.Substring(1));
                }
            }

            public string GetName(string text)
            {
                string name;

                if (!names.TryGetValue(text, out name))
                {
                    var l = text.Length;
                    var last = 0;

                    var sb = buffer;
                    sb.Length = 0;

                    for (var i = 1; i < l; i++)
                    {
                        if (char.IsUpper(text[i]))
                        {
                            if (i + 1 < l && !char.IsUpper(text[i + 1]))
                            {
                                AppendWord(text.Substring(last, i - last));
                                sb.Append(' ');
                                last = i;
                            }
                        }
                        else if (text[i] == '_')
                        {
                            if (i > last)
                            {
                                AppendWord(text.Substring(last, i - last));
                                sb.Append(' ');
                            }
                            last = i + 1;
                        }
                    }

                    if (l > last)
                        AppendWord(text.Substring(last, l - last));

                    name = sb.ToString();
                }

                return name;
            }
        }

        private static class Strings
        {
            public static class Tag
            {
                public const string
                    GameSettings = "GAMESETTINGS",
                    Option = "OPTION",
                    Resolution = "RESOLUTION",
                    Enum = "ENUM",
                    Range = "RANGE";
            }

            public static class Attribute
            {
                public const string
                    Name = "Name",
                    Value = "Value",
                    Type = "Type",
                    ValueType = "ValueType",
                    EnumValue = "EnumValue",
                    MinValue = "MinValue",
                    MaxValue = "MaxValue",
                    NumSteps = "NumSteps",
                    Width = "Width",
                    Height = "Height",
                    RefreshRate = "RefreshRate";
            }

            public static class ValueType
            {
                public const string
                    Float = "Float",
                    Int = "Int",
                    Bool = "Bool",
                    Enum = "Enum";
            }
        }

        public class Option
        {
            public enum OptionType
            {
                Enum,
                Float,
                Integer,
                Boolean,
                String,

                Resolution
            }

            public class Enum : String
            {
                protected List<string> values;

                public Enum(XmlNode source)
                    : base(source)
                {
                    base.type = OptionType.Enum;

                    var values = this.values = new List<string>();

                    foreach (XmlNode n in source.ChildNodes)
                    {
                        if (n.NodeType == XmlNodeType.Element)
                        {
                            if (n.Name.Equals(Strings.Tag.Enum, StringComparison.OrdinalIgnoreCase))
                            {
                                var v = GetValue(n.Attributes[Strings.Attribute.EnumValue]);
                                if (!string.IsNullOrEmpty(v))
                                    values.Add(v);
                            }
                        }
                    }
                }

                public List<string> EnumValues
                {
                    get
                    {
                        return this.values;
                    }
                }
            }

            public class Float : OptionValue
            {
                protected float minValue, maxValue, numSteps, value;

                public Float(XmlNode source)
                    : base(OptionType.Float, source)
                {
                    this.value = GetValueAsFloat(source.Attributes[Strings.Attribute.Value]);

                    foreach (XmlNode n in source.ChildNodes)
                    {
                        if (n.NodeType == XmlNodeType.Element)
                        {
                            if (n.Name.Equals(Strings.Tag.Range, StringComparison.OrdinalIgnoreCase))
                            {
                                minValue = GetValueAsFloat(n.Attributes[Strings.Attribute.MinValue]);
                                maxValue = GetValueAsFloat(n.Attributes[Strings.Attribute.MaxValue]);
                                numSteps = GetValueAsFloat(n.Attributes[Strings.Attribute.NumSteps]);

                                break;
                            }
                        }
                    }
                }

                internal static float GetValueAsFloat(XmlAttribute a)
                {
                    var v = GetValue(a);
                    if (v != null)
                    {
                        float f;
                        float.TryParse(v, out f);
                        return f;
                    }
                    return 0;
                }

                public float MinValue
                {
                    get
                    {
                        return minValue;
                    }
                }

                public float MaxValue
                {
                    get
                    {
                        return maxValue;
                    }
                }

                public float NumSteps
                {
                    get
                    {
                        return numSteps;
                    }
                }

                public float Value
                {
                    get
                    {
                        return value;
                    }
                    set
                    {
                        base.ValueString = string.Format("{0:0.##}", value);
                    }
                }
            }

            public class Integer : OptionValue
            {
                protected int minValue, maxValue, numSteps, value;

                public Integer(XmlNode source)
                    : base(OptionType.Integer, source)
                {
                    this.value = GetValueAsInteger(source.Attributes[Strings.Attribute.Value]);

                    foreach (XmlNode n in source.ChildNodes)
                    {
                        if (n.NodeType == XmlNodeType.Element)
                        {
                            if (n.Name.Equals(Strings.Tag.Range, StringComparison.OrdinalIgnoreCase))
                            {
                                minValue = GetValueAsInteger(n.Attributes[Strings.Attribute.MinValue]);
                                maxValue = GetValueAsInteger(n.Attributes[Strings.Attribute.MaxValue]);
                                numSteps = GetValueAsInteger(n.Attributes[Strings.Attribute.NumSteps]);

                                break;
                            }
                        }
                    }
                }

                internal static int GetValueAsInteger(XmlAttribute a)
                {
                    var v = GetValue(a);
                    if (v != null)
                    {
                        int i;
                        int.TryParse(v, out i);
                        return i;
                    }
                    return 0;
                }

                public int MinValue
                {
                    get
                    {
                        return minValue;
                    }
                }

                public int MaxValue
                {
                    get
                    {
                        return maxValue;
                    }
                }

                public int NumSteps
                {
                    get
                    {
                        return numSteps;
                    }
                }

                public int Value
                {
                    get
                    {
                        return value;
                    }
                    set
                    {
                        base.ValueString = value.ToString();
                    }
                }
            }

            public class Boolean : OptionValue
            {
                protected bool value;

                public Boolean(XmlNode source)
                    : base(OptionType.Boolean, source)
                {
                    this.value = GetValueAsBoolean(source.Attributes[Strings.Attribute.Value]);
                }

                internal static bool GetValueAsBoolean(XmlAttribute a)
                {
                    var v = GetValue(a);
                    if (v != null)
                    {
                        bool b;
                        bool.TryParse(v, out b);
                        return b;
                    }
                    return false;
                }

                public bool Value
                {
                    get
                    {
                        return value;
                    }
                    set
                    {
                        base.ValueString = value.ToString();
                    }
                }
            }

            public class Resolution : Option
            {
                protected int width, height, refreshRate;

                public Resolution(XmlNode source)
                    : base(OptionType.Resolution, source)
                {
                    this.width = Integer.GetValueAsInteger(source.Attributes[Strings.Attribute.Width]);
                    this.height = Integer.GetValueAsInteger(source.Attributes[Strings.Attribute.Height]);
                    this.refreshRate = Integer.GetValueAsInteger(source.Attributes[Strings.Attribute.RefreshRate]);
                }

                public int Width
                {
                    get
                    {
                        return width;
                    }
                    set
                    {
                        if (width != value)
                        {
                            width = value;
                            SetValue(Strings.Attribute.Width, value.ToString());

                            OnModified();
                        }
                    }
                }

                public int Height
                {
                    get
                    {
                        return height;
                    }
                    set
                    {
                        if (height != value)
                        {
                            height = value;
                            SetValue(Strings.Attribute.Height, value.ToString());

                            OnModified();
                        }
                    }
                }

                public int RefreshRate
                {
                    get
                    {
                        return refreshRate;
                    }
                    set
                    {
                        if (refreshRate != value)
                        {
                            refreshRate = value;
                            SetValue(Strings.Attribute.RefreshRate, value.ToString());

                            OnModified();
                        }
                    }
                }
            }

            public class String : OptionValue
            {
                public String(XmlNode source)
                    : base(OptionType.String, source)
                {
                }

                public string Value
                {
                    get
                    {
                        return base.ValueString;
                    }
                    set
                    {
                        base.ValueString = value;
                    }
                }
            }

            public abstract class OptionValue : Option
            {
                protected string valueString;

                public OptionValue(OptionType type, XmlNode source)
                    : base(type, source)
                {
                    valueString = GetValue(Strings.Attribute.Value);
                }

                public string Name
                {
                    get
                    {
                        return GetValue(Strings.Attribute.Name);
                    }
                }

                public string ValueType
                {
                    get
                    {
                        return GetValue(Strings.Attribute.Type);
                    }
                }

                protected string ValueString
                {
                    get
                    {
                        return valueString;
                    }
                    set
                    {
                        if (!valueString.Equals(value, StringComparison.Ordinal))
                        {
                            valueString = value;
                            SetValue(Strings.Attribute.Value, value);

                            OnModified();
                        }
                    }
                }
            }

            public event EventHandler Modified;

            protected XmlNode source;
            protected OptionType type;
            private bool modified;

            protected Option(OptionType type, XmlNode source)
            {
                this.source = source;
                this.type = type;
            }

            protected void OnModified()
            {
                if (!modified)
                {
                    modified = true;

                    if (Modified != null)
                        Modified(this, EventArgs.Empty);
                }
            }

            public static Option From(XmlNode source)
            {
                var a = source.Attributes[Strings.Attribute.Type];
                if (a != null)
                {
                    var t = a.Value;
                    if (t.Equals(Strings.ValueType.Enum, StringComparison.OrdinalIgnoreCase))
                        return new Option.Enum(source);
                    if (t.Equals(Strings.ValueType.Float, StringComparison.OrdinalIgnoreCase))
                        return new Option.Float(source);
                    if (t.Equals(Strings.ValueType.Bool, StringComparison.OrdinalIgnoreCase))
                        return new Option.Boolean(source);
                    if (t.Equals(Strings.ValueType.Int, StringComparison.OrdinalIgnoreCase))
                        return new Option.Integer(source);
                }

                return new Option.String(source);
            }

            public OptionType Type
            {
                get
                {
                    return type;
                }
            }

            public bool IsModified
            {
                get
                {
                    return modified;
                }
            }

            protected static void SetValue(XmlAttribute a, string value)
            {
                if (a != null)
                    a.Value = value;
            }

            protected void SetValue(string name, string value)
            {
                SetValue(source.Attributes[Strings.Attribute.Value], value);
            }

            protected static string GetValue(XmlAttribute a)
            {
                if (a == null)
                    return null;
                return a.Value;
            }

            protected string GetValue(string name)
            {
                return GetValue(source.Attributes[name]);
            }
        }

        public static Xml Load(string path)
        {
            XmlDocument xml = new XmlDocument()
            {
                PreserveWhitespace = true,
            };

            xml.Load(path);

            var document = xml.DocumentElement;
            if (document == null)
                return null;

            var node = document.SelectSingleNode(Strings.Tag.GameSettings);
            if (node == null)
                return null;

            return new Xml(xml, path, node);
        }

        private void option_Modified(object sender, EventArgs e)
        {
            modified = true;
        }

        public bool IsModified
        {
            get
            {
                return modified;
            }
        }

        public string Path
        {
            get
            {
                return path;
            }
        }

        public List<Option> Options
        {
            get
            {
                return options;
            }
        }

        public void SaveTo(string path)
        {
            //note, should be saved to temp path to prevent overwriting when game is running
            using (var stream = File.Open(path, FileMode.OpenOrCreate, FileAccess.Write, FileShare.Read))
            {
                xml.Save(stream);

                if (stream.Position < stream.Length)
                    stream.SetLength(stream.Position);
            }
        }
    }
}
