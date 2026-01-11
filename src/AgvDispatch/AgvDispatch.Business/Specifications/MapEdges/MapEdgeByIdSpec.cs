using AgvDispatch.Business.Entities.MapAggregate;
using Ardalis.Specification;

namespace AgvDispatch.Business.Specifications.MapEdges;

/// <summary>
/// 根据ID获取有效的边
/// </summary>
public class MapEdgeByIdSpec : Specification<MapEdge>, ISingleResultSpecification<MapEdge>
{
    public MapEdgeByIdSpec(Guid id)
    {
        Query.Where(x => x.Id == id && x.IsValid);
    }
}
