
using Advent.Common.IO;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.InteropServices;

namespace Advent.Common.Interop
{
    public class ResourceCollection : IEnumerable<IResource>, IEnumerable
    {
        private readonly UnmanagedLibrary library;
        private readonly object resType;

        internal ResourceCollection(UnmanagedLibrary lib, object resType)
        {
            this.library = lib;
            this.resType = resType;
        }

        public IEnumerator<IResource> GetEnumerator()
        {
            this.library.CheckIsReadMode();
            ResourceCollection.EnumResourcesHelper helper = new ResourceCollection.EnumResourcesHelper();
            string strType = this.resType as string;
            bool success = strType == null ? Advent.Common.Interop.NativeMethods.EnumResourceNames(this.library.Handle, (int)this.resType, new Advent.Common.Interop.NativeMethods.EnumResNameDelegate(helper.EnumResourceCallback), IntPtr.Zero) : Advent.Common.Interop.NativeMethods.EnumResourceNames(this.library.Handle, strType, new Advent.Common.Interop.NativeMethods.EnumResNameDelegate(helper.EnumResourceCallback), IntPtr.Zero);
            if (!success && Marshal.GetLastWin32Error() != 0)
                throw new Win32Exception();
            foreach (object obj in helper.ResourceNames)
            {
                if (obj is string)
                    yield return this.library.GetResource((string)obj, this.resType);
                else
                    yield return this.library.GetResource((int)obj, this.resType);
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return (IEnumerator)this.GetEnumerator();
        }

        private class EnumResourcesHelper
        {
            private readonly List<object> resourceNames = new List<object>();

            internal List<object> ResourceNames
            {
                get
                {
                    return this.resourceNames;
                }
            }

            internal bool EnumResourceCallback(IntPtr module, IntPtr lpszType, IntPtr lpszName, IntPtr param)
            {
                int id;
                string name;
                if (Advent.Common.Interop.NativeMethods.GET_RESOURCE_NAME(lpszName, out id, out name))
                    this.resourceNames.Add((object)name);
                else
                    this.resourceNames.Add((object)id);
                return true;
            }
        }
    }
}
