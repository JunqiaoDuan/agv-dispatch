namespace AgvDispatch.Shared.DTOs.PathLocks;

/// <summary>
/// 通道详细信息DTO
/// </summary>
public class ChannelDetailDto
{
    /// <summary>
    /// 通道名称
    /// </summary>
    public string ChannelName { get; set; } = string.Empty;

    /// <summary>
    /// 该通道下的所有路径锁定记录
    /// </summary>
    public List<PathLockDetailDto> PathLocks { get; set; } = [];

    /// <summary>
    /// 锁定该通道的 AGV 数量
    /// </summary>
    public int AgvCount { get; set; }
}
