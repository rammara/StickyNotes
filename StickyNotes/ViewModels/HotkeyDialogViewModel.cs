using StickyNotes.Models;
using System.Windows;
using System.Windows.Input;

namespace StickyNotes.ViewModels
{
    public class HotkeyDialogViewModel : ViewModelBase
    {
        private Hotkey _selectedHotkey = new();

        private readonly Window _window;

        public HotkeyDialogViewModel(Window window)
        {
            ArgumentNullException.ThrowIfNull(window);
            _window = window;
            InitializeCommands();
        } // SetWindow

        private void SetHotkey()
        {
            _window.DialogResult = true;
            _window?.Close();
        } // SetHotkey

        private void Cancel()
        {
            _window.DialogResult = false;
            _window.Close();
        } // Cancel

        private void Reset()
        {
            _selectedHotkey = new();
            ComboSelected = false;
            OnPropertyChanged(nameof(HotkeyDisplay));
        } // Reset

        private void InitializeCommands()
        {
            SetCommand = new RelayCommand(SetHotkey);
            CancelCommand = new RelayCommand(Cancel);
            ResetCommand = new RelayCommand(Reset);
            KeyDownCommand = new RelayCommand<KeyEventArgs>(OnKeyDown);
        } // InitializeCommands

        public Hotkey SelectedHotkey
        {
            get => _selectedHotkey;
            set
            {
                _selectedHotkey = value;
                OnPropertyChanged(nameof(HotkeyDisplay));
            }
        } // SelectedHotkey

        public string HotkeyDisplay => _selectedHotkey.ToString();

        public ICommand SetCommand { get; private set; } = null!;
        public ICommand ResetCommand { get; private set; } = null;
        public ICommand CancelCommand { get; private set; } = null!;
        public ICommand KeyDownCommand { get; private set; } = null!;

        private bool _comboSelected = false;
        public bool ComboSelected 
        { 
            get => _comboSelected;
            set {
                if (value != _comboSelected)
                {
                    _comboSelected = value;
                    OnPropertyChanged(nameof(ComboSelected));
                }
            }
        } // ComboSelected

        public void OnKeyDown(KeyEventArgs e)
        {
            
            // Игнорируем системные клавиши
            if (e.Key == Key.System || e.Key == Key.LeftCtrl || e.Key == Key.RightCtrl ||
                e.Key == Key.LeftShift || e.Key == Key.RightShift ||
                e.Key == Key.LeftAlt || e.Key == Key.RightAlt ||
                e.Key == Key.LWin || e.Key == Key.RWin)
            {
                return;
            }

            if (ComboSelected)
            {
                if (Keyboard.IsKeyDown(Key.Return)) SetHotkey();
                if (Keyboard.IsKeyDown(Key.Escape)) Cancel();
                e.Handled = true;
                return;
            }

            HotkeyModifiers modifiers = HotkeyModifiers.None;

            if (Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl))
                modifiers |= HotkeyModifiers.Control;
            if (Keyboard.IsKeyDown(Key.LeftShift) || Keyboard.IsKeyDown(Key.RightShift))
                modifiers |= HotkeyModifiers.Shift;
            if (Keyboard.IsKeyDown(Key.LeftAlt) || Keyboard.IsKeyDown(Key.RightAlt))
                modifiers |= HotkeyModifiers.Alt;
            if (Keyboard.IsKeyDown(Key.LWin) || Keyboard.IsKeyDown(Key.RWin))
                modifiers |= HotkeyModifiers.Win;

            System.Windows.Forms.Keys key = (System.Windows.Forms.Keys)KeyInterop.VirtualKeyFromKey(e.Key);

            SelectedHotkey = new Hotkey()
            {
                Key = key,
                Modifiers = modifiers
            };

            ComboSelected = true;

            e.Handled = true;
        }
        // OnKeyDown
    } // HotkeyDialogViewModel
}