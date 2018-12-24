using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gw2Launcher.Api
{
    public class Json
    {
        private const char ARRAY_START = '[';
        private const char ARRAY_END = ']';
        private const char OBJECT_START = '{';
        private const char OBJECT_END = '}';
        private const char ITEM_SEPERATOR = ',';
        private const char KEY_VALUE_SEPERATOR = ':';
        private const char SPECIAL_CHAR = '\\';
        private const char STRING = '"';
        private const char BOOL_NOT = '!';

        public static T GetValue<T>(Dictionary<string, object> json, string key)
        {
            object o;
            if (json.TryGetValue(key, out o))
            {
                if (o is T)
                {
                    return (T)o;
                }
                else
                {
                    try
                    {
                        return (T)Convert.ChangeType(o, typeof(T));
                    }
                    catch { }
                }
            }

            return default(T);
        }

        public static object Decode(string json)
        {
            int index = 1;

            if (json[0] == OBJECT_START)
                return ParseObject(ref json, ref index);
            else if (json[0] == ARRAY_START)
                return ParseArray(ref json, ref index);

            return null;
        }

        private static Dictionary<string, object> ParseObject(ref string json, ref int index)
        {
            Dictionary<string, object> o = new Dictionary<string, object>();
            bool done = false;

            int objectStart = index - 1;
            int objectEnd;

            while (!done)
            {
                char c = json[index];

                if (c == ITEM_SEPERATOR)
                {
                    index++;
                }
                else if (c == OBJECT_END)
                {
                    index++;
                    break;
                }
                else if (char.IsWhiteSpace(c))
                {
                    index++;
                }
                else
                {
                    KeyValuePair<string, object> kv = ParseItem(ref json, ref index);
                    o.Add(kv.Key, kv.Value);
                }
            }

            objectEnd = index;

            string objT = json.Substring(objectStart, objectEnd - objectStart);

            //ParsedObject obj = new ParsedObject();
            //obj.start = objectStart;
            //obj.length = objectEnd - objectStart;
            //obj.obj = o;

            return o;
        }

        private static KeyValuePair<string, object> ParseItem(ref string json, ref int index)
        {
            object key = ParseValue(ref json, ref index);
            if (json[index] == KEY_VALUE_SEPERATOR)
            {
                index++;

                while (char.IsWhiteSpace(json[index]))
                {
                    index++;
                }

                object value = ParseValue(ref json, ref index);

                return new KeyValuePair<string, object>((string)key, value);
            }

            return new KeyValuePair<string, object>("", "");
        }

        private static object ParseValue(ref string json, ref int index)
        {
            int startIndex = index;
            char o = json[index];

            if (o == ARRAY_START)
            {
                index++;
                return ParseArray(ref json, ref index);
            }
            else if (o == OBJECT_START)
            {
                index++;
                return ParseObject(ref json, ref index);
            }
            else if (o == STRING)
            {
                index++;
                startIndex = index;

                bool done = false;
                StringBuilder sb = new StringBuilder();

                while (!done)
                {
                    char c = json[index++];

                    switch (c)
                    {
                        case STRING:
                            sb.Append(json.Substring(startIndex, index - startIndex - 1));
                            return sb.ToString();
                        case SPECIAL_CHAR:
                            sb.Append(json.Substring(startIndex, index - startIndex - 1));
                            sb.Append(ParseSpecialChar(ref json, ref index));
                            startIndex = index;
                            break;
                    }
                }
            }
            else if (o == BOOL_NOT)
            {
                index++;
                object value = ParseValue(ref json, ref index);

                if (value == null)
                    return true;
                else if (value is bool)
                    return !(bool)value;
                else if (value is int)
                    return (int)value == 0;
                else if (value is string)
                    return (string)value == "";

                return false;
            }
            else if (char.IsLetter(o))
            {
                startIndex = index;

                bool done = false;

                while (!done)
                {
                    char c = json[index++ + 1];
                    done = !char.IsLetter(c);
                }

                string value = json.Substring(startIndex, index - startIndex);
                switch (value)
                {
                    case "true":
                        return true;
                    case "false":
                        return false;
                    case "null":
                        return null;
                    case "function":
                        string f = ParseFunction(ref json, ref index);
                        return f;
                    default:
                        return value;
                }
            }
            /*
        else if (o == 't')
        {
            index += 4;
            return true;
        }
        else if (o == 'f')
        {
            index += 5;
            return false;
        }
        else if (o == '\n')
        {
            index++;
            return '\n';
        }
        */
            else
            {
                bool done = false;
                StringBuilder sb = new StringBuilder();
                bool isDecimal = false;

                while (!done)
                {
                    char c = json[1 + index++];

                    if (c == '.')
                    {
                        isDecimal = true;
                    }
                    else if (c == '-')
                    {
                    }
                    else if ((c >= '0' && c <= '9'))
                    {
                    }
                    else
                    {
                        string value = json.Substring(startIndex, index - startIndex);
                        if (isDecimal)
                            return Double.Parse(value);
                        else
                            return Int32.Parse(value);
                    }
                }
            }

            return null;
        }

        private static string ParseFunction(ref string json, ref int index)
        {
            int startIndex = index;
            int depth = 0;
            bool initial = false;
            string inString = null;
            bool ignoreNext = false;

            while (!initial || depth > 0)
            {
                char c = json[index++];

                if (!ignoreNext)
                {
                    switch (c)
                    {
                        case '{':
                            initial = true;
                            depth++;
                            break;
                        case '}':
                            depth--;
                            break;
                        case '\\':
                            ignoreNext = true;
                            break;
                        case '"':
                        case '\'':
                            if (inString != null)
                            {
                                if (inString[0] == c)
                                    inString = null;
                            }
                            else
                                inString = c.ToString();
                            break;
                    }
                }
            }

            return json.Substring(startIndex, index - startIndex);
        }

        private static string ParseSpecialChar(ref string json, ref int index)
        {
            char c = json[index++];

            switch (c)
            {
                case '\\':
                    return "\\";
                case '"':
                    return "\"";
                case '\t':
                    return "\t";
                case '\n':
                    return "\n";
                case '/':
                    return "/";
            }

            return c.ToString();
        }

        private static List<object> ParseArray(ref string json, ref int index)
        {
            List<object> objects = new List<object>();
            bool done = false;
            int startIndex = index;

            while (!done)
            {
                char c = json[index];
                if (c == ARRAY_END)
                {
                    index++;
                    return objects;
                }
                else if (c == ITEM_SEPERATOR)
                {
                    index++;
                }
                else if (char.IsWhiteSpace(c))
                {
                    index++;
                }
                else
                {
                    object v = ParseValue(ref json, ref index);
                    objects.Add(v);
                }
            }

            return objects;
        }
    }
}
