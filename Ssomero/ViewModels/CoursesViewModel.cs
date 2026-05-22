using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows.Input;
using Microsoft.Maui.Controls;
using Ssomero.Interfaces;
using Ssomero.Models;

namespace Ssomero.ViewModels;

public class CoursesViewModel : BaseViewModel
{
    private readonly ICoursesService _courses;
    private DateTime _lastLoaded = DateTime.MinValue;
    private static readonly TimeSpan RefreshInterval = TimeSpan.FromSeconds(60);

    public ObservableCollection<CourseDto> Items { get; } = [];

    string errorMessage = string.Empty;
    public string ErrorMessage
    {
        get => errorMessage;
        set => SetProperty(ref errorMessage, value);
    }

    public ICommand LoadCommand { get; }
    public ICommand OpenCourseCommand { get; }

    public CoursesViewModel(ICoursesService courses)
    {
        _courses = courses;
        LoadCommand = new Command(async () => await LoadAsync(forceRefresh: true));
        OpenCourseCommand = new Command<string>(async (id) => await Shell.Current.GoToAsync($"course-detail?courseId={id}"));
    }

    public async Task LoadAsync(bool forceRefresh = false)
    {
        if (IsBusy) return;
        if (!forceRefresh && DateTime.UtcNow - _lastLoaded < RefreshInterval) return;

        IsBusy = true;
        ErrorMessage = string.Empty;
        try
        {
            Items.Clear();
            var list = await _courses.GetCoursesAsync();
            foreach (var c in list)
                Items.Add(c);

            _lastLoaded = DateTime.UtcNow;
        }
        catch (Exception ex)
        {
            ErrorMessage = "Failed to load courses. " + ex.Message;
        }
        finally
        {
            IsBusy = false;
        }
    }
}