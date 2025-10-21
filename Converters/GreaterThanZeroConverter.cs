using System.Globalization;
using Microsoft.Maui.Controls;

namespace nekohub_maui.Converters;

public sealed class GreaterThanZeroConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is int i) return i > 0;
        if (value is long l) return l > 0;
        if (value is double d) return d > 0;
        if (value is float f) return f > 0;
        if (value is nint n) return n > 0;
        if (value is nuint u) return u > 0;
        return false;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        => throw new NotSupportedException();
}
