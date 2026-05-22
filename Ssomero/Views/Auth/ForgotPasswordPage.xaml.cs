using Ssomero.ViewModels;

namespace Ssomero.Views.Auth;

public partial class ForgotPasswordPage : ContentPage
{
    public ForgotPasswordPage(ForgotPasswordViewModel vm)
    {
        InitializeComponent();
        BindingContext = vm;
    }

    protected override void OnDisappearing()
    {
        base.OnDisappearing();
        if (BindingContext is ForgotPasswordViewModel vm)
            vm.CancelPendingRequests();
    }
}
