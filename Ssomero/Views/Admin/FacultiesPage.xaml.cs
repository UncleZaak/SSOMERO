using Ssomero.ViewModels;

namespace Ssomero.Views.Admin;

public partial class FacultiesPage : ContentPage
{
    private readonly FacultiesViewModel _vm;

    public FacultiesPage(FacultiesViewModel vm)
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
