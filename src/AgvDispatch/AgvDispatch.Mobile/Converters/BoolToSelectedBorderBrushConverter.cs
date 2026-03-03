using Avalonia.Data.Converters;
using Avalonia.Media;
using System;
using System.Globalization;

namespace AgvDispatch.Mobile.Converters;

/// <summary>
/// 选中状态转边框颜色转换器
/// </summary>
public class BoolToSelectedBorderBrushConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is bool isSelected)
        {
            return isSelected
                ? new SolidColorBrush(Color.Parse("#4CAF50")) // 选中时使用绿色边框（加粗）
                : new SolidColorBrush(Color.Parse("#E0E0E0")); // 未选中时使用灰色边框
        }
        return new SolidColorBrush(Color.Parse("#E0E0E0"));
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
