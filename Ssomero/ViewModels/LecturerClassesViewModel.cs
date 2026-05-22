using System.Collections.ObjectModel;
using System.Windows.Input;
using Ssomero.Interfaces;
using Ssomero.Models;

namespace Ssomero.ViewModels;

public class LecturerClassesViewModel : BaseViewModel
{
    private readonly ILecturerApiService _lecturer;

    public ObservableCollection<LecturerClassDto> Classes { get; } = [];

    public ICommand LoadCommand { get; }
    public ICommand RefreshCommand { get; }
    public ICommand SelectClassCommand { get; }

    bool isEmpty;
    public bool IsEmpty { get => isEmpty; set => SetProperty(ref isEmpty, value); }

    string errorMessage = string.Empty;
    public string ErrorMessage { get => errorMessage; set => SetProperty(ref errorMessage, value); }

    bool hasError;
    public bool HasError { get => hasError; set => SetProperty(ref hasError, value); }

    public LecturerClassesViewModel(ILecturerApiService lecturer)
    {
        _lecturer = lecturer;

        LoadCommand    = new Command(async () => await LoadAsync());
        RefreshCommand = new Command(async () => await LoadAsync());
        SelectClassCommand = new Command<LecturerClassDto>(async cls =>
        {
            if (cls is null) return;
            await Shell.Current.GoToAsync("lecturer-class-details",
                new Dictionary<string, object> { ["ClassId"] = cls.Id, ["ClassName"] = cls.Name });
        });
    }

    public async Task LoadAsync()
    {
        if (IsBusy) return;
        IsBusy   = true;
        HasError = false;
        ErrorMessage = string.Empty;
        try
        {
            var ct = CreateLinkedToken();
            var result = await _lecturer.GetClassesAsync(ct);
            Classes.Clear();
            foreach (var c in result)
                Classes.Add(c);
            IsEmpty = Classes.Count == 0;
        }
        catch (OperationCanceledException) { }
        catch (Exception ex)
        {
            HasError     = true;
            ErrorMessage = "Could not load classes. Tap to retry.";
            await ShowErrorToastAsync("Failed to load classes.");
            _ = ex;
        }
        finally { IsBusy = false; }
    }
}
