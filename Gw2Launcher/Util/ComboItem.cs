using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Gw2Launcher.Util
{
    class ComboItem<T> : IComparable<ComboItem<T>>, IEquatable<ComboItem<T>>
    {
        private bool hasValue;
        private T value;
        private Func<T> onGet;

        public ComboItem(T value, string text)
        {
            this.Value = value;
            this.Text = text;
        }

        public ComboItem(Func<T> onGet, string text)
        {
            this.onGet = onGet;
            this.Text = text;
        }

        public T Value
        {
            get
            {
                if (hasValue)
                    return value;
                return onGet();
            }
            set
            {
                hasValue = true;
                this.value = value;
            }
        }

        public string Text
        {
            get;
            set;
        }

        public override string ToString()
        {
            return this.Text;
        }

        public bool Equals(ComboItem<T> other)
        {
            if (this.hasValue)
                return this.Value.Equals(other.Value);
            return this.onGet.Equals(other.onGet);
        }

        public int CompareTo(ComboItem<T> other)
        {
            if (this.Value is IComparable<T>)
                return ((IComparable<T>)this.Value).CompareTo(other.Value);

            return this.Text.CompareTo(other.Text);
        }

        public override int GetHashCode()
        {
            if (hasValue)
                return this.value.GetHashCode();
            else
                return this.onGet.GetHashCode();
        }

        public static int IndexOf(ComboBox combo, ComboItem<T> item)
        {
            for (int i = 0, l = combo.Items.Count; i < l; i++)
            {
                if (combo.Items.Equals(item))
                    return i;
            }
            return -1;
        }

        public static int IndexOf(ComboBox combo, T value)
        {
            for (int i = 0, l = combo.Items.Count; i < l; i++)
            {
                var item = combo.Items[i];
                if (item is ComboItem<T> && ((ComboItem<T>)item).Value.Equals(value))
                    return i;
            }
            return -1;
        }

        public static int Select(ComboBox combo, T value)
        {
            return combo.SelectedIndex = IndexOf(combo, value);
        }

        public static T SelectedValue(ComboBox combo, T defaultValue)
        {
            if (combo.SelectedIndex >= 0)
                return ((ComboItem<T>)combo.SelectedItem).Value;
            return defaultValue;
        }

        public static T SelectedValue(ComboBox combo)
        {
            if (combo.SelectedIndex >= 0)
                return ((ComboItem<T>)combo.SelectedItem).Value;
            return default(T);
        }
    }
}
