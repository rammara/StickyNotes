using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Input;

namespace StickyNotes.Services
{
    public class GlobalHookService : IDisposable
    {
        private IntPtr _hookId = IntPtr.Zero;
        private readonly NativeMethods.LowLevelKeyboardProc _proc;
        private bool _disposed = false;

        public event EventHandler<KeyEventArgs>? KeyDown;
        public event EventHandler<KeyEventArgs>? KeyUp;

        public GlobalHookService()
        {
            _proc = HookCallback;
            SetHook();
        } // GlobalHookService

        private void SetHook()
        {
            using Process curProcess = Process.GetCurrentProcess();
            using ProcessModule? curModule = curProcess.MainModule;
            if (curModule != null)
            {
                _hookId = NativeMethods.SetWindowsHookExInternal(NativeMethods.WH_KEYBOARD_LL, _proc,
                    NativeMethods.GetModuleHandleInternal(curModule.ModuleName), 0);
            }
        } // SetHook

        private IntPtr HookCallback(int nCode, IntPtr wParam, IntPtr lParam)
        {
            if (nCode >= 0)
            {
                int vkCode = Marshal.ReadInt32(lParam);
                Key key = KeyInterop.KeyFromVirtualKey(vkCode);

                if (wParam == (IntPtr)NativeMethods.WM_KEYDOWN || wParam == (IntPtr)NativeMethods.WM_SYSKEYDOWN)
                {
                    KeyDown?.Invoke(this, new KeyEventArgs(Keyboard.PrimaryDevice,
                        PresentationSource.FromVisual(null) ?? throw new InvalidOperationException(),
                        0, key));
                }
                else if (wParam == (IntPtr)NativeMethods.WM_KEYUP || wParam == (IntPtr)NativeMethods.WM_SYSKEYUP)
                {
                    KeyUp?.Invoke(this, new KeyEventArgs(Keyboard.PrimaryDevice,
                        PresentationSource.FromVisual(null) ?? throw new InvalidOperationException(),
                        0, key));
                }
            }

            return NativeMethods.CallNextHookExInternal(_hookId, nCode, wParam, lParam);
        } // HookCallback

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        } // Dispose

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (_hookId != IntPtr.Zero)
                {
                    NativeMethods.UnhookWindowsHookExInternal(_hookId);
                }
                _disposed = true;
            }
        } // Dispose

        ~GlobalHookService()
        {
            Dispose(false);
        } // ~GlobalHookService
    } // GlobalHookService
}