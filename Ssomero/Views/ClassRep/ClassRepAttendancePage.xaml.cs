using Microsoft.Maui.Controls;
using Ssomero.ViewModels;

namespace Ssomero.Views.ClassRep;

public partial class ClassRepAttendancePage : ContentPage
{
    private readonly ClassRepAttendanceViewModel _vm;

    public ClassRepAttendancePage(ClassRepAttendanceViewModel vm)
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
