using StickyNotes.Services;
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
            ServiceProvider.RegisterService(new SettingsService());
            ServiceProvider.RegisterService(new GlobalHookService());
            _mainViewModel = new MainViewModel();
            // Background mode
            ShutdownMode = ShutdownMode.OnExplicitShutdown;
        } // OnStartup

        protected override void OnExit(ExitEventArgs e)
        {
            _mainViewModel?.Dispose();
            base.OnExit(e);
        } // OnExit
    } // App
} // namespace