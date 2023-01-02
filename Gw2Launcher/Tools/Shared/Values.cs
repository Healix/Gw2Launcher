using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gw2Launcher.Tools.Shared
{
    public class Values<T> : IDisposable
    {
        public enum ValueState : byte
        {
            None,
            Loading,
            Loaded,
            Disposing,
        }

        public interface IUpdate : IDisposable
        {
            /// <summary>
            /// Sets the source to a value, wrapping it as a shared value
            /// </summary>
            void SetValue(T value);
            /// <summary>
            /// Sets the source to a shared value
            /// </summary>
            /// <param name="value">The value should be a new subscriber</param>
            void SetValue(IValue value);
            /// <summary>
            /// Sets the source to another source; changes to the source will be reflected.
            /// </summary>
            /// <param name="value">The value should be a new subscriber</param>
            void SetValue(IValueSource value);
        }

        /// <summary>
        /// Respresents the source of a shared value
        /// </summary>
        public interface IValueSource : IDisposable
        {
            event EventHandler StateChanged;

            ValueState State
            {
                get;
            }

            /// <summary>
            /// Returns an instance of the current value; changes to the source value will not affect the instance
            /// </summary>
            IValue GetValue();

            /// <summary>
            /// Allows the value to be set, or returns null if an update is already in progress
            /// </summary>
            IUpdate BeginUpdate();

            /// <summary>
            /// Allows the value to be set
            /// </summary>
            /// <param name="onUpdate">Callback to set the value</param>
            /// <returns>False if an update is already in progress</returns>
            bool BeginUpdate(Action<IUpdate> onUpdate);

            /// <summary>
            /// Allows the value to be set
            /// </summary>
            /// <param name="onUpdate">Callback to set the returned value</param>
            /// <returns>False if an update is already in progress</returns>
            bool BeginUpdate(Func<T> onUpdate);

            /// <summary>
            /// Allows the value to be set if it's currently not; returns null if the value is already set or an update is in progress
            /// </summary>
            IUpdate BeginLoad();

            /// <summary>
            /// Allows the value to be set if it's currently not
            /// </summary>
            /// <param name="onUpdate">Callback to set the value</param>
            /// <returns>False if the value is already set or is in progress</returns>
            bool BeginLoad(Action<IUpdate> onUpdate);

            /// <summary>
            /// Allows the value to be set if it's currently not
            /// </summary>
            /// <param name="onUpdate">Callback to set the returned value</param>
            /// <returns>False if the value is already set or is in progress</returns>
            bool BeginLoad(Func<T> onUpdate);

            bool IsLoaded
            {
                get;
            }

            ushort Subscribers
            {
                get;
            }

            bool IsDisposed
            {
                get;
            }

            object Key
            {
                get;
            }

            /// <summary>
            /// Returns a new subscriber for this source
            /// </summary>
            IValueSource Clone();
        }

        /// <summary>
        /// Represents a subscriber to a shared value that will be disposed once all subscribers are disposed
        /// </summary>
        public interface IValue : IDisposable
        {
            T Value
            {
                get;
            }

            bool IsDisposed
            {
                get;
            }

            /// <summary>
            /// Returns a new subscriber for this value
            /// </summary>
            IValue Clone();

            /// <summary>
            /// The source for this value that can affect all subscribers
            /// </summary>
            SharedSource Source
            {
                get;
            }
        }

        /// <summary>
        /// Allows a value to be shared with multiple subscribers and automatically disposed once no subscribers remain
        /// </summary>
        public class SharedSource : IDisposable
        {
            private class SharedValue : IValue
            {
                private SharedSource source;

                public SharedValue(SharedSource source)
                {
                    this.source = source;
                }

                ~SharedValue()
                {
                    Dispose();
                }

                public T Value
                {
                    get
                    {
                        return source.value;
                    }
                }

                public SharedSource Source
                {
                    get
                    {
                        return source;
                    }
                }

                public bool IsDisposed
                {
                    get
                    {
                        return source == null;
                    }
                }

                public IValue Clone()
                {
                    return source.GetValue();
                }

                public void Dispose()
                {
                    lock (this)
                    {
                        if (source != null)
                        {
                            GC.SuppressFinalize(this);

                            source.Dispose(false);
                            source = null;
                        }
                    }
                }
            }

            public event EventHandler SubscribersChanged;

            private T value;
            private ushort subscribers;
            private bool disposed;

            private SharedSource(T value)
            {
                this.value = value;
            }

            ~SharedSource()
            {
                Dispose(true);
            }

            /// <summary>
            /// Creates a shared value; the original value will be disposed once all shares are disposed
            /// </summary>
            public static IValue Create(T value)
            {
                return new SharedSource(value).GetValue();
            }

            /// <summary>
            /// Returns a new subscriber for this value
            /// </summary>
            public IValue GetValue()
            {
                lock (this)
                {
                    if (IsDisposed)
                        return null;
                    ++Subscribers;
                    return new SharedValue(this);
                }
            }

            public ushort Subscribers
            {
                get
                {
                    return subscribers;
                }
                set
                {
                    subscribers = value;
                    if (SubscribersChanged != null)
                        SubscribersChanged(this, EventArgs.Empty);
                }
            }

            public bool IsDisposed
            {
                get
                {
                    return disposed;
                }
            }

            private void Dispose(bool disposing)
            {
                lock (this)
                {
                    if (disposing || subscribers > 0 && --Subscribers == 0)
                    {
                        GC.SuppressFinalize(this);

                        disposed = true;
                        SubscribersChanged = null;

                        if (subscribers > 0)
                            Subscribers = 0;

                        if (value is IDisposable)
                        {
                            using ((IDisposable)value)
                            {
                                value = default(T);
                            }
                        }
                        else
                            value = default(T);
                    }
                }
            }

            /// <summary>
            /// Disposes the value even if there are remaining subscribers
            /// </summary>
            public void Dispose()
            {
                Dispose(true);
            }
        }

        /// <summary>
        /// Container for a source and/or value
        /// </summary>
        public class SourceValue : IDisposable
        {
            /// <summary>
            /// Occurs when the source value has changed/loaded; the value should be updated if desired
            /// </summary>
            public event EventHandler SourceLoaded;
            /// <summary>
            /// Occurs when the source is requesting the value to be disposed; can be ignored to retain the value
            /// </summary>
            public event EventHandler SourceDisposing;

            private IValueSource source;
            private IValue value;

            public SourceValue(IValueSource source)
            {
                this.Source = source;
                this.Value = source.GetValue();
            }

            public SourceValue(IValue value)
            {
                this.Value = value;
            }

            ~SourceValue()
            {
                Dispose();
            }

            /// <summary>
            /// Existing source will be disposed if changed
            /// </summary>
            public IValueSource Source
            {
                get
                {
                    return source;
                }
                set
                {
                    lock (this)
                    {
                        using (source)
                        {
                            if (source != null)
                                source.StateChanged -= source_StateChanged;
                            source = value;
                            if (value != null)
                                value.StateChanged += source_StateChanged;
                        }
                    }
                }
            }

            /// <summary>
            /// Existing value will be disposed if changed
            /// </summary>
            public IValue Value
            {
                get
                {
                    return value;
                }
                set
                {
                    lock (this)
                    {
                        using (this.value)
                        {
                            this.value = value;
                        }
                    }
                }
            }

            public bool HasValue
            {
                get
                {
                    return value != null;
                }
            }

            /// <summary>
            /// Sets the value from the source
            /// </summary>
            /// <param name="overwrite">False to only update the value if it's not already set</param>
            /// <returns>True if the value is set</returns>
            public bool RefreshValue(bool overwrite = false)
            {
                lock(this)
                {
                    if (!overwrite && value != null)
                        return true;

                    if (source != null)
                    {
                        var v = source.GetValue();

                        if (v != null)
                        {
                            Value = v;

                            return true;
                        }
                    }
                }

                return false;
            }

            public T GetValue()
            {
                lock(this)
                {
                    if (value != null || source != null && RefreshValue(false))
                    {
                        return value.Value;
                    }
                    return default(T);
                }
            }

            void source_StateChanged(object sender, EventArgs e)
            {
                switch (source.State)
                {
                    case ValueState.Loaded:

                        if (SourceLoaded != null)
                            SourceLoaded(this, e);
                        else
                        {
                            lock (this)
                            {
                                if (value == null)
                                    Value = source.GetValue();
                            }
                        }

                        break;
                    case ValueState.Disposing:

                        if (SourceDisposing != null)
                            SourceDisposing(this, e);

                        break;
                }
            }

            public void Dispose()
            {
                lock (this)
                {
                    SourceLoaded = null;
                    SourceDisposing = null;

                    if (value != null)
                        Value = null;
                    if (source != null)
                        Source = null;
                }
            }
        }

        private class ValueSource : IDisposable
        {
            /// <summary>
            /// A subscriber copy for the source value
            /// </summary>
            private class SourceSubscriber : IValueSource
            {
                public event EventHandler StateChanged;

                private ValueSource source;

                public SourceSubscriber(ValueSource source)
                {
                    this.source = source;

                    source.StateChanged += OnStateChanged;
                }

                ~SourceSubscriber()
                {
                    Dispose();
                }

                public object Key
                {
                    get
                    {
                        return this.source.Key;
                    }
                }

                public ushort Subscribers
                {
                    get
                    {
                        return this.source.subscribers;
                    }
                }

                public ValueState State
                {
                    get
                    {
                        return source.State;
                    }
                }

                public bool IsLoaded
                {
                    get
                    {
                        return source.State == ValueState.Loaded;
                    }
                }

                public bool IsDisposed
                {
                    get
                    {
                        return source == null || source.IsDisposed;
                    }
                }

                public IValue GetValue()
                {
                    var v = source.value;
                    if (v == null)
                        return null;
                    return v.Clone();
                }

                public IUpdate BeginLoad()
                {
                    return BeginUpdate(false);
                }

                public bool BeginLoad(Action<IUpdate> onUpdate)
                {
                    return BeginUpdate(false, onUpdate);
                }

                public bool BeginLoad(Func<T> onUpdate)
                {
                    return BeginUpdate(false, onUpdate);
                }

                public IUpdate BeginUpdate()
                {
                    return BeginUpdate(true);
                }

                public bool BeginUpdate(Action<IUpdate> onUpdate)
                {
                    return BeginUpdate(true, onUpdate);
                }

                public bool BeginUpdate(Func<T> onUpdate)
                {
                    return BeginUpdate(true, onUpdate);
                }

                private bool BeginUpdate(bool canOverwrite, Action<IUpdate> onUpdate)
                {
                    var v = BeginUpdate(canOverwrite);

                    if (v != null)
                    {
                        using (v)
                        {
                            onUpdate(v);
                        }

                        return true;
                    }

                    return false;
                }

                private bool BeginUpdate(bool canOverwrite, Func<T> onUpdate)
                {
                    var v = BeginUpdate(canOverwrite);

                    if (v != null)
                    {
                        using (v)
                        {
                            v.SetValue(onUpdate());
                        }

                        return true;
                    }

                    return false;
                }

                public IUpdate BeginUpdate(bool canOverwrite)
                {
                    lock (source)
                    {
                        if (!canOverwrite && IsLoaded)
                            return null;

                        if (source.State != ValueState.Loading && !source.IsDisposed)
                        {
                            source.State = ValueState.Loading;
                            return new SetValueToken(source);
                        }
                        else
                        {
                            return null;
                        }
                    }
                }

                void OnStateChanged(object sender, EventArgs e)
                {
                    if (StateChanged != null)
                        StateChanged(this, EventArgs.Empty);

                    if (source != null && source.State == ValueState.Disposing)
                        Dispose();
                }

                public IValueSource Clone()
                {
                    return source.GetValue();
                }

                public void Dispose()
                {
                    GC.SuppressFinalize(this);

                    lock (this)
                    {
                        if (source != null)
                        {
                            StateChanged = null;
                            source.StateChanged -= OnStateChanged;
                            source.Dispose(false);
                            source = null;
                        }
                    }
                }
            }

            /// <summary>
            /// Token for changing the source value
            /// </summary>
            private class SetValueToken : IUpdate
            {
                private ValueSource source;
                private object value;
                private bool hasValue;

                public SetValueToken(ValueSource value)
                {
                    this.source = value;
                }

                ~SetValueToken()
                {
                    Dispose();
                }

                public void SetValue(T value)
                {
                    this.hasValue = true;
                    this.value = value;
                }

                public void SetValue(IValue value)
                {
                    this.hasValue = true;
                    this.value = value;
                }

                public void SetValue(IValueSource value)
                {
                    this.hasValue = true;
                    this.value = value;
                }

                public void Dispose()
                {
                    GC.SuppressFinalize(this);

                    lock (this)
                    {
                        if (source != null)
                        {
                            lock (source)
                            {
                                if (hasValue)
                                {
                                    if (source.IsDisposed)
                                    {
                                        if (value is IDisposable)
                                        {
                                            using ((IDisposable)value) { }
                                        }
                                    }
                                    else if (value is IValue)
                                        source.SetValueSource((IValue)value);
                                    else if (value is T)
                                        source.SetValueSource((T)value);
                                    else if (value is IValueSource)
                                        source.SetValueSource((IValueSource)value);
                                    else if (value == null)
                                        source.SetValueSource(SharedSource.Create(default(T)));
                                    else
                                    {
                                        if (source.State == ValueState.Loading)
                                            source.State = ValueState.None;
                                    }
                                }
                                else if (source.State == ValueState.Loading)
                                    source.State = ValueState.None;
                            }
                            source = null;
                        }
                    }
                }
            }

            public event EventHandler StateChanged;

            private Values<T> parent;
            private ushort subscribers;
            private IValue value;
            private IValueSource source; //only used when using another source as the value for this one
            private ValueState state;

            public ValueSource(Values<T> parent, object key)
            {
                this.parent = parent;
                this.Key = key;
            }

            ~ValueSource()
            {
                Dispose(true);
            }

            public object Key
            {
                get;
                private set;
            }

            public IValue Value
            {
                get
                {
                    return value;
                }
                set
                {
                    using (this.value)
                    {
                        this.value = value;
                        this.State = value == null ? ValueState.None : ValueState.Loaded;
                    }
                }
            }

            public IValueSource Source
            {
                get
                {
                    return source;
                }
                set
                {
                    if (source != value)
                    {
                        using (source)
                        {
                            if (source != null)
                                source.StateChanged -= source_StateChanged;
                            source = value;
                            if (value != null)
                                value.StateChanged += source_StateChanged;
                        }
                    }
                }
            }

            public ValueState State
            {
                get
                {
                    return state;
                }
                set
                {
                    state = value;
                    if (StateChanged != null)
                        StateChanged(this, EventArgs.Empty);
                }
            }

            public ushort Subscribers
            {
                get
                {
                    return subscribers;
                }
            }

            public bool IsDisposed
            {
                get
                {
                    return state == ValueState.Disposing;
                }
            }

            public void SetValueSource(T value)
            {
                SetValueSource(SharedSource.Create(value));
            }

            public void SetValueSource(IValue value)
            {
                lock (parent)
                {
                    Source = null;
                    Value = value == null || value.IsDisposed ? null : value;
                }
            }

            public void SetValueSource(IValueSource value)
            {
                lock (parent)
                {
                    if (value.IsDisposed)
                    {
                        Source = null;
                        Value = null;
                    }
                    else
                    {
                        Source = value;

                        if (value.IsLoaded)
                        {
                            Value = value.GetValue();
                        }
                    }
                }
            }

            public IValueSource GetValue()
            {
                lock (parent)
                {
                    if (IsDisposed)
                        return null;
                    ++subscribers;
                    return new SourceSubscriber(this);
                }
            }

            private void Dispose(bool disposing)
            {
                lock (parent)
                {
                    if (disposing || subscribers > 0 && --subscribers == 0 && parent.RemoveOnRelease)
                    {
                        GC.SuppressFinalize(this);

                        if (!IsDisposed)
                        {
                            State = ValueState.Disposing;
                            parent.Remove(this);
                        }

                        StateChanged = null;
                        Source = null;

                        using (value)
                        {
                            value = null;
                        }
                    }
                }
            }

            public void Dispose()
            {
                lock (parent)
                {
                    if (IsDisposed)
                        return;
                    State = ValueState.Disposing;
                }

                Dispose(true);
            }

            void source_StateChanged(object sender, EventArgs e)
            {
                switch (source.State)
                {
                    case ValueState.Loaded:

                        lock (parent)
                        {
                            if (!IsDisposed)
                            {
                                Value = source.GetValue();
                            }
                        }

                        break;
                    case ValueState.Disposing:

                        Source = null;

                        break;
                }
            }
        }

        private Dictionary<object, ValueSource> values;

        public Values(bool removeOnRelease = false)
        {
            this.values = new Dictionary<object, ValueSource>();
            this.RemoveOnRelease = removeOnRelease;
        }

        /// <summary>
        /// If true, values will be removed once all subscribers are disposed
        /// </summary>
        public bool RemoveOnRelease
        {
            get;
            private set;
        }

        /// <summary>
        /// Returns a source value for the specified key, creating a new one if it doesn't already exist.
        /// </summary>
        public IValueSource GetValue(object key)
        {
            lock (this)
            {
                ValueSource v;
                if (!this.values.TryGetValue(key, out v) || v.IsDisposed)
                {
                    this.values[key] = v = new ValueSource(this, key);
                }
                return v.GetValue();
            }
        }

        /// <summary>
        /// Returns a source value for the specified key, if it exists.
        /// </summary>
        public bool TryGetValue(object key, out IValueSource source)
        {
            lock (this)
            {
                ValueSource v;
                if (!this.values.TryGetValue(key, out v) || v.IsDisposed)
                {
                    source = null;
                    return false;
                }
                source = v.GetValue();
                return true;
            }
        }

        private void Remove(ValueSource v)
        {
            lock (this)
            {
                ValueSource existing;
                if (this.values.TryGetValue(v.Key, out existing) && object.ReferenceEquals(existing, v))
                {
                    this.values.Remove(v.Key);
                }
            }
        }

        /// <summary>
        /// Removes the value with the specified key and informs any subscribers that the value should be disposed
        /// </summary>
        public bool Remove(object key)
        {
            ValueSource v;

            lock (this)
            {
                if (!this.values.TryGetValue(key, out v))
                    return false;
                this.values.Remove(key);
            }

            v.Dispose();

            return true;
        }

        /// <summary>
        /// Removes any values that aren't subscribed to
        /// </summary>
        public void Purge()
        {
            lock (this)
            {
                var count = this.values.Count;
                var k = 0;
                ValueSource[] values = null;

                foreach (var v in this.values.Values)
                {
                    if (v.Subscribers == 0)
                    {
                        if (k == 0)
                            values = new ValueSource[count];
                        values[k++] = v;
                    }

                    --count;
                }

                if (k > 0)
                {
                    for (var i = 0; i < k; i++)
                    {
                        using (values[i])
                        {
                            this.values.Remove(values[i].Key);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Clears all values and informs any subscribers that the value should be disposed
        /// </summary>
        public void Clear()
        {
            Dictionary<object, ValueSource> values;

            lock (this)
            {
                if (this.values.Count == 0)
                    return;
                values = this.values;
                this.values = new Dictionary<object, ValueSource>();
            }

            foreach (var v in values.Values)
            {
                v.Dispose();
            }
        }

        public void Dispose()
        {
            foreach (var v in values.Values)
            {
                v.Dispose();
            }
        }

        public int Count
        {
            get
            {
                return this.values.Count;
            }
        }

        public object[] Keys
        {
            get
            {
                lock (this)
                {
                    return this.values.Keys.ToArray();
                }
            }
        }
    }
}
