using AgvDispatch.Business.Entities.MapAggregate;
using Ardalis.Specification;

namespace AgvDispatch.Business.Specifications.MapNodes;

/// <summary>
/// 根据地图ID查询节点列表
/// </summary>
public class MapNodesByMapIdSpec : Specification<MapNode>
{
    public MapNodesByMapIdSpec(Guid mapId)
    {
        Query.Where(x => x.MapId == mapId && x.IsValid);
    }
}
