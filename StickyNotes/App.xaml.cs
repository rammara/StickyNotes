using StickyNotes.Services;
using StickyNotes.ViewModels;
using System.Windows;

namespace StickyNotes
{
    public partial class App : Application
    {
        private MainViewModel? _mainViewModel;
        private static Mutex? _appMutex;
        private const string AppMutexName = "Global\\StickyNotesAppSingleInstanceMutex";

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
            _appMutex = new Mutex(true, AppMutexName, out bool isNewInstance);

            // Sending args through the named pipe to the existing instance
            if (!isNewInstance)
            {
                var sendTask = Task.Run(async () =>
                {
                    try
                    {
                        return await NamedPipeService.SendToExistingInstance(e.Args);
                    }
                    catch
                    {
                        return false;
                    }
                });

                var completedTask = Task.WhenAny(
                        sendTask,
                        Task.Delay(TimeSpan.FromMilliseconds(NamedPipeService.TIMEOUT))
                    ).GetAwaiter().GetResult();

                Current.Shutdown();
                Environment.Exit(0);
                return;
            }

            ServiceProvider.RegisterService(new SettingsService());
            ServiceProvider.RegisterService(new GlobalHookService());
            
            _mainViewModel = new MainViewModel(e.Args);
            // 
            var pipeSvc = new NamedPipeService();
            pipeSvc.DataReceived += (sender, receivedData) =>
            {
                Current.Dispatcher.Invoke(() =>
                {
                    _mainViewModel?.TryOpenNote(receivedData);
                });
            };
            pipeSvc.StartServer();
            ServiceProvider.RegisterService(pipeSvc);
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