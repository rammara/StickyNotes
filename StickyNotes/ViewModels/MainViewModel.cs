using StikyNotes.Models;
using StikyNotes.Services;
using StikyNotes.Views;
using System;
using System.IO;
using System.Windows;
using System.Windows.Input;

namespace StikyNotes.ViewModels
{
    public class MainViewModel : ViewModelBase
    {
        private readonly SettingsService _settingsService;
        private readonly GlobalHookService _globalHookService;
        private readonly TrayIconService _trayIconService;
        private SettingsModel _settings;
        private Window? _currentSettingsWindow;
        private int _noteCounter = 0;

        public MainViewModel(Window mainWindow)
        {
            _settingsService = new SettingsService();
            _settings = _settingsService.LoadSettings();

            _globalHookService = new GlobalHookService();
            _globalHookService.KeyDown += OnGlobalKeyDown;

            _trayIconService = new TrayIconService(mainWindow);
            _trayIconService.DoubleClick += OnTrayDoubleClick;
            _trayIconService.RightClick += OnTrayRightClick;

            EnsureDefaultSaveFolder();
            UpdateStartupRegistry();
        } // MainViewModel

        private void OnGlobalKeyDown(object? sender, KeyEventArgs e)
        {
            if (IsHotkeyPressed(e, _settings.MainHotkey))
            {
                Application.Current.Dispatcher.Invoke(() =>
                {
                    CreateNewNote();
                });
            }
        } // OnGlobalKeyDown

        private bool IsHotkeyPressed(KeyEventArgs e, Hotkey hotkey)
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

        private void OnTrayRightClick(object? sender, EventArgs e)
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
        } // OnTrayRightClick

        public void CreateNewNote()
        {
            _noteCounter++;
            var noteWindow = new WindowNote();
            var viewModel = new WindowNoteViewModel(noteWindow, _settings, _noteCounter);
            noteWindow.DataContext = viewModel;

            // Позиционируем окно со смещением
            noteWindow.Left = SystemParameters.WorkArea.Left + (_noteCounter * 30) % 500;
            noteWindow.Top = SystemParameters.WorkArea.Top + (_noteCounter * 30) % 300;

            noteWindow.Show();
        } // CreateNewNote

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
                using (var key = Microsoft.Win32.Registry.CurrentUser.OpenSubKey(registryPath, true))
                {
                    if (key != null)
                    {
                        string appPath = System.Diagnostics.Process.GetCurrentProcess().MainModule?.FileName ??
                            System.Reflection.Assembly.GetExecutingAssembly().Location;

                        if (_settings.StartWithWindows)
                        {
                            key.SetValue("StikyNotes", $"\"{appPath}\"");
                        }
                        else
                        {
                            key.DeleteValue("StikyNotes", false);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Ошибка обновления реестра: {ex.Message}");
            }
        } // UpdateStartupRegistry

        public void ExitApplication()
        {
            _globalHookService.Dispose();
            _trayIconService.Dispose();
            Application.Current.Shutdown();
        } // ExitApplication
    } // MainViewModel
}