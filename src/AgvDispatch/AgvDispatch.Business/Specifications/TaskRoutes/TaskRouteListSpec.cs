using AgvDispatch.Business.Entities.RouteAggregate;
using Ardalis.Specification;

namespace AgvDispatch.Business.Specifications.TaskRoutes;

/// <summary>
/// 任务路线列表查询规格
/// </summary>
public class TaskRouteListSpec : Specification<TaskRoute>
{
    public TaskRouteListSpec(
        string? startStationCode = null,
        string? endStationCode = null,
        int? pageIndex = null,
        int? pageSize = null)
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

        // 排序
        Query.OrderBy(x => x.SortNo).ThenBy(x => x.StartStationCode);

        // 分页
        if (pageIndex.HasValue && pageSize.HasValue && pageSize.Value > 0)
        {
            Query.Skip(pageIndex.Value * pageSize.Value).Take(pageSize.Value);
        }
    }
}
