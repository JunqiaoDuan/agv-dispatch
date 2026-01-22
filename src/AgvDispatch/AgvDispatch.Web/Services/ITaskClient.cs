using AgvDispatch.Shared.DTOs.Tasks;

namespace AgvDispatch.Web.Services;

/// <summary>
/// 任务 API 客户端接口
/// </summary>
public interface ITaskClient
{
    #region AGV推荐/待处理

    /// <summary>
    /// 获取AGV推荐列表
    /// </summary>
    Task<List<AgvRecommendationDto>> GetRecommendationsAsync(GetRecommendationsRequestDto request);

    /// <summary>
    /// 获取等待下料的小车列表
    /// </summary>
    Task<List<AgvPendingItemDto>> GetPendingUnloadingAgvsAsync();

    /// <summary>
    /// 获取等待返回的小车列表
    /// </summary>
    Task<List<AgvPendingItemDto>> GetPendingReturnAgvsAsync();

    /// <summary>
    /// 获取可充电的小车列表
    /// </summary>
    Task<List<AgvPendingItemDto>> GetChargeableAgvsAsync();

    #endregion

    #region 任务CRUD

    /// <summary>
    /// 创建任务并分配AGV
    /// </summary>
    Task<CreateTaskResponseDto?> CreateTaskAsync(CreateTaskWithAgvRequestDto request);

    /// <summary>
    /// 取消任务
    /// </summary>
    Task<string?> CancelTaskAsync(CancelTaskRequestDto request);

    /// <summary>
    /// 获取活动任务列表（待分配、已分配、执行中）
    /// </summary>
    Task<List<TaskListItemDto>> GetActiveTasksAsync();

    /// <summary>
    /// 获取任务详情
    /// </summary>
    Task<TaskDetailDto?> GetByIdAsync(Guid id);

    #endregion

}
