namespace AgvDispatch.Shared.Enums;

/// <summary>
/// 路线段到达后动作
/// </summary>
public enum FinalAction
{
    /// <summary>
    /// 无动作，直接通过
    /// </summary>
    None = 0,

    /// <summary>
    /// 停车
    /// </summary>
    Stop = 10,

}

/// <summary>
/// FinalAction 扩展方法
/// </summary>
public static class FinalActionExtensions
{
    /// <summary>
    /// 获取动作显示文本
    /// </summary>
    public static string ToDisplayText(this FinalAction action) => action switch
    {
        FinalAction.None => "无动作",
        FinalAction.Stop => "停车",
        _ => "未知"
    };
}
