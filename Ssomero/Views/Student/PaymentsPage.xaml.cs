using Ssomero.ViewModels;

namespace Ssomero.Views.Student;

public partial class PaymentsPage : ContentPage
{
    private readonly PaymentsViewModel _vm;

    public PaymentsPage(PaymentsViewModel vm)
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
