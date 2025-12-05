using StickyNotes.Models;
using StickyNotes.Services;
using StickyNotes.Views;
using System;
using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;


namespace StickyNotes.ViewModels
{
    public class MainViewModel : ViewModelBase, IDisposable
    {
        private readonly SettingsService _settingsService;
        private readonly GlobalHookService _globalHookService;
        private readonly TrayIconService _trayIconService;
        private readonly NativeWindowHandler _windowHandler;
        private SettingsModel _settings;
        private Window? _currentSettingsWindow;
        private int _noteCounter = 0;
        private bool _disposed = false;

        public MainViewModel()
        {
            Debug.WriteLine("Creating MainViewModel...");

            // Создаем нативное окно
            _windowHandler = new NativeWindowHandler();

            _settingsService = new SettingsService();
            _settings = _settingsService.LoadSettings();

            Debug.WriteLine($"Settings loaded. Main hotkey: {_settings.MainHotkey}");

            _globalHookService = new GlobalHookService();
            _globalHookService.KeyDown += OnGlobalKeyDown;

            // Создаем сервис иконки трея
            _trayIconService = new TrayIconService(_windowHandler);
            _trayIconService.DoubleClick += OnTrayDoubleClick;
            _trayIconService.RightClick += OnTrayRightClick;

            EnsureDefaultSaveFolder();
            UpdateStartupRegistry();

            Debug.WriteLine("MainViewModel created successfully");
        } // MainViewModel

