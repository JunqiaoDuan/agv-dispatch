namespace AgvDispatch.Shared.Simulation;

/// <summary>
/// AGV 位置信息
/// </summary>
public class AgvPosition
{
    /// <summary>
    /// X 坐标（厘米）
    /// </summary>
    public decimal X { get; set; }

    /// <summary>
    /// Y 坐标（厘米）
    /// </summary>
    public decimal Y { get; set; }

    /// <summary>
    /// 朝向角度（弧度，0 为正东方向）
    /// </summary>
    public double Angle { get; set; }

    /// <summary>
    /// 当前所在边的索引
    /// </summary>
    public int CurrentEdgeIndex { get; set; }

    /// <summary>
    /// 当前边内的进度（0-1）
    /// </summary>
    public decimal EdgeProgress { get; set; }

    /// <summary>
    /// 总进度（0-1）
    /// </summary>
    public decimal OverallProgress { get; set; }

    /// <summary>
    /// 已行驶距离（厘米）
    /// </summary>
    public decimal TraveledDistance { get; set; }
}
