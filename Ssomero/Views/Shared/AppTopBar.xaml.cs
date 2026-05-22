using Microsoft.Extensions.DependencyInjection;
using Ssomero.Interfaces;

namespace Ssomero.Views.Shared;

/// <summary>
/// Shared top bar ContentView used by all four role dashboards.
/// Resolves <see cref="ITopBarService"/> from DI and keeps avatar/name
/// in sync with the authenticated user's identity state.
/// </summary>
public partial class AppTopBar : ContentView
{
    private readonly ITopBarService _topBarService;

    // ── Bindable properties (set from the hosting page) ───────────────────

    public static readonly BindableProperty TitleProperty =
        BindableProperty.Create(nameof(Title), typeof(string), typeof(AppTopBar), string.Empty,
            propertyChanged: (b, _, _) => ((AppTopBar)b).ApplyTitle());

    public static readonly BindableProperty SubtitleProperty =
        BindableProperty.Create(nameof(Subtitle), typeof(string), typeof(AppTopBar), string.Empty,
            propertyChanged: (b, _, _) => ((AppTopBar)b).ApplySubtitle());

    public static readonly BindableProperty ShowHamburgerProperty =
        BindableProperty.Create(nameof(ShowHamburger), typeof(bool), typeof(AppTopBar), true,
            propertyChanged: (b, _, _) => ((AppTopBar)b).ApplyHamburger());

    public static readonly BindableProperty ShowNotificationBellProperty =
        BindableProperty.Create(nameof(ShowNotificationBell), typeof(bool), typeof(AppTopBar), true,
            propertyChanged: (b, _, _) => ((AppTopBar)b).ApplyBell());

    public string Title   { get => (string)GetValue(TitleProperty);   set => SetValue(TitleProperty, value); }
    public string Subtitle { get => (string)GetValue(SubtitleProperty); set => SetValue(SubtitleProperty, value); }
    public bool ShowHamburger      { get => (bool)GetValue(ShowHamburgerProperty);      set => SetValue(ShowHamburgerProperty, value); }
    public bool ShowNotificationBell { get => (bool)GetValue(ShowNotificationBellProperty); set => SetValue(ShowNotificationBellProperty, value); }

    public AppTopBar()
    {
        _topBarService = IPlatformApplication.Current!.Services
                             .GetRequiredService<ITopBarService>();

        InitializeComponent();
        ApplyPlatformPadding();
        SyncFromTopBarService();

        _topBarService.ProfileChanged += (_, _) =>
            MainThread.BeginInvokeOnMainThread(SyncFromTopBarService);
    }

    // ── TopBarService → visual sync ───────────────────────────────────────

    private void SyncFromTopBarService()
    {
        AvatarView.Initials = _topBarService.Initials;
        AvatarView.HasPhoto = _topBarService.HasPhoto;
        AvatarView.PhotoUrl = _topBarService.PhotoUrlWithVersion;
        AvatarView.UserRole = _topBarService.Role;
    }

    // ── Property change handlers ──────────────────────────────────────────

    private void ApplyTitle()    => TitleLabel.Text   = Title;

    private void ApplySubtitle()
    {
        SubtitleLabel.Text      = Subtitle;
        SubtitleLabel.IsVisible = !string.IsNullOrWhiteSpace(Subtitle);
    }

    private void ApplyHamburger() => HamburgerButton.IsVisible = ShowHamburger;
    private void ApplyBell()      => BellButton.IsVisible      = ShowNotificationBell;

    // ── Safe-area padding ─────────────────────────────────────────────────

    private void ApplyPlatformPadding()
    {
        // Pages using Shell.NavBarIsVisible="False" go edge-to-edge.
        // Add a top offset so content clears the device status bar.
        Thickness padding;

#if ANDROID
        padding = new Thickness(16, 36, 8, 12);
#elif IOS
        padding = new Thickness(16, 44, 8, 12);
#else
        padding = new Thickness(16, 12, 8, 12);
#endif
        RootGrid.Padding = padding;
    }

    // ── Interaction handlers ──────────────────────────────────────────────

    private void OnHamburgerClicked(object? sender, EventArgs e)
    {
        if (Shell.Current is { } shell)
            shell.FlyoutIsPresented = true;
    }

    private async void OnBellClicked(object? sender, EventArgs e)
    {
        try { await Shell.Current.GoToAsync("notifications"); }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"AppTopBar: bell nav failed — {ex.Message}");
        }
    }

    private async void OnAvatarTapped(object? sender, TappedEventArgs e)
    {
        var route = _topBarService.Role?.ToLowerInvariant() switch
        {
            "admin"                               => "//AdminApp/AdminProfile",
            "lecturer"                            => "//LecturerApp/LecturerProfile",
            "classrepresentative" or "classrep"   => "//ClassRepApp/ClassRepProfile",
            _                                     => "//StudentApp/StudentProfile",
        };

        try { await Shell.Current.GoToAsync(route); }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"AppTopBar: avatar nav failed — {ex.Message}");
        }
    }
}
