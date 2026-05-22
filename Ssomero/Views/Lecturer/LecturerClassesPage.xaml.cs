using Ssomero.ViewModels;

namespace Ssomero.Views.Lecturer;

public partial class LecturerClassesPage : ContentPage
{
    private readonly LecturerClassesViewModel _vm;

    public LecturerClassesPage(LecturerClassesViewModel vm)
    {
        InitializeComponent();
        _vm = vm;
        BindingContext = _vm;
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        if (_vm.Classes.Count == 0)
            _vm.LoadCommand.Execute(null);
    }

    protected override void OnDisappearing()
    {
        base.OnDisappearing();
        _vm.CancelPendingRequests();
    }
}
