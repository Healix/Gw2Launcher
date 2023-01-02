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

        private static int IndexOf<T>(T[] array, T[] value, int startIndex, int count)
        {
            var limit = count - startIndex - value.Length;

            while (startIndex <= limit)
            {
                if (array[startIndex].Equals(value[0]))
                {
                    int i;

                    for (i = 1; i < value.Length; i++)
                    {
                        if (!array[startIndex + i].Equals(value[i]))
                        {
                            break;
                        }
                    }

                    if (i == value.Length)
                    {
                        return startIndex;
                    }
                }

                ++startIndex;
            }

            return -1;
        }
    }
}
