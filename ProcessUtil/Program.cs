using System;
using System.IO;
using System.Diagnostics;
using System.IO.Pipes;
using System.Security.AccessControl;

namespace ProcessUtil
{
    class Program
    {
        static int Main(string[] args)
        {
#if DEBUG
            if (!Debugger.IsAttached)
                Debugger.Launch();
#endif

            if (args.Length == 0)
                return 0;

            if (args[0] == "-handle")
            {
                #region -handle [-pid|-p|-n] [processId|"path to exe"|"process name"] "objectName" [exactMatch (0|1)]

                if (args.Length != 5 || args[1] != "-pid" && args[1] != "-p" && args[1] != "-n")
                    return new ArgumentException().HResult;

                bool isId = args[1] == "-pid";
                bool isName = args[1] == "-n";
                string path = args[2];
                string name = args[3];
                bool exactMatch = args[4] == "1";

                Process gw2 = null;

                if (!isId)
                {
                    FileInfo fi;
                    if (isName)
                        fi = null;
                    else
                    {
                        try
                        {
                            fi = new FileInfo(path);
                            if (!fi.Exists)
                                return new FileNotFoundException().HResult;
                        }
                        catch (Exception e)
                        {
                            return e.HResult;
                        }
                    }

                    Process[] ps = Process.GetProcessesByName(isName ? path : fi.Name.Substring(0, fi.Name.Length - fi.Extension.Length));
                    DateTime newest = DateTime.MinValue;

                    foreach (Process p in ps)
                    {
                        try
                        {
                            if (!p.HasExited && (isName || string.Equals(p.MainModule.FileName, fi.FullName, StringComparison.OrdinalIgnoreCase)))
                            {
                                if (p.StartTime > newest)
                                    gw2 = p;
                            }
                        }
                        catch (Exception e)
                        {
                            Console.WriteLine(e.Message);
                        }
                    }
                }
                else
                {
                    try
                    {
                        gw2 = Process.GetProcessById(Int32.Parse(path));
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e.Message);
                    }
                }

                if (gw2 != null)
                {
                    try
                    {
                        Win32Handles.IObjectHandle handle = Win32Handles.GetHandle(gw2.Id, name, exactMatch);
                        if (handle != null)
                        {
                            handle.Kill();
                            return 0;
                        }
                        return new FileNotFoundException().HResult;
                    }
                    catch (Exception e)
                    {
                        return e.HResult;
                    }
                }
                else
                {
                    return new FileNotFoundException().HResult;
                }

                #endregion
            }
            else if (args[0] == "-user")
            {
                #region -user -u "username" -p "password"

                if (args.Length != 5 || args[1] != "-u" || args[3] != "-p")
                    return new ArgumentException().HResult;

                string username = args[2];
                string password = args[4];

                try
                {
                    ProcessStartInfo p = new ProcessStartInfo("net");
                    //p.RedirectStandardInput = true;
                    //p.RedirectStandardOutput = true;
                    p.UseShellExecute = false;
                    p.CreateNoWindow = true;

                    p.Arguments = "user /add \"" + username + "\" \"" + password + "\"";

                    int exitCode;

                    using (Process cmd = Process.Start(p))
                    {
                        //StreamReader reader = cmd.StandardOutput;

                        //while (!reader.EndOfStream)
                        //{
                        //    string line = reader.ReadLine().ToLower();
                        //}

                        cmd.WaitForExit();

                        exitCode = cmd.ExitCode;
                    }

                    //p.Arguments = "localgroup Administrators /add \"" + username + "\"";

                    //using (Process cmd = Process.Start(p))
                    //{
                    //    StreamReader reader = cmd.StandardOutput;

                    //    while (!reader.EndOfStream)
                    //    {
                    //        string line = reader.ReadLine().ToLower();
                    //    }
                    //}

                    p = new ProcessStartInfo("wmic");
                    //p.RedirectStandardInput = true;
                    //p.RedirectStandardOutput = true;
                    p.UseShellExecute = false;
                    p.CreateNoWindow = true;

                    p.Arguments = "USERACCOUNT WHERE Name='" + username + "' SET PasswordExpires=FALSE";

                    using (Process cmd = Process.Start(p))
                    {
                        //StreamReader reader = cmd.StandardOutput;

                        //while (!reader.EndOfStream)
                        //{
                        //    string line = reader.ReadLine().ToLower();
                        //}

                        cmd.WaitForExit();
                    }

                    return exitCode;
                }
                catch (Exception e)
                {
                    return e.HResult;
                }

                #endregion
            }
            else if (args[0] == "-delgw2cache")
            {
                #region -delgw2cache "path1" "path2" "..."

                for (int i = 1; i < args.Length; i++)
                {
                    if (Directory.Exists(args[i]))
                    {
                        try
                        {
                            foreach (var d in new DirectoryInfo(args[i]).GetDirectories("gw2cache-{*}", SearchOption.TopDirectoryOnly))
                            {
                                try
                                {
                                    d.Delete(true);
                                }
                                catch { }
                            }
                        }
                        catch { }
                    }
                }

                #endregion
            }
            else if (args[0] == "-task")
            {
                #region -task [-create "name1" "xml1" "name2" "xml2" "..."]|[-run "name"]|[-delete "name1" "name2" "..."]

                if (args.Length < 3)
                    return new ArgumentException().HResult;

                if (args[1] == "-create")
                {
                    if (args.Length < 4)
                        return new ArgumentException().HResult;

                    int exitCode = 0;

                    for (int i = 2; i < args.Length; i+=2 )
                    {
                        try
                        {
                            ProcessStartInfo p = new ProcessStartInfo("schtasks");
                            p.UseShellExecute = false;
                            p.CreateNoWindow = true;

                            p.Arguments = "/delete /F /tn \"" + args[i] + "\"";

                            using (Process cmd = Process.Start(p))
                            {
                                cmd.WaitForExit();
                            }
                        }
                        catch { }

                        try
                        {
                            ProcessStartInfo p = new ProcessStartInfo("schtasks");
                            p.UseShellExecute = false;
                            p.CreateNoWindow = true;

                            p.Arguments = "/create /tn \"" + args[i] + "\" /xml \"" + args[i + 1] + "\"";

                            using (Process cmd = Process.Start(p))
                            {
                                cmd.WaitForExit();
                                exitCode = cmd.ExitCode;
                            }
                        }
                        catch (Exception e)
                        {
                            return e.HResult;
                        }
                    }
                    return exitCode;
                }
                else if (args[1] == "-delete")
                {
                    if (args.Length < 3)
                        return new ArgumentException().HResult;

                    int exitCode = 0;

                    for (int i = 2; i < args.Length; i++)
                    {
                        try
                        {
                            ProcessStartInfo p = new ProcessStartInfo("schtasks");
                            p.UseShellExecute = false;
                            p.CreateNoWindow = true;

                            p.Arguments = "/delete /F /tn \"" + args[i] + "\"";

                            using (Process cmd = Process.Start(p))
                            {
                                cmd.WaitForExit();
                                exitCode = cmd.ExitCode;
                            }
                        }
                        catch { }
                    }

                    return exitCode;
                }
                else if (args[1] == "-run")
                {
                    if (args.Length < 3)
                        return new ArgumentException().HResult;

                    try
                    {
                        ProcessStartInfo p = new ProcessStartInfo("schtasks");
                        p.UseShellExecute = false;
                        p.CreateNoWindow = true;

                        p.Arguments = "/run /tn \"" + args[2] + "\"";

                        using (Process cmd = Process.Start(p))
                        {
                            cmd.WaitForExit();
                            return cmd.ExitCode;
                        }
                    }
                    catch (Exception e) 
                    {
                        return e.HResult;
                    }
                }

                #endregion
            }
            else if (args[0] == "-users")
            {
                #region -users -activate:yes|no "name1" "name2" "..."
                
                if (args.Length < 3)
                    return new ArgumentException().HResult;

                if (args[1].StartsWith("-activate:"))
                {
                    string _args = "user /active:" + (args[1] == "-activate:yes" ? "yes " : "no ");

                    int exitCode = 0;

                    for (int i = 2; i < args.Length; i++)
                    {
                        try
                        {
                            ProcessStartInfo p = new ProcessStartInfo("net");
                            p.UseShellExecute = false;
                            p.CreateNoWindow = true;

                            p.Arguments = _args + "\"" + args[i] + "\"";

                            using (Process cmd = Process.Start(p))
                            {
                                cmd.WaitForExit();
                                exitCode = cmd.ExitCode;
                            }
                        }
                        catch { }
                    }

                    return exitCode;
                }

                #endregion
            }

            return 0;
        }
    }
}
