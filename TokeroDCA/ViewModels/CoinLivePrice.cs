using System.ComponentModel;

namespace TokeroDCA.ViewModels;

public class CoinLivePrice : INotifyPropertyChanged
{
    public event PropertyChangedEventHandler PropertyChanged;

    private decimal _latest;
    private decimal _previous;
    private Color _priceColor = Colors.Black;
    private bool _priceLoaded;

    public string CoinId { get; set; }
    public string Name { get; set; }

    public decimal Latest
    {
        get => _latest;
        set
        {
            if (_latest == value) return;
            _previous = _latest;
            _latest = value;
            if (_previous == 0)
                PriceColor = Colors.Black;
            else if (_latest > _previous)
                PriceColor = Colors.Green;
            else if (_latest < _previous)
                PriceColor = Colors.Red;
            else
                PriceColor = Colors.Black;

            PriceLoaded = true;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Latest)));
        }
    }

    public Color PriceColor
    {
        get => _priceColor;
        set
        {
            if (_priceColor == value) return;
            _priceColor = value;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(PriceColor)));
        }
    }

    public bool PriceLoaded
    {
        get => _priceLoaded;
        set
        {
            if (_priceLoaded == value) return;
            _priceLoaded = value;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(PriceLoaded)));
        }
    }
}