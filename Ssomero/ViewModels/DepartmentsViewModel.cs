using System.Collections.ObjectModel;
using System.Text;
using System.Windows.Input;
using Microsoft.Extensions.Logging;
using Ssomero.Interfaces;
using Ssomero.Models;

namespace Ssomero.ViewModels;

public class DepartmentsViewModel : BaseViewModel
{
    private readonly IAcademicService _academic;
    private readonly IRefreshCoordinator _refresh;
    private readonly ILogger<DepartmentsViewModel> _logger;

    private List<DepartmentDto> _allDepartments = [];

    public ObservableCollection<DepartmentDto> Departments { get; } = [];

    // ── Cascade pickers ──────────────────────────────────────────────────────
    public ObservableCollection<UniversityDto> Universities { get; } = [];
    public ObservableCollection<FacultyDto> CascadeFaculties { get; } = [];
    // Form faculty picker (all faculties, used in add/edit form)
    public ObservableCollection<FacultyDto> Faculties { get; } = [];

    UniversityDto? _selectedCascadeUniversity;
    public UniversityDto? SelectedCascadeUniversity
    {
        get => _selectedCascadeUniversity;
        set
        {
            if (SetProperty(ref _selectedCascadeUniversity, value))
                _ = OnCascadeUniversityChangedAsync(value);
        }
    }

    FacultyDto? _selectedCascadeFaculty;
    public FacultyDto? SelectedCascadeFaculty
    {
        get => _selectedCascadeFaculty;
        set
        {
            if (SetProperty(ref _selectedCascadeFaculty, value))
                _ = OnCascadeFacultyChangedAsync(value);
        }
    }

    bool _isFacultyPickerEnabled;
    public bool IsFacultyPickerEnabled { get => _isFacultyPickerEnabled; set => SetProperty(ref _isFacultyPickerEnabled, value); }

    bool _isLoadingFaculties;
    public bool IsLoadingFaculties { get => _isLoadingFaculties; set => SetProperty(ref _isLoadingFaculties, value); }

    // ── Search & Pagination ──────────────────────────────────────────────────
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

    int currentPage = 1;
    public int CurrentPage
    {
        get => currentPage;
        set { if (SetProperty(ref currentPage, value)) RaisePropertyChanged(nameof(PageInfo)); }
    }

    int totalPages = 1;
    public int TotalPages
    {
        get => totalPages;
        set { if (SetProperty(ref totalPages, value)) RaisePropertyChanged(nameof(PageInfo)); }
    }

    public string PageInfo => $"Page {CurrentPage} of {TotalPages}";

    int pageSize = 10;
    public int PageSize
    {
        get => pageSize;
        set { if (SetProperty(ref pageSize, value)) { CurrentPage = 1; ApplyFilterAndPagination(); } }
    }

    // ── Stats ────────────────────────────────────────────────────────────────
    int totalDepartments;
    public int TotalDepartments { get => totalDepartments; set => SetProperty(ref totalDepartments, value); }

    int activeDepartmentsCount;
    public int ActiveDepartmentsCount { get => activeDepartmentsCount; set => SetProperty(ref activeDepartmentsCount, value); }

    int pendingCount;
    public int PendingCount { get => pendingCount; set => SetProperty(ref pendingCount, value); }

    // ── Add/Edit form ────────────────────────────────────────────────────────
    string editName = string.Empty;
    public string EditName { get => editName; set => SetProperty(ref editName, value); }

    string? editId;
    public string? EditId { get => editId; set => SetProperty(ref editId, value); }

    string? selectedFacultyId;
    public string? SelectedFacultyId { get => selectedFacultyId; set => SetProperty(ref selectedFacultyId, value); }

    FacultyDto? selectedFaculty;
    public FacultyDto? SelectedFaculty
    {
        get => selectedFaculty;
        set { if (SetProperty(ref selectedFaculty, value)) SelectedFacultyId = value?.Id; }
    }

