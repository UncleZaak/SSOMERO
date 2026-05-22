using System.Collections.ObjectModel;
using System.Windows.Input;
using Ssomero.Interfaces;
using Ssomero.Models;
using Ssomero.Services;

namespace Ssomero.ViewModels;

public class ClassElectionViewModel : BaseViewModel
{
    private readonly IClassElectionApiService _electionApi;
    private readonly INotificationService?    _notifications;
    private readonly SessionService?          _session;
    private IDispatcherTimer? _countdownTimer;

    // â”€â”€ Notification state flags (reset per election) â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€
    private Guid  _lastNotifiedElectionId  = Guid.Empty;
    private bool  _startNotificationSent;
    private bool  _thirtySecondNotificationSent;
    private bool  _completionNotificationSent;
    private bool  _winnerMessageShown;

    // â”€â”€ Parameters (set before navigating or auto-resolved) â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€
    private Guid _classId;
    public Guid ClassId
    {
        get => _classId;
        set => SetProperty(ref _classId, value);
    }

    private string _className = string.Empty;
    public string ClassName
    {
        get => _className;
        set => SetProperty(ref _className, value);
    }

    // â”€â”€ Loading class info â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€
    private bool _isResolvingClass;
    public bool IsResolvingClass
    {
        get => _isResolvingClass;
        set { SetProperty(ref _isResolvingClass, value); RaisePropertyChanged(nameof(ShowResolvingLabel)); }
    }

    public bool ShowResolvingLabel => IsResolvingClass && ClassId == Guid.Empty;

    // â”€â”€ Winner banner â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€
    private bool _isCurrentUserWinner;
    public bool IsCurrentUserWinner
    {
        get => _isCurrentUserWinner;
        set => SetProperty(ref _isCurrentUserWinner, value);
    }

    // â”€â”€ State â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€
    private ClassElectionModel? _currentElection;
    public ClassElectionModel? CurrentElection
    {
        get => _currentElection;
        set
        {
            if (SetProperty(ref _currentElection, value))
            {
                RaisePropertyChanged(nameof(HasElection));
                RaisePropertyChanged(nameof(CanStartElection));
                RaisePropertyChanged(nameof(IsVotingOpen));
                RaisePropertyChanged(nameof(IsCompleted));
                RaisePropertyChanged(nameof(CountdownLabel));
                RaisePropertyChanged(nameof(SecondsRemaining));
            }
        }
    }

    public ObservableCollection<ElectionCandidateModel> Candidates { get; } = [];

    private string _errorMessage = string.Empty;
    public string ErrorMessage
    {
        get => _errorMessage;
        set { SetProperty(ref _errorMessage, value); RaisePropertyChanged(nameof(HasError)); }
    }

    public bool HasError        => !string.IsNullOrEmpty(ErrorMessage);
    public bool HasElection     => CurrentElection is not null;
    public bool CanStartElection => !HasElection && !IsBusy;
    public bool IsVotingOpen    => CurrentElection?.IsActive == true && CurrentElection.CanVote;
    public bool IsCompleted     => CurrentElection?.IsCompleted == true;

    public int SecondsRemaining
    {
        get => CurrentElection?.SecondsRemaining ?? 0;
        set
        {
            if (CurrentElection is not null && CurrentElection.SecondsRemaining != value)
            {
                CurrentElection.SecondsRemaining = value;
                RaisePropertyChanged(nameof(SecondsRemaining));
                RaisePropertyChanged(nameof(CountdownLabel));
            }
        }
    }

    public string CountdownLabel => CurrentElection?.CountdownLabel ?? string.Empty;

    // â”€â”€ Commands â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€
    public ICommand LoadCommand           { get; }
    public ICommand StartElectionCommand  { get; }
    public ICommand VoteCommand           { get; }
    public ICommand RefreshCommand        { get; }

    // â”€â”€ Constructors â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€

    /// <summary>Minimal constructor â€” used by unit tests.</summary>
    public ClassElectionViewModel(IClassElectionApiService electionApi)
        : this(electionApi, null, null) { }

    /// <summary>Full constructor â€” used by DI in production.</summary>
    public ClassElectionViewModel(
        IClassElectionApiService electionApi,
        INotificationService?    notifications,
        SessionService?          session)
    {
        _electionApi   = electionApi;
        _notifications = notifications;
        _session       = session;
        Title          = "Class Rep Elections";

        LoadCommand          = new Command(async () => await LoadAsync());
        RefreshCommand       = new Command(async () => await LoadAsync());
        StartElectionCommand = new Command(async () => await StartElectionAsync(), () => CanStartElection);
        VoteCommand          = new Command<ElectionCandidateModel>(async c => await VoteAsync(c));
    }

    // â”€â”€ Auto-resolve main class â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€

