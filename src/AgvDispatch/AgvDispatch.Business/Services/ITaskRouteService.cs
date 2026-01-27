using AgvDispatch.Business.Entities.TaskRouteAggregate;

namespace AgvDispatch.Business.Services;

/// <summary>
/// 任务路径规划服务接口
/// </summary>
public interface ITaskRouteService
{
    /// <summary>
    /// 创建任务路径（包含 TaskRoute、TaskRouteSegment、TaskRouteCheckpoint）
    /// </summary>
    /// <param name="taskId">任务ID</param>
    /// <param name="startStationCode">起始站点编号</param>
    /// <param name="endStationCode">终点站点编号</param>
    /// <param name="userId">用户ID（用于审计）</param>
    /// <returns>创建的 TaskRoute 实体，如果创建失败则为 null</returns>
    Task<TaskRoute?> CreateTaskRouteAsync(
        Guid taskId,
        string startStationCode,
        string endStationCode,
        Guid? userId);
}
