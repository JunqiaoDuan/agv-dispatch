using AgvDispatch.Business.Entities.RouteAggregate;
using Ardalis.Specification;

namespace AgvDispatch.Business.Specifications.TaskRoutes;

/// <summary>
/// 任务路线列表统计规格
/// </summary>
public class TaskRouteCountSpec : Specification<TaskRoute>
{
    public TaskRouteCountSpec(
        string? startStationCode = null,
        string? endStationCode = null)
    {
        // 基本过滤条件
        Query.Where(x => x.IsValid);

        // 按起点站点筛选
        if (!string.IsNullOrWhiteSpace(startStationCode))
        {
            Query.Where(x => x.StartStationCode == startStationCode);
        }

        // 按终点站点筛选
        if (!string.IsNullOrWhiteSpace(endStationCode))
        {
            Query.Where(x => x.EndStationCode == endStationCode);
        }
    }
}
