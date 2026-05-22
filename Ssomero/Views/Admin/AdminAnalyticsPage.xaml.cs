using Ssomero.ViewModels;

namespace Ssomero.Views.Admin;

public partial class AdminAnalyticsPage : ContentPage
{
    private readonly AdminAnalyticsViewModel _vm;

    public AdminAnalyticsPage(AdminAnalyticsViewModel vm)
    {
        InitializeComponent();
        _vm = vm;
        BindingContext = _vm;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        this.Opacity = 0;
        await this.FadeToAsync(1, 220, Easing.CubicOut);
        await _vm.LoadAsync();
    }

    protected override void OnDisappearing()
    {
        base.OnDisappearing();
        _vm.CancelPendingRequests();
    }
}
