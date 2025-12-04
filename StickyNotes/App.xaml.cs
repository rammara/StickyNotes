using StickyNotes.Views;
using System.Windows;

namespace StickyNotes
{
    public partial class App : Application
    {
        private MainWindow? _mainWindow;

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            _mainWindow = new MainWindow();
            // Запускаем приложение в фоновом режиме
            ShutdownMode = ShutdownMode.OnExplicitShutdown;
        } // OnStartup
    } // App
} // namespace