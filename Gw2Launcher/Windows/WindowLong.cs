using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Gw2Launcher.Windows.Native;

namespace Gw2Launcher.Windows
{
    static class WindowLong
    {
        public static IntPtr Add(IntPtr handle, int index, WindowStyle value)
        {
            return Add(handle, index, (uint)value);
        }

        public static IntPtr Add(IntPtr handle, GWL index, WindowStyle value)
        {
            return Add(handle, (int)index, (uint)value);
        }

        public static IntPtr Add(IntPtr handle, int index, uint value)
        {
            var l1 = NativeMethods.GetWindowLongPtr(handle, index);
            IntPtr l2;

            if (IntPtr.Size == 4)
                l2 = (IntPtr)(int)((uint)l1 | value);
            else
                l2 = (IntPtr)(long)((ulong)l1 | value);

            if (l2 != l1)
                return NativeMethods.SetWindowLongPtr(handle, index, l2);

            return IntPtr.Zero;
        }

        public static IntPtr Remove(IntPtr handle, int index, WindowStyle value)
        {
            return Remove(handle, index, (uint)value);
        }

        public static IntPtr Remove(IntPtr handle, GWL index, WindowStyle value)
        {
            return Remove(handle, (int)index, (uint)value);
        }

        public static IntPtr Remove(IntPtr handle, int index, uint value)
        {
            var l1 = NativeMethods.GetWindowLongPtr(handle, index);
            IntPtr l2;

            if (IntPtr.Size == 4)
                l2 = (IntPtr)(int)((uint)l1 & ~value);
            else
                l2 = (IntPtr)(long)((ulong)l1 & ~value);

            if (l2 != l1)
                return NativeMethods.SetWindowLongPtr(handle, index, l2);

            return IntPtr.Zero;
        }

        public static bool HasValue(IntPtr handle, GWL index, WindowStyle value)
        {
            return HasValue(handle, (int)index, (uint)value);
        }

        public static bool HasValue(IntPtr handle, int index, WindowStyle value)
        {
            return HasValue(handle, index, (uint)value);
        }

        public static bool HasValue(IntPtr handle, int index, uint value)
        {
            var l1 = NativeMethods.GetWindowLongPtr(handle, index);

            if (IntPtr.Size == 4)
                return ((uint)l1 & value) == value;
            else
                return ((ulong)l1 & value) == value;
        }
    }
}
