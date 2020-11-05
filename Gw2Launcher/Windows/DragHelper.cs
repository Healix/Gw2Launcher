using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.ComTypes;
using System.Drawing;
using System.IO;
using DataFormats = System.Windows.Forms.DataFormats;
using Gw2Launcher.Windows.Native;

namespace Gw2Launcher.Windows
{
    static class DragHelper
    {
        //References:
        //https://blogs.msdn.microsoft.com/adamroot/2008/02/19/shell-style-drag-and-drop-in-net-part-3/
        //https://dlaa.me/blog/post/9923072

        private const string CFSTR_DROPDESCRIPTION = "DropDescription";
        private const string CFSTR_ISSHOWINGLAYERED = "IsShowingLayered";
        private const string CFSTR_DRAGWINDOW = "DragWindow";
        private const string CFSTR_FILEDESCRIPTOR = "FileGroupDescriptorW";
        private const string CFSTR_PERFORMEDDROPEFFECT = "Performed DropEffect";
        private const string CFSTR_FILECONTENTS = "FileContents";
        private const string CFSTR_SHELLIDLIST = "Shell IDList Array";

        private static readonly short CF_FILEDESCRIPTOR = (short)(DataFormats.GetFormat(CFSTR_FILEDESCRIPTOR).Id);
        private static readonly short CF_PERFORMEDDROPEFFECT = (short)(DataFormats.GetFormat(CFSTR_PERFORMEDDROPEFFECT).Id);
        private static readonly short CF_FILECONTENTS = (short)(DataFormats.GetFormat(CFSTR_FILECONTENTS).Id);
        
        [ComImport, Guid("4657278A-411B-11d2-839A-00C04FD918D0")]
        private class DragDropHelper { }

        [ComVisible(true), ComImport, Guid("DE5BF786-477A-11D2-839D-00C04FD918D0"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
        private interface IDragSourceHelper
        {
            void InitializeFromBitmap(
                [In, MarshalAs(UnmanagedType.Struct)] ref ShDragImage dragImage,
                [In, MarshalAs(UnmanagedType.Interface)] IDataObject dataObject);

            void InitializeFromWindow(
                [In] IntPtr hwnd,
                [In] ref Point pt,
                [In, MarshalAs(UnmanagedType.Interface)] IDataObject dataObject);
        }

        [ComVisible(true), ComImport, Guid("4657278B-411B-11D2-839A-00C04FD918D0"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
        private interface IDropTargetHelper
        {
            void DragEnter(
                [In] IntPtr hwndTarget,
                [In, MarshalAs(UnmanagedType.Interface)] IDataObject dataObject,
                [In] ref Point pt,
                [In] System.Windows.Forms.DragDropEffects effect);

            void DragLeave();

            void DragOver(
                [In] ref Point pt,
                [In] System.Windows.Forms.DragDropEffects effect);

            void Drop(
                [In, MarshalAs(UnmanagedType.Interface)] IDataObject dataObject,
                [In] ref Point pt,
                [In] System.Windows.Forms.DragDropEffects effect);

            void Show(
                [In] bool show);
        }

        [ComVisible(true), ComImport, Guid("83E07D0D-0C5F-4163-BF1A-60B274051E40"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
        private interface IDragSourceHelper2
        {
            void InitializeFromBitmap(
                [In, MarshalAs(UnmanagedType.Struct)] ref ShDragImage dragImage,
                [In, MarshalAs(UnmanagedType.Interface)] IDataObject dataObject);

            void InitializeFromWindow(
                [In] IntPtr hwnd,
                [In] ref Point pt,
                [In, MarshalAs(UnmanagedType.Interface)] IDataObject dataObject);

            void SetFlags(
                [In] int dwFlags);
        }

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode, Size = 1044)]
        private struct DropDescription
        {
            public int type;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 260)]
            public string szMessage;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 260)]
            public string szInsert;
        }

        private enum DropImageType
        {
            Invalid = -1,
            None = 0,
            Copy = (int)System.Windows.Forms.DragDropEffects.Copy,
            Move = (int)System.Windows.Forms.DragDropEffects.Move,
            Link = (int)System.Windows.Forms.DragDropEffects.Link,
            Label = 6,
            Warning = 7,
            NoImage = 8
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct ShDragImage
        {
            public Size sizeDragImage;
            public Point ptOffset;
            public IntPtr hbmpDragImage;
            public int crColorKey;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct FILEGROUPDESCRIPTOR
        {
            public UInt32 cItems;
        }

        [Flags]
        private enum FileDescriptorFlags : uint
        {
            FD_CLSID = 0x00000001,
            FD_SIZEPOINT = 0x00000002,
            FD_ATTRIBUTES = 0x00000004,
            FD_CREATETIME = 0x00000008,
            FD_ACCESSTIME = 0x00000010,
            FD_WRITESTIME = 0x00000020,
            FD_FILESIZE = 0x00000040,
            FD_PROGRESSUI = 0x00004000,
            FD_LINKUI = 0x00008000,
            FD_UNICODE = 0x80000000 //Windows Vista and later
        }

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
        private struct FILEDESCRIPTOR
        {
            public FileDescriptorFlags dwFlags;
            public Guid clsid;
            public System.Drawing.Size sizel;
            public System.Drawing.Point pointl;
            public UInt32 dwFileAttributes;
            public System.Runtime.InteropServices.ComTypes.FILETIME ftCreationTime;
            public System.Runtime.InteropServices.ComTypes.FILETIME ftLastAccessTime;
            public System.Runtime.InteropServices.ComTypes.FILETIME ftLastWriteTime;
            public UInt32 nFileSizeHigh;
            public UInt32 nFileSizeLow;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 260)]
            public string cFileName;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct CIDA
        {
            /// <summary>
            ///  Number of PIDLs that are being transferred, not counting the parent folder.
            /// </summary>
            public uint cidl;

            /// <summary>
            /// An array of offsets, relative to the beginning of this structure. The array contains
            /// cidl+1 elements. The first element of aoffset contains an offset to the fully-qualified
            /// PIDL of a parent foloder. If this PIDL is empty, the parent folder is the desktop.
            /// Each of the remaining elements of the array contains an offset to one of the PIDLs to be 
            /// transferred. ALL of these PIDLs are relative to the PIDL of the parent folder.
            /// </summary>
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 1)]
            public uint[] aoffset;
        }

