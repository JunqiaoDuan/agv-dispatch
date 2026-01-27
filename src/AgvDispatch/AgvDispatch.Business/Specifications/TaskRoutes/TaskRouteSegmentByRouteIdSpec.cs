using AgvDispatch.Business.Entities.TaskRouteAggregate;
using Ardalis.Specification;

namespace AgvDispatch.Business.Specifications.TaskRoutes;

/// <summary>
/// 根据路线ID获取路线段列表
/// </summary>
public class TaskRouteSegmentByRouteIdSpec : Specification<TaskRouteSegment>
{
    public TaskRouteSegmentByRouteIdSpec(Guid routeId)
    {
        Query.Where(x => x.TaskRouteId == routeId && x.IsValid)
             .OrderBy(x => x.Seq);
    }
}
