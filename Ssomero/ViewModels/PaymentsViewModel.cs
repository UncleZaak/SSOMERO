using System.Collections.ObjectModel;
using System.Windows.Input;
using Microsoft.Extensions.Logging;
using Microsoft.Maui.Controls;
using Ssomero.Interfaces;
using Ssomero.Models;

namespace Ssomero.ViewModels;

public enum PaymentUxState { Idle, Initiating, WaitingConfirmation, Verifying, Success, Failed }

public class PaymentsViewModel : BaseViewModel
{
    private readonly IPaymentsService _payments;
    private readonly IRefreshCoordinator _refresh;
    private readonly ILogger<PaymentsViewModel> _logger;

    // ── Current plan ──────────────────────────────────────────────────────────
    PaymentDto? currentPayment;
    public PaymentDto? CurrentPayment
    {
        get => currentPayment;
        set
        {
            SetProperty(ref currentPayment, value);
            RaisePropertyChanged(nameof(HasActivePlan));
            RaisePropertyChanged(nameof(CurrentPlan));
            RaisePropertyChanged(nameof(ExpiryLabel));
            RaisePropertyChanged(nameof(StatusLabel));
            RaisePropertyChanged(nameof(PlanSummary));
            RaisePropertyChanged(nameof(ExpiryText));
        }
    }

    public bool HasActivePlan => CurrentPayment?.Status == PaymentStatus.Completed
                                 && CurrentPayment.ExpiresAt > DateTime.UtcNow;

    public string CurrentPlan  => CurrentPayment is null ? "Free Plan" : $"{CurrentPayment.Plan} Plan";
    public string ExpiryLabel  => CurrentPayment is null ? "No active subscription"
        : HasActivePlan ? $"Expires {CurrentPayment.ExpiresAt:MMM dd, yyyy}" : "Plan expired";
    public string StatusLabel  => HasActivePlan ? "Active" : "Inactive";
    public string PlanSummary  => CurrentPayment is null ? "No active plan"
        : $"{CurrentPayment.Plan} — {CurrentPayment.Currency} {CurrentPayment.Amount:N0}";
    public string ExpiryText   => ExpiryLabel;

    // ── UX state ──────────────────────────────────────────────────────────────
    PaymentUxState uxState = PaymentUxState.Idle;
    public PaymentUxState UxState
    {
        get => uxState;
        set
        {
            SetProperty(ref uxState, value);
            RaisePropertyChanged(nameof(StatusInfo));
            RaisePropertyChanged(nameof(IsInProgress));
            RaisePropertyChanged(nameof(CanRetry));
        }
    }

    public bool IsInProgress => UxState is PaymentUxState.Initiating
        or PaymentUxState.WaitingConfirmation or PaymentUxState.Verifying;

    public bool CanRetry => UxState == PaymentUxState.Failed && _pendingTxRef is not null;

    public string StatusInfo => UxState switch
    {
        PaymentUxState.Initiating          => "Initiating payment…",
        PaymentUxState.WaitingConfirmation => "Approve the Mobile Money prompt on your phone…",
        PaymentUxState.Verifying           => "Verifying payment…",
        PaymentUxState.Success             => "Payment successful ✓",
        PaymentUxState.Failed              => "Payment failed.",
        _                                  => string.Empty,
    };

    // ── Fields ────────────────────────────────────────────────────────────────
    string errorMessage = string.Empty;
    public string ErrorMessage { get => errorMessage; set => SetProperty(ref errorMessage, value); }

    string successMessage = string.Empty;
    public string SuccessMessage { get => successMessage; set => SetProperty(ref successMessage, value); }

    string phoneNumber = string.Empty;
    public string PhoneNumber { get => phoneNumber; set => SetProperty(ref phoneNumber, value); }

    bool isSubscribing;
    public bool IsSubscribing { get => isSubscribing; set => SetProperty(ref isSubscribing, value); }

    // Pending txRef used for retry
    string? _pendingTxRef;

    // ── Commands ──────────────────────────────────────────────────────────────
    public ICommand LoadCommand { get; }
    public ICommand SubscribeCommand { get; }
    public ICommand RetryVerifyCommand { get; }
    public ICommand ViewHistoryCommand { get; }

    public ObservableCollection<PlanItem> Plans { get; } =
    [
        new PlanItem("Monthly Plan",   "UGX 5,000 / month",       PaymentPlan.Monthly,  "Full access for 1 month. Materials, classes, groups, analytics."),
        new PlanItem("Semester Plan",  "UGX 15,000 / semester",   PaymentPlan.Semester, "Best value! Full access for an entire semester."),
    ];

    public PaymentsViewModel(
        IPaymentsService payments,
        IRefreshCoordinator refresh,
        ILogger<PaymentsViewModel> logger)
    {
        _payments = payments;
        _refresh  = refresh;
        _logger   = logger;

        LoadCommand        = new Command(async () => await LoadAsync());
        SubscribeCommand   = new Command<PlanItem>(async p => await BeginSubscribeAsync(p.Plan));
        RetryVerifyCommand = new Command(async () => await RetryVerifyAsync());
        ViewHistoryCommand = new Command(async () => await Shell.Current.GoToAsync("payment-history"));
    }

    public async Task LoadAsync()
    {
        if (IsBusy) return;
        IsBusy = true;
        ErrorMessage = string.Empty;
        try
        {
            var ct = CreateLinkedToken();
            CurrentPayment = await _payments.GetCurrentPlanAsync(ct);
        }
        catch (OperationCanceledException) { }
        catch (Exception ex)
        {
            _logger.LogError(ex, "LoadAsync failed");
            ErrorMessage = "Failed to load plan.";
        }
        finally { IsBusy = false; }
    }

