
using System;
using System.Runtime.InteropServices;

namespace Advent.Common.Interop
{
    public enum SIGDN : uint
    {
        NORMALDISPLAY = 0U,
        PARENTRELATIVEPARSING = 2147581953U,
        PARENTRELATIVEFORADDRESSBAR = 2147598337U,
        DESKTOPABSOLUTEPARSING = 2147647488U,
        PARENTRELATIVEEDITING = 2147684353U,
        DESKTOPABSOLUTEEDITING = 2147794944U,
        FILESYSPATH = 2147844096U,
        URL = 2147909632U,
    }

    [Guid("43826d1e-e718-42ee-bc55-a1e261c37bfe")]
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    [ComImport]
    public interface IShellItem
    {
        void BindToHandler(IntPtr pbc, [MarshalAs(UnmanagedType.LPStruct)] Guid bhid, [MarshalAs(UnmanagedType.LPStruct)] Guid riid, out IntPtr ppv);

        void GetParent(out IShellItem ppsi);

        void GetDisplayName(SIGDN sigdnName, out IntPtr ppszName);

        void GetAttributes(uint sfgaoMask, out uint psfgaoAttribs);

        void Compare(IShellItem psi, uint hint, out int piOrder);
    }
}
