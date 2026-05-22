using Microsoft.Maui.Graphics;

namespace Ssomero.Navigation;

/// <summary>
/// Describes a single item in the role-based Shell flyout menu.
/// </summary>
public sealed class NavigationItemDefinition
{
    /// <summary>Display label shown in the flyout.</summary>
    public string Title { get; init; } = string.Empty;

    /// <summary>Shell absolute route, e.g. "//StudentApp/DashboardPage". Empty for logout.</summary>
    public string Route { get; init; } = string.Empty;

    /// <summary>Emoji / icon displayed to the left of the title.</summary>
    public string Icon { get; init; } = string.Empty;

    /// <summary>When true this item triggers logout instead of navigation.</summary>
    public bool IsLogout { get; init; }

    /// <summary>When true this item renders as a thin horizontal divider, not a tappable row.</summary>
    public bool IsSeparator { get; init; }

    /// <summary>Derived text colour: red for logout, dark-slate for everything else.</summary>
    public Color TextColor => IsLogout
        ? Color.FromArgb("#EF4444")
        : Color.FromArgb("#0F172A");
}
