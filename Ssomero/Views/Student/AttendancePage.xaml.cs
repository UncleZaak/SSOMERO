using Microsoft.Maui.Controls;
using Ssomero.ViewModels;

namespace Ssomero.Views.Student;

public partial class AttendancePage : ContentPage
{
    private readonly AttendanceViewModel _vm;

    public AttendancePage(AttendanceViewModel vm)
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
