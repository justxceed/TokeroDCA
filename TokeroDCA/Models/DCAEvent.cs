namespace TokeroDCA.Models;

public class DCAEvent
{
    public int Id { get; set; }
    public string CoinId { get; set; }
    public DateTime Date { get; set; }
    public decimal AmountInvested { get; set; }
    public decimal CoinsPurchased { get; set; }
}