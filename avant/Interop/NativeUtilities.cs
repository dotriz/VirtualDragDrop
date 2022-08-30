
using System;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.ComTypes;
using System.Windows;

namespace Advent.Common.Interop
{
    public static class NativeUtilities
    {
        public static void GetWords(this long l, out int hiWord, out int loWord)
        {
            loWord = (int)(l & (long)uint.MaxValue);
            hiWord = (int)(l >> 32);
        }

        public static long MakeLong(int hiWord, int loWord)
        {
            return ((long)hiWord << 32) + (long)loWord;
        }

        public static DateTime ToDateTime(this System.Runtime.InteropServices.ComTypes.FILETIME ft)
        {
            IntPtr num = IntPtr.Zero;
            try
            {
                long[] destination = new long[1];
                num = Marshal.AllocHGlobal(Marshal.SizeOf((object)ft));
                Marshal.StructureToPtr((object)ft, num, false);
                Marshal.Copy(num, destination, 0, 1);
                return DateTime.FromFileTime(destination[0]);
            }
            finally
            {
                if (num != IntPtr.Zero)
                    Marshal.FreeHGlobal(num);
            }
        }

        public static POINT ToWin32Point(this Point pt)
        {
            return new POINT()
            {
                X = (int)pt.X,
                Y = (int)pt.Y
            };
        }
    }
}
