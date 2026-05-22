using Ssomero.ViewModels;

namespace Ssomero.Views.Lecturer;

public partial class LecturerMaterialsPage : ContentPage
{
    private readonly LecturerMaterialsViewModel _vm;

    public LecturerMaterialsPage(LecturerMaterialsViewModel vm)
    {
        InitializeComponent();
        _vm = vm;
        BindingContext = _vm;
    }

    protected override void OnDisappearing()
    {
        base.OnDisappearing();
        _vm.CancelPendingRequests();
    }
}
