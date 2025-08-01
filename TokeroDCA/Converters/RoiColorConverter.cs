using System.Globalization;

namespace TokeroDCA.Converters;

public class RoiColorConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is decimal roi)
        {
            return roi switch
            {
                > 0 => Colors.Green,
                < 0 => Colors.Red,
                _ => Colors.Black
            };
        }
        return Colors.Black;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}