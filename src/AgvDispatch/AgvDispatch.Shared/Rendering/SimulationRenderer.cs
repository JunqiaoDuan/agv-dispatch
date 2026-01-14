using System.Globalization;
using System.Text;
using AgvDispatch.Shared.DTOs.MapNodes;
using AgvDispatch.Shared.Simulation;

namespace AgvDispatch.Shared.Rendering;

/// <summary>
/// 模拟渲染器（生成独立的 SVG 层）
/// </summary>
public class SimulationRenderer
{
    private readonly MapRenderOptions _options;

    public SimulationRenderer(MapRenderOptions? options = null)
    {
        _options = options ?? new MapRenderOptions();
    }

    /// <summary>
    /// 渲染模拟层 SVG
    /// </summary>
    /// <param name="data">渲染数据</param>
    /// <param name="viewWidth">视口宽度（像素）</param>
    /// <param name="viewHeight">视口高度（像素）</param>
    /// <param name="scale">缩放比例</param>
    /// <param name="offsetX">X轴偏移（像素）</param>
    /// <param name="offsetY">Y轴偏移（像素）</param>
    /// <param name="elementScale">元素缩放比例</param>
    /// <returns>SVG 字符串</returns>
    public string RenderSimulationLayer(
        MapRenderData data,
        float viewWidth,
        float viewHeight,
        float scale,
        float offsetX,
        float offsetY,
        float elementScale = 1.0f)
    {
        if (!_options.RenderSimulation || data.SimulatedAgvs.Count == 0)
            return string.Empty;

        var sb = new StringBuilder();
        var visualScale = elementScale / scale;

        // 创建节点字典
        var nodeDict = data.Nodes.ToDictionary(n => n.Id);
        var edgeDict = data.Edges.ToDictionary(e => e.Id);

        sb.AppendLine($"<svg xmlns=\"http://www.w3.org/2000/svg\" width=\"{F(viewWidth)}\" height=\"{F(viewHeight)}\" " +
            $"style=\"position: absolute; top: 0; left: 0; pointer-events: none;\">");

        sb.AppendLine($"<g transform=\"translate({F(offsetX)},{F(offsetY)}) scale({F(scale)})\">");

        // 渲染每个模拟 AGV
        foreach (var agv in data.SimulatedAgvs)
        {
            sb.Append(RenderAgvSimulation(agv, nodeDict, edgeDict, visualScale));
        }

        sb.AppendLine("</g>");
        sb.AppendLine("</svg>");

        return sb.ToString();
    }

    /// <summary>
    /// 渲染单个 AGV 模拟
    /// </summary>
    private string RenderAgvSimulation(
        AgvSimulationData agv,
        Dictionary<Guid, MapNodeListItemDto> nodeDict,
        Dictionary<Guid, DTOs.MapEdges.MapEdgeListItemDto> edgeDict,
        float visualScale)
    {
        var sb = new StringBuilder();
        var pos = agv.Position;
        var config = agv.Config;

        // 1. 绘制起点标记
        if (nodeDict.TryGetValue(agv.StartNodeId, out var startNode))
        {
            sb.Append(RenderMarker(
                (float)startNode.X,
                (float)startNode.Y,
                config.StartMarkerColor,
                "S",
                visualScale));
        }

        // 2. 绘制终点标记
        if (nodeDict.TryGetValue(agv.EndNodeId, out var endNode))
        {
            sb.Append(RenderMarker(
                (float)endNode.X,
                (float)endNode.Y,
                config.EndMarkerColor,
                "E",
                visualScale));
        }

        // 3. 绘制已走路径（从起点到当前位置）
        if (pos.CurrentEdgeIndex >= 0 && pos.CurrentEdgeIndex < agv.PathEdgeIds.Count)
        {
            sb.Append(RenderTraveledPath(
                agv.PathEdgeIds,
                pos.CurrentEdgeIndex,
                pos.EdgeProgress,
                nodeDict,
                edgeDict,
                config,
                visualScale));
        }

        // 4. 绘制到终点的直线（虚线）
        if (endNode != null)
        {
            var toEndLineWidth = config.TraveledPathWidth * visualScale * 0.7f;
            sb.AppendLine($"  <line x1=\"{F((float)pos.X)}\" y1=\"{F((float)pos.Y)}\" " +
                $"x2=\"{F((float)endNode.X)}\" y2=\"{F((float)endNode.Y)}\" " +
                $"stroke=\"{config.ToEndLineColor}\" stroke-width=\"{F(toEndLineWidth)}\" " +
                $"stroke-dasharray=\"{config.ToEndLineDashArray}\" stroke-linecap=\"round\"/>");
        }

        // 5. 绘制 AGV 图标
        sb.Append(RenderAgvIcon(
            (float)pos.X,
            (float)pos.Y,
            pos.Angle,
            config.AgvSize * visualScale,
            config.AgvColor));

        return sb.ToString();
    }

