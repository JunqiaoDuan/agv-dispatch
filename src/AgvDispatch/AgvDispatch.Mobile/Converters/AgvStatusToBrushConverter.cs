using Avalonia.Data.Converters;
using Avalonia.Media;
using AgvDispatch.Shared.Enums;
using System;
using System.Globalization;

namespace AgvDispatch.Mobile.Converters;

/// <summary>
/// AGV状态转颜色转换器
/// </summary>
public class AgvStatusToBrushConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is AgvStatus status)
        {
            return status switch
            {
                AgvStatus.Online => new SolidColorBrush(Color.Parse("#4caf50")), // Success/Green
                AgvStatus.Offline => new SolidColorBrush(Color.Parse("#9e9e9e")), // Gray
                _ => new SolidColorBrush(Color.Parse("#9e9e9e"))
            };
        }
        return new SolidColorBrush(Color.Parse("#9e9e9e"));
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
