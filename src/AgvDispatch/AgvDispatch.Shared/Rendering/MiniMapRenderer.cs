using System.Globalization;
using System.Text;
using AgvDispatch.Shared.Simulation;

namespace AgvDispatch.Shared.Rendering;

/// <summary>
/// 小地图渲染器
/// </summary>
public class MiniMapRenderer
{
    private readonly MapRenderer _mapRenderer;
    private readonly MapRenderOptions _options;

    public MiniMapRenderer(MapRenderOptions? options = null)
    {
        // 小地图默认配置（不显示文字和网格）
        _options = options ?? new MapRenderOptions
        {
            ShowGrid = false,
            ShowArrows = false,
            ShowNodeLabels = false,
            ShowEdgeLabels = false,
            ShowStationLabels = false
        };

        _mapRenderer = new MapRenderer(_options);
    }

    /// <summary>
    /// 渲染小地图
    /// </summary>
    /// <param name="data">地图数据</param>
    /// <param name="minimapWidth">小地图宽度</param>
    /// <param name="minimapHeight">小地图高度</param>
    /// <param name="scale">缩放比例（输出参数）</param>
    /// <param name="offsetX">X轴偏移（输出参数）</param>
    /// <param name="offsetY">Y轴偏移（输出参数）</param>
    /// <param name="elementScale">元素缩放</param>
    /// <returns>小地图 SVG 字符串</returns>
    public string RenderMap(
        MapRenderData data,
        float minimapWidth,
        float minimapHeight,
        out float scale,
        out float offsetX,
        out float offsetY,
        float elementScale = 0.5f)
    {
        // 计算缩放比例
        var mapWidth = (float)data.Width;
        var mapHeight = (float)data.Height;
        scale = Math.Min(minimapWidth / mapWidth, minimapHeight / mapHeight);

        // 计算居中偏移
        var scaledWidth = mapWidth * scale;
        var scaledHeight = mapHeight * scale;
        offsetX = (minimapWidth - scaledWidth) / 2f;
        offsetY = (minimapHeight - scaledHeight) / 2f;

        // 创建小地图专用的渲染数据（不显示路线高亮和选中）
        var minimapRenderData = new MapRenderData
        {
            Width = data.Width,
            Height = data.Height,
            Nodes = data.Nodes,
            Edges = data.Edges,
            Stations = data.Stations,
            HighlightedEdgeIds = [], // 不显示路线
            SelectedNodeId = null,
            SelectedEdgeId = null,
            SelectedStationId = null,
            SimulatedAgvs = []
        };

        // 使用 MapRenderer 渲染
        return _mapRenderer.Render(
            minimapRenderData,
            minimapWidth,
            minimapHeight,
            scale,
            offsetX,
            offsetY,
            elementScale);
    }

    /// <summary>
    /// 渲染小地图覆盖层（视窗框、AGV 位置等）
    /// </summary>
    /// <param name="minimapWidth">小地图宽度</param>
    /// <param name="minimapHeight">小地图高度</param>
    /// <param name="minimapScale">小地图缩放比例</param>
    /// <param name="minimapOffsetX">小地图 X 偏移</param>
    /// <param name="minimapOffsetY">小地图 Y 偏移</param>
    /// <param name="mainViewScale">主视图缩放比例</param>
    /// <param name="mainViewOffsetX">主视图 X 偏移</param>
    /// <param name="mainViewOffsetY">主视图 Y 偏移</param>
    /// <param name="mainViewWidth">主视图宽度</param>
    /// <param name="mainViewHeight">主视图高度</param>
    /// <param name="agvPosition">AGV 位置（可选）</param>
    /// <param name="agvSize">AGV 图标大小</param>
    /// <param name="agvColor">AGV 颜色</param>
    /// <param name="endPosition">终点位置（可选）</param>
    /// <param name="endMarkerSize">终点标记大小</param>
    /// <param name="viewportColor">视窗框颜色</param>
    /// <param name="viewportStrokeWidth">视窗框线宽</param>
    /// <param name="viewportDashArray">视窗框虚线样式</param>
    /// <returns>覆盖层 SVG 字符串</returns>
    public string RenderOverlay(
        float minimapWidth,
        float minimapHeight,
        float minimapScale,
        float minimapOffsetX,
        float minimapOffsetY,
        float mainViewScale,
        float mainViewOffsetX,
        float mainViewOffsetY,
        float mainViewWidth,
        float mainViewHeight,
        AgvPosition? agvPosition = null,
        float agvSize = 16f,
        string agvColor = "#2196F3",
        (decimal X, decimal Y)? endPosition = null,
        float endMarkerSize = 12f,
        string viewportColor = "#FF5722",
        float viewportStrokeWidth = 2f,
        string viewportDashArray = "5,5")
    {
        var sb = new StringBuilder();

        // 创建覆盖层 SVG 容器
        sb.AppendLine($"<svg xmlns=\"http://www.w3.org/2000/svg\" width=\"{F(minimapWidth)}\" height=\"{F(minimapHeight)}\" " +
            $"style=\"position:absolute;top:0;left:0;pointer-events:none;\">");

        // 1. 渲染终点标记（如果有）
        if (endPosition != null)
        {
            sb.Append(RenderEndMarker(
                endPosition.Value.X,
                endPosition.Value.Y,
                minimapScale,
                minimapOffsetX,
                minimapOffsetY,
                endMarkerSize,
                agvColor));
        }

        // 2. 渲染 AGV 位置（如果有）
        if (agvPosition != null)
        {
            sb.Append(RenderAgvIcon(
                agvPosition,
                minimapScale,
                minimapOffsetX,
                minimapOffsetY,
                agvSize,
                agvColor));
        }

        // 3. 渲染视窗矩形框
        sb.Append(RenderViewport(
            minimapScale,
            minimapOffsetX,
            minimapOffsetY,
            mainViewScale,
            mainViewOffsetX,
            mainViewOffsetY,
            mainViewWidth,
            mainViewHeight,
            viewportColor,
            viewportStrokeWidth,
            viewportDashArray));

        sb.AppendLine("</svg>");

        return sb.ToString();
    }

