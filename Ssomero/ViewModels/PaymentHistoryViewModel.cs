using System.Collections.ObjectModel;
using System.Windows.Input;
using Microsoft.Extensions.Logging;
using Microsoft.Maui.Controls;
using Ssomero.Interfaces;
using Ssomero.Models;

namespace Ssomero.ViewModels;

public class PaymentHistoryViewModel : BaseViewModel
{
    private readonly IPaymentsService _payments;
    private readonly ILogger<PaymentHistoryViewModel> _logger;

    public ObservableCollection<PaymentHistoryDto> Items { get; } = [];

    bool isEmpty;
    public bool IsEmpty { get => isEmpty; private set => SetProperty(ref isEmpty, value); }

    string errorMessage = string.Empty;
    public string ErrorMessage { get => errorMessage; set => SetProperty(ref errorMessage, value); }

    bool hasError;
    public bool HasError { get => hasError; private set => SetProperty(ref hasError, value); }

    public ICommand LoadCommand { get; }
    public ICommand ViewReceiptCommand { get; }

    public PaymentHistoryViewModel(IPaymentsService payments, ILogger<PaymentHistoryViewModel> logger)
    {
        _payments = payments;
        _logger   = logger;

        LoadCommand         = new Command(async () => await LoadAsync());
        ViewReceiptCommand  = new Command<PaymentHistoryDto>(OnViewReceipt);
    }

    public async Task LoadAsync()
    {
        if (IsBusy) return;
        IsBusy = true;
        HasError = false;
        ErrorMessage = string.Empty;
        Items.Clear();

        try
        {
            var ct = CreateLinkedToken();
            var history = await _payments.GetHistoryAsync(ct);

            foreach (var item in history)
                Items.Add(item);

            IsEmpty = Items.Count == 0;
        }
        catch (OperationCanceledException) { }
        catch (Exception ex)
        {
            _logger.LogError(ex, "LoadAsync failed in PaymentHistoryViewModel");
            HasError = true;
            ErrorMessage = "Failed to load payment history. Please try again.";
        }
        finally { IsBusy = false; }
    }

    private static void OnViewReceipt(PaymentHistoryDto? item)
    {
        if (item?.ReceiptUrl is null)
        {
            Shell.Current.DisplayAlert("No Receipt", "No receipt is available for this transaction.", "OK");
            return;
        }

        Microsoft.Maui.ApplicationModel.Launcher.OpenAsync(new Uri(item.ReceiptUrl));
    }
}
