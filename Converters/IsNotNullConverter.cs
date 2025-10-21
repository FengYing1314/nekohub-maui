using System.Globalization;
using Microsoft.Maui.Controls;

namespace nekohub_maui.Converters;

public sealed class IsNotNullConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        => value is string s ? !string.IsNullOrWhiteSpace(s) : value is not null;

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        => throw new NotSupportedException();
}
