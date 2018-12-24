using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices;
using Gw2Launcher.Windows.Native;

namespace Gw2Launcher.Windows
{
    class WindowProperties
    {
        [DllImport(NativeMethods.DLL.SHELL32)]
        private static extern int SHGetPropertyStoreForWindow(
            IntPtr hwnd,
            ref Guid iid,
            [Out(), MarshalAs(UnmanagedType.Interface)]out IPropertyStore propertyStore);

        [ComImport, Guid("886D8EEB-8CF2-4446-8D02-CDBA1DBDCF99"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
        public interface IPropertyStore
        {
            void GetCount(out uint cProps);
            void GetAt([In] uint iProp, out PropertyKey pKey);
            void GetValue([In] ref PropertyKey key, [Out] PropVariant pv);
            void SetValue([In] ref PropertyKey key, [In] PropVariant propvar);
            void Commit();
        }

        [DllImport(NativeMethods.DLL.OLE32, PreserveSig = false)] // returns hresult
        private extern static void PropVariantClear([In, Out] PropVariant pvar);

        public enum StartPinOptions
        {
            Default = 0,
            NoPinOnInstall = 1,
            UserPinned = 2,
        }

        [StructLayout(LayoutKind.Sequential, Pack = 4)]
        public struct PropertyKey
        {
            public Guid formatId;
            public uint propertyId;

            public PropertyKey(Guid formatId, uint propertyId)
            {
                this.formatId = formatId;
                this.propertyId = propertyId;
            }

            private static readonly Guid GUID_AppUserModel = new Guid("9F4C2855-9F79-4B39-A8D0-E1D42DE1D5F3");

            public static readonly PropertyKey AppUserModel_ID = new PropertyKey(GUID_AppUserModel, 5);
            public static readonly PropertyKey AppUserModel_ExcludeFromShowInNewInstall = new PropertyKey(GUID_AppUserModel, 8);
            public static readonly PropertyKey AppUserModel_PreventPinning = new PropertyKey(GUID_AppUserModel, 9);
            public static readonly PropertyKey AppUserModel_StartPinOption = new PropertyKey(GUID_AppUserModel, 12);
            public static readonly PropertyKey AppUserModel_RelaunchCommand = new PropertyKey(GUID_AppUserModel, 2);
            public static readonly PropertyKey AppUserModel_RelaunchDisplayNameResource = new PropertyKey(GUID_AppUserModel, 4);
            public static readonly PropertyKey AppUserModel_RelaunchIconResource = new PropertyKey(GUID_AppUserModel, 3);
        }

        [StructLayout(LayoutKind.Explicit, Pack = 1)]
        public class PropVariant : IDisposable
        {
            [FieldOffset(0)] ushort varType;

            [FieldOffset(8)] IntPtr pszVal;
            [FieldOffset(8)] short iVal;
            [FieldOffset(8)] uint uintVal;
            [FieldOffset(8)] short boolVal;

            public PropVariant(string value)
            {
                varType = (ushort)VarEnum.VT_LPWSTR;
                pszVal = Marshal.StringToCoTaskMemUni(value);
            }

            public PropVariant(bool value)
            {
                varType = (ushort)VarEnum.VT_BOOL;
                boolVal = (short)(value ? -1 : 0);
            }

            public PropVariant(uint value)
            {
                varType = (ushort)VarEnum.VT_UI4;
                uintVal = value;
            }

            public void Dispose()
            {
                PropVariantClear(this);
                GC.SuppressFinalize(this);
            }

            ~PropVariant()
            {
                Dispose();
            }
        }
    }
}
