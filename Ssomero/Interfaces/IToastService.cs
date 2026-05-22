namespace Ssomero.Interfaces;

/// <summary>
/// Lightweight, non-blocking in-app toast notification service.
/// Toasts auto-dismiss after 2–3 seconds and never block the UI thread.
/// </summary>
public interface IToastService
{
    /// <summary>Show a green success toast.</summary>
    Task ShowSuccessAsync(string message);

    /// <summary>Show a red error toast.</summary>
    Task ShowErrorAsync(string message);

    /// <summary>Show a blue info toast.</summary>
    Task ShowInfoAsync(string message);
}
