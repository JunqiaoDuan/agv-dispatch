using System.Globalization;
using System.Text;
using AgvDispatch.Shared.DTOs.MapEdges;
using AgvDispatch.Shared.DTOs.MapNodes;
using AgvDispatch.Shared.DTOs.Stations;
using AgvDispatch.Shared.Enums;

namespace AgvDispatch.Shared.Rendering;

/// <summary>
/// SVG 地图渲染器（跨平台共享）
/// </summary>
public class MapRenderer
{
    private readonly MapRenderOptions _options;

    public MapRenderer(MapRenderOptions? options = null)
    {
        _options = options ?? new MapRenderOptions();
    }

    #region 渲染地图

    /// <summary>
    /// 生成地图 SVG
    /// </summary>
    /// <param name="data">渲染数据</param>
    /// <param name="viewWidth">视口宽度（像素）</param>
    /// <param name="viewHeight">视口高度（像素）</param>
    /// <param name="scale">缩放比例</param>
    /// <param name="offsetX">X轴偏移（像素）</param>
    /// <param name="offsetY">Y轴偏移（像素）</param>
    /// <param name="elementScale">元素缩放比例（点/线/文字）</param>
    /// <returns>SVG 字符串</returns>
    public string Render(
        MapRenderData data,
        float viewWidth,
        float viewHeight,
        float scale,
        float offsetX,
        float offsetY,
        float elementScale = 1.0f)
    {
        var sb = new StringBuilder();

        // 计算地图的实际渲染尺寸
        var mapWidth = (float)data.Width;
        var mapHeight = (float)data.Height;

        // 逆向补偿：让元素尺寸不随画布缩放变化
        // 经过外层 scale 变换后，最终尺寸 = baseSize * elementScale
        var visualScale = elementScale / scale;

        #region <svg>

        sb.AppendLine($"<svg xmlns=\"http://www.w3.org/2000/svg\" width=\"{F(viewWidth)}\" height=\"{F(viewHeight)}\" style=\"background-color: {_options.BackgroundColor};\">");

        #region <defs>

        sb.AppendLine("<defs>");

        #region <pattern> 网格图案

        // 网格图案（网格跟随地图坐标系，不做逆向补偿，但线宽保持固定）
        if (_options.ShowGrid)
        {
            var gridSize = _options.GridSpacing * scale;
            var gridLineWidth = _options.GridLineWidth * elementScale; // 网格线宽保持固定视觉大小
            sb.AppendLine($"  <pattern id=\"grid\" width=\"{F(gridSize)}\" height=\"{F(gridSize)}\" patternUnits=\"userSpaceOnUse\">");
            sb.AppendLine($"    <path d=\"M {F(gridSize)} 0 L 0 0 0 {F(gridSize)}\" fill=\"none\" stroke=\"{_options.GridColor}\" stroke-width=\"{F(gridLineWidth)}\"/>");
            sb.AppendLine("  </pattern>");
        }

        #endregion </pattern>

        #region <marker> 箭头标记

        // 箭头标记（使用 visualScale 保持固定视觉大小）
        var arrowSize = _options.ArrowSize * visualScale;
        var arrowRefX = arrowSize * 0.8f;
        var arrowRefY = arrowSize * 0.5f;
        sb.AppendLine($"  <marker id=\"arrow\" markerWidth=\"{F(arrowSize)}\" markerHeight=\"{F(arrowSize)}\" refX=\"{F(arrowRefX)}\" refY=\"{F(arrowRefY)}\" orient=\"auto\" markerUnits=\"userSpaceOnUse\">");
        sb.AppendLine($"    <path d=\"M0,0 L{F(arrowSize)},{F(arrowRefY)} L0,{F(arrowSize)} L{F(arrowSize * 0.2f)},{F(arrowRefY)} Z\" fill=\"{_options.EdgeColor}\"/>");
        sb.AppendLine("  </marker>");
        sb.AppendLine($"  <marker id=\"arrow-selected\" markerWidth=\"{F(arrowSize)}\" markerHeight=\"{F(arrowSize)}\" refX=\"{F(arrowRefX)}\" refY=\"{F(arrowRefY)}\" orient=\"auto\" markerUnits=\"userSpaceOnUse\">");
        sb.AppendLine($"    <path d=\"M0,0 L{F(arrowSize)},{F(arrowRefY)} L0,{F(arrowSize)} L{F(arrowSize * 0.2f)},{F(arrowRefY)} Z\" fill=\"{_options.SelectedEdgeColor}\"/>");
        sb.AppendLine("  </marker>");
        sb.AppendLine($"  <marker id=\"arrow-highlighted\" markerWidth=\"{F(arrowSize)}\" markerHeight=\"{F(arrowSize)}\" refX=\"{F(arrowRefX)}\" refY=\"{F(arrowRefY)}\" orient=\"auto\" markerUnits=\"userSpaceOnUse\">");
        sb.AppendLine($"    <path d=\"M0,0 L{F(arrowSize)},{F(arrowRefY)} L0,{F(arrowSize)} L{F(arrowSize * 0.2f)},{F(arrowRefY)} Z\" fill=\"{_options.HighlightedEdgeColor}\"/>");
        sb.AppendLine("  </marker>");

        #endregion </marker>

        sb.AppendLine("</defs>");

        #endregion </defs>

        #region <g> 变换组 用于实现 偏移/缩放

        sb.AppendLine($"<g transform=\"translate({F(offsetX)},{F(offsetY)}) scale({F(scale)})\">");

        #region 网格背景

        if (_options.ShowGrid)
        {
            sb.AppendLine($"  <rect x=\"0\" y=\"0\" width=\"{F(mapWidth)}\" height=\"{F(mapHeight)}\" fill=\"url(#grid)\"/>");
        }

        #endregion 网格背景

        #region 地图边框

        var borderWidth = _options.MapBorderWidth * visualScale;
        sb.AppendLine($"  <rect x=\"0\" y=\"0\" width=\"{F(mapWidth)}\" height=\"{F(mapHeight)}\" fill=\"none\" stroke=\"{_options.MapBorderColor}\" stroke-width=\"{F(borderWidth)}\"/>");

        #endregion 地图边框

        // 创建节点字典用于边的绘制
        var nodeDict = data.Nodes.ToDictionary(n => n.Id, n => n);

        #region 绘制边（先绘制，这样节点在上面）

        foreach (var edge in data.Edges)
        {
            var isHighlighted = data.HighlightedEdgeIds.Contains(edge.Id);
            var isSelected = data.SelectedEdgeId == edge.Id;
            var sequenceIndex = data.HighlightedEdgeIds.IndexOf(edge.Id);

            if (nodeDict.TryGetValue(edge.StartNodeId, out var startNode) &&
                nodeDict.TryGetValue(edge.EndNodeId, out var endNode))
            {
                sb.Append(RenderEdge(edge, startNode, endNode, isSelected, isHighlighted, sequenceIndex, visualScale));
            }
        }

        #endregion 绘制边

        #region 绘制节点

        foreach (var node in data.Nodes)
        {
            var isSelected = data.SelectedNodeId == node.Id;
            sb.Append(RenderNode(node, isSelected, visualScale));
        }

        #endregion 绘制节点

        #region 绘制站点（在节点上面显示）

        foreach (var station in data.Stations)
        {
            var isSelected = data.SelectedStationId == station.Id;
            sb.Append(RenderStation(station, isSelected, visualScale));
        }

        #endregion 绘制站点

        sb.AppendLine("</g>");

        #endregion </g>

        sb.AppendLine("</svg>");

        #endregion </svg>

        return sb.ToString();
    }

