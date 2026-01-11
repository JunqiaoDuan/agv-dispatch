using AgvDispatch.Business.Entities.RouteAggregate;
using Ardalis.Specification;

namespace AgvDispatch.Business.Specifications.RouteSegments;

/// <summary>
/// 获取指定路线的有效路线段列表
/// </summary>
public class RouteSegmentListSpec : Specification<RouteSegment>
{
    public RouteSegmentListSpec(Guid routeId)
    {
        Query.Where(x => x.RouteId == routeId && x.IsValid)
            .OrderBy(x => x.Seq);
    }
}
