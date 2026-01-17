namespace AgvDispatch.Shared.Enums;

/// <summary>
/// 任务状态
/// </summary>
public enum TaskJobStatus
{
    /// <summary>
    /// 待分配
    /// </summary>
    Pending = 0,

    /// <summary>
    /// 已分配（已下发,待执行）
    /// </summary>
    Assigned = 10,

    /// <summary>
    /// 执行中
    /// </summary>
    Executing = 20,

    /// <summary>
    /// 已完成
    /// </summary>
    Completed = 30,

    /// <summary>
    /// 已取消
    /// </summary>
    Cancelled = 40,

    /// <summary>
    /// 失败
    /// </summary>
    Failed = 50
}

/// <summary>
/// TaskStatus 扩展方法
/// </summary>
public static class TaskStatusExtensions
{
    /// <summary>
    /// 获取状态显示文本
    /// </summary>
    public static string ToDisplayText(this TaskJobStatus status) => status switch
    {
        TaskJobStatus.Pending => "待分配",
        TaskJobStatus.Assigned => "已分配",
        TaskJobStatus.Executing => "执行中",
        TaskJobStatus.Completed => "已完成",
        TaskJobStatus.Cancelled => "已取消",
        TaskJobStatus.Failed => "失败",
        _ => "未知"
    };
}
