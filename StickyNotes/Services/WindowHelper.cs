using StickyNotes.Services;
using System.Runtime.InteropServices;
using System.Windows.Interop;
using System.Windows;

public static class WindowHelper
{
    /// <summary>
    /// Принудительно активирует и отображает окно на переднем плане
    /// </summary>
    public static void ForceForegroundWindow(Window window)
    {
        if (window == null) return;

        // Создаем хендл окна, если он еще не создан
        var hwnd = new WindowInteropHelper(window).EnsureHandle();

        // Получаем ID потока окна
        uint windowThreadProcessId = NativeMethods.GetWindowThreadProcessIdInternal(hwnd, out _);
        uint currentThreadId = NativeMethods.GetCurrentThreadIdInternal();

        // Если окно свернуто - восстанавливаем его
        if (NativeMethods.IsIconicInternal(hwnd))
        {
            NativeMethods.ShowWindowInternal(hwnd, NativeMethods.SW_RESTORE);
        }

        // Показываем окно
        NativeMethods.ShowWindowInternal(hwnd, NativeMethods.SW_SHOWNA);

        // Аттачим поток ввода, чтобы SetForegroundWindow работал
        if (windowThreadProcessId != currentThreadId)
        {
            NativeMethods.AttachThreadInputInternal(windowThreadProcessId, currentThreadId, true);
        }

        try
        {
            // Устанавливаем окно на передний план
            NativeMethods.SetForegroundWindowInternal(hwnd);

            // Делаем окно "топ-мост" на короткое время, чтобы гарантировать его появление поверх
            NativeMethods.SetWindowPosInternal(
                hwnd,
                NativeMethods.HWND_TOPMOST,
                0, 0, 0, 0,
                NativeMethods.SWP_NOSIZE | NativeMethods.SWP_NOMOVE | NativeMethods.SWP_SHOWWINDOW
            );

            // Убираем топ-мост статус, но оставляем окно видимым
            NativeMethods.SetWindowPosInternal(
                hwnd,
                NativeMethods.HWND_NOTOPMOST,
                0, 0, 0, 0,
                NativeMethods.SWP_NOSIZE | NativeMethods.SWP_NOMOVE | NativeMethods.SWP_SHOWWINDOW
            );
        }
        finally
        {
            // Отсоединяем поток ввода
            if (windowThreadProcessId != currentThreadId)
            {
                NativeMethods.AttachThreadInputInternal(windowThreadProcessId, currentThreadId, false);
            }
        }

        // Мигаем иконкой в таскбаре для привлечения внимания
        FlashWindow(window);
    }

    /// <summary>
    /// Мигает окном в таскбаре
    /// </summary>
    public static void FlashWindow(Window window)
    {
        var hwnd = new WindowInteropHelper(window).Handle;

        var fwi = new FLASHWINFO
        {
            cbSize = (uint)Marshal.SizeOf(typeof(FLASHWINFO)),
            hwnd = hwnd,
            dwFlags = NativeMethods.FLASHW_ALL | NativeMethods.FLASHW_TIMERNOFG,
            uCount = 3, // Количество миганий
            dwTimeout = 0 // Использовать системное значение по умолчанию
        };

        NativeMethods.FlashWindowExInternal(ref fwi);
    }
}