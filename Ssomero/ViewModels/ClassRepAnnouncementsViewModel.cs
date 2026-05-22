using System.Collections.ObjectModel;
using System.Windows.Input;
using Microsoft.Maui.Controls;
using Ssomero.Interfaces;
using Ssomero.Models;

namespace Ssomero.ViewModels;

public class ClassRepAnnouncementsViewModel : BaseViewModel
{
    private readonly IClassAnnouncementApiService _announcementApi;
    private readonly IClassRepApiService _classRepApi;

    // ── Collections ───────────────────────────────────────────────────────────
    public ObservableCollection<ClassAnnouncementModel> Announcements { get; } = [];
    public ObservableCollection<ClassRepSubclassModel> AvailableClasses { get; } = [];

    // ── Properties ────────────────────────────────────────────────────────────
    private ClassRepSubclassModel? _selectedClass;
    public ClassRepSubclassModel? SelectedClass
    {
        get => _selectedClass;
        set => SetProperty(ref _selectedClass, value);
    }

    private string _announcementTitle = string.Empty;
    public string AnnouncementTitle
    {
        get => _announcementTitle;
        set => SetProperty(ref _announcementTitle, value);
    }

    private string _announcementMessage = string.Empty;
    public string AnnouncementMessage
    {
        get => _announcementMessage;
        set => SetProperty(ref _announcementMessage, value);
    }

    private string _errorMessage = string.Empty;
    public string ErrorMessage
    {
        get => _errorMessage;
        set { SetProperty(ref _errorMessage, value); RaisePropertyChanged(nameof(HasError)); }
    }

    public bool HasError  => !string.IsNullOrEmpty(ErrorMessage);
    public bool IsEmpty   => Announcements.Count == 0 && !IsBusy;

    // ── Commands ──────────────────────────────────────────────────────────────
    public ICommand LoadCommand               { get; }
    public ICommand RefreshCommand            { get; }
    public ICommand CreateAnnouncementCommand { get; }
    public ICommand DeleteAnnouncementCommand { get; }

    public ClassRepAnnouncementsViewModel(
        IClassAnnouncementApiService announcementApi,
        IClassRepApiService classRepApi)
    {
        _announcementApi = announcementApi;
        _classRepApi     = classRepApi;
        Title            = "Announcements";

        LoadCommand               = new Command(async () => await LoadAsync());
        RefreshCommand            = new Command(async () => await LoadAsync());
        CreateAnnouncementCommand = new Command(async () => await CreateAnnouncementAsync());
        DeleteAnnouncementCommand = new Command<ClassAnnouncementModel>(async a => await DeleteAnnouncementAsync(a));
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
            var announcementsTask = _announcementApi.GetAnnouncementsAsync(ct);
            var subclassesTask    = _classRepApi.GetSubclassesAsync(ct);

            await Task.WhenAll(announcementsTask, subclassesTask);

            Announcements.Clear();
            foreach (var a in announcementsTask.Result)
                Announcements.Add(a);

            AvailableClasses.Clear();
            foreach (var s in subclassesTask.Result)
                AvailableClasses.Add(s);
        }
        catch (OperationCanceledException) { }
        catch (Exception)
        {
            ErrorMessage = "Could not load announcements. Please try again.";
        }
        finally
        {
            IsBusy = false;
            RaisePropertyChanged(nameof(IsEmpty));
        }
    }

    private async Task CreateAnnouncementAsync()
    {
        if (string.IsNullOrWhiteSpace(AnnouncementTitle))
        {
            await ShowErrorToastAsync("Title is required.");
            return;
        }
        if (string.IsNullOrWhiteSpace(AnnouncementMessage))
        {
            await ShowErrorToastAsync("Message is required.");
            return;
        }
        if (SelectedClass is null)
        {
            await ShowErrorToastAsync("Please select a target class.");
            return;
        }
        if (IsBusy) return;
        IsBusy = true;

        var ct = CreateLinkedToken();
        try
        {
            var result = await _announcementApi.CreateAnnouncementAsync(
                new CreateClassAnnouncementRequest
                {
                    ClassId = SelectedClass.Id,
                    Title   = AnnouncementTitle.Trim(),
                    Message = AnnouncementMessage.Trim(),
                }, ct);

            if (result is not null)
            {
                Announcements.Insert(0, result);
                AnnouncementTitle   = string.Empty;
                AnnouncementMessage = string.Empty;
                SelectedClass       = null;
                RaisePropertyChanged(nameof(IsEmpty));
                await ShowSuccessToastAsync("Announcement posted.");
            }
            else
            {
                await ShowErrorToastAsync("Failed to post announcement. Please try again.");
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

    private async Task DeleteAnnouncementAsync(ClassAnnouncementModel announcement)
    {
        if (announcement is null) return;

        bool confirmed = await Shell.Current.DisplayAlert(
            "Delete Announcement",
            $"Delete \"{announcement.Title}\"?",
            "Delete", "Cancel");

        if (!confirmed) return;
        if (IsBusy) return;
        IsBusy = true;

        var ct = CreateLinkedToken();
        try
        {
            bool ok = await _announcementApi.DeleteAnnouncementAsync(announcement.Id, ct);
            if (ok)
            {
                Announcements.Remove(announcement);
                RaisePropertyChanged(nameof(IsEmpty));
                await ShowSuccessToastAsync("Announcement deleted.");
            }
            else
            {
                await ShowErrorToastAsync("Could not delete announcement.");
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
