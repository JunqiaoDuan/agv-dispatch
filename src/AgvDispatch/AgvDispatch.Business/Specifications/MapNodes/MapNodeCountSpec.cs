using AgvDispatch.Business.Entities.MapAggregate;
using Ardalis.Specification;

namespace AgvDispatch.Business.Specifications.MapNodes;

/// <summary>
/// 获取指定地图的有效节点数量
/// </summary>
public class MapNodeCountSpec : Specification<MapNode>
{
    public MapNodeCountSpec(Guid mapId)
    {
        Query.Where(x => x.MapId == mapId && x.IsValid);
    }
}