        private class AdviseSink : IAdviseSink
        {
            // The associated data object
            private DataObjectContainer data;

            /// <summary>
            /// Creates an AdviseSink associated to the specified data object.
            /// </summary>
            /// <param name="data">The data object.</param>
            public AdviseSink(DataObjectContainer data)
            {
                this.data = data;
            }

            /// <summary>
            /// Handles DataChanged events from a COM IDataObject.
            /// </summary>
            /// <param name="format">The data format that had a change.</param>
            /// <param name="stgmedium">The data value.</param>
            public void OnDataChange(ref FORMATETC format, ref STGMEDIUM stgmedium)
            {
                // We listen to DropDescription changes, so that we can unset the IsDefault
                // drop description flag.

                if (data.Description.type != Int32.MinValue)
                    data.IsDescriptionDefault = false;
            }

            public void OnClose()
            {
                throw new NotImplementedException();
            }

            public void OnRename(System.Runtime.InteropServices.ComTypes.IMoniker moniker)
            {
                throw new NotImplementedException();
            }

            public void OnSave()
            {
                throw new NotImplementedException();
            }

            public void OnViewChange(int aspect, int index)
            {
                throw new NotImplementedException();
            }
        }

        public class DragHelperInstance : IDisposable
        {
            private IDropTargetHelper instance;

            public DragHelperInstance(System.Windows.Forms.Form form)
            {
                instance = (IDropTargetHelper)new DragDropHelper();

                form.DragDrop += form_DragDrop;
                form.DragEnter += form_DragEnter;
                form.DragLeave += form_DragLeave;
                form.DragOver += form_DragOver;
            }

            void form_DragOver(object sender, System.Windows.Forms.DragEventArgs e)
            {
                var p = new Point(e.X, e.Y);
                instance.DragOver(ref p, e.Effect);
            }

            void form_DragLeave(object sender, EventArgs e)
            {
                instance.DragLeave();
            }

            void form_DragEnter(object sender, System.Windows.Forms.DragEventArgs e)
            {
                var p = new Point(e.X, e.Y);
                instance.DragEnter(((System.Windows.Forms.Form)sender).Handle, (IDataObject)e.Data, ref p, e.Effect);
            }

            void form_DragDrop(object sender, System.Windows.Forms.DragEventArgs e)
            {
                var p = new Point(e.X, e.Y);
                instance.Drop((IDataObject)e.Data, ref p, e.Effect);
            }

            public void Dispose()
            {
                if (instance != null)
                {
                    Marshal.ReleaseComObject(instance);
                    instance = null;
                }
            }
        }

        public class FileDescriptor
        {
            public FileDescriptor()
            {
                Length = -1;
            }

            /// <summary>
            /// The name of the file
            /// </summary>
            public string Name
            {
                get;
                set;
            }

            /// <summary>
            /// The size of the file, or -1 to skip
            /// </summary>
            public int Length
            {
                get;
                set;
            }

            public Func<FileDescriptor, Stream> GetContent
            {
                get;
                set;
            }

            public int Index
            {
                get;
                internal set;
            }
        }

        private class IStreamContainer : IStream, IDisposable
        {
            protected FileDescriptor file;
            protected Stream stream;
            protected IntPtr handle;

            protected IStreamContainer()
            {
                handle = Marshal.GetComInterfaceForObject(this, typeof(IStream));
            }

            public IStreamContainer(FileDescriptor file)
                : this()
            {
                this.file = file;
            }

            protected IStreamContainer(Stream stream)
                : this()
            {
                this.stream = stream;
            }

            public void Clone(out IStream ppstm)
            {
                IStreamContainer clone = new IStreamContainer(new MemoryStream((int)this.BaseStream.Length));
                var p = this.BaseStream.Position;
                this.stream.Position = 0;
                this.stream.CopyTo(clone.stream);
                clone.stream.Position = this.stream.Position = p;
                ppstm = clone;
            }

            public void Commit(int grfCommitFlags)
            {
            }

