using Ssomero.ViewModels;

namespace Ssomero.Views.Student;

public partial class AttendanceMarkPage : ContentPage
{
    private readonly AttendanceMarkViewModel _vm;

    public AttendanceMarkPage(AttendanceMarkViewModel vm)
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
