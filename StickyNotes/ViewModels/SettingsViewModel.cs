using StickyNotes.Models;
using StickyNotes.Services;
using StickyNotes.Views;
using System.Windows;
using System.Windows.Input;

namespace StickyNotes.ViewModels
{
    public class SettingsViewModel : ViewModelBase
    {
        private readonly SettingsService _settingsService;
        private readonly SettingsModel _settings;
        private readonly SettingsWindow _window;
        //private HotkeyDialog? _hotkeyDialog;

        public SettingsViewModel(SettingsWindow window)
        {
            _settingsService = ServiceProvider.GetRequiredService<SettingsService>();
            _settings = _settingsService.LoadSettings();
            _window = window;
            InitializeCommands();
            UpdateFontProperties();
        } // SettingsViewModel


        private void InitializeCommands()
        {
            SaveCommand = new RelayCommand(SaveSettings);
            CancelCommand = new RelayCommand(Cancel);
            ChangeMainHotkeyCommand = new RelayCommand(ChangeMainHotkey);
            ChangeSaveHotkeyCommand = new RelayCommand(ChangeSaveHotkey);
            ChangeDefaultFolderCommand = new RelayCommand(ChangeDefaultFolder);
            OpenDefaultFolderCommand = new RelayCommand(OpenDefaultFolder);
            ChangeFontCommand = new RelayCommand(ChangeFont);
        } // InitializeCommands

        public DoubleClickAction DoubleClickAction
        {
            get => _settings.DoubleClickAction;
            set
            {
                _settings.DoubleClickAction = value;
                OnPropertyChanged();
            }
        } // DoubleClickAction

        public string MainHotkeyDisplay => _settings.MainHotkey.ToString();
        public string SaveHotkeyDisplay => _settings.SaveHotkey.ToString();

        public bool StartWithWindows
        {
            get => _settings.StartWithWindows;
            set
            {
                _settings.StartWithWindows = value;
                OnPropertyChanged();
            }
        } // StartWithWindows

        public string DefaultSaveFolder
        {
            get => _settings.DefaultSaveFolder;
            set
            {
                _settings.DefaultSaveFolder = value;
                OnPropertyChanged();
            }
        } // DefaultSaveFolder

        public string DefaultFontDisplay =>
            $"{_settings.DefaultFont.FontFamily}, {_settings.DefaultFont.Size}pt, {_settings.DefaultFont.Style}";

        public ICommand SaveCommand { get; private set; } = null!;
        public ICommand CancelCommand { get; private set; } = null!;
        public ICommand ChangeMainHotkeyCommand { get; private set; } = null!;
        public ICommand ChangeSaveHotkeyCommand { get; private set; } = null!;
        public ICommand ChangeDefaultFolderCommand { get; private set; } = null!;
        public ICommand OpenDefaultFolderCommand { get; private set; } = null!;
        public ICommand ChangeFontCommand { get; private set; } = null!;

        private void Cancel()
        {
            _window.Close();
        } // Cancel

        private void ChangeMainHotkey()
        {
            ShowHotkeyDialog(hotkey =>
            {
                _settings.MainHotkey = hotkey;
                OnPropertyChanged(nameof(MainHotkeyDisplay));
            });
        } // ChangeMainHotkey

        private void ChangeSaveHotkey()
        {
            ShowHotkeyDialog(hotkey =>
            {
                _settings.SaveHotkey = hotkey;
                OnPropertyChanged(nameof(SaveHotkeyDisplay));
            });
        } // ChangeSaveHotkey

        private static void ShowHotkeyDialog(Action<Hotkey> callback)
        {
            var _hotkeyDialog = new HotkeyDialog();
            var viewModel = new HotkeyDialogViewModel(_hotkeyDialog);
            _hotkeyDialog.DataContext = viewModel;

            var hookSvc = ServiceProvider.GetRequiredService<GlobalHookService>();
            hookSvc.PauseHook();
            _hotkeyDialog.ShowDialog();
            hookSvc.ResumeHook();
            if (_hotkeyDialog?.DialogResult == true)
            {
                callback(viewModel.SelectedHotkey);
            }
        } // ShowHotkeyDialog

