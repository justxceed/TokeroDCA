namespace TokeroDCA.Services.Interfaces;

public interface INavigationService
{
    Task NavigateToAsync<TPage>() where TPage : Page;
    Task NavigateToAsync<TPage>(Dictionary<string, object> parameters) where TPage : Page;
}