            public void CopyTo(IStream pstm, long cb, IntPtr pcbRead, IntPtr pcbWritten)
            {
                long count = this.BaseStream.Length - this.stream.Position;
                if (cb < count)
                    count = cb;
                byte[] buffer = new byte[count];
                int read = this.stream.Read(buffer, 0, (int)count);

                pstm.Write(buffer, read, pcbWritten);

                if (pcbRead != IntPtr.Zero)
                    Marshal.WriteInt64(pcbRead, read);
            }

            public void LockRegion(long libOffset, long cb, int dwLockType)
            {
            }

            public void Read(byte[] pv, int cb, IntPtr pcbRead)
            {
                int read = this.BaseStream.Read(pv, 0, cb);
                if (pcbRead != IntPtr.Zero)
                    Marshal.WriteInt64(pcbRead, read);
            }

            public void Revert()
            {
            }

            public void Seek(long dlibMove, int dwOrigin, IntPtr plibNewPosition)
            {
                long p = this.BaseStream.Seek(dlibMove, (SeekOrigin)dwOrigin);
                if (plibNewPosition != IntPtr.Zero)
                    Marshal.WriteInt64(plibNewPosition, p);
            }

            public void SetSize(long libNewSize)
            {
                this.BaseStream.SetLength(libNewSize);
            }

            public void Stat(out System.Runtime.InteropServices.ComTypes.STATSTG pstatstg, int grfStatFlag)
            {
                pstatstg = new System.Runtime.InteropServices.ComTypes.STATSTG()
                {
                    cbSize = this.BaseStream.Length,
                    type = 2,
                };
            }

            public void UnlockRegion(long libOffset, long cb, int dwLockType)
            {
            }

            public void Write(byte[] pv, int cb, IntPtr pcbWritten)
            {
                this.BaseStream.Write(pv, 0, cb);
                if (pcbWritten != IntPtr.Zero)
                    Marshal.WriteInt64(pcbWritten, cb);
            }

            public Stream BaseStream
            {
                get
                {
                    if (this.stream == null)
                    {
                        lock (this)
                        {
                            if (this.stream == null)
                                this.stream = file.GetContent(file);
                        }
                    }
                    return this.stream;
                }
            }

            public IntPtr Handle
            {
                get
                {
                    return handle;
                }
            }

            public void Dispose()
            {
                if (this.handle != IntPtr.Zero)
                {
                    Marshal.Release(this.handle);
                    this.handle = IntPtr.Zero;

                    if (this.stream != null)
                    {
                        this.stream.Dispose();
                        this.stream = null;
                    }
                }
            }
        }

        private class DataObjectContainer : System.Windows.Forms.DataObject
        {
            protected const TYMED TYMED_ANY = TYMED.TYMED_ENHMF | TYMED.TYMED_FILE | TYMED.TYMED_GDI | TYMED.TYMED_HGLOBAL | TYMED.TYMED_ISTORAGE | TYMED.TYMED_ISTREAM | TYMED.TYMED_MFPICT;

            [Flags]
            protected enum DropDescriptionFlags
            {
                None = 0,
                IsDefault = 1,
                InvalidateRequired = 2
            }

            protected DropDescriptionFlags description;
            protected DataObject data;

            public DataObjectContainer(DataObject data)
                : base(data)
            {
                this.data = data;
            }

            public DataObject Data
            {
                get
                {
                    return this.data;
                }
            }

            public bool IsDescriptionInvalidated
            {
                get
                {
                    return description.HasFlag(DropDescriptionFlags.InvalidateRequired);
                }
                set
                {
                    if (value)
                        description |= DropDescriptionFlags.InvalidateRequired;
                    else
                        description &= ~DropDescriptionFlags.InvalidateRequired;
                }
            }

            public bool IsDescriptionDefault
            {
                get
                {
                    return description.HasFlag(DropDescriptionFlags.IsDefault);
                }
                set
                {
                    if (value)
                        description |= DropDescriptionFlags.IsDefault;
                    else
                        description &= ~DropDescriptionFlags.IsDefault;
                }
            }

            public bool IsDescriptionValid
            {
                get
                {
                    return this.Description.type >= 0;
                }
            }

            public DropDescription Description
            {
                get
                {
                    FORMATETC formatETC;
                    FillFormatETC(CFSTR_DROPDESCRIPTION, TYMED.TYMED_HGLOBAL, out formatETC);

                    if (data.QueryGetData(ref formatETC) == 0)
                    {
                        STGMEDIUM medium;
                        data.GetData(ref formatETC, out medium);

                        try
                        {
                            return (DropDescription)Marshal.PtrToStructure(medium.unionmember, typeof(DropDescription));
                        }
                        finally
                        {
                            NativeMethods.ReleaseStgMedium(ref medium);
                        }
                    }

                    return new DropDescription()
                        {
                            type = Int32.MinValue
                        };
                }
                set
                {
                    FORMATETC formatETC;
                    FillFormatETC(CFSTR_DROPDESCRIPTION, TYMED.TYMED_HGLOBAL, out formatETC);

                    IntPtr _dd = Marshal.AllocHGlobal(Marshal.SizeOf(typeof(DropDescription)));

                    try
                    {
                        Marshal.StructureToPtr(value, _dd, false);

                        STGMEDIUM medium = new STGMEDIUM()
                        {
                            pUnkForRelease = null,
                            tymed = TYMED.TYMED_HGLOBAL,
                            unionmember = _dd
                        };

                        data.SetData(ref formatETC, ref medium, true);
                    }
                    catch
                    {
                        Marshal.DestroyStructure(_dd, typeof(DropDescription));
                        Marshal.FreeHGlobal(_dd);
                        throw;
                    }
                }
            }

