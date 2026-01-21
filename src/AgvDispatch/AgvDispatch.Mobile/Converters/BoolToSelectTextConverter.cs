using Avalonia.Data.Converters;
using System;
using System.Globalization;

namespace AgvDispatch.Mobile.Converters;

/// <summary>
/// 将布尔值转换为选择按钮文本
/// true -> "选择", false -> "不可用"
/// </summary>
public class BoolToSelectTextConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is bool isAvailable)
        {
            return isAvailable ? "选择" : "不可用";
        }
        return "不可用";
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
