using Avalonia.Data.Converters;
using System;
using System.Globalization;

namespace AgvDispatch.Mobile.Converters;

/// <summary>
/// 布尔值转货物文本转换器
/// </summary>
public class BoolToCargoTextConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is bool boolValue)
        {
            return boolValue ? "有货物" : "无货物";
        }
        return "未知";
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