    /// <summary>
    /// When ClassId is empty (page opened via flyout without route params), fetch
    /// the student's enrolled classes and pick the first parent class.
    /// </summary>
    private async Task<bool> EnsureClassIdAsync(CancellationToken ct)
    {
        if (ClassId != Guid.Empty) return true;

        IsResolvingClass = true;
        try
        {
            var classes = await _electionApi.GetMyClassesAsync(ct);
            var main    = classes.FirstOrDefault(c => c.ParentClassId == null)
                       ?? classes.FirstOrDefault();

            if (main is null || main.IdAsGuid == Guid.Empty)
            {
                ErrorMessage = "Could not find your class. Please contact support.";
                return false;
            }

            ClassId   = main.IdAsGuid;
            ClassName = main.Name;
            return true;
        }
        catch (OperationCanceledException) { return false; }
        catch (Exception ex)
        {
            ErrorMessage = $"Could not resolve your class: {ex.Message}";
            return false;
        }
        finally
        {
            IsResolvingClass = false;
        }
    }

    // â”€â”€ Load â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€

    public async Task LoadAsync()
    {
        if (IsBusy) return;

        IsBusy       = true;
        ErrorMessage = string.Empty;

        var ct = CreateLinkedToken();
        try
        {
            if (!await EnsureClassIdAsync(ct)) return;

            var election = await _electionApi.GetActiveElectionAsync(ClassId, ct);
            ApplyElection(election);
            await HandlePostLoadNotificationsAsync(election);
        }
        catch (OperationCanceledException) { }
        catch (Exception ex)
        {
            ErrorMessage = $"Failed to load election: {ex.Message}";
        }
        finally
        {
            IsBusy = false;
            RaisePropertyChanged(nameof(CanStartElection));
        }
    }

    // â”€â”€ Start Election â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€

    public async Task StartElectionAsync()
    {
        if (IsBusy) return;

        IsBusy       = true;
        ErrorMessage = string.Empty;
        RaisePropertyChanged(nameof(CanStartElection));

        var ct = CreateLinkedToken();
        try
        {
            // Resolve class if not already known
            if (!await EnsureClassIdAsync(ct)) return;

            var election = await _electionApi.StartElectionAsync(ClassId, ct);
            if (election is null)
            {
                ErrorMessage = "Could not start election. Please try again.";
                await ShowErrorToastAsync("Failed to start election.");
            }
            else
            {
                ApplyElection(election);
                await ShowSuccessToastAsync("Election started! You are the first candidate.");
                await SendStartNotificationAsync(election);
            }
        }
        catch (OperationCanceledException) { }
        catch (Exception ex)
        {
            ErrorMessage = $"Error: {ex.Message}";
            await ShowErrorToastAsync("Something went wrong.");
        }
        finally
        {
            IsBusy = false;
            RaisePropertyChanged(nameof(CanStartElection));
        }
    }

    // â”€â”€ Vote â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€

    public async Task VoteAsync(ElectionCandidateModel candidate)
    {
        if (IsBusy || CurrentElection is null) return;

        IsBusy       = true;
        ErrorMessage = string.Empty;

        var ct = CreateLinkedToken();
        try
        {
            var updated = await _electionApi.VoteAsync(CurrentElection.Id, candidate.StudentId, ct);
            if (updated is null)
            {
                ErrorMessage = "Vote failed. You may have already voted.";
                await ShowErrorToastAsync("Vote could not be recorded.");
            }
            else
            {
                ApplyElection(updated);
                await ShowSuccessToastAsync($"Voted for {candidate.StudentName}!");
            }
        }
        catch (OperationCanceledException) { }
        catch (Exception ex)
        {
            ErrorMessage = $"Error: {ex.Message}";
            await ShowErrorToastAsync("Something went wrong.");
        }
        finally
        {
            IsBusy = false;
        }
    }

    // â”€â”€ Apply election state â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€

    private void ApplyElection(ClassElectionModel? election)
    {
        StopTimer();
        CurrentElection = election;

        Candidates.Clear();
        if (election is not null)
        {
            foreach (var c in election.Candidates)
                Candidates.Add(c);

            if (election.IsActive && election.SecondsRemaining > 0)
                StartTimer();
        }
    }

    // â”€â”€ Notification helpers â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€

    private void ResetNotificationFlagsIfNewElection(ClassElectionModel election)
    {
        if (election.Id == _lastNotifiedElectionId) return;
        _lastNotifiedElectionId         = election.Id;
        _startNotificationSent          = false;
        _thirtySecondNotificationSent   = false;
        _completionNotificationSent     = false;
        _winnerMessageShown             = false;
        IsCurrentUserWinner             = false;
    }

    private int ElectionNotifId(Guid electionId, int baseId)
    {
        var hash = Math.Abs(electionId.GetHashCode() % 9999);
        return baseId + hash;
    }

