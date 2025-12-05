using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Windows.Input;
using System.Windows.Threading;

namespace StickyNotes.Services
{
    public class GlobalHookService : IDisposable
    {
        private IntPtr _hookId = IntPtr.Zero;
        private readonly NativeMethods.LowLevelKeyboardProc _proc;
        private bool _disposed = false;
        private readonly Dispatcher _dispatcher;

        public event EventHandler<GlobalKeyEventArgs>? KeyDown;
        public event EventHandler<GlobalKeyEventArgs>? KeyUp;

        public GlobalHookService()
        {
            // Сохраняем диспетчер UI-потока для безопасного вызова событий
            _dispatcher = Dispatcher.CurrentDispatcher;
            _proc = HookCallback;
            SetHook();
        } // GlobalHookService

        private void SetHook()
        {
            try
            {
                using Process curProcess = Process.GetCurrentProcess();
                using ProcessModule? curModule = curProcess.MainModule;
                if (curModule != null)
                {
                    _hookId = NativeMethods.SetWindowsHookExInternal(NativeMethods.WH_KEYBOARD_LL, _proc,
                        NativeMethods.GetModuleHandleInternal(curModule.ModuleName), 0);

                    if (_hookId == IntPtr.Zero)
                    {
                        int error = Marshal.GetLastWin32Error();
                        Debug.WriteLine($"Failed to set hook. Error: {error}");
                        throw new InvalidOperationException($"Failed to set keyboard hook. Error code: {error}");
                    }

                    Debug.WriteLine($"Hook set successfully. Handle: {_hookId}");
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error setting hook: {ex.Message}");
                throw;
            }
        } // SetHook

        private IntPtr HookCallback(int nCode, IntPtr wParam, IntPtr lParam)
        {
            try
            {
                // Если nCode < 0, то мы должны передать сообщение дальше без обработки
                if (nCode < 0)
                {
                    return NativeMethods.CallNextHookExInternal(_hookId, nCode, wParam, lParam);
                }

                // Получаем информацию о нажатой клавише
                int vkCode = Marshal.ReadInt32(lParam);
                Key key = KeyInterop.KeyFromVirtualKey(vkCode);

                // Получаем состояние модификаторов
                bool ctrlPressed = (NativeMethods.GetAsyncKeyStateInternal(0x11) & 0x8000) != 0;
                bool shiftPressed = (NativeMethods.GetAsyncKeyStateInternal(0x10) & 0x8000) != 0;
                bool altPressed = (NativeMethods.GetAsyncKeyStateInternal(0x12) & 0x8000) != 0;
                bool winPressed = (NativeMethods.GetAsyncKeyStateInternal(0x5B) & 0x8000) != 0 ||
                                  (NativeMethods.GetAsyncKeyStateInternal(0x5C) & 0x8000) != 0;

                // Игнорируем системные клавиши при обработке хоткеев
                if (key == Key.LeftCtrl || key == Key.RightCtrl ||
                    key == Key.LeftShift || key == Key.RightShift ||
                    key == Key.LeftAlt || key == Key.RightAlt ||
                    key == Key.LWin || key == Key.RWin)
                {
                    return NativeMethods.CallNextHookExInternal(_hookId, nCode, wParam, lParam);
                }

                var args = new GlobalKeyEventArgs(key, ctrlPressed, shiftPressed, altPressed, winPressed);

                // Асинхронно обрабатываем событие в UI потоке
                if (wParam == (IntPtr)NativeMethods.WM_KEYDOWN || wParam == (IntPtr)NativeMethods.WM_SYSKEYDOWN)
                {
                    _dispatcher.BeginInvoke(new Action(() =>
                    {
                        try
                        {
                            KeyDown?.Invoke(this, args);
                        }
                        catch (Exception ex)
                        {
                            Debug.WriteLine($"Error in KeyDown event: {ex.Message}");
                        }
                    }), DispatcherPriority.Background, null);
                }
                else if (wParam == (IntPtr)NativeMethods.WM_KEYUP || wParam == (IntPtr)NativeMethods.WM_SYSKEYUP)
                {
                    _dispatcher.BeginInvoke(new Action(() =>
                    {
                        try
                        {
                            KeyUp?.Invoke(this, args);
                        }
                        catch (Exception ex)
                        {
                            Debug.WriteLine($"Error in KeyUp event: {ex.Message}");
                        }
                    }), DispatcherPriority.Background, null);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error in HookCallback: {ex.Message}");
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
                    try
                    {
                        bool result = NativeMethods.UnhookWindowsHookExInternal(_hookId);
                        if (!result)
                        {
                            int error = Marshal.GetLastWin32Error();
                            Debug.WriteLine($"Failed to unhook. Error: {error}");
                        }
                        else
                        {
                            Debug.WriteLine("Hook successfully unhooked");
                        }
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine($"Error unhooking: {ex.Message}");
                    }
                    _hookId = IntPtr.Zero;
                }
                _disposed = true;
            }
        } // Dispose

        ~GlobalHookService()
        {
            Dispose(false);
        } // ~GlobalHookService
    } // GlobalHookService
} // namespace