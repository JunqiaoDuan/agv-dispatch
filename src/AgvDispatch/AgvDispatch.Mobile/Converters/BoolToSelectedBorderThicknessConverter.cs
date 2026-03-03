using Avalonia;
using Avalonia.Data.Converters;
using System;
using System.Globalization;

namespace AgvDispatch.Mobile.Converters;

/// <summary>
/// 选中状态转边框厚度转换器
/// </summary>
public class BoolToSelectedBorderThicknessConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is bool isSelected)
        {
            return isSelected
                ? new Thickness(3) // 选中时边框加粗到3px
                : new Thickness(2); // 未选中时边框2px
        }
        return new Thickness(2);
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