    #region 私有渲染方法

    /// <summary>
    /// 渲染终点标记（空心圆球）
    /// </summary>
    private string RenderEndMarker(
        decimal endX,
        decimal endY,
        float minimapScale,
        float minimapOffsetX,
        float minimapOffsetY,
        float size,
        string color)
    {
        // 转换到小地图坐标
        var markerX = minimapOffsetX + (float)endX * minimapScale;
        var markerY = minimapOffsetY + (float)endY * minimapScale;
        var radius = size * 0.5f;

        // 绘制空心圆球（stroke 显示为圆环）
        return $"  <circle cx=\"{F(markerX)}\" cy=\"{F(markerY)}\" r=\"{F(radius)}\" " +
            $"fill=\"none\" stroke=\"{color}\" stroke-width=\"2\" />\n";
    }

    /// <summary>
    /// 渲染 AGV 图标（三角形）
    /// </summary>
    private string RenderAgvIcon(
        AgvPosition position,
        float minimapScale,
        float minimapOffsetX,
        float minimapOffsetY,
        float size,
        string color)
    {
        var sb = new StringBuilder();

        // 转换到小地图坐标
        var agvX = minimapOffsetX + (float)position.X * minimapScale;
        var agvY = minimapOffsetY + (float)position.Y * minimapScale;
        var agvAngle = position.Angle;

        // 三角形顶点（未旋转时，箭头指向右侧）
        var halfSize = size * 0.5f;
        var tipLength = size * 0.8f;

        var p1X = tipLength;
        var p1Y = 0f;
        var p2X = -tipLength * 0.3f;
        var p2Y = -halfSize;
        var p3X = -tipLength * 0.3f;
        var p3Y = halfSize;

        // 旋转并平移
        var cos = (float)Math.Cos(agvAngle);
        var sin = (float)Math.Sin(agvAngle);

        float RotateX(float px, float py) => agvX + px * cos - py * sin;
        float RotateY(float px, float py) => agvY + px * sin + py * cos;

        var r1X = RotateX(p1X, p1Y);
        var r1Y = RotateY(p1X, p1Y);
        var r2X = RotateX(p2X, p2Y);
        var r2Y = RotateY(p2X, p2Y);
        var r3X = RotateX(p3X, p3Y);
        var r3Y = RotateY(p3X, p3Y);

        // 绘制填充的三角形
        sb.AppendLine($"  <path d=\"M{F(r1X)},{F(r1Y)} L{F(r2X)},{F(r2Y)} L{F(r3X)},{F(r3Y)} Z\" " +
            $"fill=\"{color}\" stroke=\"white\" stroke-width=\"1\" />");

        return sb.ToString();
    }

    /// <summary>
    /// 渲染视窗矩形框
    /// </summary>
    private string RenderViewport(
        float minimapScale,
        float minimapOffsetX,
        float minimapOffsetY,
        float mainViewScale,
        float mainViewOffsetX,
        float mainViewOffsetY,
        float mainViewWidth,
        float mainViewHeight,
        string color,
        float strokeWidth,
        string dashArray)
    {
        // 计算当前视窗在地图上的位置和大小
        var viewportMapX = -mainViewOffsetX / mainViewScale;
        var viewportMapY = -mainViewOffsetY / mainViewScale;
        var viewportMapWidth = mainViewWidth / mainViewScale;
        var viewportMapHeight = mainViewHeight / mainViewScale;

        // 转换到小地图坐标
        var viewportMinimapX = minimapOffsetX + viewportMapX * minimapScale;
        var viewportMinimapY = minimapOffsetY + viewportMapY * minimapScale;
        var viewportMinimapWidth = viewportMapWidth * minimapScale;
        var viewportMinimapHeight = viewportMapHeight * minimapScale;

        // 绘制视窗矩形框
        return $"  <rect x=\"{F(viewportMinimapX)}\" y=\"{F(viewportMinimapY)}\" " +
            $"width=\"{F(viewportMinimapWidth)}\" height=\"{F(viewportMinimapHeight)}\" " +
            $"fill=\"none\" stroke=\"{color}\" stroke-width=\"{F(strokeWidth)}\" stroke-dasharray=\"{dashArray}\" />\n";
    }

    #endregion

    #region Helper

    /// <summary>
    /// 格式化浮点数（避免本地化问题）
    /// </summary>
    private static string F(float value)
    {
        return value.ToString("F2", CultureInfo.InvariantCulture);
    }

    #endregion
}
