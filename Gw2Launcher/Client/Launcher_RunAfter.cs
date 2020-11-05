using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using System.IO;

namespace Gw2Launcher.Client
{
    static partial class Launcher
    {
        private class RunAfterManager : IDisposable
        {
            private class RunAfterProcess
            {
                public Process process;
                public Settings.RunAfter settings;
                public bool started;

                public RunAfterProcess(Settings.RunAfter r)
                {
                    settings = r;
                }

                public void Start(RunAfterManager ra)
                {
                    if (started)
                        return;
                    started = true;

                    try
                    {
                        ProcessStartInfo si;

                        if (settings.Type == Settings.RunAfter.RunAfterType.ShellCommands)
                        {
                            si = new ProcessStartInfo()
                            {
                                FileName = "cmd.exe",
                                RedirectStandardInput = true,
                                UseShellExecute = false,
                                CreateNoWindow = true
                            };
                        }
                        else
                        {
                            si = new ProcessStartInfo()
                            {
                                WorkingDirectory = Path.GetDirectoryName(settings.Path),
                                FileName = settings.Path,
                                Arguments = Variables.Replace(settings.Arguments, ra.GetVariables()),
                                UseShellExecute = false,
                            };
                        }

                        if ((settings.Options & Settings.RunAfter.RunAfterOptions.UseCurrentUser) == 0)
                        {
                            var username = ra.account.Settings.WindowsAccount;

                            if (!Util.Users.IsCurrentUser(username))
                            {
                                var password = Security.Credentials.GetPassword(username);
                                if (password != null)
                                {
                                    si.UserName = username;
                                    si.Password = password;
                                    si.LoadUserProfile = true;
                                }
                            }
                        }

                        process = new Process()
                        {
                            StartInfo = si,
                        };

                        if (process.Start())
                        {
                            if (settings.Type == Settings.RunAfter.RunAfterType.ShellCommands)
                            {
                                using (StreamWriter sw = process.StandardInput)
                                {
                                    if (sw.BaseStream.CanWrite)
                                    {
                                        sw.WriteLine(Variables.Replace(settings.Arguments, ra.GetVariables()));
                                    }
                                }
                            }

                            process.Exited += process_Exited;
                            process.EnableRaisingEvents = true;
                        }
                        else
                        {
                            process.Dispose();
                            process = null;
                        }
                    }
                    catch (Exception e)
                    {
                        Util.Logging.Log(e);
                    }
                }

                public void Close(bool kill)
                {
                    var p = process;

                    if (p != null)
                    {
                        try
                        {
                            if (!kill && p.CloseMainWindow())
                            {
                                return;
                            }

                            p.Kill();
                        }
                        catch { }
                    }
                }

                public void Close()
                {
                    if ((settings.Options & Settings.RunAfter.RunAfterOptions.KillOnExit) != 0)
                    {
                        Close(true);
                    }
                    else if ((settings.Options & Settings.RunAfter.RunAfterOptions.CloseOnExit) != 0)
                    {
                        Close(false);
                    }
                }

                void process_Exited(object sender, EventArgs e)
                {
                    lock (this)
                    {
                        if (process != null)
                        {
                            process.Dispose();
                            process = null;
                        }
                    }
                }
            }

            private Account account;
            private RunAfterProcess[] processes;
            private Variables.DataSource variables;

            private RunAfterManager(Account account, RunAfterProcess[] processes)
            {
                this.account = account;
                this.processes = processes;

                account.Process.Exited += account_Exited;
                try
                {
                    if (account.Process.Process.HasExited)
                        Dispose();
                }
                catch{}
            }

            private Variables.DataSource GetVariables()
            {
                if (variables == null)
                {
                    variables = new Variables.DataSource(account.Settings, account.Process.Process);
                }
                return variables;
            }

            void account_Exited(object sender, Account e)
            {
                Dispose();
            }

            public static RunAfterManager Create(Account account)
            {
                var ra1 = account.Settings.RunAfter;
                Settings.RunAfter[] ra2 = null;

                switch (account.Type)
                {
                    case AccountType.GuildWars2:

                        ra2 = Settings.GuildWars2.RunAfter.Value;

                        break;
                    case AccountType.GuildWars1:

                        ra2 = Settings.GuildWars1.RunAfter.Value;

                        break;
                }

                var count = 0;

                if (ra1 != null)
                    count += ra1.Length;
                if (ra2 != null)
                    count += ra2.Length;

                if (count > 0)
                {
                    var ra = new RunAfterProcess[count];
                    var j = 0;

                    if (ra1 != null)
                    {
                        foreach (var r in ra1)
                        {
                            ra[j++] = new RunAfterProcess(r);
                        }
                    }

                    if (ra2 != null)
                    {
                        foreach (var r in ra2)
                        {
                            ra[j++] = new RunAfterProcess(r);
                        }
                    }

                    return new RunAfterManager(account, ra);
                }

                return null;
            }

            private int GetWaitIndex(Settings.RunAfter.RunAfterOptions o)
            {
                if ((o & Settings.RunAfter.RunAfterOptions.WaitForLauncherLoaded) != 0)
                    return 1;
                
                if ((o & Settings.RunAfter.RunAfterOptions.WaitForDxWindowLoaded) != 0)
                    return 2;

                return 0;
            }

            public void Start(Settings.RunAfter.RunAfterOptions state)
            {
                int count;

                lock(this)
                {
                    if (processes == null)
                        return;
                    count = processes.Length;
                }

                var s1 = GetWaitIndex(state);

                for (var i = 0; i < count; i++)
                {
                    lock (this)
                    {
                        if (processes == null)
                            return;

                        var p = processes[i];

                        if ((p.settings.Options & Settings.RunAfter.RunAfterOptions.Enabled) != 0)
                        {
                            var s2 = GetWaitIndex(p.settings.Options);

                            if (s1 >= s2)
                            {
                                p.Start(this);
                            }
                        }
                    }
                }
            }

            public void Dispose()
            {
                RunAfterProcess[] processes;

                lock (this)
                {
                    processes = this.processes;
                    if (processes == null)
                        return;
                    this.processes = null;
                    account.Process.Exited -= account_Exited;
                }

                foreach (var p in processes)
                {
                    p.Close();
                }
            }
        }
    }
}
