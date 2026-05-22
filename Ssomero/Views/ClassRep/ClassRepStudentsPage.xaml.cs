using Microsoft.Maui.Controls;
using Ssomero.ViewModels;

namespace Ssomero.Views.ClassRep;

public partial class ClassRepStudentsPage : ContentPage
{
    private readonly ClassRepStudentsViewModel _vm;

    public ClassRepStudentsPage(ClassRepStudentsViewModel vm)
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