    #endregion

    #region 渲染边

    /// <summary>
    /// 渲染边
    /// </summary>
    private string RenderEdge(
        MapEdgeListItemDto edge,
        MapNodeListItemDto startNode,
        MapNodeListItemDto endNode,
        bool isSelected,
        bool isHighlighted,
        int sequenceIndex,
        float visualScale)
    {
        var sb = new StringBuilder();
        var x1 = (float)startNode.X;
        var y1 = (float)startNode.Y;
        var x2 = (float)endNode.X;
        var y2 = (float)endNode.Y;

        #region 确定颜色和线宽

        string color;
        float width;
        string markerId;

        if (isSelected)
        {
            color = _options.SelectedEdgeColor;
            width = _options.HighlightedEdgeWidth * visualScale;
            markerId = "arrow-selected";
        }
        else if (isHighlighted)
        {
            color = _options.HighlightedEdgeColor;
            width = _options.HighlightedEdgeWidth * visualScale;
            markerId = "arrow-highlighted";
        }
        else
        {
            color = _options.EdgeColor;
            width = _options.EdgeWidth * visualScale;
            markerId = "arrow";
        }

        #endregion

        #region 计算缩短后的端点（避免箭头穿过节点，影响美观）

        var nodeRadius = _options.NodeRadius * visualScale;
        var angle = Math.Atan2(y2 - y1, x2 - x1);
        var shortenedX1 = x1 + (float)Math.Cos(angle) * nodeRadius;
        var shortenedY1 = y1 + (float)Math.Sin(angle) * nodeRadius;
        var shortenedX2 = x2 - (float)Math.Cos(angle) * nodeRadius;
        var shortenedY2 = y2 - (float)Math.Sin(angle) * nodeRadius;

        var markerAttr = edge.IsBidirectional ? "" : $"marker-end=\"url(#{markerId})\"";

        if (edge.EdgeType == EdgeType.Arc && edge.ArcViaX.HasValue && edge.ArcViaY.HasValue)
        {
            // 弧线：通过中间点绘制折线
            var viaX = (float)edge.ArcViaX.Value;
            var viaY = (float)edge.ArcViaY.Value;

            sb.AppendLine($"  <polyline points=\"{F(shortenedX1)},{F(shortenedY1)} {F(viaX)},{F(viaY)} {F(shortenedX2)},{F(shortenedY2)}\" " +
                $"fill=\"none\" stroke=\"{color}\" stroke-width=\"{F(width)}\" stroke-linecap=\"round\" stroke-linejoin=\"round\" " +
                $"{markerAttr} data-type=\"edge\" data-id=\"{edge.Id}\" class=\"map-edge\"/>");
        }
        else
        {
            // 直线
            sb.AppendLine($"  <line x1=\"{F(shortenedX1)}\" y1=\"{F(shortenedY1)}\" x2=\"{F(shortenedX2)}\" y2=\"{F(shortenedY2)}\" " +
                $"stroke=\"{color}\" stroke-width=\"{F(width)}\" stroke-linecap=\"round\" " +
                $"{markerAttr} data-type=\"edge\" data-id=\"{edge.Id}\" class=\"map-edge\"/>");
        }

        #endregion

        #region 绘制路线序号

        if (isHighlighted && sequenceIndex >= 0 && _options.ShowRouteSequence)
        {
            var midX = (x1 + x2) / 2;
            var midY = (y1 + y2) / 2;
            var seqRadius = _options.RouteSequenceRadius * visualScale;
            var seqFontSize = _options.RouteSequenceFontSize * visualScale;

            // 序号背景圆
            sb.AppendLine($"  <circle cx=\"{F(midX)}\" cy=\"{F(midY)}\" r=\"{F(seqRadius)}\" fill=\"{_options.HighlightedEdgeColor}\"/>");
            // 序号文字
            sb.AppendLine($"  <text x=\"{F(midX)}\" y=\"{F(midY + seqFontSize * 0.35f)}\" text-anchor=\"middle\" fill=\"white\" font-size=\"{F(seqFontSize)}\" font-family=\"sans-serif\">{sequenceIndex + 1}</text>");
        }

        #endregion

        return sb.ToString();
    }

