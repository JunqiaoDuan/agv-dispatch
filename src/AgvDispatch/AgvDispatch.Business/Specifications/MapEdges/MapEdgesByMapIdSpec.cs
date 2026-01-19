using AgvDispatch.Business.Entities.MapAggregate;
using Ardalis.Specification;

namespace AgvDispatch.Business.Specifications.MapEdges;

/// <summary>
/// 根据地图ID查询边列表
/// </summary>
public class MapEdgesByMapIdSpec : Specification<MapEdge>
{
    public MapEdgesByMapIdSpec(Guid mapId)
    {
        Query.Where(x => x.MapId == mapId && x.IsValid);
    }
}
