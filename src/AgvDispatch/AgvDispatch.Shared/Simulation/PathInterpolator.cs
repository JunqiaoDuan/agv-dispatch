using AgvDispatch.Shared.DTOs.MapEdges;
using AgvDispatch.Shared.DTOs.MapNodes;
using AgvDispatch.Shared.Enums;
using AgvDispatch.Shared.Rendering;

namespace AgvDispatch.Shared.Simulation;

/// <summary>
/// 路径插值计算器（支持直线、折线、贝塞尔曲线）
/// </summary>
public class PathInterpolator
{
    /// <summary>
    /// 在指定边上插值计算位置和角度
    /// </summary>
    /// <param name="edge">边信息</param>
    /// <param name="startNode">起始节点</param>
    /// <param name="endNode">结束节点</param>
    /// <param name="progress">进度（0-1）</param>
    /// <returns>(x, y, angle)</returns>
    public static (decimal x, decimal y, double angle) Interpolate(
        MapEdgeListItemDto edge,
        MapNodeListItemDto startNode,
        MapNodeListItemDto endNode,
        decimal progress)
    {
        // 将 progress 限制在 0 到 1 之间
        // 小于 0 返回 0，大于 1 返回 1，否则返回原值
        progress = Math.Clamp(progress, 0m, 1m);

        return edge.EdgeType switch
        {
            EdgeType.Line => InterpolateLine(startNode, endNode, progress),
            EdgeType.Arc => InterpolateArc(startNode, endNode, edge.ArcViaX, edge.ArcViaY, progress),
            EdgeType.ArcWithCurvature => InterpolateArcWithCurvature(startNode, endNode, edge.Curvature, progress),
            _ => InterpolateLine(startNode, endNode, progress)
        };
    }

    /// <summary>
    /// 直线插值
    /// </summary>
    private static (decimal x, decimal y, double angle) InterpolateLine(
        MapNodeListItemDto startNode,
        MapNodeListItemDto endNode,
        decimal t)
    {
        var x = startNode.X + (endNode.X - startNode.X) * t;
        var y = startNode.Y + (endNode.Y - startNode.Y) * t;
        var angle = Math.Atan2((double)(endNode.Y - startNode.Y), (double)(endNode.X - startNode.X));

        return (x, y, angle);
    }

    /// <summary>
    /// 弧线插值（通过经过点）
    /// </summary>
    private static (decimal x, decimal y, double angle) InterpolateArc(
        MapNodeListItemDto startNode,
        MapNodeListItemDto endNode,
        decimal? viaX,
        decimal? viaY,
        decimal t)
    {
        // 如果没有经过点，退化为直线
        if (viaX == null || viaY == null)
        {
            return InterpolateLine(startNode, endNode, t);
        }

        // 使用二次贝塞尔曲线插值（经过点作为控制点）
        return BezierCurveHelper.InterpolateOnCurve(
            startNode.X, startNode.Y,
            viaX.Value, viaY.Value,
            endNode.X, endNode.Y,
            t);
    }

    /// <summary>
    /// 弧线插值（通过曲率值，二次贝塞尔曲线）
    /// </summary>
    private static (decimal x, decimal y, double angle) InterpolateArcWithCurvature(
        MapNodeListItemDto startNode,
        MapNodeListItemDto endNode,
        decimal? curvature,
        decimal t)
    {
        // 如果没有曲率值，退化为直线
        if (curvature == null || curvature == 0m)
        {
            return InterpolateLine(startNode, endNode, t);
        }

        // 计算控制点
        var (controlX, controlY) = BezierCurveHelper.CalculateControlPoint(
            startNode.X, startNode.Y,
            endNode.X, endNode.Y,
            curvature.Value);

        // 使用二次贝塞尔曲线插值
        return BezierCurveHelper.InterpolateOnCurve(
            startNode.X, startNode.Y,
            controlX, controlY,
            endNode.X, endNode.Y,
            t);
    }
}
