using System.Drawing;
using System.IO;
using System.Runtime.InteropServices;

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
                System.Diagnostics.Debug.WriteLine($"Error initializing the TrayIconService: {ex.Message}");
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
                    System.Diagnostics.Debug.WriteLine("Icon successfully added to the system tray.");
                }
                else
                {
                    int error = Marshal.GetLastWin32Error();
                    System.Diagnostics.Debug.WriteLine($"Error adding the icon to the system tray: {error}");
                    throw new InvalidOperationException($"Failed to add tray icon. Error code: {error}");
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error while adding the icon: {ex.Message}");
                throw;
            }
        } // AddTrayIcon

        const int iconSize = 32;
        private void LoadIcon()
        {
            try
            {
                var assembly = System.Reflection.Assembly.GetEntryAssembly();
                if (assembly != null)
                {
                    var iconBytes = Properties.Resources.Note_Icon;
                    using var ms = new MemoryStream(iconBytes);
                    using var originalIcon = new Icon(ms);
                    using var bitmap = new Bitmap(iconSize, iconSize);
                    using (var g = Graphics.FromImage(bitmap))
                    {
                        g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
                        g.DrawIcon(originalIcon, new Rectangle(0, 0, iconSize, iconSize));
                    }

                    IntPtr hIcon = bitmap.GetHicon();
                    _iconData.hIcon = hIcon;
                    return;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error loading icon: {ex.Message}");
            }
        } // LoadIcon

      

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
                        System.Diagnostics.Debug.WriteLine($"Error deleting the icon: {ex.Message}");
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
                        System.Diagnostics.Debug.WriteLine($"Error releasing the icon: {ex.Message}");
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