            public static DropDescription CreateDescription(DropImageType type, string format, string insert)
            {
                return new DropDescription()
                {
                    type = (int)type,
                    szMessage = format,
                    szInsert = insert
                };
            }

            public static DropDescription CreateDescription(System.Windows.Forms.DragDropEffects effect)
            {
                switch (effect)
                {
                    case System.Windows.Forms.DragDropEffects.Copy:
                    case System.Windows.Forms.DragDropEffects.Link:
                    case System.Windows.Forms.DragDropEffects.Move:

                        return CreateDescription((DropImageType)effect, effect.ToString(), "");

                    case System.Windows.Forms.DragDropEffects.None:
                    default:

                        return CreateDescription(DropImageType.None, null, null);
                }
            }

            public DropImageType ImageType
            {
                get
                {
                    try
                    {
                        var t = this.Description.type;
                        if (t == Int32.MinValue)
                            return DropImageType.Invalid;
                        return (DropImageType)t;
                    }
                    catch
                    {
                        return DropImageType.Invalid;
                    }
                }
            }

            protected void FillFormatETC(string format, TYMED tymed, out FORMATETC formatETC)
            {
                formatETC.cfFormat = (short)NativeMethods.RegisterClipboardFormat(format);
                formatETC.dwAspect = DVASPECT.DVASPECT_CONTENT;
                formatETC.lindex = -1;
                formatETC.ptd = IntPtr.Zero;
                formatETC.tymed = tymed;
            }

            public int Advise(IAdviseSink sink, string format, ADVF advf)
            {
                FORMATETC formatETC;
                FillFormatETC(format, TYMED_ANY, out formatETC);

                int connection;
                int hr = data.DAdvise(ref formatETC, advf, sink, out connection);
                if (hr != 0)
                    Marshal.ThrowExceptionForHR(hr);

                return connection;
            }

            public int Advise()
            {
                return Advise(new AdviseSink(this), CFSTR_DROPDESCRIPTION, 0);
            }

            public void Unadvise(int connection)
            {
                data.DUnadvise(connection);
            }

            public bool IsShowingLayered
            {
                get
                {
                    if (base.GetDataPresent(CFSTR_ISSHOWINGLAYERED))
                    {
                        var data = base.GetData(CFSTR_ISSHOWINGLAYERED);
                        if (data != null)
                            return GetBoolean(data);
                    }

                    return false;
                }
            }

            public void InvalidateImage()
            {
                if (base.GetDataPresent(CFSTR_DRAGWINDOW))
                {
                    try
                    {
                        IntPtr hwnd = GetIntPtr(base.GetData(CFSTR_DRAGWINDOW));
                        NativeMethods.PostMessage(hwnd, (uint)WindowMessages.WM_INVALIDATEDRAGIMAGE, IntPtr.Zero, IntPtr.Zero);
                    }
                    catch 
                    {
                    }
                }
            }

            private bool GetBoolean(object data)
            {
                if (data is Stream)
                    return ((Stream)data).ReadByte() == 1;

                return false;
            }

            private IntPtr GetIntPtr(object data)
            {
                byte[] buffer = null;

                //int32 ptr

                if (data is Stream)
                {
                    buffer = new byte[4];
                    if (((Stream)data).Read(buffer, 0, buffer.Length) != buffer.Length)
                        buffer = null;
                }
                else if (data is byte[])
                {
                    buffer = (byte[])data;
                    if (buffer.Length < 4)
                        buffer = null;
                }

                if (buffer == null)
                    throw new ArgumentException("Failed to read IntPtr from " + data.GetType());

                return new IntPtr((buffer[3] << 24) | (buffer[2] << 16) | (buffer[1] << 8) | buffer[0]);
            }

            public void OnGiveFeedback(object sender, System.Windows.Forms.GiveFeedbackEventArgs e)
            {
                var setDefault = false;
                var isDefault = this.IsDescriptionDefault;
                var imageType = DropImageType.Invalid;

                if (isDefault || !this.IsDescriptionValid)
                {
                    imageType = this.ImageType;
                    setDefault = true;
                }

                if (this.IsShowingLayered)
                {
                    e.UseDefaultCursors = false;
                    System.Windows.Forms.Cursor.Current = System.Windows.Forms.Cursors.Arrow;
                }
                else
                {
                    //e.UseDefaultCursors = true;
                }

                if (!isDefault || imageType != DropImageType.None || this.IsDescriptionInvalidated)
                {
                    this.InvalidateImage();
                    this.IsDescriptionInvalidated = false;
                }

                if (setDefault && (DropImageType)e.Effect != imageType)
                {
                    switch (e.Effect)
                    {
                        case System.Windows.Forms.DragDropEffects.Copy:
                        case System.Windows.Forms.DragDropEffects.Link:
                        case System.Windows.Forms.DragDropEffects.Move:

                            this.Description = CreateDescription((DropImageType)e.Effect, e.Effect.ToString(), "");

                            break;
                        case System.Windows.Forms.DragDropEffects.None:

                            this.Description = CreateDescription(DropImageType.None, null, null);

                            break;
                    }

                    this.IsDescriptionDefault = true;
                    this.IsDescriptionInvalidated = true;
                }
            }
        }

