using System.Windows.Input;
using Microsoft.Extensions.Logging;
using Microsoft.Maui.Controls;
using nekohub_maui.Models;
using nekohub_maui.Services;

namespace nekohub_maui.ViewModels;

public sealed class PostDetailViewModel : BaseViewModel
{
    private readonly IPostsApi _api;
    private readonly ILogger<PostDetailViewModel> _logger;

    private Post? _post;
    public Post? Post
    {
        get => _post;
        set => SetProperty(ref _post, value);
    }

    public ICommand RefreshCommand { get; }
    public ICommand EditCommand { get; }
    public ICommand DeleteCommand { get; }
    public ICommand PublishToggleCommand { get; }

    private int _postId;

    public PostDetailViewModel(IPostsApi api, ILogger<PostDetailViewModel> logger)
    {
        _api = api;
        _logger = logger;

        RefreshCommand = new Command(async () => await LoadAsync(_postId));
        EditCommand = new Command(async () =>
        {
            if (Post != null)
                await Shell.Current.GoToAsync($"{nameof(Pages.EditPostPage)}?id={Post.Id}");
        });
        DeleteCommand = new Command(async () => await DeleteAsync());
        PublishToggleCommand = new Command(async () => await TogglePublishAsync());
    }

    public async Task LoadAsync(int id)
    {
        if (IsBusy) return;
        _postId = id;
        try
        {
            IsBusy = true;
            ErrorMessage = null;
            Post = await _api.GetByIdAsync(id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to load post {Id}", id);
            ErrorMessage = ex.Message;
        }
        finally
        {
            IsBusy = false;
        }
    }

    private async Task DeleteAsync()
    {
        if (Post == null) return;
        try
        {
            var confirm = await Application.Current?.MainPage?.DisplayAlert("删除", $"确定删除《{Post.Title}》?", "删除", "取消")!;
            if (!confirm) return;

            await _api.DeleteAsync(Post.Id);
            await Shell.Current.DisplayAlert("已删除", "文章已删除", "OK");
            await Shell.Current.GoToAsync("..", true);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Delete failed");
            await Shell.Current.DisplayAlert("错误", ex.Message, "OK");
        }
    }

    private async Task TogglePublishAsync()
    {
        if (Post == null) return;
        try
        {
            IsBusy = true;
            Post updated = Post.IsPublished
                ? await _api.UnpublishAsync(Post.Id)
                : await _api.PublishAsync(Post.Id);
            Post = updated;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Toggle publish failed");
            await Shell.Current.DisplayAlert("错误", ex.Message, "OK");
        }
        finally
        {
            IsBusy = false;
        }
    }
}
