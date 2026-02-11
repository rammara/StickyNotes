using System.Windows.Input;

namespace StickyNotes.ViewModels
{
    public class RelayCommand(Action execute, Func<bool>? canExecute = null) : ICommand
    {
        private readonly Action _execute = execute ?? throw new ArgumentNullException(nameof(execute));

        public event EventHandler? CanExecuteChanged
        {
            add => CommandManager.RequerySuggested += value;
            remove => CommandManager.RequerySuggested -= value;
        } // CanExecuteChanged

        public bool CanExecute(object? parameter) => canExecute?.Invoke() ?? true;
        public void Execute(object? parameter) => _execute();
    } // RelayCommand

    public class RelayCommand<T>(Action<T> execute, Predicate<T>? canExecute = null) : ICommand
    {
        private readonly Action<T> _execute = execute ?? throw new ArgumentNullException(nameof(execute));

        public event EventHandler? CanExecuteChanged
        {
            add => CommandManager.RequerySuggested += value;
            remove => CommandManager.RequerySuggested -= value;
        } // CanExecuteChanged

        public bool CanExecute(object? parameter) =>
            canExecute?.Invoke((T)parameter!) ?? true;

        public void Execute(object? parameter)
        {
            if (parameter is T typedParam)
            {
                _execute(typedParam);
            }
        } // Execute
    } // RelayCommand<T>
} // namespace