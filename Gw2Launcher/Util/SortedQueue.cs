using System;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gw2Launcher.Util
{
    /// <summary>
    /// A queue where each TValue is unique and sorted by TKey
    /// </summary>
    class SortedQueue<TKey, TValue> : IEnumerable<TValue>, ICollection<TValue>
        where TKey : IComparable<TKey>
    {
        private class Node
        {
            public Node previous, next;
            public TKey key;
            public TValue value;
        }

        private Comparison<TKey> comparison;
        private int count;
        private Node first, last;
        private Dictionary<TValue, Node> nodes;

        public SortedQueue(Comparison<TKey> comparison)
        {
            if (comparison == null)
            {
                this.comparison = DefaultComparison;
            }
            else
            {
                this.comparison = comparison;
            }

            this.nodes = new Dictionary<TValue, Node>();
        }

        public SortedQueue()
            : this(null)
        {
        }

        private int DefaultComparison(TKey a, TKey b)
        {
            return a.CompareTo(b);
        }

        public void Add(TKey key, TValue item)
        {
            Node n;
            if (nodes.TryGetValue(item, out n))
            {
                var c = comparison(key, n.key); //key.CompareTo(n.key);
                n.key = key;

                if (c == 0 || count == 1)
                {
                    return;
                }
                else if (c > 0)
                {
                    if (n == last || comparison(key,n.next.key) <= 0) //key.CompareTo(n.next.key) <= 0)
                        return;
                    Remove(n);
                    Add(n);
                }
                else
                {
                    if (n == first || comparison(key, n.previous.key) >= 0) //key.CompareTo(n.previous.key) >= 0)
                        return;
                    Remove(n);
                    Add(n);
                }
            }
            else
            {
                n = new Node()
                {
                    key = key,
                    value = item,
                };
                nodes.Add(item, n);
                Add(n);
            }
        }

        private void Add(Node n)
        {
            if (count > 1)
            {
                if (comparison(n.key, first.key) <= 0) //n.key.CompareTo(first.key) <= 0)
                {
                    first.previous = n;
                    n.next = first;
                    n.previous = null;
                    first = n;
                }
                else if (comparison(n.key,last.key) >= 0) //n.key.CompareTo(last.key) >= 0)
                {
                    last.next = n;
                    n.next = null;
                    n.previous = last;
                    last = n;
                }
                else
                {
                    var next = first.next;
                    do
                    {
                        if (comparison(n.key,next.key) <=0) //n.key.CompareTo(next.key) <= 0)
                        {
                            n.previous = next.previous;
                            n.next = next;
                            n.previous.next = n;
                            n.next.previous = n;

                            break;
                        }
                        else
                            next = next.next;
                    }
                    while (true);
                }
            }
            else if (count == 0)
            {
                first = last = n;
                n.next = n.previous = null;
            }
            else if (count == 1)
            {
                var c = comparison(n.key, first.key); //n.key.CompareTo(first.key);
                if (c < 0)
                {
                    first = n;
                    n.next = last;
                    n.previous = null;
                    last.previous = n;
                }
                else
                {
                    last = n;
                    n.next = null;
                    n.previous = first;
                    first.next = n;
                }
            }

            count++;
        }

        public bool Remove(TValue item)
        {
            Node n;
            if (nodes.TryGetValue(item, out n))
            {
                nodes.Remove(item);
                Remove(n);
                return true;
            }
            return false;
        }

        private void Remove(Node n)
        {
            count--;

            if (count == 0)
            {
                first = last = null;
            }
            else if (n == first)
            {
                first = n.next;
                first.previous = null;
                if (count == 1)
                    last = first;
            }
            else if (n == last)
            {
                last = n.previous;
                last.next = null;
            }
            else
            {
                var prev = n.previous;
                var next = n.next;

                prev.next = next;
                next.previous = prev;
            }
        }

        public TValue Dequeue()
        {
            if (count > 0)
            {
                var v = first.value;
                nodes.Remove(v);
                count--;

                if (count > 1)
                {
                    first = first.next;
                    first.previous = null;
                }
                else if (count == 1)
                {
                    first = last;
                    first.previous = null;
                }
                else
                {
                    first = last = null;
                }

                return v;
            }

            return default(TValue);
        }

        public bool Contains(TValue item)
        {
            return nodes.ContainsKey(item);
        }

        public TValue First
        {
            get
            {
                if (count > 0)
                    return first.value;
                return default(TValue);
            }
        }

        public TValue Last
        {
            get
            {
                if (count > 0)
                    return last.value;
                return default(TValue);
            }
        }

        public int Count
        {
            get
            {
                return count;
            }
        }

        public IEnumerator<TValue> GetEnumerator()
        {
            var n = first;

            while (n != null)
            {
                yield return n.value;
                n = n.next;
            }
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public bool IsSynchronized
        {
            get 
            {
                return false;
            }
        }

        public object SyncRoot
        {
            get 
            {
                return this;
            }
        }

        public void Add(TValue item)
        {
            Add(default(TKey), item);
        }

        public void Clear()
        {
            count = 0;
            first = last = null;
            nodes.Clear();
        }

        public void CopyTo(TValue[] array, int arrayIndex)
        {
            var n = first;

            while (n != null)
            {
                array[arrayIndex++] = n.value;
                n = n.next;
            }
        }

        public bool IsReadOnly
        {
            get
            {
                return false;
            }
        }
    }
}
