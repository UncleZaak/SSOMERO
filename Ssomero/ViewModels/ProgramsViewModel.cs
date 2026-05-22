using System.Collections.ObjectModel;
using System.Text;
using System.Windows.Input;
using Microsoft.Extensions.Logging;
using Ssomero.Interfaces;
using Ssomero.Models;

namespace Ssomero.ViewModels;

public class ProgramsViewModel : BaseViewModel
{
    private readonly IAcademicService _academic;
    private readonly IRefreshCoordinator _refresh;
    private readonly ILogger<ProgramsViewModel> _logger;

    private List<ProgramDto> _allPrograms = [];

    public ObservableCollection<ProgramDto> Programs { get; } = [];

    // -- Cascade pickers ------------------------------------------------------
    public ObservableCollection<UniversityDto> Universities { get; } = [];
    public ObservableCollection<FacultyDto> CascadeFaculties { get; } = [];
    public ObservableCollection<DepartmentDto> CascadeDepartments { get; } = [];
    // Form picker — all departments
    public ObservableCollection<DepartmentDto> Departments { get; } = [];

    UniversityDto? _selectedCascadeUniversity;
    public UniversityDto? SelectedCascadeUniversity
    {
        get => _selectedCascadeUniversity;
        set { if (SetProperty(ref _selectedCascadeUniversity, value)) _ = OnCascadeUniversityChangedAsync(value); }
    }

    FacultyDto? _selectedCascadeFaculty;
    public FacultyDto? SelectedCascadeFaculty
    {
        get => _selectedCascadeFaculty;
        set { if (SetProperty(ref _selectedCascadeFaculty, value)) _ = OnCascadeFacultyChangedAsync(value); }
    }

    DepartmentDto? _selectedCascadeDepartment;
    public DepartmentDto? SelectedCascadeDepartment
    {
        get => _selectedCascadeDepartment;
        set { if (SetProperty(ref _selectedCascadeDepartment, value)) ApplyFilterAndPagination(); }
    }

    bool _isFacultyPickerEnabled;
    public bool IsFacultyPickerEnabled { get => _isFacultyPickerEnabled; set => SetProperty(ref _isFacultyPickerEnabled, value); }

    bool _isDepartmentPickerEnabled;
    public bool IsDepartmentPickerEnabled { get => _isDepartmentPickerEnabled; set => SetProperty(ref _isDepartmentPickerEnabled, value); }

    bool _isLoadingFaculties;
    public bool IsLoadingFaculties { get => _isLoadingFaculties; set => SetProperty(ref _isLoadingFaculties, value); }

    bool _isLoadingDepartments;
    public bool IsLoadingDepartments { get => _isLoadingDepartments; set => SetProperty(ref _isLoadingDepartments, value); }