    bool isEditing;
    public bool IsEditing { get => isEditing; set => SetProperty(ref isEditing, value); }

    string errorMessage = string.Empty;
    public string ErrorMessage { get => errorMessage; set => SetProperty(ref errorMessage, value); }

    bool isEmpty;
    public bool IsEmpty { get => isEmpty; set => SetProperty(ref isEmpty, value); }

    bool isEmptySearch;
    public bool IsEmptySearch { get => isEmptySearch; set => SetProperty(ref isEmptySearch, value); }

    // ── Commands ─────────────────────────────────────────────────────────────
    public ICommand LoadCommand { get; }
    public ICommand SearchCommand { get; }
    public ICommand NextPageCommand { get; }
    public ICommand PrevPageCommand { get; }
    public ICommand AddDepartmentCommand { get; }
    public ICommand EditDepartmentCommand { get; }
    public ICommand DeleteDepartmentCommand { get; }
    public ICommand SaveCommand { get; }
    public ICommand CancelEditCommand { get; }
    public ICommand ExportCommand { get; }
    public ICommand RefreshCommand { get; }

    public DepartmentsViewModel(IAcademicService academic, IRefreshCoordinator refresh, ILogger<DepartmentsViewModel> logger)
    {
        _academic = academic;
        _refresh = refresh;
        _logger = logger;

        LoadCommand = new Command(async () => await LoadAsync());
        SearchCommand = new Command(() => ApplyFilterAndPagination());
        NextPageCommand = new Command(() => { if (CurrentPage < TotalPages) { CurrentPage++; ApplyFilterAndPagination(); } });
        PrevPageCommand = new Command(() => { if (CurrentPage > 1) { CurrentPage--; ApplyFilterAndPagination(); } });
        AddDepartmentCommand = new Command(() => { EditId = null; EditName = string.Empty; SelectedFaculty = null; IsEditing = true; });
        EditDepartmentCommand = new Command<DepartmentDto>(d =>
        {
            EditId = d.Id;
            EditName = d.Name;
            SelectedFaculty = Faculties.FirstOrDefault(f => f.Id == d.FacultyId);
            IsEditing = true;
        });
        DeleteDepartmentCommand = new Command<DepartmentDto>(async d => await DeleteAsync(d));
        SaveCommand = new Command(async () => await SaveAsync());
        CancelEditCommand = new Command(() => { IsEditing = false; EditName = string.Empty; EditId = null; SelectedFaculty = null; });
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
            // Load universities for cascade picker
            var unis = await _academic.GetUniversitiesAsync();
            Universities.Clear();
            foreach (var u in unis)
                Universities.Add(new UniversityDto { Id = u.Id, Name = u.Name });

            // Load all faculties for the add/edit form picker
            var faculties = await _academic.GetFacultyDetailsAsync();
            Faculties.Clear();
            foreach (var f in faculties)
                Faculties.Add(f);

            _allDepartments = await _academic.GetDepartmentDetailsAsync(null);
            TotalDepartments = _allDepartments.Count;
            ActiveDepartmentsCount = _allDepartments.Count(d => d.Status == "Active");
            PendingCount = _allDepartments.Count(d => d.Status != "Active");
            ApplyFilterAndPagination();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to load departments");
            ErrorMessage = "Failed to load departments.";
        }
        finally
        {
            IsBusy = false;
        }
    }

    private async Task OnCascadeUniversityChangedAsync(UniversityDto? university)
    {
        // Clear child
        SelectedCascadeFaculty = null;
        CascadeFaculties.Clear();
        IsFacultyPickerEnabled = false;

        if (university is null)
        {
            ApplyFilterAndPagination();
            return;
        }

        IsLoadingFaculties = true;
        try
        {
            var result = await _academic.GetFacultiesByUniversityAsync(university.Id);
            CascadeFaculties.Clear();
            foreach (var f in result.Data)
                CascadeFaculties.Add(f);
            IsFacultyPickerEnabled = CascadeFaculties.Count > 0;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to load faculties for university {Id}", university.Id);
        }
        finally
        {
            IsLoadingFaculties = false;
        }

        ApplyFilterAndPagination();
    }

    private Task OnCascadeFacultyChangedAsync(FacultyDto? faculty)
    {
        ApplyFilterAndPagination();
        return Task.CompletedTask;
    }

    private void ApplyFilterAndPagination()
    {
        var filtered = _allDepartments.AsEnumerable();

        // Cascade filter
        if (SelectedCascadeFaculty is not null)
            filtered = filtered.Where(d => d.FacultyId == SelectedCascadeFaculty.Id);
        else if (SelectedCascadeUniversity is not null)
            filtered = filtered.Where(d => d.UniversityId == SelectedCascadeUniversity.Id);

        if (!string.IsNullOrWhiteSpace(SearchQuery))
        {
            var q = SearchQuery.Trim().ToLowerInvariant();
            filtered = filtered.Where(d => d.Name.Contains(q, StringComparison.OrdinalIgnoreCase)
                || d.FacultyName.Contains(q, StringComparison.OrdinalIgnoreCase));
        }

        var list = filtered.OrderBy(d => d.Name).ToList();
        TotalPages = Math.Max(1, (int)Math.Ceiling(list.Count / (double)PageSize));
        if (CurrentPage > TotalPages) CurrentPage = TotalPages;

        var paged = list.Skip((CurrentPage - 1) * PageSize).Take(PageSize);
        Departments.Clear();
        foreach (var d in paged)
            Departments.Add(d);

        IsEmpty = _allDepartments.Count == 0;
        IsEmptySearch = _allDepartments.Count > 0 && list.Count == 0;
    }

    private async Task SaveAsync()
    {
        if (string.IsNullOrWhiteSpace(EditName))
        {
            ErrorMessage = "Department name is required.";
            return;
        }

        if (string.IsNullOrWhiteSpace(SelectedFacultyId))
        {
            ErrorMessage = "Please select a faculty before saving.";
            return;
        }

        IsBusy = true;
        ErrorMessage = string.Empty;

        try
        {
            if (EditId is null)
            {
                var result = await _academic.CreateDepartmentAsync(EditName.Trim(), SelectedFacultyId);
                if (result is null) { ErrorMessage = "A department with this name already exists in the selected faculty."; return; }
            }
            else
            {
                var result = await _academic.UpdateDepartmentAsync(EditId, EditName.Trim(), SelectedFacultyId);
                if (result is null) { ErrorMessage = "A department with this name already exists in the selected faculty."; return; }
            }

            IsEditing = false;
            EditName = string.Empty;
            EditId = null;
            SelectedFaculty = null;
            await _refresh.NotifyAsync(RefreshKeys.Departments);
            await LoadAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to save department");
            ErrorMessage = "Failed to save department.";
        }
        finally
        {
            IsBusy = false;
        }
    }

    private async Task DeleteAsync(DepartmentDto department)
    {
        bool confirm = await Shell.Current.DisplayAlert("Confirm Delete",
            $"Are you sure you want to delete '{department.Name}'?", "Delete", "Cancel");
        if (!confirm) return;

        IsBusy = true;
        try
        {
            var success = await _academic.DeleteDepartmentAsync(department.Id);
            if (!success)
            {
                await Shell.Current.DisplayAlert("Error", "This department cannot be deleted because it has programs assigned.", "OK");
                return;
            }
            await LoadAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to delete department");
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
            sb.AppendLine("Name,Faculty,UniversityName,Programs,Status");
            foreach (var d in _allDepartments)
                sb.AppendLine($"\"{d.Name}\",\"{d.FacultyName}\",\"{d.UniversityName}\",{d.ProgramsCount},{d.Status}");

            var filePath = Path.Combine(FileSystem.CacheDirectory, "departments_export.csv");
            await File.WriteAllTextAsync(filePath, sb.ToString());

            await Share.Default.RequestAsync(new ShareFileRequest
            {
                Title = "Export Departments",
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


