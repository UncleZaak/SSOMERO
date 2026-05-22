using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows.Input;
using Microsoft.Maui.Controls;
using Ssomero.Interfaces;
using Ssomero.Models;

namespace Ssomero.ViewModels;

public class AnnouncementsViewModel : BaseViewModel
{
    private readonly IAnnouncementsService _service;
    private DateTime _lastLoaded = DateTime.MinValue;
    private static readonly TimeSpan RefreshInterval = TimeSpan.FromSeconds(60);

    public ObservableCollection<AnnouncementDto> Items { get; } = [];

    string errorMessage = string.Empty;
    public string ErrorMessage
    {
        get => errorMessage;
        set => SetProperty(ref errorMessage, value);
    }

    public ICommand LoadCommand { get; }

    public AnnouncementsViewModel(IAnnouncementsService service)
    {
        _service = service;
        LoadCommand = new Command(async () => await LoadAsync(forceRefresh: true));
    }

    public async Task LoadAsync(bool forceRefresh = false)
    {
        if (IsBusy) return;
        if (!forceRefresh && DateTime.UtcNow - _lastLoaded < RefreshInterval) return;

        IsBusy = true;
        ErrorMessage = string.Empty;
        try
        {
            Items.Clear();
            var list = await _service.GetAnnouncementsAsync();
            foreach (var a in list)
                Items.Add(a);

            _lastLoaded = DateTime.UtcNow;
        }
        catch (Exception ex)
        {
            ErrorMessage = "Failed to load announcements. " + ex.Message;
        }
        finally
        {
            IsBusy = false;
        }
    }
}
