
using System;
using System.Runtime.InteropServices;

namespace Advent.Common.Interop
{
    public struct SIZE
    {
        public int cx;
        public int cy;

        public SIZE(int cx, int cy)
        {
            this.cx = cx;
            this.cy = cy;
        }
    }

    [Flags]
    public enum SIIGBF
    {
        SIIGBF_RESIZETOFIT = 0,
        SIIGBF_BIGGERSIZEOK = 1,
        SIIGBF_MEMORYONLY = 2,
        SIIGBF_ICONONLY = 4,
        SIIGBF_THUMBNAILONLY = 8,
        SIIGBF_INCACHEONLY = 16,
    }

    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    [Guid("bcc18b79-ba16-442f-80c4-8a59c30c463b")]
    [ComImport]
    public interface IShellItemImageFactory
    {
        void GetImage([MarshalAs(UnmanagedType.Struct), In] SIZE size, [In] SIIGBF flags, out IntPtr phbm);
    }
}
