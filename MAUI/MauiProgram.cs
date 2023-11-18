using CommunityToolkit.Maui;
using CommunityToolkit.Maui.Storage;
using Mopups.Interfaces;
using Mopups.Services;
using Mopups.Hosting;

namespace XmlSearch;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();
        builder
            .UseMauiApp<App>()
            .UseMauiCommunityToolkit()
            .ConfigureFonts(fonts =>
            {
                fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
            })
        .ConfigureMopups();

		builder.Services.AddSingleton<IFileSaver>(FileSaver.Default);
		builder.Services.AddSingleton<IPopupNavigation>(MopupService.Instance);
		builder.Services.AddTransient<MainPage>();

        return builder.Build();
    }
}

