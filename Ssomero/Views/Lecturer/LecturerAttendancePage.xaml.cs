using Ssomero.ViewModels;

namespace Ssomero.Views.Lecturer;

public partial class LecturerAttendancePage : ContentPage
{
    private readonly LecturerAttendanceViewModel _vm;

    public LecturerAttendancePage(LecturerAttendanceViewModel vm)
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
