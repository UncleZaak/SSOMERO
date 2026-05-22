using System.Collections.ObjectModel;
using System.Windows.Input;
using Microsoft.Extensions.Logging;
using Ssomero.Interfaces;
using Ssomero.Models;

namespace Ssomero.ViewModels;

public class AuditLogsViewModel : BaseViewModel
{
    private readonly IAdminService _admin;
    private readonly ILogger<AuditLogsViewModel> _logger;

    public ObservableCollection<AuditLogDto> Logs { get; } = [];

    public static readonly List<string> ActionOptions =
        ["All", "CREATE", "UPDATE", "DELETE", "APPROVE", "SUSPEND", "ACTIVATE"];

    public static readonly List<string> EntityOptions =
        ["All", "Student", "Lecturer", "Department", "Program", "Curriculum", "University", "Faculty"];

    // ── Filters ───────────────────────────────────────────────────────────────
    string searchText = string.Empty;
    public string SearchText
    {
        get => searchText;
        set => SetProperty(ref searchText, value);
    }

    string selectedAction = "All";
    public string SelectedAction
    {
        get => selectedAction;
        set => SetProperty(ref selectedAction, value);
    }

    string selectedEntity = "All";
    public string SelectedEntity
    {
        get => selectedEntity;
        set => SetProperty(ref selectedEntity, value);
    }

    DateTime? fromDate;
    public DateTime? FromDate
    {
        get => fromDate;
        set => SetProperty(ref fromDate, value);
    }

    DateTime? toDate;
    public DateTime? ToDate
    {
        get => toDate;
        set => SetProperty(ref toDate, value);
    }

    // ── Pagination ────────────────────────────────────────────────────────────
    int currentPage = 1;
    public int CurrentPage
    {
        get => currentPage;
        set
        {
            if (SetProperty(ref currentPage, value))
                RaisePropertyChanged(nameof(PageInfo));
        }
    }

    int totalPages = 1;
    public int TotalPages
    {
        get => totalPages;
        set
        {
            if (SetProperty(ref totalPages, value))
                RaisePropertyChanged(nameof(PageInfo));
        }
    }

    int totalCount;
    public int TotalCount { get => totalCount; set => SetProperty(ref totalCount, value); }

    public string PageInfo => $"Page {CurrentPage} of {TotalPages}  ({TotalCount} entries)";

    // ── State ─────────────────────────────────────────────────────────────────
    bool hasError;
    public bool HasError { get => hasError; set => SetProperty(ref hasError, value); }

    bool isEmpty;
    public bool IsEmpty { get => isEmpty; set => SetProperty(ref isEmpty, value); }

    bool hasFilters;
    public bool HasFilters { get => hasFilters; set => SetProperty(ref hasFilters, value); }

    // ── Commands ──────────────────────────────────────────────────────────────
    public ICommand LoadCommand    { get; }
    public ICommand SearchCommand  { get; }
    public ICommand ClearCommand   { get; }
    public ICommand NextPageCommand { get; }
    public ICommand PrevPageCommand { get; }
    public ICommand RefreshCommand { get; }

    public AuditLogsViewModel(IAdminService admin, ILogger<AuditLogsViewModel> logger)
    {
        _admin = admin;
        _logger = logger;
        Title = "Audit Logs";

        LoadCommand    = new Command(async () => await LoadAsync());
        RefreshCommand = new Command(async () => { CurrentPage = 1; await LoadAsync(); });
        SearchCommand  = new Command(async () => { CurrentPage = 1; await LoadAsync(); });
        ClearCommand   = new Command(async () => { ClearFilters(); await LoadAsync(); });
        NextPageCommand = new Command(
            async () => { if (CurrentPage < TotalPages) { CurrentPage++; await LoadAsync(); } },
            () => CurrentPage < TotalPages);
        PrevPageCommand = new Command(
            async () => { if (CurrentPage > 1) { CurrentPage--; await LoadAsync(); } },
            () => CurrentPage > 1);
    }

    public async Task LoadAsync()
    {
        if (IsBusy) return;
        IsBusy = true;
        HasError = false;

        try
        {
            var ct = CreateLinkedToken();
            var action = SelectedAction == "All" ? null : SelectedAction;
            var entity = SelectedEntity == "All" ? null : SelectedEntity;
            var search = string.IsNullOrWhiteSpace(SearchText) ? null : SearchText.Trim();

            UpdateHasFilters(action, entity, search);

            var result = await _admin.GetAuditLogsAsync(
                page: CurrentPage,
                pageSize: 20,
                action: action,
                entity: entity,
                fromDate: FromDate,
                toDate: ToDate,
                search: search,
                ct: ct);

            Logs.Clear();
            if (result is not null)
            {
                foreach (var log in result.Items)
                    Logs.Add(log);
                TotalCount = result.TotalCount;
                TotalPages = Math.Max(1, result.TotalPages);
            }

            IsEmpty = Logs.Count == 0;
            ((Command)NextPageCommand).ChangeCanExecute();
            ((Command)PrevPageCommand).ChangeCanExecute();
        }
        catch (OperationCanceledException) { }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Audit logs load failed");
            HasError = true;
        }
        finally
        {
            IsBusy = false;
        }
    }

    private void ClearFilters()
    {
        SearchText = string.Empty;
        SelectedAction = "All";
        SelectedEntity = "All";
        FromDate = null;
        ToDate = null;
        CurrentPage = 1;
    }

    private void UpdateHasFilters(string? action, string? entity, string? search)
        => HasFilters = action is not null || entity is not null || search is not null
                     || FromDate.HasValue || ToDate.HasValue;
}
