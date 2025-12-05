using System.Runtime.InteropServices;

namespace StickyNotes.Services
{
    public delegate IntPtr WndProcDelegate(IntPtr hWnd, uint msg, IntPtr wParam, IntPtr lParam);



    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
    public struct WNDCLASSEX
    {
        public int cbSize;
        public int style;
        public WndProcDelegate lpfnWndProc;
        public int cbClsExtra;
        public int cbWndExtra;
        public IntPtr hInstance;
        public IntPtr hIcon;
        public IntPtr hCursor;
        public IntPtr hbrBackground;
        public string? lpszMenuName;
        public string? lpszClassName;
        public IntPtr hIconSm;
    } // WNDCLASSEX

    [StructLayout(LayoutKind.Sequential)]
    public struct FLASHWINFO
    {
        public uint cbSize;
        public IntPtr hwnd;
        public uint dwFlags;
        public uint uCount;
        public uint dwTimeout;
    } // FLASHWINFO

    [StructLayout(LayoutKind.Sequential)]
    public struct KBDLLHOOKSTRUCT
    {
        public uint vkCode;
        public uint scanCode;
        public uint flags;
        public uint time;
        public IntPtr dwExtraInfo;
    } // KBDLLHOOKSTRUCT

    // [System.Diagnostics.CodeAnalysis.SuppressMessage("Interoperability", "CA1401:PInvokesShouldNotBeVisible")]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Interoperability", "SYSLIB1054:UseLibraryImportAttributeInsteadOfDllImportAttributeToGeneratePInvoke")]
    internal static class NativeMethods
    {
        // Константы для хука клавиатуры
        public const int WH_KEYBOARD_LL = 13;
        public const int WM_KEYDOWN = 0x0100;
        public const int WM_KEYUP = 0x0101;
        public const int WM_SYSKEYDOWN = 0x0104;
        public const int WM_SYSKEYUP = 0x0105;

