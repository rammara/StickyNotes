using System;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows.Interop;

namespace StickyNotes.Services
{
    public class TrayIconService : IDisposable
    {
        private readonly NativeWindowHandler _windowHandler;
        private NativeMethods.NOTIFYICONDATA _iconData;
        private bool _disposed = false;
        private bool _iconAdded = false;
        private WndProcDelegate? _windowProcDelegate;
        private IntPtr _originalWndProc;

        public event EventHandler? DoubleClick;
        public event EventHandler? RightClick;

        public TrayIconService(NativeWindowHandler windowHandler)
        {
            _windowHandler = windowHandler;
            Initialize();
        } // TrayIconService

        private void Initialize()
        {
            try
            {
                // Инициализируем структуру иконки
                _iconData = new NativeMethods.NOTIFYICONDATA
                {
                    cbSize = Marshal.SizeOf(typeof(NativeMethods.NOTIFYICONDATA)),
                    uID = 0,
                    uFlags = NativeMethods.NIF_MESSAGE | NativeMethods.NIF_ICON | NativeMethods.NIF_TIP,
                    uCallbackMessage = NativeMethods.WM_USER + 1,
                    szTip = "StickyNotes",
                    hWnd = _windowHandler.WindowHandle
                };

                // Загружаем иконку
                LoadIcon();

                // Устанавливаем свою оконную процедуру для обработки сообщений
                HookWindowProc();

                // Добавляем иконку в трей
                AddTrayIcon();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Ошибка инициализации TrayIconService: {ex.Message}");
                throw;
            }
        } // Initialize

        private void HookWindowProc()
        {
            if (_windowHandler.WindowHandle == IntPtr.Zero) return;

            // Создаем делегат для оконной процедуры
            _windowProcDelegate = new WndProcDelegate(WindowProc);

            // Устанавливаем новую оконную процедуру и сохраняем оригинальную
            _originalWndProc = NativeMethods.SetWindowLongPtrInternal(
                _windowHandler.WindowHandle,
                NativeMethods.GWLP_WNDPROC,
                Marshal.GetFunctionPointerForDelegate(_windowProcDelegate));
        } // HookWindowProc

        private void UnhookWindowProc()
        {
            if (_windowHandler.WindowHandle != IntPtr.Zero && _originalWndProc != IntPtr.Zero)
            {
                NativeMethods.SetWindowLongPtrInternal(_windowHandler.WindowHandle,
                    NativeMethods.GWLP_WNDPROC, _originalWndProc);
                _originalWndProc = IntPtr.Zero;
            }
        } // UnhookWindowProc

        private IntPtr WindowProc(IntPtr hWnd, uint msg, IntPtr wParam, IntPtr lParam)
        {
            // Обрабатываем сообщения от иконки трея
            if (msg == _iconData.uCallbackMessage) 
            {
                switch (checked((int)lParam))
                {
                    case NativeMethods.WM_LBUTTONDBLCLK:
                        DoubleClick?.Invoke(this, EventArgs.Empty);
                        return IntPtr.Zero;

                    case NativeMethods.WM_RBUTTONUP:
                        RightClick?.Invoke(this, EventArgs.Empty);
                        return IntPtr.Zero;
                }
            }

            // Для остальных сообщений вызываем стандартную процедуру
            if (_originalWndProc != IntPtr.Zero)
            {
                return NativeMethods.CallWindowProcInternal(_originalWndProc, hWnd, msg, wParam, lParam);
            }

            return NativeMethods.DefWindowProcInternal(hWnd, msg, wParam, lParam);
        } // WindowProc

        private void AddTrayIcon()
        {
            if (_iconAdded || _windowHandler.WindowHandle == IntPtr.Zero) return;

            try
            {
                bool success = NativeMethods.Shell_NotifyIconInternal(NativeMethods.NIM_ADD, ref _iconData);

                if (success)
                {
                    _iconAdded = true;
                    System.Diagnostics.Debug.WriteLine("Иконка успешно добавлена в трей");
                }
                else
                {
                    int error = Marshal.GetLastWin32Error();
                    System.Diagnostics.Debug.WriteLine($"Ошибка добавления иконки: {error}");
                    throw new InvalidOperationException($"Failed to add tray icon. Error code: {error}");
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Ошибка при добавлении иконки: {ex.Message}");
                throw;
            }
        } // AddTrayIcon

        private void LoadIcon()
        {
            try
            {
                // Пытаемся загрузить иконку из ресурсов
                var assembly = System.Reflection.Assembly.GetEntryAssembly();
                if (assembly != null)
                {
                    // Если есть иконка в ресурсах
                    using var iconStream = assembly.GetManifestResourceStream("StickyNotes.Resources.app.ico");
                    if (iconStream != null)
                    {
                        using var icon = new Icon(iconStream);
                        _iconData.hIcon = icon.Handle;
                        return;
                    }
                }

                // Или из файла
                string? exePath = System.Reflection.Assembly.GetExecutingAssembly().Location;
                if (!string.IsNullOrEmpty(exePath) && System.IO.File.Exists(exePath))
                {
                    var icon = Icon.ExtractAssociatedIcon(exePath);
                    if (icon != null)
                    {
                        _iconData.hIcon = icon.Handle;
                        return;
                    }
                }

                // Создаем простую иконку
                CreateDefaultIcon();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Ошибка загрузки иконки: {ex.Message}");
                CreateDefaultIcon();
            }
        } // LoadIcon

        private void CreateDefaultIcon()
        {
            try
            {
                // Создаем простую желтую иконку 16x16
                using var bitmap = new Bitmap(16, 16);
                using var graphics = Graphics.FromImage(bitmap);
                graphics.Clear(Color.Yellow);
                graphics.DrawRectangle(Pens.Black, 0, 0, 15, 15);

                using (var font = new Font("Arial", 8, FontStyle.Bold))
                using (var brush = new SolidBrush(Color.Black))
                {
                    graphics.DrawString("N", font, brush, 3, 2);
                }

                var icon = Icon.FromHandle(bitmap.GetHicon());
                _iconData.hIcon = icon.Handle;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Ошибка создания иконки: {ex.Message}");
                _iconData.hIcon = IntPtr.Zero;
            }
        } // CreateDefaultIcon

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
                    // Управляемые ресурсы
                }

                // Удаляем иконку из трея
                if (_iconAdded && _iconData.hWnd != IntPtr.Zero)
                {
                    try
                    {
                        NativeMethods.Shell_NotifyIconInternal(NativeMethods.NIM_DELETE, ref _iconData);
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine($"Ошибка удаления иконки: {ex.Message}");
                    }
                }

                // Восстанавливаем оригинальную оконную процедуру
                UnhookWindowProc();

                // Освобождаем ресурсы иконки
                if (_iconData.hIcon != IntPtr.Zero)
                {
                    try
                    {
                        NativeMethods.DestroyIconInternal(_iconData.hIcon);
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine($"Ошибка освобождения иконки: {ex.Message}");
                    }
                }

                _disposed = true;
            }
        } // Dispose

        ~TrayIconService()
        {
            Dispose(false);
        } // ~TrayIconService
    } // TrayIconService
} // namespace