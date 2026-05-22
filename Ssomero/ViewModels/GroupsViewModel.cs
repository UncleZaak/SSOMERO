using System.Collections.ObjectModel;
using System.Windows.Input;
using Microsoft.Maui.Controls;
using Ssomero.Interfaces;
using Ssomero.Models;

namespace Ssomero.ViewModels;

public class GroupsViewModel : BaseViewModel
{
    private readonly IGroupsService _groups;
    private DateTime _lastLoaded = DateTime.MinValue;
    private static readonly TimeSpan RefreshInterval = TimeSpan.FromMinutes(2);

    public ObservableCollection<StudyGroupDto> Groups { get; } = [];

    string errorMessage = string.Empty;
    public string ErrorMessage { get => errorMessage; set => SetProperty(ref errorMessage, value); }

    bool isEmpty;
    public bool IsEmpty { get => isEmpty; set => SetProperty(ref isEmpty, value); }

    public ICommand LoadCommand { get; }
    public ICommand OpenGroupCommand { get; }

    public GroupsViewModel(IGroupsService groups)
    {
        _groups = groups;
        LoadCommand = new Command(async () => await LoadAsync(forceRefresh: true));
        OpenGroupCommand = new Command<StudyGroupDto>(async g =>
            await Shell.Current.GoToAsync($"group-chat?groupId={g.Id}&groupName={Uri.EscapeDataString(g.Name)}"));
    }

    public async Task LoadAsync(bool forceRefresh = false)
    {
        if (IsBusy) return;
        if (!forceRefresh && DateTime.UtcNow - _lastLoaded < RefreshInterval) return;

        IsBusy = true;
        ErrorMessage = string.Empty;
        try
        {
            Groups.Clear();
            foreach (var g in await _groups.GetGroupsAsync())
                Groups.Add(g);

            IsEmpty = Groups.Count == 0;
            _lastLoaded = DateTime.UtcNow;
        }
        catch (Exception ex)
        {
            ErrorMessage = "Failed to load groups. " + ex.Message;
        }
        finally
        {
            IsBusy = false;
        }
    }
}
