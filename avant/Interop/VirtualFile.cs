
using System;
using System.IO;
using System.Drawing;
using System.Runtime.InteropServices;

namespace Advent.Common.Interop
{
    public enum FileDescriptorFlags : uint
    {
        FD_CLSID = 1U,
        FD_SIZEPOINT = 2U,
        FD_ATTRIBUTES = 4U,
        FD_CREATETIME = 8U,
        FD_ACCESSTIME = 16U,
        FD_WRITESTIME = 32U,
        FD_FILESIZE = 64U,
        FD_PROGRESSUI = 16384U,
        FD_LINKUI = 32768U,
        FD_UNICODE = 2147483648U,
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
    public struct FILEDESCRIPTOR
    {
        public FileDescriptorFlags dwFlags;
        public Guid clsid;
        public Size sizel;
        public Point pointl;
        public uint dwFileAttributes;
        public System.Runtime.InteropServices.ComTypes.FILETIME ftCreationTime;
        public System.Runtime.InteropServices.ComTypes.FILETIME ftLastAccessTime;
        public System.Runtime.InteropServices.ComTypes.FILETIME ftLastWriteTime;
        public int nFileSizeHigh;
        public int nFileSizeLow;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 260)]
        public string cFileName;
    }

    public class VirtualFile
    {
        public byte[] Contents { get; set; }

        public Func<byte[]> ContentsFunc { get; set; }

        public string Name { get; set; }

        public DateTime CreationTime { get; set; }

        public DateTime LastAccessTime { get; set; }

        public DateTime LastWriteTime { get; set; }

        public FileAttributes Attributes { get; set; }

        public VirtualFile()
        {
            this.Attributes = FileAttributes.Normal;
        }

        public VirtualFile(string name, byte[] contents)
            : this()
        {
            this.Name = name;
            this.Contents = contents;
        }

        internal static VirtualFile FromFileDescriptor(FILEDESCRIPTOR fd)
        {
            VirtualFile virtualFile = new VirtualFile();
            virtualFile.Name = fd.cFileName;
            if ((fd.dwFlags & FileDescriptorFlags.FD_ATTRIBUTES) == FileDescriptorFlags.FD_ATTRIBUTES)
                virtualFile.Attributes = (FileAttributes)fd.dwFileAttributes;
            if ((fd.dwFlags & FileDescriptorFlags.FD_CREATETIME) == FileDescriptorFlags.FD_CREATETIME)
                virtualFile.CreationTime = NativeUtilities.ToDateTime(fd.ftCreationTime);
            if ((fd.dwFlags & FileDescriptorFlags.FD_ACCESSTIME) == FileDescriptorFlags.FD_ACCESSTIME)
                virtualFile.LastAccessTime = NativeUtilities.ToDateTime(fd.ftLastAccessTime);
            if ((fd.dwFlags & FileDescriptorFlags.FD_WRITESTIME) == FileDescriptorFlags.FD_WRITESTIME)
                virtualFile.LastWriteTime = NativeUtilities.ToDateTime(fd.ftLastWriteTime);
            return virtualFile;
        }

        internal FILEDESCRIPTOR ToFileDescriptor()
        {
            FILEDESCRIPTOR filedescriptor = new FILEDESCRIPTOR();
            filedescriptor.dwFlags = FileDescriptorFlags.FD_FILESIZE;
            filedescriptor.cFileName = this.Name;
            if (this.Contents != null)
                NativeUtilities.GetWords(this.Contents.LongLength, out filedescriptor.nFileSizeHigh, out filedescriptor.nFileSizeLow);
            if (this.Attributes != FileAttributes.Normal)
            {
                filedescriptor.dwFileAttributes = (uint)this.Attributes;
                filedescriptor.dwFlags |= FileDescriptorFlags.FD_ATTRIBUTES;
            }
            if (this.CreationTime != DateTime.MinValue)
            {
                NativeUtilities.GetWords(this.CreationTime.ToFileTime(), out filedescriptor.ftCreationTime.dwHighDateTime, out filedescriptor.ftCreationTime.dwLowDateTime);
                filedescriptor.dwFlags |= FileDescriptorFlags.FD_CREATETIME;
            }
            if (this.LastAccessTime != DateTime.MinValue)
            {
                NativeUtilities.GetWords(this.LastAccessTime.ToFileTime(), out filedescriptor.ftLastAccessTime.dwHighDateTime, out filedescriptor.ftLastAccessTime.dwLowDateTime);
                filedescriptor.dwFlags |= FileDescriptorFlags.FD_ACCESSTIME;
            }
            if (this.LastWriteTime != DateTime.MinValue)
            {
                NativeUtilities.GetWords(this.LastWriteTime.ToFileTime(), out filedescriptor.ftLastWriteTime.dwHighDateTime, out filedescriptor.ftLastWriteTime.dwLowDateTime);
                filedescriptor.dwFlags |= FileDescriptorFlags.FD_WRITESTIME;
            }
            return filedescriptor;
        }
    }
}
