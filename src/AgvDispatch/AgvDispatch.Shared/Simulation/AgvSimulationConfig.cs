namespace AgvDispatch.Shared.Simulation;

/// <summary>
/// AGV 模拟配置
/// </summary>
public class AgvSimulationConfig
{
    /// <summary>
    /// 速度（厘米/秒）
    /// </summary>
    public decimal Speed { get; set; } = 100m;

    /// <summary>
    /// 更新间隔（毫秒）
    /// </summary>
    public int UpdateIntervalMs { get; set; } = 50;

    /// <summary>
    /// AGV 图标大小（像素）
    /// </summary>
    public float AgvSize { get; set; } = 20f;

    /// <summary>
    /// AGV 颜色
    /// </summary>
    public string AgvColor { get; set; } = "#2196F3";

    /// <summary>
    /// 起点标记颜色
    /// </summary>
    public string StartMarkerColor { get; set; } = "#4CAF50";

    /// <summary>
    /// 终点标记颜色
    /// </summary>
    public string EndMarkerColor { get; set; } = "#F44336";

    /// <summary>
    /// 已走路径颜色
    /// </summary>
    public string TraveledPathColor { get; set; } = "#2196F3";

    /// <summary>
    /// 已走路径线宽
    /// </summary>
    public float TraveledPathWidth { get; set; } = 3f;

    /// <summary>
    /// 到终点直线颜色
    /// </summary>
    public string ToEndLineColor { get; set; } = "#9E9E9E";

    /// <summary>
    /// 到终点直线样式
    /// </summary>
    public string ToEndLineDashArray { get; set; } = "5,5";
}
