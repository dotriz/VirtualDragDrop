
using System;
using System.IO;

namespace Advent.Common.IO
{
    public static class FileUtil
    {
        public static byte[] ReadAllBytes(string path, FileShare fileShare)
        {
            byte[] buffer;
            using (FileStream fileStream = new FileStream(path, FileMode.Open, FileAccess.Read, fileShare))
            {
                int offset = 0;
                long length = fileStream.Length;
                if (length > (long)int.MaxValue)
                    throw new IOException("File too long");
                int count = (int)length;
                buffer = new byte[count];
                while (count > 0)
                {
                    int num = fileStream.Read(buffer, offset, count);
                    if (num == 0)
                        throw new InvalidOperationException("End of file reached before expected");
                    offset += num;
                    count -= num;
                }
            }
            return buffer;
        }

        public static void CopyTo(this DirectoryInfo source, string destDirectory, bool recursive)
        {
            if (source == null)
                throw new ArgumentNullException("source");
            if (destDirectory == null)
                throw new ArgumentNullException("destDirectory");
            if (!source.Exists)
                throw new DirectoryNotFoundException("Source directory not found: " + source.FullName);
            DirectoryInfo directoryInfo = new DirectoryInfo(destDirectory);
            if (!directoryInfo.Exists)
                directoryInfo.Create();
            foreach (FileInfo fileInfo in source.GetFiles())
                fileInfo.CopyTo(Path.Combine(directoryInfo.FullName, fileInfo.Name), true);
            if (!recursive)
                return;
            foreach (DirectoryInfo source1 in source.GetDirectories())
                FileUtil.CopyTo(source1, Path.Combine(directoryInfo.FullName, source1.Name), recursive);
        }
    }
}
