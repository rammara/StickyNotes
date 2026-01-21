using System;
using System.IO;
using System.Windows;
using System.Windows.Input;
using StickyNotes.Models;
using StickyNotes.Views;

namespace StickyNotes.ViewModels
{
    public class WindowNoteViewModel : ViewModelBase
    {
        private readonly WindowNote _window;
        private readonly SettingsModel _settings;
        private string _text = string.Empty;
        private string _filePath = string.Empty;
        private string _title = "New note...";
        private bool _isPinned = false;
        private bool _hasUnsavedChanges = false;
        private bool _isDragging = false;
        private Point _dragStartPoint;

        // Обновим конструктор WindowNoteViewModel
        public WindowNoteViewModel(WindowNote window, SettingsModel settings, int counter)
        {
            _window = window;
            _settings = settings;
            _title = $"New note {counter}...";

            // Устанавливаем шрифт из настроек
            ApplyFontSettings();

            InitializeCommands();
            LoadWindowPosition();
        } // WindowNoteViewModel

        private void InitializeCommands()
        {
            CloseCommand = new RelayCommand(CloseWindow);
            TogglePinCommand = new RelayCommand(TogglePin);
            SaveCommand = new RelayCommand(Save);
            CopyToClipboardCommand = new RelayCommand(CopyToClipboard);
        } // InitializeCommands

        public string Text
        {
            get => _text;
            set
            {
                if (_text != value)
                {
                    _text = value;
                    HasUnsavedChanges = true;
                    OnPropertyChanged();
                    OnPropertyChanged(nameof(Title));
                }
            } // set
        } // Text

        public string Title
        {
            get => (_hasUnsavedChanges ? "* " : "") + _title;
            set
            {
                _title = value;
                OnPropertyChanged();
            }
        } // Title

        public bool IsPinned
        {
            get => _isPinned;
            set
            {
                _isPinned = value;
                _window.Topmost = value;
                OnPropertyChanged();
            }
        } // IsPinned

        public bool HasUnsavedChanges
        {
            get => _hasUnsavedChanges;
            set
            {
                _hasUnsavedChanges = value;
                OnPropertyChanged(nameof(Title));
            }
        } // HasUnsavedChanges

        public ICommand CloseCommand { get; private set; } = null!;
        public ICommand TogglePinCommand { get; private set; } = null!;
        public ICommand SaveCommand { get; private set; } = null!;
        public ICommand CopyToClipboardCommand { get; private set; } = null!;
        
        private void CloseWindow()
        {
            _window.Close();
        } // CloseWindow

        private void TogglePin()
        {
            IsPinned = !IsPinned;
        } // TogglePin

        private void Save()
        {
            if (string.IsNullOrEmpty(_filePath))
            {
                SaveAs();
            }
            else
            {
                SaveToFile(_filePath);
            }
        } // Save

        private void SaveAs()
        {
            var dialog = new Microsoft.Win32.SaveFileDialog
            {
                InitialDirectory = _settings.DefaultSaveFolder,
                DefaultExt = ".txt",
                Filter = "Text files (*.txt)|*.txt|All files (*.*)|*.*",
                FileName = _title.TrimStart('*', ' ').Replace("...", ""),
                AddExtension = true,
                OverwritePrompt = true
            };

            if (dialog.ShowDialog() == true)
            {
                _filePath = dialog.FileName;
                SaveToFile(_filePath);
                Title = Path.GetFileName(_filePath);
            }
        } // SaveAs

