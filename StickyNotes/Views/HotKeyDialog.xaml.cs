using StickyNotes.ViewModels;
using System.Windows;

namespace StickyNotes.Views
{
    public partial class HotkeyDialog : Window
    {
        public HotkeyDialog()
        {
            InitializeComponent();
        } // HotkeyDialog

        private void Window_PreviewKeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (DataContext is HotkeyDialogViewModel viewModel)
            {
                viewModel.OnKeyDown(e);
            }
        } // Window_PreviewKeyDown
    } // HotkeyDialog
}