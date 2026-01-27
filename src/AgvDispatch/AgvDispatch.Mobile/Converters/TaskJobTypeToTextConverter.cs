using Avalonia.Data.Converters;
using AgvDispatch.Shared.Enums;
using System;
using System.Globalization;

namespace AgvDispatch.Mobile.Converters;

/// <summary>
/// 任务类型转文本转换器
/// </summary>
public class TaskJobTypeToTextConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is not TaskJobType taskType)
        {
            return "未知";
        }

        return taskType switch
        {
            TaskJobType.CallForLoading => "上料",
            TaskJobType.SendToUnloading => "下料",
            TaskJobType.ReturnToWaiting => "返回等待",
            TaskJobType.SendToCharge => "充电",
            _ => "未知"
        };
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
