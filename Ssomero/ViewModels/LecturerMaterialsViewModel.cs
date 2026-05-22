using System.Collections.ObjectModel;
using System.Windows.Input;
using Ssomero.Interfaces;
using Ssomero.Models;

namespace Ssomero.ViewModels;

[QueryProperty(nameof(ClassId),   "ClassId")]
[QueryProperty(nameof(ClassName), "ClassName")]
public class LecturerMaterialsViewModel : BaseViewModel
{
    private readonly ILecturerApiService _lecturer;

    public ObservableCollection<LecturerMaterialDto> Materials { get; } = [];

    public ICommand LoadCommand      { get; }
    public ICommand RefreshCommand   { get; }
    public ICommand UploadCommand    { get; }

    Guid _classId;
    public Guid ClassId
    {
        get => _classId;
        set { _classId = value; _ = LoadAsync(); }
    }

    string className = string.Empty;
    public string ClassName { get => className; set => SetProperty(ref className, value); }

    string newTitle = string.Empty;
    public string NewTitle { get => newTitle; set => SetProperty(ref newTitle, value); }

    string newFileUrl = string.Empty;
    public string NewFileUrl { get => newFileUrl; set => SetProperty(ref newFileUrl, value); }

    bool isEmpty;
    public bool IsEmpty { get => isEmpty; set => SetProperty(ref isEmpty, value); }

    bool hasError;
    public bool HasError { get => hasError; set => SetProperty(ref hasError, value); }

    bool isUploading;
    public bool IsUploading { get => isUploading; set => SetProperty(ref isUploading, value); }

    public LecturerMaterialsViewModel(ILecturerApiService lecturer)
    {
        _lecturer = lecturer;

        LoadCommand    = new Command(async () => await LoadAsync());
        RefreshCommand = new Command(async () => await LoadAsync());
        UploadCommand  = new Command(async () => await UploadAsync(), () => !IsUploading);
    }

    public async Task LoadAsync()
    {
        if (ClassId == Guid.Empty || IsBusy) return;
        IsBusy   = true;
        HasError = false;
        try
        {
            var ct     = CreateLinkedToken();
            var result = await _lecturer.GetMaterialsAsync(ClassId, ct);
            Materials.Clear();
            foreach (var m in result)
                Materials.Add(m);
            IsEmpty = Materials.Count == 0;
        }
        catch (OperationCanceledException) { }
        catch (Exception) { HasError = true; }
        finally { IsBusy = false; }
    }

    private async Task UploadAsync()
    {
        if (string.IsNullOrWhiteSpace(NewTitle))
        {
            await ShowErrorToastAsync("Please enter a title.");
            return;
        }
        IsUploading = true;
        try
        {
            var ct = CreateLinkedToken();
            var (success, error) = await _lecturer.UploadMaterialAsync(
                ClassId, NewTitle.Trim(), string.IsNullOrWhiteSpace(NewFileUrl) ? null : NewFileUrl.Trim(), ct);

            if (success)
            {
                NewTitle   = string.Empty;
                NewFileUrl = string.Empty;
                await ShowSuccessToastAsync("Material uploaded.");
                await LoadAsync();
            }
            else
            {
                await ShowErrorToastAsync(error ?? "Upload failed.");
            }
        }
        catch (OperationCanceledException) { }
        catch (Exception) { await ShowErrorToastAsync("Upload failed."); }
        finally { IsUploading = false; }
    }
}