        private void ChangeDefaultFolder()
        {
            var dialog = new System.Windows.Forms.FolderBrowserDialog
            {
                SelectedPath = DefaultSaveFolder
            };

            if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                DefaultSaveFolder = dialog.SelectedPath;
            }
        } // ChangeDefaultFolder

        private void OpenDefaultFolder()
        {
            try
            {
                if (!System.IO.Directory.Exists(DefaultSaveFolder))
                {
                    System.IO.Directory.CreateDirectory(DefaultSaveFolder);
                }
                System.Diagnostics.Process.Start("explorer.exe", DefaultSaveFolder);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error while opening the folder: {ex.Message}", "Error",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        } // OpenDefaultFolder

        // Добавим в SettingsViewModel.cs следующие свойства:

        private System.Windows.Media.FontFamily _selectedFontFamily = new("Consolas");
        private double _selectedFontSize = 12;
        private FontWeight _selectedFontWeight = FontWeights.Normal;
        private FontStyle _selectedFontStyle = FontStyles.Normal;

        public System.Windows.Media.FontFamily SelectedFontFamily
        {
            get => _selectedFontFamily;
            set
            {
                _selectedFontFamily = value;
                OnPropertyChanged();
            }
        } // SelectedFontFamily

        public double SelectedFontSize
        {
            get => _selectedFontSize;
            set
            {
                _selectedFontSize = value;
                OnPropertyChanged();
            }
        } // SelectedFontSize

        public FontWeight SelectedFontWeight
        {
            get => _selectedFontWeight;
            set
            {
                _selectedFontWeight = value;
                OnPropertyChanged();
            }
        } // SelectedFontWeight

        public FontStyle SelectedFontStyle
        {
            get => _selectedFontStyle;
            set
            {
                _selectedFontStyle = value;
                OnPropertyChanged();
            }
        } // SelectedFontStyle

        // Обновим метод ChangeFont в SettingsViewModel.cs
        private void ChangeFont()
        {
            // Создаем диалог выбора шрифта
            var fontDialog = new System.Windows.Forms.FontDialog
            {
                Font = _settings.DefaultFont.ToFont(),
                ShowEffects = true,
                ShowColor = false
            };

            if (fontDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                _settings.DefaultFont.FontFamily = fontDialog.Font.FontFamily.Name;
                _settings.DefaultFont.Size = fontDialog.Font.Size;
                _settings.DefaultFont.Style = fontDialog.Font.Style;

                // Обновляем свойства для привязки
                SelectedFontFamily = new System.Windows.Media.FontFamily(_settings.DefaultFont.FontFamily);
                SelectedFontSize = _settings.DefaultFont.Size;
                SelectedFontWeight = _settings.DefaultFont.Style.HasFlag(System.Drawing.FontStyle.Bold)
                    ? FontWeights.Bold : FontWeights.Normal;
                SelectedFontStyle = _settings.DefaultFont.Style.HasFlag(System.Drawing.FontStyle.Italic)
                    ? FontStyles.Italic : FontStyles.Normal;

                OnPropertyChanged(nameof(DefaultFontDisplay));
            }
        } // ChangeFont

        private void UpdateFontProperties()
        {
            SelectedFontFamily = new System.Windows.Media.FontFamily(_settings.DefaultFont.FontFamily);
            SelectedFontSize = _settings.DefaultFont.Size;
            SelectedFontWeight = _settings.DefaultFont.Style.HasFlag(System.Drawing.FontStyle.Bold)
                ? FontWeights.Bold : FontWeights.Normal;
            SelectedFontStyle = _settings.DefaultFont.Style.HasFlag(System.Drawing.FontStyle.Italic)
                ? FontStyles.Italic : FontStyles.Normal;
        } // UpdateFontProperties

        private void SaveSettings()
        {
            _settingsService.SaveSettings(_settings);
            _window.Close();
        } // SaveSettings

    } // SettingsViewModel
}