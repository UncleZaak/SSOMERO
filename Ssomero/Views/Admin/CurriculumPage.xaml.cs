using Ssomero.ViewModels;

namespace Ssomero.Views.Admin;

public partial class CurriculumPage : ContentPage
{
    private readonly CurriculumViewModel _vm;

    public CurriculumPage(CurriculumViewModel vm)
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
