
using Advent.Common.Interop;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Media;

namespace Advent.Common.UI
{
    public static class FontUtil
    {
        private const string FontSourceFilePrefix = "file:///";

        public static string FontsPath
        {
            get
            {
                StringBuilder lpszPath = new StringBuilder(260);
                NativeMethods.SHGetFolderPath(IntPtr.Zero, 20, IntPtr.Zero, 0, lpszPath);
                return ((object)lpszPath).ToString();
            }
        }

        public static int InstallFont(string file)
        {
            string str = Path.Combine(FontUtil.FontsPath, Path.GetFileName(file));
            if (File.Exists(str))
                return 0;
            File.Copy(file, str);
            int num = NativeMethods.AddFontResource(str);
            NativeMethods.SendMessage((int)ushort.MaxValue, 29U, 0, 0);
            NativeMethods.WriteProfileString("fonts", Path.GetFileName(file) + " (TrueType)", str);
            return num;
        }

        public static int RemoveFont(string file)
        {
            string str = Path.Combine(FontUtil.FontsPath, Path.GetFileName(file));
            if (!File.Exists(str))
                return 0;
            int num = NativeMethods.RemoveFontResource(str);
            return num;
        }

        public static string GetName(this FontFamily fontFamily)
        {
            return fontFamily.Source.Substring(fontFamily.Source.LastIndexOf('#') + 1);
        }

        public static string GetFile(this FontFamily fontFamily)
        {
            return FontUtil.FindFontFile(fontFamily);
        }

        public static string FindFontFile(FontFamily fontFamily)
        {
            return !fontFamily.Source.StartsWith("file:///") ? Enumerable.FirstOrDefault<string>((IEnumerable<string>)Directory.GetFiles(FontUtil.FontsPath), (Func<string, bool>)(file => Enumerable.FirstOrDefault<FontFamily>((IEnumerable<FontFamily>)Fonts.GetFontFamilies(file), (Func<FontFamily, bool>)(o => FontUtil.GetName(o) == FontUtil.GetName(fontFamily))) != null)) : fontFamily.Source.Substring("file:///".Length, fontFamily.Source.LastIndexOf('#') - "file:///".Length).Replace('/', '\\');
        }
    }
}
