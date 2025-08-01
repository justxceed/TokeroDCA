using System.ComponentModel;

namespace TokeroDCA.ViewModels;

public class PortfolioRow : INotifyPropertyChanged
{
    public event PropertyChangedEventHandler PropertyChanged;
    public DateTime Date { get; set; }
    public decimal Invested { get; set; }
    public decimal CoinAmount { get; set; }
    public decimal ValueToday { get; set; }
    public decimal ROI => Invested == 0 ? 0 : (ValueToday - Invested) / Invested * 100;
}