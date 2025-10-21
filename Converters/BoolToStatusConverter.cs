using System.Globalization;
using Microsoft.Maui.Controls;

namespace nekohub_maui.Converters;

public sealed class BoolToStatusConverter : IValueConverter
{
    public string PublishedText { get; set; } = "已发布";
    public string DraftText { get; set; } = "草稿";

    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        var b = value is bool bb && bb;
        return b ? PublishedText : DraftText;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        => throw new NotSupportedException();
}
