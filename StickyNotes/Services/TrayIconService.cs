using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;

namespace StickyNotes.Services
{
    public class TrayIconService : IDisposable
    {
        private readonly Window _window;
        private NativeMethods.NOTIFYICONDATA _iconData;
        private bool _disposed = false;
        private bool _iconAdded = false;

        public event EventHandler? DoubleClick;
        public event EventHandler? RightClick;

        public TrayIconService(Window window)
        {
            _window = window;
            Initialize();
        } // TrayIconService

        private void Initialize()
        {
            try
            {
                // Ждем, пока окно будет полностью инициализировано
                _window.Loaded += OnWindowLoaded;
                _window.SourceInitialized += OnSourceInitialized;

                // Создаем и инициализируем структуру иконки
                _iconData = new NativeMethods.NOTIFYICONDATA
                {
                    cbSize = Marshal.SizeOf(typeof(NativeMethods.NOTIFYICONDATA)),
                    uID = 0,
                    uFlags = NativeMethods.NIF_MESSAGE | NativeMethods.NIF_ICON | NativeMethods.NIF_TIP,
                    uCallbackMessage = NativeMethods.WM_USER + 1,
                    szTip = "StickyNotes",
                    hWnd = IntPtr.Zero
                };

                // Загружаем иконку
                LoadIcon();

                ComponentDispatcher.ThreadPreprocessMessage += ComponentDispatcher_ThreadPreprocessMessage;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Ошибка инициализации TrayIconService: {ex.Message}");
            }
        } // Initialize

        private void ComponentDispatcher_ThreadPreprocessMessage(ref System.Windows.Interop.MSG msg, ref bool handled)
        {
            if (msg.message == _iconData.uCallbackMessage)
            {
                switch (msg.lParam.ToInt32())
                {
                    case NativeMethods.WM_LBUTTONDBLCLK:
                        DoubleClick?.Invoke(this, EventArgs.Empty);
                        break;
                    case NativeMethods.WM_RBUTTONUP:
                        RightClick?.Invoke(this, EventArgs.Empty);
                        break;
                }
            }
        } // ComponentDispatcher_ThreadPreprocessMessage

        private void OnWindowLoaded(object sender, RoutedEventArgs e)
        {
            // Окно загружено, пробуем добавить иконку
            TryAddIcon();
        } // OnWindowLoaded

        private void OnSourceInitialized(object? sender, EventArgs e)
        {
            // Окно получило handle, пробуем добавить иконку
            TryAddIcon();
        } // OnSourceInitialized

        private void TryAddIcon()
        {
            if (_iconAdded) return;

            try
            {
                // Получаем handle окна
                var windowHelper = new WindowInteropHelper(_window);
                if (windowHelper.Handle == IntPtr.Zero)
                {
                    // Handle еще не готов, пробуем позже
                    System.Diagnostics.Debug.WriteLine("Handle окна еще не готов");
                    return;
                }

                _iconData.hWnd = windowHelper.Handle;

                // Добавляем иконку в трей
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
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Ошибка при добавлении иконки: {ex.Message}");
            }
        } // TryAddIcon

        private void LoadIcon()
        {
            try
            {
                // Пробуем получить иконку из исполняемого файла
                string? exePath = System.Reflection.Assembly.GetEntryAssembly()?.Location;
                if (!string.IsNullOrEmpty(exePath) && System.IO.File.Exists(exePath))
                {
                    var icon = Icon.ExtractAssociatedIcon(exePath);
                    if (icon != null)
                    {
                        _iconData.hIcon = icon.Handle;
                        return;
                    }
                }

                // Альтернативный способ - создать простую иконку
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
                // Заливаем желтым цветом
                graphics.Clear(Color.FromArgb(255, 255, 220, 120));

                // Рисуем черную рамку
                graphics.DrawRectangle(Pens.Black, 0, 0, 15, 15);

                // Рисуем букву "N" (Note)
                using (var font = new Font(new FontFamily(System.Drawing.Text.GenericFontFamilies.Serif), 8, System.Drawing.FontStyle.Bold))
                using (var brush = new SolidBrush(Color.Black))
                {
                    graphics.DrawString("N", font, brush, 3, 2);
                }

                // Получаем handle иконки
                var icon = Icon.FromHandle(bitmap.GetHicon());
                _iconData.hIcon = icon.Handle;

                // Важно: не освобождаем ресурсы здесь, они понадобятся для иконки
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Ошибка создания иконки: {ex.Message}");
                _iconData.hIcon = IntPtr.Zero;
            }
        } // CreateDefaultIcon


        public void ShowBalloonTip(string title, string message, int timeout = 1000)
        {
            if (!_iconAdded || _iconData.hWnd == IntPtr.Zero) return;

            try
            {
                var notifyData = new NativeMethods.NOTIFYICONDATA
                {
                    cbSize = Marshal.SizeOf(typeof(NativeMethods.NOTIFYICONDATA)),
                    hWnd = _iconData.hWnd,
                    uID = _iconData.uID,
                    uFlags = NativeMethods.NIF_INFO,
                    szInfoTitle = title,
                    szInfo = message,
                    uTimeoutOrVersion = timeout,
                    dwInfoFlags = 1 // NIIF_INFO
                };

                NativeMethods.Shell_NotifyIconInternal(NativeMethods.NIM_MODIFY, ref notifyData);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Ошибка показа уведомления: {ex.Message}");
            }
        } // ShowBalloonTip

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
                    // Отписываемся от событий
                    _window.Loaded -= OnWindowLoaded;
                    _window.SourceInitialized -= OnSourceInitialized;
                    ComponentDispatcher.ThreadPreprocessMessage -= ComponentDispatcher_ThreadPreprocessMessage;
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

    [StructLayout(LayoutKind.Sequential)]
    public struct MSG
    {
        public IntPtr hwnd;
        public int message;
        public IntPtr wParam;
        public IntPtr lParam;
        public int time;
        public int pt_x;
        public int pt_y;
    } // MSG
} // namespace