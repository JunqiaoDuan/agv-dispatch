using AgvDispatch.Business.Entities.MapAggregate;
using Ardalis.Specification;

namespace AgvDispatch.Business.Specifications.MapNodes;

/// <summary>
/// 获取指定地图的有效节点列表
/// </summary>
public class MapNodeListSpec : Specification<MapNode>
{
    public MapNodeListSpec(Guid mapId)
    {
        Query.Where(x => x.MapId == mapId && x.IsValid)
            .OrderBy(x => x.SortNo)
            .ThenBy(x => x.NodeCode);
    }
}
