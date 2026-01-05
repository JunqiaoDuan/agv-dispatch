namespace AgvDispatch.Shared.Enums;

/// <summary>
/// 站点类型
/// </summary>
public enum StationType
{
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
    Standby = 40
}
