using AgvDispatch.Business.Entities.MapAggregate;
using Ardalis.Specification;

namespace AgvDispatch.Business.Specifications.MapEdges;

/// <summary>
/// 根据边ID列表查询边
/// </summary>
public class MapEdgesByIdsSpec : Specification<MapEdge>
{
    public MapEdgesByIdsSpec(List<Guid> edgeIds)
    {
        Query.Where(x => edgeIds.Contains(x.Id) && x.IsValid);
    }
}