        public class DataObject : IDataObject, IDisposable
        {
            public event EventHandler BeginDrop;
            public event EventHandler EndDrop;
            public event EventHandler<int> BeginGetContent;

            protected const int S_OK = 0;
            protected const int S_FALSE = 1;
            protected const int OLE_E_ADVISENOTSUPPORTED = unchecked((int)0x80040003);
            protected const int DV_E_FORMATETC = unchecked((int)0x80040064);
            protected const int DV_E_TYMED = unchecked((int)0x80040069);
            protected const int DV_E_CLIPFORMAT = unchecked((int)0x8004006A);
            protected const int DV_E_DVASPECT = unchecked((int)0x8004006B);

            protected const ADVF ADVF_ALLOWED = ADVF.ADVF_NODATA | ADVF.ADVF_ONLYONCE | ADVF.ADVF_PRIMEFIRST;

            [ComVisible(true)]
            protected class EnumFORMATETC : IEnumFORMATETC
            {
                private FORMATETC[] formats;
                private int index = 0;

                internal EnumFORMATETC(IEnumerable<StoredData> storage, int count)
                {
                    int i = 0;
                    formats = new FORMATETC[count];
                    foreach (var data in storage)
                    {
                        formats[i++] = data.format;
                        if (i == count)
                            break;
                    }
                }

                private EnumFORMATETC()
                {
                }

                public void Clone(out IEnumFORMATETC newEnum)
                {
                    EnumFORMATETC ret = new EnumFORMATETC()
                    {
                        index = index,
                        formats = new FORMATETC[formats.Length]
                    };
                    formats.CopyTo(ret.formats, 0);
                    newEnum = ret;
                }

                public int Reset()
                {
                    index = 0;
                    return S_OK;
                }

                public int Skip(int celt)
                {
                    if (index + celt > formats.Length)
                        return S_FALSE;
                    index += celt;
                    return S_OK;
                }

                public int Next(int celt, FORMATETC[] rgelt, int[] pceltFetched)
                {
                    if (celt <= 0 || rgelt == null || index >= formats.Length
                        || celt != 1 && (pceltFetched == null || pceltFetched.Length < 1))
                    {
                        if (pceltFetched != null && pceltFetched.Length > 0)
                            pceltFetched[0] = 0;

                        return S_FALSE;
                    }

                    int count = celt;

                    for (int i = 0; index < formats.Length && count > 0; i++, count--, index++)
                        rgelt[i] = formats[index];

                    if (pceltFetched != null && pceltFetched.Length > 0)
                        pceltFetched[0] = celt - count;

                    return (count == 0) ? S_OK : S_FALSE;
                }
            }

            protected class AdviseEntry
            {
                public FORMATETC format;
                public ADVF advf;
                public IAdviseSink sink;

                public AdviseEntry(ref FORMATETC format, ADVF advf, IAdviseSink sink)
                {
                    this.format = format;
                    this.advf = advf;
                    this.sink = sink;
                }
            }

            protected class StoredData : IDisposable
            {
                public StoredData(ref FORMATETC format, ref STGMEDIUM medium, bool release)
                {
                    this.format = format;
                    this.medium = medium;
                    this.release = release;
                }

                public FORMATETC format;
                public STGMEDIUM medium;
                public bool release;

                public void Dispose()
                {
                    if (release)
                    {
                        NativeMethods.ReleaseStgMedium(ref medium);
                        release = false;
                    }
                }
            }

            protected StoredData[] _storage;
            protected int _index, _count;

            protected IDictionary<int, AdviseEntry> connections;
            protected int _connection = 1;

            public DataObject()
            {
                connections = new Dictionary<int, AdviseEntry>();
                _storage = new StoredData[_count = 10];
            }

            ~DataObject()
            {
                Dispose(false);
            }

            public int EnumDAdvise(out IEnumSTATDATA enumAdvise)
            {
                enumAdvise = null;
                return OLE_E_ADVISENOTSUPPORTED;
            }

            public int GetCanonicalFormatEtc(ref FORMATETC formatIn, out FORMATETC formatOut)
            {
                formatOut = formatIn;
                return DV_E_FORMATETC;
            }

            public int DAdvise(ref FORMATETC pFormatetc, ADVF advf, IAdviseSink adviseSink, out int connection)
            {
                // Check that the specified advisory flags are supported.
                if ((int)((advf | ADVF_ALLOWED) ^ ADVF_ALLOWED) != 0)
                {
                    connection = 0;
                    return OLE_E_ADVISENOTSUPPORTED;
                }

                // Create and insert an entry for the connection list
                AdviseEntry entry = new AdviseEntry(ref pFormatetc, advf, adviseSink);
                connections.Add(_connection, entry);
                connection = _connection;
                _connection++;

                // If the ADVF_PRIMEFIRST flag is specified and the data exists,
                // raise the DataChanged event now.
                if ((advf & ADVF.ADVF_PRIMEFIRST) == ADVF.ADVF_PRIMEFIRST)
                {
                    StoredData data;
                    if (GetIndex(ref pFormatetc, out data) != -1)
                        RaiseDataChanged(connection, data);
                }

                return S_OK;
            }

