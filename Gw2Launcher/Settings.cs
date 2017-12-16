using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using System.Windows.Forms;
using System.IO;

namespace Gw2Launcher
{
    public static class Settings
    {
        public const string ASSET_HOST = "assetcdn.101.arenanetworks.com";
        public const string ASSET_COOKIE = "authCookie=access=/latest/*!/manifest/program/*!/program/*~md5=4e51ad868f87201ad93e428ff30c6691";

        private const ushort WRITE_DELAY = 10000;
        private const string FILE_NAME = "settings.dat";
        private static readonly byte[] HEADER;
        private const ushort VERSION = 2;
        private static readonly Type[] FORMS;

        public enum SortMode
        {
            None = 0,
            Name = 1,
            Account = 2,
            LastUsed = 3
        }

        public enum SortOrder
        {
            Ascending = 0,
            Descending = 1
        }

        public enum ScreenAnchor
        {
            Top = 0,
            Bottom = 1,
            Left = 2,
            Right = 3,
            TopLeft = 4,
            TopRight = 5,
            BottomLeft = 6,
            BottomRight = 7
        }

        public interface IAccount
        {
            /// <summary>
            /// Unique identifier
            /// </summary>
            ushort UID
            {
                get;
            }

            /// <summary>
            /// Displayed name
            /// </summary>
            string Name
            {
                get;
                set;
            }

            /// <summary>
            /// Name of a Windows account to use
            /// </summary>
            string WindowsAccount
            {
                get;
                set;
            }

            /// <summary>
            /// The last time (in UTC) this account was used
            /// </summary>
            DateTime LastUsedUtc
            {
                get;
                set;
            }

            /// <summary>
            /// The date (in UTC) this account was created
            /// </summary>
            DateTime CreatedUtc
            {
                get;
                set;
            }

            /// <summary>
            /// Command line arguments
            /// </summary>
            string Arguments
            {
                get;
                set;
            }

            /// <summary>
            /// True if using -windowed mode
            /// </summary>
            bool Windowed
            {
                get;
                set;
            }

            /// <summary>
            /// The bounds of the -windowed mode window
            /// </summary>
            Rectangle WindowBounds
            {
                get;
                set;
            }

            /// <summary>
            /// The name of the owned Local.dat file or null if this account
            /// uses whatever is available
            /// </summary>
            IDatFile DatFile
            {
                get;
                set;
            }

            /// <summary>
            /// The number of times the account has been launched
            /// </summary>
            ushort TotalUses
            {
                get;
                set;
            }

            /// <summary>
            /// Shows an indicator when the account's last used date is before the daily reset
            /// </summary>
            bool ShowDaily
            {
                get;
                set;
            }

            /// <summary>
            /// Records the launch and exit times
            /// </summary>
            bool RecordLaunches
            {
                get;
                set;
            }

            /// <summary>
            /// Uses the options -nopatchui, -email and -password
            /// </summary>
            bool AutomaticLogin
            {
                get;
            }

            string AutomaticLoginEmail
            {
                get;
                set;
            }

            string AutomaticLoginPassword
            {
                get;
                set;
            }

            bool VolumeEnabled
            {
                get;
                set;
            }

            float Volume
            {
                get;
                set;
            }

            string RunAfterLaunching
            {
                get;
                set;
            }
        }

        public interface IDatFile
        {
            ushort UID
            {
                get;
            }

            string Path
            {
                get;
                set;
            }

            bool IsInitialized
            {
                get;
                set;
            }
        }

        public interface IKeyedProperty<TKey,TValue>
        {
            event EventHandler<KeyValuePair<TKey, ISettingValue<TValue>>> ValueChanged;

            bool Contains(TKey key);

            ISettingValue<TValue> this[TKey key]
            {
                get;
            }

            int Count
            {
                get;
            }

            TKey[] GetKeys();
        }

        public interface IHashSetProperty<T>
        {
            event EventHandler<KeyValuePair<T, bool>> ValueChanged;

            bool this[T item]
            {
                get;
                set;
            }
        }

        public interface ISettingValue<T>
        {
            event EventHandler ValueChanged;

            T Value
            {
                get;
                set;
            }

            bool HasValue
            {
                get;
            }

            void Clear();
        }

        public interface IKeyedSettingValue<TKey, TValue> : ISettingValue<TValue>
        {
            TKey Key
            {
                get;
            }
        }

        private class KeyedProperty<TKey, TValue> : IKeyedProperty<TKey, TValue>, IEnumerable<KeyValuePair<TKey, ISettingValue<TValue>>>
        {
            public event EventHandler<KeyValuePair<TKey, ISettingValue<TValue>>> ValueChanged;

            private Dictionary<TKey, ISettingValue<TValue>> _dictionary;
            private Func<TKey, ISettingValue<TValue>> onNewKey;
            
            public KeyedProperty()
            {
                _dictionary = new Dictionary<TKey, ISettingValue<TValue>>();
            }

            public KeyedProperty(IEqualityComparer<TKey> comparer)
            {
                _dictionary = new Dictionary<TKey, ISettingValue<TValue>>(comparer);
            }

            public KeyedProperty(IEqualityComparer<TKey> comparer, Func<TKey, ISettingValue<TValue>> onNewKey)
                : this(comparer)
            {
                this.onNewKey = onNewKey;
            }

            public KeyedProperty(Func<TKey, ISettingValue<TValue>> onNewKey)
                : this()
            {
                this.onNewKey = onNewKey;
            }

            public virtual ISettingValue<TValue> OnCreateNewValue(TKey key)
            {
                if (onNewKey != null)
                    return onNewKey(key);
                return new SettingValue<TValue>();
            }

            public void Add(TKey key, ISettingValue<TValue> value)
            {
                lock (this)
                {
                    _dictionary.Add(key, value);
                }
            }

            public bool Remove(TKey key)
            {
                bool removed;

                lock (this)
                {
                    removed = _dictionary.Remove(key);
                }

                if (removed)
                {
                    return true;
                }

                return false;
            }

            public bool Contains(TKey key)
            {
                lock (this)
                {
                    return _dictionary.ContainsKey(key);
                }
            }

            public bool TryGetValue(TKey key, out ISettingValue<TValue> value)
            {
                lock (this)
                {
                    return _dictionary.TryGetValue(key, out value);
                }
            }

            public int Count
            {
                get
                {
                    lock (this)
                    {
                        return _dictionary.Count;
                    }
                }
            }

            public ISettingValue<TValue> this[TKey key]
            {
                get
                {
                    bool changed = false;
                    ISettingValue<TValue> v;
                    lock (this)
                    {
                        if (!_dictionary.TryGetValue(key, out v))
                        {
                            v = OnCreateNewValue(key);
                            _dictionary.Add(key, v);
                            changed = true;
                        }
                    }

                    if (changed && ValueChanged != null)
                        ValueChanged(this, new KeyValuePair<TKey, ISettingValue<TValue>>(key, v));

                    return v;
                }
                set
                {
                    bool changed = false;

                    lock (this)
                    {
                        ISettingValue<TValue> v;
                        if (_dictionary.TryGetValue(key, out v))
                        {
                            if (!v.Equals(value))
                            {
                                _dictionary[key] = value;
                                changed = true;
                            }
                        }
                        else
                        {
                            _dictionary.Add(key, value);
                            changed = true;
                        }
                    }

                    if (changed)
                    {
                        OnValueChanged();
                        if (ValueChanged != null)
                            ValueChanged(this, new KeyValuePair<TKey, ISettingValue<TValue>>(key, value));
                    }
                }
            }

            public Dictionary<TKey, ISettingValue<TValue>>.KeyCollection Keys
            {
                get
                {
                    return _dictionary.Keys;
                }
            }

            IEnumerator<KeyValuePair<TKey, ISettingValue<TValue>>> IEnumerable<KeyValuePair<TKey, ISettingValue<TValue>>>.GetEnumerator()
            {
                return _dictionary.GetEnumerator();
            }

