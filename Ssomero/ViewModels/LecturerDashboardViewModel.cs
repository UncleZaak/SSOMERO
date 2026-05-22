using System.Collections.ObjectModel;
using System.Windows.Input;
using Ssomero.Interfaces;
using Ssomero.Models;
using Ssomero.Services;

namespace Ssomero.ViewModels;

public class LecturerDashboardViewModel : BaseViewModel
{
    private readonly ILecturerApiService _lecturer;
    private readonly SessionService _session;

    public ObservableCollection<LecturerClassDto> Classes { get; } = [];

    public ICommand LoadCommand { get; }
    public ICommand RefreshCommand { get; }
    public ICommand GoToClassesCommand { get; }

    string lecturerName = "Lecturer";
    public string LecturerName { get => lecturerName; set => SetProperty(ref lecturerName, value); }

    string currentDate = DateTime.Now.ToString("dddd, dd MMM yyyy");
    public string CurrentDate { get => currentDate; set => SetProperty(ref currentDate, value); }

    bool isEmpty;
    public bool IsEmpty { get => isEmpty; set => SetProperty(ref isEmpty, value); }

    public LecturerDashboardViewModel(ILecturerApiService lecturer, SessionService session)
    {
        _lecturer = lecturer;
        _session  = session;

        LoadCommand    = new Command(async () => await LoadAsync());
        RefreshCommand = new Command(async () => await LoadAsync());
        GoToClassesCommand = new Command(async () =>
            await Shell.Current.GoToAsync("//LecturerApp/LecturerClassesPage"));
    }

    public async Task LoadAsync()
    {
        if (IsBusy) return;
        IsBusy = true;
        try
        {
            var ct = CreateLinkedToken();
            LecturerName = _session.CurrentUser?.FullName ?? "Lecturer";
            CurrentDate  = DateTime.Now.ToString("dddd, dd MMM yyyy");

            var classes = await _lecturer.GetClassesAsync(ct);
            Classes.Clear();
            foreach (var c in classes.Take(5))
                Classes.Add(c);

            IsEmpty = Classes.Count == 0;
        }
        catch (OperationCanceledException) { }
        catch (Exception)
        {
            await ShowErrorToastAsync("Failed to load dashboard.");
        }
        finally
        {
            IsBusy = false;
        }
    }
}
