using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Input;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Logging;
using Microsoft.Maui.Controls;
using Ssomero.Interfaces;
using Ssomero.Models;

namespace Ssomero.ViewModels;

public class ChangePasswordViewModel : BaseViewModel
{
    private readonly IProfileService _profileService;
    private readonly IAuthService _authService;
    private readonly ILogger<ChangePasswordViewModel> _logger;

    public ChangePasswordViewModel(
        IProfileService profileService,
        IAuthService authService,
        ILogger<ChangePasswordViewModel> logger)
    {
        _profileService = profileService;
        _authService    = authService;
        _logger         = logger;

        ChangePasswordCommand             = new AsyncRelayCommand(ChangePasswordAsync);
        ToggleCurrentPasswordCommand      = new Command(() => IsCurrentPasswordHidden = !IsCurrentPasswordHidden);
        ToggleNewPasswordCommand          = new Command(() => IsNewPasswordHidden      = !IsNewPasswordHidden);
        ToggleConfirmPasswordCommand      = new Command(() => IsConfirmPasswordHidden  = !IsConfirmPasswordHidden);
        GoBackCommand                     = new Command(async () => await Shell.Current.GoToAsync(".."));
    }

    // ── Commands ─────────────────────────────────────────────────────────────
    public AsyncRelayCommand ChangePasswordCommand        { get; }
    public ICommand          ToggleCurrentPasswordCommand { get; }
    public ICommand          ToggleNewPasswordCommand     { get; }
    public ICommand          ToggleConfirmPasswordCommand { get; }
    public ICommand          GoBackCommand                { get; }

    // ── Fields ───────────────────────────────────────────────────────────────
    string currentPassword = string.Empty;
    public string CurrentPassword
    {
        get => currentPassword;
        set => SetProperty(ref currentPassword, value);
    }

    string newPassword = string.Empty;
    public string NewPassword
    {
        get => newPassword;
        set
        {
            if (SetProperty(ref newPassword, value))
            {
                RaisePropertyChanged(nameof(PasswordStrengthHint));
                RaisePropertyChanged(nameof(PasswordStrengthColor));
                RaisePropertyChanged(nameof(DoPasswordsMatch));
            }
        }
    }

    string confirmPassword = string.Empty;
    public string ConfirmPassword
    {
        get => confirmPassword;
        set
        {
            if (SetProperty(ref confirmPassword, value))
                RaisePropertyChanged(nameof(DoPasswordsMatch));
        }
    }

    string errorMessage = string.Empty;
    public string ErrorMessage { get => errorMessage; set => SetProperty(ref errorMessage, value); }

    // ── Visibility toggles ────────────────────────────────────────────────────
    bool isCurrentPasswordHidden = true;
    public bool IsCurrentPasswordHidden
    {
        get => isCurrentPasswordHidden;
        set
        {
            if (SetProperty(ref isCurrentPasswordHidden, value))
                RaisePropertyChanged(nameof(CurrentPasswordToggleIcon));
        }
    }
    public string CurrentPasswordToggleIcon => IsCurrentPasswordHidden ? "👁" : "🙈";

    bool isNewPasswordHidden = true;
    public bool IsNewPasswordHidden
    {
        get => isNewPasswordHidden;
        set
        {
            if (SetProperty(ref isNewPasswordHidden, value))
                RaisePropertyChanged(nameof(NewPasswordToggleIcon));
        }
    }
    public string NewPasswordToggleIcon => IsNewPasswordHidden ? "👁" : "🙈";

    bool isConfirmPasswordHidden = true;
    public bool IsConfirmPasswordHidden
    {
        get => isConfirmPasswordHidden;
        set
        {
            if (SetProperty(ref isConfirmPasswordHidden, value))
                RaisePropertyChanged(nameof(ConfirmPasswordToggleIcon));
        }
    }
    public string ConfirmPasswordToggleIcon => IsConfirmPasswordHidden ? "👁" : "🙈";

    // ── Strength & match ─────────────────────────────────────────────────────
    public string PasswordStrengthHint => NewPassword.Length switch
    {
        0           => string.Empty,
        < 8         => "Too short (min 8 characters)",
        _ when IsStrong(NewPassword) => "Strong password ✓",
        _           => "Add uppercase, number & symbol for a stronger password"
    };

    public Color PasswordStrengthColor => NewPassword.Length switch
    {
        0           => Colors.Transparent,
        < 8         => Color.FromArgb("#EF4444"),
        _ when IsStrong(NewPassword) => Color.FromArgb("#22C55E"),
        _           => Color.FromArgb("#F59E0B")
    };

    public bool DoPasswordsMatch =>
        !string.IsNullOrEmpty(NewPassword) &&
        !string.IsNullOrEmpty(ConfirmPassword) &&
        NewPassword == ConfirmPassword;

    // ── Action ────────────────────────────────────────────────────────────────
    private async Task ChangePasswordAsync()
    {
        ErrorMessage = string.Empty;

        if (string.IsNullOrWhiteSpace(CurrentPassword))
        {
            ErrorMessage = "Please enter your current password.";
            return;
        }
        if (NewPassword.Length < 8)
        {
            ErrorMessage = "New password must be at least 8 characters.";
            return;
        }
        if (NewPassword != ConfirmPassword)
        {
            ErrorMessage = "Passwords do not match.";
            return;
        }

        IsBusy = true;
        try
        {
            var ct  = CreateLinkedToken();
            var err = await _profileService.ChangePasswordAsync(
                new ChangePasswordRequest
                {
                    CurrentPassword = CurrentPassword,
                    NewPassword     = NewPassword
                }, ct);

            if (err is not null)
            {
                ErrorMessage = err;
                return;
            }

            // Clear sensitive fields immediately
            CurrentPassword = string.Empty;
            NewPassword     = string.Empty;
            ConfirmPassword = string.Empty;

            await ShowSuccessToastAsync("Password changed! Please log in again.");
            await Task.Delay(1500);

            // Force re-login after a password change for security
            await _authService.LogoutAsync();
        }
        catch (OperationCanceledException) { }
        catch (Exception ex)
        {
            _logger.LogError(ex, "ChangePasswordAsync failed");
            ErrorMessage = "Something went wrong. Please try again.";
        }
        finally { IsBusy = false; }
    }

    private static bool IsStrong(string p) =>
        p.Length >= 8 &&
        Regex.IsMatch(p, @"[A-Z]") &&
        Regex.IsMatch(p, @"[0-9]") &&
        Regex.IsMatch(p, @"[^a-zA-Z0-9]");
}