            public void DUnadvise(int connection)
            {
                connections.Remove(connection);
            }

            public void GetData(ref FORMATETC format, out STGMEDIUM medium)
            {
                StoredData data;
                if (GetIndex(ref format, out data) != -1)
                {
                    if (format.cfFormat == CF_FILEDESCRIPTOR)
                    {
                        if (BeginDrop != null)
                            BeginDrop(this, EventArgs.Empty);
                    }
                    else if (format.cfFormat == CF_FILECONTENTS)
                    {
                        if (BeginGetContent != null)
                            BeginGetContent(this, format.lindex);
                    }

                    medium = new STGMEDIUM();
                    int hr = NativeMethods.CopyStgMedium(ref data.medium, ref medium);
                    if (hr != 0)
                        throw Marshal.GetExceptionForHR(hr);
                }
                else
                {
                    Marshal.ThrowExceptionForHR(DV_E_FORMATETC);
                    medium = new STGMEDIUM();
                }
            }

            public void GetDataHere(ref FORMATETC format, ref STGMEDIUM medium)
            {
                StoredData data;
                if (GetIndex(ref format, out data) != -1)
                {
                    if (format.cfFormat == CF_FILEDESCRIPTOR)
                    {
                        if (BeginDrop != null)
                            BeginDrop(this, EventArgs.Empty);
                    }
                    else if (format.cfFormat == CF_FILECONTENTS)
                    {
                        if (BeginGetContent != null)
                            BeginGetContent(this, format.lindex);
                    }

                    int hr = NativeMethods.CopyStgMedium(ref data.medium, ref medium);
                    if (hr != 0)
                        throw Marshal.GetExceptionForHR(hr);
                }
                else
                {
                    throw Marshal.GetExceptionForHR(DV_E_FORMATETC);
                }
            }

            protected int GetIndex(ref FORMATETC format, out StoredData data)
            {
                for (var i = 0; i < _index; i++)
                {
                    data = _storage[i];
                    if (IsFormatCompatible(ref format, ref data.format))
                        return i;
                }

                data = null;
                return -1;
            }

            protected void ExpandStorage()
            {
                lock (this)
                {
                    if (_count <= _index)
                    {
                        var storage = new StoredData[_count + 10];
                        Array.Copy(_storage, storage, _count);
                        _storage = storage;
                        _count = storage.Length;
                    }
                }
            }

            public void SetData(ref FORMATETC format, ref STGMEDIUM medium, bool release)
            {
                //warning: rapidly called while dragging

                var data = new StoredData(ref format, ref medium, release);

                lock (this)
                {
                    StoredData existing;
                    var i = GetIndex(ref format, out existing);

                    if (i == -1)
                    {
                        i = _index;
                        if (i >= _count)
                            ExpandStorage();
                        _storage[i] = data;
                        _index++;
                    }
                    else
                    {
                        _storage[i] = data;
                        using (existing) { }
                    }
                }

                RaiseDataChanged(data);

                if (format.cfFormat == CF_PERFORMEDDROPEFFECT)
                {
                    if (EndDrop != null)
                        EndDrop(this, EventArgs.Empty);
                }
            }

            public int QueryGetData(ref FORMATETC format)
            {
                if ((DVASPECT.DVASPECT_CONTENT & format.dwAspect) == 0)
                    return DV_E_DVASPECT;

                for (var i = 0; i < _index; i++)
                {
                    var data = _storage[i];
                    if ((format.tymed & data.format.tymed) > 0 && format.cfFormat == data.format.cfFormat)
                        return 0;
                }

                return DV_E_FORMATETC;
            }

            public IEnumFORMATETC EnumFormatEtc(DATADIR direction)
            {
                if (direction == DATADIR.DATADIR_GET)
                    return new EnumFORMATETC(_storage, _index);

                throw new NotImplementedException("OLE_S_USEREG");
            }

            protected void Clear()
            {
                for (var i = 0; i < _index; i++)
                {
                    using (_storage[i]) { }
                }
                _index = 0;
            }

            public void Dispose()
            {
                Dispose(true);
            }

            private void Dispose(bool disposing)
            {
                if (disposing)
                {
                    GC.SuppressFinalize(this);
                }

                Clear();
            }

            protected void RaiseDataChanged(int connection, StoredData data)
            {
                AdviseEntry adviseEntry = connections[connection];
                STGMEDIUM medium;
                if ((adviseEntry.advf & ADVF.ADVF_NODATA) != ADVF.ADVF_NODATA)
                    medium = data.medium;
                else
                    medium = default(STGMEDIUM);

                adviseEntry.sink.OnDataChange(ref data.format, ref medium);

                if ((adviseEntry.advf & ADVF.ADVF_ONLYONCE) == ADVF.ADVF_ONLYONCE)
                    connections.Remove(connection);
            }

