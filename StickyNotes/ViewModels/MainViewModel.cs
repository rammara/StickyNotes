using StickyNotes.Models;
using StickyNotes.Services;
using StickyNotes.Views;
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



        public MainViewModel(string[] args)
        {
            Debug.WriteLine("Creating MainViewModel...");

            _windowHandler = new NativeWindowHandler();
            _settingsService = ServiceProvider.GetRequiredService<SettingsService>();
            _settings = _settingsService.LoadSettings();

            Debug.WriteLine($"Settings loaded. Main hotkey: {_settings.MainHotkey}");

            _globalHookService = ServiceProvider.GetRequiredService<GlobalHookService>();
            _globalHookService.KeyDown += OnGlobalKeyDown;

            _trayIconService = new TrayIconService(_windowHandler);
            ServiceProvider.RegisterService(_trayIconService);
            _trayIconService.DoubleClick += OnTrayDoubleClick;
            _trayIconService.RightClick += OnTrayRightClick;

            EnsureDefaultSaveFolder();
            UpdateStartupRegistry();

            if (args is not null && args.Length > 0 && !string.IsNullOrEmpty(args[0]))
            {
                var path = Path.GetDirectoryName(args[0]);
                if (Directory.Exists(path))
                {
                    CreateNewNote(args[0]);
                }
            }

            Debug.WriteLine("MainViewModel created successfully");
        } // MainViewModel

        private void OnGlobalKeyDown(object? sender, GlobalKeyEventArgs e)
        {
            try
            {
                Debug.WriteLine($"Global hook: Key={e.Key}, Ctrl={e.CtrlPressed}, Shift={e.ShiftPressed}, Alt={e.AltPressed}, Win={e.WinPressed}");

                if (IsHotkeyPressed(e, _settings.MainHotkey))
                {
                    Debug.WriteLine($"Main hotkey detected: {_settings.MainHotkey}");

                    // Check if the focus is set on the textbox
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

        public void TryOpenNote(string path)
        {
            var dir = Path.GetDirectoryName(path);
            if (Directory.Exists(dir))
            {
                CreateNewNote(path);
            }
        } // TryOpenNote

        private static bool IsHotkeyPressed(GlobalKeyEventArgs e, Hotkey hotkey)
        {
            try
            {
                HotkeyModifiers pressedModifiers = HotkeyModifiers.None;
                if (e.CtrlPressed) pressedModifiers |= HotkeyModifiers.Control;
                if (e.ShiftPressed) pressedModifiers |= HotkeyModifiers.Shift;
                if (e.AltPressed) pressedModifiers |= HotkeyModifiers.Alt;
                if (e.WinPressed) pressedModifiers |= HotkeyModifiers.Win;

                // WPF Key to Windows Forms Keys
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
               
                contextMenu.IsOpen = true;
            });
        } // OnTrayRightClick
        
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

        public void CreateNewNote(string? fileName = null)
        {
            _noteCounter++;
            var noteWindow = new WindowNote();
            var viewModel = new WindowNoteViewModel(noteWindow, _settings, _noteCounter, fileName);
            noteWindow.DataContext = viewModel;

            noteWindow.Left = SystemParameters.WorkArea.Left + (_noteCounter * 30) % 500;
            noteWindow.Top = SystemParameters.WorkArea.Top + (_noteCounter * 30) % 300;
            noteWindow.Show();

            noteWindow.Dispatcher.BeginInvoke(new Action(() =>
            {
                WindowHelper.ForceForegroundWindow(noteWindow);

                // Focus on the text field if it exists
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
                var viewModel = new SettingsViewModel((SettingsWindow)_currentSettingsWindow);
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
                Debug.WriteLine($"Error creating folder: {ex.Message}");
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
                Debug.WriteLine($"Error while updating the registry: {ex.Message}");
            }
        } // UpdateStartupRegistry

        public static void ExitApplication()
        {
            ServiceProvider.Clear();
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
                    ServiceProvider.Clear();
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