            System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
            {
                return _dictionary.GetEnumerator();
            }

            public TKey[] GetKeys()
            {
                return _dictionary.Keys.ToArray<TKey>();
            }

            public void Clear()
            {
                _dictionary.Clear();
            }
        }

        private class HashSetProperty<T> : IHashSetProperty<T>, IEnumerable<T>
        {
            public event EventHandler<KeyValuePair<T, bool>> ValueChanged;

            private HashSet<T> _hashset;

            public HashSetProperty()
            {
                _hashset = new HashSet<T>();
            }

            public bool Add(T item)
            {
                lock (this)
                {
                    return _hashset.Add(item);
                }
            }

            public bool Remove(T item)
            {
                lock (this)
                {
                    return _hashset.Remove(item);
                }
            }

            public bool Contains(T item)
            {
                return _hashset.Contains(item);
            }

            public int Count
            {
                get
                {
                    lock (this)
                    {
                        return _hashset.Count;
                    }
                }
            }

            public bool this[T item]
            {
                get
                {
                    return Contains(item);
                }
                set
                {
                    if (value)
                    {
                        if (Add(item))
                        {
                            OnValueChanged();
                            if (ValueChanged != null)
                                ValueChanged(this, new KeyValuePair<T, bool>(item, true));
                        }
                    }
                    else
                    {
                        if (Remove(item))
                        {
                            OnValueChanged();
                            if (ValueChanged != null)
                                ValueChanged(this, new KeyValuePair<T, bool>(item, false));
                        }
                    }
                }
            }

            public IEnumerator<T> GetEnumerator()
            {
                return _hashset.GetEnumerator();
            }

            System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
            {
                return _hashset.GetEnumerator();
            }
        }

        private class SettingValue<T> : ISettingValue<T>
        {
            public event EventHandler ValueChanged;

            protected T value;
            protected bool hasValue;

            public SettingValue()
            {

            }

            public SettingValue(T value)
            {
                this.value = value;
                this.hasValue = true;
            }

            protected void OnValueChanged()
            {
                if (ValueChanged != null)
                    ValueChanged(this, EventArgs.Empty);
                Settings.OnValueChanged();
            }

            public T Value
            {
                get
                {
                    return this.value;
                }
                set
                {
                    if (!this.HasValue || !this.value.Equals(value))
                    {
                        this.hasValue = true;
                        this.value = value;

                        OnValueChanged();
                    }
                }
            }

            public void Clear()
            {
                if (this.hasValue)
                {
                    this.hasValue = false;
                    this.value = default(T);

                    OnValueChanged();
                }
            }

            public bool HasValue
            {
                get
                {
                    return hasValue;
                }
            }

            public void SetValue(T value)
            {
                this.value = value;
                this.hasValue = true;
            }
        }

        private class KeyedSettingValue<TKey, TValue> : SettingValue<TValue>, IKeyedSettingValue<TKey, TValue>
        {
            public KeyedSettingValue(TKey key)
            {
                this.Key = key;
            }

            public TKey Key
            {
                get;
                protected set;
            }

            public byte ReferenceCount
            {
                get;
                private set;
            }

            public void AddReference(object o)
            {
                lock (this)
                {
                    ReferenceCount++;
                }
            }

            public void RemoveReference(object o)
            {
                lock(this)
                {
                    ReferenceCount--;
                }
            }
        }

        private class Account : IAccount
        {
            public Account(ushort uid) : this()
            {
                this.UID = uid;
            }

            public Account()
            {
            }

            public ushort _UID;
            public ushort UID
            {
                get
                {
                    return _UID;
                }
                set
                {
                    if (_UID != value)
                    {
                        _UID = value;
                        OnValueChanged();
                    }
                }
            }

            public string _Name;
            public string Name
            {
                get
                {
                    return _Name;
                }
                set
                {
                    if (_Name != value)
                    {
                        _Name = value;
                        OnValueChanged();
                    }
                }
            }

            public string _WindowsAccount;
            public string WindowsAccount
            {
                get
                {
                    return _WindowsAccount;
                }
                set
                {
                    if (_WindowsAccount != value)
                    {
                        _WindowsAccount = value;
                        OnValueChanged();
                    }
                }
            }

            public DateTime _LastUsedUtc;
            public DateTime LastUsedUtc
            {
                get
                {
                    return _LastUsedUtc;
                }
                set
                {
                    if (_LastUsedUtc != value)
                    {
                        _LastUsedUtc = value;
                        OnValueChanged();
                    }
                }
            }

            public string _Arguments;
            public string Arguments
            {
                get
                {
                    return _Arguments;
                }
                set
                {
                    if (_Arguments != value)
                    {
                        _Arguments = value;
                        OnValueChanged();
                    }
                }
            }

            public bool _Windowed;
            public bool Windowed
            {
                get
                {
                    return _Windowed;
                }
                set
                {
                    if (_Windowed != value)
                    {
                        _Windowed = value;
                        OnValueChanged();
                    }
                }
            }

            public Rectangle _WindowedBounds;
            public Rectangle WindowBounds
            {
                get
                {
                    return _WindowedBounds;
                }
                set
                {
                    if (_WindowedBounds != value)
                    {
                        _WindowedBounds = value;
                        OnValueChanged();
                    }
                }
            }

            public DatFile _DatFile;
            public IDatFile DatFile
            {
                get
                {
                    return _DatFile;
                }
                set
                {
                    if (_DatFile != value)
                    {
                        if (_DatFile != null)
                        {
                            lock (_DatFile)
                            {
                                _DatFile.ReferenceCount--;
                            }
                        }
                        if (value != null)
                        {
                            lock (value)
                            {
                                ((DatFile)value).ReferenceCount++;
                            }
                        }

                        _DatFile = (DatFile)value;
                        OnValueChanged();
                    }
                }
            }

            public ushort _TotalUses;
            public ushort TotalUses
            {
                get
                {
                    return _TotalUses;
                }
                set
                {
                    if (_TotalUses != value)
                    {
                        _TotalUses = value;
                        OnValueChanged();
                    }
                }
            }

            public bool _ShowDaily;
            public bool ShowDaily
            {
                get
                {
                    return _ShowDaily;
                }
                set
                {
                    if (_ShowDaily != value)
                    {
                        _ShowDaily = value;
                        OnValueChanged();
                    }
                }
            }

            public bool _RecordLaunches;
            public bool RecordLaunches
            {
                get
                {
                    return _RecordLaunches;
                }
                set
                {
                    if (_RecordLaunches != value)
                    {
                        _RecordLaunches = value;
                        OnValueChanged();
                    }
                }
            }

            public bool AutomaticLogin
            {
                get
                {
                    return _AutomaticLoginEmail != null && _AutomaticLoginPassword != null;
                }
            }

            public string _AutomaticLoginEmail;
            public string AutomaticLoginEmail
            {
                get
                {
                    return _AutomaticLoginEmail;
                }
                set
                {
                    if (_AutomaticLoginEmail != value)
                    {
                        _AutomaticLoginEmail = value;
                        OnValueChanged();
                    }
                }
            }

            public string _AutomaticLoginPassword;
            public string AutomaticLoginPassword
            {
                get
                {
                    return _AutomaticLoginPassword;
                }
                set
                {
                    if (_AutomaticLoginPassword != value)
                    {
                        _AutomaticLoginPassword = value;
                        OnValueChanged();
                    }
                }
            }
            
            public DateTime _CreatedUtc;
            public DateTime CreatedUtc
            {
                get
                {
                    return _CreatedUtc;
                }
                set
                {
                    if (_CreatedUtc != value)
                    {
                        _CreatedUtc = value;
                        OnValueChanged();
                    }
                }
            }

            public bool VolumeEnabled
            {
                get
                {
                    return _Volume != 0;
                }
                set
                {
                    if (!value)
                        _Volume = 0;
                    else if (_Volume == 0)
                        _Volume = 1;
                }
            }

