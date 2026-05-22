using System.Collections.ObjectModel;
using System.Windows.Input;
using Microsoft.Maui.Controls;
using Ssomero.Interfaces;
using Ssomero.Models;

namespace Ssomero.ViewModels;

[QueryProperty(nameof(SubclassId), "subclassId")]
public class ClassRepLecturersViewModel : BaseViewModel
{
    private readonly IClassRepApiService _classRepApi;

    public ObservableCollection<ClassRepLecturerModel> Lecturers { get; } = [];

    private Guid _subclassId;
    public string SubclassId
    {
        get => _subclassId.ToString();
        set
        {
            if (Guid.TryParse(value, out var g))
                _subclassId = g;
        }
    }

    private ClassRepLecturerModel? _selectedLecturer;
    public ClassRepLecturerModel? SelectedLecturer
    {
        get => _selectedLecturer;
        set => SetProperty(ref _selectedLecturer, value);
    }

    private string _errorMessage = string.Empty;
    public string ErrorMessage
    {
        get => _errorMessage;
        set { SetProperty(ref _errorMessage, value); RaisePropertyChanged(nameof(HasError)); }
    }

    public bool HasError => !string.IsNullOrEmpty(ErrorMessage);
    public bool IsEmpty  => Lecturers.Count == 0 && !IsBusy;

    public ICommand LoadCommand           { get; }
    public ICommand RefreshCommand        { get; }
    public ICommand AssignLecturerCommand { get; }

    public ClassRepLecturersViewModel(IClassRepApiService classRepApi)
    {
        _classRepApi          = classRepApi;
        Title                 = "Assign Lecturer";
        LoadCommand           = new Command(async () => await LoadAsync());
        RefreshCommand        = new Command(async () => await LoadAsync());
        AssignLecturerCommand = new Command(async () => await AssignLecturerAsync());
    }

    public async Task LoadAsync()
    {
        if (IsBusy) return;
        IsBusy = true;
        ErrorMessage = string.Empty;
        RaisePropertyChanged(nameof(IsEmpty));

        var ct = CreateLinkedToken();
        try
        {
            var list = await _classRepApi.GetApprovedLecturersAsync(ct);
            Lecturers.Clear();
            foreach (var l in list) Lecturers.Add(l);
        }
        catch (OperationCanceledException) { }
        catch (Exception)
        {
            ErrorMessage = "Could not load lecturers. Please try again.";
        }
        finally
        {
            IsBusy = false;
            RaisePropertyChanged(nameof(IsEmpty));
        }
    }

    private async Task AssignLecturerAsync()
    {
        if (SelectedLecturer is null)
        {
            await ShowErrorToastAsync("Please select a lecturer first.");
            return;
        }
        if (_subclassId == Guid.Empty)
        {
            await ShowErrorToastAsync("Invalid subclass.");
            return;
        }
        if (IsBusy) return;
        IsBusy = true;

        var ct = CreateLinkedToken();
        try
        {
            bool ok = await _classRepApi.AssignLecturerAsync(_subclassId, SelectedLecturer.Id, ct);
            if (ok)
            {
                await ShowSuccessToastAsync($"{SelectedLecturer.DisplayName} assigned.");
                await Shell.Current.GoToAsync("..");
            }
            else
            {
                await ShowErrorToastAsync("Failed to assign lecturer. Please try again.");
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
