using System.Windows;
using System.Windows.Input;

namespace StikyNotes.Views
{
    public partial class WindowNote : Window
    {
        public WindowNote()
        {
            InitializeComponent();
            Loaded += OnLoaded;
        } // WindowNote

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            NoteTextBox.Focus();
        } // OnLoaded

        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            var viewModel = DataContext as ViewModels.WindowNoteViewModel;
            if (viewModel == null) return;

            // Ctrl+C или Ctrl+Insert - копировать весь текст
            if ((e.Key == Key.C && Keyboard.Modifiers == ModifierKeys.Control) ||
                (e.Key == Key.Insert && Keyboard.Modifiers == ModifierKeys.Control))
            {
                viewModel.CopyToClipboardCommand.Execute(null);
                e.Handled = true;
            }
            // Ctrl+S или F2 - сохранить
            else if ((e.Key == Key.S && Keyboard.Modifiers == ModifierKeys.Control) ||
                     e.Key == Key.F2)
            {
                viewModel.SaveCommand.Execute(null);
                e.Handled = true;
            }
        } // Window_KeyDown

        protected override void OnLocationChanged(System.EventArgs e)
        {
            base.OnLocationChanged(e);
            var viewModel = DataContext as ViewModels.WindowNoteViewModel;
            viewModel?.SaveWindowPosition();
        } // OnLocationChanged

        protected override void OnRenderSizeChanged(SizeChangedInfo sizeInfo)
        {
            base.OnRenderSizeChanged(sizeInfo);
            var viewModel = DataContext as ViewModels.WindowNoteViewModel;
            viewModel?.SaveWindowPosition();
        } // OnRenderSizeChanged
    } // WindowNote
}