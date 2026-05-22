namespace Ssomero.Navigation;

/// <summary>
/// Returns the ordered list of flyout navigation items for a given authenticated role.
/// This is the single source of truth for app navigation structure.
/// </summary>
public interface INavigationDefinitionService
{
    /// <summary>
    /// Returns the flyout items that should be displayed for <paramref name="role"/>.
    /// </summary>
    /// <param name="role">Authenticated role string (e.g. "Admin", "Lecturer", "Student").</param>
    IReadOnlyList<NavigationItemDefinition> GetItemsForRole(string role);
}
