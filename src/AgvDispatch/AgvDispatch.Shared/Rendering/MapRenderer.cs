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

        if (isHighlighted)
        {
            color = _options.HighlightedEdgeColor;
            width = _options.HighlightedEdgeWidth * visualScale;
        }
        else
        {
            color = _options.EdgeColor;
            width = _options.EdgeWidth * visualScale;
        }

        #endregion

        #region 绘制边线

        var angle = Math.Atan2(y2 - y1, x2 - x1);

        // 选中时先绘制黄色底边作为边框效果
        if (isSelected)
        {
            var highlightWidth = width + _options.SelectedEdgeHighlightWidth * visualScale * 2;

            if (edge.EdgeType == EdgeType.Arc && edge.ArcViaX.HasValue && edge.ArcViaY.HasValue)
            {
                var viaX = (float)edge.ArcViaX.Value;
                var viaY = (float)edge.ArcViaY.Value;
                sb.AppendLine($"  <polyline points=\"{F(x1)},{F(y1)} {F(viaX)},{F(viaY)} {F(x2)},{F(y2)}\" " +
                    $"fill=\"none\" stroke=\"{_options.SelectedEdgeHighlightColor}\" stroke-width=\"{F(highlightWidth)}\" stroke-linecap=\"round\" stroke-linejoin=\"round\"/>");
            }
            else if (edge.EdgeType == EdgeType.ArcWithCurvature && edge.Curvature.HasValue)
            {
                var (ctrlX, ctrlY) = CalculateCurvatureControlPoint(x1, y1, x2, y2, (float)edge.Curvature.Value);
                sb.AppendLine($"  <path d=\"M {F(x1)},{F(y1)} Q {F(ctrlX)},{F(ctrlY)} {F(x2)},{F(y2)}\" " +
                    $"fill=\"none\" stroke=\"{_options.SelectedEdgeHighlightColor}\" stroke-width=\"{F(highlightWidth)}\" stroke-linecap=\"round\"/>");
            }
            else
            {
                sb.AppendLine($"  <line x1=\"{F(x1)}\" y1=\"{F(y1)}\" x2=\"{F(x2)}\" y2=\"{F(y2)}\" " +
                    $"stroke=\"{_options.SelectedEdgeHighlightColor}\" stroke-width=\"{F(highlightWidth)}\" stroke-linecap=\"round\"/>");
            }
        }

        // 绘制边线（不带箭头标记）
        if (edge.EdgeType == EdgeType.Arc && edge.ArcViaX.HasValue && edge.ArcViaY.HasValue)
        {
            // 弧线（经过点）：通过中间点绘制折线
            var viaX = (float)edge.ArcViaX.Value;
            var viaY = (float)edge.ArcViaY.Value;

            sb.AppendLine($"  <polyline points=\"{F(x1)},{F(y1)} {F(viaX)},{F(viaY)} {F(x2)},{F(y2)}\" " +
                $"fill=\"none\" stroke=\"{color}\" stroke-width=\"{F(width)}\" stroke-linecap=\"round\" stroke-linejoin=\"round\" " +
                $"data-type=\"edge\" data-id=\"{edge.Id}\" class=\"map-edge\"/>");
        }
        else if (edge.EdgeType == EdgeType.ArcWithCurvature && edge.Curvature.HasValue)
        {
            // 弧线（曲率）：使用二次贝塞尔曲线
            var (ctrlX, ctrlY) = CalculateCurvatureControlPoint(x1, y1, x2, y2, (float)edge.Curvature.Value);

            sb.AppendLine($"  <path d=\"M {F(x1)},{F(y1)} Q {F(ctrlX)},{F(ctrlY)} {F(x2)},{F(y2)}\" " +
                $"fill=\"none\" stroke=\"{color}\" stroke-width=\"{F(width)}\" stroke-linecap=\"round\" " +
                $"data-type=\"edge\" data-id=\"{edge.Id}\" class=\"map-edge\"/>");
        }
        else
        {
            // 直线
            sb.AppendLine($"  <line x1=\"{F(x1)}\" y1=\"{F(y1)}\" x2=\"{F(x2)}\" y2=\"{F(y2)}\" " +
                $"stroke=\"{color}\" stroke-width=\"{F(width)}\" stroke-linecap=\"round\" " +
                $"data-type=\"edge\" data-id=\"{edge.Id}\" class=\"map-edge\"/>");
        }

        #endregion

        #region 绘制方向箭头（在边的1/3位置）

        if (_options.ShowArrows)
        {
            var arrowSize = _options.ArrowSize * visualScale;

            if (edge.IsBidirectional)
            {
                // 双向边：在1/3和2/3位置各画一个箭头
                // 前1/3位置：箭头指向起点（反向）
                var arrow1X = x1 + (x2 - x1) * 0.33f;
                var arrow1Y = y1 + (y2 - y1) * 0.33f;
                var angle1 = angle + (float)Math.PI; // 反向
                sb.Append(RenderArrowAtPosition(arrow1X, arrow1Y, angle1, arrowSize, color));

                // 后1/3位置：箭头指向终点（正向）
                var arrow2X = x1 + (x2 - x1) * 0.67f;
                var arrow2Y = y1 + (y2 - y1) * 0.67f;
                sb.Append(RenderArrowAtPosition(arrow2X, arrow2Y, angle, arrowSize, color));
            }
            else
            {
                // 单向边：在1/3位置画一个指向终点的箭头
                var arrowX = x1 + (x2 - x1) * 0.33f;
                var arrowY = y1 + (y2 - y1) * 0.33f;
                sb.Append(RenderArrowAtPosition(arrowX, arrowY, angle, arrowSize, color));
            }
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

        #region 绘制边标签

        if (_options.ShowEdgeLabels)
        {
            var midX = (x1 + x2) / 2;
            var midY = (y1 + y2) / 2;
            var fontSize = _options.LabelFontSize * visualScale;
            var labelOffset = _options.LabelOffset * visualScale;

            // 复用之前计算的 angle，用于调整标签位置
            var isVerticalish = Math.Abs(Math.Cos(angle)) < 0.5; // 接近垂直的边

            // 标签位置偏移（避免与边重叠）
            var labelX = midX;
            var labelY = midY - labelOffset;

            // 如果是垂直的边，标签放在右侧
            if (isVerticalish)
            {
                labelX = midX + labelOffset;
                labelY = midY;
            }

            // 边标签文字
            sb.AppendLine($"  <text x=\"{F(labelX)}\" y=\"{F(labelY + fontSize * 0.35f)}\" " +
                $"text-anchor=\"middle\" fill=\"{_options.LabelColor}\" " +
                $"font-size=\"{F(fontSize)}\" font-family=\"sans-serif\">{edge.EdgeCode}</text>");
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

        // 选中时先绘制黄色高亮圈
        if (isSelected)
        {
            var highlightRadius = radius + _options.SelectedNodeHighlightWidth * visualScale;
            sb.AppendLine($"  <circle cx=\"{F(x)}\" cy=\"{F(y)}\" r=\"{F(highlightRadius)}\" " +
                $"fill=\"none\" stroke=\"{_options.SelectedNodeHighlightColor}\" stroke-width=\"{F(_options.SelectedNodeHighlightWidth * visualScale)}\"/>");
        }

        // 节点圆形（保持原色）
        sb.AppendLine($"  <circle cx=\"{F(x)}\" cy=\"{F(y)}\" r=\"{F(radius)}\" " +
            $"fill=\"{_options.NodeColor}\" stroke=\"{_options.NodeBorderColor}\" stroke-width=\"{F(borderWidth)}\" " +
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

        // 根据站点类型获取颜色和图标（不因选中而改变）
        var (color, borderColor) = GetStationColors(station.StationType, false);

        // 选中时先绘制黄色边框
        if (isSelected)
        {
            var highlightRadius = size * 0.8f; // 适度的边框大小
            sb.AppendLine($"  <circle cx=\"{F(x)}\" cy=\"{F(y)}\" r=\"{F(highlightRadius)}\" " +
                $"fill=\"none\" stroke=\"{_options.SelectedStationHighlightColor}\" stroke-width=\"{F(_options.SelectedStationHighlightWidth * visualScale)}\"/>");
        }

        // 绘制站点图标
        sb.Append(RenderStationIcon(station.StationType, x, y, size, color, borderColor, visualScale));

        // 站点标签（受配置控制）
        if (_options.ShowStationLabels)
        {
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

    #region 绘制箭头

    /// <summary>
    /// 在指定位置和角度绘制箭头
    /// </summary>
    /// <param name="x">箭头中心X坐标</param>
    /// <param name="y">箭头中心Y坐标</param>
    /// <param name="angle">箭头角度（弧度）</param>
    /// <param name="size">箭头大小</param>
    /// <param name="color">箭头颜色</param>
    /// <returns>SVG 路径字符串</returns>
    private string RenderArrowAtPosition(float x, float y, double angle, float size, string color)
    {
        // 箭头形状：三角形
        // 箭头指向右侧（0度方向），然后旋转到指定角度
        var halfSize = size * 0.5f;
        var tipLength = size * 0.8f;

        // 定义箭头的三个顶点（未旋转时）
        // 顶点：箭头尖端在右侧
        var p1X = tipLength;
        var p1Y = 0f;
        // 左下角
        var p2X = 0f;
        var p2Y = -halfSize;
        // 左上角
        var p3X = 0f;
        var p3Y = halfSize;
        // 尾部中点（使箭头更立体）
        var p4X = tipLength * 0.2f;
        var p4Y = 0f;

        // 旋转并平移顶点
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
        var r4X = RotateX(p4X, p4Y);
        var r4Y = RotateY(p4X, p4Y);

        // 绘制填充的箭头三角形
        return $"  <path d=\"M{F(r2X)},{F(r2Y)} L{F(r1X)},{F(r1Y)} L{F(r3X)},{F(r3Y)} L{F(r4X)},{F(r4Y)} Z\" fill=\"{color}\"/>\n";
    }

    #endregion

    #region Helper

    /// <summary>
    /// 根据曲率值计算贝塞尔曲线的控制点
    /// </summary>
    /// <param name="x1">起点X</param>
    /// <param name="y1">起点Y</param>
    /// <param name="x2">终点X</param>
    /// <param name="y2">终点Y</param>
    /// <param name="curvature">曲率值（-1到1之间，0为直线，正值向右弯，负值向左弯）</param>
    /// <returns>控制点坐标</returns>
    private static (float ctrlX, float ctrlY) CalculateCurvatureControlPoint(float x1, float y1, float x2, float y2, float curvature)
    {
        // 计算中点
        var midX = (x1 + x2) / 2;
        var midY = (y1 + y2) / 2;

        // 计算线段长度
        var dx = x2 - x1;
        var dy = y2 - y1;
        var distance = (float)Math.Sqrt(dx * dx + dy * dy);

        // 计算垂直于线段的方向（顺时针旋转90度）
        // 原向量 (dx, dy)，顺时针旋转90度得到 (dy, -dx)
        var perpX = dy;
        var perpY = -dx;

        // 归一化垂直向量
        var perpLength = (float)Math.Sqrt(perpX * perpX + perpY * perpY);
        if (perpLength > 0)
        {
            perpX /= perpLength;
            perpY /= perpLength;
        }

        // 根据曲率值计算偏移距离（使用线段长度的一定比例）
        // 曲率值越大，弯曲越明显
        var offset = curvature * distance * 0.5f;

        // 计算控制点
        var ctrlX = midX + perpX * offset;
        var ctrlY = midY + perpY * offset;

        return (ctrlX, ctrlY);
    }

    /// <summary>
    /// 格式化浮点数（避免本地化问题）
    /// </summary>
    private static string F(float value)
    {
        return value.ToString("F2", CultureInfo.InvariantCulture);
    }

    #endregion

}
