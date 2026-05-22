using Microsoft.Maui.Controls;
using Ssomero.ViewModels;

namespace Ssomero.Views.Auth;

public partial class LoginPage : ContentPage
{
    private CancellationTokenSource? _pulseCts;

    public LoginPage(LoginViewModel vm)
    {
        InitializeComponent();
        BindingContext = vm;
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        _pulseCts?.Cancel();
        _pulseCts = new CancellationTokenSource();
        StartPulseAnimation(_pulseCts.Token);
    }

    protected override void OnDisappearing()
    {
        base.OnDisappearing();
        _pulseCts?.Cancel();
        _pulseCts?.Dispose();
        _pulseCts = null;

        if (BindingContext is LoginViewModel vm)
            vm.CancelPendingRequests();
    }

    private async void StartPulseAnimation(CancellationToken ct)
    {
        try
        {
            while (!ct.IsCancellationRequested)
            {
                await StatusDot.ScaleTo(1.4, 800, Easing.SinInOut);
                await StatusDot.ScaleTo(1.0, 800, Easing.SinInOut);
            }
        }
        catch (TaskCanceledException)
        {
        }
    }

    private async void OnLoginTapped(object? sender, TappedEventArgs e)
    {
        if (BindingContext is not LoginViewModel vm) return;

        await LoginButtonContainer.ScaleTo(0.96, 80, Easing.CubicIn);
        await LoginButtonContainer.ScaleTo(1.0, 80, Easing.CubicOut);

        if (vm.LoginCommand.CanExecute(null))
            vm.LoginCommand.Execute(null);
    }
}