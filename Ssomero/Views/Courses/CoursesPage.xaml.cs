using Microsoft.Maui.Controls;
using Ssomero.ViewModels;

namespace Ssomero.Views.Courses;

public partial class CoursesPage : ContentPage
{
    private readonly CoursesViewModel _vm;

    public CoursesPage(CoursesViewModel vm)
    {
        InitializeComponent();
        _vm = vm;
        BindingContext = _vm;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await _vm.LoadAsync();
    }

    protected override void OnDisappearing()
    {
        base.OnDisappearing();
        _vm.CancelPendingRequests();
    }
}