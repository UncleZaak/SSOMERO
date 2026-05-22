using Microsoft.Maui.Controls;
using Ssomero.ViewModels;

namespace Ssomero.Views.Student;

public partial class ClassesPage : ContentPage
{
    private readonly ClassesViewModel _vm;

    public ClassesPage(ClassesViewModel vm)
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
