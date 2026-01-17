namespace AgvDispatch.Shared.Enums;

/// <summary>
/// 路径锁定响应状态
/// </summary>
public enum PathLockStatus
{
    /// <summary>
    /// 等待中
    /// </summary>
    Pending = 0,

    /// <summary>
    /// 已批准,允许通过
    /// </summary>
    Approved = 10,

    /// <summary>
    /// 已拒绝,路段被占用
    /// </summary>
    Rejected = 20,

}

/// <summary>
/// PathLockStatus 扩展方法
/// </summary>
public static class PathLockStatusExtensions
{
    /// <summary>
    /// 获取状态显示文本
    /// </summary>
    public static string ToDisplayText(this PathLockStatus status) => status switch
    {
        PathLockStatus.Pending => "等待中",
        PathLockStatus.Approved => "已批准",
        PathLockStatus.Rejected => "已拒绝",
        _ => "未知"
    };
}
