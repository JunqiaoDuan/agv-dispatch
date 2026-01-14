namespace AgvDispatch.Shared.Rendering;

/// <summary>
/// 贝塞尔曲线计算辅助类（统一所有曲线相关计算）
/// </summary>
public static class BezierCurveHelper
{
    /// <summary>
    /// 根据曲率值计算二次贝塞尔曲线的控制点
    /// 这个方法是用于计算二次贝塞尔曲线的控制点，用于在两点之间生成平滑的曲线路径
    /// </summary>
    /// <param name="startX">起点X</param>
    /// <param name="startY">起点Y</param>
    /// <param name="endX">终点X</param>
    /// <param name="endY">终点Y</param>
    /// <param name="curvature">曲率值</param>
    /// <returns>控制点坐标</returns>
    public static (decimal controlX, decimal controlY) CalculateControlPoint(
        decimal startX,
        decimal startY,
        decimal endX,
        decimal endY,
        decimal curvature)
    {
        // 计算中点
        var midX = (startX + endX) / 2m;
        var midY = (startY + endY) / 2m;

        // 计算线段长度和方向
        var dx = endX - startX;
        var dy = endY - startY;
        var distance = (decimal)Math.Sqrt((double)(dx * dx + dy * dy));

        if (distance == 0)
        {
            return (midX, midY);
        }

        // 计算垂直于线段的方向（顺时针旋转90度）
        // 原向量 (dx, dy)，顺时针旋转90度得到 (dy, -dx)
        var perpX = dy / distance;
        var perpY = -dx / distance;

        // 根据曲率值计算偏移距离
        var offset = curvature * distance * 0.5m;

        // 计算控制点
        var controlX = midX + perpX * offset;
        var controlY = midY + perpY * offset;

        return (controlX, controlY);
    }

    /// <summary>
    /// 在二次贝塞尔曲线上插值计算点的位置和切线方向
    /// </summary>
    /// <param name="startX">起点X</param>
    /// <param name="startY">起点Y</param>
    /// <param name="controlX">控制点X</param>
    /// <param name="controlY">控制点Y</param>
    /// <param name="endX">终点X</param>
    /// <param name="endY">终点Y</param>
    /// <param name="t">参数t（0-1）</param>
    /// <returns>点的位置(x, y)和切线角度(angle)</returns>
    public static (decimal x, decimal y, double angle) InterpolateOnCurve(
        decimal startX,
        decimal startY,
        decimal controlX,
        decimal controlY,
        decimal endX,
        decimal endY,
        decimal t)
    {
        // 二次贝塞尔曲线公式
        // P(t) = (1-t)²P0 + 2(1-t)tP1 + t²P2
        var t2 = (double)t;
        var oneMinusT = 1.0 - t2;

        var x = (decimal)(oneMinusT * oneMinusT) * startX +
                2m * (decimal)(oneMinusT * t2) * controlX +
                (decimal)(t2 * t2) * endX;
        var y = (decimal)(oneMinusT * oneMinusT) * startY +
                2m * (decimal)(oneMinusT * t2) * controlY +
                (decimal)(t2 * t2) * endY;

        // 计算切线方向（导数）
        // P'(t) = 2(1-t)(P1-P0) + 2t(P2-P1)
        var derivX = 2.0 * oneMinusT * (double)(controlX - startX) +
                     2.0 * t2 * (double)(endX - controlX);
        var derivY = 2.0 * oneMinusT * (double)(controlY - startY) +
                     2.0 * t2 * (double)(endY - controlY);

        var angle = Math.Atan2(derivY, derivX);

        return (x, y, angle);
    }

    /// <summary>
    /// 生成二次贝塞尔曲线的SVG路径字符串（完整曲线）
    /// </summary>
    /// <param name="startX">起点X</param>
    /// <param name="startY">起点Y</param>
    /// <param name="controlX">控制点X</param>
    /// <param name="controlY">控制点Y</param>
    /// <param name="endX">终点X</param>
    /// <param name="endY">终点Y</param>
    /// <returns>SVG路径字符串（不包含path标签，只有d属性的值）</returns>
    public static string GenerateSvgPath(
        float startX,
        float startY,
        float controlX,
        float controlY,
        float endX,
        float endY)
    {
        return $"M{F(startX)},{F(startY)} Q{F(controlX)},{F(controlY)} {F(endX)},{F(endY)}";
    }

    /// <summary>
    /// 生成二次贝塞尔曲线的部分SVG路径字符串（从起点到进度t）
    /// </summary>
    /// <param name="startX">起点X</param>
    /// <param name="startY">起点Y</param>
    /// <param name="controlX">控制点X</param>
    /// <param name="controlY">控制点Y</param>
    /// <param name="endX">终点X（实际终点，用于计算曲线）</param>
    /// <param name="endY">终点Y（实际终点，用于计算曲线）</param>
    /// <param name="progress">进度（0-1）</param>
    /// <param name="segments">分段数量（采样点数量）</param>
    /// <returns>SVG路径字符串（不包含path标签，只有d属性的值）</returns>
    public static string GeneratePartialSvgPath(
        float startX,
        float startY,
        float controlX,
        float controlY,
        float endX,
        float endY,
        float progress,
        int segments = 20)
    {
        var pathData = new System.Text.StringBuilder();
        pathData.Append($"M{F(startX)},{F(startY)}");

        var actualSegments = Math.Max(5, (int)(progress * segments));

        for (int i = 1; i <= actualSegments; i++)
        {
            var t = progress * i / actualSegments;
            var oneMinusT = 1.0f - t;

            // 二次贝塞尔曲线公式
            var x = oneMinusT * oneMinusT * startX +
                    2f * oneMinusT * t * controlX +
                    t * t * endX;
            var y = oneMinusT * oneMinusT * startY +
                    2f * oneMinusT * t * controlY +
                    t * t * endY;

            pathData.Append($" L{F(x)},{F(y)}");
        }

        return pathData.ToString();
    }

    /// <summary>
    /// 格式化浮点数（避免本地化问题）
    /// </summary>
    private static string F(float value)
    {
        return value.ToString("F2", System.Globalization.CultureInfo.InvariantCulture);
    }
}
