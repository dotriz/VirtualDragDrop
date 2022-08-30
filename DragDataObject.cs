using System;
using System.Runtime.InteropServices;

namespace DragVirtual
{
    public static class DragDataObject
    {
        public static void AddPreviewImage(System.Runtime.InteropServices.ComTypes.IDataObject dataObject, string imgPath)
        {
            if (dataObject == null)
                throw new ArgumentNullException(nameof(dataObject));

            var ddh = (IDragSourceHelper)new DragDataObject.DragDropHelper();
            var dragImage = new SHDRAGIMAGE();

            System.Drawing.Image image = imgPath.StartsWith("pack://") ? LoadResourceImage(imgPath) : System.Drawing.Image.FromFile(imgPath);
            double factor = (double)image.Width / (double)image.Height;
            double width = 150;
            double height = width / factor;
            System.Drawing.Image thumb = image.GetThumbnailImage(Convert.ToInt32(width), Convert.ToInt32(height), () => false, IntPtr.Zero);

            // note you should use a thumbnail here, not a full-sized image
            var thumbnail = new System.Drawing.Bitmap(thumb);
            dragImage.sizeDragImage.cx = thumbnail.Width;
            dragImage.sizeDragImage.cy = thumbnail.Height;
            dragImage.hbmpDragImage = thumbnail.GetHbitmap();
            Marshal.ThrowExceptionForHR(ddh.InitializeFromBitmap(ref dragImage, dataObject));
        }

        private static System.Drawing.Image LoadResourceImage(string path)
        {
            try
            {
                System.Windows.Resources.StreamResourceInfo sri = System.Windows.Application.GetResourceStream(new Uri(path));
                return System.Drawing.Image.FromStream(sri.Stream);
            }
            catch (Exception)
            {
                return null;
            }
        }

        public static System.Runtime.InteropServices.ComTypes.IDataObject GetFileDataObject(string filePath)
        {
            if (filePath == null)
                throw new ArgumentNullException(nameof(filePath));

            Marshal.ThrowExceptionForHR(SHCreateItemFromParsingName(filePath, null, typeof(IShellItem).GUID, out var item));
            Marshal.ThrowExceptionForHR(item.BindToHandler(null, BHID_DataObject, typeof(System.Runtime.InteropServices.ComTypes.IDataObject).GUID, out var dataObject));
            return (System.Runtime.InteropServices.ComTypes.IDataObject)dataObject;
        }

        private static readonly Guid BHID_DataObject = new Guid("b8c0bd9f-ed24-455c-83e6-d5390c4fe8c4");

        [DllImport("shell32", CharSet = CharSet.Unicode)]
        private static extern int SHCreateItemFromParsingName(string path, System.Runtime.InteropServices.ComTypes.IBindCtx pbc, [MarshalAs(UnmanagedType.LPStruct)] Guid riid, out IShellItem ppv);

        [Guid("43826d1e-e718-42ee-bc55-a1e261c37bfe"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
        private interface IShellItem
        {
            [PreserveSig]
            int BindToHandler(System.Runtime.InteropServices.ComTypes.IBindCtx pbc, [MarshalAs(UnmanagedType.LPStruct)] Guid bhid, [MarshalAs(UnmanagedType.LPStruct)] Guid riid, [MarshalAs(UnmanagedType.IUnknown)] out object ppv);

            // other methods are not defined, we don't need them
        }

        [ComImport, Guid("4657278a-411b-11d2-839a-00c04fd918d0")] // CLSID_DragDropHelper
        private class DragDropHelper
        {
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct POINT
        {
            public int x;
            public int y;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct SIZE
        {
            public int cx;
            public int cy;
        }

        // https://docs.microsoft.com/en-us/windows/win32/api/shobjidl_core/ns-shobjidl_core-shdragimage
        [StructLayout(LayoutKind.Sequential)]
        private struct SHDRAGIMAGE
        {
            public SIZE sizeDragImage;
            public POINT ptOffset;
            public IntPtr hbmpDragImage;
            public int crColorKey;
        }

        // https://docs.microsoft.com/en-us/windows/win32/api/shobjidl_core/nn-shobjidl_core-idragsourcehelper
        [InterfaceType(ComInterfaceType.InterfaceIsIUnknown), Guid("DE5BF786-477A-11D2-839D-00C04FD918D0")]
        private interface IDragSourceHelper
        {
            [PreserveSig]
            int InitializeFromBitmap(ref SHDRAGIMAGE pshdi, System.Runtime.InteropServices.ComTypes.IDataObject pDataObject);

            [PreserveSig]
            int InitializeFromWindow(IntPtr hwnd, ref POINT ppt, System.Runtime.InteropServices.ComTypes.IDataObject pDataObject);
        }
    }
}
