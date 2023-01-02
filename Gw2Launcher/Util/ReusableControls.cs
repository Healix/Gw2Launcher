using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Gw2Launcher.Util
{
    class ReusableControls : IDisposable
    {
        private interface IControls : IDisposable
        {
            void ReleaseAll();
            void Initialize(int capacity);
        }

        public interface IResult : IDisposable, IEnumerable<Control>
        {
            int Count
            {
                get;
            }
            bool HasNext
            {
                get;
            }
            Control[] New
            {
                get;
            }
        }

        public interface IResult<T> : IResult
            where T : Control
        {
            T GetNext();
            void HideRemaining();
        }

        private class Result<T> : IResult<T>
            where T : Control
        {
            public Controls<T> owner;
            public T[] controls, controlsNew;
            public int index;
            public int count;
            public int offset;

            public Result(Controls<T> owner, T[] controls, int offset, int count, T[] controlsNew)
            {
                this.owner = owner;
                this.controls = controls;
                this.controlsNew = controlsNew;
                this.count = count;
                this.offset = offset;
            }

            public T GetNext()
            {
                if (index < count)
                    return controls[offset + index++];
                else
                    throw new IndexOutOfRangeException();
            }

            public void HideRemaining()
            {
                while (index < count)
                {
                    GetNext().Visible = false;
                }
            }

            public bool HasNext
            {
                get
                {
                    return index < count;
                }
            }

            public Control[] New
            {
                get
                {
                    return controlsNew;
                }
            }

            public int Count
            {
                get
                {
                    return count;
                }
            }

            public void Dispose()
            {
                if (index < count)
                {
                    owner.Release(controls, offset + index, count - index);
                    index = count;
                }
            }

            public IEnumerator<Control> GetEnumerator()
            {
                while (index < count)
                {
                    yield return (Control)GetNext();
                }
            }

            System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
            {
                return GetEnumerator();
            }
        }

        public class Controls<T> : IControls, IDisposable
            where T : Control
        {
            private int count, index, capacity;
            private bool canReleaseAll;
            private T[] controls;

            public Controls()
            {
                canReleaseAll = true;
            }

            public void Initialize(int capacity)
            {
                if (this.capacity == 0)
                {
                    controls = new T[this.capacity = capacity];
                }
            }

            /// <summary>
            /// Returns all of the available controls and creates new one if necessary
            /// </summary>
            public IResult<T> CreateOrAll(int count, Func<T> createNew)
            {
                int available = count - index;
                if (available > this.count)
                    return Create(available, createNew);
                return Create(this.count, createNew);
            }

            /// <summary>
            /// Returns the specified number of controls, creating new ones if necessary
            /// </summary>
            public IResult<T> Create(int count, Func<T> createNew)
            {
                int k = index;

                var add = count - this.count + index;
                if (add > 0)
                {
                    T[] _controls, _new;

                    if (this.capacity == 0)
                    {
                        controls = new T[this.count = this.capacity = count];

                        for (var i = 0; i < count; i++)
                        {
                            controls[i] = createNew();
                        }

                        index = count;
                        return new Result<T>(this, controls, 0, count, controls);
                    }

                    var _count = this.count + add;
                    _controls = this.controls;

                    if (_count > this.capacity)
                    {
                        this.capacity = _count + 10;
                        this.controls = new T[this.capacity];
                        System.Array.Copy(_controls, controls, this.count);
                    }

                    for (var i = this.count; i < _count; i++)
                    {
                        this.controls[i] = createNew();
                    }

                    _new = new T[add];

                    System.Array.Copy(this.controls, this.count, _new, 0, add);

                    this.count = _count;

                    index += count;
                    return new Result<T>(this, this.controls, k, count, _new);
                }

                index += count;
                return new Result<T>(this, controls, k, count, null);

                //_controls = new T[count];
                //Array.Copy(controls, index, _controls, 0, count);

                //index += count;
                //return new Result<T>(this, _controls, _new);
            }

            /// <summary>
            /// Adds the controls to the buffer.
            /// Warning: disables ReleaseAll until all controls are returned
            /// </summary>
            public void Release(ICollection<T> controls)
            {
                foreach (var c in controls)
                {
                    this.controls[index--] = c;
                }
                canReleaseAll = index == 0;
            }

            /// <summary>
            /// Adds the controls to the buffer. 
            /// Warning: disables ReleaseAll until all controls are returned
            /// </summary>
            public void Release(T[] controls, int startIndex, int count)
            {
                for (var i = 0; i < count; i++)
                {
                    this.controls[index--] = controls[startIndex + i];
                }
                canReleaseAll = index == 0;
            }

            /// <summary>
            /// All kept controls will be available.
            /// Warning: this cannot be done if other controls have been individually released
            /// </summary>
            public void ReleaseAll()
            {
                if (!canReleaseAll)
                    throw new InvalidOperationException("Can't release partially returned controls");
                index = 0;
            }

            public void Dispose()
            {
                for (var i = 0; i < count; i++)
                {
                    controls[i].Dispose();
                }
            }
        }

        private Dictionary<Type, IControls> cache;

        public ReusableControls()
        {
            cache = new Dictionary<Type, IControls>();
        }

        public IResult<T> Create<T>(int count, Func<T> createNew)
            where T : Control
        {
            Controls<T> controls;
            var t = typeof(T);
            IControls o;

            if (!cache.TryGetValue(t, out o))
                cache[t] = controls = new Controls<T>();
            else
                controls = (Controls<T>)o;

            return controls.Create(count, createNew);
        }

        public IResult<T> CreateOrAll<T>(int count, Func<T> createNew)
            where T : Control
        {
            Controls<T> controls;
            var t = typeof(T);
            IControls o;

            if (!cache.TryGetValue(t, out o))
                cache[t] = controls = new Controls<T>();
            else
                controls = (Controls<T>)o;

            return controls.CreateOrAll(count, createNew);
        }

        public void Release<T>(ICollection<T> controls)
            where T : Control
        {
            ((Controls<T>)cache[typeof(T)]).Release(controls);
        }

        public void ReleaseAll<T>()
            where T : Control
        {
            cache[typeof(T)].ReleaseAll();
        }

        public void Initialize<T>(int capacity)
            where T : Control
        {
            cache[typeof(T)].Initialize(capacity);
        }

        public void ReleaseAll()
        {
            foreach (var o in cache.Values)
                o.ReleaseAll();
        }

        public void Dispose()
        {
            foreach (var o in cache.Values)
            {
                o.Dispose();
            }
        }
    }
}
