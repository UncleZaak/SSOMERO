using System.Threading.Tasks;
using Microsoft.Maui.Controls;
using System;
using Microsoft.Maui;
using Microsoft.Extensions.DependencyInjection;
using Ssomero.Services;
using Ssomero.Interfaces;

namespace Ssomero.Views
{
    public partial class StartupPage : ContentPage
    {
        public StartupPage()
        {
            InitializeComponent();
            // Start the entrance animation once the page appears
            this.Appearing += StartupPage_Appearing;
        }

        private bool _navigated = false;

        private async void StartupPage_Appearing(object? sender, EventArgs e)
        {
            this.Appearing -= StartupPage_Appearing;
            // Fade-in sequence for logo and labels
            await Task.WhenAll(
                LogoImage.FadeTo(1, 450, Easing.CubicIn),
                TitleLabel.FadeTo(1, 550, Easing.CubicIn),
                SubtitleLabel.FadeTo(1, 650, Easing.CubicIn)
            );

            // While StartupPage is visible, perform authentication checks and role retrieval via DI-resolved services.
            bool isAuthenticated = false;
            string? role = null;

            var tokenService = IPlatformApplication.Current?.Services?.GetService<TokenStorageService>();
            var pollingService = IPlatformApplication.Current?.Services?.GetService<PollingService>();

            try
            {
                if (tokenService is not null)
                {
                    var token = await tokenService.GetAccessTokenAsync();
                    var isExpired = await tokenService.IsTokenExpiredAsync();

                    if (!string.IsNullOrEmpty(token) && !isExpired)
                    {
                        isAuthenticated = true;
                        role = await SecureStorage.Default.GetAsync("user_role");
                    }
                    else if (!string.IsNullOrEmpty(token))
                    {
                        // expired
                        isAuthenticated = false;
                        await tokenService.ClearAsync();
                        SecureStorage.Default.Remove("user_role");
                    }
                }
            }
            catch (Exception)
            {
                // If auth check fails, default to not authenticated and try to clear token
                isAuthenticated = false;
                try { if (tokenService is not null) await tokenService.ClearAsync(); } catch { }
                SecureStorage.Default.Remove("user_role");
            }

            // Wait remaining time so the startup screen displays ~2500ms total
            var remaining = 2500 - 700;
            if (remaining > 0)
                await Task.Delay(remaining);

            if (!_navigated)
            {
                _navigated = true;

                if (isAuthenticated && !string.IsNullOrEmpty(role))
                {
                    // Navigate directly to dashboard for the role. Use absolute route so StartupPage is removed from stack.
                    await Ssomero.Navigation.DashboardNavigator.GoToDashboardAsync(role);

                    // Start polling like earlier
                    try { pollingService?.Start(); } catch { }

                    // Populate top bar + flyout header identity state for auto-login users
                    try
                    {
                        var topBar = IPlatformApplication.Current?.Services?.GetService<ITopBarService>();
                        if (topBar is not null && !topBar.IsLoaded)
                            _ = topBar.LoadAsync();
                    }
                    catch { }
                }
                else
                {
                    // Not authenticated — navigate to LoginPage and ensure StartupPage removed from back stack.
                    await Shell.Current.GoToAsync("//LoginPage", animate: true);
                }
            }
        }
    }
}
