using System.Collections.ObjectModel;
using System.Text;
using System.Windows.Input;
using Microsoft.Extensions.Logging;
using Ssomero.Interfaces;
using Ssomero.Models;

namespace Ssomero.ViewModels;

public class FacultiesViewModel : BaseViewModel
{
    private readonly IAcademicService _academic;
    private readonly ILogger<FacultiesViewModel> _logger;

    private List<FacultyDto> _allFaculties = [];

    public ObservableCollection<FacultyDto> Faculties { get; } = [];
    public ObservableCollection<LookupItem> Universities { get; } = [];

    // Search
    string searchQuery = string.Empty;
    public string SearchQuery
    {
        get => searchQuery;
        set
        {
            if (SetProperty(ref searchQuery, value))
                ApplyFilterAndPagination();
        }
    }

    // Filter by university
    string universityFilter = "All";
    public string UniversityFilter
    {
        get => universityFilter;
        set
        {
            if (SetProperty(ref universityFilter, value))
                ApplyFilterAndPagination();
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
                ApplyFilterAndPagination();
            }
        }
    }

    // Stats
    int totalFaculties;
    public int TotalFaculties { get => totalFaculties; set => SetProperty(ref totalFaculties, value); }

    int activeFacultiesCount;
    public int ActiveFacultiesCount { get => activeFacultiesCount; set => SetProperty(ref activeFacultiesCount, value); }

    int pendingCount;
    public int PendingCount { get => pendingCount; set => SetProperty(ref pendingCount, value); }

    // Add/Edit form
    string editName = string.Empty;
    public string EditName { get => editName; set => SetProperty(ref editName, value); }

    string? editId;
    public string? EditId { get => editId; set => SetProperty(ref editId, value); }

    string? selectedUniversityId;
    public string? SelectedUniversityId { get => selectedUniversityId; set => SetProperty(ref selectedUniversityId, value); }

    LookupItem? selectedUniversity;
    public LookupItem? SelectedUniversity
    {
        get => selectedUniversity;
        set
        {
            if (SetProperty(ref selectedUniversity, value))
                SelectedUniversityId = value?.Id;
        }
    }

    bool isEmpty;
    public bool IsEmpty { get => isEmpty; set => SetProperty(ref isEmpty, value); }

    bool isEmptySearch;
    public bool IsEmptySearch { get => isEmptySearch; set => SetProperty(ref isEmptySearch, value); }

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
    public ICommand AddFacultyCommand { get; }
    public ICommand EditFacultyCommand { get; }
    public ICommand DeleteFacultyCommand { get; }
    public ICommand SaveCommand { get; }
    public ICommand CancelEditCommand { get; }
    public ICommand ExportCommand { get; }
    public ICommand RefreshCommand { get; }

    public FacultiesViewModel(IAcademicService academic, ILogger<FacultiesViewModel> logger)
    {
        _academic = academic;
        _logger = logger;

        LoadCommand = new Command(async () => await LoadAsync());
        SearchCommand = new Command(() => ApplyFilterAndPagination());
        FilterCommand = new Command<string>(filter => UniversityFilter = filter);
        NextPageCommand = new Command(() => { if (CurrentPage < TotalPages) { CurrentPage++; ApplyFilterAndPagination(); } });
        PrevPageCommand = new Command(() => { if (CurrentPage > 1) { CurrentPage--; ApplyFilterAndPagination(); } });
        AddFacultyCommand = new Command(() => { EditId = null; EditName = string.Empty; SelectedUniversity = null; IsEditing = true; });
        EditFacultyCommand = new Command<FacultyDto>(f =>
        {
            EditId = f.Id;
            EditName = f.Name;
            SelectedUniversity = Universities.FirstOrDefault(u => u.Id == f.UniversityId);
            IsEditing = true;
        });
        DeleteFacultyCommand = new Command<FacultyDto>(async f => await DeleteAsync(f));
        SaveCommand = new Command(async () => await SaveAsync());
        CancelEditCommand = new Command(() => { IsEditing = false; EditName = string.Empty; EditId = null; SelectedUniversity = null; });
        ExportCommand = new Command(async () => await ExportAsync());
        RefreshCommand = new Command(async () => await LoadAsync());
    }

    public async Task LoadAsync()
    {
        if (IsBusy) return;
        IsBusy = true;
        ErrorMessage = string.Empty;

        try
        {
            // Load universities for dropdown
            var unis = await _academic.GetUniversitiesAsync();
            Universities.Clear();
            foreach (var u in unis)
                Universities.Add(u);

            _allFaculties = await _academic.GetFacultyDetailsAsync();
            TotalFaculties = _allFaculties.Count;
            ActiveFacultiesCount = _allFaculties.Count(f => f.Status == "Active");
            PendingCount = _allFaculties.Count(f => f.Status != "Active");
            ApplyFilterAndPagination();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to load faculties");
            ErrorMessage = "Failed to load faculties.";
        }
        finally
        {
            IsBusy = false;
        }
    }

    private void ApplyFilterAndPagination()
    {
        var filtered = _allFaculties.AsEnumerable();

        if (!string.IsNullOrWhiteSpace(SearchQuery))
        {
            var q = SearchQuery.Trim().ToLowerInvariant();
            filtered = filtered.Where(f => f.Name.Contains(q, StringComparison.OrdinalIgnoreCase)
                || f.UniversityName.Contains(q, StringComparison.OrdinalIgnoreCase));
        }

        if (UniversityFilter != "All")
            filtered = filtered.Where(f => f.UniversityId == UniversityFilter);

        var list = filtered.OrderBy(f => f.Name).ToList();
        TotalPages = Math.Max(1, (int)Math.Ceiling(list.Count / (double)PageSize));
        if (CurrentPage > TotalPages) CurrentPage = TotalPages;

        var paged = list.Skip((CurrentPage - 1) * PageSize).Take(PageSize);

        Faculties.Clear();
        foreach (var f in paged)
            Faculties.Add(f);

        IsEmpty = _allFaculties.Count == 0;
        IsEmptySearch = _allFaculties.Count > 0 && list.Count == 0;
    }

    private async Task SaveAsync()
    {
        if (string.IsNullOrWhiteSpace(EditName))
        {
            ErrorMessage = "Faculty name is required.";
            return;
        }

        if (string.IsNullOrWhiteSpace(SelectedUniversityId))
        {
            ErrorMessage = "Please select a university.";
            return;
        }

        IsBusy = true;
        ErrorMessage = string.Empty;

        try
        {
            if (EditId is null)
            {
                var result = await _academic.CreateFacultyAsync(EditName.Trim(), SelectedUniversityId);
                if (result is null) { ErrorMessage = "A faculty with this name already exists in the selected university."; return; }
            }
            else
            {
                var result = await _academic.UpdateFacultyAsync(EditId, EditName.Trim(), SelectedUniversityId);
                if (result is null) { ErrorMessage = "A faculty with this name already exists in the selected university."; return; }
            }

            IsEditing = false;
            EditName = string.Empty;
            EditId = null;
            SelectedUniversity = null;
            await LoadAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to save faculty");
            ErrorMessage = "Failed to save faculty.";
        }
        finally
        {
            IsBusy = false;
        }
    }

    private async Task DeleteAsync(FacultyDto faculty)
    {
        bool confirm = await Shell.Current.DisplayAlert("Confirm Delete",
            $"Are you sure you want to delete '{faculty.Name}'?", "Delete", "Cancel");
        if (!confirm) return;

        IsBusy = true;
        try
        {
            var success = await _academic.DeleteFacultyAsync(faculty.Id);
            if (!success)
            {
                await Shell.Current.DisplayAlert("Error", "This faculty cannot be deleted because it has departments assigned.", "OK");
                return;
            }
            await LoadAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to delete faculty");
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
            sb.AppendLine("Name,University,Departments,Status");
            foreach (var f in _allFaculties)
                sb.AppendLine($"\"{f.Name}\",\"{f.UniversityName}\",{f.DepartmentsCount},{f.Status}");

            var filePath = Path.Combine(FileSystem.CacheDirectory, "faculties_export.csv");
            await File.WriteAllTextAsync(filePath, sb.ToString());

            await Share.Default.RequestAsync(new ShareFileRequest
            {
                Title = "Export Faculties",
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
