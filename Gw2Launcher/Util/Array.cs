using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gw2Launcher.Util
{
    static class Array
    {
        public static bool Equals<T>(T[] a, T[] b)
        {
            if (a == b)
            {
                return true;
            }

            if (a != null && b != null && a.Length == b.Length)
            {
                for (var i = a.Length - 1; i >= 0; --i)
                {
                    if (!a[i].Equals(b[i]))
                    {
                        return false;
                    }
                }

                return true;
            }

            return false;
        }

        public static int IndexOf<T>(T[] array, T[] value)
        {
            return IndexOf<T>(array, value, 0, array.Length);
        }

        /// <summary>
        /// Returns the index of the value within the array, or -1 if not found
        /// </summary>
        /// <param name="array">Array to search</param>
        /// <param name="value">Value to find</param>
        /// <param name="offset">Offset to start searching within the array</param>
        /// <param name="count">Number of spaces to search</param>
        public static int IndexOf<T>(T[] array, T[] value, int offset, int count)
        {
            var limit = offset + count - value.Length;

            while (offset <= limit)
            {
                if (array[offset].Equals(value[0]))
                {
                    int i;

                    for (i = 1; i < value.Length; i++)
                    {
                        if (!array[offset + i].Equals(value[i]))
                        {
                            break;
                        }
                    }

                    if (i == value.Length)
                    {
                        return offset;
                    }
                }

                ++offset;
            }

            return -1;
        }
    }
}
