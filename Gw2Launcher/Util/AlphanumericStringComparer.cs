using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gw2Launcher.Util
{
    class AlphanumericStringComparer : IComparer<string>
    {
        private class ValuePart : IComparable<ValuePart>
        {
            private enum PartType
            {
                Text,
                Number,
                Space
            }

            private PartType type;
            private object value;
            private int length;

            public ValuePart(string text, int length)
            {
                this.type = PartType.Text;
                this.value = text;
                this.length = length;
            }

            public ValuePart(int length)
            {
                this.type = PartType.Space;
                this.length = length;
            }

            public ValuePart(int num, int length)
            {
                this.type = PartType.Number;
                this.value = num;
                this.length = length;
            }

            public static List<ValuePart> From(string text)
            {
                var parts = new List<ValuePart>();
                var type = PartType.Text;
                int i = 0, l = text.Length, j = 0;

                for (; i < l; i++)
                {
                    PartType _type;
                    var c = text[i];
                    if (c == ' ')
                        _type = PartType.Space;
                    else if (char.IsDigit(c))
                        _type = PartType.Number;
                    else
                        _type = PartType.Text;

                    if (_type != type)
                    {
                        if (i != j)
                        {
                            var k = i - j;
                            var s = text.Substring(j, k);

                            switch (type)
                            {
                                case PartType.Number:
                                    parts.Add(new ValuePart(int.Parse(s), k));
                                    break;
                                case PartType.Space:
                                    parts.Add(new ValuePart(k));
                                    break;
                                case PartType.Text:
                                    parts.Add(new ValuePart(s, k));
                                    break;
                            }
                        }

                        j = i;
                        type = _type;
                    }
                }

                if (i != j)
                {
                    var k = i - j;
                    var s = text.Substring(j, k);

                    switch (type)
                    {
                        case PartType.Number:
                            parts.Add(new ValuePart(int.Parse(s), k));
                            break;
                        case PartType.Space:
                            parts.Add(new ValuePart(k));
                            break;
                        case PartType.Text:
                            parts.Add(new ValuePart(s, k));
                            break;
                    }
                }

                return parts;
            }

            public int CompareTo(ValuePart other)
            {
                //whitespace < number < text

                if (this.type == other.type)
                {
                    if (this.type == PartType.Number)
                    {
                        var c = ((int)this.value).CompareTo((int)other.value);
                        if (c == 0)
                        {
                            return -this.length.CompareTo(other.length);
                        }
                        return c;
                    }
                    else if (this.type == PartType.Space)
                    {
                        return -this.length.CompareTo(other.length);
                    }
                    else
                    {
                        return ((string)this.value).CompareTo((string)other.value);
                    }
                }
                else if (this.type == PartType.Number)
                {
                    if (other.type == PartType.Space)
                        return 1;

                    return -1;
                }
                else if (this.type == PartType.Space)
                {
                    return -1;
                }
                else
                {
                    return 1;
                }
            }
        }

        private class TextValue : IComparable<TextValue>
        {
            public string value;
            public List<ValuePart> parts;

            public TextValue(string value)
            {
                this.value = value;
                this.parts = ValuePart.From(value);
            }

            public int CompareTo(TextValue other)
            {
                if (object.ReferenceEquals(this, other))
                    return 0;

                var l1 = this.parts.Count;
                var l2 = other.parts.Count;
                int l;
                if (l1 < l2)
                    l = l1;
                else
                    l = l2;

                for (var i = 0; i < l; i++)
                {
                    var c = this.parts[i].CompareTo(other.parts[i]);
                    if (c != 0)
                        return c;
                }

                return l1.CompareTo(l2);
            }
        }

        private Dictionary<string, TextValue> values;

        public AlphanumericStringComparer()
        {
            values = new Dictionary<string, TextValue>();
        }

        private TextValue GetValue(string text)
        {
            TextValue value;
            if (!values.TryGetValue(text, out value))
                values[text] = value = new TextValue(text);
            return value;
        }

        public int Compare(string a, string b)
        {
            return GetValue(a).CompareTo(GetValue(b));
        }

        public void Clear()
        {
            values.Clear();
        }
    }
}
