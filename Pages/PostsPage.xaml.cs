using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.Devices;
using nekohub_maui.ViewModels;

namespace nekohub_maui.Pages;

public partial class PostsPage : ContentPage
{
    private readonly PostsViewModel _vm;

    public PostsPage(PostsViewModel vm)
    {
        InitializeComponent();
        BindingContext = _vm = vm;

        try
        {
            VersionLabel.Text = $"v{AppInfo.Current.VersionString} (build {AppInfo.Current.BuildString})";
            PlatformLabel.Text = DeviceInfo.Platform.ToString();
        }
        catch
        {
            // ignore any platform info errors
        }
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await _vm.OnAppearingAsync();
    }

    private void OnFilterChanged(object? sender, EventArgs e)
    {
        if (sender is Picker picker && _vm != null)
        {
            _vm.Filter = picker.SelectedIndex switch
            {
                1 => PublishedFilter.Published,
                2 => PublishedFilter.Drafts,
                _ => PublishedFilter.All
            };
        }
    }

    private void OnSortChanged(object? sender, EventArgs e)
    {
        if (sender is Picker picker && _vm != null)
        {
            switch (picker.SelectedIndex)
            {
                case 0: _vm.SortBy = "updatedAt"; _vm.SortOrder = "desc"; break;
                case 1: _vm.SortBy = "updatedAt"; _vm.SortOrder = "asc"; break;
                case 2: _vm.SortBy = "createdAt"; _vm.SortOrder = "desc"; break;
                case 3: _vm.SortBy = "createdAt"; _vm.SortOrder = "asc"; break;
                case 4: _vm.SortBy = "title"; _vm.SortOrder = "asc"; break;
                case 5: _vm.SortBy = "title"; _vm.SortOrder = "desc"; break;
                default: _vm.SortBy = "updatedAt"; _vm.SortOrder = "desc"; break;
            }
        }
    }
}
