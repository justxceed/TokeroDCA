using System.Text.Json;
using TokeroDCA.Services.Interfaces;
using TokeroDCA.ViewModels;

namespace TokeroDCA.Services;

public class NavigationService : INavigationService
{
    private readonly IServiceProvider _serviceProvider;

    public NavigationService(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public async Task NavigateToAsync<TPage>() where TPage : Page
    {
        var page = _serviceProvider.GetRequiredService<TPage>();

        if (page.BindingContext is IInitialize initialize)
        {
            await initialize.InitializeAsync();
        }

        await Application.Current!.MainPage!.Navigation.PushAsync(page);
    }

    public async Task NavigateToAsync<TPage>(Dictionary<string, object> parameters) where TPage : Page
    {
        var page = _serviceProvider.GetRequiredService<TPage>();

        if (page.BindingContext is IInitialize initialize)
        {
            await initialize.InitializeAsync(parameters);
        }

        await Application.Current!.MainPage!.Navigation.PushAsync(page);
    }
}