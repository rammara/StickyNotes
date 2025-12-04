using System;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;

namespace StikyNotes.Services
{
    public class TrayIconService : IDisposable
    {
        private readonly Window _window;
        private readonly NativeMethods.NOTIFYICONDATA _iconData;
        private bool _disposed = false;

        public event EventHandler? DoubleClick;
        public event EventHandler? RightClick;

        public TrayIconService(Window window)
        {
            _window = window;
            _iconData = new NativeMethods.NOTIFYICONDATA();
            Initialize();
        } // TrayIconService

        private void Initialize()
        {
            _window.SourceInitialized += OnSourceInitialized;

            // Создаем иконку
            var icon = System.Drawing.Icon.ExtractAssociatedIcon(System.Diagnostics.Process.GetCurrentProcess().MainModule?.FileName ??
                System.Reflection.Assembly.GetExecutingAssembly().Location);

            _iconData.cbSize = Marshal.SizeOf(_iconData);
            _iconData.hWnd = new WindowInteropHelper(_window).Handle;
            _iconData.uID = 0;
            _iconData.uFlags = NativeMethods.NIF_MESSAGE | NativeMethods.NIF_ICON | NativeMethods.NIF_TIP;
            _iconData.uCallbackMessage = NativeMethods.WM_USER + 1;
            _iconData.hIcon = icon?.Handle ?? IntPtr.Zero;
            _iconData.szTip = "StikyNotes";

            NativeMethods.Shell_NotifyIcon(NativeMethods.NIM_ADD, ref _iconData);

            ComponentDispatcher.ThreadPreprocessMessage += ComponentDispatcher_ThreadPreprocessMessage;
        } // Initialize

        private void OnSourceInitialized(object? sender, EventArgs e)
        {
            _iconData.hWnd = new WindowInteropHelper(_window).Handle;
            NativeMethods.Shell_NotifyIcon(NativeMethods.NIM_MODIFY, ref _iconData);
        } // OnSourceInitialized

        private void ComponentDispatcher_ThreadPreprocessMessage(ref MSG msg, ref bool handled)
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
                    ComponentDispatcher.ThreadPreprocessMessage -= ComponentDispatcher_ThreadPreprocessMessage;
                }

                _iconData.uFlags = 0;
                NativeMethods.Shell_NotifyIcon(NativeMethods.NIM_DELETE, ref _iconData);

                if (_iconData.hIcon != IntPtr.Zero)
                {
                    NativeMethods.DestroyIcon(_iconData.hIcon);
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
}