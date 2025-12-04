using System;
using System.Windows.Input;
using StickyNotes.Models;

namespace StickyNotes.ViewModels
{
    public class HotkeyDialogViewModel : ViewModelBase
    {
        private Hotkey _selectedHotkey = new();

        public HotkeyDialogViewModel()
        {
            InitializeCommands();
        } // HotkeyDialogViewModel

        private void InitializeCommands()
        {
            SetCommand = new RelayCommand(SetHotkey);
            CancelCommand = new RelayCommand(Cancel);
            KeyDownCommand = new RelayCommand<KeyEventArgs>(OnKeyDown);
        } // InitializeCommands

        public Hotkey SelectedHotkey
        {
            get => _selectedHotkey;
            set
            {
                _selectedHotkey = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(HotkeyDisplay));
            }
        } // SelectedHotkey

        public string HotkeyDisplay => _selectedHotkey.ToString();

        public ICommand SetCommand { get; private set; } = null!;
        public ICommand CancelCommand { get; private set; } = null!;
        public ICommand KeyDownCommand { get; private set; } = null!;

        private void SetHotkey()
        {
            // Закрываем окно с DialogResult = true
            if (System.Windows.Application.Current.Windows.Count > 0)
            {
                foreach (System.Windows.Window window in System.Windows.Application.Current.Windows)
                {
                    if (window.DataContext == this)
                    {
                        window.DialogResult = true;
                        break;
                    }
                }
            }
        } // SetHotkey

        private void Cancel()
        {
            // Закрываем окно с DialogResult = false
            if (System.Windows.Application.Current.Windows.Count > 0)
            {
                foreach (System.Windows.Window window in System.Windows.Application.Current.Windows)
                {
                    if (window.DataContext == this)
                    {
                        window.DialogResult = false;
                        break;
                    }
                }
            }
        } // Cancel

        private void OnKeyDown(KeyEventArgs e)
        {
            // Игнорируем системные клавиши
            if (e.Key == Key.System || e.Key == Key.LeftCtrl || e.Key == Key.RightCtrl ||
                e.Key == Key.LeftShift || e.Key == Key.RightShift ||
                e.Key == Key.LeftAlt || e.Key == Key.RightAlt ||
                e.Key == Key.LWin || e.Key == Key.RWin)
            {
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

            e.Handled = true;
        } // OnKeyDown
    } // HotkeyDialogViewModel
}