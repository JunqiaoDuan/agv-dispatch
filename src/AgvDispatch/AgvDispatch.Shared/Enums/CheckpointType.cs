namespace AgvDispatch.Shared.Enums;

/// <summary>
/// 检查点类型
/// </summary>
public enum CheckpointType
{
    /// <summary>
    /// 起点
    /// </summary>
    Start = 10,

    /// <summary>
    /// 中间点(需要申请通行权)
    /// </summary>
    Middle = 20,

    /// <summary>
    /// 终点
    /// </summary>
    End = 30
}

/// <summary>
/// CheckpointType 扩展方法
/// </summary>
public static class CheckpointTypeExtensions
{
    /// <summary>
    /// 获取检查点类型显示文本
    /// </summary>
    public static string ToDisplayText(this CheckpointType type) => type switch
    {
        CheckpointType.Start => "起点",
        CheckpointType.Middle => "中间点",
        CheckpointType.End => "终点",
        _ => "未知"
    };
}
