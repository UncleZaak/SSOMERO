using Ssomero.ViewModels;

namespace Ssomero.Views.Profile;

public partial class ChangePasswordPage : ContentPage
{
    public ChangePasswordPage(ChangePasswordViewModel vm)
    {
        InitializeComponent();
        BindingContext = vm;
    }

    protected override void OnDisappearing()
    {
        base.OnDisappearing();
        if (BindingContext is ChangePasswordViewModel vm)
            vm.CancelPendingRequests();
    }
}
