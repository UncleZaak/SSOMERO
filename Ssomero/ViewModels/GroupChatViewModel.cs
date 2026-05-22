using System.Collections.ObjectModel;
using System.Windows.Input;
using Microsoft.Maui.Controls;
using Ssomero.Interfaces;
using Ssomero.Models;
using Ssomero.Services;

namespace Ssomero.ViewModels;

[QueryProperty(nameof(GroupId),   "groupId")]
[QueryProperty(nameof(GroupName), "groupName")]
public class GroupChatViewModel : BaseViewModel
{
    private readonly IGroupsService _groups;
    private readonly SessionService _session;

    public ObservableCollection<GroupMessageDto> Messages { get; } = [];

    string groupId = string.Empty;
    public string GroupId { get => groupId; set { SetProperty(ref groupId, value); _ = LoadAsync(); } }

    string groupName = string.Empty;
    public string GroupName { get => groupName; set => SetProperty(ref groupName, value); }

    string messageText = string.Empty;
    public string MessageText { get => messageText; set { SetProperty(ref messageText, value); RaisePropertyChanged(nameof(CanSend)); } }

    string errorMessage = string.Empty;
    public string ErrorMessage { get => errorMessage; set => SetProperty(ref errorMessage, value); }

    bool isSending;
    public bool IsSending { get => isSending; set { SetProperty(ref isSending, value); RaisePropertyChanged(nameof(CanSend)); } }

    public bool CanSend => !IsSending && !string.IsNullOrWhiteSpace(MessageText);

    public ICommand SendCommand { get; }
    public ICommand LoadCommand { get; }

    public GroupChatViewModel(IGroupsService groups, SessionService session)
    {
        _groups  = groups;
        _session = session;
        SendCommand = new Command(async () => await SendAsync(), () => CanSend);
        LoadCommand = new Command(async () => await LoadAsync());
    }

    public async Task LoadAsync()
    {
        if (string.IsNullOrWhiteSpace(GroupId) || IsBusy) return;
        IsBusy = true;
        try
        {
            var msgs = (await _groups.GetGroupMessagesAsync(GroupId)).ToList();
            var myId = _session.CurrentUser?.Id ?? string.Empty;
            Messages.Clear();
            foreach (var m in msgs)
            {
                m.IsOwn = m.SenderId == myId;
                Messages.Add(m);
            }
        }
        catch (Exception ex)
        {
            ErrorMessage = ex.Message;
        }
        finally
        {
            IsBusy = false;
        }
    }

    private async Task SendAsync()
    {
        if (!CanSend) return;
        IsSending = true;
        var text = MessageText.Trim();
        MessageText = string.Empty;
        try
        {
            var ok = await _groups.SendMessageAsync(GroupId, text);
            if (ok)
                await LoadAsync();
            else
                ErrorMessage = "Failed to send message.";
        }
        catch (Exception ex)
        {
            ErrorMessage = ex.Message;
        }
        finally
        {
            IsSending = false;
        }
    }
}
