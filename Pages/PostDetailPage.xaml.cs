using nekohub_maui.ViewModels;

namespace nekohub_maui.Pages;

[QueryProperty(nameof(PostId), "id")]
public partial class PostDetailPage : ContentPage
{
    private readonly PostDetailViewModel _vm;

    public PostDetailPage(PostDetailViewModel vm)
    {
        InitializeComponent();
        BindingContext = _vm = vm;
    }

    public string? PostId { get; set; }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        if (int.TryParse(PostId, out var id))
        {
            await _vm.LoadAsync(id);
        }
    }
}
