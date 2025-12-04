using System.Runtime.InteropServices;

namespace StickyNotes.Services
{

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
        private static extern IntPtr GetModuleHandle(string lpModuleName);
        public static IntPtr GetModuleHandleInternal(string lpModuleName) => GetModuleHandle(lpModuleName);

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
    } // NativeMethods
} // namespace