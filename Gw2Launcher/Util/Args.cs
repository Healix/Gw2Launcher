using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gw2Launcher.Util
{
    static class Args
    {
        /// <summary>
        /// If found, returns the value of the key, otherwise null
        /// </summary>
        /// <param name="key">The name of the key without the switch character. For example, -key would be "key"</param>
        public static string GetValue(string args, string key)
        {
            int from, to;
            return GetValue(args, key, out from, out to);
        }

        private static string GetValue(string args, string key, out int from, out int to)
        {
            int i = -1;
            int l = args.Length;
            int kl = key.Length;
            from = to = -1;

            //formats: 
            //"-key value"
            //"/key=value"

            do
            {
                i = args.IndexOf(key, i + 1, StringComparison.OrdinalIgnoreCase);
                if (i == -1)
                    return null;
                if (i > 0)
                {
                    char c = args[i - 1];
                    if (c == '-' || c == '/')
                    {
                        if (i > 1 && args[i - 2] != ' ')
                            continue;

                        if (i + kl < l)
                        {
                            c = args[i + kl];
                            if (c == ' ' || c == '=')
                            {
                                from = i - 1;
                                break;
                            }
                        }
                        else
                        {
                            from = i - 1;
                            to = l;
                            return "";
                        }
                    }
                }
            }
            while (true);

            bool quoted = false;
            i += kl + 1;

            if (args[i-1] == ' ')
            {
                switch (args[i])
                {
                    case '-':
                    case '/':
                        return args.Substring(from + 2 + kl, i - from - 2 - kl);
                }
            }

            for (; i < l; i++)
            {
                char c = args[i];

                switch (c)
                {
                    case '"':
                        quoted = !quoted;
                        break;
                    case ' ':
                        if (!quoted)
                        {
                            to = i;
                            return args.Substring(from + 2 + kl, i - from - 2 - kl);
                        }
                        break;
                    case '\\':
                        i++;
                        break;
                }
            }

            to = i;

            return args.Substring(from + 2 + kl);
        }

        /// <summary>
        /// Replaces the key and its value, or adds it
        /// </summary>
        public static string AddOrReplace(string args, string key, string keyAndValue)
        {
            int from, to;
            GetValue(args, key, out from, out to);

            if (from == -1)
            {
                if (keyAndValue.Length > 0)
                    return args + (args.Length > 0 ? " " + keyAndValue : keyAndValue);
                
                return args;
            }

            if (keyAndValue.Length == 0)
            {
                if (from > 0)
                    from--;
                else if (to < args.Length)
                    to++;
            }

            string a = args.Substring(0, from);
            string b = args.Substring(to);

            if (keyAndValue.Length == 0)
                return a + b;

            return a + (a.Length > 0 && a[a.Length - 1] != ' ' ? " " + keyAndValue : keyAndValue) + (b.Length > 0 && b[0] != ' ' ? " " : "") + b;
        }
    }
}