    /// <summary>
    /// 渲染起点/终点标记
    /// </summary>
    private string RenderMarker(float x, float y, string color, string label, float visualScale)
    {
        var sb = new StringBuilder();
        var markerRadius = 12f * visualScale;
        var fontSize = 10f * visualScale;

        // 外圈
        sb.AppendLine($"  <circle cx=\"{F(x)}\" cy=\"{F(y)}\" r=\"{F(markerRadius)}\" " +
            $"fill=\"{color}\" stroke=\"white\" stroke-width=\"{F(2f * visualScale)}\"/>");

        // 标签文字
        sb.AppendLine($"  <text x=\"{F(x)}\" y=\"{F(y + fontSize * 0.35f)}\" " +
            $"text-anchor=\"middle\" fill=\"white\" " +
            $"font-size=\"{F(fontSize)}\" font-family=\"sans-serif\" font-weight=\"bold\">{label}</text>");

        return sb.ToString();
    }

    /// <summary>
    /// 渲染已走路径
    /// </summary>
    private string RenderTraveledPath(
        List<Guid> pathEdgeIds,
        int currentEdgeIndex,
        decimal edgeProgress,
        Dictionary<Guid, MapNodeListItemDto> nodeDict,
        Dictionary<Guid, DTOs.MapEdges.MapEdgeListItemDto> edgeDict,
        Simulation.AgvSimulationConfig config,
        float visualScale)
    {
        var sb = new StringBuilder();
        var pathWidth = config.TraveledPathWidth * visualScale;

        // 绘制已完全走过的边
        for (int i = 0; i < currentEdgeIndex; i++)
        {
            if (i >= pathEdgeIds.Count) break;

            var edgeId = pathEdgeIds[i];
            if (!edgeDict.TryGetValue(edgeId, out var edge)) continue;
            if (!nodeDict.TryGetValue(edge.StartNodeId, out var startNode)) continue;
            if (!nodeDict.TryGetValue(edge.EndNodeId, out var endNode)) continue;

            // 根据边类型渲染
            sb.Append(RenderEdgePath(edge, startNode, endNode, config.TraveledPathColor, pathWidth, 1.0m));
        }

        // 绘制当前正在走的边（部分）
        if (currentEdgeIndex < pathEdgeIds.Count)
        {
            var edgeId = pathEdgeIds[currentEdgeIndex];
            if (edgeDict.TryGetValue(edgeId, out var edge) &&
                nodeDict.TryGetValue(edge.StartNodeId, out var startNode) &&
                nodeDict.TryGetValue(edge.EndNodeId, out var endNode))
            {
                // 渲染部分路径
                sb.Append(RenderEdgePath(edge, startNode, endNode, config.TraveledPathColor, pathWidth, edgeProgress));
            }
        }

        return sb.ToString();
    }

    /// <summary>
    /// 渲染路径线段
    /// </summary>
    private string RenderPathSegment(float x1, float y1, float x2, float y2, string color, float width)
    {
        return $"  <line x1=\"{F(x1)}\" y1=\"{F(y1)}\" x2=\"{F(x2)}\" y2=\"{F(y2)}\" " +
            $"stroke=\"{color}\" stroke-width=\"{F(width)}\" stroke-linecap=\"round\"/>\n";
    }