        // Константы для трея
        public const int NIM_ADD = 0x00000000;
        public const int NIM_MODIFY = 0x00000001;
        public const int NIM_DELETE = 0x00000002;
        public const int NIF_MESSAGE = 0x00000001;
        public const int NIF_ICON = 0x00000002;
        public const int NIF_TIP = 0x00000004;
        public const int NIF_INFO = 0x00000010;
        public const int WM_USER = 0x0400;
        public const int WM_LBUTTONDBLCLK = 0x0203;
        public const int WM_RBUTTONUP = 0x0205;

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
        public struct NOTIFYICONDATA
        {
            public int cbSize;
            public IntPtr hWnd;
            public int uID;
            public int uFlags;
            public int uCallbackMessage;
            public IntPtr hIcon;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 128)]
            public string szTip;
            public int dwState;
            public int dwStateMask;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 256)]
            public string szInfo;
            public int uTimeoutOrVersion;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 64)]
            public string szInfoTitle;
            public int dwInfoFlags;
            public Guid guidItem;
            public IntPtr hBalloonIcon;
        } // NOTIFYICONDATA

        // Делегат для хука
        public delegate IntPtr LowLevelKeyboardProc(int nCode, IntPtr wParam, IntPtr lParam);

        public const int GWLP_WNDPROC = -4;


        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr SetWindowsHookEx(int idHook, LowLevelKeyboardProc lpfn, IntPtr hMod, uint dwThreadId);
        public static IntPtr SetWindowsHookExInternal(int idHook, LowLevelKeyboardProc lpfn, IntPtr hMod, uint dwThreadId) =>
            SetWindowsHookEx(idHook, lpfn, hMod, dwThreadId);
        
        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool UnhookWindowsHookEx(IntPtr hhk);
        public static bool UnhookWindowsHookExInternal(IntPtr hhk) => UnhookWindowsHookEx(hhk);


        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr CallNextHookEx(IntPtr hhk, int nCode, IntPtr wParam, IntPtr lParam);
        public static IntPtr CallNextHookExInternal(IntPtr hhk, int nCode, IntPtr wParam, IntPtr lParam) => CallNextHookEx(hhk, nCode, wParam, lParam);

        [DllImport("kernel32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        private static extern IntPtr GetModuleHandle(string? lpModuleName);
        public static IntPtr GetModuleHandleInternal(string? lpModuleName) => GetModuleHandle(lpModuleName);

        [DllImport("shell32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool Shell_NotifyIcon(int dwMessage, ref NOTIFYICONDATA pnid);
        public static bool Shell_NotifyIconInternal(int dwMessage, ref NOTIFYICONDATA pnid) => Shell_NotifyIcon(dwMessage, ref pnid);

        [DllImport("user32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool DestroyIcon(IntPtr hIcon);
        public static bool DestroyIconInternal(IntPtr hIcon) => DestroyIcon(hIcon);

        [DllImport("user32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        private static extern IntPtr LoadImage(IntPtr hinst, string lpszName, uint uType,
            int cxDesired, int cyDesired, uint fuLoad);

        public static IntPtr LoadImageInternal(IntPtr hinst, string lpszName, uint uType,
            int cxDesired, int cyDesired, uint fuLoad) => LoadImage(hinst, lpszName, uType, cxDesired, cyDesired, fuLoad);

        [DllImport("user32.dll", SetLastError = true)]
        private static extern IntPtr GetSystemMenu(IntPtr hWnd, bool bRevert);
        public static IntPtr GetSystemMenuInternal(IntPtr hWnd, bool bRevert) => GetSystemMenu(hWnd, bRevert);

        [DllImport("user32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool EnableMenuItem(IntPtr hMenu, uint uIDEnableItem, uint uEnable);
        public static bool EnableMenuItemInternal(IntPtr hMenu, uint uIDEnableItem, uint uEnable) => EnableMenuItem(hMenu, uIDEnableItem, uEnable);


        [DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
        private static extern IntPtr CreateWindowEx(
            int dwExStyle,
            string lpClassName,
            string lpWindowName,
            int dwStyle,
            int x,
            int y,
            int nWidth,
            int nHeight,
            IntPtr hWndParent,
            IntPtr hMenu,
            IntPtr hInstance,
            IntPtr lpParam);
        public static IntPtr CreateWindowExInternal(
            int dwExStyle,
            string lpClassName,
            string lpWindowName,
            int dwStyle,
            int x,
            int y,
            int nWidth,
            int nHeight,
            IntPtr hWndParent,
            IntPtr hMenu,
            IntPtr hInstance,
            IntPtr lpParam) =>
                CreateWindowEx(
                    dwExStyle,
                    lpClassName, 
                    lpWindowName, 
                    dwStyle, 
                    x,
                    y,
                    nWidth,
                    nHeight,
                    hWndParent,
                    hMenu,
                    hInstance,
                    lpParam);
        


        [DllImport("user32.dll", SetLastError = true)]
        private static extern bool DestroyWindow(IntPtr hWnd);
        public static bool DestroyWindowInternal(IntPtr hWnd) => DestroyWindow(hWnd);

        [DllImport("user32.dll", SetLastError = true)]
        private static extern IntPtr DefWindowProc(IntPtr hWnd, uint msg, IntPtr wParam, IntPtr lParam);
        public static IntPtr DefWindowProcInternal(IntPtr hWnd, uint msg, IntPtr wParam, IntPtr lParam) =>
            DefWindowProc(hWnd, msg, wParam, lParam);
        [DllImport("user32.dll", SetLastError = true)]
        private static extern ushort RegisterClassEx(ref WNDCLASSEX lpwcx);
        public static ushort RegisterClassExInternal(ref WNDCLASSEX lpwcx) => RegisterClassEx(ref lpwcx);

        [DllImport("user32.dll", SetLastError = true)]
        private static extern IntPtr SetWindowLongPtr(IntPtr hWnd, int nIndex, IntPtr dwNewLong);
        public static IntPtr SetWindowLongPtrInternal(IntPtr hWnd, int nIndex, IntPtr dwNewLong) =>
            SetWindowLongPtr(hWnd, nIndex, dwNewLong);

        [DllImport("user32.dll")]
        public static extern short GetKeyState(int nVirtKey);
        public static short GetKeyStateInternal(int nVirtKey) => GetKeyState(nVirtKey);


        [DllImport("user32.dll")]
        public static extern IntPtr CallWindowProc(IntPtr lpPrevWndFunc, IntPtr hWnd, uint msg, IntPtr wParam, IntPtr lParam);

        public static IntPtr CallWindowProcInternal(IntPtr lpPrevWndFunc, IntPtr hWnd, uint msg, IntPtr wParam, IntPtr lParam) =>
            CallWindowProc(lpPrevWndFunc, hWnd, msg, wParam, lParam);

        [DllImport("user32.dll")]
        public static extern bool GetKeyboardState(byte[] lpKeyState);
        public static bool GetKeyboardStateInternal(byte[] lpKeyState) => GetKeyboardState(lpKeyState);

        [DllImport("user32.dll")]
        public static extern short GetAsyncKeyState(int vKey);
        public static short GetAsyncKeyStateInternal(int vKey) => GetAsyncKeyState(vKey);


        [DllImport("user32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool SetForegroundWindow(IntPtr hWnd);
        public static bool SetForegroundWindowInternal(IntPtr hWnd) => SetForegroundWindow(hWnd);

        [DllImport("user32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);
        public static bool ShowWindowInternal(IntPtr hWnd, int nCmdShow) => ShowWindow(hWnd, nCmdShow);

        [DllImport("user32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter,
        int X, int Y, int cx, int cy, uint uFlags);
        public static bool SetWindowPosInternal(IntPtr hWnd, IntPtr hWndInsertAfter,
            int X, int Y, int cx, int cy, uint uFlags) =>
            SetWindowPos(hWnd, hWndInsertAfter, X, Y, cx, cy, uFlags);

        [DllImport("user32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool FlashWindowEx(ref FLASHWINFO pwfi);

        public static bool FlashWindowExInternal(ref FLASHWINFO pwfi) => FlashWindowEx(ref pwfi);

        [DllImport("user32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool IsIconic(IntPtr hWnd);

        public static bool IsIconicInternal(IntPtr hWnd) => IsIconic(hWnd);

        [DllImport("user32.dll", SetLastError = true)]
        private static extern uint GetWindowThreadProcessId(IntPtr hWnd, out uint lpdwProcessId);

        public static uint GetWindowThreadProcessIdInternal(IntPtr hWnd, out uint lpdwProcessId) =>
            GetWindowThreadProcessId(hWnd, out lpdwProcessId);

        [DllImport("kernel32.dll")]
        private static extern uint GetCurrentThreadId();

        public static uint GetCurrentThreadIdInternal() => GetCurrentThreadId();

        [DllImport("user32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool AttachThreadInput(uint idAttach, uint idAttachTo, bool fAttach);

        public static bool AttachThreadInputInternal(uint idAttach, uint idAttachTo, bool fAttach) =>
            AttachThreadInput(idAttach, idAttachTo, fAttach);

        // Константы для ShowWindow
        public const int SW_RESTORE = 9;
        public const int SW_SHOW = 5;
        public const int SW_SHOWNA = 8;

        // Константы для SetWindowPos
        public static readonly IntPtr HWND_TOPMOST = new IntPtr(-1);
        public static readonly IntPtr HWND_NOTOPMOST = new IntPtr(-2);
        public static readonly IntPtr HWND_TOP = new IntPtr(0);
        public const uint SWP_NOSIZE = 0x0001;
        public const uint SWP_NOMOVE = 0x0002;
        public const uint SWP_NOACTIVATE = 0x0010;
        public const uint SWP_SHOWWINDOW = 0x0040;

        // Константы для FlashWindowEx
        public const uint FLASHW_ALL = 0x00000003;
        public const uint FLASHW_TIMERNOFG = 0x0000000C;
        public const uint FLASHW_TRAY = 0x00000002;
        public const uint FLASHW_STOP = 0;
        public const uint FLASHW_CAPTION = 0x00000001;
    } // NativeMethods
} // namespace