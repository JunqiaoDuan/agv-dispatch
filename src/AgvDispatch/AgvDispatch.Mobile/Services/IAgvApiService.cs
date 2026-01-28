using AgvDispatch.Shared.DTOs;
using AgvDispatch.Shared.DTOs.Agvs;
using AgvDispatch.Shared.DTOs.PathLocks;
using AgvDispatch.Shared.DTOs.Stations;
using AgvDispatch.Shared.DTOs.Tasks;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AgvDispatch.Mobile.Services;

/// <summary>
/// AGV API 服务接口
/// </summary>
public interface IAgvApiService
{
    // AGV相关
    Task<List<AgvListItemDto>> GetAllAgvsAsync();
    Task<AgvDetailDto?> GetAgvByIdAsync(Guid id);
    Task<List<AgvMonitorItemDto>> GetAgvMonitorListAsync();
    Task<List<AgvExceptionSummaryDto>> GetAgvUnresolvedExceptionsAsync(string agvCode);
    Task<PagedResponse<AgvExceptionSummaryDto>> GetAllAgvExceptionsAsync(string agvCode, PagedAgvExceptionRequest request);
    Task<bool> ResolveExceptionsAsync(List<Guid> exceptionIds);
    Task<string?> ManualControlAsync(Guid id, ManualControlAgvRequest request);

    // 站点相关
    Task<List<StationListItemDto>> GetAllStationsAsync(Guid mapId);

    // 任务相关
    Task<List<TaskListItemDto>> GetAllTasksAsync();
    Task<List<TaskListItemDto>> GetActiveTasksAsync();
    Task<TaskDetailDto?> GetTaskDetailAsync(Guid taskId);
    Task<List<AgvRecommendationDto>> GetRecommendationsAsync(GetRecommendationsRequestDto request);
    Task<CreateTaskResponseDto?> CreateTaskAsync(CreateTaskWithAgvRequestDto request);
    Task<string?> CancelTaskAsync(CancelTaskRequestDto request);
    Task<List<AgvPendingItemDto>> GetPendingUnloadingAgvsAsync();
    Task<List<AgvPendingItemDto>> GetPendingReturnAgvsAsync();
    Task<List<AgvPendingItemDto>> GetChargeableAgvsAsync();

    // 路径锁定相关
    Task<List<ActiveChannelDto>> GetActiveChannelsAsync();
}
