using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.IO;

namespace Gw2Launcher.Tools.Backup
{
    public abstract class Backup
    {
        private const int HEADER = 1349206604;
        private const ushort VERSION = 1;

        public struct BackupOptions
        {
            public BackupFormat Format
            {
                get;
                set;
            }

            public bool IncludeLocalDat
            {
                get;
                set;
            }

            public bool IncludeGfxSettings
            {
                get;
                set;
            }

            /// <summary>
            /// Optional override encryption; null to use settings
            /// </summary>
            public Settings.EncryptionOptions Encryption
            {
                get;
                set;
            }
        }

        public interface IFileInformation
        {
            /// <summary>
            /// Relative to %appdata%, otherwise absolute; for display purposes
            /// </summary>
            string Path
            {
                get;
            }

            PathInformation Input
            {
                get;
            }

            PathInformation Output
            {
                get;
            }
        }

        public class PathInformation
        {
            public bool Exists
            {
                get;
                set;
            }

            /// <summary>
            /// Path of the file, or null if it's internal
            /// </summary>
            public string Path
            {
                get;
                set;
            }
        }

        private class FileInformation : IFileInformation
        {
            public string Path
            {
                get;
                set;
            }

            public FileType Type
            {
                get;
                set;
            }

            public ushort UID
            {
                get;
                set;
            }

            public PathInformation Input
            {
                get;
                set;
            }

            public PathInformation Output
            {
                get;
                set;
            }

            public long Position
            {
                get;
                set;
            }

            public uint Length
            {
                get;
                set;
            }
        }

        public class RestoreInformation
        {
            public RestoreInformation(string path, DateTime created, IList<IFileInformation> files, IFileInformation settings)
            {
                this.Path = path;
                this.Created = created;
                this.Settings = settings;
                this.Files = files;
            }

            public DateTime Created
            {
                get;
                private set;
            }

            public IFileInformation Settings
            {
                get;
                private set;
            }

            public bool HasSettings
            {
                get
                {
                    return this.Settings != null;
                }
            }

            public string Path
            {
                get;
                private set;
            }

            public IList<IFileInformation> Files
            {
                get;
                private set;
            }

            public void Extract(IFileInformation file, string path)
            {
                var f = (FileInformation)file;
                if (!f.Input.Exists)
                    throw new FileNotFoundException();
                if (f.Input.Path != null)
                {
                    File.Copy(f.Input.Path, path, true);
                }
                else
                {
                    using (var output = File.Open(path, FileMode.Create, FileAccess.Write, FileShare.None))
                    {
                        using (var input = File.Open(this.Path, FileMode.Open, FileAccess.Read, FileShare.Read))
                        {
                            using (var reader = new BinaryReader(input))
                            {
                                input.Position = f.Position - 4;
                                if (reader.ReadUInt32() != f.Length)
                                    throw new IOException("Length mismatch");
                                var buffer = new byte[f.Length > 4096 ? 4096 : f.Length];
                                var remaining = (long)f.Length;
                                
                                while (remaining > 0)
                                {
                                    var read = input.Read(buffer, 0, remaining > buffer.Length ? buffer.Length : (int)remaining);
                                    output.Write(buffer, 0, read);
                                    remaining -= read;
                                }
                            }
                        }
                    }
                }
            }

            public uint GetSize(IFileInformation file)
            {
                var f = (FileInformation)file;
                if (!f.Input.Exists)
                    return 0;
                if (f.Input.Path != null)
                    return (uint)new FileInfo(f.Input.Path).Length;
                else
                    return f.Length;
            }
        }

        private class RelativePath
        {
            public RelativePath(PathType type, string path)
            {
                this.Type = type;
                this.Path = path;
            }

            public PathType Type
            {
                get;
                set;
            }

            public string Path
            {
                get;
                set;
            }
        }

        private class PathData
        {
            public PathData(bool importing)
            {
                Gw2LauncherAppData = DataPath.AppData;
                Gw2LauncherAccountData = DataPath.AppDataAccountData;
                AppData = System.IO.Path.GetDirectoryName(Gw2LauncherAppData);
                if (importing)
                    Gw2AppData = System.IO.Path.Combine(AppData, "Guild Wars 2");
                else
                    Gw2AppData = Client.FileManager.GetPath(Client.FileManager.SpecialPath.AppData);
            }

            public string Gw2LauncherAppData
            {
                get;
                private set;
            }

