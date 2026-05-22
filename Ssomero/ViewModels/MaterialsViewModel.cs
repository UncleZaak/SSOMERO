using System.Collections.ObjectModel;
using System.Windows.Input;
using Microsoft.Maui.Controls;
using Ssomero.Interfaces;
using Ssomero.Models;

namespace Ssomero.ViewModels;

public class MaterialsViewModel : BaseViewModel
{
    private readonly IMaterialsService _materials;
    private readonly ICoursesService _courses;
    private List<StudyMaterialDto> _allItems = [];
    private DateTime _lastLoaded = DateTime.MinValue;
    private static readonly TimeSpan RefreshInterval = TimeSpan.FromMinutes(5);

    public ObservableCollection<StudyMaterialDto> Items { get; } = [];
    public ObservableCollection<string> CourseFilters { get; } = [];

    string errorMessage = string.Empty;
    public string ErrorMessage { get => errorMessage; set => SetProperty(ref errorMessage, value); }

    bool isEmpty;
    public bool IsEmpty { get => isEmpty; set => SetProperty(ref isEmpty, value); }

    string selectedFilter = "All";
    public string SelectedFilter
    {
        get => selectedFilter;
        set
        {
            if (SetProperty(ref selectedFilter, value))
                ApplyFilter();
        }
    }

    string searchQuery = string.Empty;
    public string SearchQuery
    {
        get => searchQuery;
        set
        {
            if (SetProperty(ref searchQuery, value))
                ApplyFilter();
        }
    }

    public ICommand LoadCommand { get; }
    public ICommand OpenMaterialCommand { get; }
    public ICommand FilterCommand { get; }

    public MaterialsViewModel(IMaterialsService materials, ICoursesService courses)
    {
        _materials = materials;
        _courses   = courses;
        LoadCommand     = new Command(async () => await LoadAsync(forceRefresh: true));
        OpenMaterialCommand = new Command<StudyMaterialDto>(async m => await OpenAsync(m));
        FilterCommand   = new Command<string>(f => SelectedFilter = f);
    }

    public async Task LoadAsync(bool forceRefresh = false)
    {
        if (IsBusy) return;
        if (!forceRefresh && DateTime.UtcNow - _lastLoaded < RefreshInterval) return;

        IsBusy = true;
        ErrorMessage = string.Empty;
        try
        {
            _allItems = (await _materials.GetMaterialsAsync()).ToList();

            // Build unique course filter list
            CourseFilters.Clear();
            CourseFilters.Add("All");
            foreach (var name in _allItems.Select(m => m.CourseName).Distinct().OrderBy(x => x))
                CourseFilters.Add(name);

            ApplyFilter();
            _lastLoaded = DateTime.UtcNow;
        }
        catch (Exception ex)
        {
            ErrorMessage = "Failed to load materials. " + ex.Message;
        }
        finally
        {
            IsBusy = false;
        }
    }

    private void ApplyFilter()
    {
        var filtered = _allItems.AsEnumerable();

        if (SelectedFilter != "All")
            filtered = filtered.Where(m => m.CourseName == SelectedFilter);

        if (!string.IsNullOrWhiteSpace(SearchQuery))
            filtered = filtered.Where(m =>
                m.Topic.Contains(SearchQuery, StringComparison.OrdinalIgnoreCase) ||
                m.FileName.Contains(SearchQuery, StringComparison.OrdinalIgnoreCase));

        Items.Clear();
        foreach (var item in filtered.OrderByDescending(m => m.UploadedAt))
            Items.Add(item);

        IsEmpty = Items.Count == 0;
    }

    private static async Task OpenAsync(StudyMaterialDto m)
    {
        if (string.IsNullOrWhiteSpace(m.FileUrl)) return;
        try
        {
            await Browser.Default.OpenAsync(m.FileUrl, BrowserLaunchMode.SystemPreferred);
        }
        catch
        {
            await Shell.Current.DisplayAlert("Error", "Could not open the material.", "OK");
        }
    }
}
