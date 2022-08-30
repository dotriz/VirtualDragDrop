
using System;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.ComTypes;

namespace Advent.Common.Interop
{
    [Guid("DE5BF786-477A-11D2-839D-00C04FD918D0")]
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    [ComVisible(true)]
    [ComImport]
    public interface IDragSourceHelper
    {
        void InitializeFromBitmap([MarshalAs(UnmanagedType.Struct), In] ref ShDragImage dragImage, [MarshalAs(UnmanagedType.Interface), In] IDataObject dataObject);

        void InitializeFromWindow([In] IntPtr hwnd, [In] ref POINT pt, [MarshalAs(UnmanagedType.Interface), In] IDataObject dataObject);
    }
}
