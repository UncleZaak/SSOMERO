using Microsoft.Maui.Controls;
using Ssomero.ViewModels;

namespace Ssomero.Views.Auth;

public partial class LecturerRegisterPage : ContentPage
{
    private readonly LecturerRegisterViewModel _vm;

    public LecturerRegisterPage(LecturerRegisterViewModel vm)
    {
        InitializeComponent();
        _vm = vm;
        BindingContext = _vm;
    }

    private async void OnGoToLoginClicked(object? sender, EventArgs e)
    {
        await Shell.Current.GoToAsync("//LoginPage");
    }
}
