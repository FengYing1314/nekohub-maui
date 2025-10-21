using System.Globalization;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Graphics;

namespace nekohub_maui.Converters;

public sealed class BoolToStatusColorConverter : IValueConverter
{
    public Color PublishedColor { get; set; } = Colors.SeaGreen;
    public Color DraftColor { get; set; } = Colors.Gray;

    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        var b = value is bool bb && bb;
        return b ? PublishedColor : DraftColor;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        => throw new NotSupportedException();
}
