using System.Globalization;
using System.Text.Json;
using TokeroDCA.Models;
using TokeroDCA.Services.Interfaces;

public class BinanceCoinRestService : ICoinRestService
{
    private readonly HttpClient _httpClient = new();
    private const string BaseUrl = "https://api.binance.com";

    // 1. Get all coins with EUR trading pairs (e.g., BTC/EUR, ETH/EUR)
    public async Task<List<Coin>> GetCoinListAsync()
    {
        var url = $"{BaseUrl}/api/v3/exchangeInfo";
        var response = await _httpClient.GetAsync(url);

        response.EnsureSuccessStatusCode();
        using var stream = await response.Content.ReadAsStreamAsync();
        using var json = await JsonDocument.ParseAsync(stream);

        var result = new List<Coin>();
        if (json.RootElement.TryGetProperty("symbols", out var symbols))
        {
            foreach (var symbol in symbols.EnumerateArray())
            {
                string baseAsset = symbol.GetProperty("baseAsset").GetString();
                string quoteAsset = symbol.GetProperty("quoteAsset").GetString();
                string symbolName = symbol.GetProperty("symbol").GetString();

                // Only include EUR quote pairs (e.g., BTCEUR, ETHEUR)
                if (quoteAsset.Equals("EUR", StringComparison.OrdinalIgnoreCase))
                {
                    result.Add(new Coin
                    {
                        Id = baseAsset.ToLower(),     // Use base asset as coin id (e.g., "btc")
                        Symbol = baseAsset,           // Binance symbol (e.g., "BTC")
                        Name = baseAsset              // You can map to friendly names if desired
                    });
                }
            }
        }

        // (Optional) Remove duplicates if any
        result = result.GroupBy(c => c.Id).Select(g => g.First()).ToList();
        return result;
    }

    // 2. Get latest EUR price for one coin (e.g., "BTC", "ETH"). Returns null if not supported.
    public async Task<decimal?> GetLatestCoinPriceEURAsync(string coinId)
    {
        // Construct trading pair (e.g., "BTCEUR")
        string pair = $"{coinId.ToUpper()}EUR";
        var url = $"{BaseUrl}/api/v3/ticker/price?symbol={pair}";

        var response = await _httpClient.GetAsync(url);
        if (!response.IsSuccessStatusCode)
            return null;

        using var stream = await response.Content.ReadAsStreamAsync();
        using var json = await JsonDocument.ParseAsync(stream);

        if (json.RootElement.TryGetProperty("price", out var priceElement))
        {
            var priceStr = priceElement.GetString();
            return decimal.TryParse(priceStr, NumberStyles.Float, CultureInfo.InvariantCulture, out var price)
                ? price
                : null;
        }
        return null;
    }

    // 3. Get historical close price for a specific day (UTC) in EUR using kline data
    public async Task<decimal?> GetCoinPriceEURAsync(string coinId, DateTime date)
    {
        string pair = $"{coinId.ToUpper()}EUR";
        const string KlinesUrl = "/api/v3/klines";
        // Use daily kline interval
        string interval = "1d";

        // Binance requires unix milliseconds timestamps
        // Request kline where openTime <= date < closeTime (UTC)
        var dayUtc = new DateTime(date.Year, date.Month, date.Day, 0, 0, 0, DateTimeKind.Utc);
        long startTime = new DateTimeOffset(dayUtc).ToUnixTimeMilliseconds();
        long endTime = new DateTimeOffset(dayUtc.AddDays(1)).ToUnixTimeMilliseconds();

        string url = $"{BaseUrl}{KlinesUrl}?symbol={pair}&interval={interval}&startTime={startTime}&endTime={endTime}&limit=1";
        var response = await _httpClient.GetAsync(url);

        if (!response.IsSuccessStatusCode)
            return null;

        using var stream = await response.Content.ReadAsStreamAsync();
        using var json = await JsonDocument.ParseAsync(stream);

        // Kline response: [ [ openTime, open, high, low, close, volume, ... ] ]
        if (json.RootElement.ValueKind == JsonValueKind.Array && json.RootElement.GetArrayLength() > 0)
        {
            var kline = json.RootElement[0];
            if (kline.ValueKind == JsonValueKind.Array && kline.GetArrayLength() >= 5)
            {
                string closeStr = kline[4].GetString();
                if (decimal.TryParse(closeStr, NumberStyles.Float, CultureInfo.InvariantCulture, out decimal close))
                    return close;
            }
        }
        return null;
    }
}
