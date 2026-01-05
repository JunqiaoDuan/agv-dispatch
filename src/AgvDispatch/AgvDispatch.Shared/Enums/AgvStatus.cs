namespace AgvDispatch.Shared.Enums;

/// <summary>
/// 小车运行状态
/// </summary>
public enum AgvStatus
{
    /// <summary>
    /// 离线
    /// </summary>
    Offline = 0,

    /// <summary>
    /// 空闲
    /// </summary>
    Idle = 10,

    /// <summary>
    /// 执行任务中
    /// </summary>
    Running = 20,

    /// <summary>
    /// 充电中
    /// </summary>
    Charging = 30,

    /// <summary>
    /// 故障
    /// </summary>
    Error = 90,

}
