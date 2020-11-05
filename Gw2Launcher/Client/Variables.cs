using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;

namespace Gw2Launcher.Client
{
    public class Variables
    {
        public enum VariableType
        {
            /// <summary>
            /// Account information
            /// </summary>
            Account,
            /// <summary>
            /// Process information; only available after the process has started
            /// </summary>
            Process,
        }

        public class DataSource
        {
            public DataSource(Settings.IAccount account, Process process)
            {
                this.Account = account;
                this.Process = process;
            }

            public Settings.IAccount Account
            {
                get;
                private set;
            }

            public Process Process
            {
                get;
                private set;
            }
        }

        public class Variable
        {
            private Func<DataSource, object> getValue;

            public Variable(VariableType type, string name, string description, Func<DataSource, object> getValue)
            {
                this.Type = type;
                this.Name = name;
                this.Description = description;
                this.getValue = getValue;
            }

            public VariableType Type
            {
                get;
                private set;
            }

            public string Name
            {
                get;
                private set;
            }

            public string Description
            {
                get;
                private set;
            }

            public object GetValue(DataSource v)
            {
                try
                {
                    return getValue(v);
                }
                catch
                {
                    return null;
                }
            }

            public string GetStringValue(DataSource v)
            {
                try
                {
                    return getValue(v).ToString();
                }
                catch
                {
                    return null;
                }
            }
        }

        private Dictionary<string, Variable> variables;

        private static Variables instance;
        
        private Variables()
        {
            this.variables = new Dictionary<string, Variable>();

            Add(new Variable(VariableType.Account, "%accountid%", "ID of the account",
                delegate(DataSource v)
                {
                    return v.Account.UID;
                }));
            Add(new Variable(VariableType.Account, "%accountname%", "Name of the account",
                delegate(DataSource v)
                {
                    return v.Account.Name;
                }));
            Add(new Variable(VariableType.Process, "%handle%", "Window handle",
                delegate(DataSource v)
                {
                    try
                    {
                        return Windows.FindWindow.FindMainWindow(v.Process);
                    }
                    catch
                    {
                        return 0;
                    }
                }));
            Add(new Variable(VariableType.Process, "%processid%", "ID of the process",
                delegate(DataSource v)
                {
                    try
                    {
                        return v.Process.Id;
                    }
                    catch
                    {
                        return 0;
                    }
                }));
        }

        private void Add(Variable v)
        {
            variables.Add(v.Name, v);
        }

        private static Variables GetInstance()
        {
            if (instance == null)
                instance = new Variables();
            return instance;
        }

        public static string Replace(string text, DataSource data)
        {
            if (text == null)
                return "";

            var i = text.IndexOf('%');

            if (i != -1)
            {
                StringBuilder sb = null;
                var previous = 0;
                var variables = GetInstance().variables;

                do
                {
                    var j = text.IndexOf('%', i + 1);

                    if (j != -1)
                    {
                        Variable v;
                        var l = text.IndexOf(' ', i + 1, j - i - 1);

                        if (l == -1 && variables.TryGetValue(text.Substring(i, j - i + 1), out v))
                        {
                            if (sb == null)
                                sb = new StringBuilder(text.Length * 3 / 2);

                            sb.Append(text, previous, i - previous);

                            try
                            {
                                sb.Append(v.GetStringValue(data));
                            }
                            catch (Exception e)
                            {
                                Util.Logging.Log(new Exception("Unable to get value for " + text.Substring(i, j - i + 1), e));
                            }

                            previous = j + 1;

                            i = text.IndexOf('%', j + 1);
                        }
                        else
                        {
                            i = j;
                        }
                    }
                    else
                    {
                        break;
                    }
                }
                while (i != -1);

                if (sb == null)
                {
                    return text;
                }
                else if (previous < text.Length)
                {
                    sb.Append(text, previous, text.Length - previous);
                }

                return sb.ToString();
            }

            return text;
        }

        public static IEnumerable<Variable> GetVariables(params VariableType[] type)
        {
            var evalues = Enum.GetValues(typeof(VariableType));
            var b = new bool[evalues.Length];

            foreach (var t in type)
            {
                b[(int)t] = true;
            }

            foreach (var v in GetInstance().variables.Values)
            {
                if (b[(int)v.Type])
                {
                    yield return v;
                }
            }
        }
    }
}
