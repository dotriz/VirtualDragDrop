
using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.ComTypes;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Advent.Common.Interop
{
    public static class Shell
    {
        public static ImageSource GenerateThumbnail(string filename)
        {
            IntPtr phbm = IntPtr.Zero;
            Guid riid = new Guid("43826d1e-e718-42ee-bc55-a1e261c37bfe");
            IBindCtx pbc = (IBindCtx)null;
            
            // This is causing some sort of memory corruption...
            //    pbc = (IBindCtx)new Shell.CreateBindContext();
            if (!File.Exists(filename))
                return (ImageSource)null;

            
            
            IShellItem ppv;
            Advent.Common.Interop.NativeMethods.SHCreateItemFromParsingName(filename, pbc, riid, out ppv);
            if (ppv == null)
                return (ImageSource)null;
            try
            {
                ((IShellItemImageFactory)ppv).GetImage(new SIZE(256, 256), SIIGBF.SIIGBF_RESIZETOFIT, out phbm);
                return (ImageSource)Imaging.CreateBitmapSourceFromHBitmap(phbm, IntPtr.Zero, Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions());
            }
            finally
            {
                Marshal.ReleaseComObject((object)ppv);
                if (phbm != IntPtr.Zero)
                    Marshal.Release(phbm);
            }
        }

        private class CreateBindContext : IBindCtx
        {
            public void EnumObjectParam(out IEnumString ppenum)
            {
                throw new NotImplementedException();
            }

            public void GetBindOptions(ref System.Runtime.InteropServices.ComTypes.BIND_OPTS pbindopts)
            {
                pbindopts.grfMode |= 4096;
            }

            public void GetObjectParam(string pszKey, out object ppunk)
            {
                throw new NotImplementedException();
            }

            public void GetRunningObjectTable(out IRunningObjectTable pprot)
            {
                throw new NotImplementedException();
            }

            public void RegisterObjectBound(object punk)
            {
                throw new NotImplementedException();
            }

            public void RegisterObjectParam(string pszKey, object punk)
            {
            }

            public void ReleaseBoundObjects()
            {
                throw new NotImplementedException();
            }

            public void RevokeObjectBound(object punk)
            {
                throw new NotImplementedException();
            }

            public int RevokeObjectParam(string pszKey)
            {
                throw new NotImplementedException();
            }

            public void SetBindOptions(ref System.Runtime.InteropServices.ComTypes.BIND_OPTS pbindopts)
            {
                throw new NotImplementedException();
            }
        }
    }
}
