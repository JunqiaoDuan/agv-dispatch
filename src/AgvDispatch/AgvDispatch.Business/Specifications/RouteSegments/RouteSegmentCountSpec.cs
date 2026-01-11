using AgvDispatch.Business.Entities.RouteAggregate;
using Ardalis.Specification;

namespace AgvDispatch.Business.Specifications.RouteSegments;

/// <summary>
/// 获取指定路线的路线段数量
/// </summary>
public class RouteSegmentCountSpec : Specification<RouteSegment>
{
    public RouteSegmentCountSpec(Guid routeId)
    {
        Query.Where(x => x.RouteId == routeId && x.IsValid);
    }
}
