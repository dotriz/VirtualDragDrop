
using System;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.ComTypes;
using System.Text;
using System.Windows;

namespace Advent.Common.Interop
{
    public struct SECURITY_ATTRIBUTES
    {
        public int nLength;
        public IntPtr lpSecurityDescriptor;
        public int bInheritHandle;
    }

    public enum JobObjectInfoClass
    {
        JobObjectBasicAccountingInformation = 1,
        JobObjectBasicLimitInformation = 2,
        JobObjectBasicProcessIdList = 3,
        JobObjectBasicUIRestrictions = 4,
        JobObjectSecurityLimitInformation = 5,
        JobObjectEndOfJobTimeInformation = 6,
        JobObjectAssociateCompletionPortInformation = 7,
        JobObjectBasicAndIoAccountingInformation = 8,
        JobObjectExtendedLimitInformation = 9,
    }

    [Flags]
    public enum JobLimitFlags
    {
        JOB_OBJECT_LIMIT_ACTIVE_PROCESS = 8,
        JOB_OBJECT_LIMIT_AFFINITY = 16,
        JOB_OBJECT_LIMIT_BREAKAWAY_OK = 2048,
        JOB_OBJECT_LIMIT_DIE_ON_UNHANDLED_EXCEPTION = 1024,
        JOB_OBJECT_LIMIT_JOB_MEMORY = 512,
        JOB_OBJECT_LIMIT_JOB_TIME = 4,
        JOB_OBJECT_LIMIT_KILL_ON_JOB_CLOSE = 8192,
        JOB_OBJECT_LIMIT_PRESERVE_JOB_TIME = 64,
        JOB_OBJECT_LIMIT_PRIORITY_CLASS = 32,
        JOB_OBJECT_LIMIT_PROCESS_MEMORY = 256,
        JOB_OBJECT_LIMIT_PROCESS_TIME = 2,
        JOB_OBJECT_LIMIT_SCHEDULING_CLASS = 128,
        JOB_OBJECT_LIMIT_SILENT_BREAKAWAY_OK = 4096,
        JOB_OBJECT_LIMIT_WORKINGSET = 1,
    }

    public struct JOBOBJECT_BASIC_LIMIT_INFORMATION
    {
        public long PerProcessUserTimeLimit;
        public long PerJobUserTimeLimit;
        public JobLimitFlags LimitFlags;
        public uint MinimumWorkingSetSize;
        public uint MaximumWorkingSetSize;
        public int ActiveProcessLimit;
        public IntPtr Affinity;
        public int PriorityClass;
        public int SchedulingClass;
    }

