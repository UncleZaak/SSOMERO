using System.Collections.ObjectModel;
using System.Text;
using System.Windows.Input;
using Microsoft.Extensions.Logging;
using Ssomero.Interfaces;
using Ssomero.Models;

namespace Ssomero.ViewModels;

public class UniversitiesViewModel : BaseViewModel
{
    private readonly IAcademicService _academic;
    private readonly ILogger<UniversitiesViewModel> _logger;

    public ObservableCollection<UniversityDto> Universities { get; } = [];

    // Search
    string searchQuery = string.Empty;
    public string SearchQuery
    {
        get => searchQuery;
        set => SetProperty(ref searchQuery, value);
    }

    // Filter (kept for UI compatibility — server filters by search only)
    string statusFilter = "All";
    public string StatusFilter
    {
        get => statusFilter;
        set
        {
            if (SetProperty(ref statusFilter, value))
                _ = LoadAsync();
        }
    }

    // Pagination
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

    public string PageInfo => $"Page {CurrentPage} of {TotalPages}";

    int pageSize = 10;
    public int PageSize
    {
        get => pageSize;
        set
        {
            if (SetProperty(ref pageSize, value))
            {
                CurrentPage = 1;
                _ = LoadAsync();
            }
        }
    }

    // Stats
    int totalInstitutions;
    public int TotalInstitutions { get => totalInstitutions; set => SetProperty(ref totalInstitutions, value); }

    int accreditedCount;
    public int AccreditedCount { get => accreditedCount; set => SetProperty(ref accreditedCount, value); }

    int pendingCount;
    public int PendingCount { get => pendingCount; set => SetProperty(ref pendingCount, value); }

    // Add/Edit form
    string editName = string.Empty;
    public string EditName { get => editName; set => SetProperty(ref editName, value); }

    string? editId;
    public string? EditId { get => editId; set => SetProperty(ref editId, value); }

    bool isEditing;
    public bool IsEditing { get => isEditing; set => SetProperty(ref isEditing, value); }

    string errorMessage = string.Empty;
    public string ErrorMessage { get => errorMessage; set => SetProperty(ref errorMessage, value); }

    // Commands
    public ICommand LoadCommand { get; }
    public ICommand SearchCommand { get; }
    public ICommand FilterCommand { get; }
    public ICommand NextPageCommand { get; }
    public ICommand PrevPageCommand { get; }
    public ICommand AddUniversityCommand { get; }
    public ICommand EditUniversityCommand { get; }
    public ICommand DeleteUniversityCommand { get; }
    public ICommand SaveCommand { get; }
    public ICommand CancelEditCommand { get; }
    public ICommand ExportCommand { get; }
    public ICommand RefreshCommand { get; }

    public UniversitiesViewModel(IAcademicService academic, ILogger<UniversitiesViewModel> logger)
    {
        _academic = academic;
        _logger = logger;

        LoadCommand = new Command(async () => await LoadAsync());
        SearchCommand = new Command(async () => { CurrentPage = 1; await LoadAsync(); });
        FilterCommand = new Command<string>(status => StatusFilter = status);
        NextPageCommand = new Command(async () => { if (CurrentPage < TotalPages) { CurrentPage++; await LoadAsync(); } });
        PrevPageCommand = new Command(async () => { if (CurrentPage > 1) { CurrentPage--; await LoadAsync(); } });
        AddUniversityCommand = new Command(() => { EditId = null; EditName = string.Empty; IsEditing = true; });
        EditUniversityCommand = new Command<UniversityDto>(u => { EditId = u.Id; EditName = u.Name; IsEditing = true; });
        DeleteUniversityCommand = new Command<UniversityDto>(async u => await DeleteAsync(u));
        SaveCommand = new Command(async () => await SaveAsync());
        CancelEditCommand = new Command(() => { IsEditing = false; EditName = string.Empty; EditId = null; });
        ExportCommand = new Command(async () => await ExportAsync());
        RefreshCommand = new Command(async () => { CurrentPage = 1; await LoadAsync(); });
    }

    public async Task LoadAsync()
    {
        if (IsBusy) return;
        IsBusy = true;
        ErrorMessage = string.Empty;

        try
        {
            var result = await _academic.GetUniversitiesPaginatedAsync(CurrentPage, PageSize, SearchQuery);

            TotalInstitutions = result.TotalCount;
            TotalPages = result.TotalPages > 0 ? result.TotalPages : 1;
            if (CurrentPage > TotalPages) CurrentPage = TotalPages;

            AccreditedCount = result.Data.Count(u => u.Status == "Active");
            PendingCount = result.Data.Count(u => u.Status != "Active");

            Universities.Clear();
            foreach (var u in result.Data)
                Universities.Add(u);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to load universities");
            ErrorMessage = "Failed to load universities.";
        }
        finally
        {
            IsBusy = false;
        }
    }

    private async Task SaveAsync()
    {
        if (string.IsNullOrWhiteSpace(EditName))
        {
            ErrorMessage = "University name is required.";
            return;
        }

        IsBusy = true;
        ErrorMessage = string.Empty;

        try
        {
            if (EditId is null)
            {
                var result = await _academic.CreateUniversityAsync(EditName.Trim());
                if (result is null) { ErrorMessage = "Failed to create university."; return; }
            }
            else
            {
                var result = await _academic.UpdateUniversityAsync(EditId, EditName.Trim());
                if (result is null) { ErrorMessage = "Failed to update university."; return; }
            }

            IsEditing = false;
            EditName = string.Empty;
            EditId = null;
            await LoadAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to save university");
            ErrorMessage = "Failed to save university.";
        }
        finally
        {
            IsBusy = false;
        }
    }

    private async Task DeleteAsync(UniversityDto uni)
    {
        bool confirm = await Shell.Current.DisplayAlert("Confirm Delete",
            $"Are you sure you want to delete '{uni.Name}'?", "Delete", "Cancel");
        if (!confirm) return;

        IsBusy = true;
        try
        {
            var success = await _academic.DeleteUniversityAsync(uni.Id);
            if (!success)
            {
                await Shell.Current.DisplayAlert("Error", "Cannot delete university. It may have faculties assigned.", "OK");
                return;
            }
            await LoadAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to delete university");
        }
        finally
        {
            IsBusy = false;
        }
    }

    private async Task ExportAsync()
    {
        try
        {
            var sb = new StringBuilder();
            sb.AppendLine("Name,Faculties,Status");
            foreach (var u in Universities)
                sb.AppendLine($"\"{u.Name}\",{u.FacultiesCount},{u.Status}");

            var filePath = Path.Combine(FileSystem.CacheDirectory, "universities_export.csv");
            await File.WriteAllTextAsync(filePath, sb.ToString());

            await Share.Default.RequestAsync(new ShareFileRequest
            {
                Title = "Export Universities",
                File = new ShareFile(filePath)
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Export failed");
            await Shell.Current.DisplayAlert("Error", "Export failed.", "OK");
        }
    }
}

