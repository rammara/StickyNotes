using System.Runtime.InteropServices;

namespace StickyNotes.Services
{
    public class NativeWindowHandler : IDisposable
    {
        private IntPtr _windowHandle = IntPtr.Zero;
        private bool _disposed = false;

        private const int WS_EX_TOOLWINDOW = 0x00000080;
        private const int WS_EX_NOACTIVATE = 0x08000000;
        private const int WS_POPUP = unchecked((int)0x80000000);
        private const int WM_DESTROY = 0x0002;
        private const int WM_CLOSE = 0x0010;

        private IntPtr _originalWndProc;
        private const int GWLP_WNDPROC = -4;

        public IntPtr WindowHandle => _windowHandle;

        public NativeWindowHandler()
        {
            CreateHiddenWindow();
        } // NativeWindowHandler

        private void CreateHiddenWindow()
        {
            try
            {
                // Создаем окно с использованием предопределенного класса "STATIC"
                // STATIC - стандартный класс Windows, который уже зарегистрирован системой
                _windowHandle = NativeMethods.CreateWindowExInternal(
                    WS_EX_TOOLWINDOW | WS_EX_NOACTIVATE,
                    "STATIC",  // Используем предопределенный класс
                    "StickyNotes Hidden Window",
                    WS_POPUP,
                    0, 0, 1, 1,  // Минимальные размеры, окно невидимо
                    IntPtr.Zero,
                    IntPtr.Zero,
                    NativeMethods.GetModuleHandleInternal(null),
                    IntPtr.Zero);

                if (_windowHandle == IntPtr.Zero)
                {
                    int error = Marshal.GetLastWin32Error();
                    throw new InvalidOperationException($"Failed to create window. Error: {error}");
                }

                System.Diagnostics.Debug.WriteLine($"Window created successfully. Handle: {_windowHandle}");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error creating window: {ex.Message}");
                throw;
            }
        } // CreateHiddenWindow

        private static IntPtr WindowProc(IntPtr hWnd, uint msg, IntPtr wParam, IntPtr lParam)
        {
            // Обрабатываем сообщения окна
            switch (msg)
            {
                case WM_CLOSE:
                    NativeMethods.DestroyWindowInternal(hWnd);
                    return IntPtr.Zero;

                case WM_DESTROY:
                    // Не завершаем приложение, так как это скрытое окно
                    return IntPtr.Zero;
            }
            return NativeMethods.DefWindowProcInternal(hWnd, msg, wParam, lParam);
        } // WindowProc

        private void UnhookWindowProc()
        {
            if (_windowHandle != IntPtr.Zero && _originalWndProc != IntPtr.Zero)
            {
                NativeMethods.SetWindowLongPtrInternal(_windowHandle, GWLP_WNDPROC, _originalWndProc);
                _originalWndProc = IntPtr.Zero;
            }
        } // UnhookWindowProc

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        } // Dispose

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    // Освобождаем управляемые ресурсы
                }

                UnhookWindowProc();

                if (_windowHandle != IntPtr.Zero)
                {
                    NativeMethods.DestroyWindowInternal(_windowHandle);
                    _windowHandle = IntPtr.Zero;
                }
                _disposed = true;
            }
        } // Dispose

        ~NativeWindowHandler()
        {
            Dispose(false);
        } // ~NativeWindowHandler
    } // NativeWindowHandler
} // namespace