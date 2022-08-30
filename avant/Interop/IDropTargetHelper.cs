
using System;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.ComTypes;

namespace Advent.Common.Interop
{
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    [Guid("4657278B-411B-11D2-839A-00C04FD918D0")]
    [ComVisible(true)]
    [ComImport]
    public interface IDropTargetHelper
    {
        void DragEnter([In] IntPtr hwndTarget, [MarshalAs(UnmanagedType.Interface), In] IDataObject dataObject, [In] ref POINT pt, [In] int effect);

        void DragLeave();

        void DragOver([In] ref POINT pt, [In] int effect);

        void Drop([MarshalAs(UnmanagedType.Interface), In] IDataObject dataObject, [In] ref POINT pt, [In] int effect);

        void Show([In] bool show);
    }
}
