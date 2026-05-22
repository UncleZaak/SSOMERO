using System.Collections.ObjectModel;
using System.Windows.Input;
using Microsoft.Maui.Controls;
using Ssomero.Interfaces;
using Ssomero.Models;

namespace Ssomero.ViewModels;

[QueryProperty(nameof(ClassId), "classId")]
public class ClassRepStudentsViewModel : BaseViewModel
{
    private readonly IClassRepApiService _classRepApi;

    public ObservableCollection<ClassRepStudentModel> Students { get; } = [];

    private Guid _classId;
    public string ClassId
    {
        get => _classId.ToString();
        set
        {
            if (Guid.TryParse(value, out var g))
                _classId = g;
        }
    }

    private string _errorMessage = string.Empty;
    public string ErrorMessage
    {
        get => _errorMessage;
        set { SetProperty(ref _errorMessage, value); RaisePropertyChanged(nameof(HasError)); }
    }

    public bool HasError => !string.IsNullOrEmpty(ErrorMessage);
    public bool IsEmpty  => Students.Count == 0 && !IsBusy;

    public ICommand LoadCommand         { get; }
    public ICommand RefreshCommand      { get; }
    public ICommand RemoveStudentCommand { get; }

    public ClassRepStudentsViewModel(IClassRepApiService classRepApi)
    {
        _classRepApi         = classRepApi;
        Title                = "Students";
        LoadCommand          = new Command(async () => await LoadAsync());
        RefreshCommand       = new Command(async () => await LoadAsync());
        RemoveStudentCommand = new Command<ClassRepStudentModel>(async s => await RemoveStudentAsync(s));
    }

    public async Task LoadAsync()
    {
        if (_classId == Guid.Empty || IsBusy) return;
        IsBusy = true;
        ErrorMessage = string.Empty;
        RaisePropertyChanged(nameof(IsEmpty));

        var ct = CreateLinkedToken();
        try
        {
            var students = await _classRepApi.GetStudentsAsync(_classId, ct);
            Students.Clear();
            foreach (var s in students) Students.Add(s);
            if (Students.Count == 0) ErrorMessage = string.Empty;
        }
        catch (OperationCanceledException) { }
        catch (Exception)
        {
            ErrorMessage = "Could not load students. Please try again.";
        }
        finally
        {
            IsBusy = false;
            RaisePropertyChanged(nameof(IsEmpty));
        }
    }

    private async Task RemoveStudentAsync(ClassRepStudentModel student)
    {
        if (student is null) return;

        bool confirmed = await Shell.Current.DisplayAlert(
            "Remove Student",
            $"Remove {student.DisplayName} from this class?",
            "Remove", "Cancel");

        if (!confirmed) return;
        if (IsBusy) return;
        IsBusy = true;

        var ct = CreateLinkedToken();
        try
        {
            bool ok = await _classRepApi.RemoveStudentAsync(_classId, student.Id, ct);
            if (ok)
            {
                Students.Remove(student);
                await ShowSuccessToastAsync($"{student.DisplayName} removed.");
                RaisePropertyChanged(nameof(IsEmpty));
            }
            else
            {
                await ShowErrorToastAsync("Failed to remove student. Please try again.");
            }
        }
        catch (OperationCanceledException) { }
        catch (Exception)
        {
            await ShowErrorToastAsync("An unexpected error occurred.");
        }
        finally
        {
            IsBusy = false;
        }
    }
}
