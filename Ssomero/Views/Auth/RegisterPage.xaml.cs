using Microsoft.Maui.Controls;
using Ssomero.ViewModels;

namespace Ssomero.Views.Auth;

public partial class RegisterPage : ContentPage
{
    private readonly RegisterViewModel _vm;

    public RegisterPage(RegisterViewModel vm)
    {
        InitializeComponent();
        _vm = vm;
        BindingContext = _vm;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await _vm.InitAsync();
    }

    private async void OnGoToLoginClicked(object? sender, EventArgs e)
    {
        await Shell.Current.GoToAsync("//LoginPage");
    }

    private void OnGoToStep3Clicked(object? sender, EventArgs e)
    {
        _vm.GoToStep3();
    }
}
