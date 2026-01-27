using AgvDispatch.Business.Entities.TaskRouteAggregate;
using Ardalis.Specification;

namespace AgvDispatch.Business.Specifications.TaskRoutes;

/// <summary>
/// 根据路线ID获取路线检查点列表
/// </summary>
public class TaskRouteCheckpointByRouteIdSpec : Specification<TaskRouteCheckpoint>
{
    public TaskRouteCheckpointByRouteIdSpec(Guid routeId)
    {
        Query.Where(x => x.TaskRouteId == routeId && x.IsValid)
             .OrderBy(x => x.Seq);
    }
}