    #endregion

    #region 渲染节点

    /// <summary>
    /// 渲染节点
    /// </summary>
    private string RenderNode(MapNodeListItemDto node, bool isSelected, float visualScale)
    {
        var sb = new StringBuilder();
        var x = (float)node.X;
        var y = (float)node.Y;
        var radius = _options.NodeRadius * visualScale;
        var borderWidth = _options.NodeBorderWidth * visualScale;
        var fontSize = _options.LabelFontSize * visualScale;
        var fillColor = isSelected ? _options.SelectedNodeColor : _options.NodeColor;

        // 节点圆形
        sb.AppendLine($"  <circle cx=\"{F(x)}\" cy=\"{F(y)}\" r=\"{F(radius)}\" " +
            $"fill=\"{fillColor}\" stroke=\"{_options.NodeBorderColor}\" stroke-width=\"{F(borderWidth)}\" " +
            $"data-type=\"node\" data-id=\"{node.Id}\" class=\"map-node\"/>");

        // 节点标签
        if (_options.ShowNodeLabels)
        {
            var labelY = y - radius - _options.LabelOffset * visualScale;
            sb.AppendLine($"  <text x=\"{F(x)}\" y=\"{F(labelY)}\" " +
                $"text-anchor=\"middle\" fill=\"{_options.LabelColor}\" " +
                $"font-size=\"{F(fontSize)}\" font-family=\"sans-serif\">{node.NodeCode}</text>");
        }

        return sb.ToString();
    }

    #endregion

    #region 渲染站点

