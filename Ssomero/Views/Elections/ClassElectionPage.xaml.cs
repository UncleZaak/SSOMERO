using Ssomero.ViewModels;

namespace Ssomero.Views.Elections;

public partial class ClassElectionPage : ContentPage
{
    private readonly ClassElectionViewModel _vm;

    public ClassElectionPage(ClassElectionViewModel vm)
    {
        InitializeComponent();
        _vm = vm;
        BindingContext = _vm;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        this.Opacity = 0;
        await this.FadeTo(1, 250, Easing.CubicOut);
        await _vm.LoadAsync();
    }

    protected override void OnDisappearing()
    {
        base.OnDisappearing();
        _vm.CancelPendingRequests();
    }
}
