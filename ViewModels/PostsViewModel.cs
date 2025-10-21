using System.Collections.ObjectModel;
using System.Windows.Input;
using Microsoft.Extensions.Logging;
using Microsoft.Maui.Controls;
using nekohub_maui.Models;
using nekohub_maui.Services;

namespace nekohub_maui.ViewModels;

public enum PublishedFilter
{
    All,
    Published,
    Drafts
}

public sealed class PostsViewModel : BaseViewModel
{
    private readonly IPostsApi _api;
    private readonly ILogger<PostsViewModel> _logger;

    private string? _searchText;
    private PublishedFilter _filter = PublishedFilter.All;
    private string _sortBy = "updatedAt";
    private string _sortOrder = "desc";
    private int _page = 1;
    private int _pageSize = 10;
    private bool _hasNext;
    private Post? _selectedPost;
    private bool _isRefreshing;

    public ObservableCollection<Post> Items { get; } = new();

    public string? SearchText
    {
        get => _searchText;
        set => SetProperty(ref _searchText, value);
    }

    public PublishedFilter Filter
    {
        get => _filter;
        set
        {
            if (SetProperty(ref _filter, value))
            {
                RefreshCommand.Execute(null);
            }
        }
    }

    public string SortBy
    {
        get => _sortBy;
        set
        {
            if (SetProperty(ref _sortBy, value))
            {
                RefreshCommand.Execute(null);
            }
        }
    }

    public string SortOrder
    {
        get => _sortOrder;
        set
        {
            if (SetProperty(ref _sortOrder, value))
            {
                RefreshCommand.Execute(null);
            }
        }
    }

    public bool HasNext
    {
        get => _hasNext;
        set => SetProperty(ref _hasNext, value);
    }

    public bool IsRefreshing
    {
        get => _isRefreshing;
        set => SetProperty(ref _isRefreshing, value);
    }

    public Post? SelectedPost
    {
        get => _selectedPost;
        set
        {
            if (SetProperty(ref _selectedPost, value) && value != null)
            {
                _ = OnSelectAsync(value);
            }
        }
    }

    public ICommand RefreshCommand { get; }
    public ICommand LoadMoreCommand { get; }
    public ICommand SearchCommand { get; }
    public ICommand ClearSearchCommand { get; }
    public ICommand CreateCommand { get; }

    public PostsViewModel(IPostsApi api, ILogger<PostsViewModel> logger)
    {
        _api = api;
        _logger = logger;

        RefreshCommand = new Command(async () => await ReloadAsync());
        LoadMoreCommand = new Command(async () => await LoadMoreAsync(), () => HasNext && !IsBusy);
        SearchCommand = new Command(async () => await ReloadAsync());
        ClearSearchCommand = new Command(async () =>
        {
            SearchText = string.Empty;
            await ReloadAsync();
        });
        CreateCommand = new Command(async () => await Shell.Current.GoToAsync(nameof(Pages.EditPostPage)));
    }

    public async Task OnAppearingAsync()
    {
        if (Items.Count == 0)
        {
            await ReloadAsync();
        }
    }

    private async Task OnSelectAsync(Post post)
    {
        try
        {
            await Shell.Current.GoToAsync($"{nameof(Pages.PostDetailPage)}?id={post.Id}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Navigation to detail failed");
        }
        finally
        {
            SelectedPost = null;
        }
    }

    private bool? GetPublishedFilterBool()
        => Filter switch
        {
            PublishedFilter.Published => true,
            PublishedFilter.Drafts => false,
            _ => null
        };

    private async Task ReloadAsync()
    {
        if (IsBusy) return;
        try
        {
            IsBusy = true;
            IsRefreshing = true;
            ErrorMessage = null;
            Items.Clear();
            _page = 1;

            var result = await _api.GetPagedAsync(_page, _pageSize, GetPublishedFilterBool(), string.IsNullOrWhiteSpace(SearchText) ? null : SearchText, SortBy, SortOrder);
            foreach (var p in result.Items)
                Items.Add(p);
            HasNext = result.HasNext;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to refresh posts");
            ErrorMessage = ex.Message;
        }
        finally
        {
            IsRefreshing = false;
            IsBusy = false;
            (LoadMoreCommand as Command)?.ChangeCanExecute();
        }
    }

    private async Task LoadMoreAsync()
    {
        if (IsBusy || !HasNext) return;
        try
        {
            IsBusy = true;
            ErrorMessage = null;
            _page++;
            var result = await _api.GetPagedAsync(_page, _pageSize, GetPublishedFilterBool(), string.IsNullOrWhiteSpace(SearchText) ? null : SearchText, SortBy, SortOrder);
            foreach (var p in result.Items)
                Items.Add(p);
            HasNext = result.HasNext;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to load more");
            ErrorMessage = ex.Message;
        }
        finally
        {
            IsBusy = false;
            (LoadMoreCommand as Command)?.ChangeCanExecute();
        }
    }
}
