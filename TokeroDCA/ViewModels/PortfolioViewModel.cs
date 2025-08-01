using System.Collections.ObjectModel;
using System.ComponentModel;
using TokeroDCA.Models;
using TokeroDCA.Services.Interfaces;
using Microcharts;
using SkiaSharp;
using Timer = System.Timers.Timer;

namespace TokeroDCA.ViewModels;

public class PortfolioViewModel : IInitialize, INotifyPropertyChanged
{
    public event PropertyChangedEventHandler PropertyChanged;
    private readonly IDatabaseService _db;
    private readonly ICoinRestService _coinService;
    private Timer _timer;
    private Chart _portfolioChart;

    public PortfolioViewModel(IDatabaseService database, ICoinRestService coinRest)
    {
        _db = database;
        _coinService = coinRest;
    }

    public ObservableCollection<CoinLivePrice> LivePrices { get; } = new();
    public ObservableCollection<PortfolioRow> PortfolioRows { get; } = new();

    public Chart PortfolioChart
    {
        get => _portfolioChart;
        private set
        {
            if (_portfolioChart == value) return;
            _portfolioChart = value;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(PortfolioChart)));
        }
    }

    public async Task InitializeAsync(Dictionary<string, object> parameters = null)
    {
        // Load all coins from DB for live prices
        var allCoins = await _db.GetAllAsync<Coin>();
        var coinPrices = new Dictionary<string, decimal>();
        LivePrices.Clear();
        foreach (var coin in allCoins)
        {
            var latestPrice = await _coinService.GetLatestCoinPriceEURAsync(coin.Id) ?? 0;
            coinPrices.TryAdd(coin.Id, latestPrice);
            LivePrices.Add(new CoinLivePrice
            {
                CoinId = coin.Id,
                Name = coin.Name
            });
        }

        // Load DCA events for selected coin
        PortfolioRows.Clear();
        var dcaEvents = await _db.GetAllAsync<DCAEvent>();

        decimal totalInvested = 0m, valueToday = 0m;
        foreach (var dcaGroup in dcaEvents.OrderBy(e => e.Date).GroupBy(e => e.Date))
        {
            totalInvested += dcaGroup.Sum(e => e.AmountInvested);
            valueToday += dcaGroup.Sum(e => e.CoinsPurchased * coinPrices?[e.CoinId]) ?? 0;
            PortfolioRows.Add(new PortfolioRow
            {
                Date = dcaGroup.FirstOrDefault().Date,
                Invested = totalInvested,
                ValueToday = valueToday,
            });
        }

        if (PortfolioRows.Count > 0)
        {
            PortfolioChart = new LineChart
            {
                LineMode = LineMode.Straight,
                ShowYAxisLines = true,
                Entries = PortfolioRows.Select(r => new ChartEntry((float)r.ValueToday)
                {
                    Label = r.Date.ToString("yyyy-MM"),
                    ValueLabel = $"€{r.ValueToday:F2}",
                    Color = SKColor.Parse("#3498db")
                }).ToList()
            };

            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(PortfolioRows)));
        }

        _timer = new Timer(10000);
        _timer.Elapsed += async (s, e) => await UpdateLivePrices();
        _timer.AutoReset = true;
        _timer.Start();
    }

    private async Task UpdateLivePrices()
    {
        foreach (var livePrice in LivePrices)
        {
            var price = await _coinService.GetLatestCoinPriceEURAsync(livePrice.CoinId);
            if (price.HasValue)
            {
                await MainThread.InvokeOnMainThreadAsync(() => { livePrice.Latest = price.Value; });
            }
        }
    }
}