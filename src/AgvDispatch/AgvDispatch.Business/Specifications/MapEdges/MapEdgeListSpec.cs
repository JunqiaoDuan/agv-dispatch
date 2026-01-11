using AgvDispatch.Business.Entities.MapAggregate;
using Ardalis.Specification;

namespace AgvDispatch.Business.Specifications.MapEdges;

/// <summary>
/// 获取指定地图的有效边列表
/// </summary>
public class MapEdgeListSpec : Specification<MapEdge>
{
    public MapEdgeListSpec(Guid mapId)
    {
        Query.Where(x => x.MapId == mapId && x.IsValid)
            .OrderBy(x => x.EdgeCode);
    }
}
