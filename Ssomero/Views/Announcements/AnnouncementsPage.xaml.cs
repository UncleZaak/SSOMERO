using Microsoft.Maui.Controls;
using Ssomero.ViewModels;

namespace Ssomero.Views.Announcements;

public partial class AnnouncementsPage : ContentPage
{
    private readonly AnnouncementsViewModel _vm;

    public AnnouncementsPage(AnnouncementsViewModel vm)
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

    protected override void OnDisappearing()
    {
        base.OnDisappearing();
        _vm.CancelPendingRequests();
    }
}