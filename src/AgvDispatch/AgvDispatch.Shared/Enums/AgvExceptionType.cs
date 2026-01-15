namespace AgvDispatch.Shared.Enums;

/// <summary>
/// 异常类型
/// </summary>
public enum AgvExceptionType
{
    /// <summary>
    /// 检测到障碍物
    /// </summary>
    ObstacleDetected = 10,

    /// <summary>
    /// 低电量
    /// </summary>
    LowBattery = 20,

    /// <summary>
    /// 网络异常
    /// </summary>
    NetworkError = 30,

    /// <summary>
    /// GPS信号异常
    /// </summary>
    GpsError = 31,

    /// <summary>
    /// 急停按钮触发
    /// </summary>
    EmergencyStop = 40,

    /// <summary>
    /// 其他
    /// </summary>
    Other = 80
}

/// <summary>
/// ExceptionType 扩展方法
/// </summary>
public static class AgvExceptionTypeExtensions
{
    /// <summary>
    /// 获取异常类型显示文本
    /// </summary>
    public static string ToDisplayText(this AgvExceptionType type) => type switch
    {
        AgvExceptionType.ObstacleDetected => "检测到障碍物",
        AgvExceptionType.LowBattery => "低电量",
        AgvExceptionType.NetworkError => "网络异常",
        AgvExceptionType.GpsError => "GPS信号异常",
        AgvExceptionType.EmergencyStop => "急停按钮触发",
        AgvExceptionType.Other => "其他",
        _ => "未知"
    };
}
