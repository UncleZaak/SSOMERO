using System.Threading.Tasks;
using Ssomero.Interfaces;
using Ssomero.Models;

namespace Ssomero.ViewModels;

public class CourseDetailViewModel : BaseViewModel
{
    private readonly ICoursesService _courses;
    private CourseDto _course = new();
    public CourseDto Course
    {
        get => _course;
        private set => SetProperty(ref _course, value);
    }

    public CourseDetailViewModel(ICoursesService courses)
    {
        _courses = courses;
    }

    public async Task LoadAsync(string id)
    {
        if (string.IsNullOrEmpty(id)) return;
        var c = await _courses.GetCourseAsync(id);
        Course = c ?? new CourseDto();
        Title = Course.Name;
    }
}
