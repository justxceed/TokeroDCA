using TokeroDCA.Models;
using TokeroDCA.Services.Interfaces;

namespace TokeroDCA.Services;

public class DCAEngine : IDCAEngine
{
    private readonly ICoinRestService _coinService;
    private readonly IDatabaseService _db;

    public DCAEngine(ICoinRestService coinService, IDatabaseService db)
    {
        _coinService = coinService;
        _db = db;
    }

    public async Task<List<DCAEvent>> CalculateDCAAsync(DCASetup setup)
    {
        //Reset all data when starting a new DCA simulation
        await _db.ClearAllDataAsync();

        var now = DateTime.Today;
        var dcaEvents = new List<DCAEvent>();
        for (var date = new DateTime(setup.StartDate.Year, setup.StartDate.Month, setup.DayOfMonth);
             date <= now; date = date.AddMonths(1))
        {
            foreach (var item in setup.ItemsToInvestIn)
            {
                var ph = await _db.GetPriceHistoryAsync(item.Coin.Id, date);
                decimal? price = ph?.PriceEUR;
                if (price == null)
                {
                    price = await _coinService.GetCoinPriceEURAsync(item.Coin.Id, date);
                    if (price.HasValue)
                    {
                        var history = new PriceHistory
                            { CoinId = item.Coin.Id, Date = date, PriceEUR = price.Value };
                        await _db.InsertOrUpdatePriceHistoryAsync(history);
                    }
                }
                if (!price.HasValue)
                {
                    continue;
                }
                item.CoinsPurchased = item.AmountInvested / price.Value;

                var dcaEvent = new DCAEvent
                {
                    CoinId = item.Coin.Id,
                    Date = date,
                    AmountInvested = item.AmountInvested,
                    CoinsPurchased = item.CoinsPurchased
                };
                dcaEvents.Add(dcaEvent);
                await _db.InsertAsync(dcaEvent);
                await _db.InsertCoinIfNewAsync(item.Coin);
            }
        }
        return dcaEvents;
    }
}
