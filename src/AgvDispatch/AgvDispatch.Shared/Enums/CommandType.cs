namespace AgvDispatch.Shared.Enums;

/// <summary>
/// 控制指令类型
/// </summary>
public enum CommandType
{
    /// <summary>
    /// 暂停
    /// </summary>
    Pause = 30,

    /// <summary>
    /// 继续
    /// </summary>
    Resume = 31,

    /// <summary>
    /// 急停
    /// </summary>
    Stop = 40,

    /// <summary>
    /// 返回待命点
    /// </summary>
    ReturnHome = 50
}

/// <summary>
/// CommandType 扩展方法
/// </summary>
public static class CommandTypeExtensions
{
    /// <summary>
    /// 获取指令类型显示文本
    /// </summary>
    public static string ToDisplayText(this CommandType type) => type switch
    {
        CommandType.Pause => "暂停",
        CommandType.Resume => "继续",
        CommandType.Stop => "急停",
        CommandType.ReturnHome => "返回待命点",
        _ => "未知"
    };
}