            //volume is stored with a +1 offset, where 0 is disabled
            public byte _Volume;
            public float Volume
            {
                get
                {
                    if (_Volume > 0)
                        return (_Volume - 1) / 254f;
                    else
                        return _Volume;
                }
                set
                {
                    byte v;
                    if (value >= 1)
                        v = 255;
                    else
                        v = (byte)(value * 254 + 1);
                    if (_Volume != v)
                    {
                        _Volume = v;
                        OnValueChanged();
                    }
                }
            }

            public string _RunAfterLaunching;
            public string RunAfterLaunching
            {
                get
                {
                    return _RunAfterLaunching;
                }
                set
                {
                    if (_RunAfterLaunching != value)
                    {
                        _RunAfterLaunching = value;
                        OnValueChanged();
                    }
                }
            }
        }

        private class DatFile : IDatFile
        {
            public DatFile(ushort uid)
            {
                this.UID = uid;
            }

            public DatFile()
            {

            }

            public byte ReferenceCount;

            public ushort _UID;
            public ushort UID
            {
                get
                {
                    return _UID;
                }
                set
                {
                    if (_UID != value)
                    {
                        _UID = value;
                        OnValueChanged();
                    }
                }
            }

            public string _Path;
            public string Path
            {
                get
                {
                    return _Path;
                }
                set
                {
                    if (_Path != value)
                    {
                        _Path = value;
                        OnValueChanged();
                    }
                }
            }

            public bool _IsInitialized;
            public bool IsInitialized
            {
                get
                {
                    return _IsInitialized;
                }
                set
                {
                    if (_IsInitialized != value)
                    {
                        _IsInitialized = value;
                        OnValueChanged();
                    }
                }
            }
        }

        public struct Values<T1, T2>
        {
            public T1 value1;
            public T2 value2;
        }

        public struct LastCheckedVersion
        {
            public DateTime lastCheck;
            public ushort version;
        }

        public struct ScreenAttachment
        {
            public byte screen;
            public ScreenAnchor anchor;
        }

        private static object _lock = new object();
        private static System.Threading.CancellationTokenSource cancelWrite;
        private static Task task;
        private static DateTime _lastModified;
        private static ushort _accountUID;
        private static ushort _datUID;

        static Settings()
        {
            HEADER = new byte[] { 41, 229, 122, 91, 23 };

            cancelWrite = new System.Threading.CancellationTokenSource();

            _WindowBounds = new KeyedProperty<Type, Rectangle>();
            _Accounts = new KeyedProperty<ushort, IAccount>();
            _GW2Path = new SettingValue<string>();
            _SortingMode = new SettingValue<SortMode>();
            _SortingOrder = new SettingValue<SortOrder>();
            _GW2Arguments = new SettingValue<string>();
            _ShowTray = new SettingValue<bool>();
            _MinimizeToTray = new SettingValue<bool>();
            _BringToFrontOnExit = new SettingValue<bool>();
            _StoreCredentials = new SettingValue<bool>();
            _DeleteCacheOnLaunch = new SettingValue<bool>();
            _HiddenUserAccounts = new KeyedProperty<string, bool>(StringComparer.OrdinalIgnoreCase);
            _LastKnownBuild = new SettingValue<int>();
            _FontSmall = new SettingValue<Font>();
            _FontLarge = new SettingValue<Font>();
            _ShowAccount = new SettingValue<bool>();
            _DatFiles = new KeyedProperty<ushort, IDatFile>(
                new Func<ushort,ISettingValue<IDatFile>>(
                    delegate (ushort key)
                    {
                        return new SettingValue<IDatFile>(new DatFile(key));
                    }));
            _RunAfterLaunching = new SettingValue<string>();
            _LocalAssetServerEnabled = new SettingValue<bool>();
            _Volume = new SettingValue<float>();
            _BackgroundPatchingEnabled = new SettingValue<bool>();
            _BackgroundPatchingNotifications = new SettingValue<ScreenAttachment>();
            _BackgroundPatchingLang = new SettingValue<byte>();
            _BackgroundPatchingMaximumThreads = new SettingValue<byte>();
            _PatchingSpeedLimit = new SettingValue<int>();
            _PatchingUseHttps = new SettingValue<bool>();
            _AutoUpdate = new SettingValue<bool>();
            _AutoUpdateInterval = new SettingValue<ushort>();
            _LastProgramVersion = new SettingValue<LastCheckedVersion>();
            _CheckForNewBuilds = new SettingValue<bool>();

            FORMS = new Type[]
            {
                typeof(UI.formMain)
            };
        }

        public static void Load()
        {
            string path = Path.Combine(DataPath.AppData, FILE_NAME);
            try
            {
                var tmp = new FileInfo(path + ".tmp");

                if (tmp.Exists && tmp.Length > 0)
                {
                    try
                    {
                        Load(path + ".tmp");
                        try
                        {
                            if (File.Exists(path))
                                File.Delete(path);
                            File.Move(path + ".tmp", path);
                        }
                        catch (Exception ex)
                        {
                            Util.Logging.Log(ex);
                        }
                    }
                    catch (Exception ex)
                    {
                        Util.Logging.Log(ex);
                        Load(path);
                    }
                }
                else
                    Load(path);
            }
            catch (Exception e)
            {
                Util.Logging.Log(e);
                SetDefaults();
            }
        }

        public static void SetDefaults()
        {
            _StoreCredentials.SetValue(true);
            _ShowTray.SetValue(true);
            _GW2Arguments.SetValue("-autologin");

        }

        private static void OnValueChanged()
        {
            DelayedWrite();
        }

        private static void DelayedWrite()
        {
            lock (_lock)
            {
                _lastModified = DateTime.UtcNow;
                if (task == null || task.IsCompleted)
                {
                    var cancel = cancelWrite.Token;

                    task = Task.Factory.StartNew(new Action(
                        delegate
                        {
                            DelayedWrite(cancel);
                        }));
                }
            }
        }

        private static void DelayedWrite(System.Threading.CancellationToken cancel)
        {
            while (true)
            {
                while (true)
                {
                    int remaining = WRITE_DELAY - (int)DateTime.UtcNow.Subtract(_lastModified).TotalMilliseconds;
                    if (remaining > 0)
                    {
                        if (cancel.WaitHandle.WaitOne(remaining))
                        {
                            _lastModified = DateTime.MinValue;
                            break;
                        }
                    }
                    else
                    {
                        lock (_lock)
                        {
                            remaining = WRITE_DELAY - (int)DateTime.UtcNow.Subtract(_lastModified).TotalMilliseconds;
                            if (remaining <= 0)
                            {
                                _lastModified = DateTime.MinValue;
                                break;
                            }
                        }
                    }
                }

                var e = Write();

                if (cancel.IsCancellationRequested)
                    return;

                lock (_lock)
                {
                    if (e != null || _lastModified != DateTime.MinValue)
                    {
                        _lastModified = DateTime.UtcNow;
                    }
                    else
                    {
                        task = null;
                        return;
                    }
                }
            }
        }

        public static byte[] CompressBooleans(params bool[] b)
        {
            if (b.Length == 0)
                return new byte[0];

            byte[] bytes = new byte[(b.Length - 1) / 8 + 1];
            int p = 0;
            byte bit = 7;

            for (int i = 0; i < b.Length; i++)
            {
                if (b[i])
                {
                    bytes[p] |= (byte)(1 << bit);
                }

                if (bit == 0)
                {
                    bit = 7;
                    p++;
                }
                else
                    bit--;
            }

            return bytes;
        }

        public static bool[] ExpandBooleans(byte[] bytes)
        {
            bool[] bools = new bool[bytes.Length * 8];

            for (int i = 0; i < bytes.Length;i++)
            {
                var b = bytes[i];
                if (b > 0)
                {
                    int p = i * 8;
                    for (int j = 0; j < 8; j++)
                    {
                        bools[p + j] = (b >> 7 - j & 1) == 1;
                    }
                }
            }

            return bools;
        }