    /// <summary>
    /// 渲染站点
    /// </summary>
    private string RenderStation(StationListItemDto station, bool isSelected, float visualScale)
    {
        var sb = new StringBuilder();
        var x = (float)station.X;
        var y = (float)station.Y;
        var size = _options.StationSize * visualScale;
        var fontSize = _options.LabelFontSize * visualScale;
        var labelOffset = _options.LabelOffset * visualScale;

        // 根据站点类型获取颜色和图标
        var (color, borderColor) = GetStationColors(station.StationType, isSelected);

        // 绘制站点图标
        sb.Append(RenderStationIcon(station.StationType, x, y, size, color, borderColor, visualScale));

        // 站点名称标签（显示在图标下方）
        var labelY = y + size + labelOffset;
        sb.AppendLine($"  <text x=\"{F(x)}\" y=\"{F(labelY)}\" " +
            $"text-anchor=\"middle\" fill=\"{color}\" " +
            $"font-size=\"{F(fontSize)}\" font-family=\"sans-serif\" font-weight=\"bold\">{station.DisplayName}</text>");

        // 站点编号（显示在名称下方）
        var codeY = labelY + fontSize + labelOffset * 0.3f;
        sb.AppendLine($"  <text x=\"{F(x)}\" y=\"{F(codeY)}\" " +
            $"text-anchor=\"middle\" fill=\"#666666\" " +
            $"font-size=\"{F(fontSize * 0.85f)}\" font-family=\"sans-serif\">{station.StationCode}</text>");

        // 选中时绘制高亮圈
        if (isSelected)
        {
            var highlightRadius = size * 1.5f;
            sb.AppendLine($"  <circle cx=\"{F(x)}\" cy=\"{F(y)}\" r=\"{F(highlightRadius)}\" " +
                $"fill=\"none\" stroke=\"{color}\" stroke-width=\"{F(_options.StationHighlightWidth * visualScale)}\" " +
                $"stroke-dasharray=\"3,3\" opacity=\"0.7\"/>");
        }

        return sb.ToString();
    }

    /// <summary>
    /// 根据站点类型获取颜色
    /// </summary>
    private static (string fillColor, string borderColor) GetStationColors(StationType type, bool isSelected)
    {
        var baseColors = type switch
        {
            StationType.Pickup => ("#4CAF50", "#2E7D32"),      // 绿色 - 取货点
            StationType.Dropoff => ("#FF9800", "#F57C00"),     // 橙色 - 卸货点
            StationType.Charge => ("#2196F3", "#1565C0"),      // 蓝色 - 充电站
            StationType.Standby => ("#9E9E9E", "#616161"),     // 灰色 - 待命点
            StationType.Intersection => ("#9C27B0", "#7B1FA2"), // 紫色 - 交叉口
            _ => ("#607D8B", "#455A64")                         // 默认蓝灰色
        };

        if (isSelected)
        {
            return ("#E91E63", "#C2185B"); // 选中时显示粉色
        }

        return baseColors;
    }

