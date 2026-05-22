using System.Windows.Input;
using Microsoft.Maui.Controls;
using Ssomero.Interfaces;
using Ssomero.Models;

namespace Ssomero.ViewModels;

public class ClassRepAttendanceViewModel : BaseViewModel
{
    private readonly IClassRepApiService _classRepApi;

    private ClassRepAttendanceSummaryModel? _summary;
    public ClassRepAttendanceSummaryModel? Summary
    {
        get => _summary;
        set => SetProperty(ref _summary, value);
    }

    private string _errorMessage = string.Empty;
    public string ErrorMessage
    {
        get => _errorMessage;
        set { SetProperty(ref _errorMessage, value); RaisePropertyChanged(nameof(HasError)); }
    }

    public bool HasError => !string.IsNullOrEmpty(ErrorMessage);

    public ICommand LoadCommand    { get; }
    public ICommand RefreshCommand { get; }

    public ClassRepAttendanceViewModel(IClassRepApiService classRepApi)
    {
        _classRepApi   = classRepApi;
        Title          = "Attendance Reports";
        LoadCommand    = new Command(async () => await LoadAsync());
        RefreshCommand = new Command(async () => await LoadAsync());
    }

    public async Task LoadAsync()
    {
        if (IsBusy) return;
        IsBusy = true;
        ErrorMessage = string.Empty;

        var ct = CreateLinkedToken();
        try
        {
            Summary = await _classRepApi.GetAttendanceSummaryAsync(ct);
            if (Summary is null)
                ErrorMessage = "Could not load attendance data.";
        }
        catch (OperationCanceledException) { }
        catch (Exception)
        {
            ErrorMessage = "An unexpected error occurred. Please try again.";
        }
        finally
        {
            IsBusy = false;
        }
    }
}