            public string Gw2LauncherAccountData
            {
                get;
                private set;
            }

            public string Gw2AppData
            {
                get;
                private set;
            }

            public string AppData
            {
                get;
                private set;
            }

            public bool IsRoot(string root, string path)
            {
                if (path.StartsWith(root, StringComparison.OrdinalIgnoreCase))
                {
                    return root.Length == path.Length || path[root.Length] == System.IO.Path.DirectorySeparatorChar || path[root.Length] == System.IO.Path.AltDirectorySeparatorChar;
                }

                return false;
            }

            public string GetRelative(string root, string path)
            {
                var i = root.Length;
                var l = path.Length;

                while (i < l && (path[i] == System.IO.Path.DirectorySeparatorChar || path[i] == System.IO.Path.AltDirectorySeparatorChar))
                {
                    i++;
                }

                return path.Substring(i);
            }

            public RelativePath GetRelativePath(string path)
            {
                if (IsRoot(Gw2LauncherAppData, path))
                {
                    if (IsRoot(Gw2LauncherAccountData, path))
                    {
                        return new RelativePath(PathType.Gw2LauncherAccountData, GetRelative(Gw2LauncherAccountData, path));
                    }
                    else
                    {
                        return new RelativePath(PathType.Gw2LauncherAppData, GetRelative(Gw2LauncherAppData, path));
                    }
                }
                else if (IsRoot(AppData, path))
                {
                    return new RelativePath(PathType.AppData, GetRelative(AppData, path));

                    //if (IsRoot(Gw2AppData, path))
                    //{
                    //    return new RelativePath(PathType.GuildWars2AppData, GetRelative(Gw2AppData, path));
                    //}
                }
                else
                {
                    return new RelativePath(PathType.Absolute, path);
                }
            }

            public string GetPath(PathType type, string relative)
            {
                var path = GetPath(type);
                if (path == null)
                    return relative;
                return System.IO.Path.Combine(path, relative);
            }

            public string GetPath(PathType type, params string[] paths)
            {
                return GetPath(type, System.IO.Path.Combine(paths));
            }

            public string GetPath(PathType type)
            {
                switch (type)
                {
                    case PathType.GuildWars2AppData:
                        return Gw2AppData;
                    case PathType.Gw2LauncherAccountData:
                        return Gw2LauncherAccountData;
                    case PathType.Gw2LauncherAppData:
                        return Gw2LauncherAppData;
                    case PathType.AppData:
                        return AppData;
                    case PathType.Absolute:
                    default:
                        return null;
                }
            }
        }

        public class FileErrorEventArgs : EventArgs
        {
            public FileErrorEventArgs(Exception e, string path)
            {
                this.Exception = e;
            }

            public string Path
            {
                get;
                private set;
            }

            public bool Abort
            {
                get;
                set;
            }

            public bool Retry
            {
                get;
                set;
            }

            public Exception Exception
            {
                get;
                private set;
            }
        }

        public enum BackupFormat : byte
        {
            File = 0,
            Directory = 1
        }

        private enum FileType : byte
        {
            Settings = 0,
            Data = 1,

            Gw2LocalDat = 2,
            Gw2GfxSettings = 3,

            EndOfFiles = 255,
        }

        private enum PathType : byte
        {
            Absolute = 0,
            AppData = 1,
            Gw2LauncherAppData = 2,
            Gw2LauncherAccountData = 3,
            GuildWars2AppData = 4,
        }

        public event EventHandler ProgressChanged;
        public event EventHandler Complete;

        private float _Progress;
        public float Progress
        {
            get
            {
                return _Progress;
            }
            protected set
            {
                _Progress = value;
                if (ProgressChanged != null)
                    ProgressChanged(this, EventArgs.Empty);
            }
        }

        public string Path
        {
            get;
            protected set;
        }

        public class Exporter : Backup
        {
            public Exporter(string path, BackupOptions options)
            {
                this.Path = path;
                this.Options = options;
            }

            public BackupOptions Options
            {
                get;
                set;
            }

