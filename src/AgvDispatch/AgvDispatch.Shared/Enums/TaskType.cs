namespace AgvDispatch.Shared.Enums;

/// <summary>
/// 任务类型
/// </summary>
public enum TaskType
{
    /// <summary>
    /// 搬运任务
    /// </summary>
    Transport = 10,

    /// <summary>
    /// 充电任务
    /// </summary>
    Charge = 20,

    /// <summary>
    /// 返回待命点
    /// </summary>
    Return = 30
}

/// <summary>
/// TaskType 扩展方法
/// </summary>
public static class TaskTypeExtensions
{
    /// <summary>
    /// 获取任务类型显示文本
    /// </summary>
    public static string ToDisplayText(this TaskType type) => type switch
    {
        TaskType.Transport => "搬运任务",
        TaskType.Charge => "充电任务",
        TaskType.Return => "返回待命点",
        _ => "未知"
    };
}
