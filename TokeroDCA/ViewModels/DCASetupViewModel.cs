using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows.Input;
using TokeroDCA.Models;
using TokeroDCA.Services.Interfaces;
using TokeroDCA.Views;

namespace TokeroDCA.ViewModels;

public class DCASetupViewModel : INotifyPropertyChanged
{
    public event PropertyChangedEventHandler PropertyChanged;
    private readonly IDCAEngine _dcaEngine;
    private readonly INavigationService _navigationService;
    private Coin _selectedCoin;
    private DateTime _startDate = DateTime.Today.AddYears(-1);
    private int _monthlyAmount = 200;
    private int _dayOfMonth = 15;
    private int _totalMonthlyAmount;
    private bool _isBusy;

    public DCASetupViewModel(
        IDCAEngine dcaEngine,
        ICoinRestService coinService,
        INavigationService navigationService)
    {
        _dcaEngine = dcaEngine;
        _navigationService = navigationService;

        SubmitCommand = new Command(async () => await SubmitAsync());
        AddCoinCommand = new Command(async () => await AddCoinAsync());
        ResetCommand = new Command(ResetSetupItems);

        // Load coins asynchronously, safely handling exceptions in production
        Task.Run(async () =>
        {
            var coins = await coinService.GetCoinListAsync();
            await MainThread.InvokeOnMainThreadAsync(() =>
            {
                foreach (var coin in coins)
                    Coins.Add(coin);
            });
        });
    }

    public ObservableCollection<Coin> Coins { get; } = new();

    public ObservableCollection<DCASetupItem> SetupItems { get; } = new();

    public Coin SelectedCoin
    {
        get => _selectedCoin;
        set
        {
            if (_selectedCoin == value) return;
            _selectedCoin = value;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(SelectedCoin)));
        }
    }

    public DateTime StartDate
    {
        get => _startDate;
        set
        {
            if (_startDate == value) return;
            _startDate = value;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(StartDate)));
        }
    }

    public int MonthlyAmount
    {
        get => _monthlyAmount;
        set
        {
            if (_monthlyAmount == value) return;
            _monthlyAmount = value;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(MonthlyAmount)));
        }
    }

    public int TotalMonthlyAmount
    {
        get => _totalMonthlyAmount;
        set
        {
            if (_totalMonthlyAmount == value) return;
            _totalMonthlyAmount = value;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(TotalMonthlyAmount)));
        }
    }

    public int DayOfMonth
    {
        get => _dayOfMonth;
        set
        {
            if (_dayOfMonth == value) return;
            _dayOfMonth = value;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(DayOfMonth)));
        }
    }

    public bool IsBusy
    {
        get => _isBusy;
        set
        {
            if (_isBusy == value) return;
            _isBusy = value;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsBusy)));
        }
    }

    public ICommand SubmitCommand { get; }

    public ICommand AddCoinCommand { get; }

    public ICommand ResetCommand { get; }

    private async Task AddCoinAsync()
    {
        if (SelectedCoin == null)
        {
            await Application.Current.MainPage.DisplayAlert(
                "Validation Error",
                "Please choose a coin.",
                "OK");
            return;
        }

        if (MonthlyAmount <= 0)
        {
            await Application.Current.MainPage.DisplayAlert(
                "Validation Error",
                "Please enter a valid monthly amount.",
                "OK");
            return;
        }

        var dcaSetupItem = new DCASetupItem
        {
            Coin = SelectedCoin,
            AmountInvested = MonthlyAmount
        };
        SetupItems.Add(dcaSetupItem);
        TotalMonthlyAmount += MonthlyAmount;
    }

    private async Task SubmitAsync()
    {
        IsBusy = true;
        try
        {
            if (SetupItems == null || SetupItems.Count == 0)
            {
                await Application.Current.MainPage.DisplayAlert(
                    "Validation Error",
                    "Please add at least one coin.",
                    "OK");
                return;
            }

            if (DayOfMonth < 1 || DayOfMonth > 28)
            {
                await Application.Current.MainPage.DisplayAlert(
                    "Validation Error",
                    "Day of month must be between 1 and 28.",
                    "OK");
                return;
            }

            var dcaSetup = new DCASetup
            {
                StartDate = StartDate,
                DayOfMonth = DayOfMonth,
                ItemsToInvestIn = SetupItems.ToList()
            };

            await _dcaEngine.CalculateDCAAsync(dcaSetup);

            await _navigationService.NavigateToAsync<PortfolioPage>();
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
        finally
        {
            IsBusy = false;
        }
    }

    private void ResetSetupItems(object obj)
    {
        SetupItems.Clear();
        TotalMonthlyAmount = 0;
    }
}