            public void Export(CancellationToken cancel)
            {
                var path = this.Path;
                string folder = null;
                var success = false;
                Stream[] streams = null;

                try
                {
                    if (this.Options.Format == BackupFormat.Directory)
                    {
                        if (File.Exists(path))
                            File.Delete(path);
                        var fn = System.IO.Path.GetFileName(path);
                        var dn = System.IO.Path.GetFileName(System.IO.Path.GetDirectoryName(path));
                        if (fn != dn)
                            path = System.IO.Path.Combine(path, System.IO.Path.GetFileName(path));
                        folder = System.IO.Path.GetDirectoryName(path);
                        Directory.CreateDirectory(folder);
                    }

                    using (var stream = new BufferedStream(File.Open(path, FileMode.Create, FileAccess.Write, FileShare.None)))
                    {
                        using (cancel.Register(
                            delegate
                            {
                                try
                                {
                                    stream.Close();
                                }
                                catch { }
                            }))
                        {
                            using (var writer = new BinaryWriter(stream))
                            {
                                writer.Write(HEADER);
                                writer.Write(VERSION);
                                writer.Write(DateTime.UtcNow.ToBinary());
                                writer.Write((byte)this.Options.Format);

                                var pd = new PathData(false);
                                var unknowns = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
                                var files = new List<FileInformation>();

                                files.Add(new FileInformation()
                                {
                                    Type = FileType.Settings,
                                    Input = new PathInformation()
                                    {
                                        Path = pd.GetPath(PathType.Gw2LauncherAppData, "settings.dat"),
                                    },
                                });

                                files.Add(new FileInformation()
                                {
                                    Type = FileType.Settings,
                                    Input = new PathInformation()
                                    {
                                        Path = pd.GetPath(PathType.Gw2LauncherAppData, "notes.dat"),
                                    },
                                });

                                if (this.Options.IncludeGfxSettings)
                                {
                                    foreach (var v in Settings.GfxFiles.GetValues())
                                    {
                                        if (v.HasValue && v.Value.References > 0 && !string.IsNullOrEmpty(v.Value.Path))
                                        {
                                            files.Add(new FileInformation()
                                            {
                                                Type = FileType.Gw2GfxSettings,
                                                UID = v.Value.UID,
                                                Input = new PathInformation()
                                                {
                                                    Path = v.Value.Path,
                                                },
                                            });
                                        }
                                    }
                                }

                                if (this.Options.IncludeLocalDat)
                                {
                                    foreach (var v in Settings.DatFiles.GetValues())
                                    {
                                        if (v.HasValue && v.Value.References > 0 && !string.IsNullOrEmpty(v.Value.Path))
                                        {
                                            files.Add(new FileInformation()
                                            {
                                                Type = FileType.Gw2LocalDat,
                                                UID = v.Value.UID,
                                                Input = new PathInformation()
                                                {
                                                    Path = v.Value.Path,
                                                },
                                            });
                                        }
                                    }
                                }

                                streams = new Stream[files.Count];
                                long estimatedLength = 0;

                                for (int i = files.Count - 1; i >= 0; --i)
                                {
                                    var f = files[i];

                                    f.Input.Exists = File.Exists(f.Input.Path);
                                    estimatedLength += f.Input.Path.Length;

                                    if (f.Input.Exists)
                                    {
                                        var retry = 3;

                                        while (true)
                                        {
                                            try
                                            {
                                                streams[i] = File.Open(f.Input.Path, FileMode.Open, FileAccess.Read, FileShare.Read);
                                                estimatedLength += streams[i].Length;
                                                break;
                                            }
                                            catch
                                            {
                                                if (--retry == 0 || cancel.IsCancellationRequested)
                                                {
                                                    if (f.Type == FileType.Settings)
                                                        break;
                                                    throw;
                                                }
                                            }

                                            cancel.WaitHandle.WaitOne(500);
                                        }
                                    }
                                    else if (f.Type == FileType.Settings)
                                    {
                                        f.Input.Exists = true;
                                    }
                                }

                                switch (this.Options.Format)
                                {
                                    case BackupFormat.File:

                                        stream.SetLength(stream.Position + estimatedLength);

                                        break;
                                }

                                for (int i = 0, count = files.Count; i < count; i++)
                                {
                                    var f = files[i];
                                    var relative = pd.GetRelativePath(f.Input.Path);

                                    Progress = (float)i / count;

                                    writer.Write((byte)f.Type);
                                    writer.Write(f.Input.Exists);
                                    writer.Write((byte)relative.Type);
                                    writer.Write(relative.Path);

                                    switch (f.Type)
                                    {
                                        case FileType.Gw2GfxSettings:
                                        case FileType.Gw2LocalDat:

                                            writer.Write(f.UID);

                                            break;
                                    }

                                    if (!f.Input.Exists)
                                        continue;

                                    if (this.Options.Format == BackupFormat.Directory)
                                    {
                                        string relativePath;

                                        if (pd.IsRoot(pd.AppData, f.Input.Path))
                                        {
                                            relativePath = pd.GetRelative(pd.AppData, f.Input.Path);
                                        }
                                        else
                                        {
                                            int k = 1;
                                            var fn = System.IO.Path.GetFileName(f.Input.Path);
                                            relativePath = fn;
                                            while (!unknowns.Add(relativePath))
                                            {
                                                relativePath = ++k + "-" + fn;
                                            }
                                        }

                                        writer.Write(relativePath);

                                        f.Output = new PathInformation()
                                        {
                                            Path = System.IO.Path.Combine(folder, relativePath),
                                        };

                                        Directory.CreateDirectory(System.IO.Path.GetDirectoryName(f.Output.Path));
                                    }

                                    using (streams[i])
                                    {
                                        if (f.Type == FileType.Settings)
                                        {
                                            using (var ms = new MemoryStream(streams[i] == null ? 10000 : (int)streams[i].Length + 1000))
                                            {
                                                Settings.WriteTo(ms, this.Options.Encryption);
                                                ms.Position = 0;

                                                if (this.Options.Format == BackupFormat.Directory)
                                                {
                                                    using (var fs = File.Open(f.Output.Path, FileMode.Create, FileAccess.Write, FileShare.None))
                                                    {
                                                        ms.CopyTo(fs);
                                                    }
                                                }
                                                else
                                                    WriteStream(writer, stream, ms);
                                            }
                                        }
                                        else
                                        {
                                            if (this.Options.Format == BackupFormat.Directory)
                                            {
                                                File.Copy(f.Input.Path, f.Output.Path, true);
                                            }
                                            else
                                            {
                                                WriteStream(writer, stream, streams[i]);
                                            }
                                        }
                                    }

                                    streams[i] = null;
                                }

                                writer.Write((byte)FileType.EndOfFiles);
                                stream.SetLength(stream.Position);

                                Progress = 1f;
                            }
                        }
                    }

                    success = true;
                }
                finally
                {
                    if (streams != null)
                    {
                        foreach (var stream in streams)
                        {
                            if (stream != null)
                                stream.Dispose();
                        }
                    }

                    if (!success)
                    {
                        try
                        {
                            File.Delete(path);
                        }
                        catch { }

                        if (folder != null)
                        {
                            try
                            {
                                Util.FileUtil.DeleteDirectory(folder);
                            }
                            catch { }
                        }
                    }

                    if (Complete != null)
                        Complete(this, EventArgs.Empty);
                }
            }

