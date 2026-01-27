using AgvDispatch.Business.Entities.TaskRouteAggregate;
using Ardalis.Specification;

namespace AgvDispatch.Business.Specifications.TaskRoutes;

/// <summary>
/// 根据地图边ID获取路线段列表
/// </summary>
public class TaskRouteSegmentByEdgeIdSpec : Specification<TaskRouteSegment>
{
    public TaskRouteSegmentByEdgeIdSpec(Guid mapEdgeId)
    {
        Query.Where(x => x.MapEdgeId == mapEdgeId && x.IsValid);
    }
}
