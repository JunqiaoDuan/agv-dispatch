namespace AgvDispatch.Shared.Enums;

/// <summary>
/// 站点类型
/// </summary>
public enum StationType
{
    // ===== 业务站点 (10-89) =====

    /// <summary>
    /// 取货点
    /// </summary>
    Pickup = 10,

    /// <summary>
    /// 卸货点
    /// </summary>
    Dropoff = 20,

    /// <summary>
    /// 充电站
    /// </summary>
    Charge = 30,

    /// <summary>
    /// 待命点
    /// </summary>
    Standby = 40,

    // ===== 系统控制点 (90+) =====

    /// <summary>
    /// 交叉口防撞等待点
    /// </summary>
    Intersection = 90,
}

/// <summary>
/// StationType 扩展方法
/// </summary>
public static class StationTypeExtensions
{
    /// <summary>
    /// 获取站点类型显示文本
    /// </summary>
    public static string ToDisplayText(this StationType type) => type switch
    {
        StationType.Pickup => "取货点",
        StationType.Dropoff => "卸货点",
        StationType.Charge => "充电站",
        StationType.Standby => "待命点",
        StationType.Intersection => "交叉口等待点",
        _ => "未知"
    };
}
