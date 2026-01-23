using AgvDispatch.Shared.Enums;

namespace AgvDispatch.Shared.DTOs.Tasks;

/// <summary>
/// 创建任务并分配AGV请求 DTO
/// </summary>
public class CreateTaskWithAgvRequestDto
{
    /// <summary>
    /// 任务类型
    /// </summary>
    public TaskJobType TaskType { get; set; }

    /// <summary>
    /// 目标站点编号
    /// </summary>
    public string TargetStationCode { get; set; } = string.Empty;

    /// <summary>
    /// 选中的小车编号
    /// </summary>
    public string SelectedAgvCode { get; set; } = string.Empty;

    /// <summary>
    /// 小车是否有货物（用于下料任务时更新小车货物状态）
    /// </summary>
    public bool? HasCargo { get; set; }
}
