namespace AgvDispatch.Shared.Enums;

/// <summary>
/// 任务类型(手动调度版)
/// </summary>
public enum TaskJobType
{
    /// <summary>
    /// 召唤小车上料
    /// </summary>
    CallForLoading = 10,

    /// <summary>
    /// 告知小车去下料
    /// </summary>
    SendToUnloading = 20,

    /// <summary>
    /// 确认下料去等待区
    /// </summary>
    ReturnToWaiting = 30,

    /// <summary>
    /// 让小车充电
    /// </summary>
    SendToCharge = 40
}

/// <summary>
/// TaskType 扩展方法
/// </summary>
public static class TaskTypeExtensions
{
    /// <summary>
    /// 获取任务类型显示文本
    /// </summary>
    public static string ToDisplayText(this TaskJobType type) => type switch
    {
        TaskJobType.CallForLoading => "召唤小车上料",
        TaskJobType.SendToUnloading => "告知小车去下料",
        TaskJobType.ReturnToWaiting => "确认下料去等待区",
        TaskJobType.SendToCharge => "让小车充电",
        _ => "未知"
    };
}