        private static Exception Write()
        {
            try
            {
                string path = Path.Combine(DataPath.AppData, FILE_NAME);

                using (BinaryWriter writer = new BinaryWriter(new BufferedStream(File.Open(path + ".tmp", FileMode.Create, FileAccess.Write, FileShare.Read))))
                {
                    writer.Write(HEADER);
                    writer.Write(VERSION);

                    bool[] booleans;

                    lock (_WindowBounds)
                    {
                        var count = _WindowBounds.Count;
                        var items = new KeyValuePair<Type, Rectangle>[count];
                        int i = 0;

                        foreach (var key in _WindowBounds.Keys)
                        {
                            if (i == count)
                                break;
                            var o = _WindowBounds[key];
                            if (o.HasValue)
                            {
                                items[i++] = new KeyValuePair<Type, Rectangle>(key, o.Value);
                            }
                        }

                        count = i;

                        writer.Write((byte)count);

                        for (i = 0; i < count; i++)
                        {
                            var item = items[i];

                            writer.Write(GetWindowID(item.Key));

                            writer.Write(item.Value.X);
                            writer.Write(item.Value.Y);
                            writer.Write(item.Value.Width);
                            writer.Write(item.Value.Height);
                        }
                    }

                    booleans = new bool[]
                    {
                        //v1-HasValue
                        _SortingMode.HasValue && _SortingMode.Value != default(SortMode),
                        _SortingOrder.HasValue && _SortingOrder.Value != default(SortOrder),
                        _StoreCredentials.HasValue,
                        _ShowTray.HasValue,
                        _MinimizeToTray.HasValue,
                        _BringToFrontOnExit.HasValue,
                        _DeleteCacheOnLaunch.HasValue,
                        _GW2Path.HasValue && !string.IsNullOrWhiteSpace(_GW2Path.Value),
                        _GW2Arguments.HasValue && !string.IsNullOrWhiteSpace(_GW2Arguments.Value),
                        _LastKnownBuild.HasValue,
                        _FontLarge.HasValue,
                        _FontSmall.HasValue,
                        _ShowAccount.HasValue,

                        //v1-Values from:13
                        _StoreCredentials.Value,
                        _ShowTray.Value,
                        _MinimizeToTray.Value,
                        _BringToFrontOnExit.Value,
                        _DeleteCacheOnLaunch.Value,
                        _ShowAccount.Value,
                        
                        //v2-HasValue from:19
                        _CheckForNewBuilds.HasValue,
                        _LastProgramVersion.HasValue,
                        _AutoUpdateInterval.HasValue,
                        _AutoUpdate.HasValue,
                        _BackgroundPatchingEnabled.HasValue,
                        _BackgroundPatchingLang.HasValue,
                        _BackgroundPatchingNotifications.HasValue,
                        _BackgroundPatchingMaximumThreads.HasValue,
                        _PatchingSpeedLimit.HasValue,
                        _PatchingUseHttps.HasValue,
                        _RunAfterLaunching.HasValue,
                        _Volume.HasValue,
                        _LocalAssetServerEnabled.HasValue,

                        //v2-Values from:32
                        _CheckForNewBuilds.Value,
                        _AutoUpdate.Value,
                        _BackgroundPatchingEnabled.Value,
                        _LocalAssetServerEnabled.Value,
                        _PatchingUseHttps.Value
                    };

                    byte[] b = CompressBooleans(booleans);

                    writer.Write((byte)b.Length);
                    writer.Write(b);

                    if (booleans[0])
                        writer.Write((byte)_SortingMode.Value);
                    if (booleans[1])
                        writer.Write((byte)_SortingOrder.Value);
                    if (booleans[7])
                        writer.Write(_GW2Path.Value);
                    if (booleans[8])
                        writer.Write(_GW2Arguments.Value);
                    if (booleans[9])
                        writer.Write(_LastKnownBuild.Value);
                    if (booleans[10])
                    {
                        try
                        {
                            writer.Write(new FontConverter().ConvertToString(_FontLarge.Value));
                        }
                        catch (Exception e)
                        {
                            Util.Logging.Log(e);
                            writer.Write("");
                        }
                    }
                    if (booleans[11])
                    {
                        try
                        {
                            writer.Write(new FontConverter().ConvertToString(_FontSmall.Value));
                        }
                        catch (Exception e)
                        {
                            Util.Logging.Log(e);
                            writer.Write("");
                        }
                    }

                    //v2
                    if (booleans[20])
                    {
                        var v = _LastProgramVersion.Value;
                        writer.Write(v.lastCheck.ToBinary());
                        writer.Write(v.version);
                    }
                    if (booleans[21])
                        writer.Write(_AutoUpdateInterval.Value);
                    if (booleans[24])
                        writer.Write(_BackgroundPatchingLang.Value);
                    if (booleans[25])
                    {
                        var v = _BackgroundPatchingNotifications.Value;
                        writer.Write((byte)v.screen);
                        writer.Write((byte)v.anchor);
                    }
                    if (booleans[26])
                        writer.Write(_BackgroundPatchingMaximumThreads.Value);
                    if (booleans[27])
                        writer.Write(_PatchingSpeedLimit.Value);
                    if (booleans[29])
                        writer.Write(_RunAfterLaunching.Value);
                    if (booleans[30])
                        writer.Write((byte)(_Volume.Value * 255));

                    lock(_DatFiles)
                    {
                        var count = _DatFiles.Count;
                        var items = new KeyValuePair<ushort, DatFile>[count];
                        int i = 0;

                        foreach (var key in _DatFiles.Keys)
                        {
                            if (i == count)
                                break;
                            var o = _DatFiles[key];
                            if (o.HasValue && ((DatFile)o.Value).ReferenceCount > 0)
                            {
                                items[i++] = new KeyValuePair<ushort, DatFile>(key, (DatFile)o.Value);
                            }
                        }

                        count = i;

                        writer.Write((ushort)count);

                        for (i = 0; i < count; i++)
                        {
                            var item = items[i];

                            writer.Write(item.Value.UID);
                            if (string.IsNullOrWhiteSpace(item.Value.Path))
                                writer.Write("");
                            else
                                writer.Write(item.Value.Path);
                            writer.Write(item.Value.IsInitialized);
                        }
                    }

                    lock (_Accounts)
                    {
                        var count = _Accounts.Count;
                        var items = new KeyValuePair<ushort, Account>[count];
                        int i = 0;

                        foreach (var key in _Accounts.Keys)
                        {
                            if (i == count)
                                break;
                            var o = _Accounts[key];
                            if (o.HasValue)
                            {
                                items[i++] = new KeyValuePair<ushort, Account>(key, (Account)o.Value);
                            }
                        }

                        count = i;

                        writer.Write((ushort)count);

                        for (i = 0; i < count; i++)
                        {
                            var item = items[i];

                            var account = item.Value;
                            writer.Write(account.UID);
                            writer.Write(account.Name);
                            writer.Write(account.WindowsAccount);
                            writer.Write(account.CreatedUtc.ToBinary());
                            writer.Write(account.LastUsedUtc.ToBinary());
                            writer.Write(account.TotalUses);
                            writer.Write(account.Arguments);

                            booleans = new bool[]
                            {
                                account.ShowDaily,
                                account.Windowed,
                                account.RecordLaunches,
                                account.AutomaticLogin,
                                account.DatFile != null,
                                account.VolumeEnabled,
                                !string.IsNullOrEmpty(account.RunAfterLaunching)
                            };

                            b = CompressBooleans(booleans);

                            writer.Write((byte)b.Length);
                            writer.Write(b);

                            if (booleans[4])
                                writer.Write(account.DatFile.UID);

                            writer.Write(account.WindowBounds.X);
                            writer.Write(account.WindowBounds.Y);
                            writer.Write(account.WindowBounds.Width);
                            writer.Write(account.WindowBounds.Height);

                            if (booleans[3])
                            {
                                writer.Write(account.AutomaticLoginEmail);
                                writer.Write(account.AutomaticLoginPassword);
                            }

                            if (booleans[5])
                                writer.Write(account._Volume);

                            if (booleans[6])
                                writer.Write(account.RunAfterLaunching);
                        }
                    }

                    lock(_HiddenUserAccounts)
                    {
                        var count = _HiddenUserAccounts.Count;
                        var items = new string[count];
                        int i = 0;

                        foreach (var key in _HiddenUserAccounts.Keys)
                        {
                            if (i == count)
                                break;
                            var o = _HiddenUserAccounts[key];
                            if (o.HasValue && o.Value)
                            {
                                items[i++] = key;
                            }
                        }

                        count = i;

                        writer.Write((ushort)count);

                        for (i = 0; i < count; i++)
                        {
                            var item = items[i];
                            writer.Write(item);
                        }
                    }
                }

                var tmp = new FileInfo(path + ".tmp");
                if (tmp.Length > 0)
                {
                    if (File.Exists(path))
                        File.Delete(path);
                    File.Move(path + ".tmp", path);
                }
            }
            catch (Exception e)
            {
                Util.Logging.Log(e);
                return e;
            }

            return null;
        }

