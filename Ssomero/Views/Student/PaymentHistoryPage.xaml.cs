using Ssomero.ViewModels;

namespace Ssomero.Views.Student;

public partial class PaymentHistoryPage : ContentPage
{
    private readonly PaymentHistoryViewModel _vm;

    public PaymentHistoryPage(PaymentHistoryViewModel vm)
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
