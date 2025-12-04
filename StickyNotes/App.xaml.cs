using StickyNotes;
using StikyNotes.Views;
using System.Windows;

namespace StikyNotes
{
    public partial class App : Application
    {
        private MainWindow? _mainWindow;

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            _mainWindow = new MainWindow();
            _mainWindow.Hide(); // Скрываем главное окно, показываем только иконку в трее

            // Запускаем приложение в фоновом режиме
            ShutdownMode = ShutdownMode.OnExplicitShutdown;
        } // OnStartup
    } // App
}