using System.Windows.Input;
using TokeroDCA.Services.Interfaces;
using TokeroDCA.Views;

namespace TokeroDCA.ViewModels;

public class LoginViewModel
{
    private readonly INavigationService _navigationService;

    public LoginViewModel(INavigationService navigationService)
    {
        _navigationService = navigationService;
        LoginCommand = new Command(async () => await LoginAsync());
    }

    public ICommand LoginCommand { get; }

    private async Task LoginAsync()
    {
        await _navigationService.NavigateToAsync<DCASetupPage>();
    }
}