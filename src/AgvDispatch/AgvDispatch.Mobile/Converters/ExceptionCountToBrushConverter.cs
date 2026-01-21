using Avalonia.Data.Converters;
using Avalonia.Media;
using System;
using System.Globalization;

namespace AgvDispatch.Mobile.Converters;

/// <summary>
/// 异常数量转颜色转换器
/// </summary>
public class ExceptionCountToBrushConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is int count)
        {
            return count > 0
                ? new SolidColorBrush(Color.Parse("#f44336")) // Error color
                : new SolidColorBrush(Color.Parse("#9e9e9e")); // Gray color
        }
        return new SolidColorBrush(Color.Parse("#9e9e9e"));
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