        private void OnGlobalKeyDown(object? sender, GlobalKeyEventArgs e)
        {
            try
            {
                Debug.WriteLine($"Global hook: Key={e.Key}, Ctrl={e.CtrlPressed}, Shift={e.ShiftPressed}, Alt={e.AltPressed}, Win={e.WinPressed}");

                // Проверяем, является ли это сочетанием клавиш
                if (IsHotkeyPressed(e, _settings.MainHotkey))
                {
                    Debug.WriteLine($"Main hotkey detected: {_settings.MainHotkey}");

                    // Проверяем, не находится ли фокус в текстовом поле
                    bool hasKeyboardFocusOnTextBox = Application.Current.Dispatcher.Invoke(() =>
                    {
                        var focusedElement = Keyboard.FocusedElement;
                        return focusedElement is System.Windows.Controls.TextBox;
                    });

                    if (!hasKeyboardFocusOnTextBox)
                    {
                        Debug.WriteLine("Creating new note...");

                        Application.Current.Dispatcher.BeginInvoke(new Action(() =>
                        {
                            try
                            {
                                CreateNewNote();
                            }
                            catch (Exception ex)
                            {
                                Debug.WriteLine($"Error creating note: {ex.Message}");
                            }
                        }), DispatcherPriority.Normal);
                    }
                    else
                    {
                        Debug.WriteLine("Ignoring hotkey - focus is in text box");
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error in OnGlobalKeyDown: {ex.Message}");
            }
        } // OnGlobalKeyDown

        private static bool IsHotkeyPressed(GlobalKeyEventArgs e, Hotkey hotkey)
        {
            try
            {
                HotkeyModifiers pressedModifiers = HotkeyModifiers.None;
                if (e.CtrlPressed) pressedModifiers |= HotkeyModifiers.Control;
                if (e.ShiftPressed) pressedModifiers |= HotkeyModifiers.Shift;
                if (e.AltPressed) pressedModifiers |= HotkeyModifiers.Alt;
                if (e.WinPressed) pressedModifiers |= HotkeyModifiers.Win;

                // Преобразуем WPF Key в Windows Forms Keys
                int vkCode = KeyInterop.VirtualKeyFromKey(e.Key);
                System.Windows.Forms.Keys formsKey = (System.Windows.Forms.Keys)vkCode;

                bool result = formsKey == hotkey.Key && pressedModifiers == hotkey.Modifiers;

                if (result)
                {
                    Debug.WriteLine($"Hotkey match! Key={formsKey}, Modifiers={pressedModifiers}");
                }

                return result;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error in IsHotkeyPressed: {ex.Message}");
                return false;
            }
        } // IsHotkeyPressed

        private void OnTrayRightClick(object? sender, EventArgs e)
        {
            // Создаем контекстное меню в UI потоке
            Application.Current.Dispatcher.Invoke(() =>
            {
                var contextMenu = new System.Windows.Controls.ContextMenu();

                var newNoteItem = new System.Windows.Controls.MenuItem { Header = "New note..." };
                newNoteItem.Click += (s, args) => CreateNewNote();
                contextMenu.Items.Add(newNoteItem);

                var settingsItem = new System.Windows.Controls.MenuItem { Header = "Settings" };
                settingsItem.Click += (s, args) => ShowSettings();
                contextMenu.Items.Add(settingsItem);

                contextMenu.Items.Add(new System.Windows.Controls.Separator());

                var exitItem = new System.Windows.Controls.MenuItem { Header = "Exit" };
                exitItem.Click += (s, args) => ExitApplication();
                contextMenu.Items.Add(exitItem);

                // Показываем меню
                contextMenu.IsOpen = true;
            });
        } // OnTrayRightClick

        // Добавим метод для проверки горячей клавиши сохранения в окне заметки
        public bool IsSaveHotkeyPressed(KeyEventArgs e)
        {
            try
            {
                return IsHotkeyPressed(e, _settings.SaveHotkey);
            }
            catch
            {
                return false;
            }
        } // IsSaveHotkeyPressed

        private static bool IsHotkeyPressed(KeyEventArgs e, Hotkey hotkey)
        {
            bool ctrlPressed = Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl);
            bool shiftPressed = Keyboard.IsKeyDown(Key.LeftShift) || Keyboard.IsKeyDown(Key.RightShift);
            bool altPressed = Keyboard.IsKeyDown(Key.LeftAlt) || Keyboard.IsKeyDown(Key.RightAlt);
            bool winPressed = Keyboard.IsKeyDown(Key.LWin) || Keyboard.IsKeyDown(Key.RWin);

            HotkeyModifiers pressedModifiers = HotkeyModifiers.None;
            if (ctrlPressed) pressedModifiers |= HotkeyModifiers.Control;
            if (shiftPressed) pressedModifiers |= HotkeyModifiers.Shift;
            if (altPressed) pressedModifiers |= HotkeyModifiers.Alt;
            if (winPressed) pressedModifiers |= HotkeyModifiers.Win;

            Key key = e.Key == Key.System ? e.SystemKey : e.Key;
            System.Windows.Forms.Keys formsKey = (System.Windows.Forms.Keys)KeyInterop.VirtualKeyFromKey(key);

            return formsKey == hotkey.Key && pressedModifiers == hotkey.Modifiers;
        } // IsHotkeyPressed

        private void OnTrayDoubleClick(object? sender, EventArgs e)
        {
            if (_settings.DoubleClickAction == DoubleClickAction.EditWindow)
            {
                CreateNewNote();
            }
            else
            {
                ShowSettings();
            }
        } // OnTrayDoubleClick

        //public void CreateNewNote()
        //{
        //    _noteCounter++;
        //    var noteWindow = new WindowNote();
        //    var viewModel = new WindowNoteViewModel(noteWindow, _settings, _noteCounter);
        //    noteWindow.DataContext = viewModel;

        //    // Позиционируем окно со смещением
        //    noteWindow.Left = SystemParameters.WorkArea.Left + (_noteCounter * 30) % 500;
        //    noteWindow.Top = SystemParameters.WorkArea.Top + (_noteCounter * 30) % 300;

        //    noteWindow.Show();
        //} // CreateNewNote

        // Обновленный метод CreateNewNote
        public void CreateNewNote()
        {
            _noteCounter++;
            var noteWindow = new WindowNote();
            var viewModel = new WindowNoteViewModel(noteWindow, _settings, _noteCounter);
            noteWindow.DataContext = viewModel;

            // Позиционируем окно со смещением
            noteWindow.Left = SystemParameters.WorkArea.Left + (_noteCounter * 30) % 500;
            noteWindow.Top = SystemParameters.WorkArea.Top + (_noteCounter * 30) % 300;

            // Показываем окно
            noteWindow.Show();

            // Принудительно активируем и выводим на передний план
            noteWindow.Dispatcher.BeginInvoke(new Action(() =>
            {
                WindowHelper.ForceForegroundWindow(noteWindow);

                // Фокусируемся на текстовом поле, если оно есть
                if (noteWindow is WindowNote wn && wn.FindName("NoteTextBox") is System.Windows.Controls.TextBox textBox)
                {
                    textBox.Focus();
                }
            }), DispatcherPriority.Background);
        }



        public void ShowSettings()
        {
            if (_currentSettingsWindow == null || !_currentSettingsWindow.IsVisible)
            {
                _currentSettingsWindow = new SettingsWindow();
                var viewModel = new SettingsViewModel(_settingsService);
                _currentSettingsWindow.DataContext = viewModel;
                _currentSettingsWindow.Closed += (s, args) =>
                {
                    _currentSettingsWindow = null;
                    _settings = _settingsService.LoadSettings();
                    UpdateStartupRegistry();
                };
                _currentSettingsWindow.Show();
            }
            else
            {
                _currentSettingsWindow.Activate();
            }
        } // ShowSettings

        private void EnsureDefaultSaveFolder()
        {
            try
            {
                if (!Directory.Exists(_settings.DefaultSaveFolder))
                {
                    Directory.CreateDirectory(_settings.DefaultSaveFolder);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Ошибка создания папки: {ex.Message}");
            }
        } // EnsureDefaultSaveFolder

        private void UpdateStartupRegistry()
        {
            try
            {
                const string registryPath = @"SOFTWARE\Microsoft\Windows\CurrentVersion\Run";
                using var key = Microsoft.Win32.Registry.CurrentUser.OpenSubKey(registryPath, true);
                if (key != null)
                {
                    string appPath = System.Diagnostics.Process.GetCurrentProcess().MainModule?.FileName ??
                        System.Reflection.Assembly.GetExecutingAssembly().Location;

                    if (_settings.StartWithWindows)
                    {
                        key.SetValue("StickyNotes", $"\"{appPath}\"");
                    }
                    else
                    {
                        key.DeleteValue("StickyNotes", false);
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Ошибка обновления реестра: {ex.Message}");
            }
        } // UpdateStartupRegistry

        public void ExitApplication()
        {
            _globalHookService.Dispose();
            _trayIconService.Dispose();
            Application.Current.Shutdown();
        } // ExitApplication

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
                    _globalHookService.Dispose();
                    _trayIconService.Dispose();
                    _windowHandler.Dispose();
                }
                _disposed = true;
            }
        } // Dispose

        ~MainViewModel()
        {
            Dispose(false);
        } // ~MainViewModel

    } // MainViewModel
} // namespace