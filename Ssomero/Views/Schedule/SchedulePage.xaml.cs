using Microsoft.Maui.Controls;
using Ssomero.ViewModels;

namespace Ssomero.Views.Schedule;

public partial class SchedulePage : ContentPage
{
    private readonly ScheduleViewModel _vm;

    public SchedulePage(ScheduleViewModel vm)
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