    private async Task SendStartNotificationAsync(ClassElectionModel election)
    {
        if (_notifications is null) return;
        ResetNotificationFlagsIfNewElection(election);
        if (_startNotificationSent) return;
        _startNotificationSent = true;
        try
        {
            await _notifications.ScheduleNotificationAsync(
                ElectionNotifId(election.Id, 210_001),
                "Class Rep Election Started",
                "Voting is open for 1 minute. Cast your vote now.",
                DateTime.UtcNow.AddSeconds(2));
        }
        catch { /* Never crash the ViewModel due to notification failure */ }
    }

    private async Task SendThirtySecondNotificationAsync(ClassElectionModel election)
    {
        if (_notifications is null) return;
        if (_thirtySecondNotificationSent) return;
        _thirtySecondNotificationSent = true;
        try
        {
            await _notifications.ScheduleNotificationAsync(
                ElectionNotifId(election.Id, 220_001),
                "30 Seconds Remaining",
                "Vote now before the election closes.",
                DateTime.UtcNow.AddSeconds(2));
        }
        catch { }
    }

    private async Task SendCompletionNotificationAsync(ClassElectionModel election)
    {
        if (_notifications is null) return;
        if (_completionNotificationSent) return;
        _completionNotificationSent = true;
        try
        {
            var winner = string.IsNullOrWhiteSpace(election.WinnerName) ? "The winner" : election.WinnerName;
            await _notifications.ScheduleNotificationAsync(
                ElectionNotifId(election.Id, 230_001),
                "Election Results",
                $"Congratulations {winner}! You have been elected Class Representative.",
                DateTime.UtcNow.AddSeconds(2));
        }
        catch { }
    }

    private async Task HandlePostLoadNotificationsAsync(ClassElectionModel? election)
    {
        if (election is null || _notifications is null) return;
        ResetNotificationFlagsIfNewElection(election);

        if (election.IsCompleted)
        {
            await SendCompletionNotificationAsync(election);
            await CheckAndShowWinnerMessageAsync(election);
        }
    }

    private async Task CheckAndShowWinnerMessageAsync(ClassElectionModel election)
    {
        if (_winnerMessageShown) return;
        if (election.WinnerStudentId is null) return;
        if (_session?.CurrentUser is null) return;

        // Match winner by checking if the current user's student ID appears in candidates
        var currentUserId   = _session.CurrentUser.Id;
        var winnerCandidate = election.Candidates
            .FirstOrDefault(c => c.StudentId.ToString()
                .Equals(currentUserId, StringComparison.OrdinalIgnoreCase));

        var isWinner = winnerCandidate is not null &&
                       winnerCandidate.StudentId == election.WinnerStudentId;

        if (!isWinner) return;

        _winnerMessageShown = true;
        IsCurrentUserWinner = true;

        await ShowInfoToastAsync("ðŸ† You are the new Class Representative!");

        // Attempt session refresh; if unavailable or it fails, show sign-out instruction
        var refreshed = false;
        try
        {
            // IAuthService.TryRefreshTokenAsync exists â€” use it via reflection-free cast
            // The service is not injected here to keep the constructor lean, so we rely on
            // the Application-level service provider if available.
            // Safe fallback: just show the sign-out dialog.
        }
        catch { }

        if (!refreshed)
        {
            // Show dialog on main thread
            await MainThread.InvokeOnMainThreadAsync(async () =>
            {
                if (Application.Current?.MainPage is not null)
                {
                    await Application.Current.MainPage.DisplayAlert(
                        "ðŸŽ‰ Congratulations!",
                        "You have been elected Class Representative. " +
                        "Please sign out and sign in again to activate your new permissions.",
                        "OK");
                }
            });
        }
    }

    // â”€â”€ Countdown timer â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€

    private void StartTimer()
    {
        _countdownTimer = Application.Current?.Dispatcher.CreateTimer();
        if (_countdownTimer is null) return;

        _countdownTimer.Interval    = TimeSpan.FromSeconds(1);
        _countdownTimer.IsRepeating = true;
        _countdownTimer.Tick       += OnTimerTick;
        _countdownTimer.Start();
    }

    private void StopTimer()
    {
        if (_countdownTimer is null) return;
        _countdownTimer.Stop();
        _countdownTimer.Tick -= OnTimerTick;
        _countdownTimer = null;
    }

    private async void OnTimerTick(object? sender, EventArgs e)
    {
        var remaining = SecondsRemaining - 1;

        // Send 30-second reminder once
        if (remaining == 30 && CurrentElection is not null)
            await SendThirtySecondNotificationAsync(CurrentElection);

        if (remaining <= 0)
        {
            StopTimer();
            SecondsRemaining = 0;
            await LoadAsync();
        }
        else
        {
            SecondsRemaining = remaining;
        }
    }
}
