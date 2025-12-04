using StikyNotes.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace StickyNotes.Views
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private MainViewModel? _viewModel;

        public MainWindow()
        {
            InitializeComponent();
            Loaded += OnLoaded;
        } // MainWindow

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            _viewModel = new MainViewModel(this);
            DataContext = _viewModel;
        } // OnLoaded

        protected override void OnClosing(System.ComponentModel.CancelEventArgs e)
        {
            // Предотвращаем закрытие окна, приложение завершается через Exit
            e.Cancel = true;
            Hide();
        } // OnClosing
    }
}