    /// <summary>
    /// 根据边类型渲染路径（支持直线和曲线）
    /// </summary>
    private string RenderEdgePath(
        DTOs.MapEdges.MapEdgeListItemDto edge,
        MapNodeListItemDto startNode,
        MapNodeListItemDto endNode,
        string color,
        float width,
        decimal progress)
    {
        var sb = new StringBuilder();

        // 如果是完整路径
        if (progress >= 1.0m)
        {
            switch (edge.EdgeType)
            {
                case Enums.EdgeType.Arc when edge.ArcViaX.HasValue && edge.ArcViaY.HasValue:
                    // 使用二次贝塞尔曲线渲染弧线
                    var pathDataArc = BezierCurveHelper.GenerateSvgPath(
                        (float)startNode.X, (float)startNode.Y,
                        (float)edge.ArcViaX.Value, (float)edge.ArcViaY.Value,
                        (float)endNode.X, (float)endNode.Y);
                    sb.AppendLine($"  <path d=\"{pathDataArc}\" " +
                        $"stroke=\"{color}\" stroke-width=\"{F(width)}\" " +
                        $"fill=\"none\" stroke-linecap=\"round\"/>");
                    break;

                case Enums.EdgeType.ArcWithCurvature when edge.Curvature.HasValue && edge.Curvature.Value != 0:
                    // 计算控制点
                    var (controlX, controlY) = BezierCurveHelper.CalculateControlPoint(
                        startNode.X, startNode.Y,
                        endNode.X, endNode.Y,
                        edge.Curvature.Value);

                    var pathDataCurve = BezierCurveHelper.GenerateSvgPath(
                        (float)startNode.X, (float)startNode.Y,
                        (float)controlX, (float)controlY,
                        (float)endNode.X, (float)endNode.Y);
                    sb.AppendLine($"  <path d=\"{pathDataCurve}\" " +
                        $"stroke=\"{color}\" stroke-width=\"{F(width)}\" " +
                        $"fill=\"none\" stroke-linecap=\"round\"/>");
                    break;

                default:
                    // 直线
                    sb.Append(RenderPathSegment(
                        (float)startNode.X, (float)startNode.Y,
                        (float)endNode.X, (float)endNode.Y,
                        color, width));
                    break;
            }
        }
        else
        {
            // 部分路径：使用插值计算当前位置，然后根据类型绘制部分曲线
            var (currentX, currentY, _) = Simulation.PathInterpolator.Interpolate(
                edge, startNode, endNode, progress);

            switch (edge.EdgeType)
            {
                case Enums.EdgeType.Arc when edge.ArcViaX.HasValue && edge.ArcViaY.HasValue:
                    // 使用贝塞尔曲线分段渲染
                    var pathDataPartialArc = BezierCurveHelper.GeneratePartialSvgPath(
                        (float)startNode.X, (float)startNode.Y,
                        (float)edge.ArcViaX.Value, (float)edge.ArcViaY.Value,
                        (float)endNode.X, (float)endNode.Y,
                        (float)progress);
                    sb.AppendLine($"  <path d=\"{pathDataPartialArc}\" " +
                        $"stroke=\"{color}\" stroke-width=\"{F(width)}\" " +
                        $"fill=\"none\" stroke-linecap=\"round\"/>");
                    break;

                case Enums.EdgeType.ArcWithCurvature when edge.Curvature.HasValue && edge.Curvature.Value != 0:
                    // 计算控制点
                    var (controlX2, controlY2) = BezierCurveHelper.CalculateControlPoint(
                        startNode.X, startNode.Y,
                        endNode.X, endNode.Y,
                        edge.Curvature.Value);

                    var pathDataPartialCurve = BezierCurveHelper.GeneratePartialSvgPath(
                        (float)startNode.X, (float)startNode.Y,
                        (float)controlX2, (float)controlY2,
                        (float)endNode.X, (float)endNode.Y,
                        (float)progress);
                    sb.AppendLine($"  <path d=\"{pathDataPartialCurve}\" " +
                        $"stroke=\"{color}\" stroke-width=\"{F(width)}\" " +
                        $"fill=\"none\" stroke-linecap=\"round\"/>");
                    break;

                default:
                    // 直线
                    sb.Append(RenderPathSegment(
                        (float)startNode.X, (float)startNode.Y,
                        (float)currentX, (float)currentY,
                        color, width));
                    break;
            }
        }

        return sb.ToString();
    }

    /// <summary>
    /// 渲染 AGV 图标（三角形）
    /// </summary>
    private string RenderAgvIcon(float x, float y, double angle, float size, string color)
    {
        var sb = new StringBuilder();

        // 三角形顶点（未旋转时，箭头指向右侧）
        var halfSize = size * 0.5f;
        var tipLength = size * 0.8f;

        // 顶点坐标
        var p1X = tipLength;
        var p1Y = 0f;
        var p2X = -tipLength * 0.3f;
        var p2Y = -halfSize;
        var p3X = -tipLength * 0.3f;
        var p3Y = halfSize;

        // 旋转并平移
        var cos = (float)Math.Cos(angle);
        var sin = (float)Math.Sin(angle);

        float RotateX(float px, float py) => x + px * cos - py * sin;
        float RotateY(float px, float py) => y + px * sin + py * cos;

        var r1X = RotateX(p1X, p1Y);
        var r1Y = RotateY(p1X, p1Y);
        var r2X = RotateX(p2X, p2Y);
        var r2Y = RotateY(p2X, p2Y);
        var r3X = RotateX(p3X, p3Y);
        var r3Y = RotateY(p3X, p3Y);

        // 绘制填充的三角形
        sb.AppendLine($"  <path d=\"M{F(r1X)},{F(r1Y)} L{F(r2X)},{F(r2Y)} L{F(r3X)},{F(r3Y)} Z\" " +
            $"fill=\"{color}\" stroke=\"white\" stroke-width=\"2\"/>");

        return sb.ToString();
    }

    /// <summary>
    /// 格式化浮点数
    /// </summary>
    private static string F(float value)
    {
        return value.ToString("F2", CultureInfo.InvariantCulture);
    }
}
