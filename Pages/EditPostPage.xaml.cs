using nekohub_maui.ViewModels;

namespace nekohub_maui.Pages;

[QueryProperty(nameof(PostId), "id")]
public partial class EditPostPage : ContentPage
{
    private readonly EditPostViewModel _vm;

    public EditPostPage(EditPostViewModel vm)
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
