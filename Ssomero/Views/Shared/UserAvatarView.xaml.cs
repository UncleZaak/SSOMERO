using Microsoft.Maui.Controls.Shapes;

namespace Ssomero.Views.Shared;

/// <summary>
/// Circular avatar that shows either a profile photo or role-coloured initials.
/// All state driven from bindable properties; no BindingContext dependency.
/// </summary>
public partial class UserAvatarView : ContentView
{
    // ── Bindable properties ───────────────────────────────────────────────

    public static readonly BindableProperty SizeProperty =
        BindableProperty.Create(nameof(Size), typeof(double), typeof(UserAvatarView), 44.0,
            propertyChanged: (b, _, _) => ((UserAvatarView)b).UpdateVisuals());

    public static readonly BindableProperty InitialsProperty =
        BindableProperty.Create(nameof(Initials), typeof(string), typeof(UserAvatarView), "S",
            propertyChanged: (b, _, _) => ((UserAvatarView)b).UpdateVisuals());

    public static readonly BindableProperty HasPhotoProperty =
        BindableProperty.Create(nameof(HasPhoto), typeof(bool), typeof(UserAvatarView), false,
            propertyChanged: (b, _, _) => ((UserAvatarView)b).UpdateVisuals());

    public static readonly BindableProperty PhotoUrlProperty =
        BindableProperty.Create(nameof(PhotoUrl), typeof(string), typeof(UserAvatarView), null,
            propertyChanged: (b, _, _) => ((UserAvatarView)b).UpdateVisuals());

    public static readonly BindableProperty UserRoleProperty =
        BindableProperty.Create(nameof(UserRole), typeof(string), typeof(UserAvatarView), string.Empty,
            propertyChanged: (b, _, _) => ((UserAvatarView)b).UpdateVisuals());

    public double Size     { get => (double)GetValue(SizeProperty);     set => SetValue(SizeProperty, value); }
    public string Initials { get => (string)GetValue(InitialsProperty); set => SetValue(InitialsProperty, value); }
    public bool   HasPhoto { get => (bool)GetValue(HasPhotoProperty);   set => SetValue(HasPhotoProperty, value); }
    public string? PhotoUrl { get => (string?)GetValue(PhotoUrlProperty); set => SetValue(PhotoUrlProperty, value); }
    public string UserRole { get => (string)GetValue(UserRoleProperty); set => SetValue(UserRoleProperty, value); }

    public UserAvatarView()
    {
        InitializeComponent();
        UpdateVisuals();
    }

    // ── Visual update ─────────────────────────────────────────────────────

    private void UpdateVisuals()
    {
        var size   = Size;
        var radius = size / 2.0;
        var shape  = new RoundRectangle { CornerRadius = new CornerRadius(radius) };

        // Container size
            RootGrid.WidthRequest  = size;
        RootGrid.HeightRequest = size;

        // Initials border
        InitialsBorder.StrokeShape       = new RoundRectangle { CornerRadius = new CornerRadius(radius) };
        InitialsBorder.BackgroundColor   = RoleColor(UserRole);
        InitialsBorder.IsVisible         = !HasPhoto;
        InitialsLabel.Text               = Initials;
        InitialsLabel.FontSize           = size * 0.38;

        // Photo border
        PhotoBorder.StrokeShape = new RoundRectangle { CornerRadius = new CornerRadius(radius) };
        PhotoBorder.IsVisible   = HasPhoto;

        if (HasPhoto && !string.IsNullOrWhiteSpace(PhotoUrl))
            PhotoImage.Source = ImageSource.FromUri(new Uri(PhotoUrl));
        else
            PhotoImage.Source = null;

        _ = shape; // suppress CS0219
    }

    // ── Role colour map ───────────────────────────────────────────────────

    private static Color RoleColor(string role) => role?.ToLowerInvariant() switch
    {
        "admin"                         => Color.FromArgb("#DC2626"),
        "lecturer"                      => Color.FromArgb("#2563EB"),
        "classrepresentative"
        or "classrep"                   => Color.FromArgb("#059669"),
        _                               => Color.FromArgb("#7C3AED"),   // Student + default
    };
}