    // -- Search & Pagination --------------------------------------------------
    string searchQuery = string.Empty;
    public string SearchQuery
    {
        get => searchQuery;
        set { if (SetProperty(ref searchQuery, value)) ApplyFilterAndPagination(); }
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

    // -- Stats ----------------------------------------------------------------
    int totalPrograms;
    public int TotalPrograms { get => totalPrograms; set => SetProperty(ref totalPrograms, value); }

    int activeProgramsCount;
    public int ActiveProgramsCount { get => activeProgramsCount; set => SetProperty(ref activeProgramsCount, value); }

    int pendingCount;
    public int PendingCount { get => pendingCount; set => SetProperty(ref pendingCount, value); }

    // -- Add/Edit form --------------------------------------------------------
    string editName = string.Empty;
    public string EditName { get => editName; set => SetProperty(ref editName, value); }

    string? editId;
    public string? EditId { get => editId; set => SetProperty(ref editId, value); }

    int editDurationSemesters = 8;
    public int EditDurationSemesters { get => editDurationSemesters; set => SetProperty(ref editDurationSemesters, value); }

    string? selectedDepartmentId;
    public string? SelectedDepartmentId { get => selectedDepartmentId; set => SetProperty(ref selectedDepartmentId, value); }

    DepartmentDto? selectedDepartment;
    public DepartmentDto? SelectedDepartment
    {
        get => selectedDepartment;
        set { if (SetProperty(ref selectedDepartment, value)) SelectedDepartmentId = value?.Id; }
    }

    bool isEditing;
    public bool IsEditing { get => isEditing; set => SetProperty(ref isEditing, value); }

    string errorMessage = string.Empty;
    public string ErrorMessage { get => errorMessage; set => SetProperty(ref errorMessage, value); }

    bool isEmpty;
    public bool IsEmpty { get => isEmpty; set => SetProperty(ref isEmpty, value); }

    bool isEmptySearch;
    public bool IsEmptySearch { get => isEmptySearch; set => SetProperty(ref isEmptySearch, value); }

    // -- Commands -------------------------------------------------------------
    public ICommand LoadCommand { get; }
    public ICommand SearchCommand { get; }
    public ICommand NextPageCommand { get; }
    public ICommand PrevPageCommand { get; }
    public ICommand AddProgramCommand { get; }
    public ICommand EditProgramCommand { get; }
    public ICommand DeleteProgramCommand { get; }
    public ICommand SaveCommand { get; }
    public ICommand CancelEditCommand { get; }
    public ICommand ExportCommand { get; }
    public ICommand RefreshCommand { get; }

    public ProgramsViewModel(IAcademicService academic, IRefreshCoordinator refresh, ILogger<ProgramsViewModel> logger)
    {
        _academic = academic;
        _refresh = refresh;
        _logger = logger;

        LoadCommand = new Command(async () => await LoadAsync());
        SearchCommand = new Command(() => ApplyFilterAndPagination());
        NextPageCommand = new Command(() => { if (CurrentPage < TotalPages) { CurrentPage++; ApplyFilterAndPagination(); } });
        PrevPageCommand = new Command(() => { if (CurrentPage > 1) { CurrentPage--; ApplyFilterAndPagination(); } });
        AddProgramCommand = new Command(() => { EditId = null; EditName = string.Empty; EditDurationSemesters = 8; SelectedDepartment = null; IsEditing = true; });
        EditProgramCommand = new Command<ProgramDto>(p =>
        {
            EditId = p.Id;
            EditName = p.Name;
            EditDurationSemesters = p.DurationSemesters;
            SelectedDepartment = Departments.FirstOrDefault(d => d.Id == p.DepartmentId);
            IsEditing = true;
        });
        DeleteProgramCommand = new Command<ProgramDto>(async p => await DeleteAsync(p));
        SaveCommand = new Command(async () => await SaveAsync());
        CancelEditCommand = new Command(() => { IsEditing = false; EditName = string.Empty; EditId = null; SelectedDepartment = null; EditDurationSemesters = 8; });
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
            var unis = await _academic.GetUniversitiesAsync();
            Universities.Clear();
            foreach (var u in unis)
                Universities.Add(new UniversityDto { Id = u.Id, Name = u.Name });

            var depts = await _academic.GetDepartmentDetailsAsync(null);
            Departments.Clear();
            foreach (var d in depts)
                Departments.Add(d);

            _allPrograms = await _academic.GetProgramDetailsAsync(null);
            TotalPrograms = _allPrograms.Count;
            ActiveProgramsCount = _allPrograms.Count(p => p.Status == "Active");
            PendingCount = _allPrograms.Count(p => p.Status != "Active");
            ApplyFilterAndPagination();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to load programs");
            ErrorMessage = "Failed to load programs.";
        }
        finally
        {
            IsBusy = false;
        }
    }

    private async Task OnCascadeUniversityChangedAsync(UniversityDto? university)
    {
        SelectedCascadeFaculty = null;
        SelectedCascadeDepartment = null;
        CascadeFaculties.Clear();
        CascadeDepartments.Clear();
        IsFacultyPickerEnabled = false;
        IsDepartmentPickerEnabled = false;

        if (university is null) { ApplyFilterAndPagination(); return; }

        IsLoadingFaculties = true;
        try
        {
            var result = await _academic.GetFacultiesByUniversityAsync(university.Id);
            foreach (var f in result.Data)
                CascadeFaculties.Add(f);
            IsFacultyPickerEnabled = CascadeFaculties.Count > 0;
        }
        catch (Exception ex) { _logger.LogError(ex, "Failed to load faculties for university {Id}", university.Id); }
        finally { IsLoadingFaculties = false; }

        ApplyFilterAndPagination();
    }

    private async Task OnCascadeFacultyChangedAsync(FacultyDto? faculty)
    {
        SelectedCascadeDepartment = null;
        CascadeDepartments.Clear();
        IsDepartmentPickerEnabled = false;

        if (faculty is null) { ApplyFilterAndPagination(); return; }

        IsLoadingDepartments = true;
        try
        {
            var result = await _academic.GetDepartmentsByFacultyAsync(faculty.Id);
            foreach (var d in result.Data)
                CascadeDepartments.Add(d);
            IsDepartmentPickerEnabled = CascadeDepartments.Count > 0;
        }
        catch (Exception ex) { _logger.LogError(ex, "Failed to load departments for faculty {Id}", faculty.Id); }
        finally { IsLoadingDepartments = false; }

        ApplyFilterAndPagination();
    }

