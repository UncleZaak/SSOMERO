using System.Net.Http;
using System.Threading.Tasks;
using System.Windows.Input;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Logging;
using Microsoft.Maui.Controls;
using Ssomero.Interfaces;

namespace Ssomero.ViewModels;

public class ForgotPasswordViewModel : BaseViewModel
{
    private readonly IAuthService _auth;
    private readonly ILogger<ForgotPasswordViewModel> _logger;
    private IDispatcherTimer? _cooldownTimer;
    private int _otpCooldownSeconds;

    public ForgotPasswordViewModel(IAuthService auth, ILogger<ForgotPasswordViewModel> logger)
    {
        _auth = auth;
        _logger = logger;

        SendOtpCommand   = new AsyncRelayCommand(SendOtpAsync);
        VerifyOtpCommand = new AsyncRelayCommand(VerifyOtpAsync);
        ResendOtpCommand = new AsyncRelayCommand(ResendOtpAsync, () => CanResendOtp);
        GoBackCommand    = new Command(async () => await Shell.Current.GoToAsync(".."));
    }

    // ── Commands ─────────────────────────────────────────────────────────────
    public AsyncRelayCommand SendOtpCommand   { get; }
    public AsyncRelayCommand VerifyOtpCommand { get; }
    public AsyncRelayCommand ResendOtpCommand { get; }
    public ICommand          GoBackCommand    { get; }

    // ── Step ─────────────────────────────────────────────────────────────────
    int currentStep = 1;
    public int CurrentStep
    {
        get => currentStep;
        set
        {
            if (SetProperty(ref currentStep, value))
            {
                RaisePropertyChanged(nameof(IsStep1));
                RaisePropertyChanged(nameof(IsStep2));
            }
        }
    }
    public bool IsStep1 => CurrentStep == 1;
    public bool IsStep2 => CurrentStep == 2;

    // ── Fields ───────────────────────────────────────────────────────────────
    string email = string.Empty;
    public string Email { get => email; set => SetProperty(ref email, value); }

    string otpCode = string.Empty;
    public string OtpCode { get => otpCode; set => SetProperty(ref otpCode, value); }

    string errorMessage = string.Empty;
    public string ErrorMessage { get => errorMessage; set => SetProperty(ref errorMessage, value); }

    bool isOtpSent;
    public bool IsOtpSent { get => isOtpSent; set => SetProperty(ref isOtpSent, value); }

    // ── Cooldown ─────────────────────────────────────────────────────────────
    public int OtpCooldownSeconds
    {
        get => _otpCooldownSeconds;
        private set
        {
            if (SetProperty(ref _otpCooldownSeconds, value))
            {
                RaisePropertyChanged(nameof(CanResendOtp));
                RaisePropertyChanged(nameof(OtpCooldownText));
                ResendOtpCommand.NotifyCanExecuteChanged();
            }
        }
    }
    public bool   CanResendOtp    => OtpCooldownSeconds == 0 && IsOtpSent;
    public string OtpCooldownText => OtpCooldownSeconds > 0 ? $"Resend in {OtpCooldownSeconds}s" : "Resend OTP";

    // ── Actions ──────────────────────────────────────────────────────────────
    private async Task SendOtpAsync()
    {
        ErrorMessage = string.Empty;
        if (string.IsNullOrWhiteSpace(Email))
        {
            ErrorMessage = "Please enter your email address.";
            return;
        }

        IsBusy = true;
        try
        {
            await _auth.ForgotPasswordAsync(Email.Trim());
            IsOtpSent   = true;
            CurrentStep = 2;
            StartCooldown();
            await ShowSuccessToastAsync("Reset code sent. Please check your email.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "ForgotPassword request failed for {Email}", Email);
            // Backend always returns 200 to avoid email enumeration; any exception is a network/server error
            ErrorMessage = "Unable to send reset code. Please check your connection and try again.";
        }
        finally
        {
            IsBusy = false;
        }
    }

    private async Task VerifyOtpAsync()
    {
        ErrorMessage = string.Empty;
        if (string.IsNullOrWhiteSpace(OtpCode))
        {
            ErrorMessage = "Please enter the OTP code.";
            return;
        }

        IsBusy = true;
        try
        {
            var resetToken = await _auth.VerifyResetOtpAsync(Email.Trim(), OtpCode.Trim());
            if (resetToken is null)
            {
                ErrorMessage = "Invalid or expired OTP. Please try again.";
                return;
            }

            StopCooldown();
            // resetToken is passed as a navigation parameter — never stored or logged
            await Shell.Current.GoToAsync($"reset-password?email={Uri.EscapeDataString(Email.Trim())}&resetToken={Uri.EscapeDataString(resetToken)}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "VerifyResetOtp failed for {Email}", Email);
            ErrorMessage = "Verification failed. Please check your connection and try again.";
        }
        finally
        {
            IsBusy = false;
        }
    }

    private async Task ResendOtpAsync()
    {
        ErrorMessage = string.Empty;
        IsBusy = true;
        try
        {
            await _auth.ForgotPasswordAsync(Email.Trim());
            OtpCode = string.Empty;
            StartCooldown();
            await ShowSuccessToastAsync("A new reset code has been sent.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "ResendOtp failed for {Email}", Email);
            ErrorMessage = "Unable to resend reset code. Please try again.";
        }
        finally
        {
            IsBusy = false;
        }
    }

    // ── Cooldown timer ────────────────────────────────────────────────────────
    private void StartCooldown()
    {
        StopCooldown();
        OtpCooldownSeconds = 60;
        _cooldownTimer = Application.Current!.Dispatcher.CreateTimer();
        _cooldownTimer.Interval = TimeSpan.FromSeconds(1);
        _cooldownTimer.Tick += (_, _) =>
        {
            if (OtpCooldownSeconds > 0)
                OtpCooldownSeconds--;
            else
                StopCooldown();
        };
        _cooldownTimer.Start();
    }

    private void StopCooldown()
    {
        _cooldownTimer?.Stop();
        _cooldownTimer = null;
        OtpCooldownSeconds = 0;
    }
}
