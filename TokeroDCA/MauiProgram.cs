using CommunityToolkit.Maui;
using Microcharts.Maui;
using Microsoft.Extensions.Logging;
using TokeroDCA.Services;
using TokeroDCA.Services.Interfaces;
using TokeroDCA.ViewModels;
using TokeroDCA.Views;

namespace TokeroDCA;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();
        builder
            .UseMauiApp<App>()
            .UseMicrocharts()
            .UseMauiCommunityToolkit()
            .ConfigureFonts(fonts =>
            {
                fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
            });
        
        builder.Services.AddSingleton<IDatabaseService>(provider =>
        {
            var dbPath = Path.Combine(
                FileSystem.AppDataDirectory, "dca.db3"
            );
            return new DatabaseService(dbPath);
        });
        builder.Services.AddSingleton<ICoinRestService, BinanceCoinRestService>();
        //Coinmarketcap requires an account and a token
        //builder.Services.AddSingleton<ICoinRestService, CoinMarketCapRestService>();
        //Coingecko is too restrictive with request rate limits
        //builder.Services.AddSingleton<ICoinRestService, CoinGeckoRestService>();
        builder.Services.AddSingleton<IDCAEngine, DCAEngine>();
        builder.Services.AddSingleton<INavigationService, NavigationService>();

        builder.Services.AddTransient<PortfolioViewModel>();
        builder.Services.AddTransient<DCASetupViewModel>();
        builder.Services.AddTransient<LoginViewModel>();

        // Pages
        builder.Services.AddTransient<PortfolioPage>();
        builder.Services.AddTransient<DCASetupPage>();
        builder.Services.AddTransient<LoginPage>();

#if DEBUG
        builder.Logging.AddDebug();
#endif

        return builder.Build();
    }
}