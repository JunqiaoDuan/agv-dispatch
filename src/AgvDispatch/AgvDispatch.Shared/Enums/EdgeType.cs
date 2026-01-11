namespace AgvDispatch.Shared.Enums;

/// <summary>
/// 地图边类型
/// </summary>
public enum EdgeType
{
    /// <summary>
    /// 直线
    /// </summary>
    Line = 10,

    /// <summary>
    /// 弧线
    /// </summary>
    Arc = 20,
}

/// <summary>
/// EdgeType 扩展方法
/// </summary>
public static class EdgeTypeExtensions
{
    /// <summary>
    /// 获取边类型显示文本
    /// </summary>
    public static string ToDisplayText(this EdgeType type) => type switch
    {
        EdgeType.Line => "直线",
        EdgeType.Arc => "弧线",
        _ => "未知"
    };
}
