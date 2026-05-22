using Ssomero.ViewModels;

namespace Ssomero.Views.Student;

public partial class GroupChatPage : ContentPage
{
    private readonly GroupChatViewModel _vm;

    public GroupChatPage(GroupChatViewModel vm)
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
