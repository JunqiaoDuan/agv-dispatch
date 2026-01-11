namespace AgvDispatch.Shared.Rendering;

/// <summary>
/// 坐标转换辅助类
/// </summary>
public static class CoordinateHelper
{
    /// <summary>
    /// 将屏幕坐标转换为地图坐标
    /// </summary>
    public static (float mapX, float mapY) ScreenToMap(
        float screenX, float screenY,
        float scale, float offsetX, float offsetY)
    {
        var mapX = (screenX - offsetX) / scale;
        var mapY = (screenY - offsetY) / scale;
        return (mapX, mapY);
    }

    /// <summary>
    /// 将地图坐标转换为屏幕坐标
    /// </summary>
    public static (float screenX, float screenY) MapToScreen(
        float mapX, float mapY,
        float scale, float offsetX, float offsetY)
    {
        var screenX = mapX * scale + offsetX;
        var screenY = mapY * scale + offsetY;
        return (screenX, screenY);
    }
}
