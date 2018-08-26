using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices;

namespace Gw2Launcher.Windows
{
    static class Volume
    {
        [ComImport]
        [Guid("BCDE0395-E52F-467C-8E3D-C4579291692E")]
        internal class MMDeviceEnumerator
        {
        }

        [Guid("77AA99A0-1BD6-484F-8BC7-2C654C9A9B6F"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
        internal interface IAudioSessionManager2
        {
            int NotImpl1();
            int NotImpl2();

            [PreserveSig]
            int GetSessionEnumerator(out IAudioSessionEnumerator SessionEnum);
        }

        [Guid("E2F5BB11-0570-40CA-ACDD-3AA01277DEE8"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
        internal interface IAudioSessionEnumerator
        {
            [PreserveSig]
            int GetCount(out int SessionCount);

            [PreserveSig]
            int GetSession(int SessionCount, out IAudioSessionControl2 Session);
        }

        [Guid("bfb7ff88-7239-4fc9-8fa2-07c950be9c6d"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
        internal interface IAudioSessionControl2
        {
            // IAudioSessionControl
            [PreserveSig]
            int NotImpl0();

            [PreserveSig]
            int GetDisplayName([MarshalAs(UnmanagedType.LPWStr)] out string pRetVal);

            [PreserveSig]
            int SetDisplayName([MarshalAs(UnmanagedType.LPWStr)]string Value, [MarshalAs(UnmanagedType.LPStruct)] Guid EventContext);

            [PreserveSig]
            int GetIconPath([MarshalAs(UnmanagedType.LPWStr)] out string pRetVal);

            [PreserveSig]
            int SetIconPath([MarshalAs(UnmanagedType.LPWStr)] string Value, [MarshalAs(UnmanagedType.LPStruct)] Guid EventContext);

            [PreserveSig]
            int GetGroupingParam(out Guid pRetVal);

            [PreserveSig]
            int SetGroupingParam([MarshalAs(UnmanagedType.LPStruct)] Guid Override, [MarshalAs(UnmanagedType.LPStruct)] Guid EventContext);

            [PreserveSig]
            int NotImpl1();

            [PreserveSig]
            int NotImpl2();

            // IAudioSessionControl2
            [PreserveSig]
            int GetSessionIdentifier([MarshalAs(UnmanagedType.LPWStr)] out string pRetVal);

            [PreserveSig]
            int GetSessionInstanceIdentifier([MarshalAs(UnmanagedType.LPWStr)] out string pRetVal);

            [PreserveSig]
            int GetProcessId(out int pRetVal);

            [PreserveSig]
            int IsSystemSoundsSession();

            [PreserveSig]
            int SetDuckingPreference(bool optOut);
        }

        [Guid("87CE5498-68D6-44E5-9215-6DA47EF883D8"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
        internal interface ISimpleAudioVolume
        {
            [PreserveSig]
            int SetMasterVolume(float fLevel, ref Guid EventContext);

            [PreserveSig]
            int GetMasterVolume(out float pfLevel);

            [PreserveSig]
            int SetMute(bool bMute, ref Guid EventContext);

            [PreserveSig]
            int GetMute(out bool pbMute);
        }

        [Guid("A95664D2-9614-4F35-A746-DE8DB63617E6"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
        internal interface IMMDeviceEnumerator
        {
            int NotImpl1();

            [PreserveSig]
            int GetDefaultAudioEndpoint(EDataFlow dataFlow, ERole role, out IMMDevice ppDevice);
        }

        [Guid("D666063F-1587-4E43-81F1-B948E807363F"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
        internal interface IMMDevice
        {
            [PreserveSig]
            int Activate(ref Guid iid, int dwClsCtx, IntPtr pActivationParams, [MarshalAs(UnmanagedType.IUnknown)] out object ppInterface);
        }

        internal enum EDataFlow
        {
            eRender,
            eCapture,
            eAll,
            EDataFlow_enum_count
        }

        internal enum ERole
        {
            eConsole,
            eMultimedia,
            eCommunications,
            ERole_enum_count
        }

        public class VolumeControl : IDisposable
        {
            private int processId;
            private IMMDeviceEnumerator deviceEnumerator;
            private IMMDevice speakers;
            private IAudioSessionManager2 mgr;
            private ISimpleAudioVolume volume;
            private bool isSupported;

            public VolumeControl(int processId)
            {
                this.processId = processId;

                try
                {
                    Initialize();
                    isSupported = true;
                }
                catch
                {
                    isSupported = false;
                }
            }

            private void Initialize()
            {
                if (deviceEnumerator == null)
                {
                    deviceEnumerator = (IMMDeviceEnumerator)(new MMDeviceEnumerator());
                    deviceEnumerator.GetDefaultAudioEndpoint(EDataFlow.eRender, ERole.eMultimedia, out speakers);
                    Guid IID_IAudioSessionManager2 = typeof(IAudioSessionManager2).GUID;
                    object o;
                    speakers.Activate(ref IID_IAudioSessionManager2, 0, IntPtr.Zero, out o);
                    mgr = (IAudioSessionManager2)o;
                }
            }

            /// <summary>
            /// Queries the volume controller for the process
            /// </summary>
            /// <returns>True if found</returns>
            public bool Query()
            {
                if (!isSupported)
                    return false;

                IAudioSessionEnumerator sessionEnumerator = null;

                try
                {
                    mgr.GetSessionEnumerator(out sessionEnumerator);
                    int count;
                    sessionEnumerator.GetCount(out count);

                    for (int i = 0; i < count; i++)
                    {
                        IAudioSessionControl2 ctl;
                        sessionEnumerator.GetSession(i, out ctl);
                        int cpid;
                        ctl.GetProcessId(out cpid);

                        if (cpid == processId)
                        {
                            volume = ctl as ISimpleAudioVolume;
                            return true;
                        }

                        Marshal.ReleaseComObject(ctl);
                    }
                }
                finally
                {
                    if (sessionEnumerator != null)
                        Marshal.ReleaseComObject(sessionEnumerator);
                }

                return false;
            }

            public float Volume
            {
                get
                {
                    if (volume != null)
                    {
                        float percent;
                        volume.GetMasterVolume(out percent);
                        return percent;
                    }

                    return 0;
                }
                set
                {
                    if (volume != null)
                    {
                        Guid guid = Guid.Empty;
                        volume.SetMasterVolume(value, ref guid);
                    }
                }
            }

            public bool IsSupported
            {
                get
                {
                    return isSupported;
                }
            }

            public void Dispose()
            {
                if (deviceEnumerator != null)
                {
                    deviceEnumerator = null;

                    if (mgr != null)
                        Marshal.ReleaseComObject(mgr);
                    if (speakers != null)
                        Marshal.ReleaseComObject(speakers);
                    if (deviceEnumerator != null)
                        Marshal.ReleaseComObject(deviceEnumerator);
                    if (volume != null)
                        Marshal.ReleaseComObject(volume);
                }
            }
        }

        public static bool GetVolume(int processId, out float percent)
        {
            ISimpleAudioVolume volume;
            try
            {
                volume = GetVolumeObject(processId);
                if (volume == null)
                {
                    percent = 0;
                    return false;
                }
            }
            catch (Exception e)
            {
                Util.Logging.Log(e);
                percent = 0;
                return false;
            }

            try
            {
                volume.GetMasterVolume(out percent);
            }
            catch (Exception e)
            {
                percent = 0;
                Util.Logging.Log(e);
            }
            finally
            {
                Marshal.ReleaseComObject(volume);
            }

            return true;
        }

        public static bool SetVolume(int processId, float percent)
        {
            ISimpleAudioVolume volume;
            try
            {
                volume = GetVolumeObject(processId);
                if (volume == null)
                    return false;
            }
            catch (Exception e)
            {
                Util.Logging.Log(e);
                return false;
            }

            try
            {
                Guid guid = Guid.Empty;
                volume.SetMasterVolume(percent, ref guid);
            }
            catch (Exception e)
            {
                Util.Logging.Log(e);
            }
            finally
            {
                Marshal.ReleaseComObject(volume);
            }

            return true;
        }

        private static ISimpleAudioVolume GetVolumeObject(int pid)
        {
            IMMDeviceEnumerator deviceEnumerator = (IMMDeviceEnumerator)(new MMDeviceEnumerator());
            IMMDevice speakers = null;
            IAudioSessionEnumerator sessionEnumerator = null;
            IAudioSessionManager2 mgr = null;

            try
            {
                deviceEnumerator.GetDefaultAudioEndpoint(EDataFlow.eRender, ERole.eMultimedia, out speakers);

                Guid IID_IAudioSessionManager2 = typeof(IAudioSessionManager2).GUID;
                object o;
                speakers.Activate(ref IID_IAudioSessionManager2, 0, IntPtr.Zero, out o);
                mgr = (IAudioSessionManager2)o;

                mgr.GetSessionEnumerator(out sessionEnumerator);
                int count;
                sessionEnumerator.GetCount(out count);

                ISimpleAudioVolume volumeControl = null;
                for (int i = 0; i < count; i++)
                {
                    IAudioSessionControl2 ctl;
                    sessionEnumerator.GetSession(i, out ctl);
                    int cpid;
                    ctl.GetProcessId(out cpid);

                    if (cpid == pid)
                    {
                        volumeControl = ctl as ISimpleAudioVolume;
                        break;
                    }
                    Marshal.ReleaseComObject(ctl);
                }

                return volumeControl;
            }
            finally
            {
                Marshal.ReleaseComObject(sessionEnumerator);
                Marshal.ReleaseComObject(mgr);
                Marshal.ReleaseComObject(speakers);
                Marshal.ReleaseComObject(deviceEnumerator);
            }
        }
    }
}