        private static void Load(string path)
        {
            using (BinaryReader reader = new BinaryReader(new BufferedStream(File.Open(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))))
            {
                byte[] header = reader.ReadBytes(HEADER.Length);
                if (!Compare(HEADER, header))
                    throw new IOException("Invalid header");
                ushort version = reader.ReadUInt16();

                lock (_WindowBounds)
                {
                    _WindowBounds.Clear();

                    var count = reader.ReadByte();
                    for (int i = 0; i < count; i++)
                    {
                        Type t = GetWindow(reader.ReadByte());
                        Rectangle r = new Rectangle(reader.ReadInt32(), reader.ReadInt32(), reader.ReadInt32(), reader.ReadInt32());

                        if (t != null && !r.IsEmpty)
                        {
                            SettingValue<Rectangle> item = new SettingValue<Rectangle>();
                            item.SetValue(r);
                            _WindowBounds.Add(t, item);
                        }
                    }
                }

                byte[] b = reader.ReadBytes(reader.ReadByte());
                bool[] booleans = ExpandBooleans(b);

                if (booleans[0])
                    _SortingMode.SetValue((SortMode)reader.ReadByte());
                else
                    _SortingMode.Clear();

                if (booleans[1])
                    _SortingOrder.SetValue((SortOrder)reader.ReadByte());
                else
                    _SortingOrder.Clear();

                if (booleans[2])
                    _StoreCredentials.SetValue(booleans[13]);
                else
                    _StoreCredentials.Clear();

                if (booleans[3])
                    _ShowTray.SetValue(booleans[14]);
                else
                    _ShowTray.Clear();

                if (booleans[4])
                    _MinimizeToTray.SetValue(booleans[15]);
                else
                    _MinimizeToTray.Clear();

                if (booleans[5])
                    _BringToFrontOnExit.SetValue(booleans[16]);
                else
                    _BringToFrontOnExit.Clear();

                if (booleans[6])
                    _DeleteCacheOnLaunch.SetValue(booleans[17]);
                else
                    _DeleteCacheOnLaunch.Clear();

                if (booleans[7])
                    _GW2Path.SetValue(reader.ReadString());
                else
                    _GW2Path.Clear();

                if (booleans[8])
                    _GW2Arguments.SetValue(reader.ReadString());
                else
                    _GW2Arguments.Clear();

                if (booleans[9])
                    _LastKnownBuild.SetValue(reader.ReadInt32());
                else
                    _LastKnownBuild.Clear();

                if (booleans[10])
                {
                    Font font = null;
                    try
                    {
                        font = new FontConverter().ConvertFromString(reader.ReadString()) as Font;
                    }
                    catch (Exception ex)
                    {
                        Util.Logging.Log(ex);
                    }
                    if (font != null)
                        _FontLarge.SetValue(font);
                    else
                        _FontLarge.Clear();
                }
                else
                    _FontLarge.Clear();

                if (booleans[11])
                {
                    Font font = null;
                    try
                    {
                        font = new FontConverter().ConvertFromString(reader.ReadString()) as Font;
                    }
                    catch (Exception ex)
                    {
                        Util.Logging.Log(ex);
                    }
                    if (font != null)
                        _FontSmall.SetValue(font);
                    else
                        _FontSmall.Clear();
                }
                else
                    _FontSmall.Clear();

                if (booleans[12])
                    _ShowAccount.SetValue(booleans[18]);
                else
                    _ShowAccount.Clear();

                if (version >= 2)
                {
                    if (booleans[19])
                        _CheckForNewBuilds.SetValue(booleans[32]);
                    else
                        _CheckForNewBuilds.Clear();

                    if (booleans[20])
                    {
                        _LastProgramVersion.SetValue(new LastCheckedVersion()
                            {
                                lastCheck = DateTime.FromBinary(reader.ReadInt64()),
                                version = reader.ReadUInt16()
                            });
                    }
                    else
                    {
                        _LastProgramVersion.Clear();
                    }

                    if (booleans[21])
                        _AutoUpdateInterval.SetValue(reader.ReadUInt16());
                    else
                        _AutoUpdateInterval.Clear();

                    if (booleans[22])
                        _AutoUpdate.SetValue(booleans[33]);
                    else
                        _AutoUpdate.Clear();

                    if (booleans[23])
                        _BackgroundPatchingEnabled.SetValue(booleans[34]);
                    else
                        _BackgroundPatchingEnabled.Clear();

                    if (booleans[24])
                        _BackgroundPatchingLang.SetValue(reader.ReadByte());
                    else
                        _BackgroundPatchingLang.Clear();

                    if (booleans[25])
                    {
                        _BackgroundPatchingNotifications.SetValue(new ScreenAttachment()
                            {
                                screen = reader.ReadByte(),
                                anchor = (ScreenAnchor)reader.ReadByte()
                            });
                    }
                    else
                        _BackgroundPatchingNotifications.Clear();

                    if (booleans[26])
                        _BackgroundPatchingMaximumThreads.SetValue(reader.ReadByte());
                    else
                        _BackgroundPatchingMaximumThreads.Clear();

                    if (booleans[27])
                        _PatchingSpeedLimit.SetValue(reader.ReadInt32());
                    else
                        _PatchingSpeedLimit.Clear();

                    if (booleans[28])
                        _PatchingUseHttps.SetValue(booleans[36]);
                    else
                        _PatchingUseHttps.Clear();

                    if (booleans[29])
                        _RunAfterLaunching.SetValue(reader.ReadString());
                    else
                        _RunAfterLaunching.Clear();

                    if (booleans[30])
                        _Volume.SetValue(reader.ReadByte() / 255f);
                    else
                        _Volume.Clear();

                    if (booleans[31])
                        _LocalAssetServerEnabled.SetValue(booleans[35]);
                    else
                        _LocalAssetServerEnabled.Clear();
                }

                _datUID = 0;

                lock (_DatFiles)
                {
                    _DatFiles.Clear();

                    var count = reader.ReadUInt16();
                    for (int i = 0; i < count; i++)
                    {
                        var s = new DatFile();
                        s._UID = reader.ReadUInt16();
                        s._Path = reader.ReadString();
                        s._IsInitialized = reader.ReadBoolean();

                        _DatFiles.Add(s.UID, new SettingValue<IDatFile>(s));

                        if (_datUID < s.UID)
                            _datUID = s.UID;
                    }
                }

                _accountUID = 0;

                lock (_Accounts)
                {
                    _Accounts.Clear();

                    var count = reader.ReadUInt16();
                    for (int i = 0; i < count; i++)
                    {
                        Account account = new Account();
                        account._UID = reader.ReadUInt16();
                        account._Name = reader.ReadString();
                        account._WindowsAccount = reader.ReadString();
                        account._CreatedUtc = DateTime.FromBinary(reader.ReadInt64());
                        account._LastUsedUtc = DateTime.FromBinary(reader.ReadInt64());
                        account._TotalUses = reader.ReadUInt16();
                        account._Arguments = reader.ReadString();

                        b = reader.ReadBytes(reader.ReadByte());
                        booleans = ExpandBooleans(b);

                        account._ShowDaily = booleans[0];
                        account._Windowed = booleans[1];
                        account._RecordLaunches = booleans[2];

                        if (booleans[4])
                        {
                            account._DatFile = (DatFile)_DatFiles[reader.ReadUInt16()].Value;
                            account._DatFile.ReferenceCount++;
                        }

                        account._WindowedBounds = new Rectangle(reader.ReadInt32(), reader.ReadInt32(), reader.ReadInt32(), reader.ReadInt32());

                        if (booleans[3])
                        {
                            account._AutomaticLoginEmail = reader.ReadString();
                            account._AutomaticLoginPassword = reader.ReadString();
                        }

                        if (version >= 2)
                        {
                            if (booleans[5])
                                account._Volume = reader.ReadByte();
                            if (booleans[6])
                                account._RunAfterLaunching = reader.ReadString();
                        }

                        SettingValue<IAccount> item = new SettingValue<IAccount>();
                        item.SetValue(account);
                        _Accounts.Add(account._UID, item);

                        if (_accountUID < account._UID)
                            _accountUID = account._UID;
                    }
                }

                lock (_HiddenUserAccounts)
                {
                    _HiddenUserAccounts.Clear();

                    var count = reader.ReadUInt16();
                    for (int i = 0; i < count; i++)
                    {
                        _HiddenUserAccounts.Add(reader.ReadString(), new SettingValue<bool>(true));
                    }
                }

                #region Upgrade old versions

                if (version == 1)
                {
                    var args = _GW2Arguments.Value;
                    if (!string.IsNullOrEmpty(args))
                    {
                        if (args.IndexOf("-l:assetsrv") != -1)
                        {
                            Settings._LocalAssetServerEnabled.SetValue(true);
                            Settings._GW2Arguments.SetValue(Util.Args.AddOrReplace(args, "l:assetsrv", ""));
                        }
                    }

                    if (Settings._LastKnownBuild.HasValue)
                        Settings._CheckForNewBuilds.SetValue(true);
                }

                #endregion
            }
        }

