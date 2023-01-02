using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices;

namespace Gw2Launcher.Tools.Mumble
{
    public static class MumbleData
    {
        public const float METER_TO_INCH = 39.3701f;

        [StructLayout(LayoutKind.Explicit)]
        public struct IdentificationData
        {
            [FieldOffset(4)]
            public uint uiTick;
            [FieldOffset(1188)]
            public uint processId;
        }

        [StructLayout(LayoutKind.Explicit)]
        public struct PositionData
        {
            [FieldOffset(8)]
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 3)]
            public float[] fAvatarPosition;
            [FieldOffset(1136)]
            public uint mapId;
        }

        /// <summary>
        /// Converts Mumble coordinates in meters to in-game coordinates in inches. Mumble coordinates are in the order x,-z,y (note z is inversed; negative z is up into the sky in-game).
        /// </summary>
        public static void ConvertCoordinates(float[] fAvatarPosition, out float x, out float y, out float z)
        {
            x = fAvatarPosition[0] * METER_TO_INCH;
            z = -fAvatarPosition[1] * METER_TO_INCH;
            y = fAvatarPosition[2] * METER_TO_INCH;
        }

        /// <summary>
        /// Converts in-game coordinates in inches to Mumble coordinates in meters. Mumble coordinates are in the order x,-z,y (note z is inversed; negative z is up into the sky in-game).
        /// </summary>
        /// <returns>x, -z, y</returns>
        public static float[] ConvertCoordinates(float x, float y, float z)
        {
            return new float[]
            {
                x / METER_TO_INCH,
                -z / METER_TO_INCH,
                y / METER_TO_INCH,
            };
        }
    }
}
