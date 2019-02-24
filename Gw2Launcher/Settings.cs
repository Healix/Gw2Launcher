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
        private const ushort VERSION = 5;
        private static readonly Type[] FORMS;

        public enum SortMode : byte
        {
            None = 0,
            Name = 1,
            Account = 2,
            LastUsed = 3,
            LaunchTime = 4,
            Custom = 5,
        }

        public enum SortOrder : byte
        {
            Ascending = 0,
            Descending = 1
        }

        public enum ScreenAnchor : byte
        {
            Top = 0,
            Bottom = 1,
            Left = 2,
            Right = 3,
            TopLeft = 4,
            TopRight = 5,
            BottomLeft = 6,
            BottomRight = 7,
            None = 8,
        }

        public enum ButtonAction : byte
        {
            None = 0,
            Focus = 1,
            Close = 2,
            Launch = 3,
            LaunchSingle = 4,
        }

        public enum ScreenshotFormat : byte
        {
            Default = 0,
            Bitmap = 1
        }

        [Flags]
        public enum DailiesMode : byte
        {
            None = 0,
            Show = 1,
            Positioned = 2,
            AutoLoad = 4,
        }

        [Flags]
        public enum MuteOptions : byte
        {
            None = 0,
            All = 1,
            Music = 2,
            Voices = 4,
        }

        public enum NetworkAuthorizationState : byte
        {
            Disabled = 0,
            Unknown = 1,
            OK = 2,
        }

        [Flags]
        public enum NetworkAuthorizationFlags : byte
        {
            None = 0,
            Manual = 1,
            Automatic = 2,
            Always = 4,

            VerificationModes = 7,

            AbortLaunchingOnFail = 64,
            RemovePreviouslyAuthorized = 128,
        }

        public enum ApiCacheState : byte
        {
            None = 0,
            OK = 1,
            Pending = 2,
        }

        public enum ProcessPriorityClass : byte
        {
            None = 0,
            High = 1,
            AboveNormal = 2,
            Normal = 3,
            BelowNormal = 4,
            Low = 5,
        }

        [Flags]
        public enum WindowOptions : byte
        {
            None = 0,
            Windowed = 1,
            RememberChanges = 2,
            PreventChanges = 4,
            TopMost = 8,
        }

        public enum IconType : byte
        {
            None = 0,
            File = 1,
            Gw2LauncherColorKey = 2,
            ColorKey = 3,
        }

        [Flags]
        public enum AccountBarStyles : byte
        {
            None = 0,
            Name = 1,
            Color = 2,
            Icon = 4,
            Exit = 8,
            HighlightFocused = 16,
        }

        [Flags]
        public enum AccountBarOptions : byte
        {
            None = 0,
            HorizontalLayout = 1,
            GroupByActive = 2,
            OnlyShowActive = 4,
            AutoHide = 8,
            TopMost = 16,
        }

        public class Notes : IEnumerable<Notes.Note>
        {
            public class Note : IComparable<Note>, IComparable<DateTime>
            {
                private DateTime expires;
                private ushort sid;
                private bool notify;

                public Note(DateTime expires, ushort sid, bool notify)
                {
                    this.expires = expires;
                    this.sid = sid;
                    this.notify = notify;
                }

                public Note(DateTime expires)
                {
                    this.expires = expires;
                }

                public DateTime Expires
                {
                    get
                    {
                        return expires;
                    }
                    private set
                    {
                        expires = value;
                    }
                }

                public ushort SID
                {
                    get
                    {
                        return sid;
                    }
                    set
                    {
                        if (sid != value)
                        {
                            sid = value;
                            OnValueChanged();
                        }
                    }
                }

                public bool Notify
                {
                    get
                    {
                        return notify;
                    }
                    set
                    {
                        if (notify != value)
                        {
                            notify = value;
                            OnValueChanged();
                        }
                    }
                }

                public int CompareTo(Note other)
                {
                    var c = this.Expires.CompareTo(other.Expires);
                    if (c == 0)
                        c = this.SID.CompareTo(other.SID);
                    return c;
                }

                public int CompareTo(DateTime other)
                {
                    return this.Expires.CompareTo(other);
                }
            }

            public event EventHandler<Note> Added, Removed;

            private Note[] notes;
            private ushort count;

            public Notes(int capacity)
            {
                if (capacity > 0)
                    notes = new Note[capacity];
            }

            /// <summary>
            /// The passed array will be used as the initial buffer
            /// </summary>
            /// <param name="notes">A sorted array of notes</param>
            public Notes(Note[] notes)
            {
                this.notes = notes;
                count = (ushort)notes.Length;
            }

            public Notes()
            {

            }

            public void Add(Note note)
            {
                lock (this)
                {
                    int index;

                    if (count == 0)
                    {
                        if (notes == null)
                            notes = new Note[3];
                        index = 0;
                    }
                    else
                    {
                        index = Array.BinarySearch<Note>(notes, 0, count, note);
                        if (index < 0)
                            index = ~index;

                        if (count == notes.Length)
                        {
                            var copy = new Note[count + 5];
                            Array.Copy(notes, 0, copy, 0, index);
                            if (index < count)
                                Array.Copy(notes, index, copy, index + 1, count - index);
                            notes = copy;
                        }
                        else if (index < count)
                        {
                            Array.Copy(notes, index, notes, index + 1, count - index);
                        }
                    }

                    notes[index] = note;
                    count++;
                }

                OnValueChanged();

                if (Added != null)
                    Added(this, note);
            }

            public void AddRange(Note[] notes)
            {
                lock (this)
                {
                    if (count + notes.Length > this.notes.Length)
                    {
                        var copy = new Note[count + notes.Length];
                        Array.Copy(this.notes, copy, count);
                    }

                    foreach (var n in notes)
                        Add(n);
                }
            }

            public int IndexOf(DateTime date)
            {
                lock (this)
                {
                    if (count == 0)
                        return -1;

                    var index = Array.BinarySearch<Note>(notes, 0, count, new Note(date));
                    if (index < 0)
                        return ~index;
                    return index;
                }
            }

            public int IndexOf(Note note)
            {
                lock (this)
                {
                    if (count == 0)
                        return -1;

                    var index = Array.BinarySearch<Note>(notes, 0, count, note);
                    if (index < 0)
                        return -1;

                    if (object.Equals(notes[index], note))
                        return index;

                    int i = index - 1;
                    Note n;

                    while ((n = notes[i]).CompareTo(note) == 0)
                    {
                        if (object.Equals(n, note))
                            return i;
                        i--;
                    }

                    i = index + 1;

                    while ((n = notes[i]).CompareTo(note) == 0)
                    {
                        if (object.Equals(n, note))
                            return i;
                        i++;
                    }

                    return -1;
                }
            }

            public bool Remove(Note note)
            {
                lock (this)
                {
                    var i = IndexOf(note);
                    if (i == -1)
                        return false;

                    if (i != count - 1)
                    {
                        Array.Copy(notes, i + 1, notes, i, count - i - 1);
                        notes[count - 1] = null;
                    }
                    else if (count == 1)
                    {
                        notes = null;
                    }
                    else
                    {
                        notes[i] = null;
                    }

                    count--;
                }

                OnValueChanged();

                if (Removed != null)
                    Removed(this, note);

                return true;
            }

            public bool RemoveRange(int index, int count)
            {
                Note[] removed;

                lock (this)
                {
                    if (index + count > this.count)
                        return false;

                    if (Removed != null)
                    {
                        removed = new Note[count];
                        Array.Copy(notes, index, removed, 0, count);
                    }
                    else
                        removed = null;

                    if (index + count == this.count)
                    {
                        Array.Clear(notes, index, count);
                    }
                    else
                    {
                        Array.Copy(notes, index + count, notes, index, this.count - index - count);
                        Array.Clear(notes, this.count - count, count);
                    }

                    this.count -= (ushort)count;
                }

                OnValueChanged();

                if (removed != null)
                {
                    foreach (var r in removed)
                    {
                        if (Removed != null)
                            Removed(this, r);
                    }
                }

                return true;
            }

            public void CopyTo(int index, Note[] array, int offset, int count)
            {
                Array.Copy(notes, index, array, offset, count);
            }

            public Note this[int index]
            {
                get
                {
                    return notes[index];
                }
            }

            public DateTime ExpiresLast
            {
                get
                {
                    if (count > 0)
                        return notes[count - 1].Expires;
                    return DateTime.MinValue;
                }
            }

            public DateTime ExpiresFirst
            {
                get
                {
                    if (count > 0)
                        return notes[0].Expires;
                    return DateTime.MinValue;
                }
            }

            public int Count
            {
                get
                {
                    return count;
                }
            }

            public IEnumerator<Note> GetEnumerator()
            {
                for (var i = 0; i < count; i++)
                    yield return notes[i];
            }

            System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
            {
                return GetEnumerator();
            }
        }

        public interface IAccountApiData
        {
            IApiValue<ushort> DailyPoints
            {
                get;
                set;
            }
            IApiValue<int> Played
            {
                get;
                set;
            }
            IApiValue<T> CreateValue<T>();
        }

        public interface IApiValue<T>
        {
            DateTime LastChange
            {
                get;
                set;
            }
            T Value
            {
                get;
                set;
            }
            ApiCacheState State
            {
                get;
                set;
            }
        }

        private class ApiValue<T> : IApiValue<T>
        {
            public DateTime _LastChange;
            public DateTime LastChange
            {
                get
                {
                    return _LastChange;
                }
                set
                {
                    if (_LastChange != value)
                    {
                        _LastChange = value;
                        OnValueChanged();
                    }
                }
            }

            public T _Value;
            public T Value
            {
                get
                {
                    return _Value;
                }
                set
                {
                    if (!_Value.Equals(value))
                    {
                        _Value = value;
                        OnValueChanged();
                    }
                }
            }

            public ApiCacheState _State;
            public ApiCacheState State
            {
                get
                {
                    return _State;
                }
                set
                {
                    if (_State != value)
                    {
                        _State = value;
                        OnValueChanged();
                    }
                }
            }
        }

        private class AccountApiData : IAccountApiData
        {
            public IApiValue<T> CreateValue<T>()
            {
                return new ApiValue<T>();
            }

            public ApiValue<ushort> _DailyPoints;
            public IApiValue<ushort> DailyPoints
            {
                get
                {
                    return _DailyPoints;
                }
                set
                {
                    if (_DailyPoints != value)
                    {
                        _DailyPoints = (ApiValue<ushort>)value;
                        OnValueChanged();
                    }
                }
            }

            public ApiValue<int> _Played;
            public IApiValue<int> Played
            {
                get
                {
                    return _Played;
                }
                set
                {
                    if (_Played != value)
                    {
                        _Played = (ApiValue<int>)value;
                        OnValueChanged();
                    }
                }
            }
        }

        public interface IAccount
        {
            event EventHandler NameChanged,
                               IconChanged,
                               IconTypeChanged,
                               ColorKeyChanged;

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
            }

            /// <summary>
            /// Options for -windowed mode
            /// </summary>
            WindowOptions WindowOptions
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
            /// The name of the owned GFXSettings.xml file or null if this account
            /// uses whatever is available
            /// </summary>
            IGfxFile GfxFile
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
            bool ShowDailyLogin
            {
                get;
                set;
            }

            /// <summary>
            /// Shows an indicator when the daily hasn't been completed
            /// </summary>
            bool ShowDailyCompletion
            {
                get;
                set;
            }

            DateTime LastDailyCompletionUtc
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
            /// Automatically login
            /// </summary>
            bool AutomaticLogin
            {
                get;
                set;
            }

            /// <summary>
            /// Automatically starts the game after logging in
            /// </summary>
            bool AutomaticPlay
            {
                get;
                set;
            }

            /// <summary>
            /// True if the email and password isn't empty
            /// </summary>
            bool HasCredentials
            {
                get;
            }

            /// <summary>
            /// The email for the account
            /// </summary>
            string Email
            {
                get;
                set;
            }

            /// <summary>
            /// The password for the account
            /// </summary>
            System.Security.SecureString Password
            {
                get;
                set;
            }

            /// <summary>
            /// True to set the volume in Windows
            /// </summary>
            bool VolumeEnabled
            {
                get;
                set;
            }

            /// <summary>
            /// Volumne between 0.0 and 1.0
            /// </summary>
            float Volume
            {
                get;
                set;
            }

            /// <summary>
            /// Shell commands to run after launching
            /// </summary>
            string RunAfterLaunching
            {
                get;
                set;
            }

            /// <summary>
            /// Enables the -autologin option
            /// </summary>
            bool AutomaticRememberedLogin
            {
                get;
                set;
            }

            /// <summary>
            /// Enables sounds options
            /// </summary>
            MuteOptions Mute
            {
                get;
                set;
            }

            /// <summary>
            /// Enables the -clientport option with the port
            /// </summary>
            ushort ClientPort
            {
                get;
                set;
            }

            /// <summary>
            /// Enables the option to change the screenshot format
            /// </summary>
            ScreenshotFormat ScreenshotsFormat
            {
                get;
                set;
            }

            /// <summary>
            /// The location where screenshots are saved
            /// </summary>
            string ScreenshotsLocation
            {
                get;
                set;
            }

            /// <summary>
            /// True if any files need to be updated when launching
            /// </summary>
            bool PendingFiles
            {
                get;
                set;
            }

            /// <summary>
            /// API data
            /// </summary>
            IAccountApiData ApiData
            {
                get;
                set;
            }

            /// <summary>
            /// API key for the account
            /// </summary>
            string ApiKey
            {
                get;
                set;
            }

            /// <summary>
            /// The state of the current network (whether it needs to be authenticated)
            /// </summary>
            NetworkAuthorizationState NetworkAuthorizationState
            {
                get;
                set;
            }

            /// <summary>
            /// Authenticator key for the account
            /// </summary>
            byte[] TotpKey
            {
                get;
                set;
            }

            /// <summary>
            /// The priority of the process
            /// </summary>
            ProcessPriorityClass ProcessPriority
            {
                get;
                set;
            }

            /// <summary>
            /// The processor affinity of the process
            /// </summary>
            long ProcessAffinity
            {
                get;
                set;
            }

            /// <summary>
            /// Notes attached to the account
            /// </summary>
            Notes Notes
            {
                get;
                set;
            }

            IAccountApiData CreateApiData();

            /// <summary>
            /// The color identifier for the account
            /// </summary>
            Color ColorKey
            {
                get;
                set;
            }

            /// <summary>
            /// The type of icon for the account
            /// </summary>
            IconType IconType
            {
                get;
                set;
            }

            /// <summary>
            /// The icon for the account
            /// </summary>
            string Icon
            {
                get;
                set;
            }

            /// <summary>
            /// The key used when sorted manually
            /// </summary>
            ushort SortKey
            {
                get;
            }

            /// <summary>
            /// Adjusts the sort key for the account based on the referenced account
            /// </summary>
            /// <param name="reference">The account that will be used as a reference</param>
            /// <param name="type">Where the account should be ordered based on the referenced account</param>
            void Sort(IAccount reference, AccountSorting.SortType type);
        }

        public interface IFile
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

            byte References
            {
                get;
            }
        }

        public interface IDatFile : IFile
        {
        }

        public interface IGfxFile : IFile
        {
        }

        public interface IKeyedProperty<TKey,TValue>
        {
            event EventHandler<KeyValuePair<TKey, ISettingValue<TValue>>> ValueChanged, ValueAdded;
            event EventHandler<TKey> ValueRemoved;

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

        public interface IPendingSettingValue<T> : ISettingValue<T>
        {
            T ValueCommit
            {
                get;
            }

            bool IsPending
            {
                get;
            }

            void Commit();
        }

        public interface ISettingValue<T>
        {
            event EventHandler ValueChanged;
            event EventHandler<T> ValueCleared;

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
            public event EventHandler<KeyValuePair<TKey, ISettingValue<TValue>>> ValueChanged, ValueAdded;
            public event EventHandler<TKey> ValueRemoved;

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

                if (ValueAdded != null)
                    ValueAdded(this, new KeyValuePair<TKey, ISettingValue<TValue>>(key, value));
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
                    if (ValueRemoved != null)
                        ValueRemoved(this, key);

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
            public event EventHandler<T> ValueCleared;

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

            protected virtual void OnValueChanged()
            {
                if (ValueChanged != null)
                    ValueChanged(this, EventArgs.Empty);
                Settings.OnValueChanged();
            }

            public virtual T Value
            {
                get
                {
                    return this.value;
                }
                set
                {
                    if (!this.hasValue || !this.value.Equals(value))
                    {
                        this.hasValue = true;
                        this.value = value;

                        OnValueChanged();
                    }
                }
            }

            public virtual void Clear()
            {
                if (this.hasValue)
                {
                    var v = this.value;

                    this.hasValue = false;
                    this.value = default(T);

                    OnValueChanged();

                    if (ValueCleared != null)
                        ValueCleared(this, v);
                }
            }

            public virtual bool HasValue
            {
                get
                {
                    return hasValue;
                }
            }

            public virtual void SetValue(T value)
            {
                this.value = value;
                this.hasValue = true;
            }
        }

        private class PendingSettingValue<T> : SettingValue<T>, IPendingSettingValue<T>
        {
            protected T valueCommit;
            protected bool isPending;

            public PendingSettingValue()
                : base()
            {
            }

            public PendingSettingValue(T value)
                : base(value)
            {
            }

            public override void Clear()
            {
                if (hasValue && !isPending)
                {
                    isPending = true;
                    valueCommit = value;
                }

                base.Clear();
            }

            public override T Value
            {
                get
                {
                    return base.value;
                }
                set
                {
                    if (!base.hasValue || !object.Equals(base.value, value))
                    {
                        if (this.isPending)
                        {
                            if (object.Equals(value, this.valueCommit))
                            {
                                this.valueCommit = default(T);
                                this.isPending = false;
                            }
                        }
                        else
                        {
                            this.valueCommit = base.value;
                            this.isPending = true;
                        }

                        base.hasValue = true;
                        base.value = value;

                        OnValueChanged();
                    }
                }
            }

            public T ValueCommit
            {
                get
                {
                    if (isPending)
                        return this.valueCommit;
                    else
                        return base.value;
                }
            }

            public void SetCommit(T value)
            {
                isPending = true;
                valueCommit = value;
            }

            public void ClearCommit()
            {
                if (isPending)
                {
                    isPending = false;
                    valueCommit = default(T);
                }
            }

            public bool IsPending
            {
                get
                {
                    return isPending;
                }
            }

            public void Commit()
            {
                if (isPending)
                {
                    valueCommit = default(T);
                    isPending = false;

                    Settings.OnValueChanged();
                }
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

        public static class AccountSorting
        {
            public static event EventHandler SortingChanged;

            public enum SortType
            {
                Before,
                After,
                Swap,
            }

            public static void Sort(IAccount account, IAccount reference, SortType type)
            {
                var _reference = (Account)reference;
                var _account = (Account)account;
                var index = _reference._SortKey;

                switch (type)
                {
                    case AccountSorting.SortType.After:

                        if (_account._SortKey > index)
                            ++index;

                        break;
                    case AccountSorting.SortType.Before:

                        if (_account._SortKey < index)
                            --index;

                        break;
                    case AccountSorting.SortType.Swap:

                        _reference._SortKey = _account._SortKey;
                        _account._SortKey = index;

                        OnValueChanged();

                        if (SortingChanged != null)
                            SortingChanged(null, EventArgs.Empty);

                        return;
                    default:
                        throw new NotSupportedException();
                }

                if (_account._SortKey == index)
                    return;

                if (_account._SortKey > index)
                {
                    foreach (var key in _Accounts.Keys)
                    {
                        var a = (Account)_Accounts[key].Value;
                        if (a != null)
                        {
                            if (a._SortKey >= index && a._SortKey < _account._SortKey)
                                ++a._SortKey;
                        }
                    }
                }
                else
                {
                    foreach (var key in _Accounts.Keys)
                    {
                        var a = (Account)_Accounts[key].Value;
                        if (a != null)
                        {
                            if (a._SortKey <= index && a._SortKey > _account._SortKey)
                                --a._SortKey;
                        }
                    }
                }

                _account._SortKey = index;

                var total = _Accounts.Count;
                total = total * (total + 1) / 2;

                foreach (var key in _Accounts.Keys)
                {
                    var a = (Account)_Accounts[key].Value;
                    if (a != null)
                    {
                        total -= a._SortKey;
                    }
                }

                OnValueChanged();

                if (SortingChanged != null)
                    SortingChanged(null, EventArgs.Empty);
            }
        }

        private class Account : IAccount
        {
            public event EventHandler NameChanged,
                                      IconChanged,
                                      IconTypeChanged,
                                      ColorKeyChanged;

            public Account(ushort uid)
                : this()
            {
                this._UID = uid;
            }

            public Account()
            {
                _LastDailyCompletionUtc = new DateTime(1);
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

                        if (NameChanged != null)
                            NameChanged(this, EventArgs.Empty);
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
                        _PendingFiles = true;
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

            public bool Windowed
            {
                get
                {
                    return _WindowOptions.HasFlag(Settings.WindowOptions.Windowed);
                }
                set
                {
                    if (Windowed != value)
                    {
                        if (value)
                            _WindowOptions |= Settings.WindowOptions.Windowed;
                        else
                            _WindowOptions &= ~Settings.WindowOptions.Windowed;

                        OnValueChanged();
                    }
                }
            }

            public WindowOptions _WindowOptions;
            public WindowOptions WindowOptions
            {
                get
                {
                    return _WindowOptions;
                }
                set
                {
                    if (_WindowOptions != value)
                    {
                        _WindowOptions = value;
                        OnValueChanged();
                    }
                }
            }

            public Rectangle _WindowBounds;
            public Rectangle WindowBounds
            {
                get
                {
                    return _WindowBounds;
                }
                set
                {
                    if (_WindowBounds != value)
                    {
                        _WindowBounds = value;
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
                            _DatFile.PathChanged -= File_PathChanged;
                        }

                        var file = (DatFile)value;
                        if (file != null)
                        {
                            lock (file)
                            {
                                file.ReferenceCount++;
                            }
                            file.PathChanged += File_PathChanged;
                        }

                        _DatFile = file;
                        _PendingFiles = true;
                        OnValueChanged();
                    }
                }
            }

            public GfxFile _GfxFile;
            public IGfxFile GfxFile
            {
                get
                {
                    return _GfxFile;
                }
                set
                {
                    if (_GfxFile != value)
                    {
                        if (_GfxFile != null)
                        {
                            lock (_GfxFile)
                            {
                                _GfxFile.ReferenceCount--;
                            }
                            _GfxFile.PathChanged -= File_PathChanged;
                        }

                        var file = (GfxFile)value;
                        if (file != null)
                        {
                            lock (file)
                            {
                                file.ReferenceCount++;
                            }
                            file.PathChanged += File_PathChanged;
                        }

                        _GfxFile = file;
                        _PendingFiles = true;
                        OnValueChanged();
                    }
                }
            }

            void File_PathChanged(object sender, EventArgs e)
            {
                if (!_PendingFiles)
                {
                    _PendingFiles = true;
                    OnValueChanged();
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

            public bool _ShowDailyLogin;
            public bool ShowDailyLogin
            {
                get
                {
                    return _ShowDailyLogin;
                }
                set
                {
                    if (_ShowDailyLogin != value)
                    {
                        _ShowDailyLogin = value;
                        OnValueChanged();
                    }
                }
            }

            public bool ShowDailyCompletion
            {
                get
                {
                    return _LastDailyCompletionUtc.Ticks != 1;
                }
                set
                {
                    if (ShowDailyCompletion != value)
                    {
                        if (value)
                            _LastDailyCompletionUtc = DateTime.MinValue;
                        else
                            _LastDailyCompletionUtc = new DateTime(1);
                        OnValueChanged();
                    }
                }
            }

            public DateTime _LastDailyCompletionUtc;
            public DateTime LastDailyCompletionUtc
            {
                get
                {
                    return _LastDailyCompletionUtc;
                }
                set
                {
                    if (_LastDailyCompletionUtc != value)
                    {
                        _LastDailyCompletionUtc = value;
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

            public bool _AutomaticLogin;
            public bool AutomaticLogin
            {
                get
                {
                    return _AutomaticLogin;
                }
                set
                {
                    if (_AutomaticLogin != value)
                    {
                        _AutomaticLogin = value;
                        OnValueChanged();
                    }
                }
            }

            public bool _AutomaticPlay;
            public bool AutomaticPlay
            {
                get
                {
                    return _AutomaticPlay;
                }
                set
                {
                    if (_AutomaticPlay != value)
                    {
                        _AutomaticPlay = value;
                        OnValueChanged();
                    }
                }
            }

            public bool HasCredentials
            {
                get
                {
                    return !string.IsNullOrEmpty(_Email) && _Password != null && _Password.Length > 0;
                }
            }

            public string _Email;
            public string Email
            {
                get
                {
                    return _Email;
                }
                set
                {
                    if (_Email != value)
                    {
                        _Email = value;
                        OnValueChanged();
                    }
                }
            }

            public System.Security.SecureString _Password;
            public System.Security.SecureString Password
            {
                get
                {
                    return _Password;
                }
                set
                {
                    if (_Password != value)
                    {
                        _Password = value;
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

            public bool _AutomaticRememberedLogin;
            public bool AutomaticRememberedLogin
            {
                get
                {
                    return _AutomaticRememberedLogin;
                }
                set
                {
                    if (_AutomaticRememberedLogin != value)
                    {
                        _AutomaticRememberedLogin = value;
                        OnValueChanged();
                    }
                }
            }

            public MuteOptions _Mute;
            public MuteOptions Mute
            {
                get
                {
                    return _Mute;
                }
                set
                {
                    if (_Mute != value)
                    {
                        _Mute = value;
                        OnValueChanged();
                    }
                }
            }

            public ushort _ClientPort;
            public ushort ClientPort
            {
                get
                {
                    return _ClientPort;
                }
                set
                {
                    if (_ClientPort != value)
                    {
                        _ClientPort = value;
                        OnValueChanged();
                    }
                }
            }

            public ScreenshotFormat _ScreenshotsFormat;
            public ScreenshotFormat ScreenshotsFormat
            {
                get
                {
                    return _ScreenshotsFormat;
                }
                set
                {
                    if (_ScreenshotsFormat != value)
                    {
                        _ScreenshotsFormat = value;
                        OnValueChanged();
                    }
                }
            }

            public string _ScreenshotsLocation;
            public string ScreenshotsLocation
            {
                get
                {
                    return _ScreenshotsLocation;
                }
                set
                {
                    if (_ScreenshotsLocation != value)
                    {
                        _ScreenshotsLocation = value;
                        _PendingFiles = true;
                        OnValueChanged();
                    }
                }
            }

            public bool _PendingFiles;
            public bool PendingFiles
            {
                get
                {
                    return _PendingFiles;
                }
                set
                {
                    if (_PendingFiles != value)
                    {
                        _PendingFiles = value;
                        OnValueChanged();
                    }
                }
            }

            public AccountApiData _ApiData;
            public IAccountApiData ApiData
            {
                get
                {
                    return _ApiData;
                }
                set
                {
                    if (_ApiData != value)
                    {
                        _ApiData = (AccountApiData)value;
                        OnValueChanged();
                    }
                }
            }

            public string _ApiKey;
            public string ApiKey
            {
                get
                {
                    return _ApiKey;
                }
                set
                {
                    if (_ApiKey != value)
                    {
                        _ApiKey = value;
                        OnValueChanged();
                    }
                }
            }

            public NetworkAuthorizationState _NetworkAuthorizationState;
            public NetworkAuthorizationState NetworkAuthorizationState
            {
                get
                {
                    return _NetworkAuthorizationState;
                }
                set
                {
                    if (_NetworkAuthorizationState != value)
                    {
                        _NetworkAuthorizationState = value;
                        OnValueChanged();
                    }
                }
            }

            public byte[] _TotpKey;
            public byte[] TotpKey
            {
                get
                {
                    return _TotpKey;
                }
                set
                {
                    if (_TotpKey != value)
                    {
                        _TotpKey = value;
                        OnValueChanged();
                    }
                }
            }

            public ProcessPriorityClass _ProcessPriority;
            public ProcessPriorityClass ProcessPriority
            {
                get
                {
                    return _ProcessPriority;
                }
                set
                {
                    if (_ProcessPriority != value)
                    {
                        _ProcessPriority = value;
                        OnValueChanged();
                    }
                }
            }

            public long _ProcessAffinity;
            public long ProcessAffinity
            {
                get
                {
                    return _ProcessAffinity;
                }
                set
                {
                    if (_ProcessAffinity != value)
                    {
                        _ProcessAffinity = value;
                        OnValueChanged();
                    }
                }
            }

            public Notes _Notes;
            public Notes Notes
            {
                get
                {
                    return _Notes;
                }
                set
                {
                    if (_Notes != value)
                    {
                        _Notes = value;
                        OnValueChanged();
                    }
                }
            }

            public IAccountApiData CreateApiData()
            {
                return new AccountApiData();
            }

            public Color _ColorKey;
            public Color ColorKey
            {
                get
                {
                    return _ColorKey;
                }
                set
                {
                    if (_ColorKey != value)
                    {
                        _ColorKey = value;
                        OnValueChanged();

                        if (ColorKeyChanged != null)
                            ColorKeyChanged(this, EventArgs.Empty);
                    }
                }
            }

            public IconType _IconType;
            public IconType IconType
            {
                get
                {
                    return _IconType;
                }
                set
                {
                    if (_IconType != value)
                    {
                        _IconType = value;
                        OnValueChanged();

                        if (IconTypeChanged != null)
                            IconTypeChanged(this, EventArgs.Empty);
                    }
                }
            }

            public string _Icon;
            public string Icon
            {
                get
                {
                    return _Icon;
                }
                set
                {
                    if (_Icon != value)
                    {
                        _Icon = value;
                        OnValueChanged();

                        if (IconChanged != null)
                            IconChanged(this, EventArgs.Empty);
                    }
                }
            }

            public ushort _SortKey;
            public ushort SortKey
            {
                get
                {
                    return _SortKey;
                }
                set
                {
                    if (_SortKey != value)
                    {
                        _SortKey = value;
                        OnValueChanged();
                    }
                }
            }

            public void Sort(IAccount reference, AccountSorting.SortType type)
            {
                AccountSorting.Sort(this, reference, type);
            }
        }
        
        private class DatFile : BaseFile, IDatFile
        {
            public DatFile(ushort uid)
                : base(uid)
            {
            }

            public DatFile()
            {

            }
        }

        private class GfxFile : BaseFile, IGfxFile
        {
            public GfxFile(ushort uid)
                : base(uid)
            {
            }

            public GfxFile()
            {

            }
        }

        private abstract class BaseFile : IFile
        {
            public event EventHandler PathChanged;

            public BaseFile(ushort uid)
            {
                this.UID = uid;
            }

            public BaseFile()
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

                        if (PathChanged != null)
                            PathChanged(this, EventArgs.Empty);
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

            public byte References
            {
                get
                {
                    return ReferenceCount;
                }
            }
        }

        public struct Values<T1, T2>
        {
            public T1 value1;
            public T2 value2;
        }

        public class LastCheckedVersion
        {
            public LastCheckedVersion(DateTime lastCheck, ushort version)
            {
                LastCheck = lastCheck;
                Version = version;
            }

            public DateTime LastCheck
            {
                get;
                private set;
            }
            public ushort Version
            {
                get;
                private set;
            }
        }

        public class ScreenAttachment
        {
            public ScreenAttachment(byte screen, ScreenAnchor anchor)
            {
                Screen = screen;
                Anchor = anchor;
            }

            public byte Screen
            {
                get;
                private set;
            }
            public ScreenAnchor Anchor
            {
                get;
                private set;
            }
        }

        public struct Point<T>
        {
            public Point(T x, T y) : this()
            {
                X = x;
                Y = y;
            }

            public T X
            {
                get;
                set;
            }

            public T Y
            {
                get;
                set;
            }

            public bool IsEmpty
            {
                get
                {
                    return object.Equals(X, Y) && object.Equals(X, default(T));
                }
            }

            public Point ToPoint()
            {
                return new Point(Convert.ToInt32(X), Convert.ToInt32(Y));
            }
        }

        public class LauncherPoints
        {
            public Point<ushort> EmptyArea
            {
                get;
                set;
            }

            public Point<ushort> PlayButton
            {
                get;
                set;
            }
        }

        public class NotificationScreenAttachment : ScreenAttachment
        {
            public NotificationScreenAttachment(byte screen, ScreenAnchor anchor, bool onlyWhileActive)
                : base(screen, anchor)
            {
                OnlyWhileActive = onlyWhileActive;
            }

            public bool OnlyWhileActive
            {
                get;
                private set;
            }
        }

        public class ScreenshotConversionOptions
        {
            public enum ImageFormat : byte
            {
                None = 0,
                Jpg = 1,
                Png = 2
            }

            public byte raw;

            public ImageFormat Format
            {
                get;
                set;
            }

            /// <summary>
            /// JPG: 0-100 quality, PNG: 24 or 16 color depth
            /// </summary>
            public byte Options
            {
                get
                {
                    return (byte)(raw & 127);
                }
                set
                {
                    if (value > 127)
                        throw new ArgumentOutOfRangeException();
                    raw = (byte)((raw & 128) | value);
                }
            }

            public bool DeleteOriginal
            {
                get
                {
                    return (raw & 128) == 128;
                }
                set
                {
                    if (value)
                        raw |= 128;
                    else
                        raw &= 127;
                }
            }
        }

        private static object _lock = new object();
        private static System.Threading.CancellationTokenSource cancelWrite;
        private static Task task;
        private static DateTime _lastModified;
        private static ushort _accountUID;
        private static ushort _datUID;
        private static ushort _gfxUID;
        private static ushort _accountSortKey;

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
            _StyleShowAccount = new SettingValue<bool>();
            _DatFiles = new KeyedProperty<ushort, IDatFile>(
                new Func<ushort,ISettingValue<IDatFile>>(
                    delegate (ushort key)
                    {
                        return new SettingValue<IDatFile>(new DatFile(key));
                    }));
            _GfxFiles = new KeyedProperty<ushort, IGfxFile>(
                new Func<ushort, ISettingValue<IGfxFile>>(
                    delegate(ushort key)
                    {
                        return new SettingValue<IGfxFile>(new GfxFile(key));
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
            _PreventTaskbarGrouping = new SettingValue<bool>();
            _WindowCaption = new SettingValue<string>();

            _AutomaticRememberedLogin = new SettingValue<bool>();
            _Mute = new SettingValue<MuteOptions>();
            _ClientPort = new SettingValue<ushort>();
            _ScreenshotsFormat = new SettingValue<ScreenshotFormat>();
            _ScreenshotsLocation = new SettingValue<string>();
            _TopMost = new SettingValue<bool>();
            _VirtualUserPath = new PendingSettingValue<string>();
            _ActionActiveLClick = new SettingValue<ButtonAction>();
            _ActionActiveLPress = new SettingValue<ButtonAction>();
            _ShowDailies = new SettingValue<DailiesMode>();
            _HiddenDailyCategories = new KeyedProperty<byte, bool>();
            _BackgroundPatchingProgress = new SettingValue<Rectangle>();
            _NetworkAuthorization = new SettingValue<NetworkAuthorizationFlags>();
            _ScreenshotNaming = new SettingValue<string>(); 
            _ScreenshotConversion = new SettingValue<ScreenshotConversionOptions>();
            _DeleteCrashLogsOnLaunch = new SettingValue<bool>();
            _ProcessPriority = new SettingValue<ProcessPriorityClass>();
            _ProcessAffinity = new SettingValue<long>();
            _PrioritizeCoherentUI = new SettingValue<bool>();
            _NotesNotifications = new SettingValue<NotificationScreenAttachment>();
            _MaxPatchConnections = new SettingValue<byte>();

            _ActionInactiveLClick = new SettingValue<ButtonAction>();
            _StyleShowColor = new SettingValue<bool>();
            _StyleHighlightFocused = new SettingValue<bool>();
            _WindowIcon = new SettingValue<bool>();
            _AccountBarEnabled = new SettingValue<bool>();
            _AccountBarStyle = new SettingValue<AccountBarStyles>();
            _AccountBarOptions = new SettingValue<AccountBarOptions>();
            _AccountBarSortingMode = new SettingValue<SortMode>();
            _AccountBarSortingOrder = new SettingValue<SortOrder>();
            _AccountBarDocked = new SettingValue<ScreenAnchor>();
            _UseGw2IconForShortcuts = new SettingValue<bool>();
            _LimitActiveAccounts = new SettingValue<byte>();
            _DelayLaunchUntilLoaded = new SettingValue<bool>();
            _DelayLaunchSeconds = new SettingValue<byte>();
            _LocalizeAccountExecution = new PendingSettingValue<bool>();
            _LauncherAutologinPoints = new SettingValue<LauncherPoints>();

            FORMS = new Type[]
            {
                typeof(UI.formMain),
                typeof(UI.formDailies),
                typeof(UI.formNotes),
                typeof(UI.formAccountBar),
                typeof(UI.formSettings),
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

                    task = new Task(
                        delegate
                        {
                            DelayedWrite(cancel);
                        });
                    task.Start();
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
            if (ReadOnly)
                return null;

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
                        //v1-HasValue from:0
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
                        _StyleShowAccount.HasValue,

                        //v1-Values from:13
                        _StoreCredentials.Value,
                        _ShowTray.Value,
                        _MinimizeToTray.Value,
                        _BringToFrontOnExit.Value,
                        _DeleteCacheOnLaunch.Value,
                        _StyleShowAccount.Value,
                        
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
                        _PatchingUseHttps.Value,

                        //v3-HasValue from:37
                        _WindowCaption.HasValue && !string.IsNullOrWhiteSpace(_WindowCaption.Value),
                        _PreventTaskbarGrouping.HasValue,
                        
                        //v3-Values from:39
                        _PreventTaskbarGrouping.Value,

                        //v4-HasValue from:40
                        _AutomaticRememberedLogin.HasValue,
                        _Mute.HasValue,
                        _ClientPort.HasValue,
                        _ScreenshotsFormat.HasValue,
                        _ScreenshotsLocation.HasValue,
                        _TopMost.HasValue,
                        _VirtualUserPath.HasValue,
                        _VirtualUserPath.IsPending,
                        _ActionActiveLClick.HasValue,
                        _ActionActiveLPress.HasValue,
                        _ShowDailies.HasValue,
                        _BackgroundPatchingProgress.HasValue,
                        _NetworkAuthorization.HasValue,
                        _ScreenshotNaming.HasValue,
                        _ScreenshotConversion.HasValue,
                        _DeleteCrashLogsOnLaunch.HasValue,
                        _ProcessPriority.HasValue,
                        _ProcessAffinity.HasValue,
                        _PrioritizeCoherentUI.HasValue,
                        _NotesNotifications.HasValue,
                        _MaxPatchConnections.HasValue,

                        //v4-Values from:61
                        _AutomaticRememberedLogin.Value,
                        _TopMost.Value,
                        _DeleteCrashLogsOnLaunch.Value,
                        _PrioritizeCoherentUI.Value,

                        //v5-HasValue from:65
                        _ActionInactiveLClick.HasValue,
                        _StyleShowColor.HasValue,
                        _StyleHighlightFocused.HasValue,
                        _WindowIcon.HasValue,
                        _AccountBarEnabled.HasValue,
                        _AccountBarStyle.HasValue,
                        _AccountBarOptions.HasValue,
                        _AccountBarSortingMode.HasValue,
                        _AccountBarSortingOrder.HasValue,
                        _AccountBarDocked.HasValue,
                        _UseGw2IconForShortcuts.HasValue,
                        _LimitActiveAccounts.HasValue,
                        _DelayLaunchUntilLoaded.HasValue,
                        _DelayLaunchSeconds.HasValue,
                        _LocalizeAccountExecution.HasValue,
                        _LocalizeAccountExecution.IsPending,
                        _LauncherAutologinPoints.HasValue,

                        //v5-Values from:82
                        _StyleShowColor.Value,
                        _StyleHighlightFocused.Value,
                        _WindowIcon.Value,
                        _AccountBarEnabled.Value,
                        _UseGw2IconForShortcuts.Value,
                        _DelayLaunchUntilLoaded.Value,
                        _LocalizeAccountExecution.Value,
                        _LocalizeAccountExecution.ValueCommit,
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
                        writer.Write(v.LastCheck.ToBinary());
                        writer.Write(v.Version);
                    }
                    if (booleans[21])
                        writer.Write(_AutoUpdateInterval.Value);
                    if (booleans[24])
                        writer.Write(_BackgroundPatchingLang.Value);
                    if (booleans[25])
                    {
                        var v = _BackgroundPatchingNotifications.Value;
                        writer.Write((byte)v.Screen);
                        writer.Write((byte)v.Anchor);
                    }
                    if (booleans[26])
                        writer.Write(_BackgroundPatchingMaximumThreads.Value);
                    if (booleans[27])
                        writer.Write(_PatchingSpeedLimit.Value);
                    if (booleans[29])
                        writer.Write(_RunAfterLaunching.Value);
                    if (booleans[30])
                        writer.Write((byte)(_Volume.Value * 255));

                    //v3
                    if (booleans[37])
                        writer.Write(_WindowCaption.Value);

                    //v4
                    if (booleans[41])
                        writer.Write((byte)_Mute.Value);
                    if (booleans[42])
                        writer.Write(_ClientPort.Value);
                    if (booleans[43])
                        writer.Write((byte)_ScreenshotsFormat.Value);
                    if (booleans[44])
                        writer.Write(_ScreenshotsLocation.Value);
                    if (booleans[46])
                        writer.Write(_VirtualUserPath.Value);
                    if (booleans[47])
                    {
                        if (_VirtualUserPath.ValueCommit == null)
                            writer.Write(string.Empty);
                        else
                            writer.Write(_VirtualUserPath.ValueCommit);
                    }
                    if (booleans[48])
                        writer.Write((byte)_ActionActiveLClick.Value);
                    if (booleans[49])
                        writer.Write((byte)_ActionActiveLPress.Value);
                    if (booleans[50])
                        writer.Write((byte)_ShowDailies.Value);
                    if (booleans[51])
                    {
                        var r = _BackgroundPatchingProgress.Value;
                        writer.Write(r.X);
                        writer.Write(r.Y);
                        writer.Write(r.Width);
                        writer.Write(r.Height);
                    }
                    if (booleans[52])
                        writer.Write((byte)_NetworkAuthorization.Value);
                    if (booleans[53])
                        writer.Write(_ScreenshotNaming.Value);
                    if (booleans[54])
                    {
                        var v = _ScreenshotConversion.Value;
                        writer.Write((byte)v.Format);
                        writer.Write(v.raw);
                    }
                    if (booleans[56])
                        writer.Write((byte)_ProcessPriority.Value);
                    if (booleans[57])
                        writer.Write(_ProcessAffinity.Value);
                    if (booleans[59])
                    {
                        var v = _NotesNotifications.Value;
                        writer.Write((byte)v.Screen);
                        writer.Write((byte)v.Anchor);
                        writer.Write(v.OnlyWhileActive);
                    }
                    if (booleans[60])
                        writer.Write(_MaxPatchConnections.Value);

                    //v5
                    if (booleans[65])
                        writer.Write((byte)_ActionInactiveLClick.Value);
                    if (booleans[70])
                        writer.Write((byte)_AccountBarStyle.Value);
                    if (booleans[71])
                        writer.Write((byte)_AccountBarOptions.Value);
                    if (booleans[72])
                        writer.Write((byte)_AccountBarSortingMode.Value);
                    if (booleans[73])
                        writer.Write((byte)_AccountBarSortingOrder.Value);
                    if (booleans[74])
                        writer.Write((byte)_AccountBarDocked.Value);
                    if (booleans[76])
                        writer.Write(_LimitActiveAccounts.Value);
                    if (booleans[78])
                        writer.Write(_DelayLaunchSeconds.Value);
                    if (booleans[81])
                    {
                        var v = _LauncherAutologinPoints.Value;
                        writer.Write(v.EmptyArea.X);
                        writer.Write(v.EmptyArea.Y);
                        writer.Write(v.PlayButton.X);
                        writer.Write(v.PlayButton.Y);
                    }

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

                    //v4
                    lock (_GfxFiles)
                    {
                        var count = _GfxFiles.Count;
                        var items = new KeyValuePair<ushort, GfxFile>[count];
                        int i = 0;

                        foreach (var key in _GfxFiles.Keys)
                        {
                            if (i == count)
                                break;
                            var o = _GfxFiles[key];
                            if (o.HasValue && ((GfxFile)o.Value).ReferenceCount > 0)
                            {
                                items[i++] = new KeyValuePair<ushort, GfxFile>(key, (GfxFile)o.Value);
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
                            writer.Write(account._UID);
                            writer.Write(account._Name);
                            writer.Write(account._WindowsAccount);
                            writer.Write(account._CreatedUtc.ToBinary());
                            writer.Write(account._LastUsedUtc.ToBinary());
                            writer.Write(account._TotalUses);
                            writer.Write(account._Arguments);

                            booleans = new bool[]
                            {
                                account._ShowDailyLogin,
                                account.Windowed,
                                account._RecordLaunches,
                                account._AutomaticLogin,
                                account._DatFile != null,
                                account.VolumeEnabled,
                                !string.IsNullOrEmpty(account._RunAfterLaunching),

                                //v4 from:7
                                !string.IsNullOrEmpty(account._Email),
                                account._Password != null && account._Password.Length > 0,
                                account._AutomaticRememberedLogin,
                                account._GfxFile != null,
                                account._PendingFiles,
                                !string.IsNullOrEmpty(account._ScreenshotsLocation),
                                !string.IsNullOrEmpty(account._ApiKey),
                                account._TotpKey != null && account._TotpKey.Length > 0,
                                account._ApiData != null,
                                !account._WindowBounds.IsEmpty,
                                (byte)account._ProcessPriority > 0,
                                account._ClientPort != 0,
                                account._LastDailyCompletionUtc.Ticks != 1,
                                account._Mute  != 0,
                                account._ScreenshotsFormat != 0,
                                account._NetworkAuthorizationState != 0,
                                account._ProcessPriority != ProcessPriorityClass.None,
                                account._ProcessAffinity != 0,
                                account._Notes != null && account._Notes.Count > 0,

                                //v5 from:26
                                !account._ColorKey.IsEmpty,
                                account._IconType != IconType.None,
                                account._SortKey != (i + 1),
                                account._AutomaticPlay
                            };

                            b = CompressBooleans(booleans);

                            writer.Write((byte)b.Length);
                            writer.Write(b);

                            if (booleans[4])
                                writer.Write(account._DatFile.UID);

                            //discontinued with v4
                            //if (booleans[3])
                            //{
                            //    writer.Write(account.Email);
                            //    writer.Write(account.Password);
                            //}

                            if (booleans[5])
                                writer.Write(account._Volume);

                            if (booleans[6])
                                writer.Write(account._RunAfterLaunching);

                            //v4
                            if (booleans[7])
                                writer.Write(account._Email);

                            if (booleans[8])
                            {
                                byte[] data;
                                try
                                {
                                    data = Security.Credentials.ToProtectedByteArray(account._Password);
                                }
                                catch (Exception e)
                                {
                                    Util.Logging.Log(e);
                                    data = null;
                                }

                                if (data != null)
                                {
                                    writer.Write((ushort)data.Length);
                                    writer.Write(data);
                                }
                                else
                                {
                                    writer.Write((ushort)0);
                                }
                            }

                            if (booleans[10])
                                writer.Write(account._GfxFile.UID);

                            if (booleans[12])
                                writer.Write(account._ScreenshotsLocation);

                            if (booleans[13])
                                writer.Write(account._ApiKey);

                            if (booleans[14])
                            {
                                writer.Write((byte)account._TotpKey.Length);
                                writer.Write(account._TotpKey);
                            }

                            if (booleans[15])
                            {
                                var ad = account._ApiData;

                                var booleansApi = new bool[]
                                {
                                    ad.DailyPoints != null,
                                    ad.Played != null
                                };

                                b = CompressBooleans(booleansApi);

                                writer.Write((byte)b.Length);
                                writer.Write(b);

                                if (booleansApi[0])
                                {
                                    writer.Write(ad._DailyPoints._LastChange.ToBinary());
                                    writer.Write((byte)ad._DailyPoints._State);
                                    writer.Write(ad._DailyPoints._Value);
                                }

                                if (booleansApi[1])
                                {
                                    writer.Write(ad._Played._LastChange.ToBinary());
                                    writer.Write((byte)ad._Played._State);
                                    writer.Write(ad._Played._Value);
                                }
                            }

                            if (booleans[16])
                            {
                                writer.Write(account._WindowBounds.X);
                                writer.Write(account._WindowBounds.Y);
                                writer.Write(account._WindowBounds.Width);
                                writer.Write(account._WindowBounds.Height);
                            }

                            if (booleans[17])
                                writer.Write((byte)account._ProcessPriority);

                            if (booleans[18])
                                writer.Write(account._ClientPort);

                            if (booleans[19])
                                writer.Write(account._LastDailyCompletionUtc.ToBinary());

                            if (booleans[20])
                                writer.Write((byte)account._Mute);

                            if (booleans[21])
                                writer.Write((byte)account._ScreenshotsFormat);

                            if (booleans[22])
                                writer.Write((byte)account._NetworkAuthorizationState);

                            if (booleans[23])
                                writer.Write((byte)account._ProcessPriority);

                            if (booleans[24])
                                writer.Write(account._ProcessAffinity);

                            if (booleans[25])
                            {
                                var notes = account._Notes;

                                lock (notes)
                                {
                                    if (notes.Count >= byte.MaxValue)
                                    {
                                        writer.Write(byte.MaxValue);
                                        writer.Write((ushort)notes.Count);
                                    }
                                    else
                                        writer.Write((byte)notes.Count);

                                    foreach (var n in notes)
                                    {
                                        writer.Write((ushort)n.SID);
                                        writer.Write(n.Expires.ToBinary());
                                        writer.Write(n.Notify);
                                    }
                                }
                            }

                            if (booleans[26])
                                writer.Write(account._ColorKey.ToArgb());

                            if (booleans[27])
                            {
                                var v = account._IconType;
                                writer.Write((byte)v);
                                if (v == IconType.File)
                                {
                                    var icon = account._Icon;
                                    if (icon == null)
                                        icon = "";
                                    writer.Write(icon);
                                }
                            }

                            if (booleans[28])
                                writer.Write(account._SortKey);
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

                    //v4
                    lock (_HiddenDailyCategories)
                    {
                        var count = _HiddenDailyCategories.Count;
                        var items = new byte[count];
                        int i = 0;

                        foreach (var key in _HiddenDailyCategories.Keys)
                        {
                            if (i == count)
                                break;
                            var o = _HiddenDailyCategories[key];
                            if (o.HasValue && o.Value)
                            {
                                items[i++] = key;
                            }
                        }

                        count = i;

                        writer.Write((byte)count);

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
                    _StyleShowAccount.SetValue(booleans[18]);
                else
                    _StyleShowAccount.Clear();

                if (version >= 2)
                {
                    if (booleans[19])
                        _CheckForNewBuilds.SetValue(booleans[32]);
                    else
                        _CheckForNewBuilds.Clear();

                    if (booleans[20])
                    {
                        _LastProgramVersion.SetValue(new LastCheckedVersion(DateTime.FromBinary(reader.ReadInt64()), reader.ReadUInt16()));
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
                        _BackgroundPatchingNotifications.SetValue(new ScreenAttachment(reader.ReadByte(), (ScreenAnchor)reader.ReadByte()));
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

                if (version >= 3)
                {
                    if (booleans[37])
                        _WindowCaption.SetValue(reader.ReadString());
                    else
                        _WindowCaption.Clear();

                    if (booleans[38])
                        _PreventTaskbarGrouping.SetValue(booleans[39]);
                    else
                        _PreventTaskbarGrouping.Clear();
                }

                if (version >= 4)
                {
                    if (booleans[40])
                        _AutomaticRememberedLogin.SetValue(booleans[61]);
                    else
                        _AutomaticRememberedLogin.Clear();

                    if (booleans[41])
                        _Mute.SetValue((MuteOptions)reader.ReadByte());
                    else
                        _Mute.Clear();

                    if (booleans[42])
                        _ClientPort.SetValue(reader.ReadUInt16());
                    else
                        _ClientPort.Clear();

                    if (booleans[43])
                        _ScreenshotsFormat.SetValue((ScreenshotFormat)reader.ReadByte());
                    else
                        _ScreenshotsFormat.Clear();

                    if (booleans[44])
                        _ScreenshotsLocation.SetValue(reader.ReadString());
                    else
                        _ScreenshotsLocation.Clear();

                    if (booleans[45])
                        _TopMost.SetValue(booleans[62]);
                    else
                        _TopMost.Clear();

                    if (booleans[46])
                        _VirtualUserPath.SetValue(reader.ReadString());
                    else
                        _VirtualUserPath.Clear();

                    if (booleans[47])
                        _VirtualUserPath.SetCommit(reader.ReadString());
                    else
                        _VirtualUserPath.ClearCommit();

                    if (booleans[48])
                        _ActionActiveLClick.SetValue((ButtonAction)reader.ReadByte());
                    else
                        _ActionActiveLClick.Clear();

                    if (booleans[49])
                        _ActionActiveLPress.SetValue((ButtonAction)reader.ReadByte());
                    else
                        _ActionActiveLPress.Clear();

                    if (booleans[50])
                        _ShowDailies.SetValue((DailiesMode)reader.ReadByte());
                    else
                        _ShowDailies.Clear();

                    if (booleans[51])
                        _BackgroundPatchingProgress.SetValue(new Rectangle(reader.ReadInt32(),reader.ReadInt32(),reader.ReadInt32(),reader.ReadInt32()));
                    else
                        _BackgroundPatchingProgress.Clear();

                    if (booleans[52])
                        _NetworkAuthorization.SetValue((NetworkAuthorizationFlags)reader.ReadByte());
                    else
                        _NetworkAuthorization.Clear();

                    if (booleans[53])
                        _ScreenshotNaming.SetValue(reader.ReadString());
                    else
                        _ScreenshotNaming.Clear();

                    if (booleans[54])
                    {
                        _ScreenshotConversion.SetValue(new ScreenshotConversionOptions()
                            {
                                Format = (ScreenshotConversionOptions.ImageFormat)reader.ReadByte(),
                                raw = reader.ReadByte(),
                            });
                    }
                    else
                        _ScreenshotConversion.Clear();

                    if (booleans[55])
                        _DeleteCrashLogsOnLaunch.SetValue(booleans[63]);
                    else
                        _DeleteCrashLogsOnLaunch.Clear();

                    if (booleans[56])
                        _ProcessPriority.SetValue((ProcessPriorityClass)reader.ReadByte());
                    else
                        _ProcessPriority.Clear();

                    if (booleans[57])
                        _ProcessAffinity.SetValue(reader.ReadInt64());
                    else
                        _ProcessAffinity.Clear();

                    if (booleans[58])
                        _PrioritizeCoherentUI.SetValue(booleans[64]);
                    else
                        _PrioritizeCoherentUI.Clear();

                    if (booleans[59])
                        _NotesNotifications.SetValue(new NotificationScreenAttachment(reader.ReadByte(), (ScreenAnchor)reader.ReadByte(), reader.ReadBoolean()));
                    else
                        _NotesNotifications.Clear();

                    if (booleans[60])
                        _MaxPatchConnections.SetValue(reader.ReadByte());
                    else
                        _MaxPatchConnections.Clear();
                }

                if (version >= 5)
                {
                    if (booleans[65])
                        _ActionInactiveLClick.SetValue((ButtonAction)reader.ReadByte());
                    else
                        _ActionInactiveLClick.Clear();

                    if (booleans[66])
                        _StyleShowColor.SetValue(booleans[82]);
                    else
                        _StyleShowColor.Clear();

                    if (booleans[67])
                        _StyleHighlightFocused.SetValue(booleans[83]);
                    else
                        _StyleHighlightFocused.Clear();

                    if (booleans[68])
                        _WindowIcon.SetValue(booleans[84]);
                    else
                        _WindowIcon.Clear();

                    if (booleans[69])
                        _AccountBarEnabled.SetValue(booleans[85]);
                    else
                        _AccountBarEnabled.Clear();

                    if (booleans[70])
                        _AccountBarStyle.SetValue((AccountBarStyles)reader.ReadByte());
                    else
                        _AccountBarStyle.Clear();

                    if (booleans[71])
                        _AccountBarOptions.SetValue((AccountBarOptions)reader.ReadByte());
                    else
                        _AccountBarOptions.Clear();

                    if (booleans[72])
                        _AccountBarSortingMode.SetValue((SortMode)reader.ReadByte());
                    else
                        _AccountBarSortingMode.Clear();

                    if (booleans[73])
                        _AccountBarSortingOrder.SetValue((SortOrder)reader.ReadByte());
                    else
                        _AccountBarSortingOrder.Clear();

                    if (booleans[74])
                        _AccountBarDocked.SetValue((ScreenAnchor)reader.ReadByte());
                    else
                        _AccountBarDocked.Clear();

                    if (booleans[75])
                        _UseGw2IconForShortcuts.SetValue(booleans[86]);
                    else
                        _UseGw2IconForShortcuts.Clear();

                    if (booleans[76])
                        _LimitActiveAccounts.SetValue(reader.ReadByte());
                    else
                        _LimitActiveAccounts.Clear();

                    if (booleans[77])
                        _DelayLaunchUntilLoaded.SetValue(booleans[87]);
                    else
                        _DelayLaunchUntilLoaded.Clear();

                    if (booleans[78])
                        _DelayLaunchSeconds.SetValue(reader.ReadByte());
                    else
                        _DelayLaunchSeconds.Clear();

                    if (booleans[79])
                        _LocalizeAccountExecution.SetValue(booleans[88]);
                    else
                        _LocalizeAccountExecution.Clear();

                    if (booleans[80])
                        _LocalizeAccountExecution.SetCommit(booleans[89]);
                    else
                        _LocalizeAccountExecution.ClearCommit();

                    if (booleans[81])
                    {
                        var v = new LauncherPoints()
                        {
                            EmptyArea = new Point<ushort>(reader.ReadUInt16(), reader.ReadUInt16()),
                            PlayButton = new Point<ushort>(reader.ReadUInt16(), reader.ReadUInt16()),
                        };
                        _LauncherAutologinPoints.SetValue(v);
                    }
                    else
                        _LauncherAutologinPoints.Clear();
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

                if (version >= 4)
                {
                    _gfxUID = 0;

                    lock (_GfxFiles)
                    {
                        _GfxFiles.Clear();

                        var count = reader.ReadUInt16();
                        for (int i = 0; i < count; i++)
                        {
                            var s = new GfxFile();
                            s._UID = reader.ReadUInt16();
                            s._Path = reader.ReadString();
                            s._IsInitialized = reader.ReadBoolean();

                            _GfxFiles.Add(s.UID, new SettingValue<IGfxFile>(s));

                            if (_gfxUID < s.UID)
                                _gfxUID = s.UID;
                        }
                    }
                }
                else
                {
                    _gfxUID = 0;

                    lock (_GfxFiles)
                    {
                        _GfxFiles.Clear();
                    }
                }

                _accountUID = 0;
                _accountSortKey = 0;
                var sids = new HashSet<ushort>();

                lock (_Accounts)
                {
                    _Accounts.Clear();

                    var count = reader.ReadUInt16();
                    var accounts = new Account[count];
                    uint sortKeySum = 0;

                    for (int i = 0; i < count; i++)
                    {
                        var account = accounts[i] = new Account();
                        account._UID = reader.ReadUInt16();
                        account._Name = reader.ReadString();
                        account._WindowsAccount = reader.ReadString();
                        account._CreatedUtc = DateTime.FromBinary(reader.ReadInt64());
                        account._LastUsedUtc = DateTime.FromBinary(reader.ReadInt64());
                        account._TotalUses = reader.ReadUInt16();
                        account._Arguments = reader.ReadString();

                        b = reader.ReadBytes(reader.ReadByte());
                        booleans = ExpandBooleans(b);

                        account._ShowDailyLogin = booleans[0];
                        account._WindowOptions = booleans[1] ? WindowOptions.Windowed : WindowOptions.None;
                        account._RecordLaunches = booleans[2];
                        account._AutomaticLogin = booleans[3]; //v4

                        if (booleans[4])
                        {
                            account._DatFile = (DatFile)_DatFiles[reader.ReadUInt16()].Value;
                            account._DatFile.ReferenceCount++;
                        }

                        if (version <= 3)
                        {
                            account._WindowBounds = new Rectangle(reader.ReadInt32(), reader.ReadInt32(), reader.ReadInt32(), reader.ReadInt32());

                            if (booleans[3])
                            {
                                account._Email = reader.ReadString();
                                var s = reader.ReadString();
                                account._Password = Security.Credentials.FromString(ref s);
                            }
                        }

                        if (version >= 2)
                        {
                            if (booleans[5])
                                account._Volume = reader.ReadByte();
                            if (booleans[6])
                                account._RunAfterLaunching = reader.ReadString();
                        }

                        if (version >= 4)
                        {
                            if (booleans[7])
                                account._Email = reader.ReadString();
                            if (booleans[8])
                            {
                                ushort length = reader.ReadUInt16();
                                if (length > 0)
                                {
                                    byte[] data = reader.ReadBytes(length);

                                    try
                                    {
                                        account._Password = Security.Credentials.FromProtectedByteArray(data);
                                    }
                                    catch (Exception e)
                                    {
                                        Util.Logging.Log(e);
                                    }
                                }
                            }

                            account._AutomaticRememberedLogin = booleans[9];

                            if (booleans[10])
                            {
                                account._GfxFile = (GfxFile)_GfxFiles[reader.ReadUInt16()].Value;
                                account._GfxFile.ReferenceCount++;
                            }

                            account._PendingFiles = booleans[11];

                            if (booleans[12])
                                account._ScreenshotsLocation = reader.ReadString();

                            if (booleans[13])
                                account._ApiKey = reader.ReadString();

                            if (booleans[14])
                                account._TotpKey = reader.ReadBytes(reader.ReadByte());

                            if (booleans[15])
                            {
                                var ad = new AccountApiData();

                                var booleansApi = ExpandBooleans(reader.ReadBytes(reader.ReadByte()));

                                if (booleansApi[0])
                                {
                                    ad._DailyPoints = new ApiValue<ushort>()
                                    {
                                        _LastChange = DateTime.FromBinary(reader.ReadInt64()),
                                        _State = (ApiCacheState)reader.ReadByte(),
                                        _Value = reader.ReadUInt16(),
                                    };
                                }

                                if (booleansApi[1])
                                {
                                    ad._Played = new ApiValue<int>()
                                    {
                                        _LastChange = DateTime.FromBinary(reader.ReadInt64()),
                                        _State = (ApiCacheState)reader.ReadByte(),
                                        _Value = reader.ReadInt32(),
                                    };
                                }

                                account._ApiData = ad;
                            }

                            if (booleans[16])
                                account._WindowBounds = new Rectangle(reader.ReadInt32(), reader.ReadInt32(), reader.ReadInt32(), reader.ReadInt32());

                            if (booleans[17])
                                account._ProcessPriority = (ProcessPriorityClass)reader.ReadByte();

                            if (booleans[18])
                                account._ClientPort = reader.ReadUInt16();

                            if (booleans[19])
                                account._LastDailyCompletionUtc = DateTime.FromBinary(reader.ReadInt64());

                            if (booleans[20])
                                account._Mute = (MuteOptions)reader.ReadByte();

                            if (booleans[21])
                                account._ScreenshotsFormat = (ScreenshotFormat)reader.ReadByte();

                            if (booleans[22])
                                account._NetworkAuthorizationState = (NetworkAuthorizationState)reader.ReadByte();

                            if (booleans[23])
                                account._ProcessPriority = (ProcessPriorityClass)reader.ReadByte();

                            if (booleans[24])
                                account._ProcessAffinity = reader.ReadInt64();

                            if (booleans[25])
                            {
                                int length = reader.ReadByte();
                                if (length == byte.MaxValue)
                                    length = reader.ReadUInt16();

                                if (length > 0)
                                {
                                    var notes = new Notes.Note[length];

                                    for (var j = 0; j < length; j++)
                                    {
                                        var sid = reader.ReadUInt16();
                                        var expires = DateTime.FromBinary(reader.ReadInt64());
                                        var notify = reader.ReadBoolean();

                                        sids.Add(sid);

                                        notes[j] = new Notes.Note(expires, sid, notify);
                                    }

                                    account._Notes = new Notes(notes);
                                }
                            }
                        }

                        if (version >= 5)
                        {
                            if (booleans[26])
                                account._ColorKey = Color.FromArgb(reader.ReadInt32());

                            if (booleans[27])
                            {
                                var v = (IconType)reader.ReadByte();
                                if (v == IconType.File)
                                {
                                    account._Icon = reader.ReadString();
                                    if (account._Icon.Length == 0)
                                        v = IconType.None;
                                }
                                account._IconType = v;
                            }

                            if (booleans[28])
                                account._SortKey = reader.ReadUInt16();
                            else
                                account._SortKey = (ushort)(i + 1);

                            account._AutomaticPlay = booleans[29];
                        }
                        else
                        {
                            account._SortKey = (ushort)(i + 1);
                            account._AutomaticLogin = false;
                        }

                        sortKeySum += account._SortKey;

                        SettingValue<IAccount> item = new SettingValue<IAccount>();
                        item.SetValue(account);
                        _Accounts.Add(account._UID, item);

                        if (_accountUID < account._UID)
                            _accountUID = account._UID;
                    }

                    if (sortKeySum != count * (count + 1) / 2)
                    {
                        Array.Sort<Account>(accounts, 
                            delegate(Account a1, Account a2)
                            {
                                var c = a1._SortKey.CompareTo(a2._SortKey);
                                if (c == 0)
                                    return a1._UID.CompareTo(a2._UID);
                                return c;
                            });

                        for (var i = 0; i < count; i++)
                        {
                            accounts[i]._SortKey = (ushort)(i + 1);
                        }
                    }

                    _accountSortKey = count;
                }

                if (sids.Count > 0)
                {
                    try
                    {
                        using (var notes = new Tools.Notes())
                        {
                            notes.RemoveExcept(sids);
                        }
                    }
                    catch (Exception e)
                    {
                        Util.Logging.Log(e);
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

                if (version >= 4)
                {
                    lock (_HiddenDailyCategories)
                    {
                        _HiddenDailyCategories.Clear();

                        var count = reader.ReadByte();
                        for (int i = 0; i < count; i++)
                        {
                            _HiddenDailyCategories.Add(reader.ReadByte(), new SettingValue<bool>(true));
                        }
                    }
                }

                #region Upgrade old versions

                if (version <= 1)
                {
                    #region Version 1

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

                    #endregion
                }
                
                if (version <= 3)
                {
                    #region Version 3

                    try
                    {
                        var pl = Path.Combine(DataPath.AppData, "PL");
                        if (Directory.Exists(pl))
                            Directory.Delete(pl, true);
                    }
                    catch { }

                    string[] keys = new string[] 
                    {
                        "clientport 80",
                        "clientport 443",
                        "autologin",
                        "nosound",
                        "nomusic",
                        "bmp",
                    };

                    var args = Settings._GW2Arguments.Value;
                    if (!string.IsNullOrWhiteSpace(args))
                    {
                        var changed = false;
                        var ikey = 0;

                        foreach (var key in keys)
                        {
                            if (Util.Args.Contains(args, key))
                            {
                                args = Util.Args.AddOrReplace(args, key, "");
                                changed = true;

                                switch (ikey)
                                {
                                    case 0:
                                        Settings._ClientPort.SetValue(80);
                                        break;
                                    case 1:
                                        Settings._ClientPort.SetValue(443);
                                        break;
                                    case 2:
                                        Settings._AutomaticRememberedLogin.SetValue(true);
                                        break;
                                    case 3:
                                        Settings._Mute.SetValue(Settings._Mute.Value | MuteOptions.All);
                                        break;
                                    case 4:
                                        Settings._Mute.SetValue(Settings._Mute.Value | MuteOptions.Music);
                                        break;
                                    case 5:
                                        Settings._ScreenshotsFormat.SetValue(ScreenshotFormat.Bitmap);
                                        break;
                                }
                            }

                            ikey++;
                        }

                        if (changed)
                            Settings._GW2Arguments.SetValue(args);
                    }

                    var gfxName = "GFXSettings.{0}.xml";
                    var exeName = string.IsNullOrEmpty(_GW2Path.Value) ? null : Path.GetFileName(_GW2Path.Value);
                    var gfxs = new Dictionary<ushort, GfxFile>();

                    foreach (var uid in _Accounts.GetKeys())
                    {
                        var account = (Account)_Accounts[uid].Value;
                        args = account._Arguments;

                        if (!string.IsNullOrWhiteSpace(args))
                        {
                            var changed = false;
                            var ikey = 0;

                            foreach (var key in keys)
                            {
                                if (Util.Args.Contains(args, key))
                                {
                                    args = Util.Args.AddOrReplace(args, key, "");
                                    changed = true;

                                    switch (ikey)
                                    {
                                        case 0:
                                            account._ClientPort = 80;
                                            break;
                                        case 1:
                                            account._ClientPort = 443;
                                            break;
                                        case 2:
                                            account._AutomaticRememberedLogin = true;
                                            break;
                                        case 3:
                                            account._Mute |= MuteOptions.All;
                                            break;
                                        case 4:
                                            account._Mute |= MuteOptions.Music;
                                            break;
                                        case 5:
                                            account._ScreenshotsFormat = ScreenshotFormat.Bitmap;
                                            break;
                                    }
                                }

                                ikey++;
                            }

                            if (changed)
                                account._Arguments = args;
                        }

                        if (exeName != null)
                        {
                            var dat = account._DatFile;
                            if (dat != null && !string.IsNullOrEmpty(dat._Path))
                            {
                                GfxFile gfx;
                                if (!gfxs.TryGetValue(dat._UID, out gfx))
                                {
                                    string gfxpath;
                                    if (dat._Path.EndsWith("." + dat._UID + ".dat"))
                                        gfxpath = string.Format(gfxName, exeName + "." + dat._UID);
                                    else
                                        gfxpath = string.Format(gfxName, exeName);

                                    gfxpath = Path.Combine(Path.GetDirectoryName(dat._Path), gfxpath);

                                    if (File.Exists(gfxpath))
                                    {
                                        gfx = (GfxFile)CreateGfxFile();
                                        gfx._Path = gfxpath;
                                    }

                                    gfxs[account._DatFile._UID] = gfx;
                                }
                                if (gfx != null)
                                    gfx.ReferenceCount++;
                                account._GfxFile = gfx;
                            }
                        }
                    }

                    #endregion
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

            return byte.MaxValue;
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

        private static SettingValue<bool> _StyleShowAccount;
        public static ISettingValue<bool> StyleShowAccount
        {
            get
            {
                return _StyleShowAccount;
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

        private static SettingValue<bool> _TopMost;
        public static ISettingValue<bool> TopMost
        {
            get
            {
                return _TopMost;
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

        private static KeyedProperty<ushort, IGfxFile> _GfxFiles;
        public static IKeyedProperty<ushort, IGfxFile> GfxFiles
        {
            get
            {
                return _GfxFiles;
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

        private static SettingValue<Rectangle> _BackgroundPatchingProgress;
        public static ISettingValue<Rectangle> BackgroundPatchingProgress
        {
            get
            {
                return _BackgroundPatchingProgress;
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

        private static SettingValue<bool> _PreventTaskbarGrouping;
        public static ISettingValue<bool> PreventTaskbarGrouping
        {
            get
            {
                return _PreventTaskbarGrouping;
            }
        }

        private static SettingValue<string> _WindowCaption;
        public static ISettingValue<string> WindowCaption
        {
            get
            {
                return _WindowCaption;
            }
        }

        private static SettingValue<bool> _AutomaticRememberedLogin;
        public static ISettingValue<bool> AutomaticRememberedLogin
        {
            get
            {
                return _AutomaticRememberedLogin;
            }
        }

        private static SettingValue<MuteOptions> _Mute;
        public static ISettingValue<MuteOptions> Mute
        {
            get
            {
                return _Mute;
            }
        }

        private static SettingValue<ushort> _ClientPort;
        public static ISettingValue<ushort> ClientPort
        {
            get
            {
                return _ClientPort;
            }
        }

        private static SettingValue<ScreenshotFormat> _ScreenshotsFormat;
        public static ISettingValue<ScreenshotFormat> ScreenshotsFormat
        {
            get
            {
                return _ScreenshotsFormat;
            }
        }

        private static SettingValue<string> _ScreenshotsLocation;
        public static ISettingValue<string> ScreenshotsLocation
        {
            get
            {
                return _ScreenshotsLocation;
            }
        }

        private static PendingSettingValue<string> _VirtualUserPath;
        public static IPendingSettingValue<string> VirtualUserPath
        {
            get
            {
                return _VirtualUserPath;
            }
        }

        private static SettingValue<ButtonAction> _ActionInactiveLClick;
        public static ISettingValue<ButtonAction> ActionInactiveLClick
        {
            get
            {
                return _ActionInactiveLClick;
            }
        }

        private static SettingValue<ButtonAction> _ActionActiveLClick;
        public static ISettingValue<ButtonAction> ActionActiveLClick
        {
            get
            {
                return _ActionActiveLClick;
            }
        }

        private static SettingValue<ButtonAction> _ActionActiveLPress;
        public static ISettingValue<ButtonAction> ActionActiveLPress
        {
            get
            {
                return _ActionActiveLPress;
            }
        }

        private static SettingValue<DailiesMode> _ShowDailies;
        public static ISettingValue<DailiesMode> ShowDailies
        {
            get
            {
                return _ShowDailies;
            }
        }

        private static KeyedProperty<byte, bool> _HiddenDailyCategories;
        public static IKeyedProperty<byte, bool> HiddenDailyCategories
        {
            get
            {
                return _HiddenDailyCategories;
            }
        }

        private static SettingValue<NetworkAuthorizationFlags> _NetworkAuthorization;
        public static ISettingValue<NetworkAuthorizationFlags> NetworkAuthorization
        {
            get
            {
                return _NetworkAuthorization;
            }
        }

        private static SettingValue<string> _ScreenshotNaming;
        public static ISettingValue<string> ScreenshotNaming
        {
            get
            {
                return _ScreenshotNaming;
            }
        }

        private static SettingValue<ScreenshotConversionOptions> _ScreenshotConversion;
        public static ISettingValue<ScreenshotConversionOptions> ScreenshotConversion
        {
            get
            {
                return _ScreenshotConversion;
            }
        }

        private static SettingValue<bool> _DeleteCrashLogsOnLaunch;
        public static ISettingValue<bool> DeleteCrashLogsOnLaunch
        {
            get
            {
                return _DeleteCrashLogsOnLaunch;
            }
        }

        private static SettingValue<Settings.ProcessPriorityClass> _ProcessPriority;
        public static ISettingValue<Settings.ProcessPriorityClass> ProcessPriority
        {
            get
            {
                return _ProcessPriority;
            }
        }

        private static SettingValue<long> _ProcessAffinity;
        public static ISettingValue<long> ProcessAffinity
        {
            get
            {
                return _ProcessAffinity;
            }
        }

        private static SettingValue<bool> _PrioritizeCoherentUI;
        public static ISettingValue<bool> PrioritizeCoherentUI
        {
            get
            {
                return _PrioritizeCoherentUI;
            }
        }

        private static SettingValue<NotificationScreenAttachment> _NotesNotifications;
        public static ISettingValue<NotificationScreenAttachment> NotesNotifications
        {
            get
            {
                return _NotesNotifications;
            }
        }

        private static SettingValue<byte> _MaxPatchConnections;
        public static ISettingValue<byte> MaxPatchConnections
        {
            get
            {
                return _MaxPatchConnections;
            }
        }

        private static SettingValue<bool> _StyleShowColor;
        public static ISettingValue<bool> StyleShowColor
        {
            get
            {
                return _StyleShowColor;
            }
        }

        private static SettingValue<bool> _StyleHighlightFocused;
        public static ISettingValue<bool> StyleHighlightFocused
        {
            get
            {
                return _StyleHighlightFocused;
            }
        }

        private static SettingValue<bool> _WindowIcon;
        public static ISettingValue<bool> WindowIcon
        {
            get
            {
                return _WindowIcon;
            }
        }

        private static SettingValue<bool> _UseGw2IconForShortcuts;
        public static ISettingValue<bool> UseGw2IconForShortcuts
        {
            get
            {
                return _UseGw2IconForShortcuts;
            }
        }

        private static SettingValue<bool> _AccountBarEnabled;
        private static SettingValue<AccountBarOptions> _AccountBarOptions;
        private static SettingValue<AccountBarStyles> _AccountBarStyle;
        private static SettingValue<SortMode> _AccountBarSortingMode;
        private static SettingValue<SortOrder> _AccountBarSortingOrder;
        private static SettingValue<ScreenAnchor> _AccountBarDocked;

        public static class AccountBar
        {
            public static ISettingValue<bool> Enabled
            {
                get
                {
                    return _AccountBarEnabled;
                }
            }

            public static ISettingValue<AccountBarOptions> Options
            {
                get
                {
                    return _AccountBarOptions;
                }
            }

            public static ISettingValue<AccountBarStyles> Style
            {
                get
                {
                    return _AccountBarStyle;
                }
            }

            public static ISettingValue<SortMode> SortingMode
            {
                get
                {
                    return _AccountBarSortingMode;
                }
            }

            public static ISettingValue<SortOrder> SortingOrder
            {
                get
                {
                    return _AccountBarSortingOrder;
                }
            }

            public static ISettingValue<ScreenAnchor> Docked
            {
                get
                {
                    return _AccountBarDocked;
                }
            }
        }

        private static SettingValue<byte> _LimitActiveAccounts;
        public static ISettingValue<byte> LimitActiveAccounts
        {
            get
            {
                return _LimitActiveAccounts;
            }
        }

        private static SettingValue<bool> _DelayLaunchUntilLoaded;
        public static ISettingValue<bool> DelayLaunchUntilLoaded
        {
            get
            {
                return _DelayLaunchUntilLoaded;
            }
        }

        private static SettingValue<byte> _DelayLaunchSeconds;
        public static ISettingValue<byte> DelayLaunchSeconds
        {
            get
            {
                return _DelayLaunchSeconds;
            }
        }

        private static PendingSettingValue<bool> _LocalizeAccountExecution;
        public static IPendingSettingValue<bool> LocalizeAccountExecution
        {
            get
            {
                return _LocalizeAccountExecution;
            }
        }

        private static SettingValue<LauncherPoints> _LauncherAutologinPoints;
        public static ISettingValue<LauncherPoints> LauncherAutologinPoints
        {
            get
            {
                return _LauncherAutologinPoints;
            }
        }

        public static bool DisableAutomaticLogins
        {
            get;
            set;
        }

        public static bool Silent
        {
            get;
            set;
        }

        public static bool ReadOnly
        {
            get;
            set;
        }

        public static ushort GetNextUID()
        {
            return (ushort)(_accountUID + 1);
        }

        public static IAccount CreateAccount()
        {
            lock (_Accounts)
            {
                var account = new Account(++_accountUID)
                {
                    _SortKey = ++_accountSortKey,
                };
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

        public static void RemoveDatFile(IDatFile file)
        {
            lock (_DatFiles)
            {
                if (_datUID == file.UID)
                    _datUID--;
                _DatFiles.Remove(file.UID);
            }
        }

        public static IGfxFile CreateGfxFile()
        {
            lock (_GfxFiles)
            {
                var gfx = new GfxFile(++_gfxUID);
                _GfxFiles.Add(gfx.UID, new SettingValue<IGfxFile>(gfx));
                return gfx;
            }
        }

        public static void RemoveGfxFile(IGfxFile file)
        {
            lock (_GfxFiles)
            {
                if (_gfxUID == file.UID)
                    _gfxUID--;
                _GfxFiles.Remove(file.UID);
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