        #region Historical v1 I/O

        //private static void LoadV1(BinaryReader reader)
        //{
        //    lock (_WindowBounds)
        //    {
        //        _WindowBounds.Clear();

        //        var count = reader.ReadByte();
        //        for (int i = 0; i < count; i++)
        //        {
        //            Type t = GetWindow(reader.ReadByte());
        //            Rectangle r = new Rectangle(reader.ReadInt32(), reader.ReadInt32(), reader.ReadInt32(), reader.ReadInt32());

        //            if (t != null && !r.IsEmpty)
        //            {
        //                SettingValue<Rectangle> item = new SettingValue<Rectangle>();
        //                item.SetValue(r);
        //                _WindowBounds.Add(t, item);
        //            }
        //        }
        //    }

        //    byte[] b = reader.ReadBytes(reader.ReadByte());
        //    bool[] booleans = ExpandBooleans(b);

        //    if (booleans[0])
        //        _SortingMode.SetValue((SortMode)reader.ReadByte());
        //    else
        //        _SortingMode.Clear();

        //    if (booleans[1])
        //        _SortingOrder.SetValue((SortOrder)reader.ReadByte());
        //    else
        //        _SortingOrder.Clear();

        //    if (booleans[2])
        //        _StoreCredentials.SetValue(booleans[13]);
        //    else
        //        _StoreCredentials.Clear();

        //    if (booleans[3])
        //        _ShowTray.SetValue(booleans[14]);
        //    else
        //        _ShowTray.Clear();

        //    if (booleans[4])
        //        _MinimizeToTray.SetValue(booleans[15]);
        //    else
        //        _MinimizeToTray.Clear();

        //    if (booleans[5])
        //        _BringToFrontOnExit.SetValue(booleans[16]);
        //    else
        //        _BringToFrontOnExit.Clear();

        //    if (booleans[6])
        //        _DeleteCacheOnLaunch.SetValue(booleans[17]);
        //    else
        //        _DeleteCacheOnLaunch.Clear();

        //    if (booleans[7])
        //        _GW2Path.SetValue(reader.ReadString());
        //    else
        //        _GW2Path.Clear();

        //    if (booleans[8])
        //        _GW2Arguments.SetValue(reader.ReadString());
        //    else
        //        _GW2Arguments.Clear();

        //    if (booleans[9])
        //        _LastKnownBuild.SetValue(reader.ReadInt32());
        //    else
        //        _LastKnownBuild.Clear();

        //    if (booleans[10])
        //    {
        //        Font font = null;
        //        try
        //        {
        //            font = new FontConverter().ConvertFromString(reader.ReadString()) as Font;
        //        }
        //        catch (Exception ex)
        //        {
        //            Util.Logging.Log(ex);
        //        }
        //        if (font != null)
        //            _FontLarge.Value = font;
        //        else
        //            _FontLarge.Clear();
        //    }
        //    else
        //        _FontLarge.Clear();

        //    if (booleans[11])
        //    {
        //        Font font = null;
        //        try
        //        {
        //            font = new FontConverter().ConvertFromString(reader.ReadString()) as Font;
        //        }
        //        catch (Exception ex)
        //        {
        //            Util.Logging.Log(ex);
        //        }
        //        if (font != null)
        //            _FontSmall.Value = font;
        //        else
        //            _FontSmall.Clear();
        //    }
        //    else
        //        _FontSmall.Clear();

        //    if (booleans[12])
        //        _ShowAccount.Value = booleans[18];
        //    else
        //        _ShowAccount.Clear();

        //    _datUID = 0;

        //    lock (_DatFiles)
        //    {
        //        _DatFiles.Clear();

        //        var count = reader.ReadUInt16();
        //        for (int i = 0; i < count; i++)
        //        {
        //            var s = new DatFile();
        //            s._UID = reader.ReadUInt16();
        //            s._Path = reader.ReadString();
        //            s._IsInitialized = reader.ReadBoolean();

        //            _DatFiles.Add(s.UID, new SettingValue<IDatFile>(s));

        //            if (_datUID < s.UID)
        //                _datUID = s.UID;
        //        }
        //    }

        //    _accountUID = 0;

        //    lock (_Accounts)
        //    {
        //        _Accounts.Clear();

        //        var count = reader.ReadUInt16();
        //        for (int i = 0; i < count; i++)
        //        {
        //            Account account = new Account();
        //            account._UID = reader.ReadUInt16();
        //            account._Name = reader.ReadString();
        //            account._WindowsAccount = reader.ReadString();
        //            account._CreatedUtc = DateTime.FromBinary(reader.ReadInt64());
        //            account._LastUsedUtc = DateTime.FromBinary(reader.ReadInt64());
        //            account._TotalUses = reader.ReadUInt16();
        //            account._Arguments = reader.ReadString();

        //            b = reader.ReadBytes(reader.ReadByte());
        //            booleans = ExpandBooleans(b);

        //            account._ShowDaily = booleans[0];
        //            account._Windowed = booleans[1];
        //            account._RecordLaunches = booleans[2];

        //            if (booleans[4])
        //            {
        //                account._DatFile = (DatFile)_DatFiles[reader.ReadUInt16()].Value;
        //                account._DatFile.ReferenceCount++;
        //            }

        //            account._WindowedBounds = new Rectangle(reader.ReadInt32(), reader.ReadInt32(), reader.ReadInt32(), reader.ReadInt32());

        //            if (booleans[3])
        //            {
        //                account._AutomaticLoginEmail = reader.ReadString();
        //                account._AutomaticLoginPassword = reader.ReadString();
        //            }

        //            SettingValue<IAccount> item = new SettingValue<IAccount>();
        //            item.SetValue(account);
        //            _Accounts.Add(account._UID, item);

        //            if (_accountUID < account._UID)
        //                _accountUID = account._UID;
        //        }
        //    }


