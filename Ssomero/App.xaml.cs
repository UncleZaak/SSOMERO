using Microsoft.Extensions.Logging;
using Ssomero.Interfaces;
using Ssomero.Services;

namespace Ssomero
{
    public partial class App : Application
    {
        private readonly TokenStorageService _tokenStorage;
        private readonly PollingService _polling;
        private readonly INotificationService _notifications;
        private readonly ILogger<App> _logger;

        public App(TokenStorageService tokenStorage, PollingService polling)
            : this(tokenStorage, polling, null, null!)
        {
        }

        // Keep this signature for test backward-compatibility (tests pass ILogger as 3rd arg)
        public App(TokenStorageService tokenStorage, PollingService polling, ILogger<App> logger)
            : this(tokenStorage, polling, null, logger)
        {
        }

        public App(TokenStorageService tokenStorage, PollingService polling, INotificationService? notifications, ILogger<App>? logger)
        {
            InitializeComponent();
            _tokenStorage  = tokenStorage;
            _polling       = polling;
            _notifications = notifications;
            _logger        = logger!;

            // Global unhandled exception handlers
            AppDomain.CurrentDomain.UnhandledException += (s, e) =>
            {
                if (e.ExceptionObject is Exception ex)
                    _logger?.LogCritical(ex, "Unhandled AppDomain exception");
            };

            TaskScheduler.UnobservedTaskException += (s, e) =>
            {
                _logger?.LogError(e.Exception, "Unobserved task exception");
                e.SetObserved();
            };
        }

        protected override void OnResume()
        {
            base.OnResume();
            _polling.OnAppResumed();
        }

        protected override void OnSleep()
        {
            base.OnSleep();
            _polling.OnAppSleeping();
        }

        protected override Window CreateWindow(IActivationState? activationState)
        {
            var shell = new AppShell();
            var window = new Window(shell);

            shell.Loaded += async (s, e) =>
            {
                // Request notification permission early — safe if already granted or denied
                _ = _notifications?.RequestPermissionAsync();

                // Navigate to StartupPage as the first visible page after the native splash
                try
                {
                    await shell.GoToAsync("//StartupPage");
                }
                catch
                {
                    // ignore navigation errors and proceed — StartupPage may already be the default
                }
            };

            return window;
        }
    }
}