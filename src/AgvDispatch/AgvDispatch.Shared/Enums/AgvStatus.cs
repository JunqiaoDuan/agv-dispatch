namespace AgvDispatch.Shared.Enums;

/// <summary>
/// 小车运行状态
/// </summary>
public enum AgvStatus
{
    /// <summary>
    /// 离线
    /// </summary>
    Offline = 0,

    /// <summary>
    /// 空闲
    /// </summary>
    Idle = 10,

    /// <summary>
    /// 执行任务中
    /// </summary>
    Running = 20,

    /// <summary>
    /// 充电中
    /// </summary>
    Charging = 30,

    /// <summary>
    /// 故障
    /// </summary>
    Error = 90,
}

/// <summary>
/// AgvStatus 扩展方法
/// </summary>
public static class AgvStatusExtensions
{
    /// <summary>
    /// 获取状态显示文本
    /// </summary>
    public static string ToDisplayText(this AgvStatus status) => status switch
    {
        AgvStatus.Offline => "离线",
        AgvStatus.Idle => "空闲",
        AgvStatus.Running => "运行中",
        AgvStatus.Charging => "充电中",
        AgvStatus.Error => "故障",
        _ => "未知"
    };
}
