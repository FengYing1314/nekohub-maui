namespace nekohub_maui;

public partial class AppShell : Shell
{
    public AppShell()
    {
        InitializeComponent();
        Routing.RegisterRoute(nameof(Pages.PostDetailPage), typeof(Pages.PostDetailPage));
        Routing.RegisterRoute(nameof(Pages.EditPostPage), typeof(Pages.EditPostPage));
    }
}