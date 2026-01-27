namespace AgvDispatch.Shared.Enums;

/// <summary>
/// 路径锁定状态
/// </summary>
public enum PathLockStatus
{
    /// <summary>
    /// 等待处理
    /// </summary>
    Pending = 0,

    /// <summary>
    /// 已批准（当前活跃）
    /// </summary>
    Approved = 10,

    /// <summary>
    /// 已拒绝
    /// </summary>
    Rejected = 20,

    /// <summary>
    /// 已释放
    /// </summary>
    Released = 30
}

/// <summary>
/// PathLockStatus 扩展方法
/// </summary>
public static class PathLockStatusExtensions
{
    /// <summary>
    /// 获取锁定状态显示文本
    /// </summary>
    public static string ToDisplayText(this PathLockStatus status) => status switch
    {
        PathLockStatus.Pending => "等待处理",
        PathLockStatus.Approved => "已批准",
        PathLockStatus.Rejected => "已拒绝",
        PathLockStatus.Released => "已释放",
        _ => "未知"
    };
}
