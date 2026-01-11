using AgvDispatch.Business.Entities.MapAggregate;
using Ardalis.Specification;

namespace AgvDispatch.Business.Specifications.MapEdges;

/// <summary>
/// 获取指定地图的有效边数量
/// </summary>
public class MapEdgeCountSpec : Specification<MapEdge>
{
    public MapEdgeCountSpec(Guid mapId)
    {
        Query.Where(x => x.MapId == mapId && x.IsValid);
    }
}
