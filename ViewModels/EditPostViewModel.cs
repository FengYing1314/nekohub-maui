using System.Collections.ObjectModel;
using System.Windows.Input;
using Microsoft.Extensions.Logging;
using Microsoft.Maui.Controls;
using nekohub_maui.Models;
using nekohub_maui.Services;

namespace nekohub_maui.ViewModels;

public sealed class EditPostViewModel : BaseViewModel
{
    private readonly IPostsApi _api;
    private readonly ILogger<EditPostViewModel> _logger;

    private int? _postId;
    private string _title = string.Empty;
    private string _content = string.Empty;
    private bool _isPublished;

    public string Title
    {
        get => _title;
        set => SetProperty(ref _title, value);
    }

    public string Content
    {
        get => _content;
        set => SetProperty(ref _content, value);
    }

    public bool IsPublished
    {
        get => _isPublished;
        set => SetProperty(ref _isPublished, value);
    }

    public ObservableCollection<string> TitleErrors { get; } = new();
    public ObservableCollection<string> ContentErrors { get; } = new();

    public ICommand SaveCommand { get; }

    public EditPostViewModel(IPostsApi api, ILogger<EditPostViewModel> logger)
    {
        _api = api;
        _logger = logger;
        SaveCommand = new Command(async () => await SaveAsync(), () => !IsBusy);
    }

    public async Task LoadAsync(int id)
    {
        try
        {
            IsBusy = true;
            ErrorMessage = null;
            var post = await _api.GetByIdAsync(id);
            _postId = id;
            Title = post.Title;
            Content = post.Content;
            IsPublished = post.IsPublished;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Load for edit failed");
            ErrorMessage = ex.Message;
        }
        finally
        {
            IsBusy = false;
            (SaveCommand as Command)?.ChangeCanExecute();
        }
    }

    private void ApplyValidationErrors(ValidationProblemDetails? validation)
    {
        TitleErrors.Clear();
        ContentErrors.Clear();
        if (validation == null) return;
        foreach (var kv in validation.Errors)
        {
            var key = kv.Key?.Trim();
            if (string.Equals(key, nameof(Title), StringComparison.OrdinalIgnoreCase))
            {
                foreach (var msg in kv.Value) TitleErrors.Add(msg);
            }
            else if (string.Equals(key, nameof(Content), StringComparison.OrdinalIgnoreCase))
            {
                foreach (var msg in kv.Value) ContentErrors.Add(msg);
            }
        }
    }

    private async Task SaveAsync()
    {
        if (IsBusy) return;
        try
        {
            IsBusy = true;
            ErrorMessage = null;
            ApplyValidationErrors(null);

            if (_postId is int id)
            {
                await _api.UpdateAsync(id, Title, Content, IsPublished);
            }
            else
            {
                var created = await _api.CreateAsync(Title, Content, IsPublished);
                _postId = created.Id;
            }

            await Shell.Current.DisplayAlert("已保存", "保存成功", "OK");
            // Navigate back
            await Shell.Current.GoToAsync("..", true);
        }
        catch (ApiException apiEx)
        {
            ApplyValidationErrors(apiEx.Validation);
            ErrorMessage = apiEx.Problem?.Detail ?? apiEx.Message;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Save failed");
            ErrorMessage = ex.Message;
        }
        finally
        {
            IsBusy = false;
            (SaveCommand as Command)?.ChangeCanExecute();
        }
    }
}
