using Avalonia.Data.Converters;
using Avalonia.Media;
using System;
using System.Globalization;

namespace AgvDispatch.Mobile.Converters;

/// <summary>
/// 布尔值转颜色转换器
/// </summary>
public class BoolToBrushConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is bool boolValue)
        {
            return boolValue
                ? new SolidColorBrush(Color.Parse("#2196f3")) // Info color for has cargo
                : new SolidColorBrush(Color.Parse("#9e9e9e")); // Gray color for no cargo
        }
        return new SolidColorBrush(Color.Parse("#9e9e9e"));
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