    /// <summary>
    /// 根据站点类型渲染不同的图标
    /// </summary>
    private string RenderStationIcon(StationType type, float x, float y, float size, string fillColor, string borderColor, float visualScale)
    {
        var sb = new StringBuilder();
        var strokeWidth = _options.StationBorderWidth * visualScale;

        switch (type)
        {
            case StationType.Pickup:
                // 取货点: 向上箭头 + 方框
                var boxSize = size * 0.8f;
                var arrowHeight = size * 0.6f;
                sb.AppendLine($"  <rect x=\"{F(x - boxSize / 2)}\" y=\"{F(y - boxSize / 2)}\" width=\"{F(boxSize)}\" height=\"{F(boxSize)}\" " +
                    $"rx=\"{F(size * 0.15f)}\" fill=\"{fillColor}\" stroke=\"{borderColor}\" stroke-width=\"{F(strokeWidth)}\"/>");
                // 向上箭头
                sb.AppendLine($"  <path d=\"M {F(x)} {F(y - arrowHeight / 3)} L {F(x - arrowHeight / 3)} {F(y + arrowHeight / 6)} L {F(x + arrowHeight / 3)} {F(y + arrowHeight / 6)} Z\" " +
                    $"fill=\"white\"/>");
                break;

            case StationType.Dropoff:
                // 卸货点: 向下箭头 + 方框
                var dropBoxSize = size * 0.8f;
                var dropArrowHeight = size * 0.6f;
                sb.AppendLine($"  <rect x=\"{F(x - dropBoxSize / 2)}\" y=\"{F(y - dropBoxSize / 2)}\" width=\"{F(dropBoxSize)}\" height=\"{F(dropBoxSize)}\" " +
                    $"rx=\"{F(size * 0.15f)}\" fill=\"{fillColor}\" stroke=\"{borderColor}\" stroke-width=\"{F(strokeWidth)}\"/>");
                // 向下箭头
                sb.AppendLine($"  <path d=\"M {F(x)} {F(y + dropArrowHeight / 3)} L {F(x - dropArrowHeight / 3)} {F(y - dropArrowHeight / 6)} L {F(x + dropArrowHeight / 3)} {F(y - dropArrowHeight / 6)} Z\" " +
                    $"fill=\"white\"/>");
                break;

            case StationType.Charge:
                // 充电站: 闪电图标 + 圆形
                var chargeRadius = size * 0.5f;
                sb.AppendLine($"  <circle cx=\"{F(x)}\" cy=\"{F(y)}\" r=\"{F(chargeRadius)}\" " +
                    $"fill=\"{fillColor}\" stroke=\"{borderColor}\" stroke-width=\"{F(strokeWidth)}\"/>");
                // 闪电
                var boltW = size * 0.3f;
                var boltH = size * 0.5f;
                sb.AppendLine($"  <path d=\"M {F(x + boltW * 0.2f)} {F(y - boltH * 0.5f)} L {F(x - boltW * 0.3f)} {F(y + boltH * 0.1f)} " +
                    $"L {F(x)} {F(y + boltH * 0.1f)} L {F(x - boltW * 0.2f)} {F(y + boltH * 0.5f)} " +
                    $"L {F(x + boltW * 0.3f)} {F(y - boltH * 0.1f)} L {F(x)} {F(y - boltH * 0.1f)} Z\" " +
                    $"fill=\"white\"/>");
                break;

            case StationType.Standby:
                // 待命点: P字母 + 圆形
                var standbyRadius = size * 0.5f;
                sb.AppendLine($"  <circle cx=\"{F(x)}\" cy=\"{F(y)}\" r=\"{F(standbyRadius)}\" " +
                    $"fill=\"{fillColor}\" stroke=\"{borderColor}\" stroke-width=\"{F(strokeWidth)}\"/>");
                // P 字母
                var pFontSize = size * 0.6f;
                sb.AppendLine($"  <text x=\"{F(x)}\" y=\"{F(y + pFontSize * 0.35f)}\" " +
                    $"text-anchor=\"middle\" fill=\"white\" " +
                    $"font-size=\"{F(pFontSize)}\" font-family=\"sans-serif\" font-weight=\"bold\">P</text>");
                break;

            case StationType.Intersection:
                // 交叉口: 菱形 + X标记
                var diamondSize = size * 0.6f;
                sb.AppendLine($"  <path d=\"M {F(x)} {F(y - diamondSize)} L {F(x + diamondSize)} {F(y)} " +
                    $"L {F(x)} {F(y + diamondSize)} L {F(x - diamondSize)} {F(y)} Z\" " +
                    $"fill=\"{fillColor}\" stroke=\"{borderColor}\" stroke-width=\"{F(strokeWidth)}\"/>");
                // X 标记
                var xSize = diamondSize * 0.4f;
                sb.AppendLine($"  <line x1=\"{F(x - xSize)}\" y1=\"{F(y - xSize)}\" x2=\"{F(x + xSize)}\" y2=\"{F(y + xSize)}\" " +
                    $"stroke=\"white\" stroke-width=\"{F(strokeWidth)}\" stroke-linecap=\"round\"/>");
                sb.AppendLine($"  <line x1=\"{F(x + xSize)}\" y1=\"{F(y - xSize)}\" x2=\"{F(x - xSize)}\" y2=\"{F(y + xSize)}\" " +
                    $"stroke=\"white\" stroke-width=\"{F(strokeWidth)}\" stroke-linecap=\"round\"/>");
                break;

            default:
                // 默认: 简单圆形
                var defaultRadius = size * 0.5f;
                sb.AppendLine($"  <circle cx=\"{F(x)}\" cy=\"{F(y)}\" r=\"{F(defaultRadius)}\" " +
                    $"fill=\"{fillColor}\" stroke=\"{borderColor}\" stroke-width=\"{F(strokeWidth)}\"/>");
                break;
        }

        return sb.ToString();
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
