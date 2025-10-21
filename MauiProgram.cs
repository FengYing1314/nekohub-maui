using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;

namespace nekohub_maui;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();
        builder
            .UseMauiApp<App>()
            .ConfigureFonts(fonts =>
            {
                fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
            });

        // DI registrations
        builder.Services.AddHttpClient<Services.IPostsApi, Services.PostsApiService>(client =>
        {
            client.BaseAddress = Services.AppConfig.GetApiBaseUri();
            client.Timeout = TimeSpan.FromSeconds(15);
        });

        builder.Services.AddTransient<ViewModels.PostsViewModel>();
        builder.Services.AddTransient<Pages.PostsPage>();
        builder.Services.AddTransient<ViewModels.PostDetailViewModel>();
        builder.Services.AddTransient<Pages.PostDetailPage>();
        builder.Services.AddTransient<ViewModels.EditPostViewModel>();
        builder.Services.AddTransient<Pages.EditPostPage>();

#if DEBUG
        builder.Logging.AddDebug();
#endif

        return builder.Build();
    }
}