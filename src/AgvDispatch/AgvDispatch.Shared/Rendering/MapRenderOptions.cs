namespace AgvDispatch.Shared.Rendering;

/// <summary>
/// 地图渲染配置（所有尺寸单位为屏幕像素）
/// </summary>
public class MapRenderOptions
{
    #region 背景与网格

    /// <summary>
    /// 背景颜色
    /// </summary>
    public string BackgroundColor { get; set; } = "#F5F5F5";

    /// <summary>
    /// 网格颜色
    /// </summary>
    public string GridColor { get; set; } = "#E0E0E0";

    /// <summary>
    /// 网格线宽度
    /// </summary>
    public float GridLineWidth { get; set; } = 1f;

    /// <summary>
    /// 网格间距（地图坐标，cm）
    /// </summary>
    public float GridSpacing { get; set; } = 100f;

    /// <summary>
    /// 是否显示网格
    /// </summary>
    public bool ShowGrid { get; set; } = true;

    /// <summary>
    /// 地图边框线宽
    /// </summary>
    public float MapBorderWidth { get; set; } = 2f;

    /// <summary>
    /// 地图边框颜色
    /// </summary>
    public string MapBorderColor { get; set; } = "#999999";

    #endregion

    #region 节点

    /// <summary>
    /// 节点半径
    /// </summary>
    public float NodeRadius { get; set; } = 1f;

    /// <summary>
    /// 节点颜色
    /// </summary>
    public string NodeColor { get; set; } = "#666666";

    /// <summary>
    /// 节点边框颜色
    /// </summary>
    public string NodeBorderColor { get; set; } = "#555555";

    /// <summary>
    /// 节点边框宽度
    /// </summary>
    public float NodeBorderWidth { get; set; } = 1.5f;

    /// <summary>
    /// 选中节点高亮颜色（黄色边框）
    /// </summary>
    public string SelectedNodeHighlightColor { get; set; } = "#FFC107";

    /// <summary>
    /// 选中节点高亮边框宽度
    /// </summary>
    public float SelectedNodeHighlightWidth { get; set; } = 2f;

    /// <summary>
    /// 是否显示节点标签
    /// </summary>
    public bool ShowNodeLabels { get; set; } = false;

    #endregion

    #region 边

    /// <summary>
    /// 边线宽度
    /// </summary>
    public float EdgeWidth { get; set; } = 2f;

    /// <summary>
    /// 边颜色
    /// </summary>
    public string EdgeColor { get; set; } = "#757575";

    /// <summary>
    /// 选中边高亮颜色（黄色边框）
    /// </summary>
    public string SelectedEdgeHighlightColor { get; set; } = "#FFC107";

    /// <summary>
    /// 选中边高亮边框宽度增量
    /// </summary>
    public float SelectedEdgeHighlightWidth { get; set; } = 1.5f;

    /// <summary>
    /// 高亮边颜色（路线）
    /// </summary>
    public string HighlightedEdgeColor { get; set; } = "#4CAF50";

    /// <summary>
    /// 高亮边线宽度
    /// </summary>
    public float HighlightedEdgeWidth { get; set; } = 3f;

    /// <summary>
    /// 箭头大小
    /// </summary>
    public float ArrowSize { get; set; } = 8f;

    /// <summary>
    /// 是否显示边标签
    /// </summary>
    public bool ShowEdgeLabels { get; set; } = false;

    /// <summary>
    /// 是否显示箭头
    /// </summary>
    public bool ShowArrows { get; set; } = false;

    #endregion

    #region 标签与文字

    /// <summary>
    /// 标签字体大小
    /// </summary>
    public float LabelFontSize { get; set; } = 12f;

    /// <summary>
    /// 标签颜色
    /// </summary>
    public string LabelColor { get; set; } = "#333333";

    /// <summary>
    /// 标签与元素的间距
    /// </summary>
    public float LabelOffset { get; set; } = 10f;

    #endregion

    #region 站点

    /// <summary>
    /// 站点图标大小
    /// </summary>
    public float StationSize { get; set; } = 10f;

    /// <summary>
    /// 站点图标描边宽度
    /// </summary>
    public float StationBorderWidth { get; set; } = 1.5f;

    /// <summary>
    /// 站点选中高亮颜色（黄色边框）
    /// </summary>
    public string SelectedStationHighlightColor { get; set; } = "#FFC107";

    /// <summary>
    /// 站点选中高亮圈线宽
    /// </summary>
    public float SelectedStationHighlightWidth { get; set; } = 1.5f;

    /// <summary>
    /// 是否显示站点标签
    /// </summary>
    public bool ShowStationLabels { get; set; } = true;

    #endregion

    #region 路线序号

    /// <summary>
    /// 是否显示路线序号
    /// </summary>
    public bool ShowRouteSequence { get; set; } = true;

    /// <summary>
    /// 路线序号圆半径
    /// </summary>
    public float RouteSequenceRadius { get; set; } = 8f;

    /// <summary>
    /// 路线序号字体大小
    /// </summary>
    public float RouteSequenceFontSize { get; set; } = 9f;

    #endregion

    #region 模拟渲染

    /// <summary>
    /// 是否渲染模拟层
    /// </summary>
    public bool RenderSimulation { get; set; } = true;

    /// <summary>
    /// AGV 图标大小
    /// </summary>
    public float SimulationAgvSize { get; set; } = 20f;

    /// <summary>
    /// AGV 颜色
    /// </summary>
    public string SimulationAgvColor { get; set; } = "#2196F3";

    /// <summary>
    /// 起点标记颜色
    /// </summary>
    public string SimulationStartMarkerColor { get; set; } = "#4CAF50";

    /// <summary>
    /// 终点标记颜色
    /// </summary>
    public string SimulationEndMarkerColor { get; set; } = "#F44336";

    /// <summary>
    /// 已走路径颜色
    /// </summary>
    public string SimulationTraveledPathColor { get; set; } = "#2196F3";

    /// <summary>
    /// 已走路径线宽
    /// </summary>
    public float SimulationTraveledPathWidth { get; set; } = 3f;

    /// <summary>
    /// 到终点直线颜色
    /// </summary>
    public string SimulationToEndLineColor { get; set; } = "#9E9E9E";

    /// <summary>
    /// 到终点直线虚线样式
    /// </summary>
    public string SimulationToEndLineDashArray { get; set; } = "5,5";

    #endregion
}
