namespace AgvDispatch.Shared.Enums;

/// <summary>
/// 边行驶方向
/// </summary>
public enum DriveDirection
{
    /// <summary>
    /// 正向（StartNode → EndNode）
    /// </summary>
    Forward = 10,

    /// <summary>
    /// 反向（EndNode → StartNode）
    /// </summary>
    Backward = 20,
}

/// <summary>
/// DriveDirection 扩展方法
/// </summary>
public static class DriveDirectionExtensions
{
    /// <summary>
    /// 获取行驶方向显示文本
    /// </summary>
    public static string ToDisplayText(this DriveDirection direction) => direction switch
    {
        DriveDirection.Forward => "正向",
        DriveDirection.Backward => "反向",
        _ => "未知"
    };
}