            protected void RaiseDataChanged(StoredData data)
            {
                foreach (KeyValuePair<int, AdviseEntry> connection in connections)
                {
                    if (IsFormatCompatible(ref connection.Value.format, ref data.format))
                        RaiseDataChanged(connection.Key, data);
                }
            }

            protected bool IsFormatCompatible(ref FORMATETC format1, ref FORMATETC format2)
            {
                return ((format1.tymed & format2.tymed) > 0
                        && format1.dwAspect == format2.dwAspect
                        && format1.cfFormat == format2.cfFormat
                        && format1.lindex == format2.lindex);
            }
        }

        public static DataObject CreateFileDropDataObject(string[] paths)
        {
            DataObject data = new DataObject();

            var format = new FORMATETC()
            {
                cfFormat = (short)DataFormats.GetFormat(DataFormats.FileDrop).Id,
                dwAspect = DVASPECT.DVASPECT_CONTENT,
                lindex = -1,
                ptd = IntPtr.Zero,
                tymed = TYMED.TYMED_HGLOBAL
            };

            STGMEDIUM medium = new STGMEDIUM();
            var converter = new System.Windows.Forms.DataObject();
            converter.SetData(DataFormats.FileDrop, true, paths);
            ((System.Runtime.InteropServices.ComTypes.IDataObject)converter).GetData(ref format, out medium);

            data.SetData(ref format, ref medium, true);

            return data;
        }

        public static DataObject CreateVirtualFileDropDataObject(IList<FileDescriptor> files)
        {
            DataObject data = new DataObject();
            MemoryStream stream = new MemoryStream(files.Count * 256);

            var buffer = GetBytes(new FILEGROUPDESCRIPTOR() { cItems = (uint)files.Count });
            stream.Write(buffer, 0, buffer.Length);

            foreach (var file in files)
            {
                var fd = new FILEDESCRIPTOR
                {
                    cFileName = file.Name
                };

                #region FileTime (not used)

                //fd.dwFlags |= FD_CREATETIME | FD_WRITESTIME;
                //var changeTime = file.ChangeTimeUtc.Value.ToLocalTime().ToFileTime();
                //var changeTimeFileTime = new System.Runtime.InteropServices.ComTypes.FILETIME
                //{
                //    dwLowDateTime = (int)(changeTime & 0xffffffff),
                //    dwHighDateTime = (int)(changeTime >> 32),
                //};
                //fd.ftLastWriteTime = changeTimeFileTime;
                //fd.ftCreationTime = changeTimeFileTime;

                #endregion

                long length = file.Length;
                if (length != -1)
                {
                    fd.dwFlags |= FileDescriptorFlags.FD_FILESIZE;
                    fd.nFileSizeLow = (uint)(length & 0xffffffff);
                    fd.nFileSizeHigh = (uint)(length >> 32);
                }

                buffer = GetBytes(fd);
                stream.Write(buffer, 0, buffer.Length);
            }

            var format = new FORMATETC()
            {
                cfFormat = CF_FILEDESCRIPTOR,
                dwAspect = DVASPECT.DVASPECT_CONTENT,
                lindex = -1,
                ptd = IntPtr.Zero,
                tymed = TYMED.TYMED_HGLOBAL
            };

            IntPtr ptr = Marshal.AllocHGlobal((int)stream.Length);
            Marshal.Copy(stream.GetBuffer(), 0, ptr, (int)stream.Length);

            STGMEDIUM medium = new STGMEDIUM()
            {
                tymed = TYMED.TYMED_HGLOBAL,
                unionmember = ptr
            };

            data.SetData(ref format, ref medium, true);

            int index = 0;

            #region Content as HGLOBAL

            //foreach (var file in files)
            //{
            //    format = new FORMATETC()
            //    {
            //        cfFormat = CF_FILE_CONTENTS,
            //        dwAspect = DVASPECT.DVASPECT_CONTENT,
            //        lindex = index++,
            //        ptd = IntPtr.Zero,
            //        tymed = TYMED.TYMED_HGLOBAL
            //    };

            //    ptr = Marshal.AllocHGlobal(file.Content.Length);
            //    Marshal.Copy(file.Content, 0, ptr, file.Content.Length);

            //    medium = new STGMEDIUM()
            //    {
            //        tymed = TYMED.TYMED_HGLOBAL,
            //        unionmember = ptr
            //    };

            //    data.SetData(ref format, ref medium, true);
            //}

            #endregion

            #region Content as IStream

            foreach (var file in files)
            {
                file.Index = index;

                format = new FORMATETC()
                {
                    cfFormat = CF_FILECONTENTS,
                    dwAspect = DVASPECT.DVASPECT_CONTENT,
                    lindex = index++,
                    ptd = IntPtr.Zero,
                    tymed = TYMED.TYMED_ISTREAM
                };

                medium = new STGMEDIUM()
                {
                    tymed = TYMED.TYMED_ISTREAM,
                    unionmember = new IStreamContainer(file).Handle
                };

                data.SetData(ref format, ref medium, true);
            }

            #endregion

            return data;
        }