        private void SaveToFile(string path)
        {
            try
            {
                File.WriteAllText(path, _text);
                HasUnsavedChanges = false;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error while saving the file: {ex.Message}", "Error",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        } // SaveToFile

        private void CopyToClipboard()
        {
            try
            {
                Clipboard.SetText(_text);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error while copying the data to the clipboard: {ex.Message}");
            }
        } // CopyToClipboard

        private void OnMouseLeftButtonDown(MouseButtonEventArgs e)
        {
            if (e.OriginalSource is FrameworkElement element &&
                element.Name == "TitleBar")
            {
                _isDragging = true;
                _dragStartPoint = e.GetPosition(_window);
                _window.CaptureMouse();
                e.Handled = true;
            }
            else
            {
                CopyToClipboard();
            }
        } // OnMouseLeftButtonDown

        private void OnMouseMove(MouseEventArgs e)
        {
            if (_isDragging && e.LeftButton == MouseButtonState.Pressed)
            {
                Point currentPosition = e.GetPosition(_window);
                Vector offset = currentPosition - _dragStartPoint;

                _window.Left += offset.X;
                _window.Top += offset.Y;

                SaveWindowPosition();
            }
        } // OnMouseMove

        private void OnMouseLeftButtonUp(MouseButtonEventArgs e)
        {
            if (_isDragging)
            {
                _isDragging = false;
                _window.ReleaseMouseCapture();
                SaveWindowPosition();
                e.Handled = true;
            }
        } // OnMouseLeftButtonUp

        private void LoadWindowPosition()
        {
            try
            {
                string appData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
                string positionFile = Path.Combine(appData, "StickyNotes", "window_position.json");

                if (File.Exists(positionFile))
                {
                    string json = File.ReadAllText(positionFile);
                    var position = System.Text.Json.JsonSerializer.Deserialize<WindowPosition>(json);

                    if (position != null)
                    {
                        _window.Left = position.Left;
                        _window.Top = position.Top;
                        _window.Width = position.Width;
                        _window.Height = position.Height;
                    }
                }
            }
            catch (Exception)
            {
                // Используем значения по умолчанию
            }
        } // LoadWindowPosition

        public void SaveWindowPosition()
        {
            try
            {
                string appData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
                string appFolder = Path.Combine(appData, "StickyNotes");
                Directory.CreateDirectory(appFolder);

                var position = new WindowPosition
                {
                    Left = _window.Left,
                    Top = _window.Top,
                    Width = _window.Width,
                    Height = _window.Height
                };

                string json = System.Text.Json.JsonSerializer.Serialize(position);
                File.WriteAllText(Path.Combine(appFolder, "window_position.json"), json);
            }
            catch (Exception)
            {
                // Игнорируем ошибки сохранения позиции
            }
        } // SaveWindowPosition
        
        private System.Windows.Media.FontFamily _fontFamily = new("Consolas");
        private double _fontSize = 12;
        private FontWeight _fontWeight = FontWeights.Normal;
        private FontStyle _fontStyle = FontStyles.Normal;

        public System.Windows.Media.FontFamily FontFamily
        {
            get => _fontFamily;
            set
            {
                _fontFamily = value;
                OnPropertyChanged();
            }
        } // FontFamily

        public double FontSize
        {
            get => _fontSize;
            set
            {
                _fontSize = value;
                OnPropertyChanged();
            }
        } // FontSize

        public System.Windows.FontWeight FontWeight
        {
            get => _fontWeight;
            set
            {
                _fontWeight = value;
                OnPropertyChanged();
            }
        } // FontWeight

        public System.Windows.FontStyle FontStyle
        {
            get => _fontStyle;
            set
            {
                _fontStyle = value;
                OnPropertyChanged();
            }
        } // FontStyle

        private void ApplyFontSettings()
        {
            FontFamily = new System.Windows.Media.FontFamily(_settings.DefaultFont.FontFamily);
            FontSize = _settings.DefaultFont.Size;
            FontWeight = _settings.DefaultFont.Style.HasFlag(System.Drawing.FontStyle.Bold)
                ? FontWeights.Bold : FontWeights.Normal;
            FontStyle = _settings.DefaultFont.Style.HasFlag(System.Drawing.FontStyle.Italic)
                ? FontStyles.Italic : FontStyles.Normal;
        } // ApplyFontSettings


    } // WindowNoteViewModel

    public class WindowPosition
    {
        public double Left { get; set; }
        public double Top { get; set; }
        public double Width { get; set; }
        public double Height { get; set; }
    } // WindowPosition
}