namespace AgvDispatch.Shared.Enums;

/// <summary>
/// 路线段到达后动作
/// </summary>
public enum FinalAction
{
    /// <summary>
    /// 无动作，直接通过
    /// </summary>
    None = 0,

    /// <summary>
    /// 停车
    /// </summary>
    Stop = 10,

    /// <summary>
    /// 装货
    /// </summary>
    Load = 20,

    /// <summary>
    /// 卸货
    /// </summary>
    Unload = 30,
}