        public static DataObject CreateShellIDListDataObject(string[] paths)
        {
            DataObject data = new DataObject();

            #region Alloc PIDLs

            int length = paths.Length * 1024, position = 0, offset = 4 * paths.Length + 8;
            IntPtr ptr = Marshal.AllocHGlobal(length);

            //[int:count][int:root offset][int:pidl offset 1-n][int:root][bytes:pidl 1-n]

            Marshal.WriteInt32(ptr, paths.Length);
            Marshal.WriteInt32(ptr + 4, offset);
            offset += 4;
            position += 8;

            IntPtr[] ptrs = new IntPtr[paths.Length];
            ushort[] sizes = new ushort[paths.Length];
            int i = 0;

            foreach (var path in paths)
            {
                var pidl = ptrs[i] = NativeMethods.ILCreateFromPath(path);
                var size = sizes[i] = (ushort)NativeMethods.ILGetSize(pidl);

                Marshal.WriteInt32(ptr + position, offset);
                position += 4;

                offset += size;
                i++;
            }

            Marshal.WriteInt32(ptr + position, 0); //root
            position += 4;

            for (i = 0; i < ptrs.Length; i++)
            {
                var pidl = ptrs[i];
                var size = sizes[i];

                if (position + size > length)
                {
                    var c = (ptrs.Length - i);
                    if (c > 1)
                        c *= size;
                    else
                        c = position + size - length;
                    ptr = Marshal.ReAllocHGlobal(ptr, (IntPtr)(length = length + c));
                }

                NativeMethods.CopyMemory(ptr + position, pidl, size);
                position += size;

                NativeMethods.ILFree(pidl);
            }

            if (position != length)
                ptr = Marshal.ReAllocHGlobal(ptr, (IntPtr)(length = position));

            #endregion

            var format = new FORMATETC()
            {
                cfFormat = (short)DataFormats.GetFormat(CFSTR_SHELLIDLIST).Id,
                dwAspect = DVASPECT.DVASPECT_CONTENT,
                lindex = -1,
                ptd = IntPtr.Zero,
                tymed = TYMED.TYMED_HGLOBAL
            };

            STGMEDIUM medium = new STGMEDIUM()
            {
                tymed = TYMED.TYMED_HGLOBAL,
                unionmember = ptr
            };

            data.SetData(ref format, ref medium, true);

            return data;
        }

        private static byte[] GetBytes(object o)
        {
            var t = o.GetType();
            var size = Marshal.SizeOf(t);
            var ptr = Marshal.AllocHGlobal(size);
            var bytes = new byte[size];
            try
            {
                Marshal.StructureToPtr(o, ptr, false);
                Marshal.Copy(ptr, bytes, 0, size);
            }
            finally
            {
                Marshal.DestroyStructure(ptr, t);
                Marshal.FreeHGlobal(ptr);
            }
            return bytes;
        }

        private static void AllowDropDescriptions(bool allow)
        {
            IDragSourceHelper2 sourceHelper = (IDragSourceHelper2)new DragDropHelper();
            sourceHelper.SetFlags(allow ? 1 : 0);
            Marshal.ReleaseComObject(sourceHelper);
        }

        public static System.Windows.Forms.DragDropEffects DoDragDrop(System.Windows.Forms.Control sender, DataObject data, System.Windows.Forms.DragDropEffects allowedEffects, bool allowDescriptions, Bitmap image, Point imageOffset, Color imageColorKey)
        {
            DataObjectContainer container = new DataObjectContainer(data);

            var advise = container.Advise();

            sender.Focus();

            sender.QueryContinueDrag += control_QueryContinueDrag;
            sender.GiveFeedback += container.OnGiveFeedback;

            //EventHandler onLostFocus = null;
            //onLostFocus = delegate
            //{
            //    //sender.DoDragDrop()
            //};

            //sender.LostFocus += onLostFocus;

            AllowDropDescriptions(allowDescriptions);

            ShDragImage _image = new ShDragImage()
            {
                sizeDragImage = image.Size,
                ptOffset = imageOffset,
                crColorKey = imageColorKey.ToArgb(),
                hbmpDragImage = image.GetHbitmap()
            };

            IDragSourceHelper sourceHelper = null;
            try
            {
                sourceHelper = (IDragSourceHelper)new DragDropHelper();
                sourceHelper.InitializeFromBitmap(ref _image, container);

                var result = sender.DoDragDrop(container, allowedEffects);

                container.Unadvise(advise);

                return result;
            }
            finally
            {
                sender.QueryContinueDrag -= control_QueryContinueDrag;
                sender.GiveFeedback -= container.OnGiveFeedback;

                if (sourceHelper != null)
                    Marshal.ReleaseComObject(sourceHelper);
                NativeMethods.DeleteObject(_image.hbmpDragImage);
            }
        }

        static void control_QueryContinueDrag(object sender, System.Windows.Forms.QueryContinueDragEventArgs e)
        {
            //note, losing focus can cause DoDragDrop to hang, example: start drag > windows key > drag over start menu > drag back to form and release
            if (e.EscapePressed || sender is System.Windows.Forms.Control && !((System.Windows.Forms.Control)sender).Focused)
                e.Action = System.Windows.Forms.DragAction.Cancel;
        }

        public static DragHelperInstance Initialize(System.Windows.Forms.Form form)
        {
            return new DragHelperInstance(form);
        }
    }
}
