using TokeroDCA.Models;

namespace TokeroDCA.Services.Interfaces;

public interface ICoinRestService
{
    Task<List<Coin>> GetCoinListAsync();
    Task<decimal?> GetCoinPriceEURAsync(string coinId, DateTime date);
    Task<decimal?> GetLatestCoinPriceEURAsync(string coinId);
}