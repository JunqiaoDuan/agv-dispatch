using AgvDispatch.Shared.Enums;

namespace AgvDispatch.Shared.DTOs.Tasks;

/// <summary>
/// 召唤小车请求 DTO
/// </summary>
public class SummonAgvRequestDto
{
    /// <summary>
    /// 任务类型
    /// </summary>
    public TaskJobType TaskType { get; set; }

    /// <summary>
    /// 目标站点编号
    /// </summary>
    public string TargetStationCode { get; set; } = string.Empty;
}
