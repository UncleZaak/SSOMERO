using Ssomero.ViewModels;

namespace Ssomero.Views.Admin;

public partial class ProgramsPage : ContentPage
{
    private readonly ProgramsViewModel _vm;

    public ProgramsPage(ProgramsViewModel vm)
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
