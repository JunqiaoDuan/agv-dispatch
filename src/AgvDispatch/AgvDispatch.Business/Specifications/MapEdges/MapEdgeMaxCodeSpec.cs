using AgvDispatch.Business.Entities.MapAggregate;
using AgvDispatch.Shared.Constants;
using Ardalis.Specification;

namespace AgvDispatch.Business.Specifications.MapEdges;

/// <summary>
/// 获取指定地图中E开头的最大编号的边
/// </summary>
public class MapEdgeMaxCodeSpec : Specification<MapEdge>, ISingleResultSpecification<MapEdge>
{
    public MapEdgeMaxCodeSpec(Guid mapId)
    {
        Query.Where(x => x.MapId == mapId && x.EdgeCode.StartsWith(EntityCodes.EdgePrefix))
            .OrderByDescending(x => x.EdgeCode);
    }
}
