namespace AgvDispatch.Shared.DTOs.PathLocks;

/// <summary>
/// 已放行通道信息
/// </summary>
public class ActiveChannelDto
{
    /// <summary>
    /// 通道名称
    /// </summary>
    public string ChannelName { get; set; } = string.Empty;

    /// <summary>
    /// 锁定该通道的 AGV 数量
    /// </summary>
    public int AgvCount { get; set; }
}