        //    lock (_HiddenUserAccounts)
        //    {
        //        _HiddenUserAccounts.Clear();

        //        var count = reader.ReadUInt16();
        //        for (int i = 0; i < count; i++)
        //        {
        //            _HiddenUserAccounts.Add(reader.ReadString(), new SettingValue<bool>(true));
        //        }
        //    }
        //}

        //private static Exception WriteV1()
        //{
        //    try
        //    {
        //        string path = Path.Combine(DataPath.AppData, FILE_NAME);
        //        using (BinaryWriter writer = new BinaryWriter(new BufferedStream(File.Open(path + ".tmp", FileMode.Create, FileAccess.Write, FileShare.Read))))
        //        {
        //            writer.Write(HEADER);
        //            writer.Write(VERSION);

        //            bool[] booleans;

        //            lock (_WindowBounds)
        //            {
        //                var count = _WindowBounds.Count;
        //                var items = new KeyValuePair<Type, Rectangle>[count];
        //                int i = 0;

        //                foreach (var key in _WindowBounds.Keys)
        //                {
        //                    if (i == count)
        //                        break;
        //                    var o = _WindowBounds[key];
        //                    if (o.HasValue)
        //                    {
        //                        items[i++] = new KeyValuePair<Type, Rectangle>(key, o.Value);
        //                    }
        //                }

        //                count = i;

        //                writer.Write((byte)count);

        //                for (i = 0; i < count; i++)
        //                {
        //                    var item = items[i];

        //                    writer.Write(GetWindowID(item.Key));

        //                    writer.Write(item.Value.X);
        //                    writer.Write(item.Value.Y);
        //                    writer.Write(item.Value.Width);
        //                    writer.Write(item.Value.Height);
        //                }
        //            }

        //            booleans = new bool[]
        //            {
        //                //HasValue
        //                _SortingMode.HasValue && _SortingMode.Value != default(SortMode),
        //                _SortingOrder.HasValue && _SortingOrder.Value != default(SortOrder),
        //                _StoreCredentials.HasValue,
        //                _ShowTray.HasValue,
        //                _MinimizeToTray.HasValue,
        //                _BringToFrontOnExit.HasValue,
        //                _DeleteCacheOnLaunch.HasValue,
        //                _GW2Path.HasValue && !string.IsNullOrWhiteSpace(_GW2Path.Value),
        //                _GW2Arguments.HasValue && !string.IsNullOrWhiteSpace(_GW2Arguments.Value),
        //                _LastKnownBuild.HasValue,
        //                _FontLarge.HasValue,
        //                _FontSmall.HasValue,
        //                _ShowAccount.HasValue,

        //                //Values
        //                _StoreCredentials.Value,
        //                _ShowTray.Value,
        //                _MinimizeToTray.Value,
        //                _BringToFrontOnExit.Value,
        //                _DeleteCacheOnLaunch.Value,
        //                _ShowAccount.Value
        //            };

        //            byte[] b = CompressBooleans(booleans);

        //            writer.Write((byte)b.Length);
        //            writer.Write(b);

        //            if (booleans[0])
        //                writer.Write((byte)_SortingMode.Value);
        //            if (booleans[1])
        //                writer.Write((byte)_SortingOrder.Value);
        //            if (booleans[7])
        //                writer.Write(_GW2Path.Value);
        //            if (booleans[8])
        //                writer.Write(_GW2Arguments.Value);
        //            if (booleans[9])
        //                writer.Write(_LastKnownBuild.Value);
        //            if (booleans[10])
        //            {
        //                try
        //                {
        //                    writer.Write(new FontConverter().ConvertToString(_FontLarge.Value));
        //                }
        //                catch (Exception e)
        //                {
        //                    Util.Logging.Log(e);
        //                    writer.Write("");
        //                }
        //            }
        //            if (booleans[11])
        //            {
        //                try
        //                {
        //                    writer.Write(new FontConverter().ConvertToString(_FontSmall.Value));
        //                }
        //                catch (Exception e)
        //                {
        //                    Util.Logging.Log(e);
        //                    writer.Write("");
        //                }
        //            }

        //            lock (_DatFiles)
        //            {
        //                var count = _DatFiles.Count;
        //                var items = new KeyValuePair<ushort, DatFile>[count];
        //                int i = 0;

        //                foreach (var key in _DatFiles.Keys)
        //                {
        //                    if (i == count)
        //                        break;
        //                    var o = _DatFiles[key];
        //                    if (o.HasValue && ((DatFile)o.Value).ReferenceCount > 0)
        //                    {
        //                        items[i++] = new KeyValuePair<ushort, DatFile>(key, (DatFile)o.Value);
        //                    }
        //                }

        //                count = i;

        //                writer.Write((ushort)count);

        //                for (i = 0; i < count; i++)
        //                {
        //                    var item = items[i];

        //                    writer.Write(item.Value.UID);
        //                    if (string.IsNullOrWhiteSpace(item.Value.Path))
        //                        writer.Write("");
        //                    else
        //                        writer.Write(item.Value.Path);
        //                    writer.Write(item.Value.IsInitialized);
        //                }
        //            }

        //            lock (_Accounts)
        //            {
        //                var count = _Accounts.Count;
        //                var items = new KeyValuePair<ushort, Account>[count];
        //                int i = 0;

        //                foreach (var key in _Accounts.Keys)
        //                {
        //                    if (i == count)
        //                        break;
        //                    var o = _Accounts[key];
        //                    if (o.HasValue)
        //                    {
        //                        items[i++] = new KeyValuePair<ushort, Account>(key, (Account)o.Value);
        //                    }
        //                }

        //                count = i;

        //                writer.Write((ushort)count);

        //                for (i = 0; i < count; i++)
        //                {
        //                    var item = items[i];

        //                    var account = item.Value;
        //                    writer.Write(account.UID);
        //                    writer.Write(account.Name);
        //                    writer.Write(account.WindowsAccount);
        //                    writer.Write(account.CreatedUtc.ToBinary());
        //                    writer.Write(account.LastUsedUtc.ToBinary());
        //                    writer.Write(account.TotalUses);
        //                    writer.Write(account.Arguments);

        //                    booleans = new bool[]
        //                    {
        //                        account.ShowDaily,
        //                        account.Windowed,
        //                        account.RecordLaunches,
        //                        account.AutomaticLogin,
        //                        account.DatFile != null
        //                    };

        //                    b = CompressBooleans(booleans);

        //                    writer.Write((byte)b.Length);
        //                    writer.Write(b);

        //                    if (booleans[4])
        //                    {
        //                        writer.Write(account.DatFile.UID);
        //                    }

        //                    writer.Write(account.WindowBounds.X);
        //                    writer.Write(account.WindowBounds.Y);
        //                    writer.Write(account.WindowBounds.Width);
        //                    writer.Write(account.WindowBounds.Height);

        //                    if (booleans[3])
        //                    {
        //                        writer.Write(account.AutomaticLoginEmail);
        //                        writer.Write(account.AutomaticLoginPassword);
        //                    }
        //                }
        //            }

        //            lock (_HiddenUserAccounts)
        //            {
        //                var count = _HiddenUserAccounts.Count;
        //                var items = new string[count];
        //                int i = 0;

        //                foreach (var key in _HiddenUserAccounts.Keys)
        //                {
        //                    if (i == count)
        //                        break;
        //                    var o = _HiddenUserAccounts[key];
        //                    if (o.HasValue && o.Value)
        //                    {
        //                        items[i++] = key;
        //                    }
        //                }

        //                count = i;

        //                writer.Write((ushort)count);

        //                for (i = 0; i < count; i++)
        //                {
        //                    var item = items[i];
        //                    writer.Write(item);
        //                }
        //            }
        //        }

        //        var tmp = new FileInfo(path + ".tmp");
        //        if (tmp.Length > 0)
        //        {
        //            if (File.Exists(path))
        //                File.Delete(path);
        //            File.Move(path + ".tmp", path);
        //        }
        //    }
        //    catch (Exception e)
        //    {
        //        Util.Logging.Log(e);
        //        return e;
        //    }

