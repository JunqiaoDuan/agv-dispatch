namespace AgvDispatch.Shared.Enums;

/// <summary>
/// 边行驶方向
/// </summary>
public enum DriveDirection
{
    /// <summary>
    /// 正向（StartNode → EndNode）
    /// </summary>
    Forward = 1,

    /// <summary>
    /// 反向（EndNode → StartNode）
    /// </summary>
    Backward = 2,
}
