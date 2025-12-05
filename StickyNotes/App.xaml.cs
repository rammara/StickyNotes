using StickyNotes.ViewModels;
using System.Windows;

namespace StickyNotes
{
    public partial class App : Application
    {
        private MainViewModel? _mainViewModel;

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
            _mainViewModel = new MainViewModel();
            // Запускаем приложение в фоновом режиме
            ShutdownMode = ShutdownMode.OnExplicitShutdown;
        } // OnStartup

        protected override void OnExit(ExitEventArgs e)
        {
            _mainViewModel?.Dispose();
            base.OnExit(e);
        } // OnExit
    } // App
} // namespace