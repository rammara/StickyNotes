using StickyNotes.ViewModels;
using System.Windows;

namespace StickyNotes.Views
{
    public partial class MainWindow : Window
    {
        private MainViewModel? _viewModel;

        public MainWindow()
        {
            InitializeComponent();
        } // MainWindow

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            // Создаем ViewModel после загрузки окна
            _viewModel = new MainViewModel(this);
            DataContext = _viewModel;

            // Скрываем окно после инициализации трея
            Hide();
        } // Window_Loaded

        protected override void OnClosing(System.ComponentModel.CancelEventArgs e)
        {
            // Предотвращаем закрытие окна, приложение завершается через Exit
            e.Cancel = true;
            Hide();
        } // OnClosing
    } // MainWindow
} // namespace
