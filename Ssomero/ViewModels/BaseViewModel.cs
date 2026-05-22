using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading;

namespace Ssomero.ViewModels;

public enum ToastType { Success, Error, Info }

public class BaseViewModel : INotifyPropertyChanged
{
    private CancellationTokenSource? _cts;
    private int _toastToken;

    // ── Toast ─────────────────────────────────────────────────────────────────
    string toastMessage = string.Empty;
    public string ToastMessage
    {
        get => toastMessage;
        private set => SetProperty(ref toastMessage, value);
    }

    bool toastIsVisible;
    public bool ToastIsVisible
    {
        get => toastIsVisible;
        private set => SetProperty(ref toastIsVisible, value);
    }

    Color toastColor = Colors.Green;
    public Color ToastColor
    {
        get => toastColor;
        private set => SetProperty(ref toastColor, value);
    }

    protected Task ShowSuccessToastAsync(string message) => ShowToastAsync(message, ToastType.Success);
    protected Task ShowErrorToastAsync(string message) => ShowToastAsync(message, ToastType.Error);
    protected Task ShowInfoToastAsync(string message) => ShowToastAsync(message, ToastType.Info);

    protected async Task ShowToastAsync(string message, ToastType type, int durationMs = 2800)
    {
        var token = Interlocked.Increment(ref _toastToken);
        ToastColor = type switch
        {
            ToastType.Error => Color.FromArgb("#EF4444"),
            ToastType.Info  => Color.FromArgb("#3B82F6"),
            _               => Color.FromArgb("#22C55E")
        };
        ToastMessage = message;
        ToastIsVisible = true;
        await Task.Delay(durationMs);
        if (_toastToken == token)
            ToastIsVisible = false;
    }

    bool isBusy;
    public bool IsBusy
    {
        get => isBusy;
        set { SetProperty(ref isBusy, value); }
    }

    string title = string.Empty;
    public string Title
    {
        get => title;
        set { SetProperty(ref title, value); }
    }

    /// <summary>
    /// Creates a new CancellationTokenSource, cancelling any previous one.
    /// Call from page Loaded / Appearing.
    /// </summary>
    protected CancellationToken CreateLinkedToken()
    {
        _cts?.Cancel();
        _cts?.Dispose();
        _cts = new CancellationTokenSource();
        return _cts.Token;
    }

    /// <summary>
    /// Cancels the current token. Call from page OnDisappearing.
    /// </summary>
    public void CancelPendingRequests()
    {
        _cts?.Cancel();
        _cts?.Dispose();
        _cts = null;
    }

    protected bool SetProperty<T>(ref T backingStore, T value, [CallerMemberName] string? propertyName = null)
    {
        if (EqualityComparer<T>.Default.Equals(backingStore, value)) return false;
        backingStore = value;
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        return true;
    }

    protected void RaisePropertyChanged(string propertyName)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    public event PropertyChangedEventHandler? PropertyChanged;
}