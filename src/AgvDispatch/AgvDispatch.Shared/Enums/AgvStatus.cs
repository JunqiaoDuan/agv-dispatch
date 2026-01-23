namespace AgvDispatch.Shared.Enums;

/// <summary>
/// 小车连接状态
/// </summary>
public enum AgvStatus
{
    /// <summary>
    /// 离线
    /// </summary>
    Offline = 0,

    /// <summary>
    /// 在线
    /// </summary>
    Online = 10,
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
        AgvStatus.Online => "在线",
        _ => "未知"
    };
}
