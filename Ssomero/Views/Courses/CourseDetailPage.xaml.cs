using Microsoft.Maui.Controls;
using Ssomero.ViewModels;

namespace Ssomero.Views.Courses;

[QueryProperty("CourseId", "courseId")]
public partial class CourseDetailPage : ContentPage
{
    public string CourseId { get; set; } = string.Empty;
    private readonly CourseDetailViewModel _vm;

    public CourseDetailPage(CourseDetailViewModel vm)
    {
        InitializeComponent();
        _vm = vm;
        BindingContext = _vm;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        if (!string.IsNullOrEmpty(CourseId))
            await _vm.LoadAsync(CourseId);
    }

    protected override void OnDisappearing()
    {
        base.OnDisappearing();
        _vm.CancelPendingRequests();
    }
}
