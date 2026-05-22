using System.Collections.ObjectModel;
using System.Windows.Input;
using Microsoft.Maui.Controls;
using Ssomero.Interfaces;
using Ssomero.Models;

namespace Ssomero.ViewModels;

public class ClassRepViewModel : BaseViewModel
{
    private readonly IClassRepApiService _classRepApi;

    // ── Observable collections ────────────────────────────────────────────────
    public ObservableCollection<ClassRepSubclassModel> Subclasses { get; } = [];

    // ── Properties ────────────────────────────────────────────────────────────
    private ClassRepMyClassModel? _myClass;
    public ClassRepMyClassModel? MyClass
    {
        get => _myClass;
        set { SetProperty(ref _myClass, value); RaisePropertyChanged(nameof(IsEmpty)); }
    }

    private ClassRepStatsModel? _stats;
    public ClassRepStatsModel? Stats
    {
        get => _stats;
        set => SetProperty(ref _stats, value);
    }

    private string _errorMessage = string.Empty;
    public string ErrorMessage
    {
        get => _errorMessage;
        set { SetProperty(ref _errorMessage, value); RaisePropertyChanged(nameof(HasError)); }
    }

    public bool HasError => !string.IsNullOrEmpty(ErrorMessage);

    public bool IsEmpty => MyClass is null && !IsBusy;

    private string _newSubclassName = string.Empty;
    public string NewSubclassName
    {
        get => _newSubclassName;
        set => SetProperty(ref _newSubclassName, value);
    }

    private string _newSubclassDescription = string.Empty;
    public string NewSubclassDescription
    {
        get => _newSubclassDescription;
        set => SetProperty(ref _newSubclassDescription, value);
    }

    // ── Commands ──────────────────────────────────────────────────────────────
    public ICommand LoadCommand { get; }
    public ICommand RefreshCommand { get; }
    public ICommand CreateSubclassCommand { get; }
    public ICommand RenameSubclassCommand { get; }
    public ICommand NavigateToStudentsCommand { get; }
    public ICommand NavigateToLecturersCommand { get; }
    public ICommand NavigateToAttendanceCommand { get; }

    public ClassRepViewModel(IClassRepApiService classRepApi)
    {
        _classRepApi = classRepApi;
        Title = "My Class";

        LoadCommand             = new Command(async () => await LoadAsync());
        RefreshCommand          = new Command(async () => await LoadAsync());
        CreateSubclassCommand   = new Command(async () => await CreateSubclassAsync());
        RenameSubclassCommand   = new Command<ClassRepSubclassModel>(async s => await RenameSubclassAsync(s));
        NavigateToStudentsCommand  = new Command<ClassRepSubclassModel>(async s => await Shell.Current.GoToAsync($"ClassRepStudentsPage?classId={s.Id}"));
        NavigateToLecturersCommand = new Command<ClassRepSubclassModel>(async s => await Shell.Current.GoToAsync($"ClassRepLecturersPage?subclassId={s.Id}"));
        NavigateToAttendanceCommand = new Command(async () => await Shell.Current.GoToAsync("ClassRepAttendancePage"));
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
            var classTask      = _classRepApi.GetMyClassAsync(ct);
            var subclassesTask = _classRepApi.GetSubclassesAsync(ct);
            var statsTask      = _classRepApi.GetStatsAsync(ct);

            await Task.WhenAll(classTask, subclassesTask, statsTask);

            MyClass = classTask.Result;
            Stats   = statsTask.Result;

            Subclasses.Clear();
            foreach (var s in subclassesTask.Result)
                Subclasses.Add(s);

            if (MyClass is null)
                ErrorMessage = "Could not load class information.";
        }
        catch (OperationCanceledException) { }
        catch (Exception)
        {
            ErrorMessage = "An unexpected error occurred. Please try again.";
        }
        finally
        {
            IsBusy = false;
            RaisePropertyChanged(nameof(IsEmpty));
        }
    }

    private async Task CreateSubclassAsync()
    {
        if (string.IsNullOrWhiteSpace(NewSubclassName))
        {
            await ShowErrorToastAsync("Subclass name is required.");
            return;
        }
        if (IsBusy) return;
        IsBusy = true;

        var ct = CreateLinkedToken();
        try
        {
            var result = await _classRepApi.CreateSubclassAsync(
                new CreateSubclassRequest { Name = NewSubclassName.Trim(), Description = NewSubclassDescription.Trim() }, ct);

            if (result is not null)
            {
                Subclasses.Add(result);
                NewSubclassName        = string.Empty;
                NewSubclassDescription = string.Empty;
                await ShowSuccessToastAsync("Subclass created successfully.");
            }
            else
            {
                await ShowErrorToastAsync("Failed to create subclass. Please try again.");
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

    private async Task RenameSubclassAsync(ClassRepSubclassModel subclass)
    {
        if (subclass is null) return;

        string? newName = await Shell.Current.DisplayPromptAsync("Rename Subclass",
            "Enter new name:", initialValue: subclass.Name, maxLength: 100, keyboard: Keyboard.Text);
        if (string.IsNullOrWhiteSpace(newName) || newName.Trim() == subclass.Name) return;

        if (IsBusy) return;
        IsBusy = true;
        var ct = CreateLinkedToken();
        try
        {
            var result = await _classRepApi.RenameSubclassAsync(subclass.Id, new RenameSubclassRequest { Name = newName.Trim() }, ct);
            if (result is not null)
            {
                var idx = Subclasses.IndexOf(subclass);
                if (idx >= 0) Subclasses[idx] = result;
                await ShowSuccessToastAsync("Subclass renamed.");
            }
            else
            {
                await ShowErrorToastAsync("Rename failed. Please try again.");
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
