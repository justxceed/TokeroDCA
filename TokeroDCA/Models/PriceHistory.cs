namespace TokeroDCA.Models;

public class PriceHistory
{
    public int Id { get; set; }
    public string CoinId { get; set; }
    public DateTime Date { get; set; }
    public decimal PriceEUR { get; set; }
}