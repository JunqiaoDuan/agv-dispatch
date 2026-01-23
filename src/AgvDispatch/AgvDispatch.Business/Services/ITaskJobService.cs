using AgvDispatch.Business.Entities.AgvAggregate;
using AgvDispatch.Business.Entities.TaskAggregate;
using AgvDispatch.Business.Messages;
using AgvDispatch.Shared.Enums;

namespace AgvDispatch.Business.Services;

/// <summary>
/// 任务管理服务接口
/// </summary>
public interface ITaskJobService
{
    #region AGV推荐/待处理

    /// <summary>
    /// 获取AGV推荐列表
    /// </summary>
    Task<List<AgvRecommendation>> GetAgvRecommendationsAsync(TaskJobType taskType);

    /// <summary>
    /// 获取等待下料的小车列表
    /// </summary>
    Task<List<AgvPendingItem>> GetPendingUnloadingAgvsAsync();

    /// <summary>
    /// 获取等待返回的小车列表
    /// </summary>
    Task<List<AgvPendingItem>> GetPendingReturnAgvsAsync();

    /// <summary>
    /// 获取可充电的小车列表
    /// </summary>
    Task<List<AgvPendingItem>> GetChargeableAgvsAsync();

    #endregion

    #region 任务CRUD

    /// <summary>
    /// 创建任务并分配AGV
    /// </summary>
    Task<TaskJob> CreateTaskWithAgvAsync(
        TaskJobType taskType,
        string targetStationCode,
        string selectedAgvCode,
        Guid? userId,
        bool? hasCargo = null);

    /// <summary>
    /// 获取任务详情
    /// </summary>
    Task<TaskJob?> GetTaskByIdAsync(Guid id);

    /// <summary>
    /// 取消任务
    /// </summary>
    /// <returns>成功标志和错误消息（成功时为 null）</returns>
    Task<(bool Success, string? Message)> CancelTaskAsync(Guid taskId, string? reason, Guid? userId);

    #endregion

}