    // ── Step 1: Initiate ──────────────────────────────────────────────────────
    private async Task BeginSubscribeAsync(PaymentPlan plan)
    {
        if (string.IsNullOrWhiteSpace(PhoneNumber))
        {
            await Shell.Current.DisplayAlert("Phone Required",
                "Enter your MTN/Airtel Mobile Money number to continue.", "OK");
            return;
        }

        var amount  = plan == PaymentPlan.Monthly ? "UGX 5,000" : "UGX 15,000";
        var confirm = await Shell.Current.DisplayAlert("Confirm Payment",
            $"Subscribe to {plan} plan for {amount}?\nMobile Money: {PhoneNumber}", "Confirm", "Cancel");
        if (!confirm) return;

        IsSubscribing = true;
        ErrorMessage  = string.Empty;
        SuccessMessage = string.Empty;
        UxState       = PaymentUxState.Initiating;

        try
        {
            var ct = CreateLinkedToken();
            var (success, error, txRef) = await _payments.InitiatePaymentAsync(plan, PhoneNumber, ct);

            if (!success || txRef is null)
            {
                await FailAsync(error ?? "Payment initiation failed.");
                return;
            }

            _pendingTxRef = txRef;
            RaisePropertyChanged(nameof(CanRetry));
            UxState = PaymentUxState.WaitingConfirmation;

            await PollVerifyAsync(txRef, ct);
        }
        catch (OperationCanceledException)
        {
            UxState = PaymentUxState.Idle;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "BeginSubscribeAsync failed");
            await FailAsync("An unexpected error occurred.");
        }
        finally { IsSubscribing = false; }
    }

    // ── Step 2: Poll verify with exponential backoff ──────────────────────────
    // Delays: 2s -> 4s -> 8s -> 15s -> 30s -> 60s x4  (cumulative ~299 s = 5 min)
    private static readonly int[] PollingDelaysMs =
        [2_000, 4_000, 8_000, 15_000, 30_000, 60_000, 60_000, 60_000, 60_000];

    private async Task PollVerifyAsync(string txRef, CancellationToken ct)
    {
        for (int i = 0; i < PollingDelaysMs.Length; i++)
        {
            await Task.Delay(PollingDelaysMs[i], ct);

            UxState = PaymentUxState.Verifying;

            var (verified, error) = await _payments.VerifyPaymentAsync(txRef, ct);

            if (verified)
            {
                await OnPaymentSuccessAsync();
                return;
            }

            // If backend explicitly says it failed/cancelled — stop polling
            if (error is not null && !error.Contains("Pending", StringComparison.OrdinalIgnoreCase)
                && !error.Contains("try again", StringComparison.OrdinalIgnoreCase))
            {
                await FailAsync(error);
                return;
            }

            // Not yet confirmed — keep waiting
            if (i < PollingDelaysMs.Length - 1)
                UxState = PaymentUxState.WaitingConfirmation;
        }

        // Polling exhausted — inform user they can retry manually
        UxState = PaymentUxState.Failed;
        ErrorMessage = "Payment confirmation is taking longer than expected. Tap 'Retry' to check again.";
        await ShowInfoToastAsync("Waiting for provider confirmation");
    }

    // ── Retry manual verification ─────────────────────────────────────────────
    private async Task RetryVerifyAsync()
    {
        if (_pendingTxRef is null) return;

        IsSubscribing = true;
        UxState       = PaymentUxState.Verifying;
        ErrorMessage  = string.Empty;

        try
        {
            var ct = CreateLinkedToken();
            var (verified, error) = await _payments.VerifyPaymentAsync(_pendingTxRef, ct);

            if (verified)
                await OnPaymentSuccessAsync();
            else
                await FailAsync(error ?? "Payment not confirmed yet. Please wait and try again.");
        }
        catch (OperationCanceledException) { UxState = PaymentUxState.Idle; }
        catch (Exception ex)
        {
            _logger.LogError(ex, "RetryVerifyAsync failed");
            await FailAsync("Verification service unavailable.");
        }
        finally { IsSubscribing = false; }
    }

    // ── On success ────────────────────────────────────────────────────────────
    private async Task OnPaymentSuccessAsync()
    {
        _pendingTxRef = null;
        UxState       = PaymentUxState.Success;
        SuccessMessage = "Payment successful";

        await ShowSuccessToastAsync("Payment successful");

        // Reload current plan
        var ct = CreateLinkedToken();
        CurrentPayment = await _payments.GetCurrentPlanAsync(ct);

        // Notify other ViewModels (profile, dashboard, subscription guards)
        await _refresh.NotifyAsync(RefreshKeys.Subscription);
        await _refresh.NotifyAsync(RefreshKeys.Dashboard);
    }

    // ── On failure ────────────────────────────────────────────────────────────
    private async Task FailAsync(string message)
    {
        UxState      = PaymentUxState.Failed;
        ErrorMessage = message;
        RaisePropertyChanged(nameof(CanRetry));
        await ShowErrorToastAsync("Payment failed");
    }

    public void CancelPendingRequests()
    {
        CreateLinkedToken(); // cancels current token without issuing a new one
        UxState       = PaymentUxState.Idle;
        IsSubscribing = false;
    }
}

public record PlanItem(string Name, string Price, PaymentPlan Plan, string Description);


