using Avalonia.Data.Converters;
using Avalonia.Media;
using System;
using System.Globalization;

namespace AgvDispatch.Mobile.Converters;

/// <summary>
/// 选中状态转背景色转换器
/// </summary>
public class BoolToSelectedBackgroundConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is bool isSelected)
        {
            return isSelected
                ? new SolidColorBrush(Color.Parse("#E8F5E9")) // 选中时使用浅绿色背景
                : new SolidColorBrush(Colors.White); // 未选中时使用白色背景
        }
        return new SolidColorBrush(Colors.White);
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
