using Ssomero.ViewModels;

namespace Ssomero.Views.Admin;

public partial class DepartmentsPage : ContentPage
{
    private readonly DepartmentsViewModel _vm;

    public DepartmentsPage(DepartmentsViewModel vm)
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
}