    private void ApplyFilterAndPagination()
    {
        var filtered = _allPrograms.AsEnumerable();

        if (SelectedCascadeDepartment is not null)
            filtered = filtered.Where(p => p.DepartmentId == SelectedCascadeDepartment.Id);
        else if (SelectedCascadeFaculty is not null)
            filtered = filtered.Where(p => p.FacultyId == SelectedCascadeFaculty.Id);
        else if (SelectedCascadeUniversity is not null)
            filtered = filtered.Where(p => p.UniversityId == SelectedCascadeUniversity.Id);

        if (!string.IsNullOrWhiteSpace(SearchQuery))
        {
            var q = SearchQuery.Trim().ToLowerInvariant();
            filtered = filtered.Where(p => p.Name.Contains(q, StringComparison.OrdinalIgnoreCase)
                || p.DepartmentName.Contains(q, StringComparison.OrdinalIgnoreCase));
        }

        var list = filtered.OrderBy(p => p.Name).ToList();
        TotalPages = Math.Max(1, (int)Math.Ceiling(list.Count / (double)PageSize));
        if (CurrentPage > TotalPages) CurrentPage = TotalPages;

        var paged = list.Skip((CurrentPage - 1) * PageSize).Take(PageSize);
        Programs.Clear();
        foreach (var p in paged)
            Programs.Add(p);

        IsEmpty = _allPrograms.Count == 0;
        IsEmptySearch = _allPrograms.Count > 0 && list.Count == 0;
    }

    private async Task SaveAsync()
    {
        if (string.IsNullOrWhiteSpace(EditName))
        {
            ErrorMessage = "Program name is required.";
            return;
        }

        if (string.IsNullOrWhiteSpace(SelectedDepartmentId))
        {
            ErrorMessage = "Please select a department before saving.";
            return;
        }

        if (EditDurationSemesters < 1 || EditDurationSemesters > 20)
        {
            ErrorMessage = "Duration must be between 1 and 20 semesters.";
            return;
        }

        IsBusy = true;
        ErrorMessage = string.Empty;

        try
        {
            if (EditId is null)
            {
                var result = await _academic.CreateProgramAsync(EditName.Trim(), SelectedDepartmentId, EditDurationSemesters);
                if (result is null) { ErrorMessage = "A program with this name already exists in the selected department."; return; }
            }
            else
            {
                var result = await _academic.UpdateProgramAsync(EditId, EditName.Trim(), SelectedDepartmentId, EditDurationSemesters);
                if (result is null) { ErrorMessage = "A program with this name already exists in the selected department."; return; }
            }

            IsEditing = false;
            EditName = string.Empty;
            EditId = null;
            SelectedDepartment = null;
            EditDurationSemesters = 8;
            await _refresh.NotifyAsync(RefreshKeys.Programs);
            await LoadAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to save program");
            ErrorMessage = "Failed to save program.";
        }
        finally
        {
            IsBusy = false;
        }
    }

    private async Task DeleteAsync(ProgramDto program)
    {
        bool confirm = await Shell.Current.DisplayAlert("Confirm Delete",
            $"Are you sure you want to delete '{program.Name}'?", "Delete", "Cancel");
        if (!confirm) return;

        IsBusy = true;
        try
        {
            var success = await _academic.DeleteProgramAsync(program.Id);
            if (!success)
            {
                await Shell.Current.DisplayAlert("Error", "This program cannot be deleted because curriculum entries exist.", "OK");
                return;
            }
            await LoadAsync();
        }
        catch (Exception ex) { _logger.LogError(ex, "Failed to delete program"); }
        finally { IsBusy = false; }
    }

    private async Task ExportAsync()
    {
        try
        {
            var sb = new StringBuilder();
            sb.AppendLine("Name,Department,Faculty,University,DurationSemesters,Curriculum,Status");
            foreach (var p in _allPrograms)
                sb.AppendLine($"\"{p.Name}\",\"{p.DepartmentName}\",\"{p.FacultyName}\",\"{p.UniversityName}\",{p.DurationSemesters},{p.CurriculumCount},{p.Status}");

            var filePath = Path.Combine(FileSystem.CacheDirectory, "programs_export.csv");
            await File.WriteAllTextAsync(filePath, sb.ToString());

            await Share.Default.RequestAsync(new ShareFileRequest
            {
                Title = "Export Programs",
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