    public struct JOBOBJECT_BASIC_PROCESS_ID_LIST
    {
        public int NumberOfAssignedProcesses;
        public int NumberOfProcessIdsInList;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 200)]
        public int[] ProcessIdList;
    }

    public struct MARGINS
    {
        public int Left;
        public int Right;
        public int Top;
        public int Bottom;

        public MARGINS(Thickness t)
        {
            this.Left = (int)t.Left;
            this.Right = (int)t.Right;
            this.Top = (int)t.Top;
            this.Bottom = (int)t.Bottom;
        }
    }

    [Flags]
    public enum DwmThumbnailFlags
    {
        RectDestination = 1,
        RectSource = 2,
        Opacity = 4,
        Visible = 8,
        SourceClientAreaOnly = 16,
    }

    public struct DwmThumbnailProperties
    {
        public DwmThumbnailFlags Flags;
        public RECT rcDestination;
        public RECT rcSource;
        public byte Opacity;
        public bool Visible;
        public bool SourceClientAreaOnly;
    }

    public static class NativeMethods
    {
        public const int MAX_PATH = 260;
        public const int CRYPTPROTECT_UI_FORBIDDEN = 1;
        public const int CRYPTPROTECT_LOCAL_MACHINE = 4;
        private const byte LANG_NEUTRAL = (byte)0;
        private const byte SUBLANG_NEUTRAL = (byte)0;
        private const byte SUBLANG_DEFAULT = (byte)1;
        internal const uint LOAD_LIBRARY_AS_DATAFILE = 2U;
        public const int RT_HTML = 23;
        public const int RT_RCDATA = 10;
        public const int RT_STRING = 6;
        public const int RT_GROUP_ICON = 14;
        public const int RT_VERSION = 16;
        internal const int ERROR_NO_MORE_FILES = 18;
        internal const int MUI_LANGUAGE_ID = 4;
        internal const int MUI_LANGUAGE_NAME = 8;
        internal const int MUI_USER_PREFERRED_UI_LANGUAGES = 16;
        internal const int MUI_USE_INSTALLED_LANGUAGES = 32;
        internal const int MUI_USE_SEARCH_ALL_LANGUAGES = 64;
        internal const int MUI_LANG_NEUTRAL_PE_FILE = 256;
        internal const int MUI_NON_LANG_NEUTRAL_FILE = 512;
        internal const int LOCALE_NAME_MAX_LENGTH = 85;
        public const uint WM_PRINT = 791U;
        public const uint WM_PRINTCLIENT = 792U;
        public const uint PRF_CLIENT = 4U;
        public const uint PRF_CHILDREN = 16U;
        public const uint PW_CLIENTONLY = 1U;
        public const int SRCCOPY = 13369376;
        public const uint WM_INVALIDATEDRAGIMAGE = 1027U;
        public const string IsShowingLayeredFormat = "IsShowingLayered";
        public const int STGM_CREATE = 4096;
        public const int WM_NCLBUTTONDOWN = 161;
        public const int HTCAPTION = 2;
        public const int GWL_EXSTYLE = -20;
        public const int GWL_STYLE = -16;
        public const int WS_EX_DLGMODALFRAME = 1;
        public const int WS_CAPTION = 12582912;
        public const int WS_MINIMIZEBOX = 131072;
        public const int WS_MAXIMIZEBOX = 65536;
        public const int WS_SYSMENU = 524288;
        public const int SWP_NOSIZE = 1;
        public const int SWP_NOMOVE = 2;
        public const int SWP_NOZORDER = 4;
        public const int SWP_FRAMECHANGED = 32;
        public const uint WM_SETICON = 128U;
        public const int WH_MOUSE_LL = 14;
        public const int WH_KEYBOARD_LL = 13;
        public const int WH_MOUSE = 7;
        public const int WH_KEYBOARD = 2;
        public const int WM_MOUSEMOVE = 512;
        public const int WM_LBUTTONDOWN = 513;
        public const int WM_RBUTTONDOWN = 516;
        public const int WM_MBUTTONDOWN = 519;
        public const int WM_LBUTTONUP = 514;
        public const int WM_RBUTTONUP = 517;
        public const int WM_MBUTTONUP = 520;
        public const int WM_LBUTTONDBLCLK = 515;
        public const int WM_RBUTTONDBLCLK = 518;
        public const int WM_MBUTTONDBLCLK = 521;
        public const int WM_MOUSEWHEEL = 522;
        public const int WM_KEYDOWN = 256;
        public const int WM_KEYUP = 257;
        public const int WM_SYSKEYDOWN = 260;
        public const int WM_SYSKEYUP = 261;
        public const byte VK_SHIFT = (byte)16;
        public const byte VK_CAPITAL = (byte)20;
        public const byte VK_NUMLOCK = (byte)144;
        public const string DropDescriptionFormat = "DropDescription";
        public const string IsShowingTextFormat = "IsShowingText";
        public const string DisableDragText = "DisableDragText";
        public const string FileContentsFormat = "FileContents";
        public const string FileGroupDescriptorFormat = "FileGroupDescriptorW";
        public const int WM_FONTCHANGE = 29;
        public const int HWND_BROADCAST = 65535;
        public const int OLE_E_ADVISENOTSUPPORTED = -2147221501;
        public const int DV_E_FORMATETC = -2147221404;
        public const int DV_E_TYMED = -2147221399;
        public const int DV_E_CLIPFORMAT = -2147221398;
        public const int DV_E_DVASPECT = -2147221397;
        public const TYMED TYMED_ANY = TYMED.TYMED_HGLOBAL | TYMED.TYMED_FILE | TYMED.TYMED_ISTREAM | TYMED.TYMED_ISTORAGE | TYMED.TYMED_GDI | TYMED.TYMED_MFPICT | TYMED.TYMED_ENHMF;

        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern bool CloseHandle(IntPtr handle);

        [DllImport("crypt32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern bool CryptProtectData(ref DATA_BLOB pPlainText, string szDescription, ref DATA_BLOB pEntropy, IntPtr pReserved, ref CRYPTPROTECT_PROMPTSTRUCT pPrompt, int dwFlags, ref DATA_BLOB pCipherText);

        [DllImport("crypt32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern bool CryptUnprotectData(ref DATA_BLOB pCipherText, ref string pszDescription, ref DATA_BLOB pEntropy, IntPtr pReserved, ref CRYPTPROTECT_PROMPTSTRUCT pPrompt, int dwFlags, ref DATA_BLOB pPlainText);

        [DllImport("kernel32.dll", CharSet = CharSet.Unicode)]
        public static extern IntPtr CreateJobObject([In] ref SECURITY_ATTRIBUTES lpJobAttributes, string lpName);

        [DllImport("kernel32.dll")]
        public static extern bool QueryInformationJobObject(IntPtr hJob, JobObjectInfoClass JobObjectInformationClass, ref JOBOBJECT_BASIC_PROCESS_ID_LIST lpJobObjectInfo, int cbJobObjectInfoLength, IntPtr lpReturnLength);

        [DllImport("kernel32.dll", EntryPoint = "SetInformationJobObject", SetLastError = true)]
        public static extern bool SetInformationJobObjectLimit(IntPtr hJob, JobObjectInfoClass JobObjectInfoClass, ref JOBOBJECT_BASIC_LIMIT_INFORMATION lpJobObjectInfo, int cbJobObjectInfoLength);

        [DllImport("kernel32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool AssignProcessToJobObject(IntPtr hJob, IntPtr hProcess);

        public static ushort MAKELANGID(byte primaryLanguage, byte subLanguage)
        {
            return (ushort)((uint)subLanguage << 10 | (uint)primaryLanguage);
        }

        public static byte PRIMARYLANGID(ushort languageId)
        {
            return (byte)((uint)languageId & 1023U);
        }

        public static byte SUBLANGID(ushort languageId)
        {
            return (byte)((uint)languageId >> 10);
        }

        [DllImport("kernel32.dll")]
        public static extern bool EnumResourceNames(IntPtr hModule, int lpszType, Advent.Common.Interop.NativeMethods.EnumResNameDelegate lpEnumFunc, IntPtr lParam);

        [DllImport("kernel32.dll")]
        public static extern bool EnumResourceNames(IntPtr hModule, string lpszType, Advent.Common.Interop.NativeMethods.EnumResNameDelegate lpEnumFunc, IntPtr lParam);

        [DllImport("kernel32.dll", SetLastError = true)]
        internal static extern IntPtr LoadLibraryEx(string lpFileName, IntPtr hFile, uint dwFlags);

        [DllImport("kernel32.dll", SetLastError = true)]
        internal static extern bool FreeLibrary(IntPtr hModule);

        [DllImport("user32.dll")]
        internal static extern int LoadString(IntPtr hInstance, int uID, StringBuilder lpBuffer, int nBufferMax);

        [DllImport("kernel32.dll", SetLastError = true)]
        internal static extern IntPtr FindResource(IntPtr hModule, IntPtr lpName, IntPtr lpType);

        [DllImport("kernel32.dll", SetLastError = true)]
        internal static extern IntPtr FindResource(IntPtr hModule, int lpName, int lpType);

        [DllImport("kernel32.dll", SetLastError = true)]
        internal static extern IntPtr FindResource(IntPtr hModule, int lpName, string lpType);

        [DllImport("kernel32.dll", SetLastError = true)]
        internal static extern IntPtr FindResource(IntPtr hModule, string lpName, int lpType);

        [DllImport("kernel32.dll", SetLastError = true)]
        internal static extern IntPtr FindResource(IntPtr hModule, string lpName, string lpType);

        [DllImport("kernel32.dll")]
        internal static extern IntPtr FindResourceEx(IntPtr hModule, string lpType, int lpName, ushort wLanguage);

        [DllImport("kernel32.dll")]
        internal static extern IntPtr FindResourceEx(IntPtr hModule, string lpType, string lpName, ushort wLanguage);

        [DllImport("kernel32.dll")]
        internal static extern IntPtr FindResourceEx(IntPtr hModule, int lpType, string lpName, ushort wLanguage);

        [DllImport("kernel32.dll")]
        internal static extern IntPtr FindResourceEx(IntPtr hModule, int lpType, int lpName, ushort wLanguage);

        [DllImport("kernel32.dll", CallingConvention = CallingConvention.Cdecl, SetLastError = true)]
        internal static extern IntPtr LockResource(IntPtr h);

        [DllImport("kernel32.dll", SetLastError = true)]
        internal static extern uint SizeofResource(IntPtr hModule, IntPtr hResInfo);

        [DllImport("kernel32.dll", SetLastError = true)]
        internal static extern IntPtr LoadResource(IntPtr hModule, IntPtr hResInfo);

        [DllImport("kernel32.dll", CharSet = CharSet.Unicode)]
        internal static extern IntPtr BeginUpdateResource(string pFileName, bool bDeleteExistingResources);

        [DllImport("kernel32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        internal static extern bool UpdateResource(IntPtr hUpdate, string lpType, string lpName, ushort wLanguage, byte[] lpData, int cbData);

        [DllImport("kernel32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        internal static extern bool UpdateResource(IntPtr hUpdate, int lpType, string lpName, ushort wLanguage, byte[] lpData, int cbData);

        [DllImport("kernel32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        internal static extern bool UpdateResource(IntPtr hUpdate, string lpType, int lpName, ushort wLanguage, byte[] lpData, int cbData);

        [DllImport("kernel32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        internal static extern bool UpdateResource(IntPtr hUpdate, int lpType, int lpName, ushort wLanguage, byte[] lpData, int cbData);

        [DllImport("kernel32.dll", SetLastError = true)]
        internal static extern bool EndUpdateResource(IntPtr hUpdate, bool fDiscard);

        [DllImport("kernel32.dll", SetLastError = true)]
        internal static extern bool EnumResourceLanguages(IntPtr hModule, string lpType, string lpName, Advent.Common.Interop.NativeMethods.EnumResLangProc lpEnumFunc, int lParam);

        [DllImport("kernel32.dll", SetLastError = true)]
        internal static extern bool EnumResourceLanguages(IntPtr hModule, int lpType, string lpName, Advent.Common.Interop.NativeMethods.EnumResLangProc lpEnumFunc, int lParam);

        [DllImport("kernel32.dll", SetLastError = true)]
        internal static extern bool EnumResourceLanguages(IntPtr hModule, string lpType, int lpName, Advent.Common.Interop.NativeMethods.EnumResLangProc lpEnumFunc, int lParam);

        [DllImport("kernel32.dll", SetLastError = true)]
        internal static extern bool EnumResourceLanguages(IntPtr hModule, int lpType, int lpName, Advent.Common.Interop.NativeMethods.EnumResLangProc lpEnumFunc, int lParam);

        [DllImport("kernel32.dll")]
        internal static extern bool EnumResourceTypes(IntPtr hModule, Advent.Common.Interop.NativeMethods.EnumResTypeProc lpEnumFunc, IntPtr lParam);

        [DllImport("kernel32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        internal static extern bool GetFileMUIPath(int dwFlags, [MarshalAs(UnmanagedType.BStr)] string pcwstrFilePath, [Out] StringBuilder pwszLanguage, ref int pcchLanguage, [Out] StringBuilder pwszFileMUIPath, ref int pcchFileMUIPath, ref long pululEnumerator);

        internal static bool IS_INTRESOURCE(IntPtr value)
        {
            return (uint)(int)value <= (uint)ushort.MaxValue;
        }

        internal static uint GET_RESOURCE_ID(IntPtr value)
        {
            if (Advent.Common.Interop.NativeMethods.IS_INTRESOURCE(value))
                return (uint)(int)value;
            else
                throw new ArgumentException("Value must be an ID.");
        }

        internal static bool GET_RESOURCE_NAME(IntPtr value, out int id, out string name)
        {
            if (Advent.Common.Interop.NativeMethods.IS_INTRESOURCE(value))
            {
                id = value.ToInt32();
                name = (string)null;
                return false;
            }
            else
            {
                name = Marshal.PtrToStringAnsi(value);
                id = 0;
                return true;
            }
        }

        [DllImport("user32.dll")]
        public static extern IntPtr GetDesktopWindow();

        [DllImport("user32.dll")]
        public static extern IntPtr GetWindowDC(IntPtr ptr);

        [DllImport("User32.dll")]
        public static extern bool PrintWindow(IntPtr hWnd, IntPtr dc, uint reservedFlag);

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool GetWindowRect(IntPtr hWnd, out Advent.Common.Interop.RECT lpRect);

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool GetClientRect(IntPtr hWnd, out Advent.Common.Interop.RECT lpRect);

        [DllImport("gdi32.dll")]
        public static extern IntPtr CreateCompatibleDC(IntPtr hDC);

        [DllImport("gdi32.dll")]
        public static extern IntPtr CreateCompatibleBitmap(IntPtr hDC, int nWidth, int nHeight);

        [DllImport("gdi32.dll")]
        public static extern IntPtr SelectObject(IntPtr hDC, IntPtr hObject);

        [DllImport("gdi32.dll")]
        public static extern bool BitBlt(IntPtr hDestDC, int X, int Y, int nWidth, int nHeight, IntPtr hSrcDC, int SrcX, int SrcY, CopyPixelOperation Rop);

        [DllImport("gdi32.dll")]
        public static extern IntPtr DeleteDC(IntPtr hDC);

        [DllImport("user32.dll")]
        public static extern IntPtr GetDC(IntPtr ptr);

        [DllImport("user32.dll")]
        public static extern IntPtr ReleaseDC(IntPtr hWnd, IntPtr hDc);

        [DllImport("user32.dll", SetLastError = true)]
        public static extern IntPtr FindWindow(string lpClassName, string lpWindowName);

        [DllImport("user32.dll")]
        public static extern void PostMessage(IntPtr hWnd, uint Msg, IntPtr wParam, IntPtr lParam);

        [DllImport("gdi32.dll")]
        public static extern bool DeleteObject(IntPtr hgdi);

        [DllImport("user32.dll")]
        public static extern int SendMessage(IntPtr hWnd, uint Msg, int wParam, int lParam);

        [DllImport("user32.dll")]
        public static extern int ReleaseCapture(IntPtr hwnd);

        [DllImport("shell32.dll", CharSet = CharSet.Unicode, PreserveSig = false)]
        public static extern void SHCreateItemFromParsingName([MarshalAs(UnmanagedType.LPWStr), In] string pszPath, [In] IBindCtx pbc, [MarshalAs(UnmanagedType.LPStruct), In] Guid riid, [MarshalAs(UnmanagedType.Interface)] out IShellItem ppv);

        [DllImport("user32.dll")]
        public static extern int GetWindowLong(IntPtr hwnd, int index);

        [DllImport("user32.dll")]
        public static extern int SetWindowLong(IntPtr hwnd, int index, int newStyle);

        [DllImport("user32.dll")]
        public static extern bool SetWindowPos(IntPtr hwnd, IntPtr hwndInsertAfter, int x, int y, int width, int height, uint flags);

        [DllImport("user32.dll")]
        public static extern IntPtr SendMessage(IntPtr hwnd, uint msg, IntPtr wParam, IntPtr lParam);

        [DllImport("user32.dll")]
        public static extern IntPtr SendMessage(IntPtr hwnd, uint msg, IntPtr wParam, uint lParam);

        [DllImport("dwmapi.dll", PreserveSig = false)]
        public static extern void DwmExtendFrameIntoClientArea(IntPtr hwnd, ref MARGINS margins);

        [DllImport("dwmapi.dll", PreserveSig = false)]
        public static extern bool DwmIsCompositionEnabled();

        [DllImport("user32.dll", CharSet = CharSet.Auto, CallingConvention = CallingConvention.StdCall, SetLastError = true)]
        public static extern int SetWindowsHookEx(int idHook, Advent.Common.Interop.NativeMethods.HookProc lpfn, IntPtr hMod, int dwThreadId);

        [DllImport("user32.dll", CharSet = CharSet.Auto, CallingConvention = CallingConvention.StdCall, SetLastError = true)]
        public static extern int UnhookWindowsHookEx(int idHook);

        [DllImport("user32.dll", CharSet = CharSet.Auto, CallingConvention = CallingConvention.StdCall)]
        public static extern int CallNextHookEx(int idHook, int nCode, int wParam, IntPtr lParam);

        [DllImport("user32.dll")]
        public static extern int ToAscii(int uVirtKey, int uScanCode, byte[] lpbKeyState, byte[] lpwTransKey, int fuState);

        [DllImport("user32.dll")]
        public static extern int GetKeyboardState(byte[] pbKeyState);

        [DllImport("user32.dll", CharSet = CharSet.Auto, CallingConvention = CallingConvention.StdCall)]
        public static extern short GetKeyState(int vKey);

        [DllImport("user32.dll")]
        public static extern uint RegisterClipboardFormat(string lpszFormatName);

        [DllImport("ole32.dll")]
        public static extern void ReleaseStgMedium(ref STGMEDIUM pmedium);

        [DllImport("ole32.dll")]
        public static extern int CreateStreamOnHGlobal(IntPtr hGlobal, bool fDeleteOnRelease, out System.Runtime.InteropServices.ComTypes.IStream ppstm);

        [DllImport("urlmon.dll")]
        public static extern int CopyStgMedium(ref STGMEDIUM pcstgmedSrc, ref STGMEDIUM pstgmedDest);

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool GetCursorPos(out POINT lpPoint);

        [DllImport("dwmapi.dll")]
        public static extern int DwmRegisterThumbnail(IntPtr DestinationHwnd, IntPtr SourceHwnd, ref IntPtr HThumbnail);

        [DllImport("dwmapi.dll")]
        public static extern int DwmUnregisterThumbnail(IntPtr HThumbnail);

        [DllImport("dwmapi.dll")]
        public static extern int DwmUpdateThumbnailProperties(IntPtr HThumbnail, ref DwmThumbnailProperties props);

        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern int WriteProfileString(string lpszSection, string lpszKeyName, string lpszString);

        [DllImport("user32.dll")]
        public static extern int SendMessage(int hWnd, uint Msg, int wParam, int lParam);

        [DllImport("gdi32.dll", EntryPoint = "AddFontResourceW", SetLastError = true)]
        public static extern int AddFontResource([In][MarshalAs(UnmanagedType.LPWStr)] string lpFileName);

        [DllImport("gdi32.dll", EntryPoint = "RemoveFontResourceW", SetLastError = true)]
        public static extern int RemoveFontResource([In][MarshalAs(UnmanagedType.LPWStr)] string lpFileName);

        [DllImport("shfolder.dll", CharSet = CharSet.Auto)]
        public static extern int SHGetFolderPath(IntPtr hwndOwner, int nFolder, IntPtr hToken, int dwFlags, StringBuilder lpszPath);

        [DllImport("shell32.dll")]
        public static extern bool SHGetKnownFolderPath([MarshalAs(UnmanagedType.LPStruct)] Guid rfid, uint flags, IntPtr token, out IntPtr pszPath);

        public delegate bool EnumResNameDelegate(IntPtr hModule, IntPtr lpszType, IntPtr lpszName, IntPtr lParam);

        internal delegate bool EnumResLangProc(IntPtr hModule, int type, int name, ushort language, IntPtr lParam);

        internal delegate bool EnumResTypeProc(IntPtr hModule, IntPtr lpszType, IntPtr lParam);

        public delegate int HookProc(int nCode, int wParam, IntPtr lParam);
    }
}
