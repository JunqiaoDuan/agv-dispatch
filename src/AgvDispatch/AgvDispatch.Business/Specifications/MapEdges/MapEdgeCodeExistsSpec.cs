using AgvDispatch.Business.Entities.MapAggregate;
using Ardalis.Specification;

namespace AgvDispatch.Business.Specifications.MapEdges;

/// <summary>
/// 检查指定地图中边编号是否存在
/// </summary>
public class MapEdgeCodeExistsSpec : Specification<MapEdge>, ISingleResultSpecification<MapEdge>
{
    public MapEdgeCodeExistsSpec(Guid mapId, string edgeCode)
    {
        Query.Where(x => x.MapId == mapId && x.EdgeCode == edgeCode && x.IsValid);
    }
}
