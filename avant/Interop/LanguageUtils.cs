
using System.Runtime.InteropServices;

namespace Advent.Common.Interop
{
    public static class LanguageUtils
    {
        [DllImport("kernel32.dll")]
        public static extern ushort GetUserDefaultUILanguage();

        [DllImport("kernel32.dll")]
        public static extern ushort GetSystemDefaultUILanguage();

        [DllImport("kernel32.dll")]
        public static extern ushort GetThreadUILanguage();
    }
}