        //    return null;
        //}

        #endregion

        private static bool Compare(byte[] a, byte[] b)
        {
            if (a.Length != b.Length)
                return false;

            for (int i = a.Length - 1; i >= 0; i--)
            {
                if (a[i] != b[i])
                    return false;
            }

            return true;
        }

        private static Type GetWindow(byte id)
        {
            if (id < FORMS.Length)
                return FORMS[id];

            return null;
        }

        private static byte GetWindowID(Type t)
        {
            for (byte i = 0; i < FORMS.Length; i++)
            {
                if (t == FORMS[i])
                    return i;
            }

            return 0;
        }

        private static SettingValue<string> _GW2Path;
        public static ISettingValue<string> GW2Path
        {
            get
            {
                return _GW2Path;
            }
        }

        private static SettingValue<string> _GW2Arguments;
        public static ISettingValue<string> GW2Arguments
        {
            get
            {
                return _GW2Arguments;
            }
        }

        private static SettingValue<Font> _FontLarge;
        public static ISettingValue<Font> FontLarge
        {
            get
            {
                return _FontLarge;
            }
        }

        private static SettingValue<Font> _FontSmall;
        public static ISettingValue<Font> FontSmall
        {
            get
            {
                return _FontSmall;
            }
        }

        private static SettingValue<bool> _ShowAccount;
        public static ISettingValue<bool> ShowAccount
        {
            get
            {
                return _ShowAccount;
            }
        }

        private static SettingValue<bool> _MinimizeToTray;
        public static ISettingValue<bool> MinimizeToTray
        {
            get
            {
                return _MinimizeToTray;
            }
        }

        private static SettingValue<bool> _ShowTray;
        public static ISettingValue<bool> ShowTray
        {
            get
            {
                return _ShowTray;
            }
        }

        private static SettingValue<bool> _BringToFrontOnExit;
        public static ISettingValue<bool> BringToFrontOnExit
        {
            get
            {
                return _BringToFrontOnExit;
            }
        }

        private static KeyedProperty<Type, Rectangle> _WindowBounds;
        public static IKeyedProperty<Type, Rectangle> WindowBounds
        {
            get
            {
                return _WindowBounds;
            }
        }

        private static KeyedProperty<string, bool> _HiddenUserAccounts;
        public static IKeyedProperty<string, bool> HiddenUserAccounts
        {
            get
            {
                return _HiddenUserAccounts;
            }
        }

        private static KeyedProperty<ushort, IAccount> _Accounts;
        public static IKeyedProperty<ushort, IAccount> Accounts
        {
            get
            {
                return _Accounts;
            }
        }

        private static KeyedProperty<ushort, IDatFile> _DatFiles;
        public static IKeyedProperty<ushort, IDatFile> DatFiles
        {
            get
            {
                return _DatFiles;
            }
        }

        private static SettingValue<SortMode> _SortingMode;
        public static ISettingValue<SortMode> SortingMode
        {
            get
            {
                return _SortingMode;
            }
        }

        private static SettingValue<SortOrder> _SortingOrder;
        public static ISettingValue<SortOrder> SortingOrder
        {
            get
            {
                return _SortingOrder;
            }
        }

        private static SettingValue<bool> _StoreCredentials;
        public static ISettingValue<bool> StoreCredentials
        {
            get
            {
                return _StoreCredentials;
            }
        }

        private static SettingValue<bool> _DeleteCacheOnLaunch;
        public static ISettingValue<bool> DeleteCacheOnLaunch
        {
            get
            {
                return _DeleteCacheOnLaunch;
            }
        }


        private static SettingValue<bool> _CheckForNewBuilds;
        public static ISettingValue<bool> CheckForNewBuilds
        {
            get
            {
                return _CheckForNewBuilds;
            }
        }

        private static SettingValue<int> _LastKnownBuild;
        public static ISettingValue<int> LastKnownBuild
        {
            get
            {
                return _LastKnownBuild;
            }
        }

        private static SettingValue<LastCheckedVersion> _LastProgramVersion;
        public static ISettingValue<LastCheckedVersion> LastProgramVersion
        {
            get
            {
                return _LastProgramVersion;
            }
        }

        private static SettingValue<ushort> _AutoUpdateInterval;
        public static ISettingValue<ushort> AutoUpdateInterval
        {
            get
            {
                return _AutoUpdateInterval;
            }
        }

        private static SettingValue<bool> _AutoUpdate;
        public static ISettingValue<bool> AutoUpdate
        {
            get
            {
                return _AutoUpdate;
            }
        }
        
        private static SettingValue<bool> _BackgroundPatchingEnabled;
        public static ISettingValue<bool> BackgroundPatchingEnabled
        {
            get
            {
                return _BackgroundPatchingEnabled;
            }
        }

        private static SettingValue<ScreenAttachment> _BackgroundPatchingNotifications;
        public static ISettingValue<ScreenAttachment> BackgroundPatchingNotifications
        {
            get
            {
                return _BackgroundPatchingNotifications;
            }
        }

        private static SettingValue<byte> _BackgroundPatchingLang;
        public static ISettingValue<byte> BackgroundPatchingLang
        {
            get
            {
                return _BackgroundPatchingLang;
            }
        }

        private static SettingValue<byte> _BackgroundPatchingMaximumThreads;
        public static ISettingValue<byte> BackgroundPatchingMaximumThreads
        {
            get
            {
                return _BackgroundPatchingMaximumThreads;
            }
        }

        private static SettingValue<string> _RunAfterLaunching;
        public static ISettingValue<string> RunAfterLaunching
        {
            get
            {
                return _RunAfterLaunching;
            }
        }

        private static SettingValue<float> _Volume;
        public static ISettingValue<float> Volume
        {
            get
            {
                return _Volume;
            }
        }

        private static SettingValue<bool> _LocalAssetServerEnabled;
        public static ISettingValue<bool> LocalAssetServerEnabled
        {
            get
            {
                return _LocalAssetServerEnabled;
            }
        }

        private static SettingValue<bool> _PatchingUseHttps;
        public static ISettingValue<bool> PatchingUseHttps
        {
            get
            {
                return _PatchingUseHttps;
            }
        }

        private static SettingValue<int> _PatchingSpeedLimit;
        public static ISettingValue<int> PatchingSpeedLimit
        {
            get
            {
                return _PatchingSpeedLimit;
            }
        }

        public static bool DisableAutomaticLogins
        {
            get;
            set;
        }

        //public static List<System.Net.IPAddress> GetDnsServers()
        //{
        //    int count = _DnsServers.Count;
        //    List<System.Net.IPAddress> servers = new List<System.Net.IPAddress>(count == 0 ? 1 : count * 2);

        //    if (count > 0)
        //    {
        //        foreach (var server in Net.DnsServers.Servers)
        //        {
        //            if (_DnsServers.Contains(server.id))
        //            {
        //                servers.AddRange(server.IP);
        //                if (--count == 0)
        //                    break;
        //            }
        //        }
        //    }

        //    if (servers.Count == 0)
        //        servers.AddRange(Net.DnsServers.Servers[0].IP);

        //    return servers;
        //}

        public static bool Silent
        {
            get;
            set;
        }

        public static IAccount CreateAccount()
        {
            lock (_Accounts)
            {
                Account account = new Account(++_accountUID);
                _Accounts.Add(account.UID, new SettingValue<IAccount>(account));
                return account;
            }
        }

        public static IDatFile CreateDatFile()
        {
            lock(_DatFiles)
            {
                var dat = new DatFile(++_datUID);
                _DatFiles.Add(dat.UID, new SettingValue<IDatFile>(dat));
                return dat;
            }
        }

        public static void Save()
        {
            Task t;
            lock(_lock)
            {
                if (task == null)
                    return;
                t = task;
                cancelWrite.Cancel();
            }
            t.Wait();
        }
    }
}