            private void WriteStream(BinaryWriter writer, Stream stream, Stream input)
            {
                var p = stream.Position;
                var length = (uint)input.Length;

                writer.Write(length);
                input.CopyTo(stream);

                var p2 = stream.Position;
                var length2 = stream.Position - p - 4;

                if (length2 != length)
                {
                    stream.Position = p;
                    writer.Write(length2);
                    stream.Position = p2;
                }
            }
        }

        public class Importer : Backup
        {
            public event EventHandler<FileErrorEventArgs> FileError;
            public event EventHandler<IFileInformation> Importing;

            public Importer(string path)
            {
                this.Path = path;
            }

            public RestoreInformation Import()
            {
                return Import(this.Path, false);
            }

            public RestoreInformation ReadInformation()
            {
                return Import(this.Path, true);
            }

            private RestoreInformation Import(string path, bool infoOnly)
            {
                using (var stream = new BufferedStream(File.Open(path, FileMode.Open, FileAccess.Read, FileShare.Read)))
                {
                    using (var reader = new BinaryReader(stream))
                    {
                        if (reader.ReadInt32() != HEADER)
                            throw new IOException("Invalid file header");

                        var version = reader.ReadUInt16();
                        var created = DateTime.FromBinary(reader.ReadInt64());
                        var format = (BackupFormat)reader.ReadByte();
                        var pd = new PathData(true);
                        var folder = format == BackupFormat.Directory ? System.IO.Path.GetDirectoryName(path) : null;
                        var files = new List<IFileInformation>();
                        IFileInformation settings = null;
                        var length = stream.Length;
                        byte[] buffer = null;

                        while (stream.Position < length)
                        {
                            Progress = (float)stream.Position / length;

                            var fileType = (FileType)reader.ReadByte();
                            if (fileType == FileType.EndOfFiles)
                                break;
                            var exists = reader.ReadBoolean();
                            var pathType = (PathType)reader.ReadByte();
                            var filePath = pd.GetPath(pathType, reader.ReadString());

                            ushort uid;
                            switch (fileType)
                            {
                                case FileType.Gw2GfxSettings:
                                case FileType.Gw2LocalDat:

                                    uid = reader.ReadUInt16();

                                    if (pathType == PathType.Absolute)
                                    {
                                        //files were stored in an unknown location; move to the data folder
                                        pathType = PathType.Gw2LauncherAccountData;
                                        filePath = pd.GetPath(pathType, uid.ToString());

                                        switch (fileType)
                                        {
                                            case FileType.Gw2GfxSettings:
                                                filePath += ".xml";
                                                break;
                                            case FileType.Gw2LocalDat:
                                                filePath += ".dat";
                                                break;
                                        }
                                    }

                                    break;
                                default:

                                    uid = 0;

                                    break;
                            }

                            var fi = new FileInformation()
                            {
                                Path = filePath,
                                Type = fileType,
                                UID = uid,
                                Input = new PathInformation()
                                {
                                    Exists = true,
                                },
                                Output = new PathInformation()
                                {
                                    Exists = File.Exists(filePath),
                                    Path = filePath,
                                },
                            };
                            files.Add(fi);

                            if (pd.IsRoot(pd.AppData, fi.Output.Path))
                            {
                                fi.Path = pd.GetRelative(pd.AppData, fi.Output.Path);
                            }

                            if (!exists)
                            {
                                if (!infoOnly && fi.Type == FileType.Data && fi.Output.Exists)
                                {
                                }

                                continue;
                            }

                            if (format == BackupFormat.Directory)
                            {
                                var relativePath = System.IO.Path.Combine(folder, reader.ReadString());

                                fi.Input.Path = relativePath;
                                fi.Input.Exists = File.Exists(relativePath);

                                if (!infoOnly && fi.Input.Exists)
                                {
                                    if (Importing != null)
                                        Importing(this, fi);

                                    while (true)
                                    {
                                        try
                                        {
                                            File.Copy(fi.Input.Path, fi.Output.Path, true);
                                            break;
                                        }
                                        catch (Exception e)
                                        {
                                            if (FileError != null)
                                            {
                                                var fe = new FileErrorEventArgs(e, fi.Output.Path);
                                                FileError(this, fe);
                                                if (fe.Abort)
                                                    return null;
                                                if (!fe.Retry)
                                                    break;
                                            }
                                            else
                                            {
                                                return null;
                                            }
                                        }
                                    }
                                }
                            }
                            else
                            {
                                fi.Length = reader.ReadUInt32();
                                fi.Position = stream.Position;

                                if (infoOnly)
                                {
                                    stream.Position = fi.Position + fi.Length;
                                }
                                else
                                {
                                    if (Importing != null)
                                        Importing(this, fi);

                                    while (true)
                                    {
                                        try
                                        {
                                            if (buffer == null)
                                                buffer = new byte[102400];

                                            using (var f = File.Open(fi.Output.Path, FileMode.Create, FileAccess.Write, FileShare.None))
                                            {
                                                long remaining = fi.Length;

                                                do
                                                {
                                                    var read = stream.Read(buffer, 0, remaining > buffer.Length ? buffer.Length : (int)remaining);
                                                    remaining -= read;

                                                    f.Write(buffer, 0, read);

                                                    Progress = (float)stream.Position / length;
                                                }
                                                while (remaining > 0);
                                            }

                                            break;
                                        }
                                        catch (Exception e)
                                        {
                                            if (FileError != null)
                                            {
                                                var fe = new FileErrorEventArgs(e, fi.Output.Path);
                                                FileError(this, fe);
                                                if (fe.Abort)
                                                    return null;
                                                if (!fe.Retry)
                                                    break;
                                            }
                                            else
                                            {
                                                return null;
                                            }
                                        }

                                        stream.Position = fi.Position;
                                    }

                                    if (stream.Position != fi.Position + fi.Length)
                                        stream.Position = fi.Position + fi.Length;
                                }
                            }

                            if (fi.Type == FileType.Settings && fi.Input.Exists)
                                settings = fi;
                        }

                        Progress = 1f;

                        return new RestoreInformation(path, created, files, settings);
                    }
                }
            }
        }
    }
}
