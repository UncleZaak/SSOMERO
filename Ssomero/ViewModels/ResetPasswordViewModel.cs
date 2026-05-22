using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Input;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Logging;
using Microsoft.Maui.Controls;
using Ssomero.Interfaces;

namespace Ssomero.ViewModels;

[QueryProperty(nameof(Email),      "email")]
[QueryProperty(nameof(ResetToken), "resetToken")]
public partial class ResetPasswordViewModel : BaseViewModel
{
    private readonly IAuthService _auth;
    private readonly ILogger<ResetPasswordViewModel> _logger;

    public ResetPasswordViewModel(IAuthService auth, ILogger<ResetPasswordViewModel> logger)
    {
        _auth   = auth;
        _logger = logger;

        ResetPasswordCommand              = new AsyncRelayCommand(ResetPasswordAsync);
        TogglePasswordCommand             = new Command(() => IsPasswordHidden        = !IsPasswordHidden);
        ToggleConfirmPasswordCommand      = new Command(() => IsConfirmPasswordHidden = !IsConfirmPasswordHidden);
    }

    // ── Commands ──────────────────────────────────────────────────────────────
    public AsyncRelayCommand ResetPasswordCommand         { get; }
    public ICommand          TogglePasswordCommand        { get; }
    public ICommand          ToggleConfirmPasswordCommand { get; }

    // ── Query-property fields (from navigation) ───────────────────────────────
    // resetToken is in memory only — never logged, displayed or persisted
    private string _resetToken = string.Empty;
    public string ResetToken
    {
        get => _resetToken;
        set => _resetToken = Uri.UnescapeDataString(value ?? string.Empty);
    }

    string _email = string.Empty;
    public string Email
    {
        get => _email;
        set => _email = Uri.UnescapeDataString(value ?? string.Empty);
    }

    // ── Fields ────────────────────────────────────────────────────────────────
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

    // ── Password visibility ───────────────────────────────────────────────────
    bool isPasswordHidden = true;
    public bool IsPasswordHidden
    {
        get => isPasswordHidden;
        set
        {
            if (SetProperty(ref isPasswordHidden, value))
                RaisePropertyChanged(nameof(PasswordToggleIcon));
        }
    }
    public string PasswordToggleIcon => IsPasswordHidden ? "\U0001F441" : "\U0001F648";

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
    public string ConfirmPasswordToggleIcon => IsConfirmPasswordHidden ? "\U0001F441" : "\U0001F648";

    // ── Password strength ─────────────────────────────────────────────────────
    public string PasswordStrengthHint => NewPassword.Length switch
    {
        0              => string.Empty,
        < 8            => "Too short (min 8 characters)",
        _ when IsStrongPassword(NewPassword) => "Strong password ✓",
        _ when NewPassword.Length >= 8       => "Add numbers or symbols for a stronger password"
    };

    public Color PasswordStrengthColor => NewPassword.Length switch
    {
        0              => Colors.Transparent,
        < 8            => Color.FromArgb("#EF4444"),
        _ when IsStrongPassword(NewPassword) => Color.FromArgb("#22C55E"),
        _              => Color.FromArgb("#F59E0B")
    };

    public bool DoPasswordsMatch =>
        !string.IsNullOrEmpty(NewPassword) &&
        !string.IsNullOrEmpty(ConfirmPassword) &&
        NewPassword == ConfirmPassword;

    // ── Action ────────────────────────────────────────────────────────────────
    private async Task ResetPasswordAsync()
    {
        ErrorMessage = string.Empty;

        if (string.IsNullOrWhiteSpace(NewPassword) || string.IsNullOrWhiteSpace(ConfirmPassword))
        {
            ErrorMessage = "Please fill in both password fields.";
            return;
        }

        if (NewPassword.Length < 8)
        {
            ErrorMessage = "Password must be at least 8 characters.";
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
            await _auth.ResetPasswordAsync(_email, _resetToken, NewPassword);

            // Clear sensitive data from memory immediately after use
            _resetToken      = string.Empty;
            NewPassword      = string.Empty;
            ConfirmPassword  = string.Empty;

            await ShowSuccessToastAsync("Password reset successful!");
            await Task.Delay(1200); // brief pause so toast is visible
            await Shell.Current.GoToAsync("//LoginPage");
        }
        catch (HttpRequestException ex)
        {
            _logger.LogWarning("ResetPassword failed for {Email}: {Message}", _email, ex.Message);
            ErrorMessage = ex.Message;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error during ResetPassword for {Email}", _email);
            ErrorMessage = "Something went wrong. Please try again.";
        }
        finally
        {
            IsBusy = false;
        }
    }

    private static bool IsStrongPassword(string pwd) =>
        pwd.Length >= 8 &&
        Regex.IsMatch(pwd, @"[A-Z]") &&
        Regex.IsMatch(pwd, @"[0-9]") &&
        Regex.IsMatch(pwd, @"[^a-zA-Z0-9]");
}
