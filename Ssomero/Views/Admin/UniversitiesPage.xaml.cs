using Ssomero.ViewModels;

namespace Ssomero.Views.Admin;

public partial class UniversitiesPage : ContentPage
{
    private readonly UniversitiesViewModel _vm;

    public UniversitiesPage(UniversitiesViewModel vm)
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
