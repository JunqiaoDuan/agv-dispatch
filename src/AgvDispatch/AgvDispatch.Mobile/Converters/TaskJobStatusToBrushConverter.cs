using Avalonia.Data.Converters;
using Avalonia.Media;
using AgvDispatch.Shared.Enums;
using System;
using System.Globalization;

namespace AgvDispatch.Mobile.Converters;

/// <summary>
/// 任务状态转颜色转换器
/// </summary>
public class TaskJobStatusToBrushConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is not TaskJobStatus status)
        {
            return new SolidColorBrush(Color.Parse("#9E9E9E")); // Gray
        }

        return status switch
        {
            TaskJobStatus.Pending => new SolidColorBrush(Color.Parse("#9E9E9E")),      // Gray
            TaskJobStatus.Assigned => new SolidColorBrush(Color.Parse("#FF9800")),     // Orange
            TaskJobStatus.Executing => new SolidColorBrush(Color.Parse("#2196F3")),    // Blue
            TaskJobStatus.Completed => new SolidColorBrush(Color.Parse("#4CAF50")),    // Green
            TaskJobStatus.Cancelled => new SolidColorBrush(Color.Parse("#9E9E9E")),    // Gray
            TaskJobStatus.Failed => new SolidColorBrush(Color.Parse("#F44336")),       // Red
            _ => new SolidColorBrush(Color.Parse("#9E9E9E"))
        };
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
