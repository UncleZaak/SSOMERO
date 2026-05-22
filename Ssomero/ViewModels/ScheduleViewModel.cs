using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows.Input;
using Microsoft.Extensions.Logging;
using Microsoft.Maui.Controls;
using Ssomero.Interfaces;
using Ssomero.Models;

namespace Ssomero.ViewModels;

public class ScheduleViewModel : BaseViewModel
{
    private readonly IScheduleService _schedule;
    private readonly ILogger<ScheduleViewModel> _logger;
    private DateTime _lastLoaded = DateTime.MinValue;
    private static readonly TimeSpan RefreshInterval = TimeSpan.FromSeconds(60);

    public ObservableCollection<ScheduleDto> Items { get; } = [];

    string errorMessage = string.Empty;
    public string ErrorMessage { get => errorMessage; set => SetProperty(ref errorMessage, value); }

    public ICommand LoadCommand { get; }

    public ScheduleViewModel(IScheduleService schedule, ILogger<ScheduleViewModel> logger)
    {
        _schedule = schedule;
        _logger = logger;
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
            var list = await _schedule.GetSchedulesAsync();
            foreach (var s in list)
                Items.Add(s);

            _lastLoaded = DateTime.UtcNow;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to load schedules");
            ErrorMessage = "Failed to load schedule. " + ex.Message;
        }
        finally
        {
            IsBusy = false;
        }
    }
}
