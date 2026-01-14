namespace AgvDispatch.Shared.Simulation;

/// <summary>
/// 模拟状态
/// </summary>
public enum SimulationState
{
    /// <summary>
    /// 未开始
    /// </summary>
    NotStarted,

    /// <summary>
    /// 运行中
    /// </summary>
    Running,

    /// <summary>
    /// 已暂停
    /// </summary>
    Paused,

    /// <summary>
    /// 已完成
    /// </summary>
    Completed,

    /// <summary>
    /// 失败
    /// </summary>
    Failed
}
