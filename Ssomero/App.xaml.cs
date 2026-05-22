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

                try
                {
                    var token = await _tokenStorage.GetAccessTokenAsync();
                    var isExpired = await _tokenStorage.IsTokenExpiredAsync();

                    if (!string.IsNullOrEmpty(token) && !isExpired)
                    {
                        var role = await SecureStorage.Default.GetAsync("user_role");
                        if (!string.IsNullOrEmpty(role))
                        {
                            await Navigation.DashboardNavigator.GoToDashboardAsync(role);
                            _polling.Start(); // Begin background refresh after successful login

                            // Populate top bar + flyout header identity state for auto-login users
                            var topBar = IPlatformApplication.Current?.Services
                                             ?.GetService(typeof(ITopBarService)) as ITopBarService;
                            if (topBar is not null && !topBar.IsLoaded)
                                _ = topBar.LoadAsync();
                        }
                        else
                        {
                            await _tokenStorage.ClearAsync();
                            await shell.GoToAsync("//LoginPage");
                        }
                    }
                    else if (!string.IsNullOrEmpty(token))
                    {
                        _logger?.LogInformation("Startup token expired, clearing session");
                        await _tokenStorage.ClearAsync();
                        SecureStorage.Default.Remove("user_role");
                        await shell.GoToAsync("//LoginPage");
                    }
                }
                catch (Exception ex)
                {
                    _logger?.LogError(ex, "Auth check on startup failed");
                    await _tokenStorage.ClearAsync();
                    SecureStorage.Default.Remove("user_role");
                    await shell.GoToAsync("//LoginPage");
                }
            };

            return window;
        }
    }
}