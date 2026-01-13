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
    /// 弧线（通过经过点定义）
    /// </summary>
    Arc = 20,

    /// <summary>
    /// 弧线（通过曲率值定义）
    /// </summary>
    ArcWithCurvature = 21,
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
        EdgeType.Arc => "弧线-路径点",
        EdgeType.ArcWithCurvature => "弧线-曲率",
        _ => "未知"
    };
}
