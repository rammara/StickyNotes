// Класс аргументов для глобальных событий клавиатуры
using System.Windows.Input;

namespace StickyNotes.Services
{
    public class GlobalKeyEventArgs(Key key, bool ctrlPressed, bool shiftPressed, bool altPressed, bool winPresed) : EventArgs
    {
        public Key Key { get; } = key;
        public bool CtrlPressed { get; } = ctrlPressed;
        public bool ShiftPressed { get; } = shiftPressed;
        public bool AltPressed { get; } = altPressed;
        public bool WinPressed { get; } = winPresed;
    } // GlobalKeyEventArgs
}
