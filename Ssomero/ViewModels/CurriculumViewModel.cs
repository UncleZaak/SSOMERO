using System.Collections.ObjectModel;
using System.Text;
using System.Windows.Input;
using Microsoft.Extensions.Logging;
using Ssomero.Interfaces;
using Ssomero.Models;

namespace Ssomero.ViewModels;

public class CurriculumViewModel : BaseViewModel
{
    private readonly IAcademicService _academic;
    private readonly IRefreshCoordinator _refresh;
    private readonly ILogger<CurriculumViewModel> _logger;

    private List<CurriculumDto> _allEntries = [];

    public ObservableCollection<CurriculumDto> Entries { get; } = [];
    public ObservableCollection<LookupItem> Semesters { get; } = [];

    // -- Cascade pickers ------------------------------------------------------
    public ObservableCollection<UniversityDto> Universities { get; } = [];
    public ObservableCollection<FacultyDto> CascadeFaculties { get; } = [];
    public ObservableCollection<DepartmentDto> CascadeDepartments { get; } = [];
    public ObservableCollection<ProgramDto> CascadePrograms { get; } = [];
    // Form picker — all programs
    public ObservableCollection<ProgramDto> Programs { get; } = [];

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
        set { if (SetProperty(ref _selectedCascadeDepartment, value)) _ = OnCascadeDepartmentChangedAsync(value); }
    }

    ProgramDto? _selectedCascadeProgram;
    public ProgramDto? SelectedCascadeProgram
    {
        get => _selectedCascadeProgram;
        set { if (SetProperty(ref _selectedCascadeProgram, value)) ApplyFilterAndPagination(); }
    }

    bool _isFacultyPickerEnabled;
    public bool IsFacultyPickerEnabled { get => _isFacultyPickerEnabled; set => SetProperty(ref _isFacultyPickerEnabled, value); }

    bool _isDepartmentPickerEnabled;
    public bool IsDepartmentPickerEnabled { get => _isDepartmentPickerEnabled; set => SetProperty(ref _isDepartmentPickerEnabled, value); }

    bool _isProgramPickerEnabled;
    public bool IsProgramPickerEnabled { get => _isProgramPickerEnabled; set => SetProperty(ref _isProgramPickerEnabled, value); }

    bool _isLoadingFaculties;
    public bool IsLoadingFaculties { get => _isLoadingFaculties; set => SetProperty(ref _isLoadingFaculties, value); }

    bool _isLoadingDepartments;
    public bool IsLoadingDepartments { get => _isLoadingDepartments; set => SetProperty(ref _isLoadingDepartments, value); }

    bool _isLoadingPrograms;
    public bool IsLoadingPrograms { get => _isLoadingPrograms; set => SetProperty(ref _isLoadingPrograms, value); }

    // -- Search, filter, pagination -------------------------------------------
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
    int totalEntries;
    public int TotalEntries { get => totalEntries; set => SetProperty(ref totalEntries, value); }

    int activeEntriesCount;
    public int ActiveEntriesCount { get => activeEntriesCount; set => SetProperty(ref activeEntriesCount, value); }

    int pendingCount;
    public int PendingCount { get => pendingCount; set => SetProperty(ref pendingCount, value); }

    // -- Add/Edit form --------------------------------------------------------
    string editCourseCode = string.Empty;
    public string EditCourseCode { get => editCourseCode; set => SetProperty(ref editCourseCode, value); }

    string editCourseName = string.Empty;
    public string EditCourseName { get => editCourseName; set => SetProperty(ref editCourseName, value); }

    int editYearOfStudy = 1;
    public int EditYearOfStudy { get => editYearOfStudy; set => SetProperty(ref editYearOfStudy, value); }

    string? editId;
    public string? EditId { get => editId; set => SetProperty(ref editId, value); }

    string? selectedProgramId;
    public string? SelectedProgramId { get => selectedProgramId; set => SetProperty(ref selectedProgramId, value); }

    ProgramDto? selectedProgram;
    public ProgramDto? SelectedProgram
    {
        get => selectedProgram;
        set { if (SetProperty(ref selectedProgram, value)) SelectedProgramId = value?.Id; }
    }

    string? selectedSemesterId;
    public string? SelectedSemesterId { get => selectedSemesterId; set => SetProperty(ref selectedSemesterId, value); }

    LookupItem? selectedSemester;
    public LookupItem? SelectedSemester
    {
        get => selectedSemester;
        set { if (SetProperty(ref selectedSemester, value)) SelectedSemesterId = value?.Id; }
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
    public ICommand AddEntryCommand { get; }
    public ICommand EditEntryCommand { get; }
    public ICommand DeleteEntryCommand { get; }
    public ICommand SaveCommand { get; }
    public ICommand CancelEditCommand { get; }
    public ICommand ExportCommand { get; }
    public ICommand RefreshCommand { get; }

    public CurriculumViewModel(IAcademicService academic, IRefreshCoordinator refresh, ILogger<CurriculumViewModel> logger)
    {
        _academic = academic;
        _refresh = refresh;
        _logger = logger;

        _refresh.Subscribe(RefreshKeys.Programs, async () => await LoadAsync());

        LoadCommand = new Command(async () => await LoadAsync());
        SearchCommand = new Command(() => ApplyFilterAndPagination());
        NextPageCommand = new Command(() => { if (CurrentPage < TotalPages) { CurrentPage++; ApplyFilterAndPagination(); } });
        PrevPageCommand = new Command(() => { if (CurrentPage > 1) { CurrentPage--; ApplyFilterAndPagination(); } });
        AddEntryCommand = new Command(() => { EditId = null; EditCourseCode = string.Empty; EditCourseName = string.Empty; EditYearOfStudy = 1; SelectedProgram = null; SelectedSemester = null; IsEditing = true; });
        EditEntryCommand = new Command<CurriculumDto>(c =>
        {
            EditId = c.Id;
            EditCourseCode = c.CourseCode;
            EditCourseName = c.CourseName;
            EditYearOfStudy = c.YearOfStudy;
            SelectedProgram = Programs.FirstOrDefault(p => p.Id == c.ProgramId);
            SelectedSemester = Semesters.FirstOrDefault(s => s.Id == c.SemesterId);
            IsEditing = true;
        });
        DeleteEntryCommand = new Command<CurriculumDto>(async c => await DeleteAsync(c));
        SaveCommand = new Command(async () => await SaveAsync());
        CancelEditCommand = new Command(() => { IsEditing = false; EditCourseCode = string.Empty; EditCourseName = string.Empty; EditId = null; SelectedProgram = null; SelectedSemester = null; EditYearOfStudy = 1; });
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

            var programs = await _academic.GetProgramDetailsAsync(null);
            Programs.Clear();
            foreach (var p in programs)
                Programs.Add(p);

            var semesters = await _academic.GetSemestersAsync();
            Semesters.Clear();
            foreach (var s in semesters)
                Semesters.Add(s);

            _allEntries = await _academic.GetCurriculumDetailsAsync(null, null);
            TotalEntries = _allEntries.Count;
            ActiveEntriesCount = _allEntries.Count(c => c.Status == "Active");
            PendingCount = _allEntries.Count(c => c.Status != "Active");
            ApplyFilterAndPagination();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to load curriculum");
            ErrorMessage = "Failed to load curriculum.";
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
        SelectedCascadeProgram = null;
        CascadeFaculties.Clear();
        CascadeDepartments.Clear();
        CascadePrograms.Clear();
        IsFacultyPickerEnabled = false;
        IsDepartmentPickerEnabled = false;
        IsProgramPickerEnabled = false;

        if (university is null) { ApplyFilterAndPagination(); return; }

        IsLoadingFaculties = true;
        try
        {
            var result = await _academic.GetFacultiesByUniversityAsync(university.Id);
            foreach (var f in result.Data) CascadeFaculties.Add(f);
            IsFacultyPickerEnabled = CascadeFaculties.Count > 0;
        }
        catch (Exception ex) { _logger.LogError(ex, "Failed to load faculties for university {Id}", university.Id); }
        finally { IsLoadingFaculties = false; }

        ApplyFilterAndPagination();
    }

    private async Task OnCascadeFacultyChangedAsync(FacultyDto? faculty)
    {
        SelectedCascadeDepartment = null;
        SelectedCascadeProgram = null;
        CascadeDepartments.Clear();
        CascadePrograms.Clear();
        IsDepartmentPickerEnabled = false;
        IsProgramPickerEnabled = false;

        if (faculty is null) { ApplyFilterAndPagination(); return; }

        IsLoadingDepartments = true;
        try
        {
            var result = await _academic.GetDepartmentsByFacultyAsync(faculty.Id);
            foreach (var d in result.Data) CascadeDepartments.Add(d);
            IsDepartmentPickerEnabled = CascadeDepartments.Count > 0;
        }
        catch (Exception ex) { _logger.LogError(ex, "Failed to load departments for faculty {Id}", faculty.Id); }
        finally { IsLoadingDepartments = false; }

        ApplyFilterAndPagination();
    }

    private async Task OnCascadeDepartmentChangedAsync(DepartmentDto? department)
    {
        SelectedCascadeProgram = null;
        CascadePrograms.Clear();
        IsProgramPickerEnabled = false;

        if (department is null) { ApplyFilterAndPagination(); return; }

        IsLoadingPrograms = true;
        try
        {
            var result = await _academic.GetProgramsByDepartmentAsync(department.Id);
            foreach (var p in result.Data) CascadePrograms.Add(p);
            IsProgramPickerEnabled = CascadePrograms.Count > 0;
        }
        catch (Exception ex) { _logger.LogError(ex, "Failed to load programs for department {Id}", department.Id); }
        finally { IsLoadingPrograms = false; }

        ApplyFilterAndPagination();
    }

    private void ApplyFilterAndPagination()
    {
        var filtered = _allEntries.AsEnumerable();

        if (SelectedCascadeProgram is not null)
            filtered = filtered.Where(c => c.ProgramId == SelectedCascadeProgram.Id);
        else if (SelectedCascadeDepartment is not null)
            filtered = filtered.Where(c => c.DepartmentName == SelectedCascadeDepartment.Name
                && c.UniversityName == SelectedCascadeDepartment.UniversityName);
        else if (SelectedCascadeFaculty is not null)
            filtered = filtered.Where(c => c.FacultyName == SelectedCascadeFaculty.Name
                && c.UniversityName == SelectedCascadeFaculty.UniversityName);
        else if (SelectedCascadeUniversity is not null)
            filtered = filtered.Where(c => c.UniversityName == SelectedCascadeUniversity.Name);

        if (!string.IsNullOrWhiteSpace(SearchQuery))
        {
            var q = SearchQuery.Trim().ToLowerInvariant();
            filtered = filtered.Where(c => c.CourseCode.Contains(q, StringComparison.OrdinalIgnoreCase)
                || c.CourseName.Contains(q, StringComparison.OrdinalIgnoreCase)
                || c.ProgramName.Contains(q, StringComparison.OrdinalIgnoreCase));
        }

        var list = filtered.OrderBy(c => c.CourseCode).ThenBy(c => c.CourseName).ToList();
        TotalPages = Math.Max(1, (int)Math.Ceiling(list.Count / (double)PageSize));
        if (CurrentPage > TotalPages) CurrentPage = TotalPages;

        var paged = list.Skip((CurrentPage - 1) * PageSize).Take(PageSize);
        Entries.Clear();
        foreach (var c in paged) Entries.Add(c);

        IsEmpty = _allEntries.Count == 0;
        IsEmptySearch = _allEntries.Count > 0 && list.Count == 0;
    }

    private async Task SaveAsync()
    {
        if (string.IsNullOrWhiteSpace(EditCourseCode))
        {
            ErrorMessage = "Course code is required.";
            return;
        }
        if (string.IsNullOrWhiteSpace(EditCourseName))
        {
            ErrorMessage = "Course name is required.";
            return;
        }
        if (string.IsNullOrWhiteSpace(SelectedProgramId))
        {
            ErrorMessage = "Please select a program before saving.";
            return;
        }
        if (string.IsNullOrWhiteSpace(SelectedSemesterId))
        {
            ErrorMessage = "Please select a semester.";
            return;
        }
        if (EditYearOfStudy < 1 || EditYearOfStudy > 10)
        {
            ErrorMessage = "Year of study must be between 1 and 10.";
            return;
        }

        IsBusy = true;
        ErrorMessage = string.Empty;

        try
        {
            if (EditId is null)
            {
                var result = await _academic.CreateCurriculumEntryAsync(SelectedProgramId, EditYearOfStudy, SelectedSemesterId, EditCourseCode.Trim(), EditCourseName.Trim());
                if (result is null) { ErrorMessage = "A curriculum entry with this course code already exists in the selected program."; return; }
            }
            else
            {
                var result = await _academic.UpdateCurriculumEntryAsync(EditId, SelectedProgramId, EditYearOfStudy, SelectedSemesterId, EditCourseCode.Trim(), EditCourseName.Trim());
                if (result is null) { ErrorMessage = "A curriculum entry with this course code already exists in the selected program."; return; }
            }

            IsEditing = false;
            EditCourseCode = string.Empty;
            EditCourseName = string.Empty;
            EditId = null;
            SelectedProgram = null;
            SelectedSemester = null;
            EditYearOfStudy = 1;
            await _refresh.NotifyAsync(RefreshKeys.Curriculum);
            await LoadAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to save curriculum entry");
            ErrorMessage = "Failed to save curriculum entry.";
        }
        finally { IsBusy = false; }
    }

    private async Task DeleteAsync(CurriculumDto entry)
    {
        bool confirm = await Shell.Current.DisplayAlert("Confirm Delete",
            $"Are you sure you want to delete '{entry.DisplayTitle}'?", "Delete", "Cancel");
        if (!confirm) return;

        IsBusy = true;
        try
        {
            var success = await _academic.DeleteCurriculumEntryAsync(entry.Id);
            if (!success)
            {
                await Shell.Current.DisplayAlert("Error", "Failed to delete curriculum entry.", "OK");
                return;
            }
            await LoadAsync();
        }
        catch (Exception ex) { _logger.LogError(ex, "Failed to delete curriculum entry"); }
        finally { IsBusy = false; }
    }

    private async Task ExportAsync()
    {
        try
        {
            var sb = new StringBuilder();
            sb.AppendLine("CourseCode,CourseName,Program,Department,Faculty,University,Year,Semester,Status");
            foreach (var c in _allEntries)
                sb.AppendLine($"\"{c.CourseCode}\",\"{c.CourseName}\",\"{c.ProgramName}\",\"{c.DepartmentName}\",\"{c.FacultyName}\",\"{c.UniversityName}\",{c.YearOfStudy},\"{c.SemesterName}\",{c.Status}");

            var filePath = Path.Combine(FileSystem.CacheDirectory, "curriculum_export.csv");
            await File.WriteAllTextAsync(filePath, sb.ToString());

            await Share.Default.RequestAsync(new ShareFileRequest
            {
                Title = "Export Curriculum",
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
