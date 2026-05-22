using Ssomero.ViewModels;

namespace Ssomero.Views.Lecturer;

public partial class LecturerClassDetailsPage : ContentPage
{
    private readonly LecturerClassDetailsViewModel _vm;

    public LecturerClassDetailsPage(LecturerClassDetailsViewModel vm)
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
