using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;
using System.IO;
using System.IO.MemoryMappedFiles;
using System.Diagnostics;

namespace Gw2Launcher.Client
{
    static class ProxyLauncher
    {
        public enum LaunchResult
        {
            None = 0,
            Success = 1,
            Failed = 2
        }

        public class ProxyOptions
        {
            private Launcher.ProcessOptions options;

            public ProxyOptions(Launcher.ProcessOptions options)
            {
                this.options = options;
            }

            public Launcher.ProcessOptions Options
            {
                get
                {
                    return options;
                }
            }

            private string filename;
            public string FileName
            {
                get
                {
                    if (filename == null)
                        return options.FileName;
                    else
                        return filename;
                }
                set
                {
                    filename = value;
                }
            }

            public static ProxyOptions FromStream(Stream stream)
            {
                using (var reader = new BinaryReader(stream, Encoding.UTF8, true))
                {
                    if (reader.ReadByte() != 0)
                        throw new IOException("Unexpected data");

                    var options = new Launcher.ProcessOptions();
                    var po = new ProxyOptions(options);

                    options.FileName = reader.ReadString();
                    options.Arguments = reader.ReadString();
                    options.WorkingDirectory = reader.ReadString();
                    options.UserName = reader.ReadString();

                    var variables = reader.ReadByte();
                    while (variables > 0)
                    {
                        options.Variables[reader.ReadString()] = reader.ReadString();
                        variables--;
                    }

                    return po;
                }
            }

            private void Write(BinaryWriter writer, string value)
            {
                if (value == null)
                    writer.Write(string.Empty);
                else
                    writer.Write(value);
            }

            public void CopyTo(Stream stream)
            {
                using (var writer = new BinaryWriter(stream, Encoding.UTF8, true))
                {
                    writer.Write((byte)LaunchResult.None);

                    Write(writer, this.FileName);
                    Write(writer, options.Arguments);
                    Write(writer, options.WorkingDirectory);
                    Write(writer, options.UserName);

                    writer.Write((byte)options.Variables.Count);
                    foreach (var key in options.Variables.Keys)
                    {
                        Write(writer, key);
                        Write(writer, options.Variables[key]);
                    }
                }
            }
        }

        private static string _fileDescription;

        static ProxyLauncher()
        {
            InitializePath();
            Settings.GW2Path.ValueChanged += GW2Path_ValueChanged;
            Settings.LocalizeAccountExecution.ValueChanged += LocalizeAccountExecution_ValueChanged;
        }

        static void LocalizeAccountExecution_ValueChanged(object sender, EventArgs e)
        {
            try
            {
                var di = new DirectoryInfo(Path.Combine(DataPath.AppDataAccountData, "pl"));
                if (di.Exists)
                {
                    foreach (var d in di.GetDirectories())
                    {
                        try
                        {
                            d.Delete(true);
                        }
                        catch { }
                    }
                }
            }
            catch (Exception ex)
            {
                Util.Logging.Log(ex);
            }
        }

        static void GW2Path_ValueChanged(object sender, EventArgs e)
        {
            InitializePath();
        }

        static void InitializePath()
        {
            _fileDescription = null;

            try
            {
                var di = new DirectoryInfo(Path.Combine(DataPath.AppDataAccountData, "pl"));
                if (di.Exists)
                {
                    foreach (var d in di.GetDirectories())
                    {
                        try
                        {
                            d.Delete(true);
                        }
                        catch { }
                    }
                }
                else
                {
                    di.Create();
                }
            }
            catch (Exception e)
            {
                Util.Logging.Log(e);
            }
        }

        private static FileInfo GetLink(Settings.IAccount account, Launcher.ProcessOptions options)
        {
            var path = Path.Combine(DataPath.AppDataAccountData, "pl", account.UID.ToString());

            string name;
            if ((name = _fileDescription) == null)
            {
                try
                {
                    _fileDescription = name = FileVersionInfo.GetVersionInfo(options.FileName).FileDescription;
                    if (string.IsNullOrWhiteSpace(name))
                        throw new NotSupportedException();
                    return new FileInfo(Path.Combine(path, name + ".lnk"));
                }
                catch
                {
                    _fileDescription = name = "Guild Wars 2";
                }
            }

            return new FileInfo(Path.Combine(path, name + ".lnk"));
        }

        public static Process Launch(Settings.IAccount account, Launcher.ProcessOptions options)
        {
            var link = GetLink(account, options);
            try
            {
                var di = link.Directory;
                if (!di.Exists)
                    di.Create();
            }
            catch (Exception e)
            {
                Util.Logging.Log(e);
            }
            if (!link.Exists)
            {
                new Windows.Shortcut(options.FileName, "")
                {
                    AppUserModelID = "Gw2Launcher." + account.UID,
                    PreventPinning = true
                }.Save(link.FullName);
            }

            var po = new ProxyOptions(options);
            po.FileName = link.FullName;

            int pid;
            using (var p = Process.GetCurrentProcess())
                pid = p.Id;

            var capacity = 512;
            MemoryMappedFile mf;

            using (var ms = new MemoryStream(capacity))
            {
                po.CopyTo(ms);

                if (ms.Length > capacity)
                    capacity = (int)ms.Length;

                mf = MemoryMappedFile.CreateNew(Messaging.MappedMessage.BASE_ID + "PL:" + pid, capacity);
                using (var stream = mf.CreateViewStream())
                {
                    ms.Position = 0;
                    ms.CopyTo(stream);
                }
            }

            using (mf)
            {
                var path = Assembly.GetExecutingAssembly().Location;
                var startInfo = new ProcessStartInfo(path, "-pl " + pid);
                startInfo.UseShellExecute = false;
                startInfo.WorkingDirectory = Path.GetDirectoryName(path);

                if (!string.IsNullOrEmpty(options.UserName))
                {
                    if (!Util.Users.IsCurrentUser(options.UserName))
                        startInfo.WorkingDirectory = DataPath.AppDataAccountData;
                    startInfo.UserName = options.UserName;
                    startInfo.Password = options.Password;
                    startInfo.LoadUserProfile = true;
                }

                using (var p = Process.Start(startInfo))
                {
                    p.WaitForExit();

                    using (var reader = new BinaryReader(mf.CreateViewStream(), Encoding.UTF8))
                    {
                        var result = (LaunchResult)reader.ReadByte();
                        if (result == LaunchResult.Success)
                        {
                            pid = reader.ReadInt32();
                            try
                            {
                                return Process.GetProcessById(pid);
                            }
                            catch (Exception e)
                            {
                                Util.Logging.Log(e);
                                return null;
                            }
                        }
                        else if (result == LaunchResult.Failed)
                        {
                            var msg = reader.ReadString();
                            throw new Exception(msg);
                        }
                        else if (result == LaunchResult.None)
                        {
                            throw new Exception("No response from launcher");
                        }
                        else
                        {
                            throw new NotSupportedException();
                        }
                    }
                }
            }
        }
    }
}
