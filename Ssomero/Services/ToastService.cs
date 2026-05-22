using Microsoft.Extensions.Logging;
using Microsoft.Maui.Controls.Shapes;
using Ssomero.Interfaces;

namespace Ssomero.Services;

/// <summary>
/// Overlay-based, non-blocking toast. Injects a floating Border into
/// the current page's root Grid then auto-dismisses.
/// </summary>
public class ToastService : IToastService
{
    private readonly ILogger<ToastService> _logger;

    public ToastService(ILogger<ToastService> logger) => _logger = logger;

    public Task ShowSuccessAsync(string message) => ShowAsync(message, "#22C55E");
    public Task ShowErrorAsync(string message)   => ShowAsync(message, "#EF4444");
    public Task ShowInfoAsync(string message)    => ShowAsync(message, "#3B82F6");

    private Task ShowAsync(string message, string colorHex, int durationMs = 2800)
    {
        _logger.LogInformation("[Toast] {Message}", message);

        return MainThread.InvokeOnMainThreadAsync(async () =>
        {
            var page = Shell.Current?.CurrentPage as ContentPage;
            if (page is null) return;

            var toast = new Border
            {
                BackgroundColor = Color.FromArgb(colorHex),
                StrokeShape = new RoundRectangle { CornerRadius = new CornerRadius(14) },
                Stroke = Colors.Transparent,
                Padding = new Thickness(20, 12),
                HorizontalOptions = LayoutOptions.Center,
                VerticalOptions = LayoutOptions.End,
                Margin = new Thickness(24, 0, 24, 56),
                Opacity = 0,
                ZIndex = 9999,
                Content = new Label
                {
                    Text = message,
                    TextColor = Colors.White,
                    FontSize = 14,
                    HorizontalTextAlignment = TextAlignment.Center
                }
            };

            // Inject into root Grid (works for both Grid and Grid-wrapped pages)
            Grid? root = page.Content as Grid;
            if (root is null) return;

            Grid.SetRowSpan(toast, Math.Max(root.RowDefinitions.Count, 1));
            Grid.SetColumnSpan(toast, Math.Max(root.ColumnDefinitions.Count, 1));
            root.Add(toast);

            // Animate in
            await Task.WhenAll(
                toast.FadeTo(1, 200, Easing.CubicOut),
                toast.TranslateTo(0, -8, 220, Easing.CubicOut));

            await Task.Delay(durationMs);

            await toast.FadeTo(0, 200, Easing.CubicIn);
            root.Remove(toast);
        });
    }
}
