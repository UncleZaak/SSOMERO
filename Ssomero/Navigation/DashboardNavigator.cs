using Microsoft.Maui.Controls;

namespace Ssomero.Navigation;

public static class DashboardNavigator
{
    public static async Task GoToDashboardAsync(string role)
    {
        var route = role switch
        {
            "Admin"               => "//AdminApp",
            "Lecturer"            => "//LecturerApp",
            "ClassRepresentative" => "//ClassRepApp",
            "ClassRep"            => "//ClassRepApp",
            _                     => "//StudentApp"
        };

        await Shell.Current.GoToAsync(route);

        // Rebuild the dynamic flyout for the authenticated role.
        if (Shell.Current is AppShell appShell)
            await appShell.RebuildFlyoutAsync(role);
    }
}

