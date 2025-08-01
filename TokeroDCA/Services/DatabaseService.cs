using TokeroDCA.Models;
using TokeroDCA.Services.Interfaces;
using SQLite;

namespace TokeroDCA.Services;

public class DatabaseService : IDatabaseService
{
    private readonly SQLiteAsyncConnection _db;

    public DatabaseService(string dbPath)
    {
        _db = new SQLiteAsyncConnection(dbPath);
        _db.CreateTableAsync<Coin>().Wait();
        _db.CreateTableAsync<DCAEvent>().Wait();
        _db.CreateTableAsync<PriceHistory>().Wait();
    }

    public Task InsertAsync<T>(T item) => _db.InsertAsync(item);

    public Task<List<T>> GetAllAsync<T>() where T : new()
        => _db.Table<T>().ToListAsync();

    public Task<List<DCAEvent>> GetDCAEventsAsync(string coinId)
        => _db.Table<DCAEvent>().Where(e => e.CoinId == coinId).OrderBy(e => e.Date).ToListAsync();

    public Task<List<PriceHistory>> GetPriceHistoriesAsync(string coinId)
        => _db.Table<PriceHistory>().Where(ph => ph.CoinId == coinId).OrderBy(ph => ph.Date).ToListAsync();

    public Task<PriceHistory> GetPriceHistoryAsync(string coinId, DateTime date)
        => _db.Table<PriceHistory>().FirstOrDefaultAsync(ph => ph.CoinId == coinId && ph.Date == date);

    public async Task InsertOrUpdatePriceHistoryAsync(PriceHistory history)
    {
        var existing = await GetPriceHistoryAsync(history.CoinId, history.Date);
        if (existing == null)
        {
            await _db.InsertAsync(history);
        }
        else
        {
            existing.PriceEUR = history.PriceEUR;
            await _db.UpdateAsync(existing);
        }
    }
    
    public async Task InsertCoinIfNewAsync(Coin coin)
    {
        var existingCoin = await _db.Table<Coin>().Where(c => c.Id == coin.Id).FirstOrDefaultAsync();
        if (existingCoin == null)
        {
            await _db.InsertAsync(coin);
        }
    }
    
    public async Task ClearAllDataAsync()
    {
        await _db.ExecuteAsync("DELETE FROM Coin");
        await _db.ExecuteAsync("DELETE FROM DCAEvent");
        await _db.ExecuteAsync("DELETE FROM PriceHistory");
    }
}