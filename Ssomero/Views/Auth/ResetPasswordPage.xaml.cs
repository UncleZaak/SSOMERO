using Ssomero.ViewModels;

namespace Ssomero.Views.Auth;

public partial class ResetPasswordPage : ContentPage
{
    public ResetPasswordPage(ResetPasswordViewModel vm)
    {
        InitializeComponent();
        BindingContext = vm;
    }

    protected override void OnDisappearing()
    {
        base.OnDisappearing();
        if (BindingContext is ResetPasswordViewModel vm)
            vm.CancelPendingRequests();
    }
}
