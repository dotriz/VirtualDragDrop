
using System;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.ComTypes;

namespace Advent.Common.Interop
{
    public struct Win32Size
    {
        public int cx;
        public int cy;
    }

    public struct ShDragImage
    {
        public Win32Size sizeDragImage;
        public POINT ptOffset;
        public IntPtr hbmpDragImage;
        public int crColorKey;
    }

    [ComVisible(true)]
    [Guid("83E07D0D-0C5F-4163-BF1A-60B274051E40")]
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    [ComImport]
    public interface IDragSourceHelper2
    {
        void InitializeFromBitmap([MarshalAs(UnmanagedType.Struct), In] ref ShDragImage dragImage, [MarshalAs(UnmanagedType.Interface), In] IDataObject dataObject);

        void InitializeFromWindow([In] IntPtr hwnd, [In] ref POINT pt, [MarshalAs(UnmanagedType.Interface), In] IDataObject dataObject);

        void SetFlags([In] int dwFlags);
    }
}
