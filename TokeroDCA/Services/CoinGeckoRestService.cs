using System.Net.Http.Json;
using System.Text.Json;
using TokeroDCA.Models;
using TokeroDCA.Services.Interfaces;

namespace TokeroDCA.Services;

public class CoinGeckoRestService : ICoinRestService
{
    private readonly HttpClient _httpClient = new();

    public async Task<List<Coin>> GetCoinListAsync()
    {
        var coins = await _httpClient.GetFromJsonAsync<List<Coin>>("https://api.coingecko.com/api/v3/coins/markets?vs_currency=usd&order=market_cap_desc");
        
        return coins?.Take(100).ToList() ?? new List<Coin>();
    }

    public async Task<decimal?> GetCoinPriceEURAsync(string coinId, DateTime date)
    {
        var dateStr = date.ToString("dd-MM-yyyy");
        var url = $"https://api.coingecko.com/api/v3/coins/{coinId}/history?date={dateStr}&localization=false";
        var resp = await _httpClient.GetAsync(url);
        if (!resp.IsSuccessStatusCode)
        {
            return null;
        }

        var json = await resp.Content.ReadFromJsonAsync<JsonElement>();
        if (json.TryGetProperty("market_data", out var md) &&
            md.TryGetProperty("current_price", out var cp) &&
            cp.TryGetProperty("eur", out var eurPrice))
        {
            return eurPrice.GetDecimal();
        }

        return null;
    }

    public async Task<decimal?> GetLatestCoinPriceEURAsync(string coinId)
    {
        var url = $"https://api.coingecko.com/api/v3/simple/price?ids={coinId}&vs_currencies=eur";
        var resp = await _httpClient.GetAsync(url);
        if (!resp.IsSuccessStatusCode)
        {
            return null;
        }

        var root = await resp.Content.ReadFromJsonAsync<JsonElement>();
        if (root.TryGetProperty(coinId, out var cc) &&
            cc.TryGetProperty("eur", out var eurPrice))
        {
            return eurPrice.GetDecimal();
        }

        return null;
    }
}