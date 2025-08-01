using TokeroDCA.Models;

namespace TokeroDCA.Services.Interfaces;

public interface IDatabaseService
{
    Task InsertAsync<T>(T item);
    Task<List<T>> GetAllAsync<T>() where T : new();
    Task<List<DCAEvent>> GetDCAEventsAsync(string coinId);
    Task<List<PriceHistory>> GetPriceHistoriesAsync(string coinId);
    Task<PriceHistory> GetPriceHistoryAsync(string coinId, DateTime date);
    Task InsertOrUpdatePriceHistoryAsync(PriceHistory history);
    Task InsertCoinIfNewAsync(Coin coin);
    Task ClearAllDataAsync();
}