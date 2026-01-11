using AgvDispatch.Business.Entities.RouteAggregate;
using Ardalis.Specification;

namespace AgvDispatch.Business.Specifications.RouteSegments;

/// <summary>
/// 检查指定边是否被路线段引用
/// </summary>
public class RouteSegmentByEdgeIdSpec : Specification<RouteSegment>
{
    public RouteSegmentByEdgeIdSpec(Guid edgeId)
    {
        Query.Where(x => x.EdgeId == edgeId && x.IsValid);
    }
}
